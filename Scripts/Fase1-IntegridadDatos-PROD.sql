/*
  FASE 1 — Integridad de datos PROD (MF CYBER DB)
  Repara vínculo venta↔deuda, conceptos legacy y MetodoPago financiado.
  Idempotente: solo aplica filas que aún no tienen (Venta Id N).
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
USE [MF CYBER DB];

BEGIN TRANSACTION;

BEGIN TRY
    -- 1.1 / 1.4 — Deuda #3004 ↔ Venta #17012 (mismo cliente/monto/fecha; faltaba Venta Id)
    IF NOT EXISTS (SELECT 1 FROM Deudas WHERE Id = 3004 AND Concepto LIKE '%Venta Id 17012%')
    BEGIN
        UPDATE Deudas
        SET Concepto = N'Venta a crédito: VIVE100 VERDE (RD$ 75.00) (Venta Id 17012)'
        WHERE Id = 3004;

        UPDATE HistorialDeudas
        SET Descripcion = N'Venta a crédito: VIVE100 VERDE (RD$ 75.00) (Venta Id 17012) | Total: 75.00 | Pago inicial: 0.00 | Saldo pendiente: 75.00 | Fecha límite: 31/08/2026'
        WHERE DeudaId = 3004 AND TipoMovimiento = 'DEUDA';
    END

    -- 1.4 — Producto a crédito activo: enlazar ventas emparejadas por cliente/monto/fecha
    IF NOT EXISTS (SELECT 1 FROM Deudas WHERE Id = 2002 AND Concepto LIKE '%Venta Id%')
    BEGIN
        UPDATE Deudas SET Concepto = N'Venta a crédito: Cool Heaven (RD$ 25.00) (Venta Id 13007)' WHERE Id = 2002;
        UPDATE HistorialDeudas SET Descripcion = N'Venta a crédito: Cool Heaven (RD$ 25.00) (Venta Id 13007) | Total: 25.00 | Pago inicial: 0.00 | Saldo pendiente: 25.00 | Fecha límite: 28/08/2026'
        WHERE DeudaId = 2002 AND TipoMovimiento = 'DEUDA';
    END

    IF NOT EXISTS (SELECT 1 FROM Deudas WHERE Id = 2003 AND Concepto LIKE '%Venta Id%')
    BEGIN
        UPDATE Deudas SET Concepto = N'Venta a crédito: WALA ENERGY, Cool Heaven x2 (RD$ 150.00) (Venta Id 13008)' WHERE Id = 2003;
        UPDATE HistorialDeudas SET Descripcion = N'Venta a crédito: WALA ENERGY, Cool Heaven x2 (RD$ 150.00) (Venta Id 13008) | Total: 150.00 | Pago inicial: 0.00 | Saldo pendiente: 150.00 | Fecha límite: 29/08/2026'
        WHERE DeudaId = 2003 AND TipoMovimiento = 'DEUDA';
    END

    IF NOT EXISTS (SELECT 1 FROM Deudas WHERE Id = 2004 AND Concepto LIKE '%Venta Id%')
    BEGIN
        UPDATE Deudas SET Concepto = N'Venta a crédito: Cool Heaven (RD$ 25.00) (Venta Id 16012)' WHERE Id = 2004;
        UPDATE HistorialDeudas SET Descripcion = N'Venta a crédito: Cool Heaven (RD$ 25.00) (Venta Id 16012) | Total: 25.00 | Pago inicial: 0.00 | Saldo pendiente: 25.00 | Fecha límite: 31/08/2026'
        WHERE DeudaId = 2004 AND TipoMovimiento = 'DEUDA';
    END

    -- 1.4 — Deudas pagadas (trazabilidad historial)
    IF NOT EXISTS (SELECT 1 FROM Deudas WHERE Id = 1001 AND Concepto LIKE '%Venta Id%')
    BEGIN
        UPDATE Deudas SET Concepto = N'1 Cool Heaven a credito (Venta Id 1004)' WHERE Id = 1001;
        UPDATE HistorialDeudas SET Descripcion = N'1 Cool Heaven a credito (Venta Id 1004) | Total: 25.00 | Pago inicial: 0.00 | Saldo pendiente: 25.00 | Fecha límite: 26/09/2026'
        WHERE DeudaId = 1001 AND TipoMovimiento = 'DEUDA';
    END

    IF NOT EXISTS (SELECT 1 FROM Deudas WHERE Id = 2001 AND Concepto LIKE '%Venta Id%')
    BEGIN
        UPDATE Deudas SET Concepto = N'Venta a crédito: Cool Heaven (RD$ 25.00) (Venta Id 13006)' WHERE Id = 2001;
        UPDATE HistorialDeudas SET Descripcion = N'Venta a crédito: Cool Heaven (RD$ 25.00) (Venta Id 13006) | Total: 25.00 | Pago inicial: 0.00 | Saldo pendiente: 25.00 | Fecha límite: 28/08/2026'
        WHERE DeudaId = 2001 AND TipoMovimiento = 'DEUDA';
    END

    -- 1.10 — MetodoPago coherente en ventas a crédito (Credito → Financiado)
    UPDATE Ventas
    SET MetodoPago = N'Financiado'
    WHERE Id IN (13006, 13007, 13008, 16012)
      AND MetodoPago IN (N'Credito', N'Crédito', N'credito');

    COMMIT TRANSACTION;
    PRINT 'Fase 1 PROD: cambios aplicados correctamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- Verificación post-fix (patrón exacto Venta Id)
SELECT 'ORPHAN_FINANCED' AS CheckType, COUNT(*) AS Cnt
FROM Ventas v
WHERE (v.MetodoPago LIKE N'%inanc%' OR v.MetodoPago LIKE N'%Financ%')
  AND NOT EXISTS (
      SELECT 1 FROM Deudas d
      WHERE d.Concepto LIKE N'%(Venta Id ' + CAST(v.Id AS VARCHAR(20)) + N')%');

SELECT 'SALDO_MISMATCH_ACTIVAS' AS CheckType, COUNT(*) AS Cnt
FROM Ventas v
INNER JOIN Deudas d ON d.Concepto LIKE N'%(Venta Id ' + CAST(v.Id AS VARCHAR(20)) + N')%'
WHERE d.Estado = N'ACTIVA'
  AND ABS((v.Total - v.MontoPagado) - d.Saldo) > 0.01;

SELECT 'PAGOS_MISMATCH' AS CheckType, COUNT(*) AS Cnt
FROM Deudas d
LEFT JOIN (SELECT DeudaId, SUM(Monto) Suma FROM PagosDeuda WHERE Estado = N'ACTIVO' GROUP BY DeudaId) p ON p.DeudaId = d.Id
WHERE d.Estado = N'ACTIVA'
  AND ABS(d.MontoPagado - ISNULL(p.Suma, 0)) > 0.01;

SELECT 'LEGACY_SIN_VENTA_ID' AS CheckType, COUNT(*) AS Cnt
FROM Deudas d
WHERE d.MembresiaId IS NULL
  AND (d.Concepto LIKE N'%credito%' OR d.Concepto LIKE N'%crédito%' OR d.Concepto LIKE N'%Venta%')
  AND d.Concepto NOT LIKE N'%Venta Id%';
