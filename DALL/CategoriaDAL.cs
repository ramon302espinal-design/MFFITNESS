using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class CategoriaDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ListarCategorias()
        {
            string query = "SELECT * FROM Categorias WHERE Activo = 1";
            return db.ExecuteQuery(query);
        }
    }
}
