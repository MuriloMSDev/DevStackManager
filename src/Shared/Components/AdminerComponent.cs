using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class AdminerComponent : ComponentBase
    {
        public override string Name => "adminer";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            if (!System.IO.Directory.Exists(DevStackConfig.adminerDir))
            {
                System.IO.Directory.CreateDirectory(DevStackConfig.adminerDir);
            }
            string url = GetUrlForVersion(version);
            string phpPath = System.IO.Path.Combine(DevStackConfig.adminerDir, "adminer.php");
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fileStream = System.IO.File.Create(phpPath);
            await response.Content.CopyToAsync(fileStream);
            Console.WriteLine($"Adminer {version} instalado em {DevStackConfig.adminerDir}. Abra o arquivo PHP no navegador.");
        }
    }
}
