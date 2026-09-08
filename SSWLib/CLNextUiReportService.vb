Imports System.Data
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms.DataVisualization.Charting
Imports Climalombarda.Common
Imports Microsoft.Reporting.WinForms

Public NotInheritable Class CLNextUiReportService

    Private Const ChartWidth As Integer = 1440
    Private Const ChartHeight As Integer = 800

    Private Sub New()
    End Sub

    Public Shared Function Prepare(
        document As CLSelectionProjectDocument,
        Optional calculation As CLNextUiCalculationResult = Nothing) As CLPreparedNextUiReport

        If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
        If document.Selection Is Nothing Then Throw New InvalidDataException("The technical selection is missing.")
        Dim input As CLNextUiCalculationInput =
            CLNextUiApplicationService.CreateInputFromProjectDocument(document)
        If calculation Is Nothing Then calculation = CLNextUiApplicationService.Calculate(input)
        CLNextUiApplicationService.PopulateCalculatedSnapshot(document, calculation)

        Dim sources As New List(Of ReportDataSource)()
        Dim header = CreateTable("Header",
            "UnitSelected_Caption", "UnitSelected_Value", "Date_Caption", "Date_Value",
            "SoftwareRelease_Caption", "SoftwareRelease_Value", "Page_Caption", "LogoBmp",
            "CustomerInfo", "Note", "Item_Code", "Item_Description",
            "TechnicalSelectionReference_Caption", "TechnicalSelectionReference_Value",
            "TechnicalSelectionRevision_Caption", "TechnicalSelectionRevision_Value",
            "TechnicalSelectionStatus_Caption", "TechnicalSelectionStatus_Value")
        AddRow(header,
            "UnitSelected_Caption", L("MainForm_UnitSelected", "Selected unit"),
            "UnitSelected_Value", calculation.Model.Code,
            "Date_Caption", L("MainForm_Date", "Date"),
            "Date_Value", DateTime.Now,
            "SoftwareRelease_Caption", L("Release", "Software release"),
            "SoftwareRelease_Value", CLEnvironment.Current.SSWInfo.ReleaseVersion.ToString(),
            "Page_Caption", L("Page", "Page"),
            "LogoBmp", ImageBytes(CLEnvironment.Current.CustomerLogo),
            "CustomerInfo", CLEnvironment.Current.SSWInfo.CustomerInfo,
            "Note", document.Selection.CustomerReference,
            "Item_Code", If(document.Selection.Unit.Code, calculation.Model.Code),
            "Item_Description", If(document.Selection.Unit.Name, calculation.Model.Name),
            "TechnicalSelectionReference_Caption", L("MainForm_Project_Title", "Technical selection"),
            "TechnicalSelectionReference_Value", SelectionReference(document),
            "TechnicalSelectionRevision_Caption", L("MainForm_TechnicalSelectionRevision", "Revision"),
            "TechnicalSelectionRevision_Value", SelectionRevision(document),
            "TechnicalSelectionStatus_Caption", L("MainForm_TechnicalSelectionStatus", "Status"),
            "TechnicalSelectionStatus_Value", SelectionStatus(document))

        Dim accordance = CreateTable("PerformanceAccordance",
            "Title", "ExternalLeakage_Caption", "ExternalLeakage_Value",
            "InternalLeakage_Caption", "InternalLeakage_Value",
            "AirflowPressure_Caption", "AirflowPressure_Value",
            "ElectricPowerInput_Caption", "ElectricPowerInput_Value",
            "NoiseLevel_Caption", "NoiseLevel_Value", "Legal_Notice")
        AddRow(accordance,
            "Title", L("PerformancesAccordanceWith", "All measurements are performed according to"),
            "ExternalLeakage_Caption", L("ExternalLeakage", "External leakage"),
            "ExternalLeakage_Value", CLEnvironment.ExternalLeakage,
            "InternalLeakage_Caption", L("InternalLeakage", "Internal leakage"),
            "InternalLeakage_Value", CLEnvironment.InternalLeakage,
            "AirflowPressure_Caption", L("AirflowPressure", "Airflow / Pressure"),
            "AirflowPressure_Value", CLEnvironment.AirflowPressure,
            "ElectricPowerInput_Caption", L("ElectricPowerInput", "Electric power"),
            "ElectricPowerInput_Value", CLEnvironment.ElectricPowerInput,
            "NoiseLevel_Caption", L("NoiseLevel", "Sound level"),
            "NoiseLevel_Value", CLEnvironment.NoiseLevel,
            "Legal_Notice", L("Legal_Notice", String.Empty))

        Dim working = WorkingPointTable("WorkingPoint")
        Dim temperatures = TemperatureTable("TemperatureConditionsAndHumidity")
        Dim exchanger = ExchangerTable("HeatExchangerPerformances")
        Dim winterWorking = WorkingPointTable("WinterWorkingPoint")
        Dim summerWorking = WorkingPointTable("SummerWorkingPoint")
        Dim winterTemperature = TemperatureTable("WinterTemperatureConditionsAndHumidity")
        Dim summerTemperature = TemperatureTable("SummerTemperatureConditionsAndHumidity")
        Dim winterExchanger = ExchangerTable("WinterHeatExchangerPerformances")
        Dim summerExchanger = ExchangerTable("SummerHeatExchangerPerformances")

        AddScenarioRows(document.Selection.Winter, calculation.Winter, "Winter",
            working, temperatures, exchanger, winterWorking, winterTemperature, winterExchanger)
        If document.Selection.Summer.Enabled AndAlso calculation.Summer IsNot Nothing Then
            AddScenarioRows(document.Selection.Summer, calculation.Summer, "Summer",
                working, temperatures, exchanger, summerWorking, summerTemperature, summerExchanger)
        End If

        Dim diagrams = CreateTable("Diagram",
            "PressureImage", "PowerImage", "AirFlowImage", "LegendImage", "CO2Image",
            "InstallationImage", "InstallationTitle",
            "InstallationConfigurationCaption", "InstallationConfigurationValue",
            "InstallationModeCaption", "InstallationModeValue")
        Dim reportModel = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(item) item.Id = calculation.Model.Id)
        If reportModel Is Nothing Then
            reportModel = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
                FirstOrDefault(Function(item) String.Equals(item.Code,
                    calculation.Model.Code, StringComparison.OrdinalIgnoreCase))
        End If
        Using installation = CLInstallationLayoutReportRenderer.Create(
            reportModel, document.Selection.LayoutCode,
            document.Selection.InstallationMode)
        AddRow(diagrams,
            "PressureImage", BuildPressureChart(calculation),
            "PowerImage", BuildPowerChart(calculation),
            "AirFlowImage", BuildEfficiencyChart(calculation, document.Selection.Summer.Enabled),
            "LegendImage", BuildLegend(calculation, document.Selection.Summer.Enabled),
            "CO2Image", BuildCo2Chart(calculation.Co2),
            "InstallationImage", BitmapBytes(installation.Image),
            "InstallationTitle", installation.Title,
            "InstallationConfigurationCaption", installation.ConfigurationCaption,
            "InstallationConfigurationValue", installation.ConfigurationValue,
            "InstallationModeCaption", installation.InstallationCaption,
            "InstallationModeValue", installation.InstallationValue)
        End Using

        Dim water = WaterTable("WaterCoilReport")
        Dim waterWinter = WaterTable("WaterCoilWinterReport")
        Dim waterSummer = WaterTable("WaterCoilSummerReport")
        FillWaterCoils(document, calculation, water, waterWinter, waterSummer)

        Dim electric = ElectricTable("ElectricHeaterReport")
        Dim electricEhd = ElectricTable("ElectricHeaterEHDReport")
        Dim electricPehd = ElectricTable("ElectricHeaterPEHDReport")
        FillElectricHeaters(document, calculation, electric, electricEhd, electricPehd)

        Dim accessories = CreateTable("AccessoryReport",
            "Title", "CodeCaption", "DescriptionCaption", "FunctionsCaption",
            "StatusCaption", "Code", "Description", "Functions", "Status")
        FillAccessories(document, accessories)
        Dim sound = SoundTable("SoundPower")
        Dim soundHeader = SoundHeaderTable()
        FillSound(document, calculation, sound, soundHeader)
        Dim co2Room = Co2RoomTable()
        Dim co2Use = Co2UseTable()
        Dim co2Parameters = Co2ParametersTable()
        FillCo2(document, calculation, co2Room, co2Use, co2Parameters)

        AddSource(sources, header)
        AddSource(sources, accordance)
        AddSource(sources, temperatures)
        AddSource(sources, sound)
        AddSource(sources, soundHeader)
        AddSource(sources, working)
        AddSource(sources, exchanger)
        AddSource(sources, diagrams)
        AddSource(sources, co2Room)
        AddSource(sources, co2Use)
        AddSource(sources, co2Parameters)
        AddSource(sources, water)
        AddSource(sources, winterWorking)
        AddSource(sources, winterTemperature)
        AddSource(sources, winterExchanger)
        AddSource(sources, summerWorking)
        AddSource(sources, summerTemperature)
        AddSource(sources, summerExchanger)
        AddSource(sources, waterWinter)
        AddSource(sources, waterSummer)
        AddSource(sources, electric)
        AddSource(sources, electricEhd)
        AddSource(sources, electricPehd)
        AddSource(sources, accessories)

        Dim hasTreatment As Boolean =
            water.Rows.Count > 0 OrElse electric.Rows.Count > 0
        Dim includeCo2 = document.Selection.Report IsNot Nothing AndAlso
            document.Selection.Report.IncludeCo2
        Dim template As String
        If includeCo2 Then
            template = If(hasTreatment,
                "CLMainReportWithCO2_Coil.rdlc", "CLMainReportWithCO2.rdlc")
        Else
            template = If(hasTreatment, "CLMainReport_Coil.rdlc", "CLMainReport.rdlc")
        End If
        Return New CLPreparedNextUiReport With {
            .DataSources = sources,
            .ReportTemplate = template
        }
    End Function

    Private Shared Sub AddScenarioRows(
        input As CLOperatingScenarioInput,
        calculation As CLBalancedScenarioCalculation,
        scenarioCode As String,
        allWorking As DataTable,
        allTemperature As DataTable,
        allExchanger As DataTable,
        scenarioWorking As DataTable,
        scenarioTemperature As DataTable,
        scenarioExchanger As DataTable)

        If calculation Is Nothing OrElse calculation.Result Is Nothing Then Return
        Dim scenarioName = If(scenarioCode = "Summer",
            L("MainForm_Summer", "Summer"), L("MainForm_Winter", "Winter"))
        Dim result = calculation.Result
        Dim branch = result.SupplyBranch
        Dim thermo = result.Thermodynamics
        Dim workingValues As Object() = {
            "Title", scenarioName & " - " & L("PDF_WorkingPoint", "Working point"),
            "AirFlow_Caption", L("MainForm_AirFlow", "Air flow [m3/h]"),
            "AirFlow_Value", F(branch.AirflowM3h, 0),
            "MaxPressure_Caption", L("MainForm_MaxPressure", "Max. pressure [Pa]"),
            "MaxPressure_Value", F(branch.AvailablePressurePa, 0),
            "PowerInput_Caption", L("MainForm_PowerInput", "Power input [W]"),
            "PowerInput_Value", F(branch.AbsorbedPowerW, 0),
            "SFP_Caption", "SFP [kW/(m3/s)]",
            "SFP_Value", F(result.CombinedSpecificFanPowerWPerM3hPerSecond, 2),
            "SEL_Caption", "SEL [J/m3]",
            "SEL_Value", F(branch.SpecificFanPowerWPerM3hPerSecond, 0),
            "RegLev_Caption", L("MainForm_RegulationLevel", "Regulation level"),
            "RegLev_Value", F(input.RegulationPercent, 0) & " %",
            "RegLev_Note", String.Empty}
        AddRow(allWorking, workingValues)
        AddRow(scenarioWorking, workingValues)

        Dim temperatureValues As Object() = {
            "Title", scenarioName & " - " & L("MainForm_TemperatureConditionsAndUmidity", "Temperature and humidity conditions"),
            "FreshInletTemp_Caption", L("MainForm_FreshInletTemperature", "Fresh air temperature [C]"),
            "FreshInletTemp_Value", F(input.OutdoorTemperatureC, 1),
            "FreshInletTemp_RH_Caption", L("MainForm_RHFreshInlet", "R.H. [%]"),
            "FreshInletTemp_RH_Value", F(input.OutdoorRelativeHumidityPercent, 0),
            "ReturnInletTemp_Caption", L("MainForm_ReturnInletTemperature", "Return air temperature [C]"),
            "ReturnInletTemp_Value", F(input.ReturnTemperatureC, 1),
            "ReturnInletTemp_RH_Caption", L("MainForm_RHReturnInlet", "R.H. [%]"),
            "ReturnInletTemp_RH_Value", F(input.ReturnRelativeHumidityPercent, 0),
            "SupplyOutletTemp_Caption", L("MainForm_SupplyOutletTemperature", "Supply outlet temperature [C]"),
            "SupplyOutletTemp_Value", F(thermo.SupplyOutletTemperatureC, 1),
            "SupplyOutletTemp_RH_Caption", L("MainForm_SupplyOutletRH", "R.H. [%]"),
            "SupplyOutletTemp_RH_Value", F(thermo.SupplyOutletRelativeHumidityPercent, 0),
            "ExhaustOutletTemp_Caption", L("MainForm_ExhaustOutletTemperature", "Exhaust outlet temperature [C]"),
            "ExhaustOutletTemp_Value", F(thermo.ExhaustOutletTemperatureC, 1),
            "ExhaustOutletTemp_RH_Caption", L("MainForm_ExhaustOutletRH", "R.H. [%]"),
            "ExhaustOutletTemp_RH_Value", F(thermo.ExhaustOutletRelativeHumidityPercent, 0)}
        AddRow(allTemperature, temperatureValues)
        AddRow(scenarioTemperature, temperatureValues)

        Dim exchangerValues As Object() = {
            "Title", scenarioName & " - " & L("MainForm_HeatExchangerPerformances", "Heat exchanger performance"),
            "HeatTransferred_Caption", L("MainForm_HeatTransferred", "Heat transferred [W]"),
            "HeatTransferred_Value", F(thermo.HeatTransferredW, 0),
            "SensibleHeat_Caption", L("MainForm_SensibleHeat", "Sensible heat [W]"),
            "SensibleHeat_Value", F(thermo.SensibleHeatW, 0),
            "LatentHeat_Caption", L("MainForm_LatentHeat", "Latent heat [W]"),
            "LatentHeat_Value", F(thermo.LatentHeatW, 0),
            "Efficiency_Caption", L("MainForm_Efficiency", "Efficiency [%]"),
            "Efficiency_Value", F(thermo.EfficiencyPercent, 0),
            "WaterProduced_Caption", L("MainForm_WaterProduced", "Water produced [l/h]"),
            "WaterProduced_Value", F(thermo.CondensateLitersPerHour, 2)}
        AddRow(allExchanger, exchangerValues)
        AddRow(scenarioExchanger, exchangerValues)
    End Sub

    Private Shared Function WorkingPointTable(name As String) As DataTable
        Return CreateTable(name, "Title", "AirFlow_Caption", "AirFlow_Value",
            "MaxPressure_Caption", "MaxPressure_Value", "PowerInput_Caption",
            "PowerInput_Value", "SFP_Caption", "SFP_Value", "SEL_Caption",
            "SEL_Value", "RegLev_Caption", "RegLev_Value", "RegLev_Note")
    End Function

    Private Shared Function TemperatureTable(name As String) As DataTable
        Return CreateTable(name, "Title", "FreshInletTemp_Caption", "FreshInletTemp_Value",
            "FreshInletTemp_RH_Caption", "FreshInletTemp_RH_Value",
            "ReturnInletTemp_Caption", "ReturnInletTemp_Value",
            "ReturnInletTemp_RH_Caption", "ReturnInletTemp_RH_Value",
            "SupplyOutletTemp_Caption", "SupplyOutletTemp_Value",
            "SupplyOutletTemp_RH_Caption", "SupplyOutletTemp_RH_Value",
            "ExhaustOutletTemp_Caption", "ExhaustOutletTemp_Value",
            "ExhaustOutletTemp_RH_Caption", "ExhaustOutletTemp_RH_Value")
    End Function

    Private Shared Function ExchangerTable(name As String) As DataTable
        Return CreateTable(name, "Title", "HeatTransferred_Caption",
            "HeatTransferred_Value", "SensibleHeat_Caption", "SensibleHeat_Value",
            "LatentHeat_Caption", "LatentHeat_Value", "Efficiency_Caption",
            "Efficiency_Value", "WaterProduced_Caption", "WaterProduced_Value")
    End Function

    Private Shared Function WaterTable(name As String) As DataTable
        Return CreateTable(name, "Visible", "Title", "Summary",
            "CustomDisclaimerAccepted", "ScenarioKey", "ScenarioCaption", "Scenario",
            "CoilCaption", "Coil", "CaseCaption", "Case", "FluidCaption", "Fluid",
            "FluidTemperatureCaption", "FluidTemperature", "GeometryCaption", "Geometry",
            "ModeCaption", "Mode", "StatusCaption", "Status", "CapacityCaption",
            "Capacity", "SensibleCaption", "Sensible", "AirOutCaption", "AirOut",
            "RHOutCaption", "RHOut", "CondCaption", "Cond", "AirDPCaption", "AirDP",
            "WaterDPCaption", "WaterDP", "FluidFlowCaption", "FluidFlow",
            "FluidSpeedCaption", "FluidSpeed", "FaceVelocityCaption", "FaceVelocity",
            "FluidInCaption", "FluidIn", "FluidOutCaption", "FluidOut",
            "GeometryTypeCaption", "GeometryType", "LengthCaption", "LengthValue",
            "HeightCaption", "HeightValue", "RowsCaption", "RowsValue",
            "FinSpacingCaption", "FinSpacingValue", "CircuitsCaption", "CircuitsValue")
    End Function

    Private Shared Sub FillWaterCoils(document As CLSelectionProjectDocument,
        calculation As CLNextUiCalculationResult, allRows As DataTable,
        winterRows As DataTable, summerRows As DataTable)

        If Not document.Selection.WaterCoil.Enabled Then Return
        For Each result In calculation.WaterCoilResults
            Dim target = If(String.Equals(result.ScenarioCode, "Summer",
                StringComparison.OrdinalIgnoreCase), summerRows, winterRows)
            Dim fluidIn = If(String.Equals(result.Mode, "CWD", StringComparison.OrdinalIgnoreCase),
                document.Selection.WaterCoil.CoolingWaterInletTemperatureC,
                document.Selection.WaterCoil.HeatingWaterInletTemperatureC)
            Dim fluidOut = If(String.Equals(result.Mode, "CWD", StringComparison.OrdinalIgnoreCase),
                document.Selection.WaterCoil.CoolingWaterOutletTemperatureC,
                document.Selection.WaterCoil.HeatingWaterOutletTemperatureC)
            Dim values As Object() = {
                "Visible", True,
                "Title", L("MainForm_WaterCoils", "Water coils"),
                "Summary", String.Empty,
                "CustomDisclaimerAccepted", document.Selection.WaterCoil.CustomDesignDisclaimerAccepted,
                "ScenarioKey", result.ScenarioCode,
                "ScenarioCaption", L("MainForm_Scenario", "Scenario"),
                "Scenario", result.ScenarioCode,
                "CoilCaption", L("MainForm_Coil", "Coil"),
                "Coil", document.Selection.WaterCoil.Coil.Name,
                "CaseCaption", L("MainForm_Case", "Case"),
                "Case", document.Selection.WaterCoil.SelectionCase,
                "FluidCaption", L("MainForm_Fluid", "Fluid"),
                "Fluid", document.Selection.WaterCoil.Fluid.Code,
                "FluidTemperatureCaption", L("MainForm_FluidTemperature", "Fluid temperatures"),
                "FluidTemperature", F(fluidIn, 1) & " / " & F(fluidOut, 1),
                "GeometryCaption", L("MainForm_Geometry", "Geometry"),
                "Geometry", document.Selection.WaterCoil.Geometry.GeometryCode,
                "ModeCaption", L("MainForm_Mode", "Mode"), "Mode", result.Mode,
                "StatusCaption", L("MainForm_Status", "Status"), "Status", result.StatusCode,
                "CapacityCaption", L("MainForm_Capacity", "Capacity [W]"), "Capacity", F(result.CapacityW, 0),
                "SensibleCaption", L("MainForm_SensibleHeat", "Sensible [W]"), "Sensible", F(result.SensibleCapacityW, 0),
                "AirOutCaption", L("MainForm_MaxAirOutletTemperature", "Max air out [C]"), "AirOut", F(result.AirOutletTemperatureC, 1),
                "RHOutCaption", L("MainForm_RHOut", "R.H. out [%]"), "RHOut", F(result.AirOutletRelativeHumidityPercent, 0),
                "CondCaption", L("MainForm_Condensate", "Cond. [l/h]"), "Cond", F(result.CondensateLitersPerHour, 2),
                "AirDPCaption", L("MainForm_AirPressureDrop", "Air DP [Pa]"), "AirDP", F(result.AirPressureDropPa, 0),
                "WaterDPCaption", L("MainForm_WaterPressureDrop", "Fluid DP [kPa]"), "WaterDP", F(result.FluidPressureDropKPa, 1),
                "FluidFlowCaption", L("MainForm_FluidFlow", "Fluid flow [l/h]"), "FluidFlow", F(result.FluidFlowLitersPerHour, 0),
                "FluidSpeedCaption", L("MainForm_FluidSpeed", "Fluid speed [m/s]"), "FluidSpeed", F(result.FluidVelocityMetersPerSecond, 2),
                "FaceVelocityCaption", L("MainForm_FaceVelocity", "Face velocity [m/s]"), "FaceVelocity", F(result.FaceVelocityMetersPerSecond, 2),
                "FluidInCaption", L("MainForm_FluidIn", "Fluid in [C]"), "FluidIn", F(fluidIn, 1),
                "FluidOutCaption", L("MainForm_FluidOut", "Fluid out [C]"), "FluidOut", F(fluidOut, 1),
                "GeometryTypeCaption", L("MainForm_Geometry", "Geometry"), "GeometryType", document.Selection.WaterCoil.SelectionCase,
                "LengthCaption", "L [mm]", "LengthValue", F(document.Selection.WaterCoil.Geometry.LengthMm, 0),
                "HeightCaption", "H [mm]", "HeightValue", F(document.Selection.WaterCoil.Geometry.HeightMm, 0),
                "RowsCaption", L("MainForm_Rows", "Rows"), "RowsValue", F(document.Selection.WaterCoil.Geometry.NumberOfRows, 0),
                "FinSpacingCaption", L("MainForm_FinSpacing", "Fin spacing [mm]"), "FinSpacingValue", F(document.Selection.WaterCoil.Geometry.FinSpacingMm, 1),
                "CircuitsCaption", L("MainForm_Circuits", "Circuits"), "CircuitsValue", F(document.Selection.WaterCoil.Geometry.NumberOfCircuits, 0)}
            AddRow(allRows, values)
            AddRow(target, values)
        Next
    End Sub

    Private Shared Function ElectricTable(name As String) As DataTable
        Return CreateTable(name, "Visible", "ScenarioKey", "Title", "ModeCaption",
            "Mode", "CaseCaption", "Case", "InstallationCaption", "Installation",
            "HeaterCaption", "Heater", "CodeCaption", "Code", "ManagementCodeCaption",
            "ManagementCode", "PowerCaption", "Power", "VoltageCaption", "Voltage",
            "PhasesCaption", "Phases", "FrequencyCaption", "Frequency", "CurrentCaption",
            "Current", "StagesCaption", "Stages", "QuantityCaption", "Quantity",
            "AirInCaption", "AirIn", "AirOutCaption", "AirOut", "AirDPCaption",
            "AirDP", "NominalAirDPCaption", "NominalAirDP", "ExhaustOutCaption",
            "ExhaustOut", "FrostStatusCaption", "FrostStatus",
            "CustomDisclaimerAccepted", "Summary")
    End Function

    Private Shared Sub FillElectricHeaters(document As CLSelectionProjectDocument,
        calculation As CLNextUiCalculationResult, allRows As DataTable,
        ehdRows As DataTable, pehdRows As DataTable)

        For Each result In calculation.ElectricHeaterResults
            Dim selection = If(String.Equals(result.Mode, "PEHD",
                StringComparison.OrdinalIgnoreCase),
                document.Selection.ElectricHeater.PEHD,
                document.Selection.ElectricHeater.EHD)
            Dim target = If(String.Equals(result.Mode, "PEHD",
                StringComparison.OrdinalIgnoreCase), pehdRows, ehdRows)
            Dim definition = calculation.AvailableElectricHeaters.
                FirstOrDefault(Function(item) item.Id = selection.Heater.Id.GetValueOrDefault())
            Dim values As Object() = {
                "Visible", True, "ScenarioKey", result.ScenarioCode,
                "Title", L("MainForm_ElectricHeaters", "Electric heaters"),
                "ModeCaption", L("MainForm_Mode", "Mode"), "Mode", result.Mode,
                "CaseCaption", L("MainForm_Case", "Case"), "Case", selection.SelectionCase,
                "InstallationCaption", L("MainForm_Installation", "Installation"), "Installation", selection.InstallationType,
                "HeaterCaption", L("MainForm_ElectricHeater", "Electric heater"), "Heater", selection.Heater.Name,
                "CodeCaption", L("MainForm_Code", "Code"), "Code", selection.Heater.Code,
                "ManagementCodeCaption", L("MainForm_ManagementCode", "Management code"), "ManagementCode", selection.Heater.ManagementCode,
                "PowerCaption", L("MainForm_Power", "Power [W]"), "Power", F(result.PowerW, 0),
                "VoltageCaption", L("MainForm_Voltage", "Voltage [V]"), "Voltage", If(definition Is Nothing, String.Empty, F(definition.VoltageV, 0)),
                "PhasesCaption", L("MainForm_Phases", "Phases"), "Phases", If(definition Is Nothing, String.Empty, definition.PhaseCount.ToString()),
                "FrequencyCaption", L("MainForm_Frequency", "Frequency [Hz]"), "Frequency", String.Empty,
                "CurrentCaption", L("MainForm_Current", "Current [A]"), "Current", F(result.CurrentA, 2),
                "StagesCaption", L("MainForm_Stages", "Stages"), "Stages", F(document.Selection.ElectricHeater.Stages, 0),
                "QuantityCaption", L("MainForm_Quantity", "Quantity"), "Quantity", If(definition Is Nothing, "1", definition.Quantity.ToString()),
                "AirInCaption", L("MainForm_AirIn", "Air in [C]"), "AirIn", F(result.AirInletTemperatureC, 1),
                "AirOutCaption", L("MainForm_MaxAirOutletTemperature", "Max air out [C]"), "AirOut", F(result.AirOutletTemperatureC, 1),
                "AirDPCaption", L("MainForm_AirPressureDrop", "Air DP [Pa]"), "AirDP", F(result.AirPressureDropPa, 1),
                "NominalAirDPCaption", L("MainForm_NominalAirPressureDrop", "Nominal air DP [Pa]"), "NominalAirDP", String.Empty,
                "ExhaustOutCaption", L("MainForm_ExhaustOutletTemperature", "Exhaust out [C]"), "ExhaustOut", F(result.ExhaustOutletTemperatureC, 1),
                "FrostStatusCaption", String.Empty, "FrostStatus", String.Empty,
                "CustomDisclaimerAccepted", selection.CustomDesignDisclaimerAccepted,
                "Summary", String.Empty}
            AddRow(allRows, values)
            AddRow(target, values)
        Next
    End Sub

    Private Shared Sub FillAccessories(document As CLSelectionProjectDocument,
        table As DataTable)
        For Each accessory In If(document.Selection.Accessories,
            New List(Of CLAccessorySelection)())

            Dim code = accessory.Code
            If accessory.Quantity > 1 Then
                code &= " x" & accessory.Quantity.ToString(CultureInfo.CurrentCulture)
            End If
            Dim description = If(
                String.IsNullOrWhiteSpace(accessory.LocalizedDescription),
                accessory.LocalizedDisplayName,
                accessory.LocalizedDescription)
            If String.IsNullOrWhiteSpace(description) Then description = accessory.Code
            AddAccessoryRow(table, code, description,
                accessory.LocalizedFunctionNames,
                String.Equals(accessory.Availability, "Standard",
                    StringComparison.OrdinalIgnoreCase),
                accessory.InstallationType)
        Next

        Dim water = document.Selection.WaterCoil
        If water IsNot Nothing AndAlso water.Enabled Then
            Dim mode = If(String.IsNullOrWhiteSpace(water.CalculationMode),
                "HCD", water.CalculationMode).ToUpperInvariant()
            AddAccessoryRow(table, mode, ThermodynamicAccessoryDescription(mode),
                Nothing, False, water.InstallationType)
        End If

        Dim electric = document.Selection.ElectricHeater
        If electric IsNot Nothing Then
            For Each heater In New CLElectricHeaterModeSelection() {
                electric.PEHD, electric.EHD}
                If heater Is Nothing OrElse Not heater.Enabled Then Continue For
                Dim mode = If(String.IsNullOrWhiteSpace(heater.Mode),
                    "-", heater.Mode).ToUpperInvariant()
                AddAccessoryRow(table, mode, ThermodynamicAccessoryDescription(mode),
                    Nothing, False, heater.InstallationType)
            Next
        End If
    End Sub

    Private Shared Sub AddAccessoryRow(table As DataTable,
        code As String,
        description As String,
        functionNames As IEnumerable(Of String),
        isStandard As Boolean,
        installationType As String)

        Dim installation = AccessoryInstallationText(installationType)
        Dim statusSymbol = If(isStandard, ChrW(&H25CF),
            If(String.Equals(installationType, "Internal",
                StringComparison.OrdinalIgnoreCase), ChrW(&H2666), ChrW(&H25A0)))
        Dim statusText = If(isStandard,
            L("MainForm_Accessories_Standard", "Standard"), installation)
        Dim functions = If(functionNames, Enumerable.Empty(Of String)()).
            Where(Function(value) Not String.IsNullOrWhiteSpace(value)).ToList()

        AddRow(table,
            "Title", L("MainForm_Accessories_Tab", "Accessories and functions"),
            "CodeCaption", L("MainForm_Accessories_Code", "Code"),
            "DescriptionCaption", L("MainForm_Accessories_Description", "Description"),
            "FunctionsCaption", L("MainForm_Accessories_Functions", "Functions"),
            "StatusCaption", L("MainForm_Accessories_Status", "Status"),
            "Code", code,
            "Description", description,
            "Functions", If(functions.Count = 0, "-",
                String.Join(Environment.NewLine, functions)),
            "Status", String.Format(CultureInfo.CurrentCulture,
                "{0} {1}", statusSymbol, statusText))
    End Sub

    Private Shared Function AccessoryInstallationText(value As String) As String
        If String.Equals(value, "Internal", StringComparison.OrdinalIgnoreCase) Then
            Return L("MainForm_CoilPerformance_Internal", "Internal")
        End If
        If String.Equals(value, "RequestedInternal",
            StringComparison.OrdinalIgnoreCase) Then
            Return L("MainForm_CoilPerformance_RequestInternal", "Request internal")
        End If
        Return L("MainForm_CoilPerformance_ExternalInstallation", "External")
    End Function

    Private Shared Function ThermodynamicAccessoryDescription(mode As String) As String
        Select Case If(mode, String.Empty).Trim().ToUpperInvariant()
            Case "CWD"
                Return L("MainForm_Accessories_CWDDescription",
                    "Chilled-water cooling coil")
            Case "HWD"
                Return L("MainForm_Accessories_HWDDescription",
                    "Hot-water heating coil")
            Case "HCD"
                Return L("MainForm_Accessories_HCDDescription",
                    "Two-pipe water coil for heating and cooling")
            Case "EHD"
                Return L("MainForm_Accessories_EHDDescription",
                    "Electric post-heater")
            Case "PEHD"
                Return L("MainForm_Accessories_PEHDDescription",
                    "Electric pre-heater")
            Case Else
                Return mode
        End Select
    End Function

    Private Shared Function BuildPressureChart(result As CLNextUiCalculationResult) As Byte()
        Dim curves = result.Winter.Curves
        Return BuildSingleChart(L("MainForm_Pressure", "Pressure [Pa]"),
            curves.OriginalAirflows, curves.OriginalPressures,
            curves.RegulatedAirflows, curves.RegulatedPressures,
            curves.WorkingPointAirflow, curves.WorkingPointPressurePa,
            Color.FromArgb(59, 130, 246), Color.FromArgb(245, 158, 11), 0)
    End Function

    Private Shared Function BuildPowerChart(result As CLNextUiCalculationResult) As Byte()
        Dim curves = result.Winter.Curves
        Return BuildSingleChart(L("MainForm_Power", "Power [W]"),
            Nothing, Nothing, curves.RegulatedAirflows, curves.RegulatedPowers,
            curves.WorkingPointAirflow, curves.WorkingPointPowerW,
            Color.Transparent, Color.FromArgb(245, 158, 11), 0)
    End Function

    Private Shared Function BuildEfficiencyChart(result As CLNextUiCalculationResult,
        summerEnabled As Boolean) As Byte()
        Using chart As New Chart()
            chart.Size = New Size(ChartWidth, ChartHeight)
            chart.BackColor = Color.White
            AddEfficiencyArea(chart, "Winter", New ElementPosition(13, 5, 82, If(summerEnabled, 38, 80)),
                result.Winter.Curves, Color.FromArgb(59, 130, 246))
            If summerEnabled AndAlso result.Summer IsNot Nothing Then
                AddEfficiencyArea(chart, "Summer", New ElementPosition(13, 54, 82, 38),
                    result.Summer.Curves, Color.FromArgb(34, 139, 85))
            End If
            Return ChartBytes(chart)
        End Using
    End Function

    Private Shared Function BuildCo2Chart(result As CLNextUiCo2Result) As Byte()
        If result Is Nothing OrElse result.Points Is Nothing OrElse
            result.Points.Count = 0 Then Return EmptyImage()
        Using chart As New Chart()
            chart.Size = New Size(ChartWidth, ChartHeight)
            chart.BackColor = Color.White
            Dim area As New ChartArea("CO2")
            area.Position = New ElementPosition(12, 5, 84, 84)
            ConfigureArea(area, "CO2 [ppm]", 0)
            area.AxisX.Title = L("CO2Level_PeriodOfTime", "Time [h]")
            area.AxisX.Maximum = 5
            area.AxisY.Maximum = Math.Max(2000,
                Math.Ceiling(result.Points.Max(Function(point) point.Ppm) / 100) * 100)
            chart.ChartAreas.Add(area)
            AddSeries(chart, "CO2", "CO2",
                result.Points.Select(Function(point) point.Hours).ToArray(),
                result.Points.Select(Function(point) point.Ppm).ToArray(),
                Color.FromArgb(59, 130, 246))
            Return ChartBytes(chart)
        End Using
    End Function

    Private Shared Sub AddEfficiencyArea(chart As Chart, name As String,
        position As ElementPosition, curves As CLPerformanceCurveCalculation,
        color As Color)
        Dim area As New ChartArea(name)
        area.Position = position
        ConfigureArea(area, L("MainForm_Efficiency", "Efficiency [%]"), 60)
        chart.ChartAreas.Add(area)
        AddSeries(chart, name & "Curve", name, curves.RegulatedAirflows,
            curves.EfficienciesPercent, color)
        AddPoint(chart, name & "Point", name, curves.WorkingPointAirflow,
            curves.WorkingPointEfficiencyPercent)
    End Sub

    Private Shared Function BuildSingleChart(yTitle As String,
        x1 As Double(), y1 As Double(), x2 As Double(), y2 As Double(),
        pointX As Double, pointY As Double, color1 As Color, color2 As Color,
        yMinimum As Double) As Byte()
        Using chart As New Chart()
            chart.Size = New Size(ChartWidth, ChartHeight)
            chart.BackColor = Color.White
            Dim area As New ChartArea("Main")
            area.Position = New ElementPosition(12, 5, 84, 84)
            ConfigureArea(area, yTitle, yMinimum)
            chart.ChartAreas.Add(area)
            If x1 IsNot Nothing Then AddSeries(chart, "Original", "Main", x1, y1, color1)
            AddSeries(chart, "Regulated", "Main", x2, y2, color2)
            AddPoint(chart, "WorkingPoint", "Main", pointX, pointY)
            Return ChartBytes(chart)
        End Using
    End Function

    Private Shared Sub ConfigureArea(area As ChartArea, yTitle As String,
        yMinimum As Double)
        area.BackColor = Color.White
        area.AxisX.Title = L("MainForm_AirFlow", "Air flow [m3/h]")
        area.AxisY.Title = yTitle
        area.AxisY.Minimum = yMinimum
        area.AxisX.Minimum = 0
        area.AxisX.MajorGrid.LineColor = Color.FromArgb(222, 226, 232)
        area.AxisY.MajorGrid.LineColor = Color.FromArgb(222, 226, 232)
        area.AxisX.LineColor = Color.FromArgb(148, 163, 184)
        area.AxisY.LineColor = Color.FromArgb(148, 163, 184)
        area.AxisX.LabelStyle.Font = New Font("Segoe UI", 10)
        area.AxisY.LabelStyle.Font = New Font("Segoe UI", 10)
        area.AxisX.TitleFont = New Font("Segoe UI", 11)
        area.AxisY.TitleFont = New Font("Segoe UI", 11)
    End Sub

    Private Shared Sub AddSeries(chart As Chart, name As String,
        area As String, x As Double(), y As Double(), color As Color)
        If x Is Nothing OrElse y Is Nothing Then Return
        Dim series As New Series(name) With {
            .ChartArea = area, .ChartType = SeriesChartType.Line,
            .BorderWidth = 3, .Color = color}
        For index = 0 To Math.Min(x.Length, y.Length) - 1
            If Not Double.IsNaN(x(index)) AndAlso Not Double.IsNaN(y(index)) Then
                series.Points.AddXY(x(index), y(index))
            End If
        Next
        chart.Series.Add(series)
    End Sub

    Private Shared Sub AddPoint(chart As Chart, name As String,
        area As String, x As Double, y As Double)
        Dim series As New Series(name) With {
            .ChartArea = area, .ChartType = SeriesChartType.Point,
            .MarkerStyle = MarkerStyle.Circle, .MarkerSize = 10,
            .Color = Color.FromArgb(225, 70, 45)}
        series.Points.AddXY(x, y)
        chart.Series.Add(series)
    End Sub

    Private Shared Function BuildLegend(result As CLNextUiCalculationResult,
        summerEnabled As Boolean) As Byte()
        Using bitmap As New Bitmap(ChartWidth, ChartHeight \ 2)
            Using graphics As System.Drawing.Graphics =
                System.Drawing.Graphics.FromImage(bitmap)
                graphics.Clear(Color.White)
                graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                Dim font As New Font("Segoe UI", 16)
                Dim y = 40
                DrawLegendLine(graphics, font, y, Color.FromArgb(59, 130, 246),
                    "100% " & L("MainForm_RegulationLevel", "Regulation level")) : y += 55
                DrawLegendLine(graphics, font, y, Color.FromArgb(245, 158, 11),
                    F(result.EffectiveRegulationPercent, 0) & "% " &
                    L("MainForm_RegulationLevel", "Regulation level")) : y += 55
                Using brush As New SolidBrush(Color.FromArgb(225, 70, 45))
                    graphics.FillEllipse(brush, 40, y + 4, 18, 18)
                End Using
                graphics.DrawString(L("PDF_WorkingPoint", "Working point"), font,
                    Brushes.Black, 80, y) : y += 55
                DrawLegendLine(graphics, font, y, Color.FromArgb(59, 130, 246),
                    L("MainForm_Winter", "Winter") & " " &
                    L("MainForm_Efficiency", "Efficiency")) : y += 55
                If summerEnabled Then
                    DrawLegendLine(graphics, font, y, Color.FromArgb(34, 139, 85),
                        L("MainForm_Summer", "Summer") & " " &
                        L("MainForm_Efficiency", "Efficiency"))
                End If
            End Using
            Return BitmapBytes(bitmap)
        End Using
    End Function

    Private Shared Sub DrawLegendLine(graphics As Graphics, font As Font,
        y As Integer, color As Color, text As String)
        Using pen As New Pen(color, 3)
            graphics.DrawLine(pen, 40, y + 12, 100, y + 12)
        End Using
        graphics.DrawString(text, font, Brushes.Black, 120, y)
    End Sub

    Private Shared Function SoundTable(name As String) As DataTable
        Return CreateTable(name, "Type", "Caption", "LwA_Value", "Hz63_Value",
            "Hz125_Value", "Hz250_Value", "Hz500_Value", "Hz1000_Value",
            "Hz2000_Value", "Hz4000_Value", "Hz8000_Value", "Lp1_Value", "Lp2_Value")
    End Function

    Private Shared Function SoundHeaderTable() As DataTable
        Return CreateTable("SoundPowerHeader", "Title", "Caption", "LwA_Caption",
            "Hz63_Caption", "Hz125_Caption", "Hz250_Caption", "Hz500_Caption",
            "Hz1000_Caption", "Hz2000_Caption", "Hz4000_Caption", "Hz8000_Caption",
            "Lp1Caption", "sndaddtorep_bool", "Lp2Caption")
    End Function

    Private Shared Function Co2RoomTable() As DataTable
        Return CreateTable("CO2LevelRoom", "RoomHeight", "RoomWidth", "RoomLength",
            "Title", "RoomHeight_Caption", "RoomLength_Caption", "RoomWidth_Caption")
    End Function

    Private Shared Function Co2UseTable() As DataTable
        Return CreateTable("CO2LevelUse", "PeriodPresence", "PeoplePresence",
            "PeriodBreak", "Title", "Period_Caption", "People_Caption",
            "Presence_Caption", "PeopleBreak", "Break_Caption", "LevelAct",
            "LevelAct_Caption", "CO2prod", "CO2prod_Caption")
    End Function

    Private Shared Function Co2ParametersTable() As DataTable
        Return CreateTable("CO2LevelParameters", "Title", "CalcMeth_Caption",
            "CalcMeth", "StdPreset_Caption", "StdPreset", "OutCO2", "MaxCO2",
            "AF_demand", "AF_person", "AF_area", "OutCO2_caption", "MaxCO2_caption",
            "AF_demand_caption", "AF_person_caption", "AF_area_caption",
            "AF_area_m3h_caption", "addtorep_bool", "AF_demand_m3h",
            "AF_person_m3h", "AF_area_m3h", "AF_demand_m3h_caption",
            "AF_person_m3h_caption")
    End Function

    Private Shared Sub FillSound(document As CLSelectionProjectDocument,
        calculation As CLNextUiCalculationResult, rows As DataTable,
        header As DataTable)

        Dim selection = If(document.Selection.Sound, New CLSoundSelection())
        Dim include = document.Selection.Report IsNot Nothing AndAlso
            document.Selection.Report.IncludeSoundPower
        AddRow(header,
            "Title", L("SoundPower", "Sound power"),
            "Caption", L("Sound_Total", "LwA [dB(A)]"),
            "LwA_Caption", L("Sound_Total", "LwA [dB(A)]"),
            "Hz63_Caption", "63 Hz", "Hz125_Caption", "125 Hz",
            "Hz250_Caption", "250 Hz", "Hz500_Caption", "500 Hz",
            "Hz1000_Caption", "1000 Hz", "Hz2000_Caption", "2000 Hz",
            "Hz4000_Caption", "4000 Hz", "Hz8000_Caption", "8000 Hz",
            "Lp1Caption", "Lp@" & F(selection.Distance1Meters, 1) &
                "m Q = " & selection.Directivity,
            "Lp2Caption", "Lp@" & F(selection.Distance2Meters, 1) &
                "m Q = " & selection.Directivity,
            "sndaddtorep_bool", Not include)
        If calculation.Sound Is Nothing Then Return
        For Each item In calculation.Sound.Rows
            Dim bands = If(item.Bands, New Double() {})
            AddRow(rows,
                "Type", item.Type, "Caption", item.Caption,
                "LwA_Value", F(item.LwA, 1),
                "Hz63_Value", Band(bands, 0), "Hz125_Value", Band(bands, 1),
                "Hz250_Value", Band(bands, 2), "Hz500_Value", Band(bands, 3),
                "Hz1000_Value", Band(bands, 4), "Hz2000_Value", Band(bands, 5),
                "Hz4000_Value", Band(bands, 6), "Hz8000_Value", Band(bands, 7),
                "Lp1_Value", F(item.Lp1, 1), "Lp2_Value", F(item.Lp2, 1))
        Next
    End Sub

    Private Shared Sub FillCo2(document As CLSelectionProjectDocument,
        calculation As CLNextUiCalculationResult, room As DataTable,
        usage As DataTable, parameters As DataTable)

        Dim selection = If(document.Selection.Co2, New CLCo2Selection())
        Dim result = calculation.Co2
        Dim include = document.Selection.Report IsNot Nothing AndAlso
            document.Selection.Report.IncludeCo2
        AddRow(room,
            "RoomHeight", F(selection.RoomHeightMeters, 1),
            "RoomWidth", F(selection.RoomWidthMeters, 1),
            "RoomLength", F(selection.RoomLengthMeters, 1),
            "Title", L("CO2Level_Room", "Room"),
            "RoomHeight_Caption", L("CO2Level_Room_Height", "Height [m]"),
            "RoomLength_Caption", L("CO2Level_Room_Length", "Length [m]"),
            "RoomWidth_Caption", L("CO2Level_Room_Witdh", "Width [m]"))
        AddRow(usage,
            "PeriodPresence", F(selection.PresenceMinutes, 0),
            "PeoplePresence", F(selection.PeopleDuringPresence, 0),
            "PeriodBreak", F(selection.BreakMinutes, 0),
            "PeopleBreak", F(selection.PeopleDuringBreak, 0),
            "Title", L("CO2Level_Usage", "Use"),
            "Period_Caption", L("CO2Level_Usage_Period", "Period [min]"),
            "People_Caption", L("CO2Level_Usage_People", "People"),
            "Presence_Caption", L("CO2Level_Usage_Presence", "Presence"),
            "Break_Caption", L("CO2Level_Usage_Break", "Break"),
            "LevelAct", F(selection.ActivityMet, 1),
            "LevelAct_Caption", L("CO2Level_Usage_LevelAct", "Activity [met]"),
            "CO2prod", F(If(result?.Co2ProductionPerPersonLitersPerHour, 0), 1),
            "CO2prod_Caption", L("CO2Level_Usage_CO2prod", "CO2 production"))
        Dim personAndArea = String.Equals(selection.CalculationMethod,
            "PersonAndArea", StringComparison.OrdinalIgnoreCase)
        AddRow(parameters,
            "Title", L("CO2Level_Parameters", "Parameters"),
            "CalcMeth_Caption", L("CO2Level_Parameters_CalcMet", "Calculation method"),
            "CalcMeth", Co2MethodCaption(selection.CalculationMethod),
            "StdPreset_Caption", L("CO2Level_Parameters_StandardPreset", "Standard preset"),
            "StdPreset", selection.StandardPreset,
            "OutCO2", F(selection.OutdoorCo2Ppm, 0),
            "MaxCO2", F(If(result?.MaximumCo2Ppm, selection.MaximumCo2Ppm), 0),
            "AF_demand", F(If(result?.RequiredAirflowLitersPerSecond, 0), 2),
            "AF_demand_m3h", F(If(result?.RequiredAirflowM3h, 0), 1),
            "AF_person", If(personAndArea,
                F(selection.AirflowPerPersonLitersPerSecond, 1), "-"),
            "AF_area", If(personAndArea,
                F(selection.AirflowPerAreaLitersPerSecondM2, 2), "-"),
            "AF_person_m3h", If(personAndArea,
                F(selection.AirflowPerPersonLitersPerSecond * 3.6, 1), "-"),
            "AF_area_m3h", If(personAndArea,
                F(selection.AirflowPerAreaLitersPerSecondM2 * 3.6, 2), "-"),
            "OutCO2_caption", L("CO2Level_Parameters_ExtCO2", "Outdoor CO2 [ppm]"),
            "MaxCO2_caption", L("CO2Level_Parameters_MaxCO2", "Maximum CO2 [ppm]"),
            "AF_demand_caption", L("CO2Level_Parameters_Af_demand", "Airflow [l/s]"),
            "AF_demand_m3h_caption", L("CO2Level_Parameters_Af_demand", "Airflow") & " [m3/h]",
            "AF_person_caption", L("CO2Level_Parameters_Af_person", "Airflow per person"),
            "AF_area_caption", L("CO2Level_Parameters_Af_area", "Airflow per area"),
            "AF_person_m3h_caption", L("CO2Level_Parameters_Af_person", "Airflow per person") & " [m3/h]",
            "AF_area_m3h_caption", L("CO2Level_Parameters_Af_area", "Airflow per area") & " [m3/h]",
            "addtorep_bool", Not include)
    End Sub

    Private Shared Function Band(values As Double(), index As Integer) As String
        Return If(index >= 0 AndAlso index < values.Length, F(values(index), 1), String.Empty)
    End Function

    Private Shared Function Co2MethodCaption(method As String) As String
        Select Case If(method, String.Empty).Trim().ToLowerInvariant()
            Case "fixedairflow"
                Return L("CO2Level_Parameters_CalcMet_Fixed_Af", "Fixed airflow")
            Case "personandarea"
                Return L("CO2Level_Parameters_CalcMet_PersonRelated", "Person and area")
            Case Else
                Return L("CO2Level_Parameters_CalcMet_MaxCO2", "Maximum CO2")
        End Select
    End Function

    Private Shared Function CreateTable(name As String,
        ParamArray columns As String()) As DataTable
        Dim table As New DataTable(name)
        For Each column In columns
            If column = "Date_Value" Then
                table.Columns.Add(column, GetType(DateTime))
            ElseIf column.EndsWith("Image", StringComparison.Ordinal) OrElse
                column = "LogoBmp" Then
                table.Columns.Add(column, GetType(Byte()))
            ElseIf column = "Visible" OrElse column.EndsWith("_bool",
                StringComparison.OrdinalIgnoreCase) OrElse
                column = "CustomDisclaimerAccepted" Then
                table.Columns.Add(column, GetType(Boolean))
            Else
                table.Columns.Add(column, GetType(String))
            End If
        Next
        Return table
    End Function

    Private Shared Sub AddRow(table As DataTable, ParamArray values As Object())
        Dim row = table.NewRow()
        For index = 0 To values.Length - 2 Step 2
            Dim column = CStr(values(index))
            If table.Columns.Contains(column) Then
                row(column) = If(values(index + 1), DBNull.Value)
            End If
        Next
        table.Rows.Add(row)
    End Sub

    Private Shared Sub AddSource(sources As List(Of ReportDataSource),
        table As DataTable)
        sources.Add(New ReportDataSource(table.TableName, table))
    End Sub

    Private Shared Function L(key As String, fallback As String) As String
        Try
            Dim value = CLEnvironment.Current.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> key AndAlso
                value.Trim() <> "?" Then Return value
        Catch
        End Try
        Return fallback
    End Function

    Private Shared Function F(value As Double?, decimals As Integer) As String
        If Not value.HasValue Then Return String.Empty
        Return value.Value.ToString("N" & decimals, CultureInfo.CurrentCulture)
    End Function

    Private Shared Function F(value As Integer?, decimals As Integer) As String
        If Not value.HasValue Then Return String.Empty
        Return value.Value.ToString("N" & decimals, CultureInfo.CurrentCulture)
    End Function

    Private Shared Function SelectionReference(document As CLSelectionProjectDocument) As String
        If document.Identity Is Nothing Then Return String.Empty
        Return If(Not String.IsNullOrWhiteSpace(document.Identity.PublicReference),
            document.Identity.PublicReference, document.Identity.LocalDraftReference)
    End Function

    Private Shared Function SelectionRevision(document As CLSelectionProjectDocument) As String
        If document.Identity Is Nothing OrElse Not document.Identity.Revision.HasValue Then Return String.Empty
        Return "R" & document.Identity.Revision.Value.ToString("00", CultureInfo.InvariantCulture)
    End Function

    Private Shared Function SelectionStatus(document As CLSelectionProjectDocument) As String
        If document.Identity IsNot Nothing AndAlso
            Not String.IsNullOrWhiteSpace(document.Identity.PublicReference) Then
            Return L("MainForm_TechnicalSelectionStatus_Registered", "Registered")
        End If
        Return L("MainForm_TechnicalSelectionStatus_Draft", "Draft")
    End Function

    Private Shared Function ImageBytes(image As Image) As Byte()
        If image Is Nothing Then Return EmptyImage()
        Using bitmap As New Bitmap(image)
            Return BitmapBytes(bitmap)
        End Using
    End Function

    Private Shared Function ChartBytes(chart As Chart) As Byte()
        Using stream As New MemoryStream()
            chart.SaveImage(stream, ChartImageFormat.Png)
            Return stream.ToArray()
        End Using
    End Function

    Private Shared Function BitmapBytes(bitmap As Bitmap) As Byte()
        Using stream As New MemoryStream()
            bitmap.Save(stream, ImageFormat.Png)
            Return stream.ToArray()
        End Using
    End Function

    Private Shared Function EmptyImage() As Byte()
        Using bitmap As New Bitmap(2, 2)
            Return BitmapBytes(bitmap)
        End Using
    End Function

End Class
