using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UI.Helpers;

namespace UI.DISEÑO
{
    /// <summary>
    /// Endereza (ángulo fino) y recorta con selección por arrastre.
    /// UI editable en el diseñador WinForms (Visual Studio → Shift+F7).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmRecortarEnderezarFoto : Form
    {
        private Image? _original;
        private Bitmap? _preview;
        private bool _arrastrando;
        private Point _inicioSel;
        private Rectangle _selDisplay = Rectangle.Empty;

        public Image? Resultado { get; private set; }

        /// <summary>Ctor para el diseñador Visual Studio.</summary>
        public FrmRecortarEnderezarFoto()
        {
            InitializeComponent();
        }

        public FrmRecortarEnderezarFoto(Image fuente)
            : this()
        {
            ArgumentNullException.ThrowIfNull(fuente);
            _original = new Bitmap(fuente);
            RebuildPreview();
        }

        private void LiberarImagenesRuntime()
        {
            if (picFoto != null)
                picFoto.Image = null;

            _preview?.Dispose();
            _preview = null;
            _original?.Dispose();
            _original = null;
        }

        private void trkAngulo_ValueChanged(object? sender, EventArgs e)
        {
            lblAngulo.Text = $"Enderezar: {trkAngulo.Value}°";
            _selDisplay = Rectangle.Empty;
            RebuildPreview();
        }

        private void btnLimpiarRecorte_Click(object? sender, EventArgs e)
        {
            _selDisplay = Rectangle.Empty;
            picFoto.Invalidate();
        }

        private void btnCancelar_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void RebuildPreview()
        {
            if (_original == null)
                return;

            Bitmap? next = ProductoImagenHelper.RotarGrados(_original, trkAngulo.Value);
            Image? old = picFoto.Image;
            picFoto.Image = next;
            if (!ReferenceEquals(old, _preview))
                old?.Dispose();
            _preview?.Dispose();
            _preview = next;
            picFoto.Invalidate();
        }

        private void picFoto_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _preview == null)
                return;

            Rectangle disp = GetImageDisplayRect(picFoto);
            if (!disp.Contains(e.Location))
                return;

            _arrastrando = true;
            _inicioSel = e.Location;
            _selDisplay = new Rectangle(e.Location, Size.Empty);
        }

        private void picFoto_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_arrastrando)
                return;

            int x1 = Math.Min(_inicioSel.X, e.X);
            int y1 = Math.Min(_inicioSel.Y, e.Y);
            int x2 = Math.Max(_inicioSel.X, e.X);
            int y2 = Math.Max(_inicioSel.Y, e.Y);
            _selDisplay = Rectangle.Intersect(
                GetImageDisplayRect(picFoto),
                Rectangle.FromLTRB(x1, y1, x2, y2));
            picFoto.Invalidate();
        }

        private void picFoto_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_arrastrando)
                return;
            _arrastrando = false;
            if (_selDisplay.Width < 8 || _selDisplay.Height < 8)
                _selDisplay = Rectangle.Empty;
            picFoto.Invalidate();
        }

        private void picFoto_Paint(object? sender, PaintEventArgs e)
        {
            if (_selDisplay.Width < 2 || _selDisplay.Height < 2)
                return;

            using var fill = new SolidBrush(Color.FromArgb(55, 59, 130, 246));
            using var pen = new Pen(Color.FromArgb(37, 99, 235), 2) { DashStyle = DashStyle.Dash };
            e.Graphics.FillRectangle(fill, _selDisplay);
            e.Graphics.DrawRectangle(pen, _selDisplay);
        }

        private void btnAplicar_Click(object? sender, EventArgs e)
        {
            if (_preview == null)
                return;

            try
            {
                Bitmap baseImg = new Bitmap(_preview);
                if (_selDisplay.Width >= 8 && _selDisplay.Height >= 8)
                {
                    Rectangle imgRect = MapDisplayToImage(picFoto, _selDisplay, baseImg.Size);
                    Bitmap cropped = ProductoImagenHelper.Recortar(baseImg, imgRect);
                    baseImg.Dispose();
                    Resultado = cropped;
                }
                else
                {
                    Resultado = baseImg;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo aplicar el recorte/enderezado.\n" + ex.Message,
                    "Foto producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private static Rectangle GetImageDisplayRect(PictureBox pb)
        {
            if (pb.Image == null)
                return Rectangle.Empty;

            float ratio = Math.Min(
                (float)pb.ClientSize.Width / pb.Image.Width,
                (float)pb.ClientSize.Height / pb.Image.Height);
            int w = Math.Max(1, (int)Math.Round(pb.Image.Width * ratio));
            int h = Math.Max(1, (int)Math.Round(pb.Image.Height * ratio));
            int x = (pb.ClientSize.Width - w) / 2;
            int y = (pb.ClientSize.Height - h) / 2;
            return new Rectangle(x, y, w, h);
        }

        private static Rectangle MapDisplayToImage(PictureBox pb, Rectangle displaySel, Size imageSize)
        {
            Rectangle disp = GetImageDisplayRect(pb);
            if (disp.Width < 1 || disp.Height < 1)
                return new Rectangle(0, 0, imageSize.Width, imageSize.Height);

            float sx = (float)imageSize.Width / disp.Width;
            float sy = (float)imageSize.Height / disp.Height;
            int x = (int)Math.Round((displaySel.X - disp.X) * sx);
            int y = (int)Math.Round((displaySel.Y - disp.Y) * sy);
            int w = (int)Math.Round(displaySel.Width * sx);
            int h = (int)Math.Round(displaySel.Height * sy);
            return Rectangle.Intersect(
                new Rectangle(0, 0, imageSize.Width, imageSize.Height),
                new Rectangle(x, y, w, h));
        }
    }
}
