using CORE;
using DL;
using System;
using System.Data;

namespace BLL
{
    public class StockBLL
    {
        private readonly StockDAL dal = new StockDAL();
        private readonly ProductoDAL productoDal = new ProductoDAL();

        public DataTable ObtenerMovimientos()
        {
            return dal.ObtenerMovimientos();
        }

        public int RegistrarEntradaConId(
            int productoId,
            int cantidad,
            string usuario,
            string descripcion,
            decimal? costoUnitario = null)
        {
            ValidarParametros(productoId, cantidad, usuario, descripcion);
            if (costoUnitario.HasValue && costoUnitario.Value <= 0)
                throw new Exception("El costo de entrada debe ser mayor a cero.");

            int movimientoId = dal.RegistrarEntrada(productoId, cantidad, usuario, descripcion, costoUnitario);
            NotificarSiStockCritico(productoId);
            return movimientoId;
        }

        public int RegistrarSalidaConId(int productoId, int cantidad, string usuario, string descripcion)
        {
            ValidarParametros(productoId, cantidad, usuario, descripcion);
            int movimientoId = dal.RegistrarSalida(productoId, cantidad, usuario, descripcion);
            NotificarSiStockCritico(productoId);
            return movimientoId;
        }

        public void RevertirMovimiento(int movimientoId, string usuario)
        {
            if (movimientoId <= 0)
                throw new Exception("Movimiento inválido.");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("Usuario obligatorio.");

            dal.RevertirMovimiento(movimientoId, usuario);
        }

        private void NotificarSiStockCritico(int productoId)
        {
            try
            {
                var (_, stock, minimo) = productoDal.ObtenerCostoStockYMinimo(productoId);
                if (stock <= 0 || stock <= minimo)
                    AppEventos.StockCritico(productoId);
            }
            catch
            {
                // El movimiento ya se registró; el aviso es best-effort.
            }
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
