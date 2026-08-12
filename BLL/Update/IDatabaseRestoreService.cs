using DL.Backup;

namespace BLL.Update
{
    public interface IDatabaseRestoreService
    {
        DatabaseRestoreResult RestoreFromBackup(
            string backupPath,
            int expectedSchemaVersion,
            Func<bool>? isUiClosed = null);
    }
}
