-- Aviso automatico al crear deuda de producto (panelFinanciamientoProducto / venta a credito).
-- CrearDeudaConId → EnviarNotificacionDeudaCreada → plantilla DEUDA_CREADA.
-- Sin la fila, EnviarMensajeTemplado aborta ("Plantilla no encontrada") y el aviso no sale.
-- Tambien cubre recordatorios del job automatico (misma cadena de deudas).
-- Destino: SchemaVersion 25.
-- Idempotente: inserta si falta; si existe solo asegura Activa=1 (no pisa texto de PROD).

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'DEUDA_CREADA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'DEUDA_CREADA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Se ha registrado una deuda.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Concepto:' + CHAR(13) + CHAR(10) + N'{CONCEPTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Monto:' + CHAR(13) + CHAR(10) + N'{MONTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCIMIENTO}',
        1,
        GETDATE());
END
ELSE
BEGIN
    UPDATE dbo.MensajesAutomaticos SET Activa = 1 WHERE Tipo = N'DEUDA_CREADA' AND Activa <> 1;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'RECORDATORIO_VENCIMIENTO_DEUDA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'RECORDATORIO_VENCIMIENTO_DEUDA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu deuda vence en {DIAS_RESTANTES} dias.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Concepto:' + CHAR(13) + CHAR(10) + N'{CONCEPTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Saldo pendiente:' + CHAR(13) + CHAR(10) + N'{SALDO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCIMIENTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Gracias por mantener tus pagos al dia.',
        1,
        GETDATE());
END
ELSE
BEGIN
    UPDATE dbo.MensajesAutomaticos
    SET Activa = 1
    WHERE Tipo = N'RECORDATORIO_VENCIMIENTO_DEUDA' AND Activa <> 1;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'DEUDA_VENCIDA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'DEUDA_VENCIDA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Tu deuda se encuentra vencida.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Concepto:' + CHAR(13) + CHAR(10) + N'{CONCEPTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Saldo pendiente:' + CHAR(13) + CHAR(10) + N'{SALDO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Fecha de vencimiento:' + CHAR(13) + CHAR(10) + N'{FECHA_VENCIMIENTO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Por favor regulariza tu pago lo antes posible.',
        1,
        GETDATE());
END
ELSE
BEGIN
    UPDATE dbo.MensajesAutomaticos SET Activa = 1 WHERE Tipo = N'DEUDA_VENCIDA' AND Activa <> 1;
END
GO
