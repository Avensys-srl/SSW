Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Threading

Public Enum CLDeviceLicenseMode
    NewInstallation
    LegacyProfileRequired
    Active
    OfflineExpired
    Revoked
End Enum

Public NotInheritable Class CLDeviceLicenseState
    Public Property SchemaVersion As Integer = 1
    Public Property FirstName As String
    Public Property LastName As String
    Public Property Email As String
    Public Property DeviceNumber As Integer?
    Public Property LastOnlineCheckUtc As DateTime?
    Public Property ValidUntilUtc As DateTime?
    Public Property Revoked As Boolean
End Class

Public NotInheritable Class CLDeviceLicenseSnapshot
    Public Property Mode As CLDeviceLicenseMode
    Public Property FirstName As String
    Public Property LastName As String
    Public Property Email As String
    Public Property DeviceNumber As Integer?
    Public Property LastOnlineCheckUtc As DateTime?
    Public Property ValidUntilUtc As DateTime?
End Class

Public NotInheritable Class CLDeviceLicenseStore
    Private Const MutexName As String = "Local\Avensys.SSW.DeviceLicense"
    Private Shared ReadOnly Entropy As Byte() = Encoding.UTF8.GetBytes("Avensys.SSW.DeviceLicense.v1")
    Private Shared ReadOnly Options As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        .PropertyNameCaseInsensitive = True,
        .WriteIndented = True
    }

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property StateFilePath As String
        Get
            Dim testPath = System.Environment.GetEnvironmentVariable("SSW_DEVICE_LICENSE_PATH")
            If Not String.IsNullOrWhiteSpace(testPath) Then Return Path.GetFullPath(testPath)
            Return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "Avensys", "SSW", "device-license.dat")
        End Get
    End Property

    Public Shared Function LoadSnapshot() As CLDeviceLicenseSnapshot
        Return Locked(Function()
            Dim state = ReadState()
            If state Is Nothing Then
                Dim credentials = CLSelectionCredentialStore.LoadOrCreate()
                Return New CLDeviceLicenseSnapshot With {
                    .Mode = If(String.IsNullOrWhiteSpace(credentials.AccessToken), CLDeviceLicenseMode.NewInstallation, CLDeviceLicenseMode.LegacyProfileRequired)
                }
            End If
            Return New CLDeviceLicenseSnapshot With {
                .Mode = If(state.Revoked, CLDeviceLicenseMode.Revoked,
                    If(state.ValidUntilUtc.HasValue AndAlso state.ValidUntilUtc.Value > DateTime.UtcNow, CLDeviceLicenseMode.Active, CLDeviceLicenseMode.OfflineExpired)),
                .FirstName = state.FirstName,
                .LastName = state.LastName,
                .Email = state.Email,
                .DeviceNumber = state.DeviceNumber,
                .LastOnlineCheckUtc = state.LastOnlineCheckUtc,
                .ValidUntilUtc = state.ValidUntilUtc
            }
        End Function)
    End Function

    Public Shared Sub SaveActive(firstName As String, lastName As String, email As String, deviceNumber As Integer, validUntilUtc As DateTime)
        Locked(Function()
            WriteState(New CLDeviceLicenseState With {
                .FirstName = firstName.Trim(), .LastName = lastName.Trim(), .Email = email.Trim().ToLowerInvariant(),
                .DeviceNumber = deviceNumber, .LastOnlineCheckUtc = DateTime.UtcNow,
                .ValidUntilUtc = validUntilUtc.ToUniversalTime(), .Revoked = False
            })
            Return Nothing
        End Function)
    End Sub

    Public Shared Sub Renew(deviceNumber As Integer, validUntilUtc As DateTime)
        Locked(Function()
            Dim state = ReadState()
            If state Is Nothing Then Throw New InvalidOperationException("The local device license profile is missing.")
            state.DeviceNumber = deviceNumber
            state.LastOnlineCheckUtc = DateTime.UtcNow
            state.ValidUntilUtc = validUntilUtc.ToUniversalTime()
            state.Revoked = False
            WriteState(state)
            Return Nothing
        End Function)
    End Sub

    Public Shared Sub MarkRevoked()
        Locked(Function()
            Dim state = ReadState()
            If state Is Nothing Then state = New CLDeviceLicenseState()
            state.Revoked = True
            WriteState(state)
            Return Nothing
        End Function)
    End Sub

    Private Shared Function ReadState() As CLDeviceLicenseState
        If Not File.Exists(StateFilePath) Then Return Nothing
        Try
            Dim protectedBytes = File.ReadAllBytes(StateFilePath)
            Dim clearBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.LocalMachine)
            Try
                Dim state = JsonSerializer.Deserialize(Of CLDeviceLicenseState)(clearBytes, Options)
                If state Is Nothing OrElse state.SchemaVersion <> 1 Then Throw New InvalidDataException("Unsupported device license state.")
                Return state
            Finally
                Array.Clear(clearBytes, 0, clearBytes.Length)
            End Try
        Catch ex As Exception When TypeOf ex Is CryptographicException OrElse TypeOf ex Is JsonException
            Throw New InvalidDataException("The local device license state is invalid.", ex)
        End Try
    End Function

    Private Shared Sub WriteState(state As CLDeviceLicenseState)
        Dim directoryPath = Path.GetDirectoryName(StateFilePath)
        System.IO.Directory.CreateDirectory(directoryPath)
        Dim clearBytes = JsonSerializer.SerializeToUtf8Bytes(state, Options)
        Dim protectedBytes As Byte()
        Try
            protectedBytes = ProtectedData.Protect(clearBytes, Entropy, DataProtectionScope.LocalMachine)
        Finally
            Array.Clear(clearBytes, 0, clearBytes.Length)
        End Try
        Dim temporary = StateFilePath & ".tmp-" & Guid.NewGuid().ToString("N")
        Dim backup = temporary & ".bak"
        Try
            File.WriteAllBytes(temporary, protectedBytes)
            If File.Exists(StateFilePath) Then
                File.Replace(temporary, StateFilePath, backup, True)
                If File.Exists(backup) Then File.Delete(backup)
            Else
                File.Move(temporary, StateFilePath)
            End If
        Finally
            Array.Clear(protectedBytes, 0, protectedBytes.Length)
            If File.Exists(temporary) Then File.Delete(temporary)
            If File.Exists(backup) Then File.Delete(backup)
        End Try
    End Sub

    Private Shared Function Locked(Of T)(action As Func(Of T)) As T
        Using mutex As New Mutex(False, MutexName)
            Dim acquired As Boolean
            Try
                Try
                    acquired = mutex.WaitOne(TimeSpan.FromSeconds(15))
                Catch ex As AbandonedMutexException
                    acquired = True
                End Try
                If Not acquired Then Throw New TimeoutException("The device license state is busy.")
                Return action()
            Finally
                If acquired Then mutex.ReleaseMutex()
            End Try
        End Using
    End Function
End Class

Public NotInheritable Class CLDeviceLicenseResult
    Public Property DeviceNumber As Integer
    Public Property ValidUntilUtc As DateTime
End Class

Partial Public NotInheritable Class CLSelectionApiClient
    Public Async Function ActivateDeviceLicenseAsync(firstName As String, lastName As String, email As String, companyName As String, pin As String,
        context As CLSelectionRegistrationContext, Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLDeviceLicenseResult)
        ValidateContext(context)
        Dim credentials = CLSelectionCredentialStore.LoadOrCreate()
        Dim payload = LicenseProfilePayload(firstName, lastName, email, companyName)
        payload("activation_code") = pin.Trim()
        payload("installation_id") = credentials.InstallationId
        payload("installation_code") = CLSelectionInstallationStateStore.GetInstallationCode()
        payload("software_version") = context.SoftwareVersion
        payload("database_schema_version") = context.DatabaseSchemaVersion
        payload("database_content_hash") = context.DatabaseContentHash
        payload("api_contract_version") = context.ApiContractVersion
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri("license/activate"))
            request.Content = JsonContent(payload)
            Dim parsed = Await SendLicenseRequestAsync(request, True, cancellationToken).ConfigureAwait(False)
            CLSelectionCredentialStore.SaveAccessToken(parsed.AccessToken, parsed.TokenExpiresAtUtc)
            Return parsed.Result
        End Using
    End Function

    Public Async Function ClaimLegacyDeviceLicenseAsync(firstName As String, lastName As String, email As String, companyName As String,
        context As CLSelectionRegistrationContext, Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLDeviceLicenseResult)
        ValidateContext(context)
        Dim token = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri("license/claim-legacy"))
            request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", token)
            request.Content = JsonContent(LicenseProfilePayload(firstName, lastName, email, companyName))
            Return (Await SendLicenseRequestAsync(request, False, cancellationToken).ConfigureAwait(False)).Result
        End Using
    End Function

    Public Async Function CheckDeviceLicenseAsync(context As CLSelectionRegistrationContext, sessionId As Guid,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLDeviceLicenseResult)
        ValidateContext(context)
        Dim token = Await EnsureAccessTokenAsync(context, cancellationToken).ConfigureAwait(False)
        Using request As New HttpRequestMessage(HttpMethod.Post, BuildUri("license/check"))
            request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", token)
            request.Content = JsonContent(New Dictionary(Of String, Object) From {{"session_id", sessionId.ToString("D")}})
            Return (Await SendLicenseRequestAsync(request, False, cancellationToken).ConfigureAwait(False)).Result
        End Using
    End Function

    Private Shared Function LicenseProfilePayload(firstName As String, lastName As String, email As String, companyName As String) As Dictionary(Of String, Object)
        If String.IsNullOrWhiteSpace(firstName) OrElse String.IsNullOrWhiteSpace(lastName) OrElse String.IsNullOrWhiteSpace(email) OrElse String.IsNullOrWhiteSpace(companyName) Then
            Throw New ArgumentException("Name, surname, email and company are required.")
        End If
        Return New Dictionary(Of String, Object) From {{"first_name", firstName.Trim()}, {"last_name", lastName.Trim()}, {"email", email.Trim().ToLowerInvariant()}, {"company_name", companyName.Trim()}}
    End Function

    Private Async Function SendLicenseRequestAsync(request As HttpRequestMessage, expectsToken As Boolean, cancellationToken As CancellationToken) As Task(Of LicenseResponse)
        Using response = Await m_HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(False)
            Dim body = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
            If Not response.IsSuccessStatusCode Then Throw CreateApiException(response.StatusCode, body)
            Using document = JsonDocument.Parse(body)
                Dim root = document.RootElement
                Dim validUntil As DateTime
                If Not DateTime.TryParse(root.GetProperty("valid_until_utc").GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal, validUntil) Then
                    Throw New InvalidDataException("The license service returned an invalid expiry date.")
                End If
                Dim parsed As New LicenseResponse With {.Result = New CLDeviceLicenseResult With {.DeviceNumber = root.GetProperty("device_number").GetInt32(), .ValidUntilUtc = validUntil}}
                If expectsToken Then
                    parsed.AccessToken = root.GetProperty("access_token").GetString()
                    If Not DateTime.TryParse(root.GetProperty("token_expires_at_utc").GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal, parsed.TokenExpiresAtUtc) Then
                        Throw New InvalidDataException("The license service returned an invalid token expiry date.")
                    End If
                End If
                Return parsed
            End Using
        End Using
    End Function

    Private NotInheritable Class LicenseResponse
        Public Property Result As CLDeviceLicenseResult
        Public Property AccessToken As String
        Public Property TokenExpiresAtUtc As DateTime
    End Class
End Class
