using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace UI.DISEÑO
{
    /// <summary>
    /// Cámara con selector de dispositivo (PC / Iriun / otras). Clic = captura → Usar / Repetir.
    /// </summary>
    public sealed class FrmCapturaProductoCamara : Form
    {
        private readonly ComboBox _cmbCamara;
        private readonly Button _btnActivar;
        private readonly PictureBox _preview;
        private readonly Panel _panelAcciones;
        private readonly Panel _panelTop;
        private readonly Button _btnUsar;
        private readonly Button _btnRepetir;
        private readonly Label _lblHint;
        private readonly System.Windows.Forms.Timer _timer;

        private VideoCapture? _capture;
        private Mat? _frame;
        private Bitmap? _frozen;
        private bool _capturado;
        private readonly List<int> _indicesCamara = new();

        public Image? FotoCapturada { get; private set; }

        public FrmCapturaProductoCamara()
        {
            Text = "Tomar foto del producto";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.Black;
            FormBorderStyle = FormBorderStyle.Sizable;
            KeyPreview = true;
            MinimizeBox = false;

            _panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(24, 24, 24)
            };

            var lblCam = new Label
            {
                Text = "Cámara:",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new System.Drawing.Point(16, 16),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            _cmbCamara = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(90, 12),
                Width = 420,
                Font = new Font("Segoe UI", 10F)
            };

            _btnActivar = new Button
            {
                Text = "Activar cámara",
                Location = new System.Drawing.Point(520, 10),
                Size = new System.Drawing.Size(140, 32),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _btnActivar.FlatAppearance.BorderSize = 0;
            _btnActivar.Click += (_, _) => ActivarCamaraSeleccionada();

            _panelTop.Controls.Add(lblCam);
            _panelTop.Controls.Add(_cmbCamara);
            _panelTop.Controls.Add(_btnActivar);

            _preview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black,
                Cursor = Cursors.Cross
            };
            _preview.Click += Preview_Click;

            _lblHint = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(20, 20, 20),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "Elige la cámara del PC (no Iriun) → Activar → clic para capturar · Esc cancela"
            };

            _panelAcciones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = Color.FromArgb(30, 30, 30),
                Visible = false
            };

            _btnUsar = new Button
            {
                Text = "Usar foto",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(180, 48)
            };
            _btnUsar.FlatAppearance.BorderSize = 0;
            _btnUsar.Click += BtnUsar_Click;

            _btnRepetir = new Button
            {
                Text = "Repetir",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(180, 48)
            };
            _btnRepetir.FlatAppearance.BorderSize = 0;
            _btnRepetir.Click += BtnRepetir_Click;

            _panelAcciones.Controls.Add(_btnUsar);
            _panelAcciones.Controls.Add(_btnRepetir);

            Controls.Add(_preview);
            Controls.Add(_panelAcciones);
            Controls.Add(_lblHint);
            Controls.Add(_panelTop);

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += Timer_Tick;

            Load += Frm_Load;
            FormClosing += Frm_FormClosing;
            KeyDown += Frm_KeyDown;
            Resize += (_, _) => ReposicionarBotones();
        }

        private void ReposicionarBotones()
        {
            int total = _btnUsar.Width + _btnRepetir.Width + 24;
            int x = Math.Max(16, (_panelAcciones.ClientSize.Width - total) / 2);
            _btnUsar.Location = new System.Drawing.Point(x, 12);
            _btnRepetir.Location = new System.Drawing.Point(x + _btnUsar.Width + 24, 12);
        }

        private void Frm_Load(object? sender, EventArgs e)
        {
            EnumerarCamaras();
            if (_indicesCamara.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "No se detectó ninguna cámara. Conecta la cámara del PC o Iriun y reabre.",
                    "Cámara",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // Preferir índice != 0 si hay varias (0 suele ser Iriun / virtual).
            int prefer = _indicesCamara.Count > 1 ? 1 : 0;
            _cmbCamara.SelectedIndex = Math.Min(prefer, _cmbCamara.Items.Count - 1);
            ActivarCamaraSeleccionada();
            ReposicionarBotones();
        }

        private void EnumerarCamaras()
        {
            _cmbCamara.Items.Clear();
            _indicesCamara.Clear();

            for (int i = 0; i < 8; i++)
            {
                VideoCapture? test = null;
                try
                {
                    test = new VideoCapture(i, VideoCaptureAPIs.DSHOW);
                    if (!test.IsOpened())
                    {
                        test.Release();
                        test.Dispose();
                        test = new VideoCapture(i);
                    }

                    if (!test.IsOpened())
                    {
                        test.Dispose();
                        continue;
                    }

                    _indicesCamara.Add(i);
                    string label = i == 0
                        ? $"Cámara {i} (a menudo Iriun / virtual)"
                        : $"Cámara {i} (PC / USB recomendada)";
                    _cmbCamara.Items.Add(label);
                }
                catch
                {
                    // índice no disponible
                }
                finally
                {
                    try { test?.Release(); } catch { /* ignore */ }
                    test?.Dispose();
                }
            }
        }

        private void ActivarCamaraSeleccionada()
        {
            if (_cmbCamara.SelectedIndex < 0 || _cmbCamara.SelectedIndex >= _indicesCamara.Count)
                return;

            int index = _indicesCamara[_cmbCamara.SelectedIndex];
            DetenerCaptura();

            try
            {
                _capture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
                if (!_capture.IsOpened())
                {
                    _capture.Dispose();
                    _capture = new VideoCapture(index);
                }

                if (!_capture.IsOpened())
                {
                    MessageBox.Show(
                        this,
                        $"No se pudo abrir la cámara {index}. Prueba otra de la lista.",
                        "Cámara",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _capture.Set(VideoCaptureProperties.FrameWidth, 640);
                _capture.Set(VideoCaptureProperties.FrameHeight, 480);
                _frame = new Mat();
                _capturado = false;
                _panelAcciones.Visible = false;
                _lblHint.Text = "Haz clic en la pantalla para tomar la foto · Esc cancela";
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error al activar cámara: " + ex.Message, "Cámara",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DetenerCaptura()
        {
            _timer.Stop();
            try { _capture?.Release(); } catch { /* ignore */ }
            _capture?.Dispose();
            _capture = null;
            _frame?.Dispose();
            _frame = null;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_capturado || _capture == null || _frame == null)
                return;

            try
            {
                if (!_capture.Read(_frame) || _frame.Empty())
                    return;

                Bitmap bmp = BitmapConverter.ToBitmap(_frame);
                Image? old = _preview.Image;
                _preview.Image = bmp;
                old?.Dispose();
            }
            catch
            {
                // Frame perdido.
            }
        }

        private void Preview_Click(object? sender, EventArgs e)
        {
            if (_capturado || _preview.Image == null)
                return;

            _capturado = true;
            _timer.Stop();
            _frozen?.Dispose();
            _frozen = new Bitmap(_preview.Image);
            _lblHint.Text = "¿Usar esta foto o repetir?";
            _panelAcciones.Visible = true;
            ReposicionarBotones();
        }

        private void BtnRepetir_Click(object? sender, EventArgs e)
        {
            _capturado = false;
            _frozen?.Dispose();
            _frozen = null;
            _panelAcciones.Visible = false;
            _lblHint.Text = "Haz clic en la pantalla para tomar la foto · Esc cancela";
            _timer.Start();
        }

        private void BtnUsar_Click(object? sender, EventArgs e)
        {
            if (_frozen == null)
            {
                MessageBox.Show(this, "Primero toma una foto haciendo clic en la pantalla.", "Foto",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FotoCapturada = new Bitmap(_frozen);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Frm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void Frm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DetenerCaptura();
            _timer.Dispose();
            _frozen?.Dispose();
            _frozen = null;
            Image? img = _preview.Image;
            _preview.Image = null;
            img?.Dispose();
        }
    }
}
