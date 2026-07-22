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

        public void EliminarCliente(int id)
        {
            string query = "DELETE FROM dbo.Clientes WHERE ID = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id)
            };

            db.ExecuteNonQuery(query, parametros);
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
