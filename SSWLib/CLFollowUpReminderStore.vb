Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading

Public NotInheritable Class CLFollowUpReminderStore

    Private Const CurrentSchemaVersion As Integer = 1
    Private Shared ReadOnly SerializerOptions As JsonSerializerOptions = CreateSerializerOptions()
    Private ReadOnly m_StateFilePath As String
    Private ReadOnly m_MutexName As String

    Public Sub New(Optional stateFilePath As String = Nothing)
        If String.IsNullOrWhiteSpace(stateFilePath) Then
            stateFilePath = Environment.GetEnvironmentVariable("SSW_FOLLOW_UP_STATE_PATH")
        End If
        If String.IsNullOrWhiteSpace(stateFilePath) Then
            stateFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Avensys",
                "SSW",
                "follow-up-reminders.json")
        End If
        m_StateFilePath = Path.GetFullPath(stateFilePath)
        m_MutexName = "Local\Avensys.SSW.FollowUpReminders." & HashPath(m_StateFilePath)
    End Sub

    Public ReadOnly Property StateFilePath As String
        Get
            Return m_StateFilePath
        End Get
    End Property

    Public ReadOnly Property BackupFilePath As String
        Get
            Return m_StateFilePath & ".bak"
        End Get
    End Property

    Public Function CreateLocal(targetType As CLFollowUpTargetType,
        targetUuid As Guid,
        displayReference As String,
        localPath As String,
        preparedUtc As DateTime,
        dueUtc As DateTime) As CLFollowUpReminder

        If Not [Enum].IsDefined(GetType(CLFollowUpTargetType), targetType) Then
            Throw New ArgumentOutOfRangeException(NameOf(targetType))
        End If
        If targetUuid = Guid.Empty Then Throw New ArgumentException("A target UUID is required.", NameOf(targetUuid))
        If String.IsNullOrWhiteSpace(displayReference) Then
            Throw New ArgumentException("A display reference is required.", NameOf(displayReference))
        End If
        localPath = ValidateLocalPath(localPath)
        preparedUtc = CLFollowUpReminderRules.RequireUtc(preparedUtc, NameOf(preparedUtc))
        dueUtc = CLFollowUpReminderRules.RequireUtc(dueUtc, NameOf(dueUtc))
        CLFollowUpReminderRules.ValidateSchedule(preparedUtc, dueUtc)

        Return WithLock(Function()
            Dim state As CLFollowUpReminderSnapshot = LoadStateWithFallback()
            Dim nowUtc As DateTime = DateTime.UtcNow
            Dim reminder As New CLFollowUpReminder With {
                .ReminderUuid = Guid.NewGuid(),
                .TargetType = targetType,
                .TargetUuid = targetUuid,
                .DisplayReference = displayReference.Trim(),
                .LocalPath = localPath,
                .EmailPreparedAtUtc = preparedUtc,
                .DueAtUtc = dueUtc,
                .Status = CLFollowUpReminderStatus.Pending,
                .CreatedAtUtc = nowUtc,
                .UpdatedAtUtc = nowUtc,
                .IsUnread = True
            }
            state.Reminders.Add(reminder)
            Dim mutation As CLFollowUpPendingMutation = CreateMutation(
                state,
                reminder.ReminderUuid,
                CLFollowUpMutationType.Create)
            mutation.DueAtUtc = reminder.DueAtUtc
            state.PendingMutations.Add(mutation)
            SaveState(state)
            Return Clone(reminder)
        End Function)
    End Function

    Public Function RescheduleLocal(reminderUuid As Guid, dueUtc As DateTime) As CLFollowUpReminder
        If reminderUuid = Guid.Empty Then Throw New ArgumentException("A reminder UUID is required.", NameOf(reminderUuid))
        dueUtc = CLFollowUpReminderRules.RequireUtc(dueUtc, NameOf(dueUtc))
        Return WithLock(Function()
            Dim state As CLFollowUpReminderSnapshot = LoadStateWithFallback()
            Dim reminder As CLFollowUpReminder = FindReminder(state, reminderUuid)
            If reminder.IsClosed Then Throw New InvalidOperationException("A closed follow-up reminder cannot be rescheduled.")
            CLFollowUpReminderRules.ValidateSchedule(DateTime.UtcNow, dueUtc)
            reminder.DueAtUtc = dueUtc
            reminder.RescheduleCount += 1
            reminder.UpdatedAtUtc = DateTime.UtcNow
            reminder.IsUnread = False
            Dim mutation As CLFollowUpPendingMutation = CreateMutation(
                state,
                reminderUuid,
                CLFollowUpMutationType.Reschedule)
            mutation.DueAtUtc = dueUtc
            state.PendingMutations.Add(mutation)
            SaveState(state)
            Return Clone(reminder)
        End Function)
    End Function

    Public Function CloseLocal(reminderUuid As Guid,
        status As CLFollowUpReminderStatus) As CLFollowUpReminder

        If reminderUuid = Guid.Empty Then Throw New ArgumentException("A reminder UUID is required.", NameOf(reminderUuid))
        If Not CLFollowUpReminderRules.IsClosingStatus(status) Then
            Throw New ArgumentException("The follow-up closure status is invalid.", NameOf(status))
        End If
        Return WithLock(Function()
            Dim state As CLFollowUpReminderSnapshot = LoadStateWithFallback()
            Dim reminder As CLFollowUpReminder = FindReminder(state, reminderUuid)
            If reminder.IsClosed Then Throw New InvalidOperationException("The follow-up reminder is already closed.")
            Dim nowUtc As DateTime = DateTime.UtcNow
            reminder.Status = status
            reminder.ClosedAtUtc = nowUtc
            reminder.UpdatedAtUtc = nowUtc
            reminder.IsUnread = False
            Dim mutation As CLFollowUpPendingMutation = CreateMutation(
                state,
                reminderUuid,
                CLFollowUpMutationType.Close)
            mutation.ClosingStatus = status
            state.PendingMutations.Add(mutation)
            SaveState(state)
            Return Clone(reminder)
        End Function)
    End Function

    Public Function LoadSnapshot() As CLFollowUpReminderSnapshot
        Return WithLock(Function()
            Return Clone(LoadStateWithFallback())
        End Function)
    End Function

    Public Sub SetRead(reminderUuid As Guid, isRead As Boolean)
        WithLock(Function()
            Dim state As CLFollowUpReminderSnapshot = LoadStateWithFallback()
            FindReminder(state, reminderUuid).IsUnread = Not isRead
            SaveState(state)
            Return Nothing
        End Function)
    End Sub

    Public Sub RecordMutationFailure(mutationUuid As Guid, diagnostic As String)
        WithLock(Function()
            Dim state As CLFollowUpReminderSnapshot = LoadStateWithFallback()
            Dim mutation As CLFollowUpPendingMutation = state.PendingMutations.FirstOrDefault(
                Function(item) item.MutationUuid = mutationUuid)
            If mutation IsNot Nothing Then
                mutation.RetryCount += 1
                mutation.LastDiagnostic = If(diagnostic, String.Empty)
                SaveState(state)
            End If
            Return Nothing
        End Function)
    End Sub

    Public Sub CompleteMutation(mutationUuid As Guid,
        Optional serverReminder As CLFollowUpReminder = Nothing)

        WithLock(Function()
            Dim state As CLFollowUpReminderSnapshot = LoadStateWithFallback()
            Dim mutation As CLFollowUpPendingMutation = state.PendingMutations.FirstOrDefault(
                Function(item) item.MutationUuid = mutationUuid)
            If mutation Is Nothing Then Return Nothing
            state.PendingMutations.Remove(mutation)
            If serverReminder IsNot Nothing Then
                If state.PendingMutations.Any(Function(item) item.ReminderUuid = mutation.ReminderUuid) Then
                    FindReminder(state, mutation.ReminderUuid).LastSuccessfulSyncAtUtc = DateTime.UtcNow
                Else
                    MergeReminder(state, serverReminder, DateTime.UtcNow)
                End If
            End If
            SaveState(state)
            Return Nothing
        End Function)
    End Sub

    Public Sub MergeIncremental(serverReminders As IEnumerable(Of CLFollowUpReminder),
        incrementalCursorUtc As DateTime?)

        WithLock(Function()
            Dim state As CLFollowUpReminderSnapshot = LoadStateWithFallback()
            Dim syncUtc As DateTime = DateTime.UtcNow
            If serverReminders IsNot Nothing Then
                For Each serverReminder As CLFollowUpReminder In serverReminders
                    MergeReminder(state, serverReminder, syncUtc)
                Next
            End If
            If incrementalCursorUtc.HasValue Then
                Dim cursor As DateTime = CLFollowUpReminderRules.RequireUtc(
                    incrementalCursorUtc.Value,
                    NameOf(incrementalCursorUtc))
                If Not state.LastIncrementalSyncAtUtc.HasValue OrElse cursor > state.LastIncrementalSyncAtUtc.Value Then
                    state.LastIncrementalSyncAtUtc = cursor
                End If
            End If
            SaveState(state)
            Return Nothing
        End Function)
    End Sub

    Private Shared Sub MergeReminder(state As CLFollowUpReminderSnapshot,
        serverReminder As CLFollowUpReminder,
        syncUtc As DateTime)

        ValidateServerReminder(serverReminder)
        Dim local As CLFollowUpReminder = state.Reminders.FirstOrDefault(
            Function(item) item.ReminderUuid = serverReminder.ReminderUuid)
        If local Is Nothing Then
            serverReminder.LocalPath = Nothing
            serverReminder.LastSuccessfulSyncAtUtc = syncUtc
            serverReminder.IsUnread = True
            state.Reminders.Add(Clone(serverReminder))
            Return
        End If

        Dim localPath As String = local.LocalPath
        Dim wasUnread As Boolean = local.IsUnread
        Dim changed As Boolean = serverReminder.UpdatedAtUtc > local.UpdatedAtUtc AndAlso
            (serverReminder.Status <> local.Status OrElse
             serverReminder.DueAtUtc <> local.DueAtUtc OrElse
             serverReminder.RescheduleCount <> local.RescheduleCount)
        If serverReminder.UpdatedAtUtc >= local.UpdatedAtUtc Then
            local.TargetType = serverReminder.TargetType
            local.TargetUuid = serverReminder.TargetUuid
            local.DisplayReference = serverReminder.DisplayReference
            local.EmailPreparedAtUtc = serverReminder.EmailPreparedAtUtc
            local.DueAtUtc = serverReminder.DueAtUtc
            local.Status = serverReminder.Status
            local.RescheduleCount = serverReminder.RescheduleCount
            local.ClosedAtUtc = serverReminder.ClosedAtUtc
            local.CreatedAtUtc = serverReminder.CreatedAtUtc
            local.UpdatedAtUtc = serverReminder.UpdatedAtUtc
        End If
        local.LocalPath = localPath
        local.LastSuccessfulSyncAtUtc = syncUtc
        local.IsUnread = wasUnread OrElse changed
    End Sub

    Private Shared Function CreateMutation(state As CLFollowUpReminderSnapshot,
        reminderUuid As Guid,
        mutationType As CLFollowUpMutationType) As CLFollowUpPendingMutation

        If state.LastSequence = Long.MaxValue Then Throw New OverflowException("The follow-up mutation sequence is exhausted.")
        state.LastSequence += 1
        Return New CLFollowUpPendingMutation With {
            .MutationUuid = Guid.NewGuid(),
            .Sequence = state.LastSequence,
            .ReminderUuid = reminderUuid,
            .MutationType = mutationType,
            .IdempotencyKey = "ssw-followup-" & Guid.NewGuid().ToString("N"),
            .CreatedAtUtc = DateTime.UtcNow
        }
    End Function

    Private Shared Function FindReminder(state As CLFollowUpReminderSnapshot,
        reminderUuid As Guid) As CLFollowUpReminder

        Dim reminder As CLFollowUpReminder = state.Reminders.FirstOrDefault(
            Function(item) item.ReminderUuid = reminderUuid)
        If reminder Is Nothing Then Throw New KeyNotFoundException("The follow-up reminder was not found.")
        Return reminder
    End Function

    Private Function LoadStateWithFallback() As CLFollowUpReminderSnapshot
        Dim state As CLFollowUpReminderSnapshot = Nothing
        If TryLoadState(m_StateFilePath, state) Then Return state

        Dim currentExists As Boolean = File.Exists(m_StateFilePath)
        If currentExists Then Quarantine(m_StateFilePath)
        If TryLoadState(BackupFilePath, state) Then
            SaveState(state)
            Return state
        End If
        If File.Exists(BackupFilePath) Then Quarantine(BackupFilePath)
        state = New CLFollowUpReminderSnapshot()
        If currentExists Then SaveState(state)
        Return state
    End Function

    Private Shared Function TryLoadState(filePath As String,
        ByRef state As CLFollowUpReminderSnapshot) As Boolean

        If Not File.Exists(filePath) Then Return False
        Try
            state = JsonSerializer.Deserialize(Of CLFollowUpReminderSnapshot)(
                File.ReadAllText(filePath, Encoding.UTF8),
                SerializerOptions)
            ValidateState(state)
            Return True
        Catch ex As Exception When TypeOf ex Is JsonException OrElse
            TypeOf ex Is InvalidDataException OrElse
            TypeOf ex Is ArgumentException
            state = Nothing
            Return False
        End Try
    End Function

    Private Sub SaveState(state As CLFollowUpReminderSnapshot)
        ValidateState(state)
        Dim directoryPath As String = Path.GetDirectoryName(m_StateFilePath)
        Directory.CreateDirectory(directoryPath)
        Dim temporaryPath As String = m_StateFilePath & ".tmp-" & Guid.NewGuid().ToString("N")
        Try
            File.WriteAllText(temporaryPath,
                JsonSerializer.Serialize(state, SerializerOptions),
                New UTF8Encoding(False))
            If File.Exists(m_StateFilePath) Then
                File.Replace(temporaryPath, m_StateFilePath, BackupFilePath, True)
            Else
                File.Move(temporaryPath, m_StateFilePath)
            End If
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
        End Try
    End Sub

    Private Shared Sub ValidateState(state As CLFollowUpReminderSnapshot)
        If state Is Nothing OrElse state.SchemaVersion <> CurrentSchemaVersion Then
            Throw New InvalidDataException("Unsupported follow-up reminder state schema.")
        End If
        If state.Reminders Is Nothing Then state.Reminders = New List(Of CLFollowUpReminder)()
        If state.PendingMutations Is Nothing Then state.PendingMutations = New List(Of CLFollowUpPendingMutation)()
        If state.LastSequence < 0 Then Throw New InvalidDataException("The follow-up sequence is invalid.")
        If state.Reminders.GroupBy(Function(item) item.ReminderUuid).Any(Function(group) group.Count() <> 1) Then
            Throw New InvalidDataException("Duplicate follow-up reminder UUID.")
        End If
        For Each reminder As CLFollowUpReminder In state.Reminders
            ValidateLocalReminder(reminder)
        Next
        Dim previousSequence As Long
        For Each mutation As CLFollowUpPendingMutation In state.PendingMutations.OrderBy(Function(item) item.Sequence)
            If mutation Is Nothing OrElse mutation.MutationUuid = Guid.Empty OrElse
                mutation.ReminderUuid = Guid.Empty OrElse mutation.Sequence <= previousSequence OrElse
                String.IsNullOrWhiteSpace(mutation.IdempotencyKey) Then
                Throw New InvalidDataException("The follow-up mutation queue is invalid.")
            End If
            If Not state.Reminders.Any(Function(item) item.ReminderUuid = mutation.ReminderUuid) Then
                Throw New InvalidDataException("A follow-up mutation references a missing reminder.")
            End If
            previousSequence = mutation.Sequence
        Next
        If state.PendingMutations.Count > 0 AndAlso state.LastSequence < previousSequence Then
            Throw New InvalidDataException("The follow-up sequence is inconsistent.")
        End If
    End Sub

    Private Shared Sub ValidateLocalReminder(reminder As CLFollowUpReminder)
        ValidateServerReminder(reminder)
        If Not String.IsNullOrWhiteSpace(reminder.LocalPath) AndAlso Not Path.IsPathRooted(reminder.LocalPath) Then
            Throw New InvalidDataException("A follow-up local path must be absolute.")
        End If
    End Sub

    Private Shared Sub ValidateServerReminder(reminder As CLFollowUpReminder)
        If reminder Is Nothing OrElse reminder.ReminderUuid = Guid.Empty OrElse reminder.TargetUuid = Guid.Empty OrElse
            Not [Enum].IsDefined(GetType(CLFollowUpTargetType), reminder.TargetType) OrElse
            Not [Enum].IsDefined(GetType(CLFollowUpReminderStatus), reminder.Status) OrElse
            String.IsNullOrWhiteSpace(reminder.DisplayReference) OrElse reminder.RescheduleCount < 0 Then
            Throw New InvalidDataException("The follow-up reminder is invalid.")
        End If
        reminder.EmailPreparedAtUtc = CLFollowUpReminderRules.RequireUtc(reminder.EmailPreparedAtUtc, "EmailPreparedAtUtc")
        reminder.DueAtUtc = CLFollowUpReminderRules.RequireUtc(reminder.DueAtUtc, "DueAtUtc")
        reminder.CreatedAtUtc = CLFollowUpReminderRules.RequireUtc(reminder.CreatedAtUtc, "CreatedAtUtc")
        reminder.UpdatedAtUtc = CLFollowUpReminderRules.RequireUtc(reminder.UpdatedAtUtc, "UpdatedAtUtc")
        If reminder.ClosedAtUtc.HasValue Then
            reminder.ClosedAtUtc = CLFollowUpReminderRules.RequireUtc(reminder.ClosedAtUtc.Value, "ClosedAtUtc")
        End If
    End Sub

    Private Shared Function ValidateLocalPath(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Throw New ArgumentException("A local selection or project path is required.", NameOf(value))
        Dim result As String = Path.GetFullPath(value)
        If Not Path.IsPathRooted(result) Then Throw New ArgumentException("The local path must be absolute.", NameOf(value))
        Return result
    End Function

    Private Shared Sub Quarantine(filePath As String)
        Dim quarantinePath As String = filePath & ".corrupt-" &
            DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", Globalization.CultureInfo.InvariantCulture) & "-" &
            Guid.NewGuid().ToString("N")
        File.Move(filePath, quarantinePath)
    End Sub

    Private Function WithLock(Of TResult)(action As Func(Of TResult)) As TResult
        Using mutex As New Mutex(False, m_MutexName)
            Dim acquired As Boolean
            Try
                Try
                    acquired = mutex.WaitOne(TimeSpan.FromSeconds(15))
                Catch ex As AbandonedMutexException
                    acquired = True
                End Try
                If Not acquired Then Throw New TimeoutException("The local follow-up reminder store is busy.")
                Return action()
            Finally
                If acquired Then mutex.ReleaseMutex()
            End Try
        End Using
    End Function

    Private Shared Function Clone(Of T)(value As T) As T
        Return JsonSerializer.Deserialize(Of T)(JsonSerializer.Serialize(value, SerializerOptions), SerializerOptions)
    End Function

    Private Shared Function HashPath(value As String) As String
        Using algorithm As SHA256 = SHA256.Create()
            Return String.Concat(algorithm.ComputeHash(Encoding.UTF8.GetBytes(value.ToUpperInvariant())).
                Take(12).Select(Function(item) item.ToString("x2")))
        End Using
    End Function

    Private Shared Function CreateSerializerOptions() As JsonSerializerOptions
        Dim options As New JsonSerializerOptions With {
            .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            .PropertyNameCaseInsensitive = True,
            .WriteIndented = True
        }
        options.Converters.Add(New JsonStringEnumConverter())
        Return options
    End Function

End Class
