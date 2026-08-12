using BLL;
using BLL.Update;
using CORE.Update;
using DL.Backup;
using DL.Migrations;

string migDir = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Database", "Migrations"));

if (!Directory.Exists(migDir))
    migDir = MigrationRunner.ResolveDefaultDirectory();

Console.WriteLine("Migrations dir: " + migDir);
int fails = 0;

fails += RunTest1(migDir);
fails += RunTest2(migDir);
fails += RunTest3(migDir);
fails += RunTest4(migDir);
fails += RunTest5(migDir);
fails += RunTest6(migDir);

Console.WriteLine();
Console.WriteLine(fails == 0 ? "ALL TESTS PASSED" : $"FAILED TESTS: {fails}");
Environment.Exit(fails == 0 ? 0 : 1);

static int RunTest1(string migDir)
{
    Console.WriteLine("===== TEST 1: target 1.0.0 / DB=current (no migrate) =====");
    int db = SchemaMigrationBLL.GetCurrentDbVersion();
    var result = UpdateManagerBLL.Run(new UpdateTarget
    {
        TargetAppVersion = "1.0.0",
        TargetDbVersion = db,
        MigrationsDirectory = migDir
    });

    bool ok = result.Success
        && !result.Blocked
        && result.BackupCreated
        && result.BackupVerified
        && !result.MigrationsApplied
        && result.FinalDbVersion == db
        && result.Stage == UpdateStage.Completed;

    Print(result, ok);
    return ok ? 0 : 1;
}

static int RunTest2(string migDir)
{
    Console.WriteLine("===== TEST 2: caja abierta (hook, sin tocar BD) =====");
    int backupsBefore = CountBackups();
    int dbBefore = SchemaMigrationBLL.GetCurrentDbVersion();

    bool migrationCalled = false;
    var hooks = new UpdateOrchestratorHooks
    {
        IsCajaAbierta = () => true,
        CreateBackup = () => throw new InvalidOperationException("Backup no debe ejecutarse con caja abierta."),
        ApplyMigrations = _ =>
        {
            migrationCalled = true;
            throw new InvalidOperationException("Migración no debe ejecutarse con caja abierta.");
        }
    };

    var result = UpdateManagerBLL.Run(new UpdateTarget
    {
        TargetAppVersion = "1.0.0",
        TargetDbVersion = dbBefore,
        MigrationsDirectory = migDir
    }, hooks);

    int backupsAfter = CountBackups();
    int dbAfter = SchemaMigrationBLL.GetCurrentDbVersion();

    bool ok = result.Blocked
        && !result.Success
        && result.Stage == UpdateStage.Blocked
        && !result.BackupCreated
        && !result.MigrationsApplied
        && !migrationCalled
        && backupsAfter == backupsBefore
        && dbAfter == dbBefore
        && (result.ErrorMessage?.Contains("Caja abierta", StringComparison.OrdinalIgnoreCase) ?? false);

    Print(result, ok);
    Console.WriteLine($"  backupsBefore={backupsBefore} after={backupsAfter} db={dbAfter} migrationCalled={migrationCalled}");
    return ok ? 0 : 1;
}

static int RunTest3(string migDir)
{
    Console.WriteLine("===== TEST 3: backup falla → no migración =====");
    int dbBefore = SchemaMigrationBLL.GetCurrentDbVersion();
    bool migrationCalled = false;

    var hooks = new UpdateOrchestratorHooks
    {
        IsCajaAbierta = () => false,
        CreateBackup = () => DatabaseBackupResult.Fail("MF CYBER DB", DateTime.Now, "Simulated backup failure"),
        ApplyMigrations = _ =>
        {
            migrationCalled = true;
            return MigrationRunResult.Fail(dbBefore, dbBefore, "should not run");
        }
    };

    var result = UpdateManagerBLL.Run(new UpdateTarget
    {
        TargetAppVersion = "1.0.0",
        TargetDbVersion = dbBefore + 1,
        MigrationsDirectory = migDir
    }, hooks);

    int dbAfter = SchemaMigrationBLL.GetCurrentDbVersion();
    bool ok = !result.Success
        && !result.Blocked
        && result.Stage == UpdateStage.Failed
        && !result.BackupVerified
        && !result.MigrationsApplied
        && !migrationCalled
        && dbAfter == dbBefore;

    Print(result, ok);
    Console.WriteLine($"  migrationCalled={migrationCalled} dbBefore={dbBefore} dbAfter={dbAfter}");
    return ok ? 0 : 1;
}

static int RunTest4(string migDir)
{
    Console.WriteLine("===== TEST 4: migración falla → SchemaVersion sin avanzar =====");
    int dbBefore = SchemaMigrationBLL.GetCurrentDbVersion();
    int target = dbBefore + 1;

    string failDir = Path.Combine(Path.GetTempPath(), "mffitness-update-fail-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(failDir);
    File.WriteAllText(
        Path.Combine(failDir, $"{target:0000}_ShouldFail.sql"),
        "INSERT INTO dbo.MigrationEngineTest (Marker) VALUES (N'update-should-rollback');\r\n" +
        "RAISERROR(N'Fallo intencional UpdateManager V0', 16, 1);\r\n");

    try
    {
        var result = UpdateManagerBLL.Run(new UpdateTarget
        {
            TargetAppVersion = "1.1.0",
            TargetDbVersion = target,
            MigrationsDirectory = failDir
        });

        int dbAfter = SchemaMigrationBLL.GetCurrentDbVersion();
        bool ok = !result.Success
            && !result.Blocked
            && result.Stage == UpdateStage.Failed
            && result.BackupCreated
            && result.BackupVerified
            && dbAfter == dbBefore
            && result.FinalDbVersion == dbBefore;

        Print(result, ok);
        Console.WriteLine($"  dbBefore={dbBefore} dbAfter={dbAfter}");
        return ok ? 0 : 1;
    }
    finally
    {
        try { Directory.Delete(failDir, true); } catch { /* ignore */ }
    }
}

static int RunTest5(string migDir)
{
    Console.WriteLine("===== TEST 5: TargetDB > CurrentDB → SUCCESS =====");
    int dbBefore = SchemaMigrationBLL.GetCurrentDbVersion();
    int target = dbBefore + 1;

    var result = UpdateManagerBLL.Run(new UpdateTarget
    {
        TargetAppVersion = "1.1.0",
        TargetDbVersion = target,
        MigrationsDirectory = migDir
    });

    int dbAfter = SchemaMigrationBLL.GetCurrentDbVersion();
    bool ok = result.Success
        && !result.Blocked
        && result.Stage == UpdateStage.Completed
        && result.BackupCreated
        && result.BackupVerified
        && result.MigrationsApplied
        && result.FinalDbVersion == target
        && dbAfter == target;

    Print(result, ok);
    Console.WriteLine($"  dbBefore={dbBefore} target={target} dbAfter={dbAfter}");
    return ok ? 0 : 1;
}

static int RunTest6(string migDir)
{
    Console.WriteLine("===== TEST 6: mismo target otra vez (idempotente) =====");
    int db = SchemaMigrationBLL.GetCurrentDbVersion();

    var result = UpdateManagerBLL.Run(new UpdateTarget
    {
        TargetAppVersion = "1.1.0",
        TargetDbVersion = db,
        MigrationsDirectory = migDir
    });

    bool ok = result.Success
        && !result.MigrationsApplied
        && result.FinalDbVersion == db
        && result.BackupCreated
        && result.BackupVerified;

    Print(result, ok);
    return ok ? 0 : 1;
}

static void Print(UpdateResult r, bool ok)
{
    Console.WriteLine(ok ? "  PASS" : "  FAIL");
    Console.WriteLine($"  Success={r.Success} Blocked={r.Blocked} Stage={r.Stage}");
    Console.WriteLine($"  App {r.CurrentAppVersion} → {r.TargetAppVersion}");
    Console.WriteLine($"  DB {r.CurrentDbVersion} → {r.TargetDbVersion} (final={r.FinalDbVersion})");
    Console.WriteLine($"  CajaAbierta={r.CajaAbierta} BackupCreated={r.BackupCreated} BackupVerified={r.BackupVerified}");
    Console.WriteLine($"  MigrationsApplied={r.MigrationsApplied} Applied=[{string.Join(",", r.AppliedMigrationVersions)}]");
    if (!string.IsNullOrEmpty(r.BackupPath))
        Console.WriteLine($"  BackupPath={r.BackupPath}");
    if (!string.IsNullOrEmpty(r.ErrorMessage))
        Console.WriteLine($"  Error={r.ErrorMessage}");
}

static int CountBackups()
{
    string dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MFFITNESS",
        "backups");
    if (!Directory.Exists(dir))
        return 0;
    return Directory.GetFiles(dir, "MFFITNESS_*.bak").Length;
}
