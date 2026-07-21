namespace BLL.Models
{
    public sealed class ClientePerfilResult
    {
        public bool EsCompleto { get; init; }
        public IReadOnlyList<string> CamposFaltantes { get; init; } = Array.Empty<string>();

        public string ResumenCamposFaltantes =>
            CamposFaltantes.Count == 0
                ? string.Empty
                : string.Join(", ", CamposFaltantes);
    }
}
