using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BLL.Services
{
    /// <summary>
    /// Evita registrar la misma factura dos veces (hash de archivo + egreso en caja abierta).
    /// </summary>
    public static class FacturaGastoDuplicateGuard
    {
        private const string HashFileName = "_hashes_ok.txt";
        private static readonly object Sync = new();

        public static string ComputeFileSha256(string filePath)
        {
            using var fs = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(fs);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public static string ShortHash(string sha256) =>
            string.IsNullOrEmpty(sha256) || sha256.Length < 12
                ? sha256
                : sha256[..12];

        /// <summary>Duplicado por contenido de archivo (hash persistido o egreso con mismo hash).</summary>
        public static bool IsFileHashDuplicate(
            string facturaRoot,
            string sha256,
            string? fileName,
            out string reason)
        {
            reason = string.Empty;
            string shortHash = ShortHash(sha256);

            if (IsHashRemembered(facturaRoot, sha256))
            {
                reason =
                    $"Esta factura ya fue registrada (archivo duplicado).\n" +
                    $"Hash: {shortHash}\nArchivo: {fileName}";
                return true;
            }

            try
            {
                var bll = new CajaBLL();
                if (!bll.ObtenerEstadoCaja())
                    return false;

                DataTable movs = bll.MovimientosHoy();
                foreach (DataRow row in movs.Rows)
                {
                    string tipo = row["TipoMovimiento"]?.ToString() ?? string.Empty;
                    if (!tipo.Equals("EGRESO", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string conc = row["Concepto"]?.ToString() ?? string.Empty;
                    if (conc.Contains("[hash:" + shortHash + "]", StringComparison.OrdinalIgnoreCase)
                        || conc.Contains(sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        reason =
                            $"Factura duplicada: ya existe un egreso hoy con el mismo archivo.\n" +
                            $"Hash: {shortHash}\nArchivo: {fileName}";
                        return true;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        /// <summary>
        /// True si ya se procesó este archivo o ya existe egreso equivalente hoy.
        /// </summary>
        public static bool IsDuplicate(
            string facturaRoot,
            string sha256,
            string? fileName,
            decimal monto,
            string concepto,
            out string reason)
        {
            if (IsFileHashDuplicate(facturaRoot, sha256, fileName, out reason))
                return true;

            reason = string.Empty;

            try
            {
                var bll = new CajaBLL();
                if (!bll.ObtenerEstadoCaja())
                    return false;

                DataTable movs = bll.MovimientosHoy();
                foreach (DataRow row in movs.Rows)
                {
                    string tipo = row["TipoMovimiento"]?.ToString() ?? string.Empty;
                    if (!tipo.Equals("EGRESO", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string conc = row["Concepto"]?.ToString() ?? string.Empty;
                    decimal rowMonto = 0m;
                    if (row["Monto"] != DBNull.Value)
                        rowMonto = Convert.ToDecimal(row["Monto"], CultureInfo.InvariantCulture);

                    // Misma foto por nombre + mismo total.
                    if (!string.IsNullOrWhiteSpace(fileName)
                        && conc.Contains(fileName, StringComparison.OrdinalIgnoreCase)
                        && Math.Abs(rowMonto - monto) < 0.01m)
                    {
                        reason =
                            $"Factura duplicada: ya hay un egreso hoy con el mismo archivo y monto.\n" +
                            $"Archivo: {fileName}\nMonto: {monto:C}";
                        return true;
                    }

                    // Mismo comercio (1ª línea) + mismo monto (solo autos).
                    string headNew = FirstLine(concepto);
                    string headOld = FirstLine(conc);
                    if (!string.IsNullOrWhiteSpace(headNew)
                        && headNew.Length >= 4
                        && string.Equals(NormalizeKey(headNew), NormalizeKey(headOld), StringComparison.Ordinal)
                        && Math.Abs(rowMonto - monto) < 0.01m
                        && conc.Contains("[Auto FacturaGastos:", StringComparison.OrdinalIgnoreCase))
                    {
                        reason =
                            $"Factura duplicada: ya existe un egreso automático hoy con el mismo comercio y total.\n" +
                            $"Comercio: {headNew}\nMonto: {monto:C}";
                        return true;
                    }
                }
            }
            catch
            {
                // Si falla la lectura de caja, no bloquear por este camino.
            }

            return false;
        }

        public static void RememberHash(string facturaRoot, string sha256)
        {
            if (string.IsNullOrWhiteSpace(facturaRoot) || string.IsNullOrWhiteSpace(sha256))
                return;

            try
            {
                Directory.CreateDirectory(facturaRoot);
                string path = Path.Combine(facturaRoot, HashFileName);
                lock (Sync)
                {
                    if (IsHashRemembered(facturaRoot, sha256))
                        return;

                    File.AppendAllText(
                        path,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + sha256.ToLowerInvariant() + Environment.NewLine,
                        Encoding.UTF8);
                }
            }
            catch
            {
                // best-effort
            }
        }

        private static bool IsHashRemembered(string facturaRoot, string sha256)
        {
            try
            {
                string path = Path.Combine(facturaRoot, HashFileName);
                if (!File.Exists(path))
                    return false;

                string wanted = sha256.ToLowerInvariant();
                lock (Sync)
                {
                    foreach (string line in File.ReadLines(path))
                    {
                        if (line.Contains(wanted, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        private static string FirstLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            using var reader = new StringReader(text.Trim());
            return reader.ReadLine()?.Trim() ?? string.Empty;
        }

        private static string NormalizeKey(string s)
        {
            string t = s.Trim().ToUpperInvariant();
            t = Regex.Replace(t, @"\s+", " ");
            int idx = t.IndexOf("[AUTO", StringComparison.Ordinal);
            if (idx > 0)
                t = t[..idx].Trim();
            return t;
        }
    }
}
