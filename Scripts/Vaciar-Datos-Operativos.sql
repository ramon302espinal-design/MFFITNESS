-- Vaciado operativo MFFITNESS.
-- Conserva: Planes, Categorias, Productos (catalogo), MensajesAutomaticos,
--           Roles, Permisos, RolPermisos, Usuarios, SchemaVersion, MigrationEngineTest.
-- Productos: StockActual = 0 (inventario en cero, sin borrar el catalogo).

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

DELETE FROM dbo.DetalleVentas;
DELETE FROM dbo.MovimientosStock;
DELETE FROM dbo.PagosDeuda;
DELETE FROM dbo.HistorialDeudas;
DELETE FROM dbo.DetalleCaja;
DELETE FROM dbo.CierreCaja;
DELETE FROM dbo.RegistroMensajes;
DELETE FROM dbo.ClienteFichaSalud;
DELETE FROM dbo.CongelacionesMembresia;
DELETE FROM dbo.Deudas;
DELETE FROM dbo.Pagos;
DELETE FROM dbo.Ventas;
DELETE FROM dbo.Membresias;
DELETE FROM dbo.HistorialMembresias;
DELETE FROM dbo.Caja;
DELETE FROM dbo.ReportesGenerados;
DELETE FROM dbo.Clientes;

UPDATE dbo.Productos SET StockActual = 0;

DBCC CHECKIDENT ('DetalleVentas', RESEED, 0);
DBCC CHECKIDENT ('MovimientosStock', RESEED, 0);
DBCC CHECKIDENT ('PagosDeuda', RESEED, 0);
DBCC CHECKIDENT ('HistorialDeudas', RESEED, 0);
DBCC CHECKIDENT ('DetalleCaja', RESEED, 0);
DBCC CHECKIDENT ('CierreCaja', RESEED, 0);
DBCC CHECKIDENT ('RegistroMensajes', RESEED, 0);
DBCC CHECKIDENT ('CongelacionesMembresia', RESEED, 0);
DBCC CHECKIDENT ('Deudas', RESEED, 0);
DBCC CHECKIDENT ('Pagos', RESEED, 0);
DBCC CHECKIDENT ('Ventas', RESEED, 0);
DBCC CHECKIDENT ('Membresias', RESEED, 0);
DBCC CHECKIDENT ('HistorialMembresias', RESEED, 0);
DBCC CHECKIDENT ('Caja', RESEED, 0);
DBCC CHECKIDENT ('ReportesGenerados', RESEED, 0);
DBCC CHECKIDENT ('Clientes', RESEED, 0);

COMMIT TRANSACTION;
PRINT 'OK: vaciado operativo completado';
