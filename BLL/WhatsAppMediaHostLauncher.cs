using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace BLL
{
    /// <summary>
    /// Fallback: asegura Kestrel (WhatsAppHost :5088) si Supabase no esta configurado.
    /// </summary>
    public static class WhatsAppMediaHostLauncher
    {
        private static readonly object Gate = new();

        public static bool EstaDisponible()
        {
            try
            {
                string baseUrl = (CORE.TwilioSettings.MediaListenUrl ?? "http://127.0.0.1:5088").TrimEnd('/');
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var resp = client.GetAsync(baseUrl + "/health").GetAwaiter().GetResult();
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Si /health falla, intenta arrancar Tools\WhatsAppHost --media-only y espera.
        /// </summary>
        public static bool EnsureRunning(int esperaSegundos = 12)
        {
            if (EstaDisponible())
                return true;

            lock (Gate)
            {
                if (EstaDisponible())
                    return true;

                string? exe = ResolverExe();
                if (exe == null)
                {
                    Trace.WriteLine("[MediaHost] No se encontro WhatsAppHost.exe");
                    return false;
                }

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = "--media-only",
                        WorkingDirectory = Path.GetDirectoryName(exe)!,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    Process.Start(psi);
                    Trace.WriteLine($"[MediaHost] Arrancado: {exe}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[MediaHost] No se pudo arrancar: {ex.Message}");
                    return false;
                }

                int deadline = Environment.TickCount + (esperaSegundos * 1000);
                while (Environment.TickCount < deadline)
                {
                    Thread.Sleep(500);
                    if (EstaDisponible())
                        return true;
                }

                return EstaDisponible();
            }
        }

        private static string? ResolverExe()
        {
            string baseDir = AppContext.BaseDirectory;
            string[] candidatos =
            {
                // UI Debug → repo\Tools\WhatsAppHost\bin\...
                Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\Tools\WhatsAppHost\bin\Debug\net10.0\WhatsAppHost.exe")),
                Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\Tools\WhatsAppHost\bin\Release\net10.0\WhatsAppHost.exe")),
                // Copia instalada junto a datos locales
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MFFITNESS", "WhatsAppHost", "WhatsAppHost.exe"),
            };

            foreach (string c in candidatos)
            {
                if (File.Exists(c))
                    return c;
            }

            return null;
        }
    }
}
