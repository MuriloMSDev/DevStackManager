using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class DBeaverComponent : ComponentBase
    {
        public override string Name => "dbeaver";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string subDir = $"dbeaver-{version}";
            string zipUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.dbeaverDir, version, zipUrl, subDir, "dbeaver.exe", "dbeaver");
            Console.WriteLine($"DBeaver {version} instalado.");
        }
    }
}
