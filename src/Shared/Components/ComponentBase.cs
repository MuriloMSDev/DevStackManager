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
        public virtual string? SubDirectory => null;
        public virtual bool IsArchive => true;
        public virtual bool RunInstaller => false;
        public virtual string? GetInstallerArgs(string version) => null;
        public virtual bool CreateBinShortcut => true;
        public virtual bool RenameExeAfterInstall => true;

        public virtual Task PostInstall(string version, string targetDir)
        {
            return Task.CompletedTask;
        }

        private IEnumerable<System.Text.Json.JsonElement>? GetJsonItems()
        {
            var assembly = GetType().Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            string? resourceName = resourceNames.FirstOrDefault(n => n.Contains($"{Name}.json"));
            if (!string.IsNullOrEmpty(resourceName))
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new System.IO.StreamReader(stream);
                    var json = reader.ReadToEnd();
                    return System.Text.Json.JsonDocument.Parse(json).RootElement.EnumerateArray().ToList();
                }
            }
            return null;
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
            var items = GetJsonItems();
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item.TryGetProperty("version", out var v) && v.GetString() == version)
                    {
                        if (item.TryGetProperty("url", out var urlProp))
                            return urlProp.GetString() ?? throw new System.Exception($"URL para a versão {version} do {Name} não encontrada.");
                    }
                }
            }
            throw new System.Exception($"URL para a versão {version} do {Name} não encontrada.");
        }

        public List<string> ListAvailable()
        {
            var versions = new List<string>();
            var items = GetJsonItems();
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item.TryGetProperty("version", out var v))
                        versions.Add(v.GetString() ?? "");
                }
            }
            return versions;
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
            bool renameExe = true,
            bool isArchive = true,
            bool runInstaller = false,
            string? installerArgs = null,
            bool createBinShortcut = false)
        {
            prefix ??= Path.GetFileNameWithoutExtension(toolDir)?.ToLowerInvariant() ?? "tool";
            subDir ??= $"{prefix}-{version}";

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
                    Console.WriteLine($"Executando instalador {downloadFileName} {args}...");
                    await ProcessManager.ExecuteProcessAsync(downloadPath, args, toolDir);

                    // Try to delete installer
                    try { File.Delete(downloadPath); } catch { }

                    Console.WriteLine($"{prefix} {version} instalado via instalador em {targetDir}");
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

                    Console.WriteLine("Extraindo...");
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

                    Console.WriteLine($"{prefix} {version} instalado.");
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
                    Console.WriteLine($"{prefix} {version} instalado em {targetDir}.");
                    rollback = true;
                }

                if (renameExe && !string.IsNullOrEmpty(exeRelativePath))
                {
                    string exePath = exeRelativePath.Replace("{version}", version);
                    Console.WriteLine("Renomeando executável principal...");
                    try
                    {
                        RenameMainExe(targetDir, exePath, version, prefix);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Aviso: falha ao renomear executável: {e.Message}");
                    }
                }

                if (createBinShortcut && !string.IsNullOrEmpty(exeRelativePath))
                {
                    try
                    {
                        string exePath = exeRelativePath.Replace("{version}", version);
                        string fullExe = Path.Combine(targetDir, exePath);
                        if (File.Exists(fullExe))
                        {
                            string binDir = Path.Combine(toolDir, "bin");
                            if (!Directory.Exists(binDir)) Directory.CreateDirectory(binDir);
                            string dst = Path.Combine(binDir, $"{prefix}-{version}{Path.GetExtension(fullExe)}");
                            File.Copy(fullExe, dst, true);
                            Console.WriteLine($"Atalho {prefix}-{version}{Path.GetExtension(fullExe)} criado em {binDir}");
                        }
                    }
                    catch { }
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

            bool renameExe = IsExecutable ? RenameExeAfterInstall : false;
            bool createBinShortcut = IsExecutable ? CreateBinShortcut : false;

            bool isArchive = IsArchive;
            bool runInstaller = RunInstaller;
            string? installerArgs = GetInstallerArgs(version);

            await InstallGenericTool(toolDirArg, version, url, subDir, exeRel, Name, renameExe, isArchive, runInstaller, installerArgs, createBinShortcut);

            string targetDir = System.IO.Path.Combine(toolDirArg, subDir);
            await PostInstall(version, targetDir);

            Console.WriteLine($"{Name} {version} instalado.");
        }

        public static void RenameMainExe(string dir, string exeName, string version, string prefix)
        {
            string exePath = Path.Combine(dir, exeName);
            string exeVersionPath = Path.Combine(dir, $"{prefix}-{version}.exe");
            if (File.Exists(exePath))
            {
                File.Move(exePath, exeVersionPath);
                Console.WriteLine($"Renomeado {exeName} para {prefix}-{version}.exe");
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
    }
}
