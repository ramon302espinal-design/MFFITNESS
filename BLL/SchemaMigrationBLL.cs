using CORE;
using CORE.Commands;
using DL;
using DL.Migrations;

namespace BLL
{
    /// <summary>
    /// Facade del motor de migraciones. Sin WinForms. Usable por UI y UpdateManager.
    /// </summary>
    public static class SchemaMigrationBLL
    {
        public static int GetCurrentDbVersion()
        {
            var db = new DBHelper();
            return SchemaVersionDAL.GetCurrent(db).Version;
        }

        public static MigrationRunResult ApplyPendingDetailed(string? migrationsDirectory = null)
        {
            var runner = new MigrationRunner(new DBHelper(), MigrationLog.Write);
            return runner.Run(migrationsDirectory);
        }

        public static CommandResult ApplyPending(string? migrationsDirectory = null)
        {
            var result = ApplyPendingDetailed(migrationsDirectory);

            if (result.Success)
                return CommandResult.Ok(result.Message, result);

            return CommandResult.Fail(result.Message);
        }

        /// <summary>
        /// Aplica migraciones solo hasta targetDbVersion (FASE 10B). Nunca supera el target.
        /// </summary>
        public static MigrationRunResult ApplyUpToDetailed(int targetDbVersion, string? migrationsDirectory = null)
        {
            var runner = new MigrationRunner(new DBHelper(), MigrationLog.Write);
            return runner.RunUpTo(targetDbVersion, migrationsDirectory);
        }

        public static CommandResult ApplyUpTo(int targetDbVersion, string? migrationsDirectory = null)
        {
            var result = ApplyUpToDetailed(targetDbVersion, migrationsDirectory);
            if (result.Success)
                return CommandResult.Ok(result.Message, result);

            return CommandResult.Fail(result.Message);
        }
    }
}
