using CORE;

namespace CORE.Ollama
{
    /// <summary>
    /// Resuelve y asegura la carpeta de facturas automáticas.
    /// Development → FacturaGastosDev; Production → FacturaGastos (raíz del proyecto / junto al .exe).
    /// </summary>
    public static class FacturaGastosFolder
    {
        public static string ResolveRoot(bool createIfMissing = true)
        {
            AppConfig.LoadOllamaOptions();

            string? configured = OllamaOptions.FacturaGastosFolderPath;
            if (!string.IsNullOrWhiteSpace(configured))
            {
                string abs = Path.GetFullPath(configured.Trim());
                if (createIfMissing)
                    EnsureTree(abs);
                return abs;
            }

            string folderName = ResolveFolderNameForEnvironment();

            // Preferir carpeta en la raíz del repo (DEV/PROD local) si existe.
            string? fromWalk = WalkUpForExistingFolder(folderName);
            if (!string.IsNullOrWhiteSpace(fromWalk))
            {
                if (createIfMissing)
                    EnsureTree(fromWalk);
                return fromWalk;
            }

            string? repoRoot = WalkUpForRepoRoot();
            string root = Path.Combine(
                repoRoot ?? AppDomain.CurrentDomain.BaseDirectory,
                folderName);

            if (createIfMissing)
                EnsureTree(root);
            return root;
        }

        /// <summary>
        /// Production → FacturaGastos; cualquier otro entorno → FacturaGastosDev
        /// (salvo override explícito de FolderName distinto a ambos defaults).
        /// </summary>
        public static string ResolveFolderNameForEnvironment()
        {
            bool isProduction = IsProductionEnvironment();
            string configured = (OllamaOptions.FacturaGastosFolderName ?? string.Empty).Trim();

            if (isProduction)
            {
                if (!string.IsNullOrWhiteSpace(configured)
                    && !string.Equals(configured, OllamaOptions.DefaultFacturaGastosDevFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    return configured;
                }

                return OllamaOptions.DefaultFacturaGastosFolderName;
            }

            // Development (y no-prod): FacturaGastosDev
            if (!string.IsNullOrWhiteSpace(configured)
                && !string.Equals(configured, OllamaOptions.DefaultFacturaGastosFolderName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(configured, OllamaOptions.DefaultFacturaGastosDevFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return configured;
            }

            return OllamaOptions.DefaultFacturaGastosDevFolderName;
        }

        private static bool IsProductionEnvironment()
        {
            try
            {
                // Peek: no requiere BD. Coherente con perfil UI (Production) / POS instalado.
                string env = AppConfig.PeekEnvironment();
                return string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string Procesadas(string root) => Path.Combine(root, "Procesadas");
        public static string Errores(string root) => Path.Combine(root, "Errores");

        public static void EnsureTree(string root)
        {
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Procesadas(root));
            Directory.CreateDirectory(Errores(root));
        }

        private static string? WalkUpForExistingFolder(string folderName)
        {
            try
            {
                DirectoryInfo? dir = new(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
                {
                    string candidate = Path.Combine(dir.FullName, folderName);
                    if (Directory.Exists(candidate))
                        return candidate;
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private static string? WalkUpForRepoRoot()
        {
            try
            {
                DirectoryInfo? dir = new(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
                {
                    if (File.Exists(Path.Combine(dir.FullName, "MFFITNESS.sln"))
                        || Directory.Exists(Path.Combine(dir.FullName, ".git"))
                        || Directory.Exists(Path.Combine(dir.FullName, "UI")))
                    {
                        return dir.FullName;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }
    }
}
