-- ===============================
-- Script para actualizar plantilla de mensaje de pago de membresía
-- ===============================

-- Actualizar la plantilla de pago de membresía
UPDATE [dbo].[MensajesAutomaticos]
SET [Plantilla] = N'¡EN HORA BUENA! Hola {CLIENTE}! Gracias por tu pago. Tu membresia ha sido activada. Fecha de vencimiento: {FECHA_VENCE}. BIENVENIDO A MFFITNESS',
	[Activa] = 1
WHERE [Tipo] = 'PAGO_MEMBRESIA';

PRINT 'Plantilla de pago de membresía actualizada correctamente.'

-- Consultar la plantilla actualizada para verificación
SELECT * FROM [dbo].[MensajesAutomaticos] WHERE [Tipo] = 'PAGO_MEMBRESIA';
