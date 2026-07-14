Imports System.IO
Imports System.Text
Imports System.Text.Json

Public NotInheritable Class CLSelectionProjectSerializer

    Public Const ProjectFormat As String = "SSWSelection"
    Public Const FileExtension As String = ".sswsel"

    Private Shared ReadOnly SerializerOptions As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        .PropertyNameCaseInsensitive = True,
        .WriteIndented = True,
        .IgnoreNullValues = True
    }

    Private Sub New()
    End Sub

    Public Shared Function CreateNew(databaseInfo As CLDatabaseCompatibilityInfo) As CLSelectionProjectDocument
        Dim document As New CLSelectionProjectDocument()
        document.Versions = CreateCurrentVersionSet(databaseInfo)
        Return document
    End Function

    Public Shared Function CreateCurrentVersionSet(databaseInfo As CLDatabaseCompatibilityInfo) As CLSelectionVersionSet
        Dim versions As New CLSelectionVersionSet With {
            .SoftwareVersion = CLTechnicalVersions.SoftwareVersion.ToString(),
            .CalculationEngineVersion = CLTechnicalVersions.CalculationEngineVersion.ToString(),
            .SelectionFormatVersion = CLTechnicalVersions.CurrentSelectionFormatVersion,
            .ReportTemplateVersion = CLTechnicalVersions.CurrentReportTemplateVersion,
            .ApiContractVersion = CLTechnicalVersions.CurrentApiContractVersion
        }

        If databaseInfo IsNot Nothing Then
            versions.DatabaseSchemaVersion = databaseInfo.SchemaVersion
            versions.DatabaseDataVersion = databaseInfo.DataVersion
            versions.DatabaseContentHash = databaseInfo.ContentHash
        End If

        Return versions
    End Function

    Public Shared Sub Save(filePath As String, document As CLSelectionProjectDocument)
        Validate(document)

        If String.IsNullOrWhiteSpace(filePath) Then
            Throw New ArgumentException("The selection project path is empty.", NameOf(filePath))
        End If

        Dim fullPath As String = Path.GetFullPath(filePath)
        Dim directoryPath As String = Path.GetDirectoryName(fullPath)
        If String.IsNullOrEmpty(directoryPath) OrElse Not Directory.Exists(directoryPath) Then
            Throw New DirectoryNotFoundException("The selection project directory does not exist.")
        End If

        document.ModifiedAtUtc = DateTime.UtcNow
        Dim json As String = JsonSerializer.Serialize(document, SerializerOptions)
        Dim temporaryPath As String = fullPath & ".tmp-" & Guid.NewGuid().ToString("N")
        Dim backupPath As String = temporaryPath & ".bak"

        Try
            File.WriteAllText(temporaryPath, json, New UTF8Encoding(False))

            If File.Exists(fullPath) Then
                File.Replace(temporaryPath, fullPath, backupPath, True)
                If File.Exists(backupPath) Then
                    File.Delete(backupPath)
                End If
            Else
                File.Move(temporaryPath, fullPath)
            End If
        Finally
            If File.Exists(temporaryPath) Then
                File.Delete(temporaryPath)
            End If
            If File.Exists(backupPath) Then
                File.Delete(backupPath)
            End If
        End Try
    End Sub

    Public Shared Function Load(filePath As String) As CLSelectionProjectDocument
        If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
            Throw New FileNotFoundException("Selection project not found.", filePath)
        End If

        Dim json As String = File.ReadAllText(filePath, Encoding.UTF8)
        InspectEnvelope(json)

        Dim document As CLSelectionProjectDocument =
            JsonSerializer.Deserialize(Of CLSelectionProjectDocument)(json, SerializerOptions)
        Validate(document)
        Return document
    End Function

    Private Shared Sub InspectEnvelope(json As String)
        Try
            Using jsonDocument As JsonDocument = JsonDocument.Parse(json)
                Dim root As JsonElement = jsonDocument.RootElement
                If root.ValueKind <> JsonValueKind.Object Then
                    Throw New InvalidDataException("The selection project root must be a JSON object.")
                End If

                Dim formatElement As JsonElement
                If Not root.TryGetProperty("format", formatElement) OrElse
                    Not String.Equals(formatElement.GetString(), ProjectFormat, StringComparison.Ordinal) Then
                    Throw New InvalidDataException("Unknown selection project format.")
                End If

                Dim versionElement As JsonElement
                If Not root.TryGetProperty("selectionFormatVersion", versionElement) OrElse
                    versionElement.ValueKind <> JsonValueKind.Number Then
                    Throw New InvalidDataException("Selection format version not found.")
                End If

                Dim formatVersion As Integer = versionElement.GetInt32()
                If formatVersion > CLTechnicalVersions.CurrentSelectionFormatVersion Then
                    Throw New NotSupportedException(String.Format(
                        "Selection format {0} requires a newer SSW (maximum supported: {1}).",
                        formatVersion,
                        CLTechnicalVersions.CurrentSelectionFormatVersion))
                End If
                If formatVersion < 1 Then
                    Throw New InvalidDataException("Selection format version is invalid.")
                End If
            End Using
        Catch ex As JsonException
            Throw New InvalidDataException("The selection project contains invalid JSON.", ex)
        End Try
    End Sub

    Private Shared Sub Validate(document As CLSelectionProjectDocument)
        If document Is Nothing Then
            Throw New InvalidDataException("The selection project is empty.")
        End If
        If Not String.Equals(document.Format, ProjectFormat, StringComparison.Ordinal) Then
            Throw New InvalidDataException("Unknown selection project format.")
        End If
        If document.SelectionFormatVersion <> CLTechnicalVersions.CurrentSelectionFormatVersion Then
            Throw New NotSupportedException("The selection project must be migrated before it can be used.")
        End If
        If document.ProjectId = Guid.Empty Then
            Throw New InvalidDataException("The selection project identifier is missing.")
        End If
        If document.Versions Is Nothing OrElse document.Selection Is Nothing Then
            Throw New InvalidDataException("The selection project does not contain the required technical blocks.")
        End If
        If document.Versions.SelectionFormatVersion <> document.SelectionFormatVersion Then
            Throw New InvalidDataException("The envelope and calculation version blocks are inconsistent.")
        End If

        If document.Features Is Nothing Then
            document.Features = New List(Of String)()
        End If
        If document.Identity Is Nothing Then
            document.Identity = New CLSelectionIdentity()
        End If
        If document.Selection.Unit Is Nothing Then
            document.Selection.Unit = New CLSelectionEntityReference()
        End If
        If document.Selection.Winter Is Nothing Then
            document.Selection.Winter = New CLOperatingScenarioInput With {.Enabled = True, .ScenarioCode = "Winter"}
        End If
        If document.Selection.Summer Is Nothing Then
            document.Selection.Summer = New CLOperatingScenarioInput With {.Enabled = False, .ScenarioCode = "Summer"}
        End If
        If document.Selection.WaterCoil Is Nothing Then
            document.Selection.WaterCoil = New CLWaterCoilSelection()
        End If
        If document.Selection.Report Is Nothing Then
            document.Selection.Report = New CLReportSelectionOptions()
        End If
    End Sub

End Class
