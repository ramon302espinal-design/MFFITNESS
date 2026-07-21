-- ===============================
-- ACTUALIZAR PLANTILLA DE DESACTIVACION
-- Texto limpio en español
-- ===============================

UPDATE [dbo].[MensajesAutomaticos]
SET [Plantilla] = N'Hola {CLIENTE}, tu membresia ha sido desactivada por la siguiente razon: {MOTIVO}. Si te detienes, tu progreso tambien y el camino hacia la meta se te hara mas largo. Te esperamos de vuelta pronto!',
	[Activa] = 1
WHERE [Tipo] = 'DESACTIVACION_MEMBRESIA';

PRINT 'Plantilla de DESACTIVACION actualizada correctamente';

-- Verificar el resultado
SELECT 
	Id,
	Tipo,
	Plantilla,
	Activa
FROM [dbo].[MensajesAutomaticos]
WHERE [Tipo] = 'DESACTIVACION_MEMBRESIA';

