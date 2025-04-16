<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CLMainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CLMainForm))
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim ChartArea2 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Series2 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim ChartArea3 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Series3 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle12 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle13 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle10 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle11 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim ChartArea4 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series4 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Me.mnsMain = New System.Windows.Forms.MenuStrip()
        Me.tsmiFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiFile_GenerateReport = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.tsmiFile_SaveCommercialSheet = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiFile_SaveIOM = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.tsmiFile_Exit = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_EN = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_DE = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_FR = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_NL = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_PL = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_SL = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_RO = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_BG = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_HU = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_IT = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Language_DA = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Unit = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Unit_SI = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiOption_Unit_IP = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiBranchs = New System.Windows.Forms.ToolStripMenuItem()
        Me.tsmiAbout = New System.Windows.Forms.ToolStripMenuItem()
        Me.tbpPerformance = New System.Windows.Forms.TabPage()
        Me.spcPerformance = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.pnlPerformance_Data = New System.Windows.Forms.Panel()
        Me.grbPerformance_TemperatureConditions = New System.Windows.Forms.GroupBox()
        Me.btn_Default = New System.Windows.Forms.Button()
        Me.btnEN308 = New System.Windows.Forms.Button()
        Me.txbPerformance_RHReturnInlet = New System.Windows.Forms.TextBox()
        Me.txbPerformance_ReturnInletTemperature = New System.Windows.Forms.TextBox()
        Me.txbPerformance_RHFreshInlet = New System.Windows.Forms.TextBox()
        Me.txbPerformance_FreshInletTemperature = New System.Windows.Forms.TextBox()
        Me.lblPerformance_FreshInletTemperature = New System.Windows.Forms.Label()
        Me.lblPerformance_RHFreshInlet = New System.Windows.Forms.Label()
        Me.lblPerformance_ReturnInletTemperature = New System.Windows.Forms.Label()
        Me.lblPerformance_RHReturnInlet = New System.Windows.Forms.Label()
        Me.grbPerformance_UnitSelection = New System.Windows.Forms.GroupBox()
        Me.cmbPerformance_Series = New System.Windows.Forms.ComboBox()
        Me.cmbPerformance_HeatRecoveryModels = New System.Windows.Forms.ComboBox()
        Me.txbPerformance_MaxPressure = New System.Windows.Forms.TextBox()
        Me.txbPerformance_AirFlow = New System.Windows.Forms.TextBox()
        Me.lblPerformance_Series = New System.Windows.Forms.Label()
        Me.lblPerformance_Unit = New System.Windows.Forms.Label()
        Me.lblPerformance_AirFlow = New System.Windows.Forms.Label()
        Me.lblPerformance_MaxPressure = New System.Windows.Forms.Label()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.tbcData = New System.Windows.Forms.TabControl()
        Me.tbpData_Thermal = New System.Windows.Forms.TabPage()
        Me.grbPerformance_HeatExchangerPerformances = New System.Windows.Forms.GroupBox()
        Me.txbPerformance_LatentHeat = New System.Windows.Forms.TextBox()
        Me.txbPerformance_WaterProduced = New System.Windows.Forms.TextBox()
        Me.txbPerformance_SensibleHeat = New System.Windows.Forms.TextBox()
        Me.txbPerformance_HeatTransferred = New System.Windows.Forms.TextBox()
        Me.lblPerformance_Efficiency = New System.Windows.Forms.TextBox()
        Me.lblPerformance_LatentHeat = New System.Windows.Forms.Label()
        Me.lblPerformance_WaterProduced = New System.Windows.Forms.Label()
        Me.lblPerformance_SensibleHeat = New System.Windows.Forms.Label()
        Me.lblPerformance_HeatTransferred = New System.Windows.Forms.Label()
        Me.txbPerformance_Efficiency = New System.Windows.Forms.Label()
        Me.grbPerformance_TemperatureConditions2 = New System.Windows.Forms.GroupBox()
        Me.txbPerformance_SupplyOutletTemperature = New System.Windows.Forms.TextBox()
        Me.txbPerformance_ExhaustOutletTemperature = New System.Windows.Forms.TextBox()
        Me.txbPerformance_SupplyOutletRH = New System.Windows.Forms.TextBox()
        Me.txbPerformance_ExhaustOutletRH = New System.Windows.Forms.TextBox()
        Me.lblPerformance_ExhaustOutletRH = New System.Windows.Forms.Label()
        Me.lblPerformance_SupplyOutletTemperature = New System.Windows.Forms.Label()
        Me.lblPerformance_SupplyOutletRH = New System.Windows.Forms.Label()
        Me.lblPerformance_ExhaustOutletTemperature = New System.Windows.Forms.Label()
        Me.tbpData_ElectricalPerformances = New System.Windows.Forms.TabPage()
        Me.grbPerformance_ERP2018 = New System.Windows.Forms.GroupBox()
        Me.chbPerformance_ERP2018_ShowArea = New System.Windows.Forms.CheckBox()
        Me.grbPerformance_SEL = New System.Windows.Forms.GroupBox()
        Me.nudPerformance_SEL_Limit = New System.Windows.Forms.NumericUpDown()
        Me.txbPerformance_SEL = New System.Windows.Forms.TextBox()
        Me.lblPerformance_SEL_SEL = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.chbPerformance_SEL_ShowArea = New System.Windows.Forms.CheckBox()
        Me.txbPerformance_ElectricalPerformances_PowerInput = New System.Windows.Forms.TextBox()
        Me.lblPerformance_ElectricalPerformances_PowerInput = New System.Windows.Forms.Label()
        Me.grbPerformance_PassiveHaus = New System.Windows.Forms.GroupBox()
        Me.txbPerformance_PassiveHaus_Limit = New System.Windows.Forms.TextBox()
        Me.txbPerformance_PassiveHaus = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblPerformance_PassiveHaus_ElectricalEfficiency = New System.Windows.Forms.Label()
        Me.chbPerformance_PassiveHaus_ShowArea = New System.Windows.Forms.CheckBox()
        Me.grbPerformance_SFP = New System.Windows.Forms.GroupBox()
        Me.nudPerformance_SFP_Limit = New System.Windows.Forms.NumericUpDown()
        Me.txbPerformance_SFP = New System.Windows.Forms.TextBox()
        Me.lblPerformance_SFP_SFP = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chbPerformance_SFP_ShowArea = New System.Windows.Forms.CheckBox()
        Me.tbpData_SoundPower = New System.Windows.Forms.TabPage()
        Me.dgvPerformance_SoundPower = New System.Windows.Forms.DataGridView()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.PictureBox4 = New System.Windows.Forms.PictureBox()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.rdb_Q8 = New System.Windows.Forms.RadioButton()
        Me.rdb_Q4 = New System.Windows.Forms.RadioButton()
        Me.rdb_Q2 = New System.Windows.Forms.RadioButton()
        Me.chbSoundPerformances_16032 = New System.Windows.Forms.CheckBox()
        Me.chbSoundPerformances_addtoreport = New System.Windows.Forms.CheckBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.nudSoundPerformances_LP2 = New System.Windows.Forms.NumericUpDown()
        Me.nudSoundPerformances_LP1 = New System.Windows.Forms.NumericUpDown()
        Me.tbpData_ItemGenerator = New System.Windows.Forms.TabPage()
        Me.lblPerformance_ItemDescr = New System.Windows.Forms.Label()
        Me.lblPerformance_ItemCode = New System.Windows.Forms.Label()
        Me.txbPerformance_ItemCode = New System.Windows.Forms.TextBox()
        Me.grbPerformance_IGClimaAcc = New System.Windows.Forms.GroupBox()
        Me.chbPerformance_IPEHD = New System.Windows.Forms.CheckBox()
        Me.rdbPerformance_ICWD = New System.Windows.Forms.RadioButton()
        Me.rdbPerformance_none = New System.Windows.Forms.RadioButton()
        Me.rdbPerformance_IDXD = New System.Windows.Forms.RadioButton()
        Me.rdbPerformance_IEHD = New System.Windows.Forms.RadioButton()
        Me.rdbPerformance_IHCD = New System.Windows.Forms.RadioButton()
        Me.rdbPerformance_IHWD = New System.Windows.Forms.RadioButton()
        Me.grbPerformance_IGComm = New System.Windows.Forms.GroupBox()
        Me.chbPerformance_RFM = New System.Windows.Forms.CheckBox()
        Me.chbPerformance_MBUS = New System.Windows.Forms.CheckBox()
        Me.grbPerformance_IGSensor = New System.Windows.Forms.GroupBox()
        Me.rdbPerformance_RH = New System.Windows.Forms.RadioButton()
        Me.rdbPerformance_VOC = New System.Windows.Forms.RadioButton()
        Me.rdbPerformance_CO2 = New System.Windows.Forms.RadioButton()
        Me.tbpData_ItemGenerator_QTM = New System.Windows.Forms.TabPage()
        Me.grbPerformance_IGHeaterQTM = New System.Windows.Forms.GroupBox()
        Me.chb_qtm_IPEHD = New System.Windows.Forms.CheckBox()
        Me.grbPerformance_IGVersionQTM = New System.Windows.Forms.GroupBox()
        Me.rdb_qtm_easy = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_preplus = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_premium = New System.Windows.Forms.RadioButton()
        Me.grbPerformance_IGLayoutQTM = New System.Windows.Forms.GroupBox()
        Me.rdb_qtm_eos = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_osc = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_fos = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_ssc = New System.Windows.Forms.RadioButton()
        Me.lblPerformance_ItemDescrQTM = New System.Windows.Forms.Label()
        Me.lblPerformance_ItemCodeQTM = New System.Windows.Forms.Label()
        Me.txbPerformance_ItemCodeQTM = New System.Windows.Forms.TextBox()
        Me.grbPerformance_IGSensorQTM = New System.Windows.Forms.GroupBox()
        Me.rdb_qtm_sensnone = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_rh = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_voc = New System.Windows.Forms.RadioButton()
        Me.rdb_qtm_co2 = New System.Windows.Forms.RadioButton()
        Me.grbPerformance_IGCommQTM = New System.Windows.Forms.GroupBox()
        Me.chb_qtm_ACTIVA = New System.Windows.Forms.CheckBox()
        Me.chb_qtm_RFM = New System.Windows.Forms.CheckBox()
        Me.chb_qtm_KTS = New System.Windows.Forms.CheckBox()
        Me.chb_qtm_MBUS = New System.Windows.Forms.CheckBox()
        Me.pnlRegulationLevel = New System.Windows.Forms.Panel()
        Me.lblPerformance_RegulationLevel = New System.Windows.Forms.Label()
        Me.hsbPerformance_RegulationLevel = New System.Windows.Forms.HScrollBar()
        Me.lblPerformance_RegulationLevelValue = New System.Windows.Forms.Label()
        Me.prbPerformance_RegulationLevel = New System.Windows.Forms.ProgressBar()
        Me.m_Note_Text = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.tlpPerformance_Graphs = New System.Windows.Forms.TableLayoutPanel()
        Me.crtPerformance_Chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.crtPerformance_Chart2 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.crtPerformance_Chart3 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.tbcMain = New System.Windows.Forms.TabControl()
        Me.tbpCertification = New System.Windows.Forms.TabPage()
        Me.btnUpdateSAPTable = New System.Windows.Forms.Button()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.dgvSAP = New System.Windows.Forms.DataGridView()
        Me.dgvSAP_ExhaustTerminalConfiguration = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvSAP_TotalExhaustFlowRate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvSAP_TotalSupplyFlowRate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvSAP_RegulationLevel = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvSAP_SpecificFanPower = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvSAP_HeatExchangeEfficiency = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.tbpAccessory = New System.Windows.Forms.TabPage()
        Me.grbAccessory_OutputData = New System.Windows.Forms.GroupBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txbAccessory_OutputData_CWDSensibleHeat = New System.Windows.Forms.TextBox()
        Me.bsrUnitCalculator = New System.Windows.Forms.BindingSource(Me.components)
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txbAccessory_OutputData_CWDHeatTransferred = New System.Windows.Forms.TextBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.txbAccessory_OutputData_HWDHeatTransferred = New System.Windows.Forms.TextBox()
        Me.lblAccessory_Status = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.txbAccessory_OutputData_Temp = New System.Windows.Forms.TextBox()
        Me.txbAccessory_OutputData_Condensation = New System.Windows.Forms.TextBox()
        Me.txbAccessory_OutputData_RH = New System.Windows.Forms.TextBox()
        Me.txbAccessory_OutputData_PressureDrop = New System.Windows.Forms.TextBox()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.grbAccessory_EHD = New System.Windows.Forms.GroupBox()
        Me.cbxAccessory_EHDEnabled = New System.Windows.Forms.CheckBox()
        Me.grbAccessory_HWD = New System.Windows.Forms.GroupBox()
        Me.nudAccessory_HWD_FluidTypeTec = New System.Windows.Forms.NumericUpDown()
        Me.lblAccessory_HWD_OutletTemperature = New System.Windows.Forms.Label()
        Me.cmbAccessory_HWD_FluidType = New System.Windows.Forms.ComboBox()
        Me.lblAccessory_HWD_FluidTypeTec = New System.Windows.Forms.Label()
        Me.lblAccessory_HWD_FluidType = New System.Windows.Forms.Label()
        Me.lblAccessory_HWD_InletTemperature = New System.Windows.Forms.Label()
        Me.txbAccessory_HWD_InletTemperature = New System.Windows.Forms.TextBox()
        Me.txbAccessory_HWD_OutletTemperature = New System.Windows.Forms.TextBox()
        Me.cbxAccessory_HWDEnabled = New System.Windows.Forms.CheckBox()
        Me.grbAccessory_CWD = New System.Windows.Forms.GroupBox()
        Me.nudAccessory_CWD_FluidTypeTec = New System.Windows.Forms.NumericUpDown()
        Me.lblAccessory_CWD_OutletTemperature = New System.Windows.Forms.Label()
        Me.cbxAccessory_CWDEnabled = New System.Windows.Forms.CheckBox()
        Me.cmbAccessory_CWD_FluidType = New System.Windows.Forms.ComboBox()
        Me.lblAccessory_CWD_FluidTypeTec = New System.Windows.Forms.Label()
        Me.lblAccessory_CWD_FluidType = New System.Windows.Forms.Label()
        Me.lblAccessory_CWD_InletTemperature = New System.Windows.Forms.Label()
        Me.txbAccessory_CWD_InletTemperature = New System.Windows.Forms.TextBox()
        Me.txbAccessory_CWD_OutletTemperature = New System.Windows.Forms.TextBox()
        Me.grbAccessory_InputData = New System.Windows.Forms.GroupBox()
        Me.lblAccessory_InputData_Temp = New System.Windows.Forms.Label()
        Me.lblAccessory_InputData_RHTemp = New System.Windows.Forms.Label()
        Me.txbAccessory_InputData_Temp = New System.Windows.Forms.TextBox()
        Me.txbAccessory_InputData_RH = New System.Windows.Forms.TextBox()
        Me.txbAccessory_InputData_AirFlow = New System.Windows.Forms.TextBox()
        Me.lblAccessory_InputData_AirFlow = New System.Windows.Forms.Label()
        Me.tbpCO2Level = New System.Windows.Forms.TabPage()
        Me.chbCO2Level_addtoreport = New System.Windows.Forms.CheckBox()
        Me.crtCO2Level_Chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.grbCO2Level_Parameters = New System.Windows.Forms.GroupBox()
        Me.chbCO2Level_Parameters_airflow_check = New System.Windows.Forms.CheckBox()
        Me.lblCO2Level_Parameters_stdpreset = New System.Windows.Forms.Label()
        Me.cmbCO2Level_Parameters_stdpreset = New System.Windows.Forms.ComboBox()
        Me.lblCO2Level_Parameters_CalcMet = New System.Windows.Forms.Label()
        Me.cmbCO2Level_Parameters_CalcMet = New System.Windows.Forms.ComboBox()
        Me.lblCO2Level_Parameters_af_area = New System.Windows.Forms.Label()
        Me.txbCO2Level_Parameters_af_area = New System.Windows.Forms.TextBox()
        Me.lblCO2Level_Parameters_af_person = New System.Windows.Forms.Label()
        Me.txbCO2Level_Parameters_af_person = New System.Windows.Forms.TextBox()
        Me.lblCO2Level_Parameters_af_area_m3h = New System.Windows.Forms.Label()
        Me.lblCO2Level_Parameters_af_person_m3h = New System.Windows.Forms.Label()
        Me.lblCO2Level_Parameters_airflow_design_m3h = New System.Windows.Forms.Label()
        Me.lblCO2Level_Parameters_airflow_m3h = New System.Windows.Forms.Label()
        Me.lblCO2Level_Parameters_airflow_design = New System.Windows.Forms.Label()
        Me.lblCO2Level_Parameters_airflow = New System.Windows.Forms.Label()
        Me.txbCO2Level_Parameters_af_area_m3h = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Parameters_airflow_design_m3h = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Parameters_af_person_m3h = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Parameters_airflow_design = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Parameters_airflow_m3h = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Parameters_airflow = New System.Windows.Forms.TextBox()
        Me.lblCO2Level_Parameters_maxCO2 = New System.Windows.Forms.Label()
        Me.txbCO2Level_Parameters_maxCO2 = New System.Windows.Forms.TextBox()
        Me.lblCO2Level_Parameters_extCO2 = New System.Windows.Forms.Label()
        Me.txbCO2Level_Parameters_extCO2 = New System.Windows.Forms.TextBox()
        Me.grbCO2Level_use = New System.Windows.Forms.GroupBox()
        Me.lblCO2Level_Usage_Presence = New System.Windows.Forms.Label()
        Me.lblCO2Level_Usage_Break = New System.Windows.Forms.Label()
        Me.lblCO2Level_Usage_People = New System.Windows.Forms.Label()
        Me.lblCO2Level_Usage_CO2prod = New System.Windows.Forms.Label()
        Me.lblCO2Level_Usage_Activity = New System.Windows.Forms.Label()
        Me.lblCO2Level_Usage_Period = New System.Windows.Forms.Label()
        Me.txbCO2Level_Usage_CO2prod = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Usage_Activity = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Usage_PeopleBreak = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Usage_PeoplePresence = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Usage_PeriodBreak = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Usage_PeriodPresence = New System.Windows.Forms.TextBox()
        Me.grbCO2Level_Room = New System.Windows.Forms.GroupBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.lblCO2Level_Room_Height = New System.Windows.Forms.Label()
        Me.lblCO2Level_Room_Width = New System.Windows.Forms.Label()
        Me.lblCO2Level_Room_Length = New System.Windows.Forms.Label()
        Me.txbCO2Level_Room_Height = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Room_Width = New System.Windows.Forms.TextBox()
        Me.txbCO2Level_Room_Length = New System.Windows.Forms.TextBox()
        Me.sfdSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.pnlMain = New System.Windows.Forms.Panel()
        Me.sfdSavePdf = New System.Windows.Forms.SaveFileDialog()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.mnsMain.SuspendLayout()
        Me.tbpPerformance.SuspendLayout()
        CType(Me.spcPerformance, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.spcPerformance.Panel1.SuspendLayout()
        Me.spcPerformance.Panel2.SuspendLayout()
        Me.spcPerformance.SuspendLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.pnlPerformance_Data.SuspendLayout()
        Me.grbPerformance_TemperatureConditions.SuspendLayout()
        Me.grbPerformance_UnitSelection.SuspendLayout()
        Me.Panel4.SuspendLayout()
        Me.tbcData.SuspendLayout()
        Me.tbpData_Thermal.SuspendLayout()
        Me.grbPerformance_HeatExchangerPerformances.SuspendLayout()
        Me.grbPerformance_TemperatureConditions2.SuspendLayout()
        Me.tbpData_ElectricalPerformances.SuspendLayout()
        Me.grbPerformance_ERP2018.SuspendLayout()
        Me.grbPerformance_SEL.SuspendLayout()
        CType(Me.nudPerformance_SEL_Limit, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grbPerformance_PassiveHaus.SuspendLayout()
        Me.grbPerformance_SFP.SuspendLayout()
        CType(Me.nudPerformance_SFP_Limit, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tbpData_SoundPower.SuspendLayout()
        CType(Me.dgvPerformance_SoundPower, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudSoundPerformances_LP2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudSoundPerformances_LP1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tbpData_ItemGenerator.SuspendLayout()
        Me.grbPerformance_IGClimaAcc.SuspendLayout()
        Me.grbPerformance_IGComm.SuspendLayout()
        Me.grbPerformance_IGSensor.SuspendLayout()
        Me.tbpData_ItemGenerator_QTM.SuspendLayout()
        Me.grbPerformance_IGHeaterQTM.SuspendLayout()
        Me.grbPerformance_IGVersionQTM.SuspendLayout()
        Me.grbPerformance_IGLayoutQTM.SuspendLayout()
        Me.grbPerformance_IGSensorQTM.SuspendLayout()
        Me.grbPerformance_IGCommQTM.SuspendLayout()
        Me.pnlRegulationLevel.SuspendLayout()
        Me.tlpPerformance_Graphs.SuspendLayout()
        CType(Me.crtPerformance_Chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.crtPerformance_Chart2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.crtPerformance_Chart3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tbcMain.SuspendLayout()
        Me.tbpCertification.SuspendLayout()
        CType(Me.dgvSAP, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tbpAccessory.SuspendLayout()
        Me.grbAccessory_OutputData.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.bsrUnitCalculator, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        Me.grbAccessory_EHD.SuspendLayout()
        Me.grbAccessory_HWD.SuspendLayout()
        CType(Me.nudAccessory_HWD_FluidTypeTec, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grbAccessory_CWD.SuspendLayout()
        CType(Me.nudAccessory_CWD_FluidTypeTec, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grbAccessory_InputData.SuspendLayout()
        Me.tbpCO2Level.SuspendLayout()
        CType(Me.crtCO2Level_Chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grbCO2Level_Parameters.SuspendLayout()
        Me.grbCO2Level_use.SuspendLayout()
        Me.grbCO2Level_Room.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'mnsMain
        '
        Me.mnsMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsmiFile, Me.tsmiOption, Me.tsmiBranchs, Me.tsmiAbout})
        Me.mnsMain.Location = New System.Drawing.Point(0, 0)
        Me.mnsMain.Name = "mnsMain"
        Me.mnsMain.Size = New System.Drawing.Size(1130, 24)
        Me.mnsMain.TabIndex = 0
        '
        'tsmiFile
        '
        Me.tsmiFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsmiFile_GenerateReport, Me.ToolStripSeparator1, Me.tsmiFile_SaveCommercialSheet, Me.tsmiFile_SaveIOM, Me.ToolStripSeparator2, Me.tsmiFile_Exit})
        Me.tsmiFile.Name = "tsmiFile"
        Me.tsmiFile.Size = New System.Drawing.Size(37, 20)
        Me.tsmiFile.Text = "File"
        '
        'tsmiFile_GenerateReport
        '
        Me.tsmiFile_GenerateReport.Name = "tsmiFile_GenerateReport"
        Me.tsmiFile_GenerateReport.Size = New System.Drawing.Size(198, 22)
        Me.tsmiFile_GenerateReport.Text = "Generate Report"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(195, 6)
        '
        'tsmiFile_SaveCommercialSheet
        '
        Me.tsmiFile_SaveCommercialSheet.Name = "tsmiFile_SaveCommercialSheet"
        Me.tsmiFile_SaveCommercialSheet.Size = New System.Drawing.Size(198, 22)
        Me.tsmiFile_SaveCommercialSheet.Text = "Save Commercial Sheet"
        '
        'tsmiFile_SaveIOM
        '
        Me.tsmiFile_SaveIOM.Name = "tsmiFile_SaveIOM"
        Me.tsmiFile_SaveIOM.Size = New System.Drawing.Size(198, 22)
        Me.tsmiFile_SaveIOM.Text = "Save Iom"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(195, 6)
        '
        'tsmiFile_Exit
        '
        Me.tsmiFile_Exit.Name = "tsmiFile_Exit"
        Me.tsmiFile_Exit.Size = New System.Drawing.Size(198, 22)
        Me.tsmiFile_Exit.Text = "Exit"
        '
        'tsmiOption
        '
        Me.tsmiOption.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsmiOption_Language, Me.tsmiOption_Unit})
        Me.tsmiOption.Name = "tsmiOption"
        Me.tsmiOption.Size = New System.Drawing.Size(56, 20)
        Me.tsmiOption.Text = "Option"
        '
        'tsmiOption_Language
        '
        Me.tsmiOption_Language.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsmiOption_Language_EN, Me.tsmiOption_Language_DE, Me.tsmiOption_Language_FR, Me.tsmiOption_Language_NL, Me.tsmiOption_Language_PL, Me.tsmiOption_Language_SL, Me.tsmiOption_Language_RO, Me.tsmiOption_Language_BG, Me.tsmiOption_Language_HU, Me.tsmiOption_Language_IT, Me.tsmiOption_Language_DA})
        Me.tsmiOption_Language.Name = "tsmiOption_Language"
        Me.tsmiOption_Language.Size = New System.Drawing.Size(126, 22)
        Me.tsmiOption_Language.Text = "Language"
        '
        'tsmiOption_Language_EN
        '
        Me.tsmiOption_Language_EN.Name = "tsmiOption_Language_EN"
        Me.tsmiOption_Language_EN.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_EN.Text = "English"
        '
        'tsmiOption_Language_DE
        '
        Me.tsmiOption_Language_DE.Name = "tsmiOption_Language_DE"
        Me.tsmiOption_Language_DE.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_DE.Text = "Deutsch"
        '
        'tsmiOption_Language_FR
        '
        Me.tsmiOption_Language_FR.Name = "tsmiOption_Language_FR"
        Me.tsmiOption_Language_FR.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_FR.Text = "Français"
        '
        'tsmiOption_Language_NL
        '
        Me.tsmiOption_Language_NL.Name = "tsmiOption_Language_NL"
        Me.tsmiOption_Language_NL.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_NL.Text = "Dutch"
        '
        'tsmiOption_Language_PL
        '
        Me.tsmiOption_Language_PL.Name = "tsmiOption_Language_PL"
        Me.tsmiOption_Language_PL.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_PL.Text = "Polacco"
        '
        'tsmiOption_Language_SL
        '
        Me.tsmiOption_Language_SL.Name = "tsmiOption_Language_SL"
        Me.tsmiOption_Language_SL.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_SL.Text = "Sloveno"
        '
        'tsmiOption_Language_RO
        '
        Me.tsmiOption_Language_RO.Name = "tsmiOption_Language_RO"
        Me.tsmiOption_Language_RO.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_RO.Text = "Rumeno"
        '
        'tsmiOption_Language_BG
        '
        Me.tsmiOption_Language_BG.Name = "tsmiOption_Language_BG"
        Me.tsmiOption_Language_BG.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_BG.Text = "Bulgaro"
        '
        'tsmiOption_Language_HU
        '
        Me.tsmiOption_Language_HU.Name = "tsmiOption_Language_HU"
        Me.tsmiOption_Language_HU.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_HU.Text = "Hungarian"
        '
        'tsmiOption_Language_IT
        '
        Me.tsmiOption_Language_IT.Name = "tsmiOption_Language_IT"
        Me.tsmiOption_Language_IT.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_IT.Text = "Italiano"
        '
        'tsmiOption_Language_DA
        '
        Me.tsmiOption_Language_DA.Name = "tsmiOption_Language_DA"
        Me.tsmiOption_Language_DA.Size = New System.Drawing.Size(130, 22)
        Me.tsmiOption_Language_DA.Text = "Dansk"
        '
        'tsmiOption_Unit
        '
        Me.tsmiOption_Unit.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsmiOption_Unit_SI, Me.tsmiOption_Unit_IP})
        Me.tsmiOption_Unit.Name = "tsmiOption_Unit"
        Me.tsmiOption_Unit.Size = New System.Drawing.Size(126, 22)
        Me.tsmiOption_Unit.Text = "Unit"
        Me.tsmiOption_Unit.Visible = False
        '
        'tsmiOption_Unit_SI
        '
        Me.tsmiOption_Unit_SI.Checked = True
        Me.tsmiOption_Unit_SI.CheckOnClick = True
        Me.tsmiOption_Unit_SI.CheckState = System.Windows.Forms.CheckState.Checked
        Me.tsmiOption_Unit_SI.Name = "tsmiOption_Unit_SI"
        Me.tsmiOption_Unit_SI.Size = New System.Drawing.Size(90, 22)
        Me.tsmiOption_Unit_SI.Text = "S.I."
        '
        'tsmiOption_Unit_IP
        '
        Me.tsmiOption_Unit_IP.CheckOnClick = True
        Me.tsmiOption_Unit_IP.Enabled = False
        Me.tsmiOption_Unit_IP.Name = "tsmiOption_Unit_IP"
        Me.tsmiOption_Unit_IP.Size = New System.Drawing.Size(90, 22)
        Me.tsmiOption_Unit_IP.Text = "I.P."
        '
        'tsmiBranchs
        '
        Me.tsmiBranchs.Name = "tsmiBranchs"
        Me.tsmiBranchs.Size = New System.Drawing.Size(67, 20)
        Me.tsmiBranchs.Text = "Branches"
        '
        'tsmiAbout
        '
        Me.tsmiAbout.Name = "tsmiAbout"
        Me.tsmiAbout.Size = New System.Drawing.Size(52, 20)
        Me.tsmiAbout.Text = "About"
        '
        'tbpPerformance
        '
        Me.tbpPerformance.Controls.Add(Me.spcPerformance)
        Me.tbpPerformance.Location = New System.Drawing.Point(4, 22)
        Me.tbpPerformance.Name = "tbpPerformance"
        Me.tbpPerformance.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpPerformance.Size = New System.Drawing.Size(1112, 607)
        Me.tbpPerformance.TabIndex = 0
        Me.tbpPerformance.Text = "Performance"
        Me.tbpPerformance.UseVisualStyleBackColor = True
        '
        'spcPerformance
        '
        Me.spcPerformance.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.spcPerformance.Dock = System.Windows.Forms.DockStyle.Fill
        Me.spcPerformance.Location = New System.Drawing.Point(3, 3)
        Me.spcPerformance.Name = "spcPerformance"
        Me.spcPerformance.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'spcPerformance.Panel1
        '
        Me.spcPerformance.Panel1.Controls.Add(Me.SplitContainer2)
        '
        'spcPerformance.Panel2
        '
        Me.spcPerformance.Panel2.Controls.Add(Me.tlpPerformance_Graphs)
        Me.spcPerformance.Size = New System.Drawing.Size(1106, 601)
        Me.spcPerformance.SplitterDistance = 376
        Me.spcPerformance.TabIndex = 2
        '
        'SplitContainer2
        '
        Me.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer2.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer2.Name = "SplitContainer2"
        Me.SplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.SplitContainer1)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.m_Note_Text)
        Me.SplitContainer2.Panel2.Controls.Add(Me.Label8)
        Me.SplitContainer2.Size = New System.Drawing.Size(1104, 374)
        Me.SplitContainer2.SplitterDistance = 311
        Me.SplitContainer2.TabIndex = 2
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.IsSplitterFixed = True
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.pnlPerformance_Data)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.Panel4)
        Me.SplitContainer1.Size = New System.Drawing.Size(1104, 311)
        Me.SplitContainer1.SplitterDistance = 380
        Me.SplitContainer1.TabIndex = 6
        '
        'pnlPerformance_Data
        '
        Me.pnlPerformance_Data.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlPerformance_Data.AutoScroll = True
        Me.pnlPerformance_Data.Controls.Add(Me.grbPerformance_TemperatureConditions)
        Me.pnlPerformance_Data.Controls.Add(Me.grbPerformance_UnitSelection)
        Me.pnlPerformance_Data.Location = New System.Drawing.Point(0, 3)
        Me.pnlPerformance_Data.Name = "pnlPerformance_Data"
        Me.pnlPerformance_Data.Size = New System.Drawing.Size(377, 303)
        Me.pnlPerformance_Data.TabIndex = 12
        '
        'grbPerformance_TemperatureConditions
        '
        Me.grbPerformance_TemperatureConditions.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.btn_Default)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.btnEN308)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.txbPerformance_RHReturnInlet)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.txbPerformance_ReturnInletTemperature)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.txbPerformance_RHFreshInlet)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.txbPerformance_FreshInletTemperature)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.lblPerformance_FreshInletTemperature)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.lblPerformance_RHFreshInlet)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.lblPerformance_ReturnInletTemperature)
        Me.grbPerformance_TemperatureConditions.Controls.Add(Me.lblPerformance_RHReturnInlet)
        Me.grbPerformance_TemperatureConditions.Location = New System.Drawing.Point(7, 150)
        Me.grbPerformance_TemperatureConditions.Name = "grbPerformance_TemperatureConditions"
        Me.grbPerformance_TemperatureConditions.Size = New System.Drawing.Size(359, 152)
        Me.grbPerformance_TemperatureConditions.TabIndex = 1
        Me.grbPerformance_TemperatureConditions.TabStop = False
        Me.grbPerformance_TemperatureConditions.Text = "Temperature Conditions"
        '
        'btn_Default
        '
        Me.btn_Default.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_Default.Location = New System.Drawing.Point(227, 79)
        Me.btn_Default.Name = "btn_Default"
        Me.btn_Default.Size = New System.Drawing.Size(126, 49)
        Me.btn_Default.TabIndex = 8
        Me.btn_Default.Text = "Default"
        Me.btn_Default.UseVisualStyleBackColor = True
        '
        'btnEN308
        '
        Me.btnEN308.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnEN308.Location = New System.Drawing.Point(227, 21)
        Me.btnEN308.Name = "btnEN308"
        Me.btnEN308.Size = New System.Drawing.Size(126, 49)
        Me.btnEN308.TabIndex = 8
        Me.btnEN308.Text = "EN308"
        Me.btnEN308.UseVisualStyleBackColor = True
        '
        'txbPerformance_RHReturnInlet
        '
        Me.txbPerformance_RHReturnInlet.AcceptsReturn = True
        Me.txbPerformance_RHReturnInlet.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txbPerformance_RHReturnInlet.Location = New System.Drawing.Point(161, 110)
        Me.txbPerformance_RHReturnInlet.Name = "txbPerformance_RHReturnInlet"
        Me.txbPerformance_RHReturnInlet.Size = New System.Drawing.Size(60, 20)
        Me.txbPerformance_RHReturnInlet.TabIndex = 7
        Me.txbPerformance_RHReturnInlet.Text = "60"
        '
        'txbPerformance_ReturnInletTemperature
        '
        Me.txbPerformance_ReturnInletTemperature.AcceptsReturn = True
        Me.txbPerformance_ReturnInletTemperature.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txbPerformance_ReturnInletTemperature.Location = New System.Drawing.Point(161, 79)
        Me.txbPerformance_ReturnInletTemperature.Name = "txbPerformance_ReturnInletTemperature"
        Me.txbPerformance_ReturnInletTemperature.Size = New System.Drawing.Size(60, 20)
        Me.txbPerformance_ReturnInletTemperature.TabIndex = 5
        Me.txbPerformance_ReturnInletTemperature.Text = "20"
        '
        'txbPerformance_RHFreshInlet
        '
        Me.txbPerformance_RHFreshInlet.AcceptsReturn = True
        Me.txbPerformance_RHFreshInlet.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txbPerformance_RHFreshInlet.Location = New System.Drawing.Point(161, 50)
        Me.txbPerformance_RHFreshInlet.Name = "txbPerformance_RHFreshInlet"
        Me.txbPerformance_RHFreshInlet.Size = New System.Drawing.Size(60, 20)
        Me.txbPerformance_RHFreshInlet.TabIndex = 3
        Me.txbPerformance_RHFreshInlet.Text = "80"
        '
        'txbPerformance_FreshInletTemperature
        '
        Me.txbPerformance_FreshInletTemperature.AcceptsReturn = True
        Me.txbPerformance_FreshInletTemperature.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txbPerformance_FreshInletTemperature.Location = New System.Drawing.Point(161, 21)
        Me.txbPerformance_FreshInletTemperature.Name = "txbPerformance_FreshInletTemperature"
        Me.txbPerformance_FreshInletTemperature.Size = New System.Drawing.Size(60, 20)
        Me.txbPerformance_FreshInletTemperature.TabIndex = 1
        Me.txbPerformance_FreshInletTemperature.Text = "-10"
        '
        'lblPerformance_FreshInletTemperature
        '
        Me.lblPerformance_FreshInletTemperature.Location = New System.Drawing.Point(6, 17)
        Me.lblPerformance_FreshInletTemperature.Name = "lblPerformance_FreshInletTemperature"
        Me.lblPerformance_FreshInletTemperature.Size = New System.Drawing.Size(163, 26)
        Me.lblPerformance_FreshInletTemperature.TabIndex = 0
        Me.lblPerformance_FreshInletTemperature.Text = "Fresh Inlet Temp. [°C]"
        Me.lblPerformance_FreshInletTemperature.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_RHFreshInlet
        '
        Me.lblPerformance_RHFreshInlet.Location = New System.Drawing.Point(6, 46)
        Me.lblPerformance_RHFreshInlet.Name = "lblPerformance_RHFreshInlet"
        Me.lblPerformance_RHFreshInlet.Size = New System.Drawing.Size(163, 26)
        Me.lblPerformance_RHFreshInlet.TabIndex = 2
        Me.lblPerformance_RHFreshInlet.Text = "R.H. [%]"
        Me.lblPerformance_RHFreshInlet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_ReturnInletTemperature
        '
        Me.lblPerformance_ReturnInletTemperature.Location = New System.Drawing.Point(6, 75)
        Me.lblPerformance_ReturnInletTemperature.Name = "lblPerformance_ReturnInletTemperature"
        Me.lblPerformance_ReturnInletTemperature.Size = New System.Drawing.Size(163, 26)
        Me.lblPerformance_ReturnInletTemperature.TabIndex = 4
        Me.lblPerformance_ReturnInletTemperature.Text = "Return Inlet Temp. [°C]"
        Me.lblPerformance_ReturnInletTemperature.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_RHReturnInlet
        '
        Me.lblPerformance_RHReturnInlet.Location = New System.Drawing.Point(6, 104)
        Me.lblPerformance_RHReturnInlet.Name = "lblPerformance_RHReturnInlet"
        Me.lblPerformance_RHReturnInlet.Size = New System.Drawing.Size(163, 26)
        Me.lblPerformance_RHReturnInlet.TabIndex = 6
        Me.lblPerformance_RHReturnInlet.Text = "R.H. [%]"
        Me.lblPerformance_RHReturnInlet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'grbPerformance_UnitSelection
        '
        Me.grbPerformance_UnitSelection.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.cmbPerformance_Series)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.cmbPerformance_HeatRecoveryModels)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.txbPerformance_MaxPressure)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.txbPerformance_AirFlow)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.lblPerformance_Series)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.lblPerformance_Unit)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.lblPerformance_AirFlow)
        Me.grbPerformance_UnitSelection.Controls.Add(Me.lblPerformance_MaxPressure)
        Me.grbPerformance_UnitSelection.Location = New System.Drawing.Point(7, 3)
        Me.grbPerformance_UnitSelection.Name = "grbPerformance_UnitSelection"
        Me.grbPerformance_UnitSelection.Size = New System.Drawing.Size(359, 138)
        Me.grbPerformance_UnitSelection.TabIndex = 0
        Me.grbPerformance_UnitSelection.TabStop = False
        Me.grbPerformance_UnitSelection.Text = "Unit Selection"
        '
        'cmbPerformance_Series
        '
        Me.cmbPerformance_Series.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbPerformance_Series.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPerformance_Series.FormattingEnabled = True
        Me.cmbPerformance_Series.Items.AddRange(New Object() {"CLRC 123", "CLRC 163", "CLRC 223", "CLRC 23", "CLRC 323", "CLRC 423", "CLRC 53", "CLRC 93"})
        Me.cmbPerformance_Series.Location = New System.Drawing.Point(161, 14)
        Me.cmbPerformance_Series.Name = "cmbPerformance_Series"
        Me.cmbPerformance_Series.Size = New System.Drawing.Size(192, 21)
        Me.cmbPerformance_Series.Sorted = True
        Me.cmbPerformance_Series.TabIndex = 1
        '
        'cmbPerformance_HeatRecoveryModels
        '
        Me.cmbPerformance_HeatRecoveryModels.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbPerformance_HeatRecoveryModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPerformance_HeatRecoveryModels.FormattingEnabled = True
        Me.cmbPerformance_HeatRecoveryModels.Items.AddRange(New Object() {"CLRC 123", "CLRC 163", "CLRC 223", "CLRC 23", "CLRC 323", "CLRC 423", "CLRC 53", "CLRC 93"})
        Me.cmbPerformance_HeatRecoveryModels.Location = New System.Drawing.Point(161, 45)
        Me.cmbPerformance_HeatRecoveryModels.Name = "cmbPerformance_HeatRecoveryModels"
        Me.cmbPerformance_HeatRecoveryModels.Size = New System.Drawing.Size(192, 21)
        Me.cmbPerformance_HeatRecoveryModels.Sorted = True
        Me.cmbPerformance_HeatRecoveryModels.TabIndex = 1
        '
        'txbPerformance_MaxPressure
        '
        Me.txbPerformance_MaxPressure.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txbPerformance_MaxPressure.Location = New System.Drawing.Point(161, 107)
        Me.txbPerformance_MaxPressure.Name = "txbPerformance_MaxPressure"
        Me.txbPerformance_MaxPressure.ReadOnly = True
        Me.txbPerformance_MaxPressure.Size = New System.Drawing.Size(192, 20)
        Me.txbPerformance_MaxPressure.TabIndex = 5
        '
        'txbPerformance_AirFlow
        '
        Me.txbPerformance_AirFlow.AcceptsReturn = True
        Me.txbPerformance_AirFlow.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txbPerformance_AirFlow.Location = New System.Drawing.Point(161, 77)
        Me.txbPerformance_AirFlow.Name = "txbPerformance_AirFlow"
        Me.txbPerformance_AirFlow.Size = New System.Drawing.Size(192, 20)
        Me.txbPerformance_AirFlow.TabIndex = 3
        Me.txbPerformance_AirFlow.Text = "100"
        '
        'lblPerformance_Series
        '
        Me.lblPerformance_Series.AutoSize = True
        Me.lblPerformance_Series.Location = New System.Drawing.Point(6, 17)
        Me.lblPerformance_Series.Name = "lblPerformance_Series"
        Me.lblPerformance_Series.Size = New System.Drawing.Size(31, 13)
        Me.lblPerformance_Series.TabIndex = 0
        Me.lblPerformance_Series.Text = "Serie"
        '
        'lblPerformance_Unit
        '
        Me.lblPerformance_Unit.AutoSize = True
        Me.lblPerformance_Unit.Location = New System.Drawing.Point(6, 48)
        Me.lblPerformance_Unit.Name = "lblPerformance_Unit"
        Me.lblPerformance_Unit.Size = New System.Drawing.Size(26, 13)
        Me.lblPerformance_Unit.TabIndex = 0
        Me.lblPerformance_Unit.Text = "Unit"
        '
        'lblPerformance_AirFlow
        '
        Me.lblPerformance_AirFlow.Location = New System.Drawing.Point(6, 74)
        Me.lblPerformance_AirFlow.Name = "lblPerformance_AirFlow"
        Me.lblPerformance_AirFlow.Size = New System.Drawing.Size(149, 26)
        Me.lblPerformance_AirFlow.TabIndex = 2
        Me.lblPerformance_AirFlow.Text = "Air Flow [m3/h]"
        Me.lblPerformance_AirFlow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_MaxPressure
        '
        Me.lblPerformance_MaxPressure.Location = New System.Drawing.Point(6, 106)
        Me.lblPerformance_MaxPressure.Name = "lblPerformance_MaxPressure"
        Me.lblPerformance_MaxPressure.Size = New System.Drawing.Size(149, 23)
        Me.lblPerformance_MaxPressure.TabIndex = 4
        Me.lblPerformance_MaxPressure.Text = "Max. Pressure [Pa]"
        Me.lblPerformance_MaxPressure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Panel4
        '
        Me.Panel4.Controls.Add(Me.tbcData)
        Me.Panel4.Controls.Add(Me.pnlRegulationLevel)
        Me.Panel4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel4.Location = New System.Drawing.Point(0, 0)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(720, 311)
        Me.Panel4.TabIndex = 12
        '
        'tbcData
        '
        Me.tbcData.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbcData.Controls.Add(Me.tbpData_Thermal)
        Me.tbcData.Controls.Add(Me.tbpData_ElectricalPerformances)
        Me.tbcData.Controls.Add(Me.tbpData_SoundPower)
        Me.tbcData.Controls.Add(Me.tbpData_ItemGenerator)
        Me.tbcData.Controls.Add(Me.tbpData_ItemGenerator_QTM)
        Me.tbcData.Location = New System.Drawing.Point(3, 65)
        Me.tbcData.Multiline = True
        Me.tbcData.Name = "tbcData"
        Me.tbcData.SelectedIndex = 0
        Me.tbcData.Size = New System.Drawing.Size(712, 241)
        Me.tbcData.TabIndex = 12
        '
        'tbpData_Thermal
        '
        Me.tbpData_Thermal.AutoScroll = True
        Me.tbpData_Thermal.Controls.Add(Me.grbPerformance_HeatExchangerPerformances)
        Me.tbpData_Thermal.Controls.Add(Me.grbPerformance_TemperatureConditions2)
        Me.tbpData_Thermal.Location = New System.Drawing.Point(4, 22)
        Me.tbpData_Thermal.Name = "tbpData_Thermal"
        Me.tbpData_Thermal.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpData_Thermal.Size = New System.Drawing.Size(704, 215)
        Me.tbpData_Thermal.TabIndex = 0
        Me.tbpData_Thermal.Text = "Heat Exchanger Performances"
        Me.tbpData_Thermal.UseVisualStyleBackColor = True
        '
        'grbPerformance_HeatExchangerPerformances
        '
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.txbPerformance_LatentHeat)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.txbPerformance_WaterProduced)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.txbPerformance_SensibleHeat)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.txbPerformance_HeatTransferred)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.lblPerformance_Efficiency)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.lblPerformance_LatentHeat)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.lblPerformance_WaterProduced)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.lblPerformance_SensibleHeat)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.lblPerformance_HeatTransferred)
        Me.grbPerformance_HeatExchangerPerformances.Controls.Add(Me.txbPerformance_Efficiency)
        Me.grbPerformance_HeatExchangerPerformances.Location = New System.Drawing.Point(6, 10)
        Me.grbPerformance_HeatExchangerPerformances.Name = "grbPerformance_HeatExchangerPerformances"
        Me.grbPerformance_HeatExchangerPerformances.Size = New System.Drawing.Size(507, 108)
        Me.grbPerformance_HeatExchangerPerformances.TabIndex = 1
        Me.grbPerformance_HeatExchangerPerformances.TabStop = False
        Me.grbPerformance_HeatExchangerPerformances.Text = "Heat Exchanger Performances"
        '
        'txbPerformance_LatentHeat
        '
        Me.txbPerformance_LatentHeat.Location = New System.Drawing.Point(190, 81)
        Me.txbPerformance_LatentHeat.Name = "txbPerformance_LatentHeat"
        Me.txbPerformance_LatentHeat.ReadOnly = True
        Me.txbPerformance_LatentHeat.Size = New System.Drawing.Size(63, 20)
        Me.txbPerformance_LatentHeat.TabIndex = 9
        '
        'txbPerformance_WaterProduced
        '
        Me.txbPerformance_WaterProduced.Location = New System.Drawing.Point(437, 53)
        Me.txbPerformance_WaterProduced.Name = "txbPerformance_WaterProduced"
        Me.txbPerformance_WaterProduced.ReadOnly = True
        Me.txbPerformance_WaterProduced.Size = New System.Drawing.Size(64, 20)
        Me.txbPerformance_WaterProduced.TabIndex = 7
        '
        'txbPerformance_SensibleHeat
        '
        Me.txbPerformance_SensibleHeat.Location = New System.Drawing.Point(190, 53)
        Me.txbPerformance_SensibleHeat.Name = "txbPerformance_SensibleHeat"
        Me.txbPerformance_SensibleHeat.ReadOnly = True
        Me.txbPerformance_SensibleHeat.Size = New System.Drawing.Size(63, 20)
        Me.txbPerformance_SensibleHeat.TabIndex = 5
        '
        'txbPerformance_HeatTransferred
        '
        Me.txbPerformance_HeatTransferred.Location = New System.Drawing.Point(190, 24)
        Me.txbPerformance_HeatTransferred.Name = "txbPerformance_HeatTransferred"
        Me.txbPerformance_HeatTransferred.ReadOnly = True
        Me.txbPerformance_HeatTransferred.Size = New System.Drawing.Size(63, 20)
        Me.txbPerformance_HeatTransferred.TabIndex = 1
        '
        'lblPerformance_Efficiency
        '
        Me.lblPerformance_Efficiency.Location = New System.Drawing.Point(436, 24)
        Me.lblPerformance_Efficiency.Name = "lblPerformance_Efficiency"
        Me.lblPerformance_Efficiency.ReadOnly = True
        Me.lblPerformance_Efficiency.Size = New System.Drawing.Size(64, 20)
        Me.lblPerformance_Efficiency.TabIndex = 3
        '
        'lblPerformance_LatentHeat
        '
        Me.lblPerformance_LatentHeat.Location = New System.Drawing.Point(6, 77)
        Me.lblPerformance_LatentHeat.Name = "lblPerformance_LatentHeat"
        Me.lblPerformance_LatentHeat.Size = New System.Drawing.Size(178, 26)
        Me.lblPerformance_LatentHeat.TabIndex = 8
        Me.lblPerformance_LatentHeat.Text = "Latent Heat [W]"
        Me.lblPerformance_LatentHeat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_WaterProduced
        '
        Me.lblPerformance_WaterProduced.Location = New System.Drawing.Point(260, 51)
        Me.lblPerformance_WaterProduced.Name = "lblPerformance_WaterProduced"
        Me.lblPerformance_WaterProduced.Size = New System.Drawing.Size(171, 26)
        Me.lblPerformance_WaterProduced.TabIndex = 6
        Me.lblPerformance_WaterProduced.Text = "Water produced [l/h]"
        Me.lblPerformance_WaterProduced.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_SensibleHeat
        '
        Me.lblPerformance_SensibleHeat.Location = New System.Drawing.Point(6, 49)
        Me.lblPerformance_SensibleHeat.Name = "lblPerformance_SensibleHeat"
        Me.lblPerformance_SensibleHeat.Size = New System.Drawing.Size(178, 26)
        Me.lblPerformance_SensibleHeat.TabIndex = 4
        Me.lblPerformance_SensibleHeat.Text = "Sensible Heat [W]"
        Me.lblPerformance_SensibleHeat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_HeatTransferred
        '
        Me.lblPerformance_HeatTransferred.Location = New System.Drawing.Point(6, 20)
        Me.lblPerformance_HeatTransferred.Name = "lblPerformance_HeatTransferred"
        Me.lblPerformance_HeatTransferred.Size = New System.Drawing.Size(178, 26)
        Me.lblPerformance_HeatTransferred.TabIndex = 0
        Me.lblPerformance_HeatTransferred.Text = "Heat Transferred [W]"
        Me.lblPerformance_HeatTransferred.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txbPerformance_Efficiency
        '
        Me.txbPerformance_Efficiency.Location = New System.Drawing.Point(259, 20)
        Me.txbPerformance_Efficiency.Name = "txbPerformance_Efficiency"
        Me.txbPerformance_Efficiency.Size = New System.Drawing.Size(171, 26)
        Me.txbPerformance_Efficiency.TabIndex = 2
        Me.txbPerformance_Efficiency.Text = "Eff. [%]"
        Me.txbPerformance_Efficiency.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'grbPerformance_TemperatureConditions2
        '
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.txbPerformance_SupplyOutletTemperature)
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.txbPerformance_ExhaustOutletTemperature)
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.txbPerformance_SupplyOutletRH)
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.txbPerformance_ExhaustOutletRH)
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.lblPerformance_ExhaustOutletRH)
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.lblPerformance_SupplyOutletTemperature)
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.lblPerformance_SupplyOutletRH)
        Me.grbPerformance_TemperatureConditions2.Controls.Add(Me.lblPerformance_ExhaustOutletTemperature)
        Me.grbPerformance_TemperatureConditions2.Location = New System.Drawing.Point(6, 124)
        Me.grbPerformance_TemperatureConditions2.Name = "grbPerformance_TemperatureConditions2"
        Me.grbPerformance_TemperatureConditions2.Size = New System.Drawing.Size(507, 81)
        Me.grbPerformance_TemperatureConditions2.TabIndex = 2
        Me.grbPerformance_TemperatureConditions2.TabStop = False
        Me.grbPerformance_TemperatureConditions2.Text = "Temperature Conditions"
        '
        'txbPerformance_SupplyOutletTemperature
        '
        Me.txbPerformance_SupplyOutletTemperature.Location = New System.Drawing.Point(190, 23)
        Me.txbPerformance_SupplyOutletTemperature.Name = "txbPerformance_SupplyOutletTemperature"
        Me.txbPerformance_SupplyOutletTemperature.ReadOnly = True
        Me.txbPerformance_SupplyOutletTemperature.Size = New System.Drawing.Size(63, 20)
        Me.txbPerformance_SupplyOutletTemperature.TabIndex = 1
        '
        'txbPerformance_ExhaustOutletTemperature
        '
        Me.txbPerformance_ExhaustOutletTemperature.Location = New System.Drawing.Point(436, 21)
        Me.txbPerformance_ExhaustOutletTemperature.Name = "txbPerformance_ExhaustOutletTemperature"
        Me.txbPerformance_ExhaustOutletTemperature.ReadOnly = True
        Me.txbPerformance_ExhaustOutletTemperature.Size = New System.Drawing.Size(63, 20)
        Me.txbPerformance_ExhaustOutletTemperature.TabIndex = 5
        '
        'txbPerformance_SupplyOutletRH
        '
        Me.txbPerformance_SupplyOutletRH.Location = New System.Drawing.Point(190, 52)
        Me.txbPerformance_SupplyOutletRH.Name = "txbPerformance_SupplyOutletRH"
        Me.txbPerformance_SupplyOutletRH.ReadOnly = True
        Me.txbPerformance_SupplyOutletRH.Size = New System.Drawing.Size(63, 20)
        Me.txbPerformance_SupplyOutletRH.TabIndex = 3
        '
        'txbPerformance_ExhaustOutletRH
        '
        Me.txbPerformance_ExhaustOutletRH.Location = New System.Drawing.Point(436, 50)
        Me.txbPerformance_ExhaustOutletRH.Name = "txbPerformance_ExhaustOutletRH"
        Me.txbPerformance_ExhaustOutletRH.ReadOnly = True
        Me.txbPerformance_ExhaustOutletRH.Size = New System.Drawing.Size(63, 20)
        Me.txbPerformance_ExhaustOutletRH.TabIndex = 7
        '
        'lblPerformance_ExhaustOutletRH
        '
        Me.lblPerformance_ExhaustOutletRH.Location = New System.Drawing.Point(259, 46)
        Me.lblPerformance_ExhaustOutletRH.Name = "lblPerformance_ExhaustOutletRH"
        Me.lblPerformance_ExhaustOutletRH.Size = New System.Drawing.Size(171, 26)
        Me.lblPerformance_ExhaustOutletRH.TabIndex = 6
        Me.lblPerformance_ExhaustOutletRH.Text = "R.H. [%]"
        Me.lblPerformance_ExhaustOutletRH.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_SupplyOutletTemperature
        '
        Me.lblPerformance_SupplyOutletTemperature.Location = New System.Drawing.Point(6, 17)
        Me.lblPerformance_SupplyOutletTemperature.Name = "lblPerformance_SupplyOutletTemperature"
        Me.lblPerformance_SupplyOutletTemperature.Size = New System.Drawing.Size(178, 26)
        Me.lblPerformance_SupplyOutletTemperature.TabIndex = 0
        Me.lblPerformance_SupplyOutletTemperature.Text = "Supply Outlet Temp. [°C]"
        Me.lblPerformance_SupplyOutletTemperature.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_SupplyOutletRH
        '
        Me.lblPerformance_SupplyOutletRH.Location = New System.Drawing.Point(6, 46)
        Me.lblPerformance_SupplyOutletRH.Name = "lblPerformance_SupplyOutletRH"
        Me.lblPerformance_SupplyOutletRH.Size = New System.Drawing.Size(178, 26)
        Me.lblPerformance_SupplyOutletRH.TabIndex = 2
        Me.lblPerformance_SupplyOutletRH.Text = "R.H. [%]"
        Me.lblPerformance_SupplyOutletRH.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_ExhaustOutletTemperature
        '
        Me.lblPerformance_ExhaustOutletTemperature.Location = New System.Drawing.Point(259, 17)
        Me.lblPerformance_ExhaustOutletTemperature.Name = "lblPerformance_ExhaustOutletTemperature"
        Me.lblPerformance_ExhaustOutletTemperature.Size = New System.Drawing.Size(171, 26)
        Me.lblPerformance_ExhaustOutletTemperature.TabIndex = 4
        Me.lblPerformance_ExhaustOutletTemperature.Text = "Exhaust Outlet Temp. [°C]"
        Me.lblPerformance_ExhaustOutletTemperature.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'tbpData_ElectricalPerformances
        '
        Me.tbpData_ElectricalPerformances.AutoScroll = True
        Me.tbpData_ElectricalPerformances.Controls.Add(Me.grbPerformance_ERP2018)
        Me.tbpData_ElectricalPerformances.Controls.Add(Me.grbPerformance_SEL)
        Me.tbpData_ElectricalPerformances.Controls.Add(Me.txbPerformance_ElectricalPerformances_PowerInput)
        Me.tbpData_ElectricalPerformances.Controls.Add(Me.lblPerformance_ElectricalPerformances_PowerInput)
        Me.tbpData_ElectricalPerformances.Controls.Add(Me.grbPerformance_PassiveHaus)
        Me.tbpData_ElectricalPerformances.Controls.Add(Me.grbPerformance_SFP)
        Me.tbpData_ElectricalPerformances.Location = New System.Drawing.Point(4, 22)
        Me.tbpData_ElectricalPerformances.Name = "tbpData_ElectricalPerformances"
        Me.tbpData_ElectricalPerformances.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpData_ElectricalPerformances.Size = New System.Drawing.Size(704, 215)
        Me.tbpData_ElectricalPerformances.TabIndex = 1
        Me.tbpData_ElectricalPerformances.Text = "Electrical Performances"
        Me.tbpData_ElectricalPerformances.UseVisualStyleBackColor = True
        '
        'grbPerformance_ERP2018
        '
        Me.grbPerformance_ERP2018.Controls.Add(Me.chbPerformance_ERP2018_ShowArea)
        Me.grbPerformance_ERP2018.Location = New System.Drawing.Point(6, 177)
        Me.grbPerformance_ERP2018.Name = "grbPerformance_ERP2018"
        Me.grbPerformance_ERP2018.Size = New System.Drawing.Size(440, 44)
        Me.grbPerformance_ERP2018.TabIndex = 5
        Me.grbPerformance_ERP2018.TabStop = False
        Me.grbPerformance_ERP2018.Text = "ERP 2018"
        '
        'chbPerformance_ERP2018_ShowArea
        '
        Me.chbPerformance_ERP2018_ShowArea.Location = New System.Drawing.Point(6, 19)
        Me.chbPerformance_ERP2018_ShowArea.Name = "chbPerformance_ERP2018_ShowArea"
        Me.chbPerformance_ERP2018_ShowArea.Size = New System.Drawing.Size(204, 18)
        Me.chbPerformance_ERP2018_ShowArea.TabIndex = 2
        Me.chbPerformance_ERP2018_ShowArea.Text = "Show ERP 2018 Area"
        Me.chbPerformance_ERP2018_ShowArea.UseVisualStyleBackColor = True
        '
        'grbPerformance_SEL
        '
        Me.grbPerformance_SEL.Controls.Add(Me.nudPerformance_SEL_Limit)
        Me.grbPerformance_SEL.Controls.Add(Me.txbPerformance_SEL)
        Me.grbPerformance_SEL.Controls.Add(Me.lblPerformance_SEL_SEL)
        Me.grbPerformance_SEL.Controls.Add(Me.Label7)
        Me.grbPerformance_SEL.Controls.Add(Me.chbPerformance_SEL_ShowArea)
        Me.grbPerformance_SEL.Location = New System.Drawing.Point(229, 33)
        Me.grbPerformance_SEL.Name = "grbPerformance_SEL"
        Me.grbPerformance_SEL.Size = New System.Drawing.Size(217, 61)
        Me.grbPerformance_SEL.TabIndex = 4
        Me.grbPerformance_SEL.TabStop = False
        '
        'nudPerformance_SEL_Limit
        '
        Me.nudPerformance_SEL_Limit.Enabled = False
        Me.nudPerformance_SEL_Limit.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        Me.nudPerformance_SEL_Limit.Location = New System.Drawing.Point(145, 37)
        Me.nudPerformance_SEL_Limit.Maximum = New Decimal(New Integer() {3000, 0, 0, 0})
        Me.nudPerformance_SEL_Limit.Minimum = New Decimal(New Integer() {500, 0, 0, 0})
        Me.nudPerformance_SEL_Limit.Name = "nudPerformance_SEL_Limit"
        Me.nudPerformance_SEL_Limit.Size = New System.Drawing.Size(66, 20)
        Me.nudPerformance_SEL_Limit.TabIndex = 4
        Me.nudPerformance_SEL_Limit.Value = New Decimal(New Integer() {1500, 0, 0, 0})
        '
        'txbPerformance_SEL
        '
        Me.txbPerformance_SEL.Location = New System.Drawing.Point(145, 11)
        Me.txbPerformance_SEL.Name = "txbPerformance_SEL"
        Me.txbPerformance_SEL.ReadOnly = True
        Me.txbPerformance_SEL.Size = New System.Drawing.Size(66, 20)
        Me.txbPerformance_SEL.TabIndex = 1
        '
        'lblPerformance_SEL_SEL
        '
        Me.lblPerformance_SEL_SEL.Location = New System.Drawing.Point(6, 8)
        Me.lblPerformance_SEL_SEL.Name = "lblPerformance_SEL_SEL"
        Me.lblPerformance_SEL_SEL.Size = New System.Drawing.Size(107, 26)
        Me.lblPerformance_SEL_SEL.TabIndex = 0
        Me.lblPerformance_SEL_SEL.Text = "SEL [J/m3]"
        Me.lblPerformance_SEL_SEL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(123, 36)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(16, 17)
        Me.Label7.TabIndex = 3
        Me.Label7.Text = "≤"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'chbPerformance_SEL_ShowArea
        '
        Me.chbPerformance_SEL_ShowArea.Location = New System.Drawing.Point(8, 37)
        Me.chbPerformance_SEL_ShowArea.Name = "chbPerformance_SEL_ShowArea"
        Me.chbPerformance_SEL_ShowArea.Size = New System.Drawing.Size(119, 17)
        Me.chbPerformance_SEL_ShowArea.TabIndex = 2
        Me.chbPerformance_SEL_ShowArea.Text = "Show Area"
        Me.chbPerformance_SEL_ShowArea.UseVisualStyleBackColor = True
        '
        'txbPerformance_ElectricalPerformances_PowerInput
        '
        Me.txbPerformance_ElectricalPerformances_PowerInput.Location = New System.Drawing.Point(374, 7)
        Me.txbPerformance_ElectricalPerformances_PowerInput.Name = "txbPerformance_ElectricalPerformances_PowerInput"
        Me.txbPerformance_ElectricalPerformances_PowerInput.ReadOnly = True
        Me.txbPerformance_ElectricalPerformances_PowerInput.Size = New System.Drawing.Size(66, 20)
        Me.txbPerformance_ElectricalPerformances_PowerInput.TabIndex = 1
        '
        'lblPerformance_ElectricalPerformances_PowerInput
        '
        Me.lblPerformance_ElectricalPerformances_PowerInput.Location = New System.Drawing.Point(9, 3)
        Me.lblPerformance_ElectricalPerformances_PowerInput.Name = "lblPerformance_ElectricalPerformances_PowerInput"
        Me.lblPerformance_ElectricalPerformances_PowerInput.Size = New System.Drawing.Size(359, 27)
        Me.lblPerformance_ElectricalPerformances_PowerInput.TabIndex = 0
        Me.lblPerformance_ElectricalPerformances_PowerInput.Text = "Power Input [W] (Single branch)"
        Me.lblPerformance_ElectricalPerformances_PowerInput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'grbPerformance_PassiveHaus
        '
        Me.grbPerformance_PassiveHaus.Controls.Add(Me.txbPerformance_PassiveHaus_Limit)
        Me.grbPerformance_PassiveHaus.Controls.Add(Me.txbPerformance_PassiveHaus)
        Me.grbPerformance_PassiveHaus.Controls.Add(Me.Label2)
        Me.grbPerformance_PassiveHaus.Controls.Add(Me.lblPerformance_PassiveHaus_ElectricalEfficiency)
        Me.grbPerformance_PassiveHaus.Controls.Add(Me.chbPerformance_PassiveHaus_ShowArea)
        Me.grbPerformance_PassiveHaus.Location = New System.Drawing.Point(6, 100)
        Me.grbPerformance_PassiveHaus.Name = "grbPerformance_PassiveHaus"
        Me.grbPerformance_PassiveHaus.Size = New System.Drawing.Size(440, 71)
        Me.grbPerformance_PassiveHaus.TabIndex = 2
        Me.grbPerformance_PassiveHaus.TabStop = False
        Me.grbPerformance_PassiveHaus.Text = "Passive Haus"
        '
        'txbPerformance_PassiveHaus_Limit
        '
        Me.txbPerformance_PassiveHaus_Limit.Location = New System.Drawing.Point(368, 43)
        Me.txbPerformance_PassiveHaus_Limit.Name = "txbPerformance_PassiveHaus_Limit"
        Me.txbPerformance_PassiveHaus_Limit.ReadOnly = True
        Me.txbPerformance_PassiveHaus_Limit.Size = New System.Drawing.Size(66, 20)
        Me.txbPerformance_PassiveHaus_Limit.TabIndex = 4
        Me.txbPerformance_PassiveHaus_Limit.Text = "0,45"
        '
        'txbPerformance_PassiveHaus
        '
        Me.txbPerformance_PassiveHaus.Location = New System.Drawing.Point(368, 18)
        Me.txbPerformance_PassiveHaus.Name = "txbPerformance_PassiveHaus"
        Me.txbPerformance_PassiveHaus.ReadOnly = True
        Me.txbPerformance_PassiveHaus.Size = New System.Drawing.Size(66, 20)
        Me.txbPerformance_PassiveHaus.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(346, 44)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(16, 17)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "≤"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblPerformance_PassiveHaus_ElectricalEfficiency
        '
        Me.lblPerformance_PassiveHaus_ElectricalEfficiency.Location = New System.Drawing.Point(3, 14)
        Me.lblPerformance_PassiveHaus_ElectricalEfficiency.Name = "lblPerformance_PassiveHaus_ElectricalEfficiency"
        Me.lblPerformance_PassiveHaus_ElectricalEfficiency.Size = New System.Drawing.Size(254, 27)
        Me.lblPerformance_PassiveHaus_ElectricalEfficiency.TabIndex = 0
        Me.lblPerformance_PassiveHaus_ElectricalEfficiency.Text = "Passive Haus Electr. Eff. [W/(m3/h)]"
        Me.lblPerformance_PassiveHaus_ElectricalEfficiency.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'chbPerformance_PassiveHaus_ShowArea
        '
        Me.chbPerformance_PassiveHaus_ShowArea.Location = New System.Drawing.Point(6, 45)
        Me.chbPerformance_PassiveHaus_ShowArea.Name = "chbPerformance_PassiveHaus_ShowArea"
        Me.chbPerformance_PassiveHaus_ShowArea.Size = New System.Drawing.Size(232, 17)
        Me.chbPerformance_PassiveHaus_ShowArea.TabIndex = 2
        Me.chbPerformance_PassiveHaus_ShowArea.Text = "Show Passive Haus Area"
        Me.chbPerformance_PassiveHaus_ShowArea.UseVisualStyleBackColor = True
        '
        'grbPerformance_SFP
        '
        Me.grbPerformance_SFP.Controls.Add(Me.nudPerformance_SFP_Limit)
        Me.grbPerformance_SFP.Controls.Add(Me.txbPerformance_SFP)
        Me.grbPerformance_SFP.Controls.Add(Me.lblPerformance_SFP_SFP)
        Me.grbPerformance_SFP.Controls.Add(Me.Label1)
        Me.grbPerformance_SFP.Controls.Add(Me.chbPerformance_SFP_ShowArea)
        Me.grbPerformance_SFP.Location = New System.Drawing.Point(6, 33)
        Me.grbPerformance_SFP.Name = "grbPerformance_SFP"
        Me.grbPerformance_SFP.Size = New System.Drawing.Size(217, 61)
        Me.grbPerformance_SFP.TabIndex = 3
        Me.grbPerformance_SFP.TabStop = False
        '
        'nudPerformance_SFP_Limit
        '
        Me.nudPerformance_SFP_Limit.DecimalPlaces = 1
        Me.nudPerformance_SFP_Limit.Enabled = False
        Me.nudPerformance_SFP_Limit.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        Me.nudPerformance_SFP_Limit.Location = New System.Drawing.Point(142, 36)
        Me.nudPerformance_SFP_Limit.Maximum = New Decimal(New Integer() {3, 0, 0, 0})
        Me.nudPerformance_SFP_Limit.Minimum = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.nudPerformance_SFP_Limit.Name = "nudPerformance_SFP_Limit"
        Me.nudPerformance_SFP_Limit.Size = New System.Drawing.Size(66, 20)
        Me.nudPerformance_SFP_Limit.TabIndex = 4
        Me.nudPerformance_SFP_Limit.Value = New Decimal(New Integer() {15, 0, 0, 65536})
        '
        'txbPerformance_SFP
        '
        Me.txbPerformance_SFP.Location = New System.Drawing.Point(142, 11)
        Me.txbPerformance_SFP.Name = "txbPerformance_SFP"
        Me.txbPerformance_SFP.ReadOnly = True
        Me.txbPerformance_SFP.Size = New System.Drawing.Size(66, 20)
        Me.txbPerformance_SFP.TabIndex = 1
        '
        'lblPerformance_SFP_SFP
        '
        Me.lblPerformance_SFP_SFP.Location = New System.Drawing.Point(3, 7)
        Me.lblPerformance_SFP_SFP.Name = "lblPerformance_SFP_SFP"
        Me.lblPerformance_SFP_SFP.Size = New System.Drawing.Size(111, 26)
        Me.lblPerformance_SFP_SFP.TabIndex = 0
        Me.lblPerformance_SFP_SFP.Text = "SFP [kW/(m3/s)]"
        Me.lblPerformance_SFP_SFP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(120, 36)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(16, 17)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "≤"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'chbPerformance_SFP_ShowArea
        '
        Me.chbPerformance_SFP_ShowArea.Location = New System.Drawing.Point(6, 37)
        Me.chbPerformance_SFP_ShowArea.Name = "chbPerformance_SFP_ShowArea"
        Me.chbPerformance_SFP_ShowArea.Size = New System.Drawing.Size(108, 18)
        Me.chbPerformance_SFP_ShowArea.TabIndex = 2
        Me.chbPerformance_SFP_ShowArea.Text = "Show Area"
        Me.chbPerformance_SFP_ShowArea.UseVisualStyleBackColor = True
        '
        'tbpData_SoundPower
        '
        Me.tbpData_SoundPower.Controls.Add(Me.dgvPerformance_SoundPower)
        Me.tbpData_SoundPower.Controls.Add(Me.Panel1)
        Me.tbpData_SoundPower.Location = New System.Drawing.Point(4, 22)
        Me.tbpData_SoundPower.Name = "tbpData_SoundPower"
        Me.tbpData_SoundPower.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpData_SoundPower.Size = New System.Drawing.Size(704, 215)
        Me.tbpData_SoundPower.TabIndex = 2
        Me.tbpData_SoundPower.Text = "Sound Power"
        Me.tbpData_SoundPower.UseVisualStyleBackColor = True
        '
        'dgvPerformance_SoundPower
        '
        Me.dgvPerformance_SoundPower.AllowUserToAddRows = False
        Me.dgvPerformance_SoundPower.AllowUserToDeleteRows = False
        Me.dgvPerformance_SoundPower.AllowUserToResizeColumns = False
        Me.dgvPerformance_SoundPower.AllowUserToResizeRows = False
        Me.dgvPerformance_SoundPower.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvPerformance_SoundPower.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvPerformance_SoundPower.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.dgvPerformance_SoundPower.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvPerformance_SoundPower.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.dgvPerformance_SoundPower.ColumnHeadersHeight = 40
        Me.dgvPerformance_SoundPower.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvPerformance_SoundPower.DefaultCellStyle = DataGridViewCellStyle2
        Me.dgvPerformance_SoundPower.Dock = System.Windows.Forms.DockStyle.Top
        Me.dgvPerformance_SoundPower.GridColor = System.Drawing.Color.Silver
        Me.dgvPerformance_SoundPower.Location = New System.Drawing.Point(3, 37)
        Me.dgvPerformance_SoundPower.MultiSelect = False
        Me.dgvPerformance_SoundPower.Name = "dgvPerformance_SoundPower"
        Me.dgvPerformance_SoundPower.ReadOnly = True
        Me.dgvPerformance_SoundPower.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvPerformance_SoundPower.RowHeadersDefaultCellStyle = DataGridViewCellStyle3
        Me.dgvPerformance_SoundPower.RowHeadersVisible = False
        Me.dgvPerformance_SoundPower.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvPerformance_SoundPower.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.dgvPerformance_SoundPower.Size = New System.Drawing.Size(698, 172)
        Me.dgvPerformance_SoundPower.TabIndex = 2
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.PictureBox4)
        Me.Panel1.Controls.Add(Me.PictureBox3)
        Me.Panel1.Controls.Add(Me.PictureBox2)
        Me.Panel1.Controls.Add(Me.rdb_Q8)
        Me.Panel1.Controls.Add(Me.rdb_Q4)
        Me.Panel1.Controls.Add(Me.rdb_Q2)
        Me.Panel1.Controls.Add(Me.chbSoundPerformances_16032)
        Me.Panel1.Controls.Add(Me.chbSoundPerformances_addtoreport)
        Me.Panel1.Controls.Add(Me.Label5)
        Me.Panel1.Controls.Add(Me.nudSoundPerformances_LP2)
        Me.Panel1.Controls.Add(Me.nudSoundPerformances_LP1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(3, 3)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(698, 34)
        Me.Panel1.TabIndex = 3
        '
        'PictureBox4
        '
        Me.PictureBox4.Image = CType(resources.GetObject("PictureBox4.Image"), System.Drawing.Image)
        Me.PictureBox4.Location = New System.Drawing.Point(400, 2)
        Me.PictureBox4.Name = "PictureBox4"
        Me.PictureBox4.Size = New System.Drawing.Size(45, 29)
        Me.PictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox4.TabIndex = 16
        Me.PictureBox4.TabStop = False
        '
        'PictureBox3
        '
        Me.PictureBox3.Image = CType(resources.GetObject("PictureBox3.Image"), System.Drawing.Image)
        Me.PictureBox3.Location = New System.Drawing.Point(285, 2)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(45, 29)
        Me.PictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox3.TabIndex = 4
        Me.PictureBox3.TabStop = False
        '
        'PictureBox2
        '
        Me.PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), System.Drawing.Image)
        Me.PictureBox2.Location = New System.Drawing.Point(173, 3)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(45, 29)
        Me.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox2.TabIndex = 4
        Me.PictureBox2.TabStop = False
        '
        'rdb_Q8
        '
        Me.rdb_Q8.AutoSize = True
        Me.rdb_Q8.Location = New System.Drawing.Point(451, 7)
        Me.rdb_Q8.Name = "rdb_Q8"
        Me.rdb_Q8.Size = New System.Drawing.Size(45, 17)
        Me.rdb_Q8.TabIndex = 15
        Me.rdb_Q8.TabStop = True
        Me.rdb_Q8.Text = "Q=8"
        Me.rdb_Q8.UseVisualStyleBackColor = True
        '
        'rdb_Q4
        '
        Me.rdb_Q4.AutoSize = True
        Me.rdb_Q4.Location = New System.Drawing.Point(336, 7)
        Me.rdb_Q4.Name = "rdb_Q4"
        Me.rdb_Q4.Size = New System.Drawing.Size(45, 17)
        Me.rdb_Q4.TabIndex = 15
        Me.rdb_Q4.Text = "Q=4"
        Me.rdb_Q4.UseVisualStyleBackColor = True
        '
        'rdb_Q2
        '
        Me.rdb_Q2.AutoSize = True
        Me.rdb_Q2.Checked = True
        Me.rdb_Q2.Location = New System.Drawing.Point(224, 7)
        Me.rdb_Q2.Name = "rdb_Q2"
        Me.rdb_Q2.Size = New System.Drawing.Size(45, 17)
        Me.rdb_Q2.TabIndex = 15
        Me.rdb_Q2.TabStop = True
        Me.rdb_Q2.Text = "Q=2"
        Me.rdb_Q2.UseVisualStyleBackColor = True
        '
        'chbSoundPerformances_16032
        '
        Me.chbSoundPerformances_16032.AutoSize = True
        Me.chbSoundPerformances_16032.Location = New System.Drawing.Point(507, 8)
        Me.chbSoundPerformances_16032.Name = "chbSoundPerformances_16032"
        Me.chbSoundPerformances_16032.Size = New System.Drawing.Size(95, 17)
        Me.chbSoundPerformances_16032.TabIndex = 14
        Me.chbSoundPerformances_16032.Text = "EN-ISO 16032"
        Me.chbSoundPerformances_16032.UseVisualStyleBackColor = True
        '
        'chbSoundPerformances_addtoreport
        '
        Me.chbSoundPerformances_addtoreport.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.chbSoundPerformances_addtoreport.AutoSize = True
        Me.chbSoundPerformances_addtoreport.Location = New System.Drawing.Point(608, 8)
        Me.chbSoundPerformances_addtoreport.Name = "chbSoundPerformances_addtoreport"
        Me.chbSoundPerformances_addtoreport.Size = New System.Drawing.Size(87, 17)
        Me.chbSoundPerformances_addtoreport.TabIndex = 14
        Me.chbSoundPerformances_addtoreport.Text = "Add to report"
        Me.chbSoundPerformances_addtoreport.UseVisualStyleBackColor = True
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(11, 9)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(36, 13)
        Me.Label5.TabIndex = 13
        Me.Label5.Text = "Lp [m]"
        '
        'nudSoundPerformances_LP2
        '
        Me.nudSoundPerformances_LP2.DecimalPlaces = 1
        Me.nudSoundPerformances_LP2.Increment = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.nudSoundPerformances_LP2.Location = New System.Drawing.Point(106, 7)
        Me.nudSoundPerformances_LP2.Maximum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudSoundPerformances_LP2.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudSoundPerformances_LP2.Name = "nudSoundPerformances_LP2"
        Me.nudSoundPerformances_LP2.Size = New System.Drawing.Size(48, 20)
        Me.nudSoundPerformances_LP2.TabIndex = 12
        Me.nudSoundPerformances_LP2.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'nudSoundPerformances_LP1
        '
        Me.nudSoundPerformances_LP1.DecimalPlaces = 1
        Me.nudSoundPerformances_LP1.Increment = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.nudSoundPerformances_LP1.Location = New System.Drawing.Point(52, 7)
        Me.nudSoundPerformances_LP1.Maximum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.nudSoundPerformances_LP1.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudSoundPerformances_LP1.Name = "nudSoundPerformances_LP1"
        Me.nudSoundPerformances_LP1.Size = New System.Drawing.Size(48, 20)
        Me.nudSoundPerformances_LP1.TabIndex = 12
        Me.nudSoundPerformances_LP1.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'tbpData_ItemGenerator
        '
        Me.tbpData_ItemGenerator.Controls.Add(Me.lblPerformance_ItemDescr)
        Me.tbpData_ItemGenerator.Controls.Add(Me.lblPerformance_ItemCode)
        Me.tbpData_ItemGenerator.Controls.Add(Me.txbPerformance_ItemCode)
        Me.tbpData_ItemGenerator.Controls.Add(Me.grbPerformance_IGClimaAcc)
        Me.tbpData_ItemGenerator.Controls.Add(Me.grbPerformance_IGComm)
        Me.tbpData_ItemGenerator.Controls.Add(Me.grbPerformance_IGSensor)
        Me.tbpData_ItemGenerator.Location = New System.Drawing.Point(4, 22)
        Me.tbpData_ItemGenerator.Name = "tbpData_ItemGenerator"
        Me.tbpData_ItemGenerator.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpData_ItemGenerator.Size = New System.Drawing.Size(704, 215)
        Me.tbpData_ItemGenerator.TabIndex = 3
        Me.tbpData_ItemGenerator.Text = "Item Coding"
        Me.tbpData_ItemGenerator.UseVisualStyleBackColor = True
        '
        'lblPerformance_ItemDescr
        '
        Me.lblPerformance_ItemDescr.AutoSize = True
        Me.lblPerformance_ItemDescr.Location = New System.Drawing.Point(342, 145)
        Me.lblPerformance_ItemDescr.Name = "lblPerformance_ItemDescr"
        Me.lblPerformance_ItemDescr.Size = New System.Drawing.Size(60, 13)
        Me.lblPerformance_ItemDescr.TabIndex = 4
        Me.lblPerformance_ItemDescr.Text = "Description"
        '
        'lblPerformance_ItemCode
        '
        Me.lblPerformance_ItemCode.AutoSize = True
        Me.lblPerformance_ItemCode.Location = New System.Drawing.Point(3, 145)
        Me.lblPerformance_ItemCode.Name = "lblPerformance_ItemCode"
        Me.lblPerformance_ItemCode.Size = New System.Drawing.Size(26, 13)
        Me.lblPerformance_ItemCode.TabIndex = 4
        Me.lblPerformance_ItemCode.Text = "NR:"
        '
        'txbPerformance_ItemCode
        '
        Me.txbPerformance_ItemCode.Location = New System.Drawing.Point(32, 142)
        Me.txbPerformance_ItemCode.Name = "txbPerformance_ItemCode"
        Me.txbPerformance_ItemCode.ReadOnly = True
        Me.txbPerformance_ItemCode.Size = New System.Drawing.Size(304, 20)
        Me.txbPerformance_ItemCode.TabIndex = 3
        '
        'grbPerformance_IGClimaAcc
        '
        Me.grbPerformance_IGClimaAcc.Controls.Add(Me.chbPerformance_IPEHD)
        Me.grbPerformance_IGClimaAcc.Controls.Add(Me.rdbPerformance_ICWD)
        Me.grbPerformance_IGClimaAcc.Controls.Add(Me.rdbPerformance_none)
        Me.grbPerformance_IGClimaAcc.Controls.Add(Me.rdbPerformance_IDXD)
        Me.grbPerformance_IGClimaAcc.Controls.Add(Me.rdbPerformance_IEHD)
        Me.grbPerformance_IGClimaAcc.Controls.Add(Me.rdbPerformance_IHCD)
        Me.grbPerformance_IGClimaAcc.Controls.Add(Me.rdbPerformance_IHWD)
        Me.grbPerformance_IGClimaAcc.Location = New System.Drawing.Point(6, 7)
        Me.grbPerformance_IGClimaAcc.Name = "grbPerformance_IGClimaAcc"
        Me.grbPerformance_IGClimaAcc.Size = New System.Drawing.Size(330, 118)
        Me.grbPerformance_IGClimaAcc.TabIndex = 2
        Me.grbPerformance_IGClimaAcc.TabStop = False
        Me.grbPerformance_IGClimaAcc.Text = "Heating and Cooling"
        '
        'chbPerformance_IPEHD
        '
        Me.chbPerformance_IPEHD.AutoSize = True
        Me.chbPerformance_IPEHD.Location = New System.Drawing.Point(7, 95)
        Me.chbPerformance_IPEHD.Name = "chbPerformance_IPEHD"
        Me.chbPerformance_IPEHD.Size = New System.Drawing.Size(59, 17)
        Me.chbPerformance_IPEHD.TabIndex = 1
        Me.chbPerformance_IPEHD.Text = "IPEHD"
        Me.chbPerformance_IPEHD.UseVisualStyleBackColor = True
        '
        'rdbPerformance_ICWD
        '
        Me.rdbPerformance_ICWD.AutoSize = True
        Me.rdbPerformance_ICWD.Location = New System.Drawing.Point(228, 23)
        Me.rdbPerformance_ICWD.Name = "rdbPerformance_ICWD"
        Me.rdbPerformance_ICWD.Size = New System.Drawing.Size(54, 17)
        Me.rdbPerformance_ICWD.TabIndex = 0
        Me.rdbPerformance_ICWD.Text = "ICWD"
        Me.rdbPerformance_ICWD.UseVisualStyleBackColor = True
        '
        'rdbPerformance_none
        '
        Me.rdbPerformance_none.AutoSize = True
        Me.rdbPerformance_none.Checked = True
        Me.rdbPerformance_none.Location = New System.Drawing.Point(228, 59)
        Me.rdbPerformance_none.Name = "rdbPerformance_none"
        Me.rdbPerformance_none.Size = New System.Drawing.Size(51, 17)
        Me.rdbPerformance_none.TabIndex = 0
        Me.rdbPerformance_none.TabStop = True
        Me.rdbPerformance_none.Text = "None"
        Me.rdbPerformance_none.UseVisualStyleBackColor = True
        '
        'rdbPerformance_IDXD
        '
        Me.rdbPerformance_IDXD.AutoSize = True
        Me.rdbPerformance_IDXD.Location = New System.Drawing.Point(116, 59)
        Me.rdbPerformance_IDXD.Name = "rdbPerformance_IDXD"
        Me.rdbPerformance_IDXD.Size = New System.Drawing.Size(51, 17)
        Me.rdbPerformance_IDXD.TabIndex = 0
        Me.rdbPerformance_IDXD.Text = "IDXD"
        Me.rdbPerformance_IDXD.UseVisualStyleBackColor = True
        '
        'rdbPerformance_IEHD
        '
        Me.rdbPerformance_IEHD.AutoSize = True
        Me.rdbPerformance_IEHD.Location = New System.Drawing.Point(116, 23)
        Me.rdbPerformance_IEHD.Name = "rdbPerformance_IEHD"
        Me.rdbPerformance_IEHD.Size = New System.Drawing.Size(51, 17)
        Me.rdbPerformance_IEHD.TabIndex = 0
        Me.rdbPerformance_IEHD.Text = "IEHD"
        Me.rdbPerformance_IEHD.UseVisualStyleBackColor = True
        '
        'rdbPerformance_IHCD
        '
        Me.rdbPerformance_IHCD.AutoSize = True
        Me.rdbPerformance_IHCD.Location = New System.Drawing.Point(7, 59)
        Me.rdbPerformance_IHCD.Name = "rdbPerformance_IHCD"
        Me.rdbPerformance_IHCD.Size = New System.Drawing.Size(51, 17)
        Me.rdbPerformance_IHCD.TabIndex = 0
        Me.rdbPerformance_IHCD.Text = "IHCD"
        Me.rdbPerformance_IHCD.UseVisualStyleBackColor = True
        '
        'rdbPerformance_IHWD
        '
        Me.rdbPerformance_IHWD.AutoSize = True
        Me.rdbPerformance_IHWD.Location = New System.Drawing.Point(7, 23)
        Me.rdbPerformance_IHWD.Name = "rdbPerformance_IHWD"
        Me.rdbPerformance_IHWD.Size = New System.Drawing.Size(55, 17)
        Me.rdbPerformance_IHWD.TabIndex = 0
        Me.rdbPerformance_IHWD.Text = "IHWD"
        Me.rdbPerformance_IHWD.UseVisualStyleBackColor = True
        '
        'grbPerformance_IGComm
        '
        Me.grbPerformance_IGComm.Controls.Add(Me.chbPerformance_RFM)
        Me.grbPerformance_IGComm.Controls.Add(Me.chbPerformance_MBUS)
        Me.grbPerformance_IGComm.Location = New System.Drawing.Point(342, 83)
        Me.grbPerformance_IGComm.Name = "grbPerformance_IGComm"
        Me.grbPerformance_IGComm.Size = New System.Drawing.Size(259, 42)
        Me.grbPerformance_IGComm.TabIndex = 1
        Me.grbPerformance_IGComm.TabStop = False
        Me.grbPerformance_IGComm.Text = "Communication"
        '
        'chbPerformance_RFM
        '
        Me.chbPerformance_RFM.AutoSize = True
        Me.chbPerformance_RFM.Location = New System.Drawing.Point(130, 19)
        Me.chbPerformance_RFM.Name = "chbPerformance_RFM"
        Me.chbPerformance_RFM.Size = New System.Drawing.Size(49, 17)
        Me.chbPerformance_RFM.TabIndex = 1
        Me.chbPerformance_RFM.Text = "RFM"
        Me.chbPerformance_RFM.UseVisualStyleBackColor = True
        '
        'chbPerformance_MBUS
        '
        Me.chbPerformance_MBUS.AutoSize = True
        Me.chbPerformance_MBUS.Location = New System.Drawing.Point(43, 19)
        Me.chbPerformance_MBUS.Name = "chbPerformance_MBUS"
        Me.chbPerformance_MBUS.Size = New System.Drawing.Size(73, 17)
        Me.chbPerformance_MBUS.TabIndex = 1
        Me.chbPerformance_MBUS.Text = "MODBUS"
        Me.chbPerformance_MBUS.UseVisualStyleBackColor = True
        '
        'grbPerformance_IGSensor
        '
        Me.grbPerformance_IGSensor.Controls.Add(Me.rdbPerformance_RH)
        Me.grbPerformance_IGSensor.Controls.Add(Me.rdbPerformance_VOC)
        Me.grbPerformance_IGSensor.Controls.Add(Me.rdbPerformance_CO2)
        Me.grbPerformance_IGSensor.Location = New System.Drawing.Point(342, 7)
        Me.grbPerformance_IGSensor.Name = "grbPerformance_IGSensor"
        Me.grbPerformance_IGSensor.Size = New System.Drawing.Size(259, 70)
        Me.grbPerformance_IGSensor.TabIndex = 0
        Me.grbPerformance_IGSensor.TabStop = False
        Me.grbPerformance_IGSensor.Text = "Integrated Sensor"
        '
        'rdbPerformance_RH
        '
        Me.rdbPerformance_RH.AutoSize = True
        Me.rdbPerformance_RH.Location = New System.Drawing.Point(187, 24)
        Me.rdbPerformance_RH.Name = "rdbPerformance_RH"
        Me.rdbPerformance_RH.Size = New System.Drawing.Size(47, 17)
        Me.rdbPerformance_RH.TabIndex = 1
        Me.rdbPerformance_RH.Text = "R.H."
        Me.rdbPerformance_RH.UseVisualStyleBackColor = True
        '
        'rdbPerformance_VOC
        '
        Me.rdbPerformance_VOC.AutoSize = True
        Me.rdbPerformance_VOC.Location = New System.Drawing.Point(109, 24)
        Me.rdbPerformance_VOC.Name = "rdbPerformance_VOC"
        Me.rdbPerformance_VOC.Size = New System.Drawing.Size(47, 17)
        Me.rdbPerformance_VOC.TabIndex = 1
        Me.rdbPerformance_VOC.Text = "VOC"
        Me.rdbPerformance_VOC.UseVisualStyleBackColor = True
        '
        'rdbPerformance_CO2
        '
        Me.rdbPerformance_CO2.AutoSize = True
        Me.rdbPerformance_CO2.Checked = True
        Me.rdbPerformance_CO2.Location = New System.Drawing.Point(28, 24)
        Me.rdbPerformance_CO2.Name = "rdbPerformance_CO2"
        Me.rdbPerformance_CO2.Size = New System.Drawing.Size(46, 17)
        Me.rdbPerformance_CO2.TabIndex = 1
        Me.rdbPerformance_CO2.TabStop = True
        Me.rdbPerformance_CO2.Text = "CO2"
        Me.rdbPerformance_CO2.UseVisualStyleBackColor = True
        '
        'tbpData_ItemGenerator_QTM
        '
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.grbPerformance_IGHeaterQTM)
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.grbPerformance_IGVersionQTM)
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.grbPerformance_IGLayoutQTM)
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.lblPerformance_ItemDescrQTM)
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.lblPerformance_ItemCodeQTM)
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.txbPerformance_ItemCodeQTM)
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.grbPerformance_IGSensorQTM)
        Me.tbpData_ItemGenerator_QTM.Controls.Add(Me.grbPerformance_IGCommQTM)
        Me.tbpData_ItemGenerator_QTM.Location = New System.Drawing.Point(4, 22)
        Me.tbpData_ItemGenerator_QTM.Name = "tbpData_ItemGenerator_QTM"
        Me.tbpData_ItemGenerator_QTM.Size = New System.Drawing.Size(704, 215)
        Me.tbpData_ItemGenerator_QTM.TabIndex = 4
        Me.tbpData_ItemGenerator_QTM.Text = "Item Coding"
        Me.tbpData_ItemGenerator_QTM.UseVisualStyleBackColor = True
        '
        'grbPerformance_IGHeaterQTM
        '
        Me.grbPerformance_IGHeaterQTM.Controls.Add(Me.chb_qtm_IPEHD)
        Me.grbPerformance_IGHeaterQTM.Location = New System.Drawing.Point(223, 4)
        Me.grbPerformance_IGHeaterQTM.Name = "grbPerformance_IGHeaterQTM"
        Me.grbPerformance_IGHeaterQTM.Size = New System.Drawing.Size(116, 53)
        Me.grbPerformance_IGHeaterQTM.TabIndex = 10
        Me.grbPerformance_IGHeaterQTM.TabStop = False
        Me.grbPerformance_IGHeaterQTM.Text = "Integrated Heater"
        '
        'chb_qtm_IPEHD
        '
        Me.chb_qtm_IPEHD.AutoSize = True
        Me.chb_qtm_IPEHD.Location = New System.Drawing.Point(12, 25)
        Me.chb_qtm_IPEHD.Name = "chb_qtm_IPEHD"
        Me.chb_qtm_IPEHD.Size = New System.Drawing.Size(59, 17)
        Me.chb_qtm_IPEHD.TabIndex = 1
        Me.chb_qtm_IPEHD.Text = "IPEHD"
        Me.chb_qtm_IPEHD.UseVisualStyleBackColor = True
        '
        'grbPerformance_IGVersionQTM
        '
        Me.grbPerformance_IGVersionQTM.Controls.Add(Me.rdb_qtm_easy)
        Me.grbPerformance_IGVersionQTM.Controls.Add(Me.rdb_qtm_preplus)
        Me.grbPerformance_IGVersionQTM.Controls.Add(Me.rdb_qtm_premium)
        Me.grbPerformance_IGVersionQTM.Location = New System.Drawing.Point(6, 66)
        Me.grbPerformance_IGVersionQTM.Name = "grbPerformance_IGVersionQTM"
        Me.grbPerformance_IGVersionQTM.Size = New System.Drawing.Size(211, 53)
        Me.grbPerformance_IGVersionQTM.TabIndex = 9
        Me.grbPerformance_IGVersionQTM.TabStop = False
        Me.grbPerformance_IGVersionQTM.Text = "Version"
        '
        'rdb_qtm_easy
        '
        Me.rdb_qtm_easy.AutoSize = True
        Me.rdb_qtm_easy.Checked = True
        Me.rdb_qtm_easy.Location = New System.Drawing.Point(6, 23)
        Me.rdb_qtm_easy.Name = "rdb_qtm_easy"
        Me.rdb_qtm_easy.Size = New System.Drawing.Size(48, 17)
        Me.rdb_qtm_easy.TabIndex = 1
        Me.rdb_qtm_easy.TabStop = True
        Me.rdb_qtm_easy.Text = "Easy"
        Me.rdb_qtm_easy.UseVisualStyleBackColor = True
        '
        'rdb_qtm_preplus
        '
        Me.rdb_qtm_preplus.AutoSize = True
        Me.rdb_qtm_preplus.Location = New System.Drawing.Point(131, 23)
        Me.rdb_qtm_preplus.Name = "rdb_qtm_preplus"
        Me.rdb_qtm_preplus.Size = New System.Drawing.Size(71, 17)
        Me.rdb_qtm_preplus.TabIndex = 0
        Me.rdb_qtm_preplus.Text = "Premium+"
        Me.rdb_qtm_preplus.UseVisualStyleBackColor = True
        '
        'rdb_qtm_premium
        '
        Me.rdb_qtm_premium.AutoSize = True
        Me.rdb_qtm_premium.Location = New System.Drawing.Point(60, 23)
        Me.rdb_qtm_premium.Name = "rdb_qtm_premium"
        Me.rdb_qtm_premium.Size = New System.Drawing.Size(65, 17)
        Me.rdb_qtm_premium.TabIndex = 0
        Me.rdb_qtm_premium.Text = "Premium"
        Me.rdb_qtm_premium.UseVisualStyleBackColor = True
        '
        'grbPerformance_IGLayoutQTM
        '
        Me.grbPerformance_IGLayoutQTM.Controls.Add(Me.rdb_qtm_eos)
        Me.grbPerformance_IGLayoutQTM.Controls.Add(Me.rdb_qtm_osc)
        Me.grbPerformance_IGLayoutQTM.Controls.Add(Me.rdb_qtm_fos)
        Me.grbPerformance_IGLayoutQTM.Controls.Add(Me.rdb_qtm_ssc)
        Me.grbPerformance_IGLayoutQTM.Location = New System.Drawing.Point(6, 4)
        Me.grbPerformance_IGLayoutQTM.Name = "grbPerformance_IGLayoutQTM"
        Me.grbPerformance_IGLayoutQTM.Size = New System.Drawing.Size(211, 53)
        Me.grbPerformance_IGLayoutQTM.TabIndex = 8
        Me.grbPerformance_IGLayoutQTM.TabStop = False
        Me.grbPerformance_IGLayoutQTM.Text = "Layout"
        '
        'rdb_qtm_eos
        '
        Me.rdb_qtm_eos.AutoSize = True
        Me.rdb_qtm_eos.Location = New System.Drawing.Point(112, 23)
        Me.rdb_qtm_eos.Name = "rdb_qtm_eos"
        Me.rdb_qtm_eos.Size = New System.Drawing.Size(47, 17)
        Me.rdb_qtm_eos.TabIndex = 0
        Me.rdb_qtm_eos.Text = "EOS"
        Me.rdb_qtm_eos.UseVisualStyleBackColor = True
        '
        'rdb_qtm_osc
        '
        Me.rdb_qtm_osc.AutoSize = True
        Me.rdb_qtm_osc.Location = New System.Drawing.Point(59, 23)
        Me.rdb_qtm_osc.Name = "rdb_qtm_osc"
        Me.rdb_qtm_osc.Size = New System.Drawing.Size(47, 17)
        Me.rdb_qtm_osc.TabIndex = 0
        Me.rdb_qtm_osc.Text = "OSC"
        Me.rdb_qtm_osc.UseVisualStyleBackColor = True
        '
        'rdb_qtm_fos
        '
        Me.rdb_qtm_fos.AutoSize = True
        Me.rdb_qtm_fos.Location = New System.Drawing.Point(165, 23)
        Me.rdb_qtm_fos.Name = "rdb_qtm_fos"
        Me.rdb_qtm_fos.Size = New System.Drawing.Size(46, 17)
        Me.rdb_qtm_fos.TabIndex = 0
        Me.rdb_qtm_fos.Text = "FOS"
        Me.rdb_qtm_fos.UseVisualStyleBackColor = True
        '
        'rdb_qtm_ssc
        '
        Me.rdb_qtm_ssc.AutoSize = True
        Me.rdb_qtm_ssc.Checked = True
        Me.rdb_qtm_ssc.Location = New System.Drawing.Point(7, 23)
        Me.rdb_qtm_ssc.Name = "rdb_qtm_ssc"
        Me.rdb_qtm_ssc.Size = New System.Drawing.Size(46, 17)
        Me.rdb_qtm_ssc.TabIndex = 0
        Me.rdb_qtm_ssc.TabStop = True
        Me.rdb_qtm_ssc.Text = "SSC"
        Me.rdb_qtm_ssc.UseVisualStyleBackColor = True
        '
        'lblPerformance_ItemDescrQTM
        '
        Me.lblPerformance_ItemDescrQTM.AutoSize = True
        Me.lblPerformance_ItemDescrQTM.Location = New System.Drawing.Point(342, 148)
        Me.lblPerformance_ItemDescrQTM.Name = "lblPerformance_ItemDescrQTM"
        Me.lblPerformance_ItemDescrQTM.Size = New System.Drawing.Size(60, 13)
        Me.lblPerformance_ItemDescrQTM.TabIndex = 7
        Me.lblPerformance_ItemDescrQTM.Text = "Description"
        '
        'lblPerformance_ItemCodeQTM
        '
        Me.lblPerformance_ItemCodeQTM.AutoSize = True
        Me.lblPerformance_ItemCodeQTM.Location = New System.Drawing.Point(3, 148)
        Me.lblPerformance_ItemCodeQTM.Name = "lblPerformance_ItemCodeQTM"
        Me.lblPerformance_ItemCodeQTM.Size = New System.Drawing.Size(26, 13)
        Me.lblPerformance_ItemCodeQTM.TabIndex = 6
        Me.lblPerformance_ItemCodeQTM.Text = "NR:"
        '
        'txbPerformance_ItemCodeQTM
        '
        Me.txbPerformance_ItemCodeQTM.Location = New System.Drawing.Point(32, 145)
        Me.txbPerformance_ItemCodeQTM.Name = "txbPerformance_ItemCodeQTM"
        Me.txbPerformance_ItemCodeQTM.ReadOnly = True
        Me.txbPerformance_ItemCodeQTM.Size = New System.Drawing.Size(304, 20)
        Me.txbPerformance_ItemCodeQTM.TabIndex = 5
        '
        'grbPerformance_IGSensorQTM
        '
        Me.grbPerformance_IGSensorQTM.Controls.Add(Me.rdb_qtm_sensnone)
        Me.grbPerformance_IGSensorQTM.Controls.Add(Me.rdb_qtm_rh)
        Me.grbPerformance_IGSensorQTM.Controls.Add(Me.rdb_qtm_voc)
        Me.grbPerformance_IGSensorQTM.Controls.Add(Me.rdb_qtm_co2)
        Me.grbPerformance_IGSensorQTM.Location = New System.Drawing.Point(345, 4)
        Me.grbPerformance_IGSensorQTM.Name = "grbPerformance_IGSensorQTM"
        Me.grbPerformance_IGSensorQTM.Size = New System.Drawing.Size(259, 53)
        Me.grbPerformance_IGSensorQTM.TabIndex = 3
        Me.grbPerformance_IGSensorQTM.TabStop = False
        Me.grbPerformance_IGSensorQTM.Text = "Integrated Sensor"
        '
        'rdb_qtm_sensnone
        '
        Me.rdb_qtm_sensnone.AutoSize = True
        Me.rdb_qtm_sensnone.Checked = True
        Me.rdb_qtm_sensnone.Location = New System.Drawing.Point(203, 24)
        Me.rdb_qtm_sensnone.Name = "rdb_qtm_sensnone"
        Me.rdb_qtm_sensnone.Size = New System.Drawing.Size(51, 17)
        Me.rdb_qtm_sensnone.TabIndex = 2
        Me.rdb_qtm_sensnone.TabStop = True
        Me.rdb_qtm_sensnone.Text = "None"
        Me.rdb_qtm_sensnone.UseVisualStyleBackColor = True
        '
        'rdb_qtm_rh
        '
        Me.rdb_qtm_rh.AutoSize = True
        Me.rdb_qtm_rh.Location = New System.Drawing.Point(138, 24)
        Me.rdb_qtm_rh.Name = "rdb_qtm_rh"
        Me.rdb_qtm_rh.Size = New System.Drawing.Size(47, 17)
        Me.rdb_qtm_rh.TabIndex = 1
        Me.rdb_qtm_rh.Text = "R.H."
        Me.rdb_qtm_rh.UseVisualStyleBackColor = True
        '
        'rdb_qtm_voc
        '
        Me.rdb_qtm_voc.AutoSize = True
        Me.rdb_qtm_voc.Location = New System.Drawing.Point(73, 24)
        Me.rdb_qtm_voc.Name = "rdb_qtm_voc"
        Me.rdb_qtm_voc.Size = New System.Drawing.Size(47, 17)
        Me.rdb_qtm_voc.TabIndex = 1
        Me.rdb_qtm_voc.Text = "VOC"
        Me.rdb_qtm_voc.UseVisualStyleBackColor = True
        '
        'rdb_qtm_co2
        '
        Me.rdb_qtm_co2.AutoSize = True
        Me.rdb_qtm_co2.Location = New System.Drawing.Point(11, 24)
        Me.rdb_qtm_co2.Name = "rdb_qtm_co2"
        Me.rdb_qtm_co2.Size = New System.Drawing.Size(46, 17)
        Me.rdb_qtm_co2.TabIndex = 1
        Me.rdb_qtm_co2.Text = "CO2"
        Me.rdb_qtm_co2.UseVisualStyleBackColor = True
        '
        'grbPerformance_IGCommQTM
        '
        Me.grbPerformance_IGCommQTM.Controls.Add(Me.chb_qtm_ACTIVA)
        Me.grbPerformance_IGCommQTM.Controls.Add(Me.chb_qtm_RFM)
        Me.grbPerformance_IGCommQTM.Controls.Add(Me.chb_qtm_KTS)
        Me.grbPerformance_IGCommQTM.Controls.Add(Me.chb_qtm_MBUS)
        Me.grbPerformance_IGCommQTM.Location = New System.Drawing.Point(223, 66)
        Me.grbPerformance_IGCommQTM.Name = "grbPerformance_IGCommQTM"
        Me.grbPerformance_IGCommQTM.Size = New System.Drawing.Size(293, 53)
        Me.grbPerformance_IGCommQTM.TabIndex = 2
        Me.grbPerformance_IGCommQTM.TabStop = False
        Me.grbPerformance_IGCommQTM.Text = "Communication"
        '
        'chb_qtm_ACTIVA
        '
        Me.chb_qtm_ACTIVA.AutoSize = True
        Me.chb_qtm_ACTIVA.Location = New System.Drawing.Point(199, 26)
        Me.chb_qtm_ACTIVA.Name = "chb_qtm_ACTIVA"
        Me.chb_qtm_ACTIVA.Size = New System.Drawing.Size(89, 17)
        Me.chb_qtm_ACTIVA.TabIndex = 2
        Me.chb_qtm_ACTIVA.Text = "ACTIVA (IoT)"
        Me.chb_qtm_ACTIVA.UseVisualStyleBackColor = True
        '
        'chb_qtm_RFM
        '
        Me.chb_qtm_RFM.AutoSize = True
        Me.chb_qtm_RFM.Location = New System.Drawing.Point(144, 26)
        Me.chb_qtm_RFM.Name = "chb_qtm_RFM"
        Me.chb_qtm_RFM.Size = New System.Drawing.Size(49, 17)
        Me.chb_qtm_RFM.TabIndex = 1
        Me.chb_qtm_RFM.Text = "RFM"
        Me.chb_qtm_RFM.UseVisualStyleBackColor = True
        '
        'chb_qtm_KTS
        '
        Me.chb_qtm_KTS.AutoSize = True
        Me.chb_qtm_KTS.Location = New System.Drawing.Point(12, 26)
        Me.chb_qtm_KTS.Name = "chb_qtm_KTS"
        Me.chb_qtm_KTS.Size = New System.Drawing.Size(47, 17)
        Me.chb_qtm_KTS.TabIndex = 1
        Me.chb_qtm_KTS.Text = "KTS"
        Me.chb_qtm_KTS.UseVisualStyleBackColor = True
        '
        'chb_qtm_MBUS
        '
        Me.chb_qtm_MBUS.AutoSize = True
        Me.chb_qtm_MBUS.Location = New System.Drawing.Point(65, 26)
        Me.chb_qtm_MBUS.Name = "chb_qtm_MBUS"
        Me.chb_qtm_MBUS.Size = New System.Drawing.Size(73, 17)
        Me.chb_qtm_MBUS.TabIndex = 1
        Me.chb_qtm_MBUS.Text = "MODBUS"
        Me.chb_qtm_MBUS.UseVisualStyleBackColor = True
        '
        'pnlRegulationLevel
        '
        Me.pnlRegulationLevel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlRegulationLevel.Controls.Add(Me.lblPerformance_RegulationLevel)
        Me.pnlRegulationLevel.Controls.Add(Me.hsbPerformance_RegulationLevel)
        Me.pnlRegulationLevel.Controls.Add(Me.lblPerformance_RegulationLevelValue)
        Me.pnlRegulationLevel.Controls.Add(Me.prbPerformance_RegulationLevel)
        Me.pnlRegulationLevel.Location = New System.Drawing.Point(3, 0)
        Me.pnlRegulationLevel.Name = "pnlRegulationLevel"
        Me.pnlRegulationLevel.Size = New System.Drawing.Size(712, 65)
        Me.pnlRegulationLevel.TabIndex = 13
        '
        'lblPerformance_RegulationLevel
        '
        Me.lblPerformance_RegulationLevel.AutoSize = True
        Me.lblPerformance_RegulationLevel.Location = New System.Drawing.Point(5, 10)
        Me.lblPerformance_RegulationLevel.Name = "lblPerformance_RegulationLevel"
        Me.lblPerformance_RegulationLevel.Size = New System.Drawing.Size(87, 13)
        Me.lblPerformance_RegulationLevel.TabIndex = 4
        Me.lblPerformance_RegulationLevel.Text = "Regulation Level"
        '
        'hsbPerformance_RegulationLevel
        '
        Me.hsbPerformance_RegulationLevel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hsbPerformance_RegulationLevel.Location = New System.Drawing.Point(8, 25)
        Me.hsbPerformance_RegulationLevel.Maximum = 109
        Me.hsbPerformance_RegulationLevel.Minimum = 20
        Me.hsbPerformance_RegulationLevel.Name = "hsbPerformance_RegulationLevel"
        Me.hsbPerformance_RegulationLevel.Size = New System.Drawing.Size(698, 16)
        Me.hsbPerformance_RegulationLevel.TabIndex = 5
        Me.hsbPerformance_RegulationLevel.Value = 100
        '
        'lblPerformance_RegulationLevelValue
        '
        Me.lblPerformance_RegulationLevelValue.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblPerformance_RegulationLevelValue.AutoSize = True
        Me.lblPerformance_RegulationLevelValue.Location = New System.Drawing.Point(670, 10)
        Me.lblPerformance_RegulationLevelValue.Name = "lblPerformance_RegulationLevelValue"
        Me.lblPerformance_RegulationLevelValue.Size = New System.Drawing.Size(36, 13)
        Me.lblPerformance_RegulationLevelValue.TabIndex = 10
        Me.lblPerformance_RegulationLevelValue.Text = "100 %"
        Me.lblPerformance_RegulationLevelValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'prbPerformance_RegulationLevel
        '
        Me.prbPerformance_RegulationLevel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.prbPerformance_RegulationLevel.Location = New System.Drawing.Point(8, 41)
        Me.prbPerformance_RegulationLevel.Name = "prbPerformance_RegulationLevel"
        Me.prbPerformance_RegulationLevel.Size = New System.Drawing.Size(698, 15)
        Me.prbPerformance_RegulationLevel.Step = 1
        Me.prbPerformance_RegulationLevel.TabIndex = 6
        Me.prbPerformance_RegulationLevel.Value = 100
        '
        'm_Note_Text
        '
        Me.m_Note_Text.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.m_Note_Text.Location = New System.Drawing.Point(52, 10)
        Me.m_Note_Text.Multiline = True
        Me.m_Note_Text.Name = "m_Note_Text"
        Me.m_Note_Text.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.m_Note_Text.Size = New System.Drawing.Size(1043, 41)
        Me.m_Note_Text.TabIndex = 3
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(7, 10)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(30, 13)
        Me.Label8.TabIndex = 2
        Me.Label8.Text = "Note"
        '
        'tlpPerformance_Graphs
        '
        Me.tlpPerformance_Graphs.ColumnCount = 3
        Me.tlpPerformance_Graphs.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.tlpPerformance_Graphs.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.tlpPerformance_Graphs.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.tlpPerformance_Graphs.Controls.Add(Me.crtPerformance_Chart1, 0, 0)
        Me.tlpPerformance_Graphs.Controls.Add(Me.crtPerformance_Chart2, 1, 0)
        Me.tlpPerformance_Graphs.Controls.Add(Me.crtPerformance_Chart3, 2, 0)
        Me.tlpPerformance_Graphs.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpPerformance_Graphs.Location = New System.Drawing.Point(0, 0)
        Me.tlpPerformance_Graphs.Name = "tlpPerformance_Graphs"
        Me.tlpPerformance_Graphs.RowCount = 1
        Me.tlpPerformance_Graphs.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpPerformance_Graphs.Size = New System.Drawing.Size(1104, 219)
        Me.tlpPerformance_Graphs.TabIndex = 11
        '
        'crtPerformance_Chart1
        '
        ChartArea1.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number
        ChartArea1.Name = "ChartArea1"
        Me.crtPerformance_Chart1.ChartAreas.Add(ChartArea1)
        Me.crtPerformance_Chart1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.crtPerformance_Chart1.Location = New System.Drawing.Point(3, 3)
        Me.crtPerformance_Chart1.Name = "crtPerformance_Chart1"
        Series1.ChartArea = "ChartArea1"
        Series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline
        Series1.Legend = "Legend1"
        Series1.Name = "Series1"
        Me.crtPerformance_Chart1.Series.Add(Series1)
        Me.crtPerformance_Chart1.Size = New System.Drawing.Size(362, 213)
        Me.crtPerformance_Chart1.TabIndex = 7
        Me.crtPerformance_Chart1.Text = "Chart1"
        '
        'crtPerformance_Chart2
        '
        ChartArea2.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number
        ChartArea2.Name = "ChartArea1"
        Me.crtPerformance_Chart2.ChartAreas.Add(ChartArea2)
        Me.crtPerformance_Chart2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.crtPerformance_Chart2.Location = New System.Drawing.Point(371, 3)
        Me.crtPerformance_Chart2.Name = "crtPerformance_Chart2"
        Series2.ChartArea = "ChartArea1"
        Series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline
        Series2.Name = "Series1"
        Me.crtPerformance_Chart2.Series.Add(Series2)
        Me.crtPerformance_Chart2.Size = New System.Drawing.Size(362, 213)
        Me.crtPerformance_Chart2.TabIndex = 8
        Me.crtPerformance_Chart2.Text = "Chart2"
        '
        'crtPerformance_Chart3
        '
        ChartArea3.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number
        ChartArea3.Name = "ChartArea1"
        Me.crtPerformance_Chart3.ChartAreas.Add(ChartArea3)
        Me.crtPerformance_Chart3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.crtPerformance_Chart3.Location = New System.Drawing.Point(739, 3)
        Me.crtPerformance_Chart3.Name = "crtPerformance_Chart3"
        Series3.ChartArea = "ChartArea1"
        Series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline
        Series3.Name = "Series1"
        Me.crtPerformance_Chart3.Series.Add(Series3)
        Me.crtPerformance_Chart3.Size = New System.Drawing.Size(362, 213)
        Me.crtPerformance_Chart3.TabIndex = 9
        Me.crtPerformance_Chart3.Text = "Generate Report"
        '
        'tbcMain
        '
        Me.tbcMain.Controls.Add(Me.tbpPerformance)
        Me.tbcMain.Controls.Add(Me.tbpCertification)
        Me.tbcMain.Controls.Add(Me.tbpAccessory)
        Me.tbcMain.Controls.Add(Me.tbpCO2Level)
        Me.tbcMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tbcMain.Location = New System.Drawing.Point(5, 5)
        Me.tbcMain.Name = "tbcMain"
        Me.tbcMain.SelectedIndex = 0
        Me.tbcMain.Size = New System.Drawing.Size(1120, 633)
        Me.tbcMain.TabIndex = 0
        '
        'tbpCertification
        '
        Me.tbpCertification.Controls.Add(Me.btnUpdateSAPTable)
        Me.tbpCertification.Controls.Add(Me.Label22)
        Me.tbpCertification.Controls.Add(Me.Label21)
        Me.tbpCertification.Controls.Add(Me.Label20)
        Me.tbpCertification.Controls.Add(Me.dgvSAP)
        Me.tbpCertification.Location = New System.Drawing.Point(4, 22)
        Me.tbpCertification.Name = "tbpCertification"
        Me.tbpCertification.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpCertification.Size = New System.Drawing.Size(1112, 607)
        Me.tbpCertification.TabIndex = 1
        Me.tbpCertification.Text = "Certification"
        Me.tbpCertification.UseVisualStyleBackColor = True
        '
        'btnUpdateSAPTable
        '
        Me.btnUpdateSAPTable.Location = New System.Drawing.Point(965, 7)
        Me.btnUpdateSAPTable.Name = "btnUpdateSAPTable"
        Me.btnUpdateSAPTable.Size = New System.Drawing.Size(125, 21)
        Me.btnUpdateSAPTable.TabIndex = 2
        Me.btnUpdateSAPTable.Text = "Update SAP table"
        Me.btnUpdateSAPTable.UseVisualStyleBackColor = True
        Me.btnUpdateSAPTable.Visible = False
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(5, 425)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(66, 13)
        Me.Label22.TabIndex = 3
        Me.Label22.Text = "Passiv Haus"
        Me.Label22.Visible = False
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(8, 271)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(21, 13)
        Me.Label21.TabIndex = 2
        Me.Label21.Text = "NF"
        Me.Label21.Visible = False
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(8, 15)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(28, 13)
        Me.Label20.TabIndex = 0
        Me.Label20.Text = "SAP"
        '
        'dgvSAP
        '
        Me.dgvSAP.AllowUserToAddRows = False
        Me.dgvSAP.AllowUserToDeleteRows = False
        Me.dgvSAP.AllowUserToResizeColumns = False
        Me.dgvSAP.AllowUserToResizeRows = False
        Me.dgvSAP.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvSAP.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        DataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvSAP.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle4
        Me.dgvSAP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvSAP.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.dgvSAP_ExhaustTerminalConfiguration, Me.dgvSAP_TotalExhaustFlowRate, Me.dgvSAP_TotalSupplyFlowRate, Me.dgvSAP_RegulationLevel, Me.dgvSAP_SpecificFanPower, Me.dgvSAP_HeatExchangeEfficiency, Me.dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant})
        DataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle12.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvSAP.DefaultCellStyle = DataGridViewCellStyle12
        Me.dgvSAP.Location = New System.Drawing.Point(8, 31)
        Me.dgvSAP.Name = "dgvSAP"
        Me.dgvSAP.ReadOnly = True
        DataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle13.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvSAP.RowHeadersDefaultCellStyle = DataGridViewCellStyle13
        Me.dgvSAP.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvSAP.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.dgvSAP.Size = New System.Drawing.Size(1101, 200)
        Me.dgvSAP.TabIndex = 1
        '
        'dgvSAP_ExhaustTerminalConfiguration
        '
        DataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.dgvSAP_ExhaustTerminalConfiguration.DefaultCellStyle = DataGridViewCellStyle5
        Me.dgvSAP_ExhaustTerminalConfiguration.HeaderText = "Exhaust terminal configuration"
        Me.dgvSAP_ExhaustTerminalConfiguration.Name = "dgvSAP_ExhaustTerminalConfiguration"
        Me.dgvSAP_ExhaustTerminalConfiguration.ReadOnly = True
        Me.dgvSAP_ExhaustTerminalConfiguration.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'dgvSAP_TotalExhaustFlowRate
        '
        DataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.dgvSAP_TotalExhaustFlowRate.DefaultCellStyle = DataGridViewCellStyle6
        Me.dgvSAP_TotalExhaustFlowRate.HeaderText = "Total exhaust flow rate [l/s]"
        Me.dgvSAP_TotalExhaustFlowRate.Name = "dgvSAP_TotalExhaustFlowRate"
        Me.dgvSAP_TotalExhaustFlowRate.ReadOnly = True
        Me.dgvSAP_TotalExhaustFlowRate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'dgvSAP_TotalSupplyFlowRate
        '
        DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.dgvSAP_TotalSupplyFlowRate.DefaultCellStyle = DataGridViewCellStyle7
        Me.dgvSAP_TotalSupplyFlowRate.HeaderText = "Total supply flow rate [l/s]"
        Me.dgvSAP_TotalSupplyFlowRate.Name = "dgvSAP_TotalSupplyFlowRate"
        Me.dgvSAP_TotalSupplyFlowRate.ReadOnly = True
        Me.dgvSAP_TotalSupplyFlowRate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'dgvSAP_RegulationLevel
        '
        DataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.dgvSAP_RegulationLevel.DefaultCellStyle = DataGridViewCellStyle8
        Me.dgvSAP_RegulationLevel.HeaderText = "Regulation Level [%]"
        Me.dgvSAP_RegulationLevel.Name = "dgvSAP_RegulationLevel"
        Me.dgvSAP_RegulationLevel.ReadOnly = True
        '
        'dgvSAP_SpecificFanPower
        '
        DataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle9.Format = "N2"
        DataGridViewCellStyle9.NullValue = Nothing
        Me.dgvSAP_SpecificFanPower.DefaultCellStyle = DataGridViewCellStyle9
        Me.dgvSAP_SpecificFanPower.HeaderText = "Specific fan power [W/l/s]"
        Me.dgvSAP_SpecificFanPower.Name = "dgvSAP_SpecificFanPower"
        Me.dgvSAP_SpecificFanPower.ReadOnly = True
        Me.dgvSAP_SpecificFanPower.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'dgvSAP_HeatExchangeEfficiency
        '
        DataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle10.Format = "N2"
        DataGridViewCellStyle10.NullValue = Nothing
        Me.dgvSAP_HeatExchangeEfficiency.DefaultCellStyle = DataGridViewCellStyle10
        Me.dgvSAP_HeatExchangeEfficiency.HeaderText = "Heat exchange effiency [%]"
        Me.dgvSAP_HeatExchangeEfficiency.Name = "dgvSAP_HeatExchangeEfficiency"
        Me.dgvSAP_HeatExchangeEfficiency.ReadOnly = True
        Me.dgvSAP_HeatExchangeEfficiency.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant
        '
        DataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle11.BackColor = System.Drawing.Color.Red
        DataGridViewCellStyle11.ForeColor = System.Drawing.Color.White
        Me.dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant.DefaultCellStyle = DataGridViewCellStyle11
        Me.dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant.HeaderText = "Energy Saving Trust Best Practice Performance Compliant"
        Me.dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant.Name = "dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant"
        Me.dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant.ReadOnly = True
        '
        'tbpAccessory
        '
        Me.tbpAccessory.Controls.Add(Me.grbAccessory_OutputData)
        Me.tbpAccessory.Controls.Add(Me.grbAccessory_EHD)
        Me.tbpAccessory.Controls.Add(Me.grbAccessory_HWD)
        Me.tbpAccessory.Controls.Add(Me.grbAccessory_CWD)
        Me.tbpAccessory.Controls.Add(Me.grbAccessory_InputData)
        Me.tbpAccessory.Location = New System.Drawing.Point(4, 22)
        Me.tbpAccessory.Name = "tbpAccessory"
        Me.tbpAccessory.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpAccessory.Size = New System.Drawing.Size(1112, 607)
        Me.tbpAccessory.TabIndex = 2
        Me.tbpAccessory.Text = "Accessory"
        Me.tbpAccessory.UseVisualStyleBackColor = True
        '
        'grbAccessory_OutputData
        '
        Me.grbAccessory_OutputData.Controls.Add(Me.GroupBox1)
        Me.grbAccessory_OutputData.Controls.Add(Me.GroupBox3)
        Me.grbAccessory_OutputData.Controls.Add(Me.GroupBox2)
        Me.grbAccessory_OutputData.Controls.Add(Me.lblAccessory_Status)
        Me.grbAccessory_OutputData.Controls.Add(Me.Label16)
        Me.grbAccessory_OutputData.Controls.Add(Me.Label19)
        Me.grbAccessory_OutputData.Controls.Add(Me.Label17)
        Me.grbAccessory_OutputData.Controls.Add(Me.txbAccessory_OutputData_Temp)
        Me.grbAccessory_OutputData.Controls.Add(Me.txbAccessory_OutputData_Condensation)
        Me.grbAccessory_OutputData.Controls.Add(Me.txbAccessory_OutputData_RH)
        Me.grbAccessory_OutputData.Controls.Add(Me.txbAccessory_OutputData_PressureDrop)
        Me.grbAccessory_OutputData.Controls.Add(Me.Label18)
        Me.grbAccessory_OutputData.Location = New System.Drawing.Point(6, 242)
        Me.grbAccessory_OutputData.Name = "grbAccessory_OutputData"
        Me.grbAccessory_OutputData.Size = New System.Drawing.Size(755, 220)
        Me.grbAccessory_OutputData.TabIndex = 10
        Me.grbAccessory_OutputData.TabStop = False
        Me.grbAccessory_OutputData.Text = "Output Data"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.txbAccessory_OutputData_CWDSensibleHeat)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.txbAccessory_OutputData_CWDHeatTransferred)
        Me.GroupBox1.Location = New System.Drawing.Point(6, 93)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(234, 79)
        Me.GroupBox1.TabIndex = 9
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "CWD"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(2, 48)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(93, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Sensible Heat [W]"
        '
        'txbAccessory_OutputData_CWDSensibleHeat
        '
        Me.txbAccessory_OutputData_CWDSensibleHeat.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "CWD_SensibleHeat", True))
        Me.txbAccessory_OutputData_CWDSensibleHeat.Location = New System.Drawing.Point(140, 45)
        Me.txbAccessory_OutputData_CWDSensibleHeat.Name = "txbAccessory_OutputData_CWDSensibleHeat"
        Me.txbAccessory_OutputData_CWDSensibleHeat.ReadOnly = True
        Me.txbAccessory_OutputData_CWDSensibleHeat.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_OutputData_CWDSensibleHeat.TabIndex = 9
        '
        'bsrUnitCalculator
        '
        Me.bsrUnitCalculator.DataSource = GetType(SSW.CLUnitCalculator)
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(2, 22)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(107, 13)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Heat Transferred [W]"
        '
        'txbAccessory_OutputData_CWDHeatTransferred
        '
        Me.txbAccessory_OutputData_CWDHeatTransferred.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "CWD_HeatTransferred", True))
        Me.txbAccessory_OutputData_CWDHeatTransferred.Location = New System.Drawing.Point(140, 19)
        Me.txbAccessory_OutputData_CWDHeatTransferred.Name = "txbAccessory_OutputData_CWDHeatTransferred"
        Me.txbAccessory_OutputData_CWDHeatTransferred.ReadOnly = True
        Me.txbAccessory_OutputData_CWDHeatTransferred.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_OutputData_CWDHeatTransferred.TabIndex = 7
        '
        'GroupBox3
        '
        Me.GroupBox3.Location = New System.Drawing.Point(500, 93)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(249, 79)
        Me.GroupBox3.TabIndex = 11
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "EHD"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.txbAccessory_OutputData_HWDHeatTransferred)
        Me.GroupBox2.Location = New System.Drawing.Point(251, 93)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(243, 79)
        Me.GroupBox2.TabIndex = 10
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "HWD"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(6, 22)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(107, 13)
        Me.Label6.TabIndex = 10
        Me.Label6.Text = "Heat Transferred [W]"
        '
        'txbAccessory_OutputData_HWDHeatTransferred
        '
        Me.txbAccessory_OutputData_HWDHeatTransferred.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "HWD_HeatTransferred", True))
        Me.txbAccessory_OutputData_HWDHeatTransferred.Location = New System.Drawing.Point(144, 19)
        Me.txbAccessory_OutputData_HWDHeatTransferred.Name = "txbAccessory_OutputData_HWDHeatTransferred"
        Me.txbAccessory_OutputData_HWDHeatTransferred.ReadOnly = True
        Me.txbAccessory_OutputData_HWDHeatTransferred.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_OutputData_HWDHeatTransferred.TabIndex = 11
        '
        'lblAccessory_Status
        '
        Me.lblAccessory_Status.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.lblAccessory_Status.Location = New System.Drawing.Point(3, 193)
        Me.lblAccessory_Status.Name = "lblAccessory_Status"
        Me.lblAccessory_Status.Size = New System.Drawing.Size(749, 24)
        Me.lblAccessory_Status.TabIndex = 8
        Me.lblAccessory_Status.Text = "Status"
        Me.lblAccessory_Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(8, 28)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(92, 13)
        Me.Label16.TabIndex = 4
        Me.Label16.Text = "Supply Temp. [°C]"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(257, 54)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(93, 13)
        Me.Label19.TabIndex = 6
        Me.Label19.Text = "Condensation [l/s]"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(257, 28)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(46, 13)
        Me.Label17.TabIndex = 6
        Me.Label17.Text = "R.H. [%]"
        '
        'txbAccessory_OutputData_Temp
        '
        Me.txbAccessory_OutputData_Temp.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "Accessory_Temp", True))
        Me.txbAccessory_OutputData_Temp.Location = New System.Drawing.Point(146, 25)
        Me.txbAccessory_OutputData_Temp.Name = "txbAccessory_OutputData_Temp"
        Me.txbAccessory_OutputData_Temp.ReadOnly = True
        Me.txbAccessory_OutputData_Temp.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_OutputData_Temp.TabIndex = 5
        '
        'txbAccessory_OutputData_Condensation
        '
        Me.txbAccessory_OutputData_Condensation.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "Accessory_CondensedWater", True))
        Me.txbAccessory_OutputData_Condensation.Location = New System.Drawing.Point(395, 51)
        Me.txbAccessory_OutputData_Condensation.Name = "txbAccessory_OutputData_Condensation"
        Me.txbAccessory_OutputData_Condensation.ReadOnly = True
        Me.txbAccessory_OutputData_Condensation.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_OutputData_Condensation.TabIndex = 7
        '
        'txbAccessory_OutputData_RH
        '
        Me.txbAccessory_OutputData_RH.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "Accessory_RH", True))
        Me.txbAccessory_OutputData_RH.Location = New System.Drawing.Point(395, 25)
        Me.txbAccessory_OutputData_RH.Name = "txbAccessory_OutputData_RH"
        Me.txbAccessory_OutputData_RH.ReadOnly = True
        Me.txbAccessory_OutputData_RH.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_OutputData_RH.TabIndex = 7
        '
        'txbAccessory_OutputData_PressureDrop
        '
        Me.txbAccessory_OutputData_PressureDrop.AcceptsReturn = True
        Me.txbAccessory_OutputData_PressureDrop.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "Accessory_PressureDrop", True))
        Me.txbAccessory_OutputData_PressureDrop.Location = New System.Drawing.Point(146, 51)
        Me.txbAccessory_OutputData_PressureDrop.Name = "txbAccessory_OutputData_PressureDrop"
        Me.txbAccessory_OutputData_PressureDrop.ReadOnly = True
        Me.txbAccessory_OutputData_PressureDrop.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_OutputData_PressureDrop.TabIndex = 3
        Me.txbAccessory_OutputData_PressureDrop.Text = "100"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(8, 54)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(96, 13)
        Me.Label18.TabIndex = 2
        Me.Label18.Text = "Pressure Drop [Pa]"
        '
        'grbAccessory_EHD
        '
        Me.grbAccessory_EHD.Controls.Add(Me.cbxAccessory_EHDEnabled)
        Me.grbAccessory_EHD.Location = New System.Drawing.Point(506, 91)
        Me.grbAccessory_EHD.Name = "grbAccessory_EHD"
        Me.grbAccessory_EHD.Size = New System.Drawing.Size(255, 145)
        Me.grbAccessory_EHD.TabIndex = 9
        Me.grbAccessory_EHD.TabStop = False
        '
        'cbxAccessory_EHDEnabled
        '
        Me.cbxAccessory_EHDEnabled.AutoSize = True
        Me.cbxAccessory_EHDEnabled.BackColor = System.Drawing.Color.White
        Me.cbxAccessory_EHDEnabled.Location = New System.Drawing.Point(6, 0)
        Me.cbxAccessory_EHDEnabled.Name = "cbxAccessory_EHDEnabled"
        Me.cbxAccessory_EHDEnabled.Size = New System.Drawing.Size(49, 17)
        Me.cbxAccessory_EHDEnabled.TabIndex = 12
        Me.cbxAccessory_EHDEnabled.Text = "EHD"
        Me.cbxAccessory_EHDEnabled.UseVisualStyleBackColor = False
        '
        'grbAccessory_HWD
        '
        Me.grbAccessory_HWD.Controls.Add(Me.nudAccessory_HWD_FluidTypeTec)
        Me.grbAccessory_HWD.Controls.Add(Me.lblAccessory_HWD_OutletTemperature)
        Me.grbAccessory_HWD.Controls.Add(Me.cmbAccessory_HWD_FluidType)
        Me.grbAccessory_HWD.Controls.Add(Me.lblAccessory_HWD_FluidTypeTec)
        Me.grbAccessory_HWD.Controls.Add(Me.lblAccessory_HWD_FluidType)
        Me.grbAccessory_HWD.Controls.Add(Me.lblAccessory_HWD_InletTemperature)
        Me.grbAccessory_HWD.Controls.Add(Me.txbAccessory_HWD_InletTemperature)
        Me.grbAccessory_HWD.Controls.Add(Me.txbAccessory_HWD_OutletTemperature)
        Me.grbAccessory_HWD.Controls.Add(Me.cbxAccessory_HWDEnabled)
        Me.grbAccessory_HWD.Location = New System.Drawing.Point(257, 91)
        Me.grbAccessory_HWD.Name = "grbAccessory_HWD"
        Me.grbAccessory_HWD.Size = New System.Drawing.Size(243, 145)
        Me.grbAccessory_HWD.TabIndex = 8
        Me.grbAccessory_HWD.TabStop = False
        '
        'nudAccessory_HWD_FluidTypeTec
        '
        Me.nudAccessory_HWD_FluidTypeTec.Location = New System.Drawing.Point(144, 103)
        Me.nudAccessory_HWD_FluidTypeTec.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nudAccessory_HWD_FluidTypeTec.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudAccessory_HWD_FluidTypeTec.Name = "nudAccessory_HWD_FluidTypeTec"
        Me.nudAccessory_HWD_FluidTypeTec.Size = New System.Drawing.Size(59, 20)
        Me.nudAccessory_HWD_FluidTypeTec.TabIndex = 22
        Me.nudAccessory_HWD_FluidTypeTec.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lblAccessory_HWD_OutletTemperature
        '
        Me.lblAccessory_HWD_OutletTemperature.AutoSize = True
        Me.lblAccessory_HWD_OutletTemperature.Location = New System.Drawing.Point(6, 53)
        Me.lblAccessory_HWD_OutletTemperature.Name = "lblAccessory_HWD_OutletTemperature"
        Me.lblAccessory_HWD_OutletTemperature.Size = New System.Drawing.Size(113, 13)
        Me.lblAccessory_HWD_OutletTemperature.TabIndex = 21
        Me.lblAccessory_HWD_OutletTemperature.Text = "Fluid Outlet Temp. [°C]"
        '
        'cmbAccessory_HWD_FluidType
        '
        Me.cmbAccessory_HWD_FluidType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbAccessory_HWD_FluidType.FormattingEnabled = True
        Me.cmbAccessory_HWD_FluidType.Items.AddRange(New Object() {"CLRC 23", "CLRC 53", "CLRC 93", "CLRC 123", "CLRC 163", "CLRC 223", "CLRC 323", "CLRC 423"})
        Me.cmbAccessory_HWD_FluidType.Location = New System.Drawing.Point(144, 75)
        Me.cmbAccessory_HWD_FluidType.Name = "cmbAccessory_HWD_FluidType"
        Me.cmbAccessory_HWD_FluidType.Size = New System.Drawing.Size(87, 21)
        Me.cmbAccessory_HWD_FluidType.TabIndex = 20
        '
        'lblAccessory_HWD_FluidTypeTec
        '
        Me.lblAccessory_HWD_FluidTypeTec.AutoSize = True
        Me.lblAccessory_HWD_FluidTypeTec.Location = New System.Drawing.Point(206, 105)
        Me.lblAccessory_HWD_FluidTypeTec.Name = "lblAccessory_HWD_FluidTypeTec"
        Me.lblAccessory_HWD_FluidTypeTec.Size = New System.Drawing.Size(15, 13)
        Me.lblAccessory_HWD_FluidTypeTec.TabIndex = 19
        Me.lblAccessory_HWD_FluidTypeTec.Text = "%"
        '
        'lblAccessory_HWD_FluidType
        '
        Me.lblAccessory_HWD_FluidType.AutoSize = True
        Me.lblAccessory_HWD_FluidType.Location = New System.Drawing.Point(6, 79)
        Me.lblAccessory_HWD_FluidType.Name = "lblAccessory_HWD_FluidType"
        Me.lblAccessory_HWD_FluidType.Size = New System.Drawing.Size(56, 13)
        Me.lblAccessory_HWD_FluidType.TabIndex = 18
        Me.lblAccessory_HWD_FluidType.Text = "Fluid Type"
        '
        'lblAccessory_HWD_InletTemperature
        '
        Me.lblAccessory_HWD_InletTemperature.AutoSize = True
        Me.lblAccessory_HWD_InletTemperature.Location = New System.Drawing.Point(6, 26)
        Me.lblAccessory_HWD_InletTemperature.Name = "lblAccessory_HWD_InletTemperature"
        Me.lblAccessory_HWD_InletTemperature.Size = New System.Drawing.Size(105, 13)
        Me.lblAccessory_HWD_InletTemperature.TabIndex = 15
        Me.lblAccessory_HWD_InletTemperature.Text = "Fluid Inlet Temp. [°C]"
        '
        'txbAccessory_HWD_InletTemperature
        '
        Me.txbAccessory_HWD_InletTemperature.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "HWD_InletTemperature", True))
        Me.txbAccessory_HWD_InletTemperature.Location = New System.Drawing.Point(144, 23)
        Me.txbAccessory_HWD_InletTemperature.Name = "txbAccessory_HWD_InletTemperature"
        Me.txbAccessory_HWD_InletTemperature.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_HWD_InletTemperature.TabIndex = 16
        Me.txbAccessory_HWD_InletTemperature.Text = "0"
        '
        'txbAccessory_HWD_OutletTemperature
        '
        Me.txbAccessory_HWD_OutletTemperature.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "HWD_OutletTemperature", True))
        Me.txbAccessory_HWD_OutletTemperature.Location = New System.Drawing.Point(144, 50)
        Me.txbAccessory_HWD_OutletTemperature.Name = "txbAccessory_HWD_OutletTemperature"
        Me.txbAccessory_HWD_OutletTemperature.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_HWD_OutletTemperature.TabIndex = 17
        Me.txbAccessory_HWD_OutletTemperature.Text = "35"
        '
        'cbxAccessory_HWDEnabled
        '
        Me.cbxAccessory_HWDEnabled.AutoSize = True
        Me.cbxAccessory_HWDEnabled.BackColor = System.Drawing.Color.White
        Me.cbxAccessory_HWDEnabled.Location = New System.Drawing.Point(6, 0)
        Me.cbxAccessory_HWDEnabled.Name = "cbxAccessory_HWDEnabled"
        Me.cbxAccessory_HWDEnabled.Size = New System.Drawing.Size(53, 17)
        Me.cbxAccessory_HWDEnabled.TabIndex = 12
        Me.cbxAccessory_HWDEnabled.Text = "HWD"
        Me.cbxAccessory_HWDEnabled.UseVisualStyleBackColor = False
        '
        'grbAccessory_CWD
        '
        Me.grbAccessory_CWD.Controls.Add(Me.nudAccessory_CWD_FluidTypeTec)
        Me.grbAccessory_CWD.Controls.Add(Me.lblAccessory_CWD_OutletTemperature)
        Me.grbAccessory_CWD.Controls.Add(Me.cbxAccessory_CWDEnabled)
        Me.grbAccessory_CWD.Controls.Add(Me.cmbAccessory_CWD_FluidType)
        Me.grbAccessory_CWD.Controls.Add(Me.lblAccessory_CWD_FluidTypeTec)
        Me.grbAccessory_CWD.Controls.Add(Me.lblAccessory_CWD_FluidType)
        Me.grbAccessory_CWD.Controls.Add(Me.lblAccessory_CWD_InletTemperature)
        Me.grbAccessory_CWD.Controls.Add(Me.txbAccessory_CWD_InletTemperature)
        Me.grbAccessory_CWD.Controls.Add(Me.txbAccessory_CWD_OutletTemperature)
        Me.grbAccessory_CWD.Location = New System.Drawing.Point(6, 91)
        Me.grbAccessory_CWD.Name = "grbAccessory_CWD"
        Me.grbAccessory_CWD.Size = New System.Drawing.Size(245, 145)
        Me.grbAccessory_CWD.TabIndex = 7
        Me.grbAccessory_CWD.TabStop = False
        '
        'nudAccessory_CWD_FluidTypeTec
        '
        Me.nudAccessory_CWD_FluidTypeTec.Location = New System.Drawing.Point(146, 103)
        Me.nudAccessory_CWD_FluidTypeTec.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nudAccessory_CWD_FluidTypeTec.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.nudAccessory_CWD_FluidTypeTec.Name = "nudAccessory_CWD_FluidTypeTec"
        Me.nudAccessory_CWD_FluidTypeTec.Size = New System.Drawing.Size(59, 20)
        Me.nudAccessory_CWD_FluidTypeTec.TabIndex = 14
        Me.nudAccessory_CWD_FluidTypeTec.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lblAccessory_CWD_OutletTemperature
        '
        Me.lblAccessory_CWD_OutletTemperature.AutoSize = True
        Me.lblAccessory_CWD_OutletTemperature.Location = New System.Drawing.Point(8, 53)
        Me.lblAccessory_CWD_OutletTemperature.Name = "lblAccessory_CWD_OutletTemperature"
        Me.lblAccessory_CWD_OutletTemperature.Size = New System.Drawing.Size(113, 13)
        Me.lblAccessory_CWD_OutletTemperature.TabIndex = 13
        Me.lblAccessory_CWD_OutletTemperature.Text = "Fluid Outlet Temp. [°C]"
        '
        'cbxAccessory_CWDEnabled
        '
        Me.cbxAccessory_CWDEnabled.AutoSize = True
        Me.cbxAccessory_CWDEnabled.BackColor = System.Drawing.Color.White
        Me.cbxAccessory_CWDEnabled.Location = New System.Drawing.Point(6, 0)
        Me.cbxAccessory_CWDEnabled.Name = "cbxAccessory_CWDEnabled"
        Me.cbxAccessory_CWDEnabled.Size = New System.Drawing.Size(52, 17)
        Me.cbxAccessory_CWDEnabled.TabIndex = 12
        Me.cbxAccessory_CWDEnabled.Text = "CWD"
        Me.cbxAccessory_CWDEnabled.UseVisualStyleBackColor = False
        '
        'cmbAccessory_CWD_FluidType
        '
        Me.cmbAccessory_CWD_FluidType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbAccessory_CWD_FluidType.FormattingEnabled = True
        Me.cmbAccessory_CWD_FluidType.Location = New System.Drawing.Point(146, 75)
        Me.cmbAccessory_CWD_FluidType.Name = "cmbAccessory_CWD_FluidType"
        Me.cmbAccessory_CWD_FluidType.Size = New System.Drawing.Size(87, 21)
        Me.cmbAccessory_CWD_FluidType.TabIndex = 9
        '
        'lblAccessory_CWD_FluidTypeTec
        '
        Me.lblAccessory_CWD_FluidTypeTec.AutoSize = True
        Me.lblAccessory_CWD_FluidTypeTec.Location = New System.Drawing.Point(209, 105)
        Me.lblAccessory_CWD_FluidTypeTec.Name = "lblAccessory_CWD_FluidTypeTec"
        Me.lblAccessory_CWD_FluidTypeTec.Size = New System.Drawing.Size(15, 13)
        Me.lblAccessory_CWD_FluidTypeTec.TabIndex = 8
        Me.lblAccessory_CWD_FluidTypeTec.Text = "%"
        '
        'lblAccessory_CWD_FluidType
        '
        Me.lblAccessory_CWD_FluidType.AutoSize = True
        Me.lblAccessory_CWD_FluidType.Location = New System.Drawing.Point(8, 79)
        Me.lblAccessory_CWD_FluidType.Name = "lblAccessory_CWD_FluidType"
        Me.lblAccessory_CWD_FluidType.Size = New System.Drawing.Size(56, 13)
        Me.lblAccessory_CWD_FluidType.TabIndex = 8
        Me.lblAccessory_CWD_FluidType.Text = "Fluid Type"
        '
        'lblAccessory_CWD_InletTemperature
        '
        Me.lblAccessory_CWD_InletTemperature.AutoSize = True
        Me.lblAccessory_CWD_InletTemperature.Location = New System.Drawing.Point(8, 26)
        Me.lblAccessory_CWD_InletTemperature.Name = "lblAccessory_CWD_InletTemperature"
        Me.lblAccessory_CWD_InletTemperature.Size = New System.Drawing.Size(105, 13)
        Me.lblAccessory_CWD_InletTemperature.TabIndex = 4
        Me.lblAccessory_CWD_InletTemperature.Text = "Fluid Inlet Temp. [°C]"
        '
        'txbAccessory_CWD_InletTemperature
        '
        Me.txbAccessory_CWD_InletTemperature.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "CWD_InletTemperature", True))
        Me.txbAccessory_CWD_InletTemperature.Location = New System.Drawing.Point(146, 23)
        Me.txbAccessory_CWD_InletTemperature.Name = "txbAccessory_CWD_InletTemperature"
        Me.txbAccessory_CWD_InletTemperature.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_CWD_InletTemperature.TabIndex = 5
        Me.txbAccessory_CWD_InletTemperature.Text = "7"
        '
        'txbAccessory_CWD_OutletTemperature
        '
        Me.txbAccessory_CWD_OutletTemperature.DataBindings.Add(New System.Windows.Forms.Binding("Text", Me.bsrUnitCalculator, "CWD_OutletTemperature", True))
        Me.txbAccessory_CWD_OutletTemperature.Location = New System.Drawing.Point(146, 50)
        Me.txbAccessory_CWD_OutletTemperature.Name = "txbAccessory_CWD_OutletTemperature"
        Me.txbAccessory_CWD_OutletTemperature.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_CWD_OutletTemperature.TabIndex = 7
        Me.txbAccessory_CWD_OutletTemperature.Text = "12"
        '
        'grbAccessory_InputData
        '
        Me.grbAccessory_InputData.Controls.Add(Me.lblAccessory_InputData_Temp)
        Me.grbAccessory_InputData.Controls.Add(Me.lblAccessory_InputData_RHTemp)
        Me.grbAccessory_InputData.Controls.Add(Me.txbAccessory_InputData_Temp)
        Me.grbAccessory_InputData.Controls.Add(Me.txbAccessory_InputData_RH)
        Me.grbAccessory_InputData.Controls.Add(Me.txbAccessory_InputData_AirFlow)
        Me.grbAccessory_InputData.Controls.Add(Me.lblAccessory_InputData_AirFlow)
        Me.grbAccessory_InputData.Location = New System.Drawing.Point(6, 6)
        Me.grbAccessory_InputData.Name = "grbAccessory_InputData"
        Me.grbAccessory_InputData.Size = New System.Drawing.Size(755, 79)
        Me.grbAccessory_InputData.TabIndex = 6
        Me.grbAccessory_InputData.TabStop = False
        Me.grbAccessory_InputData.Text = "Input Data"
        '
        'lblAccessory_InputData_Temp
        '
        Me.lblAccessory_InputData_Temp.AutoSize = True
        Me.lblAccessory_InputData_Temp.Location = New System.Drawing.Point(8, 26)
        Me.lblAccessory_InputData_Temp.Name = "lblAccessory_InputData_Temp"
        Me.lblAccessory_InputData_Temp.Size = New System.Drawing.Size(92, 13)
        Me.lblAccessory_InputData_Temp.TabIndex = 4
        Me.lblAccessory_InputData_Temp.Text = "Supply Temp. [°C]"
        '
        'lblAccessory_InputData_RHTemp
        '
        Me.lblAccessory_InputData_RHTemp.AutoSize = True
        Me.lblAccessory_InputData_RHTemp.Location = New System.Drawing.Point(285, 26)
        Me.lblAccessory_InputData_RHTemp.Name = "lblAccessory_InputData_RHTemp"
        Me.lblAccessory_InputData_RHTemp.Size = New System.Drawing.Size(46, 13)
        Me.lblAccessory_InputData_RHTemp.TabIndex = 6
        Me.lblAccessory_InputData_RHTemp.Text = "R.H. [%]"
        '
        'txbAccessory_InputData_Temp
        '
        Me.txbAccessory_InputData_Temp.Location = New System.Drawing.Point(146, 23)
        Me.txbAccessory_InputData_Temp.Name = "txbAccessory_InputData_Temp"
        Me.txbAccessory_InputData_Temp.ReadOnly = True
        Me.txbAccessory_InputData_Temp.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_InputData_Temp.TabIndex = 5
        '
        'txbAccessory_InputData_RH
        '
        Me.txbAccessory_InputData_RH.Location = New System.Drawing.Point(407, 23)
        Me.txbAccessory_InputData_RH.Name = "txbAccessory_InputData_RH"
        Me.txbAccessory_InputData_RH.ReadOnly = True
        Me.txbAccessory_InputData_RH.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_InputData_RH.TabIndex = 7
        '
        'txbAccessory_InputData_AirFlow
        '
        Me.txbAccessory_InputData_AirFlow.AcceptsReturn = True
        Me.txbAccessory_InputData_AirFlow.Location = New System.Drawing.Point(146, 49)
        Me.txbAccessory_InputData_AirFlow.Name = "txbAccessory_InputData_AirFlow"
        Me.txbAccessory_InputData_AirFlow.Size = New System.Drawing.Size(87, 20)
        Me.txbAccessory_InputData_AirFlow.TabIndex = 3
        Me.txbAccessory_InputData_AirFlow.Text = "100"
        '
        'lblAccessory_InputData_AirFlow
        '
        Me.lblAccessory_InputData_AirFlow.AutoSize = True
        Me.lblAccessory_InputData_AirFlow.Location = New System.Drawing.Point(8, 52)
        Me.lblAccessory_InputData_AirFlow.Name = "lblAccessory_InputData_AirFlow"
        Me.lblAccessory_InputData_AirFlow.Size = New System.Drawing.Size(78, 13)
        Me.lblAccessory_InputData_AirFlow.TabIndex = 2
        Me.lblAccessory_InputData_AirFlow.Text = "Air Flow [m3/h]"
        '
        'tbpCO2Level
        '
        Me.tbpCO2Level.Controls.Add(Me.chbCO2Level_addtoreport)
        Me.tbpCO2Level.Controls.Add(Me.crtCO2Level_Chart1)
        Me.tbpCO2Level.Controls.Add(Me.grbCO2Level_Parameters)
        Me.tbpCO2Level.Controls.Add(Me.grbCO2Level_use)
        Me.tbpCO2Level.Controls.Add(Me.grbCO2Level_Room)
        Me.tbpCO2Level.Location = New System.Drawing.Point(4, 22)
        Me.tbpCO2Level.Name = "tbpCO2Level"
        Me.tbpCO2Level.Padding = New System.Windows.Forms.Padding(3)
        Me.tbpCO2Level.Size = New System.Drawing.Size(1112, 607)
        Me.tbpCO2Level.TabIndex = 3
        Me.tbpCO2Level.Text = "CO2 Level Calculator"
        Me.tbpCO2Level.UseVisualStyleBackColor = True
        '
        'chbCO2Level_addtoreport
        '
        Me.chbCO2Level_addtoreport.AutoSize = True
        Me.chbCO2Level_addtoreport.Location = New System.Drawing.Point(348, 584)
        Me.chbCO2Level_addtoreport.Name = "chbCO2Level_addtoreport"
        Me.chbCO2Level_addtoreport.Size = New System.Drawing.Size(87, 17)
        Me.chbCO2Level_addtoreport.TabIndex = 4
        Me.chbCO2Level_addtoreport.Text = "Add to report"
        Me.chbCO2Level_addtoreport.UseVisualStyleBackColor = True
        '
        'crtCO2Level_Chart1
        '
        Me.crtCO2Level_Chart1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        ChartArea4.Name = "ChartArea1"
        Me.crtCO2Level_Chart1.ChartAreas.Add(ChartArea4)
        Legend1.Enabled = False
        Legend1.Name = "Legend1"
        Me.crtCO2Level_Chart1.Legends.Add(Legend1)
        Me.crtCO2Level_Chart1.Location = New System.Drawing.Point(348, 6)
        Me.crtCO2Level_Chart1.Name = "crtCO2Level_Chart1"
        Series4.ChartArea = "ChartArea1"
        Series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline
        Series4.IsVisibleInLegend = False
        Series4.Legend = "Legend1"
        Series4.Name = "Series1"
        Me.crtCO2Level_Chart1.Series.Add(Series4)
        Me.crtCO2Level_Chart1.Size = New System.Drawing.Size(758, 572)
        Me.crtCO2Level_Chart1.TabIndex = 2
        Me.crtCO2Level_Chart1.Text = "Chart1"
        '
        'grbCO2Level_Parameters
        '
        Me.grbCO2Level_Parameters.Controls.Add(Me.chbCO2Level_Parameters_airflow_check)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_stdpreset)
        Me.grbCO2Level_Parameters.Controls.Add(Me.cmbCO2Level_Parameters_stdpreset)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_CalcMet)
        Me.grbCO2Level_Parameters.Controls.Add(Me.cmbCO2Level_Parameters_CalcMet)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_af_area)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_af_area)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_af_person)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_af_person)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_af_area_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_af_person_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_airflow_design_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_airflow_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_airflow_design)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_airflow)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_af_area_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_airflow_design_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_af_person_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_airflow_design)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_airflow_m3h)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_airflow)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_maxCO2)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_maxCO2)
        Me.grbCO2Level_Parameters.Controls.Add(Me.lblCO2Level_Parameters_extCO2)
        Me.grbCO2Level_Parameters.Controls.Add(Me.txbCO2Level_Parameters_extCO2)
        Me.grbCO2Level_Parameters.Location = New System.Drawing.Point(6, 318)
        Me.grbCO2Level_Parameters.Name = "grbCO2Level_Parameters"
        Me.grbCO2Level_Parameters.Size = New System.Drawing.Size(336, 283)
        Me.grbCO2Level_Parameters.TabIndex = 1
        Me.grbCO2Level_Parameters.TabStop = False
        Me.grbCO2Level_Parameters.Text = "Parameters"
        '
        'chbCO2Level_Parameters_airflow_check
        '
        Me.chbCO2Level_Parameters_airflow_check.AutoSize = True
        Me.chbCO2Level_Parameters_airflow_check.Location = New System.Drawing.Point(203, 79)
        Me.chbCO2Level_Parameters_airflow_check.Name = "chbCO2Level_Parameters_airflow_check"
        Me.chbCO2Level_Parameters_airflow_check.Size = New System.Drawing.Size(127, 17)
        Me.chbCO2Level_Parameters_airflow_check.TabIndex = 5
        Me.chbCO2Level_Parameters_airflow_check.Text = "Check Working Point"
        Me.chbCO2Level_Parameters_airflow_check.UseVisualStyleBackColor = True
        '
        'lblCO2Level_Parameters_stdpreset
        '
        Me.lblCO2Level_Parameters_stdpreset.AutoSize = True
        Me.lblCO2Level_Parameters_stdpreset.Location = New System.Drawing.Point(6, 19)
        Me.lblCO2Level_Parameters_stdpreset.Name = "lblCO2Level_Parameters_stdpreset"
        Me.lblCO2Level_Parameters_stdpreset.Size = New System.Drawing.Size(83, 13)
        Me.lblCO2Level_Parameters_stdpreset.TabIndex = 1
        Me.lblCO2Level_Parameters_stdpreset.Text = "Standard Preset"
        '
        'cmbCO2Level_Parameters_stdpreset
        '
        Me.cmbCO2Level_Parameters_stdpreset.FormattingEnabled = True
        Me.cmbCO2Level_Parameters_stdpreset.Location = New System.Drawing.Point(110, 16)
        Me.cmbCO2Level_Parameters_stdpreset.Name = "cmbCO2Level_Parameters_stdpreset"
        Me.cmbCO2Level_Parameters_stdpreset.Size = New System.Drawing.Size(220, 21)
        Me.cmbCO2Level_Parameters_stdpreset.TabIndex = 0
        '
        'lblCO2Level_Parameters_CalcMet
        '
        Me.lblCO2Level_Parameters_CalcMet.AutoSize = True
        Me.lblCO2Level_Parameters_CalcMet.Location = New System.Drawing.Point(6, 49)
        Me.lblCO2Level_Parameters_CalcMet.Name = "lblCO2Level_Parameters_CalcMet"
        Me.lblCO2Level_Parameters_CalcMet.Size = New System.Drawing.Size(98, 13)
        Me.lblCO2Level_Parameters_CalcMet.TabIndex = 1
        Me.lblCO2Level_Parameters_CalcMet.Text = "Calculation Method"
        '
        'cmbCO2Level_Parameters_CalcMet
        '
        Me.cmbCO2Level_Parameters_CalcMet.FormattingEnabled = True
        Me.cmbCO2Level_Parameters_CalcMet.Location = New System.Drawing.Point(110, 46)
        Me.cmbCO2Level_Parameters_CalcMet.Name = "cmbCO2Level_Parameters_CalcMet"
        Me.cmbCO2Level_Parameters_CalcMet.Size = New System.Drawing.Size(220, 21)
        Me.cmbCO2Level_Parameters_CalcMet.TabIndex = 0
        '
        'lblCO2Level_Parameters_af_area
        '
        Me.lblCO2Level_Parameters_af_area.AutoSize = True
        Me.lblCO2Level_Parameters_af_area.Location = New System.Drawing.Point(6, 218)
        Me.lblCO2Level_Parameters_af_area.Name = "lblCO2Level_Parameters_af_area"
        Me.lblCO2Level_Parameters_af_area.Size = New System.Drawing.Size(128, 13)
        Me.lblCO2Level_Parameters_af_area.TabIndex = 0
        Me.lblCO2Level_Parameters_af_area.Text = "Airflow per Area  [l/(s m2)]"
        '
        'txbCO2Level_Parameters_af_area
        '
        Me.txbCO2Level_Parameters_af_area.Location = New System.Drawing.Point(142, 215)
        Me.txbCO2Level_Parameters_af_area.Name = "txbCO2Level_Parameters_af_area"
        Me.txbCO2Level_Parameters_af_area.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_af_area.TabIndex = 0
        '
        'lblCO2Level_Parameters_af_person
        '
        Me.lblCO2Level_Parameters_af_person.AutoSize = True
        Me.lblCO2Level_Parameters_af_person.Location = New System.Drawing.Point(6, 189)
        Me.lblCO2Level_Parameters_af_person.Name = "lblCO2Level_Parameters_af_person"
        Me.lblCO2Level_Parameters_af_person.Size = New System.Drawing.Size(116, 13)
        Me.lblCO2Level_Parameters_af_person.TabIndex = 0
        Me.lblCO2Level_Parameters_af_person.Text = "Airflow per Person  [l/s]"
        '
        'txbCO2Level_Parameters_af_person
        '
        Me.txbCO2Level_Parameters_af_person.Location = New System.Drawing.Point(142, 186)
        Me.txbCO2Level_Parameters_af_person.Name = "txbCO2Level_Parameters_af_person"
        Me.txbCO2Level_Parameters_af_person.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_af_person.TabIndex = 0
        '
        'lblCO2Level_Parameters_af_area_m3h
        '
        Me.lblCO2Level_Parameters_af_area_m3h.AutoSize = True
        Me.lblCO2Level_Parameters_af_area_m3h.Location = New System.Drawing.Point(211, 218)
        Me.lblCO2Level_Parameters_af_area_m3h.Name = "lblCO2Level_Parameters_af_area_m3h"
        Me.lblCO2Level_Parameters_af_area_m3h.Size = New System.Drawing.Size(61, 13)
        Me.lblCO2Level_Parameters_af_area_m3h.TabIndex = 0
        Me.lblCO2Level_Parameters_af_area_m3h.Text = "[m3/(h m2)]"
        '
        'lblCO2Level_Parameters_af_person_m3h
        '
        Me.lblCO2Level_Parameters_af_person_m3h.AutoSize = True
        Me.lblCO2Level_Parameters_af_person_m3h.Location = New System.Drawing.Point(234, 189)
        Me.lblCO2Level_Parameters_af_person_m3h.Name = "lblCO2Level_Parameters_af_person_m3h"
        Me.lblCO2Level_Parameters_af_person_m3h.Size = New System.Drawing.Size(38, 13)
        Me.lblCO2Level_Parameters_af_person_m3h.TabIndex = 0
        Me.lblCO2Level_Parameters_af_person_m3h.Text = "[m3/h]"
        '
        'lblCO2Level_Parameters_airflow_design_m3h
        '
        Me.lblCO2Level_Parameters_airflow_design_m3h.AutoSize = True
        Me.lblCO2Level_Parameters_airflow_design_m3h.Location = New System.Drawing.Point(234, 159)
        Me.lblCO2Level_Parameters_airflow_design_m3h.Name = "lblCO2Level_Parameters_airflow_design_m3h"
        Me.lblCO2Level_Parameters_airflow_design_m3h.Size = New System.Drawing.Size(38, 13)
        Me.lblCO2Level_Parameters_airflow_design_m3h.TabIndex = 0
        Me.lblCO2Level_Parameters_airflow_design_m3h.Text = "[m3/h]"
        '
        'lblCO2Level_Parameters_airflow_m3h
        '
        Me.lblCO2Level_Parameters_airflow_m3h.AutoSize = True
        Me.lblCO2Level_Parameters_airflow_m3h.Location = New System.Drawing.Point(234, 131)
        Me.lblCO2Level_Parameters_airflow_m3h.Name = "lblCO2Level_Parameters_airflow_m3h"
        Me.lblCO2Level_Parameters_airflow_m3h.Size = New System.Drawing.Size(38, 13)
        Me.lblCO2Level_Parameters_airflow_m3h.TabIndex = 0
        Me.lblCO2Level_Parameters_airflow_m3h.Text = "[m3/h]"
        '
        'lblCO2Level_Parameters_airflow_design
        '
        Me.lblCO2Level_Parameters_airflow_design.AutoSize = True
        Me.lblCO2Level_Parameters_airflow_design.Location = New System.Drawing.Point(6, 159)
        Me.lblCO2Level_Parameters_airflow_design.Name = "lblCO2Level_Parameters_airflow_design"
        Me.lblCO2Level_Parameters_airflow_design.Size = New System.Drawing.Size(95, 13)
        Me.lblCO2Level_Parameters_airflow_design.TabIndex = 0
        Me.lblCO2Level_Parameters_airflow_design.Text = "Setting Airflow [l/s]"
        '
        'lblCO2Level_Parameters_airflow
        '
        Me.lblCO2Level_Parameters_airflow.AutoSize = True
        Me.lblCO2Level_Parameters_airflow.Location = New System.Drawing.Point(6, 131)
        Me.lblCO2Level_Parameters_airflow.Name = "lblCO2Level_Parameters_airflow"
        Me.lblCO2Level_Parameters_airflow.Size = New System.Drawing.Size(105, 13)
        Me.lblCO2Level_Parameters_airflow.TabIndex = 0
        Me.lblCO2Level_Parameters_airflow.Text = "Airflow Demand  [l/s]"
        '
        'txbCO2Level_Parameters_af_area_m3h
        '
        Me.txbCO2Level_Parameters_af_area_m3h.Location = New System.Drawing.Point(278, 215)
        Me.txbCO2Level_Parameters_af_area_m3h.Name = "txbCO2Level_Parameters_af_area_m3h"
        Me.txbCO2Level_Parameters_af_area_m3h.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_af_area_m3h.TabIndex = 0
        '
        'txbCO2Level_Parameters_airflow_design_m3h
        '
        Me.txbCO2Level_Parameters_airflow_design_m3h.Location = New System.Drawing.Point(278, 156)
        Me.txbCO2Level_Parameters_airflow_design_m3h.Name = "txbCO2Level_Parameters_airflow_design_m3h"
        Me.txbCO2Level_Parameters_airflow_design_m3h.ReadOnly = True
        Me.txbCO2Level_Parameters_airflow_design_m3h.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_airflow_design_m3h.TabIndex = 0
        '
        'txbCO2Level_Parameters_af_person_m3h
        '
        Me.txbCO2Level_Parameters_af_person_m3h.Location = New System.Drawing.Point(278, 186)
        Me.txbCO2Level_Parameters_af_person_m3h.Name = "txbCO2Level_Parameters_af_person_m3h"
        Me.txbCO2Level_Parameters_af_person_m3h.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_af_person_m3h.TabIndex = 0
        '
        'txbCO2Level_Parameters_airflow_design
        '
        Me.txbCO2Level_Parameters_airflow_design.Location = New System.Drawing.Point(142, 156)
        Me.txbCO2Level_Parameters_airflow_design.Name = "txbCO2Level_Parameters_airflow_design"
        Me.txbCO2Level_Parameters_airflow_design.ReadOnly = True
        Me.txbCO2Level_Parameters_airflow_design.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_airflow_design.TabIndex = 0
        '
        'txbCO2Level_Parameters_airflow_m3h
        '
        Me.txbCO2Level_Parameters_airflow_m3h.Location = New System.Drawing.Point(278, 128)
        Me.txbCO2Level_Parameters_airflow_m3h.Name = "txbCO2Level_Parameters_airflow_m3h"
        Me.txbCO2Level_Parameters_airflow_m3h.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_airflow_m3h.TabIndex = 0
        '
        'txbCO2Level_Parameters_airflow
        '
        Me.txbCO2Level_Parameters_airflow.Location = New System.Drawing.Point(142, 128)
        Me.txbCO2Level_Parameters_airflow.Name = "txbCO2Level_Parameters_airflow"
        Me.txbCO2Level_Parameters_airflow.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_airflow.TabIndex = 0
        '
        'lblCO2Level_Parameters_maxCO2
        '
        Me.lblCO2Level_Parameters_maxCO2.AutoSize = True
        Me.lblCO2Level_Parameters_maxCO2.Location = New System.Drawing.Point(6, 105)
        Me.lblCO2Level_Parameters_maxCO2.Name = "lblCO2Level_Parameters_maxCO2"
        Me.lblCO2Level_Parameters_maxCO2.Size = New System.Drawing.Size(115, 13)
        Me.lblCO2Level_Parameters_maxCO2.TabIndex = 0
        Me.lblCO2Level_Parameters_maxCO2.Text = "Max. CO2 Level  [ppm]"
        '
        'txbCO2Level_Parameters_maxCO2
        '
        Me.txbCO2Level_Parameters_maxCO2.Location = New System.Drawing.Point(142, 102)
        Me.txbCO2Level_Parameters_maxCO2.Name = "txbCO2Level_Parameters_maxCO2"
        Me.txbCO2Level_Parameters_maxCO2.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_maxCO2.TabIndex = 0
        Me.txbCO2Level_Parameters_maxCO2.Text = "1000"
        '
        'lblCO2Level_Parameters_extCO2
        '
        Me.lblCO2Level_Parameters_extCO2.AutoSize = True
        Me.lblCO2Level_Parameters_extCO2.Location = New System.Drawing.Point(6, 79)
        Me.lblCO2Level_Parameters_extCO2.Name = "lblCO2Level_Parameters_extCO2"
        Me.lblCO2Level_Parameters_extCO2.Size = New System.Drawing.Size(130, 13)
        Me.lblCO2Level_Parameters_extCO2.TabIndex = 0
        Me.lblCO2Level_Parameters_extCO2.Text = "Outdoor CO2 Level  [ppm]"
        '
        'txbCO2Level_Parameters_extCO2
        '
        Me.txbCO2Level_Parameters_extCO2.Location = New System.Drawing.Point(142, 76)
        Me.txbCO2Level_Parameters_extCO2.Name = "txbCO2Level_Parameters_extCO2"
        Me.txbCO2Level_Parameters_extCO2.Size = New System.Drawing.Size(52, 20)
        Me.txbCO2Level_Parameters_extCO2.TabIndex = 0
        Me.txbCO2Level_Parameters_extCO2.Text = "380"
        '
        'grbCO2Level_use
        '
        Me.grbCO2Level_use.Controls.Add(Me.lblCO2Level_Usage_Presence)
        Me.grbCO2Level_use.Controls.Add(Me.lblCO2Level_Usage_Break)
        Me.grbCO2Level_use.Controls.Add(Me.lblCO2Level_Usage_People)
        Me.grbCO2Level_use.Controls.Add(Me.lblCO2Level_Usage_CO2prod)
        Me.grbCO2Level_use.Controls.Add(Me.lblCO2Level_Usage_Activity)
        Me.grbCO2Level_use.Controls.Add(Me.lblCO2Level_Usage_Period)
        Me.grbCO2Level_use.Controls.Add(Me.txbCO2Level_Usage_CO2prod)
        Me.grbCO2Level_use.Controls.Add(Me.txbCO2Level_Usage_Activity)
        Me.grbCO2Level_use.Controls.Add(Me.txbCO2Level_Usage_PeopleBreak)
        Me.grbCO2Level_use.Controls.Add(Me.txbCO2Level_Usage_PeoplePresence)
        Me.grbCO2Level_use.Controls.Add(Me.txbCO2Level_Usage_PeriodBreak)
        Me.grbCO2Level_use.Controls.Add(Me.txbCO2Level_Usage_PeriodPresence)
        Me.grbCO2Level_use.Location = New System.Drawing.Point(6, 153)
        Me.grbCO2Level_use.Name = "grbCO2Level_use"
        Me.grbCO2Level_use.Size = New System.Drawing.Size(336, 159)
        Me.grbCO2Level_use.TabIndex = 1
        Me.grbCO2Level_use.TabStop = False
        Me.grbCO2Level_use.Text = "Usage"
        '
        'lblCO2Level_Usage_Presence
        '
        Me.lblCO2Level_Usage_Presence.AutoSize = True
        Me.lblCO2Level_Usage_Presence.Location = New System.Drawing.Point(148, 17)
        Me.lblCO2Level_Usage_Presence.Name = "lblCO2Level_Usage_Presence"
        Me.lblCO2Level_Usage_Presence.Size = New System.Drawing.Size(52, 13)
        Me.lblCO2Level_Usage_Presence.TabIndex = 0
        Me.lblCO2Level_Usage_Presence.Text = "Presence"
        '
        'lblCO2Level_Usage_Break
        '
        Me.lblCO2Level_Usage_Break.AutoSize = True
        Me.lblCO2Level_Usage_Break.Location = New System.Drawing.Point(237, 17)
        Me.lblCO2Level_Usage_Break.Name = "lblCO2Level_Usage_Break"
        Me.lblCO2Level_Usage_Break.Size = New System.Drawing.Size(35, 13)
        Me.lblCO2Level_Usage_Break.TabIndex = 0
        Me.lblCO2Level_Usage_Break.Text = "Break"
        '
        'lblCO2Level_Usage_People
        '
        Me.lblCO2Level_Usage_People.AutoSize = True
        Me.lblCO2Level_Usage_People.Location = New System.Drawing.Point(43, 65)
        Me.lblCO2Level_Usage_People.Name = "lblCO2Level_Usage_People"
        Me.lblCO2Level_Usage_People.Size = New System.Drawing.Size(58, 13)
        Me.lblCO2Level_Usage_People.TabIndex = 0
        Me.lblCO2Level_Usage_People.Text = "People [nr]"
        '
        'lblCO2Level_Usage_CO2prod
        '
        Me.lblCO2Level_Usage_CO2prod.AutoSize = True
        Me.lblCO2Level_Usage_CO2prod.Location = New System.Drawing.Point(9, 132)
        Me.lblCO2Level_Usage_CO2prod.Name = "lblCO2Level_Usage_CO2prod"
        Me.lblCO2Level_Usage_CO2prod.Size = New System.Drawing.Size(145, 13)
        Me.lblCO2Level_Usage_CO2prod.TabIndex = 0
        Me.lblCO2Level_Usage_CO2prod.Text = "CO2 production [l/(h*person)]"
        '
        'lblCO2Level_Usage_Activity
        '
        Me.lblCO2Level_Usage_Activity.AutoSize = True
        Me.lblCO2Level_Usage_Activity.Location = New System.Drawing.Point(9, 106)
        Me.lblCO2Level_Usage_Activity.Name = "lblCO2Level_Usage_Activity"
        Me.lblCO2Level_Usage_Activity.Size = New System.Drawing.Size(107, 13)
        Me.lblCO2Level_Usage_Activity.TabIndex = 0
        Me.lblCO2Level_Usage_Activity.Text = "Level of activity [met]"
        Me.ToolTip1.SetToolTip(Me.lblCO2Level_Usage_Activity, resources.GetString("lblCO2Level_Usage_Activity.ToolTip"))
        '
        'lblCO2Level_Usage_Period
        '
        Me.lblCO2Level_Usage_Period.AutoSize = True
        Me.lblCO2Level_Usage_Period.Location = New System.Drawing.Point(43, 39)
        Me.lblCO2Level_Usage_Period.Name = "lblCO2Level_Usage_Period"
        Me.lblCO2Level_Usage_Period.Size = New System.Drawing.Size(62, 13)
        Me.lblCO2Level_Usage_Period.TabIndex = 0
        Me.lblCO2Level_Usage_Period.Text = "Period [min]"
        '
        'txbCO2Level_Usage_CO2prod
        '
        Me.txbCO2Level_Usage_CO2prod.Location = New System.Drawing.Point(183, 129)
        Me.txbCO2Level_Usage_CO2prod.Name = "txbCO2Level_Usage_CO2prod"
        Me.txbCO2Level_Usage_CO2prod.ReadOnly = True
        Me.txbCO2Level_Usage_CO2prod.Size = New System.Drawing.Size(108, 20)
        Me.txbCO2Level_Usage_CO2prod.TabIndex = 0
        '
        'txbCO2Level_Usage_Activity
        '
        Me.txbCO2Level_Usage_Activity.Location = New System.Drawing.Point(183, 103)
        Me.txbCO2Level_Usage_Activity.Name = "txbCO2Level_Usage_Activity"
        Me.txbCO2Level_Usage_Activity.Size = New System.Drawing.Size(108, 20)
        Me.txbCO2Level_Usage_Activity.TabIndex = 0
        Me.txbCO2Level_Usage_Activity.Text = "1.2"
        '
        'txbCO2Level_Usage_PeopleBreak
        '
        Me.txbCO2Level_Usage_PeopleBreak.Location = New System.Drawing.Point(217, 62)
        Me.txbCO2Level_Usage_PeopleBreak.Name = "txbCO2Level_Usage_PeopleBreak"
        Me.txbCO2Level_Usage_PeopleBreak.Size = New System.Drawing.Size(74, 20)
        Me.txbCO2Level_Usage_PeopleBreak.TabIndex = 0
        Me.txbCO2Level_Usage_PeopleBreak.Text = "0"
        '
        'txbCO2Level_Usage_PeoplePresence
        '
        Me.txbCO2Level_Usage_PeoplePresence.Location = New System.Drawing.Point(136, 62)
        Me.txbCO2Level_Usage_PeoplePresence.Name = "txbCO2Level_Usage_PeoplePresence"
        Me.txbCO2Level_Usage_PeoplePresence.Size = New System.Drawing.Size(74, 20)
        Me.txbCO2Level_Usage_PeoplePresence.TabIndex = 0
        Me.txbCO2Level_Usage_PeoplePresence.Text = "24"
        '
        'txbCO2Level_Usage_PeriodBreak
        '
        Me.txbCO2Level_Usage_PeriodBreak.Location = New System.Drawing.Point(217, 36)
        Me.txbCO2Level_Usage_PeriodBreak.Name = "txbCO2Level_Usage_PeriodBreak"
        Me.txbCO2Level_Usage_PeriodBreak.Size = New System.Drawing.Size(74, 20)
        Me.txbCO2Level_Usage_PeriodBreak.TabIndex = 0
        Me.txbCO2Level_Usage_PeriodBreak.Text = "15"
        '
        'txbCO2Level_Usage_PeriodPresence
        '
        Me.txbCO2Level_Usage_PeriodPresence.Location = New System.Drawing.Point(136, 36)
        Me.txbCO2Level_Usage_PeriodPresence.Name = "txbCO2Level_Usage_PeriodPresence"
        Me.txbCO2Level_Usage_PeriodPresence.Size = New System.Drawing.Size(74, 20)
        Me.txbCO2Level_Usage_PeriodPresence.TabIndex = 0
        Me.txbCO2Level_Usage_PeriodPresence.Text = "45"
        '
        'grbCO2Level_Room
        '
        Me.grbCO2Level_Room.Controls.Add(Me.PictureBox1)
        Me.grbCO2Level_Room.Controls.Add(Me.lblCO2Level_Room_Height)
        Me.grbCO2Level_Room.Controls.Add(Me.lblCO2Level_Room_Width)
        Me.grbCO2Level_Room.Controls.Add(Me.lblCO2Level_Room_Length)
        Me.grbCO2Level_Room.Controls.Add(Me.txbCO2Level_Room_Height)
        Me.grbCO2Level_Room.Controls.Add(Me.txbCO2Level_Room_Width)
        Me.grbCO2Level_Room.Controls.Add(Me.txbCO2Level_Room_Length)
        Me.grbCO2Level_Room.Location = New System.Drawing.Point(6, 6)
        Me.grbCO2Level_Room.Name = "grbCO2Level_Room"
        Me.grbCO2Level_Room.Size = New System.Drawing.Size(336, 141)
        Me.grbCO2Level_Room.TabIndex = 0
        Me.grbCO2Level_Room.TabStop = False
        Me.grbCO2Level_Room.Text = "Room"
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(127, 9)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(203, 126)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 2
        Me.PictureBox1.TabStop = False
        '
        'lblCO2Level_Room_Height
        '
        Me.lblCO2Level_Room_Height.AutoSize = True
        Me.lblCO2Level_Room_Height.Location = New System.Drawing.Point(6, 93)
        Me.lblCO2Level_Room_Height.Name = "lblCO2Level_Room_Height"
        Me.lblCO2Level_Room_Height.Size = New System.Drawing.Size(55, 13)
        Me.lblCO2Level_Room_Height.TabIndex = 1
        Me.lblCO2Level_Room_Height.Text = "Height [m]"
        '
        'lblCO2Level_Room_Width
        '
        Me.lblCO2Level_Room_Width.AutoSize = True
        Me.lblCO2Level_Room_Width.Location = New System.Drawing.Point(6, 60)
        Me.lblCO2Level_Room_Width.Name = "lblCO2Level_Room_Width"
        Me.lblCO2Level_Room_Width.Size = New System.Drawing.Size(52, 13)
        Me.lblCO2Level_Room_Width.TabIndex = 1
        Me.lblCO2Level_Room_Width.Text = "Width [m]"
        '
        'lblCO2Level_Room_Length
        '
        Me.lblCO2Level_Room_Length.AutoSize = True
        Me.lblCO2Level_Room_Length.Location = New System.Drawing.Point(6, 27)
        Me.lblCO2Level_Room_Length.Name = "lblCO2Level_Room_Length"
        Me.lblCO2Level_Room_Length.Size = New System.Drawing.Size(57, 13)
        Me.lblCO2Level_Room_Length.TabIndex = 1
        Me.lblCO2Level_Room_Length.Text = "Length [m]"
        '
        'txbCO2Level_Room_Height
        '
        Me.txbCO2Level_Room_Height.Location = New System.Drawing.Point(87, 90)
        Me.txbCO2Level_Room_Height.Name = "txbCO2Level_Room_Height"
        Me.txbCO2Level_Room_Height.Size = New System.Drawing.Size(34, 20)
        Me.txbCO2Level_Room_Height.TabIndex = 0
        Me.txbCO2Level_Room_Height.Text = "3.5"
        '
        'txbCO2Level_Room_Width
        '
        Me.txbCO2Level_Room_Width.Location = New System.Drawing.Point(87, 57)
        Me.txbCO2Level_Room_Width.Name = "txbCO2Level_Room_Width"
        Me.txbCO2Level_Room_Width.Size = New System.Drawing.Size(34, 20)
        Me.txbCO2Level_Room_Width.TabIndex = 0
        Me.txbCO2Level_Room_Width.Text = "7"
        '
        'txbCO2Level_Room_Length
        '
        Me.txbCO2Level_Room_Length.Location = New System.Drawing.Point(87, 24)
        Me.txbCO2Level_Room_Length.Name = "txbCO2Level_Room_Length"
        Me.txbCO2Level_Room_Length.Size = New System.Drawing.Size(34, 20)
        Me.txbCO2Level_Room_Length.TabIndex = 0
        Me.txbCO2Level_Room_Length.Text = "8"
        '
        'pnlMain
        '
        Me.pnlMain.Controls.Add(Me.tbcMain)
        Me.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMain.Location = New System.Drawing.Point(0, 24)
        Me.pnlMain.Name = "pnlMain"
        Me.pnlMain.Padding = New System.Windows.Forms.Padding(5)
        Me.pnlMain.Size = New System.Drawing.Size(1130, 643)
        Me.pnlMain.TabIndex = 1
        '
        'sfdSavePdf
        '
        Me.sfdSavePdf.DefaultExt = "*.pdf"
        Me.sfdSavePdf.Filter = "PDF file|*.pdf"
        '
        'ToolTip1
        '
        Me.ToolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        '
        'CLMainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1130, 667)
        Me.Controls.Add(Me.pnlMain)
        Me.Controls.Add(Me.mnsMain)
        Me.MainMenuStrip = Me.mnsMain
        Me.Name = "CLMainForm"
        Me.Text = "CLRC Selection Software"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.mnsMain.ResumeLayout(False)
        Me.mnsMain.PerformLayout()
        Me.tbpPerformance.ResumeLayout(False)
        Me.spcPerformance.Panel1.ResumeLayout(False)
        Me.spcPerformance.Panel2.ResumeLayout(False)
        CType(Me.spcPerformance, System.ComponentModel.ISupportInitialize).EndInit()
        Me.spcPerformance.ResumeLayout(False)
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        Me.SplitContainer2.Panel2.PerformLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.pnlPerformance_Data.ResumeLayout(False)
        Me.grbPerformance_TemperatureConditions.ResumeLayout(False)
        Me.grbPerformance_TemperatureConditions.PerformLayout()
        Me.grbPerformance_UnitSelection.ResumeLayout(False)
        Me.grbPerformance_UnitSelection.PerformLayout()
        Me.Panel4.ResumeLayout(False)
        Me.tbcData.ResumeLayout(False)
        Me.tbpData_Thermal.ResumeLayout(False)
        Me.grbPerformance_HeatExchangerPerformances.ResumeLayout(False)
        Me.grbPerformance_HeatExchangerPerformances.PerformLayout()
        Me.grbPerformance_TemperatureConditions2.ResumeLayout(False)
        Me.grbPerformance_TemperatureConditions2.PerformLayout()
        Me.tbpData_ElectricalPerformances.ResumeLayout(False)
        Me.tbpData_ElectricalPerformances.PerformLayout()
        Me.grbPerformance_ERP2018.ResumeLayout(False)
        Me.grbPerformance_SEL.ResumeLayout(False)
        Me.grbPerformance_SEL.PerformLayout()
        CType(Me.nudPerformance_SEL_Limit, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grbPerformance_PassiveHaus.ResumeLayout(False)
        Me.grbPerformance_PassiveHaus.PerformLayout()
        Me.grbPerformance_SFP.ResumeLayout(False)
        Me.grbPerformance_SFP.PerformLayout()
        CType(Me.nudPerformance_SFP_Limit, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tbpData_SoundPower.ResumeLayout(False)
        CType(Me.dgvPerformance_SoundPower, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudSoundPerformances_LP2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudSoundPerformances_LP1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tbpData_ItemGenerator.ResumeLayout(False)
        Me.tbpData_ItemGenerator.PerformLayout()
        Me.grbPerformance_IGClimaAcc.ResumeLayout(False)
        Me.grbPerformance_IGClimaAcc.PerformLayout()
        Me.grbPerformance_IGComm.ResumeLayout(False)
        Me.grbPerformance_IGComm.PerformLayout()
        Me.grbPerformance_IGSensor.ResumeLayout(False)
        Me.grbPerformance_IGSensor.PerformLayout()
        Me.tbpData_ItemGenerator_QTM.ResumeLayout(False)
        Me.tbpData_ItemGenerator_QTM.PerformLayout()
        Me.grbPerformance_IGHeaterQTM.ResumeLayout(False)
        Me.grbPerformance_IGHeaterQTM.PerformLayout()
        Me.grbPerformance_IGVersionQTM.ResumeLayout(False)
        Me.grbPerformance_IGVersionQTM.PerformLayout()
        Me.grbPerformance_IGLayoutQTM.ResumeLayout(False)
        Me.grbPerformance_IGLayoutQTM.PerformLayout()
        Me.grbPerformance_IGSensorQTM.ResumeLayout(False)
        Me.grbPerformance_IGSensorQTM.PerformLayout()
        Me.grbPerformance_IGCommQTM.ResumeLayout(False)
        Me.grbPerformance_IGCommQTM.PerformLayout()
        Me.pnlRegulationLevel.ResumeLayout(False)
        Me.pnlRegulationLevel.PerformLayout()
        Me.tlpPerformance_Graphs.ResumeLayout(False)
        CType(Me.crtPerformance_Chart1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.crtPerformance_Chart2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.crtPerformance_Chart3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tbcMain.ResumeLayout(False)
        Me.tbpCertification.ResumeLayout(False)
        Me.tbpCertification.PerformLayout()
        CType(Me.dgvSAP, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tbpAccessory.ResumeLayout(False)
        Me.grbAccessory_OutputData.ResumeLayout(False)
        Me.grbAccessory_OutputData.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.bsrUnitCalculator, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.grbAccessory_EHD.ResumeLayout(False)
        Me.grbAccessory_EHD.PerformLayout()
        Me.grbAccessory_HWD.ResumeLayout(False)
        Me.grbAccessory_HWD.PerformLayout()
        CType(Me.nudAccessory_HWD_FluidTypeTec, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grbAccessory_CWD.ResumeLayout(False)
        Me.grbAccessory_CWD.PerformLayout()
        CType(Me.nudAccessory_CWD_FluidTypeTec, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grbAccessory_InputData.ResumeLayout(False)
        Me.grbAccessory_InputData.PerformLayout()
        Me.tbpCO2Level.ResumeLayout(False)
        Me.tbpCO2Level.PerformLayout()
        CType(Me.crtCO2Level_Chart1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grbCO2Level_Parameters.ResumeLayout(False)
        Me.grbCO2Level_Parameters.PerformLayout()
        Me.grbCO2Level_use.ResumeLayout(False)
        Me.grbCO2Level_use.PerformLayout()
        Me.grbCO2Level_Room.ResumeLayout(False)
        Me.grbCO2Level_Room.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlMain.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub


    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
    Friend WithEvents mnsMain As System.Windows.Forms.MenuStrip
    Friend WithEvents tsmiFile As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiFile_GenerateReport As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiFile_Exit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Language As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Language_EN As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Language_DE As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Language_FR As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Language_IT As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiAbout As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tbpPerformance As System.Windows.Forms.TabPage
    Friend WithEvents grbPerformance_TemperatureConditions2 As System.Windows.Forms.GroupBox
    Friend WithEvents lblPerformance_SupplyOutletTemperature As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_SupplyOutletRH As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_SupplyOutletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_ExhaustOutletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_ExhaustOutletTemperature As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_SupplyOutletRH As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_ExhaustOutletRH As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_RHReturnInlet As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_ReturnInletTemperature As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_RHFreshInlet As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_FreshInletTemperature As System.Windows.Forms.Label
    Friend WithEvents grbPerformance_TemperatureConditions As System.Windows.Forms.GroupBox
    Friend WithEvents txbPerformance_RHReturnInlet As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_ReturnInletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_RHFreshInlet As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_FreshInletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents grbPerformance_UnitSelection As System.Windows.Forms.GroupBox
    Friend WithEvents cmbPerformance_HeatRecoveryModels As System.Windows.Forms.ComboBox
    Friend WithEvents lblPerformance_Unit As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_AirFlow As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_AirFlow As System.Windows.Forms.TextBox
    Friend WithEvents grbPerformance_HeatExchangerPerformances As System.Windows.Forms.GroupBox
    Friend WithEvents lblPerformance_LatentHeat As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_LatentHeat As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_SensibleHeat As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_SensibleHeat As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_HeatTransferred As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_HeatTransferred As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_Efficiency As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_Efficiency As System.Windows.Forms.Label
    Friend WithEvents crtPerformance_Chart3 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents crtPerformance_Chart2 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents crtPerformance_Chart1 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents tbcMain As System.Windows.Forms.TabControl
    Friend WithEvents lblPerformance_WaterProduced As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_WaterProduced As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_RegulationLevelValue As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_RegulationLevel As System.Windows.Forms.Label
    Friend WithEvents prbPerformance_RegulationLevel As System.Windows.Forms.ProgressBar
    Friend WithEvents hsbPerformance_RegulationLevel As System.Windows.Forms.HScrollBar
    Friend WithEvents lblPerformance_MaxPressure As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_MaxPressure As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_PassiveHaus As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_SFP As System.Windows.Forms.TextBox
    Friend WithEvents txbPerformance_ElectricalPerformances_PowerInput As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_PassiveHaus_ElectricalEfficiency As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_SFP_SFP As System.Windows.Forms.Label
    Friend WithEvents lblPerformance_ElectricalPerformances_PowerInput As System.Windows.Forms.Label
    Friend WithEvents tsmiOption_Unit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Unit_SI As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Unit_IP As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents sfdSaveFile As System.Windows.Forms.SaveFileDialog
    Friend WithEvents tbpCertification As System.Windows.Forms.TabPage
    Friend WithEvents dgvSAP As System.Windows.Forms.DataGridView
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents btnUpdateSAPTable As System.Windows.Forms.Button
    Friend WithEvents dgvSAP_ExhaustTerminalConfiguration As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvSAP_TotalExhaustFlowRate As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvSAP_TotalSupplyFlowRate As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvSAP_RegulationLevel As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvSAP_SpecificFanPower As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvSAP_HeatExchangeEfficiency As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dgvSAP_EnergySavingTrustBestPracticePerformanceCompliant As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents pnlMain As System.Windows.Forms.Panel
    Friend WithEvents chbPerformance_SFP_ShowArea As System.Windows.Forms.CheckBox
    Friend WithEvents chbPerformance_PassiveHaus_ShowArea As System.Windows.Forms.CheckBox
    Friend WithEvents nudPerformance_SFP_Limit As System.Windows.Forms.NumericUpDown
    Friend WithEvents grbPerformance_PassiveHaus As System.Windows.Forms.GroupBox
    Friend WithEvents grbPerformance_SFP As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_PassiveHaus_Limit As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents tsmiOption_Language_NL As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tlpPerformance_Graphs As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents pnlPerformance_Data As System.Windows.Forms.Panel
    Friend WithEvents dgvPerformance_SoundPower As System.Windows.Forms.DataGridView
    Friend WithEvents cmbPerformance_Series As System.Windows.Forms.ComboBox
    Friend WithEvents lblPerformance_Series As System.Windows.Forms.Label
    Friend WithEvents sfdSavePdf As System.Windows.Forms.SaveFileDialog
    Friend WithEvents lblPerformance_ExhaustOutletRH As System.Windows.Forms.Label
    Friend WithEvents tbpAccessory As System.Windows.Forms.TabPage
    Friend WithEvents bsrUnitCalculator As System.Windows.Forms.BindingSource
    Friend WithEvents grbAccessory_OutputData As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txbAccessory_OutputData_CWDSensibleHeat As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txbAccessory_OutputData_CWDHeatTransferred As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txbAccessory_OutputData_HWDHeatTransferred As System.Windows.Forms.TextBox
    Friend WithEvents lblAccessory_Status As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents txbAccessory_OutputData_Temp As System.Windows.Forms.TextBox
    Friend WithEvents txbAccessory_OutputData_Condensation As System.Windows.Forms.TextBox
    Friend WithEvents txbAccessory_OutputData_RH As System.Windows.Forms.TextBox
    Friend WithEvents txbAccessory_OutputData_PressureDrop As System.Windows.Forms.TextBox
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents grbAccessory_EHD As System.Windows.Forms.GroupBox
    Friend WithEvents cbxAccessory_EHDEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents grbAccessory_HWD As System.Windows.Forms.GroupBox
    Friend WithEvents nudAccessory_HWD_FluidTypeTec As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblAccessory_HWD_OutletTemperature As System.Windows.Forms.Label
    Friend WithEvents cmbAccessory_HWD_FluidType As System.Windows.Forms.ComboBox
    Friend WithEvents lblAccessory_HWD_FluidTypeTec As System.Windows.Forms.Label
    Friend WithEvents lblAccessory_HWD_FluidType As System.Windows.Forms.Label
    Friend WithEvents lblAccessory_HWD_InletTemperature As System.Windows.Forms.Label
    Friend WithEvents txbAccessory_HWD_InletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents txbAccessory_HWD_OutletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents cbxAccessory_HWDEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents grbAccessory_CWD As System.Windows.Forms.GroupBox
    Friend WithEvents nudAccessory_CWD_FluidTypeTec As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblAccessory_CWD_OutletTemperature As System.Windows.Forms.Label
    Friend WithEvents cbxAccessory_CWDEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents cmbAccessory_CWD_FluidType As System.Windows.Forms.ComboBox
    Friend WithEvents lblAccessory_CWD_FluidTypeTec As System.Windows.Forms.Label
    Friend WithEvents lblAccessory_CWD_FluidType As System.Windows.Forms.Label
    Friend WithEvents lblAccessory_CWD_InletTemperature As System.Windows.Forms.Label
    Friend WithEvents txbAccessory_CWD_InletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents txbAccessory_CWD_OutletTemperature As System.Windows.Forms.TextBox
    Friend WithEvents grbAccessory_InputData As System.Windows.Forms.GroupBox
    Friend WithEvents lblAccessory_InputData_Temp As System.Windows.Forms.Label
    Friend WithEvents lblAccessory_InputData_RHTemp As System.Windows.Forms.Label
    Friend WithEvents txbAccessory_InputData_Temp As System.Windows.Forms.TextBox
    Friend WithEvents txbAccessory_InputData_RH As System.Windows.Forms.TextBox
    Friend WithEvents txbAccessory_InputData_AirFlow As System.Windows.Forms.TextBox
    Friend WithEvents lblAccessory_InputData_AirFlow As System.Windows.Forms.Label
    Friend WithEvents tsmiOption_Language_PL As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Language_SL As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents grbPerformance_SEL As System.Windows.Forms.GroupBox
    Friend WithEvents nudPerformance_SEL_Limit As System.Windows.Forms.NumericUpDown
    Friend WithEvents txbPerformance_SEL As System.Windows.Forms.TextBox
    Friend WithEvents lblPerformance_SEL_SEL As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents chbPerformance_SEL_ShowArea As System.Windows.Forms.CheckBox
    Friend WithEvents spcPerformance As System.Windows.Forms.SplitContainer
    Friend WithEvents nudSoundPerformances_LP1 As System.Windows.Forms.NumericUpDown
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents nudSoundPerformances_LP2 As System.Windows.Forms.NumericUpDown
    Friend WithEvents tbcData As System.Windows.Forms.TabControl
    Friend WithEvents tbpData_Thermal As System.Windows.Forms.TabPage
    Friend WithEvents tbpData_ElectricalPerformances As System.Windows.Forms.TabPage
    Friend WithEvents tbpData_SoundPower As System.Windows.Forms.TabPage
    Friend WithEvents pnlRegulationLevel As System.Windows.Forms.Panel
    Friend WithEvents Panel4 As System.Windows.Forms.Panel
    Friend WithEvents tsmiFile_SaveCommercialSheet As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents SplitContainer2 As System.Windows.Forms.SplitContainer
    Friend WithEvents m_Note_Text As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents tsmiOption_Language_BG As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiOption_Language_RO As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiFile_SaveIOM As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tbpCO2Level As System.Windows.Forms.TabPage
    Friend WithEvents grbCO2Level_Parameters As System.Windows.Forms.GroupBox
    Friend WithEvents grbCO2Level_use As System.Windows.Forms.GroupBox
    Friend WithEvents grbCO2Level_Room As System.Windows.Forms.GroupBox
    Friend WithEvents lblCO2Level_Room_Height As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Room_Width As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Room_Length As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Room_Height As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Room_Width As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Room_Length As System.Windows.Forms.TextBox
    Friend WithEvents crtCO2Level_Chart1 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents lblCO2Level_Usage_Presence As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Usage_Break As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Usage_People As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Usage_CO2prod As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Usage_Activity As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Usage_Period As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Usage_CO2prod As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Usage_Activity As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Usage_PeopleBreak As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Usage_PeoplePresence As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Usage_PeriodBreak As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Usage_PeriodPresence As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents lblCO2Level_Parameters_CalcMet As System.Windows.Forms.Label
    Friend WithEvents cmbCO2Level_Parameters_CalcMet As System.Windows.Forms.ComboBox
    Friend WithEvents lblCO2Level_Parameters_airflow As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Parameters_airflow As System.Windows.Forms.TextBox
    Friend WithEvents lblCO2Level_Parameters_maxCO2 As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Parameters_maxCO2 As System.Windows.Forms.TextBox
    Friend WithEvents lblCO2Level_Parameters_extCO2 As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Parameters_extCO2 As System.Windows.Forms.TextBox
    Friend WithEvents lblCO2Level_Parameters_stdpreset As System.Windows.Forms.Label
    Friend WithEvents cmbCO2Level_Parameters_stdpreset As System.Windows.Forms.ComboBox
    Friend WithEvents lblCO2Level_Parameters_af_person As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Parameters_af_person As System.Windows.Forms.TextBox
    Friend WithEvents lblCO2Level_Parameters_af_area As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Parameters_af_area As System.Windows.Forms.TextBox
    Friend WithEvents lblCO2Level_Parameters_af_area_m3h As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Parameters_af_person_m3h As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Parameters_airflow_m3h As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Parameters_af_area_m3h As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Parameters_af_person_m3h As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Parameters_airflow_m3h As System.Windows.Forms.TextBox
    Friend WithEvents chbCO2Level_addtoreport As System.Windows.Forms.CheckBox
    Friend WithEvents chbCO2Level_Parameters_airflow_check As System.Windows.Forms.CheckBox
    Friend WithEvents lblCO2Level_Parameters_airflow_design_m3h As System.Windows.Forms.Label
    Friend WithEvents lblCO2Level_Parameters_airflow_design As System.Windows.Forms.Label
    Friend WithEvents txbCO2Level_Parameters_airflow_design_m3h As System.Windows.Forms.TextBox
    Friend WithEvents txbCO2Level_Parameters_airflow_design As System.Windows.Forms.TextBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents tsmiOption_Language_HU As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tsmiBranchs As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents chbSoundPerformances_addtoreport As System.Windows.Forms.CheckBox
    Friend WithEvents tbpData_ItemGenerator As System.Windows.Forms.TabPage
    Friend WithEvents grbPerformance_IGClimaAcc As System.Windows.Forms.GroupBox
    Friend WithEvents chbPerformance_IPEHD As System.Windows.Forms.CheckBox
    Friend WithEvents rdbPerformance_ICWD As System.Windows.Forms.RadioButton
    Friend WithEvents rdbPerformance_IDXD As System.Windows.Forms.RadioButton
    Friend WithEvents rdbPerformance_IEHD As System.Windows.Forms.RadioButton
    Friend WithEvents rdbPerformance_IHCD As System.Windows.Forms.RadioButton
    Friend WithEvents rdbPerformance_IHWD As System.Windows.Forms.RadioButton
    Friend WithEvents grbPerformance_IGComm As System.Windows.Forms.GroupBox
    Friend WithEvents chbPerformance_RFM As System.Windows.Forms.CheckBox
    Friend WithEvents chbPerformance_MBUS As System.Windows.Forms.CheckBox
    Friend WithEvents grbPerformance_IGSensor As System.Windows.Forms.GroupBox
    Friend WithEvents rdbPerformance_RH As System.Windows.Forms.RadioButton
    Friend WithEvents rdbPerformance_VOC As System.Windows.Forms.RadioButton
    Friend WithEvents rdbPerformance_CO2 As System.Windows.Forms.RadioButton
    Friend WithEvents lblPerformance_ItemCode As System.Windows.Forms.Label
    Friend WithEvents txbPerformance_ItemCode As System.Windows.Forms.TextBox
    Friend WithEvents rdbPerformance_none As System.Windows.Forms.RadioButton
    Friend WithEvents lblPerformance_ItemDescr As System.Windows.Forms.Label
    Friend WithEvents rdb_Q8 As System.Windows.Forms.RadioButton
    Friend WithEvents rdb_Q4 As System.Windows.Forms.RadioButton
    Friend WithEvents rdb_Q2 As System.Windows.Forms.RadioButton
    Friend WithEvents PictureBox4 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox3 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents grbPerformance_ERP2018 As System.Windows.Forms.GroupBox
    Friend WithEvents chbPerformance_ERP2018_ShowArea As System.Windows.Forms.CheckBox
    Friend WithEvents tbpData_ItemGenerator_QTM As TabPage
    Friend WithEvents grbPerformance_IGHeaterQTM As GroupBox
    Friend WithEvents chb_qtm_IPEHD As CheckBox
    Friend WithEvents grbPerformance_IGVersionQTM As GroupBox
    Friend WithEvents rdb_qtm_preplus As RadioButton
    Friend WithEvents rdb_qtm_premium As RadioButton
    Friend WithEvents grbPerformance_IGLayoutQTM As GroupBox
    Friend WithEvents rdb_qtm_eos As RadioButton
    Friend WithEvents rdb_qtm_osc As RadioButton
    Friend WithEvents rdb_qtm_fos As RadioButton
    Friend WithEvents rdb_qtm_ssc As RadioButton
    Friend WithEvents lblPerformance_ItemDescrQTM As Label
    Friend WithEvents lblPerformance_ItemCodeQTM As Label
    Friend WithEvents txbPerformance_ItemCodeQTM As TextBox
    Friend WithEvents grbPerformance_IGSensorQTM As GroupBox
    Friend WithEvents rdb_qtm_rh As RadioButton
    Friend WithEvents rdb_qtm_voc As RadioButton
    Friend WithEvents rdb_qtm_co2 As RadioButton
    Friend WithEvents grbPerformance_IGCommQTM As GroupBox
    Friend WithEvents chb_qtm_RFM As CheckBox
    Friend WithEvents chb_qtm_KTS As CheckBox
    Friend WithEvents chb_qtm_MBUS As CheckBox
    Friend WithEvents rdb_qtm_sensnone As RadioButton
    Friend WithEvents chbSoundPerformances_16032 As CheckBox
    Friend WithEvents rdb_qtm_easy As RadioButton
    Friend WithEvents chb_qtm_ACTIVA As CheckBox
    Friend WithEvents tsmiOption_Language_DA As ToolStripMenuItem
    Friend WithEvents btnEN308 As Button
    Friend WithEvents btn_Default As Button
End Class
