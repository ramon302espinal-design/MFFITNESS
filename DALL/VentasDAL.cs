using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DL
{
    public class VentasDAL
    {

        //================================
        // OBTENER DETALLE DE VENTA
        //================================
        public DataTable ObtenerDetalleVenta(int ventaId)
        {
            string query = @"SELECT 
                        p.Nombre AS Producto,
                        d.Cantidad,
                        d.Precio,
                        d.Subtotal
                     FROM DetalleVentas d
                     INNER JOIN Productos p ON p.Id = d.ProductoId
                     WHERE d.VentaId = @VentaId";

            SqlParameter[] parametros =
            {
             new SqlParameter("@VentaId", ventaId)
    };

            return db.ExecuteQuery(query, parametros);
        }

        //================================
        // OBTENER VENTAS 
        //================================
        public DataTable ObtenerVentas()
        {
            string query = @"SELECT 
                        v.Id,
                        v.ClienteId,
                        ISNULL(c.Nombre, 'General') AS Cliente,
                        ISNULL(c.Telefono, '') AS Telefono,
                        v.Fecha,
                        v.Total,
                        v.MontoPagado,
                        v.Saldo,
                        v.MetodoPago,
                        CASE
                            WHEN v.Saldo > 0 THEN 'FINANCIADO'
                            WHEN UPPER(LTRIM(RTRIM(ISNULL(v.MetodoPago, '')))) IN (N'FINANCIADO', N'CREDITO', N'CRÉDITO')
                                 AND ISNULL(v.MontoPagado, 0) < ISNULL(v.Total, 0) THEN 'FINANCIADO'
                            ELSE 'CONTADO'
                        END AS TipoOperacion,
                        CASE
                            WHEN v.Saldo > 0 AND v.MontoPagado > 0 THEN
                                CONCAT('Financiado · Pago inicial RD$ ', FORMAT(v.MontoPagado, 'N2'),
                                       ' · Saldo RD$ ', FORMAT(v.Saldo, 'N2'))
                            WHEN v.Saldo > 0 THEN
                                CONCAT('Financiado · Saldo RD$ ', FORMAT(v.Saldo, 'N2'))
                            WHEN UPPER(LTRIM(RTRIM(ISNULL(v.MetodoPago, '')))) IN (N'FINANCIADO', N'CREDITO', N'CRÉDITO')
                                 AND ISNULL(v.MontoPagado, 0) < ISNULL(v.Total, 0) THEN
                                CONCAT('Financiado (legacy) · Pago inicial RD$ ', FORMAT(v.MontoPagado, 'N2'),
                                       ' · Saldo RD$ ', FORMAT(v.Total - v.MontoPagado, 'N2'))
                            ELSE v.MetodoPago
                        END AS FormaPago,
                        v.Usuario,
                        ISNULL(prod.Productos, '') AS Productos
                     FROM Ventas v
                     LEFT JOIN Clientes c ON c.Id = v.ClienteId
                     OUTER APPLY (
                         SELECT STRING_AGG(p.Nombre, ', ') WITHIN GROUP (ORDER BY p.Nombre) AS Productos
                         FROM DetalleVentas d
                         INNER JOIN Productos p ON p.Id = d.ProductoId
                         WHERE d.VentaId = v.Id
                     ) prod
                     ORDER BY v.Fecha DESC";

            return db.ExecuteQuery(query);
        }


        //================================
        // REGISTRAR DETALLE DE VENTA
        //================================
        public void RegistrarDetalleVenta(
            int ventaId,
            int productoId,
            int cantidad,
            decimal precio,
            decimal subtotal,
            decimal? costoUnitario = null)
        {
            string query = @"INSERT INTO DetalleVentas
                     (VentaId, ProductoId, Cantidad, Precio, Subtotal, CostoUnitario)
                     VALUES
                     (@VentaId, @ProductoId, @Cantidad, @Precio, @Subtotal, @CostoUnitario)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@VentaId", ventaId),
                new SqlParameter("@ProductoId", productoId),
                new SqlParameter("@Cantidad", cantidad),
                new SqlParameter("@Precio", precio),
                new SqlParameter("@Subtotal", subtotal),
                new SqlParameter("@CostoUnitario", (object?)costoUnitario ?? DBNull.Value)
            };

            db.ExecuteNonQuery(query, parametros);
        }
        private readonly DBHelper db = new DBHelper();


        //================================
        // REGISTRAR VENTA
        //================================
        public int RegistrarVenta(int? clienteId, decimal total, string metodo, string usuario)
        {
            // Por defecto, ventas sin parámetro de montoPagado se consideran pagadas completas
            return RegistrarVenta(clienteId, total, total, metodo, usuario);
        }

        //================================
        // REGISTRAR VENTA CON MONTO PAGADO (SOPORTA CRÉDITO)
        //================================
        public int RegistrarVenta(int? clienteId, decimal total, decimal montoPagado, string metodo, string usuario)
        {
            string query = @"INSERT INTO Ventas
                     (ClienteId, Total, MontoPagado, MetodoPago, Usuario, Fecha)
                     OUTPUT INSERTED.Id
                     VALUES
                     (@ClienteId, @Total, @MontoPagado, @MetodoPago, @Usuario, @Fecha)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@ClienteId", (object?)clienteId ?? DBNull.Value),
                new SqlParameter("@Total", total),
                new SqlParameter("@MontoPagado", montoPagado),
                new SqlParameter("@MetodoPago", metodo),
                new SqlParameter("@Usuario", usuario),
                new SqlParameter("@Fecha", DateTime.Now)
            };

            object? result = db.ExecuteScalar(query, parametros);

            return Convert.ToInt32(result);
        }

        public int RegistrarVenta(
            SqlConnection conn,
            SqlTransaction tx,
            int? clienteId,
            decimal total,
            decimal montoPagado,
            string metodo,
            string usuario)
        {
            using SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Ventas
                (ClienteId, Total, MontoPagado, MetodoPago, Usuario, Fecha)
                OUTPUT INSERTED.Id
                VALUES
                (@ClienteId, @Total, @MontoPagado, @MetodoPago, @Usuario, @Fecha)", conn, tx);
            cmd.Parameters.AddWithValue("@ClienteId", (object?)clienteId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Total", total);
            cmd.Parameters.AddWithValue("@MontoPagado", montoPagado);
            cmd.Parameters.AddWithValue("@MetodoPago", metodo);
            cmd.Parameters.AddWithValue("@Usuario", usuario);
            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void RegistrarDetalleVenta(
            SqlConnection conn,
            SqlTransaction tx,
            int ventaId,
            int productoId,
            int cantidad,
            decimal precio,
            decimal subtotal,
            decimal? costoUnitario = null)
        {
            using SqlCommand cmd = new SqlCommand(@"
                INSERT INTO DetalleVentas
                (VentaId, ProductoId, Cantidad, Precio, Subtotal, CostoUnitario)
                VALUES
                (@VentaId, @ProductoId, @Cantidad, @Precio, @Subtotal, @CostoUnitario)", conn, tx);
            cmd.Parameters.AddWithValue("@VentaId", ventaId);
            cmd.Parameters.AddWithValue("@ProductoId", productoId);
            cmd.Parameters.AddWithValue("@Cantidad", cantidad);
            cmd.Parameters.AddWithValue("@Precio", precio);
            cmd.Parameters.AddWithValue("@Subtotal", subtotal);
            cmd.Parameters.AddWithValue("@CostoUnitario", (object?)costoUnitario ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void AnularVenta(int ventaId)
        {
            SqlParameter[] p = { new SqlParameter("@VentaId", ventaId) };

            db.ExecuteNonQuery("DELETE FROM DetalleVentas WHERE VentaId = @VentaId", p);
            db.ExecuteNonQuery("DELETE FROM Ventas WHERE Id = @VentaId", p);
        }

        public void AnularVenta(SqlConnection conn, SqlTransaction tx, int ventaId)
        {
            using SqlCommand cmdDet = new SqlCommand(
                "DELETE FROM DetalleVentas WHERE VentaId = @VentaId", conn, tx);
            cmdDet.Parameters.AddWithValue("@VentaId", ventaId);
            cmdDet.ExecuteNonQuery();

            using SqlCommand cmdV = new SqlCommand(
                "DELETE FROM Ventas WHERE Id = @VentaId", conn, tx);
            cmdV.Parameters.AddWithValue("@VentaId", ventaId);
            cmdV.ExecuteNonQuery();
        }

        [Obsolete("Stub legacy — usar overload con conn/tx.")]
        public void RegistrarVenta(SqlConnection conn, SqlTransaction tx,
           int? clienteId, decimal total, string metodoPago, string usuario)
        {
            string query = "INSERT INTO Ventas (...) VALUES (...)";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                // parámetros
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Tras pagar o revertir una deuda de venta a crédito, alinea Ventas.MontoPagado
        /// (Saldo = Total - MontoPagado) con el saldo vigente de la deuda.
        /// </summary>
        public void SincronizarMontoPagadoDesdeDeuda(SqlConnection conn, SqlTransaction tx, int deudaId)
        {
            using var cmdGet = new SqlCommand(@"
                SELECT ClienteId, Saldo, Concepto, MontoTotal, FechaCreacion
                FROM Deudas
                WHERE Id = @DeudaId", conn, tx);
            cmdGet.Parameters.AddWithValue("@DeudaId", deudaId);

            using var reader = cmdGet.ExecuteReader();
            if (!reader.Read())
                return;

            int clienteId = Convert.ToInt32(reader["ClienteId"]);
            decimal saldoDeuda = Convert.ToDecimal(reader["Saldo"]);
            string concepto = reader["Concepto"]?.ToString() ?? string.Empty;
            decimal montoTotal = Convert.ToDecimal(reader["MontoTotal"]);
            DateTime fechaCreacion = reader["FechaCreacion"] == DBNull.Value
                ? DateTime.Now
                : Convert.ToDateTime(reader["FechaCreacion"]);
            reader.Close();

            if (!EsConceptoDeudaVentaProducto(concepto))
                return;

            int? ventaId = TryExtraerVentaIdDeConcepto(concepto);

            using var cmdUpd = new SqlCommand(@"
                SET QUOTED_IDENTIFIER ON;
                UPDATE v
                SET v.MontoPagado = v.Total - @SaldoDeuda
                FROM Ventas v
                WHERE v.ClienteId = @ClienteId
                  AND v.Total >= @SaldoDeuda
                  AND (
                        (@VentaId IS NOT NULL AND v.Id = @VentaId)
                     OR (@VentaId IS NULL AND v.Id = (
                            SELECT TOP 1 v2.Id
                            FROM Ventas v2
                            WHERE v2.ClienteId = @ClienteId
                              AND v2.Total >= @MontoTotal
                            ORDER BY
                              CASE WHEN v2.MontoPagado < v2.Total OR @SaldoDeuda = 0 THEN 0 ELSE 1 END,
                              ABS(DATEDIFF(SECOND, v2.Fecha, @FechaCreacion)),
                              v2.Id DESC
                        ))
                  )", conn, tx);

            cmdUpd.Parameters.AddWithValue("@SaldoDeuda", saldoDeuda);
            cmdUpd.Parameters.AddWithValue("@ClienteId", clienteId);
            cmdUpd.Parameters.AddWithValue("@MontoTotal", montoTotal);
            cmdUpd.Parameters.AddWithValue("@FechaCreacion", fechaCreacion);
            cmdUpd.Parameters.AddWithValue("@VentaId", (object?)ventaId ?? DBNull.Value);
            cmdUpd.ExecuteNonQuery();
        }

        public static bool EsConceptoDeudaVentaProducto(string? concepto)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                return false;

            string c = concepto.Trim();
            if (TryExtraerVentaIdDeConcepto(c).HasValue)
                return true;

            return c.Contains("Venta a cr", StringComparison.OrdinalIgnoreCase)
                || c.StartsWith("Venta de productos", StringComparison.OrdinalIgnoreCase)
                || c.EndsWith(" a credito", StringComparison.OrdinalIgnoreCase);
        }

        public static int? TryExtraerVentaIdDeConcepto(string? concepto)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                return null;

            Match match = Regex.Match(
                concepto,
                @"\((?:Venta\s+)?Id\s+(\d+)\)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!match.Success)
                return null;

            return int.TryParse(match.Groups[1].Value, out int ventaId) && ventaId > 0
                ? ventaId
                : null;
        }

        /// <summary>Totales de venta por Id (precio legítimo de producto financiado).</summary>
        public Dictionary<int, decimal> ObtenerTotalesPorIds(IEnumerable<int> ventaIds)
        {
            var resultado = new Dictionary<int, decimal>();
            var ids = ventaIds?.Distinct().Where(id => id > 0).ToList();
            if (ids == null || ids.Count == 0)
                return resultado;

            string inClause = string.Join(",", ids);
            string query = $"SELECT Id, Total FROM Ventas WHERE Id IN ({inClause})";
            DataTable dt = db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
                resultado[Convert.ToInt32(row["Id"])] = Convert.ToDecimal(row["Total"]);

            return resultado;
        }
    }
}