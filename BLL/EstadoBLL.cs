using System.Data;
using DL;

namespace BLL
{
    public class EstadoBLL
    {
        private readonly EstadoDAL estadoDAL = new EstadoDAL();

        public DataTable ObtenerEstadoClientes()
        {
            return estadoDAL.ObtenerEstadoClientes();
        }
    }
}
