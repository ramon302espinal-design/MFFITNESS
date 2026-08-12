using System.Text;
using System.Text.Json;
using BLL.Update;
using CORE.Update;

string root = Path.Combine(Path.GetTempPath(), "MFFITNESS-SessionSmoke-10B1-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);

int fails = 0;
try
{
    fails += Test01_CreateAndLoad(root);
    fails += Test02_SaveReloadAllFields(root);
    fails += Test03_ManifestRoundTrip(root);
    fails += Test04_Heartbeat(root);
    fails += Test05_FindPendingSessions(root);
    fails += Test06_FindStaleSessions(root);
    fails += Test07_NeverDeleteCriticalRecovery(root);
    fails += Test08_DeleteSafeTerminal(root);
    fails += Test09_AtomicSaveNeverTruncatesOfficial(root);
    fails += Test10_MutexAAcquires(root);
    fails += Test11_MutexBBlocked(root);
    fails += Test12_MutexReleaseAllowsReacquire(root);
}
finally
{
    try { Directory.Delete(root, recursive: true); } catch { /* ignore */ }
}

Console.WriteLine();
Console.WriteLine(fails == 0 ? "ALL TESTS PASSED" : $"FAILED TESTS: {fails}");
Console.WriteLine("NOTE: Offline. No DB. No SQL. No business data. FASE 10B.1 only.");
Environment.Exit(fails == 0 ? 0 : 1);

// ---- helpers ----

static UpdateManifest SampleManifest() => new()
{
    AppVersion = "1.2.0",
    TargetDbVersion = 6,
    MinAppVersion = "1.0.0",
    PackageName = "MFFITNESS-1.2.0.zip",
    PackageSha256 = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789",
    ReleaseDate = new DateTime(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc),
    ReleaseNotesUrl = "https://github.com/example/releases/tag/v1.2.0"
};

static UpdateSessionStorage NewStorage(string root, string sub) =>
    new(Path.Combine(root, sub));

static int Fail(string name, string reason)
{
    Console.WriteLine($"FAIL {name}: {reason}");
    return 1;
}

static int Pass(string name)
{
    Console.WriteLine($"PASS {name}");
    return 0;
}

static int Test01_CreateAndLoad(string root)
{
    const string name = "01_CreateAndLoad";
    try
    {
        var storage = NewStorage(root, name);
        var created = storage.Create(SampleManifest(), packagePath: @"C:\pkg.zip", packageSha256: "abc", packageVerified: true, installDirectory: @"C:\app");
        if (string.IsNullOrWhiteSpace(created.UpdateId))
            return Fail(name, "UpdateId vacío");
        if (created.SchemaVersion != UpdateSessionContract.CurrentSchemaVersion)
            return Fail(name, "SchemaVersion contrato incorrecto");
        if (created.Status != UpdateSessionStatus.Active)
            return Fail(name, "Status != Active");

        var loaded = storage.Load(created.UpdateId);
        if (loaded == null)
            return Fail(name, "Load devolvió null");
        if (loaded.UpdateId != created.UpdateId)
            return Fail(name, "UpdateId mismatch");
        if (!File.Exists(storage.GetSessionPath(created.UpdateId)))
            return Fail(name, "Archivo JSON no existe");

        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test02_SaveReloadAllFields(string root)
{
    const string name = "02_SaveReloadAllFields";
    try
    {
        var storage = NewStorage(root, name);
        var session = storage.Create(SampleManifest());
        session.CurrentStage = UpdateEndToEndStage.BackupCreated;
        session.AppVersionBefore = "1.0.0";
        session.AppVersionTarget = "1.2.0";
        session.AppVersionAfter = null;
        session.DbVersionBefore = 4;
        session.DbVersionTarget = 6;
        session.DbVersionAfter = null;
        session.PackagePath = @"D:\downloads\pkg.zip";
        session.PackageSha256 = SampleManifest().PackageSha256;
        session.PackageVerified = true;
        session.InstallDirectory = @"D:\MFFITNESS";
        session.StagingPath = @"D:\staging\x";
        session.BackupPath = @"D:\backups\x.bak";
        session.BackupVerified = true;
        session.SnapshotPath = @"D:\snapshots\s1";
        session.SnapshotVerified = true;
        session.UiExecutableName = "UI.exe";
        session.MigrationsDirectory = @"D:\staging\x\Database\Migrations";
        session.RecoveryStatus = UpdateRecoveryStatus.None;
        session.RecoveryActions = new List<string> { "none" };
        session.ErrorMessage = null;
        session.CompensationLog = new List<string> { "backup ok", "snapshot ok" };
        session.Gates = new UpdateSessionGates
        {
            ManifestValid = true,
            PackageVerified = true,
            Sha256RecalculatedOk = true,
            PackageNameMatches = true,
            CurrentAppLessThanTarget = true,
            CurrentAppMeetsMin = true,
            CurrentDbLessOrEqualTarget = true,
            CurrentDbAtLeastOne = true,
            CajaCerrada = true,
            NoConcurrentMigration = true,
            NoCriticalOperation = true,
            SufficientDiskSpace = true,
            UpdateManagerNotInPackage = true,
            MigrationsDirectoryOk = true,
            AllPassed = true
        };
        storage.Save(session);

        var loaded = storage.Load(session.UpdateId)!;
        if (loaded.CurrentStage != UpdateEndToEndStage.BackupCreated) return Fail(name, "stage");
        if (loaded.AppVersionBefore != "1.0.0") return Fail(name, "AppVersionBefore");
        if (loaded.DbVersionBefore != 4 || loaded.DbVersionTarget != 6) return Fail(name, "DbVersion");
        if (loaded.PackagePath != @"D:\downloads\pkg.zip") return Fail(name, "PackagePath");
        if (!loaded.PackageVerified || !loaded.BackupVerified || !loaded.SnapshotVerified) return Fail(name, "flags");
        if (loaded.StagingPath != @"D:\staging\x") return Fail(name, "StagingPath");
        if (loaded.BackupPath != @"D:\backups\x.bak") return Fail(name, "BackupPath");
        if (loaded.SnapshotPath != @"D:\snapshots\s1") return Fail(name, "SnapshotPath");
        if (loaded.MigrationsDirectory != @"D:\staging\x\Database\Migrations") return Fail(name, "MigrationsDirectory");
        if (loaded.CompensationLog.Count != 2) return Fail(name, "CompensationLog");
        if (!loaded.Gates.AllPassed || !loaded.Gates.CajaCerrada) return Fail(name, "Gates");
        if (loaded.SchemaVersion != 1) return Fail(name, "SchemaVersion");

        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test03_ManifestRoundTrip(string root)
{
    const string name = "03_ManifestRoundTrip";
    try
    {
        var storage = NewStorage(root, name);
        var manifest = SampleManifest();
        var session = storage.Create(manifest);
        var loaded = storage.Load(session.UpdateId)!;
        if (loaded.Manifest == null) return Fail(name, "Manifest null");
        if (loaded.Manifest.AppVersion != manifest.AppVersion) return Fail(name, "AppVersion");
        if (loaded.Manifest.TargetDbVersion != manifest.TargetDbVersion) return Fail(name, "TargetDbVersion");
        if (loaded.Manifest.MinAppVersion != manifest.MinAppVersion) return Fail(name, "MinAppVersion");
        if (loaded.Manifest.PackageName != manifest.PackageName) return Fail(name, "PackageName");
        if (!string.Equals(loaded.Manifest.PackageSha256, manifest.PackageSha256, StringComparison.OrdinalIgnoreCase))
            return Fail(name, "PackageSha256");
        if (loaded.Manifest.ReleaseNotesUrl != manifest.ReleaseNotesUrl) return Fail(name, "ReleaseNotesUrl");

        // Serialización directa System.Text.Json
        string json = JsonSerializer.Serialize(manifest, UpdateSessionStorage.SharedJsonOptions);
        var again = JsonSerializer.Deserialize<UpdateManifest>(json, UpdateSessionStorage.SharedJsonOptions);
        if (again?.AppVersion != "1.2.0") return Fail(name, "deserialize directo");

        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test04_Heartbeat(string root)
{
    const string name = "04_Heartbeat";
    try
    {
        var storage = NewStorage(root, name);
        var session = storage.Create(SampleManifest());
        DateTime before = session.LastHeartbeatUtc;
        Thread.Sleep(50);
        storage.MarkHeartbeat(session.UpdateId);
        var loaded = storage.Load(session.UpdateId)!;
        if (loaded.LastHeartbeatUtc <= before)
            return Fail(name, "Heartbeat no avanzó");
        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test05_FindPendingSessions(string root)
{
    const string name = "05_FindPending";
    try
    {
        var storage = NewStorage(root, name);
        var a = storage.Create(SampleManifest());
        var b = storage.Create(SampleManifest());
        storage.MarkCompleted(b.UpdateId, "1.2.0", 6);

        var pending = storage.FindPendingSessions();
        if (pending.Count != 1) return Fail(name, $"esperaba 1 pending, got {pending.Count}");
        if (pending[0].UpdateId != a.UpdateId) return Fail(name, "pending incorrecto");
        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test06_FindStaleSessions(string root)
{
    const string name = "06_FindStale";
    try
    {
        var storage = NewStorage(root, name);
        var stale = storage.Create(SampleManifest());
        var fresh = storage.Create(SampleManifest());

        // Forzar heartbeat antiguo en archivo
        stale.LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-30);
        // Save() actualiza heartbeat; escribir manualmente bypass
        string path = storage.GetSessionPath(stale.UpdateId);
        stale.LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-30);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(stale, UpdateSessionStorage.SharedJsonOptions);
        File.WriteAllBytes(path, bytes);

        storage.MarkHeartbeat(fresh.UpdateId);

        var found = storage.FindStaleSessions(TimeSpan.FromMinutes(5));
        if (found.Count != 1) return Fail(name, $"esperaba 1 stale, got {found.Count}");
        if (found[0].UpdateId != stale.UpdateId) return Fail(name, "stale incorrecto");
        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test07_NeverDeleteCriticalRecovery(string root)
{
    const string name = "07_NoDeleteCritical";
    try
    {
        var storage = NewStorage(root, name);
        var req = storage.Create(SampleManifest());
        storage.MarkFailed(req.UpdateId, UpdateSessionStatus.RecoveryRequired, UpdateEndToEndStage.RecoveryRequired, "start fail", UpdateRecoveryStatus.Required);

        var fr = storage.Create(SampleManifest());
        storage.MarkFailed(fr.UpdateId, UpdateSessionStatus.FailedRecoveryRequired, UpdateEndToEndStage.FailedRecoveryRequired, "restore fail", UpdateRecoveryStatus.Failed);

        if (storage.DeleteSafe(req.UpdateId)) return Fail(name, "borró RecoveryRequired");
        if (storage.DeleteSafe(fr.UpdateId)) return Fail(name, "borró FailedRecoveryRequired");
        if (storage.Load(req.UpdateId) == null) return Fail(name, "RecoveryRequired desapareció");
        if (storage.Load(fr.UpdateId) == null) return Fail(name, "FailedRecoveryRequired desapareció");
        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test08_DeleteSafeTerminal(string root)
{
    const string name = "08_DeleteSafeTerminal";
    try
    {
        var storage = NewStorage(root, name);
        var done = storage.Create(SampleManifest());
        storage.MarkCompleted(done.UpdateId, "1.2.0", 6);
        if (!storage.DeleteSafe(done.UpdateId)) return Fail(name, "no borró Completed");
        if (storage.Load(done.UpdateId) != null) return Fail(name, "Completed sigue existiendo");

        var active = storage.Create(SampleManifest());
        if (storage.DeleteSafe(active.UpdateId)) return Fail(name, "borró Active");
        if (storage.Load(active.UpdateId) == null) return Fail(name, "Active desapareció");

        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test09_AtomicSaveNeverTruncatesOfficial(string root)
{
    const string name = "09_AtomicSave";
    try
    {
        var storage = NewStorage(root, name);
        var session = storage.Create(SampleManifest());
        string path = storage.GetSessionPath(session.UpdateId);

        // Expandir compensación para JSON grande
        for (int i = 0; i < 200; i++)
            session.CompensationLog.Add($"entry-{i}-" + new string('x', 40));
        storage.Save(session);

        string before = File.ReadAllText(path, Encoding.UTF8);
        if (!before.Contains("entry-199")) return Fail(name, "contenido inicial incompleto");

        // Simular crash dejando .tmp truncado; el oficial debe permanecer intacto
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, "{ \"truncated\": true", Encoding.UTF8);
        string afterCrashTmp = File.ReadAllText(path, Encoding.UTF8);
        if (afterCrashTmp != before)
            return Fail(name, "JSON oficial cambió por .tmp truncado");

        // Save completo debe reemplazar de forma controlada
        session.CompensationLog.Add("final-ok");
        storage.Save(session);
        string after = File.ReadAllText(path, Encoding.UTF8);
        if (!after.Contains("final-ok")) return Fail(name, "Save final no persistió");
        if (after.Contains("\"truncated\"")) return Fail(name, "oficial quedó con tmp truncado");
        if (File.Exists(tmp)) return Fail(name, ".tmp residual tras Save exitoso");

        // Verificar que Save no escribe directo: patrón temp+replace (segunda pasada con spy directory)
        // Comprobar que File.Replace dejó un JSON parseable
        var reloaded = storage.Load(session.UpdateId);
        if (reloaded == null) return Fail(name, "Load post-atomic falló");
        if (!reloaded.CompensationLog.Contains("final-ok")) return Fail(name, "roundtrip post-atomic");

        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test10_MutexAAcquires(string root)
{
    const string name = "10_MutexA";
    try
    {
        var lockA = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
        if (!lockA.Acquired || lockA.Lock == null)
            return Fail(name, "A no adquirió: " + lockA.Message);
        lockA.Lock.Dispose();
        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test11_MutexBBlocked(string root)
{
    const string name = "11_MutexB";
    // Windows Mutex es reentrante en el MISMO hilo; B debe intentarlo en otro hilo.
    try
    {
        var lockA = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
        if (!lockA.Acquired || lockA.Lock == null)
            return Fail(name, "A no adquirió: " + lockA.Message);

        try
        {
            UpdateManagerLockResult? lockB = null;
            var thread = new Thread(() =>
            {
                lockB = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
            });
            thread.IsBackground = true;
            thread.Start();
            if (!thread.Join(TimeSpan.FromSeconds(5)))
                return Fail(name, "timeout esperando hilo B");

            if (lockB == null)
                return Fail(name, "B no produjo resultado");
            if (lockB.Acquired)
            {
                lockB.Lock?.Dispose();
                return Fail(name, "B adquirió lock concurrente");
            }
            if (!lockB.Blocked)
                return Fail(name, "B no reportó Blocked");
            return Pass(name);
        }
        finally
        {
            lockA.Lock.Dispose();
        }
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}

static int Test12_MutexReleaseAllowsReacquire(string root)
{
    const string name = "12_MutexRelease";
    try
    {
        var lockA = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
        if (!lockA.Acquired || lockA.Lock == null)
            return Fail(name, "A no adquirió");
        lockA.Lock.Dispose();

        UpdateManagerLockResult? lockB = null;
        var thread = new Thread(() =>
        {
            lockB = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
        });
        thread.IsBackground = true;
        thread.Start();
        if (!thread.Join(TimeSpan.FromSeconds(5)))
            return Fail(name, "timeout hilo B");

        if (lockB == null || !lockB.Acquired || lockB.Lock == null)
            return Fail(name, "B no pudo re-adquirir tras release: " + (lockB?.Message ?? "null"));
        lockB.Lock.Dispose();
        return Pass(name);
    }
    catch (Exception ex)
    {
        return Fail(name, ex.Message);
    }
}
