Imports System.IO
Imports Climalombarda.DataCentral.LTModel
Imports Climalombarda.DataCentral
Imports Climalombarda.Common.EntityFramework
Imports Climalombarda.Common

Public Class CLLanguage

    Sub New(code As String,
        name As String)
        m_Code = code
        m_Name = name
        m_Enabled = True
    End Sub

    Private m_Code As String
    Public ReadOnly Property Code As String

        Get
            Return m_Code
        End Get

    End Property

    Private m_Name As String
    Public ReadOnly Property Name As String

        Get
            Return m_Name
        End Get

    End Property

    Private m_Enabled As Boolean
    Public Property Enabled As Boolean

        Get
            Return m_Enabled
        End Get
        Set(value As Boolean)
            m_Enabled = value
        End Set

    End Property

End Class

Public Class CLCustomerBranch

    Public Sub New(code As String, name As String, shortname As String)
        m_Code = code
        m_Name = name
        m_ShortName = shortname
    End Sub

    Private m_Code As String
    Public ReadOnly Property Code As String
        Get
            Return m_Code
        End Get
    End Property

    Private m_Name As String
    Public ReadOnly Property Name As String
        Get
            Return m_Name
        End Get
    End Property

    Private m_ShortName As String
    Public ReadOnly Property ShortName As String
        Get
            Return m_ShortName
        End Get
    End Property

End Class

Public Class CLEnvironment

    Private Shared m_Current As CLEnvironment

    Public Shared Property Current As CLEnvironment
        Get
            Return m_Current
        End Get
        Set(value As CLEnvironment)
            m_Current = value
        End Set
    End Property

    Public Const ExternalLeakage As String = "EN 13141-7  EN 13141-4"
    Public Const InternalLeakage As String = "EN 13141-7  EN 13141-4"
    Public Const AirflowPressure As String = "EN 5801  EN 13141-7  EN 13141-4 "
    Public Const ElectricPowerInput As String = "EN 5801  EN 13141-7  EN 13141-4 "
    Public Const NoiseLevel As String = "EN 5135  EN 5136"

    Public Const LanguageCode_BG As String = "bg"
    Public Const LanguageCode_DE As String = "de"
    Public Const LanguageCode_EN As String = "en"
    Public Const LanguageCode_FR As String = "fr"
    Public Const LanguageCode_IT As String = "it"
    Public Const LanguageCode_NL As String = "nl"
    Public Const LanguageCode_PL As String = "pl"
    Public Const LanguageCode_RO As String = "ro"
    Public Const LanguageCode_SL As String = "sl"
    Public Const LanguageCode_HU As String = "hu"
    Public Const LanguageCode_DA As String = "da"

    Public Const ModelCode_CLRC_13_SSC As String = "CLRC 013 SSC"
    Public Const ModelCode_CLRC_13_OSC As String = "CLRC 013 OSC"
    Public Const ModelCode_CLRC_23 As String = "CLRC 023"
    Public Const ModelCode_CLRC_33 As String = "CLRC 033"
    Public Const ModelCode_CLRC_43 As String = "CLRC 043"
    Public Const ModelCode_CLRC_53 As String = "CLRC 053"
    Public Const ModelCode_CLRC_73 As String = "CLRC 073"
    Public Const ModelCode_CLRC_93 As String = "CLRC 093"
    Public Const ModelCode_CLRC_123 As String = "CLRC 123"
    Public Const ModelCode_CLRC_163 As String = "CLRC 163"
    Public Const ModelCode_CLRC_223 As String = "CLRC 223"
    Public Const ModelCode_CLRC_323 As String = "CLRC 323"
    Public Const ModelCode_CLRC_423 As String = "CLRC 423"
    Public Const ModelCode_CLRC_543 As String = "CLRC 543"

    Public Const ModelCode_QUANTUM_25 As String = "QUANTUM 25"
    Public Const ModelCode_QUANTUM_35 As String = "QUANTUM 35"
    Public Const ModelCode_QUANTUM_45 As String = "QUANTUM 45"
    Public Const ModelCode_QUANTUM_55 As String = "QUANTUM 55"

    Public Const ModelCode_CLRC_28_SSC As String = "CLRC 028 SSC"

    Public Const ModelCode_CLRC_38_SSC As String = "CLRC 38 SSC"
    Public Const ModelCode_CLRC_48_SSC As String = "CLRC 48 SSC"
    Public Const ModelCode_CLRC_68_SSC As String = "CLRC 68 SSC"
    Public Const ModelCode_CLRC_38_OSC As String = "CLRC 38 OSC"
    Public Const ModelCode_CLRC_48_OSC As String = "CLRC 48 OSC"
    Public Const ModelCode_CLRC_68_OSC As String = "CLRC 68 OSC"

    Public Const ModelCode_SG_47_CDR As String = "SG 047 CDR"
    Public Const ModelCode_SG_47_CFD As String = "SG 047 CFD"
    Public Const ModelCode_SG_47_HOC As String = "SG 047 HCI"
    Public Const ModelCode_SG_47_TSC As String = "SG 047 CFI"
    Public Const ModelCode_SG_77_CDR As String = "SG 077 CDR"
    Public Const ModelCode_SG_77_CFD As String = "SG 077 CFD"
    Public Const ModelCode_SG_77_HOC As String = "SG 077 HCI"
    Public Const ModelCode_SG_77_TSC As String = "SG 077 CFI"
    Public Const ModelCode_SG_127_CDR As String = "SG 127 CDR"
    Public Const ModelCode_SG_127_CFD As String = "SG 127 CFD"
    Public Const ModelCode_SG_127_HOC As String = "SG 127 HCI"
    Public Const ModelCode_SG_127_FS As String = "SG 127 FS"
    Public Const ModelCode_SG_127_TSC As String = "SG 127 CFI"

    Public Class CLModelDimension
        Public Size As Integer
        Public AeraulicConnection As String
        Public Orientation As String
        Public DimensionA As Integer
        Public DimensionB As Integer
        Public DimensionC As Integer
    End Class

    Private m_Initialized As Boolean = False

    Public Sub New(dataCentralLitePath As String, sswInfo As CLSSWInfo)

        m_Localization = New CLLocalization()

        If sswInfo Is Nothing Then
            Throw New ArgumentNullException(CLObjectHelper.GetMemberName(Function() sswInfo))
        End If

        If Not File.Exists(dataCentralLitePath) Then
            Throw New Exception("Fatal Error: DataCentral not found.")
        End If

        m_SSWInfo = sswInfo

        ' Add Languages
        AddLanguage(New CLLanguage(LanguageCode_IT, "ITALIAN"))
        AddLanguage(New CLLanguage(LanguageCode_DE, "DEUTSCH"))
        AddLanguage(New CLLanguage(LanguageCode_EN, "ENGLISH"))
        AddLanguage(New CLLanguage(LanguageCode_FR, "FRANÇAIS"))
        AddLanguage(New CLLanguage(LanguageCode_NL, "DUTCH"))
        AddLanguage(New CLLanguage(LanguageCode_PL, "POLISH"))
        AddLanguage(New CLLanguage(LanguageCode_SL, "SLOVENIAN"))
        AddLanguage(New CLLanguage(LanguageCode_BG, "BULGARIAN"))
        AddLanguage(New CLLanguage(LanguageCode_RO, "ROMANIAN"))
        AddLanguage(New CLLanguage(LanguageCode_HU, "HUNGARIAN"))
        AddLanguage(New CLLanguage(LanguageCode_DA, "DANSK"))

        ModelDimensions_Initialize()

        m_Initialized = True

        DCConnect(dataCentralLitePath)

        m_SSWInfo.PrepareEnvironment(Me)

    End Sub

#Region "====[ ModelDimensions ]===="

    Private m_ModelDimensions As CLModelDimension()

    Public Sub ModelDimensions_Initialize()

        m_ModelDimensions =
        {
            New CLModelDimension With {.Size = 123, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1580, .DimensionB = 1230, .DimensionC = 480},
            New CLModelDimension With {.Size = 123, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 1580, .DimensionB = 1230, .DimensionC = 480},
            New CLModelDimension With {.Size = 123, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 1830, .DimensionB = 1130, .DimensionC = 480},
            New CLModelDimension With {.Size = 13, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 460, .DimensionB = 1020, .DimensionC = 185},
            New CLModelDimension With {.Size = 13, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 460, .DimensionB = 1020, .DimensionC = 185},
            New CLModelDimension With {.Size = 13, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 670, .DimensionB = 500, .DimensionC = 300},
            New CLModelDimension With {.Size = 163, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1250, .DimensionB = 1600, .DimensionC = 650},
            New CLModelDimension With {.Size = 163, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 1250, .DimensionB = 1250, .DimensionC = 820},
            New CLModelDimension With {.Size = 163, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 1750, .DimensionB = 1350, .DimensionC = 820},
            New CLModelDimension With {.Size = 223, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1550, .DimensionB = 1600, .DimensionC = 650},
            New CLModelDimension With {.Size = 223, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 1550, .DimensionB = 1250, .DimensionC = 820},
            New CLModelDimension With {.Size = 223, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 2000, .DimensionB = 1350, .DimensionC = 820},
            New CLModelDimension With {.Size = 23, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 580, .DimensionB = 1110, .DimensionC = 265},
            New CLModelDimension With {.Size = 23, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 580, .DimensionB = 1110, .DimensionC = 265},
            New CLModelDimension With {.Size = 23, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 665, .DimensionB = 815, .DimensionC = 300},
            New CLModelDimension With {.Size = 25, .AeraulicConnection = "", .Orientation = "HOR", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 25, .AeraulicConnection = "", .Orientation = "VER", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 323, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1850, .DimensionB = 1600, .DimensionC = 650},
            New CLModelDimension With {.Size = 323, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 1850, .DimensionB = 1250, .DimensionC = 820},
            New CLModelDimension With {.Size = 323, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 2400, .DimensionB = 1500, .DimensionC = 820},
            New CLModelDimension With {.Size = 33, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 580, .DimensionB = 1110, .DimensionC = 265},
            New CLModelDimension With {.Size = 33, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 580, .DimensionB = 1110, .DimensionC = 265},
            New CLModelDimension With {.Size = 33, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 665, .DimensionB = 815, .DimensionC = 300},
            New CLModelDimension With {.Size = 35, .AeraulicConnection = "", .Orientation = "HOR", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 35, .AeraulicConnection = "", .Orientation = "VER", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 423, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 2250, .DimensionB = 1600, .DimensionC = 650},
            New CLModelDimension With {.Size = 423, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 2250, .DimensionB = 1250, .DimensionC = 820},
            New CLModelDimension With {.Size = 423, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 2800, .DimensionB = 1550, .DimensionC = 820},
            New CLModelDimension With {.Size = 43, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 670, .DimensionB = 1178, .DimensionC = 315},
            New CLModelDimension With {.Size = 43, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 670, .DimensionB = 1178, .DimensionC = 315},
            New CLModelDimension With {.Size = 43, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 590, .DimensionB = 1060, .DimensionC = 580},
            New CLModelDimension With {.Size = 45, .AeraulicConnection = "", .Orientation = "HOR", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 45, .AeraulicConnection = "", .Orientation = "VER", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 53, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 865, .DimensionB = 1178, .DimensionC = 315},
            New CLModelDimension With {.Size = 53, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 865, .DimensionB = 1178, .DimensionC = 315},
            New CLModelDimension With {.Size = 53, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 1080, .DimensionB = 970, .DimensionC = 300},
            New CLModelDimension With {.Size = 543, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 2800, .DimensionB = 1500, .DimensionC = 1120},
            New CLModelDimension With {.Size = 55, .AeraulicConnection = "", .Orientation = "HOR", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 55, .AeraulicConnection = "", .Orientation = "VER", .DimensionA = 700, .DimensionB = 750, .DimensionC = 490},
            New CLModelDimension With {.Size = 73, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1180, .DimensionB = 1230, .DimensionC = 480},
            New CLModelDimension With {.Size = 73, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 1180, .DimensionB = 1230, .DimensionC = 480},
            New CLModelDimension With {.Size = 73, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 1530, .DimensionB = 1130, .DimensionC = 480},
            New CLModelDimension With {.Size = 93, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1180, .DimensionB = 1230, .DimensionC = 480},
            New CLModelDimension With {.Size = 93, .AeraulicConnection = "OSC", .Orientation = "VER", .DimensionA = 1180, .DimensionB = 1230, .DimensionC = 480},
            New CLModelDimension With {.Size = 93, .AeraulicConnection = "SSC", .Orientation = "VER", .DimensionA = 1530, .DimensionB = 1130, .DimensionC = 480},
            New CLModelDimension With {.Size = 47, .AeraulicConnection = "HCI", .Orientation = "HOR", .DimensionA = 1560, .DimensionB = 1360, .DimensionC = 250},
            New CLModelDimension With {.Size = 47, .AeraulicConnection = "CFI", .Orientation = "HOR", .DimensionA = 1560, .DimensionB = 1360, .DimensionC = 250},
            New CLModelDimension With {.Size = 77, .AeraulicConnection = "HCI", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1360, .DimensionC = 250},
            New CLModelDimension With {.Size = 77, .AeraulicConnection = "CFI", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1360, .DimensionC = 250},
            New CLModelDimension With {.Size = 127, .AeraulicConnection = "HCI", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1360, .DimensionC = 350},
            New CLModelDimension With {.Size = 127, .AeraulicConnection = "CFI", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1360, .DimensionC = 350},
            New CLModelDimension With {.Size = 47, .AeraulicConnection = "CFD", .Orientation = "HOR", .DimensionA = 1560, .DimensionB = 1550, .DimensionC = 250},
            New CLModelDimension With {.Size = 47, .AeraulicConnection = "CDR", .Orientation = "HOR", .DimensionA = 1560, .DimensionB = 1550, .DimensionC = 250},
            New CLModelDimension With {.Size = 77, .AeraulicConnection = "CFD", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1550, .DimensionC = 250},
            New CLModelDimension With {.Size = 77, .AeraulicConnection = "CDR", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1550, .DimensionC = 250},
            New CLModelDimension With {.Size = 127, .AeraulicConnection = "CFD", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1700, .DimensionC = 350},
            New CLModelDimension With {.Size = 127, .AeraulicConnection = "CDR", .Orientation = "HOR", .DimensionA = 1860, .DimensionB = 1700, .DimensionC = 350},
            New CLModelDimension With {.Size = 60, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 780, .DimensionB = 1680, .DimensionC = 330},
            New CLModelDimension With {.Size = 90, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 965, .DimensionB = 2000, .DimensionC = 415},
            New CLModelDimension With {.Size = 130, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1220, .DimensionB = 2170, .DimensionC = 415},
            New CLModelDimension With {.Size = 180, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1220, .DimensionB = 2250, .DimensionC = 495},
            New CLModelDimension With {.Size = 250, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1740, .DimensionB = 2350, .DimensionC = 495},
            New CLModelDimension With {.Size = 320, .AeraulicConnection = "OSC", .Orientation = "HOR", .DimensionA = 1740, .DimensionB = 2395, .DimensionC = 630}
        }
    End Sub

    Public ReadOnly Property ModelDimensions As CLModelDimension()
        Get
            Return m_ModelDimensions.ToArray()
        End Get
    End Property

    Public ReadOnly Property ModelDimensionCount As Integer
        Get
            Return m_ModelDimensions.Count
        End Get
    End Property

    Public Function FindModelDimensionsByModel(dcHeatRecoveryModel As CLDCHeatRecoveryModel) As CLModelDimension()

        If dcHeatRecoveryModel.CLEnumItem_AeraulicConnection Is Nothing Then
            Return m_ModelDimensions.Where(Function(x) x.Size = dcHeatRecoveryModel.Size).ToArray()
        End If

        Return m_ModelDimensions.Where(Function(x) x.Size = dcHeatRecoveryModel.Size.Value AndAlso x.AeraulicConnection = dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode).ToArray()
    End Function

#End Region

#Region "====[ Data Central Database ]===="

    Public Sub DCConnect(dcLiteDatabasePath As String)

        Dim efConnectionString As String

        efConnectionString = CLEntityFrameworkHelper.EFSqlServerCECreateConnectionString(
            String.Format("Data Source=""{0}""; Password=""{1}""", dcLiteDatabasePath, "@D3C1L4T2%"),
            CLDataCentralLTEntities.ModelName,
            GetType(CLDataCentralLTEntities))

        m_DCContext = New CLDataCentralLTEntities(efConnectionString)
        m_SSWConfiguration = DCContext.CLSSWConfigurations.FirstOrDefault()
        If m_SSWConfiguration Is Nothing Then
            Throw New Exception("Fatal Error: Invalid DataCentral (Configuration not found).")
        End If

        If m_SSWConfiguration.CLCustomer.Code <> m_SSWInfo.Code Then
            Throw New Exception("Fatal Error: Invalid DataCentral (Mismatch Customer Code).")
        End If

        If m_DCContext.CLDCHeatRecoveryModels.Count() = 0 Then
            Throw New Exception("Fatal Error: Invalid DataCentral (No data).")
        End If

        m_DCLanguages = New CLEnums(m_DCContext, CLDCEnumCode.Language)
        m_SerieVariantQuery = New CLDCSerieVariantQuery("SSW", m_DCContext)
        m_HeatRecoveryModelVariantQuery = New CLDCHeatRecoveryModelVariantQuery("SSW", m_DCContext)
        m_ProductVariantQuery = New CLDCProductVariantQuery("SSW", m_DCContext)
    End Sub

    Private m_DCContext As CLDataCentralLTEntities
    Public ReadOnly Property DCContext As CLDataCentralLTEntities
        Get
            Return m_DCContext
        End Get
    End Property

#Region "====[ Customer ]===="

    Public ReadOnly Property DCCustomer As CLDCCustomer
        Get
            Return IIf(m_SSWConfiguration Is Nothing, Nothing, m_SSWConfiguration.CLCustomer)
        End Get
    End Property

    Public ReadOnly Property CustomerCode As String
        Get
            If DCCustomer Is Nothing Then Return ""

            Return DCCustomer.Code
        End Get
    End Property

    Public ReadOnly Property CustomerLogo As Image
        Get
            If DCCustomer Is Nothing OrElse DCCustomer.SSWLogoImage Is Nothing Then
                MessageBox.Show("No logo","logo error",MessageBoxButtons.OK)
                Return Nothing
            End If


            Dim logo As Image = Nothing

            Try

                Dim memoryStream As MemoryStream

                memoryStream = New MemoryStream(DCCustomer.SSWLogoImage)

                logo = Image.FromStream(memoryStream)

            Catch ex As Exception
                MessageBox.Show(ex.ToString(), "logo Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try

            Return logo
        End Get
    End Property

    Public ReadOnly Property CustomerIcon As Icon
        Get
            If DCCustomer Is Nothing OrElse DCCustomer.SSWIcon Is Nothing Then
                MessageBox.Show("No icon", "Icon Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return Nothing
            End If

            Dim icon As Icon = Nothing

            Try

                Dim memoryStream As MemoryStream

                memoryStream = New MemoryStream(DCCustomer.SSWIcon)

                icon = New Icon(memoryStream)

            Catch ex As Exception
                MessageBox.Show(ex.ToString(), "Icon Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try

            Return icon
        End Get
    End Property

    Public ReadOnly Property CustomerProfile As String
        Get
            If DCCustomer Is Nothing Then Return "CL"

            Return DCCustomer.Profile
        End Get
    End Property

    Public ReadOnly Property CustomerLanguageCode As String
        Get
            If DCCustomer Is Nothing Then Return Nothing

            If DCCustomer.CLEnumItem_Language Is Nothing Then Return "EN"

            Return DCCustomer.CLEnumItem_Language.TextCode
        End Get
    End Property

#End Region

#Region "====[ Configuration ]===="

    Private m_SSWConfiguration As CLSSWConfiguration
    Public ReadOnly Property SSWConfiguration As CLSSWConfiguration
        Get
            Return m_SSWConfiguration
        End Get
    End Property

#End Region

#Region "====[ Serie ]===="

    Private m_SerieVariantQuery As CLDCSerieVariantQuery
    Public ReadOnly Property SerieVariantQuery As CLDCSerieVariantQuery
        Get
            Return m_SerieVariantQuery
        End Get
    End Property

    Public Function GetCustomerSerieName(dcSerie As CLDCSerie) As String

        Return m_SerieVariantQuery.GetName(dcSerie.Id,
            CLVariantParameter.CreateWithoutNullParameters(
            CLVariantParameterCustomerCode.Create(CustomerCode, True),
            CLVariantParameterCustomerProfile.Create(CustomerProfile, True),
            CLVariantParameterLanguageCode.Create(CustomerLanguageCode, True, True)))

    End Function

#End Region

#Region "====[ HeatRecoveryModel ]===="

    Private m_HeatRecoveryModelVariantQuery As CLDCHeatRecoveryModelVariantQuery
    Public ReadOnly Property HeatRecoveryModelVariantQuery As CLDCHeatRecoveryModelVariantQuery
        Get
            Return m_HeatRecoveryModelVariantQuery
        End Get
    End Property

    Public Function GetCustomerHeatRecoveryModelName(dcHeatRecoveryModel As CLDCHeatRecoveryModel) As String

        Return m_HeatRecoveryModelVariantQuery.GetName(dcHeatRecoveryModel.Id,
            CLVariantParameter.CreateWithoutNullParameters(
            CLVariantParameterCustomerCode.Create(CustomerCode, True),
            CLVariantParameterCustomerProfile.Create(IIf(String.IsNullOrEmpty(DCCustomer.SSWProfile), DCCustomer.Profile, DCCustomer.SSWProfile), True),
            CLVariantParameterLanguageCode.Create(CustomerLanguageCode, True, True)))

    End Function

#End Region

#Region "====[ Product ]===="

    Private m_ProductVariantQuery As CLDCProductVariantQuery
    Public ReadOnly Property ProductVariantQuery As CLDCProductVariantQuery
        Get
            Return m_ProductVariantQuery
        End Get
    End Property

#End Region

#Region "====[ Languages ]===="

    Private m_DCLanguages As CLEnums
    Public ReadOnly Property DCLanguages As CLEnums
        Get
            Return m_DCLanguages
        End Get
    End Property

#End Region
#End Region

#Region "====[ Localization ]===="

#Region "====[ Languages ]===="

    Private m_Languages As New List(Of CLLanguage)

    Public ReadOnly Property Languages As CLLanguage()

        Get
            Return m_Languages.ToArray()
        End Get

    End Property

    Public ReadOnly Property LanguageCount As Integer

        Get
            Return m_Languages.Count
        End Get

    End Property

    Public Function FindLanguage(ByVal code As String) As CLLanguage
        For Each language As CLLanguage In m_Languages
            If language.Code = code Then
                Return language
            End If
        Next
        Return Nothing
    End Function

    Public Function GetLanguage(ByVal index As Integer) As CLLanguage
        Return m_Languages(index)
    End Function

    Public Sub DisableAllLanguages()
        For Each language As CLLanguage In m_Languages
            language.Enabled = False
        Next
    End Sub

    Private Sub AddLanguage(language As CLLanguage)

        Dim path As String

        path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location),
            String.Format("{0}\{1}.resources.dll", language.Code, "SSWLib"))

        m_Localization.AddResource(language.Code, path, String.Format("SSW.Resources.{0}", language.Code))
        m_Languages.Add(language)
    End Sub

    Public ReadOnly Property ENLanguage As CLLanguage
        Get
            Return FindLanguage(LanguageCode_EN)
        End Get
    End Property
#End Region

    Private m_Localization As CLLocalization
    Public ReadOnly Property Localization As CLLocalization
        Get
            Return m_Localization
        End Get
    End Property

    Private m_PrimaryLanguage As CLLanguage
    Private m_SecondaryLanguage As CLLanguage

    Public ReadOnly Property PrimaryLanguageCode As String
        Get
            Return m_PrimaryLanguage.Code
        End Get
    End Property

    Public ReadOnly Property PrimaryLanguage As CLLanguage
        Get
            Return m_PrimaryLanguage
        End Get
    End Property

    Public ReadOnly Property SecondaryLanguage As CLLanguage
        Get
            Return m_SecondaryLanguage
        End Get
    End Property

    Public Sub SetLanguage(primaryLanguage As CLLanguage,
        Optional secondaryLanguage As CLLanguage = Nothing)

        If primaryLanguage Is Nothing Then
            primaryLanguage = ENLanguage
        End If
        If secondaryLanguage Is Nothing Then
            secondaryLanguage = ENLanguage
        End If

        m_PrimaryLanguage = primaryLanguage
        m_SecondaryLanguage = secondaryLanguage
        m_Localization.PrimaryResource = m_Localization.GetResource(primaryLanguage.Code)
        m_Localization.SecondaryResource = m_Localization.GetResource(secondaryLanguage.Code)

        If m_Initialized Then
            OnLanguageChanged()
        End If
    End Sub

    Public Event LanguageChanged(ByVal sender As Object, ByVal e As EventArgs)
    Protected Overridable Sub OnLanguageChanged()
        RaiseEvent LanguageChanged(Me, New EventArgs())
    End Sub

#End Region

#Region "====[ SSWInfo ]===="
    Private m_SSWInfo As CLSSWInfo
    Public Property SSWInfo As CLSSWInfo
        Get
            Return m_SSWInfo
        End Get
        Set(value As CLSSWInfo)
            m_SSWInfo = value
        End Set
    End Property

#End Region

    Private m_Branchs As New List(Of CLCustomerBranch)
    Public ReadOnly Property Branchs As CLCustomerBranch()
        Get
            Return m_Branchs.ToArray()
        End Get
    End Property

    Public Sub AddBranch(branch As CLCustomerBranch)
        m_Branchs.Add(branch)
    End Sub

    Public ReadOnly Property HasBranchs
        Get
            Return Not Branchs Is Nothing AndAlso Branchs.Length > 0
        End Get
    End Property

    Private m_Branch As CLCustomerBranch
    Public Property Branch As CLCustomerBranch
        Get
            Return m_Branch
        End Get
        Set(value As CLCustomerBranch)
            m_Branch = value
            OnBranchChanged()
        End Set
    End Property

    Public Event BranchChanged(ByVal sender As Object, ByVal e As EventArgs)
    Protected Overridable Sub OnBranchChanged()
        RaiseEvent BranchChanged(Me, New EventArgs())
    End Sub

End Class