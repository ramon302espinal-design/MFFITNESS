using CORE.Commands;
using DTO;

namespace BLL.Commands
{
    public static class ClienteCommandService
    {
        public static CommandResult Agregar(string nombre, DateTime fechaNacimiento, string direccion, string telefono)
        {
            try
            {
                var bll = new ClienteBLL();
                int id = bll.AgregarConId(nombre.Trim(), fechaNacimiento, direccion.Trim(), telefono.Trim());
                return CommandResult.Ok("Cliente agregado correctamente.", id);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult AgregarConFicha(
            string nombre,
            DateTime fechaNacimiento,
            string direccion,
            string telefono,
            string? sexo,
            ClienteFichaSaludDTO ficha)
        {
            try
            {
                var bll = new ClienteBLL();
                int id = bll.AgregarConFicha(
                    nombre.Trim(),
                    fechaNacimiento,
                    direccion.Trim(),
                    telefono.Trim(),
                    string.IsNullOrWhiteSpace(sexo) ? null : sexo.Trim(),
                    ficha);
                return CommandResult.Ok("Cliente agregado correctamente.", id);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult Editar(int id, string nombre, DateTime fechaNacimiento, string direccion, string telefono, string? sexo = null)
        {
            try
            {
                var bll = new ClienteBLL();
                bll.Editar(
                    id,
                    nombre.Trim(),
                    fechaNacimiento.Date,
                    direccion.Trim(),
                    telefono.Trim(),
                    string.IsNullOrWhiteSpace(sexo) ? null : sexo.Trim());
                return CommandResult.Ok("Cliente actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult EditarConFicha(
            int id,
            string nombre,
            DateTime fechaNacimiento,
            string direccion,
            string telefono,
            string? sexo,
            ClienteFichaSaludDTO ficha)
        {
            try
            {
                var bll = new ClienteBLL();
                bll.EditarConFicha(
                    id,
                    nombre.Trim(),
                    fechaNacimiento.Date,
                    direccion.Trim(),
                    telefono.Trim(),
                    string.IsNullOrWhiteSpace(sexo) ? null : sexo.Trim(),
                    ficha);
                return CommandResult.Ok("Cliente y ficha actualizados correctamente.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult Eliminar(int id)
        {
            try
            {
                var bll = new ClienteBLL();
                bll.Eliminar(id);
                return CommandResult.Ok("Cliente eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}
