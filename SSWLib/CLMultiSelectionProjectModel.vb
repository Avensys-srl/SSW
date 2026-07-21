Imports System.Text.Json.Serialization

Public NotInheritable Class CLMultiSelectionProjectDocument
    Public Property Format As String = CLMultiSelectionProjectSerializer.ProjectFormat
    Public Property FormatVersion As Integer = CLMultiSelectionProjectSerializer.CurrentFormatVersion
    Public Property ProjectId As Guid = Guid.NewGuid()
    Public Property Reference As String
    Public Property LanguageCode As String
    Public Property CreatedAtUtc As DateTime = DateTime.UtcNow
    Public Property ModifiedAtUtc As DateTime = DateTime.UtcNow
    Public Property Items As New List(Of CLMultiSelectionProjectItem)()

    <JsonIgnore>
    Public Property SourceFilePath As String
End Class

Public NotInheritable Class CLMultiSelectionProjectItem
    Public Property ItemId As Guid = Guid.NewGuid()
    Public Property SelectionProjectId As Guid
    Public Property CustomerReference As String
    Public Property UnitName As String
    Public Property AirflowM3h As Double?
    Public Property PressurePa As Double?
    Public Property PdfFileName As String
    Public Property LanguageCode As String
    Public Property SnapshotHash As String
    Public Property AddedAtUtc As DateTime = DateTime.UtcNow
    Public Property ModifiedAtUtc As DateTime = DateTime.UtcNow

    <JsonIgnore>
    Public Property SelectionJson As String

    <JsonIgnore>
    Public Property PdfBytes As Byte()

    <JsonIgnore>
    Public ReadOnly Property HasValidPdf As Boolean
        Get
            Return PdfBytes IsNot Nothing AndAlso PdfBytes.Length >= 4 AndAlso
                PdfBytes(0) = &H25 AndAlso PdfBytes(1) = &H50 AndAlso
                PdfBytes(2) = &H44 AndAlso PdfBytes(3) = &H46 AndAlso
                Not String.IsNullOrWhiteSpace(PdfFileName)
        End Get
    End Property
End Class
