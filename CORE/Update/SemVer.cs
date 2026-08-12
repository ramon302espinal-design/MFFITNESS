using System.Globalization;
using System.Text.RegularExpressions;

namespace CORE.Update
{
    /// <summary>
    /// SemVer estricto MAJOR.MINOR.PATCH (sin pre-release/build) para contratos de update.
    /// </summary>
    public static class SemVer
    {
        private static readonly Regex Pattern = new(
            @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool TryParse(string? text, out Version version)
        {
            version = new Version(0, 0, 0);
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var m = Pattern.Match(text.Trim());
            if (!m.Success)
                return false;

            int major = int.Parse(m.Groups["major"].Value, CultureInfo.InvariantCulture);
            int minor = int.Parse(m.Groups["minor"].Value, CultureInfo.InvariantCulture);
            int patch = int.Parse(m.Groups["patch"].Value, CultureInfo.InvariantCulture);
            version = new Version(major, minor, patch);
            return true;
        }

        public static int Compare(string left, string right)
        {
            if (!TryParse(left, out Version a) || !TryParse(right, out Version b))
                throw new ArgumentException("Versión SemVer inválida para comparación.");
            return a.CompareTo(b);
        }
    }
}
