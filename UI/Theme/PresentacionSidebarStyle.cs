using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Estilo moderno/minimalista solo para el menú lateral de FrmPresentacion (panel1).
    /// No modifica el dashboard ni otros controles del formulario.
    /// </summary>
    public static class PresentacionSidebarStyle
    {
        private const int SidePad = 12;
        private const int TopPad = 20;
        private const int BtnHeight = 46;
        private const int Gap = 8;

        public static void Apply(Panel sidebar, Button logoutButton, params (Button Button, string Icon, string Label)[] items)
        {
            if (sidebar == null || sidebar.IsDisposed)
                return;

            sidebar.SuspendLayout();
            try
            {
                sidebar.BackColor = AppTheme.Sidebar;
                sidebar.Padding = new Padding(SidePad, TopPad, SidePad, SidePad);

                int innerWidth = Math.Max(200, sidebar.ClientSize.Width - (SidePad * 2));
                int x = SidePad;
                int y = TopPad;

                foreach (var (btn, icon, label) in items)
                {
                    if (btn == null || btn.IsDisposed)
                        continue;

                    ShellTheme.StyleNavButtonWithIcon(btn, icon, label);
                    btn.Dock = DockStyle.None;
                    btn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    btn.Margin = Padding.Empty;
                    btn.AutoSize = false;
                    btn.Size = new Size(innerWidth, BtnHeight);
                    btn.Font = AppTheme.FontNav;

                    if (!btn.Visible)
                    {
                        btn.Location = new Point(-4000, y);
                        continue;
                    }

                    btn.Location = new Point(x, y);
                    ThemeApplier.ApplyRoundedRegion(btn, LuxuryMinimalButtonPaint.CornerRadius);
                    LuxuryMinimalButtonPaint.Attach(btn);
                    CobrarButtonStyle.ApplyIfCobrarText(btn);
                    y += BtnHeight + Gap;
                }

                if (logoutButton != null && !logoutButton.IsDisposed)
                {
                    ShellTheme.StyleNavButtonWithIcon(logoutButton, NavIcons.Logout, "SALIR");
                    logoutButton.Dock = DockStyle.Bottom;
                    logoutButton.Height = BtnHeight + 4;
                    logoutButton.FlatAppearance.BorderSize = 0;
                    logoutButton.ForeColor = AppTheme.Error;
                    ThemeApplier.ApplyRoundedRegion(logoutButton, LuxuryMinimalButtonPaint.CornerRadius);
                    LuxuryMinimalButtonPaint.Attach(logoutButton);
                }

                // COBRAR del menú principal (btnPagar) — verde + blanco aunque el paint use iconos
                foreach (var (btn, _, label) in items)
                {
                    if (btn != null && label.Equals("COBRAR", StringComparison.OrdinalIgnoreCase))
                        CobrarButtonStyle.Apply(btn);
                }
            }
            finally
            {
                sidebar.ResumeLayout(true);
            }
        }
    }
}
