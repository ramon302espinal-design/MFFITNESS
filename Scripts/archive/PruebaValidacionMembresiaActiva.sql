-- =====================================================
-- Script de Prueba: Validación de Membresía Activa
-- Propósito: Verificar que la lógica de validación funcione correctamente
-- =====================================================

USE [MF CYBER DB];
GO

PRINT '========================================';
PRINT 'PRUEBA 1: Verificar clientes con membresía activa';
PRINT '========================================';

SELECT 
	c.Id AS ClienteId,
	c.Nombre,
	m.Id AS MembresiaId,
	p.Nombre AS Plan,
	m.FechaInicio,
	m.FechaFin,
	m.Activa,
	CASE 
		WHEN m.Activa = 1 AND m.FechaFin >= GETDATE() THEN 'ACTIVA'
		WHEN m.Activa = 1 AND m.FechaFin < GETDATE() THEN 'VENCIDA'
		ELSE 'INACTIVA'
	END AS Estado,
	DATEDIFF(DAY, GETDATE(), m.FechaFin) AS DiasRestantes
FROM Clientes c
LEFT JOIN Membresias m ON c.Id = m.ClienteId
LEFT JOIN Planes p ON p.Id = m.PlanId
WHERE m.Activa = 1
ORDER BY m.FechaFin DESC;

PRINT '';
PRINT '========================================';
PRINT 'PRUEBA 2: Obtener detalles de membresía activa de un cliente específico';
PRINT '========================================';

-- Cambiar @ClienteId por el ID de un cliente de prueba
DECLARE @ClienteId INT = 1;

SELECT TOP 1 
	m.Id,
	m.FechaInicio,
	m.FechaFin,
	p.Nombre AS Plan,
	p.Precio,
	DATEDIFF(DAY, GETDATE(), m.FechaFin) AS DiasRestantes
FROM Membresias m
INNER JOIN Planes p ON p.Id = m.PlanId
WHERE m.ClienteId = @ClienteId
  AND m.Activa = 1
  AND m.FechaFin >= GETDATE()
ORDER BY m.FechaFin DESC;

PRINT '';
PRINT '========================================';
PRINT 'PRUEBA 3: Verificar si un cliente tiene membresía activa';
PRINT '========================================';

DECLARE @TieneMembresiaActiva BIT;

SELECT @TieneMembresiaActiva = CASE 
	WHEN COUNT(*) > 0 THEN 1 
	ELSE 0 
END
FROM Membresias
WHERE ClienteId = @ClienteId
  AND Activa = 1
  AND FechaFin >= GETDATE();

IF @TieneMembresiaActiva = 1
BEGIN
	PRINT '✅ El cliente tiene membresía ACTIVA';

	-- Obtener detalles
	SELECT TOP 1
		'El cliente ya tiene una membresía activa:' AS Mensaje,
		p.Nombre AS Plan,
		FORMAT(m.FechaFin, 'dd/MM/yyyy') AS Vence
	FROM Membresias m
	INNER JOIN Planes p ON p.Id = m.PlanId
	WHERE m.ClienteId = @ClienteId
	  AND m.Activa = 1
	  AND m.FechaFin >= GETDATE();
END
ELSE
BEGIN
	PRINT '✅ El cliente NO tiene membresía activa (puede pagar nueva membresía)';
END

PRINT '';
PRINT '========================================';
PRINT 'PRUEBA 4: Listar clientes que pueden pagar membresía';
PRINT '========================================';

-- Clientes SIN membresía activa o con membresía vencida
SELECT DISTINCT
	c.Id,
	c.Nombre,
	c.Telefono,
	CASE 
		WHEN m.Id IS NULL THEN 'Sin membresía previa'
		WHEN m.FechaFin < GETDATE() THEN 'Membresía vencida el ' + FORMAT(m.FechaFin, 'dd/MM/yyyy')
		ELSE 'Sin membresía activa'
	END AS Estado
FROM Clientes c
LEFT JOIN (
	SELECT ClienteId, MAX(FechaFin) AS FechaFin, MAX(Id) AS Id
	FROM Membresias
	GROUP BY ClienteId
) m ON c.Id = m.ClienteId
WHERE NOT EXISTS (
	SELECT 1 
	FROM Membresias mem
	WHERE mem.ClienteId = c.Id
	  AND mem.Activa = 1
	  AND mem.FechaFin >= GETDATE()
)
ORDER BY c.Nombre;

PRINT '';
PRINT '========================================';
PRINT 'PRUEBA 5: Estadísticas de membresías';
PRINT '========================================';

SELECT 
	'Membresías Activas' AS Tipo,
	COUNT(*) AS Cantidad
FROM Membresias
WHERE Activa = 1 AND FechaFin >= GETDATE()

UNION ALL

SELECT 
	'Membresías Vencidas',
	COUNT(*)
FROM Membresias
WHERE Activa = 1 AND FechaFin < GETDATE()

UNION ALL

SELECT 
	'Membresías Inactivas',
	COUNT(*)
FROM Membresias
WHERE Activa = 0

UNION ALL

SELECT 
	'Total Membresías',
	COUNT(*)
FROM Membresias;

PRINT '';
PRINT '✅ Pruebas completadas. Revisa los resultados para validar la lógica.';
