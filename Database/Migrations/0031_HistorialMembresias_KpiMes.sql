-- Índice para KPIs por mes en Estado Clientes (HistorialMembresias).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_HistorialMembresias_Fecha_PlanId'
      AND object_id = OBJECT_ID(N'dbo.HistorialMembresias'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_HistorialMembresias_Fecha_PlanId
        ON dbo.HistorialMembresias (Fecha, PlanId)
        INCLUDE (TipoMovimiento, Monto, ClienteId);
END
GO
