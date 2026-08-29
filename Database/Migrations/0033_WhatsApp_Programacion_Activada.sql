-- WhatsApp: aviso cuando una programación pendiente se activa (inicio del periodo).
IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'PROGRAMACION_ACTIVADA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'PROGRAMACION_ACTIVADA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu membresia {PLAN} ya esta activa (renovacion programada).' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Inicio:' + CHAR(13) + CHAR(10) + N'{FECHA_INICIO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCE}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Ya puedes disfrutar tu plan. Gracias por formar parte de MFFITNESS.',
        1,
        GETDATE());
END
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MembresiasProgramadas')
   AND COL_LENGTH(N'dbo.MembresiasProgramadas', N'WhatsAppActivacionEnviada') IS NULL
BEGIN
    ALTER TABLE dbo.MembresiasProgramadas
        ADD WhatsAppActivacionEnviada BIT NOT NULL
            CONSTRAINT DF_MembProg_WaActiv DEFAULT(0);
END
GO
