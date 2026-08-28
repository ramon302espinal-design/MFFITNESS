using System;
using System.Drawing;
using System.Windows.Forms;
using UI.Helpers;

namespace UI.DISEÑO
{
    /// <summary>
    /// Petición IA por texto o voz (micrófono manual, español Windows).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmPeticionIaFoto : Form
    {
        private PeticionIaVozHelper? _voz;
        private bool _textoEditadoManual;
        private int _ultimoNivelAudio;
        private readonly System.Windows.Forms.Timer _timerNivelVoz = new() { Interval = 80 };

        public string Peticion => txtPeticion.Text.Trim();

        public FrmPeticionIaFoto()
        {
            InitializeComponent();
            _timerNivelVoz.Tick += (_, _) => ActualizarBarraNivelVoz();
            txtPeticion.TextChanged += (_, _) => _textoEditadoManual = true;
            FormClosed += FrmPeticionIaFoto_FormClosed;
        }

        private void FrmPeticionIaFoto_Shown(object? sender, EventArgs e)
        {
            txtPeticion.Focus();
            if (!_textoEditadoManual)
                txtPeticion.SelectAll();

            // Solo preparar motor + mostrar micrófono; NO escuchar al abrir.
            PrepararVozSinEscuchar();
        }

        private void PrepararVozSinEscuchar()
        {
            _voz?.Dispose();
            _voz = new PeticionIaVozHelper();
            _voz.TextoReconocido += Voz_TextoReconocido;
            _voz.TextoParcial += Voz_TextoParcial;
            _voz.NivelAudio += nivel => _ultimoNivelAudio = nivel;
            _voz.EscuchandoCambiado += Voz_EscuchandoCambiado;
            _voz.Aviso += msg => BeginInvoke(() =>
            {
                lblEstadoVoz.ForeColor = Color.FromArgb(180, 83, 9);
                lblEstadoVoz.Text = msg.Replace('\n', ' ');
            });

            if (!_voz.TryPreparar(out string? error))
            {
                lblEstadoVoz.Text = (error ?? "Voz no disponible").Replace('\n', ' ');
                lblEstadoVoz.ForeColor = Color.FromArgb(180, 83, 9);
                btnMicrofono.Enabled = false;
                panelNivelVoz.Visible = false;
                return;
            }

            btnMicrofono.Enabled = true;
            panelNivelVoz.Visible = false;
            panelNivelVozFill.Width = 4;
            _ultimoNivelAudio = 0;
            ActualizarUiMicrofono(escuchando: false);

            string mic = _voz.NombreMicrofono ?? "Micrófono Windows";
            string motor = _voz.MotorDescripcion ?? _voz.CulturaReconocimiento ?? "Español";
            lblEstadoVoz.ForeColor = Color.FromArgb(100, 116, 139);
            lblEstadoVoz.Text = _voz.UsaWhisper
                ? $"Mic: {mic} · {motor} · 🎤 habla · ■ terminar"
                : $"Mic: {mic} · {motor} · pulsa 🎤 para dictar";
        }

        private void Voz_TextoReconocido(string texto)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => Voz_TextoReconocido(texto));
                return;
            }

            if (string.IsNullOrWhiteSpace(texto))
                return;

            const string defaultTxt = "Ponla nítida y mejora la calidad sin cambiar el producto";
            if (string.IsNullOrWhiteSpace(txtPeticion.Text) || txtPeticion.Text == defaultTxt)
            {
                txtPeticion.Text = texto;
            }
            else if (!txtPeticion.Text.Contains(texto, StringComparison.OrdinalIgnoreCase))
            {
                txtPeticion.Text = txtPeticion.Text.TrimEnd() + " " + texto;
            }

            txtPeticion.SelectionStart = txtPeticion.TextLength;
            txtPeticion.ScrollToCaret();
            lblEstadoVoz.ForeColor = Color.FromArgb(22, 101, 52);
            lblEstadoVoz.Text = "✓ Entendido: " + texto;
        }

        private void Voz_TextoParcial(string parcial)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => Voz_TextoParcial(parcial));
                return;
            }

            lblEstadoVoz.ForeColor = Color.FromArgb(37, 99, 235);
            lblEstadoVoz.Text = "Escuchando… " + parcial;
        }

        private void ActualizarBarraNivelVoz()
        {
            if (!panelNivelVoz.Visible)
                return;

            int w = Math.Clamp(_ultimoNivelAudio * panelNivelVoz.Width / 100, 0, panelNivelVoz.Width);
            panelNivelVozFill.Width = Math.Max(4, w);
        }

        private void Voz_EscuchandoCambiado(bool escuchando)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => Voz_EscuchandoCambiado(escuchando));
                return;
            }

            ActualizarUiMicrofono(escuchando);
            panelNivelVoz.Visible = escuchando;

            if (escuchando)
            {
                _timerNivelVoz.Start();
                _ultimoNivelAudio = 0;
                lblEstadoVoz.ForeColor = Color.FromArgb(220, 38, 38);
                lblEstadoVoz.Text = _voz?.UsaWhisper == true
                    ? "● GRABANDO — habla y pulsa ■ cuando termines"
                    : "● ESCUCHANDO — habla claro al micrófono";
            }
            else
            {
                _timerNivelVoz.Stop();
                panelNivelVozFill.Width = 4;
                if (!lblEstadoVoz.Text.StartsWith("✓", StringComparison.Ordinal))
                {
                    lblEstadoVoz.ForeColor = Color.FromArgb(100, 116, 139);
                    lblEstadoVoz.Text = "Micrófono apagado · pulsa 🎤 para otra frase";
                }
            }
        }

        private void ActualizarUiMicrofono(bool escuchando)
        {
            btnMicrofono.BackColor = escuchando
                ? Color.FromArgb(220, 38, 38)
                : Color.FromArgb(79, 70, 229);
            btnMicrofono.Text = escuchando ? "■" : "🎤";
        }

        private void btnMicrofono_Click(object? sender, EventArgs e)
        {
            if (_voz == null || !_voz.Preparado)
            {
                PrepararVozSinEscuchar();
                if (_voz == null || !_voz.Preparado)
                    return;
            }

            _voz.AlternarEscucha();
        }

        private void btnAplicar_Click(object? sender, EventArgs e)
        {
            _voz?.DetenerEscucha();

            if (string.IsNullOrWhiteSpace(txtPeticion.Text))
            {
                MessageBox.Show(
                    this,
                    "Escribe o dicta (🎤) qué quieres hacer con la foto.",
                    "IA foto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                txtPeticion.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object? sender, EventArgs e)
        {
            _voz?.DetenerEscucha();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FrmPeticionIaFoto_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _timerNivelVoz.Stop();
            _voz?.Dispose();
            _voz = null;
        }
    }
}
