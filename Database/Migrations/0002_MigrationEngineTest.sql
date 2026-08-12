-- Prueba del motor de migraciones. No toca tablas de negocio del POS.
-- Destino: SchemaVersion 2.

IF OBJECT_ID(N'dbo.MigrationEngineTest', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MigrationEngineTest
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Marker NVARCHAR(80) NOT NULL,
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_MigrationEngineTest_CreatedAt DEFAULT (SYSDATETIME())
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.MigrationEngineTest WHERE Marker = N'engine-ok')
BEGIN
    INSERT INTO dbo.MigrationEngineTest (Marker)
    VALUES (N'engine-ok');
END
