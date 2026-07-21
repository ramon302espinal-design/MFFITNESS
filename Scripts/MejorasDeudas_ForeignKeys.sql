-- =====================================================================
-- MEJORAS MÓDULO DE DEUDAS - FOREIGN KEYS
-- Sistema: MFFITNESS POS
-- Descripción: Agregar Foreign Keys para integridad referencial
-- Fecha: 2025
-- =====================================================================

USE [MF CYBER DB]
GO

PRINT '======================================'
PRINT '🔒 AGREGANDO FOREIGN KEYS'
PRINT '======================================'
PRINT ''

-- =====================================================================
-- 1. FOREIGN KEY: Deudas → Clientes
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_Deudas_Clientes' 
	AND parent_object_id = OBJECT_ID('Deudas')
)
BEGIN
	PRINT '➤ Creando FK_Deudas_Clientes...'

	ALTER TABLE Deudas
	ADD CONSTRAINT FK_Deudas_Clientes 
	FOREIGN KEY (ClienteId) 
	REFERENCES Clientes(ID)
	ON DELETE NO ACTION
	ON UPDATE NO ACTION;

	PRINT '✅ FK_Deudas_Clientes creada correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  FK_Deudas_Clientes ya existe'
	PRINT ''
END

-- =====================================================================
-- 2. FOREIGN KEY: PagosDeuda → Deudas
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_PagosDeuda_Deudas' 
	AND parent_object_id = OBJECT_ID('PagosDeuda')
)
BEGIN
	PRINT '➤ Creando FK_PagosDeuda_Deudas...'

	ALTER TABLE PagosDeuda
	ADD CONSTRAINT FK_PagosDeuda_Deudas 
	FOREIGN KEY (DeudaId) 
	REFERENCES Deudas(Id)
	ON DELETE NO ACTION
	ON UPDATE NO ACTION;

	PRINT '✅ FK_PagosDeuda_Deudas creada correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  FK_PagosDeuda_Deudas ya existe'
	PRINT ''
END

-- =====================================================================
-- 3. FOREIGN KEY: HistorialDeudas → Deudas
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_HistorialDeudas_Deudas' 
	AND parent_object_id = OBJECT_ID('HistorialDeudas')
)
BEGIN
	PRINT '➤ Creando FK_HistorialDeudas_Deudas...'

	ALTER TABLE HistorialDeudas
	ADD CONSTRAINT FK_HistorialDeudas_Deudas 
	FOREIGN KEY (DeudaId) 
	REFERENCES Deudas(Id)
	ON DELETE NO ACTION
	ON UPDATE NO ACTION;

	PRINT '✅ FK_HistorialDeudas_Deudas creada correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  FK_HistorialDeudas_Deudas ya existe'
	PRINT ''
END

-- =====================================================================
-- 4. FOREIGN KEY: HistorialDeudas → Clientes
-- =====================================================================
IF NOT EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_HistorialDeudas_Clientes' 
	AND parent_object_id = OBJECT_ID('HistorialDeudas')
)
BEGIN
	PRINT '➤ Creando FK_HistorialDeudas_Clientes...'

	ALTER TABLE HistorialDeudas
	ADD CONSTRAINT FK_HistorialDeudas_Clientes 
	FOREIGN KEY (ClienteId) 
	REFERENCES Clientes(ID)
	ON DELETE NO ACTION
	ON UPDATE NO ACTION;

	PRINT '✅ FK_HistorialDeudas_Clientes creada correctamente'
	PRINT ''
END
ELSE
BEGIN
	PRINT '⚠️  FK_HistorialDeudas_Clientes ya existe'
	PRINT ''
END

-- =====================================================================
-- VERIFICACIÓN FINAL
-- =====================================================================
PRINT ''
PRINT '======================================'
PRINT '📊 VERIFICACIÓN DE FOREIGN KEYS'
PRINT '======================================'
PRINT ''

SELECT 
	fk.name AS ForeignKeyName,
	tp.name AS ParentTable,
	cp.name AS ParentColumn,
	tr.name AS ReferencedTable,
	cr.name AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables AS tp ON fkc.parent_object_id = tp.object_id
INNER JOIN sys.columns AS cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.tables AS tr ON fkc.referenced_object_id = tr.object_id
INNER JOIN sys.columns AS cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
WHERE tp.name IN ('Deudas', 'PagosDeuda', 'HistorialDeudas')
ORDER BY tp.name, fk.name;

PRINT ''
PRINT '======================================'
PRINT '✅ FOREIGN KEYS CONFIGURADAS'
PRINT '======================================'
