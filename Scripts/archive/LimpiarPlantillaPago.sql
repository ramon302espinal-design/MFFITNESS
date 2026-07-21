-- ===============================
-- ACTUALIZAR PLANTILLA DE PAGO DE MEMBRESIA
-- Texto limpio en español
-- ===============================

UPDATE [dbo].[MensajesAutomaticos]
SET [Plantilla] = N'EN HORA BUENA! Hola {CLIENTE}! Gracias por tu pago. Tu membresia ha sido activada. Fecha de vencimiento: {FECHA_VENCE}. BIENVENIDO A MFFITNESS',
	[Activa] = 1
WHERE [Tipo] = 'PAGO_MEMBRESIA';

PRINT 'Plantilla de PAGO actualizada correctamente';

-- Verificar el resultado
SELECT 
	Id,
	Tipo,
	Plantilla,
	Activa
FROM [dbo].[MensajesAutomaticos]
WHERE [Tipo] = 'PAGO_MEMBRESIA';
