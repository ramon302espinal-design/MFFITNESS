-- Avisos automaticos de membresia: 10 dias, 3 dias y diario al vencer.
-- Actualiza plantillas con variables DIAS_RESTANTES / SALDO / DETALLE_DEUDA.
-- Destino: SchemaVersion 7.

IF EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'VENCIMIENTO_PROXIMO')
BEGIN
    UPDATE dbo.MensajesAutomaticos
    SET Plantilla =
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Te recordamos que tu membresia {PLAN} vence en {DIAS_RESTANTES} dia(s).' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCE}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Renueva antes de esa fecha para no interrumpir tu acceso.',
        FechaModificacion = GETDATE()
    WHERE Tipo = N'VENCIMIENTO_PROXIMO';
END
ELSE
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'VENCIMIENTO_PROXIMO',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Te recordamos que tu membresia {PLAN} vence en {DIAS_RESTANTES} dia(s).' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCE}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Renueva antes de esa fecha para no interrumpir tu acceso.',
        1,
        GETDATE());
END
GO

IF EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'MEMBRESIA_VENCIDA')
BEGIN
    UPDATE dbo.MensajesAutomaticos
    SET Plantilla =
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu membresia {PLAN} esta vencida desde el {FECHA_VENCE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'{MOTIVO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Detalle:' + CHAR(13) + CHAR(10) + N'{DETALLE_DEUDA}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Saldo total pendiente:' + CHAR(13) + CHAR(10) + N'{SALDO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Pasa por recepcion a saldar y reactivar tu acceso.',
        FechaModificacion = GETDATE()
    WHERE Tipo = N'MEMBRESIA_VENCIDA';
END
ELSE
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'MEMBRESIA_VENCIDA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu membresia {PLAN} esta vencida desde el {FECHA_VENCE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'{MOTIVO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Detalle:' + CHAR(13) + CHAR(10) + N'{DETALLE_DEUDA}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Saldo total pendiente:' + CHAR(13) + CHAR(10) + N'{SALDO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Pasa por recepcion a saldar y reactivar tu acceso.',
        1,
        GETDATE());
END
GO
