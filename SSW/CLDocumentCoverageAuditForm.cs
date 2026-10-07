using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace SSW
{
    internal sealed class CLDocumentCoverageAuditForm : Form
    {
        private static readonly string[] UntrackedCategories =
        {
            "Certificazioni", "Testo bando", "Messa in servizio",
            "Sostituzione filtro", "Materiale in servizio"
        };

        private readonly List<CLProductDocumentCoverageRow> rows;
        private readonly DataGridView grid;
        private readonly TextBox search;
        private readonly CheckBox missingOnly;
        private readonly Label summary;
        private readonly CancellationTokenSource onlineCancellation = new CancellationTokenSource();
        private static readonly string[] FallbackLanguages = { "BG", "CS", "HU", "IS", "NO", "RO", "SL" };
        private static readonly string[] DirectLanguages = { "DA", "DE", "EN", "FR", "IT", "NL", "PL", "SV" };

        internal CLDocumentCoverageAuditForm()
        {
            Text = "Controllo disponibilità documenti";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1100, 650);
            Size = new Size(1500, 850);
            ShowInTaskbar = false;

            rows = CLProductDocumentService.BuildLocalCoverage(CLSSWProfile.ShortName);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            Controls.Add(layout);

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 6, 0, 0)
            };
            filters.Controls.Add(new Label
            {
                Text = "Cerca taglia o modello",
                AutoSize = true,
                Margin = new Padding(0, 7, 8, 0)
            });
            search = new TextBox { Width = 280, Margin = new Padding(0, 2, 18, 0) };
            missingOnly = new CheckBox
            {
                Text = "Solo documenti con assenza confermata",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };
            filters.Controls.Add(search);
            filters.Controls.Add(missingOnly);
            var print = new Button
            {
                Text = "Stampa / Print",
                AutoSize = true,
                Margin = new Padding(18, 2, 0, 0)
            };
            print.Click += delegate { OpenPrintableReport(); };
            filters.Controls.Add(print);
            layout.Controls.Add(filters, 0, 0);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeight = 42,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(220, 229, 225)
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(236, 242, 239);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(25, 48, 40);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(221, 239, 230);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(25, 48, 40);
            AddTextColumn("size", "Taglia", 65, true);
            AddTextColumn("variant", "Versione / variante modello", 190, true);
            AddTextColumn("code", "Codice SDF", 145, true);
            AddTextColumn("series", "Serie", 60, true);
            AddTextColumn("commercial", "Scheda commerciale", 180, false);
            AddTextColumn("benchmark", "Benchmark document (confidential)", 180, false);
            AddTextColumn("manual", "Manuale installazione", 135, false);
            AddTextColumn("application", "Documento applicativo", 135, false);
            AddTextColumn("drawing", "Disegno dimensionale", 135, false);
            AddTextColumn("step", "Modello 3D", 115, false);
            foreach (string category in UntrackedCategories)
                AddTextColumn("untracked" + grid.Columns.Count, category, 125, false);
            grid.CellFormatting += FormatStatusCell;
            grid.CellPainting += PaintLanguageBadges;
            grid.RowTemplate.Height = 104;
            layout.Controls.Add(grid, 0, 1);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            summary = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(78, 99, 91)
            };
            var close = new Button
            {
                Text = "Chiudi",
                Dock = DockStyle.Fill,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(4, 9, 0, 7)
            };
            footer.Controls.Add(summary, 0, 0);
            footer.Controls.Add(close, 1, 0);
            layout.Controls.Add(footer, 0, 2);
            CancelButton = close;

            search.TextChanged += delegate { RefreshRows(); };
            missingOnly.CheckedChanged += delegate { RefreshRows(); };
            RefreshRows();
            FormClosed += delegate { onlineCancellation.Cancel(); };
            Shown += async delegate
            {
                summary.Text = "Verifica online delle schede e dei modelli STEP 3D in corso...";
                var checks = rows.SelectMany(row => row.CommercialSheetOnlineCandidates.Where(pair => !FallbackLanguages.Contains(pair.Key)).Select(pair =>
                    new { Row = row, Language = pair.Key, Urls = pair.Value }))
                    .Concat(rows.Select(row => new { Row = row, Language = "STEP", Urls =
                        String.IsNullOrWhiteSpace(row.StepModelOnlineUrl) ? new List<string>() : new List<string> { row.StepModelOnlineUrl } })).ToList();
                try
                {
                    await Task.Run(() => Parallel.ForEach(checks,
                        new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = onlineCancellation.Token },
                        check =>
                        {
                            string status = "Unverified";
                            bool absent = check.Urls.Count > 0;
                            foreach (string url in check.Urls)
                            {
                                onlineCancellation.Token.ThrowIfCancellationRequested();
                                string result = ProbeDocument(url);
                                if (result == "Online") { status = result; break; }
                                if (result != "Missing") absent = false;
                            }
                            if (status != "Online" && absent) status = "Missing";
                            lock (check.Row)
                            {
                                if (check.Language == "STEP") check.Row.StepModelOnlineStatus = status;
                                else check.Row.CommercialSheetOnlineStatus[check.Language] = status;
                            }
                        }));
                    if (!IsDisposed) RefreshRows();
                }
                catch (OperationCanceledException) { }
            };
        }

        private static string ProbeDocument(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 4000;
                request.AllowAutoRedirect = true;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    string contentType = (response.ContentType ?? String.Empty).Split(';')[0].Trim().ToLowerInvariant();
                    bool zip = new Uri(url).AbsolutePath.EndsWith("_stp.zip", StringComparison.OrdinalIgnoreCase);
                    bool validType = zip ? contentType == "application/zip" || contentType == "application/x-zip-compressed" ||
                        contentType == "application/octet-stream" : contentType == "application/pdf";
                    return response.StatusCode == HttpStatusCode.OK && validType ? "Online" : "Unverified";
                }
            }
            catch (WebException exception)
            {
                using (var response = exception.Response as HttpWebResponse)
                    return response != null && (response.StatusCode == HttpStatusCode.NotFound ||
                        response.StatusCode == HttpStatusCode.Gone) ? "Missing" : "Unverified";
            }
        }

        private static string CommercialAvailability(CLProductDocumentCoverageRow row)
        {
            var values = new List<string>();
            lock (row)
            {
                foreach (string language in DirectLanguages)
                {
                    bool local = row.CommercialSheetPaths.Any(path => File.Exists(path) &&
                        String.Equals(LanguageForPath(path), language, StringComparison.OrdinalIgnoreCase));
                    string status;
                    row.CommercialSheetOnlineStatus.TryGetValue(language, out status);
                    values.Add(language + ": " + (local ? "Offline" : status == "Online" ? "Online" :
                        status == "Missing" ? "Mancante / Missing" : "Online non verificato / Unverified"));
                }
            }
            return values.Count > 0 ? String.Join(Environment.NewLine, values) : Availability(row.CommercialSheetPaths);
        }

        private static string LanguageStatus(CLProductDocumentCoverageRow row, string language)
        {
            lock (row)
            {
                if (row.CommercialSheetPaths.Any(path => File.Exists(path) &&
                    String.Equals(LanguageForPath(path), language, StringComparison.OrdinalIgnoreCase))) return "Offline";
                string status;
                return row.CommercialSheetOnlineStatus.TryGetValue(language, out status) ? status : "Unverified";
            }
        }

        private static bool IsAvailable(string status) => status == "Online" || status == "Offline";

        private static string StepAvailability(CLProductDocumentCoverageRow row)
        {
            if (File.Exists(row.StepModelPath ?? String.Empty)) return "Offline";
            lock (row)
                return row.StepModelOnlineStatus == "Online" ? "Online" :
                    row.StepModelOnlineStatus == "Missing" ? "Mancante / Missing" : "Online non verificato / Unverified";
        }

        private void PaintLanguageBadges(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "commercial") return;
            var row = grid.Rows[e.RowIndex].Tag as CLProductDocumentCoverageRow;
            if (row == null) return;
            e.PaintBackground(e.ClipBounds, true);
            using (var font = new Font(Font.FontFamily, 7.5f))
            {
                Action<string[], int, bool> paint = (languages, top, fallback) =>
                {
                    for (int i = 0; i < languages.Length; i++)
                    {
                        bool available = IsAvailable(LanguageStatus(row, fallback ? "EN" : languages[i]));
                        var rectangle = new Rectangle(e.CellBounds.Left + 8 + (i % 4) * 39,
                            e.CellBounds.Top + top + (i / 4) * 20, 33, 17);
                        using (var brush = new SolidBrush(available ? Color.FromArgb(222, 241, 230) : Color.FromArgb(255, 230, 226)))
                            e.Graphics.FillRectangle(brush, rectangle);
                        TextRenderer.DrawText(e.Graphics, languages[i], font, rectangle,
                            available ? Color.FromArgb(23, 110, 66) : Color.FromArgb(170, 40, 35),
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                };
                paint(DirectLanguages, 5, false);
                TextRenderer.DrawText(e.Graphics, IsAvailable(LanguageStatus(row, "EN")) ? "Fallback EN available" : "Fallback EN non verificato/assente",
                    font, new Rectangle(e.CellBounds.Left + 6, e.CellBounds.Top + 46, 168, 16), Color.DimGray);
                paint(FallbackLanguages, 64, true);
            }
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Border);
            e.Handled = true;
        }

        private static string CommercialBadgesHtml(CLProductDocumentCoverageRow row)
        {
            Func<string, string, string> badge = (language, source) =>
            {
                string status = LanguageStatus(row, source);
                return "<span class=\"badge " + (IsAvailable(status) ? "ok" : "bad") + "\" title=\"" +
                    WebUtility.HtmlEncode(source + ": " + status) + "\">" + language + "</span>";
            };
            return String.Join("", DirectLanguages.Select(language => badge(language, language))) +
                "<div class=\"fallback\">" + (IsAvailable(LanguageStatus(row, "EN")) ? "Fallback EN available" : "Fallback EN: unverified / missing") +
                "</div>" + String.Join("", FallbackLanguages.Select(language => badge(language, "EN")));
        }

        private void AddTextColumn(string name, string title, int width, bool frozen)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = title,
                Width = width,
                MinimumWidth = width,
                SortMode = DataGridViewColumnSortMode.Automatic,
                Frozen = frozen,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    WrapMode = DataGridViewTriState.True
                }
            });
        }

        private void RefreshRows()
        {
            grid.Rows.Clear();
            string query = (search.Text ?? String.Empty).Trim();
            var visibleRows = rows.Where(row =>
            {
                string identity = String.Join(" ", row.ModelSize, row.ModelName, row.ModelCode, row.SeriesCode);
                return (query.Length == 0 || identity.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (!missingOnly.Checked || HasMissingTrackedDocument(row));
            }).ToList();

            foreach (CLProductDocumentCoverageRow row in visibleRows)
            {
                string[] values =
                {
                    row.ModelSize.HasValue ? row.ModelSize.Value.ToString() : "-",
                    row.ModelName ?? String.Empty,
                    row.ModelCode ?? String.Empty,
                    row.SeriesCode ?? String.Empty,
                    CommercialAvailability(row),
                    "N/D",
                    Availability(row.InstallationManualPaths),
                    Availability(row.ApplicationDocumentPaths),
                    row.DimensionalDrawingVersions.Count == 0
                        ? "Non trovato nel catalogo locale"
                        : String.Join(Environment.NewLine, row.DimensionalDrawingVersions),
                    StepAvailability(row),
                    "N/D", "N/D", "N/D", "N/D", "N/D"
                };
                int index = grid.Rows.Add(values);
                var gridRow = grid.Rows[index];
                gridRow.Tag = row;
                gridRow.Cells["commercial"].ToolTipText = CommercialAvailability(row) + Environment.NewLine +
                    "BG, CS, HU, IS, NO, RO, SL: fallback EN. Rosso: assente o non verificato.";
                SetFileTooltip(gridRow.Cells["manual"], row.InstallationManualPaths);
                SetFileTooltip(gridRow.Cells["application"], row.ApplicationDocumentPaths);
                gridRow.Cells["drawing"].ToolTipText =
                    row.DimensionalDrawingVersions.Count == 0
                        ? "Nessun disegno attivo associato nel catalogo dimensionale SDF."
                        : String.Join(Environment.NewLine, row.DimensionalDrawingVersions);
                gridRow.Cells["step"].ToolTipText =
                    File.Exists(row.StepModelPath ?? String.Empty)
                        ? FileDetails(row.StepModelPath)
                        : StepAvailability(row) + Environment.NewLine + row.StepModelOnlineUrl;
            }

            int missingModels = rows.Count(HasMissingTrackedDocument);
            summary.Text = String.Format(
                "{0} modelli visualizzati su {1} · {2} con scheda o STEP assente confermato · Altre categorie: verifica locale; N/D: fonte non collegata",
                visibleRows.Count, rows.Count, missingModels);
        }

        private static string Availability(List<string> paths)
        {
            if (paths == null || paths.Count == 0) return "Non trovato offline; online non verificato";
            var languages = paths
                .Where(File.Exists)
                .Select(LanguageForPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase);
            return String.Join(", ", languages);
        }

        private static string LanguageForPath(string path)
        {
            string folder = Path.GetFileName(Path.GetDirectoryName(path));
            string[] languages =
            {
                "BG", "CS", "DA", "DE", "EN", "FR", "HU", "IS",
                "IT", "NL", "NO", "PL", "RO", "SL", "SV"
            };
            if (languages.Contains(folder, StringComparer.OrdinalIgnoreCase)) return folder;
            string[] tokens = Path.GetFileNameWithoutExtension(path).Split('_');
            return tokens.Length >= 2 && languages.Contains(tokens[tokens.Length - 2], StringComparer.OrdinalIgnoreCase)
                ? tokens[tokens.Length - 2]
                : "File locale";
        }

        private static void SetFileTooltip(DataGridViewCell cell, List<string> paths)
        {
            cell.ToolTipText = paths == null || paths.Count == 0
                ? "Nessun file locale trovato."
                : String.Join(Environment.NewLine, paths.Where(File.Exists).Select(FileDetails));
        }

        private static string FileDetails(string path)
        {
            var info = new FileInfo(path);
            return info.Name + " · file modificato " + info.LastWriteTime.ToString("yyyy-MM-dd");
        }

        private static bool HasMissingTrackedDocument(CLProductDocumentCoverageRow row)
        {
            lock (row)
                return (row.StepModelOnlineStatus == "Missing" && !File.Exists(row.StepModelPath ?? String.Empty)) ||
                    row.CommercialSheetOnlineStatus.Any(pair => !FallbackLanguages.Contains(pair.Key) && pair.Value == "Missing" &&
                    !row.CommercialSheetPaths.Any(path => File.Exists(path) &&
                        String.Equals(LanguageForPath(path), pair.Key, StringComparison.OrdinalIgnoreCase)));
        }

        private void OpenPrintableReport()
        {
            string query = (search.Text ?? String.Empty).Trim();
            var visibleRows = rows.Where(row =>
            {
                string identity = String.Join(" ", row.ModelSize, row.ModelName, row.ModelCode, row.SeriesCode);
                return (query.Length == 0 || identity.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (!missingOnly.Checked || HasMissingTrackedDocument(row));
            }).ToList();

            string[] headers =
            {
                "Taglia / Size", "Variante modello / Model variant", "Codice SDF / SDF code",
                "Serie / Series", "Scheda commerciale / Commercial sheet",
                "Benchmark document (confidential)",
                "Manuale installazione / Installation manual", "Documento applicativo / Application document",
                "Disegno dimensionale / Dimensional drawing", "Modello 3D / 3D model",
                "Certificazioni / Certifications", "Testo bando / Tender specification",
                "Messa in servizio / Commissioning", "Sostituzione filtro / Filter replacement",
                "Materiale in servizio / Service material"
            };
            var html = new StringBuilder();
            html.Append("<!doctype html><html lang=\"it\"><head><meta charset=\"utf-8\"><title>Disponibilità documenti / Document availability</title>");
            html.Append("<style>@page{size:A4 landscape;margin:10mm}body{font:9pt Arial,sans-serif;color:#18232d}h1{font-size:17pt;margin:0 0 4px}p{margin:3px 0 12px;color:#53616e}table{width:100%;border-collapse:collapse;table-layout:fixed}th,td{border:1px solid #aebdb6;padding:4px;vertical-align:top;overflow-wrap:anywhere}th{background:#eaf1ed;font-size:7pt}td{font-size:7pt}.missing{color:#a52d21;font-weight:bold}.na{color:#65736d}.print{position:fixed;right:12px;top:12px;padding:8px 14px}@media print{.print{display:none}tr{break-inside:avoid}}</style></head><body>");
            html.Append("<button class=\"print\" onclick=\"window.print()\">Stampa / Print (PDF o carta / PDF or paper)</button>");
            html.Append("<style>.badge{display:inline-block;padding:2px 3px;margin:2px;font-weight:bold;border-radius:3px}.ok{background:#def1e6;color:#176e42}.bad{background:#ffe6e2;color:#aa2823}.fallback{font-size:6pt;margin-top:4px}@media print{.badge{print-color-adjust:exact;-webkit-print-color-adjust:exact}}</style>");
            html.Append("<h1>Disponibilità documenti / Document availability</h1><p>Data / Date: ");
            html.Append(WebUtility.HtmlEncode(DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
            html.Append(" · Modelli / Models: ").Append(visibleRows.Count).Append(" · Le categorie N/D non sono collegate a una fonte SSW / N/A categories are not linked to an SSW source.</p><table><thead><tr>");
            foreach (string header in headers) html.Append("<th>").Append(WebUtility.HtmlEncode(header)).Append("</th>");
            html.Append("</tr></thead><tbody>");

            foreach (CLProductDocumentCoverageRow row in visibleRows)
            {
                string[] values =
                {
                    row.ModelSize.HasValue ? row.ModelSize.Value.ToString() : "-",
                    row.ModelName ?? String.Empty,
                    row.ModelCode ?? String.Empty,
                    row.SeriesCode ?? String.Empty,
                    CommercialAvailability(row),
                    "N/D / N/A",
                    PrintAvailability(row.InstallationManualPaths),
                    PrintAvailability(row.ApplicationDocumentPaths),
                    row.DimensionalDrawingVersions.Count == 0
                        ? "Non trovato nel catalogo locale / Not found in local catalog"
                        : String.Join(", ", row.DimensionalDrawingVersions),
                    StepAvailability(row),
                    "N/D / N/A", "N/D / N/A", "N/D / N/A", "N/D / N/A", "N/D / N/A"
                };
                html.Append("<tr>");
                for (int i = 0; i < values.Length; i++)
                {
                    string value = values[i];
                    string css = value == "Mancante / Missing" ? " class=\"missing\"" :
                        value == "N/D / N/A" ? " class=\"na\"" : String.Empty;
                    html.Append("<td").Append(i == 4 ? "" : css).Append(">").Append(i == 4 ? CommercialBadgesHtml(row) : WebUtility.HtmlEncode(value).Replace(Environment.NewLine, "<br>")).Append("</td>");
                }
                html.Append("</tr>");
            }
            html.Append("</tbody></table></body></html>");

            string path = Path.Combine(Path.GetTempPath(), "SSW-document-availability-" + Guid.NewGuid().ToString("N") + ".html");
            File.WriteAllText(path, html.ToString(), new UTF8Encoding(false));
            try
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "Impossibile aprire il documento di stampa / Unable to open the print document.\r\n\r\n" + exception.Message,
                    "Stampa documenti / Print documents",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string PrintAvailability(List<string> paths)
        {
            string value = Availability(paths);
            return value == "Manca" ? "Mancante / Missing" : value;
        }

        private void FormatStatusCell(object sender, DataGridViewCellFormattingEventArgs eventArgs)
        {
            if (eventArgs.RowIndex < 0 || eventArgs.ColumnIndex < 4) return;
            if (grid.Columns[eventArgs.ColumnIndex].Name == "commercial") return;
            string value = Convert.ToString(eventArgs.Value);
            if (value != null && (value.Contains("Mancante / Missing") ||
                (grid.Columns[eventArgs.ColumnIndex].Name == "step" && value.Contains("Unverified"))))
            {
                eventArgs.CellStyle.BackColor = Color.FromArgb(255, 239, 235);
                eventArgs.CellStyle.ForeColor = Color.FromArgb(155, 55, 42);
            }
            else if (value == "N/D")
            {
                eventArgs.CellStyle.BackColor = Color.FromArgb(241, 243, 242);
                eventArgs.CellStyle.ForeColor = Color.FromArgb(115, 126, 121);
            }
            else if (value != null && value.Length > 0)
            {
                eventArgs.CellStyle.BackColor = Color.FromArgb(233, 246, 239);
                eventArgs.CellStyle.ForeColor = Color.FromArgb(29, 108, 73);
            }
        }
    }
}
