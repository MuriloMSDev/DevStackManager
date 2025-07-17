using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class WPCLIComponent : ComponentBase
    {
        public override string Name => "wpcli";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string url = GetUrlForVersion(version);
            if (!System.IO.Directory.Exists(DevStackConfig.wpcliDir))
            {
                System.IO.Directory.CreateDirectory(DevStackConfig.wpcliDir);
            }
            string pharPath = System.IO.Path.Combine(DevStackConfig.wpcliDir, $"wp-cli-{version}.phar");
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fileStream = System.IO.File.Create(pharPath);
            await response.Content.CopyToAsync(fileStream);
            fileStream.Close();
            string batPath = System.IO.Path.Combine(DevStackConfig.wpcliDir, "wp.bat");
            System.IO.File.WriteAllText(batPath, $"@echo off\nphp %~dp0wp-cli-{version}.phar %*", System.Text.Encoding.UTF8);
            Console.WriteLine($"WP-CLI {version} instalado em {DevStackConfig.wpcliDir}. Use 'wp' no terminal.");
        }
    }
}
