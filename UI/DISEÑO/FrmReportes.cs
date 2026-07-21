using BLL;
using System;
using System.Data;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmReportes : Form
    {
        private ReporteBLL reporteBLL = new ReporteBLL();
        private DataTable datosActuales = new DataTable();
        private bool estaExportando = false; // Prevención de doble clic accidental

        public FrmReportes()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            if (ThemeHost.IsDesignTime())
                return;
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
            if (datosActuales == null || datosActuales.Rows.Count == 0) return;

            decimal total = 0;
            foreach (DataRow row in datosActuales.Rows)
            {
                // Auditoría de nulos en el cálculo
                if (datosActuales.Columns.Contains("Monto") && row["Monto"] != DBNull.Value)
                    total += Convert.ToDecimal(row["Monto"]);
                else if (datosActuales.Columns.Contains("Total") && row["Total"] != DBNull.Value)
                    total += Convert.ToDecimal(row["Total"]);
            }
            lblTotal.Text = "TOTAL: " + total.ToString("C");
        }

        private void btnGenerarReporte_Click(object sender, EventArgs e)
        {
            try
            {
                string tipo = cmbReporte.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(tipo)) { MessageBox.Show("Seleccione un tipo de reporte"); return; }
                if (dtDesde.Value.Date > dtHasta.Value.Date) { MessageBox.Show("Rango de fechas inválido"); return; }

                datosActuales = reporteBLL.ObtenerReporte(tipo, dtDesde.Value.Date, dtHasta.Value.Date);
                dgvMostrarDatos.DataSource = datosActuales;
                dgvMostrarDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                CalcularTotal();
            }
            catch (Exception ex) { MessageBox.Show("Error al obtener datos: " + ex.Message); }
        }

        private void btnGenerarPDF_Click(object sender, EventArgs e)
        {
            if (estaExportando) return;

            try
            {
                if (datosActuales == null || datosActuales.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar. Genere el reporte primero.");
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PDF (*.pdf)|*.pdf";
                    sfd.DefaultExt = "pdf";
                    sfd.AddExtension = true;
                    // Nombre sugerido basado en el tipo de reporte seleccionado
                    string tipo = cmbReporte.SelectedItem?.ToString() ?? "Reporte";
                    sfd.FileName = $"{tipo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        estaExportando = true;
                        this.Cursor = Cursors.WaitCursor;

                        // Mandamos la ruta validada por el SaveFileDialog
                        reporteBLL.GenerarReporteDesdeDataTable(datosActuales, sfd.FileName, ".pdf");

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
            cmbReporte.Items.Clear();
            cmbReporte.Items.AddRange(new string[] { "CAJA", "VENTAS", "PAGOS" });
            cmbReporte.SelectedIndex = 0;
        }
    }
}