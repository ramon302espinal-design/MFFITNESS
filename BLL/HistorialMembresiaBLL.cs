using System.Data;
using DL;

namespace BLL
{
    public class HistorialMembresiaBLL
    {
       

        private readonly HistorialMembresiaDAL dal = new HistorialMembresiaDAL();

        public void Registrar(int clienteId, string tipo, int? planId, decimal? monto, string usuario, string nota)
        {
            dal.Insertar(clienteId, tipo, planId, monto, usuario, nota);
        }

        public DataTable ObtenerHistorial()
        {
            return dal.ObtenerHistorial();
        }
    }
}