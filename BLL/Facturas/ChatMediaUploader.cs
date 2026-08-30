using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CORE;
using Supabase.Storage;
using Supabase.Storage.Interfaces;

namespace BLL.Facturas
{
    /// <summary>
    /// Publica PDFs del chat en Supabase (bucket FACTURAS / carpeta chat/).
    /// </summary>
    public static class ChatMediaUploader
    {
        public static string? PublicarPdf(int clienteId, byte[] pdfBytes)
        {
            if (!SupabaseSettings.Configurado || clienteId <= 0 || pdfBytes == null || pdfBytes.Length == 0)
                return null;

            try
            {
                return UploadAsync(clienteId, pdfBytes).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Supabase] Chat PDF cliente={clienteId}: {ex.Message}");
                return null;
            }
        }

        private static async Task<string?> UploadAsync(int clienteId, byte[] pdfBytes)
        {
            string fileName = $"chat_{clienteId}_{Guid.NewGuid():N}.pdf";
            string objectPath = $"chat/{fileName}";

            var client = await FacturaSupabaseUploader.ObtenerClienteStorageAsync().ConfigureAwait(false);
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
    }
}
