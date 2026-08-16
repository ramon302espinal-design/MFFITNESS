-- Plantillas de WhatsApp que el código invoca pero no existían en MensajesAutomaticos.
-- Sin la fila, EnviarMensajeTemplado aborta con "Plantilla no encontrada" y el aviso
-- nunca sale (fallo silencioso).
--
-- Se añade además RESUMEN_DEUDAS: estado de cuenta con TODOS los financiamientos
-- pendientes del miembro (membresía y producto a crédito) con sus fechas.
--
-- Destino: SchemaVersion 6.

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'DEUDA_VENCE_HOY')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'DEUDA_VENCE_HOY',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu deuda vence hoy.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Concepto:' + CHAR(13) + CHAR(10) + N'{CONCEPTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Saldo pendiente:' + CHAR(13) + CHAR(10) + N'{SALDO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCIMIENTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Puedes pasar hoy por recepcion para regularizar tu pago.',
        1,
        GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'VENCIMIENTO_HOY')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'VENCIMIENTO_HOY',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu membresia {PLAN} vence hoy.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCE}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Renueva en recepcion para no perder tu acceso.',
        1,
        GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'MEMBRESIA_VENCIDA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'MEMBRESIA_VENCIDA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu membresia {PLAN} se encuentra vencida.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCE}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Motivo:' + CHAR(13) + CHAR(10) + N'{MOTIVO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Pasa por recepcion para reactivarla.',
        1,
        GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'RESUMEN_DEUDAS')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'RESUMEN_DEUDAS',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Este es el estado de tus financiamientos pendientes en MFFITNESS ({CANTIDAD}):' +
        CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'{DETALLE}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Total pendiente:' + CHAR(13) + CHAR(10) + N'{TOTAL}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Proximo vencimiento:' + CHAR(13) + CHAR(10) + N'{PROXIMO_VENCIMIENTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Puedes pasar por recepcion para regularizar tu pago.',
        1,
        GETDATE());
END
GO
