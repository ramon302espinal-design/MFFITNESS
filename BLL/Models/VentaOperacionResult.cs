using System.Collections.Generic;

namespace BLL.Models
{
    public sealed class VentaOperacionResult
    {
        public int VentaId { get; set; }
        public int DeudaId { get; set; }
        public int CajaMovimientoId { get; set; }
        public decimal MontoPagado { get; set; }
        public List<int> StockMovimientoIds { get; set; } = new();
    }
}
