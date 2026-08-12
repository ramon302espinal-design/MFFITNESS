using BLL.Update;
using CORE.Update;

namespace BLL
{
    /// <summary>
    /// Descarga y verificación SHA256 del paquete de actualización.
    /// No instala, no descomprime, no consulta BD ni ejecuta migraciones.
    /// </summary>
    public static class UpdatePackageDownloadBLL
    {
        public static async Task<PackageDownloadResult> DownloadAndVerifyAsync(
            UpdateManifest manifest,
            string downloadUrl,
            HttpClient? httpClient = null,
            TimeSpan? timeout = null,
            string? downloadDirectory = null,
            CancellationToken cancellationToken = default)
        {
            using var downloader = new UpdatePackageDownloader(httpClient, timeout, downloadDirectory);
            return await downloader
                .DownloadAndVerifyAsync(manifest, downloadUrl, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Usa manifest + PackageDownloadUrl de una release ya descubierta (FASE 7B).
        /// </summary>
        public static async Task<PackageDownloadResult> DownloadFromReleaseAsync(
            GitHubReleaseResult release,
            HttpClient? httpClient = null,
            TimeSpan? timeout = null,
            string? downloadDirectory = null,
            CancellationToken cancellationToken = default)
        {
            if (release.Manifest == null)
            {
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.InvalidManifest,
                    "Release sin manifest.");
            }

            if (string.IsNullOrWhiteSpace(release.PackageDownloadUrl))
            {
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.InvalidUrl,
                    $"Release sin asset '{release.Manifest.PackageName}'.");
            }

            return await DownloadAndVerifyAsync(
                    release.Manifest,
                    release.PackageDownloadUrl,
                    httpClient,
                    timeout,
                    downloadDirectory,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public static UpdatePackageDownloader CreateDownloader(
            HttpClient? httpClient = null,
            TimeSpan? timeout = null,
            string? downloadDirectory = null) =>
            new(httpClient, timeout, downloadDirectory);
    }
}
