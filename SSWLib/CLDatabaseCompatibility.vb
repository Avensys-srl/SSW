Imports System.Data.SqlServerCe
Imports System.Globalization
Imports System.IO

Public Enum CLDatabaseCompatibilityState
    Legacy = 0
    Managed = 1
End Enum

Public NotInheritable Class CLDatabaseFeatureInfo

    Public Property Code As String
    Public Property Version As Integer
    Public Property IsEnabled As Boolean

End Class

Public NotInheritable Class CLDatabaseCompatibilityInfo

    Private ReadOnly m_Features As Dictionary(Of String, CLDatabaseFeatureInfo)

    Public Sub New()
        m_Features = New Dictionary(Of String, CLDatabaseFeatureInfo)(StringComparer.OrdinalIgnoreCase)
    End Sub

    Public Property State As CLDatabaseCompatibilityState
    Public Property SchemaVersion As Integer
    Public Property DataVersion As String
    Public Property ExporterVersion As String
    Public Property ExportedAtUtc As DateTime?
    Public Property CustomerCode As String
    Public Property MinimumSSWVersion As Version
    Public Property ContentHash As String

    Public ReadOnly Property Features As IEnumerable(Of CLDatabaseFeatureInfo)
        Get
            Return m_Features.Values
        End Get
    End Property

    Friend Sub AddFeature(feature As CLDatabaseFeatureInfo)
        m_Features(feature.Code) = feature
    End Sub

    Public Function HasFeature(code As String, Optional minimumVersion As Integer = 1) As Boolean
        Dim feature As CLDatabaseFeatureInfo = Nothing
        Return m_Features.TryGetValue(code, feature) AndAlso
            feature.IsEnabled AndAlso
            feature.Version >= minimumVersion
    End Function

    Public Function ToDisplayString() As String
        If State = CLDatabaseCompatibilityState.Legacy Then
            Return "SDF: Legacy-0"
        End If

        Return String.Format(CultureInfo.InvariantCulture,
            "SDF: schema {0} | data {1} | exporter {2}",
            SchemaVersion,
            DataVersion,
            ExporterVersion)
    End Function

End Class

Public NotInheritable Class CLDatabaseCompatibilityReader

    Private Const DatabasePassword As String = "@D3C1L4T2%"

    Private Sub New()
    End Sub

    Public Shared Function Inspect(databasePath As String,
        currentSoftwareVersion As Version,
        expectedCustomerCode As String) As CLDatabaseCompatibilityInfo

        If String.IsNullOrWhiteSpace(databasePath) OrElse Not File.Exists(databasePath) Then
            Throw New InvalidDataException("Fatal Error: DataCentral not found.")
        End If

        Try
            Using connection As New SqlCeConnection(
                String.Format("Data Source=""{0}""; Password=""{1}""", databasePath, DatabasePassword))

                connection.Open()

                If Not TableExists(connection, "CLDatabaseMetadata") Then
                    Return ReadLegacyInfo(connection)
                End If

                Return ReadManagedInfo(connection, currentSoftwareVersion, expectedCustomerCode)
            End Using
        Catch ex As InvalidDataException
            Throw
        Catch ex As Exception
            Throw New InvalidDataException("Fatal Error: DataCentral is unreadable or corrupted.", ex)
        End Try
    End Function

    Private Shared Function ReadLegacyInfo(connection As SqlCeConnection) As CLDatabaseCompatibilityInfo
        Dim result As New CLDatabaseCompatibilityInfo With {
            .State = CLDatabaseCompatibilityState.Legacy,
            .SchemaVersion = 0,
            .DataVersion = "Legacy-0",
            .ExporterVersion = "Legacy",
            .ContentHash = ""
        }

        result.AddFeature(New CLDatabaseFeatureInfo With {
            .Code = "CoreData",
            .Version = 1,
            .IsEnabled = True
        })

        Dim hasWaterCoils As Boolean = TableExists(connection, "CLCoils") AndAlso
            TableExists(connection, "CLHeatRecoveryModelCoils")
        result.AddFeature(New CLDatabaseFeatureInfo With {
            .Code = "WaterCoils",
            .Version = 1,
            .IsEnabled = hasWaterCoils
        })
        result.AddFeature(New CLDatabaseFeatureInfo With {
            .Code = "CoilInstallationType",
            .Version = 1,
            .IsEnabled = hasWaterCoils AndAlso
                TableColumnExists(connection, "CLHeatRecoveryModelCoils", "InstallationType")
        })

        result.AddFeature(New CLDatabaseFeatureInfo With {
            .Code = "ElectricHeaters",
            .Version = 1,
            .IsEnabled = TableExists(connection, "CLElectricHeaters") AndAlso
                TableExists(connection, "CLHeatRecoveryModelElectricHeaters")
        })

        Return result
    End Function

    Private Shared Function ReadManagedInfo(connection As SqlCeConnection,
        currentSoftwareVersion As Version,
        expectedCustomerCode As String) As CLDatabaseCompatibilityInfo

        If Not TableExists(connection, "CLDatabaseFeatures") Then
            Throw New InvalidDataException("Fatal Error: DataCentral manifest is incomplete (features not found).")
        End If

        Dim result As CLDatabaseCompatibilityInfo = Nothing
        Using command As SqlCeCommand = connection.CreateCommand()
            command.CommandText = "SELECT SchemaVersion, DataVersion, ExporterVersion, ExportedAtUtc, " &
                "CustomerCode, MinimumSSWVersion, ContentHash FROM CLDatabaseMetadata"

            Using reader As SqlCeDataReader = command.ExecuteReader()
                If Not reader.Read() Then
                    Throw New InvalidDataException("Fatal Error: DataCentral manifest is empty.")
                End If

                Dim minimumVersionText As String = Convert.ToString(reader("MinimumSSWVersion"), CultureInfo.InvariantCulture)
                Dim minimumVersion As Version = Nothing
                If Not Version.TryParse(minimumVersionText, minimumVersion) Then
                    Throw New InvalidDataException("Fatal Error: DataCentral minimum SSW version is invalid.")
                End If

                result = New CLDatabaseCompatibilityInfo With {
                    .State = CLDatabaseCompatibilityState.Managed,
                    .SchemaVersion = Convert.ToInt32(reader("SchemaVersion"), CultureInfo.InvariantCulture),
                    .DataVersion = Convert.ToString(reader("DataVersion"), CultureInfo.InvariantCulture),
                    .ExporterVersion = Convert.ToString(reader("ExporterVersion"), CultureInfo.InvariantCulture),
                    .ExportedAtUtc = Convert.ToDateTime(reader("ExportedAtUtc"), CultureInfo.InvariantCulture),
                    .CustomerCode = Convert.ToString(reader("CustomerCode"), CultureInfo.InvariantCulture),
                    .MinimumSSWVersion = minimumVersion,
                    .ContentHash = Convert.ToString(reader("ContentHash"), CultureInfo.InvariantCulture)
                }

                If reader.Read() Then
                    Throw New InvalidDataException("Fatal Error: DataCentral manifest contains multiple metadata rows.")
                End If
            End Using
        End Using

        ValidateManagedInfo(result, currentSoftwareVersion, expectedCustomerCode)

        Using command As SqlCeCommand = connection.CreateCommand()
            command.CommandText = "SELECT FeatureCode, FeatureVersion, IsEnabled FROM CLDatabaseFeatures"
            Using reader As SqlCeDataReader = command.ExecuteReader()
                While reader.Read()
                    result.AddFeature(New CLDatabaseFeatureInfo With {
                        .Code = Convert.ToString(reader("FeatureCode"), CultureInfo.InvariantCulture),
                        .Version = Convert.ToInt32(reader("FeatureVersion"), CultureInfo.InvariantCulture),
                        .IsEnabled = Convert.ToBoolean(reader("IsEnabled"), CultureInfo.InvariantCulture)
                    })
                End While
            End Using
        End Using

        If Not result.HasFeature("CoreData") Then
            Throw New InvalidDataException("Fatal Error: DataCentral core data feature is missing or disabled.")
        End If

        Return result
    End Function

    Private Shared Sub ValidateManagedInfo(info As CLDatabaseCompatibilityInfo,
        currentSoftwareVersion As Version,
        expectedCustomerCode As String)

        If info.SchemaVersion < 1 Then
            Throw New InvalidDataException("Fatal Error: DataCentral managed schema is too old.")
        End If
        If info.SchemaVersion > CLTechnicalVersions.CurrentDatabaseSchemaVersion Then
            Throw New InvalidDataException(String.Format(CultureInfo.InvariantCulture,
                "Fatal Error: DataCentral schema {0} requires a newer SSW (maximum supported: {1}).",
                info.SchemaVersion,
                CLTechnicalVersions.CurrentDatabaseSchemaVersion))
        End If
        If currentSoftwareVersion Is Nothing OrElse currentSoftwareVersion < info.MinimumSSWVersion Then
            Throw New InvalidDataException(String.Format(CultureInfo.InvariantCulture,
                "Fatal Error: DataCentral requires SSW {0} or newer.",
                info.MinimumSSWVersion))
        End If
        If Not String.Equals(info.CustomerCode, expectedCustomerCode, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException("Fatal Error: Invalid DataCentral (Mismatch Customer Code).")
        End If
        If String.IsNullOrWhiteSpace(info.DataVersion) OrElse
            String.IsNullOrWhiteSpace(info.ExporterVersion) OrElse
            Not IsSha256(info.ContentHash) Then
            Throw New InvalidDataException("Fatal Error: DataCentral manifest contains invalid values.")
        End If
    End Sub

    Private Shared Function IsSha256(value As String) As Boolean
        If String.IsNullOrEmpty(value) OrElse value.Length <> 64 Then
            Return False
        End If

        For Each character As Char In value
            If Not ((character >= "0"c AndAlso character <= "9"c) OrElse
                (character >= "A"c AndAlso character <= "F"c) OrElse
                (character >= "a"c AndAlso character <= "f"c)) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Shared Function TableExists(connection As SqlCeConnection, tableName As String) As Boolean
        Using command As SqlCeCommand = connection.CreateCommand()
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName"
            command.Parameters.AddWithValue("@TableName", tableName)
            Return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Shared Function TableColumnExists(connection As SqlCeConnection,
        tableName As String,
        columnName As String) As Boolean

        Using command As SqlCeCommand = connection.CreateCommand()
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " &
                "WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName"
            command.Parameters.AddWithValue("@TableName", tableName)
            command.Parameters.AddWithValue("@ColumnName", columnName)
            Return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

End Class
