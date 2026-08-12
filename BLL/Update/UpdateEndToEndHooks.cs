using CORE.Update;
using DL.Backup;
using DL.Migrations;

namespace BLL.Update
{
    /// <summary>
    /// Hooks inyectables para smoke E2E offline (sin SQL real ni side effects productivos).
    /// </summary>
    public sealed class UpdateEndToEndHooks
    {
        public Func<string>? GetCurrentAppVersion { get; init; }
        public Func<int>? GetCurrentDbVersion { get; init; }
        public Func<bool>? IsCajaAbierta { get; init; }
        public Func<bool>? IsMigrationRunning { get; init; }
        public Func<bool>? HasCriticalOperation { get; init; }
        public Func<long>? GetAvailableDiskBytes { get; init; }
        public Func<UpdateInstallRequest, UpdatePackageVerifier.VerifyResult>? VerifyPackage { get; init; }
        public Func<string, string, UpdatePackageExtractor.ExtractResult>? ExtractPackage { get; init; }
        public Func<DatabaseBackupResult>? CreateBackup { get; init; }
        public Func<string, string, UpdateBinarySnapshotService.SnapshotResult>? CreateSnapshot { get; init; }
        public Func<UpdateSnapshotInfo, string, IReadOnlyList<string>?, UpdateBinarySnapshotService.RestoreResult>? RestoreSnapshot { get; init; }
        public Func<string, string, UpdateBinaryInstaller.InstallResult>? InstallFiles { get; init; }
        public Func<int, string?, MigrationRunResult>? ApplyUpTo { get; init; }
        public IDatabaseRestoreService? DatabaseRestore { get; init; }
        public IUpdateProcessController? ProcessController { get; init; }
        public IUpdateApplicationLauncher? ApplicationLauncher { get; init; }
        public IUpdateDbHealthProbe? DbHealthProbe { get; init; }
        public Func<string, UpdateManifest, UpdateHealthCheckService.HealthCheckResult>? RunHealthCheck { get; init; }
        public Func<string, UpdateManifest, (bool Ok, string? Actual, string? Error)>? VerifyInstalledVersion { get; init; }
        public Func<string, bool>? PackageContainsUpdateManager { get; init; }
        public Func<string, UpdateSnapshotInfo?>? LoadSnapshot { get; init; }
        public Action<string>? BeforeFileCopy { get; init; }
        /// <summary>Si false, el orquestador no intenta adquirir mutex (tests unitarios de flujo).</summary>
        public bool AcquireMutex { get; init; } = true;
    }
}
