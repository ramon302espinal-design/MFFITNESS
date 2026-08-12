namespace CORE.Update
{
    public sealed record UpdateInstallationResult
    {
        public bool Success { get; init; }
        public bool Blocked { get; init; }
        public UpdateInstallationStage Stage { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? ErrorMessage { get; init; }
        public string? SnapshotPath { get; init; }
        public string? StagingPath { get; init; }
        public string? InstalledAppVersion { get; init; }
        public string? ExpectedAppVersion { get; init; }
        public int? TargetDbVersion { get; init; }
        public bool? CajaAbierta { get; init; }
        public bool SnapshotCreated { get; init; }
        public bool PackageExtracted { get; init; }
        public bool FilesInstalled { get; init; }
        public bool HealthCheckPassed { get; init; }
        public bool ApplicationStarted { get; init; }
        public bool RecoveryAttempted { get; init; }
        public bool RecoverySucceeded { get; init; }
        public IReadOnlyList<string> InstalledFiles { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> RecoveredFiles { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> RecoveryFailedFiles { get; init; } = Array.Empty<string>();

        public static UpdateInstallationResult CreateBlocked(
            UpdateInstallationStage stage,
            string reason,
            bool? cajaAbierta = null) =>
            new()
            {
                Success = false,
                Blocked = true,
                Stage = stage,
                Message = reason,
                ErrorMessage = reason,
                CajaAbierta = cajaAbierta
            };

        public static UpdateInstallationResult CreateFailed(
            UpdateInstallationStage stage,
            string error,
            UpdateInstallationResult? partial = null) =>
            new()
            {
                Success = false,
                Blocked = false,
                Stage = stage == UpdateInstallationStage.FailedRecovered ||
                        stage == UpdateInstallationStage.FailedRecoveryRequired
                    ? stage
                    : UpdateInstallationStage.Failed,
                Message = error,
                ErrorMessage = error,
                SnapshotPath = partial?.SnapshotPath,
                StagingPath = partial?.StagingPath,
                InstalledAppVersion = partial?.InstalledAppVersion,
                ExpectedAppVersion = partial?.ExpectedAppVersion,
                TargetDbVersion = partial?.TargetDbVersion,
                CajaAbierta = partial?.CajaAbierta,
                SnapshotCreated = partial?.SnapshotCreated ?? false,
                PackageExtracted = partial?.PackageExtracted ?? false,
                FilesInstalled = partial?.FilesInstalled ?? false,
                HealthCheckPassed = partial?.HealthCheckPassed ?? false,
                ApplicationStarted = partial?.ApplicationStarted ?? false,
                RecoveryAttempted = partial?.RecoveryAttempted ?? false,
                RecoverySucceeded = partial?.RecoverySucceeded ?? false,
                InstalledFiles = partial?.InstalledFiles ?? Array.Empty<string>(),
                RecoveredFiles = partial?.RecoveredFiles ?? Array.Empty<string>(),
                RecoveryFailedFiles = partial?.RecoveryFailedFiles ?? Array.Empty<string>()
            };

        public static UpdateInstallationResult CreateFailedRecovered(
            string installError,
            UpdateInstallationResult state) =>
            new()
            {
                Success = false,
                Blocked = false,
                Stage = UpdateInstallationStage.FailedRecovered,
                Message = "Instalación fallida; binarios restaurados desde snapshot. " + installError,
                ErrorMessage = installError,
                SnapshotPath = state.SnapshotPath,
                StagingPath = state.StagingPath,
                ExpectedAppVersion = state.ExpectedAppVersion,
                TargetDbVersion = state.TargetDbVersion,
                CajaAbierta = state.CajaAbierta,
                SnapshotCreated = state.SnapshotCreated,
                PackageExtracted = state.PackageExtracted,
                FilesInstalled = false,
                RecoveryAttempted = true,
                RecoverySucceeded = true,
                InstalledFiles = state.InstalledFiles,
                RecoveredFiles = state.RecoveredFiles
            };

        public static UpdateInstallationResult CreateFailedRecoveryRequired(
            string installError,
            string recoveryError,
            UpdateInstallationResult state) =>
            new()
            {
                Success = false,
                Blocked = false,
                Stage = UpdateInstallationStage.FailedRecoveryRequired,
                Message = "Instalación fallida y restauración automática incompleta. " +
                          installError + " | Recovery: " + recoveryError,
                ErrorMessage = installError,
                SnapshotPath = state.SnapshotPath,
                StagingPath = state.StagingPath,
                ExpectedAppVersion = state.ExpectedAppVersion,
                TargetDbVersion = state.TargetDbVersion,
                CajaAbierta = state.CajaAbierta,
                SnapshotCreated = state.SnapshotCreated,
                PackageExtracted = state.PackageExtracted,
                FilesInstalled = false,
                ApplicationStarted = false,
                RecoveryAttempted = true,
                RecoverySucceeded = false,
                InstalledFiles = state.InstalledFiles,
                RecoveredFiles = state.RecoveredFiles,
                RecoveryFailedFiles = state.RecoveryFailedFiles
            };

        public static UpdateInstallationResult CreateCompleted(
            string message,
            UpdateInstallationResult state) =>
            new()
            {
                Success = true,
                Blocked = false,
                Stage = UpdateInstallationStage.Completed,
                Message = message,
                SnapshotPath = state.SnapshotPath,
                StagingPath = state.StagingPath,
                InstalledAppVersion = state.InstalledAppVersion,
                ExpectedAppVersion = state.ExpectedAppVersion,
                TargetDbVersion = state.TargetDbVersion,
                CajaAbierta = state.CajaAbierta,
                SnapshotCreated = state.SnapshotCreated,
                PackageExtracted = state.PackageExtracted,
                FilesInstalled = state.FilesInstalled,
                HealthCheckPassed = state.HealthCheckPassed,
                ApplicationStarted = state.ApplicationStarted,
                InstalledFiles = state.InstalledFiles
            };
    }
}
