-- Centro de conversaciones: origen del mensaje y configuración de automatizaciones.
-- No reenvía historial. No toca caja, membresías ni deudas.
-- Destino: SchemaVersion 46. Idempotente.

IF COL_LENGTH(N'dbo.ChatMensajes', N'Origen') IS NULL
    ALTER TABLE dbo.ChatMensajes ADD Origen NVARCHAR(20) NULL;
GO

IF COL_LENGTH(N'dbo.ChatMensajes', N'ContentSid') IS NULL
    ALTER TABLE dbo.ChatMensajes ADD ContentSid NVARCHAR(64) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'WhatsAppAutomatizacionConfig')
BEGIN
    CREATE TABLE dbo.WhatsAppAutomatizacionConfig
    (
        Tipo NVARCHAR(50) NOT NULL PRIMARY KEY,
        Activa BIT NOT NULL
            CONSTRAINT DF_WAAuto_Activa DEFAULT (1),
        Evento NVARCHAR(160) NOT NULL,
        Condicion NVARCHAR(300) NULL,
        DiasAntes INT NULL,
        EvitarDuplicadoHoras INT NOT NULL
            CONSTRAINT DF_WAAuto_Dedup DEFAULT (24),
        UltimoResultado NVARCHAR(500) NULL,
        UltimaEjecucion DATETIME2 NULL,
        FechaModificacion DATETIME2 NOT NULL
            CONSTRAINT DF_WAAuto_Mod DEFAULT (SYSDATETIME())
    );
END
GO

-- Alta de tipos solo si no existen. Activa=1 y DiasAntes NULL conservan el comportamiento actual.
INSERT INTO dbo.WhatsAppAutomatizacionConfig (Tipo, Activa, Evento, Condicion, DiasAntes, EvitarDuplicadoHoras)
SELECT v.Tipo, 1, v.Evento, v.Condicion, NULL, 24
FROM (VALUES
    (N'VENCIMIENTO_PROXIMO', N'FechaFin de la membresía activa', N'N días antes. Un aviso por día y membresía.'),
    (N'MEMBRESIA_VENCIDA', N'Membresía vencida y aún activa', N'Aviso diario. No cambia el acceso.'),
    (N'RECORDATORIO_VENCIMIENTO_DEUDA', N'Deuda activa con saldo', N'N días antes del pago.'),
    (N'DEUDA_CREADA', N'Se registra una deuda o préstamo', N'Al crear la venta a crédito.'),
    (N'PAGO_MEMBRESIA', N'Pago de membresía registrado', N'Cada pago nuevo.'),
    (N'PROGRAMACION_ACTIVADA', N'Se activa una membresía programada', N'Al activar la programación ya pagada.')
) v(Tipo, Evento, Condicion)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.WhatsAppAutomatizacionConfig c WHERE c.Tipo = v.Tipo);
GO
