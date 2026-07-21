using DL;
using DTO;
using System.Data;

namespace BLL
{
    public class PlanBLL
    {
        private PlanDAL dal = new PlanDAL();

        public DataTable ObtenerPlanes()
        {
            return dal.ObtenerPlanes();
        }

        public PlanDTO? ObtenerPlan(int id)
        {
            return dal.ObtenerPlan(id);
        }
    }
}