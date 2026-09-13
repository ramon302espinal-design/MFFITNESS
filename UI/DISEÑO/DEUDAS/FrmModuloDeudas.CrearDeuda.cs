using BLL;
using BLL.Commands;
using CORE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using UI.Helpers;

namespace UI
{
    /// <summary>
    /// Lógica de la pantalla "Nueva Deuda" (tabCrear): financiamiento de plan y
    /// venta de producto a crédito (solo categoría Suplementos del inventario).
    /// Los controles viven en el diseñador de tabCrear.
    /// </summary>
    public partial class FrmModuloDeudas
    {
        /// <summary>Sentinel en cmbTipoPlan: no es un Plan de membresía.</summary>
        private const int PlanIdProductoCredito = -1;
        private const string NombreProductoCredito = "PRODUCTO A CRÉDITO";
        private const int CantidadMaximaSinProducto = 9999;

        private readonly ClienteBLL clienteBLL = new ClienteBLL();
        private readonly PlanBLL planBLL = new PlanBLL();
        private readonly MembresiaBLL membresiaBLL = new MembresiaBLL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();
        private readonly CajaBLL cajaBLL = new CajaBLL();
        private readonly ProductoBLL productoBLL = new ProductoBLL();

        /// <summary>Productos ya confirmados con AGREGAR.</summary>
        private readonly List<LineaProductoCredito> _lineas = new List<LineaProductoCredito>();

        private DataTable _productos = new DataTable();

        /// <summary>Producto elegido en el buscador y aún sin confirmar con AGREGAR.</summary>
        private DataRow? _productoPendiente;
        private decimal _precioUnitarioPendiente;
        private decimal _precioPlan;
        private bool _crearDeudaInicializado;
        private bool _suppressDatosClienteUi;
        private bool _datosLaboralesCableados;
        private bool _operacionProductoCableada;
        private bool _omitirRefreshCatalogoPorGuardadoLocal;
        private string _huellaLaboralGuardada = string.Empty;

        private sealed class LineaProductoCredito
        {
            public int ProductoId { get; init; }
            public string Nombre { get; init; } = string.Empty;
            public decimal PrecioUnitario { get; init; }
            public int Cantidad { get; set; }

            public decimal Total => Math.Round(PrecioUnitario * Cantidad, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Atiende la petición de un formulario hijo de abrir la pantalla "Nueva Deuda":
        /// activa la pestaña, o avisa si el rol no tiene el permiso.
        /// </summary>
        public bool AbrirCrearDeuda()
        {
            if (!tabControl.TabPages.Contains(tabCrear))
            {
                MessageBox.Show(
                    "No tiene permiso para crear deudas.",
                    "Permisos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return true;
            }

            tabControl.SelectedTab = tabCrear;
            CargarCrear();
            return true;
        }

        /// <summary>
        /// Carga catálogos y valores por defecto. Idempotente: corre la primera vez
        /// que se entra al tab, nunca en el diseñador.
        /// </summary>
        private void InicializarCrearDeuda()
        {
            if (_crearDeudaInicializado)
                return;

            _crearDeudaInicializado = true;

            CablearDatosClienteUi();
            CablearOperacionProductoUi();
            CablearInteresesPlazosUi();
            pnlIntereses.Enabled = false;
            CargarClientes();
            CargarPlanes();
            CargarProductosInventario();
            AplicarModoProductoCredito(false);
            CalcularSaldoRestante();
        }

        /// <summary>
        /// Cablea pnlOperacion (Designer perdió los +=). Idempotente.
        /// </summary>
        private void CablearOperacionProductoUi()
        {
            if (_operacionProductoCableada)
                return;

            _operacionProductoCableada = true;

            cmbTipoPlan.SelectedIndexChanged -= cmbTipoPlan_SelectedIndexChanged;
            cmbTipoPlan.SelectedIndexChanged += cmbTipoPlan_SelectedIndexChanged;

            ConfigurarComboBuscarProductos();

            numCantidad.ValueChanged -= numCantidad_ValueChanged;
            numCantidad.ValueChanged += numCantidad_ValueChanged;

            btnagregar.Click -= btnagregar_Click;
            btnagregar.Click += btnagregar_Click;
            btnlimpiar.Click -= btnlimpiar_Click;
            btnlimpiar.Click += btnlimpiar_Click;

            txtPagodeinicio.TextChanged -= txtPagodeinicio_TextChanged;
            txtPagodeinicio.TextChanged += txtPagodeinicio_TextChanged;
            txtPagodeinicio.KeyPress -= txtPagodeinicio_KeyPress;
            txtPagodeinicio.KeyPress += txtPagodeinicio_KeyPress;

            btnGuardar.Click -= btnGuardar_Click;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click -= btnCancelar_Click;
            btnCancelar.Click += btnCancelar_Click;

            // Lista legacy: ya no se usa (reemplazada por cmbbuscarproductos).
            if (lstSugerenciasProductos != null)
            {
                lstSugerenciasProductos.Visible = false;
                lstSugerenciasProductos.Enabled = false;
                lstSugerenciasProductos.TabStop = false;
            }
        }

        private void CablearDatosClienteUi()
        {
            if (_datosLaboralesCableados)
                return;

            _datosLaboralesCableados = true;

            cbClientes.SelectedIndexChanged -= cbClientes_SelectedIndexChanged;
            cbClientes.SelectedIndexChanged += cbClientes_SelectedIndexChanged;

            txtCedula.Leave -= DatosLaborales_Leave;
            txtTrabaja.Leave -= DatosLaborales_Leave;
            txtDirTrabaja.Leave -= DatosLaborales_Leave;
            txtCedula.Leave += DatosLaborales_Leave;
            txtTrabaja.Leave += DatosLaborales_Leave;
            txtDirTrabaja.Leave += DatosLaborales_Leave;

            if (txtCedula.MaxLength <= 0 || txtCedula.MaxLength > 30)
                txtCedula.MaxLength = 30;
            txtTrabaja.MaxLength = 200;
            txtDirTrabaja.MaxLength = 200;
        }

        private void CargarClientes()
        {
            try
            {
                _suppressDatosClienteUi = true;
                DataTable dt = clienteBLL.ObtenerClientes();
                cbClientes.DataSource = dt;
                cbClientes.DisplayMember = "Nombre";
                cbClientes.ValueMember = "Id";
                cbClientes.SelectedIndex = -1;
                LimpiarCamposDatosCliente();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar clientes: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _suppressDatosClienteUi = false;
            }
        }

        private void CargarClientesPreservandoSeleccion()
        {
            int? sel = null;
            if (cbClientes?.SelectedValue != null
                && cbClientes.SelectedValue != DBNull.Value
                && int.TryParse(cbClientes.SelectedValue.ToString(), out int id)
                && id > 0)
                sel = id;

            CargarClientes();

            if (sel.HasValue && cbClientes != null)
            {
                try
                {
                    _suppressDatosClienteUi = true;
                    cbClientes.SelectedValue = sel.Value;
                }
                catch { /* cliente eliminado */ }
                finally
                {
                    _suppressDatosClienteUi = false;
                }

                // Refrescar panel con datos actuales (cédula/trabajo pueden haber cambiado).
                AplicarClienteSeleccionado();
            }
        }

        private void cbClientes_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressDatosClienteUi)
                return;

            AplicarClienteSeleccionado();
        }

        private void AplicarClienteSeleccionado()
        {
            if (cbClientes.SelectedIndex < 0
                || cbClientes.SelectedValue == null
                || cbClientes.SelectedValue == DBNull.Value
                || !int.TryParse(cbClientes.SelectedValue.ToString(), out int clienteId)
                || clienteId <= 0)
            {
                LimpiarCamposDatosCliente();
                return;
            }

            try
            {
                DataRow? row = clienteBLL.ObtenerPorId(clienteId);
                if (row == null)
                {
                    LimpiarCamposDatosCliente();
                    return;
                }

                _suppressDatosClienteUi = true;
                try
                {
                    string nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                    txtNombre.Text = string.IsNullOrEmpty(nombre) ? "—" : nombre;
                    txtDireccion.Text = row["Direccion"]?.ToString() ?? string.Empty;
                    txtTelefono.Text = row["Telefono"]?.ToString() ?? string.Empty;
                    txtCedula.Text = row.Table.Columns.Contains("Cedula")
                        ? row["Cedula"]?.ToString() ?? string.Empty
                        : string.Empty;
                    txtTrabaja.Text = row.Table.Columns.Contains("LugarTrabajo")
                        ? row["LugarTrabajo"]?.ToString() ?? string.Empty
                        : string.Empty;
                    txtDirTrabaja.Text = row.Table.Columns.Contains("DireccionTrabajo")
                        ? row["DireccionTrabajo"]?.ToString() ?? string.Empty
                        : string.Empty;

                    var ficha = clienteBLL.ObtenerFichaSalud(clienteId);
                    txtNombreEmergencia.Text = ficha?.EmergenciaNombre ?? string.Empty;
                    txtTelEmergencia.Text = ficha?.EmergenciaTelefono ?? string.Empty;
                    txtParentesco.Text = ficha?.EmergenciaParentesco ?? string.Empty;
                    _huellaLaboralGuardada = HuellaLaboralActual();
                }
                finally
                {
                    _suppressDatosClienteUi = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar el cliente: " + ex.Message,
                    "Deudas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void LimpiarCamposDatosCliente()
        {
            _suppressDatosClienteUi = true;
            try
            {
                txtNombre.Text = "NOMBRE";
                txtDireccion.Clear();
                txtTelefono.Clear();
                txtCedula.Clear();
                txtTrabaja.Clear();
                txtDirTrabaja.Clear();
                txtNombreEmergencia.Clear();
                txtTelEmergencia.Clear();
                txtParentesco.Clear();
                _huellaLaboralGuardada = string.Empty;
            }
            finally
            {
                _suppressDatosClienteUi = false;
            }
        }

        private string HuellaLaboralActual() =>
            $"{txtCedula.Text?.Trim()}\u001f{txtTrabaja.Text?.Trim()}\u001f{txtDirTrabaja.Text?.Trim()}";

        private void DatosLaborales_Leave(object? sender, EventArgs e)
        {
            if (_suppressDatosClienteUi)
                return;

            PersistirCedulaYTrabajoSiAplica();
        }

        private void PersistirCedulaYTrabajoSiAplica()
        {
            if (cbClientes.SelectedIndex < 0
                || cbClientes.SelectedValue == null
                || cbClientes.SelectedValue == DBNull.Value
                || !int.TryParse(cbClientes.SelectedValue.ToString(), out int clienteId)
                || clienteId <= 0)
                return;

            string huella = HuellaLaboralActual();
            if (string.Equals(huella, _huellaLaboralGuardada, StringComparison.Ordinal))
                return;

            _omitirRefreshCatalogoPorGuardadoLocal = true;
            try
            {
                var result = ClienteCommandService.ActualizarCedulaYTrabajo(
                    clienteId,
                    txtCedula.Text,
                    txtTrabaja.Text,
                    txtDirTrabaja.Text);

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.Message,
                        "Datos del cliente",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _huellaLaboralGuardada = huella;
            }
            finally
            {
                _omitirRefreshCatalogoPorGuardadoLocal = false;
            }
        }

        private void CargarPlanes()
        {
            try
            {
                DataTable tabla = planBLL.ObtenerPlanes().Copy();
                DataView dv = tabla.DefaultView;
                // Todos los planes reales de Planes; el pseudo-plan PRODUCTO A CRÉDITO
                // se agrega aparte más abajo. Sin nombres a mano: M-A y futuros planes entran solos.
                dv.RowFilter = $"Nombre <> '{NombreProductoCredito}' AND Nombre <> 'OFERTA'";

                DataTable opciones = tabla.Clone();
                if (!opciones.Columns.Contains("Etiqueta"))
                    opciones.Columns.Add("Etiqueta", typeof(string));

                foreach (DataRowView row in dv)
                {
                    opciones.ImportRow(row.Row);
                    DataRow importada = opciones.Rows[opciones.Rows.Count - 1];
                    string nombrePlan = importada["Nombre"]?.ToString()?.Trim() ?? string.Empty;
                    importada["Etiqueta"] = "Plan: " + nombrePlan;
                }

                DataRow credito = opciones.NewRow();
                credito["Id"] = PlanIdProductoCredito;
                credito["Nombre"] = NombreProductoCredito;
                credito["Precio"] = 0m;
                if (opciones.Columns.Contains("DuracionDias"))
                    credito["DuracionDias"] = 0;
                credito["Etiqueta"] = "Producto a crédito (solo suplementos)";
                opciones.Rows.Add(credito);

                cmbTipoPlan.DisplayMember = "Etiqueta";
                cmbTipoPlan.ValueMember = "Id";
                cmbTipoPlan.DataSource = opciones;
                cmbTipoPlan.SelectedIndex = -1;

                txtMonto.Text = "0.00";
                _precioPlan = 0m;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar planes: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CargarProductosInventario()
        {
            try
            {
                _productos = productoBLL.ObtenerProductos() ?? new DataTable();
                CargarProductosEnComboBusqueda();
            }
            catch (Exception ex)
            {
                _productos = new DataTable();
                CargarProductosEnComboBusqueda();
                MessageBox.Show(
                    "Error al cargar productos: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool EsProductoCreditoSeleccionado()
        {
            if (cmbTipoPlan.SelectedIndex < 0 || cmbTipoPlan.SelectedValue == null)
                return false;

            if (int.TryParse(cmbTipoPlan.SelectedValue.ToString(), out int id) && id == PlanIdProductoCredito)
                return true;

            return string.Equals(cmbTipoPlan.Text?.Trim(), NombreProductoCredito, StringComparison.OrdinalIgnoreCase);
        }

        private void AplicarModoProductoCredito(bool activo)
        {
            lblBuscarProducto.Enabled = activo;
            cmbbuscarproductos.Enabled = activo;
            lblCantidad.Enabled = activo;
            numCantidad.Enabled = activo;

            // Deja claro en UI: crédito de producto = solo Suplementos.
            if (lblBuscarProducto != null)
                lblBuscarProducto.Text = activo ? "SUPLEMENTO" : "PRODUCTO";

            txtConcepto.Enabled = true;
            txtPagodeinicio.Enabled = true;
            pnlCrearDeuda.Enabled = true;
            pnlOperacion.Enabled = true;

            if (!activo)
                RestablecerComboBuscarProductos();

            if (activo && numCantidad.Value < 1)
                numCantidad.Value = 1;

            SincronizarBotonesProducto();
        }

        private void SincronizarBotonesProducto()
        {
            bool productoCredito = EsProductoCreditoSeleccionado();
            btnagregar.Enabled = productoCredito && _productoPendiente != null;
            btnlimpiar.Enabled = productoCredito && (_productoPendiente != null || _lineas.Count > 0);
        }

        private void LimpiarProductosCredito()
        {
            _lineas.Clear();
            _productoPendiente = null;
            _precioUnitarioPendiente = 0m;
            RestablecerComboBuscarProductos();
            ResetearCantidad();
        }

        private void ResetearCantidad()
        {
            numCantidad.Maximum = CantidadMaximaSinProducto;
            numCantidad.Value = 1;
        }

        private void cmbTipoPlan_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _precioPlan = 0m;
            LimpiarProductosCredito();

            bool productoCredito = EsProductoCreditoSeleccionado();
            AplicarModoProductoCredito(productoCredito);

            if (productoCredito)
            {
                txtMonto.Text = "0.00";
                txtConcepto.Clear();
                if (string.IsNullOrWhiteSpace(txtPagodeinicio.Text))
                    txtPagodeinicio.Text = "0";
                CalcularSaldoRestante();
                return;
            }

            ResetearInteresesPlazosUi();
            pnlIntereses.Enabled = false;

            if (cmbTipoPlan.SelectedItem is DataRowView row &&
                row["Precio"] != DBNull.Value)
            {
                _precioPlan = Convert.ToDecimal(row["Precio"]);
                string nombrePlan = row["Nombre"]?.ToString() ?? "plan";

                txtMonto.Text = _precioPlan.ToString("N2");

                if (string.IsNullOrWhiteSpace(txtConcepto.Text) ||
                    txtConcepto.Text.StartsWith("Saldo plan ", StringComparison.OrdinalIgnoreCase) ||
                    txtConcepto.Text.StartsWith("Financiamiento ", StringComparison.OrdinalIgnoreCase) ||
                    EsConceptoProductoCreditoAuto(txtConcepto.Text))
                {
                    txtConcepto.Text = $"Financiamiento {nombrePlan}";
                }
            }
            else
            {
                txtMonto.Text = "0.00";
            }

            if (string.IsNullOrWhiteSpace(txtPagodeinicio.Text))
                txtPagodeinicio.Text = "0";

            CalcularSaldoRestante();
        }

        private void DescartarProductoPendiente()
        {
            if (_productoPendiente == null)
            {
                SincronizarBotonesProducto();
                return;
            }

            _productoPendiente = null;
            _precioUnitarioPendiente = 0m;
            ResetearCantidad();
            RecalcularProductoCredito();
            SincronizarBotonesProducto();
        }

        /// <summary>
        /// Vacía los productos agregados y la selección pendiente para empezar de cero.
        /// </summary>
        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            if (!EsProductoCreditoSeleccionado())
                return;

            LimpiarProductosCredito();
            RecalcularProductoCredito();
            SincronizarBotonesProducto();
            cmbbuscarproductos.Focus();
        }

        /// <summary>
        /// Confirma el producto elegido en cmbbuscarproductos como pendiente de AGREGAR.
        /// </summary>
        private void ConfirmarProductoSeleccionadoDesdeCombo(DataRow fila)
        {
            if (!EsProductoCreditoSeleccionado() || fila == null)
                return;

            if (!EsCategoriaSuplemento(fila))
            {
                MessageBox.Show(
                    "En Producto a crédito solo se permiten productos de la categoría Suplementos (inventario).",
                    "Categoría no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                RestablecerComboBuscarProductos();
                DescartarProductoPendiente();
                return;
            }

            int productoId = Convert.ToInt32(fila["Id"]);
            string nombre = LeerNombre(fila);
            int stock = LeerStock(fila);
            int disponible = stock - CantidadAgregada(productoId);

            if (disponible < 1)
            {
                MessageBox.Show(
                    $"Ya agregó todo el stock disponible de {nombre} ({stock}).",
                    "Stock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _productoPendiente = fila;
            _precioUnitarioPendiente = fila.Table.Columns.Contains("PrecioVenta")
                && fila["PrecioVenta"] != DBNull.Value
                ? Convert.ToDecimal(fila["PrecioVenta"])
                : 0m;

            numCantidad.Maximum = disponible;
            numCantidad.Value = 1;

            RecalcularProductoCredito();
            SincronizarBotonesProducto();
            numCantidad.Focus();
        }

        private void numCantidad_ValueChanged(object? sender, EventArgs e)
        {
            if (!EsProductoCreditoSeleccionado())
                return;

            RecalcularProductoCredito();
        }

        private void btnagregar_Click(object sender, EventArgs e)
        {
            if (!EsProductoCreditoSeleccionado())
                return;

            if (_productoPendiente == null)
            {
                MessageBox.Show("Busque y seleccione un suplemento del inventario.");
                cmbbuscarproductos.Focus();
                return;
            }

            if (!EsCategoriaSuplemento(_productoPendiente))
            {
                MessageBox.Show(
                    "En Producto a crédito solo se permiten productos de la categoría Suplementos (inventario).",
                    "Categoría no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DescartarProductoPendiente();
                return;
            }

            int productoId = Convert.ToInt32(_productoPendiente["Id"]);
            string nombre = LeerNombre(_productoPendiente);
            int cantidad = (int)numCantidad.Value;

            if (cantidad < 1)
            {
                MessageBox.Show("La cantidad debe ser al menos 1.");
                return;
            }

            if (_precioUnitarioPendiente <= 0)
            {
                MessageBox.Show($"{nombre} no tiene un precio de venta válido.");
                return;
            }

            int stock = LeerStock(_productoPendiente);
            int yaAgregado = CantidadAgregada(productoId);
            if (yaAgregado + cantidad > stock)
            {
                MessageBox.Show(
                    $"Stock insuficiente de {nombre}. Disponible: {stock - yaAgregado} (ya agregados: {yaAgregado}).",
                    "Stock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            AcumularLinea(productoId, nombre, _precioUnitarioPendiente, cantidad);

            // El pendiente ya está confirmado: concepto y monto no cambian de valor.
            _productoPendiente = null;
            _precioUnitarioPendiente = 0m;
            RestablecerComboBuscarProductos();

            ResetearCantidad();
            RecalcularProductoCredito();
            SincronizarBotonesProducto();
            cmbbuscarproductos.Focus();
        }

        private void AcumularLinea(int productoId, string nombre, decimal precioUnitario, int cantidad)
        {
            foreach (var linea in _lineas)
            {
                if (linea.ProductoId == productoId)
                {
                    linea.Cantidad += cantidad;
                    return;
                }
            }

            _lineas.Add(new LineaProductoCredito
            {
                ProductoId = productoId,
                Nombre = nombre,
                PrecioUnitario = precioUnitario,
                Cantidad = cantidad
            });
        }

        /// <summary>
        /// Líneas confirmadas + el producto pendiente del buscador (preview en tiempo real).
        /// </summary>
        private List<LineaProductoCredito> ObtenerLineasEfectivas()
        {
            var efectivas = new List<LineaProductoCredito>();
            foreach (var linea in _lineas)
            {
                efectivas.Add(new LineaProductoCredito
                {
                    ProductoId = linea.ProductoId,
                    Nombre = linea.Nombre,
                    PrecioUnitario = linea.PrecioUnitario,
                    Cantidad = linea.Cantidad
                });
            }

            if (_productoPendiente == null)
                return efectivas;

            int cantidad = (int)numCantidad.Value;
            if (cantidad < 1) cantidad = 1;

            int productoId = Convert.ToInt32(_productoPendiente["Id"]);
            foreach (var linea in efectivas)
            {
                if (linea.ProductoId == productoId)
                {
                    linea.Cantidad += cantidad;
                    return efectivas;
                }
            }

            efectivas.Add(new LineaProductoCredito
            {
                ProductoId = productoId,
                Nombre = LeerNombre(_productoPendiente),
                PrecioUnitario = _precioUnitarioPendiente,
                Cantidad = cantidad
            });

            return efectivas;
        }

        private void RecalcularProductoCredito()
        {
            var lineas = ObtenerLineasEfectivas();

            decimal total = 0m;
            var concepto = new StringBuilder();

            foreach (var linea in lineas)
            {
                total += linea.Total;

                if (concepto.Length > 0)
                    concepto.Append(", ");

                concepto.Append(linea.Cantidad).Append(' ').Append(linea.Nombre);
            }

            _precioPlan = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            txtMonto.Text = _precioPlan.ToString("N2");
            txtConcepto.Text = concepto.Length > 0
                ? concepto.Append(" a credito").ToString()
                : string.Empty;

            CalcularSaldoRestante();
        }

        private int CantidadAgregada(int productoId)
        {
            int total = 0;
            foreach (var linea in _lineas)
            {
                if (linea.ProductoId == productoId)
                    total += linea.Cantidad;
            }

            return total;
        }

        private static string LeerNombre(DataRow row) =>
            row.Table.Columns.Contains("Nombre") && row["Nombre"] != DBNull.Value
                ? row["Nombre"].ToString()?.Trim() ?? "producto"
                : "producto";

        private static int LeerStock(DataRow row) =>
            row.Table.Columns.Contains("StockActual") && row["StockActual"] != DBNull.Value
                ? Convert.ToInt32(row["StockActual"])
                : 0;

        /// <summary>
        /// Misma regla que reportes de suplementos: nombre de categoría contiene "SUPLEMENTO".
        /// Ej.: Suplementos / Suplemento (inventario Categorias).
        /// </summary>
        private static bool EsCategoriaSuplemento(DataRow? fila)
        {
            if (fila == null || !fila.Table.Columns.Contains("Categoria"))
                return false;

            string cat = fila["Categoria"]?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(cat))
                return false;

            return cat.ToUpperInvariant().Contains("SUPLEMENTO", StringComparison.Ordinal);
        }

        private bool EsProductoSuplementoPorId(int productoId)
        {
            if (productoId <= 0 || _productos.Rows.Count == 0 || !_productos.Columns.Contains("Id"))
                return false;

            DataRow[] filas = _productos.Select("Id = " + productoId);
            return filas.Length > 0 && EsCategoriaSuplemento(filas[0]);
        }

        private int LeerStockPorId(int productoId)
        {
            if (_productos.Rows.Count == 0 || !_productos.Columns.Contains("Id"))
                return 0;

            DataRow[] filas = _productos.Select("Id = " + productoId);
            return filas.Length > 0 ? LeerStock(filas[0]) : 0;
        }

        private static bool EsConceptoProductoCreditoAuto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;
            return texto.TrimEnd().EndsWith(" a credito", StringComparison.OrdinalIgnoreCase);
        }

        private void txtPagodeinicio_TextChanged(object? sender, EventArgs e)
        {
            CalcularSaldoRestante();
        }

        private void txtPagodeinicio_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void CalcularSaldoRestante()
        {
            decimal pagoInicio = decimal.TryParse(txtPagodeinicio.Text, out decimal p) ? p : 0m;
            if (pagoInicio < 0) pagoInicio = 0m;

            decimal saldo = _precioPlan - pagoInicio;
            if (saldo < 0) saldo = 0m;

            lblSaldorestante.Text = $"${saldo:N2}";
            AplicarDisponibilidadInteresesProductoCredito();
        }

        /// <summary>
        /// Sin UI de fecha límite: producto → última cuota del cronograma; membresía → hoy+30.
        /// </summary>
        private DateTime? ResolverFechaVencimientoDeuda(decimal saldo)
        {
            if (saldo <= 0m)
                return null;

            if (EsProductoCreditoSeleccionado()
                && _ultimoCronogramaUi?.Cuotas is { Count: > 0 } cuotas)
            {
                return cuotas[^1].FechaVencimiento.Date;
            }

            return DateTime.Today.AddDays(30);
        }

        private bool TryObtenerPlanSeleccionado(out int planId, out string nombrePlan)
        {
            planId = 0;
            nombrePlan = string.Empty;

            if (cmbTipoPlan.SelectedValue == null || cmbTipoPlan.SelectedIndex < 0)
                return false;

            if (!int.TryParse(cmbTipoPlan.SelectedValue.ToString(), out planId))
                return false;

            // -1 = producto a crédito (válido); 0 o negativo distinto de sentinel = inválido
            if (planId == PlanIdProductoCredito)
            {
                nombrePlan = NombreProductoCredito;
                return true;
            }

            if (planId <= 0)
                return false;

            if (cmbTipoPlan.SelectedItem is DataRowView row)
                nombrePlan = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
            else
                nombrePlan = cmbTipoPlan.Text?.Trim() ?? string.Empty;

            // Defensa: solo planes reales de membresía.
            if (!EsNombrePlanMembresia(nombrePlan))
                return false;

            return true;
        }

        /// <summary>
        /// Plan real de membresía: cualquier nombre que venga de Planes.
        /// Solo se descarta el pseudo-plan de producto a crédito, que se valida por su Id.
        /// </summary>
        private static bool EsNombrePlanMembresia(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            return !nombre.Trim().Equals(NombreProductoCredito, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerificarCajaSiHayPagoInicial(decimal pagoInicial)
        {
            if (pagoInicial <= 0)
                return true;

            if (cajaBLL.ObtenerCajaAbiertaHoy() != null)
                return true;

            MessageBox.Show(
                "Hay un pago de inicio. Debe haber una caja abierta para registrar el ingreso en POS.",
                "Caja cerrada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private bool Validar()
        {
            if (cbClientes.SelectedIndex < 0 || cbClientes.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un cliente.");
                return false;
            }

            if (!TryObtenerPlanSeleccionado(out _, out _))
            {
                MessageBox.Show("Seleccione una operación: un plan de membresía o Producto a crédito.");
                return false;
            }

            if (EsProductoCreditoSeleccionado())
            {
                var lineas = ObtenerLineasEfectivas();
                if (lineas.Count == 0)
                {
                    MessageBox.Show("Busque un producto del inventario y pulse AGREGAR.");
                    cmbbuscarproductos.Focus();
                    return false;
                }

                foreach (var linea in lineas)
                {
                    if (linea.Cantidad < 1)
                    {
                        MessageBox.Show($"La cantidad de {linea.Nombre} debe ser al menos 1.");
                        return false;
                    }

                    int stock = LeerStockPorId(linea.ProductoId);
                    if (linea.Cantidad > stock)
                    {
                        MessageBox.Show($"Stock insuficiente de {linea.Nombre}. Disponible: {stock}.");
                        return false;
                    }
                }
            }

            if (_precioPlan <= 0)
            {
                MessageBox.Show(EsProductoCreditoSeleccionado()
                    ? "Los productos seleccionados no tienen un precio de venta válido."
                    : "El plan seleccionado no tiene un precio válido.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtConcepto.Text))
            {
                MessageBox.Show("Ingrese un concepto.");
                return false;
            }

            decimal pagoInicio = decimal.TryParse(txtPagodeinicio.Text, out decimal p) ? p : -1m;
            if (pagoInicio < 0 || pagoInicio > _precioPlan)
            {
                MessageBox.Show("Pago de inicio inválido. Debe estar entre 0 y el monto total.");
                return false;
            }

            decimal saldo = _precioPlan - pagoInicio;
            if (saldo < 0) saldo = 0m;

            // Producto a crédito con saldo: exige cronograma del módulo nuevo.
            if (EsProductoCreditoSeleccionado() && saldo > 0)
            {
                if (!TryObtenerCronogramaUi(out _, out string errorCronograma))
                {
                    MessageBox.Show(errorCronograma, "Plazos e interés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validar()) return;

                int clienteId = Convert.ToInt32(cbClientes.SelectedValue);
                if (!TryObtenerPlanSeleccionado(out int planId, out string nombrePlan))
                    return;

                decimal pagoInicio = decimal.TryParse(txtPagodeinicio.Text, out decimal p) ? p : 0m;
                decimal saldo = _precioPlan - pagoInicio;
                if (saldo < 0) saldo = 0m;

                if (!VerificarCajaSiHayPagoInicial(pagoInicio))
                    return;

                string usuario = string.IsNullOrWhiteSpace(Sesion.Usuario) ? "ADMIN" : Sesion.Usuario;
                string conceptoPago = string.IsNullOrWhiteSpace(txtConcepto.Text)
                    ? (EsProductoCreditoSeleccionado()
                        ? "Producto a credito"
                        : $"Pago inicial - Membresía {nombrePlan}")
                    : txtConcepto.Text.Trim();

                DateTime? fechaVencimientoDeuda = ResolverFechaVencimientoDeuda(saldo);

                // Producto a crédito: no bloquea por deuda pendiente (es venta, no plan).
                if (EsProductoCreditoSeleccionado())
                {
                    GuardarProductoCredito(clienteId, pagoInicio, saldo, conceptoPago, fechaVencimientoDeuda, usuario);
                    return;
                }

                if (AvisoDeudaPendiente.BloqueaOperacionDePlan(this, clienteId, deudaBLL))
                    return;

                if (membresiaBLL.ClienteNoElegibleParaFinanciamiento(clienteId, out string motivoFin))
                {
                    MessageBox.Show(
                        motivoFin,
                        "Financiamiento no disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Ciclo completo POS: membresía + deuda + caja + historial (misma TX en BLL).
                var result = MembresiaCommandService.VenderMembresiaFinanciada(
                    clienteId,
                    planId,
                    pagoInicio,
                    "Efectivo",
                    conceptoPago,
                    fechaVencimientoDeuda,
                    usuario);

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                MostrarExitoYLimpiar(nombrePlan, pagoInicio, saldo, fechaVencimientoDeuda);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void GuardarProductoCredito(
            int clienteId,
            decimal pagoInicio,
            decimal saldo,
            string concepto,
            DateTime? fechaVencimientoDeuda,
            string usuario)
        {
            var lineas = ObtenerLineasEfectivas();
            if (lineas.Count == 0)
                return;

            foreach (var linea in lineas)
            {
                if (EsProductoSuplementoPorId(linea.ProductoId))
                    continue;

                MessageBox.Show(
                    $"«{linea.Nombre}» no pertenece a la categoría Suplementos.\n" +
                    "Producto a crédito solo admite suplementos registrados en inventario.",
                    "Categoría no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            PrestamoCronogramaDto? cronograma = null;
            if (saldo > 0)
            {
                if (!TryObtenerCronogramaUi(out PrestamoCronogramaDto c, out string errorCronograma))
                {
                    MessageBox.Show(errorCronograma, "Plazos e interés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cronograma = c;
                fechaVencimientoDeuda = c.Cuotas[^1].FechaVencimiento.Date;
            }

            var carrito = new DataTable();
            carrito.Columns.Add("ProductoId", typeof(int));
            carrito.Columns.Add("Producto", typeof(string));
            carrito.Columns.Add("Precio", typeof(decimal));
            carrito.Columns.Add("Cantidad", typeof(int));
            carrito.Columns.Add("Total", typeof(decimal));

            decimal total = 0m;
            foreach (var linea in lineas)
            {
                carrito.Rows.Add(
                    linea.ProductoId,
                    linea.Nombre,
                    linea.PrecioUnitario,
                    linea.Cantidad,
                    linea.Total);
                total += linea.Total;
            }

            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

            // Misma tubería POS: venta + stock + caja + deuda (capital).
            // Si hay cronograma, el WhatsApp se envía después del préstamo (detalle con interés/plazos).
            var result = VentasCommandService.RegistrarVentaPOS(
                clienteId,
                total,
                pagoInicio,
                "Efectivo",
                carrito,
                usuario,
                fechaVencimientoDeuda,
                concepto,
                omitirNotificacionDeuda: cronograma != null);

            if (!result.Success)
            {
                MessageBox.Show(
                    result.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            decimal saldoMostrar = saldo;
            string extraPrestamo = string.Empty;

            if (cronograma != null
                && result.Payload is BLL.Models.VentaOperacionResult operacion
                && operacion.DeudaId > 0)
            {
                try
                {
                    PersistirCedulaYTrabajoSiAplica();
                    new PrestamoCuotasBLL().RegistrarTrasDeudaProductoCredito(
                        operacion.DeudaId,
                        clienteId,
                        cronograma,
                        usuario,
                        concepto);

                    saldoMostrar = cronograma.TotalConInteres;
                    extraPrestamo =
                        $"\nPlazos: {cronograma.NumeroPlazos} ({cronograma.Frecuencia})" +
                        $"\nInterés fijo: {cronograma.InteresPorcentaje:N2}% (${cronograma.InteresTotal:N2})" +
                        $"\nTotal con interés: ${cronograma.TotalConInteres:N2}" +
                        (cronograma.ActivarMora ? "\nMora: activa ($50/día desde día 3)" : "\nMora: desactivada");
                }
                catch (Exception exPrestamo)
                {
                    // Fallback: aviso clásico si el cronograma falló tras crear la deuda.
                    try
                    {
                        new DeudaBLL().NotificarDeudaCreadaPostCommit(
                            clienteId,
                            concepto,
                            saldo,
                            fechaVencimientoDeuda ?? DateTime.Today.AddDays(30),
                            operacion.DeudaId,
                            pagoInicio > 0);
                    }
                    catch
                    {
                        // No bloquear el flujo de UI por el aviso.
                    }

                    MessageBox.Show(
                        "La venta/deuda se registró, pero falló el cronograma de plazos:\n" + exPrestamo.Message +
                        "\n\nRevise la deuda en Gestión y complete el préstamo si hace falta.",
                        "Plazos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else if (saldo > 0 && cronograma != null)
            {
                MessageBox.Show(
                    "La venta se registró pero no se obtuvo el Id de deuda para guardar plazos.",
                    "Plazos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            CargarProductosInventario();
            MostrarExitoYLimpiar(
                NombreProductoCredito,
                pagoInicio,
                saldoMostrar,
                fechaVencimientoDeuda,
                esProducto: true,
                extraPrestamo);
        }

        private void MostrarExitoYLimpiar(
            string nombrePlan,
            decimal pagoInicio,
            decimal saldo,
            DateTime? fechaVencimientoDeuda = null,
            bool esProducto = false,
            string? extraPrestamo = null)
        {
            string resumen =
                (esProducto
                    ? "Producto a crédito registrado correctamente.\n\n"
                    : "Deuda/financiamiento registrado correctamente.\n\n") +
                $"Cliente: {cbClientes.Text}\n" +
                $"{(esProducto ? "Tipo" : "Plan")}: {nombrePlan}\n" +
                $"Concepto: {txtConcepto.Text}\n" +
                $"Monto: ${_precioPlan:N2}\n" +
                $"Pago de inicio: ${pagoInicio:N2}\n" +
                $"Saldo pendiente: ${saldo:N2}\n" +
                (fechaVencimientoDeuda.HasValue
                    ? $"Vence deuda: {fechaVencimientoDeuda.Value:dd/MM/yyyy}\n"
                    : string.Empty) +
                (extraPrestamo ?? string.Empty) +
                (esProducto
                    ? "\nQueda reflejado en Inventario, Historial de ventas" +
                      (saldo > 0 ? ", Deudas" : string.Empty) +
                      (pagoInicio > 0 ? " y Caja." : ".")
                    : "\nQueda reflejado en Estado Clientes, Historial de Membresía" +
                      (pagoInicio > 0 ? " y Caja." : "."));

            MessageBox.Show(
                resumen,
                "Éxito",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LimpiarFormularioCrearDeuda();
        }

        private void LimpiarFormularioCrearDeuda()
        {
            _suppressDatosClienteUi = true;
            try
            {
                cbClientes.SelectedIndex = -1;
            }
            finally
            {
                _suppressDatosClienteUi = false;
            }

            LimpiarCamposDatosCliente();
            cmbTipoPlan.SelectedIndex = -1;
            LimpiarProductosCredito();
            txtConcepto.Clear();
            txtMonto.Text = "0.00";
            txtPagodeinicio.Text = "0";
            _precioPlan = 0m;
            lblSaldorestante.Text = "$0.00";
            AplicarModoProductoCredito(false);
            CalcularSaldoRestante();
            ResetearInteresesPlazosUi();
            pnlIntereses.Enabled = false;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormularioCrearDeuda();
        }
    }
}
