using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CORE;
using Supabase.Storage;
using Supabase.Storage.Interfaces;

namespace BLL.Facturas
{
    /// <summary>
    /// Sube PDFs al bucket publico FACTURAS en Supabase Storage.
    /// </summary>
    public static class FacturaSupabaseUploader
    {
        private static readonly object Sync = new();
        private static Supabase.Client? _client;

        public static void Warmup()
        {
            if (!SupabaseSettings.Configurado)
                return;

            try
            {
                GetClientAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Supabase] Warmup: {ex.Message}");
            }
        }

        /// <summary>
        /// Devuelve la URL publica solo si el PDF esta realmente descargable por Twilio.
        /// Si el objeto no existe en el bucket (PDF generado antes, o subida fallida),
        /// lo vuelve a subir desde el archivo local. Evita el error 63019 (media 0 bytes).
        /// </summary>
        public static string? AsegurarPublicada(int pagoId)
        {
            if (!SupabaseSettings.Configurado || pagoId <= 0)
                return null;

            string url = SupabaseSettings.ConstruirUrlPublicaObjeto(
                FacturaStorage.NombreArchivoPago(pagoId));

            if (ObjetoDescargable(url))
                return url;

            string? rutaLocal = FacturaStorage.ResolverRutaFacturaExistente(pagoId);
            if (string.IsNullOrWhiteSpace(rutaLocal) || !File.Exists(rutaLocal))
            {
                Trace.WriteLine($"[Supabase] factura_{pagoId}: no publicada y sin archivo local.");
                return null;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(rutaLocal);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Supabase] factura_{pagoId}: no se pudo leer el PDF local: {ex.Message}");
                return null;
            }

            string? subida = TryUploadAndGetPublicUrl(pagoId, bytes);
            if (string.IsNullOrWhiteSpace(subida))
                return null;

            return ObjetoDescargable(subida) ? subida : null;
        }

        /// <summary>
        /// Twilio valida la media con GET/HEAD: se comprueba igual antes de enviar.
        /// </summary>
        private static bool ObjetoDescargable(string url)
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                using var resp = http.SendAsync(request).GetAwaiter().GetResult();

                if (!resp.IsSuccessStatusCode)
                    return false;

                long? largo = resp.Content.Headers.ContentLength;
                return !largo.HasValue || largo.Value > 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Supabase] Verificacion de media fallo: {ex.Message}");
                return false;
            }
        }

        public static string? TryUploadAndGetPublicUrl(int pagoId, byte[] pdfBytes)
        {
            if (!SupabaseSettings.Configurado || pagoId <= 0 || pdfBytes == null || pdfBytes.Length == 0)
                return null;

            try
            {
                return UploadAsync(pagoId, pdfBytes).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Supabase] Upload factura_{pagoId} fallo: {ex.Message}");
                return null;
            }
        }

        public static async Task<string?> UploadAsync(int pagoId, byte[] pdfBytes)
        {
            if (!SupabaseSettings.Configurado)
                return null;

            string objectPath = FacturaStorage.NombreArchivoPago(pagoId);
            var client = await GetClientAsync().ConfigureAwait(false);
            IStorageFileApi<FileObject> bucket = client.Storage.From(SupabaseSettings.BucketFacturas);

            var options = new Supabase.Storage.FileOptions
            {
                ContentType = "application/pdf",
                Upsert = true
            };

            await bucket.Upload(pdfBytes, objectPath, options).ConfigureAwait(false);

            string publicUrl = bucket.GetPublicUrl(objectPath);
            if (string.IsNullOrWhiteSpace(publicUrl))
                publicUrl = SupabaseSettings.ConstruirUrlPublicaObjeto(objectPath);

            return publicUrl;
        }

        private static async Task<Supabase.Client> GetClientAsync()
        {
            if (_client != null)
                return _client;

            lock (Sync)
            {
                if (_client != null)
                    return _client;

                var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = false
                };

                _client = new Supabase.Client(SupabaseSettings.Url, SupabaseSettings.Key, options);
            }

            await _client.InitializeAsync().ConfigureAwait(false);
            return _client;
        }

        internal static Task<Supabase.Client> ObtenerClienteStorageAsync() => GetClientAsync();
    }
}
