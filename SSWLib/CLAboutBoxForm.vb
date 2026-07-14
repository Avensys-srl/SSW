Imports ClimaLombarda.Common

Public NotInheritable Class CLAboutBoxForm

    Private Const HistoryFileName As String = "History.txt"

	Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click
		Me.Close()
	End Sub

    Private ReadOnly Property Environment As CLEnvironment
        Get
            Return CLEnvironment.Current
        End Get
    End Property

    Public Sub New()

        ' Chiamata richiesta dalla finestra di progettazione.
        InitializeComponent()

        wbbCustomerInfo.DocumentText = Environment.SSWInfo.CustomerInfo
        wbbCustomerInfo.Visible = Not String.IsNullOrEmpty(wbbCustomerInfo.DocumentText)

        ' Aggiungere le eventuali istruzioni di inizializzazione dopo la chiamata a InitializeComponent().
        Me.Icon = Environment.CustomerIcon
        Me.PictureBox1.Image = Environment.CustomerLogo

        UpdateLocalization()

        lblExternalLeakage.Text = CLEnvironment.ExternalLeakage
        lblInternalLeakage.Text = CLEnvironment.InternalLeakage
        lblAirflowPressure.Text = CLEnvironment.AirflowPressure
        lblElectricPowerInput.Text = CLEnvironment.ElectricPowerInput
        lblNoiseLevel.Text = CLEnvironment.NoiseLevel

    End Sub

    Public Sub UpdateLocalization()

        lblApplicationName.Text = Environment.SSWInfo.SelectionSoftwareTitle

        lblApplicationRelease.Text = String.Format("{0}: {1}   {2}",
         Environment.Localization.GetString(CLMessageResources.Release.ToString()),
         Environment.SSWInfo.ReleaseVersion.ToString(),
         Environment.SSWInfo.ReleaseDate.ToString("dd/MMMM/yyyy"))

        lblDatabaseRelease.Text = Environment.DatabaseCompatibility.ToDisplayString()

        lblTitle.Text = Environment.Localization.GetString(CLMessageResources.AboutBoxForm_Title.ToString())

        lblExternalLeakageTitle.Text = Environment.Localization.GetString(CLMessageResources.AboutBoxForm_ExternalLeakage.ToString())

        lblInternalLeakageTitle.Text = Environment.Localization.GetString(CLMessageResources.AboutBoxForm_InternalLeakage.ToString())

        lblAirflowPressureTitle.Text = Environment.Localization.GetString(CLMessageResources.AboutBoxForm_AirflowPressure.ToString())

        lblElectricPowerInputTitle.Text = Environment.Localization.GetString(CLMessageResources.AboutBoxForm_ElectricPowerInput.ToString())

        lblNoiseLevelTitle.Text = Environment.Localization.GetString(CLMessageResources.AboutBoxForm_NoiseLevel.ToString())

        Me.Text = Environment.Localization.GetString(CLMessageResources.About.ToString())
    End Sub

    Private Sub wbbCustomerInfo_Navigating(sender As System.Object, e As System.Windows.Forms.WebBrowserNavigatingEventArgs) Handles wbbCustomerInfo.Navigating
        If e.Url.LocalPath <> "blank" Then
            Try
                Process.Start(e.Url.OriginalString)
            Catch ex As Exception
            End Try
            e.Cancel = True
        End If
    End Sub

    Private Sub btnChangelog_Click(sender As Object, e As EventArgs) Handles btnChangelog.Click
        Using changelogForm As New Form()
            changelogForm.Text = "Changelog"
            changelogForm.StartPosition = FormStartPosition.CenterParent
            changelogForm.Size = New Size(620, 420)
            changelogForm.MinimizeBox = False
            changelogForm.MaximizeBox = False
            changelogForm.ShowIcon = False

            Dim txtChangelog As New TextBox()
            txtChangelog.Multiline = True
            txtChangelog.ReadOnly = True
            txtChangelog.ScrollBars = ScrollBars.Vertical
            txtChangelog.WordWrap = True
            txtChangelog.Font = New Font("Segoe UI", 9.0!)
            txtChangelog.Dock = DockStyle.Fill
            txtChangelog.Text = LoadChangelogText()

            Dim pnlButtons As New Panel()
            pnlButtons.Dock = DockStyle.Bottom
            pnlButtons.Height = 42

            Dim btnClose As New Button()
            btnClose.Text = "Close"
            btnClose.Size = New Size(75, 26)
            btnClose.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            btnClose.Location = New Point(changelogForm.ClientSize.Width - btnClose.Width - 10, 8)

            AddHandler pnlButtons.Resize,
                Sub(senderResize As Object, eResize As EventArgs)
                    btnClose.Left = pnlButtons.ClientSize.Width - btnClose.Width - 10
                End Sub

            AddHandler btnClose.Click,
                Sub(senderClose As Object, eClose As EventArgs)
                    changelogForm.Close()
                End Sub

            pnlButtons.Controls.Add(btnClose)
            changelogForm.Controls.Add(txtChangelog)
            changelogForm.Controls.Add(pnlButtons)
            changelogForm.ShowDialog(Me)
        End Using
    End Sub

    Private Shared Function LoadChangelogText() As String
        Try
            Dim baseDirectory As New IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)

            For level As Integer = 0 To 4
                If baseDirectory Is Nothing Then
                    Exit For
                End If

                Dim candidatePaths As String() = {
                    IO.Path.Combine(baseDirectory.FullName, HistoryFileName),
                    IO.Path.Combine(baseDirectory.FullName, "SSWLib", HistoryFileName)
                }

                For Each candidatePath As String In candidatePaths
                    If IO.File.Exists(candidatePath) Then
                        Return NormalizeLineEndings(IO.File.ReadAllText(candidatePath))
                    End If
                Next

                baseDirectory = baseDirectory.Parent
            Next
        Catch ex As Exception
        End Try

        Return "Changelog file not found."
    End Function

    Private Shared Function NormalizeLineEndings(value As String) As String
        If value Is Nothing Then
            Return ""
        End If

        Return value.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Replace(vbLf, vbCrLf)
    End Function

End Class
