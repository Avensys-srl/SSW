Imports System.IO
Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.Net
Imports System.Windows.Forms.DataVisualization.Charting
Imports Climalombarda.Common
Imports Climalombarda.Common.UI
Imports System.Data
Imports Climalombarda.DataCentral.LTModel
Imports Climalombarda.DataCentral


#Const COIL = False

Public Class CLMainForm

    Private m_SAPEnable As Boolean
    Private m_DataChanging As Boolean = False
    Private m_TabPages As New List(Of TabPage)
    Private m_CoilPerformanceChanging As Boolean = False
    Private m_CoilPerformanceCoils As New List(Of CLCoilDefinition)
    Private m_CoilPerformanceAvailable As Boolean = False
    Private m_CoilPerformanceModelId As Integer = -1
    Private m_CoilPerformanceLastPressureDrop As Double = 0
    Private m_CoilPerformanceLastResults As New List(Of CLCoilCalculationResult)()
    Private m_CoilPerformanceBusy As Boolean = False
    Private m_CoilCustomDisclaimerAccepted As Boolean = False
    Private m_AutomaticUpdateCheckTask As Task
    Private m_ReportRegistrationBusy As Boolean = False
    Private m_ReportGeneratedAsDraft As Boolean = False
    Private ReadOnly m_SelectionApiClient As New CLSelectionApiClient()
    Private m_SummerCalculationEnabled As Boolean = True
    Private m_LastWinterThermo As termo
    Private m_LastSummerThermo As termo
    Private m_HasLastWinterThermo As Boolean = False
    Private m_HasLastSummerThermo As Boolean = False
    Private m_WinterReportScenarioName As String = String.Empty
    Private Const ChartSeries_SummerEfficiencyCurve_Name As String = "SummerEfficiencyCurve"
    Private Const ChartSeries_SummerEfficiencyPoint_Name As String = "SummerEfficiencyPoint"
    Private Const ChartArea_SummerEfficiency_Name As String = "SummerEfficiencyArea"
    Private Const ChartTitle_WinterEfficiency_Name As String = "WinterEfficiencyTitle"
    Private Const ChartTitle_SummerEfficiency_Name As String = "SummerEfficiencyTitle"
    Private Const ChartTitle_ReportEfficiencyAxis_Name As String = "ReportEfficiencyAxisTitle"
    Private Const ChartSeries_ReportWinterEfficiency_Name As String = "ReportWinterEfficiencyCurve"

    Private tbpData_CoilPerformance As TabPage
    Private chbCoilPerformance_Enable As CheckBox
    Private cmbCoilPerformance_EditMode As ComboBox
    Private cmbCoilPerformance_Installation As ComboBox
    Private cmbCoilPerformance_Mode As ComboBox
    Private cmbCoilPerformance_Coil As ComboBox
    Private cmbCoilPerformance_FluidType As ComboBox
    Private nudCoilPerformance_FluidTec As NumericUpDown
    Private nudCoilPerformance_Length As NumericUpDown
    Private nudCoilPerformance_Height As NumericUpDown
    Private nudCoilPerformance_Tubes As NumericUpDown
    Private cmbCoilPerformance_HeightMode As ComboBox
    Private nudCoilPerformance_Rows As NumericUpDown
    Private cmbCoilPerformance_FinSpacing As ComboBox
    Private nudCoilPerformance_Circuits As NumericUpDown
    Private nudCoilPerformance_CoolingIn As NumericUpDown
    Private nudCoilPerformance_CoolingOut As NumericUpDown
    Private nudCoilPerformance_HeatingIn As NumericUpDown
    Private nudCoilPerformance_HeatingOut As NumericUpDown
    Private dgvCoilPerformance_Results As DataGridView
    Private lblCoilPerformance_Status As Label
    Private lblCoilPerformance_CustomWarning As Label
    Private lblCoilPerformance_DimensionsNote As Label

    Private Class CLThermalCalculationResult
        Public Property Thermo As termo
        Public Property WorkPoint As Double()
        Public Property AirFlow As Double
        Public Property Curves As CLPerformanceCurveCalculation
    End Class

    Private ReadOnly Property Environment As CLEnvironment
        Get
            Return CLEnvironment.Current
        End Get
    End Property

    Public Sub New()

        ' Chiamata richiesta dalla finestra di progettazione.
        InitializeComponent()
        Project_InitializeMenus()
        MultiSelection_InitializeMenus()
        Help_InitializeMenus()

        Try
            tsmiOption_CommercialSheetAutoSync.Checked = My.Settings.CommercialSheetAutoSyncEnabled
        Catch
            tsmiOption_CommercialSheetAutoSync.Checked = True
        End Try

        Me.Icon = Environment.SSWInfo.EmbeddedIcon

        Dim culture As CultureInfo

        culture = CultureInfo.CurrentCulture.Clone

        culture.NumberFormat.NumberDecimalSeparator = ","
        culture.NumberFormat.NumberGroupSeparator = "."

        Threading.Thread.CurrentThread.CurrentCulture = culture
        Threading.Thread.CurrentThread.CurrentUICulture = culture

        AddHandler Environment.LanguageChanged, AddressOf Environment_LanguageChanged
        AddHandler Environment.BranchChanged, AddressOf Environment_BranchChanged

        ' Abilita/disabilita le opzioni delle lingue
        tsmiOption_Language_IT.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_IT).Enabled
        tsmiOption_Language_DE.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_DE).Enabled
        tsmiOption_Language_FR.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_FR).Enabled
        tsmiOption_Language_PL.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_PL).Enabled
        tsmiOption_Language_NL.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_NL).Enabled
        tsmiOption_Language_EN.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_EN).Enabled
        tsmiOption_Language_SL.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_SL).Enabled
        tsmiOption_Language_BG.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_BG).Enabled
        tsmiOption_Language_RO.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_RO).Enabled
        tsmiOption_Language_HU.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_HU).Enabled
        tsmiOption_Language_DA.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_DA).Enabled
        tsmiOption_Language_SV.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_SV).Enabled
        tsmiOption_Language_NO.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_NO).Enabled
        tsmiOption_Language_IS.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_IS).Enabled
        tsmiOption_Language_CS.Visible = Environment.FindLanguage(CLEnvironment.LanguageCode_CS).Enabled

        txbPerformance_PassiveHaus_Limit.Text = (0.45D).ToString()
        If Not Environment.HasBranchs Then
            tsmiBranchs.Visible = False
            Environment.AddBranch(New CLCustomerBranch("*", Environment.DCCustomer.Name, Environment.DCCustomer.ShortName))
            Environment.Branch = Environment.Branchs.FirstOrDefault()
        Else
            tsmiBranchs.Visible = True
            For Each branch As CLCustomerBranch In Environment.Branchs
                Dim menuItem As ToolStripMenuItem

                menuItem = New ToolStripMenuItem()
                menuItem.Text = branch.Name
                menuItem.Tag = branch
                AddHandler menuItem.Click, AddressOf Branch_Click

                If Environment.Branch Is branch Then
                    menuItem.Checked = True
                End If
                tsmiBranchs.DropDownItems.Add(menuItem)
            Next
        End If
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        RemoveHandler Environment.LanguageChanged, AddressOf Environment_LanguageChanged
        RemoveHandler Environment.BranchChanged, AddressOf Environment_BranchChanged
        MyBase.OnFormClosed(e)
    End Sub

    Private Sub CLMainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim state As String
        Dim language As CLLanguage

        ' Prepare Sound Performances
        CreateSoundDataTable()
        dgvPerformance_SoundPower.DataSource = m_SoundPerformances
        dgvPerformance_SoundPower.Columns(SoundColumnName_Type).Visible = False

        dgvPerformance_SoundPower.Columns(SoundColumnName_Caption).HeaderText = "LwA" & vbCrLf & "[dB(A)]"

        With dgvPerformance_SoundPower.Columns(SoundColumnName_63Hz)
            .HeaderText = "63" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_125Hz)
            .HeaderText = "125" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_250Hz)
            .HeaderText = "250" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_500Hz)
            .HeaderText = "500" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_1000Hz)
            .HeaderText = "1000" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_2000Hz)
            .HeaderText = "2000" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_4000Hz)
            .HeaderText = "4000" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_8000Hz)
            .HeaderText = "8000" & vbCrLf & "Hz"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_LwA)
            .HeaderText = "Tot"
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_Lp1)
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        With dgvPerformance_SoundPower.Columns(SoundColumnName_Lp2)
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.Format = "N1"
        End With
        For Each dgvColumn As DataGridViewColumn In dgvPerformance_SoundPower.Columns
            dgvColumn.SortMode = DataGridViewColumnSortMode.NotSortable
        Next
        SoundPerformances_UIDGVRefreshColumns()

        If m_NextUiReportHost Then
            CoilPerformance_InitializeTab()
            ElectricHeater_InitializeTab()
            Accessories_InitializeTab()
        End If

        ' Set regional settings
        If m_NextUiReportHost Then
            m_MeasureUnit = CLMeasureUnit.SI
            tsmiOption_Unit_SI.Checked = True
            tsmiOption_Unit_IP.Checked = False
            UpdateLocalization_MeasureUnit()
        Else
            MeasureUnit = CLMeasureUnit.SI
        End If

        state = CultureInfo.CurrentCulture.Name.Substring(3)

        If m_NextUiReportHost Then
            ' The hidden report host must use the document language already
            ' selected by Next UI, independently from the interface culture.
            language = Environment.FindLanguage(Environment.PrimaryLanguageCode)
        Else
            language = Environment.FindLanguage(CultureInfo.CurrentCulture.Name.Substring(0, 2))
        End If
        If language Is Nothing OrElse Not language.Enabled Then
            language = Environment.FindLanguage(Environment.SSWInfo.DefaultLanguage)
        End If
        If language Is Nothing OrElse Not language.Enabled Then
            language = Environment.ENLanguage
        End If

        If Not String.Equals(Environment.PrimaryLanguageCode, language.Code,
            StringComparison.OrdinalIgnoreCase) Then
            Environment.SetLanguage(language)
        Else
            ' The Next UI initializes localization before creating the hidden
            ' legacy report host, so no LanguageChanged event is raised here.
            UpdateLocalization()
        End If
        SeasonalCalculation_UpdateModeButton()

        If state = "GB" Then
            m_SAPEnable = True
        Else
            m_TabPages.Add(tbpCertification)
            tbcMain.TabPages.Remove(tbpCertification)
        End If

        If Not m_NextUiReportHost Then
            CoilPerformance_InitializeTab()
            ElectricHeater_InitializeTab()
            Accessories_InitializeTab()
        End If
        Help_ApplyToolTips()

        txbPerformance_AirFlow.Text = "100"
        TextBox1.Text = txbPerformance_AirFlow.Text
        TextBox3.Text = "32"
        TextBox4.Text = "80"
        TextBox5.Text = "26"
        TextBox6.Text = "50"

        ' Fill Series
        Series_FillCombo()

        sap_table_start()
        txbPerformance_AirFlow.Select()

        'Fill CO2 data
        CO2LevelFill()

        Project_CompleteFormLoad()
        FollowUp_Initialize()

        'CLModule.Environment.ExportModelsToCsv("d:\temp\environment.txt")

        If Not m_NextUiReportHost AndAlso
            Not String.Equals(System.Environment.GetEnvironmentVariable("SSW_TECHNICAL_BASELINE_MODE"),
            "1", StringComparison.Ordinal) Then
            m_AutomaticUpdateCheckTask = UpdateManager.CheckForSoftwareUpdate(False)
        End If
    End Sub

    Private Sub tsmiFile_Exit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiFile_Exit.Click
        Me.Close()
    End Sub

    Private Sub tsmiAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiAbout.Click
        Dim aboutBoxForm As New CLAboutBoxForm

        aboutBoxForm.ShowDialog()
    End Sub

#Region "====[ Series ]===="

    Private Sub Series_FillCombo()

        Dim serieNames As New List(Of String)

        For Each dcSerie As CLDCSerie In Environment.DCContext.CLDCSeries

            Dim serieName As String

            serieName = Environment.GetCustomerSerieName(dcSerie)

            If Not serieNames.Contains(serieName) Then
                serieNames.Add(serieName)
            End If

        Next

        cmbPerformance_Series.Items.Clear()
        cmbPerformance_Series.Items.AddRange(serieNames.ToArray())

        If cmbPerformance_Series.Items.Count > 0 Then
            cmbPerformance_Series.SelectedItem = cmbPerformance_Series.Items(0)
        Else
            MessageBox.Show("No Product Found", "Alert", MessageBoxButtons.OK)
        End If

    End Sub

    Public ReadOnly Property SelectedSerieName As String
        Get
            If cmbPerformance_Series.SelectedItem Is Nothing Then
                MessageBox.Show("No Product Selected", "Alert", MessageBoxButtons.OK)
                Return Nothing
            End If

            Return DirectCast(cmbPerformance_Series.SelectedItem, String)
        End Get
    End Property

    Private Sub cmbPerformance_Series_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbPerformance_Series.SelectedIndexChanged
        HeatRecoveryModels_FillCombo()

        If cmbPerformance_Series.SelectedItem = "UKUNDA" Then
            m_TabPages.Add(tbpData_ItemGenerator_QTM)
            tbcData.TabPages.Remove(tbpData_ItemGenerator_QTM)
            If tbcData.TabPages.Contains(tbpData_ItemGenerator) = False Then
                tbcData.TabPages.Add(tbpData_ItemGenerator)
                m_TabPages.Remove(tbpData_ItemGenerator)
            End If
            rdb_Q8.Checked = True
        ElseIf cmbPerformance_Series.SelectedItem = "RAHU" Then
            m_TabPages.Add(tbpData_ItemGenerator)
            tbcData.TabPages.Remove(tbpData_ItemGenerator)
            If tbcData.TabPages.Contains(tbpData_ItemGenerator_QTM) = False Then
                tbcData.TabPages.Add(tbpData_ItemGenerator_QTM)
                m_TabPages.Remove(tbpData_ItemGenerator_QTM)
            End If
        Else
            m_TabPages.Add(tbpData_ItemGenerator)
            tbcData.TabPages.Remove(tbpData_ItemGenerator)
            m_TabPages.Add(tbpData_ItemGenerator_QTM)
            tbcData.TabPages.Remove(tbpData_ItemGenerator_QTM)
        End If


        If cmbPerformance_Series.SelectedItem = "6" Then

            lblPerformance_WaterProduced.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_MoistureRecovery.ToString()),
         "l/h")
        Else
            lblPerformance_WaterProduced.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_WaterProduced.ToString()),
         "l/h")
        End If


    End Sub

#End Region

#Region "====[ HeatRecoveryModels ]===="

    Public ReadOnly Property SelectedHeatRecoveryModel As CLDCHeatRecoveryModel
        Get
            If Not TypeOf cmbPerformance_HeatRecoveryModels.SelectedItem Is CLComboBoxItemWrapper(Of CLDCHeatRecoveryModel) Then
                Return Nothing
            End If

            Return DirectCast(cmbPerformance_HeatRecoveryModels.SelectedItem, CLComboBoxItemWrapper(Of CLDCHeatRecoveryModel)).Value
        End Get
    End Property

    Public ReadOnly Property SelectedHeatRecoveryModelCustomerName As String
        Get
            If Not TypeOf cmbPerformance_HeatRecoveryModels.SelectedItem Is CLComboBoxItemWrapper(Of CLDCHeatRecoveryModel) Then
                Return ""
            End If

            Return DirectCast(cmbPerformance_HeatRecoveryModels.SelectedItem, CLComboBoxItemWrapper(Of CLDCHeatRecoveryModel)).Text
        End Get
    End Property

    Private Sub HeatRecoveryModels_FillCombo()

        cmbPerformance_HeatRecoveryModels.Items.Clear()

        If SelectedSerieName Is Nothing Then
            Return
        End If

        For Each dcHeatRecoveryModel As CLDCHeatRecoveryModel In Environment.DCContext.CLDCHeatRecoveryModels.OrderBy(Function(heatRecoveryModel) heatRecoveryModel.IdAeraulicConnection).OrderBy(Function(heatRecoveryModel) heatRecoveryModel.Size)

            Dim serieName As String

            serieName = Environment.GetCustomerSerieName(dcHeatRecoveryModel.CLSerie)

            If serieName = SelectedSerieName Then

                Dim dcHeatRecoveryModelName As String

                dcHeatRecoveryModelName = Environment.GetCustomerHeatRecoveryModelName(dcHeatRecoveryModel)

                cmbPerformance_HeatRecoveryModels.Items.Add(New CLComboBoxItemWrapper(Of CLDCHeatRecoveryModel)(
                    dcHeatRecoveryModelName, dcHeatRecoveryModel))
            End If
        Next

        If cmbPerformance_HeatRecoveryModels.Items.Count > 0 Then
            cmbPerformance_HeatRecoveryModels.SelectedItem = cmbPerformance_HeatRecoveryModels.Items(0)
        End If

    End Sub

    Private Sub cmbPerformance_HeatRecoveryModels_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbPerformance_HeatRecoveryModels.SelectedIndexChanged
        If SelectedHeatRecoveryModel Is Nothing Then
            Return
        End If
        tsmiFile_SaveCommercialSheet.Enabled = CommercialSheet_CanGenerate(Environment.PrimaryLanguageCode, Environment.Branch.ShortName)

        Try
            tsmiFile_SaveIOM.Enabled = IOM_CanGenerate(Environment.PrimaryLanguageCode, Environment.Branch.ShortName)

        Catch ex As Exception

        End Try


        If cmbPerformance_Series.SelectedItem = "UKUNDA" Then
            ItemCodeGenerator()
            txbPerformance_ItemCodeQTM.Text = ""
            lblPerformance_ItemDescrQTM.Text = ""
        ElseIf cmbPerformance_Series.SelectedItem = "RAHU" Then
            If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
                ItemCodeGeneratorQTM()
            End If
            txbPerformance_ItemCode.Text = ""
            lblPerformance_ItemDescr.Text = ""
            rdb_qtm_premium.Checked = True
            'If SelectedHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "LT" Then
            '    rdb_qtm_premium.Checked = False
            '    rdb_qtm_preplus.Checked = False
            '    rdb_qtm_premium.Enabled = False
            '    rdb_qtm_preplus.Enabled = False
            '    rdb_qtm_easy.Checked = True
            '    rdb_qtm_easy.Enabled = True
            'Else
            '    rdb_qtm_premium.Enabled = True
            '    rdb_qtm_preplus.Enabled = True
            '    rdb_qtm_premium.Checked = True
            '    rdb_qtm_easy.Checked = False
            '    rdb_qtm_easy.Enabled = False
            'End If
        Else
            txbPerformance_ItemCode.Text = ""
            lblPerformance_ItemDescr.Text = ""
            txbPerformance_ItemCodeQTM.Text = ""
            lblPerformance_ItemDescrQTM.Text = ""
        End If

        If cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("HCI") OrElse cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("FS") Then
            chbSoundPerformances_16032.Enabled = True
            chbSoundPerformances_16032.Visible = True
            chbSoundPerformances_16032.Checked = False
        Else
            chbSoundPerformances_16032.Enabled = False
            chbSoundPerformances_16032.Checked = False
            chbSoundPerformances_16032.Visible = False
        End If

        CoilPerformance_FillStandardCoils()
        ElectricHeater_FillAvailable()
        Accessories_FillAvailable()
        Calculate()
        sap_table_fill()
    End Sub

    Private Sub cmbPerformance_HeatRecoveryModels_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbPerformance_HeatRecoveryModels.Enter, cmbPerformance_Series.Enter
        txbPerformance_AirFlow.Text = maxflow(MeasureUnit, SelectedHeatRecoveryModel, txbPerformance_AirFlow.Text)
        Calculate()
    End Sub

#End Region

#Region "====[ Performances ]===="

    Private Sub hsbPerformance_RegulationLevel_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles hsbPerformance_RegulationLevel.Scroll
        Performance_ApplyRegulationLevel(e.NewValue, True)
    End Sub

    Private Sub Performance_ApplyRegulationLevel(value As Integer, recalculate As Boolean)
        Performance_SynchronizeRegulationLevelControls(hsbPerformance_RegulationLevel,
            lblPerformance_RegulationLevelValue, prbPerformance_RegulationLevel, value)
        If recalculate Then Calculate()
    End Sub

    Private Shared Sub Performance_SynchronizeRegulationLevelControls(scrollBar As HScrollBar,
        valueLabel As Label, progressBar As ProgressBar, value As Integer)
        Dim maximumValue As Integer = Math.Min(progressBar.Maximum,
            scrollBar.Maximum - scrollBar.LargeChange + 1)
        Dim normalizedValue As Integer = Math.Max(scrollBar.Minimum,
            Math.Min(maximumValue, value))

        scrollBar.Value = normalizedValue
        valueLabel.Text = normalizedValue.ToString(CultureInfo.CurrentCulture) & " %"
        progressBar.Value = normalizedValue
    End Sub

    Private Sub txbPerformance_PassiveHaus_ElectricalEfficiency_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_PassiveHaus.TextChanged
        If chbPerformance_PassiveHaus_ShowArea.Checked AndAlso CDbl(txbPerformance_PassiveHaus.Text) <= CDbl(txbPerformance_PassiveHaus_Limit.Text) Then
            txbPerformance_PassiveHaus.BackColor = Color.Green
            txbPerformance_PassiveHaus.ForeColor = Color.White
        Else
            txbPerformance_PassiveHaus.BackColor = SystemColors.Control
            txbPerformance_PassiveHaus.ForeColor = SystemColors.WindowText
        End If
    End Sub

#Region "====[ AirFlow ]===="

    Private txbPerformance_AirFlow_SaveValue As String = ""

    Private Sub txbPerformance_AirFlow_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_AirFlow.Enter, TextBox1.Enter
        'txbPerformance_AirFlow.Text = maxflow(MeasureUnit, CurrentUnit.Name, txbPerformance_AirFlow.Text)
        'Calculate()

        txbPerformance_AirFlow_SaveValue = txbPerformance_AirFlow.Text
        DirectCast(sender, TextBox).Tag = DirectCast(sender, TextBox).Text
    End Sub

    Private Sub txbPerformance_AirFlow_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_AirFlow.Validating, TextBox1.Validating

        Dim value As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        If Not Double.TryParse(textBox.Text, value) Then
            textBox.Text = If(textBox.Tag Is Nothing, txbPerformance_AirFlow_SaveValue, textBox.Tag.ToString())
        Else
            textBox.Text = maxflow(MeasureUnit, SelectedHeatRecoveryModel, textBox.Text)
        End If

    End Sub

    Private Sub txbPerformance_AirFlow_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_AirFlow.Validated, TextBox1.Validated
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        If textBox Is txbPerformance_AirFlow Then
            If TextBox1.Text <> txbPerformance_AirFlow.Text Then
                TextBox1.Text = txbPerformance_AirFlow.Text
            End If
        ElseIf textBox Is TextBox1 Then
            If txbPerformance_AirFlow.Text <> TextBox1.Text Then
                txbPerformance_AirFlow.Text = TextBox1.Text
            End If
        End If

        Calculate()

        Try
            m_DataChanging = True
            checkworkingpoint()
        Finally
            m_DataChanging = False
        End Try



    End Sub

#End Region

#Region "====[ FreshInletTemperature ]===="

    Private txbPerformance_FreshInletTemperature_SaveValue As String = ""

    Private Sub txbPerformance_FreshInletTemperature_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_FreshInletTemperature.Enter, TextBox3.Enter
        txbPerformance_FreshInletTemperature_SaveValue = txbPerformance_FreshInletTemperature.Text
        DirectCast(sender, TextBox).Tag = DirectCast(sender, TextBox).Text
    End Sub

    Private Sub txbPerformance_FreshInletTemperature_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_FreshInletTemperature.Validating, TextBox3.Validating

        Dim value As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        If Not Double.TryParse(textBox.Text, value) Then
            textBox.Text = If(textBox.Tag Is Nothing, txbPerformance_FreshInletTemperature_SaveValue, textBox.Tag.ToString())
        Else
            textBox.Text = FormatNumber(Math.Round(CDbl(textBox.Text), 1), 1)
        End If

    End Sub

    Private Sub txbPerformance_FreshInletTemperature_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_FreshInletTemperature.Validated, TextBox3.Validated
        Dim fit_lim As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        fit_lim = FormatNumber(Math.Round(CDbl(textBox.Text), 1), 1)

        If fit_lim < -20 Then
            fit_lim = -20
        ElseIf fit_lim > 40 Then
            fit_lim = 40
        End If

        textBox.Text = fit_lim
        Calculate()
    End Sub

#End Region

#Region "====[ RHFreshInlet ]===="

    Private txbPerformance_RHFreshInlet_SaveValue As String = ""

    Private Sub txbPerformance_RHFreshInlet_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_RHFreshInlet.Enter, TextBox4.Enter
        txbPerformance_RHFreshInlet_SaveValue = txbPerformance_RHFreshInlet.Text
        DirectCast(sender, TextBox).Tag = DirectCast(sender, TextBox).Text
    End Sub

    Private Sub txbPerformance_RHFreshInlet_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_RHFreshInlet.Validating, TextBox4.Validating

        Dim value As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        If Not Double.TryParse(textBox.Text, value) Then
            textBox.Text = If(textBox.Tag Is Nothing, txbPerformance_RHFreshInlet_SaveValue, textBox.Tag.ToString())
        Else
            textBox.Text = FormatNumber(Math.Round(CDbl(textBox.Text), 0), 0)
        End If

    End Sub

    Private Sub txbPerformance_RHFreshInlet_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_RHFreshInlet.Validated, TextBox4.Validated
        Dim rhfit_lim As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        rhfit_lim = FormatNumber(Math.Round(CDbl(textBox.Text), 0), 0)

        If rhfit_lim < 10 Then
            rhfit_lim = 10
        ElseIf rhfit_lim > 98 Then
            rhfit_lim = 98
        End If

        textBox.Text = rhfit_lim
        Calculate()
    End Sub

#End Region

#Region "====[ ReturnInletTemperature ]===="

    Private txbPerformance_ReturnInletTemperature_SaveValue As String = ""

    Private Sub txbPerformance_ReturnInletTemperature_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_ReturnInletTemperature.Enter, TextBox5.Enter
        txbPerformance_ReturnInletTemperature_SaveValue = txbPerformance_ReturnInletTemperature.Text
        DirectCast(sender, TextBox).Tag = DirectCast(sender, TextBox).Text
    End Sub

    Private Sub txbPerformance_ReturnInletTemperature_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_ReturnInletTemperature.Validating, TextBox5.Validating

        Dim value As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        If Not Double.TryParse(textBox.Text, value) Then
            textBox.Text = If(textBox.Tag Is Nothing, txbPerformance_ReturnInletTemperature_SaveValue, textBox.Tag.ToString())
        Else
            textBox.Text = FormatNumber(Math.Round(CDbl(textBox.Text), 1), 1)
        End If

    End Sub

    Private Sub txbPerformance_ReturnInletTemperature_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_ReturnInletTemperature.Validated, TextBox5.Validated
        Dim rit_lim As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        rit_lim = FormatNumber(Math.Round(CDbl(textBox.Text), 1), 1)

        If rit_lim < 8 Then
            rit_lim = 8
        ElseIf rit_lim > 40 Then
            rit_lim = 40
        End If

        textBox.Text = rit_lim
        Calculate()
    End Sub

#End Region

#Region "====[ RHReturnInlet ]===="

    Private txbPerformance_RHReturnInlet_SaveValue As String = ""

    Private Sub txbPerformance_RHReturnInlet_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_RHReturnInlet.Enter, TextBox6.Enter
        txbPerformance_RHReturnInlet_SaveValue = txbPerformance_RHReturnInlet.Text
        DirectCast(sender, TextBox).Tag = DirectCast(sender, TextBox).Text
    End Sub

    Private Sub txbPerformance_RHReturnInlet_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_RHReturnInlet.Validating, TextBox6.Validating

        Dim value As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        If Not Double.TryParse(textBox.Text, value) Then
            textBox.Text = If(textBox.Tag Is Nothing, txbPerformance_RHReturnInlet_SaveValue, textBox.Tag.ToString())
        Else
            textBox.Text = FormatNumber(Math.Round(CDbl(textBox.Text), 0), 0)
        End If

    End Sub

    Private Sub txbPerformance_RHReturnInlet_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_RHReturnInlet.Validated, TextBox6.Validated

        Dim rhrit_lim As Double
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        rhrit_lim = FormatNumber(Math.Round(CDbl(textBox.Text), 0), 0)

        If rhrit_lim < 10 Then
            rhrit_lim = 10
        ElseIf rhrit_lim > 98 Then
            rhrit_lim = 98
        End If

        textBox.Text = rhrit_lim
        Calculate()
    End Sub

#End Region

#Region "====[ Chart ]===="

    Private Function Chart_Clone(ByVal chart As DataVisualization.Charting.Chart) As DataVisualization.Charting.Chart

        Dim memoryStream As New MemoryStream
        Dim newChart As New DataVisualization.Charting.Chart
        Dim newChartSerializer As DataVisualization.Charting.ChartSerializer
        Dim chartSerializer As DataVisualization.Charting.ChartSerializer

        chartSerializer = chart.Serializer()
        chartSerializer.Save(memoryStream)

        newChartSerializer = newChart.Serializer()
        newChartSerializer.Load(memoryStream)

        Return newChart
    End Function

    Private Function Chart_GetImage(ByVal chart As DataVisualization.Charting.Chart,
     ByVal scale As Double) As Bitmap

        Dim image As Bitmap
        Dim memoryStream As New MemoryStream

        ' Report charts already use explicit 4x supersampling. Keep their
        ' rendering independent from the higher-density on-screen controls.
        chart.RenderingDpiX = 96
        chart.RenderingDpiY = 96
        chart.Scale(New SizeF(scale, scale))

        chart.BorderlineWidth *= scale
        For Each series As DataVisualization.Charting.Series In chart.Series
            series.BorderWidth *= scale
            series.Font = New System.Drawing.Font(series.Font.FontFamily.Name, CSng(series.Font.Size * scale), series.Font.Style)
            series.MarkerSize *= scale
            series.MarkerBorderWidth *= scale
        Next

        For Each legend As DataVisualization.Charting.Legend In chart.Legends
            legend.BorderWidth *= scale
            legend.Font = New System.Drawing.Font(legend.Font.FontFamily.Name, CSng(legend.Font.Size * scale), legend.Font.Style)
            legend.TitleFont = New System.Drawing.Font(legend.TitleFont.FontFamily.Name, CSng(legend.TitleFont.Size * scale), legend.TitleFont.Style)
        Next

        For Each title As DataVisualization.Charting.Title In chart.Titles
            title.Font = New System.Drawing.Font(title.Font.FontFamily.Name, CSng(title.Font.Size * scale), title.Font.Style)
        Next

        For Each chartArea As DataVisualization.Charting.ChartArea In chart.ChartAreas
            chartArea.AxisX.LineWidth *= scale
            chartArea.AxisX.MajorGrid.LineWidth *= scale
            chartArea.AxisX.MajorTickMark.LineWidth *= scale
            chartArea.AxisX.TitleFont = New System.Drawing.Font(chartArea.AxisX.TitleFont.FontFamily.Name, CSng(chartArea.AxisX.TitleFont.Size * scale), chartArea.AxisX.TitleFont.Style)
            chartArea.AxisX.LabelStyle.Font = New System.Drawing.Font(chartArea.AxisX.LabelStyle.Font.FontFamily.Name, CSng(chartArea.AxisX.LabelStyle.Font.Size * scale), chartArea.AxisX.LabelStyle.Font.Style)
            chartArea.AxisY.LineWidth *= scale
            chartArea.AxisY.MajorGrid.LineWidth *= scale
            chartArea.AxisY.MajorTickMark.LineWidth *= scale
            chartArea.AxisY.TitleFont = New System.Drawing.Font(chartArea.AxisY.TitleFont.FontFamily.Name, CSng(chartArea.AxisY.TitleFont.Size * scale), chartArea.AxisY.TitleFont.Style)
            chartArea.AxisY.LabelStyle.Font = New System.Drawing.Font(chartArea.AxisY.LabelStyle.Font.FontFamily.Name, CSng(chartArea.AxisY.LabelStyle.Font.Size * scale), chartArea.AxisY.LabelStyle.Font.Style)
        Next

        chart.SaveImage(memoryStream, ImageFormat.Png)
        image = New Bitmap(memoryStream)

        Return image
    End Function

    Private Sub Chart_ApplySummerEfficiencyStyle(chart As DataVisualization.Charting.Chart)
        If chart Is Nothing OrElse chart.Series Is Nothing Then
            Return
        End If

        Chart_ApplyModernTheme(chart)

        Dim curveSeries As Series = chart.Series.FindByName(ChartSeries_SummerEfficiencyCurve_Name)
        If curveSeries IsNot Nothing Then
            curveSeries.Color = Color.SeaGreen
            curveSeries.BorderColor = Color.SeaGreen
            curveSeries.BorderDashStyle = ChartDashStyle.Solid
            curveSeries.BorderWidth = 2
            curveSeries.LegendText = Chart_SummerEfficiencyLegendText()
            curveSeries.IsVisibleInLegend = True
        End If

        Dim pointSeries As Series = chart.Series.FindByName(ChartSeries_SummerEfficiencyPoint_Name)
        If pointSeries IsNot Nothing Then
            pointSeries.Color = Color.SeaGreen
            pointSeries.MarkerColor = Color.SeaGreen
            pointSeries.MarkerBorderColor = Color.SeaGreen
            pointSeries.MarkerStyle = MarkerStyle.Circle
            pointSeries.MarkerSize = 8
            pointSeries.MarkerBorderColor = Color.White
            pointSeries.MarkerBorderWidth = 2
            pointSeries.LegendText = Chart_SummerWorkingPointLegendText()
            pointSeries.IsVisibleInLegend = True
        End If
    End Sub

    Private Sub Chart_RemoveEfficiencyScenarioTitles(chart As Chart)
        For Each titleName As String In New String() {ChartTitle_WinterEfficiency_Name, ChartTitle_SummerEfficiency_Name}
            Dim title As Title = chart.Titles.FindByName(titleName)
            If title IsNot Nothing Then chart.Titles.Remove(title)
        Next
    End Sub

    Private Sub Chart_AddEfficiencyScenarioTitle(chart As Chart, area As ChartArea, titleName As String, text As String)
        Dim title As New Title With {
            .Name = titleName,
            .Text = text,
            .Docking = Docking.Top,
            .DockedToChartArea = area.Name,
            .IsDockedInsideChartArea = True,
            .Alignment = ContentAlignment.TopRight,
            .Font = New System.Drawing.Font("Segoe UI", 8.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(55, 65, 81)
        }
        chart.Titles.Add(title)
    End Sub

    Private Sub Chart_ConfigureSingleEfficiencyArea(chart As Chart)
        If chart Is Nothing OrElse chart.ChartAreas.Count = 0 Then Return

        For index As Integer = chart.ChartAreas.Count - 1 To 1 Step -1
            chart.ChartAreas.RemoveAt(index)
        Next

        Chart_RemoveEfficiencyScenarioTitles(chart)
        Dim area As ChartArea = chart.ChartAreas(0)
        area.Position.Auto = True
        area.InnerPlotPosition.Auto = True
        area.AxisX.LabelStyle.Enabled = True
        area.AxisX.MajorTickMark.Enabled = True
        area.AxisY.Minimum = 60
        area.AxisY.Maximum = 100
        area.AxisY.Interval = 10
    End Sub

    Private Function Chart_ConfigureSplitEfficiencyAreas(chart As Chart) As ChartArea
        Dim winterArea As ChartArea = chart.ChartAreas(0)
        Dim summerArea As ChartArea = chart.ChartAreas.FindByName(ChartArea_SummerEfficiency_Name)
        If summerArea Is Nothing Then
            summerArea = New ChartArea(ChartArea_SummerEfficiency_Name)
            chart.ChartAreas.Add(summerArea)
        End If

        Chart_RemoveEfficiencyScenarioTitles(chart)

        Dim airflowTitle As String = winterArea.AxisX.Title
        Dim efficiencyTitle As String = winterArea.AxisY.Title
        Dim axisMaximum As Double = winterArea.AxisX.Maximum
        Dim axisInterval As Double = winterArea.AxisX.Interval

        winterArea.Position.Auto = False
        winterArea.Position = New ElementPosition(0, 0, 100, 49)
        winterArea.InnerPlotPosition.Auto = False
        winterArea.InnerPlotPosition = New ElementPosition(15, 13, 82, 75)
        winterArea.AxisX.LabelStyle.Enabled = False
        winterArea.AxisX.MajorTickMark.Enabled = False
        winterArea.AxisX.Title = ""
        winterArea.AxisY.Minimum = 60
        winterArea.AxisY.Maximum = 100
        winterArea.AxisY.Interval = 10

        summerArea.Position.Auto = False
        summerArea.Position = New ElementPosition(0, 51, 100, 49)
        summerArea.InnerPlotPosition.Auto = False
        summerArea.InnerPlotPosition = New ElementPosition(15, 8, 82, 68)
        summerArea.AxisX.Minimum = 0
        summerArea.AxisX.Maximum = axisMaximum
        summerArea.AxisX.Interval = axisInterval
        summerArea.AxisX.Title = airflowTitle
        summerArea.AxisY.Minimum = 60
        summerArea.AxisY.Maximum = 100
        summerArea.AxisY.Interval = 10
        summerArea.AxisY.Title = efficiencyTitle
        summerArea.AlignWithChartArea = winterArea.Name
        summerArea.AlignmentOrientation = AreaAlignmentOrientations.Vertical
        summerArea.AlignmentStyle = AreaAlignmentStyles.AxesView

        Chart_AddEfficiencyScenarioTitle(
            chart,
            winterArea,
            ChartTitle_WinterEfficiency_Name,
            Environment.Localization.GetString(CLMessageResources.MainForm_Winter.ToString()))
        Chart_AddEfficiencyScenarioTitle(
            chart,
            summerArea,
            ChartTitle_SummerEfficiency_Name,
            Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString()))

        Return summerArea
    End Function

    Private Async Sub tsmiOption_CheckUpdates_Click(ByVal sender As Object, ByVal e As EventArgs) Handles tsmiOption_CheckUpdates.Click
        tsmiOption_CheckUpdates.Enabled = False
        Try
            Await UpdateManager.CheckForSoftwareUpdate(True)
        Finally
            tsmiOption_CheckUpdates.Enabled = True
        End Try
    End Sub

    Private Sub Chart_AddSummerEfficiencyLegendItems(chart As DataVisualization.Charting.Chart)
        If chart Is Nothing OrElse chart.Series Is Nothing OrElse chart.ChartAreas.Count = 0 Then
            Return
        End If

        Dim curveSeries As Series = chart.Series.FindByName(ChartSeries_SummerEfficiencyCurve_Name)
        If curveSeries Is Nothing Then
            curveSeries = chart.Series.Add(ChartSeries_SummerEfficiencyCurve_Name)
            curveSeries.ChartType = SeriesChartType.Line
            curveSeries.Points.AddXY(0, 0)
        End If

        Dim pointSeries As Series = chart.Series.FindByName(ChartSeries_SummerEfficiencyPoint_Name)
        If pointSeries Is Nothing Then
            pointSeries = chart.Series.Add(ChartSeries_SummerEfficiencyPoint_Name)
            pointSeries.ChartType = SeriesChartType.Point
            pointSeries.Points.AddXY(0, 0)
        End If

        Chart_ApplySummerEfficiencyStyle(chart)
    End Sub

    Private Function Chart_SummerEfficiencyLegendText() As String
        Return String.Format("{0} {1}",
            Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString()),
            Environment.Localization.GetString(CLMessageResources.MainForm_Efficiency.ToString()))
    End Function

    Private Function Chart_SummerWorkingPointLegendText() As String
        Return String.Format("{0} {1}",
            Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString()),
            Environment.Localization.GetString(CLMessageResources.PDF_WorkingPoint.ToString()))
    End Function

    Private Function Chart_WinterEfficiencyLegendText() As String
        Return String.Format("{0} {1}",
            Environment.Localization.GetString(CLMessageResources.MainForm_Winter.ToString()),
            Environment.Localization.GetString(CLMessageResources.MainForm_Efficiency.ToString()))
    End Function

    Private Sub Chart_ConfigureReportEfficiency(chart As Chart)
        If chart Is Nothing OrElse chart.ChartAreas.Count = 0 Then Return

        Chart_ApplySummerEfficiencyStyle(chart)
        Chart_RemoveEfficiencyScenarioTitles(chart)

        Dim efficiencyTitle As String = chart.ChartAreas(0).AxisY.Title
        For Each area As ChartArea In chart.ChartAreas
            area.AxisY.Title = String.Empty
        Next

        Dim existingTitle As Title = chart.Titles.FindByName(ChartTitle_ReportEfficiencyAxis_Name)
        If existingTitle IsNot Nothing Then chart.Titles.Remove(existingTitle)

        chart.Titles.Add(New Title With {
            .Name = ChartTitle_ReportEfficiencyAxis_Name,
            .Text = efficiencyTitle,
            .Docking = Docking.Left,
            .DockingOffset = 4,
            .Alignment = ContentAlignment.MiddleCenter,
            .TextOrientation = TextOrientation.Rotated270,
            .Font = New System.Drawing.Font("Segoe UI", 9.0F, FontStyle.Regular),
            .ForeColor = Color.FromArgb(31, 41, 55)
        })
    End Sub

    Private Sub Chart_AlignReportPlotHorizontally(chart As Chart,
                                                    Optional plotX As Single = 18.0F,
                                                    Optional plotWidth As Single = 80.0F)
        If chart Is Nothing OrElse chart.ChartAreas.Count = 0 Then Return

        ' Resolve automatic vertical layout before fixing identical horizontal
        ' plot bounds for the pressure and efficiency report images.
        Using layoutStream As New MemoryStream()
            chart.SaveImage(layoutStream, ChartImageFormat.Png)
        End Using

        For Each area As ChartArea In chart.ChartAreas
            Dim plotPosition As ElementPosition = area.InnerPlotPosition
            area.InnerPlotPosition = New ElementPosition(plotX, plotPosition.Y, plotWidth, plotPosition.Height)
        Next
    End Sub

    Private Sub Chart_AddReportWinterEfficiencyLegendItem(chart As Chart)
        If chart Is Nothing Then Return

        Dim series As Series = chart.Series.FindByName(ChartSeries_ReportWinterEfficiency_Name)
        If series Is Nothing Then
            series = chart.Series.Add(ChartSeries_ReportWinterEfficiency_Name)
            series.ChartType = SeriesChartType.Line
            series.Points.AddXY(0, 0)
        End If

        series.Color = ChartSeries_OriginalCurve_Color
        series.BorderColor = ChartSeries_OriginalCurve_Color
        series.BorderDashStyle = ChartDashStyle.Solid
        series.BorderWidth = 2
        series.LegendText = Chart_WinterEfficiencyLegendText()
        series.IsVisibleInLegend = True
    End Sub

#End Region

#Region "====[ Passive Haus / SEL / SFP / ERP2018]===="

    Dim m_ChangingShowArea As Boolean = False
    Dim m_FreshInletTemperature As String
    Dim m_RHFreshInlet As String
    Dim m_ReturnInletTemperature As String
    Dim m_RHReturnInlet As String


#Region "====[ SFP ]===="

    Private Sub chbPerformance_SFP_ShowArea_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chbPerformance_SFP_ShowArea.CheckedChanged

        nudPerformance_SFP_Limit.Enabled = chbPerformance_SFP_ShowArea.Checked

        If m_ChangingShowArea Then
            Return
        End If

        Try
            m_ChangingShowArea = True
            chbPerformance_PassiveHaus_ShowArea.Checked = False
            chbPerformance_SEL_ShowArea.Checked = False
            chbPerformance_ERP2018_ShowArea.Checked = False
        Finally
            m_ChangingShowArea = False
        End Try

        Calculate()

    End Sub

    Private Sub nudSFPLimit_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles nudPerformance_SFP_Limit.ValueChanged

        nudPerformance_SEL_Limit.Value = nudPerformance_SFP_Limit.Value * 1000

        Calculate()
    End Sub

    Private Sub txbPerformance_SFP_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_SFP.TextChanged

        If chbPerformance_SFP_ShowArea.Checked AndAlso CDbl(txbPerformance_SFP.Text) <= nudPerformance_SFP_Limit.Value Then
            txbPerformance_SFP.BackColor = Color.Green
            txbPerformance_SFP.ForeColor = Color.White
        Else
            txbPerformance_SFP.BackColor = SystemColors.Control
            txbPerformance_SFP.ForeColor = SystemColors.WindowText
        End If
    End Sub

#End Region

#Region "====[ Passive Haus ]===="

    Private Sub chbPerformance_PassiveHaus_ShowArea_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chbPerformance_PassiveHaus_ShowArea.CheckedChanged

        If m_ChangingShowArea Then
            Return
        End If

        Try
            m_ChangingShowArea = True
            chbPerformance_SFP_ShowArea.Checked = False
            chbPerformance_SEL_ShowArea.Checked = False
            chbPerformance_ERP2018_ShowArea.Checked = False
        Finally
            m_ChangingShowArea = False
        End Try

        Calculate()

    End Sub

#End Region

#Region "====[ SEL ]===="

    Private Sub chbPerformance_SEL_ShowArea_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chbPerformance_SEL_ShowArea.CheckedChanged

        nudPerformance_SEL_Limit.Enabled = chbPerformance_SEL_ShowArea.Checked

        If m_ChangingShowArea Then
            Return
        End If

        Try
            m_ChangingShowArea = True
            chbPerformance_PassiveHaus_ShowArea.Checked = False
            chbPerformance_SFP_ShowArea.Checked = False
            chbPerformance_ERP2018_ShowArea.Checked = False
        Finally
            m_ChangingShowArea = False
        End Try

        Calculate()

    End Sub

    Private Sub nudPerformance_SEL_Limit_ValueChanged(sender As System.Object, e As System.EventArgs) Handles nudPerformance_SEL_Limit.ValueChanged
        Calculate()

        nudPerformance_SFP_Limit.Value = nudPerformance_SEL_Limit.Value / 1000

    End Sub

#End Region

#Region "====[ ERP 2018 ]===="
    Private Sub chbPerformance_ERP2018_ShowArea_CheckedChanged(sender As Object, e As EventArgs) Handles chbPerformance_ERP2018_ShowArea.CheckedChanged

        If m_ChangingShowArea Then
            Return
        End If

        Try
            m_ChangingShowArea = True
            chbPerformance_SFP_ShowArea.Checked = False
            chbPerformance_SEL_ShowArea.Checked = False
            chbPerformance_PassiveHaus_ShowArea.Checked = False
            If chbPerformance_ERP2018_ShowArea.Checked Then

                ''Backup temperature/RH precedenti
                'm_FreshInletTemperature = txbPerformance_FreshInletTemperature.Text
                'm_RHFreshInlet = txbPerformance_RHFreshInlet.Text
                'm_ReturnInletTemperature = txbPerformance_ReturnInletTemperature.Text
                'm_RHReturnInlet = txbPerformance_RHReturnInlet.Text

                ''Forzo temperature/RH ERP 2018
                'txbPerformance_FreshInletTemperature.Text = 5
                'txbPerformance_RHFreshInlet.Text = 10
                'txbPerformance_ReturnInletTemperature.Text = 25
                'txbPerformance_RHReturnInlet.Text = 10

                ''Blocco Modifiche ai campi
                'txbPerformance_FreshInletTemperature.ReadOnly = True
                'txbPerformance_RHFreshInlet.ReadOnly = True
                'txbPerformance_ReturnInletTemperature.ReadOnly = True
                'txbPerformance_RHReturnInlet.ReadOnly = True
            Else
                ''Sblocco Modifiche ai campi
                'txbPerformance_FreshInletTemperature.ReadOnly = False
                'txbPerformance_RHFreshInlet.ReadOnly = False
                'txbPerformance_ReturnInletTemperature.ReadOnly = False
                'txbPerformance_RHReturnInlet.ReadOnly = False

                ''Ripristino temperature/RH precedenti
                'txbPerformance_FreshInletTemperature.Text = m_FreshInletTemperature
                'txbPerformance_RHFreshInlet.Text = m_RHFreshInlet
                'txbPerformance_ReturnInletTemperature.Text = m_ReturnInletTemperature
                'txbPerformance_RHReturnInlet.Text = m_RHReturnInlet

            End If

        Finally
            m_ChangingShowArea = False
        End Try

        Calculate()

    End Sub



#End Region

#End Region

#End Region

#Region "====[ Bitmap ]===="

    Private Function Bitmap_GetBytes(bitmap As Bitmap) As Byte()

        If bitmap Is Nothing Then
            Return Nothing
        End If
        Dim memoryStream As New MemoryStream()
        Dim bytes As New List(Of Byte)

        bitmap.Save(memoryStream, ImageFormat.Png)
        memoryStream.Seek(0, SeekOrigin.Begin)

        While True
            Dim byteRead As Integer = memoryStream.ReadByte()
            If byteRead = -1 Then
                Exit While
            End If
            bytes.Add(byteRead)

        End While

        Return bytes.ToArray()
    End Function

#End Region

#Region "====[ Generazione Report ]===="

    Private Async Sub SaveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiFile_GenerateReport.Click
        If m_ReportRegistrationBusy Then Return
        m_ReportRegistrationBusy = True
        tsmiFile_GenerateReport.Enabled = False
        Try
            If Await Project_RegisterBeforeReportAsync() Then Report_Generate()
        Finally
            m_ReportRegistrationBusy = False
            tsmiFile_GenerateReport.Enabled = True
        End Try
        'sfdSaveFile.Filter = "PDF file|*.pdf"
        'sfdSaveFile.FilterIndex = 1
        'sfdSaveFile.RestoreDirectory = True
        'sfdSaveFile.Title = Environment.Localization.GetString(CLMessageResources.CLMainForm_SelectPdfFile.ToString())

        'If sfdSaveFile.ShowDialog() = Windows.Forms.DialogResult.OK Then
        '	'Aggiungo l'estensione se manca
        '	Application.DoEvents()
        '	If sfdSaveFile.FileName.Contains(".pdf") = False Then
        '		sfdSaveFile.FileName = sfdSaveFile.FileName + ".pdf"
        '	End If
        '	Application.DoEvents()
        '	salva_pdf(sfdSaveFile.FileName)
        'End If

    End Sub

    Private Sub Report_Generate()

        Dim waitForm As CLPleaseWaitForm
        Dim pressureImage As System.Drawing.Bitmap
        Dim powerImage As System.Drawing.Bitmap
        Dim airflowImage As System.Drawing.Bitmap
        Dim legendImage As System.Drawing.Bitmap
        Dim CO2Image As System.Drawing.Bitmap
        Dim chartScale As Double = 4
        Dim cloneChart As DataVisualization.Charting.Chart
        Dim chartSeries As Series
        Dim chartOriginalSize As New Size(360, 200)

        ' Form attesa
        waitForm = New CLPleaseWaitForm()
        waitForm.lblMessage.Text = Environment.Localization.GetString(CLMessageResources.PleaseWaitForm_Message.ToString())
        'waitForm.StartPosition = FormStartPosition.CenterParent
        waitForm.Location = New Point(Left + (Width - waitForm.Width) / 2, Top + (Height - waitForm.Height) / 2)
        waitForm.Show()
        waitForm.Refresh()

        ' Build image Pressure
        cloneChart = Chart_Clone(crtPerformance_Chart1)
        Chart_ApplyModernTheme(cloneChart)
        cloneChart.Size = chartOriginalSize
        Chart_AlignReportPlotHorizontally(cloneChart)
        pressureImage = Chart_GetImage(cloneChart, chartScale)

        ' Build image Power
        cloneChart = Chart_Clone(crtPerformance_Chart2)
        Chart_ApplyModernTheme(cloneChart)
        cloneChart.Size = chartOriginalSize
        powerImage = Chart_GetImage(cloneChart, chartScale)

        ' Build image AirFlow
        cloneChart = Chart_Clone(crtPerformance_Chart3)
        Chart_ConfigureReportEfficiency(cloneChart)
        cloneChart.Size = chartOriginalSize
        Chart_AlignReportPlotHorizontally(cloneChart, 20.5F, 75.0F)
        airflowImage = Chart_GetImage(cloneChart, chartScale)

        ' Build image CO2
        cloneChart = Chart_Clone(crtCO2Level_Chart1)
        cloneChart.Size = chartOriginalSize
        CO2Image = Chart_GetImage(cloneChart, chartScale)

        ' Legends
        cloneChart = Chart_Clone(crtPerformance_Chart1)
        Chart_ApplyModernTheme(cloneChart)
        cloneChart.Size = chartOriginalSize
        cloneChart.Height /= 2
        cloneChart.Legends.Add("Legends")
        cloneChart.Legends(0).Position.Auto = False
        cloneChart.Legends(0).Position.X = 0
        cloneChart.Legends(0).Position.Y = 0
        cloneChart.Legends(0).Position.Width = 100
        cloneChart.Legends(0).Position.Height = 100

        cloneChart.Legends(0).LegendStyle = LegendStyle.Column
        cloneChart.Legends(0).TableStyle = LegendStyle.Column
        cloneChart.Legends(0).Alignment = StringAlignment.Near
        cloneChart.Legends(0).TitleAlignment = StringAlignment.Center

        chartSeries = cloneChart.Series.FindByName(ChartSeries_OriginalCurve_Name)
        If Not chartSeries Is Nothing Then
            chartSeries.LegendText = String.Format("100% {0}",
             Environment.Localization.GetString(CLMessageResources.MainForm_RegulationLevel.ToString()))
            chartSeries.IsVisibleInLegend = True
        End If

        chartSeries = cloneChart.Series.FindByName(ChartSeries_RegulationLevelCurve_Name)
        If Not chartSeries Is Nothing Then
            chartSeries.LegendText = String.Format("{0}% {1}", hsbPerformance_RegulationLevel.Value,
              Environment.Localization.GetString(CLMessageResources.MainForm_RegulationLevel.ToString()))
            chartSeries.IsVisibleInLegend = True
        End If

        chartSeries = cloneChart.Series.FindByName(ChartSeries_WorkingPoint_Name)
        If Not chartSeries Is Nothing Then
            chartSeries.IsVisibleInLegend = True
            chartSeries.LegendText = Environment.Localization.GetString(CLMessageResources.PDF_WorkingPoint.ToString())
        End If

        chartSeries = cloneChart.Series.FindByName(ChartSeries_WorkingArea_Name)
        If Not chartSeries Is Nothing Then
            chartSeries.IsVisibleInLegend = True
            If chbPerformance_ERP2018_ShowArea.Checked = True Then
                chartSeries.LegendText = Environment.Localization.GetString(CLMessageResources.PDF_WorkingArea.ToString()) + " - ERP 2018"
            ElseIf chbPerformance_SEL_ShowArea.Checked = True Then
                chartSeries.LegendText = Environment.Localization.GetString(CLMessageResources.PDF_WorkingArea.ToString()) + " - SEL"
            ElseIf chbPerformance_SFP_ShowArea.Checked = True Then
                chartSeries.LegendText = Environment.Localization.GetString(CLMessageResources.PDF_WorkingArea.ToString()) + " - SFP"
            ElseIf chbPerformance_PassiveHaus_ShowArea.Checked = True Then
                chartSeries.LegendText = Environment.Localization.GetString(CLMessageResources.PDF_WorkingArea.ToString()) + " - Passive Haus"
            Else
                chartSeries.LegendText = Environment.Localization.GetString(CLMessageResources.PDF_WorkingArea.ToString())
            End If
        End If

        Chart_AddReportWinterEfficiencyLegendItem(cloneChart)
        If m_SummerCalculationEnabled AndAlso m_HasLastSummerThermo Then
            Chart_AddSummerEfficiencyLegendItems(cloneChart)
        End If



        legendImage = Chart_GetImage(cloneChart, chartScale)

        Dim reportDataSet As New CLMainReportDataSet
        Dim technicalSelectionHeaderColumns As String() = {
            "TechnicalSelectionReference_Caption",
            "TechnicalSelectionReference_Value",
            "TechnicalSelectionRevision_Caption",
            "TechnicalSelectionRevision_Value",
            "TechnicalSelectionStatus_Caption",
            "TechnicalSelectionStatus_Value"
        }
        For Each columnName As String In technicalSelectionHeaderColumns
            If Not reportDataSet.HeaderDataTable.Columns.Contains(columnName) Then
                reportDataSet.HeaderDataTable.Columns.Add(columnName, GetType(String))
            End If
        Next
        Dim headerDataRow As CLMainReportDataSet.HeaderDataTableRow
        Dim performanceAccordanceDataRow As CLMainReportDataSet.PerformanceAccordanceDataTableRow
        Dim workingPointDataRow As CLMainReportDataSet.WorkingPointDataTableRow
        Dim temperatureConditionsAndHumidityDataRow As CLMainReportDataSet.TemperatureConditionsAndHumidityDataTableRow
        Dim heatExchangerPerformancesDataRow As CLMainReportDataSet.HeatExchangerPerformancesDataTableRow
        Dim diagramDataRow As CLMainReportDataSet.DiagramDataTableRow
        Dim CO2LevelDataRow As CLMainReportDataSet.CO2LevelRoomRow
        Dim CO2LevelUseDataRow As CLMainReportDataSet.CO2LevelUseRow
        Dim CO2LevelParametersDataRow As CLMainReportDataSet.CO2LevelParametersRow
        Dim waterCoilReportDataTable As DataTable = Report_CreateWaterCoilReportTable()
        Dim electricHeaterReportDataTable As DataTable = Report_CreateElectricHeaterReportTable()
        Dim accessoryReportDataTable As DataTable = Report_CreateAccessoryReportTable()

        'CO2 LevelParameters
        '---------------------------------------------------
        CO2LevelParametersDataRow = reportDataSet.CO2LevelParameters.NewCO2LevelParametersRow()

        CO2LevelParametersDataRow.addtorep_bool = Not chbCO2Level_addtoreport.Checked  ' La proprietà è Hidden quindi la logica è negativa

        If cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2 Then
            CO2LevelParametersDataRow.AF_area = txbCO2Level_Parameters_af_area.Text
            CO2LevelParametersDataRow.AF_person = txbCO2Level_Parameters_af_person.Text
            CO2LevelParametersDataRow.AF_area_m3h = txbCO2Level_Parameters_af_area_m3h.Text
            CO2LevelParametersDataRow.AF_person_m3h = txbCO2Level_Parameters_af_person_m3h.Text

        Else
            CO2LevelParametersDataRow.AF_area = "-"
            CO2LevelParametersDataRow.AF_person = "-"
            CO2LevelParametersDataRow.AF_area_m3h = "-"
            CO2LevelParametersDataRow.AF_person_m3h = "-"
        End If

        CO2LevelParametersDataRow.OutCO2 = CDbl(txbCO2Level_Parameters_extCO2.Text)
        CO2LevelParametersDataRow.MaxCO2 = CDbl(txbCO2Level_Parameters_maxCO2.Text)
        CO2LevelParametersDataRow.AF_demand = CDbl(txbCO2Level_Parameters_airflow.Text)
        CO2LevelParametersDataRow.AF_demand_m3h = CDbl(txbCO2Level_Parameters_airflow_m3h.Text)
        CO2LevelParametersDataRow.StdPreset = cmbCO2Level_Parameters_stdpreset.SelectedItem
        CO2LevelParametersDataRow.CalcMeth = cmbCO2Level_Parameters_CalcMet.SelectedItem
        CO2LevelParametersDataRow.StdPreset_Caption = lblCO2Level_Parameters_stdpreset.Text
        CO2LevelParametersDataRow.CalcMeth_Caption = lblCO2Level_Parameters_CalcMet.Text
        CO2LevelParametersDataRow.OutCO2_caption = lblCO2Level_Parameters_extCO2.Text
        CO2LevelParametersDataRow.MaxCO2_caption = lblCO2Level_Parameters_maxCO2.Text
        CO2LevelParametersDataRow.AF_demand_caption = lblCO2Level_Parameters_airflow.Text
        CO2LevelParametersDataRow.AF_person_caption = lblCO2Level_Parameters_af_person.Text
        CO2LevelParametersDataRow.AF_area_caption = lblCO2Level_Parameters_af_area.Text
        CO2LevelParametersDataRow.AF_area_m3h_caption = lblCO2Level_Parameters_af_area_m3h.Text
        CO2LevelParametersDataRow.AF_person_m3h_caption = lblCO2Level_Parameters_af_person_m3h.Text
        CO2LevelParametersDataRow.AF_demand_m3h_caption = lblCO2Level_Parameters_airflow_m3h.Text
        CO2LevelParametersDataRow.Title = grbCO2Level_Parameters.Text


        reportDataSet.CO2LevelParameters.Rows.Add(CO2LevelParametersDataRow)

        'CO2 LevelUse
        '---------------------------------------------------
        CO2LevelUseDataRow = reportDataSet.CO2LevelUse.NewCO2LevelUseRow()

        CO2LevelUseDataRow.Title = grbCO2Level_use.Text
        CO2LevelUseDataRow.LevelAct = CDbl(txbCO2Level_Usage_Activity.Text)
        CO2LevelUseDataRow.CO2prod = CDbl(txbCO2Level_Usage_CO2prod.Text)
        CO2LevelUseDataRow.PeoplePresence = CDbl(txbCO2Level_Usage_PeoplePresence.Text)
        CO2LevelUseDataRow.PeopleBreak = CDbl(txbCO2Level_Usage_PeopleBreak.Text)
        CO2LevelUseDataRow.PeriodPresence = CDbl(txbCO2Level_Usage_PeriodPresence.Text)
        CO2LevelUseDataRow.PeriodBreak = CDbl(txbCO2Level_Usage_PeriodBreak.Text)
        CO2LevelUseDataRow.LevelAct_Caption = lblCO2Level_Usage_Activity.Text
        CO2LevelUseDataRow.CO2prod_Caption = lblCO2Level_Usage_CO2prod.Text
        CO2LevelUseDataRow.People_Caption = lblCO2Level_Usage_People.Text
        CO2LevelUseDataRow.Period_Caption = lblCO2Level_Usage_Period.Text
        CO2LevelUseDataRow.Break_Caption = lblCO2Level_Usage_Break.Text
        CO2LevelUseDataRow.Presence_Caption = lblCO2Level_Usage_Presence.Text

        reportDataSet.CO2LevelUse.Rows.Add(CO2LevelUseDataRow)

        'CO2 LevelRoom
        '---------------------------------------------------
        CO2LevelDataRow = reportDataSet.CO2LevelRoom.NewCO2LevelRoomRow()

        CO2LevelDataRow.RoomHeight = CDbl(txbCO2Level_Room_Height.Text)
        CO2LevelDataRow.RoomWidth = CDbl(txbCO2Level_Room_Width.Text)
        CO2LevelDataRow.RoomLength = CDbl(txbCO2Level_Room_Length.Text)
        CO2LevelDataRow.RoomHeight_Caption = lblCO2Level_Room_Height.Text
        CO2LevelDataRow.RoomWidth_Caption = lblCO2Level_Room_Width.Text
        CO2LevelDataRow.RoomLength_Caption = lblCO2Level_Room_Length.Text
        CO2LevelDataRow.Title = grbCO2Level_Room.Text

        reportDataSet.CO2LevelRoom.Rows.Add(CO2LevelDataRow)


        ' Header
        '---------------------------------------------------
        headerDataRow = reportDataSet.HeaderDataTable.NewHeaderDataTableRow()
        headerDataRow.UnitSelected_Caption = Environment.Localization.GetString(CLMessageResources.MainForm_UnitSelected.ToString())

        If cmbPerformance_Series.SelectedItem = "UKUNDA" Then
            headerDataRow.UnitSelected_Value = lblPerformance_ItemDescr.Text
        ElseIf cmbPerformance_Series.SelectedItem = "RAHU" Then
            headerDataRow.UnitSelected_Value = lblPerformance_ItemDescrQTM.Text
        Else
            headerDataRow.UnitSelected_Value = cmbPerformance_HeatRecoveryModels.Text
        End If


        headerDataRow.Date_Caption = Environment.Localization.GetString(CLMessageResources.MainForm_Date.ToString())
        headerDataRow.Date_Value = Today.Date

        headerDataRow.SoftwareRelease_Caption = Environment.Localization.GetString(CLMessageResources.Release.ToString())
        headerDataRow.SoftwareRelease_Value = String.Format("{0} - {1}", Environment.SSWInfo.ReleaseVersion.ToString(),
         Environment.SSWInfo.ReleaseDate.ToString("MM/yyyy"))

        headerDataRow.Page_Caption = Environment.Localization.GetString(CLMessageResources.Page.ToString())

        headerDataRow.LogoBmp = Bitmap_GetBytes(Environment.CustomerLogo)

        headerDataRow.CustomerInfo = Environment.SSWInfo.CustomerInfo

        Dim hasRegisteredReference As Boolean = Not m_ReportGeneratedAsDraft AndAlso
            m_ProjectDocument IsNot Nothing AndAlso
            m_ProjectDocument.Identity IsNot Nothing AndAlso
            Not String.IsNullOrWhiteSpace(m_ProjectDocument.Identity.PublicReference) AndAlso
            m_ProjectDocument.Identity.Revision.HasValue
        Dim technicalSelectionReference As String = String.Empty
        Dim technicalSelectionRevision As String = String.Empty
        If m_ProjectDocument IsNot Nothing AndAlso m_ProjectDocument.Identity IsNot Nothing Then
            technicalSelectionReference = If(hasRegisteredReference,
                m_ProjectDocument.Identity.PublicReference,
                m_ProjectDocument.Identity.LocalDraftReference)
            If hasRegisteredReference Then
                technicalSelectionRevision = String.Format("R{0:00}", m_ProjectDocument.Identity.Revision.Value)
            End If
        End If

        headerDataRow("TechnicalSelectionReference_Caption") = Project_Text(
            "MainForm_Project_Title", "Technical selection")
        headerDataRow("TechnicalSelectionReference_Value") = If(technicalSelectionReference, String.Empty)
        headerDataRow("TechnicalSelectionRevision_Caption") = Project_Text(
            "MainForm_TechnicalSelectionRevision", "Revision")
        headerDataRow("TechnicalSelectionRevision_Value") = technicalSelectionRevision
        headerDataRow("TechnicalSelectionStatus_Caption") = Project_Text(
            "MainForm_TechnicalSelectionStatus", "Status")
        headerDataRow("TechnicalSelectionStatus_Value") = If(hasRegisteredReference,
            Project_Text("MainForm_TechnicalSelectionStatus_Registered", "Registered"),
            Project_Text("MainForm_TechnicalSelectionStatus_Draft", "Draft"))

        headerDataRow.Note = m_Note_Text.Text

        If cmbPerformance_Series.SelectedItem = "UKUNDA" Then
            headerDataRow.Item_Code = txbPerformance_ItemCode.Text
            headerDataRow.Item_Description = lblPerformance_ItemDescr.Text
        ElseIf cmbPerformance_Series.SelectedItem = "RAHU" Then
            headerDataRow.Item_Code = txbPerformance_ItemCodeQTM.Text
            headerDataRow.Item_Description = lblPerformance_ItemDescrQTM.Text
        Else
            headerDataRow.Item_Code = txbPerformance_ItemCode.Text
            headerDataRow.Item_Description = lblPerformance_ItemDescr.Text
        End If

        reportDataSet.HeaderDataTable.Rows.Add(headerDataRow)

        ' Performance Accordance
        performanceAccordanceDataRow = reportDataSet.PerformanceAccordanceDataTable.NewPerformanceAccordanceDataTableRow()
        performanceAccordanceDataRow.Title = Environment.Localization.GetString(CLMessageResources.PerformancesAccordanceWith.ToString())

        performanceAccordanceDataRow.ExternalLeakage_Caption = Environment.Localization.GetString(CLMessageResources.ExternalLeakage.ToString())
        performanceAccordanceDataRow.ExternalLeakage_Value = CLEnvironment.ExternalLeakage

        performanceAccordanceDataRow.InternalLeakage_Caption = Environment.Localization.GetString(CLMessageResources.InternalLeakage.ToString())
        performanceAccordanceDataRow.InternalLeakage_Value = CLEnvironment.InternalLeakage

        performanceAccordanceDataRow.AirflowPressure_Caption = Environment.Localization.GetString(CLMessageResources.AirflowPressure.ToString())
        performanceAccordanceDataRow.AirflowPressure_Value = CLEnvironment.AirflowPressure

        performanceAccordanceDataRow.ElectricPowerInput_Caption = Environment.Localization.GetString(CLMessageResources.ElectricPowerInput.ToString())
        performanceAccordanceDataRow.ElectricPowerInput_Value = CLEnvironment.ElectricPowerInput

        performanceAccordanceDataRow.NoiseLevel_Caption = Environment.Localization.GetString(CLMessageResources.NoiseLevel.ToString())
        performanceAccordanceDataRow.NoiseLevel_Value = CLEnvironment.NoiseLevel

        performanceAccordanceDataRow.Legal_Notice = Environment.Localization.GetString(CLMessageResources.Legal_Notice.ToString())

        reportDataSet.PerformanceAccordanceDataTable.Rows.Add(performanceAccordanceDataRow)

        ' Working Point
        '---------------------------------------------------
        workingPointDataRow = reportDataSet.WorkingPointDataTable.NewWorkingPointDataTableRow()

        workingPointDataRow.Title = String.Format("{0} - {1}",
            Report_WinterScenarioName(),
            Environment.Localization.GetString(CLMessageResources.PDF_WorkingPoint.ToString()))

        workingPointDataRow.AirFlow_Caption = lblPerformance_AirFlow.Text
        workingPointDataRow.AirFlow_Value = txbPerformance_AirFlow.Text

        workingPointDataRow.MaxPressure_Caption = lblPerformance_MaxPressure.Text
        workingPointDataRow.MaxPressure_Value = txbPerformance_MaxPressure.Text

        workingPointDataRow.PowerInput_Caption = lblPerformance_ElectricalPerformances_PowerInput.Text
        workingPointDataRow.PowerInput_Value = txbPerformance_ElectricalPerformances_PowerInput.Text

        If chbPerformance_SFP_ShowArea.Checked Then
            workingPointDataRow.SFP_Caption = String.Format("{0} <= {1}", lblPerformance_SFP_SFP.Text, nudPerformance_SFP_Limit.Value)
        Else
            workingPointDataRow.SFP_Caption = lblPerformance_SFP_SFP.Text
        End If
        workingPointDataRow.SFP_Value = txbPerformance_SFP.Text

        If chbPerformance_SEL_ShowArea.Checked Then
            workingPointDataRow.SEL_Caption = String.Format("{0} <= {1}", lblPerformance_SEL_SEL.Text, nudPerformance_SEL_Limit.Value)
        Else
            workingPointDataRow.SEL_Caption = lblPerformance_SEL_SEL.Text
        End If
        workingPointDataRow.SEL_Value = txbPerformance_SEL.Text

        workingPointDataRow.RegLev_Caption = Environment.Localization.GetString(CLMessageResources.PDF_RegLev.ToString())
        workingPointDataRow.RegLev_Value = hsbPerformance_RegulationLevel.Value
        workingPointDataRow.RegLev_Note = Environment.Localization.GetString(CLMessageResources.RegLev_Note.ToString())

        reportDataSet.WorkingPointDataTable.Rows.Add(workingPointDataRow)

        ' Temperature Conditions And Humidity
        '---------------------------------------------------
        temperatureConditionsAndHumidityDataRow = reportDataSet.TemperatureConditionsAndHumidityDataTable.NewTemperatureConditionsAndHumidityDataTableRow()

        temperatureConditionsAndHumidityDataRow.Title = String.Format("{0} - {1}",
            Report_WinterScenarioName(),
            Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString()))

        temperatureConditionsAndHumidityDataRow.FreshInletTemp_Caption = lblPerformance_FreshInletTemperature.Text
        temperatureConditionsAndHumidityDataRow.FreshInletTemp_Value = txbPerformance_FreshInletTemperature.Text
        temperatureConditionsAndHumidityDataRow.FreshInletTemp_RH_Caption = lblPerformance_RHFreshInlet.Text
        temperatureConditionsAndHumidityDataRow.FreshInletTemp_RH_Value = txbPerformance_RHFreshInlet.Text

        temperatureConditionsAndHumidityDataRow.ReturnInletTemp_Caption = lblPerformance_ReturnInletTemperature.Text
        temperatureConditionsAndHumidityDataRow.ReturnInletTemp_Value = txbPerformance_ReturnInletTemperature.Text
        temperatureConditionsAndHumidityDataRow.ReturnInletTemp_RH_Caption = lblPerformance_RHReturnInlet.Text
        temperatureConditionsAndHumidityDataRow.ReturnInletTemp_RH_Value = txbPerformance_RHReturnInlet.Text

        temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_Caption = lblPerformance_SupplyOutletTemperature.Text
        temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_Value = txbPerformance_SupplyOutletTemperature.Text
        temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_RH_Caption = lblPerformance_SupplyOutletRH.Text
        temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_RH_Value = txbPerformance_SupplyOutletRH.Text

        temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_Caption = lblPerformance_ExhaustOutletTemperature.Text
        temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_Value = txbPerformance_ExhaustOutletTemperature.Text
        temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_RH_Caption = lblPerformance_ExhaustOutletRH.Text
        temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_RH_Value = txbPerformance_ExhaustOutletRH.Text

        reportDataSet.TemperatureConditionsAndHumidityDataTable.Rows.Add(temperatureConditionsAndHumidityDataRow)

        If m_SummerCalculationEnabled AndAlso m_HasLastSummerThermo Then
            temperatureConditionsAndHumidityDataRow = reportDataSet.TemperatureConditionsAndHumidityDataTable.NewTemperatureConditionsAndHumidityDataTableRow()

            temperatureConditionsAndHumidityDataRow.Title = String.Format("{0} - {1}",
                Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString()),
                Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString()))

            temperatureConditionsAndHumidityDataRow.FreshInletTemp_Caption = lblPerformance_FreshInletTemperature.Text
            temperatureConditionsAndHumidityDataRow.FreshInletTemp_Value = TextBox3.Text
            temperatureConditionsAndHumidityDataRow.FreshInletTemp_RH_Caption = lblPerformance_RHFreshInlet.Text
            temperatureConditionsAndHumidityDataRow.FreshInletTemp_RH_Value = TextBox4.Text

            temperatureConditionsAndHumidityDataRow.ReturnInletTemp_Caption = lblPerformance_ReturnInletTemperature.Text
            temperatureConditionsAndHumidityDataRow.ReturnInletTemp_Value = TextBox5.Text
            temperatureConditionsAndHumidityDataRow.ReturnInletTemp_RH_Caption = lblPerformance_RHReturnInlet.Text
            temperatureConditionsAndHumidityDataRow.ReturnInletTemp_RH_Value = TextBox6.Text

            temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_Caption = lblPerformance_SupplyOutletTemperature.Text
            temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_Value = TextBox12.Text
            temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_RH_Caption = lblPerformance_SupplyOutletRH.Text
            temperatureConditionsAndHumidityDataRow.SupplyOutletTemp_RH_Value = TextBox14.Text

            temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_Caption = lblPerformance_ExhaustOutletTemperature.Text
            temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_Value = TextBox13.Text
            temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_RH_Caption = lblPerformance_ExhaustOutletRH.Text
            temperatureConditionsAndHumidityDataRow.ExhaustOutletTemp_RH_Value = TextBox15.Text

            reportDataSet.TemperatureConditionsAndHumidityDataTable.Rows.Add(temperatureConditionsAndHumidityDataRow)
        End If

        ' Temperature Conditions And Humidity
        '---------------------------------------------------
        heatExchangerPerformancesDataRow = reportDataSet.HeatExchangerPerformancesDataTable.NewHeatExchangerPerformancesDataTableRow()

        heatExchangerPerformancesDataRow.Title = String.Format("{0} - {1}",
            Report_WinterScenarioName(),
            Environment.Localization.GetString(CLMessageResources.MainForm_HeatExchangerPerformances.ToString()))

        heatExchangerPerformancesDataRow.HeatTransferred_Caption = lblPerformance_HeatTransferred.Text
        heatExchangerPerformancesDataRow.HeatTransferred_Value = txbPerformance_HeatTransferred.Text

        heatExchangerPerformancesDataRow.Efficiency_Caption = txbPerformance_Efficiency.Text
        heatExchangerPerformancesDataRow.Efficiency_Value = lblPerformance_Efficiency.Text

        heatExchangerPerformancesDataRow.SensibleHeat_Caption = lblPerformance_SensibleHeat.Text
        heatExchangerPerformancesDataRow.SensibleHeat_Value = txbPerformance_SensibleHeat.Text

        heatExchangerPerformancesDataRow.WaterProduced_Caption = lblPerformance_WaterProduced.Text
        heatExchangerPerformancesDataRow.WaterProduced_Value = txbPerformance_WaterProduced.Text

        heatExchangerPerformancesDataRow.LatentHeat_Caption = lblPerformance_LatentHeat.Text
        heatExchangerPerformancesDataRow.LatentHeat_Value = txbPerformance_LatentHeat.Text

        reportDataSet.HeatExchangerPerformancesDataTable.Rows.Add(heatExchangerPerformancesDataRow)

        If m_SummerCalculationEnabled AndAlso m_HasLastSummerThermo Then
            heatExchangerPerformancesDataRow = reportDataSet.HeatExchangerPerformancesDataTable.NewHeatExchangerPerformancesDataTableRow()

            heatExchangerPerformancesDataRow.Title = String.Format("{0} - {1}",
                Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString()),
                Environment.Localization.GetString(CLMessageResources.MainForm_HeatExchangerPerformances.ToString()))

            heatExchangerPerformancesDataRow.HeatTransferred_Caption = lblPerformance_HeatTransferred.Text
            heatExchangerPerformancesDataRow.HeatTransferred_Value = TextBox10.Text

            heatExchangerPerformancesDataRow.Efficiency_Caption = txbPerformance_Efficiency.Text
            heatExchangerPerformancesDataRow.Efficiency_Value = TextBox11.Text

            heatExchangerPerformancesDataRow.SensibleHeat_Caption = lblPerformance_SensibleHeat.Text
            heatExchangerPerformancesDataRow.SensibleHeat_Value = TextBox9.Text

            heatExchangerPerformancesDataRow.WaterProduced_Caption = lblPerformance_WaterProduced.Text
            heatExchangerPerformancesDataRow.WaterProduced_Value = TextBox8.Text

            heatExchangerPerformancesDataRow.LatentHeat_Caption = lblPerformance_LatentHeat.Text
            heatExchangerPerformancesDataRow.LatentHeat_Value = TextBox7.Text

            reportDataSet.HeatExchangerPerformancesDataTable.Rows.Add(heatExchangerPerformancesDataRow)
        End If

        ' Sound Power
        '---------------------------------------------------
        Dim soundPowerHeaderDataRow As CLMainReportDataSet.SoundPowerHeaderDataTableRow
        Dim directivity As Int16

        If rdb_Q2.Checked Then
            directivity = 2
        ElseIf rdb_Q4.Checked Then
            directivity = 4
        ElseIf rdb_Q8.Checked Then
            directivity = 8
        End If

        soundPowerHeaderDataRow = reportDataSet.SoundPowerHeaderDataTable.NewSoundPowerHeaderDataTableRow()

        soundPowerHeaderDataRow.sndaddtorep_bool = Not chbSoundPerformances_addtoreport.Checked  ' La proprietà è Hidden quindi la logica è negativa

        soundPowerHeaderDataRow.Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_Caption).HeaderText
        soundPowerHeaderDataRow.Title = Environment.Localization.GetString(CLMessageResources.SoundPower.ToString())
        soundPowerHeaderDataRow.Hz63_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_63Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Hz125_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_125Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Hz250_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_250Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Hz500_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_500Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Hz1000_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_1000Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Hz2000_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_2000Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Hz4000_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_4000Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Hz8000_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_8000Hz).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.LwA_Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_LwA).HeaderText.Replace(vbCrLf, " ")
        soundPowerHeaderDataRow.Lp1Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_Lp1).HeaderText.Replace(vbCrLf, " ") + " Q = " + directivity.ToString
        soundPowerHeaderDataRow.Lp2Caption = dgvPerformance_SoundPower.Columns(SoundColumnName_Lp2).HeaderText.Replace(vbCrLf, " ") + " Q = " + directivity.ToString

        reportDataSet.SoundPowerHeaderDataTable.Rows.Add(soundPowerHeaderDataRow)

        For Each soundPerformance As DataRow In m_SoundPerformances.Rows

            Dim soundPowerDataRow As CLMainReportDataSet.SoundPowerDataTableRow

            soundPowerDataRow = reportDataSet.SoundPowerDataTable.NewSoundPowerDataTableRow()

            soundPowerDataRow.Type = soundPerformance(SoundColumnName_Type)
            soundPowerDataRow.Caption = SoundPerformance_GetLocalizedCaption(
                soundPerformance(SoundColumnName_Type),
                soundPerformance(SoundColumnName_Caption))
            soundPowerDataRow.Hz63_Value = soundPerformance(SoundColumnName_63Hz)
            soundPowerDataRow.Hz125_Value = soundPerformance(SoundColumnName_125Hz)
            soundPowerDataRow.Hz250_Value = soundPerformance(SoundColumnName_250Hz)
            soundPowerDataRow.Hz500_Value = soundPerformance(SoundColumnName_500Hz)
            soundPowerDataRow.Hz1000_Value = soundPerformance(SoundColumnName_1000Hz)
            soundPowerDataRow.Hz2000_Value = soundPerformance(SoundColumnName_2000Hz)
            soundPowerDataRow.Hz4000_Value = soundPerformance(SoundColumnName_4000Hz)
            soundPowerDataRow.Hz8000_Value = soundPerformance(SoundColumnName_8000Hz)
            soundPowerDataRow.LwA_Value = soundPerformance(SoundColumnName_LwA)
            soundPowerDataRow.Lp1_Value = soundPerformance(SoundColumnName_Lp1)
            soundPowerDataRow.Lp2_Value = soundPerformance(SoundColumnName_Lp2)
            reportDataSet.SoundPowerDataTable.Rows.Add(soundPowerDataRow)

        Next

        ' Diagram
        '---------------------------------------------------
        Dim selectedLayoutCode As String = Nothing
        Dim selectedInstallationMode As String = Nothing
        If m_ProjectDocument IsNot Nothing AndAlso
            m_ProjectDocument.Selection IsNot Nothing Then
            selectedLayoutCode = m_ProjectDocument.Selection.LayoutCode
            selectedInstallationMode = m_ProjectDocument.Selection.InstallationMode
        End If
        Using installation = CLInstallationLayoutReportRenderer.Create(
            SelectedHeatRecoveryModel, selectedLayoutCode, selectedInstallationMode)
            Dim installationColumns = {
                New KeyValuePair(Of String, Type)("InstallationImage", GetType(Byte())),
                New KeyValuePair(Of String, Type)("InstallationTitle", GetType(String)),
                New KeyValuePair(Of String, Type)("InstallationConfigurationCaption", GetType(String)),
                New KeyValuePair(Of String, Type)("InstallationConfigurationValue", GetType(String)),
                New KeyValuePair(Of String, Type)("InstallationModeCaption", GetType(String)),
                New KeyValuePair(Of String, Type)("InstallationModeValue", GetType(String))}
            For Each column In installationColumns
                If Not reportDataSet.DiagramDataTable.Columns.Contains(column.Key) Then
                    reportDataSet.DiagramDataTable.Columns.Add(column.Key, column.Value)
                End If
            Next
            diagramDataRow = reportDataSet.DiagramDataTable.NewDiagramDataTableRow()
            diagramDataRow.AirFlowImage = Bitmap_GetBytes(airflowImage)
            diagramDataRow.LegendImage = Bitmap_GetBytes(legendImage)
            diagramDataRow.PowerImage = Bitmap_GetBytes(powerImage)
            diagramDataRow.PressureImage = Bitmap_GetBytes(pressureImage)
            diagramDataRow.CO2Image = Bitmap_GetBytes(CO2Image)
            diagramDataRow("InstallationImage") = Bitmap_GetBytes(installation.Image)
            diagramDataRow("InstallationTitle") = installation.Title
            diagramDataRow("InstallationConfigurationCaption") = installation.ConfigurationCaption
            diagramDataRow("InstallationConfigurationValue") = installation.ConfigurationValue
            diagramDataRow("InstallationModeCaption") = installation.InstallationCaption
            diagramDataRow("InstallationModeValue") = installation.InstallationValue
            reportDataSet.DiagramDataTable.Rows.Add(diagramDataRow)
        End Using
        reportDataSet.AcceptChanges()

        Dim winterWorkingPointTable As DataTable = Report_CopyRows(reportDataSet.WorkingPointDataTable, Function(row) True)
        Dim winterTemperatureTable As DataTable = Report_CopyRows(reportDataSet.TemperatureConditionsAndHumidityDataTable, Function(row) row.Table.Rows.IndexOf(row) = 0)
        Dim winterHeatExchangerTable As DataTable = Report_CopyRows(reportDataSet.HeatExchangerPerformancesDataTable, Function(row) row.Table.Rows.IndexOf(row) = 0)

        Dim summerWorkingPointTable As DataTable = Report_CopyRows(reportDataSet.WorkingPointDataTable, Function(row) False)
        Dim summerTemperatureTable As DataTable = Report_CopyRows(reportDataSet.TemperatureConditionsAndHumidityDataTable, Function(row) row.Table.Rows.IndexOf(row) = 1)
        Dim summerHeatExchangerTable As DataTable = Report_CopyRows(reportDataSet.HeatExchangerPerformancesDataTable, Function(row) row.Table.Rows.IndexOf(row) = 1)
        Report_FillSummerWorkingPointTable(summerWorkingPointTable, workingPointDataRow)

        Dim waterCoilWinterReportTable As DataTable = Report_CopyRows(waterCoilReportDataTable, Function(row) row("ScenarioKey").ToString() = "Winter")
        Dim waterCoilSummerReportTable As DataTable = Report_CopyRows(waterCoilReportDataTable, Function(row) row("ScenarioKey").ToString() = "Summer")
        Dim electricHeaterEHDReportTable As DataTable = Report_CopyRows(electricHeaterReportDataTable, Function(row) row("Mode").ToString() = CLElectricHeaterMode.EHD.ToString())
        Dim electricHeaterPEHDReportTable As DataTable = Report_CopyRows(electricHeaterReportDataTable, Function(row) row("Mode").ToString() = CLElectricHeaterMode.PEHD.ToString())

        ' Show Report
        ' --------------------------------------------
        Dim reportViewForm As New CLReportViewerForm
        AddHandler reportViewForm.PdfExported, AddressOf Project_ReportPdfExported
        AddHandler reportViewForm.AddToProjectRequested, AddressOf MultiSelection_AddCurrentReport
        Dim reportDataSources As New List(Of Microsoft.Reporting.WinForms.ReportDataSource)

        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("Header", DirectCast(reportDataSet.HeaderDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("PerformanceAccordance", DirectCast(reportDataSet.PerformanceAccordanceDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("TemperatureConditionsAndHumidity", DirectCast(reportDataSet.TemperatureConditionsAndHumidityDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SoundPower", DirectCast(reportDataSet.SoundPowerDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SoundPowerHeader", DirectCast(reportDataSet.SoundPowerHeaderDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WorkingPoint", DirectCast(reportDataSet.WorkingPointDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("HeatExchangerPerformances", DirectCast(reportDataSet.HeatExchangerPerformancesDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("Diagram", DirectCast(reportDataSet.DiagramDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("CO2LevelRoom", DirectCast(reportDataSet.CO2LevelRoom, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("CO2LevelUse", DirectCast(reportDataSet.CO2LevelUse, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("CO2LevelParameters", DirectCast(reportDataSet.CO2LevelParameters, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WaterCoilReport", waterCoilReportDataTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WinterWorkingPoint", winterWorkingPointTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WinterTemperatureConditionsAndHumidity", winterTemperatureTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WinterHeatExchangerPerformances", winterHeatExchangerTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SummerWorkingPoint", summerWorkingPointTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SummerTemperatureConditionsAndHumidity", summerTemperatureTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SummerHeatExchangerPerformances", summerHeatExchangerTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WaterCoilWinterReport", waterCoilWinterReportTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WaterCoilSummerReport", waterCoilSummerReportTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("ElectricHeaterReport", electricHeaterReportDataTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("ElectricHeaterEHDReport", electricHeaterEHDReportTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("ElectricHeaterPEHDReport", electricHeaterPEHDReportTable))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("AccessoryReport", accessoryReportDataTable))

        waitForm.Hide()

        Dim hasWaterCoilReport As Boolean = waterCoilReportDataTable.Rows.Count > 0
        Dim hasElectricHeaterReport As Boolean = electricHeaterReportDataTable.Rows.Count > 0
        Dim hasAirTreatmentAccessoryReport As Boolean = hasWaterCoilReport OrElse hasElectricHeaterReport
        Dim reportFileName As String
        If chbCO2Level_addtoreport.Checked Then
            reportFileName = If(hasAirTreatmentAccessoryReport, "CLMainReportWithCO2_Coil.rdlc", "CLMainReportWithCO2.rdlc")
        Else
            reportFileName = If(hasAirTreatmentAccessoryReport, "CLMainReport_Coil.rdlc", "CLMainReport.rdlc")
        End If

        If m_TechnicalBaselineReportSink IsNot Nothing Then
            m_TechnicalBaselineReportSink(reportDataSources, reportFileName)
            reportViewForm.Dispose()
            waitForm.Dispose()
            Return
        End If
        If m_NextUiReportSink IsNot Nothing Then
            m_NextUiReportSink(reportDataSources, reportFileName)
            reportViewForm.Dispose()
            waitForm.Dispose()
            Return
        End If

        reportViewForm.SetReport(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), reportFileName),
        reportDataSources.ToArray(), Microsoft.Reporting.WinForms.DisplayMode.PrintLayout)

        Dim customerReference As String = If(m_Note_Text Is Nothing, String.Empty, m_Note_Text.Text.Trim())
        Dim registrationReference As String = String.Empty
        If m_ProjectDocument IsNot Nothing AndAlso m_ProjectDocument.Identity IsNot Nothing Then
            registrationReference = If(Not String.IsNullOrWhiteSpace(m_ProjectDocument.Identity.PublicReference),
                m_ProjectDocument.Identity.PublicReference,
                m_ProjectDocument.Identity.LocalDraftReference)
        End If
        reportViewForm.SetEmailContext(SelectedHeatRecoveryModelCustomerName,
            customerReference,
            workingPointDataRow.AirFlow_Value,
            workingPointDataRow.MaxPressure_Value,
            registrationReference,
            If(m_ProjectDocument Is Nothing, Guid.Empty, m_ProjectDocument.ProjectId),
            m_ProjectFilePath)
        AddHandler reportViewForm.FollowUpPrepared, AddressOf FollowUp_EmailPrepared
        AddHandler reportViewForm.FollowUpLocalPathRequested, AddressOf FollowUp_LocalPathRequested

        Dim nomeFileSuffisso As String

        If (m_Note_Text.Text.Length()) Then
            nomeFileSuffisso = m_Note_Text.Text.Replace(" ", "_") & "_"
        Else
            nomeFileSuffisso = ""
        End If

        reportViewForm.WindowState = FormWindowState.Maximized
        reportViewForm.rpvReport.LocalReport.DisplayName = nomeFileSuffisso &
            SelectedHeatRecoveryModelCustomerName & "_" &
            workingPointDataRow.AirFlow_Value & "_" &
            workingPointDataRow.MaxPressure_Value & "_" &
            "Report_" & Environment.PrimaryLanguageCode

        reportViewForm.ShowDialog()
    End Sub

    Private Function Report_CopyRows(source As DataTable, includeRow As Func(Of DataRow, Boolean)) As DataTable
        Dim result As DataTable = source.Clone()
        For Each row As DataRow In source.Rows
            If includeRow(row) Then
                result.ImportRow(row)
            End If
        Next

        result.AcceptChanges()
        Return result
    End Function

    Private Function Report_WinterScenarioName() As String
        If Not String.IsNullOrWhiteSpace(m_WinterReportScenarioName) Then
            Return m_WinterReportScenarioName
        End If

        Return Environment.Localization.GetString(CLMessageResources.MainForm_Winter.ToString())
    End Function

    Private Sub Report_FillSummerWorkingPointTable(table As DataTable, winterRow As CLMainReportDataSet.WorkingPointDataTableRow)
        If Not m_SummerCalculationEnabled OrElse Not m_HasLastSummerThermo Then
            Return
        End If

        Dim row As DataRow = table.NewRow()
        row("Title") = String.Format("{0} - {1}",
            Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString()),
            Environment.Localization.GetString(CLMessageResources.PDF_WorkingPoint.ToString()))
        row("AirFlow_Caption") = winterRow.AirFlow_Caption
        row("AirFlow_Value") = TextBox1.Text
        row("MaxPressure_Caption") = winterRow.MaxPressure_Caption
        row("MaxPressure_Value") = TextBox2.Text
        row("PowerInput_Caption") = winterRow.PowerInput_Caption
        row("PowerInput_Value") = winterRow.PowerInput_Value
        row("SFP_Caption") = winterRow.SFP_Caption
        row("SFP_Value") = winterRow.SFP_Value
        row("SEL_Caption") = winterRow.SEL_Caption
        row("SEL_Value") = winterRow.SEL_Value
        row("RegLev_Caption") = winterRow.RegLev_Caption
        row("RegLev_Value") = winterRow.RegLev_Value
        row("RegLev_Note") = winterRow.RegLev_Note
        table.Rows.Add(row)
        table.AcceptChanges()
    End Sub

    Private Function Report_CreateWaterCoilReportTable() As DataTable
        Dim table As New DataTable("WaterCoilReport")
        Dim columns As String() = {
            "Visible",
            "ScenarioKey",
            "Title",
            "ScenarioCaption",
            "Scenario",
            "CoilCaption",
            "Coil",
            "CaseCaption",
            "Case",
            "FluidCaption",
            "Fluid",
            "FluidInCaption",
            "FluidIn",
            "FluidOutCaption",
            "FluidOut",
            "FluidTemperatureCaption",
            "FluidTemperature",
            "GeometryCaption",
            "Geometry",
            "GeometryTypeCaption",
            "GeometryType",
            "LengthCaption",
            "LengthValue",
            "HeightCaption",
            "HeightValue",
            "RowsCaption",
            "RowsValue",
            "FinSpacingCaption",
            "FinSpacingValue",
            "CircuitsCaption",
            "CircuitsValue",
            "ModeCaption",
            "Mode",
            "StatusCaption",
            "Status",
            "CapacityCaption",
            "Capacity",
            "SensibleCaption",
            "Sensible",
            "AirOutCaption",
            "AirOut",
            "RHOutCaption",
            "RHOut",
            "CondCaption",
            "Cond",
            "AirDPCaption",
            "AirDP",
            "WaterDPCaption",
            "WaterDP",
            "FluidFlowCaption",
            "FluidFlow",
            "FluidSpeedCaption",
            "FluidSpeed",
            "FaceVelocityCaption",
            "FaceVelocity",
            "CustomDisclaimerAccepted",
            "Summary"
        }

        For Each column As String In columns
            table.Columns.Add(column, GetType(String))
        Next

        If chbCoilPerformance_Enable Is Nothing OrElse Not chbCoilPerformance_Enable.Checked Then
            Return table
        End If

        If dgvCoilPerformance_Results Is Nothing OrElse dgvCoilPerformance_Results.Rows.Count = 0 Then
            Return table
        End If

        Dim selectedMode As CLCoilPerformanceEditMode = CoilPerformance_SelectedEditMode()
        Dim fluidType As CLCOFluidType = CoilPerformance_SelectedFluidType()
        Dim fluidText As String = CoilPerformance_FluidTypeName(fluidType)

        If fluidType <> CLCOFluidType.Water Then
            fluidText = String.Format("{0} - {1}: {2}%",
                fluidText,
                CoilPerformance_Text("MainForm_CoilPerformance_Glycol", "Glycol [%]").Replace(" [%]", ""),
                FormatNumber(nudCoilPerformance_FluidTec.Value, 1))
        End If

        Dim geometryText As String = String.Empty
        Dim heightText As String = String.Empty
        If selectedMode <> CLCoilPerformanceEditMode.Standard Then
            If CoilPerformance_HeightMode() = "tubes" Then
                heightText = String.Format("{0} / {1} {2}",
                    FormatNumber(nudCoilPerformance_Height.Value, 0),
                    FormatNumber(nudCoilPerformance_Tubes.Value, 0),
                    CoilPerformance_Text("MainForm_CoilPerformance_HeightTubes", "tubes"))
            Else
                heightText = FormatNumber(nudCoilPerformance_Height.Value, 0)
            End If

            geometryText = String.Format("{0}: {1}; {2}: {3}; {4}: {5}; {6}: {7}; {8}: {9}",
                CoilPerformance_Text("MainForm_CoilPerformance_Length", "Length [mm]"),
                FormatNumber(nudCoilPerformance_Length.Value, 0),
                CoilPerformance_Text("MainForm_CoilPerformance_Height", "Height [mm/tubes]"),
                heightText,
                CoilPerformance_Text("MainForm_CoilPerformance_Rows", "Rows"),
                FormatNumber(nudCoilPerformance_Rows.Value, 0),
                CoilPerformance_Text("MainForm_CoilPerformance_FinSpacing", "Fin spacing [mm]"),
                FormatNumber(CoilPerformance_SelectedFinSpacing(), 1),
                CoilPerformance_Text("MainForm_CoilPerformance_Circuits", "Circuits"),
                FormatNumber(nudCoilPerformance_Circuits.Value, 0))
        End If

        For Each row As DataGridViewRow In dgvCoilPerformance_Results.Rows
            If row.IsNewRow Then
                Continue For
            End If

            Dim mode As String = Report_DataGridValue(row, "Mode")
            Dim scenarioKey As String = "Winter"
            Dim scenario As String = Report_WinterScenarioName()
            Dim fluidInCaption As String = CoilPerformance_Text("MainForm_CoilPerformance_HeatingIn", "Heating in [C]")
            Dim fluidOutCaption As String = CoilPerformance_Text("MainForm_CoilPerformance_HeatingOut", "Heating out [C]")
            Dim fluidInValue As String = FormatNumber(nudCoilPerformance_HeatingIn.Value, 1)
            Dim fluidOutValue As String = FormatNumber(nudCoilPerformance_HeatingOut.Value, 1)
            Dim fluidTemperature As String = String.Format("{0}: {1}; {2}: {3}",
                fluidInCaption,
                fluidInValue,
                fluidOutCaption,
                fluidOutValue)

            If mode = CLCoilPerformanceMode.CWD.ToString() AndAlso m_SummerCalculationEnabled Then
                scenarioKey = "Summer"
                scenario = Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString())
                fluidInCaption = CoilPerformance_Text("MainForm_CoilPerformance_CoolingIn", "Cooling in [C]")
                fluidOutCaption = CoilPerformance_Text("MainForm_CoilPerformance_CoolingOut", "Cooling out [C]")
                fluidInValue = FormatNumber(nudCoilPerformance_CoolingIn.Value, 1)
                fluidOutValue = FormatNumber(nudCoilPerformance_CoolingOut.Value, 1)
                fluidTemperature = String.Format("{0}: {1}; {2}: {3}",
                    fluidInCaption,
                    fluidInValue,
                    fluidOutCaption,
                    fluidOutValue)
            ElseIf mode = CLCoilPerformanceMode.CWD.ToString() Then
                fluidInCaption = CoilPerformance_Text("MainForm_CoilPerformance_CoolingIn", "Cooling in [C]")
                fluidOutCaption = CoilPerformance_Text("MainForm_CoilPerformance_CoolingOut", "Cooling out [C]")
                fluidInValue = FormatNumber(nudCoilPerformance_CoolingIn.Value, 1)
                fluidOutValue = FormatNumber(nudCoilPerformance_CoolingOut.Value, 1)
                fluidTemperature = String.Format("{0}: {1}; {2}: {3}",
                    fluidInCaption,
                    fluidInValue,
                    fluidOutCaption,
                    fluidOutValue)
            End If

            Dim reportRow As DataRow = table.NewRow()
            reportRow("Visible") = "True"
            reportRow("ScenarioKey") = scenarioKey
            reportRow("Title") = CoilPerformance_Text("MainForm_CoilPerformance_Tab", "Water coils")
            reportRow("ScenarioCaption") = String.Empty
            reportRow("Scenario") = scenario
            reportRow("CoilCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Coil", "Coil")
            reportRow("Coil") = cmbCoilPerformance_Coil.Text
            reportRow("CaseCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Installation", "Installation")
            reportRow("Case") = cmbCoilPerformance_Installation.Text
            reportRow("FluidCaption") = CoilPerformance_Text("MainForm_CoilPerformance_FluidType", "Fluid")
            reportRow("Fluid") = fluidText
            reportRow("FluidInCaption") = fluidInCaption
            reportRow("FluidIn") = fluidInValue
            reportRow("FluidOutCaption") = fluidOutCaption
            reportRow("FluidOut") = fluidOutValue
            reportRow("FluidTemperatureCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Fluid", "Fluid and water temperatures")
            reportRow("FluidTemperature") = fluidTemperature
            reportRow("GeometryCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Geometry", "Geometry")
            reportRow("Geometry") = geometryText
            reportRow("GeometryTypeCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Geometry", "Geometry")
            reportRow("GeometryType") = If(selectedMode = CLCoilPerformanceEditMode.StandardCustomized,
                cmbCoilPerformance_EditMode.Text, String.Empty)
            reportRow("LengthCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Length", "Length [mm]")
            reportRow("LengthValue") = If(selectedMode = CLCoilPerformanceEditMode.StandardCustomized,
                FormatNumber(nudCoilPerformance_Length.Value, 0), String.Empty)
            reportRow("HeightCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Height", "Height [mm/tubes]")
            reportRow("HeightValue") = If(selectedMode = CLCoilPerformanceEditMode.StandardCustomized,
                heightText, String.Empty)
            reportRow("RowsCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Rows", "Rows")
            reportRow("RowsValue") = If(selectedMode = CLCoilPerformanceEditMode.StandardCustomized,
                FormatNumber(nudCoilPerformance_Rows.Value, 0), String.Empty)
            reportRow("FinSpacingCaption") = CoilPerformance_Text("MainForm_CoilPerformance_FinSpacing", "Fin spacing [mm]")
            reportRow("FinSpacingValue") = If(selectedMode = CLCoilPerformanceEditMode.StandardCustomized,
                FormatNumber(CoilPerformance_SelectedFinSpacing(), 1), String.Empty)
            reportRow("CircuitsCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Circuits", "Circuits")
            reportRow("CircuitsValue") = If(selectedMode = CLCoilPerformanceEditMode.StandardCustomized,
                FormatNumber(nudCoilPerformance_Circuits.Value, 0), String.Empty)
            reportRow("CustomDisclaimerAccepted") = If(
                selectedMode = CLCoilPerformanceEditMode.StandardCustomized AndAlso m_CoilCustomDisclaimerAccepted,
                CoilPerformance_Text("MainForm_CoilPerformance_CustomDisclaimerAccepted", "Custom coil design disclaimer accepted"),
                String.Empty)
            reportRow("ModeCaption") = dgvCoilPerformance_Results.Columns("Mode").HeaderText
            reportRow("Mode") = mode
            reportRow("StatusCaption") = dgvCoilPerformance_Results.Columns("Status").HeaderText
            reportRow("Status") = Report_DataGridValue(row, "Status")
            reportRow("CapacityCaption") = dgvCoilPerformance_Results.Columns("Capacity").HeaderText
            reportRow("Capacity") = Report_DataGridValue(row, "Capacity")
            reportRow("SensibleCaption") = dgvCoilPerformance_Results.Columns("Sensible").HeaderText
            reportRow("Sensible") = Report_DataGridValue(row, "Sensible")
            reportRow("AirOutCaption") = dgvCoilPerformance_Results.Columns("TempOut").HeaderText
            reportRow("AirOut") = Report_DataGridValue(row, "TempOut")
            reportRow("RHOutCaption") = dgvCoilPerformance_Results.Columns("RHOut").HeaderText
            reportRow("RHOut") = Report_DataGridValue(row, "RHOut")
            reportRow("CondCaption") = dgvCoilPerformance_Results.Columns("Cond").HeaderText
            reportRow("Cond") = Report_DataGridValue(row, "Cond")
            reportRow("AirDPCaption") = dgvCoilPerformance_Results.Columns("DP").HeaderText
            reportRow("AirDP") = Report_DataGridValue(row, "DP")
            reportRow("WaterDPCaption") = dgvCoilPerformance_Results.Columns("WaterDP").HeaderText
            reportRow("WaterDP") = Report_DataGridValue(row, "WaterDP")
            reportRow("FluidFlowCaption") = dgvCoilPerformance_Results.Columns("FluidFlow").HeaderText
            reportRow("FluidFlow") = Report_DataGridValue(row, "FluidFlow")
            reportRow("FluidSpeedCaption") = dgvCoilPerformance_Results.Columns("FluidSpeed").HeaderText
            reportRow("FluidSpeed") = Report_DataGridValue(row, "FluidSpeed")
            reportRow("FaceVelocityCaption") = dgvCoilPerformance_Results.Columns("Face").HeaderText
            reportRow("FaceVelocity") = Report_DataGridValue(row, "Face")
            reportRow("Summary") = String.Format("{0} | {1}: {2} | {3}: {4} | {5}: {6} | {7}: {8}{9}{10}{11}: {12} | {13}: {14} | {15}: {16} | {17}: {18} | {19}: {20} | {21}: {22} | {23}: {24} | {25}: {26} | {27}: {28} | {29}: {30} | {31}: {32} | {33}: {34}",
                reportRow("Scenario"),
                reportRow("CoilCaption"),
                reportRow("Coil"),
                reportRow("CaseCaption"),
                reportRow("Case"),
                reportRow("FluidCaption"),
                reportRow("Fluid"),
                reportRow("FluidTemperatureCaption"),
                reportRow("FluidTemperature"),
                If(String.IsNullOrWhiteSpace(geometryText), String.Empty, " | "),
                If(String.IsNullOrWhiteSpace(geometryText), String.Empty, String.Format("{0}: {1} | ", reportRow("GeometryCaption"), reportRow("Geometry"))),
                reportRow("ModeCaption"),
                reportRow("Mode"),
                reportRow("StatusCaption"),
                reportRow("Status"),
                reportRow("CapacityCaption"),
                reportRow("Capacity"),
                reportRow("SensibleCaption"),
                reportRow("Sensible"),
                reportRow("AirOutCaption"),
                reportRow("AirOut"),
                reportRow("RHOutCaption"),
                reportRow("RHOut"),
                reportRow("CondCaption"),
                reportRow("Cond"),
                reportRow("AirDPCaption"),
                reportRow("AirDP"),
                reportRow("WaterDPCaption"),
                reportRow("WaterDP"),
                reportRow("FluidFlowCaption"),
                reportRow("FluidFlow"),
                reportRow("FluidSpeedCaption"),
                reportRow("FluidSpeed"),
                reportRow("FaceVelocityCaption"),
                reportRow("FaceVelocity"))
            table.Rows.Add(reportRow)
        Next

        Return table
    End Function

    Private Function Report_DataGridValue(row As DataGridViewRow, columnName As String) As String
        If row Is Nothing OrElse Not dgvCoilPerformance_Results.Columns.Contains(columnName) Then
            Return String.Empty
        End If

        Dim value As Object = row.Cells(columnName).Value
        If value Is Nothing Then
            Return String.Empty
        End If

        Return value.ToString()
    End Function

#End Region

#Region "====[ SAP ]===="

    Private Sub sap_table_fill()

        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel

        Try
            If dgvSAP.Rows.Count = 0 Then
                Return
            End If

            Dim sap_point As Double()

            sap_point = sap_calc(dcHeatRecoveryModel, dgvSAP.Item(2, 0).Value * 3.6)
            dgvSAP.Item(0, 0).Value = "Kitchen + 1 Add wet room"
            dgvSAP.Item(1, 0).Value = 15
            dgvSAP.Item(2, 0).Value = 15
            dgvSAP.Item(3, 0).Value = sap_point(2)
            dgvSAP.Item(4, 0).Value = sap_point(0)
            dgvSAP.Item(5, 0).Value = sap_point(1)
            If (sap_point(0) < 1 And sap_point(1) > 85) Then
                dgvSAP.Item(6, 0).Value = "Yes"
                dgvSAP.Item(6, 0).Style.BackColor = Color.Green
            Else
                dgvSAP.Item(6, 0).Value = "No"
                dgvSAP.Item(6, 0).Style.BackColor = Color.Red
            End If
            sap_point = sap_calc(dcHeatRecoveryModel, dgvSAP.Item(2, 1).Value * 3.6)
            dgvSAP.Item(0, 1).Value = "Kitchen + 2 Add wet room"
            dgvSAP.Item(1, 1).Value = 21
            dgvSAP.Item(2, 1).Value = 21
            dgvSAP.Item(3, 1).Value = sap_point(2)
            dgvSAP.Item(4, 1).Value = sap_point(0)
            dgvSAP.Item(5, 1).Value = sap_point(1)
            If (sap_point(0) < 1 And sap_point(1) > 85) Then
                dgvSAP.Item(6, 1).Value = "Yes"
                dgvSAP.Item(6, 1).Style.BackColor = Color.Green
            Else
                dgvSAP.Item(6, 1).Value = "No"
                dgvSAP.Item(6, 1).Style.BackColor = Color.Red
            End If
            sap_point = sap_calc(dcHeatRecoveryModel, dgvSAP.Item(2, 2).Value * 3.6)
            dgvSAP.Item(0, 2).Value = "Kitchen + 3 Add wet room"
            dgvSAP.Item(1, 2).Value = 27
            dgvSAP.Item(2, 2).Value = 27
            dgvSAP.Item(3, 2).Value = sap_point(2)
            dgvSAP.Item(4, 2).Value = sap_point(0)
            dgvSAP.Item(5, 2).Value = sap_point(1)
            If (sap_point(0) < 1 And sap_point(1) > 85) Then
                dgvSAP.Item(6, 2).Value = "Yes"
                dgvSAP.Item(6, 2).Style.BackColor = Color.Green
            Else
                dgvSAP.Item(6, 2).Value = "No"
                dgvSAP.Item(6, 2).Style.BackColor = Color.Red
            End If
            sap_point = sap_calc(dcHeatRecoveryModel, dgvSAP.Item(2, 3).Value * 3.6)
            dgvSAP.Item(0, 3).Value = "Kitchen + 4 Add wet room"
            dgvSAP.Item(1, 3).Value = 33
            dgvSAP.Item(2, 3).Value = 33
            dgvSAP.Item(3, 3).Value = sap_point(2)
            dgvSAP.Item(4, 3).Value = sap_point(0)
            dgvSAP.Item(5, 3).Value = sap_point(1)
            If (sap_point(0) < 1 And sap_point(1) > 85) Then
                dgvSAP.Item(6, 3).Value = "Yes"
                dgvSAP.Item(6, 3).Style.BackColor = Color.Green
            Else
                dgvSAP.Item(6, 3).Value = "No"
                dgvSAP.Item(6, 3).Style.BackColor = Color.Red
            End If
            sap_point = sap_calc(dcHeatRecoveryModel, dgvSAP.Item(2, 4).Value * 3.6)
            dgvSAP.Item(0, 4).Value = "Kitchen + 5 Add wet room"
            dgvSAP.Item(1, 4).Value = 39
            dgvSAP.Item(2, 4).Value = 39
            dgvSAP.Item(3, 4).Value = sap_point(2)
            dgvSAP.Item(4, 4).Value = sap_point(0)
            dgvSAP.Item(5, 4).Value = sap_point(1)
            If (sap_point(0) < 1 And sap_point(1) > 85) Then
                dgvSAP.Item(6, 4).Value = "Yes"
                dgvSAP.Item(6, 4).Style.BackColor = Color.Green
            Else
                dgvSAP.Item(6, 4).Value = "No"
                dgvSAP.Item(6, 4).Style.BackColor = Color.Red
            End If
            sap_point = sap_calc(dcHeatRecoveryModel, dgvSAP.Item(2, 5).Value * 3.6)
            dgvSAP.Item(0, 5).Value = "Kitchen + 6 Add wet room"
            dgvSAP.Item(1, 5).Value = 45
            dgvSAP.Item(2, 5).Value = 45
            dgvSAP.Item(3, 5).Value = sap_point(2)
            dgvSAP.Item(4, 5).Value = sap_point(0)
            dgvSAP.Item(5, 5).Value = sap_point(1)
            If (sap_point(0) < 1 And sap_point(1) > 85) Then
                dgvSAP.Item(6, 5).Value = "Yes"
                dgvSAP.Item(6, 5).Style.BackColor = Color.Green
            Else
                dgvSAP.Item(6, 5).Value = "No"
                dgvSAP.Item(6, 5).Style.BackColor = Color.Red
            End If
            sap_point = sap_calc(dcHeatRecoveryModel, dgvSAP.Item(2, 6).Value * 3.6)
            dgvSAP.Item(0, 6).Value = "Kitchen + 7 Add wet room"
            dgvSAP.Item(1, 6).Value = 51
            dgvSAP.Item(2, 6).Value = 51
            dgvSAP.Item(3, 6).Value = sap_point(2)
            dgvSAP.Item(4, 6).Value = sap_point(0)
            dgvSAP.Item(5, 6).Value = sap_point(1)
            If (sap_point(0) < 1 And sap_point(1) > 85) Then
                dgvSAP.Item(6, 6).Value = "Yes"
                dgvSAP.Item(6, 6).Style.BackColor = Color.Green
            Else
                dgvSAP.Item(6, 6).Value = "No"
                dgvSAP.Item(6, 6).Style.BackColor = Color.Red
            End If
        Catch exception As Exception
            If m_NextUiPreparingReport Then Throw
            MessageBox.Show(Me, exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub sap_table_start()
        dgvSAP.Rows.Add()
        dgvSAP.Item(0, 0).Value = "Kitchen + 1 Add wet room"
        dgvSAP.Item(1, 0).Value = 15
        dgvSAP.Item(2, 0).Value = 15
        dgvSAP.Rows.Add()
        dgvSAP.Item(0, 1).Value = "Kitchen + 2 Add wet room"
        dgvSAP.Item(1, 1).Value = 21
        dgvSAP.Item(2, 1).Value = 21
        dgvSAP.Rows.Add()
        dgvSAP.Item(0, 2).Value = "Kitchen + 3 Add wet room"
        dgvSAP.Item(1, 2).Value = 27
        dgvSAP.Item(2, 2).Value = 27
        dgvSAP.Rows.Add()
        dgvSAP.Item(0, 3).Value = "Kitchen + 4 Add wet room"
        dgvSAP.Item(1, 3).Value = 33
        dgvSAP.Item(2, 3).Value = 33
        dgvSAP.Rows.Add()
        dgvSAP.Item(0, 4).Value = "Kitchen + 5 Add wet room"
        dgvSAP.Item(1, 4).Value = 39
        dgvSAP.Item(2, 4).Value = 39
        dgvSAP.Rows.Add()
        dgvSAP.Item(0, 5).Value = "Kitchen + 6 Add wet room"
        dgvSAP.Item(1, 5).Value = 45
        dgvSAP.Item(2, 5).Value = 45
        dgvSAP.Rows.Add()
        dgvSAP.Item(0, 6).Value = "Kitchen + 7 Add wet room"
        dgvSAP.Item(1, 6).Value = 51
        dgvSAP.Item(2, 6).Value = 51
    End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUpdateSAPTable.Click
        sap_table_fill()
    End Sub

#End Region

#Region "====[ Language (Localization) ]===="

    Private Sub Environment_LanguageChanged(ByVal sender As Object, ByVal e As EventArgs)

        tsmiOption_Language_DE.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_DE, True, False)
        tsmiOption_Language_EN.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_EN, True, False)
        tsmiOption_Language_FR.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_FR, True, False)
        tsmiOption_Language_IT.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_IT, True, False)
        tsmiOption_Language_NL.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_NL, True, False)
        tsmiOption_Language_PL.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_PL, True, False)
        tsmiOption_Language_SL.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_SL, True, False)
        tsmiOption_Language_BG.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_BG, True, False)
        tsmiOption_Language_RO.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_RO, True, False)
        tsmiOption_Language_HU.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_HU, True, False)
        tsmiOption_Language_DA.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_DA, True, False)
        tsmiOption_Language_SV.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_SV, True, False)
        tsmiOption_Language_NO.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_NO, True, False)
        tsmiOption_Language_IS.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_IS, True, False)
        tsmiOption_Language_CS.Checked = IIf(Environment.PrimaryLanguageCode = CLEnvironment.LanguageCode_CS, True, False)

        UpdateLocalization()
        If m_NextUiReportHost Then Return
        Calculate()
        tsmiFile_SaveCommercialSheet.Enabled = CommercialSheet_CanGenerate(Environment.PrimaryLanguageCode, Environment.Branch.ShortName)

    End Sub

    Public Sub UpdateLocalization()

        'grbPerformance_InputData.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_InputData.ToString())

        grbPerformance_UnitSelection.Text = Environment.Localization.GetString(CLMessageResources.MainForm_UnitSelection.ToString())

        lblPerformance_Unit.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Unit.ToString())

        lblPerformance_MaxPressure.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_MaxPressure.ToString()),
         "Pa")

        grbPerformance_TemperatureConditions.Text = Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString())

        lblPerformance_FreshInletTemperature.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_FreshInletTemperature.ToString()),
         "°C")

        lblPerformance_RHFreshInlet.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_RHFreshInletTemperature.ToString()),
         "%")

        lblPerformance_ReturnInletTemperature.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_ReturnInletTemperature.ToString()),
         "°C")

        lblPerformance_RHReturnInlet.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_RHReturnInletTemperature.ToString()),
         "%")

        tbpData_Thermal.Text = String.Format("{0} / {1}",
            Environment.Localization.GetString(CLMessageResources.MainForm_HeatExchangerPerformances.ToString()),
            Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString()))

        grbPerformance_HeatExchangerPerformances.Text = Environment.Localization.GetString(CLMessageResources.MainForm_HeatExchangerPerformances.ToString())

        lblPerformance_HeatTransferred.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_HeatTransferred.ToString()),
         "W")

        lblPerformance_SensibleHeat.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_SensibleHeat.ToString()),
         "W")

        lblPerformance_LatentHeat.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_LatentHeat.ToString()),
         "W")

        txbPerformance_Efficiency.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_Efficiency.ToString()),
         "%")


        If cmbPerformance_Series.SelectedItem = "6" Then

            lblPerformance_WaterProduced.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_MoistureRecovery.ToString()),
         "l/h")
        Else
            lblPerformance_WaterProduced.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_WaterProduced.ToString()),
         "l/h")
        End If

        GroupBox4.Text = grbPerformance_HeatExchangerPerformances.Text
        Label12.Text = lblPerformance_HeatTransferred.Text
        Label11.Text = lblPerformance_SensibleHeat.Text
        Label9.Text = lblPerformance_LatentHeat.Text
        Label13.Text = txbPerformance_Efficiency.Text
        Label10.Text = lblPerformance_WaterProduced.Text


        grbPerformance_TemperatureConditions2.Text = Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString())

        lblPerformance_SupplyOutletTemperature.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_SupplyOutletTemperature.ToString()),
         "°C")
        lblPerformance_SupplyOutletRH.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_RHSupplyOutletTemperature.ToString()),
         "%")

        lblPerformance_ExhaustOutletTemperature.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_ExhaustOutletTemperature.ToString()),
         "°C")
        lblPerformance_ExhaustOutletRH.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_RHExhaustOutletTemperature.ToString()),
         "%")

        GroupBox5.Text = grbPerformance_TemperatureConditions2.Text
        Label15.Text = lblPerformance_SupplyOutletTemperature.Text
        Label23.Text = lblPerformance_SupplyOutletRH.Text
        Label24.Text = lblPerformance_ExhaustOutletTemperature.Text
        Label14.Text = lblPerformance_ExhaustOutletRH.Text

        GroupBox6.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Winter.ToString())
        GroupBox7.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Summer.ToString())

        lblPerformance_RegulationLevel.Text = Environment.Localization.GetString(CLMessageResources.MainForm_RegulationLevel.ToString())

        tbpData_ElectricalPerformances.Text = Environment.Localization.GetString(CLMessageResources.MainForm_ElectricalPerformances.ToString())

        btn_summer.Text = Environment.Localization.GetString(CLMessageResources.MainForm_SummerEnable.ToString())
        btn_winter.Text = Environment.Localization.GetString(CLMessageResources.MainForm_WinterSummer.ToString())
        SeasonalCalculation_UpdateModeButton()



        lblPerformance_ElectricalPerformances_PowerInput.Text = String.Format("{0} [{1}] ({2})",
           Environment.Localization.GetString(CLMessageResources.MainForm_PowerInput.ToString()),
           "W",
           Environment.Localization.GetString(CLMessageResources.MainForm_SingleBranch.ToString()))

        grbPerformance_PassiveHaus.Text = Environment.Localization.GetString(CLMessageResources.MainForm_PassiveHouse.ToString())
        lblPerformance_PassiveHaus_ElectricalEfficiency.Text = String.Format("{0} [{1}]",
         Environment.Localization.GetString(CLMessageResources.MainForm_PassiveHouseElectricalEfficienty.ToString()),
         "W/(m3/h)")

        chbPerformance_SFP_ShowArea.Text = Environment.Localization.GetString(CLMessageResources.MainForm_ShowSFPArea.ToString())
        chbPerformance_PassiveHaus_ShowArea.Text = Environment.Localization.GetString(CLMessageResources.MainForm_ShowPassiveHouseArea.ToString())
        chbPerformance_SEL_ShowArea.Text = Environment.Localization.GetString(CLMessageResources.MainForm_ShowSELArea.ToString())
        chbPerformance_ERP2018_ShowArea.Text = Environment.Localization.GetString(CLMessageResources.MainForm_ShowERPArea.ToString())

        tbpCertification.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Certification.ToString())
        tbpPerformance.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Performance.ToString())

        tsmiFile.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_File.ToString())
        tsmiFile_GenerateReport.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_File_GenerateReport.ToString())
        tsmiFile_Exit.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_File_Exit.ToString())
        Project_UpdateLocalizedTexts()

        tsmiOption.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option.ToString())
        'tsmiOption_SelectMode.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_Menu_Option_SelectMode.ToString())
        'tsmiOption_SelectMode_Checking.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_Menu_Option_SelectMode_Checking.ToString())
        'tsmiOption_SelectMode_Design.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_Menu_Option_SelectMode_Design.ToString())

        tsmiOption_Language.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language.ToString())
        ' Language names are autonyms so users can always identify their language.
        tsmiOption_Language_DE.Text = "Deutsch"
        tsmiOption_Language_NL.Text = "Nederlands"
        tsmiOption_Language_EN.Text = "English"
        tsmiOption_Language_FR.Text = "Français"
        tsmiOption_Language_IT.Text = "Italiano"
        tsmiOption_Language_PL.Text = "Polski"
        tsmiOption_Language_SL.Text = "Slovenščina"
        tsmiOption_Language_BG.Text = "Български"
        tsmiOption_Language_RO.Text = "Română"
        tsmiOption_Language_HU.Text = "Magyar"
        tsmiOption_Language_DA.Text = "Dansk"
        tsmiOption_Language_SV.Text = "Svenska"
        tsmiOption_Language_NO.Text = "Norsk"
        tsmiOption_Language_IS.Text = "Íslenska"
        tsmiOption_Language_CS.Text = "Čeština"

        tsmiOption_Unit.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Unit.ToString())
        tsmiOption_Unit_IP.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Unit_IP.ToString())
        tsmiOption_Unit_SI.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Unit_SI.ToString())
        tsmiOption_CommercialSheetAutoSync.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_CommercialSheetAutoSync.ToString())
        tsmiOption_CheckUpdates.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_CheckUpdates.ToString())

        tsmiAbout.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_About.ToString())

        dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant.HeaderText =
         Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_EnergySavingTrustBestPracticePerformanceCompliant.ToString())

        dgvSAP_ExhaustTerminalConfiguration.HeaderText =
         Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_ExhaustTerminalConfiguration.ToString())

        dgvSAP_HeatExchangeEfficiency.HeaderText =
         Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_HeatExchangeEffiency.ToString()) & " [%]"

        dgvSAP_RegulationLevel.HeaderText = Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_RegulationLevel.ToString()) & " [%]"

        dgvSAP_SpecificFanPower.HeaderText = Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_SpecificFanPower.ToString()) & " [W/l/s]"

        dgvSAP_TotalExhaustFlowRate.HeaderText = Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_TotalExhaustFlowRate.ToString()) & "[l/s]"

        dgvSAP_TotalSupplyFlowRate.HeaderText = Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_TotalSupplyFlowRate.ToString()) & "[l/s]"

        btnUpdateSAPTable.Text = Environment.Localization.GetString(CLMessageResources.MainForm_UpdateSAPTable.ToString())

        Me.Text = Environment.SSWInfo.SelectionSoftwareTitle

        tbpData_SoundPower.Text = Environment.Localization.GetString(CLMessageResources.SoundPower.ToString())
        chbSoundPerformances_addtoreport.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_AddToReport.ToString())
        tsmiFile_SaveCommercialSheet.Text = Environment.Localization.GetString(CLMessageResources.SaveCommercialSheet.ToString())
        tsmiFile_SaveIOM.Text = Environment.Localization.GetString(CLMessageResources.SaveIOM.ToString())

        lblPerformance_Series.Text = Environment.Localization.GetString(CLMessageResources.Series.ToString())

        dgvPerformance_SoundPower.Columns(SoundColumnName_LwA).HeaderText = Environment.Localization.GetString(CLMessageResources.Sound_Total.ToString())

        tbpData_ItemGenerator.Text = Environment.Localization.GetString(CLMessageResources.Item_Code.ToString())

        tbpData_ItemGenerator_QTM.Text = Environment.Localization.GetString(CLMessageResources.Item_Code.ToString())

        'CO2 Level 
        tbpCO2Level.Text = Environment.Localization.GetString(CLMessageResources.CO2Level.ToString())

        grbCO2Level_Room.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Room.ToString())

        lblCO2Level_Room_Height.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Room_Height.ToString()) & " [m]"
        lblCO2Level_Room_Length.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Room_Length.ToString()) & " [m]"
        lblCO2Level_Room_Width.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Room_Witdh.ToString()) & " [m]"

        grbCO2Level_use.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Usage.ToString())

        lblCO2Level_Usage_Activity.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Usage_LevelAct.ToString()) & " [met]"
        lblCO2Level_Usage_CO2prod.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Usage_CO2prod.ToString()) & " [l/(h person)]"
        lblCO2Level_Usage_People.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Usage_People.ToString()) & " [nr]"
        lblCO2Level_Usage_Period.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Usage_Period.ToString()) & " [min]"
        lblCO2Level_Usage_Presence.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Usage_Presence.ToString())
        lblCO2Level_Usage_Break.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Usage_Break.ToString())

        grbCO2Level_Parameters.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters.ToString())

        lblCO2Level_Parameters_af_area.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_Af_area.ToString()) & " [l/(s m2)]"
        lblCO2Level_Parameters_af_person.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_Af_person.ToString()) & " [l/s]"
        lblCO2Level_Parameters_airflow.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_Af_demand.ToString()) & " [l/s]"
        lblCO2Level_Parameters_airflow_design.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_Af_set.ToString()) & " [l/s]"
        lblCO2Level_Parameters_CalcMet.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_CalcMet.ToString())
        lblCO2Level_Parameters_extCO2.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_ExtCO2.ToString()) & " [ppm]"
        lblCO2Level_Parameters_maxCO2.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_MaxCO2.ToString()) & " [ppm]"
        lblCO2Level_Parameters_stdpreset.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset.ToString())
        chbCO2Level_Parameters_airflow_check.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_checkwp.ToString())

        'Tengo aggiornata anche l'ultima scelta
        Dim indice As Integer

        indice = cmbCO2Level_Parameters_CalcMet.SelectedIndex
        cmbCO2Level_Parameters_CalcMet.Items.Clear()
        cmbCO2Level_Parameters_CalcMet.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_CalcMet_MaxCO2.ToString()))
        cmbCO2Level_Parameters_CalcMet.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_CalcMet_Fixed_Af.ToString()))
        cmbCO2Level_Parameters_CalcMet.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_CalcMet_PersonRelated.ToString()))
        cmbCO2Level_Parameters_CalcMet.SelectedIndex = indice

        indice = cmbCO2Level_Parameters_stdpreset.SelectedIndex
        cmbCO2Level_Parameters_stdpreset.Items.Clear()
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_01.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_02.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_03.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_04.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_05.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_06.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_07.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_08.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_09.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_10.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_11.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_12.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_13.ToString()))
        cmbCO2Level_Parameters_stdpreset.Items.Add(Environment.Localization.GetString(CLMessageResources.CO2Level_Parameters_StandardPreset_14.ToString()))
        cmbCO2Level_Parameters_stdpreset.SelectedIndex = indice

        chbCO2Level_addtoreport.Text = Environment.Localization.GetString(CLMessageResources.CO2Level_AddToReport.ToString())
        crtCO2Level_Chart1.ChartAreas(0).AxisX.Title = Environment.Localization.GetString(CLMessageResources.CO2Level_PeriodOfTime.ToString()) & " [h]"
        crtCO2Level_Chart1.ChartAreas(0).AxisY.Title = Environment.Localization.GetString(CLMessageResources.CO2Level.ToString()) & " [ppm]"

        CoilPerformance_UpdateLocalizedTexts()
        ElectricHeater_UpdateLocalizedTexts()
        Accessories_UpdateLocalizedTexts()
        Help_UpdateLocalizedTexts()
        FollowUp_UpdateLocalizedTexts()

        UpdateLocalization_MeasureUnit()
    End Sub

    Private Sub UpdateLocalization_MeasureUnit()
        Select Case m_MeasureUnit
            Case CLModule.CLMeasureUnit.SI
                lblPerformance_AirFlow.Text = String.Format("{0} [{1}]",
                  Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()),
                  "m3/h")

                lblPerformance_SFP_SFP.Text = String.Format("{0} [{1}]",
                 Environment.Localization.GetString(CLMessageResources.MainForm_SFP.ToString()),
                 "kW/(m3/s)")

                lblPerformance_SEL_SEL.Text = String.Format("{0} [{1}]",
                 Environment.Localization.GetString(CLMessageResources.MainForm_SEL.ToString()),
                 "J/m3")


            Case CLModule.CLMeasureUnit.IP
                lblPerformance_AirFlow.Text = String.Format("{0} [{1}]",
                  Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()),
                  "l/s")

                lblPerformance_SFP_SFP.Text = String.Format("{0} [{1}]",
                 Environment.Localization.GetString(CLMessageResources.MainForm_SFP.ToString()),
                 "W/(l/s)")
        End Select
    End Sub

    Private Sub tsmiOption_Language_IT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiOption_Language_IT.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_IT))
    End Sub

    Private Sub tsmiOption_Language_EN_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiOption_Language_EN.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_EN))
    End Sub

    Private Sub tsmiOption_Language_DE_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiOption_Language_DE.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_DE))
    End Sub

    Private Sub tsmiOption_Language_FR_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiOption_Language_FR.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_FR))
    End Sub

    Private Sub tsmiOption_Language_NL_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiOption_Language_NL.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_NL))
    End Sub

    Private Sub tsmiOption_Language_PL_Click(sender As System.Object, e As System.EventArgs) Handles tsmiOption_Language_PL.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_PL))
    End Sub

    Private Sub tsmiOption_Language_SL_Click(sender As System.Object, e As System.EventArgs) Handles tsmiOption_Language_SL.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_SL))
    End Sub

    Private Sub tsmiOption_Language_BG_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_BG.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_BG))
    End Sub

    Private Sub tsmiOption_Language_RO_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_RO.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_RO))
    End Sub

    Private Sub tsmiOption_Language_HU_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_HU.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_HU))
    End Sub

    Private Sub tsmiOption_Language_DA_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_DA.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_DA))
    End Sub

    Private Sub tsmiOption_Language_SV_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_SV.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_SV))
    End Sub

    Private Sub tsmiOption_Language_NO_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_NO.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_NO))
    End Sub

    Private Sub tsmiOption_Language_IS_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_IS.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_IS))
    End Sub

    Private Sub tsmiOption_Language_CS_Click(sender As Object, e As EventArgs) Handles tsmiOption_Language_CS.Click
        Environment.SetLanguage(Environment.FindLanguage(CLEnvironment.LanguageCode_CS))
    End Sub
#End Region

#Region "====[ Measure Unit ]===="

    Private m_MeasureUnit As CLMeasureUnit = CLMeasureUnit.SI
    Public Property MeasureUnit As CLMeasureUnit
        Get
            Return m_MeasureUnit
        End Get
        Set(ByVal value As CLMeasureUnit)
            m_MeasureUnit = value
            tsmiOption_Unit_SI.Checked = IIf(m_MeasureUnit = CLMeasureUnit.SI, True, False)
            tsmiOption_Unit_IP.Checked = IIf(m_MeasureUnit = CLMeasureUnit.IP, True, False)

            UpdateLocalization_MeasureUnit()
            Calculate()
        End Set
    End Property

    Private Sub tsmiOption_Unit_SI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiOption_Unit_SI.Click
        MeasureUnit = CLMeasureUnit.SI
    End Sub

    Private Sub tsmiOption_Unit_IP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiOption_Unit_IP.Click
        MeasureUnit = CLMeasureUnit.IP
    End Sub

#End Region

#Region "====[ IOM ]===="

    Private Function IOM_GetFile(languageCode As String, useEnLanguageWhenNotExist As Boolean, shortname As String) As String

        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel
        Return IOM_GetFileForModel(
            dcHeatRecoveryModel, languageCode, useEnLanguageWhenNotExist, shortname)

    End Function

    Private Shared Function IOM_GetFileForModel(
        dcHeatRecoveryModel As CLDCHeatRecoveryModel,
        languageCode As String,
        useEnLanguageWhenNotExist As Boolean,
        shortname As String) As String

        If dcHeatRecoveryModel Is Nothing OrElse String.IsNullOrEmpty(dcHeatRecoveryModel.PDFInstallationOperationManuals) _
            OrElse Not Directory.Exists(PDFDocumentDirectory) Then
            Return ""
        End If

        Dim cssPdfFiles() As String
        Dim pdfFiles As New List(Of String)

        cssPdfFiles = Directory.GetFiles(PDFDocumentDirectory, dcHeatRecoveryModel.PDFInstallationOperationManuals.Replace("%LanguageCode%", languageCode).Replace("%ShortName%", shortname))

        For Each pdfFile As String In cssPdfFiles
            pdfFiles.Add(pdfFile)
        Next

        If pdfFiles.Count = 0 AndAlso useEnLanguageWhenNotExist AndAlso languageCode <> "EN" Then
            Return IOM_GetFileForModel(
                dcHeatRecoveryModel, "EN", False, shortname)
        End If

        If pdfFiles.Count = 0 Then
            Return ""
        End If
        Return pdfFiles(0)

    End Function

    Private Function IOM_CanGenerate(languageCode As String, shortname As String) As Boolean
        Return Not String.IsNullOrEmpty(IOM_GetFile(languageCode, True, shortname))
    End Function

    Private Function IOM_Generate(filePath As String) As Boolean

        Dim pdfFile As String

        pdfFile = IOM_GetFile(Environment.PrimaryLanguageCode, True, Environment.Branch.ShortName)
        If String.IsNullOrEmpty(pdfFile) Then
            Return False
        End If

        Try
            File.Copy(pdfFile, filePath, True)
        Catch ex As Exception
            Return False
        End Try

        Process.Start(filePath)

        Return True

    End Function

    Private Sub tsmiFile_SaveIOM_Click(sender As Object, e As EventArgs) Handles tsmiFile_SaveIOM.Click

        sfdSavePdf.FileName = String.Format("{0}-{1}",
            SelectedHeatRecoveryModelCustomerName,
            Environment.Localization.GetString(CLMessageResources.IOM.ToString()))

        If sfdSavePdf.ShowDialog() = DialogResult.OK Then

            Try
                IOM_Generate(sfdSavePdf.FileName)

                MessageBox.Show(Me,
                 Environment.Localization.GetString(CLMessageResources.MainForm_FileSaved.ToString()),
                 "",
                 MessageBoxButtons.OK,
                 MessageBoxIcon.Information)

            Catch exception As Exception
                MessageBox.Show(Me, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try

        End If

    End Sub

#End Region

#Region "====[ Commercial Sheet ]===="

    Private Sub tsmiFile_SaveCommercialSheet_Click(sender As System.Object, e As System.EventArgs) Handles tsmiFile_SaveCommercialSheet.Click
        sfdSavePdf.FileName = String.Format("{0}-{1}",
            SelectedHeatRecoveryModelCustomerName,
            Environment.Localization.GetString(CLMessageResources.CommercialSheet.ToString()))
        If sfdSavePdf.ShowDialog() = DialogResult.OK Then

            Try
                CommercialSheet_Generate(sfdSavePdf.FileName)

                MessageBox.Show(Me,
                 Environment.Localization.GetString(CLMessageResources.MainForm_FileSaved.ToString()),
                 "",
                 MessageBoxButtons.OK,
                 MessageBoxIcon.Information)

            Catch exception As Exception
                MessageBox.Show(Me, exception.Message, Environment.Localization.GetString(CLMessageResources.MainForm_CantSaveFile.ToString()), MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try

        End If
    End Sub

    Private Function CommercialSheet_CanGenerate(languageCode As String, shortname As String) As Boolean
        Return Not String.IsNullOrEmpty(CommercialSheet_GetFiles(languageCode, True, shortname))
    End Function

    Private Shared Sub CommercialSheet_Log(message As String)
        Try
            Dim line As String = String.Format("{0:yyyy-MM-dd HH:mm:ss.fff} | {1}", DateTime.Now, message)
            Debug.WriteLine(line)

            Dim logPath As String = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "commercialsheet_lookup.log")
            File.AppendAllText(logPath, line & System.Environment.NewLine)
        Catch
        End Try
    End Sub

    Private Function CommercialSheet_GetFiles(languageCode As String, useEnLanguageWhenNotExist As Boolean, shortname As String) As String

        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel
        Return CommercialSheet_GetFilesForModel(
            dcHeatRecoveryModel,
            SelectedHeatRecoveryModelCustomerName,
            languageCode,
            useEnLanguageWhenNotExist,
            shortname,
            tsmiOption_CommercialSheetAutoSync.Checked)

    End Function

    Private Shared Function CommercialSheet_GetFilesForModel(
        dcHeatRecoveryModel As CLDCHeatRecoveryModel,
        selectedModelName As String,
        languageCode As String,
        useEnLanguageWhenNotExist As Boolean,
        shortname As String,
        autoSyncEnabled As Boolean) As String

        If dcHeatRecoveryModel Is Nothing Then
            CommercialSheet_Log("Lookup skipped: model is Nothing.")
            Return ""
        End If

        Dim serieCode As String = ""
        Dim serieName As String = ""
        Dim modelName As String = ""
        Dim serieDirectory As String
        Dim languageDirectory As String
        Dim pdfFile As String
        Dim normalizedLanguageCode As String = languageCode

        Try
            serieCode = CStr(CallByName(dcHeatRecoveryModel.CLSerie, "Code", CallType.Get))
        Catch
            serieCode = ""
        End Try

        Try
            serieName = CStr(CallByName(dcHeatRecoveryModel.CLSerie, "Name", CallType.Get))
        Catch
            serieName = ""
        End Try

        modelName = selectedModelName
        If String.IsNullOrWhiteSpace(modelName) Then
            Try
                modelName = CStr(CallByName(dcHeatRecoveryModel, "Name", CallType.Get))
            Catch
                modelName = ""
            End Try
        End If

        If String.IsNullOrWhiteSpace(serieCode) AndAlso String.IsNullOrWhiteSpace(serieName) _
            OrElse String.IsNullOrWhiteSpace(modelName) Then
            CommercialSheet_Log(String.Format("Lookup skipped: serieCode='{0}', serieName='{1}', modelName='{2}'", serieCode, serieName, modelName))
            Return ""
        End If

        If Not String.IsNullOrWhiteSpace(serieCode) Then
            serieDirectory = Path.Combine(PDFDocumentDirectory, CommercialSheet_GetSerieFolderName(serieCode))
        Else
            serieDirectory = Path.Combine(PDFDocumentDirectory, "S" & serieName.Trim())
        End If

        If Not String.IsNullOrWhiteSpace(normalizedLanguageCode) Then
            normalizedLanguageCode = normalizedLanguageCode.Trim().ToUpperInvariant()
            If normalizedLanguageCode.Contains("-"c) Then
                normalizedLanguageCode = normalizedLanguageCode.Split("-"c)(0)
            End If
        End If

        languageDirectory = Path.Combine(serieDirectory, normalizedLanguageCode)

        If Not String.IsNullOrWhiteSpace(serieCode) Then
            Dim expectedFileName As String = CommercialSheet_BuildExpectedFileName(modelName, normalizedLanguageCode, shortname)
            If Not String.IsNullOrEmpty(expectedFileName) Then
                If String.Equals(shortname, "AV", StringComparison.OrdinalIgnoreCase) AndAlso autoSyncEnabled Then
                    pdfFile = CommercialSheet_GetOnlineFile(serieCode.Trim(), normalizedLanguageCode, expectedFileName, shortname)
                    If Not String.IsNullOrEmpty(pdfFile) Then
                        CommercialSheet_Log("FOUND ONLINE (preferred for AV): " & pdfFile)
                        Return pdfFile
                    End If
                End If
            End If
        End If

        pdfFile = CommercialSheet_BuildAndFindFile(languageDirectory, modelName, normalizedLanguageCode, shortname)
        If Not String.IsNullOrEmpty(pdfFile) Then
            CommercialSheet_Log("FOUND LOCAL: " & pdfFile)
            Return pdfFile
        End If

        If useEnLanguageWhenNotExist AndAlso normalizedLanguageCode <> "EN" Then
            CommercialSheet_Log(String.Format("Fallback to EN from language '{0}'", normalizedLanguageCode))
            Return CommercialSheet_GetFilesForModel(
                dcHeatRecoveryModel, modelName, "EN", False, shortname,
                autoSyncEnabled)
        End If

        CommercialSheet_Log("NOT FOUND for current lookup.")
        Return ""

    End Function

    Private Sub tsmiOption_CommercialSheetAutoSync_Click(sender As Object, e As EventArgs) Handles tsmiOption_CommercialSheetAutoSync.Click
        Try
            My.Settings.CommercialSheetAutoSyncEnabled = tsmiOption_CommercialSheetAutoSync.Checked
            My.Settings.Save()
        Catch
        End Try

        tsmiFile_SaveCommercialSheet.Enabled = CommercialSheet_CanGenerate(Environment.PrimaryLanguageCode, Environment.Branch.ShortName)
    End Sub

    Private Shared Function CommercialSheet_GetSerieFolderName(serieCode As String) As String

        Dim normalizedSerieCode As String = If(serieCode, "").Trim().ToUpperInvariant()
        If normalizedSerieCode = "32" Then
            Return "SA"
        End If

        Return "S" & normalizedSerieCode

    End Function

    Private Shared Function CommercialSheet_BuildExpectedFileName(modelName As String, languageCode As String, shortname As String) As String

        Dim cleanedModelName As String = modelName.Replace(ChrW(160), " "c)
        Dim tokens() As String = cleanedModelName.Split(New Char() {" "c, ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)
        If tokens.Length >= 3 Then
            Return String.Format("{0}_{1}_{2}_{3}.pdf",
                tokens(2).ToUpperInvariant(),
                tokens(1).ToUpperInvariant(),
                languageCode.ToUpperInvariant(),
                shortname.ToUpperInvariant())
        End If

        If tokens.Length = 2 Then
            Return String.Format("{0}_{1}_{2}.pdf",
                tokens(1).ToUpperInvariant(),
                languageCode.ToUpperInvariant(),
                shortname.ToUpperInvariant())
        End If

        If tokens.Length < 2 Then
            CommercialSheet_Log(String.Format("Model name tokens < 2: modelName='{0}'", modelName))
            Return ""
        End If

        Return ""

    End Function

    Private Shared Function CommercialSheet_BuildAndFindFile(directoryPath As String, modelName As String, languageCode As String, shortname As String) As String

        If String.IsNullOrWhiteSpace(directoryPath) OrElse String.IsNullOrWhiteSpace(modelName) _
            OrElse Not Directory.Exists(directoryPath) Then
            CommercialSheet_Log(String.Format("Directory missing or invalid: '{0}'", directoryPath))
            Return ""
        End If

        Dim fileName As String = CommercialSheet_BuildExpectedFileName(modelName, languageCode, shortname)
        If String.IsNullOrEmpty(fileName) Then
            Return ""
        End If

        Dim fullPath As String = Path.Combine(directoryPath, fileName)
        CommercialSheet_Log(String.Format("Trying: model='{0}', serieDir='{1}', language='{2}', shortname='{3}', fullPath='{4}'",
            modelName,
            directoryPath,
            languageCode,
            shortname,
            fullPath))

        If File.Exists(fullPath) Then
            Return fullPath
        End If

        Return ""

    End Function

    Private Shared Function CommercialSheet_GetOnlineFile(serieCode As String, languageCode As String, fileName As String, shortname As String) As String

        Dim baseUrl As String = ""

        If String.Equals(shortname, "AV", StringComparison.OrdinalIgnoreCase) Then
            baseUrl = "https://www.avensys-srl.com/ftproot/DOCUMENTS/Commercial_leaflets/1_VENTILATION_HEAT_RECOVERY/1_Heat_recovery_units/LEAFLETS"
        End If

        If String.IsNullOrWhiteSpace(baseUrl) Then
            CommercialSheet_Log(String.Format("No online base URL configured for shortname '{0}'", shortname))
            Return ""
        End If

        Dim serieFolder As String = CommercialSheet_GetSerieFolderName(serieCode)
        Dim langFolder As String = languageCode.Trim().ToUpperInvariant()
        Dim url As String = String.Format("{0}/{1}/{2}/{3}", baseUrl.TrimEnd("/"c), serieFolder, langFolder, fileName)

        Dim targetDirectory As String = Path.Combine(PDFDocumentDirectory, serieFolder, langFolder)
        Dim targetFilePath As String = Path.Combine(targetDirectory, fileName)
        Dim temporaryFilePath As String = targetFilePath & ".download"

        Try
            Directory.CreateDirectory(targetDirectory)

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls
            Dim request = DirectCast(WebRequest.Create(url), HttpWebRequest)
            request.Method = "GET"
            request.Timeout = 10000
            request.ReadWriteTimeout = 20000
            If File.Exists(targetFilePath) Then
                request.IfModifiedSince = File.GetLastWriteTimeUtc(targetFilePath)
            End If

            CommercialSheet_Log("Trying conditional online URL: " & url)
            Using response = DirectCast(request.GetResponse(), HttpWebResponse)
                Using source = response.GetResponseStream()
                    Using destination = File.Create(temporaryFilePath)
                        source.CopyTo(destination)
                    End Using
                End Using
            End Using
            File.Copy(temporaryFilePath, targetFilePath, True)

            CommercialSheet_Log("Downloaded/updated local css file: " & targetFilePath)
            Return targetFilePath
        Catch ex As WebException
            Dim response = TryCast(ex.Response, HttpWebResponse)
            If response IsNot Nothing AndAlso
                response.StatusCode = HttpStatusCode.NotModified AndAlso
                File.Exists(targetFilePath) Then
                CommercialSheet_Log("Online file unchanged, using local css file: " & targetFilePath)
                Return targetFilePath
            End If

            CommercialSheet_Log(String.Format("Online download failed. URL='{0}' Error='{1}'", url, ex.Message))
            If File.Exists(targetFilePath) Then
                CommercialSheet_Log("Online failed, using existing local css file: " & targetFilePath)
                Return targetFilePath
            End If
            Return ""
        Catch ex As Exception
            CommercialSheet_Log(String.Format("Online download failed. URL='{0}' Error='{1}'", url, ex.Message))
            If File.Exists(targetFilePath) Then
                CommercialSheet_Log("Online failed, using existing local css file: " & targetFilePath)
                Return targetFilePath
            End If
            Return ""
        Finally
            Try
                If File.Exists(temporaryFilePath) Then File.Delete(temporaryFilePath)
            Catch
            End Try
        End Try

    End Function

    Private Function CommercialSheet_Generate(pdfCommercialBrochureFilePath As String) As Boolean


        Dim pdfFile As String

        pdfFile = CommercialSheet_GetFiles(Environment.PrimaryLanguageCode, True, Environment.Branch.ShortName)
        If String.IsNullOrEmpty(pdfFile) Then
            Return False
        End If

        Try
            File.Copy(pdfFile, pdfCommercialBrochureFilePath, True)
        Catch ex As Exception
            Return False
        End Try

        Process.Start(pdfCommercialBrochureFilePath)

        Return True


    End Function

    Private Shared ReadOnly Property PDFDocumentDirectory() As String
        Get
            Return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "css")
        End Get
    End Property

#End Region

#Region "====[ Properties ]===="

    Public ReadOnly Property AirFlow As Double
        Get
            Return ParseUIDouble(txbPerformance_AirFlow.Text)
        End Get
    End Property

    Public ReadOnly Property SupplyOutletTemp As Double
        Get
            Dim value As Double
            Return IIf(Double.TryParse(txbPerformance_SupplyOutletTemperature.Text, value), value, 0.0)
        End Get
    End Property

    Public ReadOnly Property SupplyOutletRH As Double
        Get
            Dim value As Double
            Return IIf(Double.TryParse(txbPerformance_SupplyOutletRH.Text, value), value, 0.0)
        End Get
    End Property

#End Region

#Region "====[ Coil Performance ]===="

    Private Sub CoilPerformance_InitializeTab()
        If tbpData_CoilPerformance IsNot Nothing Then
            Return
        End If

        tbpData_CoilPerformance = New TabPage(CoilPerformance_Text("MainForm_CoilPerformance_Tab", "Water coils"))
        tbpData_CoilPerformance.AutoScroll = True
        tbpData_CoilPerformance.UseVisualStyleBackColor = True

        Dim grbEnable As New GroupBox()
        grbEnable.Tag = New String() {"MainForm_CoilPerformance_EnableGroup", "Water coil calculation"}
        grbEnable.Text = CoilPerformance_Text("MainForm_CoilPerformance_EnableGroup", "Water coil calculation")
        grbEnable.Location = New Point(8, 8)
        grbEnable.Size = New Size(252, 56)

        chbCoilPerformance_Enable = New CheckBox()
        chbCoilPerformance_Enable.Tag = New String() {"MainForm_CoilPerformance_Enable", "Enable coil calculation"}
        chbCoilPerformance_Enable.Text = CoilPerformance_Text("MainForm_CoilPerformance_Enable", "Enable coil calculation")
        chbCoilPerformance_Enable.AutoSize = True
        chbCoilPerformance_Enable.Checked = False
        chbCoilPerformance_Enable.Location = New Point(12, 24)
        AddHandler chbCoilPerformance_Enable.CheckedChanged, AddressOf CoilPerformance_EnableChanged
        grbEnable.Controls.Add(chbCoilPerformance_Enable)

        Dim grbSelection As New GroupBox()
        grbSelection.Tag = New String() {"MainForm_CoilPerformance_Selection", "Coil selection"}
        grbSelection.Text = CoilPerformance_Text("MainForm_CoilPerformance_Selection", "Coil selection")
        grbSelection.Location = New Point(8, 72)
        grbSelection.Size = New Size(360, 148)

        cmbCoilPerformance_EditMode = New ComboBox()
        cmbCoilPerformance_EditMode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCoilPerformance_EditMode.Location = New Point(132, 22)
        cmbCoilPerformance_EditMode.Size = New Size(210, 21)
        CoilPerformance_FillEditModes()
        AddHandler cmbCoilPerformance_EditMode.SelectedIndexChanged, AddressOf CoilPerformance_EditModeChanged

        cmbCoilPerformance_Installation = New ComboBox()
        cmbCoilPerformance_Installation.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCoilPerformance_Installation.Location = New Point(132, 50)
        cmbCoilPerformance_Installation.Size = New Size(210, 21)
        AddHandler cmbCoilPerformance_Installation.SelectedIndexChanged, AddressOf CoilPerformance_InstallationChanged

        cmbCoilPerformance_Mode = New ComboBox()
        cmbCoilPerformance_Mode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCoilPerformance_Mode.Items.Add(CLCoilPerformanceMode.CWD)
        cmbCoilPerformance_Mode.Items.Add(CLCoilPerformanceMode.HWD)
        cmbCoilPerformance_Mode.Items.Add(CLCoilPerformanceMode.HCD)
        cmbCoilPerformance_Mode.SelectedItem = CLCoilPerformanceMode.HCD
        cmbCoilPerformance_Mode.Location = New Point(132, 78)
        cmbCoilPerformance_Mode.Size = New Size(210, 21)
        AddHandler cmbCoilPerformance_Mode.SelectedIndexChanged, AddressOf CoilPerformance_ModeChanged

        cmbCoilPerformance_Coil = New ComboBox()
        cmbCoilPerformance_Coil.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCoilPerformance_Coil.Location = New Point(132, 106)
        cmbCoilPerformance_Coil.Size = New Size(210, 21)
        AddHandler cmbCoilPerformance_Coil.SelectedIndexChanged, AddressOf CoilPerformance_CoilChanged

        grbSelection.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Case", "Case", 12, 25))
        grbSelection.Controls.Add(cmbCoilPerformance_EditMode)
        grbSelection.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Installation", "Installation", 12, 53))
        grbSelection.Controls.Add(cmbCoilPerformance_Installation)
        grbSelection.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Type", "Type", 12, 81))
        grbSelection.Controls.Add(cmbCoilPerformance_Mode)
        grbSelection.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Coil", "Coil", 12, 109))
        grbSelection.Controls.Add(cmbCoilPerformance_Coil)

        Dim grbFluid As New GroupBox()
        grbFluid.Tag = New String() {"MainForm_CoilPerformance_Fluid", "Fluid and water temperatures"}
        grbFluid.Text = CoilPerformance_Text("MainForm_CoilPerformance_Fluid", "Fluid and water temperatures")
        grbFluid.Location = New Point(376, 8)
        grbFluid.Size = New Size(344, 212)

        cmbCoilPerformance_FluidType = New ComboBox()
        cmbCoilPerformance_FluidType.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCoilPerformance_FluidType.Location = New Point(144, 24)
        cmbCoilPerformance_FluidType.Size = New Size(178, 21)
        AddHandler cmbCoilPerformance_FluidType.SelectedIndexChanged, AddressOf CoilPerformance_FluidTypeChanged
        CoilPerformance_FillFluidTypes()

        nudCoilPerformance_FluidTec = CreateCoilNumeric(144, 52, 0, 70, 10, 1)
        nudCoilPerformance_CoolingIn = CreateCoilNumeric(144, 88, -50, 100, 7, 1)
        nudCoilPerformance_CoolingOut = CreateCoilNumeric(144, 116, -50, 100, 12, 1)
        nudCoilPerformance_HeatingIn = CreateCoilNumeric(144, 152, -50, 150, 80, 1)
        nudCoilPerformance_HeatingOut = CreateCoilNumeric(144, 180, -50, 150, 70, 1)
        CoilPerformance_UpdateWaterTemperatureLimits()

        grbFluid.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_FluidType", "Fluid", 12, 27))
        grbFluid.Controls.Add(cmbCoilPerformance_FluidType)
        grbFluid.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Glycol", "Glycol [%]", 12, 55))
        grbFluid.Controls.Add(nudCoilPerformance_FluidTec)
        grbFluid.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_CoolingIn", "Cooling in [C]", 12, 91))
        grbFluid.Controls.Add(nudCoilPerformance_CoolingIn)
        grbFluid.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_CoolingOut", "Cooling out [C]", 12, 119))
        grbFluid.Controls.Add(nudCoilPerformance_CoolingOut)
        grbFluid.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_HeatingIn", "Heating in [C]", 12, 155))
        grbFluid.Controls.Add(nudCoilPerformance_HeatingIn)
        grbFluid.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_HeatingOut", "Heating out [C]", 12, 183))
        grbFluid.Controls.Add(nudCoilPerformance_HeatingOut)

        Dim grbCustomization As New GroupBox()
        grbCustomization.Tag = New String() {"MainForm_CoilPerformance_Geometry", "Geometry"}
        grbCustomization.Text = CoilPerformance_Text("MainForm_CoilPerformance_Geometry", "Geometry")
        grbCustomization.Location = New Point(728, 8)
        grbCustomization.Size = New Size(356, 230)

        nudCoilPerformance_Length = CreateCoilNumeric(150, 24, 1, 5000, 250, 0)
        nudCoilPerformance_Height = CreateCoilNumeric(150, 52, 1, 5000, 150, 0)
        nudCoilPerformance_Tubes = CreateCoilNumeric(244, 52, 1, 200, 6, 0)
        cmbCoilPerformance_HeightMode = New ComboBox()
        cmbCoilPerformance_HeightMode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCoilPerformance_HeightMode.Items.Add(New CLComboBoxItemWrapper(Of String)(CoilPerformance_Text("MainForm_CoilPerformance_HeightMm", "mm"), "mm"))
        cmbCoilPerformance_HeightMode.Items.Add(New CLComboBoxItemWrapper(Of String)(CoilPerformance_Text("MainForm_CoilPerformance_HeightTubes", "tubes"), "tubes"))
        cmbCoilPerformance_HeightMode.SelectedIndex = 0
        cmbCoilPerformance_HeightMode.Location = New Point(244, 24)
        cmbCoilPerformance_HeightMode.Size = New Size(76, 21)
        AddHandler cmbCoilPerformance_HeightMode.SelectedIndexChanged, AddressOf CoilPerformance_InputChanged

        nudCoilPerformance_Rows = CreateCoilNumeric(150, 88, 1, 20, 3, 0)
        cmbCoilPerformance_FinSpacing = New ComboBox()
        cmbCoilPerformance_FinSpacing.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCoilPerformance_FinSpacing.Location = New Point(150, 116)
        cmbCoilPerformance_FinSpacing.Size = New Size(74, 21)
        AddHandler cmbCoilPerformance_FinSpacing.SelectedIndexChanged, AddressOf CoilPerformance_InputChanged
        CoilPerformance_FillFinSpacings()
        nudCoilPerformance_Circuits = CreateCoilNumeric(150, 144, 1, 100, 2, 0)

        grbCustomization.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Length", "Length [mm]", 12, 27))
        grbCustomization.Controls.Add(nudCoilPerformance_Length)
        grbCustomization.Controls.Add(cmbCoilPerformance_HeightMode)
        grbCustomization.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Height", "Height [mm/tubes]", 12, 55))
        grbCustomization.Controls.Add(nudCoilPerformance_Height)
        grbCustomization.Controls.Add(nudCoilPerformance_Tubes)
        grbCustomization.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Rows", "Rows", 12, 91))
        grbCustomization.Controls.Add(nudCoilPerformance_Rows)
        grbCustomization.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_FinSpacing", "Fin spacing [mm]", 12, 119))
        grbCustomization.Controls.Add(cmbCoilPerformance_FinSpacing)
        grbCustomization.Controls.Add(CreateCoilLabel("MainForm_CoilPerformance_Circuits", "Circuits", 12, 147))
        grbCustomization.Controls.Add(nudCoilPerformance_Circuits)

        lblCoilPerformance_DimensionsNote = New Label()
        lblCoilPerformance_DimensionsNote.Tag = New String() {"MainForm_CoilPerformance_DimensionsNote", "The dimensions above refer to the water coil only."}
        lblCoilPerformance_DimensionsNote.Text = CoilPerformance_Text("MainForm_CoilPerformance_DimensionsNote", "The dimensions above refer to the water coil only.")
        lblCoilPerformance_DimensionsNote.Location = New Point(12, 166)
        lblCoilPerformance_DimensionsNote.Size = New Size(330, 28)
        lblCoilPerformance_DimensionsNote.Font = New System.Drawing.Font(lblCoilPerformance_DimensionsNote.Font, FontStyle.Bold)
        lblCoilPerformance_DimensionsNote.ForeColor = Color.Firebrick
        lblCoilPerformance_DimensionsNote.Visible = False
        grbCustomization.Controls.Add(lblCoilPerformance_DimensionsNote)

        lblCoilPerformance_CustomWarning = New Label()
        lblCoilPerformance_CustomWarning.Tag = New String() {"MainForm_CoilPerformance_CustomWarning", "Please ask for overall dimensions, delivery time and quotation"}
        lblCoilPerformance_CustomWarning.Text = CoilPerformance_Text("MainForm_CoilPerformance_CustomWarning", "Please ask for overall dimensions, delivery time and quotation")
        lblCoilPerformance_CustomWarning.Location = New Point(12, 194)
        lblCoilPerformance_CustomWarning.Size = New Size(330, 30)
        lblCoilPerformance_CustomWarning.Font = New System.Drawing.Font(lblCoilPerformance_CustomWarning.Font, FontStyle.Bold)
        lblCoilPerformance_CustomWarning.ForeColor = Color.Firebrick
        lblCoilPerformance_CustomWarning.Visible = False
        grbCustomization.Controls.Add(lblCoilPerformance_CustomWarning)

        dgvCoilPerformance_Results = New DataGridView()
        dgvCoilPerformance_Results.AllowUserToAddRows = False
        dgvCoilPerformance_Results.AllowUserToDeleteRows = False
        dgvCoilPerformance_Results.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        dgvCoilPerformance_Results.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvCoilPerformance_Results.Location = New Point(8, 246)
        dgvCoilPerformance_Results.ReadOnly = True
        dgvCoilPerformance_Results.RowHeadersVisible = False
        dgvCoilPerformance_Results.Size = New Size(1068, 92)
        AddCoilGridColumn("Mode", "MainForm_CoilPerformance_ResultMode", "Mode")
        AddCoilGridColumn("Status", "MainForm_CoilPerformance_ResultStatus", "Status")
        AddCoilGridColumn("Capacity", "MainForm_CoilPerformance_ResultCapacity", "Capacity [W]")
        AddCoilGridColumn("Sensible", "MainForm_CoilPerformance_ResultSensible", "Sensible [W]")
        AddCoilGridColumn("TempOut", "MainForm_CoilPerformance_ResultAirOut", "Max. outlet air temp. [C]")
        AddCoilGridColumn("RHOut", "MainForm_CoilPerformance_ResultRHOut", "R.H. out [%]")
        AddCoilGridColumn("Cond", "MainForm_CoilPerformance_ResultCond", "Cond. [l/h]")
        AddCoilGridColumn("DP", "MainForm_CoilPerformance_ResultDP", "DP [Pa]")
        AddCoilGridColumn("WaterDP", "MainForm_CoilPerformance_ResultWaterDP", "Water DP [kPa]")
        AddCoilGridColumn("FluidFlow", "MainForm_CoilPerformance_ResultFluidFlow", "Fluid flow [l/h]")
        AddCoilGridColumn("FluidSpeed", "MainForm_CoilPerformance_ResultFluidSpeed", "Fluid speed [m/s]")
        AddCoilGridColumn("Face", "MainForm_CoilPerformance_ResultFace", "Face vel. [m/s]")
        dgvCoilPerformance_Results.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        lblCoilPerformance_Status = New Label()
        lblCoilPerformance_Status.AutoSize = False
        lblCoilPerformance_Status.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblCoilPerformance_Status.Location = New Point(8, 346)
        lblCoilPerformance_Status.Size = New Size(1068, 54)
        lblCoilPerformance_Status.ForeColor = Color.Firebrick

        tbpData_CoilPerformance.Controls.Add(grbEnable)
        tbpData_CoilPerformance.Controls.Add(grbSelection)
        tbpData_CoilPerformance.Controls.Add(grbFluid)
        tbpData_CoilPerformance.Controls.Add(grbCustomization)
        tbpData_CoilPerformance.Controls.Add(dgvCoilPerformance_Results)
        tbpData_CoilPerformance.Controls.Add(lblCoilPerformance_Status)

        tbcData.TabPages.Insert(Math.Min(1, tbcData.TabPages.Count), tbpData_CoilPerformance)
        CoilPerformance_UpdateControlState()
    End Sub

    Private Sub AddCoilGridColumn(name As String, resourceName As String, fallbackText As String)
        Dim columnIndex As Integer = dgvCoilPerformance_Results.Columns.Add(name, CoilPerformance_Text(resourceName, fallbackText))
        dgvCoilPerformance_Results.Columns(columnIndex).Tag = New String() {resourceName, fallbackText}
    End Sub

    Private Function CreateCoilLabel(resourceName As String, fallbackText As String, x As Integer, y As Integer) As Label
        Dim label As New Label()
        label.AutoSize = True
        label.Location = New Point(x, y)
        label.Tag = New String() {resourceName, fallbackText}
        label.Text = CoilPerformance_Text(resourceName, fallbackText)
        Return label
    End Function

    Private Function CreateCoilNumeric(x As Integer,
        y As Integer,
        minimum As Decimal,
        maximum As Decimal,
        value As Decimal,
        decimalPlaces As Integer) As NumericUpDown

        Dim control As New NumericUpDown()
        control.DecimalPlaces = decimalPlaces
        control.Increment = If(decimalPlaces = 0, 1D, 0.1D)
        control.Minimum = minimum
        control.Maximum = maximum
        control.Value = Math.Min(Math.Max(value, minimum), maximum)
        control.Location = New Point(x, y)
        control.Size = New Size(76, 20)
        AddHandler control.ValueChanged, AddressOf CoilPerformance_InputChanged
        Return control
    End Function

    Private Function CoilPerformance_Text(resourceName As String, fallbackText As String) As String
        Try
            Dim value As String = Environment.Localization.GetString(resourceName)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> "?" AndAlso
                Not value.StartsWith("@@", StringComparison.Ordinal) Then
                Return value
            End If
        Catch
        End Try

        Return fallbackText
    End Function

    Private Function CoilPerformance_HydraulicIssueText(code As CLCoilHydraulicIssueCode) As String
        Select Case code
            Case CLCoilHydraulicIssueCode.InvalidCoolingTemperatures
                Return CoilPerformance_Text(
                    "MainForm_CoilPerformance_InvalidCoolingTemperatures",
                    "Cooling water outlet temperature must be at least 1 K higher than inlet temperature.")
            Case CLCoilHydraulicIssueCode.InvalidHeatingTemperatures
                Return CoilPerformance_Text(
                    "MainForm_CoilPerformance_InvalidHeatingTemperatures",
                    "Heating water inlet temperature must be at least 1 K higher than outlet temperature.")
            Case CLCoilHydraulicIssueCode.LowWaterDeltaT
                Return CoilPerformance_Text(
                    "MainForm_CoilPerformance_LowWaterDeltaT",
                    "Low water delta T (3 K or more, less than 5 K). Water flow is higher than typical.")
            Case CLCoilHydraulicIssueCode.CriticalWaterDeltaT
                Return CoilPerformance_Text(
                    "MainForm_CoilPerformance_CriticalWaterDeltaT",
                    "Very low water delta T (less than 3 K). Excessive water flow and pressure drop. Verify hydraulic design.")
        End Select
        Return code.ToString()
    End Function

    Private Shared Sub CoilPerformance_AddStatusMessage(messages As List(Of String), message As String)
        If Not String.IsNullOrWhiteSpace(message) AndAlso Not messages.Contains(message) Then messages.Add(message)
    End Sub

    Private Sub CoilPerformance_UpdateLocalizedTexts()
        If tbpData_CoilPerformance Is Nothing Then
            Return
        End If

        tbpData_CoilPerformance.Text = CoilPerformance_Text("MainForm_CoilPerformance_Tab", "Water coils")
        CoilPerformance_UpdateLocalizedControlTexts(tbpData_CoilPerformance)

        If dgvCoilPerformance_Results IsNot Nothing Then
            For Each column As DataGridViewColumn In dgvCoilPerformance_Results.Columns
                Dim tagValues As String() = TryCast(column.Tag, String())
                If tagValues IsNot Nothing AndAlso tagValues.Length = 2 Then
                    column.HeaderText = CoilPerformance_Text(tagValues(0), tagValues(1))
                End If
            Next
        End If

        CoilPerformance_FillFluidTypes(CoilPerformance_SelectedFluidType())
        CoilPerformance_FillEditModes(CoilPerformance_SelectedEditMode())
        CoilPerformance_FillStandardCoils()
    End Sub

    Private Sub CoilPerformance_UpdateLocalizedControlTexts(parent As Control)
        For Each control As Control In parent.Controls
            Dim tagValues As String() = TryCast(control.Tag, String())
            If tagValues IsNot Nothing AndAlso tagValues.Length = 2 Then
                control.Text = CoilPerformance_Text(tagValues(0), tagValues(1))
            End If
            CoilPerformance_UpdateLocalizedControlTexts(control)
        Next
    End Sub

    Private Sub CoilPerformance_FillFluidTypes(Optional selectedFluidType As CLCOFluidType = CLCOFluidType.Water)
        If cmbCoilPerformance_FluidType Is Nothing Then
            Return
        End If

        RemoveHandler cmbCoilPerformance_FluidType.SelectedIndexChanged, AddressOf CoilPerformance_FluidTypeChanged

        cmbCoilPerformance_FluidType.Items.Clear()
        Dim fluidTypes As CLCOFluidType() = {
            CLCOFluidType.Water,
            CLCOFluidType.Glic_Etil,
            CLCOFluidType.Glic_Prop
        }

        For Each fluidType As CLCOFluidType In fluidTypes
            cmbCoilPerformance_FluidType.Items.Add(New CLComboBoxItemWrapper(Of CLCOFluidType)(
                CoilPerformance_FluidTypeName(fluidType),
                fluidType))
        Next

        cmbCoilPerformance_FluidType.SelectedIndex = 0
        For index As Integer = 0 To cmbCoilPerformance_FluidType.Items.Count - 1
            Dim item As CLComboBoxItemWrapper(Of CLCOFluidType) = TryCast(cmbCoilPerformance_FluidType.Items(index), CLComboBoxItemWrapper(Of CLCOFluidType))
            If item IsNot Nothing AndAlso item.Value = selectedFluidType Then
                cmbCoilPerformance_FluidType.SelectedIndex = index
                Exit For
            End If
        Next

        AddHandler cmbCoilPerformance_FluidType.SelectedIndexChanged, AddressOf CoilPerformance_FluidTypeChanged
    End Sub

    Private Sub CoilPerformance_FillEditModes(Optional selectedMode As CLCoilPerformanceEditMode = CLCoilPerformanceEditMode.Standard)
        If cmbCoilPerformance_EditMode Is Nothing Then
            Return
        End If

        RemoveHandler cmbCoilPerformance_EditMode.SelectedIndexChanged, AddressOf CoilPerformance_EditModeChanged
        cmbCoilPerformance_EditMode.Items.Clear()
        cmbCoilPerformance_EditMode.Items.Add(New CLComboBoxItemWrapper(Of CLCoilPerformanceEditMode)(
            CoilPerformance_Text("MainForm_CoilPerformance_Standard", "Standard"), CLCoilPerformanceEditMode.Standard))
        cmbCoilPerformance_EditMode.Items.Add(New CLComboBoxItemWrapper(Of CLCoilPerformanceEditMode)(
            CoilPerformance_Text("MainForm_CoilPerformance_StandardCustomized", "Customized"), CLCoilPerformanceEditMode.StandardCustomized))

        cmbCoilPerformance_EditMode.SelectedIndex = 0
        For index As Integer = 0 To cmbCoilPerformance_EditMode.Items.Count - 1
            Dim item As CLComboBoxItemWrapper(Of CLCoilPerformanceEditMode) = TryCast(cmbCoilPerformance_EditMode.Items(index), CLComboBoxItemWrapper(Of CLCoilPerformanceEditMode))
            If item IsNot Nothing AndAlso item.Value = selectedMode Then
                cmbCoilPerformance_EditMode.SelectedIndex = index
                Exit For
            End If
        Next
        AddHandler cmbCoilPerformance_EditMode.SelectedIndexChanged, AddressOf CoilPerformance_EditModeChanged
    End Sub

    Private Sub CoilPerformance_FillFinSpacings()
        If cmbCoilPerformance_FinSpacing Is Nothing Then
            Return
        End If

        cmbCoilPerformance_FinSpacing.Items.Clear()

        For Each finSpacing As CLCOFinSpacing In [Enum].GetValues(GetType(CLCOFinSpacing))
            Dim value As Double = CLCoilPerformanceCalculator.FinSpacingToDouble(finSpacing)
            cmbCoilPerformance_FinSpacing.Items.Add(New CLComboBoxItemWrapper(Of Double)(
                value.ToString("0.0", Globalization.CultureInfo.CurrentCulture),
                value))
        Next

        CoilPerformance_SelectFinSpacing(2.5D)
    End Sub

    Private Sub CoilPerformance_SelectFinSpacing(value As Double)
        If cmbCoilPerformance_FinSpacing Is Nothing Then
            Return
        End If

        Dim selectedIndex As Integer = -1
        Dim bestDistance As Double = Double.MaxValue

        For i As Integer = 0 To cmbCoilPerformance_FinSpacing.Items.Count - 1
            Dim wrapper As CLComboBoxItemWrapper(Of Double) = TryCast(cmbCoilPerformance_FinSpacing.Items(i), CLComboBoxItemWrapper(Of Double))
            If wrapper Is Nothing Then
                Continue For
            End If

            Dim distance As Double = Math.Abs(wrapper.Value - value)
            If distance < bestDistance Then
                bestDistance = distance
                selectedIndex = i
            End If
        Next

        If selectedIndex >= 0 Then
            cmbCoilPerformance_FinSpacing.SelectedIndex = selectedIndex
        End If
    End Sub

    Private Function CoilPerformance_SelectedFinSpacing() As Double
        If cmbCoilPerformance_FinSpacing IsNot Nothing AndAlso TypeOf cmbCoilPerformance_FinSpacing.SelectedItem Is CLComboBoxItemWrapper(Of Double) Then
            Return DirectCast(cmbCoilPerformance_FinSpacing.SelectedItem, CLComboBoxItemWrapper(Of Double)).Value
        End If

        Return 2.5D
    End Function

    Private Function CoilPerformance_FluidTypeName(fluidType As CLCOFluidType) As String
        Select Case fluidType
            Case CLCOFluidType.Water
                Return CoilPerformance_ResourceText(CLMessageResources.Water.ToString(), "Acqua")
            Case CLCOFluidType.Glic_Etil
                Return CoilPerformance_ResourceText(CLMessageResources.Glic_Etil.ToString(), "Glicole etilico")
            Case CLCOFluidType.Glic_Prop
                Return CoilPerformance_ResourceText(CLMessageResources.Glic_Prop.ToString(), "Glicole propilenico")
            Case Else
                Return fluidType.ToString().Replace("_"c, " "c)
        End Select
    End Function

    Private Function CoilPerformance_ResourceText(resourceName As String, fallbackText As String) As String
        Try
            Dim value As String = Environment.Localization.GetString(resourceName)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> "?" AndAlso Not value.StartsWith("@@", StringComparison.Ordinal) Then
                Return value
            End If
        Catch
        End Try

        Return fallbackText
    End Function

    Private Function CoilPerformance_SelectedEditMode() As CLCoilPerformanceEditMode
        If cmbCoilPerformance_EditMode IsNot Nothing AndAlso TypeOf cmbCoilPerformance_EditMode.SelectedItem Is CLComboBoxItemWrapper(Of CLCoilPerformanceEditMode) Then
            Return DirectCast(cmbCoilPerformance_EditMode.SelectedItem, CLComboBoxItemWrapper(Of CLCoilPerformanceEditMode)).Value
        End If

        Return CLCoilPerformanceEditMode.Standard
    End Function

    Private Function CoilPerformance_SelectedInstallation() As CLCoilInstallationType
        If cmbCoilPerformance_Installation IsNot Nothing AndAlso TypeOf cmbCoilPerformance_Installation.SelectedItem Is CLComboBoxItemWrapper(Of CLCoilInstallationType) Then
            Return DirectCast(cmbCoilPerformance_Installation.SelectedItem, CLComboBoxItemWrapper(Of CLCoilInstallationType)).Value
        End If

        Return CLCoilInstallationType.Internal
    End Function

    Private Function CoilPerformance_SourceInstallation() As CLCoilInstallationType
        Dim selectedInstallation As CLCoilInstallationType = CoilPerformance_SelectedInstallation()

        If selectedInstallation = CLCoilInstallationType.RequestedInternal Then
            Return CLCoilInstallationType.External
        End If

        If selectedInstallation = CLCoilInstallationType.External AndAlso
            Not m_CoilPerformanceCoils.Any(Function(coil) coil.Installation = CLCoilInstallationType.External) AndAlso
            m_CoilPerformanceCoils.Any(Function(coil) coil.Installation = CLCoilInstallationType.Internal) Then
            Return CLCoilInstallationType.Internal
        End If

        Return selectedInstallation
    End Function

    Private Function CoilPerformance_UsesExternalGeometry() As Boolean
        Return CoilPerformance_SelectedInstallation() <> CLCoilInstallationType.Internal
    End Function

    Private Function CoilPerformance_SelectedFluidType() As CLCOFluidType
        If cmbCoilPerformance_FluidType IsNot Nothing AndAlso TypeOf cmbCoilPerformance_FluidType.SelectedItem Is CLComboBoxItemWrapper(Of CLCOFluidType) Then
            Return DirectCast(cmbCoilPerformance_FluidType.SelectedItem, CLComboBoxItemWrapper(Of CLCOFluidType)).Value
        End If

        Return CLCOFluidType.Water
    End Function

    Private Function CoilPerformance_HeightMode() As String
        If cmbCoilPerformance_HeightMode IsNot Nothing AndAlso TypeOf cmbCoilPerformance_HeightMode.SelectedItem Is CLComboBoxItemWrapper(Of String) Then
            Return DirectCast(cmbCoilPerformance_HeightMode.SelectedItem, CLComboBoxItemWrapper(Of String)).Value
        End If

        Return "mm"
    End Function

    Private Sub CoilPerformance_ModeChanged(sender As Object, e As EventArgs)
        If m_CoilPerformanceChanging Then
            Return
        End If

        CoilPerformance_UpdateControlState()
        ElectricHeater_UpdateControlState()
        CoilPerformance_Recalculate()
    End Sub

    Private Sub CoilPerformance_EnableChanged(sender As Object, e As EventArgs)
        If m_CoilPerformanceBusy Then
            Return
        End If

        CoilPerformance_UpdateControlState()
        ElectricHeater_UpdateControlState()

        If chbCoilPerformance_Enable.Checked Then
            CoilPerformance_SetBusy(True)
            Try
                Calculate()
            Finally
                CoilPerformance_SetBusy(False)
            End Try
        Else
            Calculate()
        End If
    End Sub

    Private Sub CoilPerformance_SetBusy(busy As Boolean)
        m_CoilPerformanceBusy = busy
        Me.UseWaitCursor = busy
        tbpData_CoilPerformance.UseWaitCursor = busy

        If busy Then
            chbCoilPerformance_Enable.Enabled = False
            chbCoilPerformance_Enable.Text = CoilPerformance_Text(
                "MainForm_CoilPerformance_Calculating",
                "Calculating, please wait...")
            chbCoilPerformance_Enable.Refresh()
            tbpData_CoilPerformance.Refresh()
        Else
            chbCoilPerformance_Enable.Text = CoilPerformance_Text(
                "MainForm_CoilPerformance_Enable",
                "Enable coil calculation")
            CoilPerformance_UpdateControlState()
        End If
    End Sub

    Private Sub CoilPerformance_EditModeChanged(sender As Object, e As EventArgs)
        Dim customized As Boolean = CoilPerformance_SelectedEditMode() = CLCoilPerformanceEditMode.StandardCustomized
        If customized AndAlso Not m_CoilCustomDisclaimerAccepted AndAlso Not m_ProjectApplying AndAlso Not m_CoilPerformanceChanging Then
            MessageBox.Show(Me,
                CoilPerformance_Text(
                    "MainForm_CoilPerformance_CustomDisclaimer",
                    "Verify that the customized coil operates correctly at every intended working point. Press OK to acknowledge this requirement."),
                CoilPerformance_Text("MainForm_CoilPerformance_CustomDisclaimerTitle", "Custom coil design"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            m_CoilCustomDisclaimerAccepted = True
        ElseIf Not customized Then
            m_CoilCustomDisclaimerAccepted = False
        End If
        CoilPerformance_FillStandardCoils()
        CoilPerformance_UpdateControlState()
    End Sub

    Private Sub CoilPerformance_InstallationChanged(sender As Object, e As EventArgs)
        If m_CoilPerformanceChanging Then
            Return
        End If

        CoilPerformance_FillStandardCoils()
        CoilPerformance_UpdateControlState()
    End Sub

    Private Sub CoilPerformance_FluidTypeChanged(sender As Object, e As EventArgs)
        CoilPerformance_UpdateControlState()
        CoilPerformance_Recalculate()
    End Sub

    Private Sub CoilPerformance_CoilChanged(sender As Object, e As EventArgs)
        CoilPerformance_LoadSelectedCoil()
        CoilPerformance_Recalculate()
    End Sub

    Private Sub CoilPerformance_InputChanged(sender As Object, e As EventArgs)
        If m_CoilPerformanceChanging Then
            Return
        End If

        If sender Is nudCoilPerformance_CoolingIn OrElse
            sender Is nudCoilPerformance_CoolingOut OrElse
            sender Is nudCoilPerformance_HeatingIn OrElse
            sender Is nudCoilPerformance_HeatingOut Then
            CoilPerformance_UpdateWaterTemperatureLimits()
        End If

        If sender Is nudCoilPerformance_Tubes AndAlso CoilPerformance_HeightMode() = "tubes" Then
            Try
                m_CoilPerformanceChanging = True
                nudCoilPerformance_Height.Value = Math.Min(nudCoilPerformance_Height.Maximum, nudCoilPerformance_Tubes.Value * 25D)
            Finally
                m_CoilPerformanceChanging = False
            End Try
        ElseIf sender Is cmbCoilPerformance_HeightMode Then
            CoilPerformance_UpdateControlState()
        End If

        CoilPerformance_Recalculate()
    End Sub

    Private Sub CoilPerformance_Recalculate()
        If m_CoilPerformanceAvailable AndAlso chbCoilPerformance_Enable IsNot Nothing AndAlso chbCoilPerformance_Enable.Checked Then
            Calculate()
        Else
            Calculate_CoilPerformance()
        End If
    End Sub

    Private Sub CoilPerformance_FillStandardCoils()
        If cmbCoilPerformance_Coil Is Nothing OrElse cmbCoilPerformance_Mode Is Nothing OrElse cmbCoilPerformance_Installation Is Nothing Then
            Return
        End If

        Try
            m_CoilPerformanceChanging = True
            Dim selectedCoil As CLCoilDefinition = TryCast(cmbCoilPerformance_Coil.SelectedItem, CLCoilDefinition)
            Dim preferredInstallation As CLCoilInstallationType = CoilPerformance_SelectedInstallation()
            Dim currentModelId As Integer = If(SelectedHeatRecoveryModel Is Nothing, -1, SelectedHeatRecoveryModel.Id)
            If currentModelId <> m_CoilPerformanceModelId Then
                preferredInstallation = CLCoilInstallationType.Internal
                selectedCoil = Nothing
                m_CoilPerformanceModelId = currentModelId
            End If
            cmbCoilPerformance_Coil.Items.Clear()

            m_CoilPerformanceCoils = CLCoilPerformanceCalculator.GetAvailableCoils(SelectedHeatRecoveryModel)
            m_CoilPerformanceAvailable = m_CoilPerformanceCoils.Count > 0

            If Not m_CoilPerformanceAvailable AndAlso chbCoilPerformance_Enable IsNot Nothing Then
                chbCoilPerformance_Enable.Checked = False
            End If

            RemoveHandler cmbCoilPerformance_Installation.SelectedIndexChanged, AddressOf CoilPerformance_InstallationChanged
            cmbCoilPerformance_Installation.Items.Clear()

            Dim hasInternal As Boolean = m_CoilPerformanceCoils.Any(Function(coil) coil.Installation = CLCoilInstallationType.Internal)
            Dim hasExternal As Boolean = m_CoilPerformanceCoils.Any(Function(coil) coil.Installation = CLCoilInstallationType.External)

            If hasInternal Then
                cmbCoilPerformance_Installation.Items.Add(New CLComboBoxItemWrapper(Of CLCoilInstallationType)(
                    CoilPerformance_Text("MainForm_CoilPerformance_Internal", "Internal"), CLCoilInstallationType.Internal))
                cmbCoilPerformance_Installation.Items.Add(New CLComboBoxItemWrapper(Of CLCoilInstallationType)(
                    CoilPerformance_Text("MainForm_CoilPerformance_ExternalInstallation", "External"), CLCoilInstallationType.External))
            ElseIf hasExternal Then
                cmbCoilPerformance_Installation.Items.Add(New CLComboBoxItemWrapper(Of CLCoilInstallationType)(
                    CoilPerformance_Text("MainForm_CoilPerformance_ExternalInstallation", "External"), CLCoilInstallationType.External))
                cmbCoilPerformance_Installation.Items.Add(New CLComboBoxItemWrapper(Of CLCoilInstallationType)(
                    CoilPerformance_Text("MainForm_CoilPerformance_RequestInternal", "Request internal"), CLCoilInstallationType.RequestedInternal))
            End If

            Dim installationIndex As Integer = -1
            For index As Integer = 0 To cmbCoilPerformance_Installation.Items.Count - 1
                Dim item As CLComboBoxItemWrapper(Of CLCoilInstallationType) = TryCast(cmbCoilPerformance_Installation.Items(index), CLComboBoxItemWrapper(Of CLCoilInstallationType))
                If item IsNot Nothing AndAlso item.Value = preferredInstallation Then
                    installationIndex = index
                    Exit For
                End If
            Next
            If installationIndex < 0 AndAlso cmbCoilPerformance_Installation.Items.Count > 0 Then
                installationIndex = 0
            End If
            cmbCoilPerformance_Installation.SelectedIndex = installationIndex
            AddHandler cmbCoilPerformance_Installation.SelectedIndexChanged, AddressOf CoilPerformance_InstallationChanged

            Dim sourceInstallation As CLCoilInstallationType = CoilPerformance_SourceInstallation()
            For Each coil As CLCoilDefinition In m_CoilPerformanceCoils.Where(Function(item) item.Installation = sourceInstallation)
                cmbCoilPerformance_Coil.Items.Add(coil.Clone())
            Next

            If cmbCoilPerformance_Coil.Items.Count > 0 Then
                Dim selectedIndex As Integer = 0
                If selectedCoil IsNot Nothing Then
                    For index As Integer = 0 To cmbCoilPerformance_Coil.Items.Count - 1
                        Dim candidate As CLCoilDefinition = TryCast(cmbCoilPerformance_Coil.Items(index), CLCoilDefinition)
                        If candidate IsNot Nothing AndAlso candidate.Id = selectedCoil.Id AndAlso candidate.Installation = sourceInstallation Then
                            selectedIndex = index
                            Exit For
                        End If
                    Next
                End If
                cmbCoilPerformance_Coil.SelectedIndex = selectedIndex
            End If
        Finally
            m_CoilPerformanceChanging = False
        End Try

        CoilPerformance_LoadSelectedCoil()
        CoilPerformance_UpdateControlState()
        CoilPerformance_Recalculate()
    End Sub

    Private Sub CoilPerformance_ResetWaterTemperatureLimits()
        nudCoilPerformance_CoolingIn.Minimum = -50D
        nudCoilPerformance_CoolingIn.Maximum = 99D
        nudCoilPerformance_CoolingOut.Minimum = -49D
        nudCoilPerformance_CoolingOut.Maximum = 100D
        nudCoilPerformance_HeatingIn.Minimum = -49D
        nudCoilPerformance_HeatingIn.Maximum = 150D
        nudCoilPerformance_HeatingOut.Minimum = -50D
        nudCoilPerformance_HeatingOut.Maximum = 149D
    End Sub

    Private Sub CoilPerformance_UpdateWaterTemperatureLimits()
        If nudCoilPerformance_CoolingIn Is Nothing OrElse
            nudCoilPerformance_CoolingOut Is Nothing OrElse
            nudCoilPerformance_HeatingIn Is Nothing OrElse
            nudCoilPerformance_HeatingOut Is Nothing Then
            Return
        End If

        Dim wasChanging As Boolean = m_CoilPerformanceChanging
        Try
            m_CoilPerformanceChanging = True
            CoilPerformance_ResetWaterTemperatureLimits()

            If nudCoilPerformance_CoolingOut.Value < nudCoilPerformance_CoolingIn.Value + 1D Then
                nudCoilPerformance_CoolingOut.Value = nudCoilPerformance_CoolingIn.Value + 1D
            End If
            If nudCoilPerformance_HeatingIn.Value < nudCoilPerformance_HeatingOut.Value + 1D Then
                nudCoilPerformance_HeatingOut.Value = nudCoilPerformance_HeatingIn.Value - 1D
            End If

            nudCoilPerformance_CoolingIn.Maximum = nudCoilPerformance_CoolingOut.Value - 1D
            nudCoilPerformance_CoolingOut.Minimum = nudCoilPerformance_CoolingIn.Value + 1D
            nudCoilPerformance_HeatingIn.Minimum = nudCoilPerformance_HeatingOut.Value + 1D
            nudCoilPerformance_HeatingOut.Maximum = nudCoilPerformance_HeatingIn.Value - 1D
        Finally
            m_CoilPerformanceChanging = wasChanging
        End Try
    End Sub

    Private Sub CoilPerformance_LoadSelectedCoil()
        If cmbCoilPerformance_Coil Is Nothing OrElse cmbCoilPerformance_Coil.SelectedItem Is Nothing Then
            Return
        End If

        Dim coil As CLCoilDefinition = DirectCast(cmbCoilPerformance_Coil.SelectedItem, CLCoilDefinition)

        Try
            m_CoilPerformanceChanging = True
            nudCoilPerformance_Length.Value = Math.Min(Math.Max(coil.Length, CInt(nudCoilPerformance_Length.Minimum)), CInt(nudCoilPerformance_Length.Maximum))
            nudCoilPerformance_Height.Value = Math.Min(Math.Max(coil.Height, CInt(nudCoilPerformance_Height.Minimum)), CInt(nudCoilPerformance_Height.Maximum))
            nudCoilPerformance_Rows.Value = Math.Min(Math.Max(coil.NumberOfRows, CInt(nudCoilPerformance_Rows.Minimum)), CInt(nudCoilPerformance_Rows.Maximum))
            nudCoilPerformance_Circuits.Value = Math.Min(Math.Max(coil.NumberOfCircuits, CInt(nudCoilPerformance_Circuits.Minimum)), CInt(nudCoilPerformance_Circuits.Maximum))
            CoilPerformance_SelectFinSpacing(If(coil.FinSpacingValue > 0, coil.FinSpacingValue, CLCoilPerformanceCalculator.FinSpacingToDouble(coil.FinSpacing)))
            nudCoilPerformance_Tubes.Value = Math.Min(nudCoilPerformance_Tubes.Maximum, Math.Max(nudCoilPerformance_Tubes.Minimum, Math.Round(nudCoilPerformance_Height.Value / 25D)))
        Finally
            m_CoilPerformanceChanging = False
        End Try

        CoilPerformance_UpdateControlState()
    End Sub

    Private Sub CoilPerformance_UpdateControlState()
        If chbCoilPerformance_Enable Is Nothing Then
            Return
        End If
        If cmbCoilPerformance_EditMode Is Nothing OrElse cmbCoilPerformance_Installation Is Nothing OrElse nudCoilPerformance_FluidTec Is Nothing OrElse dgvCoilPerformance_Results Is Nothing Then
            Return
        End If

        CoilPerformance_ApplyElectricPostHeaterConstraint()

        Dim enabled As Boolean = m_CoilPerformanceAvailable AndAlso chbCoilPerformance_Enable.Checked
        Dim electricPostHeaterActive As Boolean = enabled AndAlso ElectricHeater_IsModeEnabled(CLElectricHeaterMode.EHD)
        Dim calculationMode As CLCoilPerformanceMode = If(
            cmbCoilPerformance_Mode.SelectedItem Is Nothing,
            CLCoilPerformanceMode.HCD,
            DirectCast(cmbCoilPerformance_Mode.SelectedItem, CLCoilPerformanceMode))
        Dim coolingEnabled As Boolean = enabled AndAlso
            (calculationMode = CLCoilPerformanceMode.CWD OrElse calculationMode = CLCoilPerformanceMode.HCD)
        Dim heatingEnabled As Boolean = enabled AndAlso
            (calculationMode = CLCoilPerformanceMode.HWD OrElse calculationMode = CLCoilPerformanceMode.HCD)
        Dim editMode As CLCoilPerformanceEditMode = CoilPerformance_SelectedEditMode()
        Dim geometryEnabled As Boolean = enabled AndAlso editMode <> CLCoilPerformanceEditMode.Standard
        Dim externalCustomized As Boolean = geometryEnabled AndAlso CoilPerformance_UsesExternalGeometry()
        Dim fluidType As CLCOFluidType = CoilPerformance_SelectedFluidType()
        Dim heightInTubes As Boolean = CoilPerformance_HeightMode() = "tubes"

        tbpData_CoilPerformance.Enabled = m_CoilPerformanceAvailable
        chbCoilPerformance_Enable.Enabled = m_CoilPerformanceAvailable
        cmbCoilPerformance_EditMode.Enabled = enabled
        cmbCoilPerformance_Installation.Enabled = enabled AndAlso cmbCoilPerformance_Installation.Items.Count > 1
        cmbCoilPerformance_Mode.Enabled = enabled AndAlso Not electricPostHeaterActive
        cmbCoilPerformance_Coil.Enabled = enabled
        cmbCoilPerformance_FluidType.Enabled = enabled
        nudCoilPerformance_FluidTec.Enabled = enabled AndAlso fluidType <> CLCOFluidType.Water
        nudCoilPerformance_CoolingIn.Enabled = coolingEnabled
        nudCoilPerformance_CoolingOut.Enabled = coolingEnabled
        nudCoilPerformance_HeatingIn.Enabled = heatingEnabled
        nudCoilPerformance_HeatingOut.Enabled = heatingEnabled
        nudCoilPerformance_Rows.Enabled = geometryEnabled
        cmbCoilPerformance_FinSpacing.Enabled = geometryEnabled
        nudCoilPerformance_Circuits.Enabled = geometryEnabled
        nudCoilPerformance_Length.Enabled = externalCustomized
        cmbCoilPerformance_HeightMode.Enabled = externalCustomized
        nudCoilPerformance_Height.Enabled = externalCustomized AndAlso Not heightInTubes
        nudCoilPerformance_Tubes.Enabled = externalCustomized AndAlso heightInTubes
        dgvCoilPerformance_Results.Enabled = enabled
        lblCoilPerformance_CustomWarning.Visible = geometryEnabled
        lblCoilPerformance_DimensionsNote.Visible = geometryEnabled

        If Not enabled Then
            dgvCoilPerformance_Results.Rows.Clear()
            m_CoilPerformanceLastResults.Clear()
            lblCoilPerformance_Status.Text = ""
            m_CoilPerformanceLastPressureDrop = 0
        End If
    End Sub

    Private Sub CoilPerformance_ApplyElectricPostHeaterConstraint()
        If chbCoilPerformance_Enable Is Nothing OrElse Not chbCoilPerformance_Enable.Checked OrElse
            cmbCoilPerformance_Mode Is Nothing OrElse Not ElectricHeater_IsModeEnabled(CLElectricHeaterMode.EHD) Then
            Return
        End If

        If cmbCoilPerformance_Mode.SelectedItem IsNot Nothing AndAlso
            DirectCast(cmbCoilPerformance_Mode.SelectedItem, CLCoilPerformanceMode) = CLCoilPerformanceMode.CWD Then
            Return
        End If

        Dim wasChanging As Boolean = m_CoilPerformanceChanging
        Try
            m_CoilPerformanceChanging = True
            cmbCoilPerformance_Mode.SelectedItem = CLCoilPerformanceMode.CWD
        Finally
            m_CoilPerformanceChanging = wasChanging
        End Try
    End Sub

    Private Function CoilPerformance_GetEditedCoil() As CLCoilDefinition
        If cmbCoilPerformance_Coil Is Nothing OrElse cmbCoilPerformance_Coil.SelectedItem Is Nothing Then
            Return Nothing
        End If

        Dim coil As CLCoilDefinition = DirectCast(cmbCoilPerformance_Coil.SelectedItem, CLCoilDefinition).Clone()
        coil.Length = CInt(nudCoilPerformance_Length.Value)
        If CoilPerformance_SelectedEditMode() = CLCoilPerformanceEditMode.StandardCustomized AndAlso
            CoilPerformance_UsesExternalGeometry() AndAlso
            CoilPerformance_HeightMode() = "tubes" Then
            coil.Height = CInt(nudCoilPerformance_Tubes.Value * 25D)
        Else
            coil.Height = CInt(nudCoilPerformance_Height.Value)
        End If
        coil.NumberOfRows = CInt(nudCoilPerformance_Rows.Value)
        coil.NumberOfCircuits = CInt(nudCoilPerformance_Circuits.Value)
        coil.FinSpacing = CLCoilPerformanceCalculator.DoubleToFinSpacing(CoilPerformance_SelectedFinSpacing())
        coil.FinSpacingValue = CoilPerformance_SelectedFinSpacing()
        Return coil
    End Function

    Private Function Calculate_CoilPerformance() As Double
        If dgvCoilPerformance_Results Is Nothing OrElse m_CoilPerformanceChanging Then
            Return m_CoilPerformanceLastPressureDrop
        End If

        dgvCoilPerformance_Results.Rows.Clear()
        m_CoilPerformanceLastResults.Clear()
        lblCoilPerformance_Status.Text = ""
        m_CoilPerformanceLastPressureDrop = 0

        If Not m_CoilPerformanceAvailable OrElse chbCoilPerformance_Enable Is Nothing OrElse Not chbCoilPerformance_Enable.Checked Then
            Return 0
        End If

        Dim coil As CLCoilDefinition = CoilPerformance_GetEditedCoil()
        If coil Is Nothing Then
            Return 0
        End If

        Dim input As New CLCoilCalculationInput With {
            .Coil = coil,
            .CalculationMode = DirectCast(cmbCoilPerformance_Mode.SelectedItem, CLCoilPerformanceMode),
            .AirFlow = AirFlow,
            .AirInletTemperature = If(m_HasLastWinterThermo, m_LastWinterThermo.Supply_outlet_temp, SupplyOutletTemp),
            .AirInletRH = If(m_HasLastWinterThermo, 100 * m_LastWinterThermo.Supply_outlet_rh, SupplyOutletRH),
            .UseModeAirInletConditions = True,
            .CoolingAirInletTemperature = If(m_SummerCalculationEnabled AndAlso m_HasLastSummerThermo, m_LastSummerThermo.Supply_outlet_temp, If(m_HasLastWinterThermo, m_LastWinterThermo.Supply_outlet_temp, SupplyOutletTemp)),
            .CoolingAirInletRH = If(m_SummerCalculationEnabled AndAlso m_HasLastSummerThermo, 100 * m_LastSummerThermo.Supply_outlet_rh, If(m_HasLastWinterThermo, 100 * m_LastWinterThermo.Supply_outlet_rh, SupplyOutletRH)),
            .HeatingAirInletTemperature = If(m_HasLastWinterThermo, m_LastWinterThermo.Supply_outlet_temp, SupplyOutletTemp),
            .HeatingAirInletRH = If(m_HasLastWinterThermo, 100 * m_LastWinterThermo.Supply_outlet_rh, SupplyOutletRH),
            .FluidType = CoilPerformance_SelectedFluidType(),
            .FluidTypeTec = CDbl(nudCoilPerformance_FluidTec.Value),
            .CoolingFluidInletTemperature = CDbl(nudCoilPerformance_CoolingIn.Value),
            .CoolingFluidOutletTemperature = CDbl(nudCoilPerformance_CoolingOut.Value),
            .HeatingFluidInletTemperature = CDbl(nudCoilPerformance_HeatingIn.Value),
            .HeatingFluidOutletTemperature = CDbl(nudCoilPerformance_HeatingOut.Value)
        }

        Dim hydraulicIssues As List(Of CLCoilHydraulicIssue) = CLCoilHydraulicRules.Evaluate(
            input.CalculationMode,
            input.CoolingFluidInletTemperature,
            input.CoolingFluidOutletTemperature,
            input.HeatingFluidInletTemperature,
            input.HeatingFluidOutletTemperature)
        Dim statusMessages As New List(Of String)()
        Dim hasCriticalStatus As Boolean = False
        If ElectricHeater_IsModeEnabled(CLElectricHeaterMode.EHD) Then
            CoilPerformance_AddStatusMessage(
                statusMessages,
                CoilPerformance_Text(
                    "MainForm_CoilPerformance_ElectricPostHeaterConflict",
                    "Disable the electric post-heater (EHD) to enable water post-heating."))
            hasCriticalStatus = True
        End If
        For Each issue As CLCoilHydraulicIssue In hydraulicIssues
            CoilPerformance_AddStatusMessage(statusMessages, CoilPerformance_HydraulicIssueText(issue.Code))
            hasCriticalStatus = hasCriticalStatus OrElse issue.IsBlocking OrElse
                issue.Code = CLCoilHydraulicIssueCode.CriticalWaterDeltaT
        Next
        If hydraulicIssues.Any(Function(issue) issue.IsBlocking) Then
            lblCoilPerformance_Status.ForeColor = Color.Firebrick
            lblCoilPerformance_Status.Text = String.Join(System.Environment.NewLine, statusMessages)
            Return 0
        End If

        Dim results As List(Of CLCoilCalculationResult) = CLCoilPerformanceCalculator.Calculate(input)
        m_CoilPerformanceLastResults.AddRange(results)

        For Each result As CLCoilCalculationResult In results
            Dim status As String = If(result.IsOk, CoilPerformance_Text("MainForm_CoilPerformance_OK", "OK"), If(String.IsNullOrEmpty(result.ErrorMessage), result.Auxiliary.ToString(), result.ErrorMessage))
            Dim rowIndex As Integer = dgvCoilPerformance_Results.Rows.Add(
                result.Mode.ToString(),
                status,
                FormatNumber(result.HeatTransferred, 0),
                FormatNumber(result.SensibleHeat, 0),
                FormatNumber(result.OutletTemperature, 1),
                FormatNumber(result.OutletRH, 0),
                FormatNumber(result.CondensedWater, 2),
                FormatNumber(result.AirPressureDrop, 0),
                FormatNumber(result.WaterPressureDrop, 1),
                FormatNumber(result.FluidFlow, 0),
                FormatNumber(result.FluidSpeed, 2),
                FormatNumber(result.FaceVelocity, 2))

            If result.WaterPressureDrop > CLCoilHydraulicRules.MaximumRecommendedWaterPressureDrop Then
                Dim waterPressureCell As DataGridViewCell = dgvCoilPerformance_Results.Rows(rowIndex).Cells("WaterDP")
                waterPressureCell.Style.BackColor = Color.MistyRose
                waterPressureCell.Style.ForeColor = Color.Firebrick
                CoilPerformance_AddStatusMessage(
                    statusMessages,
                    CoilPerformance_Text(
                        "MainForm_CoilPerformance_WaterPressureDropWarning",
                        "Water pressure drop exceeds the recommended limit (40 kPa). Hydraulic power consumption and pumping costs may become excessive. Consider increasing coil size or reducing water velocity."))
                hasCriticalStatus = True
            End If

            If Not result.IsOk Then
                CoilPerformance_AddStatusMessage(statusMessages, status)
                hasCriticalStatus = True
            End If

            m_CoilPerformanceLastPressureDrop = Math.Max(m_CoilPerformanceLastPressureDrop, result.AirPressureDrop)
        Next

        If m_CoilPerformanceLastPressureDrop > 0 Then
            statusMessages.Insert(0, String.Format(
                "{0}: {1} Pa",
                CoilPerformance_Text("MainForm_CoilPerformance_AirPressureApplied", "Coil air pressure drop applied"),
                FormatNumber(m_CoilPerformanceLastPressureDrop, 0)))
        End If

        lblCoilPerformance_Status.ForeColor = If(hasCriticalStatus, Color.Firebrick, If(statusMessages.Count > 0, Color.DarkOrange, Color.Firebrick))
        lblCoilPerformance_Status.Text = String.Join(System.Environment.NewLine, statusMessages)

        Return m_CoilPerformanceLastPressureDrop
    End Function

#End Region

#Region "====[ Calculate ]===="

    Private Sub Calculate()
        If cmbPerformance_HeatRecoveryModels.SelectedIndex = -1 Then
            Return
        End If

        Dim maxPressure As Double = ParseUIDouble(txbPerformance_MaxPressure.Text)
        Dim preheatPowerW As Double = ElectricHeater_GetEnabledPower(CLElectricHeaterMode.PEHD)
        Calculate_Data(0, maxPressure, preheatPowerW)

        Dim coilPressureDrop As Double = Calculate_CoilPerformance()
        Dim electricPressureDrop As Double = Calculate_ElectricHeaters()
        Dim totalAccessoryPressureDrop As Double = coilPressureDrop + electricPressureDrop
        If totalAccessoryPressureDrop > 0 Then
            Calculate_Data(totalAccessoryPressureDrop, maxPressure, preheatPowerW)
            Calculate_CoilPerformance()
            Calculate_ElectricHeaters()
        End If

        Calculate_Sound()
        Calculate_CO2Level()

        ' Aggiornamento UI
        If chbPerformance_PassiveHaus_ShowArea.Checked AndAlso CDbl(txbPerformance_PassiveHaus.Text) <= CDbl(txbPerformance_PassiveHaus_Limit.Text) Then
            txbPerformance_PassiveHaus.BackColor = Color.Green
            txbPerformance_PassiveHaus.ForeColor = Color.White
        Else
            txbPerformance_PassiveHaus.BackColor = SystemColors.Control
            txbPerformance_PassiveHaus.ForeColor = SystemColors.WindowText
        End If

        If chbPerformance_SFP_ShowArea.Checked AndAlso CDbl(txbPerformance_SFP.Text) <= nudPerformance_SFP_Limit.Value Then
            txbPerformance_SFP.BackColor = Color.Green
            txbPerformance_SFP.ForeColor = Color.White
        Else
            txbPerformance_SFP.BackColor = SystemColors.Control
            txbPerformance_SFP.ForeColor = SystemColors.WindowText
        End If

        If chbPerformance_SEL_ShowArea.Checked AndAlso CDbl(txbPerformance_SEL.Text) <= nudPerformance_SEL_Limit.Value Then
            txbPerformance_SEL.BackColor = Color.Green
            txbPerformance_SEL.ForeColor = Color.White
        Else
            txbPerformance_SEL.BackColor = SystemColors.Control
            txbPerformance_SEL.ForeColor = SystemColors.WindowText
        End If
    End Sub

    Private Sub Calculate_Sound()

        'Dim fonoIsolamento As Double() = {3, 8, 14, 20, 23, 26, 27, 35}  Riverificato ASHRAE Handbook - HVAC Applications, Chapter 47 
        Dim fonoIsolamento As Double() = {20, 23, 26, 29, 32, 37, 43, 45}

        m_SoundPerformances.Rows.Clear()

        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel
        Dim soundDataRow As DataRow
        Dim soundValues() As Double
        Dim directivity As Int16

        If rdb_Q2.Checked Then
            directivity = 2
        ElseIf rdb_Q4.Checked Then
            directivity = 4
        ElseIf rdb_Q8.Checked Then
            directivity = 8
        End If

        If dcHeatRecoveryModel Is Nothing Then
            Return
        End If

        ' Sound Fresh
        If dcHeatRecoveryModel.HasSoundData_Inlet Then

            soundDataRow = m_SoundPerformances.NewRow()

            'TODO: Vedere come fare
            If dcHeatRecoveryModel.CLSerie.Code = "7" AndAlso (dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "HCI" Or dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "FS") Then
                soundValues = dcHeatRecoveryModel.SoundData_OutletItems
            Else
                soundValues = dcHeatRecoveryModel.SoundData_InletItems
            End If

            For counter As Integer = 0 To soundValues.Length - 1
                soundValues(counter) *= 1.01D
            Next

            soundDataRow(SoundColumnName_Type) = CLSoundPerformanceType._Fresh.ToString()
            soundDataRow(SoundColumnName_Caption) = Environment.Localization.GetString(CLMessageResources.Sound_Fresh.ToString())
            soundDataRow(SoundColumnName_63Hz) = soundValues(0)
            soundDataRow(SoundColumnName_125Hz) = soundValues(1)
            soundDataRow(SoundColumnName_250Hz) = soundValues(2)
            soundDataRow(SoundColumnName_500Hz) = soundValues(3)
            soundDataRow(SoundColumnName_1000Hz) = soundValues(4)
            soundDataRow(SoundColumnName_2000Hz) = soundValues(5)
            soundDataRow(SoundColumnName_4000Hz) = soundValues(6)
            soundDataRow(SoundColumnName_8000Hz) = soundValues(7)
            soundDataRow(SoundColumnName_LwA) = CLDataCentralCommon.CalculateTotalSound(soundValues)
            soundDataRow(SoundColumnName_Lp1) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP1.Value)
            soundDataRow(SoundColumnName_Lp2) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP2.Value)

            m_SoundPerformances.Rows.Add(soundDataRow)
        End If

        ' Sound Supply
        If dcHeatRecoveryModel.HasSoundData_Outlet Then

            soundDataRow = m_SoundPerformances.NewRow()

            If dcHeatRecoveryModel.CLSerie.Code = "7" AndAlso (dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "HCI" Or dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "FS") Then
                soundValues = dcHeatRecoveryModel.SoundData_InletItems
            Else
                soundValues = dcHeatRecoveryModel.SoundData_OutletItems
            End If


            soundDataRow(SoundColumnName_Type) = CLSoundPerformanceType._Supply.ToString()
            soundDataRow(SoundColumnName_Caption) = Environment.Localization.GetString(CLMessageResources.Sound_Supply.ToString())
            soundDataRow(SoundColumnName_63Hz) = soundValues(0)
            soundDataRow(SoundColumnName_125Hz) = soundValues(1)
            soundDataRow(SoundColumnName_250Hz) = soundValues(2)
            soundDataRow(SoundColumnName_500Hz) = soundValues(3)
            soundDataRow(SoundColumnName_1000Hz) = soundValues(4)
            soundDataRow(SoundColumnName_2000Hz) = soundValues(5)
            soundDataRow(SoundColumnName_4000Hz) = soundValues(6)
            soundDataRow(SoundColumnName_8000Hz) = soundValues(7)
            soundDataRow(SoundColumnName_LwA) = CLDataCentralCommon.CalculateTotalSound(soundValues)
            soundDataRow(SoundColumnName_Lp1) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP1.Value)
            soundDataRow(SoundColumnName_Lp2) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP2.Value)

            m_SoundPerformances.Rows.Add(soundDataRow)

        End If

        ' Sound Exhaust
        If dcHeatRecoveryModel.HasSoundData_Outlet Then

            soundDataRow = m_SoundPerformances.NewRow()
            soundValues = dcHeatRecoveryModel.SoundData_OutletItems

            For counter As Integer = 0 To soundValues.Length - 1
                soundValues(counter) *= 1.01D
            Next

            soundDataRow(SoundColumnName_Type) = CLSoundPerformanceType._Exhaust.ToString()
            soundDataRow(SoundColumnName_Caption) = Environment.Localization.GetString(CLMessageResources.Sound_Exhaust.ToString())
            soundDataRow(SoundColumnName_63Hz) = soundValues(0)
            soundDataRow(SoundColumnName_125Hz) = soundValues(1)
            soundDataRow(SoundColumnName_250Hz) = soundValues(2)
            soundDataRow(SoundColumnName_500Hz) = soundValues(3)
            soundDataRow(SoundColumnName_1000Hz) = soundValues(4)
            soundDataRow(SoundColumnName_2000Hz) = soundValues(5)
            soundDataRow(SoundColumnName_4000Hz) = soundValues(6)
            soundDataRow(SoundColumnName_8000Hz) = soundValues(7)
            soundDataRow(SoundColumnName_LwA) = CLDataCentralCommon.CalculateTotalSound(soundValues)
            soundDataRow(SoundColumnName_Lp1) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP1.Value)
            soundDataRow(SoundColumnName_Lp2) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP2.Value)

            m_SoundPerformances.Rows.Add(soundDataRow)

        End If

        ' Sound Return
        If dcHeatRecoveryModel.HasSoundData_Inlet Then

            soundDataRow = m_SoundPerformances.NewRow()
            soundValues = dcHeatRecoveryModel.SoundData_InletItems

            For counter As Integer = 0 To soundValues.Length - 1
                soundValues(counter) *= 0.99D
            Next

            soundDataRow(SoundColumnName_Type) = CLSoundPerformanceType._Return.ToString()
            soundDataRow(SoundColumnName_Caption) = Environment.Localization.GetString(CLMessageResources.Sound_Return.ToString())
            soundDataRow(SoundColumnName_63Hz) = soundValues(0)
            soundDataRow(SoundColumnName_125Hz) = soundValues(1)
            soundDataRow(SoundColumnName_250Hz) = soundValues(2)
            soundDataRow(SoundColumnName_500Hz) = soundValues(3)
            soundDataRow(SoundColumnName_1000Hz) = soundValues(4)
            soundDataRow(SoundColumnName_2000Hz) = soundValues(5)
            soundDataRow(SoundColumnName_4000Hz) = soundValues(6)
            soundDataRow(SoundColumnName_8000Hz) = soundValues(7)
            soundDataRow(SoundColumnName_LwA) = CLDataCentralCommon.CalculateTotalSound(soundValues)
            soundDataRow(SoundColumnName_Lp1) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP1.Value)
            soundDataRow(SoundColumnName_Lp2) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP2.Value)

            m_SoundPerformances.Rows.Add(soundDataRow)

        End If


        Dim modelDimensions As CLEnvironment.CLModelDimension() = Environment.FindModelDimensionsByModel(dcHeatRecoveryModel)

        If dcHeatRecoveryModel.HasSoundData_Outlet Then

            Dim k As Double = 0

            If Not modelDimensions Is Nothing _
            AndAlso modelDimensions.Length > 0 Then

                ' Sound Breakout
                For Each modelDimension As CLEnvironment.CLModelDimension In modelDimensions

                    Dim section As Double = modelDimension.DimensionA * modelDimension.DimensionC
                    Dim wallAndBase As Double = (modelDimension.DimensionA + modelDimension.DimensionB) * modelDimension.DimensionC * 2 + modelDimension.DimensionB * modelDimension.DimensionA
                    Dim knew As Double = 10 * Math.Log10(wallAndBase / section)

                    If knew > k Then
                        k = knew
                    End If
                Next

            Else
                Dim section As Double = 500 * 500
                Dim wallAndBase As Double = (500 + 500) * 500 * 2 + 500 * 500
                Dim knew As Double = 10 * Math.Log10(wallAndBase / section)
                If knew > k Then
                    k = knew
                End If
            End If


            soundDataRow = m_SoundPerformances.NewRow()



            If dcHeatRecoveryModel.CLSerie.Code = "7" AndAlso (dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "HCI" Or dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "FS") Then
                soundValues = dcHeatRecoveryModel.SoundData_InletItems
                For counter As Integer = 0 To soundValues.Length - 1
                    soundValues(counter) = soundValues(counter)
                Next

            ElseIf dcHeatRecoveryModel.Code.Contains("QUARK 025") Then
                soundValues = New Double(7) {}
                soundValues(0) = 54.7
                soundValues(1) = 50.7
                soundValues(2) = 35.8
                soundValues(3) = 36.8
                soundValues(4) = 34.8
                soundValues(5) = 30.8
                soundValues(6) = 27.9
                soundValues(7) = 19.9
            ElseIf dcHeatRecoveryModel.Code.Contains("QUARK 035") Then
                soundValues = New Double(7) {}
                Dim corr As Double
                corr = 1.145
                soundValues(0) = 54.7 * corr
                soundValues(1) = 50.7 * corr
                soundValues(2) = 35.8 * corr
                soundValues(3) = 36.8 * corr
                soundValues(4) = 34.8 * corr
                soundValues(5) = 30.8 * corr
                soundValues(6) = 27.9 * corr
                soundValues(7) = 19.9 * corr
            Else
                soundValues = dcHeatRecoveryModel.SoundData_OutletItems
                For counter As Integer = 0 To soundValues.Length - 1
                    soundValues(counter) = soundValues(counter) - fonoIsolamento(counter) + k
                Next
            End If



            soundDataRow(SoundColumnName_Type) = CLSoundPerformanceType._Breakout.ToString()
            soundDataRow(SoundColumnName_Caption) = Environment.Localization.GetString(CLMessageResources.Sound_Breakout.ToString())
            soundDataRow(SoundColumnName_63Hz) = soundValues(0)
            soundDataRow(SoundColumnName_125Hz) = soundValues(1)
            soundDataRow(SoundColumnName_250Hz) = soundValues(2)
            soundDataRow(SoundColumnName_500Hz) = soundValues(3)
            soundDataRow(SoundColumnName_1000Hz) = soundValues(4)
            soundDataRow(SoundColumnName_2000Hz) = soundValues(5)
            soundDataRow(SoundColumnName_4000Hz) = soundValues(6)
            soundDataRow(SoundColumnName_8000Hz) = soundValues(7)
            soundDataRow(SoundColumnName_LwA) = CLDataCentralCommon.CalculateTotalSound(soundValues)
            soundDataRow(SoundColumnName_Lp1) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP1.Value)
            soundDataRow(SoundColumnName_Lp2) = soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP2.Value)

            m_SoundPerformances.Rows.Add(soundDataRow)

        End If





        For Each soundPerformance As DataRow In m_SoundPerformances.Rows

            Dim soundData() As Double = {0, 0, 0, 0, 0, 0, 0, 0}
            Dim forceAbs As Boolean = soundPerformance(SoundColumnName_Type) = CLSoundPerformanceType._Breakout.ToString()

            If soundPerformance(SoundColumnName_Type) <> CLSoundPerformanceType._Frisse.ToString() Then
                soundPerformance(SoundColumnName_63Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_63Hz), 1, forceAbs)
                soundPerformance(SoundColumnName_125Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_125Hz), 1, forceAbs)
                soundPerformance(SoundColumnName_250Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_250Hz), 1, forceAbs)
                soundPerformance(SoundColumnName_500Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_500Hz), 1, forceAbs)
                soundPerformance(SoundColumnName_1000Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_1000Hz), 1, forceAbs)
                soundPerformance(SoundColumnName_2000Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_2000Hz), 1, forceAbs)
                soundPerformance(SoundColumnName_4000Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_4000Hz), 1, forceAbs)
                soundPerformance(SoundColumnName_8000Hz) = AdjustSoundValue(dcHeatRecoveryModel, soundPerformance(SoundColumnName_8000Hz), 1, forceAbs)

                soundData(0) = soundPerformance(SoundColumnName_63Hz)
                soundData(1) = soundPerformance(SoundColumnName_125Hz)
                soundData(2) = soundPerformance(SoundColumnName_250Hz)
                soundData(3) = soundPerformance(SoundColumnName_500Hz)
                soundData(4) = soundPerformance(SoundColumnName_1000Hz)
                soundData(5) = soundPerformance(SoundColumnName_2000Hz)
                soundData(6) = soundPerformance(SoundColumnName_4000Hz)
                soundData(7) = soundPerformance(SoundColumnName_8000Hz)

                soundPerformance(SoundColumnName_LwA) = CLDataCentralCommon.CalculateTotalSound(soundData)
                soundPerformance(SoundColumnName_Lp1) = Math.Round(soundPerformance(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP1.Value), 1)
                soundPerformance(SoundColumnName_Lp2) = Math.Round(soundPerformance(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP2.Value), 1)
            End If

        Next

        ' Sound FS
        If dcHeatRecoveryModel.HasSoundData_Inlet And chbSoundPerformances_16032.Checked Then

            soundDataRow = m_SoundPerformances.NewRow()
            Dim soundPerformance As DataRow = m_SoundPerformances.Rows(1)
            soundValues = dcHeatRecoveryModel.SoundData_InletItems

            soundValues(0) = soundPerformance(SoundColumnName_63Hz) - 7
            soundValues(1) = soundPerformance(SoundColumnName_125Hz) - 7
            soundValues(2) = soundPerformance(SoundColumnName_250Hz) - 7
            soundValues(3) = soundPerformance(SoundColumnName_500Hz) - 7
            soundValues(4) = soundPerformance(SoundColumnName_1000Hz) - 7
            soundValues(5) = soundPerformance(SoundColumnName_2000Hz) - 7
            soundValues(6) = soundPerformance(SoundColumnName_4000Hz) - 7
            soundValues(7) = soundPerformance(SoundColumnName_8000Hz) - 7


            soundDataRow(SoundColumnName_Type) = CLSoundPerformanceType._Frisse.ToString()
            soundDataRow(SoundColumnName_Caption) = "Lp@EN-ISO16032" 'Environment.Localization.GetString(CLMessageResources.Sound_Supply.ToString())
            soundDataRow(SoundColumnName_63Hz) = soundValues(0)
            soundDataRow(SoundColumnName_125Hz) = soundValues(1)
            soundDataRow(SoundColumnName_250Hz) = soundValues(2)
            soundDataRow(SoundColumnName_500Hz) = soundValues(3)
            soundDataRow(SoundColumnName_1000Hz) = soundValues(4)
            soundDataRow(SoundColumnName_2000Hz) = soundValues(5)
            soundDataRow(SoundColumnName_4000Hz) = soundValues(6)
            soundDataRow(SoundColumnName_8000Hz) = soundValues(7)
            soundDataRow(SoundColumnName_LwA) = CLDataCentralCommon.CalculateTotalSound(soundValues)
            soundDataRow(SoundColumnName_Lp1) = "" 'soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP1.Value)
            soundDataRow(SoundColumnName_Lp2) = "" 'soundDataRow(SoundColumnName_LwA) - 11 + 10 * Math.Log10(directivity) - 20 * Math.Log10(nudSoundPerformances_LP2.Value)

            m_SoundPerformances.Rows.Add(soundDataRow)

        End If


        m_SoundPerformances.AcceptChanges()

    End Sub

    Private Function AdjustSoundValue(dcHeatRecoveryModel As CLDCHeatRecoveryModel,
        ByRef value As Double,
        nDecimal As Integer,
        forceAbs As Boolean) As Double

        Dim newValue As Double
        newValue = Math.Round(sound_power_correction(hsbPerformance_RegulationLevel.Value,
                dcHeatRecoveryModel.AirflowsItems.Max(), dcHeatRecoveryModel.PressuresItems.Max(),
                IIf(Double.Parse(txbPerformance_AirFlow.Text) = 0, 1, Double.Parse(txbPerformance_AirFlow.Text)),
                IIf(Double.Parse(txbPerformance_MaxPressure.Text) = 0, 1, Double.Parse(txbPerformance_MaxPressure.Text)),
                value), nDecimal)

        If forceAbs Then
            newValue = Math.Abs(newValue)
        End If


        Return newValue
    End Function

    Private Function ParseUIDouble(value As Object, Optional fallbackValue As Double = 0) As Double
        Dim text As String = If(value Is Nothing, "", value.ToString()).Trim()
        Dim result As Double
        Dim numberStyle As NumberStyles = NumberStyles.Float Or NumberStyles.AllowThousands

        text = text.Replace(ChrW(&HA0), " "c).Replace(" "c, "")

        If Double.TryParse(text, numberStyle, CultureInfo.CurrentCulture, result) Then
            Return result
        End If
        If Double.TryParse(text, numberStyle, CultureInfo.GetCultureInfo("it-IT"), result) Then
            Return result
        End If
        If Double.TryParse(text, numberStyle, CultureInfo.InvariantCulture, result) Then
            Return result
        End If
        If Double.TryParse(text.Replace("."c, ","c), numberStyle, CultureInfo.GetCultureInfo("it-IT"), result) Then
            Return result
        End If
        If Double.TryParse(text.Replace(","c, "."c), numberStyle, CultureInfo.InvariantCulture, result) Then
            Return result
        End If

        Return fallbackValue
    End Function

    Private Function Calculate_ThermalScenario(freshTemperatureTextBox As TextBox,
        freshRHTextBox As TextBox,
        returnTemperatureTextBox As TextBox,
        returnRHTextBox As TextBox,
        airflowTextBox As TextBox,
        maxPressureTextBox As TextBox,
        maxPressureFallback As Double,
        additionalPressureDrop As Double,
        drawCharts As Boolean,
        Optional preheatPowerW As Double = 0) As CLThermalCalculationResult

        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel
        Dim ritm As Double = ParseUIDouble(returnTemperatureTextBox.Text)
        Dim rhritmPercent As Double = ParseUIDouble(returnRHTextBox.Text)
        Dim fitm As Double = ParseUIDouble(freshTemperatureTextBox.Text)
        Dim rhfitmPercent As Double = ParseUIDouble(freshRHTextBox.Text)
        Dim afm As Double = ParseUIDouble(airflowTextBox.Text)
        Dim maxPressure As Double = ParseUIDouble(maxPressureTextBox.Text, maxPressureFallback)
        Dim chart1 As Chart = If(drawCharts, crtPerformance_Chart1, New Chart())
        Dim chart2 As Chart = If(drawCharts, crtPerformance_Chart2, New Chart())
        Dim chart3 As Chart = If(drawCharts, crtPerformance_Chart3, New Chart())

        If Not drawCharts Then
            chart1.ChartAreas.Add(New ChartArea())
            chart2.ChartAreas.Add(New ChartArea())
            chart3.ChartAreas.Add(New ChartArea())
        End If

        Dim scenario As New CLSeasonCalculationInput With {
            .Enabled = True,
            .ScenarioCode = If(drawCharts, "Winter", "Summer"),
            .Airflows = CLAirflowPair.Balanced(afm),
            .MaximumPressurePa = CLBranchValuePair.Balanced(maxPressure),
            .OutdoorTemperatureC = fitm,
            .OutdoorRelativeHumidityPercent = rhfitmPercent,
            .ReturnTemperatureC = ritm,
            .ReturnRelativeHumidityPercent = rhritmPercent,
            .RegulationPercent = prbPerformance_RegulationLevel.Value
        }
        Dim applicationCalculation As CLBalancedScenarioCalculation =
            CLSelectionApplicationService.CalculateBalancedScenario(New CLBalancedScenarioCalculationRequest With {
                .Scenario = scenario,
                .Model = dcHeatRecoveryModel,
                .MeasureUnit = MeasureUnit,
                .AdditionalPressureDropPa = additionalPressureDrop,
                .PreheatPowerW = preheatPowerW,
                .ShowSfpArea = drawCharts AndAlso (chbPerformance_SFP_ShowArea.Checked OrElse chbPerformance_SEL_ShowArea.Checked),
                .ShowErpArea = drawCharts AndAlso chbPerformance_ERP2018_ShowArea.Checked,
                .SfpLimit = nudPerformance_SFP_Limit.Value,
                .ShowPassiveHouseArea = drawCharts AndAlso chbPerformance_PassiveHaus_ShowArea.Checked,
                .PassiveHouseLimit = ParseUIDouble(txbPerformance_PassiveHaus_Limit.Text)
            })

        Dim workpoint As Double() = curva(MeasureUnit,
           If(afm = 0, 1, afm),
           dcHeatRecoveryModel,
           ritm,
           rhritmPercent / 100,
           fitm,
           rhfitmPercent / 100,
           prbPerformance_RegulationLevel.Value,
           chart1,
           chart2,
           chart3,
           maxPressure,
           drawCharts AndAlso (chbPerformance_SFP_ShowArea.Checked OrElse chbPerformance_SEL_ShowArea.Checked),
           drawCharts AndAlso chbPerformance_ERP2018_ShowArea.Checked,
           nudPerformance_SFP_Limit.Value,
           drawCharts AndAlso chbPerformance_PassiveHaus_ShowArea.Checked,
           ParseUIDouble(txbPerformance_PassiveHaus_Limit.Text),
           additionalPressureDrop,
           If(m_MeasureUnit = CLMeasureUnit.IP, afm * 3.6, afm),
           applicationCalculation.Curves)

        Return New CLThermalCalculationResult With {
            .Thermo = applicationCalculation.LegacyThermodynamics,
            .WorkPoint = workpoint,
            .AirFlow = afm,
            .Curves = applicationCalculation.Curves
        }
    End Function

    Private Sub Write_ThermalOutputs(result As CLThermalCalculationResult,
        heatTransferredTextBox As TextBox,
        sensibleHeatTextBox As TextBox,
        latentHeatTextBox As TextBox,
        waterProducedTextBox As TextBox,
        efficiencyTextBox As TextBox,
        supplyTemperatureTextBox As TextBox,
        supplyRHTextBox As TextBox,
        exhaustTemperatureTextBox As TextBox,
        exhaustRHTextBox As TextBox)

        Dim thermoWork As termo = result.Thermo

        supplyTemperatureTextBox.Text = FormatNumber(Math.Round(thermoWork.Supply_outlet_temp, 1), 1)
        supplyRHTextBox.Text = FormatNumber(Math.Round(100 * thermoWork.Supply_outlet_rh, 0), 0)
        exhaustTemperatureTextBox.Text = FormatNumber(Math.Round(thermoWork.Exhaust_outlet_temp, 1), 1)
        exhaustRHTextBox.Text = FormatNumber(Math.Round(100 * thermoWork.Exhaust_outlet_rh, 0), 0)
        heatTransferredTextBox.Text = FormatNumber(thermoWork.heat_recovery, 0)
        sensibleHeatTextBox.Text = FormatNumber(thermoWork.sensible_heat, 0)
        latentHeatTextBox.Text = FormatNumber(thermoWork.latent_heat, 0)
        waterProducedTextBox.Text = FormatNumber(thermoWork.water_produced)
        efficiencyTextBox.Text = FormatNumber(100 * thermoWork.efficiency, 0)
    End Sub

    Private Sub Add_SummerEfficiencyCurve(summerResult As CLThermalCalculationResult)
        If crtPerformance_Chart3 Is Nothing OrElse crtPerformance_Chart3.Series Is Nothing Then
            Return
        End If

        If summerResult Is Nothing OrElse summerResult.Curves Is Nothing OrElse
           summerResult.Curves.OriginalAirflows Is Nothing OrElse
           summerResult.Curves.EfficienciesPercent Is Nothing Then
            Return
        End If

        Dim curveName As String = ChartSeries_SummerEfficiencyCurve_Name
        Dim pointName As String = ChartSeries_SummerEfficiencyPoint_Name

        If crtPerformance_Chart3.Series.FindByName(curveName) IsNot Nothing Then
            crtPerformance_Chart3.Series.Remove(crtPerformance_Chart3.Series(curveName))
        End If
        If crtPerformance_Chart3.Series.FindByName(pointName) IsNot Nothing Then
            crtPerformance_Chart3.Series.Remove(crtPerformance_Chart3.Series(pointName))
        End If

        Dim xValues As New List(Of Double)(summerResult.Curves.OriginalAirflows)
        Dim yValues As New List(Of Double)(summerResult.Curves.EfficienciesPercent)

        Dim summerArea As ChartArea = Chart_ConfigureSplitEfficiencyAreas(crtPerformance_Chart3)

        Dim curveSeries As Series = crtPerformance_Chart3.Series.Add(curveName)
        curveSeries.ChartArea = summerArea.Name
        curveSeries.ChartType = SeriesChartType.Line
        curveSeries.Points.DataBindXY(xValues.ToArray(), yValues.ToArray())
        curveSeries.BorderWidth = 2
        curveSeries.Color = Color.SeaGreen

        Dim pointSeries As Series = crtPerformance_Chart3.Series.Add(pointName)
        pointSeries.ChartArea = summerArea.Name
        pointSeries.ChartType = SeriesChartType.Point
        pointSeries.Points.DataBindXY(New Double() {summerResult.WorkPoint(1)}, New Double() {100 * summerResult.Thermo.efficiency})
        pointSeries.MarkerSize = 8
        pointSeries.MarkerStyle = MarkerStyle.Circle
        pointSeries.MarkerBorderColor = Color.White
        pointSeries.MarkerBorderWidth = 2
        pointSeries.Color = Color.SeaGreen

        Chart_ApplySummerEfficiencyStyle(crtPerformance_Chart3)
        Chart_ApplyHighQualityScreenRendering(crtPerformance_Chart3)
    End Sub

    Private Sub Clear_SummerThermalOutputs()
        TextBox7.Text = ""
        TextBox8.Text = ""
        TextBox9.Text = ""
        TextBox10.Text = ""
        TextBox11.Text = ""
        TextBox12.Text = ""
        TextBox13.Text = ""
        TextBox14.Text = ""
        TextBox15.Text = ""
        TextBox2.Text = ""
        m_HasLastSummerThermo = False
    End Sub

    Private Sub Calculate_Data(Optional additionalPressureDrop As Double = 0,
        Optional maxPressureOverride As Double? = Nothing,
        Optional winterPreheatPowerW As Double = 0)
        Try
            Dim winterMaxPressure As Double = If(maxPressureOverride.HasValue, maxPressureOverride.Value, ParseUIDouble(txbPerformance_MaxPressure.Text))
            Dim winterResult As CLThermalCalculationResult = Calculate_ThermalScenario(
                txbPerformance_FreshInletTemperature,
                txbPerformance_RHFreshInlet,
                txbPerformance_ReturnInletTemperature,
                txbPerformance_RHReturnInlet,
                txbPerformance_AirFlow,
                txbPerformance_MaxPressure,
                winterMaxPressure,
                additionalPressureDrop,
                True,
                winterPreheatPowerW)

            Write_ThermalOutputs(
                winterResult,
                txbPerformance_HeatTransferred,
                txbPerformance_SensibleHeat,
                txbPerformance_LatentHeat,
                txbPerformance_WaterProduced,
                lblPerformance_Efficiency,
                txbPerformance_SupplyOutletTemperature,
                txbPerformance_SupplyOutletRH,
                txbPerformance_ExhaustOutletTemperature,
                txbPerformance_ExhaustOutletRH)

            m_LastWinterThermo = winterResult.Thermo
            m_HasLastWinterThermo = True

            txbPerformance_MaxPressure.Text = FormatNumber(Math.Floor(winterResult.WorkPoint(2)), 0)

            If winterResult.WorkPoint(3) <= 5 Then
                winterResult.WorkPoint(3) = 5
            End If

            txbPerformance_ElectricalPerformances_PowerInput.Text = FormatNumber(Math.Round(winterResult.WorkPoint(3)), 0)

            Select Case m_MeasureUnit
                Case CLMeasureUnit.IP
                    txbPerformance_AirFlow.Text = FormatNumber(winterResult.WorkPoint(1), 1)
                    txbPerformance_SFP.Text = FormatNumber((winterResult.WorkPoint(3) * 2) / winterResult.WorkPoint(1))
                    txbPerformance_SEL.Text = FormatNumber(((winterResult.WorkPoint(3) * 2) / winterResult.WorkPoint(1)) * 1000, 0)
                    txbPerformance_PassiveHaus.Text = FormatNumber(winterResult.WorkPoint(3) / (winterResult.WorkPoint(1) * 3.6))
                Case CLMeasureUnit.SI
                    txbPerformance_AirFlow.Text = FormatNumber(winterResult.WorkPoint(1), 0)
                    txbPerformance_SFP.Text = FormatNumber((winterResult.WorkPoint(3) * 2) * 3.6 / winterResult.WorkPoint(1))
                    txbPerformance_SEL.Text = FormatNumber(((winterResult.WorkPoint(3) * 2) * 3.6 / winterResult.WorkPoint(1)) * 1000, 0)
                    txbPerformance_PassiveHaus.Text = FormatNumber((winterResult.WorkPoint(3) * 2) / winterResult.WorkPoint(1))
            End Select

            If Not m_SummerCalculationEnabled Then
                Clear_SummerThermalOutputs()
                Chart_ConfigureSingleEfficiencyArea(crtPerformance_Chart3)
                Return
            End If

            If String.IsNullOrWhiteSpace(TextBox1.Text) Then
                TextBox1.Text = txbPerformance_AirFlow.Text
            End If
            If String.IsNullOrWhiteSpace(TextBox2.Text) Then
                TextBox2.Text = FormatNumber(winterMaxPressure, 0)
            End If

            Dim summerResult As CLThermalCalculationResult = Calculate_ThermalScenario(
                TextBox3,
                TextBox4,
                TextBox5,
                TextBox6,
                TextBox1,
                TextBox2,
                winterMaxPressure,
                additionalPressureDrop,
                False)

            Write_ThermalOutputs(
                summerResult,
                TextBox10,
                TextBox9,
                TextBox7,
                TextBox8,
                TextBox11,
                TextBox12,
                TextBox14,
                TextBox13,
                TextBox15)

            m_LastSummerThermo = summerResult.Thermo
            m_HasLastSummerThermo = True
            TextBox2.Text = FormatNumber(Math.Floor(summerResult.WorkPoint(2)), 0)
            TextBox1.Text = If(m_MeasureUnit = CLMeasureUnit.IP, FormatNumber(summerResult.WorkPoint(1), 1), FormatNumber(summerResult.WorkPoint(1), 0))
            Add_SummerEfficiencyCurve(summerResult)
        Catch exception As Exception
            MessageBox.Show(Me, exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub Calculate_CO2Level()

        Dim volume As Double = 0
        Dim area As Double = 0
        Dim co2persona As Double = 0
        Dim personepresenti As Double = 0
        Dim personebreak As Double = 0
        Dim persone As Double = 0
        Dim af_richiesto As Double = 0
        Dim maxco2 As Double = 0
        Dim af_persone As Double = 0
        Dim af_area As Double = 0


        personepresenti = txbCO2Level_Usage_PeoplePresence.Text
        personebreak = txbCO2Level_Usage_PeopleBreak.Text

        persone = Math.Max(personebreak, personepresenti)

        volume = calc_volume_litri(txbCO2Level_Room_Width.Text, txbCO2Level_Room_Length.Text, txbCO2Level_Room_Height.Text)
        area = calc_superf_m2(txbCO2Level_Room_Width.Text, txbCO2Level_Room_Length.Text)

        co2persona = co2prodpersona(txbCO2Level_Usage_Activity.Text)

        'MaxC02
        If cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0 Then

            maxco2 = txbCO2Level_Parameters_maxCO2.Text
            af_richiesto = airflowdemand(txbCO2Level_Parameters_extCO2.Text, maxco2, persone, co2persona)
            txbCO2Level_Parameters_airflow.Text = FormatNumber(af_richiesto, 1)

        End If

        'Fixed Flow Rated
        If cmbCO2Level_Parameters_CalcMet.SelectedIndex = 1 Then

            af_richiesto = txbCO2Level_Parameters_airflow.Text
            maxco2 = 1000000 * persone * co2persona / (3600 * af_richiesto) + txbCO2Level_Parameters_extCO2.Text
            txbCO2Level_Parameters_maxCO2.Text = FormatNumber(maxco2, 0)
        End If

        'Person Related
        If cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2 Then

            af_area = txbCO2Level_Parameters_af_area.Text
            af_persone = txbCO2Level_Parameters_af_person.Text
            af_richiesto = (af_area * area + af_persone * persone)
            maxco2 = 1000000 * persone * co2persona / (3600 * af_richiesto) + txbCO2Level_Parameters_extCO2.Text
            txbCO2Level_Parameters_maxCO2.Text = FormatNumber(maxco2, 0)
            txbCO2Level_Parameters_airflow.Text = FormatNumber(af_richiesto, 1)
        End If

        txbCO2Level_Usage_CO2prod.Text = FormatNumber(co2persona, 1)

        txbCO2Level_Parameters_airflow.Text = FormatNumber(af_richiesto, 2)

        co2time(volume, txbCO2Level_Parameters_extCO2.Text, maxco2, af_richiesto, txbCO2Level_Usage_PeopleBreak.Text, txbCO2Level_Usage_PeoplePresence.Text, txbCO2Level_Usage_PeriodBreak.Text, txbCO2Level_Usage_PeriodPresence.Text, crtCO2Level_Chart1)



    End Sub

#End Region


#Region "====[ CO2Level ]===="

    Private Sub CO2LevelFill()

        'Valori di Default
        txbCO2Level_Room_Height.Text = 3
        txbCO2Level_Room_Length.Text = 8
        txbCO2Level_Room_Width.Text = 7

        txbCO2Level_Usage_Activity.Text = 1.2
        txbCO2Level_Usage_PeopleBreak.Text = 0
        txbCO2Level_Usage_PeoplePresence.Text = 20
        txbCO2Level_Usage_PeriodBreak.Text = 15
        txbCO2Level_Usage_PeriodPresence.Text = 45

        txbCO2Level_Parameters_af_area.Text = 0.35
        txbCO2Level_Parameters_af_person.Text = 10
        txbCO2Level_Parameters_maxCO2.Text = 1000
        txbCO2Level_Parameters_extCO2.Text = 380

        If cmbCO2Level_Parameters_CalcMet.Items.Count > 0 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0
        End If

        If cmbCO2Level_Parameters_stdpreset.Items.Count > 0 Then
            cmbCO2Level_Parameters_stdpreset.SelectedIndex = 0
        End If

        co2level_std_preset_change()

        co2level_calc_meth_change()

        checkworkingpoint()

    End Sub

    Private Sub cmbCO2Level_Parameters_stdpreset_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbCO2Level_Parameters_stdpreset.SelectedIndexChanged
        co2level_std_preset_change()
    End Sub

    Private Sub cmbCO2Level_Parameters_CalcMet_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbCO2Level_Parameters_CalcMet.SelectedIndexChanged
        co2level_calc_meth_change()
    End Sub

    Private Sub co2level_std_preset_change()

        'None
        'default Max CO2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 0 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0
        End If

        'Building bulletin 101 : Max 1000 ppm CO2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 1 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0
            txbCO2Level_Parameters_maxCO2.Text = 1000
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat I,B1 : 10 l/s pers & 0,5 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 2 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 10
            txbCO2Level_Parameters_af_area.Text = 0.5
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat I,B2 : 10 l/s pers & 1,0 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 3 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 10
            txbCO2Level_Parameters_af_area.Text = 1
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat I,B3 : 10 l/s pers & 2,0 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 4 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 10
            txbCO2Level_Parameters_af_area.Text = 2
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat II,B1 : 7 l/s pers & 0,35 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 5 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 7
            txbCO2Level_Parameters_af_area.Text = 0.35
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat II,B2 : 7 l/s pers & 0,7 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 6 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 7
            txbCO2Level_Parameters_af_area.Text = 0.7
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat II,B3 : 7 l/s pers & 1,4 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 7 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 7
            txbCO2Level_Parameters_af_area.Text = 1.4
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat III,B1 : 4 l/s pers & 0,2 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 8 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 4
            txbCO2Level_Parameters_af_area.Text = 0.2
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat III,B2 : 4 l/s pers & 0,4 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 9 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 4
            txbCO2Level_Parameters_af_area.Text = 0.4
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat III,B3 : 4 l/s pers & 0,8 l/s m2
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 10 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2
            txbCO2Level_Parameters_af_person.Text = 4
            txbCO2Level_Parameters_af_area.Text = 0.8
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat I : C02 max 700 ppm
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 11 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0
            txbCO2Level_Parameters_maxCO2.Text = 700
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat II : C02 max 850 ppm
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 12 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0
            txbCO2Level_Parameters_maxCO2.Text = 850
            Calculate_CO2Level()
            checkworkingpoint()
        End If

        'EN15251 cat III : C02 max 1150 ppm
        If cmbCO2Level_Parameters_stdpreset.SelectedIndex = 13 Then
            cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0
            txbCO2Level_Parameters_maxCO2.Text = 1150
            Calculate_CO2Level()
            checkworkingpoint()
        End If

    End Sub

    Private Sub co2level_calc_meth_change()


        'Max CO2
        If cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0 Then
            txbCO2Level_Parameters_af_area.Visible = False
            txbCO2Level_Parameters_af_person.Visible = False
            lblCO2Level_Parameters_af_area.Visible = False
            lblCO2Level_Parameters_af_person.Visible = False
            txbCO2Level_Parameters_af_area_m3h.Visible = False
            txbCO2Level_Parameters_af_person_m3h.Visible = False
            lblCO2Level_Parameters_af_area_m3h.Visible = False
            lblCO2Level_Parameters_af_person_m3h.Visible = False

            txbCO2Level_Parameters_airflow.ReadOnly = True
            txbCO2Level_Parameters_airflow_m3h.ReadOnly = True
            txbCO2Level_Parameters_maxCO2.ReadOnly = False
        End If


        'Fixed Flow rated
        If cmbCO2Level_Parameters_CalcMet.SelectedIndex = 1 Then
            txbCO2Level_Parameters_af_area.Visible = False
            txbCO2Level_Parameters_af_person.Visible = False
            lblCO2Level_Parameters_af_area.Visible = False
            lblCO2Level_Parameters_af_person.Visible = False
            txbCO2Level_Parameters_af_area_m3h.Visible = False
            txbCO2Level_Parameters_af_person_m3h.Visible = False
            lblCO2Level_Parameters_af_area_m3h.Visible = False
            lblCO2Level_Parameters_af_person_m3h.Visible = False

            txbCO2Level_Parameters_airflow.ReadOnly = False
            txbCO2Level_Parameters_airflow_m3h.ReadOnly = False
            txbCO2Level_Parameters_maxCO2.ReadOnly = True
        End If


        'Person Related
        If cmbCO2Level_Parameters_CalcMet.SelectedIndex = 2 Then
            txbCO2Level_Parameters_af_area.Visible = True
            txbCO2Level_Parameters_af_person.Visible = True
            lblCO2Level_Parameters_af_area.Visible = True
            lblCO2Level_Parameters_af_person.Visible = True
            txbCO2Level_Parameters_af_area_m3h.Visible = True
            txbCO2Level_Parameters_af_person_m3h.Visible = True
            lblCO2Level_Parameters_af_area_m3h.Visible = True
            lblCO2Level_Parameters_af_person_m3h.Visible = True

            txbCO2Level_Parameters_airflow.ReadOnly = True
            txbCO2Level_Parameters_airflow_m3h.ReadOnly = True
            txbCO2Level_Parameters_maxCO2.ReadOnly = True
        End If

    End Sub

    Private Sub checkworkingpoint()

        Dim af_set As Double = 0
        Dim af As Double = 0

        txbCO2Level_Parameters_airflow_design.Text = FormatNumber(CDbl(txbPerformance_AirFlow.Text / 3.6), 1)
        txbCO2Level_Parameters_airflow_design_m3h.Text = txbPerformance_AirFlow.Text
        af_set = CDbl(txbCO2Level_Parameters_airflow_design_m3h.Text)
        af = CDbl(txbCO2Level_Parameters_airflow_m3h.Text)

        If chbCO2Level_Parameters_airflow_check.Checked Then
            lblCO2Level_Parameters_airflow_design.Visible = True
            lblCO2Level_Parameters_airflow_design_m3h.Visible = True
            txbCO2Level_Parameters_airflow_design.Visible = True
            txbCO2Level_Parameters_airflow_design_m3h.Visible = True
            txbCO2Level_Parameters_airflow_design.Text = FormatNumber(CDbl(txbPerformance_AirFlow.Text / 3.6), 1)
            txbCO2Level_Parameters_airflow_design_m3h.Text = txbPerformance_AirFlow.Text
        Else
            lblCO2Level_Parameters_airflow_design.Visible = False
            lblCO2Level_Parameters_airflow_design_m3h.Visible = False
            txbCO2Level_Parameters_airflow_design.Visible = False
            txbCO2Level_Parameters_airflow_design_m3h.Visible = False
        End If

        If af_set < af Then
            txbCO2Level_Parameters_airflow_design_m3h.BackColor = Color.Red
            txbCO2Level_Parameters_airflow_design_m3h.ForeColor = Color.White
            txbCO2Level_Parameters_airflow_design.BackColor = Color.Red
            txbCO2Level_Parameters_airflow_design.ForeColor = Color.White
        Else
            txbCO2Level_Parameters_airflow_design_m3h.BackColor = Color.Green
            txbCO2Level_Parameters_airflow_design_m3h.ForeColor = Color.White
            txbCO2Level_Parameters_airflow_design.BackColor = Color.Green
            txbCO2Level_Parameters_airflow_design.ForeColor = Color.White
        End If

    End Sub

    Private Sub txbCO2Level_Parameters_airflow_Changed(sender As Object, e As EventArgs) Handles txbCO2Level_Parameters_airflow.Validated, txbCO2Level_Parameters_airflow.TextChanged, txbCO2Level_Parameters_airflow_design.Validated, txbCO2Level_Parameters_airflow_design.TextChanged
        txbCO2Level_Parameters_airflow_m3h.Text = FormatNumber(CDbl(txbCO2Level_Parameters_airflow.Text * 3.6), 0)
        checkworkingpoint()
    End Sub

    Private Sub txbCO2Level_Parameters_af_person_Validated(sender As Object, e As EventArgs) Handles txbCO2Level_Parameters_af_person.Validated, txbCO2Level_Parameters_af_person.TextChanged
        txbCO2Level_Parameters_af_person_m3h.Text = FormatNumber(CDbl(txbCO2Level_Parameters_af_person.Text * 3.6), 0)
    End Sub

    Private Sub txbCO2Level_Parameters_af_area_Validated(sender As Object, e As EventArgs) Handles txbCO2Level_Parameters_af_area.Validated, txbCO2Level_Parameters_af_area.TextChanged
        txbCO2Level_Parameters_af_area_m3h.Text = FormatNumber(CDbl(txbCO2Level_Parameters_af_area.Text * 3.6), 1)
    End Sub

    Private Sub txbCO2Level_Parameters_airflow_m3h_Validated(sender As Object, e As EventArgs) Handles txbCO2Level_Parameters_airflow_m3h.Validated, txbCO2Level_Parameters_airflow_design_m3h.Validated
        txbCO2Level_Parameters_airflow.Text = FormatNumber(CDbl(txbCO2Level_Parameters_airflow_m3h.Text / 3.6), 1)
        checkworkingpoint()
    End Sub

    Private Sub txbCO2Level_Parameters_af_person_m3h_Validated(sender As Object, e As EventArgs) Handles txbCO2Level_Parameters_af_person_m3h.Validated
        txbCO2Level_Parameters_af_person.Text = FormatNumber(CDbl(txbCO2Level_Parameters_af_person_m3h.Text / 3.6), 1)
    End Sub

    Private Sub txbCO2Level_Parameters_af_area_m3h_Validated(sender As Object, e As EventArgs) Handles txbCO2Level_Parameters_af_area_m3h.Validated
        txbCO2Level_Parameters_af_area.Text = FormatNumber(CDbl(txbCO2Level_Parameters_af_area_m3h.Text / 3.6), 2)
    End Sub

    Private Sub chbCO2Level_Parameters_airflow_check_CheckedChanged(sender As Object, e As EventArgs) Handles chbCO2Level_Parameters_airflow_check.CheckedChanged
        checkworkingpoint()
    End Sub

    Private Sub txbCO2Level_Validated(sender As Object, e As EventArgs) Handles txbCO2Level_Room_Length.Validated, txbCO2Level_Room_Width.Validated, txbCO2Level_Room_Height.Validated, txbCO2Level_Parameters_af_area.Validated, txbCO2Level_Parameters_af_area_m3h.Validated, txbCO2Level_Parameters_af_person.Validated, txbCO2Level_Parameters_af_person_m3h.Validated, txbCO2Level_Usage_Activity.Validated, txbCO2Level_Usage_PeopleBreak.Validated, txbCO2Level_Usage_PeoplePresence.Validated, txbCO2Level_Usage_PeriodBreak.Validated, txbCO2Level_Usage_PeriodPresence.Validated, txbCO2Level_Parameters_extCO2.Validated, txbCO2Level_Parameters_maxCO2.Validated, txbCO2Level_Parameters_airflow_m3h.Validated, txbCO2Level_Parameters_airflow.Validated
        Calculate_CO2Level()
        checkworkingpoint()
    End Sub



#End Region


#Region "====[ UKUNDA Item code generator ]===="

    Private Sub ItemCodeGenerator()

        Dim Code As String
        Dim Descr As String

        Code = "I1030"


        If cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("40") Then
            Code = Code + "04"
        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("70") Then

            Code = Code + "07"
        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("120") Then

            Code = Code + "12"
        Else
            Code = "ERROR"
            Descr = "ERROR"
        End If

        If cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("FS") Then

            Code = Code + "5"

        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("HCI") Then

            Code = Code + "4"

        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("CFI") Then

            Code = Code + "3"
        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("CFD") Then

            Code = Code + "2"

        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("CDR") Then

            Code = Code + "1"
        Else
            Code = "ERROR"
            Descr = "ERROR"
        End If

        Descr = cmbPerformance_HeatRecoveryModels.SelectedItem.ToString


        If Code <> "ERROR" Then

            If rdbPerformance_CO2.Checked Then
                Code = Code + "1"
                Descr = Descr + " IDPC"
            ElseIf rdbPerformance_RH.Checked Then
                Code = Code + "2"
                Descr = Descr + " IDPH"
            ElseIf rdbPerformance_VOC.Checked Then
                Code = Code + "3"
                Descr = Descr + " IDPV"
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If

            If chbPerformance_MBUS.Checked = False And chbPerformance_RFM.Checked = False Then
                Code = Code + "0"
            ElseIf chbPerformance_MBUS.Checked = True And chbPerformance_RFM.Checked = False Then
                Code = Code + "1"
                Descr = Descr + " MBUS"
            ElseIf chbPerformance_MBUS.Checked = False And chbPerformance_RFM.Checked = True Then
                Code = Code + "2"
                Descr = Descr + " RFM"
            ElseIf chbPerformance_MBUS.Checked = True And chbPerformance_RFM.Checked = True Then
                Code = Code + "3"
                Descr = Descr + " MBUS RFM"
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If

            If chbPerformance_IPEHD.Checked = True Then
                Code = Code + "1"
                Descr = Descr + " IPEHD"
            Else
                Code = Code + "0"
            End If

            If rdbPerformance_none.Checked Then
                Code = Code + "0"
            ElseIf rdbPerformance_IHWD.Checked Then
                Code = Code + "1"
                Descr = Descr + " IHWD"
            ElseIf rdbPerformance_IEHD.Checked Then
                Code = Code + "2"
                Descr = Descr + " IEHD"
            ElseIf rdbPerformance_ICWD.Checked Then
                Code = Code + "3"
                Descr = Descr + " ICWD"
            ElseIf rdbPerformance_IHCD.Checked Then
                Code = Code + "4"
                Descr = Descr + " IHCD"
            ElseIf rdbPerformance_IDXD.Checked Then
                Code = Code + "5"
                Descr = Descr + " IDXD"
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If

        End If

        txbPerformance_ItemCode.Text = Code + "2"
        lblPerformance_ItemDescr.Text = Descr + " MF"

    End Sub

    Private Sub rdbPerformance_IHWD_CheckedChanged(sender As Object, e As EventArgs) Handles rdbPerformance_IHWD.CheckedChanged
        ItemCodeGenerator()
    End Sub


    Private Sub rdbPerformance_IEHD_CheckedChanged(sender As Object, e As EventArgs) Handles rdbPerformance_IEHD.CheckedChanged
        ItemCodeGenerator()
    End Sub

    Private Sub rdbPerformance_ICWD_CheckedChanged(sender As Object, e As EventArgs) Handles rdbPerformance_ICWD.CheckedChanged
        ItemCodeGenerator()
    End Sub

    Private Sub rdbPerformance_IHCD_CheckedChanged(sender As Object, e As EventArgs) Handles rdbPerformance_IHCD.CheckedChanged
        ItemCodeGenerator()
    End Sub

    Private Sub rdbPerformance_IDXD_CheckedChanged(sender As Object, e As EventArgs) Handles rdbPerformance_IDXD.CheckedChanged
        ItemCodeGenerator()
    End Sub

    Private Sub chbPerformance_IPEHD_CheckedChanged(sender As Object, e As EventArgs) Handles chbPerformance_IPEHD.CheckedChanged
        ItemCodeGenerator()
    End Sub


    Private Sub rdbPerformance_RH_CheckedChanged(sender As Object, e As EventArgs) Handles rdbPerformance_RH.CheckedChanged
        ItemCodeGenerator()
    End Sub

    Private Sub rdbPerformance_VOC_CheckedChanged(sender As Object, e As EventArgs) Handles rdbPerformance_VOC.CheckedChanged
        ItemCodeGenerator()
    End Sub

    Private Sub chbPerformance_MBUS_CheckedChanged(sender As Object, e As EventArgs) Handles chbPerformance_MBUS.CheckedChanged
        ItemCodeGenerator()
    End Sub

    Private Sub chbPerformance_RFM_CheckedChanged(sender As Object, e As EventArgs) Handles chbPerformance_RFM.CheckedChanged
        ItemCodeGenerator()
    End Sub



#End Region

#Region "====[ RAHU Item code generator ]===="
    Private Sub ItemCodeGeneratorQTM()

        Dim Code As String
        Dim Descr As String
        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel

        Code = "I10014"
        Descr = "prova"

        If cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("25") Then
            Code = Code + "2"
        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("35") Then

            Code = Code + "3"
        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("45") Then

            Code = Code + "4"
        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("55") Then

            Code = Code + "5"
        ElseIf cmbPerformance_HeatRecoveryModels.SelectedItem.ToString.Contains("65") Then

            Code = Code + "6"
        Else
            Code = "ERROR"
            Descr = "ERROR"
        End If

        Descr = cmbPerformance_HeatRecoveryModels.SelectedItem.ToString()

        If Code <> "ERROR" Then

            If rdb_qtm_ssc.Checked Then
                Code = Code + "0"
                Descr = Descr + " SSC"
            ElseIf rdb_qtm_osc.Checked Then
                Code = Code + "1"
                Descr = Descr + " OSC"
            ElseIf rdb_qtm_eos.Checked Then
                Code = Code + "2"
                Descr = Descr + " EOS"
            ElseIf rdb_qtm_fos.Checked Then
                Code = Code + "3"
                Descr = Descr + " FOS"
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If



            If rdb_qtm_premium.Checked Then
                Code = Code + "2"
                Descr = Descr + " P"
                chb_qtm_KTS.Checked = True
                chb_qtm_KTS.Enabled = False
                chb_qtm_ACTIVA.Checked = False
                chb_qtm_ACTIVA.Enabled = False
                chb_qtm_MBUS.Enabled = True
                chb_qtm_RFM.Enabled = True
            ElseIf rdb_qtm_preplus.Checked Then
                Code = Code + "3"
                Descr = Descr + " PP"
                chb_qtm_KTS.Checked = True
                chb_qtm_KTS.Enabled = False
                chb_qtm_ACTIVA.Checked = False
                chb_qtm_ACTIVA.Enabled = False
                chb_qtm_MBUS.Enabled = True
                chb_qtm_RFM.Enabled = True
            ElseIf rdb_qtm_easy.Checked Then
                Code = Code + "1"
                Descr = Descr + " EY"
                chb_qtm_KTS.Enabled = True
                chb_qtm_ACTIVA.Enabled = True
                chb_qtm_MBUS.Enabled = False
                chb_qtm_RFM.Enabled = False
                chb_qtm_MBUS.Checked = False
                chb_qtm_RFM.Checked = False
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If



            If rdb_qtm_co2.Checked Then
                Code = Code + "1"
                Descr = Descr + " IDPC"
            ElseIf rdb_qtm_rh.Checked Then
                Code = Code + "2"
                Descr = Descr + " IDPH"
            ElseIf rdb_qtm_voc.Checked Then
                Code = Code + "3"
                Descr = Descr + " IDPV"
            ElseIf rdb_qtm_sensnone.Checked Then
                Code = Code + "0"
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If


            If chb_qtm_IPEHD.Checked = True Then
                Code = Code + "1"
                Descr = Descr + " IPEHD"
            ElseIf chb_qtm_IPEHD.Checked = False Then
                Code = Code + "0"
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If


            If chb_qtm_KTS.Checked = False And chb_qtm_MBUS.Checked = False And chb_qtm_RFM.Checked = False And chb_qtm_ACTIVA.Checked = False Then
                Code = Code + "0"
            ElseIf chb_qtm_KTS.Checked = True And chb_qtm_MBUS.Checked = False And chb_qtm_RFM.Checked = False Then
                Code = Code + "1"
                Descr = Descr + " KTS"
            ElseIf chb_qtm_KTS.Checked = True And chb_qtm_MBUS.Checked = True And chb_qtm_RFM.Checked = False Then
                Code = Code + "2"
                Descr = Descr + " KTS MBUS"
            ElseIf chb_qtm_KTS.Checked = True And chb_qtm_MBUS.Checked = False And chb_qtm_RFM.Checked = True Then
                Code = Code + "3"
                Descr = Descr + " KTS RFM"
            ElseIf chb_qtm_KTS.Checked = True And chb_qtm_MBUS.Checked = True And chb_qtm_RFM.Checked = True Then
                Code = Code + "4"
                Descr = Descr + " KTS MBUS RFM"
            ElseIf chb_qtm_ACTIVA.Checked = True And chb_qtm_MBUS.Checked = False And chb_qtm_RFM.Checked = False And chb_qtm_KTS.Checked = False Then
                Code = Code + "5"
                Descr = Descr + " IOT"
            Else
                Code = "ERROR"
                Descr = "ERROR"
            End If

            txbPerformance_ItemCodeQTM.Text = Code + "0"
            If dcHeatRecoveryModel.CLEnumItem_AeraulicConnection.TextCode = "LT" Then
                lblPerformance_ItemDescrQTM.Text = Descr + " LPHE"
            Else
                lblPerformance_ItemDescrQTM.Text = Descr + " SP"
            End If


        End If

    End Sub


    Private Sub rdb_qtm_ssc_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_ssc.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_osc_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_osc.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_eos_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_eos.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_fos_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_fos.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub chb_qtm_IPEHD_CheckedChanged(sender As Object, e As EventArgs) Handles chb_qtm_IPEHD.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_co2_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_co2.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_voc_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_voc.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_rh_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_rh.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_sensnone_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_sensnone.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_premium_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_premium.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_preplus_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_preplus.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub chb_qtm_KTS_CheckedChanged(sender As Object, e As EventArgs) Handles chb_qtm_KTS.CheckedChanged
        If chb_qtm_KTS.Checked = True Then
            chb_qtm_ACTIVA.Checked = False
        End If
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub chb_qtm_MBUS_CheckedChanged(sender As Object, e As EventArgs) Handles chb_qtm_MBUS.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub chb_qtm_RFM_CheckedChanged(sender As Object, e As EventArgs) Handles chb_qtm_RFM.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub chb_qtm_ACTIVA_CheckedChanged(sender As Object, e As EventArgs) Handles chb_qtm_ACTIVA.CheckedChanged
        If chb_qtm_ACTIVA.Checked = True Then
            chb_qtm_KTS.Checked = False
        End If

        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub

    Private Sub rdb_qtm_easy_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_qtm_easy.CheckedChanged
        If Not IsNothing(cmbPerformance_Series.SelectedItem) And Not IsNothing(cmbPerformance_HeatRecoveryModels.SelectedItem) Then
            ItemCodeGeneratorQTM()
        End If
    End Sub
#End Region

    Private Sub SoundPerformances_UIDGVRefreshColumns()
        If dgvPerformance_SoundPower.Columns.Count > 0 Then
            dgvPerformance_SoundPower.Columns(SoundColumnName_Lp1).HeaderText = String.Format("Lp@{0}m", nudSoundPerformances_LP1.Value)
            dgvPerformance_SoundPower.Columns(SoundColumnName_Lp2).HeaderText = String.Format("Lp@{0}m", nudSoundPerformances_LP2.Value)
        End If
    End Sub

    Private Sub nudSoundPerformances_LP1_ValueChanged(sender As System.Object, e As System.EventArgs) Handles nudSoundPerformances_LP1.ValueChanged
        Calculate()
        SoundPerformances_UIDGVRefreshColumns()
    End Sub

    Private Sub nudSoundPerformances_LP2_ValueChanged(sender As System.Object, e As System.EventArgs) Handles nudSoundPerformances_LP2.ValueChanged
        Calculate()
        SoundPerformances_UIDGVRefreshColumns()
    End Sub

    Private m_SoundPerformances As DataTable

    Private Enum CLSoundPerformanceType
        _Return
        _Supply
        _Exhaust
        _Fresh
        _Breakout
        _Frisse
    End Enum

    Private Function SoundPerformance_GetLocalizedCaption(soundType As Object, fallbackCaption As Object) As String
        Dim resource As CLMessageResources

        Select Case Convert.ToString(soundType)
            Case CLSoundPerformanceType._Fresh.ToString()
                resource = CLMessageResources.Sound_Fresh
            Case CLSoundPerformanceType._Supply.ToString()
                resource = CLMessageResources.Sound_Supply
            Case CLSoundPerformanceType._Exhaust.ToString()
                resource = CLMessageResources.Sound_Exhaust
            Case CLSoundPerformanceType._Return.ToString()
                resource = CLMessageResources.Sound_Return
            Case CLSoundPerformanceType._Breakout.ToString()
                resource = CLMessageResources.Sound_Breakout
            Case Else
                Return Convert.ToString(fallbackCaption)
        End Select

        Dim localizedCaption = Environment.Localization.GetString(resource.ToString())
        If String.IsNullOrWhiteSpace(localizedCaption) Then
            Return Convert.ToString(fallbackCaption)
        End If

        Return localizedCaption
    End Function

    Private Const SoundColumnName_Type As String = "Type"
    Private Const SoundColumnName_Caption As String = "Caption"
    Private Const SoundColumnName_63Hz As String = "63Hz"
    Private Const SoundColumnName_125Hz As String = "125Hz"
    Private Const SoundColumnName_250Hz As String = "250Hz"
    Private Const SoundColumnName_500Hz As String = "500Hz"
    Private Const SoundColumnName_1000Hz As String = "1000Hz"
    Private Const SoundColumnName_2000Hz As String = "2000Hz"
    Private Const SoundColumnName_4000Hz As String = "4000Hz"
    Private Const SoundColumnName_8000Hz As String = "8000Hz"
    Private Const SoundColumnName_LwA As String = "LwA"
    Private Const SoundColumnName_Lp1 As String = "Lp1"
    Private Const SoundColumnName_Lp2 As String = "Lp2"

    Private Sub CreateSoundDataTable()

        m_SoundPerformances = New DataTable()

        m_SoundPerformances.Columns.Add(SoundColumnName_Type, GetType(String))
        m_SoundPerformances.Columns.Add(SoundColumnName_Caption, GetType(String))
        m_SoundPerformances.Columns.Add(SoundColumnName_63Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_125Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_250Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_500Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_1000Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_2000Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_4000Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_8000Hz, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_LwA, GetType(Double))
        m_SoundPerformances.Columns.Add(SoundColumnName_Lp1, GetType(String))
        m_SoundPerformances.Columns.Add(SoundColumnName_Lp2, GetType(String))

    End Sub

    Private Sub Branch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        If menuItem Is Nothing Then
            Return
        End If
        Dim branch As CLCustomerBranch = TryCast(menuItem.Tag, CLCustomerBranch)
        If branch Is Nothing Then
            Return
        End If

        Environment.Branch = branch

        Try
            tsmiFile_SaveCommercialSheet.Enabled = CommercialSheet_CanGenerate(Environment.PrimaryLanguageCode, Environment.Branch.ShortName)
        Catch ex As Exception

        End Try
        Try
            tsmiFile_SaveIOM.Enabled = IOM_CanGenerate(Environment.PrimaryLanguageCode, Environment.Branch.ShortName)

        Catch ex As Exception

        End Try


    End Sub

    Private Sub Environment_BranchChanged(ByVal sender As Object, ByVal e As EventArgs)
        For Each menuItem As ToolStripMenuItem In tsmiBranchs.DropDownItems

            If TryCast(menuItem.Tag, CLCustomerBranch) Is Environment.Branch Then
                menuItem.Checked = True
            Else
                menuItem.Checked = False
            End If

        Next
    End Sub

    Private Sub rdb_Q8_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_Q8.CheckedChanged
        Calculate()
    End Sub


    Private Sub rdb_Q4_CheckedChanged(sender As Object, e As EventArgs) Handles rdb_Q4.CheckedChanged
        Calculate()
    End Sub

    Private Sub Dlabel_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub chbSoundPerformances_16032_CheckedChanged(sender As Object, e As EventArgs) Handles chbSoundPerformances_16032.CheckedChanged
        Calculate()
    End Sub

    Private Sub SeasonalCalculation_UpdateModeButton()
        If btn_winter IsNot Nothing Then
            btn_winter.Visible = True
        End If

        If btn_summer IsNot Nothing Then
            btn_summer.BackColor = If(m_SummerCalculationEnabled, Color.LightGreen, SystemColors.Control)
            btn_summer.UseVisualStyleBackColor = Not m_SummerCalculationEnabled
        End If

        For Each control As Control In New Control() {TextBox1, TextBox2, TextBox3, TextBox4, TextBox5, TextBox6, GroupBox7}
            If control IsNot Nothing Then
                control.Enabled = m_SummerCalculationEnabled
            End If
        Next

        If Not m_SummerCalculationEnabled Then
            Clear_SummerThermalOutputs()
        End If
    End Sub

    Private Sub EN308_Click(sender As Object, e As EventArgs) Handles btnEN308.Click
        m_WinterReportScenarioName = btnEN308.Text
        txbPerformance_FreshInletTemperature.Text = 5
        txbPerformance_RHFreshInlet.Text = 72
        txbPerformance_ReturnInletTemperature.Text = 25
        txbPerformance_RHReturnInlet.Text = 28
        Calculate()
    End Sub

    Private Sub btn_Default_Click(sender As Object, e As EventArgs) Handles btn_winter.Click
        m_WinterReportScenarioName = String.Empty
        txbPerformance_FreshInletTemperature.Text = -10
        txbPerformance_RHFreshInlet.Text = 80
        txbPerformance_ReturnInletTemperature.Text = 20
        txbPerformance_RHReturnInlet.Text = 60
        TextBox3.Text = 32
        TextBox4.Text = 80
        TextBox5.Text = 26
        TextBox6.Text = 50
        Calculate()
    End Sub

    Private Sub btn_summer_Click(sender As Object, e As EventArgs) Handles btn_summer.Click
        m_SummerCalculationEnabled = Not m_SummerCalculationEnabled
        SeasonalCalculation_UpdateModeButton()
        Calculate()
    End Sub

    Private Sub btnEN13141_Click(sender As Object, e As EventArgs) Handles btnEN13141.Click
        m_WinterReportScenarioName = btnEN13141.Text
        txbPerformance_FreshInletTemperature.Text = 7
        txbPerformance_RHFreshInlet.Text = 70
        txbPerformance_ReturnInletTemperature.Text = 20
        txbPerformance_RHReturnInlet.Text = 37
        Calculate()
    End Sub

End Class

