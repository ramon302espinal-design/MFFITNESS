namespace CORE.Update
{
    public enum UpdateLaunchStatus
    {
        Available,
        NotAvailable,
        Incompatible,
        DiscoveryFailed,
        DownloadFailed,
        Prepared,
        Launched,
        Blocked,
        Failed
    }

    public sealed class UpdateLaunchResult
    {
        public UpdateLaunchStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public UpdateManifest? Manifest { get; init; }
        public string? PackagePath { get; init; }
        public string? RequestPath { get; init; }
        public string? UpdateManagerPath { get; init; }
        public UpdateAvailability? Availability { get; init; }

        public static UpdateLaunchResult Create(
            UpdateLaunchStatus status,
            string message,
            UpdateManifest? manifest = null,
            string? packagePath = null,
            string? requestPath = null,
            string? updateManagerPath = null,
            UpdateAvailability? availability = null) =>
            new()
            {
                Status = status,
                Message = message,
                Manifest = manifest,
                PackagePath = packagePath,
                RequestPath = requestPath,
                UpdateManagerPath = updateManagerPath,
                Availability = availability
            };
    }
}
