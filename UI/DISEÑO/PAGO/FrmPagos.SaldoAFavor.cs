using System;
using System.Data;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
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
                _tablaSaldoAbono.Columns.Add("DetalleId", typeof(int));
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
            dgvSaldoAbono.MultiSelect = false;
            dgvSaldoAbono.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ThemeApplier.ApplyReadOnlyGridBehavior(dgvSaldoAbono);
            dgvSaldoAbono.DataBindingComplete -= dgvSaldoAbono_DataBindingComplete;
            dgvSaldoAbono.DataBindingComplete += dgvSaldoAbono_DataBindingComplete;

            LimpiarVistaSaldoAbono();
            RefrescarMiembrosConSaldo();
        }

        private void dgvSaldoAbono_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvSaldoAbono.Columns.Contains("DetalleId"))
                dgvSaldoAbono.Columns["DetalleId"].Visible = false;
            if (dgvSaldoAbono.Columns.Contains("ProductoId"))
                dgvSaldoAbono.Columns["ProductoId"].Visible = false;
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
                    0,
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
                int detalleId = row.Table.Columns.Contains("DetalleId") && row["DetalleId"] != DBNull.Value
                    ? Convert.ToInt32(row["DetalleId"])
                    : 0;
                _tablaSaldoAbono.Rows.Add(
                    detalleId,
                    Convert.ToInt32(row["ProductoId"]),
                    row["Producto"]?.ToString() ?? "Producto",
                    Convert.ToDecimal(row["Precio"]),
                    Convert.ToInt32(row["Cantidad"]),
                    linea);
                total += linea;
            }

            lblNombreSaldoAbono.Text = nombreCliente;
            lblTotalSaldoAbono.Text = $"RD$ {total:N2}";
            dgvSaldoAbono.ClearSelection();
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

                if (cabecera["ClienteNombre"] != null && cabecera["ClienteNombre"] != DBNull.Value)
                    nombre = Convert.ToString(cabecera["ClienteNombre"])?.Trim() ?? nombre;

                // Fila con DetalleId > 0 → cantidad manual; sin selección → despacho total.
                if (TryObtenerLineaDespachoSeleccionada(out int detalleId, out string producto, out int cantidadDisponible)
                    && detalleId > 0)
                {
                    DespacharCantidadSaldo(saldoId, detalleId, producto, cantidadDisponible, nombre);
                    return;
                }

                DialogResult confirmar = MessageBox.Show(
                    $"¿Despachar TODOS los productos reservados de {nombre}?\n\n" +
                    "Tip: seleccione una fila del grid para indicar la cantidad a despachar.\n" +
                    "No se cobrará de nuevo (ya pagó con saldo a favor).",
                    "Despachar reserva",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmar != DialogResult.Yes)
                    return;

                var result = VentasCommandService.RegistrarDespachoSaldoAFavor(
                    saldoId,
                    Sesion.Usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int ventaId = result.Payload is int idVenta ? idVenta : 0;

                RefrescarMiembrosConSaldo();
                LimpiarVistaSaldoAbono();
                CargarProductos();
                ProgramarRefrescoDashboard();

                MessageBox.Show(
                    $"Productos despachados para {nombre}.\n\nVenta Id {ventaId}",
                    "Despachar reserva",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Despachar reserva", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Despacha N unidades de la línea seleccionada (cantidad vía mini diálogo).
        /// Conserva cableo: venta/stock + refresco historial (OnPagoRegistrado).
        /// </summary>
        private void DespacharCantidadSaldo(
            int saldoId,
            int detalleId,
            string producto,
            int cantidadDisponible,
            string nombre)
        {
            int? elegida = SeleccionCantidadDespachoDialog.Mostrar(
                this,
                producto,
                cantidadDisponible,
                valorInicial: Math.Min(1, cantidadDisponible));

            if (elegida is not int cantidad || cantidad <= 0)
                return;

            var result = VentasCommandService.RegistrarDespachoSaldoAFavorParcial(
                saldoId,
                detalleId,
                cantidad,
                Sesion.Usuario);

            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool cerrado = false;
            int ventaId = 0;
            if (result.Payload is ValueTuple<int, bool, string> payload)
            {
                ventaId = payload.Item1;
                cerrado = payload.Item2;
            }

            CargarProductos();
            ProgramarRefrescoDashboard();

            if (cerrado)
            {
                RefrescarMiembrosConSaldo();
                LimpiarVistaSaldoAbono();
                MessageBox.Show(
                    $"Despachado {cantidad} × {producto} para {nombre}.\n" +
                    $"Reserva completada.\n\nVenta Id {ventaId}",
                    "Despachar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            RefrescarMiembrosConSaldo(seleccionarSaldoId: saldoId);
            MostrarDetalleSaldoActivo(saldoId, nombre);
            MessageBox.Show(
                $"Despachado {cantidad} × {producto} para {nombre}.\n" +
                $"Puede seguir despachando el resto.\n\nVenta Id {ventaId}",
                "Despachar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private bool TryObtenerLineaDespachoSeleccionada(
            out int detalleId,
            out string producto,
            out int cantidad)
        {
            detalleId = 0;
            producto = string.Empty;
            cantidad = 0;

            if (dgvSaldoAbono?.CurrentRow == null || dgvSaldoAbono.CurrentRow.IsNewRow)
                return false;

            if (dgvSaldoAbono.CurrentRow.DataBoundItem is not DataRowView view)
                return false;

            DataRow row = view.Row;
            if (row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached)
                return false;

            if (!row.Table.Columns.Contains("DetalleId") || row["DetalleId"] == DBNull.Value)
                return false;

            detalleId = Convert.ToInt32(row["DetalleId"]);
            if (detalleId <= 0)
                return false;

            producto = row["Producto"]?.ToString()?.Trim() ?? "Producto";
            cantidad = Convert.ToInt32(row["Cantidad"]);
            return cantidad > 0;
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
