using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class ReporteDAL
    {
        private readonly DBHelper db = new DBHelper();

        // ===============================
        // INSERTAR REPORTE GENERADO
        // ===============================
        public void InsertarReporte(DateTime fecha, string tipo, string ruta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"INSERT INTO ReportesGenerados (Fecha, Tipo, Ruta)
                                 VALUES (@Fecha, @Tipo, @Ruta)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@Tipo", tipo);
                cmd.Parameters.AddWithValue("@Ruta", ruta);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ===============================
        // VALIDAR SI YA EXISTE
        // ===============================
        public bool ExisteReporte(DateTime fecha, string tipo)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM ReportesGenerados 
                                 WHERE Fecha = @Fecha AND Tipo = @Tipo";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@Tipo", tipo);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }

        // ===============================
        // HISTORIAL
        // ===============================
        public DataTable ObtenerHistorial()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = "SELECT * FROM ReportesGenerados ORDER BY Fecha DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ===============================
        // 🔥 REPORTE CAJA POR FECHA (todo DetalleCaja)
        // ===============================
        public DataTable ObtenerCajaPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
SELECT
    dc.Fecha,
    CASE
        WHEN UPPER(LTRIM(RTRIM(ISNULL(dc.MetodoPago, '')))) = 'REVERSO'
          OR UPPER(LTRIM(RTRIM(ISNULL(dc.Concepto, '')))) LIKE 'REVERSO%'
            THEN 'REVERSO'
        ELSE dc.TipoMovimiento
    END AS [Tipo],
    CASE
        WHEN productos.DetalleProductos IS NOT NULL
            THEN productos.DetalleProductos
        ELSE dc.Concepto
    END AS Concepto,
    dc.Monto,
    COALESCE(
        NULLIF(LTRIM(RTRIM(dc.MetodoPago)), ''),
        v.MetodoPago,
        p.MetodoPago,
        pd.MetodoPago,
        CASE WHEN dc.TipoMovimiento = 'EGRESO' THEN 'N/A' ELSE 'No registrado' END
    ) AS [Método de Pago],
    COALESCE(
        c.Nombre,
        cv.Nombre,
        cp.Nombre,
        CASE
            WHEN dc.Concepto LIKE '%Venta%' AND COALESCE(dc.ClienteId, v.ClienteId) IS NULL
                THEN 'Mostrador'
            ELSE '—'
        END
    ) AS MIEMBRO,
    ISNULL(NULLIF(LTRIM(RTRIM(dc.Usuario)), ''), '—') AS USUARIO
FROM DetalleCaja dc
LEFT JOIN Clientes c ON c.ID = dc.ClienteId
OUTER APPLY (
    -- La coincidencia por Id del concepto manda; el monto/fecha es solo respaldo histórico.
    SELECT TOP 1 vv.Id, vv.ClienteId, vv.MetodoPago
    FROM Ventas vv
    WHERE dc.Concepto LIKE '%Venta de productos (Id ' + CAST(vv.Id AS varchar(20)) + ')%'
       OR (
            dc.Concepto LIKE '%Venta%'
        AND dc.Concepto NOT LIKE '%(Id %'
        AND CAST(dc.Fecha AS date) = CAST(vv.Fecha AS date)
        AND ABS(dc.Monto - ISNULL(vv.MontoPagado, vv.Total)) < 0.01
       )
    ORDER BY
        CASE
            WHEN dc.Concepto LIKE '%Venta de productos (Id ' + CAST(vv.Id AS varchar(20)) + ')%'
                THEN 0
            ELSE 1
        END,
        ABS(DATEDIFF(SECOND, vv.Fecha, dc.Fecha)),
        vv.Id DESC
) v
OUTER APPLY (
    SELECT STRING_AGG(
        CASE
            WHEN dv.Cantidad > 1
                THEN CAST(dv.Cantidad AS varchar(12)) + ' ' + pr.Nombre
            ELSE pr.Nombre
        END,
        ', '
    ) WITHIN GROUP (ORDER BY dv.Id) AS DetalleProductos
    FROM DetalleVentas dv
    INNER JOIN Productos pr ON pr.Id = dv.ProductoId
    WHERE dv.VentaId = v.Id
) productos
LEFT JOIN Clientes cv ON cv.ID = v.ClienteId
OUTER APPLY (
    SELECT TOP 1 pp.MetodoPago, pp.ClienteId
    FROM Pagos pp
    WHERE CAST(pp.FechaPago AS date) = CAST(dc.Fecha AS date)
      AND ABS(pp.Monto - dc.Monto) < 0.01
      AND (
            dc.ClienteId = pp.ClienteId
         OR dc.Concepto LIKE '%Cliente ' + CAST(pp.ClienteId AS varchar(20)) + '%'
         OR dc.Concepto LIKE '%cliente ' + CAST(pp.ClienteId AS varchar(20)) + '%'
      )
    ORDER BY ABS(DATEDIFF(SECOND, pp.FechaPago, dc.Fecha)), pp.Id DESC
) p
LEFT JOIN Clientes cp ON cp.ID = COALESCE(dc.ClienteId, p.ClienteId)
OUTER APPLY (
    SELECT TOP 1 pd0.MetodoPago
    FROM PagosDeuda pd0
    WHERE CAST(pd0.Fecha AS date) = CAST(dc.Fecha AS date)
      AND ABS(pd0.Monto - dc.Monto) < 0.01
      AND ABS(DATEDIFF(SECOND, pd0.Fecha, dc.Fecha)) <= 120
      AND (dc.Concepto LIKE '%deuda%' OR dc.Concepto LIKE '%Abono%' OR dc.Concepto LIKE '%Reverso%')
    ORDER BY ABS(DATEDIFF(SECOND, pd0.Fecha, dc.Fecha)), pd0.Id DESC
) pd
WHERE dc.Fecha >= @Desde
  AND dc.Fecha < DATEADD(DAY, 1, @Hasta)
ORDER BY dc.Fecha DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta.Date;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // ===============================
        // MEMBRESÍA / PLANES (HistorialMembresias)
        // ===============================
        public DataTable ObtenerMembresiaPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
SELECT
    h.Fecha,
    c.Nombre AS Miembro,
    ISNULL(p.Nombre, N'Sin plan') AS [Plan],
    h.TipoMovimiento AS Tipo,
    ISNULL(h.Monto, 0) AS Monto,
    ISNULL(NULLIF(LTRIM(RTRIM(h.Usuario)), ''), N'—') AS Usuario,
    ISNULL(h.Nota, N'') AS Concepto
FROM HistorialMembresias h
INNER JOIN Clientes c ON c.ID = h.ClienteId
LEFT JOIN Planes p ON p.Id = h.PlanId
WHERE h.Fecha >= @Desde
  AND h.Fecha < DATEADD(DAY, 1, @Hasta)
  AND (
        h.PlanId IS NOT NULL
     OR UPPER(LTRIM(RTRIM(h.TipoMovimiento))) IN (
            N'PAGO', N'RENOVACION', N'ALTA_EXISTENTE', N'ALTA',
            N'AJUSTE_FECHA', N'CONGELACION', N'SALIDA', N'BAJA_VENCIDO',
            N'ATLETA', N'VISITA', N'PARCIAL'
        )
  )
ORDER BY h.Fecha DESC, h.Id DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta.Date;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // ===============================
        // GASTOS (EGRESO operativo, sin reversos)
        // ===============================
        public DataTable ObtenerGastosPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
SELECT
    dc.Fecha,
    N'GASTO' AS Tipo,
    dc.Concepto,
    dc.Monto,
    ISNULL(NULLIF(LTRIM(RTRIM(dc.Usuario)), ''), N'—') AS Usuario
FROM DetalleCaja dc
WHERE dc.TipoMovimiento = N'EGRESO'
  AND UPPER(LTRIM(RTRIM(ISNULL(dc.MetodoPago, N'')))) <> N'REVERSO'
  AND UPPER(LTRIM(RTRIM(ISNULL(dc.Concepto, N'')))) NOT LIKE N'REVERSO%'
  AND dc.Fecha >= @Desde
  AND dc.Fecha < DATEADD(DAY, 1, @Hasta)
ORDER BY dc.Fecha DESC, dc.Id DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta.Date;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // ===============================
        // VENTAS: productos vendidos (inventario / DetalleVentas)
        // ===============================
        public DataTable ObtenerVentasPorFecha(DateTime desde, DateTime hasta)
            => ObtenerDetalleVentasPorFecha(desde, hasta, soloSuplemento: false);

        // ===============================
        // SUPLEMENTO: mismas columnas base + Categoria y filtro %SUPLEMENTO%
        // ===============================
        public DataTable ObtenerSuplementosPorFecha(DateTime desde, DateTime hasta)
            => ObtenerDetalleVentasPorFecha(desde, hasta, soloSuplemento: true);

        /// <summary>
        /// Núcleo compartido Ventas/Suplemento. Conserva esquemas distintos:
        /// Ventas sin columna Categoria; Suplemento con Categoria + filtro.
        /// </summary>
        private DataTable ObtenerDetalleVentasPorFecha(
            DateTime desde,
            DateTime hasta,
            bool soloSuplemento)
        {
            string columnaCategoria = soloSuplemento
                ? "    ISNULL(cat.Nombre, N'Suplemento') AS Categoria,\r\n"
                : string.Empty;

            string joinCategoria = soloSuplemento
                ? "INNER JOIN Categorias cat ON cat.Id = pr.IdCategoria\r\n"
                : string.Empty;

            string filtroCategoria = soloSuplemento
                ? "  AND UPPER(LTRIM(RTRIM(ISNULL(cat.Nombre, N'')))) LIKE N'%SUPLEMENTO%'\r\n"
                : string.Empty;

            string query = $@"
SELECT
    v.Fecha,
    v.Id AS VentaId,
    ISNULL(c.Nombre, N'Mostrador') AS Cliente,
{columnaCategoria}    ISNULL(NULLIF(LTRIM(RTRIM(pr.CodigoBarra)), N''), N'—') AS Codigo,
    pr.Nombre AS Producto,
    d.Cantidad,
    d.Precio,
    d.Subtotal AS Monto,
    ISNULL(NULLIF(LTRIM(RTRIM(v.MetodoPago)), N''), N'—') AS [Método de Pago],
    ISNULL(NULLIF(LTRIM(RTRIM(v.Usuario)), N''), N'—') AS Usuario
FROM DetalleVentas d
INNER JOIN Ventas v ON v.Id = d.VentaId
INNER JOIN Productos pr ON pr.Id = d.ProductoId
{joinCategoria}LEFT JOIN Clientes c ON c.ID = v.ClienteId
WHERE v.Fecha >= @Desde
  AND v.Fecha < DATEADD(DAY, 1, @Hasta)
{filtroCategoria}ORDER BY v.Fecha DESC, v.Id DESC, d.Id";

            using SqlConnection conn = db.GetConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde.Date;
            cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta.Date;

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}