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

        private void cmbProducto_DropDown(object? sender, EventArgs e)
        {
            RestablecerFiltroComboProducto();

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
            AplicarFiltroComboProducto(texto);

            _cmbProductoFiltrando = true;
            try
            {
                cmbProducto.DroppedDown = true;
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

            if (cmbProducto.SelectedIndex < 0 && _bsProductosCombo.Count > 0)
                cmbProducto.SelectedIndex = 0;

            RestablecerFiltroComboProducto();
            cmbProducto.DroppedDown = false;
        }

        private void cmbProducto_DropDownClosed(object? sender, EventArgs e)
        {
            RestablecerFiltroComboProducto();

            if (cmbProducto.SelectedIndex >= 0)
                return;

            _cmbProductoFiltrando = true;
            try
            {
                cmbProducto.Text = PlaceholderComboProducto;
            }
            finally
            {
                _cmbProductoFiltrando = false;
            }
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
