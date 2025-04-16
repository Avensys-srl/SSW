Imports System.IO
Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.Windows.Forms.DataVisualization.Charting
Imports ClimaLombarda.Common
Imports ClimaLombarda.Common.UI
Imports System.Data
Imports ClimaLombarda.DataCentral.LTModel
Imports Climalombarda.DataCentral


#Const COIL = False

Public Class CLMainForm

    Private m_SAPEnable As Boolean
    Private m_DataChanging As Boolean = False
    Private m_TabPages As New List(Of TabPage)

    Private ReadOnly Property Environment As CLEnvironment
        Get
            Return CLEnvironment.Current
        End Get
    End Property

    Public Sub New()

        ' Chiamata richiesta dalla finestra di progettazione.
        InitializeComponent()

        Me.Icon = Environment.SSWInfo.EmbeddedIcon

        Dim culture As CultureInfo

        culture = CultureInfo.CurrentCulture.Clone

        culture.NumberFormat.NumberDecimalSeparator = ","
        culture.NumberFormat.NumberGroupSeparator = "."

        Threading.Thread.CurrentThread.CurrentCulture = culture
        Threading.Thread.CurrentThread.CurrentUICulture = culture

        AddHandler Environment.LanguageChanged, AddressOf Environment_LanguageChanged
        AddHandler Environment.BranchChanged, AddressOf Environment_BranchChanged

        ' Inizializza il binding source UnitCalculator
        bsrUnitCalculator.DataSource = m_UnitCalculator

        m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.None

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

        ' Set regional settings
        MeasureUnit = CLMeasureUnit.SI

        state = CultureInfo.CurrentCulture.Name.Substring(3)

        language = Environment.FindLanguage(CultureInfo.CurrentCulture.Name.Substring(0, 2))
        If language Is Nothing OrElse Not language.Enabled Then
            language = Environment.FindLanguage(Environment.SSWInfo.DefaultLanguage)
        End If
        If language Is Nothing OrElse Not language.Enabled Then
            language = Environment.ENLanguage
        End If

        Environment.SetLanguage(language)

        If state = "GB" Then
            m_SAPEnable = True
        Else
            m_TabPages.Add(tbpCertification)
            tbcMain.TabPages.Remove(tbpCertification)
        End If

#If Not COIL Then
        m_TabPages.Add(tbpAccessory)
        tbcMain.TabPages.Remove(tbpAccessory)
#End If

        ' Fill Series
        Series_FillCombo()

        ' Fill HWD/CWD FluidType
        Accessory_HWD_FluidTypeFill()
        Accessory_CWD_FluidTypeFill()

        txbPerformance_AirFlow.Text = "100"
        sap_table_start()
        txbPerformance_AirFlow.Select()

        'Fill CO2 data
        CO2LevelFill()

        'CLModule.Environment.ExportModelsToCsv("d:\temp\environment.txt")
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
        lblPerformance_RegulationLevelValue.Text = e.NewValue & " %"
        prbPerformance_RegulationLevel.Value = e.NewValue
        Calculate()
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

    Private Sub txbPerformance_AirFlow_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_AirFlow.Enter
        'txbPerformance_AirFlow.Text = maxflow(MeasureUnit, CurrentUnit.Name, txbPerformance_AirFlow.Text)
        'Calculate()

        'Try
        '    m_DataChanging = True
        '    txbAccessory_InputData_AirFlow.Text = txbPerformance_AirFlow.Text
        'Finally
        '    m_DataChanging = False
        'End Try
        txbPerformance_AirFlow_SaveValue = txbPerformance_AirFlow.Text
    End Sub

    Private Sub txbPerformance_AirFlow_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_AirFlow.Validating

        Dim value As Double

        If Not Double.TryParse(txbPerformance_AirFlow.Text, value) Then
            txbPerformance_AirFlow.Text = txbPerformance_AirFlow_SaveValue
        Else
            txbPerformance_AirFlow.Text = maxflow(MeasureUnit, SelectedHeatRecoveryModel, txbPerformance_AirFlow.Text)
        End If

    End Sub

    Private Sub txbPerformance_AirFlow_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_AirFlow.Validated
        Calculate()

        Try
            m_DataChanging = True
            txbAccessory_InputData_AirFlow.Text = txbPerformance_AirFlow.Text
            checkworkingpoint()
        Finally
            m_DataChanging = False
        End Try



    End Sub

#End Region

#Region "====[ FreshInletTemperature ]===="

    Private txbPerformance_FreshInletTemperature_SaveValue As String = ""

    Private Sub txbPerformance_FreshInletTemperature_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_FreshInletTemperature.Enter
        txbPerformance_FreshInletTemperature_SaveValue = txbPerformance_FreshInletTemperature.Text
    End Sub

    Private Sub txbPerformance_FreshInletTemperature_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_FreshInletTemperature.Validating

        Dim value As Double

        If Not Double.TryParse(txbPerformance_FreshInletTemperature.Text, value) Then
            txbPerformance_FreshInletTemperature.Text = txbPerformance_FreshInletTemperature_SaveValue
        Else
            txbPerformance_FreshInletTemperature.Text = FormatNumber(Math.Round(CDbl(txbPerformance_FreshInletTemperature.Text), 1), 1)
        End If

    End Sub

    Private Sub txbPerformance_FreshInletTemperature_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_FreshInletTemperature.Validated
        Dim fit_lim As Double

        fit_lim = FormatNumber(Math.Round(CDbl(txbPerformance_FreshInletTemperature.Text), 1), 1)

        If fit_lim < -20 Then
            fit_lim = -20
        ElseIf fit_lim > 40 Then
            fit_lim = 40
        End If

        txbPerformance_FreshInletTemperature.Text = fit_lim
        Calculate()
    End Sub

#End Region

#Region "====[ RHFreshInlet ]===="

    Private txbPerformance_RHFreshInlet_SaveValue As String = ""

    Private Sub txbPerformance_RHFreshInlet_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_RHFreshInlet.Enter
        txbPerformance_RHFreshInlet_SaveValue = txbPerformance_RHFreshInlet.Text
    End Sub

    Private Sub txbPerformance_RHFreshInlet_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_RHFreshInlet.Validating

        Dim value As Double

        If Not Double.TryParse(txbPerformance_RHFreshInlet.Text, value) Then
            txbPerformance_RHFreshInlet.Text = txbPerformance_RHFreshInlet_SaveValue
        Else
            txbPerformance_RHFreshInlet.Text = FormatNumber(Math.Round(CDbl(txbPerformance_RHFreshInlet.Text), 0), 0)
        End If

    End Sub

    Private Sub txbPerformance_RHFreshInlet_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_RHFreshInlet.Validated
        Dim rhfit_lim As Double

        rhfit_lim = FormatNumber(Math.Round(CDbl(txbPerformance_RHFreshInlet.Text), 0), 0)

        If rhfit_lim < 10 Then
            rhfit_lim = 10
        ElseIf rhfit_lim > 98 Then
            rhfit_lim = 98
        End If

        txbPerformance_RHFreshInlet.Text = rhfit_lim
        Calculate()
    End Sub

#End Region

#Region "====[ ReturnInletTemperature ]===="

    Private txbPerformance_ReturnInletTemperature_SaveValue As String = ""

    Private Sub txbPerformance_ReturnInletTemperature_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_ReturnInletTemperature.Enter
        txbPerformance_ReturnInletTemperature_SaveValue = txbPerformance_ReturnInletTemperature.Text
    End Sub

    Private Sub txbPerformance_ReturnInletTemperature_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_ReturnInletTemperature.Validating

        Dim value As Double

        If Not Double.TryParse(txbPerformance_ReturnInletTemperature.Text, value) Then
            txbPerformance_ReturnInletTemperature.Text = txbPerformance_ReturnInletTemperature_SaveValue
        Else
            txbPerformance_ReturnInletTemperature.Text = FormatNumber(Math.Round(CDbl(txbPerformance_ReturnInletTemperature.Text), 1), 1)
        End If

    End Sub

    Private Sub txbPerformance_ReturnInletTemperature_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_ReturnInletTemperature.Validated
        Dim rit_lim As Double

        rit_lim = FormatNumber(Math.Round(CDbl(txbPerformance_ReturnInletTemperature.Text), 1), 1)

        If rit_lim < 8 Then
            rit_lim = 8
        ElseIf rit_lim > 40 Then
            rit_lim = 40
        End If

        txbPerformance_ReturnInletTemperature.Text = rit_lim
        Calculate()
    End Sub

#End Region

#Region "====[ RHReturnInlet ]===="

    Private txbPerformance_RHReturnInlet_SaveValue As String = ""

    Private Sub txbPerformance_RHReturnInlet_Enter(sender As System.Object, e As System.EventArgs) Handles txbPerformance_RHReturnInlet.Enter
        txbPerformance_RHReturnInlet_SaveValue = txbPerformance_RHReturnInlet.Text
    End Sub

    Private Sub txbPerformance_RHReturnInlet_Validating(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles txbPerformance_RHReturnInlet.Validating

        Dim value As Double

        If Not Double.TryParse(txbPerformance_RHReturnInlet.Text, value) Then
            txbPerformance_RHReturnInlet.Text = txbPerformance_RHReturnInlet_SaveValue
        Else
            txbPerformance_RHReturnInlet.Text = FormatNumber(Math.Round(CDbl(txbPerformance_RHReturnInlet.Text), 0), 0)
        End If

    End Sub

    Private Sub txbPerformance_RHReturnInlet_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbPerformance_RHReturnInlet.Validated

        Dim rhrit_lim As Double

        rhrit_lim = FormatNumber(Math.Round(CDbl(txbPerformance_RHReturnInlet.Text), 0), 0)

        If rhrit_lim < 10 Then
            rhrit_lim = 10
        ElseIf rhrit_lim > 98 Then
            rhrit_lim = 98
        End If

        txbPerformance_RHReturnInlet.Text = rhrit_lim
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

        chart.Scale(New SizeF(scale, scale))

        chart.BorderlineWidth *= scale
        For Each series As DataVisualization.Charting.Series In chart.Series
            series.BorderWidth *= scale
            series.Font = New System.Drawing.Font(series.Font.FontFamily.Name, series.Font.Size + (6 * scale))
            series.MarkerSize *= scale
        Next

        For Each legend As DataVisualization.Charting.Legend In chart.Legends
            legend.BorderWidth *= scale
            legend.Font = New System.Drawing.Font(legend.Font.FontFamily.Name, legend.Font.Size + (6 * scale))
            legend.TitleFont = New System.Drawing.Font(legend.TitleFont.FontFamily.Name, legend.TitleFont.Size + (6 * scale))
        Next

        For Each chartArea As DataVisualization.Charting.ChartArea In chart.ChartAreas
            chartArea.AxisX.LineWidth *= scale
            chartArea.AxisX.TitleFont = New System.Drawing.Font(chartArea.AxisX.TitleFont.FontFamily.Name, chartArea.AxisX.TitleFont.Size + (6 * scale))
            chartArea.AxisX.LabelStyle.Font = New System.Drawing.Font(chartArea.AxisX.LabelStyle.Font.FontFamily.Name, chartArea.AxisX.LabelStyle.Font.Size + (6 * scale))
            chartArea.AxisY.LineWidth *= scale
            chartArea.AxisY.TitleFont = New System.Drawing.Font(chartArea.AxisY.TitleFont.FontFamily.Name, chartArea.AxisY.TitleFont.Size + (6 * scale))
            chartArea.AxisY.LabelStyle.Font = New System.Drawing.Font(chartArea.AxisY.LabelStyle.Font.FontFamily.Name, chartArea.AxisY.LabelStyle.Font.Size + (6 * scale))
        Next

        chart.SaveImage(memoryStream, ImageFormat.Bmp)
        image = New Bitmap(memoryStream)

        Return image
    End Function

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

    Private Sub SaveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmiFile_GenerateReport.Click

        Report_Generate()
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
        Dim chartScale As Double = 3
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
        cloneChart.Size = chartOriginalSize
        pressureImage = Chart_GetImage(cloneChart, chartScale)

        ' Build image Power
        cloneChart = Chart_Clone(crtPerformance_Chart2)
        cloneChart.Size = chartOriginalSize
        powerImage = Chart_GetImage(cloneChart, chartScale)

        ' Build image AirFlow
        cloneChart = Chart_Clone(crtPerformance_Chart3)
        cloneChart.Size = chartOriginalSize
        airflowImage = Chart_GetImage(cloneChart, chartScale)

        ' Build image CO2
        cloneChart = Chart_Clone(crtCO2Level_Chart1)
        cloneChart.Size = chartOriginalSize
        CO2Image = Chart_GetImage(cloneChart, chartScale)

        ' Legends
        cloneChart = Chart_Clone(crtPerformance_Chart1)
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



        legendImage = Chart_GetImage(cloneChart, chartScale)

        Dim reportDataSet As New CLMainReportDataSet
        Dim headerDataRow As CLMainReportDataSet.HeaderDataTableRow
        Dim performanceAccordanceDataRow As CLMainReportDataSet.PerformanceAccordanceDataTableRow
        Dim workingPointDataRow As CLMainReportDataSet.WorkingPointDataTableRow
        Dim temperatureConditionsAndHumidityDataRow As CLMainReportDataSet.TemperatureConditionsAndHumidityDataTableRow
        Dim heatExchangerPerformancesDataRow As CLMainReportDataSet.HeatExchangerPerformancesDataTableRow
        Dim diagramDataRow As CLMainReportDataSet.DiagramDataTableRow
        Dim sapDataRow As CLMainReportDataSet.SAPDataTableRow
        Dim sapItemDataRow As CLMainReportDataSet.SAPItemsDataTableRow
        Dim CO2LevelDataRow As CLMainReportDataSet.CO2LevelRoomRow
        Dim CO2LevelUseDataRow As CLMainReportDataSet.CO2LevelUseRow
        Dim CO2LevelParametersDataRow As CLMainReportDataSet.CO2LevelParametersRow

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

        workingPointDataRow.Title = Environment.Localization.GetString(CLMessageResources.PDF_WorkingPoint.ToString())

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

        temperatureConditionsAndHumidityDataRow.Title = Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString())

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

        ' Temperature Conditions And Humidity
        '---------------------------------------------------
        heatExchangerPerformancesDataRow = reportDataSet.HeatExchangerPerformancesDataTable.NewHeatExchangerPerformancesDataTableRow()

        heatExchangerPerformancesDataRow.Title = Environment.Localization.GetString(CLMessageResources.MainForm_HeatExchangerPerformances.ToString())

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
            soundPowerDataRow.Caption = soundPerformance(SoundColumnName_Caption)
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
        diagramDataRow = reportDataSet.DiagramDataTable.NewDiagramDataTableRow()
        diagramDataRow.AirFlowImage = Bitmap_GetBytes(airflowImage)
        diagramDataRow.LegendImage = Bitmap_GetBytes(legendImage)
        diagramDataRow.PowerImage = Bitmap_GetBytes(powerImage)
        diagramDataRow.PressureImage = Bitmap_GetBytes(pressureImage)
        diagramDataRow.CO2Image = Bitmap_GetBytes(CO2Image)
        reportDataSet.DiagramDataTable.Rows.Add(diagramDataRow)

        ' SAP
        '---------------------------------------------------
        sapDataRow = reportDataSet.SAPDataTable.NewSAPDataTableRow()

        sapDataRow.Title = Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_EstimatedSAPTestTable.ToString())
        sapDataRow.ExhaustTerminalConfiguration_Caption = dgvSAP.Columns(0).HeaderText
        sapDataRow.TotalExhaustFlowRate_Caption = dgvSAP.Columns(1).HeaderText
        sapDataRow.TotalSupplyFlowRate_Caption = dgvSAP.Columns(2).HeaderText
        sapDataRow.RegulationLevel_Caption = dgvSAP.Columns(3).HeaderText
        sapDataRow.SpecificFanPower_Caption = dgvSAP.Columns(4).HeaderText
        sapDataRow.HeatExchangeEfficiency_Caption = dgvSAP.Columns(5).HeaderText
        sapDataRow.EnergySavingTrustBestPracticePerformanceCompliant_Caption = dgvSAP.Columns(6).HeaderText

        sapDataRow.Visible = m_SAPEnable

        reportDataSet.SAPDataTable.Rows.Add(sapDataRow)

        For rowsCounter As Integer = 0 To dgvSAP.Rows.Count - 1

            sapItemDataRow = reportDataSet.SAPItemsDataTable.NewSAPItemsDataTableRow()

            sapItemDataRow.ExhaustTerminalConfiguration_Value = dgvSAP(0, rowsCounter).Value
            sapItemDataRow.TotalExhaustFlowRate_Value = dgvSAP(1, rowsCounter).Value
            sapItemDataRow.TotalSupplyFlowRate_Value = dgvSAP(2, rowsCounter).Value
            sapItemDataRow.RegulationLevel_Value = dgvSAP(3, rowsCounter).Value
            sapItemDataRow.SpecificFanPower_Value = dgvSAP(4, rowsCounter).Value
            sapItemDataRow.HeatExchangeEfficiency_Value = dgvSAP(5, rowsCounter).Value
            sapItemDataRow.EnergySavingTrustBestPracticePerformanceCompliant_Value = dgvSAP(6, rowsCounter).Value

            reportDataSet.SAPItemsDataTable.Rows.Add(sapItemDataRow)

        Next

        reportDataSet.AcceptChanges()

        ' Show Report
        ' --------------------------------------------
        Dim reportViewForm As New CLReportViewerForm
        Dim reportDataSources As New List(Of Microsoft.Reporting.WinForms.ReportDataSource)

        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("Header", DirectCast(reportDataSet.HeaderDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("PerformanceAccordance", DirectCast(reportDataSet.PerformanceAccordanceDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("TemperatureConditionsAndHumidity", DirectCast(reportDataSet.TemperatureConditionsAndHumidityDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SoundPower", DirectCast(reportDataSet.SoundPowerDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SoundPowerHeader", DirectCast(reportDataSet.SoundPowerHeaderDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("WorkingPoint", DirectCast(reportDataSet.WorkingPointDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("HeatExchangerPerformances", DirectCast(reportDataSet.HeatExchangerPerformancesDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("Diagram", DirectCast(reportDataSet.DiagramDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SAP", DirectCast(reportDataSet.SAPDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("SAPItems", DirectCast(reportDataSet.SAPItemsDataTable, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("CO2LevelRoom", DirectCast(reportDataSet.CO2LevelRoom, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("CO2LevelUse", DirectCast(reportDataSet.CO2LevelUse, System.Data.DataTable)))
        reportDataSources.Add(New Microsoft.Reporting.WinForms.ReportDataSource("CO2LevelParameters", DirectCast(reportDataSet.CO2LevelParameters, System.Data.DataTable)))

        waitForm.Hide()

        reportViewForm.SetReport(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CLMainReport.rdlc"), _
        reportDataSources.ToArray(), Microsoft.Reporting.WinForms.DisplayMode.PrintLayout)

        reportViewForm.WindowState = FormWindowState.Maximized
        reportViewForm.rpvReport.LocalReport.DisplayName = SelectedHeatRecoveryModelCustomerName & "_" &
            workingPointDataRow.AirFlow_Value & "_" &
            workingPointDataRow.MaxPressure_Value & "_" &
            "Report_" & Environment.PrimaryLanguageCode

        reportViewForm.ShowDialog()
    End Sub

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

        UpdateLocalization()
        Calculate()
        tsmiFile_SaveCommercialSheet.Enabled = CommercialSheet_CanGenerate(Environment.PrimaryLanguageCode, Environment.Branch.ShortName)

    End Sub

    Public Sub UpdateLocalization()

        'grbPerformance_InputData.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_InputData.ToString())

        grbPerformance_UnitSelection.Text = Environment.Localization.GetString(CLMessageResources.MainForm_UnitSelection.ToString())

        lblPerformance_Unit.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Unit.ToString())

        lblPerformance_MaxPressure.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_MaxPressure.ToString()), _
         "Pa")

        grbPerformance_TemperatureConditions.Text = Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString())

        lblPerformance_FreshInletTemperature.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_FreshInletTemperature.ToString()), _
         "°C")

        lblPerformance_RHFreshInlet.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_RHFreshInletTemperature.ToString()), _
         "%")

        lblPerformance_ReturnInletTemperature.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_ReturnInletTemperature.ToString()), _
         "°C")

        lblPerformance_RHReturnInlet.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_RHReturnInletTemperature.ToString()), _
         "%")

        tbpData_Thermal.Text = String.Format("{0} / {1}",
            Environment.Localization.GetString(CLMessageResources.MainForm_HeatExchangerPerformances.ToString()),
            Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString()))

        grbPerformance_HeatExchangerPerformances.Text = Environment.Localization.GetString(CLMessageResources.MainForm_HeatExchangerPerformances.ToString())

        lblPerformance_HeatTransferred.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_HeatTransferred.ToString()), _
         "W")

        lblPerformance_SensibleHeat.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_SensibleHeat.ToString()), _
         "W")

        lblPerformance_LatentHeat.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_LatentHeat.ToString()), _
         "W")

        txbPerformance_Efficiency.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_Efficiency.ToString()), _
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



        grbPerformance_TemperatureConditions2.Text = Environment.Localization.GetString(CLMessageResources.MainForm_TemperatureConditionsAndUmidity.ToString())

        lblPerformance_SupplyOutletTemperature.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_SupplyOutletTemperature.ToString()), _
         "°C")
        lblPerformance_SupplyOutletRH.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_RHSupplyOutletTemperature.ToString()), _
         "%")

        lblPerformance_ExhaustOutletTemperature.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_ExhaustOutletTemperature.ToString()), _
         "°C")
        lblPerformance_ExhaustOutletRH.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_RHExhaustOutletTemperature.ToString()), _
         "%")

        lblPerformance_RegulationLevel.Text = Environment.Localization.GetString(CLMessageResources.MainForm_RegulationLevel.ToString())

        tbpData_ElectricalPerformances.Text = Environment.Localization.GetString(CLMessageResources.MainForm_ElectricalPerformances.ToString())

        lblPerformance_ElectricalPerformances_PowerInput.Text = String.Format("{0} [{1}] ({2})", _
           Environment.Localization.GetString(CLMessageResources.MainForm_PowerInput.ToString()), _
           "W", _
           Environment.Localization.GetString(CLMessageResources.MainForm_SingleBranch.ToString()))

        grbPerformance_PassiveHaus.Text = Environment.Localization.GetString(CLMessageResources.MainForm_PassiveHouse.ToString())
        lblPerformance_PassiveHaus_ElectricalEfficiency.Text = String.Format("{0} [{1}]", _
         Environment.Localization.GetString(CLMessageResources.MainForm_PassiveHouseElectricalEfficienty.ToString()), _
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

        tsmiOption.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option.ToString())
        'tsmiOption_SelectMode.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_Menu_Option_SelectMode.ToString())
        'tsmiOption_SelectMode_Checking.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_Menu_Option_SelectMode_Checking.ToString())
        'tsmiOption_SelectMode_Design.Text = Environment.Localization.GetString(CLMessageResources.CLMainForm_Menu_Option_SelectMode_Design.ToString())

        tsmiOption_Language.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language.ToString())
        tsmiOption_Language_DE.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Deutsch.ToString())
        tsmiOption_Language_NL.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Dutch.ToString())
        tsmiOption_Language_EN.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_English.ToString())
        tsmiOption_Language_FR.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Français.ToString())
        tsmiOption_Language_IT.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Italian.ToString())
        tsmiOption_Language_PL.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Polish.ToString())
        tsmiOption_Language_SL.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Slovenian.ToString())
        tsmiOption_Language_BG.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Bulgarian.ToString())
        tsmiOption_Language_RO.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Language_Romanian.ToString())

        tsmiOption_Unit.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Unit.ToString())
        tsmiOption_Unit_IP.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Unit_IP.ToString())
        tsmiOption_Unit_SI.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_Option_Unit_SI.ToString())

        tsmiAbout.Text = Environment.Localization.GetString(CLMessageResources.MainForm_Menu_About.ToString())

        dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant.HeaderText = _
         Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_EnergySavingTrustBestPracticePerformanceCompliant.ToString())

        dgvSAP_ExhaustTerminalConfiguration.HeaderText = _
         Environment.Localization.GetString(CLMessageResources.MainForm_GridSAP_ExhaustTerminalConfiguration.ToString())

        dgvSAP_HeatExchangeEfficiency.HeaderText = _
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

        UpdateLocalization_MeasureUnit()
    End Sub

    Private Sub UpdateLocalization_MeasureUnit()
        Select Case m_MeasureUnit
            Case CLModule.CLMeasureUnit.SI
                lblPerformance_AirFlow.Text = String.Format("{0} [{1}]",
                  Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()),
                  "m3/h")

                lblPerformance_SFP_SFP.Text = String.Format("{0} [{1}]", _
                 Environment.Localization.GetString(CLMessageResources.MainForm_SFP.ToString()), _
                 "kW/(m3/s)")

                lblPerformance_SEL_SEL.Text = String.Format("{0} [{1}]", _
                 Environment.Localization.GetString(CLMessageResources.MainForm_SEL.ToString()), _
                 "J/m3")


            Case CLModule.CLMeasureUnit.IP
                lblPerformance_AirFlow.Text = String.Format("{0} [{1}]", _
                  Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()), _
                  "l/s")

                lblPerformance_SFP_SFP.Text = String.Format("{0} [{1}]", _
                 Environment.Localization.GetString(CLMessageResources.MainForm_SFP.ToString()), _
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
            Return IOM_GetFile("EN", False, Environment.Branch.ShortName)
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

        sfdSavePdf.FileName = String.Format("{0}-{1}", _
            SelectedHeatRecoveryModelCustomerName, _
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
        sfdSavePdf.FileName = String.Format("{0}-{1}", _
            SelectedHeatRecoveryModelCustomerName, _
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

    Private Function CommercialSheet_GetFiles(languageCode As String, useEnLanguageWhenNotExist As Boolean, shortname As String) As String

        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel

        If dcHeatRecoveryModel Is Nothing OrElse String.IsNullOrEmpty(dcHeatRecoveryModel.PDFCommercialSheets) _
            OrElse Not Directory.Exists(PDFDocumentDirectory) Then
            Return ""
        End If

        Dim cssPdfFiles() As String
        Dim pdfFiles As New List(Of String)

        cssPdfFiles = Directory.GetFiles(PDFDocumentDirectory, dcHeatRecoveryModel.PDFCommercialSheets.Replace("%LanguageCode%", languageCode).Replace("%ShortName%", shortname))

        For Each pdfFile As String In cssPdfFiles
            pdfFiles.Add(pdfFile)
        Next

        If pdfFiles.Count = 0 AndAlso useEnLanguageWhenNotExist AndAlso languageCode <> "EN" Then
            Return CommercialSheet_GetFiles("EN", False, Environment.Branch.ShortName)
        End If

        If pdfFiles.Count = 0 Then
            Return ""
        End If
        Return pdfFiles(0)


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

    Private ReadOnly Property PDFDocumentDirectory() As String
        Get
            Return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "css")
        End Get
    End Property

#End Region

#Region "====[ Properties ]===="

    Public ReadOnly Property AirFlow As Double
        Get
            Dim value As Double
            Return IIf(Double.TryParse(txbPerformance_AirFlow.Text, value), value, 0.0)
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

#Region "====[ Calculate ]===="

    Private Sub Calculate()
        If cmbPerformance_HeatRecoveryModels.SelectedIndex = -1 Then
            Return
        End If

        Calculate_Data()
        Calculate_Sound()
        Calculate_CO2Level()

#If COIL Then
        Calculate_Accessory()
#End If


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
                soundValues(0) = 54.7
                soundValues(1) = 50.7
                soundValues(2) = 35.8
                soundValues(3) = 36.8
                soundValues(4) = 34.8
                soundValues(5) = 30.8
                soundValues(6) = 27.9
                soundValues(7) = 19.9
            ElseIf dcHeatRecoveryModel.Code.Contains("QUARK 035") Then
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
        newValue = Math.Round(sound_power_correction(hsbPerformance_RegulationLevel.Value, _
                dcHeatRecoveryModel.AirflowsItems.Max(), dcHeatRecoveryModel.PressuresItems.Max(), _
                IIf(Double.Parse(txbPerformance_AirFlow.Text) = 0, 1, Double.Parse(txbPerformance_AirFlow.Text)), _
                IIf(Double.Parse(txbPerformance_MaxPressure.Text) = 0, 1, Double.Parse(txbPerformance_MaxPressure.Text)),
                value), nDecimal)

        If forceAbs Then
            newValue = Math.Abs(newValue)
        End If


        Return newValue
    End Function

    Private Sub Calculate_Data()
        Try

            Dim rec As String
            Dim pl, afm, rhfitm, fitm, rhritm, ritm As Double
            Dim termo_work As termo
            Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel

            rec = dcHeatRecoveryModel.ModRec
            pl = CDbl(dcHeatRecoveryModel.LenRec)
            ritm = CDbl(txbPerformance_ReturnInletTemperature.Text)
            rhritm = CDbl(txbPerformance_RHReturnInlet.Text) / 100
            fitm = CDbl(txbPerformance_FreshInletTemperature.Text)
            rhfitm = CDbl(txbPerformance_RHFreshInlet.Text) / 100
            afm = CDbl(txbPerformance_AirFlow.Text)
            If afm = 0 Then
                afm = 1
            Else
                If m_MeasureUnit = CLMeasureUnit.IP Then
                    afm = CDbl(txbPerformance_AirFlow.Text) * 3.6
                Else
                    afm = CDbl(txbPerformance_AirFlow.Text)
                End If
            End If
            Dim workpoint(3) As Double

            termo_work = termo_calc(ritm, rhritm, fitm, rhfitm, afm, rec, pl, 0)

            txbPerformance_SupplyOutletTemperature.Text = FormatNumber(Math.Round(termo_work.Supply_outlet_temp, 1), 1)
            txbPerformance_SupplyOutletRH.Text = FormatNumber(Math.Round(100 * termo_work.Supply_outlet_rh, 0), 0)
            txbPerformance_ExhaustOutletTemperature.Text = FormatNumber(Math.Round(termo_work.Exhaust_outlet_temp, 1), 1)
            txbPerformance_ExhaustOutletRH.Text = FormatNumber(Math.Round(100 * termo_work.Exhaust_outlet_rh, 0), 0)
            txbPerformance_HeatTransferred.Text = FormatNumber(termo_work.heat_recovery, 0)
            txbPerformance_SensibleHeat.Text = FormatNumber(termo_work.sensible_heat, 0)
            txbPerformance_LatentHeat.Text = FormatNumber(termo_work.latent_heat, 0)
            txbPerformance_WaterProduced.Text = FormatNumber(termo_work.water_produced)
            lblPerformance_Efficiency.Text = FormatNumber(100 * termo_work.efficiency, 0)

            workpoint = curva(MeasureUnit,
               afm,
               dcHeatRecoveryModel,
               ritm,
               rhritm,
               fitm,
               rhfitm,
               prbPerformance_RegulationLevel.Value,
               crtPerformance_Chart1,
               crtPerformance_Chart2,
               crtPerformance_Chart3,
               CDbl(IIf(txbPerformance_MaxPressure.Text = "", "0", txbPerformance_MaxPressure.Text)),
               chbPerformance_SFP_ShowArea.Checked OrElse chbPerformance_SEL_ShowArea.Checked,
               chbPerformance_ERP2018_ShowArea.Checked,
               nudPerformance_SFP_Limit.Value,
               chbPerformance_PassiveHaus_ShowArea.Checked,
               CDbl(txbPerformance_PassiveHaus_Limit.Text))

            termo_work = termo_calc(ritm, rhritm, fitm, rhfitm, workpoint(1), rec, pl, 0)

            txbPerformance_MaxPressure.Text = FormatNumber(Math.Floor(workpoint(2)), 0)



            If workpoint(3) <= 5 Then
                workpoint(3) = 5
            End If

            txbPerformance_ElectricalPerformances_PowerInput.Text = FormatNumber(Math.Round(workpoint(3)), 0)

            Select Case m_MeasureUnit
                Case CLMeasureUnit.IP
                    txbPerformance_AirFlow.Text = FormatNumber(workpoint(1), 1)
                    txbPerformance_SFP.Text = FormatNumber((workpoint(3) * 2) / workpoint(1))
                    txbPerformance_SEL.Text = FormatNumber(((workpoint(3) * 2) / workpoint(1)) * 1000, 0)
                    txbPerformance_PassiveHaus.Text = FormatNumber(workpoint(3) / (workpoint(1) * 3.6))
                Case CLMeasureUnit.SI
                    txbPerformance_AirFlow.Text = FormatNumber(workpoint(1), 0)
                    txbPerformance_SFP.Text = FormatNumber((workpoint(3) * 2) * 3.6 / workpoint(1))
                    txbPerformance_SEL.Text = FormatNumber(((workpoint(3) * 2) * 3.6 / workpoint(1)) * 1000, 0)
                    txbPerformance_PassiveHaus.Text = FormatNumber((workpoint(3) * 2) / workpoint(1))
            End Select
        Catch exception As Exception
            MessageBox.Show(Me, exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub Calculate_Accessory()
        Dim dcHeatRecoveryModel As CLDCHeatRecoveryModel = SelectedHeatRecoveryModel

        ' Calcolo degli accessory
        m_UnitCalculator.Calculate(dcHeatRecoveryModel,
         AirFlow,
         SupplyOutletTemp,
         SupplyOutletRH)
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

#Region "====[ Accessory CWD / HWD / EHD ]===="

    Private m_AccessoryCWD_FluidTypeTec_SaveValue As Double
    Private m_AccessoryHWD_FluidTypeTec_SaveValue As Double

    Private Sub Accessory_CWD_FluidTypeFill()

        cmbAccessory_CWD_FluidType.Items.Clear()

        cmbAccessory_CWD_FluidType.Items.Add(New CLComboBoxItemWrapper(Of CLCOFluidType)(
            Environment.Localization.GetString(CLMessageResources.Water.ToString()),
            CLCOFluidType.Water))

        cmbAccessory_CWD_FluidType.Items.Add(New CLComboBoxItemWrapper(Of CLCOFluidType)(
            Environment.Localization.GetString(CLMessageResources.Glic_Etil.ToString()),
            CLCOFluidType.Glic_Etil))

        cmbAccessory_CWD_FluidType.Items.Add(New CLComboBoxItemWrapper(Of CLCOFluidType)(
            Environment.Localization.GetString(CLMessageResources.Glic_Prop.ToString()),
            CLCOFluidType.Glic_Prop))

        cmbAccessory_CWD_FluidType.SelectedIndex = 0

    End Sub

    Private Sub Accessory_HWD_FluidTypeFill()

        cmbAccessory_HWD_FluidType.Items.Clear()

        cmbAccessory_HWD_FluidType.Items.Add(New CLComboBoxItemWrapper(Of CLCOFluidType)(
            Environment.Localization.GetString(CLMessageResources.Water.ToString()),
            CLCOFluidType.Water))

        cmbAccessory_HWD_FluidType.Items.Add(New CLComboBoxItemWrapper(Of CLCOFluidType)(
            Environment.Localization.GetString(CLMessageResources.Glic_Etil.ToString()),
            CLCOFluidType.Glic_Etil))

        cmbAccessory_HWD_FluidType.Items.Add(New CLComboBoxItemWrapper(Of CLCOFluidType)(
            Environment.Localization.GetString(CLMessageResources.Glic_Prop.ToString()),
            CLCOFluidType.Glic_Prop))

        cmbAccessory_HWD_FluidType.SelectedIndex = 0

    End Sub

    Private Sub cmbAccessory_CWD_FluidType_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbAccessory_CWD_FluidType.SelectedIndexChanged

        Dim fluidType As CLCOFluidType

        If cmbAccessory_CWD_FluidType.SelectedItem Is Nothing Then
            Return
        End If

        fluidType = DirectCast(cmbAccessory_CWD_FluidType.SelectedItem, CLComboBoxItemWrapper(Of CLCOFluidType)).Value
        If fluidType = CLCOFluidType.Water Then
            nudAccessory_CWD_FluidTypeTec.Visible = False
            lblAccessory_CWD_FluidTypeTec.Visible = False
        Else
            nudAccessory_CWD_FluidTypeTec.Visible = True
            lblAccessory_CWD_FluidTypeTec.Visible = True
        End If

        m_UnitCalculator.CWD_FluidType = fluidType

        Calculate_Accessory()
    End Sub

    Private Sub cmbAccessory_HWD_FluidType_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbAccessory_HWD_FluidType.SelectedIndexChanged

        Dim fluidType As CLCOFluidType

        If cmbAccessory_HWD_FluidType.SelectedItem Is Nothing Then
            Return
        End If

        fluidType = DirectCast(cmbAccessory_HWD_FluidType.SelectedItem, CLComboBoxItemWrapper(Of CLCOFluidType)).Value
        If fluidType = CLCOFluidType.Water Then
            nudAccessory_HWD_FluidTypeTec.Visible = False
            lblAccessory_HWD_FluidTypeTec.Visible = False
        Else
            nudAccessory_HWD_FluidTypeTec.Visible = True
            lblAccessory_HWD_FluidTypeTec.Visible = True
        End If

        m_UnitCalculator.HWD_FluidType = fluidType
        Calculate_Accessory()
    End Sub

    Private Sub UIAccessoryPanel_SetEnabled(control As Control, enabled As Boolean, cbxControl As CheckBox)

        For Each childControl As Control In control.Controls
            If cbxControl Is childControl Then
                Continue For
            End If

            childControl.Enabled = enabled
        Next

    End Sub

    Private Sub cbxAccessory_CWDEnabled_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cbxAccessory_CWDEnabled.CheckedChanged

        If cbxAccessory_CWDEnabled.Checked AndAlso cbxAccessory_HWDEnabled.Checked Then
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.CWD_HWD
        ElseIf Not cbxAccessory_CWDEnabled.Checked AndAlso cbxAccessory_HWDEnabled.Checked Then
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.HWD
        ElseIf cbxAccessory_CWDEnabled.Checked AndAlso Not cbxAccessory_HWDEnabled.Checked Then
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.CWD
        Else
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.EHD
        End If

        Calculate_Accessory()

    End Sub

    Private Sub cbxAccessory_HWDEnabled_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cbxAccessory_HWDEnabled.CheckedChanged

        If cbxAccessory_CWDEnabled.Checked AndAlso cbxAccessory_HWDEnabled.Checked Then
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.CWD_HWD
        ElseIf Not cbxAccessory_CWDEnabled.Checked AndAlso cbxAccessory_HWDEnabled.Checked Then
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.HWD
        ElseIf cbxAccessory_CWDEnabled.Checked AndAlso Not cbxAccessory_HWDEnabled.Checked Then
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.CWD
        Else
            m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.EHD
        End If

        Calculate_Accessory()

    End Sub

    Private Sub cbxAccessory_EHDEnabled_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cbxAccessory_EHDEnabled.CheckedChanged

        m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.EHD

        Calculate_Accessory()

    End Sub

    Private Sub txbAccessory_InputData_AirFlow_Validated(sender As System.Object, e As System.EventArgs) Handles txbAccessory_InputData_AirFlow.Validated
        If m_DataChanging Then
            Return
        End If

        txbPerformance_AirFlow.Text = txbAccessory_InputData_AirFlow.Text
        Calculate()

        Try
            m_DataChanging = True
            txbAccessory_InputData_AirFlow.Text = txbPerformance_AirFlow.Text
        Finally
            m_DataChanging = False
        End Try
    End Sub

    Private Sub txbPerformance_SupplyOutletTemperature_TextChanged(sender As System.Object, e As System.EventArgs) Handles txbPerformance_SupplyOutletTemperature.TextChanged
        txbAccessory_InputData_Temp.Text = SupplyOutletTemp
    End Sub

    Private Sub txbPerformance_SupplyOutletRH_TextChanged(sender As System.Object, e As System.EventArgs) Handles txbPerformance_SupplyOutletRH.TextChanged
        txbAccessory_InputData_RH.Text = SupplyOutletRH
    End Sub

    Private Sub txbAccessory_CWD_InletTemperature_Validated(sender As System.Object, e As System.EventArgs) Handles txbAccessory_CWD_InletTemperature.Validated
        Calculate_Accessory()
    End Sub

    Private Sub txbAccessory_CWD_OutletTemperature_Validated(sender As System.Object, e As System.EventArgs) Handles txbAccessory_CWD_OutletTemperature.Validated
        Calculate_Accessory()
    End Sub

    Private Sub txbAccessory_HWD_InletTemperature_Validated(sender As System.Object, e As System.EventArgs) Handles txbAccessory_HWD_InletTemperature.Validated
        Calculate_Accessory()
    End Sub

    Private Sub txbAccessory_HWD_OutletTemperature_Validated(sender As System.Object, e As System.EventArgs) Handles txbAccessory_HWD_OutletTemperature.Validated
        Calculate_Accessory()
    End Sub

    Private Sub nudAccessory_CWD_FluidTypeTec_MouseUp(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles nudAccessory_CWD_FluidTypeTec.MouseUp
        If m_AccessoryCWD_FluidTypeTec_SaveValue <> nudAccessory_CWD_FluidTypeTec.Value Then
            m_UnitCalculator.CWD_FluidTypeTec = nudAccessory_CWD_FluidTypeTec.Value
            m_AccessoryCWD_FluidTypeTec_SaveValue = nudAccessory_CWD_FluidTypeTec.Value
            Calculate_Accessory()
        End If
    End Sub

    Private Sub nudAccessory_CWD_FluidTypeTec_KeyUp(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles nudAccessory_CWD_FluidTypeTec.KeyUp
        If (e.KeyValue = Keys.Up OrElse e.KeyValue = Keys.Down) AndAlso m_AccessoryCWD_FluidTypeTec_SaveValue <> nudAccessory_CWD_FluidTypeTec.Value Then
            m_UnitCalculator.CWD_FluidTypeTec = nudAccessory_CWD_FluidTypeTec.Value
            m_AccessoryCWD_FluidTypeTec_SaveValue = nudAccessory_CWD_FluidTypeTec.Value
            Calculate_Accessory()
        End If
    End Sub

    Private Sub nudAccessory_CWD_FluidTypeTec_Leave(sender As System.Object, e As System.EventArgs) Handles nudAccessory_CWD_FluidTypeTec.Leave
        If m_AccessoryCWD_FluidTypeTec_SaveValue <> nudAccessory_CWD_FluidTypeTec.Value Then
            m_UnitCalculator.CWD_FluidTypeTec = nudAccessory_CWD_FluidTypeTec.Value
            m_AccessoryCWD_FluidTypeTec_SaveValue = nudAccessory_CWD_FluidTypeTec.Value
            Calculate_Accessory()
        End If
    End Sub

    Private Sub nudAccessory_HWD_FluidTypeTec_MouseUp(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles nudAccessory_HWD_FluidTypeTec.MouseUp
        If m_AccessoryHWD_FluidTypeTec_SaveValue <> nudAccessory_HWD_FluidTypeTec.Value Then
            m_AccessoryHWD_FluidTypeTec_SaveValue = nudAccessory_HWD_FluidTypeTec.Value
            m_UnitCalculator.HWD_FluidTypeTec = nudAccessory_HWD_FluidTypeTec.Value
            Calculate_Accessory()
        End If
    End Sub

    Private Sub nudAccessory_HWD_FluidTypeTec_KeyUp(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles nudAccessory_HWD_FluidTypeTec.KeyUp
        If m_AccessoryHWD_FluidTypeTec_SaveValue <> nudAccessory_HWD_FluidTypeTec.Value Then
            m_UnitCalculator.HWD_FluidTypeTec = nudAccessory_HWD_FluidTypeTec.Value
            m_AccessoryHWD_FluidTypeTec_SaveValue = nudAccessory_HWD_FluidTypeTec.Value
            Calculate_Accessory()
        End If
    End Sub

    Private Sub nudAccessory_HWD_FluidTypeTec_Leave(sender As System.Object, e As System.EventArgs) Handles nudAccessory_HWD_FluidTypeTec.Leave
        If m_AccessoryHWD_FluidTypeTec_SaveValue <> nudAccessory_HWD_FluidTypeTec.Value Then
            m_UnitCalculator.HWD_FluidTypeTec = nudAccessory_HWD_FluidTypeTec.Value
            m_AccessoryHWD_FluidTypeTec_SaveValue = nudAccessory_HWD_FluidTypeTec.Value
            Calculate_Accessory()
        End If
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

        cmbCO2Level_Parameters_CalcMet.SelectedIndex = 0

        cmbCO2Level_Parameters_stdpreset.SelectedIndex = 0

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

#Region "====[ Unit Calculator ]===="

    Private WithEvents m_UnitCalculator As New CLUnitCalculator

    Private Sub m_UnitCalculator_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Handles m_UnitCalculator.PropertyChanged

        If e.PropertyName = CLUnitCalculator.CLPropertyName.AccessoryState.ToString() Then
            If m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.CWD OrElse m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.CWD_HWD Then
                UIAccessoryPanel_SetEnabled(grbAccessory_CWD, True, cbxAccessory_CWDEnabled)
            Else
                UIAccessoryPanel_SetEnabled(grbAccessory_CWD, False, cbxAccessory_CWDEnabled)
            End If

            If m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.HWD OrElse m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.CWD_HWD Then
                UIAccessoryPanel_SetEnabled(grbAccessory_HWD, True, cbxAccessory_HWDEnabled)
            Else
                UIAccessoryPanel_SetEnabled(grbAccessory_HWD, False, cbxAccessory_HWDEnabled)
            End If

            If m_UnitCalculator.AccessoryState = CLUnitCalculator.CLAccessoryState.EHD Then
                UIAccessoryPanel_SetEnabled(grbAccessory_EHD, True, cbxAccessory_HWDEnabled)
            Else
                UIAccessoryPanel_SetEnabled(grbAccessory_EHD, False, cbxAccessory_HWDEnabled)
            End If
        End If

    End Sub

    Private Sub m_UnitCalculator_Calculated(sender As Object, e As EventArgs) Handles m_UnitCalculator.Calculated

        Select Case m_UnitCalculator.CalculateState
            Case CLUnitCalculator.CLCalculateState.AccessoryError
                lblAccessory_Status.Text = m_UnitCalculator.ErrorMessage

            Case CLUnitCalculator.CLCalculateState.Ok
                lblAccessory_Status.Text = ""

        End Select

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


End Class

