using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class GitComponent : ComponentBase
    {
        public override string Name => "git";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string gitSubDir = $"git-{version}";
            string gitDirFull = System.IO.Path.Combine(DevStackConfig.baseDir, gitSubDir);
            if (System.IO.Directory.Exists(gitDirFull))
            {
                Console.WriteLine($"Git {version} já está instalado.");
                return;
            }
            Console.WriteLine($"Baixando Git {version}...");
            string gitUrl = GetUrlForVersion(version);
            string git7zExe = System.IO.Path.Combine(DevStackConfig.baseDir, $"PortableGit-{version}-64-bit.7z.exe");
            using var response = await httpClient.GetAsync(gitUrl);
            response.EnsureSuccessStatusCode();
            using (var fileStream = System.IO.File.Create(git7zExe))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            System.IO.Directory.CreateDirectory(gitDirFull);
            Console.WriteLine($"Extraindo Git {version}...");
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = git7zExe,
                Arguments = $"-y -o\"{gitDirFull}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            System.IO.File.Delete(git7zExe);
            Console.WriteLine($"Git {version} instalado em {gitDirFull}.");
            DevStackConfig.WriteLog($"Git {version} instalado em {gitDirFull}");
        }
    }
}
