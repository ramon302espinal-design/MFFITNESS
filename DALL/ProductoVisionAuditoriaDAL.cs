using Microsoft.Data.SqlClient;

namespace DL
{
    /// <summary>
    /// Solo auditoría de sugerencias IA. No inserta productos ni movimientos.
    /// </summary>
    public sealed class ProductoVisionAuditoriaDAL
    {
        private readonly DBHelper _db = new();

        public void Registrar(
            string? usuario,
            string? modelo,
            string? nombreSugerido,
            string? categoriaSugerida,
            decimal? precioCompra,
            decimal? precioVenta,
            string? respuestaRaw)
        {
            // Si la migración 0021 aún no corrió, no romper el flujo de sugerencia UI.
            if (!TablaExiste())
                return;

            const string sql = @"
INSERT INTO dbo.ProductoVisionAuditoria
    (Usuario, Modelo, NombreSugerido, CategoriaSugerida,
     PrecioCompraEstimado, PrecioVentaEstimado, RespuestaRaw)
VALUES
    (@Usuario, @Modelo, @Nombre, @Categoria, @Compra, @Venta, @Raw);";

            SqlParameter[] p =
            {
                new("@Usuario", (object?)usuario ?? DBNull.Value),
                new("@Modelo", (object?)modelo ?? DBNull.Value),
                new("@Nombre", (object?)Trunc(nombreSugerido, 200) ?? DBNull.Value),
                new("@Categoria", (object?)Trunc(categoriaSugerida, 120) ?? DBNull.Value),
                new("@Compra", (object?)precioCompra ?? DBNull.Value),
                new("@Venta", (object?)precioVenta ?? DBNull.Value),
                new("@Raw", (object?)respuestaRaw ?? DBNull.Value)
            };

            _db.ExecuteNonQuery(sql, p);
        }

        private bool TablaExiste()
        {
            try
            {
                object? o = _db.ExecuteScalar(
                    "SELECT OBJECT_ID(N'dbo.ProductoVisionAuditoria', N'U')",
                    Array.Empty<SqlParameter>());
                return o != null && o != DBNull.Value && Convert.ToInt32(o) != 0;
            }
            catch
            {
                return false;
            }
        }

        private static string? Trunc(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
    }
}
