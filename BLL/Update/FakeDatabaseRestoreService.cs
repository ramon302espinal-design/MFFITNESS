using DL.Backup;

namespace BLL.Update
{
    /// <summary>Fake offline para smoke E2E. No toca SQL real.</summary>
    public sealed class FakeDatabaseRestoreService : IDatabaseRestoreService
    {
        public bool ShouldSucceed { get; set; } = true;
        public int SchemaVersionAfterRestore { get; set; }
        public bool RequireUiClosed { get; set; } = true;
        public int CallCount { get; private set; }
        public string? LastBackupPath { get; private set; }
        public List<string> Log { get; } = new();

        public DatabaseRestoreResult RestoreFromBackup(
            string backupPath,
            int expectedSchemaVersion,
            Func<bool>? isUiClosed = null)
        {
            CallCount++;
            LastBackupPath = backupPath;
            Log.Add($"restore call backup={backupPath} expected={expectedSchemaVersion}");

            if (RequireUiClosed && isUiClosed != null && !isUiClosed())
            {
                Log.Add("UI abierta");
                return DatabaseRestoreResult.Fail("UI no cerrada (fake).", backupPath, log: Log.ToList());
            }

            if (!ShouldSucceed)
            {
                Log.Add("restore fallido (fake)");
                return DatabaseRestoreResult.Fail("RESTORE simulado fallido.", backupPath, log: Log.ToList());
            }

            int after = SchemaVersionAfterRestore != 0 ? SchemaVersionAfterRestore : expectedSchemaVersion;
            if (after != expectedSchemaVersion)
            {
                Log.Add($"mismatch {after}!={expectedSchemaVersion}");
                return DatabaseRestoreResult.Fail(
                    $"SchemaVersion post-restore {after} != {expectedSchemaVersion}",
                    backupPath,
                    "FakeDb",
                    after,
                    expectedSchemaVersion,
                    Log.ToList());
            }

            Log.Add("restore OK (fake)");
            return DatabaseRestoreResult.Ok("FakeDb", backupPath, after, expectedSchemaVersion, Log.ToList());
        }
    }
}
