Imports Microsoft.Reporting.WinForms

Public NotInheritable Class CLPreparedNextUiReport
    Public Property DataSources As New List(Of ReportDataSource)()
    Public Property ReportTemplate As String
End Class

Public NotInheritable Class CLNextUiProductDocuments
    Public Property CommercialSheetPath As String
    Public Property InstallationManualPath As String
    Public Property StepModelPath As String
End Class
