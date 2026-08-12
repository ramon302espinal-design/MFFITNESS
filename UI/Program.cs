using BLL;
using CORE.Update;
using System;
using System.Windows.Forms;
using UI.DISEÑO;
using UI.Facturas;

namespace UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            FacturaMembresiaPdfService.ConfigurarLicencia();

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
    }
}
