Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Threading

Public NotInheritable Class CLSelectionInstallationState

    Public Property SchemaVersion As Integer = 1
    Public Property InstallationCode As String
    Public Property LastDraftNumber As Long

End Class

Public NotInheritable Class CLSelectionInstallationStateStore

    Private Const StateMutexName As String = "Local\Avensys.SSW.TechnicalSelectionDraftCounter"
    Private Const InstallationAlphabet As String = "23456789ABCDEFGHJKMNPQRSTUVWXYZ"
    Private Shared ReadOnly SerializerOptions As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        .PropertyNameCaseInsensitive = True,
        .WriteIndented = True
    }

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property StateFilePath As String
        Get
            Dim testPath As String = System.Environment.GetEnvironmentVariable("SSW_SELECTION_STATE_PATH")
            If Not String.IsNullOrWhiteSpace(testPath) Then Return Path.GetFullPath(testPath)
            Return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                "Avensys",
                "SSW",
                "technical-selection-state.json")
        End Get
    End Property

    Public Shared Function NextDraftReference() As String
        Using mutex As New Mutex(False, StateMutexName)
            Dim acquired As Boolean
            Try
                Try
                    acquired = mutex.WaitOne(TimeSpan.FromSeconds(15))
                Catch ex As AbandonedMutexException
                    acquired = True
                End Try
                If Not acquired Then
                    Throw New TimeoutException("The local technical selection counter is busy.")
                End If

                Dim state As CLSelectionInstallationState = LoadOrCreate()
                If state.LastDraftNumber = Long.MaxValue Then
                    Throw New OverflowException("The local technical selection counter is exhausted.")
                End If
                state.LastDraftNumber += 1
                SaveState(state)
                Return String.Format(Globalization.CultureInfo.InvariantCulture,
                    "D-{0}-{1:000000}",
                    state.InstallationCode,
                    state.LastDraftNumber)
            Finally
                If acquired Then mutex.ReleaseMutex()
            End Try
        End Using
    End Function

    Public Shared Function GetInstallationCode() As String
        Using mutex As New Mutex(False, StateMutexName)
            Dim acquired As Boolean
            Try
                Try
                    acquired = mutex.WaitOne(TimeSpan.FromSeconds(15))
                Catch ex As AbandonedMutexException
                    acquired = True
                End Try
                If Not acquired Then Throw New TimeoutException("The local technical selection state is busy.")
                Dim state As CLSelectionInstallationState = LoadOrCreate()
                SaveState(state)
                Return state.InstallationCode
            Finally
                If acquired Then mutex.ReleaseMutex()
            End Try
        End Using
    End Function

    Private Shared Function LoadOrCreate() As CLSelectionInstallationState
        If Not File.Exists(StateFilePath) Then
            Return New CLSelectionInstallationState With {.InstallationCode = GenerateInstallationCode()}
        End If

        Try
            Dim state As CLSelectionInstallationState = JsonSerializer.Deserialize(Of CLSelectionInstallationState)(
                File.ReadAllText(StateFilePath, Encoding.UTF8),
                SerializerOptions)
            If state Is Nothing OrElse state.SchemaVersion <> 1 OrElse
                String.IsNullOrWhiteSpace(state.InstallationCode) OrElse state.InstallationCode.Length <> 4 OrElse
                state.InstallationCode.Any(Function(character) InstallationAlphabet.IndexOf(character) < 0) OrElse
                state.LastDraftNumber < 0 Then
                Throw New InvalidDataException("The local technical selection state is invalid.")
            End If
            Return state
        Catch ex As JsonException
            Throw New InvalidDataException("The local technical selection state contains invalid JSON.", ex)
        End Try
    End Function

    Private Shared Sub SaveState(state As CLSelectionInstallationState)
        Dim directoryPath As String = Path.GetDirectoryName(StateFilePath)
        Directory.CreateDirectory(directoryPath)
        Dim temporaryPath As String = StateFilePath & ".tmp-" & Guid.NewGuid().ToString("N")
        Dim replacementBackup As String = temporaryPath & ".bak"
        Try
            File.WriteAllText(temporaryPath,
                JsonSerializer.Serialize(state, SerializerOptions),
                New UTF8Encoding(False))
            If File.Exists(StateFilePath) Then
                File.Replace(temporaryPath, StateFilePath, replacementBackup, True)
                If File.Exists(replacementBackup) Then File.Delete(replacementBackup)
            Else
                File.Move(temporaryPath, StateFilePath)
            End If
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
            If File.Exists(replacementBackup) Then File.Delete(replacementBackup)
        End Try
    End Sub

    Private Shared Function GenerateInstallationCode() As String
        Dim result As New StringBuilder(4)
        Using generator As RandomNumberGenerator = RandomNumberGenerator.Create()
            Dim buffer(0) As Byte
            While result.Length < 4
                generator.GetBytes(buffer)
                Dim usableRange As Integer = 256 - (256 Mod InstallationAlphabet.Length)
                If buffer(0) < usableRange Then
                    result.Append(InstallationAlphabet(buffer(0) Mod InstallationAlphabet.Length))
                End If
            End While
        End Using
        Return result.ToString()
    End Function

End Class
