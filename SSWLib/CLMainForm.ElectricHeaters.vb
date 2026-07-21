Imports System.Globalization
Imports System.Reflection
Imports System.Data
Imports Climalombarda.Common.UI
Imports Climalombarda.DataCentral

Partial Public Class CLMainForm
    Private Const ElectricHeaterCustomSelectionEnabled As Boolean = False
    Private m_ElectricHeaterChanging As Boolean
    Private m_ElectricHeaterModelId As Integer = -1
    Private m_ElectricHeaters As New List(Of CLElectricHeaterDefinition)()
    Private m_ElectricHeaterLastPressureDrop As Double
    Private ReadOnly m_ElectricHeaterLastResults As New List(Of CLElectricHeaterCalculationResult)()
    Private tbpData_ElectricHeaters As TabPage
    Private ReadOnly m_ElectricModeControls As New Dictionary(Of CLElectricHeaterMode, CLElectricModeControls)()

    Private NotInheritable Class CLElectricModeControls
        Public Mode As CLElectricHeaterMode
        Public Group As GroupBox
        Public Enable As CheckBox
        Public EditMode As ComboBox
        Public Installation As ComboBox
        Public Heater As ComboBox
        Public Conflict As Label
        Public FrostStatus As Label
        Public CustomNote As Label
        Public Power As TextBox
        Public Voltage As TextBox
        Public Phases As TextBox
        Public Current As TextBox
        Public AirIn As TextBox
        Public AirOut As TextBox
        Public AirOutRH As TextBox
        Public PressureDrop As TextBox
        Public ExhaustOut As TextBox
        Public CustomDisclaimerAccepted As Boolean
    End Class

    Private NotInheritable Class CLElectricChoice(Of T)
        Public Sub New(text As String, value As T)
            Me.Text = text
            Me.Value = value
        End Sub
        Public Property Text As String
        Public Property Value As T
        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Private Sub ElectricHeater_InitializeTab()
        If tbpData_ElectricHeaters IsNot Nothing Then Return
        tbpData_ElectricHeaters = New TabPage(CoilPerformance_Text("MainForm_ElectricHeater_Tab", "Electric heaters")) With {
            .AutoScroll = True,
            .UseVisualStyleBackColor = True
        }
        m_ElectricModeControls(CLElectricHeaterMode.PEHD) = ElectricHeater_CreateModeGroup(CLElectricHeaterMode.PEHD, 8)
        m_ElectricModeControls(CLElectricHeaterMode.EHD) = ElectricHeater_CreateModeGroup(CLElectricHeaterMode.EHD, 552)
        For Each modeControls As CLElectricModeControls In m_ElectricModeControls.Values
            tbpData_ElectricHeaters.Controls.Add(modeControls.Group)
        Next
        tbcData.TabPages.Insert(Math.Min(2, tbcData.TabPages.Count), tbpData_ElectricHeaters)
        ElectricHeater_FillAvailable()
    End Sub

    Private Function ElectricHeater_CreateModeGroup(mode As CLElectricHeaterMode, x As Integer) As CLElectricModeControls
        Dim result As New CLElectricModeControls With {.Mode = mode}
        result.Group = New GroupBox With {
            .Location = New Point(x, 8),
            .Size = New Size(536, 360),
            .Text = mode.ToString()
        }

        result.Enable = New CheckBox With {.Location = New Point(14, 24), .AutoSize = True}
        ElectricHeater_Tag(result.Enable, "MainForm_ElectricHeater_Enable", "Enable calculation")
        AddHandler result.Enable.CheckedChanged, Sub(sender, args) ElectricHeater_EnableChanged(result)

        result.EditMode = ElectricHeater_CreateCombo(150, 52, 358)
        AddHandler result.EditMode.SelectedIndexChanged, Sub(sender, args) ElectricHeater_EditModeChanged(result)
        result.Installation = ElectricHeater_CreateCombo(150, 80, 358)
        AddHandler result.Installation.SelectedIndexChanged, Sub(sender, args) ElectricHeater_InstallationChanged(result)
        result.Heater = ElectricHeater_CreateCombo(150, 108, 358)
        AddHandler result.Heater.SelectedIndexChanged, Sub(sender, args) ElectricHeater_SelectedChanged(result)

        result.Group.Controls.Add(result.Enable)
        ElectricHeater_AddLabel(result.Group, "MainForm_CoilPerformance_Case", "Case", 14, 55)
        ElectricHeater_AddLabel(result.Group, "MainForm_CoilPerformance_Installation", "Installation", 14, 83)
        ElectricHeater_AddLabel(result.Group, "MainForm_ElectricHeater_Heater", "Electric heater", 14, 111)
        result.Group.Controls.Add(result.EditMode)
        result.Group.Controls.Add(result.Installation)
        result.Group.Controls.Add(result.Heater)

        result.Power = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_Power", "Power [W]", 14, 148)
        result.Voltage = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_Voltage", "Voltage [V]", 248, 148, 160)
        result.Phases = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_Phases", "Phases", 14, 176)
        result.Current = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_Current", "Current [A]", 248, 176, 160)
        result.AirIn = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_AirIn", "Air inlet [C]", 14, 204)
        result.AirOut = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_AirOut", "Max. air out temp. [°C]", 248, 204, 160)
        result.PressureDrop = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_PressureDrop", "Air DP [Pa]", 14, 232)
        result.AirOutRH = ElectricHeater_AddOutput(result.Group, "MainForm_CoilPerformance_ResultRHOut", "R.H. out [%]", 248, 232, 160)
        If mode = CLElectricHeaterMode.PEHD Then
            result.ExhaustOut = ElectricHeater_AddOutput(result.Group, "MainForm_ElectricHeater_ExhaustOut", "Exhaust outlet [C]", 14, 260, 136, True)
        End If

        result.Conflict = New Label With {.Location = New Point(14, 293), .Size = New Size(500, 28), .ForeColor = Color.Firebrick, .Font = New Font(result.Group.Font, FontStyle.Bold)}
        ElectricHeater_Tag(result.Conflict, "MainForm_ElectricHeater_WaterConflict", "Deselect the heating water coil to enable the electric post-heater.")
        result.Group.Controls.Add(result.Conflict)
        If mode = CLElectricHeaterMode.PEHD Then
            result.FrostStatus = New Label With {.Location = New Point(14, 293), .Size = New Size(500, 28), .ForeColor = Color.Firebrick, .Font = New Font(result.Group.Font, FontStyle.Bold), .Visible = False}
            ElectricHeater_Tag(result.FrostStatus, "MainForm_ElectricHeater_FrostWarning", "Exhaust temperature is not above the 3 C frost-protection target.")
            result.Group.Controls.Add(result.FrostStatus)
        End If
        result.CustomNote = New Label With {.Location = New Point(14, 322), .Size = New Size(500, 28), .ForeColor = Color.Firebrick, .Font = New Font(result.Group.Font, FontStyle.Bold)}
        ElectricHeater_Tag(result.CustomNote, "MainForm_ElectricHeater_CustomPending", "Custom electric-heater parameters will be defined in a later step.")
        result.Group.Controls.Add(result.CustomNote)
        Return result
    End Function

    Private Shared Function ElectricHeater_CreateCombo(x As Integer, y As Integer, width As Integer) As ComboBox
        Return New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(x, y), .Size = New Size(width, 21)}
    End Function

    Private Sub ElectricHeater_AddLabel(parent As Control,
        key As String,
        fallback As String,
        x As Integer,
        y As Integer,
        Optional width As Integer = 0,
        Optional height As Integer = 0)

        Dim label As New Label With {.AutoSize = width <= 0, .Location = New Point(x, y)}
        If width > 0 Then
            label.Size = New Size(width, height)
            label.TextAlign = ContentAlignment.MiddleLeft
        End If
        ElectricHeater_Tag(label, key, fallback)
        parent.Controls.Add(label)
    End Sub

    Private Function ElectricHeater_AddOutput(parent As Control,
        key As String,
        fallback As String,
        x As Integer,
        y As Integer,
        Optional valueOffset As Integer = 136,
        Optional wrapLabel As Boolean = False) As TextBox

        If wrapLabel Then
            ElectricHeater_AddLabel(parent, key, fallback, x, y - 4, valueOffset - 8, 32)
        Else
            ElectricHeater_AddLabel(parent, key, fallback, x, y + 3)
        End If
        Dim output As New TextBox With {.Location = New Point(x + valueOffset, y), .Size = New Size(92, 20), .ReadOnly = True}
        parent.Controls.Add(output)
        Return output
    End Function

    Private Sub ElectricHeater_Tag(control As Control, key As String, fallback As String)
        control.Tag = New String() {key, fallback}
        control.Text = CoilPerformance_Text(key, fallback)
    End Sub

    Private Sub ElectricHeater_FillAvailable()
        If tbpData_ElectricHeaters Is Nothing Then Return
        Dim currentModelId As Integer = If(SelectedHeatRecoveryModel Is Nothing, -1, SelectedHeatRecoveryModel.Id)
        Dim modelChanged As Boolean = currentModelId <> m_ElectricHeaterModelId
        m_ElectricHeaterModelId = currentModelId
        m_ElectricHeaters = CLElectricHeaterCalculator.GetAvailableHeaters(SelectedHeatRecoveryModel)
        For Each modeControls As CLElectricModeControls In m_ElectricModeControls.Values
            ElectricHeater_FillMode(modeControls, modelChanged)
        Next
        tbpData_ElectricHeaters.Enabled = m_ElectricHeaters.Count > 0
        ElectricHeater_UpdateControlState()
    End Sub

    Private Sub ElectricHeater_FillMode(controls As CLElectricModeControls, modelChanged As Boolean)
        Dim previous As CLElectricHeaterDefinition = TryCast(controls.Heater.SelectedItem, CLElectricHeaterDefinition)
        Dim wasCustomized As Boolean = ElectricHeater_IsCustomized(controls)
        Dim preferredInstallation As CLCoilInstallationType = ElectricHeater_SelectedInstallation(controls)
        If modelChanged Then
            previous = Nothing
            wasCustomized = False
            preferredInstallation = CLCoilInstallationType.Internal
            controls.Enable.Checked = False
        End If
        Try
            m_ElectricHeaterChanging = True
            Dim available = m_ElectricHeaters.Where(Function(item) item.Mode = controls.Mode).ToList()
            controls.EditMode.Items.Clear()
            controls.EditMode.Items.Add(New CLElectricChoice(Of CLCoilPerformanceEditMode)(CoilPerformance_Text("MainForm_CoilPerformance_Standard", "Standard"), CLCoilPerformanceEditMode.Standard))
            controls.EditMode.Items.Add(New CLElectricChoice(Of CLCoilPerformanceEditMode)(CoilPerformance_Text("MainForm_CoilPerformance_StandardCustomized", "Customized"), CLCoilPerformanceEditMode.StandardCustomized))
            controls.EditMode.SelectedIndex = If(ElectricHeaterCustomSelectionEnabled AndAlso wasCustomized, 1, 0)

            controls.Installation.Items.Clear()
            Dim hasInternal = available.Any(Function(item) item.Installation = CLCoilInstallationType.Internal)
            Dim hasExternal = available.Any(Function(item) item.Installation = CLCoilInstallationType.External)
            If hasInternal Then
                controls.Installation.Items.Add(New CLElectricChoice(Of CLCoilInstallationType)(CoilPerformance_Text("MainForm_CoilPerformance_Internal", "Internal"), CLCoilInstallationType.Internal))
                If hasExternal Then controls.Installation.Items.Add(New CLElectricChoice(Of CLCoilInstallationType)(CoilPerformance_Text("MainForm_CoilPerformance_ExternalInstallation", "External"), CLCoilInstallationType.External))
            ElseIf hasExternal Then
                controls.Installation.Items.Add(New CLElectricChoice(Of CLCoilInstallationType)(CoilPerformance_Text("MainForm_CoilPerformance_ExternalInstallation", "External"), CLCoilInstallationType.External))
                controls.Installation.Items.Add(New CLElectricChoice(Of CLCoilInstallationType)(CoilPerformance_Text("MainForm_CoilPerformance_RequestInternal", "Request internal"), CLCoilInstallationType.RequestedInternal))
            End If
            ElectricHeater_SelectInstallation(controls, preferredInstallation)
            ElectricHeater_FillHeaterCombo(controls, previous)
            If available.Count = 0 Then controls.Enable.Checked = False
        Finally
            m_ElectricHeaterChanging = False
        End Try
        ElectricHeater_LoadSelected(controls)
    End Sub

    Private Sub ElectricHeater_FillHeaterCombo(controls As CLElectricModeControls, previous As CLElectricHeaterDefinition)
        controls.Heater.Items.Clear()
        Dim source = ElectricHeater_SourceInstallation(controls)
        For Each heater In m_ElectricHeaters.Where(Function(item) item.Mode = controls.Mode AndAlso item.Installation = source)
            controls.Heater.Items.Add(heater.Clone())
        Next
        If controls.Heater.Items.Count = 0 Then Return
        Dim selectedIndex As Integer = 0
        If previous IsNot Nothing Then
            For i = 0 To controls.Heater.Items.Count - 1
                Dim candidate = DirectCast(controls.Heater.Items(i), CLElectricHeaterDefinition)
                If candidate.Id = previous.Id Then selectedIndex = i : Exit For
            Next
        End If
        controls.Heater.SelectedIndex = selectedIndex
    End Sub

    Private Sub ElectricHeater_SelectInstallation(controls As CLElectricModeControls, value As CLCoilInstallationType)
        Dim selected As Integer = -1
        For i = 0 To controls.Installation.Items.Count - 1
            Dim item = TryCast(controls.Installation.Items(i), CLElectricChoice(Of CLCoilInstallationType))
            If item IsNot Nothing AndAlso item.Value = value Then selected = i : Exit For
        Next
        controls.Installation.SelectedIndex = If(selected >= 0, selected, If(controls.Installation.Items.Count > 0, 0, -1))
    End Sub

    Private Function ElectricHeater_SelectedInstallation(controls As CLElectricModeControls) As CLCoilInstallationType
        Dim item = TryCast(controls.Installation.SelectedItem, CLElectricChoice(Of CLCoilInstallationType))
        Return If(item Is Nothing, CLCoilInstallationType.Internal, item.Value)
    End Function

    Private Function ElectricHeater_SourceInstallation(controls As CLElectricModeControls) As CLCoilInstallationType
        Return If(ElectricHeater_SelectedInstallation(controls) = CLCoilInstallationType.RequestedInternal, CLCoilInstallationType.External, ElectricHeater_SelectedInstallation(controls))
    End Function

    Private Function ElectricHeater_IsCustomized(controls As CLElectricModeControls) As Boolean
        Dim item = TryCast(controls.EditMode.SelectedItem, CLElectricChoice(Of CLCoilPerformanceEditMode))
        Return item IsNot Nothing AndAlso item.Value = CLCoilPerformanceEditMode.StandardCustomized
    End Function

    Private Sub ElectricHeater_EnableChanged(controls As CLElectricModeControls)
        If m_ElectricHeaterChanging Then Return
        If controls.Enable.Checked AndAlso controls.Mode = CLElectricHeaterMode.EHD AndAlso ElectricHeater_HasWaterHeatingConflict() Then
            controls.Enable.Checked = False
        End If
        CoilPerformance_UpdateControlState()
        ElectricHeater_UpdateControlState()
        Calculate()
    End Sub

    Private Sub ElectricHeater_EditModeChanged(controls As CLElectricModeControls)
        If m_ElectricHeaterChanging Then Return
        If ElectricHeater_IsCustomized(controls) AndAlso Not controls.CustomDisclaimerAccepted Then
            Dim message = CoilPerformance_Text("MainForm_ElectricHeater_CustomDisclaimer", "Custom electric-heater selection must be verified for every intended operating point. Continue?")
            If MessageBox.Show(Me, message, CoilPerformance_Text("MainForm_ElectricHeater_Tab", "Electric heaters"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) <> DialogResult.OK Then
                controls.EditMode.SelectedIndex = 0
                Return
            End If
            controls.CustomDisclaimerAccepted = True
        End If
        ElectricHeater_UpdateControlState()
    End Sub

    Private Sub ElectricHeater_InstallationChanged(controls As CLElectricModeControls)
        If m_ElectricHeaterChanging Then Return
        Try
            m_ElectricHeaterChanging = True
            ElectricHeater_FillHeaterCombo(controls, Nothing)
        Finally
            m_ElectricHeaterChanging = False
        End Try
        ElectricHeater_LoadSelected(controls)
        Calculate()
    End Sub

    Private Sub ElectricHeater_SelectedChanged(controls As CLElectricModeControls)
        If m_ElectricHeaterChanging Then Return
        ElectricHeater_LoadSelected(controls)
        Calculate()
    End Sub

    Private Sub ElectricHeater_LoadSelected(controls As CLElectricModeControls)
        Dim heater = TryCast(controls.Heater.SelectedItem, CLElectricHeaterDefinition)
        If heater Is Nothing Then
            For Each box In New TextBox() {controls.Power, controls.Voltage, controls.Phases, controls.Current, controls.AirIn, controls.AirOut, controls.AirOutRH, controls.PressureDrop, controls.ExhaustOut}
                If box IsNot Nothing Then box.Clear()
            Next
            Return
        End If
        controls.Power.Text = FormatNumber(heater.TotalPowerW, 0)
        controls.Voltage.Text = FormatNumber(heater.VoltageV, 0)
        controls.Phases.Text = heater.PhaseCount.ToString(CultureInfo.CurrentCulture)
        controls.Current.Text = FormatNumber(heater.TotalCurrentA, 2)
    End Sub

    Private Function ElectricHeater_HasWaterHeatingConflict() As Boolean
        If chbCoilPerformance_Enable Is Nothing OrElse Not chbCoilPerformance_Enable.Checked OrElse cmbCoilPerformance_Mode Is Nothing Then Return False
        Dim mode As CLCoilPerformanceMode = DirectCast(cmbCoilPerformance_Mode.SelectedItem, CLCoilPerformanceMode)
        Return mode = CLCoilPerformanceMode.HWD OrElse mode = CLCoilPerformanceMode.HCD
    End Function

    Private Function ElectricHeater_IsModeEnabled(mode As CLElectricHeaterMode) As Boolean
        If Not m_ElectricModeControls.ContainsKey(mode) Then Return False
        Dim controls As CLElectricModeControls = m_ElectricModeControls(mode)
        Return controls.Enable.Checked AndAlso TypeOf controls.Heater.SelectedItem Is CLElectricHeaterDefinition
    End Function

    Private Sub ElectricHeater_UpdateControlState()
        If tbpData_ElectricHeaters Is Nothing Then Return
        CoilPerformance_ApplyElectricPostHeaterConstraint()
        Dim conflict = ElectricHeater_HasWaterHeatingConflict()
        For Each modeControls As CLElectricModeControls In m_ElectricModeControls.Values
            Dim hasMode = m_ElectricHeaters.Any(Function(item) item.Mode = modeControls.Mode)
            Dim blocked = modeControls.Mode = CLElectricHeaterMode.EHD AndAlso conflict
            If blocked AndAlso modeControls.Enable.Checked Then
                m_ElectricHeaterChanging = True
                modeControls.Enable.Checked = False
                m_ElectricHeaterChanging = False
            End If
            modeControls.Enable.Enabled = hasMode AndAlso Not blocked
            Dim enabled = hasMode AndAlso modeControls.Enable.Checked AndAlso Not blocked
            modeControls.EditMode.Enabled = ElectricHeaterCustomSelectionEnabled AndAlso enabled
            modeControls.Installation.Enabled = enabled AndAlso modeControls.Installation.Items.Count > 1
            modeControls.Heater.Enabled = enabled
            modeControls.Conflict.Visible = blocked
            If modeControls.FrostStatus IsNot Nothing AndAlso Not enabled Then modeControls.FrostStatus.Visible = False
            modeControls.CustomNote.Visible = enabled AndAlso ElectricHeater_IsCustomized(modeControls)
        Next
        Accessories_UpdateSummary()
    End Sub

    Private Function ElectricHeater_GetEnabledPower(mode As CLElectricHeaterMode) As Double
        If Not m_ElectricModeControls.ContainsKey(mode) Then Return 0
        Dim controls = m_ElectricModeControls(mode)
        If Not controls.Enable.Checked OrElse (mode = CLElectricHeaterMode.EHD AndAlso ElectricHeater_HasWaterHeatingConflict()) Then Return 0
        Dim heater = TryCast(controls.Heater.SelectedItem, CLElectricHeaterDefinition)
        Return If(heater Is Nothing, 0, heater.TotalPowerW)
    End Function

    Private Function Calculate_ElectricHeaters() As Double
        m_ElectricHeaterLastPressureDrop = 0
        m_ElectricHeaterLastResults.Clear()
        If tbpData_ElectricHeaters Is Nothing Then Return 0
        Dim airflow = If(m_MeasureUnit = CLMeasureUnit.IP, ParseUIDouble(txbPerformance_AirFlow.Text) * 3.6D, ParseUIDouble(txbPerformance_AirFlow.Text))
        Dim nominalAirflow = ElectricHeater_ModelNominalAirflow(airflow)
        For Each modeControls As CLElectricModeControls In m_ElectricModeControls.Values
            Dim heater = TryCast(modeControls.Heater.SelectedItem, CLElectricHeaterDefinition)
            If heater Is Nothing OrElse Not modeControls.Enable.Checked OrElse (modeControls.Mode = CLElectricHeaterMode.EHD AndAlso ElectricHeater_HasWaterHeatingConflict()) Then
                modeControls.AirIn.Clear() : modeControls.AirOut.Clear() : modeControls.AirOutRH.Clear() : modeControls.PressureDrop.Clear()
                If modeControls.ExhaustOut IsNot Nothing Then modeControls.ExhaustOut.Clear()
                If modeControls.FrostStatus IsNot Nothing Then modeControls.FrostStatus.Visible = False
                Continue For
            End If
            Dim inlet As Double
            If modeControls.Mode = CLElectricHeaterMode.PEHD Then
                inlet = ParseUIDouble(txbPerformance_FreshInletTemperature.Text)
            Else
                inlet = If(m_HasLastWinterThermo, m_LastWinterThermo.Supply_outlet_temp, SupplyOutletTemp)
            End If
            Dim outlet = inlet + CLElectricHeaterCalculator.TemperatureRise(heater.TotalPowerW, airflow)
            Dim inletRhPercent = If(modeControls.Mode = CLElectricHeaterMode.PEHD,
                ParseUIDouble(txbPerformance_RHFreshInlet.Text),
                If(m_HasLastWinterThermo, 100D * m_LastWinterThermo.Supply_outlet_rh, SupplyOutletRH))
            Dim inletHumidity = PsychroCalc(inlet, Math.Max(0D, Math.Min(100D, inletRhPercent)) / 100D)
            Dim outletRhPercent = 100D * PsychroCalcW(outlet, inletHumidity.w).rh
            outletRhPercent = Math.Max(0D, Math.Min(100D, outletRhPercent))
            Dim nominalDrop = CLElectricHeaterCalculator.NominalPressureDrop(nominalAirflow) * Math.Max(1, heater.Quantity)
            Dim pressureDrop = CLElectricHeaterCalculator.PressureDropAtAirflow(nominalDrop, airflow, nominalAirflow)
            modeControls.AirIn.Text = FormatNumber(inlet, 1)
            modeControls.AirOut.Text = FormatNumber(outlet, 1)
            modeControls.AirOutRH.Text = FormatNumber(outletRhPercent, 0)
            modeControls.PressureDrop.Text = FormatNumber(pressureDrop, 1)
            If modeControls.ExhaustOut IsNot Nothing AndAlso m_HasLastWinterThermo Then
                modeControls.ExhaustOut.Text = FormatNumber(m_LastWinterThermo.Exhaust_outlet_temp, 1)
                modeControls.ExhaustOut.BackColor = If(m_LastWinterThermo.Exhaust_outlet_temp > 3D, SystemColors.Control, Color.MistyRose)
                If modeControls.FrostStatus IsNot Nothing Then modeControls.FrostStatus.Visible = m_LastWinterThermo.Exhaust_outlet_temp <= 3D
            End If
            m_ElectricHeaterLastPressureDrop += pressureDrop
            m_ElectricHeaterLastResults.Add(New CLElectricHeaterCalculationResult With {
                .Mode = modeControls.Mode,
                .PowerW = heater.TotalPowerW,
                .CurrentA = heater.TotalCurrentA,
                .AirInletTemperatureC = inlet,
                .AirOutletTemperatureC = outlet,
                .AirOutletRelativeHumidityPercent = outletRhPercent,
                .AirPressureDropPa = pressureDrop,
                .NominalAirPressureDropPa = nominalDrop,
                .ExhaustOutletTemperatureC = If(modeControls.Mode = CLElectricHeaterMode.PEHD AndAlso m_HasLastWinterThermo, CType(m_LastWinterThermo.Exhaust_outlet_temp, Double?), Nothing)
            })
        Next
        Return m_ElectricHeaterLastPressureDrop
    End Function

    Private Function ElectricHeater_ModelNominalAirflow(fallback As Double) As Double
        If SelectedHeatRecoveryModel Is Nothing Then Return Math.Max(1, fallback)
        For Each propertyName In New String() {"NominalAirflow", "AirFlow", "FlowRate", "Portata"}
            Dim prop = SelectedHeatRecoveryModel.GetType().GetProperty(propertyName, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.IgnoreCase)
            If prop Is Nothing Then Continue For
            Try
                Dim value = Convert.ToDouble(prop.GetValue(SelectedHeatRecoveryModel, Nothing), CultureInfo.InvariantCulture)
                If value > 0 Then Return value
            Catch
            End Try
        Next
        Return Math.Max(1, fallback)
    End Function

    Private Sub ElectricHeater_UpdateLocalizedTexts()
        If tbpData_ElectricHeaters Is Nothing Then Return
        tbpData_ElectricHeaters.Text = CoilPerformance_Text("MainForm_ElectricHeater_Tab", "Electric heaters")
        CoilPerformance_UpdateLocalizedControlTexts(tbpData_ElectricHeaters)
        ElectricHeater_FillAvailable()
    End Sub

    Private Function ElectricHeater_CaptureSelection() As CLElectricHeaterSelection
        Dim selection As New CLElectricHeaterSelection()
        selection.EHD = ElectricHeater_CaptureMode(m_ElectricModeControls(CLElectricHeaterMode.EHD))
        selection.PEHD = ElectricHeater_CaptureMode(m_ElectricModeControls(CLElectricHeaterMode.PEHD))
        selection.Enabled = selection.EHD.Enabled OrElse selection.PEHD.Enabled
        Dim selectedDefinitions = m_ElectricModeControls.Values.
            Where(Function(item) item.Enable.Checked).
            Select(Function(item) TryCast(item.Heater.SelectedItem, CLElectricHeaterDefinition)).
            Where(Function(item) item IsNot Nothing).
            ToList()
        If selectedDefinitions.Count > 0 Then
            selection.Stages = selectedDefinitions.Sum(Function(item) item.NumberOfStages * Math.Max(1, item.Quantity))
            selection.NominalPowerW = selectedDefinitions.Sum(Function(item) item.TotalPowerW)
        End If
        Return selection
    End Function

    Private Function ElectricHeater_CaptureMode(controls As CLElectricModeControls) As CLElectricHeaterModeSelection
        Dim heater = TryCast(controls.Heater.SelectedItem, CLElectricHeaterDefinition)
        Dim result As New CLElectricHeaterModeSelection With {
            .Enabled = controls.Enable.Checked,
            .Mode = controls.Mode.ToString(),
            .SelectionCase = If(ElectricHeater_IsCustomized(controls), CLCoilPerformanceEditMode.StandardCustomized.ToString(), CLCoilPerformanceEditMode.Standard.ToString()),
            .InstallationType = ElectricHeater_SelectedInstallation(controls).ToString(),
            .CustomDesignDisclaimerAccepted = controls.CustomDisclaimerAccepted
        }
        If heater IsNot Nothing Then
            result.Heater = New CLSelectionEntityReference With {.Id = heater.Id, .Code = heater.Code, .ManagementCode = heater.ManagementCode, .Name = heater.Name}
        End If
        Return result
    End Function

    Private Sub ElectricHeater_ApplySelection(selection As CLElectricHeaterSelection)
        If selection Is Nothing OrElse tbpData_ElectricHeaters Is Nothing Then Return
        ElectricHeater_ApplyMode(m_ElectricModeControls(CLElectricHeaterMode.EHD), selection.EHD)
        ElectricHeater_ApplyMode(m_ElectricModeControls(CLElectricHeaterMode.PEHD), selection.PEHD)
        ElectricHeater_UpdateControlState()
    End Sub

    Private Sub ElectricHeater_ApplyMode(controls As CLElectricModeControls, selection As CLElectricHeaterModeSelection)
        If selection Is Nothing Then Return
        Try
            m_ElectricHeaterChanging = True
            controls.CustomDisclaimerAccepted = selection.CustomDesignDisclaimerAccepted
            Dim requestedInstallation As CLCoilInstallationType
            If Not [Enum].TryParse(selection.InstallationType, True, requestedInstallation) Then requestedInstallation = CLCoilInstallationType.Internal
            ElectricHeater_SelectInstallation(controls, requestedInstallation)
            ElectricHeater_FillHeaterCombo(controls, Nothing)
            If selection.Heater IsNot Nothing Then
                For i = 0 To controls.Heater.Items.Count - 1
                    Dim candidate = TryCast(controls.Heater.Items(i), CLElectricHeaterDefinition)
                    If candidate IsNot Nothing AndAlso ((selection.Heater.Id.HasValue AndAlso candidate.Id = selection.Heater.Id.Value) OrElse String.Equals(candidate.Code, selection.Heater.Code, StringComparison.OrdinalIgnoreCase)) Then
                        controls.Heater.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If
            controls.EditMode.SelectedIndex = If(
                ElectricHeaterCustomSelectionEnabled AndAlso
                String.Equals(selection.SelectionCase, CLCoilPerformanceEditMode.StandardCustomized.ToString(), StringComparison.OrdinalIgnoreCase),
                1,
                0)
            controls.Enable.Checked = selection.Enabled AndAlso controls.Heater.Items.Count > 0
        Finally
            m_ElectricHeaterChanging = False
        End Try
        ElectricHeater_LoadSelected(controls)
    End Sub

    Private Function ElectricHeater_CaptureSnapshots() As List(Of CLElectricHeaterCalculationSnapshot)
        Dim snapshots As New List(Of CLElectricHeaterCalculationSnapshot)()
        For Each result In m_ElectricHeaterLastResults
            Dim controls = m_ElectricModeControls(result.Mode)
            Dim heater = TryCast(controls.Heater.SelectedItem, CLElectricHeaterDefinition)
            snapshots.Add(New CLElectricHeaterCalculationSnapshot With {
                .Mode = result.Mode.ToString(),
                .HeaterCode = If(heater Is Nothing, Nothing, heater.Code),
                .PowerW = result.PowerW,
                .CurrentA = result.CurrentA,
                .AirInletTemperatureC = result.AirInletTemperatureC,
                .AirOutletTemperatureC = result.AirOutletTemperatureC,
                .AirOutletRelativeHumidityPercent = result.AirOutletRelativeHumidityPercent,
                .AirPressureDropPa = result.AirPressureDropPa,
                .ExhaustOutletTemperatureC = result.ExhaustOutletTemperatureC
            })
        Next
        Return snapshots
    End Function

    Private Function Report_CreateElectricHeaterReportTable() As DataTable
        Dim table As New DataTable("ElectricHeaterReport")
        Dim columns As String() = {
            "Visible", "ScenarioKey", "Title",
            "ModeCaption", "Mode",
            "CaseCaption", "Case",
            "InstallationCaption", "Installation",
            "HeaterCaption", "Heater",
            "CodeCaption", "Code",
            "ManagementCodeCaption", "ManagementCode",
            "PowerCaption", "Power",
            "VoltageCaption", "Voltage",
            "PhasesCaption", "Phases",
            "FrequencyCaption", "Frequency",
            "CurrentCaption", "Current",
            "StagesCaption", "Stages",
            "QuantityCaption", "Quantity",
            "AirInCaption", "AirIn",
            "AirOutCaption", "AirOut",
            "AirDPCaption", "AirDP",
            "NominalAirDPCaption", "NominalAirDP",
            "ExhaustOutCaption", "ExhaustOut",
            "FrostStatusCaption", "FrostStatus",
            "CustomDisclaimerAccepted", "Summary"
        }
        For Each columnName In columns
            table.Columns.Add(columnName, GetType(String))
        Next

        For Each calculation In m_ElectricHeaterLastResults
            Dim modeControls = m_ElectricModeControls(calculation.Mode)
            If Not modeControls.Enable.Checked Then Continue For
            Dim heater = TryCast(modeControls.Heater.SelectedItem, CLElectricHeaterDefinition)
            If heater Is Nothing Then Continue For

            Dim frostStatus As String = String.Empty
            If calculation.Mode = CLElectricHeaterMode.PEHD AndAlso calculation.ExhaustOutletTemperatureC.HasValue Then
                frostStatus = If(calculation.ExhaustOutletTemperatureC.Value > 3D,
                    CoilPerformance_Text("MainForm_ElectricHeater_FrostTargetReached", "Target reached"),
                    CoilPerformance_Text("MainForm_ElectricHeater_FrostTargetNotReached", "Target not reached"))
            End If

            Dim row = table.NewRow()
            row("Visible") = "True"
            row("ScenarioKey") = "Winter"
            row("Title") = CoilPerformance_Text("MainForm_ElectricHeater_Tab", "Electric heaters")
            row("ModeCaption") = CoilPerformance_Text("MainForm_CoilPerformance_ResultMode", "Mode")
            row("Mode") = calculation.Mode.ToString()
            row("CaseCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Case", "Case")
            row("Case") = modeControls.EditMode.Text
            row("InstallationCaption") = CoilPerformance_Text("MainForm_CoilPerformance_Installation", "Installation")
            row("Installation") = modeControls.Installation.Text
            row("HeaterCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Heater", "Electric heater")
            row("Heater") = heater.ToString()
            row("CodeCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Code", "Code")
            row("Code") = heater.Code
            row("ManagementCodeCaption") = CoilPerformance_Text("MainForm_ElectricHeater_ManagementCode", "Management code")
            row("ManagementCode") = heater.ManagementCode
            row("PowerCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Power", "Power [W]")
            row("Power") = FormatNumber(calculation.PowerW, 0)
            row("VoltageCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Voltage", "Voltage [V]")
            row("Voltage") = FormatNumber(heater.VoltageV, 0)
            row("PhasesCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Phases", "Phases")
            row("Phases") = heater.PhaseCount.ToString(CultureInfo.CurrentCulture)
            row("FrequencyCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Frequency", "Frequency [Hz]")
            row("Frequency") = FormatNumber(heater.FrequencyHz, 0)
            row("CurrentCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Current", "Current [A]")
            row("Current") = FormatNumber(calculation.CurrentA, 2)
            row("StagesCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Stages", "Stages")
            row("Stages") = (heater.NumberOfStages * Math.Max(1, heater.Quantity)).ToString(CultureInfo.CurrentCulture)
            row("QuantityCaption") = CoilPerformance_Text("MainForm_ElectricHeater_Quantity", "Quantity")
            row("Quantity") = heater.Quantity.ToString(CultureInfo.CurrentCulture)
            row("AirInCaption") = CoilPerformance_Text("MainForm_ElectricHeater_AirIn", "Air inlet [C]")
            row("AirIn") = FormatNumber(calculation.AirInletTemperatureC, 1)
            row("AirOutCaption") = String.Format("{0} - {1}",
                CoilPerformance_Text("MainForm_ElectricHeater_AirOut", "Max. air out temp. [°C]"),
                CoilPerformance_Text("MainForm_CoilPerformance_ResultRHOut", "R.H. out [%]"))
            row("AirOut") = String.Format("{0} - {1}",
                FormatNumber(calculation.AirOutletTemperatureC, 1),
                FormatNumber(calculation.AirOutletRelativeHumidityPercent, 0))
            row("AirDPCaption") = CoilPerformance_Text("MainForm_ElectricHeater_PressureDrop", "Air DP [Pa]")
            row("AirDP") = FormatNumber(calculation.AirPressureDropPa, 1)
            row("NominalAirDPCaption") = CoilPerformance_Text("MainForm_ElectricHeater_NominalPressureDrop", "Nominal air DP [Pa]")
            row("NominalAirDP") = FormatNumber(calculation.NominalAirPressureDropPa, 0)
            row("ExhaustOutCaption") = CoilPerformance_Text("MainForm_ElectricHeater_ExhaustOut", "Exhaust outlet [C]")
            row("ExhaustOut") = If(calculation.ExhaustOutletTemperatureC.HasValue, FormatNumber(calculation.ExhaustOutletTemperatureC.Value, 1), String.Empty)
            row("FrostStatusCaption") = CoilPerformance_Text("MainForm_ElectricHeater_FrostStatus", "Frost-protection target")
            row("FrostStatus") = frostStatus
            row("CustomDisclaimerAccepted") = If(
                ElectricHeater_IsCustomized(modeControls) AndAlso modeControls.CustomDisclaimerAccepted,
                CoilPerformance_Text("MainForm_ElectricHeater_CustomDisclaimerAccepted", "Custom electric-heater disclaimer accepted"),
                String.Empty)
            row("Summary") = String.Format("{0} | {1} | {2}: {3} | {4}: {5} | {6}: {7} | {8}: {9} | {10}: {11}",
                calculation.Mode,
                heater,
                row("PowerCaption"), row("Power"),
                row("VoltageCaption"), row("Voltage"),
                row("CurrentCaption"), row("Current"),
                row("AirOutCaption"), row("AirOut"),
                row("AirDPCaption"), row("AirDP"))
            table.Rows.Add(row)
        Next
        table.AcceptChanges()
        Return table
    End Function
End Class
