using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PgSQLComponent : ComponentBase
    {
        public override string Name => "pgsql";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string subDir = $"pgsql-{version}";
            string zipUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.pgsqlDir, version, zipUrl, subDir, System.IO.Path.Combine("bin", "psql.exe"), "psql");
            Console.WriteLine($"PostgreSQL {version} instalado.");
        }
    }
}
