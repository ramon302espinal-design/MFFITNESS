-- Plan especial OFERTA (cortesia / promo) en catalogo Planes.
-- Destino: SchemaVersion 19.
-- Idempotente.

IF NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE Nombre = N'OFERTA')
BEGIN
    INSERT INTO dbo.Planes (Nombre, Precio, DuracionDias, Activo)
    VALUES (N'OFERTA', 0.00, 30, 1);
END
ELSE
BEGIN
    UPDATE dbo.Planes
    SET Precio = 0.00,
        DuracionDias = ISNULL(NULLIF(DuracionDias, 0), 30),
        Activo = 1
    WHERE Nombre = N'OFERTA';
END
GO
