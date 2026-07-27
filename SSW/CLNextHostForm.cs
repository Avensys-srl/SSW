using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SSW
{
    internal static class CLNextUiSmokeCommand
    {
        public static int Run()
        {
            var models = CLNextUiApplicationService.GetModels();
            if (models == null || models.Count == 0) return 10;
            var model = models.Find(item => item.Code == "CLRC 038 OSC") ?? models[0];
            CLNextUiCalculationResult result = CLNextUiApplicationService.Calculate(
                new CLNextUiCalculationInput
                {
                    ModelCode = model.Code,
                    SupplyAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    ExtractAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    PressurePa = 100
                });
            if (result.Winter == null || result.Summer == null) return 11;
            if (result.Winter.Curves.WorkingPointAirflow <= 0) return 12;
            if (result.Layout == null) return 13;
            string json = new JavaScriptSerializer().Serialize(result);
            return String.IsNullOrWhiteSpace(json) || json.Length < 100 ? 14 : 0;
        }
    }

    internal sealed class CLNextHostForm : Form
    {
        private const string VirtualHostName = "ssw.local";
        private readonly WebView2 webView;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public CLNextHostForm()
        {
            Text = CLSSWProfile.AssemblyTitle + " - UI Preview";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 720);
            WindowState = FormWindowState.Maximized;
            webView = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(webView);
            Shown += async delegate { await InitializeAsync(); };
        }

        private async Task InitializeAsync()
        {
            try
            {
                string userDataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Avensys", "SSW", "WebView2Preview");
                CoreWebView2Environment environment =
                    await CoreWebView2Environment.CreateAsync(null, userDataDirectory);
                await webView.EnsureCoreWebView2Async(environment);

                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
#if DEBUG
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
#else
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif
                webView.CoreWebView2.WebMessageReceived += WebMessageReceived;

                string developmentUrl = Environment.GetEnvironmentVariable("SSW_NEXT_UI_DEV_URL");
                if (!String.IsNullOrWhiteSpace(developmentUrl))
                {
                    webView.Source = new Uri(developmentUrl);
                    return;
                }

                string assetDirectory = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    "frontend", "ssw-next");
                string indexPath = Path.Combine(assetDirectory, "index.html");
                if (!File.Exists(indexPath))
                    throw new FileNotFoundException("SSW Next UI assets are missing.", indexPath);

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    VirtualHostName,
                    assetDirectory,
                    CoreWebView2HostResourceAccessKind.DenyCors);
                webView.Source = new Uri("https://" + VirtualHostName + "/index.html");
            }
            catch (Exception exception)
            {
                DialogResult result = MessageBox.Show(
                    this,
                    "The UI preview could not be started.\r\n\r\n" + exception.Message +
                    "\r\n\r\nOpen the current selection interface?",
                    CLSSWProfile.AssemblyTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    OpenLegacy();
                else
                    Close();
            }
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs eventArgs)
        {
            BridgeRequest request = null;
            try
            {
                request = serializer.Deserialize<BridgeRequest>(eventArgs.WebMessageAsJson);
                if (request == null || String.IsNullOrWhiteSpace(request.Command))
                    throw new InvalidOperationException("Bridge command is missing.");

                object payload;
                switch (request.Command)
                {
                    case "app.initialize":
                        payload = BuildInitializationPayload();
                        BeginInvoke(new Action(delegate
                        {
                            Text = CLSSWProfile.AssemblyTitle + " - UI Preview (SDF connected)";
                        }));
                        break;
                    case "selection.calculate":
                        payload = CalculateSelection(request.Payload);
                        break;
                    case "legacy.open":
                        BeginInvoke(new Action(OpenLegacy));
                        payload = new { opened = true };
                        break;
                    case "app.close":
                        BeginInvoke(new Action(Close));
                        payload = new { closed = true };
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported bridge command: " + request.Command);
                }
                PostResponse(request.RequestId, true, payload, null);
            }
            catch (Exception exception)
            {
                PostResponse(request == null ? null : request.RequestId, false, null, exception.Message);
            }
        }

        private static object BuildInitializationPayload()
        {
            return new
            {
                bridgeVersion = 1,
                profile = CLSSWProfile.ShortName,
                applicationName = CLSSWProfile.AssemblyTitle,
                legacyAvailable = true,
                measureUnit = "SI",
                models = CLNextUiApplicationService.GetModels()
            };
        }

        private static object CalculateSelection(Dictionary<string, object> payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return CLNextUiApplicationService.Calculate(new CLNextUiCalculationInput
            {
                ModelCode = TextValue(payload, "modelCode"),
                SupplyAirflowM3h = NumberValue(payload, "supplyAirflow", 0),
                ExtractAirflowM3h = NumberValue(payload, "extractAirflow", 0),
                PressurePa = NumberValue(payload, "pressure", 0),
                RegulationPercent = NumberValue(payload, "regulation", 100)
            });
        }

        private void OpenLegacy()
        {
            Hide();
            using (var legacy = new CLMainForm())
                legacy.ShowDialog(this);
            Show();
        }

        private void PostResponse(string requestId, bool success, object payload, string error)
        {
            if (webView.CoreWebView2 == null) return;
            string json = serializer.Serialize(new
            {
                requestId,
                success,
                payload,
                error
            });
            webView.CoreWebView2.PostWebMessageAsJson(json);
        }

        private static string TextValue(IDictionary<string, object> payload, string key)
        {
            object value;
            return payload.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value, CultureInfo.InvariantCulture)
                : String.Empty;
        }

        private static double NumberValue(IDictionary<string, object> payload, string key, double fallback)
        {
            object value;
            if (!payload.TryGetValue(key, out value) || value == null) return fallback;
            double parsed;
            return Double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Float, CultureInfo.InvariantCulture, out parsed)
                ? parsed
                : fallback;
        }

        private sealed class BridgeRequest
        {
            public string RequestId { get; set; }
            public string Command { get; set; }
            public Dictionary<string, object> Payload { get; set; }
        }
    }
}
