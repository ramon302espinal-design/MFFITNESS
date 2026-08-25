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

        public bool ExisteCodigoBarra(string codigoBarra, int? excluirId = null)
        {
            string query = @"SELECT COUNT(*)
                             FROM Productos
                             WHERE CodigoBarra = @CodigoBarra
                               AND Activo = 1
                               AND (@ExcluirId IS NULL OR Id <> @ExcluirId)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@CodigoBarra", codigoBarra),
                new SqlParameter("@ExcluirId", (object?)excluirId ?? DBNull.Value)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros)) > 0;
        }

        /// <summary>Busca producto activo por código exacto. Null si no existe.</summary>
        public DataRow? BuscarPorCodigoBarra(string codigoBarra)
        {
            string query = @"SELECT TOP 1
                                P.Id,
                                P.Nombre,
                                P.IdCategoria,
                                C.Nombre AS Categoria,
                                P.PrecioCompra,
                                P.PrecioVenta,
                                P.StockActual,
                                P.StockMinimo,
                                P.CodigoBarra,
                                P.RutaImagen,
                                P.Activo
                             FROM Productos P
                             INNER JOIN Categorias C ON P.IdCategoria = C.Id
                             WHERE P.Activo = 1
                               AND P.CodigoBarra = @CodigoBarra";

            SqlParameter[] parametros =
            {
                new SqlParameter("@CodigoBarra", codigoBarra)
            };

            DataTable table = db.ExecuteQuery(query, parametros);
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public int Agregar(string nombre, int categoriaId, decimal compra, decimal venta, int stockInicial, int stockMinimo, bool activo, string? codigoBarra = null, string? rutaImagen = null)
        {
            string query = @"INSERT INTO Productos
                            (Nombre, IdCategoria, PrecioCompra, PrecioVenta, StockActual, StockMinimo, Activo, CodigoBarra, RutaImagen)
                            OUTPUT INSERTED.Id
                            VALUES
                            (@Nombre, @IdCategoria, @Compra, @Venta, @StockActual, @StockMinimo, @Activo, @CodigoBarra, @RutaImagen)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@IdCategoria", categoriaId),
                new SqlParameter("@Compra", compra),
                new SqlParameter("@Venta", venta),
                new SqlParameter("@StockActual", stockInicial),
                new SqlParameter("@StockMinimo", stockMinimo),
                new SqlParameter("@Activo", activo),
                new SqlParameter("@CodigoBarra", (object?)codigoBarra ?? DBNull.Value),
                new SqlParameter("@RutaImagen", (object?)rutaImagen ?? DBNull.Value)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros));
        }

        public void Editar(int id, string nombre, int categoriaId, decimal compra, decimal venta, int stockMinimo, bool activo, string? codigoBarra = null, string? rutaImagen = null)
        {
            string query = @"UPDATE Productos SET
                            Nombre = @Nombre,
                            IdCategoria = @IdCategoria,
                            PrecioCompra = @Compra,
                            PrecioVenta = @Venta,
                            StockMinimo = @StockMinimo,
                            Activo = @Activo,
                            CodigoBarra = @CodigoBarra,
                            RutaImagen = COALESCE(@RutaImagen, RutaImagen)
                            WHERE Id = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@IdCategoria", categoriaId),
                new SqlParameter("@Compra", compra),
                new SqlParameter("@Venta", venta),
                new SqlParameter("@StockMinimo", stockMinimo),
                new SqlParameter("@Activo", activo),
                new SqlParameter("@CodigoBarra", (object?)codigoBarra ?? DBNull.Value),
                new SqlParameter("@RutaImagen", (object?)rutaImagen ?? DBNull.Value)
            };

            db.ExecuteNonQuery(query, parametros);
        }

        public void ActualizarRutaImagen(int id, string? rutaImagen)
        {
            string query = @"UPDATE Productos SET RutaImagen = @RutaImagen WHERE Id = @Id";
            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@RutaImagen", (object?)rutaImagen ?? DBNull.Value)
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
                                P.CodigoBarra,
                                P.RutaImagen,
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
            var (costo, stock, _) = ObtenerCostoStockYMinimo(productoId);
            return (costo, stock);
        }

        /// <summary>Costo, stock actual y stock mínimo (avisos en tiempo real).</summary>
        public (decimal CostoUnitario, int StockActual, int StockMinimo) ObtenerCostoStockYMinimo(int productoId)
        {
            string query = @"SELECT PrecioCompra, StockActual, StockMinimo
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
            int minimo = row["StockMinimo"] == DBNull.Value
                ? 0
                : Convert.ToInt32(row["StockMinimo"]);

            return (costo, stock, minimo);
        }
    }
}
