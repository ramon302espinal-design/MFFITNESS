namespace CORE
{
    /// <summary>
    /// Formatos de fecha/hora visibles al usuario.
    /// La hora del sistema se presenta en 12 horas (AM/PM).
    /// No usar estos formatos en nombres de archivo, logs técnicos ni stamps.
    /// </summary>
    public static class FechaHoraFormats
    {
        public const string Fecha = "dd/MM/yyyy";
        public const string FechaHora = "dd/MM/yyyy hh:mm tt";
        public const string FechaHoraSegundos = "dd/MM/yyyy hh:mm:ss tt";
        public const string Hora = "hh:mm tt";
        public const string HoraSegundos = "hh:mm:ss tt";
    }
}
