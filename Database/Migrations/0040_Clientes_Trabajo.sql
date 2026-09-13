-- Datos laborales del cliente (deudas tabCrear → ficha datos generales).
-- Destino: SchemaVersion 40.
-- Idempotente. MF CYBER DB (PROD) y MF_CYBER_DB_DEV.

IF COL_LENGTH(N'dbo.Clientes', N'LugarTrabajo') IS NULL
BEGIN
    ALTER TABLE dbo.Clientes
        ADD LugarTrabajo NVARCHAR(200) NULL;
END
GO

IF COL_LENGTH(N'dbo.Clientes', N'DireccionTrabajo') IS NULL
BEGIN
    ALTER TABLE dbo.Clientes
        ADD DireccionTrabajo NVARCHAR(200) NULL;
END
GO
