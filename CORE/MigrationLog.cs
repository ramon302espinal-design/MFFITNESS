using System.IO;

namespace CORE
{
    /// <summary>
    /// Log mínimo de migraciones en %LocalAppData%\MFFITNESS\logs (mismo patrón que WhatsApp).
    /// </summary>
    public static class MigrationLog
    {
        public static void Write(string message)
        {
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
            System.Diagnostics.Debug.WriteLine(line);

            try
            {
                string dir = Path.Combine(FacturaStorage.CarpetaRaizMffitness, "logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(
                    Path.Combine(dir, $"migrations-{DateTime.Today:yyyyMMdd}.log"),
                    line + Environment.NewLine);
            }
            catch
            {
                // No bloquear el runner por fallos de log.
            }
        }
    }
}
