using System;
using System.Collections.Generic;

namespace BLL.Models
{
    public sealed class RenovacionOperacionResult
    {
        public int MembresiaId { get; set; }
        public int PagoId { get; set; }
        public int CajaMovimientoId { get; set; }
        public DateTime FechaFinMembresia { get; set; }

        /// <summary>
        /// Membresías desactivadas al renovar.
        /// </summary>
        public List<(int Id, DateTime FechaFin)> MembresiasCerradas { get; set; } = new();
    }
}
