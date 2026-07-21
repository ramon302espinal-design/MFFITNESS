using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class ReporteDAL
    {
        private readonly DBHelper db = new DBHelper();

        // ===============================
        // INSERTAR REPORTE GENERADO
        // ===============================
        public void InsertarReporte(DateTime fecha, string tipo, string ruta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"INSERT INTO ReportesGenerados (Fecha, Tipo, Ruta)
                                 VALUES (@Fecha, @Tipo, @Ruta)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@Tipo", tipo);
                cmd.Parameters.AddWithValue("@Ruta", ruta);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ===============================
        // VALIDAR SI YA EXISTE
        // ===============================
        public bool ExisteReporte(DateTime fecha, string tipo)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM ReportesGenerados 
                                 WHERE Fecha = @Fecha AND Tipo = @Tipo";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@Tipo", tipo);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }

        // ===============================
        // HISTORIAL
        // ===============================
        public DataTable ObtenerHistorial()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = "SELECT * FROM ReportesGenerados ORDER BY Fecha DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ===============================
        // 🔥 REPORTE CAJA POR FECHA
        // ===============================
        public DataTable ObtenerCajaPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT Fecha, TipoMovimiento, Concepto, Monto 
                         FROM DetalleCaja
                         WHERE Fecha >= @Desde AND Fecha < DATEADD(DAY, 1, @Hasta)
                         ORDER BY Fecha DESC";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ===============================
        // 🔥 REPORTE PAGOS
        // ===============================
        public DataTable ObtenerPagosPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT FechaPago, ClienteId, Monto, MetodoPago, Concepto 
                                 FROM Pagos
                                 WHERE FechaPago BETWEEN @Desde AND @Hasta
                                 ORDER BY FechaPago DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Desde", desde);
                cmd.Parameters.AddWithValue("@Hasta", hasta);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ===============================
        // 🔥 REPORTE VENTAS
        // ===============================
        public DataTable ObtenerVentasPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT Fecha, ClienteId, Total, MetodoPago 
                                 FROM Ventas
                                 WHERE Fecha BETWEEN @Desde AND @Hasta
                                 ORDER BY Fecha DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Desde", desde);
                cmd.Parameters.AddWithValue("@Hasta", hasta);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }
    }
}