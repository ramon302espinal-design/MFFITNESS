using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CORE;
using CORE.Update;
using UI.Helpers;
using UI.Services;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmPresentacion : Form
    {
        private System.Windows.Forms.Timer? _timerWhatsApp;
        private System.Windows.Forms.Timer? _timerChatBadge;
        private System.Windows.Forms.Timer? _timerDashboard;
        private System.Windows.Forms.Timer? _timerStockAlert;
        private System.Windows.Forms.Timer? _timerStockCriticoDebounce;
        private readonly HashSet<int> _stockCriticoPendiente = new();
        private readonly object _stockCriticoSync = new();
        private readonly StringBuilder _bufferEscannerInicio = new();
        private DateTime _ultimaTeclaEscannerInicio = DateTime.MinValue;
        private readonly PosScannerIntervalGate _intervaloEscannerInicio = new();
        private volatile bool _whatsappAutomatizacionEnCurso;
        private readonly DeudaBLL deudaNotificacionBLL = new DeudaBLL();
        // KPI de deudas: instancia propia; la de notificaciones corre en hilo aparte.
        private readonly DeudaBLL deudaKpiBLL = new DeudaBLL();
        private readonly ChatBLL chatBadgeBLL = new ChatBLL();
        private int _ultimoNoLeidosMenu = -1;
        private bool _puedeVerIngresos = true;
        private bool _puedeVerDeudas = true;
        private bool _puedeVerIngresoPendiente = true;
        private bool _dashboardStyled;
        private Label? _lblDashWelcome;
        private Label? _lblDashDateTime;
        private LinkLabel? _lnkDashUpdate;

        public FrmPresentacion()
        {
            InitializeComponent();
        }

        private void FrmPresentacion_Load(object sender, EventArgs e)
        {
            ShellTheme.TryApplyFormIcon(this);

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

                // 3️⃣b Fase 11.5 / 12 / 13 — auditorías en background (no bloquean UI)
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        new IntegridadFinancieraBLL().RegistrarAuditoriaEnLog();
                        new DevProdParityBLL().RegistrarComparacionEnLog();
                        new AceptacionFinalBLL().RegistrarEnLog();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Auditoria F11-F13] Error: {ex.Message}");
                    }
                });

                // 4️⃣ AUTOMATIZACIÓN: Revisamos quién venció hoy y lanzamos el Popup de alertas
                MostrarAlertas();

                // 4️⃣b Inventario: agotados / stock mínimo al abrir + cada 15 min + tiempo real
                StockInventoryAlert.TryShow(this);
                IniciarAvisosStockPeriodicos();
                AppEventos.OnStockCritico -= OnStockCriticoTiempoReal;
                AppEventos.OnStockCritico += OnStockCriticoTiempoReal;

                // 5️⃣ WhatsApp: recordatorios de deuda y membresías
                IniciarAutomatizacionWhatsApp();
                IniciarBadgeChatWhatsApp();
                ChatNotificationHost.Start();
                System.Threading.Tasks.Task.Run(() => BLL.WhatsAppStackBootstrapper.EnsureReady(tryLaunchNgrok: true));

                // 5️⃣b Facturas → egreso automático (AppHost: no se cancela al salir de Caja)
                try
                {
                    FacturaGastosAppHost.Start(this);
                    AppEventos.OnCajaCambiada -= OnCajaCambiadaFacturaAuto;
                    AppEventos.OnCajaCambiada += OnCajaCambiadaFacturaAuto;
                    try
                    {
                        FacturaGastosAppHost.NotifyCajaState(new CajaBLL().ObtenerEstadoCaja());
                    }
                    catch { /* ignore */ }
                }
                catch (Exception exFactura)
                {
                    System.Diagnostics.Debug.WriteLine("FacturaGastosAppHost: " + exFactura.Message);
                }

                // 6️⃣ Indicadores financieros en vivo (ingresos + deudas + caja).
                AppEventos.OnPagoRegistrado += RefrescarIndicadoresFinancieros;
                AppEventos.OnDeudaModificada += RefrescarIndicadoresFinancieros;
                AppEventos.OnCajaCambiada += RefrescarIndicadoresFinancieros;
                AppEventos.OnProgramacionActivada += OnProgramacionActivada;
                AppEventos.OnEstadoMembresiaCambiada += RefrescarConteosClientes;

                ConfigurarCapturaEscannerInicio();
                ModuloAtajosTeclado.AsegurarFiltroGlobal();

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

        private void IniciarBadgeChatWhatsApp()
        {
            if (!btnChat.Visible)
                return;

            ActualizarBadgeChatWhatsApp();

            _timerChatBadge = new System.Windows.Forms.Timer { Interval = 15000 };
            _timerChatBadge.Tick += (_, _) => ActualizarBadgeChatWhatsApp();
            _timerChatBadge.Start();
        }

        private void ActualizarBadgeChatWhatsApp()
        {
            if (!btnChat.Visible || btnChat.IsDisposed)
                return;

            try
            {
                int noLeidos = chatBadgeBLL.ContarNoLeidosTotal();
                _ultimoNoLeidosMenu = noLeidos;

                btnChat.Text = noLeidos > 0 ? $"WHATSAPP ({noLeidos})" : "WHATSAPP";
            }
            catch
            {
                btnChat.Text = "WHATSAPP";
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
            AppEventos.OnPagoRegistrado -= RefrescarIndicadoresFinancieros;
            AppEventos.OnDeudaModificada -= RefrescarIndicadoresFinancieros;
            AppEventos.OnCajaCambiada -= RefrescarIndicadoresFinancieros;
            AppEventos.OnProgramacionActivada -= OnProgramacionActivada;
            AppEventos.OnEstadoMembresiaCambiada -= RefrescarConteosClientes;
            AppEventos.OnStockCritico -= OnStockCriticoTiempoReal;

            if (_timerWhatsApp != null)
            {
                _timerWhatsApp.Stop();
                _timerWhatsApp.Dispose();
                _timerWhatsApp = null;
            }

            if (_timerChatBadge != null)
            {
                _timerChatBadge.Stop();
                _timerChatBadge.Dispose();
                _timerChatBadge = null;
            }

            ChatNotificationHost.Stop();

            if (_timerDashboard != null)
            {
                _timerDashboard.Stop();
                _timerDashboard.Dispose();
                _timerDashboard = null;
            }

            if (_timerStockAlert != null)
            {
                _timerStockAlert.Stop();
                _timerStockAlert.Dispose();
                _timerStockAlert = null;
            }

            if (_timerStockCriticoDebounce != null)
            {
                _timerStockCriticoDebounce.Stop();
                _timerStockCriticoDebounce.Dispose();
                _timerStockCriticoDebounce = null;
            }

            AppEventos.OnCajaCambiada -= OnCajaCambiadaFacturaAuto;
            try { FacturaGastosAppHost.Stop(); } catch { /* ignore */ }

            base.OnFormClosed(e);
        }

        private void OnProgramacionActivada(ProgramacionActivadaEventArgs info)
        {
            FrmProgramacionActivadaToast.MostrarUnaVez(
                info.ProgramacionId,
                info.PlanNombre,
                info.ClienteNombre);
            RefrescarConteosClientes();
        }

        private void OnCajaCambiadaFacturaAuto()
        {
            try
            {
                FacturaGastosAppHost.NotifyCajaState(new CajaBLL().ObtenerEstadoCaja());
            }
            catch
            {
                // ignore
            }
        }

        private void IniciarAvisosStockPeriodicos()
        {
            _timerStockAlert?.Stop();
            _timerStockAlert?.Dispose();

            _timerStockAlert = new System.Windows.Forms.Timer
            {
                Interval = StockInventoryAlert.IntervaloMinutos * 60 * 1000
            };
            _timerStockAlert.Tick += (_, _) =>
            {
                if (IsDisposed || !IsHandleCreated)
                    return;
                StockInventoryAlert.TryShow(this);
            };
            _timerStockAlert.Start();
        }

        /// <summary>
        /// Tiempo real: agrupa avisos de una misma venta/movimiento (debounce ~350 ms).
        /// </summary>
        private void OnStockCriticoTiempoReal(int productoId)
        {
            if (productoId <= 0 || IsDisposed)
                return;

            lock (_stockCriticoSync)
                _stockCriticoPendiente.Add(productoId);

            void ArmarDebounce()
            {
                if (IsDisposed)
                    return;

                _timerStockCriticoDebounce ??= new System.Windows.Forms.Timer { Interval = 350 };
                _timerStockCriticoDebounce.Tick -= TimerStockCriticoDebounce_Tick;
                _timerStockCriticoDebounce.Tick += TimerStockCriticoDebounce_Tick;
                _timerStockCriticoDebounce.Stop();
                _timerStockCriticoDebounce.Start();
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(ArmarDebounce);
                }
                catch (ObjectDisposedException)
                {
                }
                return;
            }

            ArmarDebounce();
        }

        private void TimerStockCriticoDebounce_Tick(object? sender, EventArgs e)
        {
            _timerStockCriticoDebounce?.Stop();

            List<int> ids;
            lock (_stockCriticoSync)
            {
                ids = _stockCriticoPendiente.ToList();
                _stockCriticoPendiente.Clear();
            }

            if (ids.Count == 0 || IsDisposed)
                return;

            StockInventoryAlert.AvisarProductosSiCriticos(this, ids);
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

            PresentacionDashboardStyle.ApplyDebtRow(
                panelDashboard,
                panelDeudasActivas, lblTituloDeudasActivas, lblDeudasActivas,
                panelDeudasVencidas, lblTituloDeudasVencidas, lblDeudasVencidas,
                panelIngresoPendiente, lblTituloIngresoPendiente, lblIngresoPendiente);

            _lblDashDateTime = panelDashboard.Controls["lblDashDateTime"] as Label;
            PresentacionDashboardStyle.ActualizarEncabezado(_lblDashWelcome, _lblDashDateTime);
            EnsureDashboardUpdateEntry();
            IniciarRelojDashboard();
            _dashboardStyled = true;
        }

        /// <summary>
        /// Entrada discreta al Centro de Actualizaciones (solo ADMIN).
        /// </summary>
        private void EnsureDashboardUpdateEntry()
        {
            bool isAdmin = string.Equals(Sesion.Rol?.Trim(), "ADMIN", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                if (_lnkDashUpdate != null)
                    _lnkDashUpdate.Visible = false;
                return;
            }

            if (_lnkDashUpdate == null || _lnkDashUpdate.IsDisposed)
            {
                _lnkDashUpdate = new LinkLabel
                {
                    Name = "lnkDashUpdate",
                    AutoSize = true,
                    Font = AppTheme.FontCaption,
                    LinkColor = AppTheme.Primary,
                    ActiveLinkColor = AppTheme.PrimaryDark,
                    VisitedLinkColor = AppTheme.Primary,
                    LinkBehavior = LinkBehavior.HoverUnderline,
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent,
                    Text = $"Sistema v{AppVersion.SemanticVersion}  ·  Actualizar"
                };
                _lnkDashUpdate.LinkClicked += (_, _) => UpdateCenterSupport.Open(this);
                panelDashboard.Controls.Add(_lnkDashUpdate);
            }

            _lnkDashUpdate.Text = $"Sistema v{AppVersion.SemanticVersion}  ·  Actualizar";
            _lnkDashUpdate.Visible = true;
            _lnkDashUpdate.Location = new Point(40, 228);
            _lnkDashUpdate.BringToFront();
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
            btnChat.Visible = false;

            // Aplicar visibilidad según rol
            switch (rol)
            {
                case "ADMIN":
                    MostrarTodos();
                    break;
                case "CAJERO":
                    // Caja 1 / 2 / 3: operación de mostrador + consulta de miembros/stock.
                    btnPagar.Visible = true;
                    btnCaja.Visible = true;
                    btnHistorial.Visible = true;
                    btnAgregarCliente.Visible = true;   // CLIENTES
                    btnProductoStock.Visible = true;    // INVENTARIO
                    btnEstado.Visible = true;           // ESTADO
                    btnChat.Visible = true;
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
                rol == "ADMIN")
            {
                btnDeudas.Visible = true;
            }
            else
            {
                btnDeudas.Visible = false;
            }

            // Montos: solo ADMIN o quien tenga VER_INGRESOS.
            _puedeVerIngresos = Sesion.TienePermiso("VER_INGRESOS") || rol == "ADMIN";

            // Caja 1 / 2 / 3 (rol CAJERO): sin paneles de ingresos realizados.
            panelIngresosHoy.Visible = rol != "CAJERO";
            panelIngresosMes.Visible = rol != "CAJERO";

            // Tarjetas de deudas: misma regla que el módulo de Deudas.
            _puedeVerDeudas = btnDeudas.Visible;
            panelDeudasActivas.Visible = _puedeVerDeudas;
            panelDeudasVencidas.Visible = _puedeVerDeudas;

            // El pendiente por cobrar es un indicador operativo para Caja 1 / 2 / 3,
            // sin otorgar acceso al módulo de Deudas ni a los ingresos realizados.
            _puedeVerIngresoPendiente = _puedeVerDeudas || rol == "CAJERO";
            panelIngresoPendiente.Visible = _puedeVerIngresoPendiente;
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
            btnChat.Visible = true;
        }

        // ===============================
        // BOTONES
        // ===============================
        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            IntentarCerrarSesion();
        }

        /// <summary>
        /// Misma ruta que el botón SALIR: valida caja abierta y regresa al login.
        /// Usado por tecla END y por el botón de la barra.
        /// </summary>
        public void IntentarCerrarSesion()
        {
            if (!Sesion.Activa || IsDisposed)
                return;

            if (!Visible)
                Show();

            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Maximized;

            BringToFront();
            Activate();

            if (!PuedeSalirTrasValidarCaja())
                return;

            RealizarSalidaALogin();
        }

        /// <summary>
        /// Con caja ABIERTA el cuadre es obligatorio: "No" solo cierra el aviso (no sale).
        /// </summary>
        private bool PuedeSalirTrasValidarCaja()
        {
            var cajaBll = new CajaBLL();
            if (!cajaBll.ObtenerEstadoCaja())
                return true;

            bool quiereCuadrar = AppDialog.Confirm(
                this,
                "Aún no has cerrado caja. ¿Desea hacer el cuadre para salir?",
                "Caja abierta");

            if (!quiereCuadrar)
                return false;

            using (var frmCaja = new FrmCajaDashboard())
                frmCaja.ShowDialog(this);

            try { CargarDashboard(); } catch { /* no bloquear salida por refresh */ }

            if (cajaBll.ObtenerEstadoCaja())
            {
                AppDialog.Warning(
                    this,
                    "Debe completar el cuadre y cerrar la caja antes de salir.",
                    "Caja abierta");
                return false;
            }

            return true;
        }

        private void RealizarSalidaALogin()
        {
            Sesion.CerrarSesion();
            Hide();
            Application.OpenForms[0]?.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // OTA / UpdateManager: salir completo sin validar caja.
            if (UpdateExitSignal.ForceExitRequested
                || e.CloseReason == CloseReason.ApplicationExitCall
                || e.CloseReason == CloseReason.WindowsShutDown
                || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                base.OnFormClosing(e);
                return;
            }

            // Alt+F4 / X / CloseMainWindow (OTA): no dejar FrmLogin oculto vivo.
            if (e.CloseReason == CloseReason.UserClosing
                && Sesion.Activa
                && Visible)
            {
                if (new CajaBLL().ObtenerEstadoCaja())
                {
                    e.Cancel = true;
                    if (PuedeSalirTrasValidarCaja())
                        RealizarSalidaALogin();
                    return;
                }

                // Caja cerrada: salir de toda la aplicación (Login oculto no debe mantener UI.exe).
                e.Cancel = true;
                Sesion.CerrarSesion();
                Application.Exit();
                return;
            }

            base.OnFormClosing(e);
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
            AbrirVentanaProductosPos(null);
        }

        private void ConfigurarCapturaEscannerInicio()
        {
            KeyPreview = true;
            KeyDown -= FrmPresentacion_CapturaEscannerKeyDown;
            KeyDown += FrmPresentacion_CapturaEscannerKeyDown;
        }

        private bool DebeCapturarEscannerInicio()
        {
            if (!Visible || !Enabled)
                return false;

            if (BusquedaFocusHelper.DebeAnularAtajosTeclado(this))
                return false;

            foreach (Form frm in Application.OpenForms)
            {
                if (frm == this || frm.IsDisposed)
                    continue;
                if (frm.Modal && frm.Visible)
                    return false;
            }

            return true;
        }

        private void FrmPresentacion_CapturaEscannerKeyDown(object? sender, KeyEventArgs e)
        {
            if (ModuloAtajosTeclado.TryHandleNavegacion(e, this))
                return;

            PosScannerCaptureHelper.HandleKeyDown(
                e,
                _bufferEscannerInicio,
                ref _ultimaTeclaEscannerInicio,
                DebeCapturarEscannerInicio,
                raw =>
                {
                    if (!ProductoBarcodeNormalizer.TryNormalizeBarcode(raw, out string? codigo))
                        return;
                    if (!_intervaloEscannerInicio.TryAcceptScan())
                        return;
                    AbrirVentanaProductosPos(codigo);
                });
        }

        /// <summary>
        /// Abre Pagar → pestaña PRODUCTOS. Si hay EAN, lo agrega al carrito al abrir.
        /// </summary>
        private void AbrirVentanaProductosPos(string? codigoBarra)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FrmPagos pagos)
                {
                    pagos.Focus();
                    if (!string.IsNullOrWhiteSpace(codigoBarra))
                        pagos.ProcesarEscaneoDesdeExterno(codigoBarra);
                    else
                        pagos.ActivarTabProductosPublico();
                    return;
                }
            }

            FrmPagos pago = new FrmPagos(this);
            if (!string.IsNullOrWhiteSpace(codigoBarra))
                pago.EstablecerEscaneoInicial(codigoBarra);

            pago.ShowDialog();
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
            using FrmHistorialVentas historial = new FrmHistorialVentas(this);
            historial.ShowDialog();
            CargarDashboard();
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

                // 1. Clientes: SSOT EstadoConteosSSOT (ACTIVO+PROGRAMADO / VENCIDO = mismo grid Estado)
                CargarConteosClientes();

                // 2. Datos Financieros Reales (Vienen de MovimientosCaja: Ventas + Membresías)
                CargarIngresosDashboard();

                lblClientesActivos.ForeColor = AppTheme.Success;
                lblClientesVencidos.ForeColor = AppTheme.Error;
                CargarKpiDeudas();

                PresentacionDashboardStyle.ActualizarEncabezado(_lblDashWelcome, _lblDashDateTime);
            }
            catch (Exception ex)
            {
                AppDialog.Warning(this, "Error al actualizar indicadores: " + ex.Message, "Dashboard");
            }
        }

        /// <summary>Conteos Activos/Vencidos — Fase 9 SSOT con grid Estado Clientes.</summary>
        private void CargarConteosClientes()
        {
            var conteos = EstadoConteosSSOT.Obtener();
            lblClientesActivos.Text = conteos.Activos.ToString();
            lblClientesVencidos.Text = conteos.Vencidos.ToString();
        }

        /// <summary>Tras programación, congelar, desactivar, etc.</summary>
        private void RefrescarConteosClientes()
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(RefrescarConteosClientes));
                }
                catch (ObjectDisposedException)
                {
                }
                return;
            }

            try
            {
                new MembresiaBLL().ActualizarVencimientos();
                CargarConteosClientes();
                lblClientesActivos.ForeColor = AppTheme.Success;
                lblClientesVencidos.ForeColor = AppTheme.Error;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Refresco conteos clientes: " + ex.Message);
            }
        }

        /// <summary>Ingresos hoy/mes desde DetalleCaja (SSOT CajaDAL).</summary>
        private void CargarIngresosDashboard()
        {
            if (!_puedeVerIngresos)
            {
                lblIngresosHoy.Text = "—";
                lblIngresosMes.Text = "—";
                lblIngresosHoy.ForeColor = AppTheme.TextMuted;
                lblIngresosMes.ForeColor = AppTheme.TextMuted;
                return;
            }

            var dashboardBLL = new DashboardBLL();
            decimal hoy = dashboardBLL.IngresosHoy();
            decimal mes = dashboardBLL.IngresosMes();

            lblIngresosHoy.Text = hoy.ToString("C");
            lblIngresosMes.Text = mes.ToString("C");
            lblIngresosHoy.ForeColor = hoy > 0 ? AppTheme.Primary : AppTheme.Error;
            lblIngresosMes.ForeColor = mes > 0 ? Color.FromArgb(168, 85, 247) : AppTheme.TextMuted;
        }

        /// <summary>
        /// Tarjetas de deudas. Falla en silencio para no tumbar el resto del dashboard.
        /// </summary>
        private void CargarKpiDeudas()
        {
            if (!_puedeVerDeudas && !_puedeVerIngresoPendiente)
                return;

            try
            {
                if (_puedeVerDeudas)
                {
                    int activas = deudaKpiBLL.DeudasActivas();
                    int vencidas = deudaKpiBLL.DeudasVencidas();

                    lblDeudasActivas.Text = activas.ToString();
                    lblDeudasVencidas.Text = vencidas.ToString();
                    lblDeudasActivas.ForeColor = activas > 0 ? Color.FromArgb(147, 51, 234) : AppTheme.TextMuted;
                    lblDeudasVencidas.ForeColor = vencidas > 0 ? AppTheme.Error : AppTheme.TextMuted;
                }

                if (_puedeVerIngresoPendiente)
                {
                    decimal pendiente = deudaKpiBLL.IngresoPendiente();
                    lblIngresoPendiente.Text = pendiente.ToString("C");
                    lblIngresoPendiente.ForeColor = pendiente > 0 ? AppTheme.Success : AppTheme.TextMuted;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("KPI deudas: " + ex.Message);
            }
        }

        /// <summary>
        /// Refresco en vivo: ingresos, deudas y conteos de clientes (Fase 4 + Fase 9).
        /// </summary>
        private void RefrescarIndicadoresFinancieros()
        {
            if (IsDisposed || Disposing)
                return;

            if (!_puedeVerIngresos && !_puedeVerDeudas && !_puedeVerIngresoPendiente)
                return;

            if (InvokeRequired)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(RefrescarIndicadoresFinancieros));
                }
                catch (ObjectDisposedException)
                {
                }
                return;
            }

            try
            {
                CargarIngresosDashboard();
                CargarKpiDeudas();
                CargarConteosClientes();
                lblClientesActivos.ForeColor = AppTheme.Success;
                lblClientesVencidos.ForeColor = AppTheme.Error;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Refresco financiero home: " + ex.Message);
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
                // Entrada al CRM Financiero (shell FrmCRMFinanciero → pnlContent).
                // FrmReportes (legacy) permanece intacto; no se modifica ni se reemplaza su código.
                using FrmCRMFinanciero frm = new FrmCRMFinanciero();
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog(this);
                CargarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir CRM financiero: " + ex.Message);
            }
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            try
            {
                ModuloNavBar.AbrirChat(this);
                ActualizarBadgeChatWhatsApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir chat WhatsApp: " + ex.Message,
                    "Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeudas_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ ABRIR MÓDULO UNIFICADO DE DEUDAS
                // (Permisos ya validados en AplicarPermisos y en FrmModuloDeudas_Load)
                FrmModuloDeudas frm = new FrmModuloDeudas();
                frm.ShowDialog();

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
