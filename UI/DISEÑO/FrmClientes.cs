using BLL;
using BLL.Commands;
using CORE;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using UI.DISEÑO.Controles;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmClientes : Form
    {
        private ClienteBLL? _service;
        private ClienteBLL service => _service ??= new ClienteBLL();
        private readonly BindingSource _bsClientes = new();
        private int idSeleccionado = 0;
        private readonly int? _clienteIdPreseleccionado;

        private FrmPresentacion _presentacion;

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmClientes()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _presentacion = null!;
        }

        public FrmClientes(FrmPresentacion presentacion)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _presentacion = presentacion;
            if (ThemeHost.IsDesignTime())
                return;

            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloClientes);
            btnBack.Visible = false;
            dgvClientes.DataSource = _bsClientes;
            ConfigurarGridClientes();
            ConfigurarFechasEdicion();
            InicializarFichaSaludUi();
            ucFichaMiembro.WireEventos();
            ConfigurarDetalleMiembrosSoloLectura();
            ActualizarBotonesMiembros();
        }

        private void ConfigurarDetalleMiembrosSoloLectura()
        {
            txtEditNombre.ReadOnly = true;
            txtEditTelefono.ReadOnly = true;
            txtEditDireccion.ReadOnly = true;
            txtEditId.ReadOnly = true;
            dtpEditFechaNac.Enabled = false;
            ucFichaMiembro.SetSoloLectura(true);
        }

        public FrmClientes(FrmPresentacion presentacion, int clienteIdPreseleccionado)
            : this(presentacion)
        {
            _clienteIdPreseleccionado = clienteIdPreseleccionado;
        }

        private void FrmClientes_Load(object sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime() || DesignMode)
                return;

            txtFecha.MaxDate = DateTime.Today;
            txtFecha.MinDate = new DateTime(1920, 1, 1);
            dtpFechaIngreso.Enabled = true;
            dtpFechaIngreso.Value = DateTime.Today;

            InicializarFichaSaludUi();
            ucFichaMiembro.WireEventos();
            LimpiarFormularioAlta();
            CargarClientes();
            LimpiarFormularioEdicion();

            if (_clienteIdPreseleccionado.HasValue)
            {
                tabControlClientes.SelectedTab = tabMiembros;
                SeleccionarClientePorId(_clienteIdPreseleccionado.Value);
            }
        }

        private void ConfigurarFechasEdicion()
        {
            dtpEditFechaNac.MaxDate = DateTime.Today;
            dtpEditFechaNac.MinDate = new DateTime(1920, 1, 1);
            dtpEditFechaNac.CustomFormat = "dd-MMMM-yyyy";
            dtpEditFechaNac.Format = DateTimePickerFormat.Custom;
        }

        private void tabControlClientes_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (tabControlClientes.SelectedTab == tabAgregar)
            {
                // Alta limpia: nunca arrastra datos de un miembro seleccionado.
                LimpiarFormularioAlta();
            }
            else if (tabControlClientes.SelectedTab == tabMiembros)
            {
                CargarClientes(idSeleccionado > 0 ? idSeleccionado : null);
            }
        }

        private void ConfigurarGridClientes()
        {
            dgvClientes.BorderStyle = BorderStyle.None;
            dgvClientes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvClientes.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvClientes.GridColor = Color.FromArgb(226, 232, 240);
            dgvClientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvClientes.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvClientes.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            dgvClientes.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgvClientes.BackgroundColor = Color.White;
            dgvClientes.EnableHeadersVisualStyles = false;
            dgvClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvClientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvClientes.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgvClientes.ColumnHeadersHeight = 36;
            dgvClientes.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvClientes.RowTemplate.Height = 32;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.MultiSelect = false;
            dgvClientes.ReadOnly = true;
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AllowUserToDeleteRows = false;
            dgvClientes.RowHeadersVisible = false;

            ThemeApplier.ApplyReadOnlyGridBehavior(dgvClientes);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        // ===============================
        // TAB AGREGAR — solo alta
        // ===============================
        private void LimpiarFormularioAlta()
        {
            txtNombre.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
            txtId.Clear();

            DateTime fechaNacDefault = DateTime.Today.AddYears(-18);
            if (fechaNacDefault < txtFecha.MinDate)
                fechaNacDefault = txtFecha.MinDate;
            if (fechaNacDefault > txtFecha.MaxDate)
                fechaNacDefault = txtFecha.MaxDate;
            txtFecha.Value = fechaNacDefault;
            dtpFechaIngreso.Value = DateTime.Today;
            ActualizarEdad();
            LimpiarFichaSaludUi();
        }

        private bool ValidarAlta(out string mensaje)
        {
            mensaje = "";
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                mensaje = "El nombre es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                mensaje = "El teléfono del cliente es obligatorio.";
                return false;
            }

            if (txtFecha.Value.Date > DateTime.Today)
            {
                mensaje = "La fecha de nacimiento no puede ser futura.";
                return false;
            }

            return true;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarAlta(out string validacion))
                {
                    MessageBox.Show(validacion);
                    return;
                }

                var ficha = ConstruirFichaDesdeUi();
                var result = ClienteCommandService.AgregarConFicha(
                    txtNombre.Text.Trim(),
                    txtFecha.Value.Date,
                    txtDireccion.Text.Trim(),
                    txtTelefono.Text.Trim(),
                    ficha
                );

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                int nuevoId = result.Payload is int id ? id : 0;
                LimpiarFormularioAlta();
                tabControlClientes.SelectedTab = tabMiembros;
                CargarClientes(nuevoId > 0 ? nuevoId : null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ===============================
        // TAB MIEMBROS — editar / eliminar
        // ===============================
        private void LimpiarFormularioEdicion()
        {
            idSeleccionado = 0;
            txtEditId.Clear();
            txtEditNombre.Clear();
            txtEditTelefono.Clear();
            txtEditDireccion.Clear();

            DateTime fechaNacDefault = DateTime.Today.AddYears(-18);
            if (fechaNacDefault < dtpEditFechaNac.MinDate)
                fechaNacDefault = dtpEditFechaNac.MinDate;
            if (fechaNacDefault > dtpEditFechaNac.MaxDate)
                fechaNacDefault = dtpEditFechaNac.MaxDate;
            dtpEditFechaNac.Value = fechaNacDefault;

            ucFichaMiembro.Limpiar();
            ActualizarBotonesMiembros();
        }

        private void ActualizarBotonesMiembros()
        {
            // Detalle en solo lectura: sin GUARDAR / ELIMINAR.
        }

        private void CargarClientes(int? resaltarId = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => CargarClientes(resaltarId)));
                return;
            }

            _bsClientes.DataSource = service.ObtenerClientes();
            AplicarFiltroBusqueda();

            if (resaltarId.HasValue && resaltarId.Value > 0)
            {
                SeleccionarClientePorId(resaltarId.Value);
                return;
            }

            dgvClientes.ClearSelection();
        }

        private void AplicarFiltroBusqueda()
        {
            var termino = txtBuscar.Text.Trim();
            _bsClientes.Filter = string.IsNullOrEmpty(termino)
                ? null
                : BusquedaGridHelper.ConstruirFiltroClientes(termino);
        }

        private static object? ObtenerValorId(DataGridViewRow fila)
        {
            if (fila.DataGridView?.Columns.Contains("Id") == true)
                return fila.Cells["Id"].Value;

            if (fila.DataGridView?.Columns.Contains("ID") == true)
                return fila.Cells["ID"].Value;

            return null;
        }

        private static bool TryObtenerIdCliente(DataGridViewRow fila, out int clienteId)
        {
            clienteId = 0;
            var valor = ObtenerValorId(fila);

            if (valor == null || valor == DBNull.Value)
                return false;

            return int.TryParse(Convert.ToString(valor, CultureInfo.InvariantCulture), out clienteId);
        }

        private void SeleccionarClientePorId(int clienteId)
        {
            foreach (DataGridViewRow fila in dgvClientes.Rows)
            {
                if (!TryObtenerIdCliente(fila, out int id) || id != clienteId)
                    continue;

                fila.Selected = true;
                if (fila.Cells.Count > 0 && fila.Cells[0].Visible)
                    dgvClientes.CurrentCell = fila.Cells[0];
                if (idSeleccionado != clienteId)
                    CargarFilaEnEdicion(fila);
                break;
            }
        }

        private void CargarFilaEnEdicion(DataGridViewRow fila)
        {
            if (!TryObtenerIdCliente(fila, out int id) || id <= 0)
                return;

            idSeleccionado = id;
            txtEditId.Text = id.ToString();
            txtEditNombre.Text = fila.Cells["Nombre"].Value?.ToString() ?? "";
            txtEditTelefono.Text = fila.Cells["Telefono"].Value?.ToString() ?? "";
            txtEditDireccion.Text = fila.Cells["Direccion"].Value?.ToString() ?? "";

            if (fila.Cells["FechaNacimiento"].Value != null
                && fila.Cells["FechaNacimiento"].Value != DBNull.Value)
            {
                DateTime fechaNac = Convert.ToDateTime(fila.Cells["FechaNacimiento"].Value).Date;
                if (fechaNac < dtpEditFechaNac.MinDate)
                    fechaNac = dtpEditFechaNac.MinDate;
                if (fechaNac > dtpEditFechaNac.MaxDate)
                    fechaNac = dtpEditFechaNac.MaxDate;
                dtpEditFechaNac.Value = fechaNac;
            }

            try
            {
                var ficha = service.ObtenerFichaSalud(id);
                ucFichaMiembro.Cargar(ficha, id);
                ucFichaMiembro.SetSoloLectura(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar la ficha de salud: " + ex.Message,
                    "Ficha de salud",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            ActualizarBotonesMiembros();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroBusqueda();
        }

        private void dgvClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            CargarFilaEnEdicion(dgvClientes.Rows[e.RowIndex]);
        }

        private void dgvClientes_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null || dgvClientes.CurrentRow.IsNewRow)
                return;

            if (!TryObtenerIdCliente(dgvClientes.CurrentRow, out int id) || id <= 0)
                return;

            if (id == idSeleccionado)
                return;

            CargarFilaEnEdicion(dgvClientes.CurrentRow);
        }

        private void dgvClientes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgvClientes.Rows[e.RowIndex];
            if (!TryObtenerIdCliente(fila, out int clienteId))
                return;

            string nombre = fila.Cells["Nombre"].Value?.ToString() ?? string.Empty;

            var frmHistorial = new FrmHistorialVentas(this, clienteId, nombre);
            frmHistorial.ShowDialog();
        }

        private void btnIrAPagar_Click(object sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null || idSeleccionado <= 0)
            {
                MessageBox.Show(
                    "Debe seleccionar un cliente.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            int id = Convert.ToInt32(ObtenerValorId(dgvClientes.CurrentRow));
            string nombre = dgvClientes.CurrentRow.Cells["Nombre"].Value?.ToString() ?? "";

            FrmPagos frm = new FrmPagos(_presentacion, id, nombre);
            frm.ShowDialog();
        }

        private void layoutNavClientes_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
