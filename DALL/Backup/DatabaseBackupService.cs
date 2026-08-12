using Microsoft.Data.SqlClient;

namespace DL.Backup
{
    /// <summary>
    /// Backup nativo de SQL Server + RESTORE VERIFYONLY. Sin WinForms.
    /// Reutilizable desde BLL y un futuro UpdateManager.
    /// </summary>
    public sealed class DatabaseBackupService
    {
        private readonly DBHelper _db;
        private readonly Action<string> _log;

        public DatabaseBackupService(DBHelper db, Action<string>? log = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _log = log ?? (_ => { });
        }

        public DatabaseBackupResult CreateVerifiedBackup(string backupDirectory)
        {
            DateTime createdAt = DateTime.Now;
            string databaseName = string.Empty;
            string? path = null;

            try
            {
                if (string.IsNullOrWhiteSpace(backupDirectory))
                    return DatabaseBackupResult.Fail(databaseName, createdAt, "Directorio de backup no especificado.");

                Directory.CreateDirectory(backupDirectory);
                path = AllocateUniquePath(backupDirectory, createdAt);

                using SqlConnection conn = _db.GetConnection();
                conn.Open();

                databaseName = ReadDatabaseName(conn);
                _log($"backup inicio: base de datos [{databaseName}]");
                _log($"backup ruta: {path}");

                RunBackup(conn, databaseName, path);
                _log("backup BACKUP DATABASE: OK");

                if (!File.Exists(path))
                {
                    string err = "BACKUP DATABASE terminó pero el archivo .bak no existe.";
                    _log($"backup error: {err}");
                    return DatabaseBackupResult.Fail(databaseName, createdAt, err, path);
                }

                long size = new FileInfo(path).Length;
                _log($"backup tamaño: {size} bytes");
                if (size <= 0)
                {
                    string err = "El archivo .bak existe pero el tamaño es 0.";
                    _log($"backup error: {err}");
                    return DatabaseBackupResult.Fail(databaseName, createdAt, err, path, size);
                }

                RunVerifyOnly(conn, path);
                _log("backup RESTORE VERIFYONLY: OK");
                _log($"backup finalización: Success=true Verified=true {path}");

                return new DatabaseBackupResult
                {
                    Success = true,
                    BackupPath = path,
                    DatabaseName = databaseName,
                    CreatedAt = createdAt,
                    SizeBytes = size,
                    Verified = true
                };
            }
            catch (Exception ex)
            {
                _log($"backup error: {ex.Message}");
                long size = 0;
                try
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        size = new FileInfo(path).Length;
                }
                catch { /* ignore */ }

                return DatabaseBackupResult.Fail(databaseName, createdAt, ex.Message, path, size);
            }
        }

        private static string AllocateUniquePath(string directory, DateTime createdAt)
        {
            string stamp = createdAt.ToString("yyyyMMdd_HHmmss");
            string path = Path.Combine(directory, $"MFFITNESS_{stamp}.bak");
            int n = 2;
            while (File.Exists(path))
            {
                path = Path.Combine(directory, $"MFFITNESS_{stamp}_{n}.bak");
                n++;
            }
            return path;
        }

        private static string ReadDatabaseName(SqlConnection conn)
        {
            using var cmd = new SqlCommand("SELECT DB_NAME();", conn);
            object? value = cmd.ExecuteScalar();
            string name = value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("No se pudo obtener el nombre de la base de datos.");
            return name;
        }

        private static string QuoteIdentifier(string name) =>
            "[" + name.Replace("]", "]]") + "]";

        private static void RunBackup(SqlConnection conn, string databaseName, string path)
        {
            using var cmd = new SqlCommand($@"
BACKUP DATABASE {QuoteIdentifier(databaseName)}
TO DISK = @BackupPath
WITH COPY_ONLY, CHECKSUM, INIT;", conn)
            {
                CommandTimeout = 300
            };
            cmd.Parameters.AddWithValue("@BackupPath", path);
            cmd.ExecuteNonQuery();
        }

        private static void RunVerifyOnly(SqlConnection conn, string path)
        {
            using var cmd = new SqlCommand(@"
RESTORE VERIFYONLY
FROM DISK = @BackupPath
WITH CHECKSUM;", conn)
            {
                CommandTimeout = 300
            };
            cmd.Parameters.AddWithValue("@BackupPath", path);
            cmd.ExecuteNonQuery();
        }
    }
}
