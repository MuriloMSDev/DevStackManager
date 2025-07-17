using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class ComposerComponent : ComponentBase
    {
        public override string Name => "composer";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string composerSubDir = $"composer-{version}";
            string composerPhar = $"composer-{version}.phar";
            string composerPharPath = System.IO.Path.Combine(DevStackConfig.composerDir, composerSubDir);
            if (System.IO.Directory.Exists(composerPharPath))
            {
                Console.WriteLine($"Composer {version} já está instalado.");
                return;
            }
            Console.WriteLine($"Baixando Composer {version}...");
            System.IO.Directory.CreateDirectory(composerPharPath);
            string composerUrl = GetUrlForVersion(version);
            string pharPath = System.IO.Path.Combine(composerPharPath, composerPhar);
            using var response = await httpClient.GetAsync(composerUrl);
            response.EnsureSuccessStatusCode();
            using (var fileStream = System.IO.File.Create(pharPath))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            Console.WriteLine($"Composer {version} instalado.");
            DevStackConfig.WriteLog($"Composer {version} instalado em {composerPharPath}");
        }
    }
}
