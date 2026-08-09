using BLL;
using BLL.Commands;
using BLL.Models;
using DTO;
using System;
using System.Data;
using System.Windows.Forms;
using CORE;
using UI.Theme;
using UI.Helpers;
using UI.Facturas;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmPagos : Form
    {
        // 1. Agregamos '?' para que el compilador acepte que puede ser nulo
        private readonly Form? formularioAnterior;

        // ===============================
        // DECLARACIÓN DE INSTANCIAS BLL
        // ===============================
        private readonly VentasBLL ventasBLL = new VentasBLL();
        private readonly ProductoBLL productoBLL = new ProductoBLL();
        private readonly CajaBLL cajaBLL = new CajaBLL();
        private readonly MembresiaBLL membresiaBLL = new MembresiaBLL();
        private readonly DataTable carrito = new DataTable();
        private FrmPresentacion? _presentacion;
        private readonly ClienteBLL clienteBLL = new ClienteBLL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();

        // ===============================
        // CONSTRUCTORES
        // ===============================

        public FrmPagos(Form frm)
        {
            InitializeComponent();
            formularioAnterior = frm;
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloPagar);
        }

        public FrmPagos()
        {
            InitializeComponent();
            formularioAnterior = null; // Ahora permitido por el '?' arriba
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloPagar);
        }

        // Agregamos '?' a nombreCliente para que acepte el valor null por defecto
        public FrmPagos(FrmPresentacion presentacion, int? clienteId = null, string? nombreCliente = null)
        {
            InitializeComponent();
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloPagar);
            _presentacion = presentacion;
            formularioAnterior = presentacion;

            CargarClientes();
            CargarProductos();
            ConfigurarCarrito();
            CargarMembresias();

            if (clienteId.HasValue)
            {
                cmbCliente.SelectedValue = clienteId.Value;
            }
        }

        public FrmPagos(FrmPresentacion presentacion, int clienteId, string nombreCliente)
        {
            InitializeComponent();
            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloPagar);
            _presentacion = presentacion;
            formularioAnterior = presentacion;

            CargarClientes();
            CargarProductos();
            ConfigurarCarrito();
            CargarMembresias();

            cmbCliente.SelectedValue = clienteId;
        }

        // ===============================
        // VERIFICAR CAJA ABIERTA
        // ===============================
        private bool VerificarCajaAbierta()
        {
            var caja = cajaBLL.ObtenerCajaAbiertaHoy();

            if (caja == null)
            {
                DialogResult result = MessageBox.Show(
                    "No hay caja abierta. ¿Deseas abrirla?",
                    "Caja cerrada",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    string input = Microsoft.VisualBasic.Interaction.InputBox(
                        "Ingrese el monto inicial de caja:",
                        "Apertura de Caja",
                        "0"
                    );

                    if (!decimal.TryParse(input, out decimal montoInicial))
                    {
                        MessageBox.Show("Monto inválido.");
                        return false;
                    }

                    cajaBLL.AbrirCaja(montoInicial, Sesion.Usuario ?? "ADMIN");
                    MessageBox.Show("Caja abierta correctamente.");
                    return true;
                }
                return false;
            }
            return true;
        }

        private void FrmPagos_Load(object sender, EventArgs e)
        {
            // Solo cargar si no se ha inicializado (evita doble carga en constructores con parámetros)
            if (cmbCliente.DataSource == null)
            {
                CargarClientes();
                CargarProductos();
                ConfigurarCarrito();
                CargarMembresias();
            }

            dtpFechaVencimiento.Value = DateTime.Today.AddDays(30);
            dtpFechaVencimiento.Enabled = false;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        private void CargarClientes()
        {
            ClienteBLL clienteBLL = new ClienteBLL();
            DataTable dt = clienteBLL.ObtenerClientes();

            // ValueMember ANTES del DataSource; columna real = "Id" (no "ID").
            cmbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCliente.DisplayMember = "Nombre";
            cmbCliente.ValueMember = "Id";
            cmbCliente.DataSource = dt;
            cmbCliente.SelectedIndex = -1;
        }

        /// <summary>
        /// Obtiene el Id real del cliente seleccionado (evita homónimos / SelectedValue roto).
        /// </summary>
        private bool TryObtenerClienteSeleccionado(out int clienteId, out string nombre)
        {
            clienteId = 0;
            nombre = string.Empty;

            if (cmbCliente.SelectedItem is DataRowView row)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    return false;

                clienteId = Convert.ToInt32(row["Id"]);
                nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                return clienteId > 0;
            }

            if (cmbCliente.SelectedValue != null
                && cmbCliente.SelectedValue != DBNull.Value
                && int.TryParse(cmbCliente.SelectedValue.ToString(), out int id)
                && id > 0)
            {
                clienteId = id;
                nombre = cmbCliente.Text.Trim();
                return true;
            }

            return false;
        }

        private void CargarProductos()
        {
            cmbProducto.DataSource = productoBLL.ObtenerProductos();
            cmbProducto.DisplayMember = "Nombre";
            cmbProducto.ValueMember = "Id";
            txtPrecioProducto.ReadOnly = true;
        }

        private void CargarMembresias()
        {
            PlanBLL planBLL = new PlanBLL();
            DataTable dt = planBLL.ObtenerPlanes();

            // 🔥 FILTRO AQUÍ
            DataView dv = dt.DefaultView;
            dv.RowFilter = "Nombre <> 'MENSUALIDAD' AND Nombre <> '3x'";

            cmbMembresia.DataSource = dv;
            cmbMembresia.DisplayMember = "Nombre";
            cmbMembresia.ValueMember = "Id";
            cmbMembresia.SelectedIndex = -1;
        }

        private void ConfigurarCarrito()
        {
            if (carrito.Columns.Count == 0)
            {
                carrito.Columns.Add("ProductoId", typeof(int));
                carrito.Columns.Add("Producto", typeof(string));
                carrito.Columns.Add("Precio", typeof(decimal));
                carrito.Columns.Add("Cantidad", typeof(int));
                carrito.Columns.Add("Total", typeof(decimal));
            }

            dgvCarrito.DataSource = carrito;

            if (!dgvCarrito.Columns.Contains("Eliminar"))
            {
                DataGridViewButtonColumn btnEliminar = new DataGridViewButtonColumn
                {
                    Name = "Eliminar",
                    Text = "X",
                    UseColumnTextForButtonValue = true
                };
                dgvCarrito.Columns.Add(btnEliminar);
            }

            dgvCarrito.AllowUserToAddRows = false;
            dgvCarrito.ReadOnly = true;
            dgvCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ThemeApplier.ApplyReadOnlyGridBehavior(dgvCarrito);
        }

        private void dgvCarrito_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCarrito.CurrentRow?.Cells["ProductoId"].Value == null) return;
            int id = Convert.ToInt32(dgvCarrito.CurrentRow.Cells["ProductoId"].Value);
            cmbProducto.SelectedValue = id;
        }

        private void dgvCarrito_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCarrito.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                if (MessageBox.Show("¿Eliminar producto?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    carrito.Rows[e.RowIndex].Delete();
                    carrito.AcceptChanges();
                    CalcularTotal();
                }
            }
        }

        private void btnLimpiarCarrito_Click(object sender, EventArgs e)
        {
            if (carrito.Rows.Count == 0) return;
            if (MessageBox.Show("¿Limpiar carrito?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                carrito.Clear();
                CalcularTotal();
            }
        }

        private void cmbProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProducto.SelectedItem is DataRowView row && row["PrecioVenta"] != DBNull.Value)
            {
                txtPrecioProducto.Text = Convert.ToDecimal(row["PrecioVenta"]).ToString("0.00");
            }
        }

        private void btnAgregarCarrito_Click(object sender, EventArgs e)
        {
            if (cmbProducto.SelectedValue == null) return;

            int id = Convert.ToInt32(cmbProducto.SelectedValue);
            string nombre = cmbProducto.Text;
            decimal precio = decimal.TryParse(txtPrecioProducto.Text, out decimal p) ? p : 0;
            int cantidad = (int)numCantidad.Value;

            DataRow[] filas = carrito.Select("ProductoId = " + id);

            if (filas.Length > 0)
            {
                int cantidadActual = Convert.ToInt32(filas[0]["Cantidad"]);
                filas[0]["Cantidad"] = cantidadActual + cantidad;
                filas[0]["Total"] = (cantidadActual + cantidad) * precio;
            }
            else
            {
                carrito.Rows.Add(id, nombre, precio, cantidad, precio * cantidad);
            }

            CalcularTotal();
            numCantidad.Value = 1;
        }

        private void CalcularTotal()
        {
            decimal total = 0;
            foreach (DataRow row in carrito.Rows)
                total += Convert.ToDecimal(row["Total"]);

            lblTotal.Text = total.ToString("0.00");
        }

        private void btnPagarProductos_Click(object sender, EventArgs e)
        {
            try
            {
                if (carrito.Rows.Count == 0) return;
                if (!VerificarCajaAbierta()) return;

                int? clienteId = TryObtenerClienteSeleccionado(out int idCliente, out _)
                    ? idCliente
                    : (int?)null;
                decimal total = ObtenerTotalCarrito();
                if (total <= 0)
                {
                    MessageBox.Show("El total del carrito debe ser mayor a cero.");
                    return;
                }

                if (!TryCobrarConCalculadora(total, out SolicitudPagoDTO? pago) || pago == null)
                    return;

                // A caja/BD solo entra lo aplicado a la venta (el exceso es cambio al cliente).
                decimal montoAplicado = pago.MontoRecibido >= total ? total : pago.MontoRecibido;

                var result = VentasCommandService.RegistrarVentaPOS(
                    clienteId,
                    total,
                    montoAplicado,
                    pago.MetodoSeleccionado.ToMetodoBd(),
                    carrito,
                    Sesion.Usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message);
                    return;
                }

                if (pago.DebeImprimirRecibo)
                {
                    string? clienteNombre = cmbCliente.SelectedItem is DataRowView row
                        ? row["Nombre"]?.ToString()
                        : null;

                    ReciboPosHelper.MostrarVenta(
                        this,
                        pago,
                        carrito,
                        clienteNombre,
                        Sesion.Usuario ?? "ADMIN");
                }

                MessageBox.Show("Venta realizada.");
                carrito.Clear();
                CalcularTotal();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private decimal ObtenerTotalCarrito()
        {
            decimal total = 0;
            foreach (DataRow row in carrito.Rows)
                total += Convert.ToDecimal(row["Total"]);
            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        private void cmbMembresia_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMembresia.SelectedItem is DataRowView row)
            {
                txtMonto.Text = Convert.ToDecimal(row["Precio"]).ToString("0.00");

                // 🆕 Actualizar cálculo de saldo si financiamiento está activo
                if (chkFinanciamiento.Checked)
                {
                    CalcularSaldoFinanciamiento();
                }
            }
        }

        private void btnPagarMembresia_Click(object sender, EventArgs e)
        {
            if (!btnPagar.Enabled)
                return;

            try
            {
                if (!TryObtenerClienteSeleccionado(out int clienteId, out _) || cmbMembresia.SelectedValue == null)
                {
                    MessageBox.Show("Faltan datos de cliente o membresía.");
                    return;
                }

                if (!VerificarCajaAbierta()) return;

                if (!ConfirmarPerfilCliente(clienteId)) return;

                if (deudaBLL.ClienteBloqueadoPorDeudaPendiente(clienteId, out string motivoDeuda))
                {
                    MessageBox.Show(
                        motivoDeuda,
                        "Deuda pendiente",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Con financiamiento: vencido/desactivado/sin plan puede activarse a crédito.
                // Sin financiamiento: ofrecer renovación (misma regla que Estado) si aplica.
                if (!chkFinanciamiento.Checked && IntentarRedirigirRenovacion(clienteId))
                    return;

                int planId = Convert.ToInt32(cmbMembresia.SelectedValue);
                string usuario = Sesion.Usuario ?? "ADMIN";

                PlanBLL planBLL = new PlanBLL();
                var plan = planBLL.ObtenerPlan(planId);

                if (plan == null)
                {
                    MessageBox.Show("Plan no encontrado.");
                    return;
                }

                DateTime inicio = DateTime.Now;
                DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);

                btnPagar.Enabled = false;
                Cursor = Cursors.WaitCursor;

                try
                {
                    if (chkFinanciamiento.Checked)
                    {
                        CobrarMembresiaFinanciada(clienteId, planId, plan, fin, usuario);
                    }
                    else
                    {
                        CobrarMembresiaCompleta(clienteId, planId, plan, fin, usuario);
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                    if (!IsDisposed)
                        btnPagar.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                if (!IsDisposed)
                    btnPagar.Enabled = true;
                MessageBox.Show(ex.Message);
            }
        }

        private void CobrarMembresiaFinanciada(
            int clienteId,
            int planId,
            PlanDTO plan,
            DateTime fin,
            string usuario)
        {
            if (membresiaBLL.ClienteNoElegibleParaFinanciamiento(clienteId, out string motivoFinanciamiento))
            {
                MessageBox.Show(
                    motivoFinanciamiento,
                    "Financiamiento no disponible",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                chkFinanciamiento.Checked = false;
                return;
            }

            decimal pagoInicial = decimal.TryParse(txtPagoInicial.Text, out decimal p) ? p : 0;

            if (pagoInicial < 0 || pagoInicial > plan.Precio)
            {
                MessageBox.Show("Pago inicial inválido.");
                return;
            }

            decimal saldo = plan.Precio - pagoInicial;
            string conceptoPago = $"Pago inicial - Membresía {cmbMembresia.Text}";
            string metodoPago = "Efectivo";

            DateTime? fechaVencimientoDeuda = saldo > 0
                ? dtpFechaVencimiento.Value.Date
                : null;

            var result = MembresiaCommandService.VenderMembresiaFinanciada(
                clienteId,
                planId,
                pagoInicial,
                metodoPago,
                conceptoPago,
                fechaVencimientoDeuda,
                usuario);

            if (!result.Success)
            {
                MessageBox.Show(result.Message);
                return;
            }

            LimpiarCampos();

            MessageBox.Show(
                $"Membresía financiada registrada correctamente.\n\n" +
                $"Plan: {plan.Nombre}\n" +
                $"Pago inicial: ${pagoInicial:N2}\n" +
                $"Saldo pendiente: ${saldo:N2}\n" +
                $"Cliente activado inmediatamente.",
                "Financiamiento Exitoso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ProgramarRefrescoTrasPago();

            if (pagoInicial > 0 && result.Payload is MembresiaOperacionResult opFin)
            {
                string? nota = saldo > 0
                    ? $"Tu membresía está activa. Saldo pendiente: RD${saldo:N0}. Vence el {fin:dd/MM/yyyy}."
                    : null;
                // WhatsApp ya lo dispara MembresiaBLL en background; aquí solo PDF de respaldo.
                IniciarPostPagoEnSegundoPlano(
                    clienteId,
                    planId,
                    plan.Nombre ?? cmbMembresia.Text,
                    pagoInicial,
                    fin,
                    metodoPago,
                    opFin,
                    notaExtra: nota,
                    enviarWhatsAppFactura: false);
            }
        }

        private void CobrarMembresiaCompleta(
            int clienteId,
            int planId,
            PlanDTO plan,
            DateTime fin,
            string usuario)
        {
            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Monto inválido.");
                return;
            }

            string concepto = $"Membresía {cmbMembresia.Text}";
            string metodoPago = "Efectivo";

            var result = MembresiaCommandService.PagarMembresia(
                clienteId,
                planId,
                monto,
                metodoPago,
                concepto,
                fin,
                usuario);

            if (!result.Success)
            {
                MessageBox.Show(result.Message);
                return;
            }

            LimpiarCampos();
            MessageBox.Show("Membresía registrada correctamente.");

            ProgramarRefrescoTrasPago();

            if (result.Payload is MembresiaOperacionResult opPago)
            {
                IniciarPostPagoEnSegundoPlano(
                    clienteId,
                    planId,
                    plan.Nombre ?? cmbMembresia.Text,
                    monto,
                    fin,
                    metodoPago,
                    opPago);
            }
        }

        /// <summary>
        /// Dashboard + eventos fuera del click sincronizado (evita freeze por listeners).
        /// </summary>
        private void ProgramarRefrescoTrasPago()
        {
            if (IsDisposed)
                return;

            BeginInvoke(new Action(() =>
            {
                try
                {
                    CORE.AppEventos.PagoRegistrado();
                    _presentacion?.CargarDashboard();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Refresco post-pago] {ex.Message}");
                }
            }));
        }

        /// <summary>
        /// PDF + Supabase + WhatsApp fuera del hilo UI. Sin popups ni abrir factura en el PC.
        /// </summary>
        private void IniciarPostPagoEnSegundoPlano(
            int clienteId,
            int planId,
            string nombrePlan,
            decimal monto,
            DateTime fin,
            string metodoPago,
            MembresiaOperacionResult opPago,
            string? notaExtra = null,
            bool enviarWhatsAppFactura = true)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                if (enviarWhatsAppFactura)
                {
                    try
                    {
                        string? waDetalle = membresiaBLL.EnviarWhatsAppTrasPagoMembresia(
                            clienteId,
                            planId,
                            monto,
                            DateTime.Now,
                            fin,
                            metodoPago,
                            opPago.PagoId);

                        System.Diagnostics.Debug.WriteLine(
                            $"[WhatsApp post-pago] {waDetalle ?? "(sin detalle)"}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[WhatsApp post-pago] Error: {ex.Message}");
                    }
                }

                try
                {
                    FacturaMembresiaPdfService.GenerarDesdeOperacion(
                        owner: null,
                        clienteId,
                        nombrePlan,
                        monto,
                        fin,
                        metodoPago,
                        opPago,
                        notaExtra: notaExtra,
                        abrirPdf: false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PDF post-pago] {ex.Message}");
                }
            });
        }

        private void LimpiarCampos()
        {
            cmbCliente.SelectedIndex = -1;
            cmbProducto.SelectedIndex = -1;
            cmbMembresia.SelectedIndex = -1;
            txtMonto.Clear();
            txtPrecioProducto.Clear();
            numCantidad.Value = 1;
            chkFinanciamiento.Checked = false;
            txtPagoInicial.Text = "0";
            lblSaldoValor.Text = "$0.00";
            dtpFechaVencimiento.Value = DateTime.Today.AddDays(30);
            dtpFechaVencimiento.Enabled = false;
            pnlFinanciamiento.Visible = false;
        }

        private bool ConfirmarPerfilCliente(int clienteId)
        {
            var perfil = clienteBLL.ValidarPerfilCompleto(clienteId);
            if (perfil.EsCompleto)
                return true;

            string detalle = string.IsNullOrWhiteSpace(perfil.ResumenCamposFaltantes)
                ? string.Empty
                : $"\n\nCampos faltantes: {perfil.ResumenCamposFaltantes}";

            DialogResult respuesta = MessageBox.Show(
                "El cliente tiene datos incompletos en su perfil. ¿Deseas ir a actualizarlos o proceder con el pago?" + detalle,
                "Perfil incompleto",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (respuesta == DialogResult.Yes)
            {
                if (_presentacion != null)
                {
                    using var frmClientes = new FrmClientes(_presentacion, clienteId);
                    frmClientes.ShowDialog();
                }
                else
                {
                    MessageBox.Show(
                        "Abra el módulo de clientes para completar el perfil.",
                        "Perfil incompleto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return false;
            }

            return true;
        }

        private bool IntentarRedirigirRenovacion(int clienteId)
        {
            // Misma regla que botón RENOVAR en FrmEstadoClientes (VENCIDO / DESACTIVADO).
            if (!membresiaBLL.ClienteElegibleParaRenovacion(clienteId))
                return false;

            string nombre = cmbCliente.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                nombre = "Cliente";

            DialogResult respuesta = MessageBox.Show(
                $"El cliente {nombre} (#{clienteId}) aparece como VENCIDO o DESACTIVADO en Estado.\n\n" +
                "¿Desea renovar el plan?\n\n" +
                "Sí = renovar | No = cobrar como membresía nueva",
                "Renovación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return false;

            bool renovado = RenovacionMembresiaDialog.Mostrar(this, clienteId, nombre, () =>
            {
                ProgramarRefrescoTrasPago();
            });

            if (renovado)
            {
                MessageBox.Show(
                    "Renovación registrada correctamente.",
                    "Renovación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            // Si canceló el diálogo de renovación, permitir continuar con cobro nuevo.
            return renovado;
        }

        // ===============================
        // 🆕 EVENTOS DE FINANCIAMIENTO
        // ===============================

        private void chkFinanciamiento_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFinanciamiento.Checked)
            {
                if (!TryObtenerClienteSeleccionado(out int clienteIdChk, out _))
                {
                    MessageBox.Show(
                        "Seleccione un cliente antes de activar el financiamiento.",
                        "Financiamiento",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    chkFinanciamiento.Checked = false;
                    return;
                }

                if (membresiaBLL.ClienteNoElegibleParaFinanciamiento(clienteIdChk, out string motivoChk))
                {
                    MessageBox.Show(
                        motivoChk,
                        "Financiamiento no disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    chkFinanciamiento.Checked = false;
                    return;
                }
            }

            pnlFinanciamiento.Visible = chkFinanciamiento.Checked;
            dtpFechaVencimiento.Enabled = chkFinanciamiento.Checked;

            if (chkFinanciamiento.Checked)
            {
                txtMonto.Enabled = false;
                txtPagoInicial.Text = "0";
                dtpFechaVencimiento.Value = DateTime.Today.AddDays(30);
                CalcularSaldoFinanciamiento();
            }
            else
            {
                txtMonto.Enabled = true;
                lblSaldoValor.Text = "$0.00";
            }
        }

        private void txtPagoInicial_TextChanged(object sender, EventArgs e)
        {
            CalcularSaldoFinanciamiento();
        }

        private void CalcularSaldoFinanciamiento()
        {
            try
            {
                if (cmbMembresia.SelectedValue == null) return;

                int planId = Convert.ToInt32(cmbMembresia.SelectedValue);
                PlanBLL planBLL = new PlanBLL();
                var plan = planBLL.ObtenerPlan(planId);

                if (plan == null) return;

                decimal precioTotal = plan.Precio;
                decimal pagoInicial = decimal.TryParse(txtPagoInicial.Text, out decimal p) ? p : 0;
                decimal saldo = precioTotal - pagoInicial;

                if (saldo < 0) saldo = 0;

                lblSaldoValor.Text = $"${saldo:N2}";
                txtMonto.Text = precioTotal.ToString("0.00");

                if (saldo <= 0 && chkFinanciamiento.Checked)
                    chkFinanciamiento.Checked = false;
            }
            catch
            {
                lblSaldoValor.Text = "$0.00";
            }
        }

        /// <summary>
        /// Abre el modal FrmPago (calculadora POS) y devuelve la solicitud de cobro.
        /// </summary>
        private bool TryCobrarConCalculadora(decimal totalAPagar, out SolicitudPagoDTO? solicitud)
        {
            solicitud = null;

            if (totalAPagar <= 0)
            {
                MessageBox.Show("El monto a cobrar debe ser mayor a cero.", "Cobro inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            using var frmPago = new FrmPago(totalAPagar);
            if (frmPago.ShowDialog(this) != DialogResult.OK)
                return false;

            solicitud = frmPago.PagoResultado;
            return solicitud != null;
        }
    }
}