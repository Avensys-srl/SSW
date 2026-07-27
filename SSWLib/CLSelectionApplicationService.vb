Imports Climalombarda.DataCentral.LTModel
Imports System.Collections.Generic
Imports System.Linq

Public NotInheritable Class CLPerformanceCurveRequest

    Public Property MeasureUnit As CLMeasureUnit
    Public Property RequestedAirflow As Double
    Public Property Model As CLDCHeatRecoveryModel
    Public Property ReturnTemperatureC As Double
    Public Property ReturnRelativeHumidity As Double
    Public Property FreshTemperatureC As Double
    Public Property FreshRelativeHumidity As Double
    Public Property RegulationPercent As Integer
    Public Property ShowSfpArea As Boolean
    Public Property ShowErpArea As Boolean
    Public Property SfpLimit As Double
    Public Property ShowPassiveHouseArea As Boolean
    Public Property PassiveHouseLimit As Double
    Public Property AdditionalPressureDropPa As Double
    Public Property PressureDropReferenceAirflowM3h As Double

End Class

Public NotInheritable Class CLPerformanceCurveCalculation

    Public Property OriginalAirflows As Double()
    Public Property OriginalPressures As Double()
    Public Property OriginalPowers As Double()
    Public Property RegulatedAirflows As Double()
    Public Property RegulatedPressures As Double()
    Public Property RegulatedPowers As Double()
    Public Property EfficienciesPercent As Double()
    Public Property WorkingAreaAirflows As Double()
    Public Property WorkingAreaPressures As Double()
    Public Property WorkingPointAirflow As Double
    Public Property WorkingPointPressurePa As Double
    Public Property WorkingPointPowerW As Double
    Public Property WorkingPointEfficiencyPercent As Double

    Public Function ToLegacyWorkPoint() As Double()
        Return New Double() {
            WorkingPointEfficiencyPercent,
            WorkingPointAirflow,
            WorkingPointPressurePa,
            WorkingPointPowerW
        }
    End Function

End Class

Public NotInheritable Class CLBalancedScenarioCalculationRequest

    Public Property Scenario As CLSeasonCalculationInput
    Public Property Model As CLDCHeatRecoveryModel
    Public Property MeasureUnit As CLMeasureUnit
    Public Property AdditionalPressureDropPa As Double
    Public Property PreheatPowerW As Double
    Public Property ShowSfpArea As Boolean
    Public Property ShowErpArea As Boolean
    Public Property SfpLimit As Double
    Public Property ShowPassiveHouseArea As Boolean
    Public Property PassiveHouseLimit As Double

End Class

Public NotInheritable Class CLBalancedScenarioCalculation

    Public Property Result As New CLSeasonCalculationResult()
    Public Property Curves As New CLPerformanceCurveCalculation()
    Public Property EffectiveFreshTemperatureC As Double
    Public Property EffectiveFreshRelativeHumidity As Double
    Friend Property LegacyThermodynamics As termo

End Class

Public NotInheritable Class CLSelectionApplicationService

    Private Sub New()
    End Sub

    Public Shared Function CalculateBalancedScenario(request As CLBalancedScenarioCalculationRequest) As CLBalancedScenarioCalculation
        If request Is Nothing Then Throw New ArgumentNullException("request")
        If request.Scenario Is Nothing Then Throw New ArgumentNullException("request.Scenario")
        If request.Model Is Nothing Then Throw New ArgumentNullException("request.Model")

        Dim airflow As Double = If(request.Scenario.Airflows Is Nothing OrElse
            Not request.Scenario.Airflows.SupplyM3h.HasValue, 0, request.Scenario.Airflows.SupplyM3h.Value)
        If airflow = 0 Then airflow = 1

        Dim calculationAirflow As Double = If(request.MeasureUnit = CLMeasureUnit.IP, airflow * 3.6R, airflow)
        Dim returnTemperature As Double = request.Scenario.ReturnTemperatureC.GetValueOrDefault()
        Dim returnHumidity As Double = request.Scenario.ReturnRelativeHumidityPercent.GetValueOrDefault() / 100.0R
        Dim freshTemperature As Double = request.Scenario.OutdoorTemperatureC.GetValueOrDefault()
        Dim freshHumidity As Double = request.Scenario.OutdoorRelativeHumidityPercent.GetValueOrDefault() / 100.0R
        Dim originalFreshTemperature As Double = freshTemperature
        Dim originalFreshHumidity As psychro = PsychroCalc(freshTemperature, freshHumidity)

        If request.PreheatPowerW > 0 Then
            freshTemperature = originalFreshTemperature +
                CLElectricHeaterCalculator.TemperatureRise(request.PreheatPowerW, calculationAirflow)
            freshHumidity = PsychroCalcW(freshTemperature, originalFreshHumidity.w).rh
        End If

        Dim curves As CLPerformanceCurveCalculation = CalculatePerformanceCurve(New CLPerformanceCurveRequest With {
            .MeasureUnit = request.MeasureUnit,
            .RequestedAirflow = airflow,
            .Model = request.Model,
            .ReturnTemperatureC = returnTemperature,
            .ReturnRelativeHumidity = returnHumidity,
            .FreshTemperatureC = freshTemperature,
            .FreshRelativeHumidity = freshHumidity,
            .RegulationPercent = CInt(Math.Round(request.Scenario.RegulationPercent.GetValueOrDefault(100))),
            .ShowSfpArea = request.ShowSfpArea,
            .ShowErpArea = request.ShowErpArea,
            .SfpLimit = request.SfpLimit,
            .ShowPassiveHouseArea = request.ShowPassiveHouseArea,
            .PassiveHouseLimit = request.PassiveHouseLimit,
            .AdditionalPressureDropPa = request.AdditionalPressureDropPa,
            .PressureDropReferenceAirflowM3h = calculationAirflow
        })

        Dim thermoAirflow As Double = If(request.MeasureUnit = CLMeasureUnit.IP,
            curves.WorkingPointAirflow * 3.6R, curves.WorkingPointAirflow)
        If request.PreheatPowerW > 0 Then
            freshTemperature = originalFreshTemperature +
                CLElectricHeaterCalculator.TemperatureRise(request.PreheatPowerW, thermoAirflow)
            freshHumidity = PsychroCalcW(freshTemperature, originalFreshHumidity.w).rh
        End If

        Dim thermoWork As termo = termo_calc(
            returnTemperature,
            returnHumidity,
            freshTemperature,
            freshHumidity,
            thermoAirflow,
            request.Model.ModRec,
            CDbl(request.Model.LenRec),
            False)

        Dim branchSfp As Double? = Nothing
        If curves.WorkingPointAirflow > 0 Then
            If request.MeasureUnit = CLMeasureUnit.IP Then
                branchSfp = curves.WorkingPointPowerW / curves.WorkingPointAirflow
            Else
                branchSfp = curves.WorkingPointPowerW * 3.6R / curves.WorkingPointAirflow
            End If
        End If

        Dim seasonResult As New CLSeasonCalculationResult With {
            .ScenarioCode = request.Scenario.ScenarioCode,
            .SupplyBranch = New CLBranchCalculationResult With {
                .BranchCode = "Supply",
                .AirflowM3h = curves.WorkingPointAirflow,
                .AvailablePressurePa = curves.WorkingPointPressurePa,
                .AbsorbedPowerW = curves.WorkingPointPowerW,
                .SpecificFanPowerWPerM3hPerSecond = branchSfp
            },
            .ExtractBranch = New CLBranchCalculationResult With {
                .BranchCode = "Extract",
                .AirflowM3h = curves.WorkingPointAirflow,
                .AvailablePressurePa = curves.WorkingPointPressurePa,
                .AbsorbedPowerW = curves.WorkingPointPowerW,
                .SpecificFanPowerWPerM3hPerSecond = branchSfp
            },
            .CombinedSpecificFanPowerWPerM3hPerSecond = If(branchSfp.HasValue, branchSfp.Value * 2, CType(Nothing, Double?)),
            .Thermodynamics = MapThermodynamics(thermoWork)
        }

        Return New CLBalancedScenarioCalculation With {
            .Result = seasonResult,
            .Curves = curves,
            .EffectiveFreshTemperatureC = freshTemperature,
            .EffectiveFreshRelativeHumidity = freshHumidity,
            .LegacyThermodynamics = thermoWork
        }
    End Function

    Public Shared Function CalculatePerformanceCurve(request As CLPerformanceCurveRequest) As CLPerformanceCurveCalculation
        If request Is Nothing Then Throw New ArgumentNullException("request")
        If request.Model Is Nothing Then Throw New ArgumentNullException("request.Model")

        Dim airflows As Double() = DirectCast(request.Model.AirflowsItems.Clone(), Double())
        Dim pressures As Double() = DirectCast(request.Model.PressuresItems.Clone(), Double())
        Dim powers As Double() = DirectCast(request.Model.PowersItems.Clone(), Double())
        For index As Integer = 0 To powers.Length - 1
            powers(index) -= 3.5R
        Next

        Dim requestedAirflowSi As Double = If(request.MeasureUnit = CLMeasureUnit.IP,
            request.RequestedAirflow * 3.6R, request.RequestedAirflow)
        Dim interpolatedAirflows As Double() = BuildInterpolationAirflows(airflows.Max())
        Dim interpolatedPressures As Double() = Nothing
        Dim interpolatedPowers As Double() = Nothing
        alglib.spline1dconvcubic(airflows, pressures, interpolatedAirflows, interpolatedPressures)
        alglib.spline1dconvcubic(airflows, powers, interpolatedAirflows, interpolatedPowers)

        Dim originalAirflows As Double() = DirectCast(interpolatedAirflows.Clone(), Double())
        Dim originalPressures As Double() = DirectCast(interpolatedPressures.Clone(), Double())
        Dim originalPowers As Double() = DirectCast(interpolatedPowers.Clone(), Double())

        If request.RegulationPercent <> 100 Then
            Dim regulationFactor As Double = request.RegulationPercent / 100.0R
            For index As Integer = 0 To interpolatedAirflows.Length - 1
                interpolatedAirflows(index) *= regulationFactor
                interpolatedPressures(index) *= regulationFactor ^ 2
                interpolatedPowers(index) *= regulationFactor ^ 3
            Next
        End If

        If request.AdditionalPressureDropPa > 0 Then
            Dim referenceAirflow As Double = If(request.PressureDropReferenceAirflowM3h > 0,
                request.PressureDropReferenceAirflowM3h, requestedAirflowSi)
            If referenceAirflow > 0 Then
                ApplyQuadraticPressureDrop(interpolatedAirflows, interpolatedPressures,
                    request.AdditionalPressureDropPa, referenceAirflow)
                ApplyQuadraticPressureDrop(originalAirflows, originalPressures,
                    request.AdditionalPressureDropPa, referenceAirflow)
            End If
        End If

        TrimPressureCurveAtZero(interpolatedAirflows, interpolatedPressures, interpolatedPowers)
        TrimPressureCurveAtZero(originalAirflows, originalPressures, originalPowers)

        Dim efficiencies(originalAirflows.Length - 1) As Double
        For index As Integer = 0 To originalAirflows.Length - 1
            Dim thermal As termo = termo_calc(
                request.ReturnTemperatureC,
                request.ReturnRelativeHumidity,
                request.FreshTemperatureC,
                request.FreshRelativeHumidity,
                originalAirflows(index),
                request.Model.ModRec,
                CDbl(request.Model.LenRec),
                False)
            efficiencies(index) = 100 * thermal.efficiency
        Next

        Dim calculatedAirflow As Double = Math.Min(requestedAirflowSi, interpolatedAirflows(interpolatedAirflows.Length - 1))
        Dim upperIndex As Integer = GetUpperIndex(interpolatedAirflows, calculatedAirflow)
        Dim workingPressure As Double = Math.Max(0,
            InterpolateCurveValue(interpolatedAirflows, interpolatedPressures, calculatedAirflow, upperIndex))
        Dim workingPower As Double =
            InterpolateCurveValue(interpolatedAirflows, interpolatedPowers, calculatedAirflow, upperIndex)
        Dim efficiencyAirflow As Double = Math.Min(calculatedAirflow, originalAirflows(originalAirflows.Length - 1))
        Dim efficiencyIndex As Integer = GetUpperIndex(originalAirflows, efficiencyAirflow)
        Dim workingEfficiency As Double =
            InterpolateCurveValue(originalAirflows, efficiencies, efficiencyAirflow, efficiencyIndex)

        Dim displayOriginalAirflows As Double() = DirectCast(originalAirflows.Clone(), Double())
        Dim displayRegulatedAirflows As Double() = DirectCast(interpolatedAirflows.Clone(), Double())
        Dim displayWorkingAirflow As Double = calculatedAirflow
        If request.MeasureUnit = CLMeasureUnit.IP Then
            ConvertAirflowsToIp(displayOriginalAirflows)
            ConvertAirflowsToIp(displayRegulatedAirflows)
            displayWorkingAirflow /= 3.6R
        End If

        Dim workingAreaAirflows As New List(Of Double)()
        Dim workingAreaPressures As New List(Of Double)()
        BuildWorkingArea(request, displayRegulatedAirflows, interpolatedPressures,
            interpolatedPowers, efficiencies, workingAreaAirflows, workingAreaPressures)

        Return New CLPerformanceCurveCalculation With {
            .OriginalAirflows = displayOriginalAirflows,
            .OriginalPressures = originalPressures,
            .OriginalPowers = originalPowers,
            .RegulatedAirflows = displayRegulatedAirflows,
            .RegulatedPressures = interpolatedPressures,
            .RegulatedPowers = interpolatedPowers,
            .EfficienciesPercent = efficiencies,
            .WorkingAreaAirflows = workingAreaAirflows.ToArray(),
            .WorkingAreaPressures = workingAreaPressures.ToArray(),
            .WorkingPointAirflow = displayWorkingAirflow,
            .WorkingPointPressurePa = workingPressure,
            .WorkingPointPowerW = workingPower,
            .WorkingPointEfficiencyPercent = workingEfficiency
        }
    End Function

    Private Shared Function MapThermodynamics(source As termo) As CLThermodynamicCalculationResult
        Return New CLThermodynamicCalculationResult With {
            .HeatTransferredW = source.heat_recovery,
            .SensibleHeatW = source.sensible_heat,
            .LatentHeatW = source.latent_heat,
            .EfficiencyPercent = 100 * source.efficiency,
            .CondensateLitersPerHour = source.water_produced,
            .SupplyOutletTemperatureC = source.Supply_outlet_temp,
            .SupplyOutletRelativeHumidityPercent = 100 * source.Supply_outlet_rh,
            .ExhaustOutletTemperatureC = source.Exhaust_outlet_temp,
            .ExhaustOutletRelativeHumidityPercent = 100 * source.Exhaust_outlet_rh
        }
    End Function

    Private Shared Function BuildInterpolationAirflows(maximumAirflow As Double) As Double()
        Dim stepSize As Double
        If Math.Floor(maximumAirflow) >= 5000 Then
            stepSize = 50
        ElseIf Math.Floor(maximumAirflow) >= 2000 Then
            stepSize = 25
        ElseIf Math.Floor(maximumAirflow) >= 1000 Then
            stepSize = 15
        ElseIf Math.Floor(maximumAirflow) >= 500 Then
            stepSize = 10
        Else
            stepSize = 1
        End If

        Dim values(CInt(Math.Floor(maximumAirflow / stepSize)) + 1) As Double
        For index As Integer = 0 To values.Length - 1
            values(index) = stepSize * index
        Next
        Return values
    End Function

    Private Shared Sub BuildWorkingArea(request As CLPerformanceCurveRequest,
        airflows As Double(),
        pressures As Double(),
        powers As Double(),
        efficiencies As Double(),
        outputAirflows As List(Of Double),
        outputPressures As List(Of Double))

        For index As Integer = 1 To airflows.Length - 1
            Dim include As Boolean
            If request.ShowSfpArea Then
                include = ((2 * powers(index)) * 3.6R) / airflows(index) <= request.SfpLimit
            ElseIf request.ShowPassiveHouseArea Then
                include = (powers(index) * 2) / airflows(index) <= request.PassiveHouseLimit
            ElseIf request.ShowErpArea Then
                Dim erp As FanERP2018 = ERP2018_calculation(
                    airflows(index) / 3600.0R,
                    efficiencies(Math.Min(index, efficiencies.Length - 1)),
                    pressures(index),
                    powers(index),
                    request.Model)
                include = erp.Delta > 0
            End If

            If include Then
                outputAirflows.Add(airflows(index))
                outputPressures.Add(pressures(index))
            End If
        Next
    End Sub

    Private Shared Sub ConvertAirflowsToIp(values As Double())
        For index As Integer = 0 To values.Length - 1
            values(index) /= 3.6R
        Next
    End Sub

    Private Shared Sub ApplyQuadraticPressureDrop(xValues As Double(),
        yValues As Double(),
        pressureDrop As Double,
        referenceAirflow As Double)

        If xValues Is Nothing OrElse yValues Is Nothing OrElse referenceAirflow <= 0 OrElse pressureDrop <= 0 Then Return
        For index As Integer = 0 To Math.Min(xValues.Length, yValues.Length) - 1
            yValues(index) -= pressureDrop * (xValues(index) / referenceAirflow) ^ 2
        Next
    End Sub

    Private Shared Sub TrimPressureCurveAtZero(ByRef xValues As Double(),
        ByRef yValues As Double(),
        ByRef zValues As Double())

        Dim zeroIndex As Integer = -1
        For index As Integer = 1 To yValues.Length - 1
            If yValues(index) <= 0 Then
                zeroIndex = index
                Exit For
            End If
        Next
        If zeroIndex < 0 Then Return

        Dim previousIndex As Integer = zeroIndex - 1
        Dim zeroAirflow As Double = xValues(zeroIndex)
        Dim zeroPower As Double = zValues(zeroIndex)
        If yValues(previousIndex) > 0 AndAlso yValues(zeroIndex) < 0 Then
            Dim ratio As Double = yValues(previousIndex) /
                (yValues(previousIndex) - yValues(zeroIndex))
            zeroAirflow = xValues(previousIndex) + ratio * (xValues(zeroIndex) - xValues(previousIndex))
            zeroPower = zValues(previousIndex) + ratio * (zValues(zeroIndex) - zValues(previousIndex))
        End If

        ReDim Preserve xValues(zeroIndex)
        ReDim Preserve yValues(zeroIndex)
        ReDim Preserve zValues(zeroIndex)
        xValues(zeroIndex) = zeroAirflow
        yValues(zeroIndex) = 0
        zValues(zeroIndex) = zeroPower
    End Sub

    Private Shared Function GetUpperIndex(values As Double(), value As Double) As Integer
        If values Is Nothing OrElse values.Length = 0 Then Return 0
        For index As Integer = 0 To values.Length - 1
            If values(index) >= value Then Return index
        Next
        Return values.Length - 1
    End Function

    Private Shared Function InterpolateCurveValue(xValues As Double(),
        yValues As Double(),
        xValue As Double,
        upperIndex As Integer) As Double

        If upperIndex <= 0 Then Return yValues(0)
        If upperIndex >= xValues.Length Then Return yValues(yValues.Length - 1)

        Dim x0 As Double = xValues(upperIndex - 1)
        Dim x1 As Double = xValues(upperIndex)
        Dim y0 As Double = yValues(upperIndex - 1)
        Dim y1 As Double = yValues(upperIndex)
        If x1 = x0 Then Return y1
        Return y0 + (xValue - x0) * (y1 - y0) / (x1 - x0)
    End Function

End Class
