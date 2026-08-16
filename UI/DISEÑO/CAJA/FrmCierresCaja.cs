using BLL;
using CORE;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCierresCaja : Form
    {
        private readonly CierreCajaBLL cierreBLL = new CierreCajaBLL();
        private readonly ReporteBLL reporteBLL = new ReporteBLL();
        private readonly BindingSource _bsCierres = new BindingSource();
        private DataTable? _tablaCierresCompleta;
        private readonly Form? _formularioAnterior;
        private bool _esAdmin;

        public FrmCierresCaja(Form? formularioAnterior = null)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _formularioAnterior = formularioAnterior;
        }

        private void FrmCierresCaja_Load(object sender, EventArgs e)
        {
            _esAdmin = string.Equals(
                Sesion.Rol?.Trim(),
                "ADMIN",
                StringComparison.OrdinalIgnoreCase);
            btnEliminarCierre.Visible = _esAdmin;
            lblTituloCierre.Text = _esAdmin
                ? "CUADRES DE CAJA — TODOS LOS USUARIOS"
                : $"MIS CUADRES DE CAJA — {Sesion.Usuario.Trim().ToUpperInvariant()}";

            InicializarFiltrosCierre();
            CargarCierresCaja();
            dgvCierres.ClearSelection();
        }

        private void CargarCierresCaja()
        {
            _tablaCierresCompleta = cierreBLL.ObtenerHistorial();
            _bsCierres.DataSource = _tablaCierresCompleta;

            dgvCierres.DataBindingComplete -= DgvCierres_DespuesDeEnlazar;
            dgvCierres.DataSource = _bsCierres;
            dgvCierres.DataBindingComplete += DgvCierres_DespuesDeEnlazar;

            if (dgvCierres.Columns.Count > 0)
                ConfigurarColumnasCierres(dgvCierres);

            AplicarFiltroCierres();
        }

        private void InicializarFiltrosCierre()
        {
            cmbRangoCierre.Items.Clear();
            cmbRangoCierre.Items.AddRange(BusquedaCierreCajaHelper.PresetsRango);
            cmbRangoCierre.SelectedIndex = 1;

            dtpDesdeCierre.Value = DateTime.Today;
            dtpHastaCierre.Value = DateTime.Today;
            dgvCierres.CellFormatting -= dgvCierres_CellFormatting;
            dgvCierres.CellFormatting += dgvCierres_CellFormatting;
        }

        private void DgvCierres_DespuesDeEnlazar(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvCierres.DataBindingComplete -= DgvCierres_DespuesDeEnlazar;
            ConfigurarColumnasCierres(dgvCierres);
        }

        private static void ConfigurarColumnasCierres(DataGridView grid)
        {
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ConfigurarColumnaMoneda(grid, "MontoInicial", "Monto inicial");
            ConfigurarColumnaMoneda(grid, "TotalIngresos", "Ingresos");
            ConfigurarColumnaMoneda(grid, "TotalGastos", "Gastos");
            ConfigurarColumnaMoneda(grid, "TotalSistema", "Total sistema");
            ConfigurarColumnaMoneda(grid, "TotalContado", "Contado");
            ConfigurarColumnaMoneda(grid, "Diferencia", "Diferencia");

            if (grid.Columns["Fecha"] is DataGridViewColumn colFecha)
            {
                colFecha.HeaderText = "Fecha cuadre";
                colFecha.DefaultCellStyle.Format = "dd/MM/yyyy";
                colFecha.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (grid.Columns["FechaCierre"] is DataGridViewColumn colCierre)
            {
                colCierre.HeaderText = "Cerrado el";
                colCierre.DefaultCellStyle.Format = FechaHoraFormats.FechaHora;
                colCierre.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (grid.Columns["Turno"] != null) grid.Columns["Turno"]!.HeaderText = "Turno";
            if (grid.Columns["Usuario"] != null) grid.Columns["Usuario"]!.HeaderText = "Usuario";
            if (grid.Columns["CajaId"] != null) grid.Columns["CajaId"]!.Visible = false;
            if (grid.Columns["Id"] != null) grid.Columns["Id"]!.HeaderText = "ID";
        }

        private static void ConfigurarColumnaMoneda(DataGridView grid, string nombre, string titulo)
        {
            if (grid.Columns[nombre] is not DataGridViewColumn col)
                return;

            col.HeaderText = titulo;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
        }

        private void dgvCierres_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string nombreColumna = dgvCierres.Columns[e.ColumnIndex].Name;

            if (nombreColumna == "Turno")
            {
                DateTime? fechaCierre = null;
                if (dgvCierres.Rows[e.RowIndex].Cells["FechaCierre"]?.Value is DateTime fc)
                    fechaCierre = fc;
                else if (DateTime.TryParse(dgvCierres.Rows[e.RowIndex].Cells["FechaCierre"]?.Value?.ToString(), out DateTime fcParsed))
                    fechaCierre = fcParsed;

                e.Value = CajaServiceBLL.NormalizarNombreTurno(e.Value?.ToString(), fechaCierre);
                e.FormattingApplied = true;
                return;
            }

            if (nombreColumna is not ("MontoInicial" or "TotalIngresos" or "TotalGastos" or "TotalSistema"
                or "TotalContado" or "Diferencia"))
                return;

            if (e.Value == null || e.Value == DBNull.Value)
            {
                e.Value = MonedaHelper.FormatearRd(0m);
                e.FormattingApplied = true;
                return;
            }

            if (decimal.TryParse(e.Value.ToString(), out decimal monto))
            {
                e.Value = MonedaHelper.FormatearRd(monto);
                e.FormattingApplied = true;

                if (nombreColumna == "Diferencia")
                {
                    e.CellStyle.ForeColor = monto switch
                    {
                        0 => Color.DarkGreen,
                        > 0 => Color.DarkOrange,
                        _ => Color.DarkRed
                    };
                    e.CellStyle.Font = new Font(dgvCierres.Font, FontStyle.Bold);
                }
            }
        }

        private void txtBuscarCierre_TextChanged(object sender, EventArgs e) =>
            AplicarFiltroCierres();

        private void cmbRangoCierre_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool personalizado = cmbRangoCierre.SelectedItem?.ToString() == "Personalizado";
            dtpDesdeCierre.Enabled = personalizado;
            dtpHastaCierre.Enabled = personalizado;
            AplicarFiltroCierres();
        }

        private void FiltroCierre_Changed(object? sender, EventArgs e)
        {
            if (cmbRangoCierre.SelectedItem?.ToString() == "Personalizado")
                AplicarFiltroCierres();
        }

        private void btnLimpiarFiltroCierre_Click(object sender, EventArgs e)
        {
            txtBuscarCierre.Clear();
            cmbRangoCierre.SelectedIndex = 0;
            dtpDesdeCierre.Value = DateTime.Today;
            dtpHastaCierre.Value = DateTime.Today;
            AplicarFiltroCierres();
        }

        private void AplicarFiltroCierres()
        {
            if (_tablaCierresCompleta == null)
                return;

            string preset = cmbRangoCierre.SelectedItem?.ToString() ?? "Todos";
            RangoFechaBusqueda? rangoPreset = preset == "Personalizado"
                ? null
                : BusquedaCierreCajaHelper.ResolverPreset(preset);

            RangoFechaBusqueda? rangoPersonalizado = preset == "Personalizado"
                ? RangoFechaBusqueda.Entre(dtpDesdeCierre.Value, dtpHastaCierre.Value)
                : null;

            string filtro = BusquedaCierreCajaHelper.ConstruirFiltroDataView(
                rangoPreset,
                rangoPersonalizado,
                txtBuscarCierre.Text);

            _bsCierres.Filter = filtro;
            ActualizarResumenCierres();
            dgvCierres.ClearSelection();
        }

        private void ActualizarResumenCierres()
        {
            if (_tablaCierresCompleta == null)
            {
                lblResumenCierres.Text = "0 cierres";
                return;
            }

            int cantidad = 0;
            decimal ingresos = 0m;
            decimal gastos = 0m;
            decimal diferencia = 0m;

            foreach (DataRowView fila in _bsCierres.List)
            {
                cantidad++;
                ingresos += Convert.ToDecimal(fila["TotalIngresos"]);
                gastos += Convert.ToDecimal(fila["TotalGastos"]);
                diferencia += Convert.ToDecimal(fila["Diferencia"]);
            }

            lblResumenCierres.Text =
                $"{cantidad} cierre(s) · Ingresos {MonedaHelper.FormatearRd(ingresos)} · " +
                $"Gastos {MonedaHelper.FormatearRd(gastos)} · Diferencia {MonedaHelper.FormatearRd(diferencia)}";
        }

        private void btnEliminarCierre_Click(object sender, EventArgs e)
        {
            if (!_esAdmin)
            {
                MessageBox.Show(
                    "Solo ADMIN puede eliminar cuadres de caja.",
                    "Acceso restringido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (dgvCierres.CurrentRow?.Cells["Id"]?.Value == null) return;

            string password = Interaction.InputBox("Ingrese contraseña:", "Seguridad", "");

            if (password != "12345")
            {
                MessageBox.Show("Contraseña incorrecta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idCierre = Convert.ToInt32(dgvCierres.CurrentRow.Cells["Id"].Value);

            if (MessageBox.Show("¿Eliminar cierre?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                cierreBLL.EliminarCierre(idCierre);
                CargarCierresCaja();
            }
        }

        private void btnDescargar_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable datos = CrearTablaVisibleParaExportar();
                if (datos.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No hay cuadres visibles para descargar.",
                        "Descargar cuadres",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using var sfd = new SaveFileDialog
                {
                    Title = "Descargar cuadres visibles",
                    Filter = "Libro de Excel (*.xlsx)|*.xlsx|Documento PDF (*.pdf)|*.pdf",
                    FilterIndex = 1,
                    AddExtension = true,
                    FileName = $"Cuadres_Caja_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                string extension = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                if (extension is not (".xlsx" or ".pdf"))
                    extension = sfd.FilterIndex == 2 ? ".pdf" : ".xlsx";

                string ruta = Path.ChangeExtension(sfd.FileName, extension);
                reporteBLL.GenerarReporteDesdeDataTable(datos, ruta, extension);

                MessageBox.Show(
                    $"Cuadres descargados correctamente en:\n{ruta}",
                    "Descarga completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible descargar los cuadres: " + ex.Message,
                    "Error de descarga",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private DataTable CrearTablaVisibleParaExportar()
        {
            var resultado = new DataTable("Cuadres de Caja");
            var columnas = new List<(DataGridViewColumn Grid, DataColumn Tabla)>();

            foreach (DataGridViewColumn columnaGrid in dgvCierres.Columns)
            {
                if (!columnaGrid.Visible)
                    continue;

                string origen = string.IsNullOrWhiteSpace(columnaGrid.DataPropertyName)
                    ? columnaGrid.Name
                    : columnaGrid.DataPropertyName;
                Type tipo = _tablaCierresCompleta?.Columns[origen]?.DataType ?? typeof(string);
                string titulo = string.IsNullOrWhiteSpace(columnaGrid.HeaderText)
                    ? columnaGrid.Name
                    : columnaGrid.HeaderText.Trim();
                string nombreUnico = titulo;
                int sufijo = 2;
                while (resultado.Columns.Contains(nombreUnico))
                    nombreUnico = $"{titulo} ({sufijo++})";

                DataColumn columnaTabla = resultado.Columns.Add(nombreUnico, tipo);
                columnas.Add((columnaGrid, columnaTabla));
            }

            foreach (DataRowView filaVisible in _bsCierres.List)
            {
                DataRow nueva = resultado.NewRow();
                foreach ((DataGridViewColumn grid, DataColumn tabla) in columnas)
                {
                    string origen = string.IsNullOrWhiteSpace(grid.DataPropertyName)
                        ? grid.Name
                        : grid.DataPropertyName;
                    nueva[tabla] = filaVisible.Row.Table.Columns.Contains(origen)
                        ? filaVisible[origen]
                        : DBNull.Value;
                }
                resultado.Rows.Add(nueva);
            }

            return resultado;
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
