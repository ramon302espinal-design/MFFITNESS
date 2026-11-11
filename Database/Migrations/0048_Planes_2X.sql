-- Plan de membresía 2X (RD$800.00, ciclo mensual).
-- Destino: SchemaVersion 48.
-- Idempotente. Misma fila/columnas que 3x, PREMIUM, GLUTEOS, ABDOMEN:
--   Nombre, Precio, DuracionDias, Activo en dbo.Planes.
-- Aparece en cmbMembresia (FrmPagos), renovación, Estado, historial, caja y reportes
-- porque esos módulos leen el catálogo Activo=1 (no listas hardcodeadas de cobro).
-- No altera historial ni membresías existentes.

IF NOT EXISTS (SELECT 1 FROM dbo.Planes WHERE Nombre = N'2X')
BEGIN
    INSERT INTO dbo.Planes (Nombre, Precio, DuracionDias, Activo)
    VALUES (N'2X', 800.00, 30, 1);
END
ELSE
BEGIN
    UPDATE dbo.Planes
    SET Precio = 800.00,
        DuracionDias = ISNULL(NULLIF(DuracionDias, 0), 30),
        Activo = 1
    WHERE Nombre = N'2X';
END
GO
