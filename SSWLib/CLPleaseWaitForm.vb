Imports ClimaLombarda.Common

Public Class CLPleaseWaitForm

	Sub New()

		' Chiamata richiesta dalla finestra di progettazione.
		InitializeComponent()

		' Aggiungere le eventuali istruzioni di inizializzazione dopo la chiamata a InitializeComponent().
        lblMessage.Text = CLEnvironment.Current.Localization.GetString(CLMessageResources.PleaseWaitForm_Message.ToString())
	End Sub


End Class
