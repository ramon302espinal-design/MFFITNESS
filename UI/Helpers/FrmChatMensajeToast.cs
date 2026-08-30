using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BLL.Models;
using UI.DISEÑO.CHAT;

namespace UI.Helpers
{
    /// <summary>
    /// Toast flotante en la esquina superior derecha al recibir un mensaje WhatsApp entrante.
    /// </summary>
    public sealed class FrmChatMensajeToast : Form
    {
        private readonly System.Windows.Forms.Timer _autoClose;
        private readonly int _clienteId;
        private static readonly List<FrmChatMensajeToast> Activos = new();
        private const int ToastWidth = 420;
        private const int ToastHeight = 96;
        private const int StackGap = 10;
        private const int AutoCloseMs = 12_000;

        private FrmChatMensajeToast(ChatNotificacionDto notificacion)
        {
            _clienteId = notificacion.ClienteId;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(ToastWidth, ToastHeight);
            BackColor = Color.FromArgb(0, 128, 105);
            Cursor = Cursors.Hand;

            string nombre = string.IsNullOrWhiteSpace(notificacion.ClienteNombre)
                ? "Miembro"
                : notificacion.ClienteNombre.Trim();
            string preview = FormatearVistaPrevia(notificacion.Cuerpo);
            string hora = notificacion.Fecha.ToString("hh:mm tt");

            var lblIcono = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 16F),
                ForeColor = Color.White,
                Location = new Point(12, 28),
                Size = new Size(36, 36),
                Text = "💬",
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTitulo = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(54, 14),
                Size = new Size(ToastWidth - 120, 24),
                Text = "WhatsApp · " + nombre
            };

            var lblHora = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(210, 255, 245),
                Location = new Point(ToastWidth - 72, 16),
                Text = hora
            };

            var lblPreview = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(230, 255, 250),
                Location = new Point(54, 40),
                Size = new Size(ToastWidth - 66, 44),
                Text = preview
            };

            Controls.Add(lblIcono);
            Controls.Add(lblTitulo);
            Controls.Add(lblHora);
            Controls.Add(lblPreview);

            Click += AbrirChat;
            foreach (Control ctrl in Controls)
                ctrl.Click += AbrirChat;

            Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(160, 255, 230), 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
                using var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
                e.Graphics.FillRectangle(shadow, 4, Height - 2, Width - 8, 2);
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

        public static void Mostrar(ChatNotificacionDto notificacion)
        {
            void ShowCore()
            {
                var toast = new FrmChatMensajeToast(notificacion);
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

            Form? ui = Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);
            if (ui != null && ui.InvokeRequired)
                ui.BeginInvoke(ShowCore);
            else
                ShowCore();
        }

        private void AbrirChat(object? sender, EventArgs e)
        {
            Close();
            ModuloNavBar.AbrirChat(Form.ActiveForm, _clienteId);
        }

        private static string FormatearVistaPrevia(string? cuerpo)
        {
            if (string.IsNullOrWhiteSpace(cuerpo))
                return "Nuevo mensaje";

            string texto = cuerpo.Trim().Replace("\r\n", " ").Replace('\n', ' ');
            if (texto.StartsWith("[PDF", StringComparison.OrdinalIgnoreCase)
                || texto.Contains("[PDF enviado]", StringComparison.OrdinalIgnoreCase))
                return "📎 Documento PDF";

            const int max = 100;
            return texto.Length <= max ? texto : texto[..max] + "…";
        }

        /// <summary>Apila toasts en la esquina superior derecha de la pantalla principal.</summary>
        private static void Relayout()
        {
            Screen screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
            Rectangle wa = screen.WorkingArea;

            var vivos = Activos.Where(t => !t.IsDisposed).ToList();
            int x = wa.Right - ToastWidth - StackGap;
            int y = wa.Top + StackGap;

            foreach (FrmChatMensajeToast t in vivos)
            {
                t.Location = new Point(x, y);
                y += ToastHeight + StackGap;
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
