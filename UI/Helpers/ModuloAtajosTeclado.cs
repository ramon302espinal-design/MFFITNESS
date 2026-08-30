using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using UI.DISEÑO;
using UI.DISEÑO.CHAT;

namespace UI.Helpers
{
    /// <summary>
    /// Atajos globales: P Cobrar · C Caja · E Estado · D Deudas · H Historial · R Reportes · I Inventario · M Clientes · W WhatsApp.
    /// FrmPagos productos: Shift → financiar · Espacio/Enter → COBRAR.
    /// </summary>
    internal static class ModuloAtajosTeclado
    {
        private static bool _filtroGlobalInstalado;
        private static readonly object AtajoLock = new();
        private static int _ultimoAtajoTick;
        private static Keys _ultimoAtajoKey;
        private static readonly ConditionalWeakTable<Form, object> FormsConAtajos = new();

        public static void AsegurarFiltroGlobal()
        {
            if (_filtroGlobalInstalado)
                return;

            Application.AddMessageFilter(new FiltroAtajosModulos());
            _filtroGlobalInstalado = true;
        }

        public static bool TryHandleNavegacion(KeyEventArgs e, Form host, IntPtr hwndOrigen = default)
        {
            if (!EsTeclaModuloSola(e))
                return false;

            if (BusquedaFocusHelper.EsEntradaTextoActiva(hwndOrigen, host))
                return false;

            Keys key = e.KeyCode;
            if (!EsTeclaAtajo(key))
                return false;

            if (!IntentarReservarAtajo(key))
                return false;

            e.Handled = true;
            e.SuppressKeyPress = true;

            Form hostCapture = host;
            void Ejecutar()
            {
                try
                {
                    EjecutarAtajoNavegacion(key, hostCapture);
                }
                catch
                {
                    // Ignorar: el módulo destino ya muestra errores al usuario.
                }
            }

            if (hostCapture.IsHandleCreated && !hostCapture.IsDisposed)
                hostCapture.BeginInvoke(Ejecutar);
            else
                Ejecutar();

            return true;
        }

        /// <summary>Shift (solo) en pestaña Productos → FINANCIAR.</summary>
        public static bool TryHandleFinanciamientoProducto(
            KeyEventArgs e,
            Form host,
            Button btnFinanciamiento,
            TabControl tabProductos,
            TabPage tabPago,
            Panel? panelFinanciamientoProducto)
        {
            if (e.KeyCode is not (Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey))
                return false;

            if (e.Alt || e.Control)
                return false;

            if (tabProductos.SelectedTab != tabPago)
                return false;

            if (!btnFinanciamiento.Enabled || !btnFinanciamiento.Visible)
                return false;

            if (BusquedaFocusHelper.EsEntradaTextoActiva(default, host))
                return false;

            if (panelFinanciamientoProducto != null
                && panelFinanciamientoProducto.Visible
                && panelFinanciamientoProducto.ContainsFocus)
                return false;

            btnFinanciamiento.PerformClick();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return true;
        }

        public static void WireAtajosEnFormulario(Form host)
        {
            AsegurarFiltroGlobal();

            if (host == null || host.IsDisposed)
                return;

            host.KeyPreview = true;

            // FrmPresentacion y FrmPagos tienen KeyDown propio (escáner + atajos).
            if (host is FrmPresentacion or FrmPagos)
                return;

            if (FormsConAtajos.TryGetValue(host, out _))
                return;

            FormsConAtajos.Add(host, new object());
            host.KeyDown += OnFormKeyDownAtajos;
        }

        private static void OnFormKeyDownAtajos(object? sender, KeyEventArgs e)
        {
            if (sender is Form form)
                TryHandleNavegacion(e, form);
        }

        private static void EjecutarAtajoNavegacion(Keys key, Form host)
        {
            host = ResolverHostNavegacion(host);

            switch (key)
            {
                case Keys.P:
                    if (YaEnModulo(host, ModuloNavBar.ModuloPagar))
                        return;
                    ModuloNavBar.AbrirCobrar(host);
                    break;
                case Keys.C:
                    if (YaEnModulo(host, ModuloNavBar.ModuloCaja))
                        return;
                    ModuloNavBar.AbrirCaja(host);
                    break;
                case Keys.E:
                    if (YaEnModulo(host, ModuloNavBar.ModuloEstado))
                        return;
                    ModuloNavBar.AbrirEstado(host);
                    break;
                case Keys.D:
                    if (YaEnModulo(host, ModuloNavBar.ModuloDeudas))
                        return;
                    ModuloNavBar.AbrirDeudas(host);
                    break;
                case Keys.H:
                    if (YaEnModulo(host, ModuloNavBar.ModuloHistorial))
                        return;
                    ModuloNavBar.AbrirHistorial(host);
                    break;
                case Keys.R:
                    if (host is FrmCRMFinanciero crm)
                    {
                        crm.ActivarVistaReportesPos();
                        return;
                    }
                    if (YaEnModulo(host, ModuloNavBar.ModuloReportes))
                        return;
                    ModuloNavBar.AbrirReportes(host);
                    break;
                case Keys.I:
                    if (YaEnModulo(host, ModuloNavBar.ModuloInventario))
                        return;
                    ModuloNavBar.AbrirInventario(host);
                    break;
                case Keys.M:
                    if (YaEnModulo(host, ModuloNavBar.ModuloClientes))
                        return;
                    ModuloNavBar.AbrirClientes(host);
                    break;
                case Keys.W:
                    if (YaEnModulo(host, ModuloNavBar.ModuloChat))
                        return;
                    ModuloNavBar.AbrirChat(host);
                    break;
            }
        }

        /// <summary>Formulario contenedor (módulo embebido → shell padre).</summary>
        internal static Form ResolverHostNavegacion(Form host)
        {
            if (host == null || host.IsDisposed)
            {
                Form? activa = Form.ActiveForm;
                return activa != null && !activa.IsDisposed ? activa : host!;
            }

            Form? outermost = host;
            for (Control? c = host; c != null; c = c.Parent)
            {
                if (c is Form f)
                    outermost = f;
            }

            return outermost ?? host;
        }

        private static bool YaEnModulo(Form host, string modulo) =>
            modulo switch
            {
                ModuloNavBar.ModuloPagar => host is FrmPagos,
                ModuloNavBar.ModuloCaja => host is FrmCajaDashboard,
                ModuloNavBar.ModuloEstado => host is FrmEstadoClientes,
                ModuloNavBar.ModuloDeudas => host is FrmModuloDeudas,
                ModuloNavBar.ModuloHistorial => host is FrmHistorialVentas,
                ModuloNavBar.ModuloReportes => host is FrmCRMFinanciero,
                ModuloNavBar.ModuloInventario => host is FrmProductos,
                ModuloNavBar.ModuloClientes => host is FrmClientes,
                ModuloNavBar.ModuloChat => host is FrmChat,
                _ => false
            };

        private static bool EsTeclaAtajo(Keys key) => key switch
        {
            Keys.P or Keys.C or Keys.E or Keys.D or Keys.H or Keys.R or Keys.I or Keys.M or Keys.W => true,
            _ => false
        };

        private static bool IntentarReservarAtajo(Keys key)
        {
            lock (AtajoLock)
            {
                int now = Environment.TickCount;
                if (key == _ultimoAtajoKey && unchecked(now - _ultimoAtajoTick) < 250)
                    return false;

                _ultimoAtajoKey = key;
                _ultimoAtajoTick = now;
                return true;
            }
        }

        private static bool EsTeclaModuloSola(KeyEventArgs e) =>
            !e.Alt && !e.Control && !e.Shift;

        private static Form? ResolverFormularioDesdeMensaje(Message m)
        {
            try
            {
                Control? c = Control.FromHandle(m.HWnd);
                while (c != null)
                {
                    if (c is Form f && !f.IsDisposed)
                        return f;
                    c = c.Parent;
                }
            }
            catch
            {
                // Handle inválido durante cierre de ventana.
            }

            return null;
        }

        private sealed class FiltroAtajosModulos : IMessageFilter
        {
            private const int WmKeyDown = 0x0100;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WmKeyDown)
                    return false;

                Form? form = ResolverFormularioDesdeMensaje(m)
                    ?? Form.ActiveForm;
                if (form == null || form.IsDisposed || !form.Visible)
                    return false;

                Form host = ResolverHostNavegacion(form);

                Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;
                Keys keyData = keyCode | Control.ModifierKeys;
                var e = new KeyEventArgs(keyData);

                return TryHandleNavegacion(e, host, m.HWnd);
            }
        }
    }
}
