Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json

Public Enum CLSelectionChangeKind
    NewSelection = 0
    Reprint = 1
    TechnicalChange = 2
    DatabaseChange = 3
    AlgorithmChange = 4
    CalculationResultChange = 5
End Enum

Public NotInheritable Class CLSelectionSnapshotService

    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        .PropertyNameCaseInsensitive = True,
        .IgnoreNullValues = True
    }

    Private Sub New()
    End Sub

    Public Shared Function Refresh(document As CLSelectionProjectDocument) As CLSelectionFingerprintSet
        If document Is Nothing OrElse document.Selection Is Nothing OrElse
            document.Snapshot Is Nothing OrElse document.Versions Is Nothing Then
            Throw New InvalidDataException("A calculated technical selection is required to create a snapshot fingerprint.")
        End If

        Dim technicalInputHash As String = HashCanonical(
            document.Selection,
            New HashSet(Of String)(New String() {"customerCode", "customerReference"}, StringComparer.Ordinal))
        Dim outputHash As String = HashCanonical(
            document.Snapshot,
            New HashSet(Of String)(New String() {"calculatedAtUtc", "versions"}, StringComparer.Ordinal))
        Dim basisHash As String = HashCanonical(New CLCalculationBasisFingerprintInput With {
            .TechnicalInputHash = technicalInputHash,
            .CalculationEngineVersion = document.Versions.CalculationEngineVersion,
            .ReportTemplateVersion = document.Versions.ReportTemplateVersion,
            .DatabaseSchemaVersion = document.Versions.DatabaseSchemaVersion,
            .DatabaseDataVersion = document.Versions.DatabaseDataVersion,
            .DatabaseContentHash = document.Versions.DatabaseContentHash
        }, Nothing)
        Dim snapshotHash As String = HashCanonical(New CLCompleteSnapshotFingerprintInput With {
            .CalculationBasisHash = basisHash,
            .CalculationOutputHash = outputHash
        }, Nothing)

        Dim result As New CLSelectionFingerprintSet With {
            .TechnicalInputHash = technicalInputHash,
            .CalculationOutputHash = outputHash,
            .CalculationBasisHash = basisHash,
            .SnapshotHash = snapshotHash
        }
        If document.RevisionTracking Is Nothing Then document.RevisionTracking = New CLSelectionRevisionTracking()
        document.RevisionTracking.Current = result
        document.RevisionTracking.PendingChangeKind = Evaluate(document).ToString()
        Return result
    End Function

    Public Shared Function Evaluate(document As CLSelectionProjectDocument) As CLSelectionChangeKind
        If document Is Nothing OrElse document.RevisionTracking Is Nothing OrElse
            document.RevisionTracking.LastRegistered Is Nothing Then
            Return CLSelectionChangeKind.NewSelection
        End If
        Dim current As CLSelectionFingerprintSet = document.RevisionTracking.Current
        Dim baseline As CLRegisteredSelectionRevision = document.RevisionTracking.LastRegistered
        If current Is Nothing OrElse baseline.Fingerprints Is Nothing OrElse baseline.Versions Is Nothing Then
            Return CLSelectionChangeKind.NewSelection
        End If
        If Not SameText(document.Versions.CalculationEngineVersion, baseline.Versions.CalculationEngineVersion) Then
            Return CLSelectionChangeKind.AlgorithmChange
        End If
        If document.Versions.DatabaseSchemaVersion <> baseline.Versions.DatabaseSchemaVersion OrElse
            Not SameText(document.Versions.DatabaseDataVersion, baseline.Versions.DatabaseDataVersion) OrElse
            Not SameText(document.Versions.DatabaseContentHash, baseline.Versions.DatabaseContentHash) Then
            Return CLSelectionChangeKind.DatabaseChange
        End If
        If document.Versions.ReportTemplateVersion <> baseline.Versions.ReportTemplateVersion Then
            Return CLSelectionChangeKind.TechnicalChange
        End If
        If Not SameText(current.TechnicalInputHash, baseline.Fingerprints.TechnicalInputHash) Then
            Return CLSelectionChangeKind.TechnicalChange
        End If
        If Not SameText(current.CalculationOutputHash, baseline.Fingerprints.CalculationOutputHash) Then
            Return CLSelectionChangeKind.CalculationResultChange
        End If
        Return CLSelectionChangeKind.Reprint
    End Function

    Public Shared Sub MarkRegistered(document As CLSelectionProjectDocument,
        publicReference As String,
        revision As Integer,
        resumeToken As String,
        registeredAtUtc As DateTime)

        If String.IsNullOrWhiteSpace(publicReference) Then Throw New ArgumentException("Public reference is required.", NameOf(publicReference))
        If revision < 1 Then Throw New ArgumentOutOfRangeException(NameOf(revision))
        Dim current As CLSelectionFingerprintSet = Refresh(document)
        If registeredAtUtc.Kind <> DateTimeKind.Utc Then registeredAtUtc = registeredAtUtc.ToUniversalTime()
        document.Identity.PublicReference = publicReference
        document.Identity.Revision = revision
        document.Identity.ResumeToken = resumeToken
        document.RevisionTracking.LastRegistered = New CLRegisteredSelectionRevision With {
            .PublicReference = publicReference,
            .Revision = revision,
            .ResumeToken = resumeToken,
            .RegisteredAtUtc = registeredAtUtc,
            .Fingerprints = CloneFingerprints(current),
            .Versions = CloneVersions(document.Versions)
        }
        document.RevisionTracking.PendingChangeKind = CLSelectionChangeKind.Reprint.ToString()
    End Sub

    Public Shared Function IsValidHash(value As String) As Boolean
        If String.IsNullOrWhiteSpace(value) OrElse value.Length <> 64 Then Return False
        Return value.All(Function(character) (character >= "0"c AndAlso character <= "9"c) OrElse
            (character >= "a"c AndAlso character <= "f"c))
    End Function

    Private Shared Function HashCanonical(value As Object, excludedProperties As HashSet(Of String)) As String
        Dim serialized As Byte() = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions)
        Using document As JsonDocument = JsonDocument.Parse(serialized)
            Dim canonical As New StringBuilder()
            WriteCanonical(document.RootElement, canonical, excludedProperties)
            Using algorithm As SHA256 = SHA256.Create()
                Return String.Concat(algorithm.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())).
                    Select(Function(item) item.ToString("x2")))
            End Using
        End Using
    End Function

    Private Shared Sub WriteCanonical(element As JsonElement,
        output As StringBuilder,
        excludedProperties As HashSet(Of String))

        Select Case element.ValueKind
            Case JsonValueKind.Object
                output.Append("{"c)
                Dim first As Boolean = True
                For Each propertyValue As JsonProperty In element.EnumerateObject().
                    Where(Function(item) excludedProperties Is Nothing OrElse Not excludedProperties.Contains(item.Name)).
                    OrderBy(Function(item) item.Name, StringComparer.Ordinal)
                    If Not first Then output.Append(","c)
                    first = False
                    output.Append(JsonSerializer.Serialize(propertyValue.Name))
                    output.Append(":"c)
                    WriteCanonical(propertyValue.Value, output, excludedProperties)
                Next
                output.Append("}"c)
            Case JsonValueKind.Array
                output.Append("["c)
                Dim first As Boolean = True
                For Each item As JsonElement In element.EnumerateArray()
                    If Not first Then output.Append(","c)
                    first = False
                    WriteCanonical(item, output, excludedProperties)
                Next
                output.Append("]"c)
            Case JsonValueKind.String
                output.Append(JsonSerializer.Serialize(element.GetString()))
            Case JsonValueKind.Number
                Dim decimalValue As Decimal
                If element.TryGetDecimal(decimalValue) Then
                    output.Append(decimalValue.ToString("G29", Globalization.CultureInfo.InvariantCulture))
                Else
                    output.Append(element.GetDouble().ToString("R", Globalization.CultureInfo.InvariantCulture))
                End If
            Case JsonValueKind.True
                output.Append("true")
            Case JsonValueKind.False
                output.Append("false")
            Case JsonValueKind.Null, JsonValueKind.Undefined
                output.Append("null")
            Case Else
                Throw New InvalidDataException("Unsupported value in canonical technical selection JSON.")
        End Select
    End Sub

    Private Shared Function CloneFingerprints(value As CLSelectionFingerprintSet) As CLSelectionFingerprintSet
        Return New CLSelectionFingerprintSet With {
            .SchemaVersion = value.SchemaVersion,
            .TechnicalInputHash = value.TechnicalInputHash,
            .CalculationOutputHash = value.CalculationOutputHash,
            .CalculationBasisHash = value.CalculationBasisHash,
            .SnapshotHash = value.SnapshotHash
        }
    End Function

    Private Shared Function CloneVersions(value As CLSelectionVersionSet) As CLSelectionVersionSet
        Return New CLSelectionVersionSet With {
            .SoftwareVersion = value.SoftwareVersion,
            .CalculationEngineVersion = value.CalculationEngineVersion,
            .DatabaseSchemaVersion = value.DatabaseSchemaVersion,
            .DatabaseDataVersion = value.DatabaseDataVersion,
            .DatabaseContentHash = value.DatabaseContentHash,
            .SelectionFormatVersion = value.SelectionFormatVersion,
            .ReportTemplateVersion = value.ReportTemplateVersion,
            .ApiContractVersion = value.ApiContractVersion
        }
    End Function

    Private Shared Function SameText(left As String, right As String) As Boolean
        Return String.Equals(If(left, String.Empty), If(right, String.Empty), StringComparison.Ordinal)
    End Function

    Private NotInheritable Class CLCalculationBasisFingerprintInput
        Public Property TechnicalInputHash As String
        Public Property CalculationEngineVersion As String
        Public Property ReportTemplateVersion As Integer
        Public Property DatabaseSchemaVersion As Integer
        Public Property DatabaseDataVersion As String
        Public Property DatabaseContentHash As String
    End Class

    Private NotInheritable Class CLCompleteSnapshotFingerprintInput
        Public Property CalculationBasisHash As String
        Public Property CalculationOutputHash As String
    End Class

End Class
