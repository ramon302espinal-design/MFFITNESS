using System.Security.Cryptography;
using CORE;
using CORE.Update;

namespace BLL.Update
{
    public static class UpdatePackageVerifier
    {
        public sealed class VerifyResult
        {
            public bool Success { get; init; }
            public string Message { get; init; } = string.Empty;
            public string? ComputedSha256 { get; init; }
        }

        public static VerifyResult VerifyPackage(UpdateInstallRequest request)
        {
            if (request.Manifest == null)
                return new VerifyResult { Success = false, Message = "Manifest nulo." };

            var manifestValidation = UpdateManifestValidator.Validate(request.Manifest);
            if (!manifestValidation.IsValid)
            {
                return new VerifyResult
                {
                    Success = false,
                    Message = "Manifest inválido: " + string.Join(" ", manifestValidation.Errors)
                };
            }

            if (!request.PackageVerified)
            {
                return new VerifyResult
                {
                    Success = false,
                    Message = "Paquete no verificado por FASE 8 (PackageVerified=false)."
                };
            }

            if (string.IsNullOrWhiteSpace(request.PackagePath) || !File.Exists(request.PackagePath))
                return new VerifyResult { Success = false, Message = "Paquete no encontrado." };

            string fileName = Path.GetFileName(request.PackagePath);
            if (!string.Equals(fileName, request.Manifest.PackageName, StringComparison.OrdinalIgnoreCase))
            {
                return new VerifyResult
                {
                    Success = false,
                    Message = $"PackageName incorrecto. Esperado '{request.Manifest.PackageName}', actual '{fileName}'."
                };
            }

            string computed = ComputeSha256Hex(request.PackagePath);
            string expected = request.ExpectedSha256.Trim();

            if (!string.Equals(computed, expected, StringComparison.OrdinalIgnoreCase))
            {
                return new VerifyResult
                {
                    Success = false,
                    Message = "HashMismatch: SHA256 no coincide con manifest.",
                    ComputedSha256 = computed
                };
            }

            if (!string.Equals(computed, request.Manifest.PackageSha256.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new VerifyResult
                {
                    Success = false,
                    Message = "HashMismatch: SHA256 no coincide con PackageSha256.",
                    ComputedSha256 = computed
                };
            }

            return new VerifyResult
            {
                Success = true,
                Message = "Paquete verificado.",
                ComputedSha256 = computed
            };
        }

        private static string ComputeSha256Hex(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
    }
}
