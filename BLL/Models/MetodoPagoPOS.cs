namespace BLL.Models
{
    public enum MetodoPagoPOS
    {
        Efectivo,
        Tarjeta,
        Transferencia
    }

    public static class MetodoPagoPOSExtensions
    {
        public static string ToMetodoBd(this MetodoPagoPOS metodo) => metodo switch
        {
            MetodoPagoPOS.Efectivo => "Efectivo",
            MetodoPagoPOS.Tarjeta => "Tarjeta",
            MetodoPagoPOS.Transferencia => "Transferencia",
            _ => "Efectivo"
        };

        public static string ToEtiqueta(this MetodoPagoPOS metodo) => metodo switch
        {
            MetodoPagoPOS.Efectivo => "EFECTIVO",
            MetodoPagoPOS.Tarjeta => "TARJETA",
            MetodoPagoPOS.Transferencia => "TRANSFERENCIA",
            _ => "EFECTIVO"
        };
    }
}
