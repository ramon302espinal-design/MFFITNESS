using System.Net;
using System.Security.Cryptography;
using CORE;
using CORE.Update;

namespace BLL.Update
{
    /// <summary>
    /// Descarga segura del paquete ZIP por HTTPS, streaming a disco y verificación SHA256.
    /// No instala, no descomprime, no ejecuta migraciones ni modifica el POS.
    /// </summary>
    public sealed class UpdatePackageDownloader : IDisposable
    {
        /// <summary>
        /// Límite documentado de GitHub por asset de release (2 GiB).
        /// </summary>
        public const long MaxPackageBytes = 2L * 1024 * 1024 * 1024;

        private readonly HttpClient _http;
        private readonly bool _ownsClient;
        private readonly string _downloadDirectory;

        public UpdatePackageDownloader(
            HttpClient? httpClient = null,
            TimeSpan? timeout = null,
            string? downloadDirectory = null)
        {
            if (httpClient != null)
            {
                _http = httpClient;
                _ownsClient = false;
            }
            else
            {
                _http = new HttpClient
                {
                    Timeout = timeout ?? TimeSpan.FromMinutes(5)
                };
                _ownsClient = true;
            }

            _downloadDirectory = downloadDirectory ?? UpdateDownloadStorage.CarpetaDescargas;
            Directory.CreateDirectory(_downloadDirectory);
        }

        public async Task<PackageDownloadResult> DownloadAndVerifyAsync(
            UpdateManifest manifest,
            string downloadUrl,
            CancellationToken cancellationToken = default)
        {
            if (manifest == null)
            {
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.InvalidManifest,
                    "Manifest nulo.");
            }

            var validation = UpdateManifestValidator.Validate(manifest);
            if (!validation.IsValid)
            {
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.InvalidManifest,
                    "Manifest inválido: " + string.Join(" ", validation.Errors));
            }

            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.InvalidUrl,
                    "URL de descarga vacía.");
            }

            if (!IsHttpsUrl(downloadUrl))
            {
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.InvalidUrl,
                    "Solo se permite HTTPS para descargar el paquete.");
            }

            if (!UrlMatchesPackageName(downloadUrl, manifest.PackageName))
            {
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.InvalidUrl,
                    $"El asset de la URL no coincide con PackageName '{manifest.PackageName}'.");
            }

            string partialPath = Path.Combine(_downloadDirectory, manifest.PackageName + ".part");
            string finalPath = Path.Combine(_downloadDirectory, manifest.PackageName);

            SafeDelete(partialPath);
            SafeDelete(finalPath);

            HttpResponseMessage? response = null;
            try
            {
                response = await _http
                    .GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                int httpCode = (int)response.StatusCode;

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return PackageDownloadResult.Fail(
                        PackageDownloadStatus.HttpError,
                        "Recurso no encontrado (404).",
                        httpCode);
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return PackageDownloadResult.Fail(
                        PackageDownloadStatus.HttpError,
                        "Acceso denegado (403).",
                        httpCode);
                }

                if ((int)response.StatusCode >= 500)
                {
                    return PackageDownloadResult.Fail(
                        PackageDownloadStatus.HttpError,
                        $"Servidor respondió {httpCode}.",
                        httpCode);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return PackageDownloadResult.Fail(
                        PackageDownloadStatus.HttpError,
                        $"HTTP {httpCode}.",
                        httpCode);
                }

                long? contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue)
                {
                    if (contentLength.Value <= 0)
                    {
                        return PackageDownloadResult.Fail(
                            PackageDownloadStatus.FileError,
                            "Content-Length indica archivo vacío.");
                    }

                    if (contentLength.Value > MaxPackageBytes)
                    {
                        return PackageDownloadResult.Fail(
                            PackageDownloadStatus.FileError,
                            $"Content-Length ({contentLength.Value}) excede el límite de GitHub ({MaxPackageBytes} bytes).");
                    }
                }

                await using Stream networkStream = await response.Content
                    .ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);

                long written = await WriteStreamToFileAsync(networkStream, partialPath, cancellationToken)
                    .ConfigureAwait(false);

                response.Dispose();
                response = null;

                if (written <= 0)
                {
                    SafeDelete(partialPath);
                    return PackageDownloadResult.Fail(
                        PackageDownloadStatus.FileError,
                        "Archivo descargado vacío.");
                }

                if (written > MaxPackageBytes)
                {
                    SafeDelete(partialPath);
                    return PackageDownloadResult.Fail(
                        PackageDownloadStatus.FileError,
                        $"Archivo descargado ({written} bytes) excede el límite permitido.");
                }

                string computedHash = ComputeSha256Hex(partialPath);
                string expectedHash = manifest.PackageSha256.Trim();

                if (!string.Equals(computedHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    SafeDelete(partialPath);
                    return PackageDownloadResult.Fail(
                        PackageDownloadStatus.HashMismatch,
                        "SHA256 no coincide con PackageSha256 del manifest.",
                        expectedSha256: expectedHash,
                        computedSha256: computedHash);
                }

                if (File.Exists(finalPath))
                    File.Delete(finalPath);

                File.Move(partialPath, finalPath);

                return PackageDownloadResult.Verified(finalPath, written, computedHash, manifest);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                SafeDelete(partialPath);
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.Cancelled,
                    "Descarga cancelada.");
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                SafeDelete(partialPath);
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.Timeout,
                    "Timeout durante la descarga: " + ex.Message);
            }
            catch (HttpRequestException ex)
            {
                SafeDelete(partialPath);
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.NetworkError,
                    "Error de red: " + ex.Message);
            }
            catch (IOException ex)
            {
                SafeDelete(partialPath);
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.FileError,
                    "Error de archivo: " + ex.Message);
            }
            catch (Exception ex)
            {
                SafeDelete(partialPath);
                return PackageDownloadResult.Fail(
                    PackageDownloadStatus.NetworkError,
                    "Error inesperado: " + ex.Message);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public static bool IsHttpsUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
            uri.Scheme == Uri.UriSchemeHttps;

        public static bool UrlMatchesPackageName(string url, string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName) ||
                !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            string fileName = Path.GetFileName(uri.LocalPath);
            return string.Equals(fileName, packageName.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<long> WriteStreamToFileAsync(
            Stream source,
            string destinationPath,
            CancellationToken cancellationToken)
        {
            await using FileStream fileStream = new(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            long total = 0;
            byte[] buffer = new byte[81920];

            while (true)
            {
                int read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                    break;

                total += read;
                if (total > MaxPackageBytes)
                    throw new IOException($"Descarga excede el límite de {MaxPackageBytes} bytes.");

                await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            }

            await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return total;
        }

        private static string ComputeSha256Hex(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            byte[] hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // Best effort: el resultado ya refleja el fallo principal.
            }
        }

        public void Dispose()
        {
            if (_ownsClient)
                _http.Dispose();
        }
    }
}
