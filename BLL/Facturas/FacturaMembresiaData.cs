namespace BLL.Facturas
{
    public sealed class FacturaMembresiaData
    {
        public int ClienteId { get; init; }
        public string ClienteNombre { get; init; } = string.Empty;
        public string ClienteTelefono { get; init; } = string.Empty;
        public string NombrePlan { get; init; } = string.Empty;
        /// <summary>Monto cobrado en esta operación (entra a caja).</summary>
        public decimal MontoPagado { get; init; }
        /// <summary>Precio de lista del plan (catálogo).</summary>
        public decimal PrecioUnitario { get; init; }
        public decimal DescuentoMonto { get; init; }
        public decimal DescuentoPorcentaje { get; init; }
        /// <summary>Asunto/motivo de oferta (módulo Oferta).</summary>
        public string? AsuntoOferta { get; init; }
        public DateTime FechaEmision { get; init; } = DateTime.Now;
        public DateTime FechaVencimientoMembresia { get; init; }
        public string MetodoPago { get; init; } = "Efectivo";
        public int NumeroFactura { get; init; }
        public string? NotaImportanteExtra { get; init; }
    }
}
