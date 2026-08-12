namespace DL.Backup
{
    public sealed class DatabaseRestoreResult
    {
        public bool Success { get; init; }
        public string? BackupPath { get; init; }
        public string DatabaseName { get; init; } = string.Empty;
        public int? SchemaVersionAfter { get; init; }
        public int? ExpectedSchemaVersion { get; init; }
        public string Message { get; init; } = string.Empty;
        public IReadOnlyList<string> CompensationLog { get; init; } = Array.Empty<string>();

        public static DatabaseRestoreResult Ok(
            string databaseName,
            string backupPath,
            int schemaVersionAfter,
            int expectedSchemaVersion,
            IReadOnlyList<string> log) =>
            new()
            {
                Success = true,
                DatabaseName = databaseName,
                BackupPath = backupPath,
                SchemaVersionAfter = schemaVersionAfter,
                ExpectedSchemaVersion = expectedSchemaVersion,
                Message = $"RESTORE OK. SchemaVersion={schemaVersionAfter}.",
                CompensationLog = log
            };

        public static DatabaseRestoreResult Fail(
            string message,
            string? backupPath = null,
            string databaseName = "",
            int? schemaVersionAfter = null,
            int? expectedSchemaVersion = null,
            IReadOnlyList<string>? log = null) =>
            new()
            {
                Success = false,
                BackupPath = backupPath,
                DatabaseName = databaseName,
                SchemaVersionAfter = schemaVersionAfter,
                ExpectedSchemaVersion = expectedSchemaVersion,
                Message = message,
                CompensationLog = log ?? Array.Empty<string>()
            };
    }
}
