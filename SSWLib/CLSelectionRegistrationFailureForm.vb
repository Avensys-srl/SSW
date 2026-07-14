Public Enum CLSelectionRegistrationFailureChoice
    Retry
    GenerateDraft
    Cancel
End Enum

Public NotInheritable Class CLSelectionRegistrationFailureForm
    Inherits Form

    Private ReadOnly m_Message As New Label()
    Private ReadOnly m_Retry As New Button()
    Private ReadOnly m_Draft As New Button()
    Private ReadOnly m_Cancel As New Button()

    Public Sub New(title As String, message As String, retryText As String, draftText As String, cancelText As String)
        Text = title
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        ShowInTaskbar = False
        ShowIcon = False
        MaximizeBox = False
        MinimizeBox = False
        ClientSize = New Size(520, 164)
        MinimumSize = Size
        MaximumSize = Size

        m_Message.SetBounds(16, 14, 488, 86)
        m_Message.Text = message
        m_Message.AutoEllipsis = True

        m_Retry.SetBounds(156, 116, 108, 30)
        m_Retry.Text = retryText
        m_Retry.DialogResult = DialogResult.Retry

        m_Draft.SetBounds(272, 116, 116, 30)
        m_Draft.Text = draftText
        m_Draft.DialogResult = DialogResult.Ignore

        m_Cancel.SetBounds(396, 116, 108, 30)
        m_Cancel.Text = cancelText
        m_Cancel.DialogResult = DialogResult.Cancel

        Controls.AddRange(New Control() {m_Message, m_Retry, m_Draft, m_Cancel})
        AcceptButton = m_Retry
        CancelButton = m_Cancel
    End Sub

    Public Function ShowChoice(owner As IWin32Window) As CLSelectionRegistrationFailureChoice
        Select Case ShowDialog(owner)
            Case DialogResult.Retry
                Return CLSelectionRegistrationFailureChoice.Retry
            Case DialogResult.Ignore
                Return CLSelectionRegistrationFailureChoice.GenerateDraft
            Case Else
                Return CLSelectionRegistrationFailureChoice.Cancel
        End Select
    End Function
End Class
