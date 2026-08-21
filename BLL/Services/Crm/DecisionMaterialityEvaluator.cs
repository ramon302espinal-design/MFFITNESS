using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de materialidad (FASE 10.7).</summary>
    public static class DecisionMaterialityPolicy
    {
        public const string Definition =
            "FASE 10.7: Materialidad filtra ruido antes de emitir DecisionEvent. " +
            "Evita fatiga de alertas. NO inventa umbrales — reutiliza SSOT FASE 7/9.";

        public const string Rule =
            "NotMaterial ⇒ ShouldEmitAlert=false. " +
            "InsufficientData ⇒ no alerta avanzada (TEST 7). " +
            "CrossSignal / Stockout con demanda ⇒ material aunque capital sea bajo.";

        public const string Sources =
            "Variación: SalesVariationThresholds (Flat 2% / Strong 15%). " +
            "Capital: InventoryHealthThresholds (MinMaterial 1,000 / Critical 10,000) + noise 500. " +
            "Share: InventoryAlertService (25% / 40%).";

        public const string Deferred =
            "FASE 10 completa.";
    }

    /// <summary>
    /// Umbrales de materialidad. Defaults = copia de SSOT existente (no nuevos números mágicos).
    /// </summary>
    public sealed class DecisionMaterialityThresholds
    {
        public static DecisionMaterialityThresholds Default { get; } = FromSsot();

        /// <summary>SalesVariationThresholds.FlatBandPct</summary>
        public decimal FlatVariationBandPct { get; init; } = 2m;

        /// <summary>SalesVariationThresholds.StrongBandPct</summary>
        public decimal StrongVariationBandPct { get; init; } = 15m;

        /// <summary>InventoryHealthThresholds.MinMaterialCapital</summary>
        public decimal MinMaterialCapital { get; init; } = 1_000m;

        /// <summary>InventoryHealthThresholds.CriticalCapitalMin</summary>
        public decimal CriticalCapitalMin { get; init; } = 10_000m;

        /// <summary>InventoryAlertService ResolvePriorityByCapital noise floor</summary>
        public decimal NoiseFloorCapital { get; init; } = 500m;

        /// <summary>InventoryAlertService.HighImmobilizedShareThresholdPct</summary>
        public decimal HighImmobilizedSharePct { get; init; } = 25m;

        /// <summary>InventoryAlertService Critical share band</summary>
        public decimal CriticalImmobilizedSharePct { get; init; } = 40m;

        public static DecisionMaterialityThresholds FromSsot()
        {
            SalesVariationThresholds v = SalesVariationThresholds.Default;
            InventoryHealthThresholds h = InventoryHealthThresholds.Default;
            return new DecisionMaterialityThresholds
            {
                FlatVariationBandPct = v.FlatBandPct,
                StrongVariationBandPct = v.StrongBandPct,
                MinMaterialCapital = h.MinMaterialCapital,
                CriticalCapitalMin = h.CriticalCapitalMin,
                NoiseFloorCapital = 500m,
                HighImmobilizedSharePct = InventoryAlertService.HighImmobilizedShareThresholdPct,
                CriticalImmobilizedSharePct = 40m
            };
        }
    }

    /// <summary>Evalúa si un cambio/condición es material para alertar.</summary>
    public static class DecisionMaterialityEvaluator
    {
        public static DecisionMaterialityResult Evaluate(
            DecisionMaterialityInput input,
            DecisionMaterialityThresholds? thresholds = null)
        {
            DecisionMaterialityThresholds t = thresholds ?? DecisionMaterialityThresholds.Default;

            if (input.InsufficientData)
            {
                return Result(
                    false, false, DecisionMaterialityKind.NotMaterial,
                    DecisionImpactLevel.None,
                    "Datos insuficientes — no alerta avanzada (TEST 7/13).");
            }

            if (input.TimeSensitiveStockout)
            {
                return Result(
                    true, true, DecisionMaterialityKind.Strong,
                    DecisionImpactLevel.Critical,
                    "Quiebre con demanda — material sin silenciar por capital bajo.");
            }

            if (input.CrossSignal)
            {
                return Result(
                    true, true, DecisionMaterialityKind.Material,
                    DecisionImpactLevel.High,
                    "Señal cruzada — material aunque variaciones individuales sean mild.");
            }

            if (input.OpportunitySignal)
            {
                // Oportunidad requiere evidencia fuerte de variación si se aporta
                if (input.VariationPct.HasValue
                    && Math.Abs(input.VariationPct.Value) < t.StrongVariationBandPct)
                {
                    return Result(
                        false, false, DecisionMaterialityKind.NotMaterial,
                        DecisionImpactLevel.Low,
                        "Oportunidad sin variación fuerte — no alertar (anti-ruido).");
                }

                return Result(
                    true, true, DecisionMaterialityKind.Strong,
                    DecisionImpactLevel.Medium,
                    "Oportunidad con variación fuerte — material.");
            }

            DecisionMaterialityKind best = DecisionMaterialityKind.NotMaterial;
            DecisionImpactLevel impact = DecisionImpactLevel.None;
            var reasons = new List<string>();

            if (input.VariationPct.HasValue)
            {
                decimal abs = Math.Abs(input.VariationPct.Value);
                if (abs <= t.FlatVariationBandPct)
                {
                    reasons.Add($"Variación |{input.VariationPct:N2}%| ≤ FlatBand {t.FlatVariationBandPct}% (ruido).");
                }
                else if (abs >= t.StrongVariationBandPct)
                {
                    best = Max(best, DecisionMaterialityKind.Strong);
                    impact = MaxImpact(impact, DecisionImpactLevel.High);
                    reasons.Add($"Variación |{input.VariationPct:N2}%| ≥ StrongBand {t.StrongVariationBandPct}%.");
                }
                else
                {
                    best = Max(best, DecisionMaterialityKind.Material);
                    impact = MaxImpact(impact, DecisionImpactLevel.Medium);
                    reasons.Add($"Variación |{input.VariationPct:N2}%| entre Flat y Strong.");
                }
            }

            if (input.CapitalAmount.HasValue)
            {
                decimal c = input.CapitalAmount.Value;
                if (c < t.NoiseFloorCapital)
                {
                    reasons.Add($"Capital {c:N2} < noise floor {t.NoiseFloorCapital:N0}.");
                }
                else if (c < t.MinMaterialCapital)
                {
                    reasons.Add($"Capital {c:N2} < MinMaterialCapital {t.MinMaterialCapital:N0} (FASE 7.8).");
                }
                else if (c >= t.CriticalCapitalMin)
                {
                    best = Max(best, DecisionMaterialityKind.Strong);
                    impact = MaxImpact(impact, DecisionImpactLevel.Critical);
                    reasons.Add($"Capital {c:N2} ≥ CriticalCapitalMin {t.CriticalCapitalMin:N0}.");
                }
                else
                {
                    best = Max(best, DecisionMaterialityKind.Material);
                    impact = MaxImpact(impact, DecisionImpactLevel.High);
                    reasons.Add($"Capital {c:N2} ≥ MinMaterialCapital {t.MinMaterialCapital:N0}.");
                }
            }

            if (input.ImmobilizedSharePct.HasValue)
            {
                decimal s = input.ImmobilizedSharePct.Value;
                if (s < t.HighImmobilizedSharePct)
                {
                    reasons.Add($"FrozenShare {s:N2}% < {t.HighImmobilizedSharePct}%.");
                }
                else if (s >= t.CriticalImmobilizedSharePct)
                {
                    best = Max(best, DecisionMaterialityKind.Strong);
                    impact = MaxImpact(impact, DecisionImpactLevel.Critical);
                    reasons.Add($"FrozenShare {s:N2}% ≥ {t.CriticalImmobilizedSharePct}%.");
                }
                else
                {
                    best = Max(best, DecisionMaterialityKind.Material);
                    impact = MaxImpact(impact, DecisionImpactLevel.High);
                    reasons.Add($"FrozenShare {s:N2}% ≥ {t.HighImmobilizedSharePct}%.");
                }
            }

            bool material = best != DecisionMaterialityKind.NotMaterial;
            if (!material && reasons.Count == 0)
            {
                reasons.Add("Sin métricas de materialidad aportadas.");
            }

            return Result(
                material, material, best, impact,
                string.Join(" ", reasons));
        }

        /// <summary>Gate para el motor: false ⇒ no emitir / no fatigar.</summary>
        public static bool ShouldEmit(DecisionMaterialityInput input, DecisionMaterialityThresholds? thresholds = null)
            => Evaluate(input, thresholds).ShouldEmitAlert;

        /// <summary>
        /// Si no es material, marca el evento como InsufficientData solo cuando aplica;
        /// en ruido numérico el caller debe descartar (ShouldEmit=false).
        /// </summary>
        public static DecisionEvent? GateEmit(DecisionEvent draft, DecisionMaterialityResult materiality)
        {
            if (!materiality.ShouldEmitAlert)
                return null;
            return draft;
        }

        private static DecisionMaterialityResult Result(
            bool isMaterial, bool emit, DecisionMaterialityKind kind,
            DecisionImpactLevel impact, string reason)
            => new()
            {
                IsMaterial = isMaterial,
                ShouldEmitAlert = emit,
                Kind = kind,
                SuggestedImpact = impact,
                Reason = reason
            };

        private static DecisionMaterialityKind Max(DecisionMaterialityKind a, DecisionMaterialityKind b)
            => (DecisionMaterialityKind)Math.Max((int)a, (int)b);

        private static DecisionImpactLevel MaxImpact(DecisionImpactLevel a, DecisionImpactLevel b)
            => (DecisionImpactLevel)Math.Max((int)a, (int)b);
    }
}
