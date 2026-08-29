-- Programación de membresía: cobro anticipado del siguiente periodo (miembro sigue ACTIVO).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MembresiasProgramadas')
BEGIN
    CREATE TABLE dbo.MembresiasProgramadas
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClienteId INT NOT NULL
            CONSTRAINT FK_MembProg_Clientes REFERENCES dbo.Clientes(ID),
        PlanId INT NOT NULL
            CONSTRAINT FK_MembProg_Planes REFERENCES dbo.Planes(Id),
        Monto DECIMAL(18,2) NOT NULL,
        FechaPago DATETIME NOT NULL,
        FechaInicioProgramada DATE NOT NULL,
        FechaFinProgramada DATE NOT NULL,
        MembresiaOrigenId INT NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_MembProg_Estado DEFAULT(N'PENDIENTE'),
        Usuario NVARCHAR(80) NULL,
        FechaRegistro DATETIME NOT NULL
            CONSTRAINT DF_MembProg_FechaReg DEFAULT(GETDATE()),
        PagoId INT NULL,
        CajaMovimientoId INT NULL,
        Nota NVARCHAR(400) NULL
    );

    CREATE INDEX IX_MembProg_Cliente_Estado
        ON dbo.MembresiasProgramadas (ClienteId, Estado);

    CREATE INDEX IX_MembProg_Inicio_Estado
        ON dbo.MembresiasProgramadas (FechaInicioProgramada, Estado);
END
