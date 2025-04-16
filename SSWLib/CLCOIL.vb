Imports System.Runtime.InteropServices

Public Enum CLCOCoilType
	Heating = 1
	Cooling = 2
	Condensing = 3
	Evaporating = 4
End Enum

Public Enum CLCOGeometry
	_25_08S = 21
	_25_10S = 22
	_30_10S = 23
	_30_12S = 24
	_35_12L = 25
	_30_16L = 26
	_60_16S = 27
	_60_16L = 28
	_100_16S = 29
End Enum

Public Enum CLCORowMaterial
	CU_0_28 = 21
	CU_0_30 = 22
	CU_0_35 = 23
	CU_0_41 = 24
	CU_0_50 = 25
	CU_0_70 = 26
	CU_1_00 = 27
	CR_0_33 = 28
	CR_0_35 = 29
	AL_0_50 = 30
	AL_0_70 = 31
	X4_0_70 = 32
	X4_1_00 = 33
	X6_0_70 = 34
	X6_1_00 = 35
End Enum

Public Enum CLCOFinSpacing
	_1_6 = 20
	_1_8 = 22
	_2_0 = 23
	_2_1 = 24
	_2_3 = 25
	_2_5 = 26
	_2_8 = 27
	_3_0 = 28
	_3_2 = 29
	_3_5 = 30
	_4_0 = 31
	_4_2 = 32
	_4_5 = 33
	_4_8 = 34
	_5_0 = 35
	_5_5 = 36
	_6_0 = 37
	_6_5 = 38
	_7_0 = 39
	_7_5 = 40
	_8_0 = 41
	_8_5 = 42
	_9_0 = 43
	_9_5 = 44
	_10_0 = 45
	_10_5 = 46
	_11_0 = 47
	_12_0 = 48
	_16_0 = 49
	_22_0 = 50
End Enum

Public Enum CLCOFinMaterial
	AL_0_10 = 31
	AL_0_12 = 32
	AL_0_15 = 33
	AL_0_18 = 34
	AL_0_20 = 35
	AL_0_25 = 36
	AP_0_10 = 37
	AP_0_12 = 38
	AP_0_15 = 39
	AP_0_20 = 40
	AI_0_10 = 41
	AI_0_12 = 42
	AI_0_15 = 43
	AI_0_20 = 44
	CU_0_10 = 45
	CU_0_15 = 46
	CU_0_20 = 47
	X4_0_12 = 48
	X4_0_15 = 49
	X6_0_12 = 50
	X6_0_15 = 51
End Enum

Public Enum CLCOFinOption
	No = 0
	Yes = 1
End Enum

Public Enum CLCOFluidType
	Water = 1
	Glic_Etil = 2
	Glic_Prop = 3
	Pekasol_50 = 4
	Pekasol_L = 5
	Glykosol = 6
	Tyfoxit = 7
	Tyfoxit_F = 8
	Hycool = 9
	Other = 10
End Enum

Public Enum CLCOHeaderMaterial
	Cu = 1
	Fe = 2
End Enum

Public Enum CLCOHeaderType
	_5_16 = 0
	_3_8 = 1
	_1_2 = 2
	_3_4 = 3
	_1 = 4
	_1_1_4 = 5
	_1_1_2 = 6
	_2 = 7
	_2_1_2 = 8
	_3 = 9
	_4 = 10
	_NW125 = 11
	_NW150 = 12
	NO = 13
End Enum

Public Enum CLCOTypeOfCircuits
	Same = 1
	Opp = 2
	Fixed = 3
End Enum

Public Structure CLCOILStructure

	'input
	Dim COIL As Integer
	Dim GEOMETRY As Integer
	Dim ROWSMAT As Integer
	Dim FINSPACING As Integer
	Dim FINSPACINGFREE As Double
	Dim FINMAT As Integer
	Dim FINOPTION As Integer
	Dim FLUID_TYPE As Integer
	Dim FLUID_TYPE_TEC_ As Double
	Dim HEADERMAT_DE_ As Integer
	Dim HEADERTYPE_DC_ As Integer
	Dim TYPEOFCIRCUITS As Integer
	Dim FREETUBES As Double
	Dim FLUID_MAXPRESSUREWORK As Integer
	Dim FLUID_MAXPRESSUREDROP As Double
	Dim SEALEVEL As Double
	Dim FOULINGFACTOR As Double
	Dim SAFETYFACTOR As Double
	Dim AIR_FLOWRATE As Double
	Dim AIR_VELOCITY As Double
	Dim AIR_INLETTEMP As Double
	Dim AIR_HUMIDITYIN As Double
	Dim AIR_OUTLETTEMP As Double
	Dim FLUID_INLETTEMP_TS_ As Double
	Dim FLUID_OUTLETTEMP_TS_ As Double
	Dim FLUID_FLOWRATE_TC_ As Double
	Dim NOMINALCAPACITY As Double
	Dim LENGHT As Double
	Dim HEIGHT As Double
	Dim NUMBEROFROWS As Double
	Dim NUMBEROFCIRCUITS As Double
	Dim HEADERCONF As Integer

	'output
	Dim _AIR_FLOWRATE As Double
	Dim _AIR_VELOCITY As Double
	Dim _AIR_INLETTEMP As Double
	Dim _AIR_HUMIDITYIN As Double
	Dim _AIR_OUTLETTEMP As Double
	Dim _AIR_HUMIDITYOUT As Double
	Dim _AIR_PRESSUREDROP As Double
	Dim _SHF As Double
	Dim _AIR_CONDENSEDWATER As Double
	Dim _FLUID_INLETTEMP_TS As Double
	Dim _FLUID_OUTLETTEMP_TS As Double
	Dim _FLUID_FLOWRATE As Double
	Dim _FLUID_VELOCITY_TEC_ As Double
	Dim _FLUID_PRESSUREDROP As Double
	Dim _NOMINALCAPACITY As Double
	Dim _MAXCAPACITY As Double
	Dim _LENGHT As Double
	Dim _HEIGHT As Double
	Dim _NUMBEROFROWS As Double
	Dim _NUMBEROFCIRCUITS As Double
	Dim _INTSURFACE As Double
	Dim _EXTSURFACE As Double
	Dim _VOLUME As Double
	Dim _WEIGHT As Double
	Dim _AIR_PRESSURE As Double
	Dim _FLUID_HEADERPRESSUREDROP As Double
	Dim _AIR_FLOWRATEKGS As Double
	Dim _FLUID_FLOWRATEKGS As Double
	Dim _AIR_INLETWATER As Double
	Dim _AIR_OUTLETWATER As Double
	Dim _FLUID_INLETVAPORFRACTION As Double
	Dim _CODE As String
	Dim _FLUID As String
	Dim _FLUID_PRESSURETEC As Double

	'others
	Dim AUXILIARY As Double
	Dim PASSWORD As String
	Dim AIR__STD As Double
	Dim FLUID__NAME As String
	Dim FLUID__SPECHEAD As Double
	Dim FLUID__SPECWEIGHT As Double
	Dim FLUID__VISCOSITY As Double
	Dim FLUID__CONDUCTIVITY As Double

End Structure

Public Class CLCOIL

	<DllImport("COILcalc.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.Cdecl)> _
	Public Shared Function COILcalc(ByRef data As CLCOILStructure) As Long

	End Function

	Public Shared Sub Calculate(ByRef data As CLCOILStructure)

        Dim retValue As Long
        'data.PASSWORD = "KFLDBNW4L9FN7S"
        data.PASSWORD = "KFLFMN45698DHN"
        retValue = COILcalc(data)
	End Sub

End Class

