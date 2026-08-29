using System;

namespace DTO
{
    /// <summary>Miembro ACTIVO del grid Estado para el diálogo de programación.</summary>
    public class ProgramacionMiembroItemDTO
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Membresia { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
    }
}
