using BLL;
using BLL.Models;
using CORE;
using System;
using System.Data;
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

        private ClienteBLL? _clienteBLL;
        private PlanBLL? _planBLL;
        private MembresiaBLL? _membresiaBLL;
        private MensajeAutomaticoBLL? _mensajeBLL;

        private ClienteBLL Clientes => _clienteBLL ??= new();
        private PlanBLL Planes => _planBLL ??= new();
        private MembresiaBLL Membresias => _membresiaBLL ??= new();
        private MensajeAutomaticoBLL Mensajes => _mensajeBLL ??= new();

        public bool CambioRealizado { get; private set; }

        public FrmAñadirMiembro()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        private void FrmAñadirMiembro_Load(object? sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;

            try
            {
                InicializarFechaIngreso();
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

        /// <summary>Fecha de ingreso libre (histórica o futura).</summary>
        private void InicializarFechaIngreso()
        {
            DateTime hoy = DateTime.Today;
            if (hoy < dtFechaInicio.MinDate)
                hoy = dtFechaInicio.MinDate;
            if (hoy > dtFechaInicio.MaxDate)
                hoy = dtFechaInicio.MaxDate;
            dtFechaInicio.Value = hoy;
        }

        private void dtFechaInicio_ValueChanged(object? sender, EventArgs e)
        {
            ActualizarVistaVencimiento();
        }

        private void CargarPlanes()
        {
            DataTable planes = Planes.ObtenerPlanes() ?? new DataTable();
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
            DataTable clientes = Clientes.ObtenerClientesNoActivos() ?? new DataTable();
            cmbMiembro.DisplayMember = "Nombre";
            cmbMiembro.ValueMember = "Id";
            cmbMiembro.DataSource = clientes;
            cmbMiembro.SelectedIndex = clientes.Rows.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// Calcula vencimiento desde la fecha de ingreso: 7–19 → 15 del mes siguiente;
        /// 1–6 y 20 en adelante → último día del mes siguiente.
        /// </summary>
        private void ActualizarVistaVencimiento()
        {
            DateTime inicio = dtFechaInicio.Value.Date;
            DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);

            dtFechaVence.MinDate = new DateTime(2000, 1, 1);
            dtFechaVence.MaxDate = new DateTime(2100, 12, 31);
            dtFechaVence.Value = fin;
            dtFechaVence.MinDate = fin;
            dtFechaVence.MaxDate = fin;

            int dia = inicio.Day;
            string regla = dia >= 7 && dia <= 19
                ? "día 15 del mes siguiente"
                : "último día del mes siguiente";
            lblNotaVence.Text =
                $"Ingreso {inicio:dd/MM/yyyy} → vence {fin:dd/MM/yyyy} ({regla}). Sin movimiento en caja.";
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
                DateTime vence = MembresiaHelper.CalcularFechaVencimiento(inicio);

                var confirm = MessageBox.Show(this,
                    $"¿Integrar a {nombreCliente} con plan {nombrePlan}?\n\n" +
                    $"Ingreso: {inicio:dd/MM/yyyy}\n" +
                    $"Vence: {vence:dd/MM/yyyy}\n\n" +
                    "No se registrará ingreso en caja.",
                    "Confirmar integración",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                tbnGuardar.Enabled = false;
                MembresiaOperacionResult result = Membresias.RegistrarMiembroYaPagado(
                    clienteId, planId, inicio, Sesion.Usuario, vence);

                CambioRealizado = true;

                int membresiaId = result.MembresiaId;
                DateTime fechaFin = result.FechaFinMembresia;
                string planCapture = nombrePlan;
                int clienteCapture = clienteId;

                Task.Run(() =>
                {
                    try
                    {
                        Mensajes.EnviarMensajeAltaMiembroExistente(
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
