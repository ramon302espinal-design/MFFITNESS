using CORE;
using CORE.Commands;

namespace BLL.Commands
{
    public static class CajaCommandService
    {
        public static CommandResult RegistrarIngreso(
            string concepto,
            decimal monto,
            string? usuario = null)
        {
            try
            {
                var bll = new CajaBLL();
                int id = bll.RegistrarIngresoConId(concepto.Trim(), monto, ResolveUsuario(usuario));
                return CommandResult.Ok("Movimiento registrado correctamente.", id);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult RegistrarGasto(
            string concepto,
            decimal monto,
            string? usuario = null)
        {
            try
            {
                var bll = new CajaBLL();
                int id = bll.RegistrarEgresoConId(concepto.Trim(), monto, ResolveUsuario(usuario));
                return CommandResult.Ok("Movimiento registrado correctamente.", id);
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
