using System.Windows.Forms;

namespace UI.Helpers
{
    /// <summary>
    /// WinForms: un Form con TopLevel=false y WindowState=Maximized (diseñador)
    /// suele quedar con área cliente 0 dentro de TabControl/paneles.
    /// </summary>
    internal static class EmbeddedFormHelper
    {
        public static void Preparar(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.WindowState = FormWindowState.Normal;
            form.StartPosition = FormStartPosition.Manual;
            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;
            form.Dock = DockStyle.Fill;
        }

        public static void MontarEn(Control host, Form form)
        {
            Preparar(form);
            host.Controls.Add(form);
            form.Show();
            form.BringToFront();
            form.PerformLayout();
            host.PerformLayout();
        }

        /// <summary>Si el form se creó embebido, corrige estado de ventana heredado del diseñador.</summary>
        public static void CorregirSiEmbebido(Form form)
        {
            if (form.TopLevel)
                return;

            form.WindowState = FormWindowState.Normal;
            form.StartPosition = FormStartPosition.Manual;
            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;
        }
    }
}
