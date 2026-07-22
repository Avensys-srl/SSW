Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Public NotInheritable Class CLFollowUpNotificationItem
    Public Property ReminderUuid As Guid
    Public Property DisplayReference As String
    Public Property TargetType As String
    Public Property DueAtUtc As DateTime
    Public Property Status As String
    Public Property RescheduleCount As Integer
    Public Property IsUnread As Boolean
    Public Property LocalPath As String
End Class

Public NotInheritable Class CLFollowUpActionEventArgs
    Inherits EventArgs

    Public Property ReminderUuid As Guid
    Public Property ActionName As String
    Public Property Days As Integer?
End Class

Public NotInheritable Class CLFollowUpNotificationCenterForm
    Inherits Form

    Public Event ActionRequested As EventHandler(Of CLFollowUpActionEventArgs)
    Public Event OpenRequested As EventHandler(Of CLFollowUpActionEventArgs)
    Public Event RefreshRequested As EventHandler

    Private ReadOnly m_Tabs As New TabControl()
    Private ReadOnly m_Grid As New DataGridView()
    Private ReadOnly m_Status As New Label()
    Private ReadOnly m_Items As New List(Of CLFollowUpNotificationItem)()

    Public Sub New()
        Text = T("FollowUp_CenterTitle", "Follow-up reminders")
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(760, 420)
        Size = New Size(1020, 610)
        Padding = New Padding(10)
        BuildUi()
    End Sub

    Public Sub SetItems(items As IEnumerable(Of CLFollowUpNotificationItem), synchronizationStatus As String)
        m_Items.Clear()
        If items IsNot Nothing Then m_Items.AddRange(items)
        m_Status.Text = If(synchronizationStatus, String.Empty)
        ReloadGrid()
    End Sub

    Private Sub BuildUi()
        Dim root As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 3}
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))

        m_Tabs.Dock = DockStyle.Fill
        m_Tabs.TabPages.Add(New TabPage(T("FollowUp_FilterDue", "Due / overdue")) With {.Tag = "Due"})
        m_Tabs.TabPages.Add(New TabPage(T("FollowUp_FilterUpcoming", "Upcoming")) With {.Tag = "Upcoming"})
        m_Tabs.TabPages.Add(New TabPage(T("FollowUp_FilterClosed", "Closed")) With {.Tag = "Closed"})
        AddHandler m_Tabs.SelectedIndexChanged, Sub(sender, args) ReloadGrid()
        root.Controls.Add(m_Tabs, 0, 0)

        m_Grid.Dock = DockStyle.Fill
        m_Grid.ReadOnly = True
        m_Grid.AllowUserToAddRows = False
        m_Grid.AllowUserToDeleteRows = False
        m_Grid.AllowUserToResizeRows = False
        m_Grid.AutoGenerateColumns = False
        m_Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        m_Grid.BackgroundColor = SystemColors.Window
        m_Grid.BorderStyle = BorderStyle.Fixed3D
        m_Grid.MultiSelect = False
        m_Grid.RowHeadersVisible = False
        m_Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        AddColumn("Reference", T("FollowUp_ColumnReference", "Reference"), 35)
        AddColumn("Type", T("FollowUp_ColumnType", "Type"), 13)
        AddColumn("Due", T("FollowUp_ColumnDue", "Follow-up date"), 22)
        AddColumn("Status", T("FollowUp_ColumnStatus", "Status"), 18)
        AddColumn("Count", T("FollowUp_ColumnReschedules", "Reschedules"), 12)
        AddHandler m_Grid.CellDoubleClick, AddressOf GridDoubleClick
        root.Controls.Add(m_Grid, 0, 1)

        Dim footer As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1}
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        m_Status.Dock = DockStyle.Fill
        m_Status.TextAlign = ContentAlignment.MiddleLeft
        footer.Controls.Add(m_Status, 0, 0)

        Dim commands As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .AutoSize = True,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False
        }
        AddButton(commands, T("FollowUp_CloseWindow", "Close"), Sub(sender, args) Close())
        AddButton(commands, T("FollowUp_Refresh", "Refresh"), Sub(sender, args) RaiseEvent RefreshRequested(Me, EventArgs.Empty))
        AddButton(commands, T("FollowUp_Open", "Open"), AddressOf OpenClick)
        AddButton(commands, T("FollowUp_CancelReminder", "Cancel reminder"), Sub(sender, args) RequestAction("Cancelled"))
        AddButton(commands, T("FollowUp_Reschedule", "Reschedule"), AddressOf RescheduleClick)
        AddButton(commands, T("FollowUp_CloseUnsuccessful", "Close unsuccessful"), Sub(sender, args) RequestAction("Unsuccessful"))
        AddButton(commands, T("FollowUp_CloseSuccessful", "Close successful"), Sub(sender, args) RequestAction("Succeeded"))
        footer.Controls.Add(commands, 1, 0)
        root.Controls.Add(footer, 0, 2)
        Controls.Add(root)
    End Sub

    Private Sub ReloadGrid()
        m_Grid.Rows.Clear()
        Dim filter As String = If(m_Tabs.SelectedTab Is Nothing, "Due", Convert.ToString(m_Tabs.SelectedTab.Tag, CultureInfo.InvariantCulture))
        Dim nowUtc As DateTime = DateTime.UtcNow
        For Each item As CLFollowUpNotificationItem In m_Items.OrderBy(Function(value) value.DueAtUtc)
            Dim isClosed As Boolean = Not String.Equals(item.Status, "Pending", StringComparison.OrdinalIgnoreCase)
            Dim include As Boolean = (filter = "Closed" AndAlso isClosed) OrElse
                (filter = "Due" AndAlso Not isClosed AndAlso item.DueAtUtc <= nowUtc) OrElse
                (filter = "Upcoming" AndAlso Not isClosed AndAlso item.DueAtUtc > nowUtc)
            If Not include Then Continue For

            Dim rowIndex As Integer = m_Grid.Rows.Add(
                If(String.IsNullOrWhiteSpace(item.DisplayReference), "-", item.DisplayReference),
                TargetCaption(item.TargetType),
                item.DueAtUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture),
                StatusCaption(item.Status),
                item.RescheduleCount.ToString(CultureInfo.CurrentCulture))
            Dim row As DataGridViewRow = m_Grid.Rows(rowIndex)
            row.Tag = item
            If item.IsUnread AndAlso Not isClosed Then row.DefaultCellStyle.Font = New Font(m_Grid.Font, FontStyle.Bold)
        Next
    End Sub

    Private Sub AddColumn(name As String, caption As String, weight As Single)
        m_Grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = name,
            .HeaderText = caption,
            .FillWeight = weight,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        })
    End Sub

    Private Shared Sub AddButton(panel As FlowLayoutPanel, caption As String, handler As EventHandler)
        Dim button As New Button With {
            .Text = caption,
            .AutoSize = True,
            .MinimumSize = New Size(80, 27),
            .Margin = New Padding(6, 5, 0, 0)
        }
        AddHandler button.Click, handler
        panel.Controls.Add(button)
    End Sub

    Private Sub GridDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        OpenSelected()
    End Sub

    Private Sub OpenClick(sender As Object, e As EventArgs)
        OpenSelected()
    End Sub

    Private Sub OpenSelected()
        Dim item As CLFollowUpNotificationItem = SelectedItem()
        If item Is Nothing Then Return
        RaiseEvent OpenRequested(Me, New CLFollowUpActionEventArgs With {.ReminderUuid = item.ReminderUuid, .ActionName = "Open"})
    End Sub

    Private Sub RequestAction(actionName As String)
        Dim item As CLFollowUpNotificationItem = SelectedItem()
        If item Is Nothing OrElse Not String.Equals(item.Status, "Pending", StringComparison.OrdinalIgnoreCase) Then Return
        RaiseEvent ActionRequested(Me, New CLFollowUpActionEventArgs With {
            .ReminderUuid = item.ReminderUuid,
            .ActionName = actionName
        })
    End Sub

    Private Sub RescheduleClick(sender As Object, e As EventArgs)
        Dim item As CLFollowUpNotificationItem = SelectedItem()
        If item Is Nothing OrElse Not String.Equals(item.Status, "Pending", StringComparison.OrdinalIgnoreCase) Then Return
        Dim days As Integer? = CLFollowUpRescheduleDialog.Prompt(Me)
        If Not days.HasValue Then Return
        RaiseEvent ActionRequested(Me, New CLFollowUpActionEventArgs With {
            .ReminderUuid = item.ReminderUuid,
            .ActionName = "Reschedule",
            .Days = days
        })
    End Sub

    Private Function SelectedItem() As CLFollowUpNotificationItem
        If m_Grid.SelectedRows.Count = 0 Then Return Nothing
        Return TryCast(m_Grid.SelectedRows(0).Tag, CLFollowUpNotificationItem)
    End Function

    Private Shared Function TargetCaption(value As String) As String
        If String.Equals(value, "Project", StringComparison.OrdinalIgnoreCase) Then
            Return T("FollowUp_TargetProject", "Project")
        End If
        Return T("FollowUp_TargetSelection", "Selection")
    End Function

    Private Shared Function StatusCaption(value As String) As String
        Select Case If(value, String.Empty).ToLowerInvariant()
            Case "succeeded" : Return T("FollowUp_StatusSucceeded", "Successful")
            Case "unsuccessful" : Return T("FollowUp_StatusUnsuccessful", "Unsuccessful")
            Case "cancelled" : Return T("FollowUp_StatusCancelled", "Cancelled")
            Case Else : Return T("FollowUp_StatusPending", "Pending")
        End Select
    End Function

    Friend Shared Function T(key As String, fallback As String) As String
        Try
            Dim value As String = CLEnvironment.Current.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> "?" Then Return value
        Catch
        End Try
        Return fallback
    End Function
End Class

Friend NotInheritable Class CLFollowUpRescheduleDialog
    Inherits Form

    Private ReadOnly m_Days As New NumericUpDown()

    Private Sub New()
        Text = CLFollowUpNotificationCenterForm.T("FollowUp_Reschedule", "Reschedule")
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ClientSize = New Size(345, 105)
        Padding = New Padding(12)

        Dim root As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 2}
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 85.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 35.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0F))
        root.Controls.Add(New Label With {
            .Text = CLFollowUpNotificationCenterForm.T("FollowUp_ScheduleDays", "Follow up after [days]"),
            .AutoSize = True,
            .Anchor = AnchorStyles.Left
        }, 0, 0)
        m_Days.Minimum = 1D
        m_Days.Maximum = 90D
        m_Days.Value = 7D
        m_Days.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        root.Controls.Add(m_Days, 1, 0)

        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft, .WrapContents = False}
        Dim cancelButton As New Button With {.Text = CLFollowUpNotificationCenterForm.T("FollowUp_Cancel", "Cancel"), .DialogResult = DialogResult.Cancel, .AutoSize = True}
        Dim okButton As New Button With {.Text = CLFollowUpNotificationCenterForm.T("FollowUp_Confirm", "OK"), .DialogResult = DialogResult.OK, .AutoSize = True}
        buttons.Controls.Add(cancelButton)
        buttons.Controls.Add(okButton)
        root.SetColumnSpan(buttons, 2)
        root.Controls.Add(buttons, 0, 1)
        AcceptButton = okButton
        CancelButton = cancelButton
        Controls.Add(root)
    End Sub

    Public Shared Function Prompt(owner As IWin32Window) As Integer?
        Using dialog As New CLFollowUpRescheduleDialog()
            If dialog.ShowDialog(owner) <> DialogResult.OK Then Return Nothing
            Return Decimal.ToInt32(dialog.m_Days.Value)
        End Using
    End Function
End Class
