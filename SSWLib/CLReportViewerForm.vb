Public Class CLReportViewerForm

    Public Event PdfExported As EventHandler(Of CLPdfExportedEventArgs)

    Private m_PdfExportButton As ToolStripButton
    Private m_EmailButton As ToolStripButton
    Private m_EmailModelName As String = String.Empty
    Private m_EmailSelectionReference As String = String.Empty

    Public Sub SetEmailContext(modelName As String, selectionReference As String)
        m_EmailModelName = If(modelName, String.Empty).Trim()
        m_EmailSelectionReference = If(selectionReference, String.Empty).Trim()
        UpdateEmailButtonState()
    End Sub

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
        m_EmailButton = New ToolStripButton With {
            .Name = "emailPdf",
            .Text = EmailText(CLMessageResources.ReportViewer_Email, "Email"),
            .ToolTipText = EmailText(CLMessageResources.ReportViewer_EmailTooltip, "Send the PDF by email"),
            .Image = CreateEmailIcon(),
            .DisplayStyle = ToolStripItemDisplayStyle.Image
        }
        AddHandler m_EmailButton.Click, AddressOf EmailButton_Click
        toolStrip.Items.Insert(toolStrip.Items.IndexOf(m_PdfExportButton) + 1, m_EmailButton)
        UpdateEmailButtonState()
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
                Dim exportedPath As String = ExportPdf(dialog.FileName)
                RaiseEvent PdfExported(Me, New CLPdfExportedEventArgs(exportedPath))
            Catch ex As Exception
                MessageBox.Show(Me, ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                Cursor = Cursors.Default
            End Try
        End Using
    End Sub

    Private Sub EmailButton_Click(sender As Object, eventArgs As EventArgs)
        If String.IsNullOrWhiteSpace(m_EmailModelName) Then Return

        Dim pdfPath As String
        Try
            Cursor = Cursors.WaitCursor
            pdfPath = ExportPdf(CreateTemporaryEmailPdfPath(rpvReport.LocalReport.DisplayName))
        Catch ex As Exception
            Diagnostics.Trace.WriteLine(ex.ToString())
            ShowEmailError(EmailText(CLMessageResources.ReportViewer_EmailPdfError,
                "Unable to generate the selection PDF."))
            Return
        Finally
            Cursor = Cursors.Default
        End Try

        Try
            Dim subject As String = CLSelectionEmailComposer.BuildSubject(
                EmailText(CLMessageResources.ReportViewer_EmailSubject,
                    "Ventilation unit selection - {0}"),
                m_EmailModelName,
                m_EmailSelectionReference)
            Dim body As String = CLSelectionEmailComposer.BuildBody(
                EmailText(CLMessageResources.ReportViewer_EmailBody,
                    "Good morning," & Environment.NewLine & Environment.NewLine &
                    "please find attached the PDF for the selection of ventilation unit {0}, prepared using SSW software." &
                    Environment.NewLine & Environment.NewLine &
                    "Please contact us if you require any clarification or further technical information." &
                    Environment.NewLine & Environment.NewLine & "Kind regards"),
                m_EmailModelName)
            CLOutlookEmailService.DisplayMessage(subject, body, pdfPath)
        Catch ex As CLOutlookEmailException
            Diagnostics.Trace.WriteLine(ex.ToString())
            Select Case ex.Failure
                Case CLOutlookEmailFailure.OutlookUnavailable
                    ShowEmailError(EmailText(CLMessageResources.ReportViewer_EmailOutlookUnavailable,
                        "Microsoft Outlook is not available or cannot be started."))
                Case CLOutlookEmailFailure.AttachmentFailed
                    ShowEmailError(EmailText(CLMessageResources.ReportViewer_EmailAttachmentError,
                        "Unable to attach the PDF to the new email."))
                Case Else
                    ShowEmailError(EmailText(CLMessageResources.ReportViewer_EmailError,
                        "Unable to create the new email."))
            End Select
        Catch ex As Exception
            Diagnostics.Trace.WriteLine(ex.ToString())
            ShowEmailError(EmailText(CLMessageResources.ReportViewer_EmailError,
                "Unable to create the new email."))
        End Try
    End Sub

    Private Function ExportPdf(filePath As String) As String
        Dim fullPath As String = IO.Path.GetFullPath(filePath)
        Dim bytes As Byte() = rpvReport.LocalReport.Render("PDF")
        WritePdfAtomically(fullPath, bytes)
        If Not IO.File.Exists(fullPath) OrElse New IO.FileInfo(fullPath).Length = 0 Then
            Throw New IO.IOException("The rendered PDF was not created.")
        End If
        Return fullPath
    End Function

    Private Shared Function CreateTemporaryEmailPdfPath(displayName As String) As String
        Dim rootPath As String = IO.Path.Combine(IO.Path.GetTempPath(), "Avensys", "SSW", "Email")
        CleanupTemporaryEmailPdfs(rootPath)
        Dim messageDirectory As String = IO.Path.Combine(rootPath,
            DateTime.Now.ToString("yyyyMMdd-HHmmss") & "-" & Guid.NewGuid().ToString("N"))
        IO.Directory.CreateDirectory(messageDirectory)
        Return IO.Path.Combine(messageDirectory, GetSafePdfFileName(displayName) & ".pdf")
    End Function

    Private Shared Sub CleanupTemporaryEmailPdfs(rootPath As String)
        If Not IO.Directory.Exists(rootPath) Then Return
        Try
            For Each directoryPath As String In IO.Directory.GetDirectories(rootPath)
                If IO.Directory.GetCreationTimeUtc(directoryPath) < DateTime.UtcNow.AddDays(-7) Then
                    IO.Directory.Delete(directoryPath, True)
                End If
            Next
        Catch ex As Exception
            Diagnostics.Trace.WriteLine(ex.ToString())
        End Try
    End Sub

    Private Shared Function CreateEmailIcon() As Image
        Dim bitmap As New Bitmap(16, 16)
        Using graphics As Graphics = Graphics.FromImage(bitmap)
            graphics.Clear(Color.Transparent)
            Using pen As New Pen(SystemColors.ControlText, 1.4F)
                graphics.DrawRectangle(pen, 1.5F, 3.5F, 13.0F, 9.0F)
                graphics.DrawLine(pen, 2.0F, 4.0F, 8.0F, 9.0F)
                graphics.DrawLine(pen, 14.0F, 4.0F, 8.0F, 9.0F)
            End Using
        End Using
        Return bitmap
    End Function

    Private Sub UpdateEmailButtonState()
        If m_EmailButton IsNot Nothing Then
            m_EmailButton.Enabled = Not String.IsNullOrWhiteSpace(m_EmailModelName)
        End If
    End Sub

    Private Sub ShowEmailError(message As String)
        MessageBox.Show(Me, message, EmailText(CLMessageResources.ReportViewer_Email, "Email"),
            MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Shared Function EmailText(resource As CLMessageResources, fallback As String) As String
        Dim value As String = CLEnvironment.Current.Localization.GetString(resource.ToString())
        Return If(String.IsNullOrWhiteSpace(value), fallback, value)
    End Function

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
