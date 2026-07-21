using System.Collections.Generic;

namespace DTO
{
    public class UsuarioDTO
    {
        public int Id { get; set; }

        public string? Usuario { get; set; }
        public string? Clave { get; set; }

        public int RolId { get; set; }
        public string? Rol { get; set; }

        // 🔥 NUEVO: permisos del usuario según su rol
        public List<string> Permisos { get; set; } = new List<string>();
    }
}