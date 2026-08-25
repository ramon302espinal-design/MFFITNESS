-- Planes nuevos: GLUTEOS GRANDE y ABDOMEN PLANO.
-- Destino: SchemaVersion 22.
-- Idempotente. Precio catalogo = 6000.00; Duracion 30 dias.
-- No genera movimientos de caja.

IF NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE Nombre = N'GLUTEOS GRANDE')
BEGIN
    INSERT INTO dbo.Planes (Nombre, Precio, DuracionDias, Activo)
    VALUES (N'GLUTEOS GRANDE', 6000.00, 30, 1);
END
ELSE
BEGIN
    UPDATE dbo.Planes
    SET Precio = 6000.00,
        DuracionDias = ISNULL(NULLIF(DuracionDias, 0), 30),
        Activo = 1
    WHERE Nombre = N'GLUTEOS GRANDE';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE Nombre = N'ABDOMEN PLANO')
BEGIN
    INSERT INTO dbo.Planes (Nombre, Precio, DuracionDias, Activo)
    VALUES (N'ABDOMEN PLANO', 6000.00, 30, 1);
END
ELSE
BEGIN
    UPDATE dbo.Planes
    SET Precio = 6000.00,
        DuracionDias = ISNULL(NULLIF(DuracionDias, 0), 30),
        Activo = 1
    WHERE Nombre = N'ABDOMEN PLANO';
END
GO

-- Plantilla WhatsApp: alta de miembro ya pagado (sin monto).
-- Detalle solo (Miembro/Asunto/Fecha los pone la plantilla UTILITY Twilio).
IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'ALTA_MIEMBRO_EXISTENTE')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'ALTA_MIEMBRO_EXISTENTE',
        N'Se te ha agregado al sistema con la membresia {PLAN} ya pagada; su proxima fecha de pago sera para {FECHA_VENCE}. Gracias por formar parte de la familia MFFITNESS.',
        1,
        GETDATE());
END
ELSE
BEGIN
    UPDATE dbo.MensajesAutomaticos
    SET Plantilla =
        N'Se te ha agregado al sistema con la membresia {PLAN} ya pagada; su proxima fecha de pago sera para {FECHA_VENCE}. Gracias por formar parte de la familia MFFITNESS.',
        Activa = 1
    WHERE Tipo = N'ALTA_MIEMBRO_EXISTENTE';
END
GO
