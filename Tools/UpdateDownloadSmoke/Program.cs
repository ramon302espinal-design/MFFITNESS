using System.Net;
using System.Security.Cryptography;
using System.Text;
using BLL;
using BLL.Update;
using CORE.Update;

int fails = 0;
string testDir = Path.Combine(Path.GetTempPath(), "MFFITNESS-UpdateDownloadSmoke-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testDir);

try
{
    byte[] goodBytes = Encoding.UTF8.GetBytes("MFFITNESS-1.1.0-test-package-content");
    string goodSha = ToSha256Hex(goodBytes);
    byte[] badBytes = Encoding.UTF8.GetBytes("MFFITNESS-1.1.0-wrong-content");

    fails += Test1_SuccessVerified(goodBytes, goodSha, testDir);
    fails += Test2_HashMismatch(goodBytes, goodSha, testDir);
    fails += Test3_HttpUrlRejected(testDir);
    fails += Test4_HttpsUrlAccepted(goodBytes, goodSha, testDir);
    fails += Test5_EmptyFile(testDir);
    fails += Test6_Http404(testDir);
    fails += Test7_Http403(testDir);
    fails += Test8_Http500(testDir);
    fails += Test9_Timeout(testDir);
    fails += Test10_Cancellation(goodBytes, testDir);
    fails += Test11_PackageNameMismatch(goodBytes, goodSha, testDir);
    fails += Test12_InvalidManifestSha(testDir);
    fails += Test13_FileDeletedAfterHashMismatch(goodBytes, goodSha, testDir);
    fails += Test14_FileRemainsAfterSuccessVerified(goodBytes, goodSha, testDir);
}
finally
{
    try { Directory.Delete(testDir, recursive: true); }
    catch { /* best effort cleanup */ }
}

Console.WriteLine();
Console.WriteLine(fails == 0 ? "ALL TESTS PASSED" : $"FAILED TESTS: {fails}");
Console.WriteLine("NOTE: No DB. No migrations. No backup. No install. Fake HTTP only (no Internet).");
Console.WriteLine("TEST 15 (Release build) runs separately via dotnet build.");
Environment.Exit(fails == 0 ? 0 : 1);

static UpdateManifest SampleManifest(string sha) =>
    new()
    {
        AppVersion = "1.1.0",
        TargetDbVersion = 5,
        MinAppVersion = "1.0.0",
        PackageName = "MFFITNESS-1.1.0.zip",
        PackageSha256 = sha,
        ReleaseDate = new DateTime(2026, 8, 12),
        ReleaseNotesUrl = "https://github.com/ramon302espinal-design/MFFITNESS/releases/tag/v1.1.0"
    };

static string PackageUrl(string fileName) =>
    $"https://fake.github.local/releases/download/v1.1.0/{fileName}";

static string ToSha256Hex(byte[] data) =>
    Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

static HttpClient CreateClient(FakeDownloadHandler handler, TimeSpan? timeout = null)
{
    handler.Client = new HttpClient(handler, disposeHandler: false)
    {
        Timeout = timeout ?? TimeSpan.FromSeconds(30)
    };
    return handler.Client;
}

static int Test1_SuccessVerified(byte[] bytes, string sha, string dir)
{
    Console.WriteLine("===== TEST 1: Download exitoso + hash correcto =====");
    var handler = new FakeDownloadHandler();
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), bytes);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(SampleManifest(sha), PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.SuccessVerified
        && result.LocalFilePath != null
        && File.Exists(result.LocalFilePath)
        && result.FileSizeBytes == bytes.Length
        && string.Equals(result.ComputedSha256, sha, StringComparison.OrdinalIgnoreCase)
        && result.AppVersion == "1.1.0"
        && result.TargetDbVersion == 5;

    Print(ok, $"{result.Status} size={result.FileSizeBytes} sha={result.ComputedSha256?[..8]}...");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static int Test2_HashMismatch(byte[] bytes, string correctSha, string dir)
{
    Console.WriteLine("===== TEST 2: Hash incorrecto =====");
    var handler = new FakeDownloadHandler();
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), bytes);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    string wrongExpected = new string('a', 64);
    var result = dl.DownloadAndVerifyAsync(SampleManifest(wrongExpected), PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.HashMismatch
        && !File.Exists(Path.Combine(dir, "MFFITNESS-1.1.0.zip"))
        && !File.Exists(Path.Combine(dir, "MFFITNESS-1.1.0.zip.part"));

    Print(ok, $"{result.Status}: {result.Message}");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static int Test3_HttpUrlRejected(string dir)
{
    Console.WriteLine("===== TEST 3: URL HTTP rechazada =====");
    using var dl = new UpdatePackageDownloader(downloadDirectory: dir);
    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(new string('b', 64)),
            "http://insecure.example/MFFITNESS-1.1.0.zip")
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.InvalidUrl;
    Print(ok, $"{result.Status}: {result.Message}");
    return ok ? 0 : 1;
}

static int Test4_HttpsUrlAccepted(byte[] bytes, string sha, string dir)
{
    Console.WriteLine("===== TEST 4: URL HTTPS aceptada =====");
    var handler = new FakeDownloadHandler();
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), bytes);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(SampleManifest(sha), PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.SuccessVerified
        && UpdatePackageDownloader.IsHttpsUrl(PackageUrl("MFFITNESS-1.1.0.zip"));

    Print(ok, $"{result.Status} https={UpdatePackageDownloader.IsHttpsUrl(PackageUrl("MFFITNESS-1.1.0.zip"))}");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static int Test5_EmptyFile(string dir)
{
    Console.WriteLine("===== TEST 5: Archivo vacío =====");
    var handler = new FakeDownloadHandler();
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), Array.Empty<byte>());
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(new string('c', 64)),
            PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.FileError;
    Print(ok, $"{result.Status}: {result.Message}");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static int Test6_Http404(string dir)
{
    Console.WriteLine("===== TEST 6: HTTP 404 =====");
    var handler = new FakeDownloadHandler();
    handler.SetStatus(PackageUrl("MFFITNESS-1.1.0.zip"), HttpStatusCode.NotFound);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(new string('d', 64)),
            PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.HttpError && result.HttpStatusCode == 404;
    Print(ok, $"{result.Status} code={result.HttpStatusCode}");
    return ok ? 0 : 1;
}

static int Test7_Http403(string dir)
{
    Console.WriteLine("===== TEST 7: HTTP 403 =====");
    var handler = new FakeDownloadHandler();
    handler.SetStatus(PackageUrl("MFFITNESS-1.1.0.zip"), HttpStatusCode.Forbidden);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(new string('e', 64)),
            PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.HttpError && result.HttpStatusCode == 403;
    Print(ok, $"{result.Status} code={result.HttpStatusCode}");
    return ok ? 0 : 1;
}

static int Test8_Http500(string dir)
{
    Console.WriteLine("===== TEST 8: HTTP 500 =====");
    var handler = new FakeDownloadHandler();
    handler.SetStatus(PackageUrl("MFFITNESS-1.1.0.zip"), HttpStatusCode.InternalServerError);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(new string('f', 64)),
            PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.HttpError && result.HttpStatusCode == 500;
    Print(ok, $"{result.Status} code={result.HttpStatusCode}");
    return ok ? 0 : 1;
}

static int Test9_Timeout(string dir)
{
    Console.WriteLine("===== TEST 9: Timeout =====");
    var handler = new FakeDownloadHandler { Delay = TimeSpan.FromSeconds(5) };
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), Encoding.UTF8.GetBytes("slow"));
    using var client = CreateClient(handler, TimeSpan.FromMilliseconds(200));
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(new string('a', 64)),
            PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.Timeout;
    Print(ok, $"{result.Status}: {result.Message}");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static int Test10_Cancellation(byte[] bytes, string dir)
{
    Console.WriteLine("===== TEST 10: CancellationToken =====");
    var handler = new FakeDownloadHandler { Delay = TimeSpan.FromSeconds(2) };
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), bytes);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(new string('a', 64)),
            PackageUrl("MFFITNESS-1.1.0.zip"),
            cts.Token)
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.Cancelled;
    Print(ok, $"{result.Status}: {result.Message}");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static int Test11_PackageNameMismatch(byte[] bytes, string sha, string dir)
{
    Console.WriteLine("===== TEST 11: PackageName ≠ asset URL =====");
    var handler = new FakeDownloadHandler();
    handler.SetBytes(PackageUrl("OTHER-PACKAGE.zip"), bytes);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(
            SampleManifest(sha),
            PackageUrl("OTHER-PACKAGE.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.InvalidUrl
        && result.Message.Contains("PackageName", StringComparison.OrdinalIgnoreCase);

    Print(ok, $"{result.Status}: {result.Message}");
    return ok ? 0 : 1;
}

static int Test12_InvalidManifestSha(string dir)
{
    Console.WriteLine("===== TEST 12: Manifest SHA inválido =====");
    using var dl = new UpdatePackageDownloader(downloadDirectory: dir);
    var bad = SampleManifest("abc");
    var result = dl.DownloadAndVerifyAsync(bad, PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    bool ok = result.Status == PackageDownloadStatus.InvalidManifest;
    Print(ok, $"{result.Status}: {result.Message}");
    return ok ? 0 : 1;
}

static int Test13_FileDeletedAfterHashMismatch(byte[] bytes, string sha, string dir)
{
    Console.WriteLine("===== TEST 13: Archivo eliminado tras HashMismatch =====");
    var handler = new FakeDownloadHandler();
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), bytes);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    string wrongSha = new string('0', 64);
    var result = dl.DownloadAndVerifyAsync(SampleManifest(wrongSha), PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    string finalPath = Path.Combine(dir, "MFFITNESS-1.1.0.zip");
    string partPath = Path.Combine(dir, "MFFITNESS-1.1.0.zip.part");

    bool ok = result.Status == PackageDownloadStatus.HashMismatch
        && !File.Exists(finalPath)
        && !File.Exists(partPath);

    Print(ok, $"exists_final={File.Exists(finalPath)} exists_part={File.Exists(partPath)}");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static int Test14_FileRemainsAfterSuccessVerified(byte[] bytes, string sha, string dir)
{
    Console.WriteLine("===== TEST 14: Archivo permanece tras SuccessVerified =====");
    var handler = new FakeDownloadHandler();
    handler.SetBytes(PackageUrl("MFFITNESS-1.1.0.zip"), bytes);
    using var client = CreateClient(handler);
    using var dl = new UpdatePackageDownloader(client, downloadDirectory: dir);

    var result = dl.DownloadAndVerifyAsync(SampleManifest(sha), PackageUrl("MFFITNESS-1.1.0.zip"))
        .GetAwaiter().GetResult();

    string finalPath = Path.Combine(dir, "MFFITNESS-1.1.0.zip");
    bool ok = result.Status == PackageDownloadStatus.SuccessVerified
        && File.Exists(finalPath)
        && new FileInfo(finalPath).Length == bytes.Length;

    Print(ok, $"exists={File.Exists(finalPath)} size={new FileInfo(finalPath).Length}");
    CleanupPackage(dir);
    return ok ? 0 : 1;
}

static void CleanupPackage(string dir)
{
    TryDelete(Path.Combine(dir, "MFFITNESS-1.1.0.zip"));
    TryDelete(Path.Combine(dir, "MFFITNESS-1.1.0.zip.part"));
}

static void TryDelete(string path)
{
    try { if (File.Exists(path)) File.Delete(path); }
    catch { /* ignore */ }
}

static void Print(bool ok, string detail)
{
    Console.WriteLine(ok ? "  PASS" : "  FAIL");
    Console.WriteLine("  " + detail);
}

sealed class FakeDownloadHandler : HttpMessageHandler
{
    private readonly Dictionary<string, byte[]> _bytes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HttpStatusCode> _status = new(StringComparer.OrdinalIgnoreCase);

    public HttpClient? Client { get; set; }
    public TimeSpan Delay { get; set; }

    public void SetBytes(string url, byte[] data) => _bytes[url] = data;

    public void SetStatus(string url, HttpStatusCode status) => _status[url] = status;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (Delay > TimeSpan.Zero)
            await Task.Delay(Delay, cancellationToken).ConfigureAwait(false);

        string url = request.RequestUri?.ToString() ?? string.Empty;

        if (_status.TryGetValue(url, out HttpStatusCode code))
            return new HttpResponseMessage(code);

        if (!_bytes.TryGetValue(url, out byte[]? data))
            return new HttpResponseMessage(HttpStatusCode.NotFound);

        var content = new ByteArrayContent(data);
        content.Headers.ContentLength = data.LongLength;
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }
}
