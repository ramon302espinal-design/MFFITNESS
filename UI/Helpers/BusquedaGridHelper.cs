using System;

namespace UI.Helpers
{
    public static class BusquedaGridHelper
    {
        public static string EscaparFiltroDataView(string valor)
        {
            return valor
                .Replace("[", "[[]", StringComparison.Ordinal)
                .Replace("]", "[]]", StringComparison.Ordinal)
                .Replace("*", "[*]", StringComparison.Ordinal)
                .Replace("%", "[%]", StringComparison.Ordinal)
                .Replace("'", "''", StringComparison.Ordinal);
        }

        public static string ConstruirFiltroClientes(string termino)
        {
            var valor = EscaparFiltroDataView(termino);
            var like = $"'%{valor}%'";

            return $"Convert(Id, 'System.String') LIKE {like} " +
                   $"OR Nombre LIKE {like} " +
                   $"OR Telefono LIKE {like} " +
                   $"OR Direccion LIKE {like} " +
                   $"OR Convert(FechaNacimiento, 'System.String') LIKE {like}";
        }

        public static string ConstruirFiltroEstadoClientes(string termino)
        {
            var valor = EscaparFiltroDataView(termino);
            var like = $"'%{valor}%'";

            return $"Convert(ID, 'System.String') LIKE {like} " +
                   $"OR Nombre LIKE {like} " +
                   $"OR Membresia LIKE {like} " +
                   $"OR Estado LIKE {like} " +
                   $"OR Convert(FechaInicio, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaFin, 'System.String') LIKE {like}";
        }

        public static string ConstruirFiltroDeudas(string termino)
        {
            var valor = EscaparFiltroDataView(termino);
            var like = $"'%{valor}%'";

            return $"Convert(Id, 'System.String') LIKE {like} " +
                   $"OR Convert(ClienteId, 'System.String') LIKE {like} " +
                   $"OR Nombre LIKE {like} " +
                   $"OR Concepto LIKE {like} " +
                   $"OR Plan LIKE {like} " +
                   $"OR Estado LIKE {like} " +
                   $"OR AporteInicial LIKE {like} " +
                   $"OR Convert(MontoTotal, 'System.String') LIKE {like} " +
                   $"OR Convert(MontoPagado, 'System.String') LIKE {like} " +
                   $"OR Convert(Saldo, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaVencimiento, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaInicioMembresia, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaFinMembresia, 'System.String') LIKE {like}";
        }
    }
}
