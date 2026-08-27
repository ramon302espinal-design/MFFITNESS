using CORE;
using CORE.Commands;
using CORE.Ollama;
using BLL.Commands;

namespace BLL.Services
{
    /// <summary>Resultado del procesamiento automático FacturaGastos → egreso.</summary>
    public sealed class FacturaGastoAutoResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? ErrorDetail { get; init; }
        public string? Concepto { get; init; }
        public decimal? Monto { get; init; }
        public int? MovimientoId { get; init; }
        public string? SourceFileName { get; init; }
        public string PipelineTrace { get; init; } = string.Empty;

        public static FacturaGastoAutoResult Ok(
            string message,
            string concepto,
            decimal monto,
            int movimientoId,
            string? fileName,
            string trace) =>
            new()
            {
                Success = true,
                Message = message,
                Concepto = concepto,
                Monto = monto,
                MovimientoId = movimientoId,
                SourceFileName = fileName,
                PipelineTrace = trace
            };

        public static FacturaGastoAutoResult Fail(
            string message,
            string? detail = null,
            string? fileName = null,
            string? trace = null) =>
            new()
            {
                Success = false,
                Message = message,
                ErrorDetail = detail,
                SourceFileName = fileName,
                PipelineTrace = trace ?? string.Empty
            };
    }

    /// <summary>
    /// Orquesta modelos Ollama del listado local:
    /// visión qwen2.5vl → fallback gemma3 → reparación qwen2.5-coder →
    /// validación llama3.1 → razonamiento deepseek-r1 (último recurso).
    /// Registra egreso solo si la lectura es útil y la caja está abierta.
    /// </summary>
    public sealed class FacturaGastoOrchestrator
    {
        private const string VisionPrompt =
            "OCR de factura/recibo de gasto (República Dominicana). Prioridad absoluta de campos:\n" +
            "A) TITULO COMERCIAL = nombre del negocio / razón social (comercio).\n" +
            "B) Por cada ítem: DESCRIPCION, CANTIDAD, PRECIO unitario, SUBTOTAL de línea.\n" +
            "C) TOTAL A PAGAR / TOTAL GENERAL / TOTAL (con ITBIS si está incluido) → campo monto.\n" +
            "Reglas:\n" +
            "1) concepto = SOLO el detalle descriptivo multilínea para caja: " +
            "línea 1 = título comercial; luego cada ítem '- DESCRIPCION xCANT @ PRECIO = SUBTOTAL'. " +
            "NO pongas el total a pagar dentro de concepto.\n" +
            "2) monto = ÚNICAMENTE el TOTAL A PAGAR (número). NUNCA subtotal de una línea, " +
            "ni ITBIS solo, ni propina si hay total mayor.\n" +
            "3) Si un campo no es legible con confianza: deja vacío / 0.\n" +
            "JSON únicamente: " +
            "{\"comercio\":\"...\",\"lineas\":[{\"descripcion\":\"...\",\"cantidad\":1,\"precio\":0,\"subtotal\":0}]," +
            "\"concepto\":\"...\",\"monto\":0}";

        private const string VisionPromptAgresivo =
            "REINTENTO OCR factura (RD). Extrae con máximo cuidado:\n" +
            "1) Título comercial del establecimiento.\n" +
            "2) Cada línea: descripción + cantidad + precio + subtotal.\n" +
            "3) concepto = comercio + líneas descriptivas (SIN el total).\n" +
            "4) monto = SOLO el TOTAL A PAGAR / total final (el más grande etiquetado como total).\n" +
            "JSON: {\"comercio\":\"...\",\"lineas\":[{\"descripcion\":\"...\",\"cantidad\":1,\"precio\":0,\"subtotal\":0}]," +
            "\"concepto\":\"...\",\"monto\":0}";

        private readonly OllamaClient _client;
        private readonly List<string> _trace = new();

        public FacturaGastoOrchestrator(OllamaClient? client = null)
        {
            _client = client ?? new OllamaClient();
        }

        public async Task<FacturaGastoAutoResult> ProcesarYRegistrarAsync(
            string filePath,
            byte[]? imageJpegBytes,
            string? usuario,
            Func<bool>? isCajaAbierta,
            CancellationToken ct = default)
        {
            _trace.Clear();
            string fileName = Path.GetFileName(filePath);

            try
            {
                if (isCajaAbierta != null && !isCajaAbierta())
                    return Fail("La caja está cerrada. El gasto no se registró.", fileName);

                if (!new CajaBLL().ObtenerEstadoCaja())
                    return Fail("La caja está cerrada. El gasto no se registró.", fileName);

                AppConfig.LoadOllamaOptions();

                string facturaRoot = Path.GetDirectoryName(filePath) ?? string.Empty;
                string sha256;
                try
                {
                    sha256 = FacturaGastoDuplicateGuard.ComputeFileSha256(filePath);
                    Note("hash:" + FacturaGastoDuplicateGuard.ShortHash(sha256));
                }
                catch (Exception exHash)
                {
                    return Fail(
                        "No se pudo verificar duplicados de la factura.",
                        fileName,
                        exHash.Message);
                }

                // Duplicado por hash de archivo (rápido, antes de gastar IA).
                if (FacturaGastoDuplicateGuard.IsFileHashDuplicate(
                        facturaRoot, sha256, fileName, out string dupEarly))
                {
                    Note("duplicado-archivo");
                    return Fail(
                        "Factura duplicada. No se registró el egreso.",
                        fileName,
                        dupEarly);
                }

                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                string visionModel = ext == ".pdf"
                    ? OllamaOptions.VisionModel
                    : await ResolveVisionModelAsync(ct).ConfigureAwait(false);

                FacturaVisionSuggestion? suggestion = null;
                const int maxIntentos = 3;
                for (int intento = 1; intento <= maxIntentos; intento++)
                {
                    ct.ThrowIfCancellationRequested();
                    Note($"ronda:{intento}/{maxIntentos}");

                    FacturaVisionSuggestion? round = await ExtraerAsync(
                            filePath,
                            imageJpegBytes,
                            visionModel,
                            intentoAgresivo: intento > 1,
                            ct)
                        .ConfigureAwait(false);

                    if (EsMejor(round, suggestion))
                        suggestion = round;

                    if (EsUtil(suggestion))
                        break;

                    // Pausa breve antes de reintentar la orquesta completa.
                    if (intento < maxIntentos)
                        await Task.Delay(500 * intento, ct).ConfigureAwait(false);
                }

                string? concepto = FacturaVisionSuggestion.SanitizeConcepto(suggestion?.Concepto);
                decimal? monto = FacturaVisionSuggestion.SanitizeMonto(suggestion?.Monto);

                if (string.IsNullOrWhiteSpace(concepto) || monto is null or <= 0)
                {
                    return Fail(
                        "La IA no pudo leer concepto y monto de la factura tras varios intentos.",
                        fileName,
                        Truncate(suggestion?.RawResponse, 600)
                        ?? "Sin respuesta útil de los modelos.");
                }

                if (FacturaGastoDuplicateGuard.IsDuplicate(
                        facturaRoot,
                        sha256,
                        fileName,
                        monto.Value,
                        concepto,
                        out string dupReason))
                {
                    Note("duplicado");
                    return Fail(
                        "Factura duplicada. No se registró el egreso.",
                        fileName,
                        dupReason);
                }

                concepto = AnexarOrigen(concepto, fileName, sha256);

                CommandResult? reg = null;
                for (int r = 1; r <= 2; r++)
                {
                    if (isCajaAbierta != null && !isCajaAbierta())
                        return Fail("La caja está cerrada. El gasto no se registró.", fileName);

                    reg = CajaCommandService.RegistrarGasto(concepto, monto.Value, usuario);
                    if (reg.Success)
                        break;

                    Note($"registro-reintento:{r}");
                    await Task.Delay(400 * r, ct).ConfigureAwait(false);
                }

                if (reg is null || !reg.Success)
                {
                    return Fail(
                        "La factura se leyó, pero falló el registro en caja.",
                        fileName,
                        reg?.Message ?? "Sin resultado de registro.");
                }

                FacturaGastoDuplicateGuard.RememberHash(facturaRoot, sha256);

                int id = reg.Payload is int mid ? mid : 0;
                return FacturaGastoAutoResult.Ok(
                    "Se ha subido de manera exitosa. ¿Desea ver?",
                    concepto,
                    monto.Value,
                    id,
                    fileName,
                    string.Join(" → ", _trace));
            }
            catch (OperationCanceledException)
            {
                return Fail("Proceso cancelado o tiempo agotado.", fileName: fileName, trace: Trace());
            }
            catch (Exception ex)
            {
                return Fail(
                    "Error al procesar la factura automáticamente.",
                    fileName,
                    ex.Message,
                    Trace());
            }
        }

        private async Task<FacturaVisionSuggestion?> ExtraerAsync(
            string filePath,
            byte[]? imageJpegBytes,
            string primaryVisionModel,
            bool intentoAgresivo,
            CancellationToken ct)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string visionPrompt = intentoAgresivo ? VisionPromptAgresivo : VisionPrompt;

            if (ext == ".pdf")
            {
                Note("pdf-texto");
                string? pdfText = FacturaPdfTextExtractor.TryExtract(filePath);
                if (string.IsNullOrWhiteSpace(pdfText))
                    throw new InvalidOperationException(
                        "No se pudo extraer texto del PDF. Guarda la factura como JPG/PNG.");

                return await PipelineDesdeTextoAsync(pdfText, ct).ConfigureAwait(false);
            }

            if (imageJpegBytes == null || imageJpegBytes.Length == 0)
                throw new InvalidOperationException("Imagen vacía o no legible.");

            FacturaVisionSuggestion? best = await VisionPassAsync(
                    primaryVisionModel, imageJpegBytes, visionPrompt, ct)
                .ConfigureAwait(false);

            if (!EsUtil(best)
                && !string.Equals(
                    OllamaOptions.VisionFallbackModel,
                    primaryVisionModel,
                    StringComparison.OrdinalIgnoreCase)
                && await SoftHasModelAsync(OllamaOptions.VisionFallbackModel, ct).ConfigureAwait(false))
            {
                FacturaVisionSuggestion? fallback = await VisionPassAsync(
                        OllamaOptions.VisionFallbackModel, imageJpegBytes, visionPrompt, ct)
                    .ConfigureAwait(false);
                if (EsMejor(fallback, best))
                    best = fallback;
            }

            // Siempre intentar reparación si hay cualquier rastro de OCR.
            if (!EsUtil(best)
                || string.IsNullOrWhiteSpace(best?.Concepto)
                || best?.Monto is null or <= 0)
            {
                string raw = best?.RawResponse ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(raw)
                    || !string.IsNullOrWhiteSpace(best?.Concepto)
                    || intentoAgresivo)
                {
                    FacturaVisionSuggestion? repaired = await RepairPassAsync(best, ct)
                        .ConfigureAwait(false);
                    if (EsMejor(repaired, best))
                        best = repaired;
                }
            }

            if (EsUtil(best))
            {
                FacturaVisionSuggestion? validated = await ValidatePassAsync(best!, ct)
                    .ConfigureAwait(false);
                if (EsMejor(validated, best))
                    best = validated;
            }
            else
            {
                FacturaVisionSuggestion? reasoned = await ReasonPassAsync(best, ct)
                    .ConfigureAwait(false);
                if (EsMejor(reasoned, best))
                    best = reasoned;

                // Último empujón: coder otra vez con lo que haya.
                if (!EsUtil(best))
                {
                    FacturaVisionSuggestion? repaired2 = await RepairPassAsync(best, ct)
                        .ConfigureAwait(false);
                    if (EsMejor(repaired2, best))
                        best = repaired2;
                }
            }

            return best;
        }

        private async Task<FacturaVisionSuggestion?> PipelineDesdeTextoAsync(
            string invoiceText,
            CancellationToken ct)
        {
            FacturaVisionSuggestion? best = await TextStructureAsync(
                    OllamaOptions.TextRepairModel,
                    "Estructura factura (texto) a JSON de gasto.\n" +
                    "Prioridad: título comercial, descripción/cantidad/precio/subtotal por línea.\n" +
                    "concepto = SOLO descripción (comercio + líneas). monto = SOLO TOTAL A PAGAR.\n" +
                    "JSON: {\"comercio\":\"\",\"lineas\":[{\"descripcion\":\"\",\"cantidad\":1,\"precio\":0,\"subtotal\":0}]," +
                    "\"concepto\":\"\",\"monto\":0}\n\n" +
                    "TEXTO:\n" + Truncate(invoiceText, 6000),
                    520,
                    ct)
                .ConfigureAwait(false);

            if (!EsUtil(best)
                && await SoftHasModelAsync(OllamaOptions.TextValidateModel, ct).ConfigureAwait(false))
            {
                FacturaVisionSuggestion? v = await TextStructureAsync(
                        OllamaOptions.TextValidateModel,
                        "Corrige JSON de gasto. concepto=descripción (sin total); monto=TOTAL A PAGAR.\n" +
                        "JSON: {\"concepto\":\"\",\"monto\":0}\n\nTEXTO:\n" + Truncate(invoiceText, 5000),
                        360,
                        ct)
                    .ConfigureAwait(false);
                if (EsMejor(v, best))
                    best = v;
            }

            if (!EsUtil(best)
                && await SoftHasModelAsync(OllamaOptions.TextReasonModel, ct).ConfigureAwait(false))
            {
                FacturaVisionSuggestion? r = await TextStructureAsync(
                        OllamaOptions.TextReasonModel,
                        "Extrae concepto (título comercial + ítems) y monto = total a pagar. " +
                        "SOLO JSON {\"concepto\":\"...\",\"monto\":0}\n\n" + Truncate(invoiceText, 4000),
                        640,
                        ct)
                    .ConfigureAwait(false);
                if (EsMejor(r, best))
                    best = r;
            }

            return best;
        }

        private async Task<FacturaVisionSuggestion?> VisionPassAsync(
            string model,
            byte[] imageBytes,
            string prompt,
            CancellationToken ct)
        {
            Note($"visión:{model}");
            try
            {
                string b64 = Convert.ToBase64String(imageBytes);
                string response = await _client.GenerateWithImagesAsync(
                        model,
                        prompt,
                        new[] { b64 },
                        jsonFormat: true,
                        numPredict: 520,
                        ct: ct)
                    .ConfigureAwait(false);

                return FacturaVisionSuggestion.TryParse(response)
                       ?? new FacturaVisionSuggestion { RawResponse = response };
            }
            catch (Exception ex)
            {
                Note($"visión-falló:{Truncate(ex.Message, 80)}");
                return null;
            }
        }

        private async Task<FacturaVisionSuggestion?> RepairPassAsync(
            FacturaVisionSuggestion? current,
            CancellationToken ct)
        {
            if (!await SoftHasModelAsync(OllamaOptions.TextRepairModel, ct).ConfigureAwait(false))
                return current;

            Note($"reparar:{OllamaOptions.TextRepairModel}");
            string payload = Truncate(current?.RawResponse ?? current?.Concepto ?? "", 3500);
            string prompt =
                "Repara OCR de factura a JSON limpio.\n" +
                "concepto = título comercial + líneas (descripcion x cant @ precio = subtotal). SIN total.\n" +
                "monto = SOLO total a pagar.\n" +
                "JSON: {\"comercio\":\"\",\"lineas\":[],\"concepto\":\"\",\"monto\":0}\n\n" +
                "ENTRADA:\n" + payload;

            return await TextStructureAsync(
                    OllamaOptions.TextRepairModel, prompt, 480, ct)
                .ConfigureAwait(false) ?? current;
        }

        private async Task<FacturaVisionSuggestion?> ValidatePassAsync(
            FacturaVisionSuggestion current,
            CancellationToken ct)
        {
            if (!await SoftHasModelAsync(OllamaOptions.TextValidateModel, ct).ConfigureAwait(false))
                return current;

            Note($"validar:{OllamaOptions.TextValidateModel}");
            string prompt =
                "Valida JSON de gasto. concepto debe ser descripción (comercio+ítems), " +
                "sin el total. Si monto no es TOTAL A PAGAR, corrígelo.\n" +
                "JSON: {\"concepto\":\"...\",\"monto\":0}\n\n" +
                "ACTUAL:\n" + Truncate(
                    current.RawResponse
                    ?? $"{{\"concepto\":\"{current.Concepto}\",\"monto\":{current.Monto}}}",
                    3000);

            FacturaVisionSuggestion? v = await TextStructureAsync(
                    OllamaOptions.TextValidateModel, prompt, 280, ct)
                .ConfigureAwait(false);

            return EsMejor(v, current) ? v : current;
        }

        private async Task<FacturaVisionSuggestion?> ReasonPassAsync(
            FacturaVisionSuggestion? current,
            CancellationToken ct)
        {
            if (!await SoftHasModelAsync(OllamaOptions.TextReasonModel, ct).ConfigureAwait(false))
                return current;

            Note($"razonar:{OllamaOptions.TextReasonModel}");
            string prompt =
                "Con razonamiento breve, extrae gasto de esta factura. " +
                "Responde SOLO JSON {\"concepto\":\"...\",\"monto\":0} sin markdown.\n\n" +
                Truncate(current?.RawResponse ?? current?.Concepto ?? "", 2800);

            return await TextStructureAsync(
                    OllamaOptions.TextReasonModel, prompt, 700, ct)
                .ConfigureAwait(false) ?? current;
        }

        private async Task<FacturaVisionSuggestion?> TextStructureAsync(
            string model,
            string prompt,
            int numPredict,
            CancellationToken ct)
        {
            try
            {
                string response = await _client.GenerateTextAsync(
                        model,
                        prompt,
                        jsonFormat: true,
                        numPredict: numPredict,
                        ct: ct)
                    .ConfigureAwait(false);

                return FacturaVisionSuggestion.TryParse(response)
                       ?? new FacturaVisionSuggestion { RawResponse = response };
            }
            catch (Exception ex)
            {
                Note($"{model}:falló({Truncate(ex.Message, 80)})");
                return null;
            }
        }

        private async Task<string> ResolveVisionModelAsync(CancellationToken ct)
        {
            var (available, hasPrimary) = await _client.CheckAsync(OllamaOptions.VisionModel, ct)
                .ConfigureAwait(false);

            if (!available)
                throw new InvalidOperationException(
                    "Ollama no está disponible. Abre Ollama (http://127.0.0.1:11434).");

            if (hasPrimary)
                return OllamaOptions.VisionModel;

            if (await SoftHasModelAsync(OllamaOptions.VisionFallbackModel, ct).ConfigureAwait(false))
            {
                Note($"visión-primario-ausente;usar:{OllamaOptions.VisionFallbackModel}");
                return OllamaOptions.VisionFallbackModel;
            }

            throw new InvalidOperationException(
                $"Falta el modelo '{OllamaOptions.VisionModel}'. Ejecuta: ollama pull {OllamaOptions.VisionModel}");
        }

        private async Task<bool> SoftHasModelAsync(string model, CancellationToken ct)
        {
            try
            {
                var (available, has) = await _client.CheckAsync(model, ct).ConfigureAwait(false);
                return available && has;
            }
            catch
            {
                return false;
            }
        }

        private void Note(string step) => _trace.Add(step);
        private string Trace() => string.Join(" → ", _trace);

        private FacturaGastoAutoResult Fail(string message, string? fileName, string? detail = null, string? trace = null) =>
            FacturaGastoAutoResult.Fail(message, detail, fileName, trace ?? Trace());

        private static bool EsUtil(FacturaVisionSuggestion? s) =>
            s != null
            && !string.IsNullOrWhiteSpace(FacturaVisionSuggestion.SanitizeConcepto(s.Concepto))
            && FacturaVisionSuggestion.SanitizeMonto(s.Monto) is > 0;

        private static bool EsMejor(FacturaVisionSuggestion? a, FacturaVisionSuggestion? b)
        {
            if (a == null)
                return false;
            if (b == null)
                return EsUtil(a) || !string.IsNullOrWhiteSpace(a.Concepto) || a.Monto is > 0;

            bool ua = EsUtil(a);
            bool ub = EsUtil(b);
            if (ua && !ub)
                return true;
            if (!ua && ub)
                return false;
            if (ua && ub)
            {
                int la = a.Concepto?.Length ?? 0;
                int lb = b.Concepto?.Length ?? 0;
                return la >= lb;
            }

            int scoreA = (a.Concepto?.Length ?? 0) + (a.Monto is > 0 ? 100 : 0);
            int scoreB = (b.Concepto?.Length ?? 0) + (b.Monto is > 0 ? 100 : 0);
            return scoreA > scoreB;
        }

        private static string AnexarOrigen(string concepto, string fileName, string sha256)
        {
            string shortHash = FacturaGastoDuplicateGuard.ShortHash(sha256);
            string tag = $"\n[Auto FacturaGastos: {fileName} | hash:{shortHash}]";
            if (concepto.Length + tag.Length <= FacturaVisionSuggestion.ConceptoMaxLength)
                return concepto + tag;

            int keep = FacturaVisionSuggestion.ConceptoMaxLength - tag.Length;
            if (keep < 40)
                return concepto[..Math.Min(concepto.Length, FacturaVisionSuggestion.ConceptoMaxLength)];

            return concepto[..keep].TrimEnd() + tag;
        }

        private static string Truncate(string? s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? (s ?? string.Empty) : s[..max] + "…";
    }
}
