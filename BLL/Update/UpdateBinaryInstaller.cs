using System.Reflection;
using CORE.Update;

namespace BLL.Update
{
    public static class UpdateBinaryInstaller
    {
        public sealed class InstallResult
        {
            public bool Success { get; init; }
            public string Message { get; init; } = string.Empty;
            /// <summary>Archivos copiados exitosamente antes de éxito o fallo (pueden ser parciales).</summary>
            public IReadOnlyList<string> InstalledFiles { get; init; } = Array.Empty<string>();
            public string? FailedOnFile { get; init; }
        }

        /// <summary>
        /// Copia ordenada (RequiredFiles primero) desde staging hacia instalación.
        /// No es atómica por File.Copy secuencial: ante fallo el caller debe restaurar desde snapshot.
        /// </summary>
        /// <param name="beforeFileCopy">
        /// Hook de prueba: puede lanzar para simular fallo en un archivo concreto.
        /// </param>
        public static InstallResult InstallFromStaging(
            string stagingDirectory,
            string installDirectory,
            Action<string>? beforeFileCopy = null)
        {
            if (!Directory.Exists(stagingDirectory))
                return new InstallResult { Success = false, Message = "Staging no existe." };

            if (!Directory.Exists(installDirectory))
                return new InstallResult { Success = false, Message = "Directorio de instalación no existe." };

            var installed = new List<string>();

            try
            {
                foreach (string relative in EnumerateInstallCandidates(stagingDirectory))
                {
                    if (string.Equals(relative, AllowedUpdatePackageFiles.UpdateManagerExe,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    beforeFileCopy?.Invoke(relative);

                    string source = Path.Combine(stagingDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
                    string dest = Path.Combine(installDirectory, relative.Replace('/', Path.DirectorySeparatorChar));

                    string? destDir = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Copy(source, dest, overwrite: true);
                    installed.Add(relative);
                }

                return new InstallResult
                {
                    Success = true,
                    Message = "Archivos instalados desde staging.",
                    InstalledFiles = installed
                };
            }
            catch (Exception ex)
            {
                string? failFile = PeekNextCandidate(stagingDirectory, installed);

                return new InstallResult
                {
                    Success = false,
                    Message = "Error copiando archivos: " + ex.Message,
                    InstalledFiles = installed,
                    FailedOnFile = failFile
                };
            }
        }

        public static bool VerifyRequiredFiles(string installDirectory, out string? error)
        {
            foreach (string required in AllowedUpdatePackageFiles.RequiredFiles)
            {
                string path = Path.Combine(installDirectory, required);
                if (!File.Exists(path))
                {
                    error = $"Archivo requerido no instalado: {required}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        public static bool VerifyAppVersion(string installDirectory, string expectedAppVersion, out string? actual, out string? error)
        {
            string uiPath = Path.Combine(installDirectory, "UI.exe");
            actual = UpdateBinarySnapshotService.ReadFileVersion(uiPath);
            string expected = expectedAppVersion.Trim();

            if (!SemVer.TryParse(expected, out _))
            {
                error = "AppVersion del manifest inválida.";
                return false;
            }

            if (!string.Equals(NormalizeVersion(actual), NormalizeVersion(expected), StringComparison.OrdinalIgnoreCase))
            {
                error = $"Versión instalada ({actual}) no coincide con manifest ({expected}).";
                return false;
            }

            error = null;
            return true;
        }

        public static bool CanLoadMainAssembly(string installDirectory, out string? error)
        {
            try
            {
                // .NET WinExe: UI.exe es apphost nativo (sin metadata managed).
                // El ensamblado real es UI.dll.
                string uiDll = Path.Combine(installDirectory, "UI.dll");
                string uiExe = Path.Combine(installDirectory, "UI.exe");

                if (File.Exists(uiDll))
                {
                    AssemblyName.GetAssemblyName(uiDll);
                    error = null;
                    return true;
                }

                if (!File.Exists(uiExe))
                {
                    error = "UI.exe/UI.dll no existen.";
                    return false;
                }

                // Fallback legacy / smokes con UI.exe managed
                try
                {
                    AssemblyName.GetAssemblyName(uiExe);
                    error = null;
                    return true;
                }
                catch (BadImageFormatException)
                {
                    error = "UI.dll ausente y UI.exe no es ensamblado managed (apphost). Package incompleto.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = "No se pudo cargar el ensamblado principal: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Orden determinista: RequiredFiles en contrato, luego el resto alfabético.
        /// </summary>
        public static IReadOnlyList<string> EnumerateInstallCandidates(string stagingDirectory)
        {
            var present = Directory.EnumerateFiles(stagingDirectory, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(stagingDirectory, f).Replace('\\', '/'))
                .Where(r => !string.Equals(r, AllowedUpdatePackageFiles.UpdateManagerExe, StringComparison.OrdinalIgnoreCase))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var ordered = new List<string>();

            foreach (string required in AllowedUpdatePackageFiles.RequiredFiles)
            {
                string n = AllowedUpdatePackageFiles.Normalize(required);
                if (present.Contains(n))
                    ordered.Add(n);
            }

            foreach (string extra in present
                         .Where(p => !ordered.Contains(p, StringComparer.OrdinalIgnoreCase))
                         .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                ordered.Add(extra);
            }

            return ordered;
        }

        private static string? PeekNextCandidate(string stagingDirectory, List<string> alreadyInstalled)
        {
            var all = EnumerateInstallCandidates(stagingDirectory);
            var done = new HashSet<string>(alreadyInstalled, StringComparer.OrdinalIgnoreCase);
            return all.FirstOrDefault(c => !done.Contains(c));
        }

        private static string NormalizeVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return string.Empty;

            int plus = version.IndexOf('+');
            if (plus >= 0)
                version = version[..plus];

            return version.Trim();
        }
    }
}
