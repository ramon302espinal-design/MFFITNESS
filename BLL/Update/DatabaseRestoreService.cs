using DL;
using DL.Backup;
using Microsoft.Data.SqlClient;

namespace BLL.Update
{
    /// <summary>
    /// RESTORE DATABASE desde .bak. Sin down-migrations. Sin tocar tablas de negocio directamente.
    /// </summary>
    public sealed class DatabaseRestoreService : IDatabaseRestoreService
    {
        private readonly DBHelper _db;
        private readonly Action<string> _log;

        public DatabaseRestoreService(DBHelper? db = null, Action<string>? log = null)
        {
            _db = db ?? new DBHelper();
            _log = log ?? (_ => { });
        }

        public DatabaseRestoreResult RestoreFromBackup(
            string backupPath,
            int expectedSchemaVersion,
            Func<bool>? isUiClosed = null)
        {
            var compensation = new List<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
                    return DatabaseRestoreResult.Fail("BackupPath inválido o inexistente.", backupPath, log: compensation);

                if (isUiClosed != null && !isUiClosed())
                {
                    compensation.Add("UI todavía abierta; restore abortado.");
                    return DatabaseRestoreResult.Fail(
                        "UI no está cerrada. RESTORE abortado.",
                        backupPath,
                        log: compensation);
                }

                compensation.Add("UI cerrado confirmado (o no requerido).");
                _log("restore: confirmado UI cerrado / proceed");

                string databaseName;
                using (var probe = new SqlConnection(_db.ConnectionString))
                {
                    probe.Open();
                    databaseName = ReadDatabaseName(probe);
                }

                compensation.Add($"database={databaseName}");
                _log($"restore inicio: [{databaseName}] from {backupPath}");

                // Liberar conexiones del pool hacia la DB objetivo.
                SqlConnection.ClearAllPools();
                compensation.Add("SqlConnection.ClearAllPools()");
                _log("restore: pools liberados");

                string masterCs = BuildMasterConnectionString(_db.ConnectionString);
                using (var master = new SqlConnection(masterCs))
                {
                    master.Open();
                    compensation.Add("conectado a master");

                    SetSingleUser(master, databaseName);
                    compensation.Add("SINGLE_USER WITH ROLLBACK IMMEDIATE");
                    _log("restore: SINGLE_USER");

                    try
                    {
                        RunRestore(master, databaseName, backupPath);
                        compensation.Add("RESTORE DATABASE OK");
                        _log("restore: RESTORE DATABASE OK");
                    }
                    finally
                    {
                        try
                        {
                            SetMultiUser(master, databaseName);
                            compensation.Add("MULTI_USER");
                        }
                        catch (Exception ex)
                        {
                            compensation.Add("MULTI_USER falló: " + ex.Message);
                            _log("restore: MULTI_USER error " + ex.Message);
                        }
                    }
                }

                SqlConnection.ClearAllPools();
                compensation.Add("pools liberados post-restore");

                int actualVersion;
                using (var reconnect = new SqlConnection(_db.ConnectionString))
                {
                    reconnect.Open();
                    compensation.Add("reconectado a DB");
                    var info = SchemaVersionDAL.GetCurrent(reconnect);
                    actualVersion = info.Version;
                }

                compensation.Add($"SchemaVersion después={actualVersion}; esperado={expectedSchemaVersion}");
                _log($"restore SchemaVersion={actualVersion} esperado={expectedSchemaVersion}");

                if (actualVersion != expectedSchemaVersion)
                {
                    return DatabaseRestoreResult.Fail(
                        $"SchemaVersion post-restore {actualVersion} != esperado {expectedSchemaVersion}.",
                        backupPath,
                        databaseName,
                        actualVersion,
                        expectedSchemaVersion,
                        compensation);
                }

                return DatabaseRestoreResult.Ok(
                    databaseName,
                    backupPath,
                    actualVersion,
                    expectedSchemaVersion,
                    compensation);
            }
            catch (Exception ex)
            {
                compensation.Add("error: " + ex.Message);
                _log("restore error: " + ex.Message);
                return DatabaseRestoreResult.Fail(ex.Message, backupPath, log: compensation);
            }
        }

        private static string BuildMasterConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };
            return builder.ConnectionString;
        }

        private static string ReadDatabaseName(SqlConnection conn)
        {
            using var cmd = new SqlCommand("SELECT DB_NAME();", conn);
            string name = cmd.ExecuteScalar()?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("No se pudo obtener el nombre de la base de datos.");
            return name;
        }

        private static string QuoteIdentifier(string name) =>
            "[" + name.Replace("]", "]]") + "]";

        private static void SetSingleUser(SqlConnection master, string databaseName)
        {
            using var cmd = new SqlCommand(
                $"ALTER DATABASE {QuoteIdentifier(databaseName)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;",
                master)
            {
                CommandTimeout = 120
            };
            cmd.ExecuteNonQuery();
        }

        private static void SetMultiUser(SqlConnection master, string databaseName)
        {
            using var cmd = new SqlCommand(
                $"ALTER DATABASE {QuoteIdentifier(databaseName)} SET MULTI_USER;",
                master)
            {
                CommandTimeout = 120
            };
            cmd.ExecuteNonQuery();
        }

        private static void RunRestore(SqlConnection master, string databaseName, string backupPath)
        {
            using var cmd = new SqlCommand($@"
RESTORE DATABASE {QuoteIdentifier(databaseName)}
FROM DISK = @BackupPath
WITH REPLACE, CHECKSUM;", master)
            {
                CommandTimeout = 600
            };
            cmd.Parameters.AddWithValue("@BackupPath", backupPath);
            cmd.ExecuteNonQuery();
        }
    }
}
