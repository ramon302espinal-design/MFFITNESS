using BLL.Update;
using CORE.Update;

namespace BLL
{
    /// <summary>
    /// Facade E2E. Sin WinForms. Invocada desde UpdateManager.exe.
    /// </summary>
    public static class UpdateEndToEndBLL
    {
        public static UpdateEndToEndResult Run(
            UpdateEndToEndRequest request,
            UpdateEndToEndHooks? hooks = null,
            UpdateSessionStorage? storage = null) =>
            new UpdateEndToEndOrchestrator(hooks, storage: storage).Run(request);

        public static UpdateEndToEndResult Recover(
            UpdateSession session,
            UpdateEndToEndRequest? request = null,
            UpdateEndToEndHooks? hooks = null,
            UpdateSessionStorage? storage = null) =>
            new UpdateEndToEndOrchestrator(hooks, storage: storage).Recover(session, request);
    }
}
