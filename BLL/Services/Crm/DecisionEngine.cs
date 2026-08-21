using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato del motor base (FASE 10.8).</summary>
    public static class DecisionEnginePolicy
    {
        public const string Definition =
            "FASE 10.8: DecisionEngine orquesta reglas deterministas. " +
            "Candidato → Materialidad (10.7) → Evento → Severidad (10.5) → Prioridad (10.6) → Dedup fingerprint. " +
            "DETECTA / ANALIZA / RECOMIENDA — el usuario DECIDE. Nunca auto-compra ni muta stock/caja.";

        public const string DomainRules =
            "Reglas de dominio (ventas…inversiones) = FASE 10.9–10.17. " +
            "Agrupación = 10.18 · Recomendaciones = 10.19 · Centro = 10.20 · Persistencia = 10.21+.";

        public const string SoftLanguage =
            "Recommendation: Revisar/Evaluar/Considerar/Analizar — sin órdenes irreversibles.";
    }

    /// <summary>Regla determinista. Sin LLM. Sin side-effects.</summary>
    public interface IDecisionRule
    {
        string RuleId { get; }

        IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context);
    }

    /// <summary>
    /// Motor base de decisiones (FASE 10.8).
    /// No recalcula métricas financieras — consume candidatos de reglas / SSOT.
    /// </summary>
    public sealed class DecisionEngine
    {
        private readonly DecisionMaterialityThresholds _thresholds;

        public DecisionEngine(DecisionMaterialityThresholds? thresholds = null)
        {
            _thresholds = thresholds ?? DecisionMaterialityThresholds.Default;
        }

        /// <summary>Ejecuta reglas y aplica el pipeline completo.</summary>
        public DecisionEngineReport Run(
            IReadOnlyList<IDecisionRule> rules,
            DecisionRuleContext context,
            DateTime? detectedAt = null)
        {
            var candidates = new List<DecisionRuleCandidate>();
            foreach (IDecisionRule rule in rules)
            {
                foreach (DecisionRuleCandidate c in rule.Evaluate(context))
                    candidates.Add(c);
            }

            return Evaluate(candidates, detectedAt);
        }

        /// <summary>Pipeline puro sobre candidatos (tests / reglas externas).</summary>
        public DecisionEngineReport Evaluate(
            IEnumerable<DecisionRuleCandidate> candidates,
            DateTime? detectedAt = null)
        {
            DateTime at = detectedAt ?? DateTime.UtcNow;
            int considered = 0;
            int suppressedMat = 0;
            int suppressedDup = 0;

            var byFingerprint = new Dictionary<string, DecisionEvent>(StringComparer.Ordinal);

            foreach (DecisionRuleCandidate c in candidates)
            {
                considered++;

                DecisionMaterialityResult mat = DecisionMaterialityEvaluator.Evaluate(
                    c.Materiality, _thresholds);

                if (!mat.ShouldEmitAlert)
                {
                    suppressedMat++;
                    continue;
                }

                DecisionImpactAssessment impact = EnrichImpact(c.ImpactAssessment, mat, c.Materiality);

                DecisionEvent draft = DecisionEventFactory.Create(
                    eventType: c.EventType,
                    area: c.Area,
                    entityType: c.EntityType,
                    entityId: c.EntityId,
                    entityName: c.EntityName,
                    periodKey: c.PeriodKey,
                    source: string.IsNullOrWhiteSpace(c.Source) ? c.RuleId : c.Source,
                    title: c.Title,
                    description: c.Description,
                    detectedAt: at,
                    evidence: c.Evidence,
                    metricKeys: c.MetricKeys,
                    status: c.Materiality.InsufficientData
                        ? DecisionEventStatus.InsufficientData
                        : DecisionEventStatus.Active,
                    reason: c.Reason,
                    impact: string.IsNullOrWhiteSpace(c.Impact)
                        ? DecisionSeverityCatalog.DisplayName(
                            DecisionSeverityResolver.Resolve(impact))
                        : c.Impact,
                    recommendation: NormalizeRecommendation(c.Recommendation));

                DecisionEvent withSev = DecisionSeverityResolver.Apply(draft, impact);

                var priorityInput = new DecisionPriorityAssessment
                {
                    Severity = withSev.Severity,
                    Urgency = c.Urgency,
                    RequiresImmediateReview = c.RequiresImmediateReview,
                    TimeSensitiveStockout = c.TimeSensitiveStockout || c.Materiality.TimeSensitiveStockout,
                    OpportunityWindow = c.OpportunityWindow || c.Materiality.OpportunitySignal,
                    ProductStillSelling = c.ProductStillSelling || impact.ProductStillSelling,
                    InsufficientData = c.Materiality.InsufficientData || impact.InsufficientData
                };

                DecisionEvent final = DecisionPriorityResolver.Apply(withSev, priorityInput);

                if (byFingerprint.TryGetValue(final.Fingerprint, out DecisionEvent? existing))
                {
                    suppressedDup++;
                    if (IsPreferred(final, existing))
                        byFingerprint[final.Fingerprint] = final;
                }
                else
                {
                    byFingerprint[final.Fingerprint] = final;
                }
            }

            List<DecisionEvent> ordered = byFingerprint.Values
                .OrderByDescending(e => (int)e.Priority)
                .ThenByDescending(e => (int)e.Severity)
                .ThenBy(e => e.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 10.19: suavizar Recommendation de cada evento
            List<DecisionEvent> withRecs = ordered
                .Select(DecisionRecommendationComposer.ApplyToEvent)
                .ToList();

            // 10.18 + 10.19: agrupar y narrar
            IReadOnlyList<DecisionGroup> rawGroups = DecisionEventGrouper.Group(withRecs);
            List<DecisionGroup> groups = rawGroups
                .Select(DecisionRecommendationComposer.ApplyToGroup)
                .OrderByDescending(g => (int)g.Priority)
                .ThenByDescending(g => (int)g.Severity)
                .ThenByDescending(g => g.EventCount)
                .ToList();

            IReadOnlyList<DecisionRecommendation> recommendations =
                DecisionRecommendationComposer.ComposeReport(withRecs, groups);

            return new DecisionEngineReport
            {
                Events = withRecs,
                Groups = groups,
                Recommendations = recommendations,
                CandidatesConsidered = considered,
                EmittedCount = withRecs.Count,
                SuppressedByMateriality = suppressedMat,
                SuppressedByDuplicate = suppressedDup,
                PolicyNote = DecisionEnginePolicy.Definition
                    + " " + DecisionGroupPolicy.Definition
                    + " " + DecisionRecommendationPolicy.Definition
            };
        }

        private static DecisionImpactAssessment EnrichImpact(
            DecisionImpactAssessment baseImpact,
            DecisionMaterialityResult mat,
            DecisionMaterialityInput materiality)
        {
            DecisionImpactLevel suggested = mat.SuggestedImpact;
            DecisionImpactLevel financial = Max(baseImpact.Financial, suggested);
            DecisionImpactLevel sales = baseImpact.Sales;
            DecisionImpactLevel capital = baseImpact.Capital;
            DecisionImpactLevel inventory = baseImpact.Inventory;

            // Si el caller no tipó dimensiones, propagar suggested al área más plausible
            if (baseImpact.Financial == DecisionImpactLevel.None
                && baseImpact.Sales == DecisionImpactLevel.None
                && baseImpact.Capital == DecisionImpactLevel.None
                && baseImpact.Inventory == DecisionImpactLevel.None
                && baseImpact.Liquidity == DecisionImpactLevel.None
                && baseImpact.Operational == DecisionImpactLevel.None
                && suggested != DecisionImpactLevel.None)
            {
                if (materiality.CapitalAmount.HasValue || materiality.ImmobilizedSharePct.HasValue)
                    capital = suggested;
                else if (materiality.TimeSensitiveStockout)
                    inventory = suggested;
                else if (materiality.VariationPct.HasValue || materiality.CrossSignal)
                    sales = suggested;
                else
                    financial = suggested;
            }

            return new DecisionImpactAssessment
            {
                Financial = financial,
                Sales = sales,
                Inventory = inventory,
                Liquidity = baseImpact.Liquidity,
                Capital = capital,
                Operational = baseImpact.Operational,
                ProductStillSelling = baseImpact.ProductStillSelling,
                SeasonalContext = baseImpact.SeasonalContext,
                InsufficientData = baseImpact.InsufficientData || materiality.InsufficientData
            };
        }

        private static string NormalizeRecommendation(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return DecisionSoftLanguageGuard.Ensure(text.Trim());
        }

        private static bool IsPreferred(DecisionEvent candidate, DecisionEvent existing)
        {
            if ((int)candidate.Priority != (int)existing.Priority)
                return (int)candidate.Priority > (int)existing.Priority;
            return (int)candidate.Severity >= (int)existing.Severity;
        }

        private static DecisionImpactLevel Max(DecisionImpactLevel a, DecisionImpactLevel b)
            => (DecisionImpactLevel)Math.Max((int)a, (int)b);
    }

    /// <summary>Registro de reglas built-in (se completa en 10.9–10.17).</summary>
    public static class DecisionRuleRegistry
    {
        public static IReadOnlyList<IDecisionRule> BuiltIn { get; } =
        [
            new SalesAlertDecisionRule(),
            new ProfitAlertDecisionRule(),
            new RoiAlertDecisionRule(),
            new InventoryAlertDecisionRule(),
            new CapitalAlertDecisionRule(),
            new ProductAlertDecisionRule(),
            new TrendAlertDecisionRule(),
            new ForecastAlertDecisionRule(),
            new InvestmentAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> SalesOnly { get; } =
        [
            new SalesAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> ProfitOnly { get; } =
        [
            new ProfitAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> RoiOnly { get; } =
        [
            new RoiAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> InventoryOnly { get; } =
        [
            new InventoryAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> CapitalOnly { get; } =
        [
            new CapitalAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> ProductOnly { get; } =
        [
            new ProductAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> TrendOnly { get; } =
        [
            new TrendAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> ForecastOnly { get; } =
        [
            new ForecastAlertDecisionRule()
        ];

        public static IReadOnlyList<IDecisionRule> InvestmentOnly { get; } =
        [
            new InvestmentAlertDecisionRule()
        ];
    }
}
