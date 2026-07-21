# Scripts SQL — MFFITNESS

Scripts de **esquema / migración** útiles para un entorno nuevo o para documentar cambios ya aplicados en LocalDB `MF CYBER DB`.

La app **no** ejecuta estos `.sql` en runtime. El acceso a datos es vía `DALL` (`DBHelper` + consultas/`sp`).

## Activos (esta carpeta)

| Script | Propósito |
|--------|-----------|
| `CreateCommandAudit.sql` | Auditoría de comandos |
| `CreateMensajesAutomaticos_Table.sql` | Tabla mensajes automáticos |
| `CreateTablasMensajes.sql` | Tablas de mensajería |
| `WhatsApp_ContentSid_Columna.sql` | Columna ContentSid |
| `WhatsApp_Intentos_Columna.sql` | Columna intentos de envío |
| `MejorasDeudas_Ventas.sql` | Mejoras deudas/ventas |
| `MejorasDeudas_Indices.sql` | Índices deudas |
| `MejorasDeudas_ForeignKeys.sql` | FKs deudas |
| `Adjust_DetalleCaja_Monto_Check.sql` | CHECK Monto >= 0 en `DetalleCaja` |
| `Actualizar_sp_ObtenerHistorial_DeudaId.sql` | SP historial con DeudaId |

## Archivo histórico

One-shots ya aplicados, pruebas y fixes urgentes → [`archive/`](archive/).

**No los re-ejecutes en producción** salvo que sepas exactamente qué hacen.
