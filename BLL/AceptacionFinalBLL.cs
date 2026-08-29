using CORE;
using DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    /// <summary>
    /// Fase 13 — verificacion final de sinergia 100%.
    /// Items automatizados validan coherencia BD; manuales documentan prueba operador.
    /// </summary>
    public class AceptacionFinalBLL
    {
        private readonly AceptacionFinalDAL dal = new();

        public AceptacionFinalReporte Ejecutar()
        {
            var integridad = dal.Integridad;
            var r = new AceptacionFinalReporte
            {
                Entorno = AppConfig.EnvironmentName,
                BaseDatos = AppConfig.DatabaseName,
                AppVersion = AppVersion.Informational,
                SchemaVersion = SchemaMigrationBLL.GetCurrentDbVersion()
            };

            r.Items.Add(Auto("13.1", "Venta contado cobrada tiene caja",
                dal.ContarVentasContadoSinCaja(),
                "ventas contado sin ingreso caja"));

            r.Items.Add(Auto("13.2", "Venta financiada + pago inicial coherente",
                dal.ContarVentasFinanciadasConCobroSinCaja()
                + integridad.ContarVentasFinanciadasHuerfanas()
                + integridad.ContarFinanciamientosConCobroSinPagoInicial(),
                "cobro sin caja / huérfana / sin PAGO_INICIAL"));

            r.Items.Add(Auto("13.3", "Deudas membresia vinculadas",
                dal.ContarDeudasMembresiaRotas(),
                "MembresiaId invalido"));

            r.Items.Add(Auto("13.4", "Abonos de deuda alineados",
                dal.ContarDeudasAbonoDesalineado(),
                "MontoPagado != sum PagosDeuda"));

            r.Items.Add(Auto("13.5", "Pago inicial deuda vs historial/caja",
                integridad.ContarFinanciamientosConCobroSinPagoInicial()
                + integridad.ContarIngresosVentaSinVenta(),
                "PAGO_INICIAL o caja desalineados"));

            r.Items.Add(Auto("13.6", "Reversos de pago coherentes",
                dal.ContarPagosDeudaAnuladosSinReversoCaja(),
                "pagos anulados sin reverso caja (90d)"));

            r.Items.Add(Manual("13.7",
                "Export PDF historial deudas = grid en pantalla",
                "Exportar PDF y comparar totales con grid visible"));

            r.Items.Add(Manual("13.8",
                "CRM reportes POS actualizado tras cobro sin cambiar periodo",
                "Cobrar deuda/venta y abrir CRM Reportes POS mismo periodo"));

            r.Items.Add(Auto("13.9", "0 ventas financiadas huérfanas",
                integridad.ContarVentasFinanciadasHuerfanas(),
                "ventas Saldo>0 sin deuda ACTIVA (Venta Id N)"));

            r.Items.Add(Manual("13.10",
                "Atajos + buscadores sin pantallas stale",
                "Sesion: P/C/E/D/H/R/I/M + buscar en Deudas/CRM sin perder sync"));

            r.AutomatizadosPass = r.Items
                .Where(i => i.Automatizado && i.Pass)
                .Count();
            r.AutomatizadosTotal = r.Items.Count(i => i.Automatizado);
            r.ManualPendientes = r.Items.Count(i => !i.Automatizado);
            r.SinergiaLista = r.Items
                .Where(i => i.Automatizado)
                .All(i => i.Pass);

            return r;
        }

        public void RegistrarEnLog()
        {
            AceptacionFinalReporte r = Ejecutar();
            if (r.SinergiaLista)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Fase13] AUTO OK {r.AutomatizadosPass}/{r.AutomatizadosTotal} · " +
                    $"Manual pendiente: {r.ManualPendientes} · {r.BaseDatos}");
                return;
            }

            foreach (var item in r.Items.Where(i => i.Automatizado && !i.Pass))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Fase13] FAIL {item.Id} {item.Titulo}: {item.Contador} ({item.Detalle})");
            }
        }

        private static AceptacionFinalItem Auto(string id, string titulo, int contador, string detalle) =>
            new()
            {
                Id = id,
                Titulo = titulo,
                Automatizado = true,
                Pass = contador == 0,
                Contador = contador,
                Detalle = detalle
            };

        private static AceptacionFinalItem Manual(string id, string titulo, string instruccion) =>
            new()
            {
                Id = id,
                Titulo = titulo,
                Automatizado = false,
                Pass = false,
                Contador = -1,
                Detalle = instruccion
            };
    }

    public sealed class AceptacionFinalReporte
    {
        public string Entorno { get; set; } = "";
        public string BaseDatos { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public int SchemaVersion { get; set; }
        public List<AceptacionFinalItem> Items { get; set; } = new();
        public int AutomatizadosPass { get; set; }
        public int AutomatizadosTotal { get; set; }
        public int ManualPendientes { get; set; }
        public bool SinergiaLista { get; set; }
    }

    public sealed class AceptacionFinalItem
    {
        public string Id { get; set; } = "";
        public string Titulo { get; set; } = "";
        public bool Automatizado { get; set; }
        public bool Pass { get; set; }
        public int Contador { get; set; }
        public string Detalle { get; set; } = "";
    }
}
