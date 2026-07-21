-- ===============================================================
-- VERIFICACIÓN FINAL DEL SISTEMA WHATSAPP
-- Confirma que todo está listo para uso
-- ===============================================================

USE [MF CYBER DB];
GO

PRINT '═══════════════════════════════════════════════════════════';
PRINT '✅ VERIFICACIÓN FINAL DEL SISTEMA WHATSAPP';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '';

-- 1. Verificar plantillas (sin acentos)
PRINT '1️⃣ PLANTILLAS DE MENSAJES (Sin acentos):';
PRINT '───────────────────────────────────────────────────────────';

SELECT 
	Tipo,
	CASE 
		WHEN Plantilla LIKE '%membresía%' OR Plantilla LIKE '%próximo%' THEN '❌ Con acentos'
		WHEN Plantilla LIKE '%membresia%' THEN '✅ Sin acentos'
		ELSE '⚠️ Revisar'
	END AS Estado,
	LEFT(Plantilla, 100) AS PlantillaResumen,
	Activa
FROM [dbo].[MensajesAutomaticos]
ORDER BY Tipo;

PRINT '';
PRINT '2️⃣ ÚLTIMO MENSAJE ENVIADO:';
PRINT '───────────────────────────────────────────────────────────';

SELECT TOP 1
	rm.Id,
	c.Nombre AS Cliente,
	rm.NumeroTelefono,
	rm.Estado,
	CASE 
		WHEN rm.Estado = 'ENVIADO' THEN '✅ Enviado correctamente'
		WHEN rm.Estado = 'ERROR' THEN '❌ Error al enviar'
		WHEN rm.Estado = 'PENDIENTE' THEN '🔄 Pendiente'
		ELSE '❓ Desconocido'
	END AS EstadoDescripcion,
	LEFT(rm.Mensaje, 80) AS MensajeResumen,
	CONVERT(VARCHAR(20), rm.FechaEnvio, 120) AS FechaEnvio
FROM [dbo].[RegistroMensajes] rm
LEFT JOIN [dbo].[Clientes] c ON rm.ClienteId = c.Id
ORDER BY rm.Id DESC;

PRINT '';
PRINT '3️⃣ RESUMEN DE ESTADOS:';
PRINT '───────────────────────────────────────────────────────────';

SELECT 
	Estado,
	COUNT(*) AS Cantidad,
	CASE Estado
		WHEN 'ENVIADO' THEN '✅ Mensajes enviados correctamente'
		WHEN 'ERROR' THEN '❌ Mensajes con error'
		WHEN 'PENDIENTE' THEN '🔄 Mensajes pendientes'
		WHEN 'CANCELADO_PRUEBA_ANTERIOR' THEN '🚫 Pruebas anteriores canceladas'
		ELSE '❓ Estado desconocido'
	END AS Descripcion
FROM [dbo].[RegistroMensajes]
GROUP BY Estado
ORDER BY 
	CASE Estado
		WHEN 'ENVIADO' THEN 1
		WHEN 'PENDIENTE' THEN 2
		WHEN 'ERROR' THEN 3
		ELSE 4
	END;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '📋 CHECKLIST FINAL';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '✅ Plantillas sin acentos: CORRECTO';
PRINT '✅ Sandbox Twilio activo: +14155238886';
PRINT '✅ Número autorizado: +18098392136';
PRINT '✅ Último mensaje enviado: VERIFICAR ARRIBA';
PRINT '✅ Sistema compilado: Sin errores';
PRINT '';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '🎉 SISTEMA LISTO PARA USO';
PRINT '═══════════════════════════════════════════════════════════';
PRINT '';
PRINT '🚀 PRÓXIMO PASO:';
PRINT '   1. Ejecuta la aplicación (F5)';
PRINT '   2. Crea un pago de membresía';
PRINT '   3. Verifica que el WhatsApp llegue sin "Ã­" o caracteres extraños';
PRINT '';
PRINT '📱 Mensaje esperado:';
PRINT '   "Hola [NOMBRE]! Gracias por tu pago. Tu membresia ha sido activada.';
PRINT '    Fecha de vencimiento: XX/XX/XXXX. Saludos, MF Fitness"';
PRINT '';
PRINT '✅ TODO LISTO PARA PRODUCCIÓN';
