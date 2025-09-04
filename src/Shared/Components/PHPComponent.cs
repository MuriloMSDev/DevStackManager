using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PHPComponent : ComponentBase
    {
        public override string Name => "php";
        public override string ToolDir => DevStackConfig.phpDir;
        public override bool IsService => true;
        public override bool IsExecutable => true;
        public override string? ExecutablePattern => "php.exe";
        public override string? ServicePattern => "php-cgi.exe";
        public override string? CreateBinShortcut => "php-{version}.exe";

        public override async Task PostInstall(string version, string targetDir)
        {
            string phpDir = System.IO.Path.Combine(DevStackConfig.phpDir, $"php-{version}");

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

            await Task.CompletedTask;
        }
    }
}
