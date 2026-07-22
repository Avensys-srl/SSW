Imports System.Drawing
Imports System.Windows.Forms

Public NotInheritable Class CLFollowUpScheduleChoice
    Public Property Proceed As Boolean
    Public Property Schedule As Boolean
    Public Property Days As Integer = 7
End Class

Public NotInheritable Class CLFollowUpPreparedEventArgs
    Inherits EventArgs

    Public Property TargetType As String
    Public Property TargetUuid As Guid
    Public Property DisplayReference As String
    Public Property LocalPath As String
    Public Property PreparedAtUtc As DateTime
    Public Property DueAtUtc As DateTime
End Class

Public NotInheritable Class CLFollowUpScheduleDialog
    Inherits Form

    Private ReadOnly m_Schedule As New CheckBox()
    Private ReadOnly m_Days As New NumericUpDown()
    Private ReadOnly m_DaysLabel As New Label()
    Private ReadOnly m_Explanation As New Label()

    Private Sub New()
        Text = T("FollowUp_ScheduleTitle", "Email follow-up")
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ClientSize = New Size(470, 178)
        Padding = New Padding(14)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 4
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 95.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 31.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 36.0F))

        m_Schedule.Text = T("FollowUp_ScheduleEnabled", "Schedule follow-up")
        m_Schedule.Checked = True
        m_Schedule.AutoSize = True
        m_Schedule.Anchor = AnchorStyles.Left
        AddHandler m_Schedule.CheckedChanged, AddressOf ScheduleChanged
        root.SetColumnSpan(m_Schedule, 2)
        root.Controls.Add(m_Schedule, 0, 0)

        m_DaysLabel.Text = T("FollowUp_ScheduleDays", "Follow up after [days]")
        m_DaysLabel.AutoSize = True
        m_DaysLabel.Anchor = AnchorStyles.Left
        root.Controls.Add(m_DaysLabel, 0, 1)

        m_Days.Minimum = 1D
        m_Days.Maximum = 90D
        m_Days.Value = 7D
        m_Days.DecimalPlaces = 0
        m_Days.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        root.Controls.Add(m_Days, 1, 1)

        m_Explanation.Text = T("FollowUp_ScheduleExplanation",
            "The reminder starts when the Outlook email draft has been prepared.")
        m_Explanation.AutoSize = False
        m_Explanation.Dock = DockStyle.Fill
        m_Explanation.TextAlign = ContentAlignment.MiddleLeft
        root.SetColumnSpan(m_Explanation, 2)
        root.Controls.Add(m_Explanation, 0, 2)

        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False
        }
        Dim cancelButton As New Button With {
            .Text = T("FollowUp_Cancel", "Cancel"),
            .DialogResult = DialogResult.Cancel,
            .AutoSize = True,
            .MinimumSize = New Size(92, 27)
        }
        Dim prepareButton As New Button With {
            .Text = T("FollowUp_PrepareEmail", "Prepare email"),
            .DialogResult = DialogResult.OK,
            .AutoSize = True,
            .MinimumSize = New Size(112, 27)
        }
        buttons.Controls.Add(cancelButton)
        buttons.Controls.Add(prepareButton)
        root.SetColumnSpan(buttons, 2)
        root.Controls.Add(buttons, 0, 3)

        AcceptButton = prepareButton
        CancelButton = cancelButton
        Controls.Add(root)
    End Sub

    Public Shared Function Prompt(owner As IWin32Window) As CLFollowUpScheduleChoice
        Using dialog As New CLFollowUpScheduleDialog()
            If dialog.ShowDialog(owner) <> DialogResult.OK Then
                Return New CLFollowUpScheduleChoice With {.Proceed = False}
            End If
            Return New CLFollowUpScheduleChoice With {
                .Proceed = True,
                .Schedule = dialog.m_Schedule.Checked,
                .Days = Decimal.ToInt32(dialog.m_Days.Value)
            }
        End Using
    End Function

    Private Sub ScheduleChanged(sender As Object, e As EventArgs)
        m_Days.Enabled = m_Schedule.Checked
        m_DaysLabel.Enabled = m_Schedule.Checked
    End Sub

    Private Shared Function T(key As String, fallback As String) As String
        Try
            Dim value As String = CLEnvironment.Current.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> "?" Then Return value
        Catch
        End Try
        Return fallback
    End Function
End Class
