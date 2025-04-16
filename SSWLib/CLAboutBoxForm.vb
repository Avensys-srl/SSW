Imports ClimaLombarda.Common

Public NotInheritable Class CLAboutBoxForm

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


End Class
