using BLL;
using BLL.Models.Crm;
using BLL.Services.Crm;
using CORE;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using UI.Helpers;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmReportes : Form, ICrmPeriodRefreshable
    {
        private static readonly string[] CategoriasReporte =
        {
            "CAJA", "MEMBRESIA", "VENTAS", "SUPLEMENTO", "GASTO"
        };

        private static readonly string[] ColumnasMoneda =
        {
            "Monto", "Total", "Subtotal", "Precio"
        };

        private readonly ReporteBLL reporteBLL = new();
        private DataTable datosFuente = new();
        private DataTable datosActuales = new();
        private bool estaExportando;
        private bool _cargandoUi;
        private bool _reporteInicialCargado;

        /// <summary>Rango inclusive (desde/hasta). Lo alimenta panelHeader del CRM o default 30 días.</summary>
        private DateTime _desde = DateTime.Today.AddDays(-29);
        private DateTime _hasta = DateTime.Today;

        public FrmReportes()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            dgvMostrarDatos.CellFormatting += DgvMostrarDatos_CellFormatting;
        }

        /// <summary>
        /// Host CRM: sin MinimumSize/Maximized; Dock Fill bajo panelHeader del shell.
        /// </summary>
        public void PrepararParaEmbebido()
        {
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            MinimumSize = Size.Empty;
            MaximumSize = Size.Empty;
            AutoScaleMode = AutoScaleMode.Dpi;
            Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Cableado al período de FrmCRMFinanciero.panelHeader (cmbPeriodo / fechas).
        /// </summary>
        public void Recargar(
            ProfitPeriodKind period,
            DateTime? customFrom = null,
            DateTime? customToExclusive = null)
        {
            if (IsDisposed || Disposing)
                return;

            ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(
                period, DateTime.Today, customFrom, customToExclusive);

            _desde = (range.From ?? DateTime.Today.AddDays(-29)).Date;
            _hasta = range.ToExclusive.HasValue
                ? range.ToExclusive.Value.Date.AddDays(-1)
                : DateTime.Today;

            if (_hasta < _desde)
                _hasta = _desde;

            if (!_cargandoUi && IsHandleCreated)
                CargarReporte();
        }

        private void AsegurarLayoutGrid()
        {
            if (IsDisposed || Disposing)
                return;

            if (!TopLevel)
            {
                WindowState = FormWindowState.Normal;
                MinimumSize = Size.Empty;
            }

            SuspendLayout();
            try
            {
                panelHeader.Dock = DockStyle.Top;
                panelPie.Dock = DockStyle.Bottom;
                dgvMostrarDatos.Dock = DockStyle.Fill;
                dgvMostrarDatos.BringToFront();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void CalcularTotal()
        {
            if (lblTotal == null)
                return;

            if (datosActuales == null || datosActuales.Rows.Count == 0)
            {
                lblTotal.Text = "TOTAL: " + 0m.ToString("C");
                return;
            }

            decimal total = ObtenerMontoTotal();
            string etiqueta = ObtenerEtiquetaTotal(ObtenerTipoSeleccionado());
            lblTotal.Text = $"{etiqueta}: {total.ToString("C")}";
        }

        private static string ObtenerEtiquetaTotal(string? tipo) => tipo switch
        {
            "GASTO" or "GASTOS" => "TOTAL GASTOS",
            "VENTAS" => "TOTAL VENTAS",
            "SUPLEMENTO" or "SUPLEMENTOS" => "TOTAL SUPLEMENTO",
            "MEMBRESIA" or "MEMBRESÍA" => "TOTAL MEMBRESÍA",
            "CAJA" => "TOTAL CAJA",
            _ => "TOTAL"
        };

        private decimal ObtenerMontoTotal()
        {
            if (datosActuales == null)
                return 0m;

            decimal total = 0m;
            foreach (DataRow row in datosActuales.Rows)
            {
                if (TryLeerMonto(row, "Monto", out decimal monto)
                    || TryLeerMonto(row, "Total", out monto)
                    || TryLeerMonto(row, "Subtotal", out monto))
                {
                    total += monto;
                }
            }

            return total;
        }

        private static bool TryLeerMonto(DataRow row, string columna, out decimal monto)
        {
            monto = 0m;
            if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return false;

            monto = Convert.ToDecimal(row[columna]);
            return true;
        }

        private string? ObtenerTipoSeleccionado()
            => cmbReporte.SelectedItem?.ToString()?.Trim().ToUpperInvariant();

        private void MostrarTablaVacia(string? totalTexto = null)
        {
            datosFuente = new DataTable();
            datosActuales = datosFuente;
            dgvMostrarDatos.DataSource = datosActuales;
            if (totalTexto != null)
                lblTotal.Text = totalTexto;
            else
                CalcularTotal();
        }

        /// <summary>
        /// UI → ReporteBLL → ReporteDAL → DBHelper/AppConfig (MF CYBER DB / DEV).
        /// </summary>
        private void CargarReporte()
        {
            if (_cargandoUi || IsDisposed || Disposing)
                return;

            try
            {
                string? tipo = cmbReporte.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(tipo))
                {
                    MostrarTablaVacia();
                    return;
                }

                if (_desde.Date > _hasta.Date)
                {
                    MostrarTablaVacia("TOTAL: —");
                    return;
                }

                datosFuente = reporteBLL.ObtenerReporte(tipo, _desde.Date, _hasta.Date)
                    ?? new DataTable();
                AplicarBusquedaInteligente();
                FormatearColumnasReporte(tipo);
                CalcularTotal();
                AsegurarLayoutGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al obtener datos: " + ex.Message
                    + "\n\nBD: " + AppConfig.DatabaseName
                    + " (" + AppConfig.EnvironmentName + ")",
                    "Reportes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void FormatearColumnasReporte(string tipo)
        {
            if (dgvMostrarDatos.Columns.Count == 0)
                return;

            dgvMostrarDatos.AutoGenerateColumns = true;

            if (dgvMostrarDatos.Columns.Contains("Fecha"))
                dgvMostrarDatos.Columns["Fecha"].DefaultCellStyle.Format = FechaHoraFormats.FechaHora;

            if (dgvMostrarDatos.Columns.Contains("FechaPago"))
                dgvMostrarDatos.Columns["FechaPago"].DefaultCellStyle.Format = FechaHoraFormats.FechaHora;

            foreach (string colMoneda in ColumnasMoneda)
            {
                if (!dgvMostrarDatos.Columns.Contains(colMoneda))
                    continue;

                dgvMostrarDatos.Columns[colMoneda].DefaultCellStyle.Format = "N2";
                dgvMostrarDatos.Columns[colMoneda].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            string clave = tipo.Trim().ToUpperInvariant();
            switch (clave)
            {
                case "CAJA":
                    SetHeaderSiExiste("Tipo", "Tipo");
                    SetHeaderSiExiste("Concepto", "Concepto");
                    SetHeaderSiExiste("Monto", "Monto");
                    break;
                case "MEMBRESIA":
                case "MEMBRESÍA":
                    SetHeaderSiExiste("Miembro", "Miembro");
                    SetHeaderSiExiste("Plan", "Plan");
                    SetHeaderSiExiste("Tipo", "Movimiento");
                    break;
                case "VENTAS":
                case "SUPLEMENTO":
                case "SUPLEMENTOS":
                    SetHeaderSiExiste("VentaId", "Venta #");
                    SetHeaderSiExiste("Producto", "Producto");
                    SetHeaderSiExiste("Categoria", "Categoría");
                    SetHeaderSiExiste("Monto", "Subtotal");
                    break;
                case "GASTO":
                case "GASTOS":
                    SetHeaderSiExiste("Concepto", "Concepto");
                    SetHeaderSiExiste("Monto", "Monto");
                    break;
            }
        }

        private void SetHeaderSiExiste(string columna, string header)
        {
            if (dgvMostrarDatos.Columns.Contains(columna))
                dgvMostrarDatos.Columns[columna].HeaderText = header;
        }

        private void DgvMostrarDatos_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0
                || !dgvMostrarDatos.Columns.Contains("Tipo"))
                return;

            DataGridViewRow row = dgvMostrarDatos.Rows[e.RowIndex];
            string tipo = row.Cells["Tipo"].Value?.ToString()?.Trim() ?? string.Empty;
            if (!tipo.Equals("REVERSO", StringComparison.OrdinalIgnoreCase))
                return;

            string columna = dgvMostrarDatos.Columns[e.ColumnIndex].Name;
            if (!columna.Equals("Tipo", StringComparison.OrdinalIgnoreCase))
                return;

            e.Value = "REVERSO";
            e.FormattingApplied = true;

            DataGridViewCellStyle estilo = e.CellStyle ?? new DataGridViewCellStyle();
            estilo.BackColor = Color.Firebrick;
            estilo.ForeColor = Color.White;
            estilo.SelectionBackColor = Color.DarkRed;
            estilo.SelectionForeColor = Color.White;
            estilo.Font = new Font(dgvMostrarDatos.Font, FontStyle.Bold);
            estilo.Alignment = DataGridViewContentAlignment.MiddleCenter;
            e.CellStyle = estilo;
        }

        private void cmbReporte_SelectedIndexChanged(object? sender, EventArgs e)
            => CargarReporte();

        private void txtBusca_TextChanged(object? sender, EventArgs e)
            => AplicarBusquedaInteligente();

        private void AplicarBusquedaInteligente()
        {
            if (datosFuente == null)
                return;

            string consulta = NormalizarTexto(txtBusca?.Text);
            string[] terminos = consulta.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (terminos.Length == 0)
            {
                datosActuales = datosFuente;
            }
            else
            {
                DataTable filtrados = datosFuente.Clone();

                foreach (DataRow row in datosFuente.Rows)
                {
                    string contenido = ConstruirTextoBusqueda(row);
                    bool coincide = true;

                    foreach (string termino in terminos)
                    {
                        if (!contenido.Contains(termino, StringComparison.Ordinal))
                        {
                            coincide = false;
                            break;
                        }
                    }

                    if (coincide)
                        filtrados.ImportRow(row);
                }

                datosActuales = filtrados;
            }

            dgvMostrarDatos.DataSource = null;
            dgvMostrarDatos.DataSource = datosActuales;
            CalcularTotal();
        }

        private static string ConstruirTextoBusqueda(DataRow row)
        {
            var texto = new StringBuilder();

            foreach (DataColumn columna in row.Table.Columns)
            {
                object valor = row[columna];
                if (valor == null || valor == DBNull.Value)
                    continue;

                texto.Append(' ').Append(NormalizarTexto(columna.ColumnName));

                if (valor is DateTime fecha)
                {
                    texto.Append(' ').Append(fecha.ToString(FechaHoraFormats.FechaHora));
                    texto.Append(' ').Append(fecha.ToString("yyyy-MM-dd hh:mm tt"));
                    texto.Append(' ').Append(fecha.ToString(FechaHoraFormats.Hora));
                    texto.Append(' ').Append(fecha.ToString(FechaHoraFormats.HoraSegundos));
                    texto.Append(' ').Append(fecha.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("es-DO")));
                }
                else if (valor is decimal monto)
                {
                    texto.Append(' ').Append(monto.ToString("0.00", CultureInfo.InvariantCulture));
                    texto.Append(' ').Append(monto.ToString("N2", CultureInfo.GetCultureInfo("es-DO")));
                }
                else
                {
                    texto.Append(' ').Append(Convert.ToString(valor, CultureInfo.CurrentCulture));
                }
            }

            return NormalizarTexto(texto.ToString());
        }

        private static string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            string descompuesto = texto
                .Trim()
                .ToUpperInvariant()
                .Normalize(NormalizationForm.FormD);

            var resultado = new StringBuilder(descompuesto.Length);
            bool espacioAnterior = false;

            foreach (char caracter in descompuesto)
            {
                UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);
                if (categoria == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(caracter))
                {
                    resultado.Append(caracter);
                    espacioAnterior = false;
                }
                else if (!espacioAnterior)
                {
                    resultado.Append(' ');
                    espacioAnterior = true;
                }
            }

            return resultado.ToString().Trim();
        }

        private void btnGenerarPDF_Click(object sender, EventArgs e)
        {
            if (estaExportando)
                return;

            try
            {
                if (datosActuales == null || datosActuales.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar. Seleccione una categoría con movimientos.");
                    return;
                }

                using SaveFileDialog sfd = new();
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.DefaultExt = "pdf";
                sfd.AddExtension = true;
                string tipo = cmbReporte.SelectedItem?.ToString() ?? "Reporte";
                DateTime fechaDescarga = DateTime.Now;
                sfd.FileName = $"{tipo}_{fechaDescarga:yyyyMMdd_hhmmss_tt}.pdf";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                estaExportando = true;
                Cursor = Cursors.WaitCursor;

                reporteBLL.GenerarReportePdfDetallado(
                    datosActuales,
                    sfd.FileName,
                    tipo,
                    _desde.Date,
                    _hasta.Date,
                    fechaDescarga,
                    ObtenerMontoTotal());

                MessageBox.Show(
                    "PDF generado con éxito",
                    "MFFITNESS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Anomalía en exportación: " + ex.Message,
                    "Error Crítico",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                estaExportando = false;
            }
        }

        private void btnGenerarExcel_Click(object sender, EventArgs e)
        {
            if (estaExportando)
                return;

            try
            {
                if (datosActuales == null || datosActuales.Rows.Count == 0)
                    return;

                using SaveFileDialog sfd = new();
                sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                sfd.FileName = "Reporte_MFFitness_" + DateTime.Now.ToString("yyyyMMdd");

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                estaExportando = true;
                reporteBLL.GenerarReporteDesdeDataTable(datosActuales, sfd.FileName, ".xlsx");
                MessageBox.Show("Excel generado correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                estaExportando = false;
            }
        }

        private void FrmReportes_Load(object sender, EventArgs e)
        {
            BusquedaFocusHelper.Wire(this);

            _cargandoUi = true;
            try
            {
                cmbReporte.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbReporte.Items.Clear();
                cmbReporte.Items.AddRange(CategoriasReporte);
                cmbReporte.SelectedIndex = 0;
            }
            finally
            {
                _cargandoUi = false;
            }

            AsegurarLayoutGrid();
            CargarReporte();
            _reporteInicialCargado = true;
        }

        private void FrmReportes_Shown(object? sender, EventArgs e)
        {
            AsegurarLayoutGrid();

            if (!_reporteInicialCargado
                || (dgvMostrarDatos.DataSource == null && cmbReporte.SelectedItem != null))
            {
                CargarReporte();
                _reporteInicialCargado = true;
            }
        }
    }
}
