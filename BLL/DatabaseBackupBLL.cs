using CORE;
using DL;
using DL.Backup;

namespace BLL
{
    /// <summary>
    /// Facade de backup SQL. Sin WinForms. Usable por UpdateManager.
    /// </summary>
    public static class DatabaseBackupBLL
    {
        public static DatabaseBackupResult CreateVerifiedBackup()
        {
            var service = new DatabaseBackupService(new DBHelper(), MigrationLog.Write);
            return service.CreateVerifiedBackup(BackupStorage.CarpetaBackups);
        }
    }
}
