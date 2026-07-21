using System.Globalization;

namespace UI.Helpers
{
    public static class MonedaHelper
    {
        public const string FormatoGridRd = "#,##0.00";

        private static readonly CultureInfo CulturaRd = CultureInfo.GetCultureInfo("es-DO");

        public static string FormatearRd(decimal monto) =>
            "RD$ " + monto.ToString(FormatoGridRd, CulturaRd);

        public static string FormatearRd(object? valor)
        {
            if (valor == null || valor == DBNull.Value)
                return "RD$ 0.00";

            return decimal.TryParse(valor.ToString(), out decimal monto)
                ? FormatearRd(monto)
                : "RD$ 0.00";
        }
    }
}
