using System;
using System.Collections;
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
            if (result.AvailableWaterCoils == null ||
                result.AvailableElectricHeaters == null) return 15;
            if (result.AvailableWaterCoils.Count > 0)
            {
                var coilInput = new CLNextUiCalculationInput
                {
                    ModelCode = model.Code,
                    SupplyAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    ExtractAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    PressurePa = 100,
                    WaterCoilEnabled = true,
                    WaterCoilId = result.AvailableWaterCoils[0].Id,
                    WaterCoilMode = result.AvailableWaterCoils[0].Mode
                };
                CLNextUiCalculationResult coilResult =
                    CLNextUiApplicationService.Calculate(coilInput);
                if (coilResult.WaterCoilResults.Count == 0) return 16;
            }
            if (result.AvailableElectricHeaters.Count > 0)
            {
                CLNextUiElectricHeaterSummary heater =
                    result.AvailableElectricHeaters[0];
                var heaterInput = new CLNextUiCalculationInput
                {
                    ModelCode = model.Code,
                    SupplyAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    ExtractAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    PressurePa = 100
                };
                if (String.Equals(heater.Mode, "PEHD", StringComparison.OrdinalIgnoreCase))
                {
                    heaterInput.ElectricPreheaterEnabled = true;
                    heaterInput.ElectricPreheaterId = heater.Id;
                }
                else
                {
                    heaterInput.ElectricPostheaterEnabled = true;
                    heaterInput.ElectricPostheaterId = heater.Id;
                }
                CLNextUiCalculationResult heaterResult =
                    CLNextUiApplicationService.Calculate(heaterInput);
                if (heaterResult.ElectricHeaterResults.Count == 0) return 19;
            }
            var documentInput = new CLNextUiCalculationInput
            {
                ProjectName = "Smoke",
                CustomerReference = "NEXT-UI",
                LanguageCode = "it",
                ModelCode = model.Code,
                SupplyAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                ExtractAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                PressurePa = 100,
                AccessoryCodes = new List<string>()
            };
            CLSelectionProjectDocument document =
                CLNextUiApplicationService.CreateProjectDocument(documentInput);
            if (document.Selection.Unit.Code != model.Code ||
                document.Selection.CustomerReference != "NEXT-UI") return 17;
            string selectionJson = CLSelectionProjectSerializer.Serialize(document);
            CLSelectionProjectDocument restoredDocument =
                CLSelectionProjectSerializer.Deserialize(selectionJson);
            if (restoredDocument.Selection.Unit.Code != model.Code ||
                restoredDocument.Selection.CustomerReference != "NEXT-UI") return 18;
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
                    case "project.edit":
                        BeginInvoke(new Action(delegate
                        {
                            OpenLegacy(CreateInput(request.Payload), false, true);
                        }));
                        payload = new { opened = true };
                        break;
                    case "report.generate":
                        BeginInvoke(new Action(delegate
                        {
                            OpenLegacy(CreateInput(request.Payload), true, false);
                        }));
                        payload = new { opened = true, delegated = true };
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
            return CLNextUiApplicationService.Calculate(CreateInput(payload));
        }

        private static CLNextUiCalculationInput CreateInput(
            Dictionary<string, object> payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return new CLNextUiCalculationInput
            {
                ProjectName = TextValue(payload, "projectName"),
                CustomerReference = TextValue(payload, "customerReference"),
                LanguageCode = TextValue(payload, "languageCode"),
                ModelCode = TextValue(payload, "modelCode"),
                SupplyAirflowM3h = NumberValue(payload, "supplyAirflow", 0),
                ExtractAirflowM3h = NumberValue(payload, "extractAirflow", 0),
                PressurePa = NumberValue(payload, "pressure", 0),
                RegulationPercent = NumberValue(payload, "regulation", 100),
                SummerEnabled = BooleanValue(payload, "summerEnabled", true),
                WinterOutdoorTemperatureC = NumberValue(payload, "winterOutdoorTemperature", -10),
                WinterOutdoorRhPercent = NumberValue(payload, "winterOutdoorRh", 80),
                WinterReturnTemperatureC = NumberValue(payload, "winterReturnTemperature", 20),
                WinterReturnRhPercent = NumberValue(payload, "winterReturnRh", 60),
                SummerOutdoorTemperatureC = NumberValue(payload, "summerOutdoorTemperature", 32),
                SummerOutdoorRhPercent = NumberValue(payload, "summerOutdoorRh", 80),
                SummerReturnTemperatureC = NumberValue(payload, "summerReturnTemperature", 26),
                SummerReturnRhPercent = NumberValue(payload, "summerReturnRh", 50),
                WaterCoilEnabled = BooleanValue(payload, "waterCoilEnabled"),
                WaterCoilId = IntegerValue(payload, "waterCoilId", 0),
                WaterCoilMode = TextValue(payload, "waterCoilMode"),
                WaterCoilCustomized = BooleanValue(payload, "waterCoilCustomized"),
                WaterCoilLengthMm = IntegerValue(payload, "waterCoilLengthMm", 0),
                WaterCoilHeightMm = IntegerValue(payload, "waterCoilHeightMm", 0),
                WaterCoilRows = IntegerValue(payload, "waterCoilRows", 0),
                WaterCoilCircuits = IntegerValue(payload, "waterCoilCircuits", 0),
                WaterCoilFinSpacingMm = NumberValue(payload, "waterCoilFinSpacingMm", 0),
                FluidCode = TextValue(payload, "fluidCode"),
                GlycolPercent = NumberValue(payload, "glycolPercent", 10),
                CoolingWaterInletTemperatureC = NumberValue(payload, "coolingWaterInletTemperature", 7),
                CoolingWaterOutletTemperatureC = NumberValue(payload, "coolingWaterOutletTemperature", 12),
                HeatingWaterInletTemperatureC = NumberValue(payload, "heatingWaterInletTemperature", 80),
                HeatingWaterOutletTemperatureC = NumberValue(payload, "heatingWaterOutletTemperature", 70),
                ElectricPreheaterEnabled = BooleanValue(payload, "electricPreheaterEnabled"),
                ElectricPreheaterId = IntegerValue(payload, "electricPreheaterId", 0),
                ElectricPostheaterEnabled = BooleanValue(payload, "electricPostheaterEnabled"),
                ElectricPostheaterId = IntegerValue(payload, "electricPostheaterId", 0),
                AccessoryCodes = TextListValue(payload, "accessoryCodes")
            };
        }

        private void OpenLegacy()
        {
            Hide();
            using (var legacy = new CLMainForm())
                legacy.ShowDialog(this);
            Show();
        }

        private void OpenLegacy(
            CLNextUiCalculationInput input,
            bool generateReport,
            bool saveAs)
        {
            CLSelectionProjectDocument document =
                CLNextUiApplicationService.CreateProjectDocument(input);
            Hide();
            using (var legacy = new CLMainForm())
            {
                legacy.Shown += delegate
                {
                    if (generateReport)
                        legacy.Project_GenerateNextUiReport(document);
                    else if (saveAs)
                        legacy.Project_SaveNextUiDocument(document);
                    else
                        legacy.Project_ApplyNextUiDocument(document);
                };
                legacy.ShowDialog(this);
            }
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

        private static int IntegerValue(IDictionary<string, object> payload, string key, int fallback)
        {
            return Convert.ToInt32(Math.Round(NumberValue(payload, key, fallback)));
        }

        private static bool BooleanValue(
            IDictionary<string, object> payload,
            string key,
            bool fallback = false)
        {
            object value;
            if (!payload.TryGetValue(key, out value) || value == null) return fallback;
            bool parsed;
            if (Boolean.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out parsed))
                return parsed;
            return NumberValue(payload, key, 0) != 0;
        }

        private static List<string> TextListValue(
            IDictionary<string, object> payload,
            string key)
        {
            var result = new List<string>();
            object value;
            if (!payload.TryGetValue(key, out value) || value == null) return result;
            var enumerable = value as IEnumerable;
            if (enumerable == null || value is string) return result;
            foreach (object item in enumerable)
            {
                string text = Convert.ToString(item, CultureInfo.InvariantCulture);
                if (!String.IsNullOrWhiteSpace(text)) result.Add(text);
            }
            return result;
        }

        private sealed class BridgeRequest
        {
            public string RequestId { get; set; }
            public string Command { get; set; }
            public Dictionary<string, object> Payload { get; set; }
        }
    }
}
