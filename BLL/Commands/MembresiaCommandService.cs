using CORE;
using CORE.Commands;

namespace BLL.Commands
{
    public static class MembresiaCommandService
    {
        public static CommandResult PagarMembresia(
            int clienteId,
            int planId,
            decimal monto,
            string metodo,
            string concepto,
            DateTime fechaVencimiento,
            string? usuario = null)
        {
            try
            {
                var bll = new MembresiaBLL();
                var operacion = bll.PagarMembresiaCompleta(
                    clienteId,
                    planId,
                    monto,
                    fechaVencimiento,
                    metodo,
                    concepto,
                    ResolveUsuario(usuario));
                return CommandResult.Ok("Membresía registrada correctamente.", operacion);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult VenderMembresiaFinanciada(
            int clienteId,
            int planId,
            decimal pagoInicial,
            string metodo,
            string conceptoPago,
            DateTime? fechaVencimientoDeuda = null,
            string? usuario = null)
        {
            try
            {
                var bll = new MembresiaBLL();
                var operacion = bll.VenderMembresiaFinanciadaConResultado(
                    clienteId,
                    planId,
                    pagoInicial,
                    metodo,
                    conceptoPago,
                    ResolveUsuario(usuario),
                    fechaVencimientoDeuda);
                return CommandResult.Ok("Membresía financiada registrada.", operacion);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult RenovarMembresia(
            int clienteId,
            int planId,
            decimal precio,
            string? usuario = null)
            => RenovarMembresia(clienteId, planId, precio, concepto: null, usuario);

        public static CommandResult RenovarMembresia(
            int clienteId,
            int planId,
            decimal precio,
            string? concepto,
            string? usuario = null)
        {
            try
            {
                var bll = new RenovacionBLL();
                var operacion = bll.RenovarClienteConResultado(
                    clienteId, planId, precio, ResolveUsuario(usuario), concepto);
                return CommandResult.Ok("Renovación registrada correctamente.", operacion);
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
