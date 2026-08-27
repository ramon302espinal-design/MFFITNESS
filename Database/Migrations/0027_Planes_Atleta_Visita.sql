-- Planes parciales ATLETA / VISITA (día / acceso sin membresía activa).
-- Destino: SchemaVersion 27.
-- Idempotente. No crea filas en Membresias al cobrar (lógica en BLL).

IF NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE Nombre = N'ATLETA')
BEGIN
    INSERT INTO dbo.Planes (Nombre, Precio, DuracionDias, Activo)
    VALUES (N'ATLETA', 75.00, 1, 1);
END
ELSE
BEGIN
    UPDATE dbo.Planes
    SET Precio = 75.00,
        DuracionDias = 1,
        Activo = 1
    WHERE Nombre = N'ATLETA';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE Nombre = N'VISITA')
BEGIN
    INSERT INTO dbo.Planes (Nombre, Precio, DuracionDias, Activo)
    VALUES (N'VISITA', 100.00, 1, 1);
END
ELSE
BEGIN
    UPDATE dbo.Planes
    SET Precio = 100.00,
        DuracionDias = 1,
        Activo = 1
    WHERE Nombre = N'VISITA';
END
GO
