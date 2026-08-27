using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    /// <summary>
    /// Banner flotante TopMost a nivel aplicación (por encima de todos los módulos).
    /// Estados: Leyendo / Éxito / Error.
    /// </summary>
    public sealed class FrmFacturaGastoHud : Form
    {
        private static FrmFacturaGastoHud? _instance;
        private static readonly object Sync = new();

        private readonly Label _lblTitulo;
        private readonly Label _lblDetalle;
        private readonly Button _btnAccion;
        private readonly System.Windows.Forms.Timer _autoHide;
        private Action? _onVer;
        private string? _errorDetail;

        private FrmFacturaGastoHud()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(520, 72);
            BackColor = Color.FromArgb(20, 28, 40);
            Padding = new Padding(14, 10, 14, 10);

            _lblTitulo = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "FACTURA",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblDetalle = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(220, 230, 240),
                Text = string.Empty,
                TextAlign = ContentAlignment.TopLeft
            };

            _btnAccion = new Button
            {
                Dock = DockStyle.Right,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(50, 90, 140),
                Text = "Ver",
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnAccion.FlatAppearance.BorderSize = 0;
            _btnAccion.Click += (_, _) =>
            {
                if (_onVer != null)
                {
                    var cb = _onVer;
                    _onVer = null;
                    cb.Invoke();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(_errorDetail))
                {
                    MessageBox.Show(
                        _errorDetail,
                        "Error factura automática",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            };

            var body = new Panel { Dock = DockStyle.Fill };
            body.Controls.Add(_lblDetalle);
            body.Controls.Add(_btnAccion);

            Controls.Add(body);
            Controls.Add(_lblTitulo);

            Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(90, 160, 220), 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
            };

            _autoHide = new System.Windows.Forms.Timer();
            _autoHide.Tick += (_, _) =>
            {
                _autoHide.Stop();
                Hide();
            };

            PositionBottomRight();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                return cp;
            }
        }

        public static void EnsureCreated()
        {
            RunOnUi(() =>
            {
                lock (Sync)
                {
                    if (_instance == null || _instance.IsDisposed)
                        _instance = new FrmFacturaGastoHud();
                }
            });
        }

        public static void ShowLeyendo(string fileName)
        {
            RunOnUi(() =>
            {
                var hud = Instance();
                hud.ApplyState(
                    Color.FromArgb(18, 48, 78),
                    Color.FromArgb(80, 170, 255),
                    "LEYENDO FACTURA",
                    string.IsNullOrWhiteSpace(fileName) ? "Analizando con IA…" : fileName,
                    showAction: false,
                    onVer: null,
                    errorDetail: null,
                    autoHideMs: 0);
                hud.ShowHud();
            });
        }

        public static void ShowExito(string message, Action? onVer)
        {
            RunOnUi(() =>
            {
                var hud = Instance();
                hud.ApplyState(
                    Color.FromArgb(18, 52, 32),
                    Color.FromArgb(70, 190, 110),
                    "SE LEYÓ CON ÉXITO",
                    message,
                    showAction: onVer != null,
                    onVer: onVer,
                    errorDetail: null,
                    autoHideMs: 20_000);
                hud.ShowHud();
            });
        }

        public static void ShowError(string message, string? detail)
        {
            RunOnUi(() =>
            {
                var hud = Instance();
                hud.ApplyState(
                    Color.FromArgb(60, 22, 22),
                    Color.FromArgb(220, 90, 90),
                    "ERROR AL LEER FACTURA",
                    message,
                    showAction: !string.IsNullOrWhiteSpace(detail),
                    onVer: null,
                    errorDetail: string.IsNullOrWhiteSpace(detail)
                        ? message
                        : message + "\n\n" + detail,
                    autoHideMs: 25_000);
                hud._btnAccion.Text = "Ver error";
                hud.ShowHud();
            });
        }

        public static void ShowVigilando(string folderPath)
        {
            RunOnUi(() =>
            {
                var hud = Instance();
                hud.ApplyState(
                    Color.FromArgb(28, 32, 40),
                    Color.FromArgb(120, 140, 160),
                    "VIGILANDO FACTURAS",
                    folderPath,
                    showAction: false,
                    onVer: null,
                    errorDetail: null,
                    autoHideMs: 5_000);
                hud.ShowHud();
            });
        }

        private void ApplyState(
            Color back,
            Color borderAccent,
            string title,
            string detail,
            bool showAction,
            Action? onVer,
            string? errorDetail,
            int autoHideMs)
        {
            _autoHide.Stop();
            BackColor = back;
            _lblTitulo.Text = title;
            _lblDetalle.Text = detail;
            _onVer = onVer;
            _errorDetail = errorDetail;
            _btnAccion.Visible = showAction;
            _btnAccion.Text = onVer != null ? "Ver" : "Ver error";
            _btnAccion.BackColor = borderAccent;

            Tag = borderAccent;
            Invalidate();

            if (autoHideMs > 0)
            {
                _autoHide.Interval = autoHideMs;
                _autoHide.Start();
            }
        }

        private void ShowHud()
        {
            PositionBottomRight();
            if (!Visible)
                Show();
            else
            {
                TopMost = true;
                BringToFront();
            }
        }

        private void PositionBottomRight()
        {
            Screen screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
            Rectangle wa = screen.WorkingArea;
            Location = new Point(
                wa.Right - Width - 14,
                wa.Bottom - Height - 14);
        }

        private static FrmFacturaGastoHud Instance()
        {
            lock (Sync)
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new FrmFacturaGastoHud();
                return _instance;
            }
        }

        private static void RunOnUi(Action action)
        {
            try
            {
                Form? ui = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
                if (ui != null && !ui.IsDisposed && ui.IsHandleCreated && ui.InvokeRequired)
                    ui.BeginInvoke(action);
                else
                    action();
            }
            catch
            {
                try { action(); } catch { /* ignore */ }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color accent = Tag is Color c ? c : Color.FromArgb(90, 160, 220);
            using var pen = new Pen(accent, 2);
            e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoHide.Stop();
                _autoHide.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
