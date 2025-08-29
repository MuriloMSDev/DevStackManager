using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PHPCsFixerComponent : ComponentBase
    {
        public override string Name => "phpcsfixer";
        public override string ToolDir => DevStackConfig.phpcsfixerDir;
        public override bool IsArchive => false;

        public override Task PostInstall(string version, string targetDir)
        {
            string phpCsFixerSubDir = $"phpcsfixer-{version}";
            string toolDir = System.IO.Path.Combine(DevStackConfig.phpcsfixerDir, phpCsFixerSubDir);
            if (System.IO.Directory.Exists(toolDir))
            {
                Console.WriteLine($"PHP CS Fixer {version} já está instalado.");
                return Task.CompletedTask;
            }
            System.IO.Directory.CreateDirectory(toolDir);
            // No remote download here; InstallGenericTool handled the fetch.
            Console.WriteLine($"PHP CS Fixer {version} post-install in {toolDir}");
            DevStackConfig.WriteLog($"PHP CS Fixer {version} post-install in {toolDir}");
            return Task.CompletedTask;
        }
    }
}
