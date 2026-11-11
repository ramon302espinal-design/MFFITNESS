using System;
using System.Globalization;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using BLL.Models;
using CORE;
using DTO;
using UI.Facturas;

namespace UI.DISEÑO
{
    public partial class FrmRenovarMembresia
    {
        private bool _sincronizandoFinanciamiento;

        private void chkFinanciamiento_CheckedChanged(object? sender, EventArgs e)
        {
            if (_sincronizandoFinanciamiento)
                return;

            if (chkFinanciamiento.Checked)
            {
                if (EsPlanOfertaSeleccionado())
                {
                    MessageBox.Show(
                        this,
                        "El plan OFERTA no admite financiamiento. Elija otro plan o quite el financiamiento.",
                        "Financiamiento",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    _sincronizandoFinanciamiento = true;
                    chkFinanciamiento.Checked = false;
                    _sincronizandoFinanciamiento = false;
                    return;
                }

                if (cmbPlan.SelectedValue == null || cmbPlan.SelectedIndex < 0)
                {
                    MessageBox.Show(
                        this,
                        "Seleccione un plan antes de activar el financiamiento.",
                        "Financiamiento",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    _sincronizandoFinanciamiento = true;
                    chkFinanciamiento.Checked = false;
                    _sincronizandoFinanciamiento = false;
                    return;
                }

                if (_deudaBLL.TieneDeudasActivas(_clienteId))
                {
                    MessageBox.Show(
                        this,
                        "No se puede financiar: el cliente tiene deudas pendientes.\n" +
                        "Saldar o abonar la deuda antes de renovar a crédito.",
                        "Financiamiento no disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _sincronizandoFinanciamiento = true;
                    chkFinanciamiento.Checked = false;
                    _sincronizandoFinanciamiento = false;
                    return;
                }
            }

            pnlFinanciamiento.Visible = chkFinanciamiento.Checked;
            dtpFechaLimitePago.Enabled = chkFinanciamiento.Checked;

            if (chkFinanciamiento.Checked)
            {
                if (pnlOferta.Visible)
                    ResetOfertaCampos();

                txtPagoInicial.Text = "0";
                dtpFechaLimitePago.Value = DateTime.Today.AddDays(30);
                CalcularSaldoFinanciamiento();
            }
            else
            {
                lblSaldoValor.Text = "$0.00";
            }
        }

        private void txtPagoInicial_TextChanged(object? sender, EventArgs e)
        {
            CalcularSaldoFinanciamiento();
        }

        private void CalcularSaldoFinanciamiento()
        {
            try
            {
                if (!chkFinanciamiento.Checked)
                    return;

                if (cmbPlan.SelectedValue == null)
                {
                    lblSaldoValor.Text = "$0.00";
                    return;
                }

                int planId = Convert.ToInt32(cmbPlan.SelectedValue);
                var plan = Planes.ObtenerPlan(planId);
                if (plan == null)
                {
                    lblSaldoValor.Text = "$0.00";
                    return;
                }

                decimal pagoInicial = TryParseMonto(txtPagoInicial.Text);
                if (pagoInicial < 0)
                    pagoInicial = 0;

                decimal saldo = plan.Precio - pagoInicial;
                if (saldo < 0)
                    saldo = 0;

                lblSaldoValor.Text = "$" + saldo.ToString("N2", CultureInfo.CurrentCulture);

                // Igual que Pagos: si el inicial cubre el plan, sale del modo financiamiento.
                if (saldo <= 0 && chkFinanciamiento.Checked && !_sincronizandoFinanciamiento)
                {
                    _sincronizandoFinanciamiento = true;
                    chkFinanciamiento.Checked = false;
                    _sincronizandoFinanciamiento = false;
                    pnlFinanciamiento.Visible = false;
                    lblSaldoValor.Text = "$0.00";
                }
            }
            catch
            {
                lblSaldoValor.Text = "$0.00";
            }
        }

        private void ResetFinanciamientoCampos()
        {
            _sincronizandoFinanciamiento = true;
            chkFinanciamiento.Checked = false;
            _sincronizandoFinanciamiento = false;
            pnlFinanciamiento.Visible = false;
            txtPagoInicial.Text = "0";
            lblSaldoValor.Text = "$0.00";
            dtpFechaLimitePago.Enabled = false;
            dtpFechaLimitePago.Value = DateTime.Today.AddDays(30);
        }

        private bool ConfirmarRenovacionFinanciada(int planId, PlanDTO plan, string usuario)
        {
            decimal pagoInicial = TryParseMonto(txtPagoInicial.Text);
            if (pagoInicial < 0 || pagoInicial > plan.Precio)
            {
                MessageBox.Show(this, "Pago inicial inválido.", "Financiamiento",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            decimal saldo = Math.Round(plan.Precio - pagoInicial, 2, MidpointRounding.AwayFromZero);
            if (saldo <= 0)
            {
                MessageBox.Show(
                    this,
                    "Para financiamiento el pago inicial debe ser menor al precio del plan.",
                    "Financiamiento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            DateTime fechaLimite = dtpFechaLimitePago.Value.Date;
            if (fechaLimite < DateTime.Today)
            {
                MessageBox.Show(
                    this,
                    "La fecha límite de pago no puede ser anterior a hoy.",
                    "Financiamiento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            if (!AsegurarCajaAbierta(usuario))
                return false;

            if (!TryCobrarRenovacionConMetodo(pagoInicial, out string metodoPago))
                return false;

            btnConfirmar.Enabled = false;
            btnCancelar.Enabled = false;
            Cursor = Cursors.WaitCursor;

            var result = MembresiaCommandService.RenovarMembresiaFinanciada(
                _clienteId,
                planId,
                pagoInicial,
                metodoPago,
                fechaLimite,
                usuario);

            if (!result.Success)
            {
                MessageBox.Show(this, result.Message, "Renovación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (result.Payload is RenovacionOperacionResult opRen)
            {
                IniciarPostRenovacionFinanciada(opRen, plan, planId, pagoInicial, saldo, metodoPago);
            }

            MessageBox.Show(
                this,
                $"Renovación financiada registrada correctamente.\n\n" +
                $"Plan: {plan.Nombre}\n" +
                $"Pago inicial: ${pagoInicial:N2}\n" +
                $"Saldo pendiente: ${saldo:N2}\n" +
                $"Cliente renovado con plan activo.",
                "Financiamiento Exitoso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            RenovacionCompletada = true;
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        private void IniciarPostRenovacionFinanciada(
            RenovacionOperacionResult opRen,
            PlanDTO plan,
            int planId,
            decimal pagoInicial,
            decimal saldo,
            string metodoPago)
        {
            // WhatsApp financiado lo dispara el BLL. Aquí solo PDF local (igual que Pagos).
            int pagoIdBg = opRen.PagoId;
            int membresiaIdBg = opRen.MembresiaId;
            int cajaMovIdBg = opRen.CajaMovimientoId;
            DateTime finBg = opRen.FechaFinMembresia == default
                ? MembresiaHelper.CalcularFechaVencimiento(DateTime.Now)
                : opRen.FechaFinMembresia;
            DateTime pagoBg = opRen.FechaPago == default ? DateTime.Now : opRen.FechaPago;
            string planNombreBg = plan.Nombre ?? "PLAN";
            int clienteBg = _clienteId;
            string metodoBg = string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago.Trim();
            decimal precioListaBg = plan.Precio;
            decimal saldoBg = saldo;
            string? notaFin = $"Renovación financiada. Saldo pendiente: RD${saldo:N2}. Vence el {finBg:dd/MM/yyyy}.";

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    FacturaMembresiaPdfService.GenerarDesdeOperacion(
                        owner: null,
                        clienteBg,
                        planNombreBg,
                        pagoInicial,
                        finBg,
                        metodoBg,
                        new MembresiaOperacionResult
                        {
                            MembresiaId = membresiaIdBg,
                            PagoId = pagoIdBg > 0 ? pagoIdBg : membresiaIdBg,
                            CajaMovimientoId = cajaMovIdBg,
                            PlanId = planId,
                            PlanNombre = planNombreBg,
                            FechaPago = pagoBg,
                            FechaFinMembresia = finBg
                        },
                        notaExtra: notaFin,
                        abrirPdf: false,
                        precioLista: precioListaBg,
                        forzarRegenerar: true,
                        esFinanciada: true,
                        saldoPendiente: saldoBg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PDF renovación financiada] {ex.Message}");
                }
            });
        }

        private static decimal TryParseMonto(string? texto)
        {
            texto = (texto ?? string.Empty).Trim().Replace("RD$", "").Replace("$", "").Trim();
            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal v))
                return v;
            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                return v;
            return 0m;
        }
    }
}
