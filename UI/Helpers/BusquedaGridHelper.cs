using System;
using System.Collections.Generic;

namespace UI.Helpers
{
    public static class BusquedaGridHelper
    {
        public static string EscaparFiltroDataView(string valor)
        {
            return valor
                .Replace("[", "[[]", StringComparison.Ordinal)
                .Replace("]", "[]]", StringComparison.Ordinal)
                .Replace("*", "[*]", StringComparison.Ordinal)
                .Replace("%", "[%]", StringComparison.Ordinal)
                .Replace("'", "''", StringComparison.Ordinal);
        }

        public static string ConstruirFiltroClientes(string termino)
        {
            var valor = EscaparFiltroDataView(termino);
            var like = $"'%{valor}%'";

            return $"Convert(Id, 'System.String') LIKE {like} " +
                   $"OR Nombre LIKE {like} " +
                   $"OR Telefono LIKE {like} " +
                   $"OR Direccion LIKE {like} " +
                   $"OR Convert(FechaNacimiento, 'System.String') LIKE {like}";
        }

        public static string ConstruirFiltroEstadoClientes(string termino)
        {
            var valor = EscaparFiltroDataView(termino);
            var like = $"'%{valor}%'";

            return $"Convert(ID, 'System.String') LIKE {like} " +
                   $"OR Nombre LIKE {like} " +
                   $"OR Membresia LIKE {like} " +
                   $"OR Estado LIKE {like} " +
                   $"OR Convert(FechaInicio, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaFin, 'System.String') LIKE {like}";
        }

        public static string ConstruirFiltroDeudas(string termino)
        {
            var valor = EscaparFiltroDataView(termino);
            var like = $"'%{valor}%'";

            return $"Convert(Id, 'System.String') LIKE {like} " +
                   $"OR Convert(ClienteId, 'System.String') LIKE {like} " +
                   $"OR Nombre LIKE {like} " +
                   $"OR Concepto LIKE {like} " +
                   $"OR Plan LIKE {like} " +
                   $"OR Estado LIKE {like} " +
                   $"OR AporteInicial LIKE {like} " +
                   $"OR Convert(MontoTotal, 'System.String') LIKE {like} " +
                   $"OR Convert(MontoPagado, 'System.String') LIKE {like} " +
                   $"OR Convert(Saldo, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaVencimiento, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaInicioMembresia, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaFinMembresia, 'System.String') LIKE {like}";
        }

        /// <summary>
        /// Filtro inteligente: nombre/apellido, teléfono, lugar (dirección), tipo plan, movimiento.
        /// </summary>
        public static string ConstruirFiltroHistorialMembresia(string termino)
        {
            var valor = EscaparFiltroDataView(termino);
            var like = $"'%{valor}%'";

            return $"Convert(ClienteId, 'System.String') LIKE {like} " +
                   $"OR Nombre LIKE {like} " +
                   $"OR Telefono LIKE {like} " +
                   $"OR Direccion LIKE {like} " +
                   $"OR PlanNombre LIKE {like} " +
                   $"OR TipoMovimiento LIKE {like} " +
                   $"OR Usuario LIKE {like} " +
                   $"OR Nota LIKE {like} " +
                   $"OR Convert(FechaPago, 'System.String') LIKE {like} " +
                   $"OR Convert(FechaVence, 'System.String') LIKE {like} " +
                   $"OR Convert(Monto, 'System.String') LIKE {like}";
        }

        /// <summary>
        /// Filtro POS en vivo: Id, nombre, categoría, código de barras, precios y stock.
        /// Multi-token (AND): "cool hea" exige ambas partes.
        /// </summary>
        public static string ConstruirFiltroProductosPos(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return string.Empty;

            var partes = new List<string>();
            foreach (string tokenBruto in termino.Split(
                         new[] { ' ', '\t', ',', ';' },
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var valor = EscaparFiltroDataView(tokenBruto);
                var like = $"'%{valor}%'";
                var likeInicio = $"'{valor}%'";
                var likePalabra = $"'% {valor}%'";

                partes.Add(
                    "(" +
                    $"Convert(Id, 'System.String') LIKE {like} " +
                    $"OR Convert(Id, 'System.String') LIKE {likeInicio} " +
                    $"OR Nombre LIKE {like} " +
                    $"OR Nombre LIKE {likeInicio} " +
                    $"OR Nombre LIKE {likePalabra} " +
                    $"OR Categoria LIKE {like} " +
                    $"OR Categoria LIKE {likeInicio} " +
                    $"OR CodigoBarra LIKE {like} " +
                    $"OR CodigoBarra LIKE {likeInicio} " +
                    $"OR Convert(IdCategoria, 'System.String') LIKE {like} " +
                    $"OR Convert(PrecioCompra, 'System.String') LIKE {like} " +
                    $"OR Convert(PrecioVenta, 'System.String') LIKE {like} " +
                    $"OR Convert(StockActual, 'System.String') LIKE {like} " +
                    $"OR Convert(StockMinimo, 'System.String') LIKE {like}" +
                    ")");
            }

            return partes.Count == 0 ? string.Empty : string.Join(" AND ", partes);
        }

        /// <summary>
        /// Iniciales en mayúsculas: "Proteína Hidrolizada" → "PH", "Cool Head" → "CH".
        /// </summary>
        public static string ConstruirSiglasProducto(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return string.Empty;

            var siglas = new System.Text.StringBuilder();
            bool inicioPalabra = true;

            foreach (char c in nombre.Trim())
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (inicioPalabra)
                    {
                        siglas.Append(char.ToUpperInvariant(c));
                        inicioPalabra = false;
                    }
                }
                else
                {
                    inicioPalabra = true;
                }
            }

            return siglas.ToString();
        }

        /// <summary>
        /// Filtro combo inventario: nombre, código, categoría e iniciales (columna Siglas).
        /// Multi-token AND: "c h" exige ambas partes.
        /// </summary>
        public static string ConstruirFiltroProductosCombo(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return string.Empty;

            var partes = new List<string>();
            foreach (string tokenBruto in termino.Split(
                         new[] { ' ', '\t', ',', ';' },
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var valor = EscaparFiltroDataView(tokenBruto);
                var valorSiglas = EscaparFiltroDataView(tokenBruto.ToUpperInvariant());
                var like = $"'%{valor}%'";
                var likeInicio = $"'{valor}%'";
                var likePalabra = $"'% {valor}%'";
                var likeSiglas = $"'{valorSiglas}%'";

                partes.Add(
                    "(" +
                    $"Convert(Id, 'System.String') LIKE {like} " +
                    $"OR Convert(Id, 'System.String') LIKE {likeInicio} " +
                    $"OR Nombre LIKE {like} " +
                    $"OR Nombre LIKE {likeInicio} " +
                    $"OR Nombre LIKE {likePalabra} " +
                    $"OR Categoria LIKE {like} " +
                    $"OR Categoria LIKE {likeInicio} " +
                    $"OR CodigoBarra LIKE {like} " +
                    $"OR CodigoBarra LIKE {likeInicio} " +
                    $"OR Siglas LIKE {likeSiglas} " +
                    $"OR Siglas LIKE {like}" +
                    ")");
            }

            return partes.Count == 0 ? string.Empty : string.Join(" AND ", partes);
        }

        /// <summary>
        /// Filtro combo cliente/miembro: nombre, teléfono, dirección e iniciales (Siglas).
        /// Multi-token AND: "j m" exige ambas partes.
        /// </summary>
        public static string ConstruirFiltroClientesCombo(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return string.Empty;

            var partes = new List<string>();
            foreach (string tokenBruto in termino.Split(
                         new[] { ' ', '\t', ',', ';' },
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var valor = EscaparFiltroDataView(tokenBruto);
                var valorSiglas = EscaparFiltroDataView(tokenBruto.ToUpperInvariant());
                var like = $"'%{valor}%'";
                var likeInicio = $"'{valor}%'";
                var likePalabra = $"'% {valor}%'";
                var likeSiglas = $"'{valorSiglas}%'";

                partes.Add(
                    "(" +
                    $"Convert(Id, 'System.String') LIKE {like} " +
                    $"OR Convert(Id, 'System.String') LIKE {likeInicio} " +
                    $"OR Nombre LIKE {like} " +
                    $"OR Nombre LIKE {likeInicio} " +
                    $"OR Nombre LIKE {likePalabra} " +
                    $"OR Telefono LIKE {like} " +
                    $"OR Telefono LIKE {likeInicio} " +
                    $"OR Direccion LIKE {like} " +
                    $"OR Direccion LIKE {likeInicio} " +
                    $"OR Siglas LIKE {likeSiglas} " +
                    $"OR Siglas LIKE {like}" +
                    ")");
            }

            return partes.Count == 0 ? string.Empty : string.Join(" AND ", partes);
        }

        /// <summary>
        /// Historial de ventas de productos: multi-token (AND), fechas inteligentes
        /// (hoy/ayer/dd/MM/yyyy) y coincidencia por cliente, metodo, usuario o producto vendido.
        /// </summary>
        public static string ConstruirFiltroHistorialVentasProductos(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return string.Empty;

            var partes = new List<string>();
            var rangoCompleto = BusquedaCierreCajaHelper.IntentarResolverFechaInteligente(termino);
            if (rangoCompleto != null)
            {
                if (rangoCompleto.Desde != null)
                    partes.Add($"Fecha >= #{rangoCompleto.Desde.Value:MM/dd/yyyy}#");
                if (rangoCompleto.Hasta != null)
                    partes.Add($"Fecha < #{rangoCompleto.Hasta.Value.AddDays(1):MM/dd/yyyy}#");
                return string.Join(" AND ", partes);
            }

            foreach (string tokenBruto in termino.Split(
                         new[] { ' ', '\t', ',', ';' },
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var rangoToken = BusquedaCierreCajaHelper.IntentarResolverFechaInteligente(tokenBruto);
                if (rangoToken != null)
                {
                    if (rangoToken.Desde != null)
                        partes.Add($"Fecha >= #{rangoToken.Desde.Value:MM/dd/yyyy}#");
                    if (rangoToken.Hasta != null)
                        partes.Add($"Fecha < #{rangoToken.Hasta.Value.AddDays(1):MM/dd/yyyy}#");
                    continue;
                }

                string valor = EscaparFiltroDataView(tokenBruto);
                string like = $"'%{valor}%'";
                partes.Add(
                    "(" +
                    $"Convert(Id, 'System.String') LIKE {like} " +
                    $"OR Convert(ClienteId, 'System.String') LIKE {like} " +
                    $"OR Cliente LIKE {like} " +
                    $"OR Telefono LIKE {like} " +
                    $"OR Productos LIKE {like} " +
                    $"OR MetodoPago LIKE {like} " +
                    $"OR Usuario LIKE {like} " +
                    $"OR Convert(Total, 'System.String') LIKE {like} " +
                    $"OR Convert(MontoPagado, 'System.String') LIKE {like} " +
                    $"OR Convert(Saldo, 'System.String') LIKE {like} " +
                    $"OR Convert(Fecha, 'System.String') LIKE {like}" +
                    ")");
            }

            return partes.Count == 0 ? string.Empty : string.Join(" AND ", partes);
        }
    }
}
