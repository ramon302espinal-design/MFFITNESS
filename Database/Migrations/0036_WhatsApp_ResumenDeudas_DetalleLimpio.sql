-- RESUMEN_DEUDAS: el envoltorio UTILITY Twilio ya pone Hola/Miembro/Asunto/Fecha.
-- La plantilla local solo aporta {DETALLE} limpio (sin anidar "estado de cuenta...").
-- Tambien repara mojibake UTF-8 tipico en Concepto (crÃ©dito).
-- Destino: SchemaVersion 36. Idempotente para PROD (MF CYBER DB) y DEV.

UPDATE dbo.MensajesAutomaticos
SET Plantilla = N'{DETALLE}',
    Activa = 1
WHERE Tipo = N'RESUMEN_DEUDAS';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'RESUMEN_DEUDAS')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'RESUMEN_DEUDAS',
        N'{DETALLE}',
        1,
        GETDATE());
END
GO

-- Mojibake comun: UTF-8 "crédito" leido como Latin1.
IF COL_LENGTH('dbo.Deudas', 'Concepto') IS NOT NULL
BEGIN
    UPDATE dbo.Deudas
    SET Concepto = REPLACE(Concepto, N'crÃ©dito', N'crédito')
    WHERE Concepto LIKE N'%crÃ©dito%';

    UPDATE dbo.Deudas
    SET Concepto = REPLACE(Concepto, N'CrÃ©dito', N'Crédito')
    WHERE Concepto LIKE N'%CrÃ©dito%';
END
GO

IF COL_LENGTH('dbo.HistorialDeudas', 'Descripcion') IS NOT NULL
BEGIN
    UPDATE dbo.HistorialDeudas
    SET Descripcion = REPLACE(Descripcion, N'crÃ©dito', N'crédito')
    WHERE Descripcion LIKE N'%crÃ©dito%';

    UPDATE dbo.HistorialDeudas
    SET Descripcion = REPLACE(Descripcion, N'CrÃ©dito', N'Crédito')
    WHERE Descripcion LIKE N'%CrÃ©dito%';
END
GO
