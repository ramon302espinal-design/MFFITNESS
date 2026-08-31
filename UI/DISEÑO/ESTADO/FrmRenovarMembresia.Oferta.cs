using System;
using System.Globalization;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using BLL.Models;
using DTO;

namespace UI.DISEÑO
{
    public partial class FrmRenovarMembresia
    {
        private bool _sincronizandoOferta;
        private readonly ClienteBLL _clienteBLL = new ClienteBLL();
        private readonly DeudaBLL _deudaBLL = new DeudaBLL();

        private bool EsPlanOfertaSeleccionado()
        {
            if (cmbPlan.SelectedItem is System.Data.DataRowView row)
                return PlanNombres.EsOferta(Convert.ToString(row["Nombre"]));
            return PlanNombres.EsOferta(cmbPlan.Text);
        }

        /// <summary>Oferta no aplica a ACTIVO, CONGELADO ni con deudas activas.</summary>
        private bool PuedeActivarOferta(out string motivo)
        {
            motivo = string.Empty;
            if (_clienteId <= 0)
            {
                motivo = "Cliente no válido para oferta.";
                return false;
            }

            string estado = _clienteBLL.ObtenerEstadoMembresia(_clienteId);
            if (string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
            {
                motivo = "No se puede aplicar oferta a un miembro ACTIVO.";
                return false;
            }

            if (string.Equals(estado, "CONGELADO", StringComparison.OrdinalIgnoreCase))
            {
                motivo = "No se puede aplicar oferta a un miembro CONGELADO.";
                return false;
            }

            if (_deudaBLL.TieneDeudasActivas(_clienteId))
            {
                motivo = "No se puede aplicar oferta: el cliente tiene deudas pendientes.";
                return false;
            }

            return true;
        }

        private void ActualizarPanelOfertaPorPlan()
        {
            if (_sincronizandoOferta)
                return;

            if (!EsPlanOfertaSeleccionado())
            {
                if (pnlOferta.Visible)
                    ResetOfertaCampos();
                lblPrecio.Visible = true;
                return;
            }

            if (!PuedeActivarOferta(out string motivo))
            {
                MessageBox.Show(this, motivo, "Oferta no disponible",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _sincronizandoOferta = true;
                cmbPlan.SelectedIndex = -1;
                _sincronizandoOferta = false;
                ResetOfertaCampos();
                return;
            }

            pnlOferta.Visible = true;
            lblPrecio.Visible = false;

            _sincronizandoOferta = true;
            if (string.IsNullOrWhiteSpace(txtDescuentoPorcental.Text))
                txtDescuentoPorcental.Text = "100";
            if (string.IsNullOrWhiteSpace(txtDescuentoMonto.Text))
                txtDescuentoMonto.Text = "0.00";
            _sincronizandoOferta = false;

            RecalcularOfertaDesdeReferencia();
        }

        private void txtDescuentoPorcental_TextChanged(object? sender, EventArgs e)
        {
            if (_sincronizandoOferta || !EsPlanOfertaSeleccionado())
                return;
            RecalcularOfertaDesdeReferencia();
        }

        private void txtDescuentoMonto_TextChanged(object? sender, EventArgs e)
        {
            if (_sincronizandoOferta || !EsPlanOfertaSeleccionado())
                return;
            RecalcularOfertaDesdeReferencia();
        }

        private void RecalcularOfertaDesdeReferencia()
        {
            if (!TryParseDecimalFlexible(txtDescuentoMonto.Text, out decimal valorRef))
                valorRef = 0;
            if (valorRef < 0) valorRef = 0;

            if (!TryParseDecimalFlexible(txtDescuentoPorcental.Text, out decimal pct))
                pct = 0;
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;

            decimal descuento = Math.Round(valorRef * pct / 100m, 2, MidpointRounding.AwayFromZero);
            if (descuento > valorRef) descuento = valorRef;
            decimal total = Math.Round(valorRef - descuento, 2, MidpointRounding.AwayFromZero);

            lblTotalPagar.Text = "RD$ " + total.ToString("N2", CultureInfo.CurrentCulture);
        }

        private void ResetOfertaCampos()
        {
            _sincronizandoOferta = true;
            txtDescuentoPorcental.Text = "100";
            txtDescuentoMonto.Text = "0.00";
            txtMotivo.Clear();
            lblTotalPagar.Text = "RD$ 0.00";
            _sincronizandoOferta = false;
            pnlOferta.Visible = false;
            lblPrecio.Visible = true;
        }

        private static bool TryParseDecimalFlexible(string? texto, out decimal valor)
        {
            texto = (texto ?? string.Empty).Trim().Replace("%", "").Replace("RD$", "").Replace("$", "").Trim();
            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out valor))
                return true;
            return decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out valor);
        }

        private bool TryObtenerDatosOferta(
            out decimal valorReferencia,
            out decimal porcentaje,
            out decimal descuento,
            out decimal totalPagar,
            out string motivo)
        {
            valorReferencia = 0;
            porcentaje = 0;
            descuento = 0;
            totalPagar = 0;
            motivo = (txtMotivo.Text ?? string.Empty).Trim();

            if (!EsPlanOfertaSeleccionado())
                return false;

            TryParseDecimalFlexible(txtDescuentoMonto.Text, out valorReferencia);
            TryParseDecimalFlexible(txtDescuentoPorcental.Text, out porcentaje);
            if (valorReferencia < 0) valorReferencia = 0;
            if (porcentaje < 0) porcentaje = 0;
            if (porcentaje > 100) porcentaje = 100;

            descuento = Math.Round(valorReferencia * porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
            if (descuento > valorReferencia) descuento = valorReferencia;
            totalPagar = Math.Round(valorReferencia - descuento, 2, MidpointRounding.AwayFromZero);
            return true;
        }

        private bool ConfirmarRenovacionConOferta(int planId, PlanDTO plan, string usuario)
        {
            if (!PuedeActivarOferta(out string motivoBloqueo))
            {
                MessageBox.Show(this, motivoBloqueo, "Oferta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!TryObtenerDatosOferta(out decimal valorRef, out decimal pct, out decimal desc, out decimal total, out string motivo))
            {
                MessageBox.Show(this, "Seleccione el plan OFERTA e indique los datos.", "Oferta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (total < 0)
            {
                MessageBox.Show(this, "El total a pagar no puede ser negativo.", "Oferta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show(this, "Indique el motivo / asunto de la oferta.", "Oferta",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtMotivo.Focus();
                return false;
            }

            if (valorRef <= 0 && total == 0)
            {
                pct = 100;
                desc = 0;
            }
            else if (valorRef <= 0 && total > 0)
            {
                valorRef = total;
                pct = 0;
                desc = 0;
            }

            string concepto =
                total == 0
                    ? $"Renovación OFERTA | CORTESÍA {pct:0.##}% | {motivo}"
                    : $"Renovación OFERTA | {pct:0.##}% (-RD${desc:0.00}) | Total RD${total:0.00} | {motivo}";

            if (!AsegurarCajaAbierta(usuario))
                return false;

            // Cortesía RD$0: sin diálogo; monto > 0 pide Efectivo/Transferencia.
            if (!TryCobrarRenovacionConMetodo(total, out string metodoPago))
                return false;

            btnConfirmar.Enabled = false;
            btnCancelar.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                var result = MembresiaCommandService.RenovarMembresia(
                    _clienteId,
                    planId,
                    total,
                    concepto,
                    usuario,
                    metodoPago);

                if (!result.Success)
                {
                    MessageBox.Show(this, result.Message, "Renovación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (result.Payload is RenovacionOperacionResult opRen)
                    IniciarPostRenovacionEnSegundoPlano(
                        opRen, plan, planId, total, metodoPago, valorRef, desc, pct, motivo);

                RenovacionCompletada = true;
                DialogResult = DialogResult.OK;
                Close();
                return true;
            }
            finally
            {
                if (!IsDisposed)
                {
                    Cursor = Cursors.Default;
                    btnConfirmar.Enabled = true;
                    btnCancelar.Enabled = true;
                }
            }
        }
    }
}
