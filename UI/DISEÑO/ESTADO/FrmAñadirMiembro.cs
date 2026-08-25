using BLL;
using BLL.Models;
using CORE;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.Theme;

namespace UI.DISEÑO.ESTADO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAñadirMiembro : Form
    {
        private static readonly string[] PlanesPermitidos =
        {
            "PREMIUM", "PRO", "MENSUALIDAD", "GLUTEOS GRANDE", "ABDOMEN PLANO", "M-A", "3x"
        };

        private readonly ClienteBLL _clienteBLL = new();
        private readonly PlanBLL _planBLL = new();
        private readonly MembresiaBLL _membresiaBLL = new();
        private readonly MensajeAutomaticoBLL _mensajeBLL = new();
        private readonly Label _lblVence = new();

        public bool CambioRealizado { get; private set; }

        public FrmAñadirMiembro()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            Text = "Añadir miembro (ya pagado)";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AcceptButton = tbnGuardar;

            cbmTipoPlanAñadir.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMiembro.DropDownStyle = ComboBoxStyle.DropDownList;
            dtFechaInicio.Format = DateTimePickerFormat.Short;
            dtFechaInicio.Value = DateTime.Today;

            _lblVence.AutoSize = false;
            _lblVence.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblVence.ForeColor = Color.FromArgb(22, 163, 74);
            _lblVence.Location = new Point(172, 200);
            _lblVence.Size = new Size(360, 24);
            Controls.Add(_lblVence);

            tbnGuardar.Click += tbnGuardar_Click;
            dtFechaInicio.ValueChanged += (_, _) => ActualizarVistaVencimiento();
            Load += FrmAñadirMiembro_Load;
        }

        private void FrmAñadirMiembro_Load(object? sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;

            try
            {
                CargarPlanes();
                CargarMiembros();
                ActualizarVistaVencimiento();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error al cargar datos: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarPlanes()
        {
            DataTable planes = _planBLL.ObtenerPlanes() ?? new DataTable();
            var filtrados = planes.Clone();

            foreach (DataRow row in planes.Rows)
            {
                string nombre = Convert.ToString(row["Nombre"])?.Trim() ?? string.Empty;
                if (PlanesPermitidos.Any(p => string.Equals(p, nombre, StringComparison.OrdinalIgnoreCase)))
                    filtrados.ImportRow(row);
            }

            cbmTipoPlanAñadir.DisplayMember = "Nombre";
            cbmTipoPlanAñadir.ValueMember = "Id";
            cbmTipoPlanAñadir.DataSource = filtrados;
            cbmTipoPlanAñadir.SelectedIndex = filtrados.Rows.Count > 0 ? 0 : -1;
        }

        private void CargarMiembros()
        {
            DataTable clientes = _clienteBLL.ObtenerClientesNoActivos() ?? new DataTable();
            cmbMiembro.DisplayMember = "Nombre";
            cmbMiembro.ValueMember = "Id";
            cmbMiembro.DataSource = clientes;
            cmbMiembro.SelectedIndex = clientes.Rows.Count > 0 ? 0 : -1;
        }

        private void ActualizarVistaVencimiento()
        {
            DateTime fin = MembresiaHelper.CalcularFechaVencimiento(dtFechaInicio.Value.Date);
            _lblVence.Text = "Vence (próximo pago): " + fin.ToString("dd/MM/yyyy");
        }

        private void tbnGuardar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbMiembro.SelectedValue == null || cmbMiembro.SelectedIndex < 0)
                {
                    MessageBox.Show(this, "Seleccione un miembro.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (cbmTipoPlanAñadir.SelectedValue == null || cbmTipoPlanAñadir.SelectedIndex < 0)
                {
                    MessageBox.Show(this, "Seleccione un plan.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int clienteId = Convert.ToInt32(cmbMiembro.SelectedValue);
                int planId = Convert.ToInt32(cbmTipoPlanAñadir.SelectedValue);
                string nombrePlan = cbmTipoPlanAñadir.Text?.Trim() ?? "membresía";
                string nombreCliente = cmbMiembro.Text?.Trim() ?? "Miembro";
                DateTime inicio = dtFechaInicio.Value.Date;

                var confirm = MessageBox.Show(this,
                    $"¿Integrar a {nombreCliente} con plan {nombrePlan}?\n\n" +
                    $"Ingreso: {inicio:dd/MM/yyyy}\n" +
                    $"Vence: {MembresiaHelper.CalcularFechaVencimiento(inicio):dd/MM/yyyy}\n\n" +
                    "No se registrará ingreso en caja.",
                    "Confirmar integración",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                tbnGuardar.Enabled = false;
                MembresiaOperacionResult result = _membresiaBLL.RegistrarMiembroYaPagado(
                    clienteId, planId, inicio, Sesion.Usuario);

                CambioRealizado = true;

                int membresiaId = result.MembresiaId;
                DateTime fechaFin = result.FechaFinMembresia;
                string planCapture = nombrePlan;
                int clienteCapture = clienteId;

                Task.Run(() =>
                {
                    try
                    {
                        _mensajeBLL.EnviarMensajeAltaMiembroExistente(
                            clienteCapture, planCapture, fechaFin, membresiaId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "WhatsApp ALTA_MIEMBRO_EXISTENTE: " + ex.Message);
                    }
                });

                MessageBox.Show(this,
                    $"Miembro integrado.\nPlan: {nombrePlan}\nVence: {fechaFin:dd/MM/yyyy}\n\n" +
                    "Historial actualizado. Sin movimiento en caja.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                tbnGuardar.Enabled = true;
                MessageBox.Show(this, "No se pudo integrar el miembro: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
