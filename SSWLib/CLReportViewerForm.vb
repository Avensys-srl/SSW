Public Class CLReportViewerForm

    Public Event PdfExported As EventHandler(Of CLPdfExportedEventArgs)

    Private m_PdfExportButton As ToolStripButton

    Public Sub SetReport(reportPath As String,
        reportDataSources As Microsoft.Reporting.WinForms.ReportDataSource(),
        displayMode As Microsoft.Reporting.WinForms.DisplayMode)

        rpvReport.LocalReport.ReportPath = reportPath
        For Each reportDataSource As Microsoft.Reporting.WinForms.ReportDataSource In reportDataSources
            rpvReport.LocalReport.DataSources.Add(reportDataSource)
        Next
        rpvReport.SetDisplayMode(displayMode)
        rpvReport.RefreshReport()
    End Sub

    Private Sub rpvReport_PrintingBegin(sender As Object,
        e As Microsoft.Reporting.WinForms.ReportPrintEventArgs) Handles rpvReport.PrintingBegin
    End Sub

    Private Sub ReportControl_AddExportHandler(reportControl As Control)
        For Each childControl As Control In reportControl.Controls
            If TypeOf childControl Is ToolStrip Then
                Dim toolStrip As ToolStrip = CType(childControl, ToolStrip)
                Dim exportButton As ToolStripDropDownButton = Nothing
                For Each item As ToolStripItem In toolStrip.Items
                    If TypeOf item Is ToolStripDropDownButton AndAlso item.Name = "export" Then
                        exportButton = CType(item, ToolStripDropDownButton)
                        Exit For
                    End If
                Next
                If exportButton IsNot Nothing Then ReportControl_ReplaceExportButton(toolStrip, exportButton)
            End If
            If childControl.Controls.Count > 0 Then ReportControl_AddExportHandler(childControl)
        Next
    End Sub

    Private Sub ReportControl_ReplaceExportButton(toolStrip As ToolStrip,
        exportButton As ToolStripDropDownButton)

        If m_PdfExportButton IsNot Nothing Then Return
        m_PdfExportButton = New ToolStripButton With {
            .Name = "exportPdf",
            .Text = "PDF",
            .ToolTipText = exportButton.ToolTipText,
            .Image = exportButton.Image,
            .DisplayStyle = If(exportButton.Image Is Nothing,
                ToolStripItemDisplayStyle.Text,
                ToolStripItemDisplayStyle.Image)
        }
        AddHandler m_PdfExportButton.Click, AddressOf PdfExportButton_Click
        toolStrip.Items.Insert(toolStrip.Items.IndexOf(exportButton), m_PdfExportButton)
        exportButton.Visible = False
    End Sub

    Private Sub PdfExportButton_Click(sender As Object, eventArgs As EventArgs)
        Using dialog As New SaveFileDialog With {
            .DefaultExt = "pdf",
            .AddExtension = True,
            .Filter = "PDF file|*.pdf",
            .FileName = GetSafePdfFileName(rpvReport.LocalReport.DisplayName)
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                Cursor = Cursors.WaitCursor
                Dim bytes As Byte() = rpvReport.LocalReport.Render("PDF")
                WritePdfAtomically(dialog.FileName, bytes)
                RaiseEvent PdfExported(Me, New CLPdfExportedEventArgs(dialog.FileName))
            Catch ex As Exception
                MessageBox.Show(Me, ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                Cursor = Cursors.Default
            End Try
        End Using
    End Sub

    Private Shared Function GetSafePdfFileName(displayName As String) As String
        Dim fileName As String = If(displayName, String.Empty).Trim()
        For Each invalidCharacter As Char In IO.Path.GetInvalidFileNameChars()
            fileName = fileName.Replace(invalidCharacter, "_"c)
        Next
        If String.IsNullOrWhiteSpace(fileName) Then fileName = "Report"
        Return fileName
    End Function

    Private Shared Sub WritePdfAtomically(filePath As String, bytes As Byte())
        Dim fullPath As String = IO.Path.GetFullPath(filePath)
        Dim directoryPath As String = IO.Path.GetDirectoryName(fullPath)
        If String.IsNullOrWhiteSpace(directoryPath) OrElse Not IO.Directory.Exists(directoryPath) Then
            Throw New IO.DirectoryNotFoundException("The PDF destination directory does not exist.")
        End If
        Dim temporaryPath As String = fullPath & ".tmp-" & Guid.NewGuid().ToString("N")
        Dim backupPath As String = temporaryPath & ".bak"
        Try
            IO.File.WriteAllBytes(temporaryPath, bytes)
            If IO.File.Exists(fullPath) Then
                IO.File.Replace(temporaryPath, fullPath, backupPath, True)
                If IO.File.Exists(backupPath) Then IO.File.Delete(backupPath)
            Else
                IO.File.Move(temporaryPath, fullPath)
            End If
        Finally
            If IO.File.Exists(temporaryPath) Then IO.File.Delete(temporaryPath)
            If IO.File.Exists(backupPath) Then IO.File.Delete(backupPath)
        End Try
    End Sub

    Private Sub CLReportViewerForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        ReportControl_AddExportHandler(rpvReport)
    End Sub

End Class

Public NotInheritable Class CLPdfExportedEventArgs
    Inherits EventArgs

    Public Sub New(filePath As String)
        Me.FilePath = IO.Path.GetFullPath(filePath)
    End Sub

    Public ReadOnly Property FilePath As String

End Class
