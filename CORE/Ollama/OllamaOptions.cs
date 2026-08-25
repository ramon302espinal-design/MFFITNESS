namespace CORE.Ollama
{
    /// <summary>
    /// Configuración local de Ollama (PC). No afecta DB ni POS.
    /// </summary>
    public static class OllamaOptions
    {
        public const string DefaultBaseUrl = "http://127.0.0.1:11434";
        public const string DefaultVisionModel = "llava:7b";
        public const int DefaultTimeoutSeconds = 90;

        public static string BaseUrl { get; private set; } = DefaultBaseUrl;
        public static string VisionModel { get; private set; } = DefaultVisionModel;
        public static int TimeoutSeconds { get; private set; } = DefaultTimeoutSeconds;

        /// <summary>Lado máximo de imagen enviada a llava (mayor = mejor texto pequeño).</summary>
        public static int VisionMaxSide { get; private set; } = 896;

        public static void ApplyFromConfiguration(Microsoft.Extensions.Configuration.IConfiguration? config)
        {
            if (config == null)
                return;

            string? url = config["Ollama:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(url))
                BaseUrl = url.Trim().TrimEnd('/');

            string? model = config["Ollama:VisionModel"];
            if (!string.IsNullOrWhiteSpace(model))
                VisionModel = model.Trim();

            if (int.TryParse(config["Ollama:TimeoutSeconds"], out int timeout) && timeout > 0)
                TimeoutSeconds = timeout;

            if (int.TryParse(config["Ollama:VisionMaxSide"], out int maxSide) && maxSide >= 256)
                VisionMaxSide = Math.Min(maxSide, 1024);
        }
    }
}
