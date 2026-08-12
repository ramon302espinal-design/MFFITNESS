namespace CORE.Update
{
    public enum PackageDownloadStatus
    {
        Success,
        InvalidUrl,
        NetworkError,
        HttpError,
        Timeout,
        Cancelled,
        FileError,
        HashMismatch,
        InvalidManifest,
        SuccessVerified
    }
}
