-- =====================================================================
-- MEJORAS MÓDULO DE DEUDAS - SOPORTE VENTAS A CRÉDITO
-- Sistema: MFFITNESS POS
-- Descripción: Agregar columnas para ventas con saldo pendiente
-- Fecha: 2025
-- =====================================================================

USE [MF CYBER DB]
GO

SET QUOTED_IDENTIFIER ON
GO

PRINT '======================================'
PRINT '💳 HABILITANDO VENTAS A CRÉDITO'
PRINT '======================================'
PRINT ''

-- =====================================================================
-- 1. AGREGAR COLUMNA MontoPagado
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.columns 
	WHERE object_id = OBJECT_ID('Ventas') 
	AND name = 'MontoPagado'
)
BEGIN
	PRINT '➤ Agregando columna MontoPagado a Ventas...'

	ALTER TABLE Ventas 
	ADD MontoPagado DECIMAL(18,2) NULL;

	PRINT '✅ Columna MontoPagado agregada correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  Columna MontoPagado ya existe'
	PRINT ''
END
GO

-- =====================================================================
-- 1B. INICIALIZAR VALORES DE MontoPagado
-- =====================================================================
PRINT '➤ Verificando valores NULL en MontoPagado...'

UPDATE Ventas 
SET MontoPagado = Total 
WHERE MontoPagado IS NULL;

PRINT '✅ Valores NULL inicializados (ventas antiguas = pagadas completas)'
PRINT ''
GO

-- =====================================================================
-- 2. AGREGAR COLUMNA Saldo
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.columns 
	WHERE object_id = OBJECT_ID('Ventas') 
	AND name = 'Saldo'
)
BEGIN
	PRINT '➤ Agregando columna Saldo a Ventas...'

	ALTER TABLE Ventas 
	ADD Saldo AS (Total - ISNULL(MontoPagado, 0)) PERSISTED;

	PRINT '✅ Columna Saldo agregada correctamente (calculada automáticamente)'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  Columna Saldo ya existe'
	PRINT ''
END
GO

-- =====================================================================
-- 3. ÍNDICE PARA VENTAS CON SALDO PENDIENTE
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_Ventas_Saldo' 
	AND object_id = OBJECT_ID('Ventas')
)
BEGIN
	PRINT '➤ Creando índice IX_Ventas_Saldo...'

	CREATE NONCLUSTERED INDEX IX_Ventas_Saldo
	ON Ventas(ClienteId)
	INCLUDE (Total, MontoPagado, Saldo, Fecha);

	PRINT '✅ Índice IX_Ventas_Saldo creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  Índice IX_Ventas_Saldo ya existe'
	PRINT ''
END

-- =====================================================================
-- 4. ÍNDICE PARA VENTAS POR CLIENTE
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.indexes 
	WHERE name = 'IX_Ventas_ClienteId' 
	AND object_id = OBJECT_ID('Ventas')
)
BEGIN
	PRINT '➤ Creando índice IX_Ventas_ClienteId...'

	CREATE NONCLUSTERED INDEX IX_Ventas_ClienteId
	ON Ventas(ClienteId)
	INCLUDE (Total, MontoPagado, Saldo, Fecha);

	PRINT '✅ Índice IX_Ventas_ClienteId creado correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  Índice IX_Ventas_ClienteId ya existe'
	PRINT ''
END

-- =====================================================================
-- VERIFICACIÓN FINAL
-- =====================================================================
PRINT ''
PRINT '======================================'
PRINT '📊 VERIFICACIÓN DE CAMBIOS'
PRINT '======================================'
PRINT ''

-- Verificar estructura
SELECT 
	COLUMN_NAME,
	DATA_TYPE,
	IS_NULLABLE,
	COLUMNPROPERTY(OBJECT_ID('Ventas'), COLUMN_NAME, 'IsComputed') AS IsComputed
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Ventas'
AND COLUMN_NAME IN ('Total', 'MontoPagado', 'Saldo')
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT '-- Estadísticas de ventas --'

SELECT 
	COUNT(*) AS TotalVentas,
	SUM(CASE WHEN Saldo > 0 THEN 1 ELSE 0 END) AS VentasConSaldo,
	SUM(CASE WHEN Saldo = 0 THEN 1 ELSE 0 END) AS VentasPagadas,
	SUM(Saldo) AS TotalSaldoPendiente
FROM Ventas;

PRINT ''
PRINT '======================================'
PRINT '✅ VENTAS A CRÉDITO HABILITADAS'
PRINT '======================================'
PRINT ''
PRINT '⚠️  IMPORTANTE:'
PRINT '   - MontoPagado se inicializó = Total en ventas antiguas'
PRINT '   - Saldo es columna calculada automáticamente'
PRINT '   - Desde ahora debes capturar MontoPagado en cada venta'
PRINT '   - Si MontoPagado < Total, se debe crear deuda automática'
PRINT ''
