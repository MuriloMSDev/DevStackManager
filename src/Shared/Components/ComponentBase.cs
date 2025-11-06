using System;
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
        public static readonly HttpClient httpClient;

        static ComponentBase()
        {
            // Use a handler with CookieContainer so cookies set by preflight are preserved for subsequent requests.
            var handler = new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer(),
                AllowAutoRedirect = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DevStackManager");
            httpClient.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        }

        // Create a HttpRequestMessage with headers similar to a browser to reduce anti-bot blocking
        public static HttpRequestMessage CreateBrowserGetRequest(string url, string? referer = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.ParseAdd("*/*");
            req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            if (!string.IsNullOrEmpty(referer))
            {
                try { req.Headers.Referrer = new Uri(referer); } catch { }
            }
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "no-cors");
            req.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            req.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            req.Headers.Connection.ParseAdd("keep-alive");
            return req;
        }

        // Send a GET with browser-like headers. Optionally perform a lightweight preflight to the host
        // using ResponseHeadersRead to minimize body transfer during preflight.
        public static async Task<HttpResponseMessage> SendBrowserRequestAsync(string url, bool doPreflight = true)
        {
            if (doPreflight)
            {
                try
                {
                    var baseUri = new Uri(url).GetLeftPart(UriPartial.Authority);
                    using var preReq = CreateBrowserGetRequest(baseUri);
                    await httpClient.SendAsync(preReq, HttpCompletionOption.ResponseHeadersRead);
                }
                catch { }
            }

            var baseUriForReferer = new Uri(url).GetLeftPart(UriPartial.Authority);
            var req = CreateBrowserGetRequest(url, baseUriForReferer);

            var resp = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            // Keep behavior consistent: callers often call EnsureSuccessStatusCode, but ensure here too
            resp.EnsureSuccessStatusCode();
            return resp;
        }

        public abstract string Name { get; }
        public abstract string ToolDir { get; }
        public virtual bool IsService => false;
        public virtual bool IsExecutable => false;
        public virtual bool IsCommandLine => false;
        public virtual string? ExecutableFolder => "";
        public virtual string? ExecutablePattern => "";
        public virtual string? ServicePattern => "";
        public virtual string? SubDirectory => null;
        public virtual bool IsArchive => true;
        public virtual bool RunInstaller => false;
        public virtual string? GetInstallerArgs(string version) => null;
        public virtual string? CreateBinShortcut => null;
        public virtual int? MaxWorkers => null;

        /// <summary>
        /// Obtém o tipo de serviço baseado no nome do componente
        /// </summary>
        public virtual string GetServiceType(DevStackShared.LocalizationManager localizationManager)
        {
            return Name.ToLowerInvariant() switch
            {
                "nginx" => localizationManager.GetString("gui.services_tab.types.web_server"),
                "php" => localizationManager.GetString("gui.services_tab.types.php_fpm"),
                "mysql" => localizationManager.GetString("gui.services_tab.types.database"),
                "mongodb" => localizationManager.GetString("gui.services_tab.types.database"),
                "pgsql" => localizationManager.GetString("gui.services_tab.types.database"),
                "elasticsearch" => localizationManager.GetString("gui.services_tab.types.search_engine"),
                _ => localizationManager.GetString("gui.services_tab.types.service")
            };
        }

        /// <summary>
        /// Obtém a descrição do serviço
        /// </summary>
        public virtual string GetServiceDescription(string version, DevStackShared.LocalizationManager localizationManager)
        {
            return Name.ToLowerInvariant() switch
            {
                "php" => $"PHP {version} {localizationManager.GetString("gui.services_tab.types.fastcgi")}",
                "nginx" => $"Nginx {version}",
                "mysql" => $"MySQL {version}",
                "mongodb" => $"MongoDB {version}",
                "pgsql" => $"PostgreSQL {version}",
                "elasticsearch" => $"Elasticsearch {version}",
                _ => $"{Name} {version}"
            };
        }

        public virtual Task PostInstall(string version, string targetDir)
        {
            return Task.CompletedTask;
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
            string subDir = $"{Name}-{version}";
            UninstallGenericTool(Path.Combine(DevStackConfig.baseDir, Name), subDir);
        }

        public string GetUrlForVersion(string version)
        {
            var versionInfo = DevStackShared.AvailableVersions.Providers.VersionRegistry.GetVersion(Name, version);
            if (versionInfo != null)
            {
                return versionInfo.Url;
            }
            throw new System.Exception($"URL para a versão {version} do {Name} não encontrada.");
        }

        public List<string> ListAvailable()
        {
            var versionInfos = GetVersions();
            if (versionInfos != null)
            {
                return versionInfos.Select(v => v.Version).ToList();
            }
            return new List<string>();
        }

        public string GetLatestVersion()
        {
            var available = ListAvailable();
            if (available.Any())
                return available.Last();
            throw new System.Exception($"Não foi possível obter a última versão do {Name}.");
        }
        
        public virtual List<string> ListInstalled()
        {
            var versions = new List<string>();
            string? baseDir = Path.Combine(DevStackConfig.baseDir, Name);
            if (string.IsNullOrEmpty(baseDir) || !System.IO.Directory.Exists(baseDir))
                return versions;
            var prefix = $"{Name}-";
            try
            {
                var directories = System.IO.Directory.GetDirectories(baseDir, $"{prefix}*");
                foreach (var directory in directories)
                {
                    var dirName = System.IO.Path.GetFileName(directory);
                    if (dirName.StartsWith(prefix))
                    {
                        var version = dirName.Substring(prefix.Length);
                        versions.Add(version);
                    }
                }
            }
            catch
            {
                // Ignora erros
            }
            return versions.OrderBy(v => v).ToList();
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

            string targetDir = Path.Combine(toolDir, subDir);
            bool rollback = false;
            try
            {
                if (Directory.Exists(targetDir))
                {
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.already_installed", prefix, version) ?? $"{prefix} {version} já está instalado.");
                    return true;
                }
                if (!Directory.Exists(toolDir))
                {
                    Directory.CreateDirectory(toolDir);
                }

                Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.downloading", prefix, version) ?? $"Baixando {prefix} {version}...");

                // Normalize url
                if (string.IsNullOrEmpty(url)) throw new ArgumentException("URL inválida para download.", nameof(url));

                // Decide action based on url extension or flags
                string lowerUrl = url.ToLowerInvariant();
                // Prepare download paths
                string downloadFileName = Path.GetFileName(new Uri(url).LocalPath);
                if (string.IsNullOrEmpty(downloadFileName)) downloadFileName = $"{prefix}-{version}";
                string downloadPath = Path.Combine(toolDir, downloadFileName);

                // Installer flow
                if (runInstaller || lowerUrl.EndsWith(".exe") && installerArgs != null)
                {
                    using (var response = await SendBrowserRequestAsync(url))
                    {
                        await using (var fs = File.Create(downloadPath))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }

                    // Ensure target directory exists
                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                    // Run installer with provided args (powershell wrapper to keep compatibility)
                    string args = installerArgs ?? string.Empty;
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.running_installer", downloadFileName, args) ?? $"Executando instalador {downloadFileName} {args}...");
                    await ProcessManager.ExecuteProcessAsync(downloadPath, args, toolDir);

                    // Try to delete installer
                    try { File.Delete(downloadPath); } catch { }

                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.installed_via_installer", prefix, version, targetDir) ?? $"{prefix} {version} instalado via instalador em {targetDir}");
                    rollback = true;
                }
                else if (isArchive || lowerUrl.EndsWith(".zip"))
                {
                    string zipPath = Path.Combine(toolDir, $"{prefix}-{version}.zip");

                    using (var response = await SendBrowserRequestAsync(url))
                    {
                        await using (var fs = File.Create(zipPath))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }

                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.extracting") ?? "Extraindo...");
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
                        archive.ExtractToDirectory(targetDir, true);
                    }
                    await Task.Delay(100);
                    if (!string.IsNullOrEmpty(topFolder))
                    {
                        string extractedPath = Path.Combine(targetDir, topFolder);
                        if (Directory.Exists(extractedPath))
                        {
                            foreach (var item in Directory.GetFileSystemEntries(extractedPath))
                            {
                                string destPath = Path.Combine(targetDir, Path.GetFileName(item));
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
                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        try
                        {
                            File.Delete(zipPath);
                            break;
                        }
                        catch (IOException) when (attempt < 4)
                        {
                            await Task.Delay(200);
                        }
                    }

                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.installed", prefix, version) ?? $"{prefix} {version} instalado.");
                    rollback = true;
                }
                else
                {
                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                    using var response = await SendBrowserRequestAsync(url);
                    string destFile = Path.Combine(targetDir, downloadFileName);
                    await using (var fs = File.Create(destFile))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.installed_in", prefix, version, targetDir) ?? $"{prefix} {version} instalado em {targetDir}.");
                    rollback = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.error_installing", prefix, version, ex.Message) ?? $"Erro ao instalar {prefix} {version}: {ex.Message}");
                if (rollback && Directory.Exists(targetDir))
                {
                    try { Directory.Delete(targetDir, true); } catch { }
                }
                throw;
            }
        }

        public virtual async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string url = GetUrlForVersion(version);

            string baseToolDir = !string.IsNullOrEmpty(ToolDir) ? ToolDir! : Path.Combine(DevStackConfig.baseDir, Name);

            string subDir = SubDirectory ?? $"{Name}-{version}";
            
            string toolDirArg;
            string? exeRel = null;

            if (!string.IsNullOrEmpty(ExecutableFolder))
            {
                if (Path.IsPathRooted(ExecutableFolder))
                {
                    // legacy behaviour: ExecutableFolder contained the absolute tool directory
                    toolDirArg = ExecutableFolder!;
                    if (!string.IsNullOrEmpty(ExecutablePattern))
                        exeRel = ExecutablePattern.Replace("{version}", version);
                }
                else
                {
                    toolDirArg = baseToolDir;
                    if (!string.IsNullOrEmpty(ExecutablePattern))
                        exeRel = Path.Combine(ExecutableFolder, ExecutablePattern.Replace("{version}", version));
                }
            }
            else
            {
                toolDirArg = baseToolDir;
                if (!string.IsNullOrEmpty(ExecutablePattern))
                    exeRel = ExecutablePattern.Replace("{version}", version);
            }

            bool isArchive = IsArchive;
            bool runInstaller = RunInstaller;
            string? installerArgs = GetInstallerArgs(version);

            await InstallGenericTool(toolDirArg, version, url, subDir, exeRel, Name, isArchive, runInstaller, installerArgs);

            string targetDir = System.IO.Path.Combine(toolDirArg, subDir);
            await PostInstall(version, targetDir);

            bool createBinShortcutFlag = IsExecutable && !string.IsNullOrEmpty(CreateBinShortcut);
            // Handle CreateBinShortcut if specified - usar ExecutablePattern se definido
            if (createBinShortcutFlag && !string.IsNullOrEmpty(CreateBinShortcut))
            {
                try
                {
                    string shortcutName = CreateBinShortcut.Replace("{version}", version);
                    string sourceDir = targetDir;
                    string sourcePattern;
                    
                    // Se ExecutablePattern estiver definido, usar ele como source file
                    if (!string.IsNullOrEmpty(ExecutablePattern))
                    {
                        sourcePattern = ExecutablePattern; // ExecutablePattern já está sem {version}
                        
                        // Se ExecutableFolder estiver definido, usar ele como source directory
                        if (!string.IsNullOrEmpty(ExecutableFolder))
                        {
                            if (Path.IsPathRooted(ExecutableFolder))
                            {
                                sourceDir = ExecutableFolder;
                            }
                            else
                            {
                                sourceDir = Path.Combine(targetDir, ExecutableFolder);
                            }
                        }
                    }
                    else
                    {
                        // Fallback para usar o próprio CreateBinShortcut como source
                        sourcePattern = shortcutName;
                    }
                    
                    CreateGlobalBinShortcut(sourceDir, sourcePattern, version, Name, shortcutName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.shortcut_creation_failed", e.Message) ?? $"Aviso: falha ao criar atalho: {e.Message}");
                }
            }

            Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.install.component_installed", Name, version) ?? $"{Name} {version} instalado.");
        }

        public static void CreateGlobalBinShortcut(string sourceDir, string sourcePattern, string version, string componentName, string? shortcutName = null)
        {
            string sourceFile = Path.Combine(sourceDir, sourcePattern);
            if (File.Exists(sourceFile))
            {
                // Usar pasta bin geral no DevStackConfig.baseDir
                string globalBinDir = Path.Combine(DevStackConfig.baseDir, "bin");
                if (!Directory.Exists(globalBinDir)) Directory.CreateDirectory(globalBinDir);
                
                // Determinar o nome do atalho (com extensão .cmd)
                string shortcutName_final;
                if (!string.IsNullOrEmpty(shortcutName))
                {
                    // Remove extensão se fornecida no shortcutName e adiciona .cmd
                    shortcutName_final = Path.GetFileNameWithoutExtension(shortcutName.Replace("{version}", version)) + ".cmd";
                }
                else
                {
                    shortcutName_final = $"{componentName}-{version}.cmd";
                }
                
                string shortcutPath = Path.Combine(globalBinDir, shortcutName_final);
                
                try
                {
                    // Remover atalho existente se houver
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                    }
                    
                    // Criar script .cmd (funciona no Windows quando está no PATH e pode ser chamado sem extensão)
                    string cmdContent = $"@echo off\nsetlocal\n\"{sourceFile}\" %*\nendlocal";
                    
                    File.WriteAllText(shortcutPath, cmdContent, System.Text.Encoding.ASCII);
                    
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(shortcutPath);
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.shortcuts.created", nameWithoutExt, sourceFile) ?? $"Atalho {nameWithoutExt} criado apontando para {sourceFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.shortcuts.error_creating", ex.Message) ?? $"Erro ao criar atalho: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.shortcuts.file_not_found", sourcePattern) ?? $"Aviso: arquivo {sourcePattern} não encontrado para criar atalho");
            }
        }
        
        public static void UninstallGenericTool(string toolDir, string subDir)
        {
            string targetDir = Path.Combine(toolDir, subDir);
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

        /// <summary>
        /// Remove um atalho global criado anteriormente
        /// </summary>
        public static void RemoveGlobalBinShortcut(string componentName, string version, string shortcutPattern)
        {
            try
            {
                string globalBinDir = Path.Combine(DevStackConfig.baseDir, "bin");
                
                if (!Directory.Exists(globalBinDir))
                {
                    return; // Não há diretório bin, não há atalhos para remover
                }

                // Processar o padrão do atalho (com extensão .cmd)
                string finalShortcutName = shortcutPattern.Replace("{version}", version);
                
                // Remove extensão se tiver e adiciona .cmd
                finalShortcutName = Path.GetFileNameWithoutExtension(finalShortcutName) + ".cmd";
                
                string shortcutPath = Path.Combine(globalBinDir, finalShortcutName);
                
                // Remover atalho se existir
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.shortcuts.removed", finalShortcutName) ?? $"Atalho {finalShortcutName} removido");
                }
                else
                {
                    Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.shortcuts.not_found", finalShortcutName) ?? $"Atalho {finalShortcutName} não encontrado para remoção");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.shortcuts.error_removing", ex.Message) ?? $"Erro ao remover atalho: {ex.Message}");
                DevStackConfig.WriteLog($"Erro ao remover atalho global: {ex}");
            }
        }
    }
}
