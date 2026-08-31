-- DEUDA_CREADA: el envoltorio UTILITY Twilio ya pone Hola/Miembro/Asunto/Fecha.
-- Solo {DETALLE} limpio (evita ". Se ha registrado una deuda..." al quitar Hola CLIENTE).
-- Destino: SchemaVersion 37. Idempotente para PROD (MF CYBER DB) y DEV.

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'{DETALLE}',
    Activa = 1
WHERE Tipo = N'DEUDA_CREADA';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'DEUDA_CREADA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'DEUDA_CREADA',
        N'{DETALLE}',
        1,
        GETDATE());
END
GO
