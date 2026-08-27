using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using UI.Helpers;

namespace UI.DISEÑO
{
    /// <summary>
    /// Collage 2 paneles: izquierda nombre → derecha código. Una cámara Iriun compartida.
    /// IA arranca solo cuando el padre recibe ambas decisiones (código opcional vía Omitir).
    /// </summary>
    public sealed class FrmCapturaProductoDualCamara : Form
    {
        private enum PasoCaptura
        {
            NombreEnVivo,
            NombreRevision,
            CodigoEnVivo,
            CodigoRevision
        }

        private readonly PictureBox _previewNombre;
        private readonly PictureBox _previewCodigo;
        private readonly Panel _overlayCodigo;
        private readonly Label _lblTituloNombre;
        private readonly Label _lblTituloCodigo;
        private readonly Label _lblHint;
        private readonly Panel _accionesNombre;
        private readonly Panel _accionesCodigo;
        private readonly Button _btnRepetirNombre;
        private readonly Button _btnAceptarNombre;
        private readonly Button _btnRepetirCodigo;
        private readonly Button _btnAceptarCodigo;
        private Button _btnOmitirCodigo;
        private readonly System.Windows.Forms.Timer _timer;

        private VideoCapture? _capture;
        private Mat? _frame;
        private Bitmap? _frozenActual;
        private PasoCaptura _paso = PasoCaptura.NombreEnVivo;
        private int _indiceCamara = -1;

        public Image? FotoNombre { get; private set; }
        public byte[]? JpegCodigo { get; private set; }

        public FrmCapturaProductoDualCamara()
        {
            Text = "Captura producto — Nombre + Código";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(18, 18, 18);
            FormBorderStyle = FormBorderStyle.Sizable;
            KeyPreview = true;
            MinimizeBox = false;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel panelNombre = CrearCelda(
                "1 · NOMBRE DEL PRODUCTO",
                Color.FromArgb(37, 99, 235),
                out _lblTituloNombre,
                out _previewNombre,
                out _accionesNombre,
                out _btnRepetirNombre,
                out _btnAceptarNombre,
                aceptarTexto: "Aceptar",
                mostrarOmitir: false);

            Panel panelCodigo = CrearCelda(
                "2 · CÓDIGO DE BARRAS",
                Color.FromArgb(22, 163, 74),
                out _lblTituloCodigo,
                out _previewCodigo,
                out _accionesCodigo,
                out _btnRepetirCodigo,
                out _btnAceptarCodigo,
                aceptarTexto: "Aceptar",
                mostrarOmitir: false,
                out _);

            _btnOmitirCodigo = new Button
            {
                Text = "Omitir",
                Size = new System.Drawing.Size(100, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Visible = false
            };
            _btnOmitirCodigo.FlatAppearance.BorderSize = 0;
            panelCodigo.Controls.Add(_btnOmitirCodigo);
            _btnOmitirCodigo.BringToFront();

            _overlayCodigo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(160, 0, 0, 0)
            };
            var lblEspera = new Label
            {
                Text = "Acepta la foto del nombre\npara activar esta cámara",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            _overlayCodigo.Controls.Add(lblEspera);
            panelCodigo.Controls.Add(_overlayCodigo);
            _overlayCodigo.BringToFront();

            root.Controls.Add(panelNombre, 0, 0);
            root.Controls.Add(panelCodigo, 1, 0);

            _lblHint = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(24, 24, 24),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "Izquierda activa · Enfoca el NOMBRE · clic para capturar · Esc cancela"
            };

            Controls.Add(root);
            Controls.Add(_lblHint);

            _previewNombre.Click += (_, _) => Preview_Click(esCodigo: false);
            _previewCodigo.Click += (_, _) => Preview_Click(esCodigo: true);

            _btnRepetirNombre.Click += (_, _) => Repetir(esCodigo: false);
            _btnAceptarNombre.Click += (_, _) => AceptarNombre();
            _btnRepetirCodigo.Click += (_, _) => Repetir(esCodigo: true);
            _btnAceptarCodigo.Click += (_, _) => AceptarCodigo();
            _btnOmitirCodigo.Click += (_, _) => OmitirCodigo();

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += Timer_Tick;

            Load += Frm_Load;
            FormClosing += Frm_FormClosing;
            KeyDown += Frm_KeyDown;
            Resize += (_, _) => ReposicionarOmitir();
            ActualizarEstadoVisual();
        }

        private void ReposicionarOmitir()
        {
            if (_btnOmitirCodigo.Parent == null)
                return;

            _btnOmitirCodigo.Location = new System.Drawing.Point(
                _btnOmitirCodigo.Parent.ClientSize.Width - _btnOmitirCodigo.Width - 12,
                40);
        }

        private static Panel CrearCelda(
            string titulo,
            Color colorTitulo,
            out Label lblTitulo,
            out PictureBox preview,
            out Panel acciones,
            out Button btnRepetir,
            out Button btnAceptar,
            string aceptarTexto,
            bool mostrarOmitir,
            out Button btnOmitir)
        {
            btnOmitir = new Button();
            var celda = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(6),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            lblTitulo = new Label
            {
                Text = titulo,
                Dock = DockStyle.Top,
                Height = 32,
                ForeColor = colorTitulo,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            preview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black,
                Cursor = Cursors.Cross,
                Margin = new Padding(0, 4, 0, 4)
            };

            acciones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                Visible = false
            };

            btnRepetir = new Button
            {
                Text = "Repetir",
                Size = new System.Drawing.Size(140, 44),
                Location = new System.Drawing.Point(8, 6),
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnRepetir.FlatAppearance.BorderSize = 0;

            btnAceptar = new Button
            {
                Text = aceptarTexto,
                Size = new System.Drawing.Size(140, 44),
                Location = new System.Drawing.Point(156, 6),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnAceptar.FlatAppearance.BorderSize = 0;

            acciones.Controls.Add(btnRepetir);
            acciones.Controls.Add(btnAceptar);

            if (mostrarOmitir)
            {
                btnOmitir = new Button
                {
                    Text = "Omitir",
                    Size = new System.Drawing.Size(100, 44),
                    Location = new System.Drawing.Point(304, 6),
                    BackColor = Color.FromArgb(100, 116, 139),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                btnOmitir.FlatAppearance.BorderSize = 0;
                acciones.Controls.Add(btnOmitir);
            }

            celda.Controls.Add(preview);
            celda.Controls.Add(acciones);
            celda.Controls.Add(lblTitulo);
            return celda;
        }

        // Sobrecarga sin btnOmitir para panel nombre
        private static Panel CrearCelda(
            string titulo,
            Color colorTitulo,
            out Label lblTitulo,
            out PictureBox preview,
            out Panel acciones,
            out Button btnRepetir,
            out Button btnAceptar,
            string aceptarTexto,
            bool mostrarOmitir)
        {
            return CrearCelda(
                titulo, colorTitulo, out lblTitulo, out preview, out acciones,
                out btnRepetir, out btnAceptar, aceptarTexto, mostrarOmitir,
                out _);
        }

        private void Frm_Load(object? sender, EventArgs e)
        {
            _indiceCamara = ResolverIndiceIriun();
            if (_indiceCamara < 0)
            {
                MessageBox.Show(
                    this,
                    "No se detectó Iriun Webcam. Conéctala y reabre.",
                    "Cámara",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (!IniciarCamara())
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _timer.Start();
        }

        private static int ResolverIndiceIriun()
        {
            for (int i = 0; i < 8; i++)
            {
                using var test = AbrirCaptura(i);
                if (test != null && test.IsOpened())
                    return i;
            }
            return -1;
        }

        private static VideoCapture? AbrirCaptura(int index)
        {
            var cap = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            if (cap.IsOpened())
                return cap;
            cap.Dispose();
            cap = new VideoCapture(index);
            return cap.IsOpened() ? cap : null;
        }

        private bool IniciarCamara()
        {
            DetenerCamara();
            try
            {
                _capture = AbrirCaptura(_indiceCamara);
                if (_capture == null || !_capture.IsOpened())
                    return false;

                _capture.Set(VideoCaptureProperties.FrameWidth, 1280);
                _capture.Set(VideoCaptureProperties.FrameHeight, 720);
                _frame = new Mat();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void DetenerCamara()
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
            if (_capture == null || _frame == null)
                return;

            if (_paso != PasoCaptura.NombreEnVivo && _paso != PasoCaptura.CodigoEnVivo)
                return;

            try
            {
                if (!_capture.Read(_frame) || _frame.Empty())
                    return;

                Bitmap bmp = BitmapConverter.ToBitmap(_frame);
                PictureBox destino = _paso == PasoCaptura.NombreEnVivo ? _previewNombre : _previewCodigo;
                Image? old = destino.Image;
                destino.Image = bmp;
                old?.Dispose();
            }
            catch
            {
                // frame perdido
            }
        }

        private void Preview_Click(bool esCodigo)
        {
            if (esCodigo && (_paso != PasoCaptura.CodigoEnVivo || _overlayCodigo.Visible))
                return;
            if (!esCodigo && _paso != PasoCaptura.NombreEnVivo)
                return;

            PictureBox activo = esCodigo ? _previewCodigo : _previewNombre;
            if (activo.Image == null)
                return;

            _frozenActual?.Dispose();
            _frozenActual = new Bitmap(activo.Image);
            _timer.Stop();

            if (esCodigo)
            {
                _paso = PasoCaptura.CodigoRevision;
                _accionesCodigo.Visible = true;
            }
            else
            {
                _paso = PasoCaptura.NombreRevision;
                _accionesNombre.Visible = true;
            }

            ActualizarEstadoVisual();
        }

        private void Repetir(bool esCodigo)
        {
            if (esCodigo && _paso != PasoCaptura.CodigoRevision)
                return;
            if (!esCodigo && _paso != PasoCaptura.NombreRevision)
                return;

            _frozenActual?.Dispose();
            _frozenActual = null;

            if (esCodigo)
            {
                _paso = PasoCaptura.CodigoEnVivo;
                _accionesCodigo.Visible = false;
            }
            else
            {
                _paso = PasoCaptura.NombreEnVivo;
                _accionesNombre.Visible = false;
            }

            _timer.Start();
            ActualizarEstadoVisual();
        }

        private void AceptarNombre()
        {
            if (_paso != PasoCaptura.NombreRevision || _frozenActual == null)
                return;

            FotoNombre?.Dispose();
            FotoNombre = new Bitmap(_frozenActual);

            Image? old = _previewNombre.Image;
            _previewNombre.Image = new Bitmap(_frozenActual);
            old?.Dispose();

            _frozenActual.Dispose();
            _frozenActual = null;
            _accionesNombre.Visible = false;

            _paso = PasoCaptura.CodigoEnVivo;
            _overlayCodigo.Visible = false;
            _previewCodigo.Image = null;
            _timer.Start();
            ActualizarEstadoVisual();
        }

        private void AceptarCodigo()
        {
            if (_paso != PasoCaptura.CodigoRevision || _frozenActual == null)
                return;

            using var foto = new Bitmap(_frozenActual);
            JpegCodigo = ProductoImagenHelper.ToJpegBytes(foto, maxSide: 1024, quality: 85);

            Finalizar(DialogResult.OK);
        }

        private void OmitirCodigo()
        {
            if (_paso != PasoCaptura.CodigoEnVivo && _paso != PasoCaptura.CodigoRevision)
                return;

            JpegCodigo = null;
            Finalizar(DialogResult.OK);
        }

        private void Finalizar(DialogResult result)
        {
            DialogResult = result;
            Close();
        }

        private void ActualizarEstadoVisual()
        {
            bool nombreActivo = _paso is PasoCaptura.NombreEnVivo or PasoCaptura.NombreRevision;
            bool codigoActivo = _paso is PasoCaptura.CodigoEnVivo or PasoCaptura.CodigoRevision;

            _previewNombre.Cursor = _paso == PasoCaptura.NombreEnVivo ? Cursors.Cross : Cursors.Default;
            _previewCodigo.Cursor = _paso == PasoCaptura.CodigoEnVivo ? Cursors.Cross : Cursors.Default;

            _lblTituloNombre.ForeColor = nombreActivo
                ? Color.FromArgb(96, 165, 250)
                : Color.FromArgb(100, 116, 139);
            _lblTituloCodigo.ForeColor = codigoActivo
                ? Color.FromArgb(74, 222, 128)
                : Color.FromArgb(100, 116, 139);

            _btnOmitirCodigo.Visible = codigoActivo;
            ReposicionarOmitir();

            _lblHint.Text = _paso switch
            {
                PasoCaptura.NombreEnVivo => "← Activo · Enfoca el NOMBRE · clic capturar · Repetir / Aceptar",
                PasoCaptura.NombreRevision => "← Revisa la foto del nombre · Repetir o Aceptar para continuar →",
                PasoCaptura.CodigoEnVivo => "→ Activo · Enfoca el CÓDIGO DE BARRAS · clic capturar · Omitir si no aplica",
                PasoCaptura.CodigoRevision => "→ Revisa el código · Repetir, Aceptar u Omitir",
                _ => _lblHint.Text
            };
        }

        private void Frm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Escape)
                return;

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Frm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DetenerCamara();
            _timer.Dispose();
            _frozenActual?.Dispose();
            _frozenActual = null;

            if (DialogResult != DialogResult.OK)
            {
                FotoNombre?.Dispose();
                FotoNombre = null;
                JpegCodigo = null;
            }

            LiberarPreview(_previewNombre);
            LiberarPreview(_previewCodigo);
        }

        private static void LiberarPreview(PictureBox box)
        {
            Image? img = box.Image;
            box.Image = null;
            img?.Dispose();
        }
    }
}
