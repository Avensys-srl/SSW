Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.Json
Imports System.Threading

Partial Public NotInheritable Class CLSelectionApiClient

    Public Async Function CreateFollowUpAsync(reminder As CLFollowUpReminder,
        idempotencyKey As String,
        context As CLSelectionRegistrationContext,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLFollowUpReminder)

        ValidateFollowUpRequest(reminder, idempotencyKey)
        Return Await SendAuthenticatedFollowUpAsync(
            context,
            Function(accessToken) SendCreateFollowUpAsync(reminder, idempotencyKey, accessToken, cancellationToken),
            cancellationToken).ConfigureAwait(False)
    End Function

    Public Async Function RescheduleFollowUpAsync(reminderUuid As Guid,
        dueAtUtc As DateTime,
        idempotencyKey As String,
        context As CLSelectionRegistrationContext,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLFollowUpReminder)

        If reminderUuid = Guid.Empty Then Throw New ArgumentException("A reminder UUID is required.", NameOf(reminderUuid))
        dueAtUtc = CLFollowUpReminderRules.RequireUtc(dueAtUtc, NameOf(dueAtUtc))
        ValidateIdempotencyKey(idempotencyKey)
        Dim rescheduledAtUtc As DateTime = DateTime.UtcNow
        Return Await SendAuthenticatedFollowUpAsync(
            context,
            Function(accessToken) SendFollowUpMutationAsync(
                reminderUuid,
                "reschedule",
                New Dictionary(Of String, Object) From {
                    {"rescheduled_at_utc", FormatUtc(rescheduledAtUtc)},
                    {"due_at_utc", FormatUtc(dueAtUtc)},
                    {"follow_up_days", FollowUpDays(rescheduledAtUtc, dueAtUtc)}
                },
                idempotencyKey,
                accessToken,
                cancellationToken),
            cancellationToken).ConfigureAwait(False)
    End Function

    Public Async Function CloseFollowUpAsync(reminderUuid As Guid,
        status As CLFollowUpReminderStatus,
        idempotencyKey As String,
        context As CLSelectionRegistrationContext,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLFollowUpReminder)

        If reminderUuid = Guid.Empty Then Throw New ArgumentException("A reminder UUID is required.", NameOf(reminderUuid))
        If Not CLFollowUpReminderRules.IsClosingStatus(status) Then
            Throw New ArgumentException("The follow-up closure status is invalid.", NameOf(status))
        End If
        ValidateIdempotencyKey(idempotencyKey)
        Return Await SendAuthenticatedFollowUpAsync(
            context,
            Function(accessToken) SendFollowUpMutationAsync(
                reminderUuid,
                "close",
                New Dictionary(Of String, Object) From {
                    {"status", status.ToString()}
                },
                idempotencyKey,
                accessToken,
                cancellationToken),
            cancellationToken).ConfigureAwait(False)
    End Function

    Public Async Function ListFollowUpsAsync(context As CLSelectionRegistrationContext,
        Optional updatedSinceUtc As DateTime? = Nothing,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLFollowUpReminderListResult)

        If updatedSinceUtc.HasValue Then
            updatedSinceUtc = CLFollowUpReminderRules.RequireUtc(updatedSinceUtc.Value, NameOf(updatedSinceUtc))
        End If
        Return Await SendAuthenticatedFollowUpAsync(
            context,
            Function(accessToken) SendListFollowUpsAsync(updatedSinceUtc, accessToken, cancellationToken),
            cancellationToken).ConfigureAwait(False)
    End Function

    Private Async Function SendCreateFollowUpAsync(reminder As CLFollowUpReminder,
        idempotencyKey As String,
        accessToken As String,
        cancellationToken As CancellationToken) As Task(Of CLFollowUpReminder)

        Dim payload As New Dictionary(Of String, Object) From {
            {"reminder_id", reminder.ReminderUuid.ToString("D")},
            {"target_type", reminder.TargetType.ToString()},
            {"target_id", reminder.TargetUuid.ToString("D")},
            {"display_reference", reminder.DisplayReference},
            {"email_prepared_at_utc", FormatUtc(reminder.EmailPreparedAtUtc)},
            {"due_at_utc", FormatUtc(reminder.DueAtUtc)},
            {"follow_up_days", FollowUpDays(reminder.EmailPreparedAtUtc, reminder.DueAtUtc)}
        }
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri("follow-ups"))
            AddFollowUpHeaders(request, accessToken, idempotencyKey)
            request.Content = New StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
            Return Await SendFollowUpRequestAsync(request, cancellationToken).ConfigureAwait(False)
        End Using
    End Function

    Private Async Function SendFollowUpMutationAsync(reminderUuid As Guid,
        operation As String,
        payload As Dictionary(Of String, Object),
        idempotencyKey As String,
        accessToken As String,
        cancellationToken As CancellationToken) As Task(Of CLFollowUpReminder)

        Dim relativePath As String = "follow-ups/" & reminderUuid.ToString("D") & "/" & operation
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri(relativePath))
            AddFollowUpHeaders(request, accessToken, idempotencyKey)
            request.Content = New StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
            Return Await SendFollowUpRequestAsync(request, cancellationToken).ConfigureAwait(False)
        End Using
    End Function

    Private Async Function SendFollowUpRequestAsync(request As HttpRequestMessage,
        cancellationToken As CancellationToken) As Task(Of CLFollowUpReminder)

        Using response As HttpResponseMessage = Await m_HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(False)
            Dim body As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
            If Not response.IsSuccessStatusCode Then Throw CreateApiException(response.StatusCode, body)
            Return ParseFollowUpResponse(body)
        End Using
    End Function

    Private Async Function SendListFollowUpsAsync(updatedSinceUtc As DateTime?,
        accessToken As String,
        cancellationToken As CancellationToken) As Task(Of CLFollowUpReminderListResult)

        Dim result As New CLFollowUpReminderListResult()
        Dim cursorUtc As DateTime? = updatedSinceUtc
        Dim cursorId As Long?
        Do
            Dim relativePath As String = "follow-ups?scope=all&limit=200"
            If cursorUtc.HasValue Then
                relativePath &= "&updated_since=" & Uri.EscapeDataString(FormatUtc(cursorUtc.Value))
                If cursorId.HasValue Then
                    relativePath &= "&updated_after_id=" & cursorId.Value.ToString(CultureInfo.InvariantCulture)
                End If
            End If
            Using request As New HttpRequestMessage(HttpMethod.Get, BuildUri(relativePath))
                request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", accessToken)
                Using response As HttpResponseMessage = Await m_HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(False)
                    Dim body As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                    If Not response.IsSuccessStatusCode Then Throw CreateApiException(response.StatusCode, body)
                    Dim page As CLFollowUpReminderListResult = ParseFollowUpListResponse(body)
                    result.Reminders.AddRange(page.Reminders)
                    result.ServerTimeUtc = page.ServerTimeUtc
                    If Not page.HasMore Then Exit Do
                    If Not page.NextUpdatedSinceUtc.HasValue OrElse Not page.NextUpdatedAfterId.HasValue Then
                        Throw New InvalidDataException("The follow-up API returned an incomplete pagination cursor.")
                    End If
                    cursorUtc = page.NextUpdatedSinceUtc
                    cursorId = page.NextUpdatedAfterId
                End Using
            End Using
        Loop
        result.IncrementalCursorUtc = result.ServerTimeUtc
        If Not result.IncrementalCursorUtc.HasValue AndAlso result.Reminders.Count > 0 Then
            result.IncrementalCursorUtc = result.Reminders.Max(Function(item) item.UpdatedAtUtc)
        End If
        Return result
    End Function

    Private Async Function SendAuthenticatedFollowUpAsync(Of TResult)(context As CLSelectionRegistrationContext,
        operation As Func(Of String, Task(Of TResult)),
        cancellationToken As CancellationToken) As Task(Of TResult)

        ValidateContext(context)
        Dim accessToken As String = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Try
            Return Await operation(accessToken).ConfigureAwait(False)
        Catch ex As CLSelectionApiException When ex.StatusCode = HttpStatusCode.Unauthorized
            CLSelectionCredentialStore.ClearAccessToken()
        End Try
        accessToken = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Return Await operation(accessToken).ConfigureAwait(False)
    End Function

    Private Shared Sub AddFollowUpHeaders(request As HttpRequestMessage,
        accessToken As String,
        idempotencyKey As String)

        request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", accessToken)
        request.Headers.Add("Idempotency-Key", idempotencyKey)
    End Sub

    Private Shared Sub ValidateFollowUpRequest(reminder As CLFollowUpReminder, idempotencyKey As String)
        If reminder Is Nothing OrElse reminder.ReminderUuid = Guid.Empty OrElse reminder.TargetUuid = Guid.Empty OrElse
            String.IsNullOrWhiteSpace(reminder.DisplayReference) Then
            Throw New ArgumentException("The follow-up reminder is invalid.", NameOf(reminder))
        End If
        If reminder.Status <> CLFollowUpReminderStatus.Pending Then
            Throw New ArgumentException("A new follow-up reminder must be pending.", NameOf(reminder))
        End If
        CLFollowUpReminderRules.ValidateSchedule(reminder.EmailPreparedAtUtc, reminder.DueAtUtc)
        ValidateIdempotencyKey(idempotencyKey)
    End Sub

    Private Shared Sub ValidateIdempotencyKey(value As String)
        If String.IsNullOrWhiteSpace(value) OrElse value.Length > 128 Then
            Throw New ArgumentException("A valid idempotency key is required.", NameOf(value))
        End If
    End Sub

    Private Shared Function ParseFollowUpResponse(body As String) As CLFollowUpReminder
        Using document As JsonDocument = JsonDocument.Parse(body)
            Dim element As JsonElement = document.RootElement
            Dim nested As JsonElement
            If element.ValueKind = JsonValueKind.Object AndAlso element.TryGetProperty("reminder", nested) Then
                element = nested
            End If
            Return ParseFollowUpReminder(element)
        End Using
    End Function

    Private Shared Function ParseFollowUpListResponse(body As String) As CLFollowUpReminderListResult
        Using document As JsonDocument = JsonDocument.Parse(body)
            Dim root As JsonElement = document.RootElement
            Dim items As JsonElement = root
            Dim propertyValue As JsonElement
            If root.ValueKind = JsonValueKind.Object Then
                If root.TryGetProperty("items", propertyValue) Then
                    items = propertyValue
                ElseIf root.TryGetProperty("reminders", propertyValue) Then
                    items = propertyValue
                Else
                    Throw New InvalidDataException("The follow-up API returned no reminder list.")
                End If
            End If
            If items.ValueKind <> JsonValueKind.Array Then
                Throw New InvalidDataException("The follow-up API returned an invalid reminder list.")
            End If
            Dim result As New CLFollowUpReminderListResult()
            For Each item As JsonElement In items.EnumerateArray()
                result.Reminders.Add(ParseFollowUpReminder(item))
            Next
            If root.ValueKind = JsonValueKind.Object Then
                If root.TryGetProperty("server_time_utc", propertyValue) Then
                    result.ServerTimeUtc = ParseUtc(propertyValue.GetString(), "server timestamp")
                ElseIf root.TryGetProperty("incremental_cursor_utc", propertyValue) OrElse
                    root.TryGetProperty("updated_since", propertyValue) Then
                    result.IncrementalCursorUtc = ParseUtc(propertyValue.GetString(), "incremental cursor")
                End If
                If root.TryGetProperty("has_more", propertyValue) AndAlso
                    (propertyValue.ValueKind = JsonValueKind.True OrElse propertyValue.ValueKind = JsonValueKind.False) Then
                    result.HasMore = propertyValue.GetBoolean()
                End If
                If root.TryGetProperty("next_updated_since", propertyValue) AndAlso propertyValue.ValueKind = JsonValueKind.String Then
                    result.NextUpdatedSinceUtc = ParseUtc(propertyValue.GetString(), "next updated timestamp")
                End If
                If root.TryGetProperty("next_updated_after_id", propertyValue) AndAlso propertyValue.ValueKind = JsonValueKind.Number Then
                    Dim nextId As Long
                    If propertyValue.TryGetInt64(nextId) AndAlso nextId >= 0 Then result.NextUpdatedAfterId = nextId
                End If
            End If
            If Not result.IncrementalCursorUtc.HasValue AndAlso result.Reminders.Count > 0 Then
                result.IncrementalCursorUtc = result.Reminders.Max(Function(item) item.UpdatedAtUtc)
            End If
            Return result
        End Using
    End Function

    Private Shared Function ParseFollowUpReminder(element As JsonElement) As CLFollowUpReminder
        If element.ValueKind <> JsonValueKind.Object Then
            Throw New InvalidDataException("The follow-up API returned an invalid reminder.")
        End If
        Dim reminderUuid As Guid = ParseGuid(GetRequiredStringEither(element, "reminder_id", "reminder_uuid"), "reminder UUID")
        Dim targetUuid As Guid = ParseGuid(GetRequiredStringEither(element, "target_id", "target_uuid"), "target UUID")
        Dim targetType As CLFollowUpTargetType
        If Not [Enum].TryParse(GetRequiredString(element, "target_type"), True, targetType) OrElse
            Not [Enum].IsDefined(GetType(CLFollowUpTargetType), targetType) Then
            Throw New InvalidDataException("The follow-up API returned an invalid target type.")
        End If
        Dim status As CLFollowUpReminderStatus
        If Not [Enum].TryParse(GetRequiredString(element, "status"), True, status) OrElse
            Not [Enum].IsDefined(GetType(CLFollowUpReminderStatus), status) Then
            Throw New InvalidDataException("The follow-up API returned an invalid status.")
        End If
        Dim result As New CLFollowUpReminder With {
            .ReminderUuid = reminderUuid,
            .TargetType = targetType,
            .TargetUuid = targetUuid,
            .DisplayReference = GetRequiredString(element, "display_reference"),
            .EmailPreparedAtUtc = ParseUtc(GetRequiredString(element, "email_prepared_at_utc"), "prepared timestamp"),
            .DueAtUtc = ParseUtc(GetRequiredString(element, "due_at_utc"), "due timestamp"),
            .Status = status,
            .RescheduleCount = GetOptionalInteger(element, "reschedule_count"),
            .CreatedAtUtc = ParseUtc(GetRequiredStringEither(element, "created_at_utc", "created_at"), "created timestamp"),
            .UpdatedAtUtc = ParseUtc(GetRequiredStringEither(element, "updated_at_utc", "updated_at"), "updated timestamp")
        }
        Dim closedValue As JsonElement
        If element.TryGetProperty("closed_at_utc", closedValue) AndAlso closedValue.ValueKind <> JsonValueKind.Null Then
            result.ClosedAtUtc = ParseUtc(closedValue.GetString(), "closed timestamp")
        End If
        Return result
    End Function

    Private Shared Function GetRequiredString(element As JsonElement, propertyName As String) As String
        Dim value As JsonElement
        If Not element.TryGetProperty(propertyName, value) OrElse value.ValueKind <> JsonValueKind.String OrElse
            String.IsNullOrWhiteSpace(value.GetString()) Then
            Throw New InvalidDataException("The follow-up API response is missing " & propertyName & ".")
        End If
        Return value.GetString()
    End Function

    Private Shared Function GetRequiredStringEither(element As JsonElement,
        primaryPropertyName As String,
        legacyPropertyName As String) As String

        Dim value As JsonElement
        If element.TryGetProperty(primaryPropertyName, value) AndAlso value.ValueKind = JsonValueKind.String AndAlso
            Not String.IsNullOrWhiteSpace(value.GetString()) Then Return value.GetString()
        Return GetRequiredString(element, legacyPropertyName)
    End Function

    Private Shared Function GetOptionalInteger(element As JsonElement, propertyName As String) As Integer
        Dim value As JsonElement
        If Not element.TryGetProperty(propertyName, value) OrElse value.ValueKind = JsonValueKind.Null Then Return 0
        Dim result As Integer
        If value.ValueKind <> JsonValueKind.Number OrElse Not value.TryGetInt32(result) OrElse result < 0 Then
            Throw New InvalidDataException("The follow-up API response contains an invalid " & propertyName & ".")
        End If
        Return result
    End Function

    Private Shared Function ParseGuid(value As String, description As String) As Guid
        Dim result As Guid
        If Not Guid.TryParse(value, result) OrElse result = Guid.Empty Then
            Throw New InvalidDataException("The follow-up API returned an invalid " & description & ".")
        End If
        Return result
    End Function

    Private Shared Function ParseUtc(value As String, description As String) As DateTime
        Dim result As DateTime
        If Not DateTime.TryParse(value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal,
            result) Then
            Throw New InvalidDataException("The follow-up API returned an invalid " & description & ".")
        End If
        Return DateTime.SpecifyKind(result, DateTimeKind.Utc)
    End Function

    Private Shared Function FormatUtc(value As DateTime) As String
        Return CLFollowUpReminderRules.RequireUtc(value, NameOf(value)).ToString("O", CultureInfo.InvariantCulture)
    End Function

    Private Shared Function FollowUpDays(fromUtc As DateTime, dueUtc As DateTime) As Integer
        Dim elapsedDays As Double = (CLFollowUpReminderRules.RequireUtc(dueUtc, NameOf(dueUtc)) -
            CLFollowUpReminderRules.RequireUtc(fromUtc, NameOf(fromUtc))).TotalDays
        Return Math.Max(CLFollowUpReminderRules.MinimumDelayDays,
            Math.Min(CLFollowUpReminderRules.MaximumDelayDays, CInt(Math.Round(elapsedDays, MidpointRounding.AwayFromZero))))
    End Function

End Class
