using DTO;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DL
{
    public class UsuarioDAL
    {
        private readonly DBHelper db = new DBHelper();

        public List<string> ObtenerPermisosPorRol(int rolId)
        {
            List<string> permisos = new List<string>();

            string query = @"
        SELECT p.Nombre
        FROM RolPermisos rp
        INNER JOIN Permisos p ON rp.PermisoId = p.Id
        WHERE rp.RolId = @RolId";

            SqlParameter[] p =
            {
        new SqlParameter("@RolId", rolId)
    };

            DataTable dt = db.ExecuteQuery(query, p);

            foreach (DataRow row in dt.Rows)
            {
                permisos.Add(row["Nombre"]?.ToString() ?? string.Empty);
            }

            return permisos;
        }
        public UsuarioDTO? LoginPorId(int idUsuario, string clave)
        {
            string query = @"
        SELECT 
            u.Id,
            u.Usuario,
            u.RolId,
            r.Nombre AS Rol
        FROM Usuarios u
        INNER JOIN Roles r ON u.RolId = r.Id
        WHERE u.Id = @Id 
          AND u.Clave = @Clave 
          AND u.Activo = 1";

            SqlParameter[] p =
            {
        new SqlParameter("@Id", idUsuario),
        new SqlParameter("@Clave", clave)
    };

            DataTable dt = db.ExecuteQuery(query, p);

            if (dt.Rows.Count == 0) return null;

            return new UsuarioDTO
            {
                Id = Convert.ToInt32(dt.Rows[0]["Id"]),
                Usuario = dt.Rows[0]["Usuario"].ToString(),
                RolId = Convert.ToInt32(dt.Rows[0]["RolId"]),
                Rol = dt.Rows[0]["Rol"].ToString()?.Trim().ToUpper()
            };
        }

        public DataTable TraerUsuariosActivos()
        {
            string query = "SELECT u.Id, u.Usuario, r.Nombre AS Rol FROM Usuarios u INNER JOIN Roles r ON u.RolId=r.Id WHERE u.Activo=1";
            return db.ExecuteQuery(query);
        }
        public void CambiarClave(int usuarioId, string nuevaClave)
        {
            string query = "UPDATE Usuarios SET Clave = @Clave WHERE Id = @Id";

            SqlParameter[] p = {
                new SqlParameter("@Clave", nuevaClave),
                new SqlParameter("@Id", usuarioId)
            };

            db.ExecuteNonQuery(query, p);
        }
    }
}