<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CLReportViewerForm
	Inherits System.Windows.Forms.Form

	'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Richiesto da Progettazione Windows Form
	Private components As System.ComponentModel.IContainer

	'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
	'Può essere modificata in Progettazione Windows Form.  
	'Non modificarla nell'editor del codice.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.rpvReport = New Microsoft.Reporting.WinForms.ReportViewer()
		Me.SuspendLayout()
		'
		'rpvReport
		'
		Me.rpvReport.Dock = System.Windows.Forms.DockStyle.Fill
		Me.rpvReport.DocumentMapCollapsed = True
		Me.rpvReport.LocalReport.DisplayName = "MainReport"
		Me.rpvReport.Location = New System.Drawing.Point(0, 0)
		Me.rpvReport.Name = "rpvReport"
		Me.rpvReport.PageCountMode = Microsoft.Reporting.WinForms.PageCountMode.Actual
		Me.rpvReport.Size = New System.Drawing.Size(563, 466)
		Me.rpvReport.TabIndex = 0
		'
		'CLReportViewerForm
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(563, 466)
		Me.Controls.Add(Me.rpvReport)
		Me.Name = "CLReportViewerForm"
		Me.Text = "Report"
		Me.ResumeLayout(False)

	End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Friend WithEvents rpvReport As Microsoft.Reporting.WinForms.ReportViewer

    Public Sub New()

        ' La chiamata è richiesta dalla finestra di progettazione.
        InitializeComponent()

        ' Aggiungere le eventuali istruzioni di inizializzazione dopo la chiamata a InitializeComponent().

    End Sub
End Class
