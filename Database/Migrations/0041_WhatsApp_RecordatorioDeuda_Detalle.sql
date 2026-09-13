-- RECORDATORIO_VENCIMIENTO_DEUDA: envoltorio UTILITY Twilio (Hola/Miembro/Asunto/Fecha).
-- Solo {DETALLE}: dias restantes, concepto, saldo, cuota y fecha de pago.
-- Destino: SchemaVersion 41. Idempotente para PROD (MF CYBER DB) y DEV.

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'{DETALLE}',
    Activa = 1
WHERE Tipo = N'RECORDATORIO_VENCIMIENTO_DEUDA';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'RECORDATORIO_VENCIMIENTO_DEUDA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'RECORDATORIO_VENCIMIENTO_DEUDA',
        N'{DETALLE}',
        1,
        GETDATE());
END
GO
