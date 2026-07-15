Imports System.Text.Json.Serialization

Public NotInheritable Class CLSelectionProjectDocument

    Public Property Format As String = "SSWSelection"
    Public Property SelectionFormatVersion As Integer = CLTechnicalVersions.CurrentSelectionFormatVersion
    Public Property ProjectId As Guid = Guid.NewGuid()
    Public Property CreatedAtUtc As DateTime = DateTime.UtcNow
    Public Property ModifiedAtUtc As DateTime = DateTime.UtcNow
    Public Property Versions As New CLSelectionVersionSet()
    Public Property Features As New List(Of String)()
    Public Property Identity As New CLSelectionIdentity()
    Public Property Selection As New CLTechnicalSelection()
    Public Property Snapshot As CLCalculatedSelectionSnapshot
    Public Property RevisionTracking As New CLSelectionRevisionTracking()

    <JsonIgnore>
    Public Property SourceFormatVersion As Integer = CLTechnicalVersions.CurrentSelectionFormatVersion

    <JsonIgnore>
    Public Property RequiresMigrationBackup As Boolean

    <JsonIgnore>
    Public Property SourceFilePath As String

End Class

Public NotInheritable Class CLSelectionVersionSet

    Public Property SoftwareVersion As String
    Public Property CalculationEngineVersion As String
    Public Property DatabaseSchemaVersion As Integer
    Public Property DatabaseDataVersion As String
    Public Property DatabaseContentHash As String
    Public Property SelectionFormatVersion As Integer
    Public Property ReportTemplateVersion As Integer
    Public Property ApiContractVersion As Integer

End Class

Public NotInheritable Class CLSelectionIdentity

    Public Property LocalDraftReference As String
    Public Property PublicReference As String
    Public Property Revision As Integer?
    Public Property ParentPublicReference As String
    Public Property ResumeToken As String

End Class

Public NotInheritable Class CLSelectionRevisionTracking

    Public Property Current As CLSelectionFingerprintSet
    Public Property LastRegistered As CLRegisteredSelectionRevision
    Public Property PendingChangeKind As String

End Class

Public NotInheritable Class CLSelectionFingerprintSet

    Public Property SchemaVersion As Integer = 1
    Public Property TechnicalInputHash As String
    Public Property CalculationOutputHash As String
    Public Property CalculationBasisHash As String
    Public Property SnapshotHash As String

End Class

Public NotInheritable Class CLRegisteredSelectionRevision

    Public Property PublicReference As String
    Public Property Revision As Integer
    Public Property ResumeToken As String
    Public Property RegisteredAtUtc As DateTime
    Public Property Fingerprints As New CLSelectionFingerprintSet()
    Public Property Versions As New CLSelectionVersionSet()

End Class

Public NotInheritable Class CLTechnicalSelection

    Public Property CustomerCode As String
    Public Property CustomerReference As String
    Public Property Unit As New CLSelectionEntityReference()
    Public Property Winter As New CLOperatingScenarioInput With {.Enabled = True, .ScenarioCode = "Winter"}
    Public Property Summer As New CLOperatingScenarioInput With {.Enabled = False, .ScenarioCode = "Summer"}
    Public Property WaterCoil As New CLWaterCoilSelection()
    Public Property ElectricHeater As New CLElectricHeaterSelection()
    Public Property Report As New CLReportSelectionOptions()

End Class

Public NotInheritable Class CLElectricHeaterSelection

    Public Property Enabled As Boolean
    Public Property Stages As Integer?
    Public Property NominalPowerW As Double?
    Public Property EHD As New CLElectricHeaterModeSelection With {.Mode = "EHD"}
    Public Property PEHD As New CLElectricHeaterModeSelection With {.Mode = "PEHD"}

End Class

Public NotInheritable Class CLElectricHeaterModeSelection

    Public Property Enabled As Boolean
    Public Property Mode As String
    Public Property SelectionCase As String = "Standard"
    Public Property InstallationType As String = "Internal"
    Public Property CustomDesignDisclaimerAccepted As Boolean
    Public Property Heater As New CLSelectionEntityReference()

End Class

Public NotInheritable Class CLSelectionEntityReference

    Public Property Id As Integer?
    Public Property Code As String
    Public Property ManagementCode As String
    Public Property Name As String

End Class

Public NotInheritable Class CLOperatingScenarioInput

    Public Property Enabled As Boolean
    Public Property ScenarioCode As String
    Public Property StandardCode As String
    Public Property SupplyAirflowM3h As Double?
    Public Property ExtractAirflowM3h As Double?
    Public Property MaximumPressurePa As Double?
    Public Property OutdoorTemperatureC As Double?
    Public Property OutdoorRelativeHumidityPercent As Double?
    Public Property ReturnTemperatureC As Double?
    Public Property ReturnRelativeHumidityPercent As Double?
    Public Property RegulationPercent As Double?

End Class

Public NotInheritable Class CLWaterCoilSelection

    Public Property Enabled As Boolean
    Public Property CustomDesignDisclaimerAccepted As Boolean
    Public Property SelectionCase As String = "Standard"
    Public Property InstallationType As String = "Internal"
    Public Property CalculationMode As String = "HCD"
    Public Property Coil As New CLSelectionEntityReference()
    Public Property Fluid As New CLFluidSelection()
    Public Property Geometry As New CLCoilGeometrySelection()
    Public Property CoolingWaterInletTemperatureC As Double?
    Public Property CoolingWaterOutletTemperatureC As Double?
    Public Property HeatingWaterInletTemperatureC As Double?
    Public Property HeatingWaterOutletTemperatureC As Double?

End Class

Public NotInheritable Class CLFluidSelection

    Public Property Code As String = "Water"
    Public Property GlycolPercent As Double?

End Class

Public NotInheritable Class CLCoilGeometrySelection

    Public Property GeometryCode As String = "2510"
    Public Property LengthMm As Integer?
    Public Property HeightMm As Integer?
    Public Property Tubes As Integer?
    Public Property NumberOfRows As Integer?
    Public Property FinSpacingMm As Double?
    Public Property NumberOfCircuits As Integer?
    Public Property HeaderTypeCode As String

End Class

Public NotInheritable Class CLReportSelectionOptions

    Public Property LanguageCode As String
    Public Property IncludePerformanceCharts As Boolean = True
    Public Property IncludeSoundPower As Boolean
    Public Property IncludeCo2 As Boolean

End Class

Public NotInheritable Class CLCalculatedSelectionSnapshot

    Public Property Status As String = "Calculated"
    Public Property CalculatedAtUtc As DateTime
    Public Property Versions As New CLSelectionVersionSet()
    Public Property Winter As CLScenarioCalculationSnapshot
    Public Property Summer As CLScenarioCalculationSnapshot
    Public Property WaterCoils As New List(Of CLWaterCoilCalculationSnapshot)()
    Public Property ElectricHeaters As New List(Of CLElectricHeaterCalculationSnapshot)()

End Class

Public NotInheritable Class CLElectricHeaterCalculationSnapshot

    Public Property ScenarioCode As String = "Winter"
    Public Property Mode As String
    Public Property HeaterCode As String
    Public Property PowerW As Double?
    Public Property CurrentA As Double?
    Public Property AirInletTemperatureC As Double?
    Public Property AirOutletTemperatureC As Double?
    Public Property AirPressureDropPa As Double?
    Public Property ExhaustOutletTemperatureC As Double?

End Class

Public NotInheritable Class CLScenarioCalculationSnapshot

    Public Property ScenarioCode As String
    Public Property AirflowM3h As Double?
    Public Property AvailablePressurePa As Double?
    Public Property AbsorbedPowerW As Double?
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

Public NotInheritable Class CLWaterCoilCalculationSnapshot

    Public Property ScenarioCode As String
    Public Property Mode As String
    Public Property Status As String
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
