namespace CORE.Update
{
    public sealed class UpdateResult
    {
        public string CurrentAppVersion { get; init; } = string.Empty;
        public string TargetAppVersion { get; init; } = string.Empty;
        public int CurrentDbVersion { get; init; }
        public int TargetDbVersion { get; init; }
        public int FinalDbVersion { get; init; }
        public bool? CajaAbierta { get; init; }
        public bool BackupCreated { get; init; }
        public bool BackupVerified { get; init; }
        public string? BackupPath { get; init; }
        public bool MigrationsApplied { get; init; }
        public IReadOnlyList<int> AppliedMigrationVersions { get; init; } = Array.Empty<int>();
        public bool Success { get; init; }
        public bool Blocked { get; init; }
        public string? ErrorMessage { get; init; }
        public UpdateStage Stage { get; init; }

        public static UpdateResult CreateBlocked(
            string currentApp,
            string targetApp,
            int currentDb,
            int targetDb,
            bool? cajaAbierta,
            string reason) =>
            new()
            {
                CurrentAppVersion = currentApp,
                TargetAppVersion = targetApp,
                CurrentDbVersion = currentDb,
                TargetDbVersion = targetDb,
                FinalDbVersion = currentDb,
                CajaAbierta = cajaAbierta,
                Success = false,
                Blocked = true,
                ErrorMessage = reason,
                Stage = UpdateStage.Blocked
            };

        public static UpdateResult CreateFailed(
            string currentApp,
            string targetApp,
            int currentDb,
            int targetDb,
            int finalDb,
            bool? cajaAbierta,
            bool backupCreated,
            bool backupVerified,
            string? backupPath,
            bool migrationsApplied,
            IReadOnlyList<int>? applied,
            string error,
            UpdateStage stage) =>
            new()
            {
                CurrentAppVersion = currentApp,
                TargetAppVersion = targetApp,
                CurrentDbVersion = currentDb,
                TargetDbVersion = targetDb,
                FinalDbVersion = finalDb,
                CajaAbierta = cajaAbierta,
                BackupCreated = backupCreated,
                BackupVerified = backupVerified,
                BackupPath = backupPath,
                MigrationsApplied = migrationsApplied,
                AppliedMigrationVersions = applied ?? Array.Empty<int>(),
                Success = false,
                Blocked = false,
                ErrorMessage = error,
                Stage = stage
            };

        public static UpdateResult CreateSuccess(
            string currentApp,
            string targetApp,
            int currentDb,
            int targetDb,
            int finalDb,
            bool cajaAbierta,
            bool backupCreated,
            bool backupVerified,
            string? backupPath,
            bool migrationsApplied,
            IReadOnlyList<int>? applied) =>
            new()
            {
                CurrentAppVersion = currentApp,
                TargetAppVersion = targetApp,
                CurrentDbVersion = currentDb,
                TargetDbVersion = targetDb,
                FinalDbVersion = finalDb,
                CajaAbierta = cajaAbierta,
                BackupCreated = backupCreated,
                BackupVerified = backupVerified,
                BackupPath = backupPath,
                MigrationsApplied = migrationsApplied,
                AppliedMigrationVersions = applied ?? Array.Empty<int>(),
                Success = true,
                Blocked = false,
                Stage = UpdateStage.Completed
            };
    }
}
