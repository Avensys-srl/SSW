Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.Json
Imports System.Threading

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
        Return New CLSelectionRegistrationContext With {
            .CustomerCode = environment.CustomerCode,
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

Public NotInheritable Class CLSelectionApiClient

    Private Const DefaultBaseUrl As String = "https://www.avensys-srl.com/api/v1/"
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
            If credentials.TokenExpiresAtUtc.Value > DateTime.UtcNow.AddDays(14) Then Return credentials.AccessToken
            Try
                Dim renewed As CLSelectionTokenResponse = Await RenewAsync(credentials.AccessToken, cancellationToken).ConfigureAwait(False)
                CLSelectionCredentialStore.SaveAccessToken(renewed.AccessToken, renewed.ExpiresAt)
                Return renewed.AccessToken
            Catch ex As CLSelectionApiException When ex.StatusCode = HttpStatusCode.Unauthorized
                CLSelectionCredentialStore.ClearAccessToken()
            End Try
        End If

        Dim registered As CLSelectionTokenResponse = Await RegisterAsync(context, credentials.InstallationId, cancellationToken).ConfigureAwait(False)
        CLSelectionCredentialStore.SaveAccessToken(registered.AccessToken, registered.ExpiresAt)
        Return registered.AccessToken
    End Function

    Private Async Function RegisterAsync(context As CLSelectionRegistrationContext,
        installationId As String,
        cancellationToken As CancellationToken) As Task(Of CLSelectionTokenResponse)

        Dim bootstrapKey As String = ResolveBootstrapKey(context.CustomerCode)
        If String.IsNullOrWhiteSpace(bootstrapKey) Then
            Throw New InvalidOperationException("The technical selection bootstrap credential is not configured.")
        End If
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
            request.Headers.Add("X-SSW-Bootstrap-Key", bootstrapKey)
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
        Dim value As String = System.Environment.GetEnvironmentVariable("SSW_SELECTION_BOOTSTRAP_KEY_" & safeCustomerCode)
        If String.IsNullOrWhiteSpace(value) Then value = System.Environment.GetEnvironmentVariable("SSW_SELECTION_BOOTSTRAP_KEY")
        If String.IsNullOrWhiteSpace(value) Then value = ConfigurationManager.AppSettings("TechnicalSelectionBootstrapKey")
        Return value
    End Function

    Private Shared Sub ValidateContext(context As CLSelectionRegistrationContext)
        If context Is Nothing Then Throw New ArgumentNullException(NameOf(context))
        If String.IsNullOrWhiteSpace(context.CustomerCode) Then Throw New ArgumentException("Customer code is required.")
        If String.IsNullOrWhiteSpace(context.SoftwareVersion) Then Throw New ArgumentException("Software version is required.")
        If context.DatabaseSchemaVersion < 1 Then Throw New ArgumentException("Database schema version is invalid.")
        If String.IsNullOrWhiteSpace(context.DatabaseContentHash) Then Throw New ArgumentException("Database content hash is required.")
        If context.ApiContractVersion < 1 Then Throw New ArgumentException("API contract version is invalid.")
    End Sub

End Class
