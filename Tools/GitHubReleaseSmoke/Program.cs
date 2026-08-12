using System.Text.Json;
using BLL;
using BLL.Update;
using CORE.Update;

int fails = 0;
fails += Test1_ParseValidManifest();
fails += Test2_NormalizeVPrefix();
fails += Test3_ValidReleaseDiscovery();
fails += Test4_InvalidManifestFromRelease();
fails += Test5_ReleaseNotFound();
fails += Test6_NetworkUnavailable();
fails += Test7_Timeout();
fails += Test8_InvalidJson();
fails += Test9_ReleaseWithoutManifest();
fails += Test10_PrereleaseIgnored();
fails += Test11_DraftIgnored();
fails += Test12_EndToEndAvailable();

Console.WriteLine();
Console.WriteLine(fails == 0 ? "ALL TESTS PASSED" : $"FAILED TESTS: {fails}");
Console.WriteLine("NOTE: No package ZIP downloaded. No migrations. No backup. Fake client only (no GitHub writes).");
Environment.Exit(fails == 0 ? 0 : 1);

static UpdateManifest SampleManifest(
    string app = "1.1.0",
    string min = "1.0.0",
    int db = 5,
    string sha = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa") =>
    new()
    {
        AppVersion = app,
        TargetDbVersion = db,
        MinAppVersion = min,
        PackageName = "MFFITNESS-1.1.0.zip",
        PackageSha256 = sha,
        ReleaseDate = new DateTime(2026, 8, 12),
        ReleaseNotesUrl = "https://github.com/ramon302espinal-design/MFFITNESS/releases/tag/v1.1.0"
    };

static int Test1_ParseValidManifest()
{
    Console.WriteLine("===== TEST 1: Parseo manifest válido =====");
    const string json = """
        {
          "appVersion": "1.1.0",
          "targetDbVersion": 5,
          "minAppVersion": "1.0.0",
          "packageName": "MFFITNESS-1.1.0.zip",
          "packageSha256": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
          "releaseDate": "2026-08-12",
          "releaseNotesUrl": "https://github.com/ramon302espinal-design/MFFITNESS/releases/tag/v1.1.0"
        }
        """;
    var m = JsonSerializer.Deserialize<UpdateManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    var v = UpdateManifestValidator.Validate(m);
    bool ok = m != null && v.IsValid && m.AppVersion == "1.1.0" && m.TargetDbVersion == 5;
    Print(ok, v.IsValid ? "parsed+valid" : string.Join("; ", v.Errors));
    return ok ? 0 : 1;
}

static int Test2_NormalizeVPrefix()
{
    Console.WriteLine("===== TEST 2: v1.1.0 → 1.1.0 =====");
    string stripped = GitHubVersionNormalizer.StripVersionPrefix("v1.1.0");
    var m = GitHubVersionNormalizer.NormalizeManifestVersions(
        SampleManifest(app: "v1.1.0", min: "v1.0.0"));
    bool ok = stripped == "1.1.0" && m.AppVersion == "1.1.0" && m.MinAppVersion == "1.0.0";
    Print(ok, $"strip={stripped} app={m.AppVersion} min={m.MinAppVersion}");
    return ok ? 0 : 1;
}

static int Test3_ValidReleaseDiscovery()
{
    Console.WriteLine("===== TEST 3: Release válida =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Ok(SampleManifest(), "v1.1.0", "1.1.0", "https://github.com/x/y/releases/tag/v1.1.0", DateTimeOffset.UtcNow));
    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.Success
        && d.Validation?.IsValid == true
        && d.Availability?.Status == UpdateAvailabilityStatus.Available;
    Print(ok, $"{d.Release.Status} avail={d.Availability?.Status}");
    return ok ? 0 : 1;
}

static int Test4_InvalidManifestFromRelease()
{
    Console.WriteLine("===== TEST 4: Manifest inválido =====");
    var bad = SampleManifest(sha: "abc");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Ok(bad, "v1.1.0", "bad", null, null));
    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.InvalidManifest
        && d.Availability?.Status == UpdateAvailabilityStatus.InvalidManifest;
    Print(ok, $"{d.Release.Status}: {d.Release.Message}");
    return ok ? 0 : 1;
}

static int Test5_ReleaseNotFound()
{
    Console.WriteLine("===== TEST 5: Release inexistente =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Fail(GitHubReleaseStatus.NotFound, "Recurso no encontrado (404).", 404));
    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.NotFound && d.Availability == null;
    Print(ok, d.Release.Message);
    return ok ? 0 : 1;
}

static int Test6_NetworkUnavailable()
{
    Console.WriteLine("===== TEST 6: GitHub no disponible =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Fail(GitHubReleaseStatus.NetworkError, "Sin conexión o error de red."));
    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.NetworkError;
    Print(ok, d.Release.Message);
    return ok ? 0 : 1;
}

static int Test7_Timeout()
{
    Console.WriteLine("===== TEST 7: Timeout =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Fail(GitHubReleaseStatus.NetworkError, "Timeout al consultar GitHub."));
    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.NetworkError
        && d.Release.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase);
    Print(ok, d.Release.Message);
    return ok ? 0 : 1;
}

static int Test8_InvalidJson()
{
    Console.WriteLine("===== TEST 8: JSON inválido =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Fail(GitHubReleaseStatus.ParseError, "manifest.json inválido."));
    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.ParseError;
    Print(ok, d.Release.Message);
    return ok ? 0 : 1;
}

static int Test9_ReleaseWithoutManifest()
{
    Console.WriteLine("===== TEST 9: Release sin manifest =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Fail(GitHubReleaseStatus.NoManifest, "Release sin manifest.json.", tag: "v1.1.0"));
    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.NoManifest;
    Print(ok, d.Release.Message);
    return ok ? 0 : 1;
}

static int Test10_PrereleaseIgnored()
{
    Console.WriteLine("===== TEST 10: Release prerelease =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Fail(GitHubReleaseStatus.DraftOrPrerelease, "La release es prerelease y se ignora.", tag: "v1.1.0-beta"));
    var d = GitHubReleaseDiscoveryBLL.DiscoverByTagAsync(client, "v1.1.0-beta", "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.DraftOrPrerelease;
    Print(ok, d.Release.Message);
    return ok ? 0 : 1;
}

static int Test11_DraftIgnored()
{
    Console.WriteLine("===== TEST 11: Release draft =====");
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Fail(GitHubReleaseStatus.DraftOrPrerelease, "La release es draft y se ignora.", tag: "v1.1.0"));
    var d = GitHubReleaseDiscoveryBLL.DiscoverByTagAsync(client, "v1.1.0", "1.0.0", 4).GetAwaiter().GetResult();
    bool ok = d.Release.Status == GitHubReleaseStatus.DraftOrPrerelease;
    Print(ok, d.Release.Message);
    return ok ? 0 : 1;
}

static int Test12_EndToEndAvailable()
{
    Console.WriteLine("===== TEST 12: GitHub→Manifest→Validator→Evaluator =====");
    // Current 1.0.0 / DB 4 vs Manifest 1.1.0 / TargetDB 5 / Min 1.0.0 → Available
    var client = FakeGitHubReleaseClient.WithResult(
        GitHubReleaseResult.Ok(SampleManifest(), "v1.1.0", "Release 1.1.0",
            "https://github.com/ramon302espinal-design/MFFITNESS/releases/tag/v1.1.0",
            new DateTimeOffset(2026, 8, 12, 0, 0, 0, TimeSpan.Zero)));

    var d = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, currentAppVersion: "1.0.0", currentDbVersion: 4)
        .GetAwaiter().GetResult();

    bool ok = d.Release.Status == GitHubReleaseStatus.Success
        && d.Release.Manifest?.AppVersion == "1.1.0"
        && d.Validation?.IsValid == true
        && d.Availability?.Status == UpdateAvailabilityStatus.Available
        && d.Availability.Reason.Contains("disponible", StringComparison.OrdinalIgnoreCase);

    // NotAvailable when already on target
    var d2 = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client, "1.1.0", 5).GetAwaiter().GetResult();
    bool ok2 = d2.Availability?.Status == UpdateAvailabilityStatus.NotAvailable;

    // Incompatible when below min
    var oldMin = SampleManifest(app: "2.0.0", min: "1.5.0");
    var client3 = FakeGitHubReleaseClient.WithResult(GitHubReleaseResult.Ok(oldMin, "v2.0.0", null, null, null));
    var d3 = GitHubReleaseDiscoveryBLL.DiscoverLatestAsync(client3, "1.0.0", 4).GetAwaiter().GetResult();
    bool ok3 = d3.Availability?.Status == UpdateAvailabilityStatus.Incompatible;

    Print(ok && ok2 && ok3, $"Available={ok} NotAvailable={ok2} Incompatible={ok3}");
    return ok && ok2 && ok3 ? 0 : 1;
}

static void Print(bool ok, string detail)
{
    Console.WriteLine(ok ? "  PASS" : "  FAIL");
    Console.WriteLine("  " + detail);
}
