Imports System.Globalization

Public Enum CLFollowUpReminderStatus
    Pending = 0
    Succeeded = 1
    Unsuccessful = 2
    Cancelled = 3
End Enum

Public Enum CLFollowUpTargetType
    Selection = 0
    Project = 1
End Enum

Public Enum CLFollowUpMutationType
    Create = 0
    Reschedule = 1
    Close = 2
End Enum

Public NotInheritable Class CLFollowUpReminder

    Public Property ReminderUuid As Guid
    Public Property TargetType As CLFollowUpTargetType
    Public Property TargetUuid As Guid
    Public Property DisplayReference As String
    Public Property LocalPath As String
    Public Property EmailPreparedAtUtc As DateTime
    Public Property DueAtUtc As DateTime
    Public Property Status As CLFollowUpReminderStatus = CLFollowUpReminderStatus.Pending
    Public Property RescheduleCount As Integer
    Public Property ClosedAtUtc As DateTime?
    Public Property CreatedAtUtc As DateTime
    Public Property UpdatedAtUtc As DateTime
    Public Property LastSuccessfulSyncAtUtc As DateTime?
    Public Property IsUnread As Boolean = True

    Public ReadOnly Property IsClosed As Boolean
        Get
            Return Status <> CLFollowUpReminderStatus.Pending
        End Get
    End Property

End Class

Public NotInheritable Class CLFollowUpPendingMutation

    Public Property MutationUuid As Guid
    Public Property Sequence As Long
    Public Property ReminderUuid As Guid
    Public Property MutationType As CLFollowUpMutationType
    Public Property IdempotencyKey As String
    Public Property DueAtUtc As DateTime?
    Public Property ClosingStatus As CLFollowUpReminderStatus?
    Public Property CreatedAtUtc As DateTime
    Public Property RetryCount As Integer
    Public Property LastDiagnostic As String

End Class

Public NotInheritable Class CLFollowUpReminderSnapshot

    Public Property SchemaVersion As Integer = 1
    Public Property LastSequence As Long
    Public Property LastIncrementalSyncAtUtc As DateTime?
    Public Property Reminders As New List(Of CLFollowUpReminder)()
    Public Property PendingMutations As New List(Of CLFollowUpPendingMutation)()

End Class

Public NotInheritable Class CLFollowUpReminderListResult

    Public Property Reminders As New List(Of CLFollowUpReminder)()
    Public Property IncrementalCursorUtc As DateTime?
    Friend Property HasMore As Boolean
    Friend Property NextUpdatedSinceUtc As DateTime?
    Friend Property NextUpdatedAfterId As Long?
    Friend Property ServerTimeUtc As DateTime?

End Class

Public NotInheritable Class CLFollowUpSynchronizationResult

    Public Property Succeeded As Boolean
    Public Property UploadedMutationCount As Integer
    Public Property DownloadedReminderCount As Integer
    Public Property Diagnostic As String

End Class

Public NotInheritable Class CLFollowUpReminderRules

    Public Const MinimumDelayDays As Integer = 1
    Public Const MaximumDelayDays As Integer = 90

    Private Sub New()
    End Sub

    Public Shared Sub ValidateDelayDays(delayDays As Integer)
        If delayDays < MinimumDelayDays OrElse delayDays > MaximumDelayDays Then
            Throw New ArgumentOutOfRangeException(NameOf(delayDays),
                String.Format(CultureInfo.InvariantCulture,
                    "The follow-up delay must be between {0} and {1} days.",
                    MinimumDelayDays,
                    MaximumDelayDays))
        End If
    End Sub

    Public Shared Function CalculateDueUtc(preparedUtc As DateTime,
        delayDays As Integer,
        Optional timeZone As TimeZoneInfo = Nothing) As DateTime

        ValidateDelayDays(delayDays)
        preparedUtc = RequireUtc(preparedUtc, NameOf(preparedUtc))
        Dim effectiveTimeZone As TimeZoneInfo = If(timeZone, TimeZoneInfo.Local)
        Dim localPrepared As DateTime = TimeZoneInfo.ConvertTimeFromUtc(preparedUtc, effectiveTimeZone)
        Dim localDue As DateTime = DateTime.SpecifyKind(localPrepared.AddDays(delayDays), DateTimeKind.Unspecified)
        If effectiveTimeZone.IsInvalidTime(localDue) Then
            localDue = localDue.AddHours(1)
        End If
        Return TimeZoneInfo.ConvertTimeToUtc(localDue, effectiveTimeZone)
    End Function

    Public Shared Sub ValidateSchedule(preparedUtc As DateTime, dueUtc As DateTime)
        preparedUtc = RequireUtc(preparedUtc, NameOf(preparedUtc))
        dueUtc = RequireUtc(dueUtc, NameOf(dueUtc))
        Dim elapsed As TimeSpan = dueUtc - preparedUtc
        If elapsed < TimeSpan.FromHours(23) OrElse elapsed > TimeSpan.FromHours(24 * MaximumDelayDays + 2) Then
            Throw New ArgumentOutOfRangeException(NameOf(dueUtc),
                "The follow-up due date must represent a delay from 1 through 90 local calendar days.")
        End If
    End Sub

    Public Shared Function RequireUtc(value As DateTime, parameterName As String) As DateTime
        If value.Kind = DateTimeKind.Local Then Return value.ToUniversalTime()
        If value.Kind <> DateTimeKind.Utc Then
            Throw New ArgumentException("A UTC timestamp is required.", parameterName)
        End If
        Return value
    End Function

    Public Shared Function IsClosingStatus(status As CLFollowUpReminderStatus) As Boolean
        Return status = CLFollowUpReminderStatus.Succeeded OrElse
            status = CLFollowUpReminderStatus.Unsuccessful OrElse
            status = CLFollowUpReminderStatus.Cancelled
    End Function

End Class
