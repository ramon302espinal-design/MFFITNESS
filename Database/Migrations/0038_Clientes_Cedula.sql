-- Cédula / documento de identidad del cliente (opcional).
-- Destino: SchemaVersion 38.
-- Idempotente. MF CYBER DB (PROD) y MF_CYBER_DB_DEV.

IF COL_LENGTH(N'dbo.Clientes', N'Cedula') IS NULL
BEGIN
    ALTER TABLE dbo.Clientes
        ADD Cedula NVARCHAR(30) NULL;
END
GO
