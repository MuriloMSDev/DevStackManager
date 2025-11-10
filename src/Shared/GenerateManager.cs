using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    public static class GenerateManager
    {
        private const string SSL_CONFIG_SUBDIR = "configs/nginx/ssl";
        private const string OPENSSL_BIN_SUBDIR = "bin";
        private const string OPENSSL_EXE = "openssl.exe";
        private const int SSL_CERTIFICATE_DAYS = 730;
        private const int RSA_KEY_SIZE = 2048;
        /// <summary>
        /// Gera um certificado SSL para o domínio especificado usando OpenSSL
        /// </summary>
        public static async Task GenerateSslCertificate(string[] args)
        {
            if (!ValidateArguments(args, out var domain, out var opensslVersion))
            {
                return;
            }

            var sslDir = EnsureSslDirectoryExists();
            var (certPath, keyPath) = GetCertificatePaths(sslDir, domain);

            opensslVersion = await EnsureOpenSslVersion(opensslVersion);
            if (string.IsNullOrEmpty(opensslVersion))
            {
                return;
            }

            var opensslExePath = await GetOrInstallOpenSsl(opensslVersion);
            if (string.IsNullOrEmpty(opensslExePath))
            {
                Console.WriteLine("OpenSSL não encontrado.");
                return;
            }

            await GenerateCertificateFiles(opensslExePath, domain, certPath, keyPath, sslDir);

            if (!VerifyCertificateGeneration(certPath, keyPath))
            {
                Console.WriteLine($"Falha ao gerar certificado SSL para {domain}.");
                return;
            }

            Console.WriteLine($"Certificado SSL gerado para {domain}.");
            TryInstallCertificateToWindowsStore(certPath, domain);
            await UpdateNginxConfiguration(domain, certPath, keyPath);
        }

        private static bool ValidateArguments(string[] args, out string domain, out string? opensslVersion)
        {
            domain = string.Empty;
            opensslVersion = null;

            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager ssl <dominio> [-openssl <versao>]");
                return false;
            }

            domain = args[0];
            opensslVersion = ParseOpenSslVersionFromArgs(args);
            return true;
        }

        private static string? ParseOpenSslVersionFromArgs(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-openssl" && (i + 1) < args.Length)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        private static string EnsureSslDirectoryExists()
        {
            var sslDir = Path.Combine(DevStackConfig.baseDir, SSL_CONFIG_SUBDIR);
            if (!Directory.Exists(sslDir))
            {
                Directory.CreateDirectory(sslDir);
            }
            return sslDir;
        }

        private static (string certPath, string keyPath) GetCertificatePaths(string sslDir, string domain)
        {
            var certPath = Path.Combine(sslDir, $"{domain}.crt");
            var keyPath = Path.Combine(sslDir, $"{domain}.key");
            return (certPath, keyPath);
        }

        private static Task<string?> EnsureOpenSslVersion(string? opensslVersion)
        {
            if (!string.IsNullOrEmpty(opensslVersion))
            {
                return Task.FromResult<string?>(opensslVersion);
            }

            var opensslComponent = Components.ComponentsFactory.GetComponent("openssl");
            if (opensslComponent == null)
            {
                Console.WriteLine("Versão do OpenSSL não especificada.");
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(opensslComponent.GetLatestVersion());
        }

        private static async Task<string?> GetOrInstallOpenSsl(string opensslVersion)
        {
            var opensslDir = Path.Combine(DevStackConfig.openSSLDir, $"openssl-{opensslVersion}", OPENSSL_BIN_SUBDIR);
            var opensslExePath = Path.Combine(opensslDir, OPENSSL_EXE);

            if (File.Exists(opensslExePath))
            {
                return opensslExePath;
            }

            await TryInstallOpenSsl(opensslVersion);

            return File.Exists(opensslExePath) ? opensslExePath : null;
        }

        private static async Task TryInstallOpenSsl(string opensslVersion)
        {
            try
            {
                await InstallManager.InstallCommands(new[] { "openssl", opensslVersion });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao instalar OpenSSL: {ex.Message}");
            }
        }

        private static async Task GenerateCertificateFiles(
            string opensslExePath,
            string domain,
            string certPath,
            string keyPath,
            string sslDir)
        {
            var configPath = CreateOpenSslConfigFile(sslDir, domain);

            try
            {
                var arguments = BuildOpenSslArguments(domain, certPath, keyPath, configPath);
                await ProcessManager.ExecuteProcessAsync(opensslExePath, arguments);
            }
            finally
            {
                DeleteFileIfExists(configPath);
            }
        }

        private static string CreateOpenSslConfigFile(string sslDir, string domain)
        {
            var configPath = Path.Combine(sslDir, $"openssl-{domain}-san.conf");
            var configContent = BuildOpenSslConfig(domain);
            File.WriteAllText(configPath, configContent);
            return configPath;
        }

        private static string BuildOpenSslConfig(string domain)
        {
            return $@"[req]
distinguished_name = req_distinguished_name
req_extensions = v3_req
[req_distinguished_name]
[v3_req]
subjectAltName = @alt_names
[alt_names]
DNS.1 = {domain}
";
        }

        private static string BuildOpenSslArguments(string domain, string certPath, string keyPath, string configPath)
        {
            return $"req -x509 -nodes -days {SSL_CERTIFICATE_DAYS} -newkey rsa:{RSA_KEY_SIZE} " +
                   $"-keyout \"{keyPath}\" -out \"{certPath}\" -subj \"/CN={domain}\" " +
                   $"-extensions v3_req -config \"{configPath}\"";
        }

        private static void DeleteFileIfExists(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore deletion errors
            }
        }

        private static bool VerifyCertificateGeneration(string certPath, string keyPath)
        {
            return File.Exists(certPath) && File.Exists(keyPath);
        }

        private static void TryInstallCertificateToWindowsStore(string certPath, string domain)
        {
            try
            {
                RemoveOldCertificateFromStore(domain);
                AddCertificateToStore(certPath);
                Console.WriteLine("Certificado removido (se existia) e instalado na store de autoridades confiáveis do Windows.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao instalar certificado na store do Windows: {ex.Message}");
            }
        }

        private static void RemoveOldCertificateFromStore(string domain)
        {
            var removeProcess = new ProcessStartInfo
            {
                FileName = "certutil.exe",
                Arguments = $"-delstore Root {domain}",
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true
            };
            Process.Start(removeProcess);
        }

        private static void AddCertificateToStore(string certPath)
        {
            var addProcess = new ProcessStartInfo
            {
                FileName = "certutil.exe",
                Arguments = $"-addstore Root \"{certPath}\"",
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true
            };
            Process.Start(addProcess);
        }

        private static Task UpdateNginxConfiguration(string domain, string certPath, string keyPath)
        {
            var (configPath, nginxVersion) = FindNginxConfigForDomain(domain);

            if (string.IsNullOrEmpty(configPath))
            {
                Console.WriteLine($"Arquivo de configuração do domínio {domain}.conf não encontrado em nenhuma versão do Nginx.");
                return Task.CompletedTask;
            }

            if (AddSslDirectivesToConfig(configPath, certPath, keyPath))
            {
                Console.WriteLine($"Diretivas SSL adicionadas ao arquivo {configPath} (nginx v{nginxVersion}).");
            }
            else
            {
                Console.WriteLine($"Diretivas SSL já presentes em {configPath} (nginx v{nginxVersion}).");
            }
            
            return Task.CompletedTask;
        }

        private static (string configPath, string nginxVersion) FindNginxConfigForDomain(string domain)
        {
            var nginxRootDir = DevStackConfig.nginxDir;

            if (!Directory.Exists(nginxRootDir))
            {
                return (string.Empty, string.Empty);
            }

            var nginxVersions = Directory.GetDirectories(nginxRootDir, "nginx-*");

            foreach (var nginxVersionDir in nginxVersions)
            {
                var versionName = Path.GetFileName(nginxVersionDir)?.Replace("nginx-", "") ?? string.Empty;
                var sitesDir = Path.Combine(nginxVersionDir, DevStackConfig.nginxSitesDir);
                var configPath = Path.Combine(sitesDir, $"{domain}.conf");

                if (File.Exists(configPath))
                {
                    return (configPath, versionName);
                }
            }

            return (string.Empty, string.Empty);
        }

        private static bool AddSslDirectivesToConfig(string configPath, string certPath, string keyPath)
        {
            var lines = File.ReadAllLines(configPath).ToList();
            var sslDirectives = CreateSslDirectives(certPath, keyPath);

            if (AllDirectivesExist(lines, sslDirectives))
            {
                return false;
            }

            var insertIndex = FindSslInsertIndex(lines);
            InsertMissingDirectives(lines, sslDirectives, insertIndex);

            File.WriteAllLines(configPath, lines);
            return true;
        }

        private static List<string> CreateSslDirectives(string certPath, string keyPath)
        {
            return new List<string>
            {
                "    listen 443 ssl;",
                "    listen [::]:443 ssl;",
                $"    ssl_certificate      {certPath.Replace("\\", "/")};",
                $"    ssl_certificate_key  {keyPath.Replace("\\", "/")};"
            };
        }

        private static bool AllDirectivesExist(List<string> lines, List<string> directives)
        {
            return directives.All(directive =>
                lines.Any(line => line.Trim() == directive.Trim()));
        }

        private static int FindSslInsertIndex(List<string> lines)
        {
            var listenIndex = lines.FindIndex(l => l.Contains("listen [::]:80"));
            if (listenIndex != -1)
            {
                return listenIndex + 1;
            }

            var serverIndex = lines.FindIndex(l => l.Trim().StartsWith("server {"));
            return serverIndex != -1 ? serverIndex + 1 : 0;
        }

        private static void InsertMissingDirectives(List<string> lines, List<string> directives, int startIndex)
        {
            var addedCount = 0;
            foreach (var directive in directives)
            {
                if (!lines.Any(l => l.Trim() == directive.Trim()))
                {
                    lines.Insert(startIndex + addedCount, directive);
                    addedCount++;
                }
            }
        }
    }
}
