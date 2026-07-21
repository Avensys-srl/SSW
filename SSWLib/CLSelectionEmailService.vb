Imports System.Runtime.InteropServices

Public NotInheritable Class CLSelectionEmailComposer
    Private Sub New()
    End Sub

    Public Shared Function BuildSubject(subjectFormat As String,
        modelName As String,
        customerReference As String,
        airFlow As String,
        pressure As String,
        registrationReference As String) As String

        Dim selectionName As String = CLSelectionFileName.BuildSuggestedName(
            If(modelName, String.Empty).Trim(), customerReference)
        If Not String.IsNullOrWhiteSpace(airFlow) Then
            selectionName &= "_" & airFlow.Trim()
        End If
        If Not String.IsNullOrWhiteSpace(pressure) Then
            selectionName &= "_" & pressure.Trim()
        End If
        Dim subject As String = String.Format(Globalization.CultureInfo.CurrentCulture,
            subjectFormat, selectionName)
        Dim reference As String = If(registrationReference, String.Empty).Trim()
        If Not String.IsNullOrWhiteSpace(reference) Then subject &= " - " & reference
        Return subject
    End Function

    Public Shared Function BuildBody(bodyFormat As String,
        customerReferenceFormat As String,
        modelName As String,
        airFlow As String,
        pressure As String,
        customerReference As String) As String

        Dim referenceParagraph As String = String.Empty
        If Not String.IsNullOrWhiteSpace(customerReference) Then
            referenceParagraph = Environment.NewLine & Environment.NewLine &
                String.Format(Globalization.CultureInfo.CurrentCulture,
                    customerReferenceFormat, customerReference.Trim())
        End If
        Return String.Format(Globalization.CultureInfo.CurrentCulture,
            bodyFormat,
            If(modelName, String.Empty).Trim(),
            If(airFlow, String.Empty).Trim(),
            If(pressure, String.Empty).Trim(),
            referenceParagraph)
    End Function
End Class

Public Enum CLOutlookEmailFailure
    OutlookUnavailable
    AttachmentFailed
    MessageFailed
End Enum

Public NotInheritable Class CLOutlookEmailException
    Inherits Exception

    Public Sub New(failure As CLOutlookEmailFailure, innerException As Exception)
        MyBase.New(failure.ToString(), innerException)
        Me.Failure = failure
    End Sub

    Public ReadOnly Property Failure As CLOutlookEmailFailure
End Class

Public NotInheritable Class CLOutlookEmailService
    Private Const OutlookMailItem As Integer = 0

    Private Sub New()
    End Sub

    Public Shared Sub DisplayMessage(subject As String, body As String, attachmentPath As String)
        If String.IsNullOrWhiteSpace(attachmentPath) OrElse Not IO.File.Exists(attachmentPath) Then
            Throw New CLOutlookEmailException(CLOutlookEmailFailure.AttachmentFailed,
                New IO.FileNotFoundException("Email attachment not found.", attachmentPath))
        End If

        Dim outlookApplication As Object = Nothing
        Dim mailItem As Object = Nothing
        Dim attachments As Object = Nothing
        Dim attachment As Object = Nothing
        Try
            Dim outlookType As Type = Type.GetTypeFromProgID("Outlook.Application")
            If outlookType Is Nothing Then
                Throw New CLOutlookEmailException(CLOutlookEmailFailure.OutlookUnavailable,
                    New InvalidOperationException("Outlook.Application is not registered."))
            End If

            Try
                outlookApplication = Activator.CreateInstance(outlookType)
                mailItem = outlookApplication.CreateItem(OutlookMailItem)
            Catch ex As CLOutlookEmailException
                Throw
            Catch ex As Exception
                Throw New CLOutlookEmailException(CLOutlookEmailFailure.OutlookUnavailable, ex)
            End Try

            mailItem.Subject = subject
            mailItem.Body = body
            Try
                attachments = mailItem.Attachments
                attachment = attachments.Add(IO.Path.GetFullPath(attachmentPath))
            Catch ex As Exception
                Throw New CLOutlookEmailException(CLOutlookEmailFailure.AttachmentFailed, ex)
            End Try

            Try
                mailItem.Display(False)
            Catch ex As Exception
                Throw New CLOutlookEmailException(CLOutlookEmailFailure.MessageFailed, ex)
            End Try
        Finally
            ReleaseComObject(attachment)
            ReleaseComObject(attachments)
            ReleaseComObject(mailItem)
            ReleaseComObject(outlookApplication)
        End Try
    End Sub

    Private Shared Sub ReleaseComObject(value As Object)
        If value Is Nothing OrElse Not Marshal.IsComObject(value) Then Return
        Try
            Marshal.FinalReleaseComObject(value)
        Catch
        End Try
    End Sub
End Class
