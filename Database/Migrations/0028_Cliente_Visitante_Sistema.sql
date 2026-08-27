-- Cliente técnico para cobros ATLETA / VISITA sin miembro permanente.
-- Destino: SchemaVersion 28.
-- Idempotente. No aparece en Estado ACTIVO (sin Membresias).

IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Nombre = N'VISITANTE (SISTEMA)')
BEGIN
    INSERT INTO dbo.Clientes (Nombre, FechaNacimiento, Direccion, Telefono)
    VALUES (N'VISITANTE (SISTEMA)', '2000-01-01', N'Acceso parcial ATLETA/VISITA', N'N/A');
END
GO
