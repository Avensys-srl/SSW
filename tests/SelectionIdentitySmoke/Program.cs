using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SSW;

internal sealed class TokenHandler : HttpMessageHandler
{
    public int RegistrationCount { get; private set; }
    public int RenewalCount { get; private set; }

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
}

internal static class Program
{
    private static int Main()
    {
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

            TestSnapshotFingerprints(root);

            Console.WriteLine("Selection identity/snapshot smoke test passed: register=1 cache=1 renew=1 fingerprints=stable classifications=5");
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
