-- ===============================================================
-- PREPARACIÓN PARA PRUEBA CON SANDBOX ACTIVO
-- Limpia mensajes antiguos y prepara para nueva prueba exitosa
-- ===============================================================

USE [MF CYBER DB];
GO

PRINT '═══════════════════════════════════════════════════════════';
PRINT 'PASO 1: Estado actual de mensajes';
PRINT '═══════════════════════════════════════════════════════════';

SELECT 
	Estado,
	COUNT(*) AS Cantidad
FROM [dbo].[RegistroMensajes]
GROUP BY Estado;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT 'PASO 2: Limpiar mensajes de error antiguos';
PRINT '═══════════════════════════════════════════════════════════';

-- Marcar los errores antiguos como CANCELADO para mantener historial
UPDATE [dbo].[RegistroMensajes]
SET Estado = 'CANCELADO_PRUEBA_ANTERIOR',
	Respuesta = 'Mensaje cancelado - Prueba con número incorrecto antes de sandbox'
WHERE Estado = 'ERROR'
  AND Respuesta LIKE '%Error al enviar por Twilio%'
  AND FechaCreacion < GETDATE();

PRINT CONCAT('Mensajes de error antiguos marcados como CANCELADO: ', @@ROWCOUNT);

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT 'PASO 3: Verificar clientes de prueba';
PRINT '═══════════════════════════════════════════════════════════';

SELECT 
	Id,
	Nombre,
	Telefono,
	CASE 
		WHEN Telefono LIKE '+%' THEN '✅ Formato internacional correcto'
		WHEN Telefono IS NOT NULL THEN '⚠️ Se normalizará a +1' + REPLACE(REPLACE(Telefono, '-', ''), ' ', '')
		ELSE '❌ Sin teléfono'
	END AS EstadoFormato
FROM [dbo].[Clientes]
WHERE Id IN (1, 5008)
   OR Telefono LIKE '%8098392136%';

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT 'PASO 4: Configuración actual esperada';
PRINT '═══════════════════════════════════════════════════════════';

PRINT 'Número FROM (Sandbox Twilio): whatsapp:+14155238886 ✅';
PRINT 'Número TO (Tu teléfono): whatsapp:+18098392136 ✅';
PRINT 'Account SID: AC_TU_TWILIO_ACCOUNT_SID_AQUI ✅';
PRINT '';
PRINT '✅ Sandbox activado: Confirmado por usuario';
PRINT '✅ Código configurado correctamente';
PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT 'LISTO PARA PRUEBA';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '';
PRINT '🚀 SIGUIENTE PASO:';
PRINT '   1. Ejecuta la aplicación (F5 en Visual Studio)';
PRINT '   2. Abre Output window (View → Output → Debug)';
PRINT '   3. Crea un pago de membresía para cualquier cliente';
PRINT '   4. Observa los logs en Output';
PRINT '   5. Verifica que llegue WhatsApp a tu teléfono +18098392136';
PRINT '';
PRINT '📊 PARA VER RESULTADOS DESPUÉS:';
PRINT '   SELECT TOP 5 * FROM RegistroMensajes ORDER BY Id DESC;';
