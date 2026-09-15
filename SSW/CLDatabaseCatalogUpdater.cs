using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SSW
{
    internal sealed class CatalogManifest
    {
        public int manifestVersion { get; set; }
        public string catalogVersion { get; set; }
        public int schemaVersion { get; set; }
        public string minimumSswVersion { get; set; }
        public string customerCode { get; set; }
        public string downloadUrl { get; set; }
        public long sizeBytes { get; set; }
        public string sha256 { get; set; }
    }

    internal sealed class CatalogUpdateState
    {
        public string catalogVersion { get; set; }
        public string sha256 { get; set; }
    }

    internal sealed class TimedWebClient : WebClient
    {
        private readonly int timeoutMilliseconds;
        internal TimedWebClient(int timeoutMilliseconds) { this.timeoutMilliseconds = timeoutMilliseconds; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            request.Timeout = timeoutMilliseconds;
            return request;
        }
    }

    internal sealed class CatalogUpdateForm : Form
    {
        private readonly Label status;

        internal CatalogUpdateForm()
        {
            Text = "SSW";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ControlBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(430, 95);
            status = new Label {
                AutoSize = false, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 11.0F, FontStyle.Bold)
            };
            Controls.Add(status);
        }

        internal void SetStatus(string value)
        {
            status.Text = value;
            Refresh();
            Application.DoEvents();
        }
    }

    internal static class CLDatabaseCatalogUpdater
    {
        internal static void CheckAndUpdate(string databasePath, CLSSWInfo info)
        {
            string manifestUrl = ConfigurationManager.AppSettings["DataCentralCatalogManifestUrl"];
            if (String.IsNullOrWhiteSpace(manifestUrl) || info == null) return;
            string logPath = Path.Combine(Path.GetDirectoryName(databasePath), "catalog-update.log");
            string statePath = Path.Combine(Path.GetDirectoryName(databasePath), "catalog-update.state.json");
            using (var progress = new CatalogUpdateForm())
            {
                progress.Show();
                progress.SetStatus("Verifica aggiornamenti catalogo...");
                try
                {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    CatalogManifest manifest;
                    using (var client = new TimedWebClient(8000))
                    {
                        client.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                        string json = client.DownloadString(manifestUrl);
                        manifest = new JavaScriptSerializer().Deserialize<CatalogManifest>(json);
                    }
                    ValidateManifest(manifest, info);
                    if (File.Exists(databasePath) && IsApplied(statePath, manifest)) return;

                    progress.SetStatus("Aggiornamento database in corso...\r\n" + manifest.catalogVersion);
                    string temporaryPath = databasePath + ".download";
                    string backupPath = databasePath + ".previous";
                    Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
                    if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
                    using (var client = new TimedWebClient(60000)) client.DownloadFile(manifest.downloadUrl, temporaryPath);
                    var file = new FileInfo(temporaryPath);
                    if (file.Length != manifest.sizeBytes || !Hash(temporaryPath).Equals(manifest.sha256, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Downloaded catalog integrity check failed.");
                    CLDatabaseCompatibilityReader.Inspect(temporaryPath, info.ReleaseVersion, info.Code);
                    if (File.Exists(databasePath))
                    {
                        if (File.Exists(backupPath)) File.Delete(backupPath);
                        File.Replace(temporaryPath, databasePath, backupPath, true);
                    }
                    else File.Move(temporaryPath, databasePath);
                    File.WriteAllText(statePath, new JavaScriptSerializer().Serialize(new CatalogUpdateState {
                        catalogVersion = manifest.catalogVersion, sha256 = manifest.sha256
                    }));
                    File.WriteAllText(logPath, DateTime.UtcNow.ToString("o") + " updated " + manifest.catalogVersion);
                    progress.SetStatus("Catalogo aggiornato correttamente.");
                    System.Threading.Thread.Sleep(700);
                }
                catch (Exception exception)
                {
                    try { File.AppendAllText(logPath, DateTime.UtcNow.ToString("o") + " skipped: " + exception.Message + Environment.NewLine); } catch { }
                }
                finally { progress.Close(); }
            }
        }

        private static void ValidateManifest(CatalogManifest manifest, CLSSWInfo info)
        {
            Version minimum;
            if (manifest == null || manifest.manifestVersion != 1 || manifest.schemaVersion < 1 ||
                manifest.schemaVersion > CLTechnicalVersions.CurrentDatabaseSchemaVersion ||
                !Version.TryParse(manifest.minimumSswVersion, out minimum) || info.ReleaseVersion < minimum ||
                !String.Equals(manifest.customerCode, info.Code, StringComparison.OrdinalIgnoreCase) ||
                !Uri.IsWellFormedUriString(manifest.downloadUrl, UriKind.Absolute) || manifest.sizeBytes <= 0 ||
                String.IsNullOrWhiteSpace(manifest.sha256) || manifest.sha256.Length != 64)
                throw new InvalidDataException("Catalog manifest is invalid or incompatible.");
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        private static bool IsApplied(string statePath, CatalogManifest manifest)
        {
            try
            {
                if (!File.Exists(statePath)) return false;
                var state = new JavaScriptSerializer().Deserialize<CatalogUpdateState>(File.ReadAllText(statePath));
                return state != null && String.Equals(state.catalogVersion, manifest.catalogVersion, StringComparison.Ordinal) &&
                    String.Equals(state.sha256, manifest.sha256, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }
    }
}
