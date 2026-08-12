namespace CORE.Update
{
    /// <summary>
    /// Resultado estructurado del orquestador end-to-end (FASE 10B).
    /// </summary>
    public sealed class UpdateEndToEndResult
    {
        public bool Success { get; init; }
        public bool Blocked { get; init; }
        public UpdateSessionStatus Status { get; init; }
        public UpdateEndToEndStage Stage { get; init; }
        public UpdateRecoveryStatus RecoveryStatus { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? ErrorMessage { get; init; }
        public string? UpdateId { get; init; }
        public string? AppVersionBefore { get; init; }
        public string? AppVersionAfter { get; init; }
        public int? DbVersionBefore { get; init; }
        public int? DbVersionAfter { get; init; }
        public string? BackupPath { get; init; }
        public string? SnapshotPath { get; init; }
        public bool BackupVerified { get; init; }
        public bool SnapshotVerified { get; init; }
        public bool HealthCheckPassed { get; init; }
        public bool ApplicationStarted { get; init; }
        public IReadOnlyList<int> AppliedMigrations { get; init; } = Array.Empty<int>();
        public IReadOnlyList<string> CompensationLog { get; init; } = Array.Empty<string>();

        public static UpdateEndToEndResult FromSession(UpdateSession session, string? message = null) =>
            new()
            {
                Success = session.Status == UpdateSessionStatus.Completed,
                Blocked = session.Status == UpdateSessionStatus.Blocked,
                Status = session.Status,
                Stage = session.CurrentStage,
                RecoveryStatus = session.RecoveryStatus,
                Message = message ?? session.ErrorMessage ?? session.Status.ToString(),
                ErrorMessage = session.ErrorMessage,
                UpdateId = session.UpdateId,
                AppVersionBefore = session.AppVersionBefore,
                AppVersionAfter = session.AppVersionAfter,
                DbVersionBefore = session.DbVersionBefore,
                DbVersionAfter = session.DbVersionAfter,
                BackupPath = session.BackupPath,
                SnapshotPath = session.SnapshotPath,
                BackupVerified = session.BackupVerified,
                SnapshotVerified = session.SnapshotVerified,
                HealthCheckPassed = session.CurrentStage is UpdateEndToEndStage.HealthCheckPassed
                    or UpdateEndToEndStage.StartingApplication
                    or UpdateEndToEndStage.FinalVerification
                    or UpdateEndToEndStage.Completed,
                CompensationLog = session.CompensationLog.ToList()
            };
    }
}
