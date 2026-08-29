using System;

namespace BLL.Models
{
    public sealed class ProgramacionOperacionResult
    {
        public int ProgramacionId { get; set; }
        public int PagoId { get; set; }
        public int CajaMovimientoId { get; set; }
        public int PlanId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaInicioProgramada { get; set; }
        public DateTime FechaFinProgramada { get; set; }
        public string? PlanNombre { get; set; }
    }
}
