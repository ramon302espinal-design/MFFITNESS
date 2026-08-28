using System;
using System.Data;
using System.Windows.Forms;
using BLL;
using BLL.Models;
using CORE;
using UI.Theme;

namespace UI.DISEÑO
{
    public partial class FrmPagos
    {
        private readonly SaldoClienteBLL saldoClienteBLL = new SaldoClienteBLL();
        private readonly DataTable _tablaSaldoAbono = new DataTable();
        private bool _suppressSaldoUi;
        private int? _saldoVistaId;
        private bool _saldoModoAsignar;

        private void ConfigurarSaldoAFavor()
        {
            if (pnlSaldoAFavor == null)
                return;

            pnlSaldoAFavor.Visible = false;

            if (_tablaSaldoAbono.Columns.Count == 0)
            {
                _tablaSaldoAbono.Columns.Add("ProductoId", typeof(int));
                _tablaSaldoAbono.Columns.Add("Producto", typeof(string));
                _tablaSaldoAbono.Columns.Add("Precio", typeof(decimal));
                _tablaSaldoAbono.Columns.Add("Cantidad", typeof(int));
                _tablaSaldoAbono.Columns.Add("Total", typeof(decimal));
            }

            dgvSaldoAbono.DataSource = _tablaSaldoAbono;
            dgvSaldoAbono.AllowUserToAddRows = false;
            dgvSaldoAbono.AllowUserToDeleteRows = false;
            dgvSaldoAbono.ReadOnly = true;
            dgvSaldoAbono.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSaldoAbono.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ThemeApplier.ApplyReadOnlyGridBehavior(dgvSaldoAbono);

            LimpiarVistaSaldoAbono();
            RefrescarMiembrosConSaldo();
        }

        private void CargarCombosSaldoAFavor(DataTable clientesPos)
        {
            if (cmbAsignarSaldo == null || cmbConSaldo == null)
                return;

            _suppressSaldoUi = true;
            try
            {
                DataTable copia = clientesPos.Copy();

                cmbAsignarSaldo.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbAsignarSaldo.DisplayMember = "Nombre";
                cmbAsignarSaldo.ValueMember = "Id";
                cmbAsignarSaldo.DataSource = copia.Copy();
                cmbAsignarSaldo.SelectedIndex = -1;

                cmbConSaldo.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbConSaldo.DisplayMember = "ClienteNombre";
                cmbConSaldo.ValueMember = "Id";
                RefrescarMiembrosConSaldo();
            }
            finally
            {
                _suppressSaldoUi = false;
            }
        }

        private void RefrescarMiembrosConSaldo(int? seleccionarSaldoId = null)
        {
            if (cmbConSaldo == null)
                return;

            DataTable activos = saldoClienteBLL.ObtenerActivos();
            cmbConSaldo.DataSource = activos;

            if (seleccionarSaldoId.HasValue && activos.Rows.Count > 0)
            {
                for (int i = 0; i < activos.Rows.Count; i++)
                {
                    if (Convert.ToInt32(activos.Rows[i]["Id"]) == seleccionarSaldoId.Value)
                    {
                        cmbConSaldo.SelectedIndex = i;
                        return;
                    }
                }
            }

            cmbConSaldo.SelectedIndex = -1;
        }

        private void MostrarPanelSaldoAFavor(bool visible)
        {
            if (pnlSaldoAFavor == null)
                return;

            pnlSaldoAFavor.Visible = visible;
            if (visible)
            {
                pnlSaldoAFavor.BringToFront();
                RefrescarMiembrosConSaldo();
            }

            if (chkSaldoAFavor != null && chkSaldoAFavor.Checked != visible)
            {
                _suppressSaldoUi = true;
                try { chkSaldoAFavor.Checked = visible; }
                finally { _suppressSaldoUi = false; }
            }
        }

        private void chkSaldoAFavor_CheckedChanged(object? sender, EventArgs e)
        {
            if (_suppressSaldoUi || chkSaldoAFavor == null)
                return;

            MostrarPanelSaldoAFavor(chkSaldoAFavor.Checked);
        }

        private void btnCerrarSaldo_Click(object? sender, EventArgs e)
        {
            MostrarPanelSaldoAFavor(false);
        }

        private void cmbAsignarSaldo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressSaldoUi)
                return;

            if (cmbAsignarSaldo?.SelectedIndex < 0)
            {
                _saldoModoAsignar = false;
                if (cmbConSaldo?.SelectedIndex < 0)
                    LimpiarVistaSaldoAbono();
                return;
            }

            _saldoModoAsignar = true;
            _saldoVistaId = null;

            _suppressSaldoUi = true;
            try
            {
                if (cmbConSaldo != null)
                    cmbConSaldo.SelectedIndex = -1;
            }
            finally
            {
                _suppressSaldoUi = false;
            }

            if (carrito.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Agregue productos al carrito antes de asignar saldo a favor.",
                    "Saldo a favor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (TryObtenerClienteAsignarSaldo(out _, out string nombre))
                SincronizarSaldoAbonoDesdeCarrito(nombre);
        }

        private void cmbConSaldo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressSaldoUi)
                return;

            if (cmbConSaldo?.SelectedIndex < 0)
            {
                if (cmbAsignarSaldo?.SelectedIndex < 0)
                    LimpiarVistaSaldoAbono();
                return;
            }

            _saldoModoAsignar = false;

            _suppressSaldoUi = true;
            try
            {
                if (cmbAsignarSaldo != null)
                    cmbAsignarSaldo.SelectedIndex = -1;
            }
            finally
            {
                _suppressSaldoUi = false;
            }

            if (!TryObtenerSaldoSeleccionado(out int saldoId, out string nombre))
                return;

            MostrarDetalleSaldoActivo(saldoId, nombre);
        }

        private void SincronizarSaldoAbonoDesdeCarritoSiAsignando()
        {
            if (!_saldoModoAsignar || cmbAsignarSaldo?.SelectedIndex < 0)
                return;

            if (!TryObtenerClienteAsignarSaldo(out _, out string nombre))
                return;

            SincronizarSaldoAbonoDesdeCarrito(nombre);
        }

        private void SincronizarSaldoAbonoDesdeCarrito(string nombreCliente)
        {
            _tablaSaldoAbono.Clear();
            decimal total = 0m;

            foreach (DataRow row in carrito.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                _tablaSaldoAbono.Rows.Add(
                    Convert.ToInt32(row["ProductoId"]),
                    row["Producto"]?.ToString() ?? "Producto",
                    Convert.ToDecimal(row["Precio"]),
                    Convert.ToInt32(row["Cantidad"]),
                    Convert.ToDecimal(row["Total"]));

                total += Convert.ToDecimal(row["Total"]);
            }

            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            lblNombreSaldoAbono.Text = nombreCliente;
            lblTotalSaldoAbono.Text = $"RD$ {total:N2}";
        }

        private void MostrarDetalleSaldoActivo(int saldoClienteId, string nombreCliente)
        {
            _saldoVistaId = saldoClienteId;
            DataTable detalle = saldoClienteBLL.ObtenerDetalle(saldoClienteId);

            _tablaSaldoAbono.Clear();
            decimal total = 0m;
            foreach (DataRow row in detalle.Rows)
            {
                decimal linea = Convert.ToDecimal(row["Total"]);
                _tablaSaldoAbono.Rows.Add(
                    Convert.ToInt32(row["ProductoId"]),
                    row["Producto"]?.ToString() ?? "Producto",
                    Convert.ToDecimal(row["Precio"]),
                    Convert.ToInt32(row["Cantidad"]),
                    linea);
                total += linea;
            }

            lblNombreSaldoAbono.Text = nombreCliente;
            lblTotalSaldoAbono.Text = $"RD$ {total:N2}";
        }

        private void LimpiarVistaSaldoAbono()
        {
            _tablaSaldoAbono.Clear();
            _saldoVistaId = null;
            _saldoModoAsignar = false;
            lblNombreSaldoAbono.Text = "—";
            lblTotalSaldoAbono.Text = "RD$ 0.00";
        }

        private void btnCobrarSaldo_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!VerificarCajaAbierta())
                    return;

                if (!TryObtenerClienteAsignarSaldo(out int clienteId, out string nombre))
                {
                    MessageBox.Show(
                        "Seleccione un miembro en ASIGNAR.",
                        "Saldo a favor",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (carrito.Rows.Count == 0)
                {
                    MessageBox.Show("El carrito está vacío.");
                    return;
                }

                decimal total = ObtenerTotalCarrito();
                if (total <= 0)
                {
                    MessageBox.Show("El total debe ser mayor a cero.");
                    return;
                }

                if (saldoClienteBLL.TieneSaldoActivo(clienteId))
                {
                    DialogResult reemplazar = MessageBox.Show(
                        $"{nombre} ya tiene saldo a favor activo.\n\n¿Reemplazarlo con el carrito actual?",
                        "Saldo a favor",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (reemplazar != DialogResult.Yes)
                        return;
                }

                if (!TryCobrarConCalculadora(total, out SolicitudPagoDTO? pago) || pago == null)
                    return;

                decimal montoAplicado = pago.MontoRecibido >= total ? total : pago.MontoRecibido;
                if (montoAplicado < total)
                {
                    MessageBox.Show(
                        $"Debe cobrar al menos RD$ {total:N2} para cubrir la reserva.",
                        "Saldo a favor",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                int saldoId = saldoClienteBLL.CobrarSaldoReserva(
                    clienteId,
                    nombre,
                    carrito,
                    montoAplicado,
                    pago.MetodoSeleccionado.ToMetodoBd(),
                    Sesion.Usuario);

                carrito.Clear();
                CalcularTotal();
                CargarProductos();

                _suppressSaldoUi = true;
                try
                {
                    if (cmbAsignarSaldo != null)
                        cmbAsignarSaldo.SelectedIndex = -1;
                }
                finally
                {
                    _suppressSaldoUi = false;
                }

                RefrescarMiembrosConSaldo(seleccionarSaldoId: saldoId);
                MostrarDetalleSaldoActivo(saldoId, nombre);

                AppEventos.CajaCambiada();
                AppEventos.PagoRegistrado();

                MessageBox.Show(
                    $"Saldo a favor registrado para {nombre}.\n\n" +
                    $"Cobrado: RD$ {montoAplicado:N2}\n" +
                    "Los productos quedan reservados hasta el despacho.",
                    "Saldo a favor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Saldo a favor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAbonarSaldo_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!TryObtenerSaldoSeleccionado(out int saldoId, out string nombre)
                    && !_saldoVistaId.HasValue)
                {
                    MessageBox.Show(
                        "Seleccione un miembro en CON SALDO.",
                        "Despachar reserva",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (!TryObtenerSaldoSeleccionado(out saldoId, out nombre))
                    saldoId = _saldoVistaId!.Value;

                var cabecera = saldoClienteBLL.ObtenerCabeceraActiva(saldoId);
                if (cabecera == null)
                {
                    MessageBox.Show("El saldo a favor ya no está activo.");
                    RefrescarMiembrosConSaldo();
                    LimpiarVistaSaldoAbono();
                    return;
                }

                DialogResult confirmar = MessageBox.Show(
                    $"¿Despachar los productos reservados de {nombre}?\n\n" +
                    "No se cobrará de nuevo (ya pagó con saldo a favor).",
                    "Despachar reserva",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmar != DialogResult.Yes)
                    return;

                var operacion = saldoClienteBLL.DespacharSaldo(saldoId, Sesion.Usuario);

                RefrescarMiembrosConSaldo();
                LimpiarVistaSaldoAbono();
                CargarProductos();
                ProgramarRefrescoDashboard();

                AppEventos.PagoRegistrado();

                MessageBox.Show(
                    $"Productos despachados para {nombre}.\n\nVenta Id {operacion.VentaId}",
                    "Despachar reserva",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Despachar reserva", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryObtenerClienteAsignarSaldo(out int clienteId, out string nombre)
        {
            clienteId = 0;
            nombre = string.Empty;

            if (cmbAsignarSaldo?.SelectedItem is DataRowView row)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    return false;

                clienteId = Convert.ToInt32(row["Id"]);
                nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                return clienteId > 0;
            }

            if (cmbAsignarSaldo?.SelectedValue != null
                && cmbAsignarSaldo.SelectedValue != DBNull.Value
                && int.TryParse(cmbAsignarSaldo.SelectedValue.ToString(), out int id)
                && id > 0)
            {
                clienteId = id;
                nombre = cmbAsignarSaldo.Text.Trim();
                return true;
            }

            return false;
        }

        private bool TryObtenerSaldoSeleccionado(out int saldoId, out string nombre)
        {
            saldoId = 0;
            nombre = string.Empty;

            if (cmbConSaldo?.SelectedItem is DataRowView row)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    return false;

                saldoId = Convert.ToInt32(row["Id"]);
                nombre = row["ClienteNombre"]?.ToString()?.Trim() ?? string.Empty;
                return saldoId > 0;
            }

            if (cmbConSaldo?.SelectedValue != null
                && cmbConSaldo.SelectedValue != DBNull.Value
                && int.TryParse(cmbConSaldo.SelectedValue.ToString(), out int id)
                && id > 0)
            {
                saldoId = id;
                nombre = cmbConSaldo.Text.Trim();
                return true;
            }

            return false;
        }
    }
}
