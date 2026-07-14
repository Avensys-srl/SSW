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

    Public Shared Function GetReportCompanionPath(pdfFilePath As String) As String
        If String.IsNullOrWhiteSpace(pdfFilePath) Then
            Throw New ArgumentException("The PDF report path is empty.", NameOf(pdfFilePath))
        End If
        Return Path.ChangeExtension(Path.GetFullPath(pdfFilePath), FileExtension)
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
            CreateMigrationBackupIfRequired(fullPath, document)
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

        document.SourceFormatVersion = document.SelectionFormatVersion
        document.RequiresMigrationBackup = False
        document.SourceFilePath = fullPath
    End Sub

    Public Shared Function Load(filePath As String) As CLSelectionProjectDocument
        If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
            Throw New FileNotFoundException("Selection project not found.", filePath)
        End If

        Dim fullPath As String = Path.GetFullPath(filePath)
        Dim json As String = File.ReadAllText(fullPath, Encoding.UTF8)
        Dim sourceFormatVersion As Integer = InspectEnvelope(json)
        Dim migrationResult As CLSelectionMigrationResult =
            CLSelectionMigrationRunner.Run(json, sourceFormatVersion)

        Dim document As CLSelectionProjectDocument =
            JsonSerializer.Deserialize(Of CLSelectionProjectDocument)(migrationResult.Json, SerializerOptions)
        Dim normalized As Boolean = Normalize(document)
        Validate(document)
        document.SourceFormatVersion = sourceFormatVersion
        document.RequiresMigrationBackup = migrationResult.WasMigrated OrElse normalized
        document.SourceFilePath = fullPath
        Return document
    End Function

    Private Shared Function InspectEnvelope(json As String) As Integer
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

                Dim requiredElement As JsonElement
                For Each requiredProperty As String In New String() {
                    "projectId", "createdAtUtc", "modifiedAtUtc", "versions", "selection"}
                    If Not root.TryGetProperty(requiredProperty, requiredElement) OrElse
                        requiredElement.ValueKind = JsonValueKind.Null OrElse
                        requiredElement.ValueKind = JsonValueKind.Undefined Then
                        Throw New InvalidDataException(String.Format(
                            "The selection project required field '{0}' is missing.",
                            requiredProperty))
                    End If
                Next

                If root.GetProperty("versions").ValueKind <> JsonValueKind.Object OrElse
                    root.GetProperty("selection").ValueKind <> JsonValueKind.Object Then
                    Throw New InvalidDataException("The selection project required technical blocks are invalid.")
                End If
                Return formatVersion
            End Using
        Catch ex As JsonException
            Throw New InvalidDataException("The selection project contains invalid JSON.", ex)
        End Try
    End Function

    Private Shared Sub CreateMigrationBackupIfRequired(fullPath As String, document As CLSelectionProjectDocument)
        If Not document.RequiresMigrationBackup OrElse Not File.Exists(fullPath) Then
            Return
        End If
        If Not String.IsNullOrWhiteSpace(document.SourceFilePath) AndAlso
            Not String.Equals(Path.GetFullPath(document.SourceFilePath), fullPath, StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        Dim backupPath As String = fullPath & String.Format(
            ".pre-migration-v{0}.bak",
            document.SourceFormatVersion)
        If Not File.Exists(backupPath) Then
            File.Copy(fullPath, backupPath, False)
        End If
    End Sub

    Private Shared Function Normalize(document As CLSelectionProjectDocument) As Boolean
        If document Is Nothing Then
            Return False
        End If

        Dim changed As Boolean
        If document.Features Is Nothing Then
            document.Features = New List(Of String)()
            changed = True
        End If
        If document.Identity Is Nothing Then
            document.Identity = New CLSelectionIdentity()
            changed = True
        End If
        If document.RevisionTracking Is Nothing Then
            document.RevisionTracking = New CLSelectionRevisionTracking()
            changed = True
        End If
        If document.Selection Is Nothing OrElse document.Versions Is Nothing Then
            Return changed
        End If
        If document.Versions.SelectionFormatVersion = 0 Then
            document.Versions.SelectionFormatVersion = document.SelectionFormatVersion
            changed = True
        End If
        If document.Selection.Unit Is Nothing Then
            document.Selection.Unit = New CLSelectionEntityReference()
            changed = True
        End If
        If document.Selection.Winter Is Nothing Then
            document.Selection.Winter = New CLOperatingScenarioInput With {.Enabled = True, .ScenarioCode = "Winter"}
            changed = True
        End If
        If document.Selection.Summer Is Nothing Then
            document.Selection.Summer = New CLOperatingScenarioInput With {.Enabled = False, .ScenarioCode = "Summer"}
            changed = True
        End If
        changed = NormalizeScenario(document.Selection.Winter, "Winter", True) OrElse changed
        changed = NormalizeScenario(document.Selection.Summer, "Summer", False) OrElse changed
        If document.Selection.WaterCoil Is Nothing Then
            document.Selection.WaterCoil = New CLWaterCoilSelection()
            changed = True
        End If
        If document.Selection.ElectricHeater Is Nothing Then
            document.Selection.ElectricHeater = New CLElectricHeaterSelection()
            changed = True
        End If
        If document.Selection.Report Is Nothing Then
            document.Selection.Report = New CLReportSelectionOptions()
            changed = True
        End If
        Return changed
    End Function

    Private Shared Function NormalizeScenario(
        scenario As CLOperatingScenarioInput,
        scenarioCode As String,
        enabledByDefault As Boolean) As Boolean

        Dim changed As Boolean
        If String.IsNullOrWhiteSpace(scenario.ScenarioCode) Then
            scenario.ScenarioCode = scenarioCode
            scenario.Enabled = enabledByDefault
            changed = True
        End If
        If Not scenario.ExtractAirflowM3h.HasValue AndAlso scenario.SupplyAirflowM3h.HasValue Then
            scenario.ExtractAirflowM3h = scenario.SupplyAirflowM3h
            changed = True
        End If
        Return changed
    End Function

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
        ValidateRevisionTracking(document.RevisionTracking)

    End Sub

    Private Shared Sub ValidateRevisionTracking(tracking As CLSelectionRevisionTracking)
        If tracking Is Nothing Then Return
        ValidateFingerprints(tracking.Current)
        If tracking.LastRegistered IsNot Nothing Then
            If tracking.LastRegistered.Revision < 1 OrElse
                String.IsNullOrWhiteSpace(tracking.LastRegistered.PublicReference) OrElse
                tracking.LastRegistered.Versions Is Nothing Then
                Throw New InvalidDataException("The registered selection revision metadata is invalid.")
            End If
            ValidateFingerprints(tracking.LastRegistered.Fingerprints)
        End If
    End Sub

    Private Shared Sub ValidateFingerprints(fingerprints As CLSelectionFingerprintSet)
        If fingerprints Is Nothing Then Return
        If fingerprints.SchemaVersion <> 1 OrElse
            Not CLSelectionSnapshotService.IsValidHash(fingerprints.TechnicalInputHash) OrElse
            Not CLSelectionSnapshotService.IsValidHash(fingerprints.CalculationOutputHash) OrElse
            Not CLSelectionSnapshotService.IsValidHash(fingerprints.CalculationBasisHash) OrElse
            Not CLSelectionSnapshotService.IsValidHash(fingerprints.SnapshotHash) Then
            Throw New InvalidDataException("The technical selection fingerprint metadata is invalid.")
        End If
    End Sub

End Class

Friend NotInheritable Class CLSelectionMigrationResult

    Public Property Json As String
    Public Property WasMigrated As Boolean

End Class

Friend NotInheritable Class CLSelectionMigrationRunner

    Private Sub New()
    End Sub

    Public Shared Function Run(json As String, sourceVersion As Integer) As CLSelectionMigrationResult
        Dim currentJson As String = json
        Dim currentVersion As Integer = sourceVersion

        While currentVersion < CLTechnicalVersions.CurrentSelectionFormatVersion
            Select Case currentVersion
                Case Else
                    Throw New NotSupportedException(String.Format(
                        "No migration is registered from selection format {0} to {1}.",
                        currentVersion,
                        currentVersion + 1))
            End Select
        End While

        Return New CLSelectionMigrationResult With {
            .Json = currentJson,
            .WasMigrated = currentVersion <> sourceVersion
        }
    End Function

End Class
