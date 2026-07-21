using Microsoft.Data.SqlClient;
using System;
using System.Data;

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
                        ISNULL(c.Nombre, 'General') AS Cliente,
                        v.Fecha,
                        v.Total,
                        v.MetodoPago,
                        v.Usuario
                     FROM Ventas v
                     LEFT JOIN Clientes c ON c.Id = v.ClienteId
                     ORDER BY v.Fecha DESC";

            return db.ExecuteQuery(query);
        }


        //================================
        // REGISTRAR DETALLE DE VENTA
        //================================
        public void RegistrarDetalleVenta(int ventaId, int productoId, int cantidad, decimal precio, decimal subtotal)
        {
            string query = @"INSERT INTO DetalleVentas
                     (VentaId, ProductoId, Cantidad, Precio, Subtotal)
                     VALUES
                     (@VentaId, @ProductoId, @Cantidad, @Precio, @Subtotal)";

            SqlParameter[] parametros =
            {
        new SqlParameter("@VentaId", ventaId),
        new SqlParameter("@ProductoId", productoId),
        new SqlParameter("@Cantidad", cantidad),
        new SqlParameter("@Precio", precio),
        new SqlParameter("@Subtotal", subtotal)
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

        public void AnularVenta(int ventaId)
        {
            SqlParameter[] p = { new SqlParameter("@VentaId", ventaId) };

            db.ExecuteNonQuery("DELETE FROM DetalleVentas WHERE VentaId = @VentaId", p);
            db.ExecuteNonQuery("DELETE FROM Ventas WHERE Id = @VentaId", p);
        }

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
    }
}