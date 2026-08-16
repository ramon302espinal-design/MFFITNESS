/*
    Prueba del reverso de pago inicial al editar una deuda (rollback al final).
    Reproduce exactamente las sentencias de DeudaDAL.ActualizarDeudaFinanciamiento.

    Caso: Luis Mario financió un plan y su pago inicial se corrige de 1,000 a 2,000.
*/
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

DECLARE @DeudaId int = 2;
DECLARE @PagoInicialNuevo decimal(18,2) = 2000.00;
DECLARE @Usuario varchar(100) = 'TEST';
DECLARE @Metodo varchar(50) = 'Efectivo';

BEGIN TRAN;

DECLARE @ClienteId int, @Concepto varchar(200), @MontoTotal decimal(18,2),
        @MontoPagado decimal(18,2), @FechaCreacion datetime, @PlanId int,
        @MembresiaId int, @CajaId int;

SELECT @ClienteId = ClienteId, @Concepto = Concepto, @MontoTotal = MontoTotal,
       @MontoPagado = MontoPagado, @FechaCreacion = FechaCreacion,
       @PlanId = PlanId, @MembresiaId = MembresiaId
FROM Deudas WITH (UPDLOCK, ROWLOCK)
WHERE Id = @DeudaId;

SELECT TOP 1 @CajaId = Id FROM Caja WHERE Estado = 'ABIERTA' ORDER BY Id DESC;

DECLARE @PagoInicialAnterior decimal(18,2) =
(
    SELECT ISNULL(SUM(CASE WHEN TipoMovimiento = 'PAGO_INICIAL' THEN Monto ELSE -Monto END), 0)
    FROM HistorialDeudas
    WHERE DeudaId = @DeudaId
      AND TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL')
);

DECLARE @Total decimal(18,2) = @MontoTotal + @PagoInicialAnterior;
DECLARE @NuevoMontoTotal decimal(18,2) = @Total - @PagoInicialNuevo;
DECLARE @NuevoSaldo decimal(18,2) = @NuevoMontoTotal - @MontoPagado;
DECLARE @NuevoEstado varchar(20) = CASE WHEN @NuevoSaldo <= 0 THEN 'PAGADA' ELSE 'ACTIVA' END;

PRINT '--- ANTES ---';
SELECT @ClienteId AS ClienteId, @Total AS TotalFinanciado, @PagoInicialAnterior AS PagoInicialAnterior,
       @MontoTotal AS FinanciadoAntes, @MontoPagado AS Abonos, @CajaId AS CajaAbierta;

UPDATE Deudas
SET Concepto = @Concepto,
    MontoTotal = @NuevoMontoTotal,
    Saldo = @NuevoSaldo,
    FechaVencimiento = FechaVencimiento,
    PlanId = @PlanId,
    Estado = @NuevoEstado
WHERE Id = @DeudaId
  AND Estado = 'ACTIVA';

-- Reverso del inicial anterior (historial + egreso en caja)
INSERT INTO HistorialDeudas (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
VALUES (@DeudaId, @ClienteId, 'REVERSO_PAGO_INICIAL', @PagoInicialAnterior,
        'Reverso pago inicial por edicion', GETDATE(), @Usuario);

-- Concepto del reverso referido al ingreso original (así lo descuentan los paneles)
DECLARE @MovId int, @ConceptoOriginal varchar(200);

SELECT TOP 1 @MovId = dc.Id, @ConceptoOriginal = dc.Concepto
FROM DetalleCaja dc
WHERE dc.CajaId = @CajaId
  AND dc.TipoMovimiento = 'INGRESO'
  AND dc.ClienteId = @ClienteId
  AND ABS(dc.Monto - @PagoInicialAnterior) < 0.01
  AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
  AND NOT EXISTS (
      SELECT 1 FROM DetalleCaja rev
      WHERE rev.CajaId = dc.CajaId
        AND rev.TipoMovimiento = 'EGRESO'
        AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
  )
ORDER BY dc.Id DESC;

DECLARE @ConceptoReverso varchar(200) = ISNULL(
    'REVERSO (Ref #' + CONVERT(varchar(12), @MovId) + '): ' + @ConceptoOriginal,
    'REVERSO pago inicial - Deuda #2');

PRINT '--- INGRESO ORIGINAL LOCALIZADO EN CAJA ---';
SELECT @MovId AS MovimientoOriginal, @ConceptoReverso AS ConceptoReverso;

INSERT INTO DetalleCaja (CajaId, TipoMovimiento, Concepto, Monto, Fecha, Usuario, MetodoPago, ClienteId)
VALUES (@CajaId, 'EGRESO', @ConceptoReverso, @PagoInicialAnterior,
        GETDATE(), @Usuario, 'REVERSO', @ClienteId);

-- Nuevo inicial (historial + ingreso en caja)
INSERT INTO HistorialDeudas (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
VALUES (@DeudaId, @ClienteId, 'PAGO_INICIAL', @PagoInicialNuevo,
        'Pago inicial corregido', GETDATE(), @Usuario);

INSERT INTO DetalleCaja (CajaId, TipoMovimiento, Concepto, Monto, Fecha, Usuario, MetodoPago, ClienteId)
VALUES (@CajaId, 'INGRESO', 'Pago membresia - Pago inicial corregido (Deuda #2)', @PagoInicialNuevo,
        GETDATE(), @Usuario, @Metodo, @ClienteId);

-- Pago de membresía: se reemplaza el importe viejo (localizado por cliente/monto/fecha)
DECLARE @PagoId int =
(
    SELECT TOP 1 Id
    FROM Pagos
    WHERE ClienteId = @ClienteId
      AND ABS(Monto - @PagoInicialAnterior) < 0.01
      AND ABS(DATEDIFF(MINUTE, FechaPago, @FechaCreacion)) <= 10
    ORDER BY ABS(DATEDIFF(SECOND, FechaPago, @FechaCreacion)), Id DESC
);

PRINT '--- PAGO DE MEMBRESIA LOCALIZADO ---';
SELECT @PagoId AS PagoIdEncontrado;

UPDATE Pagos SET Monto = @PagoInicialNuevo, Usuario = @Usuario WHERE Id = @PagoId;

-- Historial de membresía alineado al nuevo inicial
UPDATE HistorialMembresias
SET Monto = @PagoInicialNuevo,
    Nota = 'Financiamiento - Inicial: $' + CONVERT(varchar(20), @PagoInicialNuevo)
         + ', Saldo: $' + CONVERT(varchar(20), @NuevoSaldo) + ' (corregido)'
WHERE Id =
(
    SELECT TOP 1 Id
    FROM HistorialMembresias
    WHERE ClienteId = @ClienteId
      AND TipoMovimiento = 'PAGO'
      AND ABS(Monto - @PagoInicialAnterior) < 0.01
      AND ABS(DATEDIFF(MINUTE, Fecha, @FechaCreacion)) <= 10
      AND (@PlanId IS NULL OR PlanId = @PlanId)
    ORDER BY ABS(DATEDIFF(SECOND, Fecha, @FechaCreacion)), Id DESC
);

INSERT INTO HistorialDeudas (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
VALUES (@DeudaId, @ClienteId, 'EDICION', @NuevoMontoTotal,
        'Edicion de deuda: Pago inicial: 1,000.00 -> 2,000.00 (reverso en caja)', GETDATE(), @Usuario);

PRINT '--- DESPUES: DEUDA ---';
SELECT Id, MontoTotal AS Financiado, MontoPagado AS Abonos, Saldo, Estado FROM Deudas WHERE Id = @DeudaId;

PRINT '--- DESPUES: PAGO INICIAL VIGENTE (debe ser 2000) ---';
SELECT ISNULL(SUM(CASE WHEN TipoMovimiento = 'PAGO_INICIAL' THEN Monto ELSE -Monto END), 0) AS PagoInicialVigente
FROM HistorialDeudas
WHERE DeudaId = @DeudaId AND TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL');

PRINT '--- DESPUES: HISTORIAL DE LA DEUDA ---';
SELECT Id, TipoMovimiento, Monto FROM HistorialDeudas WHERE DeudaId = @DeudaId ORDER BY Id;

PRINT '--- DESPUES: CAJA DEL DIA (neto debe subir 1000) ---';
SELECT TipoMovimiento, SUM(Monto) AS Total
FROM DetalleCaja
WHERE CajaId = @CajaId
GROUP BY TipoMovimiento;

PRINT '--- DESPUES: INGRESOS NETOS DEL PANEL (no debe contar el 1000 reversado) ---';
SELECT ISNULL(SUM(dc.Monto), 0) AS IngresosNetosHoy
FROM DetalleCaja dc
WHERE dc.TipoMovimiento = 'INGRESO'
  AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
  AND NOT EXISTS (
      SELECT 1 FROM DetalleCaja rev
      WHERE rev.CajaId = dc.CajaId
        AND rev.TipoMovimiento = 'EGRESO'
        AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
  )
  AND CAST(dc.Fecha AS DATE) = CAST(GETDATE() AS DATE);

PRINT '--- MOVIMIENTOS DEL CLIENTE EN CAJA ---';
SELECT Id, TipoMovimiento, Monto, LEFT(Concepto, 70) AS Concepto
FROM DetalleCaja
WHERE ClienteId = @ClienteId
ORDER BY Id;

PRINT '--- DESPUES: PAGO Y HISTORIAL DE MEMBRESIA ---';
SELECT Id, Monto, Concepto FROM Pagos WHERE ClienteId = @ClienteId ORDER BY Id;
SELECT Id, Monto, LEFT(Nota, 70) AS Nota FROM HistorialMembresias WHERE ClienteId = @ClienteId ORDER BY Id;

PRINT '--- DESPUES: LECTURA DEL GRID DE DEUDAS ---';
SELECT d.Id, d.MontoTotal, ISNULL(pi.PagoInicial, 0) AS PagoInicialFinanciamiento, d.Saldo
FROM Deudas d
OUTER APPLY (
    SELECT SUM(CASE WHEN h.TipoMovimiento = 'PAGO_INICIAL' THEN h.Monto ELSE -h.Monto END) AS PagoInicial
    FROM HistorialDeudas h
    WHERE h.DeudaId = d.Id
      AND h.TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL')
) pi
WHERE d.Id = @DeudaId;

ROLLBACK TRAN;
PRINT '--- ROLLBACK APLICADO: la base queda intacta ---';
