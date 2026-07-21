-- ===============================================================
-- ACTUALIZAR PLANTILLAS DE MENSAJES SIN ACENTOS
-- Corrige problemas de codificación en WhatsApp
-- ===============================================================

USE [MF CYBER DB];
GO

PRINT '═══════════════════════════════════════════════════════════';
PRINT 'ACTUALIZANDO PLANTILLAS DE MENSAJES (Sin acentos)';
PRINT '═══════════════════════════════════════════════════════════';

-- 1. Mensaje de Pago de Membresía (sin acentos/tildes)
UPDATE [dbo].[MensajesAutomaticos]
SET [Plantilla] = 'Hola {CLIENTE}! Gracias por tu pago. Tu membresia ha sido activada. Fecha de vencimiento: {FECHA_VENCE}. Saludos, MF Fitness'
WHERE [Tipo] = 'PAGO_MEMBRESIA';

PRINT 'Plantilla PAGO_MEMBRESIA actualizada';

-- 2. Mensaje de Vencimiento Próximo (sin acentos/tildes)
UPDATE [dbo].[MensajesAutomaticos]
SET [Plantilla] = 'Hola {CLIENTE}! Tu membresia vence pronto el {FECHA_VENCE}. Renueva a tiempo para seguir disfrutando. Saludos, MF Fitness'
WHERE [Tipo] = 'VENCIMIENTO_PROXIMO';

PRINT 'Plantilla VENCIMIENTO_PROXIMO actualizada';

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT 'VERIFICACIÓN DE PLANTILLAS ACTUALIZADAS';
PRINT '═══════════════════════════════════════════════════════════';

SELECT 
	Tipo,
	Plantilla,
	Activa
FROM [dbo].[MensajesAutomaticos]
ORDER BY Tipo;

PRINT '';
PRINT '✅ Plantillas actualizadas sin acentos';
PRINT '📱 Los próximos mensajes se verán correctamente en WhatsApp';
PRINT '';
PRINT '🔄 Prueba nuevamente:';
PRINT '   1. Crea un nuevo pago de membresía';
PRINT '   2. Verifica que el mensaje llegue sin "Ã­" o caracteres extraños';
