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

        public int AgregarProductoConId(string nombre, int categoriaId, decimal compra, decimal venta, int stockInicial, int stockMinimo, bool activo)
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

            return dal.Agregar(nombre, categoriaId, compra, venta, stockInicial, stockMinimo, activo);
        }

        public void EditarProducto(int id, string nombre, int categoriaId, decimal compra, decimal venta, int stockMinimo, bool activo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception("El nombre es obligatorio.");

            if (compra <= 0)
                throw new Exception("El precio de compra debe ser mayor a 0.");

            if (venta <= 0)
                throw new Exception("El precio de venta debe ser mayor a 0.");

            if (stockMinimo < 0)
                throw new Exception("El stock mínimo no puede ser negativo.");

            dal.Editar(id, nombre, categoriaId, compra, venta, stockMinimo, activo);
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
