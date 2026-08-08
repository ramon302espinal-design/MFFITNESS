using BLL;
using CORE;
using DL;
using System;
using System.Windows.Forms;
using UI.Helpers;

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
            if (DesignMode)
                return;
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloCaja);
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnCajaCambiada -= RefrescarEstadoCaja;
            base.OnFormClosed(e);
        }
        private void RefrescarEstadoCaja()
        {
          

            bool cajaAbierta = cajaBLL.ObtenerEstadoCaja();
            ActualizarEstadoCaja(cajaAbierta);
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
            }
            else
            {
                lblEstadoCaja.Text = "CERRADA";
                lblEstadoCaja.ForeColor = Color.Red;
                btnAbrirCaja.Enabled = true;    // 🔥 permite abrir
                btnCerrarCaja.Enabled = false;  // 🔥 bloquea cerrar
            }
        }

        private void FrmCajaDashboard_Load(object sender, EventArgs e)
        {
            bool cajaAbierta = cajaBLL.ObtenerEstadoCaja();
            ActualizarEstadoCaja(cajaAbierta);

            AppEventos.OnCajaCambiada += RefrescarEstadoCaja;

            ActualizarDashboard();
        }

        // ================================
        // ACTUALIZA DASHBOARD
        // ================================
        private void ActualizarDashboard()
        {
            try
            {
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

                cajaBLL.AbrirCajaSeguro(montoInicial, Sesion.Usuario ?? "ADMIN");

                MessageBox.Show("Caja abierta correctamente.");
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
           
            AppEventos.CajaCambiada();
            try
            {
                //if (cajaBLL.ObtenerCajaAbiertaHoy() == null)
                //{
                //MessageBox.Show("No hay caja abierta.");
                //return;
                //}

                decimal ingresos = cajaBLL.IngresosHoy();
                decimal egresos = cajaBLL.EgresosHoy();
                decimal sistema = ingresos - egresos;

                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"💰 SISTEMA: {sistema:C}\n\nIngrese dinero contado:",
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
                    $"Sistema: {resultado.sistema:C}\n" +
                    $"Diferencia: {resultado.diferencia:C}"
                );
                AppEventos.CajaCambiada();
                ActualizarDashboard();
            }    
            catch (Exception ex)
            {
                MessageBox.Show("Error en cierre: " + ex.Message);
            }
        }

        // ================================
        // REGISTRAR GASTO
        // ================================
        private void btnRegistrarGasto_Click(object sender, EventArgs e)
        {
            FrmRegistrarGasto frm = new FrmRegistrarGasto();
            frm.ShowDialog();
            ActualizarDashboard();
        }

        // ================================
        // VER MOVIMIENTOS
        // ================================
        private void btnVerMovimientos_Click(object sender, EventArgs e)
        {
            FrmMovimientosCaja frm = new FrmMovimientosCaja(this); // ✅
            frm.ShowDialog();
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