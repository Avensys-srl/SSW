Imports Microsoft.Win32

Partial Public Class CLMainForm
    Private Const HelpSettingsRegistryPath As String = "Software\Avensys\SSW"
    Private Const HelpToolTipsRegistryValue As String = "HelpToolTipsEnabled"

    Private tsmiHelp As ToolStripMenuItem
    Private tsmiHelp_OpenGuide As ToolStripMenuItem
    Private tsmiHelp_ShowToolTips As ToolStripMenuItem
    Private tsmiOption_ShowHelpToolTips As ToolStripMenuItem
    Private m_HelpToolTipsEnabled As Boolean = True

    Private Sub Help_InitializeMenus()
        m_HelpToolTipsEnabled = Help_LoadToolTipPreference()

        tsmiHelp = New ToolStripMenuItem()
        tsmiHelp_OpenGuide = New ToolStripMenuItem()
        tsmiHelp_ShowToolTips = New ToolStripMenuItem() With {.CheckOnClick = False}
        tsmiOption_ShowHelpToolTips = New ToolStripMenuItem() With {.CheckOnClick = False}

        AddHandler tsmiHelp_OpenGuide.Click, AddressOf Help_OpenGuide_Click
        AddHandler tsmiHelp_ShowToolTips.Click, AddressOf Help_ToggleToolTips_Click
        AddHandler tsmiOption_ShowHelpToolTips.Click, AddressOf Help_ToggleToolTips_Click

        tsmiHelp.DropDownItems.Add(tsmiHelp_OpenGuide)
        tsmiHelp.DropDownItems.Add(New ToolStripSeparator())
        tsmiHelp.DropDownItems.Add(tsmiHelp_ShowToolTips)

        Dim aboutIndex As Integer = mnsMain.Items.IndexOf(tsmiAbout)
        If aboutIndex >= 0 Then
            mnsMain.Items.Insert(aboutIndex, tsmiHelp)
        Else
            mnsMain.Items.Add(tsmiHelp)
        End If

        tsmiOption.DropDownItems.Add(New ToolStripSeparator())
        tsmiOption.DropDownItems.Add(tsmiOption_ShowHelpToolTips)
        Help_UpdateLocalizedTexts()
    End Sub

    Private Sub Help_OpenGuide_Click(sender As Object, e As EventArgs)
        Using helpForm As New CLHelpForm(Environment)
            helpForm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub Help_ToggleToolTips_Click(sender As Object, e As EventArgs)
        m_HelpToolTipsEnabled = Not m_HelpToolTipsEnabled
        Help_SaveToolTipPreference(m_HelpToolTipsEnabled)
        Help_ApplyToolTips()
    End Sub

    Private Sub Help_UpdateLocalizedTexts()
        If tsmiHelp Is Nothing Then Return

        tsmiHelp.Text = Help_Text("Help_Menu", "Help")
        tsmiHelp_OpenGuide.Text = Help_Text("Help_OpenGuide", "User guide")
        tsmiHelp_ShowToolTips.Text = Help_Text("Help_ShowToolTips", "Show help tips")
        tsmiOption_ShowHelpToolTips.Text = Help_Text("Help_ShowToolTips", "Show help tips")
        Help_ApplyToolTips()
    End Sub

    Private Sub Help_ApplyToolTips()
        If ToolTip1 Is Nothing Then Return

        ToolTip1.Active = m_HelpToolTipsEnabled
        ToolTip1.AutoPopDelay = 12000
        ToolTip1.InitialDelay = 550
        ToolTip1.ReshowDelay = 150
        ToolTip1.ShowAlways = True

        If tsmiHelp_ShowToolTips IsNot Nothing Then tsmiHelp_ShowToolTips.Checked = m_HelpToolTipsEnabled
        If tsmiOption_ShowHelpToolTips IsNot Nothing Then tsmiOption_ShowHelpToolTips.Checked = m_HelpToolTipsEnabled

        ' ToolStrip menu tips require both the item text and ShowItemToolTips on
        ' every owning drop-down. Keep this traversal here so future submenus
        ' automatically inherit the same help behaviour.
        Help_EnableMenuToolTips(mnsMain.Items)

        Help_SetToolTip(cmbPerformance_Series, "Help_Tip_Series", "Choose the product family first; the unit list is filtered accordingly.")
        Help_SetToolTip(cmbPerformance_HeatRecoveryModels, "Help_Tip_Unit", "Choose the ventilation unit to calculate.")
        Help_SetToolTip(txbPerformance_AirFlow, "Help_Tip_Airflow", "Enter the winter design airflow. Summer airflow is currently kept synchronized.")
        Help_SetToolTip(txbPerformance_MaxPressure, "Help_Tip_Pressure", "Available external static pressure at the selected airflow.")
        Help_SetToolTip(grbPerformance_TemperatureConditions, "Help_Tip_Conditions", "Set winter conditions on the left and summer conditions on the right.")
        Help_SetToolTip(btn_summer, "Help_Tip_Summer", "Enable or disable the independent summer calculation.")
        Help_SetToolTip(hsbPerformance_RegulationLevel, "Help_Tip_Regulation", "Move the control level and SSW recalculates the operating point and performance.")
        Help_SetToolTip(m_Note_Text, "Help_Tip_Reference", "Customer reference used in saved file names, reports and projects.")
        Help_SetToolTip(tbpData_CoilPerformance, "Help_Tip_WaterCoils", "Select and calculate compatible water coils. Heating and EHD post-heating are mutually exclusive.")
        Help_SetToolTip(tbpData_ElectricHeaters, "Help_Tip_ElectricHeaters", "Select compatible pre-heaters (PEHD) and post-heaters (EHD).")
        Help_SetToolTip(tbpData_Accessories, "Help_Tip_Accessories", "Complete the selection with compatible accessories and control functions.")

        Help_SetMenuToolTip(m_ProjectMenuNew, "Help_MenuTip_New", "Start a blank technical selection. You can save the current changes before continuing.")
        Help_SetMenuToolTip(m_ProjectMenuOpen, "Help_MenuTip_Open", "Open an .sswsel file and restore its saved configuration and selection identity.")
        Help_SetMenuToolTip(m_ProjectMenuSave, "Help_MenuTip_Save", "Save changes to the current .sswsel file, or choose a file name if it has not been saved before.")
        Help_SetMenuToolTip(m_ProjectMenuSaveAs, "Help_MenuTip_SaveAs", "Save the current selection to another .sswsel file while retaining the same technical selection identity.")
        Help_SetMenuToolTip(m_ProjectMenuDuplicate, "Help_MenuTip_Duplicate", "Create an independent selection with a new identity, preserving the current configuration as its starting point.")
        Help_SetMenuToolTip(m_ProjectMenuAlternative, "Help_MenuTip_Alternative", "Create an independent alternative and add or increment the Alt. XX prefix in the customer reference.")
        Help_SetMenuToolTip(m_ProjectMenuRecent, "Help_MenuTip_Recent", "Open one of the most recently used .sswsel files.")
        Help_SetMenuToolTip(m_MultiSelectionMenu, "Help_MenuTip_Project", "Create or manage a project that groups several selections and their PDF reports.")
        Help_SetMenuToolTip(tsmiFile_GenerateReport, "Help_MenuTip_Report", "Register the current technical selection and generate its localized PDF report.")
        Help_SetMenuToolTip(tsmiFile_SaveCommercialSheet, "Help_MenuTip_CommercialSheet", "Generate the commercial product sheet when it is available for the selected unit and language.")
        Help_SetMenuToolTip(tsmiFile_SaveIOM, "Help_MenuTip_Iom", "Generate the installation, operation and maintenance document when it is available.")
        Help_SetMenuToolTip(tsmiFile_Exit, "Help_MenuTip_Exit", "Close SSW. You will be asked whether to save unsaved selection changes.")

        Help_SetMenuToolTip(tsmiOption_Language, "Help_MenuTip_Language", "Change the interface, report and communication language. Language names remain written in their own language.")
        Help_SetMenuToolTip(tsmiOption_Unit, "Help_MenuTip_Units", "Choose the measurement system used by the interface and generated documents.")
        Help_SetMenuToolTip(tsmiOption_CommercialSheetAutoSync, "Help_MenuTip_CommercialSync", "Automatically synchronize the commercial sheet with the active selection when this option is available.")
        Help_SetMenuToolTip(tsmiOption_CheckUpdates, "Help_MenuTip_Updates", "Check manually whether a newer SSW version is available.")
        Help_SetMenuToolTip(tsmiHelp_OpenGuide, "Help_MenuTip_OpenGuide", "Open the searchable user guide for selections, reports and projects.")
        Help_SetMenuToolTip(tsmiHelp_ShowToolTips, "Help_MenuTip_ToggleTips", "Enable or disable contextual help tips throughout SSW.")
        Help_SetMenuToolTip(tsmiOption_ShowHelpToolTips, "Help_MenuTip_ToggleTips", "Enable or disable contextual help tips throughout SSW.")
        If m_FollowUpStore IsNot Nothing Then FollowUp_UpdateBell()
    End Sub

    Private Sub Help_EnableMenuToolTips(items As ToolStripItemCollection)
        If items Is Nothing Then Return
        If mnsMain IsNot Nothing Then mnsMain.ShowItemToolTips = m_HelpToolTipsEnabled

        For Each item As ToolStripItem In items
            Dim menuItem As ToolStripMenuItem = TryCast(item, ToolStripMenuItem)
            If menuItem Is Nothing Then Continue For
            menuItem.AutoToolTip = False
            menuItem.DropDown.ShowItemToolTips = m_HelpToolTipsEnabled
            If menuItem.HasDropDownItems Then Help_EnableMenuToolTips(menuItem.DropDownItems)
        Next
    End Sub

    Private Sub Help_SetMenuToolTip(item As ToolStripItem, key As String, fallback As String)
        If item Is Nothing Then Return
        item.AutoToolTip = False
        item.ToolTipText = If(m_HelpToolTipsEnabled, Help_Text(key, fallback), String.Empty)
    End Sub

    Private Sub Help_SetToolTip(control As Control, key As String, fallback As String)
        If control IsNot Nothing Then ToolTip1.SetToolTip(control, Help_Text(key, fallback))
    End Sub

    Private Function Help_Text(key As String, fallback As String) As String
        Try
            Dim value As String = Environment.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso Not value.StartsWith("@@", StringComparison.Ordinal) Then
                Return value
            End If
        Catch
        End Try
        Return fallback
    End Function

    Private Shared Function Help_LoadToolTipPreference() As Boolean
        Try
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(HelpSettingsRegistryPath, False)
                If key Is Nothing Then Return True
                Dim value As Object = key.GetValue(HelpToolTipsRegistryValue, Nothing)
                If value Is Nothing Then Return True
                Return Convert.ToInt32(value, Globalization.CultureInfo.InvariantCulture) <> 0
            End Using
        Catch
            Return True
        End Try
    End Function

    Private Shared Sub Help_SaveToolTipPreference(enabled As Boolean)
        Try
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(HelpSettingsRegistryPath)
                key.SetValue(HelpToolTipsRegistryValue, If(enabled, 1, 0), RegistryValueKind.DWord)
            End Using
        Catch
            ' Help remains usable even when the user profile is read-only.
        End Try
    End Sub
End Class
