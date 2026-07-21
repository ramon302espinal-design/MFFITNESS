-- Corregir plantillas (evitar RD$ en SQL que borra variables)
USE [MF CYBER DB];
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, recordatorio de MFFITNESS: Tu deuda vence en {DIAS_RESTANTES} dias ({FECHA_VENCIMIENTO}). Concepto: {CONCEPTO}. Saldo pendiente: {SALDO}. Te esperamos.'
WHERE Tipo = 'RECORDATORIO_VENCIMIENTO_DEUDA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, se registro una deuda en MFFITNESS. Concepto: {CONCEPTO}. Monto: {MONTO}. Vence: {FECHA_VENCIMIENTO}.'
WHERE Tipo = 'DEUDA_CREADA';
GO

IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'PRUEBA_SISTEMA')
BEGIN
    INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
    VALUES (
        'PRUEBA_SISTEMA',
        N'MFFITNESS - Mensaje de verificacion. Hola {CLIENTE}! Si recibes esto, WhatsApp funciona. Fecha: {FECHA}',
        1
    );
END
ELSE
BEGIN
    UPDATE MensajesAutomaticos
    SET Plantilla = N'MFFITNESS - Mensaje de verificacion. Hola {CLIENTE}! Si recibes esto, WhatsApp funciona. Fecha: {FECHA}',
        Activa = 1
    WHERE Tipo = 'PRUEBA_SISTEMA';
END
GO

PRINT 'Plantillas WhatsApp corregidas.';
