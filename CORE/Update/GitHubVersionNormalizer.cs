namespace CORE.Update
{
    /// <summary>
    /// Normaliza tags de GitHub (v1.1.0) a SemVer (1.1.0).
    /// </summary>
    public static class GitHubVersionNormalizer
    {
        public static string StripVersionPrefix(string? tagOrVersion)
        {
            if (string.IsNullOrWhiteSpace(tagOrVersion))
                return string.Empty;

            string t = tagOrVersion.Trim();
            if (t.StartsWith("v", StringComparison.OrdinalIgnoreCase) && t.Length > 1 && char.IsDigit(t[1]))
                return t[1..];

            return t;
        }

        /// <summary>
        /// Si AppVersion viene con prefijo v, lo normaliza. No inventa versión desde el tag.
        /// </summary>
        public static UpdateManifest NormalizeManifestVersions(UpdateManifest manifest)
        {
            string app = StripVersionPrefix(manifest.AppVersion);
            string min = StripVersionPrefix(manifest.MinAppVersion);
            if (app == manifest.AppVersion && min == manifest.MinAppVersion)
                return manifest;

            return manifest with
            {
                AppVersion = app,
                MinAppVersion = min
            };
        }
    }
}
