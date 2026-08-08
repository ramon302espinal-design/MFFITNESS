using BLL;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmReporteDeudas : Form
    {
        private readonly DeudaBLL deudaBLL = new DeudaBLL();
        private readonly ReporteBLL reporteBLL = new ReporteBLL();
        private DataTable _datos = new DataTable();

        public FrmReporteDeudas()
        {
            InitializeComponent();
        }

        private void FrmReporteDeudas_Load(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                _datos = deudaBLL.ObtenerDatosReporteDeudas();
                dgvReporte.DataSource = _datos;
                FormatearGrid();

                decimal totalPendiente = 0m;
                foreach (DataRow row in _datos.Rows)
                {
                    if (row["MontoPendiente"] != DBNull.Value)
                        totalPendiente += Convert.ToDecimal(row["MontoPendiente"]);
                }

                lblResumen.Text = _datos.Rows.Count == 0
                    ? "No hay deudas activas."
                    : $"{_datos.Rows.Count} registro(s) · TOTAL MONTO PENDIENTE: RD$ {totalPendiente:N2} · Generado {DateTime.Now:dd/MM/yyyy hh:mm tt}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando reporte: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatearGrid()
        {
            if (dgvReporte.Columns.Count == 0) return;

            void Header(string col, string titulo)
            {
                if (dgvReporte.Columns.Contains(col))
                    dgvReporte.Columns[col]!.HeaderText = titulo;
            }

            Header("NombreDelDeudor", "Nombre del que debe");
            Header("Telefono", "Teléfono");
            Header("Direccion", "Dirección");
            Header("DeudasActivas", "Deudas activas");
            Header("MontoDeudasActivas", "Monto deudas activas");
            Header("MontoPendiente", "Monto pendiente");
            Header("DeudasVencidas", "Deudas vencidas");
            Header("FechaHoraDeuda", "Fecha y hora de deuda");
            Header("PagoInicial", "Pago inicial");
            Header("FechaAVencer", "Fecha a vencer");
            Header("Concepto", "Concepto");

            if (dgvReporte.Columns["MontoDeudasActivas"] is DataGridViewColumn c1)
                c1.DefaultCellStyle.Format = "N2";
            if (dgvReporte.Columns["MontoPendiente"] is DataGridViewColumn cPend)
                cPend.DefaultCellStyle.Format = "N2";
            if (dgvReporte.Columns["PagoInicial"] is DataGridViewColumn c2)
                c2.DefaultCellStyle.Format = "N2";
            if (dgvReporte.Columns["FechaHoraDeuda"] is DataGridViewColumn c3)
                c3.DefaultCellStyle.Format = "dd/MM/yyyy hh:mm tt";
            if (dgvReporte.Columns["FechaAVencer"] is DataGridViewColumn c4)
                c4.DefaultCellStyle.Format = "dd/MM/yyyy";

            dgvReporte.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReporte.ReadOnly = true;
            dgvReporte.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporte.RowHeadersVisible = false;
        }

        private void btnDescargarPdf_Click(object sender, EventArgs e)
        {
            if (_datos == null || _datos.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para descargar.", "Reporte",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                AddExtension = true,
                FileName = $"Reporte_Deudas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                reporteBLL.GenerarPdfReporteDeudas(_datos, sfd.FileName);

                DialogResult abrir = MessageBox.Show(
                    "PDF generado correctamente.\n\n¿Desea abrirlo?",
                    "Reporte de deudas",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (abrir == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando PDF: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e) => Close();
    }
}
