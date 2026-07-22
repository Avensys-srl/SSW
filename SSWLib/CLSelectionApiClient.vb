Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports Microsoft.Win32

Public NotInheritable Class CLSelectionApiException
    Inherits Exception

    Public Sub New(statusCode As HttpStatusCode, errorCode As String, message As String)
        MyBase.New(message)
        Me.StatusCode = statusCode
        Me.ErrorCode = errorCode
    End Sub

    Public ReadOnly Property StatusCode As HttpStatusCode
    Public ReadOnly Property ErrorCode As String
End Class

Public NotInheritable Class CLSelectionRegistrationContext
    Public Property CustomerCode As String
    Public Property SoftwareVersion As String
    Public Property DatabaseSchemaVersion As Integer
    Public Property DatabaseContentHash As String
    Public Property ApiContractVersion As Integer

    Public Shared Function FromEnvironment(environment As CLEnvironment) As CLSelectionRegistrationContext
        If environment Is Nothing Then Throw New ArgumentNullException(NameOf(environment))
        Dim databaseInfo As CLDatabaseCompatibilityInfo = environment.DatabaseCompatibility
        If databaseInfo Is Nothing Then Throw New InvalidOperationException("Database compatibility information is unavailable.")
        Dim contentHash As String = databaseInfo.ContentHash
        If String.IsNullOrWhiteSpace(contentHash) Then contentHash = "legacy"
        Dim apiCustomerCode As String = environment.CustomerProfile
        If String.IsNullOrWhiteSpace(apiCustomerCode) Then apiCustomerCode = environment.CustomerCode
        Return New CLSelectionRegistrationContext With {
            .CustomerCode = apiCustomerCode,
            .SoftwareVersion = CLTechnicalVersions.SoftwareVersion.ToString(),
            .DatabaseSchemaVersion = Math.Max(1, databaseInfo.SchemaVersion),
            .DatabaseContentHash = contentHash,
            .ApiContractVersion = CLTechnicalVersions.CurrentApiContractVersion
        }
    End Function
End Class

Friend NotInheritable Class CLSelectionTokenResponse
    Public Property AccessToken As String
    Public Property ExpiresAt As DateTime
End Class

Public NotInheritable Class CLSelectionRegistrationResult
    Public Property PublicReference As String
    Public Property ReferenceDigits As String
    Public Property Revision As Integer
    Public Property ResumeToken As String
    Public Property SnapshotHash As String
    Public Property ChangeKind As String
End Class

Public NotInheritable Partial Class CLSelectionApiClient

    Private Const DefaultBaseUrl As String = "https://www.avensys-srl.com/api/v1/"
    Private Const BootstrapRegistryPath As String = "Software\Avensys\SSW\TechnicalSelection"
    Private Const BootstrapRegistryValuePrefix As String = "BootstrapKey_"
    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {
        .PropertyNameCaseInsensitive = True
    }
    Private ReadOnly m_HttpClient As HttpClient

    Public Sub New(Optional httpClient As HttpClient = Nothing)
        m_HttpClient = If(httpClient, New HttpClient())
        m_HttpClient.Timeout = TimeSpan.FromSeconds(20)
    End Sub

    Public Async Function EnsureAccessTokenAsync(context As CLSelectionRegistrationContext,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)

        ValidateContext(context)
        Dim credentials As CLSelectionCredentialSnapshot = CLSelectionCredentialStore.LoadOrCreate()
        If Not String.IsNullOrWhiteSpace(credentials.AccessToken) AndAlso credentials.TokenExpiresAtUtc.HasValue Then
            If credentials.TokenExpiresAtUtc.Value > DateTime.UtcNow.AddDays(14) Then
                ClearUserBootstrapKey(context.CustomerCode)
                Return credentials.AccessToken
            End If
            Try
                Dim renewed As CLSelectionTokenResponse = Await RenewAsync(credentials.AccessToken, cancellationToken).ConfigureAwait(False)
                CLSelectionCredentialStore.SaveAccessToken(renewed.AccessToken, renewed.ExpiresAt)
                ClearUserBootstrapKey(context.CustomerCode)
                Return renewed.AccessToken
            Catch ex As CLSelectionApiException When ex.StatusCode = HttpStatusCode.Unauthorized
                CLSelectionCredentialStore.ClearAccessToken()
            End Try
        End If

        Dim registered As CLSelectionTokenResponse = Await RegisterAsync(context, credentials.InstallationId, cancellationToken).ConfigureAwait(False)
        CLSelectionCredentialStore.SaveAccessToken(registered.AccessToken, registered.ExpiresAt)
        ClearUserBootstrapKey(context.CustomerCode)
        Return registered.AccessToken
    End Function

    Public Async Function RegisterSelectionAsync(document As CLSelectionProjectDocument,
        context As CLSelectionRegistrationContext,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLSelectionRegistrationResult)

        ValidateSelectionDocument(document)
        ValidateContext(context)
        Dim accessToken As String = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Try
            Return Await SendSelectionAsync(document, accessToken, cancellationToken).ConfigureAwait(False)
        Catch ex As CLSelectionApiException When ex.StatusCode = HttpStatusCode.Unauthorized
            CLSelectionCredentialStore.ClearAccessToken()
        End Try
        accessToken = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Return Await SendSelectionAsync(document, accessToken, cancellationToken).ConfigureAwait(False)
    End Function

    Public Async Function SyncMultiSelectionProjectAsync(document As CLMultiSelectionProjectDocument,
        context As CLSelectionRegistrationContext,
        Optional cancellationToken As CancellationToken = Nothing) As Task

        If document Is Nothing OrElse document.ProjectId = Guid.Empty OrElse
            String.IsNullOrWhiteSpace(document.Reference) Then
            Throw New InvalidDataException("The selection project is invalid.")
        End If
        ValidateContext(context)
        Dim accessToken As String = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Try
            Await SendMultiSelectionProjectAsync(document, accessToken, cancellationToken).ConfigureAwait(False)
            Return
        Catch ex As CLSelectionApiException When ex.StatusCode = HttpStatusCode.Unauthorized
            CLSelectionCredentialStore.ClearAccessToken()
        End Try
        accessToken = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Await SendMultiSelectionProjectAsync(document, accessToken, cancellationToken).ConfigureAwait(False)
    End Function

    Private Async Function SendMultiSelectionProjectAsync(document As CLMultiSelectionProjectDocument,
        accessToken As String,
        cancellationToken As CancellationToken) As Task

        Dim items As New List(Of Dictionary(Of String, Object))()
        For Each item As CLMultiSelectionProjectItem In document.Items
            items.Add(New Dictionary(Of String, Object) From {
                {"item_id", item.ItemId.ToString("D")},
                {"selection_project_id", item.SelectionProjectId.ToString("D")},
                {"customer_reference", If(String.IsNullOrWhiteSpace(item.CustomerReference), Nothing, item.CustomerReference)},
                {"unit_name", If(String.IsNullOrWhiteSpace(item.UnitName), Nothing, item.UnitName)},
                {"airflow_m3h", item.AirflowM3h},
                {"pressure_pa", item.PressurePa},
                {"pdf_filename", If(String.IsNullOrWhiteSpace(item.PdfFileName), Nothing, item.PdfFileName)},
                {"language_code", item.LanguageCode},
                {"snapshot_hash", If(String.IsNullOrWhiteSpace(item.SnapshotHash), Nothing, item.SnapshotHash)}
            })
        Next
        Dim payload As New Dictionary(Of String, Object) From {
            {"reference", document.Reference},
            {"language_code", document.LanguageCode},
            {"items", items}
        }
        Dim payloadJson As String = JsonSerializer.Serialize(payload, JsonOptions)
        Dim idempotencyKey As String = CreateIdempotencyKey("project-sync", document.ProjectId, payloadJson)
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri("projects/" & document.ProjectId.ToString("D")))
            request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", accessToken)
            request.Headers.Add("Idempotency-Key", idempotencyKey)
            request.Content = New StringContent(payloadJson, Encoding.UTF8, "application/json")
            Using response As HttpResponseMessage = Await m_HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(False)
                Dim body As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                If Not response.IsSuccessStatusCode Then Throw CreateApiException(response.StatusCode, body)
            End Using
        End Using
    End Function

    Private Async Function SendSelectionAsync(document As CLSelectionProjectDocument,
        accessToken As String,
        cancellationToken As CancellationToken) As Task(Of CLSelectionRegistrationResult)

        Dim isRevision As Boolean = Not String.IsNullOrWhiteSpace(document.Identity.PublicReference)
        Dim relativePath As String = "selections"
        Dim operation As String = "create"
        If Not isRevision AndAlso String.IsNullOrWhiteSpace(document.Identity.ResumeToken) Then
            document.Identity.ResumeToken = GenerateOpaqueToken()
        End If
        Dim payload As New Dictionary(Of String, Object) From {
            {"selection", document.Selection},
            {"versions", CreateVersionsPayload(document.Versions)},
            {"fingerprints", CreateFingerprintsPayload(document.RevisionTracking.Current)},
            {"resume_token", document.Identity.ResumeToken}
        }
        If isRevision Then
            If String.IsNullOrWhiteSpace(document.Identity.ResumeToken) Then
                Throw New InvalidDataException("The selection project does not contain its resume token.")
            End If
            Dim referenceDigits As String = GetReferenceDigits(document.Identity.PublicReference)
            relativePath = "selections/" & referenceDigits & "/revisions"
            operation = "revise-" & referenceDigits
        Else
            payload.Add("project_id", document.ProjectId.ToString("D"))
        End If

        Dim payloadJson As String = JsonSerializer.Serialize(payload, JsonOptions)
        Dim idempotencyKey As String = CreateIdempotencyKey(operation, document.ProjectId, payloadJson)
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri(relativePath))
            request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", accessToken)
            request.Headers.Add("Idempotency-Key", idempotencyKey)
            request.Content = New StringContent(payloadJson, Encoding.UTF8, "application/json")
            Using response As HttpResponseMessage = Await m_HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(False)
                Dim body As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                If Not response.IsSuccessStatusCode Then Throw CreateApiException(response.StatusCode, body)
                Return ParseSelectionResponse(body, document.Identity.ResumeToken)
            End Using
        End Using
    End Function

    Private Async Function RegisterAsync(context As CLSelectionRegistrationContext,
        installationId As String,
        cancellationToken As CancellationToken) As Task(Of CLSelectionTokenResponse)

        Dim bootstrapKey As String = ResolveBootstrapKey(context.CustomerCode)
        Dim payload = New Dictionary(Of String, Object) From {
            {"customer_code", context.CustomerCode},
            {"installation_id", installationId},
            {"installation_code", CLSelectionInstallationStateStore.GetInstallationCode()},
            {"software_version", context.SoftwareVersion},
            {"database_schema_version", context.DatabaseSchemaVersion},
            {"database_content_hash", context.DatabaseContentHash},
            {"api_contract_version", context.ApiContractVersion}
        }
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri("installations/register"))
            If Not String.IsNullOrWhiteSpace(bootstrapKey) Then
                request.Headers.Add("X-SSW-Bootstrap-Key", bootstrapKey)
            End If
            request.Content = JsonContent(payload)
            Return Await SendTokenRequestAsync(request, cancellationToken).ConfigureAwait(False)
        End Using
    End Function

    Private Async Function RenewAsync(accessToken As String,
        cancellationToken As CancellationToken) As Task(Of CLSelectionTokenResponse)
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri("installations/token/renew"))
            request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", accessToken)
            request.Content = JsonContent(New Dictionary(Of String, Object)())
            Return Await SendTokenRequestAsync(request, cancellationToken).ConfigureAwait(False)
        End Using
    End Function

    Private Async Function SendTokenRequestAsync(request As HttpRequestMessage,
        cancellationToken As CancellationToken) As Task(Of CLSelectionTokenResponse)
        Using response As HttpResponseMessage = Await m_HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(False)
            Dim body As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
            If Not response.IsSuccessStatusCode Then Throw CreateApiException(response.StatusCode, body)
            Using document As JsonDocument = JsonDocument.Parse(body)
                Dim root = document.RootElement
                Dim accessToken As String = root.GetProperty("access_token").GetString()
                Dim expiresText As String = root.GetProperty("expires_at").GetString()
                Dim expiresAt As DateTime
                If String.IsNullOrWhiteSpace(accessToken) OrElse Not DateTime.TryParse(expiresText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal,
                    expiresAt) Then
                    Throw New InvalidDataException("The technical selection API returned an invalid token response.")
                End If
                Return New CLSelectionTokenResponse With {.AccessToken = accessToken, .ExpiresAt = expiresAt}
            End Using
        End Using
    End Function

    Private Shared Function CreateApiException(statusCode As HttpStatusCode, body As String) As CLSelectionApiException
        Dim errorCode As String = "api_error"
        Dim message As String = "The technical selection service rejected the request."
        Try
            Using document As JsonDocument = JsonDocument.Parse(body)
                Dim propertyValue As JsonElement
                If document.RootElement.TryGetProperty("error", propertyValue) Then errorCode = propertyValue.GetString()
                If document.RootElement.TryGetProperty("message", propertyValue) Then message = propertyValue.GetString()
            End Using
        Catch ex As JsonException
        End Try
        Return New CLSelectionApiException(statusCode, errorCode, message)
    End Function

    Private Shared Function JsonContent(payload As Object) As HttpContent
        Return New StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
    End Function

    Private Shared Function CreateVersionsPayload(value As CLSelectionVersionSet) As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object) From {
            {"software_version", value.SoftwareVersion},
            {"calculation_engine_version", value.CalculationEngineVersion},
            {"database_schema_version", value.DatabaseSchemaVersion},
            {"database_data_version", value.DatabaseDataVersion},
            {"database_content_hash", value.DatabaseContentHash},
            {"selection_format_version", value.SelectionFormatVersion},
            {"report_template_version", value.ReportTemplateVersion},
            {"api_contract_version", value.ApiContractVersion}
        }
    End Function

    Private Shared Function CreateFingerprintsPayload(value As CLSelectionFingerprintSet) As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object) From {
            {"technical_input_hash", value.TechnicalInputHash},
            {"calculation_output_hash", value.CalculationOutputHash},
            {"calculation_basis_hash", value.CalculationBasisHash},
            {"snapshot_hash", value.SnapshotHash}
        }
    End Function

    Private Shared Function ParseSelectionResponse(body As String, existingResumeToken As String) As CLSelectionRegistrationResult
        Using document As JsonDocument = JsonDocument.Parse(body)
            Dim root As JsonElement = document.RootElement
            Dim fullReference As String = root.GetProperty("reference").GetString()
            Dim resumeToken As String = existingResumeToken
            Dim propertyValue As JsonElement
            If root.TryGetProperty("resume_token", propertyValue) Then resumeToken = propertyValue.GetString()
            Return New CLSelectionRegistrationResult With {
                .PublicReference = RemoveRevisionSuffix(fullReference),
                .ReferenceDigits = root.GetProperty("reference_digits").GetString(),
                .Revision = root.GetProperty("revision").GetInt32(),
                .ResumeToken = resumeToken,
                .SnapshotHash = root.GetProperty("snapshot_hash").GetString(),
                .ChangeKind = root.GetProperty("change_kind").GetString()
            }
        End Using
    End Function

    Private Shared Function RemoveRevisionSuffix(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Throw New InvalidDataException("The technical selection API returned an invalid reference.")
        Dim suffixIndex As Integer = value.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase)
        Return If(suffixIndex > 0, value.Substring(0, suffixIndex), value)
    End Function

    Private Shared Function GetReferenceDigits(value As String) As String
        Dim baseReference As String = RemoveRevisionSuffix(value)
        Dim digits As String = New String(baseReference.Where(Function(character) Char.IsDigit(character)).ToArray())
        If digits.Length <> 16 Then Throw New InvalidDataException("The selection project contains an invalid public reference.")
        Return digits
    End Function

    Private Shared Function CreateIdempotencyKey(operation As String, projectId As Guid, payloadJson As String) As String
        Dim material As String = operation & "|" & projectId.ToString("D") & "|" & payloadJson
        Using algorithm As SHA256 = SHA256.Create()
            Dim hash As String = String.Concat(algorithm.ComputeHash(Encoding.UTF8.GetBytes(material)).
                Select(Function(item) item.ToString("x2")))
            Return "ssw-" & hash.Substring(0, 48)
        End Using
    End Function

    Private Shared Function GenerateOpaqueToken() As String
        Dim bytes(31) As Byte
        Using generator As RandomNumberGenerator = RandomNumberGenerator.Create()
            generator.GetBytes(bytes)
        End Using
        Return Convert.ToBase64String(bytes).TrimEnd("="c).Replace("+"c, "-"c).Replace("/"c, "_"c)
    End Function

    Private Shared Function BuildUri(relativePath As String) As Uri
        Dim configured As String = System.Environment.GetEnvironmentVariable("SSW_SELECTION_API_BASE_URL")
        If String.IsNullOrWhiteSpace(configured) Then
            configured = ConfigurationManager.AppSettings("TechnicalSelectionApiBaseUrl")
        End If
        If String.IsNullOrWhiteSpace(configured) Then configured = DefaultBaseUrl
        Return New Uri(New Uri(configured.TrimEnd("/"c) & "/"), relativePath)
    End Function

    Private Shared Function ResolveBootstrapKey(customerCode As String) As String
        Dim safeCustomerCode As String = New String(customerCode.ToUpperInvariant().Where(
            Function(character) Char.IsLetterOrDigit(character)).ToArray())
        Dim value As String = ResolveRegistryBootstrapKey(safeCustomerCode)
        If String.IsNullOrWhiteSpace(value) Then
            value = ResolveEnvironmentValue("SSW_SELECTION_BOOTSTRAP_KEY_" & safeCustomerCode)
        End If
        If String.IsNullOrWhiteSpace(value) Then value = ResolveEnvironmentValue("SSW_SELECTION_BOOTSTRAP_KEY")
        If String.IsNullOrWhiteSpace(value) Then value = ConfigurationManager.AppSettings("TechnicalSelectionBootstrapKey")
        Return value
    End Function

    Private Shared Function ResolveRegistryBootstrapKey(safeCustomerCode As String) As String
        If String.IsNullOrWhiteSpace(safeCustomerCode) Then Return Nothing
        Try
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(BootstrapRegistryPath, False)
                If key Is Nothing Then Return Nothing
                Return TryCast(key.GetValue(
                    BootstrapRegistryValuePrefix & safeCustomerCode,
                    Nothing,
                    RegistryValueOptions.DoNotExpandEnvironmentNames), String)
            End Using
        Catch ex As Exception When TypeOf ex Is System.Security.SecurityException OrElse
            TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

    Private Shared Function ResolveEnvironmentValue(name As String) As String
        Dim value As String = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process)
        If String.IsNullOrWhiteSpace(value) Then
            value = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
        End If
        If String.IsNullOrWhiteSpace(value) Then
            value = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine)
        End If
        Return value
    End Function

    Private Shared Sub ClearUserBootstrapKey(customerCode As String)
        Dim safeCustomerCode As String = New String(customerCode.ToUpperInvariant().Where(
            Function(character) Char.IsLetterOrDigit(character)).ToArray())
        If String.IsNullOrWhiteSpace(safeCustomerCode) Then Return
        Try
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(BootstrapRegistryPath, True)
                If key IsNot Nothing Then
                    key.DeleteValue(BootstrapRegistryValuePrefix & safeCustomerCode, False)
                End If
            End Using
        Catch ex As Exception When TypeOf ex Is System.Security.SecurityException OrElse
            TypeOf ex Is UnauthorizedAccessException
            ' A protected user profile must not invalidate an otherwise valid access token.
        End Try
        Try
            Dim environmentName As String = "SSW_SELECTION_BOOTSTRAP_KEY_" & safeCustomerCode
            Dim userValue As String = System.Environment.GetEnvironmentVariable(
                environmentName,
                EnvironmentVariableTarget.User)
            If Not String.IsNullOrWhiteSpace(userValue) Then
                Dim processValue As String = System.Environment.GetEnvironmentVariable(
                    environmentName,
                    EnvironmentVariableTarget.Process)
                If Not String.IsNullOrWhiteSpace(processValue) AndAlso
                    Not String.Equals(processValue, userValue, StringComparison.Ordinal) Then
                    Return
                End If

                System.Environment.SetEnvironmentVariable(
                    environmentName,
                    Nothing,
                    EnvironmentVariableTarget.User)
                If String.Equals(processValue, userValue, StringComparison.Ordinal) Then
                    System.Environment.SetEnvironmentVariable(
                        environmentName,
                        Nothing,
                        EnvironmentVariableTarget.Process)
                End If
            End If
        Catch ex As Exception When TypeOf ex Is System.Security.SecurityException OrElse
            TypeOf ex Is UnauthorizedAccessException
            ' A protected user profile must not invalidate an otherwise valid access token.
        End Try
    End Sub

    Private Shared Sub ValidateContext(context As CLSelectionRegistrationContext)
        If context Is Nothing Then Throw New ArgumentNullException(NameOf(context))
        If String.IsNullOrWhiteSpace(context.CustomerCode) Then Throw New ArgumentException("Customer code is required.")
        If String.IsNullOrWhiteSpace(context.SoftwareVersion) Then Throw New ArgumentException("Software version is required.")
        If context.DatabaseSchemaVersion < 1 Then Throw New ArgumentException("Database schema version is invalid.")
        If String.IsNullOrWhiteSpace(context.DatabaseContentHash) Then Throw New ArgumentException("Database content hash is required.")
        If context.ApiContractVersion < 1 Then Throw New ArgumentException("API contract version is invalid.")
    End Sub

    Private Shared Sub ValidateSelectionDocument(document As CLSelectionProjectDocument)
        If document Is Nothing OrElse document.Selection Is Nothing OrElse document.Versions Is Nothing OrElse
            document.RevisionTracking Is Nothing OrElse document.RevisionTracking.Current Is Nothing Then
            Throw New InvalidDataException("A calculated technical selection snapshot is required.")
        End If
        Dim fingerprints As CLSelectionFingerprintSet = document.RevisionTracking.Current
        If Not CLSelectionSnapshotService.IsValidHash(fingerprints.TechnicalInputHash) OrElse
            Not CLSelectionSnapshotService.IsValidHash(fingerprints.CalculationOutputHash) OrElse
            Not CLSelectionSnapshotService.IsValidHash(fingerprints.CalculationBasisHash) OrElse
            Not CLSelectionSnapshotService.IsValidHash(fingerprints.SnapshotHash) Then
            Throw New InvalidDataException("The technical selection fingerprints are invalid.")
        End If
    End Sub

End Class
