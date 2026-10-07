Imports System.Collections.Generic

Public Enum CLValidationSeverity
    Information
    Warning
    [Error]
End Enum

Public Enum CLChartKind
    AirflowPressure
    AbsorbedPower
    Efficiency
End Enum

Public Enum CLChartSeriesRole
    Supply
    Extract
    Winter
    Summer
    WorkingPoint
End Enum

Public NotInheritable Class CLAirflowPair

    Public Property SupplyM3h As Double?
    Public Property ExtractM3h As Double?

    Public ReadOnly Property IsComplete As Boolean
        Get
            Return SupplyM3h.HasValue AndAlso ExtractM3h.HasValue
        End Get
    End Property

    Public Function IsBalanced(Optional toleranceM3h As Double = 0.001R) As Boolean
        Return IsComplete AndAlso Math.Abs(SupplyM3h.Value - ExtractM3h.Value) <= Math.Abs(toleranceM3h)
    End Function

    Public Shared Function Balanced(airflowM3h As Double?) As CLAirflowPair
        Return New CLAirflowPair With {
            .SupplyM3h = airflowM3h,
            .ExtractM3h = airflowM3h
        }
    End Function

End Class

Public NotInheritable Class CLBranchValuePair

    Public Property Supply As Double?
    Public Property Extract As Double?

    Public Shared Function Balanced(value As Double?) As CLBranchValuePair
        Return New CLBranchValuePair With {
            .Supply = value,
            .Extract = value
        }
    End Function

End Class

Public NotInheritable Class CLCalculationEntityReference

    Public Property Id As Integer?
    Public Property Code As String
    Public Property ManagementCode As String
    Public Property Name As String

End Class

Public NotInheritable Class CLSelectionCalculationInput

    Public Property CustomerCode As String
    Public Property CustomerReference As String
    Public Property Unit As New CLCalculationEntityReference()
    Public Property Winter As New CLSeasonCalculationInput With {.Enabled = True, .ScenarioCode = "Winter"}
    Public Property Summer As New CLSeasonCalculationInput With {.ScenarioCode = "Summer"}
    Public Property WaterCoil As New CLWaterCoilCalculationOptions()
    Public Property ElectricHeater As New CLElectricHeaterCalculationOptions()
    Public Property Accessories As New List(Of CLAccessoryCalculationOption)()

End Class

Public NotInheritable Class CLSeasonCalculationInput

    Public Property Enabled As Boolean
    Public Property ScenarioCode As String
    Public Property StandardCode As String
    Public Property Airflows As New CLAirflowPair()
    Public Property MaximumPressurePa As New CLBranchValuePair()
    Public Property OutdoorTemperatureC As Double?
    Public Property OutdoorRelativeHumidityPercent As Double?
    Public Property ReturnTemperatureC As Double?
    Public Property ReturnRelativeHumidityPercent As Double?
    Public Property RegulationPercent As Double?

End Class

Public NotInheritable Class CLWaterCoilCalculationOptions

    Public Property Enabled As Boolean
    Public Property CustomDesignDisclaimerAccepted As Boolean
    Public Property SelectionCase As String
    Public Property InstallationType As String
    Public Property CalculationMode As String
    Public Property Coil As New CLCalculationEntityReference()
    Public Property FluidCode As String
    Public Property GlycolPercent As Double?
    Public Property GeometryCode As String
    Public Property LengthMm As Integer?
    Public Property HeightMm As Integer?
    Public Property Tubes As Integer?
    Public Property NumberOfRows As Integer?
    Public Property FinSpacingMm As Double?
    Public Property NumberOfCircuits As Integer?
    Public Property HeaderTypeCode As String
    Public Property CoolingWaterInletTemperatureC As Double?
    Public Property CoolingWaterOutletTemperatureC As Double?
    Public Property HeatingWaterInletTemperatureC As Double?
    Public Property HeatingWaterOutletTemperatureC As Double?

End Class

Public NotInheritable Class CLElectricHeaterCalculationOptions

    Public Property Enabled As Boolean
    Public Property Stages As Integer?
    Public Property NominalPowerW As Double?
    Public Property EHD As New CLElectricHeaterModeCalculationOption With {.Mode = "EHD"}
    Public Property PEHD As New CLElectricHeaterModeCalculationOption With {.Mode = "PEHD"}

End Class

Public NotInheritable Class CLElectricHeaterModeCalculationOption

    Public Property Enabled As Boolean
    Public Property Mode As String
    Public Property SelectionCase As String
    Public Property InstallationType As String
    Public Property CustomDesignDisclaimerAccepted As Boolean
    Public Property Heater As New CLCalculationEntityReference()

End Class

Public NotInheritable Class CLAccessoryCalculationOption

    Public Property Code As String
    Public Property ItemType As String
    Public Property Quantity As Integer
    Public Property Availability As String
    Public Property InstallationType As String

End Class

Public NotInheritable Class CLSelectionCalculationResult

    Public Property StatusCode As String
    Public Property Winter As CLSeasonCalculationResult
    Public Property Summer As CLSeasonCalculationResult
    Public Property WaterCoils As New List(Of CLWaterCoilResult)()
    Public Property ElectricHeaters As New List(Of CLElectricHeaterResult)()
    Public Property Charts As New List(Of CLChartDefinition)()
    Public Property Validation As New CLValidationResult()

End Class

Public NotInheritable Class CLSeasonCalculationResult

    Public Property ScenarioCode As String
    Public Property SupplyBranch As New CLBranchCalculationResult With {.BranchCode = "Supply"}
    Public Property ExtractBranch As New CLBranchCalculationResult With {.BranchCode = "Extract"}
    Public Property CombinedSpecificFanPowerWPerM3hPerSecond As Double?
    Public Property Thermodynamics As New CLThermodynamicCalculationResult()

End Class

Public NotInheritable Class CLBranchCalculationResult

    Public Property BranchCode As String
    Public Property AirflowM3h As Double?
    Public Property AvailablePressurePa As Double?
    Public Property AbsorbedPowerW As Double?
    Public Property SpecificFanPowerWPerM3hPerSecond As Double?

End Class

Public NotInheritable Class CLThermodynamicCalculationResult

    Public Property HeatTransferredW As Double?
    Public Property SensibleHeatW As Double?
    Public Property LatentHeatW As Double?
    Public Property EfficiencyPercent As Double?
    Public Property CondensateLitersPerHour As Double?
    Public Property SupplyOutletTemperatureC As Double?
    Public Property SupplyOutletRelativeHumidityPercent As Double?
    Public Property ExhaustOutletTemperatureC As Double?
    Public Property ExhaustOutletRelativeHumidityPercent As Double?

End Class

Public NotInheritable Class CLWaterCoilResult

    Public Property ScenarioCode As String
    Public Property Mode As String
    Public Property StatusCode As String
    Public Property CapacityW As Double?
    Public Property SensibleCapacityW As Double?
    Public Property AirOutletTemperatureC As Double?
    Public Property AirOutletRelativeHumidityPercent As Double?
    Public Property CondensateLitersPerHour As Double?
    Public Property AirPressureDropPa As Double?
    Public Property FluidPressureDropKPa As Double?
    Public Property FluidFlowLitersPerHour As Double?
    Public Property FluidVelocityMetersPerSecond As Double?
    Public Property FaceVelocityMetersPerSecond As Double?

End Class

Public NotInheritable Class CLElectricHeaterResult

    Public Property ScenarioCode As String
    Public Property Mode As String
    Public Property HeaterCode As String
    Public Property PowerW As Double?
    Public Property CurrentA As Double?
    Public Property AirInletTemperatureC As Double?
    Public Property AirOutletTemperatureC As Double?
    Public Property AirOutletRelativeHumidityPercent As Double?
    Public Property AirPressureDropPa As Double?
    Public Property ExhaustOutletTemperatureC As Double?

End Class

Public NotInheritable Class CLChartDefinition

    Public Property ChartCode As String
    Public Property Kind As CLChartKind
    Public Property XAxisUnit As String
    Public Property YAxisUnit As String
    Public Property Series As New List(Of CLChartSeries)()

End Class

Public NotInheritable Class CLChartSeries

    Public Property SeriesCode As String
    Public Property Role As CLChartSeriesRole
    Public Property BranchCode As String
    Public Property ScenarioCode As String
    Public Property Points As New List(Of CLChartPoint)()

End Class

Public NotInheritable Class CLChartPoint

    Public Property X As Double
    Public Property Y As Double

End Class

Public NotInheritable Class CLValidationIssue

    Public Property Code As String
    Public Property Severity As CLValidationSeverity
    Public Property Path As String
    Public Property MessageKey As String
    Public Property Arguments As New List(Of String)()

End Class

Public NotInheritable Class CLValidationResult

    Public Property Issues As New List(Of CLValidationIssue)()

    Public ReadOnly Property HasErrors As Boolean
        Get
            Return Issues.Exists(Function(issue As CLValidationIssue) issue.Severity = CLValidationSeverity.Error)
        End Get
    End Property

    Public ReadOnly Property HasWarnings As Boolean
        Get
            Return Issues.Exists(Function(issue As CLValidationIssue) issue.Severity = CLValidationSeverity.Warning)
        End Get
    End Property

End Class
