Imports Climalombarda.DataCentral.LTModel
Imports System.Globalization
Imports System.Data.SqlServerCe

Public Enum CLCoilPerformanceMode
    CWD
    HWD
    HCD
End Enum

Public Enum CLCoilPerformanceSource
    Standard
    External
End Enum

Public Enum CLCoilPerformanceEditMode
    Standard
    StandardCustomized
    External
End Enum

Public Class CLCoilDefinition
    Public Property Id As Integer
    Public Property Name As String
    Public Property Mode As CLCoilPerformanceMode
    Public Property Source As CLCoilPerformanceSource
    Public Property Length As Integer
    Public Property Height As Integer
    Public Property NumberOfRows As Integer
    Public Property NumberOfCircuits As Integer
    Public Property FinSpacing As CLCOFinSpacing
    Public Property FinSpacingValue As Double = 2.5
    Public Property HeaderType As CLCOHeaderType

    Public Overrides Function ToString() As String
        Return Name
    End Function

    Public Function Clone() As CLCoilDefinition
        Return DirectCast(MemberwiseClone(), CLCoilDefinition)
    End Function
End Class

Public Class CLCoilCalculationInput
    Public Property Coil As CLCoilDefinition
    Public Property CalculationMode As CLCoilPerformanceMode = CLCoilPerformanceMode.HCD
    Public Property AirFlow As Double
    Public Property AirInletTemperature As Double
    Public Property AirInletRH As Double
    Public Property FluidType As CLCOFluidType = CLCOFluidType.Water
    Public Property FluidTypeTec As Double = 10.0
    Public Property CoolingFluidInletTemperature As Double = 7.0
    Public Property CoolingFluidOutletTemperature As Double = 12.0
    Public Property HeatingFluidInletTemperature As Double = 80.0
    Public Property HeatingFluidOutletTemperature As Double = 70.0
End Class

Public Class CLCoilCalculationResult
    Public Property Mode As CLCoilPerformanceMode
    Public Property OutletTemperature As Double
    Public Property OutletRH As Double
    Public Property AirPressureDrop As Double
    Public Property WaterPressureDrop As Double
    Public Property CondensedWater As Double
    Public Property HeatTransferred As Double
    Public Property SensibleHeat As Double
    Public Property FaceVelocity As Double
    Public Property Auxiliary As CLCoilAuxiliaryState
    Public Property ErrorMessage As String

    Public ReadOnly Property IsOk As Boolean
        Get
            Return String.IsNullOrEmpty(ErrorMessage) AndAlso Auxiliary = CLCoilAuxiliaryState.Ok
        End Get
    End Property
End Class

Public Enum CLCoilAuxiliaryState
    Ok = 0
    [Error] = 1
    PressureDropTooHigh = 2
    Ice = 3
End Enum

Public Class CLCoilPerformanceCalculator

    Public Shared Function GetAvailableCoils(dcHeatRecoveryModel As CLDCHeatRecoveryModel) As List(Of CLCoilDefinition)
        Return GetAssociatedCoils(dcHeatRecoveryModel)
    End Function

    Public Shared Function HasAssociatedCoils(dcHeatRecoveryModel As CLDCHeatRecoveryModel) As Boolean
        Return GetAssociatedCoils(dcHeatRecoveryModel).Count > 0
    End Function

    Private Shared Function GetAssociatedCoils(dcHeatRecoveryModel As CLDCHeatRecoveryModel) As List(Of CLCoilDefinition)
        Dim coils As New List(Of CLCoilDefinition)

        If dcHeatRecoveryModel Is Nothing OrElse CLEnvironment.Current Is Nothing OrElse String.IsNullOrEmpty(CLEnvironment.Current.DCLiteDatabasePath) Then
            Return coils
        End If

        Try
            Using connection As New SqlCeConnection(String.Format("Data Source=""{0}""; Password=""{1}""", CLEnvironment.Current.DCLiteDatabasePath, "@D3C1L4T2%"))
                connection.Open()

                If Not TableExists(connection, "CLCoils") OrElse Not TableExists(connection, "CLHeatRecoveryModelCoils") Then
                    Return coils
                End If

                Using command As SqlCeCommand = connection.CreateCommand()
                    command.CommandText =
                        "SELECT c.Id, c.Name, r.CoilMode, c.Length, c.Height, c.NumberOfRows, " &
                        "c.NumberOfCircuits, c.FinSpacing_mm " &
                        "FROM CLHeatRecoveryModelCoils r " &
                        "INNER JOIN CLCoils c ON c.Id = r.IdCoil " &
                        "WHERE r.IdHeatRecoveryModel = @IdHeatRecoveryModel " &
                        "AND r.Active = 1 AND c.Active = 1 " &
                        "ORDER BY r.IsDefault DESC, r.SortOrder, c.Name"
                    command.Parameters.Add(New SqlCeParameter("@IdHeatRecoveryModel", dcHeatRecoveryModel.Id))

                    Using reader As SqlCeDataReader = command.ExecuteReader()
                        Dim addedCoilIds As New HashSet(Of Integer)

                        While reader.Read()
                            Dim coilId As Integer = Convert.ToInt32(reader("Id"), CultureInfo.InvariantCulture)
                            If addedCoilIds.Contains(coilId) Then
                                Continue While
                            End If

                            Dim mode As CLCoilPerformanceMode
                            If Not [Enum].TryParse(Convert.ToString(reader("CoilMode")), True, mode) Then
                                mode = CLCoilPerformanceMode.HCD
                            End If

                            Dim finSpacingValue As Double = Convert.ToDouble(reader("FinSpacing_mm"), CultureInfo.InvariantCulture)
                            coils.Add(New CLCoilDefinition With {
                                .Id = coilId,
                                .Name = Convert.ToString(reader("Name")),
                                .Mode = mode,
                                .Source = CLCoilPerformanceSource.Standard,
                                .Length = Convert.ToInt32(reader("Length"), CultureInfo.InvariantCulture),
                                .Height = Convert.ToInt32(reader("Height"), CultureInfo.InvariantCulture),
                                .NumberOfRows = Convert.ToInt32(reader("NumberOfRows"), CultureInfo.InvariantCulture),
                                .NumberOfCircuits = Convert.ToInt32(reader("NumberOfCircuits"), CultureInfo.InvariantCulture),
                                .FinSpacing = DoubleToFinSpacing(finSpacingValue),
                                .FinSpacingValue = finSpacingValue,
                                .HeaderType = CLCOHeaderType._3_4
                            })

                            addedCoilIds.Add(coilId)
                        End While
                    End Using
                End Using
            End Using
        Catch
            Return New List(Of CLCoilDefinition)
        End Try

        Return coils
    End Function

    Private Shared Function TableExists(connection As SqlCeConnection, tableName As String) As Boolean
        Using command As SqlCeCommand = connection.CreateCommand()
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName"
            command.Parameters.Add(New SqlCeParameter("@TableName", tableName))
            Return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Shared Sub AddModelCoil(coils As List(Of CLCoilDefinition), dcHeatRecoveryModel As CLDCHeatRecoveryModel, mode As CLCoilPerformanceMode)
        Dim length As Integer
        Dim height As Integer
        Dim rows As Integer
        Dim circuits As Integer
        Dim finSpacing As CLCOFinSpacing
        Dim headerType As CLCOHeaderType

        Select Case mode
            Case CLCoilPerformanceMode.CWD
                length = dcHeatRecoveryModel.CWD_Length
                height = dcHeatRecoveryModel.CWD_Height
                rows = dcHeatRecoveryModel.CWD_NumerOfRows
                circuits = dcHeatRecoveryModel.CWD_NumerOfCircuits
                finSpacing = TranslateFinsStep(dcHeatRecoveryModel.CLEnumItem_CWDFinsStep)
                headerType = TranslateHeaderType(dcHeatRecoveryModel.CLEnumItem_CWDHeaderType)

            Case CLCoilPerformanceMode.HWD
                length = dcHeatRecoveryModel.HWD_Length
                height = dcHeatRecoveryModel.HWD_Height
                rows = dcHeatRecoveryModel.HWD_NumerOfRows
                circuits = dcHeatRecoveryModel.HWD_NumerOfCircuits
                finSpacing = TranslateFinsStep(dcHeatRecoveryModel.CLEnumItem_HWDFinsStep)
                headerType = TranslateHeaderType(dcHeatRecoveryModel.CLEnumItem_HWDHeaderType)

            Case CLCoilPerformanceMode.HCD
                If dcHeatRecoveryModel.CWD_Length > 0 AndAlso dcHeatRecoveryModel.CWD_Height > 0 Then
                    length = dcHeatRecoveryModel.CWD_Length
                    height = dcHeatRecoveryModel.CWD_Height
                    rows = dcHeatRecoveryModel.CWD_NumerOfRows
                    circuits = dcHeatRecoveryModel.CWD_NumerOfCircuits
                    finSpacing = TranslateFinsStep(dcHeatRecoveryModel.CLEnumItem_CWDFinsStep)
                    headerType = TranslateHeaderType(dcHeatRecoveryModel.CLEnumItem_CWDHeaderType)
                Else
                    length = dcHeatRecoveryModel.HWD_Length
                    height = dcHeatRecoveryModel.HWD_Height
                    rows = dcHeatRecoveryModel.HWD_NumerOfRows
                    circuits = dcHeatRecoveryModel.HWD_NumerOfCircuits
                    finSpacing = TranslateFinsStep(dcHeatRecoveryModel.CLEnumItem_HWDFinsStep)
                    headerType = TranslateHeaderType(dcHeatRecoveryModel.CLEnumItem_HWDHeaderType)
                End If
        End Select

        If length <= 0 OrElse height <= 0 OrElse rows <= 0 OrElse circuits <= 0 Then
            Return
        End If

        coils.Add(New CLCoilDefinition With {
            .Name = String.Format("Standard {0} {1}x{2}", mode, length, height),
            .Mode = mode,
            .Source = CLCoilPerformanceSource.Standard,
            .Length = length,
            .Height = height,
            .NumberOfRows = rows,
            .NumberOfCircuits = circuits,
            .FinSpacing = finSpacing,
            .FinSpacingValue = FinSpacingToDouble(finSpacing),
            .HeaderType = headerType
        })
    End Sub

    Private Shared Function CreateFallbackCoil(name As String,
        mode As CLCoilPerformanceMode,
        length As Integer,
        height As Integer,
        rows As Integer,
        circuits As Integer,
        finSpacing As Double,
        headerType As CLCOHeaderType,
        Optional source As CLCoilPerformanceSource = CLCoilPerformanceSource.Standard) As CLCoilDefinition

        Return New CLCoilDefinition With {
            .Name = name,
            .Mode = mode,
            .Source = source,
            .Length = length,
            .Height = height,
            .NumberOfRows = rows,
            .NumberOfCircuits = circuits,
            .FinSpacing = DoubleToFinSpacing(finSpacing),
            .FinSpacingValue = finSpacing,
            .HeaderType = headerType
        }
    End Function

    Public Shared Function Calculate(input As CLCoilCalculationInput) As List(Of CLCoilCalculationResult)
        Dim results As New List(Of CLCoilCalculationResult)

        If input Is Nothing OrElse input.Coil Is Nothing Then
            results.Add(New CLCoilCalculationResult With {.ErrorMessage = "Coil not selected."})
            Return results
        End If

        Select Case input.CalculationMode
            Case CLCoilPerformanceMode.CWD
                results.Add(CalculateSingle(input, CLCOCoilType.Cooling, CLCoilPerformanceMode.CWD))

            Case CLCoilPerformanceMode.HWD
                results.Add(CalculateSingle(input, CLCOCoilType.Heating, CLCoilPerformanceMode.HWD))

            Case CLCoilPerformanceMode.HCD
                results.Add(CalculateSingle(input, CLCOCoilType.Cooling, CLCoilPerformanceMode.CWD))
                results.Add(CalculateSingle(input, CLCOCoilType.Heating, CLCoilPerformanceMode.HWD))
        End Select

        Return results
    End Function

    Private Shared Function CalculateSingle(input As CLCoilCalculationInput,
        coilType As CLCOCoilType,
        resultMode As CLCoilPerformanceMode) As CLCoilCalculationResult

        Dim result As New CLCoilCalculationResult With {.Mode = resultMode}

        Try
            Dim response As String = CLCOIL.Calculate(BuildRequest(input, coilType))
            Dim values As Dictionary(Of String, String) = ParseResponse(response)
            Dim res As Double = GetValue(values, "Res", 0)
            Dim powerW As Double = GetValue(values, "Power", 0) * 1000
            Dim shr As Double = GetValue(values, "SHR", 1)

            result.OutletTemperature = Math.Round(GetValue(values, "AirTOff", 0), 2)
            result.OutletRH = Math.Round(GetValue(values, "AirFOff", 0), 2)
            result.AirPressureDrop = Math.Round(GetValue(values, "PDropWet", GetValue(values, "PDropDry", 0)), 2)
            result.WaterPressureDrop = Math.Round(GetFirstValue(values, 0, "kPaMed", "PDropMed", "MedPDrop", "MedDP", "FluidPDrop", "FluidPressureDrop", "PDropFluid"), 2)
            result.CondensedWater = Math.Round(GetValue(values, "QCondens", 0), 2)
            result.HeatTransferred = Math.Round(powerW, 2)
            result.SensibleHeat = Math.Round(If(coilType = CLCOCoilType.Heating, powerW, powerW * shr), 2)
            result.FaceVelocity = Math.Round(CalculateFaceVelocity(input), 2)
            result.Auxiliary = If(res = 0, CLCoilAuxiliaryState.Ok, CLCoilAuxiliaryState.Error)

            If response = "" Then
                result.ErrorMessage = "Empty CoilCalc response."
            End If
        Catch ex As Exception
            result.ErrorMessage = ex.Message
        End Try

        Return result
    End Function

    Private Shared Function BuildRequest(input As CLCoilCalculationInput, coilType As CLCOCoilType) As String
        Dim medOn As Double
        Dim medOff As Double

        If coilType = CLCOCoilType.Cooling Then
            medOn = input.CoolingFluidInletTemperature
            medOff = input.CoolingFluidOutletTemperature
        Else
            medOn = input.HeatingFluidInletTemperature
            medOff = input.HeatingFluidOutletTemperature
        End If

        Return String.Format(CultureInfo.InvariantCulture,
            "CalcMode=STD;Geo=P2510;TubeMat=CU;FinMat=AL;FrameMat=VZ;ConnMat=CU;MediType={0};GConc={1};FHeight={2};FLength={3};NumRows={4};NumCirc={5};FinSpace={6};AirVolume={7};AirTOn={8};AirFOn={9};Power=0;MedOn={10};MedOff={11};DPMaxMed=40;ConIn={12};",
            GetMediaType(input.FluidType),
            If(input.FluidType = CLCOFluidType.Water, 0, input.FluidTypeTec),
            input.Coil.Height,
            input.Coil.Length,
            input.Coil.NumberOfRows,
            input.Coil.NumberOfCircuits,
            FormatInputNumber(If(input.Coil.FinSpacingValue > 0, input.Coil.FinSpacingValue, FinSpacingToDouble(input.Coil.FinSpacing))),
            FormatInputNumber(input.AirFlow),
            FormatInputNumber(input.AirInletTemperature),
            FormatInputNumber(input.AirInletRH),
            FormatInputNumber(medOn),
            FormatInputNumber(medOff),
            CInt(input.Coil.HeaderType))
    End Function

    Private Shared Function FormatInputNumber(value As Double) As String
        Return value.ToString("0.###", CultureInfo.InvariantCulture).Replace("."c, ","c)
    End Function

    Private Shared Function GetMediaType(fluidType As CLCOFluidType) As String
        Select Case fluidType
            Case CLCOFluidType.Glic_Etil
                Return "SE"
            Case CLCOFluidType.Glic_Prop
                Return "SP"
            Case CLCOFluidType.Pekasol_50
                Return "Pekasol_50"
            Case CLCOFluidType.Pekasol_L
                Return "Pekasol_L"
            Case CLCOFluidType.Glykosol
                Return "Glykosol"
            Case CLCOFluidType.Tyfoxit
                Return "Tyfoxit"
            Case CLCOFluidType.Tyfoxit_F
                Return "Tyfoxit_F"
            Case CLCOFluidType.Hycool
                Return "Hycool"
            Case Else
                Return "W"
        End Select
    End Function

    Public Shared Function FinSpacingToDouble(finSpacing As CLCOFinSpacing) As Double
        Dim rawName As String = finSpacing.ToString().TrimStart("_"c).Replace("_"c, "."c)
        Dim value As Double

        If Double.TryParse(rawName, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
            Return value
        End If

        Return 2.5
    End Function

    Public Shared Function DoubleToFinSpacing(value As Double) As CLCOFinSpacing
        Dim bestValue As CLCOFinSpacing = CLCOFinSpacing._2_5
        Dim bestDistance As Double = Double.MaxValue

        For Each finSpacing As CLCOFinSpacing In [Enum].GetValues(GetType(CLCOFinSpacing))
            Dim distance As Double = Math.Abs(FinSpacingToDouble(finSpacing) - value)
            If distance < bestDistance Then
                bestDistance = distance
                bestValue = finSpacing
            End If
        Next

        Return bestValue
    End Function

    Private Shared Function CalculateFaceVelocity(input As CLCoilCalculationInput) As Double
        Dim faceArea As Double = (input.Coil.Length / 1000) * (input.Coil.Height / 1000)
        If faceArea <= 0 Then
            Return 0
        End If

        Return (input.AirFlow / 3600) / faceArea
    End Function

    Private Shared Function ParseResponse(response As String) As Dictionary(Of String, String)
        Dim values As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        For Each part As String In response.Split(";"c)
            Dim separatorIndex As Integer = part.IndexOf("="c)
            If separatorIndex > 0 Then
                values(part.Substring(0, separatorIndex).Trim()) = part.Substring(separatorIndex + 1).Trim()
            End If
        Next

        Return values
    End Function

    Private Shared Function GetValue(values As Dictionary(Of String, String), key As String, defaultValue As Double) As Double
        Dim rawValue As String = Nothing
        Dim value As Double

        If values.TryGetValue(key, rawValue) AndAlso Double.TryParse(rawValue.Replace(","c, "."c), NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
            Return value
        End If

        Return defaultValue
    End Function

    Private Shared Function GetFirstValue(values As Dictionary(Of String, String), defaultValue As Double, ParamArray keys As String()) As Double
        For Each key As String In keys
            Dim rawValue As String = Nothing
            Dim value As Double

            If values.TryGetValue(key, rawValue) AndAlso Double.TryParse(rawValue.Replace(","c, "."c), NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
                Return value
            End If
        Next

        Return defaultValue
    End Function

    Private Shared Function TranslateFinsStep(dcFinStep As CLDCEnumItem) As CLCOFinSpacing
        If dcFinStep IsNot Nothing Then
            Select Case dcFinStep.TextCode
                Case "2-1"
                    Return CLCOFinSpacing._2_1
                Case "2-5"
                    Return CLCOFinSpacing._2_5
            End Select
        End If

        Return CLCOFinSpacing._2_5
    End Function

    Private Shared Function TranslateHeaderType(dcHeaderType As CLDCEnumItem) As CLCOHeaderType
        If dcHeaderType IsNot Nothing Then
            Select Case dcHeaderType.TextCode
                Case "1"
                    Return CLCOHeaderType._1
                Case "3/4"
                    Return CLCOHeaderType._3_4
            End Select
        End If

        Return CLCOHeaderType._3_4
    End Function
End Class
