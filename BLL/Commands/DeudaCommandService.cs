using CORE;
using CORE.Commands;

namespace BLL.Commands
{
    public static class DeudaCommandService
    {
        public static CommandResult RegistrarPago(
            int deudaId,
            decimal monto,
            string metodo,
            string? usuario = null)
        {
            try
            {
                var bll = new DeudaBLL();
                int pagoId = bll.RegistrarPagoConId(deudaId, monto, metodo, ResolveUsuario(usuario));
                return CommandResult.Ok("Pago registrado correctamente.", pagoId);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult CrearDeuda(
            int clienteId,
            string concepto,
            decimal monto,
            DateTime vencimiento,
            string? usuario = null)
        {
            try
            {
                var bll = new DeudaBLL();
                int deudaId = bll.CrearDeudaConId(
                    clienteId, concepto, monto, vencimiento, ResolveUsuario(usuario));
                return CommandResult.Ok("Deuda creada correctamente.", deudaId);
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
