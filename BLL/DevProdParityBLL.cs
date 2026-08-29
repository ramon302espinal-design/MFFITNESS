using CORE;
using DL;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Fase 12 — paridad DEV ↔ PROD (schema, integridad, entorno).
    /// </summary>
    public class DevProdParityBLL
    {
        private const string DevCatalog = "MF_CYBER_DB_DEV";
        private const string ProdCatalog = "MF CYBER DB";

        public DevProdParidadReporte EjecutarAuditoriaLocal()
        {
            var reporte = new DevProdParidadReporte
            {
                Entorno = AppConfig.EnvironmentName,
                BaseDatos = AppConfig.DatabaseName,
                AppVersion = AppVersion.Informational,
                SchemaVersion = SchemaMigrationBLL.GetCurrentDbVersion()
            };

            var integridad = new IntegridadFinancieraBLL().EjecutarAuditoria();
            reporte.AlertasIntegridad = integridad.TotalAlertas;
            reporte.VentasFinanciadasHuerfanas = integridad.VentasFinanciadasHuerfanas;

            return reporte;
        }

        /// <summary>Compara schema e integridad entre DEV y PROD (solo LocalDB).</summary>
        public DevProdParidadComparacion CompararDevYProd()
        {
            var cmp = new DevProdParidadComparacion();

            cmp.Dev = LeerSnapshot(DevCatalog);
            cmp.Prod = LeerSnapshot(ProdCatalog);
            cmp.SchemaParidadOk = cmp.Dev.SchemaVersion == cmp.Prod.SchemaVersion;
            cmp.IntegridadParidadOk =
                cmp.Dev.VentasFinanciadasHuerfanas == 0
                && cmp.Prod.VentasFinanciadasHuerfanas == 0;
            cmp.ParidadOk = cmp.SchemaParidadOk && cmp.IntegridadParidadOk;

            return cmp;
        }

        private static DevProdParidadSnapshot LeerSnapshot(string catalog)
        {
            string cs = BuildLocalDbConnectionString(catalog);
            var snap = new DevProdParidadSnapshot { BaseDatos = catalog };

            using var conn = new SqlConnection(cs);
            conn.Open();

            using (var cmd = new SqlCommand("SELECT ISNULL(MAX(Version), 0) FROM dbo.SchemaVersion", conn))
                snap.SchemaVersion = Convert.ToInt32(cmd.ExecuteScalar());

            const string orphanQuery = @"
                SELECT COUNT(*)
                FROM Ventas v
                WHERE v.Saldo > 0
                  AND NOT EXISTS (
                      SELECT 1 FROM Deudas d
                      WHERE d.Estado = 'ACTIVA'
                        AND d.Saldo > 0
                        AND d.Concepto LIKE '%(Venta Id ' + CAST(v.Id AS NVARCHAR(20)) + ')%'
                  )";

            using (var cmd = new SqlCommand(orphanQuery, conn))
                snap.VentasFinanciadasHuerfanas = Convert.ToInt32(cmd.ExecuteScalar());

            return snap;
        }

        private static string BuildLocalDbConnectionString(string catalog) =>
            $"Server=(localdb)\\MSSQLLocalDB;Database={catalog};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

        public void RegistrarComparacionEnLog()
        {
            DevProdParidadComparacion c = CompararDevYProd();
            if (c.ParidadOk)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Paridad F12] OK · Schema DEV={c.Dev.SchemaVersion} PROD={c.Prod.SchemaVersion} · Huérfanas=0");
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[Paridad F12] DESALINEADO · Schema DEV={c.Dev.SchemaVersion} PROD={c.Prod.SchemaVersion} · " +
                $"Huérfanas DEV={c.Dev.VentasFinanciadasHuerfanas} PROD={c.Prod.VentasFinanciadasHuerfanas}");
        }
    }

    public sealed class DevProdParidadReporte
    {
        public string Entorno { get; set; } = "";
        public string BaseDatos { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public int SchemaVersion { get; set; }
        public int AlertasIntegridad { get; set; }
        public int VentasFinanciadasHuerfanas { get; set; }
    }

    public sealed class DevProdParidadComparacion
    {
        public DevProdParidadSnapshot Dev { get; set; } = new();
        public DevProdParidadSnapshot Prod { get; set; } = new();
        public bool SchemaParidadOk { get; set; }
        public bool IntegridadParidadOk { get; set; }
        public bool ParidadOk { get; set; }
    }

    public sealed class DevProdParidadSnapshot
    {
        public string BaseDatos { get; set; } = "";
        public int SchemaVersion { get; set; }
        public int VentasFinanciadasHuerfanas { get; set; }
    }
}
