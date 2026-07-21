using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    public class StockDAL
    {
        private readonly DBHelper db = new DBHelper();

        // ===============================
        // OBTENER MOVIMIENTOS
        // ===============================
        public DataTable ObtenerMovimientos()
        {
            string query = @"SELECT 
                                M.Id,
                                P.Nombre AS Producto,
                                M.TipoMovimiento,
                                M.Cantidad,
                                M.StockAnterior,
                                M.StockNuevo,
                                M.Fecha,
                                M.Usuario,
                                M.Descripcion
                             FROM MovimientosStock M
                             INNER JOIN Productos P ON P.Id = M.ProductoId
                             ORDER BY M.Fecha DESC";

            return db.ExecuteQuery(query);
        }

        // ===============================
        // REGISTRAR ENTRADA DE STOCK
        // ===============================
        public int RegistrarEntrada(int productoId, int cantidad, string usuario, string descripcion)
        {
            if (cantidad <= 0) throw new Exception("La cantidad debe ser mayor a cero.");

            using SqlConnection conn = db.GetConnection();
            conn.Open();
            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                int stockAnterior = ObtenerStockActual(productoId, conn, transaction);
                int stockNuevo = stockAnterior + cantidad;

                int movimientoId = InsertarMovimiento(productoId, "ENTRADA", cantidad, stockAnterior, stockNuevo, usuario, descripcion, conn, transaction);
                ActualizarStock(productoId, cantidad, true, conn, transaction);

                transaction.Commit();
                return movimientoId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ===============================
        // REGISTRAR SALIDA DE STOCK
        // ===============================
        public int RegistrarSalida(int productoId, int cantidad, string usuario, string descripcion)
        {
            if (cantidad <= 0) throw new Exception("La cantidad debe ser mayor a cero.");

            using SqlConnection conn = db.GetConnection();
            conn.Open();
            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                int stockAnterior = ObtenerStockActual(productoId, conn, transaction);
                if (stockAnterior < cantidad)
                    throw new Exception("Stock insuficiente para realizar la venta.");

                int stockNuevo = stockAnterior - cantidad;

                int movimientoId = InsertarMovimiento(productoId, "SALIDA", cantidad, stockAnterior, stockNuevo, usuario, descripcion, conn, transaction);
                ActualizarStock(productoId, cantidad, false, conn, transaction);

                transaction.Commit();
                return movimientoId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void RevertirMovimiento(int movimientoId, string usuario)
        {
            using SqlConnection conn = db.GetConnection();
            conn.Open();
            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                SqlCommand cmdGet = new SqlCommand(@"
                    SELECT ProductoId, TipoMovimiento, Cantidad, Descripcion
                    FROM MovimientosStock
                    WHERE Id = @MovimientoId", conn, transaction);

                cmdGet.Parameters.AddWithValue("@MovimientoId", movimientoId);

                int productoId;
                string tipoOriginal;
                int cantidad;
                string descripcionOriginal;

                using (var reader = cmdGet.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("Movimiento de stock no encontrado.");

                    productoId = Convert.ToInt32(reader["ProductoId"]);
                    tipoOriginal = reader["TipoMovimiento"]?.ToString()?.ToUpperInvariant() ?? "";
                    cantidad = Convert.ToInt32(reader["Cantidad"]);
                    descripcionOriginal = reader["Descripcion"]?.ToString() ?? "";
                }

                if (descripcionOriginal.StartsWith("REVERSO (Ref #", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("No se puede deshacer un movimiento de reversión.");

                string marcaReverso = $"REVERSO (Ref #{movimientoId}):";
                SqlCommand cmdExists = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM MovimientosStock
                    WHERE ProductoId = @ProductoId
                      AND Descripcion LIKE @MarcaReverso", conn, transaction);

                cmdExists.Parameters.AddWithValue("@ProductoId", productoId);
                cmdExists.Parameters.AddWithValue("@MarcaReverso", marcaReverso + "%");

                if (Convert.ToInt32(cmdExists.ExecuteScalar()) > 0)
                    throw new Exception("Este movimiento de stock ya fue deshecho.");

                int stockAnterior = ObtenerStockActual(productoId, conn, transaction);
                string tipoInverso;
                int stockNuevo;

                if (tipoOriginal == "ENTRADA")
                {
                    if (stockAnterior < cantidad)
                        throw new Exception("Stock insuficiente para deshacer la entrada.");

                    tipoInverso = "SALIDA";
                    stockNuevo = stockAnterior - cantidad;
                    ActualizarStock(productoId, cantidad, false, conn, transaction);
                }
                else if (tipoOriginal == "SALIDA")
                {
                    tipoInverso = "ENTRADA";
                    stockNuevo = stockAnterior + cantidad;
                    ActualizarStock(productoId, cantidad, true, conn, transaction);
                }
                else
                {
                    throw new Exception("Tipo de movimiento de stock no soportado para deshacer.");
                }

                string descripcionReverso = $"{marcaReverso} {descripcionOriginal}";
                InsertarMovimiento(productoId, tipoInverso, cantidad, stockAnterior, stockNuevo, usuario, descripcionReverso, conn, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ===============================
        // OBTENER STOCK ACTUAL
        // ===============================
        private int ObtenerStockActual(int productoId, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "SELECT StockActual FROM Productos WHERE Id=@Id";

            using SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", productoId);

            object? result = cmd.ExecuteScalar();
            if (result == null) throw new Exception("Producto no encontrado.");
            return Convert.ToInt32(result);
        }

        // ===============================
        // INSERTAR MOVIMIENTO
        // ===============================
        private int InsertarMovimiento(
            int productoId,
            string TipoMovimiento,
            int cantidad,
            int stockAnterior,
            int stockNuevo,
            string usuario,
            string descripcion,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            string query = @"INSERT INTO MovimientosStock
                             (ProductoId, TipoMovimiento, Cantidad, StockAnterior, StockNuevo, Fecha, Usuario, Descripcion)
                             OUTPUT INSERTED.Id
                             VALUES
                             (@ProductoId, @TipoMovimiento, @Cantidad, @StockAnterior, @StockNuevo, GETDATE(), @Usuario, @Descripcion)";

            using SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@ProductoId", productoId);
            cmd.Parameters.AddWithValue("@TipoMovimiento", TipoMovimiento);
            cmd.Parameters.AddWithValue("@Cantidad", cantidad);
            cmd.Parameters.AddWithValue("@StockAnterior", stockAnterior);
            cmd.Parameters.AddWithValue("@StockNuevo", stockNuevo);
            cmd.Parameters.AddWithValue("@Usuario", usuario);
            cmd.Parameters.AddWithValue("@Descripcion", descripcion);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ===============================
        // ACTUALIZAR STOCK
        // ===============================
        private void ActualizarStock(int productoId, int cantidad, bool esEntrada, SqlConnection conn, SqlTransaction transaction)
        {
            string operador = esEntrada ? "+" : "-";
            string query = $@"UPDATE Productos
                              SET StockActual = StockActual {operador} @Cantidad
                              WHERE Id=@ProductoId";

            using SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Cantidad", cantidad);
            cmd.Parameters.AddWithValue("@ProductoId", productoId);

            cmd.ExecuteNonQuery();
        }
    }
}