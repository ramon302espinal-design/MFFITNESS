-- Precio correcto GLUTEOS GRANDE / ABDOMEN PLANO = 6000.00
-- Destino: SchemaVersion 23.
-- Idempotente. No toca caja ni membresias existentes (solo catalogo Planes).

UPDATE dbo.Planes
SET Precio = 6000.00,
    DuracionDias = ISNULL(NULLIF(DuracionDias, 0), 30),
    Activo = 1
WHERE Nombre IN (N'GLUTEOS GRANDE', N'ABDOMEN PLANO');
GO
