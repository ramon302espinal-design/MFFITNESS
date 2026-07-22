using BLL;
using BLL.Commands;
using CORE;
using System;
using System.Data;
using System.Windows.Forms;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCrearDeuda : Form
    {
        private readonly ClienteBLL clienteBLL = new ClienteBLL();
        private readonly PlanBLL planBLL = new PlanBLL();
        private readonly MembresiaBLL membresiaBLL = new MembresiaBLL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();
        private readonly CajaBLL cajaBLL = new CajaBLL();

        private decimal _precioPlan;

        public FrmCrearDeuda()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        private void FrmCrearDeuda_Load(object sender, EventArgs e)
        {
            cbClientes.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoPlan.DropDownStyle = ComboBoxStyle.DropDownList;
            dtpFechaVencimientodeuda.Value = DateTime.Today.AddDays(30);
            dtpFechaVencimientodeuda.MinDate = DateTime.Today;

            CargarClientes();
            CargarPlanes();
            CalcularSaldoRestante();
        }

        private void CargarClientes()
        {
            try
            {
                DataTable dt = clienteBLL.ObtenerClientes();
                cbClientes.DataSource = dt;
                cbClientes.DisplayMember = "Nombre";
                cbClientes.ValueMember = "Id";
                cbClientes.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar clientes: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CargarPlanes()
        {
            try
            {
                DataTable tabla = planBLL.ObtenerPlanes();
                DataView dv = tabla.DefaultView;
                // Mismos planes del flujo de renovación / activación.
                dv.RowFilter = "Nombre IN ('PREMIUM', 'PRO', 'MENSUALIDAD', '3x')";

                cmbTipoPlan.DataSource = dv;
                cmbTipoPlan.DisplayMember = "Nombre";
                cmbTipoPlan.ValueMember = "Id";
                cmbTipoPlan.SelectedIndex = -1;

                txtMonto.Text = "0.00";
                _precioPlan = 0m;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar planes: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void cmbTipoPlan_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _precioPlan = 0m;

            if (cmbTipoPlan.SelectedItem is DataRowView row &&
                row["Precio"] != DBNull.Value)
            {
                _precioPlan = Convert.ToDecimal(row["Precio"]);
                string nombrePlan = row["Nombre"]?.ToString() ?? "plan";

                txtMonto.Text = _precioPlan.ToString("N2");

                if (string.IsNullOrWhiteSpace(txtConcepto.Text) ||
                    txtConcepto.Text.StartsWith("Saldo plan ", StringComparison.OrdinalIgnoreCase) ||
                    txtConcepto.Text.StartsWith("Financiamiento ", StringComparison.OrdinalIgnoreCase))
                {
                    txtConcepto.Text = $"Financiamiento {nombrePlan}";
                }
            }
            else
            {
                txtMonto.Text = "0.00";
            }

            if (string.IsNullOrWhiteSpace(txtPagodeinicio.Text))
                txtPagodeinicio.Text = "0";

            CalcularSaldoRestante();
        }

        private void txtPagodeinicio_TextChanged(object? sender, EventArgs e)
        {
            CalcularSaldoRestante();
        }

        private void txtPagodeinicio_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void CalcularSaldoRestante()
        {
            decimal pagoInicio = decimal.TryParse(txtPagodeinicio.Text, out decimal p) ? p : 0m;
            if (pagoInicio < 0) pagoInicio = 0m;

            decimal saldo = _precioPlan - pagoInicio;
            if (saldo < 0) saldo = 0m;

            lblSaldorestante.Text = $"${saldo:N2}";
            dtpFechaVencimientodeuda.Enabled = saldo > 0;
        }

        private bool TryObtenerPlanSeleccionado(out int planId, out string nombrePlan)
        {
            planId = 0;
            nombrePlan = string.Empty;

            if (cmbTipoPlan.SelectedValue == null || cmbTipoPlan.SelectedIndex < 0)
                return false;

            if (!int.TryParse(cmbTipoPlan.SelectedValue.ToString(), out planId) || planId <= 0)
                return false;

            nombrePlan = cmbTipoPlan.Text?.Trim() ?? string.Empty;
            return true;
        }

        private bool VerificarCajaSiHayPagoInicial(decimal pagoInicial)
        {
            if (pagoInicial <= 0)
                return true;

            if (cajaBLL.ObtenerCajaAbiertaHoy() != null)
                return true;

            MessageBox.Show(
                "Hay un pago de inicio. Debe haber una caja abierta para registrar el ingreso en POS.",
                "Caja cerrada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private bool Validar()
        {
            if (cbClientes.SelectedIndex < 0 || cbClientes.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un cliente.");
                return false;
            }

            if (!TryObtenerPlanSeleccionado(out _, out _))
            {
                MessageBox.Show("Seleccione un tipo de plan (PREMIUM, PRO, MENSUALIDAD o 3x).");
                return false;
            }

            if (_precioPlan <= 0)
            {
                MessageBox.Show("El plan seleccionado no tiene un precio válido.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtConcepto.Text))
            {
                MessageBox.Show("Ingrese un concepto.");
                return false;
            }

            decimal pagoInicio = decimal.TryParse(txtPagodeinicio.Text, out decimal p) ? p : -1m;
            if (pagoInicio < 0 || pagoInicio > _precioPlan)
            {
                MessageBox.Show("Pago de inicio inválido. Debe estar entre 0 y el monto del plan.");
                return false;
            }

            decimal saldo = _precioPlan - pagoInicio;
            if (saldo > 0 && dtpFechaVencimientodeuda.Value.Date < DateTime.Today)
            {
                MessageBox.Show("La fecha límite de pago no puede ser anterior a hoy.");
                return false;
            }

            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validar()) return;

                int clienteId = Convert.ToInt32(cbClientes.SelectedValue);
                if (!TryObtenerPlanSeleccionado(out int planId, out string nombrePlan))
                    return;

                decimal pagoInicio = decimal.TryParse(txtPagodeinicio.Text, out decimal p) ? p : 0m;
                decimal saldo = _precioPlan - pagoInicio;
                if (saldo < 0) saldo = 0m;

                if (!VerificarCajaSiHayPagoInicial(pagoInicio))
                    return;

                if (deudaBLL.ClienteBloqueadoPorDeudaPendiente(clienteId, out string motivoDeuda))
                {
                    MessageBox.Show(
                        motivoDeuda,
                        "Deuda pendiente",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (membresiaBLL.ClienteNoElegibleParaFinanciamiento(clienteId, out string motivoFin))
                {
                    MessageBox.Show(
                        motivoFin,
                        "Financiamiento no disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string usuario = string.IsNullOrWhiteSpace(Sesion.Usuario) ? "ADMIN" : Sesion.Usuario;
                string conceptoPago = string.IsNullOrWhiteSpace(txtConcepto.Text)
                    ? $"Pago inicial - Membresía {nombrePlan}"
                    : txtConcepto.Text.Trim();

                DateTime? fechaVencimientoDeuda = saldo > 0
                    ? dtpFechaVencimientodeuda.Value.Date
                    : null;

                // Ciclo completo POS: membresía + deuda + caja + historial (misma TX en BLL).
                var result = MembresiaCommandService.VenderMembresiaFinanciada(
                    clienteId,
                    planId,
                    pagoInicio,
                    "Efectivo",
                    conceptoPago,
                    fechaVencimientoDeuda,
                    usuario);

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                AppEventos.PagoRegistrado();
                // DeudaModificada ya se dispara en BLL al crear la deuda financiada.

                string resumen =
                    $"Deuda/financiamiento registrado correctamente.\n\n" +
                    $"Cliente: {cbClientes.Text}\n" +
                    $"Plan: {nombrePlan}\n" +
                    $"Monto plan: ${_precioPlan:N2}\n" +
                    $"Pago de inicio: ${pagoInicio:N2}\n" +
                    $"Saldo pendiente: ${saldo:N2}\n" +
                    (saldo > 0
                        ? $"Vence deuda: {dtpFechaVencimientodeuda.Value:dd/MM/yyyy}\n"
                        : string.Empty) +
                    "\nQueda reflejado en Estado Clientes, Historial de Membresía" +
                    (pagoInicio > 0 ? " y Caja." : ".");

                MessageBox.Show(
                    resumen,
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Embebido en FrmModuloDeudas: no cerrar (evita disposed + AppEventos).
                if (!TopLevel)
                {
                    LimpiarFormulario();
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormulario()
        {
            cbClientes.SelectedIndex = -1;
            cmbTipoPlan.SelectedIndex = -1;
            txtConcepto.Clear();
            txtMonto.Text = "0.00";
            txtPagodeinicio.Text = "0";
            _precioPlan = 0m;
            lblSaldorestante.Text = "$0.00";
            dtpFechaVencimientodeuda.Value = DateTime.Today.AddDays(30);
            CalcularSaldoRestante();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                LimpiarFormulario();
                return;
            }

            Close();
        }
    }
}
