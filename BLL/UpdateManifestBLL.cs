using CORE;
using CORE.Update;

namespace BLL
{
    /// <summary>
    /// Facade read-only para comprobar disponibilidad de updates.
    /// No crea backup ni ejecuta migraciones.
    /// </summary>
    public static class UpdateManifestBLL
    {
        public static UpdateManifestValidationResult Validate(UpdateManifest? manifest) =>
            UpdateManifestValidator.Validate(manifest);

        public static UpdateAvailability Evaluate(
            UpdateManifest? manifest,
            string? currentAppVersion = null,
            int? currentDbVersion = null)
        {
            string app = string.IsNullOrWhiteSpace(currentAppVersion)
                ? AppVersion.SemanticVersion
                : currentAppVersion.Trim();

            int db = currentDbVersion ?? SchemaMigrationBLL.GetCurrentDbVersion();
            return UpdateAvailabilityEvaluator.Evaluate(manifest, app, db);
        }
    }
}
