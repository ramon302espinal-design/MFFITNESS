using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using BLL.Models;
using CORE;
using DTO;
using UI.Facturas;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmRenovarMembresia : Form
    {
        private readonly int _clienteId;
        private readonly string _nombreCliente;
        private PlanBLL? _planBLL;
        private CajaBLL? _cajaBLL;

        private PlanBLL Planes => _planBLL ??= new PlanBLL();
        private CajaBLL Caja => _cajaBLL ??= new CajaBLL();

        public bool RenovacionCompletada { get; private set; }

        /// <summary>Constructor para el diseñador WinForms.</summary>
        public FrmRenovarMembresia()
        {
            InitializeComponent();
            _nombreCliente = string.Empty;
        }

        public FrmRenovarMembresia(int clienteId, string nombreCliente) : this()
        {
            _clienteId = clienteId;
            _nombreCliente = nombreCliente ?? string.Empty;
        }

        private void FrmRenovarMembresia_Load(object? sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;

            Text = "Renovar - " + (string.IsNullOrWhiteSpace(_nombreCliente) ? "Cliente" : _nombreCliente);
            lblCliente.Text = string.IsNullOrWhiteSpace(_nombreCliente) ? "Cliente" : _nombreCliente;
            ResetOfertaCampos();
            CargarPlanes();
        }

        private void CargarPlanes()
        {
            DataTable tablaPlanes = Planes.ObtenerPlanes() ?? new DataTable();
            DataTable fuente = FiltrarPlanesRenovacion(tablaPlanes);

            cmbPlan.DisplayMember = "Nombre";
            cmbPlan.ValueMember = "Id";
            cmbPlan.DataSource = fuente.Rows.Count > 0 ? fuente : null;
            cmbPlan.SelectedIndex = -1;
            lblPrecio.Text = "Precio: RD$ 0.00";
        }

        private void cmbPlan_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!EsPlanOfertaSeleccionado())
            {
                if (cmbPlan.SelectedItem is DataRowView row && row["Precio"] != DBNull.Value)
                {
                    decimal precio = Convert.ToDecimal(row["Precio"], CultureInfo.InvariantCulture);
                    lblPrecio.Text = "Precio: RD$ " + precio.ToString("0.00", CultureInfo.CurrentCulture);
                }
                else
                {
                    lblPrecio.Text = "Precio: RD$ 0.00";
                }
            }

            ActualizarPanelOfertaPorPlan();
        }

        private void btnConfirmar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbPlan.SelectedValue == null || cmbPlan.SelectedIndex < 0)
                {
                    MessageBox.Show(this, "Seleccione un plan.", "Renovación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int planId = Convert.ToInt32(cmbPlan.SelectedValue);
                var plan = Planes.ObtenerPlan(planId);
                if (plan == null)
                {
                    MessageBox.Show(this, "El plan no existe o no se pudo cargar.", "Renovación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string usuario = Sesion.Usuario ?? "ADMIN";

                if (EsPlanOfertaSeleccionado())
                {
                    ConfirmarRenovacionConOferta(planId, plan, usuario);
                    return;
                }

                if (!AsegurarCajaAbierta(usuario))
                    return;

                btnConfirmar.Enabled = false;
                btnCancelar.Enabled = false;
                Cursor = Cursors.WaitCursor;

                var result = MembresiaCommandService.RenovarMembresia(
                    _clienteId,
                    planId,
                    plan.Precio,
                    usuario);

                if (!result.Success)
                {
                    MessageBox.Show(this, result.Message, "Renovación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (result.Payload is RenovacionOperacionResult opRen)
                    IniciarPostRenovacionEnSegundoPlano(opRen, plan, planId, plan.Precio);

                RenovacionCompletada = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message, "Renovación",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    Cursor = Cursors.Default;
                    btnConfirmar.Enabled = true;
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

        private void IniciarPostRenovacionEnSegundoPlano(
            RenovacionOperacionResult opRen,
            PlanDTO plan,
            int planId,
            decimal montoCobrado,
            decimal? valorReferencia = null,
            decimal? descuento = null,
            decimal? porcentaje = null,
            string? asuntoOferta = null)
        {
            int pagoIdBg = opRen.PagoId;
            int membresiaIdBg = opRen.MembresiaId;
            int cajaMovIdBg = opRen.CajaMovimientoId;
            DateTime finBg = opRen.FechaFinMembresia == default
                ? MembresiaHelper.CalcularFechaVencimiento(DateTime.Now)
                : opRen.FechaFinMembresia;
            DateTime pagoBg = DateTime.Now;
            string planNombreBg = plan.Nombre ?? "PLAN";
            decimal precioBg = montoCobrado;
            decimal listaBg = valorReferencia ?? montoCobrado;
            decimal descBg = descuento ?? 0;
            decimal pctBg = porcentaje ?? 0;
            string? asuntoBg = asuntoOferta;
            int clienteBg = _clienteId;
            const string metodoBg = "Efectivo";
            bool esOferta = PlanNombres.EsOferta(plan.Nombre);

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    string? nota = esOferta && !string.IsNullOrWhiteSpace(asuntoBg)
                        ? $"Asunto: {asuntoBg}. Cortesía {pctBg:0.##}% (-RD${descBg:N2}). Valor ref. RD${listaBg:N2} → total RD${precioBg:N2}."
                        : null;

                    FacturaMembresiaPdfService.GenerarDesdeOperacion(
                        owner: null,
                        clienteBg,
                        planNombreBg,
                        precioBg,
                        finBg,
                        metodoBg,
                        new MembresiaOperacionResult
                        {
                            MembresiaId = membresiaIdBg,
                            PagoId = pagoIdBg,
                            CajaMovimientoId = cajaMovIdBg,
                            FechaFinMembresia = finBg
                        },
                        notaExtra: nota,
                        abrirPdf: false,
                        precioLista: esOferta ? listaBg : precioBg,
                        descuentoMonto: esOferta && descBg > 0 ? descBg : null,
                        descuentoPorcentaje: esOferta ? pctBg : null,
                        asuntoOferta: asuntoBg,
                        forzarRegenerar: esOferta);

                    if (pagoIdBg > 0 && precioBg > 0)
                    {
                        string? wa = new MembresiaBLL().EnviarWhatsAppTrasPagoMembresia(
                            clienteBg,
                            planId,
                            precioBg,
                            pagoBg,
                            finBg,
                            metodoBg,
                            pagoIdBg);

                        System.Diagnostics.Debug.WriteLine(
                            $"[WhatsApp renovación] {wa ?? "(sin detalle)"}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PDF/WhatsApp renovación] {ex.Message}");
                }
            });
        }

        private static DataTable FiltrarPlanesRenovacion(DataTable planes)
        {
            if (planes.Rows.Count == 0 || !planes.Columns.Contains("Nombre"))
                return planes;

            DataView dv = planes.DefaultView;
            try
            {
                dv.RowFilter =
                    "Nombre <> 'PRODUCTO A CRÉDITO' AND Nombre <> 'M-A' " +
                    "AND Nombre <> 'ATLETA' AND Nombre <> 'VISITA'";
                DataTable filtrada = dv.ToTable();
                return filtrada.Rows.Count > 0 ? filtrada : planes.Copy();
            }
            catch
            {
                return planes.Copy();
            }
            finally
            {
                dv.RowFilter = string.Empty;
            }
        }
    }
}
