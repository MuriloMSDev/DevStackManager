using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class NginxComponent : ComponentBase
    {
        public override string Name => "nginx";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            var subDir = $"nginx-{version}";
            var zipUrl = GetUrlForVersion(version);
            Console.WriteLine(DevStackConfig.nginxDir);
            await InstallGenericTool(DevStackConfig.nginxDir, version, zipUrl, subDir, "nginx.exe", "nginx");
        }
    }
}
