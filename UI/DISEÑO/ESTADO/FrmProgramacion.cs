using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using BLL.Models;
using CORE;
using DTO;
using UI.Facturas;
using UI.Theme;

namespace UI.DISEÑO.ESTADO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmProgramacion : Form
    {
        private readonly IReadOnlyList<ProgramacionMiembroItemDTO> _miembrosActivos;
        private readonly int _clientePreseleccionado;
        private PlanBLL? _planBLL;
        private CajaBLL? _cajaBLL;

        private PlanBLL Planes => _planBLL ??= new PlanBLL();
        private CajaBLL Caja => _cajaBLL ??= new CajaBLL();

        public bool ProgramacionCompletada { get; private set; }

        public FrmProgramacion()
        {
            InitializeComponent();
            _miembrosActivos = Array.Empty<ProgramacionMiembroItemDTO>();
            _clientePreseleccionado = 0;
        }

        public FrmProgramacion(IReadOnlyList<ProgramacionMiembroItemDTO> miembrosActivos, int clientePreseleccionado = 0) : this()
        {
            _miembrosActivos = miembrosActivos ?? Array.Empty<ProgramacionMiembroItemDTO>();
            _clientePreseleccionado = clientePreseleccionado;
        }

        private void FrmProgramacion_Load(object? sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;

            ThemeHost.Attach(this);
            CargarMiembros();
            CargarPlanes();
            LimpiarDetalleMiembro();
            PreseleccionarMiembro();
        }

        private void PreseleccionarMiembro()
        {
            if (_clientePreseleccionado <= 0 || cmbPMiembro.Items.Count == 0)
                return;

            for (int i = 0; i < cmbPMiembro.Items.Count; i++)
            {
                if (cmbPMiembro.Items[i] is ProgramacionMiembroItemDTO item
                    && item.ClienteId == _clientePreseleccionado)
                {
                    cmbPMiembro.SelectedIndex = i;
                    return;
                }
            }
        }

        private void CargarMiembros()
        {
            cmbPMiembro.DisplayMember = nameof(ProgramacionMiembroItemDTO.Nombre);
            cmbPMiembro.ValueMember = nameof(ProgramacionMiembroItemDTO.ClienteId);
            cmbPMiembro.DataSource = _miembrosActivos.Count > 0
                ? _miembrosActivos.ToList()
                : null;
            cmbPMiembro.SelectedIndex = -1;
        }

        private void CargarPlanes()
        {
            DataTable todos = Planes.ObtenerPlanes() ?? new DataTable();
            DataTable filtrados = FiltrarPlanesProgramacion(todos);

            cmbPMembresia.DisplayMember = "Nombre";
            cmbPMembresia.ValueMember = "Id";
            cmbPMembresia.DataSource = filtrados.Rows.Count > 0 ? filtrados : null;
            cmbPMembresia.SelectedIndex = -1;
            lblPmontoPlan.Text = "Precio: RD$ 0.00";
        }

        private static DataTable FiltrarPlanesProgramacion(DataTable planes)
        {
            if (planes.Rows.Count == 0 || !planes.Columns.Contains("Nombre"))
                return planes;

            var permitidos = new HashSet<string>(ProgramacionBLL.PlanesPermitidos, StringComparer.OrdinalIgnoreCase);
            var destino = planes.Clone();
            foreach (DataRow row in planes.Rows)
            {
                string nombre = Convert.ToString(row["Nombre"])?.Trim() ?? string.Empty;
                if (permitidos.Contains(nombre))
                    destino.ImportRow(row);
            }

            if (destino.Rows.Count == 0)
                return planes.Clone();

            return destino;
        }

        private void cmbPMiembro_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbPMiembro.SelectedItem is not ProgramacionMiembroItemDTO item)
            {
                LimpiarDetalleMiembro();
                return;
            }

            lblPNombre.Text = item.Nombre;
            lblPMembresia.Text = item.Membresia;
            lblPVence.Text = item.FechaVencimiento.ToString("dd/MM/yyyy");

            DateTime inicioSugerido = item.FechaVencimiento.Date;
            if (inicioSugerido < DateTime.Today)
                inicioSugerido = DateTime.Today;

            dtPProgramar.Value = inicioSugerido;
            RecalcularFechaFinProgramada();
        }

        private void LimpiarDetalleMiembro()
        {
            lblPNombre.Text = "—";
            lblPMembresia.Text = "—";
            lblPVence.Text = "—";
            dtPProgramado.Value = DateTime.Today;
        }

        private void cmbPMembresia_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbPMembresia.SelectedItem is DataRowView row && row["Precio"] != DBNull.Value)
            {
                decimal precio = Convert.ToDecimal(row["Precio"], CultureInfo.InvariantCulture);
                lblPmontoPlan.Text = "Precio: RD$ " + precio.ToString("N2", CultureInfo.CurrentCulture);
            }
            else
            {
                lblPmontoPlan.Text = "Precio: RD$ 0.00";
            }
        }

        private void dtPProgramar_ValueChanged(object? sender, EventArgs e)
        {
            RecalcularFechaFinProgramada();
        }

        private void RecalcularFechaFinProgramada()
        {
            DateTime fin = MembresiaHelper.CalcularFechaFinProgramacion(dtPProgramar.Value.Date);
            dtPProgramado.Value = fin;
        }

        private void btnPProgramar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbPMiembro.SelectedItem is not ProgramacionMiembroItemDTO miembro)
                {
                    MessageBox.Show(this, "Seleccione un miembro activo.", "Programación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbPMembresia.SelectedValue == null || cmbPMembresia.SelectedIndex < 0)
                {
                    MessageBox.Show(this, "Seleccione el plan a programar.", "Programación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int planId = Convert.ToInt32(cmbPMembresia.SelectedValue);
                var plan = Planes.ObtenerPlan(planId);
                if (plan == null)
                {
                    MessageBox.Show(this, "El plan no existe o no se pudo cargar.", "Programación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string usuario = Sesion.Usuario ?? "ADMIN";
                if (!AsegurarCajaAbierta(usuario))
                    return;

                btnPProgramar.Enabled = false;
                btnCancelar.Enabled = false;
                Cursor = Cursors.WaitCursor;

                DateTime inicio = dtPProgramar.Value.Date;
                DateTime fin = dtPProgramado.Value.Date;

                var result = MembresiaCommandService.ProgramarMembresia(
                    miembro.ClienteId,
                    planId,
                    plan.Precio,
                    inicio,
                    fin,
                    usuario);

                if (!result.Success)
                {
                    MessageBox.Show(this, result.Message, "Programación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (result.Payload is ProgramacionOperacionResult op)
                    IniciarPostProgramacionEnSegundoPlano(op, miembro.ClienteId, plan.Nombre ?? "PLAN");

                ProgramacionCompletada = true;
                AppEventos.PagoRegistrado();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message, "Programación",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    Cursor = Cursors.Default;
                    btnPProgramar.Enabled = true;
                    btnCancelar.Enabled = true;
                }
            }
        }

        private bool AsegurarCajaAbierta(string usuario)
        {
            if (Caja.ObtenerCajaAbiertaHoy() != null)
                return true;

            DialogResult r = MessageBox.Show(
                this,
                "No hay caja abierta. ¿Deseas abrir caja?",
                "Caja",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (r != DialogResult.Yes)
                return false;

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese monto inicial:",
                "Abrir Caja",
                "0");

            if (!decimal.TryParse(input, out decimal montoInicial) || montoInicial <= 0)
            {
                MessageBox.Show(this, "Monto inválido.", "Caja",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            Caja.AbrirCaja(montoInicial, usuario);
            AppEventos.CajaCambiada();
            return true;
        }

        private void IniciarPostProgramacionEnSegundoPlano(
            ProgramacionOperacionResult op,
            int clienteId,
            string planNombre)
        {
            int pagoIdBg = op.PagoId;
            int cajaMovIdBg = op.CajaMovimientoId;
            DateTime finBg = op.FechaFinProgramada;
            DateTime inicioBg = op.FechaInicioProgramada;
            decimal precioBg = op.Monto;
            int planIdBg = op.PlanId;
            string planNombreBg = planNombre;
            const string metodoBg = "Efectivo";

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    string nota = $"Programado para {inicioBg:dd/MM/yyyy}. Vence {finBg:dd/MM/yyyy}.";

                    FacturaMembresiaPdfService.GenerarDesdeOperacion(
                        owner: null,
                        clienteId,
                        planNombreBg,
                        precioBg,
                        finBg,
                        metodoBg,
                        new MembresiaOperacionResult
                        {
                            PagoId = pagoIdBg,
                            CajaMovimientoId = cajaMovIdBg,
                            FechaFinMembresia = finBg
                        },
                        notaExtra: nota,
                        abrirPdf: false);

                    if (pagoIdBg > 0 && precioBg > 0)
                    {
                        string? wa = new MembresiaBLL().EnviarWhatsAppTrasPagoMembresia(
                            clienteId,
                            planIdBg,
                            precioBg,
                            DateTime.Now,
                            finBg,
                            metodoBg,
                            pagoIdBg);

                        System.Diagnostics.Debug.WriteLine(
                            $"[WhatsApp programación] {wa ?? "(sin detalle)"}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PDF/WhatsApp programación] {ex.Message}");
                }
            });
        }
    }
}
