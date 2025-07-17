using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class OpenSSLComponent : ComponentBase
    {
        public override string Name => "openssl";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string arch = "x64";
            string archPrefix = arch == "x86" ? "Win32OpenSSL" : "Win64OpenSSL";
            string subDir = $"openssl-{version}";
            string versionUnderscore = version.Replace(".", "_");
            string installerName = $"{archPrefix}-{versionUnderscore}.exe";
            string installerUrl = GetUrlForVersion(version);
            string installDir = System.IO.Path.Combine("C:", "devstack", "openssl", subDir);
            string installerPath = System.IO.Path.Combine(DevStackConfig.tmpDir, installerName);
            if (!System.IO.Directory.Exists(DevStackConfig.tmpDir))
            {
                System.IO.Directory.CreateDirectory(DevStackConfig.tmpDir);
            }
            Console.WriteLine($"Baixando instalador do OpenSSL {version} ({arch})...");
            using var response = await httpClient.GetAsync(installerUrl);
            response.EnsureSuccessStatusCode();
            await using (var fileStream = System.IO.File.Create(installerPath))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            Console.WriteLine($"Executando instalador do OpenSSL {version} ({arch})...");
            if (!System.IO.Directory.Exists(installDir))
            {
                System.IO.Directory.CreateDirectory(installDir);
            }
            // Executa o instalador em modo oculto usando ProcessManager.ExecuteProcess
            string psCommand = $"Start-Process -FilePath \"{installerPath}\" -ArgumentList '/VERYSILENT /DIR=\"{installDir}\"' -WindowStyle Hidden -Verb runAs -Wait";
            ProcessManager.ExecuteProcess("powershell.exe", $"-Command \"{psCommand}\"", DevStackConfig.tmpDir);
            System.IO.File.Delete(installerPath);
            Console.WriteLine($"OpenSSL {version} ({arch}) instalado via instalador em {installDir}");
        }
    }
}
