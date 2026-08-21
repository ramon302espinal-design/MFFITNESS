using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DL
{
    /// <summary>
    /// Lectura agregada para CRM inventario+costos (FASE 4.5 / 7.3–7.5).
    /// Una consulta: evita N+1.
    /// </summary>
    public class CrmInventoryFinancialDAL
    {
        private readonly DBHelper db = new DBHelper();

        /// <summary>
        /// Productos activos + agregados de ventas (opcionalmente por período)
        /// + unidades en ventana de velocidad (FASE 7.5).
        /// </summary>
        /// <param name="velocityFrom">Inicio inclusivo ventana velocidad (requerido).</param>
        /// <param name="velocityToExclusive">Fin exclusivo ventana velocidad (requerido).</param>
        public DataTable ObtenerBaseFinanciera(
            DateTime? desde = null,
            DateTime? hasta = null,
            DateTime? velocityFrom = null,
            DateTime? velocityToExclusive = null)
        {
            bool filtrarPeriodo = desde.HasValue || hasta.HasValue;

            string filtroFecha = string.Empty;
            if (filtrarPeriodo)
            {
                if (desde.HasValue)
                    filtroFecha += " AND v.Fecha >= @Desde";
                if (hasta.HasValue)
                    filtroFecha += " AND v.Fecha < @Hasta";
            }

            DateTime velFrom = (velocityFrom ?? DateTime.Today.AddDays(-30)).Date;
            DateTime velTo = (velocityToExclusive ?? DateTime.Today.AddDays(1)).Date;

            string query = $@"
SELECT
    P.Id AS ProductId,
    P.Nombre AS ProductName,
    C.Nombre AS Category,
    P.Activo,
    P.StockActual AS Stock,
    P.StockMinimo,
    ISNULL(P.PrecioCompra, 0) AS UnitCost,
    ISNULL(P.PrecioVenta, 0) AS SalePrice,
    ISNULL(S.UnitsSold, 0) AS UnitsSold,
    ISNULL(S.Revenue, 0) AS Revenue,
    ISNULL(S.Cogs, 0) AS Cogs,
    ISNULL(S.RealizedProfit, 0) AS RealizedProfit,
    ISNULL(S.LinesWithCost, 0) AS LinesWithCost,
    ISNULL(S.LinesWithoutCost, 0) AS LinesWithoutCost,
    S.FirstSaleDate,
    S.LastSaleDate,
    E.FirstEntryDate,
    E.LatestEntryDate,
    ISNULL(V.UnitsSoldVelocity, 0) AS UnitsSoldVelocity,
    ISNULL(V.CogsVelocity, 0) AS CogsVelocity
FROM Productos P
INNER JOIN Categorias C ON C.Id = P.IdCategoria
OUTER APPLY (
    SELECT
        SUM(d.Cantidad) AS UnitsSold,
        SUM(d.Subtotal) AS Revenue,
        SUM(CASE
                WHEN d.CostoUnitario IS NOT NULL
                THEN d.Cantidad * d.CostoUnitario
                ELSE 0
            END) AS Cogs,
        SUM(CASE
                WHEN d.CostoUnitario IS NOT NULL
                THEN d.Subtotal - (d.Cantidad * d.CostoUnitario)
                ELSE 0
            END) AS RealizedProfit,
        SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN 1 ELSE 0 END) AS LinesWithCost,
        SUM(CASE WHEN d.CostoUnitario IS NULL THEN 1 ELSE 0 END) AS LinesWithoutCost,
        MIN(v.Fecha) AS FirstSaleDate,
        MAX(v.Fecha) AS LastSaleDate
    FROM DetalleVentas d
    INNER JOIN Ventas v ON v.Id = d.VentaId
    WHERE d.ProductoId = P.Id
    {filtroFecha}
) S
OUTER APPLY (
    SELECT
        MIN(M.Fecha) AS FirstEntryDate,
        MAX(M.Fecha) AS LatestEntryDate
    FROM MovimientosStock M
    WHERE M.ProductoId = P.Id
      AND M.TipoMovimiento = N'ENTRADA'
) E
OUTER APPLY (
    SELECT
        SUM(d.Cantidad) AS UnitsSoldVelocity,
        SUM(CASE
                WHEN d.CostoUnitario IS NOT NULL
                THEN d.Cantidad * d.CostoUnitario
                ELSE 0
            END) AS CogsVelocity
    FROM DetalleVentas d
    INNER JOIN Ventas v ON v.Id = d.VentaId
    WHERE d.ProductoId = P.Id
      AND v.Fecha >= @VelocityDesde
      AND v.Fecha < @VelocityHasta
) V
WHERE P.Activo = 1
ORDER BY P.Nombre";

            var parametros = new List<SqlParameter>
            {
                new SqlParameter("@VelocityDesde", velFrom),
                new SqlParameter("@VelocityHasta", velTo)
            };
            if (desde.HasValue)
                parametros.Add(new SqlParameter("@Desde", desde.Value));
            if (hasta.HasValue)
                parametros.Add(new SqlParameter("@Hasta", hasta.Value));

            return db.ExecuteQuery(query, parametros.ToArray());
        }
    }
}
