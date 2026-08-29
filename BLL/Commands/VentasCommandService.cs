using CORE;
using CORE.Commands;
using BLL.Models;
using System.Data;

namespace BLL.Commands
{
    public static class VentasCommandService
    {
        public static CommandResult RegistrarVentaPOS(
            int? clienteId,
            decimal total,
            decimal montoPagado,
            string metodo,
            DataTable carrito,
            string? usuario = null,
            DateTime? fechaVencimientoDeuda = null,
            string? conceptoDeuda = null)
        {
            try
            {
                var bll = new VentasBLL();
                var operacion = bll.RegistrarVentaCompletaConResultado(
                    clienteId,
                    total,
                    montoPagado,
                    metodo,
                    ResolveUsuario(usuario),
                    carrito,
                    fechaVencimientoDeuda,
                    conceptoDeuda);

                NotificarEventosPostVentaProducto(operacion);

                return CommandResult.Ok("Venta registrada correctamente.", operacion.VentaId);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        /// <summary>Despacho de saldo a favor: venta + stock sin caja (única entrada POS).</summary>
        public static CommandResult RegistrarDespachoSaldoAFavor(int saldoClienteId, string? usuario = null)
        {
            try
            {
                var bll = new SaldoClienteBLL();
                VentaOperacionResult operacion = bll.DespacharSaldo(
                    saldoClienteId,
                    ResolveUsuario(usuario));

                MovimientoFinancieroNotifier.VentaSinCaja();

                return CommandResult.Ok(
                    "Despacho registrado correctamente.",
                    operacion.VentaId);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Sincroniza historial, caja, deudas y dashboards que escuchan AppEventos.
        /// </summary>
        private static void NotificarEventosPostVentaProducto(VentaOperacionResult operacion)
        {
            MovimientoFinancieroNotifier.VentaProducto(operacion.CajaMovimientoId, operacion.DeudaId);
        }

        private static string ResolveUsuario(string? usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario))
                return usuario;
            if (!string.IsNullOrWhiteSpace(Sesion.Usuario))
                return Sesion.Usuario;
            return "ADMIN";
        }
    }
}
