using System;

namespace DTO
{
    public class CongelacionDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int? MembresiaId { get; set; }
        public DateTime FechaCongelacion { get; set; }
        public int DiaAncla { get; set; }
        public int DiasRestantes { get; set; }
        public DateTime? FechaFinOriginal { get; set; }
        public string Motivo { get; set; } = "";
        public string Usuario { get; set; } = "";
        public DateTime? FechaActivacion { get; set; }
        public bool Activa { get; set; }
    }
}
