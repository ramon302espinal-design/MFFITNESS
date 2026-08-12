namespace CORE.Update
{
    /// <summary>
    /// Sesión persistente de actualización end-to-end (FASE 10B).
    /// Ruta: %LocalAppData%\MFFITNESS\updates\sessions\{UpdateId}.json
    /// </summary>
    public sealed class UpdateSession
    {
        /// <summary>
        /// Versión del contrato JSON de la sesión (evolución futura).
        /// No confundir con SchemaVersion de la base de datos (DbVersion*).
        /// </summary>
        public int SchemaVersion { get; set; } = UpdateSessionContract.CurrentSchemaVersion;

        public string UpdateId { get; set; } = string.Empty;

        public UpdateSessionStatus Status { get; set; } = UpdateSessionStatus.Active;

        public UpdateEndToEndStage CurrentStage { get; set; } = UpdateEndToEndStage.Checking;

        public DateTime StartedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public DateTime LastHeartbeatUtc { get; set; }

        public string? AppVersionBefore { get; set; }

        public string? AppVersionTarget { get; set; }

        public string? AppVersionAfter { get; set; }

        public int? DbVersionBefore { get; set; }

        public int? DbVersionTarget { get; set; }

        public int? DbVersionAfter { get; set; }

        public UpdateManifest? Manifest { get; set; }

        public string? PackagePath { get; set; }

        public string? PackageSha256 { get; set; }

        public bool PackageVerified { get; set; }

        public string? InstallDirectory { get; set; }

        public string? StagingPath { get; set; }

        public string? BackupPath { get; set; }

        public bool BackupVerified { get; set; }

        public string? SnapshotPath { get; set; }

        public bool SnapshotVerified { get; set; }

        public string UiExecutableName { get; set; } = "UI.exe";

        public string? MigrationsDirectory { get; set; }

        public UpdateRecoveryStatus RecoveryStatus { get; set; } = UpdateRecoveryStatus.None;

        public List<string> RecoveryActions { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public List<string> CompensationLog { get; set; } = new();

        public UpdateSessionGates Gates { get; set; } = new();

        public bool IsTerminal =>
            Status is UpdateSessionStatus.Completed
                or UpdateSessionStatus.Blocked
                or UpdateSessionStatus.Failed
                or UpdateSessionStatus.FailedRecovered
                or UpdateSessionStatus.FailedRecoveryRequired
                or UpdateSessionStatus.RecoveryRequired;

        public bool IsCriticalRecovery =>
            Status is UpdateSessionStatus.FailedRecoveryRequired
                or UpdateSessionStatus.RecoveryRequired
            || RecoveryStatus is UpdateRecoveryStatus.Required or UpdateRecoveryStatus.Failed;
    }

    /// <summary>Constantes del contrato de sesión.</summary>
    public static class UpdateSessionContract
    {
        public const int CurrentSchemaVersion = 1;
    }
}
