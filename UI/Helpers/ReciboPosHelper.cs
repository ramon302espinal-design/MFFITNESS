using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using BLL.Models;
using UI;

namespace UI.Helpers
{
    /// <summary>
    /// Genera y muestra recibos POS en FrmVistaPrevia.
    /// </summary>
    public static class ReciboPosHelper
    {
        public static void MostrarVenta(
            IWin32Window? owner,
            SolicitudPagoDTO pago,
            DataTable carrito,
            string? clienteNombre,
            string usuario)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========== MFFITNESS ==========");
            sb.AppendLine("RECIBO DE VENTA");
            sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Usuario: {usuario}");
            sb.AppendLine($"Cliente: {clienteNombre ?? "Consumidor final"}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine("DETALLE");
            sb.AppendLine("--------------------------------");

            foreach (DataRow row in carrito.Rows)
            {
                string producto = row["Producto"]?.ToString() ?? "-";
                int cantidad = Convert.ToInt32(row["Cantidad"]);
                decimal totalLinea = Convert.ToDecimal(row["Total"]);
                sb.AppendLine($"{cantidad} x {producto}");
                sb.AppendLine($"    {totalLinea:N2}");
            }

            sb.AppendLine("--------------------------------");
            sb.AppendLine($"TOTAL:      RD$ {pago.TotalAPagar:N2}");
            sb.AppendLine($"RECIBIDO:   RD$ {pago.MontoRecibido:N2}");
            sb.AppendLine($"CAMBIO:     RD$ {pago.Cambio:N2}");
            sb.AppendLine($"MÉTODO:     {pago.MetodoSeleccionado.ToMetodoBd()}");
            sb.AppendLine("================================");
            sb.AppendLine("Gracias por su compra.");

            MostrarRecibo(owner, sb.ToString());
        }

        public static void MostrarMembresia(
            IWin32Window? owner,
            SolicitudPagoDTO pago,
            string clienteNombre,
            string planNombre,
            string concepto,
            string usuario)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========== MFFITNESS ==========");
            sb.AppendLine("RECIBO DE MEMBRESÍA");
            sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Usuario: {usuario}");
            sb.AppendLine($"Cliente: {clienteNombre}");
            sb.AppendLine($"Plan: {planNombre}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine(concepto);
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"TOTAL:      RD$ {pago.TotalAPagar:N2}");
            sb.AppendLine($"RECIBIDO:   RD$ {pago.MontoRecibido:N2}");

            if (pago.MetodoSeleccionado == MetodoPagoPOS.Efectivo)
                sb.AppendLine($"CAMBIO:     RD$ {pago.Cambio:N2}");

            sb.AppendLine($"MÉTODO:     {pago.MetodoSeleccionado.ToMetodoBd()}");
            sb.AppendLine("================================");
            sb.AppendLine("Gracias por su preferencia.");

            MostrarRecibo(owner, sb.ToString());
        }

        private static void MostrarRecibo(IWin32Window? owner, string contenido)
        {
            using var frm = new FrmVistaPrevia(contenido);
            frm.ShowDialog(owner);
        }
    }
}
