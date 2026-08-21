-- Plantillas WhatsApp: congelacion / descongelacion de membresia.
-- Destino: SchemaVersion 17.
-- Idempotente: no duplica filas por Tipo.

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'CONGELACION_MEMBRESIA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'CONGELACION_MEMBRESIA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu membresia ha sido congelada.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Motivo:' + CHAR(13) + CHAR(10) + N'{MOTIVO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de congelacion:' + CHAR(13) + CHAR(10) + N'{FECHA_CONGELACION}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Puedes volver a entrar desde el dia {DIA_ANCLA} ({FECHA_REACTIVACION_DESDE}).' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Dias de membresia restantes al congelar:' + CHAR(13) + CHAR(10) + N'{DIAS_RESTANTES}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Vencimiento original del plan:' + CHAR(13) + CHAR(10) + N'{FECHA_FIN_ORIGINAL}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Pasa por recepcion para reactivar tu acceso cuando corresponda.',
        1,
        GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'DESCONGELACION_MEMBRESIA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'DESCONGELACION_MEMBRESIA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu membresia ha sido reactivada.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de activacion:' + CHAR(13) + CHAR(10) + N'{FECHA_ACTIVACION}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Nueva fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCE}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Ya puedes entrar nuevamente. Gracias por formar parte de MFFITNESS.',
        1,
        GETDATE());
END
GO
