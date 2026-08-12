using BLL.Update;
using CORE.Update;
using DL.Migrations;

string root = Path.Combine(Path.GetTempPath(), "MFFITNESS-E2E-10B-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);

int fails = 0;
try
{
    fails += T01_HappyPath(root);
    fails += T02_BackupFail(root);
    fails += T03_SnapshotFail(root);
    fails += T04_CajaAbierta(root);
    fails += T05_PackageInvalid(root);
    fails += T06_InstallFirstFail(root);
    fails += T07_InstallMiddleFail(root);
    fails += T08_InstallRecoveryFail(root);
    fails += T09_MigrationFail(root);
    fails += T10_PartialMigration(root);
    fails += T11_DbRestoreFail(root);
    fails += T12_HealthDbMismatch(root);
    fails += T13_HealthFailAfterMigration(root);
    fails += T14_CannotCompletedWrongApp(root);
    fails += T15_CannotCompletedWrongDb(root);
    fails += T16_CannotCompletedHealthFail(root);
    fails += T17_ApplyUpTo5Only(root);
    fails += T18_ApplyUpTo6(root);
    fails += T19_MissingMigration(root);
    fails += T20_ConcurrentBlocked(root);
    fails += T21_CrashDuringInstall(root);
    fails += T22_CrashDuringMigration(root);
    fails += T23_SessionCorruption(root);
    fails += T24_RecoverIdempotent(root);
    fails += T25_TerminalMatrix(root);
}
finally
{
    try { Directory.Delete(root, recursive: true); } catch { /* ignore */ }
}

Console.WriteLine();
Console.WriteLine(fails == 0 ? "ALL TESTS PASSED" : $"FAILED TESTS: {fails}");
Console.WriteLine("NOTE: Offline fakes. No real SQL. No business data. FASE 10B E2E.");
Environment.Exit(fails == 0 ? 0 : 1);

static int Fail(string n, string r) { Console.WriteLine($"FAIL {n}: {r}"); return 1; }
static int Pass(string n) { Console.WriteLine($"PASS {n}"); return 0; }

static int T01_HappyPath(string root)
{
    const string n = "01_HappyPath";
    try
    {
        var w = new FakeWorld(root, n);
        var r = w.Run();
        if (!r.Success || r.Status != UpdateSessionStatus.Completed) return Fail(n, r.Message);
        if (r.AppVersionAfter != "1.1.0" || r.DbVersionAfter != 6) return Fail(n, "pair");
        if (!r.HealthCheckPassed) return Fail(n, "health");
        if (r.RecoveryStatus != UpdateRecoveryStatus.None) return Fail(n, "recovery");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T02_BackupFail(string root)
{
    const string n = "02_BackupFail";
    try
    {
        var w = new FakeWorld(root, n) { BackupOk = false };
        var r = w.Run();
        if (r.Status == UpdateSessionStatus.Completed) return Fail(n, "Completed");
        if (w.AppVersion != "1.0.0" || w.DbVersion != 4) return Fail(n, "side effects");
        if (r.Status != UpdateSessionStatus.Failed) return Fail(n, r.Status.ToString());
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T03_SnapshotFail(string root)
{
    const string n = "03_SnapshotFail";
    try
    {
        var w = new FakeWorld(root, n) { SnapshotOk = false };
        var r = w.Run();
        if (w.AppVersion != "1.0.0" || w.DbVersion != 4) return Fail(n, "side effects");
        if (r.Status != UpdateSessionStatus.Failed) return Fail(n, r.Status.ToString());
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T04_CajaAbierta(string root)
{
    const string n = "04_CajaAbierta";
    try
    {
        var w = new FakeWorld(root, n) { CajaAbierta = true };
        var r = w.Run();
        if (!r.Blocked || r.Status != UpdateSessionStatus.Blocked) return Fail(n, r.Message);
        if (w.DbVersion != 4) return Fail(n, "migrated");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T05_PackageInvalid(string root)
{
    const string n = "05_PackageInvalid";
    try
    {
        var w = new FakeWorld(root, n) { PackageVerifyOk = false };
        var r = w.Run();
        if (!r.Blocked) return Fail(n, r.Message);
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T06_InstallFirstFail(string root)
{
    const string n = "06_InstallFirstFail";
    try
    {
        var w = new FakeWorld(root, n) { FailInstallOn = "UI.exe" };
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status + " " + r.Message);
        if (w.AppVersion != "1.0.0" || w.DbVersion != 4) return Fail(n, "not OLD+OLD");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T07_InstallMiddleFail(string root)
{
    const string n = "07_InstallMiddleFail";
    try
    {
        var w = new FakeWorld(root, n) { FailInstallOn = "DL.dll" };
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status.ToString());
        if (w.DbVersion != 4) return Fail(n, "db touched");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T08_InstallRecoveryFail(string root)
{
    const string n = "08_InstallRecoveryFail";
    try
    {
        var w = new FakeWorld(root, n) { FailInstallOn = "BLL.dll", RestoreSnapshotOk = false };
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecoveryRequired) return Fail(n, r.Status.ToString());
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T09_MigrationFail(string root)
{
    const string n = "09_MigrationFail";
    try
    {
        var w = new FakeWorld(root, n) { MigrateOk = false, MigrateFailAt = 5 };
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status + " " + r.Message);
        if (w.DbRestore.CallCount < 1) return Fail(n, "no db restore");
        if (w.AppVersion != "1.0.0") return Fail(n, "app");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T10_PartialMigration(string root)
{
    const string n = "10_PartialMigration";
    try
    {
        var w = new FakeWorld(root, n) { MigrateFailAt = 6 };
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status + " " + r.Message);
        if (w.DbRestore.CallCount < 1) return Fail(n, "no restore");
        if (r.DbVersionAfter != 4) return Fail(n, "session DbAfter=" + r.DbVersionAfter);
        if (r.AppVersionAfter != "1.0.0") return Fail(n, "app after");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T11_DbRestoreFail(string root)
{
    const string n = "11_DbRestoreFail";
    try
    {
        var w = new FakeWorld(root, n) { MigrateFailAt = 5 };
        w.DbRestore.ShouldSucceed = false;
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecoveryRequired) return Fail(n, r.Status.ToString());
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T12_HealthDbMismatch(string root)
{
    const string n = "12_HealthDbMismatch";
    try
    {
        var w = new FakeWorld(root, n) { HealthDbForce = 5 };
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status + " " + r.Message);
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T13_HealthFailAfterMigration(string root)
{
    const string n = "13_HealthFailAfterMig";
    try
    {
        var w = new FakeWorld(root, n) { HealthOverrideFail = true };
        var r = w.Run();
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status.ToString());
        if (w.DbRestore.CallCount < 1) return Fail(n, "expected db restore");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T14_CannotCompletedWrongApp(string root)
{
    const string n = "14_NoCompletedWrongApp";
    try
    {
        var w = new FakeWorld(root, n);
        var h = w.Hooks();
        int calls = 0;
        var custom = new UpdateEndToEndHooks
        {
            AcquireMutex = false,
            GetCurrentAppVersion = () => w.AppVersion,
            GetCurrentDbVersion = () => w.DbVersion,
            IsCajaAbierta = () => false,
            GetAvailableDiskBytes = () => long.MaxValue,
            PackageContainsUpdateManager = _ => false,
            VerifyPackage = h.VerifyPackage,
            ExtractPackage = h.ExtractPackage,
            CreateBackup = h.CreateBackup,
            CreateSnapshot = h.CreateSnapshot,
            RestoreSnapshot = h.RestoreSnapshot,
            InstallFiles = h.InstallFiles,
            ApplyUpTo = h.ApplyUpTo,
            DatabaseRestore = w.DbRestore,
            ProcessController = w.Proc,
            ApplicationLauncher = w.Launcher,
            VerifyInstalledVersion = (i, m) =>
            {
                calls++;
                if (calls >= 2)
                    return (true, "0.0.1", null);
                return (true, "1.1.0", null);
            },
            RunHealthCheck = (i, m) => new UpdateHealthCheckService.HealthCheckResult
            {
                Success = true,
                InstalledAppVersion = "1.1.0",
                Message = "ok"
            }
        };
        File.WriteAllText(Path.Combine(w.Root, "backup.bak"), "bak");
        var r = new UpdateEndToEndOrchestrator(custom, _ => { }, w.Storage()).Run(w.Request());
        if (r.Status == UpdateSessionStatus.Completed) return Fail(n, "Completed with wrong app");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T15_CannotCompletedWrongDb(string root)
{
    const string n = "15_NoCompletedWrongDb";
    try
    {
        var w = new FakeWorld(root, n);
        var h = w.Hooks();
        int healthCalls = 0;
        var custom = new UpdateEndToEndHooks
        {
            AcquireMutex = false,
            GetCurrentAppVersion = () => w.AppVersion,
            GetCurrentDbVersion = () =>
            {
                if (healthCalls >= 1 && w.DbVersion == 6) return 5;
                return w.DbVersion;
            },
            IsCajaAbierta = () => false,
            GetAvailableDiskBytes = () => long.MaxValue,
            PackageContainsUpdateManager = _ => false,
            VerifyPackage = h.VerifyPackage,
            ExtractPackage = h.ExtractPackage,
            CreateBackup = h.CreateBackup,
            CreateSnapshot = h.CreateSnapshot,
            RestoreSnapshot = h.RestoreSnapshot,
            InstallFiles = h.InstallFiles,
            ApplyUpTo = h.ApplyUpTo,
            DatabaseRestore = w.DbRestore,
            ProcessController = w.Proc,
            ApplicationLauncher = w.Launcher,
            VerifyInstalledVersion = (i, m) => (true, "1.1.0", null),
            RunHealthCheck = (i, m) =>
            {
                healthCalls++;
                return new UpdateHealthCheckService.HealthCheckResult
                {
                    Success = true,
                    InstalledAppVersion = "1.1.0",
                    Message = "ok"
                };
            }
        };
        File.WriteAllText(Path.Combine(w.Root, "backup.bak"), "bak");
        var r = new UpdateEndToEndOrchestrator(custom, _ => { }, w.Storage()).Run(w.Request());
        if (r.Status == UpdateSessionStatus.Completed) return Fail(n, "Completed with wrong db");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T16_CannotCompletedHealthFail(string root)
{
    const string n = "16_NoCompletedHealthFail";
    try
    {
        var w = new FakeWorld(root, n) { HealthOverrideFail = true };
        var r = w.Run();
        if (r.Status == UpdateSessionStatus.Completed) return Fail(n, "Completed");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T17_ApplyUpTo5Only(string root)
{
    const string n = "17_ApplyUpTo5";
    try
    {
        var w = new FakeWorld(root, n) { AvailableMigrations = new List<int> { 5, 6, 7 } };
        var result = w.Hooks().ApplyUpTo!(5, null);
        if (!result.Success) return Fail(n, result.Message);
        if (result.AppliedVersions.Count != 1 || result.AppliedVersions[0] != 5)
            return Fail(n, string.Join(",", result.AppliedVersions));
        if (result.FinalVersion != 5) return Fail(n, "final");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T18_ApplyUpTo6(string root)
{
    const string n = "18_ApplyUpTo6";
    try
    {
        var w = new FakeWorld(root, n) { DbVersion = 4, AvailableMigrations = new List<int> { 5, 6, 7 } };
        var result = w.Hooks().ApplyUpTo!(6, null);
        if (!result.Success) return Fail(n, result.Message);
        if (!result.AppliedVersions.SequenceEqual(new[] { 5, 6 }))
            return Fail(n, string.Join(",", result.AppliedVersions));
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T19_MissingMigration(string root)
{
    const string n = "19_MissingMigration";
    try
    {
        var w = new FakeWorld(root, n) { AvailableMigrations = new List<int> { 5, 7 } };
        var r = w.Run(targetDb: 6);
        if (r.Success) return Fail(n, "should fail");
        if (r.Status is not (UpdateSessionStatus.FailedRecovered or UpdateSessionStatus.Failed))
            return Fail(n, r.Status.ToString());
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T20_ConcurrentBlocked(string root)
{
    const string n = "20_Concurrent";
    try
    {
        var a = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
        if (!a.Acquired || a.Lock == null) return Fail(n, "A " + a.Message);
        try
        {
            UpdateManagerLockResult? b = null;
            var t = new Thread(() => b = UpdateManagerLock.TryAcquire(TimeSpan.Zero));
            t.Start();
            t.Join(5000);
            if (b == null || b.Acquired || !b.Blocked) return Fail(n, "B not blocked");

            var w = new FakeWorld(root, n);
            var hooks = new UpdateEndToEndHooks
            {
                AcquireMutex = true,
                GetCurrentAppVersion = () => "1.0.0",
                GetCurrentDbVersion = () => 4,
                IsCajaAbierta = () => false,
                VerifyPackage = _ => new UpdatePackageVerifier.VerifyResult { Success = true, Message = "ok" }
            };
            var r = new UpdateEndToEndOrchestrator(hooks, _ => { }, w.Storage()).Run(w.Request());
            if (!r.Blocked) return Fail(n, "orchestrator not blocked");
            return Pass(n);
        }
        finally { a.Lock.Dispose(); }
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T21_CrashDuringInstall(string root)
{
    const string n = "21_CrashInstall";
    try
    {
        var w = new FakeWorld(root, n);
        var storage = w.Storage();
        var session = storage.Create(FakeWorld.Manifest(), w.Package, new string('a', 64), true, w.Install);
        session.CurrentStage = UpdateEndToEndStage.BinariesInstalled;
        session.AppVersionBefore = "1.0.0";
        session.DbVersionBefore = 4;
        session.SnapshotPath = Path.Combine(w.Snapshots, "s1");
        Directory.CreateDirectory(session.SnapshotPath);
        foreach (var f in new[] { "UI.exe", "UI.dll", "BLL.dll", "DL.dll", "DTO.dll", "CORE.dll" })
            File.WriteAllText(Path.Combine(session.SnapshotPath!, f), "OLD_" + f);
        session.SnapshotVerified = true;
        session.BackupPath = Path.Combine(w.Root, "backup.bak");
        File.WriteAllText(session.BackupPath, "bak");
        session.BackupVerified = true;
        storage.Save(session);

        w.AppVersion = "1.1.0";
        w.WriteInstall("NEW");
        var r = new UpdateEndToEndOrchestrator(w.Hooks(), _ => { }, storage).Recover(session);
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status + " " + r.Message);
        if (w.AppVersion != "1.0.0") return Fail(n, "app not restored");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T22_CrashDuringMigration(string root)
{
    const string n = "22_CrashMigration";
    try
    {
        var w = new FakeWorld(root, n);
        var storage = w.Storage();
        var session = storage.Create(FakeWorld.Manifest(), w.Package, new string('a', 64), true, w.Install);
        session.CurrentStage = UpdateEndToEndStage.DbMigrated;
        session.AppVersionBefore = "1.0.0";
        session.AppVersionAfter = "1.1.0";
        session.DbVersionBefore = 4;
        session.DbVersionAfter = 5;
        session.BackupPath = Path.Combine(w.Root, "backup.bak");
        File.WriteAllText(session.BackupPath, "bak");
        session.BackupVerified = true;
        session.SnapshotPath = Path.Combine(w.Snapshots, "s1");
        Directory.CreateDirectory(session.SnapshotPath);
        foreach (var f in new[] { "UI.exe", "UI.dll", "BLL.dll", "DL.dll", "DTO.dll", "CORE.dll" })
            File.WriteAllText(Path.Combine(session.SnapshotPath!, f), "OLD_" + f);
        session.SnapshotVerified = true;
        storage.Save(session);

        w.AppVersion = "1.1.0";
        w.DbVersion = 5;
        var r = new UpdateEndToEndOrchestrator(w.Hooks(), _ => { }, storage).Recover(session);
        if (r.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, r.Status + " " + r.Message);
        if (w.DbRestore.CallCount < 1) return Fail(n, "no db restore");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T23_SessionCorruption(string root)
{
    const string n = "23_SessionCorruption";
    try
    {
        var dir = Path.Combine(root, n, "sessions");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "corrupt.json"), "{ not json");
        var decision = UpdateSessionGuard.Evaluate(new UpdateSessionStorage(dir));
        if (!decision.BlockStartup) return Fail(n, "should block");
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T24_RecoverIdempotent(string root)
{
    const string n = "24_RecoverIdempotent";
    try
    {
        var w = new FakeWorld(root, n);
        var storage = w.Storage();
        var session = storage.Create(FakeWorld.Manifest(), w.Package, new string('a', 64), true, w.Install);
        session.CurrentStage = UpdateEndToEndStage.Checking;
        storage.Save(session);
        var orch = new UpdateEndToEndOrchestrator(w.Hooks(), _ => { }, storage);
        var r1 = orch.Recover(session);
        if (r1.Status != UpdateSessionStatus.Failed) return Fail(n, "r1 " + r1.Status);
        var loaded = storage.Load(session.UpdateId)!;
        _ = orch.Recover(loaded);
        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}

static int T25_TerminalMatrix(string root)
{
    const string n = "25_TerminalMatrix";
    try
    {
        var w1 = new FakeWorld(root, n + "_ok");
        var ok = w1.Run();
        if (ok.Status != UpdateSessionStatus.Completed || ok.AppVersionAfter != "1.1.0" || ok.DbVersionAfter != 6)
            return Fail(n, "happy matrix");

        var w2 = new FakeWorld(root, n + "_fr") { FailInstallOn = "UI.exe" };
        var fr = w2.Run();
        if (fr.Status != UpdateSessionStatus.FailedRecovered) return Fail(n, "fr status");
        if (fr.AppVersionAfter != "1.0.0") return Fail(n, "fr app");
        if (w2.DbVersion != 4) return Fail(n, "fr db");

        var w3 = new FakeWorld(root, n + "_frr") { FailInstallOn = "UI.exe", RestoreSnapshotOk = false };
        var frr = w3.Run();
        if (frr.Status != UpdateSessionStatus.FailedRecoveryRequired) return Fail(n, "frr");

        return Pass(n);
    }
    catch (Exception ex) { return Fail(n, ex.Message); }
}
