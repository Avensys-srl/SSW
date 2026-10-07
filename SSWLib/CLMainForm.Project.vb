Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports Climalombarda.Common
Imports Climalombarda.Common.UI
Imports Microsoft.Reporting.WinForms

Partial Public Class CLMainForm
    Private m_NextUiReportSink As Action(Of List(Of ReportDataSource), String)
    Private m_NextUiPreparingReport As Boolean
    Private m_NextUiReportHost As Boolean
    Private m_ProjectSuppressClosePrompt As Boolean

    Public Sub Project_EnableNextUiReportHost()
        m_NextUiReportHost = True
        m_ProjectSuppressClosePrompt = True
    End Sub

    Public Sub Project_ApplyNextUiDocument(document As CLSelectionProjectDocument)
        If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
        m_ProjectDocument = document
        m_ProjectFilePath = Nothing
        Project_ApplyDocument(document)
    End Sub

    Public Function Project_ResolveNextUiProductDocuments(
        document As CLSelectionProjectDocument,
        languageCode As String) As CLNextUiProductDocuments

        Return Project_ResolveNextUiProductDocumentsDirect(
            document, languageCode, Environment.Branch.ShortName)
    End Function

    Public Shared Function Project_ResolveNextUiProductDocumentsDirect(
        document As CLSelectionProjectDocument,
        languageCode As String,
        shortname As String) As CLNextUiProductDocuments

        Return CLProductDocumentService.Resolve(
            document,
            languageCode,
            shortname,
            My.Settings.CommercialSheetAutoSyncEnabled)
    End Function

    Public Sub Project_SaveNextUiDocument(document As CLSelectionProjectDocument)
        Project_ApplyNextUiDocument(document)
        Project_Save(True)
    End Sub

    Public Async Sub Project_GenerateNextUiReport(document As CLSelectionProjectDocument)
        Await Project_GenerateNextUiReportAsync(document)
    End Sub

    Public Async Function Project_GenerateNextUiReportAsync(
        document As CLSelectionProjectDocument) As Task

        Project_ApplyNextUiDocument(document)
        If Await Project_RegisterBeforeReportAsync() Then
            Report_Generate()
        End If
    End Function

    Public Async Function Project_PrepareNextUiReportAsync(
        document As CLSelectionProjectDocument,
        Optional registerSelection As Boolean = True) As Task(Of CLPreparedNextUiReport)

        Dim prepared As CLPreparedNextUiReport = Nothing
        m_ProjectSuppressClosePrompt = True
        m_NextUiPreparingReport = True
        Try
            Project_ApplyNextUiDocument(document)
            If registerSelection AndAlso
                Not Await Project_RegisterBeforeReportAsync() Then Return Nothing

            m_NextUiReportSink =
                Sub(sources As List(Of ReportDataSource), reportTemplate As String)
                    prepared = New CLPreparedNextUiReport With {
                        .DataSources = sources,
                        .ReportTemplate = reportTemplate
                    }
                End Sub
            Report_Generate()
        Finally
            m_NextUiReportSink = Nothing
            m_NextUiPreparingReport = False
        End Try
        Return prepared
    End Function

    Private ReadOnly m_ProjectMenuNew As New ToolStripMenuItem()
    Private ReadOnly m_ProjectMenuOpen As New ToolStripMenuItem()
    Private ReadOnly m_ProjectMenuSave As New ToolStripMenuItem()
    Private ReadOnly m_ProjectMenuSaveAs As New ToolStripMenuItem()
    Private ReadOnly m_ProjectMenuDuplicate As New ToolStripMenuItem()
    Private ReadOnly m_ProjectMenuAlternative As New ToolStripMenuItem()
    Private ReadOnly m_ProjectMenuRecent As New ToolStripMenuItem()
    Private ReadOnly m_ProjectMenuSeparator As New ToolStripSeparator()
    Private m_ProjectDocument As CLSelectionProjectDocument
    Private m_ProjectFilePath As String
    Private m_ProjectDirty As Boolean
    Private m_ProjectApplying As Boolean = True
    Private m_ProjectFormLoaded As Boolean
    Private m_ProjectBaseTitle As String

    Private Shared ReadOnly Property ProjectRecentFilePath As String
        Get
            Dim testPath As String = System.Environment.GetEnvironmentVariable("SSW_SELECTION_RECENT_PATH")
            If Not String.IsNullOrWhiteSpace(testPath) Then Return Path.GetFullPath(testPath)
            Return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "Avensys",
                "SSW",
                "recent-selections.txt")
        End Get
    End Property

    Private Sub Project_InitializeMenus()
        m_ProjectBaseTitle = Me.Text
        m_ProjectMenuNew.ShortcutKeys = Keys.Control Or Keys.N
        m_ProjectMenuOpen.ShortcutKeys = Keys.Control Or Keys.O
        m_ProjectMenuSave.ShortcutKeys = Keys.Control Or Keys.S
        AddHandler m_ProjectMenuNew.Click, AddressOf Project_NewClick
        AddHandler m_ProjectMenuOpen.Click, AddressOf Project_OpenClick
        AddHandler m_ProjectMenuSave.Click, AddressOf Project_SaveClick
        AddHandler m_ProjectMenuSaveAs.Click, AddressOf Project_SaveAsClick
        AddHandler m_ProjectMenuDuplicate.Click, AddressOf Project_DuplicateClick
        AddHandler m_ProjectMenuAlternative.Click, AddressOf Project_CreateAlternativeClick

        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuSeparator)
        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuRecent)
        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuDuplicate)
        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuAlternative)
        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuSaveAs)
        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuSave)
        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuOpen)
        tsmiFile.DropDownItems.Insert(0, m_ProjectMenuNew)
        Project_UpdateLocalizedTexts()
        Project_RebuildRecentMenu()
    End Sub

    Private Sub Project_CompleteFormLoad()
        m_ProjectDocument = CLSelectionProjectSerializer.CreateNew(Environment.DatabaseCompatibility)
        m_ProjectDocument.Selection.CustomerCode = Environment.CustomerCode
        m_ProjectDocument.Identity.LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
        m_ProjectApplying = False
        m_ProjectFormLoaded = True
        Project_AttachDirtyHandlers()
        Project_CaptureForm(m_ProjectDocument)
        Project_SetDirty(False)
    End Sub

    Private Sub Project_AttachDirtyHandlers()
        For Each textBox As TextBox In New TextBox() {
            txbPerformance_AirFlow, TextBox1, txbPerformance_MaxPressure, TextBox2,
            txbPerformance_FreshInletTemperature, txbPerformance_RHFreshInlet,
            txbPerformance_ReturnInletTemperature, txbPerformance_RHReturnInlet,
            TextBox3, TextBox4, TextBox5, TextBox6, m_Note_Text}
            AddHandler textBox.TextChanged, AddressOf Project_InputChanged
        Next
        AddHandler cmbPerformance_Series.SelectedIndexChanged, AddressOf Project_InputChanged
        AddHandler cmbPerformance_HeatRecoveryModels.SelectedIndexChanged, AddressOf Project_InputChanged
        AddHandler hsbPerformance_RegulationLevel.ValueChanged, AddressOf Project_InputChanged
        AddHandler chbSoundPerformances_addtoreport.CheckedChanged, AddressOf Project_InputChanged
        AddHandler chbCO2Level_addtoreport.CheckedChanged, AddressOf Project_InputChanged

        If chbCoilPerformance_Enable IsNot Nothing Then
            AddHandler chbCoilPerformance_Enable.CheckedChanged, AddressOf Project_InputChanged
            For Each combo As ComboBox In New ComboBox() {
                cmbCoilPerformance_EditMode, cmbCoilPerformance_Installation,
                cmbCoilPerformance_Mode, cmbCoilPerformance_Coil,
                cmbCoilPerformance_FluidType, cmbCoilPerformance_HeightMode,
                cmbCoilPerformance_FinSpacing}
                AddHandler combo.SelectedIndexChanged, AddressOf Project_InputChanged
            Next
            For Each numeric As NumericUpDown In New NumericUpDown() {
                nudCoilPerformance_FluidTec, nudCoilPerformance_Length,
                nudCoilPerformance_Height, nudCoilPerformance_Tubes,
                nudCoilPerformance_Rows, nudCoilPerformance_Circuits,
                nudCoilPerformance_CoolingIn, nudCoilPerformance_CoolingOut,
                nudCoilPerformance_HeatingIn, nudCoilPerformance_HeatingOut}
                AddHandler numeric.ValueChanged, AddressOf Project_InputChanged
            Next
        End If
    End Sub

    Private Sub Project_InputChanged(sender As Object, e As EventArgs)
        Project_MarkDirty()
    End Sub

    Private Sub Project_MarkDirty()
        If m_ProjectFormLoaded AndAlso Not m_ProjectApplying Then
            Project_SetDirty(True)
        End If
    End Sub

    Private Sub Project_SetDirty(value As Boolean)
        m_ProjectDirty = value
        Project_UpdateTitle()
    End Sub

    Private Sub Project_UpdateTitle()
        Dim reference As String = Nothing
        If m_ProjectDocument IsNot Nothing AndAlso m_ProjectDocument.Identity IsNot Nothing Then
            reference = m_ProjectDocument.Identity.LocalDraftReference
        End If
        Dim name As String = If(String.IsNullOrWhiteSpace(m_ProjectFilePath), reference, Path.GetFileName(m_ProjectFilePath))
        Me.Text = m_ProjectBaseTitle & If(String.IsNullOrWhiteSpace(name), "", " - " & name) & If(m_ProjectDirty, " *", "")
    End Sub

    Private Sub Project_NewClick(sender As Object, e As EventArgs)
        If Not Project_ConfirmDiscardChanges() Then Return
        Project_NewDocument()
    End Sub

    Private Sub Project_NewDocument()
        m_ProjectApplying = True
        Try
            m_ProjectDocument = CLSelectionProjectSerializer.CreateNew(Environment.DatabaseCompatibility)
            m_ProjectDocument.Selection.CustomerCode = Environment.CustomerCode
            m_ProjectDocument.Identity.LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
            m_ProjectFilePath = Nothing
            m_Note_Text.Clear()
            If cmbPerformance_Series.Items.Count > 0 Then cmbPerformance_Series.SelectedIndex = 0
            If cmbPerformance_HeatRecoveryModels.Items.Count > 0 Then cmbPerformance_HeatRecoveryModels.SelectedIndex = 0
            txbPerformance_AirFlow.Text = "100"
            TextBox1.Text = "100"
            txbPerformance_FreshInletTemperature.Text = "-10"
            txbPerformance_RHFreshInlet.Text = "80"
            txbPerformance_ReturnInletTemperature.Text = "20"
            txbPerformance_RHReturnInlet.Text = "60"
            TextBox3.Text = "32"
            TextBox4.Text = "80"
            TextBox5.Text = "26"
            TextBox6.Text = "50"
            m_SummerCalculationEnabled = True
            SeasonalCalculation_UpdateModeButton()
            m_CoilCustomDisclaimerAccepted = False
            If chbCoilPerformance_Enable IsNot Nothing Then chbCoilPerformance_Enable.Checked = False
            Calculate()
            Project_CaptureForm(m_ProjectDocument)
        Finally
            m_ProjectApplying = False
        End Try
        Project_SetDirty(False)
    End Sub

    Private Sub Project_OpenClick(sender As Object, e As EventArgs)
        If Not Project_ConfirmDiscardChanges() Then Return
        Using dialog As New OpenFileDialog()
            dialog.Filter = Project_FileFilter()
            dialog.DefaultExt = CLSelectionProjectSerializer.FileExtension.TrimStart("."c)
            If dialog.ShowDialog(Me) = DialogResult.OK Then Project_OpenFile(dialog.FileName)
        End Using
    End Sub

    Private Sub Project_OpenRecentClick(sender As Object, e As EventArgs)
        If Not Project_ConfirmDiscardChanges() Then Return
        Project_OpenFile(Convert.ToString(DirectCast(sender, ToolStripMenuItem).Tag, CultureInfo.InvariantCulture))
    End Sub

    Private Sub Project_OpenFile(filePath As String)
        Try
            Dim document As CLSelectionProjectDocument = CLSelectionProjectSerializer.Load(filePath)
            Project_ApplyDocument(document)
            Dim assignedDraftReference As Boolean = False
            If document.Identity Is Nothing Then document.Identity = New CLSelectionIdentity()
            If String.IsNullOrWhiteSpace(document.Identity.LocalDraftReference) Then
                document.Identity.LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
                assignedDraftReference = True
            End If
            m_ProjectDocument = document
            m_ProjectFilePath = Path.GetFullPath(filePath)
            Project_AddRecentFile(m_ProjectFilePath)
            Project_SetDirty(assignedDraftReference)
        Catch ex As Exception
            MessageBox.Show(Me,
                String.Format(Project_Text("MainForm_Project_OpenError", "Unable to open the selection project: {0}"), ex.Message),
                Project_Text("MainForm_Project_Title", "Technical selection"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Project_SaveClick(sender As Object, e As EventArgs)
        Project_Save(False)
    End Sub

    Private Sub Project_SaveAsClick(sender As Object, e As EventArgs)
        Project_Save(True)
    End Sub

    Private Function Project_Save(saveAs As Boolean) As Boolean
        Dim targetPath As String = m_ProjectFilePath
        If saveAs OrElse String.IsNullOrWhiteSpace(targetPath) Then
            Using dialog As New SaveFileDialog()
                dialog.Filter = Project_FileFilter()
                dialog.DefaultExt = CLSelectionProjectSerializer.FileExtension.TrimStart("."c)
                dialog.AddExtension = True
                If m_ProjectDocument IsNot Nothing AndAlso m_ProjectDocument.Identity IsNot Nothing Then
                    dialog.FileName = CLSelectionFileName.BuildSuggestedName(
                        m_ProjectDocument.Identity.LocalDraftReference,
                        If(m_Note_Text Is Nothing, String.Empty, m_Note_Text.Text))
                End If
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return False
                targetPath = dialog.FileName
            End Using
        End If

        Try
            If m_ProjectDocument Is Nothing Then
                m_ProjectDocument = CLSelectionProjectSerializer.CreateNew(Environment.DatabaseCompatibility)
            End If
            Project_CaptureForm(m_ProjectDocument)
            CLSelectionProjectSerializer.Save(targetPath, m_ProjectDocument)
            m_ProjectFilePath = Path.GetFullPath(targetPath)
            Project_AddRecentFile(m_ProjectFilePath)
            Project_SetDirty(False)
            Return True
        Catch ex As Exception
            MessageBox.Show(Me,
                String.Format(Project_Text("MainForm_Project_SaveError", "Unable to save the selection project: {0}"), ex.Message),
                Project_Text("MainForm_Project_Title", "Technical selection"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub Project_ReportPdfExported(sender As Object, eventArgs As CLPdfExportedEventArgs)
        Try
            If m_ProjectDocument Is Nothing Then
                m_ProjectDocument = CLSelectionProjectSerializer.CreateNew(Environment.DatabaseCompatibility)
                m_ProjectDocument.Selection.CustomerCode = Environment.CustomerCode
                m_ProjectDocument.Identity.LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
            End If
            Project_CaptureForm(m_ProjectDocument)
            Dim companionPath As String = CLSelectionProjectSerializer.GetReportCompanionPath(eventArgs.FilePath)
            CLSelectionProjectSerializer.Save(companionPath, m_ProjectDocument)
            m_ProjectFilePath = companionPath
            Project_AddRecentFile(companionPath)
            Project_SetDirty(False)
        Catch ex As Exception
            MessageBox.Show(Me,
                String.Format(Project_Text("MainForm_Project_SaveError", "Unable to save the selection project: {0}"), ex.Message),
                Project_Text("MainForm_Project_Title", "Technical selection"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Function Project_RegisterBeforeReportAsync() As Task(Of Boolean)
        m_ReportGeneratedAsDraft = False
        Do
            Try
                If m_ProjectDocument Is Nothing Then
                    m_ProjectDocument = CLSelectionProjectSerializer.CreateNew(Environment.DatabaseCompatibility)
                    m_ProjectDocument.Selection.CustomerCode = Environment.CustomerCode
                    m_ProjectDocument.Identity.LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
                End If
                Project_CaptureForm(m_ProjectDocument)
                Dim context As CLSelectionRegistrationContext = CLSelectionRegistrationContext.FromEnvironment(Environment)
                Dim result As CLSelectionRegistrationResult = Await Project_RegisterSelectionAttemptAsync(context)
                If Not String.Equals(result.SnapshotHash,
                    m_ProjectDocument.RevisionTracking.Current.SnapshotHash,
                    StringComparison.Ordinal) Then
                    Throw New InvalidDataException("The registration response does not match the current technical snapshot.")
                End If
                If String.IsNullOrWhiteSpace(result.ResumeToken) Then
                    Throw New InvalidDataException("The registration response does not contain the selection resume token.")
                End If
                CLSelectionSnapshotService.MarkRegistered(m_ProjectDocument,
                    result.PublicReference,
                    result.Revision,
                    result.ResumeToken,
                    DateTime.UtcNow)
                Project_PersistRegisteredSelection()
                m_ReportGeneratedAsDraft = False
                Return True
            Catch ex As Exception
                If m_ProjectDocument IsNot Nothing AndAlso m_ProjectDocument.Identity IsNot Nothing AndAlso
                    Not String.IsNullOrWhiteSpace(m_ProjectDocument.Identity.ResumeToken) Then Project_SetDirty(True)
                Using dialog As New CLSelectionRegistrationFailureForm(
                    Project_Text("MainForm_SelectionRegistration_Title", "Technical selection"),
                    String.Format(Project_Text("MainForm_SelectionRegistration_Failed",
                        "The technical selection could not be registered: {0}"), ex.Message),
                    Project_Text("MainForm_SelectionRegistration_Retry", "Retry"),
                    Project_Text("MainForm_SelectionRegistration_Draft", "Generate draft"),
                    Project_Text("MainForm_SelectionRegistration_Cancel", "Cancel"))

                    Select Case dialog.ShowChoice(Me)
                        Case CLSelectionRegistrationFailureChoice.Retry
                            Continue Do
                        Case CLSelectionRegistrationFailureChoice.GenerateDraft
                            Project_SetDirty(True)
                            m_ReportGeneratedAsDraft = True
                            Return True
                        Case Else
                            Return False
                    End Select
                End Using
            End Try
        Loop
    End Function

    Private Async Function Project_RegisterSelectionAttemptAsync(context As CLSelectionRegistrationContext) As Task(Of CLSelectionRegistrationResult)
        Using waitForm As New CLPleaseWaitForm()
            waitForm.lblMessage.Text = Project_Text("MainForm_SelectionRegistration_Wait",
                "Registering the technical selection. Please wait...")
            waitForm.Location = New Point(Left + (Width - waitForm.Width) \ 2, Top + (Height - waitForm.Height) \ 2)
            waitForm.ControlBox = False
            waitForm.Show(Me)
            waitForm.Refresh()
            Enabled = False
            Try
                Return Await m_SelectionApiClient.RegisterSelectionAsync(m_ProjectDocument, context)
            Finally
                Enabled = True
                waitForm.Close()
                Activate()
            End Try
        End Using
    End Function

    Private Sub Project_PersistRegisteredSelection()
        If String.IsNullOrWhiteSpace(m_ProjectFilePath) Then
            Project_SetDirty(True)
            Return
        End If
        Try
            CLSelectionProjectSerializer.Save(m_ProjectFilePath, m_ProjectDocument)
            Project_AddRecentFile(m_ProjectFilePath)
            Project_SetDirty(False)
        Catch ex As Exception
            Project_SetDirty(True)
            MessageBox.Show(Me,
                String.Format(Project_Text("MainForm_Project_SaveError", "Unable to save the selection project: {0}"), ex.Message),
                Project_Text("MainForm_Project_Title", "Technical selection"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub Project_DuplicateClick(sender As Object, e As EventArgs)
        Project_Duplicate(False)
    End Sub

    Private Sub Project_CreateAlternativeClick(sender As Object, e As EventArgs)
        Project_Duplicate(True)
    End Sub

    Private Sub Project_Duplicate(createAlternative As Boolean)
        If m_ProjectDocument Is Nothing Then Return
        Project_CaptureForm(m_ProjectDocument)
        Dim originalProjectId As Guid = m_ProjectDocument.ProjectId
        Dim originalCreatedAtUtc As DateTime = m_ProjectDocument.CreatedAtUtc
        Dim originalIdentity As CLSelectionIdentity = m_ProjectDocument.Identity
        Dim originalRevisionTracking As CLSelectionRevisionTracking = m_ProjectDocument.RevisionTracking
        Dim originalPath As String = m_ProjectFilePath
        Dim originalDirty As Boolean = m_ProjectDirty
        Dim originalCustomerReference As String = m_ProjectDocument.Selection.CustomerReference
        m_ProjectDocument.ProjectId = Guid.NewGuid()
        m_ProjectDocument.CreatedAtUtc = DateTime.UtcNow
        m_ProjectDocument.Identity = New CLSelectionIdentity With {
            .LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
        }
        m_ProjectDocument.RevisionTracking = New CLSelectionRevisionTracking()
        If createAlternative Then
            Dim alternativeReference As String = Project_NextAlternativeReference(originalCustomerReference)
            m_ProjectDocument.Selection.CustomerReference = alternativeReference
            m_Note_Text.Text = alternativeReference
        End If
        m_ProjectFilePath = Nothing
        Project_SetDirty(True)
        If Not Project_Save(True) Then
            m_ProjectDocument.ProjectId = originalProjectId
            m_ProjectDocument.CreatedAtUtc = originalCreatedAtUtc
            m_ProjectDocument.Identity = originalIdentity
            m_ProjectDocument.RevisionTracking = originalRevisionTracking
            m_ProjectFilePath = originalPath
            m_ProjectDocument.Selection.CustomerReference = originalCustomerReference
            m_Note_Text.Text = originalCustomerReference
            Project_SetDirty(originalDirty)
        End If
    End Sub

    Private Shared Function Project_NextAlternativeReference(reference As String) As String
        Dim value As String = If(reference, String.Empty).Trim()
        Dim match As Match = Regex.Match(value, "^Alt\.\s*(\d+)\s*:\s*(.*)$", RegexOptions.IgnoreCase)
        Dim number As Integer = 1
        Dim baseReference As String = value
        If match.Success Then
            Dim parsed As Integer
            If Integer.TryParse(match.Groups(1).Value, NumberStyles.None, CultureInfo.InvariantCulture, parsed) Then
                number = parsed + 1
            End If
            baseReference = match.Groups(2).Value.Trim()
        End If
        Dim prefix As String = "Alt. " & number.ToString("00", CultureInfo.InvariantCulture)
        Return If(String.IsNullOrWhiteSpace(baseReference), prefix, prefix & ": " & baseReference)
    End Function

    Private Function Project_ConfirmDiscardChanges() As Boolean
        If Not m_ProjectDirty Then Return True
        Dim result As DialogResult = MessageBox.Show(Me,
            Project_Text("MainForm_Project_Unsaved", "The current selection has unsaved changes. Save them now?"),
            Project_Text("MainForm_Project_Title", "Technical selection"),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question)
        If result = DialogResult.Cancel Then Return False
        If result = DialogResult.Yes Then Return Project_Save(False)
        Return True
    End Function

    Private Sub Project_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If m_ProjectSuppressClosePrompt Then Return
        If Not Project_ConfirmDiscardChanges() Then e.Cancel = True
    End Sub

    Private Sub Project_CaptureForm(document As CLSelectionProjectDocument)
        document.Versions = CLSelectionProjectSerializer.CreateCurrentVersionSet(Environment.DatabaseCompatibility)
        document.Selection.CustomerCode = Environment.CustomerCode
        document.Selection.CustomerReference = m_Note_Text.Text
        document.Selection.Unit = New CLSelectionEntityReference With {
            .Id = If(SelectedHeatRecoveryModel Is Nothing, CType(Nothing, Integer?), SelectedHeatRecoveryModel.Id),
            .Code = SelectedHeatRecoveryModelCustomerName,
            .ManagementCode = If(cmbPerformance_Series.SelectedItem Is Nothing, Nothing, cmbPerformance_Series.SelectedItem.ToString()),
            .Name = SelectedHeatRecoveryModelCustomerName
        }
        document.Selection.Winter = Project_CaptureScenario(
            True, "Winter", m_WinterReportScenarioName,
            txbPerformance_AirFlow, txbPerformance_MaxPressure,
            txbPerformance_FreshInletTemperature, txbPerformance_RHFreshInlet,
            txbPerformance_ReturnInletTemperature, txbPerformance_RHReturnInlet)
        document.Selection.Summer = Project_CaptureScenario(
            m_SummerCalculationEnabled, "Summer", "Summer",
            TextBox1, TextBox2, TextBox3, TextBox4, TextBox5, TextBox6)
        document.Selection.WaterCoil = Project_CaptureWaterCoil()
        document.Selection.ElectricHeater = ElectricHeater_CaptureSelection()
        document.Selection.Accessories = Accessories_CaptureSelection()
        document.Selection.Report = New CLReportSelectionOptions With {
            .LanguageCode = Environment.PrimaryLanguageCode,
            .IncludePerformanceCharts = True,
            .IncludeSoundPower = chbSoundPerformances_addtoreport.Checked,
            .IncludeCo2 = chbCO2Level_addtoreport.Checked
        }
        document.Snapshot = Project_CaptureSnapshot(document.Versions)

        document.Features.Clear()
        If m_SummerCalculationEnabled Then document.Features.Add("SummerCalculation")
        If document.Selection.WaterCoil.Enabled Then document.Features.Add("WaterCoils")
        If document.Selection.ElectricHeater.Enabled Then document.Features.Add("ElectricHeaters")
        If document.Selection.Accessories.Count > 0 Then document.Features.Add("AccessoriesAndControlFunctions")
        CLSelectionSnapshotService.Refresh(document)
    End Sub

    Private Function Project_CaptureScenario(enabled As Boolean, scenarioCode As String, standardCode As String,
        airflow As TextBox, pressure As TextBox, outdoorTemperature As TextBox, outdoorRh As TextBox,
        returnTemperature As TextBox, returnRh As TextBox) As CLOperatingScenarioInput

        Dim flow As Double? = Project_ParseNullable(airflow.Text)
        Return New CLOperatingScenarioInput With {
            .Enabled = enabled,
            .ScenarioCode = scenarioCode,
            .StandardCode = standardCode,
            .SupplyAirflowM3h = flow,
            .ExtractAirflowM3h = flow,
            .MaximumPressurePa = Project_ParseNullable(pressure.Text),
            .OutdoorTemperatureC = Project_ParseNullable(outdoorTemperature.Text),
            .OutdoorRelativeHumidityPercent = Project_ParseNullable(outdoorRh.Text),
            .ReturnTemperatureC = Project_ParseNullable(returnTemperature.Text),
            .ReturnRelativeHumidityPercent = Project_ParseNullable(returnRh.Text),
            .RegulationPercent = hsbPerformance_RegulationLevel.Value
        }
    End Function

    Private Function Project_CaptureWaterCoil() As CLWaterCoilSelection
        Dim selection As New CLWaterCoilSelection()
        If chbCoilPerformance_Enable Is Nothing Then Return selection
        selection.Enabled = chbCoilPerformance_Enable.Checked
        selection.CustomDesignDisclaimerAccepted = m_CoilCustomDisclaimerAccepted
        selection.SelectionCase = CoilPerformance_SelectedEditMode().ToString()
        selection.InstallationType = CoilPerformance_SelectedInstallation().ToString()
        selection.CalculationMode = Convert.ToString(cmbCoilPerformance_Mode.SelectedItem, CultureInfo.InvariantCulture)
        Dim coil As CLCoilDefinition = TryCast(cmbCoilPerformance_Coil.SelectedItem, CLCoilDefinition)
        If coil IsNot Nothing Then
            selection.Coil = New CLSelectionEntityReference With {.Id = coil.Id, .Code = coil.Name, .Name = coil.Name}
        End If
        selection.Fluid = New CLFluidSelection With {
            .Code = CoilPerformance_SelectedFluidType().ToString(),
            .GlycolPercent = CDbl(nudCoilPerformance_FluidTec.Value)
        }
        selection.Geometry = New CLCoilGeometrySelection With {
            .GeometryCode = "2510",
            .LengthMm = CInt(nudCoilPerformance_Length.Value),
            .HeightMm = CInt(nudCoilPerformance_Height.Value),
            .Tubes = CInt(nudCoilPerformance_Tubes.Value),
            .NumberOfRows = CInt(nudCoilPerformance_Rows.Value),
            .FinSpacingMm = CoilPerformance_SelectedFinSpacing(),
            .NumberOfCircuits = CInt(nudCoilPerformance_Circuits.Value)
        }
        selection.CoolingWaterInletTemperatureC = CDbl(nudCoilPerformance_CoolingIn.Value)
        selection.CoolingWaterOutletTemperatureC = CDbl(nudCoilPerformance_CoolingOut.Value)
        selection.HeatingWaterInletTemperatureC = CDbl(nudCoilPerformance_HeatingIn.Value)
        selection.HeatingWaterOutletTemperatureC = CDbl(nudCoilPerformance_HeatingOut.Value)
        Return selection
    End Function

    Private Function Project_CaptureSnapshot(versions As CLSelectionVersionSet) As CLCalculatedSelectionSnapshot
        Dim snapshot As New CLCalculatedSelectionSnapshot With {
            .CalculatedAtUtc = DateTime.UtcNow,
            .Versions = versions,
            .Winter = Project_CaptureScenarioSnapshot("Winter", txbPerformance_AirFlow, txbPerformance_MaxPressure,
                txbPerformance_HeatTransferred, txbPerformance_SensibleHeat, txbPerformance_LatentHeat,
                lblPerformance_Efficiency, txbPerformance_WaterProduced, txbPerformance_SupplyOutletTemperature,
                txbPerformance_SupplyOutletRH, txbPerformance_ExhaustOutletTemperature, txbPerformance_ExhaustOutletRH)
        }
        If m_SummerCalculationEnabled Then
            snapshot.Summer = Project_CaptureScenarioSnapshot("Summer", TextBox1, TextBox2,
                TextBox10, TextBox9, TextBox7, TextBox11, TextBox8, TextBox12, TextBox14, TextBox13, TextBox15)
        End If
        snapshot.WaterCoils = Project_CaptureWaterCoilSnapshots()
        snapshot.ElectricHeaters = ElectricHeater_CaptureSnapshots()
        Return snapshot
    End Function

    Private Function Project_CaptureWaterCoilSnapshots() As List(Of CLWaterCoilCalculationSnapshot)
        Dim snapshots As New List(Of CLWaterCoilCalculationSnapshot)()
        If chbCoilPerformance_Enable Is Nothing OrElse Not chbCoilPerformance_Enable.Checked Then Return snapshots
        For Each result As CLCoilCalculationResult In m_CoilPerformanceLastResults
            Dim scenarioCode As String = If(result.Mode = CLCoilPerformanceMode.CWD AndAlso m_SummerCalculationEnabled,
                "Summer", "Winter")
            snapshots.Add(New CLWaterCoilCalculationSnapshot With {
                .ScenarioCode = scenarioCode,
                .Mode = result.Mode.ToString(),
                .Status = If(result.IsOk, "OK", If(String.IsNullOrWhiteSpace(result.ErrorMessage), result.Auxiliary.ToString(), result.ErrorMessage)),
                .CapacityW = result.HeatTransferred,
                .SensibleCapacityW = result.SensibleHeat,
                .AirOutletTemperatureC = result.OutletTemperature,
                .AirOutletRelativeHumidityPercent = result.OutletRH,
                .CondensateLitersPerHour = result.CondensedWater,
                .AirPressureDropPa = result.AirPressureDrop,
                .FluidPressureDropKPa = result.WaterPressureDrop,
                .FluidFlowLitersPerHour = result.FluidFlow,
                .FluidVelocityMetersPerSecond = result.FluidSpeed,
                .FaceVelocityMetersPerSecond = result.FaceVelocity
            })
        Next
        Return snapshots
    End Function

    Private Function Project_CaptureScenarioSnapshot(code As String, airflow As TextBox, pressure As TextBox,
        heat As TextBox, sensible As TextBox, latent As TextBox, efficiency As Control, condensate As TextBox,
        supplyTemperature As TextBox, supplyRh As TextBox, exhaustTemperature As TextBox, exhaustRh As TextBox) As CLScenarioCalculationSnapshot

        Return New CLScenarioCalculationSnapshot With {
            .ScenarioCode = code,
            .AirflowM3h = Project_ParseNullable(airflow.Text),
            .AvailablePressurePa = Project_ParseNullable(pressure.Text),
            .AbsorbedPowerW = Project_ParseNullable(txbPerformance_ElectricalPerformances_PowerInput.Text),
            .HeatTransferredW = Project_ParseNullable(heat.Text),
            .SensibleHeatW = Project_ParseNullable(sensible.Text),
            .LatentHeatW = Project_ParseNullable(latent.Text),
            .EfficiencyPercent = Project_ParseNullable(efficiency.Text),
            .CondensateLitersPerHour = Project_ParseNullable(condensate.Text),
            .SupplyOutletTemperatureC = Project_ParseNullable(supplyTemperature.Text),
            .SupplyOutletRelativeHumidityPercent = Project_ParseNullable(supplyRh.Text),
            .ExhaustOutletTemperatureC = Project_ParseNullable(exhaustTemperature.Text),
            .ExhaustOutletRelativeHumidityPercent = Project_ParseNullable(exhaustRh.Text)
        }
    End Function

    Private Sub Project_ApplyDocument(document As CLSelectionProjectDocument)
        m_ProjectApplying = True
        Try
            Dim unit As CLSelectionEntityReference = document.Selection.Unit
            If Not String.IsNullOrWhiteSpace(unit.ManagementCode) Then
                Project_SelectComboText(cmbPerformance_Series, unit.ManagementCode)
            End If
            If Not Project_SelectModel(unit) Then
                Throw New InvalidDataException(String.Format("Unit '{0}' is not available in the current database.", unit.Code))
            End If

            m_Note_Text.Text = document.Selection.CustomerReference
            Project_ApplyScenario(document.Selection.Winter, txbPerformance_AirFlow, txbPerformance_MaxPressure,
                txbPerformance_FreshInletTemperature, txbPerformance_RHFreshInlet,
                txbPerformance_ReturnInletTemperature, txbPerformance_RHReturnInlet)
            Project_ApplyScenario(document.Selection.Summer, TextBox1, TextBox2, TextBox3, TextBox4, TextBox5, TextBox6)
            Project_ApplyRegulationLevel(document.Selection.Winter, document.Selection.Summer)
            m_SummerCalculationEnabled = document.Selection.Summer.Enabled
            m_WinterReportScenarioName = document.Selection.Winter.StandardCode
            SeasonalCalculation_UpdateModeButton()
            Project_ApplyWaterCoil(document.Selection.WaterCoil)
            ElectricHeater_ApplySelection(document.Selection.ElectricHeater)
            Accessories_ApplySelection(document.Selection.Accessories)
            chbSoundPerformances_addtoreport.Checked = document.Selection.Report.IncludeSoundPower
            chbCO2Level_addtoreport.Checked = document.Selection.Report.IncludeCo2
            Calculate()
        Finally
            m_ProjectApplying = False
        End Try
    End Sub

    Private Function Project_SelectModel(reference As CLSelectionEntityReference) As Boolean
        If Project_SelectModelFromCurrentSeries(reference) Then Return True

        Dim resolvedModel = Environment.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(model) _
                (reference.Id.HasValue AndAlso model.Id = reference.Id.Value) OrElse
                    (Not String.IsNullOrWhiteSpace(reference.Code) AndAlso
                     (String.Equals(model.Code, reference.Code, StringComparison.OrdinalIgnoreCase) OrElse
                      String.Equals(model.Name, reference.Code, StringComparison.OrdinalIgnoreCase))))
        If resolvedModel Is Nothing OrElse resolvedModel.CLSerie Is Nothing Then Return False

        Dim customerSeriesName = Environment.GetCustomerSerieName(resolvedModel.CLSerie)
        Project_SelectComboText(cmbPerformance_Series, customerSeriesName)
        If Project_SelectModelFromCurrentSeries(reference) Then Return True

        ' A canonical project may reference a technically valid SDF model that is
        ' not exposed by the current legacy customer-name filter. Keep project
        ' loading and the SSW Next report adapter bound to the resolved SDF row.
        Dim customerModelName = Environment.GetCustomerHeatRecoveryModelName(resolvedModel)
        If String.IsNullOrWhiteSpace(customerModelName) Then
            customerModelName = resolvedModel.Code
        End If
        cmbPerformance_HeatRecoveryModels.Items.Add(
            New CLComboBoxItemWrapper(Of Climalombarda.DataCentral.LTModel.CLDCHeatRecoveryModel)(
                customerModelName, resolvedModel))
        cmbPerformance_HeatRecoveryModels.SelectedIndex =
            cmbPerformance_HeatRecoveryModels.Items.Count - 1
        Return True
    End Function

    Private Function Project_SelectModelFromCurrentSeries(
        reference As CLSelectionEntityReference) As Boolean

        For index As Integer = 0 To cmbPerformance_HeatRecoveryModels.Items.Count - 1
            Dim wrapper = TryCast(cmbPerformance_HeatRecoveryModels.Items(index), CLComboBoxItemWrapper(Of Climalombarda.DataCentral.LTModel.CLDCHeatRecoveryModel))
            If wrapper Is Nothing Then Continue For
            If (Not String.IsNullOrWhiteSpace(reference.Code) AndAlso
                (String.Equals(wrapper.Text, reference.Code, StringComparison.OrdinalIgnoreCase) OrElse
                 String.Equals(wrapper.Value.Name, reference.Code, StringComparison.OrdinalIgnoreCase))) OrElse
                (reference.Id.HasValue AndAlso wrapper.Value.Id = reference.Id.Value) Then
                cmbPerformance_HeatRecoveryModels.SelectedIndex = index
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub Project_ApplyScenario(scenario As CLOperatingScenarioInput, airflow As TextBox, pressure As TextBox,
        outdoorTemperature As TextBox, outdoorRh As TextBox, returnTemperature As TextBox, returnRh As TextBox)
        Project_SetNullableText(airflow, scenario.SupplyAirflowM3h)
        Project_SetNullableText(pressure, scenario.MaximumPressurePa)
        Project_SetNullableText(outdoorTemperature, scenario.OutdoorTemperatureC)
        Project_SetNullableText(outdoorRh, scenario.OutdoorRelativeHumidityPercent)
        Project_SetNullableText(returnTemperature, scenario.ReturnTemperatureC)
        Project_SetNullableText(returnRh, scenario.ReturnRelativeHumidityPercent)
    End Sub

    Private Sub Project_ApplyRegulationLevel(winter As CLOperatingScenarioInput, summer As CLOperatingScenarioInput)
        Dim regulationPercent As Double? = Nothing
        If winter IsNot Nothing AndAlso winter.RegulationPercent.HasValue Then
            regulationPercent = winter.RegulationPercent
        ElseIf summer IsNot Nothing AndAlso summer.RegulationPercent.HasValue Then
            regulationPercent = summer.RegulationPercent
        End If
        If regulationPercent.HasValue Then
            Performance_ApplyRegulationLevel(CInt(Math.Round(regulationPercent.Value)), False)
        End If
    End Sub

    Private Sub Project_ApplyWaterCoil(selection As CLWaterCoilSelection)
        If chbCoilPerformance_Enable Is Nothing Then Return
        m_CoilCustomDisclaimerAccepted = selection.CustomDesignDisclaimerAccepted
        Project_SelectWrappedEnum(cmbCoilPerformance_Installation, selection.InstallationType)
        CoilPerformance_FillStandardCoils()
        Project_SelectWrappedEnum(cmbCoilPerformance_EditMode, selection.SelectionCase)
        Project_SelectEnum(cmbCoilPerformance_Mode, selection.CalculationMode)
        If selection.Coil IsNot Nothing Then Project_SelectCoil(selection.Coil)
        If selection.Fluid IsNot Nothing Then
            Project_SelectWrappedEnum(cmbCoilPerformance_FluidType, selection.Fluid.Code)
            Project_SetNumeric(nudCoilPerformance_FluidTec, selection.Fluid.GlycolPercent)
        End If
        If selection.Geometry IsNot Nothing Then
            Project_SetNumeric(nudCoilPerformance_Length, selection.Geometry.LengthMm)
            Project_SetNumeric(nudCoilPerformance_Height, selection.Geometry.HeightMm)
            Project_SetNumeric(nudCoilPerformance_Tubes, selection.Geometry.Tubes)
            Project_SetNumeric(nudCoilPerformance_Rows, selection.Geometry.NumberOfRows)
            Project_SetNumeric(nudCoilPerformance_Circuits, selection.Geometry.NumberOfCircuits)
            If selection.Geometry.FinSpacingMm.HasValue Then CoilPerformance_SelectFinSpacing(selection.Geometry.FinSpacingMm.Value)
        End If
        Dim wasCoilPerformanceChanging As Boolean = m_CoilPerformanceChanging
        Try
            m_CoilPerformanceChanging = True
            CoilPerformance_ResetWaterTemperatureLimits()
            Project_SetNumeric(nudCoilPerformance_CoolingIn, selection.CoolingWaterInletTemperatureC)
            Project_SetNumeric(nudCoilPerformance_CoolingOut, selection.CoolingWaterOutletTemperatureC)
            Project_SetNumeric(nudCoilPerformance_HeatingIn, selection.HeatingWaterInletTemperatureC)
            Project_SetNumeric(nudCoilPerformance_HeatingOut, selection.HeatingWaterOutletTemperatureC)
            CoilPerformance_UpdateWaterTemperatureLimits()
        Finally
            m_CoilPerformanceChanging = wasCoilPerformanceChanging
        End Try
        chbCoilPerformance_Enable.Checked = selection.Enabled AndAlso m_CoilPerformanceAvailable
        CoilPerformance_UpdateControlState()
    End Sub

    Private Sub Project_SelectCoil(reference As CLSelectionEntityReference)
        For index As Integer = 0 To cmbCoilPerformance_Coil.Items.Count - 1
            Dim coil As CLCoilDefinition = TryCast(cmbCoilPerformance_Coil.Items(index), CLCoilDefinition)
            If coil IsNot Nothing AndAlso
                ((Not String.IsNullOrWhiteSpace(reference.Code) AndAlso String.Equals(coil.Name, reference.Code, StringComparison.OrdinalIgnoreCase)) OrElse
                 (reference.Id.HasValue AndAlso coil.Id = reference.Id.Value)) Then
                cmbCoilPerformance_Coil.SelectedIndex = index
                Exit For
            End If
        Next
    End Sub

    Private Shared Sub Project_SelectComboText(combo As ComboBox, value As String)
        For index As Integer = 0 To combo.Items.Count - 1
            If String.Equals(combo.Items(index).ToString(), value, StringComparison.OrdinalIgnoreCase) Then
                combo.SelectedIndex = index
                Exit For
            End If
        Next
    End Sub

    Private Shared Sub Project_SelectEnum(combo As ComboBox, value As String)
        For index As Integer = 0 To combo.Items.Count - 1
            If String.Equals(combo.Items(index).ToString(), value, StringComparison.OrdinalIgnoreCase) Then
                combo.SelectedIndex = index
                Exit For
            End If
        Next
    End Sub

    Private Shared Sub Project_SelectWrappedEnum(combo As ComboBox, value As String)
        For index As Integer = 0 To combo.Items.Count - 1
            Dim item As Object = combo.Items(index)
            Dim propertyInfo = item.GetType().GetProperty("Value")
            If propertyInfo IsNot Nothing AndAlso String.Equals(Convert.ToString(propertyInfo.GetValue(item, Nothing), CultureInfo.InvariantCulture), value, StringComparison.OrdinalIgnoreCase) Then
                combo.SelectedIndex = index
                Exit For
            End If
        Next
    End Sub

    Private Shared Sub Project_SetNumeric(control As NumericUpDown, value As Double?)
        If Not value.HasValue Then Return
        control.Value = Math.Max(control.Minimum, Math.Min(control.Maximum, CDec(value.Value)))
    End Sub

    Private Shared Sub Project_SetNumeric(control As NumericUpDown, value As Integer?)
        If value.HasValue Then Project_SetNumeric(control, CDbl(value.Value))
    End Sub

    Private Shared Sub Project_SetNullableText(control As TextBox, value As Double?)
        If value.HasValue Then control.Text = value.Value.ToString("0.##", CultureInfo.CurrentCulture)
    End Sub

    Private Shared Function Project_ParseNullable(value As String) As Double?
        Dim parsed As Double
        If Double.TryParse(value, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, parsed) Then Return parsed
        If Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, parsed) Then Return parsed
        Return Nothing
    End Function

    Private Sub Project_UpdateLocalizedTexts()
        If m_ProjectMenuNew Is Nothing Then Return
        m_ProjectMenuNew.Text = Project_Text("MainForm_Project_New", "New selection")
        m_ProjectMenuOpen.Text = Project_Text("MainForm_Project_Open", "Open selection...")
        m_ProjectMenuSave.Text = Project_Text("MainForm_Project_Save", "Save selection")
        m_ProjectMenuSaveAs.Text = Project_Text("MainForm_Project_SaveAs", "Save selection as...")
        m_ProjectMenuDuplicate.Text = Project_Text("MainForm_Project_Duplicate", "Duplicate as new selection...")
        m_ProjectMenuAlternative.Text = Project_Text("MainForm_Project_CreateAlternative", "Create an alternative...")
        m_ProjectMenuRecent.Text = Project_Text("MainForm_Project_Recent", "Recent selections")
        m_MultiSelectionMenu.Text = Project_Text("MultiProject_Menu", "Selection project...")
    End Sub

    Private Function Project_Text(resourceName As String, fallback As String) As String
        Try
            Dim value As String = Environment.Localization.GetString(resourceName)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> "?" AndAlso Not value.StartsWith("@@", StringComparison.Ordinal) Then Return value
        Catch
        End Try
        Return fallback
    End Function

    Private Function Project_FileFilter() As String
        Return Project_Text("MainForm_Project_Filter", "SSW technical selection") & " (*" & CLSelectionProjectSerializer.FileExtension & ")|*" & CLSelectionProjectSerializer.FileExtension
    End Function

    Private Sub Project_AddRecentFile(filePath As String)
        Dim files As List(Of String) = Project_LoadRecentFiles()
        files.RemoveAll(Function(item) String.Equals(item, filePath, StringComparison.OrdinalIgnoreCase))
        files.Insert(0, filePath)
        If files.Count > 10 Then files.RemoveRange(10, files.Count - 10)
        Dim directoryPath As String = Path.GetDirectoryName(ProjectRecentFilePath)
        Directory.CreateDirectory(directoryPath)
        Dim temporaryPath As String = ProjectRecentFilePath & ".tmp"
        File.WriteAllLines(temporaryPath, files.ToArray())
        If File.Exists(ProjectRecentFilePath) Then
            File.Replace(temporaryPath, ProjectRecentFilePath, Nothing)
        Else
            File.Move(temporaryPath, ProjectRecentFilePath)
        End If
        Project_RebuildRecentMenu()
    End Sub

    Private Function Project_LoadRecentFiles() As List(Of String)
        If Not File.Exists(ProjectRecentFilePath) Then Return New List(Of String)()
        Return File.ReadAllLines(ProjectRecentFilePath).
            Where(Function(item) Not String.IsNullOrWhiteSpace(item) AndAlso File.Exists(item)).
            Distinct(StringComparer.OrdinalIgnoreCase).Take(10).ToList()
    End Function

    Private Sub Project_RebuildRecentMenu()
        m_ProjectMenuRecent.DropDownItems.Clear()
        For Each filePath As String In Project_LoadRecentFiles()
            Dim item As New ToolStripMenuItem(Path.GetFileName(filePath)) With {.Tag = filePath, .ToolTipText = filePath}
            AddHandler item.Click, AddressOf Project_OpenRecentClick
            m_ProjectMenuRecent.DropDownItems.Add(item)
        Next
        m_ProjectMenuRecent.Enabled = m_ProjectMenuRecent.DropDownItems.Count > 0
    End Sub

End Class
