-- =============================================================================
-- Vaciar DINERO / deudas / historiales de dinero / reportes en [MF CYBER DB]
-- CONSERVA:
--   Clientes, ClienteFichaSalud, Membresias, HistorialMembresias, CongelacionesMembresia
--   Productos, Categorias, Planes
--   Usuarios, Roles, Permisos, RolPermisos, MensajesAutomaticos
--   SchemaVersion, MigrationEngineTest
-- BORRA:
--   Caja, DetalleCaja, CierreCaja, Ventas, DetalleVentas, Pagos
--   Deudas, PagosDeuda, HistorialDeudas, MovimientosStock, ReportesGenerados
--   CRM financiero / decisiones (inversiones, acciones, auditorías)
--   RegistroMensajes (cola operativa WhatsApp)
-- NO toca StockActual de Productos (solo quita movimientos de stock).
-- =============================================================================

USE [MF CYBER DB];
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

-- CRM (orden por FKs)
IF OBJECT_ID(N'dbo.CrmBusinessActionAudit', N'U') IS NOT NULL
    DELETE FROM dbo.CrmBusinessActionAudit;
IF OBJECT_ID(N'dbo.CrmBusinessActions', N'U') IS NOT NULL
    DELETE FROM dbo.CrmBusinessActions;
IF OBJECT_ID(N'dbo.CrmDecisionAudit', N'U') IS NOT NULL
    DELETE FROM dbo.CrmDecisionAudit;
IF OBJECT_ID(N'dbo.CrmInversionLineas', N'U') IS NOT NULL
    DELETE FROM dbo.CrmInversionLineas;
IF OBJECT_ID(N'dbo.CrmInversiones', N'U') IS NOT NULL
    DELETE FROM dbo.CrmInversiones;
IF OBJECT_ID(N'dbo.CrmDecisionEvents', N'U') IS NOT NULL
    DELETE FROM dbo.CrmDecisionEvents;

-- Ventas / inventario movimientos (dinero)
DELETE FROM dbo.DetalleVentas;
DELETE FROM dbo.MovimientosStock;
DELETE FROM dbo.Ventas;

-- Deudas
DELETE FROM dbo.PagosDeuda;
DELETE FROM dbo.HistorialDeudas;
DELETE FROM dbo.Deudas;

-- Caja / pagos / reportes
DELETE FROM dbo.DetalleCaja;
DELETE FROM dbo.CierreCaja;
DELETE FROM dbo.Pagos;
DELETE FROM dbo.Caja;
DELETE FROM dbo.ReportesGenerados;

-- Cola de mensajes (no es catálogo predeterminado)
IF OBJECT_ID(N'dbo.RegistroMensajes', N'U') IS NOT NULL
    DELETE FROM dbo.RegistroMensajes;

-- Reseed identidades de lo vaciado (no Clientes / Membresias / Productos)
IF OBJECT_ID(N'dbo.CrmBusinessActionAudit', N'U') IS NOT NULL
    DBCC CHECKIDENT ('CrmBusinessActionAudit', RESEED, 0);
IF OBJECT_ID(N'dbo.CrmBusinessActions', N'U') IS NOT NULL
    DBCC CHECKIDENT ('CrmBusinessActions', RESEED, 0);
IF OBJECT_ID(N'dbo.CrmDecisionAudit', N'U') IS NOT NULL
    DBCC CHECKIDENT ('CrmDecisionAudit', RESEED, 0);
IF OBJECT_ID(N'dbo.CrmInversionLineas', N'U') IS NOT NULL
    DBCC CHECKIDENT ('CrmInversionLineas', RESEED, 0);
IF OBJECT_ID(N'dbo.CrmInversiones', N'U') IS NOT NULL
    DBCC CHECKIDENT ('CrmInversiones', RESEED, 0);
IF OBJECT_ID(N'dbo.CrmDecisionEvents', N'U') IS NOT NULL
    DBCC CHECKIDENT ('CrmDecisionEvents', RESEED, 0);

DBCC CHECKIDENT ('DetalleVentas', RESEED, 0);
DBCC CHECKIDENT ('MovimientosStock', RESEED, 0);
DBCC CHECKIDENT ('Ventas', RESEED, 0);
DBCC CHECKIDENT ('PagosDeuda', RESEED, 0);
DBCC CHECKIDENT ('HistorialDeudas', RESEED, 0);
DBCC CHECKIDENT ('Deudas', RESEED, 0);
DBCC CHECKIDENT ('DetalleCaja', RESEED, 0);
DBCC CHECKIDENT ('CierreCaja', RESEED, 0);
DBCC CHECKIDENT ('Pagos', RESEED, 0);
DBCC CHECKIDENT ('Caja', RESEED, 0);
DBCC CHECKIDENT ('ReportesGenerados', RESEED, 0);
IF OBJECT_ID(N'dbo.RegistroMensajes', N'U') IS NOT NULL
    DBCC CHECKIDENT ('RegistroMensajes', RESEED, 0);

COMMIT TRANSACTION;

PRINT 'OK: vaciado de dinero completado en [MF CYBER DB].';
PRINT 'Conservado: Clientes, Membresias (+historial/congelaciones), Productos, Planes, Categorias, Usuarios.';
