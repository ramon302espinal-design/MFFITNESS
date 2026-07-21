using BLL;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCierresCaja : Form
    {
        private readonly CierreCajaBLL cierreBLL = new CierreCajaBLL();
        private readonly BindingSource _bsCierres = new BindingSource();
        private DataTable? _tablaCierresCompleta;
        private readonly Form? _formularioAnterior;

        public FrmCierresCaja(Form? formularioAnterior = null)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _formularioAnterior = formularioAnterior;
        }

        private void FrmCierresCaja_Load(object sender, EventArgs e)
        {
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
                colCierre.DefaultCellStyle.Format = "dd/MM/yyyy hh:mm tt";
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
            if (nombreColumna is not ("TotalIngresos" or "TotalGastos" or "TotalSistema"
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

        private void btnVolver_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
