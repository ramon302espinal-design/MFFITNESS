-- Crear tabla para almacenar plantillas de mensajes automáticos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MensajesAutomaticos]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MensajesAutomaticos] (
		[Id] INT PRIMARY KEY IDENTITY(1,1),
		[Tipo] NVARCHAR(50) NOT NULL, -- 'PAGO_MEMBRESIA', 'VENCIMIENTO_PROXIMO', etc.
		[Plantilla] NVARCHAR(MAX) NOT NULL, -- Texto del mensaje con variables {CLIENTE}, {FECHA_VENCE}, etc.
		[Activa] BIT DEFAULT 1,
		[FechaCreacion] DATETIME DEFAULT GETDATE(),
		[FechaModificacion] DATETIME DEFAULT GETDATE()
	);

	-- Insertar plantillas por defecto
	INSERT INTO [dbo].[MensajesAutomaticos] ([Tipo], [Plantilla], [Activa])
	VALUES 
	(
		'PAGO_MEMBRESIA',
		'¡Hola {CLIENTE}! 👋\n\nThanks for your payment! ✅\n\nYour membership has been activated.\n\n📅 *Expiration date:* {FECHA_VENCE}\n\nIf you have any questions, feel free to contact us.\n\n*MF Fitness* 💪',
		1
	),
	(
		'VENCIMIENTO_PROXIMO',
		'¡Hola {CLIENTE}! ⏰\n\nYour membership expires in *3 days*!\n\n📅 *Final date:* {FECHA_VENCE}\n\nDon''t miss the opportunity to renew your membership today.\n\n*MF Fitness* 💪',
		1
	);
END;

-- Crear tabla para registrar envíos de mensajes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegistroMensajes]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RegistroMensajes] (
		[Id] INT PRIMARY KEY IDENTITY(1,1),
		[ClienteId] INT NOT NULL,
		[Tipo] NVARCHAR(50) NOT NULL,
		[NumeroTelefono] NVARCHAR(20) NOT NULL,
		[Mensaje] NVARCHAR(MAX) NOT NULL,
		[Estado] NVARCHAR(20) DEFAULT 'PENDIENTE', -- 'PENDIENTE', 'ENVIADO', 'ERROR'
		[Respuesta] NVARCHAR(MAX) NULL,
		[FechaEnvio] DATETIME NULL,
		[FechaCreacion] DATETIME DEFAULT GETDATE(),
		CONSTRAINT FK_RegistroMensajes_Cliente FOREIGN KEY ([ClienteId]) REFERENCES [dbo].[Clientes]([Id])
	);
END;
