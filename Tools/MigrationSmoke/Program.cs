using BLL;
using DL.Migrations;

string? directory = args.Length > 0 ? args[0] : MigrationRunner.ResolveDefaultDirectory();
Console.WriteLine("Migrations dir: " + directory);

var result = SchemaMigrationBLL.ApplyPending(directory);
Console.WriteLine(result.Success ? "OK: " + result.Message : "FAIL: " + result.Message);

if (result.Payload is MigrationRunResult run)
{
    Console.WriteLine($"InitialVersion={run.InitialVersion}");
    Console.WriteLine($"FinalVersion={run.FinalVersion}");
    Console.WriteLine($"Applied=[{string.Join(",", run.AppliedVersions)}]");
    if (!string.IsNullOrEmpty(run.FailedMigration))
        Console.WriteLine("FailedMigration=" + run.FailedMigration);
}

Environment.Exit(result.Success ? 0 : 1);
