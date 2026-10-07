Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Collections.Generic
Imports System.Linq
Imports Climalombarda.DataCentral
Imports Climalombarda.DataCentral.LTModel

Public NotInheritable Class CLProductDocumentService
    Private Const CommercialSheetBaseUrl As String =
        "https://www.avensys-srl.com/ftproot/DOCUMENTS/Commercial_leaflets/1_VENTILATION_HEAT_RECOVERY/1_Heat_recovery_units/LEAFLETS"
    Private Const StepModelBaseUrl As String =
        "https://www.avensys-srl.com/ftproot/DOCUMENTS/Drawing"

    Private Sub New()
    End Sub

    Public Shared Function Resolve(
        document As CLSelectionProjectDocument,
        languageCode As String,
        shortName As String) As CLNextUiProductDocuments

        Return Resolve(
            document,
            languageCode,
            shortName,
            My.Settings.CommercialSheetAutoSyncEnabled)
    End Function

    Public Shared Function Resolve(
        document As CLSelectionProjectDocument,
        languageCode As String,
        shortName As String,
        autoSyncEnabled As Boolean) As CLNextUiProductDocuments

        If document Is Nothing OrElse
            document.Selection Is Nothing OrElse
            document.Selection.Unit Is Nothing Then
            Throw New ArgumentException(
                "The selection does not contain a valid unit.", NameOf(document))
        End If

        Dim modelCode = document.Selection.Unit.Code
        Dim model = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.ToList().
            FirstOrDefault(Function(item) String.Equals(
                item.Code, modelCode, StringComparison.OrdinalIgnoreCase))
        If model Is Nothing Then
            Throw New InvalidOperationException(
                "The selected unit is not available in the local SDF.")
        End If

        Dim modelName = document.Selection.Unit.Name
        If String.IsNullOrWhiteSpace(modelName) Then
            modelName = CLEnvironment.Current.GetCustomerHeatRecoveryModelName(model)
        End If

        Dim normalizedLanguage = NormalizeLanguageCode(languageCode)
        Return New CLNextUiProductDocuments With {
            .Brochures = If(String.Equals(shortName, "AV", StringComparison.OrdinalIgnoreCase),
                CLCommercialBrochureService.Resolve(If(model.CLSerie Is Nothing, "", model.CLSerie.Code),
                    If(model.CLEnumItem_AeraulicConnection Is Nothing, "", model.CLEnumItem_AeraulicConnection.TextCode), model.Size, normalizedLanguage),
                New List(Of CLCommercialBrochure)()),
            .CommercialSheetPath = ResolveCommercialSheet(
                model, modelName, normalizedLanguage, True,
                shortName, autoSyncEnabled),
            .InstallationManualPath = ResolveInstallationManual(
                model, normalizedLanguage, True, shortName),
            .ApplicationDocumentPath = ResolveApplicationDocument(
                model, normalizedLanguage, shortName),
            .StepModelPath = ResolveStepModel(model)
        }
    End Function

    Public Shared Function ResolveApplicationDocument(
        model As CLDCHeatRecoveryModel,
        languageCode As String,
        shortName As String,
        Optional fallbackToEnglish As Boolean = True,
        Optional online As Boolean = True) As String

        If model Is Nothing Then Return String.Empty
        Dim seriesCode = ReadProperty(model.CLSerie, "Code")
        If String.IsNullOrWhiteSpace(seriesCode) Then Return String.Empty

        If String.Equals(shortName, "AV", StringComparison.OrdinalIgnoreCase) Then
            Dim matched = CLApplicationDocumentService.Resolve(seriesCode,
                If(model.CLEnumItem_AeraulicConnection Is Nothing, "", model.CLEnumItem_AeraulicConnection.TextCode),
                model.Size, NormalizeLanguageCode(languageCode), online, fallbackToEnglish).FirstOrDefault()
            Return If(matched Is Nothing, String.Empty, matched.Path)
        End If

        Dim directoryPath = Path.Combine(
            PdfDocumentDirectory, "ApplicationDocuments",
            GetSeriesFolderName(seriesCode))
        Dim normalizedLanguage = NormalizeLanguageCode(languageCode)
        Dim localFile = BuildAndFindFile(
            Path.Combine(directoryPath, normalizedLanguage),
            model.Name, normalizedLanguage, shortName)
        If Not String.IsNullOrEmpty(localFile) Then Return localFile
        If Not fallbackToEnglish OrElse normalizedLanguage = "EN" Then
            Return String.Empty
        End If

        Return BuildAndFindFile(
            Path.Combine(directoryPath, "EN"), model.Name, "EN", shortName)
    End Function

    Public Shared Function BuildLocalCoverage(
        shortName As String) As List(Of CLProductDocumentCoverageRow)

        Dim rows As New List(Of CLProductDocumentCoverageRow)()
        If CLEnvironment.Current Is Nothing OrElse
            CLEnvironment.Current.DCContext Is Nothing Then
            Return rows
        End If

        Dim drawingVersions = CLDimensionalDrawingService.GetActiveDrawingCoverage()
        Dim stepDirectory = Path.Combine(PdfDocumentDirectory, "STEP")
        Dim models = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.ToList()
        For Each model In models.
            Where(Function(item) Not String.Equals(item.Code, "ACC", StringComparison.OrdinalIgnoreCase) AndAlso
                                 Not String.Equals(item.Code, "IOM3", StringComparison.OrdinalIgnoreCase)).
            OrderBy(Function(item) If(item.CLSerie Is Nothing, String.Empty, item.CLSerie.Code)).
            ThenBy(Function(item) item.Size).
            ThenBy(Function(item) item.Code)

            Dim seriesCode = If(model.CLSerie Is Nothing, String.Empty, model.CLSerie.Code)
            Dim seriesFolder = If(seriesCode = "32", "SA", "S" & seriesCode.Trim())
            Dim row As New CLProductDocumentCoverageRow With {
                .ModelCode = If(model.Code, String.Empty),
                .ModelName = CLEnvironment.Current.GetCustomerHeatRecoveryModelName(model),
                .SeriesCode = seriesCode,
                .ModelSize = model.Size,
                .AeraulicConnectionCode = If(model.CLEnumItem_AeraulicConnection Is Nothing, "", model.CLEnumItem_AeraulicConnection.TextCode)
            }
            For Each language In New String() {"BG", "CS", "DA", "DE", "EN", "FR", "HU", "IS", "IT", "NL", "NO", "PL", "RO", "SL", "SV"}
                If String.Equals(shortName, "AV", StringComparison.OrdinalIgnoreCase) Then
                    Dim brochureCandidates = CLCommercialBrochureService.Candidates(seriesCode,
                        row.AeraulicConnectionCode, model.Size, language, False, False)
                    row.BrochureOnlineCandidates(language) = brochureCandidates.Select(Function(item) item.Url).ToList()
                    row.BrochurePaths.AddRange(brochureCandidates.Where(Function(item) File.Exists(item.Path)).Select(Function(item) item.Path))
                End If
                If String.Equals(shortName, "AV", StringComparison.OrdinalIgnoreCase) Then
                    Dim candidates As New List(Of String)()
                    For Each candidateName In New String() {row.ModelName, model.Name}
                        Dim candidateFile = BuildExpectedFileName(candidateName, language, shortName)
                        If Not String.IsNullOrEmpty(candidateFile) Then
                            Dim candidateUrl = CommercialSheetBaseUrl & "/" & GetSeriesFolderName(seriesCode) & "/" & language & "/" & Uri.EscapeDataString(candidateFile)
                            If Not candidates.Contains(candidateUrl) Then candidates.Add(candidateUrl)
                        End If
                    Next
                    row.CommercialSheetOnlineCandidates(language) = candidates
                End If
                Dim commercialDirectory = Path.Combine(
                    PdfDocumentDirectory, seriesFolder, language)
                Dim commercialFile = BuildExpectedFileName(
                    row.ModelName, language, shortName)
                Dim commercialPath = If(
                    String.IsNullOrEmpty(commercialFile),
                    String.Empty,
                    Path.Combine(commercialDirectory, commercialFile))
                If Not File.Exists(commercialPath) Then
                    commercialPath = BuildAndFindFile(
                        commercialDirectory, model.Name, language, shortName)
                End If
                If Not String.IsNullOrEmpty(commercialPath) AndAlso
                    File.Exists(commercialPath) Then
                    row.CommercialSheetPaths.Add(commercialPath)
                End If

                If Directory.Exists(PdfDocumentDirectory) AndAlso
                    Not String.IsNullOrWhiteSpace(model.PDFInstallationOperationManuals) Then
                    Dim manualPattern = model.PDFInstallationOperationManuals.
                        Replace("%LanguageCode%", language).
                        Replace("%ShortName%", shortName)
                    If manualPattern.IndexOfAny(
                        New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}) < 0 AndAlso
                        Not manualPattern.Contains("..") Then
                        row.InstallationManualPaths.AddRange(
                            Directory.GetFiles(PdfDocumentDirectory, manualPattern))
                    End If
                End If

                Dim applicationFile = ResolveApplicationDocument(model, language, shortName, False, False)
                If Not String.IsNullOrWhiteSpace(applicationFile) Then
                    row.ApplicationDocumentPaths.Add(applicationFile)
                End If
            Next

            Dim modelId = Convert.ToString(model.Id, Globalization.CultureInfo.InvariantCulture)
            row.StepModelPath = Path.Combine(stepDirectory, modelId & "_stp.zip")
            row.StepModelOnlineUrl = StepModelBaseUrl & "/" & Uri.EscapeDataString(modelId) & "_stp.zip"
            If Not File.Exists(row.StepModelPath) Then row.StepModelPath = String.Empty
            Dim revisions As String = Nothing
            If drawingVersions.TryGetValue(model.Id, revisions) Then
                row.DimensionalDrawingVersions.AddRange(
                    revisions.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            End If
            rows.Add(row)
        Next
        Return rows
    End Function

    Private Shared Function FindDocumentLanguages() As List(Of String)
        Dim available As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim supported As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "BG", "CS", "DA", "DE", "EN", "FR", "HU", "IS", "IT", "NL", "NO", "PL", "RO", "SL", "SV"
        }
        If Directory.Exists(PdfDocumentDirectory) Then
            For Each directoryPath In Directory.GetDirectories(PdfDocumentDirectory, "*", SearchOption.AllDirectories)
                Dim language = Path.GetFileName(directoryPath)
                If supported.Contains(language) Then available.Add(language.ToUpperInvariant())
            Next
            For Each filePath In Directory.GetFiles(PdfDocumentDirectory, "IOM_*_AV.pdf")
                Dim tokens = Path.GetFileNameWithoutExtension(filePath).Split("_"c)
                If tokens.Length >= 4 AndAlso supported.Contains(tokens(tokens.Length - 2)) Then
                    available.Add(tokens(tokens.Length - 2).ToUpperInvariant())
                End If
            Next
        End If
        available.Add("EN")
        Return available.OrderBy(Function(item) item).ToList()
    End Function

    Public Shared Function ResolveStepModel(model As CLDCHeatRecoveryModel) As String
        If model Is Nothing Then Return String.Empty

        Dim modelId = ReadProperty(model, "ID")
        If String.IsNullOrWhiteSpace(modelId) Then
            modelId = ReadProperty(model, "Id")
        End If
        If String.IsNullOrWhiteSpace(modelId) Then
            Log("STEP lookup skipped: model ID is unavailable.")
            Return String.Empty
        End If

        Dim url = String.Format(
            "{0}/{1}_stp.zip",
            StepModelBaseUrl.TrimEnd("/"c),
            Uri.EscapeDataString(modelId.Trim()))
        Dim targetDirectory = Path.Combine(PdfDocumentDirectory, "STEP")
        Dim targetFilePath = Path.Combine(
            targetDirectory, modelId.Trim() & "_stp.zip")
        Dim temporaryFilePath = targetFilePath & ".download"
        Try
            Directory.CreateDirectory(targetDirectory)
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 Or
                SecurityProtocolType.Tls11 Or
                SecurityProtocolType.Tls
            Dim request = DirectCast(WebRequest.Create(url), HttpWebRequest)
            request.Method = "GET"
            request.Timeout = 10000
            request.ReadWriteTimeout = 30000
            request.AllowAutoRedirect = True
            If File.Exists(targetFilePath) Then
                request.IfModifiedSince = File.GetLastWriteTimeUtc(targetFilePath)
            End If
            Using response = DirectCast(request.GetResponse(), HttpWebResponse)
                Using source = response.GetResponseStream()
                    Using destination = File.Create(temporaryFilePath)
                        source.CopyTo(destination)
                    End Using
                End Using
            End Using
            File.Copy(temporaryFilePath, targetFilePath, True)
            Log("Downloaded/updated STEP file: " & targetFilePath)
            Return targetFilePath
        Catch exception As WebException
            Dim response = TryCast(exception.Response, HttpWebResponse)
            If response IsNot Nothing AndAlso
                response.StatusCode = HttpStatusCode.NotModified AndAlso
                File.Exists(targetFilePath) Then
                Log("STEP file unchanged, using local file: " & targetFilePath)
                Return targetFilePath
            End If
            Log(String.Format(
                "STEP lookup failed. URL='{0}' Error='{1}'",
                url, exception.Message))
        Catch exception As Exception
            Log(String.Format(
                "STEP lookup failed. URL='{0}' Error='{1}'",
                url, exception.Message))
        Finally
            Try
                If File.Exists(temporaryFilePath) Then File.Delete(temporaryFilePath)
            Catch
            End Try
        End Try
        Return If(File.Exists(targetFilePath), targetFilePath, String.Empty)
    End Function

    Public Shared Function ResolveInstallationManual(
        model As CLDCHeatRecoveryModel,
        languageCode As String,
        fallbackToEnglish As Boolean,
        shortName As String) As String

        If model Is Nothing OrElse
            String.IsNullOrEmpty(model.PDFInstallationOperationManuals) OrElse
            Not Directory.Exists(PdfDocumentDirectory) Then
            Return String.Empty
        End If

        Dim files = Directory.GetFiles(
            PdfDocumentDirectory,
            model.PDFInstallationOperationManuals.
                Replace("%LanguageCode%", languageCode).
                Replace("%ShortName%", shortName))
        If files.Length > 0 Then Return files(0)

        If fallbackToEnglish AndAlso
            Not String.Equals(languageCode, "EN", StringComparison.OrdinalIgnoreCase) Then
            Return ResolveInstallationManual(model, "EN", False, shortName)
        End If
        Return String.Empty
    End Function

    Public Shared Function ResolveCommercialSheet(
        model As CLDCHeatRecoveryModel,
        selectedModelName As String,
        languageCode As String,
        fallbackToEnglish As Boolean,
        shortName As String,
        autoSyncEnabled As Boolean) As String

        If model Is Nothing Then
            Log("Lookup skipped: model is Nothing.")
            Return String.Empty
        End If

        Dim seriesCode = ReadProperty(model.CLSerie, "Code")
        Dim seriesName = ReadProperty(model.CLSerie, "Name")
        Dim modelName = selectedModelName
        If String.IsNullOrWhiteSpace(modelName) Then
            modelName = ReadProperty(model, "Name")
        End If

        If (String.IsNullOrWhiteSpace(seriesCode) AndAlso
            String.IsNullOrWhiteSpace(seriesName)) OrElse
            String.IsNullOrWhiteSpace(modelName) Then
            Log(String.Format(
                "Lookup skipped: serieCode='{0}', serieName='{1}', modelName='{2}'",
                seriesCode, seriesName, modelName))
            Return String.Empty
        End If

        Dim normalizedLanguage = NormalizeLanguageCode(languageCode)
        Dim seriesDirectory = If(
            String.IsNullOrWhiteSpace(seriesCode),
            Path.Combine(PdfDocumentDirectory, "S" & seriesName.Trim()),
            Path.Combine(PdfDocumentDirectory, GetSeriesFolderName(seriesCode)))
        Dim languageDirectory = Path.Combine(seriesDirectory, normalizedLanguage)

        If Not String.IsNullOrWhiteSpace(seriesCode) Then
            Dim expectedFileName =
                BuildExpectedFileName(modelName, normalizedLanguage, shortName)
            If Not String.IsNullOrEmpty(expectedFileName) AndAlso
                String.Equals(shortName, "AV", StringComparison.OrdinalIgnoreCase) AndAlso
                autoSyncEnabled Then
                Dim onlineFile = GetOnlineFile(
                    seriesCode.Trim(), normalizedLanguage, expectedFileName, shortName)
                If Not String.IsNullOrEmpty(onlineFile) Then
                    Log("FOUND ONLINE (preferred for AV): " & onlineFile)
                    Return onlineFile
                End If
            End If
        End If

        Dim localFile = BuildAndFindFile(
            languageDirectory, modelName, normalizedLanguage, shortName)
        If Not String.IsNullOrEmpty(localFile) Then
            Log("FOUND LOCAL: " & localFile)
            Return localFile
        End If

        If fallbackToEnglish AndAlso
            Not String.Equals(normalizedLanguage, "EN", StringComparison.OrdinalIgnoreCase) Then
            Log(String.Format(
                "Fallback to EN from language '{0}'", normalizedLanguage))
            Return ResolveCommercialSheet(
                model, modelName, "EN", False, shortName, autoSyncEnabled)
        End If

        Log("NOT FOUND for current lookup.")
        Return String.Empty
    End Function

    Private Shared Function ReadProperty(instance As Object, propertyName As String) As String
        If instance Is Nothing Then Return String.Empty
        Try
            Return Convert.ToString(
                CallByName(instance, propertyName, CallType.Get),
                Globalization.CultureInfo.InvariantCulture)
        Catch
            Return String.Empty
        End Try
    End Function

    Private Shared Function NormalizeLanguageCode(languageCode As String) As String
        Dim normalized = If(languageCode, String.Empty).Trim().ToUpperInvariant()
        If normalized.Contains("-"c) Then normalized = normalized.Split("-"c)(0)
        If String.IsNullOrWhiteSpace(normalized) Then normalized = "EN"
        Return normalized
    End Function

    Private Shared Function GetSeriesFolderName(seriesCode As String) As String
        Dim normalized = If(seriesCode, String.Empty).Trim().ToUpperInvariant()
        If normalized = "32" Then Return "SA"
        Return "S" & normalized
    End Function

    Private Shared Function BuildExpectedFileName(
        modelName As String,
        languageCode As String,
        shortName As String) As String

        Dim cleanedModelName = modelName.Replace(ChrW(160), " "c)
        Dim tokens = cleanedModelName.Split(
            New Char() {" "c, ControlChars.Tab},
            StringSplitOptions.RemoveEmptyEntries)
        If tokens.Length >= 3 Then
            Return String.Format(
                "{0}_{1}_{2}_{3}.pdf",
                tokens(2).ToUpperInvariant(),
                tokens(1).ToUpperInvariant(),
                languageCode.ToUpperInvariant(),
                shortName.ToUpperInvariant())
        End If
        If tokens.Length = 2 Then
            Return String.Format(
                "{0}_{1}_{2}.pdf",
                tokens(1).ToUpperInvariant(),
                languageCode.ToUpperInvariant(),
                shortName.ToUpperInvariant())
        End If
        Log(String.Format("Model name tokens < 2: modelName='{0}'", modelName))
        Return String.Empty
    End Function

    Private Shared Function BuildAndFindFile(
        directoryPath As String,
        modelName As String,
        languageCode As String,
        shortName As String) As String

        If String.IsNullOrWhiteSpace(directoryPath) OrElse
            String.IsNullOrWhiteSpace(modelName) OrElse
            Not Directory.Exists(directoryPath) Then
            Log(String.Format("Directory missing or invalid: '{0}'", directoryPath))
            Return String.Empty
        End If

        Dim fileName = BuildExpectedFileName(modelName, languageCode, shortName)
        If String.IsNullOrEmpty(fileName) Then Return String.Empty

        Dim fullPath = Path.Combine(directoryPath, fileName)
        Log(String.Format(
            "Trying: model='{0}', serieDir='{1}', language='{2}', shortname='{3}', fullPath='{4}'",
            modelName, directoryPath, languageCode, shortName, fullPath))
        Return If(File.Exists(fullPath), fullPath, String.Empty)
    End Function

    Private Shared Function GetOnlineFile(
        seriesCode As String,
        languageCode As String,
        fileName As String,
        shortName As String) As String

        If Not String.Equals(shortName, "AV", StringComparison.OrdinalIgnoreCase) Then
            Log(String.Format(
                "No online base URL configured for shortname '{0}'", shortName))
            Return String.Empty
        End If

        Dim seriesFolder = GetSeriesFolderName(seriesCode)
        Dim languageFolder = languageCode.Trim().ToUpperInvariant()
        Dim url = String.Format(
            "{0}/{1}/{2}/{3}",
            CommercialSheetBaseUrl.TrimEnd("/"c),
            seriesFolder,
            languageFolder,
            fileName)
        Dim targetDirectory =
            Path.Combine(PdfDocumentDirectory, seriesFolder, languageFolder)
        Dim targetFilePath = Path.Combine(targetDirectory, fileName)
        Dim temporaryFilePath = targetFilePath & ".download"

        Try
            Directory.CreateDirectory(targetDirectory)
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 Or
                SecurityProtocolType.Tls11 Or
                SecurityProtocolType.Tls
            Dim request = DirectCast(WebRequest.Create(url), HttpWebRequest)
            request.Method = "GET"
            request.Timeout = 10000
            request.ReadWriteTimeout = 20000
            If File.Exists(targetFilePath) Then
                request.IfModifiedSince = File.GetLastWriteTimeUtc(targetFilePath)
            End If

            Log("Trying conditional online URL: " & url)
            Using response = DirectCast(request.GetResponse(), HttpWebResponse)
                Using source = response.GetResponseStream()
                    Using destination = File.Create(temporaryFilePath)
                        source.CopyTo(destination)
                    End Using
                End Using
            End Using
            File.Copy(temporaryFilePath, targetFilePath, True)
            Log("Downloaded/updated local css file: " & targetFilePath)
            Return targetFilePath
        Catch exception As WebException
            Dim response = TryCast(exception.Response, HttpWebResponse)
            If response IsNot Nothing AndAlso
                response.StatusCode = HttpStatusCode.NotModified AndAlso
                File.Exists(targetFilePath) Then
                Log("Online file unchanged, using local css file: " & targetFilePath)
                Return targetFilePath
            End If
            Log(String.Format(
                "Online download failed. URL='{0}' Error='{1}'",
                url, exception.Message))
            Return If(File.Exists(targetFilePath), targetFilePath, String.Empty)
        Catch exception As Exception
            Log(String.Format(
                "Online download failed. URL='{0}' Error='{1}'",
                url, exception.Message))
            Return If(File.Exists(targetFilePath), targetFilePath, String.Empty)
        Finally
            Try
                If File.Exists(temporaryFilePath) Then File.Delete(temporaryFilePath)
            Catch
            End Try
        End Try
    End Function

    Private Shared Sub Log(message As String)
        Try
            Dim line = String.Format(
                "{0:yyyy-MM-dd HH:mm:ss.fff} | {1}", DateTime.Now, message)
            Debug.WriteLine(line)
            File.AppendAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commercialsheet_lookup.log"),
                line & Environment.NewLine)
        Catch
        End Try
    End Sub

    Private Shared ReadOnly Property PdfDocumentDirectory As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css")
        End Get
    End Property
End Class
