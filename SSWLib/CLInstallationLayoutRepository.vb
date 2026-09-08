Imports Climalombarda.DataCentral.LTModel
Imports System.Collections.Generic
Imports System.Data.SqlServerCe
Imports System.IO
Imports System.Linq

Public Enum CLInstallationLayoutOrientation
    Horizontal = 0
    Vertical = 1
End Enum

Public NotInheritable Class CLDimensionalValue
    Public Property Code As String
    Public Property ValueMillimeters As Double?
End Class

Public NotInheritable Class CLFlowPortDefinition
    Public Property FlowCode As String
    Public Property Position As Integer?
    Public Property X As Integer?
    Public Property Y As Integer?
    Public Property Z As Integer?
    Public Property K As Integer?
End Class

Public NotInheritable Class CLInstallationConfiguration
    Public Property Code As String
    Public Property Orientation As CLInstallationLayoutOrientation
    Public Property IsDefault As Boolean
    Public Property InstallationMode As String
    Public Property AccessSide As String
    Public Property ReferenceView As String
End Class

Public NotInheritable Class CLLayoutImageReference
    Public Property Content As Byte()
    Public Property MimeType As String
    Public Property Sha256 As String

    Public ReadOnly Property IsAvailable As Boolean
        Get
            Return Content IsNot Nothing AndAlso Content.Length > 0
        End Get
    End Property
End Class

Public NotInheritable Class CLInstallationLayoutSnapshot
    Public Property ModelCode As String
    Public Property AeraulicConnectionCode As String
    Public Property ConfigurationCode As String
    Public Property Configurations As New List(Of CLInstallationConfiguration)()
    Public Property HorizontalDimensions As New List(Of CLDimensionalValue)()
    Public Property VerticalDimensions As New List(Of CLDimensionalValue)()
    Public Property FlowPorts As New List(Of CLFlowPortDefinition)()
    Public Property DimensionalImage As New CLLayoutImageReference()
    Public Property FlowLayoutImage As New CLLayoutImageReference()
    Public Property DataIssues As New List(Of String)()

    Public ReadOnly Property DefaultConfiguration As CLInstallationConfiguration
        Get
            Return Configurations.FirstOrDefault(Function(item) item.IsDefault)
        End Get
    End Property

    Public ReadOnly Property HasOfflineImages As Boolean
        Get
            Return DimensionalImage.IsAvailable AndAlso FlowLayoutImage.IsAvailable
        End Get
    End Property
End Class

Public Interface ICLInstallationLayoutRepository
    Function GetForModel(model As CLDCHeatRecoveryModel,
        Optional configurationCode As String = Nothing) As CLInstallationLayoutSnapshot
End Interface

Public NotInheritable Class CLInstallationLayoutRepository
    Private Sub New()
    End Sub

    Public Shared Function Create() As ICLInstallationLayoutRepository
        Return New CLNormalizedSdfInstallationLayoutRepository(
            New CLLegacySdfInstallationLayoutRepository())
    End Function
End Class

' Reads the normalized catalog exported by CLDataCentralLib schema 4. Older SDF
' files transparently retain the legacy HorVariants/VerVariants behavior.
Public NotInheritable Class CLNormalizedSdfInstallationLayoutRepository
    Implements ICLInstallationLayoutRepository

    Private Const DatabasePassword As String = "@D3C1L4T2%"
    Private ReadOnly m_LegacyRepository As ICLInstallationLayoutRepository

    Public Sub New(legacyRepository As ICLInstallationLayoutRepository)
        If legacyRepository Is Nothing Then Throw New ArgumentNullException("legacyRepository")
        m_LegacyRepository = legacyRepository
    End Sub

    Public Function GetForModel(model As CLDCHeatRecoveryModel,
        Optional configurationCode As String = Nothing) As CLInstallationLayoutSnapshot _
        Implements ICLInstallationLayoutRepository.GetForModel

        Dim fallback = m_LegacyRepository.GetForModel(model, configurationCode)
        If model Is Nothing OrElse CLEnvironment.Current Is Nothing Then Return fallback

        Dim databasePath = CLEnvironment.Current.DCLiteDatabasePath
        If String.IsNullOrWhiteSpace(databasePath) OrElse Not File.Exists(databasePath) Then
            Throw New FileNotFoundException("The installation configuration database is unavailable.", databasePath)
        End If

        Try
            Using connection As New SqlCeConnection(String.Format(
                "Data Source=""{0}""; Password=""{1}""", databasePath, DatabasePassword))
                connection.Open()
                If Not HasNormalizedSchema(connection) Then Return fallback

                Dim configurations = ReadConfigurations(connection, model.Id)
                For Each entry In configurations
                    Dim definition = entry.Value
                    If Not {"ceiling", "floor", "wall"}.Contains(definition.InstallationMode) OrElse
                        Not {"upper", "lower", "front"}.Contains(definition.AccessSide) OrElse
                        Not {"OSC_NORTH_SOUTH", "OSC_EAST_WEST", "SSC_FRONT", "SSC_UPRIGHT", "SSC_FLAT"}.
                            Contains(definition.ReferenceView) Then
                        Throw New InvalidDataException("Invalid geometry metadata for configuration " & definition.Code)
                    End If
                Next
                fallback.Configurations = configurations.Select(Function(item) item.Value).ToList()
                fallback.ConfigurationCode = Nothing
                For Each port In fallback.FlowPorts
                    port.Position = Nothing
                Next
                If configurations.Count = 0 Then
                    fallback.DataIssues.Add("LayoutConfigurationsMissing")
                    Return fallback
                End If

                Dim selected = SelectConfiguration(configurations, configurationCode)
                If selected Is Nothing Then Return fallback

                Dim roles = ReadPortRoles(connection, selected.Value.Key)
                ValidatePortRoles(roles, selected.Value.Value.Code)

                fallback.Configurations = configurations.Select(Function(item) item.Value).ToList()
                fallback.ConfigurationCode = selected.Value.Value.Code
                ApplyPortRoles(fallback.FlowPorts, roles)
                fallback.DataIssues.Remove("LayoutConfigurationsMissing")
                fallback.DataIssues.Remove("FlowPortPositionsIncomplete")
                Return fallback
            End Using
        Catch ex As Exception
            Throw New InvalidDataException("Cannot read the installation configuration catalog.", ex)
        End Try
    End Function

    Private Shared Sub ValidatePortRoles(roles As SortedDictionary(Of Integer, String), code As String)
        If roles.Count <> 4 OrElse Not roles.Keys.SequenceEqual({1, 2, 3, 4}) OrElse
            Not New HashSet(Of String)(roles.Values, StringComparer.OrdinalIgnoreCase).
                SetEquals({"Fresh", "Supply", "Return", "Exhaust"}) Then
            Throw New InvalidDataException("Invalid flow ports for configuration " & code)
        End If
    End Sub

    Private Shared Function HasNormalizedSchema(connection As SqlCeConnection) As Boolean
        Dim count = {"CLFlowConfigurations", "CLFlowConfigurationPorts",
            "CLHeatRecoveryModelFlowConfigurations"}.Count(Function(name) TableExists(connection, name))
        If count <> 0 AndAlso count <> 3 Then
            Throw New InvalidDataException("Incomplete installation configuration schema.")
        End If
        If count = 0 AndAlso TableExists(connection, "CLDatabaseMetadata") Then
            Using command = connection.CreateCommand()
                command.CommandText = "SELECT MAX(SchemaVersion) FROM CLDatabaseMetadata"
                Dim version = command.ExecuteScalar()
                If version IsNot Nothing AndAlso version IsNot DBNull.Value AndAlso Convert.ToInt32(version) >= 4 Then
                    Throw New InvalidDataException("Installation configuration tables are missing from schema 4.")
                End If
            End Using
        End If
        Return count = 3
    End Function

    Private Shared Function TableExists(connection As SqlCeConnection, tableName As String) As Boolean
        Using command = connection.CreateCommand()
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName"
            command.Parameters.AddWithValue("@TableName", tableName)
            Return Convert.ToInt32(command.ExecuteScalar()) > 0
        End Using
    End Function

    Private Shared Function ReadConfigurations(connection As SqlCeConnection,
        modelId As Integer) As List(Of KeyValuePair(Of Integer, CLInstallationConfiguration))

        Dim result As New List(Of KeyValuePair(Of Integer, CLInstallationConfiguration))()
        Using command = connection.CreateCommand()
            command.CommandText =
                "SELECT configuration.Id, configuration.Code, configuration.Orientation, " &
                "configuration.InstallationMode, configuration.AccessSide, configuration.ReferenceView, " &
                "relation.IsDefault " &
                "FROM CLHeatRecoveryModelFlowConfigurations relation " &
                "INNER JOIN CLFlowConfigurations configuration ON configuration.Id = relation.IdFlowConfiguration " &
                "WHERE relation.IdHeatRecoveryModel = @ModelId AND relation.Active = 1 " &
                "AND configuration.Active = 1 " &
                "ORDER BY relation.SortOrder, configuration.SortOrder, configuration.Code"
            command.Parameters.AddWithValue("@ModelId", modelId)

            Using reader = command.ExecuteReader()
                While reader.Read()
                    Dim code = reader.GetString(1).Trim().ToUpperInvariant()
                    If Not {"H", "V"}.Contains(reader.GetString(2).Trim().ToUpperInvariant()) Then
                        Throw New InvalidDataException("Invalid dimensional orientation for configuration " & code)
                    End If
                    If result.Any(Function(item) String.Equals(item.Value.Code, code,
                        StringComparison.OrdinalIgnoreCase)) Then
                        Throw New InvalidDataException("Duplicate installation configuration: " & code)
                    End If

                    result.Add(New KeyValuePair(Of Integer, CLInstallationConfiguration)(
                        reader.GetInt32(0),
                        New CLInstallationConfiguration With {
                            .Code = code,
                            .Orientation = If(String.Equals(reader.GetString(2), "V",
                                StringComparison.OrdinalIgnoreCase),
                                CLInstallationLayoutOrientation.Vertical,
                                CLInstallationLayoutOrientation.Horizontal),
                            .InstallationMode = reader.GetString(3),
                            .AccessSide = reader.GetString(4),
                            .ReferenceView = reader.GetString(5),
                            .IsDefault = reader.GetBoolean(6)
                        }))
                End While
            End Using
        End Using
        If result.Where(Function(item) item.Value.IsDefault).Count() > 1 Then
            Throw New InvalidDataException("Multiple default installation configurations for model " & modelId.ToString())
        End If
        If result.Count > 0 AndAlso Not result.Any(Function(item) item.Value.IsDefault) Then
            result(0).Value.IsDefault = True
        End If
        Return result
    End Function

    Private Shared Function SelectConfiguration(
        configurations As IList(Of KeyValuePair(Of Integer, CLInstallationConfiguration)),
        requestedCode As String) As KeyValuePair(Of Integer, CLInstallationConfiguration)?

        Dim normalized = If(requestedCode, String.Empty).Trim()
        For Each item In configurations
            If String.Equals(item.Value.Code, normalized, StringComparison.OrdinalIgnoreCase) Then
                Return item
            End If
        Next
        For Each item In configurations
            If item.Value.IsDefault Then Return item
        Next
        If configurations.Count > 0 Then Return configurations(0)
        Return Nothing
    End Function

    Private Shared Function ReadPortRoles(connection As SqlCeConnection,
        configurationId As Integer) As SortedDictionary(Of Integer, String)

        Dim result As New SortedDictionary(Of Integer, String)()
        Using command = connection.CreateCommand()
            command.CommandText = "SELECT PositionNumber, AirRole FROM CLFlowConfigurationPorts " &
                "WHERE IdFlowConfiguration = @ConfigurationId ORDER BY PositionNumber"
            command.Parameters.AddWithValue("@ConfigurationId", configurationId)
            Using reader = command.ExecuteReader()
                While reader.Read()
                    result.Add(Convert.ToInt32(reader.GetValue(0)), reader.GetString(1).Trim())
                End While
            End Using
        End Using
        Return result
    End Function

    Private Shared Sub ApplyPortRoles(ports As IList(Of CLFlowPortDefinition),
        roles As IDictionary(Of Integer, String))

        For Each port In ports
            port.Position = Nothing
        Next
        For Each role In roles
            Dim port = ports.FirstOrDefault(Function(item) String.Equals(
                item.FlowCode, role.Value, StringComparison.OrdinalIgnoreCase))
            If port IsNot Nothing Then port.Position = role.Key
        Next
    End Sub
End Class

' Adapter for the layout fields already present in the legacy SDF. It deliberately
' reports missing normalized images instead of resolving historic network paths.
Public NotInheritable Class CLLegacySdfInstallationLayoutRepository
    Implements ICLInstallationLayoutRepository

    Public Function GetForModel(model As CLDCHeatRecoveryModel,
        Optional configurationCode As String = Nothing) As CLInstallationLayoutSnapshot _
        Implements ICLInstallationLayoutRepository.GetForModel

        If model Is Nothing Then Throw New ArgumentNullException("model")

        Dim result As New CLInstallationLayoutSnapshot With {
            .ModelCode = model.Code,
            .AeraulicConnectionCode = If(model.CLEnumItem_AeraulicConnection Is Nothing,
                Nothing, model.CLEnumItem_AeraulicConnection.TextCode)
        }

        AddConfigurations(result.Configurations, model.HorVariantsItems,
            CLInstallationLayoutOrientation.Horizontal)
        AddConfigurations(result.Configurations, model.VerVariantsItems,
            CLInstallationLayoutOrientation.Vertical)
        ApplyLegacyInstallationModes(
            result.Configurations,
            result.AeraulicConnectionCode,
            model.Code)
        SelectDefaultConfiguration(result.Configurations)
        Dim selectedConfiguration = SelectConfiguration(
            result.Configurations, configurationCode)
        result.ConfigurationCode = If(selectedConfiguration Is Nothing,
            Nothing, selectedConfiguration.Code)

        AddDimensions(result.HorizontalDimensions,
            model.Dimension_A_Hor, model.Dimension_B_Hor,
            model.Dimension_C_Hor, model.Dimension_D_Hor)
        AddDimensions(result.VerticalDimensions,
            model.Dimension_A_Ver, model.Dimension_B_Ver,
            model.Dimension_C_Ver, model.Dimension_D_Ver)

        result.FlowPorts.Add(CreatePort("Fresh", model.TFreshPosition,
            model.Fresh_X, model.Fresh_Y, model.Fresh_Z, model.Fresh_K))
        result.FlowPorts.Add(CreatePort("Return", model.TReturnPosition,
            model.Return_X, model.Return_Y, model.Return_Z, model.Return_K))
        result.FlowPorts.Add(CreatePort("Supply", model.TSupplyPosition,
            model.Supply_X, model.Supply_Y, model.Supply_Z, model.Supply_K))
        result.FlowPorts.Add(CreatePort("Exhaust", model.TExaustPosition,
            model.Exaust_X, model.Exaust_Y, model.Exaust_Z, model.Exaust_K))
        ApplyLegacyConfigurationPorts(
            result.FlowPorts,
            result.ConfigurationCode,
            result.AeraulicConnectionCode,
            model.Code)

        If result.Configurations.Count = 0 Then
            result.DataIssues.Add("LayoutConfigurationsMissing")
        End If
        If result.FlowPorts.Any(Function(port) Not port.Position.HasValue) Then
            result.DataIssues.Add("FlowPortPositionsIncomplete")
        End If
        result.DataIssues.Add("OfflineLayoutImagesMissing")

        Return result
    End Function

    Private Shared Function SelectConfiguration( _
        configurations As IList(Of CLInstallationConfiguration), _
        requestedCode As String) As CLInstallationConfiguration

        Dim normalizedCode = If(requestedCode, String.Empty).Trim()
        Dim selected = configurations.FirstOrDefault(Function(item) _
            String.Equals(item.Code, normalizedCode, StringComparison.OrdinalIgnoreCase))
        If selected IsNot Nothing Then Return selected
        Return configurations.FirstOrDefault(Function(item) item.IsDefault)
    End Function

    Private Shared Sub ApplyLegacyConfigurationPorts( _
        ports As IList(Of CLFlowPortDefinition), _
        configurationCode As String, _
        aeraulicConnectionCode As String, _
        modelCode As String)

        If ports Is Nothing OrElse ports.Count <> 4 Then Return

        Dim roles As String() = Nothing
        If Not String.IsNullOrWhiteSpace(configurationCode) Then
            roles = LegacyPortRoles(
                configurationCode,
                aeraulicConnectionCode,
                modelCode)
        End If

        If roles Is Nothing OrElse roles.Length <> 4 Then
            Dim existingPositions = ports. _
                Where(Function(item) item.Position.HasValue AndAlso
                    item.Position.Value >= 1 AndAlso
                    item.Position.Value <= 4). _
                Select(Function(item) item.Position.Value). _
                Distinct().Count()
            If existingPositions = 4 Then Return

            roles = If(IsSameSideConnection(
                aeraulicConnectionCode, modelCode),
                {"Exhaust", "Fresh", "Return", "Supply"},
                {"Exhaust", "Fresh", "Supply", "Return"})
        End If

        For Each port In ports
            port.Position = Nothing
        Next
        For position = 1 To 4
            Dim role = roles(position - 1)
            Dim port = ports.FirstOrDefault(Function(item) _
                String.Equals(item.FlowCode, role, _
                    StringComparison.OrdinalIgnoreCase))
            If port IsNot Nothing Then port.Position = position
        Next
    End Sub

    ' Compatibility mapping from the configuration sheet currently used by the
    ' legacy SDF. Normalized configuration-port rows will replace this table.
    Private Shared Function LegacyPortRoles( _
        configurationCode As String, _
        aeraulicConnectionCode As String, _
        modelCode As String) As String()

        Dim code = configurationCode.Trim().ToUpperInvariant()
        Dim sameSide = IsSameSideConnection(
            aeraulicConnectionCode,
            modelCode)

        If sameSide Then
            Select Case code
                Case "A1", "A2", "A3"
                    Return {"Exhaust", "Fresh", "Return", "Supply"}
                Case "B1", "B2", "B3"
                    Return {"Supply", "Return", "Fresh", "Exhaust"}
            End Select
            Return Nothing
        End If

        Select Case code
            Case "A3", "A4", "B1"
                Return {"Exhaust", "Fresh", "Return", "Supply"}
            Case "B2", "B3"
                Return {"Supply", "Return", "Fresh", "Exhaust"}
            Case "B4", "B5", "B6"
                Return {"Exhaust", "Fresh", "Supply", "Return"}
            Case "C1", "C4"
                Return {"Exhaust", "Supply", "Fresh", "Return"}
            Case "C2", "D3"
                Return {"Fresh", "Supply", "Exhaust", "Return"}
            Case "C3", "D1", "D2", "D4"
                Return {"Supply", "Exhaust", "Return", "Fresh"}
        End Select
        Return Nothing
    End Function

    Private Shared Sub ApplyLegacyInstallationModes( _
        configurations As IEnumerable(Of CLInstallationConfiguration), _
        aeraulicConnectionCode As String, _
        modelCode As String)

        Dim sameSide = IsSameSideConnection(
            aeraulicConnectionCode,
            modelCode)
        Dim oppositeSide = IsOppositeSideConnection(
            aeraulicConnectionCode,
            modelCode)

        For Each layoutItem In configurations
            Dim code = If(layoutItem.Code, String.Empty). _
                Trim().ToUpperInvariant()
            If sameSide Then
                Select Case code
                    Case "A2", "B2"
                        layoutItem.InstallationMode = "ceiling"
                    Case "A1", "A3", "B1", "B3"
                        layoutItem.InstallationMode = "floor"
                End Select
            ElseIf oppositeSide Then
                Select Case code
                    Case "A4", "B6", "C4", "D4"
                        layoutItem.InstallationMode = "ceiling"
                    Case "A3", "B5", "C3", "D3"
                        layoutItem.InstallationMode = "floor"
                    Case "B1", "B2", "B3", "B4", "C1", "C2", "D1", "D2"
                        layoutItem.InstallationMode = "wall"
                End Select
            Else
                ' VS, FS and every connection family without an explicit
                ' mapping use the OSC installation convention.
                Select Case code
                    Case "A4", "B6", "C4", "D4"
                        layoutItem.InstallationMode = "ceiling"
                    Case "A3", "B5", "C3", "D3"
                        layoutItem.InstallationMode = "floor"
                    Case "B1", "B2", "B3", "B4", "C1", "C2", "D1", "D2"
                        layoutItem.InstallationMode = "wall"
                End Select
            End If
            layoutItem.AccessSide = If(layoutItem.InstallationMode = "wall", "front",
                If(layoutItem.InstallationMode = "floor", "upper", "lower"))
            layoutItem.ReferenceView = "OSC_NORTH_SOUTH"
            If sameSide Then
                layoutItem.ReferenceView = "SSC_FRONT"
                If {"A1", "B1"}.Contains(code) Then
                    layoutItem.ReferenceView = "SSC_UPRIGHT"
                    layoutItem.AccessSide = "front"
                ElseIf {"A3", "B3"}.Contains(code) Then
                    layoutItem.ReferenceView = "SSC_FLAT"
                End If
            ElseIf layoutItem.InstallationMode = "wall" AndAlso
                {"B1", "B2", "C1", "D1"}.Contains(code) Then
                layoutItem.ReferenceView = "OSC_EAST_WEST"
            End If
        Next
    End Sub

    Private Shared Function IsSameSideConnection( _
        aeraulicConnectionCode As String, _
        modelCode As String) As Boolean

        Dim connection = If(aeraulicConnectionCode, String.Empty)
        Dim model = If(modelCode, String.Empty)
        Return connection.IndexOf("SSC", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            connection.IndexOf("SAME", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            model.IndexOf("SSC", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Function IsOppositeSideConnection( _
        aeraulicConnectionCode As String, _
        modelCode As String) As Boolean

        Dim connection = If(aeraulicConnectionCode, String.Empty)
        Dim model = If(modelCode, String.Empty)
        Return connection.IndexOf("OSC", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            connection.IndexOf("OPPOSITE", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            model.IndexOf("OSC", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Sub AddConfigurations(target As IList(Of CLInstallationConfiguration),
        source As IEnumerable(Of String),
        orientation As CLInstallationLayoutOrientation)

        If source Is Nothing Then Return
        For Each rawCode As String In source
            Dim code As String = If(rawCode, String.Empty).Trim().ToUpperInvariant()
            If code.Length = 0 OrElse
               target.Any(Function(item) String.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)) Then
                Continue For
            End If
            target.Add(New CLInstallationConfiguration With {
                .Code = code,
                .Orientation = orientation
            })
        Next
    End Sub

    Private Shared Sub SelectDefaultConfiguration(configurations As IList(Of CLInstallationConfiguration))
        If configurations.Count = 0 Then Return

        Dim selected As CLInstallationConfiguration =
            configurations.FirstOrDefault(Function(item) item.Orientation = CLInstallationLayoutOrientation.Horizontal AndAlso item.Code = "B6")
        If selected Is Nothing Then
            selected = configurations.FirstOrDefault(Function(item) item.Orientation = CLInstallationLayoutOrientation.Horizontal AndAlso item.Code = "A4")
        End If
        If selected Is Nothing Then
            selected = configurations.FirstOrDefault(Function(item) item.Orientation = CLInstallationLayoutOrientation.Horizontal)
        End If
        If selected Is Nothing Then selected = configurations(0)
        selected.IsDefault = True
    End Sub

    Private Shared Sub AddDimensions(target As IList(Of CLDimensionalValue),
        a As Double?,
        b As Double?,
        c As Double?,
        d As Double?)

        target.Add(New CLDimensionalValue With {.Code = "A", .ValueMillimeters = a})
        target.Add(New CLDimensionalValue With {.Code = "B", .ValueMillimeters = b})
        target.Add(New CLDimensionalValue With {.Code = "C", .ValueMillimeters = c})
        target.Add(New CLDimensionalValue With {.Code = "D", .ValueMillimeters = d})
    End Sub

    Private Shared Function CreatePort(code As String,
        position As Integer?,
        x As Integer?,
        y As Integer?,
        z As Integer?,
        k As Integer?) As CLFlowPortDefinition

        Return New CLFlowPortDefinition With {
            .FlowCode = code,
            .Position = position,
            .X = x,
            .Y = y,
            .Z = z,
            .K = k
        }
    End Function
End Class
