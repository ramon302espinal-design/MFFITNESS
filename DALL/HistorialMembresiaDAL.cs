using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class HistorialMembresiaDAL
    {
        private readonly DBHelper db = new DBHelper();

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
            // Cambiar INNER JOIN a LEFT JOIN para incluir salidas sin plan
            string query = @"
            SELECT 
                h.ClienteId,
                c.Nombre AS Nombre, 
                ISNULL(p.Nombre, 'N/A') AS PlanNombre, 
                h.TipoMovimiento, 
                h.Monto, 
                h.Fecha AS FechaPago,
                CASE 
                    WHEN h.PlanId IS NOT NULL THEN EOMONTH(h.Fecha, 1)
                    ELSE NULL
                END AS FechaVence, 
                h.Usuario, 
                h.Nota 
            FROM HistorialMembresias h
            INNER JOIN Clientes c ON c.ID = h.ClienteId
            LEFT JOIN Planes p ON p.Id = h.PlanId
            ORDER BY h.Fecha DESC";

            return db.ExecuteQuery(query);
        }
    }
}
