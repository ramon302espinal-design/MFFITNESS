using System.Drawing;
using System.Windows.Forms;

namespace UI.Helpers
{
    /// <summary>
    /// Toast verde esquina superior izquierda cuando se activa una membresía programada.
    /// Una instancia por programación; no roba foco.
    /// </summary>
    public sealed class FrmProgramacionActivadaToast : Form
    {
        private readonly System.Windows.Forms.Timer _autoClose;
        private static readonly List<FrmProgramacionActivadaToast> Activos = new();
        private static readonly HashSet<int> ProgramacionesMostradas = new();
        private const int ToastWidth = 420;
        private const int ToastHeight = 72;
        private const int ScreenMargin = 12;
        private const int AutoCloseMs = 8_000;

        private FrmProgramacionActivadaToast(string mensaje)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(ToastWidth, ToastHeight);
            BackColor = Color.FromArgb(22, 163, 74);

            var lbl = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Padding = new Padding(14, 10, 14, 10),
                Text = mensaje,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lbl);

            Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(180, 255, 200), 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
            };

            _autoClose = new System.Windows.Forms.Timer { Interval = AutoCloseMs };
            _autoClose.Tick += (_, _) => Close();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000;
                cp.ExStyle |= 0x00000008;
                return cp;
            }
        }

        public static void MostrarUnaVez(int programacionId, string planNombre, string clienteNombre)
        {
            lock (ProgramacionesMostradas)
            {
                if (!ProgramacionesMostradas.Add(programacionId))
                    return;
            }

            string plan = string.IsNullOrWhiteSpace(planNombre) ? "PLAN" : planNombre.Trim().ToUpperInvariant();
            string cliente = string.IsNullOrWhiteSpace(clienteNombre) ? "MIEMBRO" : clienteNombre.Trim().ToUpperInvariant();
            string mensaje = $"SE ACTIVO LA MEMBRESIA {plan} DE {cliente}";

            void ShowCore()
            {
                var toast = new FrmProgramacionActivadaToast(mensaje);
                Activos.Add(toast);
                toast.FormClosed += (_, _) =>
                {
                    Activos.Remove(toast);
                    Relayout();
                };
                Relayout();
                toast._autoClose.Start();
                toast.Show();
            }

            if (Application.OpenForms.Count > 0 && Application.OpenForms[0]!.InvokeRequired)
                Application.OpenForms[0]!.BeginInvoke(ShowCore);
            else
                ShowCore();
        }

        private static void Relayout()
        {
            Screen screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
            Rectangle wa = screen.WorkingArea;
            int y = wa.Top + ScreenMargin;
            foreach (FrmProgramacionActivadaToast t in Activos.ToArray())
            {
                if (t.IsDisposed)
                    continue;
                t.Location = new Point(wa.Left + ScreenMargin, y);
                y += ToastHeight + ScreenMargin;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoClose.Stop();
                _autoClose.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
