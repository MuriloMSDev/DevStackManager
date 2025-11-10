using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStackManager
{
    public static class InstallManager
    {
        private const string HOSTS_FILE_RELATIVE_PATH = @"System32\drivers\etc\hosts";

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
            Console.WriteLine($"Arquivo {confPath} criado/configurado com sucesso!");

            AddDomainToHostsFile(domain);
        }

        private static bool ValidateInstallArgs(
            string[] args, 
            out string component, 
            out string? version)
        {
            component = string.Empty;
            version = null;

            if (args.Length == 0)
            {
                Console.WriteLine("Nenhum componente especificado para instalar.");
                return false;
            }

            component = args[0];
            version = args.Length > 1 ? args[1] : null;
            return true;
        }

        private static void LogUnknownComponent(string component)
        {
            Console.WriteLine($"Componente desconhecido: {component}");
        }

        private static void ValidateNginxInstallation(string nginxVersion)
        {
            var nginxVersionDir = Path.Combine(DevStackConfig.nginxDir, $"nginx-{nginxVersion}");
            
            if (!Directory.Exists(nginxVersionDir))
            {
                throw new InvalidOperationException(
                    $"A versão do Nginx ({nginxVersion}) não está instalada em {nginxVersionDir}.");
            }
        }

        private static string GetNginxSitesDirectory(string nginxVersion)
        {
            var nginxVersionDir = Path.Combine(DevStackConfig.nginxDir, $"nginx-{nginxVersion}");
            return Path.Combine(nginxVersionDir, DevStackConfig.nginxSitesDir);
        }

        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

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

        private static void AddDomainToHostsFile(string domain)
        {
            var hostsPath = GetHostsFilePath();
            var entry = $"127.0.0.1\t{domain}";

            try
            {
                if (IsDomainAlreadyInHosts(hostsPath, entry))
                {
                    Console.WriteLine($"{domain} já está presente no arquivo hosts.");
                    return;
                }

                AppendToHostsFile(hostsPath, entry);
                Console.WriteLine($"Adicionado {domain} ao arquivo hosts.");
            }
            catch
            {
                Console.WriteLine("Erro ao modificar arquivo hosts. Execute como administrador.");
            }
        }

        private static string GetHostsFilePath()
        {
            var systemRoot = Environment.GetEnvironmentVariable("SystemRoot") 
                ?? throw new InvalidOperationException("SystemRoot environment variable not found.");
            
            return Path.Combine(systemRoot, HOSTS_FILE_RELATIVE_PATH);
        }

        private static bool IsDomainAlreadyInHosts(string hostsPath, string entry)
        {
            if (!File.Exists(hostsPath))
            {
                return false;
            }

            var hostsContent = File.ReadAllLines(hostsPath);
            return hostsContent.Contains(entry);
        }

        private static void AppendToHostsFile(string hostsPath, string entry)
        {
            File.AppendAllText(hostsPath, Environment.NewLine + entry, Encoding.UTF8);
        }
    }
}
