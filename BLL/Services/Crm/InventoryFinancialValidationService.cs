using BLL.Models.Crm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Validaciones de integridad financiera de inventario (FASE 4.8).
    /// No corrige datos; detecta anomalías para CRM/alertas.
    /// </summary>
    public class InventoryFinancialValidationService
    {
        private readonly InventoryFinancialService _financial = new();

        public InventoryFinancialValidationReport Validate(DateTime? asOf = null)
        {
            InventoryFinancialSummary summary = _financial.GetInventoryFinancials(null, null, asOf);
            var anomalies = new List<InventoryFinancialAnomaly>();

            foreach (InventoryFinancialRow row in summary.Rows)
            {
                if (row.FlagNegativeStock)
                {
                    anomalies.Add(new InventoryFinancialAnomaly
                    {
                        Code = InventoryFinancialAnomalyCode.NegativeStock,
                        Severity = InventoryFinancialAnomalySeverity.Critical,
                        ProductId = row.ProductId,
                        ProductName = row.ProductName,
                        Message = "Stock negativo: anomalía de inventario. No se normaliza a cero.",
                        Stock = row.Stock,
                        UnitCost = row.UnitCost,
                        SalePrice = row.SalePrice
                    });
                }

                if (row.Stock > 0 && row.FlagNoCost)
                {
                    anomalies.Add(new InventoryFinancialAnomaly
                    {
                        Code = InventoryFinancialAnomalyCode.CostNotDefined,
                        Severity = InventoryFinancialAnomalySeverity.Warning,
                        ProductId = row.ProductId,
                        ProductName = row.ProductName,
                        Message = "Costo no definido: no se calcula capital congelado, margen ni ROI ficticios.",
                        Stock = row.Stock,
                        UnitCost = row.UnitCost,
                        SalePrice = row.SalePrice
                    });
                }

                if (row.Stock > 0 && row.FlagNoPrice)
                {
                    anomalies.Add(new InventoryFinancialAnomaly
                    {
                        Code = InventoryFinancialAnomalyCode.PriceNotDefined,
                        Severity = InventoryFinancialAnomalySeverity.Warning,
                        ProductId = row.ProductId,
                        ProductName = row.ProductName,
                        Message = "Precio no definido: no se calcula valor/ganancia potencial.",
                        Stock = row.Stock,
                        UnitCost = row.UnitCost,
                        SalePrice = row.SalePrice
                    });
                }

                if (row.FlagUncostedSales)
                {
                    anomalies.Add(new InventoryFinancialAnomaly
                    {
                        Code = InventoryFinancialAnomalyCode.UncostedSales,
                        Severity = InventoryFinancialAnomalySeverity.Info,
                        ProductId = row.ProductId,
                        ProductName = row.ProductName,
                        Message = "Hay ventas sin snapshot de costo (histórico previo a cutover). Ganancia realizada parcial.",
                        Stock = row.Stock,
                        UnitCost = row.UnitCost,
                        SalePrice = row.SalePrice
                    });
                }

                if (row.FlagNoRotation)
                {
                    anomalies.Add(new InventoryFinancialAnomaly
                    {
                        Code = InventoryFinancialAnomalyCode.NoRotationWithStock,
                        Severity = InventoryFinancialAnomalySeverity.Warning,
                        ProductId = row.ProductId,
                        ProductName = row.ProductName,
                        Message = "Stock con capital y sin ventas en el alcance: posible capital congelado / sin rotación.",
                        Stock = row.Stock,
                        UnitCost = row.UnitCost,
                        SalePrice = row.SalePrice,
                        FrozenCapitalAtRisk = row.InventoryCapital > 0 ? row.InventoryCapital : null
                    });
                }

                if (row.Stock > 0 && row.StockMinimo > 0 && row.Stock <= row.StockMinimo)
                {
                    anomalies.Add(new InventoryFinancialAnomaly
                    {
                        Code = InventoryFinancialAnomalyCode.BelowStockMinimum,
                        Severity = InventoryFinancialAnomalySeverity.Info,
                        ProductId = row.ProductId,
                        ProductName = row.ProductName,
                        Message = $"Stock ({row.Stock}) en o bajo mínimo ({row.StockMinimo}).",
                        Stock = row.Stock,
                        UnitCost = row.UnitCost,
                        SalePrice = row.SalePrice
                    });
                }
            }

            var ordered = anomalies
                .OrderByDescending(a => a.Severity)
                .ThenBy(a => a.ProductName)
                .ThenBy(a => a.Code)
                .ToList();

            int critical = ordered.Count(a => a.Severity == InventoryFinancialAnomalySeverity.Critical);
            int warning = ordered.Count(a => a.Severity == InventoryFinancialAnomalySeverity.Warning);
            int info = ordered.Count(a => a.Severity == InventoryFinancialAnomalySeverity.Info);

            return new InventoryFinancialValidationReport
            {
                TotalAnomalies = ordered.Count,
                CriticalCount = critical,
                WarningCount = warning,
                InfoCount = info,
                HasBlockingIssues = critical > 0,
                Anomalies = ordered
            };
        }
    }
}
