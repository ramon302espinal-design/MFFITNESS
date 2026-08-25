using CORE;
using CORE.Ollama;
using DL;

namespace BLL.Services
{
    /// <summary>
    /// Sugiere datos de producto con Ollama llava:7b. No crea productos ni mueve stock.
    /// </summary>
    public sealed class ProductoVisionService
    {
        private readonly OllamaClient _client;
        private readonly ProductoVisionAuditoriaDAL _auditoria = new();

        public ProductoVisionService(OllamaClient? client = null)
        {
            _client = client ?? new OllamaClient();
        }

        public async Task EnsureOllamaReadyAsync(CancellationToken ct = default)
        {
            var (available, hasModel) = await _client.CheckAsync(OllamaOptions.VisionModel, ct)
                .ConfigureAwait(false);

            if (!available)
                throw new InvalidOperationException(
                    "Ollama no está disponible. Abre Ollama en esta PC (http://127.0.0.1:11434).");

            if (!hasModel)
                throw new InvalidOperationException(
                    $"El modelo '{OllamaOptions.VisionModel}' no está instalado. Ejecuta: ollama pull {OllamaOptions.VisionModel}");
        }

        public async Task<ProductoVisionSuggestion> AnalizarProductoAsync(
            byte[] imageBytes,
            IReadOnlyList<string>? categoriasCatalogo = null,
            CancellationToken ct = default)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Imagen vacía.", nameof(imageBytes));

            await EnsureOllamaReadyAsync(ct).ConfigureAwait(false);

            string b64 = Convert.ToBase64String(imageBytes);
            string cats = categoriasCatalogo is { Count: > 0 }
                ? string.Join(", ", categoriasCatalogo.Where(c => !string.IsNullOrWhiteSpace(c)).Take(20))
                : "Suplementos, Bebidas, Accesorios, Ropa, Snacks, Otro";

            // OCR explícito: marca + línea + variante (texto grande y pequeño del empaque).
            string prompt =
                "Lee el empaque con atención: captura texto GRANDE y también letras más pequeñas " +
                "(marca, línea, sabor/variante, tipo de producto). " +
                "En \"nombre\" escribe el nombre comercial COMPLETO en una sola frase, " +
                "ej.: \"Nature Valley Fruit & Nut Trail Mix Chewy Granola Bars\". " +
                "No uses solo el título más grande. Ignora conteos (15 BARS), QR, Box Tops y legales. " +
                "Categoría: una de [" + cats + "]. " +
                "Responde SOLO JSON sin markdown: " +
                "{\"nombre\":\"...\",\"categoria\":\"...\",\"descripcion\":\"breve\"," +
                "\"precioCompra\":0,\"precioVenta\":0}.";

            string response = await _client.GenerateWithImagesAsync(
                OllamaOptions.VisionModel,
                prompt,
                new[] { b64 },
                jsonFormat: true,
                ct).ConfigureAwait(false);

            ProductoVisionSuggestion suggestion = ProductoVisionSuggestion.TryParse(response)
                   ?? new ProductoVisionSuggestion { RawResponse = response };

            try
            {
                _auditoria.Registrar(
                    Sesion.Usuario,
                    OllamaOptions.VisionModel,
                    suggestion.Nombre,
                    suggestion.Categoria,
                    suggestion.PrecioCompraEstimado,
                    suggestion.PrecioVentaEstimado,
                    suggestion.RawResponse);
            }
            catch
            {
                // Auditoría best-effort.
            }

            return suggestion;
        }
    }
}
