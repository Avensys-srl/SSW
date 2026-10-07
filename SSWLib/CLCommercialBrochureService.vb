Imports System.IO
Imports System.Net
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Linq

Public NotInheritable Class CLCommercialBrochureService
    Public Const BaseUrl As String = "https://www.avensys-srl.com/ftproot/DOCUMENTS/Commercial_leaflets/1_VENTILATION_HEAT_RECOVERY/1_Heat_recovery_units/"
    Private Shared ReadOnly Gate As New Object()
    Private Shared cachedCatalog As BrochureCatalog
    Private Shared ReadOnly pdfChecks As New Dictionary(Of String, DateTime)()

    Public Class BrochureCatalog
        Public Property schema_version As Integer
        Public Property documents As New List(Of BrochureDefinition)()
    End Class
    Public Class BrochureDefinition
        Public Property id As String
        Public Property order As Integer
        Public Property filename_template As String
        Public Property rules As New List(Of BrochureRule)()
    End Class
    Public Class BrochureRule
        Public Property series As New List(Of String)()
        Public Property configurations As New List(Of String)()
        Public Property size_min As Integer?
        Public Property size_max As Integer?
    End Class

    Public Shared ReadOnly Property CacheDirectory As String
        Get
            Dim isolated = Environment.GetEnvironmentVariable("SSW_BROCHURE_CACHE_DIRECTORY")
            If Not String.IsNullOrWhiteSpace(isolated) Then Return IO.Path.GetFullPath(isolated)
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Avensys", "SSW", "Brochures")
        End Get
    End Property

    Public Shared Function ParseCatalog(json As String) As BrochureCatalog
        Dim catalog = JsonSerializer.Deserialize(Of BrochureCatalog)(json)
        If catalog Is Nothing OrElse catalog.schema_version <> 1 OrElse catalog.documents Is Nothing Then
            Throw New InvalidDataException("Unsupported brochure catalog.")
        End If
        Return catalog
    End Function

    Public Shared Function CandidateFiles(catalog As BrochureCatalog, seriesCode As String, configuration As String,
                                         size As Integer?, language As String, Optional fallback As Boolean = True) As List(Of CLCommercialBrochure)
        Return CandidateFilesCore(catalog, seriesCode, configuration, size, language, fallback,
            "\ACOMMERCIAL/[A-Z]{2}/[A-Za-z0-9_-]+\.pdf\z", CacheDirectory)
    End Function

    Friend Shared Function CandidateFilesCore(catalog As BrochureCatalog, seriesCode As String, configuration As String,
                                             size As Integer?, language As String, fallback As Boolean,
                                             pathPattern As String, cacheRoot As String) As List(Of CLCommercialBrochure)
        Dim result As New List(Of CLCommercialBrochure)()
        seriesCode = If(seriesCode, "").Trim()
        Dim folder = If(seriesCode = "32", "SA", If(seriesCode.StartsWith("S", StringComparison.OrdinalIgnoreCase), seriesCode, "S" & seriesCode)).ToUpperInvariant()
        Dim lang = If(language, "EN").Trim().Replace("_", "-").Split("-"c)(0).ToUpperInvariant()
        If Not Regex.IsMatch(lang, "^[A-Z]{2}$") Then lang = "EN"
        Dim languages = If(fallback, New String() {lang, "EN"}, New String() {lang}).Distinct()
        For Each doc In catalog.documents.Where(Function(item) item IsNot Nothing).OrderBy(Function(item) item.order)
            If doc Is Nothing OrElse String.IsNullOrWhiteSpace(doc.id) OrElse doc.rules Is Nothing OrElse doc.filename_template Is Nothing Then Continue For
            Dim matches = doc.rules.Any(Function(rule) rule IsNot Nothing AndAlso rule.series IsNot Nothing AndAlso
                rule.series.Contains(folder) AndAlso
                (rule.configurations Is Nothing OrElse rule.configurations.Count = 0 OrElse rule.configurations.Contains(If(configuration, "").ToUpperInvariant())) AndAlso
                (Not rule.size_min.HasValue OrElse (size.HasValue AndAlso size.Value >= rule.size_min.Value)) AndAlso
                (Not rule.size_max.HasValue OrElse (size.HasValue AndAlso size.Value <= rule.size_max.Value)))
            If Not matches Then Continue For
            For Each candidateLanguage In languages
                Dim relative = doc.filename_template.Replace("{lang}", candidateLanguage)
                If Not Regex.IsMatch(relative, pathPattern) Then Continue For
                result.Add(New CLCommercialBrochure With {.Id = doc.id, .Language = candidateLanguage,
                    .RelativePath = relative, .Url = BaseUrl & relative, .Path = Path.Combine(cacheRoot, relative.Replace("/"c, IO.Path.DirectorySeparatorChar))})
            Next
        Next
        Return result
    End Function

    Private Shared Function Catalog(Optional online As Boolean = True) As BrochureCatalog
        SyncLock Gate
            If cachedCatalog IsNot Nothing Then Return cachedCatalog
            Dim path = IO.Path.Combine(CacheDirectory, "brochures.json")
            If online Then
                Try
                    Dim bytes = Fetch(BaseUrl & "brochures.json", 1024 * 1024)
                    Dim parsed = ParseCatalog(System.Text.Encoding.UTF8.GetString(bytes))
                    SaveCache(path, bytes)
                    cachedCatalog = parsed
                    Return parsed
                Catch ex As Exception
                    Diagnostics.Trace.TraceWarning("Brochure catalog: " & ex.Message)
                End Try
            End If
            For Each candidate In {path, IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css", "Brochures", "brochures.json")}
                Try
                    If File.Exists(candidate) Then
                        Dim parsed = ParseCatalog(File.ReadAllText(candidate))
                        If online Then cachedCatalog = parsed
                        Return parsed
                    End If
                Catch ex As Exception
                    Diagnostics.Trace.TraceWarning("Brochure catalog cache: " & ex.Message)
                End Try
            Next
            Dim empty As New BrochureCatalog With {.schema_version = 1}
            If online Then cachedCatalog = empty
            Return empty
        End SyncLock
    End Function

    Public Shared Function Candidates(seriesCode As String, configuration As String, size As Integer?, language As String,
                                     Optional onlineCatalog As Boolean = True, Optional fallback As Boolean = True) As List(Of CLCommercialBrochure)
        Return CandidateFiles(Catalog(onlineCatalog), seriesCode, configuration, size, language, fallback)
    End Function

    Public Shared Function Resolve(seriesCode As String, configuration As String, size As Integer?, language As String,
                                   Optional online As Boolean = True) As List(Of CLCommercialBrochure)
        Dim found As New List(Of CLCommercialBrochure)()
        SyncLock Gate
            For Each group In Candidates(seriesCode, configuration, size, language, online).GroupBy(Function(item) item.Id)
                For Each candidate In group
                    Dim checkedAt As DateTime
                    If online AndAlso (Not pdfChecks.TryGetValue(candidate.Url, checkedAt) OrElse DateTime.UtcNow - checkedAt > TimeSpan.FromMinutes(5)) Then
                        pdfChecks(candidate.Url) = DateTime.UtcNow
                        Try
                            Dim bytes = Fetch(candidate.Url, 25 * 1024 * 1024)
                            If bytes.Length < 5 OrElse System.Text.Encoding.ASCII.GetString(bytes, 0, 5) <> "%PDF-" Then Throw New InvalidDataException("Not a PDF.")
                            SaveCache(candidate.Path, bytes)
                        Catch ex As Exception
                            Diagnostics.Trace.TraceWarning("Brochure PDF: " & ex.Message)
                        End Try
                    End If
                    If File.Exists(candidate.Path) Then
                        found.Add(candidate)
                        Exit For
                    End If
                Next
            Next
        End SyncLock
        Return found
    End Function

    Friend Shared Function Fetch(url As String, maxBytes As Integer) As Byte()
        ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol Or SecurityProtocolType.Tls12
        Dim request = DirectCast(WebRequest.Create(url), HttpWebRequest)
        request.Timeout = 4000
        request.ReadWriteTimeout = 4000
        Using response = request.GetResponse(), source = response.GetResponseStream(), buffer As New MemoryStream()
            Dim block(8191) As Byte
            While True
                Dim count = source.Read(block, 0, block.Length)
                If count = 0 Then Exit While
                If buffer.Length + count > maxBytes Then Throw New InvalidDataException("Brochure response exceeds size limit.")
                buffer.Write(block, 0, count)
            End While
            Return buffer.ToArray()
        End Using
    End Function

    Friend Shared Sub SaveCache(path As String, bytes As Byte())
        Directory.CreateDirectory(IO.Path.GetDirectoryName(path))
        Dim temporary = path & "." & Guid.NewGuid().ToString("N") & ".download"
        Try
            File.WriteAllBytes(temporary, bytes)
            If File.Exists(path) Then
                File.Replace(temporary, path, Nothing)
            Else
                File.Move(temporary, path)
            End If
        Finally
            If File.Exists(temporary) Then File.Delete(temporary)
        End Try
    End Sub
End Class

Public Class CLCommercialBrochure
    Public Property Id As String
    Public Property Language As String
    Public Property Path As String
    Public Property RelativePath As String
    Public Property Url As String
End Class
