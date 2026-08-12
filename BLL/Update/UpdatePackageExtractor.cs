using System.IO.Compression;
using CORE.Update;

namespace BLL.Update
{
    public static class UpdatePackageExtractor
    {
        public sealed class ExtractResult
        {
            public bool Success { get; init; }
            public string Message { get; init; } = string.Empty;
            public string StagingDirectory { get; init; } = string.Empty;
            public IReadOnlyList<string> ExtractedFiles { get; init; } = Array.Empty<string>();
        }

        public static ExtractResult ExtractToStaging(string zipPath, string stagingDirectory)
        {
            var validation = UpdateZipPathValidator.ValidateArchive(zipPath);
            if (!validation.IsValid)
                return new ExtractResult { Success = false, Message = validation.Message };

            try
            {
                if (Directory.Exists(stagingDirectory))
                    Directory.Delete(stagingDirectory, recursive: true);

                Directory.CreateDirectory(stagingDirectory);

                var extracted = new List<string>();

                using ZipArchive archive = ZipFile.OpenRead(zipPath);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    var errors = new List<string>();
                    string? relative = UpdateZipPathValidator.NormalizeEntryPath(entry.FullName, errors);
                    if (relative == null)
                    {
                        Directory.Delete(stagingDirectory, recursive: true);
                        return new ExtractResult
                        {
                            Success = false,
                            Message = "Extracción abortada: " + string.Join(" ", errors)
                        };
                    }

                    string destPath = Path.GetFullPath(Path.Combine(stagingDirectory, relative.Replace('/', Path.DirectorySeparatorChar)));
                    string stagingRoot = Path.GetFullPath(stagingDirectory);
                    if (!destPath.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Delete(stagingDirectory, recursive: true);
                        return new ExtractResult
                        {
                            Success = false,
                            Message = "Extracción abortada: destino fuera de staging (Zip Slip)."
                        };
                    }

                    string? destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    entry.ExtractToFile(destPath, overwrite: true);
                    extracted.Add(relative);
                }

                return new ExtractResult
                {
                    Success = true,
                    Message = "Paquete extraído en staging.",
                    StagingDirectory = stagingDirectory,
                    ExtractedFiles = extracted
                };
            }
            catch (Exception ex)
            {
                try { if (Directory.Exists(stagingDirectory)) Directory.Delete(stagingDirectory, recursive: true); }
                catch { /* ignore */ }

                return new ExtractResult { Success = false, Message = "Error extrayendo ZIP: " + ex.Message };
            }
        }
    }
}
