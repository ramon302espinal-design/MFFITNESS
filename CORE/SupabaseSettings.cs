using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace CORE
{
    /// <summary>
    /// Credenciales Supabase fuera del repo.
    /// Archivo: %LocalAppData%\MFFITNESS\supabase.secrets.config
    /// Env: SUPABASE_URL / SUPABASE_KEY (o SUPABASE_PUBLISHABLE_KEY)
    /// </summary>
    public static class SupabaseSettings
    {
        private static readonly object Sync = new();
        private static Dictionary<string, string>? _cache;

        public static bool Habilitado =>
            !bool.TryParse(ConfigurationManager.AppSettings["SupabaseStorageEnabled"], out bool on) || on;

        public static string Url
        {
            get
            {
                string? env = Environment.GetEnvironmentVariable("SUPABASE_URL");
                if (!string.IsNullOrWhiteSpace(env))
                    return env.Trim().TrimEnd('/');

                string? local = GetSecret("SupabaseUrl") ?? GetSecret("Url");
                if (!string.IsNullOrWhiteSpace(local))
                    return local.Trim().TrimEnd('/');

                return ConfigurationManager.AppSettings["SupabaseUrl"]?.Trim().TrimEnd('/')
                       ?? string.Empty;
            }
        }

        /// <summary>
        /// Publishable / anon key (bucket publico con policy de upload, o service_role).
        /// </summary>
        public static string Key
        {
            get
            {
                string? env = Environment.GetEnvironmentVariable("SUPABASE_KEY")
                              ?? Environment.GetEnvironmentVariable("SUPABASE_PUBLISHABLE_KEY")
                              ?? Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY");
                if (!string.IsNullOrWhiteSpace(env))
                    return env.Trim();

                string? local = GetSecret("SupabaseKey")
                                ?? GetSecret("SupabasePublishableKey")
                                ?? GetSecret("Key")
                                ?? GetSecret("AnonKey");
                if (!string.IsNullOrWhiteSpace(local))
                    return local.Trim();

                return ConfigurationManager.AppSettings["SupabaseKey"]?.Trim()
                       ?? string.Empty;
            }
        }

        public static string BucketFacturas =>
            string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SupabaseBucketFacturas"])
                ? "FACTURAS"
                : ConfigurationManager.AppSettings["SupabaseBucketFacturas"]!.Trim();

        public static bool Configurado =>
            Habilitado
            && !string.IsNullOrWhiteSpace(Url)
            && !string.IsNullOrWhiteSpace(Key);

        public static string RutaArchivoSecretos =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MFFITNESS",
                "supabase.secrets.config");

        public static string ConstruirUrlPublicaObjeto(string objectPath)
        {
            string path = (objectPath ?? string.Empty).Trim().TrimStart('/');
            return $"{Url.TrimEnd('/')}/storage/v1/object/public/{BucketFacturas}/{path}";
        }

        private static string? GetSecret(string key)
        {
            var map = EnsureLoaded();
            return map.TryGetValue(key, out string? value) ? value : null;
        }

        private static Dictionary<string, string> EnsureLoaded()
        {
            if (_cache != null)
                return _cache;

            lock (Sync)
            {
                if (_cache != null)
                    return _cache;

                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    if (File.Exists(RutaArchivoSecretos))
                    {
                        XDocument doc = XDocument.Load(RutaArchivoSecretos);
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
                        $"SupabaseSettings: no se pudo leer secretos: {ex.Message}");
                }

                _cache = map;
                return _cache;
            }
        }
    }
}
