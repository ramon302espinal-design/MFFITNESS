using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    /// <summary>Queries de auditoría Fase 11.5 (solo lectura).</summary>
    public class IntegridadFinancieraDAL
    {
        private readonly DBHelper db = new();

        /// <summary>Venta financiada sin deuda ACTIVA con (Venta Id N).</summary>
        public int ContarVentasFinanciadasHuerfanas()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Ventas v
                WHERE v.Saldo > 0
                  AND NOT EXISTS (
                      SELECT 1 FROM Deudas d
                      WHERE d.Estado = 'ACTIVA'
                        AND d.Saldo > 0
                        AND d.Concepto LIKE '%(Venta Id ' + CAST(v.Id AS NVARCHAR(20)) + ')%'
                  )";

            return ScalarInt(query);
        }

        public int ContarDeudasProductoSinVentaId()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Deudas d
                WHERE d.Estado = 'ACTIVA'
                  AND d.Saldo > 0
                  AND d.MembresiaId IS NULL
                  AND d.Concepto NOT LIKE '%(Venta Id %'";

            return ScalarInt(query);
        }

        public int ContarDeudasSaldoDesalineadoConVenta()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Deudas d
                INNER JOIN Ventas v ON v.Id = TRY_CAST(
                    SUBSTRING(
                        d.Concepto,
                        PATINDEX('%(Venta Id [0-9]%', d.Concepto) + 10,
                        CHARINDEX(')', d.Concepto, PATINDEX('%(Venta Id [0-9]%', d.Concepto)) 
                            - PATINDEX('%(Venta Id [0-9]%', d.Concepto) - 10
                    ) AS INT)
                WHERE d.Estado = 'ACTIVA'
                  AND d.Saldo > 0
                  AND d.Concepto LIKE '%(Venta Id %'
                  AND ABS(v.Saldo - d.Saldo) > 0.01";

            return ScalarIntSafe(query);
        }

        public int ContarFinanciamientosConCobroSinPagoInicial()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Deudas d
                INNER JOIN Ventas v ON v.Id = TRY_CAST(
                    SUBSTRING(
                        d.Concepto,
                        PATINDEX('%(Venta Id [0-9]%', d.Concepto) + 10,
                        CHARINDEX(')', d.Concepto, PATINDEX('%(Venta Id [0-9]%', d.Concepto)) 
                            - PATINDEX('%(Venta Id [0-9]%', d.Concepto) - 10
                    ) AS INT)
                WHERE d.Estado = 'ACTIVA'
                  AND d.Concepto LIKE '%(Venta Id %'
                  AND PATINDEX('%(Venta Id [0-9]%', d.Concepto) > 0
                  AND v.MontoPagado > 0
                  AND NOT EXISTS (
                      SELECT 1 FROM HistorialDeudas h
                      WHERE h.DeudaId = d.Id AND h.TipoMovimiento = 'PAGO_INICIAL'
                  )";

            return ScalarIntSafe(query);
        }

        public int ContarIngresosVentaSinVenta()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM DetalleCaja dc
                WHERE dc.TipoMovimiento = 'INGRESO'
                  AND dc.Concepto LIKE '%Venta de productos (Id %'
                  AND NOT EXISTS (
                      SELECT 1 FROM Ventas v
                      WHERE dc.Concepto LIKE '%(Id ' + CAST(v.Id AS NVARCHAR(20)) + ')%'
                  )";

            return ScalarInt(query);
        }

        private int ScalarInt(string query)
        {
            object? o = db.ExecuteScalar(query);
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }

        private int ScalarIntSafe(string query)
        {
            try
            {
                return ScalarInt(query);
            }
            catch
            {
                return 0;
            }
        }
    }
}
