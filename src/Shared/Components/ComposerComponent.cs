using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class ComposerComponent : ComponentBase
    {
        public override string Name => "composer";
        public override string ToolDir => DevStackConfig.composerDir;
        public override bool IsArchive => false;

    public override Task PostInstall(string version, string targetDir)
        {
            string composerSubDir = $"composer-{version}";
            string composerPhar = $"composer-{version}.phar";
            string composerPharPath = System.IO.Path.Combine(DevStackConfig.composerDir, composerSubDir);
            if (System.IO.Directory.Exists(composerPharPath))
            {
                Console.WriteLine($"Composer {version} já está instalado.");
                return Task.CompletedTask;
            }
            System.IO.Directory.CreateDirectory(composerPharPath);
            string composerUrl = GetUrlForVersion(version);
            string pharPath = System.IO.Path.Combine(composerPharPath, composerPhar);
                // No remote download in PostInstall; InstallGenericTool already downloaded the file into targetDir.
                Console.WriteLine($"Composer {version} post-install actions completed.");
                DevStackConfig.WriteLog($"Composer {version} post-install in {composerPharPath}");
                return Task.CompletedTask;
        }
    }
}
