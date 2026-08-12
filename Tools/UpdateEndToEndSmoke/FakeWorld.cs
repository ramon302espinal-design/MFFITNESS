using BLL.Update;
using CORE.Update;
using DL.Backup;
using DL.Migrations;

internal sealed class FakeWorld
{
    public string Root;
    public string Sessions;
    public string Install;
    public string Staging;
    public string Snapshots;
    public string Package;
    public string AppVersion = "1.0.0";
    public int DbVersion = 4;
    public bool CajaAbierta;
    public bool BackupOk = true;
    public bool SnapshotOk = true;
    public bool RestoreSnapshotOk = true;
    public bool InstallOk = true;
    public string? FailInstallOn;
    public bool MigrateOk = true;
    public int? MigrateFailAt;
    public List<int> Applied = new();
    public FakeDatabaseRestoreService DbRestore = new();
    public FakeUpdateDbHealthProbe DbProbe = new() { SchemaVersion = 4 };
    public FakeUpdateProcessController Proc = new();
    public FakeUpdateApplicationLauncher Launcher = new();
    public bool HealthOverrideFail;
    public string? HealthAppVersionOverride = null;
    public int? HealthDbForce;
    public bool PackageVerifyOk = true;
    public List<int> AvailableMigrations = new() { 5, 6, 7 };

    public FakeWorld(string root, string name)
    {
        Root = Path.Combine(root, name);
        Sessions = Path.Combine(Root, "sessions");
        Install = Path.Combine(Root, "install");
        Staging = Path.Combine(Root, "staging");
        Snapshots = Path.Combine(Root, "snapshots");
        Package = Path.Combine(Root, "MFFITNESS-1.1.0.zip");
        Directory.CreateDirectory(Sessions);
        Directory.CreateDirectory(Install);
        Directory.CreateDirectory(Staging);
        Directory.CreateDirectory(Snapshots);
        File.WriteAllText(Package, "fake-zip-content");
        WriteInstall("OLD");
        DbRestore.SchemaVersionAfterRestore = 4;
        DbProbe.SchemaVersion = 4;
    }

    public static UpdateManifest Manifest(string app = "1.1.0", int db = 6) => new()
    {
        AppVersion = app,
        TargetDbVersion = db,
        MinAppVersion = "1.0.0",
        PackageName = "MFFITNESS-1.1.0.zip",
        PackageSha256 = new string('a', 64),
        ReleaseDate = new DateTime(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc)
    };

    public void WriteInstall(string tag)
    {
        Directory.CreateDirectory(Install);
        foreach (var f in new[] { "UI.exe", "UI.dll", "BLL.dll", "DL.dll", "DTO.dll", "CORE.dll" })
            File.WriteAllText(Path.Combine(Install, f), tag + "_" + f);
    }

    public UpdateSessionStorage Storage() => new(Sessions);

    public UpdateEndToEndRequest Request(int targetDb = 6) => new()
    {
        Manifest = Manifest(db: targetDb),
        PackagePath = Package,
        ExpectedSha256 = new string('a', 64),
        PackageVerified = PackageVerifyOk,
        InstallDirectory = Install,
        StagingDirectory = Path.Combine(Staging, Guid.NewGuid().ToString("N")),
        SnapshotDirectory = Snapshots,
        SessionsDirectory = Sessions,
        MigrationsDirectoryOverride = Path.Combine(Staging, "migrations"),
        StartApplicationAfterInstall = true
    };

    public UpdateEndToEndHooks Hooks()
    {
        return new UpdateEndToEndHooks
        {
            AcquireMutex = false,
            GetCurrentAppVersion = () => AppVersion,
            GetCurrentDbVersion = () => DbVersion,
            IsCajaAbierta = () => CajaAbierta,
            IsMigrationRunning = () => false,
            HasCriticalOperation = () => false,
            GetAvailableDiskBytes = () => 10L * 1024 * 1024 * 1024,
            PackageContainsUpdateManager = _ => false,
            VerifyPackage = _ => PackageVerifyOk
                ? new UpdatePackageVerifier.VerifyResult { Success = true, Message = "ok", ComputedSha256 = new string('a', 64) }
                : new UpdatePackageVerifier.VerifyResult { Success = false, Message = "package invalid" },
            ExtractPackage = (zip, staging) =>
            {
                Directory.CreateDirectory(staging);
                Directory.CreateDirectory(Path.Combine(staging, "Database", "Migrations"));
                foreach (var f in new[] { "UI.exe", "UI.dll", "BLL.dll", "DL.dll", "DTO.dll", "CORE.dll" })
                    File.WriteAllText(Path.Combine(staging, f), "NEW_" + f);
                return new UpdatePackageExtractor.ExtractResult
                {
                    Success = true,
                    Message = "extracted",
                    StagingDirectory = staging
                };
            },
            CreateBackup = () => BackupOk
                ? new DatabaseBackupResult
                {
                    Success = true,
                    Verified = true,
                    BackupPath = Path.Combine(Root, "backup.bak"),
                    DatabaseName = "FakeDb",
                    CreatedAt = DateTime.UtcNow,
                    SizeBytes = 100
                }
                : DatabaseBackupResult.Fail("FakeDb", DateTime.UtcNow, "backup fail"),
            CreateSnapshot = (install, snapRoot) =>
            {
                if (!SnapshotOk)
                    return new UpdateBinarySnapshotService.SnapshotResult { Success = false, Message = "snapshot fail" };
                string dir = Path.Combine(snapRoot, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(dir);
                foreach (var f in Directory.GetFiles(install))
                    File.Copy(f, Path.Combine(dir, Path.GetFileName(f)), true);
                var lastSnap = new UpdateSnapshotInfo
                {
                    SnapshotId = Path.GetFileName(dir),
                    SnapshotDirectory = dir,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    AppVersion = AppVersion,
                    InstallDirectory = install,
                    Files = Directory.GetFiles(dir).Select(p => new UpdateSnapshotFileEntry
                    {
                        RelativePath = Path.GetFileName(p),
                        SizeBytes = 1,
                        Sha256 = "x"
                    }).ToList()
                };
                File.WriteAllText(Path.Combine(dir, "snapshot.json"), "{}");
                return new UpdateBinarySnapshotService.SnapshotResult
                {
                    Success = true,
                    Message = "ok",
                    Snapshot = lastSnap
                };
            },
            RestoreSnapshot = (snap, install, _) =>
            {
                if (!RestoreSnapshotOk)
                    return new UpdateBinarySnapshotService.RestoreResult { Success = false, Message = "restore snap fail" };
                foreach (var f in Directory.GetFiles(snap.SnapshotDirectory))
                {
                    string name = Path.GetFileName(f);
                    if (name.Equals("snapshot.json", StringComparison.OrdinalIgnoreCase)) continue;
                    File.Copy(f, Path.Combine(install, name), true);
                }
                AppVersion = "1.0.0";
                return new UpdateBinarySnapshotService.RestoreResult { Success = true, Message = "restored" };
            },
            InstallFiles = (staging, install) =>
            {
                var installed = new List<string>();
                foreach (var f in new[] { "UI.exe", "UI.dll", "BLL.dll", "DL.dll", "DTO.dll", "CORE.dll" })
                {
                    if (FailInstallOn != null &&
                        string.Equals(FailInstallOn, f, StringComparison.OrdinalIgnoreCase))
                    {
                        return new UpdateBinaryInstaller.InstallResult
                        {
                            Success = false,
                            Message = "fail on " + f,
                            InstalledFiles = installed,
                            FailedOnFile = f
                        };
                    }
                    if (!InstallOk && FailInstallOn == null)
                    {
                        return new UpdateBinaryInstaller.InstallResult
                        {
                            Success = false,
                            Message = "install fail",
                            InstalledFiles = installed
                        };
                    }
                    File.Copy(Path.Combine(staging, f), Path.Combine(install, f), true);
                    installed.Add(f);
                }
                AppVersion = "1.1.0";
                return new UpdateBinaryInstaller.InstallResult
                {
                    Success = true,
                    Message = "ok",
                    InstalledFiles = installed
                };
            },
            ApplyUpTo = (target, dir) =>
            {
                Applied.Clear();
                int current = DbVersion;
                if (current > target)
                    return MigrationRunResult.Fail(current, current, "cannot downgrade");

                var pending = AvailableMigrations.Where(v => v > current && v <= target).OrderBy(v => v).ToList();
                for (int v = current + 1; v <= target; v++)
                {
                    if (!AvailableMigrations.Contains(v))
                        return MigrationRunResult.Fail(current, current, $"Faltan migraciones requeridas hasta target {target}: {v:0000}");
                }

                foreach (int v in pending)
                {
                    if (MigrateFailAt == v || (!MigrateOk && MigrateFailAt == null))
                    {
                        return MigrationRunResult.Fail(current, DbVersion, $"Falló {v:0000}_X.sql", $"{v:0000}_X.sql");
                    }
                    DbVersion = v;
                    Applied.Add(v);
                    DbProbe.SchemaVersion = v;
                }
                return MigrationRunResult.Ok(current, DbVersion, Applied.ToList(), "ok");
            },
            DatabaseRestore = DbRestore,
            ProcessController = Proc,
            ApplicationLauncher = Launcher,
            DbHealthProbe = DbProbe,
            LoadSnapshot = path =>
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                    return null;
                var files = Directory.GetFiles(path)
                    .Where(p => !Path.GetFileName(p).Equals("snapshot.json", StringComparison.OrdinalIgnoreCase))
                    .Select(p => new UpdateSnapshotFileEntry
                    {
                        RelativePath = Path.GetFileName(p),
                        SizeBytes = 1,
                        Sha256 = "x"
                    }).ToList();
                return new UpdateSnapshotInfo
                {
                    SnapshotId = Path.GetFileName(path),
                    SnapshotDirectory = path,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    AppVersion = "1.0.0",
                    InstallDirectory = Install,
                    Files = files
                };
            },
            VerifyInstalledVersion = (install, m) =>
            {
                string actual = HealthAppVersionOverride ?? AppVersion;
                bool ok = string.Equals(actual, m.AppVersion, StringComparison.OrdinalIgnoreCase);
                return (ok, actual, ok ? null : "app version mismatch");
            },
            RunHealthCheck = (install, m) =>
            {
                if (HealthOverrideFail)
                    return new UpdateHealthCheckService.HealthCheckResult { Success = false, Message = "health fail", InstalledAppVersion = AppVersion };

                string app = HealthAppVersionOverride ?? AppVersion;
                int db = HealthDbForce ?? DbVersion;
                DbProbe.SchemaVersion = db;
                bool ok = string.Equals(app, m.AppVersion, StringComparison.OrdinalIgnoreCase)
                          && db == m.TargetDbVersion
                          && !DbProbe.ForcePending;
                return new UpdateHealthCheckService.HealthCheckResult
                {
                    Success = ok,
                    Message = ok ? "OK" : "health mismatch",
                    InstalledAppVersion = app
                };
            }
        };
    }

    public UpdateEndToEndResult Run(int targetDb = 6)
    {
        if (BackupOk) File.WriteAllText(Path.Combine(Root, "backup.bak"), "bak");
        DbRestore.SchemaVersionAfterRestore = 4;
        var storage = Storage();
        var orch = new UpdateEndToEndOrchestrator(Hooks(), _ => { }, storage);
        return orch.Run(Request(targetDb));
    }
}
