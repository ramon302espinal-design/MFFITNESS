using DL;
using DTO;
using System.Data;
using System.Collections.Generic;

namespace BLL
{
    public class UsuarioBLL
    {
        private readonly UsuarioDAL dal = new UsuarioDAL();

        public UsuarioDTO? LoginPorId(int idUsuario, string clave)
        {
            // 1. Validar usuario en BD
            var user = dal.LoginPorId(idUsuario, clave);

            if (user == null)
                return null;

            // 2. Cargar permisos del rol (REGLA DE NEGOCIO)
            user.Permisos = dal.ObtenerPermisosPorRol(user.RolId);

            return user;
        }

        public DataTable TraerUsuariosActivos()
        {
            return dal.TraerUsuariosActivos();
        }
    }
}