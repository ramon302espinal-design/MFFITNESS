namespace DTO
{
    /// <summary>
    /// Resultado de editar el financiamiento de una deuda: saldo/estado finales y
    /// el detalle del reverso aplicado al pago inicial anterior.
    /// </summary>
    public class EdicionDeudaDTO
    {
        public int DeudaId { get; set; }
        public decimal TotalFinanciado { get; set; }
        public decimal PagoInicialAnterior { get; set; }
        public decimal PagoInicialNuevo { get; set; }
        public decimal Abonos { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; } = "";

        /// <summary>Se registró el egreso de reverso del pago inicial anterior.</summary>
        public bool ReversoCaja { get; set; }

        /// <summary>Se registró el ingreso del nuevo pago inicial.</summary>
        public bool IngresoCaja { get; set; }
    }
}
