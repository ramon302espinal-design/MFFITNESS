using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DL
{
    /// <summary>
    /// Persistencia CRM inversiones ↔ ENTRADAS (FASE 6.3).
    /// </summary>
    public class CrmInvestmentDAL
    {
        private readonly DBHelper db = new DBHelper();

        public int CrearInversion(
            string nombre,
            string? descripcion,
            DateTime fechaInicio,
            byte estado,
            string? observaciones,
            string? usuario)
        {
            const string sql = @"
INSERT INTO CrmInversiones
    (Nombre, Descripcion, FechaInicio, Estado, Observaciones, UsuarioCreacion)
OUTPUT INSERTED.Id
VALUES
    (@Nombre, @Descripcion, @FechaInicio, @Estado, @Observaciones, @Usuario)";

            SqlParameter[] p =
            {
                new("@Nombre", nombre.Trim()),
                new("@Descripcion", (object?)descripcion?.Trim() ?? DBNull.Value),
                new("@FechaInicio", fechaInicio.Date),
                new("@Estado", estado),
                new("@Observaciones", (object?)observaciones?.Trim() ?? DBNull.Value),
                new("@Usuario", (object?)usuario?.Trim() ?? DBNull.Value)
            };

            return Convert.ToInt32(db.ExecuteScalar(sql, p));
        }

        public DataTable ListarInversiones()
        {
            const string sql = @"
SELECT Id, Nombre, Descripcion, FechaInicio, FechaCierre, Estado,
       Observaciones, UsuarioCreacion, FechaCreacion
FROM CrmInversiones
ORDER BY FechaInicio DESC, Id DESC";
            return db.ExecuteQuery(sql);
        }

        public DataRow? ObtenerInversion(int id)
        {
            const string sql = @"
SELECT Id, Nombre, Descripcion, FechaInicio, FechaCierre, Estado,
       Observaciones, UsuarioCreacion, FechaCreacion
FROM CrmInversiones
WHERE Id = @Id";
            DataTable t = db.ExecuteQuery(sql, new[] { new SqlParameter("@Id", id) });
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        public void ActualizarEstado(int inversionId, byte estado, DateTime? fechaCierre)
        {
            const string sql = @"
UPDATE CrmInversiones
SET Estado = @Estado,
    FechaCierre = @FechaCierre
WHERE Id = @Id";
            db.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@Estado", estado),
                new SqlParameter("@FechaCierre", (object?)fechaCierre?.Date ?? DBNull.Value),
                new SqlParameter("@Id", inversionId)
            });
        }

        /// <summary>
        /// Asigna una ENTRADA a la inversión. Falla si no es ENTRADA o ya está asignada.
        /// </summary>
        public int AsignarEntrada(int inversionId, int movimientoStockId)
        {
            using SqlConnection conn = db.GetConnection();
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();

            try
            {
                EnsureEntradaDisponible(movimientoStockId, conn, tx);

                const string insert = @"
INSERT INTO CrmInversionLineas (InversionId, MovimientoStockId)
OUTPUT INSERTED.Id
VALUES (@InversionId, @MovimientoStockId)";

                using var cmd = new SqlCommand(insert, conn, tx);
                cmd.Parameters.AddWithValue("@InversionId", inversionId);
                cmd.Parameters.AddWithValue("@MovimientoStockId", movimientoStockId);
                int lineId = Convert.ToInt32(cmd.ExecuteScalar());

                // Si estaba planificada → activa al materializar capital
                using var cmdEstado = new SqlCommand(@"
UPDATE CrmInversiones
SET Estado = CASE WHEN Estado = 0 THEN 1 ELSE Estado END
WHERE Id = @Id", conn, tx);
                cmdEstado.Parameters.AddWithValue("@Id", inversionId);
                cmdEstado.ExecuteNonQuery();

                tx.Commit();
                return lineId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void QuitarEntrada(int inversionId, int movimientoStockId)
        {
            const string sql = @"
DELETE FROM CrmInversionLineas
WHERE InversionId = @InversionId AND MovimientoStockId = @MovimientoStockId";
            int n = db.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@InversionId", inversionId),
                new SqlParameter("@MovimientoStockId", movimientoStockId)
            });
            if (n == 0)
                throw new Exception("La entrada no está asignada a esta inversión.");
        }

        public DataTable ListarLineas(int inversionId)
        {
            const string sql = @"
SELECT
    L.Id,
    L.InversionId,
    L.MovimientoStockId,
    L.FechaAsignacion,
    M.ProductoId,
    P.Nombre AS Producto,
    M.Cantidad,
    M.CostoUnitario,
    M.CostoTotal,
    M.Fecha AS FechaEntrada,
    M.Descripcion,
    M.TipoMovimiento
FROM CrmInversionLineas L
INNER JOIN MovimientosStock M ON M.Id = L.MovimientoStockId
INNER JOIN Productos P ON P.Id = M.ProductoId
WHERE L.InversionId = @InversionId
ORDER BY M.Fecha, L.Id";
            return db.ExecuteQuery(sql, new[] { new SqlParameter("@InversionId", inversionId) });
        }

        /// <summary>ENTRADAS aún no asignadas a ninguna inversión.</summary>
        public DataTable ListarEntradasDisponibles()
        {
            const string sql = @"
SELECT
    M.Id AS MovimientoStockId,
    M.ProductoId,
    P.Nombre AS Producto,
    M.Cantidad,
    M.CostoUnitario,
    M.CostoTotal,
    M.Fecha,
    M.Descripcion,
    M.Usuario
FROM MovimientosStock M
INNER JOIN Productos P ON P.Id = M.ProductoId
WHERE UPPER(LTRIM(RTRIM(M.TipoMovimiento))) = N'ENTRADA'
  AND NOT EXISTS (
        SELECT 1 FROM CrmInversionLineas L WHERE L.MovimientoStockId = M.Id
  )
ORDER BY M.Fecha DESC, M.Id DESC";
            return db.ExecuteQuery(sql);
        }

        /// <summary>Productos agregados por inversión (FASE 6.4).</summary>
        public DataTable ListarProductosPorInversion(int inversionId)
        {
            const string sql = @"
SELECT
    M.ProductoId,
    P.Nombre AS Producto,
    C.Nombre AS Categoria,
    SUM(M.Cantidad) AS QuantityPurchased,
    COUNT(*) AS EntryCount,
    SUM(CASE WHEN M.CostoUnitario IS NULL AND M.CostoTotal IS NULL THEN 1 ELSE 0 END) AS EntriesWithoutCost,
    SUM(CASE
            WHEN M.CostoTotal IS NOT NULL THEN M.CostoTotal
            WHEN M.CostoUnitario IS NOT NULL THEN M.Cantidad * M.CostoUnitario
            ELSE 0
        END) AS CapitalAssigned,
    SUM(CASE
            WHEN M.CostoTotal IS NOT NULL OR M.CostoUnitario IS NOT NULL THEN M.Cantidad
            ELSE 0
        END) AS QtyWithCost
FROM CrmInversionLineas L
INNER JOIN MovimientosStock M ON M.Id = L.MovimientoStockId
INNER JOIN Productos P ON P.Id = M.ProductoId
INNER JOIN Categorias C ON C.Id = P.IdCategoria
WHERE L.InversionId = @InversionId
GROUP BY M.ProductoId, P.Nombre, C.Nombre
ORDER BY CapitalAssigned DESC, P.Nombre";
            return db.ExecuteQuery(sql, new[] { new SqlParameter("@InversionId", inversionId) });
        }

        /// <summary>
        /// Ventas de productos de la inversión (para FIFO de capital recuperado).
        /// </summary>
        public DataTable ListarVentasDeProductosInversion(int inversionId)
        {
            const string sql = @"
SELECT
    d.Id AS SaleLineId,
    d.ProductoId,
    v.Fecha AS SaleDate,
    d.Cantidad,
    d.Subtotal,
    d.CostoUnitario
FROM DetalleVentas d
INNER JOIN Ventas v ON v.Id = d.VentaId
WHERE d.ProductoId IN (
    SELECT DISTINCT M.ProductoId
    FROM CrmInversionLineas L
    INNER JOIN MovimientosStock M ON M.Id = L.MovimientoStockId
    WHERE L.InversionId = @InversionId
)
ORDER BY v.Fecha, d.Id";
            return db.ExecuteQuery(sql, new[] { new SqlParameter("@InversionId", inversionId) });
        }

        /// <summary>Precios de venta actuales de productos de la inversión (potencial).</summary>
        public Dictionary<int, decimal> ObtenerPreciosVentaProductosInversion(int inversionId)
        {
            const string sql = @"
SELECT DISTINCT M.ProductoId, ISNULL(P.PrecioVenta, 0) AS PrecioVenta
FROM CrmInversionLineas L
INNER JOIN MovimientosStock M ON M.Id = L.MovimientoStockId
INNER JOIN Productos P ON P.Id = M.ProductoId
WHERE L.InversionId = @InversionId";

            DataTable table = db.ExecuteQuery(sql, new[] { new SqlParameter("@InversionId", inversionId) });
            var map = new Dictionary<int, decimal>();
            foreach (DataRow row in table.Rows)
            {
                int id = Convert.ToInt32(row["ProductoId"]);
                decimal price = row["PrecioVenta"] == DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(row["PrecioVenta"]);
                map[id] = price;
            }

            return map;
        }

        private static void EnsureEntradaDisponible(
            int movimientoStockId,
            SqlConnection conn,
            SqlTransaction tx)
        {
            using var cmd = new SqlCommand(@"
SELECT TipoMovimiento,
       (SELECT COUNT(*) FROM CrmInversionLineas WHERE MovimientoStockId = @Id) AS YaAsignado
FROM MovimientosStock
WHERE Id = @Id", conn, tx);
            cmd.Parameters.AddWithValue("@Id", movimientoStockId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                throw new Exception("Movimiento de stock no encontrado.");

            string tipo = reader["TipoMovimiento"]?.ToString()?.Trim().ToUpperInvariant() ?? "";
            int ya = Convert.ToInt32(reader["YaAsignado"]);

            if (tipo != "ENTRADA")
                throw new Exception("Solo se pueden asignar movimientos de tipo ENTRADA.");
            if (ya > 0)
                throw new Exception("Esta entrada ya está asignada a otra inversión (regla v1).");
        }
    }
}
