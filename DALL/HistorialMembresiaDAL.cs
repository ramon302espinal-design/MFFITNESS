using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class HistorialMembresiaDAL
    {
        private readonly DBHelper db = new DBHelper();

        /// <summary>Último TipoMovimiento del cliente (por Id), o null si no hay historial.</summary>
        public string? ObtenerUltimoTipoMovimiento(int clienteId)
        {
            string query = @"
                SELECT TOP 1 h.TipoMovimiento
                FROM HistorialMembresias h
                WHERE h.ClienteId = @ClienteId
                ORDER BY h.Id DESC";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId)
            };

            object? result = db.ExecuteScalar(query, parametros);
            if (result == null || result == DBNull.Value)
                return null;

            return result.ToString();
        }

        public void Insertar(int clienteId, string tipo, int? planId, decimal? monto, string usuario, string nota)
        {
            // Agregamos Fecha con GETDATE() para que el cálculo del vencimiento funcione
            string query = @"
            INSERT INTO HistorialMembresias
            (ClienteId, TipoMovimiento, PlanId, Monto, Usuario, Nota, Fecha)
            VALUES
            (@ClienteId, @Tipo, @PlanId, @Monto, @Usuario, @Nota, GETDATE())";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@PlanId", (object?)planId ?? DBNull.Value),
                new SqlParameter("@Monto", (object?)monto ?? DBNull.Value),
                new SqlParameter("@Usuario", usuario ?? "ADMIN"),
                new SqlParameter("@Nota", nota ?? "")
            };

            db.ExecuteNonQuery(query, parametros);
        }

        // Nueva sobrecarga que acepta conexión y transacción para uso dentro de transacciones externas
        public void Registrar(SqlConnection conn, SqlTransaction tx,
            int clienteId, string tipo, int? planId, decimal? monto, string usuario, string nota)
        {
            string query = @"
            INSERT INTO HistorialMembresias
            (ClienteId, TipoMovimiento, PlanId, Monto, Usuario, Nota, Fecha)
            VALUES
            (@ClienteId, @Tipo, @PlanId, @Monto, @Usuario, @Nota, GETDATE())";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@Tipo", tipo);
                cmd.Parameters.AddWithValue("@PlanId", (object?)planId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Monto", (object?)monto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Usuario", usuario ?? "ADMIN");
                cmd.Parameters.AddWithValue("@Nota", nota ?? "");

                cmd.ExecuteNonQuery();
            }
        }

        public DataTable ObtenerHistorial()
        {
            // FechaVence: Membresias.FechaFin real; si no hay match, misma regla que MembresiaHelper.
            string query = @"
            SELECT 
                h.ClienteId,
                c.Nombre AS Nombre,
                ISNULL(c.Telefono, '') AS Telefono,
                ISNULL(c.Direccion, '') AS Direccion,
                ISNULL(p.Nombre, 'N/A') AS PlanNombre, 
                h.TipoMovimiento, 
                h.Monto, 
                h.Fecha AS FechaPago,
                CASE
                    WHEN h.TipoMovimiento IN ('SALIDA', 'BAJA_VENCIDO') THEN NULL
                    WHEN mem.FechaFin IS NOT NULL THEN CAST(mem.FechaFin AS DATE)
                    WHEN h.PlanId IS NOT NULL OR h.TipoMovimiento IN ('PAGO', 'RENOVACION') THEN
                        CASE
                            WHEN DAY(h.Fecha) <= 19 THEN
                                DATEFROMPARTS(
                                    YEAR(DATEADD(MONTH, 1, h.Fecha)),
                                    MONTH(DATEADD(MONTH, 1, h.Fecha)),
                                    15)
                            ELSE EOMONTH(h.Fecha, 1)
                        END
                    ELSE NULL
                END AS FechaVence,
                h.Usuario, 
                h.Nota 
            FROM HistorialMembresias h
            INNER JOIN Clientes c ON c.ID = h.ClienteId
            LEFT JOIN Planes p ON p.Id = h.PlanId
            OUTER APPLY (
                SELECT TOP 1 m.FechaFin
                FROM Membresias m
                WHERE m.ClienteId = h.ClienteId
                  AND (h.PlanId IS NULL OR m.PlanId = h.PlanId)
                  AND (
                        CAST(m.FechaInicio AS DATE) = CAST(h.Fecha AS DATE)
                     OR (
                            m.FechaInicio >= DATEADD(MINUTE, -10, h.Fecha)
                        AND m.FechaInicio <= DATEADD(MINUTE, 10, h.Fecha)
                     )
                  )
                ORDER BY m.Id DESC
            ) mem
            ORDER BY h.Fecha DESC";

            return db.ExecuteQuery(query);
        }
    }
}
