using System.IO;

namespace CORE
{
    /// <summary>
    /// Carpetas de actualización: downloads, staging, snapshots, sessions bajo %LocalAppData%\MFFITNESS\updates
    /// </summary>
    public static class UpdateDownloadStorage
    {
        public static string CarpetaUpdates
        {
            get
            {
                string dir = Path.Combine(FacturaStorage.CarpetaRaizMffitness, "updates");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string CarpetaDescargas
        {
            get
            {
                string dir = Path.Combine(CarpetaUpdates, "downloads");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string CarpetaStaging
        {
            get
            {
                string dir = Path.Combine(CarpetaUpdates, "staging");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string CarpetaSnapshots
        {
            get
            {
                string dir = Path.Combine(CarpetaUpdates, "snapshots");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string CarpetaSessions
        {
            get
            {
                string dir = Path.Combine(CarpetaUpdates, "sessions");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }
    }
}
