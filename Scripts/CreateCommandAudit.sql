IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'CommandAudit'
)
BEGIN
    CREATE TABLE CommandAudit (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        CommandType     NVARCHAR(100) NOT NULL,
        Module          NVARCHAR(50)  NOT NULL,
        Description     NVARCHAR(500) NOT NULL,
        Action          NVARCHAR(20)  NOT NULL,
        Usuario         NVARCHAR(100) NOT NULL,
        Fecha           DATETIME      NOT NULL DEFAULT GETDATE(),
        Success         BIT           NOT NULL,
        ErrorMessage    NVARCHAR(500) NULL
    );

    CREATE NONCLUSTERED INDEX IX_CommandAudit_Fecha
        ON CommandAudit(Fecha DESC);

    PRINT 'Tabla CommandAudit creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla CommandAudit ya existe.';
END
