Imports System.Threading
Imports System.IO

Public NotInheritable Class CLFollowUpSynchronizationService

    Private ReadOnly m_Store As CLFollowUpReminderStore
    Private ReadOnly m_ApiClient As CLSelectionApiClient
    Private ReadOnly m_SynchronizationGate As New SemaphoreSlim(1, 1)

    Public Sub New(store As CLFollowUpReminderStore,
        Optional apiClient As CLSelectionApiClient = Nothing)

        If store Is Nothing Then Throw New ArgumentNullException(NameOf(store))
        m_Store = store
        m_ApiClient = If(apiClient, New CLSelectionApiClient())
    End Sub

    Public Async Function SyncBestEffortAsync(context As CLSelectionRegistrationContext,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLFollowUpSynchronizationResult)

        Dim result As New CLFollowUpSynchronizationResult()
        Await m_SynchronizationGate.WaitAsync(cancellationToken).ConfigureAwait(False)
        Try
            Dim snapshot As CLFollowUpReminderSnapshot = m_Store.LoadSnapshot()
            For Each mutation As CLFollowUpPendingMutation In snapshot.PendingMutations.OrderBy(Function(item) item.Sequence)
                cancellationToken.ThrowIfCancellationRequested()
                Dim reminder As CLFollowUpReminder = snapshot.Reminders.FirstOrDefault(
                    Function(item) item.ReminderUuid = mutation.ReminderUuid)
                If reminder Is Nothing Then
                    Throw New InvalidDataException("The follow-up mutation references a missing reminder.")
                End If

                Try
                    Dim serverReminder As CLFollowUpReminder = Nothing
                    Select Case mutation.MutationType
                        Case CLFollowUpMutationType.Create
                            If Not mutation.DueAtUtc.HasValue Then
                                Throw New InvalidDataException("The queued creation has no original due date.")
                            End If
                            Dim createReminder As New CLFollowUpReminder With {
                                .ReminderUuid = reminder.ReminderUuid,
                                .TargetType = reminder.TargetType,
                                .TargetUuid = reminder.TargetUuid,
                                .DisplayReference = reminder.DisplayReference,
                                .EmailPreparedAtUtc = reminder.EmailPreparedAtUtc,
                                .DueAtUtc = mutation.DueAtUtc.Value,
                                .Status = CLFollowUpReminderStatus.Pending,
                                .CreatedAtUtc = reminder.CreatedAtUtc,
                                .UpdatedAtUtc = reminder.CreatedAtUtc
                            }
                            serverReminder = Await m_ApiClient.CreateFollowUpAsync(
                                createReminder,
                                mutation.IdempotencyKey,
                                context,
                                cancellationToken).ConfigureAwait(False)
                        Case CLFollowUpMutationType.Reschedule
                            If Not mutation.DueAtUtc.HasValue Then
                                Throw New InvalidDataException("The queued reschedule has no due date.")
                            End If
                            serverReminder = Await m_ApiClient.RescheduleFollowUpAsync(
                                reminder.ReminderUuid,
                                mutation.DueAtUtc.Value,
                                mutation.IdempotencyKey,
                                context,
                                cancellationToken).ConfigureAwait(False)
                        Case CLFollowUpMutationType.Close
                            If Not mutation.ClosingStatus.HasValue Then
                                Throw New InvalidDataException("The queued closure has no outcome.")
                            End If
                            serverReminder = Await m_ApiClient.CloseFollowUpAsync(
                                reminder.ReminderUuid,
                                mutation.ClosingStatus.Value,
                                mutation.IdempotencyKey,
                                context,
                                cancellationToken).ConfigureAwait(False)
                        Case Else
                            Throw New InvalidDataException("The queued follow-up operation is invalid.")
                    End Select
                    m_Store.CompleteMutation(mutation.MutationUuid, serverReminder)
                    result.UploadedMutationCount += 1
                    snapshot = m_Store.LoadSnapshot()
                Catch ex As OperationCanceledException
                    Throw
                Catch ex As Exception
                    m_Store.RecordMutationFailure(mutation.MutationUuid, ex.Message)
                    result.Diagnostic = ex.Message
                    result.Succeeded = False
                    Return result
                End Try
            Next

            snapshot = m_Store.LoadSnapshot()
            Dim listResult As CLFollowUpReminderListResult = Await m_ApiClient.ListFollowUpsAsync(
                context,
                snapshot.LastIncrementalSyncAtUtc,
                cancellationToken).ConfigureAwait(False)
            m_Store.MergeIncremental(listResult.Reminders, listResult.IncrementalCursorUtc)
            result.DownloadedReminderCount = listResult.Reminders.Count
            result.Succeeded = True
            Return result
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            result.Succeeded = False
            result.Diagnostic = ex.Message
            Return result
        Finally
            m_SynchronizationGate.Release()
        End Try
    End Function

End Class
