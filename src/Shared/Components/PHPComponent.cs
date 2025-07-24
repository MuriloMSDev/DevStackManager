using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PHPComponent : ComponentBase
    {
        public override string Name => "php";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string phpUrl = GetUrlForVersion(version);
            string subDir = $"php-{version}";
            await InstallGenericTool(DevStackConfig.phpDir, version, phpUrl, subDir, "php.exe", "php");
            // Rename php-cgi.exe
            string phpDir = System.IO.Path.Combine(DevStackConfig.phpDir, subDir);
            RenameMainExe(phpDir, "php-cgi.exe", version, "php-cgi");

            // Copia o php.ini do EmbeddedResource
            string phpIniDst = System.IO.Path.Combine(phpDir, "php.ini");
            var assembly = typeof(PHPComponent).Assembly;
            string? resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.Contains("php.ini"));
            if (!string.IsNullOrEmpty(resourceName))
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new System.IO.StreamReader(stream!))
                {
                    var iniContent = reader.ReadToEnd();
                    System.IO.File.WriteAllText(phpIniDst, iniContent, System.Text.Encoding.UTF8);
                }
                Console.WriteLine($"Arquivo php.ini copiado para {phpDir}");

                // Add essential extensions
                string extDir = System.IO.Path.Combine(phpDir, "ext");
                if (System.IO.Directory.Exists(extDir))
                {
                    string[] acceptExt = {
                        "mbstring", "intl", "pdo", "pdo_mysql", "pdo_pgsql", "openssl",
                        "json", "fileinfo", "curl", "gd", "gd2", "zip", "xml", "xmlrpc"
                    };

                    var extensions = System.IO.Directory.GetFiles(extDir, "*.dll")
                        .Select(f => System.IO.Path.GetFileNameWithoutExtension(f).Replace("php_", ""))
                        .Where(name => acceptExt.Contains(name))
                        .Select(name => $"extension={name}")
                        .ToArray();

                    if (extensions.Any())
                    {
                        System.IO.File.AppendAllText(phpIniDst, $"\n; Extensões essenciais para CakePHP e Laravel:\n{string.Join("\n", extensions)}", System.Text.Encoding.UTF8);
                        Console.WriteLine("Bloco de extensões essenciais adicionado ao php.ini");
                    }
                }
            }
            else
            {
                Console.WriteLine("Arquivo php.ini não encontrado. Arquivo php.ini não copiado.");
            }
        }
    }
}
