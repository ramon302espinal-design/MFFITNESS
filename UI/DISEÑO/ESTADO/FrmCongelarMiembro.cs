using BLL;
using CORE;
using DTO;
using System;
using System.Drawing;
using System.Windows.Forms;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCongelarMiembro : Form
    {
        private readonly int _clienteId;
        private readonly string _nombreCliente;
        private readonly MembresiaBLL _membresiaBLL = new MembresiaBLL();
        private readonly CongelacionDTO? _congelacion;

        public bool CambioRealizado { get; private set; }

        public FrmCongelarMiembro()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _nombreCliente = "";
        }

        public FrmCongelarMiembro(int clienteId, string nombreCliente) : this()
        {
            _clienteId = clienteId;
            _nombreCliente = nombreCliente ?? "";
            _congelacion = _membresiaBLL.ObtenerCongelacionActiva(clienteId);
            ConfigurarVista();
        }

        private void ConfigurarVista()
        {
            Text = "Congelar - " + (string.IsNullOrWhiteSpace(_nombreCliente) ? "Miembro" : _nombreCliente);
            lblCliente.Text = string.IsNullOrWhiteSpace(_nombreCliente) ? "Miembro" : _nombreCliente;

            if (_congelacion == null)
            {
                DateTime hoy = CongelacionHelper.HoyPc();
                int ancla = CongelacionHelper.CalcularDiaAncla(hoy);
                DateTime desde = CongelacionHelper.CalcularFechaReactivacionDesde(ancla, hoy);
                lblInfo.Text =
                    $"Hoy: {hoy:dd/MM/yyyy}. Podrá volver a entrar desde el día {ancla} ({desde:dd/MM/yyyy}).\n" +
                    "Si el plan es de 15, al reactivar vence el 15 de ese mismo mes.";
                txtMotivo.ReadOnly = false;
                txtMotivo.Text = "";
                btnConfirmar.Text = "CONGELAR";
                btnConfirmar.BackColor = Color.FromArgb(14, 165, 233);
            }
            else
            {
                int diaAncla = _congelacion.FechaCongelacion.Day > 0
                    ? _congelacion.FechaCongelacion.Day
                    : _congelacion.DiaAncla;
                DateTime desde = CongelacionHelper.CalcularFechaReactivacionDesde(diaAncla);
                lblInfo.Text =
                    $"Congelado el {_congelacion.FechaCongelacion:dd/MM/yyyy}. " +
                    $"Puede volver a entrar desde el día {diaAncla} ({desde:dd/MM/yyyy}).\n" +
                    "Si el plan es de 15 y ya pasó la fecha original, al activar vence el 15 de este mes.";
                txtMotivo.ReadOnly = true;
                txtMotivo.Text = _congelacion.Motivo;
                btnConfirmar.Text = "ACTIVAR";
                btnConfirmar.BackColor = Color.FromArgb(22, 163, 74);
            }
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                string usuario = Sesion.Usuario ?? "ADMIN";

                if (_congelacion == null)
                {
                    string motivo = txtMotivo.Text.Trim();
                    if (string.IsNullOrWhiteSpace(motivo))
                    {
                        MessageBox.Show(
                            "Indique el motivo de congelamiento.",
                            "Congelar",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        txtMotivo.Focus();
                        return;
                    }

                    CongelacionDTO cong = _membresiaBLL.CongelarMiembro(_clienteId, motivo, usuario);
                    DateTime desde = CongelacionHelper.CalcularFechaReactivacionDesde(cong.DiaAncla);

                    MessageBox.Show(
                        $"{_nombreCliente} quedó CONGELADO.\n" +
                        $"Puede volver a entrar desde el día {cong.DiaAncla} ({desde:dd/MM/yyyy}).\n" +
                        "Se intentará avisar por WhatsApp.",
                        "Congelar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    int clienteWa = _clienteId;
                    string motivoWa = cong.Motivo ?? motivo;
                    DateTime fechaCong = cong.FechaCongelacion;
                    int ancla = cong.DiaAncla;
                    DateTime fechaDesde = desde;
                    int diasRest = cong.DiasRestantes;
                    DateTime finOrig = cong.FechaFinOriginal ?? cong.FechaCongelacion;
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            new MensajeAutomaticoBLL().EnviarMensajeCongelacion(
                                clienteWa,
                                motivoWa,
                                fechaCong,
                                ancla,
                                fechaDesde,
                                diasRest,
                                finOrig);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("WhatsApp congelación: " + ex.Message);
                        }
                    });
                }
                else
                {
                    DateTime nuevaFin = _membresiaBLL.ActivarMiembroCongelado(_clienteId, usuario);
                    DateTime hoy = CongelacionHelper.HoyPc();

                    MessageBox.Show(
                        $"{_nombreCliente} fue activado.\nEl plan continúa. Nueva fecha de vencimiento: {nuevaFin:dd/MM/yyyy}.\n" +
                        "Se intentará avisar por WhatsApp.",
                        "Activar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    int clienteWa = _clienteId;
                    DateTime fechaAct = hoy;
                    DateTime fechaVence = nuevaFin;
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            new MensajeAutomaticoBLL().EnviarMensajeDescongelacion(
                                clienteWa, fechaAct, fechaVence);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("WhatsApp descongelación: " + ex.Message);
                        }
                    });
                }

                CambioRealizado = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Activación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Congelar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
