namespace BLL
{
    /// <summary>Nombres de plan con comportamiento especial en POS.</summary>
    public static class PlanNombres
    {
        public const string Oferta = "OFERTA";

        public static bool EsOferta(string? nombre) =>
            string.Equals((nombre ?? string.Empty).Trim(), Oferta, StringComparison.OrdinalIgnoreCase);
    }
}
