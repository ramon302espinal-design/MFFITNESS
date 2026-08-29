using DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// SSOT de números de financiamiento (producto / membresía) para grids, historial y reportes.
    /// Saldo pendiente → Deudas.Saldo · Pago inicial → HistorialDeudas · Precio producto → Ventas.Total
    /// </summary>
    public static class FinanciamientoSSOT
    {
        public static class Origen
        {
            public const string Producto = "Producto";
            public const string Membresia = "Membresía";
        }

        public readonly struct Resumen
        {
            public decimal SaldoPendiente { get; init; }
            public decimal CapitalDeuda { get; init; }
            public decimal PagoInicial { get; init; }
            public decimal PrecioTotal { get; init; }
            public string OrigenPrecio { get; init; }
            public bool EsFinanciado { get; init; }
        }

        /// <summary>Resuelve precio legítimo y origen para una fila de Deudas.</summary>
        public static Resumen ResolverDeuda(
            string? concepto,
            decimal capitalDeuda,
            decimal saldoPendiente,
            decimal pagoInicial,
            bool esMembresia,
            decimal? ventaTotal = null,
            string? textoParseo = null)
        {
            pagoInicial = decimal.Round(Math.Max(0m, pagoInicial), 2);
            capitalDeuda = decimal.Round(Math.Max(0m, capitalDeuda), 2);
            saldoPendiente = decimal.Round(Math.Max(0m, saldoPendiente), 2);

            string conceptoNorm = concepto ?? string.Empty;
            string parseo = textoParseo ?? conceptoNorm;
            bool esProducto = VentasDAL.EsConceptoDeudaVentaProducto(conceptoNorm);
            bool esFinanciado = esMembresia || esProducto || pagoInicial > 0m
                || EsFinanciamientoMembresia(parseo)
                || parseo.Contains("Pago inicial:", StringComparison.OrdinalIgnoreCase);

            decimal precioTotal = ResolverPrecioTotal(
                esFinanciado,
                capitalDeuda,
                pagoInicial,
                ventaTotal,
                parseo);

            string origen = esProducto ? Origen.Producto
                : esMembresia ? Origen.Membresia
                : string.Empty;

            return new Resumen
            {
                SaldoPendiente = saldoPendiente,
                CapitalDeuda = capitalDeuda,
                PagoInicial = pagoInicial,
                PrecioTotal = precioTotal,
                OrigenPrecio = origen,
                EsFinanciado = esFinanciado
            };
        }

        public static string FormatearAporteInicial(decimal pagoInicial, bool esFinanciado)
        {
            if (!esFinanciado)
                return "-";

            return pagoInicial > 0m
                ? $"Sí ({pagoInicial:N2})"
                : "No ($0.00)";
        }

        /// <summary>Enriquece grid de deudas: AporteInicial, PrecioTotal, OrigenPrecio.</summary>
        public static void EnriquecerGridDeudas(DataTable dt, VentasDAL? ventasDal = null)
        {
            if (dt == null) return;

            ventasDal ??= new VentasDAL();

            if (!dt.Columns.Contains("AporteInicial"))
                dt.Columns.Add("AporteInicial", typeof(string));
            if (!dt.Columns.Contains("PrecioTotal"))
                dt.Columns.Add("PrecioTotal", typeof(decimal));
            if (!dt.Columns.Contains("OrigenPrecio"))
                dt.Columns.Add("OrigenPrecio", typeof(string));

            bool tienePagoInicial = dt.Columns.Contains("PagoInicialFinanciamiento");
            bool tieneSaldo = dt.Columns.Contains("Saldo");
            bool tieneMembresia = dt.Columns.Contains("MembresiaId");

            var ventaIds = dt.AsEnumerable()
                .Select(r => VentasDAL.TryExtraerVentaIdDeConcepto(r.Table.Columns.Contains("Concepto")
                    ? r["Concepto"]?.ToString()
                    : null))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            Dictionary<int, decimal> totalesVenta = ventasDal.ObtenerTotalesPorIds(ventaIds);

            foreach (DataRow row in dt.Rows)
            {
                decimal capitalDeuda = row.Table.Columns.Contains("MontoTotal") && row["MontoTotal"] != DBNull.Value
                    ? Convert.ToDecimal(row["MontoTotal"])
                    : 0m;
                decimal saldoPendiente = tieneSaldo && row["Saldo"] != DBNull.Value
                    ? Convert.ToDecimal(row["Saldo"])
                    : capitalDeuda;

                decimal pagoInicial = 0m;
                if (tienePagoInicial && row["PagoInicialFinanciamiento"] != DBNull.Value)
                    pagoInicial = Convert.ToDecimal(row["PagoInicialFinanciamiento"]);

                bool esMembresia = tieneMembresia && row["MembresiaId"] != DBNull.Value && row["MembresiaId"] != null;
                string concepto = row.Table.Columns.Contains("Concepto")
                    ? row["Concepto"]?.ToString() ?? string.Empty
                    : string.Empty;

                decimal? ventaTotal = VentasDAL.TryExtraerVentaIdDeConcepto(concepto) is int ventaId
                    && totalesVenta.TryGetValue(ventaId, out decimal total)
                    ? total
                    : null;

                Resumen resumen = ResolverDeuda(
                    concepto,
                    capitalDeuda,
                    saldoPendiente,
                    pagoInicial,
                    esMembresia,
                    ventaTotal);

                row["AporteInicial"] = FormatearAporteInicial(resumen.PagoInicial, resumen.EsFinanciado);
                row["PrecioTotal"] = resumen.EsFinanciado ? resumen.PrecioTotal : capitalDeuda;
                row["OrigenPrecio"] = resumen.OrigenPrecio;
            }
        }

        /// <summary>Enriquece historial de deudas: columnas de financiamiento sin mutar Monto.</summary>
        public static void EnriquecerHistorialDeudas(DataTable dt, DeudaDAL deudaDal, VentasDAL ventasDal)
        {
            if (dt == null) return;

            if (!dt.Columns.Contains("AporteInicial"))
                dt.Columns.Add("AporteInicial", typeof(string));
            if (!dt.Columns.Contains("SaldoDeuda"))
                dt.Columns.Add("SaldoDeuda", typeof(decimal));
            if (!dt.Columns.Contains("PrecioTotal"))
                dt.Columns.Add("PrecioTotal", typeof(decimal));
            if (!dt.Columns.Contains("OrigenPrecio"))
                dt.Columns.Add("OrigenPrecio", typeof(string));

            var pagosInicialesPorDeuda = AcumularPagosInicialesHistorial(dt);

            var deudaIds = dt.Columns.Contains("DeudaId")
                ? dt.AsEnumerable()
                    .Where(r => r["DeudaId"] != DBNull.Value)
                    .Select(r => Convert.ToInt32(r["DeudaId"]))
                    .Distinct()
                    .ToList()
                : new List<int>();

            var contextoPorDeuda = new Dictionary<int, DataRow>();
            if (deudaIds.Count > 0)
            {
                foreach (DataRow ctx in deudaDal.ObtenerContextoFinanciamientoPorDeudas(deudaIds).Rows)
                    contextoPorDeuda[Convert.ToInt32(ctx["DeudaId"])] = ctx;
            }

            var ventaIds = contextoPorDeuda.Values
                .Select(r => VentasDAL.TryExtraerVentaIdDeConcepto(r["Concepto"]?.ToString()))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            Dictionary<int, decimal> totalesVenta = ventasDal.ObtenerTotalesPorIds(ventaIds);
            var precioPorDeuda = new Dictionary<int, Resumen>();

            foreach (DataRow row in dt.Rows)
            {
                if ((row["Tipo"]?.ToString() ?? string.Empty) != "DEUDA")
                    continue;
                if (row["DeudaId"] == DBNull.Value)
                    continue;

                int deudaId = Convert.ToInt32(row["DeudaId"]);
                string descripcion = row["Descripcion"]?.ToString() ?? string.Empty;
                decimal saldoMovimiento = row["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Monto"]);
                row["SaldoDeuda"] = saldoMovimiento;

                contextoPorDeuda.TryGetValue(deudaId, out DataRow? ctx);
                string concepto = ctx?["Concepto"]?.ToString() ?? descripcion;
                bool esMembresia = ctx != null && ctx["MembresiaId"] != DBNull.Value && ctx["MembresiaId"] != null
                    || EsFinanciamientoMembresia(descripcion);

                decimal pagoInicial = pagosInicialesPorDeuda.TryGetValue(deudaId, out decimal pi) ? pi : 0m;
                if (pagoInicial <= 0m && ctx != null && ctx["PagoInicialFinanciamiento"] != DBNull.Value)
                    pagoInicial = Convert.ToDecimal(ctx["PagoInicialFinanciamiento"]);

                decimal capitalDeuda = ctx != null && ctx["SaldoDeuda"] != DBNull.Value
                    ? Convert.ToDecimal(ctx["SaldoDeuda"])
                    : saldoMovimiento;

                decimal? ventaTotal = VentasDAL.TryExtraerVentaIdDeConcepto(concepto) is int ventaId
                    && totalesVenta.TryGetValue(ventaId, out decimal vt)
                    ? vt
                    : null;

                Resumen resumen = ResolverDeuda(
                    concepto,
                    capitalDeuda,
                    saldoMovimiento,
                    pagoInicial,
                    esMembresia,
                    ventaTotal,
                    descripcion);

                if (resumen.EsFinanciado)
                    precioPorDeuda[deudaId] = resumen;
            }

            foreach (DataRow row in dt.Rows)
            {
                string tipo = row["Tipo"]?.ToString() ?? string.Empty;
                AplicarAporteInicialHistorial(row, tipo, pagosInicialesPorDeuda, precioPorDeuda);

                if (row["DeudaId"] == DBNull.Value
                    || !precioPorDeuda.TryGetValue(Convert.ToInt32(row["DeudaId"]), out Resumen precioInfo))
                {
                    row["PrecioTotal"] = DBNull.Value;
                    row["OrigenPrecio"] = string.Empty;
                    continue;
                }

                row["PrecioTotal"] = precioInfo.PrecioTotal;
                row["OrigenPrecio"] = precioInfo.OrigenPrecio;

                if (tipo == "DEUDA" && row["SaldoDeuda"] == DBNull.Value)
                    row["SaldoDeuda"] = precioInfo.SaldoPendiente;
            }
        }

        private static Dictionary<int, decimal> AcumularPagosInicialesHistorial(DataTable dt)
        {
            var pagosInicialesPorDeuda = new Dictionary<int, decimal>();
            if (!dt.Columns.Contains("DeudaId"))
                return pagosInicialesPorDeuda;

            foreach (DataRow row in dt.Rows)
            {
                string tipoFila = row["Tipo"]?.ToString() ?? string.Empty;
                bool esInicial = tipoFila == "PAGO_INICIAL";
                bool esReverso = tipoFila == "REVERSO_PAGO_INICIAL";

                if ((!esInicial && !esReverso) || row["DeudaId"] == DBNull.Value)
                    continue;

                int deudaId = Convert.ToInt32(row["DeudaId"]);
                decimal monto = row["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Monto"]);

                pagosInicialesPorDeuda.TryGetValue(deudaId, out decimal acumulado);
                pagosInicialesPorDeuda[deudaId] = esInicial ? acumulado + monto : acumulado - monto;
            }

            return pagosInicialesPorDeuda;
        }

        private static void AplicarAporteInicialHistorial(
            DataRow row,
            string tipo,
            Dictionary<int, decimal> pagosInicialesPorDeuda,
            Dictionary<int, Resumen> precioPorDeuda)
        {
            if (tipo == "PAGO_INICIAL")
            {
                row["AporteInicial"] = $"Sí ({Convert.ToDecimal(row["Monto"]):N2})";
                return;
            }

            if (tipo == "REVERSO_PAGO_INICIAL")
            {
                row["AporteInicial"] = $"Reverso (-{Convert.ToDecimal(row["Monto"]):N2})";
                return;
            }

            if (tipo != "DEUDA")
            {
                row["AporteInicial"] = string.Empty;
                return;
            }

            string descripcion = row["Descripcion"]?.ToString() ?? string.Empty;
            if (row["DeudaId"] != DBNull.Value
                && precioPorDeuda.TryGetValue(Convert.ToInt32(row["DeudaId"]), out Resumen info))
            {
                row["AporteInicial"] = FormatearAporteInicial(
                    pagosInicialesPorDeuda.TryGetValue(Convert.ToInt32(row["DeudaId"]), out decimal pi)
                        ? pi
                        : info.PagoInicial,
                    true);
                return;
            }

            if (EsFinanciamientoMembresia(descripcion)
                || descripcion.Contains("Pago inicial:", StringComparison.OrdinalIgnoreCase))
            {
                int deudaId = Convert.ToInt32(row["DeudaId"]);
                row["AporteInicial"] = FormatearAporteInicial(
                    pagosInicialesPorDeuda.TryGetValue(deudaId, out decimal pi) ? pi : 0m,
                    true);
                return;
            }

            row["AporteInicial"] = "-";
        }

        private static decimal ResolverPrecioTotal(
            bool esFinanciado,
            decimal capitalDeuda,
            decimal pagoInicial,
            decimal? ventaTotal,
            string? textoParseo)
        {
            if (!esFinanciado)
                return capitalDeuda;

            if (ventaTotal.HasValue && ventaTotal.Value > 0m)
                return decimal.Round(ventaTotal.Value, 2);

            decimal? parseado = TryParsePrecioTotalDescripcion(textoParseo);
            if (parseado.HasValue && parseado.Value > 0m)
                return parseado.Value;

            return decimal.Round(capitalDeuda + pagoInicial, 2);
        }

        public static bool EsFinanciamientoMembresia(string? descripcion) =>
            !string.IsNullOrWhiteSpace(descripcion) && (
                descripcion.Contains("Financiamiento", StringComparison.OrdinalIgnoreCase)
                || descripcion.Contains("Saldo plan", StringComparison.OrdinalIgnoreCase)
                || descripcion.Contains("Total plan:", StringComparison.OrdinalIgnoreCase));

        public static bool EsFinanciamiento(string? descripcion, bool esProducto, bool esMembresia) =>
            esProducto
            || esMembresia
            || (!string.IsNullOrWhiteSpace(descripcion)
                && descripcion.Contains("Pago inicial:", StringComparison.OrdinalIgnoreCase))
            || EsFinanciamientoMembresia(descripcion);

        public static decimal? TryParsePrecioTotalDescripcion(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return null;

            Match matchPlan = Regex.Match(
                descripcion,
                @"Total plan:\s*([\d,\.]+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (matchPlan.Success && TryParseMonto(matchPlan.Groups[1].Value, out decimal plan))
                return plan;

            Match matchTotal = Regex.Match(
                descripcion,
                @"\|\s*Total:\s*([\d,\.]+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (matchTotal.Success && TryParseMonto(matchTotal.Groups[1].Value, out decimal total))
                return total;

            Match matchRd = Regex.Match(
                descripcion,
                @"\(RD\$\s*([\d,\.]+)\)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (matchRd.Success && TryParseMonto(matchRd.Groups[1].Value, out decimal rd))
                return rd;

            return null;
        }

        private static bool TryParseMonto(string texto, out decimal valor)
        {
            valor = 0m;
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            string normalizado = texto.Trim().Replace(",", "");
            return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }
    }
}
