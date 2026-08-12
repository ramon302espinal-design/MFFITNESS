using BLL.Update;
using CORE.Update;

namespace BLL
{
    /// <summary>
    /// Facade de instalación segura. Sin WinForms. Invocada desde UpdateManager.exe.
    /// </summary>
    public static class UpdateInstallationBLL
    {
        public static UpdateInstallationResult Install(
            UpdateInstallRequest request,
            UpdateInstallerHooks? hooks = null) =>
            new UpdateInstaller(hooks).Install(request);
    }
}
