using CORE;
using CORE.Commands;
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
                return CommandResult.Ok("Venta registrada correctamente.", operacion.VentaId);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
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
