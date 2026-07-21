-- ===============================================================
-- Fix error Twilio 21656: plantillas en una sola linea (sin \n)
-- Ejecutar en MF CYBER DB despues de Fix_Plantillas_WhatsApp.sql
-- ===============================================================

USE [MF CYBER DB];
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, se ha registrado una deuda a tu nombre en MFFITNESS. Concepto: {CONCEPTO}. Monto: {MONTO}. Vence: {FECHA_VENCIMIENTO}. Gracias por tu confianza, te esperamos.'
WHERE Tipo = 'DEUDA_CREADA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, recordatorio de MFFITNESS: Tu deuda vence en {DIAS_RESTANTES} dias ({FECHA_VENCIMIENTO}). Concepto: {CONCEPTO}. Saldo pendiente: {SALDO}. Te esperamos.'
WHERE Tipo = 'RECORDATORIO_VENCIMIENTO_DEUDA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, hoy vence tu deuda en MFFITNESS. Concepto: {CONCEPTO}. Saldo: {SALDO}. Fecha limite: {FECHA_VENCIMIENTO}. Te esperamos hoy.'
WHERE Tipo = 'DEUDA_VENCE_HOY';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, tu deuda en MFFITNESS ha vencido. Concepto: {CONCEPTO}. Saldo: {SALDO}. Vencia el: {FECHA_VENCIMIENTO}. Por favor acercate a saldarla. Gracias.'
WHERE Tipo = 'DEUDA_VENCIDA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Gracias {CLIENTE}! Pago recibido en MFFITNESS. Monto pagado: {MONTO_PAGO}. Saldo restante: {SALDO}.'
WHERE Tipo = 'PAGO_DEUDA_RECIBIDO';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'FELICIDADES {CLIENTE}! Deuda pagada completamente en MFFITNESS. Monto total: {MONTO_TOTAL}. Gracias por tu puntualidad.'
WHERE Tipo = 'DEUDA_PAGADA_COMPLETA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'MFFITNESS - Comprobante de pago. Cliente: {CLIENTE}. Plan: {PLAN}. Monto: {MONTO}. Metodo: {METODO_PAGO}. Fecha pago: {FECHA_PAGO}. Vence: {FECHA_VENCE}. Recibo: {NUMERO_RECIBO}.'
WHERE Tipo = 'FACTURA_MEMBRESIA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}! Tu membresia {PLAN} fue activada en MFFITNESS. Precio total: {PRECIO_TOTAL}. Pago inicial: {PAGO_INICIAL}. Saldo pendiente: {SALDO}. Fecha limite de pago: {FECHA_VENCIMIENTO}.'
WHERE Tipo = 'FINANCIAMIENTO';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}! Tu membresia en MFFITNESS vence el {FECHA_VENCE}. Renueva a tiempo para seguir entrenando.'
WHERE Tipo = 'VENCIMIENTO_PROXIMO';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}! Tu membresia {PLAN} en MFFITNESS vence HOY ({FECHA_VENCE}). Renueva hoy para no perder acceso.'
WHERE Tipo = 'VENCIMIENTO_HOY';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, tu membresia {PLAN} en MFFITNESS vencio el {FECHA_VENCE}. Motivo: {MOTIVO}. Renueva para volver a entrenar.'
WHERE Tipo = 'MEMBRESIA_VENCIDA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, tu membresia ha sido desactivada. Motivo: {MOTIVO}. Te esperamos de vuelta pronto en MFFITNESS.'
WHERE Tipo = 'DESACTIVACION_MEMBRESIA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'MFFITNESS - Mensaje de verificacion. Hola {CLIENTE}! Si recibes esto, WhatsApp funciona. Fecha: {FECHA}'
WHERE Tipo = 'PRUEBA_SISTEMA';
GO

UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}! Pago de membresia registrado en MFFITNESS. Tu plan vence el {FECHA_VENCE}. Gracias.'
WHERE Tipo = 'PAGO_MEMBRESIA';
GO

PRINT 'Plantillas WhatsApp actualizadas a una sola linea (fix 21656).';
GO
