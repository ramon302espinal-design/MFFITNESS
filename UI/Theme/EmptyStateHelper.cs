using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Muestra un mensaje elegante cuando un DataGridView está vacío.
    /// </summary>
    public static class EmptyStateHelper
    {
        private sealed class EmptyStateStyle
        {
            public string Message { get; set; } = "No hay registros para mostrar";
            public Color Color { get; set; } = AppTheme.TextSecondary;
            public Font Font { get; set; } = AppTheme.FontBody;
        }

        private static readonly Dictionary<DataGridView, EmptyStateStyle> Styles = new();

        public static void Attach(
            DataGridView dgv,
            string message = "No hay registros para mostrar",
            Color? textColor = null,
            Font? font = null)
        {
            Styles[dgv] = new EmptyStateStyle
            {
                Message = message,
                Color = textColor ?? AppTheme.TextSecondary,
                Font = font ?? AppTheme.FontBody
            };

            if (dgv.Tag?.ToString() == "empty-state")
            {
                dgv.Invalidate();
                return;
            }

            dgv.Tag = "empty-state";
            dgv.Paint += Dgv_Paint;
            dgv.RowsAdded += (_, _) => dgv.Invalidate();
            dgv.RowsRemoved += (_, _) => dgv.Invalidate();
            dgv.DataBindingComplete += (_, _) => dgv.Invalidate();
            dgv.DataSourceChanged += (_, _) => dgv.Invalidate();
        }

        private static void Dgv_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not DataGridView dgv) return;
            if (!IsEmpty(dgv)) return;
            if (!Styles.TryGetValue(dgv, out var style)) return;

            var rect = dgv.DisplayRectangle;
            using var bg = new SolidBrush(AppTheme.Surface);
            e.Graphics.FillRectangle(bg, rect);

            using var brush = new SolidBrush(style.Color);
            ThemeFonts.DrawString(e.Graphics, style.Message, style.Font, brush, rect,
                new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });
        }

        private static bool IsEmpty(DataGridView dgv)
        {
            return dgv.Rows.Count == 0 ||
                   (dgv.Rows.Count == 1 && dgv.Rows.Cast<DataGridViewRow>().All(r => r.IsNewRow));
        }
    }
}
