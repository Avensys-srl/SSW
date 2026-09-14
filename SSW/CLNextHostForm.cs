using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SSW
{
    internal static class CLNextUiScreenshotCommand
    {
        public static int Run(string outputPath, string step)
        {
            if (String.IsNullOrWhiteSpace(outputPath)) return 2;

            string resolvedPath = Path.GetFullPath(outputPath);
            string directory = Path.GetDirectoryName(resolvedPath);
            if (!String.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            using (var form = new CLNextHostForm(resolvedPath, step))
            {
                Application.Run(form);
                return form.ScreenshotExitCode;
            }
        }
    }

    internal static class CLNextUiSmokeCommand
    {
        public static int Run()
        {
            var models = CLNextUiApplicationService.GetModels();
            if (models == null || models.Count == 0) return 10;
            if (CLEnvironment.Current.DatabaseCompatibility != null &&
                CLEnvironment.Current.DatabaseCompatibility.HasFeature("DimensionalDrawings"))
            {
                var primeModel = models.Find(item => item.Code.StartsWith(
                    "PRIME ", StringComparison.OrdinalIgnoreCase));
                if (primeModel == null) return 70;
                var drawing = CLDimensionalDrawingService.Resolve(primeModel.Code, null);
                if (!drawing.Available ||
                    String.IsNullOrWhiteSpace(drawing.ContentBase64) ||
                    drawing.Dimensions == null || drawing.Dimensions.Count != 4 ||
                    drawing.VisibleDimensions == null || drawing.VisibleDimensions.Count == 0 ||
                    drawing.PageWidthPoints <= 0 || drawing.PageHeightPoints <= 0)
                    return 71;
                string testPdf = Path.Combine(
                    Path.GetTempPath(),
                    "ssw-dimensional-drawing-" + Guid.NewGuid().ToString("N") + ".pdf");
                try
                {
                    byte[] testImage;
                    using (var bitmap = new Bitmap(320, 180))
                    using (var graphics = Graphics.FromImage(bitmap))
                    using (var imageStream = new MemoryStream())
                    {
                        graphics.Clear(Color.White);
                        graphics.DrawRectangle(Pens.Black, 35, 35, 250, 110);
                        bitmap.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
                        testImage = imageStream.ToArray();
                    }
                    CLNextHostForm.WriteDimensionalDrawingPdf(testPdf, testImage, drawing);
                    using (var reader = new iTextSharp.text.pdf.PdfReader(testPdf))
                    {
                        if (reader.NumberOfPages != 1) return 72;
                        string pdfText = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, 1);
                        string normalizedPdfText = System.Text.RegularExpressions.Regex.Replace(pdfText, @"\s+", " ");
                        foreach (CLDimensionalValue dimension in drawing.VisibleDimensions)
                        {
                            string expected = dimension.Code + " " +
                                dimension.ValueMillimeters.Value.ToString("0", CultureInfo.InvariantCulture) + " mm";
                            if (normalizedPdfText.IndexOf(expected, StringComparison.Ordinal) < 0) return 73;
                        }
                        if (drawing.UnitWeightKilograms.HasValue && drawing.UnitWeightKilograms.Value != 0)
                        {
                            string expectedWeight = "Peso " +
                                drawing.UnitWeightKilograms.Value.ToString("0", CultureInfo.InvariantCulture) + " kg";
                            if (normalizedPdfText.IndexOf(expectedWeight, StringComparison.Ordinal) < 0) return 74;
                        }
                        if (normalizedPdfText.IndexOf("Pallet", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            normalizedPdfText.IndexOf("Total", StringComparison.OrdinalIgnoreCase) >= 0)
                            return 75;
                    }
                }
                finally
                {
                    if (File.Exists(testPdf)) File.Delete(testPdf);
                }
            }
            var model = models.Find(item => item.Code == "CLRC 023 OSC") ?? models[0];
            var preselectionInput = new CLNextUiCalculationInput
            {
                SupplyAirflowM3h = Math.Max(1, model.NominalAirflowM3h),
                ExtractAirflowM3h = Math.Max(1, model.NominalAirflowM3h),
                PressurePa = Math.Max(0, model.StaticPressurePa)
            };
            var compatibleModels = CLNextUiApplicationService.Preselect(preselectionInput);
            if (compatibleModels == null || compatibleModels.Count == 0) return 21;
            if (compatibleModels.Count >= models.Count)
            {
                Console.Error.WriteLine(
                    "Preselection did not reduce the catalogue at {0} m3/h and {1} Pa: {2}/{3}.",
                    preselectionInput.SupplyAirflowM3h,
                    preselectionInput.PressurePa,
                    compatibleModels.Count,
                    models.Count);
                return 24;
            }
            if (compatibleModels.Exists(item =>
                item.RequiredRegulationPercent < 70 ||
                item.AvailablePressurePa <= 0 ||
                item.AvailablePressurePa < preselectionInput.PressurePa ||
                item.AbsorbedPowerW <= 0 ||
                item.CombinedSfp <= 0 ||
                double.IsNaN(item.CombinedSfp) ||
                double.IsInfinity(item.CombinedSfp))) return 22;
            var lowDutyPoint = new CLNextUiCalculationInput
            {
                SupplyAirflowM3h = 100,
                ExtractAirflowM3h = 100,
                PressurePa = 100
            };
            if (CLNextUiApplicationService.Preselect(lowDutyPoint).Exists(item =>
                item.AvailablePressurePa <= 0 ||
                item.AbsorbedPowerW <= 0 ||
                item.CombinedSfp <= 0 ||
                double.IsNaN(item.CombinedSfp) ||
                double.IsInfinity(item.CombinedSfp))) return 25;
            var lowDutyCandidates = CLNextUiApplicationService.Preselect(lowDutyPoint);
            if (lowDutyCandidates.Count == 0) return 26;
            var minimumSfp = lowDutyCandidates.Min(item => item.CombinedSfp);
            lowDutyPoint.PreselectionFilters = new CLNextUiPreselectionFilters
            {
                MaximumSfpEnabled = true,
                MaximumSfp = Math.Max(0, minimumSfp - 0.001)
            };
            if (CLNextUiApplicationService.Preselect(lowDutyPoint).Count != 0)
                return 67;

            lowDutyPoint.PreselectionFilters = new CLNextUiPreselectionFilters
            {
                SupplyNoiseEnabled = true,
                SupplyNoiseMetric = "LPA",
                MaximumSupplyNoiseDbA = 200,
                SupplyNoiseDirectivity = 4,
                SupplyNoiseDistanceMeters = 3,
                BreakoutNoiseEnabled = true,
                BreakoutNoiseMetric = "LWA",
                MaximumBreakoutNoiseDbA = 200
            };
            var acousticCandidates = CLNextUiApplicationService.Preselect(lowDutyPoint);
            if (acousticCandidates.Count == 0 || acousticCandidates.Exists(item =>
                !item.SupplySoundPowerDbA.HasValue ||
                !item.SupplySoundPressureDbA.HasValue ||
                !item.BreakoutSoundPowerDbA.HasValue ||
                !item.BreakoutSoundPressureDbA.HasValue ||
                item.SupplySoundPressureDbA.Value > 200 ||
                item.BreakoutSoundPowerDbA.Value > 200)) return 68;
            var minimumSupplyLpa = acousticCandidates.Min(
                item => item.SupplySoundPressureDbA.Value);
            lowDutyPoint.PreselectionFilters.MaximumSupplyNoiseDbA =
                Math.Max(0, minimumSupplyLpa - 0.1);
            if (CLNextUiApplicationService.Preselect(lowDutyPoint).Count != 0)
                return 69;
            var comparisonDutyPoint = new CLNextUiCalculationInput
            {
                SupplyAirflowM3h = 1000,
                ExtractAirflowM3h = 1000,
                PressurePa = 100
            };
            var comparisonCandidates =
                CLNextUiApplicationService.Preselect(comparisonDutyPoint);
            if (comparisonCandidates.Count == 0) return 28;
            if (comparisonCandidates.Exists(candidate =>
                candidate.Model.Code.StartsWith(
                    "CLRC 013 ",
                    StringComparison.OrdinalIgnoreCase))) return 32;
            if (SameCandidateOrder(
                CLNextUiApplicationService.Preselect(lowDutyPoint),
                comparisonCandidates)) return 29;
            foreach (var candidate in comparisonCandidates.GetRange(
                0, Math.Min(10, comparisonCandidates.Count)))
            {
                var candidateResult = CLNextUiApplicationService.Calculate(
                    new CLNextUiCalculationInput
                    {
                        ModelCode = candidate.Model.Code,
                        SupplyAirflowM3h = comparisonDutyPoint.SupplyAirflowM3h,
                        ExtractAirflowM3h = comparisonDutyPoint.ExtractAirflowM3h,
                        PressurePa = comparisonDutyPoint.PressurePa,
                        RegulationPercent = candidate.RequiredRegulationPercent
                    });
                if (!NearlyEqual(
                        candidate.AvailablePressurePa,
                        candidateResult.Winter.Curves.WorkingPointPressurePa,
                        0.11) ||
                    !NearlyEqual(
                        candidate.AbsorbedPowerW,
                        candidateResult.Winter.Curves.WorkingPointPowerW,
                        0.11) ||
                    !NearlyEqual(
                        candidate.CombinedSfp,
                        candidateResult.Winter.Result.
                            CombinedSpecificFanPowerWPerM3hPerSecond.GetValueOrDefault(),
                        0.01))
                    return 30;
            }
            preselectionInput.SupplyAirflowM3h = 1000000000000;
            preselectionInput.ExtractAirflowM3h = 1000000000000;
            if (CLNextUiApplicationService.Preselect(preselectionInput).Count != 0) return 23;
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
            if (result.Sound == null || result.Sound.Rows == null ||
                result.Sound.Rows.Count < 5 ||
                result.Sound.Rows.Exists(row =>
                    row.Bands == null || row.Bands.Length != 8 ||
                    double.IsNaN(row.LwA) || double.IsInfinity(row.LwA)))
                return 43;
            CLNextUiCalculationResult nonIsoSoundResult =
                CLNextUiApplicationService.Calculate(
                    new CLNextUiCalculationInput
                    {
                        ModelCode = model.Code,
                        SupplyAirflowM3h = Math.Max(
                            1, Math.Min(100, model.NominalAirflowM3h)),
                        ExtractAirflowM3h = Math.Max(
                            1, Math.Min(100, model.NominalAirflowM3h)),
                        PressurePa = 100,
                        Sound = new CLNextUiSoundInput
                        {
                            IncludeIso16032 = true
                        }
                    });
            if (nonIsoSoundResult.Sound.Iso16032Available ||
                nonIsoSoundResult.Sound.Rows.Exists(row =>
                    String.Equals(
                        row.Caption, "Lp@EN-ISO16032",
                        StringComparison.OrdinalIgnoreCase)))
                return 49;
            var isoSoundModel = models.Find(item =>
                item.Code.IndexOf("HCI", StringComparison.OrdinalIgnoreCase) >= 0 ||
                item.Code.IndexOf("FS", StringComparison.OrdinalIgnoreCase) >= 0);
            if (isoSoundModel != null)
            {
                CLNextUiCalculationResult isoSoundResult =
                    CLNextUiApplicationService.Calculate(
                        new CLNextUiCalculationInput
                        {
                            ModelCode = isoSoundModel.Code,
                            SupplyAirflowM3h = Math.Max(
                                1, Math.Min(100, isoSoundModel.NominalAirflowM3h)),
                            ExtractAirflowM3h = Math.Max(
                                1, Math.Min(100, isoSoundModel.NominalAirflowM3h)),
                            PressurePa = 100,
                            Sound = new CLNextUiSoundInput
                            {
                                IncludeIso16032 = true
                            }
                        });
                if (!isoSoundResult.Sound.Iso16032Available ||
                    (isoSoundResult.Sound.Rows.Count > 1 &&
                     !isoSoundResult.Sound.Rows.Exists(row =>
                         String.Equals(
                             row.Caption, "Lp@EN-ISO16032",
                             StringComparison.OrdinalIgnoreCase))))
                    return 50;
            }
            if (result.Co2 == null || result.Co2.Points == null ||
                result.Co2.Points.Count != 301 ||
                result.Co2.RequiredAirflowLitersPerSecond <= 0 ||
                result.Co2.RequiredAirflowM3h <= 0)
                return 44;
            if (!HasValidEfficiencyStart(result.Winter.Curves) ||
                !HasValidEfficiencyStart(result.Summer.Curves)) return 26;
            if (result.Layout == null) return 13;
            if (result.Layout.Configurations.Exists(item => item.Code == "A4") &&
                result.Layout.Configurations.Exists(item => item.Code == "B6"))
            {
                var a4Input = new CLNextUiCalculationInput
                {
                    ModelCode = model.Code,
                    SupplyAirflowM3h = 100,
                    ExtractAirflowM3h = 100,
                    PressurePa = 100,
                    LayoutCode = "A4"
                };
                var b6Input = new CLNextUiCalculationInput
                {
                    ModelCode = model.Code,
                    SupplyAirflowM3h = 100,
                    ExtractAirflowM3h = 100,
                    PressurePa = 100,
                    LayoutCode = "B6"
                };
                CLInstallationLayoutSnapshot a4 =
                    CLNextUiApplicationService.Calculate(a4Input).Layout;
                CLInstallationLayoutSnapshot b6 =
                    CLNextUiApplicationService.Calculate(b6Input).Layout;
                if (FlowPosition(a4, "Return") == FlowPosition(b6, "Return") ||
                    FlowPosition(a4, "Supply") == FlowPosition(b6, "Supply"))
                    return 27;
            }
            foreach (var layoutModel in new[]
            {
                models.Find(item => item.Code.IndexOf("SSC", StringComparison.OrdinalIgnoreCase) >= 0),
                models.Find(item => item.Code.IndexOf("OSC", StringComparison.OrdinalIgnoreCase) >= 0),
                models.Find(item =>
                    item.Code.IndexOf("SSC", StringComparison.OrdinalIgnoreCase) < 0 &&
                    item.Code.IndexOf("OSC", StringComparison.OrdinalIgnoreCase) < 0 &&
                    (item.Code.IndexOf(" VS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     item.Code.IndexOf(" FS", StringComparison.OrdinalIgnoreCase) >= 0))
            })
            {
                if (layoutModel == null) continue;
                CLInstallationLayoutSnapshot layout =
                    CLNextUiApplicationService.Calculate(
                        new CLNextUiCalculationInput
                        {
                            ModelCode = layoutModel.Code,
                            SupplyAirflowM3h = Math.Max(
                                1, Math.Min(100, layoutModel.NominalAirflowM3h)),
                            ExtractAirflowM3h = Math.Max(
                                1, Math.Min(100, layoutModel.NominalAirflowM3h)),
                            PressurePa = 100
                        }).Layout;
                if (!HasValidInstallationModes(layoutModel.Code, layout)) return 52;
                if (layout.Configurations.Count > 0 && !HasFourFlowPorts(layout)) return 53;
                if (layout.Configurations.Count == 0 &&
                    (!layout.DataIssues.Contains("LayoutConfigurationsMissing") ||
                     layout.FlowPorts.Any(port => port.Position.HasValue))) return 53;
            }
            if (result.AvailableWaterCoils == null ||
                result.AvailableElectricHeaters == null) return 15;
            foreach (string languageCode in new[]
            {
                "bg", "cs", "da", "de", "en", "fr", "hu", "is",
                "it", "nl", "no", "pl", "ro", "sl", "sv"
            })
            {
                CLNextUiCalculationResult localizedResult =
                    CLNextUiApplicationService.Calculate(
                        new CLNextUiCalculationInput
                        {
                            LanguageCode = languageCode,
                            ModelCode = model.Code,
                            SupplyAirflowM3h =
                                Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                            ExtractAirflowM3h =
                                Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                            PressurePa = 100
                        });
                string localizedText =
                    (localizedResult.WaterCoilStandardLabel ?? String.Empty) +
                    (localizedResult.WaterCoilCustomizedLabel ?? String.Empty) +
                    (localizedResult.PressureCapacityExceededMessage ?? String.Empty);
                if (String.IsNullOrWhiteSpace(localizedText) ||
                    localizedText.IndexOf(
                        "PrimaryCulture",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    return 36;
            }
            if (result.AvailableWaterCoils.Count > 0)
            {
                var coilInput = new CLNextUiCalculationInput
                {
                    LanguageCode = "it",
                    ModelCode = model.Code,
                    SupplyAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    ExtractAirflowM3h = Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                    PressurePa = 100,
                    RegulationPercent = 70,
                    WaterCoilEnabled = true,
                    WaterCoilId = result.AvailableWaterCoils[0].Id,
                    WaterCoilMode = result.AvailableWaterCoils[0].Mode
                };
                CLNextUiCalculationResult coilResult =
                    CLNextUiApplicationService.Calculate(coilInput);
                if (coilResult.WaterCoilResults.Count == 0) return 16;
                if (coilResult.EffectiveRegulationPercent < coilInput.RegulationPercent)
                    return 35;
                if (String.IsNullOrWhiteSpace(coilResult.WaterCoilStandardLabel) ||
                    coilResult.WaterCoilStandardLabel.IndexOf(
                        "PrimaryCulture", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    coilResult.AvailableWaterCoils.Exists(item =>
                        (item.InstallationLabel ?? String.Empty).IndexOf(
                            "PrimaryCulture", StringComparison.OrdinalIgnoreCase) >= 0))
                    return 33;
                if (!coilResult.PressureCapacityExceeded &&
                    coilResult.Winter.Curves.WorkingPointPressurePa + 0.5 <
                        coilInput.PressurePa)
                    return 34;
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
                CLSelectionProjectDocument heaterDocument =
                    CLNextUiApplicationService.CreateProjectDocument(
                        heaterInput, heaterResult);
                if (!CanPrepareReport(heaterDocument)) return 41;
            }
            CLSelectionProjectDocument bareDocument =
                CLNextUiApplicationService.CreateProjectDocument(
                    new CLNextUiCalculationInput
                    {
                        LanguageCode = "it",
                        ModelCode = model.Code,
                        SupplyAirflowM3h =
                            Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                        ExtractAirflowM3h =
                            Math.Max(1, Math.Min(100, model.NominalAirflowM3h)),
                        PressurePa = 100
                    });
            if (!CanPrepareReport(bareDocument)) return 42;
            var reportModel =
                models.Find(item => item.Code == "CLRC 163 SSC") ?? model;
            var documentInput = new CLNextUiCalculationInput
            {
                ProjectName = "Smoke",
                CustomerReference = "NEXT-UI",
                LanguageCode = "it",
                ModelCode = reportModel.Code,
                SupplyAirflowM3h =
                    Math.Max(1, Math.Min(1000, reportModel.NominalAirflowM3h)),
                ExtractAirflowM3h =
                    Math.Max(1, Math.Min(1000, reportModel.NominalAirflowM3h)),
                PressurePa = 100,
                InstallationMode = "Ceiling",
                LayoutCode = "B6",
                ImbalanceEnabled = false,
                AccessoryCodes = new List<string>(),
                Sound = new CLNextUiSoundInput
                {
                    IncludeInReport = true,
                    Directivity = 4,
                    Distance1Meters = 1.5,
                    Distance2Meters = 4,
                    IncludeIso16032 = true
                },
                PreselectionFilters = new CLNextUiPreselectionFilters
                {
                    MaximumSfpEnabled = true,
                    MaximumSfp = 1.75,
                    SupplyNoiseEnabled = true,
                    SupplyNoiseMetric = "LPA",
                    MaximumSupplyNoiseDbA = 48,
                    SupplyNoiseDirectivity = 4,
                    SupplyNoiseDistanceMeters = 2.5,
                    BreakoutNoiseEnabled = true,
                    BreakoutNoiseMetric = "LWA",
                    MaximumBreakoutNoiseDbA = 52
                },
                Co2 = new CLNextUiCo2Input
                {
                    IncludeInReport = true,
                    RoomHeightMeters = 3,
                    RoomLengthMeters = 8,
                    RoomWidthMeters = 7,
                    ActivityMet = 1.2,
                    PeopleDuringBreak = 0,
                    PeopleDuringPresence = 20,
                    BreakMinutes = 15,
                    PresenceMinutes = 45,
                    CalculationMethod = "MaximumCO2",
                    StandardPreset = "None",
                    OutdoorCo2Ppm = 380,
                    MaximumCo2Ppm = 1000,
                    AirflowPerPersonLitersPerSecond = 10,
                    AirflowPerAreaLitersPerSecondM2 = 0.35
                }
            };
            CLNextUiCalculationResult reportCalculation =
                CLNextUiApplicationService.Calculate(documentInput);
            var effectiveLayout = reportCalculation.Layout.Configurations.FirstOrDefault(
                item => item.Code == reportCalculation.Layout.ConfigurationCode);
            if (effectiveLayout == null) return 54;
            documentInput.LayoutCode = effectiveLayout.Code;
            documentInput.InstallationMode = effectiveLayout.InstallationMode;
            CLNextUiWaterCoilSummary reportCoil =
                reportCalculation.AvailableWaterCoils.Find(item =>
                    String.Equals(item.Mode, "HCD", StringComparison.OrdinalIgnoreCase));
            if (reportCoil != null)
            {
                documentInput.WaterCoilEnabled = true;
                documentInput.WaterCoilId = reportCoil.Id;
                documentInput.WaterCoilMode = "HCD";
            }
            CLSelectionProjectDocument document =
                CLNextUiApplicationService.CreateProjectDocument(documentInput);
            if (document.Selection.DimensionalDrawing == null ||
                document.Selection.DimensionalDrawing.Dimensions == null ||
                document.Selection.DimensionalDrawing.Dimensions.Count != 4)
                return 72;
            if (!HasValidAccessoryReport(document, reportCoil != null)) return 51;
            if (document.Selection.Unit.Code != reportModel.Code ||
                String.IsNullOrWhiteSpace(document.Selection.Unit.ManagementCode) ||
                document.Selection.CustomerReference != "NEXT-UI") return 17;
            string selectionJson = CLSelectionProjectSerializer.Serialize(document);
            CLSelectionProjectDocument restoredDocument =
                CLSelectionProjectSerializer.Deserialize(selectionJson);
            if (restoredDocument.Selection.Unit.Code != reportModel.Code ||
                restoredDocument.Selection.CustomerReference != "NEXT-UI" ||
                restoredDocument.Selection.ProjectName != "Smoke" ||
                restoredDocument.Selection.InstallationMode != documentInput.InstallationMode ||
                restoredDocument.Selection.LayoutCode != documentInput.LayoutCode ||
                restoredDocument.Selection.DimensionalDrawing == null ||
                restoredDocument.Selection.DimensionalDrawing.Dimensions == null ||
                restoredDocument.Selection.DimensionalDrawing.Dimensions.Count != 4 ||
                restoredDocument.Selection.Sound == null ||
                !restoredDocument.Selection.Sound.IncludeInReport ||
                restoredDocument.Selection.Sound.Directivity != 4 ||
                restoredDocument.Selection.PreselectionFilters == null ||
                !restoredDocument.Selection.PreselectionFilters.MaximumSfpEnabled ||
                restoredDocument.Selection.PreselectionFilters.MaximumSfp != 1.75 ||
                restoredDocument.Selection.PreselectionFilters.SupplyNoiseMetric != "LPA" ||
                restoredDocument.Selection.PreselectionFilters.SupplyNoiseDistanceMeters != 2.5 ||
                restoredDocument.Selection.PreselectionFilters.BreakoutNoiseMetric != "LWA" ||
                restoredDocument.Selection.Co2 == null ||
                !restoredDocument.Selection.Co2.IncludeInReport ||
                restoredDocument.Selection.Co2.PeopleDuringPresence != 20)
                return 18;
            CLNextUiCalculationInput restoredInput =
                CLNextUiApplicationService.CreateInputFromProjectDocument(
                    restoredDocument);
            if (restoredInput.ProjectName != documentInput.ProjectName ||
                restoredInput.ModelCode != documentInput.ModelCode ||
                restoredInput.LayoutCode != documentInput.LayoutCode ||
                restoredInput.SupplyAirflowM3h !=
                    documentInput.SupplyAirflowM3h ||
                restoredInput.Sound == null ||
                !restoredInput.Sound.IncludeInReport ||
                restoredInput.PreselectionFilters == null ||
                !restoredInput.PreselectionFilters.MaximumSfpEnabled ||
                restoredInput.PreselectionFilters.SupplyNoiseMetric != "LPA" ||
                restoredInput.PreselectionFilters.BreakoutNoiseMetric != "LWA" ||
                restoredInput.Co2 == null ||
                !restoredInput.Co2.IncludeInReport) return 37;
            if (!HasPopulatedIndoorQualityReport(restoredDocument)) return 45;
            if (!CanPrepareReport(restoredDocument)) return 31;
            if (!CanPrepareReportInLanguage(restoredDocument, "bg")) return 39;
            if (!CanPrepareReportInLanguage(restoredDocument, "no")) return 40;
            if (!CanRoundTripMultiSelectionProject(restoredDocument)) return 38;
            string json = new JavaScriptSerializer().Serialize(result);
            int invalidNumberCount;
            string normalizedJson = CLNextHostForm.NormalizeJsonNumbers(
                json, out invalidNumberCount);
            try
            {
                new JavaScriptSerializer().DeserializeObject(normalizedJson);
            }
            catch
            {
                return 20;
            }
            return String.IsNullOrWhiteSpace(normalizedJson) ||
                normalizedJson.Length < 100 ? 14 : 0;
        }

        private static bool CanRoundTripMultiSelectionProject(
            CLSelectionProjectDocument selection)
        {
            string directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Avensys",
                "SSW",
                "NextUiSmoke",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);
            string pdfPath = Path.Combine(directoryPath, "selection.pdf");
            string projectPath = Path.Combine(
                directoryPath,
                "selection" + CLMultiSelectionProjectSerializer.FileExtension);
            try
            {
                File.WriteAllBytes(
                    pdfPath,
                    new byte[]
                    {
                        0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34,
                        0x0A, 0x25, 0x25, 0x45, 0x4F, 0x46, 0x0A
                    });
                CLMultiSelectionProjectDocument project =
                    CLMultiSelectionProjectSerializer.CreateNew(
                        "Next UI smoke",
                        "cs");
                CLSelectionProjectDocument secondSelection =
                    CLSelectionProjectSerializer.Deserialize(
                        CLSelectionProjectSerializer.Serialize(selection));
                secondSelection.ProjectId = Guid.NewGuid();
                secondSelection.Selection.CustomerReference =
                    "NEXT-UI-SECOND";
                string originalLanguage =
                    selection.Selection.Report.LanguageCode;
                selection.Selection.Report.LanguageCode = "cs";
                secondSelection.Selection.Report.LanguageCode = "cs";
                try
                {
                    CLMultiSelectionProjectSerializer.AddOrUpdate(
                        project,
                        selection,
                        pdfPath,
                        "cs");
                    CLMultiSelectionProjectSerializer.AddOrUpdate(
                        project,
                        secondSelection,
                        pdfPath,
                        "cs");
                }
                finally
                {
                    selection.Selection.Report.LanguageCode =
                        originalLanguage;
                }
                CLMultiSelectionProjectSerializer.Save(projectPath, project);
                CLMultiSelectionProjectDocument restored =
                    CLMultiSelectionProjectSerializer.Load(projectPath);
                string html = CLMultiSelectionEmailComposer.BuildHtml(restored);
                bool initialRoundTripIsValid =
                    restored.Items.Count == 2 &&
                    restored.Items[0].SelectionProjectId == selection.ProjectId &&
                    restored.Items[1].SelectionProjectId ==
                        secondSelection.ProjectId &&
                    restored.Items[0].PdfBytes != null &&
                    restored.Items[0].PdfBytes.Length > 4 &&
                    html.IndexOf(
                        "Reference projektu",
                        StringComparison.Ordinal) >= 0;
                if (!initialRoundTripIsValid) return false;

                CLMultiSelectionProjectDocument translated =
                    CLMultiSelectionProjectSerializer.CreateNew(
                        restored.Reference,
                        "no");
                translated.ProjectId = restored.ProjectId;
                translated.CreatedAtUtc = restored.CreatedAtUtc;
                string translatedOriginalLanguage =
                    selection.Selection.Report.LanguageCode;
                selection.Selection.Report.LanguageCode = "no";
                secondSelection.Selection.Report.LanguageCode = "no";
                try
                {
                    CLMultiSelectionRegeneration.AddItem(
                        translated,
                        restored.Items[0],
                        selection,
                        pdfPath,
                        "no");
                    CLMultiSelectionRegeneration.AddItem(
                        translated,
                        restored.Items[1],
                        secondSelection,
                        pdfPath,
                        "no");
                }
                finally
                {
                    selection.Selection.Report.LanguageCode =
                        translatedOriginalLanguage;
                }
                CLMultiSelectionProjectSerializer.Save(
                    projectPath,
                    translated);
                CLMultiSelectionProjectDocument restoredTranslation =
                    CLMultiSelectionProjectSerializer.Load(projectPath);
                CLSelectionProjectDocument restoredSelection =
                    CLSelectionProjectSerializer.Deserialize(
                        restoredTranslation.Items[0].SelectionJson);
                string translatedSubject =
                    CLMultiSelectionEmailComposer.BuildSubject(
                        restoredTranslation);
                return
                    String.Equals(
                        restoredTranslation.LanguageCode,
                        "no",
                        StringComparison.OrdinalIgnoreCase) &&
                    restoredTranslation.Items.Count == 2 &&
                    String.Equals(
                        restoredTranslation.Items[0].LanguageCode,
                        "no",
                        StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(
                        restoredTranslation.Items[1].LanguageCode,
                        "no",
                        StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(
                        restoredSelection.Selection.Report.LanguageCode,
                        "no",
                        StringComparison.OrdinalIgnoreCase) &&
                    CLMultiSelectionProjectSerializer.IsCurrent(
                        restoredTranslation.Items[0],
                        "no") &&
                    CLMultiSelectionProjectSerializer.IsCurrent(
                        restoredTranslation.Items[1],
                        "no") &&
                    translatedSubject.StartsWith(
                        "Prosjekt - ",
                        StringComparison.Ordinal);
            }
            finally
            {
                try { Directory.Delete(directoryPath, true); }
                catch { }
            }
        }

        private static bool HasValidEfficiencyStart(
            CLPerformanceCurveCalculation curves)
        {
            if (curves == null ||
                curves.OriginalAirflows == null ||
                curves.EfficienciesPercent == null ||
                curves.OriginalAirflows.Length < 3 ||
                curves.OriginalAirflows.Length != curves.EfficienciesPercent.Length ||
                curves.OriginalAirflows[0] != 0) return false;

            double first = curves.EfficienciesPercent[0];
            double second = curves.EfficienciesPercent[1];
            return !double.IsNaN(first) &&
                !double.IsInfinity(first) &&
                first >= 0 &&
                first <= 100 &&
                first >= second;
        }

        private static bool SameCandidateOrder(
            IList<CLNextUiPreselectionSummary> left,
            IList<CLNextUiPreselectionSummary> right)
        {
            if (left == null || right == null || left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
            {
                if (!String.Equals(
                    left[index].Model.Code,
                    right[index].Model.Code,
                    StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        private static bool HasFourFlowPorts(CLInstallationLayoutSnapshot layout)
        {
            if (layout == null || layout.FlowPorts == null ||
                layout.FlowPorts.Count != 4) return false;
            string[] required = { "Fresh", "Return", "Supply", "Exhaust" };
            return required.All(flowCode => layout.FlowPorts.Count(item =>
                String.Equals(item.FlowCode, flowCode,
                    StringComparison.OrdinalIgnoreCase)) == 1) &&
                layout.FlowPorts.Select(item => item.Position ?? 0)
                    .OrderBy(position => position)
                    .SequenceEqual(new[] { 1, 2, 3, 4 });
        }

        private static bool NearlyEqual(double left, double right, double tolerance)
        {
            return !double.IsNaN(left) && !double.IsNaN(right) &&
                !double.IsInfinity(left) && !double.IsInfinity(right) &&
                Math.Abs(left - right) <= tolerance;
        }

        private static bool CanPrepareReportInLanguage(
            CLSelectionProjectDocument document,
            string languageCode)
        {
            string originalLanguage = document.Selection.Report.LanguageCode;
            try
            {
                document.Selection.Report.LanguageCode = languageCode;
                CLNextUiApplicationService.ApplyLanguage(languageCode);
                return
                    CanPrepareReport(document) &&
                    String.Equals(
                        CLEnvironment.Current.PrimaryLanguageCode,
                        languageCode,
                        StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                document.Selection.Report.LanguageCode = originalLanguage;
                CLNextUiApplicationService.ApplyLanguage(originalLanguage);
            }
        }
        private static bool CanPrepareReport(CLSelectionProjectDocument document)
        {
            CLPreparedNextUiReport prepared =
                CLNextUiReportService.Prepare(document);
            if (prepared == null ||
                String.IsNullOrWhiteSpace(prepared.ReportTemplate) ||
                prepared.DataSources == null ||
                prepared.DataSources.Count == 0)
                return ReportPreparationFailed("Prepared report is empty.");
            Microsoft.Reporting.WinForms.ReportDataSource header =
                prepared.DataSources.FirstOrDefault(source =>
                    String.Equals(
                        source.Name, "Header",
                        StringComparison.OrdinalIgnoreCase));
            var headerTable = header == null
                ? null
                : header.Value as System.Data.DataTable;
            if (headerTable == null || headerTable.Rows.Count != 1 ||
                headerTable.Columns["Date_Value"] == null ||
                headerTable.Columns["Date_Value"].DataType != typeof(DateTime) ||
                !(headerTable.Rows[0]["Date_Value"] is DateTime))
                return ReportPreparationFailed("Header date is not a DateTime value.");
            Microsoft.Reporting.WinForms.ReportDataSource diagram =
                prepared.DataSources.FirstOrDefault(source =>
                    String.Equals(
                        source.Name, "Diagram",
                        StringComparison.OrdinalIgnoreCase));
            var diagramTable = diagram == null
                ? null
                : diagram.Value as System.Data.DataTable;
            if (diagramTable == null || diagramTable.Rows.Count != 1 ||
                diagramTable.Columns["InstallationImage"] == null ||
                diagramTable.Columns["InstallationTitle"] == null ||
                diagramTable.Columns["InstallationConfigurationValue"] == null ||
                diagramTable.Columns["InstallationModeValue"] == null ||
                !(diagramTable.Rows[0]["InstallationImage"] is byte[]) ||
                ((byte[])diagramTable.Rows[0]["InstallationImage"]).Length < 1000 ||
                String.IsNullOrWhiteSpace(Convert.ToString(
                    diagramTable.Rows[0]["InstallationTitle"])) ||
                String.IsNullOrWhiteSpace(Convert.ToString(
                    diagramTable.Rows[0]["InstallationConfigurationValue"])) ||
                String.IsNullOrWhiteSpace(Convert.ToString(
                    diagramTable.Rows[0]["InstallationModeValue"])))
                return ReportPreparationFailed(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "Installation diagram data is missing or incomplete. " +
                        "table={0}; rows={1}; image={2}; title='{3}'; " +
                        "configuration='{4}'; installation='{5}'.",
                        diagramTable != null,
                        diagramTable == null ? -1 : diagramTable.Rows.Count,
                        diagramTable == null || diagramTable.Rows.Count == 0 ||
                            !(diagramTable.Rows[0]["InstallationImage"] is byte[])
                            ? -1
                            : ((byte[])diagramTable.Rows[0]["InstallationImage"]).Length,
                        diagramTable == null || diagramTable.Rows.Count == 0
                            ? String.Empty
                            : Convert.ToString(diagramTable.Rows[0]["InstallationTitle"]),
                        diagramTable == null || diagramTable.Rows.Count == 0
                            ? String.Empty
                            : Convert.ToString(diagramTable.Rows[0]["InstallationConfigurationValue"]),
                        diagramTable == null || diagramTable.Rows.Count == 0
                            ? String.Empty
                            : Convert.ToString(diagramTable.Rows[0]["InstallationModeValue"])));
            using (var report = new Microsoft.Reporting.WinForms.LocalReport())
            {
                report.ReportPath = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    prepared.ReportTemplate);
                foreach (Microsoft.Reporting.WinForms.ReportDataSource source
                    in prepared.DataSources)
                    report.DataSources.Add(source);
                byte[] pdf = report.Render("PDF");
                if (pdf == null || pdf.Length <= 1000)
                    return ReportPreparationFailed("Rendered PDF is empty.");
                return true;
            }
        }

        private static bool ReportPreparationFailed(string reason)
        {
            try
            {
                File.WriteAllText(
                    Path.Combine(
                        Path.GetDirectoryName(Application.ExecutablePath),
                        "next-ui-report-smoke-error.log"),
                    reason ?? "Unknown report preparation error.");
            }
            catch
            {
            }
            return false;
        }

        private static bool HasPopulatedIndoorQualityReport(
            CLSelectionProjectDocument document)
        {
            CLPreparedNextUiReport prepared =
                CLNextUiReportService.Prepare(document);
            if (prepared == null ||
                prepared.ReportTemplate == null ||
                prepared.ReportTemplate.IndexOf(
                    "WithCO2", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            Microsoft.Reporting.WinForms.ReportDataSource sound =
                prepared.DataSources.FirstOrDefault(source =>
                    String.Equals(
                        source.Name, "SoundPower",
                        StringComparison.OrdinalIgnoreCase));
            Microsoft.Reporting.WinForms.ReportDataSource co2 =
                prepared.DataSources.FirstOrDefault(source =>
                    String.Equals(
                        source.Name, "CO2LevelParameters",
                        StringComparison.OrdinalIgnoreCase));
            var soundTable = sound == null ? null : sound.Value as System.Data.DataTable;
            var co2Table = co2 == null ? null : co2.Value as System.Data.DataTable;
            return soundTable != null && soundTable.Rows.Count >= 5 &&
                co2Table != null && co2Table.Rows.Count == 1;
        }

        private static bool HasValidAccessoryReport(
            CLSelectionProjectDocument document,
            bool expectWaterCoil)
        {
            CLPreparedNextUiReport prepared =
                CLNextUiReportService.Prepare(document);
            Microsoft.Reporting.WinForms.ReportDataSource accessories =
                prepared.DataSources.FirstOrDefault(source =>
                    String.Equals(
                        source.Name, "AccessoryReport",
                        StringComparison.OrdinalIgnoreCase));
            var table = accessories == null
                ? null
                : accessories.Value as System.Data.DataTable;
            if (table == null || table.Rows.Count == 0) return false;
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (string column in new[]
                {
                    "Title", "CodeCaption", "DescriptionCaption",
                    "FunctionsCaption", "StatusCaption", "Status"
                })
                {
                    if (Convert.ToString(row[column]).Trim() == "?")
                        return false;
                }
            }
            return !expectWaterCoil ||
                table.Rows.Cast<System.Data.DataRow>().Any(row =>
                    String.Equals(
                        Convert.ToString(row["Code"]),
                        "HCD",
                        StringComparison.OrdinalIgnoreCase));
        }

        private static int FlowPosition(
            CLInstallationLayoutSnapshot layout,
            string flowCode)
        {
            CLFlowPortDefinition port = layout.FlowPorts.Find(item =>
                String.Equals(item.FlowCode, flowCode, StringComparison.OrdinalIgnoreCase));
            return port == null || !port.Position.HasValue ? 0 : port.Position.Value;
        }

        private static bool HasValidInstallationModes(
            string modelCode,
            CLInstallationLayoutSnapshot layout)
        {
            if (layout == null || layout.Configurations == null) return false;
            bool sameSide = modelCode.IndexOf(
                "SSC", StringComparison.OrdinalIgnoreCase) >= 0;
            var ceiling = sameSide
                ? new[] { "A2", "B2" }
                : new[] { "A4", "B6", "C4", "D4" };
            var floor = sameSide
                ? new[] { "A1", "A3", "B1", "B3" }
                : new[] { "A3", "B5", "C3", "D3" };
            var wall = sameSide
                ? new string[0]
                : new[] { "B1", "B2", "B3", "B4", "C1", "C2", "D1", "D2" };

            foreach (CLInstallationConfiguration item in layout.Configurations)
            {
                string expected = ceiling.Contains(item.Code)
                    ? "ceiling"
                    : floor.Contains(item.Code)
                        ? "floor"
                        : wall.Contains(item.Code) ? "wall" : null;
                if (expected != null && !String.Equals(
                    expected, item.InstallationMode,
                    StringComparison.OrdinalIgnoreCase)) return false;
                if (sameSide && String.Equals(
                    item.InstallationMode, "wall",
                    StringComparison.OrdinalIgnoreCase)) return false;
            }
            return true;
        }
    }

    internal static class CLMultiSelectionRegeneration
    {
        internal static void AddItem(
            CLMultiSelectionProjectDocument targetProject,
            CLMultiSelectionProjectItem sourceItem,
            CLSelectionProjectDocument selection,
            string pdfPath,
            string languageCode)
        {
            CLMultiSelectionProjectSerializer.AddOrUpdate(
                targetProject,
                selection,
                pdfPath,
                languageCode);
            CLMultiSelectionProjectItem targetItem =
                targetProject.Items.First(item =>
                    item.SelectionProjectId ==
                        sourceItem.SelectionProjectId);
            targetItem.ItemId = sourceItem.ItemId;
            targetItem.AddedAtUtc = sourceItem.AddedAtUtc;
        }
    }

    internal sealed class CLNextHostForm : Form
    {
        private const string VirtualHostName = "ssw.local";
        private readonly WebView2 webView;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private readonly string screenshotOutputPath;
        private readonly string screenshotStep;
        private readonly CLFollowUpReminderStore followUpStore =
            new CLFollowUpReminderStore();
        private readonly Dictionary<string, CLNextUiProductDocuments>
            productDocumentCache =
                new Dictionary<string, CLNextUiProductDocuments>(
                    StringComparer.OrdinalIgnoreCase);
        private CLSelectionProjectDocument currentSelectionDocument;
        private string currentSelectionPath;
        private CLMultiSelectionProjectDocument currentMultiSelectionDocument;
        private string currentMultiSelectionPath;
        private bool currentMultiSelectionDirty;
        private bool screenshotStarted;

        internal int ScreenshotExitCode { get; private set; }

        public CLNextHostForm()
            : this(null, null)
        {
        }

        internal CLNextHostForm(string screenshotOutputPath, string screenshotStep)
        {
            this.screenshotOutputPath = screenshotOutputPath;
            this.screenshotStep = screenshotStep;
            ScreenshotExitCode = String.IsNullOrWhiteSpace(screenshotOutputPath) ? 0 : 1;
            serializer.MaxJsonLength = Int32.MaxValue;
            serializer.RecursionLimit = 128;
            Text = CLSSWProfile.AssemblyTitle + " - UI Preview";
            if (CLEnvironment.Current != null &&
                CLEnvironment.Current.SSWInfo != null)
                Icon = CLEnvironment.Current.SSWInfo.EmbeddedIcon;
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
                webView.CoreWebView2.NavigationCompleted += async delegate(
                    object navigationSender,
                    CoreWebView2NavigationCompletedEventArgs navigationArgs)
                {
                    WriteDiagnostic(
                        "Navigation completed. Success=" + navigationArgs.IsSuccess +
                        ", status=" + navigationArgs.WebErrorStatus);
                    if (!navigationArgs.IsSuccess &&
                        !String.IsNullOrWhiteSpace(screenshotOutputPath) &&
                        !screenshotStarted)
                    {
                        screenshotStarted = true;
                        CompleteScreenshotWithError(new InvalidOperationException(
                            "WebView2 navigation failed: " +
                            navigationArgs.WebErrorStatus));
                        return;
                    }
                    if (navigationArgs.IsSuccess &&
                        !String.IsNullOrWhiteSpace(screenshotOutputPath) &&
                        !screenshotStarted)
                    {
                        screenshotStarted = true;
                        await CaptureScreenshotAndCloseAsync();
                    }
                };

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
                if (!String.IsNullOrWhiteSpace(screenshotOutputPath))
                {
                    CompleteScreenshotWithError(exception);
                    return;
                }

                MessageBox.Show(
                    this,
                    "The UI preview could not be started.\r\n\r\n" + exception.Message +
                    "\r\n\r\nThe application will now close.",
                    CLSSWProfile.AssemblyTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
        }

        private async Task CaptureScreenshotAndCloseAsync()
        {
            try
            {
                await WaitForFrontendReadyAsync();

                bool layoutReviewScenario =
                    String.Equals(screenshotStep, "layout-review", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(screenshotStep, "layout-review-accepted", StringComparison.OrdinalIgnoreCase);
                bool needsConfiguredWorkflow =
                    String.Equals(screenshotStep, "layout", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(screenshotStep, "layout-transitions", StringComparison.OrdinalIgnoreCase) ||
                    layoutReviewScenario ||
                    String.Equals(screenshotStep, "dimensional-drawing", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(screenshotStep, "co2", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(screenshotStep, "sound", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(screenshotStep, "documents", StringComparison.OrdinalIgnoreCase);
                if (needsConfiguredWorkflow)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "(function () {" +
                        "var search = document.querySelector('[data-step=\"preselection\"]');" +
                        "if (!search) return false;" +
                        "search.click();" +
                        "return true;" +
                        "})()");
                    await WaitForConditionAsync(
                        "document.querySelector('[data-select-unit]') !== null",
                        "The preselection results did not become ready.");
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "document.querySelector('[data-select-unit]').click()");
                    await WaitForConditionAsync(
                        "document.querySelector('.layout-preview') !== null",
                        "The Layout view did not become ready.");
                }

                if (String.Equals(screenshotStep, "layout-transitions", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string mode in new[] { "ceiling", "wall", "ceiling", "wall" })
                    {
                        await webView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('[data-installation=\"" + mode + "\"]:not([disabled])').click()");
                        await WaitForConditionAsync(
                            "document.querySelector('.airflow-diagram.installation-" + mode + "') !== null && document.querySelectorAll('.flow[data-port]').length === 4",
                            "Layout transition did not settle: " + mode);
                        string codesJson = await webView.CoreWebView2.ExecuteScriptAsync(
                            "Array.from(document.querySelector('#layoutCode').options).map(o => o.value)");
                        foreach (string code in new JavaScriptSerializer().Deserialize<string[]>(codesJson))
                        {
                            await webView.CoreWebView2.ExecuteScriptAsync(
                                "(function(){var select=document.querySelector('#layoutCode');select.value=" + new JavaScriptSerializer().Serialize(code) + ";select.dispatchEvent(new Event('change',{bubbles:true}));})()");
                            await WaitForConditionAsync(
                                "document.querySelector('.airflow-diagram') !== null && document.querySelector('#layoutCode').value === " + new JavaScriptSerializer().Serialize(code) +
                                " && new Set(Array.from(document.querySelectorAll('.flow')).map(p=>p.dataset.flow)).size === 4",
                                "Configuration transition did not settle: " + code);
                            string airflowGeometry = await webView.CoreWebView2.ExecuteScriptAsync(
                                "JSON.stringify(Array.from(document.querySelectorAll('.flow[data-port]')).map(function(f){" +
                                "var m=document.querySelector('.duct-marker[data-port=\"'+f.dataset.port+'\"]'),fr=f.getBoundingClientRect(),mr=m.getBoundingClientRect();" +
                                "return {port:f.dataset.port,flowX:fr.left,flowY:fr.top,ductX:mr.left+mr.width/2,ductY:mr.top+mr.height/2};}))");
                            await WaitForConditionAsync(
                                "(function(){var d=document.querySelector('.airflow-diagram');if(!d)return false;var horizontal=d.classList.contains('wall-east-west');" +
                                "return Array.from(document.querySelectorAll('.flow[data-port]')).every(function(f){" +
                                "var m=document.querySelector('.duct-marker[data-port=\"'+f.dataset.port+'\"]');if(!m)return false;" +
                                "var fr=f.getBoundingClientRect(),mr=m.getBoundingClientRect();" +
                                "return Math.abs((horizontal?fr.top:fr.left)-(horizontal?(mr.top+mr.height/2):(mr.left+mr.width/2)))<=1;});})()",
                                "Airflow symbol alignment failed: " + code + " " + airflowGeometry);
                        }
                    }
                    WindowState = FormWindowState.Normal;
                    ClientSize = new Size(1200, 1200);
                    await Task.Delay(300);
                }

                if (layoutReviewScenario)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "document.querySelector('[data-step=\"preselection\"]').click()");
                    await WaitForConditionAsync(
                        "document.querySelectorAll('[data-select-unit]').length > 1",
                        "A second preselection result is required for the Layout review screenshot.");
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "document.querySelectorAll('[data-select-unit]')[1].click()");
                    await WaitForConditionAsync(
                        "document.querySelector('[data-step=\"installation\"].attention') !== null && " +
                        "document.querySelector('.layout-confirmation-warning') !== null && " +
                        "document.querySelector('.layout-preview') !== null",
                        "The Layout review warning did not become ready.");
                    if (String.Equals(
                        screenshotStep,
                        "layout-review-accepted",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        await webView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('[data-step=\"summary\"]').click()");
                        await WaitForConditionAsync(
                            "document.querySelector('.summary-layout') !== null && " +
                            "document.querySelector('[data-step=\"installation\"].attention') === null && " +
                            "document.querySelectorAll('.step-button.skipped').length === 6",
                            "The accepted Layout review warning was not cleared.");
                    }
                }
                else if (String.Equals(
                    screenshotStep,
                        "preselection",
                        StringComparison.OrdinalIgnoreCase))
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "(function () {" +
                        "var button = document.querySelector('[data-step=\"preselection\"]');" +
                        "if (!button) return false;" +
                        "button.click();" +
                        "return true;" +
                        "})()");
                    await WaitForConditionAsync(
                        "document.querySelector('.additional-selection') !== null",
                        "The preselection filters did not become ready.");
                }
                else if (String.Equals(
                    screenshotStep,
                    "dimensional-drawing",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await WaitForConditionAsync(
                        "document.querySelector('[data-action=\"open-dimensional-drawing\"]:not([disabled])') !== null",
                        "The dimensional drawing preview did not become ready.");
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "document.querySelector('[data-action=\"open-dimensional-drawing\"]').click()");
                    await WaitForConditionAsync(
                        "document.querySelector('.dimensional-drawing-dialog') !== null && " +
                        "document.querySelector('[data-action=\"download-dimensional-drawing\"]') !== null",
                        "The dimensional drawing dialog did not become ready.");
                }
                else if (String.Equals(
                    screenshotStep,
                    "co2",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "(function () {" +
                        "var button = document.querySelector('[data-step=\"co2\"]');" +
                        "if (!button) return false;" +
                        "button.click();" +
                        "return true;" +
                        "})()");
                    await WaitForConditionAsync(
                        "document.querySelector('.co2-step') !== null && " +
                        "document.querySelector('.co2-result-grid') !== null",
                        "The CO2 calculation view did not become ready.");
                }
                else if (String.Equals(
                    screenshotStep,
                    "sound",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "(function () {" +
                        "var button = document.querySelector('[data-step=\"sound\"]');" +
                        "if (!button) return false;" +
                        "button.click();" +
                        "return true;" +
                        "})()");
                    await WaitForConditionAsync(
                        "document.querySelector('.sound-step') !== null && " +
                        "document.querySelectorAll('.sound-spectrum-table tbody tr').length >= 5",
                        "The sound calculation view did not become ready.");
                }
                else if (String.Equals(
                    screenshotStep,
                    "notifications",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "document.querySelector('[data-action=\"notifications\"]').click()");
                    await WaitForConditionAsync(
                        "document.querySelector('.notification-dialog') !== null",
                        "The integrated notification center did not become ready.");
                }
                else if (String.Equals(
                    screenshotStep,
                    "documents",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "(function () {" +
                        "var button = document.querySelector('[data-step=\"documents\"]');" +
                        "if (!button) return false;" +
                        "button.click();" +
                        "return true;" +
                        "})()");
                    await WaitForConditionAsync(
                        "document.querySelectorAll('.document-card').length === 2",
                        "The Documents view did not become ready.");
                }
                await webView.CoreWebView2.ExecuteScriptAsync(
                    "document.fonts && document.fonts.ready ? document.fonts.ready.then(() => true) : true");
                if (screenshotStep == "layout" || screenshotStep == "layout-transitions")
                {
                    await WaitForConditionAsync(
                        "Array.from(document.querySelectorAll('.flow[data-port]')).length === 4 && " +
                        "Array.from(document.querySelectorAll('.flow[data-port] img')).every(function(i){return i.complete&&i.naturalWidth>0;}) && " +
                        "document.querySelectorAll('.airflow-legend-item').length === 5",
                        "Airflow symbols or the fixed legend did not become ready.");
                }
                await Task.Delay(250);
                await CaptureFullPagePngAsync(screenshotOutputPath);
                ScreenshotExitCode = 0;
            }
            catch (Exception exception)
            {
                CompleteScreenshotWithError(exception);
                return;
            }

            BeginInvoke(new Action(Close));
        }

        private async Task WaitForFrontendReadyAsync()
        {
            await WaitForConditionAsync(
                "document.querySelector('.workspace') !== null && " +
                "document.querySelector('.loading-screen') === null",
                "The SSW Next frontend did not become ready.");
        }

        private async Task WaitForConditionAsync(string expression, string timeoutMessage)
        {
            const int maximumAttempts = 150;
            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                string result = await webView.CoreWebView2.ExecuteScriptAsync(expression);
                if (String.Equals(result, "true", StringComparison.OrdinalIgnoreCase))
                    return;
                await Task.Delay(100);
            }
            throw new TimeoutException(timeoutMessage);
        }

        private async Task CaptureFullPagePngAsync(string outputPath)
        {
            string metricsJson =
                await webView.CoreWebView2.CallDevToolsProtocolMethodAsync(
                    "Page.getLayoutMetrics",
                    "{}");
            var metrics = serializer.Deserialize<Dictionary<string, object>>(metricsJson);
            var contentSize = metrics["contentSize"] as Dictionary<string, object>;
            if (contentSize == null)
                throw new InvalidOperationException("WebView2 did not return the page dimensions.");

            double width = Convert.ToDouble(
                contentSize["width"],
                CultureInfo.InvariantCulture);
            double height = Convert.ToDouble(
                contentSize["height"],
                CultureInfo.InvariantCulture);
            string captureArguments = serializer.Serialize(new
            {
                format = "png",
                fromSurface = true,
                captureBeyondViewport = true,
                clip = new
                {
                    x = 0,
                    y = 0,
                    width = Math.Max(1, width),
                    height = Math.Max(1, height),
                    scale = 1
                }
            });
            string captureJson =
                await webView.CoreWebView2.CallDevToolsProtocolMethodAsync(
                    "Page.captureScreenshot",
                    captureArguments);
            var capture = serializer.Deserialize<Dictionary<string, object>>(captureJson);
            string encodedPng = capture["data"] as string;
            if (String.IsNullOrWhiteSpace(encodedPng))
                throw new InvalidOperationException("WebView2 returned an empty screenshot.");

            File.WriteAllBytes(outputPath, Convert.FromBase64String(encodedPng));
        }

        private object DownloadDimensionalDrawing(Dictionary<string, object> payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            string modelCode = TextValue(payload, "modelCode");
            string layoutCode = TextValue(payload, "layoutCode");
            string encodedImage = TextValue(payload, "imageBase64");
            if (String.IsNullOrWhiteSpace(encodedImage))
                throw new InvalidDataException("Dimensional drawing image is missing.");

            CLDimensionalDrawingResult drawing =
                CLDimensionalDrawingService.Resolve(modelCode, layoutCode, false);
            if (!drawing.Available)
                throw new InvalidOperationException("Dimensional drawing is not available.");

            byte[] imageBytes = Convert.FromBase64String(encodedImage);

            string outputPath;
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "SSW";
                dialog.Filter = "PDF (*.pdf)|*.pdf";
                dialog.DefaultExt = "pdf";
                dialog.AddExtension = true;
                dialog.FileName = SafeFileName(
                    modelCode + "_dimensional_drawing",
                    "dimensional_drawing") + ".pdf";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return new { saved = false, cancelled = true };
                outputPath = dialog.FileName;
            }

            WriteDimensionalDrawingPdf(outputPath, imageBytes, drawing);

            return new
            {
                saved = true,
                fileName = Path.GetFileName(outputPath)
            };
        }

        internal static void WriteDimensionalDrawingPdf(
            string outputPath,
            byte[] imageBytes,
            CLDimensionalDrawingResult drawing)
        {
            if (String.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path is required.", "outputPath");
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Drawing image is required.", "imageBytes");
            if (drawing == null || drawing.VisibleDimensions == null)
                throw new InvalidDataException("Dimensional values are required.");

            iTextSharp.text.Image drawingImage = iTextSharp.text.Image.GetInstance(imageBytes);
            iTextSharp.text.Rectangle pageSize = iTextSharp.text.PageSize.A4.Rotate();
            using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var document = new iTextSharp.text.Document(pageSize, 28, 28, 28, 28);
                try
                {
                    iTextSharp.text.pdf.PdfWriter writer =
                        iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
                    document.Open();

                    int legendRows = drawing.VisibleDimensions.Count +
                        (drawing.UnitWeightKilograms.HasValue && drawing.UnitWeightKilograms.Value != 0 ? 1 : 0);
                    float legendHeight = Math.Max(42f, legendRows * 20f + 8f);
                    float drawingBottom = document.BottomMargin + legendHeight;
                    float drawingWidth = pageSize.Width - document.LeftMargin - document.RightMargin;
                    float drawingHeight = pageSize.Height - document.TopMargin - drawingBottom;
                    drawingImage.ScaleToFit(drawingWidth, drawingHeight);
                    drawingImage.SetAbsolutePosition(
                        (pageSize.Width - drawingImage.ScaledWidth) / 2f,
                        drawingBottom + (drawingHeight - drawingImage.ScaledHeight) / 2f);
                    document.Add(drawingImage);

                    var legend = new iTextSharp.text.pdf.PdfPTable(2);
                    legend.TotalWidth = 170f;
                    legend.LockedWidth = true;
                    legend.SetWidths(new float[] { 1f, 1.65f });
                    iTextSharp.text.Font labelFont =
                        iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 9f);
                    iTextSharp.text.Font valueFont =
                        iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 9f);
                    foreach (CLDimensionalValue dimension in drawing.VisibleDimensions)
                    {
                        var labelCell = new iTextSharp.text.pdf.PdfPCell(
                            new iTextSharp.text.Phrase(dimension.Code, labelFont));
                        labelCell.BackgroundColor = new iTextSharp.text.BaseColor(237, 243, 240);
                        labelCell.Padding = 5f;
                        var valueCell = new iTextSharp.text.pdf.PdfPCell(
                            new iTextSharp.text.Phrase(
                                dimension.ValueMillimeters.HasValue
                                    ? dimension.ValueMillimeters.Value.ToString("0", CultureInfo.InvariantCulture) + " mm"
                                    : "-",
                                valueFont));
                        valueCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                        valueCell.Padding = 5f;
                        legend.AddCell(labelCell);
                        legend.AddCell(valueCell);
                    }
                    if (drawing.UnitWeightKilograms.HasValue && drawing.UnitWeightKilograms.Value != 0)
                    {
                        var labelCell = new iTextSharp.text.pdf.PdfPCell(
                            new iTextSharp.text.Phrase("Peso", labelFont));
                        labelCell.BackgroundColor = new iTextSharp.text.BaseColor(237, 243, 240);
                        labelCell.Padding = 5f;
                        var valueCell = new iTextSharp.text.pdf.PdfPCell(
                            new iTextSharp.text.Phrase(
                                drawing.UnitWeightKilograms.Value.ToString("0", CultureInfo.InvariantCulture) + " kg",
                                valueFont));
                        valueCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                        valueCell.Padding = 5f;
                        legend.AddCell(labelCell);
                        legend.AddCell(valueCell);
                    }
                    legend.WriteSelectedRows(
                        0,
                        -1,
                        pageSize.Width - document.RightMargin - legend.TotalWidth,
                        document.BottomMargin + legendHeight - 4f,
                        writer.DirectContent);
                }
                finally
                {
                    document.Close();
                }
            }
        }

        private void CompleteScreenshotWithError(Exception exception)
        {
            ScreenshotExitCode = 3;
            try
            {
                File.WriteAllText(
                    screenshotOutputPath + ".error.log",
                    exception.ToString());
            }
            catch
            {
            }
            BeginInvoke(new Action(Close));
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs eventArgs)
        {
            BridgeRequest request = null;
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                request = serializer.Deserialize<BridgeRequest>(eventArgs.WebMessageAsJson);
                if (request == null || String.IsNullOrWhiteSpace(request.Command))
                    throw new InvalidOperationException("Bridge command is missing.");
                WriteDiagnostic("Bridge request started: " + request.Command);

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
                    case "selection.preselect":
                        payload = PreselectModels(request.Payload);
                        break;
                    case "project.edit":
                        payload = SaveSelection(request.Payload, false);
                        break;
                    case "project.saveAs":
                        payload = SaveSelection(request.Payload, true);
                        break;
                    case "project.open":
                        payload = OpenSelection();
                        break;
                    case "project.workspace":
                        payload = GetMultiSelectionProject(request.Payload);
                        break;
                    case "project.workspaceNew":
                        payload = NewMultiSelectionProject(request.Payload);
                        break;
                    case "project.workspaceOpen":
                        payload = OpenMultiSelectionProject();
                        break;
                    case "project.workspaceSave":
                        payload = SaveMultiSelectionProject(request.Payload, false);
                        break;
                    case "project.workspaceSaveAs":
                        payload = SaveMultiSelectionProject(request.Payload, true);
                        break;
                    case "project.workspaceAddCurrent":
                        payload = AddCurrentSelectionToProject(request.Payload);
                        break;
                    case "project.workspaceRemove":
                        payload = RemoveMultiSelectionItem(request.Payload);
                        break;
                    case "project.workspaceOpenItem":
                        payload = OpenMultiSelectionItem(request.Payload);
                        break;
                    case "project.workspaceLanguage":
                        payload = ChangeMultiSelectionLanguage(request.Payload);
                        break;
                    case "project.workspaceEmail":
                        payload = EmailMultiSelectionProject(request.Payload);
                        break;
                    case "report.generate":
                        BeginInvoke(new Action(delegate
                        {
                            OpenNextReport(CreateInput(request.Payload));
                        }));
                        payload = new { opened = true, delegated = true };
                        break;
                    case "documents.list":
                        payload = ResolveProductDocuments(request.Payload, false);
                        break;
                    case "documents.open":
                        payload = ResolveProductDocuments(request.Payload, true);
                        break;
                    case "drawing.get":
                        if (request.Payload == null) throw new ArgumentNullException("payload");
                        payload = CLDimensionalDrawingService.Resolve(
                            TextValue(request.Payload, "modelCode"),
                            TextValue(request.Payload, "layoutCode"));
                        break;
                    case "drawing.download":
                        payload = DownloadDimensionalDrawing(request.Payload);
                        break;
                    case "notifications.open":
                    case "notifications.list":
                        payload = BuildNotificationPayload(true);
                        break;
                    case "notifications.peek":
                        payload = BuildNotificationPayload(false);
                        break;
                    case "notifications.action":
                        payload = ApplyNotificationAction(request.Payload);
                        break;
                    case "notifications.openTarget":
                        payload = OpenNotificationTarget(request.Payload);
                        break;
                    case "app.close":
                        BeginInvoke(new Action(Close));
                        payload = new { closed = true };
                        break;
                    case "app.clientError":
                        WriteDiagnostic(
                            "Frontend error: " + TextValue(request.Payload, "message"));
                        payload = new { logged = true };
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported bridge command: " + request.Command);
                }
                PostResponse(request.RequestId, true, payload, null);
                WriteDiagnostic(
                    "Bridge request completed: " + request.Command +
                    " (" + stopwatch.ElapsedMilliseconds + " ms)");
            }
            catch (Exception exception)
            {
                WriteDiagnostic(
                    "Bridge request failed: " +
                    (request == null ? "(unknown)" : request.Command) +
                    " (" + stopwatch.ElapsedMilliseconds + " ms)\r\n" +
                    exception);
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
                legacyAvailable = false,
                measureUnit = "SI",
                models = CLNextUiApplicationService.GetModels()
            };
        }

        private static object CalculateSelection(Dictionary<string, object> payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return CLNextUiApplicationService.Calculate(CreateInput(payload));
        }

        private static object PreselectModels(Dictionary<string, object> payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return CLNextUiApplicationService.Preselect(CreateInput(payload));
        }

        private object ResolveProductDocuments(
            Dictionary<string, object> payload,
            bool openRequestedDocument)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            CLNextUiCalculationInput input = CreateInput(payload);
            string cacheKey =
                (input.ModelCode ?? String.Empty).Trim().ToUpperInvariant() +
                "|" + NormalizeLanguageCode(input.LanguageCode);
            CLNextUiProductDocuments resolved;
            if (!productDocumentCache.TryGetValue(cacheKey, out resolved))
            {
                CLSelectionProjectDocument document =
                    CLNextUiApplicationService.CreateProjectDocument(input);
                resolved = CLProductDocumentService.Resolve(
                    document,
                    input.LanguageCode,
                    CLSSWProfile.ShortName);
                if (resolved == null)
                    throw new InvalidOperationException(
                        "Product documents could not be resolved.");
                productDocumentCache[cacheKey] = resolved;
            }

            string documentType = TextValue(payload, "documentType");
            string requestedPath =
                String.Equals(
                    documentType,
                    "commercial-sheet",
                    StringComparison.OrdinalIgnoreCase)
                    ? resolved.CommercialSheetPath
                    : String.Equals(
                        documentType,
                        "installation-manual",
                        StringComparison.OrdinalIgnoreCase)
                        ? resolved.InstallationManualPath
                        : String.Empty;
            bool available = !String.IsNullOrWhiteSpace(requestedPath) &&
                File.Exists(requestedPath);
            bool opened = false;
            if (openRequestedDocument && available)
            {
                Process.Start(new ProcessStartInfo(requestedPath)
                {
                    UseShellExecute = true
                });
                opened = true;
            }
            return new
            {
                commercialSheetAvailable =
                    !String.IsNullOrWhiteSpace(resolved.CommercialSheetPath) &&
                    File.Exists(resolved.CommercialSheetPath),
                installationManualAvailable =
                    !String.IsNullOrWhiteSpace(resolved.InstallationManualPath) &&
                    File.Exists(resolved.InstallationManualPath),
                opened,
                available
            };
        }

        private static CLNextUiCalculationInput CreateInput(
            Dictionary<string, object> payload)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            IDictionary<string, object> sound =
                DictionaryValue(payload, "sound");
            IDictionary<string, object> preselectionFilters =
                DictionaryValue(payload, "preselectionFilters");
            IDictionary<string, object> co2 =
                DictionaryValue(payload, "co2");
            string co2Method = TextValue(co2, "calculationMethod");
            switch ((co2Method ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "fixed-airflow":
                    co2Method = "FixedAirflow";
                    break;
                case "airflow-per-person-and-area":
                    co2Method = "PersonAndArea";
                    break;
                default:
                    co2Method = "MaximumCO2";
                    break;
            }
            return new CLNextUiCalculationInput
            {
                ProjectName = TextValue(payload, "projectName"),
                CustomerReference = TextValue(payload, "customerReference"),
                LanguageCode = TextValue(payload, "languageCode"),
                ModelCode = TextValue(payload, "modelCode"),
                SupplyAirflowM3h = NumberValue(payload, "supplyAirflow", 0),
                ExtractAirflowM3h = NumberValue(payload, "extractAirflow", 0),
                ImbalanceEnabled = BooleanValue(payload, "imbalanceEnabled"),
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
                WaterCoilCustomDisclaimerAccepted = BooleanValue(payload, "waterCoilCustomDisclaimerAccepted"),
                WaterCoilLengthMm = IntegerValue(payload, "waterCoilLengthMm", 0),
                WaterCoilHeightMm = IntegerValue(payload, "waterCoilHeightMm", 0),
                WaterCoilRows = IntegerValue(payload, "waterCoilRows", 0),
                WaterCoilCircuits = IntegerValue(payload, "waterCoilCircuits", 0),
                WaterCoilFinSpacingMm = NumberValue(payload, "waterCoilFinSpacingMm", 0),
                InstallationMode = TextValue(payload, "installationMode"),
                LayoutCode = TextValue(payload, "layoutCode"),
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
                AccessoryCodes = TextListValue(payload, "accessoryCodes"),
                Sound = new CLNextUiSoundInput
                {
                    IncludeInReport = BooleanValue(sound, "includeInReport"),
                    Directivity = IntegerValue(sound, "directivityFactor", 2),
                    Distance1Meters = NumberValue(sound, "distance1Meters", 1),
                    Distance2Meters = NumberValue(sound, "distance2Meters", 3),
                    IncludeIso16032 = BooleanValue(sound, "iso16032Enabled")
                },
                PreselectionFilters = new CLNextUiPreselectionFilters
                {
                    MaximumSfpEnabled = BooleanValue(preselectionFilters, "maximumSfpEnabled"),
                    MaximumSfp = NumberValue(preselectionFilters, "maximumSfp", 2),
                    SupplyNoiseEnabled = BooleanValue(preselectionFilters, "supplyNoiseEnabled"),
                    SupplyNoiseMetric = TextValue(preselectionFilters, "supplyNoiseMetric"),
                    MaximumSupplyNoiseDbA = NumberValue(preselectionFilters, "maximumSupplyNoiseDbA", 50),
                    SupplyNoiseDirectivity = IntegerValue(preselectionFilters, "supplyNoiseDirectivityFactor", 2),
                    SupplyNoiseDistanceMeters = NumberValue(preselectionFilters, "supplyNoiseDistanceMeters", 1),
                    BreakoutNoiseEnabled = BooleanValue(preselectionFilters, "breakoutNoiseEnabled"),
                    BreakoutNoiseMetric = TextValue(preselectionFilters, "breakoutNoiseMetric"),
                    MaximumBreakoutNoiseDbA = NumberValue(preselectionFilters, "maximumBreakoutNoiseDbA", 50),
                    BreakoutNoiseDirectivity = IntegerValue(preselectionFilters, "breakoutNoiseDirectivityFactor", 2),
                    BreakoutNoiseDistanceMeters = NumberValue(preselectionFilters, "breakoutNoiseDistanceMeters", 1)
                },
                Co2 = new CLNextUiCo2Input
                {
                    IncludeInReport = BooleanValue(co2, "includeInReport"),
                    RoomHeightMeters = NumberValue(co2, "roomHeightMeters", 3),
                    RoomLengthMeters = NumberValue(co2, "roomLengthMeters", 8),
                    RoomWidthMeters = NumberValue(co2, "roomWidthMeters", 7),
                    ActivityMet = NumberValue(co2, "activityMet", 1.2),
                    PeopleDuringBreak = NumberValue(co2, "breakPeople", 0),
                    PeopleDuringPresence = NumberValue(co2, "occupiedPeople", 20),
                    BreakMinutes = NumberValue(co2, "breakMinutes", 15),
                    PresenceMinutes = NumberValue(co2, "occupiedMinutes", 45),
                    CalculationMethod = co2Method,
                    StandardPreset = "None",
                    OutdoorCo2Ppm =
                        NumberValue(co2, "outdoorConcentrationPpm", 380),
                    MaximumCo2Ppm =
                        NumberValue(co2, "maximumConcentrationPpm", 1000),
                    FixedAirflowLitersPerSecond =
                        NumberValue(co2, "fixedAirflowLitersPerSecond", 0),
                    AirflowPerPersonLitersPerSecond =
                        NumberValue(
                            co2, "airflowPerPersonLitersPerSecond", 10),
                    AirflowPerAreaLitersPerSecondM2 =
                        NumberValue(
                            co2,
                            "airflowPerAreaLitersPerSecondPerSquareMeter",
                            0.35)
                }
            };
        }

        private object SaveSelection(
            Dictionary<string, object> payload,
            bool saveAs)
        {
            CLSelectionProjectDocument document =
                CLNextUiApplicationService.CreateProjectDocument(CreateInput(payload));
            if (currentSelectionDocument != null)
            {
                document.ProjectId = currentSelectionDocument.ProjectId;
                document.CreatedAtUtc = currentSelectionDocument.CreatedAtUtc;
                document.Identity = currentSelectionDocument.Identity;
                document.RevisionTracking = currentSelectionDocument.RevisionTracking;
            }
            if (document.Identity == null)
                document.Identity = new CLSelectionIdentity();
            if (String.IsNullOrWhiteSpace(document.Identity.LocalDraftReference))
                document.Identity.LocalDraftReference =
                    CLSelectionInstallationStateStore.NextDraftReference();

            string targetPath = currentSelectionPath;
            if (saveAs || String.IsNullOrWhiteSpace(targetPath))
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Title = "SSW - " + document.Identity.LocalDraftReference;
                    dialog.Filter =
                        "SSW technical selection (*" +
                        CLSelectionProjectSerializer.FileExtension + ")|*" +
                        CLSelectionProjectSerializer.FileExtension;
                    dialog.DefaultExt =
                        CLSelectionProjectSerializer.FileExtension.TrimStart('.');
                    dialog.AddExtension = true;
                    dialog.FileName = CLSelectionFileName.BuildSuggestedName(
                        document.Selection.Unit == null
                            ? "selection"
                            : document.Selection.Unit.Name,
                        document.Selection.CustomerReference);
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return new
                        {
                            saved = false,
                            cancelled = true,
                            path = currentSelectionPath ?? String.Empty
                        };
                    }
                    targetPath = dialog.FileName;
                }
            }

            CLSelectionProjectSerializer.Save(targetPath, document);
            currentSelectionDocument = document;
            currentSelectionPath = Path.GetFullPath(targetPath);
            return BuildProjectState(true);
        }

        private object OpenSelection()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "SSW";
                dialog.Filter =
                    "SSW technical selection (*" +
                    CLSelectionProjectSerializer.FileExtension + ")|*" +
                    CLSelectionProjectSerializer.FileExtension;
                dialog.DefaultExt =
                    CLSelectionProjectSerializer.FileExtension.TrimStart('.');
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return new { opened = false, cancelled = true };

                return LoadSelection(dialog.FileName);
            }
        }

        private object LoadSelection(string path)
        {
            currentSelectionDocument = CLSelectionProjectSerializer.Load(path);
            currentSelectionPath = Path.GetFullPath(path);
            CLNextUiCalculationInput input =
                CLNextUiApplicationService.CreateInputFromProjectDocument(
                    currentSelectionDocument);
            return new
            {
                opened = true,
                input,
                project = BuildProjectState(false)
            };
        }

        private object GetMultiSelectionProject(Dictionary<string, object> payload)
        {
            EnsureMultiSelectionProject(
                TextValue(payload, "reference"),
                TextValue(payload, "languageCode"));
            return BuildMultiSelectionProjectState();
        }

        private object NewMultiSelectionProject(Dictionary<string, object> payload)
        {
            currentMultiSelectionDocument =
                CLMultiSelectionProjectSerializer.CreateNew(
                    RequiredProjectReference(TextValue(payload, "reference")),
                    NormalizeLanguageCode(TextValue(payload, "languageCode")));
            currentMultiSelectionPath = null;
            currentMultiSelectionDirty = true;
            return BuildMultiSelectionProjectState();
        }

        private object OpenMultiSelectionProject()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "SSW";
                dialog.Filter = MultiSelectionProjectFilter();
                dialog.DefaultExt =
                    CLMultiSelectionProjectSerializer.FileExtension.TrimStart('.');
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return new { opened = false, cancelled = true };

                currentMultiSelectionDocument =
                    CLMultiSelectionProjectSerializer.Load(dialog.FileName);
                currentMultiSelectionPath = Path.GetFullPath(dialog.FileName);
                currentMultiSelectionDirty = false;
                return new
                {
                    opened = true,
                    project = BuildMultiSelectionProjectState()
                };
            }
        }

        private object SaveMultiSelectionProject(
            Dictionary<string, object> payload,
            bool saveAs)
        {
            string languageCode =
                NormalizeLanguageCode(TextValue(payload, "languageCode"));
            EnsureMultiSelectionProject(
                TextValue(payload, "reference"),
                languageCode);
            RegenerateMultiSelectionReports(languageCode);
            UpdateMultiSelectionMetadata(payload);
            if (!TryResolveMultiSelectionPath(saveAs))
                return new
                {
                    saved = false,
                    cancelled = true,
                    project = BuildMultiSelectionProjectState()
                };

            CLMultiSelectionProjectSerializer.Save(
                currentMultiSelectionPath,
                currentMultiSelectionDocument);
            currentMultiSelectionDirty = false;
            return new
            {
                saved = true,
                cancelled = false,
                project = BuildMultiSelectionProjectState()
            };
        }

        private object AddCurrentSelectionToProject(
            Dictionary<string, object> payload)
        {
            CLNextUiCalculationInput input = CreateInput(payload);
            string interfaceLanguage = input.LanguageCode;
            EnsureMultiSelectionProject(input.ProjectName, input.LanguageCode);
            UpdateMultiSelectionMetadata(payload);
            input.LanguageCode =
                currentMultiSelectionDocument.LanguageCode;

            try
            {
                CLSelectionProjectDocument selection =
                    CLNextUiApplicationService.CreateProjectDocument(input);
                PreserveCurrentSelectionIdentity(selection);
                string pdfPath = PrepareNextReportPdf(selection, input);
                CLMultiSelectionProjectSerializer.AddOrUpdate(
                    currentMultiSelectionDocument,
                    selection,
                    pdfPath,
                    currentMultiSelectionDocument.LanguageCode);
                currentSelectionDocument = selection;
                currentSelectionPath = null;
                currentMultiSelectionDirty = true;

                if (!String.IsNullOrWhiteSpace(currentMultiSelectionPath))
                {
                    CLMultiSelectionProjectSerializer.Save(
                        currentMultiSelectionPath,
                        currentMultiSelectionDocument);
                    currentMultiSelectionDirty = false;
                }
                return BuildMultiSelectionProjectState();
            }
            finally
            {
                CLNextUiApplicationService.ApplyLanguage(interfaceLanguage);
            }
        }

        private object RemoveMultiSelectionItem(
            Dictionary<string, object> payload)
        {
            EnsureMultiSelectionProject(null, null);
            Guid itemId;
            if (!Guid.TryParse(TextValue(payload, "itemId"), out itemId))
                throw new InvalidOperationException(
                    "The project item identifier is invalid.");
            CLMultiSelectionProjectItem item =
                currentMultiSelectionDocument.Items.FirstOrDefault(
                    value => value.ItemId == itemId);
            if (item == null)
                throw new InvalidOperationException(
                    "The project selection was not found.");
            currentMultiSelectionDocument.Items.Remove(item);
            currentMultiSelectionDocument.ModifiedAtUtc = DateTime.UtcNow;
            currentMultiSelectionDirty = true;
            SaveMultiSelectionProjectIfPossible();
            return BuildMultiSelectionProjectState();
        }

        private object OpenMultiSelectionItem(
            Dictionary<string, object> payload)
        {
            EnsureMultiSelectionProject(null, null);
            Guid itemId;
            if (!Guid.TryParse(TextValue(payload, "itemId"), out itemId))
                throw new InvalidOperationException(
                    "The project item identifier is invalid.");
            CLMultiSelectionProjectItem item =
                currentMultiSelectionDocument.Items.FirstOrDefault(
                    value => value.ItemId == itemId);
            if (item == null || String.IsNullOrWhiteSpace(item.SelectionJson))
                return new { opened = false };

            currentSelectionDocument =
                CLSelectionProjectSerializer.Deserialize(item.SelectionJson);
            currentSelectionPath = null;
            return new
            {
                opened = true,
                input = CLNextUiApplicationService.CreateInputFromProjectDocument(
                    currentSelectionDocument),
                selection = BuildProjectState(false),
                project = BuildMultiSelectionProjectState()
            };
        }

        private object ChangeMultiSelectionLanguage(
            Dictionary<string, object> payload)
        {
            string languageCode =
                NormalizeLanguageCode(TextValue(payload, "languageCode"));
            EnsureMultiSelectionProject(
                TextValue(payload, "reference"),
                languageCode);
            UpdateMultiSelectionMetadata(payload);
            RegenerateMultiSelectionReports(languageCode, true);
            SaveMultiSelectionProjectIfPossible();
            return BuildMultiSelectionProjectState();
        }

        private object EmailMultiSelectionProject(
            Dictionary<string, object> payload)
        {
            string languageCode =
                NormalizeLanguageCode(TextValue(payload, "languageCode"));
            EnsureMultiSelectionProject(
                TextValue(payload, "reference"),
                languageCode);
            UpdateMultiSelectionMetadata(payload);
            RegenerateMultiSelectionReports(languageCode);
            if (currentMultiSelectionDocument.Items.Count == 0)
                throw new InvalidOperationException(
                    "The project does not contain any reports.");
            if (!TryResolveMultiSelectionPath(false))
                return new { prepared = false, cancelled = true };

            CLMultiSelectionProjectSerializer.Save(
                currentMultiSelectionPath,
                currentMultiSelectionDocument);
            currentMultiSelectionDirty = false;

            string directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Avensys",
                "SSW",
                "Projects",
                Guid.NewGuid().ToString("N"));
            var attachments = new List<string>();
            foreach (CLMultiSelectionProjectItem item
                in currentMultiSelectionDocument.Items)
            {
                attachments.Add(
                    CLMultiSelectionProjectSerializer.ExtractPdf(
                        item,
                        directoryPath));
            }
            CLOutlookEmailService.DisplayHtmlMessage(
                CLMultiSelectionEmailComposer.BuildSubject(
                    currentMultiSelectionDocument),
                CLMultiSelectionEmailComposer.BuildHtml(
                    currentMultiSelectionDocument),
                attachments);

            if (BooleanValue(payload, "schedule"))
            {
                int days = Math.Max(
                    1,
                    Math.Min(90, IntegerValue(payload, "days", 7)));
                DateTime preparedAtUtc = DateTime.UtcNow;
                followUpStore.CreateLocal(
                    CLFollowUpTargetType.Project,
                    currentMultiSelectionDocument.ProjectId,
                    currentMultiSelectionDocument.Reference,
                    currentMultiSelectionPath,
                    preparedAtUtc,
                    preparedAtUtc.AddDays(days));
            }
            return new
            {
                prepared = true,
                project = BuildMultiSelectionProjectState()
            };
        }

        private void EnsureMultiSelectionProject(
            string reference,
            string languageCode)
        {
            if (currentMultiSelectionDocument != null) return;
            currentMultiSelectionDocument =
                CLMultiSelectionProjectSerializer.CreateNew(
                    RequiredProjectReference(reference),
                    NormalizeLanguageCode(languageCode));
            currentMultiSelectionDirty = true;
        }

        private void UpdateMultiSelectionMetadata(
            Dictionary<string, object> payload)
        {
            string referenceValue = TextValue(payload, "reference");
            if (String.IsNullOrWhiteSpace(referenceValue))
                referenceValue = TextValue(payload, "projectName");
            string reference = RequiredProjectReference(referenceValue);
            if (!String.Equals(
                    currentMultiSelectionDocument.Reference,
                    reference,
                    StringComparison.Ordinal))
            {
                currentMultiSelectionDocument.Reference = reference;
                currentMultiSelectionDocument.ModifiedAtUtc = DateTime.UtcNow;
                currentMultiSelectionDirty = true;
            }
        }

        private void RegenerateMultiSelectionReports(
            string languageCode,
            bool force = false)
        {
            string targetLanguage = NormalizeLanguageCode(languageCode);
            if (!force && String.Equals(
                    currentMultiSelectionDocument.LanguageCode,
                    targetLanguage,
                    StringComparison.OrdinalIgnoreCase) &&
                HasCurrentReportTemplates(currentMultiSelectionDocument))
                return;

            if (currentMultiSelectionDocument.Items.Count == 0)
            {
                currentMultiSelectionDocument.LanguageCode = targetLanguage;
                currentMultiSelectionDocument.ModifiedAtUtc = DateTime.UtcNow;
                currentMultiSelectionDirty = true;
                return;
            }

            string interfaceLanguage =
                NormalizeLanguageCode(
                    CLEnvironment.Current.PrimaryLanguageCode);
            CLMultiSelectionProjectDocument staged =
                CreateMultiSelectionProjectStage(targetLanguage);
            try
            {
                CLNextUiApplicationService.ApplyLanguage(targetLanguage);
                foreach (CLMultiSelectionProjectItem sourceItem
                    in currentMultiSelectionDocument.Items)
                {
                    CLSelectionProjectDocument selection =
                        CLSelectionProjectSerializer.Deserialize(
                            sourceItem.SelectionJson);
                    if (selection.Selection.Report == null)
                        selection.Selection.Report =
                            new CLReportSelectionOptions();
                    selection.Selection.Report.LanguageCode = targetLanguage;
                    selection.Versions =
                        CLSelectionProjectSerializer.CreateCurrentVersionSet(
                            CLEnvironment.Current.DatabaseCompatibility);

                    CLNextUiCalculationInput input =
                        CLNextUiApplicationService.
                            CreateInputFromProjectDocument(selection);
                    input.LanguageCode = targetLanguage;
                    string pdfPath =
                        PrepareNextReportPdf(selection, input, false);

                    CLMultiSelectionRegeneration.AddItem(
                        staged,
                        sourceItem,
                        selection,
                        pdfPath,
                        targetLanguage);
                }
            }
            finally
            {
                CLNextUiApplicationService.ApplyLanguage(interfaceLanguage);
            }

            currentMultiSelectionDocument = staged;
            currentMultiSelectionDirty = true;
        }

        private static bool HasCurrentReportTemplates(
            CLMultiSelectionProjectDocument project)
        {
            if (project == null || project.Items == null) return false;
            foreach (CLMultiSelectionProjectItem item in project.Items)
            {
                try
                {
                    CLSelectionProjectDocument selection =
                        CLSelectionProjectSerializer.Deserialize(
                            item.SelectionJson);
                    if (selection.Versions == null ||
                        selection.Versions.ReportTemplateVersion !=
                            CLTechnicalVersions.CurrentReportTemplateVersion)
                        return false;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        private CLMultiSelectionProjectDocument
            CreateMultiSelectionProjectStage(string languageCode)
        {
            return new CLMultiSelectionProjectDocument
            {
                Format = currentMultiSelectionDocument.Format,
                FormatVersion =
                    currentMultiSelectionDocument.FormatVersion,
                ProjectId = currentMultiSelectionDocument.ProjectId,
                Reference = currentMultiSelectionDocument.Reference,
                LanguageCode = languageCode,
                CreatedAtUtc =
                    currentMultiSelectionDocument.CreatedAtUtc,
                ModifiedAtUtc = DateTime.UtcNow,
                SourceFilePath = currentMultiSelectionPath
            };
        }

        private bool TryResolveMultiSelectionPath(bool saveAs)
        {
            if (!saveAs &&
                !String.IsNullOrWhiteSpace(currentMultiSelectionPath))
                return true;
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "SSW";
                dialog.Filter = MultiSelectionProjectFilter();
                dialog.DefaultExt =
                    CLMultiSelectionProjectSerializer.FileExtension.TrimStart('.');
                dialog.AddExtension = true;
                dialog.FileName = SafeFileName(
                    currentMultiSelectionDocument.Reference,
                    "Project_01");
                if (dialog.ShowDialog(this) != DialogResult.OK) return false;
                currentMultiSelectionPath = Path.GetFullPath(dialog.FileName);
                return true;
            }
        }

        private void SaveMultiSelectionProjectIfPossible()
        {
            if (String.IsNullOrWhiteSpace(currentMultiSelectionPath)) return;
            CLMultiSelectionProjectSerializer.Save(
                currentMultiSelectionPath,
                currentMultiSelectionDocument);
            currentMultiSelectionDirty = false;
        }

        private object BuildMultiSelectionProjectState()
        {
            if (currentMultiSelectionDocument == null)
                return new
                {
                    loaded = false,
                    dirty = false,
                    path = String.Empty,
                    fileName = String.Empty,
                    reference = String.Empty,
                    languageCode = String.Empty,
                    modifiedAt = String.Empty,
                    items = new object[0]
                };
            return new
            {
                loaded = true,
                dirty = currentMultiSelectionDirty,
                path = currentMultiSelectionPath ?? String.Empty,
                fileName = String.IsNullOrWhiteSpace(currentMultiSelectionPath)
                    ? String.Empty
                    : Path.GetFileName(currentMultiSelectionPath),
                reference = currentMultiSelectionDocument.Reference,
                languageCode = currentMultiSelectionDocument.LanguageCode,
                modifiedAt =
                    currentMultiSelectionDocument.ModifiedAtUtc.ToString(
                        "O",
                        CultureInfo.InvariantCulture),
                items = currentMultiSelectionDocument.Items.Select(item =>
                    new
                    {
                        itemId = item.ItemId.ToString("D"),
                        selectionProjectId =
                            item.SelectionProjectId.ToString("D"),
                        customerReference = item.CustomerReference,
                        unitName = item.UnitName,
                        airflow = item.AirflowM3h,
                        pressure = item.PressurePa,
                        pdfFileName = item.PdfFileName,
                        languageCode = item.LanguageCode,
                        current = currentSelectionDocument != null &&
                            currentSelectionDocument.ProjectId ==
                                item.SelectionProjectId,
                        ready = CLMultiSelectionProjectSerializer.IsCurrent(
                            item,
                            currentMultiSelectionDocument.LanguageCode)
                    }).ToArray()
            };
        }

        private void PreserveCurrentSelectionIdentity(
            CLSelectionProjectDocument selection)
        {
            if (currentSelectionDocument == null) return;
            selection.ProjectId = currentSelectionDocument.ProjectId;
            selection.CreatedAtUtc = currentSelectionDocument.CreatedAtUtc;
            selection.Identity = currentSelectionDocument.Identity;
            selection.RevisionTracking =
                currentSelectionDocument.RevisionTracking;
        }

        private string PrepareNextReportPdf(
            CLSelectionProjectDocument document,
            CLNextUiCalculationInput input,
            bool registerSelection = true)
        {
            CLPreparedNextUiReport prepared =
                PrepareNextReport(document, registerSelection);
            if (prepared == null)
                throw new InvalidOperationException(
                    "The report could not be prepared.");
            string directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Avensys",
                "SSW",
                "Projects",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);
            string targetPath = Path.Combine(
                directoryPath,
                SafeFileName(BuildNextReportDisplayName(input), "Report") +
                ".pdf");
            using (var report = new Microsoft.Reporting.WinForms.LocalReport())
            {
                report.ReportPath = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    prepared.ReportTemplate);
                foreach (Microsoft.Reporting.WinForms.ReportDataSource source
                    in prepared.DataSources)
                {
                    report.DataSources.Add(source);
                }
                File.WriteAllBytes(targetPath, report.Render("PDF"));
            }
            return targetPath;
        }

        private CLPreparedNextUiReport PrepareNextReport(
            CLSelectionProjectDocument document,
            bool registerSelection)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            CLNextUiCalculationInput input =
                CLNextUiApplicationService.CreateInputFromProjectDocument(
                    document);
            CLNextUiCalculationResult calculation =
                CLNextUiApplicationService.Calculate(input);
            CLNextUiApplicationService.PopulateCalculatedSnapshot(
                document, calculation);
            if (registerSelection && !RegisterSelectionWithChoice(document))
                return null;
            return CLNextUiReportService.Prepare(document, calculation);
        }

        private bool RegisterSelectionWithChoice(
            CLSelectionProjectDocument document)
        {
            var registration =
                new CLTechnicalSelectionRegistrationService();
            for (;;)
            {
                try
                {
                    registration.RegisterAsync(
                        document, CLEnvironment.Current).
                        GetAwaiter().GetResult();
                    PersistRegisteredSelection(document);
                    return true;
                }
                catch (Exception exception)
                {
                    DialogResult choice = MessageBox.Show(
                        this,
                        "The technical selection could not be registered:\r\n\r\n" +
                        exception.Message +
                        "\r\n\r\nYes: retry\r\nNo: generate draft\r\nCancel: abort",
                        "Technical selection",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);
                    if (choice == DialogResult.Yes) continue;
                    return choice == DialogResult.No;
                }
            }
        }

        private void PersistRegisteredSelection(
            CLSelectionProjectDocument document)
        {
            if (document == null ||
                currentSelectionDocument == null ||
                document.ProjectId != currentSelectionDocument.ProjectId)
                return;
            currentSelectionDocument = document;
            if (String.IsNullOrWhiteSpace(currentSelectionPath)) return;
            CLSelectionProjectSerializer.Save(
                currentSelectionPath, currentSelectionDocument);
        }

        private static string RequiredProjectReference(string value)
        {
            string result = String.IsNullOrWhiteSpace(value)
                ? "Project 01"
                : value.Trim();
            return result;
        }

        private static string NormalizeLanguageCode(string value)
        {
            return String.IsNullOrWhiteSpace(value)
                ? "en"
                : value.Trim().ToLowerInvariant();
        }

        private static string MultiSelectionProjectFilter()
        {
            return "SSW selection project (*" +
                CLMultiSelectionProjectSerializer.FileExtension + ")|*" +
                CLMultiSelectionProjectSerializer.FileExtension;
        }

        private static string SafeFileName(string value, string fallback)
        {
            string result = String.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
                result = result.Replace(invalid, '_');
            return String.IsNullOrWhiteSpace(result) ? fallback : result;
        }

        private object BuildProjectState(bool saved)
        {
            CLSelectionIdentity identity =
                currentSelectionDocument == null
                    ? null
                    : currentSelectionDocument.Identity;
            return new
            {
                saved,
                cancelled = false,
                savedAt = currentSelectionDocument == null
                    ? String.Empty
                    : currentSelectionDocument.ModifiedAtUtc.ToString(
                        "O", CultureInfo.InvariantCulture),
                path = currentSelectionPath ?? String.Empty,
                fileName = String.IsNullOrWhiteSpace(currentSelectionPath)
                    ? String.Empty
                    : Path.GetFileName(currentSelectionPath),
                localReference = identity == null
                    ? String.Empty
                    : identity.LocalDraftReference,
                publicReference = identity == null
                    ? String.Empty
                    : identity.PublicReference,
                revision = identity != null && identity.Revision.HasValue
                    ? identity.Revision.Value
                    : 0
            };
        }

        private object BuildNotificationPayload(bool markDueAsRead)
        {
            CLFollowUpReminderSnapshot snapshot = followUpStore.LoadSnapshot();
            DateTime nowUtc = DateTime.UtcNow;
            var reminders = new List<object>();
            int unreadDueCount = 0;
            foreach (CLFollowUpReminder reminder in snapshot.Reminders)
            {
                bool due =
                    reminder.Status == CLFollowUpReminderStatus.Pending &&
                    reminder.DueAtUtc <= nowUtc;
                if (due && reminder.IsUnread)
                    unreadDueCount++;
                reminders.Add(new
                {
                    id = reminder.ReminderUuid.ToString("D"),
                    targetType = reminder.TargetType.ToString(),
                    reference = reminder.DisplayReference,
                    localPath = reminder.LocalPath,
                    preparedAt = reminder.EmailPreparedAtUtc.ToString(
                        "O", CultureInfo.InvariantCulture),
                    dueAt = reminder.DueAtUtc.ToString(
                        "O", CultureInfo.InvariantCulture),
                    status = reminder.Status.ToString(),
                    rescheduleCount = reminder.RescheduleCount,
                    unread = reminder.IsUnread,
                    due,
                    fileAvailable =
                        !String.IsNullOrWhiteSpace(reminder.LocalPath) &&
                        File.Exists(reminder.LocalPath)
                });
                if (markDueAsRead && due && reminder.IsUnread)
                    followUpStore.SetRead(reminder.ReminderUuid, true);
            }
            return new
            {
                unreadDueCount,
                reminders = reminders.ToArray()
            };
        }

        private object ApplyNotificationAction(
            Dictionary<string, object> payload)
        {
            Guid reminderId;
            if (!Guid.TryParse(TextValue(payload, "id"), out reminderId))
                throw new InvalidOperationException("The reminder identifier is invalid.");
            string action = TextValue(payload, "action");
            if (String.Equals(action, "reschedule", StringComparison.OrdinalIgnoreCase))
            {
                int days = IntegerValue(payload, "days", 7);
                DateTime dueUtc =
                    CLFollowUpReminderRules.CalculateDueUtc(DateTime.UtcNow, days);
                followUpStore.RescheduleLocal(reminderId, dueUtc);
            }
            else if (String.Equals(action, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                followUpStore.CloseLocal(
                    reminderId, CLFollowUpReminderStatus.Succeeded);
            }
            else if (String.Equals(action, "unsuccessful", StringComparison.OrdinalIgnoreCase))
            {
                followUpStore.CloseLocal(
                    reminderId, CLFollowUpReminderStatus.Unsuccessful);
            }
            else
            {
                throw new InvalidOperationException(
                    "The reminder action is not supported.");
            }
            return BuildNotificationPayload(false);
        }

        private object OpenNotificationTarget(
            Dictionary<string, object> payload)
        {
            Guid reminderId;
            if (!Guid.TryParse(TextValue(payload, "id"), out reminderId))
                throw new InvalidOperationException(
                    "The reminder identifier is invalid.");
            CLFollowUpReminder reminder = followUpStore.LoadSnapshot().Reminders
                .FirstOrDefault(item => item.ReminderUuid == reminderId);
            if (reminder == null ||
                String.IsNullOrWhiteSpace(reminder.LocalPath) ||
                !File.Exists(reminder.LocalPath))
            {
                return new { opened = false, fileAvailable = false };
            }
            if (String.Equals(
                Path.GetExtension(reminder.LocalPath),
                CLMultiSelectionProjectSerializer.FileExtension,
                StringComparison.OrdinalIgnoreCase))
            {
                currentMultiSelectionDocument =
                    CLMultiSelectionProjectSerializer.Load(reminder.LocalPath);
                currentMultiSelectionPath = Path.GetFullPath(reminder.LocalPath);
                currentMultiSelectionDirty = false;
                return new
                {
                    opened = true,
                    openedProject = true,
                    fileAvailable = true,
                    project = BuildMultiSelectionProjectState()
                };
            }
            if (!String.Equals(
                Path.GetExtension(reminder.LocalPath),
                CLSelectionProjectSerializer.FileExtension,
                StringComparison.OrdinalIgnoreCase))
                return new { opened = false, fileAvailable = false };
            return LoadSelection(reminder.LocalPath);
        }

        private void OpenNextReport(CLNextUiCalculationInput input)
        {
            CLSelectionProjectDocument document =
                CLNextUiApplicationService.CreateProjectDocument(input);
            CLPreparedNextUiReport preparedReport;
            try
            {
                PreserveCurrentSelectionIdentity(document);
                preparedReport = PrepareNextReport(document, true);
            }
            catch (Exception reportError)
            {
                WriteDiagnostic(
                    "Next report generation failed.\r\n" + reportError);
                MessageBox.Show(
                    this,
                    reportError.Message,
                    CLSSWProfile.AssemblyTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (preparedReport == null) return;

            using (var viewer = new CLReportViewerForm())
            {
                string reportPath = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    preparedReport.ReportTemplate);
                viewer.SetReport(
                    reportPath,
                    preparedReport.DataSources.ToArray(),
                    Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
                viewer.SetDisplayName(BuildNextReportDisplayName(input));
                viewer.WindowState = FormWindowState.Maximized;
                viewer.ShowDialog(this);
            }
        }

        private static string BuildNextReportDisplayName(
            CLNextUiCalculationInput input)
        {
            string reference = String.IsNullOrWhiteSpace(input.CustomerReference)
                ? String.Empty
                : Regex.Replace(input.CustomerReference.Trim(), @"[^\p{L}\p{N}_-]+", "_") + "_";
            string model = Regex.Replace(
                input.ModelCode ?? "SSW",
                @"[^\p{L}\p{N}_-]+",
                "_");
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}_{2:0}_{3:0}_Report_{4}",
                reference,
                model,
                input.SupplyAirflowM3h,
                input.PressurePa,
                String.IsNullOrWhiteSpace(input.LanguageCode) ? "it" : input.LanguageCode);
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
            int invalidNumberCount;
            json = NormalizeJsonNumbers(json, out invalidNumberCount);
            if (invalidNumberCount > 0)
            {
                WriteDiagnostic(
                    "Bridge response normalized " + invalidNumberCount +
                    " non-finite numeric value(s).");
            }
            webView.CoreWebView2.PostWebMessageAsJson(json);
        }

        internal static string NormalizeJsonNumbers(
            string json,
            out int invalidNumberCount)
        {
            int count = 0;
            string normalized = Regex.Replace(
                json ?? String.Empty,
                @"(?<=[:\[,])(?:NaN|-?Infinity)(?=[,\]}])",
                delegate
                {
                    count++;
                    return "null";
                });
            invalidNumberCount = count;
            return normalized;
        }

        private static void WriteDiagnostic(string message)
        {
            try
            {
                string logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Avensys", "SSW", "Logs");
                Directory.CreateDirectory(logDirectory);
                File.AppendAllText(
                    Path.Combine(logDirectory, "next-ui.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                    " " + message + Environment.NewLine);
            }
            catch
            {
                // Diagnostics must never prevent the selector from starting.
            }
        }

        private static string TextValue(IDictionary<string, object> payload, string key)
        {
            object value;
            return payload.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value, CultureInfo.InvariantCulture)
                : String.Empty;
        }

        private static IDictionary<string, object> DictionaryValue(
            IDictionary<string, object> payload,
            string key)
        {
            object value;
            if (payload != null &&
                payload.TryGetValue(key, out value) &&
                value is IDictionary<string, object>)
                return (IDictionary<string, object>)value;
            return new Dictionary<string, object>();
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
