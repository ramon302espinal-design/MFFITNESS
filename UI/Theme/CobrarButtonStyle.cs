using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Estilo uniforme para botones con texto COBRAR: fondo verde, letra blanca.
    /// </summary>
    public static class CobrarButtonStyle
    {
        public static readonly Color Verde = Color.FromArgb(22, 163, 74);
        public static readonly Color VerdeHover = Color.FromArgb(34, 197, 94);

        public static void Apply(Button btn)
        {
            if (btn == null || btn.IsDisposed)
                return;

            btn.BackColor = Verde;
            btn.ForeColor = Color.White;
            btn.UseVisualStyleBackColor = false;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter -= OnEnter;
            btn.MouseLeave -= OnLeave;
            btn.MouseEnter += OnEnter;
            btn.MouseLeave += OnLeave;
        }

        public static void ApplyIfCobrarText(Button btn)
        {
            if (btn == null)
                return;

            string text = btn.Text?.Trim() ?? "";
            if (text.Equals("COBRAR", StringComparison.OrdinalIgnoreCase)
                || text.Equals("IR A COBRAR", StringComparison.OrdinalIgnoreCase))
            {
                Apply(btn);
            }
        }

        private static void OnEnter(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackColor = VerdeHover;
                btn.ForeColor = Color.White;
                btn.Invalidate();
            }
        }

        private static void OnLeave(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackColor = Verde;
                btn.ForeColor = Color.White;
                btn.Invalidate();
            }
        }
    }
}
