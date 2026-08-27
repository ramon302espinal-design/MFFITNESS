namespace CORE.Ollama
{
    /// <summary>
    /// Configuración local de Ollama (PC). No afecta DB ni POS.
    /// </summary>
    public static class OllamaOptions
    {
        public const string DefaultBaseUrl = "http://127.0.0.1:11434";
        public const string DefaultVisionModel = "qwen2.5vl:7b";
        public const string DefaultVisionFallbackModel = "gemma3:4b";
        public const string DefaultTextRepairModel = "qwen2.5-coder:7b";
        public const string DefaultTextValidateModel = "llama3.1:8b";
        public const string DefaultTextReasonModel = "deepseek-r1:7b";
        public const string DefaultFacturaGastosFolderName = "FacturaGastos";
        public const string DefaultFacturaGastosDevFolderName = "FacturaGastosDev";
        public const int DefaultTimeoutSeconds = 120;

        public static string BaseUrl { get; private set; } = DefaultBaseUrl;
        public static string VisionModel { get; private set; } = DefaultVisionModel;
        public static string VisionFallbackModel { get; private set; } = DefaultVisionFallbackModel;
        public static string TextRepairModel { get; private set; } = DefaultTextRepairModel;
        public static string TextValidateModel { get; private set; } = DefaultTextValidateModel;
        public static string TextReasonModel { get; private set; } = DefaultTextReasonModel;
        public static int TimeoutSeconds { get; private set; } = DefaultTimeoutSeconds;

        /// <summary>Lado máximo de imagen enviada al modelo VL (OCR de empaque).</summary>
        public static int VisionMaxSide { get; private set; } = 1024;

        /// <summary>Nombre de carpeta (relativo a raíz del proyecto o al .exe).</summary>
        public static string FacturaGastosFolderName { get; private set; } = DefaultFacturaGastosFolderName;

        /// <summary>Ruta absoluta opcional; si está vacía se resuelve automáticamente.</summary>
        public static string? FacturaGastosFolderPath { get; private set; }

        public static bool FacturaGastosAutoEnabled { get; private set; } = true;

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

            string? visionFallback = config["Ollama:VisionFallbackModel"];
            if (!string.IsNullOrWhiteSpace(visionFallback))
                VisionFallbackModel = visionFallback.Trim();

            string? textRepair = config["Ollama:TextRepairModel"];
            if (!string.IsNullOrWhiteSpace(textRepair))
                TextRepairModel = textRepair.Trim();

            string? textValidate = config["Ollama:TextValidateModel"];
            if (!string.IsNullOrWhiteSpace(textValidate))
                TextValidateModel = textValidate.Trim();

            string? textReason = config["Ollama:TextReasonModel"];
            if (!string.IsNullOrWhiteSpace(textReason))
                TextReasonModel = textReason.Trim();

            if (int.TryParse(config["Ollama:TimeoutSeconds"], out int timeout) && timeout > 0)
                TimeoutSeconds = timeout;

            if (int.TryParse(config["Ollama:VisionMaxSide"], out int maxSide) && maxSide >= 256)
                VisionMaxSide = Math.Min(maxSide, 1024);

            string? folderName = config["Ollama:FacturaGastos:FolderName"];
            if (!string.IsNullOrWhiteSpace(folderName))
                FacturaGastosFolderName = folderName.Trim();

            string? folderPath = config["Ollama:FacturaGastos:FolderPath"];
            FacturaGastosFolderPath = string.IsNullOrWhiteSpace(folderPath) ? null : folderPath.Trim();

            string? enabled = config["Ollama:FacturaGastos:Enabled"];
            if (bool.TryParse(enabled, out bool autoEnabled))
                FacturaGastosAutoEnabled = autoEnabled;
        }
    }
}
