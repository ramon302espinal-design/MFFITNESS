using System.Text.RegularExpressions;

namespace CORE.Update
{
    public sealed class UpdateManifestValidationResult
    {
        public bool IsValid { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static UpdateManifestValidationResult Ok() =>
            new() { IsValid = true };

        public static UpdateManifestValidationResult Fail(params string[] errors) =>
            new() { IsValid = false, Errors = errors };
    }

    /// <summary>
    /// Valida el contrato de release. Independiente de red, BD y WinForms.
    /// </summary>
    public static class UpdateManifestValidator
    {
        private static readonly Regex Sha256Hex = new(
            @"^[0-9a-fA-F]{64}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static UpdateManifestValidationResult Validate(UpdateManifest? manifest)
        {
            if (manifest == null)
                return UpdateManifestValidationResult.Fail("Manifest nulo.");

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(manifest.AppVersion))
                errors.Add("AppVersion vacío.");
            else if (!SemVer.TryParse(manifest.AppVersion, out _))
                errors.Add("AppVersion no es SemVer válida (MAJOR.MINOR.PATCH).");

            if (manifest.TargetDbVersion < 1)
                errors.Add("TargetDbVersion debe ser >= 1.");

            if (string.IsNullOrWhiteSpace(manifest.MinAppVersion))
                errors.Add("MinAppVersion vacío.");
            else if (!SemVer.TryParse(manifest.MinAppVersion, out _))
                errors.Add("MinAppVersion no es SemVer válida (MAJOR.MINOR.PATCH).");

            if (string.IsNullOrWhiteSpace(manifest.PackageName))
                errors.Add("PackageName vacío.");

            if (string.IsNullOrWhiteSpace(manifest.PackageSha256))
                errors.Add("PackageSha256 vacío.");
            else if (!Sha256Hex.IsMatch(manifest.PackageSha256.Trim()))
                errors.Add("PackageSha256 debe ser exactamente 64 caracteres hexadecimales.");

            if (manifest.ReleaseDate == default ||
                manifest.ReleaseDate.Year < 2000 ||
                manifest.ReleaseDate.Year > 2100)
            {
                errors.Add("ReleaseDate inválida.");
            }

            if (SemVer.TryParse(manifest.AppVersion, out _) &&
                SemVer.TryParse(manifest.MinAppVersion, out _) &&
                SemVer.Compare(manifest.MinAppVersion, manifest.AppVersion) > 0)
            {
                errors.Add("MinAppVersion no puede ser mayor que AppVersion.");
            }

            if (!string.IsNullOrWhiteSpace(manifest.ReleaseNotesUrl))
            {
                string url = manifest.ReleaseNotesUrl.Trim();
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    errors.Add("ReleaseNotesUrl inválida (se espera http/https absoluto).");
                }
            }

            return errors.Count == 0
                ? UpdateManifestValidationResult.Ok()
                : UpdateManifestValidationResult.Fail(errors.ToArray());
        }
    }
}
