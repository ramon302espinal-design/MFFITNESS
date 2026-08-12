using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CORE.Update;

namespace UpdatePackageBuilder;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || HasFlag(args, "--help") || HasFlag(args, "-h"))
            {
                PrintUsage();
                return args.Length == 0 ? 1 : 0;
            }

            string source = RequireArg(args, "--source");
            string output = GetArg(args, "--out") ?? Path.Combine(source, "..", "update-package");
            string appVersion = GetArg(args, "--app-version") ?? ReadAppVersionFromUi(source) ?? "0.0.0";
            string minAppVersion = GetArg(args, "--min-app-version") ?? "1.0.0";
            int targetDb = int.TryParse(GetArg(args, "--target-db-version"), out int db)
                ? db
                : DetectMaxMigrationVersion(source);
            string? notes = GetArg(args, "--release-notes-url");
            bool includePdbs = HasFlag(args, "--include-pdbs");

            if (!Directory.Exists(source))
            {
                Console.Error.WriteLine("ERROR: --source no existe: " + source);
                return 2;
            }

            foreach (string required in AllowedUpdatePackageFiles.RequiredFiles)
            {
                string path = Path.Combine(source, required);
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine("ERROR: falta archivo requerido en publish: " + required);
                    return 3;
                }
            }

            if (!SemVer.TryParse(appVersion, out _))
            {
                Console.Error.WriteLine("ERROR: --app-version no es SemVer: " + appVersion);
                return 4;
            }

            if (targetDb < 1)
            {
                Console.Error.WriteLine("ERROR: TargetDbVersion inválido.");
                return 5;
            }

            output = Path.GetFullPath(output);
            Directory.CreateDirectory(output);

            string packageName = $"MFFITNESS-{appVersion}.zip";
            string zipPath = Path.Combine(output, packageName);
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            var included = new List<string>();
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                {
                    string relative = Path.GetRelativePath(source, file).Replace('\\', '/');
                    if (!ShouldInclude(relative, includePdbs))
                        continue;

                    zip.CreateEntryFromFile(file, relative, CompressionLevel.Optimal);
                    included.Add(relative);
                }
            }

            if (included.Count == 0)
            {
                Console.Error.WriteLine("ERROR: el ZIP quedó vacío.");
                return 6;
            }

            foreach (string required in AllowedUpdatePackageFiles.RequiredFiles)
            {
                if (!included.Any(f => string.Equals(f, required, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.Error.WriteLine("ERROR: ZIP sin requerido: " + required);
                    return 7;
                }
            }

            if (included.Any(f =>
                    Path.GetFileName(f).StartsWith("UpdateManager.", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetFileName(f), AllowedUpdatePackageFiles.UpdateManagerExe,
                        StringComparison.OrdinalIgnoreCase)))
            {
                Console.Error.WriteLine("ERROR: artefactos UpdateManager.* no deben ir en el package.");
                return 8;
            }

            string sha = ComputeSha256Hex(zipPath);
            var manifest = new UpdateManifest
            {
                AppVersion = appVersion,
                TargetDbVersion = targetDb,
                MinAppVersion = minAppVersion,
                PackageName = packageName,
                PackageSha256 = sha,
                ReleaseDate = DateTime.UtcNow.Date,
                ReleaseNotesUrl = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            };

            var validation = UpdateManifestValidator.Validate(manifest);
            if (!validation.IsValid)
            {
                Console.Error.WriteLine("ERROR: manifest inválido: " + string.Join(" ", validation.Errors));
                return 9;
            }

            string manifestPath = Path.Combine(output, "manifest.json");
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), Encoding.UTF8);

            string listPath = Path.Combine(output, "package-files.txt");
            File.WriteAllLines(listPath, included.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

            Console.WriteLine("Update package OK");
            Console.WriteLine("  ZIP:      " + zipPath);
            Console.WriteLine("  Manifest: " + manifestPath);
            Console.WriteLine("  SHA256:   " + sha);
            Console.WriteLine("  App:      " + appVersion);
            Console.WriteLine("  TargetDb: " + targetDb);
            Console.WriteLine("  Files:    " + included.Count);
            Console.WriteLine("  NOTE: UpdateManager.exe excluido del ZIP (correcto).");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ERROR: " + ex.Message);
            return 1;
        }
    }

    private static bool ShouldInclude(string relative, bool includePdbs)
    {
        string n = AllowedUpdatePackageFiles.Normalize(relative);
        string fileName = Path.GetFileName(n);

        // UpdateManager se despliega solo en install inicial; nunca via package.
        if (n.StartsWith("updatemanager/", StringComparison.OrdinalIgnoreCase))
            return false;

        if (fileName.StartsWith("UpdateManager.", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fileName, AllowedUpdatePackageFiles.UpdateManagerExe, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // No incluir request/session/logs locales.
        if (n.StartsWith("updates/", StringComparison.OrdinalIgnoreCase))
            return false;
        if (n.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!includePdbs && n.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
            return false;

        return AllowedUpdatePackageFiles.IsAllowedRelativePath(n);
    }

    private static string? ReadAppVersionFromUi(string source)
    {
        string ui = Path.Combine(source, "UI.exe");
        if (!File.Exists(ui))
            return null;

        try
        {
            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(ui);
            string? v = info.ProductVersion ?? info.FileVersion;
            if (string.IsNullOrWhiteSpace(v))
                return null;

            // ProductVersion puede ser "1.0.0+abc" → tomar SemVer base
            int plus = v.IndexOf('+');
            if (plus > 0)
                v = v[..plus];

            return SemVer.TryParse(v, out _) ? v : null;
        }
        catch
        {
            return null;
        }
    }

    private static int DetectMaxMigrationVersion(string source)
    {
        string dir = Path.Combine(source, "Database", "Migrations");
        if (!Directory.Exists(dir))
            return 1;

        var rx = new Regex(@"^(?<v>\d{4})_.+\.sql$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        int max = 1;
        foreach (string file in Directory.EnumerateFiles(dir, "*.sql"))
        {
            var m = rx.Match(Path.GetFileName(file));
            if (!m.Success)
                continue;
            int v = int.Parse(m.Groups["v"].Value);
            if (v > max)
                max = v;
        }

        return max;
    }

    private static string ComputeSha256Hex(string path)
    {
        using var stream = File.OpenRead(path);
        byte[] hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
            UpdatePackageBuilder — genera ZIP + manifest.json para GitHub Release

            Uso:
              UpdatePackageBuilder --source <publish-dir> [--out <dir>]
                                  [--app-version 1.1.0] [--min-app-version 1.0.0]
                                  [--target-db-version 4] [--release-notes-url URL]
                                  [--include-pdbs]

            Reglas:
              - Incluye solo rutas whitelist (AllowedUpdatePackageFiles)
              - NUNCA incluye UpdateManager.exe
              - Calcula SHA256 del ZIP y lo escribe en manifest.json
            """);
    }

    private static bool HasFlag(string[] args, string name) =>
        args.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));

    private static string? GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }

    private static string RequireArg(string[] args, string name) =>
        GetArg(args, name) ?? throw new ArgumentException("Falta argumento " + name);
}
