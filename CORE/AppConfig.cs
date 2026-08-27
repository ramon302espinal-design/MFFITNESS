using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CORE
{
    /// <summary>
    /// Configuración centralizada de la app.
    /// Connection strings: appsettings*.json + variables de entorno (sin hardcode en DAL).
    /// Prioridad de entorno: variables → appsettings.Local/DefaultEnvironment → Development.
    /// El POS instalado escribe appsettings.Local.json con Production (Deploy/Publish Release).
    /// </summary>
    public static class AppConfig
    {
        private static readonly object Sync = new();
        private static ResolvedDatabase? _resolved;
        private static IConfigurationRoot? _configuration;

        /// <summary>Development | Production (u otro nombre de perfil en appsettings).</summary>
        public static string EnvironmentName => Resolve().EnvironmentName;

        /// <summary>
        /// Entorno sin abrir BD (útil para rutas Ollama/FacturaGastos).
        /// Misma prioridad que Resolve: env vars → appsettings.Local/DefaultEnvironment → Development.
        /// </summary>
        public static string PeekEnvironment() => DetectEnvironment();

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
        /// Carga Ollama:BaseUrl / VisionModel / TimeoutSeconds desde appsettings.
        /// Seguro llamar múltiples veces; no toca la BD.
        /// </summary>
        public static void LoadOllamaOptions()
        {
            try
            {
                string env = DetectEnvironment();
                IConfigurationRoot config = BuildConfiguration(env);
                Ollama.OllamaOptions.ApplyFromConfiguration(config);
            }
            catch
            {
                // Mantener defaults (qwen2.5vl:7b local).
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
        /// 1) MFFITNESS_ENVIRONMENT / DOTNET_ENVIRONMENT / ASPNETCORE_ENVIRONMENT / NODE_ENV
        /// 2) Database:DefaultEnvironment en appsettings.Local.json luego appsettings.json
        /// 3) Development (seguro al desarrollar sin perfil)
        /// </summary>
        private static string DetectEnvironment()
        {
            string? raw =
                Environment.GetEnvironmentVariable("MFFITNESS_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("NODE_ENV");

            if (!string.IsNullOrWhiteSpace(raw))
                return NormalizeEnvironmentName(raw);

            string? fromFiles = TryReadDefaultEnvironmentFromAppsettingsFiles();
            if (!string.IsNullOrWhiteSpace(fromFiles))
                return fromFiles;

            return "Development";
        }

        private static string NormalizeEnvironmentName(string raw)
        {
            raw = raw.Trim();

            if (raw.Equals("prod", StringComparison.OrdinalIgnoreCase)
                || raw.Equals("production", StringComparison.OrdinalIgnoreCase))
                return "Production";

            if (raw.Equals("dev", StringComparison.OrdinalIgnoreCase)
                || raw.Equals("development", StringComparison.OrdinalIgnoreCase))
                return "Development";

            return raw;
        }

        /// <summary>
        /// Lee DefaultEnvironment sin construir IConfiguration completa
        /// (evita dependencia circular env ↔ config).
        /// </summary>
        private static string? TryReadDefaultEnvironmentFromAppsettingsFiles()
        {
            string basePath = AppContext.BaseDirectory;
            foreach (string fileName in new[] { "appsettings.Local.json", "appsettings.json" })
            {
                string path = Path.Combine(basePath, fileName);
                if (!File.Exists(path))
                    continue;

                try
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    if (!doc.RootElement.TryGetProperty("Database", out JsonElement database))
                        continue;
                    if (!database.TryGetProperty("DefaultEnvironment", out JsonElement defEnv))
                        continue;

                    string? value = defEnv.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return NormalizeEnvironmentName(value);
                }
                catch
                {
                    // Archivo corrupto / parcial: seguir con el siguiente.
                }
            }

            return null;
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
