using System.Data;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace DL
{
    public sealed class SchemaVersionInfo
    {
        public int Id { get; init; }
        public int Version { get; init; }
        public DateTime AppliedAt { get; init; }
        public string Description { get; init; } = string.Empty;
        public bool EsActual { get; init; }
    }

    /// <summary>
    /// Registra y lee la versión del esquema.
    /// Idempotente: crea dbo.SchemaVersion y el baseline Version=1 una sola vez.
    /// </summary>
    public static class SchemaVersionDAL
    {
        private static int _state;
        private static readonly object Sync = new();

        internal const string EnsureBaselineSql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SchemaVersion')
BEGIN
    CREATE TABLE dbo.SchemaVersion
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Version INT NOT NULL,
        AppliedAt DATETIME2 NOT NULL
            CONSTRAINT DF_SchemaVersion_AppliedAt DEFAULT (SYSDATETIME()),
        Description NVARCHAR(300) NOT NULL,
        EsActual BIT NOT NULL
            CONSTRAINT DF_SchemaVersion_EsActual DEFAULT (0),
        CONSTRAINT UQ_SchemaVersion_Version UNIQUE (Version)
    );

    CREATE UNIQUE INDEX UX_SchemaVersion_EsActual
        ON dbo.SchemaVersion (EsActual)
        WHERE EsActual = 1;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion)
BEGIN
    INSERT INTO dbo.SchemaVersion (Version, Description, EsActual)
    VALUES (1, N'Baseline inicial del esquema existente de MFFITNESS POS', 1);
END";

        /// <summary>
        /// Crea la tabla y el baseline si no existen. Seguro ante reentrancia y reintentos.
        /// </summary>
        public static void EnsureBaseline(DBHelper db)
        {
            if (Volatile.Read(ref _state) == 2)
                return;

            lock (Sync)
            {
                if (_state == 2)
                    return;
                if (_state == 1)
                    return;

                _state = 1;
                try
                {
                    db.ExecuteNonQuery(EnsureBaselineSql);
                    _state = 2;
                }
                catch
                {
                    _state = 0;
                    throw;
                }
            }
        }

        public static SchemaVersionInfo GetCurrent(DBHelper db)
        {
            EnsureBaseline(db);
            using SqlConnection conn = db.GetConnection();
            conn.Open();
            return GetCurrent(conn);
        }

        public static SchemaVersionInfo GetCurrent(SqlConnection conn, SqlTransaction? tx = null)
        {
            using var cmd = new SqlCommand(@"
SELECT Id, Version, AppliedAt, Description, EsActual
FROM dbo.SchemaVersion
WHERE EsActual = 1;", conn, tx);

            using var reader = cmd.ExecuteReader();
            SchemaVersionInfo? current = null;
            int count = 0;
            while (reader.Read())
            {
                count++;
                current = ReadInfo(reader);
            }

            if (count != 1 || current == null)
                throw new InvalidOperationException(
                    $"dbo.SchemaVersion debe tener exactamente una fila con EsActual=1. Encontradas: {count}.");

            if (current.Version < 1)
                throw new InvalidOperationException($"Versión de esquema inválida: {current.Version}.");

            return current;
        }

        public static void RegisterApplied(SqlConnection conn, SqlTransaction tx, int version, string description)
        {
            if (version < 1)
                throw new ArgumentOutOfRangeException(nameof(version));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description requerida.", nameof(description));

            using (var clear = new SqlCommand(
                "UPDATE dbo.SchemaVersion SET EsActual = 0 WHERE EsActual = 1;", conn, tx))
            {
                int updated = clear.ExecuteNonQuery();
                if (updated != 1)
                    throw new InvalidOperationException(
                        $"Se esperaba desactivar 1 versión actual y se actualizaron {updated}.");
            }

            using var insert = new SqlCommand(@"
INSERT INTO dbo.SchemaVersion (Version, Description, EsActual)
VALUES (@Version, @Description, 1);", conn, tx);
            insert.Parameters.AddWithValue("@Version", version);
            insert.Parameters.AddWithValue("@Description", description.Trim());
            insert.ExecuteNonQuery();
        }

        private static SchemaVersionInfo ReadInfo(SqlDataReader reader) =>
            new()
            {
                Id = reader.GetInt32(0),
                Version = reader.GetInt32(1),
                AppliedAt = reader.GetDateTime(2),
                Description = reader.GetString(3),
                EsActual = reader.GetBoolean(4)
            };
    }
}
