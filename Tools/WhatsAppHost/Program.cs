using BLL;
using CORE;
using System;
using System.IO;
using System.Threading;

namespace WhatsAppHost
{
    internal static class Program
    {
        private const string MutexName = @"Global\MFFITNESS_WhatsAppHost";

        private static int Main(string[] args)
        {
            bool once = TieneFlag(args, "--once");
            bool help = TieneFlag(args, "--help") || TieneFlag(args, "-h");
            bool mediaOnly = TieneFlag(args, "--media-only");

            if (help)
            {
                Console.WriteLine("""
                    MFFITNESS WhatsApp Host 24/7 (+ media local solo si no hay Supabase)

                    Uso:
                      WhatsAppHost.exe              Loop de automatizacion (recordatorios)
                      WhatsAppHost.exe --once       Una corrida y sale
                      WhatsAppHost.exe --media-only Solo Kestrel local :5088 (fallback sin Supabase)
                      WhatsAppHost.exe --help       Ayuda

                    Produccion: PDF via Supabase Storage (bucket FACTURAS).
                    Fallback: Kestrel + WhatsAppPublicBaseUrl (HTTPS publico).
                    Tunel Ngrok historico: archive\Start-MediaTunnel.ps1

                    Instalar como tarea Windows (admin):
                      powershell -ExecutionPolicy Bypass -File .\Install-WhatsAppHost.ps1
                    """);
                return 0;
            }

            using var mutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                Console.WriteLine($"[{Ahora()}] Ya hay otra instancia de WhatsAppHost en ejecucion. Saliendo.");
                return 2;
            }

            if (!TwilioSettings.WhatsAppHabilitado && !mediaOnly)
            {
                Console.WriteLine($"[{Ahora()}] TwilioWhatsAppEnabled=false. Nada que hacer.");
                return 0;
            }

            string? advertencia = TwilioSettings.ObtenerAdvertenciaConfiguracion();
            if (!string.IsNullOrWhiteSpace(advertencia))
                Console.WriteLine($"[{Ahora()}] AVISO: {advertencia}");

            Console.WriteLine($"[{Ahora()}] WhatsApp Host iniciado.");
            Console.WriteLine($"[{Ahora()}] Origen: {TwilioSettings.PhoneNumber}");
            Console.WriteLine($"[{Ahora()}] ContentSid: {TwilioSettings.ContentSidGenerico}");
            Console.WriteLine($"[{Ahora()}] Facturas: {FacturaStorage.CarpetaFacturas}");
            Console.WriteLine("Ctrl+C para detener.");
            Console.WriteLine();

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine($"[{Ahora()}] Deteniendo...");
            };

            // Kestrel/Ngrok solo si NO hay Supabase (PDF publico en bucket FACTURAS).
            bool usarMediaLocal = mediaOnly || !SupabaseSettings.Configurado;
            if (usarMediaLocal)
            {
                FacturaMediaServer.Start(cts.Token);
            }
            else
            {
                Console.WriteLine($"[{Ahora()}] Media: Supabase Storage ({SupabaseSettings.BucketFacturas}). Sin Kestrel/Ngrok.");
            }

            if (mediaOnly)
            {
                Console.WriteLine($"[{Ahora()}] Modo --media-only. Esperando peticiones...");
                try
                {
                    cts.Token.WaitHandle.WaitOne();
                }
                catch (ObjectDisposedException)
                {
                }

                Console.WriteLine($"[{Ahora()}] WhatsApp Host detenido.");
                return 0;
            }

            var deudaBll = new DeudaBLL();
            string logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);

            do
            {
                try
                {
                    Console.WriteLine($"[{Ahora()}] Ejecutando automatizaciones...");
                    int enviados = deudaBll.VerificarYEnviarNotificaciones();
                    string linea = $"[{Ahora()}] OK. Mensajes entregados en este ciclo: {enviados}";
                    Console.WriteLine(linea);
                    AppendLog(logDir, linea);
                }
                catch (Exception ex)
                {
                    string linea = $"[{Ahora()}] ERROR: {ex.Message}";
                    Console.WriteLine(linea);
                    AppendLog(logDir, linea);
                }

                if (once || cts.IsCancellationRequested)
                    break;

                int minutos = Math.Max(1, TwilioSettings.IntervaloAutomatizacionMinutos);
                Console.WriteLine($"[{Ahora()}] Esperando {minutos} minuto(s)...");
                try
                {
                    cts.Token.WaitHandle.WaitOne(TimeSpan.FromMinutes(minutos));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
            while (!cts.IsCancellationRequested);

            Console.WriteLine($"[{Ahora()}] WhatsApp Host detenido.");
            return 0;
        }

        private static bool TieneFlag(string[] args, string flag) =>
            Array.Exists(args, a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));

        private static string Ahora() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        private static void AppendLog(string logDir, string linea)
        {
            try
            {
                string path = Path.Combine(logDir, $"whatsapp-host-{DateTime.Today:yyyyMMdd}.log");
                File.AppendAllText(path, linea + Environment.NewLine);
            }
            catch
            {
                // No bloquear el host por fallos de log.
            }
        }
    }
}
