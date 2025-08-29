using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Compression;
using System;
using System.IO;

namespace DevStackManager.Components
{
    public class GitComponent : ComponentBase
    {
        public override string Name => "git";
        public override string ToolDir => DevStackConfig.gitDir;

        public override Task PostInstall(string version, string targetDir)
        {
            // Log install location
            DevStackConfig.WriteLog($"Git {version} instalado em {targetDir}");
            Console.WriteLine($"Git {version} instalado em {targetDir}");
            return Task.CompletedTask;
        }
    }
}
