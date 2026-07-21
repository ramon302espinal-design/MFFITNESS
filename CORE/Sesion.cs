using System.Collections.Generic;

namespace CORE
{
    public static class Sesion
    {
        public static int UsuarioId { get; set; }
        public static string Usuario { get; set; } = string.Empty;

        public static int RolId { get; set; }
        public static string Rol { get; set; } = string.Empty;

        // 🔥 CLAVE DEL SISTEMA
        public static List<string> Permisos { get; set; } = new List<string>();

        public static bool Activa => UsuarioId > 0;

        public static void Iniciar(int usuarioId, string usuario, int rolId, string rol, List<string> permisos)
        {
            UsuarioId = usuarioId;
            Usuario = usuario;
            RolId = rolId;
            Rol = rol;
            Permisos = permisos;
        }

        public static bool TienePermiso(string permiso)
        {
            return Permisos.Contains(permiso);
        }

        public static void CerrarSesion()
        {
            UsuarioId = 0;
            Usuario = string.Empty;
            RolId = 0;
            Rol = string.Empty;
            Permisos.Clear();
        }
    }
}