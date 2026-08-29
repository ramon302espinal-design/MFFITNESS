using System;

namespace DTO
{
    public class MembresiaProgramadaDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaInicioProgramada { get; set; }
        public DateTime FechaFinProgramada { get; set; }
        public int? MembresiaOrigenId { get; set; }
        public string Estado { get; set; } = "PENDIENTE";
        public string? Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? PagoId { get; set; }
        public int? CajaMovimientoId { get; set; }
        public string? Nota { get; set; }
        public string? PlanNombre { get; set; }
        public string? ClienteNombre { get; set; }
    }
}
