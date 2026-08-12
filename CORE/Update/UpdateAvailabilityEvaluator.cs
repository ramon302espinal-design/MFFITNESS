namespace CORE.Update
{
    /// <summary>
    /// Evalúa disponibilidad de actualización (solo lectura). No ejecuta backup ni migraciones.
    /// </summary>
    public static class UpdateAvailabilityEvaluator
    {
        public static UpdateAvailability Evaluate(
            UpdateManifest? manifest,
            string currentAppVersion,
            int currentDbVersion)
        {
            var validation = UpdateManifestValidator.Validate(manifest);
            if (!validation.IsValid)
            {
                return UpdateAvailability.Invalid(
                    "Manifest inválido: " + string.Join(" ", validation.Errors),
                    currentAppVersion,
                    currentDbVersion);
            }

            // manifest no es null si IsValid
            var m = manifest!;

            if (!SemVer.TryParse(currentAppVersion, out _))
            {
                return UpdateAvailability.Invalid(
                    "CurrentAppVersion no es SemVer válida.",
                    currentAppVersion,
                    currentDbVersion);
            }

            if (currentDbVersion < 1)
            {
                return UpdateAvailability.Invalid(
                    "CurrentDbVersion inválido.",
                    currentAppVersion,
                    currentDbVersion);
            }

            int cmpToTarget = SemVer.Compare(currentAppVersion, m.AppVersion);
            if (cmpToTarget >= 0)
            {
                return UpdateAvailability.Create(
                    UpdateAvailabilityStatus.NotAvailable,
                    "Ya está actualizado.",
                    currentAppVersion,
                    currentDbVersion,
                    m);
            }

            if (SemVer.Compare(currentAppVersion, m.MinAppVersion) < 0)
            {
                return UpdateAvailability.Create(
                    UpdateAvailabilityStatus.Incompatible,
                    "Aplicación demasiado antigua.",
                    currentAppVersion,
                    currentDbVersion,
                    m);
            }

            if (m.TargetDbVersion < 1)
            {
                return UpdateAvailability.Create(
                    UpdateAvailabilityStatus.InvalidManifest,
                    "Target DB inválido.",
                    currentAppVersion,
                    currentDbVersion,
                    m);
            }

            return UpdateAvailability.Create(
                UpdateAvailabilityStatus.Available,
                "Nueva versión disponible.",
                currentAppVersion,
                currentDbVersion,
                m);
        }
    }
}
