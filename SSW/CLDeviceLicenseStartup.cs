using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSW
{
    internal static class CLDeviceLicenseStartup
    {
        private static readonly Guid SessionId = Guid.NewGuid();
        private static System.Threading.Timer heartbeat;

        internal static bool ValidateForNormalStartup()
        {
            CLDeviceLicenseSnapshot snapshot = CLDeviceLicenseStore.LoadSnapshot();
            if (snapshot.Mode == CLDeviceLicenseMode.NewInstallation ||
                snapshot.Mode == CLDeviceLicenseMode.LegacyProfileRequired)
            {
                CLDeviceLicenseMode formMode = snapshot.Mode;
                while (true)
                {
                    DialogResult result;
                    using (var form = new CLDeviceLicenseForm(formMode)) result = form.ShowDialog();
                    if (result == DialogResult.Retry)
                    {
                        formMode = CLDeviceLicenseMode.NewInstallation;
                        continue;
                    }
                    if (result != DialogResult.OK) return false;
                    break;
                }
                snapshot = CLDeviceLicenseStore.LoadSnapshot();
            }

            if (snapshot.Mode == CLDeviceLicenseMode.Active)
            {
                StartHeartbeat(TimeSpan.FromSeconds(2));
                return true;
            }
            if (!TryRefresh(snapshot)) return false;
            StartHeartbeat(TimeSpan.FromHours(6));
            return true;
        }

        private static bool TryRefresh(CLDeviceLicenseSnapshot snapshot)
        {
            try
            {
                RefreshAsync().GetAwaiter().GetResult();
                return true;
            }
            catch (CLSelectionApiException exception)
            {
                if (exception.StatusCode == HttpStatusCode.Forbidden || exception.StatusCode == HttpStatusCode.Unauthorized)
                {
                    CLDeviceLicenseStore.MarkRevoked();
                    MessageBox.Show("La licenza SSW non è attiva. Contattare Avensys per riattivarla.", "Licenza SSW", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return AllowOfflineOrExplain(snapshot);
            }
            catch (Exception exception)
            {
                if (exception is HttpRequestException || exception is TaskCanceledException)
                    return AllowOfflineOrExplain(snapshot);
                throw;
            }
        }

        private static bool AllowOfflineOrExplain(CLDeviceLicenseSnapshot snapshot)
        {
            if (snapshot.ValidUntilUtc.HasValue && snapshot.ValidUntilUtc.Value > DateTime.UtcNow && snapshot.Mode != CLDeviceLicenseMode.Revoked)
                return true;
            MessageBox.Show("Per verificare la licenza è necessaria una connessione Internet. Collegare il computer e riaprire SSW.", "Licenza SSW", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private static async Task RefreshAsync()
        {
            var client = new CLSelectionApiClient();
            CLDeviceLicenseResult result = await client.CheckDeviceLicenseAsync(
                CLSelectionRegistrationContext.FromEnvironment(CLEnvironment.Current), SessionId);
            CLDeviceLicenseStore.Renew(result.DeviceNumber, result.ValidUntilUtc);
        }

        private static void StartHeartbeat(TimeSpan dueTime)
        {
            heartbeat = new System.Threading.Timer(async _ =>
            {
                try { await RefreshAsync(); }
                catch (CLSelectionApiException exception)
                {
                    if (exception.StatusCode != HttpStatusCode.Forbidden && exception.StatusCode != HttpStatusCode.Unauthorized) return;
                    CLDeviceLicenseStore.MarkRevoked();
                    if (Application.OpenForms.Count == 0) return;
                    Application.OpenForms[0].BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("La licenza SSW è stata revocata. Contattare Avensys.", "Licenza SSW", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Application.Exit();
                    }));
                }
                catch (HttpRequestException) { }
                catch (TaskCanceledException) { }
            }, null, dueTime, TimeSpan.FromHours(6));
        }
    }

    internal sealed class CLDeviceLicenseForm : Form
    {
        private readonly CLDeviceLicenseMode mode;
        private readonly TextBox firstName = new TextBox();
        private readonly TextBox lastName = new TextBox();
        private readonly TextBox email = new TextBox();
        private readonly TextBox pin = new TextBox();
        private readonly Label error = new Label();
        private readonly Button confirm = new Button();

        internal CLDeviceLicenseForm(CLDeviceLicenseMode mode)
        {
            this.mode = mode;
            Text = mode == CLDeviceLicenseMode.NewInstallation ? "Attivazione SSW" : "Completa il profilo SSW";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            ClientSize = new Size(620, mode == CLDeviceLicenseMode.NewInstallation ? 470 : 410);
            MinimumSize = new Size(620, mode == CLDeviceLicenseMode.NewInstallation ? 470 : 410);

            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 4,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(239, 247, 243), Padding = new Padding(32, 20, 32, 14) };
            var accent = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = Color.FromArgb(32, 127, 88) };
            var eyebrow = new Label {
                Text = "AVENSYS  ·  LICENZA SSW",
                ForeColor = Color.FromArgb(23, 113, 78),
                Font = new Font("Segoe UI Semibold", 8.5F),
                AutoSize = true,
                Location = new Point(32, 20)
            };
            var title = new Label {
                Text = Text,
                ForeColor = Color.FromArgb(20, 49, 39),
                Font = new Font("Segoe UI Semibold", 18F),
                AutoSize = true,
                Location = new Point(29, 43)
            };
            var description = new Label {
                Text = mode == CLDeviceLicenseMode.NewInstallation
                    ? "Inserisci i dati comunicati ad Avensys e il codice di installazione ricevuto."
                    : "Questa installazione è già attiva. Associala alla persona che la utilizza.",
                ForeColor = Color.FromArgb(83, 105, 97),
                Location = new Point(32, 82),
                Size = new Size(545, 28)
            };
            header.Controls.Add(description);
            header.Controls.Add(title);
            header.Controls.Add(eyebrow);
            header.Controls.Add(accent);
            root.Controls.Add(header, 0, 0);

            var fields = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = mode == CLDeviceLicenseMode.NewInstallation ? 3 : 2,
                Padding = new Padding(24, 18, 24, 8),
                BackColor = Color.White
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int row = 0; row < fields.RowCount; row++) fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / fields.RowCount));
            fields.Controls.Add(FieldBlock("Nome", firstName), 0, 0);
            fields.Controls.Add(FieldBlock("Cognome", lastName), 1, 0);
            fields.Controls.Add(FieldBlock("Email", email), 0, 1);
            fields.SetColumnSpan(fields.GetControlFromPosition(0, 1), 2);
            if (mode == CLDeviceLicenseMode.NewInstallation)
            {
                pin.MaxLength = 6;
                pin.TextAlign = HorizontalAlignment.Center;
                fields.Controls.Add(FieldBlock("Codice di installazione", pin, "6 cifre, ricevute da Avensys"), 0, 2);
                fields.SetColumnSpan(fields.GetControlFromPosition(0, 2), 2);
            }
            root.Controls.Add(fields, 0, 1);

            error.ForeColor = Color.Firebrick;
            error.BackColor = Color.FromArgb(255, 247, 247);
            error.Dock = DockStyle.Fill;
            error.Padding = new Padding(28, 11, 28, 8);
            error.AutoEllipsis = true;
            root.Controls.Add(error, 0, 2);

            confirm.Text = mode == CLDeviceLicenseMode.NewInstallation ? "Attiva" : "Associa installazione";
            confirm.Size = new Size(190, 40);
            confirm.BackColor = Color.FromArgb(32, 127, 88);
            confirm.ForeColor = Color.White;
            confirm.FlatStyle = FlatStyle.Flat;
            confirm.FlatAppearance.BorderSize = 0;
            confirm.Font = new Font("Segoe UI Semibold", 9.5F);
            confirm.Click += async (_, __) => await SubmitAsync();
            var cancel = new Button {
                Text = "Esci",
                DialogResult = DialogResult.Cancel,
                Size = new Size(92, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(37, 66, 56),
                FlatStyle = FlatStyle.Flat
            };
            cancel.FlatAppearance.BorderColor = Color.FromArgb(177, 196, 188);
            var actions = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(24, 15, 24, 12),
                BackColor = Color.FromArgb(246, 249, 248)
            };
            confirm.Margin = new Padding(10, 0, 0, 0);
            cancel.Margin = Padding.Empty;
            actions.Controls.Add(confirm);
            actions.Controls.Add(cancel);
            root.Controls.Add(actions, 0, 3);
            Controls.Add(root);
            AcceptButton = confirm;
            CancelButton = cancel;
        }

        private static Control FieldBlock(string label, TextBox box, string hint = null)
        {
            var block = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = hint == null ? 2 : 3, Margin = new Padding(6, 0, 6, 8) };
            block.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            block.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            if (hint != null) block.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            block.Controls.Add(new Label {
                Text = label,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(29, 58, 48),
                Font = new Font("Segoe UI Semibold", 9F),
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 0);
            box.Dock = DockStyle.Fill;
            box.Font = new Font("Segoe UI", 10.5F);
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Margin = new Padding(0, 3, 0, 0);
            block.Controls.Add(box, 0, 1);
            if (hint != null) block.Controls.Add(new Label {
                Text = hint,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(104, 120, 114),
                Font = new Font("Segoe UI", 8F),
                Padding = new Padding(0, 3, 0, 0)
            }, 0, 2);
            return block;
        }

        private async Task SubmitAsync()
        {
            error.Text = "";
            if (String.IsNullOrWhiteSpace(firstName.Text) || String.IsNullOrWhiteSpace(lastName.Text) ||
                String.IsNullOrWhiteSpace(email.Text) || !email.Text.Contains("@") ||
                (mode == CLDeviceLicenseMode.NewInstallation && (pin.Text.Length != 6 || !Int32.TryParse(pin.Text, out _))))
            {
                error.Text = "Compila tutti i campi con dati validi.";
                return;
            }
            SetBusy(true);
            try
            {
                var client = new CLSelectionApiClient();
                var context = CLSelectionRegistrationContext.FromEnvironment(CLEnvironment.Current);
                CLDeviceLicenseResult result = mode == CLDeviceLicenseMode.NewInstallation
                    ? await client.ActivateDeviceLicenseAsync(firstName.Text, lastName.Text, email.Text, pin.Text, context)
                    : await client.ClaimLegacyDeviceLicenseAsync(firstName.Text, lastName.Text, email.Text, context);
                CLDeviceLicenseStore.SaveActive(firstName.Text, lastName.Text, email.Text, result.DeviceNumber, result.ValidUntilUtc);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (CLSelectionApiException exception)
            {
                if (exception.ErrorCode == "activation_code_required" && mode == CLDeviceLicenseMode.LegacyProfileRequired)
                {
                    DialogResult = DialogResult.Retry;
                    Close();
                    return;
                }
                error.Text = FriendlyError(exception.ErrorCode);
            }
            catch (HttpRequestException)
            {
                error.Text = "Connessione al servizio non disponibile. Verifica Internet e riprova.";
            }
            catch (TaskCanceledException)
            {
                error.Text = "Il servizio non ha risposto in tempo. Riprova.";
            }
            finally { SetBusy(false); }
        }

        private void SetBusy(bool busy)
        {
            confirm.Enabled = !busy;
            firstName.Enabled = lastName.Enabled = email.Enabled = pin.Enabled = !busy;
            confirm.Text = busy ? "Verifica in corso..." : (mode == CLDeviceLicenseMode.NewInstallation ? "Attiva" : "Associa installazione");
        }

        private static string FriendlyError(string code)
        {
            if (code == "activation_denied") return "Email o codice di installazione non validi.";
            if (code == "device_limit_reached") return "Questa email ha già due dispositivi attivi.";
            if (code == "user_assignment_conflict" || code == "installation_already_assigned") return "L'installazione è già associata a un'altra utenza. Contatta Avensys.";
            return "Attivazione non riuscita. Verifica i dati o contatta Avensys.";
        }
    }
}
