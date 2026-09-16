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
            ClientSize = new Size(470, mode == CLDeviceLicenseMode.NewInstallation ? 405 : 350);
            Font = new Font("Segoe UI", 9F);

            var title = new Label { Text = Text, Font = new Font(Font, FontStyle.Bold), Location = new Point(28, 24), AutoSize = true };
            var description = new Label {
                Text = mode == CLDeviceLicenseMode.NewInstallation
                    ? "Inserisci i dati comunicati ad Avensys e il codice di installazione ricevuto."
                    : "Questa installazione è già attiva. Associala alla persona che la utilizza.",
                Location = new Point(28, 56), Size = new Size(414, 38)
            };
            Controls.Add(title); Controls.Add(description);

            int y = 104;
            AddField("Nome", firstName, ref y);
            AddField("Cognome", lastName, ref y);
            AddField("Email", email, ref y);
            if (mode == CLDeviceLicenseMode.NewInstallation)
            {
                pin.MaxLength = 6;
                AddField("Codice di installazione (6 cifre)", pin, ref y);
            }

            error.ForeColor = Color.Firebrick;
            error.Location = new Point(28, y + 2);
            error.Size = new Size(414, 36);
            Controls.Add(error);

            confirm.Text = mode == CLDeviceLicenseMode.NewInstallation ? "Attiva" : "Associa installazione";
            confirm.Location = new Point(252, ClientSize.Height - 54);
            confirm.Size = new Size(190, 32);
            confirm.Click += async (_, __) => await SubmitAsync();
            var cancel = new Button { Text = "Esci", DialogResult = DialogResult.Cancel, Location = new Point(160, ClientSize.Height - 54), Size = new Size(82, 32) };
            Controls.Add(cancel); Controls.Add(confirm);
            AcceptButton = confirm;
            CancelButton = cancel;
        }

        private void AddField(string label, TextBox box, ref int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(28, y), AutoSize = true });
            box.Location = new Point(28, y + 20);
            box.Size = new Size(414, 25);
            Controls.Add(box);
            y += 60;
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
