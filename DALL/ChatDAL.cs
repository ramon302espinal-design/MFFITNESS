using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Linq;

namespace DL
{
    public class ChatDAL
    {
        private readonly DBHelper db = new DBHelper();
        private static bool _schemaReady;
        private static readonly object SchemaLock = new();

        public void EnsureSchema()
        {
            if (_schemaReady) return;
            lock (SchemaLock)
            {
                if (_schemaReady) return;

                db.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ChatMensajes')
BEGIN
    CREATE TABLE dbo.ChatMensajes
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClienteId INT NOT NULL
            CONSTRAINT FK_ChatMensajes_Clientes REFERENCES dbo.Clientes(ID),
        Direccion NVARCHAR(10) NOT NULL,
        Cuerpo NVARCHAR(MAX) NOT NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_ChatMensajes_Estado DEFAULT (N'ENVIADO'),
        TwilioMessageSid NVARCHAR(64) NULL,
        Usuario NVARCHAR(100) NULL,
        Fecha DATETIME2 NOT NULL
            CONSTRAINT DF_ChatMensajes_Fecha DEFAULT (SYSDATETIME()),
        Leido BIT NOT NULL
            CONSTRAINT DF_ChatMensajes_Leido DEFAULT (0),
        DetalleError NVARCHAR(500) NULL
    );

    CREATE INDEX IX_ChatMensajes_Cliente_Fecha
        ON dbo.ChatMensajes (ClienteId, Fecha DESC);

    CREATE INDEX IX_ChatMensajes_Entrada_NoLeido
        ON dbo.ChatMensajes (ClienteId, Leido)
        WHERE Direccion = N'ENTRADA';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_ChatMensajes_TwilioSid'
      AND object_id = OBJECT_ID(N'dbo.ChatMensajes'))
BEGIN
    CREATE UNIQUE INDEX UX_ChatMensajes_TwilioSid
        ON dbo.ChatMensajes (TwilioMessageSid)
        WHERE TwilioMessageSid IS NOT NULL;
END

IF COL_LENGTH(N'dbo.ChatMensajes', N'Origen') IS NULL
    ALTER TABLE dbo.ChatMensajes ADD Origen NVARCHAR(20) NULL;

IF COL_LENGTH(N'dbo.ChatMensajes', N'ContentSid') IS NULL
    ALTER TABLE dbo.ChatMensajes ADD ContentSid NVARCHAR(64) NULL;");

                _schemaReady = true;
            }
        }

        public DataTable ListarConversaciones(string? filtroNombre)
        {
            EnsureSchema();

            string filtro = string.IsNullOrWhiteSpace(filtroNombre)
                ? string.Empty
                : @" AND (
                        c.Nombre LIKE @Filtro
                        OR ISNULL(c.Telefono, N'') LIKE @Filtro
                        OR (
                            @Digitos <> N''
                            AND REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(c.Telefono, N''), N' ', N''), N'-', N''), N'+', N''), N'(', N''), N')', N'') LIKE @Digitos
                        )
                    ) ";

            string query = $@"
WITH Timeline AS (
    SELECT ClienteId, Cuerpo, Fecha, Direccion, Leido
    FROM dbo.ChatMensajes
    UNION ALL
    SELECT ClienteId,
           Mensaje AS Cuerpo,
           COALESCE(FechaEnvio, FechaCreacion) AS Fecha,
           N'SALIDA' AS Direccion,
           CAST(1 AS BIT) AS Leido
    FROM dbo.RegistroMensajes
    WHERE Estado IN (N'ENVIADO', N'ERROR', N'PENDIENTE')
),
Ultimo AS (
    SELECT ClienteId, Cuerpo, Fecha,
           ROW_NUMBER() OVER (PARTITION BY ClienteId ORDER BY Fecha DESC) AS Rn
    FROM Timeline
),
NoLeidos AS (
    SELECT ClienteId, COUNT(*) AS Cantidad
    FROM dbo.ChatMensajes
    WHERE Direccion = N'ENTRADA' AND Leido = 0
    GROUP BY ClienteId
)
SELECT
    c.ID AS ClienteId,
    c.Nombre,
    ISNULL(c.Telefono, N'') AS Telefono,
    u.Cuerpo AS UltimoMensaje,
    u.Fecha AS UltimaFecha,
    ISNULL(nl.Cantidad, 0) AS NoLeidos
FROM dbo.Clientes c
LEFT JOIN Ultimo u ON u.ClienteId = c.ID AND u.Rn = 1
LEFT JOIN NoLeidos nl ON nl.ClienteId = c.ID
WHERE c.Telefono IS NOT NULL
  AND LTRIM(RTRIM(c.Telefono)) <> N''
  {filtro}
ORDER BY
    CASE WHEN u.Fecha IS NULL THEN 1 ELSE 0 END,
    u.Fecha DESC,
    c.Nombre ASC;";

            if (string.IsNullOrWhiteSpace(filtroNombre))
                return db.ExecuteQuery(query);

            string digitos = new string(filtroNombre.Where(char.IsDigit).ToArray());
            SqlParameter[] parametros =
            {
                new SqlParameter("@Filtro", "%" + filtroNombre.Trim() + "%"),
                new SqlParameter("@Digitos", string.IsNullOrEmpty(digitos) ? string.Empty : "%" + digitos + "%")
            };
            return db.ExecuteQuery(query, parametros);
        }

        public DataTable ListarMensajes(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT
    CAST(Id AS BIGINT) AS Id,
    ClienteId,
    Direccion,
    Cuerpo,
    Estado,
    Usuario,
    Fecha,
    CAST(0 AS BIT) AS EsAutomatico,
    CAST(NULL AS NVARCHAR(50)) AS TipoAutomatico,
    ISNULL(Origen, N'MANUAL') AS Origen,
    DetalleError
FROM dbo.ChatMensajes
WHERE ClienteId = @ClienteId

UNION ALL

SELECT
    CAST(1000000000 + Id AS BIGINT) AS Id,
    ClienteId,
    N'SALIDA' AS Direccion,
    Mensaje AS Cuerpo,
    Estado,
    CAST(NULL AS NVARCHAR(100)) AS Usuario,
    COALESCE(FechaEnvio, FechaCreacion) AS Fecha,
    CAST(1 AS BIT) AS EsAutomatico,
    Tipo AS TipoAutomatico,
    N'AUTO' AS Origen,
    CASE WHEN Estado = N'ERROR' THEN LEFT(ISNULL(Respuesta, N''), 500) ELSE NULL END AS DetalleError
FROM dbo.RegistroMensajes
WHERE ClienteId = @ClienteId
  AND Estado IN (N'ENVIADO', N'ERROR', N'PENDIENTE')

ORDER BY Fecha ASC;";

            SqlParameter[] parametros = { new SqlParameter("@ClienteId", clienteId) };
            return db.ExecuteQuery(query, parametros);
        }

        public int ContarMensajesCliente(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT
    (SELECT COUNT(1) FROM dbo.ChatMensajes WHERE ClienteId = @ClienteId)
  + (SELECT COUNT(1) FROM dbo.RegistroMensajes
     WHERE ClienteId = @ClienteId
       AND Estado IN (N'ENVIADO', N'ERROR', N'PENDIENTE'));";

            SqlParameter[] parametros = { new SqlParameter("@ClienteId", clienteId) };
            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result);
        }

        public int ObtenerMaxIdChatCliente(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT ISNULL(MAX(Id), 0)
FROM dbo.ChatMensajes
WHERE ClienteId = @ClienteId;";

            SqlParameter[] parametros = { new SqlParameter("@ClienteId", clienteId) };
            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result);
        }

        public int InsertarMensaje(
            int clienteId,
            string direccion,
            string cuerpo,
            string estado,
            string? usuario,
            string? twilioSid = null,
            string? detalleError = null,
            string? origen = null,
            string? contentSid = null)
        {
            EnsureSchema();

            string query = @"
INSERT INTO dbo.ChatMensajes
    (ClienteId, Direccion, Cuerpo, Estado, TwilioMessageSid, Usuario, DetalleError, Origen, ContentSid)
OUTPUT INSERTED.Id
VALUES
    (@ClienteId, @Direccion, @Cuerpo, @Estado, @TwilioSid, @Usuario, @DetalleError, @Origen, @ContentSid);";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@Direccion", direccion),
                new SqlParameter("@Cuerpo", cuerpo),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@TwilioSid", (object?)twilioSid ?? DBNull.Value),
                new SqlParameter("@Usuario", (object?)usuario ?? DBNull.Value),
                new SqlParameter("@DetalleError", (object?)detalleError ?? DBNull.Value),
                new SqlParameter("@Origen", (object?)origen ?? DBNull.Value),
                new SqlParameter("@ContentSid", (object?)contentSid ?? DBNull.Value)
            };

            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result);
        }

        public void ActualizarEstadoMensaje(
            int mensajeId,
            string estado,
            string? twilioSid,
            string? detalleError,
            string? contentSid = null)
        {
            EnsureSchema();

            string query = @"
UPDATE dbo.ChatMensajes
SET Estado = @Estado,
    TwilioMessageSid = COALESCE(@TwilioSid, TwilioMessageSid),
    ContentSid = COALESCE(@ContentSid, ContentSid),
    DetalleError = @DetalleError
WHERE Id = @Id;";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", mensajeId),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@TwilioSid", (object?)twilioSid ?? DBNull.Value),
                new SqlParameter("@ContentSid", (object?)contentSid ?? DBNull.Value),
                new SqlParameter("@DetalleError", (object?)detalleError ?? DBNull.Value)
            };

            db.ExecuteNonQuery(query, parametros);
        }

        public int ObtenerHuellaEstados(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT ISNULL(CHECKSUM_AGG(BINARY_CHECKSUM(Id, Estado, ISNULL(DetalleError, N''))), 0)
FROM dbo.ChatMensajes
WHERE ClienteId = @ClienteId
  AND Direccion = N'SALIDA';";

            object? result = db.ExecuteScalar(query, new[] { new SqlParameter("@ClienteId", clienteId) });
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public void MarcarEntradaLeida(int clienteId)
        {
            EnsureSchema();

            string query = @"
UPDATE dbo.ChatMensajes
SET Leido = 1
WHERE ClienteId = @ClienteId
  AND Direccion = N'ENTRADA'
  AND Leido = 0;";

            SqlParameter[] parametros = { new SqlParameter("@ClienteId", clienteId) };
            db.ExecuteNonQuery(query, parametros);
        }

        public int ContarEntradaNoLeida(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT COUNT(1)
FROM dbo.ChatMensajes
WHERE ClienteId = @ClienteId
  AND Direccion = N'ENTRADA'
  AND Leido = 0;";

            SqlParameter[] parametros = { new SqlParameter("@ClienteId", clienteId) };
            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result);
        }

        public int ContarTotalEntradaNoLeida()
        {
            EnsureSchema();

            string query = @"
SELECT COUNT(1)
FROM dbo.ChatMensajes
WHERE Direccion = N'ENTRADA'
  AND Leido = 0;";

            object? result = db.ExecuteScalar(query);
            return Convert.ToInt32(result);
        }

        public int ObtenerMaxIdEntrada()
        {
            EnsureSchema();

            string query = @"
SELECT ISNULL(MAX(Id), 0)
FROM dbo.ChatMensajes
WHERE Direccion = N'ENTRADA';";

            object? result = db.ExecuteScalar(query);
            return Convert.ToInt32(result);
        }

        public DataTable ListarEntradasDesdeId(int ultimoId)
        {
            EnsureSchema();

            string query = @"
SELECT
    cm.Id AS MensajeId,
    cm.ClienteId,
    c.Nombre AS ClienteNombre,
    cm.Cuerpo,
    cm.Fecha
FROM dbo.ChatMensajes cm
INNER JOIN dbo.Clientes c ON c.ID = cm.ClienteId
WHERE cm.Direccion = N'ENTRADA'
  AND cm.Id > @UltimoId
ORDER BY cm.Id ASC;";

            SqlParameter[] parametros = { new SqlParameter("@UltimoId", ultimoId) };
            return db.ExecuteQuery(query, parametros);
        }

        public bool ExistePorTwilioSid(string messageSid)
        {
            if (string.IsNullOrWhiteSpace(messageSid))
                return false;

            EnsureSchema();

            string query = @"
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.ChatMensajes WHERE TwilioMessageSid = @Sid
) THEN 1 ELSE 0 END;";

            SqlParameter[] parametros = { new SqlParameter("@Sid", messageSid.Trim()) };
            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result) == 1;
        }

        public DateTime? ObtenerUltimaEntrada(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT MAX(Fecha)
FROM dbo.ChatMensajes
WHERE ClienteId = @ClienteId
  AND Direccion = N'ENTRADA';";

            object? result = db.ExecuteScalar(query, new[] { new SqlParameter("@ClienteId", clienteId) });
            if (result == null || result == DBNull.Value)
                return null;

            return Convert.ToDateTime(result);
        }

        public string? ObtenerEstadoPorTwilioSid(string messageSid)
        {
            if (string.IsNullOrWhiteSpace(messageSid))
                return null;

            EnsureSchema();

            string query = @"
SELECT TOP 1 Estado
FROM dbo.ChatMensajes
WHERE TwilioMessageSid = @Sid;";

            object? result = db.ExecuteScalar(query, new[] { new SqlParameter("@Sid", messageSid.Trim()) });
            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        public void ActualizarEstadoPorTwilioSid(string messageSid, string estado, string? detalleError)
        {
            EnsureSchema();

            string query = @"
UPDATE dbo.ChatMensajes
SET Estado = @Estado,
    DetalleError = @DetalleError
WHERE TwilioMessageSid = @Sid;";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Sid", messageSid.Trim()),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@DetalleError", (object?)detalleError ?? DBNull.Value)
            };
            db.ExecuteNonQuery(query, parametros);
        }

        public DataTable ObtenerResumenMiembro(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT
    c.ID AS ClienteId,
    c.Nombre,
    ISNULL(c.Telefono, N'') AS Telefono,
    (
        SELECT TOP 1 ISNULL(p.Nombre, N'')
        FROM dbo.Membresias m
        LEFT JOIN dbo.Planes p ON p.Id = m.PlanId
        WHERE m.ClienteId = c.ID
        ORDER BY m.Id DESC
    ) AS [Plan],
    (
        SELECT TOP 1 m.FechaFin
        FROM dbo.Membresias m
        WHERE m.ClienteId = c.ID
        ORDER BY m.Id DESC
    ) AS FechaFin,
    (
        SELECT ISNULL(SUM(d.Saldo), 0)
        FROM dbo.Deudas d
        WHERE d.ClienteId = c.ID
          AND d.Estado = N'ACTIVA'
          AND d.Saldo > 0
    ) AS SaldoDeuda
FROM dbo.Clientes c
WHERE c.ID = @ClienteId;";

            return db.ExecuteQuery(query, new[] { new SqlParameter("@ClienteId", clienteId) });
        }
    }
}
