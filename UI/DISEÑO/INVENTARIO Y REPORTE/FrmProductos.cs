using BLL;
using BLL.Commands;
using CORE;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using UI.Helpers;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmProductos : Form
    {
        private readonly ProductoBLL productoBLL = new ProductoBLL();
        private readonly CategoriaBLL categoriaBLL = new CategoriaBLL();
        private readonly StockBLL stockBLL = new StockBLL();
        private static readonly CultureInfo CulturaDo = CultureInfo.GetCultureInfo("es-DO");
        private readonly PosScannerIntervalGate _intervaloEscanner = new();
        private readonly BindingSource _bsProductosInventario = new();
        private int _ultimoPreviewProductoId = -1;

        public FrmProductos()
        {
            InitializeComponent();
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloInventario);
            dgvProductos.DataSource = _bsProductosInventario;
            dgvProductos.DataBindingComplete += dgvProductos_DataBindingComplete;
            ConfigurarComboProductoBusqueda();
        }

        // ===============================
        // LIMPIAR MOTIVO DE SALIDA
        // ===============================
        private void LimpiarMotivo()
        {
            cmbMotivo.SelectedIndex = -1;
            txtMotivoExtra.Clear();
            txtMotivoExtra.Enabled = false;
        }

        // ===============================
        // OBTENER MOTIVO (Corregido para evitar Nulls)
        // ===============================
        private string ObtenerMotivo()
        {
            if (cmbMotivo.SelectedItem == null)
                throw new Exception("Seleccione un motivo.");

            string motivo = cmbMotivo.SelectedItem.ToString() ?? "Otro";

            if (motivo == "Otro")
            {
                if (string.IsNullOrWhiteSpace(txtMotivoExtra.Text))
                    throw new Exception("Debe escribir el motivo.");

                return txtMotivoExtra.Text.Trim();
            }

            return motivo;
        }

        private void FrmProductos_Load(object sender, EventArgs e)
        {
            CargarCategorias();
            CargarProductos();
            CargarProductosCombo();
            CargarMovimientos();
            CargarMotivos();
            txtMotivoExtra.Enabled = false;
            PrecalentarVisionIaEnSegundoPlano();
            BeginInvoke(new Action(() => txtCodigo.Focus()));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        private void CargarMotivos()
        {
            cmbMotivo.Items.Clear();
            cmbMotivo.Items.Add("Compra");
            cmbMotivo.Items.Add("Ajuste");
            cmbMotivo.Items.Add("Daño");
            cmbMotivo.Items.Add("Robo");
            cmbMotivo.Items.Add("Uso interno");
            cmbMotivo.Items.Add("Otro");
            cmbMotivo.SelectedIndex = -1;
        }

        private void CargarCategorias()
        {
            cmbCategoria.DataSource = categoriaBLL.ObtenerCategorias();
            cmbCategoria.DisplayMember = "Nombre";
            cmbCategoria.ValueMember = "Id";
            cmbCategoria.SelectedIndex = -1;
        }

        private void CargarProductos()
        {
            string? filtroPrevio = null;
            try { filtroPrevio = _bsProductosInventario.Filter; }
            catch { /* ignore */ }

            DataTable dt = productoBLL.ObtenerProductos();
            _bsProductosInventario.DataSource = dt;

            // Reaplicar búsqueda viva tras refresco (guardar/editar/eliminar).
            if (!string.IsNullOrWhiteSpace(txtBuscarProductos?.Text))
                AplicarFiltroBusquedaProductos();
            else if (!string.IsNullOrEmpty(filtroPrevio))
            {
                try { _bsProductosInventario.RemoveFilter(); }
                catch { /* ignore */ }
            }

            ActualizarKpisInventario();
        }

        private void txtBuscarProductos_TextChanged(object? sender, EventArgs e)
        {
            AplicarFiltroBusquedaProductos();
        }

        private void txtBuscarProductos_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            if (dgvProductos.Rows.Count == 0)
                return;

            dgvProductos.ClearSelection();
            dgvProductos.Rows[0].Selected = true;
            if (dgvProductos.Columns.Contains("Nombre") && dgvProductos.Rows[0].Cells["Nombre"].Visible)
                dgvProductos.CurrentCell = dgvProductos.Rows[0].Cells["Nombre"];
            dgvProductos.Focus();
        }

        /// <summary>
        /// Filtro inteligente en vivo: nombre, categoría, código de barras, precios/stock.
        /// No toca stock, combos de movimiento ni CRUD.
        /// </summary>
        private void AplicarFiltroBusquedaProductos()
        {
            if (_bsProductosInventario.DataSource == null)
                return;

            string termino = txtBuscarProductos?.Text?.Trim() ?? string.Empty;

            try
            {
                _bsProductosInventario.Filter = string.IsNullOrEmpty(termino)
                    ? null
                    : BusquedaGridHelper.ConstruirFiltroProductosPos(termino);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtro inventario productos: {ex.Message}");
                try { _bsProductosInventario.RemoveFilter(); }
                catch { /* ignore */ }
            }

            ActualizarKpisInventario();
        }

        /// <summary>
        /// KPIs desde dgvProductos (tiempo real al rebind):
        /// - Costo inventario = Σ Stock × PrecioCompra
        /// - Ganancia potencial = Σ Stock × (PrecioVenta − PrecioCompra)
        /// Solo filas con stock &gt; 0 y costos/precios válidos.
        /// </summary>
        private void ActualizarKpisInventario()
        {
            decimal costoTotal = 0m;
            decimal gananciaPotencial = 0m;

            foreach (DataGridViewRow row in dgvProductos.Rows)
            {
                if (row.IsNewRow)
                    continue;

                object? stockObj = row.Cells["StockActual"]?.Value;
                object? costoObj = row.Cells["PrecioCompra"]?.Value;
                object? ventaObj = row.Cells["PrecioVenta"]?.Value;
                if (stockObj == null || stockObj == DBNull.Value
                    || costoObj == null || costoObj == DBNull.Value)
                    continue;

                int stock = Convert.ToInt32(stockObj);
                decimal costo = Convert.ToDecimal(costoObj);
                if (stock <= 0 || costo <= 0)
                    continue;

                costoTotal += stock * costo;

                if (ventaObj != null && ventaObj != DBNull.Value)
                {
                    decimal venta = Convert.ToDecimal(ventaObj);
                    if (venta > 0)
                        gananciaPotencial += stock * (venta - costo);
                }
            }

            if (lblKpiInvVal != null && !lblKpiInvVal.IsDisposed)
                lblKpiInvVal.Text = "RD$ " + costoTotal.ToString("N2", CulturaDo);

            if (lblKpiGanVal != null && !lblKpiGanVal.IsDisposed)
                lblKpiGanVal.Text = "RD$ " + gananciaPotencial.ToString("N2", CulturaDo);
        }


        private void CargarMovimientos()
        {
            dgvMovimientos.DataSource = stockBLL.ObtenerMovimientos();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCampos()) return;

                string nombre = txtNombre.Text.Trim();
                if (productoBLL.ExisteNombre(nombre))
                {
                    MessageBox.Show("Ya existe un producto con ese nombre.");
                    return;
                }

                int categoriaId = Convert.ToInt32(cmbCategoria.SelectedValue);

                var result = ProductoCommandService.AgregarProducto(
                    nombre,
                    categoriaId,
                    decimal.Parse(txtCompra.Text),
                    decimal.Parse(txtVenta.Text),
                    0,
                    int.Parse(txtStockMinimo.Text),
                    true,
                    txtCodigo.Text,
                    null
                );

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (result.Payload is int nuevoId && nuevoId > 0)
                {
                    ConfirmarImagenTrasAlta(nuevoId);
                    string? rutaFinal = ProductoImagenStorage.ResolverRutaExistente(
                        ProductoImagenStorage.RutaProducto(nuevoId));
                    if (rutaFinal != null)
                        productoBLL.ActualizarRutaImagen(nuevoId, rutaFinal);
                }

                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefrescarTodo();
                LimpiarCampos();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un producto.");
                return;
            }

            try
            {
                if (!ValidarCampos()) return;
                int id = Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id"].Value);
                string? rutaImagen = ObtenerRutaImagenParaGuardar(id);

                var result = ProductoCommandService.EditarProducto(
                    id,
                    txtNombre.Text.Trim(),
                    Convert.ToInt32(cmbCategoria.SelectedValue),
                    decimal.Parse(txtCompra.Text),
                    decimal.Parse(txtVenta.Text),
                    int.Parse(txtStockMinimo.Text),
                    true,
                    txtCodigo.Text,
                    rutaImagen
                );

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefrescarTodo();
                LimpiarCampos();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un producto.");
                return;
            }

            int id = Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id"].Value);
            var result = ProductoCommandService.EliminarProducto(id);

            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefrescarTodo();
            LimpiarCampos();
        }

        private void dgvProductos_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            ActualizarKpisInventario();
            OcultarColumnaRutaImagenSiExiste();

            if (dgvProductos.CurrentRow != null && !dgvProductos.CurrentRow.IsNewRow)
                MostrarFotoSeleccionGrilla(dgvProductos.CurrentRow);
        }

        private void OcultarColumnaRutaImagenSiExiste()
        {
            if (dgvProductos.Columns.Contains("RutaImagen"))
                dgvProductos.Columns["RutaImagen"].Visible = false;
        }

        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            object valor = dgvProductos.Rows[e.RowIndex].Cells["StockActual"].Value;
            lblStockActual.Text = (valor != null && valor != DBNull.Value) ? valor.ToString() : "0";
            MostrarFotoSeleccionGrilla(dgvProductos.Rows[e.RowIndex]);
        }

        /// <summary>Hover: muestra la foto del producto bajo el cursor sin alterar la selección.</summary>
        private void dgvProductos_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewRow row = dgvProductos.Rows[e.RowIndex];
            if (row.IsNewRow)
                return;

            MostrarFotoSeleccionGrilla(row);
        }

        /// <summary>Flechas arriba/abajo y selección con foco en la grilla.</summary>
        private void dgvProductos_RowEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || !dgvProductos.Focused)
                return;

            DataGridViewRow row = dgvProductos.Rows[e.RowIndex];
            if (row.IsNewRow)
                return;

            MostrarFotoSeleccionGrilla(row);
        }

        private void dgvProductos_SelectionChanged(object? sender, EventArgs e)
        {
            if (!dgvProductos.Focused)
                return;

            if (dgvProductos.CurrentRow == null || dgvProductos.CurrentRow.IsNewRow)
                return;

            MostrarFotoSeleccionGrilla(dgvProductos.CurrentRow);
        }

        /// <summary>Solo preview al seleccionar fila; no toca campos ni foto pendiente de IA.</summary>
        private void MostrarFotoSeleccionGrilla(DataGridViewRow row)
        {
            if (row.IsNewRow)
                return;

            int id = 0;
            if (row.DataGridView.Columns.Contains("Id")
                && row.Cells["Id"].Value != null
                && row.Cells["Id"].Value != DBNull.Value)
            {
                id = Convert.ToInt32(row.Cells["Id"].Value);
            }

            if (id > 0 && id == _ultimoPreviewProductoId && picFotoProducto.Image != null)
                return;

            _ultimoPreviewProductoId = id;

            string? ruta = null;
            if (row.DataGridView.Columns.Contains("RutaImagen"))
                ruta = row.Cells["RutaImagen"].Value?.ToString();

            // Fallback: producto_{id}.jpg en LocalAppData si la columna viene vacía.
            if (string.IsNullOrWhiteSpace(ruta) && id > 0)
                ruta = ProductoImagenStorage.RutaProducto(id);

            MostrarFotoEnPreview(ruta);

            string nombre = row.Cells["Nombre"].Value?.ToString()?.Trim() ?? string.Empty;
            if (lblFotoaqui != null)
            {
                lblFotoaqui.Text = string.IsNullOrWhiteSpace(
                    ProductoImagenStorage.ResolverRutaExistente(ruta))
                    ? (string.IsNullOrWhiteSpace(nombre) ? "SIN FOTO" : nombre)
                    : nombre;
            }
        }

        private void dgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            CargarProductoDesdeGrilla(dgvProductos.Rows[e.RowIndex]);
        }

        private void CargarProductoDesdeGrilla(DataGridViewRow row)
        {
            txtNombre.Text = row.Cells["Nombre"].Value?.ToString() ?? string.Empty;
            txtCompra.Text = row.Cells["PrecioCompra"].Value?.ToString() ?? string.Empty;
            txtVenta.Text = row.Cells["PrecioVenta"].Value?.ToString() ?? string.Empty;
            txtStock.Text = row.Cells["StockActual"].Value?.ToString() ?? "0";
            txtStockMinimo.Text = row.Cells["StockMinimo"].Value?.ToString() ?? string.Empty;

            if (row.DataGridView.Columns.Contains("CodigoBarra"))
                txtCodigo.Text = row.Cells["CodigoBarra"].Value?.ToString() ?? string.Empty;
            else
                txtCodigo.Clear();

            object categoriaId = row.Cells["IdCategoria"].Value;
            cmbCategoria.SelectedValue = (categoriaId != null && categoriaId != DBNull.Value) ? categoriaId : -1;
            lblStockActual.Text = txtStock.Text;

            string? ruta = null;
            if (row.DataGridView.Columns.Contains("RutaImagen"))
                ruta = row.Cells["RutaImagen"].Value?.ToString();
            _rutaImagenPendiente = null;
            _jpegPendiente = null;
            MostrarFotoEnPreview(ruta);
            if (lblFotoaqui != null)
                lblFotoaqui.Text = string.IsNullOrWhiteSpace(ruta) ? "FOTO DEL PRODUCTO" : "FOTO DEL PRODUCTO";
        }

        private void CargarProductoDesdeFila(DataRow row)
        {
            txtNombre.Text = row["Nombre"]?.ToString() ?? string.Empty;
            txtCompra.Text = row["PrecioCompra"]?.ToString() ?? string.Empty;
            txtVenta.Text = row["PrecioVenta"]?.ToString() ?? string.Empty;
            txtStock.Text = row["StockActual"]?.ToString() ?? "0";
            txtStockMinimo.Text = row["StockMinimo"]?.ToString() ?? string.Empty;
            txtCodigo.Text = row.Table.Columns.Contains("CodigoBarra")
                ? row["CodigoBarra"]?.ToString() ?? string.Empty
                : string.Empty;

            object categoriaId = row["IdCategoria"];
            cmbCategoria.SelectedValue = (categoriaId != null && categoriaId != DBNull.Value) ? categoriaId : -1;
            lblStockActual.Text = txtStock.Text;

            string? ruta = row.Table.Columns.Contains("RutaImagen")
                ? row["RutaImagen"]?.ToString()
                : null;
            _rutaImagenPendiente = null;
            _jpegPendiente = null;
            MostrarFotoEnPreview(ruta);
        }

        private void SeleccionarProductoEnGrilla(int productoId)
        {
            foreach (DataGridViewRow row in dgvProductos.Rows)
            {
                if (row.IsNewRow) continue;
                object? idObj = row.Cells["Id"].Value;
                if (idObj == null || idObj == DBNull.Value) continue;
                if (Convert.ToInt32(idObj) != productoId) continue;

                row.Selected = true;
                dgvProductos.CurrentCell = row.Cells["Nombre"];
                try { dgvProductos.FirstDisplayedScrollingRowIndex = row.Index; }
                catch { /* fila fuera de rango visible */ }
                return;
            }
        }


        private void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;
            ProcesarEscaneoCodigo();
        }

        /// <summary>
        /// Lector NetumScan NS-A5 (HID): escribe en txtCodigo y envía Enter al final.
        /// Si no existe el producto, conserva el código para registrarlo al guardar.
        /// </summary>
        private void ProcesarEscaneoCodigo()
        {
            if (!_intervaloEscanner.TryAcceptScan())
                return;

            if (!ProductoBarcodeNormalizer.TryNormalizeBarcode(txtCodigo.Text, out string? codigo))
            {
                txtCodigo.Clear();
                txtCodigo.Focus();
                return;
            }

            txtCodigo.Text = codigo;
            DataRow? fila = productoBLL.BuscarPorCodigoBarra(codigo);
            if (fila == null)
            {
                txtCodigo.Text = codigo;
                txtCodigo.Focus();
                return;
            }

            int id = Convert.ToInt32(fila["Id"]);
            CargarProductoDesdeFila(fila);
            SeleccionarProductoEnGrilla(id);
            SeleccionarProductoEnCombo(id);
            numCantidad.Focus();
            numCantidad.Select(0, numCantidad.Text.Length);
        }

        private void dgvProductos_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvProductos.Rows[e.RowIndex];
            object valStock = row.Cells["StockActual"].Value;
            object valMinimo = row.Cells["StockMinimo"].Value;

            if (valStock != null && valMinimo != null && valStock != DBNull.Value && valMinimo != DBNull.Value)
            {
                int stockActual = Convert.ToInt32(valStock);
                int stockMinimo = Convert.ToInt32(valMinimo);

                row.DefaultCellStyle.BackColor = (stockActual <= stockMinimo) ? Color.LightCoral : Color.White;
            }
        }

        private void cmbProducto_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbProducto.SelectedItem is DataRowView fila)
            {
                lblStockActual.Text = fila["StockActual"]?.ToString() ?? "0";
            }
        }

        private void btnEntrada_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbProducto.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un producto.");
                    return;
                }

                int productoId = Convert.ToInt32(cmbProducto.SelectedValue);
                int cantidad = Convert.ToInt32(numCantidad.Value);
                string motivo = ObtenerMotivo();

                var result = ProductoCommandService.RegistrarEntrada(
                    productoId, cantidad, motivo, Sesion.Usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefrescarTodo();
                LimpiarMotivo();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnSalida_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbProducto.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un producto.");
                    return;
                }

                int productoId = Convert.ToInt32(cmbProducto.SelectedValue);
                int cantidad = Convert.ToInt32(numSalida.Value);
                string motivo = ObtenerMotivo();

                var result = ProductoCommandService.RegistrarSalida(
                    productoId, cantidad, motivo, Sesion.Usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefrescarTodo();
                LimpiarMotivo();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void dgvMovimientos_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            object valor = dgvMovimientos.Rows[e.RowIndex].Cells["TipoMovimiento"].Value;

            if (valor != null)
            {
                string tipo = valor.ToString() ?? string.Empty;
                if (tipo == "ENTRADA") dgvMovimientos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                else if (tipo == "SALIDA") dgvMovimientos.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
            }
        }

        private void dgvMovimientos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMovimientos.Columns[e.ColumnIndex].Name == "TipoMovimiento" && e.Value != null)
            {
                e.CellStyle.ForeColor = Color.Black;
                e.CellStyle.Font = new Font(dgvMovimientos.Font, FontStyle.Bold);
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtCompra.Text) ||
                string.IsNullOrWhiteSpace(txtVenta.Text) ||
                string.IsNullOrWhiteSpace(txtStockMinimo.Text) ||
                cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Complete todos los campos.");
                return false;
            }

            bool c1 = decimal.TryParse(txtCompra.Text, out _);
            bool c2 = decimal.TryParse(txtVenta.Text, out _);
            bool c3 = int.TryParse(txtStockMinimo.Text, out _);

            if (!c1 || !c2 || !c3)
            {
                MessageBox.Show("Asegúrese de que los precios y el stock sean números válidos.");
                return false;
            }

            return true;
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtCompra.Clear();
            txtVenta.Clear();
            txtStock.Text = "0";
            txtStockMinimo.Clear();
            txtCodigo.Clear();
            cmbCategoria.SelectedIndex = -1;
            LimpiarFotoPendiente();
            txtCodigo.Focus();
        }

        private void RefrescarTodo()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefrescarTodo));
                return;
            }

            CargarProductos();
            CargarProductosCombo();
            CargarMovimientos();
        }

        private void cmbMotivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMotivo.SelectedItem == null) return;
            string motivo = cmbMotivo.SelectedItem.ToString() ?? string.Empty;

            if (motivo == "Otro")
            {
                txtMotivoExtra.Enabled = true;
                txtMotivoExtra.Focus();
            }
            else
            {
                txtMotivoExtra.Enabled = false;
                txtMotivoExtra.Clear();
            }
        }
    }
}