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
        // 🔥 REPORTE CAJA POR FECHA
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

                cmd.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ===============================
        // 🔥 REPORTE PAGOS
        // ===============================
        public DataTable ObtenerPagosPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT FechaPago, ClienteId, Monto, MetodoPago, Concepto 
                                 FROM Pagos
                                 WHERE FechaPago BETWEEN @Desde AND @Hasta
                                 ORDER BY FechaPago DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Desde", desde);
                cmd.Parameters.AddWithValue("@Hasta", hasta);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ===============================
        // 🔥 REPORTE VENTAS
        // ===============================
        public DataTable ObtenerVentasPorFecha(DateTime desde, DateTime hasta)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT Fecha, ClienteId, Total, MetodoPago 
                                 FROM Ventas
                                 WHERE Fecha BETWEEN @Desde AND @Hasta
                                 ORDER BY Fecha DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Desde", desde);
                cmd.Parameters.AddWithValue("@Hasta", hasta);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }
    }
}