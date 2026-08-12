namespace CORE.Update
{
    /// <summary>
    /// Etapas del orquestador end-to-end (FASE 10B). Persistidas en UpdateSession.CurrentStage.
    /// </summary>
    public enum UpdateEndToEndStage
    {
        Checking = 0,
        Prepared = 1,
        BackupCreated = 2,
        SnapshotCreated = 3,
        UiClosed = 4,
        BinariesInstalled = 5,
        DbMigrated = 6,
        HealthCheckPassed = 7,
        StartingApplication = 8,
        FinalVerification = 9,
        Completed = 10,
        Blocked = 11,
        Failed = 12,
        FailedRecovered = 13,
        FailedRecoveryRequired = 14,
        RecoveryRequired = 15
    }
}
