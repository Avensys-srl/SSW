Imports System.Drawing
Imports System.Windows.Forms

Public NotInheritable Class CLHelpForm
    Inherits Form

    Private NotInheritable Class HelpTopic
        Public Property Title As String
        Public Property Body As String

        Public Overrides Function ToString() As String
            Return Title
        End Function
    End Class

    Private NotInheritable Class HelpTopicDefinition
        Public Property TitleKey As String
        Public Property TitleFallback As String
        Public Property BodyKey As String
        Public Property BodyFallback As String
    End Class

    ' Add a topic here and the guide, search index and localization fallback are
    ' updated together. The matching keys belong in every Resources.*.resx file.
    Private Shared ReadOnly TopicDefinitions As HelpTopicDefinition() = {
        Topic("Help_Topic_GettingStarted", "Getting started", "Begin by choosing the product family and ventilation unit. Enter the winter airflow and available external static pressure, then set winter and, when required, summer temperature and humidity conditions." & vbCrLf & vbCrLf & "Review the operating point on the graphs before adding coils, electric heaters, accessories or control functions. Complete the customer reference before saving or generating a report; it is used in file names, reports and selection projects."),
        Topic("Help_Topic_Workflow", "Recommended selection workflow", "1. Choose the family and unit." & vbCrLf & "2. Enter airflow, pressure and design conditions." & vbCrLf & "3. Adjust the regulation level and verify the operating point." & vbCrLf & "4. Add compatible water coils, electric heaters, accessories and functions." & vbCrLf & "5. Enter the customer reference and save the .sswsel file." & vbCrLf & "6. Generate and review the localized PDF." & vbCrLf & "7. If the job contains several units, add each report to a selection project."),
        Topic("Help_Topic_Conditions", "Winter and summer conditions", "Winter inputs are shown on the left and summer inputs on the right. Summer airflow currently remains synchronized with winter airflow. Use Enable summer to run both thermodynamic calculations; disable it when only the winter case is required." & vbCrLf & vbCrLf & "The Summer/Winter preset restores the standard design conditions. EN 308 and EN 13141-7 presets affect the winter case only and do not overwrite the summer inputs."),
        Topic("Help_Topic_Performance", "Reading performance", "The regulation control moves the selected point along the fan curve and recalculates pressure, absorbed power and heat-recovery performance. The orange point identifies the active airflow and pressure; the efficiency graphs show the winter and, when enabled, summer working points." & vbCrLf & vbCrLf & "Verify that the requested airflow lies inside the usable fan curve. Any enabled water or electric coil adds its air-side pressure loss and can reduce the maximum admissible airflow."),
        Topic("Help_Topic_WaterCoils", "Water coils", "The Water coils tab is available only when the selected unit has compatible relationships in the database. Choose the calculation mode CWD, HWD or HCD, the internal or external installation, the fluid and its temperatures." & vbCrLf & vbCrLf & "Standard uses the catalog geometry. Customized permits the allowed geometric changes and requires the design disclaimer. Check capacity, maximum air outlet temperature, air pressure drop, water pressure drop, fluid flow and velocity. HCD performs heating and cooling calculations on the same fixed geometry. Water heating and EHD electric post-heating cannot be enabled together."),
        Topic("Help_Topic_ElectricHeaters", "Electric heaters", "PEHD is an electric pre-heater: it heats winter fresh air before the heat exchanger and therefore changes the exchanger inlet and outlet temperatures. EHD is an electric post-heater: it heats supply air after heat recovery." & vbCrLf & vbCrLf & "Only heaters related to the selected unit are available. Their air-side pressure loss is included in the fan curve. PEHD may be combined with water-coil modes; EHD cannot be combined with HWD or the heating part of HCD."),
        Topic("Help_Topic_Accessories", "Accessories and functions", "Use search and category filters to find compatible accessories and control functions. Standard items are selected automatically and cannot be removed. Optional items can be selected or cleared; unavailable choices remain disabled and their tooltip explains the prerequisite." & vbCrLf & vbCrLf & "Some functions require a specific accessory or controller level. The selected-accessories summary remains visible outside the tab so the current configuration can be checked at any time."),
        Topic("Help_Topic_Save", "New, save and reopen selections", "New selection starts a blank technical selection after offering to save unsaved work. Open selection restores an .sswsel file, including its configuration, accessories, local draft reference and registered selection history." & vbCrLf & vbCrLf & "Save updates the current file. Save as writes the same technical selection to another path: it changes the file location, not the identity of the selection. The customer reference is sanitized and used as a prefix in the proposed file name. An asterisk in the window title indicates unsaved changes."),
        Topic("Help_Topic_Duplicate", "Duplicate and create alternatives", "Duplicate as new selection copies the current configuration but assigns a new independent selection identity and local draft reference. Use it when the copied unit must no longer share the original revision history." & vbCrLf & vbCrLf & "Create an alternative also creates an independent identity, but adds Alt. 01 to the customer reference. Creating another alternative from an existing one increments the number. Use alternatives for technically comparable proposals belonging to the same customer request."),
        Topic("Help_Topic_Report", "Reports, registration and revisions", "Generate report validates the current configuration, registers the technical selection when the service is available, creates a localized PDF and opens the report viewer. From the viewer you can inspect the result, send the PDF by email or add it to a selection project." & vbCrLf & vbCrLf & "Generating a report again after changing an already registered selection creates the next revision of the same identity. Duplicating or creating an alternative first creates a different identity instead. If online registration is unavailable, SSW can generate a local draft without interrupting technical work."),
        Topic("Help_Topic_Project", "Multi-selection projects", "A selection project (.sswproj) groups reports for several ventilation units under one project reference and language. It does not replace the individual .sswsel files: each row retains the serialized selection and its current PDF." & vbCrLf & vbCrLf & "Create or open the project from File > Selection project. Generate each unit report and use Add to project in the report viewer. In the project window you can reopen a selection, remove obsolete rows, update PDFs and prepare one localized email containing every current report and a summary table."),
        Topic("Help_Topic_Exports", "Commercial sheet and IOM", "Commercial sheet and IOM are supplementary documents and are enabled only when content exists for the selected unit, branch and language. They do not save the technical selection and do not replace the calculation report." & vbCrLf & vbCrLf & "Save the .sswsel file first when you need to preserve editable calculation inputs. Generate report when you need the traceable technical result and its revision reference."),
        Topic("Help_Topic_Tips", "Help tips", "Contextual help tips explain controls and operational menu commands without opening the guide. They are enabled by default on a new installation. Disable or enable them from Options or Help; the preference is stored in the Windows user profile and is retained after an SSW update." & vbCrLf & vbCrLf & "Language names in the language menu are intentionally always displayed as autonyms, so every user can recognize their own language even when the current interface uses another language.")
    }

    Private ReadOnly m_Environment As CLEnvironment
    Private ReadOnly m_AllTopics As New List(Of HelpTopic)()
    Private ReadOnly m_TopicList As New ListBox()
    Private ReadOnly m_Content As New RichTextBox()
    Private ReadOnly m_Search As New TextBox()
    Private ReadOnly m_SearchLabel As New Label()
    Private ReadOnly m_Close As New Button()

    Public Sub New(environment As CLEnvironment)
        If environment Is Nothing Then Throw New ArgumentNullException(NameOf(environment))
        m_Environment = environment
        InitializeLayout()
        LoadLocalizedContent()
    End Sub

    Private Sub InitializeLayout()
        Text = "SSW Help"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(760, 500)
        Size = New Size(980, 680)
        ShowIcon = False
        ShowInTaskbar = False

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .Padding = New Padding(12)
        }
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
        Controls.Add(root)

        Dim searchPanel As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1}
        searchPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 105))
        searchPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        m_SearchLabel.Dock = DockStyle.Fill
        m_SearchLabel.TextAlign = ContentAlignment.MiddleLeft
        m_Search.Dock = DockStyle.Fill
        searchPanel.Controls.Add(m_SearchLabel, 0, 0)
        searchPanel.Controls.Add(m_Search, 1, 0)
        root.Controls.Add(searchPanel, 0, 0)

        Dim split As New SplitContainer With {.Dock = DockStyle.Fill, .SplitterDistance = 235, .FixedPanel = FixedPanel.Panel1}
        m_TopicList.Dock = DockStyle.Fill
        m_TopicList.IntegralHeight = False
        m_TopicList.Font = New Font(Font, FontStyle.Regular)
        split.Panel1.Controls.Add(m_TopicList)

        m_Content.Dock = DockStyle.Fill
        m_Content.ReadOnly = True
        m_Content.BorderStyle = BorderStyle.FixedSingle
        m_Content.BackColor = SystemColors.Window
        m_Content.Font = New Font("Segoe UI", 10.0F)
        m_Content.DetectUrls = True
        split.Panel2.Padding = New Padding(10, 0, 0, 0)
        split.Panel2.Controls.Add(m_Content)
        root.Controls.Add(split, 0, 1)

        Dim buttonPanel As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(0, 7, 0, 0)}
        m_Close.Size = New Size(100, 28)
        m_Close.DialogResult = DialogResult.OK
        buttonPanel.Controls.Add(m_Close)
        root.Controls.Add(buttonPanel, 0, 2)
        AcceptButton = m_Close
        CancelButton = m_Close

        AddHandler m_Search.TextChanged, AddressOf Search_TextChanged
        AddHandler m_TopicList.SelectedIndexChanged, AddressOf TopicList_SelectedIndexChanged
    End Sub

    Private Sub LoadLocalizedContent()
        Text = T("Help_Title", "SSW user guide")
        m_SearchLabel.Text = T("Help_Search", "Search")
        m_Close.Text = T("Help_Close", "Close")

        m_AllTopics.Clear()
        For Each definition As HelpTopicDefinition In TopicDefinitions
            AddTopic(definition.TitleKey, definition.TitleFallback, definition.BodyKey, definition.BodyFallback)
        Next

        ApplyFilter()
    End Sub

    Private Sub AddTopic(titleKey As String, titleFallback As String, bodyKey As String, bodyFallback As String)
        m_AllTopics.Add(New HelpTopic With {.Title = T(titleKey, titleFallback), .Body = T(bodyKey, bodyFallback)})
    End Sub

    Private Shared Function Topic(keyRoot As String, titleFallback As String, bodyFallback As String) As HelpTopicDefinition
        Return New HelpTopicDefinition With {
            .TitleKey = keyRoot & "_Title",
            .TitleFallback = titleFallback,
            .BodyKey = keyRoot & "_Body",
            .BodyFallback = bodyFallback
        }
    End Function

    Private Sub Search_TextChanged(sender As Object, e As EventArgs)
        ApplyFilter()
    End Sub

    Private Sub ApplyFilter()
        Dim searchText As String = m_Search.Text.Trim()
        m_TopicList.BeginUpdate()
        m_TopicList.Items.Clear()
        For Each topic As HelpTopic In m_AllTopics
            If searchText.Length = 0 OrElse topic.Title.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0 OrElse topic.Body.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0 Then
                m_TopicList.Items.Add(topic)
            End If
        Next
        m_TopicList.EndUpdate()
        If m_TopicList.Items.Count > 0 Then
            m_TopicList.SelectedIndex = 0
        Else
            m_Content.Clear()
        End If
    End Sub

    Private Sub TopicList_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim topic As HelpTopic = TryCast(m_TopicList.SelectedItem, HelpTopic)
        If topic Is Nothing Then Return
        m_Content.Text = topic.Title & Environment.NewLine & Environment.NewLine & topic.Body
        m_Content.Select(0, topic.Title.Length)
        m_Content.SelectionFont = New Font(m_Content.Font, FontStyle.Bold)
        m_Content.Select(0, 0)
    End Sub

    Private Function T(key As String, fallback As String) As String
        Try
            Dim value As String = m_Environment.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso Not value.StartsWith("@@", StringComparison.Ordinal) Then Return value
        Catch
        End Try
        Return fallback
    End Function
End Class
