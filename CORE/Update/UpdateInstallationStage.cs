namespace CORE.Update
{
    public enum UpdateInstallationStage
    {
        Checking,
        Blocked,
        Preparing,
        SnapshotCreated,
        StoppingApplication,
        Extracting,
        ValidatingPackage,
        Installing,
        Verifying,
        HealthChecking,
        StartingApplication,
        Completed,
        Failed,
        /// <summary>Instalación falló; binarios restaurados desde snapshot (SHA256 OK).</summary>
        FailedRecovered,
        /// <summary>Instalación falló; restauración automática no pudo garantizar el estado anterior.</summary>
        FailedRecoveryRequired
    }
}
