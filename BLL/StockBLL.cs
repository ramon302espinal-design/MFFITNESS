using DL;
using System;
using System.Data;

namespace BLL
{
    public class StockBLL
    {
        private readonly StockDAL dal = new StockDAL();

        public DataTable ObtenerMovimientos()
        {
            return dal.ObtenerMovimientos();
        }

        public int RegistrarEntradaConId(int productoId, int cantidad, string usuario, string descripcion)
        {
            ValidarParametros(productoId, cantidad, usuario, descripcion);
            return dal.RegistrarEntrada(productoId, cantidad, usuario, descripcion);
        }

        public int RegistrarSalidaConId(int productoId, int cantidad, string usuario, string descripcion)
        {
            ValidarParametros(productoId, cantidad, usuario, descripcion);
            return dal.RegistrarSalida(productoId, cantidad, usuario, descripcion);
        }

        public void RevertirMovimiento(int movimientoId, string usuario)
        {
            if (movimientoId <= 0)
                throw new Exception("Movimiento inválido.");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("Usuario obligatorio.");

            dal.RevertirMovimiento(movimientoId, usuario);
        }

        private void ValidarParametros(int productoId, int cantidad, string usuario, string descripcion)
        {
            if (productoId <= 0)
                throw new Exception("Producto inválido.");

            if (cantidad <= 0)
                throw new Exception("La cantidad debe ser mayor a cero.");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("Usuario obligatorio.");

            if (string.IsNullOrWhiteSpace(descripcion))
                throw new Exception("Descripción obligatoria.");
        }
    }
}
