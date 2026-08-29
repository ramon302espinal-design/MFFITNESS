/*
  FASE 12.3 — Gate pre-release (solo lectura).
  Ejecutar en DEV tras 1 venta financiada producto + 1 membresía de prueba.
  Repetir en PROD antes de declarar paridad.
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

PRINT '=== F12 GATE — ' + DB_NAME() + ' ===';

DECLARE @SchemaVersion INT = (SELECT ISNULL(MAX(Version), 0) FROM dbo.SchemaVersion);
PRINT 'SchemaVersion=' + CAST(@SchemaVersion AS NVARCHAR(10));

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
PRINT 'VentasFinanciadasHuerfanas=' + CAST(@Orphans AS NVARCHAR(10));

DECLARE @SinVentaId INT = (
    SELECT COUNT(*)
    FROM Deudas d
    WHERE d.Estado = 'ACTIVA' AND d.Saldo > 0
      AND d.MembresiaId IS NULL
      AND d.Concepto NOT LIKE '%(Venta Id %'
);
PRINT 'DeudasProductoSinVentaId=' + CAST(@SinVentaId AS NVARCHAR(10));

DECLARE @CajaAbierta INT = (SELECT COUNT(*) FROM Caja WHERE Estado = 'ABIERTA');
PRINT 'CajasAbiertas=' + CAST(@CajaAbierta AS NVARCHAR(10));

DECLARE @Pass BIT = CASE WHEN @Orphans = 0 THEN 1 ELSE 0 END;
PRINT CASE WHEN @Pass = 1 THEN 'GATE: PASS' ELSE 'GATE: FAIL — corregir antes de release' END;

SELECT
    @SchemaVersion AS SchemaVersion,
    @Orphans AS VentasFinanciadasHuerfanas,
    @SinVentaId AS DeudasProductoSinVentaId,
    @CajaAbierta AS CajasAbiertas,
    @Pass AS GatePass;
