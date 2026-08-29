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

        /// <summary>Catálogo de clientes con Estado SSOT (misma regla que dgvEstado).</summary>
        public DataTable ObtenerClientes()
        {
            return clienteDAL.ListarClientes();
        }

        /// <summary>Catálogo de cobro (excluye VISITANTE sistema).</summary>
        public DataTable ObtenerClientesParaPos()
        {
            return clienteDAL.ListarClientesParaPos();
        }

        /// <summary>Miembros registrados (con Membresias); excluye SOLO CLIENTE.</summary>
        public DataTable ObtenerMiembrosRegistradosParaPos()
        {
            return clienteDAL.ListarMiembrosRegistradosParaPos();
        }

        public bool EsMiembroRegistrado(int clienteId) =>
            clienteDAL.EsMiembroRegistrado(clienteId);

        /// <summary>Cliente técnico para ATLETA/VISITA sin miembro en combo.</summary>
        public int ObtenerOCrearVisitanteSistema()
        {
            return clienteDAL.ObtenerOCrearVisitanteSistema();
        }

        /// <summary>Clientes elegibles para alta sin cobro (excluye ACTIVO y ACTIVO Y PROGRAMADO).</summary>
        public DataTable ObtenerClientesNoActivos()
        {
            return clienteDAL.ListarClientesNoActivos();
        }

        public int AgregarConId(string nombre, DateTime fechaNacimiento,
                            string direccion, string telefono, string? sexo = null)
        {
            return clienteDAL.InsertarCliente(nombre, fechaNacimiento, direccion, telefono, sexo);
        }

        /// <summary>
        /// Alta completa: cliente + ficha de salud/emergencia.
        /// </summary>
        public int AgregarConFicha(
            string nombre,
            DateTime fechaNacimiento,
            string direccion,
            string telefono,
            string? sexo,
            ClienteFichaSaludDTO ficha)
        {
            if (ficha == null)
                throw new ArgumentNullException(nameof(ficha));

            if (!ClienteFichaSaludValidator.Validar(ficha, out string errorFicha))
                throw new Exception(errorFicha);

            int id = clienteDAL.InsertarCliente(nombre, fechaNacimiento, direccion, telefono, sexo);
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
                           string direccion, string telefono, string? sexo = null)
        {
            clienteDAL.ActualizarCliente(id, nombre, fechaNacimiento, direccion, telefono, sexo);
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
            string? sexo,
            ClienteFichaSaludDTO ficha)
        {
            if (id <= 0)
                throw new Exception("Cliente inválido.");
            if (ficha == null)
                throw new ArgumentNullException(nameof(ficha));

            if (!ClienteFichaSaludValidator.Validar(ficha, out string errorFicha))
                throw new Exception(errorFicha);

            clienteDAL.ActualizarCliente(id, nombre, fechaNacimiento, direccion, telefono, sexo);
            ficha.ClienteId = id;
            fichaDAL.Guardar(ficha);
        }

        /// <summary>
        /// Elimina el cliente de la app. Solo si está DESACTIVADO o VENCIDO
        /// y no tiene deudas activas con saldo. No borra miembros ACTIVO/CONGELADO.
        /// </summary>
        public void Eliminar(int id)
        {
            if (id <= 0)
                throw new Exception("Cliente inválido.");

            if (clienteDAL.ObtenerClientePorId(id) == null)
                throw new Exception("Cliente no encontrado.");

            string estado = clienteDAL.ObtenerEstadoMembresia(id);
            if (string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
                throw new Exception("No se puede eliminar un cliente ACTIVO. Desactívelo o espere a que esté vencido.");

            if (string.Equals(estado, "CONGELADO", StringComparison.OrdinalIgnoreCase))
                throw new Exception("No se puede eliminar un cliente CONGELADO. Reactívelo o desactívelo primero.");

            bool elegible =
                string.Equals(estado, "DESACTIVADO", StringComparison.OrdinalIgnoreCase)
                || string.Equals(estado, "VENCIDO", StringComparison.OrdinalIgnoreCase);

            if (!elegible)
                throw new Exception(
                    "Solo se puede eliminar un cliente DESACTIVADO o VENCIDO. Estado actual: " + estado + ".");

            if (new DeudaDAL().TieneDeudasActivas(id))
                throw new Exception("No se puede eliminar: el cliente tiene deudas pendientes. Liquide o cierre la deuda primero.");

            clienteDAL.EliminarCliente(id);
        }

        public DataRow? ObtenerPorId(int id)
        {
            return clienteDAL.ObtenerClientePorId(id);
        }

        /// <summary>Estado SSOT de membresía (ACTIVO / VENCIDO / CONGELADO / …).</summary>
        public string ObtenerEstadoMembresia(int clienteId) =>
            clienteDAL.ObtenerEstadoMembresia(clienteId);

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
