using System;
using System.Data;
using System.Windows.Forms;
using UI.Helpers;

namespace UI
{
    /// <summary>
    /// cmbbuscarproductos: solo suplementos del inventario + búsqueda inteligente
    /// (mismo patrón que FrmProductos). Producto a crédito no admite otras categorías.
    /// </summary>
    public partial class FrmModuloDeudas
    {
        private const string PlaceholderComboBuscarProducto = "Buscar suplemento…";

        private readonly BindingSource _bsBuscarProductos = new();
        private bool _cmbBuscarProductoFiltrando;
        private bool _cmbBuscarProductoConfigurado;

        private void ConfigurarComboBuscarProductos()
        {
            if (_cmbBuscarProductoConfigurado || cmbbuscarproductos == null || cmbbuscarproductos.IsDisposed)
                return;

            _cmbBuscarProductoConfigurado = true;
            cmbbuscarproductos.DropDownStyle = ComboBoxStyle.DropDown;
            cmbbuscarproductos.AutoCompleteMode = AutoCompleteMode.None;
            cmbbuscarproductos.IntegralHeight = false;
            cmbbuscarproductos.MaxDropDownItems = 16;

            cmbbuscarproductos.SelectionChangeCommitted -= cmbbuscarproductos_SelectionChangeCommitted;
            cmbbuscarproductos.SelectionChangeCommitted += cmbbuscarproductos_SelectionChangeCommitted;
            cmbbuscarproductos.Enter -= cmbbuscarproductos_Enter;
            cmbbuscarproductos.Enter += cmbbuscarproductos_Enter;
            cmbbuscarproductos.DropDown -= cmbbuscarproductos_DropDown;
            cmbbuscarproductos.DropDown += cmbbuscarproductos_DropDown;
            cmbbuscarproductos.TextUpdate -= cmbbuscarproductos_TextUpdate;
            cmbbuscarproductos.TextUpdate += cmbbuscarproductos_TextUpdate;
            cmbbuscarproductos.DropDownClosed -= cmbbuscarproductos_DropDownClosed;
            cmbbuscarproductos.DropDownClosed += cmbbuscarproductos_DropDownClosed;
            cmbbuscarproductos.KeyDown -= cmbbuscarproductos_KeyDown;
            cmbbuscarproductos.KeyDown += cmbbuscarproductos_KeyDown;
        }

        private void CargarProductosEnComboBusqueda()
        {
            ConfigurarComboBuscarProductos();

            DataTable dt = FiltrarProductosSoloSuplementos(_productos);
            PrepararTablaComboBuscarProductos(dt);

            _cmbBuscarProductoFiltrando = true;
            try
            {
                _bsBuscarProductos.DataSource = dt;
                cmbbuscarproductos.DataSource = _bsBuscarProductos;
                cmbbuscarproductos.DisplayMember = "Nombre";
                cmbbuscarproductos.ValueMember = "Id";
                RestablecerComboBuscarProductos();
            }
            finally
            {
                _cmbBuscarProductoFiltrando = false;
            }
        }

        /// <summary>Copia solo filas de categoría Suplemento(s) para el buscador de crédito.</summary>
        private static DataTable FiltrarProductosSoloSuplementos(DataTable fuente)
        {
            if (fuente == null)
                return new DataTable();

            DataTable dt = fuente.Clone();
            if (!fuente.Columns.Contains("Categoria"))
                return dt;

            foreach (DataRow row in fuente.Rows)
            {
                if (EsCategoriaSuplemento(row))
                    dt.ImportRow(row);
            }

            return dt;
        }

        private static void PrepararTablaComboBuscarProductos(DataTable dt)
        {
            if (!dt.Columns.Contains("Siglas"))
                dt.Columns.Add("Siglas", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                string nombre = Convert.ToString(row["Nombre"]) ?? string.Empty;
                row["Siglas"] = BusquedaGridHelper.ConstruirSiglasProducto(nombre);
            }
        }

        private void AplicarFiltroComboBuscarProductos(string? termino)
        {
            if (_bsBuscarProductos.DataSource == null)
                return;

            string texto = termino?.Trim() ?? string.Empty;
            if (string.Equals(texto, PlaceholderComboBuscarProducto, StringComparison.OrdinalIgnoreCase))
                texto = string.Empty;

            try
            {
                string filtro = BusquedaGridHelper.ConstruirFiltroProductosCombo(texto);
                _bsBuscarProductos.Filter = string.IsNullOrEmpty(filtro) ? null : filtro;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtro combo producto (deudas): {ex.Message}");
                try { _bsBuscarProductos.RemoveFilter(); }
                catch { /* ignore */ }
            }
        }

        private void RestablecerFiltroComboBuscarProductos()
        {
            try { _bsBuscarProductos.RemoveFilter(); }
            catch { /* ignore */ }
        }

        private void RestablecerComboBuscarProductos()
        {
            _cmbBuscarProductoFiltrando = true;
            try
            {
                RestablecerFiltroComboBuscarProductos();
                cmbbuscarproductos.SelectedIndex = -1;
                cmbbuscarproductos.Text = PlaceholderComboBuscarProducto;
            }
            catch
            {
                try
                {
                    cmbbuscarproductos.SelectedIndex = -1;
                    cmbbuscarproductos.Text = PlaceholderComboBuscarProducto;
                }
                catch { /* ignore */ }
            }
            finally
            {
                _cmbBuscarProductoFiltrando = false;
            }
        }

        private void PreservarProductoComboTrasQuitarFiltro(int? productoId)
        {
            _cmbBuscarProductoFiltrando = true;
            try
            {
                RestablecerFiltroComboBuscarProductos();

                if (productoId is > 0)
                {
                    cmbbuscarproductos.SelectedValue = productoId.Value;
                    if (cmbbuscarproductos.SelectedItem is DataRowView row)
                    {
                        string nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                        if (!string.IsNullOrEmpty(nombre))
                            cmbbuscarproductos.Text = nombre;
                    }
                }
                else
                {
                    cmbbuscarproductos.SelectedIndex = -1;
                    cmbbuscarproductos.Text = PlaceholderComboBuscarProducto;
                }
            }
            catch
            {
                cmbbuscarproductos.SelectedIndex = -1;
                cmbbuscarproductos.Text = PlaceholderComboBuscarProducto;
            }
            finally
            {
                _cmbBuscarProductoFiltrando = false;
            }
        }

        private bool TryLeerProductoDelComboBusqueda(out int productoId, out DataRow? fila)
        {
            productoId = 0;
            fila = null;

            if (cmbbuscarproductos?.SelectedItem is DataRowView row)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    return false;

                productoId = Convert.ToInt32(row["Id"]);
                fila = row.Row;
                return productoId > 0;
            }

            return false;
        }

        private bool TryResolverProductoComboBusqueda(out int productoId, out DataRow? fila)
        {
            if (TryLeerProductoDelComboBusqueda(out productoId, out fila))
                return true;

            string texto = (cmbbuscarproductos.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(texto)
                || string.Equals(texto, PlaceholderComboBuscarProducto, StringComparison.OrdinalIgnoreCase))
                return false;

            if (_bsBuscarProductos.DataSource is not DataTable)
                return false;

            for (int i = 0; i < _bsBuscarProductos.Count; i++)
            {
                if (_bsBuscarProductos[i] is not DataRowView drv)
                    continue;

                string n = drv["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                if (!string.Equals(n, texto, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (drv["Id"] == null || drv["Id"] == DBNull.Value)
                    continue;

                productoId = Convert.ToInt32(drv["Id"]);
                fila = drv.Row;
                if (productoId <= 0)
                    continue;

                _cmbBuscarProductoFiltrando = true;
                try { cmbbuscarproductos.SelectedIndex = i; }
                finally { _cmbBuscarProductoFiltrando = false; }
                return true;
            }

            if (_bsBuscarProductos.Count == 1 && _bsBuscarProductos[0] is DataRowView unico)
            {
                if (unico["Id"] != null && unico["Id"] != DBNull.Value)
                {
                    productoId = Convert.ToInt32(unico["Id"]);
                    fila = unico.Row;
                    if (productoId > 0)
                    {
                        _cmbBuscarProductoFiltrando = true;
                        try { cmbbuscarproductos.SelectedIndex = 0; }
                        finally { _cmbBuscarProductoFiltrando = false; }
                        return true;
                    }
                }
            }

            return false;
        }

        private void cmbbuscarproductos_Enter(object? sender, EventArgs e)
        {
            if (_cmbBuscarProductoFiltrando)
                return;

            _cmbBuscarProductoFiltrando = true;
            try
            {
                if (cmbbuscarproductos.SelectedIndex < 0
                    || string.Equals(cmbbuscarproductos.Text, PlaceholderComboBuscarProducto, StringComparison.OrdinalIgnoreCase))
                {
                    cmbbuscarproductos.Text = string.Empty;
                }

                cmbbuscarproductos.SelectionStart = cmbbuscarproductos.Text.Length;
                cmbbuscarproductos.SelectionLength = 0;
            }
            finally
            {
                _cmbBuscarProductoFiltrando = false;
            }
        }

        private void cmbbuscarproductos_DropDown(object? sender, EventArgs e)
        {
            _cmbBuscarProductoFiltrando = true;
            try
            {
                if (cmbbuscarproductos.SelectedIndex < 0
                    || string.Equals(cmbbuscarproductos.Text, PlaceholderComboBuscarProducto, StringComparison.OrdinalIgnoreCase))
                {
                    cmbbuscarproductos.Text = string.Empty;
                }

                // Sin texto: mostrar suplementos del inventario (filtrados al cargar).
                if (string.IsNullOrWhiteSpace(cmbbuscarproductos.Text))
                    RestablecerFiltroComboBuscarProductos();

                cmbbuscarproductos.SelectionStart = cmbbuscarproductos.Text.Length;
                cmbbuscarproductos.SelectionLength = 0;
            }
            finally
            {
                _cmbBuscarProductoFiltrando = false;
            }
        }

        private void cmbbuscarproductos_TextUpdate(object? sender, EventArgs e)
        {
            if (_cmbBuscarProductoFiltrando)
                return;

            string texto = cmbbuscarproductos.Text ?? string.Empty;

            _cmbBuscarProductoFiltrando = true;
            try
            {
                AplicarFiltroComboBuscarProductos(texto);
                if (!cmbbuscarproductos.DroppedDown)
                    cmbbuscarproductos.DroppedDown = true;

                cmbbuscarproductos.SelectedIndex = -1;
                cmbbuscarproductos.Text = texto;
                cmbbuscarproductos.SelectionStart = texto.Length;
                cmbbuscarproductos.SelectionLength = 0;
            }
            finally
            {
                _cmbBuscarProductoFiltrando = false;
            }
        }

        private void cmbbuscarproductos_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (!TryResolverProductoComboBusqueda(out int productoId, out DataRow? fila) || fila == null)
                return;

            PreservarProductoComboTrasQuitarFiltro(productoId);
            cmbbuscarproductos.DroppedDown = false;
            ConfirmarProductoSeleccionadoDesdeCombo(fila);
        }

        private void cmbbuscarproductos_DropDownClosed(object? sender, EventArgs e)
        {
            if (_cmbBuscarProductoFiltrando)
                return;

            int? id = null;
            DataRow? fila = null;
            if (TryLeerProductoDelComboBusqueda(out int productoId, out DataRow? f) && f != null)
            {
                id = productoId;
                fila = f;
            }

            PreservarProductoComboTrasQuitarFiltro(id);
            if (fila != null)
                ConfirmarProductoSeleccionadoDesdeCombo(fila);
        }

        private void cmbbuscarproductos_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_cmbBuscarProductoFiltrando)
                return;

            if (!TryResolverProductoComboBusqueda(out int productoId, out DataRow? fila) || fila == null)
                return;

            PreservarProductoComboTrasQuitarFiltro(productoId);
            ConfirmarProductoSeleccionadoDesdeCombo(fila);
        }
    }
}
