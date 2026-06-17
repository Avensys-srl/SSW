Imports System.Net.Http
Imports System.IO
Imports System.Text.Json ' Or Newtonsoft.Json if you prefer/use that


' Helper class to deserialize the JSON response from the PHP script
Public Class SoftwareVersionInfo
    Public Property latest_version As String
    Public Property download_url As String
    Public Property filename As String
    Public Property [error] As String ' Correct property name with []
End Class

Public Class UpdateManager

    Private Const CheckUpdateUrl As String = "https://www.avensys-srl.com/api/ssw_check_update.php"

    ' Async Function that returns a Task, making it awaitable
    Public Shared Async Function CheckForSoftwareUpdate() As Task

        Dim currentAppVersion As Version = Nothing
        Dim latestServerVersion As Version = Nothing

        ' --- 1. Get Current Application Version ---
        Try ' Start Try 1
            currentAppVersion = CLEnvironment.Current.SSWInfo.ReleaseVersion

            If currentAppVersion Is Nothing Then ' Start If 1
                MessageBox.Show("Could not determine the current application version.",
                                "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return ' Exit Function
            End If ' End If 1

        Catch ex As Exception
            MessageBox.Show($"Error retrieving current version: {ex.Message}",
                            "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return ' Exit Function
        End Try ' End Try 1

        ' --- 2. Contact PHP script to get the latest version ---
        Dim versionInfo As SoftwareVersionInfo = Nothing
        Using client As New HttpClient() ' Start Using
            Try ' Start Try 2
                client.Timeout = TimeSpan.FromSeconds(15) ' Example timeout
                Dim response As HttpResponseMessage = Await client.GetAsync(CheckUpdateUrl)

                If response.IsSuccessStatusCode Then ' Start If 2 (HTTP Success)
                    Dim jsonString As String = Await response.Content.ReadAsStringAsync()
                    Try ' Start Try 3 (JSON Parse)
                        versionInfo = JsonSerializer.Deserialize(Of SoftwareVersionInfo)(jsonString, New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})

                        ' Check if the API returned a specific error in JSON
                        If versionInfo IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(versionInfo.[error]) Then ' Start If 3 (API Error)
                            MessageBox.Show($"The API returned an error: {versionInfo.[error]}",
                                            "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Return ' Exit Function
                        End If ' End If 3 (API Error)

                        ' Check if we actually got the version string
                        If versionInfo Is Nothing OrElse String.IsNullOrWhiteSpace(versionInfo.latest_version) Then ' Start If 4 (Invalid/Missing Version)
                            MessageBox.Show("The server response did not contain a valid version number.",
                                            "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Return ' Exit Function
                        End If ' End If 4 (Invalid/Missing Version)

                    Catch jsonEx As Exception ' Catch for Try 3 (JSON Parse)
                        MessageBox.Show($"Error parsing JSON response: {jsonEx.Message}",
                                        "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return ' Exit Function
                    End Try ' End Try 3 (JSON Parse)
                Else ' Else for If 2 (HTTP Success)
                    ' HTTP error (e.g., 404 Not Found, 503 Service Unavailable)
                    MessageBox.Show($"Error communicating with the server: {response.StatusCode} - {response.ReasonPhrase}",
                                    "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return ' Exit Function
                End If ' End If 2 (HTTP Success)

            Catch netEx As HttpRequestException ' Catch for Try 2
                MessageBox.Show($"Network error during update check: {netEx.Message}",
                                "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return ' Exit Function
            Catch taskEx As TaskCanceledException ' Catch for Try 2
                MessageBox.Show("Timeout during update check.",
                                "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return ' Exit Function
            Catch ex As Exception ' Catch for Try 2
                MessageBox.Show($"Unexpected error during check: {ex.Message}",
                                "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return ' Exit Function
            End Try ' End Try 2 (HTTP Request)
        End Using ' End Using (HttpClient)

        ' --- 3. Parse the received version string ---
        If versionInfo IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(versionInfo.latest_version) Then ' Start If 5 (Check versionInfo before parsing)
            Try ' Start Try 4 (Version Parse)
                latestServerVersion = Version.Parse(versionInfo.latest_version)
            Catch ex As FormatException ' Catch for Try 4
                MessageBox.Show($"The version format received from the server ('{versionInfo.latest_version}') is invalid.",
                                "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return ' Exit Function
            Catch ex As Exception ' Catch for Try 4
                MessageBox.Show($"Error parsing received version: {ex.Message}",
                                "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return ' Exit Function
            End Try ' End Try 4 (Version Parse)
        Else ' Else for If 5
            MessageBox.Show("Could not get version from the server.",
                            "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return ' Exit Function
        End If ' End If 5 (Check versionInfo before parsing)

        ' --- 4. Compare versions ---
        If currentAppVersion IsNot Nothing AndAlso latestServerVersion IsNot Nothing Then ' Start If 6 (Check both versions exist)
            Try ' Start Try 5 (Comparison)
                If latestServerVersion > currentAppVersion Then ' Start If 7 (Compare versions)
                    Dim message As String = $"New update available!{vbCrLf}{vbCrLf}" &
                                            $"Current version: {currentAppVersion}{vbCrLf}" &
                                            $"New version: {latestServerVersion}{vbCrLf}{vbCrLf}" &
                                            "Download and start the installer now?"
                    Dim title As String = "Software Update Available"
                    Dim buttons As MessageBoxButtons = MessageBoxButtons.YesNo
                    Dim icon As MessageBoxIcon = MessageBoxIcon.Information

                    Dim userChoice As DialogResult = MessageBox.Show(message, title, buttons, icon)

                    If userChoice = DialogResult.Yes Then
                        Await DownloadAndStartInstaller(versionInfo)
                    End If

                Else ' Else for If 7 (Versions are same or current is newer)
                    ' Optional: Show message that app is up to date (perhaps only in debug mode)
                    ' MessageBox.Show("Application is up to date.", "Software Update", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If ' End If 7 (Compare versions)

            Catch ex As Exception ' Catch for Try 5
                MessageBox.Show($"Error comparing versions: {ex.Message}",
                                "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try ' End Try 5 (Comparison)
        End If ' End If 6 (Check both versions exist)

        ' If the function reaches here without prior errors, the Task completes normally.

    End Function ' End Function CheckForSoftwareUpdate

    Private Shared Async Function DownloadAndStartInstaller(versionInfo As SoftwareVersionInfo) As Task
        Dim downloadUrl As String = versionInfo.download_url
        If String.IsNullOrWhiteSpace(downloadUrl) Then
            downloadUrl = "https://www.avensys-srl.com/api/ssw_download.php"
        End If

        Dim fileName As String = versionInfo.filename
        If String.IsNullOrWhiteSpace(fileName) Then
            fileName = System.IO.Path.GetFileName(New Uri(downloadUrl).LocalPath)
        End If
        If String.IsNullOrWhiteSpace(fileName) Then
            fileName = "SSW_Update.exe"
        End If

        Dim localPath As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName)
        Dim progressForm As DownloadProgressForm = Nothing

        Try
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

            progressForm.Close()
            progressForm = Nothing

            Dim psi As New ProcessStartInfo()
            psi.FileName = localPath
            psi.UseShellExecute = True
            Process.Start(psi)
        Catch ex As Exception
            If progressForm IsNot Nothing Then
                progressForm.Close()
            End If

            MessageBox.Show($"Could not download or start the update installer.{vbCrLf}" &
                            $"Error: {ex.Message}{vbCrLf}{vbCrLf}" &
                            $"You can download it manually from:{vbCrLf}{downloadUrl}",
                            "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
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
