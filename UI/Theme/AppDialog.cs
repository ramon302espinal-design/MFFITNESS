using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI.Theme
{
    public enum DialogType { Info, Success, Warning, Error, Question }

    /// <summary>
    /// Diálogos modernos con color según tipo de mensaje.
    /// </summary>
    public static class AppDialog
    {
        public static void Info(IWin32Window? owner, string message, string title = "Información")
            => Show(owner, message, title, DialogType.Info);

        public static void Success(IWin32Window? owner, string message, string title = "Éxito")
            => Show(owner, message, title, DialogType.Success);

        public static void Warning(IWin32Window? owner, string message, string title = "Advertencia")
            => Show(owner, message, title, DialogType.Warning);

        public static void Error(IWin32Window? owner, string message, string title = "Error")
            => Show(owner, message, title, DialogType.Error);

        public static bool Confirm(IWin32Window? owner, string message, string title = "Confirmar")
            => ShowConfirm(owner, message, title);

        public static void Show(IWin32Window? owner, string message, string title, DialogType type)
        {
            using var dlg = CreateDialog(message, title, type, false);
            dlg.ShowDialog(owner);
        }

        public static bool ShowConfirm(IWin32Window? owner, string message, string title)
        {
            using var dlg = CreateDialog(message, title, DialogType.Question, true);
            return dlg.ShowDialog(owner) == DialogResult.Yes;
        }

        private static Form CreateDialog(string message, string title, DialogType type, bool confirm)
        {
            Color accent = type switch
            {
                DialogType.Success => AppTheme.Success,
                DialogType.Warning => AppTheme.Warning,
                DialogType.Error => AppTheme.Error,
                DialogType.Question => AppTheme.Primary,
                _ => AppTheme.Info
            };

            string icon = type switch
            {
                DialogType.Success => "✓",
                DialogType.Warning => "!",
                DialogType.Error => "✕",
                DialogType.Question => "?",
                _ => "i"
            };

            var form = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                BackColor = AppTheme.Surface,
                Font = AppTheme.FontBody,
                ClientSize = new Size(440, 220),
                Padding = new Padding(0)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = accent
            };

            var lblTitle = new Label
            {
                Text = $"  {icon}  {title}",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.TextOnPrimary,
                Font = AppTheme.FontSubtitle,
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblTitle);

            var lblMessage = new Label
            {
                Text = message,
                Location = new Point(24, 68),
                Size = new Size(392, 90),
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.FontBody
            };

            var btnOk = new Button
            {
                Text = confirm ? "Sí" : "Aceptar",
                DialogResult = confirm ? DialogResult.Yes : DialogResult.OK,
                Size = new Size(110, 36),
                Location = new Point(confirm ? 200 : 310, 168),
                FlatStyle = FlatStyle.Flat,
                BackColor = accent,
                ForeColor = AppTheme.TextOnPrimary,
                Font = AppTheme.FontButton,
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            ThemeApplier.ApplyRoundedRegion(btnOk, AppTheme.RadiusSmall);

            form.Controls.Add(header);
            form.Controls.Add(lblMessage);
            form.Controls.Add(btnOk);
            form.AcceptButton = btnOk;

            if (confirm)
            {
                var btnNo = new Button
                {
                    Text = "No",
                    DialogResult = DialogResult.No,
                    Size = new Size(110, 36),
                    Location = new Point(310, 168),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = AppTheme.SurfaceElevated,
                    ForeColor = AppTheme.TextPrimary,
                    Font = AppTheme.FontButton,
                    Cursor = Cursors.Hand
                };
                btnNo.FlatAppearance.BorderSize = 0;
                ThemeApplier.ApplyRoundedRegion(btnNo, AppTheme.RadiusSmall);
                form.Controls.Add(btnNo);
                form.CancelButton = btnNo;
            }

            return form;
        }
    }
}
