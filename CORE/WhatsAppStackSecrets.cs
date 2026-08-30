using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CORE
{
    /// <summary>
    /// Config del stack WhatsApp (túnel + URL pública) fuera del repo.
    /// Ruta: %LocalAppData%\MFFITNESS\whatsapp.stack.config
    /// </summary>
    public static class WhatsAppStackSecrets
    {
        private static readonly object Sync = new();
        private static Dictionary<string, string>? _cache;
        private static DateTime _cacheFileUtc = DateTime.MinValue;

        public static string RutaArchivo =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MFFITNESS",
                "whatsapp.stack.config");

        public static string? Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            Dictionary<string, string> map = EnsureLoaded();
            return map.TryGetValue(key, out string? value) ? value : null;
        }

        public static string TunnelProvider =>
            (Get("TunnelProvider") ?? "Ngrok").Trim();

        public static string? NgrokDomain =>
            NullIfEmpty(Get("NgrokDomain"));

        public static string? CloudflaredToken =>
            NullIfEmpty(Get("CloudflaredToken"));

        public static string? PublicBaseUrlOverride =>
            NullIfEmpty(Get("WhatsAppPublicBaseUrl"));

        public static bool TieneUrlPublicaFija =>
            !string.IsNullOrWhiteSpace(PublicBaseUrlOverride);

        public static bool UsaCloudflared =>
            string.Equals(TunnelProvider, "Cloudflared", StringComparison.OrdinalIgnoreCase);

        public static bool UsaNgrok =>
            !UsaCloudflared;

        public static void EnsureFileExists()
        {
            string path = RutaArchivo;
            if (File.Exists(path))
                return;

            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, DefaultTemplate);
            InvalidateCache();
        }

        public static void SetValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            EnsureFileExists();
            string path = RutaArchivo;

            XDocument doc = File.Exists(path)
                ? XDocument.Load(path)
                : XDocument.Parse(DefaultTemplate);

            XElement? node = doc.Descendants("add")
                .FirstOrDefault(e => string.Equals(e.Attribute("key")?.Value, key, StringComparison.OrdinalIgnoreCase));

            if (node == null)
            {
                doc.Root?.Element("appSettings")?.Add(new XElement("add",
                    new XAttribute("key", key),
                    new XAttribute("value", value)));
            }
            else
            {
                node.SetAttributeValue("value", value);
            }

            doc.Save(path);
            InvalidateCache();
        }

        public static void InvalidateCache()
        {
            lock (Sync)
            {
                _cache = null;
                _cacheFileUtc = DateTime.MinValue;
            }
        }

        private const string DefaultTemplate =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <appSettings>
                <add key="WhatsAppPublicBaseUrl" value="" />
                <add key="NgrokDomain" value="" />
                <add key="TunnelProvider" value="Ngrok" />
                <add key="CloudflaredToken" value="" />
                <add key="WhatsAppMediaListenPort" value="5088" />
              </appSettings>
            </configuration>
            """;

        private static Dictionary<string, string> EnsureLoaded()
        {
            string path = RutaArchivo;
            DateTime fileUtc = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;

            if (_cache != null && fileUtc == _cacheFileUtc)
                return _cache;

            lock (Sync)
            {
                fileUtc = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
                if (_cache != null && fileUtc == _cacheFileUtc)
                    return _cache;

                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    if (File.Exists(path))
                    {
                        XDocument doc = XDocument.Load(path);
                        foreach (XElement add in doc.Descendants("add"))
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
                        $"WhatsAppStackSecrets: no se pudo leer {path}: {ex.Message}");
                }

                _cache = map;
                _cacheFileUtc = fileUtc;
                return _cache;
            }
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

}
