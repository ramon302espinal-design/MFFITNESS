# Migraciones SQL — MFFITNESS

Motor versionado. Cada archivo `NNNN_Descripcion.sql` lleva `dbo.SchemaVersion` a la versión `NNNN`.

El baseline existente es **Version = 1** (sin archivo `0001_*.sql`).

Migraciones de prueba del motor (no tocan tablas de negocio):

- `0002_MigrationEngineTest.sql` → tabla `dbo.MigrationEngineTest`
- `0003_MigrationEngineTestColumn.sql` → columna `AppliedBy` (usa `GO` entre ALTER y UPDATE)
- `0004_MigrationEngineUpdateManagerTest.sql` → columna `UpdateManagerMarker` (prueba Update Manager V0)

**Histórico — no borrar ni renumerar:** `0002`–`0004` ya fueron aplicadas en entornos de desarrollo
y forman parte del historial de `dbo.SchemaVersion`. Aunque son migraciones de prueba del motor,
permanecen en el repo. La siguiente migración real de negocio debe ser `0005_...sql` o superior.

Convención: no referenciar en el mismo batch una columna recién agregada; separar con `GO`. El runner ejecuta esos batches en la misma transacción.

## Cómo se aplican

`DL.Migrations.MigrationRunner` (invocado por `BLL.SchemaMigrationBLL`):

1. Lee la versión actual (`EsActual = 1`).
2. Descubre `Database/Migrations/*.sql`.
3. Ejecuta pendientes en orden, cada una en una transacción.
4. Si falla: `ROLLBACK` y **no** avanza `SchemaVersion`.

## EnsureSchema() — reemplazo futuro (no en esta fase)

Siguen activos y no se modificaron:

| Mecanismo | Qué crea | Migración futura sugerida |
|-----------|----------|---------------------------|
| `CongelacionDAL.EnsureSchema()` | `dbo.CongelacionesMembresia` + índice | `000N_CongelacionesMembresia.sql` (CREATE IF NOT EXISTS) y luego quitar la llamada runtime |
| `ClienteFichaSaludDAL.EnsureSchema()` | `dbo.ClienteFichaSalud` + columnas | `000N_ClienteFichaSalud.sql` (tabla + ALTERs) y luego quitar `AsegurarColumna` runtime |

Hasta esas migraciones, `EnsureSchema()` sigue siendo la red de seguridad en PCs que aún no tienen esas tablas.
