using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public abstract class ComponentBase : ComponentInterface
    {
        private static readonly HttpClient _httpClient;
        private const int FILE_DELETE_RETRY_COUNT = 5;
        private const int FILE_DELETE_RETRY_DELAY_MS = 200;
        private const int POST_INSTALL_DELAY_MS = 100;

        static ComponentBase()
        {
            _httpClient = CreateConfiguredHttpClient();
        }

        private static HttpClient CreateConfiguredHttpClient()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer(),
                AllowAutoRedirect = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan
            };
            client.DefaultRequestHeaders.Add("User-Agent", "DevStackManager");
            
            return client;
        }

        private static HttpRequestMessage CreateBrowserGetRequest(string url, string? referer = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            ConfigureBrowserHeaders(request, referer);
            return request;
        }

        private static void ConfigureBrowserHeaders(HttpRequestMessage request, string? referer)
        {
            request.Headers.Accept.ParseAdd("*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            
            if (!string.IsNullOrEmpty(referer))
            {
                try { request.Headers.Referrer = new Uri(referer); } 
                catch { /* Ignore invalid referer */ }
            }
            
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "no-cors");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            request.Headers.Connection.ParseAdd("keep-alive");
        }

        private static async Task<HttpResponseMessage> SendBrowserRequestAsync(string url, bool doPreflight = true)
        {
            if (doPreflight)
            {
                await TryPreflightRequest(url);
            }

            var baseUri = new Uri(url).GetLeftPart(UriPartial.Authority);
            var request = CreateBrowserGetRequest(url, baseUri);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            return response;
        }

        private static async Task TryPreflightRequest(string url)
        {
            try
            {
                var baseUri = new Uri(url).GetLeftPart(UriPartial.Authority);
                using var preRequest = CreateBrowserGetRequest(baseUri);
                await _httpClient.SendAsync(preRequest, HttpCompletionOption.ResponseHeadersRead);
            }
            catch { /* Preflight failures are acceptable */ }
        }

        // Abstract properties - must be implemented by derived classes
        public abstract string Name { get; }
        public abstract string Label { get; }
        public abstract string ToolDir { get; }

        // Virtual properties with default values
        public virtual bool IsService => false;
        public virtual bool IsExecutable => false;
        public virtual bool IsCommandLine => false;
        public virtual string? ExecutableFolder => string.Empty;
        public virtual string? ExecutablePattern => string.Empty;
        public virtual string? ServicePattern => string.Empty;
        public virtual string? SubDirectory => null;
        public virtual bool IsArchive => true;
        public virtual bool RunInstaller => false;
        public virtual string? GetInstallerArgs(string version) => null;
        public virtual string? CreateBinShortcut => null;
        public virtual int? MaxWorkers => null;

        public virtual string GetServiceType(DevStackShared.LocalizationManager localizationManager)
        {
            var serviceTypes = new Dictionary<string, string>
            {
                ["nginx"] = "gui.services_tab.types.web_server",
                ["php"] = "gui.services_tab.types.php_fpm",
                ["mysql"] = "gui.services_tab.types.database",
                ["mongodb"] = "gui.services_tab.types.database",
                ["pgsql"] = "gui.services_tab.types.database",
                ["elasticsearch"] = "gui.services_tab.types.search_engine"
            };

            var componentName = Name.ToLowerInvariant();
            var typeKey = serviceTypes.GetValueOrDefault(componentName, "gui.services_tab.types.service");
            
            return localizationManager.GetString(typeKey);
        }

        public virtual string GetServiceDescription(string version, DevStackShared.LocalizationManager localizationManager)
        {
            var componentName = Name.ToLowerInvariant();
            
            return componentName switch
            {
                "php" => $"PHP {version} {localizationManager.GetString("gui.services_tab.types.fastcgi")}",
                _ => $"{Name} {version}"
            };
        }

        public virtual Task PostInstall(string version, string targetDir) => Task.CompletedTask;

        // Version management methods
        public string GetLatestVersion()
        {
            var available = ListAvailable();
            if (!available.Any())
            {
                throw new InvalidOperationException($"Não foi possível obter a última versão do {Name}.");
            }
            
            return available.Last();
        }

        public List<string> ListAvailable()
        {
            var versionInfos = GetVersions();
            return versionInfos?.Select(v => v.Version).ToList() ?? new List<string>();
        }

        public virtual List<string> ListInstalled()
        {
            var baseDir = Path.Combine(DevStackConfig.baseDir, Name);
            if (!Directory.Exists(baseDir))
            {
                return new List<string>();
            }

            var prefix = $"{Name}-";
            try
            {
                return Directory.GetDirectories(baseDir, $"{prefix}*")
                    .Select(Path.GetFileName)
                    .Where(dirName => !string.IsNullOrEmpty(dirName) && dirName.StartsWith(prefix))
                    .Select(dirName => dirName!.Substring(prefix.Length))
                    .OrderBy(v => v)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public string GetUrlForVersion(string version)
        {
            var versionInfo = DevStackShared.AvailableVersions.Providers.VersionRegistry.GetVersion(Name, version);
            if (versionInfo == null)
            {
                throw new InvalidOperationException($"URL para a versão {version} do {Name} não encontrada.");
            }
            
            return versionInfo.Url;
        }

        private List<DevStackShared.AvailableVersions.Models.VersionInfo>? GetVersions()
        {
            return DevStackShared.AvailableVersions.Providers.VersionRegistry.GetAvailableVersions(Name);
        }

        public void Uninstall(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine($"Erro: Versão do {Name} deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine($"Exemplo: uninstall {Name} <version>", ConsoleColor.Yellow);
                return;
            }
            
            var subDir = $"{Name}-{version}";
            UninstallGenericTool(Path.Combine(DevStackConfig.baseDir, Name), subDir);
        }

        public static async Task<bool> InstallGenericTool(
            string toolDir,
            string version,
            string url,
            string? subDir = null,
            string? exeRelativePath = null,
            string? prefix = null,
            bool isArchive = true,
            bool runInstaller = false,
            string? installerArgs = null)
        {
            prefix ??= Path.GetFileNameWithoutExtension(toolDir)?.ToLowerInvariant() ?? "tool";
            subDir ??= $"{prefix}-{version}";

            var targetDir = Path.Combine(toolDir, subDir);
            
            if (await CheckAlreadyInstalled(targetDir, prefix, version))
            {
                return true;
            }

            EnsureDirectoryExists(toolDir);
            LogDownloadStart(prefix, version);
            ValidateUrl(url);

            var downloadFileName = GetDownloadFileName(url, prefix, version);
            var downloadPath = Path.Combine(toolDir, downloadFileName);
            
            bool rollback = false;
            try
            {
                if (ShouldRunInstaller(runInstaller, url, installerArgs))
                {
                    rollback = await InstallViaInstaller(url, downloadPath, targetDir, toolDir, installerArgs, prefix, version);
                }
                else if (ShouldExtractArchive(isArchive, url))
                {
                    rollback = await InstallViaArchive(url, toolDir, targetDir, prefix, version);
                }
                else
                {
                    rollback = await InstallAsDirectDownload(url, targetDir, downloadFileName, prefix, version);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogInstallError(prefix, version, ex);
                if (rollback && Directory.Exists(targetDir))
                {
                    TryDeleteDirectory(targetDir);
                }
                throw;
            }
        }

        private static Task<bool> CheckAlreadyInstalled(string targetDir, string prefix, string version)
        {
            if (Directory.Exists(targetDir))
            {
                Console.WriteLine(GetLocalizedString("shared.install.already_installed", 
                    $"{prefix} {version} já está instalado.", prefix, version));
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void LogDownloadStart(string prefix, string version)
        {
            Console.WriteLine(GetLocalizedString("shared.install.downloading", 
                $"Baixando {prefix} {version}...", prefix, version));
        }

        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL inválida para download.", nameof(url));
            }
        }

        private static string GetDownloadFileName(string url, string prefix, string version)
        {
            var fileName = Path.GetFileName(new Uri(url).LocalPath);
            return string.IsNullOrEmpty(fileName) ? $"{prefix}-{version}" : fileName;
        }

        private static bool ShouldRunInstaller(bool runInstaller, string url, string? installerArgs)
        {
            return runInstaller || (url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && installerArgs != null);
        }

        private static bool ShouldExtractArchive(bool isArchive, string url)
        {
            return isArchive || url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<bool> InstallViaInstaller(
            string url, 
            string downloadPath, 
            string targetDir, 
            string toolDir, 
            string? installerArgs, 
            string prefix, 
            string version)
        {
            await DownloadFile(url, downloadPath);
            
            EnsureDirectoryExists(targetDir);

            var args = installerArgs ?? string.Empty;
            Console.WriteLine(GetLocalizedString("shared.install.running_installer", 
                $"Executando instalador {Path.GetFileName(downloadPath)} {args}...", 
                Path.GetFileName(downloadPath), args));
            
            await ProcessManager.ExecuteProcessAsync(downloadPath, args, toolDir);

            TryDeleteFile(downloadPath);

            Console.WriteLine(GetLocalizedString("shared.install.installed_via_installer", 
                $"{prefix} {version} instalado via instalador em {targetDir}", 
                prefix, version, targetDir));
            
            return true;
        }

        private static async Task<bool> InstallViaArchive(
            string url, 
            string toolDir, 
            string targetDir, 
            string prefix, 
            string version)
        {
            var zipPath = Path.Combine(toolDir, $"{prefix}-{version}.zip");

            await DownloadFile(url, zipPath);

            Console.WriteLine(GetLocalizedString("shared.install.extracting", "Extraindo..."));
            
            await ExtractAndFlattenArchive(zipPath, targetDir);
            await DeleteFileWithRetry(zipPath);

            Console.WriteLine(GetLocalizedString("shared.install.installed", 
                $"{prefix} {version} instalado.", prefix, version));
            
            return true;
        }

        private static async Task<bool> InstallAsDirectDownload(
            string url, 
            string targetDir, 
            string downloadFileName, 
            string prefix, 
            string version)
        {
            EnsureDirectoryExists(targetDir);

            var destFile = Path.Combine(targetDir, downloadFileName);
            await DownloadFile(url, destFile);

            Console.WriteLine(GetLocalizedString("shared.install.installed_in", 
                $"{prefix} {version} instalado em {targetDir}.", prefix, version, targetDir));
            
            return true;
        }

        private static async Task DownloadFile(string url, string destinationPath)
        {
            using var response = await SendBrowserRequestAsync(url);
            await using var fs = File.Create(destinationPath);
            await response.Content.CopyToAsync(fs);
        }

        private static async Task ExtractAndFlattenArchive(string zipPath, string targetDir)
        {
            string? topFolder = null;
            
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                topFolder = GetTopLevelFolderFromArchive(archive);
                archive.ExtractToDirectory(targetDir, true);
            }

            await Task.Delay(POST_INSTALL_DELAY_MS);

            if (!string.IsNullOrEmpty(topFolder))
            {
                FlattenExtractedFolder(targetDir, topFolder);
            }
        }

        private static string? GetTopLevelFolderFromArchive(ZipArchive archive)
        {
            var firstEntry = archive.Entries.FirstOrDefault();
            if (firstEntry != null)
            {
                var match = Regex.Match(firstEntry.FullName, @"^([^/\\]+)[/\\]");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return null;
        }

        private static void FlattenExtractedFolder(string targetDir, string topFolder)
        {
            var extractedPath = Path.Combine(targetDir, topFolder);
            if (!Directory.Exists(extractedPath))
            {
                return;
            }

            foreach (var item in Directory.GetFileSystemEntries(extractedPath))
            {
                var destPath = Path.Combine(targetDir, Path.GetFileName(item));
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

        private static async Task DeleteFileWithRetry(string filePath)
        {
            for (int attempt = 0; attempt < FILE_DELETE_RETRY_COUNT; attempt++)
            {
                try
                {
                    File.Delete(filePath);
                    break;
                }
                catch (IOException) when (attempt < FILE_DELETE_RETRY_COUNT - 1)
                {
                    await Task.Delay(FILE_DELETE_RETRY_DELAY_MS);
                }
            }
        }

        private static void TryDeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Ignore deletion errors
            }
        }

        private static void TryDeleteDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch
            {
                // Ignore deletion errors
            }
        }

        private static void LogInstallError(string prefix, string version, Exception ex)
        {
            Console.WriteLine(GetLocalizedString("shared.install.error_installing", 
                $"Erro ao instalar {prefix} {version}: {ex.Message}", prefix, version, ex.Message));
        }

        private static string GetLocalizedString(string key, string fallback, params object[] args)
        {
            return DevStackShared.LocalizationManager.Instance?.GetString(key, args) ?? fallback;
        }

        public virtual async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            var url = GetUrlForVersion(version);
            var baseToolDir = GetBaseToolDirectory();
            var subDir = SubDirectory ?? $"{Name}-{version}";
            
            var (toolDir, exeRelativePath) = ResolveInstallationPaths(baseToolDir, version);

            await InstallGenericTool(
                toolDir, version, url, subDir, exeRelativePath, Name, 
                IsArchive, RunInstaller, GetInstallerArgs(version));

            var targetDir = Path.Combine(toolDir, subDir);
            await PostInstall(version, targetDir);

            TryCreateShortcutIfNeeded(version, targetDir);

            Console.WriteLine(GetLocalizedString("shared.install.component_installed", 
                $"{Name} {version} instalado.", Name, version));
        }

        private string GetBaseToolDirectory()
        {
            return !string.IsNullOrEmpty(ToolDir) 
                ? ToolDir 
                : Path.Combine(DevStackConfig.baseDir, Name);
        }

        private (string toolDir, string? exeRelativePath) ResolveInstallationPaths(string baseToolDir, string version)
        {
            if (string.IsNullOrEmpty(ExecutableFolder))
            {
                return (baseToolDir, GetExecutablePattern(version));
            }

            if (Path.IsPathRooted(ExecutableFolder))
            {
                // Legacy behavior: ExecutableFolder contains absolute path
                return (ExecutableFolder, GetExecutablePattern(version));
            }

            var exePath = !string.IsNullOrEmpty(ExecutablePattern)
                ? Path.Combine(ExecutableFolder, ExecutablePattern.Replace("{version}", version))
                : null;

            return (baseToolDir, exePath);
        }

        private string? GetExecutablePattern(string version)
        {
            return !string.IsNullOrEmpty(ExecutablePattern) 
                ? ExecutablePattern.Replace("{version}", version) 
                : null;
        }

        private void TryCreateShortcutIfNeeded(string version, string targetDir)
        {
            if (!IsExecutable || string.IsNullOrEmpty(CreateBinShortcut))
            {
                return;
            }

            try
            {
                var (sourceDir, sourcePattern) = ResolveShortcutSource(version, targetDir);
                var shortcutName = CreateBinShortcut.Replace("{version}", version);
                
                CreateGlobalBinShortcut(sourceDir, sourcePattern, version, Name, shortcutName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(GetLocalizedString("shared.install.shortcut_creation_failed", 
                    $"Aviso: falha ao criar atalho: {ex.Message}", ex.Message));
            }
        }

        private (string sourceDir, string sourcePattern) ResolveShortcutSource(string version, string targetDir)
        {
            if (!string.IsNullOrEmpty(ExecutablePattern))
            {
                var sourceDir = ResolveShortcutSourceDirectory(targetDir);
                return (sourceDir, ExecutablePattern);
            }

            // Fallback: use CreateBinShortcut as source pattern
            var shortcutName = CreateBinShortcut!.Replace("{version}", version);
            return (targetDir, shortcutName);
        }

        private string ResolveShortcutSourceDirectory(string targetDir)
        {
            if (string.IsNullOrEmpty(ExecutableFolder))
            {
                return targetDir;
            }

            return Path.IsPathRooted(ExecutableFolder) 
                ? ExecutableFolder 
                : Path.Combine(targetDir, ExecutableFolder);
        }

        public static void CreateGlobalBinShortcut(
            string sourceDir, 
            string sourcePattern, 
            string version, 
            string componentName, 
            string? shortcutName = null)
        {
            var sourceFile = Path.Combine(sourceDir, sourcePattern);
            if (!File.Exists(sourceFile))
            {
                Console.WriteLine(GetLocalizedString("shared.shortcuts.file_not_found", 
                    $"Aviso: arquivo {sourcePattern} não encontrado para criar atalho", sourcePattern));
                return;
            }

            var globalBinDir = Path.Combine(DevStackConfig.baseDir, "bin");
            EnsureDirectoryExists(globalBinDir);

            var finalShortcutName = BuildShortcutFileName(shortcutName, version, componentName);
            var shortcutPath = Path.Combine(globalBinDir, finalShortcutName);

            TryCreateShortcutFile(shortcutPath, sourceFile, finalShortcutName);
        }

        private static string BuildShortcutFileName(string? shortcutName, string version, string componentName)
        {
            if (!string.IsNullOrEmpty(shortcutName))
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(shortcutName.Replace("{version}", version));
                return $"{nameWithoutExtension}.cmd";
            }

            return $"{componentName}-{version}.cmd";
        }

        private static void TryCreateShortcutFile(string shortcutPath, string sourceFile, string finalShortcutName)
        {
            try
            {
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                var cmdContent = $"@echo off\nsetlocal\n\"{sourceFile}\" %*\nendlocal";
                File.WriteAllText(shortcutPath, cmdContent, System.Text.Encoding.ASCII);

                var nameWithoutExtension = Path.GetFileNameWithoutExtension(shortcutPath);
                Console.WriteLine(GetLocalizedString("shared.shortcuts.created", 
                    $"Atalho {nameWithoutExtension} criado apontando para {sourceFile}", 
                    nameWithoutExtension, sourceFile));
            }
            catch (Exception ex)
            {
                Console.WriteLine(GetLocalizedString("shared.shortcuts.error_creating", 
                    $"Erro ao criar atalho: {ex.Message}", ex.Message));
            }
        }

        public static void RemoveGlobalBinShortcut(string componentName, string version, string shortcutPattern)
        {
            try
            {
                var globalBinDir = Path.Combine(DevStackConfig.baseDir, "bin");
                if (!Directory.Exists(globalBinDir))
                {
                    return;
                }

                var finalShortcutName = BuildShortcutFileName(shortcutPattern, version, componentName);
                var shortcutPath = Path.Combine(globalBinDir, finalShortcutName);

                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                    Console.WriteLine(GetLocalizedString("shared.shortcuts.removed", 
                        $"Atalho {finalShortcutName} removido", finalShortcutName));
                }
                else
                {
                    Console.WriteLine(GetLocalizedString("shared.shortcuts.not_found", 
                        $"Atalho {finalShortcutName} não encontrado para remoção", finalShortcutName));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(GetLocalizedString("shared.shortcuts.error_removing", 
                    $"Erro ao remover atalho: {ex.Message}", ex.Message));
                DevStackConfig.WriteLog($"Erro ao remover atalho global: {ex}");
            }
        }

        public static void UninstallGenericTool(string toolDir, string subDir)
        {
            var targetDir = Path.Combine(toolDir, subDir);
            
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
                DevStackConfig.pathManager?.RemoveFromPath(new[] { targetDir });
                DevStackConfig.WriteColoredLine($"{subDir} removido.", ConsoleColor.Green);
            }
            else
            {
                DevStackConfig.WriteColoredLine($"{subDir} não está instalado.", ConsoleColor.Yellow);
            }
        }
    }
}
