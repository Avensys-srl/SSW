<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
 Partial Class CLPleaseWaitForm
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
		Me.lblMessage = New System.Windows.Forms.Label()
		Me.SuspendLayout()
		'
		'lblMessage
		'
		Me.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill
		Me.lblMessage.Location = New System.Drawing.Point(0, 0)
		Me.lblMessage.Name = "lblMessage"
		Me.lblMessage.Size = New System.Drawing.Size(230, 76)
		Me.lblMessage.TabIndex = 0
		Me.lblMessage.Text = "Please wait just few seconds..."
		Me.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblMessage.UseWaitCursor = True
		'
		'CLPleaseWaitForm
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(230, 76)
		Me.Controls.Add(Me.lblMessage)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
		Me.Location = New System.Drawing.Point(800, 600)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "CLPleaseWaitForm"
		Me.ShowIcon = False
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
		Me.Text = "Saving Process"
		Me.UseWaitCursor = True
		Me.ResumeLayout(False)

	End Sub
	Friend WithEvents lblMessage As System.Windows.Forms.Label
End Class
