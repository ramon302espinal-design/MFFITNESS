using System;
using System.Diagnostics;
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
    }
}
