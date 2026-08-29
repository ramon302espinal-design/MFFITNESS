/*
  FASE 13 — Aceptacion final (sinergia 100%).
  Ejecutar en DEV y PROD. Items 13.7, 13.8, 13.10 requieren prueba manual en UI.
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

PRINT '=== F13 ACEPTACION — ' + DB_NAME() + ' ===';

DECLARE @SchemaVersion INT = (SELECT ISNULL(MAX(Version), 0) FROM dbo.SchemaVersion);

DECLARE @C13_1 INT = (
    SELECT COUNT(*)
    FROM Ventas v
    WHERE v.MontoPagado > 0 AND ISNULL(v.Saldo, 0) <= 0
      AND UPPER(LTRIM(RTRIM(ISNULL(v.MetodoPago, '')))) NOT IN (
          N'SALDO A FAVOR', N'CREDITO', N'CRÉDITO', N'FINANCIADO')
      AND UPPER(LTRIM(RTRIM(ISNULL(v.MetodoPago, '')))) NOT LIKE N'%FINANC%'
      AND v.Fecha >= '2026-08-29'
      AND NOT EXISTS (
          SELECT 1 FROM DetalleCaja dc
          WHERE dc.TipoMovimiento = 'INGRESO'
            AND (
                  dc.Concepto LIKE '%(Id ' + CAST(v.Id AS NVARCHAR(20)) + ')%'
               OR dc.Concepto LIKE '%Venta Id ' + CAST(v.Id AS NVARCHAR(20)) + '%'
            )
            AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
            AND NOT EXISTS (
                SELECT 1 FROM DetalleCaja rev
                WHERE rev.CajaId = dc.CajaId AND rev.TipoMovimiento = 'EGRESO'
                  AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
            )
      )
);

DECLARE @Orphans INT = (
    SELECT COUNT(*)
    FROM Ventas v
    WHERE v.Saldo > 0
      AND NOT EXISTS (
          SELECT 1 FROM Deudas d
          WHERE d.Estado = 'ACTIVA' AND d.Saldo > 0
            AND d.Concepto LIKE '%(Venta Id ' + CAST(v.Id AS NVARCHAR(20)) + ')%'
      )
);

DECLARE @SinPagoInicial INT = (
    SELECT COUNT(*)
    FROM Deudas d
    INNER JOIN Ventas v ON v.Id = TRY_CAST(
        SUBSTRING(
            d.Concepto,
            PATINDEX('%(Venta Id [0-9]%', d.Concepto) + 10,
            NULLIF(
                CHARINDEX(')', d.Concepto, PATINDEX('%(Venta Id [0-9]%', d.Concepto))
                    - PATINDEX('%(Venta Id [0-9]%', d.Concepto) - 10,
                0)
        ) AS INT)
    WHERE d.Estado = 'ACTIVA'
      AND d.Concepto LIKE '%(Venta Id %'
      AND PATINDEX('%(Venta Id [0-9]%', d.Concepto) > 0
      AND v.MontoPagado > 0
      AND NOT EXISTS (
          SELECT 1 FROM HistorialDeudas h
          WHERE h.DeudaId = d.Id AND h.TipoMovimiento = 'PAGO_INICIAL'
      )
);

DECLARE @AbonoDesalineado INT = (
    SELECT COUNT(*)
    FROM Deudas d
    WHERE d.Estado = 'ACTIVA'
      AND ABS(ISNULL(d.MontoPagado, 0) - ISNULL((
            SELECT SUM(pd.Monto) FROM PagosDeuda pd
            WHERE pd.DeudaId = d.Id AND ISNULL(pd.Estado, '') <> 'ANULADO'
        ), 0)) > 0.01
);

DECLARE @MembresiaRota INT = (
    SELECT COUNT(*)
    FROM Deudas d
    LEFT JOIN Membresias m ON m.Id = d.MembresiaId
    WHERE d.Estado = 'ACTIVA' AND d.MembresiaId IS NOT NULL AND m.Id IS NULL
);

DECLARE @AutoPass BIT = CASE
    WHEN @C13_1 = 0 AND @Orphans = 0 AND @SinPagoInicial = 0
         AND @AbonoDesalineado = 0 AND @MembresiaRota = 0 THEN 1 ELSE 0 END;

SELECT
    '13.1' AS ItemId, 'Venta contado -> caja' AS Prueba,
    CASE WHEN @C13_1 = 0 THEN 'PASS' ELSE 'FAIL' END AS Resultado,
    @C13_1 AS Contador
UNION ALL SELECT '13.2', 'Venta financiada + pago inicial',
    CASE WHEN @Orphans = 0 AND @SinPagoInicial = 0 THEN 'PASS' ELSE 'FAIL' END,
    @Orphans + @SinPagoInicial
UNION ALL SELECT '13.3', 'Membresia financiada vinculada',
    CASE WHEN @MembresiaRota = 0 THEN 'PASS' ELSE 'FAIL' END, @MembresiaRota
UNION ALL SELECT '13.4', 'Abono deuda alineado',
    CASE WHEN @AbonoDesalineado = 0 THEN 'PASS' ELSE 'FAIL' END, @AbonoDesalineado
UNION ALL SELECT '13.5', 'Editar pago inicial (historial/caja)',
    CASE WHEN @SinPagoInicial = 0 THEN 'PASS' ELSE 'FAIL' END, @SinPagoInicial
UNION ALL SELECT '13.6', 'Revertir pago (BD coherente)',
    'MANUAL', -1
UNION ALL SELECT '13.7', 'PDF historial = grid',
    'MANUAL', -1
UNION ALL SELECT '13.8', 'CRM reportes POS post-cobro',
    'MANUAL', -1
UNION ALL SELECT '13.9', '0 ventas financiadas huérfanas',
    CASE WHEN @Orphans = 0 THEN 'PASS' ELSE 'FAIL' END, @Orphans
UNION ALL SELECT '13.10', 'Atajos + buscadores sin stale',
    'MANUAL', -1;

PRINT 'SchemaVersion=' + CAST(@SchemaVersion AS NVARCHAR(10));
PRINT CASE WHEN @AutoPass = 1
    THEN 'F13 AUTO: PASS (manual 13.6-13.8, 13.10 pendiente operador)'
    ELSE 'F13 AUTO: FAIL — revisar filas FAIL arriba' END;
