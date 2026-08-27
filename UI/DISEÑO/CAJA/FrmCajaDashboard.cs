using BLL;
using CORE;
using DL;
using System;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCajaDashboard : Form
    {
       
        private CajaBLL cajaBLL = new CajaBLL();
        private CajaServiceBLL cajaService = new CajaServiceBLL();

        public FrmCajaDashboard()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            if (ThemeHost.IsDesignTime())
                return;
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloCaja);
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnCajaCambiada -= RefrescarEstadoCaja;
            AppEventos.OnPagoRegistrado -= ActualizarIngresosYBalance;
            AppEventos.OnDeudaModificada -= ActualizarIngresosYBalance;
            base.OnFormClosed(e);
        }
        private void RefrescarEstadoCaja()
        {
            bool cajaAbierta = cajaBLL.ObtenerEstadoCaja();
            ActualizarEstadoCaja(cajaAbierta);
            // Apertura/cierre: refresco completo (incluye inicial y gastos).
            ActualizarDashboard();
        }
       
        private void ActualizarEstadoCaja(bool cajaAbierta)
        {
            lblEstadoCaja.Font = new Font(lblEstadoCaja.Font, FontStyle.Bold);
            lblEstadoCaja.TextAlign = ContentAlignment.MiddleCenter;

            if (cajaAbierta)
            {
                lblEstadoCaja.Text = "ABIERTA";
                lblEstadoCaja.ForeColor = Color.Green;
                btnAbrirCaja.Enabled = false;   // 🔥 bloquea abrir
                btnCerrarCaja.Enabled = true;
                btnRegistrarIngreso.Enabled = true;
                btnRegistrarGasto.Enabled = true;
            }
            else
            {
                lblEstadoCaja.Text = "CERRADA";
                lblEstadoCaja.ForeColor = Color.Red;
                btnAbrirCaja.Enabled = true;    // 🔥 permite abrir
                btnCerrarCaja.Enabled = false;  // 🔥 bloquea cerrar
                btnRegistrarIngreso.Enabled = false;
                btnRegistrarGasto.Enabled = false;
            }
        }

        private void FrmCajaDashboard_Load(object sender, EventArgs e)
        {
            bool cajaAbierta = cajaBLL.ObtenerEstadoCaja();
            ActualizarEstadoCaja(cajaAbierta);

            AppEventos.OnCajaCambiada += RefrescarEstadoCaja;
            // Edición de deudas / pagos: solo ingresos y balance. El reverso no toca
            // monto inicial ni el panel de gastos.
            AppEventos.OnPagoRegistrado += ActualizarIngresosYBalance;
            AppEventos.OnDeudaModificada += ActualizarIngresosYBalance;

            ActualizarDashboard();
        }

        // ================================
        // ACTUALIZA DASHBOARD
        // ================================
        private void ActualizarDashboard()
        {
            try
            {
                if (IsDisposed || Disposing)
                    return;

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(ActualizarDashboard));
                    return;
                }

                lblMontoInicial.Text = cajaBLL.ObtenerMontoInicial().ToString("C");
                lblIngresosHoy.Text = cajaBLL.IngresosHoy().ToString("C");
                lblGastosHoy.Text = cajaBLL.EgresosHoy().ToString("C");
                lblBalance.Text = cajaBLL.BalanceActual().ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando caja: " + ex.Message);
            }
        }

        /// <summary>
        /// Refresco en vivo de panel1 (ingresos) y panelBalance tras un pago o la
        /// edición de una deuda. No toca panelMontoInicial ni panelGastos: el reverso
        /// del pago inicial no es un gasto y el fondo de apertura no cambia.
        /// </summary>
        private void ActualizarIngresosYBalance()
        {
            try
            {
                if (IsDisposed || Disposing)
                    return;

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(ActualizarIngresosYBalance));
                    return;
                }

                lblIngresosHoy.Text = cajaBLL.IngresosHoy().ToString("C");
                lblBalance.Text = cajaBLL.BalanceActual().ToString("C");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error refrescando ingresos/balance: " + ex.Message);
            }
        }

        // ================================
        // ABRIR CAJA
        // ================================
        private void btnAbrirCaja_Click(object sender, EventArgs e)
        {
            try
            {
                //if (cajaBLL.ObtenerCajaAbiertaHoy() != null)
                //{
                //MessageBox.Show("Ya hay una caja abierta hoy.");
                //return;
                //}

                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Ingrese monto inicial de la caja:",
                    "Apertura de Caja",
                    "0");

                if (!decimal.TryParse(input, out decimal montoInicial) || montoInicial <= 0)
                {
                    MessageBox.Show("Monto inválido.");
                    return;
                }

                cajaBLL.AbrirCajaSeguro(montoInicial, "Admin");

                MessageBox.Show("Caja abierta correctamente.");
                ActualizarEstadoCaja(true);
                ActualizarDashboard();
                AppEventos.CajaCambiada();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ================================
        // CERRAR CAJA (CUADRE REAL)
        // ================================
        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            try
            {
                if (cajaBLL.ObtenerCajaAbiertaHoy() == null)
                {
                    MessageBox.Show("No hay caja abierta.");
                    ActualizarEstadoCaja(false);
                    return;
                }

                // SISTEMA = Monto Inicial + Ingresos − Gastos (mismo criterio que BalanceActual / cuadre BLL).
                decimal montoInicial = cajaBLL.ObtenerMontoInicial();
                decimal ingresos = cajaBLL.IngresosHoy();
                decimal egresos = cajaBLL.EgresosHoy();
                decimal sistema = montoInicial + ingresos - egresos;

                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"💰 SISTEMA: {sistema:C}\n" +
                    $"(Inicial {montoInicial:C} + Ingresos {ingresos:C} − Gastos {egresos:C})\n\n" +
                    "Ingrese dinero contado:",
                    "Cierre de Caja",
                    "0"
                );

                if (!decimal.TryParse(input, out decimal contado) || contado < 0)
                {
                    MessageBox.Show("Monto inválido.");
                    return;
                }

                decimal diferencia = contado - sistema;

                string estado = diferencia == 0 ? "CUADRE PERFECTO ✅" : "DIFERENCIA ⚠️";

                DialogResult confirm = MessageBox.Show(
                    $"SISTEMA: {sistema:C}\n" +
                    $"CONTADO: {contado:C}\n" +
                    $"DIFERENCIA: {diferencia:C}\n\n" +
                    $"{estado}\n\n¿Confirmar cierre?",
                    "Confirmar Cierre",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirm != DialogResult.Yes)
                    return;

                var resultado = cajaService.CerrarCajaConCuadre(contado);

                MessageBox.Show(
                    $"✅ Caja cerrada correctamente\n\n" +
                    $"Turno: {CajaServiceBLL.ObtenerTurnoActual()}\n" +
                    $"Sistema: {resultado.sistema:C}\n" +
                    $"Diferencia: {resultado.diferencia:C}"
                );

                ActualizarEstadoCaja(false);
                ActualizarDashboard();
                AppEventos.CajaCambiada();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en cierre: " + ex.Message);
                RefrescarEstadoCaja();
            }
        }

        // ================================
        // REGISTRAR INGRESO (manual → DetalleCaja INGRESO)
        // ================================
        private void btnRegistrarIngreso_Click(object sender, EventArgs e)
        {
            if (!cajaBLL.ObtenerEstadoCaja())
            {
                MessageBox.Show("Abra la caja antes de registrar un ingreso.");
                return;
            }

            using var frm = new FrmRegistrarGasto(esIngreso: true);
            frm.ShowDialog(this);
            // AppEventos.CajaCambiada refresca paneles/movimientos; refuerzo local.
            ActualizarDashboard();
        }

        // ================================
        // REGISTRAR GASTO
        // ================================
        private void btnRegistrarGasto_Click(object sender, EventArgs e)
        {
            if (!cajaBLL.ObtenerEstadoCaja())
            {
                MessageBox.Show("Abra la caja antes de registrar un gasto.");
                return;
            }

            using var frm = new FrmRegistrarGasto(esIngreso: false);
            frm.ShowDialog(this);
            // Gasto real: sí actualiza panelGastos (y el resto).
            ActualizarDashboard();
        }

        // ================================
        // VER MOVIMIENTOS
        // ================================
        private void btnVerMovimientos_Click(object sender, EventArgs e)
        {
            FrmMovimientosCaja frm = new FrmMovimientosCaja(this); // ✅
            frm.ShowDialog();
            // Al volver, relee ingresos/balance por si hubo cambios en otra ventana.
            ActualizarIngresosYBalance();
        }

        // ================================
        // HISTORIAL DE CIERRES DE CAJA
        // ================================
        private void btnCierresCaja_Click(object sender, EventArgs e)
        {
            using var frm = new FrmCierresCaja(this);
            frm.ShowDialog(this);
        }

        // ================================
        // CERRAR FORM
        // ================================
        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}