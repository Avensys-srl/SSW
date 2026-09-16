Imports Climalombarda.DataCentral.LTModel
Imports Climalombarda.DataCentral
Imports System.Linq

Public NotInheritable Class CLNextUiSoundInput
    Public Property IncludeInReport As Boolean
    Public Property Directivity As Integer = 2
    Public Property Distance1Meters As Double = 1
    Public Property Distance2Meters As Double = 3
    Public Property IncludeIso16032 As Boolean
End Class

Public NotInheritable Class CLNextUiSoundRow
    Public Property Type As String
    Public Property Caption As String
    Public Property Bands As Double()
    Public Property LwA As Double
    Public Property Lp1 As Double?
    Public Property Lp2 As Double?
End Class

Public NotInheritable Class CLNextUiSoundResult
    Public Property Directivity As Integer
    Public Property Distance1Meters As Double
    Public Property Distance2Meters As Double
    Public Property Iso16032Available As Boolean
    Public Property Rows As New List(Of CLNextUiSoundRow)()
End Class

Public NotInheritable Class CLNextUiPreselectionFilters
    Public Property RotaryOnlyEnabled As Boolean
    Public Property MaximumSfpEnabled As Boolean
    Public Property MaximumSfp As Double = 2
    Public Property SupplyNoiseEnabled As Boolean
    Public Property SupplyNoiseMetric As String = "LWA"
    Public Property MaximumSupplyNoiseDbA As Double = 50
    Public Property SupplyNoiseDirectivity As Integer = 2
    Public Property SupplyNoiseDistanceMeters As Double = 1
    Public Property BreakoutNoiseEnabled As Boolean
    Public Property BreakoutNoiseMetric As String = "LWA"
    Public Property MaximumBreakoutNoiseDbA As Double = 50
    Public Property BreakoutNoiseDirectivity As Integer = 2
    Public Property BreakoutNoiseDistanceMeters As Double = 1
End Class

Public NotInheritable Class CLNextUiCo2Input
    Public Property IncludeInReport As Boolean
    Public Property RoomHeightMeters As Double = 3
    Public Property RoomLengthMeters As Double = 8
    Public Property RoomWidthMeters As Double = 7
    Public Property ActivityMet As Double = 1.2
    Public Property PeopleDuringBreak As Double
    Public Property PeopleDuringPresence As Double = 20
    Public Property BreakMinutes As Double = 15
    Public Property PresenceMinutes As Double = 45
    Public Property CalculationMethod As String = "MaximumCO2"
    Public Property StandardPreset As String = "None"
    Public Property OutdoorCo2Ppm As Double = 380
    Public Property MaximumCo2Ppm As Double = 1000
    Public Property FixedAirflowLitersPerSecond As Double
    Public Property AirflowPerPersonLitersPerSecond As Double = 10
    Public Property AirflowPerAreaLitersPerSecondM2 As Double = 0.35
End Class

Public NotInheritable Class CLNextUiCo2Point
    Public Property Hours As Double
    Public Property Ppm As Double
End Class

Public NotInheritable Class CLNextUiCo2Result
    Public Property RoomVolumeLiters As Double
    Public Property RoomAreaSquareMeters As Double
    Public Property Co2ProductionPerPersonLitersPerHour As Double
    Public Property RequiredAirflowLitersPerSecond As Double
    Public Property RequiredAirflowM3h As Double
    Public Property MaximumCo2Ppm As Double
    Public Property Points As New List(Of CLNextUiCo2Point)()
End Class

Public NotInheritable Class CLNextUiIndoorQualityService

    Private Shared ReadOnly BreakoutInsulation As Double() =
        {20, 23, 26, 29, 32, 37, 43, 45}

    Private Sub New()
    End Sub

    Public Shared Function CalculateSound(
        model As CLDCHeatRecoveryModel,
        regulationPercent As Double,
        airflowM3h As Double,
        pressurePa As Double,
        input As CLNextUiSoundInput) As CLNextUiSoundResult

        If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
        If input Is Nothing Then input = New CLNextUiSoundInput()
        Dim directivity = If(input.Directivity = 4 OrElse input.Directivity = 8,
            input.Directivity, 2)
        Dim distance1 = Math.Max(0.1, input.Distance1Meters)
        Dim distance2 = Math.Max(0.1, input.Distance2Meters)
        Dim iso16032Available = SupportsIso16032(model)
        Dim result As New CLNextUiSoundResult With {
            .Directivity = directivity,
            .Distance1Meters = distance1,
            .Distance2Meters = distance2,
            .Iso16032Available = iso16032Available
        }
        Dim swapped = IsSeriesSevenSwapped(model)

        If model.HasSoundData_Inlet Then
            AddSoundRow(result, model, "Fresh", L("Sound_Fresh", "Fresh"),
                Scale(CloneValues(If(swapped, model.SoundData_OutletItems,
                    model.SoundData_InletItems)), 1.01),
                regulationPercent, directivity, distance1, distance2, False)
        End If
        If model.HasSoundData_Outlet Then
            AddSoundRow(result, model, "Supply", L("Sound_Supply", "Supply"),
                CloneValues(If(swapped, model.SoundData_InletItems,
                    model.SoundData_OutletItems)),
                regulationPercent, directivity, distance1, distance2, False)
            AddSoundRow(result, model, "Exhaust", L("Sound_Exhaust", "Exhaust"),
                Scale(CloneValues(model.SoundData_OutletItems), 1.01),
                regulationPercent, directivity, distance1, distance2, False)
        End If
        If model.HasSoundData_Inlet Then
            AddSoundRow(result, model, "Return", L("Sound_Return", "Return"),
                Scale(CloneValues(model.SoundData_InletItems), 0.99),
                regulationPercent, directivity, distance1, distance2, False)
        End If
        If model.HasSoundData_Outlet Then
            AddSoundRow(result, model, "Breakout", L("Sound_Breakout", "Breakout"),
                BreakoutValues(model, swapped),
                regulationPercent, directivity, distance1, distance2, True)
        End If

        If iso16032Available AndAlso input.IncludeIso16032 AndAlso
            model.HasSoundData_Inlet AndAlso
            result.Rows.Count > 1 Then
            Dim isoValues = result.Rows(1).Bands.Select(Function(value) value - 7).ToArray()
            result.Rows.Add(New CLNextUiSoundRow With {
                .Type = "Frisse",
                .Caption = "Lp@EN-ISO16032",
                .Bands = isoValues,
                .LwA = CLDataCentralCommon.CalculateTotalSound(isoValues)
            })
        End If
        Return result
    End Function

    Public Shared Function SupportsIso16032(model As CLDCHeatRecoveryModel) As Boolean
        If model Is Nothing Then Return False

        Dim identifiers As New List(Of String) From {
            model.Code,
            model.ToString()
        }
        Try
            identifiers.Add(CLEnvironment.Current.GetCustomerHeatRecoveryModelName(model))
        Catch
        End Try

        Return identifiers.
            Where(Function(value) Not String.IsNullOrWhiteSpace(value)).
            Any(Function(value)
                    Return value.IndexOf("HCI", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                        value.IndexOf("FS", StringComparison.OrdinalIgnoreCase) >= 0
                End Function)
    End Function

    Public Shared Function CalculateSoundPressure(
        soundPowerDbA As Double,
        directivity As Integer,
        distanceMeters As Double) As Double

        Dim normalizedDirectivity = If(directivity = 4 OrElse directivity = 8,
            directivity, 2)
        Dim normalizedDistance = Math.Max(0.1, distanceMeters)
        Return Math.Round(soundPowerDbA - 11 +
            10 * Math.Log10(normalizedDirectivity) -
            20 * Math.Log10(normalizedDistance), 1)
    End Function

    Public Shared Function CalculateCo2(input As CLNextUiCo2Input) As CLNextUiCo2Result
        If input Is Nothing Then input = New CLNextUiCo2Input()
        Dim volume = Math.Max(1,
            input.RoomWidthMeters * input.RoomLengthMeters *
            input.RoomHeightMeters * 1000)
        Dim area = Math.Max(0, input.RoomWidthMeters * input.RoomLengthMeters)
        Dim people = Math.Max(input.PeopleDuringBreak, input.PeopleDuringPresence)
        Dim co2PerPerson =
            0.00276 * 1.645 * input.ActivityMet / (0.23 * 0.83 + 0.77) * 3600
        Dim airflow = input.FixedAirflowLitersPerSecond
        Dim maximum = input.MaximumCo2Ppm
        Select Case If(input.CalculationMethod, String.Empty).Trim().ToLowerInvariant()
            Case "fixedairflow"
                airflow = Math.Max(0.001, airflow)
                maximum = 1000000 * people * co2PerPerson /
                    (3600 * airflow) + input.OutdoorCo2Ppm
            Case "personandarea"
                airflow =
                    input.AirflowPerAreaLitersPerSecondM2 * area +
                    input.AirflowPerPersonLitersPerSecond * people
                airflow = Math.Max(0.001, airflow)
                maximum = 1000000 * people * co2PerPerson /
                    (3600 * airflow) + input.OutdoorCo2Ppm
            Case Else
                Dim delta = Math.Max(1, maximum - input.OutdoorCo2Ppm)
                airflow = 1000000 * (people * co2PerPerson / 3600) / delta
        End Select

        Dim result As New CLNextUiCo2Result With {
            .RoomVolumeLiters = volume,
            .RoomAreaSquareMeters = area,
            .Co2ProductionPerPersonLitersPerHour = co2PerPerson,
            .RequiredAirflowLitersPerSecond = airflow,
            .RequiredAirflowM3h = airflow * 3.6,
            .MaximumCo2Ppm = maximum
        }
        FillCo2Points(result.Points, input, volume, maximum, airflow)
        Return result
    End Function

    Private Shared Sub FillCo2Points(points As List(Of CLNextUiCo2Point),
        input As CLNextUiCo2Input, volume As Double, maximum As Double,
        airflow As Double)

        Dim maximumPeople = Math.Max(
            input.PeopleDuringBreak, input.PeopleDuringPresence)
        Dim ppmPerPerson = If(maximumPeople > 0,
            (maximum - input.OutdoorCo2Ppm) / maximumPeople, 0)
        Dim current = input.OutdoorCo2Ppm
        Dim presenceCycle = Math.Max(1, input.PresenceMinutes)
        Dim breakCycle = Math.Max(0, input.BreakMinutes)
        Dim cycle = Math.Max(1, presenceCycle + breakCycle)
        points.Add(New CLNextUiCo2Point With {.Hours = 0, .Ppm = current})
        For minuteIndex As Integer = 1 To 300
            Dim cycleMinute = (minuteIndex - 1) Mod cycle
            Dim occupants = If(cycleMinute < presenceCycle,
                input.PeopleDuringPresence, input.PeopleDuringBreak)
            Dim intervalMaximum =
                input.OutdoorCo2Ppm + occupants * ppmPerPerson
            current = (intervalMaximum - current) *
                (1 - Math.Exp(-Math.Max(0, airflow) / volume * 60)) + current
            points.Add(New CLNextUiCo2Point With {
                .Hours = minuteIndex / 60.0,
                .Ppm = current
            })
        Next
    End Sub

    Private Shared Sub AddSoundRow(result As CLNextUiSoundResult,
        model As CLDCHeatRecoveryModel, type As String, caption As String,
        rawValues As Double(), regulationPercent As Double, directivity As Integer,
        distance1 As Double, distance2 As Double, forceAbsolute As Boolean)

        If rawValues Is Nothing OrElse rawValues.Length < 8 Then Return
        Dim corrected = rawValues.Take(8).
            Select(Function(value)
                Dim adjusted = Math.Round(
                    value + 55 * Math.Log10(Math.Max(1, regulationPercent) / 100),
                    1)
                Return If(forceAbsolute, Math.Abs(adjusted), adjusted)
            End Function).ToArray()
        Dim lwa = CLDataCentralCommon.CalculateTotalSound(corrected)
        result.Rows.Add(New CLNextUiSoundRow With {
            .Type = type,
            .Caption = caption,
            .Bands = corrected,
            .LwA = lwa,
            .Lp1 = CalculateSoundPressure(lwa, directivity, distance1),
            .Lp2 = CalculateSoundPressure(lwa, directivity, distance2)
        })
    End Sub

    Private Shared Function BreakoutValues(model As CLDCHeatRecoveryModel,
        swapped As Boolean) As Double()

        If swapped Then Return CloneValues(model.SoundData_InletItems)
        If model.Code.IndexOf("QUARK 025", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return {54.7, 50.7, 35.8, 36.8, 34.8, 30.8, 27.9, 19.9}
        End If
        If model.Code.IndexOf("QUARK 035", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Scale({54.7, 50.7, 35.8, 36.8, 34.8, 30.8, 27.9, 19.9}, 1.145)
        End If

        Dim factor = BreakoutGeometryFactor(model)
        Dim values = CloneValues(model.SoundData_OutletItems)
        For index = 0 To Math.Min(7, values.Length - 1)
            values(index) = values(index) - BreakoutInsulation(index) + factor
        Next
        Return values
    End Function

    Private Shared Function BreakoutGeometryFactor(
        model As CLDCHeatRecoveryModel) As Double
        Dim factor As Double
        Dim dimensions = CLEnvironment.Current.FindModelDimensionsByModel(model)
        If dimensions IsNot Nothing AndAlso dimensions.Length > 0 Then
            For Each dimension In dimensions
                Dim section = dimension.DimensionA * dimension.DimensionC
                If section <= 0 Then Continue For
                Dim wallAndBase =
                    (dimension.DimensionA + dimension.DimensionB) *
                    dimension.DimensionC * 2 +
                    dimension.DimensionB * dimension.DimensionA
                factor = Math.Max(factor, 10 * Math.Log10(wallAndBase / section))
            Next
        Else
            factor = 10 * Math.Log10(1250000.0 / 250000.0)
        End If
        Return factor
    End Function

    Private Shared Function IsSeriesSevenSwapped(
        model As CLDCHeatRecoveryModel) As Boolean
        Return model.CLSerie IsNot Nothing AndAlso model.CLSerie.Code = "7" AndAlso
            model.CLEnumItem_AeraulicConnection IsNot Nothing AndAlso
            (model.CLEnumItem_AeraulicConnection.TextCode = "HCI" OrElse
             model.CLEnumItem_AeraulicConnection.TextCode = "FS")
    End Function

    Private Shared Function CloneValues(values As Double()) As Double()
        Return If(values, New Double() {}).ToArray()
    End Function

    Private Shared Function Scale(values As Double(), factor As Double) As Double()
        For index = 0 To values.Length - 1
            values(index) *= factor
        Next
        Return values
    End Function

    Private Shared Function L(key As String, fallback As String) As String
        Try
            Dim value = CLEnvironment.Current.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> key Then Return value
        Catch
        End Try
        Return fallback
    End Function
End Class
