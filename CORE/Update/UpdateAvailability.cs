namespace CORE.Update
{
    public enum UpdateAvailabilityStatus
    {
        Available,
        NotAvailable,
        Incompatible,
        InvalidManifest
    }

    public sealed class UpdateAvailability
    {
        public UpdateAvailabilityStatus Status { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string CurrentAppVersion { get; init; } = string.Empty;
        public int CurrentDbVersion { get; init; }
        public string? ManifestAppVersion { get; init; }
        public int? ManifestTargetDbVersion { get; init; }
        public string? ManifestMinAppVersion { get; init; }

        public static UpdateAvailability Invalid(string reason, string currentApp, int currentDb) =>
            new()
            {
                Status = UpdateAvailabilityStatus.InvalidManifest,
                Reason = reason,
                CurrentAppVersion = currentApp,
                CurrentDbVersion = currentDb
            };

        public static UpdateAvailability Create(
            UpdateAvailabilityStatus status,
            string reason,
            string currentApp,
            int currentDb,
            UpdateManifest manifest) =>
            new()
            {
                Status = status,
                Reason = reason,
                CurrentAppVersion = currentApp,
                CurrentDbVersion = currentDb,
                ManifestAppVersion = manifest.AppVersion,
                ManifestTargetDbVersion = manifest.TargetDbVersion,
                ManifestMinAppVersion = manifest.MinAppVersion
            };
    }
}
