using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PhpMyAdminComponent : ComponentBase
    {
        public override string Name => "phpmyadmin";
        public override string ToolDir => DevStackConfig.pmaDir;

        public override Task PostInstall(string version, string targetDir)
        {
            string pmaVersionDir = System.IO.Path.Combine(DevStackConfig.pmaDir, $"phpmyadmin-{version}");
            if (System.IO.Directory.Exists(pmaVersionDir))
            {
                Console.WriteLine($"phpMyAdmin {version} já está instalado.");
                return Task.CompletedTask;
            }
            // InstallGenericTool already downloaded and extracted the archive into targetDir.
            Console.WriteLine($"phpMyAdmin {version} post-install completed in {targetDir}.");
            DevStackConfig.WriteLog($"phpMyAdmin {version} post-install in {targetDir}");
            return Task.CompletedTask;
        }
    }
}
