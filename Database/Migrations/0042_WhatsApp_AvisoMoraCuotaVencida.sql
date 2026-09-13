-- Aviso al vencer la cuota con mora: plantilla UTILITY solo {DETALLE}.
-- Tipos: AVISO_MORA_CUOTA_VENCIDA (mora activa) y DEUDA_VENCE_HOY (sin mora).
-- Destino: SchemaVersion 42. Idempotente para PROD (MF CYBER DB) y DEV.

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'{DETALLE}',
    Activa = 1
WHERE Tipo = N'DEUDA_VENCE_HOY';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'DEUDA_VENCE_HOY')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'DEUDA_VENCE_HOY',
        N'{DETALLE}',
        1,
        GETDATE());
END
GO

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'{DETALLE}',
    Activa = 1
WHERE Tipo = N'AVISO_MORA_CUOTA_VENCIDA';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'AVISO_MORA_CUOTA_VENCIDA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'AVISO_MORA_CUOTA_VENCIDA',
        N'{DETALLE}',
        1,
        GETDATE());
END
GO
