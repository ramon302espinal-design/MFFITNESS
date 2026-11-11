using System;
using System.Drawing;
using System.Windows.Forms;
using CORE;

namespace UI.DISEÑO
{
    public partial class FrmEstadoClientes
    {
        private ToolStripDropDown? _calendarioPopup;

        private void btnCalendario_Click(object? sender, EventArgs e)
        {
            if (_calendarioPopup != null && _calendarioPopup.Visible)
            {
                _calendarioPopup.Close();
                return;
            }

            MostrarCalendarioFlotante();
        }

        /// <summary>
        /// Popup anclado al botón: se cierra solo al hacer clic fuera (AutoClose).
        /// </summary>
        private void MostrarCalendarioFlotante()
        {
            if (btnCalendario == null || btnCalendario.IsDisposed)
                return;

            DateTime ahora = TimeZoneHelper.NowDominicanRepublic();

            Color bgCard = Color.FromArgb(12, 14, 18);
            Color bgHeader = Color.FromArgb(18, 22, 30);
            Color accent = Color.FromArgb(27, 146, 255);
            Color muted = Color.FromArgb(150, 165, 185);

            var panel = new Panel
            {
                BackColor = bgCard,
                Padding = new Padding(0),
                Size = new Size(304, 332)
            };

            // Borde sutil
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(55, 70, 95), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            string diaSemana = ahora.ToString("dddd", CulturaDo);
            if (!string.IsNullOrEmpty(diaSemana))
                diaSemana = char.ToUpper(diaSemana[0], CulturaDo) + diaSemana[1..];

            string mesAnio = ahora.ToString("MMMM yyyy", CulturaDo);
            if (!string.IsNullOrEmpty(mesAnio))
                mesAnio = char.ToUpper(mesAnio[0], CulturaDo) + mesAnio[1..];

            var header = new Panel
            {
                BackColor = bgHeader,
                Dock = DockStyle.Top,
                Height = 78,
                Padding = new Padding(14, 10, 14, 8)
            };

            // Acento lateral
            var accentBar = new Panel
            {
                BackColor = accent,
                Dock = DockStyle.Left,
                Width = 4
            };
            header.Controls.Add(accentBar);

            var lblDiaSemana = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(18, 8),
                Text = diaSemana.ToUpper(CulturaDo)
            };

            var lblDia = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(14, 28),
                Text = ahora.Day.ToString(CulturaDo)
            };

            var lblMes = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = muted,
                Location = new Point(78, 42),
                Text = mesAnio
            };

            header.Controls.Add(lblDiaSemana);
            header.Controls.Add(lblDia);
            header.Controls.Add(lblMes);

            var body = new Panel
            {
                BackColor = bgCard,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };

            var calHost = new Panel
            {
                BackColor = Color.FromArgb(236, 242, 248),
                Dock = DockStyle.Fill,
                Padding = new Padding(6)
            };

            var cal = new MonthCalendar
            {
                BackColor = Color.FromArgb(236, 242, 248),
                FirstDayOfWeek = Day.Monday,
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill,
                MaxSelectionCount = 1,
                ShowToday = true,
                ShowTodayCircle = true,
                TitleBackColor = accent,
                TitleForeColor = Color.White,
                TrailingForeColor = Color.FromArgb(150, 160, 175)
            };

            try
            {
                cal.SetDate(ahora.Date);
            }
            catch
            {
                // Ignorar si la fecha está fuera de rango del control.
            }

            // MonthCalendar no respeta bien Dock Fill; ajustar tamaño real.
            Size calPref = cal.PreferredSize;
            if (calPref.Width < 10 || calPref.Height < 10)
                calPref = new Size(248, 200);

            cal.Dock = DockStyle.None;
            cal.Location = new Point(6, 6);
            cal.Size = calPref;
            calHost.Controls.Add(cal);

            int bodyW = Math.Max(calPref.Width + 24, 268);
            int bodyH = calPref.Height + 24;
            int totalW = bodyW + 8;
            int totalH = 78 + bodyH + 4;

            panel.Size = new Size(totalW, totalH);
            body.Controls.Add(calHost);

            // Orden: Fill primero, luego Top (WinForms Z-order)
            panel.Controls.Add(body);
            panel.Controls.Add(header);

            var host = new ToolStripControlHost(panel)
            {
                AutoSize = false,
                Size = panel.Size,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _calendarioPopup?.Close();
            _calendarioPopup?.Dispose();

            _calendarioPopup = new ToolStripDropDown
            {
                AutoClose = true,
                DropShadowEnabled = true,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                BackColor = bgCard
            };
            _calendarioPopup.Items.Add(host);

            Point origen = btnCalendario.PointToScreen(new Point(0, btnCalendario.Height + 4));
            int x = origen.X + btnCalendario.Width - panel.Width;
            if (x < 8)
                x = 8;

            Rectangle pantalla = Screen.FromControl(btnCalendario).WorkingArea;
            if (origen.Y + panel.Height > pantalla.Bottom - 8)
                origen = new Point(origen.X, Math.Max(pantalla.Top + 8, btnCalendario.PointToScreen(Point.Empty).Y - panel.Height - 4));

            _calendarioPopup.Show(new Point(x, origen.Y));
        }

        private void ActualizarTextoBotonCalendario()
        {
            if (btnCalendario == null || btnCalendario.IsDisposed)
                return;

            DateTime ahora = TimeZoneHelper.NowDominicanRepublic();
            string mes = ahora.ToString("MMM", CulturaDo);
            if (!string.IsNullOrEmpty(mes))
                mes = char.ToUpper(mes[0], CulturaDo) + mes[1..].TrimEnd('.');

            btnCalendario.Text = $"📅  {ahora.Day} {mes}  ·  {ahora:yyyy}";
        }
    }
}
