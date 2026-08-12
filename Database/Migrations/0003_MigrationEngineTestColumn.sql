-- Extiende la tabla de prueba del motor. No toca tablas de negocio del POS.
-- Destino: SchemaVersion 3.
-- GO: SQL Server no permite referenciar una columna recién agregada en el mismo batch.

IF COL_LENGTH(N'dbo.MigrationEngineTest', N'AppliedBy') IS NULL
    ALTER TABLE dbo.MigrationEngineTest ADD AppliedBy NVARCHAR(40) NULL;
GO

UPDATE dbo.MigrationEngineTest
SET AppliedBy = N'migration-0003'
WHERE Marker = N'engine-ok' AND AppliedBy IS NULL;
