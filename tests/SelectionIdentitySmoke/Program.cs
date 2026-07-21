using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using Microsoft.Reporting.WinForms;
using SSW;

internal sealed class TokenHandler : HttpMessageHandler
{
    public int RegistrationCount { get; private set; }
    public int RenewalCount { get; private set; }
    public int SelectionCreateCount { get; private set; }
    public int SelectionRevisionCount { get; private set; }
    public bool LastRegistrationHadBootstrap { get; private set; }
    public List<string> IdempotencyKeys { get; } = new List<string>();
    private string latestSnapshotHash;
    private string latestResumeToken;
    private int latestRevision;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token;
        if (request.RequestUri.AbsolutePath.EndsWith("/installations/register", StringComparison.Ordinal))
        {
            RegistrationCount++;
            LastRegistrationHadBootstrap = request.Headers.Contains("X-SSW-Bootstrap-Key");
            token = "registered-token-never-plaintext";
        }
        else if (request.RequestUri.AbsolutePath.EndsWith("/installations/token/renew", StringComparison.Ordinal))
        {
            RenewalCount++;
            token = "renewed-token-never-plaintext";
        }
        else if (request.RequestUri.AbsolutePath.EndsWith("/selections", StringComparison.Ordinal))
        {
            SelectionCreateCount++;
            VerifySelectionHeaders(request);
            string body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!body.Contains("\"project_id\"") || !body.Contains("\"selection\"") ||
                !body.Contains("\"versions\"") || !body.Contains("\"fingerprints\"") ||
                !body.Contains("\"Accessories\"") || !body.Contains("\"KTS EXTRA\"") ||
                !body.Contains("\"LocalizedDisplayName\""))
                throw new InvalidOperationException("Create-selection payload is incomplete.");
            latestSnapshotHash = JsonString(body, "snapshot_hash");
            latestResumeToken = JsonString(body, "resume_token");
            if (latestResumeToken.Length < 40) throw new InvalidOperationException("Client resume token is too weak.");
            latestRevision = 1;
            return JsonResponse(HttpStatusCode.Created,
                "{\"reference\":\"4827-1936-5048-2715-R01\",\"reference_digits\":\"4827193650482715\","
                + "\"revision\":1,\"resume_token\":\"" + latestResumeToken + "\","
                + "\"snapshot_hash\":\"" + latestSnapshotHash + "\",\"change_kind\":\"NewSelection\"}");
        }
        else if (request.RequestUri.AbsolutePath.EndsWith("/revisions", StringComparison.Ordinal))
        {
            SelectionRevisionCount++;
            VerifySelectionHeaders(request);
            string body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (JsonString(body, "resume_token") != latestResumeToken)
                throw new InvalidOperationException("Resume token missing from revision payload.");
            string snapshotHash = JsonString(body, "snapshot_hash");
            bool reprint = snapshotHash == latestSnapshotHash;
            if (!reprint)
            {
                latestSnapshotHash = snapshotHash;
                latestRevision++;
            }
            string displayedRevision = latestRevision.ToString("00");
            return JsonResponse(reprint ? HttpStatusCode.OK : HttpStatusCode.Created,
                "{\"reference\":\"4827-1936-5048-2715-R" + displayedRevision
                + "\",\"reference_digits\":\"4827193650482715\",\"revision\":" + latestRevision
                + ",\"snapshot_hash\":\"" + snapshotHash + "\",\"change_kind\":\""
                + (reprint ? "Reprint" : "TechnicalChange") + "\"}");
        }
        else
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        string json = "{\"access_token\":\"" + token + "\",\"expires_at\":\"2027-07-14T12:00:00Z\"}";
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    private void VerifySelectionHeaders(HttpRequestMessage request)
    {
        if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
            throw new InvalidOperationException("Selection bearer token missing.");
        if (!request.Headers.Contains("Idempotency-Key"))
            throw new InvalidOperationException("Selection idempotency key missing.");
        IdempotencyKeys.Add(String.Join("", request.Headers.GetValues("Idempotency-Key")));
    }

    private static Task<HttpResponseMessage> JsonResponse(HttpStatusCode status, string json)
    {
        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    private static string JsonString(string json, string propertyName)
    {
        Match match = Regex.Match(json, "\\\"" + Regex.Escape(propertyName) + "\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"");
        if (!match.Success) throw new InvalidOperationException("JSON property missing: " + propertyName);
        return match.Groups[1].Value;
    }
}

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(argument => String.Equals(argument, "--live-bootstrap", StringComparison.OrdinalIgnoreCase)))
            return RunLiveBootstrap();

        string root = Path.Combine(Path.GetTempPath(), "ssw-selection-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Environment.SetEnvironmentVariable("SSW_SELECTION_CREDENTIAL_PATH", Path.Combine(root, "credentials.json"));
        Environment.SetEnvironmentVariable("SSW_SELECTION_STATE_PATH", Path.Combine(root, "draft-state.json"));
        Environment.SetEnvironmentVariable("SSW_SELECTION_BOOTSTRAP_KEY_AV", "test-bootstrap-key");
        Environment.SetEnvironmentVariable("SSW_SELECTION_API_BASE_URL", "https://localhost/api/v1/");
        try
        {
            var handler = new TokenHandler();
            var client = new CLSelectionApiClient(new HttpClient(handler));
            var context = new CLSelectionRegistrationContext
            {
                CustomerCode = "AV",
                SoftwareVersion = "1.3.0.44",
                DatabaseSchemaVersion = 1,
                DatabaseContentHash = "A280D8C",
                ApiContractVersion = 1
            };

            string first = client.EnsureAccessTokenAsync(context).GetAwaiter().GetResult();
            string cached = client.EnsureAccessTokenAsync(context).GetAwaiter().GetResult();
            if (first != cached || handler.RegistrationCount != 1) throw new InvalidOperationException("Registration cache failed.");
            if (!handler.LastRegistrationHadBootstrap) throw new InvalidOperationException("Provisioned bootstrap header was not sent.");
            string stored = File.ReadAllText(CLSelectionCredentialStore.StateFilePath);
            if (stored.Contains(first)) throw new InvalidOperationException("Access token was stored in plaintext.");

            CLSelectionCredentialStore.SaveAccessToken(first, DateTime.UtcNow.AddDays(1));
            string renewed = client.EnsureAccessTokenAsync(context).GetAwaiter().GetResult();
            if (renewed != "renewed-token-never-plaintext" || handler.RenewalCount != 1) throw new InvalidOperationException("Token renewal failed.");
            if (File.ReadAllText(CLSelectionCredentialStore.StateFilePath).Contains(renewed)) throw new InvalidOperationException("Renewed token was stored in plaintext.");

            TestSelectionRegistrationClient(client, handler, context);
            TestSnapshotFingerprints(root);
            TestSdfFixtures(root);
            TestAccessoryLocalization();
            TestAccessoryReportTemplates();
            TestKtsExclusiveGroupReplacement();
            TestRegulationLevelControlSynchronization();
            TestReportEmailFeature();
            TestRegistrationFailureDialog();
            TestUpdateIntegrity(root);
            TestPracticalSelectionRules();
            TestRegistryBootstrapProvisioning();
            TestBootstraplessRegistration(root);

            Console.WriteLine("Selection identity/snapshot smoke test passed: token=ok create=R01 reprint=R01 revise=R02 fingerprints=stable dialog=rendered update_integrity=ok practical_rules=ok registry_bootstrap=ok public_enrollment=ok");
            return 0;
        }
        finally
        {
            Environment.SetEnvironmentVariable("SSW_SELECTION_CREDENTIAL_PATH", null);
            Environment.SetEnvironmentVariable("SSW_SELECTION_STATE_PATH", null);
            Environment.SetEnvironmentVariable("SSW_SELECTION_BOOTSTRAP_KEY_AV", null);
            Environment.SetEnvironmentVariable("SSW_SELECTION_API_BASE_URL", null);
            try { Directory.Delete(root, true); } catch { }
        }
    }

    private static void TestBootstraplessRegistration(string root)
    {
        string originalCredentialPath = Environment.GetEnvironmentVariable("SSW_SELECTION_CREDENTIAL_PATH");
        try
        {
            Environment.SetEnvironmentVariable("SSW_SELECTION_CREDENTIAL_PATH",
                Path.Combine(root, "public-enrollment-credentials.json"));
            var handler = new TokenHandler();
            var client = new CLSelectionApiClient(new HttpClient(handler));
            var context = new CLSelectionRegistrationContext
            {
                CustomerCode = "PUBLICENROLLMENTTEST",
                SoftwareVersion = "1.3.0.50",
                DatabaseSchemaVersion = 1,
                DatabaseContentHash = "public-enrollment-test",
                ApiContractVersion = 1
            };
            string token = client.EnsureAccessTokenAsync(context).GetAwaiter().GetResult();
            if (String.IsNullOrWhiteSpace(token) || handler.RegistrationCount != 1 ||
                handler.LastRegistrationHadBootstrap)
                throw new InvalidOperationException("Bootstrapless installation registration failed.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SSW_SELECTION_CREDENTIAL_PATH", originalCredentialPath);
        }
    }

    private static void TestAccessoryLocalization()
    {
        string repositoryRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        string[] languages = { "bg", "da", "de", "en", "fr", "hu", "it", "nl", "pl", "ro", "sl", "sv" };
        string[] keys =
        {
            "MainForm_Accessories_Tab", "MainForm_Accessories_AllCategories",
            "MainForm_Accessories_RequiresExtraController", "MainForm_Accessories_EnableFirst",
            "MainForm_Accessories_ConflictsWith", "MainForm_Accessories_ControllerLevel",
            "MainForm_Accessories_RequiredBy", "MainForm_Accessories_Selected",
            "MainForm_Accessories_Summary",
            "MainForm_Accessories_Search", "MainForm_Accessories_Category",
            "MainForm_Accessories_Code", "MainForm_Accessories_Description",
            "MainForm_Accessories_Functions", "MainForm_Accessories_Status",
            "MainForm_Accessories_Standard", "MainForm_Accessories_Optional",
            "MainForm_Accessories_CWDDescription", "MainForm_Accessories_HWDDescription",
            "MainForm_Accessories_HCDDescription", "MainForm_Accessories_EHDDescription",
            "MainForm_Accessories_PEHDDescription",
            "MainForm_CoilPerformance_ExternalInstallation"
        };

        foreach (string language in languages)
        {
            var document = new XmlDocument();
            document.Load(Path.Combine(repositoryRoot, "SSWLib", "Resources." + language + ".resx"));
            foreach (string key in keys)
            {
                XmlNodeList nodes = document.SelectNodes("/root/data[@name='" + key + "']/value");
                if (nodes == null || nodes.Count != 1 || String.IsNullOrWhiteSpace(nodes[0].InnerText) ||
                    nodes[0].InnerText == "?")
                    throw new InvalidOperationException("Accessory localization is missing or invalid: " + language + "/" + key);
            }
            if (language == "it" && document.SelectSingleNode(
                "/root/data[@name='MainForm_CoilPerformance_ExternalInstallation']/value").InnerText != "Esterna")
                throw new InvalidOperationException("Italian external-installation localization is invalid.");
            if (language == "it" && document.SelectSingleNode(
                "/root/data[@name='MainForm_Accessories_HCDDescription']/value").InnerText !=
                "Batteria ad acqua a 2 tubi per riscaldamento e raffreddamento")
                throw new InvalidOperationException("Italian HCD description localization is invalid.");
        }
    }

    private static void TestKtsExclusiveGroupReplacement()
    {
        var extra = new CLSelectionCatalogItem { Id = 1, Code = "KTS EXTRA", ExclusiveGroupCode = "KTS", Availability = "Optional" };
        var wifi = new CLSelectionCatalogItem { Id = 2, Code = "KTS WIFI", ExclusiveGroupCode = "KTS", Availability = "Optional" };
        var sma = new CLSelectionCatalogItem { Id = 3, Code = "SMA", Availability = "Optional" };
        sma.Dependencies.Add(new CLSelectionDependencyRule { DependencyType = "Requires", TargetItemId = wifi.Id });
        var dependent = new CLSelectionCatalogItem { Id = 4, Code = "SMA CHILD", Availability = "Optional" };
        dependent.Dependencies.Add(new CLSelectionDependencyRule { DependencyType = "Includes", TargetItemId = sma.Id });
        var unrelated = new CLSelectionCatalogItem { Id = 5, Code = "DPC", Availability = "Optional" };
        var items = new List<CLSelectionCatalogItem> { extra, wifi, sma, dependent, unrelated };
        var selected = new HashSet<int> { wifi.Id, sma.Id, dependent.Id, unrelated.Id };

        MethodInfo method = typeof(CLMainForm).GetMethod(
            "Accessories_RemoveDependentsForExclusiveGroupChange",
            BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null) throw new InvalidOperationException("KTS replacement rule was not found.");
        method.Invoke(null, new object[] { items, selected, extra });

        if (selected.Contains(sma.Id) || selected.Contains(dependent.Id) || !selected.Contains(unrelated.Id))
            throw new InvalidOperationException("KTS replacement did not remove only incompatible dependent functions.");
    }

    private static void TestRegulationLevelControlSynchronization()
    {
        MethodInfo method = typeof(CLMainForm).GetMethod(
            "Performance_SynchronizeRegulationLevelControls",
            BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null) throw new InvalidOperationException("Regulation-level control synchronization was not found.");

        using (var scrollBar = new HScrollBar { Minimum = 20, Maximum = 109, LargeChange = 10, Value = 100 })
        using (var valueLabel = new Label { Text = "100 %" })
        using (var progressBar = new ProgressBar { Minimum = 0, Maximum = 100, Value = 100 })
        {
            method.Invoke(null, new object[] { scrollBar, valueLabel, progressBar, 88 });
            if (scrollBar.Value != 88 || progressBar.Value != 88 || valueLabel.Text != "88 %")
                throw new InvalidOperationException("Loaded regulation level did not synchronize all UI controls.");

            method.Invoke(null, new object[] { scrollBar, valueLabel, progressBar, 109 });
            if (scrollBar.Value != 100 || progressBar.Value != 100 || valueLabel.Text != "100 %")
                throw new InvalidOperationException("Regulation-level upper bound is invalid.");
        }
    }

    private static void TestReportEmailFeature()
    {
        string subject = CLSelectionEmailComposer.BuildSubject(
            "Selection - {0}", "CLRC 038 OSC", "Project 42", "1234-5678-9012-3456");
        if (subject != "Selection - Project_42_CLRC 038 OSC - 1234-5678-9012-3456")
            throw new InvalidOperationException("Selection email subject is invalid.");

        string subjectWithoutReference = CLSelectionEmailComposer.BuildSubject(
            "Selection - {0}", "CLRC 038 OSC", " ", " ");
        if (subjectWithoutReference != "Selection - CLRC 038 OSC")
            throw new InvalidOperationException("Selection email subject has a trailing separator.");

        string body = CLSelectionEmailComposer.BuildBody("Selected unit: {0}", "CLRC 038 OSC");
        if (body != "Selected unit: CLRC 038 OSC")
            throw new InvalidOperationException("Selection email body is invalid.");

        string repositoryRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        string[] languages = { "bg", "da", "de", "en", "fr", "hu", "it", "nl", "pl", "ro", "sl", "sv" };
        string[] keys =
        {
            "ReportViewer_Email", "ReportViewer_EmailTooltip", "ReportViewer_EmailSubject",
            "ReportViewer_EmailBody", "ReportViewer_EmailPdfError",
            "ReportViewer_EmailOutlookUnavailable", "ReportViewer_EmailAttachmentError",
            "ReportViewer_EmailError"
        };
        foreach (string language in languages)
        {
            var document = new XmlDocument();
            document.Load(Path.Combine(repositoryRoot, "SSWLib", "Resources." + language + ".resx"));
            foreach (string key in keys)
            {
                XmlNode value = document.SelectSingleNode("/root/data[@name='" + key + "']/value");
                if (value == null || String.IsNullOrWhiteSpace(value.InnerText))
                    throw new InvalidOperationException("Missing email translation " + key + " for " + language + ".");
            }
        }
    }

    private static void TestAccessoryReportTemplates()
    {
        string repositoryRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        string[] reportFiles = {
            "CLMainReport.rdlc", "CLMainReport_Coil.rdlc",
            "CLMainReportWithCO2.rdlc", "CLMainReportWithCO2_Coil.rdlc"
        };
        string[] fields = {
            "Title", "CodeCaption", "DescriptionCaption", "FunctionsCaption", "StatusCaption",
            "Code", "Description", "Functions", "Status"
        };
        foreach (string reportFile in reportFiles)
        {
            var document = new XmlDocument();
            document.Load(Path.Combine(repositoryRoot, "SSWLib", reportFile));
            var namespaces = new XmlNamespaceManager(document.NameTable);
            namespaces.AddNamespace("r", document.DocumentElement.NamespaceURI);
            if (document.SelectSingleNode("//r:Rectangle[@Name='AccessorySelectionBlock']", namespaces) == null ||
                document.SelectSingleNode("//r:Tablix[@Name='TablixAccessoryReport']", namespaces) == null)
                throw new InvalidOperationException("Accessory report section is missing: " + reportFile);
            foreach (string field in fields)
            {
                string query = "//r:DataSet[@Name='AccessoryReport']/r:Fields/r:Field[@Name='" + field + "']";
                if (document.SelectSingleNode(query, namespaces) == null)
                    throw new InvalidOperationException("Accessory report field is missing: " + reportFile + "/" + field);
            }

            byte[] shortReport = RenderAccessoryReport(document, Path.Combine(repositoryRoot, "SSWLib", reportFile), 1);
            byte[] longReport = RenderAccessoryReport(document, Path.Combine(repositoryRoot, "SSWLib", reportFile), 80);
            if (shortReport.Length < 1000 || longReport.Length < 1000)
                throw new InvalidOperationException("Accessory report PDF rendering is empty: " + reportFile);
            if (PdfPageCount(longReport) < 2)
                throw new InvalidOperationException("Long accessory report did not paginate: " + reportFile);
        }
    }

    private static byte[] RenderAccessoryReport(XmlDocument document, string reportPath, int accessoryRows)
    {
        string reportNamespace = document.DocumentElement.NamespaceURI;
        var namespaces = new XmlNamespaceManager(document.NameTable);
        namespaces.AddNamespace("r", reportNamespace);
        namespaces.AddNamespace("rd", "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");

        using (var report = new LocalReport { ReportPath = reportPath })
        {
            foreach (XmlNode dataSetNode in document.SelectNodes("//r:DataSets/r:DataSet", namespaces))
            {
                string dataSetName = dataSetNode.Attributes["Name"].Value;
                var table = new DataTable(dataSetName);
                foreach (XmlNode fieldNode in dataSetNode.SelectNodes("r:Fields/r:Field", namespaces))
                {
                    string fieldName = fieldNode.Attributes["Name"].Value;
                    XmlNode typeNode = fieldNode.SelectSingleNode("rd:TypeName", namespaces);
                    Type fieldType = typeNode == null ? typeof(string) : Type.GetType(typeNode.InnerText, false) ?? typeof(string);
                    table.Columns.Add(fieldName, fieldType);
                }

                if (dataSetName == "AccessoryReport")
                {
                    for (int rowIndex = 0; rowIndex < accessoryRows; rowIndex++)
                    {
                        DataRow row = table.NewRow();
                        row["Title"] = "Accessories and functions";
                        row["CodeCaption"] = "Accessory";
                        row["DescriptionCaption"] = "Description";
                        row["FunctionsCaption"] = "Functions";
                        row["StatusCaption"] = "Status";
                        row["Code"] = "ACC " + (rowIndex + 1).ToString("00");
                        row["Description"] = "Accessory description used to verify report growth and pagination";
                        row["Functions"] = "First associated function" + Environment.NewLine + "Second associated function";
                        row["Status"] = "\u25A0 Optional - External";
                        table.Rows.Add(row);
                    }
                }
                report.DataSources.Add(new ReportDataSource(dataSetName, table));
            }
            return report.Render("PDF");
        }
    }

    private static int PdfPageCount(byte[] pdf)
    {
        return Regex.Matches(Encoding.ASCII.GetString(pdf), @"/Type\s*/Page\b").Count;
    }

    private static void TestRegistryBootstrapProvisioning()
    {
        const string keyPath = @"Software\Avensys\SSW\TechnicalSelection";
        const string valueName = "BootstrapKey_AV";
        const string testValue = "registry-bootstrap-test-value";
        object existingValue = null;
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
        {
            existingValue = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            key.SetValue(valueName, testValue, RegistryValueKind.String);
        }

        try
        {
            MethodInfo resolve = typeof(CLSelectionApiClient).GetMethod(
                "ResolveBootstrapKey", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo clear = typeof(CLSelectionApiClient).GetMethod(
                "ClearUserBootstrapKey", BindingFlags.NonPublic | BindingFlags.Static);
            if (resolve == null || clear == null)
                throw new InvalidOperationException("Bootstrap registry methods are unavailable.");
            string resolved = (string)resolve.Invoke(null, new object[] { "AV" });
            if (!String.Equals(resolved, testValue, StringComparison.Ordinal))
                throw new InvalidOperationException("Bootstrap registry value was not resolved.");
            clear.Invoke(null, new object[] { "AV" });
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, false))
            {
                if (key != null && key.GetValue(valueName) != null)
                    throw new InvalidOperationException("Consumed bootstrap registry value was not removed.");
            }
        }
        finally
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (existingValue == null) key.DeleteValue(valueName, false);
                else key.SetValue(valueName, existingValue, RegistryValueKind.String);
            }
        }
    }

    private static void TestPracticalSelectionRules()
    {
        string suggested = CLSelectionFileName.BuildSuggestedName(
            "D-ABCD-000123",
            "  Offerta cliente: reparto nord con nome molto lungo  ");
        const string baseName = "_D-ABCD-000123";
        string prefix = suggested.Substring(0, suggested.Length - baseName.Length);
        if (!suggested.EndsWith(baseName, StringComparison.Ordinal) ||
            prefix.Length > 30 || suggested.Contains(" ") || suggested.Contains(":"))
        {
            throw new InvalidOperationException("Customer reference filename prefix sanitization failed: " + suggested);
        }
        if (CLSelectionFileName.BuildSuggestedName("D-ABCD-000123", "  ") != "D-ABCD-000123")
            throw new InvalidOperationException("Empty customer reference changed the suggested filename.");

        List<CLCoilHydraulicIssue> invalidCooling = CLCoilHydraulicRules.Evaluate(
            CLCoilPerformanceMode.CWD, 12, 7, 80, 70);
        if (!invalidCooling.Any(issue => issue.IsBlocking &&
            issue.Code == CLCoilHydraulicIssueCode.InvalidCoolingTemperatures))
            throw new InvalidOperationException("Invalid cooling temperature direction was not blocked.");

        List<CLCoilHydraulicIssue> invalidHeating = CLCoilHydraulicRules.Evaluate(
            CLCoilPerformanceMode.HWD, 7, 12, 70, 80);
        if (!invalidHeating.Any(issue => issue.IsBlocking &&
            issue.Code == CLCoilHydraulicIssueCode.InvalidHeatingTemperatures))
            throw new InvalidOperationException("Invalid heating temperature direction was not blocked.");

        List<CLCoilHydraulicIssue> critical = CLCoilHydraulicRules.Evaluate(
            CLCoilPerformanceMode.CWD, 7, 9.5, 80, 70);
        if (!critical.Any(issue => !issue.IsBlocking &&
            issue.Code == CLCoilHydraulicIssueCode.CriticalWaterDeltaT))
            throw new InvalidOperationException("Water delta T below 3 K was not classified as critical.");

        List<CLCoilHydraulicIssue> warning = CLCoilHydraulicRules.Evaluate(
            CLCoilPerformanceMode.HWD, 7, 12, 80, 75.1);
        if (!warning.Any(issue => !issue.IsBlocking &&
            issue.Code == CLCoilHydraulicIssueCode.LowWaterDeltaT))
            throw new InvalidOperationException("Water delta T from 3 K inclusive to 5 K exclusive was not classified as warning.");

        List<CLCoilHydraulicIssue> fiveKelvin = CLCoilHydraulicRules.Evaluate(
            CLCoilPerformanceMode.HWD, 7, 12, 80, 75);
        if (fiveKelvin.Any(issue => issue.Code == CLCoilHydraulicIssueCode.LowWaterDeltaT))
            throw new InvalidOperationException("Water delta T of exactly 5 K must not produce a warning.");

        if (CLCoilHydraulicRules.MaximumRecommendedWaterPressureDrop != 40.0)
            throw new InvalidOperationException("Recommended water pressure drop limit changed unexpectedly.");
    }

    private static void TestUpdateIntegrity(string root)
    {
        string installerPath = Path.Combine(root, "SSW_Setup_test.exe");
        File.WriteAllBytes(installerPath, Encoding.UTF8.GetBytes("verified installer payload"));
        string hash;
        using (SHA256 algorithm = SHA256.Create())
            hash = String.Concat(algorithm.ComputeHash(File.ReadAllBytes(installerPath)).Select(value => value.ToString("X2")));

        var versionInfo = new SoftwareVersionInfo
        {
            verified_manifest = true,
            manifest_version = 1,
            sha256 = hash,
            size_bytes = new FileInfo(installerPath).Length
        };
        MethodInfo verify = typeof(UpdateManager).GetMethod("VerifyDownloadedInstaller",
            BindingFlags.NonPublic | BindingFlags.Static);
        if (verify == null) throw new InvalidOperationException("Update integrity verifier was not found.");
        verify.Invoke(null, new object[] { installerPath, versionInfo });

        File.AppendAllText(installerPath, "tampered");
        try
        {
            verify.Invoke(null, new object[] { installerPath, versionInfo });
            throw new InvalidOperationException("A tampered update package was accepted.");
        }
        catch (TargetInvocationException exception) when (exception.InnerException is InvalidDataException)
        {
        }
    }

    private static void TestSdfFixtures(string temporaryRoot)
    {
        string repositoryRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        string fixtureRoot = Path.Combine(repositoryRoot, "tests", "fixtures", "sdf");
        string legacyPath = CopySdfFixture(fixtureRoot, temporaryRoot, "legacy-0.sdf");
        string schema1Path = CopySdfFixture(fixtureRoot, temporaryRoot, "schema-1-av.sdf");
        string schema2Path = CopySdfFixture(fixtureRoot, temporaryRoot, "schema-2-av.sdf");
        string schema3Path = CopySdfFixture(fixtureRoot, temporaryRoot, "schema-3-av.sdf");
        CLDatabaseCompatibilityInfo legacy = CLDatabaseCompatibilityReader.Inspect(
            legacyPath, new Version(1, 3, 0, 45), "035889");
        if (legacy.State != CLDatabaseCompatibilityState.Legacy || legacy.SchemaVersion != 0)
            throw new InvalidOperationException("Legacy-0 SDF fixture was not recognized.");

        CLDatabaseCompatibilityInfo schema1 = CLDatabaseCompatibilityReader.Inspect(
            schema1Path, new Version(1, 3, 0, 45), "035889");
        if (schema1.State != CLDatabaseCompatibilityState.Managed || schema1.SchemaVersion != 1 ||
            !schema1.HasFeature("CoreData") || !schema1.HasFeature("WaterCoils"))
            throw new InvalidOperationException("Managed schema-1 SDF fixture failed compatibility checks.");

        CLDatabaseCompatibilityInfo schema2 = CLDatabaseCompatibilityReader.Inspect(
            schema2Path, new Version(1, 3, 0, 45), "035889");
        if (schema2.State != CLDatabaseCompatibilityState.Managed || schema2.SchemaVersion != 2 ||
            !schema2.HasFeature("CoreData") || !schema2.HasFeature("WaterCoils") ||
            !schema2.HasFeature("CoilInstallationType") || !schema2.HasFeature("ElectricHeaters"))
            throw new InvalidOperationException("Managed schema-2 SDF fixture failed compatibility checks.");

        CLDatabaseCompatibilityInfo schema3 = CLDatabaseCompatibilityReader.Inspect(
            schema3Path, new Version(1, 3, 0, 52), "035889");
        if (schema3.State != CLDatabaseCompatibilityState.Managed || schema3.SchemaVersion != 3 ||
            !schema3.HasFeature("CoreData") || !schema3.HasFeature("WaterCoils") ||
            !schema3.HasFeature("CoilInstallationType") || !schema3.HasFeature("ElectricHeaters") ||
            !schema3.HasFeature("AccessoriesAndControlFunctions"))
            throw new InvalidOperationException("Managed schema-3 SDF fixture failed compatibility checks.");
    }

    private static string CopySdfFixture(string fixtureRoot, string temporaryRoot, string fileName)
    {
        string destination = Path.Combine(temporaryRoot, fileName);
        File.Copy(Path.Combine(fixtureRoot, fileName), destination, true);
        return destination;
    }

    private static int RunLiveBootstrap()
    {
        const string bootstrapName = "SSW_SELECTION_BOOTSTRAP_KEY_AV";
        string bootstrapKey = Environment.GetEnvironmentVariable(bootstrapName, EnvironmentVariableTarget.User);
        if (String.IsNullOrWhiteSpace(bootstrapKey))
            throw new InvalidOperationException("The AV user bootstrap credential is not provisioned.");

        var context = new CLSelectionRegistrationContext
        {
            CustomerCode = "AV",
            SoftwareVersion = typeof(CLMainForm).Assembly.GetName().Version.ToString(),
            DatabaseSchemaVersion = 1,
            DatabaseContentHash = "release-installer-smoke",
            ApiContractVersion = 1
        };
        var client = new CLSelectionApiClient();
        string first = client.EnsureAccessTokenAsync(context).GetAwaiter().GetResult();
        string cached = client.EnsureAccessTokenAsync(context).GetAwaiter().GetResult();
        if (String.IsNullOrWhiteSpace(first) || !String.Equals(first, cached, StringComparison.Ordinal))
            throw new InvalidOperationException("The live installation token was not cached.");

        string stored = File.ReadAllText(CLSelectionCredentialStore.StateFilePath);
        if (stored.Contains(first))
            throw new InvalidOperationException("The live installation token was stored in plaintext.");
        if (!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(bootstrapName, EnvironmentVariableTarget.User)))
            throw new InvalidOperationException("The consumed bootstrap credential was not removed.");

        Console.WriteLine("Live installation bootstrap passed: https=ok dpapi=ok cache=ok bootstrap_consumed=true");
        return 0;
    }

    private static void TestRegistrationFailureDialog()
    {
        string imagePath = Path.Combine(Path.GetTempPath(), "ssw-selection-registration-dialog-smoke.png");
        using (var dialog = new CLSelectionRegistrationFailureForm(
            "Technische Auswahl",
            "Die technische Auswahl konnte nicht registriert werden: Die Verbindung zum technischen Auswahldienst ist derzeit nicht verfügbar.",
            "Erneut versuchen",
            "Entwurf erstellen",
            "Abbrechen"))
        {
            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location = new Point(-32000, -32000);
            dialog.Show();
            Application.DoEvents();
            Button[] buttons = dialog.Controls.OfType<Button>().OrderBy(button => button.Left).ToArray();
            if (buttons.Length != 3) throw new InvalidOperationException("Registration fallback dialog buttons are incomplete.");
            for (int index = 0; index < buttons.Length; index++)
            {
                if (!dialog.ClientRectangle.Contains(buttons[index].Bounds))
                    throw new InvalidOperationException("Registration fallback button is outside the dialog.");
                if (index > 0 && buttons[index - 1].Bounds.IntersectsWith(buttons[index].Bounds))
                    throw new InvalidOperationException("Registration fallback buttons overlap.");
            }
            using (var bitmap = new Bitmap(dialog.Width, dialog.Height))
            {
                dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, dialog.Size));
                bitmap.Save(imagePath, ImageFormat.Png);
            }
            dialog.Hide();
        }
        if (!File.Exists(imagePath) || new FileInfo(imagePath).Length < 1000)
            throw new InvalidOperationException("Registration fallback dialog did not render correctly.");
    }

    private static void TestSelectionRegistrationClient(CLSelectionApiClient client, TokenHandler handler,
        CLSelectionRegistrationContext context)
    {
        CLSelectionProjectDocument document = CreateCalculatedDocument();
        CLSelectionSnapshotService.Refresh(document);
        CLSelectionRegistrationResult created = client.RegisterSelectionAsync(document, context).GetAwaiter().GetResult();
        CLSelectionRegistrationResult retried = client.RegisterSelectionAsync(document, context).GetAwaiter().GetResult();
        if (created.Revision != 1 || retried.Revision != 1 || created.ResumeToken != retried.ResumeToken ||
            handler.SelectionCreateCount != 2 || handler.IdempotencyKeys[0] != handler.IdempotencyKeys[1])
            throw new InvalidOperationException("Create-selection retry was not deterministic.");

        CLSelectionSnapshotService.MarkRegistered(document, created.PublicReference, created.Revision,
            created.ResumeToken, DateTime.UtcNow);
        CLSelectionRegistrationResult reprint = client.RegisterSelectionAsync(document, context).GetAwaiter().GetResult();
        if (reprint.Revision != 1 || reprint.ChangeKind != "Reprint")
            throw new InvalidOperationException("Unchanged selection did not remain at R01.");

        document.Selection.Winter.SupplyAirflowM3h = 120;
        CLSelectionSnapshotService.Refresh(document);
        CLSelectionRegistrationResult revised = client.RegisterSelectionAsync(document, context).GetAwaiter().GetResult();
        if (revised.Revision != 2 || revised.ChangeKind != "TechnicalChange" || handler.SelectionRevisionCount != 2)
            throw new InvalidOperationException("Changed selection did not create R02.");
    }

    private static void TestSnapshotFingerprints(string root)
    {
        CLSelectionProjectDocument document = CreateCalculatedDocument();
        CLSelectionFingerprintSet initial = CLSelectionSnapshotService.Refresh(document);
        document.Snapshot.CalculatedAtUtc = document.Snapshot.CalculatedAtUtc.AddHours(3);
        document.Selection.CustomerReference = "changed customer note";
        CLSelectionFingerprintSet nonTechnical = CLSelectionSnapshotService.Refresh(document);
        if (initial.SnapshotHash != nonTechnical.SnapshotHash) throw new InvalidOperationException("Volatile/non-technical data changed the snapshot hash.");
        document.Selection.Accessories[0].LocalizedDisplayName = "Localized controller name";
        document.Selection.Accessories[0].LocalizedDescription = "Localized presentation text";
        document.Selection.Accessories[0].LocalizedFunctionNames = new List<string> { "Localized function" };
        CLSelectionFingerprintSet localizedPresentation = CLSelectionSnapshotService.Refresh(document);
        if (nonTechnical.SnapshotHash != localizedPresentation.SnapshotHash)
            throw new InvalidOperationException("Localized accessory presentation changed the technical snapshot hash.");

        CLSelectionSnapshotService.MarkRegistered(document, "4827-1936-5048-2715", 1, "resume-token", DateTime.UtcNow);
        CLSelectionSnapshotService.Refresh(document);
        AssertChange(document, CLSelectionChangeKind.Reprint);

        document.Selection.Report.LanguageCode = "FR";
        CLSelectionSnapshotService.Refresh(document);
        AssertChange(document, CLSelectionChangeKind.TechnicalChange);
        document.Selection.Report.LanguageCode = "IT";

        document.Selection.Winter.SupplyAirflowM3h = 120;
        CLSelectionSnapshotService.Refresh(document);
        AssertChange(document, CLSelectionChangeKind.TechnicalChange);
        document.Selection.Winter.SupplyAirflowM3h = 100;

        document.Selection.Accessories[0].Quantity = 2;
        CLSelectionSnapshotService.Refresh(document);
        AssertChange(document, CLSelectionChangeKind.TechnicalChange);
        document.Selection.Accessories[0].Quantity = 1;

        document.Versions.DatabaseContentHash = "DIFFERENT-SDF";
        CLSelectionSnapshotService.Refresh(document);
        AssertChange(document, CLSelectionChangeKind.DatabaseChange);
        document.Versions.DatabaseContentHash = "A280D8C";

        document.Versions.CalculationEngineVersion = "9.9.9.9";
        CLSelectionSnapshotService.Refresh(document);
        AssertChange(document, CLSelectionChangeKind.AlgorithmChange);
        document.Versions.CalculationEngineVersion = "1.3.0.44";

        document.Snapshot.Winter.HeatTransferredW = 999;
        CLSelectionSnapshotService.Refresh(document);
        AssertChange(document, CLSelectionChangeKind.CalculationResultChange);

        string projectPath = Path.Combine(root, "fingerprint-roundtrip.sswsel");
        CLSelectionProjectSerializer.Save(projectPath, document);
        CLSelectionProjectDocument loaded = CLSelectionProjectSerializer.Load(projectPath);
        if (loaded.RevisionTracking == null || loaded.RevisionTracking.Current == null ||
            loaded.RevisionTracking.Current.SnapshotHash != document.RevisionTracking.Current.SnapshotHash ||
            loaded.Identity.ResumeToken != "resume-token" ||
            !loaded.Selection.WaterCoil.CustomDesignDisclaimerAccepted ||
            loaded.Selection.Accessories.Count != 1 ||
            loaded.Selection.Accessories[0].Code != "KTS EXTRA" ||
            loaded.Selection.Accessories[0].Quantity != 1 ||
            loaded.Selection.Accessories[0].LocalizedDisplayName != "Localized controller name" ||
            loaded.Selection.Accessories[0].LocalizedFunctionNames.Count != 1)
        {
            throw new InvalidOperationException("Snapshot/revision metadata round-trip failed.");
        }

        string reportPath = Path.Combine(root, "SSW report reference.pdf");
        string companionPath = CLSelectionProjectSerializer.GetReportCompanionPath(reportPath);
        string expectedCompanionPath = Path.Combine(root, "SSW report reference.sswsel");
        if (!String.Equals(companionPath, expectedCompanionPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The report companion project path is not deterministic.");
        }
        CLSelectionProjectSerializer.Save(companionPath, document);
        CLSelectionProjectDocument companion = CLSelectionProjectSerializer.Load(companionPath);
        if (companion.RevisionTracking.Current.SnapshotHash != document.RevisionTracking.Current.SnapshotHash)
        {
            throw new InvalidOperationException("The report companion project lost its snapshot fingerprint.");
        }

        string repositoryRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        foreach (string fixtureName in new[] { "selection-v1.sswsel", "selection-v1-sparse.sswsel" })
        {
            string migratedPath = Path.Combine(root, fixtureName);
            File.Copy(Path.Combine(repositoryRoot, "docs", "examples", fixtureName), migratedPath, true);
            CLSelectionProjectDocument legacy = CLSelectionProjectSerializer.Load(migratedPath);
            if (legacy.SelectionFormatVersion != 2 || legacy.SourceFormatVersion != 1 ||
                !legacy.RequiresMigrationBackup || legacy.RevisionTracking == null ||
                legacy.Selection.Accessories == null || legacy.Selection.Accessories.Count != 0)
                throw new InvalidOperationException("Legacy V1 fixture compatibility failed: " + fixtureName);
            CLSelectionProjectSerializer.Save(migratedPath, legacy);
            string backupPath = migratedPath + ".pre-migration-v1.bak";
            if (!File.Exists(backupPath) || !File.ReadAllText(backupPath).Contains("\"selectionFormatVersion\": 1"))
                throw new InvalidOperationException("Legacy V1 migration backup failed: " + fixtureName);
            CLSelectionProjectDocument reloaded = CLSelectionProjectSerializer.Load(migratedPath);
            if (reloaded.SelectionFormatVersion != 2 || reloaded.SourceFormatVersion != 2 ||
                reloaded.RequiresMigrationBackup)
                throw new InvalidOperationException("Migrated V2 round-trip failed: " + fixtureName);
        }
    }

    private static void AssertChange(CLSelectionProjectDocument document, CLSelectionChangeKind expected)
    {
        CLSelectionChangeKind actual = CLSelectionSnapshotService.Evaluate(document);
        if (actual != expected) throw new InvalidOperationException("Expected " + expected + ", got " + actual + ".");
    }

    private static CLSelectionProjectDocument CreateCalculatedDocument()
    {
        return new CLSelectionProjectDocument
        {
            Versions = new CLSelectionVersionSet
            {
                SoftwareVersion = "1.3.0.44",
                CalculationEngineVersion = "1.3.0.44",
                DatabaseSchemaVersion = 1,
                DatabaseDataVersion = "2026.07.14",
                DatabaseContentHash = "A280D8C",
                SelectionFormatVersion = CLTechnicalVersions.CurrentSelectionFormatVersion,
                ReportTemplateVersion = 1,
                ApiContractVersion = 1
            },
            Selection = new CLTechnicalSelection
            {
                CustomerCode = "AV",
                CustomerReference = "initial note",
                Unit = new CLSelectionEntityReference { Id = 6, Code = "CLRC 06A OSC", Name = "CLRC 06A OSC" },
                Winter = new CLOperatingScenarioInput
                {
                    Enabled = true,
                    ScenarioCode = "Winter",
                    SupplyAirflowM3h = 100,
                    ExtractAirflowM3h = 100,
                    MaximumPressurePa = 473,
                    OutdoorTemperatureC = -10,
                    OutdoorRelativeHumidityPercent = 80,
                    ReturnTemperatureC = 20,
                    ReturnRelativeHumidityPercent = 60
                },
                Summer = new CLOperatingScenarioInput { Enabled = false, ScenarioCode = "Summer" },
                WaterCoil = new CLWaterCoilSelection
                {
                    Enabled = true,
                    SelectionCase = "StandardCustomized",
                    CustomDesignDisclaimerAccepted = true
                },
                Accessories = new List<CLAccessorySelection>
                {
                    new CLAccessorySelection
                    {
                        Code = "KTS EXTRA",
                        ItemType = "Accessory",
                        Quantity = 1,
                        Availability = "Optional",
                        InstallationType = "External",
                        LocalizedDisplayName = "KTS Extra touch screen controller",
                        LocalizedDescription = "Touch screen controller",
                        LocalizedFunctionNames = new List<string> { "Constant airflow control" }
                    }
                },
                Report = new CLReportSelectionOptions { LanguageCode = "IT", IncludePerformanceCharts = true }
            },
            Snapshot = new CLCalculatedSelectionSnapshot
            {
                CalculatedAtUtc = DateTime.UtcNow,
                Winter = new CLScenarioCalculationSnapshot
                {
                    ScenarioCode = "Winter",
                    AirflowM3h = 100,
                    AvailablePressurePa = 473,
                    HeatTransferredW = 967,
                    SensibleHeatW = 611,
                    LatentHeatW = 356,
                    EfficiencyPercent = 95,
                    SupplyOutletTemperatureC = 18.6
                }
            }
        };
    }
}
