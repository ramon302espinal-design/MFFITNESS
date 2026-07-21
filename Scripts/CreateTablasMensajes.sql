IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MensajesAutomaticos]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MensajesAutomaticos] (
		[Id] INT PRIMARY KEY IDENTITY(1,1),
		[Tipo] NVARCHAR(50) NOT NULL,
		[Plantilla] NVARCHAR(MAX) NOT NULL,
		[Activa] BIT DEFAULT 1,
		[FechaCreacion] DATETIME DEFAULT GETDATE(),
		[FechaModificacion] DATETIME DEFAULT GETDATE()
	);

	INSERT INTO [dbo].[MensajesAutomaticos] ([Tipo], [Plantilla], [Activa])
	VALUES 
	(
		'PAGO_MEMBRESIA',
		'Hola {CLIENTE}! Tu membresía ha sido activada. Fecha de vencimiento: {FECHA_VENCE}',
		1
	),
	(
		'VENCIMIENTO_PROXIMO',
		'Hola {CLIENTE}! Tu membresía vence en 3 días. Fecha final: {FECHA_VENCE}',
		1
	);

	PRINT 'Tabla MensajesAutomaticos creada exitosamente.';
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegistroMensajes]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RegistroMensajes] (
		[Id] INT PRIMARY KEY IDENTITY(1,1),
		[ClienteId] INT NOT NULL,
		[Tipo] NVARCHAR(50) NOT NULL,
		[NumeroTelefono] NVARCHAR(20) NOT NULL,
		[Mensaje] NVARCHAR(MAX) NOT NULL,
		[Estado] NVARCHAR(20) DEFAULT 'PENDIENTE',
		[Respuesta] NVARCHAR(MAX) NULL,
		[FechaEnvio] DATETIME NULL,
		[FechaCreacion] DATETIME DEFAULT GETDATE(),
		CONSTRAINT FK_RegistroMensajes_Cliente FOREIGN KEY ([ClienteId]) REFERENCES [dbo].[Clientes]([Id])
	);

	CREATE INDEX IX_RegistroMensajes_Estado ON [dbo].[RegistroMensajes]([Estado]);
	CREATE INDEX IX_RegistroMensajes_ClienteId ON [dbo].[RegistroMensajes]([ClienteId]);

	PRINT 'Tabla RegistroMensajes creada exitosamente.';
END;

SELECT 'MensajesAutomaticos' AS Tabla, COUNT(*) AS Registros FROM [dbo].[MensajesAutomaticos]
UNION ALL
SELECT 'RegistroMensajes' AS Tabla, COUNT(*) AS Registros FROM [dbo].[RegistroMensajes];
