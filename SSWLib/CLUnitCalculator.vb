Imports System.ComponentModel
Imports Climalombarda.DataCentral.LTModel

Public Class CLUnitCalculator
    Implements INotifyPropertyChanged

#Region "====[ CTor/Dtor/Dispose ]===="

    Sub New()


        m_AccessoryState = CLAccessoryState.None

        m_CWD_FluidType = CLCOFluidType.Water
        m_CWD_FluidTypeTec = 10.0
        m_CWD_InletTemperature = 7.0
        m_CWD_OutletTemperature = 12.0

        m_HWD_FluidType = CLCOFluidType.Water
        m_HWD_FluidTypeTec = 10.0
        m_HWD_InletTemperature = 80.0
        m_HWD_OutletTemperature = 70.0

        SetCalculateState(CLCalculateState.Ok)

    End Sub

#End Region

#Region "====[ PropertyChanged ]===="

    Public Enum CLPropertyName
        AccessoryState
        CWD_Enabled
        CWD_InletTemperature
        CWD_OutletTemperature
        CWD_FluidType
        CWD_FluidTypeTec
        CWD_HeatTransferred
        CWD_SensibleHeat
        HWD_Enabled
        HWD_InletTemperature
        HWD_OutletTemperature
        HWD_FluidType
        HWD_FluidTypeTec
        HWD_HeatTransferred
        Accessory_Temp
        Accessory_RH
        Accessory_PressureDrop
        Accessory_CondensedWater
        Accessory_ValidData
    End Enum

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub

#End Region

#Region "====[ Accessory ]===="

    Public Enum CLAccessoryState
        None
        CWD
        HWD
        CWD_HWD
        EHD
    End Enum

#Region "====[ Properties ]===="

    Private m_AccessoryState As CLAccessoryState = CLAccessoryState.None
    Public Property AccessoryState As CLAccessoryState
        Get
            Return m_AccessoryState
        End Get
        Set(value As CLAccessoryState)

            m_AccessoryState = value

            OnPropertyChanged(CLPropertyName.AccessoryState.ToString())
            'Calculate()
        End Set
    End Property

    Private m_CWD_InletTemperature As Double
    Public Property CWD_InletTemperature As Double
        Get
            Return m_CWD_InletTemperature
        End Get
        Set(value As Double)
            m_CWD_InletTemperature = value
            OnPropertyChanged(CLPropertyName.CWD_Enabled.ToString())
            'Calculate()
        End Set
    End Property

    Private m_CWD_OutletTemperature As Double
    Public Property CWD_OutletTemperature As Double
        Get
            Return m_CWD_OutletTemperature
        End Get
        Set(value As Double)
            m_CWD_OutletTemperature = value
            OnPropertyChanged(CLPropertyName.CWD_OutletTemperature.ToString())
            'Calculate()
        End Set
    End Property

    Private m_CWD_FluidType As CLCOFluidType
    Public Property CWD_FluidType As CLCOFluidType
        Get
            Return m_CWD_FluidType
        End Get
        Set(value As CLCOFluidType)
            m_CWD_FluidType = value
            OnPropertyChanged(CLPropertyName.CWD_FluidType.ToString())
            'Calculate()
        End Set
    End Property

    Private m_CWD_FluidTypeTec As Double
    Public Property CWD_FluidTypeTec As Double
        Get
            Return m_CWD_FluidTypeTec
        End Get
        Set(value As Double)
            m_CWD_FluidTypeTec = value
            OnPropertyChanged(CLPropertyName.CWD_FluidTypeTec.ToString())
            'Calculate()
        End Set
    End Property

    Private m_CWD_HeatTransferred As Double
    Public Property CWD_HeatTransferred As Double
        Get
            Return m_CWD_HeatTransferred
        End Get
        Private Set(value As Double)
            m_CWD_HeatTransferred = value
            OnPropertyChanged(CLPropertyName.CWD_HeatTransferred.ToString())
            'Calculate()
        End Set
    End Property

    Private m_CWD_SensibleHeat As Double
    Public Property CWD_SensibleHeat As Double
        Get
            Return m_CWD_SensibleHeat
        End Get
        Private Set(value As Double)
            m_CWD_SensibleHeat = value
            OnPropertyChanged(CLPropertyName.CWD_SensibleHeat.ToString())
            'Calculate()
        End Set
    End Property

    Private m_HWD_InletTemperature As Double
    Public Property HWD_InletTemperature As Double
        Get
            Return m_HWD_InletTemperature
        End Get
        Set(value As Double)
            m_HWD_InletTemperature = value
            OnPropertyChanged(CLPropertyName.HWD_InletTemperature.ToString())
            'Calculate()
        End Set
    End Property

    Private m_HWD_OutletTemperature As Double
    Public Property HWD_OutletTemperature As Double
        Get
            Return m_HWD_OutletTemperature
        End Get
        Set(value As Double)
            m_HWD_OutletTemperature = value
            OnPropertyChanged(CLPropertyName.HWD_OutletTemperature.ToString())
            'Calculate()
        End Set
    End Property

    Private m_HWD_FluidType As CLCOFluidType
    Public Property HWD_FluidType As CLCOFluidType
        Get
            Return m_HWD_FluidType
        End Get
        Set(value As CLCOFluidType)
            m_HWD_FluidType = value
            OnPropertyChanged(CLPropertyName.HWD_FluidType.ToString())
            'Calculate()
        End Set
    End Property

    Private m_HWD_FluidTypeTec As Double
    Public Property HWD_FluidTypeTec As Double
        Get
            Return m_HWD_FluidTypeTec
        End Get
        Set(value As Double)
            m_HWD_FluidTypeTec = value
            OnPropertyChanged(CLPropertyName.HWD_FluidTypeTec.ToString())
            'Calculate()
        End Set
    End Property

    Private m_HWD_HeatTransferred As Double
    Public Property HWD_HeatTransferred As Double
        Get
            Return m_HWD_HeatTransferred
        End Get
        Private Set(value As Double)
            m_HWD_HeatTransferred = value
            OnPropertyChanged(CLPropertyName.HWD_HeatTransferred.ToString())
            'Calculate()
        End Set
    End Property

    Private m_Accessory_Temp As Double
    Public Property Accessory_Temp As Double
        Get
            Return m_Accessory_Temp
        End Get
        Private Set(value As Double)
            m_Accessory_Temp = value
            OnPropertyChanged(CLPropertyName.Accessory_Temp.ToString())
            'Calculate()
        End Set
    End Property

    Private m_Accessory_RH As Double
    Public Property Accessory_RH As Double
        Get
            Return m_Accessory_RH
        End Get
        Private Set(value As Double)
            m_Accessory_RH = value
            OnPropertyChanged(CLPropertyName.Accessory_RH.ToString())
            'Calculate()
        End Set
    End Property

    Private m_Accessory_PressureDrop As Double
    Public Property Accessory_PressureDrop As Double
        Get
            Return m_Accessory_PressureDrop
        End Get
        Private Set(value As Double)
            m_Accessory_PressureDrop = value
            OnPropertyChanged(CLPropertyName.Accessory_PressureDrop.ToString())
            'Calculate()
        End Set
    End Property

    Private m_Accessory_CondensedWater As Double
    Public Property Accessory_CondensedWater As Double
        Get
            Return m_Accessory_CondensedWater
        End Get
        Private Set(value As Double)
            m_Accessory_CondensedWater = value
            OnPropertyChanged(CLPropertyName.Accessory_CondensedWater.ToString())
            'Calculate()
        End Set
    End Property

#End Region

    Private Function Accessory_TranslateFinsStep(dcFinStep As CLDCEnumItem) As CLCOFinSpacing

        If Not dcFinStep Is Nothing Then
            Select Case dcFinStep.TextCode
                Case "2-1"
                    Return CLCOFinSpacing._2_1
                Case "2-5"
                    Return CLCOFinSpacing._2_5
            End Select
        End If

        Throw New Exception(String.Format("Invalid FinStep value ['{0}'].", IIf(dcFinStep Is Nothing, "Null", dcFinStep.TextCode)))
    End Function

    Private Function Accessory_TranslateHeaderType(dcHeaderType As CLDCEnumItem) As CLCOHeaderType

        If Not dcHeaderType Is Nothing Then
            Select Case dcHeaderType.TextCode
                Case "1"
                    Return CLCOHeaderType._1
                Case "3/4"
                    Return CLCOHeaderType._3_4
            End Select
        End If

        Throw New Exception(String.Format("Invalid HeaderType value ['{0}'].", IIf(dcHeaderType Is Nothing, "Null", dcHeaderType.TextCode)))
    End Function

    Public Enum CLAccessory_CalculateResult
        Ok
        InError
        PressureDropTooHigh
        Ice
    End Enum

    Private Structure CLAccessoryCalculateOutputData

        Dim OutletTemperature As Double
        Dim OutletRH As Double
        Dim PressureDrop As Double
        Dim CondensedWater As Double
        Dim HeatTransferred As Double
        Dim SensibleHeat As Double

    End Structure

    Private Function Accessory_Calculate(dcHeatRecoveryModel As CLDCHeatRecoveryModel,
     coilType As CLCOCoilType,
     inAirFlow As Double,
     inFluidType As CLCOFluidType,
     inFluidTypeTec As Double,
     inSupplyOutletTemp As Double,
     inSupplyOutletRH As Double,
     inInletTemperature As Double,
     inOutletTemperature As Double,
     ByRef outputData As CLAccessoryCalculateOutputData) As CLAccessory_CalculateResult

        Dim data As New CLCOILStructure
        Dim waterCoil_FinStep As CLDCEnumItem
        Dim waterCoil_Connection As CLDCEnumItem
        Dim waterCoil_Length As Integer
        Dim waterCoil_Height As Integer
        Dim waterCoil_NumberOfRows As Integer
        Dim waterCoil_NumberOfCircuits As Integer

        If coilType = CLCOCoilType.Cooling Then
            waterCoil_FinStep = dcHeatRecoveryModel.CLEnumItem_CWDFinsStep
            waterCoil_Connection = dcHeatRecoveryModel.CLEnumItem_CWDHeaderType
            waterCoil_Length = dcHeatRecoveryModel.CWD_Length
            waterCoil_Height = dcHeatRecoveryModel.CWD_Height
            waterCoil_NumberOfRows = dcHeatRecoveryModel.CWD_NumerOfRows
            waterCoil_NumberOfCircuits = dcHeatRecoveryModel.CWD_NumerOfCircuits
        Else
            waterCoil_FinStep = dcHeatRecoveryModel.CLEnumItem_HWDFinsStep
            waterCoil_Connection = dcHeatRecoveryModel.CLEnumItem_HWDHeaderType
            waterCoil_Length = dcHeatRecoveryModel.HWD_Length
            waterCoil_Height = dcHeatRecoveryModel.HWD_Height
            waterCoil_NumberOfRows = dcHeatRecoveryModel.HWD_NumerOfRows
            waterCoil_NumberOfCircuits = dcHeatRecoveryModel.HWD_NumerOfCircuits
        End If

        With data

            .COIL = coilType
            .GEOMETRY = CLCOGeometry._25_10S
            .ROWSMAT = CLCORowMaterial.CU_0_28

            .FINSPACING = Accessory_TranslateFinsStep(waterCoil_FinStep)

            .FINSPACINGFREE = 0
            .FINMAT = CLCOFinMaterial.AL_0_10
            .FINOPTION = CLCOFinOption.No

            .FLUID_TYPE = inFluidType
            .FLUID_TYPE_TEC_ = inFluidTypeTec

            .HEADERMAT_DE_ = CLCOHeaderMaterial.Cu
            .HEADERTYPE_DC_ = Accessory_TranslateHeaderType(waterCoil_Connection)
            .TYPEOFCIRCUITS = CLCOTypeOfCircuits.Same
            .FREETUBES = 0
            .FLUID_MAXPRESSUREWORK = 0
            .FLUID_MAXPRESSUREDROP = 0
            .SEALEVEL = 0
            .FOULINGFACTOR = 0
            .SAFETYFACTOR = 0

            .AIR_FLOWRATE = inAirFlow / 3600
            .AIR_VELOCITY = 0
            .AIR_INLETTEMP = inSupplyOutletTemp
            .AIR_HUMIDITYIN = inSupplyOutletRH / 100
            .AIR_OUTLETTEMP = 0

            .FLUID_INLETTEMP_TS_ = inInletTemperature
            .FLUID_OUTLETTEMP_TS_ = inOutletTemperature

            .FLUID_FLOWRATE_TC_ = 0
            .NOMINALCAPACITY = 0
            .LENGHT = waterCoil_Length
            .HEIGHT = waterCoil_Height
            .NUMBEROFROWS = waterCoil_NumberOfRows
            .NUMBEROFCIRCUITS = waterCoil_NumberOfCircuits
            .HEADERCONF = 0

            .AUXILIARY = 0
            .AIR__STD = 0
            .FLUID__NAME = ""
            .FLUID__SPECHEAD = 0
            .FLUID__SPECWEIGHT = 0
            .FLUID__VISCOSITY = 0
            .FLUID__CONDUCTIVITY = 0
        End With

        CLCOIL.Calculate(data)

        ' Out
        outputData.OutletTemperature = Math.Round(data._AIR_OUTLETTEMP, 2)
        outputData.OutletRH = Math.Round(data._AIR_HUMIDITYOUT * 100, 2)
        outputData.PressureDrop = Math.Round(data._AIR_PRESSUREDROP, 2)
        outputData.CondensedWater = Math.Round(data._AIR_CONDENSEDWATER * 3600, 2)
        outputData.HeatTransferred = Math.Round(data._MAXCAPACITY, 2)
        outputData.SensibleHeat = Math.Round(data._MAXCAPACITY * data._SHF, 2)

        Return data.AUXILIARY

    End Function

#End Region

#Region "====[ Calculate ]===="

    Public Enum CLCalculateState
        Ok
        AccessoryError
    End Enum

    Private Sub SetCalculateState(state As CLCalculateState, Optional errorMessage As String = "")
        m_CalculateState = state
        m_ErrorMessage = errorMessage
    End Sub

    Public Sub Calculate(dcHeatRecoveryModel As CLDCHeatRecoveryModel,
       inAirFlow As Double,
       inSupplyOutletTemp As Double,
       inSupplyOutletRH As Double)

        Dim accessoryCalculateOutputData As CLAccessoryCalculateOutputData
        Dim accessoryCalculateResult As Nullable(Of CLAccessory_CalculateResult) = New Nullable(Of CLAccessory_CalculateResult)
        Dim clearValue As Boolean = False
        Dim accessoryCWDHeatTransferred As Double = 0.0
        Dim accessoryCWDSensibleHeat As Double = 0.0
        Dim accessoryHWDHeatTransferred As Double = 0.0
        Dim accessoryHWDSensibleHeat As Double = 0.0

        SetCalculateState(CLCalculateState.Ok)

        ' Calcolo degli accessory
        Select Case m_AccessoryState
            Case CLAccessoryState.CWD

                If m_CWD_InletTemperature = 0 OrElse m_CWD_OutletTemperature = 0 Then
                    SetCalculateState(CLCalculateState.AccessoryError, "CWD Temperature.")
                    Exit Select
                End If

                If m_CWD_OutletTemperature <= m_CWD_InletTemperature Then
                    SetCalculateState(CLCalculateState.AccessoryError, "CWD Temperature.")
                    Exit Select
                End If

                If m_CWD_OutletTemperature >= inSupplyOutletTemp OrElse
                 m_CWD_InletTemperature >= inSupplyOutletTemp Then
                    SetCalculateState(CLCalculateState.AccessoryError, "CWD Temperature.")
                    Exit Select
                End If

                accessoryCalculateResult = Accessory_Calculate(dcHeatRecoveryModel,
                  CLCOCoilType.Cooling,
                  inAirFlow,
                  m_CWD_FluidType,
                  m_CWD_FluidTypeTec,
                  inSupplyOutletTemp,
                  inSupplyOutletRH,
                  m_CWD_InletTemperature,
                  m_CWD_OutletTemperature,
                  accessoryCalculateOutputData)

                accessoryCWDHeatTransferred = accessoryCalculateOutputData.HeatTransferred
                accessoryCWDSensibleHeat = accessoryCalculateOutputData.SensibleHeat

            Case CLAccessoryState.HWD

                If m_HWD_InletTemperature = 0 OrElse m_HWD_OutletTemperature = 0 Then
                    SetCalculateState(CLCalculateState.AccessoryError, "HWD Temperature.")
                    Exit Select
                End If

                If m_HWD_OutletTemperature >= m_HWD_InletTemperature Then
                    SetCalculateState(CLCalculateState.AccessoryError, "HWD Temperature.")
                    Exit Select
                End If

                If m_HWD_OutletTemperature <= inSupplyOutletTemp OrElse
                 m_HWD_InletTemperature <= inSupplyOutletTemp Then
                    SetCalculateState(CLCalculateState.AccessoryError, "HWD Temperature.")
                    Exit Select
                End If

                accessoryCalculateResult = Accessory_Calculate(dcHeatRecoveryModel,
                 CLCOCoilType.Heating,
                 inAirFlow,
                 m_HWD_FluidType,
                 m_HWD_FluidTypeTec,
                 inSupplyOutletTemp,
                 inSupplyOutletRH,
                 m_HWD_InletTemperature,
                 m_HWD_OutletTemperature,
                 accessoryCalculateOutputData)

                accessoryHWDHeatTransferred = accessoryCalculateOutputData.HeatTransferred
                accessoryHWDSensibleHeat = accessoryCalculateOutputData.SensibleHeat

            Case CLAccessoryState.CWD_HWD

                Dim accessoryCWDCalculateOutputData As CLAccessoryCalculateOutputData

                If m_CWD_InletTemperature = 0 OrElse m_CWD_OutletTemperature = 0 Then
                    SetCalculateState(CLCalculateState.AccessoryError, "CWD Temperature.")
                    Exit Select
                End If

                If m_CWD_OutletTemperature <= m_CWD_InletTemperature Then
                    SetCalculateState(CLCalculateState.AccessoryError, "CWD Temperature.")
                    Exit Select
                End If

                If m_CWD_OutletTemperature >= inSupplyOutletTemp OrElse
                 m_CWD_InletTemperature >= inSupplyOutletTemp Then
                    SetCalculateState(CLCalculateState.AccessoryError, "CWD Temperature.")
                    Exit Select
                End If

                If m_HWD_InletTemperature = 0 OrElse m_HWD_OutletTemperature = 0 Then
                    SetCalculateState(CLCalculateState.AccessoryError, "HWD Temperature.")
                    Exit Select
                End If

                If m_HWD_OutletTemperature >= m_HWD_InletTemperature Then
                    SetCalculateState(CLCalculateState.AccessoryError, "HWD Temperature.")
                    Exit Select
                End If

                If m_HWD_OutletTemperature <= inSupplyOutletTemp OrElse
                 m_HWD_InletTemperature <= inSupplyOutletTemp Then
                    SetCalculateState(CLCalculateState.AccessoryError, "HWD Temperature.")
                    Exit Select
                End If

                accessoryCalculateResult = Accessory_Calculate(dcHeatRecoveryModel,
                  CLCOCoilType.Cooling,
                  inAirFlow,
                  m_CWD_FluidType,
                  m_CWD_FluidTypeTec,
                  inSupplyOutletTemp,
                  inSupplyOutletRH,
                  m_CWD_InletTemperature,
                  m_CWD_OutletTemperature,
                  accessoryCWDCalculateOutputData)

                If accessoryCalculateResult.Value <> CLAccessory_CalculateResult.Ok Then
                    Exit Select
                End If

                accessoryCalculateResult = Accessory_Calculate(dcHeatRecoveryModel,
                 CLCOCoilType.Heating,
                 inAirFlow,
                 m_HWD_FluidType,
                 m_HWD_FluidTypeTec,
                 accessoryCWDCalculateOutputData.OutletTemperature,
                 accessoryCWDCalculateOutputData.OutletRH,
                 m_HWD_InletTemperature,
                 m_HWD_OutletTemperature,
                 accessoryCalculateOutputData)

                accessoryCWDHeatTransferred = accessoryCWDCalculateOutputData.HeatTransferred
                accessoryCWDSensibleHeat = accessoryCWDCalculateOutputData.SensibleHeat
                accessoryHWDHeatTransferred = accessoryCalculateOutputData.HeatTransferred
                accessoryHWDSensibleHeat = accessoryCalculateOutputData.SensibleHeat
        End Select

        If Not accessoryCalculateResult.HasValue Then
            clearValue = True
        Else
            Select Case accessoryCalculateResult.Value
                Case CLAccessory_CalculateResult.Ok
                    Accessory_Temp = accessoryCalculateOutputData.OutletTemperature
                    Accessory_RH = accessoryCalculateOutputData.OutletRH
                    Accessory_PressureDrop = accessoryCalculateOutputData.PressureDrop
                    Accessory_CondensedWater = accessoryCalculateOutputData.CondensedWater
                    CWD_HeatTransferred = accessoryCWDHeatTransferred
                    CWD_SensibleHeat = accessoryCWDSensibleHeat
                    HWD_HeatTransferred = accessoryHWDHeatTransferred

                Case CLAccessory_CalculateResult.InError
                    SetCalculateState(CLCalculateState.AccessoryError, "Errore durante il calcolo")
                    clearValue = True

                Case CLAccessory_CalculateResult.PressureDropTooHigh
                    SetCalculateState(CLCalculateState.AccessoryError, "PressureDropTooHigh")
                    clearValue = True

                Case CLAccessory_CalculateResult.Ice
                    SetCalculateState(CLCalculateState.AccessoryError, "Ice")
                    clearValue = True

            End Select
        End If

        If clearValue Then
            Accessory_Temp = 0
            Accessory_RH = 0
            Accessory_PressureDrop = 0
            Accessory_CondensedWater = 0
            CWD_SensibleHeat = 0
            CWD_HeatTransferred = 0
            HWD_HeatTransferred = 0
        End If

        OnCalculated()

    End Sub

    Public Event Calculated As EventHandler

    Protected Sub OnCalculated()
        RaiseEvent Calculated(Me, New EventArgs())
    End Sub

#Region "====[ Properties ]===="

    Private m_CalculateState As CLCalculateState
    Public ReadOnly Property CalculateState As CLCalculateState
        Get
            Return m_CalculateState
        End Get
    End Property

    Private m_ErrorMessage As String
    Public ReadOnly Property ErrorMessage As String
        Get
            Return m_ErrorMessage
        End Get
    End Property

#End Region

#End Region

End Class
