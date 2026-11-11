using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    public class PlanDAL
    {
        private DBHelper db = new DBHelper();

        /// <summary>
        /// Catálogo activo (PREMIUM, PRO, MENSUALIDAD, 3x, 2X, etc.).
        /// Misma fuente para Pagos, Renovación, Estado, Programación y Deudas.
        /// </summary>
        public DataTable ObtenerPlanes()
        {
            string query = @"
SELECT Id, Nombre, Precio, DuracionDias
FROM dbo.Planes
WHERE Activo = 1
ORDER BY Nombre ASC;";
            return db.ExecuteQuery(query);
        }

        public PlanDTO? ObtenerPlan(int planId)
        {
            string query = "SELECT * FROM Planes WHERE Id = @Id";

            SqlParameter[] p =
            {
                new SqlParameter("@Id", planId)
            };

            DataTable dt = db.ExecuteQuery(query, p);

            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];

            return new PlanDTO
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString(),
                Precio = Convert.ToDecimal(row["Precio"]),
                DuracionDias = Convert.ToInt32(row["DuracionDias"])
            };
        }

        public PlanDTO? ObtenerPlan(SqlConnection conn, SqlTransaction tx, int planId)
        {
            string query = "SELECT * FROM Planes WHERE Id = @Id";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Id", planId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new PlanDTO
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Precio = Convert.ToDecimal(reader["Precio"]),
                            DuracionDias = Convert.ToInt32(reader["DuracionDias"])
                        };
                    }
                }
            }

            return null;
        }
    }
}
