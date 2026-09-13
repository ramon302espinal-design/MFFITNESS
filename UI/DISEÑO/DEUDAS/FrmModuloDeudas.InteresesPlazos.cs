using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using BLL;
using DL;

namespace UI
{
    /// <summary>
    /// pnlIntereses: interés fijo + plazos (semanal / quincenal / mensual) en tiempo real.
    /// Capital = saldo pendiente. Persistencia vía producto a crédito (Guardar).
    /// </summary>
    public partial class FrmModuloDeudas
    {
        private bool _interesesPlazosCableados;
        private bool _suppressInteresesUi;
        private PrestamoCronogramaDto? _ultimoCronogramaUi;

        /// <summary>Días de gracia tras el vencimiento antes de generar mora.</summary>
        private const int MoraDiasGracia = 3;

        /// <summary>Mora diaria (RD$) a partir del 3.er día de vencido.</summary>
        private const decimal MoraPesosDiarios = 50m;

        private enum FrecuenciaPlazo
        {
            Semanal,
            Quincenal,
            Mensual
        }

        private void CablearInteresesPlazosUi()
        {
            if (_interesesPlazosCableados)
                return;

            _interesesPlazosCableados = true;

            ConfigurarGridPlazos();

            numPlazos.Minimum = 0;
            numPlazos.Maximum = 120;
            numPlazos.DecimalPlaces = 0;
            numPlazos.Increment = 1;

            txtResumenFinanciamiento.ReadOnly = true;
            txtResumenFinanciamiento.TabStop = false;
            txtResumenFinanciamiento.Multiline = true;

            rdSemanal.CheckedChanged -= FrecuenciaPlazo_CheckedChanged;
            rdQuincenal.CheckedChanged -= FrecuenciaPlazo_CheckedChanged;
            rdMensual.CheckedChanged -= FrecuenciaPlazo_CheckedChanged;
            rdSemanal.CheckedChanged += FrecuenciaPlazo_CheckedChanged;
            rdQuincenal.CheckedChanged += FrecuenciaPlazo_CheckedChanged;
            rdMensual.CheckedChanged += FrecuenciaPlazo_CheckedChanged;

            txtInteres.TextChanged -= txtInteres_TextChangedLive;
            txtInteres.TextChanged += txtInteres_TextChangedLive;
            txtInteres.KeyPress -= txtInteres_KeyPress;
            txtInteres.KeyPress += txtInteres_KeyPress;

            numPlazos.ValueChanged -= numPlazos_ValueChangedLive;
            numPlazos.ValueChanged += numPlazos_ValueChangedLive;
            CablearTextoInternoNumPlazos();

            chkMora.CheckedChanged -= chkMora_CheckedChangedLive;
            chkMora.CheckedChanged += chkMora_CheckedChangedLive;

            dtInicioDepago.ValueChanged -= dtInicioDepago_ValueChangedLive;
            dtInicioDepago.ValueChanged += dtInicioDepago_ValueChangedLive;

            _suppressInteresesUi = true;
            try
            {
                if (!rdSemanal.Checked && !rdQuincenal.Checked && !rdMensual.Checked)
                    rdSemanal.Checked = true;
                ActualizarEtiquetaPlazos();
                InicializarDtInicioDePago(forzarDefault: true);
                LimpiarResumenYPlazos();
            }
            finally
            {
                _suppressInteresesUi = false;
            }
        }

        private void txtInteres_TextChangedLive(object? sender, EventArgs e) =>
            RecalcularInteresesYPlazos();

        private void numPlazos_ValueChangedLive(object? sender, EventArgs e) =>
            RecalcularInteresesYPlazos();

        private void chkMora_CheckedChangedLive(object? sender, EventArgs e) =>
            RecalcularInteresesYPlazos();

        private void dtInicioDepago_ValueChangedLive(object? sender, EventArgs e) =>
            RecalcularInteresesYPlazos();

        /// <summary>
        /// Primera cuota = esta fecha. No permite fechas anteriores a hoy.
        /// </summary>
        private void InicializarDtInicioDePago(bool forzarDefault)
        {
            DateTime hoy = DateTime.Today;
            try
            {
                if (dtInicioDepago.MinDate != hoy)
                    dtInicioDepago.MinDate = hoy;
            }
            catch
            {
                // MinDate/Value conflict: ajustar en orden seguro.
                dtInicioDepago.MinDate = DateTimePicker.MinimumDateTime;
                dtInicioDepago.Value = hoy;
                dtInicioDepago.MinDate = hoy;
            }

            if (forzarDefault || dtInicioDepago.Value.Date < hoy)
            {
                DateTime inicioDefault = hoy.AddDays(7);
                if (inicioDefault < dtInicioDepago.MinDate)
                    inicioDefault = dtInicioDepago.MinDate;
                dtInicioDepago.Value = inicioDefault;
            }
        }

        /// <summary>Día en que el cliente debe pagar la 1.ª cuota.</summary>
        private DateTime ObtenerInicioDePagoUi()
        {
            DateTime fecha = dtInicioDepago.Value.Date;
            DateTime hoy = DateTime.Today;
            if (fecha < hoy)
                fecha = hoy;
            return fecha;
        }
        /// <summary>
        /// NumericUpDown solo confirma Value al salir; el TextBox interno actualiza en cada tecla.
        /// </summary>
        private void CablearTextoInternoNumPlazos()
        {
            foreach (Control c in numPlazos.Controls)
            {
                if (c is not TextBox tb)
                    continue;

                tb.TextChanged -= numPlazosTexto_TextChangedLive;
                tb.TextChanged += numPlazosTexto_TextChangedLive;
                tb.KeyUp -= numPlazosTexto_KeyUpLive;
                tb.KeyUp += numPlazosTexto_KeyUpLive;
                break;
            }
        }

        private void numPlazosTexto_TextChangedLive(object? sender, EventArgs e) =>
            RecalcularInteresesYPlazos();

        private void numPlazosTexto_KeyUpLive(object? sender, KeyEventArgs e) =>
            RecalcularInteresesYPlazos();

        /// <summary>Plazos mientras se teclea (antes de que Value se confirme).</summary>
        private int LeerNumeroPlazosUi()
        {
            foreach (Control c in numPlazos.Controls)
            {
                if (c is not TextBox tb)
                    continue;

                string texto = (tb.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(texto))
                    return 0;

                if (int.TryParse(texto, NumberStyles.Integer, CultureInfo.CurrentCulture, out int n)
                    || int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                {
                    if (n < 0) n = 0;
                    if (n > (int)numPlazos.Maximum) n = (int)numPlazos.Maximum;
                    return n;
                }

                return 0;
            }

            return (int)numPlazos.Value;
        }

        /// <summary>
        /// Solo producto a crédito con saldo pendiente usa plazos/interés por ahora.
        /// </summary>
        private void AplicarDisponibilidadInteresesProductoCredito()
        {
            if (!_interesesPlazosCableados)
                return;

            bool producto = EsProductoCreditoSeleccionado();
            decimal capital = ObtenerCapitalAFinanciar();
            bool habilitar = producto && capital > 0m;

            pnlIntereses.Enabled = habilitar;
            if (!habilitar)
            {
                _ultimoCronogramaUi = null;
                if (!producto)
                    ResetearInteresesPlazosUi();
                else
                    LimpiarResumenYPlazos();
            }
            else
            {
                // Actualiza MinDate (hoy) sin pisar la fecha elegida por el usuario.
                _suppressInteresesUi = true;
                try { InicializarDtInicioDePago(forzarDefault: false); }
                finally { _suppressInteresesUi = false; }
                RecalcularInteresesYPlazos();
            }
        }

        private bool TryObtenerCronogramaUi(out PrestamoCronogramaDto cronograma, out string error)
        {
            cronograma = null!;
            error = string.Empty;

            if (!EsProductoCreditoSeleccionado())
            {
                error = "El cronograma de plazos solo aplica a Producto a crédito.";
                return false;
            }

            if (ObtenerCapitalAFinanciar() <= 0m)
            {
                error = "No hay saldo pendiente para financiar con plazos.";
                return false;
            }

            if (_ultimoCronogramaUi == null
                || _ultimoCronogramaUi.NumeroPlazos <= 0
                || _ultimoCronogramaUi.Cuotas.Count == 0)
            {
                error = "Configure frecuencia, interés % y número de plazos hasta ver el cronograma en la grilla.";
                return false;
            }

            cronograma = _ultimoCronogramaUi;
            return true;
        }

        private void ConfigurarGridPlazos()
        {
            dgvPlazos.AllowUserToAddRows = false;
            dgvPlazos.AllowUserToDeleteRows = false;
            dgvPlazos.ReadOnly = true;
            dgvPlazos.MultiSelect = false;
            dgvPlazos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPlazos.RowHeadersVisible = false;
            dgvPlazos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void FrecuenciaPlazo_CheckedChanged(object? sender, EventArgs e)
        {
            if (_suppressInteresesUi)
                return;

            if (sender is RadioButton { Checked: false })
                return;

            ActualizarEtiquetaPlazos();
            RecalcularInteresesYPlazos();
        }

        private void ActualizarEtiquetaPlazos()
        {
            lblPlazos.Text = ObtenerFrecuenciaPlazo() switch
            {
                FrecuenciaPlazo.Quincenal => "PLAZOS QUINCENALES",
                FrecuenciaPlazo.Mensual => "PLAZOS MENSUALES",
                _ => "PLAZOS SEMANALES"
            };
        }

        private FrecuenciaPlazo ObtenerFrecuenciaPlazo()
        {
            if (rdQuincenal.Checked)
                return FrecuenciaPlazo.Quincenal;
            if (rdMensual.Checked)
                return FrecuenciaPlazo.Mensual;
            return FrecuenciaPlazo.Semanal;
        }

        private static string NombreFrecuencia(FrecuenciaPlazo f) => f switch
        {
            FrecuenciaPlazo.Quincenal => "QUINCENAL",
            FrecuenciaPlazo.Mensual => "MENSUAL",
            _ => "SEMANAL"
        };

        private decimal ObtenerCapitalAFinanciar()
        {
            decimal pagoInicio = decimal.TryParse(
                txtPagodeinicio.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal p)
                ? p
                : 0m;
            if (pagoInicio < 0m)
                pagoInicio = 0m;

            decimal saldo = _precioPlan - pagoInicio;
            return saldo < 0m ? 0m : decimal.Round(saldo, 2, MidpointRounding.AwayFromZero);
        }

        private bool TryLeerInteresPorcentaje(out decimal interesPct)
        {
            interesPct = 0m;
            string texto = (txtInteres.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(texto))
                return true;

            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out interesPct)
                && !decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out interesPct))
                return false;

            if (interesPct < 0m)
                interesPct = 0m;

            return true;
        }

        private void RecalcularInteresesYPlazos()
        {
            if (_suppressInteresesUi || !_interesesPlazosCableados)
                return;

            // Panel deshabilitado: no pintar (evita basura visual); al habilitar se recalcula.
            if (!pnlIntereses.Enabled)
                return;

            ActualizarEtiquetaPlazos();

            decimal capital = ObtenerCapitalAFinanciar();
            int plazos = LeerNumeroPlazosUi();

            if (capital <= 0m)
            {
                _ultimoCronogramaUi = null;
                LimpiarResumenYPlazos();
                txtResumenFinanciamiento.Text =
                    "INTERES TOTAL:\r\n\r\nTOTAL CON INTERES:\r\n\r\nCUOTA BASE:\r\n\r\n" +
                    "(Agregue productos y deje saldo pendiente para calcular plazos.)";
                return;
            }

            if (plazos <= 0)
            {
                _ultimoCronogramaUi = null;
                if (dgvPlazos.Rows.Count > 0)
                    dgvPlazos.Rows.Clear();

                string interesTxt = TryLeerInteresPorcentaje(out decimal pctPreview)
                    ? $"{pctPreview:N2}%"
                    : "(interés inválido)";

                txtResumenFinanciamiento.Text =
                    $"CAPITAL A FINANCIAR: ${capital:N2}\r\n" +
                    $"INTERÉS: {interesTxt}\r\n\r\n" +
                    "TOTAL CON INTERES:\r\n\r\nCUOTA BASE:\r\n\r\n" +
                    "(Indique el número de plazos para ver el cronograma.)";
                return;
            }

            if (!TryLeerInteresPorcentaje(out decimal interesPct))
            {
                _ultimoCronogramaUi = null;
                if (dgvPlazos.Rows.Count > 0)
                    dgvPlazos.Rows.Clear();

                txtResumenFinanciamiento.Text =
                    $"CAPITAL A FINANCIAR: ${capital:N2}\r\n" +
                    "INTERES TOTAL: (interés inválido)\r\n\r\nTOTAL CON INTERES:\r\n\r\nCUOTA BASE:";
                return;
            }

            // Interés fijo sobre el capital (saldo pendiente). Vacío = 0%.
            decimal interesTotal = decimal.Round(capital * (interesPct / 100m), 2, MidpointRounding.AwayFromZero);
            decimal totalConInteres = decimal.Round(capital + interesTotal, 2, MidpointRounding.AwayFromZero);
            decimal cuotaBase = decimal.Round(totalConInteres / plazos, 2, MidpointRounding.AwayFromZero);

            decimal capitalCuota = decimal.Round(capital / plazos, 2, MidpointRounding.AwayFromZero);
            decimal interesCuota = decimal.Round(interesTotal / plazos, 2, MidpointRounding.AwayFromZero);

            FrecuenciaPlazo frecuencia = ObtenerFrecuenciaPlazo();
            DateTime inicio = ObtenerInicioDePagoUi();
            DateTime hoy = DateTime.Today;
            bool moraActiva = chkMora.Checked;
            var cuotas = new List<PrestamoCuotaDetalleRow>(plazos);

            dgvPlazos.SuspendLayout();
            try
            {
                dgvPlazos.Rows.Clear();

                decimal capitalAcum = 0m;
                decimal interesAcum = 0m;
                decimal moraTotalPotencial = 0m;

                for (int i = 1; i <= plazos; i++)
                {
                    decimal cap;
                    decimal inte;
                    if (i == plazos)
                    {
                        cap = decimal.Round(capital - capitalAcum, 2, MidpointRounding.AwayFromZero);
                        inte = decimal.Round(interesTotal - interesAcum, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        cap = capitalCuota;
                        inte = interesCuota;
                        capitalAcum += cap;
                        interesAcum += inte;
                    }

                    DateTime vencimiento = CalcularFechaVencimientoCuota(inicio, frecuencia, i);
                    decimal mora = CalcularMoraPotencial(vencimiento, moraActiva, hoy);
                    moraTotalPotencial += mora;
                    decimal totalFila = decimal.Round(cap + inte + mora, 2, MidpointRounding.AwayFromZero);

                    cuotas.Add(new PrestamoCuotaDetalleRow
                    {
                        NumeroCuota = i,
                        FechaVencimiento = vencimiento.Date,
                        Capital = cap,
                        Interes = inte,
                        MoraPotencial = mora,
                        Total = totalFila
                    });

                    dgvPlazos.Rows.Add(
                        i.ToString(CultureInfo.InvariantCulture),
                        vencimiento.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture),
                        cap.ToString("N2", CultureInfo.CurrentCulture),
                        inte.ToString("N2", CultureInfo.CurrentCulture),
                        mora.ToString("N2", CultureInfo.CurrentCulture),
                        totalFila.ToString("N2", CultureInfo.CurrentCulture));
                }

                _ultimoCronogramaUi = new PrestamoCronogramaDto
                {
                    Frecuencia = NombreFrecuencia(frecuencia),
                    InteresPorcentaje = interesPct,
                    NumeroPlazos = plazos,
                    InteresTotal = interesTotal,
                    TotalConInteres = totalConInteres,
                    CuotaBase = cuotaBase,
                    ActivarMora = moraActiva,
                    Cuotas = cuotas
                };

                string lineaMora = moraActiva
                    ? $"MORA: activa · ${MoraPesosDiarios:N0}/día desde el día {MoraDiasGracia} de vencido\r\n" +
                      $"MORA ACUMULADA HOY: ${moraTotalPotencial:N2}\r\n\r\n"
                    : "MORA: desactivada\r\n\r\n";

                DateTime ultimaCuota = cuotas[^1].FechaVencimiento.Date;

                txtResumenFinanciamiento.Text =
                    $"CAPITAL: ${capital:N2}\r\n\r\n" +
                    $"INICIO DE PAGO: {inicio:dd/MM/yyyy}\r\n" +
                    $"(1.ª cuota en esa fecha)\r\n\r\n" +
                    $"INTERES TOTAL: ${interesTotal:N2}\r\n" +
                    $"({interesPct:N2}% fijo · {NombreFrecuencia(frecuencia)} · {plazos} plazo(s))\r\n\r\n" +
                    $"TOTAL CON INTERES: ${totalConInteres:N2}\r\n" +
                    $"(Capital ${capital:N2} + interés)\r\n\r\n" +
                    $"CUOTA BASE: ${cuotaBase:N2}\r\n" +
                    $"ÚLTIMA CUOTA: {ultimaCuota:dd/MM/yyyy}\r\n\r\n" +
                    lineaMora;
            }
            finally
            {
                dgvPlazos.ResumeLayout(true);
                dgvPlazos.Refresh();
                txtResumenFinanciamiento.Refresh();
            }
        }

        /// <summary>
        /// Mora potencial: 0 en los primeros 2 días vencidos; desde el día 3 → RD$50 por cada día vencido
        /// (día 3 = $50, día 4 = $100, …). Aplica a semanal, quincenal y mensual.
        /// </summary>
        private static decimal CalcularMoraPotencial(DateTime fechaVencimiento, bool activarMora, DateTime hoy)
        {
            if (!activarMora)
                return 0m;

            int diasVencido = (hoy.Date - fechaVencimiento.Date).Days;
            if (diasVencido < MoraDiasGracia)
                return 0m;

            // Día 3 vencido → 1 día de mora; día 4 → 2 días…
            int diasConMora = diasVencido - (MoraDiasGracia - 1);
            return diasConMora * MoraPesosDiarios;
        }

        /// <summary>
        /// <paramref name="inicio"/> = fecha de la 1.ª cuota (dtInicioDepago).
        /// Cuota N = inicio + (N-1) períodos.
        /// </summary>
        private static DateTime CalcularFechaVencimientoCuota(
            DateTime inicio,
            FrecuenciaPlazo frecuencia,
            int numeroCuota)
        {
            int offset = Math.Max(0, numeroCuota - 1);
            return frecuencia switch
            {
                FrecuenciaPlazo.Quincenal => inicio.AddDays(15 * offset),
                FrecuenciaPlazo.Mensual => inicio.AddMonths(offset),
                _ => inicio.AddDays(7 * offset)
            };
        }

        private void LimpiarResumenYPlazos()
        {
            _ultimoCronogramaUi = null;
            if (dgvPlazos.Rows.Count > 0)
                dgvPlazos.Rows.Clear();

            txtResumenFinanciamiento.Text =
                "INTERES TOTAL:\r\n\r\nTOTAL CON INTERES:\r\n\r\nCUOTA BASE:";
        }

        private void ResetearInteresesPlazosUi()
        {
            if (!_interesesPlazosCableados)
                return;

            _suppressInteresesUi = true;
            try
            {
                txtInteres.Clear();
                numPlazos.Value = 0;
                chkMora.Checked = false;
                rdSemanal.Checked = true;
                ActualizarEtiquetaPlazos();
                InicializarDtInicioDePago(forzarDefault: true);
                LimpiarResumenYPlazos();
            }
            finally
            {
                _suppressInteresesUi = false;
            }
        }

        private void txtInteres_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            char sep = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (e.KeyChar == '.' || e.KeyChar == ',')
            {
                string texto = txtInteres.Text ?? string.Empty;
                if (texto.Contains(sep) || texto.Contains('.') || texto.Contains(','))
                    e.Handled = true;
                return;
            }

            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;
        }
    }
}
