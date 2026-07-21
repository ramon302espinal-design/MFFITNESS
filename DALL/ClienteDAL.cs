using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class ClienteDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ListarClientes()
        {
            string query = @"SELECT 
                        ID as Id,
                        Nombre,
                        Telefono,
                        Direccion,
                        FechaNacimiento
                     FROM dbo.Clientes";

            return db.ExecuteQuery(query);
        }

        public int InsertarCliente(string nombre, DateTime fechaNacimiento,
                                     string direccion, string telefono)
        {
            string query = @"INSERT INTO dbo.Clientes
                             (Nombre, FechaNacimiento, Direccion, Telefono)
                             OUTPUT INSERTED.ID
                             VALUES
                             (@Nombre, @FechaNacimiento, @Direccion, @Telefono)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@FechaNacimiento", fechaNacimiento),
                new SqlParameter("@Direccion", direccion),
                new SqlParameter("@Telefono", telefono)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parametros));
        }

        public void ActualizarCliente(int id, string nombre, DateTime fechaNacimiento,
                                      string direccion, string telefono)
        {
            string query = @"UPDATE dbo.Clientes
                             SET Nombre = @Nombre,
                                 FechaNacimiento = @FechaNacimiento,
                                 Direccion = @Direccion,
                                 Telefono = @Telefono
                             WHERE ID = @Id";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@FechaNacimiento", fechaNacimiento.Date),
                new SqlParameter("@Direccion", direccion),
                new SqlParameter("@Telefono", telefono)
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
            string query = @"SELECT ID as Id, Nombre, Telefono, Direccion, FechaNacimiento
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
