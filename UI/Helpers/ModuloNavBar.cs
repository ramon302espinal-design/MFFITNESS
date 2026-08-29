using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using UI.DISEÑO;
using UI.Theme;

namespace UI.Helpers
{
    /// <summary>
    /// Barra superior: ◀ · COBRAR · DEUDAS · ESTADO · CAJA · HISTORIAL · INVENTARIO · REPORTES · CLIENTES
    /// </summary>
    public static class ModuloNavBar
    {
        public const string ModuloCaja = "CAJA";
        public const string ModuloDeudas = "DEUDAS";
        public const string ModuloEstado = "ESTADO";
        public const string ModuloInventario = "INVENTARIO";
        public const string ModuloHistorial = "HISTORIAL";
        public const string ModuloPagar = "PAGAR";
        public const string ModuloReportes = "REPORTES";
        public const string ModuloClientes = "CLIENTES";

        private const int Gap = 8;
        private const int StartX = 8;
        private const int BtnY = 10;
        private const int BtnH = 32;
        private const string TagRelayout = "nav-relayout";

        private static readonly string[] OrdenBotones =
        {
            "btnNavPagar",
            "btnNavDeudas",
            "btnNavEstado",
            "btnNavCaja",
            "btnNavHistorial",
            "btnNavInventario",
            "btnNavReportes",
            "btnNavClientes"
        };

        // Evita depender de Button.Tag (puede sobrescribirse) y permite re-wire seguro
        private static readonly ConditionalWeakTable<Button, object> WiredClicks = new();
        private static readonly object WiredMarker = new();

        public static void Wire(Panel panelNav, Form host, string moduloActual)
        {
            if (panelNav == null || host == null)
                return;
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            ModuloAtajosTeclado.AsegurarFiltroGlobal();

            OcultarBotonActual(panelNav, moduloActual);
            OrdenarBotones(panelNav);
            WireBack(panelNav, host);
            WireAllNavClicks(panelNav, host);
            ModuloAtajosTeclado.WireAtajosEnFormulario(host);
            BusquedaFocusHelper.Wire(host);
            BusquedaFocusHelper.WireFormulariosEmbebidos(host);
            AsegurarRelayout(panelNav, host, moduloActual);
        }

        public static void AbrirCaja(IWin32Window? owner)
        {
            using var frm = new FrmCajaDashboard();
            MostrarDialogo(frm, owner);
            ObtenerPresentacion()?.CargarDashboard();
        }

        public static void AbrirEstado(IWin32Window? owner)
        {
            var presentacion = ObtenerPresentacion();
            if (presentacion == null)
            {
                MessageBox.Show(ResolverOwnerForm(owner),
                    "No se puede abrir Estado. Ábrelo desde el menú principal.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var frm = new FrmEstadoClientes(presentacion);
            MostrarDialogo(frm, owner);
            presentacion.CargarDashboard();
        }

        public static void AbrirDeudas(IWin32Window? owner)
        {
            using var frm = new FrmModuloDeudas();
            MostrarDialogo(frm, owner);
            ObtenerPresentacion()?.CargarDashboard();
        }

        public static void AbrirHistorial(IWin32Window? owner)
        {
            var presentacion = ObtenerPresentacion();
            using Form frm = presentacion != null
                ? new FrmHistorialVentas(presentacion)
                : new FrmHistorialVentas();
            MostrarDialogo(frm, owner);
            presentacion?.CargarDashboard();
        }

        public static void AbrirInventario(IWin32Window? owner)
        {
            using var frm = new FrmProductos { StartPosition = FormStartPosition.CenterScreen };
            MostrarDialogo(frm, owner);
        }

        public static void AbrirReportes(IWin32Window? owner)
        {
            using var frm = new FrmCRMFinanciero { StartPosition = FormStartPosition.CenterScreen };
            MostrarDialogo(frm, owner);
            ObtenerPresentacion()?.CargarDashboard();
        }

        public static void AbrirClientes(IWin32Window? owner)
        {
            var presentacion = ObtenerPresentacion();
            if (presentacion == null)
            {
                MessageBox.Show(ResolverOwnerForm(owner),
                    "No se puede abrir Clientes. Ábrelo desde el menú principal.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var frm = new FrmClientes(presentacion);
            MostrarDialogo(frm, owner);
            presentacion.CargarDashboard();
        }

        private static Form? ResolverOwnerForm(IWin32Window? owner)
        {
            if (owner is Form f && !f.IsDisposed)
                return f;
            return Form.ActiveForm is { IsDisposed: false } active ? active : null;
        }

        private static DialogResult MostrarDialogo(Form frm, IWin32Window? owner)
        {
            Form? host = ResolverOwnerForm(owner);
            return host != null ? frm.ShowDialog(host) : frm.ShowDialog();
        }

        public static void OcultarBotonActual(Panel panelNav, string moduloActual)
        {
            if (panelNav == null) return;
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            foreach (Control c in panelNav.Controls)
            {
                if (c is not Button btn) continue;

                if (EsBotonBack(btn))
                {
                    btn.Visible = true;
                    btn.Enabled = true;
                    continue;
                }

                // Solo ocultar el botón legacy btnNavCobrar o el del módulo actual
                if (string.Equals(btn.Name, "btnNavCobrar", StringComparison.OrdinalIgnoreCase))
                {
                    btn.Visible = false;
                    continue;
                }

                string? moduloBtn = NombreModuloDeBoton(btn);
                if (EsBotonDelModuloActual(moduloActual, moduloBtn))
                    btn.Visible = false;
                else if (btn.Name.StartsWith("btnNav", StringComparison.OrdinalIgnoreCase))
                    btn.Visible = true;
            }

            OrdenarBotones(panelNav);
        }

        public static void OrdenarBotones(Panel panelNav)
        {
            if (panelNav == null || panelNav.IsDisposed)
                return;

            panelNav.SuspendLayout();
            try
            {
                panelNav.Enabled = true;

                Button? back = null;
                foreach (Control c in panelNav.Controls)
                {
                    if (c is Button b && EsBotonBack(b))
                    {
                        back = b;
                        break;
                    }
                }

                int x = StartX;
                if (back != null)
                {
                    PrepararBotonBarra(back);
                    AplicarIconoBack(back);
                    back.Visible = true;
                    back.Enabled = true;
                    back.Size = new Size(43, 35);
                    back.Location = new Point(x, 8);
                    back.BringToFront();
                    x += back.Width + Gap;
                }

                foreach (string name in OrdenBotones)
                {
                    Button? btn = BuscarBoton(panelNav, name);
                    if (btn == null)
                        continue;

                    PrepararBotonBarra(btn);
                    btn.Enabled = true;

                    if (!btn.Visible)
                    {
                        btn.Location = new Point(-4000, BtnY);
                        continue;
                    }

                    if (btn.Height != BtnH)
                        btn.Height = BtnH;

                    btn.Location = new Point(x, BtnY);
                    btn.BringToFront();
                    if (string.Equals(name, "btnNavPagar", StringComparison.OrdinalIgnoreCase))
                        CobrarButtonStyle.Apply(btn);
                    x += Math.Max(btn.Width, 90) + Gap;
                }
            }
            finally
            {
                panelNav.ResumeLayout(true);
            }
        }

        private static void WireAllNavClicks(Panel panelNav, Form host)
        {
            WireClick(panelNav, host, "btnNavPagar", () => AbrirCobrar(host));
            WireClick(panelNav, host, "btnNavDeudas", () => AbrirDeudas(host));
            WireClick(panelNav, host, "btnNavEstado", () => AbrirEstado(host));
            WireClick(panelNav, host, "btnNavCaja", () => AbrirCaja(host));
            WireClick(panelNav, host, "btnNavHistorial", () => AbrirHistorial(host));
            WireClick(panelNav, host, "btnNavInventario", () => AbrirInventario(host));
            WireClick(panelNav, host, "btnNavReportes", () => AbrirReportes(host));
            WireClick(panelNav, host, "btnNavClientes", () => AbrirClientes(host));
        }

        private static void AsegurarRelayout(Panel panelNav, Form host, string moduloActual)
        {
            if (Equals(panelNav.Tag, TagRelayout))
                return;

            panelNav.Tag = TagRelayout;

            void Relayout(object? _, EventArgs __)
            {
                if (panelNav.IsDisposed || host.IsDisposed)
                    return;
                OcultarBotonActual(panelNav, moduloActual);
                OrdenarBotones(panelNav);
                WireBack(panelNav, host);
                WireAllNavClicks(panelNav, host);
            }

            host.Load += Relayout;
            host.Shown += Relayout;
            host.ResizeEnd += Relayout;
        }

        private static void PrepararBotonBarra(Button btn)
        {
            btn.Dock = DockStyle.None;
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btn.Margin = Padding.Empty;
            btn.AutoSize = false;
            btn.Cursor = Cursors.Hand;
            btn.Enabled = true;
            btn.Region = null; // región redondeada puede romper el hit-test
            btn.TabStop = true;
        }

        private static Image? _backIcon;

        private static Image? ObtenerIconoBack()
        {
            if (_backIcon != null)
                return _backIcon;

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "nav_back.png");
                if (!File.Exists(path))
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nav_back.png");

                if (File.Exists(path))
                {
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var img = Image.FromStream(fs);
                    _backIcon = new Bitmap(img);
                }
            }
            catch
            {
                // texto de respaldo
            }

            return _backIcon;
        }

        private static void AplicarIconoBack(Button back)
        {
            PrepararBotonBarra(back);
            back.FlatStyle = FlatStyle.Flat;
            back.FlatAppearance.BorderSize = 0;
            back.UseVisualStyleBackColor = true;
            back.TextAlign = ContentAlignment.MiddleCenter;
            back.ImageAlign = ContentAlignment.MiddleCenter;

            var icono = ObtenerIconoBack();
            if (icono != null)
            {
                back.Text = string.Empty;
                back.Image = icono;
            }
            else if (string.IsNullOrWhiteSpace(back.Text) || back.Text == "?" || back.Text.StartsWith('?'))
            {
                back.Image = null;
                back.Text = "<";
                back.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            }
        }

        private static void WireBack(Panel panelNav, Form host)
        {
            Button? back = BuscarBoton(panelNav, "btnBack")
                ?? BuscarBoton(panelNav, "btnNavBack");

            if (back == null)
            {
                foreach (Control c in panelNav.Controls)
                {
                    if (c is Button b && EsBotonBack(b))
                    {
                        back = b;
                        break;
                    }
                }
            }

            if (back == null)
                return;

            AplicarIconoBack(back);
            back.Visible = true;
            back.Enabled = true;

            if (WiredClicks.TryGetValue(back, out _))
                return;

            WiredClicks.Add(back, WiredMarker);
            Form destino = host;
            back.Click += (_, _) =>
            {
                if (destino.IsDisposed)
                    return;
                try
                {
                    destino.DialogResult = DialogResult.Cancel;
                    destino.Close();
                }
                catch
                {
                    destino.Close();
                }
            };
        }

        private static void WireClick(Panel panelNav, Form host, string name, Action action)
        {
            Button? btn = BuscarBoton(panelNav, name);
            if (btn == null)
                return;

            // Cablear aunque esté oculto (módulo actual); al mostrarse en otra pantalla ya tiene handler
            PrepararBotonBarra(btn);

            if (WiredClicks.TryGetValue(btn, out _))
                return;

            WiredClicks.Add(btn, WiredMarker);
            string destino = string.IsNullOrWhiteSpace(btn.Text) ? name : btn.Text;
            btn.Click += (_, _) =>
            {
                if (!btn.Enabled)
                    return;
                try { action(); }
                catch (Exception ex)
                {
                    MessageBox.Show(host, $"Error al abrir {destino.ToLowerInvariant()}: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private static bool EsBotonBack(Button btn) =>
            btn.Name is "btnBack" or "btnNavBack";

        private static Button? BuscarBoton(Panel panelNav, string name)
        {
            foreach (Control c in panelNav.Controls)
            {
                if (c is Button b && string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase))
                    return b;
            }
            return null;
        }

        public static void AbrirCobrar(IWin32Window? owner)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FrmPagos pagos && !pagos.IsDisposed)
                {
                    pagos.Focus();
                    pagos.ActivarTabProductosPublico();
                    return;
                }
            }

            var presentacion = ObtenerPresentacion();
            Form? host = ResolverOwnerForm(owner);
            using Form frm = presentacion != null
                ? new FrmPagos(presentacion)
                : host != null
                    ? new FrmPagos(host)
                    : new FrmPagos();
            MostrarDialogo(frm, owner);
            presentacion?.CargarDashboard();
        }

        private static bool EsBotonDelModuloActual(string? moduloActual, string? moduloBoton)
        {
            if (string.IsNullOrEmpty(moduloActual) || string.IsNullOrEmpty(moduloBoton))
                return false;

            // PAGAR y COBRAR son el mismo módulo POS
            if (string.Equals(moduloActual, ModuloPagar, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(moduloBoton, ModuloPagar, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(moduloBoton, "COBRAR", StringComparison.OrdinalIgnoreCase)))
                return true;

            return string.Equals(moduloActual, moduloBoton, StringComparison.OrdinalIgnoreCase);
        }

        private static string? NombreModuloDeBoton(Button btn)
        {
            return btn.Name.ToLowerInvariant() switch
            {
                "btnnavpagar" => ModuloPagar,
                "btnnavdeudas" => ModuloDeudas,
                "btnnavestado" => ModuloEstado,
                "btnnavcaja" => ModuloCaja,
                "btnnavhistorial" => ModuloHistorial,
                "btnnavinventario" => ModuloInventario,
                "btnnavreportes" => ModuloReportes,
                "btnnavclientes" => ModuloClientes,
                "btnnavcobrar" => ModuloPagar,
                "btnback" or "btnnavback" => null,
                _ => MapTextToModulo(btn.Text)
            };
        }

        private static string? MapTextToModulo(string? text)
        {
            if (string.IsNullOrWhiteSpace(text) || text == "◀" || text == "<")
                return null;

            string t = text.Trim().ToUpperInvariant();
            if (t is "COBRAR" or "PAGAR" or "IR A COBRAR" or "IR A PAGAR")
                return ModuloPagar;
            return t;
        }

        private static FrmPresentacion? ObtenerPresentacion()
        {
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is FrmPresentacion abierta)
                    return abierta;
            }

            return null;
        }
    }
}
