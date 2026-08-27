using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using BLL.Models;
using DTO;

namespace UI.DISEÑO
{
    public partial class FrmPagos
    {
        private bool _sincronizandoOferta;

        private bool EsPlanOfertaSeleccionado()
        {
            if (cmbMembresia.SelectedItem is DataRowView row)
                return PlanNombres.EsOferta(Convert.ToString(row["Nombre"]));
            return PlanNombres.EsOferta(cmbMembresia.Text);
        }

        private bool EsPlanParcialSeleccionado()
        {
            if (cmbMembresia.SelectedItem is DataRowView row)
                return PlanNombres.EsParcial(Convert.ToString(row["Nombre"]));
            return PlanNombres.EsParcial(cmbMembresia.Text);
        }

        /// <summary>Oferta no aplica a ACTIVO, CONGELADO ni con deudas activas.</summary>
        private bool PuedeActivarOferta(int clienteId, out string motivo)
        {
            motivo = string.Empty;
            if (clienteId <= 0)
            {
                motivo = "Seleccione un cliente antes de usar el plan OFERTA.";
                return false;
            }

            string estado = clienteBLL.ObtenerEstadoMembresia(clienteId);
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

            if (deudaBLL.TieneDeudasActivas(clienteId))
            {
                motivo = "No se puede aplicar oferta: el cliente tiene deudas pendientes.";
                return false;
            }

            return true;
        }

        /// <summary>Muestra pnlOferta al elegir el plan OFERTA en el combo.</summary>
        private void ActualizarPanelOfertaPorPlan()
        {
            if (_sincronizandoOferta)
                return;

            bool esOferta = EsPlanOfertaSeleccionado();
            if (!esOferta)
            {
                if (pnlOferta.Visible)
                    ResetOfertaCampos();
                RestaurarEtiquetasOferta();
                return;
            }

            if (!TryObtenerClienteSeleccionado(out int clienteId, out _))
            {
                MessageBox.Show(
                    "Seleccione un cliente antes de elegir el plan OFERTA.",
                    "Oferta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                _sincronizandoOferta = true;
                cmbMembresia.SelectedIndex = -1;
                _sincronizandoOferta = false;
                ResetOfertaCampos();
                return;
            }

            if (!PuedeActivarOferta(clienteId, out string motivo))
            {
                MessageBox.Show(motivo, "Oferta no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _sincronizandoOferta = true;
                cmbMembresia.SelectedIndex = -1;
                _sincronizandoOferta = false;
                ResetOfertaCampos();
                return;
            }

            if (chkFinanciamiento.Checked)
                chkFinanciamiento.Checked = false;

            AplicarEtiquetasOferta();
            pnlOferta.Visible = true;

            _sincronizandoOferta = true;
            if (string.IsNullOrWhiteSpace(txtDescuentoPorcental.Text))
                txtDescuentoPorcental.Text = "100";
            if (string.IsNullOrWhiteSpace(txtDescuentoMonto.Text))
                txtDescuentoMonto.Text = "0.00";
            _sincronizandoOferta = false;

            RecalcularOfertaDesdeReferencia();
        }

        private void AplicarEtiquetasOferta()
        {
            lblOfertaPct.Text = "Cortesía %:";
            lblOfertaMonto.Text = "Valor ref. RD$:";
        }

        private void RestaurarEtiquetasOferta()
        {
            lblOfertaPct.Text = "Descuento %:";
            lblOfertaMonto.Text = "Descuento RD$:";
        }

        private void cmbCliente_Oferta_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!EsPlanOfertaSeleccionado())
                return;

            string motivo = string.Empty;
            if (!TryObtenerClienteSeleccionado(out int clienteId, out _)
                || !PuedeActivarOferta(clienteId, out motivo))
            {
                if (!string.IsNullOrWhiteSpace(motivo))
                    MessageBox.Show(motivo, "Oferta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _sincronizandoOferta = true;
                cmbMembresia.SelectedIndex = -1;
                _sincronizandoOferta = false;
                ResetOfertaCampos();
            }
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

        /// <summary>
        /// Valor ref. RD$ + Cortesía % → total a pagar (puede ser RD$ 0).
        /// </summary>
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

            ActualizarTotalPagarLabel(total);
            _sincronizandoOferta = true;
            txtMonto.Text = total.ToString("0.00", CultureInfo.CurrentCulture);
            _sincronizandoOferta = false;
        }

        /// <summary>Compat Designer: recalcula si el plan OFERTA está activo.</summary>
        private void RecalcularOfertaDesdePorcentaje() => RecalcularOfertaDesdeReferencia();

        private void ActualizarTotalPagarLabel(decimal total) =>
            lblTotalPagar.Text = "RD$ " + total.ToString("N2", CultureInfo.CurrentCulture);

        private void ResetOfertaCampos()
        {
            _sincronizandoOferta = true;
            txtDescuentoPorcental.Text = "100";
            txtDescuentoMonto.Text = "0.00";
            txtMotivo.Clear();
            ActualizarTotalPagarLabel(0);
            _sincronizandoOferta = false;
            pnlOferta.Visible = false;
            RestaurarEtiquetasOferta();
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

            if (decimal.TryParse(txtMonto.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal montoUi)
                && montoUi >= 0)
                totalPagar = Math.Round(montoUi, 2, MidpointRounding.AwayFromZero);

            return true;
        }

        private void CobrarMembresiaConOferta(
            int clienteId,
            int planId,
            PlanDTO plan,
            DateTime fin,
            string usuario)
        {
            if (!PuedeActivarOferta(clienteId, out string motivoBloqueo))
            {
                MessageBox.Show(motivoBloqueo, "Oferta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryObtenerDatosOferta(out decimal valorRef, out decimal pct, out decimal desc, out decimal total, out string motivo))
            {
                MessageBox.Show("Seleccione el plan OFERTA e indique los datos.", "Oferta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (total < 0)
            {
                MessageBox.Show("El total a pagar no puede ser negativo.", "Oferta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show("Indique el motivo / asunto de la oferta.", "Oferta",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtMotivo.Focus();
                return;
            }

            // Si no hay valor ref., documentar cortesía total sobre el cobro real.
            if (valorRef <= 0 && total == 0)
            {
                valorRef = 0;
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
                    ? $"Membresía OFERTA | CORTESÍA {pct:0.##}% | {motivo}"
                    : $"Membresía OFERTA | {pct:0.##}% (-RD${desc:0.00}) | Total RD${total:0.00} | {motivo}";
            string metodoPago = "Efectivo";

            var result = MembresiaCommandService.PagarMembresia(
                clienteId,
                planId,
                total,
                metodoPago,
                concepto,
                fin,
                usuario);

            if (!result.Success)
            {
                MessageBox.Show(result.Message);
                return;
            }

            LimpiarCampos();
            MessageBox.Show(
                $"Membresía OFERTA registrada.\n\n" +
                $"Valor referencia: RD$ {valorRef:N2}\n" +
                $"Cortesía: {pct:0.##}% (RD$ {desc:N2})\n" +
                $"Total cobrado: RD$ {total:N2}\n" +
                $"Asunto: {motivo}",
                "Oferta aplicada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ProgramarRefrescoTrasPago();

            if (result.Payload is MembresiaOperacionResult opPago)
            {
                string nota =
                    $"Asunto: {motivo}. Cortesía {pct:0.##}% (-RD${desc:N2}). " +
                    $"Valor ref. RD${valorRef:N2} → total RD${total:N2}.";

                IniciarPostPagoEnSegundoPlano(
                    clienteId,
                    planId,
                    plan.Nombre ?? PlanNombres.Oferta,
                    total,
                    fin,
                    metodoPago,
                    opPago,
                    notaExtra: nota,
                    enviarWhatsAppFactura: total > 0,
                    precioLista: valorRef > 0 ? valorRef : null,
                    descuentoMonto: desc > 0 ? desc : null,
                    descuentoPorcentaje: pct,
                    asuntoOferta: motivo,
                    enviarWhatsAppOferta: true);
            }
        }
    }
}
