using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DL
{
    /// <summary>
    /// Agregados de ganancias realizadas por período (FASE 5.5 / 5.8 / 5.9).
    /// Fuente P&amp;L: solo DetalleVentas JOIN Ventas vigentes.
    /// No usa MovimientosStock ni DetalleCaja (reversos no alteran ganancia aquí).
    /// Ventas eliminadas (AnularVenta DELETE) no aparecen → no cuentan.
    /// </summary>
    public class CrmProfitAnalyticsDAL
    {
        private readonly DBHelper db = new DBHelper();

        /// <summary>
        /// Agregado de cabeceras Ventas + líneas DetalleVentas.
        /// Fechas: desde inclusivo, hasta exclusivo (sobre Ventas.Fecha).
        /// </summary>
        public DataSet ObtenerAgregadoPeriodo(DateTime? desde, DateTime? hastaExclusive)
        {
            string filtroFecha = BuildFechaFilter(desde, hastaExclusive, "v");

            string sqlHeader = $@"
SELECT
    COUNT(*) AS TransactionCount,
    ISNULL(SUM(v.Total), 0) AS SalesHeaderTotal,
    ISNULL(SUM(v.MontoPagado), 0) AS CollectedAtSale,
    ISNULL(SUM(v.Saldo), 0) AS ReceivableAtSale
FROM Ventas v
WHERE 1 = 1
{filtroFecha}";

            string sqlDetail = $@"
SELECT
    ISNULL(SUM(d.Cantidad), 0) AS UnitsSold,
    ISNULL(SUM(d.Subtotal), 0) AS RevenueTotal,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal ELSE 0 END), 0) AS RevenueWithCost,
    ISNULL(SUM(CASE
            WHEN d.CostoUnitario IS NOT NULL
            THEN d.Cantidad * d.CostoUnitario
            ELSE 0
        END), 0) AS Cogs,
    ISNULL(SUM(CASE
            WHEN d.CostoUnitario IS NOT NULL
            THEN d.Subtotal - (d.Cantidad * d.CostoUnitario)
            ELSE 0
        END), 0) AS RealizedProfit,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN 1 ELSE 0 END), 0) AS LinesWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NULL THEN 1 ELSE 0 END), 0) AS LinesWithoutCost
FROM DetalleVentas d
INNER JOIN Ventas v ON v.Id = d.VentaId
WHERE 1 = 1
{filtroFecha}";

            var parametros = BuildFechaParams(desde, hastaExclusive);

            var ds = new DataSet();
            DataTable header = parametros.Count == 0
                ? db.ExecuteQuery(sqlHeader)
                : db.ExecuteQuery(sqlHeader, parametros.ToArray());
            header.TableName = "Header";

            // Nuevos parámetros (SqlParameter no se reutiliza entre comandos).
            parametros = BuildFechaParams(desde, hastaExclusive);
            DataTable detail = parametros.Count == 0
                ? db.ExecuteQuery(sqlDetail)
                : db.ExecuteQuery(sqlDetail, parametros.ToArray());
            detail.TableName = "Detail";

            ds.Tables.Add(header.Copy());
            ds.Tables.Add(detail.Copy());
            return ds;
        }

        /// <summary>Agregado por producto (FASE 5.8).</summary>
        public DataTable ObtenerPorProducto(DateTime? desde, DateTime? hastaExclusive)
        {
            string filtroFecha = BuildFechaFilter(desde, hastaExclusive, "v");
            string sql = $@"
SELECT
    p.Id AS ProductId,
    p.Nombre AS ProductName,
    c.Id AS CategoryId,
    c.Nombre AS CategoryName,
    COUNT(DISTINCT v.Id) AS TransactionCount,
    ISNULL(SUM(d.Cantidad), 0) AS UnitsSold,
    ISNULL(SUM(d.Subtotal), 0) AS RevenueTotal,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal ELSE 0 END), 0) AS RevenueWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Cantidad * d.CostoUnitario ELSE 0 END), 0) AS Cogs,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal - (d.Cantidad * d.CostoUnitario) ELSE 0 END), 0) AS RealizedProfit,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN 1 ELSE 0 END), 0) AS LinesWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NULL THEN 1 ELSE 0 END), 0) AS LinesWithoutCost
FROM DetalleVentas d
INNER JOIN Ventas v ON v.Id = d.VentaId
INNER JOIN Productos p ON p.Id = d.ProductoId
INNER JOIN Categorias c ON c.Id = p.IdCategoria
WHERE 1 = 1
{filtroFecha}
GROUP BY p.Id, p.Nombre, c.Id, c.Nombre
ORDER BY RealizedProfit DESC, RevenueTotal DESC, p.Nombre";

            return ExecuteConFecha(sql, desde, hastaExclusive);
        }

        /// <summary>Agregado por categoría (FASE 5.8).</summary>
        public DataTable ObtenerPorCategoria(DateTime? desde, DateTime? hastaExclusive)
        {
            string filtroFecha = BuildFechaFilter(desde, hastaExclusive, "v");
            string sql = $@"
SELECT
    c.Id AS CategoryId,
    c.Nombre AS CategoryName,
    COUNT(DISTINCT v.Id) AS TransactionCount,
    ISNULL(SUM(d.Cantidad), 0) AS UnitsSold,
    ISNULL(SUM(d.Subtotal), 0) AS RevenueTotal,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal ELSE 0 END), 0) AS RevenueWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Cantidad * d.CostoUnitario ELSE 0 END), 0) AS Cogs,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal - (d.Cantidad * d.CostoUnitario) ELSE 0 END), 0) AS RealizedProfit,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN 1 ELSE 0 END), 0) AS LinesWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NULL THEN 1 ELSE 0 END), 0) AS LinesWithoutCost
FROM DetalleVentas d
INNER JOIN Ventas v ON v.Id = d.VentaId
INNER JOIN Productos p ON p.Id = d.ProductoId
INNER JOIN Categorias c ON c.Id = p.IdCategoria
WHERE 1 = 1
{filtroFecha}
GROUP BY c.Id, c.Nombre
ORDER BY RealizedProfit DESC, RevenueTotal DESC, c.Nombre";

            return ExecuteConFecha(sql, desde, hastaExclusive);
        }

        /// <summary>Agregado por día (CAST Fecha) + conteo de tickets (FASE 5.8).</summary>
        public DataTable ObtenerPorDia(DateTime? desde, DateTime? hastaExclusive)
        {
            string filtroFecha = BuildFechaFilter(desde, hastaExclusive, "v");
            string sql = $@"
SELECT
    CAST(v.Fecha AS date) AS SaleDate,
    COUNT(DISTINCT v.Id) AS TransactionCount,
    ISNULL(SUM(d.Cantidad), 0) AS UnitsSold,
    ISNULL(SUM(d.Subtotal), 0) AS RevenueTotal,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal ELSE 0 END), 0) AS RevenueWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Cantidad * d.CostoUnitario ELSE 0 END), 0) AS Cogs,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal - (d.Cantidad * d.CostoUnitario) ELSE 0 END), 0) AS RealizedProfit,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN 1 ELSE 0 END), 0) AS LinesWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NULL THEN 1 ELSE 0 END), 0) AS LinesWithoutCost
FROM DetalleVentas d
INNER JOIN Ventas v ON v.Id = d.VentaId
WHERE 1 = 1
{filtroFecha}
GROUP BY CAST(v.Fecha AS date)
ORDER BY SaleDate";

            return ExecuteConFecha(sql, desde, hastaExclusive);
        }

        /// <summary>Agregado por hora (0–23) sobre Ventas.Fecha (FASE 9.8).</summary>
        public DataTable ObtenerPorHora(DateTime? desde, DateTime? hastaExclusive)
        {
            string filtroFecha = BuildFechaFilter(desde, hastaExclusive, "v");
            string sql = $@"
SELECT
    DATEPART(hour, v.Fecha) AS SaleHour,
    COUNT(DISTINCT v.Id) AS TransactionCount,
    ISNULL(SUM(d.Cantidad), 0) AS UnitsSold,
    ISNULL(SUM(d.Subtotal), 0) AS RevenueTotal,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal ELSE 0 END), 0) AS RevenueWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Cantidad * d.CostoUnitario ELSE 0 END), 0) AS Cogs,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN d.Subtotal - (d.Cantidad * d.CostoUnitario) ELSE 0 END), 0) AS RealizedProfit,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NOT NULL THEN 1 ELSE 0 END), 0) AS LinesWithCost,
    ISNULL(SUM(CASE WHEN d.CostoUnitario IS NULL THEN 1 ELSE 0 END), 0) AS LinesWithoutCost
FROM DetalleVentas d
INNER JOIN Ventas v ON v.Id = d.VentaId
WHERE 1 = 1
{filtroFecha}
GROUP BY DATEPART(hour, v.Fecha)
ORDER BY SaleHour";

            return ExecuteConFecha(sql, desde, hastaExclusive);
        }

        private DataTable ExecuteConFecha(string sql, DateTime? desde, DateTime? hastaExclusive)
        {
            var parametros = BuildFechaParams(desde, hastaExclusive);
            return parametros.Count == 0
                ? db.ExecuteQuery(sql)
                : db.ExecuteQuery(sql, parametros.ToArray());
        }

        private static string BuildFechaFilter(DateTime? desde, DateTime? hastaExclusive, string alias)
        {
            string filtro = string.Empty;
            if (desde.HasValue)
                filtro += $" AND {alias}.Fecha >= @Desde";
            if (hastaExclusive.HasValue)
                filtro += $" AND {alias}.Fecha < @Hasta";
            return filtro;
        }

        private static List<SqlParameter> BuildFechaParams(DateTime? desde, DateTime? hastaExclusive)
        {
            var parametros = new List<SqlParameter>();
            if (desde.HasValue)
                parametros.Add(new SqlParameter("@Desde", desde.Value));
            if (hastaExclusive.HasValue)
                parametros.Add(new SqlParameter("@Hasta", hastaExclusive.Value));
            return parametros;
        }
    }
}
