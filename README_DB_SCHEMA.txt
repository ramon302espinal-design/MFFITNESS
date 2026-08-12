# Esquema BD — MFFITNESS

Base de datos LocalDB: **MF CYBER DB**.

El acceso en código es por `DALL` (`DBHelper` + consultas / stored procedures).

La versión del esquema vive en `dbo.SchemaVersion`. El motor de migraciones está en `DL.Migrations.MigrationRunner` y los archivos en [`Database/Migrations/`](Database/Migrations/).

## Scripts en el repo

Ver índice actualizado: [`Scripts/README.md`](Scripts/README.md).

- **Esquema / migraciones útiles:** carpeta `Scripts\`
- **One-shots históricos (Fix/Prueba/Limpiar):** `Scripts\archive\`

## DetalleCaja — monto cero

Si un CHECK impide `Monto = 0`, usar el script canónico:

`Scripts\Adjust_DetalleCaja_Monto_Check.sql`

Antes de alterar constraints en producción: backup.
