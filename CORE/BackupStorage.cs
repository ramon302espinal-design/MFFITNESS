using System.IO;

namespace CORE
{
    /// <summary>
    /// Carpeta de backups SQL: %LocalAppData%\MFFITNESS\backups
    /// </summary>
    public static class BackupStorage
    {
        public static string CarpetaBackups
        {
            get
            {
                string dir = Path.Combine(FacturaStorage.CarpetaRaizMffitness, "backups");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }
    }
}
