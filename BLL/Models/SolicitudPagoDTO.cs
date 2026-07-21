namespace BLL.Models
{
    /// <summary>
    /// Resultado del modal FrmPago (calculadora POS).
    /// </summary>
    public sealed class SolicitudPagoDTO
    {
        public decimal TotalAPagar { get; init; }
        public decimal MontoRecibido { get; init; }
        public decimal Cambio => MontoRecibido - TotalAPagar;
        public MetodoPagoPOS MetodoSeleccionado { get; init; }
        public bool DebeImprimirRecibo { get; init; }
    }
}
