using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class AdminerComponent : ComponentBase
    {
        public override string Name => "adminer";
        public override string ToolDir => DevStackConfig.adminerDir;
        public override bool IsArchive => false;

        public override Task PostInstall(string version, string targetDir)
        {
            // No remote download in PostInstall; InstallGenericTool already downloaded the single file into targetDir.
            Console.WriteLine($"Adminer {version} post-install completed in {DevStackConfig.adminerDir}.");
            return Task.CompletedTask;
        }
    }
}
