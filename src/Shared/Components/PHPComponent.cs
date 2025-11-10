using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PHPComponent : ComponentBase
    {
        private const string PHP_INI_RESOURCE_NAME = "php.ini";
        private const string PHP_INI_FILE_NAME = "php.ini";
        private const string PHP_EXTENSIONS_DIR = "ext";
        private const string PHP_EXTENSION_PATTERN = "*.dll";
        private const string PHP_EXTENSION_PREFIX = "php_";

        private static readonly string[] EssentialExtensions = 
        {
            "mbstring", "intl", "pdo", "pdo_mysql", "pdo_pgsql", "openssl",
            "json", "fileinfo", "curl", "gd", "gd2", "zip", "xml", "xmlrpc"
        };

        public override string Name => "php";
        public override string Label => "PHP";
        public override string ToolDir => DevStackConfig.phpDir;
        public override bool IsService => true;
        public override bool IsExecutable => true;
        public override string? ExecutablePattern => "php.exe";
        public override string? ServicePattern => "php-cgi.exe";
        public override string? CreateBinShortcut => "php-{version}.exe";
        public override int? MaxWorkers => 6;

        public override async Task PostInstall(string version, string targetDir)
        {
            var phpDir = Path.Combine(DevStackConfig.phpDir, $"php-{version}");
            await CopyPhpIniConfiguration(phpDir);
            AddEssentialExtensionsToIni(phpDir);
        }

        private async Task CopyPhpIniConfiguration(string phpDir)
        {
            var phpIniDestination = Path.Combine(phpDir, PHP_INI_FILE_NAME);
            var assembly = typeof(PHPComponent).Assembly;
            
            var resourceName = FindPhpIniResourceName(assembly);
            if (string.IsNullOrEmpty(resourceName))
            {
                Console.WriteLine("Arquivo php.ini não encontrado. Arquivo php.ini não copiado.");
                return;
            }

            await CopyEmbeddedResourceToFile(assembly, resourceName, phpIniDestination);
            Console.WriteLine($"Arquivo php.ini copiado para {phpDir}");
        }

        private string? FindPhpIniResourceName(System.Reflection.Assembly assembly)
        {
            return assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.Contains(PHP_INI_RESOURCE_NAME));
        }

        private async Task CopyEmbeddedResourceToFile(
            System.Reflection.Assembly assembly, 
            string resourceName, 
            string destinationPath)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException($"Resource {resourceName} not found.");
            }

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            await File.WriteAllTextAsync(destinationPath, content, Encoding.UTF8);
        }

        private void AddEssentialExtensionsToIni(string phpDir)
        {
            var phpIniPath = Path.Combine(phpDir, PHP_INI_FILE_NAME);
            var extensionsDir = Path.Combine(phpDir, PHP_EXTENSIONS_DIR);

            if (!Directory.Exists(extensionsDir))
            {
                return;
            }

            var availableExtensions = GetAvailableExtensions(extensionsDir);
            if (!availableExtensions.Any())
            {
                return;
            }

            AppendExtensionsToIni(phpIniPath, availableExtensions);
            Console.WriteLine("Bloco de extensões essenciais adicionado ao php.ini");
        }

        private string[] GetAvailableExtensions(string extensionsDir)
        {
            return Directory.GetFiles(extensionsDir, PHP_EXTENSION_PATTERN)
                .Select(f => Path.GetFileNameWithoutExtension(f).Replace(PHP_EXTENSION_PREFIX, string.Empty))
                .Where(name => EssentialExtensions.Contains(name))
                .Select(name => $"extension={name}")
                .ToArray();
        }

        private void AppendExtensionsToIni(string phpIniPath, string[] extensions)
        {
            var extensionsBlock = BuildExtensionsBlock(extensions);
            File.AppendAllText(phpIniPath, extensionsBlock, Encoding.UTF8);
        }

        private string BuildExtensionsBlock(string[] extensions)
        {
            var header = "\n; Extensões essenciais para CakePHP e Laravel:\n";
            return header + string.Join("\n", extensions);
        }
    }
}
