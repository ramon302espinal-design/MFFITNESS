using BLL.Models.Crm;
using System.Globalization;

namespace BLL.Services.Crm
{
    /// <summary>Contrato capital liberado / incremento observado (FASE 11.10).</summary>
    public static class BusinessActionCapitalImpactPolicy
    {
        public const string Definition =
            "FASE 11.10: a partir de deltas, reportar capital liberado (↓ inmovilizado/riesgo/inventario) " +
            "e incrementos observados (↑ ingresos/ganancia; margen en pp). " +
            "Lenguaje cauteloso: 'se observó' — nunca 'la acción liberó/causó'.";

        public const string Caution =
            "Los montos son diferencias Before→After del período. " +
            "Pueden influir estacionalidad, mix u otros factores. No atribuir causalidad automática.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Compone impacto de capital / ventas observados desde deltas (FASE 11.10).</summary>
    public static class BusinessActionCapitalImpactComposer
    {
        public static BusinessActionObservedCapitalImpact FromRecord(BusinessActionRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return FromDeltas(record.ActualImpact?.Deltas);
        }

        public static BusinessActionObservedCapitalImpact FromDeltas(
            IReadOnlyList<BusinessActionMetricDelta>? deltas)
        {
            if (deltas == null || deltas.Count == 0)
            {
                return new BusinessActionObservedCapitalImpact
                {
                    Caution = BusinessActionCapitalImpactPolicy.Caution,
                    Narrative = "Sin deltas para estimar capital liberado o incrementos observados."
                };
            }

            decimal? libImm = LiberatedAmount(Find(deltas, "capital.immobilized"));
            decimal? libRisk = LiberatedAmount(Find(deltas, "capital.at_risk"));
            decimal? libInv = LiberatedAmount(Find(deltas, "capital.inventory"));

            decimal? revUp = IncreaseAmount(Find(deltas, "sales.revenue"));
            decimal? profitUp = IncreaseAmount(Find(deltas, "profit.realized"));

            BusinessActionMetricDelta? margin = Find(deltas, "profit.margin_pct");
            decimal? marginPp = margin != null
                ? BusinessActionMetricDeltaMath.AbsoluteDelta(margin.Before, margin.After)
                : null;

            decimal? totalLib = null;
            if (libImm.HasValue || libRisk.HasValue)
                totalLib = (libImm ?? 0m) + (libRisk ?? 0m);
            else if (libInv.HasValue)
                totalLib = libInv;

            bool anyLib = totalLib is > 0 || libImm is > 0 || libRisk is > 0 || libInv is > 0;

            bool hasSignal = anyLib
                || revUp.HasValue
                || profitUp.HasValue
                || marginPp.HasValue;

            var impact = new BusinessActionObservedCapitalImpact
            {
                LiberatedImmobilized = libImm,
                LiberatedAtRisk = libRisk,
                LiberatedInventoryCapital = libInv,
                TotalLiberatedCapital = anyLib && totalLib is > 0 ? totalLib : null,
                ObservedRevenueIncrease = revUp,
                ObservedProfitIncrease = profitUp,
                ObservedMarginChangePp = marginPp,
                HasAnySignal = hasSignal,
                Caution = BusinessActionCapitalImpactPolicy.Caution,
                Narrative = BuildNarrative(libImm, libRisk, libInv, anyLib ? totalLib : null, revUp, profitUp, marginPp)
            };

            return impact;
        }

        /// <summary>Monto liberado = Before − After cuando hubo reducción; null si no ↓.</summary>
        public static decimal? LiberatedAmount(BusinessActionMetricDelta? delta)
        {
            if (delta?.Before == null || delta.After == null)
                return null;
            decimal drop = delta.Before.Value - delta.After.Value;
            return drop > 0m
                ? Math.Round(drop, 2, MidpointRounding.AwayFromZero)
                : null;
        }

        /// <summary>Incremento observado = After − Before cuando hubo alza; null si no ↑.</summary>
        public static decimal? IncreaseAmount(BusinessActionMetricDelta? delta)
        {
            if (delta?.Before == null || delta.After == null)
                return null;
            decimal up = delta.After.Value - delta.Before.Value;
            return up > 0m
                ? Math.Round(up, 2, MidpointRounding.AwayFromZero)
                : null;
        }

        public static string BuildNarrative(
            decimal? liberatedImmobilized,
            decimal? liberatedAtRisk,
            decimal? liberatedInventory,
            decimal? totalLiberated,
            decimal? revenueIncrease,
            decimal? profitIncrease,
            decimal? marginChangePp)
        {
            var parts = new List<string>();

            if (totalLiberated is > 0)
                parts.Add($"reducción observada de capital (liberación aparente) de {Money(totalLiberated.Value)}");
            else
            {
                if (liberatedImmobilized is > 0)
                    parts.Add($"reducción observada de capital inmovilizado de {Money(liberatedImmobilized.Value)}");
                if (liberatedAtRisk is > 0)
                    parts.Add($"reducción observada de capital en riesgo de {Money(liberatedAtRisk.Value)}");
                if (liberatedInventory is > 0)
                    parts.Add($"reducción observada de capital en inventario de {Money(liberatedInventory.Value)}");
            }

            if (revenueIncrease is > 0)
                parts.Add($"incremento observado de ingresos de {Money(revenueIncrease.Value)}");
            if (profitIncrease is > 0)
                parts.Add($"incremento observado de ganancia de {Money(profitIncrease.Value)}");
            if (marginChangePp.HasValue && marginChangePp.Value != 0m)
            {
                string sign = marginChangePp.Value > 0 ? "+" : "";
                parts.Add($"cambio observado de margen de {sign}{marginChangePp.Value:0.##} pp");
            }

            if (parts.Count == 0)
            {
                return BusinessActionSoftLanguageGuard.EnsureObserved(
                    "No se observó liberación de capital ni incrementos monetarios comparables en el período.");
            }

            return BusinessActionSoftLanguageGuard.EnsureObserved(
                "Durante el período posterior se observó: "
                + string.Join("; ", parts)
                + ". No se atribuye causalidad automática a la acción.");
        }

        private static BusinessActionMetricDelta? Find(
            IReadOnlyList<BusinessActionMetricDelta> deltas,
            string key)
            => deltas.FirstOrDefault(d =>
                string.Equals(d.MetricKey, key, StringComparison.OrdinalIgnoreCase));

        private static string Money(decimal amount)
            => amount.ToString("C2", CultureInfo.GetCultureInfo("es-DO"));
    }
}
