namespace CORE.Update
{
    /// <summary>
    /// Solicitud de ejecución E2E. El paquete debe estar descargado y verificado (FASE 8).
    /// </summary>
    public sealed class UpdateEndToEndRequest
    {
        public required UpdateManifest Manifest { get; init; }
        public required string PackagePath { get; init; }
        public required string ExpectedSha256 { get; init; }
        public bool PackageVerified { get; init; }
        public required string InstallDirectory { get; init; }
        public string UiExecutableName { get; init; } = "UI.exe";
        public TimeSpan UiCloseTimeout { get; init; } = TimeSpan.FromSeconds(90);
        public bool StartApplicationAfterInstall { get; init; } = true;
        public string? StagingDirectory { get; init; }
        public string? SnapshotDirectory { get; init; }
        public string? SessionsDirectory { get; init; }
        public string? MigrationsDirectoryOverride { get; init; }
        /// <summary>Si se provee, reanuda/usa esa sesión en lugar de crear una nueva.</summary>
        public string? ExistingUpdateId { get; init; }
    }
}
