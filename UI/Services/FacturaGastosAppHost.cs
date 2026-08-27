using BLL;
using CORE;
using UI.DISEÑO;
using UI.Services;

namespace UI.Services
{
    /// <summary>
    /// Host de aplicación para FacturaGastos: vive con FrmPresentacion,
    /// no se cancela al cerrar el módulo Caja (evita matar Ollama a mitad).
    /// Solo registra egreso si la caja está ABIERTA en BD.
    /// </summary>
    public static class FacturaGastosAppHost
    {
        private static readonly object Sync = new();
        private static FacturaGastosWatcherHost? _watcher;
        private static Form? _uiAnchor;

        public static void Start(Form uiAnchor)
        {
            lock (Sync)
            {
                _uiAnchor = uiAnchor ?? throw new ArgumentNullException(nameof(uiAnchor));
                FrmFacturaGastoHud.EnsureCreated();

                _watcher?.Dispose();
                _watcher = new FacturaGastosWatcherHost(
                    owner: uiAnchor,
                    isCajaAbierta: () =>
                    {
                        try { return new CajaBLL().ObtenerEstadoCaja(); }
                        catch { return false; }
                    },
                    onGastoRegistrado: () =>
                    {
                        try { AppEventos.CajaCambiada(); } catch { /* ignore */ }
                    },
                    onVerMovimientos: () =>
                    {
                        Form? anchor = _uiAnchor;
                        if (anchor == null || anchor.IsDisposed)
                            return;

                        void Open()
                        {
                            using var frm = new FrmMovimientosCaja(anchor);
                            frm.ShowDialog(anchor);
                        }

                        if (anchor.InvokeRequired)
                            anchor.BeginInvoke(Open);
                        else
                            Open();
                    },
                    onStatusChanged: null);

                _watcher.Start();
            }
        }

        public static void Stop()
        {
            lock (Sync)
            {
                try { _watcher?.Dispose(); } catch { /* ignore */ }
                _watcher = null;
            }
        }

        /// <summary>Reescanea pendientes cuando la caja pasa a ABIERTA.</summary>
        public static void NotifyCajaState(bool cajaAbierta)
        {
            lock (Sync)
            {
                _watcher?.SyncCajaState(cajaAbierta);
            }
        }
    }
}
