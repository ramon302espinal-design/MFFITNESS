# Esquema BD — MFFITNESS

Base de datos LocalDB: **MF CYBER DB**.

El acceso en código es por `DALL` (`DBHelper` + consultas / stored procedures). No hay migraciones automáticas al arrancar la UI.

## Scripts en el repo

Ver índice actualizado: [`Scripts/README.md`](Scripts/README.md).

- **Esquema / migraciones útiles:** carpeta `Scripts\`
- **One-shots históricos (Fix/Prueba/Limpiar):** `Scripts\archive\`

## DetalleCaja — monto cero

Si un CHECK impide `Monto = 0`, usar el script canónico:

`Scripts\Adjust_DetalleCaja_Monto_Check.sql`

Antes de alterar constraints en producción: backup.
