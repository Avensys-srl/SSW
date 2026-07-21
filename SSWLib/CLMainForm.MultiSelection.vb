Imports System.IO

Partial Public Class CLMainForm
    Private ReadOnly m_MultiSelectionMenu As New ToolStripMenuItem()
    Private m_MultiSelectionDocument As CLMultiSelectionProjectDocument
    Private m_MultiSelectionPath As String

    Private Sub MultiSelection_InitializeMenus()
        m_MultiSelectionMenu.Text = MultiSelection_Text("MultiProject_Menu", "Selection project...")
        AddHandler m_MultiSelectionMenu.Click, AddressOf MultiSelection_MenuClick
        Dim separatorIndex As Integer = tsmiFile.DropDownItems.IndexOf(m_ProjectMenuSeparator)
        If separatorIndex < 0 Then separatorIndex = tsmiFile.DropDownItems.Count
        tsmiFile.DropDownItems.Insert(separatorIndex, m_MultiSelectionMenu)
    End Sub

    Private Async Sub MultiSelection_MenuClick(sender As Object, e As EventArgs)
        Using form As New CLMultiSelectionProjectForm(
            m_MultiSelectionDocument,
            m_MultiSelectionPath,
            MultiSelection_Text("MultiProject_DefaultReference", "Project 01"),
            Environment.PrimaryLanguageCode)

            Dim result As DialogResult = form.ShowDialog(Me)
            m_MultiSelectionDocument = form.Document
            m_MultiSelectionPath = form.ProjectPath
            If result = DialogResult.OK AndAlso form.SelectionToOpen IsNot Nothing Then
                MultiSelection_OpenSelection(form.SelectionToOpen)
            End If
        End Using
        Await MultiSelection_SyncBestEffortAsync()
    End Sub

    Private Async Sub MultiSelection_AddCurrentReport(sender As Object, eventArgs As CLPdfExportedEventArgs)
        Try
            If m_ProjectDocument Is Nothing Then
                m_ProjectDocument = CLSelectionProjectSerializer.CreateNew(Environment.DatabaseCompatibility)
                m_ProjectDocument.Selection.CustomerCode = Environment.CustomerCode
                m_ProjectDocument.Identity.LocalDraftReference = CLSelectionInstallationStateStore.NextDraftReference()
            End If
            Project_CaptureForm(m_ProjectDocument)
            If m_MultiSelectionDocument Is Nothing Then
                m_MultiSelectionDocument = CLMultiSelectionProjectSerializer.CreateNew(
                    MultiSelection_Text("MultiProject_DefaultReference", "Project 01"),
                    Environment.PrimaryLanguageCode)
            End If

            CLMultiSelectionProjectSerializer.AddOrUpdate(
                m_MultiSelectionDocument,
                m_ProjectDocument,
                eventArgs.FilePath,
                Environment.PrimaryLanguageCode)

            If String.IsNullOrWhiteSpace(m_MultiSelectionPath) Then
                Using dialog As New SaveFileDialog With {
                    .Filter = MultiSelection_Text("MultiProject_Filter", "SSW selection project") &
                        " (*" & CLMultiSelectionProjectSerializer.FileExtension & ")|*" & CLMultiSelectionProjectSerializer.FileExtension,
                    .DefaultExt = CLMultiSelectionProjectSerializer.FileExtension.TrimStart("."c),
                    .AddExtension = True,
                    .FileName = MultiSelection_SafeName(m_MultiSelectionDocument.Reference)
                }
                    If dialog.ShowDialog(Me) = DialogResult.OK Then m_MultiSelectionPath = dialog.FileName
                End Using
            End If
            If Not String.IsNullOrWhiteSpace(m_MultiSelectionPath) Then
                CLMultiSelectionProjectSerializer.Save(m_MultiSelectionPath, m_MultiSelectionDocument)
            End If
            Await MultiSelection_SyncBestEffortAsync()

            MessageBox.Show(DirectCast(sender, IWin32Window),
                MultiSelection_Text("MultiProject_Added", "The report was added to the selection project."),
                MultiSelection_Text("MultiProject_Title", "Selection project"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(DirectCast(sender, IWin32Window), ex.Message,
                MultiSelection_Text("MultiProject_Title", "Selection project"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Function MultiSelection_SyncBestEffortAsync() As Task
        If m_MultiSelectionDocument Is Nothing OrElse String.IsNullOrWhiteSpace(m_MultiSelectionPath) Then Return
        Try
            Dim context As CLSelectionRegistrationContext = CLSelectionRegistrationContext.FromEnvironment(Environment)
            Await m_SelectionApiClient.SyncMultiSelectionProjectAsync(m_MultiSelectionDocument, context)
        Catch
            ' The local project remains authoritative while the central service is unavailable.
        End Try
    End Function

    Private Sub MultiSelection_OpenSelection(item As CLMultiSelectionProjectItem)
        If item Is Nothing OrElse Not Project_ConfirmDiscardChanges() Then Return
        Try
            Dim document As CLSelectionProjectDocument = CLSelectionProjectSerializer.Deserialize(item.SelectionJson)
            Project_ApplyDocument(document)
            m_ProjectDocument = document
            m_ProjectFilePath = Nothing
            Project_SetDirty(False)
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message,
                MultiSelection_Text("MultiProject_Title", "Selection project"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function MultiSelection_Text(resourceName As String, fallback As String) As String
        Return Project_Text(resourceName, fallback)
    End Function

    Private Shared Function MultiSelection_SafeName(value As String) As String
        Dim result As String = If(value, String.Empty).Trim()
        For Each invalid As Char In Path.GetInvalidFileNameChars()
            result = result.Replace(invalid, "_"c)
        Next
        Return If(String.IsNullOrWhiteSpace(result), "Project_01", result)
    End Function
End Class
