using CORE;
using DL;
using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Historial de cierres. El cierre operativo vive en <see cref="CajaServiceBLL.CerrarCajaConCuadre"/>.
    /// </summary>
    public class CierreCajaBLL
    {
        private readonly CierreCajaDAL cierreCajaDAL = new CierreCajaDAL();

        public void EliminarCierre(int id)
        {
            cierreCajaDAL.EliminarCierre(id);
        }

        public DataTable ObtenerHistorial()
        {
            bool verTodos = string.Equals(
                Sesion.Rol?.Trim(),
                "ADMIN",
                StringComparison.OrdinalIgnoreCase);
            string usuario = Sesion.Usuario?.Trim() ?? string.Empty;

            if (!verTodos && string.IsNullOrWhiteSpace(usuario))
                throw new InvalidOperationException("No hay un usuario válido para consultar los cierres.");

            return cierreCajaDAL.ObtenerHistorialCierres(verTodos, usuario);
        }
    }
}
