namespace CORE.Update
{
    /// <summary>
    /// Estado de compensación/recovery de la sesión.
    /// Completed solo es válido si RecoveryStatus == None.
    /// </summary>
    public enum UpdateRecoveryStatus
    {
        None = 0,
        Attempted = 1,
        Succeeded = 2,
        Failed = 3,
        Required = 4
    }
}
