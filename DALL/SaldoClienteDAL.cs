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
                    Id AS DetalleId,
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

        public DataRow? ObtenerLineaDetalleActiva(int saldoClienteId, int detalleId)
        {
            const string query = @"
                SELECT d.Id AS DetalleId, d.ProductoId, d.Producto, d.Precio, d.Cantidad, d.Total
                FROM dbo.SaldoClientesDetalle d
                INNER JOIN dbo.SaldoClientes s ON s.Id = d.SaldoClienteId
                WHERE d.Id = @DetalleId
                  AND d.SaldoClienteId = @SaldoId
                  AND s.Estado = N'ACTIVO'";

            DataTable dt = db.ExecuteQuery(query, new[]
            {
                new SqlParameter("@DetalleId", SqlDbType.Int) { Value = detalleId },
                new SqlParameter("@SaldoId", SqlDbType.Int) { Value = saldoClienteId }
            });

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Resta unidades de una línea. Si llega a 0, elimina la línea.
        /// Si no quedan líneas, marca DESPACHADO; si quedan, recalcula TotalReserva.
        /// </summary>
        public bool ConsumirDetalleTrasDespacho(
            int saldoClienteId,
            int detalleId,
            int cantidadDespachada,
            int ventaId,
            string usuario)
        {
            if (cantidadDespachada <= 0)
                throw new Exception("Cantidad a despachar inválida.");

            using var conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                int cantidadActual;
                decimal precio;
                using (var cmd = new SqlCommand(@"
                    SELECT d.Cantidad, d.Precio
                    FROM dbo.SaldoClientesDetalle d WITH (UPDLOCK, ROWLOCK)
                    INNER JOIN dbo.SaldoClientes s WITH (UPDLOCK, ROWLOCK)
                        ON s.Id = d.SaldoClienteId
                    WHERE d.Id = @DetalleId
                      AND d.SaldoClienteId = @SaldoId
                      AND s.Estado = N'ACTIVO';", conn, tx))
                {
                    cmd.Parameters.Add("@DetalleId", SqlDbType.Int).Value = detalleId;
                    cmd.Parameters.Add("@SaldoId", SqlDbType.Int).Value = saldoClienteId;
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                        throw new Exception("La línea de reserva ya no está disponible.");

                    cantidadActual = reader.GetInt32(0);
                    precio = reader.GetDecimal(1);
                }

                if (cantidadDespachada > cantidadActual)
                    throw new Exception(
                        $"Solo quedan {cantidadActual} unidad(es) de ese producto en la reserva.");

                int restante = cantidadActual - cantidadDespachada;
                if (restante == 0)
                {
                    using var del = new SqlCommand(@"
                        DELETE FROM dbo.SaldoClientesDetalle
                        WHERE Id = @DetalleId AND SaldoClienteId = @SaldoId;", conn, tx);
                    del.Parameters.Add("@DetalleId", SqlDbType.Int).Value = detalleId;
                    del.Parameters.Add("@SaldoId", SqlDbType.Int).Value = saldoClienteId;
                    del.ExecuteNonQuery();
                }
                else
                {
                    decimal totalLinea = Math.Round(precio * restante, 2, MidpointRounding.AwayFromZero);
                    using var upd = new SqlCommand(@"
                        UPDATE dbo.SaldoClientesDetalle
                        SET Cantidad = @Cantidad, Total = @Total
                        WHERE Id = @DetalleId AND SaldoClienteId = @SaldoId;", conn, tx);
                    upd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = restante;
                    upd.Parameters.Add("@Total", SqlDbType.Decimal).Value = totalLinea;
                    upd.Parameters.Add("@DetalleId", SqlDbType.Int).Value = detalleId;
                    upd.Parameters.Add("@SaldoId", SqlDbType.Int).Value = saldoClienteId;
                    upd.ExecuteNonQuery();
                }

                int lineasRestantes;
                decimal totalRestante;
                using (var cmdSum = new SqlCommand(@"
                    SELECT COUNT(1), ISNULL(SUM(Total), 0)
                    FROM dbo.SaldoClientesDetalle
                    WHERE SaldoClienteId = @SaldoId;", conn, tx))
                {
                    cmdSum.Parameters.Add("@SaldoId", SqlDbType.Int).Value = saldoClienteId;
                    using var reader = cmdSum.ExecuteReader();
                    reader.Read();
                    lineasRestantes = reader.GetInt32(0);
                    totalRestante = reader.GetDecimal(1);
                }

                bool cerrado;
                if (lineasRestantes == 0)
                {
                    using var cerrar = new SqlCommand(@"
                        UPDATE dbo.SaldoClientes
                        SET Estado = N'DESPACHADO',
                            VentaId = @VentaId,
                            FechaDespacho = SYSDATETIME(),
                            TotalReserva = 0
                        WHERE Id = @Id AND Estado = N'ACTIVO';", conn, tx);
                    cerrar.Parameters.Add("@VentaId", SqlDbType.Int).Value = ventaId;
                    cerrar.Parameters.Add("@Id", SqlDbType.Int).Value = saldoClienteId;
                    if (cerrar.ExecuteNonQuery() == 0)
                        throw new Exception("El saldo a favor ya no está activo.");
                    cerrado = true;
                }
                else
                {
                    using var updCab = new SqlCommand(@"
                        UPDATE dbo.SaldoClientes
                        SET TotalReserva = @TotalReserva
                        WHERE Id = @Id AND Estado = N'ACTIVO';", conn, tx);
                    updCab.Parameters.Add("@TotalReserva", SqlDbType.Decimal).Value =
                        Math.Round(totalRestante, 2, MidpointRounding.AwayFromZero);
                    updCab.Parameters.Add("@Id", SqlDbType.Int).Value = saldoClienteId;
                    if (updCab.ExecuteNonQuery() == 0)
                        throw new Exception("El saldo a favor ya no está activo.");
                    cerrado = false;
                }

                tx.Commit();
                _ = usuario;
                return cerrado;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
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
