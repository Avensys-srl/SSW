Public Class CLReportViewerForm
    Private Const A4PdfDeviceInfo As String =
        "<DeviceInfo><OutputFormat>PDF</OutputFormat><PageWidth>21cm</PageWidth>" &
        "<PageHeight>29.7cm</PageHeight><MarginTop>0.5cm</MarginTop>" &
        "<MarginBottom>0.5cm</MarginBottom><MarginLeft>0.5cm</MarginLeft>" &
        "<MarginRight>0.5cm</MarginRight></DeviceInfo>"

    Private m_Preview As Microsoft.Web.WebView2.WinForms.WebView2
    Private m_PreviewPdfPath As String
    Private m_PdfBytes As Byte()

    Public Sub SetDisplayName(displayName As String)
        rpvReport.LocalReport.DisplayName = displayName
    End Sub


    Public Event PdfExported As EventHandler(Of CLPdfExportedEventArgs)
    Public Event AddToProjectRequested As EventHandler(Of CLPdfExportedEventArgs)
    Public Event FollowUpPrepared As EventHandler(Of CLFollowUpPreparedEventArgs)
    Public Event FollowUpLocalPathRequested As EventHandler(Of CLFollowUpLocalPathRequestedEventArgs)

    Private m_PdfExportButton As ToolStripButton
    Private m_EmailButton As ToolStripButton
    Private m_AddToProjectButton As ToolStripButton
    Private m_EmailModelName As String = String.Empty
    Private m_EmailCustomerReference As String = String.Empty
    Private m_EmailAirFlow As String = String.Empty
    Private m_EmailPressure As String = String.Empty
    Private m_EmailRegistrationReference As String = String.Empty
    Private m_EmailTargetUuid As Guid
    Private m_EmailLocalPath As String = String.Empty

    Public Sub SetEmailContext(modelName As String,
        customerReference As String,
        airFlow As String,
        pressure As String,
        registrationReference As String,
        Optional targetUuid As Guid = Nothing,
        Optional localPath As String = Nothing)

        m_EmailModelName = If(modelName, String.Empty).Trim()
        m_EmailCustomerReference = If(customerReference, String.Empty).Trim()
        m_EmailAirFlow = If(airFlow, String.Empty).Trim()
        m_EmailPressure = If(pressure, String.Empty).Trim()
        m_EmailRegistrationReference = If(registrationReference, String.Empty).Trim()
        m_EmailTargetUuid = targetUuid
        m_EmailLocalPath = If(localPath, String.Empty).Trim()
        UpdateEmailButtonState()
    End Sub

    Public Sub SetReport(reportPath As String,
        reportDataSources As Microsoft.Reporting.WinForms.ReportDataSource(),
        displayMode As Microsoft.Reporting.WinForms.DisplayMode)

        rpvReport.LocalReport.ReportPath = reportPath
        For Each reportDataSource As Microsoft.Reporting.WinForms.ReportDataSource In reportDataSources
            rpvReport.LocalReport.DataSources.Add(reportDataSource)
        Next
        ' Render the A4 PDF when the window opens, after the display name is set.
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
        m_AddToProjectButton = New ToolStripButton With {
            .Name = "addToSelectionProject",
            .Text = EmailText("ReportViewer_AddToProject", "Add to project"),
            .ToolTipText = EmailText("ReportViewer_AddToProjectTooltip", "Add or update this report in a multi-selection project"),
            .Image = CreateProjectIcon(),
            .DisplayStyle = ToolStripItemDisplayStyle.Image,
            .Visible = False
        }
        AddHandler m_AddToProjectButton.Click, AddressOf AddToProjectButton_Click
        toolStrip.Items.Insert(toolStrip.Items.IndexOf(m_EmailButton) + 1, m_AddToProjectButton)
        UpdateEmailButtonState()
        exportButton.Visible = False
    End Sub

    Private Sub AddToProjectButton_Click(sender As Object, eventArgs As EventArgs)
        Try
            Cursor = Cursors.WaitCursor
            Dim pdfPath As String = ExportPdf(CreateTemporaryEmailPdfPath(rpvReport.LocalReport.DisplayName))
            RaiseEvent AddToProjectRequested(Me, New CLPdfExportedEventArgs(pdfPath))
        Catch ex As Exception
            Diagnostics.Trace.WriteLine(ex.ToString())
            MessageBox.Show(Me, ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor = Cursors.Default
        End Try
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

        Dim scheduleChoice As CLFollowUpScheduleChoice = CLFollowUpScheduleDialog.Prompt(Me)
        If Not scheduleChoice.Proceed Then Return
        If scheduleChoice.Schedule AndAlso String.IsNullOrWhiteSpace(m_EmailLocalPath) Then
            Dim localPathRequest As New CLFollowUpLocalPathRequestedEventArgs()
            RaiseEvent FollowUpLocalPathRequested(Me, localPathRequest)
            m_EmailLocalPath = If(localPathRequest.LocalPath, String.Empty).Trim()
            If String.IsNullOrWhiteSpace(m_EmailLocalPath) Then Return
        End If

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
                    "Selection - {0}"),
                m_EmailModelName,
                m_EmailCustomerReference,
                m_EmailAirFlow,
                m_EmailPressure,
                m_EmailRegistrationReference)
            Dim body As String = CLSelectionEmailComposer.BuildBody(
                EmailText(CLMessageResources.ReportViewer_EmailBody,
                    "Good morning," & Environment.NewLine & Environment.NewLine &
                    "Thank you for your enquiry." & Environment.NewLine & Environment.NewLine &
                    "Please find attached the PDF relating to the selection of ventilation unit {0} at an airflow of {1} m³/h and pressure of {2} Pa.{3}" &
                    Environment.NewLine & Environment.NewLine &
                    "Please contact us if you require any clarification or further technical information." &
                    Environment.NewLine & Environment.NewLine & "Kind regards"),
                EmailText(CLMessageResources.ReportViewer_EmailCustomerReference,
                    "Your reference: {0}"),
                m_EmailModelName,
                m_EmailAirFlow,
                m_EmailPressure,
                m_EmailCustomerReference)
            CLOutlookEmailService.DisplayMessage(subject, body, pdfPath)
            If scheduleChoice.Schedule Then
                Dim preparedAtUtc As DateTime = DateTime.UtcNow
                RaiseEvent FollowUpPrepared(Me, New CLFollowUpPreparedEventArgs With {
                    .TargetType = "Selection",
                    .TargetUuid = m_EmailTargetUuid,
                    .DisplayReference = If(String.IsNullOrWhiteSpace(m_EmailCustomerReference),
                        m_EmailRegistrationReference,
                        m_EmailCustomerReference),
                    .LocalPath = m_EmailLocalPath,
                    .PreparedAtUtc = preparedAtUtc,
                    .DueAtUtc = DateTime.Now.AddDays(scheduleChoice.Days).ToUniversalTime()
                })
            End If
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
        Dim bytes As Byte() = GetPdfBytes()
        WritePdfAtomically(fullPath, bytes)
        If Not IO.File.Exists(fullPath) OrElse New IO.FileInfo(fullPath).Length = 0 Then
            Throw New IO.IOException("The rendered PDF was not created.")
        End If
        Return fullPath
    End Function

    Public Sub PreparePdf()
        GetPdfBytes()
    End Sub

    Private Function GetPdfBytes() As Byte()
        If m_PdfBytes Is Nothing Then
            m_PdfBytes = RenderA4Pdf(rpvReport.LocalReport)
        End If
        Return m_PdfBytes
    End Function

    Public Shared Function RenderA4Pdf(report As Microsoft.Reporting.WinForms.LocalReport) As Byte()
        If report Is Nothing Then Throw New ArgumentNullException(NameOf(report))
        Return report.Render("PDF", A4PdfDeviceInfo)
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

    Private Shared Function CreateProjectIcon() As Image
        Dim bitmap As New Bitmap(16, 16)
        Using graphics As Graphics = Graphics.FromImage(bitmap)
            graphics.Clear(Color.Transparent)
            Using pen As New Pen(SystemColors.ControlText, 1.4F)
                graphics.DrawRectangle(pen, 1.5F, 3.5F, 9.0F, 10.0F)
                graphics.DrawLine(pen, 3.0F, 6.0F, 9.0F, 6.0F)
                graphics.DrawLine(pen, 3.0F, 8.5F, 7.0F, 8.5F)
                graphics.DrawLine(pen, 12.5F, 7.0F, 12.5F, 14.0F)
                graphics.DrawLine(pen, 9.0F, 10.5F, 16.0F, 10.5F)
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

    Private Shared Function EmailText(resourceName As String, fallback As String) As String
        Dim value As String = CLEnvironment.Current.Localization.GetString(resourceName)
        Return If(String.IsNullOrWhiteSpace(value) OrElse value = "?", fallback, value)
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

    Private Async Sub CLReportViewerForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Try
            UseWaitCursor = True
            m_PreviewPdfPath = IO.Path.Combine(IO.Path.GetTempPath(),
                "ssw-report-preview-" & Guid.NewGuid().ToString("N") & ".pdf")
            WritePdfAtomically(m_PreviewPdfPath, GetPdfBytes())

            Dim toolbar As New ToolStrip With {
                .GripStyle = ToolStripGripStyle.Hidden,
                .Dock = DockStyle.Fill,
                .Margin = Padding.Empty
            }
            Dim pdfButton As New ToolStripButton("PDF")
            AddHandler pdfButton.Click, AddressOf PdfExportButton_Click
            toolbar.Items.Add(pdfButton)
            Dim printButton As New ToolStripButton(EmailText("ReportViewer_Print", "Print")) With {
                .Enabled = False
            }
            AddHandler printButton.Click, Sub()
                                              If m_Preview.CoreWebView2 IsNot Nothing Then
                                                  m_Preview.CoreWebView2.ShowPrintUI()
                                              End If
                                          End Sub
            toolbar.Items.Add(printButton)
            m_EmailButton = New ToolStripButton(EmailText(CLMessageResources.ReportViewer_Email, "Email"))
            AddHandler m_EmailButton.Click, AddressOf EmailButton_Click
            toolbar.Items.Add(m_EmailButton)
            m_AddToProjectButton = New ToolStripButton(EmailText("ReportViewer_AddToProject", "Add to project"))
            AddHandler m_AddToProjectButton.Click, AddressOf AddToProjectButton_Click
            m_AddToProjectButton.Visible = False
            toolbar.Items.Add(m_AddToProjectButton)
            UpdateEmailButtonState()

            m_Preview = New Microsoft.Web.WebView2.WinForms.WebView2 With {
                .Dock = DockStyle.Fill,
                .Margin = Padding.Empty
            }
            Dim previewLayout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .Margin = Padding.Empty,
                .Padding = Padding.Empty
            }
            previewLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            previewLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            previewLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            previewLayout.Controls.Add(toolbar, 0, 0)
            previewLayout.Controls.Add(m_Preview, 0, 1)
            Controls.Add(previewLayout)
            previewLayout.BringToFront()
            rpvReport.Visible = False
            Dim userDataDirectory = IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Avensys", "SSW", "WebView2Preview")
            Dim webEnvironment = Await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                Nothing, userDataDirectory)
            If IsDisposed Then Return
            Await m_Preview.EnsureCoreWebView2Async(webEnvironment)
            If IsDisposed Then Return
            m_Preview.Source = New Uri(m_PreviewPdfPath)
            printButton.Enabled = True
        Catch ex As Exception
            Diagnostics.Trace.WriteLine(ex.ToString())
            If m_Preview IsNot Nothing Then m_Preview.Parent.Visible = False
            rpvReport.Visible = True
            rpvReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.Normal)
            rpvReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.PageWidth
            rpvReport.RefreshReport()
            ReportControl_AddExportHandler(rpvReport)
            MessageBox.Show(Me, ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            UseWaitCursor = False
        End Try
    End Sub

    Private Sub CLReportViewerForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        If m_Preview IsNot Nothing Then m_Preview.Dispose()
        If Not String.IsNullOrWhiteSpace(m_PreviewPdfPath) Then
            Try
                IO.File.Delete(m_PreviewPdfPath)
            Catch ex As IO.IOException
                Diagnostics.Trace.WriteLine(ex.ToString())
            End Try
        End If
    End Sub

End Class

Public NotInheritable Class CLFollowUpLocalPathRequestedEventArgs
    Inherits EventArgs

    Public Property LocalPath As String

End Class

Public NotInheritable Class CLPdfExportedEventArgs
    Inherits EventArgs

    Public Sub New(filePath As String)
        Me.FilePath = IO.Path.GetFullPath(filePath)
    End Sub

    Public ReadOnly Property FilePath As String

End Class
