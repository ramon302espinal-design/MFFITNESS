using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CORE.Update;

namespace BLL.Update
{
    /// <summary>
    /// Consulta GitHub Releases por HTTPS. Solo lee metadata y manifest.json (no el ZIP del POS).
    /// </summary>
    public sealed class GitHubReleaseClient : IGitHubReleaseClient, IDisposable
    {
        public const string DefaultOwner = "ramon302espinal-design";
        public const string DefaultRepo = "MFFITNESS";
        public const string ManifestAssetName = "manifest.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private readonly HttpClient _http;
        private readonly string _owner;
        private readonly string _repo;
        private readonly bool _ownsClient;

        public GitHubReleaseClient(
            string? owner = null,
            string? repo = null,
            HttpClient? httpClient = null,
            TimeSpan? timeout = null)
        {
            _owner = string.IsNullOrWhiteSpace(owner) ? DefaultOwner : owner.Trim();
            _repo = string.IsNullOrWhiteSpace(repo) ? DefaultRepo : repo.Trim();

            if (httpClient != null)
            {
                _http = httpClient;
                _ownsClient = false;
            }
            else
            {
                _http = new HttpClient
                {
                    Timeout = timeout ?? TimeSpan.FromSeconds(20)
                };
                _ownsClient = true;
            }

            if (!_http.DefaultRequestHeaders.UserAgent.Any())
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("MFFITNESS-POS/1.0 (+https://github.com/ramon302espinal-design/MFFITNESS)");
            }

            if (!_http.DefaultRequestHeaders.Accept.Any())
            {
                _http.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            }
        }

        public async Task<GitHubReleaseResult> GetLatestStableReleaseManifestAsync(CancellationToken cancellationToken = default)
        {
            // Listado: /latest puede apuntar a prerelease; filtramos estables explícitamente.
            string url = $"https://api.github.com/repos/{_owner}/{_repo}/releases?per_page=30";
            var (status, body, httpCode, error) = await SendGetAsync(url, cancellationToken).ConfigureAwait(false);
            if (status != null)
                return status;

            List<GhReleaseDto>? releases;
            try
            {
                releases = JsonSerializer.Deserialize<List<GhReleaseDto>>(body!, JsonOptions);
            }
            catch (JsonException ex)
            {
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.ParseError, "JSON de releases inválido: " + ex.Message, httpCode);
            }

            if (releases == null || releases.Count == 0)
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.NotFound, "No hay releases publicadas.", httpCode);

            GhReleaseDto? stable = releases.FirstOrDefault(r => !r.Draft && !r.Prerelease);
            if (stable == null)
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.NotFound, "No hay releases estables (solo draft/prerelease).", httpCode);

            return await LoadManifestFromReleaseAsync(stable, cancellationToken).ConfigureAwait(false);
        }

        public async Task<GitHubReleaseResult> GetReleaseManifestByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.NotFound, "Tag vacío.");

            string encoded = Uri.EscapeDataString(tag.Trim());
            string url = $"https://api.github.com/repos/{_owner}/{_repo}/releases/tags/{encoded}";
            var (status, body, httpCode, _) = await SendGetAsync(url, cancellationToken).ConfigureAwait(false);
            if (status != null)
                return status;

            GhReleaseDto? release;
            try
            {
                release = JsonSerializer.Deserialize<GhReleaseDto>(body!, JsonOptions);
            }
            catch (JsonException ex)
            {
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.ParseError, "JSON de release inválido: " + ex.Message, httpCode);
            }

            if (release == null)
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.ParseError, "Release vacía.", httpCode);

            if (release.Draft)
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.DraftOrPrerelease, "La release es draft y se ignora.", httpCode, release.TagName);

            if (release.Prerelease)
                return GitHubReleaseResult.Fail(GitHubReleaseStatus.DraftOrPrerelease, "La release es prerelease y se ignora.", httpCode, release.TagName);

            return await LoadManifestFromReleaseAsync(release, cancellationToken).ConfigureAwait(false);
        }

        private async Task<GitHubReleaseResult> LoadManifestFromReleaseAsync(GhReleaseDto release, CancellationToken ct)
        {
            var assets = release.Assets ?? new List<GhAssetDto>();
            var manifests = assets
                .Where(a => string.Equals(a.Name, ManifestAssetName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (manifests.Count == 0)
            {
                return GitHubReleaseResult.Fail(
                    GitHubReleaseStatus.NoManifest,
                    "Release sin manifest.json.",
                    tag: release.TagName);
            }

            if (manifests.Count > 1)
            {
                return GitHubReleaseResult.Fail(
                    GitHubReleaseStatus.DuplicateManifest,
                    "Release con manifest.json duplicado.",
                    tag: release.TagName);
            }

            string? downloadUrl = manifests[0].BrowserDownloadUrl;
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                return GitHubReleaseResult.Fail(
                    GitHubReleaseStatus.NoManifest,
                    "manifest.json sin browser_download_url.",
                    tag: release.TagName);
            }

            if (!IsHttpsUrl(downloadUrl))
            {
                return GitHubReleaseResult.Fail(
                    GitHubReleaseStatus.HttpError,
                    "URL de manifest no es HTTPS.",
                    tag: release.TagName);
            }

            var (err, json, httpCode, _) = await SendGetAsync(downloadUrl, ct).ConfigureAwait(false);
            if (err != null)
                return err;

            UpdateManifest? manifest;
            try
            {
                manifest = JsonSerializer.Deserialize<UpdateManifest>(json!, JsonOptions);
            }
            catch (JsonException ex)
            {
                return GitHubReleaseResult.Fail(
                    GitHubReleaseStatus.ParseError,
                    "manifest.json inválido: " + ex.Message,
                    httpCode,
                    release.TagName);
            }

            if (manifest == null)
            {
                return GitHubReleaseResult.Fail(
                    GitHubReleaseStatus.ParseError,
                    "manifest.json vacío.",
                    httpCode,
                    release.TagName);
            }

            manifest = GitHubVersionNormalizer.NormalizeManifestVersions(manifest);

            if (string.IsNullOrWhiteSpace(manifest.ReleaseNotesUrl) && !string.IsNullOrWhiteSpace(release.HtmlUrl))
            {
                manifest = manifest with { ReleaseNotesUrl = release.HtmlUrl };
            }

            if (manifest.ReleaseDate == default && release.PublishedAt.HasValue)
            {
                manifest = manifest with { ReleaseDate = release.PublishedAt.Value.UtcDateTime.Date };
            }

            string? packageUrl = ResolvePackageDownloadUrl(release, manifest.PackageName);

            return GitHubReleaseResult.Ok(
                manifest,
                release.TagName,
                release.Name,
                release.HtmlUrl,
                release.PublishedAt,
                packageUrl);
        }

        /// <summary>
        /// Resuelve browser_download_url del asset cuyo nombre coincide con packageName (case-insensitive).
        /// </summary>
        private static string? ResolvePackageDownloadUrl(GhReleaseDto release, string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return null;

            var assets = release.Assets ?? new List<GhAssetDto>();
            var matches = assets
                .Where(a => string.Equals(a.Name, packageName.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count != 1)
                return null;

            string? url = matches[0].BrowserDownloadUrl;
            return !string.IsNullOrWhiteSpace(url) && IsHttpsUrl(url) ? url : null;
        }

        private async Task<(GitHubReleaseResult? Error, string? Body, int? HttpCode, string? Detail)> SendGetAsync(
            string url,
            CancellationToken ct)
        {
            if (!IsHttpsUrl(url))
                return (GitHubReleaseResult.Fail(GitHubReleaseStatus.HttpError, "Solo se permite HTTPS."), null, null, null);

            try
            {
                using var response = await _http.GetAsync(url, ct).ConfigureAwait(false);
                int code = (int)response.StatusCode;
                string body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return (GitHubReleaseResult.Fail(GitHubReleaseStatus.NotFound, "Recurso no encontrado (404).", code), null, code, null);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    return (GitHubReleaseResult.Fail(GitHubReleaseStatus.HttpError, "GitHub respondió 403 (rate limit o acceso denegado).", code), null, code, null);

                if ((int)response.StatusCode >= 500)
                    return (GitHubReleaseResult.Fail(GitHubReleaseStatus.HttpError, $"GitHub respondió {code}.", code), null, code, null);

                if (!response.IsSuccessStatusCode)
                    return (GitHubReleaseResult.Fail(GitHubReleaseStatus.HttpError, $"HTTP {code}.", code), null, code, null);

                return (null, body, code, null);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                return (GitHubReleaseResult.Fail(GitHubReleaseStatus.NetworkError, "Timeout al consultar GitHub: " + ex.Message), null, null, null);
            }
            catch (HttpRequestException ex)
            {
                return (GitHubReleaseResult.Fail(GitHubReleaseStatus.NetworkError, "Sin conexión o error de red: " + ex.Message), null, null, null);
            }
            catch (Exception ex)
            {
                return (GitHubReleaseResult.Fail(GitHubReleaseStatus.NetworkError, "Error de red: " + ex.Message), null, null, null);
            }
        }

        private static bool IsHttpsUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
            uri.Scheme == Uri.UriSchemeHttps;

        public void Dispose()
        {
            if (_ownsClient)
                _http.Dispose();
        }

        private sealed class GhReleaseDto
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }

            [JsonPropertyName("draft")]
            public bool Draft { get; set; }

            [JsonPropertyName("prerelease")]
            public bool Prerelease { get; set; }

            [JsonPropertyName("published_at")]
            public DateTimeOffset? PublishedAt { get; set; }

            [JsonPropertyName("assets")]
            public List<GhAssetDto>? Assets { get; set; }
        }

        private sealed class GhAssetDto
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("browser_download_url")]
            public string? BrowserDownloadUrl { get; set; }
        }
    }
}
