namespace CORE.Update
{
    public enum UpdateStage
    {
        Checking,
        Blocked,
        BackingUp,
        Migrating,
        Verifying,
        Completed,
        Failed
    }
}
