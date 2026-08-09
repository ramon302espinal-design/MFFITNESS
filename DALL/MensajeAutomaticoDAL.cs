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
            SELECT m.Id AS MembresiaId, m.ClienteId, m.FechaFin, ISNULL(p.Nombre, 'Membresia') AS Plan
            FROM Membresias m
            LEFT JOIN Planes p ON p.Id = m.PlanId
            WHERE m.Activa = 1
              AND CAST(m.FechaFin AS DATE) = CAST(DATEADD(DAY, @DiasAntes, GETDATE()) AS DATE)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@DiasAntes", diasAntes)
            };

            return db.ExecuteQuery(query, parametros);
        }

        public DataTable ObtenerMembresiasVencenHoy()
        {
            string query = @"
            SELECT m.Id AS MembresiaId, m.ClienteId, m.FechaFin, ISNULL(p.Nombre, 'Membresia') AS Plan
            FROM Membresias m
            LEFT JOIN Planes p ON p.Id = m.PlanId
            WHERE m.Activa = 1
              AND CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)";

            return db.ExecuteQuery(query);
        }

        public DataTable ObtenerMembresiasVencidasActivas()
        {
            string query = @"
            SELECT m.Id AS MembresiaId, m.ClienteId, m.FechaFin, ISNULL(p.Nombre, 'Membresia') AS Plan
            FROM Membresias m
            LEFT JOIN Planes p ON p.Id = m.PlanId
            WHERE m.Activa = 1
              AND m.FechaFin IS NOT NULL
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
              )";

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
    }
}
