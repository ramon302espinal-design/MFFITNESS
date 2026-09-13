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
            // Ventas de producto (incl. despacho saldo a favor / VentaSinCaja → OnPagoRegistrado).
            CargarVentas();
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
            DataTable detalle = ventasBLL.ListarDetalleVenta(ventaId);

            decimal interesTotal = 0m;
            DataRowView? fila = filaVenta?.DataBoundItem as DataRowView
                ?? dgvVentasProductos.CurrentRow?.DataBoundItem as DataRowView;

            if (fila?.Row.Table.Columns.Contains("InteresTotal") == true
                && fila["InteresTotal"] != DBNull.Value)
            {
                interesTotal = Convert.ToDecimal(fila["InteresTotal"]);
            }

            AplicarInteresProporcionalDetalle(detalle, interesTotal);

            dgvDetalleProductos.DataSource = detalle;
            ConfigurarColumnasDetalleProductos();
            ActualizarTotalDetalleProductos(detalle);

            if (label3 != null)
                label3.Text = "DETALLE DE PRODUCTOS";
        }

        private void ActualizarTotalDetalleProductos(DataTable? detalle)
        {
            if (lblTotalDetalleProductos == null || lblTotalDetalleProductos.IsDisposed)
                return;

            decimal total = 0m;
            if (detalle != null && detalle.Columns.Contains("PrecioTotal"))
            {
                foreach (DataRow row in detalle.Rows)
                {
                    if (row["PrecioTotal"] != DBNull.Value && row["PrecioTotal"] != null)
                        total += Convert.ToDecimal(row["PrecioTotal"]);
                }
            }
            else if (detalle != null && detalle.Columns.Contains("Subtotal"))
            {
                foreach (DataRow row in detalle.Rows)
                {
                    if (row["Subtotal"] != DBNull.Value && row["Subtotal"] != null)
                        total += Convert.ToDecimal(row["Subtotal"]);
                }
            }

            lblTotalDetalleProductos.Text = $"TOTAL: RD$ {total:N2}";
        }

        private void LimpiarDetalleProductosUi()
        {
            dgvDetalleProductos.DataSource = null;
            ActualizarTotalDetalleProductos(null);
            if (label3 != null)
                label3.Text = "DETALLE DE PRODUCTOS";
        }

        /// <summary>
        /// Reparte el interés fijo del préstamo entre líneas (proporcional al subtotal),
        /// para que Precio Total de detalle = producto + interés (misma regla del historial).
        /// </summary>
        private static void AplicarInteresProporcionalDetalle(DataTable detalle, decimal interesTotal)
        {
            if (detalle == null)
                return;

            if (!detalle.Columns.Contains("Interes"))
                detalle.Columns.Add("Interes", typeof(decimal));
            if (!detalle.Columns.Contains("PrecioTotal"))
                detalle.Columns.Add("PrecioTotal", typeof(decimal));

            interesTotal = decimal.Round(Math.Max(0m, interesTotal), 2);

            decimal sumaSubtotales = 0m;
            foreach (DataRow row in detalle.Rows)
            {
                if (row["Subtotal"] != DBNull.Value)
                    sumaSubtotales += Convert.ToDecimal(row["Subtotal"]);
            }

            decimal interesAsignado = 0m;
            for (int i = 0; i < detalle.Rows.Count; i++)
            {
                DataRow row = detalle.Rows[i];
                decimal subtotal = row["Subtotal"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Subtotal"]);
                decimal interesLinea;

                if (interesTotal <= 0m || sumaSubtotales <= 0m)
                {
                    interesLinea = 0m;
                }
                else if (i == detalle.Rows.Count - 1)
                {
                    interesLinea = decimal.Round(interesTotal - interesAsignado, 2);
                }
                else
                {
                    interesLinea = decimal.Round(interesTotal * (subtotal / sumaSubtotales), 2);
                    interesAsignado += interesLinea;
                }

                row["Interes"] = interesLinea;
                row["PrecioTotal"] = decimal.Round(subtotal + interesLinea, 2);
            }
        }

        private void ConfigurarColumnasDetalleProductos()
        {
            if (dgvDetalleProductos.Columns.Count == 0)
                return;

            DataGridViewHelper.RunColumnLayout(dgvDetalleProductos, () =>
            {
                DataGridViewHelper.ConfigureColumn(dgvDetalleProductos, "Producto", col =>
                {
                    col.HeaderText = "Producto";
                });

                DataGridViewHelper.ConfigureColumn(dgvDetalleProductos, "Cantidad", col =>
                {
                    col.HeaderText = "Cant.";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    DataGridViewHelper.SetColumnWidth(col, 70);
                });

                DataGridViewHelper.ConfigureColumn(dgvDetalleProductos, "Precio", col =>
                {
                    col.HeaderText = "Precio";
                    col.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                });

                DataGridViewHelper.ConfigureColumn(dgvDetalleProductos, "Subtotal", col =>
                {
                    col.HeaderText = "Subtotal";
                    col.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                });

                DataGridViewHelper.ConfigureColumn(dgvDetalleProductos, "Interes", col =>
                {
                    col.HeaderText = "Interés";
                    col.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    DataGridViewHelper.SetColumnWidth(col, 90);
                });

                DataGridViewHelper.ConfigureColumn(dgvDetalleProductos, "PrecioTotal", col =>
                {
                    col.HeaderText = "Precio Total";
                    col.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.DefaultCellStyle.Font = new Font(dgvDetalleProductos.Font, FontStyle.Bold);
                });

                DataGridViewHelper.SetDisplayIndex(dgvDetalleProductos, "Producto", 0);
                DataGridViewHelper.SetDisplayIndex(dgvDetalleProductos, "Cantidad", 1);
                DataGridViewHelper.SetDisplayIndex(dgvDetalleProductos, "Precio", 2);
                DataGridViewHelper.SetDisplayIndex(dgvDetalleProductos, "Subtotal", 3);
                DataGridViewHelper.SetDisplayIndex(dgvDetalleProductos, "Interes", 4);
                DataGridViewHelper.SetDisplayIndex(dgvDetalleProductos, "PrecioTotal", 5);
            }, restoreFill: true);

            dgvDetalleProductos.ReadOnly = true;
            dgvDetalleProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalleProductos.MultiSelect = false;
            dgvDetalleProductos.RowHeadersVisible = false;
            dgvDetalleProductos.AllowUserToAddRows = false;
            dgvDetalleProductos.AllowUserToDeleteRows = false;
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
            int? ventaIdSeleccionada = null;
            if (dgvVentasProductos.CurrentRow?.Cells["Id"]?.Value != null
                && dgvVentasProductos.CurrentRow.Cells["Id"].Value != DBNull.Value)
            {
                ventaIdSeleccionada = Convert.ToInt32(dgvVentasProductos.CurrentRow.Cells["Id"].Value);
            }

            DataTable dt = ventasBLL.ListarVentas();
            if (!dt.Columns.Contains("FaltaCuotaTexto"))
                dt.Columns.Add("FaltaCuotaTexto", typeof(string));

            foreach (DataRow row in dt.Rows)
                row["FaltaCuotaTexto"] = ConstruirTextoFaltaCuotaVenta(row);

            _bsVentasProductos.DataSource = dt;
            dgvVentasProductos.DataSource = _bsVentasProductos;

            ConfigurarColumnasVentasProductos();

            if (!string.IsNullOrEmpty(filtroActual) && txtBuscarProductos != null)
                txtBuscarProductos.Text = filtroActual;

            AplicarFiltroBusquedaProductos();

            if (ventaIdSeleccionada.HasValue
                && RestaurarSeleccionVentaProducto(ventaIdSeleccionada.Value))
            {
                // Selección y detalle restaurados tras refresh (pago/deuda).
            }
            else
            {
                dgvVentasProductos.ClearSelection();
                LimpiarDetalleProductosUi();
            }
        }

        /// <summary>Restaura fila tras refresh sin limpiar el filtro de búsqueda.</summary>
        private bool RestaurarSeleccionVentaProducto(int ventaId)
        {
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

                CargarDetalleVentaProducto(ventaId, row);
                return true;
            }

            return false;
        }

        /// <summary>Misma regla visual que Gestión de Deudas (abonos vs cronograma).</summary>
        private static string ConstruirTextoFaltaCuotaVenta(DataRow row)
        {
            if (row.Table.Columns.Contains("FaltaEstaCuota")
                && row["FaltaEstaCuota"] != DBNull.Value
                && row["FaltaEstaCuota"] != null)
            {
                decimal falta = Convert.ToDecimal(row["FaltaEstaCuota"]);
                if (falta > 0m)
                    return $"Falta RD$ {falta:N2} de esta cuota";
            }

            if ((!row.Table.Columns.Contains("PrestamoId")
                 || row["PrestamoId"] == DBNull.Value
                 || row["PrestamoId"] == null)
                && row.Table.Columns.Contains("Saldo")
                && row["Saldo"] != DBNull.Value)
            {
                decimal saldo = Convert.ToDecimal(row["Saldo"]);
                if (saldo > 0m)
                    return $"Falta RD$ {saldo:N2}";
            }

            return string.Empty;
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
                    LimpiarDetalleProductosUi();
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

            // Anchos fijos + AutoSize None → scroll horizontal para ver todos los headers.
            DataGridViewHelper.RunColumnLayout(dgvVentasProductos, () =>
            {
                DataGridViewHelper.HideColumn(dgvVentasProductos, "Saldo");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "MontoPagado");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "PagoInicialFinanciamiento");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "PrecioProducto");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "InteresTotal");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "TotalConInteres");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "CuotaBase");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "ProximaCuotaNumero");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "ProximaCuotaFecha");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "FaltaCuotaTexto");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "ProximaCuotaMonto");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "FaltaEstaCuota");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "DeudaId");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "PrestamoId");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "ClienteId");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "Telefono");
                DataGridViewHelper.HideColumn(dgvVentasProductos, "MetodoPago");

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "Id", col =>
                {
                    DataGridViewHelper.SetColumnWidth(col, 55);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "TipoOperacion", col =>
                {
                    col.HeaderText = "Operación";
                    DataGridViewHelper.SetColumnWidth(col, 100);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "Cliente", col =>
                {
                    DataGridViewHelper.SetColumnWidth(col, 160);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "Productos", col =>
                {
                    col.HeaderText = "Productos";
                    DataGridViewHelper.SetColumnWidth(col, 220);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "FrecuenciaPrestamo", col =>
                {
                    col.HeaderText = "Frecuencia";
                    DataGridViewHelper.SetColumnWidth(col, 95);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "NumeroPlazos", col =>
                {
                    col.HeaderText = "Plazos";
                    DataGridViewHelper.SetColumnWidth(col, 60);
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "InteresPorcentaje", col =>
                {
                    col.HeaderText = "Interés %";
                    col.DefaultCellStyle.Format = "N2";
                    DataGridViewHelper.SetColumnWidth(col, 75);
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "MoraPrestamo", col =>
                {
                    col.HeaderText = "Mora";
                    DataGridViewHelper.SetColumnWidth(col, 55);
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "Fecha", col =>
                {
                    col.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    col.HeaderText = "Fecha";
                    DataGridViewHelper.SetColumnWidth(col, 130);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "PrecioTotal", col =>
                {
                    col.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.HeaderText = "Precio Total";
                    DataGridViewHelper.SetColumnWidth(col, 110);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "PagoInicial", col =>
                {
                    col.DefaultCellStyle.Format = MonedaHelper.FormatoGridRd;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.HeaderText = "Pago Inicial";
                    DataGridViewHelper.SetColumnWidth(col, 120);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "FormaPago", col =>
                {
                    col.HeaderText = "Forma de pago";
                    DataGridViewHelper.SetColumnWidth(col, 220);
                });

                DataGridViewHelper.ConfigureColumn(dgvVentasProductos, "Usuario", col =>
                {
                    col.HeaderText = "Atendió";
                    DataGridViewHelper.SetColumnWidth(col, 110);
                });

                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "Id", 0);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "TipoOperacion", 1);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "Cliente", 2);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "Productos", 3);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "FrecuenciaPrestamo", 4);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "NumeroPlazos", 5);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "InteresPorcentaje", 6);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "MoraPrestamo", 7);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "Fecha", 8);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "PrecioTotal", 9);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "PagoInicial", 10);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "FormaPago", 11);
                DataGridViewHelper.SetDisplayIndex(dgvVentasProductos, "Usuario", 12);
            }, restoreFill: false);

            dgvVentasProductos.ScrollBars = ScrollBars.Both;
            dgvVentasProductos.ReadOnly = true;
            dgvVentasProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVentasProductos.MultiSelect = false;
            dgvVentasProductos.RowHeadersVisible = false;
            dgvVentasProductos.AllowUserToAddRows = false;
            dgvVentasProductos.AllowUserToDeleteRows = false;
            dgvVentasProductos.CellFormatting -= DgvVentasProductos_CellFormatting;
            dgvVentasProductos.CellFormatting += DgvVentasProductos_CellFormatting;
        }

        private void DgvVentasProductos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string nombreColumna = dgvVentasProductos.Columns[e.ColumnIndex].Name;

            if (nombreColumna == "TipoOperacion"
                && dgvVentasProductos.Rows[e.RowIndex].DataBoundItem is DataRowView filaTipo)
            {
                string tipo = filaTipo["TipoOperacion"]?.ToString() ?? string.Empty;
                if (string.Equals(tipo, "FINANCIADO", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.CellStyle.Font = new Font(dgvVentasProductos.Font, FontStyle.Bold);
                }
            }

            if (nombreColumna == "FrecuenciaPrestamo" && e.Value != null
                && !string.IsNullOrWhiteSpace(e.Value.ToString()))
            {
                e.CellStyle.ForeColor = Color.FromArgb(0x0D, 0x47, 0xA1);
                e.CellStyle.Font = new Font(dgvVentasProductos.Font, FontStyle.Bold);
            }

            if (nombreColumna == "MoraPrestamo" && e.Value != null)
            {
                string mora = e.Value.ToString() ?? string.Empty;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (string.Equals(mora, "Sí", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.DarkRed;
                    e.CellStyle.Font = new Font(dgvVentasProductos.Font, FontStyle.Bold);
                }
            }

        }

        private void dgvVentasProductos_SelectionChanged(object sender, EventArgs e)
        {
            var val = dgvVentasProductos.CurrentRow?.Cells["Id"]?.Value;
            if (val == null || val == DBNull.Value)
            {
                LimpiarDetalleProductosUi();
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
