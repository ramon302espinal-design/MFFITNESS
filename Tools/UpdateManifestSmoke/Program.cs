using CORE.Update;

int fails = 0;
fails += Test1_ValidManifest();
fails += Test2_InvalidAppVersion();
fails += Test3_InvalidTargetDb();
fails += Test4_InvalidSha256();
fails += Test5_MinGreaterThanApp();
fails += Test6_CurrentEqualsTarget();
fails += Test7_CurrentLessThanTarget();
fails += Test8_CurrentLessThanMin();
fails += Test9_EmptyRequiredFields();

Console.WriteLine();
Console.WriteLine(fails == 0 ? "ALL TESTS PASSED" : $"FAILED TESTS: {fails}");
Environment.Exit(fails == 0 ? 0 : 1);

static UpdateManifest ValidSample() => new()
{
    AppVersion = "1.1.0",
    TargetDbVersion = 5,
    MinAppVersion = "1.0.0",
    PackageName = "MFFITNESS-1.1.0.zip",
    PackageSha256 = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
    ReleaseDate = new DateTime(2026, 8, 12),
    ReleaseNotesUrl = "https://example.com/releases/1.1.0"
};

static int Test1_ValidManifest()
{
    Console.WriteLine("===== TEST 1: Manifest válido =====");
    var v = UpdateManifestValidator.Validate(ValidSample());
    bool ok = v.IsValid && v.Errors.Count == 0;
    Print(ok, v.IsValid ? "valid" : string.Join("; ", v.Errors));
    return ok ? 0 : 1;
}

static int Test2_InvalidAppVersion()
{
    Console.WriteLine("===== TEST 2: AppVersion inválida =====");
    var m = ValidSample() with { AppVersion = "1.1" };
    var v = UpdateManifestValidator.Validate(m);
    bool ok = !v.IsValid && v.Errors.Any(e => e.Contains("AppVersion", StringComparison.OrdinalIgnoreCase));
    Print(ok, string.Join("; ", v.Errors));
    return ok ? 0 : 1;
}

static int Test3_InvalidTargetDb()
{
    Console.WriteLine("===== TEST 3: TargetDB inválido =====");
    var m = ValidSample() with { TargetDbVersion = 0 };
    var v = UpdateManifestValidator.Validate(m);
    bool ok = !v.IsValid && v.Errors.Any(e => e.Contains("TargetDbVersion", StringComparison.OrdinalIgnoreCase));
    Print(ok, string.Join("; ", v.Errors));
    return ok ? 0 : 1;
}

static int Test4_InvalidSha256()
{
    Console.WriteLine("===== TEST 4: SHA256 inválido =====");
    var m = ValidSample() with { PackageSha256 = "abc123" };
    var v = UpdateManifestValidator.Validate(m);
    bool ok = !v.IsValid && v.Errors.Any(e => e.Contains("PackageSha256", StringComparison.OrdinalIgnoreCase));
    Print(ok, string.Join("; ", v.Errors));
    return ok ? 0 : 1;
}

static int Test5_MinGreaterThanApp()
{
    Console.WriteLine("===== TEST 5: MinAppVersion > AppVersion =====");
    var m = ValidSample() with { AppVersion = "1.0.0", MinAppVersion = "1.1.0" };
    var v = UpdateManifestValidator.Validate(m);
    bool ok = !v.IsValid && v.Errors.Any(e => e.Contains("MinAppVersion", StringComparison.OrdinalIgnoreCase));
    Print(ok, string.Join("; ", v.Errors));
    return ok ? 0 : 1;
}

static int Test6_CurrentEqualsTarget()
{
    Console.WriteLine("===== TEST 6: CurrentApp = target → NotAvailable =====");
    var a = UpdateAvailabilityEvaluator.Evaluate(ValidSample() with { AppVersion = "1.0.0", MinAppVersion = "1.0.0" }, "1.0.0", 4);
    bool ok = a.Status == UpdateAvailabilityStatus.NotAvailable
        && a.Reason.Contains("actualizado", StringComparison.OrdinalIgnoreCase);
    Print(ok, $"{a.Status}: {a.Reason}");
    return ok ? 0 : 1;
}

static int Test7_CurrentLessThanTarget()
{
    Console.WriteLine("===== TEST 7: CurrentApp < target → Available =====");
    var a = UpdateAvailabilityEvaluator.Evaluate(ValidSample(), "1.0.0", 4);
    bool ok = a.Status == UpdateAvailabilityStatus.Available
        && a.Reason.Contains("disponible", StringComparison.OrdinalIgnoreCase);
    Print(ok, $"{a.Status}: {a.Reason}");
    return ok ? 0 : 1;
}

static int Test8_CurrentLessThanMin()
{
    Console.WriteLine("===== TEST 8: CurrentApp < MinAppVersion → Incompatible =====");
    var m = ValidSample() with { AppVersion = "2.0.0", MinAppVersion = "1.5.0" };
    var a = UpdateAvailabilityEvaluator.Evaluate(m, "1.0.0", 4);
    bool ok = a.Status == UpdateAvailabilityStatus.Incompatible
        && a.Reason.Contains("antigua", StringComparison.OrdinalIgnoreCase);
    Print(ok, $"{a.Status}: {a.Reason}");
    return ok ? 0 : 1;
}

static int Test9_EmptyRequiredFields()
{
    Console.WriteLine("===== TEST 9: campos obligatorios vacíos =====");
    var m = new UpdateManifest
    {
        AppVersion = "",
        TargetDbVersion = 5,
        MinAppVersion = "",
        PackageName = "",
        PackageSha256 = "",
        ReleaseDate = default
    };
    var v = UpdateManifestValidator.Validate(m);
    bool ok = !v.IsValid
        && v.Errors.Count >= 4
        && v.Errors.Any(e => e.Contains("AppVersion", StringComparison.OrdinalIgnoreCase))
        && v.Errors.Any(e => e.Contains("PackageName", StringComparison.OrdinalIgnoreCase))
        && v.Errors.Any(e => e.Contains("PackageSha256", StringComparison.OrdinalIgnoreCase))
        && v.Errors.Any(e => e.Contains("ReleaseDate", StringComparison.OrdinalIgnoreCase));
    Print(ok, string.Join("; ", v.Errors));
    return ok ? 0 : 1;
}

static void Print(bool ok, string detail)
{
    Console.WriteLine(ok ? "  PASS" : "  FAIL");
    Console.WriteLine("  " + detail);
}
