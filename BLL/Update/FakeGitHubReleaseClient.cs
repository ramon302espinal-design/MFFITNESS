using CORE.Update;

namespace BLL.Update
{
    /// <summary>
    /// Cliente falso para pruebas sin Internet ni releases reales.
    /// </summary>
    public sealed class FakeGitHubReleaseClient : IGitHubReleaseClient
    {
        private readonly Func<CancellationToken, Task<GitHubReleaseResult>> _latest;
        private readonly Func<string, CancellationToken, Task<GitHubReleaseResult>>? _byTag;

        public FakeGitHubReleaseClient(
            Func<CancellationToken, Task<GitHubReleaseResult>> latest,
            Func<string, CancellationToken, Task<GitHubReleaseResult>>? byTag = null)
        {
            _latest = latest ?? throw new ArgumentNullException(nameof(latest));
            _byTag = byTag;
        }

        public static FakeGitHubReleaseClient WithResult(GitHubReleaseResult result) =>
            new(_ => Task.FromResult(result));

        public Task<GitHubReleaseResult> GetLatestStableReleaseManifestAsync(CancellationToken cancellationToken = default) =>
            _latest(cancellationToken);

        public Task<GitHubReleaseResult> GetReleaseManifestByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            if (_byTag != null)
                return _byTag(tag, cancellationToken);

            return _latest(cancellationToken);
        }
    }
}
