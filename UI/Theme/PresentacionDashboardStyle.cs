using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Estilo tecnológico/minimalista solo para panelDashboard de FrmPresentacion.
    /// Conserva tamaño y posición de las 4 tarjetas KPI.
    /// </summary>
    public static class PresentacionDashboardStyle
    {
        public const int CardWidth = 300;
        public const int CardHeight = 168;

        public static Label Apply(
            Panel host,
            Panel panelActivos, Label titleActivos, Label valueActivos,
            Panel panelVencidos, Label titleVencidos, Label valueVencidos,
            Panel panelHoy, Label titleHoy, Label valueHoy,
            Panel panelMes, Label titleMes, Label valueMes)
        {
            if (host == null || host.IsDisposed)
                return new Label();

            host.SuspendLayout();
            try
            {
                host.BackColor = AppTheme.Background;
                host.Padding = new Padding(24, 12, 24, 16);

                var welcome = EnsureLabel(host, "lblDashWelcome", new Point(40, 10));
                StyleHeaderLabel(welcome, AppTheme.FontSubtitle, AppTheme.TextPrimary);

                var clock = EnsureLabel(host, "lblDashDateTime", new Point(host.ClientSize.Width - 420, 14));
                clock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                StyleHeaderLabel(clock, AppTheme.FontBody, AppTheme.TextSecondary);
                clock.AutoSize = true;
                PositionClock(host, clock);

                // Misma posición/tamaño que el Designer
                StyleCard(panelActivos, titleActivos, valueActivos, AppTheme.Success, "CLIENTES ACTIVOS", new Point(40, 48));
                StyleCard(panelVencidos, titleVencidos, valueVencidos, AppTheme.Error, "CLIENTES VENCIDOS", new Point(360, 48));
                StyleCard(panelHoy, titleHoy, valueHoy, AppTheme.Primary, "INGRESOS HOY", new Point(680, 48));
                StyleCard(panelMes, titleMes, valueMes, Color.FromArgb(168, 85, 247), "INGRESOS MENSUAL", new Point(1000, 48));

                welcome.BringToFront();
                clock.BringToFront();
                return welcome;
            }
            finally
            {
                host.ResumeLayout(true);
            }
        }

        public static void ActualizarEncabezado(Label? welcome, Label? clock)
        {
            if (welcome != null && !welcome.IsDisposed)
            {
                string usuario = CORE.Sesion.Usuario?.Trim() ?? "Usuario";
                string rol = FormatearRol(CORE.Sesion.Rol);
                welcome.Text = $"Bienvenido, {usuario}  ·  {rol}";
            }

            if (clock != null && !clock.IsDisposed)
            {
                var cultura = new CultureInfo("es-DO");
                clock.Text = DateTime.Now.ToString("dddd, d 'de' MMMM yyyy  ·  HH:mm:ss", cultura);
                if (clock.Parent is Panel host)
                    PositionClock(host, clock);
            }
        }

        private static void StyleCard(Panel card, Label title, Label value, Color accent, string titleText, Point location)
        {
            // Conservar sitio y tamaño exactos del layout actual
            var size = new Size(CardWidth, CardHeight);
            card.Location = location;
            card.Size = size;
            card.MinimumSize = size;
            card.MaximumSize = size;
            card.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            card.Dock = DockStyle.None;

            StatCardHelper.Configure(card, title, value, accent, titleText);
            card.Location = location;
            card.Size = size;
        }

        private static Label EnsureLabel(Panel host, string name, Point location)
        {
            if (host.Controls[name] is Label existing)
            {
                existing.Location = location;
                return existing;
            }

            var label = new Label
            {
                Name = name,
                AutoSize = true,
                Location = location,
                BackColor = Color.Transparent
            };
            host.Controls.Add(label);
            return label;
        }

        private static void StyleHeaderLabel(Label label, Font font, Color color)
        {
            label.AutoSize = true;
            label.Font = font;
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            label.Dock = DockStyle.None;
        }

        private static void PositionClock(Panel host, Label clock)
        {
            clock.AutoSize = true;
            int x = Math.Max(40, host.ClientSize.Width - clock.PreferredWidth - 32);
            clock.Location = new Point(x, 16);
        }

        private static string FormatearRol(string? rol)
        {
            return (rol?.Trim().ToUpperInvariant()) switch
            {
                "ADMIN" => "Administrador",
                "CAJERO" => "Cajero",
                "CONSULTA" => "Consulta",
                _ => string.IsNullOrWhiteSpace(rol) ? "Usuario" : rol.Trim()
            };
        }
    }
}
