-- Chat WhatsApp: mensajes manuales e inbound (Fase 14).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ChatMensajes')
BEGIN
    CREATE TABLE dbo.ChatMensajes
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClienteId INT NOT NULL
            CONSTRAINT FK_ChatMensajes_Clientes REFERENCES dbo.Clientes(ID),
        Direccion NVARCHAR(10) NOT NULL,
        Cuerpo NVARCHAR(MAX) NOT NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_ChatMensajes_Estado DEFAULT (N'ENVIADO'),
        TwilioMessageSid NVARCHAR(64) NULL,
        Usuario NVARCHAR(100) NULL,
        Fecha DATETIME2 NOT NULL
            CONSTRAINT DF_ChatMensajes_Fecha DEFAULT (SYSDATETIME()),
        Leido BIT NOT NULL
            CONSTRAINT DF_ChatMensajes_Leido DEFAULT (0),
        DetalleError NVARCHAR(500) NULL
    );

    CREATE INDEX IX_ChatMensajes_Cliente_Fecha
        ON dbo.ChatMensajes (ClienteId, Fecha DESC);

    CREATE INDEX IX_ChatMensajes_Entrada_NoLeido
        ON dbo.ChatMensajes (ClienteId, Leido)
        WHERE Direccion = N'ENTRADA';
END
