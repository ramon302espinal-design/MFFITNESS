using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using CORE;

namespace BLL
{
    /// <summary>
    /// Arranca y detiene Kestrel (:5088) + ngrok vinculados al ciclo de vida de la app UI.
    /// </summary>
    public static class WhatsAppStackBootstrapper
    {
        private static readonly object Gate = new();
        private static Process? _ngrokProcess;
        private static int _shutdownDone;

        public sealed class BootstrapResult
        {
            public bool KestrelOk { get; init; }
            public bool WebhookConfigurado { get; init; }
            public string? PublicUrl { get; init; }
            public string Mensaje { get; init; } = string.Empty;
        }

        public static BootstrapResult EnsureReady(bool tryLaunchNgrok = false)
        {
            lock (Gate)
            {
                WhatsAppStackSecrets.EnsureFileExists();
                TryClearZombieHost();

                bool kestrel = WhatsAppMediaHostLauncher.EnsureRunning();
                if (!kestrel)
                {
                    return new BootstrapResult
                    {
                        KestrelOk = false,
                        Mensaje = "WhatsAppHost no responde en :5088. Compile Tools\\WhatsAppHost."
                    };
                }

                string? publicUrl = null;
                if (tryLaunchNgrok)
                    publicUrl = EnsureNgrokRunning();

                publicUrl ??= TryGetNgrokPublicUrl();

                if (!string.IsNullOrWhiteSpace(publicUrl))
                {
                    PersistPublicUrl(publicUrl);
                    return Ok(kestrel, publicUrl,
                        tryLaunchNgrok ? "Ngrok activo" : "URL ngrok detectada y guardada");
                }

                if (TwilioSettings.WebhookInboundConfigurado)
                {
                    return new BootstrapResult
                    {
                        KestrelOk = kestrel,
                        WebhookConfigurado = false,
                        PublicUrl = TwilioSettings.PublicBaseUrl,
                        Mensaje = "Kestrel OK. URL guardada pero ngrok no responde — reinicie la app o ngrok."
                    };
                }

                return new BootstrapResult
                {
                    KestrelOk = kestrel,
                    WebhookConfigurado = false,
                    Mensaje = tryLaunchNgrok
                        ? "Kestrel OK. Instale ngrok (ngrok config add-authtoken) o configure WhatsAppPublicBaseUrl."
                        : "Kestrel OK. Falta túnel HTTPS (ngrok) o WhatsAppPublicBaseUrl."
                };
            }
        }

        private static BootstrapResult Ok(bool kestrel, string? url, string msg) =>
            new()
            {
                KestrelOk = kestrel,
                WebhookConfigurado = true,
                PublicUrl = url?.TrimEnd('/'),
                Mensaje = msg
            };

        private static void PersistPublicUrl(string url)
        {
            url = url.Trim().TrimEnd('/');
            WhatsAppStackSecrets.SetValue("WhatsAppPublicBaseUrl", url);
            WhatsAppStackSecrets.InvalidateCache();
        }

        /// <summary>Arranca ngrok si no corre y devuelve la URL HTTPS pública.</summary>
        public static string? EnsureNgrokRunning()
        {
            string? url = TryGetNgrokPublicUrl();
            if (!string.IsNullOrWhiteSpace(url))
                return url;

            return TryLaunchNgrok(out url) ? url : null;
        }

        public static string? TryGetNgrokPublicUrl()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                string json = client.GetStringAsync("http://127.0.0.1:4040/api/tunnels").GetAwaiter().GetResult();
                using JsonDocument doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("tunnels", out JsonElement tunnels))
                    return null;

                foreach (JsonElement tunnel in tunnels.EnumerateArray())
                {
                    if (!tunnel.TryGetProperty("public_url", out JsonElement urlEl))
                        continue;

                    string? url = urlEl.GetString();
                    if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        return url.TrimEnd('/');
                }
            }
            catch
            {
                // ngrok no corre
            }

            return null;
        }

        private static bool TryLaunchNgrok(out string? publicUrl)
        {
            publicUrl = TryGetNgrokPublicUrl();
            if (!string.IsNullOrWhiteSpace(publicUrl))
                return true;

            string? ngrok = ResolverNgrokExe();
            if (ngrok == null)
                return false;

            try
            {
                if (NgrokYaCorriendo())
                {
                    int esperaHasta = Environment.TickCount + 20_000;
                    while (Environment.TickCount < esperaHasta)
                    {
                        Thread.Sleep(500);
                        publicUrl = TryGetNgrokPublicUrl();
                        if (!string.IsNullOrWhiteSpace(publicUrl))
                            return true;
                    }

                    return false;
                }

                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                {
                    publicUrl = TryGetNgrokPublicUrl();
                    return !string.IsNullOrWhiteSpace(publicUrl);
                }

                string? domain = WhatsAppStackSecrets.NgrokDomain;
                string args = string.IsNullOrWhiteSpace(domain)
                    ? "http 5088 --log=stdout"
                    : $"http --url={domain} 5088 --log=stdout";

                _ngrokProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = ngrok,
                    Arguments = args,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Minimized
                });

                int deadline = Environment.TickCount + 35_000;
                while (Environment.TickCount < deadline)
                {
                    Thread.Sleep(1000);
                    publicUrl = TryGetNgrokPublicUrl();
                    if (!string.IsNullOrWhiteSpace(publicUrl))
                        return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[WhatsAppStack] ngrok: {ex.Message}");
            }

            return false;
        }

        private static bool NgrokYaCorriendo()
        {
            try
            {
                return Process.GetProcessesByName("ngrok").Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static string? ResolverNgrokExe()
        {
            string baseDir = AppContext.BaseDirectory;
            string[] bundled =
            {
                Path.Combine(baseDir, "Tools", "ngrok", "ngrok.exe"),
                Path.Combine(baseDir, "ngrok", "ngrok.exe"),
                Path.Combine(baseDir, "ngrok.exe"),
                Path.Combine(baseDir, "WhatsAppHost", "ngrok.exe")
            };

            foreach (string candidate in bundled)
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            string? path = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(path))
            {
                foreach (string dir in path.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(dir))
                        continue;

                    string candidate = Path.Combine(dir.Trim(), "ngrok.exe");
                    if (File.Exists(candidate))
                        return candidate;
                }
            }

            string windowsApps = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "WindowsApps", "ngrok.exe");
            return File.Exists(windowsApps) ? windowsApps : null;
        }

        /// <summary>Si hay procesos WhatsAppHost pero :5088 no responde, intenta cerrarlos.</summary>
        private static void TryClearZombieHost()
        {
            if (WhatsAppMediaHostLauncher.EstaDisponible())
                return;

            Process[] procesos = Process.GetProcessesByName("WhatsAppHost");
            if (procesos.Length == 0)
                return;

            foreach (Process p in procesos)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit(4000);
                    }
                }
                catch
                {
                    // Sin permisos: el usuario debe cerrar desde Administrador de tareas.
                }
                finally
                {
                    p.Dispose();
                }
            }

            Thread.Sleep(1500);
        }

        /// <summary>Detiene ngrok y WhatsAppHost al cerrar la aplicación.</summary>
        public static void Shutdown()
        {
            if (Interlocked.Exchange(ref _shutdownDone, 1) != 0)
                return;

            lock (Gate)
            {
                DetenerProcesos("ngrok", waitMs: 3000);
                _ngrokProcess = null;
                DetenerProcesos("WhatsAppHost", waitMs: 4000);
                Trace.WriteLine("[WhatsAppStack] Stack detenido (ngrok + WhatsAppHost).");
            }
        }

        private static void DetenerProcesos(string nombre, int waitMs)
        {
            Process[] procesos;
            try
            {
                procesos = Process.GetProcessesByName(nombre);
            }
            catch
            {
                return;
            }

            foreach (Process p in procesos)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.Kill(entireProcessTree: true);
                        p.WaitForExit(waitMs);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[WhatsAppStack] No se pudo cerrar {nombre} ({p.Id}): {ex.Message}");
                }
                finally
                {
                    p.Dispose();
                }
            }
        }
    }
}
