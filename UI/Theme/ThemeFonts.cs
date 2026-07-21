using System;
using System.Drawing;
using System.Drawing.Text;

namespace UI.Theme
{
    /// <summary>
    /// Utilidades seguras para pintado GDI+ con fuentes compartidas.
    /// </summary>
    public static class ThemeFonts
    {
        public static void ValidateFont(Font font)
        {
            using var bmp = new Bitmap(4, 4);
            using var g = Graphics.FromImage(bmp);
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.DrawString("A", font, Brushes.Black, 0, 0);
        }

        public static void DrawString(Graphics g, string text, Font font, Brush brush, float x, float y)
        {
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                g.DrawString(text, font, brush, x, y);
            }
            catch (ArgumentException)
            {
                g.DrawString(text, SystemFonts.DefaultFont, brush, x, y);
            }
        }

        public static void DrawString(Graphics g, string text, Font font, Brush brush, float x, float y, StringFormat format)
        {
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                g.DrawString(text, font, brush, x, y, format);
            }
            catch (ArgumentException)
            {
                g.DrawString(text, SystemFonts.DefaultFont, brush, x, y, format);
            }
        }

        public static void DrawString(Graphics g, string text, Font font, Brush brush, RectangleF layout, StringFormat? format = null)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (layout.Width <= 0 || layout.Height <= 0) return;

            try
            {
                if (format == null)
                    g.DrawString(text, font, brush, layout);
                else
                    g.DrawString(text, font, brush, layout, format);
            }
            catch (ArgumentException)
            {
                var fallback = format ?? StringFormat.GenericDefault;
                g.DrawString(text, SystemFonts.DefaultFont, brush, layout, fallback);
            }
        }
    }
}
