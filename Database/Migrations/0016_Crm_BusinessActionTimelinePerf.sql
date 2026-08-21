-- CRM Financiero FASE 11.21: índices para timeline (evitar scans / N+1 lookups).
-- Destino: SchemaVersion 16.
-- Idempotente. No muta datos de negocio.

IF OBJECT_ID(N'dbo.CrmBusinessActions', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_CrmBusinessActions_DecisionHistoryId'
          AND object_id = OBJECT_ID(N'dbo.CrmBusinessActions'))
BEGIN
    CREATE INDEX IX_CrmBusinessActions_DecisionHistoryId
        ON dbo.CrmBusinessActions (DecisionHistoryId)
        WHERE DecisionHistoryId IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.CrmBusinessActions', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_CrmBusinessActions_CreatedAt'
          AND object_id = OBJECT_ID(N'dbo.CrmBusinessActions'))
BEGIN
    CREATE INDEX IX_CrmBusinessActions_CreatedAt
        ON dbo.CrmBusinessActions (CreatedAt DESC, Id DESC);
END
GO
