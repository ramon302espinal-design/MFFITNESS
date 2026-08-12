namespace CORE.Update
{
    /// <summary>
    /// Contrato de archivos permitidos/requeridos en el paquete de actualización.
    /// </summary>
    public static class AllowedUpdatePackageFiles
    {
        public static readonly IReadOnlyList<string> RequiredFiles = new[]
        {
            "UI.exe",
            "UI.dll",
            "BLL.dll",
            "DL.dll",
            "DTO.dll",
            "CORE.dll"
        };

        /// <summary>
        /// Extensiones permitidas bajo el directorio de instalación (relativas).
        /// </summary>
        public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe",
            ".dll",
            ".json",
            ".config",
            ".xml",
            ".pdb",
            ".sql",
            ".png",
            ".jpg",
            ".jpeg",
            ".ico",
            ".txt",
            ".md",
            ".deps.json",
            ".runtimeconfig.json"
        };

        /// <summary>
        /// Prefijos de carpeta relativos permitidos (además de raíz).
        /// </summary>
        public static readonly IReadOnlyList<string> AllowedRelativePrefixes = new[]
        {
            "Resources/",
            "Database/",
            "Database/Migrations/",
            "runtimes/"
        };

        /// <summary>
        /// UpdateManager.exe no se reemplaza mientras corre (FASE 9).
        /// </summary>
        public const string UpdateManagerExe = "UpdateManager.exe";

        public static bool IsRequired(string relativePath) =>
            RequiredFiles.Any(r => string.Equals(Normalize(r), Normalize(relativePath), StringComparison.OrdinalIgnoreCase));

        public static bool IsAllowedRelativePath(string relativePath)
        {
            string n = Normalize(relativePath);
            if (string.IsNullOrWhiteSpace(n))
                return false;

            if (string.Equals(n, UpdateManagerExe, StringComparison.OrdinalIgnoreCase))
                return false;

            if (RequiredFiles.Any(r => string.Equals(Normalize(r), n, StringComparison.OrdinalIgnoreCase)))
                return true;

            string ext = Path.GetExtension(n);
            // deps.json / runtimeconfig.json usan doble extensión
            if (n.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) ||
                n.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase))
            {
                if (!n.Contains('/'))
                    return true;
            }

            bool underAllowedFolder = AllowedRelativePrefixes.Any(p =>
                n.StartsWith(Normalize(p), StringComparison.OrdinalIgnoreCase));

            if (underAllowedFolder)
                return AllowedExtensions.Contains(ext) || string.IsNullOrEmpty(ext);

            // Solo archivos en raíz con extensión permitida
            if (!n.Contains('/'))
                return AllowedExtensions.Contains(ext);

            return false;
        }

        public static string Normalize(string path) =>
            path.Replace('\\', '/').Trim().TrimStart('/');
    }
}
