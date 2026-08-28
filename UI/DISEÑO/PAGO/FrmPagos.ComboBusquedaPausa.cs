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

        private void cmbClientePausarVenta_Enter(object? sender, EventArgs e)
        {
            if (_cmbPausaFiltrando)
                return;

            RestablecerFiltroComboClientePausa();

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
            RestablecerFiltroComboClientePausa();

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
            AplicarFiltroComboClientePausa(texto);

            _cmbPausaFiltrando = true;
            try
            {
                cmbClientePausarVenta.DroppedDown = true;
                cmbClientePausarVenta.Text = texto;
                cmbClientePausarVenta.SelectionStart = texto.Length;
                cmbClientePausarVenta.SelectionLength = 0;
            }
            finally
            {
                _cmbPausaFiltrando = false;
            }
        }

        private void cmbClientePausarVenta_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (cmbClientePausarVenta.SelectedIndex < 0 && _bsClientePausa.Count > 0)
                cmbClientePausarVenta.SelectedIndex = 0;

            RestablecerFiltroComboClientePausa();
            cmbClientePausarVenta.DroppedDown = false;
        }

        private void cmbClientePausarVenta_DropDownClosed(object? sender, EventArgs e)
        {
            RestablecerFiltroComboClientePausa();

            if (cmbClientePausarVenta.SelectedIndex >= 0)
                return;

            _cmbPausaFiltrando = true;
            try
            {
                cmbClientePausarVenta.Text = PlaceholderClientePausa;
            }
            finally
            {
                _cmbPausaFiltrando = false;
            }
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
