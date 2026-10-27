using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DL
{
    public class MensajeAutomaticoDAL
    {
        private readonly DBHelper db = new DBHelper();

        public string ObtenerPlantilla(string tipo)
        {
            string query = @"
            SELECT [Plantilla]
            FROM [dbo].[MensajesAutomaticos]
            WHERE [Tipo] = @Tipo AND [Activa] = 1";

            SqlParameter[] parametros = { new SqlParameter("@Tipo", tipo) };

            object? resultado = db.ExecuteScalar(query, parametros);
            return resultado?.ToString() ?? "";
        }

        public string? ObtenerContentSid(string tipo)
        {
            if (!ColumnaExiste("MensajesAutomaticos", "ContentSid"))
                return null;

            string query = @"
            SELECT [ContentSid]
            FROM [dbo].[MensajesAutomaticos]
            WHERE [Tipo] = @Tipo AND [Activa] = 1";

            SqlParameter[] parametros = { new SqlParameter("@Tipo", tipo) };
            object? resultado = db.ExecuteScalar(query, parametros);
            string? sid = resultado?.ToString();
            return string.IsNullOrWhiteSpace(sid) ? null : sid.Trim();
        }

        public string? ObtenerUltimaRespuestaError(int clienteId)
        {
            string query = @"
            SELECT TOP 1 Respuesta
            FROM [dbo].[RegistroMensajes]
            WHERE ClienteId = @ClienteId AND Estado = 'ERROR'
            ORDER BY Id DESC";

            SqlParameter[] parametros = { new SqlParameter("@ClienteId", clienteId) };
            object? resultado = db.ExecuteScalar(query, parametros);
            return resultado?.ToString();
        }

        private bool ColumnaExiste(string tabla, string columna)
        {
            string query = @"
            SELECT COUNT(1)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @Tabla AND COLUMN_NAME = @Columna";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Tabla", tabla),
                new SqlParameter("@Columna", columna)
            };

            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result) > 0;
        }

        public int RegistrarMensaje(
            int clienteId,
            string tipo,
            string numeroTelefono,
            string mensaje,
            string estado = "PENDIENTE",
            int? referenciaId = null)
        {
            string query = @"
            INSERT INTO [dbo].[RegistroMensajes] 
            ([ClienteId], [Tipo], [NumeroTelefono], [Mensaje], [Estado], [ReferenciaId], [FechaCreacion])
            OUTPUT INSERTED.Id
            VALUES 
            (@ClienteId, @Tipo, @NumeroTelefono, @Mensaje, @Estado, @ReferenciaId, GETDATE())";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@NumeroTelefono", numeroTelefono),
                new SqlParameter("@Mensaje", mensaje),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@ReferenciaId", (object?)referenciaId ?? DBNull.Value)
            };

            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result);
        }

        public void ActualizarEstadoMensaje(int id, string estado, string? respuesta = null)
        {
            string query = @"
            UPDATE [dbo].[RegistroMensajes]
            SET [Estado] = @Estado, 
                [Respuesta] = @Respuesta,
                [FechaEnvio] = GETDATE()
            WHERE [Id] = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@Respuesta", (object?)respuesta ?? DBNull.Value)
            };

            db.ExecuteNonQuery(query, parametros);
        }

        public DataTable ObtenerUltimosMensajes(int clienteId, int top = 5)
        {
            string query = @"
            SELECT TOP (@Top) Id, Tipo, Estado, NumeroTelefono, Respuesta, FechaEnvio, FechaCreacion
            FROM [dbo].[RegistroMensajes]
            WHERE ClienteId = @ClienteId
            ORDER BY Id DESC";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Top", top),
                new SqlParameter("@ClienteId", clienteId)
            };

            return db.ExecuteQuery(query, parametros);
        }

        public DataTable ObtenerMensajesPendientes()
        {
            string query = @"
            SELECT [Id], [ClienteId], [NumeroTelefono], [Mensaje], [Tipo]
            FROM [dbo].[RegistroMensajes]
            WHERE [Estado] = 'PENDIENTE'
            ORDER BY [FechaCreacion] ASC";

            return db.ExecuteQuery(query);
        }

        public bool NotificacionYaEnviada(int clienteId, string tipo, int? referenciaId = null)
        {
            string query = @"
            SELECT COUNT(1)
            FROM [dbo].[RegistroMensajes]
            WHERE [ClienteId] = @ClienteId
              AND [Tipo] = @Tipo
              AND [Estado] = 'ENVIADO'
              AND CAST([FechaEnvio] AS DATE) = CAST(GETDATE() AS DATE)
              AND (@ReferenciaId IS NULL OR [ReferenciaId] = @ReferenciaId)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@ReferenciaId", (object?)referenciaId ?? DBNull.Value)
            };

            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result) > 0;
        }

        public DataTable ObtenerMembresiasPorVencer(int diasAntes)
        {
            string query = @"
            SELECT m.Id AS MembresiaId, m.ClienteId, m.FechaFin, ISNULL(p.Nombre, 'Membresia') AS [Plan]
            FROM Membresias m
            LEFT JOIN Planes p ON p.Id = m.PlanId
            WHERE m.Activa = 1
              AND CAST(m.FechaFin AS DATE) = CAST(DATEADD(DAY, @DiasAntes, GETDATE()) AS DATE)"
            + MembresiaEstadoSql.FiltroMembresiaSinProgramacionPendiente;

            SqlParameter[] parametros =
            {
                new SqlParameter("@DiasAntes", diasAntes)
            };

            return db.ExecuteQuery(query, parametros);
        }

        public DataTable ObtenerMembresiasVencenHoy()
        {
            string query = @"
            SELECT m.Id AS MembresiaId, m.ClienteId, m.FechaFin, ISNULL(p.Nombre, 'Membresia') AS [Plan]
            FROM Membresias m
            LEFT JOIN Planes p ON p.Id = m.PlanId
            WHERE m.Activa = 1
              AND CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)"
            + MembresiaEstadoSql.FiltroMembresiaSinProgramacionPendiente;

            return db.ExecuteQuery(query);
        }

        public DataTable ObtenerMembresiasVencidasActivas()
        {
            // Última membresía del cliente con FechaFin ya pasada.
            // No exigir Activa=1: ActualizarVencidas() la apaga el mismo ciclo
            // y el aviso cada 3 días dejaría de salir.
            // Excluye SALIDA manual, congelación activa y programación pendiente.
            string query = @"
            SELECT m.Id AS MembresiaId, m.ClienteId, m.FechaFin, ISNULL(p.Nombre, 'Membresia') AS [Plan]
            FROM Membresias m
            INNER JOIN (
                SELECT ClienteId, MAX(Id) AS IdUltima
                FROM Membresias
                GROUP BY ClienteId
            ) u ON u.IdUltima = m.Id
            LEFT JOIN Planes p ON p.Id = m.PlanId
            WHERE m.FechaFin IS NOT NULL
              AND CAST(m.FechaFin AS DATE) < CAST(GETDATE() AS DATE)
              AND NOT EXISTS (
                  SELECT 1
                  FROM HistorialMembresias h
                  INNER JOIN (
                      SELECT ClienteId, MAX(Id) AS UltimoId
                      FROM HistorialMembresias
                      GROUP BY ClienteId
                  ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimoId = h.Id
                    WHERE h.ClienteId = m.ClienteId
                    AND h.TipoMovimiento = 'SALIDA'
              )
              AND NOT EXISTS (
                  SELECT 1
                  FROM CongelacionesMembresia g
                  WHERE g.ClienteId = m.ClienteId
                    AND g.Activa = 1
              )"
            + MembresiaEstadoSql.FiltroMembresiaSinProgramacionPendiente;

            return db.ExecuteQuery(query);
        }

        public DataTable ObtenerMensajesParaReintento(int max, int maxIntentosPorMensaje)
        {
            bool tieneIntentos = ColumnaExiste("RegistroMensajes", "Intentos");

            string filtroIntentos = tieneIntentos
                ? " AND ISNULL([Intentos], 0) < @MaxIntentos"
                : string.Empty;

            string query = $@"
            SELECT TOP (@Max) Id, ClienteId, NumeroTelefono, Mensaje, Tipo, ReferenciaId, Respuesta
            FROM [dbo].[RegistroMensajes]
            WHERE Estado IN ('PENDIENTE', 'ERROR')
              AND FechaCreacion >= DATEADD(HOUR, -24, GETDATE())
              {filtroIntentos}
            ORDER BY FechaCreacion ASC";

            var parametros = new List<SqlParameter>
            {
                new SqlParameter("@Max", max)
            };

            if (tieneIntentos)
                parametros.Add(new SqlParameter("@MaxIntentos", maxIntentosPorMensaje));

            return db.ExecuteQuery(query, parametros.ToArray());
        }

        public void IncrementarIntentos(int id)
        {
            if (!ColumnaExiste("RegistroMensajes", "Intentos"))
                return;

            string query = @"
            UPDATE [dbo].[RegistroMensajes]
            SET [Intentos] = ISNULL([Intentos], 0) + 1
            WHERE [Id] = @Id";

            SqlParameter[] parametros = { new SqlParameter("@Id", id) };
            db.ExecuteNonQuery(query, parametros);
        }

        /// <summary>
        /// Fecha del último envío exitoso de este aviso. La regla de cada 3 días
        /// se decide en código con el reloj de República Dominicana.
        /// </summary>
        public DateTime? ObtenerUltimaFechaEnvio(int clienteId, string tipo, int? referenciaId = null)
        {
            string query = @"
            SELECT MAX([FechaEnvio])
            FROM [dbo].[RegistroMensajes]
            WHERE [ClienteId] = @ClienteId
              AND [Tipo] = @Tipo
              AND [Estado] = 'ENVIADO'
              AND (@ReferenciaId IS NULL OR [ReferenciaId] = @ReferenciaId)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@ReferenciaId", (object?)referenciaId ?? DBNull.Value)
            };

            object? result = db.ExecuteScalar(query, parametros);
            if (result == null || result == DBNull.Value)
                return null;

            return Convert.ToDateTime(result);
        }

        public bool NotificacionEnviadaEnDias(int clienteId, string tipo, int dias, int? referenciaId = null)
        {
            string query = @"
            SELECT COUNT(1)
            FROM [dbo].[RegistroMensajes]
            WHERE [ClienteId] = @ClienteId
              AND [Tipo] = @Tipo
              AND [Estado] = 'ENVIADO'
              AND [FechaEnvio] >= DATEADD(DAY, -@Dias, GETDATE())
              AND (@ReferenciaId IS NULL OR [ReferenciaId] = @ReferenciaId)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@Dias", dias),
                new SqlParameter("@ReferenciaId", (object?)referenciaId ?? DBNull.Value)
            };

            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result) > 0;
        }

        private static bool _configLista;
        private static readonly object ConfigLock = new();

        public void EnsureConfigSchema()
        {
            if (_configLista)
                return;

            lock (ConfigLock)
            {
                if (_configLista)
                    return;

                db.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'WhatsAppAutomatizacionConfig')
BEGIN
    CREATE TABLE dbo.WhatsAppAutomatizacionConfig
    (
        Tipo NVARCHAR(50) NOT NULL PRIMARY KEY,
        Activa BIT NOT NULL CONSTRAINT DF_WAAuto_Activa DEFAULT (1),
        Evento NVARCHAR(160) NOT NULL,
        Condicion NVARCHAR(300) NULL,
        DiasAntes INT NULL,
        EvitarDuplicadoHoras INT NOT NULL CONSTRAINT DF_WAAuto_Dedup DEFAULT (24),
        UltimoResultado NVARCHAR(500) NULL,
        UltimaEjecucion DATETIME2 NULL,
        FechaModificacion DATETIME2 NOT NULL CONSTRAINT DF_WAAuto_Mod DEFAULT (SYSDATETIME())
    );
END
IF COL_LENGTH('dbo.WhatsAppAutomatizacionConfig', 'DiasUrgente') IS NULL
    ALTER TABLE dbo.WhatsAppAutomatizacionConfig ADD DiasUrgente INT NULL;");

                db.ExecuteNonQuery(@"
INSERT INTO dbo.WhatsAppAutomatizacionConfig (Tipo, Activa, Evento, Condicion, DiasAntes, EvitarDuplicadoHoras)
SELECT v.Tipo, 1, v.Evento, v.Condicion, NULL,
       CASE WHEN v.Tipo = N'MEMBRESIA_VENCIDA' THEN 72 ELSE 24 END
FROM (VALUES
    (N'VENCIMIENTO_PROXIMO', N'FechaFin de la membresía activa', N'N días antes. Un aviso por día y membresía.'),
    (N'MEMBRESIA_VENCIDA', N'Membresía vencida y aún activa', N'Cada 3 días, a partir de las 9:00. No cambia el acceso.'),
    (N'RECORDATORIO_VENCIMIENTO_DEUDA', N'Deuda activa con saldo', N'N días antes del pago.'),
    (N'DEUDA_CREADA', N'Se registra una deuda o préstamo', N'Al crear la venta a crédito.'),
    (N'PAGO_MEMBRESIA', N'Pago de membresía registrado', N'Cada pago nuevo.'),
    (N'PROGRAMACION_ACTIVADA', N'Se activa una membresía programada', N'Al activar la programación ya pagada.')
) v(Tipo, Evento, Condicion)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.WhatsAppAutomatizacionConfig c WHERE c.Tipo = v.Tipo);

UPDATE dbo.WhatsAppAutomatizacionConfig
SET Condicion = N'Cada 3 días, a partir de las 9:00. No cambia el acceso.',
    EvitarDuplicadoHoras = 72,
    FechaModificacion = SYSDATETIME()
WHERE Tipo = N'MEMBRESIA_VENCIDA'
  AND (
      EvitarDuplicadoHoras <= 24
      OR Condicion LIKE N'Aviso diario%'
  );");

                _configLista = true;
            }
        }

        public bool AutomatizacionEstaActiva(string tipo)
        {
            EnsureConfigSchema();

            string query = @"
SELECT Activa
FROM dbo.WhatsAppAutomatizacionConfig
WHERE Tipo = @Tipo;";

            object? result = db.ExecuteScalar(query, new[] { new SqlParameter("@Tipo", tipo) });
            if (result == null || result == DBNull.Value)
                return true;

            return Convert.ToBoolean(result);
        }

        public int? ObtenerDiasAntes(string tipo)
        {
            EnsureConfigSchema();

            object? result = db.ExecuteScalar(
                "SELECT DiasAntes FROM dbo.WhatsAppAutomatizacionConfig WHERE Tipo = @Tipo;",
                new[] { new SqlParameter("@Tipo", tipo) });

            if (result == null || result == DBNull.Value)
                return null;

            int dias = Convert.ToInt32(result);
            return dias > 0 ? dias : null;
        }

        public int? ObtenerDiasUrgente(string tipo)
        {
            EnsureConfigSchema();

            object? result = db.ExecuteScalar(
                "SELECT DiasUrgente FROM dbo.WhatsAppAutomatizacionConfig WHERE Tipo = @Tipo;",
                new[] { new SqlParameter("@Tipo", tipo) });

            if (result == null || result == DBNull.Value)
                return null;

            int dias = Convert.ToInt32(result);
            return dias > 0 ? dias : null;
        }

        public int ObtenerHorasDedup(string tipo)
        {
            EnsureConfigSchema();

            object? result = db.ExecuteScalar(
                "SELECT EvitarDuplicadoHoras FROM dbo.WhatsAppAutomatizacionConfig WHERE Tipo = @Tipo;",
                new[] { new SqlParameter("@Tipo", tipo) });

            if (result == null || result == DBNull.Value)
                return 24;

            int horas = Convert.ToInt32(result);
            return horas > 0 ? horas : 24;
        }

        /// <summary>Persiste la configuración. No dispara envíos.</summary>
        public void GuardarAutomatizacion(string tipo, bool activa, int? diasAntes, int? diasUrgente, int evitarDuplicadoHoras)
        {
            EnsureConfigSchema();

            string query = @"
UPDATE dbo.WhatsAppAutomatizacionConfig
SET Activa = @Activa,
    DiasAntes = @DiasAntes,
    DiasUrgente = @DiasUrgente,
    EvitarDuplicadoHoras = @Horas,
    FechaModificacion = SYSDATETIME()
WHERE Tipo = @Tipo;";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@Activa", activa),
                new SqlParameter("@DiasAntes", (object?)diasAntes ?? DBNull.Value),
                new SqlParameter("@DiasUrgente", (object?)diasUrgente ?? DBNull.Value),
                new SqlParameter("@Horas", evitarDuplicadoHoras < 1 ? 24 : evitarDuplicadoHoras)
            };
            db.ExecuteNonQuery(query, parametros);
        }

        public void RegistrarUltimoResultado(string tipo, string? resultado)
        {
            try
            {
                EnsureConfigSchema();
                string texto = resultado ?? string.Empty;
                if (texto.Length > 500)
                    texto = texto[..500];

                db.ExecuteNonQuery(@"
UPDATE dbo.WhatsAppAutomatizacionConfig
SET UltimoResultado = @Resultado,
    UltimaEjecucion = SYSDATETIME()
WHERE Tipo = @Tipo;",
                    new[]
                    {
                        new SqlParameter("@Tipo", tipo),
                        new SqlParameter("@Resultado", texto)
                    });
            }
            catch
            {
                // El registro del resultado no debe tumbar el envío.
            }
        }

        public DataTable ListarConfiguracion()
        {
            EnsureConfigSchema();
            return db.ExecuteQuery(@"
SELECT Tipo, Activa, Evento, Condicion, DiasAntes, DiasUrgente, EvitarDuplicadoHoras, UltimoResultado, UltimaEjecucion
FROM dbo.WhatsAppAutomatizacionConfig
ORDER BY Tipo;");
        }

        public DataTable ListarPlantillas()
        {
            return db.ExecuteQuery(@"
SELECT Tipo, Plantilla, Activa
FROM dbo.MensajesAutomaticos
ORDER BY Tipo;");
        }

        /// <summary>Actualiza el texto futuro. No inserta en RegistroMensajes.</summary>
        public void GuardarPlantilla(string tipo, string plantilla)
        {
            bool tieneModificacion = ColumnaExiste("MensajesAutomaticos", "FechaModificacion");
            string setFecha = tieneModificacion ? ", FechaModificacion = GETDATE()" : string.Empty;

            string update = $@"
UPDATE dbo.MensajesAutomaticos
SET Plantilla = @Plantilla
    {setFecha}
WHERE Tipo = @Tipo;";

            int filas = db.ExecuteNonQuery(update, new[]
            {
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@Plantilla", plantilla ?? string.Empty)
            });

            if (filas > 0)
                return;

            string insert = tieneModificacion
                ? @"INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion, FechaModificacion)
                    VALUES (@Tipo, @Plantilla, 1, GETDATE(), GETDATE());"
                : @"INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
                    VALUES (@Tipo, @Plantilla, 1, GETDATE());";

            db.ExecuteNonQuery(insert, new[]
            {
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@Plantilla", plantilla ?? string.Empty)
            });
        }
    }
}
