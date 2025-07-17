using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class MySQLComponent : ComponentBase
    {
        public override string Name => "mysql";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string subDir = $"mysql-{version}";
            string zipUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.mysqlDir, version, zipUrl, subDir, System.IO.Path.Combine("bin", "mysqld.exe"), "mysqld");
            Console.WriteLine($"MySQL {version} instalado.");
        }
    }
}
