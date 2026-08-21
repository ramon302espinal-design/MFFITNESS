using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CORE
{
    /// <summary>
    /// Configuración centralizada de la app.
    /// Connection strings: appsettings*.json + variables de entorno (sin hardcode en DAL).
    /// Entorno por defecto: Development (MF_CYBER_DB_DEV) por seguridad.
    /// </summary>
    public static class AppConfig
    {
        private static readonly object Sync = new();
        private static ResolvedDatabase? _resolved;
        private static IConfigurationRoot? _configuration;

        /// <summary>Development | Production (u otro nombre de perfil en appsettings).</summary>
        public static string EnvironmentName => Resolve().EnvironmentName;

        /// <summary>Nombre de la base (Initial Catalog / Database).</summary>
        public static string DatabaseName => Resolve().DatabaseName;

        /// <summary>Cadena lista para Microsoft.Data.SqlClient.</summary>
        public static string ConnectionString => Resolve().ConnectionString;

        public static bool IsProduction =>
            string.Equals(EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase);

        public static bool IsDevelopment =>
            string.Equals(EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase);

        public static bool ModoPrueba
        {
            get
            {
                string? valor = System.Configuration.ConfigurationManager.AppSettings["ModoPrueba"];
                return valor != null && valor.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Abre la conexión, escribe el log de arranque y cierra.
        /// Idempotente (solo verifica una vez por proceso).
        /// </summary>
        public static void EnsureDatabaseLogged()
        {
            var db = Resolve();
            if (db.Logged)
                return;

            lock (Sync)
            {
                if (db.Logged)
                    return;

                try
                {
                    using var conn = new SqlConnection(db.ConnectionString);
                    conn.Open();
                    string catalog = string.IsNullOrWhiteSpace(conn.Database)
                        ? db.DatabaseName
                        : conn.Database;

                    string line = $"[DATABASE] Conectado exitosamente a: {catalog}";
                    WriteStartupLog(line);
                    WriteStartupLog($"[DATABASE] Entorno: {db.EnvironmentName}");
                    db.Logged = true;
                }
                catch (Exception ex)
                {
                    string fail =
                        $"[DATABASE] ERROR al conectar a '{db.DatabaseName}' " +
                        $"(entorno {db.EnvironmentName}): {ex.Message}";
                    WriteStartupLog(fail);
                    throw new InvalidOperationException(fail, ex);
                }
            }
        }

        private static ResolvedDatabase Resolve()
        {
            if (_resolved != null)
                return _resolved;

            lock (Sync)
            {
                if (_resolved != null)
                    return _resolved;

                string env = DetectEnvironment();
                IConfigurationRoot config = BuildConfiguration(env);
                _configuration = config;

                string? cs =
                    config[$"Database:ConnectionStrings:{env}"]
                    ?? config.GetConnectionString(env)
                    ?? config.GetConnectionString("Default")
                    ?? BuiltInConnectionString(env);

                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException(
                        $"No hay ConnectionString para el entorno '{env}'.");

                cs = NormalizeSqlClientConnectionString(cs);
                var builder = new SqlConnectionStringBuilder(cs);
                if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
                    throw new InvalidOperationException(
                        $"ConnectionString del entorno '{env}' sin Database/Initial Catalog.");

                _resolved = new ResolvedDatabase(env, builder.InitialCatalog, builder.ConnectionString);
                return _resolved;
            }
        }

        /// <summary>
        /// Orden: MFFITNESS_ENVIRONMENT → DOTNET_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → NODE_ENV.
        /// Sin valor → Development (seguro).
        /// </summary>
        private static string DetectEnvironment()
        {
            string? raw =
                Environment.GetEnvironmentVariable("MFFITNESS_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("NODE_ENV");

            if (string.IsNullOrWhiteSpace(raw))
                return "Development";

            raw = raw.Trim();

            if (raw.Equals("prod", StringComparison.OrdinalIgnoreCase)
                || raw.Equals("production", StringComparison.OrdinalIgnoreCase))
                return "Production";

            if (raw.Equals("dev", StringComparison.OrdinalIgnoreCase)
                || raw.Equals("development", StringComparison.OrdinalIgnoreCase))
                return "Development";

            // Perfil personalizado (p. ej. Staging): se busca Database:ConnectionStrings:{raw}
            return raw;
        }

        private static IConfigurationRoot BuildConfiguration(string env)
        {
            string basePath = AppContext.BaseDirectory;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static string BuiltInConnectionString(string env)
        {
            string database = env.Equals("Production", StringComparison.OrdinalIgnoreCase)
                ? "MF CYBER DB"
                : "MF_CYBER_DB_DEV";

            return NormalizeSqlClientConnectionString(
                $"Server=(localdb)\\MSSQLLocalDB;Database={database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True");
        }

        /// <summary>
        /// Homogeneiza Trusted_Connection / TrustServerCertificate para SqlClient
        /// (equivalente práctico a ODBC Driver 18 Trusted_Connection=yes + TrustServerCertificate=yes).
        /// </summary>
        private static string NormalizeSqlClientConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                TrustServerCertificate = true,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true
            };

            if (string.IsNullOrWhiteSpace(builder.DataSource))
                builder.DataSource = @"(localdb)\MSSQLLocalDB";

            return builder.ConnectionString;
        }

        private static void WriteStartupLog(string message)
        {
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
            Debug.WriteLine(line);
            Console.WriteLine(message);
            Trace.WriteLine(message);

            try
            {
                string dir = Path.Combine(FacturaStorage.CarpetaRaizMffitness, "logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(
                    Path.Combine(dir, $"database-{DateTime.Today:yyyyMMdd}.log"),
                    line + Environment.NewLine);
            }
            catch
            {
                // No bloquear arranque por fallos de log en disco.
            }
        }

        private sealed class ResolvedDatabase
        {
            public ResolvedDatabase(string environmentName, string databaseName, string connectionString)
            {
                EnvironmentName = environmentName;
                DatabaseName = databaseName;
                ConnectionString = connectionString;
            }

            public string EnvironmentName { get; }
            public string DatabaseName { get; }
            public string ConnectionString { get; }
            public bool Logged { get; set; }
        }
    }
}
