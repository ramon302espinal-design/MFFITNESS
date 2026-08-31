using System;
using System.Data;
using System.Windows.Forms;
using UI.Helpers;

namespace UI.DISEÑO
{
    public partial class FrmPagos
    {
        private const string PlaceholderClientePausa = "Buscar miembro...";

        private readonly BindingSource _bsClientePausa = new();
        private bool _cmbPausaFiltrando;
        private bool _cmbPausaComboConfigurado;

        private void ConfigurarComboClientePausaBusqueda()
        {
            if (_cmbPausaComboConfigurado || cmbClientePausarVenta == null || cmbClientePausarVenta.IsDisposed)
                return;

            _cmbPausaComboConfigurado = true;
            cmbClientePausarVenta.DropDownStyle = ComboBoxStyle.DropDown;
            cmbClientePausarVenta.AutoCompleteMode = AutoCompleteMode.None;
            // Solo confirma selección real del usuario (click / Enter), no cambios por filtro BindingSource.
            cmbClientePausarVenta.SelectionChangeCommitted -= cmbClientePausarVenta_SelectionChangeCommitted;
            cmbClientePausarVenta.SelectionChangeCommitted += cmbClientePausarVenta_SelectionChangeCommitted;
        }

        private void CargarComboClientePausarVenta(DataTable dtOrigen)
        {
            if (cmbClientePausarVenta == null || cmbClientePausarVenta.IsDisposed)
                return;

            ConfigurarComboClientePausaBusqueda();

            int? seleccionPrevia = null;
            if (cmbClientePausarVenta.SelectedValue != null && cmbClientePausarVenta.SelectedValue != DBNull.Value)
            {
                try { seleccionPrevia = Convert.ToInt32(cmbClientePausarVenta.SelectedValue); }
                catch { /* ignore */ }
            }

            DataTable dt = dtOrigen.Copy();
            PrepararTablaComboClientesPausa(dt);

            _suppressPausaUi = true;
            _cmbPausaFiltrando = true;
            try
            {
                _bsClientePausa.DataSource = dt;
                cmbClientePausarVenta.DataSource = _bsClientePausa;
                cmbClientePausarVenta.DisplayMember = "Nombre";
                cmbClientePausarVenta.ValueMember = "Id";

                if (seleccionPrevia.HasValue)
                    SeleccionarClientePausaEnCombo(seleccionPrevia.Value);
                else
                {
                    cmbClientePausarVenta.SelectedIndex = -1;
                    cmbClientePausarVenta.Text = PlaceholderClientePausa;
                }
            }
            finally
            {
                _cmbPausaFiltrando = false;
                _suppressPausaUi = false;
            }
        }

        private static void PrepararTablaComboClientesPausa(DataTable dt)
        {
            if (!dt.Columns.Contains("Siglas"))
                dt.Columns.Add("Siglas", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                string nombre = Convert.ToString(row["Nombre"]) ?? string.Empty;
                row["Siglas"] = BusquedaGridHelper.ConstruirSiglasProducto(nombre);
            }
        }

        private void AplicarFiltroComboClientePausa(string? termino)
        {
            if (_bsClientePausa.DataSource == null)
                return;

            string texto = termino?.Trim() ?? string.Empty;
            if (string.Equals(texto, PlaceholderClientePausa, StringComparison.OrdinalIgnoreCase))
                texto = string.Empty;

            try
            {
                string filtro = BusquedaGridHelper.ConstruirFiltroClientesCombo(texto);
                _bsClientePausa.Filter = string.IsNullOrEmpty(filtro) ? null : filtro;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtro combo pausa cliente: {ex.Message}");
                try { _bsClientePausa.RemoveFilter(); }
                catch { /* ignore */ }
            }
        }

        private void RestablecerFiltroComboClientePausa()
        {
            try { _bsClientePausa.RemoveFilter(); }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Quitar el filtro del BindingSource sin perder el Id clicado (SelectedIndex se recalcula).
        /// </summary>
        private void PreservarClientePausaTrasQuitarFiltro(int? clienteId)
        {
            _cmbPausaFiltrando = true;
            _suppressPausaUi = true;
            try
            {
                RestablecerFiltroComboClientePausa();

                if (clienteId is > 0)
                {
                    cmbClientePausarVenta.SelectedValue = clienteId.Value;
                    if (cmbClientePausarVenta.SelectedItem is DataRowView row)
                    {
                        string nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                        if (!string.IsNullOrEmpty(nombre))
                            cmbClientePausarVenta.Text = nombre;
                    }
                }
                else
                {
                    cmbClientePausarVenta.SelectedIndex = -1;
                    cmbClientePausarVenta.Text = PlaceholderClientePausa;
                }
            }
            catch
            {
                cmbClientePausarVenta.SelectedIndex = -1;
                cmbClientePausarVenta.Text = PlaceholderClientePausa;
            }
            finally
            {
                _cmbPausaFiltrando = false;
                _suppressPausaUi = false;
            }
        }

        private bool TryLeerClientePausaDelCombo(out int clienteId, out string nombre)
        {
            clienteId = 0;
            nombre = string.Empty;

            if (cmbClientePausarVenta?.SelectedItem is DataRowView row)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    return false;

                clienteId = Convert.ToInt32(row["Id"]);
                nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                return clienteId > 0;
            }

            return false;
        }

        /// <summary>
        /// Resuelve el miembro resaltado/clicado sin asumir índice 0 del filtro.
        /// </summary>
        private bool TryResolverClientePausaPreciso(out int clienteId, out string nombre)
        {
            if (TryLeerClientePausaDelCombo(out clienteId, out nombre))
                return true;

            string texto = (cmbClientePausarVenta.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(texto)
                || string.Equals(texto, PlaceholderClientePausa, StringComparison.OrdinalIgnoreCase))
                return false;

            if (_bsClientePausa.DataSource is not DataTable)
                return false;

            // Coincidencia exacta de nombre en la vista filtrada actual.
            for (int i = 0; i < _bsClientePausa.Count; i++)
            {
                if (_bsClientePausa[i] is not DataRowView drv)
                    continue;

                string n = drv["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                if (!string.Equals(n, texto, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (drv["Id"] == null || drv["Id"] == DBNull.Value)
                    continue;

                clienteId = Convert.ToInt32(drv["Id"]);
                nombre = n;
                if (clienteId > 0)
                {
                    _cmbPausaFiltrando = true;
                    _suppressPausaUi = true;
                    try { cmbClientePausarVenta.SelectedIndex = i; }
                    finally
                    {
                        _cmbPausaFiltrando = false;
                        _suppressPausaUi = false;
                    }
                    return true;
                }
            }

            return false;
        }

        private void cmbClientePausarVenta_Enter(object? sender, EventArgs e)
        {
            if (_cmbPausaFiltrando)
                return;

            _cmbPausaFiltrando = true;
            try
            {
                if (cmbClientePausarVenta.SelectedIndex < 0
                    || string.Equals(cmbClientePausarVenta.Text, PlaceholderClientePausa, StringComparison.OrdinalIgnoreCase))
                {
                    cmbClientePausarVenta.Text = string.Empty;
                }

                cmbClientePausarVenta.SelectionStart = cmbClientePausarVenta.Text.Length;
                cmbClientePausarVenta.SelectionLength = 0;
            }
            finally
            {
                _cmbPausaFiltrando = false;
            }
        }

        private void cmbClientePausarVenta_DropDown(object? sender, EventArgs e)
        {
            // No limpiar filtro aquí: TextUpdate ya filtró; quitarlo haría que el click apunte a otro Id.
            _cmbPausaFiltrando = true;
            try
            {
                if (cmbClientePausarVenta.SelectedIndex < 0
                    || string.Equals(cmbClientePausarVenta.Text, PlaceholderClientePausa, StringComparison.OrdinalIgnoreCase))
                {
                    cmbClientePausarVenta.Text = string.Empty;
                }

                cmbClientePausarVenta.SelectionStart = cmbClientePausarVenta.Text.Length;
                cmbClientePausarVenta.SelectionLength = 0;
            }
            finally
            {
                _cmbPausaFiltrando = false;
            }
        }

        private void cmbClientePausarVenta_TextUpdate(object? sender, EventArgs e)
        {
            if (_cmbPausaFiltrando)
                return;

            string texto = cmbClientePausarVenta.Text ?? string.Empty;

            // El filtro dispara SelectedIndexChanged: hay que silenciarlo o pausa al 1.er match al tipear.
            _cmbPausaFiltrando = true;
            _suppressPausaUi = true;
            try
            {
                AplicarFiltroComboClientePausa(texto);
                if (!cmbClientePausarVenta.DroppedDown)
                    cmbClientePausarVenta.DroppedDown = true;

                cmbClientePausarVenta.SelectedIndex = -1;
                cmbClientePausarVenta.Text = texto;
                cmbClientePausarVenta.SelectionStart = texto.Length;
                cmbClientePausarVenta.SelectionLength = 0;
            }
            finally
            {
                _cmbPausaFiltrando = false;
                _suppressPausaUi = false;
            }
        }

        private void cmbClientePausarVenta_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (!TryResolverClientePausaPreciso(out int clienteId, out string nombre))
                return;

            PreservarClientePausaTrasQuitarFiltro(clienteId);
            cmbClientePausarVenta.DroppedDown = false;
            AplicarSeleccionClientePausa(clienteId, nombre);
        }

        private void cmbClientePausarVenta_DropDownClosed(object? sender, EventArgs e)
        {
            int? id = null;
            if (TryLeerClientePausaDelCombo(out int clienteId, out _))
                id = clienteId;

            PreservarClientePausaTrasQuitarFiltro(id);
        }

        private void cmbClientePausarVenta_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_suppressPausaUi || _cmbPausaFiltrando)
                return;

            if (!TryResolverClientePausaPreciso(out int clienteId, out string nombre))
                return;

            PreservarClientePausaTrasQuitarFiltro(clienteId);
            AplicarSeleccionClientePausa(clienteId, nombre);
        }

        private void SeleccionarClientePausaEnCombo(int clienteId)
        {
            _cmbPausaFiltrando = true;
            _suppressPausaUi = true;
            try
            {
                RestablecerFiltroComboClientePausa();
                cmbClientePausarVenta.SelectedValue = clienteId;
            }
            catch
            {
                cmbClientePausarVenta.SelectedIndex = -1;
                cmbClientePausarVenta.Text = PlaceholderClientePausa;
            }
            finally
            {
                _cmbPausaFiltrando = false;
                _suppressPausaUi = false;
            }
        }
    }
}
