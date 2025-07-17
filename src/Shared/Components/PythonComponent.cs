using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PythonComponent : ComponentBase
    {
        public override string Name => "python";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string pySubDir = $"python-{version}";
            string pyUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.pythonDir, version, pyUrl, pySubDir, "python.exe", "python");
            Console.WriteLine($"Python {version} instalado.");
        }
    }
}
