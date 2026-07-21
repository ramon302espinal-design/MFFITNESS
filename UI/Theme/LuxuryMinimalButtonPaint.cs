using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Borde de lujo minimalista en Paint: radio 4px, 1px, gris sutil, AntiAlias.
    /// No modifica BackColor ni Font.
    /// </summary>
    public static class LuxuryMinimalButtonPaint
    {
        public const int CornerRadius = 4;
        private static readonly Color BorderColor = Color.FromArgb(48, 148, 163, 184);

        public static void Attach(Button button)
        {
            if (button == null || button.IsDisposed)
                return;

            button.Paint -= Button_Paint;
            button.Paint += Button_Paint;
        }

        public static void DrawBorder(Graphics g, Rectangle bounds)
        {
            if (g == null || bounds.Width <= 1 || bounds.Height <= 1)
                return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            using var path = CreateRoundedPath(rect, CornerRadius);
            using var pen = new Pen(BorderColor, 1f);
            g.DrawPath(pen, path);
        }

        private static void Button_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button btn)
                return;

            DrawBorder(e.Graphics, btn.ClientRectangle);
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            int d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
