using System;
using System.IO;

namespace CORE
{
    /// <summary>
    /// Fotos de producto en disco (misma raíz que facturas).
    /// %LocalAppData%\MFFITNESS\Productos — NO se guarda BLOB en SQL.
    /// </summary>
    public static class ProductoImagenStorage
    {
        public static string CarpetaProductos
        {
            get
            {
                string dir = Path.Combine(FacturaStorage.CarpetaRaizMffitness, "Productos");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string NombreArchivoProducto(int productoId) =>
            $"producto_{Math.Max(1, productoId)}.jpg";

        public static string RutaProducto(int productoId) =>
            Path.Combine(CarpetaProductos, NombreArchivoProducto(productoId));

        /// <summary>Guarda JPEG temporal antes de crear el producto en BD.</summary>
        public static string GuardarPendiente(byte[] jpegBytes)
        {
            if (jpegBytes == null || jpegBytes.Length == 0)
                throw new ArgumentException("Imagen vacía.", nameof(jpegBytes));

            string name = $"pendiente_{Guid.NewGuid():N}.jpg";
            string path = Path.Combine(CarpetaProductos, name);
            File.WriteAllBytes(path, jpegBytes);
            return path;
        }

        /// <summary>Asocia la imagen al Id de producto (sobrescribe si existe).</summary>
        public static string GuardarParaProducto(int productoId, byte[] jpegBytes)
        {
            if (productoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productoId));
            if (jpegBytes == null || jpegBytes.Length == 0)
                throw new ArgumentException("Imagen vacía.", nameof(jpegBytes));

            string path = RutaProducto(productoId);
            File.WriteAllBytes(path, jpegBytes);
            return path;
        }

        /// <summary>
        /// Si hay pendiente, la mueve/copia a producto_{id}.jpg y borra el pendiente.
        /// </summary>
        public static string? FinalizarPendiente(int productoId, string? rutaPendiente)
        {
            if (productoId <= 0 || string.IsNullOrWhiteSpace(rutaPendiente))
                return null;

            string dest = RutaProducto(productoId);
            if (File.Exists(rutaPendiente))
            {
                File.Copy(rutaPendiente, dest, overwrite: true);
                try
                {
                    if (!string.Equals(
                            Path.GetFullPath(rutaPendiente),
                            Path.GetFullPath(dest),
                            StringComparison.OrdinalIgnoreCase))
                        File.Delete(rutaPendiente);
                }
                catch
                {
                    // best-effort
                }
            }

            return File.Exists(dest) ? dest : null;
        }

        public static string? ResolverRutaExistente(string? rutaONombre)
        {
            if (string.IsNullOrWhiteSpace(rutaONombre))
                return null;

            if (File.Exists(rutaONombre))
                return rutaONombre;

            string combinada = Path.Combine(CarpetaProductos, Path.GetFileName(rutaONombre));
            return File.Exists(combinada) ? combinada : null;
        }
    }
}
