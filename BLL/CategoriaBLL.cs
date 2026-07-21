using DL;
using System.Data;

namespace BLL
{
    public class CategoriaBLL
    {
        private readonly CategoriaDAL dal = new CategoriaDAL();

        public DataTable ObtenerCategorias()
        {
            return dal.ListarCategorias();
        }
    }
}
