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
End Class

Public NotInheritable Class CLNextUiAccessorySummary
    Public Property Code As String
    Public Property Name As String
    Public Property Category As String
    Public Property Installation As String
    Public Property Included As Boolean
    Public Property Locked As Boolean
End Class

Public NotInheritable Class CLNextUiWaterCoilSummary
    Public Property Id As Integer
    Public Property Name As String
    Public Property Mode As String
    Public Property Installation As String
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
    Public Property WaterCoilLengthMm As Integer
    Public Property WaterCoilHeightMm As Integer
    Public Property WaterCoilRows As Integer
    Public Property WaterCoilCircuits As Integer
    Public Property WaterCoilFinSpacingMm As Double
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
    Public Property AdditionalPressureDropPa As Double
    Public Property Validation As New CLValidationResult()
End Class

Public NotInheritable Class CLNextUiApplicationService
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
        If input.SupplyAirflowM3h <= 0 Then
            Return New List(Of CLNextUiPreselectionSummary)()
        End If

        Dim requestedPressure = Math.Max(0, input.PressurePa)
        Return CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            ToList().
            Where(Function(model) Not String.Equals(model.Code, "ACC", StringComparison.OrdinalIgnoreCase) AndAlso
                                  Not String.Equals(model.Code, "IOM3", StringComparison.OrdinalIgnoreCase)).
            Select(Function(model) CalculatePreselectionCandidate(
                model, input.SupplyAirflowM3h, requestedPressure)).
            Where(Function(candidate) candidate IsNot Nothing).
            OrderBy(Function(candidate) candidate.CombinedSfp).
            ThenBy(Function(candidate) candidate.RequiredRegulationPercent).
            ThenBy(Function(candidate) candidate.Model.NominalAirflowM3h).
            ThenBy(Function(candidate) candidate.Model.Code).
            ToList()
    End Function

    Public Shared Function Calculate(input As CLNextUiCalculationInput) As CLNextUiCalculationResult
        If input Is Nothing Then Throw New ArgumentNullException("input")
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

        Dim winter = CalculateSeason(
            model, "Winter", airflow, pressure, input.RegulationPercent,
            input.WinterOutdoorTemperatureC, input.WinterOutdoorRhPercent,
            input.WinterReturnTemperatureC, input.WinterReturnRhPercent,
            0, preheatPower)
        Dim summer = CalculateSeason(
            model, "Summer", airflow, pressure, input.RegulationPercent,
            input.SummerOutdoorTemperatureC, input.SummerOutdoorRhPercent,
            input.SummerReturnTemperatureC, input.SummerReturnRhPercent,
            0, 0)

        Dim waterResults As New List(Of CLWaterCoilResult)()
        Dim electricResults As New List(Of CLElectricHeaterResult)()
        Dim additionalPressureDrop = CalculateAirTreatment(
            input, model, airflow, coils, selectedPreheater, selectedPostheater,
            winter, summer, waterResults, electricResults)

        If additionalPressureDrop > 0 Then
            winter = CalculateSeason(
                model, "Winter", airflow, pressure, input.RegulationPercent,
                input.WinterOutdoorTemperatureC, input.WinterOutdoorRhPercent,
                input.WinterReturnTemperatureC, input.WinterReturnRhPercent,
                additionalPressureDrop, preheatPower)
            summer = CalculateSeason(
                model, "Summer", airflow, pressure, input.RegulationPercent,
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

        Return New CLNextUiCalculationResult With {
            .Model = MapModel(model),
            .Winter = winter,
            .Summer = summer,
            .Layout = New CLLegacySdfInstallationLayoutRepository().GetForModel(model),
            .Accessories = GetAccessories(model),
            .AvailableWaterCoils = coils.Select(Function(item) MapWaterCoil(item)).ToList(),
            .AvailableElectricHeaters = heaters.Select(Function(item) MapElectricHeater(item)).ToList(),
            .WaterCoilResults = waterResults,
            .ElectricHeaterResults = electricResults,
            .AdditionalPressureDropPa = additionalPressureDrop,
            .Validation = ValidateAirTreatment(input, selectedPostheater)
        }
    End Function

    Public Shared Function CreateProjectDocument(
        input As CLNextUiCalculationInput) As CLSelectionProjectDocument

        If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
        Dim model = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(item) item.Code = input.ModelCode)
        If model Is Nothing Then
            Throw New InvalidOperationException("The selected unit is not available in the local SDF.")
        End If

        Dim document = CLSelectionProjectSerializer.CreateNew(
            CLEnvironment.Current.DatabaseCompatibility)
        document.Selection.CustomerReference = input.CustomerReference
        document.Selection.Unit = New CLSelectionEntityReference With {
            .Id = model.Id,
            .Code = model.Code,
            .Name = model.Name
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

        Dim coil = SelectCoil(
            CLCoilPerformanceCalculator.GetAvailableCoils(model), input.WaterCoilId)
        document.Selection.WaterCoil.Enabled = input.WaterCoilEnabled AndAlso coil IsNot Nothing
        document.Selection.WaterCoil.CalculationMode = input.WaterCoilMode
        document.Selection.WaterCoil.SelectionCase = If(
            input.WaterCoilCustomized, "Customized", "Standard")
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

        Dim availableAccessories = GetAccessories(model)
        For Each code In If(input.AccessoryCodes, New List(Of String)())
            Dim item = availableAccessories.FirstOrDefault(
                Function(candidate) String.Equals(
                    candidate.Code, code, StringComparison.OrdinalIgnoreCase))
            If item Is Nothing Then Continue For
            document.Selection.Accessories.Add(New CLAccessorySelection With {
                .Code = item.Code,
                .Quantity = 1,
                .InstallationType = item.Installation,
                .LocalizedDisplayName = item.Name,
                .LocalizedDescription = item.Name
            })
        Next
        Return document
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
                If input.ElectricPostheaterEnabled AndAlso postheater IsNot Nothing AndAlso
                    mode <> CLCoilPerformanceMode.CWD Then
                    mode = CLCoilPerformanceMode.CWD
                End If
                Dim coilInput As New CLCoilCalculationInput With {
                    .Coil = coil,
                    .CalculationMode = mode,
                    .AirFlow = airflow,
                    .UseModeAirInletConditions = True,
                    .CoolingAirInletTemperature = summer.Result.Thermodynamics.SupplyOutletTemperatureC.GetValueOrDefault(),
                    .CoolingAirInletRH = summer.Result.Thermodynamics.SupplyOutletRelativeHumidityPercent.GetValueOrDefault(),
                    .HeatingAirInletTemperature = winter.Result.Thermodynamics.SupplyOutletTemperatureC.GetValueOrDefault(),
                    .HeatingAirInletRH = winter.Result.Thermodynamics.SupplyOutletRelativeHumidityPercent.GetValueOrDefault(),
                    .FluidType = ParseFluidType(input.FluidCode),
                    .FluidTypeTec = input.GlycolPercent,
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

    Private Shared Function SelectCoil(
        coils As List(Of CLCoilDefinition),
        id As Integer) As CLCoilDefinition

        Dim selected = coils.FirstOrDefault(Function(item) item.Id = id)
        If selected Is Nothing Then selected = coils.FirstOrDefault()
        Return If(selected Is Nothing, Nothing, selected.Clone())
    End Function

    Private Shared Function SelectHeater(
        heaters As List(Of CLElectricHeaterDefinition),
        mode As CLElectricHeaterMode,
        id As Integer) As CLElectricHeaterDefinition

        Dim available = heaters.Where(Function(item) item.Mode = mode).ToList()
        Dim selected = available.FirstOrDefault(Function(item) item.Id = id)
        If selected Is Nothing Then
            selected = available.FirstOrDefault(Function(item) item.IsDefault)
        End If
        If selected Is Nothing Then selected = available.FirstOrDefault()
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
        postheater As CLElectricHeaterDefinition) As CLValidationResult

        Dim validation As New CLValidationResult()
        If input.WaterCoilEnabled AndAlso input.ElectricPostheaterEnabled AndAlso
            postheater IsNot Nothing AndAlso
            Not String.Equals(input.WaterCoilMode, "CWD", StringComparison.OrdinalIgnoreCase) Then
            validation.Issues.Add(New CLValidationIssue With {
                .Code = "WaterHeatingElectricPostheaterConflict",
                .Severity = CLValidationSeverity.Warning,
                .Path = "WaterCoil.CalculationMode",
                .MessageKey = "validation.waterEhdConflict"
            })
        End If
        Return validation
    End Function

    Private Shared Function MapWaterCoil(coil As CLCoilDefinition) As CLNextUiWaterCoilSummary
        Return New CLNextUiWaterCoilSummary With {
            .Id = coil.Id,
            .Name = coil.Name,
            .Mode = coil.Mode.ToString(),
            .Installation = coil.Installation.ToString(),
            .LengthMm = coil.Length,
            .HeightMm = coil.Height,
            .Rows = coil.NumberOfRows,
            .Circuits = coil.NumberOfCircuits,
            .FinSpacingMm = coil.FinSpacingValue
        }
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

    Private Shared Function GetAccessories(model As CLDCHeatRecoveryModel) As List(Of CLNextUiAccessorySummary)
        Dim languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        Return CLSelectionCatalogRepository.GetEffectiveItems(
                CLEnvironment.Current.DCLiteDatabasePath,
                model.Id,
                languageCode).
            Where(Function(item) Not String.Equals(item.ItemType, "ControlFunction", StringComparison.OrdinalIgnoreCase)).
            Select(Function(item) New CLNextUiAccessorySummary With {
                .Code = item.Code,
                .Name = item.Name,
                .Category = item.CategoryName,
                .Installation = item.InstallationType,
                .Included = item.DefaultSelected OrElse item.IsStandard,
                .Locked = item.IsStandard OrElse Not item.CustomerSelectable
            }).
            ToList()
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
        requestedPressure As Double) As CLNextUiPreselectionSummary

        Try
            Dim operatingPoint = CLSelectionApplicationService.FindCompatibleFanOperatingPoint(
                model, requestedAirflow, requestedPressure, 70)
            If operatingPoint Is Nothing Then Return Nothing
            Dim combinedSfp = If(requestedAirflow > 0,
                2 * operatingPoint.PowerW * 3.6R / requestedAirflow, Double.MaxValue)
            If Double.IsNaN(combinedSfp) OrElse Double.IsInfinity(combinedSfp) OrElse
                combinedSfp <= 0 Then Return Nothing

            Return New CLNextUiPreselectionSummary With {
                .Model = MapModel(model),
                .RequiredRegulationPercent = operatingPoint.RegulationPercent,
                .AvailablePressurePa = operatingPoint.PressurePa,
                .AbsorbedPowerW = operatingPoint.PowerW,
                .CombinedSfp = combinedSfp
            }
        Catch
            Return Nothing
        End Try
    End Function
End Class
