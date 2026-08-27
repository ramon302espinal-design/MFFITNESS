using BLL.Commands;
using CORE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    /// <summary>
    /// Financiamiento de carrito (productos): txtMiembroDebe + listMiembros con búsqueda inteligente.
    /// Sin ComboBox — filtro en vivo, tokens AND, Id, sin acentos.
    /// </summary>
    public partial class FrmPagos
    {
        private readonly List<MiembroDebeItem> _miembrosDebeCatalogo = new();
        private MiembroDebeItem? _miembroDebeSeleccionado;
        private bool _suppressMiembroDebeUi;

        private void CargarCatalogoMiembrosDebe(DataTable fuenteClientes)
        {
            _miembrosDebeCatalogo.Clear();
            foreach (DataRow row in fuenteClientes.Rows)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    continue;

                int id = Convert.ToInt32(row["Id"]);
                if (id <= 0)
                    continue;

                string nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(nombre))
                    continue;

                _miembrosDebeCatalogo.Add(new MiembroDebeItem(id, nombre));
            }

            _miembrosDebeCatalogo.Sort((a, b) =>
                string.Compare(a.Nombre, b.Nombre, StringComparison.CurrentCultureIgnoreCase));

            RefrescarListaMiembros(string.Empty, conservarSeleccionId: null);
        }

        private void txtMiembroDebe_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressMiembroDebeUi)
                return;

            RefrescarListaMiembros(txtMiembroDebe.Text, _miembroDebeSeleccionado?.Id);
        }

        private void txtMiembroDebe_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                if (listMiembros.Items.Count > 0)
                {
                    listMiembros.Focus();
                    if (listMiembros.SelectedIndex < 0)
                        listMiembros.SelectedIndex = 0;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (listMiembros.Items.Count == 0)
                return;

            if (listMiembros.SelectedIndex < 0)
                listMiembros.SelectedIndex = 0;

            ConfirmarSeleccionMiembroDebe();
        }

        private void listMiembros_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressMiembroDebeUi)
                return;

            if (listMiembros.SelectedItem is MiembroDebeItem item)
                _miembroDebeSeleccionado = item;
        }

        private void listMiembros_Click(object? sender, EventArgs e) =>
            ConfirmarSeleccionMiembroDebe();

        private void listMiembros_DoubleClick(object? sender, EventArgs e) =>
            ConfirmarSeleccionMiembroDebe();

        private void listMiembros_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            ConfirmarSeleccionMiembroDebe();
            txtMiembroDebe.Focus();
        }

        private void ConfirmarSeleccionMiembroDebe()
        {
            if (listMiembros.SelectedItem is not MiembroDebeItem item)
                return;

            _miembroDebeSeleccionado = item;
            _suppressMiembroDebeUi = true;
            try
            {
                txtMiembroDebe.Text = item.Nombre;
            }
            finally
            {
                _suppressMiembroDebeUi = false;
            }

            RefrescarListaMiembros(item.Nombre, item.Id);
        }

        private void RefrescarListaMiembros(string? filtro, int? conservarSeleccionId)
        {
            IReadOnlyList<MiembroDebeItem> vista = string.IsNullOrWhiteSpace(filtro)
                ? _miembrosDebeCatalogo
                : RankearMiembrosDebe(filtro);

            _suppressMiembroDebeUi = true;
            try
            {
                listMiembros.BeginUpdate();
                listMiembros.Items.Clear();
                foreach (MiembroDebeItem item in vista)
                    listMiembros.Items.Add(item);

                int select = -1;
                if (conservarSeleccionId.HasValue)
                {
                    for (int i = 0; i < listMiembros.Items.Count; i++)
                    {
                        if (listMiembros.Items[i] is MiembroDebeItem m
                            && m.Id == conservarSeleccionId.Value)
                        {
                            select = i;
                            break;
                        }
                    }
                }

                if (select >= 0)
                {
                    listMiembros.SelectedIndex = select;
                    _miembroDebeSeleccionado = (MiembroDebeItem)listMiembros.Items[select]!;
                }
                else if (vista.Count == 1)
                {
                    listMiembros.SelectedIndex = 0;
                    _miembroDebeSeleccionado = vista[0];
                }
                else
                {
                    listMiembros.SelectedIndex = -1;
                    if (string.IsNullOrWhiteSpace(filtro))
                        _miembroDebeSeleccionado = null;
                    else if (_miembroDebeSeleccionado != null
                             && !ContieneId(vista, _miembroDebeSeleccionado.Id))
                        _miembroDebeSeleccionado = null;
                }
            }
            finally
            {
                listMiembros.EndUpdate();
                _suppressMiembroDebeUi = false;
            }
        }

        private static bool ContieneId(IReadOnlyList<MiembroDebeItem> items, int id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                    return true;
            }
            return false;
        }

        private List<MiembroDebeItem> RankearMiembrosDebe(string filtro)
        {
            string q = NormalizarBusquedaMiembro(filtro.Trim());
            if (q.Length == 0)
                return new List<MiembroDebeItem>(_miembrosDebeCatalogo);

            string[] tokens = q.Split(
                new[] { ' ', '\t', '-', ',' },
                StringSplitOptions.RemoveEmptyEntries);

            var ranked = new List<(MiembroDebeItem Item, int Score)>(_miembrosDebeCatalogo.Count);
            foreach (MiembroDebeItem item in _miembrosDebeCatalogo)
            {
                int score = CalcularScoreMiembroDebe(item, q, tokens);
                if (score < 0)
                    continue;
                ranked.Add((item, score));
            }

            ranked.Sort((a, b) =>
            {
                int byScore = a.Score.CompareTo(b.Score);
                if (byScore != 0)
                    return byScore;
                return string.Compare(a.Item.Nombre, b.Item.Nombre, StringComparison.CurrentCultureIgnoreCase);
            });

            var result = new List<MiembroDebeItem>(ranked.Count);
            foreach (var (item, _) in ranked)
                result.Add(item);

            return result;
        }

        private static int CalcularScoreMiembroDebe(
            MiembroDebeItem item,
            string queryNorm,
            string[] tokens)
        {
            string idTxt = item.Id.ToString(CultureInfo.InvariantCulture);
            if (queryNorm == idTxt || queryNorm == "#" + idTxt)
                return 0;
            if (tokens.Length == 1 && (tokens[0] == idTxt || tokens[0] == "#" + idTxt))
                return 0;

            string nombre = item.NombreNorm;

            if (nombre.StartsWith(queryNorm, StringComparison.Ordinal))
                return 1;

            string[] partes = nombre.Split(
                new[] { ' ', '\t', '-', ',' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string parte in partes)
            {
                if (parte.StartsWith(queryNorm, StringComparison.Ordinal))
                    return 2;
            }

            if (tokens.Length > 1)
            {
                int tokenScore = 0;
                foreach (string token in tokens)
                {
                    bool ok = false;
                    foreach (string parte in partes)
                    {
                        if (parte.StartsWith(token, StringComparison.Ordinal))
                        {
                            ok = true;
                            break;
                        }
                        if (parte.Contains(token, StringComparison.Ordinal))
                        {
                            ok = true;
                            tokenScore += 1;
                            break;
                        }
                    }

                    if (!ok && !nombre.Contains(token, StringComparison.Ordinal))
                        return -1;

                    if (!ok)
                        tokenScore += 2;
                }

                return 3 + tokenScore;
            }

            if (nombre.Contains(queryNorm, StringComparison.Ordinal))
                return 10;

            return -1;
        }

        private static string NormalizarBusquedaMiembro(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            string formD = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (char c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        private void RestaurarListaCompletaMiembroDebe(bool limpiarTexto)
        {
            _miembroDebeSeleccionado = null;
            if (limpiarTexto)
            {
                _suppressMiembroDebeUi = true;
                try { txtMiembroDebe.Clear(); }
                finally { _suppressMiembroDebeUi = false; }
            }

            RefrescarListaMiembros(
                limpiarTexto ? string.Empty : txtMiembroDebe.Text,
                null);
        }

        private void SeleccionarMiembroDebePorId(int clienteId)
        {
            if (clienteId <= 0)
                return;

            MiembroDebeItem? item = null;
            foreach (MiembroDebeItem x in _miembrosDebeCatalogo)
            {
                if (x.Id == clienteId)
                {
                    item = x;
                    break;
                }
            }

            if (item == null)
                return;

            _miembroDebeSeleccionado = item;
            _suppressMiembroDebeUi = true;
            try { txtMiembroDebe.Text = item.Nombre; }
            finally { _suppressMiembroDebeUi = false; }

            RefrescarListaMiembros(item.Nombre, item.Id);
        }

        private bool TryObtenerMiembroDeudor(out int clienteId, out string nombre)
        {
            clienteId = 0;
            nombre = string.Empty;

            if (_miembroDebeSeleccionado != null && _miembroDebeSeleccionado.Id > 0)
            {
                clienteId = _miembroDebeSeleccionado.Id;
                nombre = _miembroDebeSeleccionado.Nombre;
                return true;
            }

            if (listMiembros.SelectedItem is MiembroDebeItem item && item.Id > 0)
            {
                clienteId = item.Id;
                nombre = item.Nombre;
                _miembroDebeSeleccionado = item;
                return true;
            }

            string texto = txtMiembroDebe.Text?.Trim() ?? string.Empty;
            if (texto.Length > 0)
            {
                string norm = NormalizarBusquedaMiembro(texto);
                foreach (MiembroDebeItem m in _miembrosDebeCatalogo)
                {
                    if (m.NombreNorm.Equals(norm, StringComparison.OrdinalIgnoreCase))
                    {
                        clienteId = m.Id;
                        nombre = m.Nombre;
                        _miembroDebeSeleccionado = m;
                        return true;
                    }
                }
            }

            return false;
        }

        private void btnFinanciamiento_Click(object? sender, EventArgs e)
        {
            if (carrito.Rows.Count == 0)
            {
                MessageBox.Show(
                    "El carrito está vacío. Agregue productos antes de financiar.",
                    "Financiamiento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            decimal total = ObtenerTotalCarrito();
            if (total <= 0)
            {
                MessageBox.Show("El total del carrito debe ser mayor a cero.");
                return;
            }

            if (_miembrosDebeCatalogo.Count == 0 && cmbCliente.DataSource is DataTable dt)
                CargarCatalogoMiembrosDebe(dt);

            panelFinanciamientoProducto.Visible = true;
            panelFinanciamientoProducto.BringToFront();

            txtMiembroDebe.Enabled = true;
            txtMiembroDebe.ReadOnly = false;
            txtMiembroDebe.TabStop = true;
            listMiembros.Enabled = true;
            listMiembros.TabStop = true;
            lblMiembroDebe.BringToFront();
            txtMiembroDebe.BringToFront();
            listMiembros.BringToFront();

            RefrescarPanelFinanciamientoProducto();

            if (TryObtenerClienteSeleccionado(out int clienteId, out _))
                SeleccionarMiembroDebePorId(clienteId);

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || txtMiembroDebe.IsDisposed)
                    return;
                if (!panelFinanciamientoProducto.Visible)
                    return;
                txtMiembroDebe.Focus();
                txtMiembroDebe.SelectAll();
            }));
        }

        private void btnCerrarFinProducto_Click(object? sender, EventArgs e) =>
            OcultarPanelFinanciamientoProducto();

        private void OcultarPanelFinanciamientoProducto()
        {
            if (panelFinanciamientoProducto == null)
                return;

            panelFinanciamientoProducto.Visible = false;
            txtProducto.Clear();
            txtPagoInicioProducto.Text = "0";
            lblSaldoRestanteProducto.Text = "$0.00";
            RestaurarListaCompletaMiembroDebe(limpiarTexto: true);
        }

        private void RefrescarPanelFinanciamientoProducto()
        {
            if (txtProducto == null)
                return;

            txtProducto.Text = ConstruirConceptoDeudaCarrito();

            if (string.IsNullOrWhiteSpace(txtPagoInicioProducto.Text))
                txtPagoInicioProducto.Text = "0";

            CalcularSaldoFinanciamientoProducto();
        }

        private string ConstruirConceptoDeudaCarrito()
        {
            var sb = new StringBuilder();
            sb.Append("Venta a crédito");

            int lineas = 0;
            foreach (DataRow row in carrito.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                string nombre = row["Producto"]?.ToString() ?? "Producto";
                int cant = Convert.ToInt32(row["Cantidad"]);
                if (lineas == 0)
                    sb.Append(": ");
                else
                    sb.Append(", ");

                sb.Append(nombre);
                if (cant > 1)
                    sb.Append(" x").Append(cant);

                lineas++;
                if (lineas >= 4)
                {
                    sb.Append("…");
                    break;
                }
            }

            decimal total = ObtenerTotalCarrito();
            sb.Append(" (RD$ ").Append(total.ToString("N2")).Append(')');
            string concepto = sb.ToString();
            return concepto.Length > 200 ? concepto.Substring(0, 200) : concepto;
        }

        private void txtPagoInicioProducto_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void txtPagoInicioProducto_TextChanged(object? sender, EventArgs e) =>
            CalcularSaldoFinanciamientoProducto();

        private void CalcularSaldoFinanciamientoProducto()
        {
            if (lblSaldoRestanteProducto == null)
                return;

            decimal total = ObtenerTotalCarrito();
            decimal pagoInicio = decimal.TryParse(txtPagoInicioProducto?.Text, out decimal p) ? p : 0m;
            if (pagoInicio < 0) pagoInicio = 0m;

            decimal saldo = total - pagoInicio;
            if (saldo < 0) saldo = 0m;

            lblSaldoRestanteProducto.Text = $"${saldo:N2}";
            if (dtpVenceDeudaProducto != null)
                dtpVenceDeudaProducto.Enabled = saldo > 0;
        }

        private void btnGuardarDeudaProducto_Click(object? sender, EventArgs e)
        {
            try
            {
                if (carrito.Rows.Count == 0)
                {
                    MessageBox.Show("El carrito está vacío.");
                    return;
                }

                decimal total = ObtenerTotalCarrito();
                if (total <= 0)
                {
                    MessageBox.Show("El total del carrito debe ser mayor a cero.");
                    return;
                }

                if (!TryObtenerMiembroDeudor(out int clienteId, out string nombreCliente))
                {
                    MessageBox.Show(
                        "Seleccione el miembro deudor.",
                        "Financiamiento producto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                decimal pagoInicio = decimal.TryParse(txtPagoInicioProducto.Text, out decimal p) ? p : -1m;
                if (pagoInicio < 0 || pagoInicio > total)
                {
                    MessageBox.Show("Pago inicial inválido. Debe estar entre 0 y el total del carrito.");
                    return;
                }

                pagoInicio = Math.Round(pagoInicio, 2, MidpointRounding.AwayFromZero);
                decimal saldo = Math.Round(total - pagoInicio, 2, MidpointRounding.AwayFromZero);

                if (pagoInicio > 0 && !VerificarCajaAbierta())
                    return;

                if (saldo > 0 && dtpVenceDeudaProducto.Value.Date < DateTime.Today)
                {
                    MessageBox.Show("La fecha límite de pago no puede ser anterior a hoy.");
                    return;
                }

                if (deudaBLL.TieneAvisoDeudaProducto(clienteId, out string avisoProducto))
                {
                    var cont = MessageBox.Show(
                        avisoProducto + "\n\n¿Desea registrar otra deuda de producto de todos modos?",
                        "Deuda de producto pendiente",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (cont != DialogResult.Yes)
                        return;
                }

                string concepto = string.IsNullOrWhiteSpace(txtProducto.Text)
                    ? ConstruirConceptoDeudaCarrito()
                    : txtProducto.Text.Trim();
                if (concepto.Length > 200)
                    concepto = concepto.Substring(0, 200);

                DateTime? fechaVenc = saldo > 0
                    ? dtpVenceDeudaProducto.Value.Date
                    : null;

                string metodo = pagoInicio > 0 ? "Efectivo" : "Credito";

                var result = VentasCommandService.RegistrarVentaPOS(
                    clienteId,
                    total,
                    pagoInicio,
                    metodo,
                    carrito,
                    Sesion.Usuario,
                    fechaVenc,
                    concepto);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string resumen =
                    $"Venta financiada registrada.\n\n" +
                    $"Cliente: {nombreCliente}\n" +
                    $"Total: ${total:N2}\n" +
                    $"Pago inicial: ${pagoInicio:N2}\n" +
                    $"Saldo pendiente: ${saldo:N2}\n" +
                    (saldo > 0
                        ? $"Vence: {dtpVenceDeudaProducto.Value:dd/MM/yyyy}\n"
                        : string.Empty) +
                    "\nQueda en Deudas, historial, caja (si hubo abono) y CRM/ventas.";

                MessageBox.Show(resumen, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                OcultarPanelFinanciamientoProducto();
                FinalizarPosTrasVenta();
                ProgramarRefrescoDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private sealed class MiembroDebeItem
        {
            public MiembroDebeItem(int id, string nombre)
            {
                Id = id;
                Nombre = nombre;
                NombreNorm = NormalizarBusquedaMiembro(nombre);
            }

            public int Id { get; }
            public string Nombre { get; }
            public string NombreNorm { get; }

            public override string ToString() => Nombre;
        }
    }
}
