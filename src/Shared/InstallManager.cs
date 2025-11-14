using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Manages component installation and Nginx site configuration.
    /// Handles component installation orchestration and hosts file management.
    /// </summary>
    public static class InstallManager
    {
        /// <summary>
        /// Relative path to Windows hosts file for domain configuration.
        /// </summary>
        private const string HOSTS_FILE_RELATIVE_PATH = @"System32\drivers\etc\hosts";

        /// <summary>
        /// Installs a component with an optional specified version.
        /// </summary>
        /// <param name="args">Command arguments: component name and optional version.</param>
        public static async Task InstallCommands(string[] args)
        {
            if (!ValidateInstallArgs(args, out var component, out var version))
            {
                return;
            }

            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp == null)
            {
                LogUnknownComponent(component);
                return;
            }

            await comp.Install(version);
        }

        /// <summary>
        /// Creates an Nginx site configuration file and adds the domain to the hosts file.
        /// </summary>
        /// <param name="domain">Domain name for the site.</param>
        /// <param name="root">Document root directory path.</param>
        /// <param name="phpUpstream">PHP FastCGI upstream address (e.g., 127.0.0.1:9000).</param>
        /// <param name="nginxVersion">Nginx version to create the configuration for.</param>
        public static void CreateNginxSiteConfig(
            string domain, 
            string root, 
            string phpUpstream, 
            string nginxVersion)
        {
            ValidateNginxInstallation(nginxVersion);

            var nginxSitesDir = GetNginxSitesDirectory(nginxVersion);
            EnsureDirectoryExists(nginxSitesDir);

            var confPath = CreateSiteConfigFile(nginxSitesDir, domain, root, phpUpstream);
            Console.WriteLine($"File {confPath} created/configured successfully!");

            AddDomainToHostsFile(domain);
        }

        /// <summary>
        /// Validates installation command arguments.
        /// </summary>
        /// <param name="args">Command arguments array.</param>
        /// <param name="component">Extracted component name.</param>
        /// <param name="version">Optional extracted version string.</param>
        /// <returns>True if arguments are valid, false otherwise.</returns>
        private static bool ValidateInstallArgs(
            string[] args, 
            out string component, 
            out string? version)
        {
            component = string.Empty;
            version = null;

            if (args.Length == 0)
            {
                Console.WriteLine("No component specified for installation.");
                return false;
            }

            component = args[0];
            version = args.Length > 1 ? args[1] : null;
            return true;
        }

        /// <summary>
        /// Logs an unknown component error message.
        /// </summary>
        /// <param name="component">The unknown component name.</param>
        private static void LogUnknownComponent(string component)
        {
            Console.WriteLine($"Unknown component: {component}");
        }

        /// <summary>
        /// Validates that the specified Nginx version is installed.
        /// </summary>
        /// <param name="nginxVersion">Nginx version to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown when the Nginx version is not installed.</exception>
        private static void ValidateNginxInstallation(string nginxVersion)
        {
            var nginxVersionDir = Path.Combine(DevStackConfig.nginxDir, $"nginx-{nginxVersion}");
            
            if (!Directory.Exists(nginxVersionDir))
            {
                throw new InvalidOperationException(
                    $"Nginx version ({nginxVersion}) is not installed at {nginxVersionDir}.");
            }
        }

        /// <summary>
        /// Gets the sites directory path for a specific Nginx version.
        /// </summary>
        /// <param name="nginxVersion">Nginx version.</param>
        /// <returns>The absolute path to the sites directory.</returns>
        private static string GetNginxSitesDirectory(string nginxVersion)
        {
            var nginxVersionDir = Path.Combine(DevStackConfig.nginxDir, $"nginx-{nginxVersion}");
            return Path.Combine(nginxVersionDir, DevStackConfig.nginxSitesDir);
        }

        /// <summary>
        /// Ensures a directory exists, creating it if necessary.
        /// </summary>
        /// <param name="directory">Directory path to ensure exists.</param>
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Creates an Nginx site configuration file from a template.
        /// </summary>
        /// <param name="nginxSitesDir">Directory to create the configuration file in.</param>
        /// <param name="domain">Domain name.</param>
        /// <param name="root">Document root path.</param>
        /// <param name="phpUpstream">PHP FastCGI upstream address.</param>
        /// <returns>The path to the created configuration file.</returns>
        private static string CreateSiteConfigFile(
            string nginxSitesDir, 
            string domain, 
            string root, 
            string phpUpstream)
        {
            var confPath = Path.Combine(nginxSitesDir, $"{domain}.conf");
            var template = BuildNginxConfigTemplate(domain, root, phpUpstream);
            
            File.WriteAllText(confPath, template, new UTF8Encoding(false));
            
            return confPath;
        }

        /// <summary>
        /// Builds an Nginx server configuration template with PHP support.
        /// </summary>
        /// <param name="domain">Domain name.</param>
        /// <param name="root">Document root path.</param>
        /// <param name="phpUpstream">PHP FastCGI upstream address.</param>
        /// <returns>The Nginx configuration file content.</returns>
        private static string BuildNginxConfigTemplate(string domain, string root, string phpUpstream)
        {
            return $@"server {{

    listen 80;
    listen [::]:80;

    server_name {domain};
    root {root};
    index index.php index.html index.htm;

    location / {{
         try_files $uri $uri/ /index.php$is_args$args;
    }}

    location ~ \.php$ {{
        try_files $uri /index.php =404;
        fastcgi_pass {phpUpstream};
        fastcgi_index index.php;
        fastcgi_buffers 16 16k;
        fastcgi_buffer_size 32k;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        fastcgi_read_timeout 600;
        include fastcgi_params;
    }}

    location ~ /\.ht {{
        deny all;
    }}

    location /.well-known/acme-challenge/ {{
        root /var/www/letsencrypt/;
        log_not_found off;
    }}

    location /api {{
        rewrite ^/api/(\w+).*$ /api.php?type=$1 last;
    }}

    error_log logs\{domain}_error.log;
    access_log logs\{domain}_access.log;
}}";
        }

        /// <summary>
        /// Adds a domain entry to the Windows hosts file mapping to 127.0.0.1.
        /// </summary>
        /// <param name="domain">Domain name to add.</param>
        private static void AddDomainToHostsFile(string domain)
        {
            var hostsPath = GetHostsFilePath();
            var entry = $"127.0.0.1\t{domain}";

            try
            {
                if (IsDomainAlreadyInHosts(hostsPath, entry))
                {
                    Console.WriteLine($"{domain} is already present in the hosts file.");
                    return;
                }

                AppendToHostsFile(hostsPath, entry);
                Console.WriteLine($"Added {domain} to the hosts file.");
            }
            catch
            {
                Console.WriteLine("Error modifying hosts file. Run as administrator.");
            }
        }

        /// <summary>
        /// Gets the absolute path to the Windows hosts file.
        /// </summary>
        /// <returns>The hosts file path.</returns>
        /// <exception cref="InvalidOperationException">Thrown when SystemRoot environment variable is not found.</exception>
        private static string GetHostsFilePath()
        {
            var systemRoot = Environment.GetEnvironmentVariable("SystemRoot") 
                ?? throw new InvalidOperationException("SystemRoot environment variable not found.");
            
            return Path.Combine(systemRoot, HOSTS_FILE_RELATIVE_PATH);
        }

        /// <summary>
        /// Checks if a domain entry already exists in the hosts file.
        /// </summary>
        /// <param name="hostsPath">Path to the hosts file.</param>
        /// <param name="entry">The hosts file entry to check for.</param>
        /// <returns>True if the entry exists, false otherwise.</returns>
        private static bool IsDomainAlreadyInHosts(string hostsPath, string entry)
        {
            if (!File.Exists(hostsPath))
            {
                return false;
            }

            var hostsContent = File.ReadAllLines(hostsPath);
            return hostsContent.Contains(entry);
        }

        /// <summary>
        /// Appends an entry to the Windows hosts file.
        /// </summary>
        /// <param name="hostsPath">Path to the hosts file.</param>
        /// <param name="entry">The entry to append.</param>
        private static void AppendToHostsFile(string hostsPath, string entry)
        {
            File.AppendAllText(hostsPath, Environment.NewLine + entry, Encoding.UTF8);
        }
    }
}
