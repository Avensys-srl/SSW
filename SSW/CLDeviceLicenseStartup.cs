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

        private static string L(CLMessageResources key)
        {
            return CLEnvironment.Current.GetLocalizedString(key);
        }

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
                    MessageBox.Show(L(CLMessageResources.DeviceLicense_NotActive), L(CLMessageResources.DeviceLicense_LicenseTitle), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            MessageBox.Show(L(CLMessageResources.DeviceLicense_InternetRequired), L(CLMessageResources.DeviceLicense_LicenseTitle), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        MessageBox.Show(L(CLMessageResources.DeviceLicense_Revoked), L(CLMessageResources.DeviceLicense_LicenseTitle), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private readonly TextBox company = new TextBox();
        private readonly TextBox pin = new TextBox();
        private readonly Label error = new Label();
        private readonly Button confirm = new Button();
        private readonly CheckBox privacyAcknowledgement = new CheckBox();

        private static string L(CLMessageResources key)
        {
            return CLEnvironment.Current.GetLocalizedString(key);
        }

        internal CLDeviceLicenseForm(CLDeviceLicenseMode mode)
        {
            this.mode = mode;
            Text = mode == CLDeviceLicenseMode.NewInstallation
                ? L(CLMessageResources.DeviceLicense_ActivationTitle)
                : L(CLMessageResources.DeviceLicense_ProfileTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            ClientSize = new Size(620, mode == CLDeviceLicenseMode.NewInstallation ? 725 : 665);
            MinimumSize = ClientSize;

            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 5,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 195F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(239, 247, 243), Padding = new Padding(32, 20, 32, 14) };
            var eyebrow = new Label {
                Text = L(CLMessageResources.DeviceLicense_Eyebrow),
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
                    ? L(CLMessageResources.DeviceLicense_ActivationDescription)
                    : L(CLMessageResources.DeviceLicense_ProfileDescription),
                ForeColor = Color.FromArgb(83, 105, 97),
                Location = new Point(32, 82),
                Size = new Size(545, 28)
            };
            header.Controls.Add(description);
            header.Controls.Add(title);
            header.Controls.Add(eyebrow);
            root.Controls.Add(header, 0, 0);

            var fields = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = mode == CLDeviceLicenseMode.NewInstallation ? 4 : 3,
                Padding = new Padding(24, 18, 24, 8),
                BackColor = Color.White
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int row = 0; row < fields.RowCount; row++) fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / fields.RowCount));
            fields.Controls.Add(FieldBlock(L(CLMessageResources.DeviceLicense_FirstName), firstName), 0, 0);
            fields.Controls.Add(FieldBlock(L(CLMessageResources.DeviceLicense_LastName), lastName), 1, 0);
            fields.Controls.Add(FieldBlock(L(CLMessageResources.DeviceLicense_Email), email), 0, 1);
            fields.SetColumnSpan(fields.GetControlFromPosition(0, 1), 2);
            fields.Controls.Add(FieldBlock(L(CLMessageResources.DeviceLicense_Company), company), 0, 2);
            fields.SetColumnSpan(fields.GetControlFromPosition(0, 2), 2);
            if (mode == CLDeviceLicenseMode.NewInstallation)
            {
                pin.MaxLength = 6;
                pin.TextAlign = HorizontalAlignment.Center;
                fields.Controls.Add(FieldBlock(L(CLMessageResources.DeviceLicense_InstallationCode), pin,
                    L(CLMessageResources.DeviceLicense_InstallationCodeHint)), 0, 3);
                fields.SetColumnSpan(fields.GetControlFromPosition(0, 3), 2);
            }
            root.Controls.Add(fields, 0, 1);

            var privacy = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(30, 4, 30, 4),
                BackColor = Color.White
            };
            privacy.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            privacy.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            privacy.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            privacy.Controls.Add(new Label {
                Text = L(CLMessageResources.DeviceLicense_PrivacyTitle),
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(29, 58, 48),
                Font = new Font("Segoe UI Semibold", 9F)
            }, 0, 0);
            privacy.Controls.Add(new RichTextBox {
                Text = L(CLMessageResources.DeviceLicense_PrivacyNotice),
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 251, 251),
                ForeColor = Color.FromArgb(52, 68, 62),
                Font = new Font("Segoe UI", 8.5F),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = true
            }, 0, 1);
            privacyAcknowledgement.Text = L(CLMessageResources.DeviceLicense_PrivacyAcknowledgement);
            privacyAcknowledgement.Dock = DockStyle.Fill;
            privacyAcknowledgement.Font = new Font("Segoe UI Semibold", 8.5F);
            privacyAcknowledgement.CheckedChanged += (_, __) => confirm.Enabled = privacyAcknowledgement.Checked;
            privacy.Controls.Add(privacyAcknowledgement, 0, 2);
            root.Controls.Add(privacy, 0, 2);

            error.ForeColor = Color.Firebrick;
            error.BackColor = Color.FromArgb(255, 247, 247);
            error.Dock = DockStyle.Fill;
            error.Padding = new Padding(28, 11, 28, 8);
            error.AutoEllipsis = true;
            root.Controls.Add(error, 0, 3);

            confirm.Text = ConfirmText();
            confirm.Enabled = false;
            confirm.Size = new Size(190, 40);
            confirm.BackColor = Color.FromArgb(32, 127, 88);
            confirm.ForeColor = Color.White;
            confirm.FlatStyle = FlatStyle.Flat;
            confirm.FlatAppearance.BorderSize = 0;
            confirm.Font = new Font("Segoe UI Semibold", 9.5F);
            confirm.Click += async (_, __) => await SubmitAsync();
            var cancel = new Button {
                Text = L(CLMessageResources.DeviceLicense_Exit),
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
            root.Controls.Add(actions, 0, 4);
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
                String.IsNullOrWhiteSpace(email.Text) || !email.Text.Contains("@") || String.IsNullOrWhiteSpace(company.Text) ||
                (mode == CLDeviceLicenseMode.NewInstallation && (pin.Text.Length != 6 || !Int32.TryParse(pin.Text, out _))))
            {
                error.Text = L(CLMessageResources.DeviceLicense_InvalidFields);
                return;
            }
            SetBusy(true);
            try
            {
                var client = new CLSelectionApiClient();
                var context = CLSelectionRegistrationContext.FromEnvironment(CLEnvironment.Current);
                CLDeviceLicenseResult result = mode == CLDeviceLicenseMode.NewInstallation
                    ? await client.ActivateDeviceLicenseAsync(firstName.Text, lastName.Text, email.Text, company.Text, pin.Text, context)
                    : await client.ClaimLegacyDeviceLicenseAsync(firstName.Text, lastName.Text, email.Text, company.Text, context);
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
                error.Text = L(CLMessageResources.DeviceLicense_ConnectionUnavailable);
            }
            catch (TaskCanceledException)
            {
                error.Text = L(CLMessageResources.DeviceLicense_RequestTimeout);
            }
            finally { SetBusy(false); }
        }

        private void SetBusy(bool busy)
        {
            confirm.Enabled = !busy && privacyAcknowledgement.Checked;
            firstName.Enabled = lastName.Enabled = email.Enabled = company.Enabled = pin.Enabled = !busy;
            confirm.Text = busy ? L(CLMessageResources.DeviceLicense_Checking) : ConfirmText();
        }

        private string ConfirmText()
        {
            return mode == CLDeviceLicenseMode.NewInstallation
                ? L(CLMessageResources.DeviceLicense_Activate)
                : L(CLMessageResources.DeviceLicense_Register);
        }

        private static string FriendlyError(string code)
        {
            if (code == "activation_denied") return L(CLMessageResources.DeviceLicense_InvalidActivation);
            if (code == "device_limit_reached") return L(CLMessageResources.DeviceLicense_DeviceLimit);
            if (code == "user_assignment_conflict" || code == "installation_already_assigned")
                return L(CLMessageResources.DeviceLicense_AssignmentConflict);
            return L(CLMessageResources.DeviceLicense_ActivationFailed);
        }
    }
}
