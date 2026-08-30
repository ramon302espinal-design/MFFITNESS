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
        private static int _stackApagado;

        [STAThread]
        static void Main()
        {
            ConfigurarFormatoHora12();
            ApplicationConfiguration.Initialize();
            ModuloAtajosTeclado.AsegurarFiltroGlobal();
            Application.AddMessageFilter(new EscapeInicioMessageFilter());
            FacturaMembresiaPdfService.ConfigurarLicencia();
            RegistrarApagadoStackWhatsApp();

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
                    "Cambia el entorno con MFFITNESS_ENVIRONMENT / DOTNET_ENVIRONMENT, " +
                    "o Database:DefaultEnvironment en appsettings.Local.json " +
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

            // WhatsApp: Kestrel + túnel ngrok (webhook inbound) siempre que WhatsApp esté activo.
            try
            {
                if (CORE.TwilioSettings.WhatsAppHabilitado)
                    System.Threading.Tasks.Task.Run(() => BLL.WhatsAppStackBootstrapper.EnsureReady(tryLaunchNgrok: true));
                else if (!CORE.SupabaseSettings.Configurado)
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

            StartUpdateExitWatcher();
            Application.Run(new FrmLogin());
        }

        /// <summary>Al salir de MFFITNESS, apaga ngrok y WhatsAppHost.</summary>
        private static void RegistrarApagadoStackWhatsApp()
        {
            Application.ApplicationExit += (_, _) => ApagarStackWhatsApp();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => ApagarStackWhatsApp();
        }

        private static void ApagarStackWhatsApp()
        {
            if (Interlocked.Exchange(ref _stackApagado, 1) != 0)
                return;

            try
            {
                ChatNotificationHost.Stop();
                WhatsAppStackBootstrapper.Shutdown();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApagarStackWhatsApp: {ex.Message}");
            }
        }

        /// <summary>
        /// UpdateManager puede pedir salida total vía EventWaitHandle (OTA).
        /// Cierra el proceso aunque FrmLogin esté oculto.
        /// </summary>
        private static void StartUpdateExitWatcher()
        {
            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        using var exitEvent = new EventWaitHandle(
                            false,
                            EventResetMode.AutoReset,
                            UpdateExitSignal.EventName);

                        while (true)
                        {
                            exitEvent.WaitOne();
                            UpdateExitSignal.ForceExitRequested = true;
                            try
                            {
                                if (Application.MessageLoop)
                                    Application.Exit();
                            }
                            catch { /* ignore */ }

                            try { Environment.Exit(0); }
                            catch { /* ignore */ }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdateExitWatcher: {ex.Message}");
                    }
                })
                {
                    IsBackground = true,
                    Name = "MFFITNESS-UpdateExitWatcher"
                };
                thread.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateExitWatcher start: {ex.Message}");
            }
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
