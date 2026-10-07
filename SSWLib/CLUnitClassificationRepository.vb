Imports System.Data.SqlServerCe
Imports System.IO

Public NotInheritable Class CLUnitClassification
    Public Property RecoveryOperation As String
    Public Property ExchangerType As String
    Public Property UnitApplication As String
    Public Property InstallationEnvironment As String
    Public Property OutdoorIncreaseA As Integer
    Public Property OutdoorIncreaseB As Integer
    Public Property OutdoorIncreaseC As Integer
End Class

Public NotInheritable Class CLUnitClassificationRepository
    Private Shared ReadOnly Gate As New Object()
    Private Shared CacheKey As String
    Private Shared Cache As New Dictionary(Of Integer, CLUnitClassification)()

    Public Shared Function Find(modelId As Integer) As CLUnitClassification
        Dim path = CLEnvironment.Current.DCLiteDatabasePath
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then Return Nothing
        Dim fileInfo As New FileInfo(path)
        Dim key = path & "|" & fileInfo.LastWriteTimeUtc.Ticks & "|" & fileInfo.Length
        SyncLock Gate
            If key <> CacheKey Then
                Dim loaded As New Dictionary(Of Integer, CLUnitClassification)()
                Using connection As New SqlCeConnection("Data Source=""" & path & """;Password=@D3C1L4T2%")
                    connection.Open()
                    Using command = connection.CreateCommand()
                        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='CLHeatRecoveryModelClassifications'"
                        If Convert.ToInt32(command.ExecuteScalar()) > 0 Then
                            command.CommandText = "SELECT IdHeatRecoveryModel,RecoveryOperation,ExchangerType,UnitApplication,InstallationEnvironment,OutdoorIncreaseA,OutdoorIncreaseB,OutdoorIncreaseC FROM CLHeatRecoveryModelClassifications"
                            Using reader = command.ExecuteReader()
                                While reader.Read()
                                    loaded.Add(reader.GetInt32(0), New CLUnitClassification With {
                                        .RecoveryOperation = reader.GetString(1), .ExchangerType = reader.GetString(2),
                                        .UnitApplication = reader.GetString(3), .InstallationEnvironment = reader.GetString(4),
                                        .OutdoorIncreaseA = WholeMillimeters(reader.GetValue(5)),
                                        .OutdoorIncreaseB = WholeMillimeters(reader.GetValue(6)),
                                        .OutdoorIncreaseC = WholeMillimeters(reader.GetValue(7))})
                                End While
                            End Using
                        End If
                    End Using
                End Using
                Cache = loaded : CacheKey = key
            End If
            Dim result As CLUnitClassification = Nothing
            Cache.TryGetValue(modelId, result)
            Return result
        End SyncLock
    End Function

    Private Shared Function WholeMillimeters(value As Object) As Integer
        Dim number = Convert.ToDecimal(value)
        If number < 0 OrElse number > 99999999D OrElse Decimal.Truncate(number) <> number Then
            Throw New InvalidDataException("Outdoor dimension increases must be whole nonnegative millimeters.")
        End If
        Return Convert.ToInt32(number)
    End Function

    Public Shared Function Matches(modelId As Integer, filters As CLNextUiPreselectionFilters) As Boolean
        Dim row = Find(modelId)
        Dim operation = filters.RecoveryOperation
        Dim application = filters.UnitApplication
        If String.IsNullOrWhiteSpace(operation) OrElse operation = "any" Then
            If filters.RotaryOnlyEnabled OrElse filters.RecoveryCategory = "rotary" Then operation = "Rotary"
            If filters.RecoveryCategory = "plate" Then operation = "Plate"
        End If
        If String.IsNullOrWhiteSpace(application) OrElse application = "any" Then
            If filters.RecoveryCategory = "centralized" Then application = "Centralized"
            If filters.RecoveryCategory = "decentralized" Then application = "Decentralized"
        End If
        Return MatchValue(row?.RecoveryOperation, operation) AndAlso
            MatchValue(row?.ExchangerType, filters.ExchangerType) AndAlso
            MatchValue(row?.UnitApplication, application) AndAlso
            MatchEnvironment(row?.InstallationEnvironment, filters.InstallationEnvironment)
    End Function

    Public Shared Function EffectiveEnvironment(modelId As Integer, requested As String) As String
        Dim row = Find(modelId)
        If row Is Nothing Then
            If String.Equals(requested, "Outdoor", StringComparison.OrdinalIgnoreCase) Then Throw New InvalidOperationException("Outdoor classification is not available for this model.")
            Return "Indoor"
        End If
        If row.InstallationEnvironment = "Outdoor" Then Return "Outdoor"
        If String.Equals(requested, "Outdoor", StringComparison.OrdinalIgnoreCase) Then
            If row.InstallationEnvironment <> "Both" Then Throw New InvalidOperationException("This model is available for indoor installation only.")
            Return "Outdoor"
        End If
        Return "Indoor"
    End Function

    Public Shared Function RequiresOutdoorKit(modelId As Integer, environment As String) As Boolean
        Return Find(modelId)?.InstallationEnvironment = "Both" AndAlso String.Equals(environment, "Outdoor", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Shared Sub ApplyDimensions(modelId As Integer, environment As String, dimensions As IEnumerable(Of CLDimensionalValue))
        ApplyDimensions(Find(modelId), environment, dimensions)
    End Sub

    Public Shared Sub ApplyDimensions(row As CLUnitClassification, environment As String, dimensions As IEnumerable(Of CLDimensionalValue))
        If row Is Nothing OrElse row.InstallationEnvironment <> "Both" OrElse environment <> "Outdoor" Then Return
        For Each dimension In dimensions
            If Not dimension.ValueMillimeters.HasValue Then Continue For
            Select Case dimension.Code.ToUpperInvariant()
                Case "A", "W" : dimension.ValueMillimeters += row.OutdoorIncreaseA
                Case "B", "L" : dimension.ValueMillimeters += row.OutdoorIncreaseB
                Case "C", "H" : dimension.ValueMillimeters += row.OutdoorIncreaseC
            End Select
        Next
    End Sub

    Private Shared Function MatchValue(actual As String, requested As String) As Boolean
        Return String.IsNullOrWhiteSpace(requested) OrElse requested = "any" OrElse
            String.Equals(actual, requested, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function MatchEnvironment(actual As String, requested As String) As Boolean
        Return MatchValue(actual, requested) OrElse
            ((String.Equals(requested, "Indoor", StringComparison.OrdinalIgnoreCase) OrElse
              String.Equals(requested, "Outdoor", StringComparison.OrdinalIgnoreCase)) AndAlso
             String.Equals(actual, "Both", StringComparison.OrdinalIgnoreCase))
    End Function
End Class
