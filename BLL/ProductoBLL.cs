using DL;
using System;
using System.Data;

namespace BLL
{
    public class ProductoBLL
    {
        private readonly ProductoDAL dal = new ProductoDAL();

        public bool ExisteNombre(string nombre)
        {
            return dal.ExisteNombre(nombre);
        }

        public int AgregarProductoConId(string nombre, int categoriaId, decimal compra, decimal venta, int stockInicial, int stockMinimo, bool activo, string? codigoBarra = null, string? rutaImagen = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception("El nombre es obligatorio.");

            if (compra <= 0)
                throw new Exception("El precio de compra debe ser mayor a 0.");

            if (venta <= 0)
                throw new Exception("El precio de venta debe ser mayor a 0.");

            if (stockInicial < 0)
                throw new Exception("El stock inicial no puede ser negativo.");

            if (stockMinimo < 0)
                throw new Exception("El stock mínimo no puede ser negativo.");

            string? codigo = null;
            if (!string.IsNullOrWhiteSpace(codigoBarra))
            {
                if (!ProductoBarcodeNormalizer.TryNormalizeBarcode(codigoBarra, out codigo))
                {
                    throw new Exception(
                        "Código de barras inválido. Use EAN numérico o código interno (no QR ni URL).");
                }
            }

            ValidarCodigoBarraUnico(codigo, null);

            return dal.Agregar(nombre, categoriaId, compra, venta, stockInicial, stockMinimo, activo, codigo, rutaImagen);
        }

        public void EditarProducto(int id, string nombre, int categoriaId, decimal compra, decimal venta, int stockMinimo, bool activo, string? codigoBarra = null, string? rutaImagen = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception("El nombre es obligatorio.");

            if (compra <= 0)
                throw new Exception("El precio de compra debe ser mayor a 0.");

            if (venta <= 0)
                throw new Exception("El precio de venta debe ser mayor a 0.");

            if (stockMinimo < 0)
                throw new Exception("El stock mínimo no puede ser negativo.");

            string? codigo = null;
            if (!string.IsNullOrWhiteSpace(codigoBarra))
            {
                if (!ProductoBarcodeNormalizer.TryNormalizeBarcode(codigoBarra, out codigo))
                {
                    throw new Exception(
                        "Código de barras inválido. Use EAN numérico o código interno (no QR ni URL).");
                }
            }

            ValidarCodigoBarraUnico(codigo, id);

            dal.Editar(id, nombre, categoriaId, compra, venta, stockMinimo, activo, codigo, rutaImagen);
        }

        public void ActualizarRutaImagen(int id, string? rutaImagen)
        {
            if (id <= 0)
                throw new Exception("Producto inválido.");
            dal.ActualizarRutaImagen(id, rutaImagen);
        }

        public DataRow? BuscarPorCodigoBarra(string? codigoBarra)
        {
            if (!ProductoBarcodeNormalizer.TryNormalizeBarcode(codigoBarra, out string? codigo))
                return null;
            return dal.BuscarPorCodigoBarra(codigo!);
        }

        private void ValidarCodigoBarraUnico(string? codigo, int? excluirId)
        {
            if (codigo == null)
                return;

            if (codigo.Length > 32)
                throw new Exception("El código de barras no puede superar 32 caracteres.");

            if (dal.ExisteCodigoBarra(codigo, excluirId))
                throw new Exception("Ya existe un producto con ese código de barras.");
        }

        public void EliminarProducto(int id)
        {
            dal.Eliminar(id);
        }

        public DataTable ObtenerProductos()
        {
            return dal.Listar();
        }
    }
}
