using BLL;
using CORE;
using CORE.Update;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.Theme;

namespace UI.DISEÑO
{
    /// <summary>
    /// FASE 11 mínima: buscar / descargar / lanzar UpdateManager.
    /// </summary>
    public sealed class FrmActualizacion : Form
    {
        private readonly Label _lblStatus;
        private readonly Label _lblDetail;
        private readonly Button _btnCheck;
        private readonly Button _btnDownloadInstall;
        private readonly Button _btnClose;
        private readonly ProgressBar _progress;

        private UpdateLaunchBLL.PreparedUpdate? _prepared;

        public FrmActualizacion()
        {
            Text = "Actualización del sistema";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 280);
            BackColor = AppTheme.Surface;

            _lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(24, 24),
                Size = new Size(472, 28),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "Estado: sin consultar"
            };

            _lblDetail = new Label
            {
                AutoSize = false,
                Location = new Point(24, 60),
                Size = new Size(472, 80),
                Font = new Font("Segoe UI", 9F),
                Text = AppVersion.DisplayText.Replace(Environment.NewLine, " · ")
            };

            _progress = new ProgressBar
            {
                Location = new Point(24, 150),
                Size = new Size(472, 18),
                Style = ProgressBarStyle.Marquee,
                Visible = false,
                MarqueeAnimationSpeed = 30
            };

            _btnCheck = new Button
            {
                Text = "Buscar actualización",
                Location = new Point(24, 190),
                Size = new Size(180, 40),
                Cursor = Cursors.Hand
            };
            _btnCheck.Click += async (_, _) => await CheckAsync();

            _btnDownloadInstall = new Button
            {
                Text = "Descargar e instalar",
                Location = new Point(216, 190),
                Size = new Size(180, 40),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            _btnDownloadInstall.Click += async (_, _) => await DownloadAndInstallAsync();

            _btnClose = new Button
            {
                Text = "Cerrar",
                Location = new Point(408, 190),
                Size = new Size(88, 40),
                Cursor = Cursors.Hand
            };
            _btnClose.Click += (_, _) => Close();

            Controls.Add(_lblStatus);
            Controls.Add(_lblDetail);
            Controls.Add(_progress);
            Controls.Add(_btnCheck);
            Controls.Add(_btnDownloadInstall);
            Controls.Add(_btnClose);

            ThemeApplier.StyleButton(_btnCheck, ButtonVariant.Secondary);
            ThemeApplier.StyleButton(_btnDownloadInstall, ButtonVariant.Primary);
            ThemeApplier.StyleButton(_btnClose, ButtonVariant.Secondary);
        }

        private async Task CheckAsync()
        {
            SetBusy(true, "Consultando GitHub Releases...");
            _prepared = null;
            _btnDownloadInstall.Enabled = false;

            try
            {
                var result = await UpdateLaunchBLL.CheckForUpdateAsync().ConfigureAwait(true);
                _lblStatus.Text = "Estado: " + result.Status;
                _lblDetail.Text = result.Message;
                if (result.Manifest != null)
                {
                    _lblDetail.Text += Environment.NewLine
                        + $"Disponible: App {result.Manifest.AppVersion} · DB {result.Manifest.TargetDbVersion}";
                }

                _btnDownloadInstall.Enabled = result.Status == UpdateLaunchStatus.Available;
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Estado: error";
                _lblDetail.Text = ex.Message;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task DownloadAndInstallAsync()
        {
            var confirm = MessageBox.Show(
                "Se descargará el paquete, se cerrará esta aplicación y UpdateManager instalará la actualización."
                + Environment.NewLine + Environment.NewLine
                + "Requisitos: caja cerrada, no interrumpir el proceso.",
                "Confirmar actualización",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            SetBusy(true, "Descargando y verificando SHA256...");
            try
            {
                var (result, prepared) = await UpdateLaunchBLL.DownloadLatestAsync().ConfigureAwait(true);
                _lblStatus.Text = "Estado: " + result.Status;
                _lblDetail.Text = result.Message;

                if (result.Status == UpdateLaunchStatus.Blocked)
                {
                    MessageBox.Show(result.Message, "Actualización bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (prepared == null || result.Status != UpdateLaunchStatus.Prepared)
                {
                    MessageBox.Show(result.Message, "No se pudo preparar la actualización", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _prepared = prepared;
                SetBusy(true, "Iniciando UpdateManager...");
                var launch = UpdateLaunchBLL.WriteRequestAndLaunch(prepared);
                _lblStatus.Text = "Estado: " + launch.Status;
                _lblDetail.Text = launch.Message;

                if (launch.Status != UpdateLaunchStatus.Launched)
                {
                    MessageBox.Show(launch.Message, "No se pudo iniciar UpdateManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show(
                    "UpdateManager se inició. La aplicación se cerrará ahora.",
                    "Actualización en curso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Cerrar toda la app para liberar binarios.
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy, string? status = null)
        {
            _progress.Visible = busy;
            _btnCheck.Enabled = !busy;
            _btnClose.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                _lblStatus.Text = status;

            if (busy)
                _btnDownloadInstall.Enabled = false;

            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}
