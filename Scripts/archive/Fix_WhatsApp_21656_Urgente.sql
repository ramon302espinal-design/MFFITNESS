-- Limpia ContentSid viejos en BD (siempre usa App.config TwilioContentSidGenerico)
USE [MF CYBER DB];
GO

IF COL_LENGTH('dbo.MensajesAutomaticos', 'ContentSid') IS NOT NULL
BEGIN
    UPDATE dbo.MensajesAutomaticos SET ContentSid = NULL;
    PRINT 'ContentSid limpiado en MensajesAutomaticos. Se usara App.config.';
END
GO

-- Plantillas en una sola linea (sin saltos de linea ni emojis)
UPDATE MensajesAutomaticos
SET Plantilla = N'recordatorio de MFFITNESS: Tu deuda vence en {DIAS_RESTANTES} dias ({FECHA_VENCIMIENTO}). Concepto: {CONCEPTO}. Saldo pendiente: {SALDO}. Te esperamos.'
WHERE Tipo = 'RECORDATORIO_VENCIMIENTO_DEUDA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'se registro una deuda en MFFITNESS. Concepto: {CONCEPTO}. Monto: {MONTO}. Vence: {FECHA_VENCIMIENTO}.'
WHERE Tipo = 'DEUDA_CREADA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'hoy vence tu deuda en MFFITNESS. Concepto: {CONCEPTO}. Saldo: {SALDO}. Fecha limite: {FECHA_VENCIMIENTO}.'
WHERE Tipo = 'DEUDA_VENCE_HOY';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'tu deuda en MFFITNESS ha vencido. Concepto: {CONCEPTO}. Saldo: {SALDO}. Vencia el: {FECHA_VENCIMIENTO}. Acercate a saldarla.'
WHERE Tipo = 'DEUDA_VENCIDA';
GO

PRINT 'Fix WhatsApp 21656 aplicado en BD.';
GO
