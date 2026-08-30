using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CORE
{
    /// <summary>
    /// PDFs temporales para envío manual desde el chat (Twilio media).
    /// </summary>
    public static partial class ChatMediaStorage
    {
        [GeneratedRegex(@"^chat_\d+_[a-f0-9]+\.pdf$", RegexOptions.IgnoreCase)]
        private static partial Regex NombreArchivoValido();

        public static string CarpetaChatMedia
        {
            get
            {
                string dir = Path.Combine(FacturaStorage.CarpetaRaizMffitness, "ChatMedia");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string CarpetaWwwrootChat
        {
            get
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "wwwroot", "chat");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string GuardarPdf(int clienteId, byte[] pdfBytes)
        {
            if (clienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(clienteId));
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new ArgumentException("PDF vacío.", nameof(pdfBytes));

            string fileName = $"chat_{clienteId}_{Guid.NewGuid():N}.pdf";
            string primaria = Path.Combine(CarpetaChatMedia, fileName);
            File.WriteAllBytes(primaria, pdfBytes);

            try
            {
                string espejo = Path.Combine(CarpetaWwwrootChat, fileName);
                File.Copy(primaria, espejo, overwrite: true);
            }
            catch
            {
                // Espejo opcional; Kestrel puede servir desde LocalAppData si se configura después.
            }

            return fileName;
        }

        public static string? ResolverRutaExistente(string fileName)
        {
            if (!EsNombreSeguro(fileName))
                return null;

            string primaria = Path.Combine(CarpetaChatMedia, fileName);
            if (File.Exists(primaria))
                return primaria;

            string espejo = Path.Combine(CarpetaWwwrootChat, fileName);
            if (File.Exists(espejo))
                return espejo;

            return null;
        }

        public static string? ConstruirMediaUrlPublica(string fileName)
        {
            if (!EsNombreSeguro(fileName))
                return null;

            if (SupabaseSettings.Configurado)
                return SupabaseSettings.ConstruirUrlPublicaObjeto($"chat/{fileName}");

            string baseUrl = TwilioSettings.PublicBaseUrl?.Trim().TrimEnd('/') ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            return $"{baseUrl}/media/chat/{fileName}";
        }

        public static bool EsNombreSeguro(string? fileName) =>
            !string.IsNullOrWhiteSpace(fileName) && NombreArchivoValido().IsMatch(fileName);
    }
}
