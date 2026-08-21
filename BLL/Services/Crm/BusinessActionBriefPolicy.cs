namespace BLL.Services.Crm
{
    /// <summary>Batería del brief FASE 11 §80 (TEST 1–12) — FASE 11.22.</summary>
    public static class BusinessActionBriefPolicy
    {
        public const string Definition =
            "FASE 11.22: batería brief §80 TEST 1–12. " +
            "Decisión→acción→resultado→variación→outcome→historial→recurrencia→histórico≠garantía. " +
            "Sin mutar POS. Sin inventar impacto.";

        public const string SoftLanguage =
            "Histórico ≠ garantía futura (brief §86). Soft language guard = BusinessActionSoftLanguageGuard.";

        public const string Deferred =
            "FASE 11 completa.";
    }
}
