namespace BLL.Facturas
{
    public sealed class FacturaMembresiaData
    {
        public int ClienteId { get; init; }
        public string ClienteNombre { get; init; } = string.Empty;
        public string ClienteTelefono { get; init; } = string.Empty;
        public string NombrePlan { get; init; } = string.Empty;
        public decimal MontoPagado { get; init; }
        public decimal PrecioUnitario { get; init; }
        public DateTime FechaEmision { get; init; } = DateTime.Now;
        public DateTime FechaVencimientoMembresia { get; init; }
        public string MetodoPago { get; init; } = "Efectivo";
        public int NumeroFactura { get; init; }
        public string? NotaImportanteExtra { get; init; }
    }
}
