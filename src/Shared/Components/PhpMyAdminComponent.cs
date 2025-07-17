using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PhpMyAdminComponent : ComponentBase
    {
        public override string Name => "phpmyadmin";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string pmaVersionDir = System.IO.Path.Combine(DevStackConfig.pmaDir, $"phpmyadmin-{version}");
            if (System.IO.Directory.Exists(pmaVersionDir))
            {
                Console.WriteLine($"phpMyAdmin {version} já está instalado.");
                return;
            }
            Console.WriteLine($"Baixando phpMyAdmin {version}...");
            string pmaZip = System.IO.Path.Combine(DevStackConfig.baseDir, $"phpmyadmin-{version}-all-languages.zip");
            string pmaUrl = GetUrlForVersion(version);
            using var response = await httpClient.GetAsync(pmaUrl);
            response.EnsureSuccessStatusCode();
            using (var fileStream = System.IO.File.Create(pmaZip))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            System.IO.Compression.ZipFile.ExtractToDirectory(pmaZip, DevStackConfig.baseDir, true);
            string extractedDir = System.IO.Path.Combine(DevStackConfig.baseDir, $"phpMyAdmin-{version}-all-languages");
            if (System.IO.Directory.Exists(extractedDir))
            {
                System.IO.Directory.Move(extractedDir, pmaVersionDir);
            }
            // Try to delete with retry mechanism
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    System.IO.File.Delete(pmaZip);
                    break;
                }
                catch (System.IO.IOException) when (attempt < 4)
                {
                    await Task.Delay(200);
                }
            }
            Console.WriteLine($"phpMyAdmin {version} instalado em {pmaVersionDir}.");
            DevStackConfig.WriteLog($"phpMyAdmin {version} instalado em {pmaVersionDir}");
        }
    }
}
