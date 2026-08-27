using CORE;
using CORE.Ollama;
using DL;

namespace BLL.Services
{
    /// <summary>
    /// Sugiere datos de producto con Ollama qwen2.5vl:7b (OCR de empaque).
    /// No crea productos ni mueve stock/caja.
    /// </summary>
    public sealed class ProductoVisionService
    {
        private static readonly object ReadySync = new();
        private static bool _ollamaReadyCached;
        private static DateTime _ollamaReadyUtc = DateTime.MinValue;

        private readonly OllamaClient _client;
        private readonly ProductoVisionAuditoriaDAL _auditoria = new();

        public ProductoVisionService(OllamaClient? client = null)
        {
            _client = client ?? new OllamaClient();
        }

        /// <summary>Precalienta Ollama en segundo plano (form Load).</summary>
        public static async Task WarmUpAsync(CancellationToken ct = default)
        {
            try
            {
                AppConfig.LoadOllamaOptions();
                var svc = new ProductoVisionService();
                await svc.EnsureOllamaReadyAsync(ct).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort: el usuario verá el error al capturar foto.
            }
        }

        public async Task EnsureOllamaReadyAsync(CancellationToken ct = default)
        {
            lock (ReadySync)
            {
                if (_ollamaReadyCached && (DateTime.UtcNow - _ollamaReadyUtc).TotalMinutes < 15)
                    return;
            }

            var (available, hasModel) = await _client.CheckAsync(OllamaOptions.VisionModel, ct)
                .ConfigureAwait(false);

            if (!available)
                throw new InvalidOperationException(
                    "Ollama no está disponible. Abre Ollama en esta PC (http://127.0.0.1:11434).");

            if (!hasModel)
                throw new InvalidOperationException(
                    $"El modelo '{OllamaOptions.VisionModel}' no está instalado. Ejecuta:\n" +
                    $"ollama pull {OllamaOptions.VisionModel}");

            lock (ReadySync)
            {
                _ollamaReadyCached = true;
                _ollamaReadyUtc = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// OCR de factura/recibo → concepto + monto. No escribe en caja ni auditoría de producto.
        /// </summary>
        public async Task<FacturaVisionSuggestion> AnalizarFacturaGastoAsync(
            byte[] imageBytes,
            CancellationToken ct = default)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Imagen vacía.", nameof(imageBytes));

            // Límite seguro: evita payloads enormes al modelo local.
            const int maxBytes = 3_500_000;
            if (imageBytes.Length > maxBytes)
                throw new InvalidOperationException(
                    "La imagen es demasiado grande. Usa una foto más liviana (JPG/PNG).");

            await EnsureOllamaReadyAsync(ct).ConfigureAwait(false);

            const string prompt =
                "OCR de factura/recibo de gasto (República Dominicana). Reglas estrictas:\n" +
                "1) comercio = título comercial / razón social del establecimiento.\n" +
                "2) lineas = cada producto/servicio visible: descripcion, cantidad, precio unitario, subtotal de esa línea.\n" +
                "3) concepto = texto multilínea listo para caja: primera línea = comercio; " +
                "luego cada ítem como '- desc xCANT @ PRECIO = SUBTOTAL'. Incluye lo legible; máx ~15 ítems.\n" +
                "4) monto = ÚNICAMENTE el TOTAL / TOTAL A PAGAR / TOTAL GENERAL (con ITBIS si aplica). " +
                "NUNCA uses subtotal de una línea, ni ITBIS solo, ni propina sola si hay total mayor.\n" +
                "5) Si no lees con confianza: concepto=\"\", lineas=[], monto=0.\n" +
                "JSON únicamente: " +
                "{\"comercio\":\"...\",\"lineas\":[{\"descripcion\":\"...\",\"cantidad\":1,\"precio\":0,\"subtotal\":0}]," +
                "\"concepto\":\"...\",\"monto\":0}";

            string b64 = Convert.ToBase64String(imageBytes);
            string response = await _client.GenerateWithImagesAsync(
                OllamaOptions.VisionModel,
                prompt,
                new[] { b64 },
                jsonFormat: true,
                numPredict: 480,
                ct: ct).ConfigureAwait(false);

            return FacturaVisionSuggestion.TryParse(response)
                   ?? new FacturaVisionSuggestion { RawResponse = response };
        }

        /// <summary>OCR del frente del empaque (nombre, categoría, precios).</summary>
        public Task<ProductoVisionSuggestion> AnalizarNombreEmpaqueAsync(
            byte[] imageBytes,
            IReadOnlyList<string>? categoriasCatalogo = null,
            CancellationToken ct = default) =>
            AnalizarConPromptAsync(
                imageBytes,
                ConstruirPromptNombre(categoriasCatalogo),
                numPredict: 220,
                ct);

        /// <summary>Fallback IA si ZXing no leyó el código en foto.</summary>
        public Task<string?> LeerCodigoBarraImagenAsync(byte[] imageBytes, CancellationToken ct = default) =>
            AnalizarCodigoBarraInternoAsync(imageBytes, ct);

        [Obsolete("Usar AnalizarNombreEmpaqueAsync")]
        public Task<ProductoVisionSuggestion> AnalizarProductoAsync(
            byte[] imageBytes,
            IReadOnlyList<string>? categoriasCatalogo = null,
            CancellationToken ct = default) =>
            AnalizarNombreEmpaqueAsync(imageBytes, categoriasCatalogo, ct);

        private async Task<ProductoVisionSuggestion> AnalizarConPromptAsync(
            byte[] imageBytes,
            string prompt,
            int numPredict,
            CancellationToken ct)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Imagen vacía.", nameof(imageBytes));

            await EnsureOllamaReadyAsync(ct).ConfigureAwait(false);

            string b64 = Convert.ToBase64String(imageBytes);
            string response = await _client.GenerateWithImagesAsync(
                OllamaOptions.VisionModel,
                prompt,
                new[] { b64 },
                jsonFormat: true,
                numPredict: numPredict,
                ct: ct).ConfigureAwait(false);

            ProductoVisionSuggestion suggestion = ProductoVisionSuggestion.TryParse(response)
                   ?? new ProductoVisionSuggestion { RawResponse = response };

            RegistrarAuditoria(suggestion);
            return suggestion;
        }

        private async Task<string?> AnalizarCodigoBarraInternoAsync(byte[] imageBytes, CancellationToken ct)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            await EnsureOllamaReadyAsync(ct).ConfigureAwait(false);

            const string prompt =
                "Lee SOLO el código de barras numérico (EAN/UPC) visible en la imagen. " +
                "Responde JSON: {\"codigoBarra\":\"digitos\"}. Si no hay código legible: {\"codigoBarra\":\"\"}.";

            string b64 = Convert.ToBase64String(imageBytes);
            string response = await _client.GenerateWithImagesAsync(
                OllamaOptions.VisionModel,
                prompt,
                new[] { b64 },
                jsonFormat: true,
                numPredict: 48,
                ct: ct).ConfigureAwait(false);

            return ProductoVisionSuggestion.TryParseCodigoBarra(response);
        }

        private void RegistrarAuditoria(ProductoVisionSuggestion suggestion)
        {
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
        }

        private static string ConstruirPromptNombre(IReadOnlyList<string>? categoriasCatalogo)
        {
            string cats = categoriasCatalogo is { Count: > 0 }
                ? string.Join(", ", categoriasCatalogo.Where(c => !string.IsNullOrWhiteSpace(c)).Take(24))
                : "Suplementos, Bebidas, Accesorios, Ropa, Snacks, Otro";

            return
                "OCR de empaque: lee marca + nombre comercial + variante. " +
                "Ignora código de barras, QR, conteos (15 BARS), ingredientes legales. " +
                "nombre = línea comercial completa en una sola línea. " +
                "Si no ves precio, 0. categoria: una de [" + cats + "]. " +
                "JSON únicamente: {\"nombre\":\"...\",\"categoria\":\"...\",\"descripcion\":\"breve\"," +
                "\"precioCompra\":0,\"precioVenta\":0}";
        }
    }
}
