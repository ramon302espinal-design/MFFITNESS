using BLL;
using System;
using System.Drawing;
using System.Windows.Forms;
using CORE;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmPresentacion : Form
    {
        private System.Windows.Forms.Timer? _timerWhatsApp;
        private System.Windows.Forms.Timer? _timerDashboard;
        private volatile bool _whatsappAutomatizacionEnCurso;
        private readonly DeudaBLL deudaNotificacionBLL = new DeudaBLL();
        private bool _puedeVerIngresos = true;
        private bool _dashboardStyled;
        private Label? _lblDashWelcome;
        private Label? _lblDashDateTime;

        public FrmPresentacion()
        {
            InitializeComponent();
        }

        private void FrmPresentacion_Load(object sender, EventArgs e)
        {
            // 🛡️ PASO 0: SEGURIDAD (El "Filtro" de entrada)
            if (string.IsNullOrEmpty(Sesion.Usuario) || Sesion.Usuario.ToUpper() == "ANONIMO")
            {
                MessageBox.Show("Sesión no válida. Por favor, inicie sesión.", "Seguridad",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close();
                return;
            }

            try
            {
                // 1️⃣ UX: Cambiamos el cursor a "Cargando" para indicar que el Core está trabajando
                this.Cursor = Cursors.WaitCursor;

                // 2️⃣ SEGURIDAD INTERNA: Aplicamos qué botones puede ver según su Rol
                AplicarPermisos();
                AplicarEstiloSidebarModern();
                AplicarEstiloDashboard();

                // 3️⃣ VÍNCULO CORE: Cargamos ingresos hoy, mes y conteo de clientes
                CargarDashboard();

                // 4️⃣ AUTOMATIZACIÓN: Revisamos quién venció hoy y lanzamos el Popup de alertas
                MostrarAlertas();

                // 5️⃣ WhatsApp: recordatorios de deuda y membresías
                IniciarAutomatizacionWhatsApp();

                string? advertenciaWhatsApp = TwilioSettings.ObtenerAdvertenciaConfiguracion();
                if (!string.IsNullOrWhiteSpace(advertenciaWhatsApp))
                {
                    MessageBox.Show(
                        advertenciaWhatsApp,
                        "WhatsApp - Configuracion requerida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar el panel de control: " + ex.Message,
                                "Inicio de Sistema", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                // 6️⃣ UX: Restauramos el cursor normal siempre (aunque haya error)
                this.Cursor = Cursors.Default;
            }
        }

        private void IniciarAutomatizacionWhatsApp()
        {
            if (!TwilioSettings.WhatsAppHabilitado)
                return;

            if (!TwilioSettings.TimerUIHabilitado)
            {
                System.Diagnostics.Debug.WriteLine(
                    "WhatsApp: timer UI desactivado (TwilioTimerUIHabilitado=false). Use WhatsAppHost 24/7.");
                return;
            }

            EjecutarAutomatizacionWhatsAppEnBackground("startup");

            _timerWhatsApp = new System.Windows.Forms.Timer
            {
                Interval = TwilioSettings.IntervaloAutomatizacionMinutos * 60 * 1000
            };
            _timerWhatsApp.Tick += TimerWhatsApp_Tick;
            _timerWhatsApp.Start();
        }

        private void TimerWhatsApp_Tick(object? sender, EventArgs e)
        {
            EjecutarAutomatizacionWhatsAppEnBackground("timer");
        }

        private void EjecutarAutomatizacionWhatsAppEnBackground(string origen)
        {
            if (_whatsappAutomatizacionEnCurso)
                return;

            _whatsappAutomatizacionEnCurso = true;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    int enviados = deudaNotificacionBLL.VerificarYEnviarNotificaciones();
                    if (enviados > 0)
                        System.Diagnostics.Debug.WriteLine($"WhatsApp {origen}: {enviados} mensaje(s) enviado(s).");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WhatsApp {origen}: {ex.Message}");
                }
                finally
                {
                    _whatsappAutomatizacionEnCurso = false;
                }
            });
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_timerWhatsApp != null)
            {
                _timerWhatsApp.Stop();
                _timerWhatsApp.Dispose();
                _timerWhatsApp = null;
            }

            if (_timerDashboard != null)
            {
                _timerDashboard.Stop();
                _timerDashboard.Dispose();
                _timerDashboard = null;
            }

            base.OnFormClosed(e);
        }

        private void AplicarEstiloSidebarModern()
        {
            PresentacionSidebarStyle.Apply(
                panel1,
                btnCerrarSesion,
                (btnPagar, NavIcons.Pos, "COBRAR"),
                (btnAgregarCliente, NavIcons.Client, "CLIENTES"),
                (btnCaja, NavIcons.Cash, "CAJA"),
                (btnProductoStock, NavIcons.Inventory, "INVENTARIO"),
                (btnEstado, NavIcons.Status, "ESTADO"),
                (btnDeudas, NavIcons.Debts, "DEUDAS"),
                (btnHistorial, NavIcons.History, "HISTORIAL DE PAGOS"),
                (btnReportes, NavIcons.Reports, "REPORTES"));

            if (btnPagar.Visible)
                CobrarButtonStyle.Apply(btnPagar);
        }

        private void AplicarEstiloDashboard()
        {
            if (_dashboardStyled)
            {
                PresentacionDashboardStyle.ActualizarEncabezado(_lblDashWelcome, _lblDashDateTime);
                return;
            }

            _lblDashWelcome = PresentacionDashboardStyle.Apply(
                panelDashboard,
                panelClientesActivos, label1, lblClientesActivos,
                panelClientesVencidos, label2, lblClientesVencidos,
                panelIngresosHoy, label3, lblIngresosHoy,
                panelIngresosMes, label4, lblIngresosMes);

            _lblDashDateTime = panelDashboard.Controls["lblDashDateTime"] as Label;
            PresentacionDashboardStyle.ActualizarEncabezado(_lblDashWelcome, _lblDashDateTime);
            IniciarRelojDashboard();
            _dashboardStyled = true;
        }

        private void IniciarRelojDashboard()
        {
            _timerDashboard?.Stop();
            _timerDashboard?.Dispose();
            _timerDashboard = new System.Windows.Forms.Timer { Interval = 1000 };
            _timerDashboard.Tick += (_, _) =>
                PresentacionDashboardStyle.ActualizarEncabezado(_lblDashWelcome, _lblDashDateTime);
            _timerDashboard.Start();
        }

        // ===============================
        // PERMISOS POR ROL
        // ===============================
        private void AplicarPermisos()
        {
            string? rol = Sesion.Rol?.Trim().ToUpper();

            // Inicialmente ocultar todos los botones principales
            btnAgregarCliente.Visible = false;
            btnPagar.Visible = false;
            btnCaja.Visible = false;
            btnHistorial.Visible = false;
            btnEstado.Visible = false;
            btnProductoStock.Visible = false;

            // Aplicar visibilidad según rol
            switch (rol)
            {
                case "ADMIN":
                    MostrarTodos();
                    break;
                case "CAJERO":
                    btnPagar.Visible = true;
                    btnCaja.Visible = true;
                    btnHistorial.Visible = true;
                    if (EsUsuarioCajaOperativa())
                    {
                        btnAgregarCliente.Visible = true;
                        btnProductoStock.Visible = true;
                        btnEstado.Visible = true;
                    }
                    break;
                case "CONSULTA":
                    btnEstado.Visible = true;
                    btnHistorial.Visible = true;
                    // Deudas: Dashboard (reporte) + Historial; sin crear/cobrar.
                    break;
                default:
                    MessageBox.Show("Rol no válido");
                    break;
            }

            // 🔥 PERMISOS ADICIONALES GRANULARES
            // Control de visibilidad de Deudas según permisos específicos
            if (Sesion.TienePermiso("VER_DEUDAS") ||
                Sesion.TienePermiso("1003") ||
                Sesion.TienePermiso("VER_HISTORIAL_DEUDAS") ||
                rol == "ADMIN" ||
                EsUsuarioCajaOperativa())
            {
                btnDeudas.Visible = true;
            }
            else
            {
                btnDeudas.Visible = false;
            }

            // Los 4 paneles KPI: Caja1/2/3 no ven Ingresos Hoy ni Ingresos Mensual.
            _puedeVerIngresos = Sesion.TienePermiso("VER_INGRESOS") || rol == "ADMIN";
            bool ocultarKpisCaja = EsUsuarioCajaOperativa();
            panelIngresosHoy.Visible = !ocultarKpisCaja;
            panelIngresosMes.Visible = !ocultarKpisCaja;
            panelClientesVencidos.Visible = true;
        }

        /// <summary>
        /// Usuarios operativos de caja (caja1, caja2, caja3): misma vista KPI y módulos.
        /// </summary>
        private static bool EsUsuarioCajaOperativa()
        {
            string usuario = (Sesion.Usuario ?? string.Empty)
                .Trim()
                .Replace(" ", "", StringComparison.Ordinal)
                .ToUpperInvariant();

            return usuario is "CAJA1" or "CAJA2" or "CAJ2" or "CAJA3";
        }

        private void MostrarTodos()
        {
            btnAgregarCliente.Visible = true;
            btnPagar.Visible = true;
            btnCaja.Visible = true;
            btnHistorial.Visible = true;
            btnEstado.Visible = true;
            btnProductoStock.Visible = true;
            btnDeudas.Visible = true; // ADMIN siempre ve el módulo de deudas
        }

        // ===============================
        // BOTONES
        // ===============================
        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (!ConfirmarSalidaConRevisionCaja())
                return;

            Sesion.CerrarSesion();
            this.Hide();
            Application.OpenForms[0]?.Show();
        }

        /// <summary>
        /// Si el usuario tiene caja abierta, ofrece cuadre antes de salir.
        /// Sí = cuadre y salir; No = salir dejando su caja abierta; Cancelar = quedarse.
        /// </summary>
        private bool ConfirmarSalidaConRevisionCaja()
        {
            var cajaBll = new CajaBLL();
            if (!cajaBll.ObtenerEstadoCaja())
                return true;

            DialogResult decision = MessageBox.Show(
                "Aún no has cerrado caja.\n¿Deseas hacer el cuadre para salir?",
                "Caja abierta",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (decision == DialogResult.Cancel)
                return false;

            if (decision == DialogResult.No)
                return true;

            return EjecutarCuadreCajaParaSalir(cajaBll);
        }

        private bool EjecutarCuadreCajaParaSalir(CajaBLL cajaBll)
        {
            try
            {
                var cajaService = new CajaServiceBLL();
                decimal ingresos = cajaBll.IngresosHoy();
                decimal egresos = cajaBll.EgresosHoy();
                decimal sistema = ingresos - egresos;

                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"SISTEMA: {sistema:C}\n\nIngrese dinero contado:",
                    "Cuadre de Caja",
                    "0");

                if (string.IsNullOrWhiteSpace(input))
                    return false;

                if (!decimal.TryParse(input, out decimal contado) || contado < 0)
                {
                    MessageBox.Show("Monto inválido.", "Cuadre de Caja",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                decimal diferencia = contado - sistema;
                string estado = diferencia == 0 ? "CUADRE PERFECTO" : "DIFERENCIA";

                DialogResult confirm = MessageBox.Show(
                    $"SISTEMA: {sistema:C}\n" +
                    $"CONTADO: {contado:C}\n" +
                    $"DIFERENCIA: {diferencia:C}\n\n" +
                    $"{estado}\n\n¿Confirmar cierre y salir?",
                    "Confirmar Cierre",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return false;

                var resultado = cajaService.CerrarCajaConCuadre(contado);
                MessageBox.Show(
                    $"Caja cerrada correctamente.\n\nSistema: {resultado.sistema:C}\nDiferencia: {resultado.diferencia:C}",
                    "Caja",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                AppEventos.CajaCambiada();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cerrar la caja: " + ex.Message,
                    "Caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnAgregarCliente_Click(object sender, EventArgs e)
        {
            // 1️⃣ Cambiamos a ShowDialog para que el código se detenga hasta cerrar la ventana
            FrmClientes frm = new FrmClientes(this);
            frm.ShowDialog();

            // 🔥 2️⃣ ACTUALIZACIÓN CORE: Refrescamos los indicadores (clientes activos, etc.)
            // Esto asegura que si agregaste un cliente, el label de "Activos" suba de inmediato
            CargarDashboard();
        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            // 1️⃣ Mantener tu lógica de evitar múltiples instancias
            foreach (Form form in Application.OpenForms)
            {
                if (form is FrmPagos)
                {
                    form.Focus();
                    return;
                }
            }

            // 2️⃣ Abrir como cuadro de diálogo para pausar la ejecución aquí
            FrmPagos pago = new FrmPagos(this);
            pago.ShowDialog();

            // 🔥 3️⃣ ACTUALIZACIÓN CORE: Al cerrar la ventana, refrescamos los labels
            CargarDashboard();
        }

        private void btnEstado_Click(object sender, EventArgs e)
        {
            try
            {
                // 1️⃣ Usamos 'using' para asegurar que la memoria del formulario se libere al cerrar
                using (FrmEstadoClientes estado = new FrmEstadoClientes(this))
                {
                    // 2️⃣ Abrimos como Dialog (detiene la ejecución aquí hasta cerrar)
                    estado.ShowDialog();
                }

                // 🔥 3️⃣ ACTUALIZACIÓN CORE: 
                // Refrescamos indicadores porque en esta ventana se suelen 
                // activar/desactivar miembros manualmente.
                CargarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar los estados después de la revisión: " + ex.Message,
                                "Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            FrmHistorialVentas historial = new FrmHistorialVentas();
            historial.ShowDialog();
        }

        private void btnProductoStock_Click(object sender, EventArgs e)
        {
            FrmProductos productos = new FrmProductos
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            productos.ShowDialog();
        }

        private void btnCaja_Click(object sender, EventArgs e)
        {
            try
            {
                // 1️⃣ Usamos 'using' para liberar memoria inmediatamente al cerrar la ventana
                using (FrmCajaDashboard caja = new FrmCajaDashboard())
                {
                    // 2️⃣ Abrimos como Dialog para pausar la ejecución en el Dashboard
                    caja.ShowDialog();
                }

                // 🔥 3️⃣ VÍNCULO CORE: Refresco inmediato del Dashboard
                // Si el usuario abrió caja, cerró caja o registró un retiro manual,
                // los números de 'Ingresos Hoy' deben cambiar al instante.
                CargarDashboard();
            }
            catch (Exception ex)
            {
                // 4️⃣ Alerta profesional con icono de error
                MessageBox.Show("Error en el módulo de Caja: " + ex.Message,
                                "Gestión Financiera", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ===============================
        // DASHBOARD Y ALERTAS
        // ===============================
        public void CargarDashboard()
        {
            try
            {
                // Sincroniza Activa=0 tras el día de vencimiento antes de contar.
                new MembresiaBLL().ActualizarVencimientos();

                // Usamos el nuevo BLL que centraliza todo el Core
                DashboardBLL dashboardBLL = new DashboardBLL();

                // 1. Datos de Clientes (misma regla que Estado: Activo / Vencido)
                lblClientesActivos.Text = dashboardBLL.ClientesActivos().ToString();
                lblClientesVencidos.Text = dashboardBLL.ClientesVencidos().ToString();

                // 2. Datos Financieros Reales (Vienen de MovimientosCaja: Ventas + Membresías)
                decimal hoy = dashboardBLL.IngresosHoy();
                decimal mes = dashboardBLL.IngresosMes();

                if (_puedeVerIngresos)
                {
                    lblIngresosHoy.Text = hoy.ToString("C");
                    lblIngresosMes.Text = mes.ToString("C");
                    lblIngresosHoy.ForeColor = hoy > 0 ? AppTheme.Primary : AppTheme.Error;
                    lblIngresosMes.ForeColor = mes > 0 ? Color.FromArgb(168, 85, 247) : AppTheme.TextMuted;
                }
                else
                {
                    lblIngresosHoy.Text = "—";
                    lblIngresosMes.Text = "—";
                    lblIngresosHoy.ForeColor = AppTheme.TextMuted;
                    lblIngresosMes.ForeColor = AppTheme.TextMuted;
                }

                lblClientesActivos.ForeColor = AppTheme.Success;
                lblClientesVencidos.ForeColor = AppTheme.Error;
                PresentacionDashboardStyle.ActualizarEncabezado(_lblDashWelcome, _lblDashDateTime);
            }
            catch (Exception ex)
            {
                AppDialog.Warning(this, "Error al actualizar indicadores: " + ex.Message, "Dashboard");
            }
        }
        private void MostrarAlertas()
        {
            try
            {
                MembresiaBLL bll = new MembresiaBLL();

                bll.ActualizarVencimientos();

                int vencenHoy = bll.ObtenerVencenHoy();
                int vencidos = bll.ObtenerVencidos();

                string mensaje = "";

                if (vencenHoy > 0)
                    mensaje += $"{vencenHoy} clientes vencen hoy\n";

                if (vencidos > 0)
                    mensaje += $"{vencidos} clientes vencidos\n";

                if (!string.IsNullOrEmpty(mensaje))
                    AppDialog.Warning(this, mensaje.TrimEnd(), "Alertas del sistema");
            }
            catch (Exception ex)
            {
                AppDialog.Error(this, "Error al cargar alertas: " + ex.Message);
            }
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            try
            {
                FrmReportes frm = new FrmReportes();
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir módulo de reportes: " + ex.Message);
            }
        }

        private void btnDeudas_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ ABRIR MÓDULO UNIFICADO DE DEUDAS
                // (Permisos ya validados en AplicarPermisos y en FrmModuloDeudas_Load)
                // using: al cerrar se Dispose y se liberan los hijos embebidos
                // (evita instancias zombie suscritas a AppEventos).
                using (FrmModuloDeudas frm = new FrmModuloDeudas())
                {
                    frm.ShowDialog(this);
                }

                // 🔥 ACTUALIZACIÓN CORE: Refrescar dashboard al cerrar el módulo de deudas
                CargarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir módulo de deudas: " + ex.Message,
                    "Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
