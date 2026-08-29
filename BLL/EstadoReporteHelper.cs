using System;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    /// <summary>Agrupa planes para resumen PDF / KPIs de Estado Clientes.</summary>
    internal static class EstadoReporteHelper
    {
        internal static string ClasificarPlan(string nombrePlan)
        {
            string n = (nombrePlan ?? string.Empty).Trim();
            if (string.Equals(n, "M-A", StringComparison.OrdinalIgnoreCase)
                || string.Equals(n, "MENSUALIDAD", StringComparison.OrdinalIgnoreCase))
                return "MENSUALIDAD";
            if (string.Equals(n, "PREMIUM", StringComparison.OrdinalIgnoreCase))
                return "PREMIUM";
            if (string.Equals(n, "PRO", StringComparison.OrdinalIgnoreCase))
                return "PRO";
            if (string.Equals(n, "3x", StringComparison.OrdinalIgnoreCase)
                || string.Equals(n, "3X", StringComparison.OrdinalIgnoreCase))
                return "3X";
            if (string.Equals(n, "ABDOMEN PLANO", StringComparison.OrdinalIgnoreCase))
                return "ABDOMEN PLANO";
            if (string.Equals(n, "GLUTEOS GRANDE", StringComparison.OrdinalIgnoreCase))
                return "GLUTEOS GRANDE";
            if (PlanNombres.EsOferta(n) || PlanNombres.EsAtleta(n) || PlanNombres.EsVisita(n))
                return "OTROS";
            // Planes nuevos del catálogo: cuentan en TOTAL (panel7) aunque no tengan panel propio.
            return string.IsNullOrEmpty(n) ? string.Empty : "OTROS";
        }

        internal static DataTable CrearTablaResumen()
        {
            var dt = new DataTable();
            dt.Columns.Add("Plan", typeof(string));
            dt.Columns.Add("Cantidad", typeof(int));
            dt.Columns.Add("MontoTotal", typeof(decimal));
            return dt;
        }

        internal static DataTable AgregarResumenDesdeFilas(
            IEnumerable<(string Plan, int Cantidad, decimal Monto)> filas)
        {
            var buckets = new Dictionary<string, (int Cantidad, decimal Monto)>(StringComparer.OrdinalIgnoreCase)
            {
                ["MENSUALIDAD"] = (0, 0m),
                ["PREMIUM"] = (0, 0m),
                ["PRO"] = (0, 0m),
                ["3X"] = (0, 0m),
                ["ABDOMEN PLANO"] = (0, 0m),
                ["GLUTEOS GRANDE"] = (0, 0m),
                ["OTROS"] = (0, 0m)
            };

            foreach (var (plan, cantidad, monto) in filas)
            {
                string bucket = ClasificarPlan(plan);
                if (string.IsNullOrEmpty(bucket))
                    continue;

                var actual = buckets[bucket];
                buckets[bucket] = (actual.Cantidad + cantidad, actual.Monto + monto);
            }

            DataTable resumen = CrearTablaResumen();
            foreach (var kv in buckets)
            {
                if (kv.Value.Cantidad <= 0 && kv.Value.Monto <= 0)
                    continue;

                resumen.Rows.Add(kv.Key, kv.Value.Cantidad, kv.Value.Monto);
            }

            return resumen;
        }
    }
}
