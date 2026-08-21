using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Inversiones CRM — FASE 6.14: InvestmentService (sin lógica en el Form).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaInversiones : Form
    {
        public FrmAnaInversiones()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            IReadOnlyList<InvestmentSummary>? summaries =
                CrmInvestmentUiBinder.TryLoadAllSummaries(out string? error);

            if (summaries == null)
            {
                MessageBox.Show(
                    "No se pudieron cargar inversiones.\n" +
                    (error ?? "") +
                    "\n\n¿Migración 0009 aplicada?",
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            decimal capitalTotal = summaries.Where(s => s.HasReliableCost).Sum(s => s.CapitalInvested);
            decimal recovered = summaries.Sum(s => s.CapitalRecovered);
            decimal frozen = summaries.Sum(s => s.FrozenCapital);
            decimal profit = summaries.Sum(s => s.RealizedProfit);

            int activas = summaries.Count(s =>
                s.Status is InvestmentStatus.Activa or InvestmentStatus.Recuperada or InvestmentStatus.Planificada);
            int cerradas = summaries.Count(s =>
                s.Status is InvestmentStatus.Cerrada or InvestmentStatus.ConPerdida);

            InvestmentSummary? bestRoi = summaries
                .Where(s => s.RoiRealizedPct.HasValue)
                .OrderByDescending(s => s.RoiRealizedPct)
                .FirstOrDefault();

            InvestmentSummary? topDetail = summaries
                .OrderByDescending(s => s.CapitalInvested)
                .FirstOrDefault();

            lblResumenValue.Text = CrmInvestmentUiBinder.Money(capitalTotal);
            lblResumenDesc.Text =
                $"Recuperado {CrmInvestmentUiBinder.Money(recovered)} · " +
                $"Congelado {CrmInvestmentUiBinder.Money(frozen)} · " +
                $"Ganancia {CrmInvestmentUiBinder.Money(profit)}";

            lblInversionesactivasValue.Text = CrmInvestmentUiBinder.Count(activas);
            lblInversionesactivasDesc.Text = summaries.Count > 0
                ? $"{CrmInvestmentUiBinder.Count(summaries.Count)} totales"
                : "Sin inversiones creadas";

            lblInversionescerradasValue.Text = CrmInvestmentUiBinder.Count(cerradas);
            lblInversionescerradasDesc.Text = summaries.Count(s => s.IsLoss) > 0
                ? $"{summaries.Count(s => s.IsLoss)} con pérdida"
                : "Cerradas / con pérdida";

            lblHistorialValue.Text = bestRoi != null
                ? CrmInvestmentUiBinder.Pct(bestRoi.RoiRealizedPct)
                : "N/D";
            lblHistorialDesc.Text = bestRoi != null
                ? $"Mejor ROI: {bestRoi.Name}"
                : "Sin ROI confiable";

            InvestmentSummary? topTrapped = CrmInvestmentUiBinder
                .TryRanking(InvestmentRankKind.ByFrozenCapitalDesc, out _, top: 1)
                ?.FirstOrDefault()
                ?.Summary;

            if (topTrapped != null && topTrapped.FrozenCapital > 0)
            {
                lblDetalleValue.Text = CrmInvestmentUiBinder.Money(topTrapped.FrozenCapital);
                lblDetalleDesc.Text =
                    $"Mayor capital atrapado: {topTrapped.Name} · " +
                    CrmInvestmentUiBinder.StatusLabel(topTrapped.Status);
            }
            else if (topDetail != null)
            {
                lblDetalleValue.Text = CrmInvestmentUiBinder.StatusLabel(topDetail.Status);
                lblDetalleDesc.Text =
                    $"{topDetail.Name} · Capital {CrmInvestmentUiBinder.Money(topDetail.CapitalInvested)}" +
                    (topDetail.PaybackDays.HasValue
                        ? $" · Payback {topDetail.PaybackDays}d"
                        : "");
            }
            else
            {
                lblDetalleValue.Text = "—";
                lblDetalleDesc.Text = "Cree una inversión y asigne ENTRADAS de stock";
            }
        }
    }
}
