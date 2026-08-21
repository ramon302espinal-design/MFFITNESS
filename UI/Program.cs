using BLL;
using CORE;
using CORE.Update;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using UI.DISEÑO;
using UI.Facturas;
using UI.Helpers;

namespace UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ConfigurarFormatoHora12();
            ApplicationConfiguration.Initialize();
            Application.AddMessageFilter(new EscapeInicioMessageFilter());
            FacturaMembresiaPdfService.ConfigurarLicencia();

            try
            {
                AppConfig.EnsureDatabaseLogged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message + Environment.NewLine + Environment.NewLine +
                    "Entorno: " + AppConfig.EnvironmentName + Environment.NewLine +
                    "Base: " + AppConfig.DatabaseName + Environment.NewLine + Environment.NewLine +
                    "Cambia el entorno con DOTNET_ENVIRONMENT / MFFITNESS_ENVIRONMENT " +
                    "(Development = MF_CYBER_DB_DEV, Production = [MF CYBER DB]).",
                    "Base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var startup = UpdateSessionGuard.Evaluate();
            if (startup.BlockStartup)
            {
                MessageBox.Show(
                    startup.Message,
                    "Actualización — recuperación requerida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!startup.SkipAutoMigrations)
            {
                var migracion = SchemaMigrationBLL.ApplyPending();
                if (!migracion.Success)
                {
                    MessageBox.Show(
                        migracion.Message,
                        "Migración de base de datos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            // Trabajo pesado fuera del hilo UI para no congelar el arranque.
            try
            {
                if (!CORE.SupabaseSettings.Configurado)
                    System.Threading.Tasks.Task.Run(() => WhatsAppMediaHostLauncher.EnsureRunning());
                else
                    System.Threading.Tasks.Task.Run(() => BLL.Facturas.FacturaSupabaseUploader.Warmup());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MediaHost/Supabase warmup: {ex.Message}");
            }

            try
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    try { new ReporteBLL().GenerarReporteAutomaticoDiario(); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Reporte automatico: {ex.Message}"); }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reporte automatico: {ex.Message}");
            }

            Application.Run(new FrmLogin());
        }

        /// <summary>
        /// Hora visible del sistema en 12 horas (AM/PM), manteniendo es-DO.
        /// </summary>
        private static void ConfigurarFormatoHora12()
        {
            var cultura = (CultureInfo)CultureInfo.GetCultureInfo("es-DO").Clone();
            cultura.DateTimeFormat.AMDesignator = "AM";
            cultura.DateTimeFormat.PMDesignator = "PM";
            cultura.DateTimeFormat.ShortTimePattern = "hh:mm tt";
            cultura.DateTimeFormat.LongTimePattern = "hh:mm:ss tt";

            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;
            Thread.CurrentThread.CurrentCulture = cultura;
            Thread.CurrentThread.CurrentUICulture = cultura;
        }
    }
}
