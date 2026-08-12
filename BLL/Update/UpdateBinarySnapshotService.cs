using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using CORE.Update;

namespace BLL.Update
{
    public static class UpdateBinarySnapshotService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public sealed class SnapshotResult
        {
            public bool Success { get; init; }
            public string Message { get; init; } = string.Empty;
            public UpdateSnapshotInfo? Snapshot { get; init; }
        }

        public sealed class RestoreResult
        {
            public bool Success { get; init; }
            public string Message { get; init; } = string.Empty;
            public IReadOnlyList<string> RestoredFiles { get; init; } = Array.Empty<string>();
            public IReadOnlyList<string> FailedFiles { get; init; } = Array.Empty<string>();
        }

        public static SnapshotResult CreateSnapshot(string installDirectory, string snapshotRootDirectory)
        {
            if (!Directory.Exists(installDirectory))
                return new SnapshotResult { Success = false, Message = "Directorio de instalación no existe." };

            string snapshotId = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N")[..8];
            string snapshotDir = Path.Combine(snapshotRootDirectory, snapshotId);

            try
            {
                Directory.CreateDirectory(snapshotDir);
                var entries = new List<UpdateSnapshotFileEntry>();

                foreach (string required in AllowedUpdatePackageFiles.RequiredFiles)
                {
                    string source = Path.Combine(installDirectory, required);
                    if (!File.Exists(source))
                        continue;

                    string dest = Path.Combine(snapshotDir, required);
                    string? destParent = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(destParent))
                        Directory.CreateDirectory(destParent);

                    File.Copy(source, dest, overwrite: true);
                    entries.Add(new UpdateSnapshotFileEntry
                    {
                        RelativePath = AllowedUpdatePackageFiles.Normalize(required),
                        SizeBytes = new FileInfo(dest).Length,
                        Sha256 = ComputeSha256Hex(dest)
                    });
                }

                string appVersion = ReadFileVersion(sourcePath: Path.Combine(installDirectory, "UI.exe"));
                string informational = ReadInformationalVersion(Path.Combine(installDirectory, "UI.exe"));

                var info = new UpdateSnapshotInfo
                {
                    SnapshotId = snapshotId,
                    SnapshotDirectory = snapshotDir,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    AppVersion = appVersion,
                    InformationalVersion = informational,
                    InstallDirectory = installDirectory,
                    Files = entries
                };

                File.WriteAllText(
                    Path.Combine(snapshotDir, "snapshot.json"),
                    JsonSerializer.Serialize(info, JsonOptions));

                return new SnapshotResult
                {
                    Success = true,
                    Message = "Snapshot creado.",
                    Snapshot = info
                };
            }
            catch (Exception ex)
            {
                try { if (Directory.Exists(snapshotDir)) Directory.Delete(snapshotDir, recursive: true); }
                catch { /* ignore */ }

                return new SnapshotResult { Success = false, Message = "Error creando snapshot: " + ex.Message };
            }
        }

        public static UpdateSnapshotInfo? LoadSnapshot(string snapshotDirectory)
        {
            string jsonPath = Path.Combine(snapshotDirectory, "snapshot.json");
            if (!File.Exists(jsonPath))
                return null;

            try
            {
                return JsonSerializer.Deserialize<UpdateSnapshotInfo>(File.ReadAllText(jsonPath), JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Restaura binarios desde snapshot y verifica SHA256 de cada archivo restaurado.
        /// Solo toca rutas listadas en el snapshot (whitelist implícita de RequiredFiles).
        /// </summary>
        public static RestoreResult RestoreFromSnapshot(
            UpdateSnapshotInfo snapshot,
            string installDirectory,
            IReadOnlyList<string>? onlyRelativePaths = null)
        {
            if (snapshot == null)
                return new RestoreResult { Success = false, Message = "Snapshot nulo." };

            if (string.IsNullOrWhiteSpace(snapshot.SnapshotDirectory) ||
                !Directory.Exists(snapshot.SnapshotDirectory))
            {
                return new RestoreResult { Success = false, Message = "Directorio de snapshot no existe." };
            }

            if (!Directory.Exists(installDirectory))
                return new RestoreResult { Success = false, Message = "Directorio de instalación no existe." };

            var restored = new List<string>();
            var failed = new List<string>();

            IEnumerable<UpdateSnapshotFileEntry> targets = snapshot.Files;
            if (onlyRelativePaths != null && onlyRelativePaths.Count > 0)
            {
                var set = new HashSet<string>(
                    onlyRelativePaths.Select(AllowedUpdatePackageFiles.Normalize),
                    StringComparer.OrdinalIgnoreCase);

                // Restaurar todos los del snapshot que fueron tocados; si la lista es parcial,
                // también restauramos el resto de RequiredFiles del snapshot para consistencia.
                targets = snapshot.Files.Where(f =>
                    set.Contains(AllowedUpdatePackageFiles.Normalize(f.RelativePath)) ||
                    AllowedUpdatePackageFiles.IsRequired(f.RelativePath));
            }

            foreach (UpdateSnapshotFileEntry entry in targets)
            {
                string relative = AllowedUpdatePackageFiles.Normalize(entry.RelativePath);
                if (string.Equals(relative, AllowedUpdatePackageFiles.UpdateManagerExe, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!AllowedUpdatePackageFiles.IsAllowedRelativePath(relative) &&
                    !AllowedUpdatePackageFiles.IsRequired(relative))
                {
                    failed.Add(relative + " (no permitido)");
                    continue;
                }

                string source = Path.Combine(
                    snapshot.SnapshotDirectory,
                    relative.Replace('/', Path.DirectorySeparatorChar));

                string dest = Path.Combine(
                    installDirectory,
                    relative.Replace('/', Path.DirectorySeparatorChar));

                try
                {
                    if (!File.Exists(source))
                    {
                        failed.Add(relative + " (ausente en snapshot)");
                        continue;
                    }

                    string? destDir = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Copy(source, dest, overwrite: true);

                    string actualSha = ComputeSha256Hex(dest);
                    if (!string.Equals(actualSha, entry.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        failed.Add(relative + " (SHA256 mismatch post-restore)");
                        continue;
                    }

                    restored.Add(relative);
                }
                catch (Exception ex)
                {
                    failed.Add(relative + " (" + ex.Message + ")");
                }
            }

            if (failed.Count > 0)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "Restauración incompleta: " + string.Join("; ", failed),
                    RestoredFiles = restored,
                    FailedFiles = failed
                };
            }

            return new RestoreResult
            {
                Success = true,
                Message = "Binarios restaurados desde snapshot (SHA256 verificado).",
                RestoredFiles = restored,
                FailedFiles = Array.Empty<string>()
            };
        }

        public static string ComputeSha256Hex(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }

        public static string ReadFileVersion(string sourcePath)
        {
            if (!File.Exists(sourcePath))
                return "0.0.0";

            var info = FileVersionInfo.GetVersionInfo(sourcePath);
            return info.ProductVersion?.Split('+')[0]?.Trim() ?? info.FileVersion ?? "0.0.0";
        }

        public static string ReadInformationalVersion(string sourcePath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    return string.Empty;

                var asm = AssemblyName.GetAssemblyName(sourcePath);
                return FileVersionInfo.GetVersionInfo(sourcePath).ProductVersion ?? asm.Version?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
