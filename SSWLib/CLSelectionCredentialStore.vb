Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Threading

Public NotInheritable Class CLSelectionCredentialState

    Public Property SchemaVersion As Integer = 1
    Public Property InstallationId As String
    Public Property ProtectedAccessToken As String
    Public Property TokenExpiresAtUtc As DateTime?

End Class

Public NotInheritable Class CLSelectionCredentialSnapshot

    Public Property InstallationId As String
    Public Property AccessToken As String
    Public Property TokenExpiresAtUtc As DateTime?

End Class

Public NotInheritable Class CLSelectionCredentialStore

    Private Const StateMutexName As String = "Local\Avensys.SSW.TechnicalSelectionCredentials"
    Private Shared ReadOnly Entropy As Byte() = Encoding.UTF8.GetBytes("Avensys.SSW.TechnicalSelections.v1")
    Private Shared ReadOnly SerializerOptions As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        .PropertyNameCaseInsensitive = True,
        .WriteIndented = True
    }

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property StateFilePath As String
        Get
            Dim testPath As String = System.Environment.GetEnvironmentVariable("SSW_SELECTION_CREDENTIAL_PATH")
            If Not String.IsNullOrWhiteSpace(testPath) Then Return Path.GetFullPath(testPath)
            Return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                "Avensys",
                "SSW",
                "technical-selection-credentials.json")
        End Get
    End Property

    Public Shared Function LoadOrCreate() As CLSelectionCredentialSnapshot
        Return WithLock(Function()
            Dim state As CLSelectionCredentialState = ReadOrCreateState()
            SaveState(state)
            Return ToSnapshot(state)
        End Function)
    End Function

    Public Shared Sub SaveAccessToken(accessToken As String, expiresAtUtc As DateTime)
        If String.IsNullOrWhiteSpace(accessToken) Then Throw New ArgumentException("Access token is required.", NameOf(accessToken))
        If expiresAtUtc.Kind <> DateTimeKind.Utc Then expiresAtUtc = expiresAtUtc.ToUniversalTime()
        WithLock(Function()
            Dim state As CLSelectionCredentialState = ReadOrCreateState()
            Dim clearBytes As Byte() = Encoding.UTF8.GetBytes(accessToken)
            Try
                state.ProtectedAccessToken = Convert.ToBase64String(
                    ProtectedData.Protect(clearBytes, Entropy, DataProtectionScope.LocalMachine))
            Finally
                Array.Clear(clearBytes, 0, clearBytes.Length)
            End Try
            state.TokenExpiresAtUtc = expiresAtUtc
            SaveState(state)
            Return Nothing
        End Function)
    End Sub

    Public Shared Sub ClearAccessToken()
        WithLock(Function()
            Dim state As CLSelectionCredentialState = ReadOrCreateState()
            state.ProtectedAccessToken = Nothing
            state.TokenExpiresAtUtc = Nothing
            SaveState(state)
            Return Nothing
        End Function)
    End Sub

    Private Shared Function ReadOrCreateState() As CLSelectionCredentialState
        If Not File.Exists(StateFilePath) Then
            Return New CLSelectionCredentialState With {.InstallationId = Guid.NewGuid().ToString("D")}
        End If
        Try
            Dim state = JsonSerializer.Deserialize(Of CLSelectionCredentialState)(
                File.ReadAllText(StateFilePath, Encoding.UTF8), SerializerOptions)
            If state Is Nothing OrElse state.SchemaVersion <> 1 Then Throw New InvalidDataException("Unsupported credential state schema.")
            Dim installationId As Guid
            If Not Guid.TryParse(state.InstallationId, installationId) Then Throw New InvalidDataException("Invalid installation identifier.")
            If String.IsNullOrWhiteSpace(state.ProtectedAccessToken) Xor Not state.TokenExpiresAtUtc.HasValue Then
                Throw New InvalidDataException("Incomplete installation credential.")
            End If
            Return state
        Catch ex As JsonException
            Throw New InvalidDataException("The installation credential state contains invalid JSON.", ex)
        End Try
    End Function

    Private Shared Function ToSnapshot(state As CLSelectionCredentialState) As CLSelectionCredentialSnapshot
        Dim token As String = Nothing
        If Not String.IsNullOrWhiteSpace(state.ProtectedAccessToken) Then
            Try
                Dim protectedBytes As Byte() = Convert.FromBase64String(state.ProtectedAccessToken)
                Dim clearBytes As Byte() = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.LocalMachine)
                Try
                    token = Encoding.UTF8.GetString(clearBytes)
                Finally
                    Array.Clear(clearBytes, 0, clearBytes.Length)
                End Try
            Catch ex As Exception When TypeOf ex Is CryptographicException OrElse TypeOf ex Is FormatException
                Throw New InvalidDataException("The installation credential cannot be decrypted on this computer.", ex)
            End Try
        End If
        Return New CLSelectionCredentialSnapshot With {
            .InstallationId = state.InstallationId,
            .AccessToken = token,
            .TokenExpiresAtUtc = state.TokenExpiresAtUtc
        }
    End Function

    Private Shared Sub SaveState(state As CLSelectionCredentialState)
        Dim directoryPath As String = Path.GetDirectoryName(StateFilePath)
        Directory.CreateDirectory(directoryPath)
        Dim temporaryPath As String = StateFilePath & ".tmp-" & Guid.NewGuid().ToString("N")
        Dim backupPath As String = temporaryPath & ".bak"
        Try
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(state, SerializerOptions), New UTF8Encoding(False))
            If File.Exists(StateFilePath) Then
                File.Replace(temporaryPath, StateFilePath, backupPath, True)
                If File.Exists(backupPath) Then File.Delete(backupPath)
            Else
                File.Move(temporaryPath, StateFilePath)
            End If
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
            If File.Exists(backupPath) Then File.Delete(backupPath)
        End Try
    End Sub

    Private Shared Function WithLock(Of TResult)(action As Func(Of TResult)) As TResult
        Using mutex As New Mutex(False, StateMutexName)
            Dim acquired As Boolean
            Try
                Try
                    acquired = mutex.WaitOne(TimeSpan.FromSeconds(15))
                Catch ex As AbandonedMutexException
                    acquired = True
                End Try
                If Not acquired Then Throw New TimeoutException("The installation credential is busy.")
                Return action()
            Finally
                If acquired Then mutex.ReleaseMutex()
            End Try
        End Using
    End Function

End Class

