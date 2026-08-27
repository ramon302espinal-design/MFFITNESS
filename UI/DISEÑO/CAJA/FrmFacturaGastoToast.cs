using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    /// <summary>
    /// Toast esquina superior derecha: éxito / error de factura automática.
    /// Se cierra solo a los 20 s. No activa el foco de la app.
    /// </summary>
    public sealed class FrmFacturaGastoToast : Form
    {
        private readonly System.Windows.Forms.Timer _autoClose;
        private readonly Label _lblTitulo;
        private readonly Label _lblDetalle;
        private readonly Button _btnAccion;
        private readonly Action? _onVer;
        private readonly string? _errorDetail;
        private static readonly List<FrmFacturaGastoToast> Activos = new();
        private const int ToastWidth = 340;
        private const int ToastHeight = 108;
        private const int ScreenMargin = 12;
        private const int AutoCloseMs = 20_000;

        public FrmFacturaGastoToast(
            bool success,
            string message,
            string? errorDetail,
            Action? onVer)
        {
            _onVer = onVer;
            _errorDetail = errorDetail;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(ToastWidth, ToastHeight);
            BackColor = success ? Color.FromArgb(28, 48, 36) : Color.FromArgb(52, 28, 28);
            Padding = new Padding(12);

            _lblTitulo = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = success ? "Factura → Egreso" : "Error factura automática",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblDetalle = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(230, 230, 230),
                Text = message,
                TextAlign = ContentAlignment.TopLeft
            };

            _btnAccion = new Button
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = success ? Color.FromArgb(46, 125, 70) : Color.FromArgb(140, 50, 50),
                Text = success
                    ? (onVer != null ? "Ver" : "OK")
                    : "Ver error",
                Cursor = Cursors.Hand,
                Visible = !success || onVer != null
            };
            _btnAccion.FlatAppearance.BorderSize = 0;
            _btnAccion.Click += (_, _) =>
            {
                if (success)
                {
                    _onVer?.Invoke();
                    Close();
                }
                else
                {
                    string detail = string.IsNullOrWhiteSpace(_errorDetail)
                        ? message
                        : message + "\n\n" + _errorDetail;
                    MessageBox.Show(
                        detail,
                        "Error factura automática",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            };

            // Info-only (sin Ver): auto-cierre más corto.
            int closeMs = (success && onVer == null) ? 6_000 : AutoCloseMs;

            Controls.Add(_lblDetalle);
            Controls.Add(_btnAccion);
            Controls.Add(_lblTitulo);

            Paint += (_, e) =>
            {
                using var pen = new Pen(success ? Color.FromArgb(80, 180, 110) : Color.FromArgb(200, 90, 90), 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
            };

            _autoClose = new System.Windows.Forms.Timer { Interval = closeMs };
            _autoClose.Tick += (_, _) => Close();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return cp;
            }
        }

        public static void ShowSuccess(IWin32Window? owner, string message, Action? onVer)
        {
            ShowInternal(owner, success: true, message, errorDetail: null, onVer);
        }

        public static void ShowError(IWin32Window? owner, string message, string? errorDetail)
        {
            ShowInternal(owner, success: false, message, errorDetail, onVer: null);
        }

        private static void ShowInternal(
            IWin32Window? owner,
            bool success,
            string message,
            string? errorDetail,
            Action? onVer)
        {
            void ShowCore()
            {
                var toast = new FrmFacturaGastoToast(success, message, errorDetail, onVer);
                Activos.Add(toast);
                toast.FormClosed += (_, _) =>
                {
                    Activos.Remove(toast);
                    Relayout();
                };
                Relayout();
                toast._autoClose.Start();
                toast.Show(owner);
            }

            if (owner is Control c && c.InvokeRequired)
                c.BeginInvoke(ShowCore);
            else if (Application.OpenForms.Count > 0 && Application.OpenForms[0]!.InvokeRequired)
                Application.OpenForms[0]!.BeginInvoke(ShowCore);
            else
                ShowCore();
        }

        private static void Relayout()
        {
            Screen screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
            Rectangle wa = screen.WorkingArea;
            int y = wa.Top + ScreenMargin;
            for (int i = Activos.Count - 1; i >= 0; i--)
            {
                FrmFacturaGastoToast t = Activos[i];
                if (t.IsDisposed)
                    continue;
                t.Location = new Point(wa.Right - ToastWidth - ScreenMargin, y);
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
