using BLL.Models;

namespace UI.Helpers
{
    /// <summary>Resultado del diálogo FrmDesactivacionMiembro.</summary>
    public sealed class DesactivacionMiembroResult
    {
        public bool Cancelado { get; init; }
        public bool Activar { get; init; }
        public ModoDesactivacionMiembro ModoDesactivacion { get; init; } = ModoDesactivacionMiembro.SinMembresia;

        public static DesactivacionMiembroResult Cancelar() => new() { Cancelado = true };

        public static DesactivacionMiembroResult Activacion() => new() { Activar = true };

        public static DesactivacionMiembroResult Desactivacion(ModoDesactivacionMiembro modo) =>
            new() { ModoDesactivacion = modo };
    }
}
