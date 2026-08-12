using System.Text.Json;
using BLL;
using BLL.Update;
using CORE.Update;

namespace UpdateManager;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("MFFITNESS UpdateManager — orquestador E2E (FASE 10B)");
        Console.WriteLine("Proceso externo. Flujo: Backup → Snapshot → Close UI → Install → Migrate → Health → Start.");

        if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        if (args.Contains("--recover", StringComparer.OrdinalIgnoreCase))
            return RunRecover(args);

        string? requestPath = GetArg(args, "--request");
        if (string.IsNullOrWhiteSpace(requestPath) || !File.Exists(requestPath))
        {
            Console.Error.WriteLine("ERROR: --request <ruta.json> requerido (o use --recover).");
            return 2;
        }

        try
        {
            string json = File.ReadAllText(requestPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Acepta UpdateEndToEndRequest o UpdateInstallRequest legacy.
            UpdateEndToEndRequest request = ParseRequest(json);
            var result = UpdateEndToEndBLL.Run(request);
            PrintResult(result);
            return ExitCode(result);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ERROR: " + ex.Message);
            return 1;
        }
    }

    private static int RunRecover(string[] args)
    {
        int staleMinutes = 15;
        string? staleArg = GetArg(args, "--stale-minutes");
        if (int.TryParse(staleArg, out int parsed) && parsed > 0)
            staleMinutes = parsed;

        var storage = new UpdateSessionStorage();
        var stale = storage.FindStaleSessions(TimeSpan.FromMinutes(staleMinutes));
        var pending = storage.FindPendingSessions();

        var targets = stale.Count > 0 ? stale : pending;
        if (targets.Count == 0)
        {
            Console.WriteLine("No hay sesiones pendientes/stale para recuperar.");
            return 0;
        }

        int worst = 0;
        foreach (var session in targets)
        {
            Console.WriteLine($"Recover session {session.UpdateId} stage={session.CurrentStage} status={session.Status}");
            var result = UpdateEndToEndBLL.Recover(session);
            PrintResult(result);
            worst = Math.Max(worst, ExitCode(result));
        }

        return worst;
    }

    private static UpdateEndToEndRequest ParseRequest(string json)
    {
        var options = JsonOptions();
        var e2e = JsonSerializer.Deserialize<UpdateEndToEndRequest>(json, options);
        if (e2e?.Manifest != null && !string.IsNullOrWhiteSpace(e2e.PackagePath))
            return e2e;

        var legacy = JsonSerializer.Deserialize<UpdateInstallRequest>(json, options);
        if (legacy?.Manifest == null)
            throw new InvalidOperationException("request JSON inválido: falta Manifest.");

        return new UpdateEndToEndRequest
        {
            Manifest = legacy.Manifest,
            PackagePath = legacy.PackagePath,
            ExpectedSha256 = legacy.ExpectedSha256,
            PackageVerified = legacy.PackageVerified,
            InstallDirectory = legacy.InstallDirectory,
            UiExecutableName = legacy.UiExecutableName,
            UiCloseTimeout = legacy.UiCloseTimeout,
            StartApplicationAfterInstall = legacy.StartApplicationAfterInstall,
            StagingDirectory = legacy.StagingDirectory,
            SnapshotDirectory = legacy.SnapshotDirectory
        };
    }

    private static void PrintResult(UpdateEndToEndResult result)
    {
        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Stage: {result.Stage}");
        Console.WriteLine($"Success: {result.Success} Blocked: {result.Blocked} Recovery: {result.RecoveryStatus}");
        Console.WriteLine($"Message: {result.Message}");
        if (!string.IsNullOrWhiteSpace(result.ErrorMessage) && result.ErrorMessage != result.Message)
            Console.WriteLine($"Error: {result.ErrorMessage}");
        if (result.UpdateId != null)
            Console.WriteLine($"UpdateId: {result.UpdateId}");
        if (result.BackupPath != null)
            Console.WriteLine($"Backup: {result.BackupPath}");
        if (result.SnapshotPath != null)
            Console.WriteLine($"Snapshot: {result.SnapshotPath}");
        Console.WriteLine($"App: {result.AppVersionBefore} → {result.AppVersionAfter}");
        Console.WriteLine($"DB: {result.DbVersionBefore} → {result.DbVersionAfter}");
    }

    private static int ExitCode(UpdateEndToEndResult result)
    {
        if (result.Success) return 0;
        if (result.Blocked) return 10;
        return result.Status switch
        {
            UpdateSessionStatus.FailedRecovered => 11,
            UpdateSessionStatus.FailedRecoveryRequired => 12,
            UpdateSessionStatus.RecoveryRequired => 13,
            _ => 1
        };
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("  UpdateManager.exe --request <e2e-or-install-request.json>");
        Console.WriteLine("  UpdateManager.exe --recover [--stale-minutes N]");
        Console.WriteLine();
        Console.WriteLine("Exit codes: 0=Completed 10=Blocked 11=FailedRecovered");
        Console.WriteLine("            12=FailedRecoveryRequired 13=RecoveryRequired 1=Failed");
    }

    private static string? GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}
