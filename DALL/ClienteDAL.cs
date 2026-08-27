using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class ClienteDAL
    {
        private readonly DBHelper db = new DBHelper();
        private static bool _sexoColumnReady;
        private static readonly object SchemaLock = new();

        public void EnsureSexoColumn()
        {
            if (_sexoColumnReady) return;
            lock (SchemaLock)
            {
                if (_sexoColumnReady) return;

                db.ExecuteNonQuery(@"
IF COL_LENGTH('dbo.Clientes', 'Sexo') IS NULL
    ALTER TABLE dbo.Clientes ADD Sexo NVARCHAR(20) NULL;");

                _sexoColumnReady = true;
            }
        }

        public DataTable ListarClientes()
        {
            EnsureSexoColumn();

            string query = @"SELECT 
                        ID as Id,
                        Nombre,
                        Telefono,
                        Direccion,
                        FechaNacimiento,
                        Sexo
                     FROM dbo.Clientes";

            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Id del cliente técnico VISITANTE (SISTEMA); lo crea si no existe.
        /// </summary>
        public int ObtenerOCrearVisitanteSistema()
        {
            EnsureSexoColumn();

            const string nombre = "VISITANTE (SISTEMA)";
            object? idExistente = db.ExecuteScalar(
                @"SELECT TOP 1 ID FROM dbo.Clientes WHERE Nombre = @Nombre",
                new[] { new SqlParameter("@Nombre", nombre) });

            if (idExistente != null && idExistente != DBNull.Value)
                return Convert.ToInt32(idExistente);

            return InsertarCliente(
                nombre,
                new DateTime(2000, 1, 1),
                "Acceso parcial ATLETA/VISITA",
                "N/A",
                null);
        }

        /// <summary>
        /// Catálogo POS sin el cliente técnico de accesos parciales.
        /// </summary>
        public DataTable ListarClientesParaPos()
        {
            EnsureSexoColumn();

            string query = @"SELECT 
                        ID as Id,
                        Nombre,
                        Telefono,
                        Direccion,
                        FechaNacimiento,
                        Sexo
                     FROM dbo.Clientes
                     WHERE Nombre <> N'VISITANTE (SISTEMA)'
                     ORDER BY Nombre";

            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Clientes del catálogo excepto los que ya están ACTIVO (SSOT Estado).
        /// Usado para integrar miembros ya pagados fuera de la app.
        /// </summary>
        public DataTable ListarClientesNoActivos()
        {
            EnsureSexoColumn();
            new CongelacionDAL().EnsureSchema();

            string query = $@"
                SELECT c.ID AS Id, c.Nombre
                FROM dbo.Clientes c
                {MembresiaEstadoSql.OuterApplyUltimaMembresia}
                WHERE ({MembresiaEstadoSql.CasoEstado}) <> 'ACTIVO'
                ORDER BY c.Nombre";

            return db.ExecuteQuery(query);
        }

        public int InsertarCliente(string nombre, DateTime fechaNacimiento,
                                     string direccion, string telefono, string? sexo = null)
        {
            EnsureSexoColumn();

            string query = @"INSERT INTO dbo.Clientes
                             (Nombre, FechaNacimiento, Direccion, Telefono, Sexo)
                             OUTPUT INSERTED.ID
                             VALUES
                             (@Nombre, @FechaNacimiento, @Direccion, @Telefono, @Sexo)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@FechaNacimiento", fechaNacimiento),
                new SqlParameter("@Direccion", direccion),
                new SqlParameter("@Telefono", telefono),
                new SqlParameter("@Sexo", (object?)sexo ?? DBNull.Value)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros));
        }

        public void ActualizarCliente(int id, string nombre, DateTime fechaNacimiento,
                                      string direccion, string telefono, string? sexo = null)
        {
            EnsureSexoColumn();

            string query = @"UPDATE dbo.Clientes
                             SET Nombre = @Nombre,
                                 FechaNacimiento = @FechaNacimiento,
                                 Direccion = @Direccion,
                                 Telefono = @Telefono,
                                 Sexo = @Sexo
                             WHERE ID = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@FechaNacimiento", fechaNacimiento.Date),
                new SqlParameter("@Direccion", direccion),
                new SqlParameter("@Telefono", telefono),
                new SqlParameter("@Sexo", (object?)sexo ?? DBNull.Value)
            };

            int filas = db.ExecuteNonQuery(query, parametros);
            if (filas <= 0)
                throw new InvalidOperationException("No se encontró el cliente a actualizar.");
        }

        /// <summary>
        /// Estado de membresía SSOT (misma regla que Estado / dashboard).
        /// </summary>
        public string ObtenerEstadoMembresia(int clienteId)
        {
            if (clienteId <= 0)
                return "SIN MEMBRESIA";

            new CongelacionDAL().EnsureSchema();

            string query = $@"
                SELECT {MembresiaEstadoSql.CasoEstado} AS Estado
                FROM Clientes c
                {MembresiaEstadoSql.OuterApplyUltimaMembresia}
                WHERE c.ID = @Id";

            object? result = db.ExecuteScalar(query, new[] { new SqlParameter("@Id", clienteId) });
            string estado = Convert.ToString(result)?.Trim() ?? string.Empty;
            return string.IsNullOrEmpty(estado) ? "SIN MEMBRESIA" : estado.ToUpperInvariant();
        }

        /// <summary>
        /// Elimina el cliente y sus vínculos operativos en una transacción
        /// (orden seguro ante FKs). No aplica reglas de negocio: las valida BLL.
        /// </summary>
        public void EliminarCliente(int id)
        {
            if (id <= 0)
                throw new InvalidOperationException("Cliente inválido.");

            new CongelacionDAL().EnsureSchema();

            using SqlConnection conn = db.GetConnection();
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();

            try
            {
                void Exec(string sql)
                {
                    using var cmd = new SqlCommand(sql, conn, tx);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                // Ventas / detalle
                Exec(@"DELETE FROM dbo.DetalleVentas
                       WHERE VentaId IN (SELECT Id FROM dbo.Ventas WHERE ClienteId = @Id)");
                Exec("DELETE FROM dbo.Ventas WHERE ClienteId = @Id");

                // Deudas y satélites
                Exec(@"DELETE FROM dbo.PagosDeuda
                       WHERE DeudaId IN (SELECT Id FROM dbo.Deudas WHERE ClienteId = @Id)");
                Exec("DELETE FROM dbo.HistorialDeudas WHERE ClienteId = @Id");
                Exec(@"DELETE FROM dbo.HistorialDeudas
                       WHERE DeudaId IN (SELECT Id FROM dbo.Deudas WHERE ClienteId = @Id)");
                Exec("DELETE FROM dbo.Deudas WHERE ClienteId = @Id");

                // Caja: conservar movimiento, desvincular miembro
                Exec("UPDATE dbo.DetalleCaja SET ClienteId = NULL WHERE ClienteId = @Id");

                Exec("DELETE FROM dbo.RegistroMensajes WHERE ClienteId = @Id");
                Exec("DELETE FROM dbo.ClienteFichaSalud WHERE ClienteId = @Id");
                Exec("DELETE FROM dbo.CongelacionesMembresia WHERE ClienteId = @Id");
                Exec("DELETE FROM dbo.Pagos WHERE ClienteId = @Id");
                Exec("DELETE FROM dbo.Membresias WHERE ClienteId = @Id");
                Exec("DELETE FROM dbo.HistorialMembresias WHERE ClienteId = @Id");
                Exec("DELETE FROM dbo.Clientes WHERE ID = @Id");

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public DataRow? ObtenerClientePorId(int id)
        {
            EnsureSexoColumn();

            string query = @"SELECT ID as Id, Nombre, Telefono, Direccion, FechaNacimiento, Sexo
                             FROM dbo.Clientes
                             WHERE ID = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id)
            };

            DataTable dt = db.ExecuteQuery(query, parametros);
            return dt?.Rows.Count > 0 ? dt.Rows[0] : null;
        }
    }
}
