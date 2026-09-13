using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DL
{
    /// <summary>
    /// Snapshot de aplicación de abonos sobre el cronograma (solo lectura / evidencia).
    /// No modifica Saldo ni PagosDeuda.
    /// </summary>
    public sealed class CuotaEvidenciaSnapshot
    {
        public int? CuotaNumero { get; init; }
        public DateTime? CuotaFecha { get; init; }
        public decimal? CuotaTotal { get; init; }
        public decimal? FaltaCuota { get; init; }
        public IReadOnlyList<CuotaCubiertaInfo> CuotasCubiertas { get; init; } = Array.Empty<CuotaCubiertaInfo>();
        public int? ProximaCuotaNumero { get; init; }
        public DateTime? ProximaCuotaFecha { get; init; }
    }

    public sealed class CuotaCubiertaInfo
    {
        public int NumeroCuota { get; init; }
        public DateTime FechaVencimiento { get; init; }
        public decimal Total { get; init; }
    }

    public static class PrestamoCuotaEvidenciaHelper
    {
        public static CuotaEvidenciaSnapshot Resolver(
            SqlConnection conn,
            SqlTransaction tx,
            int deudaId,
            decimal montoPagadoAntes,
            decimal montoPagadoDespues)
        {
            var cuotas = new List<(int Numero, DateTime Fecha, decimal Total)>();

            using (var cmd = new SqlCommand(@"
SELECT det.NumeroCuota, det.FechaVencimiento, det.Total
FROM dbo.PrestamoDetalleCuotas det
INNER JOIN dbo.PrestamosCuotas pc ON pc.Id = det.PrestamoId
WHERE pc.DeudaId = @DeudaId
  AND pc.Estado = N'ACTIVO'
ORDER BY det.NumeroCuota ASC;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@DeudaId", deudaId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cuotas.Add((
                        Convert.ToInt32(reader["NumeroCuota"]),
                        Convert.ToDateTime(reader["FechaVencimiento"]).Date,
                        Convert.ToDecimal(reader["Total"])));
                }
            }

            if (cuotas.Count == 0)
                return new CuotaEvidenciaSnapshot();

            decimal pagoInicial = 0m;
            using (var cmdPi = new SqlCommand(@"
SELECT CAST(ISNULL(SUM(CASE
    WHEN TipoMovimiento = N'PAGO_INICIAL' THEN Monto
    WHEN TipoMovimiento = N'REVERSO_PAGO_INICIAL' THEN -Monto
    ELSE 0 END), 0) AS DECIMAL(18,2))
FROM dbo.HistorialDeudas
WHERE DeudaId = @DeudaId
  AND TipoMovimiento IN (N'PAGO_INICIAL', N'REVERSO_PAGO_INICIAL');", conn, tx))
            {
                cmdPi.Parameters.AddWithValue("@DeudaId", deudaId);
                object? o = cmdPi.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                    pagoInicial = Convert.ToDecimal(o);
                if (pagoInicial < 0m)
                    pagoInicial = 0m;
            }

            // MontoPagado del ledger incluye aporte inicial; el cronograma solo ve abonos posteriores.
            decimal abonadoAntes = Math.Max(0m, montoPagadoAntes - pagoInicial);
            decimal abonadoDespues = Math.Max(0m, montoPagadoDespues - pagoInicial);
            var cubiertas = new List<CuotaCubiertaInfo>();

            decimal acum = 0m;
            int? cuotaActual = null;
            DateTime? fechaActual = null;
            decimal? totalActual = null;
            decimal? falta = null;

            foreach (var c in cuotas)
            {
                decimal tope = acum + c.Total;

                if (abonadoAntes < tope && abonadoDespues >= tope)
                {
                    cubiertas.Add(new CuotaCubiertaInfo
                    {
                        NumeroCuota = c.Numero,
                        FechaVencimiento = c.Fecha,
                        Total = c.Total
                    });
                }

                if (abonadoDespues < tope && cuotaActual == null)
                {
                    cuotaActual = c.Numero;
                    fechaActual = c.Fecha;
                    totalActual = c.Total;
                    falta = tope - abonadoDespues;
                }

                acum = tope;
            }

            return new CuotaEvidenciaSnapshot
            {
                CuotaNumero = cuotaActual ?? (cubiertas.Count > 0 ? cubiertas[^1].NumeroCuota : null),
                CuotaFecha = fechaActual ?? (cubiertas.Count > 0 ? cubiertas[^1].FechaVencimiento : null),
                CuotaTotal = totalActual ?? (cubiertas.Count > 0 ? cubiertas[^1].Total : null),
                FaltaCuota = falta,
                CuotasCubiertas = cubiertas,
                ProximaCuotaNumero = cuotaActual,
                ProximaCuotaFecha = fechaActual
            };
        }
    }
}
