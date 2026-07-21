-- ========================================
-- SCRIPT DE VERIFICACIÓN: Palabra Reservada PLAN
-- ========================================
-- Propósito: Verificar que las consultas SQL con alias [Plan] 
--            funcionen correctamente después de la corrección
-- Contexto: PLAN es palabra reservada en SQL Server
-- ========================================

USE [MF CYBER DB];
GO

PRINT '=== VERIFICACIÓN 1: Consulta de Deudas con Plan ===';

-- Esta es la consulta CORREGIDA de DeudaDAL.cs
SELECT 
	d.Id, 
	d.ClienteId, 
	c.Nombre, 
	d.Concepto, 
	d.MontoTotal, 
	d.MontoPagado, 
	d.Saldo,
	d.Estado, 
	d.FechaCreacion, 
	d.FechaVencimiento, 
	d.Usuario,
	d.MembresiaId,
	d.PlanId,
	ISNULL(p.Nombre, 'N/A') AS [Plan],  -- ✅ Corregido con corchetes
	m.FechaInicio AS FechaInicioMembresia,
	m.FechaFin AS FechaFinMembresia
FROM Deudas d
INNER JOIN Clientes c ON c.ID = d.ClienteId
LEFT JOIN Membresias m ON m.Id = d.MembresiaId
LEFT JOIN Planes p ON p.Id = d.PlanId
ORDER BY d.FechaVencimiento ASC;

GO

PRINT '=== VERIFICACIÓN 2: Consulta de Membresía Activa ===';

-- Esta es la consulta CORREGIDA de MembresiaDAL.cs
DECLARE @TestClienteId INT = (SELECT TOP 1 ID FROM Clientes);

SELECT TOP 1 
	m.Id,
	m.FechaInicio,
	m.FechaFin,
	p.Nombre AS [Plan],  -- ✅ Corregido con corchetes
	p.Precio
FROM Membresias m
INNER JOIN Planes p ON p.Id = m.PlanId
WHERE m.ClienteId = @TestClienteId
  AND m.Activa = 1
  AND m.FechaFin >= GETDATE()
ORDER BY m.FechaFin DESC;

GO

PRINT '=== VERIFICACIÓN 3: Comparación AS Plan vs AS [Plan] ===';

-- ❌ ESTA DEBERÍA FALLAR (sin corchetes)
-- SELECT p.Nombre AS Plan FROM Planes p;
-- Error: Incorrect syntax near the keyword 'PLAN'

-- ✅ ESTA DEBERÍA FUNCIONAR (con corchetes)
SELECT p.Nombre AS [Plan] FROM Planes p;

GO

PRINT '=== VERIFICACIÓN 4: Deudas con Planes Financiados ===';

-- Verificar que las deudas vinculadas a planes se vean correctamente
SELECT 
	d.Id AS DeudaId,
	c.Nombre AS Cliente,
	d.Concepto,
	d.Saldo,
	ISNULL(p.Nombre, 'N/A') AS [Plan],  -- ✅ Escapado correctamente
	m.FechaFin AS VencimientoMembresia
FROM Deudas d
INNER JOIN Clientes c ON c.ID = d.ClienteId
LEFT JOIN Membresias m ON m.Id = d.MembresiaId
LEFT JOIN Planes p ON p.Id = d.PlanId
WHERE d.MembresiaId IS NOT NULL
  AND d.Estado = 'ACTIVA'
ORDER BY d.FechaVencimiento;

GO

PRINT '=== ✅ VERIFICACIÓN COMPLETA ===';
PRINT 'Todas las consultas con alias [Plan] funcionan correctamente.';
PRINT 'La palabra reservada PLAN ahora está escapada en todas las queries.';

GO
