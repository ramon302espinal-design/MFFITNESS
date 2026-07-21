-- ===============================
-- Script para agregar plantilla de mensaje de desactivación
-- ===============================

-- Verificar si la tabla MensajesAutomaticos existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MensajesAutomaticos')
BEGIN
	PRINT 'ERROR: La tabla MensajesAutomaticos no existe en la base de datos.'
	PRINT 'Este script requiere que la tabla ya esté creada.'
END
ELSE
BEGIN
	-- Insertar la plantilla de desactivación si no existe
	IF NOT EXISTS (SELECT * FROM [dbo].[MensajesAutomaticos] WHERE [Tipo] = 'DESACTIVACION_MEMBRESIA')
	BEGIN
		INSERT INTO [dbo].[MensajesAutomaticos] ([Tipo], [Plantilla], [Activa])
		VALUES (
			'DESACTIVACION_MEMBRESIA',
			N'Hola {CLIENTE}, tu membresía ha sido desactivada por la siguiente razón: {MOTIVO}. Si te detienes, tu progreso también y el camino hacia la meta se te hará más largo. Te esperamos de vuelta pronto!',
			1
		);
		PRINT 'Plantilla de desactivación agregada correctamente.'
	END
	ELSE
	BEGIN
		-- Actualizar la plantilla si ya existe
		UPDATE [dbo].[MensajesAutomaticos]
		SET [Plantilla] = N'Hola {CLIENTE}, tu membresía ha sido desactivada por la siguiente razón: {MOTIVO}. Si te detienes, tu progreso también y el camino hacia la meta se te hará más largo. Te esperamos de vuelta pronto!',
			[Activa] = 1
		WHERE [Tipo] = 'DESACTIVACION_MEMBRESIA';
		PRINT 'Plantilla de desactivación actualizada correctamente.'
	END
END

-- Consultar todas las plantillas para verificación
SELECT * FROM [dbo].[MensajesAutomaticos];
