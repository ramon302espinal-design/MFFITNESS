-- Comprobante de abono WhatsApp: plantilla UTILITY solo {DETALLE}.
-- Destino: SchemaVersion 45. Idempotente PROD (MF CYBER DB) y DEV.

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'{DETALLE}',
    Activa = 1
WHERE Tipo = N'COMPROBANTE_ABONO_DEUDA';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'COMPROBANTE_ABONO_DEUDA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'COMPROBANTE_ABONO_DEUDA',
        N'{DETALLE}',
        1,
        GETDATE());
END
GO
