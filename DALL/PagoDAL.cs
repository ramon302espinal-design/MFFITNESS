using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class PagoDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ObtenerPagos()
        {
            string query = @"
        SELECT 
            p.Id, 
            c.Nombre AS Cliente, 
            p.FechaPago, 
            p.FechaVencimiento, 
            p.Monto, 
            p.MetodoPago, 
            p.Concepto, 
            p.Usuario 
        FROM Pagos p 
        INNER JOIN Clientes c ON c.Id = p.ClienteId 
        ORDER BY p.FechaPago DESC";

            return db.ExecuteQuery(query);
        }

        public int RegistrarPagoConId(
            int clienteId,
            DateTime fechaPago,
            DateTime fechaVencimiento,
            decimal monto,
            string metodoPago,
            string concepto,
            string usuario)
        {
            string query = @"INSERT INTO Pagos
                     (ClienteId, FechaPago, FechaVencimiento, Monto, MetodoPago, Concepto, Usuario)
                     OUTPUT INSERTED.Id
                     VALUES
                     (@ClienteId, @FechaPago, @FechaVencimiento, @Monto, @MetodoPago, @Concepto, @Usuario)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@FechaPago", fechaPago),
                new SqlParameter("@FechaVencimiento", fechaVencimiento),
                new SqlParameter("@Monto", monto),
                new SqlParameter("@MetodoPago", metodoPago),
                new SqlParameter("@Concepto", concepto),
                new SqlParameter("@Usuario", usuario)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros));
        }

        public int RegistrarPagoConId(SqlConnection conn, SqlTransaction tx,
             int clienteId, DateTime fechaPago, DateTime fechaVencimiento,
             decimal monto, string metodoPago, string concepto, string usuario)
        {
            string query = @"INSERT INTO Pagos
                     (ClienteId, FechaPago, FechaVencimiento, Monto, MetodoPago, Concepto, Usuario)
                     OUTPUT INSERTED.Id
                     VALUES
                     (@ClienteId, @FechaPago, @FechaVencimiento, @Monto, @MetodoPago, @Concepto, @Usuario)";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@FechaPago", fechaPago);
                cmd.Parameters.AddWithValue("@FechaVencimiento", fechaVencimiento);
                cmd.Parameters.AddWithValue("@Monto", monto);
                cmd.Parameters.AddWithValue("@MetodoPago", metodoPago);
                cmd.Parameters.AddWithValue("@Concepto", concepto);
                cmd.Parameters.AddWithValue("@Usuario", usuario);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void RevertirPagoMembresia(int pagoId, int cajaMovimientoId, string usuario)
        {
            using SqlConnection conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();

            try
            {
                SqlCommand cmdGet = new SqlCommand(@"
                    SELECT ClienteId, Monto, Concepto
                    FROM Pagos
                    WHERE Id = @PagoId", conn, tx);

                cmdGet.Parameters.AddWithValue("@PagoId", pagoId);

                using (var reader = cmdGet.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("Pago de membresía no encontrado.");
                }

                SqlCommand cmdDelete = new SqlCommand("DELETE FROM Pagos WHERE Id = @PagoId", conn, tx);
                cmdDelete.Parameters.AddWithValue("@PagoId", pagoId);
                cmdDelete.ExecuteNonQuery();

                if (cajaMovimientoId > 0)
                {
                    CajaDAL cajaDAL = new CajaDAL();
                    cajaDAL.RevertirMovimientoEnTransaccion(conn, tx, cajaMovimientoId, usuario);
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
