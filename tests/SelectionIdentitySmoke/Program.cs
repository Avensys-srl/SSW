using System;
using System.Collections.Generic;
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
using SSW;

internal sealed class TokenHandler : HttpMessageHandler
{
    public int RegistrationCount { get; private set; }
    public int RenewalCount { get; private set; }
    public int SelectionCreateCount { get; private set; }
    public int SelectionRevisionCount { get; private set; }
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
            if (!request.Headers.Contains("X-SSW-Bootstrap-Key")) throw new InvalidOperationException("Bootstrap header missing.");
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
                !body.Contains("\"versions\"") || !body.Contains("\"fingerprints\""))
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
            string stored = File.ReadAllText(CLSelectionCredentialStore.StateFilePath);
            if (stored.Contains(first)) throw new InvalidOperationException("Access token was stored in plaintext.");

            CLSelectionCredentialStore.SaveAccessToken(first, DateTime.UtcNow.AddDays(1));
            string renewed = client.EnsureAccessTokenAsync(context).GetAwaiter().GetResult();
            if (renewed != "renewed-token-never-plaintext" || handler.RenewalCount != 1) throw new InvalidOperationException("Token renewal failed.");
            if (File.ReadAllText(CLSelectionCredentialStore.StateFilePath).Contains(renewed)) throw new InvalidOperationException("Renewed token was stored in plaintext.");

            TestSelectionRegistrationClient(client, handler, context);
            TestSnapshotFingerprints(root);
            TestSdfFixtures();
            TestRegistrationFailureDialog();
            TestUpdateIntegrity(root);

            Console.WriteLine("Selection identity/snapshot smoke test passed: token=ok create=R01 reprint=R01 revise=R02 fingerprints=stable dialog=rendered update_integrity=ok");
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

    private static void TestSdfFixtures()
    {
        string repositoryRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        string fixtureRoot = Path.Combine(repositoryRoot, "tests", "fixtures", "sdf");
        CLDatabaseCompatibilityInfo legacy = CLDatabaseCompatibilityReader.Inspect(
            Path.Combine(fixtureRoot, "legacy-0.sdf"), new Version(1, 3, 0, 45), "035889");
        if (legacy.State != CLDatabaseCompatibilityState.Legacy || legacy.SchemaVersion != 0)
            throw new InvalidOperationException("Legacy-0 SDF fixture was not recognized.");

        CLDatabaseCompatibilityInfo schema1 = CLDatabaseCompatibilityReader.Inspect(
            Path.Combine(fixtureRoot, "schema-1-av.sdf"), new Version(1, 3, 0, 45), "035889");
        if (schema1.State != CLDatabaseCompatibilityState.Managed || schema1.SchemaVersion != 1 ||
            !schema1.HasFeature("CoreData") || !schema1.HasFeature("WaterCoils"))
            throw new InvalidOperationException("Managed schema-1 SDF fixture failed compatibility checks.");

        CLDatabaseCompatibilityInfo schema2 = CLDatabaseCompatibilityReader.Inspect(
            Path.Combine(fixtureRoot, "schema-2-av.sdf"), new Version(1, 3, 0, 45), "035889");
        if (schema2.State != CLDatabaseCompatibilityState.Managed || schema2.SchemaVersion != 2 ||
            !schema2.HasFeature("CoreData") || !schema2.HasFeature("WaterCoils") ||
            !schema2.HasFeature("CoilInstallationType") || !schema2.HasFeature("ElectricHeaters"))
            throw new InvalidOperationException("Managed schema-2 SDF fixture failed compatibility checks.");
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
            loaded.Identity.ResumeToken != "resume-token")
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
            CLSelectionProjectDocument legacy = CLSelectionProjectSerializer.Load(
                Path.Combine(repositoryRoot, "docs", "examples", fixtureName));
            if (legacy.SelectionFormatVersion != 1 || legacy.RevisionTracking == null)
                throw new InvalidOperationException("Legacy V1 fixture compatibility failed: " + fixtureName);
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
                SelectionFormatVersion = 1,
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
