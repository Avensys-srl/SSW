<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
 Partial Class CLAboutBoxForm
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
        Me.lblApplicationRelease = New System.Windows.Forms.Label()
        Me.lblApplicationName = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.lblInternalLeakageTitle = New System.Windows.Forms.Label()
        Me.lblAirflowPressureTitle = New System.Windows.Forms.Label()
        Me.lblElectricPowerInputTitle = New System.Windows.Forms.Label()
        Me.lblNoiseLevelTitle = New System.Windows.Forms.Label()
        Me.lblExternalLeakage = New System.Windows.Forms.Label()
        Me.lblElectricPowerInput = New System.Windows.Forms.Label()
        Me.lblNoiseLevel = New System.Windows.Forms.Label()
        Me.lblAirflowPressure = New System.Windows.Forms.Label()
        Me.lblInternalLeakage = New System.Windows.Forms.Label()
        Me.lblExternalLeakageTitle = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.pnlCustomerInfo = New System.Windows.Forms.Panel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.wbbCustomerInfo = New System.Windows.Forms.WebBrowser()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlCustomerInfo.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblApplicationRelease
        '
        Me.lblApplicationRelease.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.lblApplicationRelease.Location = New System.Drawing.Point(308, 192)
        Me.lblApplicationRelease.Name = "lblApplicationRelease"
        Me.lblApplicationRelease.Size = New System.Drawing.Size(240, 16)
        Me.lblApplicationRelease.TabIndex = 3
        Me.lblApplicationRelease.Text = "Release 0.4 beta - July 2013"
        Me.lblApplicationRelease.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblApplicationName
        '
        Me.lblApplicationName.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.lblApplicationName.Location = New System.Drawing.Point(9, 192)
        Me.lblApplicationName.Name = "lblApplicationName"
        Me.lblApplicationName.Size = New System.Drawing.Size(245, 16)
        Me.lblApplicationName.TabIndex = 4
        Me.lblApplicationName.Text = "CLRC Selection Software"
        Me.lblApplicationName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Button1
        '
        Me.Button1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button1.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Button1.Location = New System.Drawing.Point(480, 403)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(68, 26)
        Me.Button1.TabIndex = 5
        Me.Button1.Text = "&OK"
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.lblInternalLeakageTitle, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.lblAirflowPressureTitle, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.lblElectricPowerInputTitle, 0, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.lblNoiseLevelTitle, 0, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.lblExternalLeakage, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.lblElectricPowerInput, 1, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.lblNoiseLevel, 1, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.lblAirflowPressure, 1, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.lblInternalLeakage, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.lblExternalLeakageTitle, 0, 0)
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(12, 247)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.Padding = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel1.RowCount = 5
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(536, 150)
        Me.TableLayoutPanel1.TabIndex = 7
        '
        'lblInternalLeakageTitle
        '
        Me.lblInternalLeakageTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblInternalLeakageTitle.Location = New System.Drawing.Point(7, 32)
        Me.lblInternalLeakageTitle.Name = "lblInternalLeakageTitle"
        Me.lblInternalLeakageTitle.Size = New System.Drawing.Size(258, 28)
        Me.lblInternalLeakageTitle.TabIndex = 3
        Me.lblInternalLeakageTitle.Text = "Internal Leakage"
        Me.lblInternalLeakageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblAirflowPressureTitle
        '
        Me.lblAirflowPressureTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblAirflowPressureTitle.Location = New System.Drawing.Point(7, 60)
        Me.lblAirflowPressureTitle.Name = "lblAirflowPressureTitle"
        Me.lblAirflowPressureTitle.Size = New System.Drawing.Size(258, 28)
        Me.lblAirflowPressureTitle.TabIndex = 3
        Me.lblAirflowPressureTitle.Text = "Airflow / Pressure"
        Me.lblAirflowPressureTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblElectricPowerInputTitle
        '
        Me.lblElectricPowerInputTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblElectricPowerInputTitle.Location = New System.Drawing.Point(7, 88)
        Me.lblElectricPowerInputTitle.Name = "lblElectricPowerInputTitle"
        Me.lblElectricPowerInputTitle.Size = New System.Drawing.Size(258, 28)
        Me.lblElectricPowerInputTitle.TabIndex = 3
        Me.lblElectricPowerInputTitle.Text = "Electric power input"
        Me.lblElectricPowerInputTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblNoiseLevelTitle
        '
        Me.lblNoiseLevelTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblNoiseLevelTitle.Location = New System.Drawing.Point(7, 116)
        Me.lblNoiseLevelTitle.Name = "lblNoiseLevelTitle"
        Me.lblNoiseLevelTitle.Size = New System.Drawing.Size(258, 30)
        Me.lblNoiseLevelTitle.TabIndex = 3
        Me.lblNoiseLevelTitle.Text = "Noise Level"
        Me.lblNoiseLevelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblExternalLeakage
        '
        Me.lblExternalLeakage.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblExternalLeakage.Location = New System.Drawing.Point(271, 4)
        Me.lblExternalLeakage.Name = "lblExternalLeakage"
        Me.lblExternalLeakage.Size = New System.Drawing.Size(258, 28)
        Me.lblExternalLeakage.TabIndex = 3
        Me.lblExternalLeakage.Text = "EN 13141-7  EN 13141-4"
        Me.lblExternalLeakage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblElectricPowerInput
        '
        Me.lblElectricPowerInput.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblElectricPowerInput.Location = New System.Drawing.Point(271, 88)
        Me.lblElectricPowerInput.Name = "lblElectricPowerInput"
        Me.lblElectricPowerInput.Size = New System.Drawing.Size(258, 28)
        Me.lblElectricPowerInput.TabIndex = 3
        Me.lblElectricPowerInput.Text = "EN 5801  EN 13141-7  EN 13141-4 "
        Me.lblElectricPowerInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblNoiseLevel
        '
        Me.lblNoiseLevel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblNoiseLevel.Location = New System.Drawing.Point(271, 116)
        Me.lblNoiseLevel.Name = "lblNoiseLevel"
        Me.lblNoiseLevel.Size = New System.Drawing.Size(258, 30)
        Me.lblNoiseLevel.TabIndex = 3
        Me.lblNoiseLevel.Text = "EN 5135  EN 5136"
        Me.lblNoiseLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblAirflowPressure
        '
        Me.lblAirflowPressure.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblAirflowPressure.Location = New System.Drawing.Point(271, 60)
        Me.lblAirflowPressure.Name = "lblAirflowPressure"
        Me.lblAirflowPressure.Size = New System.Drawing.Size(258, 28)
        Me.lblAirflowPressure.TabIndex = 3
        Me.lblAirflowPressure.Text = "EN 5801  EN 13141-7  EN 13141-4 "
        Me.lblAirflowPressure.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblInternalLeakage
        '
        Me.lblInternalLeakage.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblInternalLeakage.Location = New System.Drawing.Point(271, 32)
        Me.lblInternalLeakage.Name = "lblInternalLeakage"
        Me.lblInternalLeakage.Size = New System.Drawing.Size(258, 28)
        Me.lblInternalLeakage.TabIndex = 3
        Me.lblInternalLeakage.Text = "EN 13141-7  EN 13141-4"
        Me.lblInternalLeakage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblExternalLeakageTitle
        '
        Me.lblExternalLeakageTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblExternalLeakageTitle.Location = New System.Drawing.Point(7, 4)
        Me.lblExternalLeakageTitle.Name = "lblExternalLeakageTitle"
        Me.lblExternalLeakageTitle.Size = New System.Drawing.Size(258, 28)
        Me.lblExternalLeakageTitle.TabIndex = 3
        Me.lblExternalLeakageTitle.Text = "External Leakage"
        Me.lblExternalLeakageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Location = New System.Drawing.Point(5, 5)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(301, 168)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'lblTitle
        '
        Me.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Location = New System.Drawing.Point(9, 219)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(281, 13)
        Me.lblTitle.TabIndex = 3
        Me.lblTitle.Text = "All performances has been carried out in accordance with:"
        Me.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pnlCustomerInfo
        '
        Me.pnlCustomerInfo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlCustomerInfo.BackColor = System.Drawing.Color.White
        Me.pnlCustomerInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.pnlCustomerInfo.Controls.Add(Me.PictureBox1)
        Me.pnlCustomerInfo.Controls.Add(Me.Panel1)
        Me.pnlCustomerInfo.Controls.Add(Me.wbbCustomerInfo)
        Me.pnlCustomerInfo.Location = New System.Drawing.Point(12, 7)
        Me.pnlCustomerInfo.Name = "pnlCustomerInfo"
        Me.pnlCustomerInfo.Padding = New System.Windows.Forms.Padding(5)
        Me.pnlCustomerInfo.Size = New System.Drawing.Size(536, 182)
        Me.pnlCustomerInfo.TabIndex = 9
        '
        'Panel1
        '
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Right
        Me.Panel1.Location = New System.Drawing.Point(306, 5)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(19, 168)
        Me.Panel1.TabIndex = 9
        '
        'wbbCustomerInfo
        '
        Me.wbbCustomerInfo.AllowWebBrowserDrop = False
        Me.wbbCustomerInfo.Dock = System.Windows.Forms.DockStyle.Right
        Me.wbbCustomerInfo.IsWebBrowserContextMenuEnabled = False
        Me.wbbCustomerInfo.Location = New System.Drawing.Point(325, 5)
        Me.wbbCustomerInfo.MinimumSize = New System.Drawing.Size(20, 20)
        Me.wbbCustomerInfo.Name = "wbbCustomerInfo"
        Me.wbbCustomerInfo.ScriptErrorsSuppressed = True
        Me.wbbCustomerInfo.ScrollBarsEnabled = False
        Me.wbbCustomerInfo.Size = New System.Drawing.Size(202, 168)
        Me.wbbCustomerInfo.TabIndex = 12
        Me.wbbCustomerInfo.WebBrowserShortcutsEnabled = False
        '
        'CLAboutBoxForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(561, 438)
        Me.Controls.Add(Me.pnlCustomerInfo)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.lblApplicationName)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.lblApplicationRelease)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "CLAboutBoxForm"
        Me.Padding = New System.Windows.Forms.Padding(9)
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "About"
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlCustomerInfo.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents lblApplicationRelease As System.Windows.Forms.Label
    Friend WithEvents lblApplicationName As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblExternalLeakageTitle As System.Windows.Forms.Label
    Friend WithEvents lblInternalLeakageTitle As System.Windows.Forms.Label
    Friend WithEvents lblAirflowPressureTitle As System.Windows.Forms.Label
    Friend WithEvents lblElectricPowerInputTitle As System.Windows.Forms.Label
    Friend WithEvents lblNoiseLevelTitle As System.Windows.Forms.Label
    Friend WithEvents lblExternalLeakage As System.Windows.Forms.Label
    Friend WithEvents lblInternalLeakage As System.Windows.Forms.Label
    Friend WithEvents lblElectricPowerInput As System.Windows.Forms.Label
    Friend WithEvents lblNoiseLevel As System.Windows.Forms.Label
    Friend WithEvents lblAirflowPressure As System.Windows.Forms.Label
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents pnlCustomerInfo As System.Windows.Forms.Panel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents wbbCustomerInfo As System.Windows.Forms.WebBrowser

End Class
