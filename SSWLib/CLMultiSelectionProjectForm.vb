Imports System.Globalization
Imports System.IO

Public NotInheritable Class CLMultiSelectionProjectForm
    Inherits Form

    Private ReadOnly m_ReferenceText As New TextBox()
    Private ReadOnly m_LanguageText As New TextBox()
    Private ReadOnly m_Grid As New DataGridView()
    Private m_Document As CLMultiSelectionProjectDocument
    Private m_ProjectPath As String
    Private m_OpenItem As CLMultiSelectionProjectItem

    Public Sub New(document As CLMultiSelectionProjectDocument, projectPath As String,
        defaultReference As String, languageCode As String)

        m_Document = If(document, CLMultiSelectionProjectSerializer.CreateNew(defaultReference, languageCode))
        m_ProjectPath = projectPath
        BuildUi()
        LoadDocument()
    End Sub

    Public ReadOnly Property Document As CLMultiSelectionProjectDocument
        Get
            Return m_Document
        End Get
    End Property

    Public ReadOnly Property ProjectPath As String
        Get
            Return m_ProjectPath
        End Get
    End Property

    Public ReadOnly Property SelectionToOpen As CLMultiSelectionProjectItem
        Get
            Return m_OpenItem
        End Get
    End Property

    Private Sub BuildUi()
        Text = T("MultiProject_Title", "Selection project")
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(850, 430)
        Size = New Size(1050, 590)
        Padding = New Padding(10)

        Dim root As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 3}
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        Dim header As New TableLayoutPanel With {.Dock = DockStyle.Top, .AutoSize = True, .ColumnCount = 4}
        header.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        header.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        header.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        header.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90.0F))
        header.Controls.Add(New Label With {.Text = T("MultiProject_Reference", "Project reference"), .AutoSize = True, .Anchor = AnchorStyles.Left, .Margin = New Padding(0, 7, 8, 0)}, 0, 0)
        m_ReferenceText.Dock = DockStyle.Fill
        m_ReferenceText.Margin = New Padding(0, 3, 18, 6)
        header.Controls.Add(m_ReferenceText, 1, 0)
        header.Controls.Add(New Label With {.Text = T("MultiProject_Language", "Language"), .AutoSize = True, .Anchor = AnchorStyles.Left, .Margin = New Padding(0, 7, 8, 0)}, 2, 0)
        m_LanguageText.Dock = DockStyle.Fill
        m_LanguageText.ReadOnly = True
        m_LanguageText.Margin = New Padding(0, 3, 0, 6)
        header.Controls.Add(m_LanguageText, 3, 0)

        Dim toolbar As New FlowLayoutPanel With {.Dock = DockStyle.Top, .AutoSize = True, .WrapContents = False, .Padding = New Padding(0, 2, 0, 8)}
        AddButton(toolbar, T("MultiProject_New", "New"), AddressOf NewClick)
        AddButton(toolbar, T("MultiProject_Open", "Open..."), AddressOf OpenClick)
        AddButton(toolbar, T("MultiProject_Save", "Save"), AddressOf SaveClick)
        AddButton(toolbar, T("MultiProject_SaveAs", "Save as..."), AddressOf SaveAsClick)
        AddButton(toolbar, T("MultiProject_Remove", "Remove"), AddressOf RemoveClick)
        AddButton(toolbar, T("MultiProject_OpenSelection", "Open selection"), AddressOf OpenSelectionClick)
        AddButton(toolbar, T("MultiProject_Email", "Email project"), AddressOf EmailClick)
        AddButton(toolbar, T("MultiProject_Close", "Close"), Sub(sender, e) Close())

        m_Grid.Dock = DockStyle.Fill
        m_Grid.ReadOnly = True
        m_Grid.AllowUserToAddRows = False
        m_Grid.AllowUserToDeleteRows = False
        m_Grid.AllowUserToResizeRows = False
        m_Grid.AutoGenerateColumns = False
        m_Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        m_Grid.BackgroundColor = SystemColors.Window
        m_Grid.BorderStyle = BorderStyle.Fixed3D
        m_Grid.MultiSelect = False
        m_Grid.RowHeadersVisible = False
        m_Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        AddColumn("Reference", T("MultiProject_ColumnReference", "Reference"), 24)
        AddColumn("Unit", T("MultiProject_ColumnUnit", "Selected unit"), 23)
        AddColumn("Airflow", T("MultiProject_ColumnAirflow", "Airflow [m³/h]"), 13)
        AddColumn("Pressure", T("MultiProject_ColumnPressure", "Pressure [Pa]"), 12)
        AddColumn("Pdf", T("MultiProject_ColumnPdf", "PDF file"), 20)
        AddColumn("Status", T("MultiProject_ColumnStatus", "Status"), 8)

        root.Controls.Add(header, 0, 0)
        root.Controls.Add(toolbar, 0, 1)
        root.Controls.Add(m_Grid, 0, 2)
        Controls.Add(root)
    End Sub

    Private Shared Sub AddButton(panel As FlowLayoutPanel, caption As String, handler As EventHandler)
        Dim button As New Button With {.Text = caption, .AutoSize = True, .MinimumSize = New Size(82, 27), .Margin = New Padding(0, 0, 6, 0)}
        AddHandler button.Click, handler
        panel.Controls.Add(button)
    End Sub

    Private Sub AddColumn(name As String, caption As String, weight As Single)
        m_Grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = name, .HeaderText = caption, .FillWeight = weight, .SortMode = DataGridViewColumnSortMode.NotSortable})
    End Sub

    Private Sub LoadDocument()
        m_ReferenceText.Text = m_Document.Reference
        m_LanguageText.Text = m_Document.LanguageCode.ToUpperInvariant()
        RefreshRows()
        UpdateTitle()
    End Sub

    Private Sub RefreshRows()
        m_Grid.Rows.Clear()
        For Each item As CLMultiSelectionProjectItem In m_Document.Items
            Dim rowIndex As Integer = m_Grid.Rows.Add(
                If(String.IsNullOrWhiteSpace(item.CustomerReference), "-", item.CustomerReference),
                item.UnitName,
                FormatNumber(item.AirflowM3h),
                FormatNumber(item.PressurePa),
                item.PdfFileName,
                If(CLMultiSelectionProjectSerializer.IsCurrent(item, m_Document.LanguageCode), T("MultiProject_Ready", "Ready"), T("MultiProject_Incomplete", "Incomplete")))
            m_Grid.Rows(rowIndex).Tag = item
        Next
    End Sub

    Private Sub UpdateTitle()
        Text = T("MultiProject_Title", "Selection project") & If(String.IsNullOrWhiteSpace(m_ProjectPath), String.Empty, " - " & Path.GetFileName(m_ProjectPath))
    End Sub

    Private Sub NewClick(sender As Object, e As EventArgs)
        m_Document = CLMultiSelectionProjectSerializer.CreateNew(T("MultiProject_DefaultReference", "Project 01"), CLEnvironment.Current.PrimaryLanguageCode)
        m_ProjectPath = Nothing
        LoadDocument()
    End Sub

    Private Sub OpenClick(sender As Object, e As EventArgs)
        Using dialog As New OpenFileDialog With {.Filter = FileFilter(), .DefaultExt = CLMultiSelectionProjectSerializer.FileExtension.TrimStart("."c)}
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                m_Document = CLMultiSelectionProjectSerializer.Load(dialog.FileName)
                m_ProjectPath = Path.GetFullPath(dialog.FileName)
                LoadDocument()
            Catch ex As Exception
                ShowError(ex.Message)
            End Try
        End Using
    End Sub

    Private Sub SaveClick(sender As Object, e As EventArgs)
        SaveProject(False)
    End Sub

    Private Sub SaveAsClick(sender As Object, e As EventArgs)
        SaveProject(True)
    End Sub

    Public Function SaveProject(saveAs As Boolean) As Boolean
        m_Document.Reference = m_ReferenceText.Text.Trim()
        If String.IsNullOrWhiteSpace(m_Document.Reference) Then
            ShowError(T("MultiProject_ReferenceRequired", "The project reference is required."))
            m_ReferenceText.Focus()
            Return False
        End If
        Dim targetPath As String = m_ProjectPath
        If saveAs OrElse String.IsNullOrWhiteSpace(targetPath) Then
            Using dialog As New SaveFileDialog With {.Filter = FileFilter(), .DefaultExt = CLMultiSelectionProjectSerializer.FileExtension.TrimStart("."c), .AddExtension = True, .FileName = SafeName(m_Document.Reference)}
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return False
                targetPath = dialog.FileName
            End Using
        End If
        Try
            CLMultiSelectionProjectSerializer.Save(targetPath, m_Document)
            m_ProjectPath = Path.GetFullPath(targetPath)
            UpdateTitle()
            Return True
        Catch ex As Exception
            ShowError(ex.Message)
            Return False
        End Try
    End Function

    Private Sub RemoveClick(sender As Object, e As EventArgs)
        Dim item As CLMultiSelectionProjectItem = SelectedItem()
        If item Is Nothing Then Return
        If MessageBox.Show(Me, T("MultiProject_RemoveConfirm", "Remove the selected report from the project?"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        m_Document.Items.Remove(item)
        m_Document.ModifiedAtUtc = DateTime.UtcNow
        RefreshRows()
    End Sub

    Private Sub OpenSelectionClick(sender As Object, e As EventArgs)
        m_OpenItem = SelectedItem()
        If m_OpenItem Is Nothing Then Return
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub EmailClick(sender As Object, e As EventArgs)
        m_Document.Reference = m_ReferenceText.Text.Trim()
        If String.IsNullOrWhiteSpace(m_Document.Reference) Then
            ShowError(T("MultiProject_ReferenceRequired", "The project reference is required."))
            m_ReferenceText.Focus()
            Return
        End If
        If m_Document.Items.Count = 0 Then
            ShowError(T("MultiProject_Empty", "The project does not contain any reports."))
            Return
        End If
        Try
            Cursor = Cursors.WaitCursor
            Dim directoryPath As String = Path.Combine(Path.GetTempPath(), "Avensys", "SSW", "Projects", Guid.NewGuid().ToString("N"))
            Dim attachments As New List(Of String)()
            For Each item As CLMultiSelectionProjectItem In m_Document.Items
                attachments.Add(CLMultiSelectionProjectSerializer.ExtractPdf(item, directoryPath))
            Next
            CLOutlookEmailService.DisplayHtmlMessage(
                CLMultiSelectionEmailComposer.BuildSubject(m_Document),
                CLMultiSelectionEmailComposer.BuildHtml(m_Document),
                attachments)
        Catch ex As Exception
            ShowError(ex.Message)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Function SelectedItem() As CLMultiSelectionProjectItem
        If m_Grid.SelectedRows.Count = 0 Then Return Nothing
        Return TryCast(m_Grid.SelectedRows(0).Tag, CLMultiSelectionProjectItem)
    End Function

    Private Shared Function FormatNumber(value As Double?) As String
        Return If(value.HasValue, value.Value.ToString("0.##", CultureInfo.CurrentCulture), "-")
    End Function

    Private Shared Function SafeName(value As String) As String
        Dim result As String = value.Trim()
        For Each invalid As Char In Path.GetInvalidFileNameChars()
            result = result.Replace(invalid, "_"c)
        Next
        Return If(String.IsNullOrWhiteSpace(result), "Project_01", result)
    End Function

    Private Shared Function FileFilter() As String
        Return T("MultiProject_Filter", "SSW selection project") & " (*" & CLMultiSelectionProjectSerializer.FileExtension & ")|*" & CLMultiSelectionProjectSerializer.FileExtension
    End Function

    Private Shared Function T(resourceName As String, fallback As String) As String
        Try
            Dim value As String = CLEnvironment.Current.Localization.GetString(resourceName)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> "?" AndAlso Not value.StartsWith("@@", StringComparison.Ordinal) Then Return value
        Catch
        End Try
        Return fallback
    End Function

    Private Sub ShowError(message As String)
        MessageBox.Show(Me, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub
End Class
