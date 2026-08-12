using BLL.Update;
using CORE.Update;

namespace BLL
{
    /// <summary>
    /// Facade Update Manager V0. Sin WinForms. Preparado para UpdateManager.exe.
    /// </summary>
    public static class UpdateManagerBLL
    {
        public static UpdateResult Run(UpdateTarget target, UpdateOrchestratorHooks? hooks = null) =>
            new UpdateOrchestrator(hooks).Run(target);
    }
}
