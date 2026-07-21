using BLL;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmHistorialVentas : Form
    {
        private Form? formularioAnterior;
        private readonly int? _clienteIdParaSeleccionar;
        private readonly string? _nombreClienteParaSeleccionar;
        private bool _seleccionClienteAplicada;

        private readonly PagoBLL pagoBLL = new PagoBLL();
        private readonly VentasBLL ventasBLL = new VentasBLL();
        private readonly HistorialMembresiaBLL historialBLL = new HistorialMembresiaBLL();

        public FrmHistorialVentas(Form frm)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            formularioAnterior = frm;
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloHistorial);
        }

        public FrmHistorialVentas(Form frm, int clienteId, string? nombreCliente = null) : this(frm)
        {
            _clienteIdParaSeleccionar = clienteId;
            _nombreClienteParaSeleccionar = nombreCliente;
        }

        private void ActualizarHistorial()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ActualizarHistorial));
                return;
            }

            CargarHistorialPagos();
        }

        public FrmHistorialVentas()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloHistorial);
        }

        private void CargarHistorialMembresia()
        {
            dgvHistorialMembresia.DataBindingComplete -= DgvHistorialMembresia_DespuesDeEnlazar;

            dgvHistorialMembresia.Columns.Clear();
            dgvHistorialMembresia.DataSource = historialBLL.ObtenerHistorial();

            if (_clienteIdParaSeleccionar.HasValue)
                dgvHistorialMembresia.DataBindingComplete += DgvHistorialMembresia_DespuesDeEnlazar;
            else if (dgvHistorialMembresia.Columns.Count > 0)
                ConfigurarColumnasHistorialMembresia(dgvHistorialMembresia);
        }

        private void DgvHistorialMembresia_DespuesDeEnlazar(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvHistorialMembresia.DataBindingComplete -= DgvHistorialMembresia_DespuesDeEnlazar;

            if (dgvHistorialMembresia.Columns.Count > 0)
                ConfigurarColumnasHistorialMembresia(dgvHistorialMembresia);

            if (!_clienteIdParaSeleccionar.HasValue || _seleccionClienteAplicada)
                return;

            BeginInvoke(new Action(AplicarSeleccionClientePendiente));
        }

        private static void ConfigurarColumnasHistorialMembresia(DataGridView grid)
        {
            if (grid.Columns["Monto"] is DataGridViewColumn colMonto)
            {
                colMonto.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                colMonto.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns["Nombre"] != null) grid.Columns["Nombre"]!.HeaderText = "Cliente";
            if (grid.Columns["PlanNombre"] != null) grid.Columns["PlanNombre"]!.HeaderText = "Plan";
            if (grid.Columns["TipoMovimiento"] != null) grid.Columns["TipoMovimiento"]!.HeaderText = "Movimiento";

            if (grid.Columns["FechaPago"] is DataGridViewColumn colFechaPago)
            {
                colFechaPago.HeaderText = "Pagado el";
                colFechaPago.DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (grid.Columns["FechaVence"] is DataGridViewColumn colFechaVence)
            {
                colFechaVence.HeaderText = "Vence el";
                colFechaVence.DefaultCellStyle.Format = "dd/MM/yyyy";
                colFechaVence.DefaultCellStyle.ForeColor = Color.Blue;
                colFechaVence.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
            }

            if (grid.Columns["Usuario"] != null) grid.Columns["Usuario"]!.HeaderText = "Atendió";
            if (grid.Columns["Nota"] != null) grid.Columns["Nota"]!.HeaderText = "Detalle";
            if (grid.Columns["ClienteId"] != null) grid.Columns["ClienteId"]!.Visible = false;

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_clienteIdParaSeleccionar.HasValue && !_seleccionClienteAplicada)
                AplicarSeleccionClientePendiente();
        }

        private void AplicarSeleccionClientePendiente()
        {
            if (!_clienteIdParaSeleccionar.HasValue || _seleccionClienteAplicada)
                return;

            if (SeleccionarUltimaAccionCliente(_clienteIdParaSeleccionar.Value, _nombreClienteParaSeleccionar))
                _seleccionClienteAplicada = true;
        }

        private void FrmHistorialVentas_Load(object sender, EventArgs e)
        {
            CORE.AppEventos.OnPagoRegistrado += ActualizarHistorial;
            CargarHistorialPagos();
            CargarVentas();
            CargarHistorialMembresia();

            dgvHistorial.ClearSelection();
            dgvVentasProductos.ClearSelection();
            dgvDetalleProductos.ClearSelection();
        }

        private bool SeleccionarUltimaAccionCliente(int clienteId, string? nombreCliente = null)
        {
            tabControl1.SelectedTab = tabMembresia;

            DataGridViewRow? filaObjetivo = null;
            DateTime fechaMasReciente = DateTime.MinValue;

            foreach (DataGridViewRow row in dgvHistorialMembresia.Rows)
            {
                if (row.IsNewRow) continue;

                if (!TryObtenerClienteIdFila(row, out int idFila) || idFila != clienteId)
                    continue;

                if (!CoincideNombreClienteFila(row, nombreCliente))
                    continue;

                if (!TryObtenerFechaPagoFila(row, out DateTime fechaFila))
                    fechaFila = DateTime.MinValue;

                if (filaObjetivo == null || fechaFila >= fechaMasReciente)
                {
                    fechaMasReciente = fechaFila;
                    filaObjetivo = row;
                }
            }

            if (filaObjetivo == null)
                return false;

            dgvHistorialMembresia.ClearSelection();
            filaObjetivo.Selected = true;

            var celdaVisible = ObtenerPrimeraCeldaVisible(filaObjetivo);
            if (celdaVisible != null)
                dgvHistorialMembresia.CurrentCell = celdaVisible;

            if (filaObjetivo.Index >= 0 && filaObjetivo.Index < dgvHistorialMembresia.RowCount)
                dgvHistorialMembresia.FirstDisplayedScrollingRowIndex = filaObjetivo.Index;

            return true;
        }

        private static bool TryObtenerClienteIdFila(DataGridViewRow row, out int clienteId)
        {
            clienteId = 0;

            if (row.DataBoundItem is DataRowView fila)
            {
                if (fila.Row.Table.Columns.Contains("ClienteId") &&
                    fila["ClienteId"] != DBNull.Value &&
                    int.TryParse(fila["ClienteId"]?.ToString(), out clienteId))
                {
                    return true;
                }
            }

            foreach (var nombreColumna in new[] { "ClienteId", "ClienteID", "ID", "Id" })
            {
                if (row.DataGridView?.Columns.Contains(nombreColumna) != true)
                    continue;

                var valor = row.Cells[nombreColumna].Value;
                if (valor != null && valor != DBNull.Value && int.TryParse(valor.ToString(), out clienteId))
                    return true;
            }

            return false;
        }

        private static bool TryObtenerFechaPagoFila(DataGridViewRow row, out DateTime fecha)
        {
            fecha = DateTime.MinValue;

            if (row.DataBoundItem is DataRowView fila)
            {
                if (fila.Row.Table.Columns.Contains("FechaPago") &&
                    fila["FechaPago"] != DBNull.Value &&
                    DateTime.TryParse(fila["FechaPago"]?.ToString(), out fecha))
                {
                    return true;
                }
            }

            if (row.DataGridView?.Columns.Contains("FechaPago") == true)
            {
                var valor = row.Cells["FechaPago"].Value;
                if (valor != null && valor != DBNull.Value && DateTime.TryParse(valor.ToString(), out fecha))
                    return true;
            }

            return false;
        }

        private static bool CoincideNombreClienteFila(DataGridViewRow row, string? nombreEsperado)
        {
            if (string.IsNullOrWhiteSpace(nombreEsperado))
                return true;

            string? nombreFila = null;

            if (row.DataBoundItem is DataRowView fila && fila.Row.Table.Columns.Contains("Nombre"))
                nombreFila = fila["Nombre"]?.ToString();
            else if (row.DataGridView?.Columns.Contains("Nombre") == true)
                nombreFila = row.Cells["Nombre"].Value?.ToString();

            return string.Equals(
                nombreFila?.Trim(),
                nombreEsperado.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private static DataGridViewCell? ObtenerPrimeraCeldaVisible(DataGridViewRow row)
        {
            foreach (DataGridViewCell celda in row.Cells)
            {
                if (celda.Visible && celda.OwningColumn?.Visible == true)
                    return celda;
            }

            return null;
        }

        public void CargarHistorialPagos()
        {
            dgvHistorial.DataSource = pagoBLL.ListarPagos();
            var grid = dgvHistorial;
            grid.EnableHeadersVisualStyles = false;

            if (grid.Columns["FechaVencimiento"] is DataGridViewColumn colVence)
            {
                colVence.HeaderText = "Vence el";
                colVence.DefaultCellStyle.Format = "dd/MM/yyyy";
                colVence.DefaultCellStyle.ForeColor = Color.Blue;
                colVence.DefaultCellStyle.SelectionForeColor = Color.Blue;
                colVence.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                colVence.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.ClearSelection();
        }

        private void CargarVentas()
        {
            dgvVentasProductos.DataSource = ventasBLL.ListarVentas();
        }

        private void dgvVentasProductos_SelectionChanged(object sender, EventArgs e)
        {
            var val = dgvVentasProductos.CurrentRow?.Cells["Id"]?.Value;
            if (val == null) return;

            int ventaId = Convert.ToInt32(val);
            dgvDetalleProductos.DataSource = ventasBLL.ListarDetalleVenta(ventaId);
        }

        private void FrmHistorialVentas_FormClosed(object sender, FormClosedEventArgs e)
        {
            CORE.AppEventos.OnPagoRegistrado -= ActualizarHistorial;
        }
    }
}
