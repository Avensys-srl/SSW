Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports Climalombarda.Common

Public NotInheritable Class CLTechnicalSelectionRegistrationService

    Private ReadOnly m_Client As CLSelectionApiClient

    Public Sub New(Optional client As CLSelectionApiClient = Nothing)
        m_Client = If(client, New CLSelectionApiClient())
    End Sub

    Public Async Function RegisterAsync(
        document As CLSelectionProjectDocument,
        environment As CLEnvironment,
        Optional cancellationToken As CancellationToken = Nothing) As Task(Of CLSelectionRegistrationResult)

        If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
        If environment Is Nothing Then Throw New ArgumentNullException(NameOf(environment))
        If document.RevisionTracking Is Nothing OrElse
            document.RevisionTracking.Current Is Nothing OrElse
            String.IsNullOrWhiteSpace(document.RevisionTracking.Current.SnapshotHash) Then
            CLSelectionSnapshotService.Refresh(document)
        End If

        Dim context As CLSelectionRegistrationContext =
            CLSelectionRegistrationContext.FromEnvironment(environment)
        Dim result As CLSelectionRegistrationResult =
            Await m_Client.RegisterSelectionAsync(
                document, context, cancellationToken).ConfigureAwait(False)

        If result Is Nothing Then
            Throw New InvalidDataException(
                "The technical selection registration returned no result.")
        End If
        If Not String.Equals(
            result.SnapshotHash,
            document.RevisionTracking.Current.SnapshotHash,
            StringComparison.Ordinal) Then
            Throw New InvalidDataException(
                "The registration response does not match the current technical snapshot.")
        End If
        If String.IsNullOrWhiteSpace(result.ResumeToken) Then
            Throw New InvalidDataException(
                "The registration response does not contain the selection resume token.")
        End If

        CLSelectionSnapshotService.MarkRegistered(
            document,
            result.PublicReference,
            result.Revision,
            result.ResumeToken,
            DateTime.UtcNow)
        Return result
    End Function

End Class
