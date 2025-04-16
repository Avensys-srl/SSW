Public Class CLReportViewerForm

	Public Sub SetReport(reportPath As String, _
	   reportDataSources As Microsoft.Reporting.WinForms.ReportDataSource(),
	   displayMode As Microsoft.Reporting.WinForms.DisplayMode)

		'Dim reportParameterInPrinting As Microsoft.Reporting.WinForms.ReportParameterInfo

		rpvReport.LocalReport.ReportPath = reportPath
		For Each reportDataSource As Microsoft.Reporting.WinForms.ReportDataSource In reportDataSources
			rpvReport.LocalReport.DataSources.Add(reportDataSource)
		Next

		rpvReport.SetDisplayMode(displayMode)

		rpvReport.RefreshReport()

		'reportParameterInPrinting = rpvReport.LocalReport.GetParameters("InPrinting")
		'If Not reportParameterInPrinting Is Nothing Then
		'	rpvReport.LocalReport.SetParameters(New Microsoft.Reporting.WinForms.ReportParameter("InPrinting", False))
		'End If
	End Sub

	Private Sub rpvReport_PrintingBegin(sender As System.Object, e As Microsoft.Reporting.WinForms.ReportPrintEventArgs) Handles rpvReport.PrintingBegin
		'Dim reportParameterInPrinting As Microsoft.Reporting.WinForms.ReportParameterInfo
		'reportParameterInPrinting = rpvReport.LocalReport.GetParameters("InPrinting")
		'If Not reportParameterInPrinting Is Nothing Then
		'	rpvReport.LocalReport.SetParameters(New Microsoft.Reporting.WinForms.ReportParameter("InPrinting", True))
		'End If
	End Sub

    Private Sub ReportControl_AddExportHandler(reportControl As Control)

        For Each childControl As Control In reportControl.Controls

            If TypeOf childControl Is ToolStrip Then
                For Each item As ToolStripItem In CType(childControl, ToolStrip).Items
                    If TypeOf item Is ToolStripDropDownButton AndAlso item.Name = "export" Then
                        AddHandler CType(item, ToolStripDropDownButton).DropDownOpened,
                            New EventHandler(AddressOf ToolStripDropDownButton_DropDownOpened)
                    End If
                Next
            End If

            If childControl.Controls.Count > 0 Then
                ReportControl_AddExportHandler(childControl)
            End If

        Next


    End Sub

    Private Sub ToolStripDropDownButton_DropDownOpened(sender As Object, eventArgs As EventArgs)

        If TypeOf sender Is ToolStripDropDownButton Then
            Dim dropDownButton As ToolStripDropDownButton = sender

            For Each dropDownItem As ToolStripItem In dropDownButton.DropDownItems
                If TypeOf dropDownItem Is ToolStripMenuItem Then
                    dropDownItem.Visible = (String.Compare(dropDownItem.Text, "PDF", StringComparison.CurrentCultureIgnoreCase) = 0)
                End If
            Next
        End If
    End Sub

    Private Sub CLReportViewerForm_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        ReportControl_AddExportHandler(rpvReport)
    End Sub
End Class