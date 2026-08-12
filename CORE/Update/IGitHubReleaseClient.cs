namespace CORE.Update
{
    /// <summary>
    /// Cliente de discovery de GitHub Releases (solo lectura de metadata/manifest).
    /// No descarga el paquete ZIP de la aplicación.
    /// </summary>
    public interface IGitHubReleaseClient
    {
        Task<GitHubReleaseResult> GetLatestStableReleaseManifestAsync(CancellationToken cancellationToken = default);

        Task<GitHubReleaseResult> GetReleaseManifestByTagAsync(string tag, CancellationToken cancellationToken = default);
    }
}
