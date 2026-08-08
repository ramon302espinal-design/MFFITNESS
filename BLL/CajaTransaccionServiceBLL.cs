using Microsoft.Data.SqlClient;
using System;
using CORE;
using DL;

namespace BLL.Services
{
    public class CajaTransaccionService
    {
        private readonly string connectionString = AppConfig.ConnectionString;

        public void Ejecutar(Action<SqlConnection, SqlTransaction> operacion)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        operacion(conn, tx);
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

        public int RegistrarIngresoConId(SqlConnection conn, SqlTransaction tx,
            decimal monto, string concepto, string usuario)
        {
            CajaDAL cajaDAL = new CajaDAL();

            var caja = cajaDAL.ObtenerCajaAbierta(conn, tx, usuario);

            if (caja == null)
                throw new Exception("No hay caja abierta.");

            int cajaId = Convert.ToInt32(caja["Id"]);

            return cajaDAL.InsertarMovimientoConId(
                conn,
                tx,
                cajaId,
                "INGRESO",
                concepto,
                monto,
                usuario
            );
        }
    }
}