using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    public class ProductoDAL
    {
        private readonly DBHelper db = new DBHelper();

        public bool ExisteNombre(string nombre)
        {
            string query = @"SELECT COUNT(*) 
                             FROM Productos 
                             WHERE Nombre = @Nombre AND Activo = 1";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Nombre", nombre)
            };

            int count = Convert.ToInt32(db.ExecuteScalar(query, parametros));
            return count > 0;
        }

        public int Agregar(string nombre, int categoriaId, decimal compra, decimal venta, int stockInicial, int stockMinimo, bool activo)
        {
            string query = @"INSERT INTO Productos
                            (Nombre, IdCategoria, PrecioCompra, PrecioVenta, StockActual, StockMinimo, Activo)
                            OUTPUT INSERTED.Id
                            VALUES
                            (@Nombre, @IdCategoria, @Compra, @Venta, @StockActual, @StockMinimo, @Activo)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@IdCategoria", categoriaId),
                new SqlParameter("@Compra", compra),
                new SqlParameter("@Venta", venta),
                new SqlParameter("@StockActual", stockInicial),
                new SqlParameter("@StockMinimo", stockMinimo),
                new SqlParameter("@Activo", activo)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros));
        }

        public void Editar(int id, string nombre, int categoriaId, decimal compra, decimal venta, int stockMinimo, bool activo)
        {
            string query = @"UPDATE Productos SET
                            Nombre = @Nombre,
                            IdCategoria = @IdCategoria,
                            PrecioCompra = @Compra,
                            PrecioVenta = @Venta,
                            StockMinimo = @StockMinimo,
                            Activo = @Activo
                            WHERE Id = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@IdCategoria", categoriaId),
                new SqlParameter("@Compra", compra),
                new SqlParameter("@Venta", venta),
                new SqlParameter("@StockMinimo", stockMinimo),
                new SqlParameter("@Activo", activo)
            };

            db.ExecuteNonQuery(query, parametros);
        }

        public DataTable Listar()
        {
            string query = @"SELECT 
                                P.Id,
                                P.Nombre,
                                P.IdCategoria,
                                C.Nombre AS Categoria,
                                P.PrecioCompra,
                                P.PrecioVenta,
                                P.StockActual,
                                P.StockMinimo,
                                P.Activo
                             FROM Productos P
                             INNER JOIN Categorias C ON P.IdCategoria = C.Id
                             WHERE P.Activo = 1";

            return db.ExecuteQuery(query);
        }

        public void Eliminar(int id)
        {
            string query = @"UPDATE Productos SET Activo = 0 WHERE Id = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id)
            };

            db.ExecuteNonQuery(query, parametros);
        }

        /// <summary>
        /// Costo vigente (PrecioCompra) y stock para snapshot CRM / promedio ponderado.
        /// </summary>
        public (decimal CostoUnitario, int StockActual) ObtenerCostoYStock(int productoId)
        {
            string query = @"SELECT PrecioCompra, StockActual
                             FROM Productos
                             WHERE Id = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", productoId)
            };

            DataTable table = db.ExecuteQuery(query, parametros);
            if (table.Rows.Count == 0)
                throw new Exception("Producto no encontrado.");

            DataRow row = table.Rows[0];
            decimal costo = row["PrecioCompra"] == DBNull.Value
                ? 0m
                : Convert.ToDecimal(row["PrecioCompra"]);
            int stock = row["StockActual"] == DBNull.Value
                ? 0
                : Convert.ToInt32(row["StockActual"]);

            return (costo, stock);
        }
    }
}
