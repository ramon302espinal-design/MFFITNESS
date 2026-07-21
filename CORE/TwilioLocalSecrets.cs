using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace CORE
{
    /// <summary>
    /// Credenciales Twilio fuera del repo: env vars o archivo local.
    /// Ruta: %LocalAppData%\MFFITNESS\twilio.secrets.config
    /// </summary>
    internal static class TwilioLocalSecrets
    {
        private static readonly object Sync = new();
        private static Dictionary<string, string>? _cache;

        public static string? Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var map = EnsureLoaded();
            return map.TryGetValue(key, out string? value) ? value : null;
        }

        public static string RutaArchivoSecretos =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MFFITNESS",
                "twilio.secrets.config");

        private static Dictionary<string, string> EnsureLoaded()
        {
            if (_cache != null)
                return _cache;

            lock (Sync)
            {
                if (_cache != null)
                    return _cache;

                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string path = RutaArchivoSecretos;

                try
                {
                    if (File.Exists(path))
                    {
                        XDocument doc = XDocument.Load(path);
                        foreach (var add in doc.Descendants("add"))
                        {
                            string? k = add.Attribute("key")?.Value;
                            string? v = add.Attribute("value")?.Value;
                            if (!string.IsNullOrWhiteSpace(k) && v != null)
                                map[k] = v;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"TwilioLocalSecrets: no se pudo leer {path}: {ex.Message}");
                }

                _cache = map;
                return _cache;
            }
        }
    }
}
