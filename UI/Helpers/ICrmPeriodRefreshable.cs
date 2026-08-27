using BLL.Models.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Vista hospedada en <see cref="FrmCRMFinanciero"/> que puede recargar por período del header.
    /// </summary>
    public interface ICrmPeriodRefreshable
    {
        /// <param name="customFrom">Inclusive; solo para <see cref="ProfitPeriodKind.Custom"/>.</param>
        /// <param name="customToExclusive">Exclusivo; solo para Custom.</param>
        void Recargar(
            ProfitPeriodKind period,
            DateTime? customFrom = null,
            DateTime? customToExclusive = null);
    }
}
