using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class GoComponent : ComponentBase
    {
        public override string Name => "go";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string subDir = $"go-{version}";
            string zipUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.goDir, version, zipUrl, subDir, System.IO.Path.Combine("bin", "go.exe"), "go");
            Console.WriteLine($"Go {version} instalado.");
        }
    }
}
