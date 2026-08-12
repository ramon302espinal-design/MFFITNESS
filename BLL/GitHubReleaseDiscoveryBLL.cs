using BLL.Update;
using CORE;
using CORE.Update;

namespace BLL
{
    /// <summary>
    /// Discovery de releases: GitHub → UpdateManifest → Validator → Availability.
    /// Solo lectura. No descarga el paquete ZIP ni ejecuta migraciones/backup.
    /// </summary>
    public static class GitHubReleaseDiscoveryBLL
    {
        public sealed class DiscoveryResult
        {
            public GitHubReleaseResult Release { get; init; } = GitHubReleaseResult.Fail(
                GitHubReleaseStatus.NetworkError, "Sin resultado.");

            public UpdateAvailability? Availability { get; init; }
            public UpdateManifestValidationResult? Validation { get; init; }
        }

        public static async Task<DiscoveryResult> DiscoverLatestAsync(
            IGitHubReleaseClient client,
            string? currentAppVersion = null,
            int? currentDbVersion = null,
            CancellationToken cancellationToken = default)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            GitHubReleaseResult release = await client
                .GetLatestStableReleaseManifestAsync(cancellationToken)
                .ConfigureAwait(false);

            return EvaluateRelease(release, currentAppVersion, currentDbVersion);
        }

        public static async Task<DiscoveryResult> DiscoverByTagAsync(
            IGitHubReleaseClient client,
            string tag,
            string? currentAppVersion = null,
            int? currentDbVersion = null,
            CancellationToken cancellationToken = default)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            GitHubReleaseResult release = await client
                .GetReleaseManifestByTagAsync(tag, cancellationToken)
                .ConfigureAwait(false);

            return EvaluateRelease(release, currentAppVersion, currentDbVersion);
        }

        /// <summary>
        /// Cliente real HTTPS contra el repo del producto (sin token).
        /// </summary>
        public static GitHubReleaseClient CreateDefaultClient(TimeSpan? timeout = null) =>
            new(timeout: timeout);

        private static DiscoveryResult EvaluateRelease(
            GitHubReleaseResult release,
            string? currentAppVersion,
            int? currentDbVersion)
        {
            string app = string.IsNullOrWhiteSpace(currentAppVersion)
                ? AppVersion.SemanticVersion
                : currentAppVersion.Trim();

            // Read-only: si no pasan DB, no consultar SQL (smoke/tests).
            int db = currentDbVersion ?? 1;

            if (release.Status != GitHubReleaseStatus.Success || release.Manifest == null)
            {
                return new DiscoveryResult { Release = release };
            }

            var validation = UpdateManifestValidator.Validate(release.Manifest);
            if (!validation.IsValid)
            {
                return new DiscoveryResult
                {
                    Release = GitHubReleaseResult.Fail(
                        GitHubReleaseStatus.InvalidManifest,
                        "Manifest inválido: " + string.Join(" ", validation.Errors),
                        release.HttpStatusCode,
                        release.TagName),
                    Validation = validation,
                    Availability = UpdateAvailability.Invalid(
                        "Manifest inválido: " + string.Join(" ", validation.Errors),
                        app,
                        db)
                };
            }

            // Read-only opcional: solo consulta SchemaVersion si el caller no pasó currentDbVersion.
            // Los smoke tests siempre pasan currentDbVersion explícito para no tocar SQL.
            if (currentDbVersion == null)
            {
                try { db = SchemaMigrationBLL.GetCurrentDbVersion(); }
                catch { db = 1; }
            }
            else
            {
                db = currentDbVersion.Value;
            }

            var availability = UpdateAvailabilityEvaluator.Evaluate(release.Manifest, app, db);
            return new DiscoveryResult
            {
                Release = release,
                Validation = validation,
                Availability = availability
            };
        }
    }
}
