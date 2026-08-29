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
        private readonly int? _ventaIdParaSeleccionar;
        private bool _seleccionVentaAplicada;

        private readonly PagoBLL pagoBLL = new PagoBLL();
        private readonly VentasBLL ventasBLL = new VentasBLL();
        private readonly HistorialMembresiaBLL historialBLL = new HistorialMembresiaBLL();
        private readonly BindingSource _bsHistorialMembresia = new BindingSource();
        private readonly BindingSource _bsVentasProductos = new BindingSource();

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

        /// <summary>
        /// Abre el historial enfocado en PRODUCTOS y selecciona la venta indicada.
        /// </summary>
        public FrmHistorialVentas(Form frm, int ventaId, bool seleccionarProducto) : this(frm)
        {
            if (seleccionarProducto && ventaId > 0)
                _ventaIdParaSeleccionar = ventaId;
        }

        private void ActualizarHistorial()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ActualizarHistorial));
                return;
            }

            CargarHistorialPagos();
            CargarHistorialMembresia();
        }

        public FrmHistorialVentas()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloHistorial);
        }

        private void CargarHistorialMembresia()
        {
            string filtroActual = txtBuscarHistMembresia?.Text?.Trim() ?? string.Empty;

            dgvHistorialMembresia.DataBindingComplete -= DgvHistorialMembresia_DespuesDeEnlazar;

            dgvHistorialMembresia.Columns.Clear();
            _bsHistorialMembresia.DataSource = historialBLL.ObtenerHistorial();
            dgvHistorialMembresia.DataSource = _bsHistorialMembresia;

            if (!string.IsNullOrEmpty(filtroActual) && txtBuscarHistMembresia != null)
                txtBuscarHistMembresia.Text = filtroActual;

            AplicarFiltroBusquedaHistorialMembresia();

            if (_clienteIdParaSeleccionar.HasValue)
                dgvHistorialMembresia.DataBindingComplete += DgvHistorialMembresia_DespuesDeEnlazar;
            else if (dgvHistorialMembresia.Columns.Count > 0)
                ConfigurarColumnasHistorialMembresia(dgvHistorialMembresia);
        }

        private void txtBuscarHistMembresia_TextChanged(object? sender, EventArgs e)
        {
            AplicarFiltroBusquedaHistorialMembresia();
        }

        private void AplicarFiltroBusquedaHistorialMembresia()
        {
            if (_bsHistorialMembresia.DataSource == null)
                return;

            var termino = txtBuscarHistMembresia?.Text?.Trim() ?? string.Empty;
            try
            {
                _bsHistorialMembresia.Filter = string.IsNullOrEmpty(termino)
                    ? null
                    : BusquedaGridHelper.ConstruirFiltroHistorialMembresia(termino);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtro historial membresía: {ex.Message}");
                _bsHistorialMembresia.RemoveFilter();
            }
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
            // Usados por el buscador; no saturan el grid.
            if (grid.Columns["Telefono"] != null) grid.Columns["Telefono"]!.Visible = false;
            if (grid.Columns["Direccion"] != null) grid.Columns["Direccion"]!.Visible = false;

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

            if (_ventaIdParaSeleccionar.HasValue && !_seleccionVentaAplicada)
                AplicarSeleccionVentaPendiente();
        }

        private void AplicarSeleccionClientePendiente()
        {
            if (!_clienteIdParaSeleccionar.HasValue || _seleccionClienteAplicada)
                return;

            if (SeleccionarUltimaAccionCliente(_clienteIdParaSeleccionar.Value, _nombreClienteParaSeleccionar))
                _seleccionClienteAplicada = true;
        }

        private void AplicarSeleccionVentaPendiente()
        {
            if (!_ventaIdParaSeleccionar.HasValue || _seleccionVentaAplicada)
                return;

            if (SeleccionarVentaProducto(_ventaIdParaSeleccionar.Value))
                _seleccionVentaAplicada = true;
        }

        /// <summary>
        /// Activa tabProductos, selecciona la venta y carga su detalle.
        /// </summary>
        private bool SeleccionarVentaProducto(int ventaId)
        {
            if (tabControl1 == null || tabProductos == null || dgvVentasProductos == null)
                return false;

            tabControl1.SelectedTab = tabProductos;

            // Quitar filtro para garantizar que la venta exista en la vista.
            if (txtBuscarProductos != null && !string.IsNullOrWhiteSpace(txtBuscarProductos.Text))
                txtBuscarProductos.Clear();

            if (!dgvVentasProductos.Columns.Contains("Id"))
                return false;

            foreach (DataGridViewRow row in dgvVentasProductos.Rows)
            {
                if (row.IsNewRow)
                    continue;

                var valor = row.Cells["Id"].Value;
                if (valor == null || valor == DBNull.Value)
                    continue;

                if (Convert.ToInt32(valor) != ventaId)
                    continue;

                dgvVentasProductos.ClearSelection();
                row.Selected = true;

                var celdaVisible = ObtenerPrimeraCeldaVisible(row);
                if (celdaVisible != null)
                    dgvVentasProductos.CurrentCell = celdaVisible;

                if (row.Index >= 0 && row.Index < dgvVentasProductos.RowCount)
                    dgvVentasProductos.FirstDisplayedScrollingRowIndex = row.Index;

                CargarDetalleVentaProducto(ventaId, row);
                return true;
            }

            return false;
        }

        private void CargarDetalleVentaProducto(int ventaId, DataGridViewRow? filaVenta = null)
        {
            dgvDetalleProductos.DataSource = ventasBLL.ListarDetalleVenta(ventaId);
            dgvDetalleProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetalleProductos.ReadOnly = true;
            dgvDetalleProductos.RowHeadersVisible = false;

            decimal totalVenta = 0m;
            decimal saldoVenta = 0m;
            decimal pagoInicial = 0m;

            if (filaVenta?.DataBoundItem is DataRowView fila)
            {
                totalVenta = fila["Total"] == DBNull.Value ? 0m : Convert.ToDecimal(fila["Total"]);
                saldoVenta = fila["Saldo"] == DBNull.Value ? 0m : Convert.ToDecimal(fila["Saldo"]);
                pagoInicial = fila["MontoPagado"] == DBNull.Value ? 0m : Convert.ToDecimal(fila["MontoPagado"]);
            }
            else if (dgvVentasProductos.CurrentRow?.DataBoundItem is DataRowView filaActual)
            {
                totalVenta = filaActual["Total"] == DBNull.Value ? 0m : Convert.ToDecimal(filaActual["Total"]);
                saldoVenta = filaActual["Saldo"] == DBNull.Value ? 0m : Convert.ToDecimal(filaActual["Saldo"]);
                pagoInicial = filaActual["MontoPagado"] == DBNull.Value ? 0m : Convert.ToDecimal(filaActual["MontoPagado"]);
            }

            if (label3 != null)
            {
                label3.Text = saldoVenta > 0
                    ? $"DETALLE · FINANCIADO · Precio Total RD$ {totalVenta:N2} · Pago Inicial RD$ {pagoInicial:N2} · Saldo Pendiente RD$ {saldoVenta:N2}"
                    : "DETALLE DE PRODUCTOS";
            }
        }

        private void FrmHistorialVentas_Load(object sender, EventArgs e)
        {
            CORE.AppEventos.OnPagoRegistrado += ActualizarHistorial;
            CORE.AppEventos.OnDeudaModificada += ActualizarHistorial;
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
            string filtroActual = txtBuscarProductos?.Text?.Trim() ?? string.Empty;

            _bsVentasProductos.DataSource = ventasBLL.ListarVentas();
            dgvVentasProductos.DataSource = _bsVentasProductos;

            ConfigurarColumnasVentasProductos();

            if (!string.IsNullOrEmpty(filtroActual) && txtBuscarProductos != null)
                txtBuscarProductos.Text = filtroActual;

            AplicarFiltroBusquedaProductos();
            dgvVentasProductos.ClearSelection();
            dgvDetalleProductos.DataSource = null;
        }

        private void txtBuscarProductos_TextChanged(object? sender, EventArgs e)
        {
            AplicarFiltroBusquedaProductos();
        }

        private void AplicarFiltroBusquedaProductos()
        {
            if (_bsVentasProductos.DataSource == null)
                return;

            var termino = txtBuscarProductos?.Text?.Trim() ?? string.Empty;
            try
            {
                string filtro = BusquedaGridHelper.ConstruirFiltroHistorialVentasProductos(termino);
                _bsVentasProductos.Filter = string.IsNullOrEmpty(filtro) ? null : filtro;

                if (_bsVentasProductos.Count == 1 && dgvVentasProductos.Rows.Count > 0)
                {
                    dgvVentasProductos.ClearSelection();
                    dgvVentasProductos.Rows[0].Selected = true;
                    var celda = ObtenerPrimeraCeldaVisible(dgvVentasProductos.Rows[0]);
                    if (celda != null)
                        dgvVentasProductos.CurrentCell = celda;
                }
                else if (_bsVentasProductos.Count == 0)
                {
                    dgvDetalleProductos.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtro historial ventas productos: {ex.Message}");
                _bsVentasProductos.RemoveFilter();
            }
        }

        private void ConfigurarColumnasVentasProductos()
        {
            if (dgvVentasProductos.Columns.Count == 0)
                return;

            DataGridViewHelper.HideColumn(dgvVentasProductos, "ClienteId");
            DataGridViewHelper.HideColumn(dgvVentasProductos, "Telefono");
            DataGridViewHelper.HideColumn(dgvVentasProductos, "MetodoPago");

            if (dgvVentasProductos.Columns["TipoOperacion"] is DataGridViewColumn colTipo)
            {
                colTipo.HeaderText = "Operación";
                colTipo.DisplayIndex = 1;
            }

            if (dgvVentasProductos.Columns["FormaPago"] is DataGridViewColumn colForma)
            {
                colForma.HeaderText = "Forma de pago";
                colForma.DisplayIndex = 8;
            }

            if (dgvVentasProductos.Columns["Productos"] is DataGridViewColumn colProd)
            {
                colProd.HeaderText = "Productos";
                colProd.DisplayIndex = 2;
            }

            if (dgvVentasProductos.Columns["Fecha"] is DataGridViewColumn colFecha)
            {
                colFecha.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                colFecha.HeaderText = "Fecha";
            }

            foreach (var nombre in new[] { "Total", "MontoPagado", "Saldo" })
            {
                if (dgvVentasProductos.Columns[nombre] is DataGridViewColumn colMonto)
                {
                    colMonto.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                    colMonto.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            if (dgvVentasProductos.Columns["Total"] != null)
                dgvVentasProductos.Columns["Total"]!.HeaderText = "Precio Total";
            if (dgvVentasProductos.Columns["MontoPagado"] != null)
                dgvVentasProductos.Columns["MontoPagado"]!.HeaderText = "Pago Inicial / Pagado";
            if (dgvVentasProductos.Columns["Saldo"] != null)
                dgvVentasProductos.Columns["Saldo"]!.HeaderText = "Saldo Pendiente";
            if (dgvVentasProductos.Columns["Usuario"] != null)
                dgvVentasProductos.Columns["Usuario"]!.HeaderText = "Atendió";

            dgvVentasProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVentasProductos.ReadOnly = true;
            dgvVentasProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVentasProductos.RowHeadersVisible = false;
            dgvVentasProductos.CellFormatting -= DgvVentasProductos_CellFormatting;
            dgvVentasProductos.CellFormatting += DgvVentasProductos_CellFormatting;
        }

        private void DgvVentasProductos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvVentasProductos.Columns[e.ColumnIndex].Name != "TipoOperacion")
                return;

            if (dgvVentasProductos.Rows[e.RowIndex].DataBoundItem is not DataRowView fila)
                return;

            string tipo = fila["TipoOperacion"]?.ToString() ?? string.Empty;
            if (!string.Equals(tipo, "FINANCIADO", StringComparison.OrdinalIgnoreCase))
                return;

            e.CellStyle.ForeColor = Color.DarkOrange;
            e.CellStyle.Font = new Font(dgvVentasProductos.Font, FontStyle.Bold);
        }

        private void dgvVentasProductos_SelectionChanged(object sender, EventArgs e)
        {
            var val = dgvVentasProductos.CurrentRow?.Cells["Id"]?.Value;
            if (val == null || val == DBNull.Value)
            {
                dgvDetalleProductos.DataSource = null;
                if (label3 != null)
                    label3.Text = "DETALLE DE PRODUCTOS";
                return;
            }

            int ventaId = Convert.ToInt32(val);
            CargarDetalleVentaProducto(ventaId, dgvVentasProductos.CurrentRow);
        }

        private void FrmHistorialVentas_FormClosed(object sender, FormClosedEventArgs e)
        {
            CORE.AppEventos.OnPagoRegistrado -= ActualizarHistorial;
            CORE.AppEventos.OnDeudaModificada -= ActualizarHistorial;
        }
    }
}
