using BLL.Models.Crm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Motor de alertas de capital (FASE 7.11).
    /// Forms solo muestran; no recalculan.
    /// </summary>
    public class InventoryAlertService
    {
        private readonly InventoryFinancialService _financial = new();

        /// <summary>% de inventario inmovilizado que dispara alerta global HighImmobilizedShare.</summary>
        public const decimal HighImmobilizedShareThresholdPct = 25m;

        public InventoryAlertReport GetAlerts(DateTime? asOf = null, int? top = null)
        {
            InventoryFinancialSummary summary = _financial.GetInventoryFinancials(null, null, asOf);
            InventoryRiskReport risk = _financial.GetInventoryRiskReport(asOf);
            InventoryHealthThresholds t = InventoryHealthThresholds.Default;

            var alerts = new List<InventoryAlert>();

            if (summary.FrozenSharePct.HasValue
                && summary.FrozenSharePct.Value >= HighImmobilizedShareThresholdPct
                && summary.FrozenCapitalTotal > 0)
            {
                alerts.Add(new InventoryAlert
                {
                    Kind = InventoryAlertKind.HighImmobilizedShare,
                    Priority = summary.FrozenSharePct.Value >= 40m
                        ? InventoryAlertPriority.Critical
                        : InventoryAlertPriority.High,
                    Message =
                        $"Capital inmovilizado clasificado: {summary.FrozenSharePct.Value:N2}% " +
                        $"({summary.FrozenCapitalTotal:N2}) del inventario.",
                    CapitalAmount = summary.FrozenCapitalTotal
                });
            }

            foreach (InventoryFinancialRow row in summary.Rows.Where(r => r.Stock > 0))
            {
                if (row.HealthStatus == InventoryHealthStatus.Critical)
                {
                    alerts.Add(ProductAlert(
                        InventoryAlertKind.CriticalCapital,
                        ResolvePriorityByCapital(row.InventoryCapital, t, floor: InventoryAlertPriority.High),
                        row,
                        $"Crítico: capital {row.InventoryCapital:N2}, idle {FormatIdle(row)}."));
                }
                else if (row.HealthStatus == InventoryHealthStatus.Frozen)
                {
                    alerts.Add(ProductAlert(
                        InventoryAlertKind.FrozenCapital,
                        ResolvePriorityByCapital(row.InventoryCapital, t, floor: InventoryAlertPriority.Medium),
                        row,
                        $"Congelado: capital {row.InventoryCapital:N2}, idle {FormatIdle(row)}."));
                }
                else if (row.HealthStatus == InventoryHealthStatus.Slow
                         && row.InventoryCapital >= t.MinMaterialCapital)
                {
                    alerts.Add(ProductAlert(
                        InventoryAlertKind.SlowCapital,
                        InventoryAlertPriority.Low,
                        row,
                        $"Lento: capital {row.InventoryCapital:N2}, idle {FormatIdle(row)}."));
                }

                if (row.FlagNeverSold
                    && row.HealthStatus != InventoryHealthStatus.New
                    && row.InventoryCapital >= t.MinMaterialCapital)
                {
                    // Evitar duplicar si ya hay Critical/Frozen never-sold del mismo producto
                    if (!alerts.Any(a => a.ProductId == row.ProductId
                                         && a.Kind is InventoryAlertKind.CriticalCapital
                                             or InventoryAlertKind.FrozenCapital))
                    {
                        alerts.Add(ProductAlert(
                            InventoryAlertKind.NeverSold,
                            ResolvePriorityByCapital(row.InventoryCapital, t, floor: InventoryAlertPriority.Medium),
                            row,
                            $"Nunca vendido: capital {row.InventoryCapital:N2}, {FormatIdle(row)}."));
                    }
                }

                if (row.FlagOverstock && row.InventoryCapital >= t.MinMaterialCapital)
                {
                    alerts.Add(ProductAlert(
                        InventoryAlertKind.Overstock,
                        InventoryAlertPriority.Medium,
                        row,
                        $"Sobreinventario: cobertura {row.DaysOfCover:N0} días, capital {row.InventoryCapital:N2}."));
                }

                if (row.FlagStockoutRisk)
                {
                    alerts.Add(ProductAlert(
                        InventoryAlertKind.StockoutRisk,
                        InventoryAlertPriority.High,
                        row,
                        $"Riesgo de quiebre: stock {row.Stock} ≤ mínimo {row.StockMinimo}, demanda activa."));
                }

                if (row.HealthStatus is InventoryHealthStatus.Frozen or InventoryHealthStatus.Critical
                    && row.PotentialProfit < 0
                    && row.InventoryCapital >= t.MinMaterialCapital)
                {
                    alerts.Add(ProductAlert(
                        InventoryAlertKind.AtRiskLoss,
                        ResolvePriorityByCapital(row.InventoryCapital, t, floor: InventoryAlertPriority.High),
                        row,
                        $"Riesgo de pérdida latente: potencial {row.PotentialProfit:N2}."));
                }
            }

            var ordered = alerts
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CapitalAmount ?? 0m)
                .ThenBy(a => a.ProductName)
                .ThenBy(a => a.Kind)
                .ToList();

            if (top.HasValue && top.Value > 0)
                ordered = ordered.Take(top.Value).ToList();

            return new InventoryAlertReport
            {
                TotalAlerts = ordered.Count,
                CriticalCount = ordered.Count(a => a.Priority == InventoryAlertPriority.Critical),
                HighCount = ordered.Count(a => a.Priority == InventoryAlertPriority.High),
                MediumCount = ordered.Count(a => a.Priority == InventoryAlertPriority.Medium),
                LowCount = ordered.Count(a => a.Priority == InventoryAlertPriority.Low),
                ImmobilizedCapital = risk.ImmobilizedCapital,
                AtRiskCapital = risk.AtRiskCapital,
                FrozenSharePct = summary.FrozenSharePct,
                Alerts = ordered
            };
        }

        private static InventoryAlert ProductAlert(
            InventoryAlertKind kind,
            InventoryAlertPriority priority,
            InventoryFinancialRow row,
            string message)
            => new()
            {
                Kind = kind,
                Priority = priority,
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                Category = row.Category,
                Message = message,
                CapitalAmount = row.InventoryCapital > 0 ? row.InventoryCapital : null,
                IdleDays = row.IdleDays,
                DaysOfCover = row.DaysOfCover
            };

        /// <summary>
        /// RD$500 → Low; material → Medium+; CriticalCapitalMin → High/Critical según floor.
        /// </summary>
        public static InventoryAlertPriority ResolvePriorityByCapital(
            decimal capital,
            InventoryHealthThresholds? thresholds = null,
            InventoryAlertPriority floor = InventoryAlertPriority.Low)
        {
            InventoryHealthThresholds t = thresholds ?? InventoryHealthThresholds.Default;

            InventoryAlertPriority p;
            if (capital >= t.CriticalCapitalMin)
                p = InventoryAlertPriority.Critical;
            else if (capital >= t.MinMaterialCapital)
                p = InventoryAlertPriority.High;
            else if (capital >= 500m)
                p = InventoryAlertPriority.Medium;
            else
                p = InventoryAlertPriority.Low;

            return (InventoryAlertPriority)Math.Max((int)p, (int)floor);
        }

        private static string FormatIdle(InventoryFinancialRow row)
            => row.IdleDays.HasValue
                ? $"{row.IdleDays.Value} días ({row.IdleKind})"
                : "idle N/D";
    }
}
