using CORE;
using CORE.Commands;
using DTO;

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

        public static CommandResult ActualizarDeudaFinanciamiento(
            int deudaId,
            string concepto,
            decimal totalFinanciado,
            decimal pagoInicial,
            DateTime vencimiento,
            int? planId,
            string metodoPago = "Efectivo",
            string? usuario = null)
        {
            try
            {
                var bll = new DeudaBLL();
                var edicion = bll.ActualizarDeudaFinanciamiento(
                    deudaId,
                    concepto,
                    totalFinanciado,
                    pagoInicial,
                    vencimiento,
                    planId,
                    metodoPago,
                    ResolveUsuario(usuario));

                return CommandResult.Ok(ConstruirMensaje(edicion), edicion);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        private static string ConstruirMensaje(EdicionDeudaDTO edicion)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine(edicion.Saldo > 0
                ? $"Deuda actualizada. Saldo pendiente: RD$ {edicion.Saldo:N2}."
                : "Deuda actualizada y saldada por completo.");

            if (edicion.PagoInicialAnterior != edicion.PagoInicialNuevo)
            {
                sb.AppendLine();
                sb.AppendLine($"Pago inicial: RD$ {edicion.PagoInicialAnterior:N2} → RD$ {edicion.PagoInicialNuevo:N2}");

                if (edicion.ReversoCaja)
                    sb.AppendLine($"Reverso en caja: -RD$ {edicion.PagoInicialAnterior:N2}");

                if (edicion.IngresoCaja)
                    sb.AppendLine($"Ingreso en caja: +RD$ {edicion.PagoInicialNuevo:N2}");
            }

            return sb.ToString().TrimEnd();
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
