Imports Climalombarda.DataCentral.LTModel
Imports System.Collections.Generic
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
    Function GetForModel(model As CLDCHeatRecoveryModel) As CLInstallationLayoutSnapshot
End Interface

' Adapter for the layout fields already present in the legacy SDF. It deliberately
' reports missing normalized images instead of resolving historic network paths.
Public NotInheritable Class CLLegacySdfInstallationLayoutRepository
    Implements ICLInstallationLayoutRepository

    Public Function GetForModel(model As CLDCHeatRecoveryModel) As CLInstallationLayoutSnapshot _
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
        SelectDefaultConfiguration(result.Configurations)

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

        If result.Configurations.Count = 0 Then
            result.DataIssues.Add("LayoutConfigurationsMissing")
        End If
        If result.FlowPorts.Any(Function(port) Not port.Position.HasValue) Then
            result.DataIssues.Add("FlowPortPositionsIncomplete")
        End If
        result.DataIssues.Add("OfflineLayoutImagesMissing")

        Return result
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
