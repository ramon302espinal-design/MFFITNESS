-- CRM Financiero FASE 4.4: costo histórico en ventas y entradas de stock.
-- Destino: SchemaVersion 8.
-- G1: DetalleVentas.CostoUnitario (snapshot al vender)
-- G2: MovimientosStock.CostoUnitario / CostoTotal (entrada con costo)

IF COL_LENGTH(N'dbo.DetalleVentas', N'CostoUnitario') IS NULL
    ALTER TABLE dbo.DetalleVentas ADD CostoUnitario DECIMAL(18, 4) NULL;
GO

IF COL_LENGTH(N'dbo.MovimientosStock', N'CostoUnitario') IS NULL
    ALTER TABLE dbo.MovimientosStock ADD CostoUnitario DECIMAL(18, 4) NULL;
GO

IF COL_LENGTH(N'dbo.MovimientosStock', N'CostoTotal') IS NULL
    ALTER TABLE dbo.MovimientosStock ADD CostoTotal DECIMAL(18, 4) NULL;
GO
