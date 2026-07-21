using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DL
{
    public class MembresiaDAL
    {
        // ===============================
        // MÉTODO PARA VER SI UN CLIENTE TIENE MEMBRESÍA ACTIVA
        // ===============================
        public bool TieneMembresiaActiva(int clienteId)
        {
            // Misma regla que dashboard / Estado: vigente inclusive el día de FechaFin.
            string query = @"
                SELECT COUNT(*)
                FROM Membresias m
                WHERE m.ClienteId = @ClienteId
                  AND m.FechaFin IS NOT NULL
                  AND (
                        CAST(m.FechaFin AS DATE) > CAST(GETDATE() AS DATE)
                     OR (
                            CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)
                        AND m.Activa = 1
                     )
                  )
                  AND NOT EXISTS (
                      SELECT 1
                      FROM HistorialMembresias h
                      INNER JOIN (
                          SELECT ClienteId, MAX(Fecha) AS UltimaFecha
                          FROM HistorialMembresias
                          GROUP BY ClienteId
                      ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimaFecha = h.Fecha
                      WHERE h.ClienteId = @ClienteId
                        AND h.TipoMovimiento = 'SALIDA'
                  )
                  AND NOT (
                      CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)
                      AND EXISTS (
                          SELECT 1
                          FROM HistorialMembresias h
                          INNER JOIN (
                              SELECT ClienteId, MAX(Fecha) AS UltimaFecha
                              FROM HistorialMembresias
                              GROUP BY ClienteId
                          ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimaFecha = h.Fecha
                          WHERE h.ClienteId = @ClienteId
                            AND h.TipoMovimiento = 'BAJA_VENCIDO'
                      )
                  )";

            SqlParameter[] p =
            {
                new SqlParameter("@ClienteId", clienteId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, p)) > 0;
        }

        /// <summary>
        /// Hubo membresía con fecha fin pasada (o marcada inactiva vencida), sin plan vigente.
        /// </summary>
        public bool TieneMembresiaVencida(int clienteId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Membresias m
                WHERE m.ClienteId = @ClienteId
                  AND m.FechaFin IS NOT NULL
                  AND CAST(m.FechaFin AS DATE) < CAST(GETDATE() AS DATE)";

            SqlParameter[] p =
            {
                new SqlParameter("@ClienteId", clienteId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, p)) > 0;
        }

        // ===============================
        // OBTENER INFORMACIÓN DE LA MEMBRESÍA ACTIVA
        // ===============================
        public DataRow? ObtenerMembresiaActiva(int clienteId)
        {
            string query = @"SELECT TOP 1 
                                m.Id,
                                m.FechaInicio,
                                m.FechaFin,
                                p.Nombre AS [Plan],
                                p.Precio
                             FROM Membresias m
                             INNER JOIN Planes p ON p.Id = m.PlanId
                             WHERE m.ClienteId = @ClienteId
                             AND m.FechaFin IS NOT NULL
                             AND (
                                   CAST(m.FechaFin AS DATE) > CAST(GETDATE() AS DATE)
                                OR (
                                       CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)
                                   AND m.Activa = 1
                                )
                             )
                             AND NOT EXISTS (
                                 SELECT 1
                                 FROM HistorialMembresias h
                                 INNER JOIN (
                                     SELECT ClienteId, MAX(Fecha) AS UltimaFecha
                                     FROM HistorialMembresias
                                     GROUP BY ClienteId
                                 ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimaFecha = h.Fecha
                                 WHERE h.ClienteId = @ClienteId
                                   AND h.TipoMovimiento = 'SALIDA'
                             )
                             AND NOT (
                                 CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)
                                 AND EXISTS (
                                     SELECT 1
                                     FROM HistorialMembresias h
                                     INNER JOIN (
                                         SELECT ClienteId, MAX(Fecha) AS UltimaFecha
                                         FROM HistorialMembresias
                                         GROUP BY ClienteId
                                     ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimaFecha = h.Fecha
                                     WHERE h.ClienteId = @ClienteId
                                       AND h.TipoMovimiento = 'BAJA_VENCIDO'
                                 )
                             )
                             ORDER BY m.FechaFin DESC, m.Id DESC";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId)
            };

            var dt = db.ExecuteQuery(query, parametros);

            if (dt.Rows.Count > 0)
                return dt.Rows[0];

            return null;
        }

        // ===============================
        // DESACTIVAR MIEMBRO (transaccional: membresía + historial SALIDA)
        // ===============================
        public int DesactivarMiembro(int clienteId, string usuario, string motivo, bool marcarComoVencido)
        {
            var historialDAL = new HistorialMembresiaDAL();

            using SqlConnection conn = db.GetConnection();
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();

            try
            {
                int? membresiaId = null;
                using (var cmdGet = new SqlCommand(@"
                    SELECT TOP 1 Id
                    FROM Membresias
                    WHERE ClienteId = @ClienteId
                    ORDER BY FechaFin DESC, Id DESC", conn, tx))
                {
                    cmdGet.Parameters.AddWithValue("@ClienteId", clienteId);
                    object? result = cmdGet.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        membresiaId = Convert.ToInt32(result);
                }

                if (membresiaId.HasValue)
                {
                    string update = marcarComoVencido
                        ? @"
                        UPDATE Membresias
                        SET Activa = 0,
                            FechaFin = CASE
                                WHEN FechaFin IS NOT NULL
                                     AND CAST(FechaFin AS DATE) < CAST(GETDATE() AS DATE)
                                    THEN FechaFin
                                ELSE CAST(GETDATE() AS DATE)
                            END
                        WHERE Id = @MembresiaId"
                        : @"
                        UPDATE Membresias
                        SET Activa = 0
                        WHERE Id = @MembresiaId";

                    using var cmdUpdate = new SqlCommand(update, conn, tx);
                    cmdUpdate.Parameters.AddWithValue("@MembresiaId", membresiaId.Value);
                    cmdUpdate.ExecuteNonQuery();
                }

                string tipoHistorial = marcarComoVencido ? "BAJA_VENCIDO" : "SALIDA";
                historialDAL.Registrar(
                    conn,
                    tx,
                    clienteId,
                    tipoHistorial,
                    null,
                    null,
                    usuario,
                    motivo
                );

                tx.Commit();
                return 1;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ===============================
        // CLIENTES VENCEN HOY Y VENCIDOS
        // ===============================
        public int ClientesVencenHoy()
        {
            string query = @"
    SELECT COUNT(*)
    FROM Membresias
    WHERE CAST(FechaFin AS DATE) = CAST(GETDATE() AS DATE)
    AND Activa = 1";

            object result = db.ExecuteScalar(query);
            return result != null ? Convert.ToInt32(result) : 0;
        }


        // ===============================
        // CLIENTES VENCIDOS
        // ===============================
        public int ClientesVencidos()
        {
            // Misma definición de VENCIDO que FrmEstadoClientes.
            string query = $@"
    SELECT COUNT(*)
    FROM Clientes c
    {MembresiaEstadoSql.OuterApplyUltimaMembresia}
    WHERE {MembresiaEstadoSql.PredicadoVencido}";

            object result = db.ExecuteScalar(query);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // ===============================
        // ACTUALIZAR MEMBRESÍAS VENCIDAS
        // ===============================
        public void ActualizarVencidas()
        {
            // 1) Desactivar solo después del día de vencimiento.
            string queryVencer = @"
    UPDATE Membresias
    SET Activa = 0
    WHERE FechaFin IS NOT NULL
      AND CAST(FechaFin AS DATE) < CAST(GETDATE() AS DATE)
      AND Activa = 1";

            db.ExecuteNonQuery(queryVencer);

            // 2) Reactivar planes con FechaFin futura que quedaron Activa=0
            //    (historial BAJA residual no manda si la fecha aún no llega).
            string queryReactivar = @"
    UPDATE m
    SET Activa = 1
    FROM Membresias m
    INNER JOIN (
        SELECT ClienteId, MAX(Id) AS IdUltima
        FROM Membresias
        GROUP BY ClienteId
    ) u ON u.IdUltima = m.Id
    WHERE m.Activa = 0
      AND m.FechaFin IS NOT NULL
      AND CAST(m.FechaFin AS DATE) > CAST(GETDATE() AS DATE)
      AND NOT EXISTS (
          SELECT 1
          FROM HistorialMembresias h
          INNER JOIN (
              SELECT ClienteId, MAX(Fecha) AS UltimaFecha
              FROM HistorialMembresias
              GROUP BY ClienteId
          ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimaFecha = h.Fecha
          WHERE h.ClienteId = m.ClienteId
            AND h.TipoMovimiento = 'SALIDA'
      )";

            db.ExecuteNonQuery(queryReactivar);
        }


        // ===============================
        // CERRAR MEMBRESÍA ACTIVA ANTES DE VENDER NUEVA
        // ===============================

        public void CerrarMembresiaActiva(int clienteId)
        {
            string query = @"
    UPDATE Membresias
    SET Activa = 0
    WHERE ClienteId = @ClienteId
    AND Activa = 1";

            SqlParameter[] p =
            {
        new SqlParameter("@ClienteId", clienteId)
    };

            db.ExecuteNonQuery(query, p);
        }

        /// <summary>
        /// Desactiva todas las membresías Activa=1 del cliente dentro de una TX.
        /// Devuelve Id/FechaFin de las que cerró (para poder reactivar en undo).
        /// </summary>
        public List<(int Id, DateTime FechaFin)> CerrarMembresiasActivas(
            SqlConnection conn,
            SqlTransaction tx,
            int clienteId)
        {
            var cerradas = new List<(int Id, DateTime FechaFin)>();

            string select = @"
                SELECT Id, FechaFin
                FROM Membresias
                WHERE ClienteId = @ClienteId
                  AND Activa = 1";

            using (SqlCommand cmdSel = new SqlCommand(select, conn, tx))
            {
                cmdSel.Parameters.AddWithValue("@ClienteId", clienteId);
                using var reader = cmdSel.ExecuteReader();
                while (reader.Read())
                {
                    cerradas.Add((
                        Convert.ToInt32(reader["Id"]),
                        Convert.ToDateTime(reader["FechaFin"])));
                }
            }

            if (cerradas.Count == 0)
                return cerradas;

            string update = @"
                UPDATE Membresias
                SET Activa = 0
                WHERE ClienteId = @ClienteId
                  AND Activa = 1";

            using (SqlCommand cmdUpd = new SqlCommand(update, conn, tx))
            {
                cmdUpd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmdUpd.ExecuteNonQuery();
            }

            return cerradas;
        }


        private DBHelper db = new DBHelper();


        // ===============================
        // REGISTRAR NUEVA MEMBRESÍA
        // ===============================

        public void RegistrarMembresia(int clienteId, int planId, decimal precio, DateTime inicio, DateTime fin, string usuario)
        {
            RegistrarMembresiaConId(clienteId, planId, precio, inicio, fin, usuario, "Inscripción");
        }

        /// <summary>
        /// Libera el índice filtrado UX_Cliente_Activa: pone Activa=0 en cualquier
        /// fila marcada activa (vigente o vencida). Usar solo después de ValidarMembresiaActiva
        /// o en renovación, para no saltarse el bloqueo de plan vigente.
        /// </summary>
        public int LiberarMarcadasActivas(int clienteId)
        {
            if (clienteId <= 0)
                return 0;

            string query = @"
                UPDATE Membresias
                SET Activa = 0
                WHERE ClienteId = @ClienteId
                  AND Activa = 1";

            SqlParameter[] p = { new SqlParameter("@ClienteId", clienteId) };
            return db.ExecuteNonQuery(query, p);
        }

        public int LiberarMarcadasActivas(SqlConnection conn, SqlTransaction tx, int clienteId)
        {
            if (clienteId <= 0)
                return 0;

            using var cmd = new SqlCommand(@"
                UPDATE Membresias
                SET Activa = 0
                WHERE ClienteId = @ClienteId
                  AND Activa = 1", conn, tx);
            cmd.Parameters.AddWithValue("@ClienteId", clienteId);
            return cmd.ExecuteNonQuery();
        }

        public int RegistrarMembresiaConId(
            int clienteId,
            int planId,
            decimal precio,
            DateTime inicio,
            DateTime fin,
            string usuario,
            string tipo = "Inscripción")
        {
            // Evita error UX_Cliente_Activa cuando quedó Activa=1 en una membresía vencida/salida.
            LiberarMarcadasActivas(clienteId);

            string query = @"
            INSERT INTO Membresias 
            (ClienteId, PlanId, Precio, FechaInicio, FechaFin, Usuario, Activa, FechaRegistro, Tipo)
            OUTPUT INSERTED.Id
            VALUES 
            (@ClienteId, @PlanId, @Precio, @Inicio, @Fin, @Usuario, 1, GETDATE(), @Tipo)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@PlanId", planId),
                new SqlParameter("@Precio", precio),
                new SqlParameter("@Inicio", inicio),
                new SqlParameter("@Fin", fin),
                new SqlParameter("@Usuario", usuario ?? "ADMIN"),
                new SqlParameter("@Tipo", tipo ?? "Inscripción")
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros));
        }

        public int CrearMembresiaConId(SqlConnection conn, SqlTransaction tx, MembresiaDTO dto, decimal precio, string usuario)
        {
            LiberarMarcadasActivas(conn, tx, dto.ClienteId);

            string query = @"
                INSERT INTO Membresias
                (ClienteId, PlanId, Precio, FechaInicio, FechaFin, Usuario, Activa, FechaRegistro, Tipo)
                OUTPUT INSERTED.Id
                VALUES
                (@ClienteId, @PlanId, @Precio, @FechaInicio, @FechaFin, @Usuario, 1, GETDATE(), @Tipo)";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ClienteId", dto.ClienteId);
                cmd.Parameters.AddWithValue("@PlanId", dto.PlanId);
                cmd.Parameters.AddWithValue("@Precio", precio);
                cmd.Parameters.AddWithValue("@FechaInicio", dto.FechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", dto.FechaFin);
                cmd.Parameters.AddWithValue("@Usuario", usuario ?? "ADMIN");
                cmd.Parameters.AddWithValue("@Tipo", dto.Estado ?? "Inscripción");

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void DesactivarMembresiaPorId(int membresiaId)
        {
            string query = @"
                UPDATE Membresias
                SET Activa = 0
                WHERE Id = @MembresiaId";

            SqlParameter[] p = { new SqlParameter("@MembresiaId", membresiaId) };
            db.ExecuteNonQuery(query, p);
        }

        public void ReactivarMembresiaPorId(int membresiaId, DateTime fechaFin)
        {
            string query = @"
                UPDATE Membresias
                SET Activa = 1,
                    FechaFin = @FechaFin
                WHERE Id = @MembresiaId";

            SqlParameter[] p =
            {
                new SqlParameter("@MembresiaId", membresiaId),
                new SqlParameter("@FechaFin", fechaFin)
            };

            db.ExecuteNonQuery(query, p);
        }

        // ===============================
        // 🆕 REGISTRAR MEMBRESÍA FINANCIADA Y RETORNAR ID
        // ===============================
        /// <summary>
        /// Registra una membresía y retorna su ID para vincular con deuda
        /// </summary>
        public int RegistrarMembresiaConRetornoId(
            int clienteId, 
            int planId, 
            decimal precio, 
            DateTime inicio, 
            DateTime fin, 
            string usuario)
        {
            LiberarMarcadasActivas(clienteId);

            string query = @"
            INSERT INTO Membresias 
            (ClienteId, PlanId, Precio, FechaInicio, FechaFin, Usuario, Activa, FechaRegistro, Tipo)
            OUTPUT INSERTED.Id
            VALUES 
            (@ClienteId, @PlanId, @Precio, @Inicio, @Fin, @Usuario, 1, GETDATE(), @Tipo)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@PlanId", planId),
                new SqlParameter("@Precio", precio),
                new SqlParameter("@Inicio", inicio),
                new SqlParameter("@Fin", fin),
                new SqlParameter("@Usuario", usuario ?? "ADMIN"),
                new SqlParameter("@Tipo", "Financiado")
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros));
        }
        // Versión fuerte: acepta precio y usuario para insertar todas las columnas existentes en la tabla Membresias
        public void CrearMembresia(SqlConnection conn, SqlTransaction tx, MembresiaDTO dto, decimal precio, string usuario)
        {
            string query = @"
                INSERT INTO Membresias
                (ClienteId, PlanId, Precio, FechaInicio, FechaFin, Usuario, Activa, FechaRegistro, Tipo)
                VALUES
                (@ClienteId, @PlanId, @Precio, @FechaInicio, @FechaFin, @Usuario, 1, GETDATE(), @Tipo)";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ClienteId", dto.ClienteId);
                cmd.Parameters.AddWithValue("@PlanId", dto.PlanId);
                cmd.Parameters.AddWithValue("@Precio", precio);
                cmd.Parameters.AddWithValue("@FechaInicio", dto.FechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", dto.FechaFin);
                cmd.Parameters.AddWithValue("@Usuario", usuario ?? "ADMIN");
                cmd.Parameters.AddWithValue("@Tipo", dto.Estado ?? "Inscripción");

                cmd.ExecuteNonQuery();
            }
        }

        // Versión de compatibilidad: delega a la versión fuerte con valores por defecto
        public void CrearMembresia(SqlConnection conn, SqlTransaction tx, MembresiaDTO dto)
        {
            CrearMembresia(conn, tx, dto, 0m, "ADMIN");
        }

        // ===============================
        // OBTENER TOTAL ACTIVOS (SOLUCIONA EL ERROR DEL DASHBOARD)
        // ===============================
        public int ObtenerTotalActivos()
        {
            // Misma definición de ACTIVO que FrmEstadoClientes (tarjeta "CLIENTES ACTIVOS").
            string query = $@"
    SELECT COUNT(*)
    FROM Clientes c
    {MembresiaEstadoSql.OuterApplyUltimaMembresia}
    WHERE {MembresiaEstadoSql.PredicadoActivo}";

            object result = db.ExecuteScalar(query);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }   
}
