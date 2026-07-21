using DL;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Historial de cierres. El cierre operativo vive en <see cref="CajaServiceBLL.CerrarCajaConCuadre"/>.
    /// </summary>
    public class CierreCajaBLL
    {
        private readonly CierreCajaDAL cierreCajaDAL = new CierreCajaDAL();

        public void EliminarCierre(int id)
        {
            cierreCajaDAL.EliminarCierre(id);
        }

        public DataTable ObtenerHistorial()
        {
            return cierreCajaDAL.ObtenerHistorialCierres();
        }
    }
}
