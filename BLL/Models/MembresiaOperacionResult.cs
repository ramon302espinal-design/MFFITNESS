using System;

namespace BLL.Models
{
    public sealed class MembresiaOperacionResult
    {
        public int MembresiaId { get; set; }
        public int PagoId { get; set; }
        public int CajaMovimientoId { get; set; }
        public int DeudaId { get; set; }
        public int PlanId { get; set; }
        public string? PlanNombre { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaFinMembresia { get; set; }
        public string? WhatsAppDetalle { get; set; }
    }
}
