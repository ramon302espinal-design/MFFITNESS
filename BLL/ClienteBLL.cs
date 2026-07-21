using System;
using System.Collections.Generic;
using System.Data;
using BLL.Models;
using DL;
using DTO;

namespace BLL
{
    public class ClienteBLL
    {
        private readonly ClienteDAL clienteDAL = new ClienteDAL();
        private readonly ClienteFichaSaludDAL fichaDAL = new ClienteFichaSaludDAL();

        public DataTable ObtenerClientes()
        {
            return clienteDAL.ListarClientes();
        }

        public int AgregarConId(string nombre, DateTime fechaNacimiento,
                            string direccion, string telefono)
        {
            return clienteDAL.InsertarCliente(nombre, fechaNacimiento, direccion, telefono);
        }

        /// <summary>
        /// Alta completa: cliente + ficha de salud/emergencia.
        /// </summary>
        public int AgregarConFicha(
            string nombre,
            DateTime fechaNacimiento,
            string direccion,
            string telefono,
            ClienteFichaSaludDTO ficha)
        {
            if (ficha == null)
                throw new ArgumentNullException(nameof(ficha));

            if (!ClienteFichaSaludValidator.Validar(ficha, out string errorFicha))
                throw new Exception(errorFicha);

            int id = clienteDAL.InsertarCliente(nombre, fechaNacimiento, direccion, telefono);
            try
            {
                ficha.ClienteId = id;
                fichaDAL.Guardar(ficha);
            }
            catch
            {
                try { clienteDAL.EliminarCliente(id); } catch { /* best-effort rollback */ }
                throw;
            }
            return id;
        }

        public void Editar(int id, string nombre, DateTime fechaNacimiento,
                           string direccion, string telefono)
        {
            clienteDAL.ActualizarCliente(id, nombre, fechaNacimiento, direccion, telefono);
        }

        /// <summary>
        /// Actualiza datos del cliente y guarda/merge de la ficha de salud.
        /// </summary>
        public void EditarConFicha(
            int id,
            string nombre,
            DateTime fechaNacimiento,
            string direccion,
            string telefono,
            ClienteFichaSaludDTO ficha)
        {
            if (id <= 0)
                throw new Exception("Cliente inválido.");
            if (ficha == null)
                throw new ArgumentNullException(nameof(ficha));

            if (!ClienteFichaSaludValidator.Validar(ficha, out string errorFicha))
                throw new Exception(errorFicha);

            clienteDAL.ActualizarCliente(id, nombre, fechaNacimiento, direccion, telefono);
            ficha.ClienteId = id;
            fichaDAL.Guardar(ficha);
        }

        public void Eliminar(int id)
        {
            clienteDAL.EliminarCliente(id);
        }

        public DataRow? ObtenerPorId(int id)
        {
            return clienteDAL.ObtenerClientePorId(id);
        }

        public ClienteFichaSaludDTO? ObtenerFichaSalud(int clienteId) =>
            clienteId > 0 ? fichaDAL.ObtenerPorClienteId(clienteId) : null;

        /// <summary>
        /// Valida campos críticos del perfil antes de procesar pagos.
        /// </summary>
        public ClientePerfilResult ValidarPerfilCompleto(int clienteId)
        {
            var faltantes = new List<string>();
            var row = clienteDAL.ObtenerClientePorId(clienteId);

            if (row == null)
            {
                return new ClientePerfilResult
                {
                    EsCompleto = false,
                    CamposFaltantes = new[] { "Cliente no encontrado" }
                };
            }

            if (string.IsNullOrWhiteSpace(row["Nombre"]?.ToString()))
                faltantes.Add("Nombre");

            if (string.IsNullOrWhiteSpace(row["Telefono"]?.ToString()))
                faltantes.Add("Teléfono");

            if (string.IsNullOrWhiteSpace(row["Direccion"]?.ToString()))
                faltantes.Add("Dirección");

            if (row["FechaNacimiento"] == DBNull.Value ||
                !DateTime.TryParse(row["FechaNacimiento"]?.ToString(), out DateTime fechaNac) ||
                fechaNac.Year < 1900 ||
                fechaNac.Date > DateTime.Today)
            {
                faltantes.Add("Fecha de nacimiento");
            }

            return new ClientePerfilResult
            {
                EsCompleto = faltantes.Count == 0,
                CamposFaltantes = faltantes
            };
        }
    }
}
