using CORE;
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
                AppEventos.ClienteCatalogoCambiado();
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
            ClienteFichaSaludDTO ficha,
            string? cedula = null)
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
                    ficha,
                    NormalizarCedula(cedula));
                AppEventos.ClienteCatalogoCambiado();
                return CommandResult.Ok("Cliente agregado correctamente.", id);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult Editar(int id, string nombre, DateTime fechaNacimiento, string direccion, string telefono, string? sexo = null, string? cedula = null)
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
                    string.IsNullOrWhiteSpace(sexo) ? null : sexo.Trim(),
                    NormalizarCedula(cedula));
                AppEventos.ClienteCatalogoCambiado();
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
            ClienteFichaSaludDTO ficha,
            string? cedula = null)
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
                    ficha,
                    NormalizarCedula(cedula));
                AppEventos.ClienteCatalogoCambiado();
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
                AppEventos.ClienteCatalogoCambiado();
                return CommandResult.Ok("Cliente eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        /// <summary>Cédula + trabajo desde tabCrear (no toca ficha de salud ni tel/dirección).</summary>
        public static CommandResult ActualizarCedulaYTrabajo(
            int id,
            string? cedula,
            string? lugarTrabajo,
            string? direccionTrabajo)
        {
            try
            {
                var bll = new ClienteBLL();
                bll.ActualizarCedulaYTrabajo(id, cedula, lugarTrabajo, direccionTrabajo);
                AppEventos.ClienteCatalogoCambiado();
                return CommandResult.Ok("Datos de cédula/trabajo actualizados.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        private static string? NormalizarCedula(string? cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return null;

            string valor = cedula.Trim();
            return valor.Length == 0 ? null : valor;
        }
    }
}
