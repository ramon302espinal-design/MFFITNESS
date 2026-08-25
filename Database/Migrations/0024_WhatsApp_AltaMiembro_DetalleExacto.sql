-- Detalle exacto ALTA_MIEMBRO_EXISTENTE (sin monto).
-- Destino: SchemaVersion 24.
-- El envoltorio Twilio agrega Miembro / Asunto / Fecha / pie automatico.
-- Texto sin tildes: WhatsAppContentVariableHelper.Sanitizar las elimina igual.

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'Se te ha agregado al sistema con la membresia {PLAN} ya pagada; su proxima fecha de pago sera para {FECHA_VENCE}. Gracias por formar parte de la familia MFFITNESS.',
    Activa = 1
WHERE Tipo = N'ALTA_MIEMBRO_EXISTENTE';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'ALTA_MIEMBRO_EXISTENTE')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'ALTA_MIEMBRO_EXISTENTE',
        N'Se te ha agregado al sistema con la membresia {PLAN} ya pagada; su proxima fecha de pago sera para {FECHA_VENCE}. Gracias por formar parte de la familia MFFITNESS.',
        1,
        GETDATE());
END
GO
