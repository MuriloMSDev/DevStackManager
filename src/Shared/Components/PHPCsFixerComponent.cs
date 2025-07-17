using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PHPCsFixerComponent : ComponentBase
    {
        public override string Name => "phpcsfixer";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string phpCsFixerSubDir = $"phpcsfixer-{version}";
            string toolDir = System.IO.Path.Combine(DevStackConfig.phpcsfixerDir, phpCsFixerSubDir);
            if (System.IO.Directory.Exists(toolDir))
            {
                Console.WriteLine($"PHP CS Fixer {version} já está instalado.");
                return;
            }
            Console.WriteLine($"Baixando PHP CS Fixer {version}...");
            System.IO.Directory.CreateDirectory(toolDir);
            string url = GetUrlForVersion(version);
            string pharPath = System.IO.Path.Combine(toolDir, $"php-cs-fixer-{version}.phar");
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fileStream = System.IO.File.Create(pharPath);
            await response.Content.CopyToAsync(fileStream);
            Console.WriteLine($"PHP CS Fixer {version} instalado em {toolDir}");
            DevStackConfig.WriteLog($"PHP CS Fixer {version} instalado em {toolDir}");
        }
    }
}
