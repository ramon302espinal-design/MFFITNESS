namespace CORE.Update
{
    public enum GitHubReleaseStatus
    {
        Success,
        NotFound,
        InvalidManifest,
        NetworkError,
        HttpError,
        ParseError,
        NoManifest,
        DuplicateManifest,
        DraftOrPrerelease
    }

    public sealed class GitHubReleaseResult
    {
        public GitHubReleaseStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public UpdateManifest? Manifest { get; init; }
        public string? TagName { get; init; }
        public string? ReleaseName { get; init; }
        public string? ReleaseHtmlUrl { get; init; }
        public DateTimeOffset? PublishedAt { get; init; }
        public int? HttpStatusCode { get; init; }
        /// <summary>URL HTTPS del asset que coincide con manifest.PackageName (si existe).</summary>
        public string? PackageDownloadUrl { get; init; }

        public static GitHubReleaseResult Ok(
            UpdateManifest manifest,
            string? tag,
            string? name,
            string? htmlUrl,
            DateTimeOffset? publishedAt,
            string? packageDownloadUrl = null) =>
            new()
            {
                Status = GitHubReleaseStatus.Success,
                Message = "Release obtenida.",
                Manifest = manifest,
                TagName = tag,
                ReleaseName = name,
                ReleaseHtmlUrl = htmlUrl,
                PublishedAt = publishedAt,
                PackageDownloadUrl = packageDownloadUrl
            };

        public static GitHubReleaseResult Fail(
            GitHubReleaseStatus status,
            string message,
            int? httpStatus = null,
            string? tag = null) =>
            new()
            {
                Status = status,
                Message = message,
                HttpStatusCode = httpStatus,
                TagName = tag
            };
    }
}
