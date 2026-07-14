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

            Console.WriteLine("Selection identity smoke test passed: register=1 cache=1 renew=1 plaintext=false");
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
}

