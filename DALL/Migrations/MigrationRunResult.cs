namespace DL.Migrations
{
    public sealed class MigrationRunResult
    {
        public bool Success { get; init; }
        public int InitialVersion { get; init; }
        public int FinalVersion { get; init; }
        public IReadOnlyList<int> AppliedVersions { get; init; } = Array.Empty<int>();
        public string Message { get; init; } = string.Empty;
        public string? FailedMigration { get; init; }

        public static MigrationRunResult Ok(int initial, int final, IReadOnlyList<int> applied, string message) =>
            new()
            {
                Success = true,
                InitialVersion = initial,
                FinalVersion = final,
                AppliedVersions = applied,
                Message = message
            };

        public static MigrationRunResult Fail(int initial, int final, string message, string? failedMigration = null) =>
            new()
            {
                Success = false,
                InitialVersion = initial,
                FinalVersion = final,
                Message = message,
                FailedMigration = failedMigration
            };
    }
}
