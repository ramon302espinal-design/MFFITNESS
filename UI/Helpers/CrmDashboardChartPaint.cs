using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace UI.Helpers
{
    /// <summary>
    /// Gráficos ligeros WinForms para FrmAnaDashboard (sin dependencias externas).
    /// Solo pintura — los datos vienen de binders BLL.
    /// </summary>
    public static class CrmDashboardChartPaint
    {
        public sealed class Segment
        {
            public string Label { get; init; } = string.Empty;
            public decimal Value { get; init; }
            public Color Color { get; init; }
        }

        public static void PaintStackedBar(
            PaintEventArgs e,
            Rectangle bounds,
            IReadOnlyList<Segment> segments,
            string? emptyMessage = null)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.FromArgb(247, 249, 252));

            if (segments == null || segments.Count == 0 || segments.All(s => s.Value <= 0))
            {
                DrawCentered(e.Graphics, bounds, emptyMessage ?? "Sin datos de capital");
                return;
            }

            decimal total = segments.Sum(s => s.Value);
            if (total <= 0)
            {
                DrawCentered(e.Graphics, bounds, emptyMessage ?? "Sin capital en inventario");
                return;
            }

            var bar = new Rectangle(bounds.X + 8, bounds.Y + 28, bounds.Width - 16, 22);
            int x = bar.X;
            int right = bar.Right;

            foreach (Segment seg in segments.Where(s => s.Value > 0))
            {
                int w = (int)Math.Round((double)(seg.Value / total) * bar.Width);
                if (w <= 0 && seg.Value > 0)
                    w = 1;
                if (x + w > right)
                    w = right - x;
                if (w <= 0)
                    continue;

                using var brush = new SolidBrush(seg.Color);
                e.Graphics.FillRectangle(brush, x, bar.Y, w, bar.Height);
                x += w;
            }

            using var border = new Pen(Color.FromArgb(203, 213, 224));
            e.Graphics.DrawRectangle(border, bar);

            int legendY = bar.Bottom + 6;
            int legendX = bounds.X + 8;
            using var font = new Font("Segoe UI", 7.5f);
            foreach (Segment seg in segments.Where(s => s.Value > 0))
            {
                using var brush = new SolidBrush(seg.Color);
                e.Graphics.FillRectangle(brush, legendX, legendY + 2, 10, 10);
                string pct = (seg.Value / total * 100m).ToString("0.#", CultureInfo.InvariantCulture);
                string text = $"{seg.Label} {pct}%";
                e.Graphics.DrawString(text, font, Brushes.DimGray, legendX + 14, legendY);
                legendX += Math.Min(bounds.Width / 2 - 8, TextRenderer.MeasureText(text, font).Width + 22);
                if (legendX > bounds.Right - 80)
                {
                    legendX = bounds.X + 8;
                    legendY += 14;
                }
            }
        }

        public static void PaintSparkline(
            PaintEventArgs e,
            Rectangle bounds,
            IReadOnlyList<decimal> values,
            string title,
            Color lineColor,
            string? subtitle = null,
            string? emptyMessage = null)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.FromArgb(247, 249, 252));

            using var titleFont = new Font("Segoe UI", 8f, FontStyle.Bold);
            e.Graphics.DrawString(title, titleFont, Brushes.DimGray, bounds.X + 6, bounds.Y + 4);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                using var subFont = new Font("Segoe UI", 7.5f);
                e.Graphics.DrawString(subtitle, subFont, Brushes.Gray, bounds.X + 6, bounds.Y + 18);
            }

            if (values == null || values.Count == 0)
            {
                DrawCentered(e.Graphics, new Rectangle(bounds.X, bounds.Y + 30, bounds.Width, bounds.Height - 34),
                    emptyMessage ?? "Sin serie para el período");
                return;
            }

            var plot = new Rectangle(bounds.X + 10, bounds.Y + 36, bounds.Width - 20, bounds.Height - 44);
            if (plot.Width < 4 || plot.Height < 4)
                return;

            decimal min = values.Min();
            decimal max = values.Max();
            if (max <= min)
            {
                max = min + 1m;
            }

            var points = new List<PointF>(values.Count);
            for (int i = 0; i < values.Count; i++)
            {
                float x = plot.Left + (values.Count == 1 ? plot.Width / 2f : i * (plot.Width - 1f) / (values.Count - 1));
                float norm = (float)((values[i] - min) / (max - min));
                float y = plot.Bottom - norm * (plot.Height - 1);
                points.Add(new PointF(x, y));
            }

            using var fillPath = new GraphicsPath();
            fillPath.AddLines(points.ToArray());
            fillPath.AddLine(points[^1].X, plot.Bottom, points[0].X, plot.Bottom);
            fillPath.CloseFigure();
            using var fillBrush = new SolidBrush(Color.FromArgb(40, lineColor));
            e.Graphics.FillPath(fillBrush, fillPath);

            using var pen = new Pen(lineColor, 2f);
            if (points.Count >= 2)
                e.Graphics.DrawLines(pen, points.ToArray());
            else
                e.Graphics.FillEllipse(Brushes.DarkGray, points[0].X - 2, points[0].Y - 2, 4, 4);

            using var axisPen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

            using var valFont = new Font("Segoe UI", 7f);
            string maxLabel = FormatCompact(max);
            string minLabel = FormatCompact(min);
            e.Graphics.DrawString(maxLabel, valFont, Brushes.Gray, plot.Right - 48, plot.Top);
            e.Graphics.DrawString(minLabel, valFont, Brushes.Gray, plot.Right - 48, plot.Bottom - 12);
        }

        private static string FormatCompact(decimal v)
        {
            if (Math.Abs(v) >= 1_000_000m)
                return (v / 1_000_000m).ToString("0.#M", CultureInfo.InvariantCulture);
            if (Math.Abs(v) >= 1_000m)
                return (v / 1_000m).ToString("0.#k", CultureInfo.InvariantCulture);
            return v.ToString("0", CultureInfo.InvariantCulture);
        }

        private static void DrawCentered(Graphics g, Rectangle bounds, string text)
        {
            using var font = new Font("Segoe UI", 8.5f);
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
            TextRenderer.DrawText(g, text, font, bounds, Color.FromArgb(113, 128, 150), flags);
        }
    }
}
