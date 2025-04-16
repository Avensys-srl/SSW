Imports System.Net.Http
Imports System.Text.Json ' Or Newtonsoft.Json if you prefer/use that


' Helper class to deserialize the JSON response from the PHP script
Public Class SoftwareVersionInfo
    Public Property latest_version As String
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
                    ' --- DOWNLOAD MODIFICATION ---
                    ' 1. Prepare message and buttons
                    Dim message As String = $"New update available!{vbCrLf}{vbCrLf}" &
                                            $"Current version: {currentAppVersion}{vbCrLf}" &
                                            $"New version: {latestServerVersion}{vbCrLf}{vbCrLf}" &
                                            "Download now?" ' Question in English
                    Dim title As String = "Software Update Available" ' Title in English
                    Dim buttons As MessageBoxButtons = MessageBoxButtons.YesNo ' Standard Yes/No buttons
                    Dim icon As MessageBoxIcon = MessageBoxIcon.Information

                    ' 2. Show MessageBox and get user response
                    Dim userChoice As DialogResult = MessageBox.Show(message, title, buttons, icon)

                    ' 3. Check if user clicked "Yes"
                    If userChoice = DialogResult.Yes Then
                        ' 4. Start download by opening URL in default browser
                        Dim downloadUrl As String = "https://www.avensys-srl.com/api/ssw_download.php"
                        Try
                            Dim psi As New ProcessStartInfo()
                            psi.FileName = downloadUrl
                            psi.UseShellExecute = True ' Important to open URL/file with associated app
                            Process.Start(psi)
                        Catch ex As Exception
                            ' Handle potential errors opening the URL
                            MessageBox.Show($"Could not start the browser for download.{vbCrLf}" &
                                            $"Error: {ex.Message}{vbCrLf}{vbCrLf}" &
                                            $"You can download the update manually from:{vbCrLf}{downloadUrl}",
                                            "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error) ' Title and message in English
                        End Try
                    End If
                    ' --- END DOWNLOAD MODIFICATION ---

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

End Class