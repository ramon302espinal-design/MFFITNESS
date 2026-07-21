



namespace DTO
{
    public class MembresiaDTO
    {
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? Estado { get; set; }
    }
}