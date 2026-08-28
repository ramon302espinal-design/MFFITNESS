using BLL;
using BLL.Commands;
using BLL.Models;
using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
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
        private readonly BindingSource _bsProductos = new BindingSource();
        private readonly StringBuilder _bufferEscannerPos = new();
        private DateTime _ultimaTeclaEscannerPos = DateTime.MinValue;
        private readonly PosScannerIntervalGate _intervaloEscannerPos = new();
        private int _hoverProductoIdPos = -1;
        /// <summary>Id del producto cuya foto está en picProductoPos (no se limpia al salir del listado).</summary>
        private int _fotoProductoIdPos = -1;
        private string? _rutaFotoProductoPosActual;
        private ToolTip? _toolTipFotoProductoPos;
        private bool _fotoProductoPosBusy;
        private bool _toolbarFotoProductoPosListo;
        /// <summary>Historial Undo en JPEG (independiente de GDI+/PictureBox).</summary>
        private readonly List<byte[]> _undoFotoProductoPos = new();
        private const int MaxUndoFotoProductoPos = 12;

        private FrmPresentacion? _presentacion;
        private readonly ClienteBLL clienteBLL = new ClienteBLL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();
        private readonly VentaPausadaBLL ventaPausadaBLL = new VentaPausadaBLL();
        private string? _codigoBarraInicial;
        private bool _suppressPausaUi;
        private int? _pausaVistaId;

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

            txtCantidad.Text = "1";
            txtCantidad.Enabled = false;
            lblCantidad.Enabled = false;

            if (dtpVenceDeudaProducto != null)
            {
                dtpVenceDeudaProducto.Value = DateTime.Today.AddDays(30);
                dtpVenceDeudaProducto.MinDate = DateTime.Today;
            }

            if (panelFinanciamientoProducto != null)
                panelFinanciamientoProducto.Visible = false;

            if (pnlPausarVentas != null)
            {
                pnlPausarVentas.Visible = false;
                ConfigurarGridPausaVentas();
                RefrescarMiembrosPausados();
            }

            if (chkPausarVenta != null)
                chkPausarVenta.Checked = false;

            ConfigurarCapturaEscannerPos();
            ProcesarEscaneoInicialPendiente();
            ConfigurarToolbarFotoProductoPos();
            ActualizarEstadoToolbarFotoProductoPos(false);

            if (tabProductos.SelectedTab == tabPago)
                EnfocarEscannerPos();
        }

        /// <summary>Precarga un EAN antes de ShowDialog (p. ej. escaneo desde inicio).</summary>
        public void EstablecerEscaneoInicial(string codigoBarra)
        {
            _codigoBarraInicial = codigoBarra;
        }

        /// <summary>Escaneo recibido con POS ya abierto (misma instancia).</summary>
        public void ProcesarEscaneoDesdeExterno(string codigoBarra)
        {
            ActivarTabProductos();
            ProcesarEscaneoProductoPos(codigoBarra);
        }

        private void ProcesarEscaneoInicialPendiente()
        {
            if (string.IsNullOrWhiteSpace(_codigoBarraInicial))
                return;

            string codigo = _codigoBarraInicial;
            _codigoBarraInicial = null;

            BeginInvoke(new Action(() =>
            {
                ActivarTabProductos();
                ProcesarEscaneoProductoPos(codigo);
            }));
        }

        private void ActivarTabProductos()
        {
            if (tabProductos.SelectedTab != tabPago)
                tabProductos.SelectedTab = tabPago;

            EnfocarEscannerPos();
        }

        public void ActivarTabProductosPublico() => ActivarTabProductos();

        private void ConfigurarCapturaEscannerPos()
        {
            KeyPreview = true;
            KeyDown -= FrmPagos_CapturaEscannerKeyDown;
            KeyDown += FrmPagos_CapturaEscannerKeyDown;
        }

        private bool DebeInterceptarEscannerGlobal()
        {
            if (tabProductos.SelectedTab != tabPago && tabProductos.SelectedTab != tabMembresia)
                return false;

            if (txtBuscarProducto != null && txtBuscarProducto.Focused)
                return false;

            // Panel financiamiento abierto: permitir escribir en txtMiembroDebe / listMiembros.
            if (panelFinanciamientoProducto != null && panelFinanciamientoProducto.Visible)
                return false;

            if (txtMiembroDebe != null && txtMiembroDebe.Focused)
                return false;

            if (listMiembros != null && listMiembros.Focused)
                return false;

            if (cmbCliente.DroppedDown)
                return false;

            if (tabProductos.SelectedTab == tabMembresia && ActiveControl is TextBox)
                return false;

            return true;
        }

        /// <summary>
        /// Lee códigos aunque el foco esté en lista, grilla o pestaña Membresía (wedge HID).
        /// </summary>
        private void FrmPagos_CapturaEscannerKeyDown(object? sender, KeyEventArgs e)
        {
            PosScannerCaptureHelper.HandleKeyDown(
                e,
                _bufferEscannerPos,
                ref _ultimaTeclaEscannerPos,
                DebeInterceptarEscannerGlobal,
                raw =>
                {
                    ActivarTabProductos();
                    ProcesarEscaneoProductoPos(raw);
                });
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (tabProductos.SelectedTab == tabPago)
                EnfocarEscannerPos();
        }

        private void tabProductos_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabProductos.SelectedTab == tabPago)
                EnfocarEscannerPos();
        }

        private void EnfocarEscannerPos()
        {
            if (txtBuscarProducto == null || txtBuscarProducto.IsDisposed)
                return;

            // No robar foco mientras se busca el miembro deudor.
            if (panelFinanciamientoProducto != null && panelFinanciamientoProducto.Visible)
                return;

            BeginInvoke(new Action(() =>
            {
                if (txtBuscarProducto.IsDisposed)
                    return;
                if (panelFinanciamientoProducto != null && panelFinanciamientoProducto.Visible)
                    return;
                txtBuscarProducto.Focus();
                txtBuscarProducto.SelectAll();
            }));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        private void CargarClientes()
        {
            ClienteBLL clienteBLLLocal = new ClienteBLL();
            DataTable dt = clienteBLLLocal.ObtenerClientesParaPos();

            // ValueMember ANTES del DataSource; columna real = "Id" (no "ID").
            cmbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCliente.DisplayMember = "Nombre";
            cmbCliente.ValueMember = "Id";
            cmbCliente.DataSource = dt;
            cmbCliente.SelectedIndex = -1;

            // Copia independiente del catálogo para búsqueda TextBox + ListBox (no ComboBox).
            CargarCatalogoMiembrosDebe(dt);

            if (cmbClientePausarVenta != null)
            {
                _suppressPausaUi = true;
                try
                {
                    cmbClientePausarVenta.DropDownStyle = ComboBoxStyle.DropDownList;
                    cmbClientePausarVenta.DisplayMember = "Nombre";
                    cmbClientePausarVenta.ValueMember = "Id";
                    cmbClientePausarVenta.DataSource = dt.Copy();
                    cmbClientePausarVenta.SelectedIndex = -1;
                }
                finally
                {
                    _suppressPausaUi = false;
                }
            }
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
            DataTable productos = productoBLL.ObtenerProductos()
                ?? new DataTable();

            _bsProductos.RaiseListChangedEvents = false;
            _bsProductos.DataSource = productos;
            _bsProductos.RaiseListChangedEvents = true;

            ConfigurarListaProductos();
            AplicarFiltroBusquedaProducto();
        }

        /// <summary>
        /// lstProductosPos es el selector de productos del POS: se alimenta del
        /// BindingSource, por lo que txtBuscarProducto la filtra en vivo.
        /// </summary>
        private void ConfigurarListaProductos()
        {
            lstProductosPos.DisplayMember = "Nombre";
            lstProductosPos.ValueMember = "Id";
            lstProductosPos.DataSource = _bsProductos;
            lstProductosPos.ClearSelected();
        }

        /// <summary>
        /// Clic izquierdo suma una unidad, clic derecho resta. La lista no se cierra,
        /// así se pueden acumular varios productos seguidos.
        /// </summary>
        private void lstProductosPos_MouseDown(object? sender, MouseEventArgs e)
        {
            int index = lstProductosPos.IndexFromPoint(e.Location);
            if (index < 0 || index >= lstProductosPos.Items.Count)
                return;

            lstProductosPos.SelectedIndex = index;

            if (lstProductosPos.Items[index] is not DataRowView row)
                return;

            MostrarFotoProductoPos(row.Row);

            if (e.Button == MouseButtons.Left)
                AjustarCantidadCarrito(row.Row, 1);
            else if (e.Button == MouseButtons.Right)
                AjustarCantidadCarrito(row.Row, -1);

            EnfocarEscannerPos();
        }

        /// <summary>
        /// Hover en vivo: cambia la ventanita de foto según el ítem bajo el cursor.
        /// No altera carrito ni cobro.
        /// </summary>
        private void lstProductosPos_MouseMove(object? sender, MouseEventArgs e)
        {
            int index = lstProductosPos.IndexFromPoint(e.Location);
            if (index < 0 || index >= lstProductosPos.Items.Count)
                return;

            if (lstProductosPos.Items[index] is not DataRowView row)
                return;

            MostrarFotoProductoPos(row.Row);
        }

        private void lstProductosPos_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstProductosPos.SelectedItem is DataRowView row)
                MostrarFotoProductoPos(row.Row);
        }

        private void lstProductosPos_MouseLeave(object? sender, EventArgs e)
        {
            // Conserva la última foto vista; limpia solo el caché de hover.
            _hoverProductoIdPos = -1;
        }

        private void MostrarFotoProductoPos(DataRow producto)
        {
            if (producto == null || picProductoPos == null)
                return;

            int id = 0;
            if (producto.Table.Columns.Contains("Id")
                && producto["Id"] != DBNull.Value)
                id = Convert.ToInt32(producto["Id"]);

            // Misma foto ya en pantalla: no recargar ni borrar el historial Undo.
            if (id > 0 && id == _fotoProductoIdPos && picProductoPos.Image != null)
            {
                _hoverProductoIdPos = id;
                ActualizarEstadoToolbarFotoProductoPos(true);
                return;
            }

            _hoverProductoIdPos = id;

            // Solo limpiar Undo al cambiar de producto.
            if (id != _fotoProductoIdPos)
                LimpiarUndoFotoProductoPos();

            _fotoProductoIdPos = -1;
            _rutaFotoProductoPosActual = null;

            string? ruta = null;
            if (producto.Table.Columns.Contains("RutaImagen"))
                ruta = producto["RutaImagen"]?.ToString();

            if (string.IsNullOrWhiteSpace(ruta) && id > 0)
                ruta = ProductoImagenStorage.RutaProducto(id);

            string? real = ProductoImagenStorage.ResolverRutaExistente(ruta);
            Image? old = picProductoPos.Image;
            picProductoPos.Image = null;
            old?.Dispose();

            if (real == null)
            {
                if (lblFotoProductoPos != null)
                    lblFotoProductoPos.Text = "SIN FOTO";
                ActualizarEstadoToolbarFotoProductoPos(false);
                return;
            }

            try
            {
                // Clonar a Bitmap independiente: Image.FromStream + dispose del FileStream
                // deja la imagen inválida y rompe Undo/edición.
                using var fs = new FileStream(real, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var tmp = Image.FromStream(fs);
                picProductoPos.Image = new Bitmap(tmp);
                _fotoProductoIdPos = id;
                _rutaFotoProductoPosActual = real;
                if (lblFotoProductoPos != null)
                    lblFotoProductoPos.Text = "FOTO PRODUCTO";
                ActualizarEstadoToolbarFotoProductoPos(true);
                ReubicarToolbarFotoProductoPos();
            }
            catch
            {
                _fotoProductoIdPos = -1;
                _rutaFotoProductoPosActual = null;
                if (lblFotoProductoPos != null)
                    lblFotoProductoPos.Text = "SIN FOTO";
                ActualizarEstadoToolbarFotoProductoPos(false);
            }
        }

        private void ConfigurarToolbarFotoProductoPos()
        {
            if (_toolbarFotoProductoPosListo || panelToolbarFotoPos == null)
                return;

            _toolTipFotoProductoPos ??= new ToolTip();
            _toolTipFotoProductoPos.SetToolTip(btnUndoFotoProductoPos, "Deshacer último cambio de foto (Ctrl+Z)");
            _toolTipFotoProductoPos.SetToolTip(btnIaFotoProductoPos, "Arreglar con IA · escribe qué quieres · Ctrl+Z deshace");
            _toolTipFotoProductoPos.SetToolTip(btnRecortarFotoProductoPos, "Recortar y enderezar · se guarda al instante");
            _toolTipFotoProductoPos.SetToolTip(btnGirarFotoProductoPos, "Girar foto 90° (vertical) · se guarda al instante");

            btnUndoFotoProductoPos.Enabled = true; // el click valida si hay historial
            ReubicarToolbarFotoProductoPos();
            panelToolbarFotoPos.BringToFront();
            _toolbarFotoProductoPosListo = true;
        }

        /// <summary>Mantiene la barra encima de la esquina de la foto (no dentro del PictureBox).</summary>
        private void ReubicarToolbarFotoProductoPos()
        {
            if (panelToolbarFotoPos == null || picProductoPos == null)
                return;

            panelToolbarFotoPos.Parent = tabPago;
            panelToolbarFotoPos.Location = new Point(
                picProductoPos.Right - panelToolbarFotoPos.Width - 6,
                picProductoPos.Bottom - panelToolbarFotoPos.Height - 6);
            panelToolbarFotoPos.BringToFront();
        }

        private void ActualizarEstadoToolbarFotoProductoPos(bool visible)
        {
            ConfigurarToolbarFotoProductoPos();
            if (panelToolbarFotoPos == null)
                return;

            bool show = visible
                && !_fotoProductoPosBusy
                && picProductoPos?.Image != null
                && _fotoProductoIdPos > 0
                && !string.IsNullOrWhiteSpace(_rutaFotoProductoPosActual);

            panelToolbarFotoPos.Visible = show;
            if (show)
            {
                ReubicarToolbarFotoProductoPos();
                ActualizarEstadoBotonUndoFoto();
            }
        }

        private void btnUndoFotoProductoPos_Click(object? sender, EventArgs e)
            => DeshacerFotoProductoPos();

        private async void btnIaFotoProductoPos_Click(object? sender, EventArgs e)
            => await btnIaFotoProductoPos_ClickAsync();

        private bool PuedeEditarFotoProductoPos()
            => !_fotoProductoPosBusy
               && picProductoPos?.Image != null
               && _fotoProductoIdPos > 0
               && !string.IsNullOrWhiteSpace(_rutaFotoProductoPosActual);

        private void LimpiarUndoFotoProductoPos()
        {
            _undoFotoProductoPos.Clear();
            ActualizarEstadoBotonUndoFoto();
        }

        private void PushUndoFotoProductoPos()
        {
            if (picProductoPos?.Image == null)
                return;

            try
            {
                byte[] jpeg = ProductoImagenHelper.ToJpegBytes(
                    picProductoPos.Image,
                    maxSide: 2048,
                    quality: 92);
                if (jpeg.Length == 0)
                    return;

                _undoFotoProductoPos.Add(jpeg);
                while (_undoFotoProductoPos.Count > MaxUndoFotoProductoPos)
                    _undoFotoProductoPos.RemoveAt(0);

                ActualizarEstadoBotonUndoFoto();
            }
            catch
            {
                // No bloquear la edición si el snapshot falla.
            }
        }

        private void ActualizarEstadoBotonUndoFoto()
        {
            if (btnUndoFotoProductoPos == null)
                return;

            bool can = _undoFotoProductoPos.Count > 0 && !_fotoProductoPosBusy;
            // Siempre clickable; visual distinto si no hay historial.
            btnUndoFotoProductoPos.Enabled = !_fotoProductoPosBusy;
            btnUndoFotoProductoPos.BackColor = can
                ? Color.FromArgb(34, 197, 94)
                : Color.FromArgb(100, 116, 139);
            btnUndoFotoProductoPos.Cursor = Cursors.Hand;
        }

        private void DeshacerFotoProductoPos()
        {
            if (_fotoProductoPosBusy)
                return;

            if (_undoFotoProductoPos.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "No hay cambios de foto para deshacer.\nEdita la foto (IA / girar / recortar) y luego usa ↶ o Ctrl+Z.",
                    "Undo foto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (_fotoProductoIdPos <= 0 || string.IsNullOrWhiteSpace(_rutaFotoProductoPosActual))
                return;

            byte[] prevJpeg = _undoFotoProductoPos[^1];
            _undoFotoProductoPos.RemoveAt(_undoFotoProductoPos.Count - 1);

            try
            {
                using var ms = new MemoryStream(prevJpeg, writable: false);
                using var tmp = Image.FromStream(ms);
                var restore = new Bitmap(tmp);
                AplicarYGuardarFotoProductoPos(restore, pushUndo: false);
                ActualizarEstadoToolbarFotoProductoPos(true);
                ActualizarEstadoBotonUndoFoto();
            }
            catch (Exception ex)
            {
                ActualizarEstadoBotonUndoFoto();
                MessageBox.Show(
                    this,
                    "No se pudo deshacer el cambio de foto.\n" + ex.Message,
                    "Foto producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void AplicarYGuardarFotoProductoPos(Image nueva, bool pushUndo = true)
        {
            ArgumentNullException.ThrowIfNull(nueva);
            if (_fotoProductoIdPos <= 0 || string.IsNullOrWhiteSpace(_rutaFotoProductoPosActual))
            {
                nueva.Dispose();
                return;
            }

            if (pushUndo)
                PushUndoFotoProductoPos();

            int productoId = _fotoProductoIdPos;
            Image? old = picProductoPos!.Image;
            picProductoPos.Image = nueva;
            if (!ReferenceEquals(old, nueva))
                old?.Dispose();
            picProductoPos.Refresh();

            byte[] jpeg = ProductoImagenHelper.ToJpegBytes(nueva, maxSide: 2048, quality: 90);

            string rutaActual = _rutaFotoProductoPosActual!;
            File.WriteAllBytes(rutaActual, jpeg);

            string canonical = ProductoImagenStorage.RutaProducto(productoId);
            if (!string.Equals(
                    Path.GetFullPath(rutaActual),
                    Path.GetFullPath(canonical),
                    StringComparison.OrdinalIgnoreCase))
            {
                File.WriteAllBytes(canonical, jpeg);
            }

            try
            {
                productoBLL.ActualizarRutaImagen(productoId, canonical);
            }
            catch
            {
                // Archivo ya guardado; BD es refuerzo.
            }

            if (lstProductosPos?.SelectedItem is DataRowView drv
                && drv.Row.Table.Columns.Contains("RutaImagen")
                && drv.Row.Table.Columns.Contains("Id")
                && drv.Row["Id"] != DBNull.Value
                && Convert.ToInt32(drv.Row["Id"]) == productoId)
            {
                drv.Row["RutaImagen"] = canonical;
            }
            else if (_bsProductos.DataSource is DataTable table
                     && table.Columns.Contains("Id")
                     && table.Columns.Contains("RutaImagen"))
            {
                foreach (DataRow row in table.Rows)
                {
                    if (row["Id"] == DBNull.Value || Convert.ToInt32(row["Id"]) != productoId)
                        continue;
                    row["RutaImagen"] = canonical;
                    break;
                }
            }

            _rutaFotoProductoPosActual = File.Exists(canonical) ? canonical : rutaActual;
            ActualizarEstadoBotonUndoFoto();
        }

        private async System.Threading.Tasks.Task btnIaFotoProductoPos_ClickAsync()
        {
            if (!PuedeEditarFotoProductoPos() || picProductoPos?.Image == null)
                return;

            string peticion;
            using (var dlg = new FrmPeticionIaFoto())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                peticion = dlg.Peticion;
            }

            if (string.IsNullOrWhiteSpace(peticion))
                return;

            _fotoProductoPosBusy = true;
            ActualizarEstadoToolbarFotoProductoPos(false);
            ActualizarEstadoBotonUndoFoto();
            UseWaitCursor = true;
            try
            {
                // Clonar en UI: GDI+ no es thread-safe.
                using var clone = new Bitmap(picProductoPos.Image);
                var (mejorada, plan) = await ProductoFotoIaHelper
                    .AplicarPeticionAsync(clone, peticion)
                    .ConfigureAwait(true);

                AplicarYGuardarFotoProductoPos(mejorada);

                if (lblFotoProductoPos != null)
                {
                    lblFotoProductoPos.Text = "IA: " + plan.Resumen;
                    var t = new System.Windows.Forms.Timer { Interval = 2500 };
                    t.Tick += (_, _) =>
                    {
                        t.Stop();
                        t.Dispose();
                        if (lblFotoProductoPos != null && picProductoPos?.Image != null)
                            lblFotoProductoPos.Text = "FOTO PRODUCTO";
                    };
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo arreglar la foto con IA.\n" + ex.Message,
                    "Foto producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
                _fotoProductoPosBusy = false;
                ActualizarEstadoToolbarFotoProductoPos(picProductoPos?.Image != null);
                ActualizarEstadoBotonUndoFoto();
            }
        }

        private void btnRecortarFotoProductoPos_Click(object? sender, EventArgs e)
        {
            if (!PuedeEditarFotoProductoPos() || picProductoPos?.Image == null)
                return;

            try
            {
                using var dlg = new FrmRecortarEnderezarFoto(picProductoPos.Image);
                if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Resultado == null)
                    return;

                AplicarYGuardarFotoProductoPos(dlg.Resultado);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo recortar/enderezar la foto.\n" + ex.Message,
                    "Foto producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnGirarFotoProductoPos_Click(object? sender, EventArgs e)
        {
            if (!PuedeEditarFotoProductoPos() || picProductoPos?.Image == null)
                return;

            try
            {
                // Clonar + girar: no mutar in-place (evita rarezas al re-guardar).
                var girada = new Bitmap(picProductoPos.Image);
                girada.RotateFlip(RotateFlipType.Rotate90FlipNone);
                AplicarYGuardarFotoProductoPos(girada);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo girar/guardar la foto.\n" + ex.Message,
                    "Foto producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void lstProductosPos_KeyDown(object? sender, KeyEventArgs e)
        {
            // Enter cobra (ProcessCmdKey). Aquí solo +/- para sumar/restar unidades.
            if (lstProductosPos.SelectedItem is not DataRowView row)
                return;

            if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
            {
                AjustarCantidadCarrito(row.Row, 1);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                AjustarCantidadCarrito(row.Row, -1);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Enter dispara COBRAR de la pestaña activa (productos o membresía).
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z))
            {
                // No interferir con undo nativo de cajas de texto.
                if (ActiveControl is TextBoxBase { ReadOnly: false } or ComboBox { DropDownStyle: ComboBoxStyle.DropDown })
                    return base.ProcessCmdKey(ref msg, keyData);

                if (!_fotoProductoPosBusy && _undoFotoProductoPos.Count > 0
                    && picProductoPos?.Image != null && _fotoProductoIdPos > 0)
                {
                    DeshacerFotoProductoPos();
                    return true;
                }
            }

            if (keyData == Keys.Enter || keyData == Keys.Return)
            {
                if (txtBuscarProducto != null && txtBuscarProducto.Focused)
                    return false;

                // No cobrar con Enter mientras se busca el miembro deudor.
                if (txtMiembroDebe != null && txtMiembroDebe.Focused)
                    return false;
                if (listMiembros != null && listMiembros.Focused)
                    return false;

                if (tabProductos.SelectedTab == tabPago)
                {
                    if (btnPagarProductos.Enabled && btnPagarProductos.Visible)
                    {
                        btnPagarProductos.PerformClick();
                        return true;
                    }
                }
                else if (tabProductos.SelectedTab == tabMembresia)
                {
                    if (btnPagar.Enabled && btnPagar.Visible)
                    {
                        btnPagar.PerformClick();
                        return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Suma o resta unidades del producto en el carrito y refresca el total al instante.
        /// Al llegar a cero la línea se elimina.
        /// </summary>
        private bool AjustarCantidadCarrito(DataRow producto, int delta)
        {
            if (producto == null || delta == 0)
                return false;

            int productoId = Convert.ToInt32(producto["Id"]);
            string nombre = producto["Nombre"]?.ToString()?.Trim() ?? "producto";

            DataRow[] filas = carrito.Select("ProductoId = " + productoId);
            int cantidadActual = filas.Length > 0 ? Convert.ToInt32(filas[0]["Cantidad"]) : 0;
            int cantidadNueva = cantidadActual + delta;

            if (cantidadNueva <= 0)
            {
                if (filas.Length > 0)
                {
                    filas[0].Delete();
                    carrito.AcceptChanges();
                    CalcularTotal();
                }
                return true;
            }

            decimal precio = LeerPrecioVenta(producto);
            if (precio <= 0)
            {
                MessageBox.Show(
                    $"{nombre} no tiene un precio de venta válido.",
                    "Producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            int? stock = LeerStockActual(producto);
            if (delta > 0 && stock.HasValue && cantidadNueva > stock.Value)
            {
                MessageBox.Show(
                    $"Stock insuficiente de {nombre}. Disponible: {stock.Value}.",
                    "Stock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            decimal total = Math.Round(precio * cantidadNueva, 2, MidpointRounding.AwayFromZero);

            if (filas.Length > 0)
            {
                filas[0]["Precio"] = precio;
                filas[0]["Cantidad"] = cantidadNueva;
                filas[0]["Total"] = total;
            }
            else
            {
                carrito.Rows.Add(productoId, nombre, precio, cantidadNueva, total);
            }

            CalcularTotal();
            return true;
        }

        private static decimal LeerPrecioVenta(DataRow row) =>
            row.Table.Columns.Contains("PrecioVenta") && row["PrecioVenta"] != DBNull.Value
                ? Convert.ToDecimal(row["PrecioVenta"])
                : 0m;

        private static int? LeerStockActual(DataRow row) =>
            row.Table.Columns.Contains("StockActual") && row["StockActual"] != DBNull.Value
                ? Convert.ToInt32(row["StockActual"])
                : (int?)null;

        private void txtBuscarProducto_TextChanged(object? sender, EventArgs e)
        {
            // Siempre filtrar lstProductosPos en vivo (nombre, Id, código, categoría…).
            // El escáner sigue resolviendo con Enter → ProcesarEscaneoProductoPos.
            AplicarFiltroBusquedaProducto();
        }

        private void txtBuscarProducto_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            ActivarTabProductos();

            string texto = txtBuscarProducto?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(texto))
            {
                if (carrito.Rows.Count > 0)
                    btnPagarProductos.PerformClick();
                return;
            }

            // EAN / código interno completo → cobro por escáner (sin romper wedge).
            if (ProductoBarcodeNormalizer.TryNormalizeBarcode(texto, out _))
            {
                ProcesarEscaneoProductoPos();
                return;
            }

            // Búsqueda inteligente: 1 match → agregar; varios → enfocar el primero.
            AplicarFiltroBusquedaProducto();
            if (_bsProductos.Count == 1 && lstProductosPos.SelectedItem is DataRowView unico)
            {
                AjustarCantidadCarrito(unico.Row, 1);
                LimpiarBusquedaPos();
                return;
            }

            if (_bsProductos.Count > 0)
            {
                if (lstProductosPos.SelectedIndex < 0)
                    lstProductosPos.SelectedIndex = 0;
                lstProductosPos.Focus();
            }
        }

        /// <summary>
        /// Solo código de barras registrado (EAN / CodigoBarra). Sin popups por QR o código inexistente.
        /// </summary>
        private void ProcesarEscaneoProductoPos(string? rawOverride = null)
        {
            if (!_intervaloEscannerPos.TryAcceptScan())
            {
                if (rawOverride == null)
                    LimpiarBusquedaPos();
                return;
            }

            string? source = rawOverride ?? txtBuscarProducto?.Text;

            if (!ProductoBarcodeNormalizer.TryNormalizeBarcode(source, out string? codigo))
            {
                LimpiarBusquedaPos();
                return;
            }

            DataRow? filaExacta = productoBLL.BuscarPorCodigoBarra(codigo);
            if (filaExacta != null && AjustarCantidadCarrito(filaExacta, 1))
            {
                SeleccionarProductoEnListaPos(Convert.ToInt32(filaExacta["Id"]));
                LimpiarBusquedaPos();
                return;
            }

            LimpiarBusquedaPos();
        }

        private void SeleccionarProductoEnListaPos(int productoId)
        {
            try { lstProductosPos.SelectedValue = productoId; }
            catch { /* puede estar filtrado */ }
        }

        private void LimpiarBusquedaPos()
        {
            txtBuscarProducto!.Clear();
            AplicarFiltroBusquedaProducto();
            EnfocarEscannerPos();
        }

        private void AplicarFiltroBusquedaProducto()
        {
            if (_bsProductos.DataSource == null)
                return;

            var termino = txtBuscarProducto?.Text?.Trim() ?? string.Empty;
            object? seleccionPrevia = lstProductosPos.SelectedValue;

            try
            {
                string filtro = string.IsNullOrEmpty(termino)
                    ? string.Empty
                    : BusquedaGridHelper.ConstruirFiltroProductosPos(termino);

                _bsProductos.Filter = string.IsNullOrEmpty(filtro) ? null : filtro;

                if (seleccionPrevia != null)
                {
                    try { lstProductosPos.SelectedValue = seleccionPrevia; }
                    catch { /* ya no está en el filtro */ }
                }

                if (lstProductosPos.SelectedIndex < 0 && _bsProductos.Count == 1)
                    lstProductosPos.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtro productos POS: {ex.Message}");
                try { _bsProductos.RemoveFilter(); }
                catch { /* ignore */ }
            }
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
            // El borrado por línea lo maneja dgvCarrito_KeyDown (Delete / Insert).
            dgvCarrito.AllowUserToDeleteRows = false;
            dgvCarrito.ReadOnly = true;
            dgvCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ThemeApplier.ApplyReadOnlyGridBehavior(dgvCarrito);
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

        /// <summary>
        /// Delete / Insert elimina la línea seleccionada del carrito.
        /// </summary>
        private void dgvCarrito_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete && e.KeyCode != Keys.Insert)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (dgvCarrito.CurrentRow?.DataBoundItem is not DataRowView row)
                return;

            row.Row.Delete();
            carrito.AcceptChanges();
            CalcularTotal();
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

        private void CalcularTotal()
        {
            decimal total = 0;
            foreach (DataRow row in carrito.Rows)
                total += Convert.ToDecimal(row["Total"]);

            lblTotal.Text = total.ToString("0.00");

            if (panelFinanciamientoProducto != null && panelFinanciamientoProducto.Visible)
                RefrescarPanelFinanciamientoProducto();
        }

        // ===============================
        // VENTAS PAUSADAS (hold carrito)
        // ===============================

        private void ConfigurarGridPausaVentas()
        {
            if (dgvPausaVentas == null)
                return;

            dgvPausaVentas.AllowUserToAddRows = false;
            dgvPausaVentas.AllowUserToDeleteRows = false;
            dgvPausaVentas.ReadOnly = true;
            dgvPausaVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPausaVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ThemeApplier.ApplyReadOnlyGridBehavior(dgvPausaVentas);
            LimpiarVistaPausa();
        }

        private void MostrarPanelPausarVentas(bool visible)
        {
            if (pnlPausarVentas == null)
                return;

            pnlPausarVentas.Visible = visible;
            if (visible)
            {
                pnlPausarVentas.BringToFront();
                RefrescarMiembrosPausados();
            }

            if (chkPausarVenta != null && chkPausarVenta.Checked != visible)
            {
                _suppressPausaUi = true;
                try { chkPausarVenta.Checked = visible; }
                finally { _suppressPausaUi = false; }
            }
        }

        private void chkPausarVenta_CheckedChanged(object? sender, EventArgs e)
        {
            if (_suppressPausaUi || chkPausarVenta == null)
                return;

            MostrarPanelPausarVentas(chkPausarVenta.Checked);
        }

        private void btnCerrarPnlPausa_Click(object? sender, EventArgs e)
        {
            MostrarPanelPausarVentas(false);
        }

        private void cmbClientePausarVenta_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressPausaUi)
                return;

            if (!TryObtenerClientePausaAsignar(out int clienteId, out string nombre))
            {
                LimpiarVistaPausa();
                return;
            }

            // Con carrito: pausar (click/selección izquierda).
            if (carrito.Rows.Count > 0)
            {
                EjecutarPausaCarrito(clienteId, nombre);
                return;
            }

            // Sin carrito: solo simular si ya tiene pausa.
            int? pausaId = ventaPausadaBLL.ObtenerIdPausaActivaPorCliente(clienteId);
            if (pausaId.HasValue)
                MostrarDetallePausa(pausaId.Value);
            else
            {
                lblNombrePausaVenta.Text = nombre;
                lblTotalPausaVenta.Text = "$0.00";
                dgvPausaVentas.DataSource = null;
                _pausaVistaId = null;
            }
        }

        private void cmbClientePausarVenta_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (!TryObtenerClientePausaAsignar(out int clienteId, out string nombre))
            {
                MessageBox.Show("Seleccione un miembro para eliminar su pausa.");
                return;
            }

            if (!ventaPausadaBLL.TienePausaActiva(clienteId))
            {
                MessageBox.Show($"{nombre} no tiene venta en pausa.");
                return;
            }

            var ok = MessageBox.Show(
                $"¿Eliminar la venta en pausa de {nombre}?",
                "Cancelar pausa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ok != DialogResult.Yes)
                return;

            try
            {
                ventaPausadaBLL.CancelarPorCliente(clienteId);
                RefrescarMiembrosPausados();
                LimpiarVistaPausa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbMiembroPausados_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressPausaUi)
                return;

            if (!TryObtenerPausaSeleccionada(out int pausaId, out _))
            {
                LimpiarVistaPausa();
                return;
            }

            MostrarDetallePausa(pausaId);
        }

        private void btnDespausar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!TryObtenerPausaSeleccionada(out int pausaId, out string nombre)
                    && !_pausaVistaId.HasValue)
                {
                    MessageBox.Show(
                        "Seleccione un miembro en pausa.",
                        "Despausar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (!TryObtenerPausaSeleccionada(out pausaId, out nombre))
                    pausaId = _pausaVistaId!.Value;

                var cabecera = ventaPausadaBLL.ObtenerCabeceraActiva(pausaId);
                if (cabecera == null)
                {
                    MessageBox.Show("La pausa ya no está activa.");
                    RefrescarMiembrosPausados();
                    LimpiarVistaPausa();
                    return;
                }

                nombre = cabecera["ClienteNombre"]?.ToString() ?? nombre;

                if (carrito.Rows.Count > 0)
                {
                    var cont = MessageBox.Show(
                        "Hay productos en el carrito actual.\n\n¿Reemplazarlos por la venta pausada?",
                        "Despausar",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (cont != DialogResult.Yes)
                        return;
                }

                DataTable detalle = ventaPausadaBLL.Despausar(pausaId);
                RestaurarCarritoDesdeDetalle(detalle);
                RefrescarMiembrosPausados();
                LimpiarVistaPausa();
                MostrarPanelPausarVentas(false);

                MessageBox.Show(
                    $"Venta de {nombre} restaurada en el carrito.",
                    "Despausar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EjecutarPausaCarrito(int clienteId, string nombre)
        {
            try
            {
                if (carrito.Rows.Count == 0)
                {
                    MessageBox.Show("El carrito está vacío.");
                    return;
                }

                if (ventaPausadaBLL.TienePausaActiva(clienteId))
                {
                    var cont = MessageBox.Show(
                        $"{nombre} ya tiene una venta en pausa.\n\n¿Reemplazarla con el carrito actual?",
                        "Pausa existente",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (cont != DialogResult.Yes)
                        return;
                }

                int pausaId = ventaPausadaBLL.PausarCarrito(
                    clienteId,
                    nombre,
                    carrito,
                    Sesion.Usuario);

                carrito.Clear();
                CalcularTotal();

                RefrescarMiembrosPausados(seleccionarPausaId: pausaId);
                MostrarDetallePausa(pausaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al pausar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefrescarMiembrosPausados(int? seleccionarPausaId = null)
        {
            if (cmbMiembroPausados == null)
                return;

            _suppressPausaUi = true;
            try
            {
                DataTable dt = ventaPausadaBLL.ObtenerPausadasActivas();
                cmbMiembroPausados.DisplayMember = "ClienteNombre";
                cmbMiembroPausados.ValueMember = "Id";
                cmbMiembroPausados.DataSource = dt;

                if (seleccionarPausaId.HasValue && dt.Rows.Count > 0)
                {
                    cmbMiembroPausados.SelectedValue = seleccionarPausaId.Value;
                }
                else
                {
                    cmbMiembroPausados.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Pausas] {ex.Message}");
                cmbMiembroPausados.DataSource = null;
            }
            finally
            {
                _suppressPausaUi = false;
            }
        }

        private void MostrarDetallePausa(int ventaPausadaId)
        {
            try
            {
                var cabecera = ventaPausadaBLL.ObtenerCabeceraActiva(ventaPausadaId);
                if (cabecera == null)
                {
                    LimpiarVistaPausa();
                    RefrescarMiembrosPausados();
                    return;
                }

                _pausaVistaId = ventaPausadaId;
                lblNombrePausaVenta.Text = cabecera["ClienteNombre"]?.ToString() ?? "(sin nombre)";
                decimal total = Convert.ToDecimal(cabecera["Total"]);
                lblTotalPausaVenta.Text = $"${total:N2}";

                DataTable detalle = ventaPausadaBLL.ObtenerDetalle(ventaPausadaId);
                dgvPausaVentas.DataSource = detalle;

                if (dgvPausaVentas.Columns.Contains("ProductoId"))
                    dgvPausaVentas.Columns["ProductoId"].Visible = false;

                _suppressPausaUi = true;
                try
                {
                    if (cmbMiembroPausados.DataSource != null)
                        cmbMiembroPausados.SelectedValue = ventaPausadaId;
                }
                catch { /* ignore */ }
                finally
                {
                    _suppressPausaUi = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarVistaPausa()
        {
            _pausaVistaId = null;
            if (lblNombrePausaVenta != null)
                lblNombrePausaVenta.Text = "(sin selección)";
            if (lblTotalPausaVenta != null)
                lblTotalPausaVenta.Text = "$0.00";
            if (dgvPausaVentas != null)
                dgvPausaVentas.DataSource = null;
        }

        private void RestaurarCarritoDesdeDetalle(DataTable detalle)
        {
            carrito.Clear();
            foreach (DataRow row in detalle.Rows)
            {
                carrito.Rows.Add(
                    Convert.ToInt32(row["ProductoId"]),
                    row["Producto"]?.ToString() ?? "Producto",
                    Convert.ToDecimal(row["Precio"]),
                    Convert.ToInt32(row["Cantidad"]),
                    Convert.ToDecimal(row["Total"]));
            }

            CalcularTotal();
        }

        private bool TryObtenerClientePausaAsignar(out int clienteId, out string nombre)
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

            if (cmbClientePausarVenta?.SelectedValue != null
                && cmbClientePausarVenta.SelectedValue != DBNull.Value
                && int.TryParse(cmbClientePausarVenta.SelectedValue.ToString(), out int id)
                && id > 0)
            {
                clienteId = id;
                nombre = cmbClientePausarVenta.Text.Trim();
                return true;
            }

            return false;
        }

        private bool TryObtenerPausaSeleccionada(out int pausaId, out string nombre)
        {
            pausaId = 0;
            nombre = string.Empty;

            if (cmbMiembroPausados?.SelectedItem is DataRowView row)
            {
                if (row["Id"] == null || row["Id"] == DBNull.Value)
                    return false;

                pausaId = Convert.ToInt32(row["Id"]);
                nombre = row["ClienteNombre"]?.ToString()?.Trim() ?? string.Empty;
                return pausaId > 0;
            }

            if (cmbMiembroPausados?.SelectedValue != null
                && cmbMiembroPausados.SelectedValue != DBNull.Value
                && int.TryParse(cmbMiembroPausados.SelectedValue.ToString(), out int id)
                && id > 0)
            {
                pausaId = id;
                nombre = cmbMiembroPausados.Text.Trim();
                return true;
            }

            return false;
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

                FinalizarPosTrasVenta();
                ProgramarRefrescoDashboard();
                MessageBox.Show("Venta realizada.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// Tras cobrar: limpia carrito, recarga stock en lista y deja listo el escáner.
        /// </summary>
        private void FinalizarPosTrasVenta()
        {
            carrito.Clear();
            CalcularTotal();
            CargarProductos();
            LimpiarBusquedaPos();
        }

        private void ProgramarRefrescoDashboard()
        {
            if (IsDisposed)
                return;

            BeginInvoke(new Action(() =>
            {
                try
                {
                    _presentacion?.CargarDashboard();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Dashboard post-venta POS] {ex.Message}");
                }
            }));
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

                bool esParcial = EsPlanParcialSeleccionado();
                if (esParcial && chkFinanciamiento.Checked)
                    chkFinanciamiento.Checked = false;

                // ATLETA/VISITA: no requieren miembro en combo; cantidad para varios visitantes.
                cmbCliente.Enabled = !esParcial;
                if (esParcial)
                    cmbCliente.SelectedIndex = -1;

                ActualizarUiPlanParcialCantidad();

                if (chkFinanciamiento.Checked)
                    CalcularSaldoFinanciamiento();

                ActualizarPanelOfertaPorPlan();
            }
            else
            {
                cmbCliente.Enabled = true;
                ActualizarUiPlanParcialCantidad();
                ActualizarPanelOfertaPorPlan();
            }
        }

        private void btnPagarMembresia_Click(object sender, EventArgs e)
        {
            if (!btnPagar.Enabled)
                return;

            try
            {
                if (cmbMembresia.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un plan (membresía).");
                    return;
                }

                bool esParcial = EsPlanParcialSeleccionado();
                int clienteId = 0;
                if (!esParcial)
                {
                    if (!TryObtenerClienteSeleccionado(out clienteId, out _))
                    {
                        MessageBox.Show("Faltan datos de cliente o membresía.");
                        return;
                    }
                }

                if (!VerificarCajaAbierta()) return;

                if (!esParcial)
                {
                    if (!ConfirmarPerfilCliente(clienteId)) return;

                    if (AvisoDeudaPendiente.BloqueaOperacionDePlan(this, clienteId, deudaBLL))
                        return;
                }

                // Con financiamiento, oferta o acceso parcial: no forzar diálogo de renovación.
                if (!chkFinanciamiento.Checked
                    && !EsPlanOfertaSeleccionado()
                    && !esParcial
                    && IntentarRedirigirRenovacion(clienteId))
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
                    if (esParcial)
                    {
                        CobrarPlanParcial(clienteId, planId, plan, usuario);
                    }
                    else if (EsPlanOfertaSeleccionado())
                    {
                        CobrarMembresiaConOferta(clienteId, planId, plan, fin, usuario);
                    }
                    else if (chkFinanciamiento.Checked)
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
            decimal pagoInicial = decimal.TryParse(txtPagoInicial.Text, out decimal p) ? p : 0;

            EjecutarVentaFinanciada(
                clienteId,
                planId,
                plan,
                fin,
                usuario,
                pagoInicial,
                dtpFechaVencimiento.Value.Date);
        }

        /// <summary>
        /// Cobro a crédito de un plan: el pago inicial entra a caja y la diferencia
        /// queda como deuda activa dentro de la misma transacción del BLL.
        /// </summary>
        private void EjecutarVentaFinanciada(
            int clienteId,
            int planId,
            PlanDTO plan,
            DateTime fin,
            string usuario,
            decimal pagoInicial,
            DateTime fechaLimiteDeuda)
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

            if (pagoInicial < 0 || pagoInicial > plan.Precio)
            {
                MessageBox.Show("Pago inicial inválido.");
                return;
            }

            decimal saldo = plan.Precio - pagoInicial;
            string conceptoPago = $"Pago inicial - Membresía {cmbMembresia.Text}";
            string metodoPago = "Efectivo";

            DateTime? fechaVencimientoDeuda = saldo > 0
                ? fechaLimiteDeuda
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

            // Deudas, dashboard e historial escuchan este evento para refrescar el saldo nuevo.
            if (saldo > 0)
                CORE.AppEventos.DeudaModificada();

            if (pagoInicial > 0 && result.Payload is MembresiaOperacionResult opFin)
            {
                string? nota = saldo > 0
                    ? $"Tu membresía está activa. Saldo pendiente: RD${saldo:N2}. Vence el {fin:dd/MM/yyyy}."
                    : null;
                // WhatsApp financiamiento lo dispara MembresiaBLL; aquí PDF con precio lista + abono.
                IniciarPostPagoEnSegundoPlano(
                    clienteId,
                    planId,
                    plan.Nombre ?? cmbMembresia.Text,
                    pagoInicial,
                    fin,
                    metodoPago,
                    opFin,
                    notaExtra: nota,
                    enviarWhatsAppFactura: false,
                    precioLista: plan.Precio);
            }
        }

        private void CobrarPlanParcial(
            int clienteIdOpcional,
            int planId,
            PlanDTO plan,
            string usuario)
        {
            if (!TryObtenerCantidadPlanParcial(out int cantidad, out string? errorCantidad))
            {
                MessageBox.Show(errorCantidad ?? "Cantidad inválida.");
                txtCantidad.Focus();
                return;
            }

            decimal montoEsperado = Math.Round(
                plan.Precio * cantidad,
                2,
                MidpointRounding.AwayFromZero);

            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Monto inválido.");
                return;
            }

            if (Math.Abs(monto - montoEsperado) > 0.009m)
            {
                MessageBox.Show(
                    cantidad > 1
                        ? $"ATLETA y VISITA: {cantidad} × RD$ {plan.Precio:N2} = RD$ {montoEsperado:N2}."
                        : $"ATLETA y VISITA se cobran al precio fijo del plan (RD$ {plan.Precio:N2}).",
                    "Acceso parcial",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                txtMonto.Text = montoEsperado.ToString("0.00");
                return;
            }

            string concepto = cantidad > 1
                ? $"Plan {plan.Nombre} x{cantidad}"
                : $"Plan {plan.Nombre}";
            string metodoPago = "Efectivo";

            // clienteId 0 → BLL usa VISITANTE (SISTEMA); no exige cmbCliente.
            var result = MembresiaCommandService.RegistrarPlanParcial(
                clienteIdOpcional,
                planId,
                plan.Precio,
                metodoPago,
                concepto,
                usuario,
                cantidad);

            if (!result.Success)
            {
                MessageBox.Show(result.Message);
                return;
            }

            LimpiarCampos();
            MessageBox.Show(
                cantidad > 1
                    ? $"{cantidad} × {plan.Nombre} registrados.\n\n" +
                      $"Total: RD$ {montoEsperado:N2}\n" +
                      "Sin miembro permanente · no activa Estado Clientes."
                    : $"{plan.Nombre} registrado.\n\n" +
                      $"Monto: RD$ {montoEsperado:N2}\n" +
                      "Sin miembro permanente · no activa Estado Clientes.",
                "Acceso parcial",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ProgramarRefrescoTrasPago();
            CORE.AppEventos.CajaCambiada();
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

            // Cobro parcial del plan: la diferencia no puede quedar sin registrar,
            // se convierte en deuda activa por la vía financiada.
            if (monto < plan.Precio)
            {
                CobrarMembresiaConSaldoPendiente(clienteId, planId, plan, fin, usuario, monto);
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
                    opPago,
                    precioLista: plan.Precio);
            }
        }

        /// <summary>
        /// El cajero cobró menos que el precio del plan: se confirma y se procesa como
        /// financiamiento para que el resto quede como deuda activa del cliente.
        /// </summary>
        private void CobrarMembresiaConSaldoPendiente(
            int clienteId,
            int planId,
            PlanDTO plan,
            DateTime fin,
            string usuario,
            decimal pagoInicial)
        {
            decimal saldo = plan.Precio - pagoInicial;
            DateTime fechaLimite = dtpFechaVencimiento.Value.Date < DateTime.Today
                ? DateTime.Today.AddDays(30)
                : dtpFechaVencimiento.Value.Date;

            var respuesta = MessageBox.Show(
                $"El monto cobrado (RD$ {pagoInicial:N2}) es menor al precio del plan " +
                $"{plan.Nombre} (RD$ {plan.Precio:N2}).\n\n" +
                $"Se registrará como pago inicial y RD$ {saldo:N2} quedará como deuda " +
                $"activa con fecha límite {fechaLimite:dd/MM/yyyy}.\n\n" +
                "¿Continuar?",
                "Pago parcial del plan",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            EjecutarVentaFinanciada(
                clienteId,
                planId,
                plan,
                fin,
                usuario,
                pagoInicial,
                fechaLimite);
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
        /// PDF primero (lista/descuento/asunto), luego WhatsApp factura. Sin popups en PC.
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
            bool enviarWhatsAppFactura = true,
            decimal? precioLista = null,
            decimal? descuentoMonto = null,
            decimal? descuentoPorcentaje = null,
            string? asuntoOferta = null,
            bool enviarWhatsAppOferta = false)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
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
                        abrirPdf: false,
                        precioLista: precioLista,
                        descuentoMonto: descuentoMonto,
                        descuentoPorcentaje: descuentoPorcentaje,
                        asuntoOferta: asuntoOferta,
                        forzarRegenerar: descuentoMonto.GetValueOrDefault() > 0
                            || !string.IsNullOrWhiteSpace(asuntoOferta));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PDF post-pago] {ex.Message}");
                }

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

                if (enviarWhatsAppOferta
                    && descuentoMonto.GetValueOrDefault() > 0
                    && !string.IsNullOrWhiteSpace(asuntoOferta))
                {
                    try
                    {
                        new MensajeAutomaticoBLL().EnviarMensajeOfertaMembresia(
                            clienteId,
                            nombrePlan,
                            precioLista ?? monto,
                            descuentoPorcentaje ?? 0,
                            descuentoMonto ?? 0,
                            monto,
                            asuntoOferta!);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("[WhatsApp oferta] " + ex.Message);
                    }
                }
            });
        }

        private void LimpiarCampos()
        {
            cmbCliente.Enabled = true;
            cmbCliente.SelectedIndex = -1;
            lstProductosPos.ClearSelected();
            if (txtBuscarProducto != null)
                txtBuscarProducto.Clear();
            cmbMembresia.SelectedIndex = -1;
            txtMonto.Clear();
            txtCantidad.Text = "1";
            txtCantidad.Enabled = false;
            lblCantidad.Enabled = false;
            chkFinanciamiento.Checked = false;
            txtPagoInicial.Text = "0";
            lblSaldoValor.Text = "$0.00";
            dtpFechaVencimiento.Value = DateTime.Today.AddDays(30);
            dtpFechaVencimiento.Enabled = false;
            pnlFinanciamiento.Visible = false;
            ResetOfertaCampos();
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

                if (EsPlanOfertaSeleccionado())
                {
                    MessageBox.Show(
                        "El plan OFERTA no admite financiamiento. Elija otro plan o quite el financiamiento.",
                        "Financiamiento",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    chkFinanciamiento.Checked = false;
                    return;
                }

                if (EsPlanParcialSeleccionado())
                {
                    MessageBox.Show(
                        "ATLETA y VISITA no admiten financiamiento. Cobre el monto completo.",
                        "Financiamiento",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
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