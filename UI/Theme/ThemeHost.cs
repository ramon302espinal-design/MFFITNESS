using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Aplica el tema en runtime sin herencia (compatible con el diseñador WinForms).
    /// </summary>
    public static class ThemeHost
    {
        public static bool IsDesignTime()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().ProcessName
                    .Contains("DesignToolsServer", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enlaza el Load del formulario para aplicar tema solo en ejecución.
        /// Seguro llamar varias veces; no hace nada en el diseñador.
        /// </summary>
        public static void Attach(Form form, Action? customTheme = null)
        {
            if (form == null || IsDesignTime())
                return;

            void Handler(object? sender, EventArgs e)
            {
                form.Load -= Handler;
                if (form.IsDisposed || IsDesignTime())
                    return;

                try
                {
                    if (customTheme != null)
                        customTheme();
                    else
                        ThemeApplier.ApplyToForm(form);

                    // La hora en 12 horas no depende del tema: aplica también a pantallas clásicas.
                    Formato12Horas.AplicarAFormulario(form);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"ThemeHost.Attach ({form.GetType().Name}): {ex.Message}");
                }
            }

            form.Load += Handler;
        }
    }
}
