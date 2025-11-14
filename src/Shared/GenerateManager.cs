using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Manages generation of SSL certificates and Nginx configuration for custom domains.
    /// Automates OpenSSL certificate creation and Windows Trusted Root certificate installation.
    /// </summary>
    public static class GenerateManager
    {
        /// <summary>
        /// Subdirectory path for SSL configuration files within Nginx configs.
        /// </summary>
        private const string SSL_CONFIG_SUBDIR = "configs/nginx/ssl";
        
        /// <summary>
        /// Subdirectory path for OpenSSL binary files.
        /// </summary>
        private const string OPENSSL_BIN_SUBDIR = "bin";
        
        /// <summary>
        /// OpenSSL executable file name.
        /// </summary>
        private const string OPENSSL_EXE = "openssl.exe";
        
        /// <summary>
        /// Number of days for SSL certificate validity (2 years).
        /// </summary>
        private const int SSL_CERTIFICATE_DAYS = 730;
        
        /// <summary>
        /// RSA key size in bits for SSL certificate generation.
        /// </summary>
        private const int RSA_KEY_SIZE = 2048;

        /// <summary>
        /// Generates an SSL certificate for the specified domain using OpenSSL.
        /// Creates certificate files, installs them to Windows certificate store, and updates Nginx configuration.
        /// </summary>
        /// <param name="args">Command arguments: domain name, optional -openssl version flag.</param>
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
                Console.WriteLine("OpenSSL not found.");
                return;
            }

            await GenerateCertificateFiles(opensslExePath, domain, certPath, keyPath, sslDir);

            if (!VerifyCertificateGeneration(certPath, keyPath))
            {
                Console.WriteLine($"Failed to generate SSL certificate for {domain}.");
                return;
            }

            Console.WriteLine($"SSL certificate generated for {domain}.");
            TryInstallCertificateToWindowsStore(certPath, domain);
            await UpdateNginxConfiguration(domain, certPath, keyPath);
        }

        /// <summary>
        /// Validates command line arguments and extracts domain and OpenSSL version.
        /// </summary>
        /// <param name="args">Command arguments array.</param>
        /// <param name="domain">Extracted domain name.</param>
        /// <param name="opensslVersion">Optional OpenSSL version from -openssl flag.</param>
        /// <returns>True if arguments are valid, false otherwise.</returns>
        private static bool ValidateArguments(string[] args, out string domain, out string? opensslVersion)
        {
            domain = string.Empty;
            opensslVersion = null;

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: DevStackManager ssl <domain> [-openssl <version>]");
                return false;
            }

            domain = args[0];
            opensslVersion = ParseOpenSslVersionFromArgs(args);
            return true;
        }

        /// <summary>
        /// Parses the OpenSSL version from command line arguments following the -openssl flag.
        /// </summary>
        /// <param name="args">Command arguments array.</param>
        /// <returns>The version string if found, null otherwise.</returns>
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

        /// <summary>
        /// Ensures the SSL configuration directory exists, creating it if necessary.
        /// </summary>
        /// <returns>The absolute path to the SSL directory.</returns>
        private static string EnsureSslDirectoryExists()
        {
            var sslDir = Path.Combine(DevStackConfig.baseDir, SSL_CONFIG_SUBDIR);
            if (!Directory.Exists(sslDir))
            {
                Directory.CreateDirectory(sslDir);
            }
            return sslDir;
        }

        /// <summary>
        /// Constructs the file paths for certificate and private key files based on domain name.
        /// </summary>
        /// <param name="sslDir">SSL configuration directory path.</param>
        /// <param name="domain">Domain name.</param>
        /// <returns>Tuple containing certificate path and key path.</returns>
        private static (string certPath, string keyPath) GetCertificatePaths(string sslDir, string domain)
        {
            var certPath = Path.Combine(sslDir, $"{domain}.crt");
            var keyPath = Path.Combine(sslDir, $"{domain}.key");
            return (certPath, keyPath);
        }

        /// <summary>
        /// Ensures an OpenSSL version is specified, using the latest version if not provided.
        /// </summary>
        /// <param name="opensslVersion">Optional version from command arguments.</param>
        /// <returns>The OpenSSL version to use, or null if unavailable.</returns>
        private static Task<string?> EnsureOpenSslVersion(string? opensslVersion)
        {
            if (!string.IsNullOrEmpty(opensslVersion))
            {
                return Task.FromResult<string?>(opensslVersion);
            }

            var opensslComponent = Components.ComponentsFactory.GetComponent("openssl");
            if (opensslComponent == null)
            {
                Console.WriteLine("OpenSSL version not specified.");
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(opensslComponent.GetLatestVersion());
        }

        /// <summary>
        /// Gets the path to the OpenSSL executable, installing it if not found.
        /// </summary>
        /// <param name="opensslVersion">The OpenSSL version to locate or install.</param>
        /// <returns>The path to openssl.exe, or null if unavailable.</returns>
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

        /// <summary>
        /// Attempts to install OpenSSL using the InstallManager.
        /// </summary>
        /// <param name="opensslVersion">The OpenSSL version to install.</param>
        private static async Task TryInstallOpenSsl(string opensslVersion)
        {
            try
            {
                await InstallManager.InstallCommands(new[] { "openssl", opensslVersion });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing OpenSSL: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates the certificate and private key files using OpenSSL.
        /// Creates a temporary configuration file with SAN extensions for the domain.
        /// </summary>
        /// <param name="opensslExePath">Path to the OpenSSL executable.</param>
        /// <param name="domain">Domain name for the certificate.</param>
        /// <param name="certPath">Output path for the certificate file.</param>
        /// <param name="keyPath">Output path for the private key file.</param>
        /// <param name="sslDir">SSL configuration directory.</param>
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

        /// <summary>
        /// Creates a temporary OpenSSL configuration file with SAN extension for the domain.
        /// </summary>
        /// <param name="sslDir">SSL configuration directory.</param>
        /// <param name="domain">Domain name to include in SAN extension.</param>
        /// <returns>The path to the created configuration file.</returns>
        private static string CreateOpenSslConfigFile(string sslDir, string domain)
        {
            var configPath = Path.Combine(sslDir, $"openssl-{domain}-san.conf");
            var configContent = BuildOpenSslConfig(domain);
            File.WriteAllText(configPath, configContent);
            return configPath;
        }

        /// <summary>
        /// Builds the OpenSSL configuration file content with SAN extension.
        /// </summary>
        /// <param name="domain">Domain name to include in SAN.</param>
        /// <returns>The configuration file content.</returns>
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

        /// <summary>
        /// Builds the OpenSSL command line arguments for certificate generation.
        /// </summary>
        /// <param name="domain">Domain name for the certificate CN.</param>
        /// <param name="certPath">Output path for the certificate.</param>
        /// <param name="keyPath">Output path for the private key.</param>
        /// <param name="configPath">Path to the OpenSSL configuration file.</param>
        /// <returns>The command line arguments string.</returns>
        private static string BuildOpenSslArguments(string domain, string certPath, string keyPath, string configPath)
        {
            return $"req -x509 -nodes -days {SSL_CERTIFICATE_DAYS} -newkey rsa:{RSA_KEY_SIZE} " +
                   $"-keyout \"{keyPath}\" -out \"{certPath}\" -subj \"/CN={domain}\" " +
                   $"-extensions v3_req -config \"{configPath}\"";
        }

        /// <summary>
        /// Deletes a file if it exists, ignoring any errors.
        /// </summary>
        /// <param name="filePath">The file path to delete.</param>
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
            }
        }

        /// <summary>
        /// Verifies that both certificate and key files were successfully created.
        /// </summary>
        /// <param name="certPath">Certificate file path.</param>
        /// <param name="keyPath">Private key file path.</param>
        /// <returns>True if both files exist, false otherwise.</returns>
        private static bool VerifyCertificateGeneration(string certPath, string keyPath)
        {
            return File.Exists(certPath) && File.Exists(keyPath);
        }

        /// <summary>
        /// Attempts to install the certificate to the Windows Trusted Root certificate store.
        /// Removes any existing certificate for the domain before installing the new one.
        /// </summary>
        /// <param name="certPath">Path to the certificate file.</param>
        /// <param name="domain">Domain name for the certificate.</param>
        private static void TryInstallCertificateToWindowsStore(string certPath, string domain)
        {
            try
            {
                RemoveOldCertificateFromStore(domain);
                AddCertificateToStore(certPath);
                Console.WriteLine("Certificate removed (if existed) and installed to Windows Trusted Root Authorities store.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to install certificate to Windows store: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes an old certificate for the domain from the Windows certificate store.
        /// </summary>
        /// <param name="domain">Domain name to remove from the certificate store.</param>
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

        /// <summary>
        /// Adds a certificate to the Windows Trusted Root Authorities store.
        /// </summary>
        /// <param name="certPath">Path to the certificate file to add.</param>
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

        /// <summary>
        /// Updates the Nginx configuration file for the domain with SSL directives.
        /// Searches all Nginx versions for the domain configuration file.
        /// </summary>
        /// <param name="domain">Domain name to update.</param>
        /// <param name="certPath">Path to the SSL certificate file.</param>
        /// <param name="keyPath">Path to the SSL private key file.</param>
        private static Task UpdateNginxConfiguration(string domain, string certPath, string keyPath)
        {
            var (configPath, nginxVersion) = FindNginxConfigForDomain(domain);

            if (string.IsNullOrEmpty(configPath))
            {
                Console.WriteLine($"Configuration file {domain}.conf not found in any Nginx version.");
                return Task.CompletedTask;
            }

            if (AddSslDirectivesToConfig(configPath, certPath, keyPath))
            {
                Console.WriteLine($"SSL directives added to {configPath} (nginx v{nginxVersion}).");
            }
            else
            {
                Console.WriteLine($"SSL directives already present in {configPath} (nginx v{nginxVersion}).");
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Searches all Nginx installation directories for a domain configuration file.
        /// </summary>
        /// <param name="domain">Domain name to search for.</param>
        /// <returns>Tuple containing the configuration file path and Nginx version, or empty strings if not found.</returns>
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

        /// <summary>
        /// Adds SSL directives to an Nginx configuration file if they don't already exist.
        /// </summary>
        /// <param name="configPath">Path to the Nginx configuration file.</param>
        /// <param name="certPath">Path to the SSL certificate file.</param>
        /// <param name="keyPath">Path to the SSL private key file.</param>
        /// <returns>True if directives were added, false if they already exist.</returns>
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

        /// <summary>
        /// Creates the list of SSL configuration directives for Nginx.
        /// </summary>
        /// <param name="certPath">Path to the SSL certificate file.</param>
        /// <param name="keyPath">Path to the SSL private key file.</param>
        /// <returns>List of SSL directive strings with proper formatting.</returns>
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

        /// <summary>
        /// Checks if all SSL directives already exist in the configuration file.
        /// </summary>
        /// <param name="lines">Configuration file lines.</param>
        /// <param name="directives">SSL directives to check for.</param>
        /// <returns>True if all directives exist, false otherwise.</returns>
        private static bool AllDirectivesExist(List<string> lines, List<string> directives)
        {
            return directives.All(directive =>
                lines.Any(line => line.Trim() == directive.Trim()));
        }

        /// <summary>
        /// Finds the appropriate line index to insert SSL directives in the Nginx configuration.
        /// </summary>
        /// <param name="lines">Configuration file lines.</param>
        /// <returns>The line index where SSL directives should be inserted.</returns>
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

        /// <summary>
        /// Inserts SSL directives that don't already exist into the configuration file.
        /// </summary>
        /// <param name="lines">Configuration file lines.</param>
        /// <param name="directives">SSL directives to insert.</param>
        /// <param name="startIndex">Line index to start inserting at.</param>
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
