-- ===============================================================
-- SCRIPT: Prueba de Envío de WhatsApp
-- Limpia mensajes antiguos y prepara para nueva prueba
-- ===============================================================

USE [MF CYBER DB];
GO

-- 1. Ver mensajes actuales
SELECT 
	Id,
	ClienteId,
	TipoMensaje,
	NumeroTelefono,
	Estado,
	FechaEnvio,
	RespuestaApi
FROM [dbo].[RegistroMensajes]
ORDER BY FechaEnvio DESC;

-- 2. Limpiar mensajes de prueba anteriores (OPCIONAL - descomenta si quieres limpiar)
-- DELETE FROM [dbo].[RegistroMensajes];

-- 3. Verificar cliente de prueba
SELECT TOP 1
	Id,
	Nombre,
	Telefono
FROM [dbo].[Clientes]
WHERE Telefono LIKE '%8098392136%'
ORDER BY Id DESC;
