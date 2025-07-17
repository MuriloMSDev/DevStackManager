using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class MongoDBComponent : ComponentBase
    {
        public override string Name => "mongodb";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string subDir = $"mongodb-{version}";
            string zipUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.mongoDir, version, zipUrl, subDir, System.IO.Path.Combine("bin", "mongod.exe"), "mongod");
            Console.WriteLine($"MongoDB {version} instalado.");
        }
    }
}
