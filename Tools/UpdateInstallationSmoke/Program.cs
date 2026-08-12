using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using BLL;
using BLL.Update;
using CORE.Update;

string root = Path.Combine(Path.GetTempPath(), "MFFITNESS-InstallSmoke-91-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);

int fails = 0;
try
{
    // --- FASE 9.1 auditoria / recovery ---
    fails += TestP01_FullInstallSuccess(root);
    fails += TestP02_FailFirstFile(root);
    fails += TestP03_FailIntermediateFile(root);
    fails += TestP04_FailLastFile(root);
    fails += TestP05_RestoreAfterIntermediateFailure(root);
    fails += TestP06_Sha256MatchesAfterRecovery(root);
    fails += TestP07_RecoveryFailed(root);
    fails += TestP08_ValidPackage(root);
    fails += TestP09_CajaAbiertaBlocked(root);
    fails += TestP10_HealthCheckSuccess(root);

    // --- Regresion FASE 9 (subset) ---
    fails += TestR01_PackageNotVerified(root);
    fails += TestR02_ZipPathTraversal(root);
    fails += TestR03_PartialInstallWithoutRecoveryHook_DocumentsRisk(root);
}
finally
{
    try { Directory.Delete(root, recursive: true); } catch { /* ignore */ }
}

Console.WriteLine();
Console.WriteLine(fails == 0 ? "ALL TESTS PASSED" : $"FAILED TESTS: {fails}");
Console.WriteLine("NOTE: Offline fakes. No DB. No migrations. No backup. No DB rollback.");
Console.WriteLine("TEST 11 (Release build) runs via dotnet build.");
Environment.Exit(fails == 0 ? 0 : 1);

// ---- helpers ----

static UpdateManifest SampleManifest(string sha, string app = "1.1.0") => new()
{
    AppVersion = app,
    TargetDbVersion = 5,
    MinAppVersion = "1.0.0",
    PackageName = "MFFITNESS-1.1.0.zip",
    PackageSha256 = sha,
    ReleaseDate = new DateTime(2026, 8, 12),
    ReleaseNotesUrl = "https://github.com/example/releases/tag/v1.1.0"
};

static string Sha256File(string path)
{
    using var s = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(s)).ToLowerInvariant();
}

static string CreateOldInstall(string dir)
{
    Directory.CreateDirectory(dir);
    File.WriteAllText(Path.Combine(dir, "UI.exe"), "OLD_UI");
    File.WriteAllText(Path.Combine(dir, "UI.dll"), "OLD_UIDLL");
    File.WriteAllText(Path.Combine(dir, "BLL.dll"), "OLD_BLL");
    File.WriteAllText(Path.Combine(dir, "DL.dll"), "OLD_DL");
    File.WriteAllText(Path.Combine(dir, "DTO.dll"), "OLD_DTO");
    File.WriteAllText(Path.Combine(dir, "CORE.dll"), "OLD_CORE");
    return dir;
}

static string CreateNewZip(string zipPath)
{
    if (File.Exists(zipPath)) File.Delete(zipPath);
    using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
    zip.CreateEntryFromBytes("UI.exe", Encoding.UTF8.GetBytes("NEW_UI"));
    zip.CreateEntryFromBytes("UI.dll", Encoding.UTF8.GetBytes("NEW_UIDLL"));
    zip.CreateEntryFromBytes("BLL.dll", Encoding.UTF8.GetBytes("NEW_BLL"));
    zip.CreateEntryFromBytes("DL.dll", Encoding.UTF8.GetBytes("NEW_DL"));
    zip.CreateEntryFromBytes("DTO.dll", Encoding.UTF8.GetBytes("NEW_DTO"));
    zip.CreateEntryFromBytes("CORE.dll", Encoding.UTF8.GetBytes("NEW_CORE"));
    return zipPath;
}

static UpdateInstallRequest BaseRequest(string testDir, string installDir, string zipPath, string sha) => new()
{
    Manifest = SampleManifest(sha),
    PackagePath = zipPath,
    ExpectedSha256 = sha,
    PackageVerified = true,
    InstallDirectory = installDir,
    StagingDirectory = Path.Combine(testDir, "staging"),
    SnapshotDirectory = Path.Combine(testDir, "snapshots"),
    StartApplicationAfterInstall = false
};

static UpdateInstallerHooks StandardHooks(string uiPath, bool cajaAbierta = false, Action<string>? beforeCopy = null) =>
    new()
    {
        IsCajaAbierta = () => cajaAbierta,
        IsMigrationRunning = () => false,
        HasCriticalOperation = () => false,
        ProcessController = CreateIdleProcess(uiPath),
        ApplicationLauncher = new FakeUpdateApplicationLauncher(),
        BeforeFileCopy = beforeCopy,
        VerifyInstalledVersion = (_, m) => (true, m.AppVersion, null),
        RunHealthCheck = (_, m) => new UpdateHealthCheckService.HealthCheckResult
        {
            Success = true,
            Message = "OK",
            InstalledAppVersion = m.AppVersion
        }
    };

static FakeUpdateProcessController CreateIdleProcess(string uiPath)
{
    var proc = new FakeUpdateProcessController();
    proc.SetRunning(uiPath, running: false);
    return proc;
}

static string Read(string install, string file) => File.ReadAllText(Path.Combine(install, file));

static void ReportInstallState(string install, string label)
{
    Console.WriteLine($"  [{label}]");
    foreach (string f in AllowedUpdatePackageFiles.RequiredFiles)
    {
        string path = Path.Combine(install, f);
        string content = File.Exists(path) ? File.ReadAllText(path) : "<missing>";
        Console.WriteLine($"    {f} = {content}");
    }
}

static bool AllOld(string install) =>
    Read(install, "UI.exe") == "OLD_UI" &&
    Read(install, "BLL.dll") == "OLD_BLL" &&
    Read(install, "DL.dll") == "OLD_DL" &&
    Read(install, "DTO.dll") == "OLD_DTO" &&
    Read(install, "CORE.dll") == "OLD_CORE";

static bool AllNew(string install) =>
    Read(install, "UI.exe") == "NEW_UI" &&
    Read(install, "BLL.dll") == "NEW_BLL" &&
    Read(install, "DL.dll") == "NEW_DL" &&
    Read(install, "DTO.dll") == "NEW_DTO" &&
    Read(install, "CORE.dll") == "NEW_CORE";

static (string dir, string install, string zip, string sha, UpdateInstallRequest req) Prep(string root, string name)
{
    string dir = Path.Combine(root, name);
    Directory.CreateDirectory(dir);
    string install = CreateOldInstall(Path.Combine(dir, "install"));
    string zip = Path.Combine(dir, "MFFITNESS-1.1.0.zip");
    CreateNewZip(zip);
    string sha = Sha256File(zip);
    return (dir, install, zip, sha, BaseRequest(dir, install, zip, sha));
}

static void Print(bool ok, string detail)
{
    Console.WriteLine(ok ? "  PASS" : "  FAIL");
    Console.WriteLine("  " + detail);
}

// ---- FASE 9.1 tests ----

static int TestP01_FullInstallSuccess(string root)
{
    Console.WriteLine("===== TEST 1: Instalación completa exitosa =====");
    var (dir, install, _, _, req) = Prep(root, "p01");
    var result = UpdateInstallationBLL.Install(req, StandardHooks(Path.Combine(install, "UI.exe")));
    ReportInstallState(install, "final");
    bool ok = result.Success && result.Stage == UpdateInstallationStage.Completed && AllNew(install);
    Print(ok, $"{result.Stage} allNew={AllNew(install)}");
    return ok ? 0 : 1;
}

static int TestP02_FailFirstFile(string root)
{
    Console.WriteLine("===== TEST 2: Fallo en primer archivo (UI.exe) =====");
    var (dir, install, _, _, req) = Prep(root, "p02");
    var hooks = StandardHooks(Path.Combine(install, "UI.exe"), beforeCopy: relative =>
    {
        if (string.Equals(relative, "UI.exe", StringComparison.OrdinalIgnoreCase))
            throw new IOException("Simulated fail on UI.exe");
    });
    var result = UpdateInstallationBLL.Install(req, hooks);
    ReportInstallState(install, "after-fail-first");
    bool ok = result.Stage == UpdateInstallationStage.FailedRecovered
        && AllOld(install)
        && result.RecoverySucceeded
        && (result.InstalledFiles.Count == 0);
    Print(ok, $"{result.Stage} recovered={result.RecoverySucceeded} modifiedBeforeFail={result.InstalledFiles.Count}");
    return ok ? 0 : 1;
}

static int TestP03_FailIntermediateFile(string root)
{
    Console.WriteLine("===== TEST 3: Fallo en archivo intermedio (DL.dll) — riesgo original =====");
    string dir = Path.Combine(root, "p03");
    Directory.CreateDirectory(dir);

    // Demostración del riesgo SIN recovery: InstallFromStaging solo.
    string rawInstall = CreateOldInstall(Path.Combine(dir, "install-raw"));
    string staging = Path.Combine(dir, "staging-raw");
    Directory.CreateDirectory(staging);
    File.WriteAllText(Path.Combine(staging, "UI.exe"), "NEW_UI");
    File.WriteAllText(Path.Combine(staging, "UI.dll"), "NEW_UIDLL");
    File.WriteAllText(Path.Combine(staging, "BLL.dll"), "NEW_BLL");
    File.WriteAllText(Path.Combine(staging, "DL.dll"), "NEW_DL");
    File.WriteAllText(Path.Combine(staging, "DTO.dll"), "NEW_DTO");
    File.WriteAllText(Path.Combine(staging, "CORE.dll"), "NEW_CORE");

    var raw = UpdateBinaryInstaller.InstallFromStaging(staging, rawInstall, relative =>
    {
        if (string.Equals(relative, "DL.dll", StringComparison.OrdinalIgnoreCase))
            throw new IOException("Simulated fail on DL.dll");
    });

    Console.WriteLine($"  RAW InstallFromStaging Success={raw.Success} FailedOn={raw.FailedOnFile}");
    Console.WriteLine($"  RAW modified=[{string.Join(",", raw.InstalledFiles)}]");
    ReportInstallState(rawInstall, "RAW-partial-NO-recovery");
    bool demonstratedPartial =
        !raw.Success
        && raw.InstalledFiles.Contains("UI.exe", StringComparer.OrdinalIgnoreCase)
        && raw.InstalledFiles.Contains("BLL.dll", StringComparer.OrdinalIgnoreCase)
        && !raw.InstalledFiles.Contains("DL.dll", StringComparer.OrdinalIgnoreCase)
        && Read(rawInstall, "UI.exe") == "NEW_UI"
        && Read(rawInstall, "BLL.dll") == "NEW_BLL"
        && Read(rawInstall, "DL.dll") == "OLD_DL";

    // Con UpdateInstaller + recovery
    var (_, install2, _, _, req2) = Prep(root, "p03b");
    var hooks = StandardHooks(Path.Combine(install2, "UI.exe"), beforeCopy: relative =>
    {
        if (string.Equals(relative, "DL.dll", StringComparison.OrdinalIgnoreCase))
            throw new IOException("Simulated fail on DL.dll");
    });
    var result = UpdateInstallationBLL.Install(req2, hooks);
    ReportInstallState(install2, "with-recovery");

    bool ok = demonstratedPartial
        && result.Stage == UpdateInstallationStage.FailedRecovered
        && AllOld(install2);
    Print(ok, $"rawPartial={demonstratedPartial} recoveredStage={result.Stage} allOld={AllOld(install2)}");
    return ok ? 0 : 1;
}

static int TestP04_FailLastFile(string root)
{
    Console.WriteLine("===== TEST 4: Fallo en último archivo (CORE.dll) =====");
    var (dir, install, _, _, req) = Prep(root, "p04");
    var hooks = StandardHooks(Path.Combine(install, "UI.exe"), beforeCopy: relative =>
    {
        if (string.Equals(relative, "CORE.dll", StringComparison.OrdinalIgnoreCase))
            throw new IOException("Simulated fail on CORE.dll");
    });
    var result = UpdateInstallationBLL.Install(req, hooks);
    ReportInstallState(install, "after-fail-last");
    bool ok = result.Stage == UpdateInstallationStage.FailedRecovered
        && AllOld(install)
        && result.InstalledFiles.Count >= 4;
    Print(ok, $"{result.Stage} modifiedBeforeFail={result.InstalledFiles.Count} allOld={AllOld(install)}");
    return ok ? 0 : 1;
}

static int TestP05_RestoreAfterIntermediateFailure(string root)
{
    Console.WriteLine("===== TEST 5: Restauración correcta tras fallo intermedio =====");
    var (dir, install, _, _, req) = Prep(root, "p05");
    var hooks = StandardHooks(Path.Combine(install, "UI.exe"), beforeCopy: relative =>
    {
        if (string.Equals(relative, "DL.dll", StringComparison.OrdinalIgnoreCase))
            throw new IOException("fail mid");
    });
    var result = UpdateInstallationBLL.Install(req, hooks);
    bool ok = result.Stage == UpdateInstallationStage.FailedRecovered
        && result.RecoveryAttempted
        && result.RecoverySucceeded
        && result.RecoveredFiles.Count >= 5
        && AllOld(install)
        && !result.Success
        && !result.ApplicationStarted;
    Print(ok, $"recovered={string.Join(",", result.RecoveredFiles)}");
    return ok ? 0 : 1;
}

static int TestP06_Sha256MatchesAfterRecovery(string root)
{
    Console.WriteLine("===== TEST 6: SHA256 snapshot coincide tras recovery =====");
    var (dir, install, _, _, req) = Prep(root, "p06");

    // Snapshot esperado (OLD)
    var expectedHashes = AllowedUpdatePackageFiles.RequiredFiles
        .ToDictionary(
            f => f,
            f => Sha256File(Path.Combine(install, f)),
            StringComparer.OrdinalIgnoreCase);

    var hooks = StandardHooks(Path.Combine(install, "UI.exe"), beforeCopy: relative =>
    {
        if (string.Equals(relative, "DTO.dll", StringComparison.OrdinalIgnoreCase))
            throw new IOException("fail before DTO");
    });
    var result = UpdateInstallationBLL.Install(req, hooks);

    bool hashesMatch = AllowedUpdatePackageFiles.RequiredFiles.All(f =>
        string.Equals(Sha256File(Path.Combine(install, f)), expectedHashes[f], StringComparison.OrdinalIgnoreCase));

    // También verificar contra snapshot.json
    var snap = UpdateBinarySnapshotService.LoadSnapshot(result.SnapshotPath!);
    bool snapOk = snap != null && snap.Files.All(e =>
        string.Equals(
            Sha256File(Path.Combine(install, e.RelativePath)),
            e.Sha256,
            StringComparison.OrdinalIgnoreCase));

    bool ok = result.Stage == UpdateInstallationStage.FailedRecovered && hashesMatch && snapOk;
    Print(ok, $"hashesMatch={hashesMatch} snapOk={snapOk} stage={result.Stage}");
    return ok ? 0 : 1;
}

static int TestP07_RecoveryFailed(string root)
{
    Console.WriteLine("===== TEST 7: Recovery fallido =====");
    var (dir, install, _, _, req) = Prep(root, "p07");
    var hooks = StandardHooks(Path.Combine(install, "UI.exe"), beforeCopy: relative =>
    {
        if (string.Equals(relative, "BLL.dll", StringComparison.OrdinalIgnoreCase))
            throw new IOException("fail BLL");
    });
    hooks = hooks with
    {
        RestoreSnapshot = (_, _, _) => new UpdateBinarySnapshotService.RestoreResult
        {
            Success = false,
            Message = "Simulated recovery failure",
            RestoredFiles = Array.Empty<string>(),
            FailedFiles = new[] { "UI.exe (simulated)" }
        }
    };
    var result = UpdateInstallationBLL.Install(req, hooks);
    bool ok = result.Stage == UpdateInstallationStage.FailedRecoveryRequired
        && result.RecoveryAttempted
        && !result.RecoverySucceeded
        && !result.Success
        && !result.ApplicationStarted;
    Print(ok, $"{result.Stage}: {result.Message}");
    return ok ? 0 : 1;
}

static int TestP08_ValidPackage(string root)
{
    Console.WriteLine("===== TEST 8: Paquete válido =====");
    var (dir, install, _, _, req) = Prep(root, "p08");
    var result = UpdateInstallationBLL.Install(req, StandardHooks(Path.Combine(install, "UI.exe")));
    bool ok = result.Success && AllNew(install);
    Print(ok, result.Stage.ToString());
    return ok ? 0 : 1;
}

static int TestP09_CajaAbiertaBlocked(string root)
{
    Console.WriteLine("===== TEST 9: Caja abierta bloquea =====");
    var (dir, install, _, _, req) = Prep(root, "p09");
    var result = UpdateInstallationBLL.Install(req, StandardHooks(Path.Combine(install, "UI.exe"), cajaAbierta: true));
    bool ok = result.Blocked && result.Stage == UpdateInstallationStage.Blocked && AllOld(install);
    Print(ok, $"{result.Stage}: {result.Message}");
    return ok ? 0 : 1;
}

static int TestP10_HealthCheckSuccess(string root)
{
    Console.WriteLine("===== TEST 10: Health-check exitoso =====");
    string install = CreateOldInstall(Path.Combine(root, "p10-install"));
    var health = new UpdateHealthCheckService.HealthCheckResult
    {
        Success = true,
        Message = "OK",
        InstalledAppVersion = "1.1.0"
    };
    // Direct hook path used by installer
    var hooks = StandardHooks(Path.Combine(install, "UI.exe"));
    var hc = hooks.RunHealthCheck!(install, SampleManifest(new string('a', 64)));
    bool ok = hc.Success;
    Print(ok, hc.Message);
    return ok ? 0 : 1;
}

static int TestR01_PackageNotVerified(string root)
{
    Console.WriteLine("===== TEST R1: Paquete no verificado =====");
    var (dir, install, zip, sha, req) = Prep(root, "r01");
    req = req with { PackageVerified = false };
    var result = UpdateInstallationBLL.Install(req, StandardHooks(Path.Combine(install, "UI.exe")));
    bool ok = !result.Success && result.Message.Contains("no verificado", StringComparison.OrdinalIgnoreCase);
    Print(ok, result.Message);
    return ok ? 0 : 1;
}

static int TestR02_ZipPathTraversal(string root)
{
    Console.WriteLine("===== TEST R2: ZIP path traversal =====");
    string zip = Path.Combine(root, "r02.zip");
    CreateNewZip(zip);
    using (var z = ZipFile.Open(zip, ZipArchiveMode.Update))
        z.CreateEntryFromBytes("../evil.dll", Encoding.UTF8.GetBytes("x"));
    var validation = UpdateZipPathValidator.ValidateArchive(zip);
    bool ok = !validation.IsValid && validation.Errors.Any(e => e.Contains("Zip Slip", StringComparison.OrdinalIgnoreCase));
    Print(ok, string.Join("; ", validation.Errors));
    return ok ? 0 : 1;
}

static int TestR03_PartialInstallWithoutRecoveryHook_DocumentsRisk(string root)
{
    Console.WriteLine("===== TEST R3: Documenta riesgo File.Copy secuencial (sin recovery) =====");
    string dir = Path.Combine(root, "r03");
    Directory.CreateDirectory(dir);
    string install = CreateOldInstall(Path.Combine(dir, "install"));
    string staging = Path.Combine(dir, "staging");
    Directory.CreateDirectory(staging);
    File.WriteAllText(Path.Combine(staging, "UI.exe"), "NEW_UI");
    File.WriteAllText(Path.Combine(staging, "UI.dll"), "NEW_UIDLL");
    File.WriteAllText(Path.Combine(staging, "BLL.dll"), "NEW_BLL");
    File.WriteAllText(Path.Combine(staging, "DL.dll"), "NEW_DL");
    File.WriteAllText(Path.Combine(staging, "DTO.dll"), "NEW_DTO");
    File.WriteAllText(Path.Combine(staging, "CORE.dll"), "NEW_CORE");

    var result = UpdateBinaryInstaller.InstallFromStaging(staging, install, relative =>
    {
        if (relative.Equals("DL.dll", StringComparison.OrdinalIgnoreCase))
            throw new IOException("forced");
    });

    var modified = result.InstalledFiles.ToList();
    var untouched = AllowedUpdatePackageFiles.RequiredFiles
        .Where(f => !modified.Contains(f, StringComparer.OrdinalIgnoreCase))
        .ToList();

    Console.WriteLine($"  Result Success={result.Success} FailedOn={result.FailedOnFile}");
    Console.WriteLine($"  Modified: [{string.Join(", ", modified)}]");
    Console.WriteLine($"  Untouched: [{string.Join(", ", untouched)}]");
    ReportInstallState(install, "partial-state");

    bool riskConfirmed = !result.Success
        && modified.Count >= 2
        && Read(install, "UI.exe") == "NEW_UI"
        && Read(install, "DL.dll") == "OLD_DL";

    Print(riskConfirmed, "Sequential File.Copy leaves partial install if caller does not recover.");
    return riskConfirmed ? 0 : 1;
}

static class ZipExtensions
{
    public static ZipArchiveEntry CreateEntryFromBytes(this ZipArchive zip, string name, byte[] content)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Fastest);
        using var s = entry.Open();
        s.Write(content, 0, content.Length);
        return entry;
    }
}
