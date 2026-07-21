Imports System.Net.Http
Imports System.IO
Imports System.Text.Json ' Or Newtonsoft.Json if you prefer/use that
Imports System.Security.Cryptography


' Helper class to deserialize the JSON response from the PHP script
Public Class SoftwareVersionInfo
    Public Property latest_version As String
    Public Property download_url As String
    Public Property filename As String
    Public Property manifest_version As Integer
    Public Property verified_manifest As Boolean
    Public Property sha256 As String
    Public Property size_bytes As Long?
    Public Property [error] As String ' Correct property name with []
End Class

Public Class UpdateManager

    Private Const CheckUpdateUrl As String = "https://www.avensys-srl.com/api/ssw_check_update.php"
    Private Shared ReadOnly CheckSemaphore As New System.Threading.SemaphoreSlim(1, 1)

    Public Shared Async Function CheckForSoftwareUpdate(Optional interactive As Boolean = False) As Task
        Await CheckSemaphore.WaitAsync()
        Try
            Dim currentAppVersion As Version
            Try
                currentAppVersion = CLEnvironment.Current.SSWInfo.ReleaseVersion
            Catch ex As Exception
                ShowCheckError(interactive, ex.Message)
                Return
            End Try

            If currentAppVersion Is Nothing Then
                ShowCheckError(interactive, "Could not determine the current application version.")
                Return
            End If

            Dim versionInfo As SoftwareVersionInfo
            Try
                Using client As New HttpClient()
                    client.Timeout = TimeSpan.FromSeconds(15)
                    Dim checkUri As String = CheckUpdateUrl & "?current_version=" &
                        Uri.EscapeDataString(currentAppVersion.ToString())
                    Using response As HttpResponseMessage = Await client.GetAsync(checkUri)
                        If Not response.IsSuccessStatusCode Then
                            ShowCheckError(interactive, $"{response.StatusCode} - {response.ReasonPhrase}")
                            Return
                        End If

                        Dim jsonString As String = Await response.Content.ReadAsStringAsync()
                        versionInfo = JsonSerializer.Deserialize(Of SoftwareVersionInfo)(jsonString,
                            New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})
                    End Using
                End Using
            Catch ex As TaskCanceledException
                ShowCheckError(interactive, "Timeout.")
                Return
            Catch ex As Exception
                ShowCheckError(interactive, ex.Message)
                Return
            End Try

            If versionInfo Is Nothing Then
                ShowCheckError(interactive, "The server response is empty.")
                Return
            End If

            If Not String.IsNullOrWhiteSpace(versionInfo.[error]) Then
                ShowCheckError(interactive, versionInfo.[error])
                Return
            End If

            Dim latestServerVersion As Version = Nothing
            If String.IsNullOrWhiteSpace(versionInfo.latest_version) OrElse
                Not Version.TryParse(versionInfo.latest_version, latestServerVersion) Then
                ShowCheckError(interactive, "The server response does not contain a valid version number.")
                Return
            End If

            If latestServerVersion > currentAppVersion Then
                If Not versionInfo.verified_manifest OrElse versionInfo.manifest_version < 1 OrElse
                    String.IsNullOrWhiteSpace(versionInfo.sha256) OrElse versionInfo.sha256.Length <> 64 OrElse
                    Not versionInfo.size_bytes.HasValue OrElse versionInfo.size_bytes.Value <= 0 Then
                    ShowCheckError(interactive, PackageIntegrityError())
                    Return
                End If
                Dim message As String = LocalizedText(CLMessageResources.Update_NewAvailable,
                                                       "A new software update is available!") & vbCrLf & vbCrLf &
                    LocalizedText(CLMessageResources.Update_CurrentVersion, "Current version") & ": " & currentAppVersion.ToString() & vbCrLf &
                    LocalizedText(CLMessageResources.Update_NewVersion, "New version") & ": " & latestServerVersion.ToString() & vbCrLf & vbCrLf &
                    LocalizedText(CLMessageResources.Update_DownloadQuestion, "Download and start the installer now?")

                Dim userChoice As DialogResult = MessageBox.Show(message,
                    LocalizedText(CLMessageResources.Update_Title, "Software Update"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information)

                If userChoice = DialogResult.Yes Then
                    Await DownloadAndStartInstaller(versionInfo)
                End If
            ElseIf interactive Then
                MessageBox.Show(LocalizedText(CLMessageResources.Update_UpToDate, "The software is up to date."),
                                LocalizedText(CLMessageResources.Update_Title, "Software Update"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Finally
            CheckSemaphore.Release()
        End Try
    End Function

    Private Shared Sub ShowCheckError(interactive As Boolean, details As String)
        If Not interactive Then
            Return
        End If

        MessageBox.Show(String.Format(
                            LocalizedText(CLMessageResources.Update_CheckFailed, "Unable to check for updates: {0}"),
                            details),
                        LocalizedText(CLMessageResources.Update_ErrorTitle, "Update Error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Shared Function LocalizedText(resource As CLMessageResources, fallback As String) As String
        Try
            Dim value As String = CLEnvironment.Current.Localization.GetString(resource.ToString())
            If Not String.IsNullOrWhiteSpace(value) AndAlso Not value.StartsWith("@@", StringComparison.Ordinal) Then
                Return value
            End If
        Catch
        End Try

        Return fallback
    End Function

    Private Shared Async Function DownloadAndStartInstaller(versionInfo As SoftwareVersionInfo) As Task
        Dim downloadUrl As String = versionInfo.download_url
        If String.IsNullOrWhiteSpace(downloadUrl) Then
            downloadUrl = "https://www.avensys-srl.com/api/ssw_download.php?source=update&from_version=" &
                Uri.EscapeDataString(Application.ProductVersion)
        End If

        Dim fileName As String = versionInfo.filename
        If String.IsNullOrWhiteSpace(fileName) Then
            fileName = System.IO.Path.GetFileName(New Uri(downloadUrl).LocalPath)
        End If
        If String.IsNullOrWhiteSpace(fileName) Then
            fileName = "SSW_Update.exe"
        End If
        Dim localPath As String = String.Empty
        Dim progressForm As DownloadProgressForm = Nothing

        Try
            fileName = Path.GetFileName(fileName)
            If Not fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException(PackageIntegrityError())
            End If
            Dim downloadUri As New Uri(downloadUrl)
            If downloadUri.Scheme <> Uri.UriSchemeHttps OrElse
                Not String.Equals(downloadUri.Host, "www.avensys-srl.com", StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException(PackageIntegrityError())
            End If
            localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName)

            Using client As New HttpClient()
                client.Timeout = TimeSpan.FromMinutes(10)

                progressForm = New DownloadProgressForm()
                progressForm.Show()
                progressForm.Refresh()

                Using response As HttpResponseMessage = Await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead)
                    If Not response.IsSuccessStatusCode Then
                        Throw New InvalidOperationException($"Download failed: {response.StatusCode} - {response.ReasonPhrase}")
                    End If

                    Dim totalBytes As Long? = response.Content.Headers.ContentLength
                    Using sourceStream As Stream = Await response.Content.ReadAsStreamAsync()
                        Using targetStream As New FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None)
                            Await CopyToFileWithProgress(sourceStream, targetStream, totalBytes, progressForm)
                        End Using
                    End Using
                End Using
            End Using

            VerifyDownloadedInstaller(localPath, versionInfo)

            progressForm.Close()
            progressForm = Nothing

            Dim psi As New ProcessStartInfo()
            psi.FileName = localPath
            psi.Arguments = "/CLOSEAPPLICATIONS /RESTARTAPPLICATIONS"
            psi.UseShellExecute = True
            Process.Start(psi)
            Application.Exit()
        Catch ex As Exception
            If progressForm IsNot Nothing Then
                progressForm.Close()
            End If
            Try
                If Not String.IsNullOrWhiteSpace(localPath) AndAlso File.Exists(localPath) Then File.Delete(localPath)
            Catch
            End Try

            MessageBox.Show($"Could not download or start the update installer.{vbCrLf}" &
                            $"Error: {ex.Message}{vbCrLf}{vbCrLf}" &
                            $"You can download it manually from:{vbCrLf}{downloadUrl}",
                            "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Function

    Private Shared Sub VerifyDownloadedInstaller(filePath As String, versionInfo As SoftwareVersionInfo)
        Dim fileInfo As New FileInfo(filePath)
        If Not versionInfo.size_bytes.HasValue OrElse fileInfo.Length <> versionInfo.size_bytes.Value Then
            Throw New InvalidDataException(PackageIntegrityError())
        End If
        Dim actualHash As String
        Using algorithm As SHA256 = SHA256.Create()
            Using stream As FileStream = System.IO.File.OpenRead(filePath)
                actualHash = String.Concat(algorithm.ComputeHash(stream).Select(Function(value) value.ToString("X2")))
            End Using
        End Using
        If Not String.Equals(actualHash, versionInfo.sha256, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException(PackageIntegrityError())
        End If
    End Sub

    Private Shared Function PackageIntegrityError() As String
        Return LocalizedText(CLMessageResources.Update_PackageIntegrityFailed,
                             "The update package failed integrity verification.")
    End Function

    Private Shared Async Function CopyToFileWithProgress(sourceStream As Stream,
                                                         targetStream As Stream,
                                                         totalBytes As Long?,
                                                         progressForm As DownloadProgressForm) As Task
        Dim buffer(81919) As Byte
        Dim downloadedBytes As Long = 0
        Dim bytesRead As Integer = Await sourceStream.ReadAsync(buffer, 0, buffer.Length)

        While bytesRead > 0
            Await targetStream.WriteAsync(buffer, 0, bytesRead)
            downloadedBytes += bytesRead
            progressForm.UpdateProgress(downloadedBytes, totalBytes)
            Application.DoEvents()

            bytesRead = Await sourceStream.ReadAsync(buffer, 0, buffer.Length)
        End While
    End Function

End Class

Friend Class DownloadProgressForm
    Inherits Form

    Private ReadOnly progressBar As ProgressBar
    Private ReadOnly statusLabel As Label

    Public Sub New()
        Text = "Software Update"
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterScreen
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ControlBox = False
        Width = 420
        Height = 135

        statusLabel = New Label()
        statusLabel.AutoSize = False
        statusLabel.Left = 16
        statusLabel.Top = 18
        statusLabel.Width = 370
        statusLabel.Height = 22
        statusLabel.Text = "Downloading update..."

        progressBar = New ProgressBar()
        progressBar.Left = 16
        progressBar.Top = 50
        progressBar.Width = 370
        progressBar.Height = 22
        progressBar.Minimum = 0
        progressBar.Maximum = 100
        progressBar.Style = ProgressBarStyle.Marquee
        progressBar.MarqueeAnimationSpeed = 30

        Controls.Add(statusLabel)
        Controls.Add(progressBar)
    End Sub

    Public Sub UpdateProgress(downloadedBytes As Long, totalBytes As Long?)
        If InvokeRequired Then
            BeginInvoke(New Action(Of Long, Long?)(AddressOf UpdateProgress), downloadedBytes, totalBytes)
            Return
        End If

        If totalBytes.HasValue AndAlso totalBytes.Value > 0 Then
            Dim percent As Integer = CInt(Math.Min(100, Math.Truncate(downloadedBytes * 100.0R / totalBytes.Value)))
            progressBar.Style = ProgressBarStyle.Blocks
            progressBar.MarqueeAnimationSpeed = 0
            progressBar.Value = percent
            statusLabel.Text = $"Downloading update... {percent}% ({FormatMegabytes(downloadedBytes)} / {FormatMegabytes(totalBytes.Value)})"
        Else
            statusLabel.Text = $"Downloading update... {FormatMegabytes(downloadedBytes)}"
        End If
    End Sub

    Private Shared Function FormatMegabytes(bytes As Long) As String
        Return $"{bytes / 1024.0R / 1024.0R:0.0} MB"
    End Function
End Class
