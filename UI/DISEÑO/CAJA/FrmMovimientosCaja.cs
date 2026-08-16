
using BLL;
using CORE;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmMovimientosCaja : Form
    {

        private static readonly Regex VentaProductoIdRegex = new(
            @"Venta de productos\s*\(Id\s*(\d+)\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Color ColorReverso = Color.Firebrick;

        private CajaBLL cajaBLL = new CajaBLL();

        private Form? formularioAnterior;

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmMovimientosCaja()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        public FrmMovimientosCaja(Form frm)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            formularioAnterior = frm;
            if (ThemeHost.IsDesignTime())
                return;

            CargarMovimientos();
            ConfigurarEventos();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnPagoRegistrado -= CargarMovimientos;
            AppEventos.OnDeudaModificada -= CargarMovimientos;
            AppEventos.OnCajaCambiada -= CargarMovimientos;
            base.OnFormClosed(e);
        }

        private void CargarMovimientos()
        {
            try
            {
                if (IsDisposed || Disposing)
                    return;

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(CargarMovimientos));
                    return;
                }

                dgvMovimientos.DataSource = cajaBLL.MovimientosHoy();
                dgvMovimientos.ClearSelection();
                DataGridViewHelper.HideColumn(dgvMovimientos, "ClienteId");
                if (dgvMovimientos.Columns["Id"] is DataGridViewColumn colId)
                {
                    colId.HeaderText = "Id";
                    colId.DisplayIndex = 0;
                    colId.FillWeight = 40;
                }
                if (dgvMovimientos.Columns["NombreCliente"] is DataGridViewColumn colNombre)
                    colNombre.HeaderText = "Cliente";
                if (dgvMovimientos.Columns["TipoMovimiento"] is DataGridViewColumn colTipo)
                    colTipo.HeaderText = "Tipo";
                if (dgvMovimientos.Columns["Concepto"] is DataGridViewColumn colConcepto)
                    colConcepto.HeaderText = "Concepto";
                if (dgvMovimientos.Columns["Monto"] is DataGridViewColumn colMonto)
                {
                    colMonto.DefaultCellStyle.Format = "C2";
                    colMonto.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (dgvMovimientos.Columns["Fecha"] is DataGridViewColumn colFecha)
                    colFecha.DefaultCellStyle.Format = FechaHoraFormats.FechaHora;
                dgvMovimientos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando movimientos: " + ex.Message);
            }
        }

        private void ConfigurarEventos()
        {
            dgvMovimientos.CellDoubleClick += DgvMovimientos_CellDoubleClick;
            dgvMovimientos.CellFormatting += DgvMovimientos_CellFormatting;

            // Si la ventana sigue abierta (poco habitual como diálogo) se alinea con
            // pagos, edición de deudas y apertura/cierre de caja.
            AppEventos.OnPagoRegistrado += CargarMovimientos;
            AppEventos.OnDeudaModificada += CargarMovimientos;
            AppEventos.OnCajaCambiada += CargarMovimientos;
        }

        /// <summary>
        /// Los egresos de corrección se muestran como REVERSO (en rojo), no como gasto.
        /// </summary>
        private void DgvMovimientos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewRow row = dgvMovimientos.Rows[e.RowIndex];
            if (row.IsNewRow)
                return;

            if (!dgvMovimientos.Columns.Contains("Concepto"))
                return;

            string concepto = row.Cells["Concepto"].Value?.ToString() ?? string.Empty;
            string metodo = dgvMovimientos.Columns.Contains("MetodoPago")
                ? row.Cells["MetodoPago"].Value?.ToString() ?? string.Empty
                : string.Empty;

            if (!CajaConceptoHelper.EsReverso(concepto, metodo))
                return;

            string columna = dgvMovimientos.Columns[e.ColumnIndex].Name;

            if (string.Equals(columna, "TipoMovimiento", StringComparison.OrdinalIgnoreCase))
            {
                e.Value = "REVERSO";
                e.FormattingApplied = true;
            }

            DataGridViewCellStyle estilo = e.CellStyle ?? new DataGridViewCellStyle();
            estilo.ForeColor = ColorReverso;
            estilo.SelectionForeColor = ColorReverso;
            if (string.Equals(columna, "TipoMovimiento", StringComparison.OrdinalIgnoreCase)
                || string.Equals(columna, "Monto", StringComparison.OrdinalIgnoreCase))
            {
                estilo.Font = new Font(dgvMovimientos.Font, FontStyle.Bold);
            }
            e.CellStyle = estilo;
        }

        private void DgvMovimientos_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            try
            {
                var row = dgvMovimientos.Rows[e.RowIndex];
                string concepto = row.Cells["Concepto"].Value?.ToString() ?? string.Empty;

                // Doble clic en Id de una venta de productos → Historial / tab PRODUCTOS.
                if (EsColumnaId(e.ColumnIndex) &&
                    TryExtraerVentaProductoId(concepto, out int ventaId))
                {
                    AbrirHistorialProductos(ventaId);
                    return;
                }

                if (!EsColumnaId(e.ColumnIndex))
                    return;

                if (row.Cells["ClienteId"].Value == null || row.Cells["ClienteId"].Value == DBNull.Value)
                {
                    MessageBox.Show(
                        "Este movimiento no está asociado a ningún cliente ni a una venta de productos.",
                        "Sin vínculo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                int clienteId = Convert.ToInt32(row.Cells["ClienteId"].Value);
                string nombreCliente = row.Cells["NombreCliente"].Value?.ToString() ?? "Desconocido";

                var result = MessageBox.Show(
                    $"¿Desea ver el historial de membresía de {nombreCliente}?",
                    "Ir a Historial de Membresía",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                    AbrirHistorialMembresia(clienteId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al procesar el movimiento: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool EsColumnaId(int columnIndex)
        {
            return string.Equals(
                dgvMovimientos.Columns[columnIndex].Name,
                "Id",
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryExtraerVentaProductoId(string concepto, out int ventaId)
        {
            ventaId = 0;
            if (string.IsNullOrWhiteSpace(concepto))
                return false;

            Match match = VentaProductoIdRegex.Match(concepto);
            if (!match.Success)
                return false;

            return int.TryParse(match.Groups[1].Value, out ventaId) && ventaId > 0;
        }

        private void AbrirHistorialProductos(int ventaId)
        {
            Form owner = formularioAnterior ?? this;
            using var frmHistorial = new FrmHistorialVentas(owner, ventaId, seleccionarProducto: true);
            frmHistorial.ShowDialog(this);
        }

        private void AbrirHistorialMembresia(int clienteId)
        {
            FrmHistorialMembresia frmHistorial = new FrmHistorialMembresia(clienteId);
            frmHistorial.ShowDialog();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
