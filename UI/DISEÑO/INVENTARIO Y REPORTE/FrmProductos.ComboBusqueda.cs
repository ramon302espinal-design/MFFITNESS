using System;
using System.Data;
using System.Windows.Forms;
using UI.Helpers;

namespace UI.DISEÑO
{
    public partial class FrmProductos
    {
        private const string PlaceholderComboProducto = "SELECCION DE PRODUCTOS";

        private readonly BindingSource _bsProductosCombo = new();
        private bool _cmbProductoFiltrando;
        private bool _cmbProductoComboConfigurado;

        private void ConfigurarComboProductoBusqueda()
        {
            if (_cmbProductoComboConfigurado || cmbProducto == null || cmbProducto.IsDisposed)
                return;

            _cmbProductoComboConfigurado = true;
            cmbProducto.DropDownStyle = ComboBoxStyle.DropDown;
            cmbProducto.AutoCompleteMode = AutoCompleteMode.None;
            // Confirmación real (click/Enter), no cambios por filtro BindingSource.
            cmbProducto.SelectionChangeCommitted -= cmbProducto_SelectionChangeCommitted;
            cmbProducto.SelectionChangeCommitted += cmbProducto_SelectionChangeCommitted;
            cmbProducto.Enter -= cmbProducto_Enter;
            cmbProducto.Enter += cmbProducto_Enter;
        }

        private void CargarProductosCombo()
        {
            ConfigurarComboProductoBusqueda();

            int? seleccionPrevia = null;
            if (cmbProducto.SelectedValue != null && cmbProducto.SelectedValue != DBNull.Value)
            {
                try { seleccionPrevia = Convert.ToInt32(cmbProducto.SelectedValue); }
                catch { /* ignore */ }
            }

            DataTable dt = productoBLL.ObtenerProductos();
            PrepararTablaComboProductos(dt);

            _cmbProductoFiltrando = true;
            try
            {
                _bsProductosCombo.DataSource = dt;
                cmbProducto.DataSource = _bsProductosCombo;
                cmbProducto.DisplayMember = "Nombre";
                cmbProducto.ValueMember = "Id";

                if (seleccionPrevia.HasValue)
                    SeleccionarProductoEnCombo(seleccionPrevia.Value);
                else
                {
                    cmbProducto.SelectedIndex = -1;
                    cmbProducto.Text = PlaceholderComboProducto;
                }
            }
            finally
            {
                _cmbProductoFiltrando = false;
            }
        }

        private static void PrepararTablaComboProductos(DataTable dt)
        {
            if (!dt.Columns.Contains("Siglas"))
                dt.Columns.Add("Siglas", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                string nombre = Convert.ToString(row["Nombre"]) ?? string.Empty;
                row["Siglas"] = BusquedaGridHelper.ConstruirSiglasProducto(nombre);
            }
        }

        private void AplicarFiltroComboProducto(string? termino)
        {
            if (_bsProductosCombo.DataSource == null)
                return;

            string texto = termino?.Trim() ?? string.Empty;
            if (string.Equals(texto, PlaceholderComboProducto, StringComparison.OrdinalIgnoreCase))
                texto = string.Empty;

            try
            {
                string filtro = BusquedaGridHelper.ConstruirFiltroProductosCombo(texto);
                _bsProductosCombo.Filter = string.IsNullOrEmpty(filtro) ? null : filtro;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtro combo producto: {ex.Message}");
                try { _bsProductosCombo.RemoveFilter(); }
                catch { /* ignore */ }
            }
        }

        private void RestablecerFiltroComboProducto()
        {
            try { _bsProductosCombo.RemoveFilter(); }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Quita el filtro sin perder el Id clicado (SelectedIndex se recalcula al quitar Filter).
        /// </summary>
        private void PreservarProductoTrasQuitarFiltro(int? productoId)
        {
            _cmbProductoFiltrando = true;
            try
            {
                RestablecerFiltroComboProducto();

                if (productoId is > 0)
                {
                    cmbProducto.SelectedValue = productoId.Value;
                    if (cmbProducto.SelectedItem is DataRowView row)
                    {
                        string nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                        if (!string.IsNullOrEmpty(nombre))
                            cmbProducto.Text = nombre;
                    }
                }
                else
                {
                    cmbProducto.SelectedIndex = -1;
                    cmbProducto.Text = PlaceholderComboProducto;
                }
            }
            catch
            {
                cmbProducto.SelectedIndex = -1;
                cmbProducto.Text = PlaceholderComboProducto;
            }
            finally
            {
                _cmbProductoFiltrando = false;
            }
        }

        private bool TryLeerProductoDelCombo(out int productoId, out string nombre)
        {
            productoId = 0;
            nombre = string.Empty;

            if (cmbProducto?.SelectedItem is DataRowView row)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    return false;

                productoId = Convert.ToInt32(row["Id"]);
                nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                return productoId > 0;
            }

            return false;
        }

        /// <summary>Resuelve el producto resaltado/clicado sin asumir índice 0 del filtro.</summary>
        private bool TryResolverProductoComboPreciso(out int productoId, out string nombre)
        {
            if (TryLeerProductoDelCombo(out productoId, out nombre))
                return true;

            string texto = (cmbProducto.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(texto)
                || string.Equals(texto, PlaceholderComboProducto, StringComparison.OrdinalIgnoreCase))
                return false;

            if (_bsProductosCombo.DataSource is not DataTable)
                return false;

            for (int i = 0; i < _bsProductosCombo.Count; i++)
            {
                if (_bsProductosCombo[i] is not DataRowView drv)
                    continue;

                string n = drv["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                if (!string.Equals(n, texto, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (drv["Id"] == null || drv["Id"] == DBNull.Value)
                    continue;

                productoId = Convert.ToInt32(drv["Id"]);
                nombre = n;
                if (productoId <= 0)
                    continue;

                _cmbProductoFiltrando = true;
                try { cmbProducto.SelectedIndex = i; }
                finally { _cmbProductoFiltrando = false; }
                return true;
            }

            // Un solo match en la vista filtrada → tomar ese.
            if (_bsProductosCombo.Count == 1 && _bsProductosCombo[0] is DataRowView unico)
            {
                if (unico["Id"] != null && unico["Id"] != DBNull.Value)
                {
                    productoId = Convert.ToInt32(unico["Id"]);
                    nombre = unico["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                    if (productoId > 0)
                    {
                        _cmbProductoFiltrando = true;
                        try { cmbProducto.SelectedIndex = 0; }
                        finally { _cmbProductoFiltrando = false; }
                        return true;
                    }
                }
            }

            return false;
        }

        private void cmbProducto_Enter(object? sender, EventArgs e)
        {
            if (_cmbProductoFiltrando)
                return;

            _cmbProductoFiltrando = true;
            try
            {
                if (cmbProducto.SelectedIndex < 0
                    || string.Equals(cmbProducto.Text, PlaceholderComboProducto, StringComparison.OrdinalIgnoreCase))
                {
                    cmbProducto.Text = string.Empty;
                }

                cmbProducto.SelectionStart = cmbProducto.Text.Length;
                cmbProducto.SelectionLength = 0;
            }
            finally
            {
                _cmbProductoFiltrando = false;
            }
        }

        private void cmbProducto_DropDown(object? sender, EventArgs e)
        {
            // No limpiar filtro: TextUpdate ya filtró; quitarlo haría que el click apunte a otro Id.
            _cmbProductoFiltrando = true;
            try
            {
                if (cmbProducto.SelectedIndex < 0
                    || string.Equals(cmbProducto.Text, PlaceholderComboProducto, StringComparison.OrdinalIgnoreCase))
                {
                    cmbProducto.Text = string.Empty;
                }

                cmbProducto.SelectionStart = cmbProducto.Text.Length;
                cmbProducto.SelectionLength = 0;
            }
            finally
            {
                _cmbProductoFiltrando = false;
            }
        }

        private void cmbProducto_TextUpdate(object? sender, EventArgs e)
        {
            if (_cmbProductoFiltrando)
                return;

            string texto = cmbProducto.Text ?? string.Empty;

            // El filtro dispara SelectedIndexChanged: silenciar o pinta stock del 1.er match al tipear.
            _cmbProductoFiltrando = true;
            try
            {
                AplicarFiltroComboProducto(texto);
                if (!cmbProducto.DroppedDown)
                    cmbProducto.DroppedDown = true;

                cmbProducto.SelectedIndex = -1;
                cmbProducto.Text = texto;
                cmbProducto.SelectionStart = texto.Length;
                cmbProducto.SelectionLength = 0;
            }
            finally
            {
                _cmbProductoFiltrando = false;
            }
        }

        private void cmbProducto_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (!TryResolverProductoComboPreciso(out int productoId, out _))
                return;

            PreservarProductoTrasQuitarFiltro(productoId);
            cmbProducto.DroppedDown = false;
            ActualizarStockLabelDesdeCombo();
        }

        private void cmbProducto_DropDownClosed(object? sender, EventArgs e)
        {
            int? id = null;
            if (TryLeerProductoDelCombo(out int productoId, out _))
                id = productoId;

            PreservarProductoTrasQuitarFiltro(id);
            if (id.HasValue)
                ActualizarStockLabelDesdeCombo();
        }

        private void cmbProducto_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_cmbProductoFiltrando)
                return;

            if (!TryResolverProductoComboPreciso(out int productoId, out _))
                return;

            PreservarProductoTrasQuitarFiltro(productoId);
            ActualizarStockLabelDesdeCombo();
        }

        private void ActualizarStockLabelDesdeCombo()
        {
            if (cmbProducto.SelectedItem is DataRowView fila)
                lblStockActual.Text = fila["StockActual"]?.ToString() ?? "0";
        }

        private void SeleccionarProductoEnCombo(int productoId)
        {
            _cmbProductoFiltrando = true;
            try
            {
                RestablecerFiltroComboProducto();
                cmbProducto.SelectedValue = productoId;
            }
            catch
            {
                cmbProducto.SelectedIndex = -1;
                cmbProducto.Text = PlaceholderComboProducto;
            }
            finally
            {
                _cmbProductoFiltrando = false;
            }
        }
    }
}
