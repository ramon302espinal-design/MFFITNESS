using BLL;
using CORE;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmReportes : Form
    {
        private ReporteBLL reporteBLL = new ReporteBLL();
        private DataTable datosFuente = new DataTable();
        private DataTable datosActuales = new DataTable();
        private bool estaExportando = false;
        private bool _cargandoUi;

        public FrmReportes()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            if (ThemeHost.IsDesignTime())
                return;
            dgvMostrarDatos.CellFormatting += DgvMostrarDatos_CellFormatting;
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloReportes);
            AjustarContenidoTrasNavBar();
        }

        /// <summary>
        /// Baja el contenido absoluto para que no quede debajo de la barra Dock.Top.
        /// </summary>
        private void AjustarContenidoTrasNavBar()
        {
            const int offset = 52;
            foreach (Control c in Controls)
            {
                if (c.Dock != DockStyle.None)
                    continue;
                c.Top += offset;
            }
        }

        private void CalcularTotal()
        {
            if (datosActuales == null || datosActuales.Rows.Count == 0)
            {
                lblTotal.Text = "TOTAL: " + 0m.ToString("C");
                return;
            }

            decimal total = ObtenerMontoTotal();
            lblTotal.Text = "TOTAL: " + total.ToString("C");
        }

        private decimal ObtenerMontoTotal()
        {
            if (datosActuales == null)
                return 0m;

            decimal total = 0m;
            foreach (DataRow row in datosActuales.Rows)
            {
                if (datosActuales.Columns.Contains("Monto") && row["Monto"] != DBNull.Value)
                    total += Convert.ToDecimal(row["Monto"]);
                else if (datosActuales.Columns.Contains("Total") && row["Total"] != DBNull.Value)
                    total += Convert.ToDecimal(row["Total"]);
            }

            return total;
        }

        /// <summary>
        /// Recarga el grid según categoría y rango. Sustituye al botón GENERAR REPORTE.
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
                    datosActuales = new DataTable();
                    dgvMostrarDatos.DataSource = datosActuales;
                    lblTotal.Text = "TOTAL: " + 0m.ToString("C");
                    return;
                }

                if (dtDesde.Value.Date > dtHasta.Value.Date)
                {
                    lblTotal.Text = "TOTAL: —";
                    return;
                }

                datosFuente = reporteBLL.ObtenerReporte(tipo, dtDesde.Value.Date, dtHasta.Value.Date);
                AplicarBusquedaInteligente();
                dgvMostrarDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                FormatearColumnasReporte(tipo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener datos: " + ex.Message);
            }
        }

        private void FormatearColumnasReporte(string tipo)
        {
            if (dgvMostrarDatos.Columns.Count == 0)
                return;

            if (dgvMostrarDatos.Columns.Contains("Fecha"))
                dgvMostrarDatos.Columns["Fecha"].DefaultCellStyle.Format = FechaHoraFormats.FechaHora;

            if (dgvMostrarDatos.Columns.Contains("FechaPago"))
                dgvMostrarDatos.Columns["FechaPago"].DefaultCellStyle.Format = FechaHoraFormats.FechaHora;

            if (dgvMostrarDatos.Columns.Contains("Monto"))
                dgvMostrarDatos.Columns["Monto"].DefaultCellStyle.Format = "N2";

            if (dgvMostrarDatos.Columns.Contains("Total"))
                dgvMostrarDatos.Columns["Total"].DefaultCellStyle.Format = "N2";

            if (string.Equals(tipo, "CAJA", StringComparison.OrdinalIgnoreCase))
            {
                if (dgvMostrarDatos.Columns.Contains("Método de Pago"))
                    dgvMostrarDatos.Columns["Método de Pago"].HeaderText = "Método de Pago";
                if (dgvMostrarDatos.Columns.Contains("MIEMBRO"))
                    dgvMostrarDatos.Columns["MIEMBRO"].HeaderText = "MIEMBRO";
                if (dgvMostrarDatos.Columns.Contains("USUARIO"))
                    dgvMostrarDatos.Columns["USUARIO"].HeaderText = "USUARIO";
            }
        }

        /// <summary>
        /// Identifica visualmente las correcciones de caja: la celda Tipo muestra
        /// REVERSO y queda roja, sin confundirla con un gasto/EGRESO operativo.
        /// </summary>
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
        {
            CargarReporte();
        }

        private void txtBusca_TextChanged(object? sender, EventArgs e)
        {
            AplicarBusquedaInteligente();
        }

        /// <summary>
        /// Búsqueda inmediata en todas las columnas visibles. Ignora acentos,
        /// mayúsculas y signos; admite varias palabras en cualquier orden.
        /// </summary>
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
            if (estaExportando) return;

            try
            {
                if (datosActuales == null || datosActuales.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar. Seleccione una categoría con movimientos.");
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PDF (*.pdf)|*.pdf";
                    sfd.DefaultExt = "pdf";
                    sfd.AddExtension = true;
                    string tipo = cmbReporte.SelectedItem?.ToString() ?? "Reporte";
                    DateTime fechaDescarga = DateTime.Now;
                    sfd.FileName = $"{tipo}_{fechaDescarga:yyyyMMdd_hhmmss_tt}.pdf";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        estaExportando = true;
                        this.Cursor = Cursors.WaitCursor;

                        reporteBLL.GenerarReportePdfDetallado(
                            datosActuales,
                            sfd.FileName,
                            tipo,
                            dtDesde.Value.Date,
                            dtHasta.Value.Date,
                            fechaDescarga,
                            ObtenerMontoTotal());

                        MessageBox.Show("PDF generado con éxito", "MFFITNESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Anomalía en exportación: " + ex.Message, "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                estaExportando = false;
            }
        }

        private void btnGenerarExcel_Click(object sender, EventArgs e)
        {
            if (estaExportando) return;

            try
            {
                if (datosActuales == null || datosActuales.Rows.Count == 0) return;

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                    sfd.FileName = "Reporte_MFFitness_" + DateTime.Now.ToString("yyyyMMdd");

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        estaExportando = true;
                        reporteBLL.GenerarReporteDesdeDataTable(datosActuales, sfd.FileName, ".xlsx");
                        MessageBox.Show("Excel generado correctamente");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            finally { estaExportando = false; }
        }

        private void FrmReportes_Load(object sender, EventArgs e)
        {
            _cargandoUi = true;
            try
            {
                cmbReporte.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbReporte.Items.Clear();
                cmbReporte.Items.AddRange(new string[] { "CAJA", "VENTAS", "PAGOS" });
                cmbReporte.SelectedIndex = 0;
                ActualizarLblTiempo();
            }
            finally
            {
                _cargandoUi = false;
            }

            CargarReporte();
        }

        private void RangoFechas_ValueChanged(object? sender, EventArgs e)
        {
            ActualizarLblTiempo();
            CargarReporte();
        }

        /// <summary>
        /// Muestra en lbltiempo cuántos días cubre el rango Desde–Hasta (inclusive).
        /// </summary>
        private void ActualizarLblTiempo()
        {
            if (lbltiempo == null || dtDesde == null || dtHasta == null)
                return;

            DateTime desde = dtDesde.Value.Date;
            DateTime hasta = dtHasta.Value.Date;

            if (desde > hasta)
            {
                lbltiempo.ForeColor = Color.FromArgb(220, 38, 38);
                lbltiempo.Text = "Rango inválido";
                return;
            }

            // Días transcurridos: del 10/08 al 20/08 son 10 días.
            int dias = (hasta - desde).Days;
            lbltiempo.ForeColor = Color.FromArgb(27, 146, 255);
            lbltiempo.Text = dias == 1 ? "1 día" : $"{dias} días";
        }
    }
}
