using BLL;
using CORE;
using System;
using System.Drawing;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmModuloDeudas : Form
    {
        // Instancias de los formularios hijos (se crearán una sola vez)
        private FrmDeudaDashboard? dashboardForm;
        private FrmDeudas? gestionForm;
        private FrmCrearDeuda? crearForm;
        private FrmHistorialDeudas? historialForm;
        private readonly int? _clienteIdParaGestion;
        private bool _navegacionInicialPendiente;

        public FrmModuloDeudas() : this(null)
        {
        }

        public FrmModuloDeudas(int? clienteIdParaGestion)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _clienteIdParaGestion = clienteIdParaGestion;
            _navegacionInicialPendiente = clienteIdParaGestion.HasValue;

            // En el diseñador solo se muestran los tabs; la barra se cablea al ejecutar.
            if (ThemeHost.IsDesignTime())
                return;

            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloDeudas);
        }

        private void FrmModuloDeudas_Load(object sender, EventArgs e)
        {
            bool tieneAcceso = PuedeAccederModuloDeudas();

            if (!tieneAcceso)
            {
                MessageBox.Show("No tienes acceso al módulo de deudas", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            ConfigurarTabControl();
            AplicarPermisosTabs();

            // Si solo puede ver historial (ej. rol CONSULTA), no abrir Gestión.
            if (_navegacionInicialPendiente && _clienteIdParaGestion.HasValue && PuedeGestionarDeudas())
                tabControl.SelectedTab = tabGestion;
            else
                CargarTabInicial();
        }

        private static bool EsAdmin() =>
            string.Equals(Sesion.Rol?.Trim(), "ADMIN", StringComparison.OrdinalIgnoreCase);

        private static bool PuedeVerDashboardDeudas() =>
            EsAdmin()
            || Sesion.TienePermiso("VER_DEUDAS")
            // CONSULTA: puede ver dashboard (solo lectura) y descargar reporte.
            || Sesion.TienePermiso("VER_HISTORIAL_DEUDAS")
            || Sesion.TienePermiso("1003");

        private static bool PuedeGestionarDeudas() =>
            EsAdmin()
            || Sesion.TienePermiso("VER_DEUDAS")
            || Sesion.TienePermiso("PAGAR_DEUDA");

        private static bool PuedeCrearDeuda() =>
            EsAdmin()
            || Sesion.TienePermiso("CREAR_DEUDA")
            || Sesion.TienePermiso("VER_DEUDAS");

        private static bool PuedeVerHistorialDeudas() =>
            EsAdmin()
            || Sesion.TienePermiso("VER_HISTORIAL_DEUDAS")
            || Sesion.TienePermiso("VER_DEUDAS")
            || Sesion.TienePermiso("1003");

        private static bool PuedeAccederModuloDeudas() =>
            PuedeVerDashboardDeudas()
            || PuedeGestionarDeudas()
            || PuedeCrearDeuda()
            || PuedeVerHistorialDeudas();

        /// <summary>
        /// Deja visibles solo las pestañas permitidas por rol/permiso.
        /// CONSULTA ve Dashboard (reporte) + Historial; sin Gestión/Crear.
        /// </summary>
        private void AplicarPermisosTabs()
        {
            tabControl.SelectedIndexChanged -= tabControl_SelectedIndexChanged;

            tabControl.TabPages.Clear();

            if (PuedeVerDashboardDeudas())
                tabControl.TabPages.Add(tabDashboard);

            if (PuedeGestionarDeudas())
                tabControl.TabPages.Add(tabGestion);

            if (PuedeCrearDeuda())
                tabControl.TabPages.Add(tabCrear);

            if (PuedeVerHistorialDeudas())
                tabControl.TabPages.Add(tabHistorial);

            if (tabControl.TabPages.Count == 0)
            {
                MessageBox.Show("No tienes pestañas disponibles en el módulo de deudas.",
                    "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            // Una sola pestaña: ocultar la franja de tabs para que se vea solo el historial.
            if (tabControl.TabPages.Count == 1)
            {
                tabControl.Appearance = TabAppearance.FlatButtons;
                tabControl.ItemSize = new Size(0, 1);
                tabControl.SizeMode = TabSizeMode.Fixed;
            }

            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
        }

        // ===============================
        // CONFIGURAR APARIENCIA PROFESIONAL
        // ===============================
        private void ConfigurarTabControl()
        {
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            tabControl.ItemSize = new Size(250, 40);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += TabControl_DrawItem;
        }

        // ===============================
        // DIBUJO PERSONALIZADO DE TABS
        // ===============================
        private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= tabControl.TabPages.Count)
                return;

            Graphics g = e.Graphics;
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabBounds = tabControl.GetTabRect(e.Index);

            // Color de fondo según estado
            Color backColor = (e.State == DrawItemState.Selected)
                ? AppTheme.Primary
                : AppTheme.SurfaceElevated;

            Color textColor = (e.State == DrawItemState.Selected)
                ? AppTheme.TextOnPrimary
                : AppTheme.TextPrimary;

            // Dibujar fondo
            using (SolidBrush brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, tabBounds);
            }

            // Dibujar texto centrado
            TextRenderer.DrawText(g, tabPage.Text, tabControl.Font,
                tabBounds, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // ===============================
        // CARGAR TAB INICIAL
        // ===============================
        private void CargarTabInicial()
        {
            if (tabControl.TabPages.Contains(tabDashboard) && PuedeVerDashboardDeudas())
            {
                tabControl.SelectedTab = tabDashboard;
                CargarDashboard();
                return;
            }

            if (tabControl.TabPages.Contains(tabHistorial) && PuedeVerHistorialDeudas())
            {
                tabControl.SelectedTab = tabHistorial;
                CargarHistorial();
                return;
            }

            if (tabControl.TabPages.Contains(tabGestion) && PuedeGestionarDeudas())
            {
                tabControl.SelectedTab = tabGestion;
                CargarGestion();
                return;
            }

            if (tabControl.TabPages.Count > 0)
            {
                tabControl.SelectedIndex = 0;
                tabControl_SelectedIndexChanged(tabControl, EventArgs.Empty);
            }
        }

        // ===============================
        // EVENTO: CAMBIO DE TAB
        // ===============================
        private void tabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabControl.SelectedTab == null)
                return;

            if (tabControl.SelectedTab == tabDashboard)
                CargarDashboard();
            else if (tabControl.SelectedTab == tabGestion)
                CargarGestion();
            else if (tabControl.SelectedTab == tabCrear)
                CargarCrear();
            else if (tabControl.SelectedTab == tabHistorial)
                CargarHistorial();
        }

        // ===============================
        // CARGAR DASHBOARD
        // ===============================
        private void CargarDashboard()
        {
            if (dashboardForm == null || dashboardForm.IsDisposed)
            {
                dashboardForm = new FrmDeudaDashboard
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                tabDashboard.Controls.Add(dashboardForm);
                dashboardForm.Show();
            }
            else
            {
                // Refrescar datos cuando se vuelva a seleccionar el tab
                dashboardForm.ActualizarDatos();
                dashboardForm.BringToFront();
            }
        }

        // ===============================
        // CARGAR GESTIÓN DE DEUDAS
        // ===============================
        private void CargarGestion()
        {
            int? clienteId = _navegacionInicialPendiente ? _clienteIdParaGestion : null;

            if (gestionForm == null || gestionForm.IsDisposed)
            {
                gestionForm = new FrmDeudas(clienteId)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                tabGestion.Controls.Add(gestionForm);
                gestionForm.Show();

                if (_navegacionInicialPendiente)
                    _navegacionInicialPendiente = false;
            }
            else
            {
                if (_clienteIdParaGestion.HasValue)
                    gestionForm.SeleccionarCliente(_clienteIdParaGestion.Value);
                else
                    gestionForm.ActualizarDatos();

                gestionForm.BringToFront();
            }
        }

        // ===============================
        // CARGAR NUEVA DEUDA
        // ===============================
        private void CargarCrear()
        {
            if (crearForm == null || crearForm.IsDisposed)
            {
                crearForm = new FrmCrearDeuda
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                tabCrear.Controls.Add(crearForm);
                crearForm.Show();
            }
            else
            {
                crearForm.BringToFront();
            }
        }

        // ===============================
        // CARGAR HISTORIAL
        // ===============================
        private void CargarHistorial()
        {
            if (historialForm == null || historialForm.IsDisposed)
            {
                historialForm = new FrmHistorialDeudas
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                tabHistorial.Controls.Add(historialForm);
                historialForm.Show();
            }
            else
            {
                historialForm.ActualizarDatos();
                historialForm.BringToFront();
            }
        }

        // ===============================
        // LIMPIAR AL CERRAR — desuscribir AppEventos de formularios hijos
        // ===============================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CerrarFormularioHijo(ref dashboardForm);
            CerrarFormularioHijo(ref gestionForm);
            CerrarFormularioHijo(ref crearForm);
            CerrarFormularioHijo(ref historialForm);

            base.OnFormClosing(e);
        }

        private static void CerrarFormularioHijo<T>(ref T? form) where T : Form
        {
            if (form == null)
                return;

            try
            {
                if (!form.IsDisposed)
                {
                    form.Close(); // dispara OnFormClosed → se desuscribe de AppEventos
                    form.Dispose();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                form = null;
            }
        }
    }
}
