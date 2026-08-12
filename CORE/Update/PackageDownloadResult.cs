namespace CORE.Update
{
    public sealed class PackageDownloadResult
    {
        public PackageDownloadStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? LocalFilePath { get; init; }
        public long? FileSizeBytes { get; init; }
        public string? ComputedSha256 { get; init; }
        public string? ExpectedSha256 { get; init; }
        public string? AppVersion { get; init; }
        public int? TargetDbVersion { get; init; }
        public int? HttpStatusCode { get; init; }

        public static PackageDownloadResult Fail(
            PackageDownloadStatus status,
            string message,
            int? httpStatus = null,
            string? expectedSha256 = null,
            string? computedSha256 = null) =>
            new()
            {
                Status = status,
                Message = message,
                HttpStatusCode = httpStatus,
                ExpectedSha256 = expectedSha256,
                ComputedSha256 = computedSha256
            };

        public static PackageDownloadResult Verified(
            string localFilePath,
            long fileSizeBytes,
            string computedSha256,
            UpdateManifest manifest) =>
            new()
            {
                Status = PackageDownloadStatus.SuccessVerified,
                Message = "Paquete descargado y verificado (SHA256).",
                LocalFilePath = localFilePath,
                FileSizeBytes = fileSizeBytes,
                ComputedSha256 = computedSha256,
                ExpectedSha256 = manifest.PackageSha256,
                AppVersion = manifest.AppVersion,
                TargetDbVersion = manifest.TargetDbVersion
            };
    }
}
