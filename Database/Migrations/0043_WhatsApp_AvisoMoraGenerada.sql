-- Aviso diario de mora generada (acumulado progresivo): plantilla UTILITY {DETALLE}.
-- Destino: SchemaVersion 43. Idempotente para PROD (MF CYBER DB) y DEV.

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'{DETALLE}',
    Activa = 1
WHERE Tipo = N'AVISO_MORA_GENERADA';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'AVISO_MORA_GENERADA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'AVISO_MORA_GENERADA',
        N'{DETALLE}',
        1,
        GETDATE());
END
GO
