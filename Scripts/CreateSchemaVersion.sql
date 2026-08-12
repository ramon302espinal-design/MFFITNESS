-- Control de versión del esquema. Idempotente: no recrea la tabla ni duplica el baseline.
-- La app aplica el mismo SQL desde DL.SchemaVersionDAL en el primer acceso a datos.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SchemaVersion')
BEGIN
    CREATE TABLE dbo.SchemaVersion
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Version INT NOT NULL,
        AppliedAt DATETIME2 NOT NULL
            CONSTRAINT DF_SchemaVersion_AppliedAt DEFAULT (SYSDATETIME()),
        Description NVARCHAR(300) NOT NULL,
        EsActual BIT NOT NULL
            CONSTRAINT DF_SchemaVersion_EsActual DEFAULT (0),
        CONSTRAINT UQ_SchemaVersion_Version UNIQUE (Version)
    );

    CREATE UNIQUE INDEX UX_SchemaVersion_EsActual
        ON dbo.SchemaVersion (EsActual)
        WHERE EsActual = 1;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion)
BEGIN
    INSERT INTO dbo.SchemaVersion (Version, Description, EsActual)
    VALUES (1, N'Baseline inicial del esquema existente de MFFITNESS POS', 1);
END
GO
