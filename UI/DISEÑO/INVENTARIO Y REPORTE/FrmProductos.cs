using BLL;
using BLL.Commands;
using CORE;
using System;
using System.Data;
using System.Drawing;
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

        public FrmProductos()
        {
            InitializeComponent();
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloInventario);
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

        // =============================
        // EVENTO PARA LIMPIAR AL HACER CLIC EN LA PESTAÑA
        // =============================
        private void tabProductos_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
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
            dgvProductos.DataSource = productoBLL.ObtenerProductos();
        }

        private void CargarProductosCombo()
        {
            cmbProducto.DataSource = null;
            cmbProducto.DataSource = productoBLL.ObtenerProductos();
            cmbProducto.DisplayMember = "Nombre";
            cmbProducto.ValueMember = "Id";
            cmbProducto.SelectedIndex = -1;
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
                    true
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

                var result = ProductoCommandService.EditarProducto(
                    id,
                    txtNombre.Text.Trim(),
                    Convert.ToInt32(cmbCategoria.SelectedValue),
                    decimal.Parse(txtCompra.Text),
                    decimal.Parse(txtVenta.Text),
                    int.Parse(txtStockMinimo.Text),
                    true
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

        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            object valor = dgvProductos.Rows[e.RowIndex].Cells["StockActual"].Value;
            lblStockActual.Text = (valor != null && valor != DBNull.Value) ? valor.ToString() : "0";
        }

        private void dgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvProductos.Rows[e.RowIndex];

            txtNombre.Text = row.Cells["Nombre"].Value?.ToString() ?? string.Empty;
            txtCompra.Text = row.Cells["PrecioCompra"].Value?.ToString() ?? string.Empty;
            txtVenta.Text = row.Cells["PrecioVenta"].Value?.ToString() ?? string.Empty;
            txtStock.Text = row.Cells["StockActual"].Value?.ToString() ?? "0";
            txtStockMinimo.Text = row.Cells["StockMinimo"].Value?.ToString() ?? string.Empty;

            object categoriaId = row.Cells["IdCategoria"].Value;
            cmbCategoria.SelectedValue = (categoriaId != null && categoriaId != DBNull.Value) ? categoriaId : -1;
            lblStockActual.Text = txtStock.Text;
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

        private void cmbProducto_SelectedIndexChanged(object sender, EventArgs e)
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
            cmbCategoria.SelectedIndex = -1;
            txtNombre.Focus();
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