using System.Reflection;

namespace CORE
{
    /// <summary>
    /// Versión del producto leída del ensamblado de entrada (UI.exe).
    /// Los metadatos provienen de Directory.Build.props en tiempo de compilación.
    /// </summary>
    public static class AppVersion
    {
        private static readonly Assembly SourceAssembly =
            Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        public static string ProductName => "MFFITNESS POS";

        /// <summary>Versión SemVer del producto (p. ej. 1.0.0).</summary>
        public static string SemanticVersion =>
            SourceAssembly.GetName().Version?.ToString(3) ?? "0.0.0";

        /// <summary>Identificador de build embebido en InformationalVersion (p. ej. ec24e58).</summary>
        public static string Build
        {
            get
            {
                string? informational = SourceAssembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;

                if (string.IsNullOrWhiteSpace(informational))
                    return "unknown";

                int plus = informational.IndexOf('+');
                if (plus >= 0 && plus < informational.Length - 1)
                    return informational[(plus + 1)..];

                return "unknown";
            }
        }

        /// <summary>InformationalVersion completo (p. ej. 1.0.0+ec24e58).</summary>
        public static string Informational =>
            SourceAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? SemanticVersion;

        /// <summary>Texto listo para mostrar en la UI.</summary>
        public static string DisplayText =>
            $"{ProductName}{Environment.NewLine}Versión {SemanticVersion}{Environment.NewLine}Build: {Build}";
    }
}
