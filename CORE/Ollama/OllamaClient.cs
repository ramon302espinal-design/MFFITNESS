using System.Text;
using System.Text.Json;

namespace CORE.Ollama
{
    /// <summary>
    /// Cliente HTTP hacia Ollama local. HttpClient compartido (keep-alive).
    /// </summary>
    public sealed class OllamaClient
    {
        private static readonly object Sync = new();
        private static HttpClient? _shared;

        private readonly HttpClient _http;

        public OllamaClient(HttpClient? httpClient = null)
        {
            _http = httpClient ?? GetSharedClient();
        }

        private static HttpClient GetSharedClient()
        {
            if (_shared != null)
                return _shared;

            lock (Sync)
            {
                if (_shared != null)
                    return _shared;

                _shared = new HttpClient
                {
                    BaseAddress = new Uri(OllamaOptions.BaseUrl.TrimEnd('/') + "/"),
                    Timeout = TimeSpan.FromSeconds(OllamaOptions.TimeoutSeconds)
                };
                return _shared;
            }
        }

        /// <summary>Una sola llamada a /api/tags: disponible + modelo presente.</summary>
        public async Task<(bool Available, bool HasModel)> CheckAsync(
            string model,
            CancellationToken ct = default)
        {
            try
            {
                using var response = await _http.GetAsync("api/tags", ct).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return (false, false);

                using var doc = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false),
                    cancellationToken: ct).ConfigureAwait(false);

                if (!doc.RootElement.TryGetProperty("models", out JsonElement models))
                    return (true, false);

                string wanted = model.Trim();
                foreach (JsonElement m in models.EnumerateArray())
                {
                    string? name = m.TryGetProperty("name", out JsonElement n) ? n.GetString() : null;
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (string.Equals(name, wanted, StringComparison.OrdinalIgnoreCase))
                        return (true, true);

                    if (wanted.Contains(':')
                        && name.StartsWith(wanted.Split(':')[0] + ":", StringComparison.OrdinalIgnoreCase)
                        && (name.Equals(wanted, StringComparison.OrdinalIgnoreCase)
                            || name.EndsWith(":latest", StringComparison.OrdinalIgnoreCase)))
                        return (true, true);
                }

                return (true, false);
            }
            catch
            {
                return (false, false);
            }
        }

        public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
        {
            var (available, _) = await CheckAsync(OllamaOptions.VisionModel, ct).ConfigureAwait(false);
            return available;
        }

        public async Task<bool> HasModelAsync(string model, CancellationToken ct = default)
        {
            var (_, has) = await CheckAsync(model, ct).ConfigureAwait(false);
            return has;
        }

        public Task<string> GenerateWithImagesAsync(
            string model,
            string prompt,
            IReadOnlyList<string> imagesBase64,
            bool jsonFormat = true,
            int? numPredict = null,
            CancellationToken ct = default) =>
            GenerateCoreAsync(model, prompt, imagesBase64, jsonFormat, numPredict, ct);

        /// <summary>Generación solo texto (reparación / validación JSON de factura).</summary>
        public Task<string> GenerateTextAsync(
            string model,
            string prompt,
            bool jsonFormat = true,
            int? numPredict = null,
            CancellationToken ct = default) =>
            GenerateCoreAsync(model, prompt, imagesBase64: null, jsonFormat, numPredict, ct);

        private async Task<string> GenerateCoreAsync(
            string model,
            string prompt,
            IReadOnlyList<string>? imagesBase64,
            bool jsonFormat,
            int? numPredict,
            CancellationToken ct)
        {
            int predict = numPredict ?? 360;
            var body = new Dictionary<string, object?>
            {
                ["model"] = model,
                ["prompt"] = prompt,
                ["stream"] = false,
                ["options"] = new Dictionary<string, object?>
                {
                    ["num_predict"] = predict,
                    ["temperature"] = 0.0,
                    ["top_p"] = 0.8,
                    ["repeat_penalty"] = 1.05
                }
            };
            if (imagesBase64 is { Count: > 0 })
                body["images"] = imagesBase64;
            if (jsonFormat)
                body["format"] = "json";

            using var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            using var response = await _http.PostAsync("api/generate", content, ct).ConfigureAwait(false);
            string raw = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Ollama respondió {(int)response.StatusCode}: {Truncate(raw, 400)}");

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("response", out JsonElement resp))
                return resp.GetString() ?? string.Empty;

            return raw;
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max] + "…";
    }
}
