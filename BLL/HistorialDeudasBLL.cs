using Microsoft.Data.SqlClient;
using System;
using System.Data;
using DL;

namespace BLL
{
    public class HistorialBLL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ObtenerHistorial(int? clienteId, string? tipo, DateTime? desde, DateTime? hasta)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(db.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ObtenerHistorial", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = (object)clienteId ?? DBNull.Value;
                    cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 50).Value = (object)tipo ?? DBNull.Value;
                    cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = (object)desde ?? DBNull.Value;
                    cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = (object)hasta ?? DBNull.Value;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    conn.Open();
                    da.Fill(dt);

                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener historial: " + ex.Message);
            }
        }
    }
}