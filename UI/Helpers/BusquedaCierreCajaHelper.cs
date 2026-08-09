using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UI.Helpers
{
    public sealed class RangoFechaBusqueda
    {
        public DateTime? Desde { get; init; }
        public DateTime? Hasta { get; init; }

        public static RangoFechaBusqueda Dia(DateTime fecha) =>
            new() { Desde = fecha.Date, Hasta = fecha.Date };

        public static RangoFechaBusqueda Entre(DateTime desde, DateTime hasta) =>
            new() { Desde = desde.Date, Hasta = hasta.Date };
    }

    public static class BusquedaCierreCajaHelper
    {
        public static readonly string[] PresetsRango =
        {
            "Todos",
            "Hoy",
            "Ayer",
            "Últimos 7 días",
            "Este mes",
            "Mes anterior",
            "Personalizado"
        };

        public static RangoFechaBusqueda? ResolverPreset(string preset)
        {
            DateTime hoy = DateTime.Today;

            return preset switch
            {
                "Hoy" => RangoFechaBusqueda.Dia(hoy),
                "Ayer" => RangoFechaBusqueda.Dia(hoy.AddDays(-1)),
                "Últimos 7 días" => RangoFechaBusqueda.Entre(hoy.AddDays(-6), hoy),
                "Este mes" => RangoFechaBusqueda.Entre(
                    new DateTime(hoy.Year, hoy.Month, 1),
                    hoy),
                "Mes anterior" =>
                    RangoFechaBusqueda.Entre(
                        new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-1),
                        new DateTime(hoy.Year, hoy.Month, 1).AddDays(-1)),
                "Personalizado" => null,
                _ => null
            };
        }

        public static RangoFechaBusqueda? IntentarResolverFechaInteligente(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            string t = texto.Trim().ToLowerInvariant();
            DateTime hoy = DateTime.Today;

            if (t is "hoy" or "today")
                return RangoFechaBusqueda.Dia(hoy);

            if (t is "ayer" or "yesterday")
                return RangoFechaBusqueda.Dia(hoy.AddDays(-1));

            if (t.Contains("semana", StringComparison.Ordinal))
                return RangoFechaBusqueda.Entre(hoy.AddDays(-(int)hoy.DayOfWeek), hoy);

            if (t is "mes" or "este mes" or "mes actual")
                return RangoFechaBusqueda.Entre(new DateTime(hoy.Year, hoy.Month, 1), hoy);

            if (t is "mes anterior" or "mes pasado")
            {
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
                return RangoFechaBusqueda.Entre(inicioMes.AddMonths(-1), inicioMes.AddDays(-1));
            }

            if (TryParseMesAnio(t, out var rangoMes))
                return rangoMes;

            if (DateTime.TryParseExact(t, new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy" },
                    CulturaEs, DateTimeStyles.None, out DateTime dia))
                return RangoFechaBusqueda.Dia(dia);

            if (DateTime.TryParse(t, CulturaEs, DateTimeStyles.None, out dia))
                return RangoFechaBusqueda.Dia(dia);

            if (Regex.IsMatch(t, @"^\d{1,2}/\d{4}$") &&
                DateTime.TryParseExact("01/" + t, "dd/MM/yyyy", CulturaEs, DateTimeStyles.None, out DateTime mesAnio))
            {
                var fin = mesAnio.AddMonths(1).AddDays(-1);
                return RangoFechaBusqueda.Entre(mesAnio, fin);
            }

            return null;
        }

        public static string ConstruirFiltroDataView(
            RangoFechaBusqueda? rangoPreset,
            RangoFechaBusqueda? rangoPersonalizado,
            string textoLibre)
        {
            var partes = new List<string>();
            var rango = rangoPreset ?? IntentarResolverFechaInteligente(textoLibre) ?? rangoPersonalizado;

            if (rango?.Desde != null)
                partes.Add($"Fecha >= #{rango.Desde.Value:MM/dd/yyyy}#");

            if (rango?.Hasta != null)
                partes.Add($"Fecha <= #{rango.Hasta.Value:MM/dd/yyyy}#");

            string terminoTexto = textoLibre?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(terminoTexto) && IntentarResolverFechaInteligente(terminoTexto) == null)
            {
                string esc = EscaparFiltroDataView(terminoTexto);
                string like = $"'%{esc}%'";

                partes.Add(
                    "(" +
                    $"Turno LIKE {like} " +
                    $"OR Usuario LIKE {like} " +
                    $"OR Convert(MontoInicial, 'System.String') LIKE {like} " +
                    $"OR Convert(TotalIngresos, 'System.String') LIKE {like} " +
                    $"OR Convert(TotalGastos, 'System.String') LIKE {like} " +
                    $"OR Convert(TotalSistema, 'System.String') LIKE {like} " +
                    $"OR Convert(TotalContado, 'System.String') LIKE {like} " +
                    $"OR Convert(Diferencia, 'System.String') LIKE {like} " +
                    $"OR Convert(Fecha, 'System.String') LIKE {like} " +
                    $"OR Convert(FechaCierre, 'System.String') LIKE {like}" +
                    ")");
            }

            return partes.Count == 0 ? string.Empty : string.Join(" AND ", partes);
        }

        private static bool TryParseMesAnio(string texto, out RangoFechaBusqueda? rango)
        {
            rango = null;
            var meses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["enero"] = 1, ["febrero"] = 2, ["marzo"] = 3, ["abril"] = 4,
                ["mayo"] = 5, ["junio"] = 6, ["julio"] = 7, ["agosto"] = 8,
                ["septiembre"] = 9, ["setiembre"] = 9, ["octubre"] = 10,
                ["noviembre"] = 11, ["diciembre"] = 12
            };

            foreach (var par in meses)
            {
                if (!texto.Contains(par.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                int anio = DateTime.Today.Year;
                var matchAnio = Regex.Match(texto, @"\b(20\d{2})\b");
                if (matchAnio.Success)
                    int.TryParse(matchAnio.Value, out anio);

                var inicio = new DateTime(anio, par.Value, 1);
                rango = RangoFechaBusqueda.Entre(inicio, inicio.AddMonths(1).AddDays(-1));
                return true;
            }

            return false;
        }

        private static string EscaparFiltroDataView(string valor) =>
            BusquedaGridHelper.EscaparFiltroDataView(valor);

        private static readonly CultureInfo CulturaEs = CultureInfo.GetCultureInfo("es-DO");
    }
}
