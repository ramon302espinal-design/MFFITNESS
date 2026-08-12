using System.IO.Compression;
using CORE.Update;

namespace BLL.Update
{
    /// <summary>
    /// Valida entradas ZIP contra Zip Slip, rutas absolutas y contrato de archivos permitidos.
    /// </summary>
    public static class UpdateZipPathValidator
    {
        public sealed class ZipValidationResult
        {
            public bool IsValid { get; init; }
            public string Message { get; init; } = string.Empty;
            public IReadOnlyList<string> RelativePaths { get; init; } = Array.Empty<string>();
            public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

            public static ZipValidationResult Ok(IReadOnlyList<string> paths) =>
                new() { IsValid = true, Message = "ZIP válido.", RelativePaths = paths };

            public static ZipValidationResult Fail(string message, params string[] errors) =>
                new() { IsValid = false, Message = message, Errors = errors };
        }

        public static ZipValidationResult ValidateArchive(string zipPath)
        {
            if (string.IsNullOrWhiteSpace(zipPath) || !File.Exists(zipPath))
                return ZipValidationResult.Fail("Paquete ZIP no encontrado.");

            var paths = new List<string>();
            var errors = new List<string>();

            try
            {
                using ZipArchive archive = ZipFile.OpenRead(zipPath);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue; // directorio

                    string? relative = NormalizeEntryPath(entry.FullName, errors);
                    if (relative == null)
                        continue;

                    paths.Add(relative);

                    if (!AllowedUpdatePackageFiles.IsAllowedRelativePath(relative))
                    {
                        errors.Add($"Archivo no permitido en el paquete: {relative}");
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                return ZipValidationResult.Fail("ZIP corrupto o inválido.", ex.Message);
            }
            catch (Exception ex)
            {
                return ZipValidationResult.Fail("Error leyendo ZIP.", ex.Message);
            }

            foreach (string required in AllowedUpdatePackageFiles.RequiredFiles)
            {
                if (!paths.Any(p => string.Equals(p, AllowedUpdatePackageFiles.Normalize(required),
                        StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add($"Archivo requerido faltante: {required}");
                }
            }

            if (errors.Count > 0)
                return ZipValidationResult.Fail("Validación de paquete fallida.", errors.ToArray());

            return ZipValidationResult.Ok(paths);
        }

        internal static string? NormalizeEntryPath(string entryPath, IList<string> errors)
        {
            if (string.IsNullOrWhiteSpace(entryPath))
            {
                errors.Add("Entrada ZIP vacía.");
                return null;
            }

            string normalized = entryPath.Replace('\\', '/').Trim();

            if (Path.IsPathRooted(normalized) ||
                normalized.StartsWith("/", StringComparison.Ordinal) ||
                normalized.Contains(":/", StringComparison.Ordinal) ||
                normalized.StartsWith("\\\\", StringComparison.Ordinal))
            {
                errors.Add($"Ruta absoluta o UNC no permitida: {entryPath}");
                return null;
            }

            if (normalized.Contains("../", StringComparison.Ordinal) ||
                normalized.Contains("/..", StringComparison.Ordinal) ||
                normalized.StartsWith("..", StringComparison.Ordinal))
            {
                errors.Add($"Path traversal no permitido (Zip Slip): {entryPath}");
                return null;
            }

            return AllowedUpdatePackageFiles.Normalize(normalized);
        }
    }
}
