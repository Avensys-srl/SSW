Imports Microsoft.Reporting.WinForms

Public NotInheritable Class CLPreparedNextUiReport
    Public Property DataSources As New List(Of ReportDataSource)()
    Public Property ReportTemplate As String
End Class

Public NotInheritable Class CLNextUiProductDocuments
    Public Property CommercialSheetPath As String
    Public Property Brochures As New List(Of CLCommercialBrochure)()
    Public Property InstallationManualPath As String
    Public Property ApplicationDocumentPath As String
    Public Property StepModelPath As String
End Class

Public NotInheritable Class CLProductDocumentCoverageRow
    Public Property ModelCode As String
    Public Property ModelName As String
    Public Property SeriesCode As String
    Public Property ModelSize As Integer?
    Public Property AeraulicConnectionCode As String
    Public Property CommercialSheetPaths As New List(Of String)()
    Public Property CommercialSheetOnlineCandidates As New Dictionary(Of String, List(Of String))()
    Public Property CommercialSheetOnlineStatus As New Dictionary(Of String, String)()
    Public Property BrochurePaths As New List(Of String)()
    Public Property BrochureOnlineCandidates As New Dictionary(Of String, List(Of String))()
    Public Property BrochureOnlineStatus As New Dictionary(Of String, String)()
    Public Property InstallationManualPaths As New List(Of String)()
    Public Property ApplicationDocumentPaths As New List(Of String)()
    Public Property DimensionalDrawingVersions As New List(Of String)()
    Public Property StepModelPath As String
    Public Property StepModelOnlineUrl As String
    Public Property StepModelOnlineStatus As String
End Class
