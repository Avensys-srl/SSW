Imports System.IO
Imports System.Linq

Public NotInheritable Class CLApplicationDocumentService
    Private Shared ReadOnly Gate As New Object()
    Private Shared catalog As CLCommercialBrochureService.BrochureCatalog
    Private Shared ReadOnly checks As New Dictionary(Of String, DateTime)()

    Public Shared ReadOnly Property CacheDirectory As String
        Get
            Dim isolated = Environment.GetEnvironmentVariable("SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY")
            Return If(String.IsNullOrWhiteSpace(isolated), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Avensys", "SSW", "ApplicationDocuments"), Path.GetFullPath(isolated))
        End Get
    End Property

    Private Shared Function ReadCatalog(online As Boolean) As CLCommercialBrochureService.BrochureCatalog
        If online AndAlso catalog IsNot Nothing Then Return catalog
        Dim cachedPath = Path.Combine(CacheDirectory, "application-documents.json")
        If online Then
            Try
                Dim bytes = CLCommercialBrochureService.Fetch(CLCommercialBrochureService.BaseUrl & "application-documents.json", 1024 * 1024)
                Dim parsed = CLCommercialBrochureService.ParseCatalog(System.Text.Encoding.UTF8.GetString(bytes))
                CLCommercialBrochureService.SaveCache(cachedPath, bytes)
                catalog = parsed
                Return parsed
            Catch ex As Exception
                Diagnostics.Trace.TraceWarning("Application document catalog: " & ex.Message)
            End Try
        End If
        For Each candidate In {cachedPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css", "ApplicationDocuments", "application-documents.json")}
            Try
                If File.Exists(candidate) Then
                    Dim parsed = CLCommercialBrochureService.ParseCatalog(File.ReadAllText(candidate))
                    If online Then catalog = parsed
                    Return parsed
                End If
            Catch ex As Exception
                Diagnostics.Trace.TraceWarning("Application document catalog cache: " & ex.Message)
            End Try
        Next
        Return New CLCommercialBrochureService.BrochureCatalog With {.schema_version = 1}
    End Function

    Public Shared Function CandidateFiles(definition As CLCommercialBrochureService.BrochureCatalog, seriesCode As String,
                                         configuration As String, size As Integer?, language As String,
                                         Optional fallback As Boolean = True) As List(Of CLCommercialBrochure)
        Return CLCommercialBrochureService.CandidateFilesCore(definition, seriesCode, configuration, size, language, fallback,
            "\AApplicationDocuments/[A-Z0-9]+/[A-Z]{2}/[A-Za-z0-9_-]+\.pdf\z", CacheDirectory)
    End Function

    Public Shared Function Resolve(seriesCode As String, configuration As String, size As Integer?, language As String,
                                   Optional online As Boolean = True, Optional fallback As Boolean = True) As List(Of CLCommercialBrochure)
        Dim result As New List(Of CLCommercialBrochure)()
        SyncLock Gate
            For Each group In CandidateFiles(ReadCatalog(online), seriesCode, configuration, size, language, fallback).GroupBy(Function(item) item.Id)
                For Each candidate In group
                    Dim lastCheck As DateTime
                    If online AndAlso (Not checks.TryGetValue(candidate.Url, lastCheck) OrElse DateTime.UtcNow - lastCheck > TimeSpan.FromMinutes(5)) Then
                        checks(candidate.Url) = DateTime.UtcNow
                        Try
                            Dim bytes = CLCommercialBrochureService.Fetch(candidate.Url, 25 * 1024 * 1024)
                            If bytes.Length < 5 OrElse System.Text.Encoding.ASCII.GetString(bytes, 0, 5) <> "%PDF-" Then Throw New InvalidDataException("Not a PDF.")
                            CLCommercialBrochureService.SaveCache(candidate.Path, bytes)
                        Catch ex As Exception
                            Diagnostics.Trace.TraceWarning("Application document PDF: " & ex.Message)
                        End Try
                    End If
                    If Not File.Exists(candidate.Path) Then
                        Dim bundled = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css", candidate.RelativePath.Replace("/"c, Path.DirectorySeparatorChar))
                        If File.Exists(bundled) Then candidate.Path = bundled
                    End If
                    If File.Exists(candidate.Path) Then
                        result.Add(candidate)
                        Exit For
                    End If
                Next
            Next
        End SyncLock
        Return result
    End Function
End Class
