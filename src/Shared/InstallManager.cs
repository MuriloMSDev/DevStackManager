using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevStackManager
{
    public static class InstallManager
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static InstallManager()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DevStackManager");
        }

        public static async Task InstallCommands(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Nenhum componente especificado para instalar.");
                return;
            }

            string toolName = args[0];
            string? version = args.Length > 1 ? args[1] : null;
            await InstallComponent(toolName, version);

            // PathManager.AddBinDirsToPath() será chamado no DevStackConfig.cs após a instalação
        }

        private static async Task InstallComponent(string component, string? version)
        {
            switch (component.ToLowerInvariant())
            {
                case "php": await InstallPHP(version); break;
                case "nginx": await InstallNginx(version); break;
                case "mysql": await InstallMySQL(version); break;
                case "node": await InstallNode(version); break;
                case "python": await InstallPython(version); break;
                case "composer": await InstallComposer(version); break;
                case "phpmyadmin": await InstallPhpMyAdmin(version); break;
                case "git": await InstallGit(version); break;
                case "mongodb": await InstallMongoDB(version); break;
                case "redis": await InstallRedis(version); break;
                case "pgsql": await InstallPgsql(version); break;
                case "mailhog": await InstallMailhog(version); break;
                case "elasticsearch": await InstallElastic(version); break;
                case "memcached": await InstallMemcached(version); break;
                case "docker": await InstallDocker(version); break;
                case "yarn": InstallYarn(version); break;
                case "pnpm": InstallPnpm(version); break;
                case "wpcli": await InstallWpcli(version); break;
                case "adminer": await InstallAdminer(version); break;
                case "poetry": InstallPoetry(version); break;
                case "ruby": await InstallRuby(version); break;
                case "go": await InstallGo(version); break;
                case "certbot": InstallCertbot(version); break;
                case "openssl": await InstallOpenSSL(version); break;
                case "phpcsfixer": await InstallPHPCsFixer(version); break;
                default: Console.WriteLine($"Componente desconhecido: {component}"); break;
            }
        }

        private static async Task<bool> InstallGenericTool(string toolDir, string version, string zipUrl, string subDir, string exeName, string prefix, bool renameExe = true)
        {
            string targetDir = Path.Combine(toolDir, subDir);
            bool rollback = false;

            try
            {
                if (Directory.Exists(targetDir))
                {
                    Console.WriteLine($"{prefix} {version} já está instalado.");
                    return true;
                }

                if (!Directory.Exists(toolDir))
                {
                    Directory.CreateDirectory(toolDir);
                }

                Console.WriteLine($"Baixando {prefix} {version}...");
                string zipPath = Path.Combine(toolDir, $"{prefix}-{version}.zip");
                await DownloadAndExtractZip(zipUrl, zipPath, targetDir);
                Console.WriteLine($"{prefix} {version} instalado.");
                rollback = true;

                if (renameExe)
                {
                    Console.WriteLine("Renomeando executável principal...");
                    RenameMainExe(targetDir, exeName, version, prefix);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao instalar {prefix} {version}: {ex.Message}");
                if (rollback && Directory.Exists(targetDir))
                {
                    try { Directory.Delete(targetDir, true); } catch { }
                }
                throw;
            }
        }

        private static async Task DownloadAndExtractZip(string url, string zipPath, string extractTo)
        {
            Console.WriteLine($"Baixando {url}...");
            
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            // Use using block for automatic disposal
            using (var fileStream = File.Create(zipPath))
            {
                await response.Content.CopyToAsync(fileStream);
            } // fileStream is automatically closed and disposed here

            Console.WriteLine("Extraindo...");
            
            // Check if zip contains a top-level folder
            string? topFolder = null;
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                var firstEntry = archive.Entries.FirstOrDefault();
                if (firstEntry != null)
                {
                    var match = Regex.Match(firstEntry.FullName, @"^([^/\\]+)[/\\]");
                    if (match.Success)
                    {
                        topFolder = match.Groups[1].Value;
                    }
                }

                archive.ExtractToDirectory(extractTo, true);
            } // archive is automatically closed and disposed here

            // Wait a bit to ensure file handles are released
            await Task.Delay(100);

            // If the zip contains a top-level folder, move its contents up
            if (!string.IsNullOrEmpty(topFolder))
            {
                string extractedPath = Path.Combine(extractTo, topFolder);
                if (Directory.Exists(extractedPath))
                {
                    foreach (var item in Directory.GetFileSystemEntries(extractedPath))
                    {
                        string destPath = Path.Combine(extractTo, Path.GetFileName(item));
                        if (Directory.Exists(item))
                        {
                            Directory.Move(item, destPath);
                        }
                        else
                        {
                            File.Move(item, destPath);
                        }
                    }
                    Directory.Delete(extractedPath);
                }
            }

            // Try to delete with retry mechanism
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    File.Delete(zipPath);
                    break;
                }
                catch (IOException) when (attempt < 4)
                {
                    // Wait a bit and try again
                    await Task.Delay(200);
                }
            }
        }

        private static void RenameMainExe(string dir, string exeName, string version, string prefix)
        {
            string exePath = Path.Combine(dir, exeName);
            string exeVersionPath = Path.Combine(dir, $"{prefix}-{version}.exe");

            if (File.Exists(exePath))
            {
                File.Move(exePath, exeVersionPath);
                Console.WriteLine($"Renomeado {exeName} para {prefix}-{version}.exe");
            }
        }

        // Version getter methods
        public static string GetLatestNginxVersion()
        {
            var versionData = DataManager.GetNginxVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Nginx.");
        }

        public static string GetLatestPHPVersion()
        {
            var versionData = DataManager.GetPHPVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do PHP.");
        }

        public static string GetLatestNodeVersion()
        {
            var versionData = DataManager.GetNodeVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão LTS do Node.js.");
        }

        public static string GetLatestPythonVersion()
        {
            var versionData = DataManager.GetPythonVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível encontrar uma versão estável do Python nos índices Windows");
        }

        public static string GetLatestComposerVersion()
        {
            var versionData = DataManager.GetComposerVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Composer.");
        }

        public static string GetLatestPhpMyAdminVersion()
        {
            var versionData = DataManager.GetPhpMyAdminVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do phpMyAdmin.");
        }

        public static string GetLatestMongoDBVersion()
        {
            var versionData = DataManager.GetMongoDBVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do MongoDB.");
        }

        public static string GetLatestRedisVersion()
        {
            var versionData = DataManager.GetRedisVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Redis.");
        }

        public static string GetLatestOpenSSLVersion()
        {
            var versionData = DataManager.GetOpenSSLVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do OpenSSL.");
        }

        // Install methods
        public static async Task InstallNginx(string? version = null)
        {
            version ??= GetLatestNginxVersion();
            var subDir = $"nginx-{version}";
            var zipUrl = $"https://nginx.org/download/nginx-{version}.zip";
            Console.WriteLine(DevStackConfig.nginxDir);
            await InstallGenericTool(DevStackConfig.nginxDir, version, zipUrl, subDir, "nginx.exe", "nginx");
        }

        public static async Task InstallPHP(string? version = null)
        {
            version ??= GetLatestPHPVersion();

            string[] urls = [
                "https://windows.php.net/downloads/releases/",
                "https://windows.php.net/downloads/releases/archives/"
            ];

            string? phpUrl = null;
            string phpZipName = $"php-{version}-Win32-vs16-x64.zip";

            foreach (string baseUrl in urls)
            {
                var page = await httpClient.GetStringAsync(baseUrl);
                var match = Regex.Match(page, $@"php-{Regex.Escape(version)}-Win32-[^-]+-x64\.zip");
                if (match.Success)
                {
                    phpZipName = match.Value;
                    phpUrl = baseUrl + phpZipName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(phpUrl))
            {
                throw new Exception($"Não foi encontrado o arquivo .zip para PHP {version} nas releases oficiais.");
            }

            string subDir = $"php-{version}";
            await InstallGenericTool(DevStackConfig.phpDir, version, phpUrl, subDir, "php.exe", "php");
            
            // Rename php-cgi.exe
            string phpDir = Path.Combine(DevStackConfig.phpDir, subDir);
            RenameMainExe(phpDir, "php-cgi.exe", version, "php-cgi");

            // Copy php.ini if exists
            string phpIniSrc = Path.Combine("configs", "php", "php.ini");
            string phpIniDst = Path.Combine(phpDir, "php.ini");
            
            if (File.Exists(phpIniSrc))
            {
                File.Copy(phpIniSrc, phpIniDst, true);
                Console.WriteLine($"Arquivo php.ini copiado para {phpDir}");
                
                // Add essential extensions
                string extDir = Path.Combine(phpDir, "ext");
                if (Directory.Exists(extDir))
                {
                    string[] acceptExt = {
                        "mbstring", "intl", "pdo", "pdo_mysql", "pdo_pgsql", "openssl", 
                        "json", "fileinfo", "curl", "gd", "gd2", "zip", "xml", "xmlrpc"
                    };

                    var extensions = Directory.GetFiles(extDir, "*.dll")
                        .Select(f => Path.GetFileNameWithoutExtension(f).Replace("php_", ""))
                        .Where(name => acceptExt.Contains(name))
                        .Select(name => $"extension={name}")
                        .ToArray();

                    if (extensions.Any())
                    {
                        File.AppendAllText(phpIniDst, $"\n; Extensões essenciais para CakePHP e Laravel:\n{string.Join("\n", extensions)}", Encoding.UTF8);
                        Console.WriteLine("Bloco de extensões essenciais adicionado ao php.ini");
                    }
                }
            }
            else
            {
                Console.WriteLine("Arquivo configs\\php\\php.ini não encontrado. Pulei a cópia do php.ini.");
            }
        }

        public static async Task InstallMySQL(string? version = null)
        {
            if (version == null)
            {
                var versionData = DataManager.GetMySQLVersions();
                if (versionData.Status == "success" && versionData.Versions.Any())
                {
                    version = versionData.Versions.First();
                }
                else
                {
                    version = "8.0.36"; // fallback version
                }
            }
            
            string subDir = $"mysql-{version}-winx64";
            string zipUrl = $"https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-{version}-winx64.zip";
            await InstallGenericTool(DevStackConfig.mysqlDir, version, zipUrl, subDir, Path.Combine("bin", "mysqld.exe"), "mysqld");
        }

        public static async Task InstallNode(string? version = null)
        {
            version ??= GetLatestNodeVersion();
            
            string subDir = $"node-{version}";
            string zipUrl = $"https://nodejs.org/dist/v{version}/node-v{version}-win-x64.zip";
            
            await InstallGenericTool(DevStackConfig.nodeDir, version, zipUrl, subDir, "node.exe", "node", false);
            
            string nodePath = Path.Combine(DevStackConfig.nodeDir, subDir);
            string binDir = Path.Combine(DevStackConfig.nodeDir, "bin");
            
            if (!Directory.Exists(binDir))
            {
                Directory.CreateDirectory(binDir);
            }

            string srcExe = Path.Combine(nodePath, "node.exe");
            string dstExe = Path.Combine(binDir, $"node-{version}.exe");
            
            if (File.Exists(srcExe))
            {
                File.Copy(srcExe, dstExe, true);
                Console.WriteLine($"Atalho node-{version}.exe criado em {binDir}");
            }

            // Handle npm and npx renaming
            string npmPkgJson = Path.Combine(nodePath, "node_modules", "npm", "package.json");
            if (File.Exists(npmPkgJson))
            {
                var npmPackageContent = File.ReadAllText(npmPkgJson);
                using var doc = JsonDocument.Parse(npmPackageContent);
                string npmVersion = doc.RootElement.GetProperty("version").GetString()!;

                string[] npmFiles = { "npm", "npm.cmd", "npm.ps1" };
                string[] npxFiles = { "npx", "npx.cmd", "npx.ps1" };

                foreach (string npmFile in npmFiles)
                {
                    string fullPath = Path.Combine(nodePath, npmFile);
                    if (File.Exists(fullPath))
                    {
                        string ext = Path.GetExtension(fullPath);
                        string newName = $"npm-{npmVersion}{ext}";
                        string newPath = Path.Combine(nodePath, newName);
                        File.Move(fullPath, newPath);
                        Console.WriteLine($"Renomeado {npmFile} para {newName}");
                    }
                }

                foreach (string npxFile in npxFiles)
                {
                    string fullPath = Path.Combine(nodePath, npxFile);
                    if (File.Exists(fullPath))
                    {
                        string ext = Path.GetExtension(fullPath);
                        string newName = $"npx-{npmVersion}{ext}";
                        string newPath = Path.Combine(nodePath, newName);
                        File.Move(fullPath, newPath);
                        Console.WriteLine($"Renomeado {npxFile} para {newName}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Arquivo package.json do npm não encontrado.");
            }
        }

        public static async Task InstallPython(string? version = null)
        {
            version ??= GetLatestPythonVersion();
            
            string pySubDir = $"python-{version}";
            
            string[] pythonIndexUrls = {
                "https://www.python.org/ftp/python/index-windows-recent.json",
                "https://www.python.org/ftp/python/index-windows-legacy.json",
                "https://www.python.org/ftp/python/index-windows.json"
            };

            string? pyUrl = null;
            foreach (string indexUrl in pythonIndexUrls)
            {
                try
                {
                    var json = await httpClient.GetStringAsync(indexUrl);
                    using var doc = JsonDocument.Parse(json);
                    
                    if (doc.RootElement.TryGetProperty("versions", out var versions))
                    {
                        foreach (var versionEntry in versions.EnumerateArray())
                        {
                            if (versionEntry.TryGetProperty("url", out var url))
                            {
                                var urlString = url.GetString()!;
                                if (urlString.EndsWith($"python-{version}-amd64.zip") && 
                                    !urlString.Contains("embeddable") && !urlString.Contains("test"))
                                {
                                    Console.WriteLine($"Found exact match for Python {version} in {indexUrl}");
                                    pyUrl = urlString;
                                    break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(pyUrl)) break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(pyUrl))
            {
                Console.WriteLine("URL específica não encontrada no índice, usando URL padrão.", Console.ForegroundColor = ConsoleColor.Yellow);
                pyUrl = $"https://www.python.org/ftp/python/{version}/python-{version}-amd64.zip";
                Console.WriteLine($"Tentando download de: {pyUrl}");
            }
            else
            {
                Console.WriteLine($"Usando URL encontrada nos índices: {pyUrl}", Console.ForegroundColor = ConsoleColor.Green);
            }

            await InstallGenericTool(DevStackConfig.pythonDir, version, pyUrl, pySubDir, "python.exe", "python");
            Console.WriteLine($"Python {version} instalado.");
        }

        public static async Task InstallComposer(string? version = null)
        {
            version ??= GetLatestComposerVersion();
            
            string composerSubDir = $"composer-{version}";
            string composerPhar = $"composer-{version}.phar";
            string composerPharPath = Path.Combine(DevStackConfig.composerDir, composerSubDir);
            
            if (Directory.Exists(composerPharPath))
            {
                Console.WriteLine($"Composer {version} já está instalado.");
                return;
            }

            Console.WriteLine($"Baixando Composer {version}...");
            Directory.CreateDirectory(composerPharPath);
            
            string composerUrl = $"https://getcomposer.org/download/{version}/composer.phar";
            string pharPath = Path.Combine(composerPharPath, composerPhar);
            
            using var response = await httpClient.GetAsync(composerUrl);
            response.EnsureSuccessStatusCode();
            
            using (var fileStream = File.Create(pharPath))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            
            Console.WriteLine($"Composer {version} instalado.");
            DevStackConfig.WriteLog($"Composer {version} instalado em {composerPharPath}");
        }

        public static async Task InstallPhpMyAdmin(string? version = null)
        {
            version ??= GetLatestPhpMyAdminVersion();
            
            string pmaVersionDir = Path.Combine(DevStackConfig.pmaDir, $"phpmyadmin-{version}");
            if (Directory.Exists(pmaVersionDir))
            {
                Console.WriteLine($"phpMyAdmin {version} já está instalado.");
                return;
            }

            Console.WriteLine($"Baixando phpMyAdmin {version}...");
            string pmaZip = Path.Combine(DevStackConfig.baseDir, $"phpmyadmin-{version}-all-languages.zip");
            string pmaUrl = $"https://files.phpmyadmin.net/phpMyAdmin/{version}/phpMyAdmin-{version}-all-languages.zip";
            
            using var response = await httpClient.GetAsync(pmaUrl);
            response.EnsureSuccessStatusCode();
            
            using (var fileStream = File.Create(pmaZip))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            ZipFile.ExtractToDirectory(pmaZip, DevStackConfig.baseDir, true);
            
            string extractedDir = Path.Combine(DevStackConfig.baseDir, $"phpMyAdmin-{version}-all-languages");
            if (Directory.Exists(extractedDir))
            {
                Directory.Move(extractedDir, pmaVersionDir);
            }
            
            // Try to delete with retry mechanism
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    File.Delete(pmaZip);
                    break;
                }
                catch (IOException) when (attempt < 4)
                {
                    await Task.Delay(200);
                }
            }
            Console.WriteLine($"phpMyAdmin {version} instalado em {pmaVersionDir}.");
            DevStackConfig.WriteLog($"phpMyAdmin {version} instalado em {pmaVersionDir}");
        }

        public static async Task InstallGit(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                var json = await httpClient.GetStringAsync("https://api.github.com/repos/git-for-windows/git/releases/latest");
                using var doc = JsonDocument.Parse(json);
                version = doc.RootElement.GetProperty("tag_name").GetString()!.TrimStart('v');
            }

            string gitSubDir = $"git-{version}";
            string gitDirFull = Path.Combine(DevStackConfig.baseDir, gitSubDir);
            
            if (Directory.Exists(gitDirFull))
            {
                Console.WriteLine($"Git {version} já está instalado.");
                return;
            }

            Console.WriteLine($"Baixando Git {version}...");
            string gitUrl = $"https://github.com/git-for-windows/git/releases/download/v{version}/PortableGit-{version}-64-bit.7z.exe";
            string git7zExe = Path.Combine(DevStackConfig.baseDir, $"PortableGit-{version}-64-bit.7z.exe");
            
            using var response = await httpClient.GetAsync(gitUrl);
            response.EnsureSuccessStatusCode();
            
            using (var fileStream = File.Create(git7zExe))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            Directory.CreateDirectory(gitDirFull);
            Console.WriteLine($"Extraindo Git {version}...");
            
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = git7zExe,
                Arguments = $"-y -o\"{gitDirFull}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            
            File.Delete(git7zExe);
            Console.WriteLine($"Git {version} instalado em {gitDirFull}.");
            DevStackConfig.WriteLog($"Git {version} instalado em {gitDirFull}");
        }

        public static async Task InstallMongoDB(string? version = null)
        {
            version ??= GetLatestMongoDBVersion();
            string subDir = $"mongodb-{version}";
            string zipUrl = $"https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-{version}.zip";
            await InstallGenericTool(DevStackConfig.mongoDir, version, zipUrl, subDir, Path.Combine("bin", "mongod.exe"), "mongod");
            Console.WriteLine($"MongoDB {version} instalado.");
        }

        public static async Task InstallRedis(string? version = null)
        {
            version ??= GetLatestRedisVersion();
            string subDir = $"redis-{version}";
            string zipUrl = $"https://github.com/tporadowski/redis/releases/download/v{version}/redis-{version}.zip";
            await InstallGenericTool(DevStackConfig.redisDir, version, zipUrl, subDir, "redis-server.exe", "redis");
            Console.WriteLine($"Redis {version} instalado.");
        }

        public static async Task InstallPgsql(string? version = null)
        {
            version ??= GetLatestPgSQLVersion();
            string subDir = $"pgsql-{version}";
            string zipUrl = $"https://get.enterprisedb.com/postgresql/postgresql-{version}-windows-x64-binaries.zip";
            await InstallGenericTool(DevStackConfig.pgsqlDir, version, zipUrl, subDir, Path.Combine("bin", "psql.exe"), "psql");
            Console.WriteLine($"PostgreSQL {version} instalado.");
        }

        public static async Task InstallMailhog(string? version = null)
        {
            version ??= GetLatestMailHogVersion();
            string subDir = $"mailhog-{version}";
            string zipUrl = $"https://github.com/mailhog/MailHog/releases/download/v{version}/MailHog_windows_amd64.zip";
            await InstallGenericTool(DevStackConfig.mailhogDir, version, zipUrl, subDir, "MailHog.exe", "mailhog");
            Console.WriteLine($"MailHog {version} instalado.");
        }

        public static async Task InstallElastic(string? version = null)
        {
            version ??= GetLatestElasticsearchVersion();
            string subDir = $"elasticsearch-{version}";
            string zipUrl = $"https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-{version}-windows-x86_64.zip";
            await InstallGenericTool(DevStackConfig.elasticDir, version, zipUrl, subDir, Path.Combine("bin", "elasticsearch.bat"), "elasticsearch");
            Console.WriteLine($"Elasticsearch {version} instalado.");
        }

        public static async Task InstallMemcached(string? version = null)
        {
            version ??= GetLatestMemcachedVersion();
            string subDir = $"memcached-{version}";
            string zipUrl = $"https://github.com/nono303/memcached/releases/download/{version}/memcached-{version}-win64.zip";
            await InstallGenericTool(DevStackConfig.memcachedDir, version, zipUrl, subDir, "memcached.exe", "memcached");
            Console.WriteLine($"Memcached {version} instalado.");
        }

        public static async Task InstallDocker(string? version = null)
        {
            version ??= GetLatestDockerVersion();
            string url = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe";
            string installer = Path.Combine(DevStackConfig.dockerDir, "DockerDesktopInstaller.exe");
            
            if (!Directory.Exists(DevStackConfig.dockerDir))
            {
                Directory.CreateDirectory(DevStackConfig.dockerDir);
            }

            Console.WriteLine("Baixando Docker Desktop...");
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fileStream = File.Create(installer);
            await response.Content.CopyToAsync(fileStream);
            fileStream.Close();

            Console.WriteLine("Executando instalador do Docker Desktop...");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = installer,
                Arguments = "/install /quiet",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            
            File.Delete(installer);
            Console.WriteLine("Docker Desktop instalado.");
        }

        public static void InstallYarn(string? version = null)
        {
            Console.WriteLine("Instalando Yarn via npm...");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "install -g yarn",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            Console.WriteLine("Yarn instalado globalmente.");
        }

        public static void InstallPnpm(string? version = null)
        {
            Console.WriteLine("Instalando pnpm via npm...");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "install -g pnpm",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            Console.WriteLine("pnpm instalado globalmente.");
        }

        public static async Task InstallWpcli(string? version = null)
        {
            version ??= GetLatestWPCLIVersion();
            string url = $"https://github.com/wp-cli/wp-cli/releases/download/v{version}/wp-cli-{version}.phar";
            
            if (!Directory.Exists(DevStackConfig.wpcliDir))
            {
                Directory.CreateDirectory(DevStackConfig.wpcliDir);
            }

            string pharPath = Path.Combine(DevStackConfig.wpcliDir, $"wp-cli-{version}.phar");
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fileStream = File.Create(pharPath);
            await response.Content.CopyToAsync(fileStream);
            fileStream.Close();

            string batPath = Path.Combine(DevStackConfig.wpcliDir, "wp.bat");
            File.WriteAllText(batPath, $"@echo off\nphp %~dp0wp-cli-{version}.phar %*", Encoding.UTF8);
            Console.WriteLine($"WP-CLI {version} instalado em {DevStackConfig.wpcliDir}. Use 'wp' no terminal.");
        }

        public static async Task InstallAdminer(string? version = null)
        {
            version ??= GetLatestAdminerVersion();
            
            if (!Directory.Exists(DevStackConfig.adminerDir))
            {
                Directory.CreateDirectory(DevStackConfig.adminerDir);
            }

            string url = $"https://github.com/vrana/adminer/releases/download/v{version}/adminer-{version}.php";
            string phpPath = Path.Combine(DevStackConfig.adminerDir, "adminer.php");
            
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fileStream = File.Create(phpPath);
            await response.Content.CopyToAsync(fileStream);
            
            Console.WriteLine($"Adminer {version} instalado em {DevStackConfig.adminerDir}. Abra o arquivo PHP no navegador.");
        }

        public static void InstallPoetry(string? version = null)
        {
            Console.WriteLine("Instalando Poetry via pip...");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "pip",
                Arguments = "install --upgrade poetry",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            Console.WriteLine("Poetry instalado globalmente.");
        }

        public static async Task InstallRuby(string? version = null)
        {
            version ??= GetLatestRubyVersion();
            string subDir = $"ruby-{version}";
            string zipUrl = $"https://github.com/oneclick/rubyinstaller2/releases/download/RubyInstaller-{version}/rubyinstaller-{version}-x64.7z";
            await InstallGenericTool(DevStackConfig.rubyDir, version, zipUrl, subDir, Path.Combine("bin", "ruby.exe"), "ruby");
            Console.WriteLine($"Ruby {version} instalado.");
        }

        public static async Task InstallGo(string? version = null)
        {
            version ??= GetLatestGoVersion();
            string subDir = $"go-{version}";
            string zipUrl = $"https://go.dev/dl/go{version}.windows-amd64.zip";
            await InstallGenericTool(DevStackConfig.goDir, version, zipUrl, subDir, Path.Combine("bin", "go.exe"), "go");
            Console.WriteLine($"Go {version} instalado.");
        }

        public static void InstallCertbot(string? version = null)
        {
            Console.WriteLine("Instalando Certbot via pip...");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "pip",
                Arguments = "install --upgrade certbot",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            Console.WriteLine("Certbot instalado globalmente.");
        }

        public static async Task InstallOpenSSL(string? version = null, string arch = "x64")
        {
            version ??= GetLatestOpenSSLVersion();
            string archPrefix = arch == "x86" ? "Win32OpenSSL" : "Win64OpenSSL";
            string subDir = $"openssl-{version}";
            string versionUnderscore = version.Replace(".", "_");
            string installerName = $"{archPrefix}-{versionUnderscore}.exe";
            string installerUrl = $"https://slproweb.com/download/{installerName}";
            
            Console.WriteLine(installerUrl);
            string installDir = Path.Combine("C:", "devstack", "openssl", subDir);
            string installerPath = Path.Combine(DevStackConfig.tmpDir, installerName);
            
            if (!Directory.Exists(DevStackConfig.tmpDir))
            {
                Directory.CreateDirectory(DevStackConfig.tmpDir);
            }

            Console.WriteLine($"Baixando instalador do OpenSSL {version} ({arch})...");
            using var response = await httpClient.GetAsync(installerUrl);
            response.EnsureSuccessStatusCode();
            await using var fileStream = File.Create(installerPath);
            await response.Content.CopyToAsync(fileStream);
            fileStream.Close();

            Console.WriteLine($"Executando instalador do OpenSSL {version} ({arch})...");
            if (!Directory.Exists(installDir))
            {
                Directory.CreateDirectory(installDir);
            }

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = $"/silent /DIR=\"{installDir}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            
            File.Delete(installerPath);
            Console.WriteLine($"OpenSSL {version} ({arch}) instalado via instalador em {installDir}");
        }

        public static async Task InstallPHPCsFixer(string? version = null)
        {
            version ??= GetLatestPHPCsFixerVersion();
            string phpCsFixerSubDir = $"php-cs-fixer-{version}";
            string toolDir = Path.Combine(DevStackConfig.phpcsfixerDir, phpCsFixerSubDir);
            
            if (Directory.Exists(toolDir))
            {
                Console.WriteLine($"PHP CS Fixer {version} já está instalado.");
                return;
            }

            Console.WriteLine($"Baixando PHP CS Fixer {version}...");
            Directory.CreateDirectory(toolDir);
            
            string url = $"https://github.com/FriendsOfPHP/PHP-CS-Fixer/releases/download/v{version}/php-cs-fixer.phar";
            string pharPath = Path.Combine(toolDir, $"php-cs-fixer-{version}.phar");
            
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fileStream = File.Create(pharPath);
            await response.Content.CopyToAsync(fileStream);
            
            Console.WriteLine($"PHP CS Fixer {version} instalado em {toolDir}. Use 'php-cs-fixer' no terminal.");
            DevStackConfig.WriteLog($"PHP CS Fixer {version} instalado em {toolDir}");
        }

        public static void CreateNginxSiteConfig(string domain, string? root, string? phpUpstream, string? nginxVersion)
        {
            nginxVersion ??= GetLatestNginxVersion();
            string nginxVersionDir = Path.Combine(DevStackConfig.nginxDir, $"nginx-{nginxVersion}");
            string nginxSitesDirFull = Path.Combine(nginxVersionDir, DevStackConfig.nginxSitesDir);

            if (!Directory.Exists(nginxVersionDir))
            {
                throw new Exception($"A versão do Nginx ({nginxVersion}) não está instalada em {nginxVersionDir}.");
            }

            if (!Directory.Exists(nginxSitesDirFull))
            {
                Directory.CreateDirectory(nginxSitesDirFull);
            }

            phpUpstream ??= "127.0.0.1:9000";

            if (string.IsNullOrEmpty(root) && Directory.Exists(Path.Combine("C:", "Workspace", domain)))
            {
                root = Path.Combine("C:", "Workspace", domain);
            }

            string confPath = Path.Combine(nginxSitesDirFull, $"{domain}.conf");
            string serverName = $"{domain}.localhost";
            string rootPath = root ?? "";

            string template = $@"server {{

    listen 80;
    listen [::]:80;

    server_name {serverName};
    root {rootPath};
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

            File.WriteAllText(confPath, template, new UTF8Encoding(false));
            Console.WriteLine($"Arquivo {confPath} criado/configurado com sucesso!");

            string hostsPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot")!, "System32", "drivers", "etc", "hosts");
            string entry = $"127.0.0.1\t{serverName}";
            
            try
            {
                string[] hostsContent = File.Exists(hostsPath) ? File.ReadAllLines(hostsPath) : Array.Empty<string>();
                if (!hostsContent.Contains(entry))
                {
                    File.AppendAllText(hostsPath, Environment.NewLine + entry, Encoding.UTF8);
                    Console.WriteLine($"Adicionado {serverName} ao arquivo hosts.");
                }
                else
                {
                    Console.WriteLine($"{serverName} já está presente no arquivo hosts.");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao modificar arquivo hosts. Execute como administrador.");
            }
        }

        // Additional helper methods for getting latest versions of remaining tools
        private static string GetLatestPgSQLVersion()
        {
            var versionData = DataManager.GetPgSQLVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do PostgreSQL.");
        }

        private static string GetLatestMailHogVersion()
        {
            var versionData = DataManager.GetMailHogVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do MailHog.");
        }

        private static string GetLatestElasticsearchVersion()
        {
            var versionData = DataManager.GetElasticsearchVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Elasticsearch.");
        }

        private static string GetLatestMemcachedVersion()
        {
            var versionData = DataManager.GetMemcachedVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Memcached.");
        }

        private static string GetLatestDockerVersion()
        {
            var versionData = DataManager.GetDockerVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Docker.");
        }

        private static string GetLatestYarnVersion()
        {
            var versionData = DataManager.GetYarnVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Yarn.");
        }

        private static string GetLatestPnpmVersion()
        {
            var versionData = DataManager.GetPnpmVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do pnpm.");
        }

        private static string GetLatestWPCLIVersion()
        {
            var versionData = DataManager.GetWPCLIVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do WP-CLI.");
        }

        private static string GetLatestAdminerVersion()
        {
            var versionData = DataManager.GetAdminerVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Adminer.");
        }

        private static string GetLatestPoetryVersion()
        {
            var versionData = DataManager.GetPoetryVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Poetry.");
        }

        private static string GetLatestRubyVersion()
        {
            var versionData = DataManager.GetRubyVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Ruby.");
        }

        private static string GetLatestGoVersion()
        {
            var versionData = DataManager.GetGoVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Go.");
        }

        private static string GetLatestCertbotVersion()
        {
            var versionData = DataManager.GetCertbotVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do Certbot.");
        }

        private static string GetLatestPHPCsFixerVersion()
        {
            var versionData = DataManager.GetPHPCsFixerVersions();
            if (versionData.Status == "success" && versionData.Versions.Any())
            {
                return versionData.Versions.First();
            }
            throw new Exception("Não foi possível obter a última versão do PHP CS Fixer.");
        }
    }
}
