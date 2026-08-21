-- =============================================================================
-- LocalDB: bases Development / Production para MFFITNESS
-- Instancia: (localdb)\MSSQLLocalDB
--
-- Development → MF_CYBER_DB_DEV  (pruebas; debe tener esquema POS completo)
-- Production  → [MF CYBER DB]   (gimnasio real HOY; SchemaVersion actual)
--
-- MF_CYBER_DB vacía NO sirve de Production: el baseline Version=1 no crea tablas.
-- Cuando renombres el histórico:
--   ALTER DATABASE [MF CYBER DB] MODIFY NAME = [MF_CYBER_DB];
-- y actualiza appsettings.Production.json.
-- =============================================================================

SET NOCOUNT ON;

IF DB_ID(N'MF_CYBER_DB_DEV') IS NULL
BEGIN
    PRINT 'Creando MF_CYBER_DB_DEV...';
    CREATE DATABASE [MF_CYBER_DB_DEV];
END
ELSE
    PRINT 'MF_CYBER_DB_DEV ya existe.';

IF DB_ID(N'MF_CYBER_DB') IS NULL
BEGIN
    IF DB_ID(N'MF CYBER DB') IS NOT NULL
    BEGIN
        PRINT 'AVISO: Existe [MF CYBER DB] (nombre histórico).';
        PRINT '       Production en appsettings apunta a MF_CYBER_DB.';
        PRINT '       Renombra cuando convenga:';
        PRINT '       ALTER DATABASE [MF CYBER DB] MODIFY NAME = [MF_CYBER_DB];';
        PRINT '       O crea MF_CYBER_DB vacía abajo.';
        PRINT 'Creando MF_CYBER_DB (vacía) además del histórico...';
        CREATE DATABASE [MF_CYBER_DB];
    END
    ELSE
    BEGIN
        PRINT 'Creando MF_CYBER_DB...';
        CREATE DATABASE [MF_CYBER_DB];
    END
END
ELSE
    PRINT 'MF_CYBER_DB ya existe.';

PRINT 'Listo. Development=MF_CYBER_DB_DEV | Production=MF_CYBER_DB';
