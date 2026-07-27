Imports System.Globalization

Public NotInheritable Class CLSelectionContractValidator

    Private Sub New()
    End Sub

    Public Shared Function Validate(input As CLSelectionCalculationInput) As CLValidationResult
        Dim result As New CLValidationResult()
        If input Is Nothing Then
            AddIssue(result, "selection.required", CLValidationSeverity.Error, "selection", "Validation.SelectionRequired")
            Return result
        End If

        ValidateSeason(result, input.Winter, "winter")
        ValidateSeason(result, input.Summer, "summer")
        ValidateWaterCoil(result, input.WaterCoil)
        ValidateElectricHeaterCompatibility(result, input.WaterCoil, input.ElectricHeater)

        Return result
    End Function

    Private Shared Sub ValidateSeason(result As CLValidationResult, season As CLSeasonCalculationInput, path As String)
        If season Is Nothing OrElse Not season.Enabled Then Return

        If season.Airflows Is Nothing Then
            AddIssue(result, "airflow.required", CLValidationSeverity.Error, path & ".airflows", "Validation.AirflowRequired")
        Else
            ValidatePositive(result, season.Airflows.SupplyM3h, path & ".airflows.supply", "airflow.supply.invalid")
            ValidatePositive(result, season.Airflows.ExtractM3h, path & ".airflows.extract", "airflow.extract.invalid")
        End If

        ValidateRange(result, season.OutdoorRelativeHumidityPercent, 0, 100,
            path & ".outdoorRelativeHumidityPercent", "humidity.outdoor.invalid")
        ValidateRange(result, season.ReturnRelativeHumidityPercent, 0, 100,
            path & ".returnRelativeHumidityPercent", "humidity.return.invalid")
        ValidateRange(result, season.RegulationPercent, 0, 100,
            path & ".regulationPercent", "regulation.invalid")
    End Sub

    Private Shared Sub ValidateWaterCoil(result As CLValidationResult, coil As CLWaterCoilCalculationOptions)
        If coil Is Nothing OrElse Not coil.Enabled Then Return

        Dim mode As String = If(coil.CalculationMode, String.Empty).Trim().ToUpperInvariant()
        If mode = "CWD" OrElse mode = "HCD" Then
            ValidateWaterDelta(result, coil.CoolingWaterOutletTemperatureC, coil.CoolingWaterInletTemperatureC,
                "waterCoil.cooling", "cooling")
        End If
        If mode = "HWD" OrElse mode = "HCD" Then
            ValidateWaterDelta(result, coil.HeatingWaterInletTemperatureC, coil.HeatingWaterOutletTemperatureC,
                "waterCoil.heating", "heating")
        End If
    End Sub

    Private Shared Sub ValidateWaterDelta(result As CLValidationResult, higher As Double?, lower As Double?,
        path As String, mode As String)

        If Not higher.HasValue OrElse Not lower.HasValue Then
            AddIssue(result, "water.delta.required", CLValidationSeverity.Error, path, "Validation.WaterDeltaRequired", mode)
            Return
        End If

        Dim deltaT As Double = higher.Value - lower.Value
        If deltaT < CLCoilHydraulicRules.MinimumDeltaT Then
            AddIssue(result, "water.delta.invalid", CLValidationSeverity.Error, path,
                "Validation.WaterDeltaInvalid", mode, FormatNumber(deltaT))
        ElseIf deltaT < CLCoilHydraulicRules.CriticalDeltaT Then
            AddIssue(result, "water.delta.critical", CLValidationSeverity.Warning, path,
                "Validation.WaterDeltaCritical", mode, FormatNumber(deltaT))
        ElseIf deltaT < CLCoilHydraulicRules.WarningDeltaT Then
            AddIssue(result, "water.delta.low", CLValidationSeverity.Warning, path,
                "Validation.WaterDeltaLow", mode, FormatNumber(deltaT))
        End If
    End Sub

    Private Shared Sub ValidateElectricHeaterCompatibility(result As CLValidationResult,
        waterCoil As CLWaterCoilCalculationOptions,
        electricHeater As CLElectricHeaterCalculationOptions)

        If waterCoil Is Nothing OrElse electricHeater Is Nothing OrElse
            Not waterCoil.Enabled OrElse electricHeater.EHD Is Nothing OrElse Not electricHeater.EHD.Enabled Then Return

        Dim mode As String = If(waterCoil.CalculationMode, String.Empty).Trim().ToUpperInvariant()
        If mode = "HWD" OrElse mode = "HCD" Then
            AddIssue(result, "heater.ehd.hot_water_conflict", CLValidationSeverity.Error,
                "electricHeater.ehd", "Validation.EhdHotWaterConflict")
        End If
    End Sub

    Private Shared Sub ValidatePositive(result As CLValidationResult, value As Double?,
        path As String, code As String)

        If Not value.HasValue OrElse value.Value <= 0 Then
            AddIssue(result, code, CLValidationSeverity.Error, path, "Validation.PositiveValueRequired")
        End If
    End Sub

    Private Shared Sub ValidateRange(result As CLValidationResult, value As Double?,
        minimum As Double, maximum As Double, path As String, code As String)

        If value.HasValue AndAlso (value.Value < minimum OrElse value.Value > maximum) Then
            AddIssue(result, code, CLValidationSeverity.Error, path, "Validation.ValueOutOfRange",
                FormatNumber(minimum), FormatNumber(maximum))
        End If
    End Sub

    Private Shared Sub AddIssue(result As CLValidationResult, code As String,
        severity As CLValidationSeverity, path As String, messageKey As String,
        ParamArray arguments As String())

        Dim issue As New CLValidationIssue With {
            .Code = code,
            .Severity = severity,
            .Path = path,
            .MessageKey = messageKey
        }
        If arguments IsNot Nothing Then issue.Arguments.AddRange(arguments)
        result.Issues.Add(issue)
    End Sub

    Private Shared Function FormatNumber(value As Double) As String
        Return value.ToString("0.###", CultureInfo.InvariantCulture)
    End Function

End Class
