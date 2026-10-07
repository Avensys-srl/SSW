Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms

Partial Public Class CLMainForm

    Private ReadOnly m_FollowUpMenu As New ToolStripMenuItem()
    Private ReadOnly m_FollowUpTimer As New Timer()
    Private m_FollowUpStore As CLFollowUpReminderStore
    Private m_FollowUpSynchronization As CLFollowUpSynchronizationService
    Private m_FollowUpSynchronizationStatus As String
    Private m_FollowUpCenter As CLFollowUpNotificationCenterForm
    Private m_FollowUpPendingOpenPath As String

    Private Sub FollowUp_Initialize()
        m_FollowUpStore = New CLFollowUpReminderStore()
        m_FollowUpSynchronization = New CLFollowUpSynchronizationService(m_FollowUpStore, m_SelectionApiClient)

        m_FollowUpMenu.Alignment = ToolStripItemAlignment.Right
        m_FollowUpMenu.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
        m_FollowUpMenu.ImageScaling = ToolStripItemImageScaling.None
        m_FollowUpMenu.Margin = New Padding(3, 0, 3, 0)
        AddHandler m_FollowUpMenu.Click, AddressOf FollowUp_MenuClick
        mnsMain.Items.Add(m_FollowUpMenu)

        m_FollowUpTimer.Interval = 15 * 60 * 1000
        AddHandler m_FollowUpTimer.Tick, AddressOf FollowUp_TimerTick
        m_FollowUpTimer.Start()

        m_FollowUpSynchronizationStatus = FollowUp_Text("FollowUp_SyncOffline", "Offline - changes saved locally")
        FollowUp_UpdateBell()
        Dim synchronizationTask As Task = FollowUp_SynchronizeBestEffortAsync()
    End Sub

    Private Sub FollowUp_UpdateLocalizedTexts()
        If m_FollowUpStore Is Nothing Then Return
        FollowUp_UpdateBell()
    End Sub

    Private Async Sub FollowUp_EmailPrepared(sender As Object, e As CLFollowUpPreparedEventArgs)
        If e Is Nothing OrElse m_FollowUpStore Is Nothing OrElse e.TargetUuid = Guid.Empty OrElse
            String.IsNullOrWhiteSpace(e.LocalPath) Then Return

        Try
            Dim targetType As CLFollowUpTargetType = If(
                String.Equals(e.TargetType, "Project", StringComparison.OrdinalIgnoreCase),
                CLFollowUpTargetType.Project,
                CLFollowUpTargetType.Selection)
            m_FollowUpStore.CreateLocal(
                targetType,
                e.TargetUuid,
                If(String.IsNullOrWhiteSpace(e.DisplayReference), Path.GetFileNameWithoutExtension(e.LocalPath), e.DisplayReference),
                e.LocalPath,
                e.PreparedAtUtc,
                e.DueAtUtc)
            FollowUp_UpdateBell()
            Await FollowUp_SynchronizeBestEffortAsync()
        Catch ex As Exception
            MessageBox.Show(Me,
                String.Format(FollowUp_Text("MainForm_Project_SaveError", "The follow-up reminder could not be saved: {0}"), ex.Message),
                FollowUp_Text("FollowUp_CenterTitle", "Follow-up reminders"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub FollowUp_LocalPathRequested(sender As Object, e As CLFollowUpLocalPathRequestedEventArgs)
        If e Is Nothing Then Return
        Try
            If m_ProjectDocument Is Nothing Then
                m_ProjectDocument = CLSelectionProjectSerializer.CreateNew(Environment.DatabaseCompatibility)
                m_ProjectDocument.Selection.CustomerCode = Environment.CustomerCode
                m_ProjectDocument.Identity.LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
            End If
            Project_CaptureForm(m_ProjectDocument)

            Dim targetPath As String = m_ProjectFilePath
            If String.IsNullOrWhiteSpace(targetPath) Then
                Dim directoryPath As String = Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "Avensys",
                    "SSW",
                    "FollowUps",
                    "Selections")
                Directory.CreateDirectory(directoryPath)
                targetPath = Path.Combine(directoryPath,
                    m_ProjectDocument.ProjectId.ToString("N") & CLSelectionProjectSerializer.FileExtension)
            End If

            CLSelectionProjectSerializer.Save(targetPath, m_ProjectDocument)
            e.LocalPath = Path.GetFullPath(targetPath)
        Catch ex As Exception
            MessageBox.Show(Me,
                String.Format(FollowUp_Text("MainForm_Project_SaveError", "Unable to save the selection project: {0}"), ex.Message),
                FollowUp_Text("FollowUp_CenterTitle", "Follow-up reminders"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Sub FollowUp_TimerTick(sender As Object, e As EventArgs)
        Await FollowUp_SynchronizeBestEffortAsync()
    End Sub

    Private Async Sub FollowUp_MenuClick(sender As Object, e As EventArgs)
        Await FollowUp_SynchronizeBestEffortAsync()
        FollowUp_ShowCenter()
    End Sub

    Private Async Function FollowUp_SynchronizeBestEffortAsync() As Task
        If m_FollowUpSynchronization Is Nothing Then Return
        Try
            Dim context As CLSelectionRegistrationContext = CLSelectionRegistrationContext.FromEnvironment(Environment)
            Dim result As CLFollowUpSynchronizationResult = Await m_FollowUpSynchronization.SyncBestEffortAsync(context)
            m_FollowUpSynchronizationStatus = If(result.Succeeded,
                FollowUp_Text("FollowUp_SyncCurrent", "Synchronized"),
                FollowUp_Text("FollowUp_SyncOffline", "Offline - changes saved locally"))
        Catch
            m_FollowUpSynchronizationStatus = FollowUp_Text("FollowUp_SyncOffline", "Offline - changes saved locally")
        End Try
        FollowUp_UpdateBell()
        If m_FollowUpCenter IsNot Nothing AndAlso Not m_FollowUpCenter.IsDisposed Then FollowUp_RefreshCenter()
    End Function

    Private Sub FollowUp_ShowCenter()
        m_FollowUpPendingOpenPath = Nothing
        Using center As New CLFollowUpNotificationCenterForm()
            m_FollowUpCenter = center
            AddHandler center.ActionRequested, AddressOf FollowUp_ActionRequested
            AddHandler center.OpenRequested, AddressOf FollowUp_OpenRequested
            AddHandler center.RefreshRequested, AddressOf FollowUp_RefreshRequested
            FollowUp_MarkDueAsRead()
            FollowUp_RefreshCenter()
            center.ShowDialog(Me)
            m_FollowUpCenter = Nothing
        End Using

        If Not String.IsNullOrWhiteSpace(m_FollowUpPendingOpenPath) Then
            Dim path As String = m_FollowUpPendingOpenPath
            m_FollowUpPendingOpenPath = Nothing
            FollowUp_OpenLocalTarget(path)
        End If
    End Sub

    Public Sub FollowUp_ShowNextUiCenter()
        FollowUp_ShowCenter()
    End Sub

    Private Async Sub FollowUp_ActionRequested(sender As Object, e As CLFollowUpActionEventArgs)
        If e Is Nothing OrElse m_FollowUpStore Is Nothing Then Return
        Try
            Select Case e.ActionName
                Case "Succeeded"
                    m_FollowUpStore.CloseLocal(e.ReminderUuid, CLFollowUpReminderStatus.Succeeded)
                Case "Unsuccessful"
                    m_FollowUpStore.CloseLocal(e.ReminderUuid, CLFollowUpReminderStatus.Unsuccessful)
                Case "Cancelled"
                    m_FollowUpStore.CloseLocal(e.ReminderUuid, CLFollowUpReminderStatus.Cancelled)
                Case "Reschedule"
                    If Not e.Days.HasValue Then Return
                    Dim dueUtc As DateTime = CLFollowUpReminderRules.CalculateDueUtc(DateTime.UtcNow, e.Days.Value)
                    m_FollowUpStore.RescheduleLocal(e.ReminderUuid, dueUtc)
                Case Else
                    Return
            End Select
            FollowUp_UpdateBell()
            FollowUp_RefreshCenter()
            Await FollowUp_SynchronizeBestEffortAsync()
        Catch ex As Exception
            MessageBox.Show(m_FollowUpCenter, ex.Message,
                FollowUp_Text("FollowUp_CenterTitle", "Follow-up reminders"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub FollowUp_OpenRequested(sender As Object, e As CLFollowUpActionEventArgs)
        Dim reminder As CLFollowUpReminder = FollowUp_FindReminder(e.ReminderUuid)
        If reminder Is Nothing Then Return
        If String.IsNullOrWhiteSpace(reminder.LocalPath) OrElse Not File.Exists(reminder.LocalPath) Then
            MessageBox.Show(m_FollowUpCenter,
                String.Format(FollowUp_Text("FollowUp_MissingFile", "The local file is no longer available: {0}"), reminder.LocalPath),
                FollowUp_Text("FollowUp_CenterTitle", "Follow-up reminders"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
            Return
        End If
        m_FollowUpPendingOpenPath = reminder.LocalPath
        m_FollowUpCenter.Close()
    End Sub

    Private Async Sub FollowUp_RefreshRequested(sender As Object, e As EventArgs)
        Await FollowUp_SynchronizeBestEffortAsync()
        FollowUp_MarkDueAsRead()
        FollowUp_RefreshCenter()
    End Sub

    Private Sub FollowUp_OpenLocalTarget(path As String)
        Try
            If String.Equals(IO.Path.GetExtension(path), CLMultiSelectionProjectSerializer.FileExtension, StringComparison.OrdinalIgnoreCase) Then
                Dim document As CLMultiSelectionProjectDocument = CLMultiSelectionProjectSerializer.Load(path)
                m_MultiSelectionDocument = document
                m_MultiSelectionPath = IO.Path.GetFullPath(path)
                Using form As New CLMultiSelectionProjectForm(
                    m_MultiSelectionDocument,
                    m_MultiSelectionPath,
                    MultiSelection_Text("MultiProject_DefaultReference", "Project 01"),
                    Environment.PrimaryLanguageCode)
                    AddHandler form.FollowUpPrepared, AddressOf FollowUp_EmailPrepared
                    Dim result As DialogResult = form.ShowDialog(Me)
                    m_MultiSelectionDocument = form.Document
                    m_MultiSelectionPath = form.ProjectPath
                    If result = DialogResult.OK AndAlso form.SelectionToOpen IsNot Nothing Then
                        MultiSelection_OpenSelection(form.SelectionToOpen)
                    End If
                End Using
            Else
                If Project_ConfirmDiscardChanges() Then Project_OpenFile(path)
            End If
        Catch ex As Exception
            MessageBox.Show(Me,
                String.Format(FollowUp_Text("FollowUp_OpenError", "Unable to open the reminder file: {0}"), ex.Message),
                FollowUp_Text("FollowUp_CenterTitle", "Follow-up reminders"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FollowUp_MarkDueAsRead()
        If m_FollowUpStore Is Nothing Then Return
        Dim snapshot As CLFollowUpReminderSnapshot = m_FollowUpStore.LoadSnapshot()
        For Each reminder As CLFollowUpReminder In snapshot.Reminders
            If reminder.Status = CLFollowUpReminderStatus.Pending AndAlso reminder.DueAtUtc <= DateTime.UtcNow AndAlso reminder.IsUnread Then
                m_FollowUpStore.SetRead(reminder.ReminderUuid, True)
            End If
        Next
        FollowUp_UpdateBell()
    End Sub

    Private Sub FollowUp_RefreshCenter()
        If m_FollowUpCenter Is Nothing OrElse m_FollowUpCenter.IsDisposed OrElse m_FollowUpStore Is Nothing Then Return
        Dim snapshot As CLFollowUpReminderSnapshot = m_FollowUpStore.LoadSnapshot()
        Dim items As IEnumerable(Of CLFollowUpNotificationItem) = snapshot.Reminders.Select(
            Function(reminder) New CLFollowUpNotificationItem With {
                .ReminderUuid = reminder.ReminderUuid,
                .DisplayReference = reminder.DisplayReference,
                .TargetType = reminder.TargetType.ToString(),
                .DueAtUtc = reminder.DueAtUtc,
                .Status = reminder.Status.ToString(),
                .RescheduleCount = reminder.RescheduleCount,
                .IsUnread = reminder.IsUnread,
                .LocalPath = reminder.LocalPath
            })
        m_FollowUpCenter.SetItems(items, m_FollowUpSynchronizationStatus)
    End Sub

    Private Function FollowUp_FindReminder(reminderUuid As Guid) As CLFollowUpReminder
        If m_FollowUpStore Is Nothing Then Return Nothing
        Return m_FollowUpStore.LoadSnapshot().Reminders.FirstOrDefault(
            Function(reminder) reminder.ReminderUuid = reminderUuid)
    End Function

    Private Sub FollowUp_UpdateBell()
        If m_FollowUpStore Is Nothing Then Return
        Dim dueCount As Integer = m_FollowUpStore.LoadSnapshot().Reminders.Where(
            Function(reminder) reminder.Status = CLFollowUpReminderStatus.Pending AndAlso reminder.DueAtUtc <= DateTime.UtcNow).Count()
        Dim badge As String = If(dueCount > 9, "9+", dueCount.ToString(Globalization.CultureInfo.InvariantCulture))
        m_FollowUpMenu.Text = If(dueCount > 0, badge, String.Empty)
        m_FollowUpMenu.ToolTipText = If(m_HelpToolTipsEnabled,
            If(dueCount > 0,
                String.Format(FollowUp_Text("FollowUp_BellDue", "{0} follow-up reminders due"), dueCount),
                FollowUp_Text("Help_MenuTip_FollowUps", "Open customer follow-up reminders for selections and projects.")),
            String.Empty)
        Dim oldImage As Image = m_FollowUpMenu.Image
        m_FollowUpMenu.Image = FollowUp_CreateBellImage(dueCount > 0)
        If oldImage IsNot Nothing Then oldImage.Dispose()
    End Sub

    Private Shared Function FollowUp_CreateBellImage(hasDueItems As Boolean) As Bitmap
        Dim bitmap As New Bitmap(20, 20)
        Using graphics As Graphics = Graphics.FromImage(bitmap)
            graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            graphics.Clear(Color.Transparent)
            Dim bellColor As Color = If(hasDueItems, Drawing.Color.FromArgb(214, 45, 57), Drawing.Color.FromArgb(70, 86, 103))
            Using brush As New SolidBrush(bellColor), pen As New Pen(bellColor, 1.7F)
                graphics.DrawArc(pen, 5, 3, 10, 10, 180, 180)
                graphics.DrawLine(pen, 5, 8, 5, 13)
                graphics.DrawLine(pen, 15, 8, 15, 13)
                graphics.DrawLine(pen, 4, 14, 16, 14)
                graphics.FillEllipse(brush, 8, 16, 4, 2)
            End Using
        End Using
        Return bitmap
    End Function

    Private Function FollowUp_Text(key As String, fallback As String) As String
        Try
            Dim value As String = Environment.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> "?" AndAlso Not value.StartsWith("@@", StringComparison.Ordinal) Then Return value
        Catch
        End Try
        Return fallback
    End Function

End Class
