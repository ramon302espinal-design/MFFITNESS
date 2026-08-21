using BLL.Models.Crm;
using DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Inversiones CRM (FASE 6.3–6.9).
    /// Capital, recuperación, congelado, ganancia y ROI sobre capital invertido.
    /// </summary>
    public class InvestmentService
    {
        private readonly CrmInvestmentDAL dal = new();

        public Investment Create(
            string name,
            DateTime? startDate = null,
            string? description = null,
            string? notes = null,
            string? user = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nombre de inversión requerido.");

            DateTime start = (startDate ?? DateTime.Today).Date;
            int id = dal.CrearInversion(
                name.Trim(),
                description,
                start,
                (byte)InvestmentStatus.Planificada,
                notes,
                user);

            return GetRequired(id);
        }

        public IReadOnlyList<Investment> List()
        {
            DataTable table = dal.ListarInversiones();
            var list = new List<Investment>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
                list.Add(MapInvestment(row));
            return list;
        }

        public Investment? Get(int id)
        {
            DataRow? row = dal.ObtenerInversion(id);
            return row == null ? null : MapInvestment(row);
        }

        public Investment GetRequired(int id)
            => Get(id) ?? throw new Exception($"Inversión {id} no encontrada.");

        public IReadOnlyList<InvestmentLine> GetLines(int investmentId)
        {
            _ = GetRequired(investmentId);
            DataTable table = dal.ListarLineas(investmentId);
            var list = new List<InvestmentLine>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
                list.Add(MapLine(row));
            return list;
        }

        /// <summary>
        /// Productos de la inversión (agregado de ENTRADAS). FASE 6.4.
        /// </summary>
        public IReadOnlyList<InvestmentProductRow> GetProducts(int investmentId)
        {
            _ = GetRequired(investmentId);
            DataTable table = dal.ListarProductosPorInversion(investmentId);
            var list = new List<InvestmentProductRow>(table.Rows.Count);

            foreach (DataRow row in table.Rows)
            {
                int qty = GetInt(row, "QuantityPurchased");
                int qtyWithCost = GetInt(row, "QtyWithCost");
                decimal capital = RoundMoney(GetDecimal(row, "CapitalAssigned"));
                int withoutCost = GetInt(row, "EntriesWithoutCost");
                bool reliable = qtyWithCost > 0 && capital > 0;

                decimal? avgCost = null;
                if (reliable && qtyWithCost > 0)
                    avgCost = Math.Round(capital / qtyWithCost, 4, MidpointRounding.AwayFromZero);

                list.Add(new InvestmentProductRow
                {
                    ProductId = GetInt(row, "ProductoId"),
                    ProductName = GetString(row, "Producto"),
                    Category = GetString(row, "Categoria"),
                    QuantityPurchased = qty,
                    AverageUnitCost = avgCost,
                    CapitalAssigned = capital,
                    EntryCount = GetInt(row, "EntryCount"),
                    EntriesWithoutCost = withoutCost,
                    HasReliableCost = reliable
                });
            }

            return list;
        }

        /// <summary>
        /// Resumen financiero (FASE 6.5–6.10): capital, recuperación, ganancia, ROI, payback.
        /// </summary>
        public InvestmentSummary GetSummary(int investmentId)
        {
            Investment inv = GetRequired(investmentId);
            IReadOnlyList<InvestmentLine> lines = GetLines(investmentId);

            var capitals = new List<decimal>(lines.Count);
            var fifoEntries = new List<InvestmentFifoEntry>(lines.Count);

            foreach (InvestmentLine line in lines)
            {
                decimal cap = InvestmentMath.LineCapital(line.Quantity, line.UnitCost, line.LineCapital);
                if (cap > 0)
                    capitals.Add(cap);

                decimal unit = 0m;
                if (line.UnitCost.HasValue && line.UnitCost.Value > 0)
                    unit = line.UnitCost.Value;
                else if (cap > 0 && line.Quantity > 0)
                    unit = Math.Round(cap / line.Quantity, 4, MidpointRounding.AwayFromZero);

                if (unit > 0 && line.Quantity > 0)
                {
                    fifoEntries.Add(new InvestmentFifoEntry(
                        line.StockMovementId,
                        line.ProductId,
                        line.EntryDate,
                        line.Quantity,
                        unit));
                }
            }

            decimal invested = InvestmentMath.CapitalInvested(capitals);
            List<InvestmentFifoSale> sales = LoadSales(investmentId);
            Dictionary<int, decimal> prices = dal.ObtenerPreciosVentaProductosInversion(investmentId);
            InvestmentFifoResult fifo = InvestmentMath.RunFifo(fifoEntries, sales, prices);

            decimal recovered = fifo.Recovered;
            decimal frozen = fifo.Frozen;
            decimal pending = InvestmentMath.CapitalPending(invested, recovered);
            bool reliable = invested > 0;

            decimal? roiRealized = InvestmentMath.RoiPct(fifo.RealizedProfit, invested);
            decimal? roiPotential = InvestmentMath.RoiPct(fifo.PotentialProfit, invested);
            decimal? roiProjected = InvestmentMath.RoiProjectedPct(
                fifo.RealizedProfit, fifo.PotentialProfit, invested);

            int? paybackDays = InvestmentMath.PaybackDays(
                inv.StartDate, invested, fifoEntries, sales);

            int? daysActive = inv.CloseDate.HasValue
                ? Math.Max(0, (inv.CloseDate.Value.Date - inv.StartDate.Date).Days)
                : Math.Max(0, (DateTime.Today - inv.StartDate.Date).Days);

            return new InvestmentSummary
            {
                InvestmentId = inv.Id,
                Name = inv.Name,
                Status = inv.Status,
                StartDate = inv.StartDate,
                CloseDate = inv.CloseDate,
                CapitalInvested = invested,
                CapitalRecovered = recovered,
                CapitalPending = pending,
                FrozenCapital = frozen,
                RealizedProfit = fifo.RealizedProfit,
                PotentialProfit = fifo.PotentialProfit,
                RoiRealizedPct = roiRealized,
                RoiPotentialPct = roiPotential,
                RoiProjectedPct = roiProjected,
                RecoveryPct = InvestmentMath.RecoveryPct(recovered, invested),
                DaysActive = daysActive,
                PaybackDays = paybackDays,
                HasReliableCost = reliable,
                IsLoss = fifo.RealizedProfit < 0
            };
        }

        /// <summary>Capital invertido (atajo FASE 6.5).</summary>
        public decimal GetCapitalInvested(int investmentId)
            => GetSummary(investmentId).CapitalInvested;

        /// <summary>Capital recuperado = COGS atribuible FIFO (FASE 6.6).</summary>
        public decimal GetCapitalRecovered(int investmentId)
            => GetSummary(investmentId).CapitalRecovered;

        /// <summary>Capital congelado = resto del pool etiquetado × costo (FASE 6.7).</summary>
        public decimal GetFrozenCapital(int investmentId)
            => GetSummary(investmentId).FrozenCapital;

        /// <summary>Ganancia realizada atribuible (FASE 6.8).</summary>
        public decimal GetRealizedProfit(int investmentId)
            => GetSummary(investmentId).RealizedProfit;

        /// <summary>ROI realizado = ganancia / capital invertido (FASE 6.9).</summary>
        public decimal? GetRoiRealizedPct(int investmentId)
            => GetSummary(investmentId).RoiRealizedPct;

        /// <summary>Días de payback (FASE 6.10); null si aún no recupera el capital.</summary>
        public int? GetPaybackDays(int investmentId)
            => GetSummary(investmentId).PaybackDays;

        private List<InvestmentFifoSale> LoadSales(int investmentId)
        {
            DataTable salesTable = dal.ListarVentasDeProductosInversion(investmentId);
            var sales = new List<InvestmentFifoSale>(salesTable.Rows.Count);
            foreach (DataRow row in salesTable.Rows)
            {
                sales.Add(new InvestmentFifoSale(
                    GetInt(row, "SaleLineId"),
                    GetInt(row, "ProductoId"),
                    Convert.ToDateTime(row["SaleDate"]),
                    GetInt(row, "Cantidad"),
                    GetDecimal(row, "Subtotal")));
            }

            return sales;
        }

        /// <summary>
        /// Asigna MovimientosStock ENTRADA a la inversión (1:1 en v1).
        /// Pasa Planificada → Activa automáticamente.
        /// </summary>
        public InvestmentLine AssignEntrada(int investmentId, int stockMovementId)
        {
            _ = GetRequired(investmentId);
            int lineId = dal.AsignarEntrada(investmentId, stockMovementId);
            return GetLines(investmentId).First(l => l.Id == lineId);
        }

        public void UnassignEntrada(int investmentId, int stockMovementId)
        {
            _ = GetRequired(investmentId);
            dal.QuitarEntrada(investmentId, stockMovementId);
        }

        public IReadOnlyList<InvestmentLine> ListAvailableEntradas()
        {
            DataTable table = dal.ListarEntradasDisponibles();
            var list = new List<InvestmentLine>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                decimal? unit = GetDecimalOrNull(row, "CostoUnitario");
                int qty = GetInt(row, "Cantidad");
                decimal? total = GetDecimalOrNull(row, "CostoTotal");
                if (!total.HasValue && unit.HasValue)
                    total = Math.Round(unit.Value * qty, 4, MidpointRounding.AwayFromZero);

                list.Add(new InvestmentLine
                {
                    Id = 0,
                    InvestmentId = 0,
                    StockMovementId = GetInt(row, "MovimientoStockId"),
                    AssignedAt = default,
                    ProductId = GetInt(row, "ProductoId"),
                    ProductName = GetString(row, "Producto"),
                    Quantity = qty,
                    UnitCost = unit,
                    LineCapital = total,
                    EntryDate = Convert.ToDateTime(row["Fecha"]),
                    EntryDescription = GetStringOrNull(row, "Descripcion"),
                    MovementType = "ENTRADA"
                });
            }

            return list;
        }

        public void SetStatus(int investmentId, InvestmentStatus status, DateTime? closeDate = null)
        {
            Investment current = GetRequired(investmentId);
            if (!InvestmentStatusPolicy.CanTransition(current.Status, status))
                throw new Exception($"Transición no permitida: {current.Status} → {status}.");

            DateTime? cierre = status switch
            {
                InvestmentStatus.Cerrada => closeDate ?? DateTime.Today,
                InvestmentStatus.ConPerdida => closeDate ?? current.CloseDate ?? DateTime.Today,
                InvestmentStatus.Planificada => null,
                InvestmentStatus.Activa => null,
                _ => closeDate ?? current.CloseDate
            };

            dal.ActualizarEstado(investmentId, (byte)status, cierre);
        }

        /// <summary>Estado sugerido según métricas (sin persistir). FASE 6.11.</summary>
        public InvestmentStatus SuggestStatus(int investmentId)
            => InvestmentStatusPolicy.SuggestStatus(GetSummary(investmentId));

        /// <summary>
        /// Aplica estado sugerido si la transición es válida. No reabre Cerrada.
        /// </summary>
        public InvestmentStatus SyncStatusFromMetrics(int investmentId)
        {
            Investment current = GetRequired(investmentId);
            InvestmentStatus suggested = InvestmentStatusPolicy.SuggestStatus(GetSummary(investmentId));

            if (suggested == current.Status)
                return current.Status;

            if (!InvestmentStatusPolicy.CanTransition(current.Status, suggested))
                return current.Status;

            DateTime? close = suggested is InvestmentStatus.Cerrada or InvestmentStatus.ConPerdida
                ? (current.CloseDate ?? DateTime.Today)
                : null;

            dal.ActualizarEstado(investmentId, (byte)suggested, close);
            return suggested;
        }

        /// <summary>Sincroniza todas las inversiones. Devuelve cuántas cambiaron de estado.</summary>
        public int SyncAllStatusesFromMetrics()
        {
            int changed = 0;
            foreach (Investment inv in List())
            {
                InvestmentStatus before = inv.Status;
                if (SyncStatusFromMetrics(inv.Id) != before)
                    changed++;
            }

            return changed;
        }

        /// <summary>
        /// Ranking multi-criterio (FASE 6.12). Cada kind es un ordenamiento distinto.
        /// </summary>
        public IReadOnlyList<InvestmentRankRow> GetRanking(
            InvestmentRankKind kind,
            int? top = null,
            bool onlyReliable = true)
        {
            IEnumerable<InvestmentSummary> summaries = List()
                .Select(i => GetSummary(i.Id));

            if (onlyReliable)
                summaries = summaries.Where(s => s.HasReliableCost);

            IOrderedEnumerable<InvestmentSummary> ordered = kind switch
            {
                InvestmentRankKind.ByRealizedProfit =>
                    summaries.OrderByDescending(s => s.RealizedProfit).ThenBy(s => s.Name),
                InvestmentRankKind.ByRoiRealized =>
                    summaries.OrderByDescending(s => s.RoiRealizedPct ?? decimal.MinValue).ThenBy(s => s.Name),
                InvestmentRankKind.ByCapitalInvested =>
                    summaries.OrderByDescending(s => s.CapitalInvested).ThenBy(s => s.Name),
                InvestmentRankKind.ByRecoveryPct =>
                    summaries.OrderByDescending(s => s.RecoveryPct ?? decimal.MinValue).ThenBy(s => s.Name),
                InvestmentRankKind.ByPaybackSpeed =>
                    summaries
                        .Where(s => s.PaybackDays.HasValue)
                        .OrderBy(s => s.PaybackDays!.Value)
                        .ThenByDescending(s => s.RealizedProfit)
                        .ThenBy(s => s.Name),
                InvestmentRankKind.ByFrozenCapitalAsc =>
                    summaries.OrderBy(s => s.FrozenCapital).ThenByDescending(s => s.RealizedProfit).ThenBy(s => s.Name),
                InvestmentRankKind.ByFrozenCapitalDesc =>
                    summaries.OrderByDescending(s => s.FrozenCapital).ThenByDescending(s => s.CapitalInvested).ThenBy(s => s.Name),
                InvestmentRankKind.ByPotentialProfit =>
                    summaries.OrderByDescending(s => s.PotentialProfit).ThenBy(s => s.Name),
                InvestmentRankKind.ByProjectedRoi =>
                    summaries.OrderByDescending(s => s.RoiProjectedPct ?? decimal.MinValue).ThenBy(s => s.Name),
                _ => summaries.OrderByDescending(s => s.RealizedProfit).ThenBy(s => s.Name)
            };

            var list = ordered.ToList();
            if (top.HasValue && top.Value > 0)
                list = list.Take(top.Value).ToList();

            int rank = 0;
            var rows = new List<InvestmentRankRow>(list.Count);
            foreach (InvestmentSummary s in list)
            {
                rank++;
                rows.Add(new InvestmentRankRow
                {
                    Rank = rank,
                    Kind = kind,
                    Summary = s,
                    SortLabel = FormatRankLabel(kind, s)
                });
            }

            return rows;
        }

        private static string FormatRankLabel(InvestmentRankKind kind, InvestmentSummary s) => kind switch
        {
            InvestmentRankKind.ByRealizedProfit => $"Ganancia {s.RealizedProfit:N2}",
            InvestmentRankKind.ByRoiRealized => s.RoiRealizedPct.HasValue ? $"ROI {s.RoiRealizedPct:N2}%" : "ROI N/D",
            InvestmentRankKind.ByCapitalInvested => $"Capital {s.CapitalInvested:N2}",
            InvestmentRankKind.ByRecoveryPct => s.RecoveryPct.HasValue ? $"Recup. {s.RecoveryPct:N2}%" : "Recup. N/D",
            InvestmentRankKind.ByPaybackSpeed => s.PaybackDays.HasValue ? $"{s.PaybackDays} días" : "Sin payback",
            InvestmentRankKind.ByFrozenCapitalAsc => $"Congelado {s.FrozenCapital:N2}",
            InvestmentRankKind.ByFrozenCapitalDesc => $"Atrapado {s.FrozenCapital:N2}",
            InvestmentRankKind.ByPotentialProfit => $"Potencial {s.PotentialProfit:N2}",
            InvestmentRankKind.ByProjectedRoi => s.RoiProjectedPct.HasValue ? $"ROI proy. {s.RoiProjectedPct:N2}%" : "N/D",
            _ => s.Name
        };

        private static Investment MapInvestment(DataRow row) => new()
        {
            Id = GetInt(row, "Id"),
            Name = GetString(row, "Nombre"),
            Description = GetStringOrNull(row, "Descripcion"),
            StartDate = Convert.ToDateTime(row["FechaInicio"]).Date,
            CloseDate = row["FechaCierre"] == DBNull.Value
                ? null
                : Convert.ToDateTime(row["FechaCierre"]).Date,
            Status = (InvestmentStatus)Convert.ToByte(row["Estado"]),
            Notes = GetStringOrNull(row, "Observaciones"),
            CreatedBy = GetStringOrNull(row, "UsuarioCreacion"),
            CreatedAt = Convert.ToDateTime(row["FechaCreacion"])
        };

        private static InvestmentLine MapLine(DataRow row)
        {
            decimal? unit = GetDecimalOrNull(row, "CostoUnitario");
            int qty = GetInt(row, "Cantidad");
            decimal? total = GetDecimalOrNull(row, "CostoTotal");
            if (!total.HasValue && unit.HasValue)
                total = Math.Round(unit.Value * qty, 4, MidpointRounding.AwayFromZero);

            return new InvestmentLine
            {
                Id = GetInt(row, "Id"),
                InvestmentId = GetInt(row, "InversionId"),
                StockMovementId = GetInt(row, "MovimientoStockId"),
                AssignedAt = Convert.ToDateTime(row["FechaAsignacion"]),
                ProductId = GetInt(row, "ProductoId"),
                ProductName = GetString(row, "Producto"),
                Quantity = qty,
                UnitCost = unit,
                LineCapital = total,
                EntryDate = Convert.ToDateTime(row["FechaEntrada"]),
                EntryDescription = GetStringOrNull(row, "Descripcion"),
                MovementType = GetString(row, "TipoMovimiento")
            };
        }

        private static int GetInt(DataRow row, string col)
            => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);

        private static decimal GetDecimal(DataRow row, string col)
            => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);

        private static decimal RoundMoney(decimal v)
            => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static decimal? GetDecimalOrNull(DataRow row, string col)
            => row[col] == DBNull.Value ? null : Convert.ToDecimal(row[col]);

        private static string GetString(DataRow row, string col)
            => row[col] == DBNull.Value ? string.Empty : Convert.ToString(row[col]) ?? string.Empty;

        private static string? GetStringOrNull(DataRow row, string col)
            => row[col] == DBNull.Value ? null : Convert.ToString(row[col]);
    }
}
