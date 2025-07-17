using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class ElasticsearchComponent : ComponentBase
    {
        public override string Name => "elasticsearch";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string subDir = $"elasticsearch-{version}";
            string zipUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.elasticDir, version, zipUrl, subDir, System.IO.Path.Combine("bin", "elasticsearch.bat"), "elasticsearch");
            Console.WriteLine($"Elasticsearch {version} instalado.");
        }
    }
}
