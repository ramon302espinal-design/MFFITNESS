-- =====================================================================
-- MEJORAS MÓDULO DE DEUDAS - ÍNDICES DE RENDIMIENTO
-- Sistema: MFFITNESS POS
-- Descripción: Agregar índices para mejorar consultas frecuentes
-- Fecha: 2025
-- =====================================================================

USE [MF CYBER DB]
GO

PRINT '======================================'
PRINT '⚡ AGREGANDO ÍNDICES DE RENDIMIENTO'
PRINT '======================================'
PRINT ''

-- =====================================================================
-- 1. ÍNDICE: Deudas.ClienteId
-- Mejora: Búsqueda de deudas por cliente
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_Deudas_ClienteId' 
	AND object_id = OBJECT_ID('Deudas')
)
BEGIN
	PRINT '➤ Creando IX_Deudas_ClienteId...'

	CREATE NONCLUSTERED INDEX IX_Deudas_ClienteId
	ON Deudas(ClienteId)
	INCLUDE (Saldo, Estado, FechaVencimiento);

	PRINT '✅ IX_Deudas_ClienteId creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_Deudas_ClienteId ya existe'
	PRINT ''
END

-- =====================================================================
-- 2. ÍNDICE: Deudas.Estado
-- Mejora: Filtrado de deudas activas/pagadas
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_Deudas_Estado' 
	AND object_id = OBJECT_ID('Deudas')
)
BEGIN
	PRINT '➤ Creando IX_Deudas_Estado...'

	CREATE NONCLUSTERED INDEX IX_Deudas_Estado
	ON Deudas(Estado)
	INCLUDE (ClienteId, Saldo, FechaVencimiento);

	PRINT '✅ IX_Deudas_Estado creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_Deudas_Estado ya existe'
	PRINT ''
END

-- =====================================================================
-- 3. ÍNDICE: Deudas.FechaCreacion
-- Mejora: Reportes por rango de fechas
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_Deudas_FechaCreacion' 
	AND object_id = OBJECT_ID('Deudas')
)
BEGIN
	PRINT '➤ Creando IX_Deudas_FechaCreacion...'

	CREATE NONCLUSTERED INDEX IX_Deudas_FechaCreacion
	ON Deudas(FechaCreacion DESC)
	INCLUDE (ClienteId, MontoTotal, Saldo, Estado);

	PRINT '✅ IX_Deudas_FechaCreacion creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_Deudas_FechaCreacion ya existe'
	PRINT ''
END

-- =====================================================================
-- 4. ÍNDICE: Deudas.FechaVencimiento
-- Mejora: Detección de deudas vencidas
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_Deudas_FechaVencimiento' 
	AND object_id = OBJECT_ID('Deudas')
)
BEGIN
	PRINT '➤ Creando IX_Deudas_FechaVencimiento...'

	CREATE NONCLUSTERED INDEX IX_Deudas_FechaVencimiento
	ON Deudas(FechaVencimiento)
	INCLUDE (ClienteId, Saldo, Estado);

	PRINT '✅ IX_Deudas_FechaVencimiento creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_Deudas_FechaVencimiento ya existe'
	PRINT ''
END

-- =====================================================================
-- 5. ÍNDICE: PagosDeuda.DeudaId
-- Mejora: Historial de pagos por deuda
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_PagosDeuda_DeudaId' 
	AND object_id = OBJECT_ID('PagosDeuda')
)
BEGIN
	PRINT '➤ Creando IX_PagosDeuda_DeudaId...'

	CREATE NONCLUSTERED INDEX IX_PagosDeuda_DeudaId
	ON PagosDeuda(DeudaId)
	INCLUDE (Fecha, Monto, MetodoPago, Estado);

	PRINT '✅ IX_PagosDeuda_DeudaId creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_PagosDeuda_DeudaId ya existe'
	PRINT ''
END

-- =====================================================================
-- 6. ÍNDICE: PagosDeuda.Fecha
-- Mejora: Reportes de cobros por fecha
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_PagosDeuda_Fecha' 
	AND object_id = OBJECT_ID('PagosDeuda')
)
BEGIN
	PRINT '➤ Creando IX_PagosDeuda_Fecha...'

	CREATE NONCLUSTERED INDEX IX_PagosDeuda_Fecha
	ON PagosDeuda(Fecha DESC)
	INCLUDE (DeudaId, Monto, Estado);

	PRINT '✅ IX_PagosDeuda_Fecha creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_PagosDeuda_Fecha ya existe'
	PRINT ''
END

-- =====================================================================
-- 7. ÍNDICE: HistorialDeudas.DeudaId
-- Mejora: Auditoría por deuda
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_HistorialDeudas_DeudaId' 
	AND object_id = OBJECT_ID('HistorialDeudas')
)
BEGIN
	PRINT '➤ Creando IX_HistorialDeudas_DeudaId...'

	CREATE NONCLUSTERED INDEX IX_HistorialDeudas_DeudaId
	ON HistorialDeudas(DeudaId)
	INCLUDE (Fecha, TipoMovimiento, Monto);

	PRINT '✅ IX_HistorialDeudas_DeudaId creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_HistorialDeudas_DeudaId ya existe'
	PRINT ''
END

-- =====================================================================
-- 8. ÍNDICE: HistorialDeudas.ClienteId
-- Mejora: Auditoría por cliente
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_HistorialDeudas_ClienteId' 
	AND object_id = OBJECT_ID('HistorialDeudas')
)
BEGIN
	PRINT '➤ Creando IX_HistorialDeudas_ClienteId...'

	CREATE NONCLUSTERED INDEX IX_HistorialDeudas_ClienteId
	ON HistorialDeudas(ClienteId)
	INCLUDE (Fecha, TipoMovimiento, Monto);

	PRINT '✅ IX_HistorialDeudas_ClienteId creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  IX_HistorialDeudas_ClienteId ya existe'
	PRINT ''
END

-- =====================================================================
-- VERIFICACIÓN FINAL
-- =====================================================================
PRINT ''
PRINT '======================================'
PRINT '📊 VERIFICACIÓN DE ÍNDICES'
PRINT '======================================'
PRINT ''

SELECT 
	i.name AS IndexName,
	t.name AS TableName,
	i.type_desc AS IndexType,
	COUNT(ic.column_id) AS ColumnCount
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.tables AS t ON i.object_id = t.object_id
WHERE t.name IN ('Deudas', 'PagosDeuda', 'HistorialDeudas')
AND i.is_primary_key = 0
AND i.name IS NOT NULL
GROUP BY i.name, t.name, i.type_desc
ORDER BY t.name, i.name;

PRINT ''
PRINT '======================================'
PRINT '✅ ÍNDICES CONFIGURADOS'
PRINT '======================================'
