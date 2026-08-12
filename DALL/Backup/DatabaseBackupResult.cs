namespace DL.Backup
{
    public sealed class DatabaseBackupResult
    {
        public bool Success { get; init; }
        public string? BackupPath { get; init; }
        public string DatabaseName { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public long SizeBytes { get; init; }
        public bool Verified { get; init; }
        public string? ErrorMessage { get; init; }

        public static DatabaseBackupResult Fail(string databaseName, DateTime createdAt, string error, string? path = null, long size = 0) =>
            new()
            {
                Success = false,
                BackupPath = path,
                DatabaseName = databaseName,
                CreatedAt = createdAt,
                SizeBytes = size,
                Verified = false,
                ErrorMessage = error
            };
    }
}
