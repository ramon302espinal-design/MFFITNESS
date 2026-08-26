using System;
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
    /// UI en FrmActualizacion.Designer.cs (editable en el diseñador WinForms).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmActualizacion : Form
    {
        private UpdateLaunchBLL.PreparedUpdate? _prepared;
        private bool _busy;
        /// <summary>True si hay update instalable (caja/UM OK). SetBusy solo lo oculta mientras trabaja.</summary>
        private bool _installAllowed;

        public FrmActualizacion()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        private async void FrmActualizacion_Shown(object? sender, EventArgs e)
        {
            RefreshLocalUi();
            await CheckAsync();
        }

        private async void btnCheck_Click(object? sender, EventArgs e)
        {
            await CheckAsync();
        }

        private async void btnInstall_Click(object? sender, EventArgs e)
        {
            await InstallAsync();
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void RefreshLocalUi()
        {
            var snap = UpdateCenterSupport.CaptureLocal();

            string dbText = snap.DbVersion.HasValue
                ? $"DB {snap.DbVersion.Value}"
                : $"DB n/d{(string.IsNullOrWhiteSpace(snap.DbError) ? string.Empty : " · " + Truncate(snap.DbError, 48))}";

            lblInstalledValue.Text =
                $"App {snap.InstalledAppVersion}  ·  Build {snap.Build}  ·  {dbText}";

            if (snap.CajaCheckOk)
            {
                if (snap.CajaAbierta)
                {
                    lblReqCaja.Text = "✗  Caja abierta — cierre caja antes de instalar";
                    lblReqCaja.ForeColor = AppTheme.Error;
                }
                else
                {
                    lblReqCaja.Text = "✓  Caja cerrada";
                    lblReqCaja.ForeColor = AppTheme.Success;
                }
            }
            else
            {
                lblReqCaja.Text = "✗  No se pudo verificar caja (fail-closed)";
                lblReqCaja.ForeColor = AppTheme.Error;
            }

            if (snap.UpdateManagerPresent)
            {
                lblReqManager.Text = "✓  UpdateManager disponible";
                lblReqManager.ForeColor = AppTheme.Success;
            }
            else
            {
                lblReqManager.Text = "✗  Falta UpdateManager\\UpdateManager.exe (despliegue inicial)";
                lblReqManager.ForeColor = AppTheme.Error;
            }

            lblSession.Text = UpdateCenterSupport.FormatSessionSummary(snap.LastSession);
        }

        private void ApplyStatus(UpdateLaunchStatus status, string message, UpdateManifest? manifest)
        {
            var view = UpdateCenterSupport.MapLaunchStatus(status, message);
            panelBadgeAccent.BackColor = view.Accent;
            lblStatusTitle.Text = view.Title;
            lblStatusTitle.ForeColor = view.Accent;
            lblStatusHint.Text = view.Hint;

            if (manifest != null)
            {
                lblDetail.Text =
                    $"Release: App {manifest.AppVersion}  ·  Target DB {manifest.TargetDbVersion}"
                    + $"  ·  Min App {manifest.MinAppVersion}"
                    + (string.IsNullOrWhiteSpace(manifest.PackageName)
                        ? string.Empty
                        : $"  ·  {manifest.PackageName}");
            }
            else if (string.IsNullOrWhiteSpace(lblDetail.Text))
            {
                lblDetail.Text = message;
            }

            // NO mezclar con _busy aquí: CheckAsync llama ApplyStatus con busy=true
            // y luego SetBusy(false) debe rehabilitar el botón.
            bool canInstall = view.CanInstall;
            if (canInstall)
            {
                var snap = UpdateCenterSupport.CaptureLocal();
                canInstall = snap.UpdateManagerPresent
                             && snap.CajaCheckOk
                             && !snap.CajaAbierta;
            }

            _installAllowed = canInstall;
            SyncInstallButtonEnabled();
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

                // CRÍTICO: no mostrar diálogo modal aquí.
                // UpdateManager ya está en marcha y espera el cierre de UI.exe (UiCloseTimeout).
                // Un MessageBox bloqueaba el Exit y provocaba "UI no terminó dentro del timeout".
                // También: Application.Exit cierra Login oculto; Environment.Exit es cinturón.
                UpdateExitSignal.ForceExitRequested = true;
                try { Application.Exit(); } catch { /* ignore */ }
                Environment.Exit(0);
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
            progressBar.Visible = busy;
            btnCheck.Enabled = !busy;
            btnClose.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;

            if (!string.IsNullOrWhiteSpace(statusHint))
            {
                lblStatusTitle.Text = busy ? "Trabajando…" : lblStatusTitle.Text;
                lblStatusHint.Text = statusHint;
                panelBadgeAccent.BackColor = AppTheme.Primary;
            }

            SyncInstallButtonEnabled();
        }

        private void SyncInstallButtonEnabled()
        {
            btnInstall.Enabled = _installAllowed && !_busy;
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
                return value;
            return value[..(max - 1)] + "…";
        }
    }
}
