using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    /// <summary>
    /// Prepago reserva productos (saldo a favor). Cobro en caja; despacho posterior sin nuevo cobro.
    /// </summary>
    public class SaldoClienteDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ObtenerActivos()
        {
            const string query = @"
                SELECT
                    Id,
                    ClienteId,
                    ClienteNombre,
                    TotalReserva,
                    MontoCobrado,
                    FechaCobro,
                    Usuario
                FROM dbo.SaldoClientes
                WHERE Estado = N'ACTIVO'
                ORDER BY FechaCobro DESC";

            return db.ExecuteQuery(query);
        }

        public DataTable ObtenerDetalle(int saldoClienteId)
        {
            const string query = @"
                SELECT
                    ProductoId,
                    Producto,
                    Precio,
                    Cantidad,
                    Total
                FROM dbo.SaldoClientesDetalle
                WHERE SaldoClienteId = @Id
                ORDER BY Id";

            return db.ExecuteQuery(query, new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = saldoClienteId }
            });
        }

        public int? ObtenerIdActivoPorCliente(int clienteId)
        {
            const string query = @"
                SELECT TOP 1 Id
                FROM dbo.SaldoClientes
                WHERE ClienteId = @ClienteId
                  AND Estado = N'ACTIVO'
                ORDER BY Id DESC";

            object? result = db.ExecuteScalar(query, new[]
            {
                new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId }
            });

            if (result == null || result == DBNull.Value)
                return null;

            return Convert.ToInt32(result);
        }

        public DataRow? ObtenerCabeceraActiva(int saldoClienteId)
        {
            const string query = @"
                SELECT
                    Id, ClienteId, ClienteNombre, TotalReserva, MontoCobrado,
                    CajaMovimientoId, VentaId, FechaCobro, Usuario, Estado
                FROM dbo.SaldoClientes
                WHERE Id = @Id AND Estado = N'ACTIVO'";

            DataTable dt = db.ExecuteQuery(query, new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = saldoClienteId }
            });

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>Inserta saldo + detalle + caja en una sola transacción.</summary>
        public int CobrarSaldoConCaja(
            int clienteId,
            string clienteNombre,
            decimal totalReserva,
            decimal montoCobrado,
            string usuario,
            string metodoPago,
            string conceptoCaja,
            DataTable lineas)
        {
            using var conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                CancelarActivoPorCliente(conn, tx, clienteId);

                var cajaDal = new CajaDAL();
                var caja = cajaDal.ObtenerCajaAbierta(conn, tx)
                    ?? throw new Exception("No hay caja abierta.");
                int cajaId = Convert.ToInt32(caja["Id"]);

                int cajaMovId = cajaDal.InsertarMovimientoConId(
                    conn,
                    tx,
                    cajaId,
                    "INGRESO",
                    conceptoCaja,
                    montoCobrado,
                    usuario,
                    metodoPago,
                    clienteId);

                int saldoId;
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.SaldoClientes
                        (ClienteId, ClienteNombre, TotalReserva, MontoCobrado, CajaMovimientoId, Usuario, Estado)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ClienteId, @ClienteNombre, @TotalReserva, @MontoCobrado, @CajaMovimientoId, @Usuario, N'ACTIVO');",
                    conn, tx))
                {
                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                    cmd.Parameters.Add("@ClienteNombre", SqlDbType.NVarChar, 200).Value = clienteNombre;
                    cmd.Parameters.Add("@TotalReserva", SqlDbType.Decimal).Value = totalReserva;
                    cmd.Parameters.Add("@MontoCobrado", SqlDbType.Decimal).Value = montoCobrado;
                    cmd.Parameters.Add("@CajaMovimientoId", SqlDbType.Int).Value = cajaMovId;
                    cmd.Parameters.Add("@Usuario", SqlDbType.NVarChar, 100).Value =
                        (object?)usuario ?? DBNull.Value;
                    saldoId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (DataRow row in lineas.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    using var cmdDet = new SqlCommand(@"
                        INSERT INTO dbo.SaldoClientesDetalle
                            (SaldoClienteId, ProductoId, Producto, Precio, Cantidad, Total)
                        VALUES
                            (@SaldoClienteId, @ProductoId, @Producto, @Precio, @Cantidad, @Total);",
                        conn, tx);

                    cmdDet.Parameters.Add("@SaldoClienteId", SqlDbType.Int).Value = saldoId;
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
                return saldoId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Inserta saldo + detalle (caja ya registrada).</summary>
        public int CobrarSaldo(
            int clienteId,
            string clienteNombre,
            decimal totalReserva,
            decimal montoCobrado,
            string usuario,
            int cajaMovimientoId,
            DataTable lineas)
        {
            using var conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                CancelarActivoPorCliente(conn, tx, clienteId);

                int saldoId;
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.SaldoClientes
                        (ClienteId, ClienteNombre, TotalReserva, MontoCobrado, CajaMovimientoId, Usuario, Estado)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@ClienteId, @ClienteNombre, @TotalReserva, @MontoCobrado, @CajaMovimientoId, @Usuario, N'ACTIVO');",
                    conn, tx))
                {
                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                    cmd.Parameters.Add("@ClienteNombre", SqlDbType.NVarChar, 200).Value = clienteNombre;
                    cmd.Parameters.Add("@TotalReserva", SqlDbType.Decimal).Value = totalReserva;
                    cmd.Parameters.Add("@MontoCobrado", SqlDbType.Decimal).Value = montoCobrado;
                    cmd.Parameters.Add("@CajaMovimientoId", SqlDbType.Int).Value = cajaMovimientoId;
                    cmd.Parameters.Add("@Usuario", SqlDbType.NVarChar, 100).Value =
                        (object?)usuario ?? DBNull.Value;
                    saldoId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (DataRow row in lineas.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    using var cmdDet = new SqlCommand(@"
                        INSERT INTO dbo.SaldoClientesDetalle
                            (SaldoClienteId, ProductoId, Producto, Precio, Cantidad, Total)
                        VALUES
                            (@SaldoClienteId, @ProductoId, @Producto, @Precio, @Cantidad, @Total);",
                        conn, tx);

                    cmdDet.Parameters.Add("@SaldoClienteId", SqlDbType.Int).Value = saldoId;
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
                return saldoId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void MarcarDespachado(int saldoClienteId, int ventaId, string usuario)
        {
            const string query = @"
                UPDATE dbo.SaldoClientes
                SET Estado = N'DESPACHADO',
                    VentaId = @VentaId,
                    FechaDespacho = SYSDATETIME()
                WHERE Id = @Id AND Estado = N'ACTIVO'";

            int rows = db.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = saldoClienteId },
                new SqlParameter("@VentaId", SqlDbType.Int) { Value = ventaId }
            });

            if (rows == 0)
                throw new Exception("El saldo a favor ya no está activo.");

            _ = usuario;
        }

        public void CancelarPorCliente(int clienteId)
        {
            const string query = @"
                UPDATE dbo.SaldoClientes
                SET Estado = N'CANCELADO'
                WHERE ClienteId = @ClienteId
                  AND Estado = N'ACTIVO'";

            db.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId }
            });
        }

        private static void CancelarActivoPorCliente(SqlConnection conn, SqlTransaction tx, int clienteId)
        {
            using var cmd = new SqlCommand(@"
                UPDATE dbo.SaldoClientes
                SET Estado = N'CANCELADO'
                WHERE ClienteId = @ClienteId
                  AND Estado = N'ACTIVO';", conn, tx);
            cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            cmd.ExecuteNonQuery();
        }
    }
}
