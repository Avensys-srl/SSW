Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Text
Imports System.Text.Json

Public NotInheritable Class CLMultiSelectionProjectSerializer
    Public Const ProjectFormat As String = "SSWMultiSelectionProject"
    Public Const CurrentFormatVersion As Integer = 1
    Public Const FileExtension As String = ".sswproj"
    Private Const ManifestEntryName As String = "project.json"

    Private Shared ReadOnly SerializerOptions As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        .PropertyNameCaseInsensitive = True,
        .WriteIndented = True,
        .IgnoreNullValues = True
    }

    Private Sub New()
    End Sub

    Public Shared Function CreateNew(reference As String, languageCode As String) As CLMultiSelectionProjectDocument
        Return New CLMultiSelectionProjectDocument With {
            .Reference = If(reference, String.Empty).Trim(),
            .LanguageCode = NormalizeLanguage(languageCode)
        }
    End Function

    Public Shared Sub AddOrUpdate(document As CLMultiSelectionProjectDocument,
        selection As CLSelectionProjectDocument,
        pdfPath As String,
        languageCode As String)

        If selection Is Nothing OrElse selection.Selection Is Nothing Then
            Throw New InvalidDataException("The technical selection is missing.")
        End If
        If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
            Throw New FileNotFoundException("The selection PDF was not found.", pdfPath)
        End If

        Validate(document)
        Dim normalizedLanguage As String = NormalizeLanguage(languageCode)
        If Not String.Equals(document.LanguageCode, normalizedLanguage, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException("The selection PDF language does not match the project language.")
        End If

        Dim item As CLMultiSelectionProjectItem = document.Items.FirstOrDefault(
            Function(value) value.SelectionProjectId = selection.ProjectId)
        If item Is Nothing Then
            item = New CLMultiSelectionProjectItem With {.SelectionProjectId = selection.ProjectId}
            document.Items.Add(item)
        End If

        Dim scenario As CLOperatingScenarioInput = selection.Selection.Winter
        item.CustomerReference = If(selection.Selection.CustomerReference, String.Empty).Trim()
        item.UnitName = If(selection.Selection.Unit?.Name, String.Empty).Trim()
        item.AirflowM3h = If(scenario Is Nothing, CType(Nothing, Double?), CType(scenario.SupplyAirflowM3h, Double?))
        item.PressurePa = If(scenario Is Nothing, CType(Nothing, Double?), CType(scenario.MaximumPressurePa, Double?))
        item.PdfFileName = Path.GetFileName(pdfPath)
        item.LanguageCode = normalizedLanguage
        item.SnapshotHash = GetSnapshotHash(selection)
        item.SelectionJson = CLSelectionProjectSerializer.Serialize(selection)
        item.PdfBytes = File.ReadAllBytes(pdfPath)
        item.ModifiedAtUtc = DateTime.UtcNow
        document.ModifiedAtUtc = DateTime.UtcNow
    End Sub

    Public Shared Function IsCurrent(item As CLMultiSelectionProjectItem,
        projectLanguage As String) As Boolean

        Return item IsNot Nothing AndAlso item.HasValidPdf AndAlso
            Not String.IsNullOrWhiteSpace(item.SelectionJson) AndAlso
            String.Equals(NormalizeLanguage(item.LanguageCode), NormalizeLanguage(projectLanguage),
                StringComparison.OrdinalIgnoreCase)
    End Function

    Public Shared Sub Save(filePath As String, document As CLMultiSelectionProjectDocument)
        Validate(document)
        Dim fullPath As String = Path.GetFullPath(filePath)
        Dim directoryPath As String = Path.GetDirectoryName(fullPath)
        If String.IsNullOrWhiteSpace(directoryPath) OrElse Not Directory.Exists(directoryPath) Then
            Throw New DirectoryNotFoundException("The multi-selection project directory does not exist.")
        End If

        document.ModifiedAtUtc = DateTime.UtcNow
        Dim temporaryPath As String = fullPath & ".tmp-" & Guid.NewGuid().ToString("N")
        Dim backupPath As String = temporaryPath & ".bak"
        Try
            Using archive As ZipArchive = ZipFile.Open(temporaryPath, ZipArchiveMode.Create)
                WriteTextEntry(archive, ManifestEntryName,
                    JsonSerializer.Serialize(document, SerializerOptions))
                For Each item As CLMultiSelectionProjectItem In document.Items
                    WriteTextEntry(archive, SelectionEntryName(item), item.SelectionJson)
                    WriteBytesEntry(archive, PdfEntryName(item), item.PdfBytes)
                Next
            End Using

            If File.Exists(fullPath) Then
                File.Replace(temporaryPath, fullPath, backupPath, True)
                If File.Exists(backupPath) Then File.Delete(backupPath)
            Else
                File.Move(temporaryPath, fullPath)
            End If
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
            If File.Exists(backupPath) Then File.Delete(backupPath)
        End Try
        document.SourceFilePath = fullPath
    End Sub

    Public Shared Function Load(filePath As String) As CLMultiSelectionProjectDocument
        Dim fullPath As String = Path.GetFullPath(filePath)
        If Not File.Exists(fullPath) Then Throw New FileNotFoundException("Project not found.", fullPath)

        Using archive As ZipArchive = ZipFile.OpenRead(fullPath)
            Dim manifest As ZipArchiveEntry = archive.GetEntry(ManifestEntryName)
            If manifest Is Nothing Then Throw New InvalidDataException("The project manifest is missing.")
            Dim document As CLMultiSelectionProjectDocument = JsonSerializer.Deserialize(Of CLMultiSelectionProjectDocument)(
                ReadTextEntry(manifest), SerializerOptions)
            Validate(document, False)
            For Each item As CLMultiSelectionProjectItem In document.Items
                Dim selectionEntry As ZipArchiveEntry = archive.GetEntry(SelectionEntryName(item))
                Dim pdfEntry As ZipArchiveEntry = archive.GetEntry(PdfEntryName(item))
                If selectionEntry Is Nothing OrElse pdfEntry Is Nothing Then
                    Throw New InvalidDataException("A project selection or PDF entry is missing.")
                End If
                item.SelectionJson = ReadTextEntry(selectionEntry)
                item.PdfBytes = ReadBytesEntry(pdfEntry)
                CLSelectionProjectSerializer.Deserialize(item.SelectionJson)
            Next
            Validate(document)
            document.SourceFilePath = fullPath
            Return document
        End Using
    End Function

    Public Shared Function ExtractPdf(item As CLMultiSelectionProjectItem, directoryPath As String) As String
        If item Is Nothing OrElse Not item.HasValidPdf Then Throw New InvalidDataException("The project PDF is unavailable.")
        Dim itemDirectory As String = Path.Combine(directoryPath, item.ItemId.ToString("N"))
        Directory.CreateDirectory(itemDirectory)
        Dim targetPath As String = Path.Combine(itemDirectory, SafeFileName(item.PdfFileName))
        File.WriteAllBytes(targetPath, item.PdfBytes)
        Return targetPath
    End Function

    Private Shared Sub Validate(document As CLMultiSelectionProjectDocument, Optional requirePayload As Boolean = True)
        If document Is Nothing OrElse Not String.Equals(document.Format, ProjectFormat, StringComparison.Ordinal) Then
            Throw New InvalidDataException("Unknown multi-selection project format.")
        End If
        If document.FormatVersion <> CurrentFormatVersion Then Throw New NotSupportedException("Unsupported project format version.")
        If document.ProjectId = Guid.Empty OrElse String.IsNullOrWhiteSpace(document.Reference) Then
            Throw New InvalidDataException("The project reference is required.")
        End If
        document.LanguageCode = NormalizeLanguage(document.LanguageCode)
        If document.Items Is Nothing Then document.Items = New List(Of CLMultiSelectionProjectItem)()
        Dim ids As New HashSet(Of Guid)()
        For Each item As CLMultiSelectionProjectItem In document.Items
            If item Is Nothing OrElse item.ItemId = Guid.Empty OrElse item.SelectionProjectId = Guid.Empty OrElse
                Not ids.Add(item.SelectionProjectId) Then
                Throw New InvalidDataException("The project contains an invalid or duplicate selection.")
            End If
            If requirePayload AndAlso (Not IsCurrent(item, document.LanguageCode)) Then
                Throw New InvalidDataException("The project contains an incomplete or mismatched selection PDF.")
            End If
        Next
    End Sub

    Private Shared Function GetSnapshotHash(selection As CLSelectionProjectDocument) As String
        If selection.RevisionTracking?.Current IsNot Nothing AndAlso
            Not String.IsNullOrWhiteSpace(selection.RevisionTracking.Current.SnapshotHash) Then
            Return selection.RevisionTracking.Current.SnapshotHash
        End If
        Return If(selection.Snapshot Is Nothing, String.Empty,
            CLSelectionSnapshotService.Refresh(selection).SnapshotHash)
    End Function

    Private Shared Function NormalizeLanguage(value As String) As String
        Dim result As String = If(value, String.Empty).Trim().ToLowerInvariant()
        If result.Length = 0 Then result = "en"
        Return result
    End Function

    Private Shared Function SelectionEntryName(item As CLMultiSelectionProjectItem) As String
        Return "selections/" & item.ItemId.ToString("N") & CLSelectionProjectSerializer.FileExtension
    End Function

    Private Shared Function PdfEntryName(item As CLMultiSelectionProjectItem) As String
        Return "reports/" & item.ItemId.ToString("N") & "/" & SafeFileName(item.PdfFileName)
    End Function

    Private Shared Function SafeFileName(value As String) As String
        Dim result As String = Path.GetFileName(If(value, String.Empty).Trim())
        For Each invalidCharacter As Char In Path.GetInvalidFileNameChars()
            result = result.Replace(invalidCharacter, "_"c)
        Next
        Return If(String.IsNullOrWhiteSpace(result), "Report.pdf", result)
    End Function

    Private Shared Sub WriteTextEntry(archive As ZipArchive, name As String, value As String)
        Dim entry As ZipArchiveEntry = archive.CreateEntry(name, CompressionLevel.Optimal)
        Using writer As New StreamWriter(entry.Open(), New UTF8Encoding(False))
            writer.Write(If(value, String.Empty))
        End Using
    End Sub

    Private Shared Sub WriteBytesEntry(archive As ZipArchive, name As String, value As Byte())
        Dim entry As ZipArchiveEntry = archive.CreateEntry(name, CompressionLevel.Optimal)
        Using stream As Stream = entry.Open()
            stream.Write(value, 0, value.Length)
        End Using
    End Sub

    Private Shared Function ReadTextEntry(entry As ZipArchiveEntry) As String
        Using reader As New StreamReader(entry.Open(), Encoding.UTF8)
            Return reader.ReadToEnd()
        End Using
    End Function

    Private Shared Function ReadBytesEntry(entry As ZipArchiveEntry) As Byte()
        Using input As Stream = entry.Open(), output As New MemoryStream()
            input.CopyTo(output)
            Return output.ToArray()
        End Using
    End Function
End Class
