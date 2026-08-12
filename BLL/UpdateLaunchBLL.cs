using System.Diagnostics;
using System.Text.Json;
using BLL.Update;
using CORE;
using CORE.Update;

namespace BLL
{
    /// <summary>
    /// FASE 11 mínima: discovery → download → request.json → Launch UpdateManager.exe.
    /// No ejecuta install/migrate (eso es UpdateManager).
    /// </summary>
    public static class UpdateLaunchBLL
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public sealed class PreparedUpdate
        {
            public required UpdateManifest Manifest { get; init; }
            public required string PackagePath { get; init; }
            public required string ExpectedSha256 { get; init; }
            public required string InstallDirectory { get; init; }
            public string? PackageDownloadUrl { get; init; }
        }

        public static async Task<UpdateLaunchResult> CheckForUpdateAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = GitHubReleaseDiscoveryBLL.CreateDefaultClient();
                var discovery = await GitHubReleaseDiscoveryBLL
                    .DiscoverLatestAsync(client, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (discovery.Release.Status != GitHubReleaseStatus.Success || discovery.Release.Manifest == null)
                {
                    return UpdateLaunchResult.Create(
                        UpdateLaunchStatus.DiscoveryFailed,
                        discovery.Release.Message ?? "No se pudo consultar GitHub Releases.");
                }

                var availability = discovery.Availability;
                if (availability == null)
                {
                    return UpdateLaunchResult.Create(
                        UpdateLaunchStatus.DiscoveryFailed,
                        "Sin evaluación de disponibilidad.");
                }

                return availability.Status switch
                {
                    UpdateAvailabilityStatus.Available => UpdateLaunchResult.Create(
                        UpdateLaunchStatus.Available,
                        availability.Reason,
                        discovery.Release.Manifest,
                        availability: availability),
                    UpdateAvailabilityStatus.NotAvailable => UpdateLaunchResult.Create(
                        UpdateLaunchStatus.NotAvailable,
                        availability.Reason,
                        discovery.Release.Manifest,
                        availability: availability),
                    UpdateAvailabilityStatus.Incompatible => UpdateLaunchResult.Create(
                        UpdateLaunchStatus.Incompatible,
                        availability.Reason,
                        discovery.Release.Manifest,
                        availability: availability),
                    _ => UpdateLaunchResult.Create(
                        UpdateLaunchStatus.DiscoveryFailed,
                        availability.Reason,
                        availability: availability)
                };
            }
            catch (Exception ex)
            {
                return UpdateLaunchResult.Create(UpdateLaunchStatus.Failed, ex.Message);
            }
        }

        /// <summary>
        /// Descarga + verifica SHA256 y deja PreparedUpdate listo para lanzar UpdateManager.
        /// </summary>
        public static async Task<(UpdateLaunchResult Result, PreparedUpdate? Prepared)> DownloadLatestAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = GitHubReleaseDiscoveryBLL.CreateDefaultClient();
                var discovery = await GitHubReleaseDiscoveryBLL
                    .DiscoverLatestAsync(client, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (discovery.Release.Status != GitHubReleaseStatus.Success ||
                    discovery.Release.Manifest == null ||
                    discovery.Availability?.Status != UpdateAvailabilityStatus.Available)
                {
                    var status = discovery.Availability?.Status switch
                    {
                        UpdateAvailabilityStatus.NotAvailable => UpdateLaunchStatus.NotAvailable,
                        UpdateAvailabilityStatus.Incompatible => UpdateLaunchStatus.Incompatible,
                        _ => UpdateLaunchStatus.DiscoveryFailed
                    };
                    return (UpdateLaunchResult.Create(
                        status,
                        discovery.Availability?.Reason ?? discovery.Release.Message ?? "Update no disponible."), null);
                }

                // Caja fail-closed antes de side effects de download (download es OK; install lo revalida)
                try
                {
                    if (new CajaBLL().ObtenerEstadoCaja() == true)
                    {
                        return (UpdateLaunchResult.Create(
                            UpdateLaunchStatus.Blocked,
                            "Caja abierta. Cierre la caja antes de actualizar."), null);
                    }
                }
                catch (Exception ex)
                {
                    return (UpdateLaunchResult.Create(
                        UpdateLaunchStatus.Blocked,
                        "No se pudo verificar caja (fail closed): " + ex.Message), null);
                }

                var download = await UpdatePackageDownloadBLL
                    .DownloadFromReleaseAsync(discovery.Release, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (download.Status != PackageDownloadStatus.SuccessVerified ||
                    string.IsNullOrWhiteSpace(download.LocalFilePath) ||
                    string.IsNullOrWhiteSpace(download.ComputedSha256))
                {
                    return (UpdateLaunchResult.Create(
                        UpdateLaunchStatus.DownloadFailed,
                        download.Message), null);
                }

                var prepared = new PreparedUpdate
                {
                    Manifest = discovery.Release.Manifest,
                    PackagePath = download.LocalFilePath,
                    ExpectedSha256 = download.ComputedSha256,
                    InstallDirectory = AppContext.BaseDirectory.TrimEnd(
                        Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    PackageDownloadUrl = discovery.Release.PackageDownloadUrl
                };

                return (UpdateLaunchResult.Create(
                    UpdateLaunchStatus.Prepared,
                    "Paquete descargado y verificado.",
                    prepared.Manifest,
                    prepared.PackagePath), prepared);
            }
            catch (Exception ex)
            {
                return (UpdateLaunchResult.Create(UpdateLaunchStatus.Failed, ex.Message), null);
            }
        }

        public static UpdateLaunchResult WriteRequestAndLaunch(
            PreparedUpdate prepared,
            bool startApplicationAfterInstall = true)
        {
            try
            {
                string umPath = ResolveUpdateManagerPath(prepared.InstallDirectory);
                if (umPath == null)
                {
                    return UpdateLaunchResult.Create(
                        UpdateLaunchStatus.Failed,
                        "UpdateManager.exe no encontrado. Se espera UpdateManager\\UpdateManager.exe junto a UI.exe (Publish-Pos.ps1).",
                        prepared.Manifest,
                        prepared.PackagePath);
                }

                var request = new UpdateEndToEndRequest
                {
                    Manifest = prepared.Manifest,
                    PackagePath = prepared.PackagePath,
                    ExpectedSha256 = prepared.ExpectedSha256,
                    PackageVerified = true,
                    InstallDirectory = prepared.InstallDirectory,
                    StartApplicationAfterInstall = startApplicationAfterInstall
                };

                string requestsDir = Path.Combine(UpdateDownloadStorage.CarpetaUpdates, "requests");
                Directory.CreateDirectory(requestsDir);
                string requestPath = Path.Combine(
                    requestsDir,
                    $"request-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.json");

                File.WriteAllText(requestPath, JsonSerializer.Serialize(request, JsonOptions));

                // WorkingDirectory = carpeta del manager (sus propias DLLs), NO el install root.
                // Así UpdateManager no bloquea BLL.dll/CORE.dll del POS al reemplazarlos.
                string umDir = Path.GetDirectoryName(umPath) ?? prepared.InstallDirectory;
                var psi = new ProcessStartInfo
                {
                    FileName = umPath,
                    Arguments = $"--request \"{requestPath}\"",
                    WorkingDirectory = umDir,
                    UseShellExecute = true
                };

                Process.Start(psi);

                return UpdateLaunchResult.Create(
                    UpdateLaunchStatus.Launched,
                    "UpdateManager iniciado desde runtime aislado. La UI debe cerrarse para liberar binarios del POS.",
                    prepared.Manifest,
                    prepared.PackagePath,
                    requestPath,
                    umPath);
            }
            catch (Exception ex)
            {
                return UpdateLaunchResult.Create(UpdateLaunchStatus.Failed, ex.Message, prepared.Manifest, prepared.PackagePath);
            }
        }

        /// <summary>
        /// Preferir UpdateManager\UpdateManager.exe (runtime aislado). Fallback: raíz (legacy).
        /// </summary>
        public static string? ResolveUpdateManagerPath(string installDirectory)
        {
            string nested = Path.Combine(installDirectory, "UpdateManager", AllowedUpdatePackageFiles.UpdateManagerExe);
            if (File.Exists(nested))
                return nested;

            string root = Path.Combine(installDirectory, AllowedUpdatePackageFiles.UpdateManagerExe);
            if (File.Exists(root))
                return root;

            return null;
        }
    }
}
