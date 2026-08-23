using BLL.Models.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Vista hospedada en <see cref="FrmCRMFinanciero"/> que puede recargar por período del header.
    /// </summary>
    public interface ICrmPeriodRefreshable
    {
        void Recargar(ProfitPeriodKind period);
    }
}
