using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    /// <summary>
    /// Snapshot de carrito POS en pausa. No toca Ventas/Caja/Stock.
    /// </summary>
    public class VentaPausadaDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ObtenerPausadasActivas()
        {
            const string query = @"
                SELECT
                    Id,
                    ClienteId,
                    ClienteNombre,
                    Total,
                    FechaPausa,
                    Usuario
                FROM dbo.VentasPausadas
                WHERE Estado = N'PAUSADA'
                ORDER BY FechaPausa DESC";

            return db.ExecuteQuery(query);
        }

        public DataTable ObtenerDetalle(int ventaPausadaId)
        {
            const string query = @"
                SELECT
                    ProductoId,
                    Producto,
                    Precio,
                    Cantidad,
                    Total
                FROM dbo.VentasPausadasDetalle
                WHERE VentaPausadaId = @Id
                ORDER BY Id";

            return db.ExecuteQuery(query, new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = ventaPausadaId }
            });
        }

        public int? ObtenerIdPausaActivaPorCliente(int clienteId)
        {
            const string query = @"
                SELECT TOP 1 Id
                FROM dbo.VentasPausadas
                WHERE ClienteId = @ClienteId
                  AND Estado = N'PAUSADA'
                ORDER BY Id DESC";

            object? result = db.ExecuteScalar(query, new[]
            {
                new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId }
            });

            if (result == null || result == DBNull.Value)
                return null;

            return Convert.ToInt32(result);
        }

        public DataRow? ObtenerCabeceraActiva(int ventaPausadaId)
        {
            const string query = @"
                SELECT Id, ClienteId, ClienteNombre, Total, FechaPausa, Usuario, Estado
                FROM dbo.VentasPausadas
                WHERE Id = @Id AND Estado = N'PAUSADA'";

            DataTable dt = db.ExecuteQuery(query, new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = ventaPausadaId }
            });

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Reemplaza (si existe) la pausa activa del cliente e inserta cabecera + detalle.
        /// </summary>
        public int Pausar(
            int clienteId,
            string clienteNombre,
            decimal total,
            string usuario,
            DataTable lineas)
        {
            using var conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                CancelarActivaPorCliente(conn, tx, clienteId, usuario, "Reemplazo de pausa");

                int ventaPausadaId;
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.VentasPausadas
                        (ClienteId, ClienteNombre, Total, Usuario, Estado)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ClienteId, @ClienteNombre, @Total, @Usuario, N'PAUSADA');", conn, tx))
                {
                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                    cmd.Parameters.Add("@ClienteNombre", SqlDbType.NVarChar, 200).Value = clienteNombre;
                    cmd.Parameters.Add("@Total", SqlDbType.Decimal).Value = total;
                    cmd.Parameters.Add("@Usuario", SqlDbType.NVarChar, 100).Value =
                        (object?)usuario ?? DBNull.Value;
                    ventaPausadaId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (DataRow row in lineas.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    using var cmdDet = new SqlCommand(@"
                        INSERT INTO dbo.VentasPausadasDetalle
                            (VentaPausadaId, ProductoId, Producto, Precio, Cantidad, Total)
                        VALUES
                            (@VentaPausadaId, @ProductoId, @Producto, @Precio, @Cantidad, @Total);",
                        conn, tx);

                    cmdDet.Parameters.Add("@VentaPausadaId", SqlDbType.Int).Value = ventaPausadaId;
                    cmdDet.Parameters.Add("@ProductoId", SqlDbType.Int).Value =
                        Convert.ToInt32(row["ProductoId"]);
                    cmdDet.Parameters.Add("@Producto", SqlDbType.NVarChar, 200).Value =
                        Convert.ToString(row["Producto"]) ?? string.Empty;
                    cmdDet.Parameters.Add("@Precio", SqlDbType.Decimal).Value =
                        Convert.ToDecimal(row["Precio"]);
                    cmdDet.Parameters.Add("@Cantidad", SqlDbType.Int).Value =
                        Convert.ToInt32(row["Cantidad"]);
                    cmdDet.Parameters.Add("@Total", SqlDbType.Decimal).Value =
                        Convert.ToDecimal(row["Total"]);
                    cmdDet.ExecuteNonQuery();
                }

                tx.Commit();
                return ventaPausadaId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void MarcarEstado(int ventaPausadaId, string estado)
        {
            const string query = @"
                UPDATE dbo.VentasPausadas
                SET Estado = @Estado
                WHERE Id = @Id AND Estado = N'PAUSADA'";

            int rows = db.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = ventaPausadaId },
                new SqlParameter("@Estado", SqlDbType.NVarChar, 20) { Value = estado }
            });

            if (rows == 0)
                throw new Exception("La venta pausada no existe o ya no está activa.");
        }

        public void CancelarPorCliente(int clienteId)
        {
            const string query = @"
                UPDATE dbo.VentasPausadas
                SET Estado = N'CANCELADA'
                WHERE ClienteId = @ClienteId
                  AND Estado = N'PAUSADA'";

            db.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId }
            });
        }

        private static void CancelarActivaPorCliente(
            SqlConnection conn,
            SqlTransaction tx,
            int clienteId,
            string usuario,
            string motivo)
        {
            using var cmd = new SqlCommand(@"
                UPDATE dbo.VentasPausadas
                SET Estado = N'CANCELADA'
                WHERE ClienteId = @ClienteId
                  AND Estado = N'PAUSADA';", conn, tx);
            cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            cmd.ExecuteNonQuery();
            _ = usuario;
            _ = motivo;
        }
    }
}
