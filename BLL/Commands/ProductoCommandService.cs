using CORE;
using CORE.Commands;

namespace BLL.Commands
{
    public static class ProductoCommandService
    {
        public static CommandResult AgregarProducto(
            string nombre,
            int categoriaId,
            decimal compra,
            decimal venta,
            int stockInicial,
            int stockMinimo,
            bool activo)
        {
            try
            {
                var bll = new ProductoBLL();
                int id = bll.AgregarProductoConId(
                    nombre.Trim(), categoriaId, compra, venta, stockInicial, stockMinimo, activo);
                return CommandResult.Ok("Producto guardado correctamente.", id);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult EditarProducto(
            int id,
            string nombre,
            int categoriaId,
            decimal compra,
            decimal venta,
            int stockMinimo,
            bool activo)
        {
            try
            {
                var bll = new ProductoBLL();
                bll.EditarProducto(id, nombre.Trim(), categoriaId, compra, venta, stockMinimo, activo);
                return CommandResult.Ok("Producto actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult EliminarProducto(int id)
        {
            try
            {
                var bll = new ProductoBLL();
                bll.EliminarProducto(id);
                return CommandResult.Ok("Producto eliminado.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult RegistrarEntrada(int productoId, int cantidad, string descripcion, string? usuario = null)
        {
            try
            {
                var stock = new StockBLL();
                int movimientoId = stock.RegistrarEntradaConId(
                    productoId, cantidad, ResolveUsuario(usuario), descripcion.Trim());
                return CommandResult.Ok("Entrada registrada.", movimientoId);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        public static CommandResult RegistrarSalida(int productoId, int cantidad, string descripcion, string? usuario = null)
        {
            try
            {
                var stock = new StockBLL();
                int movimientoId = stock.RegistrarSalidaConId(
                    productoId, cantidad, ResolveUsuario(usuario), descripcion.Trim());
                return CommandResult.Ok("Salida registrada.", movimientoId);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }

        private static string ResolveUsuario(string? usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario))
                return usuario;
            if (!string.IsNullOrWhiteSpace(Sesion.Usuario))
                return Sesion.Usuario;
            return "ADMIN";
        }
    }
}
