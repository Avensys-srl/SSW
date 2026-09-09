Imports Climalombarda.DataCentral.LTModel
Imports System.Globalization
Imports System.Linq

Public NotInheritable Class CLNextUiModelSummary
    Public Property Id As Integer
    Public Property Code As String
    Public Property Name As String
    Public Property SeriesCode As String
    Public Property NominalAirflowM3h As Double
    Public Property StaticPressurePa As Double
    Public Property AeraulicConnectionCode As String
End Class

Public NotInheritable Class CLNextUiPreselectionSummary
    Public Property Model As CLNextUiModelSummary
    Public Property RequiredRegulationPercent As Double
    Public Property AvailablePressurePa As Double
    Public Property AbsorbedPowerW As Double
    Public Property CombinedSfp As Double
    Public Property SupplySoundPowerDbA As Double?
    Public Property SupplySoundPressureDbA As Double?
    Public Property BreakoutSoundPowerDbA As Double?
    Public Property BreakoutSoundPressureDbA As Double?
End Class

Public NotInheritable Class CLNextUiAccessorySummary
    Public Property Code As String
    Public Property ItemType As String
    Public Property Name As String
    Public Property Description As String
    Public Property Category As String
    Public Property Availability As String
    Public Property Installation As String
    Public Property FunctionNames As New List(Of String)()
    Public Property Included As Boolean
    Public Property Locked As Boolean
    Public Property Enabled As Boolean = True
    Public Property DisabledReason As String
End Class

Public NotInheritable Class CLNextUiWaterCoilSummary
    Public Property Id As Integer
    Public Property Name As String
    Public Property Mode As String
    Public Property Installation As String
    Public Property InstallationLabel As String
    Public Property LengthMm As Integer
    Public Property HeightMm As Integer
    Public Property Rows As Integer
    Public Property Circuits As Integer
    Public Property FinSpacingMm As Double
End Class

Public NotInheritable Class CLNextUiElectricHeaterSummary
    Public Property Id As Integer
    Public Property Code As String
    Public Property Name As String
    Public Property Mode As String
    Public Property Installation As String
    Public Property PowerW As Double
    Public Property VoltageV As Double
    Public Property CurrentA As Double
    Public Property PhaseCount As Integer
    Public Property Quantity As Integer
    Public Property IsDefault As Boolean
End Class

Public NotInheritable Class CLNextUiCalculationInput
    Public Property ProjectName As String
    Public Property CustomerReference As String
    Public Property LanguageCode As String = "it"
    Public Property ModelCode As String
    Public Property SupplyAirflowM3h As Double
    Public Property ExtractAirflowM3h As Double
    Public Property ImbalanceEnabled As Boolean
    Public Property PressurePa As Double
    Public Property RegulationPercent As Double = 100
    Public Property SummerEnabled As Boolean = True
    Public Property WinterOutdoorTemperatureC As Double = -10
    Public Property WinterOutdoorRhPercent As Double = 80
    Public Property WinterReturnTemperatureC As Double = 20
    Public Property WinterReturnRhPercent As Double = 60
    Public Property SummerOutdoorTemperatureC As Double = 32
    Public Property SummerOutdoorRhPercent As Double = 80
    Public Property SummerReturnTemperatureC As Double = 26
    Public Property SummerReturnRhPercent As Double = 50
    Public Property WaterCoilEnabled As Boolean
    Public Property WaterCoilId As Integer
    Public Property WaterCoilMode As String = "HCD"
    Public Property WaterCoilCustomized As Boolean
    Public Property WaterCoilCustomDisclaimerAccepted As Boolean
    Public Property WaterCoilLengthMm As Integer
    Public Property WaterCoilHeightMm As Integer
    Public Property WaterCoilRows As Integer
    Public Property WaterCoilCircuits As Integer
    Public Property WaterCoilFinSpacingMm As Double
    Public Property InstallationMode As String = "Ceiling"
    Public Property LayoutCode As String
    Public Property FluidCode As String = "Water"
    Public Property GlycolPercent As Double = 10
    Public Property CoolingWaterInletTemperatureC As Double = 7
    Public Property CoolingWaterOutletTemperatureC As Double = 12
    Public Property HeatingWaterInletTemperatureC As Double = 80
    Public Property HeatingWaterOutletTemperatureC As Double = 70
    Public Property ElectricPreheaterEnabled As Boolean
    Public Property ElectricPreheaterId As Integer
    Public Property ElectricPostheaterEnabled As Boolean
    Public Property ElectricPostheaterId As Integer
    Public Property AccessoryCodes As New List(Of String)()
    Public Property Sound As New CLNextUiSoundInput()
    Public Property PreselectionFilters As New CLNextUiPreselectionFilters()
    Public Property Co2 As New CLNextUiCo2Input()
End Class

Public NotInheritable Class CLNextUiCalculationResult
    Public Property Model As CLNextUiModelSummary
    Public Property Winter As CLBalancedScenarioCalculation
    Public Property Summer As CLBalancedScenarioCalculation
    Public Property Layout As CLInstallationLayoutSnapshot
    Public Property Accessories As New List(Of CLNextUiAccessorySummary)()
    Public Property AvailableWaterCoils As New List(Of CLNextUiWaterCoilSummary)()
    Public Property AvailableElectricHeaters As New List(Of CLNextUiElectricHeaterSummary)()
    Public Property WaterCoilResults As New List(Of CLWaterCoilResult)()
    Public Property ElectricHeaterResults As New List(Of CLElectricHeaterResult)()
    Public Property Sound As CLNextUiSoundResult
    Public Property Co2 As CLNextUiCo2Result
    Public Property AdditionalPressureDropPa As Double
    Public Property EffectiveRegulationPercent As Double
    Public Property PressureCapacityExceeded As Boolean
    Public Property PressureCapacityExceededMessage As String
    Public Property WaterHeatingEnabled As Boolean = True
    Public Property WaterHeatingDisabledReason As String
    Public Property ElectricPostheaterEnabled As Boolean = True
    Public Property ElectricPostheaterDisabledReason As String
    Public Property WaterCoilStandardLabel As String
    Public Property WaterCoilCustomizedLabel As String
    Public Property WaterCoilCustomDisclaimer As String
    Public Property WaterCoilDimensionsNotice As String
    Public Property WaterCoilQuotationNotice As String
    Public Property Validation As New CLValidationResult()
End Class

Public NotInheritable Class CLNextUiApplicationService

    Public Shared Sub ApplyLanguage(languageCode As String)
        EnsureLanguage(languageCode)
    End Sub
    Private Sub New()
    End Sub

    Public Shared Function GetModels() As List(Of CLNextUiModelSummary)
        Return CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            OrderBy(Function(model) model.CLSerie.Code).
            ThenBy(Function(model) model.Code).
            ToList().
            Select(Function(model) MapModel(model)).
            ToList()
    End Function

    Public Shared Function Preselect(
        input As CLNextUiCalculationInput) As List(Of CLNextUiPreselectionSummary)

        If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
        EnsureLanguage(input.LanguageCode)
        If input.SupplyAirflowM3h <= 0 Then
            Return New List(Of CLNextUiPreselectionSummary)()
        End If

        Dim requestedPressure = Math.Max(0, input.PressurePa)
        Return CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            ToList().
            Where(Function(model) Not String.Equals(model.Code, "ACC", StringComparison.OrdinalIgnoreCase) AndAlso
                                  Not String.Equals(model.Code, "IOM3", StringComparison.OrdinalIgnoreCase)).
            Select(Function(model) CalculatePreselectionCandidate(
                model, input.SupplyAirflowM3h, requestedPressure,
                input.PreselectionFilters)).
            Where(Function(candidate) candidate IsNot Nothing).
            OrderBy(Function(candidate) candidate.CombinedSfp).
            ThenBy(Function(candidate) candidate.RequiredRegulationPercent).
            ThenBy(Function(candidate) candidate.Model.NominalAirflowM3h).
            ThenBy(Function(candidate) candidate.Model.Code).
            ToList()
    End Function

    Public Shared Function Calculate(input As CLNextUiCalculationInput) As CLNextUiCalculationResult
        If input Is Nothing Then Throw New ArgumentNullException("input")
        EnsureLanguage(input.LanguageCode)
        Dim model = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(item) item.Code = input.ModelCode)
        If model Is Nothing Then Throw New InvalidOperationException("Selected model was not found.")

        Dim airflow = If(input.SupplyAirflowM3h > 0, input.SupplyAirflowM3h, CDbl(model.NominalAirflow.GetValueOrDefault()))
        Dim pressure = If(input.PressurePa >= 0, input.PressurePa, CDbl(model.StaticPressure.GetValueOrDefault()))
        Dim coils = CLCoilPerformanceCalculator.GetAvailableCoils(model)
        Dim heaters = CLElectricHeaterCalculator.GetAvailableHeaters(model)
        Dim selectedPreheater = SelectHeater(
            heaters, CLElectricHeaterMode.PEHD, input.ElectricPreheaterId)
        Dim selectedPostheater = SelectHeater(
            heaters, CLElectricHeaterMode.EHD, input.ElectricPostheaterId)
        Dim preheatPower = If(input.ElectricPreheaterEnabled AndAlso
            selectedPreheater IsNot Nothing, selectedPreheater.TotalPowerW, 0)

        Dim effectiveRegulation = Math.Max(1, Math.Min(100, input.RegulationPercent))
        Dim pressureCapacityExceeded As Boolean = False
        Dim winter = CalculateSeason(
            model, "Winter", airflow, pressure, effectiveRegulation,
            input.WinterOutdoorTemperatureC, input.WinterOutdoorRhPercent,
            input.WinterReturnTemperatureC, input.WinterReturnRhPercent,
            0, preheatPower)
        Dim summer = CalculateSeason(
            model, "Summer", airflow, pressure, effectiveRegulation,
            input.SummerOutdoorTemperatureC, input.SummerOutdoorRhPercent,
            input.SummerReturnTemperatureC, input.SummerReturnRhPercent,
            0, 0)

        Dim waterResults As New List(Of CLWaterCoilResult)()
        Dim electricResults As New List(Of CLElectricHeaterResult)()
        Dim additionalPressureDrop = CalculateAirTreatment(
            input, model, airflow, coils, selectedPreheater, selectedPostheater,
            winter, summer, waterResults, electricResults)

        If additionalPressureDrop > 0 Then
            Dim requiredFanPressure = pressure + additionalPressureDrop
            Dim operatingPoint = CLSelectionApplicationService.FindCompatibleFanOperatingPoint(
                model,
                airflow,
                requiredFanPressure,
                CInt(Math.Ceiling(effectiveRegulation)))
            If operatingPoint Is Nothing Then
                effectiveRegulation = 100
                pressureCapacityExceeded = True
            Else
                effectiveRegulation = operatingPoint.RegulationPercent
            End If
            winter = CalculateSeason(
                model, "Winter", airflow, pressure, effectiveRegulation,
                input.WinterOutdoorTemperatureC, input.WinterOutdoorRhPercent,
                input.WinterReturnTemperatureC, input.WinterReturnRhPercent,
                additionalPressureDrop, preheatPower)
            summer = CalculateSeason(
                model, "Summer", airflow, pressure, effectiveRegulation,
                input.SummerOutdoorTemperatureC, input.SummerOutdoorRhPercent,
                input.SummerReturnTemperatureC, input.SummerReturnRhPercent,
                additionalPressureDrop, 0)
            waterResults.Clear()
            electricResults.Clear()
            Dim effectiveAirflow = Math.Max(1, winter.Curves.WorkingPointAirflow)
            CalculateAirTreatment(
                input, model, effectiveAirflow, coils, selectedPreheater, selectedPostheater,
                winter, summer, waterResults, electricResults)
        End If

        Dim layout = CLInstallationLayoutRepository.Create().GetForModel(model, input.LayoutCode)
        Dim accessories = GetAccessories(model, input.AccessoryCodes)
        Dim validation = ValidateAirTreatment(input, selectedPostheater, waterResults)
        For Each unavailable In UnavailableTreatments(input, coils, heaters)
            validation.Issues.Add(New CLValidationIssue With {
                .Code = "TreatmentNotAvailable", .Severity = CLValidationSeverity.Error,
                .Path = unavailable, .MessageKey = unavailable & ": " & LocalizedText(
                    "MainForm_Accessories_Unavailable", "Not available for this unit.")})
        Next
        If layout.Configurations.Count = 0 Then
            validation.Issues.Add(New CLValidationIssue With {
                .Code = "LayoutConfigurationsMissing", .Severity = CLValidationSeverity.Error,
                .Path = "LayoutCode", .MessageKey = LocalizedText(
                    "Report_InstallationLayout_Title", "Installation configuration") & ": " & LocalizedText(
                    "MainForm_Accessories_Unavailable", "Not available for this unit.")})
        End If
        For Each code In If(input.AccessoryCodes, Enumerable.Empty(Of String)())
            If Not accessories.Any(Function(item) item.Included AndAlso
                String.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)) Then
                validation.Issues.Add(New CLValidationIssue With {
                    .Code = "AccessoryNotCompatible", .Severity = CLValidationSeverity.Error,
                    .Path = "AccessoryCodes", .MessageKey = code & ": " & LocalizedText(
                        "MainForm_Accessories_Unavailable", "Not available for this unit.")})
            End If
        Next
        Return New CLNextUiCalculationResult With {
            .Model = MapModel(model),
            .Winter = winter,
            .Summer = summer,
            .Layout = layout,
            .Accessories = accessories,
            .AvailableWaterCoils = coils.Select(Function(item) MapWaterCoil(item)).ToList(),
            .AvailableElectricHeaters = heaters.Select(Function(item) MapElectricHeater(item)).ToList(),
            .WaterCoilResults = waterResults,
            .ElectricHeaterResults = electricResults,
            .Sound = CLNextUiIndoorQualityService.CalculateSound(
                model, effectiveRegulation, airflow, pressure, input.Sound),
            .Co2 = CLNextUiIndoorQualityService.CalculateCo2(input.Co2),
            .AdditionalPressureDropPa = additionalPressureDrop,
            .EffectiveRegulationPercent = effectiveRegulation,
            .PressureCapacityExceeded = pressureCapacityExceeded,
            .PressureCapacityExceededMessage = PressureCapacityExceededText(),
            .WaterHeatingEnabled = Not input.ElectricPostheaterEnabled,
            .WaterHeatingDisabledReason = If(
                input.ElectricPostheaterEnabled,
                LocalizedText(
                    "MainForm_CoilPerformance_ElectricPostHeaterConflict",
                    "Disable the electric post-heater (EHD) to enable water post-heating."),
                String.Empty),
            .ElectricPostheaterEnabled = Not (
                input.WaterCoilEnabled AndAlso
                Not String.Equals(input.WaterCoilMode, "CWD", StringComparison.OrdinalIgnoreCase)),
            .ElectricPostheaterDisabledReason = If(
                input.WaterCoilEnabled AndAlso
                Not String.Equals(input.WaterCoilMode, "CWD", StringComparison.OrdinalIgnoreCase),
                LocalizedText(
                    "MainForm_ElectricHeater_WaterConflict",
                    "Deselect the heating water coil to enable the electric post-heater."),
                String.Empty),
            .WaterCoilStandardLabel = LocalizedText(
                "MainForm_CoilPerformance_Standard", "Standard"),
            .WaterCoilCustomizedLabel = LocalizedText(
                "MainForm_CoilPerformance_StandardCustomized", "Customized"),
            .WaterCoilCustomDisclaimer = LocalizedText(
                "MainForm_CoilPerformance_CustomDisclaimer",
                "Verify that the customized coil operates correctly at every intended working point. Press OK to acknowledge this requirement."),
            .WaterCoilDimensionsNotice = LocalizedText(
                "MainForm_CoilPerformance_DimensionsNote",
                "The dimensions shown refer to the water coil only."),
            .WaterCoilQuotationNotice = LocalizedText(
                "MainForm_CoilPerformance_CustomWarning",
                "Please ask for overall dimensions, delivery time and quotation."),
            .Validation = validation
        }
    End Function

    Public Shared Function CreateProjectDocument(
        input As CLNextUiCalculationInput,
        Optional calculation As CLNextUiCalculationResult = Nothing) As CLSelectionProjectDocument

        If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
        EnsureLanguage(input.LanguageCode)
        Dim model = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(item) item.Code = input.ModelCode)
        If model Is Nothing Then
            Throw New InvalidOperationException("The selected unit is not available in the local SDF.")
        End If

        Dim layout = CLInstallationLayoutRepository.Create().GetForModel(model, input.LayoutCode)
        Dim chosenLayout = If(String.IsNullOrWhiteSpace(input.LayoutCode), layout.DefaultConfiguration,
            layout.Configurations.FirstOrDefault(Function(item) String.Equals(item.Code, input.LayoutCode,
                StringComparison.OrdinalIgnoreCase)))
        If chosenLayout Is Nothing OrElse (Not String.IsNullOrWhiteSpace(input.LayoutCode) AndAlso
            Not layout.Configurations.Any(Function(item) String.Equals(item.Code, input.LayoutCode,
            StringComparison.OrdinalIgnoreCase) AndAlso String.Equals(item.InstallationMode,
            input.InstallationMode, StringComparison.OrdinalIgnoreCase))) Then
            Throw New InvalidOperationException("The installation configuration must be reviewed before saving.")
        End If
        Dim effectiveAccessories = GetAccessories(model, input.AccessoryCodes)
        If UnavailableTreatments(input, CLCoilPerformanceCalculator.GetAvailableCoils(model),
            CLElectricHeaterCalculator.GetAvailableHeaters(model)).Any() Then
            Throw New InvalidOperationException("The air-treatment selection must be reviewed before saving.")
        End If
        If If(input.AccessoryCodes, Enumerable.Empty(Of String)()).Any(Function(code) _
            Not effectiveAccessories.Any(Function(item) item.Included AndAlso String.Equals(
                item.Code, code, StringComparison.OrdinalIgnoreCase))) Then
            Throw New InvalidOperationException("The accessory selection must be reviewed before saving.")
        End If
        Dim document = CLSelectionProjectSerializer.CreateNew(
            CLEnvironment.Current.DatabaseCompatibility)
        document.Selection.ProjectName = input.ProjectName
        document.Selection.CustomerReference = input.CustomerReference
        document.Selection.InstallationMode = chosenLayout.InstallationMode
        document.Selection.LayoutCode = chosenLayout.Code
        document.Selection.ImbalanceEnabled = input.ImbalanceEnabled
        document.Selection.Unit = New CLSelectionEntityReference With {
            .Id = model.Id,
            .Code = model.Code,
            .ManagementCode = If(
                model.CLSerie Is Nothing,
                Nothing,
                CLEnvironment.Current.GetCustomerSerieName(model.CLSerie)),
            .Name = CLEnvironment.Current.GetCustomerHeatRecoveryModelName(model)
        }
        Dim dimensionalDrawing = CLDimensionalDrawingService.Resolve(
            model.Code, chosenLayout.Code, False)
        document.Selection.DimensionalDrawing = New CLDimensionalDrawingSelection With {
            .Available = dimensionalDrawing.Available,
            .AssetCode = dimensionalDrawing.Code,
            .Revision = dimensionalDrawing.Revision,
            .ContentHash = dimensionalDrawing.Sha256,
            .Orientation = dimensionalDrawing.Orientation,
            .Dimensions = dimensionalDrawing.Dimensions.Select(Function(item) _
                New CLDimensionalSelectionValue With {
                    .Code = item.Code,
                    .ValueMillimeters = item.ValueMillimeters
                }).ToList()
        }
        document.Selection.Winter = MapScenario(
            "Winter", True, input.SupplyAirflowM3h, input.ExtractAirflowM3h,
            input.PressurePa, input.RegulationPercent,
            input.WinterOutdoorTemperatureC, input.WinterOutdoorRhPercent,
            input.WinterReturnTemperatureC, input.WinterReturnRhPercent)
        document.Selection.Summer = MapScenario(
            "Summer", input.SummerEnabled, input.SupplyAirflowM3h, input.ExtractAirflowM3h,
            input.PressurePa, input.RegulationPercent,
            input.SummerOutdoorTemperatureC, input.SummerOutdoorRhPercent,
            input.SummerReturnTemperatureC, input.SummerReturnRhPercent)
        document.Selection.Report.LanguageCode = If(
            String.IsNullOrWhiteSpace(input.LanguageCode), "it", input.LanguageCode)
        document.Selection.Sound = MapSoundSelection(input.Sound)
        document.Selection.PreselectionFilters = MapPreselectionFilters(
            input.PreselectionFilters)
        document.Selection.Co2 = MapCo2Selection(input.Co2)
        document.Selection.Report.IncludeSoundPower =
            document.Selection.Sound.IncludeInReport
        document.Selection.Report.IncludeCo2 =
            document.Selection.Co2.IncludeInReport

        Dim coil = SelectCoil(
            CLCoilPerformanceCalculator.GetAvailableCoils(model), input.WaterCoilId)
        document.Selection.WaterCoil.Enabled = input.WaterCoilEnabled AndAlso coil IsNot Nothing
        document.Selection.WaterCoil.CalculationMode = input.WaterCoilMode
        document.Selection.WaterCoil.SelectionCase = If(
            input.WaterCoilCustomized, "Customized", "Standard")
        document.Selection.WaterCoil.CustomDesignDisclaimerAccepted =
            input.WaterCoilCustomized AndAlso input.WaterCoilCustomDisclaimerAccepted
        document.Selection.WaterCoil.Fluid.Code = input.FluidCode
        document.Selection.WaterCoil.Fluid.GlycolPercent = input.GlycolPercent
        document.Selection.WaterCoil.CoolingWaterInletTemperatureC =
            input.CoolingWaterInletTemperatureC
        document.Selection.WaterCoil.CoolingWaterOutletTemperatureC =
            input.CoolingWaterOutletTemperatureC
        document.Selection.WaterCoil.HeatingWaterInletTemperatureC =
            input.HeatingWaterInletTemperatureC
        document.Selection.WaterCoil.HeatingWaterOutletTemperatureC =
            input.HeatingWaterOutletTemperatureC
        If coil IsNot Nothing Then
            document.Selection.WaterCoil.InstallationType = coil.Installation.ToString()
            document.Selection.WaterCoil.Coil = New CLSelectionEntityReference With {
                .Id = coil.Id,
                .Code = coil.Name,
                .ManagementCode = coil.Name,
                .Name = coil.Name
            }
            document.Selection.WaterCoil.Geometry = New CLCoilGeometrySelection With {
                .GeometryCode = "2510",
                .LengthMm = If(input.WaterCoilLengthMm > 0, input.WaterCoilLengthMm, coil.Length),
                .HeightMm = If(input.WaterCoilHeightMm > 0, input.WaterCoilHeightMm, coil.Height),
                .NumberOfRows = If(input.WaterCoilRows > 0, input.WaterCoilRows, coil.NumberOfRows),
                .NumberOfCircuits = If(input.WaterCoilCircuits > 0, input.WaterCoilCircuits, coil.NumberOfCircuits),
                .FinSpacingMm = If(input.WaterCoilFinSpacingMm > 0, input.WaterCoilFinSpacingMm, coil.FinSpacingValue)
            }
        End If

        Dim heaters = CLElectricHeaterCalculator.GetAvailableHeaters(model)
        MapHeaterSelection(
            document.Selection.ElectricHeater.PEHD,
            SelectHeater(heaters, CLElectricHeaterMode.PEHD, input.ElectricPreheaterId),
            input.ElectricPreheaterEnabled)
        MapHeaterSelection(
            document.Selection.ElectricHeater.EHD,
            SelectHeater(heaters, CLElectricHeaterMode.EHD, input.ElectricPostheaterId),
            input.ElectricPostheaterEnabled)
        document.Selection.ElectricHeater.Enabled =
            document.Selection.ElectricHeater.PEHD.Enabled OrElse
            document.Selection.ElectricHeater.EHD.Enabled

        Dim availableAccessories = GetAccessories(model, input.AccessoryCodes)
        For Each item In availableAccessories.Where(Function(candidate) candidate.Included)
            document.Selection.Accessories.Add(New CLAccessorySelection With {
                .Code = item.Code,
                .ItemType = item.ItemType,
                .Quantity = 1,
                .Availability = item.Availability,
                .InstallationType = item.Installation,
                .LocalizedDisplayName = item.Name,
                .LocalizedDescription = item.Description,
                .LocalizedFunctionNames = item.FunctionNames.ToList()
            })
        Next
        If calculation Is Nothing Then calculation = Calculate(input)
        PopulateCalculatedSnapshot(document, calculation)
        Return document
    End Function

    Public Shared Sub PopulateCalculatedSnapshot(
        document As CLSelectionProjectDocument,
        calculation As CLNextUiCalculationResult)

        If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
        If calculation Is Nothing Then Throw New ArgumentNullException(NameOf(calculation))

        document.Versions = CLSelectionProjectSerializer.CreateCurrentVersionSet(
            CLEnvironment.Current.DatabaseCompatibility)
        document.Snapshot = New CLCalculatedSelectionSnapshot With {
            .Status = If(
                calculation.Validation IsNot Nothing AndAlso
                calculation.Validation.HasErrors,
                "Invalid",
                "Calculated"),
            .CalculatedAtUtc = DateTime.UtcNow,
            .Versions = document.Versions,
            .Winter = MapScenarioSnapshot("Winter", calculation.Winter)
        }
        If document.Selection.Summer IsNot Nothing AndAlso
            document.Selection.Summer.Enabled Then
            document.Snapshot.Summer =
                MapScenarioSnapshot("Summer", calculation.Summer)
        End If

        document.Snapshot.WaterCoils =
            calculation.WaterCoilResults.
                Select(Function(item) New CLWaterCoilCalculationSnapshot With {
                    .ScenarioCode = item.ScenarioCode,
                    .Mode = item.Mode,
                    .Status = item.StatusCode,
                    .CapacityW = item.CapacityW,
                    .SensibleCapacityW = item.SensibleCapacityW,
                    .AirOutletTemperatureC = item.AirOutletTemperatureC,
                    .AirOutletRelativeHumidityPercent =
                        item.AirOutletRelativeHumidityPercent,
                    .CondensateLitersPerHour = item.CondensateLitersPerHour,
                    .AirPressureDropPa = item.AirPressureDropPa,
                    .FluidPressureDropKPa = item.FluidPressureDropKPa,
                    .FluidFlowLitersPerHour = item.FluidFlowLitersPerHour,
                    .FluidVelocityMetersPerSecond =
                        item.FluidVelocityMetersPerSecond,
                    .FaceVelocityMetersPerSecond =
                        item.FaceVelocityMetersPerSecond
                }).ToList()
        document.Snapshot.ElectricHeaters =
            calculation.ElectricHeaterResults.
                Select(Function(item) New CLElectricHeaterCalculationSnapshot With {
                    .ScenarioCode = item.ScenarioCode,
                    .Mode = item.Mode,
                    .HeaterCode = item.HeaterCode,
                    .PowerW = item.PowerW,
                    .CurrentA = item.CurrentA,
                    .AirInletTemperatureC = item.AirInletTemperatureC,
                    .AirOutletTemperatureC = item.AirOutletTemperatureC,
                    .AirOutletRelativeHumidityPercent =
                        item.AirOutletRelativeHumidityPercent,
                    .AirPressureDropPa = item.AirPressureDropPa,
                    .ExhaustOutletTemperatureC =
                        item.ExhaustOutletTemperatureC
                }).ToList()
        document.Snapshot.SoundRows =
            If(calculation.Sound?.Rows, New List(Of CLNextUiSoundRow)()).
                Select(Function(item) New CLSoundCalculationSnapshot With {
                    .Type = item.Type,
                    .Caption = item.Caption,
                    .Bands = If(item.Bands, New Double() {}).ToArray(),
                    .LwA = item.LwA,
                    .Lp1 = item.Lp1,
                    .Lp2 = item.Lp2
                }).ToList()
        If calculation.Co2 IsNot Nothing Then
            document.Snapshot.Co2 = New CLCo2CalculationSnapshot With {
                .RequiredAirflowLitersPerSecond =
                    calculation.Co2.RequiredAirflowLitersPerSecond,
                .RequiredAirflowM3h = calculation.Co2.RequiredAirflowM3h,
                .MaximumCo2Ppm = calculation.Co2.MaximumCo2Ppm,
                .Co2ProductionPerPersonLitersPerHour =
                    calculation.Co2.Co2ProductionPerPersonLitersPerHour
            }
        End If

        document.Features.Clear()
        If document.Selection.Summer IsNot Nothing AndAlso
            document.Selection.Summer.Enabled Then
            document.Features.Add("SummerCalculation")
        End If
        If document.Selection.WaterCoil IsNot Nothing AndAlso
            document.Selection.WaterCoil.Enabled Then
            document.Features.Add("WaterCoils")
        End If
        If document.Selection.ElectricHeater IsNot Nothing AndAlso
            document.Selection.ElectricHeater.Enabled Then
            document.Features.Add("ElectricHeaters")
        End If
        If document.Selection.Accessories IsNot Nothing AndAlso
            document.Selection.Accessories.Count > 0 Then
            document.Features.Add("AccessoriesAndControlFunctions")
        End If
        If document.Selection.DimensionalDrawing IsNot Nothing AndAlso
            document.Selection.DimensionalDrawing.Dimensions IsNot Nothing AndAlso
            document.Selection.DimensionalDrawing.Dimensions.Count > 0 Then
            document.Features.Add("DimensionalDrawing")
        End If
        CLSelectionSnapshotService.Refresh(document)
    End Sub

    Private Shared Function MapScenarioSnapshot(
        scenarioCode As String,
        calculation As CLBalancedScenarioCalculation) As CLScenarioCalculationSnapshot

        If calculation Is Nothing OrElse calculation.Result Is Nothing Then
            Return Nothing
        End If
        Dim result = calculation.Result
        Dim supply = If(result.SupplyBranch, New CLBranchCalculationResult())
        Dim thermodynamics = If(
            result.Thermodynamics,
            New CLThermodynamicCalculationResult())
        Return New CLScenarioCalculationSnapshot With {
            .ScenarioCode = scenarioCode,
            .AirflowM3h = supply.AirflowM3h,
            .AvailablePressurePa = supply.AvailablePressurePa,
            .AbsorbedPowerW = supply.AbsorbedPowerW,
            .HeatTransferredW = thermodynamics.HeatTransferredW,
            .SensibleHeatW = thermodynamics.SensibleHeatW,
            .LatentHeatW = thermodynamics.LatentHeatW,
            .EfficiencyPercent = thermodynamics.EfficiencyPercent,
            .CondensateLitersPerHour =
                thermodynamics.CondensateLitersPerHour,
            .SupplyOutletTemperatureC =
                thermodynamics.SupplyOutletTemperatureC,
            .SupplyOutletRelativeHumidityPercent =
                thermodynamics.SupplyOutletRelativeHumidityPercent,
            .ExhaustOutletTemperatureC =
                thermodynamics.ExhaustOutletTemperatureC,
            .ExhaustOutletRelativeHumidityPercent =
                thermodynamics.ExhaustOutletRelativeHumidityPercent
        }
    End Function

    Public Shared Function CreateInputFromProjectDocument(
        document As CLSelectionProjectDocument) As CLNextUiCalculationInput

        If document Is Nothing OrElse document.Selection Is Nothing Then
            Throw New ArgumentNullException(NameOf(document))
        End If

        Dim selection = document.Selection
        Dim winter = If(selection.Winter, New CLOperatingScenarioInput())
        Dim summer = If(selection.Summer, New CLOperatingScenarioInput())
        Dim water = If(selection.WaterCoil, New CLWaterCoilSelection())
        Dim fluid = If(water.Fluid, New CLFluidSelection())
        Dim geometry = If(water.Geometry, New CLCoilGeometrySelection())
        Dim electric = If(selection.ElectricHeater, New CLElectricHeaterSelection())
        Dim pehd = If(electric.PEHD, New CLElectricHeaterModeSelection())
        Dim ehd = If(electric.EHD, New CLElectricHeaterModeSelection())
        Dim sound = If(selection.Sound, New CLSoundSelection())
        Dim filters = If(selection.PreselectionFilters,
            New CLPreselectionFilterSelection())
        Dim co2 = If(selection.Co2, New CLCo2Selection())

        Return New CLNextUiCalculationInput With {
            .ProjectName = selection.ProjectName,
            .CustomerReference = selection.CustomerReference,
            .LanguageCode = If(selection.Report?.LanguageCode, "it"),
            .ModelCode = selection.Unit?.Code,
            .SupplyAirflowM3h = winter.SupplyAirflowM3h.GetValueOrDefault(100),
            .ExtractAirflowM3h = winter.ExtractAirflowM3h.GetValueOrDefault(
                winter.SupplyAirflowM3h.GetValueOrDefault(100)),
            .ImbalanceEnabled = selection.ImbalanceEnabled,
            .PressurePa = winter.MaximumPressurePa.GetValueOrDefault(),
            .RegulationPercent = winter.RegulationPercent.GetValueOrDefault(100),
            .SummerEnabled = summer.Enabled,
            .WinterOutdoorTemperatureC = winter.OutdoorTemperatureC.GetValueOrDefault(-10),
            .WinterOutdoorRhPercent = winter.OutdoorRelativeHumidityPercent.GetValueOrDefault(80),
            .WinterReturnTemperatureC = winter.ReturnTemperatureC.GetValueOrDefault(20),
            .WinterReturnRhPercent = winter.ReturnRelativeHumidityPercent.GetValueOrDefault(60),
            .SummerOutdoorTemperatureC = summer.OutdoorTemperatureC.GetValueOrDefault(32),
            .SummerOutdoorRhPercent = summer.OutdoorRelativeHumidityPercent.GetValueOrDefault(80),
            .SummerReturnTemperatureC = summer.ReturnTemperatureC.GetValueOrDefault(26),
            .SummerReturnRhPercent = summer.ReturnRelativeHumidityPercent.GetValueOrDefault(50),
            .InstallationMode = If(selection.InstallationMode, "Ceiling"),
            .LayoutCode = If(selection.LayoutCode, String.Empty),
            .WaterCoilEnabled = water.Enabled,
            .WaterCoilId = water.Coil?.Id.GetValueOrDefault(),
            .WaterCoilMode = If(water.CalculationMode, "HCD"),
            .WaterCoilCustomized = String.Equals(
                water.SelectionCase, "Customized", StringComparison.OrdinalIgnoreCase),
            .WaterCoilCustomDisclaimerAccepted = water.CustomDesignDisclaimerAccepted,
            .WaterCoilLengthMm = geometry.LengthMm.GetValueOrDefault(),
            .WaterCoilHeightMm = geometry.HeightMm.GetValueOrDefault(),
            .WaterCoilRows = geometry.NumberOfRows.GetValueOrDefault(),
            .WaterCoilCircuits = geometry.NumberOfCircuits.GetValueOrDefault(),
            .WaterCoilFinSpacingMm = geometry.FinSpacingMm.GetValueOrDefault(),
            .FluidCode = If(fluid.Code, "Water"),
            .GlycolPercent = fluid.GlycolPercent.GetValueOrDefault(10),
            .CoolingWaterInletTemperatureC = water.CoolingWaterInletTemperatureC.GetValueOrDefault(7),
            .CoolingWaterOutletTemperatureC = water.CoolingWaterOutletTemperatureC.GetValueOrDefault(12),
            .HeatingWaterInletTemperatureC = water.HeatingWaterInletTemperatureC.GetValueOrDefault(80),
            .HeatingWaterOutletTemperatureC = water.HeatingWaterOutletTemperatureC.GetValueOrDefault(70),
            .ElectricPreheaterEnabled = pehd.Enabled,
            .ElectricPreheaterId = pehd.Heater?.Id.GetValueOrDefault(),
            .ElectricPostheaterEnabled = ehd.Enabled,
            .ElectricPostheaterId = ehd.Heater?.Id.GetValueOrDefault(),
            .Sound = New CLNextUiSoundInput With {
                .IncludeInReport = sound.IncludeInReport OrElse
                    selection.Report?.IncludeSoundPower,
                .Directivity = sound.Directivity,
                .Distance1Meters = sound.Distance1Meters,
                .Distance2Meters = sound.Distance2Meters,
                .IncludeIso16032 = sound.IncludeIso16032
            },
            .PreselectionFilters = New CLNextUiPreselectionFilters With {
                .MaximumSfpEnabled = filters.MaximumSfpEnabled,
                .MaximumSfp = filters.MaximumSfp,
                .SupplyNoiseEnabled = filters.SupplyNoiseEnabled,
                .SupplyNoiseMetric = filters.SupplyNoiseMetric,
                .MaximumSupplyNoiseDbA = filters.MaximumSupplyNoiseDbA,
                .SupplyNoiseDirectivity = filters.SupplyNoiseDirectivity,
                .SupplyNoiseDistanceMeters = filters.SupplyNoiseDistanceMeters,
                .BreakoutNoiseEnabled = filters.BreakoutNoiseEnabled,
                .BreakoutNoiseMetric = filters.BreakoutNoiseMetric,
                .MaximumBreakoutNoiseDbA = filters.MaximumBreakoutNoiseDbA,
                .BreakoutNoiseDirectivity = filters.BreakoutNoiseDirectivity,
                .BreakoutNoiseDistanceMeters = filters.BreakoutNoiseDistanceMeters
            },
            .Co2 = New CLNextUiCo2Input With {
                .IncludeInReport = co2.IncludeInReport OrElse
                    selection.Report?.IncludeCo2,
                .RoomHeightMeters = co2.RoomHeightMeters,
                .RoomLengthMeters = co2.RoomLengthMeters,
                .RoomWidthMeters = co2.RoomWidthMeters,
                .ActivityMet = co2.ActivityMet,
                .PeopleDuringBreak = co2.PeopleDuringBreak,
                .PeopleDuringPresence = co2.PeopleDuringPresence,
                .BreakMinutes = co2.BreakMinutes,
                .PresenceMinutes = co2.PresenceMinutes,
                .CalculationMethod = co2.CalculationMethod,
                .StandardPreset = co2.StandardPreset,
                .OutdoorCo2Ppm = co2.OutdoorCo2Ppm,
                .MaximumCo2Ppm = co2.MaximumCo2Ppm,
                .FixedAirflowLitersPerSecond = co2.FixedAirflowLitersPerSecond,
                .AirflowPerPersonLitersPerSecond =
                    co2.AirflowPerPersonLitersPerSecond,
                .AirflowPerAreaLitersPerSecondM2 =
                    co2.AirflowPerAreaLitersPerSecondM2
            },
            .AccessoryCodes = If(selection.Accessories, New List(Of CLAccessorySelection)()).
                Select(Function(item) item.Code).Where(Function(code) Not String.IsNullOrWhiteSpace(code)).
                ToList()
        }
    End Function

    Private Shared Function MapSoundSelection(
        input As CLNextUiSoundInput) As CLSoundSelection
        If input Is Nothing Then input = New CLNextUiSoundInput()
        Return New CLSoundSelection With {
            .IncludeInReport = input.IncludeInReport,
            .Directivity = input.Directivity,
            .Distance1Meters = input.Distance1Meters,
            .Distance2Meters = input.Distance2Meters,
            .IncludeIso16032 = input.IncludeIso16032
        }
    End Function

    Private Shared Function MapPreselectionFilters(
        input As CLNextUiPreselectionFilters) As CLPreselectionFilterSelection
        If input Is Nothing Then input = New CLNextUiPreselectionFilters()
        Return New CLPreselectionFilterSelection With {
            .MaximumSfpEnabled = input.MaximumSfpEnabled,
            .MaximumSfp = input.MaximumSfp,
            .SupplyNoiseEnabled = input.SupplyNoiseEnabled,
            .SupplyNoiseMetric = input.SupplyNoiseMetric,
            .MaximumSupplyNoiseDbA = input.MaximumSupplyNoiseDbA,
            .SupplyNoiseDirectivity = input.SupplyNoiseDirectivity,
            .SupplyNoiseDistanceMeters = input.SupplyNoiseDistanceMeters,
            .BreakoutNoiseEnabled = input.BreakoutNoiseEnabled,
            .BreakoutNoiseMetric = input.BreakoutNoiseMetric,
            .MaximumBreakoutNoiseDbA = input.MaximumBreakoutNoiseDbA,
            .BreakoutNoiseDirectivity = input.BreakoutNoiseDirectivity,
            .BreakoutNoiseDistanceMeters = input.BreakoutNoiseDistanceMeters
        }
    End Function

    Private Shared Function MapCo2Selection(
        input As CLNextUiCo2Input) As CLCo2Selection
        If input Is Nothing Then input = New CLNextUiCo2Input()
        Return New CLCo2Selection With {
            .IncludeInReport = input.IncludeInReport,
            .RoomHeightMeters = input.RoomHeightMeters,
            .RoomLengthMeters = input.RoomLengthMeters,
            .RoomWidthMeters = input.RoomWidthMeters,
            .ActivityMet = input.ActivityMet,
            .PeopleDuringBreak = input.PeopleDuringBreak,
            .PeopleDuringPresence = input.PeopleDuringPresence,
            .BreakMinutes = input.BreakMinutes,
            .PresenceMinutes = input.PresenceMinutes,
            .CalculationMethod = input.CalculationMethod,
            .StandardPreset = input.StandardPreset,
            .OutdoorCo2Ppm = input.OutdoorCo2Ppm,
            .MaximumCo2Ppm = input.MaximumCo2Ppm,
            .FixedAirflowLitersPerSecond = input.FixedAirflowLitersPerSecond,
            .AirflowPerPersonLitersPerSecond =
                input.AirflowPerPersonLitersPerSecond,
            .AirflowPerAreaLitersPerSecondM2 =
                input.AirflowPerAreaLitersPerSecondM2
        }
    End Function

    Private Shared Function MapScenario(
        code As String,
        enabled As Boolean,
        supplyAirflow As Double,
        extractAirflow As Double,
        pressure As Double,
        regulation As Double,
        outdoorTemperature As Double,
        outdoorRh As Double,
        returnTemperature As Double,
        returnRh As Double) As CLOperatingScenarioInput

        Return New CLOperatingScenarioInput With {
            .ScenarioCode = code,
            .Enabled = enabled,
            .SupplyAirflowM3h = supplyAirflow,
            .ExtractAirflowM3h = extractAirflow,
            .MaximumPressurePa = pressure,
            .RegulationPercent = regulation,
            .OutdoorTemperatureC = outdoorTemperature,
            .OutdoorRelativeHumidityPercent = outdoorRh,
            .ReturnTemperatureC = returnTemperature,
            .ReturnRelativeHumidityPercent = returnRh
        }
    End Function

    Private Shared Sub MapHeaterSelection(
        target As CLElectricHeaterModeSelection,
        heater As CLElectricHeaterDefinition,
        enabled As Boolean)

        target.Enabled = enabled AndAlso heater IsNot Nothing
        If heater Is Nothing Then Return
        target.InstallationType = heater.Installation.ToString()
        target.Heater = New CLSelectionEntityReference With {
            .Id = heater.Id,
            .Code = heater.Code,
            .ManagementCode = heater.ManagementCode,
            .Name = heater.Name
        }
    End Sub

    Private Shared Function CalculateSeason(
        model As CLDCHeatRecoveryModel,
        scenarioCode As String,
        airflow As Double,
        pressure As Double,
        regulationPercent As Double,
        outdoorTemperature As Double,
        outdoorRh As Double,
        returnTemperature As Double,
        returnRh As Double,
        additionalPressureDropPa As Double,
        preheatPowerW As Double) As CLBalancedScenarioCalculation

        Dim scenario As New CLSeasonCalculationInput With {
            .Enabled = True,
            .ScenarioCode = scenarioCode,
            .Airflows = CLAirflowPair.Balanced(airflow),
            .MaximumPressurePa = CLBranchValuePair.Balanced(pressure),
            .OutdoorTemperatureC = outdoorTemperature,
            .OutdoorRelativeHumidityPercent = outdoorRh,
            .ReturnTemperatureC = returnTemperature,
            .ReturnRelativeHumidityPercent = returnRh,
            .RegulationPercent = regulationPercent
        }
        Return CLSelectionApplicationService.CalculateBalancedScenario(
            New CLBalancedScenarioCalculationRequest With {
                .Scenario = scenario,
                .Model = model,
                .MeasureUnit = CLMeasureUnit.SI,
                .AdditionalPressureDropPa = additionalPressureDropPa,
                .PreheatPowerW = preheatPowerW
            })
    End Function

    Private Shared Function CalculateAirTreatment(
        input As CLNextUiCalculationInput,
        model As CLDCHeatRecoveryModel,
        airflow As Double,
        coils As List(Of CLCoilDefinition),
        preheater As CLElectricHeaterDefinition,
        postheater As CLElectricHeaterDefinition,
        winter As CLBalancedScenarioCalculation,
        summer As CLBalancedScenarioCalculation,
        waterResults As List(Of CLWaterCoilResult),
        electricResults As List(Of CLElectricHeaterResult)) As Double

        Dim waterPressureDrop As Double = 0
        If input.WaterCoilEnabled Then
            Dim coil = SelectCoil(coils, input.WaterCoilId)
            If coil IsNot Nothing Then
                coil = ApplyCoilOverrides(coil, input)
                Dim mode As CLCoilPerformanceMode
                If Not [Enum].TryParse(input.WaterCoilMode, True, mode) Then
                    mode = coil.Mode
                End If
                Dim hydraulicIssues = CLCoilHydraulicRules.Evaluate(
                    mode,
                    input.CoolingWaterInletTemperatureC,
                    input.CoolingWaterOutletTemperatureC,
                    input.HeatingWaterInletTemperatureC,
                    input.HeatingWaterOutletTemperatureC)
                Dim canCalculateCoil =
                    Not (input.ElectricPostheaterEnabled AndAlso
                         postheater IsNot Nothing AndAlso
                         mode <> CLCoilPerformanceMode.CWD) AndAlso
                    Not hydraulicIssues.Any(Function(issue) issue.IsBlocking)
                If canCalculateCoil Then
                    Dim fluidType = ParseFluidType(input.FluidCode)
                    Dim coilInput As New CLCoilCalculationInput With {
                    .Coil = coil,
                    .CalculationMode = mode,
                    .AirFlow = airflow,
                    .UseModeAirInletConditions = True,
                    .CoolingAirInletTemperature = summer.Result.Thermodynamics.SupplyOutletTemperatureC.GetValueOrDefault(),
                    .CoolingAirInletRH = summer.Result.Thermodynamics.SupplyOutletRelativeHumidityPercent.GetValueOrDefault(),
                    .HeatingAirInletTemperature = winter.Result.Thermodynamics.SupplyOutletTemperatureC.GetValueOrDefault(),
                    .HeatingAirInletRH = winter.Result.Thermodynamics.SupplyOutletRelativeHumidityPercent.GetValueOrDefault(),
                    .FluidType = fluidType,
                    .FluidTypeTec = If(fluidType = CLCOFluidType.Water, 0, input.GlycolPercent),
                    .CoolingFluidInletTemperature = input.CoolingWaterInletTemperatureC,
                    .CoolingFluidOutletTemperature = input.CoolingWaterOutletTemperatureC,
                    .HeatingFluidInletTemperature = input.HeatingWaterInletTemperatureC,
                    .HeatingFluidOutletTemperature = input.HeatingWaterOutletTemperatureC
                    }
                    For Each result In CLCoilPerformanceCalculator.Calculate(coilInput)
                        waterResults.Add(MapWaterCoilResult(result))
                        waterPressureDrop = Math.Max(waterPressureDrop, result.AirPressureDrop)
                    Next
                End If
            End If
        End If

        Dim electricPressureDrop As Double = 0
        If input.ElectricPreheaterEnabled AndAlso preheater IsNot Nothing Then
            Dim result = CalculateElectricHeater(
                preheater,
                input.WinterOutdoorTemperatureC,
                input.WinterOutdoorRhPercent,
                airflow,
                model.NominalAirflow.GetValueOrDefault(),
                winter.Result.Thermodynamics.ExhaustOutletTemperatureC)
            electricResults.Add(result)
            electricPressureDrop += result.AirPressureDropPa.GetValueOrDefault()
        End If
        If input.ElectricPostheaterEnabled AndAlso postheater IsNot Nothing Then
            Dim result = CalculateElectricHeater(
                postheater,
                winter.Result.Thermodynamics.SupplyOutletTemperatureC.GetValueOrDefault(),
                winter.Result.Thermodynamics.SupplyOutletRelativeHumidityPercent.GetValueOrDefault(),
                airflow,
                model.NominalAirflow.GetValueOrDefault(),
                Nothing)
            electricResults.Add(result)
            electricPressureDrop += result.AirPressureDropPa.GetValueOrDefault()
        End If

        Return waterPressureDrop + electricPressureDrop
    End Function

    Private Shared Function CalculateElectricHeater(
        heater As CLElectricHeaterDefinition,
        inletTemperature As Double,
        inletRhPercent As Double,
        airflow As Double,
        nominalAirflow As Double,
        exhaustTemperature As Double?) As CLElectricHeaterResult

        Dim outletTemperature = inletTemperature +
            CLElectricHeaterCalculator.TemperatureRise(heater.TotalPowerW, airflow)
        Dim inletHumidity = PsychroCalc(
            inletTemperature,
            Math.Max(0, Math.Min(100, inletRhPercent)) / 100)
        Dim outletRh = Math.Max(0, Math.Min(100,
            100 * PsychroCalcW(outletTemperature, inletHumidity.w).rh))
        Dim referenceAirflow = If(nominalAirflow > 0, nominalAirflow, airflow)
        Dim nominalDrop = CLElectricHeaterCalculator.NominalPressureDrop(referenceAirflow) *
            Math.Max(1, heater.Quantity)
        Dim pressureDrop = CLElectricHeaterCalculator.PressureDropAtAirflow(
            nominalDrop, airflow, referenceAirflow)
        Return New CLElectricHeaterResult With {
            .ScenarioCode = "Winter",
            .Mode = heater.Mode.ToString(),
            .HeaterCode = heater.Code,
            .PowerW = heater.TotalPowerW,
            .CurrentA = heater.TotalCurrentA,
            .AirInletTemperatureC = inletTemperature,
            .AirOutletTemperatureC = outletTemperature,
            .AirOutletRelativeHumidityPercent = outletRh,
            .AirPressureDropPa = pressureDrop,
            .ExhaustOutletTemperatureC = exhaustTemperature
        }
    End Function

    Private Shared Function UnavailableTreatments(input As CLNextUiCalculationInput,
        coils As List(Of CLCoilDefinition), heaters As List(Of CLElectricHeaterDefinition)) As List(Of String)

        Dim missing As New List(Of String)()
        If input.WaterCoilEnabled AndAlso SelectCoil(coils, input.WaterCoilId) Is Nothing Then missing.Add("WaterCoil")
        If input.ElectricPreheaterEnabled AndAlso SelectHeater(heaters, CLElectricHeaterMode.PEHD,
            input.ElectricPreheaterId) Is Nothing Then missing.Add("PEHD")
        If input.ElectricPostheaterEnabled AndAlso SelectHeater(heaters, CLElectricHeaterMode.EHD,
            input.ElectricPostheaterId) Is Nothing Then missing.Add("EHD")
        Return missing
    End Function

    Private Shared Function SelectCoil(
        coils As List(Of CLCoilDefinition),
        id As Integer) As CLCoilDefinition

        Dim selected = coils.FirstOrDefault(Function(item) item.Id = id)
        If selected Is Nothing AndAlso id <= 0 Then selected = coils.FirstOrDefault()
        Return If(selected Is Nothing, Nothing, selected.Clone())
    End Function

    Private Shared Function SelectHeater(
        heaters As List(Of CLElectricHeaterDefinition),
        mode As CLElectricHeaterMode,
        id As Integer) As CLElectricHeaterDefinition

        Dim available = heaters.Where(Function(item) item.Mode = mode).ToList()
        Dim selected = available.FirstOrDefault(Function(item) item.Id = id)
        If selected Is Nothing AndAlso id <= 0 Then
            selected = available.FirstOrDefault(Function(item) item.IsDefault)
        End If
        If selected Is Nothing AndAlso id <= 0 Then selected = available.FirstOrDefault()
        Return If(selected Is Nothing, Nothing, selected.Clone())
    End Function

    Private Shared Function ApplyCoilOverrides(
        coil As CLCoilDefinition,
        input As CLNextUiCalculationInput) As CLCoilDefinition

        If Not input.WaterCoilCustomized Then Return coil
        If input.WaterCoilLengthMm > 0 Then coil.Length = input.WaterCoilLengthMm
        If input.WaterCoilHeightMm > 0 Then coil.Height = input.WaterCoilHeightMm
        If input.WaterCoilRows > 0 Then coil.NumberOfRows = input.WaterCoilRows
        If input.WaterCoilCircuits > 0 Then coil.NumberOfCircuits = input.WaterCoilCircuits
        If input.WaterCoilFinSpacingMm > 0 Then
            coil.FinSpacingValue = input.WaterCoilFinSpacingMm
            coil.FinSpacing = CLCoilPerformanceCalculator.DoubleToFinSpacing(
                input.WaterCoilFinSpacingMm)
        End If
        Return coil
    End Function

    Private Shared Function ParseFluidType(code As String) As CLCOFluidType
        Dim value As CLCOFluidType
        If [Enum].TryParse(code, True, value) Then Return value
        Select Case If(code, String.Empty).Trim().ToLowerInvariant()
            Case "ethylene", "ethylene glycol", "glicole etilico"
                Return CLCOFluidType.Glic_Etil
            Case "propylene", "propylene glycol", "glicole propilenico"
                Return CLCOFluidType.Glic_Prop
            Case Else
                Return CLCOFluidType.Water
        End Select
    End Function

    Private Shared Function ValidateAirTreatment(
        input As CLNextUiCalculationInput,
        postheater As CLElectricHeaterDefinition,
        waterResults As IEnumerable(Of CLWaterCoilResult)) As CLValidationResult

        Dim validation As New CLValidationResult()
        If input.WaterCoilEnabled AndAlso input.ElectricPostheaterEnabled AndAlso
            postheater IsNot Nothing AndAlso
            Not String.Equals(input.WaterCoilMode, "CWD", StringComparison.OrdinalIgnoreCase) Then
            validation.Issues.Add(New CLValidationIssue With {
                .Code = "WaterHeatingElectricPostheaterConflict",
                .Severity = CLValidationSeverity.Error,
                .Path = "WaterCoil.CalculationMode",
                .MessageKey = LocalizedText(
                    "MainForm_CoilPerformance_ElectricPostHeaterConflict",
                    "Disable the electric post-heater (EHD) to enable water post-heating.")
            })
        End If
        If input.WaterCoilEnabled Then
            Dim mode As CLCoilPerformanceMode
            If Not [Enum].TryParse(input.WaterCoilMode, True, mode) Then
                mode = CLCoilPerformanceMode.HCD
            End If
            For Each issue In CLCoilHydraulicRules.Evaluate(
                mode,
                input.CoolingWaterInletTemperatureC,
                input.CoolingWaterOutletTemperatureC,
                input.HeatingWaterInletTemperatureC,
                input.HeatingWaterOutletTemperatureC)

                validation.Issues.Add(New CLValidationIssue With {
                    .Code = issue.Code.ToString(),
                    .Severity = If(
                        issue.IsBlocking OrElse
                        issue.Code = CLCoilHydraulicIssueCode.CriticalWaterDeltaT,
                        CLValidationSeverity.Error,
                        CLValidationSeverity.Warning),
                    .Path = "WaterCoil.FluidTemperatures",
                    .MessageKey = HydraulicIssueText(issue.Code)
                })
            Next
            If waterResults IsNot Nothing AndAlso waterResults.Any(
                Function(result) result.FluidPressureDropKPa >
                    CLCoilHydraulicRules.MaximumRecommendedWaterPressureDrop) Then
                validation.Issues.Add(New CLValidationIssue With {
                    .Code = "WaterPressureDropHigh",
                    .Severity = CLValidationSeverity.Error,
                    .Path = "WaterCoil.Results.FluidPressureDropKPa",
                    .MessageKey = LocalizedText(
                        "MainForm_CoilPerformance_WaterPressureDropWarning",
                        "Water pressure drop exceeds the recommended limit (40 kPa). Hydraulic power consumption and pumping costs may become excessive. Consider increasing coil size or reducing water velocity.")
                })
            End If
        End If
        Return validation
    End Function

    Private Shared Function HydraulicIssueText(code As CLCoilHydraulicIssueCode) As String
        Select Case code
            Case CLCoilHydraulicIssueCode.InvalidCoolingTemperatures
                Return LocalizedText(
                    "MainForm_CoilPerformance_InvalidCoolingTemperatures",
                    "Cooling water outlet temperature must be at least 1 K higher than inlet temperature.")
            Case CLCoilHydraulicIssueCode.InvalidHeatingTemperatures
                Return LocalizedText(
                    "MainForm_CoilPerformance_InvalidHeatingTemperatures",
                    "Heating water inlet temperature must be at least 1 K higher than outlet temperature.")
            Case CLCoilHydraulicIssueCode.LowWaterDeltaT
                Return LocalizedText(
                    "MainForm_CoilPerformance_LowWaterDeltaT",
                    "Low water delta T (3 K or more, less than 5 K). Water flow is higher than typical.")
            Case CLCoilHydraulicIssueCode.CriticalWaterDeltaT
                Return LocalizedText(
                    "MainForm_CoilPerformance_CriticalWaterDeltaT",
                    "Very low water delta T (less than 3 K). Excessive water flow and pressure drop. Verify hydraulic design.")
            Case Else
                Return code.ToString()
        End Select
    End Function

    Private Shared Function MapWaterCoil(coil As CLCoilDefinition) As CLNextUiWaterCoilSummary
        Return New CLNextUiWaterCoilSummary With {
            .Id = coil.Id,
            .Name = coil.Name,
            .Mode = coil.Mode.ToString(),
            .Installation = coil.Installation.ToString(),
            .InstallationLabel = CoilInstallationLabel(coil.Installation),
            .LengthMm = coil.Length,
            .HeightMm = coil.Height,
            .Rows = coil.NumberOfRows,
            .Circuits = coil.NumberOfCircuits,
            .FinSpacingMm = coil.FinSpacingValue
        }
    End Function

    Private Shared Function CoilInstallationLabel(
        installation As CLCoilInstallationType) As String

        Select Case installation
            Case CLCoilInstallationType.Internal
                Return LocalizedText("MainForm_CoilPerformance_Internal", "Internal")
            Case CLCoilInstallationType.RequestedInternal
                Return LocalizedText(
                    "MainForm_CoilPerformance_RequestInternal",
                    "Request internal")
            Case Else
                Return LocalizedText("MainForm_CoilPerformance_External", "External")
        End Select
    End Function

    Private Shared Function MapElectricHeater(
        heater As CLElectricHeaterDefinition) As CLNextUiElectricHeaterSummary
        Return New CLNextUiElectricHeaterSummary With {
            .Id = heater.Id,
            .Code = heater.Code,
            .Name = heater.Name,
            .Mode = heater.Mode.ToString(),
            .Installation = heater.Installation.ToString(),
            .PowerW = heater.TotalPowerW,
            .VoltageV = heater.VoltageV,
            .CurrentA = heater.TotalCurrentA,
            .PhaseCount = heater.PhaseCount,
            .Quantity = heater.Quantity,
            .IsDefault = heater.IsDefault
        }
    End Function

    Private Shared Function MapWaterCoilResult(
        result As CLCoilCalculationResult) As CLWaterCoilResult
        Return New CLWaterCoilResult With {
            .ScenarioCode = If(result.Mode = CLCoilPerformanceMode.CWD, "Summer", "Winter"),
            .Mode = result.Mode.ToString(),
            .StatusCode = If(result.IsOk, "OK", If(String.IsNullOrWhiteSpace(result.ErrorMessage), result.Auxiliary.ToString(), result.ErrorMessage)),
            .CapacityW = result.HeatTransferred,
            .SensibleCapacityW = result.SensibleHeat,
            .AirOutletTemperatureC = result.OutletTemperature,
            .AirOutletRelativeHumidityPercent = result.OutletRH,
            .CondensateLitersPerHour = result.CondensedWater,
            .AirPressureDropPa = result.AirPressureDrop,
            .FluidPressureDropKPa = result.WaterPressureDrop,
            .FluidFlowLitersPerHour = result.FluidFlow,
            .FluidVelocityMetersPerSecond = result.FluidSpeed,
            .FaceVelocityMetersPerSecond = result.FaceVelocity
        }
    End Function

    Private Shared Function GetAccessories(
        model As CLDCHeatRecoveryModel,
        requestedCodes As IEnumerable(Of String)) As List(Of CLNextUiAccessorySummary)

        Dim languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        Dim items = CLSelectionCatalogRepository.GetEffectiveItems(
                CLEnvironment.Current.DCLiteDatabasePath,
                model.Id,
                languageCode).
            Where(Function(item) Not String.Equals(
                item.ItemType, "ControlFunction", StringComparison.OrdinalIgnoreCase)).
            ToList()
        Dim selected As New HashSet(Of Integer)(
            items.Where(Function(item) item.IsStandard OrElse item.DefaultSelected).
                Select(Function(item) item.Id))

        For Each code In If(requestedCodes, Enumerable.Empty(Of String)())
            Dim item = items.FirstOrDefault(Function(candidate) String.Equals(
                candidate.Code, code, StringComparison.OrdinalIgnoreCase))
            If item Is Nothing Then Continue For
            If String.Equals(item.Availability, "Unavailable", StringComparison.OrdinalIgnoreCase) OrElse
                (Not item.CustomerSelectable AndAlso Not item.IsStandard) Then Continue For
            If Not String.IsNullOrWhiteSpace(item.ExclusiveGroupCode) AndAlso items.Any(
                Function(candidate) candidate.Id <> item.Id AndAlso candidate.IsStandard AndAlso
                    String.Equals(candidate.ExclusiveGroupCode, item.ExclusiveGroupCode,
                        StringComparison.OrdinalIgnoreCase)) Then Continue For
            If Not String.IsNullOrWhiteSpace(item.ExclusiveGroupCode) Then
                For Each grouped In items.Where(Function(candidate) candidate.Id <> item.Id AndAlso
                    String.Equals(candidate.ExclusiveGroupCode, item.ExclusiveGroupCode,
                        StringComparison.OrdinalIgnoreCase))
                    selected.Remove(grouped.Id)
                Next
            End If
            selected.Add(item.Id)
        Next
        NormalizeAccessorySelection(items, selected)

        Return items.Select(Function(item)
            Dim disabledReason = AccessoryDisabledReason(items, selected, item)
            Dim requiredReason = AccessoryRequiredReason(items, selected, item)
            Return New CLNextUiAccessorySummary With {
                .Code = item.Code,
                .ItemType = item.ItemType,
                .Name = item.Name,
                .Description = item.Description,
                .Category = item.CategoryName,
                .Availability = item.Availability,
                .Installation = item.InstallationType,
                .FunctionNames = item.FunctionNames.ToList(),
                .Included = selected.Contains(item.Id),
                .Locked = item.IsStandard OrElse Not item.CustomerSelectable OrElse
                    Not String.IsNullOrWhiteSpace(requiredReason),
                .Enabled = String.IsNullOrWhiteSpace(disabledReason),
                .DisabledReason = If(
                    String.IsNullOrWhiteSpace(disabledReason), requiredReason, disabledReason)
            }
        End Function).ToList()
    End Function

    Private Shared Sub NormalizeAccessorySelection(
        items As List(Of CLSelectionCatalogItem),
        selected As HashSet(Of Integer))

        For Each item In items.Where(Function(candidate) candidate.IsStandard)
            selected.Add(item.Id)
        Next
        For Each group In items.Where(Function(item) selected.Contains(item.Id) AndAlso
            Not String.IsNullOrWhiteSpace(item.ExclusiveGroupCode)).GroupBy(Function(item) _
                item.ExclusiveGroupCode, StringComparer.OrdinalIgnoreCase)
            Dim standards = group.Where(Function(item) item.IsStandard).ToList()
            If standards.Count > 1 Then Throw New InvalidOperationException("Multiple standard accessories in group " & group.Key)
            Dim keep = If(standards.FirstOrDefault(), group.First())
            For Each item In group.Where(Function(candidate) candidate.Id <> keep.Id)
                selected.Remove(item.Id)
            Next
        Next
        Dim visited As New HashSet(Of String)(StringComparer.Ordinal)
        Dim changed As Boolean
        Do
            Dim signature = String.Join(",", selected.OrderBy(Function(id) id).Select(Function(id) id.ToString()).ToArray())
            If Not visited.Add(signature) Then Throw New InvalidOperationException("Cyclic accessory dependencies in the catalog.")
            changed = False
            For Each item In items.Where(Function(candidate) selected.Contains(candidate.Id)).ToArray()
                For Each dependency In item.Dependencies
                    If IsAutomaticDependency(dependency.DependencyType) AndAlso
                        Not selected.Contains(dependency.TargetItemId) Then
                        Dim target = items.FirstOrDefault(
                            Function(candidate) candidate.Id = dependency.TargetItemId)
                        If target Is Nothing OrElse String.Equals(target.Availability, "Unavailable", StringComparison.OrdinalIgnoreCase) Then
                            If item.IsStandard Then Throw New InvalidOperationException("Unavailable dependency for standard accessory " & item.Code)
                            selected.Remove(item.Id)
                            changed = True
                            Exit For
                        End If
                        If target IsNot Nothing AndAlso
                            Not String.IsNullOrWhiteSpace(target.ExclusiveGroupCode) Then
                            For Each grouped In items.Where(Function(candidate) candidate.Id <> target.Id AndAlso
                                String.Equals(candidate.ExclusiveGroupCode, target.ExclusiveGroupCode,
                                    StringComparison.OrdinalIgnoreCase))
                                selected.Remove(grouped.Id)
                                If grouped.IsStandard Then Throw New InvalidOperationException("Accessory dependency conflicts with standard " & grouped.Code)
                            Next
                        End If
                        selected.Add(dependency.TargetItemId)
                        changed = True
                    ElseIf String.Equals(
                        dependency.DependencyType, "Enables",
                        StringComparison.OrdinalIgnoreCase) AndAlso
                        Not selected.Contains(dependency.TargetItemId) Then
                        selected.Remove(item.Id)
                        changed = True
                    End If
                Next
            Next
            For Each item In items.Where(Function(candidate) selected.Contains(candidate.Id)).ToArray()
                Dim reason = AccessoryDisabledReason(items, selected, item, True)
                If Not String.IsNullOrWhiteSpace(reason) Then
                    If item.IsStandard Then Throw New InvalidOperationException(item.Code & ": " & reason)
                    selected.Remove(item.Id)
                    changed = True
                End If
            Next
        Loop While changed
    End Sub

    Private Shared Function AccessoryDisabledReason(
        items As List(Of CLSelectionCatalogItem),
        selected As HashSet(Of Integer),
        item As CLSelectionCatalogItem,
        Optional ignoreSelectionLock As Boolean = False) As String

        If String.Equals(item.Availability, "Unavailable", StringComparison.OrdinalIgnoreCase) Then
            Return LocalizedText("MainForm_Accessories_Unavailable", "Not available for this unit.")
        End If
        If Not ignoreSelectionLock AndAlso Not item.CustomerSelectable AndAlso Not item.IsStandard Then
            Return LocalizedText("MainForm_Accessories_NotSelectable", "This option cannot be selected.")
        End If
        If Not String.Equals(item.ExclusiveGroupCode, "KTS", StringComparison.OrdinalIgnoreCase) Then
            Dim controller As CLSelectionCatalogItem = Nothing
            For Each candidate In items
                If selected.Contains(candidate.Id) AndAlso
                    String.Equals(candidate.ExclusiveGroupCode, "KTS",
                        StringComparison.OrdinalIgnoreCase) Then
                    controller = candidate
                    Exit For
                End If
            Next
            If item.MinimumControllerLevel > 0 AndAlso (controller Is Nothing OrElse
                controller.ControllerLevel < item.MinimumControllerLevel) Then
                Return LocalizedText(
                    "MainForm_Accessories_RequiresExtraController",
                    "Requires KTS Extra or higher.")
            End If
        End If
        For Each dependency In item.Dependencies
            If ignoreSelectionLock AndAlso IsAutomaticDependency(dependency.DependencyType) AndAlso
                Not selected.Contains(dependency.TargetItemId) Then Return "Missing dependency: " & dependency.TargetCode
            If String.Equals(dependency.DependencyType, "Enables",
                StringComparison.OrdinalIgnoreCase) AndAlso
                Not selected.Contains(dependency.TargetItemId) Then
                Return String.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedText(
                        "MainForm_Accessories_EnableFirst",
                        "Select {0} first."),
                    dependency.TargetCode)
            End If
            If String.Equals(dependency.DependencyType, "Conflicts",
                StringComparison.OrdinalIgnoreCase) AndAlso
                selected.Contains(dependency.TargetItemId) Then
                Return String.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedText(
                        "MainForm_Accessories_ConflictsWith",
                        "Not compatible with {0}."),
                    dependency.TargetCode)
            End If
        Next
        Dim reverseConflict As CLSelectionCatalogItem = Nothing
        For Each source In items
            If Not selected.Contains(source.Id) Then Continue For
            For Each dependency In source.Dependencies
                If String.Equals(dependency.DependencyType, "Conflicts",
                    StringComparison.OrdinalIgnoreCase) AndAlso
                    dependency.TargetItemId = item.Id Then
                    reverseConflict = source
                    Exit For
                End If
            Next
            If reverseConflict IsNot Nothing Then Exit For
        Next
        If reverseConflict IsNot Nothing Then
            Return String.Format(
                CultureInfo.CurrentCulture,
                LocalizedText(
                    "MainForm_Accessories_ConflictsWith",
                    "Not compatible with {0}."),
                reverseConflict.Code)
        End If
        If String.Equals(item.ExclusiveGroupCode, "KTS", StringComparison.OrdinalIgnoreCase) Then
            Dim requiring As New List(Of String)()
            For Each candidate In items
                If selected.Contains(candidate.Id) AndAlso
                    candidate.MinimumControllerLevel > item.ControllerLevel Then
                    requiring.Add(candidate.Code)
                End If
            Next
            If requiring.Count > 0 Then
                Return String.Format(
                    CultureInfo.CurrentCulture,
                    LocalizedText(
                        "MainForm_Accessories_ControllerLevel",
                        "Requires a higher controller level because of: {0}."),
                    String.Join(", ", requiring.ToArray()))
            End If
        End If
        Return String.Empty
    End Function

    Private Shared Function AccessoryRequiredReason(
        items As List(Of CLSelectionCatalogItem),
        selected As HashSet(Of Integer),
        item As CLSelectionCatalogItem) As String

        Dim requiring As New List(Of String)()
        For Each source In items
            If Not selected.Contains(source.Id) Then Continue For
            For Each dependency In source.Dependencies
                If IsAutomaticDependency(dependency.DependencyType) AndAlso
                    dependency.TargetItemId = item.Id Then
                    requiring.Add(source.Code)
                    Exit For
                End If
            Next
        Next
        If requiring.Count = 0 Then Return String.Empty
        Return String.Format(
            CultureInfo.CurrentCulture,
            LocalizedText("MainForm_Accessories_RequiredBy", "Required by: {0}."),
            String.Join(", ", requiring.ToArray()))
    End Function

    Private Shared Function IsAutomaticDependency(dependencyType As String) As Boolean
        Return String.Equals(dependencyType, "Requires", StringComparison.OrdinalIgnoreCase) OrElse
            String.Equals(dependencyType, "Includes", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Sub EnsureLanguage(languageCode As String)
        Dim normalized = If(languageCode, String.Empty).Trim().ToLowerInvariant()
        Dim language = CLEnvironment.Current.FindLanguage(normalized)
        If language Is Nothing OrElse Not language.Enabled Then
            language = CLEnvironment.Current.FindLanguage(
                CLEnvironment.Current.SSWInfo.DefaultLanguage)
        End If
        If language Is Nothing OrElse Not language.Enabled Then
            language = CLEnvironment.Current.ENLanguage
        End If
        If Not String.Equals(
            CLEnvironment.Current.PrimaryLanguageCode,
            language.Code,
            StringComparison.OrdinalIgnoreCase) Then
            CLEnvironment.Current.SetLanguage(language)
        End If
    End Sub

    Private Shared Function PressureCapacityExceededText() As String
        Select Case CLEnvironment.Current.PrimaryLanguageCode.ToLowerInvariant()
            Case "bg"
                Return "Необходимите дебит и налягане не могат да бъдат достигнати дори при 100% регулиране."
            Case "cs"
                Return "Požadovaného průtoku a tlaku nelze dosáhnout ani při 100% regulaci."
            Case "da"
                Return "Den krævede luftmængde og det krævede tryk kan ikke nås, selv ved 100 % regulering."
            Case "de"
                Return "Der erforderliche Volumenstrom und Druck können auch bei 100 % Regelung nicht erreicht werden."
            Case "fr"
                Return "Le débit et la pression demandés ne peuvent pas être atteints, même avec une régulation à 100 %."
            Case "hu"
                Return "A szükséges légszállítás és nyomás 100%-os szabályozás mellett sem érhető el."
            Case "is"
                Return "Ekki er hægt að ná tilskildu loftmagni og þrýstingi, jafnvel við 100% stýringu."
            Case "it"
                Return "La portata e la pressione richieste non sono raggiungibili nemmeno con la regolazione al 100%."
            Case "nl"
                Return "Het vereiste luchtdebiet en de vereiste druk kunnen zelfs bij 100% regeling niet worden bereikt."
            Case "no"
                Return "Nødvendig luftmengde og trykk kan ikke oppnås selv ved 100 % regulering."
            Case "pl"
                Return "Wymaganego przepływu i ciśnienia nie można osiągnąć nawet przy regulacji 100%."
            Case "ro"
                Return "Debitul și presiunea necesare nu pot fi atinse nici la o reglare de 100%."
            Case "sl"
                Return "Zahtevanega pretoka in tlaka ni mogoče doseči niti pri 100-odstotni regulaciji."
            Case "sv"
                Return "Krävt luftflöde och tryck kan inte uppnås ens vid 100 % reglering."
            Case Else
                Return "The required airflow and pressure cannot be reached even at 100% regulation."
        End Select
    End Function

    Private Shared Function LocalizedText(resourceName As String, fallback As String) As String
        Try
            Dim value = CLEnvironment.Current.Localization.GetString(resourceName)
            If Not String.IsNullOrWhiteSpace(value) AndAlso
                value.IndexOf(
                    "PrimaryCulture not set",
                    StringComparison.OrdinalIgnoreCase) < 0 AndAlso
                Not String.Equals(value, resourceName, StringComparison.OrdinalIgnoreCase) Then
                Return value
            End If
        Catch
        End Try
        Return fallback
    End Function

    Private Shared Function MapModel(model As CLDCHeatRecoveryModel) As CLNextUiModelSummary
        Return New CLNextUiModelSummary With {
            .Id = model.Id,
            .Code = model.Code,
            .Name = CLEnvironment.Current.GetCustomerHeatRecoveryModelName(model),
            .SeriesCode = If(model.CLSerie Is Nothing, String.Empty, model.CLSerie.Code),
            .NominalAirflowM3h = model.NominalAirflow.GetValueOrDefault(),
            .StaticPressurePa = model.StaticPressure.GetValueOrDefault(),
            .AeraulicConnectionCode = If(model.CLEnumItem_AeraulicConnection Is Nothing,
                String.Empty,
                model.CLEnumItem_AeraulicConnection.TextCode)
        }
    End Function

    Private Shared Function CalculatePreselectionCandidate(
        model As CLDCHeatRecoveryModel,
        requestedAirflow As Double,
        requestedPressure As Double,
        filters As CLNextUiPreselectionFilters) As CLNextUiPreselectionSummary

        Try
            Dim operatingPoint = CLSelectionApplicationService.FindCompatibleFanOperatingPoint(
                model, requestedAirflow, requestedPressure, 70)
            If operatingPoint Is Nothing Then Return Nothing
            Dim combinedSfp = If(requestedAirflow > 0,
                2 * operatingPoint.PowerW * 3.6R / requestedAirflow, Double.MaxValue)
            If Double.IsNaN(combinedSfp) OrElse Double.IsInfinity(combinedSfp) OrElse
                combinedSfp <= 0 Then Return Nothing

            If filters Is Nothing Then filters = New CLNextUiPreselectionFilters()
            If filters.MaximumSfpEnabled AndAlso
                combinedSfp > Math.Max(0, filters.MaximumSfp) Then Return Nothing

            Dim supplyLwa As Double? = Nothing
            Dim supplyLpa As Double? = Nothing
            Dim breakoutLwa As Double? = Nothing
            Dim breakoutLpa As Double? = Nothing
            If filters.SupplyNoiseEnabled OrElse filters.BreakoutNoiseEnabled Then
                Dim sound = CLNextUiIndoorQualityService.CalculateSound(
                    model, operatingPoint.RegulationPercent, requestedAirflow,
                    requestedPressure, New CLNextUiSoundInput())
                Dim supply = sound.Rows.FirstOrDefault(Function(row) row.Type = "Supply")
                Dim breakout = sound.Rows.FirstOrDefault(Function(row) row.Type = "Breakout")

                If supply IsNot Nothing Then
                    supplyLwa = supply.LwA
                    supplyLpa = CLNextUiIndoorQualityService.CalculateSoundPressure(
                        supply.LwA, filters.SupplyNoiseDirectivity,
                        filters.SupplyNoiseDistanceMeters)
                End If
                If breakout IsNot Nothing Then
                    breakoutLwa = breakout.LwA
                    breakoutLpa = CLNextUiIndoorQualityService.CalculateSoundPressure(
                        breakout.LwA, filters.BreakoutNoiseDirectivity,
                        filters.BreakoutNoiseDistanceMeters)
                End If

                If filters.SupplyNoiseEnabled AndAlso Not NoiseCriterionSatisfied(
                    filters.SupplyNoiseMetric, filters.MaximumSupplyNoiseDbA,
                    supplyLwa, supplyLpa) Then Return Nothing
                If filters.BreakoutNoiseEnabled AndAlso Not NoiseCriterionSatisfied(
                    filters.BreakoutNoiseMetric, filters.MaximumBreakoutNoiseDbA,
                    breakoutLwa, breakoutLpa) Then Return Nothing
            End If

            Return New CLNextUiPreselectionSummary With {
                .Model = MapModel(model),
                .RequiredRegulationPercent = operatingPoint.RegulationPercent,
                .AvailablePressurePa = operatingPoint.PressurePa,
                .AbsorbedPowerW = operatingPoint.PowerW,
                .CombinedSfp = combinedSfp,
                .SupplySoundPowerDbA = supplyLwa,
                .SupplySoundPressureDbA = supplyLpa,
                .BreakoutSoundPowerDbA = breakoutLwa,
                .BreakoutSoundPressureDbA = breakoutLpa
            }
        Catch
            Return Nothing
        End Try
    End Function

    Private Shared Function NoiseCriterionSatisfied(
        metric As String,
        maximumDbA As Double,
        soundPowerDbA As Double?,
        soundPressureDbA As Double?) As Boolean

        Dim measured = If(String.Equals(metric, "LPA", StringComparison.OrdinalIgnoreCase),
            soundPressureDbA, soundPowerDbA)
        Return measured.HasValue AndAlso measured.Value <= Math.Max(0, maximumDbA)
    End Function
End Class
