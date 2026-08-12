-- Extiende tabla de prueba del motor. No toca tablas de negocio del POS.
-- Destino: SchemaVersion 4.

IF COL_LENGTH(N'dbo.MigrationEngineTest', N'UpdateManagerMarker') IS NULL
    ALTER TABLE dbo.MigrationEngineTest ADD UpdateManagerMarker NVARCHAR(40) NULL;
GO

UPDATE dbo.MigrationEngineTest
SET UpdateManagerMarker = N'update-v0-ok'
WHERE Marker = N'engine-ok' AND UpdateManagerMarker IS NULL;
