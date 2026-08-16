using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using CORE;
using CORE.Commands;

namespace UI
{
    /// <summary>
    /// Edición del financiamiento de una deuda activa: concepto, plan o producto
    /// financiado, pago inicial y fecha límite de pago. El monto financiado se deriva
    /// del total menos el pago inicial; si el pago inicial cambia, el anterior se
    /// reversa en caja y se asienta el nuevo. La membresía del miembro no se altera.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmEditarDeuda : Form
    {
        private const string NombreProductoCredito = "PRODUCTO A CRÉDITO";
        private const string TextoProductoCredito = "Producto a crédito / otro concepto";
        private const string PrefijoSaldoPlan = "Saldo plan ";
        private const string MetodoPagoInicial = "Efectivo";

        private readonly DeudaBLL deudaBLL = new DeudaBLL();
        private readonly PlanBLL planBLL = new PlanBLL();
        private readonly CajaBLL cajaBLL = new CajaBLL();

        private int _deudaId;
        private int? _membresiaId;
        private int? _planIdOriginal;
        private decimal _montoPagado;
        private decimal _pagoInicialActual;
        private decimal _totalFinanciado;
        private decimal _totalOriginal;
        private DateTime _fechaCreacion = DateTime.Today;
        private bool _cargando;
        private bool _editable = true;
        private OpcionPlan? _opcionAnterior;

        /// <summary>Deuda que está siendo editada.</summary>
        public int DeudaId => _deudaId;

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmEditarDeuda()
        {
            InitializeComponent();
        }

        /// <exception cref="Exception">Si la deuda no existe.</exception>
        public FrmEditarDeuda(int deudaId) : this()
        {
            CargarDeuda(deudaId);
        }

        // ===============================
        // CARGA
        // ===============================
        private void CargarDeuda(int deudaId)
        {
            DataRow fila = deudaBLL.ObtenerDeudaDetalle(deudaId)
                ?? throw new Exception("La deuda no existe o fue eliminada.");

            _deudaId = deudaId;
            _montoPagado = LeerDecimal(fila, "MontoPagado");
            _pagoInicialActual = LeerDecimal(fila, "PagoInicial");
            _membresiaId = LeerEntero(fila, "MembresiaId");
            _planIdOriginal = LeerEntero(fila, "PlanId");

            // El total del financiamiento es lo que aún se financia más el inicial ya cobrado:
            // así el pago inicial se puede subir o bajar sin perder el precio pactado.
            _totalOriginal = LeerDecimal(fila, "MontoTotal") + _pagoInicialActual;
            _totalFinanciado = _totalOriginal;

            string estado = LeerTexto(fila, "Estado");

            Text = $"Editar deuda #{deudaId}";
            lblCliente.Text = LeerTexto(fila, "Cliente");
            lblEstado.Text = estado.ToUpperInvariant();
            lblEstado.ForeColor = estado.Equals("ACTIVA", StringComparison.OrdinalIgnoreCase)
                ? Color.Firebrick
                : Color.DimGray;
            lblMontoPagado.Text = "RD$ " + _montoPagado.ToString("N2");

            if (fila.Table.Columns.Contains("FechaCreacion") && fila["FechaCreacion"] != DBNull.Value)
                _fechaCreacion = Convert.ToDateTime(fila["FechaCreacion"]).Date;
            lblFechaCreacion.Text = _fechaCreacion.ToString("dd/MM/yyyy");

            _cargando = true;
            try
            {
                CargarOpcionesPlan(_planIdOriginal, LeerTexto(fila, "Plan"));
                txtConcepto.Text = LeerTexto(fila, "Concepto");
                txtPagoInicial.Text = _pagoInicialActual.ToString("N2");

                DateTime vencimiento = fila.Table.Columns.Contains("FechaVencimiento")
                                       && fila["FechaVencimiento"] != DBNull.Value
                    ? Convert.ToDateTime(fila["FechaVencimiento"]).Date
                    : DateTime.Today;

                dtpVencimiento.Value = vencimiento < dtpVencimiento.MinDate
                    ? DateTime.Today
                    : vencimiento;
            }
            finally
            {
                _cargando = false;
            }

            ActualizarTotales();

            if (!estado.Equals("ACTIVA", StringComparison.OrdinalIgnoreCase))
            {
                BloquearEdicion(
                    $"Solo se pueden editar deudas activas. Esta deuda está {estado.ToUpperInvariant()}.");
                return;
            }

            if (!PermitePagoInicial())
            {
                txtPagoInicial.ReadOnly = true;
                txtPagoInicial.BackColor = SystemColors.Control;
                lblNota.Text =
                    "El pago inicial pertenece a la venta que originó esta deuda, no al financiamiento: " +
                    "se ajusta desde la venta. Aquí puede corregir concepto, producto y fecha límite.";
            }
            else if (_membresiaId.HasValue)
            {
                lblNota.Text =
                    "Corrija el pago inicial del plan financiado: el monto anterior se reversa en caja " +
                    "y el nuevo se registra como ingreso. La vigencia de la membresía no se modifica.";
            }
        }

        /// <summary>
        /// El pago inicial solo se corrige donde el financiamiento lo registró: planes
        /// financiados y deudas que ya tienen un inicial asentado.
        /// </summary>
        private bool PermitePagoInicial() => _membresiaId.HasValue || _pagoInicialActual > 0m;

        /// <summary>
        /// Ofrece todos los planes activos del catálogo (M-A y futuros entran solos)
        /// y, cuando la deuda no pertenece a una membresía, la opción de producto a crédito.
        /// </summary>
        private void CargarOpcionesPlan(int? planIdActual, string nombrePlanActual)
        {
            cmbPlan.Items.Clear();

            // Una deuda de membresía siempre debe conservar un plan.
            if (!_membresiaId.HasValue)
                cmbPlan.Items.Add(new OpcionPlan(null, string.Empty, 0m));

            DataTable planes = planBLL.ObtenerPlanes() ?? new DataTable();
            bool planActualEnCatalogo = false;

            foreach (DataRow row in planes.Rows)
            {
                if (row["Id"] == DBNull.Value)
                    continue;

                int id = Convert.ToInt32(row["Id"]);
                string nombre = (row["Nombre"]?.ToString() ?? string.Empty).Trim();

                if (nombre.Length == 0)
                    continue;

                // El pseudo-plan de ventas a crédito no es un plan de membresía.
                if (nombre.Equals(NombreProductoCredito, StringComparison.OrdinalIgnoreCase))
                    continue;

                decimal precio = row.Table.Columns.Contains("Precio") && row["Precio"] != DBNull.Value
                    ? Convert.ToDecimal(row["Precio"])
                    : 0m;

                cmbPlan.Items.Add(new OpcionPlan(id, nombre, precio));

                if (planIdActual.HasValue && planIdActual.Value == id)
                    planActualEnCatalogo = true;
            }

            // Plan desactivado del catálogo: se conserva para no perder el dato de la deuda.
            if (planIdActual.HasValue && !planActualEnCatalogo)
            {
                string nombre = string.IsNullOrWhiteSpace(nombrePlanActual)
                    ? $"#{planIdActual.Value}"
                    : nombrePlanActual.Trim();

                cmbPlan.Items.Add(new OpcionPlan(planIdActual.Value, nombre, 0m, inactivo: true));
            }

            SeleccionarOpcionPlan(planIdActual);
        }

        private void SeleccionarOpcionPlan(int? planId)
        {
            for (int i = 0; i < cmbPlan.Items.Count; i++)
            {
                if (cmbPlan.Items[i] is OpcionPlan opcion && opcion.PlanId == planId)
                {
                    cmbPlan.SelectedIndex = i;
                    _opcionAnterior = opcion;
                    return;
                }
            }

            if (cmbPlan.Items.Count > 0)
            {
                cmbPlan.SelectedIndex = 0;
                _opcionAnterior = cmbPlan.Items[0] as OpcionPlan;
            }
        }

        // ===============================
        // EVENTOS
        // ===============================
        private void cmbPlan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPlan.SelectedItem is not OpcionPlan opcion)
                return;

            if (!_cargando)
            {
                if (opcion.PlanId.HasValue)
                {
                    string sugerido = SugerirConcepto(opcion.NombrePlan);
                    if (sugerido.Length > 0)
                        txtConcepto.Text = sugerido;
                }

                // Cambiar de plan cambia el total a financiar; volver al original lo restaura.
                if (opcion.PlanId == _planIdOriginal)
                    _totalFinanciado = _totalOriginal;
                else if (opcion.Precio > 0m)
                    _totalFinanciado = opcion.Precio;
            }

            lblPrecioPlan.Text = opcion.Precio > 0
                ? $"Precio de lista: RD$ {opcion.Precio:N2}"
                : string.Empty;

            _opcionAnterior = opcion;
            ActualizarTotales();
        }

        /// <summary>
        /// Propone el concepto del plan recién elegido solo si el actual seguía siendo el
        /// que genera el sistema al financiar ("M-A" o "Saldo plan M-A"). Un texto escrito
        /// por el usuario nunca se sobreescribe.
        /// </summary>
        private string SugerirConcepto(string nombrePlanNuevo)
        {
            string actual = txtConcepto.Text.Trim();
            if (actual.Length == 0)
                return nombrePlanNuevo;

            string anterior = _opcionAnterior?.NombrePlan ?? string.Empty;
            if (anterior.Length == 0)
                return string.Empty;

            if (actual.Equals(anterior, StringComparison.OrdinalIgnoreCase))
                return nombrePlanNuevo;

            if (actual.Equals(PrefijoSaldoPlan + anterior, StringComparison.OrdinalIgnoreCase))
                return PrefijoSaldoPlan + nombrePlanNuevo;

            return string.Empty;
        }

        private void txtPagoInicial_TextChanged(object sender, EventArgs e)
        {
            ActualizarTotales();
        }

        private void txtPagoInicial_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar))
                return;

            if (e.KeyChar == '.' || e.KeyChar == ',')
                return;

            e.Handled = true;
        }

        private void txtPagoInicial_Leave(object sender, EventArgs e)
        {
            if (TryLeerPagoInicial(out decimal pagoInicial))
                txtPagoInicial.Text = pagoInicial.ToString("N2");
        }

        private void dtpVencimiento_ValueChanged(object sender, EventArgs e)
        {
            if (_cargando)
                return;

            int dias = (dtpVencimiento.Value.Date - DateTime.Today).Days;
            lblTituloVencimiento.ForeColor = dias < 0 ? Color.Firebrick : SystemColors.ControlText;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarEntrada(out string concepto, out decimal pagoInicial, out int? planId))
                    return;

                decimal financiado = _totalFinanciado - pagoInicial;

                if (!ConfirmarOperacion(pagoInicial, financiado))
                    return;

                CommandResult resultado;
                Cursor = Cursors.WaitCursor;
                try
                {
                    resultado = DeudaCommandService.ActualizarDeudaFinanciamiento(
                        _deudaId,
                        concepto,
                        _totalFinanciado,
                        pagoInicial,
                        dtpVencimiento.Value.Date,
                        planId,
                        MetodoPagoInicial,
                        Sesion.Usuario);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }

                if (!resultado.Success)
                {
                    MessageBox.Show(this, resultado.Message, "Editar deuda",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(this, resultado.Message, "Editar deuda",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo actualizar la deuda: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Avisa del reverso que se va a asentar en caja y de que la deuda quedará saldada,
        /// para que el cajero cuadre el efectivo antes de guardar.
        /// </summary>
        private bool ConfirmarOperacion(decimal pagoInicial, decimal financiado)
        {
            decimal saldo = financiado - _montoPagado;
            bool cambiaInicial = pagoInicial != _pagoInicialActual;

            if (!cambiaInicial && saldo > 0m)
                return true;

            var mensaje = new System.Text.StringBuilder();

            if (cambiaInicial)
            {
                mensaje.AppendLine(
                    $"Pago inicial: RD$ {_pagoInicialActual:N2} → RD$ {pagoInicial:N2}");
                mensaje.AppendLine();

                if (_pagoInicialActual > 0m)
                    mensaje.AppendLine($"Reverso en caja: -RD$ {_pagoInicialActual:N2}");

                if (pagoInicial > 0m)
                    mensaje.AppendLine($"Ingreso en caja: +RD$ {pagoInicial:N2}");

                mensaje.AppendLine(
                    $"Diferencia en efectivo: RD$ {(pagoInicial - _pagoInicialActual):N2}");
                mensaje.AppendLine();
            }

            mensaje.AppendLine(saldo > 0m
                ? $"Saldo financiado: RD$ {saldo:N2}"
                : "La deuda queda saldada y saldrá de Gestión de Deudas.");

            mensaje.AppendLine();
            mensaje.Append("¿Desea continuar?");

            return MessageBox.Show(
                this,
                mensaje.ToString(),
                "Editar deuda",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        // ===============================
        // VALIDACIÓN
        // ===============================
        private bool ValidarEntrada(out string concepto, out decimal pagoInicial, out int? planId)
        {
            concepto = txtConcepto.Text.Trim();
            pagoInicial = 0m;
            planId = null;

            if (cmbPlan.SelectedItem is not OpcionPlan opcion)
            {
                Advertir("Seleccione el plan financiado o la opción de producto a crédito.", cmbPlan);
                return false;
            }

            planId = opcion.PlanId;

            if (_membresiaId.HasValue && !planId.HasValue)
            {
                Advertir("Esta deuda financia una membresía: debe mantener un plan seleccionado.", cmbPlan);
                return false;
            }

            if (concepto.Length == 0)
            {
                Advertir("Escriba el concepto de la deuda (plan o producto financiado).", txtConcepto);
                return false;
            }

            if (_totalFinanciado <= 0m)
            {
                Advertir(
                    "El plan seleccionado no tiene un total válido para financiar.",
                    cmbPlan);
                return false;
            }

            if (!TryLeerPagoInicial(out pagoInicial))
            {
                Advertir("Ingrese un pago inicial válido (0 o mayor).", txtPagoInicial);
                return false;
            }

            if (pagoInicial > _totalFinanciado)
            {
                Advertir(
                    $"El pago inicial no puede superar el total financiado (RD$ {_totalFinanciado:N2}).",
                    txtPagoInicial);
                return false;
            }

            if (_totalFinanciado - pagoInicial < _montoPagado)
            {
                Advertir(
                    "Con ese pago inicial el financiamiento queda por debajo de los abonos ya " +
                    $"cobrados (RD$ {_montoPagado:N2}). Reduzca el pago inicial.",
                    txtPagoInicial);
                return false;
            }

            if (dtpVencimiento.Value.Date < _fechaCreacion)
            {
                Advertir(
                    $"La fecha límite no puede ser anterior al registro de la deuda ({_fechaCreacion:dd/MM/yyyy}).",
                    dtpVencimiento);
                return false;
            }

            if (pagoInicial != _pagoInicialActual && cajaBLL.ObtenerCajaAbiertaHoy() == null)
            {
                Advertir(
                    "Debe haber una caja abierta: el pago inicial anterior se reversa y el nuevo " +
                    "se registra como ingreso en POS.",
                    txtPagoInicial);
                return false;
            }

            return true;
        }

        private void Advertir(string mensaje, Control control)
        {
            MessageBox.Show(this, mensaje, "Editar deuda", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (control.Enabled)
                control.Focus();
        }

        private void BloquearEdicion(string motivo)
        {
            _editable = false;
            cmbPlan.Enabled = false;
            txtConcepto.ReadOnly = true;
            txtPagoInicial.ReadOnly = true;
            dtpVencimiento.Enabled = false;
            btnGuardar.Enabled = false;

            lblNota.ForeColor = Color.Firebrick;
            lblNota.Text = motivo;
        }

        // ===============================
        // AUXILIARES
        // ===============================
        /// <summary>
        /// Refresca total, saldo resultante y el aviso del reverso con lo que hay en pantalla.
        /// </summary>
        private void ActualizarTotales()
        {
            lblTotalFinanciado.Text = _totalFinanciado > 0m
                ? "RD$ " + _totalFinanciado.ToString("N2")
                : "—";

            if (!TryLeerPagoInicial(out decimal pagoInicial))
            {
                lblSaldoResultante.Text = "—";
                lblSaldoResultante.ForeColor = Color.DimGray;
                lblAvisoReverso.Text = string.Empty;
                return;
            }

            ActualizarAvisoReverso(pagoInicial);

            if (pagoInicial > _totalFinanciado)
            {
                lblSaldoResultante.Text = "Supera el total";
                lblSaldoResultante.ForeColor = Color.Firebrick;
                return;
            }

            decimal saldo = _totalFinanciado - pagoInicial - _montoPagado;

            if (saldo < 0m)
            {
                lblSaldoResultante.Text = "Menor a lo abonado";
                lblSaldoResultante.ForeColor = Color.Firebrick;
                return;
            }

            lblSaldoResultante.Text = "RD$ " + saldo.ToString("N2");
            lblSaldoResultante.ForeColor = saldo > 0m ? Color.Firebrick : Color.SeaGreen;
        }

        private void ActualizarAvisoReverso(decimal pagoInicial)
        {
            if (!_editable || pagoInicial == _pagoInicialActual)
            {
                lblAvisoReverso.Text = string.Empty;
                return;
            }

            string reverso = _pagoInicialActual > 0m
                ? $"Reverso en caja: -RD$ {_pagoInicialActual:N2}. "
                : string.Empty;

            lblAvisoReverso.Text = reverso +
                (pagoInicial > 0m ? $"Nuevo ingreso: +RD$ {pagoInicial:N2}." : "Queda sin pago inicial.");
        }

        private bool TryLeerPagoInicial(out decimal pagoInicial)
        {
            pagoInicial = 0m;
            string texto = txtPagoInicial.Text.Trim();

            if (texto.Length == 0)
                return true;

            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out pagoInicial))
                return false;

            return pagoInicial >= 0m;
        }

        private static string LeerTexto(DataRow fila, string columna)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return string.Empty;

            return (fila[columna]?.ToString() ?? string.Empty).Trim();
        }

        private static decimal LeerDecimal(DataRow fila, string columna)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return 0m;

            return Convert.ToDecimal(fila[columna]);
        }

        private static int? LeerEntero(DataRow fila, string columna)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return null;

            return Convert.ToInt32(fila[columna]);
        }

        /// <summary>
        /// Opción del combo: plan real del catálogo o financiamiento sin plan
        /// (producto a crédito / otro concepto).
        /// </summary>
        private sealed class OpcionPlan
        {
            public OpcionPlan(int? planId, string nombrePlan, decimal precio, bool inactivo = false)
            {
                PlanId = planId;
                NombrePlan = nombrePlan?.Trim() ?? string.Empty;
                Precio = precio;

                Texto = planId.HasValue
                    ? "Plan: " + NombrePlan + (inactivo ? " (inactivo)" : string.Empty)
                    : TextoProductoCredito;
            }

            public int? PlanId { get; }
            public string NombrePlan { get; }
            public decimal Precio { get; }
            public string Texto { get; }

            public override string ToString() => Texto;
        }
    }
}
