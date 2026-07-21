-- ===============================
-- Script final para actualizar AMBAS plantillas con codificación correcta
-- ===============================

-- Actualizar plantilla de PAGO DE MEMBRESÍA
UPDATE [dbo].[MensajesAutomaticos]
SET [Plantilla] = N'¡EN HORA BUENA! Hola {CLIENTE}! Gracias por tu pago. Tu membresia ha sido activada. Fecha de vencimiento: {FECHA_VENCE}. BIENVENIDO A MFFITNESS',
	[Activa] = 1
WHERE [Tipo] = 'PAGO_MEMBRESIA';

PRINT '✓ Plantilla de PAGO actualizada';

-- Actualizar plantilla de DESACTIVACIÓN
UPDATE [dbo].[MensajesAutomaticos]
SET [Plantilla] = N'Hola {CLIENTE}, tu membresía ha sido desactivada por la siguiente razón: {MOTIVO}. Si te detienes, tu progreso también y el camino hacia la meta se te hará más largo. Te esperamos de vuelta pronto!',
	[Activa] = 1
WHERE [Tipo] = 'DESACTIVACION_MEMBRESIA';

PRINT '✓ Plantilla de DESACTIVACIÓN actualizada';

-- Mostrar resultado
PRINT '';
PRINT '=== PLANTILLAS ACTUALIZADAS ===';
PRINT '';

SELECT 
	Id,
	Tipo,
	LEFT(Plantilla, 100) + '...' AS [Vista_Previa],
	Activa
FROM [dbo].[MensajesAutomaticos]
ORDER BY Id;
