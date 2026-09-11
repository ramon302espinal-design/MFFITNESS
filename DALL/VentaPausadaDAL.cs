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

        /// <summary>
        /// Suma líneas a una pausa ACTIVA (misma ProductoId → acumula cantidad/total).
        /// Recalcula Total de cabecera. No cancela ni crea otra pausa.
        /// </summary>
        public void AgregarLineasAPausa(int ventaPausadaId, DataTable lineas)
        {
            if (ventaPausadaId <= 0)
                throw new Exception("Venta pausada inválida.");

            if (lineas == null || lineas.Rows.Count == 0)
                throw new Exception("No hay productos para agregar.");

            using var conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                using (var cmdCab = new SqlCommand(@"
                    SELECT Id FROM dbo.VentasPausadas WITH (UPDLOCK, ROWLOCK)
                    WHERE Id = @Id AND Estado = N'PAUSADA';", conn, tx))
                {
                    cmdCab.Parameters.Add("@Id", SqlDbType.Int).Value = ventaPausadaId;
                    object? ok = cmdCab.ExecuteScalar();
                    if (ok == null || ok == DBNull.Value)
                        throw new Exception("La venta pausada ya no está activa.");
                }

                foreach (DataRow row in lineas.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    int productoId = Convert.ToInt32(row["ProductoId"]);
                    string producto = Convert.ToString(row["Producto"]) ?? string.Empty;
                    decimal precio = Convert.ToDecimal(row["Precio"]);
                    int cantidad = Convert.ToInt32(row["Cantidad"]);
                    decimal totalLinea = Convert.ToDecimal(row["Total"]);

                    if (productoId <= 0 || cantidad <= 0)
                        continue;

                    int? detalleId = null;
                    int cantActual = 0;
                    decimal totalActual = 0m;

                    using (var cmdFind = new SqlCommand(@"
                        SELECT TOP 1 Id, Cantidad, Total
                        FROM dbo.VentasPausadasDetalle WITH (UPDLOCK, ROWLOCK)
                        WHERE VentaPausadaId = @PausaId AND ProductoId = @ProductoId
                        ORDER BY Id;", conn, tx))
                    {
                        cmdFind.Parameters.Add("@PausaId", SqlDbType.Int).Value = ventaPausadaId;
                        cmdFind.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                        using var reader = cmdFind.ExecuteReader();
                        if (reader.Read())
                        {
                            detalleId = reader.GetInt32(0);
                            cantActual = reader.GetInt32(1);
                            totalActual = reader.GetDecimal(2);
                        }
                    }

                    if (detalleId.HasValue)
                    {
                        int nuevaCant = cantActual + cantidad;
                        decimal nuevoTotal = Math.Round(totalActual + totalLinea, 2, MidpointRounding.AwayFromZero);
                        decimal nuevoPrecio = nuevaCant > 0
                            ? Math.Round(nuevoTotal / nuevaCant, 2, MidpointRounding.AwayFromZero)
                            : precio;

                        using var cmdUpd = new SqlCommand(@"
                            UPDATE dbo.VentasPausadasDetalle
                            SET Cantidad = @Cantidad, Precio = @Precio, Total = @Total, Producto = @Producto
                            WHERE Id = @Id;", conn, tx);
                        cmdUpd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = nuevaCant;
                        cmdUpd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = nuevoPrecio;
                        cmdUpd.Parameters.Add("@Total", SqlDbType.Decimal).Value = nuevoTotal;
                        cmdUpd.Parameters.Add("@Producto", SqlDbType.NVarChar, 200).Value = producto;
                        cmdUpd.Parameters.Add("@Id", SqlDbType.Int).Value = detalleId.Value;
                        cmdUpd.ExecuteNonQuery();
                    }
                    else
                    {
                        using var cmdIns = new SqlCommand(@"
                            INSERT INTO dbo.VentasPausadasDetalle
                                (VentaPausadaId, ProductoId, Producto, Precio, Cantidad, Total)
                            VALUES
                                (@VentaPausadaId, @ProductoId, @Producto, @Precio, @Cantidad, @Total);",
                            conn, tx);
                        cmdIns.Parameters.Add("@VentaPausadaId", SqlDbType.Int).Value = ventaPausadaId;
                        cmdIns.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                        cmdIns.Parameters.Add("@Producto", SqlDbType.NVarChar, 200).Value = producto;
                        cmdIns.Parameters.Add("@Precio", SqlDbType.Decimal).Value = precio;
                        cmdIns.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad;
                        cmdIns.Parameters.Add("@Total", SqlDbType.Decimal).Value = totalLinea;
                        cmdIns.ExecuteNonQuery();
                    }
                }

                using (var cmdTot = new SqlCommand(@"
                    UPDATE dbo.VentasPausadas
                    SET Total = (
                        SELECT ISNULL(SUM(Total), 0)
                        FROM dbo.VentasPausadasDetalle
                        WHERE VentaPausadaId = @Id)
                    WHERE Id = @Id AND Estado = N'PAUSADA';", conn, tx))
                {
                    cmdTot.Parameters.Add("@Id", SqlDbType.Int).Value = ventaPausadaId;
                    if (cmdTot.ExecuteNonQuery() == 0)
                        throw new Exception("La venta pausada ya no está activa.");
                }

                tx.Commit();
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
