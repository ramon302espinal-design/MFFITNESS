using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;
using CORE.Update;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    /// <summary>
    /// Centro de actualizaciones OTA: consultar → preparar → lanzar UpdateManager.
    /// </summary>
    public sealed class FrmActualizacion : Form
    {
        private readonly Panel _header;
        private readonly Label _lblHeaderTitle;
        private readonly Label _lblHeaderSub;

        private readonly Panel _cardInstalled;
        private readonly Label _lblInstalledTitle;
        private readonly Label _lblInstalledValue;

        private readonly Panel _cardStatus;
        private readonly Panel _badgeAccent;
        private readonly Label _lblStatusTitle;
        private readonly Label _lblStatusHint;

        private readonly Panel _cardReqs;
        private readonly Label _lblReqCaja;
        private readonly Label _lblReqManager;
        private readonly Label _lblSession;

        private readonly Label _lblDetail;
        private readonly ProgressBar _progress;

        private readonly Button _btnCheck;
        private readonly Button _btnInstall;
        private readonly Button _btnClose;

        private UpdateLaunchBLL.PreparedUpdate? _prepared;
        private bool _busy;

        public FrmActualizacion()
        {
            Text = "Actualización del sistema";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(640, 560);
            BackColor = AppTheme.Background;
            Font = AppTheme.FontBody;
            Padding = new Padding(0);

            _header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = AppTheme.Primary
            };

            _lblHeaderTitle = new Label
            {
                AutoSize = false,
                Location = new Point(24, 14),
                Size = new Size(592, 28),
                Font = AppTheme.FontSubtitle,
                ForeColor = AppTheme.TextOnPrimary,
                Text = "Centro de actualizaciones",
                BackColor = Color.Transparent
            };

            _lblHeaderSub = new Label
            {
                AutoSize = false,
                Location = new Point(24, 42),
                Size = new Size(592, 22),
                Font = AppTheme.FontCaption,
                ForeColor = Color.FromArgb(220, 235, 255),
                Text = "Descarga verificada (SHA256) · instalación segura con backup y recovery",
                BackColor = Color.Transparent
            };

            _header.Controls.Add(_lblHeaderTitle);
            _header.Controls.Add(_lblHeaderSub);

            _cardInstalled = CreateCard(24, 88, 592, 86);
            _lblInstalledTitle = CreateSectionTitle(16, 12, "INSTALACIÓN ACTUAL");
            _lblInstalledValue = new Label
            {
                AutoSize = false,
                Location = new Point(16, 38),
                Size = new Size(560, 36),
                Font = AppTheme.FontBody,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent
            };
            _cardInstalled.Controls.Add(_lblInstalledTitle);
            _cardInstalled.Controls.Add(_lblInstalledValue);

            _cardStatus = CreateCard(24, 186, 592, 100);
            _badgeAccent = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(6, 100),
                BackColor = AppTheme.Info
            };
            _lblStatusTitle = new Label
            {
                AutoSize = false,
                Location = new Point(20, 14),
                Size = new Size(556, 28),
                Font = AppTheme.FontSubtitle,
                ForeColor = AppTheme.TextPrimary,
                Text = "Sin consultar",
                BackColor = Color.Transparent
            };
            _lblStatusHint = new Label
            {
                AutoSize = false,
                Location = new Point(20, 46),
                Size = new Size(556, 42),
                Font = AppTheme.FontCaption,
                ForeColor = AppTheme.TextSecondary,
                Text = "Pulse «Buscar» para consultar el último release en GitHub.",
                BackColor = Color.Transparent
            };
            _cardStatus.Controls.Add(_badgeAccent);
            _cardStatus.Controls.Add(_lblStatusTitle);
            _cardStatus.Controls.Add(_lblStatusHint);

            _cardReqs = CreateCard(24, 298, 592, 110);
            var lblReqTitle = CreateSectionTitle(16, 10, "REQUISITOS");
            _lblReqCaja = CreateReqLabel(16, 36);
            _lblReqManager = CreateReqLabel(16, 58);
            _lblSession = new Label
            {
                AutoSize = false,
                Location = new Point(16, 82),
                Size = new Size(560, 20),
                Font = AppTheme.FontCaption,
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent
            };
            _cardReqs.Controls.Add(lblReqTitle);
            _cardReqs.Controls.Add(_lblReqCaja);
            _cardReqs.Controls.Add(_lblReqManager);
            _cardReqs.Controls.Add(_lblSession);

            _lblDetail = new Label
            {
                AutoSize = false,
                Location = new Point(24, 420),
                Size = new Size(592, 36),
                Font = AppTheme.FontCaption,
                ForeColor = AppTheme.TextSecondary,
                Text = string.Empty
            };

            _progress = new ProgressBar
            {
                Location = new Point(24, 458),
                Size = new Size(592, 12),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 28,
                Visible = false
            };

            _btnCheck = new Button
            {
                Text = "Buscar",
                Location = new Point(24, 490),
                Size = new Size(140, 42),
                Cursor = Cursors.Hand
            };
            _btnCheck.Click += async (_, _) => await CheckAsync();

            _btnInstall = new Button
            {
                Text = "Instalar actualización",
                Location = new Point(176, 490),
                Size = new Size(220, 42),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            _btnInstall.Click += async (_, _) => await InstallAsync();

            _btnClose = new Button
            {
                Text = "Cerrar",
                Location = new Point(500, 490),
                Size = new Size(116, 42),
                Cursor = Cursors.Hand
            };
            _btnClose.Click += (_, _) => Close();

            Controls.Add(_header);
            Controls.Add(_cardInstalled);
            Controls.Add(_cardStatus);
            Controls.Add(_cardReqs);
            Controls.Add(_lblDetail);
            Controls.Add(_progress);
            Controls.Add(_btnCheck);
            Controls.Add(_btnInstall);
            Controls.Add(_btnClose);

            ThemeApplier.StyleButton(_btnCheck, ButtonVariant.Secondary);
            ThemeApplier.StyleButton(_btnInstall, ButtonVariant.Primary);
            ThemeApplier.StyleButton(_btnClose, ButtonVariant.Secondary);

            CancelButton = _btnClose;
            Shown += async (_, _) =>
            {
                RefreshLocalUi();
                await CheckAsync();
            };
        }

        private static Panel CreateCard(int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = AppTheme.Surface,
                Padding = new Padding(0)
            };
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(AppTheme.Border);
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            };
            return card;
        }

        private static Label CreateSectionTitle(int x, int y, string text) => new()
        {
            AutoSize = false,
            Location = new Point(x, y),
            Size = new Size(560, 20),
            Font = AppTheme.FontCaption,
            ForeColor = AppTheme.TextMuted,
            Text = text,
            BackColor = Color.Transparent
        };

        private static Label CreateReqLabel(int x, int y) => new()
        {
            AutoSize = false,
            Location = new Point(x, y),
            Size = new Size(560, 20),
            Font = AppTheme.FontCaption,
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent
        };

        private void RefreshLocalUi()
        {
            var snap = UpdateCenterSupport.CaptureLocal();

            string dbText = snap.DbVersion.HasValue
                ? $"DB {snap.DbVersion.Value}"
                : $"DB n/d{(string.IsNullOrWhiteSpace(snap.DbError) ? string.Empty : " · " + Truncate(snap.DbError, 48))}";

            _lblInstalledValue.Text =
                $"App {snap.InstalledAppVersion}  ·  Build {snap.Build}  ·  {dbText}";

            if (snap.CajaCheckOk)
            {
                if (snap.CajaAbierta)
                {
                    _lblReqCaja.Text = "✗  Caja abierta — cierre caja antes de instalar";
                    _lblReqCaja.ForeColor = AppTheme.Error;
                }
                else
                {
                    _lblReqCaja.Text = "✓  Caja cerrada";
                    _lblReqCaja.ForeColor = AppTheme.Success;
                }
            }
            else
            {
                _lblReqCaja.Text = "✗  No se pudo verificar caja (fail-closed)";
                _lblReqCaja.ForeColor = AppTheme.Error;
            }

            if (snap.UpdateManagerPresent)
            {
                _lblReqManager.Text = "✓  UpdateManager disponible";
                _lblReqManager.ForeColor = AppTheme.Success;
            }
            else
            {
                _lblReqManager.Text = "✗  Falta UpdateManager\\UpdateManager.exe (despliegue inicial)";
                _lblReqManager.ForeColor = AppTheme.Error;
            }

            _lblSession.Text = UpdateCenterSupport.FormatSessionSummary(snap.LastSession);
        }

        private void ApplyStatus(UpdateLaunchStatus status, string message, UpdateManifest? manifest)
        {
            var view = UpdateCenterSupport.MapLaunchStatus(status, message);
            _badgeAccent.BackColor = view.Accent;
            _lblStatusTitle.Text = view.Title;
            _lblStatusTitle.ForeColor = view.Accent;
            _lblStatusHint.Text = view.Hint;

            if (manifest != null)
            {
                _lblDetail.Text =
                    $"Release: App {manifest.AppVersion}  ·  Target DB {manifest.TargetDbVersion}"
                    + $"  ·  Min App {manifest.MinAppVersion}"
                    + (string.IsNullOrWhiteSpace(manifest.PackageName)
                        ? string.Empty
                        : $"  ·  {manifest.PackageName}");
            }
            else if (string.IsNullOrWhiteSpace(_lblDetail.Text))
            {
                _lblDetail.Text = message;
            }

            bool canInstall = view.CanInstall && !_busy;
            if (canInstall)
            {
                var snap = UpdateCenterSupport.CaptureLocal();
                canInstall = snap.UpdateManagerPresent
                             && snap.CajaCheckOk
                             && !snap.CajaAbierta;
            }

            _btnInstall.Enabled = canInstall;
        }

        private async Task CheckAsync()
        {
            if (_busy)
                return;

            SetBusy(true, "Consultando GitHub Releases…");
            _prepared = null;
            RefreshLocalUi();

            try
            {
                var result = await UpdateLaunchBLL.CheckForUpdateAsync().ConfigureAwait(true);
                ApplyStatus(result.Status, result.Message, result.Manifest);
            }
            catch (Exception ex)
            {
                ApplyStatus(UpdateLaunchStatus.Failed, ex.Message, null);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task InstallAsync()
        {
            if (_busy)
                return;

            RefreshLocalUi();
            var snap = UpdateCenterSupport.CaptureLocal();
            if (!snap.UpdateManagerPresent)
            {
                AppDialog.Error(this,
                    "No se encontró UpdateManager\\UpdateManager.exe.\n"
                    + "El despliegue inicial debe incluir la carpeta UpdateManager.",
                    "UpdateManager requerido");
                return;
            }

            if (!snap.CajaCheckOk || snap.CajaAbierta)
            {
                AppDialog.Warning(this,
                    "La caja debe estar cerrada antes de instalar una actualización.",
                    "Caja abierta");
                return;
            }

            bool ok = AppDialog.Confirm(this,
                "Se descargará el paquete, se verificará el SHA256 y se cerrará la aplicación.\n\n"
                + "UpdateManager realizará backup, instalación, migraciones y health check.\n"
                + "No interrumpa el proceso.",
                "Confirmar actualización");
            if (!ok)
                return;

            SetBusy(true, "Descargando y verificando paquete…");
            try
            {
                var (result, prepared) = await UpdateLaunchBLL.DownloadLatestAsync().ConfigureAwait(true);
                ApplyStatus(result.Status, result.Message, result.Manifest);

                if (result.Status == UpdateLaunchStatus.Blocked)
                {
                    AppDialog.Warning(this, result.Message, "Actualización bloqueada");
                    return;
                }

                if (prepared == null || result.Status != UpdateLaunchStatus.Prepared)
                {
                    AppDialog.Error(this, result.Message, "No se pudo preparar la actualización");
                    return;
                }

                _prepared = prepared;
                SetBusy(true, "Iniciando UpdateManager…");
                var launch = UpdateLaunchBLL.WriteRequestAndLaunch(prepared);
                ApplyStatus(launch.Status, launch.Message, prepared.Manifest);

                if (launch.Status != UpdateLaunchStatus.Launched)
                {
                    AppDialog.Error(this, launch.Message, "No se pudo iniciar UpdateManager");
                    return;
                }

                AppDialog.Info(this,
                    "UpdateManager se inició correctamente.\nLa aplicación se cerrará ahora para liberar los archivos.",
                    "Actualización en curso");

                Application.Exit();
            }
            catch (Exception ex)
            {
                AppDialog.Error(this, ex.Message, "Error de actualización");
                ApplyStatus(UpdateLaunchStatus.Failed, ex.Message, null);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy, string? statusHint = null)
        {
            _busy = busy;
            _progress.Visible = busy;
            _btnCheck.Enabled = !busy;
            _btnClose.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;

            if (!string.IsNullOrWhiteSpace(statusHint))
            {
                _lblStatusTitle.Text = busy ? "Trabajando…" : _lblStatusTitle.Text;
                _lblStatusHint.Text = statusHint;
                _badgeAccent.BackColor = AppTheme.Primary;
            }

            if (busy)
                _btnInstall.Enabled = false;
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
                return value;
            return value[..(max - 1)] + "…";
        }
    }
}
