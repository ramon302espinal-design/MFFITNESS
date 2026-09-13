using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using BLL.Models;
using CORE;
using UI;

namespace UI.Helpers
{
    /// <summary>
    /// Genera y muestra recibos POS en FrmVistaPrevia.
    /// </summary>
    public static class ReciboPosHelper
    {
        public enum TipoComprobanteAbono
        {
            AbonoDeuda,
            CuotaCompleta,
            AbonoParcial
        }

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
            sb.AppendLine($"Fecha: {DateTime.Now.ToString(FechaHoraFormats.FechaHora)}");
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
            sb.AppendLine($"Fecha: {DateTime.Now.ToString(FechaHoraFormats.FechaHora)}");
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

        /// <summary>
        /// Comprobante de abono desde FrmDeudas: distingue cuota completa vs parcial.
        /// </summary>
        public static void MostrarAbonoDeuda(
            IWin32Window? owner,
            TipoComprobanteAbono tipo,
            string clienteNombre,
            int deudaId,
            string concepto,
            decimal montoAbonado,
            string metodo,
            decimal saldoAnterior,
            decimal saldoActual,
            string usuario,
            int? cuotaNumero = null,
            DateTime? cuotaFecha = null,
            decimal? faltaAntes = null,
            decimal? faltaDespues = null,
            int? proximaCuotaNumero = null,
            DateTime? proximaCuotaFecha = null,
            bool deudaLiquidada = false)
        {
            string titulo = tipo switch
            {
                TipoComprobanteAbono.CuotaCompleta => "COMPROBANTE — CUOTA COMPLETA",
                TipoComprobanteAbono.AbonoParcial => "COMPROBANTE — ABONO PARCIAL",
                _ => "COMPROBANTE — ABONO A DEUDA"
            };

            var sb = new StringBuilder();
            sb.AppendLine("========== MFFITNESS ==========");
            sb.AppendLine(titulo);
            sb.AppendLine($"Fecha: {DateTime.Now.ToString(FechaHoraFormats.FechaHora)}");
            sb.AppendLine($"Usuario: {usuario}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Cliente: {clienteNombre}");
            sb.AppendLine($"Deuda #: {deudaId}");
            sb.AppendLine($"Concepto: {concepto}");
            sb.AppendLine("--------------------------------");

            if (tipo is TipoComprobanteAbono.CuotaCompleta or TipoComprobanteAbono.AbonoParcial)
            {
                sb.AppendLine($"Cuota #: {cuotaNumero?.ToString() ?? "-"}");
                sb.AppendLine($"Vence cuota: {(cuotaFecha.HasValue ? cuotaFecha.Value.ToString("dd/MM/yyyy") : "-")}");
                if (faltaAntes.HasValue)
                    sb.AppendLine($"Cuota / faltaba: RD$ {faltaAntes.Value:N2}");
                sb.AppendLine($"Abonado ahora:   RD$ {montoAbonado:N2}");

                if (tipo == TipoComprobanteAbono.AbonoParcial)
                {
                    decimal aunFalta = faltaDespues
                        ?? (faltaAntes.HasValue
                            ? Math.Max(0m, faltaAntes.Value - montoAbonado)
                            : 0m);
                    sb.AppendLine($"Aún falta cuota: RD$ {aunFalta:N2}");
                    sb.AppendLine("Estado cuota:    PARCIAL");
                }
                else
                {
                    sb.AppendLine("Estado cuota:    COMPLETA");
                    if (!deudaLiquidada)
                    {
                        if (proximaCuotaNumero.HasValue && proximaCuotaFecha.HasValue)
                            sb.AppendLine(
                                $"Próx. cuota:     #{proximaCuotaNumero} — {proximaCuotaFecha:dd/MM/yyyy}");
                        else if (proximaCuotaNumero.HasValue)
                            sb.AppendLine($"Próx. cuota:     #{proximaCuotaNumero}");
                    }
                }

                sb.AppendLine("--------------------------------");
            }

            sb.AppendLine($"Saldo anterior:  RD$ {saldoAnterior:N2}");
            sb.AppendLine($"Saldo actual:    RD$ {saldoActual:N2}");
            sb.AppendLine($"Método:          {metodo}");

            if (deudaLiquidada)
            {
                sb.AppendLine("--------------------------------");
                sb.AppendLine("DEUDA LIQUIDADA");
            }

            sb.AppendLine("================================");
            sb.AppendLine(tipo == TipoComprobanteAbono.AbonoParcial
                ? "Conserve este comprobante."
                : "Gracias por su pago.");

            MostrarRecibo(owner, sb.ToString());
        }

        private static void MostrarRecibo(IWin32Window? owner, string contenido)
        {
            using var frm = new FrmVistaPrevia(contenido);
            frm.ShowDialog(owner);
        }
    }
}
