-- ===============================================================
-- VERIFICACIÓN FINAL DEL SISTEMA WHATSAPP
-- Ejecuta este script para ver el estado completo
-- ===============================================================

USE [MF CYBER DB];
GO

PRINT '═══════════════════════════════════════════════════════════';
PRINT '1. VERIFICACIÓN DE PLANTILLAS DE MENSAJES';
PRINT '═══════════════════════════════════════════════════════════';

SELECT 
	Tipo,
	Activa,
	LEFT(Plantilla, 100) AS PlantillaResumen
FROM [dbo].[MensajesAutomaticos]
ORDER BY Tipo;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '2. CLIENTES DE PRUEBA CON TELÉFONO';
PRINT '═══════════════════════════════════════════════════════════';

SELECT TOP 5
	Id,
	Nombre,
	Telefono,
	CASE 
		WHEN Telefono LIKE '+%' THEN '✅ Con prefijo internacional'
		WHEN Telefono IS NULL THEN '❌ Sin teléfono'
		ELSE '⚠️ Sin prefijo (se normalizará automáticamente)'
	END AS EstadoFormato
FROM [dbo].[Clientes]
WHERE Telefono IS NOT NULL
ORDER BY Id DESC;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '3. REGISTRO DE MENSAJES (ÚLTIMOS 10)';
PRINT '═══════════════════════════════════════════════════════════';

SELECT TOP 10
	Id,
	ClienteId,
	Tipo,
	NumeroTelefono,
	Estado,
	CONVERT(VARCHAR(20), FechaCreacion, 120) AS FechaCreacion,
	CONVERT(VARCHAR(20), FechaEnvio, 120) AS FechaEnvio,
	LEFT(ISNULL(Respuesta, 'Sin respuesta'), 50) AS RespuestaResumen
FROM [dbo].[RegistroMensajes]
ORDER BY Id DESC;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '4. RESUMEN DE ESTADOS';
PRINT '═══════════════════════════════════════════════════════════';

SELECT 
	Estado,
	COUNT(*) AS Cantidad,
	CASE Estado
		WHEN 'ENVIADO' THEN '✅ Mensajes enviados correctamente'
		WHEN 'PENDIENTE' THEN '🔄 Mensajes en cola'
		WHEN 'ERROR' THEN '❌ Mensajes con error'
		WHEN 'CANCELADO' THEN '🚫 Mensajes cancelados'
		ELSE '❓ Estado desconocido'
	END AS Descripcion
FROM [dbo].[RegistroMensajes]
GROUP BY Estado
ORDER BY 
	CASE Estado
		WHEN 'PENDIENTE' THEN 1
		WHEN 'ENVIADO' THEN 2
		WHEN 'ERROR' THEN 3
		ELSE 4
	END;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '5. MENSAJES CON ERROR (ÚLTIMOS 5)';
PRINT '═══════════════════════════════════════════════════════════';

SELECT TOP 5
	rm.Id,
	c.Nombre AS Cliente,
	rm.NumeroTelefono,
	rm.Tipo,
	rm.Respuesta AS DetalleError,
	CONVERT(VARCHAR(20), rm.FechaCreacion, 120) AS FechaCreacion
FROM [dbo].[RegistroMensajes] rm
INNER JOIN [dbo].[Clientes] c ON rm.ClienteId = c.Id
WHERE rm.Estado = 'ERROR'
ORDER BY rm.Id DESC;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '6. VERIFICACIÓN DE CONFIGURACIÓN ESPERADA';
PRINT '═══════════════════════════════════════════════════════════';

PRINT 'Modo produccion: numero WhatsApp Business (+14247284594 u otro aprobado en Twilio)';
PRINT 'Formato FROM: whatsapp:+1XXXXXXXXXX';
PRINT 'Formato TO: whatsapp:+1XXXXXXXXXX (E.164 con prefijo +1)';
PRINT 'Plantilla generica: TwilioContentSidGenerico con cuerpo {{1}} en estado Approved';
PRINT '';
PRINT 'Consola Twilio Content Templates:';
PRINT '   https://console.twilio.com/us1/develop/sms/content-template-builder';

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT 'VERIFICACIÓN COMPLETA';
PRINT '═══════════════════════════════════════════════════════════';
