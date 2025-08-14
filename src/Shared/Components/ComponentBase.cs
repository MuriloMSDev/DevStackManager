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
        public static readonly HttpClient httpClient = new HttpClient();

        static ComponentBase()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DevStackManager");
            httpClient.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        }

        public abstract string Name { get; }
        public abstract Task Install(string? version = null);

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

        public static async Task<bool> InstallGenericTool(string toolDir, string version, string zipUrl, string subDir, string exeName, string prefix, bool renameExe = true)
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

        public static async Task DownloadAndExtractZip(string url, string zipPath, string extractTo)
        {
            Console.WriteLine($"Baixando {url}...");
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using (var fileStream = File.Create(zipPath))
            {
                await response.Content.CopyToAsync(fileStream);
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
                archive.ExtractToDirectory(extractTo, true);
            }
            await Task.Delay(100);
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
