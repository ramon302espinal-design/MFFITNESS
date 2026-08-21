-- Plantilla WhatsApp: oferta / descuento en pago de membresia.
-- Destino: SchemaVersion 18.
-- Idempotente.

IF NOT EXISTS (SELECT 1 FROM dbo.MensajesAutomaticos WHERE Tipo = N'OFERTA_MEMBRESIA')
BEGIN
    INSERT INTO dbo.MensajesAutomaticos (Tipo, Plantilla, Activa, FechaCreacion)
    VALUES (
        N'OFERTA_MEMBRESIA',
        N'Hola {CLIENTE}.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Se aplico una oferta a tu membresia.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Plan:' + CHAR(13) + CHAR(10) + N'{PLAN}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Precio de lista:' + CHAR(13) + CHAR(10) + N'{PRECIO_LISTA}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Descuento:' + CHAR(13) + CHAR(10) + N'{PORCENTAJE}% ({DESCUENTO})' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Total pagado:' + CHAR(13) + CHAR(10) + N'{TOTAL}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Motivo / asunto:' + CHAR(13) + CHAR(10) + N'{MOTIVO}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
        N'Gracias por formar parte de MFFITNESS.',
        1,
        GETDATE());
END
GO
