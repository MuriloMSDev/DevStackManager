using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// PHP runtime component manager for DevStack.
    /// Handles PHP installation, configuration (php.ini), extension management,
    /// and multi-worker FastCGI service support for web development.
    /// </summary>
    public class PHPComponent : ComponentBase
    {
        /// <summary>
        /// Embedded resource name for PHP configuration file.
        /// </summary>
        private const string PHP_INI_RESOURCE_NAME = "php.ini";
        
        /// <summary>
        /// File name for PHP configuration file.
        /// </summary>
        private const string PHP_INI_FILE_NAME = "php.ini";
        
        /// <summary>
        /// Directory name for PHP extensions.
        /// </summary>
        private const string PHP_EXTENSIONS_DIR = "ext";
        
        /// <summary>
        /// File pattern for matching PHP extension DLL files.
        /// </summary>
        private const string PHP_EXTENSION_PATTERN = "*.dll";
        
        /// <summary>
        /// Prefix used for PHP extension file names.
        /// </summary>
        private const string PHP_EXTENSION_PREFIX = "php_";

        /// <summary>
        /// Array of essential PHP extensions to enable by default in php.ini.
        /// </summary>
        private static readonly string[] EssentialExtensions = 
        {
            "mbstring", "intl", "pdo", "pdo_mysql", "pdo_pgsql", "openssl",
            "json", "fileinfo", "curl", "gd", "gd2", "zip", "xml", "xmlrpc"
        };

        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "php";
        
        /// <summary>
        /// Gets the display label for PHP.
        /// </summary>
        public override string Label => "PHP";
        
        /// <summary>
        /// Gets the PHP installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.phpDir;
        
        /// <summary>
        /// Gets whether PHP runs as a service (FastCGI).
        /// </summary>
        public override bool IsService => true;
        
        /// <summary>
        /// Gets whether PHP is executable from command line.
        /// </summary>
        public override bool IsExecutable => true;
        
        /// <summary>
        /// Gets the PHP CLI executable pattern.
        /// </summary>
        public override string? ExecutablePattern => "php.exe";
        
        /// <summary>
        /// Gets the PHP FastCGI service executable pattern.
        /// </summary>
        public override string? ServicePattern => "php-cgi.exe";
        
        /// <summary>
        /// Gets the shortcut name pattern for PHP binaries.
        /// </summary>
        public override string? CreateBinShortcut => "php-{version}.exe";
        
        /// <summary>
        /// Gets the maximum number of FastCGI worker processes (default 6).
        /// </summary>
        public override int? MaxWorkers => 6;

        /// <summary>
        /// Executes post-installation configuration for PHP.
        /// Copies php.ini configuration and enables essential extensions for CakePHP and Laravel.
        /// </summary>
        /// <param name="version">PHP version being installed.</param>
        /// <param name="targetDir">Installation target directory.</param>
        public override async Task PostInstall(string version, string targetDir)
        {
            var phpDir = Path.Combine(DevStackConfig.phpDir, $"php-{version}");
            await CopyPhpIniConfiguration(phpDir);
            AddEssentialExtensionsToIni(phpDir);
        }

        /// <summary>
        /// Copies embedded php.ini configuration to PHP installation directory.
        /// </summary>
        /// <param name="phpDir">PHP installation directory.</param>
        private async Task CopyPhpIniConfiguration(string phpDir)
        {
            var phpIniDestination = Path.Combine(phpDir, PHP_INI_FILE_NAME);
            var assembly = typeof(PHPComponent).Assembly;
            
            var resourceName = FindPhpIniResourceName(assembly);
            if (string.IsNullOrEmpty(resourceName))
            {
                Console.WriteLine("php.ini file not found. php.ini file not copied.");
                return;
            }

            await CopyEmbeddedResourceToFile(assembly, resourceName, phpIniDestination);
            Console.WriteLine($"php.ini file copied to {phpDir}");
        }

        /// <summary>
        /// Finds the php.ini embedded resource name in the assembly.
        /// </summary>
        /// <param name="assembly">Assembly to search for resources.</param>
        /// <returns>Resource name if found, null otherwise.</returns>
        private string? FindPhpIniResourceName(System.Reflection.Assembly assembly)
        {
            return assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.Contains(PHP_INI_RESOURCE_NAME));
        }

        /// <summary>
        /// Copies an embedded resource stream to a file on disk.
        /// </summary>
        /// <param name="assembly">Assembly containing the resource.</param>
        /// <param name="resourceName">Full resource name.</param>
        /// <param name="destinationPath">Destination file path.</param>
        /// <exception cref="InvalidOperationException">Thrown if resource not found.</exception>
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

        /// <summary>
        /// Adds essential PHP extensions to php.ini configuration.
        /// Scans the ext directory and enables extensions required for CakePHP and Laravel.
        /// </summary>
        /// <param name="phpDir">PHP installation directory.</param>
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
            Console.WriteLine("Essential extensions block added to php.ini");
        }

        /// <summary>
        /// Gets available essential extensions from the extensions directory.
        /// Filters DLL files to match the EssentialExtensions list.
        /// </summary>
        /// <param name="extensionsDir">PHP extensions directory path.</param>
        /// <returns>Array of extension=name directives for php.ini.</returns>
        private string[] GetAvailableExtensions(string extensionsDir)
        {
            return Directory.GetFiles(extensionsDir, PHP_EXTENSION_PATTERN)
                .Select(f => Path.GetFileNameWithoutExtension(f).Replace(PHP_EXTENSION_PREFIX, string.Empty))
                .Where(name => EssentialExtensions.Contains(name))
                .Select(name => $"extension={name}")
                .ToArray();
        }

        /// <summary>
        /// Appends extension directives to php.ini file.
        /// </summary>
        /// <param name="phpIniPath">Path to php.ini file.</param>
        /// <param name="extensions">Array of extension directives.</param>
        private void AppendExtensionsToIni(string phpIniPath, string[] extensions)
        {
            var extensionsBlock = BuildExtensionsBlock(extensions);
            File.AppendAllText(phpIniPath, extensionsBlock, Encoding.UTF8);
        }

        /// <summary>
        /// Builds the extensions configuration block for php.ini.
        /// </summary>
        /// <param name="extensions">Array of extension directives.</param>
        /// <returns>Formatted extensions block with header comment.</returns>
        private string BuildExtensionsBlock(string[] extensions)
        {
            var header = "\n; Essential extensions for CakePHP and Laravel:\n";
            return header + string.Join("\n", extensions);
        }
    }
}
