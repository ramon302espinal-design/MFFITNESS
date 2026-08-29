using System;

namespace DL
{
    /// <summary>Queries de aceptacion Fase 13 (solo lectura).</summary>
    public class AceptacionFinalDAL
    {
        private readonly DBHelper db = new();
        private readonly IntegridadFinancieraDAL integridad = new();

        /// <summary>13.1 — venta contado cobrada sin ingreso vigente en caja (excluye credito/saldo a favor legacy).</summary>
        public int ContarVentasContadoSinCaja()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Ventas v
                WHERE v.MontoPagado > 0
                  AND ISNULL(v.Saldo, 0) <= 0
                  AND UPPER(LTRIM(RTRIM(ISNULL(v.MetodoPago, '')))) NOT IN (
                      N'SALDO A FAVOR', N'CREDITO', N'CRÉDITO', N'FINANCIADO')
                  AND UPPER(LTRIM(RTRIM(ISNULL(v.MetodoPago, '')))) NOT LIKE N'%FINANC%'
                  AND v.Fecha >= '2026-08-29'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM DetalleCaja dc
                      WHERE dc.TipoMovimiento = 'INGRESO'
                        AND (
                              dc.Concepto LIKE '%(Id ' + CAST(v.Id AS NVARCHAR(20)) + ')%'
                           OR dc.Concepto LIKE '%Venta Id ' + CAST(v.Id AS NVARCHAR(20)) + '%'
                        )
                        AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
                        AND NOT EXISTS (
                            SELECT 1
                            FROM DetalleCaja rev
                            WHERE rev.CajaId = dc.CajaId
                              AND rev.TipoMovimiento = 'EGRESO'
                              AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
                        )
                  )";

            return ScalarInt(query);
        }

        /// <summary>13.2 — venta financiada con cobro sin movimiento de caja.</summary>
        public int ContarVentasFinanciadasConCobroSinCaja()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Ventas v
                WHERE v.MontoPagado > 0
                  AND v.Saldo > 0
                  AND NOT EXISTS (
                      SELECT 1
                      FROM DetalleCaja dc
                      WHERE dc.TipoMovimiento = 'INGRESO'
                        AND dc.Concepto LIKE '%(Id ' + CAST(v.Id AS NVARCHAR(20)) + ')%'
                        AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
                        AND NOT EXISTS (
                            SELECT 1
                            FROM DetalleCaja rev
                            WHERE rev.CajaId = dc.CajaId
                              AND rev.TipoMovimiento = 'EGRESO'
                              AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
                        )
                  )";

            return ScalarInt(query);
        }

        /// <summary>13.3 — deuda membresia con MembresiaId invalido.</summary>
        public int ContarDeudasMembresiaRotas()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Deudas d
                LEFT JOIN Membresias m ON m.Id = d.MembresiaId
                WHERE d.Estado = 'ACTIVA'
                  AND d.MembresiaId IS NOT NULL
                  AND m.Id IS NULL";

            return ScalarIntSafe(query);
        }

        /// <summary>13.4 — MontoPagado de deuda != suma PagosDeuda activos.</summary>
        public int ContarDeudasAbonoDesalineado()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Deudas d
                WHERE d.Estado = 'ACTIVA'
                  AND ABS(
                        ISNULL(d.MontoPagado, 0)
                        - ISNULL((
                            SELECT SUM(pd.Monto)
                            FROM PagosDeuda pd
                            WHERE pd.DeudaId = d.Id
                              AND ISNULL(pd.Estado, '') <> 'ANULADO'
                        ), 0)
                      ) > 0.01";

            return ScalarInt(query);
        }

        /// <summary>13.6 — pago de deuda anulado sin reverso de caja emparejado (heuristica).</summary>
        public int ContarPagosDeudaAnuladosSinReversoCaja()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM PagosDeuda pd
                WHERE pd.Estado = 'ANULADO'
                  AND pd.Fecha >= DATEADD(DAY, -90, GETDATE())
                  AND NOT EXISTS (
                      SELECT 1
                      FROM DetalleCaja dc
                      WHERE dc.TipoMovimiento = 'EGRESO'
                        AND (
                              dc.Concepto LIKE '%Reverso%pago%deuda%'
                           OR dc.Concepto LIKE '%REVERSO%'
                        )
                        AND ABS(dc.Monto - pd.Monto) < 0.01
                        AND ABS(DATEDIFF(MINUTE, dc.Fecha, pd.Fecha)) <= 1440
                  )";

            return ScalarIntSafe(query);
        }

        public IntegridadFinancieraDAL Integridad => integridad;

        private int ScalarInt(string query)
        {
            object? o = db.ExecuteScalar(query);
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
        }

        private int ScalarIntSafe(string query)
        {
            try { return ScalarInt(query); }
            catch { return 0; }
        }
    }
}
