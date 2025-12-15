using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#region Models

internal class UrlCheckResult
{
    public string Url { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public int StatusCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ContentError { get; set; } = string.Empty;
    public long ContentLength { get; set; }
}

internal class VersionEntry
{
    public string version { get; set; } = string.Empty;
    public string url { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class ComponentVersions
{
    [JsonPropertyName("componentId")]
    public string ComponentId { get; set; } = string.Empty;

    [JsonPropertyName("versions")]
    public Dictionary<string, string> Versions { get; set; } = new();

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class VersionsFile
{
    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("components")]
    public List<ComponentVersions> Components { get; set; } = new();
}

#endregion

internal static class ConsoleEx
{
    public static void Print(string text, ConsoleColor color = ConsoleColor.Gray, bool newline = true)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        if (newline) Console.WriteLine(text); else Console.Write(text);
        Console.ForegroundColor = prev;
    }
}

internal static class Paths
{
    private static string? _customBasePath;
    
    public static string BasePath => _customBasePath ?? ResolveDefaultBasePath();
    public static string VersionsJsonPath => Path.Combine(BasePath, "versions.json");
    public static string BackupPath => Path.Combine(BasePath, "backup");
    public static string CachePath => Path.Combine(BackupPath, "cache");

    /// <summary>
    /// Define um diretório base personalizado para o versions.json e arquivos relacionados.
    /// </summary>
    public static void SetBasePath(string path)
    {
        _customBasePath = path;
    }

    /// <summary>
    /// Reseta o diretório base para o padrão (diretório do executável ou raiz do repositório).
    /// </summary>
    public static void ResetBasePath()
    {
        _customBasePath = null;
    }

    private static string ResolveDefaultBasePath()
    {
        // First try: executable directory
        var exeDir = AppContext.BaseDirectory;
        
        // If running from development environment, walk up to find solution file
        var dir = exeDir;
        for (int i = 0; i < 8; i++)
        {
            var sln = Directory.EnumerateFiles(dir, "DevStackManager.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (sln != null) return dir;
            var parent = Directory.GetParent(dir)?.FullName;
            if (parent == null) break;
            dir = parent;
        }
        
        // fallback: executable directory (for installed version)
        return exeDir;
    }
}

internal static class HttpUtil
{
    private static readonly string[] UserAgents = new[]
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/121.0",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/120.0",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0"
    };

    private static readonly HttpClient Client = CreateClient();

    private static HttpClient CreateClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        var http = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        ApplyDefaultHeaders(http);
        return http;
    }

    private static void ApplyDefaultHeaders(HttpClient http)
    {
        http.DefaultRequestHeaders.Clear();
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", PickUserAgent());
        http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "no-cors");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
    }

    private static string PickUserAgent()
    {
        var rnd = Random.Shared.Next(UserAgents.Length);
        return UserAgents[rnd];
    }

    public static async Task<(byte[]? Content, string? Error, int Status)> GetAsync(string url, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyDomainHeaders(req);
            using var resp = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
            return (bytes, null, (int)resp.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            return (null, ex.Message, 0);
        }
        catch (TaskCanceledException)
        {
            return (null, "Timeout", 408);
        }
        catch (Exception ex)
        {
            return (null, ex.Message, 0);
        }
    }

    public static async Task<UrlCheckResult> HeadOrLightGetAsync(string url, CancellationToken ct)
    {
        var result = new UrlCheckResult { Url = url };
        try
        {
            using (var head = new HttpRequestMessage(HttpMethod.Head, url))
            {
                ApplyDomainHeaders(head);
                using var resp = await Client.SendAsync(head, HttpCompletionOption.ResponseHeadersRead, ct);
                result.StatusCode = (int)resp.StatusCode;
                result.IsValid = result.StatusCode < 400;
                if (resp.Content.Headers.ContentLength.HasValue)
                    result.ContentLength = resp.Content.Headers.ContentLength.Value;
                return result;
            }
        }
        catch
        {
            // fallback to GET headers only
        }

        try
        {
            using var get = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyDomainHeaders(get);
            using var resp = await Client.SendAsync(get, HttpCompletionOption.ResponseHeadersRead, ct);
            result.StatusCode = (int)resp.StatusCode;
            result.IsValid = result.StatusCode < 400;
            if (resp.Content.Headers.ContentLength.HasValue)
                result.ContentLength = resp.Content.Headers.ContentLength.Value;
        }
        catch (HttpRequestException ex)
        {
            result.IsValid = false;
            result.ErrorMessage = ex.Message;
        }
        catch (TaskCanceledException)
        {
            result.IsValid = false;
            result.ErrorMessage = "Timeout";
            result.StatusCode = 408;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ErrorMessage = ex.Message;
        }
        return result;
    }

    private static void ApplyDomainHeaders(HttpRequestMessage req)
    {
        try
        {
            var host = req.RequestUri?.Host?.ToLowerInvariant() ?? string.Empty;
            if (host.Contains("mysql.com") || host.Contains("dev.mysql.com"))
            {
                req.Headers.TryAddWithoutValidation("Referer", "https://dev.mysql.com/downloads/mysql/");
                req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
                req.Headers.TryAddWithoutValidation("Cache-Control", "max-age=0");
            }
            else if (host.Contains("github.com") || host.Contains("api.github.com"))
            {
                req.Headers.TryAddWithoutValidation("Referer", "https://github.com/");
                // Para páginas HTML do GitHub, usar Accept de HTML, não JSON
                if (host == "api.github.com")
                    req.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");
                else
                    req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                if (!string.IsNullOrWhiteSpace(token))
                    req.Headers.TryAddWithoutValidation("Authorization", $"token {token}");
            }
            else if (host.Contains("nodejs.org"))
            {
                req.Headers.TryAddWithoutValidation("Referer", "https://nodejs.org/");
                req.Headers.TryAddWithoutValidation("Accept", "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            }
            else if (host.Contains("windows.php.net"))
            {
                req.Headers.TryAddWithoutValidation("Referer", "https://windows.php.net/download");
                req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
            }
            else if (host.Contains("nginx.org"))
            {
                req.Headers.TryAddWithoutValidation("Referer", "https://nginx.org/");
                req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
            }
            else if (host.Contains("enterprisedb.com"))
            {
                req.Headers.TryAddWithoutValidation("Referer", "https://www.enterprisedb.com/download-postgresql-binaries");
                req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
            }
        }
        catch { }
    }

    /// <summary>
    /// Busca todas as páginas de releases do GitHub API com paginação.
    /// A API do GitHub retorna no máximo 100 itens por página.
    /// </summary>
    public static async Task<List<JsonElement>> GetAllGitHubReleasesAsync(string repoUrl, CancellationToken ct, int maxPages = 10)
    {
        var allReleases = new List<JsonElement>();
        int page = 1;
        
        while (page <= maxPages)
        {
            var url = $"{repoUrl}?per_page=100&page={page}";
            var (content, err, status) = await GetAsync(url, ct);
            
            if (content == null || status != 200)
                break;
            
            try
            {
                var doc = JsonDocument.Parse(content);
                var array = doc.RootElement;
                
                if (array.GetArrayLength() == 0)
                    break;
                
                foreach (var item in array.EnumerateArray())
                {
                    allReleases.Add(item.Clone());
                }
                
                // Se retornou menos que 100, não há mais páginas
                if (array.GetArrayLength() < 100)
                    break;
                    
                page++;
            }
            catch
            {
                break;
            }
        }
        
        return allReleases;
    }
}

internal static class FileUtil
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static VersionsFile LoadVersionsJson()
    {
        if (!File.Exists(Paths.VersionsJsonPath))
            return new VersionsFile();

        try
        {
            var json = File.ReadAllText(Paths.VersionsJsonPath, Encoding.UTF8);
            return JsonSerializer.Deserialize<VersionsFile>(json, JsonOptions) ?? new VersionsFile();
        }
        catch
        {
            return new VersionsFile();
        }
    }

    public static void SaveVersionsJson(VersionsFile versionsFile)
    {
        versionsFile.GeneratedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(versionsFile, JsonOptions);
        File.WriteAllText(Paths.VersionsJsonPath, json, Encoding.UTF8);
    }

    public static List<VersionEntry> GetComponentVersions(VersionsFile versionsFile, string componentId)
    {
        var component = versionsFile.Components.FirstOrDefault(c => c.ComponentId == componentId);
        if (component == null) return new List<VersionEntry>();
        
        return component.Versions.Select(kv => new VersionEntry
        {
            version = kv.Key,
            url = kv.Value
        }).ToList();
    }

    public static void UpdateComponentVersions(VersionsFile versionsFile, string componentId, List<VersionEntry> versions)
    {
        var component = versionsFile.Components.FirstOrDefault(c => c.ComponentId == componentId);
        if (component == null)
        {
            component = new ComponentVersions { ComponentId = componentId };
            versionsFile.Components.Add(component);
        }

        // Use GroupBy to handle duplicate versions, keeping the first URL found
        component.Versions = versions
            .GroupBy(v => v.version)
            .ToDictionary(g => g.Key, g => g.First().url);
        component.LastUpdated = DateTime.Now;
    }

    public static void EnsureDir(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    public static void CreateBackup()
    {
        if (!File.Exists(Paths.VersionsJsonPath)) return;
        
        EnsureDir(Paths.BackupPath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFile = Path.Combine(Paths.BackupPath, $"versions.json_{timestamp}.bak");
        File.Copy(Paths.VersionsJsonPath, backupFile, overwrite: true);
        ConsoleEx.Print($"Backup criado: {backupFile}", ConsoleColor.Green);
        ClearOldBackups(keepCount: 10);
    }

    public static void ClearOldBackups(int keepCount)
    {
        if (!Directory.Exists(Paths.BackupPath)) return;
        var files = Directory.GetFiles(Paths.BackupPath, "versions.json_*.bak")
            .Select(p => new FileInfo(p))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();
        if (files.Count <= keepCount) return;
        foreach (var f in files.Skip(keepCount))
        {
            try
            {
                f.Delete();
                ConsoleEx.Print($"  Backup antigo removido: {f.Name}", ConsoleColor.DarkGray);
            }
            catch { }
        }
        ConsoleEx.Print($"  {files.Count - keepCount} backups antigos removidos (mantidos {keepCount} mais recentes)", ConsoleColor.DarkGray);
    }
}

internal static class CacheUtil
{
    private record FailedEntry(string Version, string Url, string FailedDate, string ErrorMessage);

    public static List<Dictionary<string, object>> GetFailedVersionsCache(string component)
    {
        FileUtil.EnsureDir(Paths.CachePath);
        var file = Path.Combine(Paths.CachePath, component + "-failed.json");
        if (!File.Exists(file)) return new();
        try
        {
            var json = File.ReadAllText(file, Encoding.UTF8);
            var list = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json) ?? new();
            var cutoff = DateTime.Now.AddDays(-7);
            var valid = list.Where(e => DateTime.TryParse(e.GetValueOrDefault("FailedDate")?.ToString(), out var d) && d > cutoff).ToList();
            if (valid.Count != list.Count)
            {
                if (valid.Count == 0) File.Delete(file);
                else File.WriteAllText(file, JsonSerializer.Serialize(valid, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
            }
            return valid;
        }
        catch
        {
            return new();
        }
    }

    public static void SaveFailed(string component, List<VersionEntry> failed)
    {
        if (failed.Count == 0) return;
        FileUtil.EnsureDir(Paths.CachePath);
        var file = Path.Combine(Paths.CachePath, component + "-failed.json");
        var existing = GetFailedVersionsCache(component);
        var now = DateTime.Now.ToString("o");
        foreach (var v in failed)
        {
            var entry = new Dictionary<string, object>
            {
                ["Version"] = v.version,
                ["Url"] = v.url,
                ["FailedDate"] = now,
                ["ErrorMessage"] = v.ErrorMessage ?? string.Empty
            };
            var exists = existing.Any(e => (e.GetValueOrDefault("Version")?.ToString() ?? "") == v.version && (e.GetValueOrDefault("Url")?.ToString() ?? "") == v.url);
            if (!exists) existing.Add(entry);
        }
        File.WriteAllText(file, JsonSerializer.Serialize(existing, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
        ConsoleEx.Print($"  Cache de versões falhadas atualizado: {failed.Count} novas entradas", ConsoleColor.DarkGray);
    }

    public static void ClearAllCache()
    {
        if (!Directory.Exists(Paths.CachePath)) return;
        var files = Directory.GetFiles(Paths.CachePath, "*-failed.json");
        foreach (var f in files) try { File.Delete(f); } catch { }
        if (files.Length > 0)
            ConsoleEx.Print($"Cache removido ({files.Length} arquivos)", ConsoleColor.Green);
    }

    public static void ClearExpiredCache()
    {
        if (!Directory.Exists(Paths.CachePath)) return;
        var files = Directory.GetFiles(Paths.CachePath, "*-failed.json");
        if (files.Length == 0) return;
        var cutoff = DateTime.Now.AddDays(-7);
        int removedEntries = 0, removedFiles = 0;
        foreach (var f in files)
        {
            try
            {
                var json = File.ReadAllText(f, Encoding.UTF8);
                var list = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json) ?? new();
                var original = list.Count;
                list = list.Where(e => DateTime.TryParse(e.GetValueOrDefault("FailedDate")?.ToString(), out var d) && d > cutoff).ToList();
                if (list.Count != original)
                {
                    if (list.Count == 0) { File.Delete(f); removedFiles++; }
                    else File.WriteAllText(f, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
                    removedEntries += (original - list.Count);
                }
            }
            catch { }
        }
        if (removedEntries > 0 || removedFiles > 0)
            ConsoleEx.Print($"Cache limpo: {removedEntries} entradas expiradas, {removedFiles} arquivos removidos", ConsoleColor.Green);
    }
}

internal static class VersionLogic
{
    public static string? Normalize(string versionString, string componentName = "")
    {
        var version = versionString;
        version = Regex.Replace(version, "^[vV]", "");
        version = Regex.Replace(version, "^node-v?", "");
        version = Regex.Replace(version, "^go", "");
        version = Regex.Replace(version, "^mongodb-windows-x86_64-", "");
        version = Regex.Replace(version, "^php-cs-fixer-?v?", "");
        version = Regex.Replace(version, "^phpMyAdmin-", "");
        version = Regex.Replace(version, "^Win64OpenSSL-", "");
        version = Regex.Replace(version, "^elasticsearch-", "");
        version = Regex.Replace(version, "^postgresql-", "");
        version = Regex.Replace(version, "^dbeaver-ce-", "");
        version = Regex.Replace(version, "^wp-cli-", "");
        version = Regex.Replace(version, "^MinGit-", "");
        version = Regex.Replace(version, "^adminer-", "");
        version = Regex.Replace(version, "^composer-", "");
        version = Regex.Replace(version, "^python-", "");
        version = Regex.Replace(version, "^mysql-", "");
        version = Regex.Replace(version, "^nginx-", "");
        version = Regex.Replace(version, "^php-", "");
        version = Regex.Replace(version, "-winx64.*$", "");
        version = Regex.Replace(version, "-win-x64.*$", "");
        version = Regex.Replace(version, "-win32\\.win32\\.x86_64.*$", "");
        version = Regex.Replace(version, "-windows-x86_64.*$", "");
        version = Regex.Replace(version, "-all-languages.*$", "");
        version = Regex.Replace(version, "-amd64.*$", "");
        version = Regex.Replace(version, "-embed-amd64.*$", "");
        version = Regex.Replace(version, "-64-bit.*$", "");
        version = Regex.Replace(version, "\\.zip$", "");
        version = Regex.Replace(version, "\\.exe$", "");
        version = Regex.Replace(version, "\\.phar$", "");
        version = Regex.Replace(version, "\\.php$", "");
        version = Regex.Replace(version, "-Win32-vs1[67]-x64.*$", "");
        version = Regex.Replace(version, "\\.windows\\.(\\d+)", "");
        version = Regex.Replace(version, "_(\\d+)_(\\d+)", @".$1.$2");
        version = Regex.Replace(version, "-?(alpha|beta|rc|dev|snapshot)\\d*.*$", "", RegexOptions.IgnoreCase);
        var m = Regex.Match(version, @"^(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)");
        if (m.Success)
        {
            var extracted = m.Groups[1].Value;
            if (componentName.Equals("phpmyadmin", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(extracted, @"^\d+\.\d+\.\d+$"))
                extracted = extracted + ".0";
            return extracted;
        }
        return null;
    }

    public static List<VersionEntry> Sort(List<VersionEntry> versions)
    {
        int Key(string v)
        {
            var parts = v.Split('.', StringSplitOptions.RemoveEmptyEntries);
            int P(int i) => i < parts.Length && int.TryParse(Regex.Replace(parts[i], "\\D", ""), out var n) ? n : 0;
            return P(0) * 1_000_000 + P(1) * 10_000 + P(2) * 100 + P(3);
        }
        return versions.OrderBy(x => Key(x.version)).ToList();
    }
}

internal static class Discover
{
    public static async Task<List<VersionEntry>> GitAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API do GitHub para Git com paginação
        var releases = await HttpUtil.GetAllGitHubReleasesAsync("https://api.github.com/repos/git-for-windows/git/releases", ct);
        if (releases.Count == 0)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Git", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        try
        {
            foreach (var rel in releases)
            {
                if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;
                if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
                if (!rel.TryGetProperty("tag_name", out var tagProp)) continue;
                var version = VersionLogic.Normalize(tagProp.GetString() ?? string.Empty, "git");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                if (rel.TryGetProperty("assets", out var assets))
                {
                    foreach (var a in assets.EnumerateArray())
                    {
                        var name = a.GetProperty("name").GetString() ?? string.Empty;
                        if (name.Contains("MinGit") && Regex.IsMatch(name, "64-bit\\.zip$"))
                        {
                            var url = a.GetProperty("browser_download_url").GetString() ?? string.Empty;
                            list.Add(new VersionEntry { version = version, url = url });
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do Git: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "git", ct);
    }

    public static async Task<List<VersionEntry>> NodeAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        var (content, err, status) = await HttpUtil.GetAsync("https://nodejs.org/dist/index.json", ct);
        if (content == null)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Node.js: {err}", ConsoleColor.Yellow);
            return new();
        }
        var doc = JsonDocument.Parse(content);
        var list = new List<VersionEntry>();
        foreach (var rel in doc.RootElement.EnumerateArray())
        {
            var tag = rel.TryGetProperty("version", out var ver) ? ver.GetString() ?? string.Empty : string.Empty;
            var version = VersionLogic.Normalize(tag, "node");
            if (version == null) continue;
            if (existing.Any(v => v.version == version)) continue;
            list.Add(new VersionEntry
            {
                version = version,
                url = $"https://nodejs.org/dist/{tag}/node-{tag}-win-x64.zip"
            });
        }
        return await ValidateNewAsync(list, "node", ct);
    }

    public static async Task<List<VersionEntry>> PhpAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        var list = new List<VersionEntry>();
        foreach (var url in new[] { "https://windows.php.net/downloads/releases", "https://windows.php.net/downloads/releases/archives/" })
        {
            var (content, err, status) = await HttpUtil.GetAsync(url, ct);
            if (content == null)
            {
                ConsoleEx.Print($"Erro ao buscar de {url}: {err}", ConsoleColor.Yellow);
                continue;
            }
            var html = Encoding.UTF8.GetString(content);
            var rx = new Regex("href=\"(?<h>[^\"]*php-(?<v>\\d+\\.\\d+\\.\\d+)-Win32[^\"]*x64\\.zip)\"", RegexOptions.IgnoreCase);
            foreach (Match m in rx.Matches(html))
            {
                var vstr = m.Groups["v"].Value;
                var version = VersionLogic.Normalize(vstr, "php");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                var href = m.Groups["h"].Value;
                var dl = href.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? href : $"https://windows.php.net{href}";
                list.Add(new VersionEntry { version = version, url = dl });
            }
        }
        return await ValidateNewAsync(list, "php", ct);
    }

    public static async Task<List<VersionEntry>> PythonAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API oficial do Python que lista todas as versões disponíveis
        var (content, err, status) = await HttpUtil.GetAsync("https://www.python.org/api/v2/downloads/release/?is_published=true", ct);
        if (content == null)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Python: {err}", ConsoleColor.Yellow);
            return new();
        }
        var html = Encoding.UTF8.GetString(content);
        var list = new List<VersionEntry>();
        var foundVersions = new HashSet<string>();
        // Match version patterns like "name": "Python 3.12.0"
        var rx = new Regex(@"""name""\s*:\s*""Python\s+(?<v>\d+\.\d+\.\d+)""", RegexOptions.IgnoreCase);
        foreach (Match m in rx.Matches(html))
        {
            var version = VersionLogic.Normalize(m.Groups["v"].Value, "python");
            if (version == null) continue;
            if (existing.Any(v => v.version == version)) continue;
            if (foundVersions.Contains(version)) continue;
            foundVersions.Add(version);
            // Versões 3.10 e anteriores usam embed-amd64.zip, versões 3.11+ usam -amd64.zip
            var versionParts = version.Split('.');
            var major = int.TryParse(versionParts[0], out var maj) ? maj : 3;
            var minor = versionParts.Length > 1 && int.TryParse(versionParts[1], out var min) ? min : 0;
            var useEmbed = major < 3 || (major == 3 && minor <= 10);
            var suffix = useEmbed ? "-embed-amd64.zip" : "-amd64.zip";
            list.Add(new VersionEntry { version = version, url = $"https://www.python.org/ftp/python/{version}/python-{version}{suffix}" });
        }
        return await ValidateNewAsync(list, "python", ct);
    }

    public static async Task<List<VersionEntry>> MysqlAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Coletar versões diretamente das páginas de Release Notes da Oracle
        // Descobrir dinamicamente as séries a partir do índice e construir URLs no padrão do CDN oficial.
        var seriesIndexUrl = "https://dev.mysql.com/doc/relnotes/mysql/en/";
        var discoveredSeries = new List<string>();
        var (idxContent, idxErr, idxStatus) = await HttpUtil.GetAsync(seriesIndexUrl, ct);
        if (idxContent != null)
        {
            var idxHtml = Encoding.UTF8.GetString(idxContent);
            // Extrai links no formato /doc/relnotes/mysql/<serie>/en/
            foreach (Match m in Regex.Matches(idxHtml, @"/doc/relnotes/mysql/(?<s>\d+\.\d+)/en/", RegexOptions.IgnoreCase))
            {
                var s = m.Groups["s"].Value;
                if (!string.IsNullOrWhiteSpace(s) && !discoveredSeries.Contains(s)) discoveredSeries.Add(s);
            }
        }
        // Se nada encontrado (mudança de site), usar um conjunto mínimo de fallback para não quebrar
        if (discoveredSeries.Count == 0)
            discoveredSeries.AddRange(new[] { "8.4", "9.0" });

        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var candidates = new List<string>();

        foreach (var series in discoveredSeries)
        {
            var url = $"https://dev.mysql.com/doc/relnotes/mysql/{series}/en/";
            var (content, err, status) = await HttpUtil.GetAsync(url, ct);
            if (content == null) continue;
            var html = Encoding.UTF8.GetString(content);
            // Procurar padrões "MySQL X.Y.Z" e também casos com sufixo a/b (5.5.25a)
            foreach (Match m in Regex.Matches(html, @"MySQL\s+(?<v>\d+\.\d+\.\d+[a-z]?)", RegexOptions.IgnoreCase))
            {
                var raw = m.Groups["v"].Value;
                var norm = VersionLogic.Normalize(raw, "mysql");
                if (norm == null) continue;
                if (!found.Contains(raw)) { found.Add(raw); candidates.Add(raw); }
            }
        }

        // Remover versões já existentes e construir URLs para validação
        var list = new List<VersionEntry>();
        foreach (var raw in candidates.Distinct())
        {
            var version = VersionLogic.Normalize(raw, "mysql");
            if (version == null) continue;
            if (existing.Any(v => v.version == version)) continue;
            var mm = Regex.Match(version, @"^(\d+)\.(\d+)");
            if (!mm.Success) continue;
            var majorMinor = mm.Groups[1].Value + "." + mm.Groups[2].Value;
            var url = $"https://dev.mysql.com/get/Downloads/MySQL-{majorMinor}/mysql-{raw}-winx64.zip";
            list.Add(new VersionEntry { version = version, url = url });
        }

        return await ValidateNewAsync(list, "mysql", ct);
    }

    public static async Task<List<VersionEntry>> GoAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API oficial do Go
        var (content, err, status) = await HttpUtil.GetAsync("https://go.dev/dl/?mode=json&include=all", ct);
        if (content == null)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Go: {err}", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        var foundVersions = new HashSet<string>();
        try
        {
            var doc = JsonDocument.Parse(content);
            foreach (var rel in doc.RootElement.EnumerateArray())
            {
                if (!rel.TryGetProperty("version", out var verProp)) continue;
                var tag = verProp.GetString() ?? string.Empty;
                // Extrair versão limpa do tag (ex: go1.22.0 -> 1.22.0)
                var rawVersion = tag.Replace("go", "");
                var version = VersionLogic.Normalize(rawVersion, "go");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                if (foundVersions.Contains(version)) continue;
                foundVersions.Add(version);
                // URL: versões X.Y.0 usam goX.Y, outras usam goX.Y.Z
                var parts = version.Split('.');
                string urlVersion;
                if (parts.Length >= 3 && parts[2] == "0")
                    urlVersion = $"{parts[0]}.{parts[1]}";
                else
                    urlVersion = version;
                list.Add(new VersionEntry { version = version, url = $"https://go.dev/dl/go{urlVersion}.windows-amd64.zip" });
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do Go: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "go", ct);
    }

    public static async Task<List<VersionEntry>> MongoDbAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Extrair versões de forma dinâmica a partir do índice de Release Notes
        var root = "https://www.mongodb.com/docs/manual/release-notes/";
        var pages = new List<string> { root };
        var (rootContent, rootErr, rootStatus) = await HttpUtil.GetAsync(root, ct);
        if (rootContent != null)
        {
            var html = Encoding.UTF8.GetString(rootContent);
            // Capturar links de séries, ex: /docs/manual/release-notes/7.3/
            foreach (Match m in Regex.Matches(html, @"/docs/manual/release-notes/(?<s>\d+\.\d+)/", RegexOptions.IgnoreCase))
            {
                var s = m.Groups["s"].Value;
                var url = $"https://www.mongodb.com/docs/manual/release-notes/{s}/";
                if (!pages.Contains(url)) pages.Add(url);
            }
            // Links antigos como 4.4 sem house-keeping
            foreach (Match m in Regex.Matches(html, @"/docs/manual/release-notes/(?<s>\d+\.\d{1})/", RegexOptions.IgnoreCase))
            {
                var s = m.Groups["s"].Value;
                var url = $"https://www.mongodb.com/docs/manual/release-notes/{s}/";
                if (!pages.Contains(url)) pages.Add(url);
            }
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var versions = new List<string>();

        foreach (var url in pages)
        {
            var (content, err, status) = await HttpUtil.GetAsync(url, ct);
            if (content == null) continue;
            var html = Encoding.UTF8.GetString(content);
            foreach (Match m in Regex.Matches(html, @"\b(?<v>\d+\.\d+\.\d+)\b"))
            {
                var v = m.Groups["v"].Value;
                // Limitar a versões plausíveis (>=4.3)
                if (!Regex.IsMatch(v, @"^(?:4\.[3-9]|5\.|6\.|7\.|8\.)")) continue;
                if (seen.Add(v)) versions.Add(v);
            }
        }

        var list = new List<VersionEntry>();
        foreach (var v in versions.Distinct())
        {
            var norm = VersionLogic.Normalize(v, "mongodb");
            if (norm == null) continue;
            if (existing.Any(e => e.version == norm)) continue;
            var url = $"https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-{v}.zip";
            list.Add(new VersionEntry { version = norm, url = url });
        }

        return await ValidateNewAsync(list, "mongodb", ct);
    }

    public static async Task<List<VersionEntry>> NginxAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        var (content, err, status) = await HttpUtil.GetAsync("https://nginx.org/download/", ct);
        if (content == null)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Nginx: {err}", ConsoleColor.Yellow);
            return new();
        }
        var html = Encoding.UTF8.GetString(content);
        var list = new List<VersionEntry>();
        var rx = new Regex("href=\"(?<h>nginx-(?<v>\\d+\\.\\d+\\.\\d+)\\.zip)\"", RegexOptions.IgnoreCase);
        foreach (Match m in rx.Matches(html))
        {
            var v = VersionLogic.Normalize(m.Groups["v"].Value, "nginx");
            if (v == null) continue;
            if (existing.Any(e => e.version == v)) continue;
            list.Add(new VersionEntry { version = v, url = $"https://nginx.org/download/{m.Groups["h"].Value}" });
        }
        return await ValidateNewAsync(list, "nginx", ct);
    }

    public static async Task<List<VersionEntry>> ElasticAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API do GitHub para Elasticsearch com paginação
        var releases = await HttpUtil.GetAllGitHubReleasesAsync("https://api.github.com/repos/elastic/elasticsearch/releases", ct);
        if (releases.Count == 0)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Elasticsearch", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        try
        {
            foreach (var rel in releases)
            {
                if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;
                if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
                var tag = rel.TryGetProperty("tag_name", out var tn) ? tn.GetString() ?? string.Empty : string.Empty;
                var version = VersionLogic.Normalize(tag, "elasticsearch");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                var url = $"https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-{version}-windows-x86_64.zip";
                list.Add(new VersionEntry { version = version, url = url });
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do Elasticsearch: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "elasticsearch", ct);
    }

    public static async Task<List<VersionEntry>> ComposerAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API do GitHub para Composer com paginação
        var releases = await HttpUtil.GetAllGitHubReleasesAsync("https://api.github.com/repos/composer/composer/releases", ct);
        if (releases.Count == 0)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Composer", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        try
        {
            foreach (var rel in releases)
            {
                if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;
                if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
                var tag = rel.TryGetProperty("tag_name", out var tn) ? tn.GetString() ?? string.Empty : string.Empty;
                var version = VersionLogic.Normalize(tag, "composer");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                list.Add(new VersionEntry { version = version, url = $"https://getcomposer.org/download/{version}/composer.phar" });
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do Composer: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "composer", ct);
    }

    public static async Task<List<VersionEntry>> AdminerAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API do GitHub para Adminer com paginação
        var releases = await HttpUtil.GetAllGitHubReleasesAsync("https://api.github.com/repos/vrana/adminer/releases", ct);
        if (releases.Count == 0)
        {
            ConsoleEx.Print($"Erro ao buscar versões do Adminer", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        try
        {
            foreach (var rel in releases)
            {
                if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;
                if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
                var tag = rel.TryGetProperty("tag_name", out var tn) ? tn.GetString() ?? string.Empty : string.Empty;
                var version = VersionLogic.Normalize(tag, "adminer");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                list.Add(new VersionEntry { version = version, url = $"https://github.com/vrana/adminer/releases/download/v{version}/adminer-{version}.php" });
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do Adminer: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "adminer", ct);
    }

    public static async Task<List<VersionEntry>> DBeaverAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API do GitHub para DBeaver com paginação
        var releases = await HttpUtil.GetAllGitHubReleasesAsync("https://api.github.com/repos/dbeaver/dbeaver/releases", ct);
        if (releases.Count == 0)
        {
            ConsoleEx.Print($"Erro ao buscar versões do DBeaver", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        try
        {
            foreach (var rel in releases)
            {
                if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;
                if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
                var tag = rel.TryGetProperty("tag_name", out var tn) ? tn.GetString() ?? string.Empty : string.Empty;
                var version = VersionLogic.Normalize(tag, "dbeaver");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                list.Add(new VersionEntry { version = version, url = $"https://dbeaver.io/files/{version}/dbeaver-ce-{version}-win32.win32.x86_64.zip" });
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do DBeaver: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "dbeaver", ct);
    }

    public static async Task<List<VersionEntry>> OpenSslAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar JSON oficial do Slproweb com hashes
        var (content, err, status) = await HttpUtil.GetAsync("https://slproweb.com/download/win32_openssl_hashes.json", ct);
        if (content == null)
        {
            ConsoleEx.Print($"Erro ao buscar versões do OpenSSL: {err}", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        var foundVersions = new HashSet<string>();
        try
        {
            var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("files", out var files))
            {
                foreach (var file in files.EnumerateObject())
                {
                    var fileName = file.Name;
                    // Pegar apenas Win64 EXE não-light
                    if (!fileName.StartsWith("Win64OpenSSL-") || !fileName.EndsWith(".exe")) continue;
                    if (fileName.Contains("Light")) continue;
                    
                    var props = file.Value;
                    if (!props.TryGetProperty("basever", out var baseVerProp)) continue;
                    if (!props.TryGetProperty("arch", out var archProp) || archProp.GetString() != "INTEL") continue;
                    if (!props.TryGetProperty("bits", out var bitsProp) || bitsProp.GetInt32() != 64) continue;
                    
                    var baseVer = baseVerProp.GetString() ?? "";
                    
                    // Normalizar versão
                    var version = VersionLogic.Normalize(baseVer, "openssl");
                    if (version == null) continue;
                    if (existing.Any(v => v.version == version)) continue;
                    if (foundVersions.Contains(version)) continue;
                    foundVersions.Add(version);
                    
                    // Construir URL no formato do Provider: Win64OpenSSL-X_Y_Z.exe
                    var urlVersion = version.Replace(".", "_");
                    var url = $"https://slproweb.com/download/Win64OpenSSL-{urlVersion}.exe";
                    
                    list.Add(new VersionEntry { version = version, url = url });
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do OpenSSL: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "openssl", ct);
    }

    public static async Task<List<VersionEntry>> PgSqlAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Estratégia:
        // 1) Raspar a página oficial da EDB para obter pares (versão, fileid)
        // 2) Como fallback, usar versões das notas de lançamento e tentar URL direta do get.enterprisedb.com
        var list = new List<VersionEntry>();

        // 1) Página oficial com links getfile.jsp
        var (content, err, status) = await HttpUtil.GetAsync("https://www.enterprisedb.com/download-postgresql-binaries", ct);
        if (content != null)
        {
            var html = Encoding.UTF8.GetString(content);
            // Captura trechos onde aparece um número de versão próximo a um fileid
            var rx = new Regex(@"PostgreSQL\s+(?<v>\d+\.\d+\.\d+).*?getfile\\.jsp\?fileid=(?<id>\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in rx.Matches(html))
            {
                var v = m.Groups["v"].Value;
                var id = m.Groups["id"].Value;
                var norm = VersionLogic.Normalize(v, "postgresql");
                if (norm == null) continue;
                if (existing.Any(e => e.version == norm)) continue;
                var url = $"https://sbp.enterprisedb.com/getfile.jsp?fileid={id}";
                list.Add(new VersionEntry { version = norm, url = url });
            }
        }

        // 2) Fallback: coletar versões de release notes oficiais e tentar URL direta
        if (list.Count == 0)
        {
            var pgSources = new[]
            {
                "https://www.postgresql.org/docs/release/",
                "https://www.postgresql.org/docs/current/release.html"
            };
            var found = new HashSet<string>();
            foreach (var s in pgSources)
            {
                var (c, e2, st) = await HttpUtil.GetAsync(s, ct);
                if (c == null) continue;
                var html = Encoding.UTF8.GetString(c);
                foreach (Match m in Regex.Matches(html, @"\b(?<v>\d+\.\d+\.\d+)\b"))
                {
                    var v = m.Groups["v"].Value;
                    if (found.Add(v))
                    {
                        var norm = VersionLogic.Normalize(v, "postgresql");
                        if (norm == null) continue;
                        if (existing.Any(e => e.version == norm)) continue;
                        // Tentar com sufixo -1 (padrão comum) e sem sufixo
                        var url1 = $"https://get.enterprisedb.com/postgresql/postgresql-{v}-1-windows-x64-binaries.zip";
                        var url2 = $"https://get.enterprisedb.com/postgresql/postgresql-{v}-windows-x64-binaries.zip";
                        list.Add(new VersionEntry { version = norm, url = url1 });
                        list.Add(new VersionEntry { version = norm, url = url2 });
                    }
                }
            }
        }

        // Validar todos os candidatos e manter somente URLs válidas
        return await ValidateNewAsync(list, "pgsql", ct);
    }

    public static async Task<List<VersionEntry>> PhpCsFixerAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API do GitHub para PHP CS Fixer com paginação
        var releases = await HttpUtil.GetAllGitHubReleasesAsync("https://api.github.com/repos/PHP-CS-Fixer/PHP-CS-Fixer/releases", ct);
        if (releases.Count == 0)
        {
            ConsoleEx.Print($"Erro ao buscar versões do PHP CS Fixer", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        foreach (var rel in releases)
        {
            if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;
            if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
            var tag = rel.TryGetProperty("tag_name", out var tn) ? tn.GetString() ?? string.Empty : string.Empty;
            var version = VersionLogic.Normalize(tag, "phpcsfixer");
            if (version == null) continue;
            if (existing.Any(v => v.version == version)) continue;
            list.Add(new VersionEntry { version = version, url = $"https://github.com/PHP-CS-Fixer/PHP-CS-Fixer/releases/download/v{version}/php-cs-fixer.phar" });
        }
        return await ValidateNewAsync(list, "phpcsfixer", ct);
    }

    public static async Task<List<VersionEntry>> PhpMyAdminAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar página de downloads oficial do phpMyAdmin
        var (content, err, status) = await HttpUtil.GetAsync("https://www.phpmyadmin.net/files/", ct);
        if (content == null)
        {
            ConsoleEx.Print($"Erro ao buscar versões do phpMyAdmin: {err}", ConsoleColor.Yellow);
            return new();
        }
        var html = Encoding.UTF8.GetString(content);
        var list = new List<VersionEntry>();
        var foundVersions = new HashSet<string>();
        // Match versões na página de arquivos
        var rx = new Regex(@"phpMyAdmin-(?<v>\d+\.\d+\.\d+)-all-languages\.zip", RegexOptions.IgnoreCase);
        foreach (Match m in rx.Matches(html))
        {
            var rawVersion = m.Groups["v"].Value; // ex: 5.2.3
            var version = VersionLogic.Normalize(rawVersion, "phpmyadmin"); // ex: 5.2.3.0
            if (version == null) continue;
            if (existing.Any(v => v.version == version)) continue;
            if (foundVersions.Contains(version)) continue;
            foundVersions.Add(version);
            // Usar rawVersion na URL, não a versão normalizada
            list.Add(new VersionEntry { version = version, url = $"https://files.phpmyadmin.net/phpMyAdmin/{rawVersion}/phpMyAdmin-{rawVersion}-all-languages.zip" });
        }
        return await ValidateNewAsync(list, "phpmyadmin", ct);
    }

    public static async Task<List<VersionEntry>> WpCliAsync(List<VersionEntry> existing, CancellationToken ct)
    {
        // Usar API do GitHub para WP-CLI com paginação
        var releases = await HttpUtil.GetAllGitHubReleasesAsync("https://api.github.com/repos/wp-cli/wp-cli/releases", ct);
        if (releases.Count == 0)
        {
            ConsoleEx.Print($"Erro ao buscar versões do WP-CLI", ConsoleColor.Yellow);
            return new();
        }
        var list = new List<VersionEntry>();
        try
        {
            foreach (var rel in releases)
            {
                if (rel.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()) continue;
                if (rel.TryGetProperty("draft", out var draft) && draft.GetBoolean()) continue;
                var tag = rel.TryGetProperty("tag_name", out var tn) ? tn.GetString() ?? string.Empty : string.Empty;
                var version = VersionLogic.Normalize(tag, "wpcli");
                if (version == null) continue;
                if (existing.Any(v => v.version == version)) continue;
                list.Add(new VersionEntry { version = version, url = $"https://github.com/wp-cli/wp-cli/releases/download/v{version}/wp-cli-{version}.phar" });
            }
        }
        catch (Exception ex)
        {
            ConsoleEx.Print($"Erro ao processar JSON do WP-CLI: {ex.Message}", ConsoleColor.Yellow);
        }
        return await ValidateNewAsync(list, "wpcli", ct);
    }

    public static async Task<List<VersionEntry>> ValidateNewAsync(List<VersionEntry> newVersions, string component, CancellationToken ct)
    {
        if (newVersions.Count == 0) return new();
        var failedCache = CacheUtil.GetFailedVersionsCache(component);
        var toCheck = new List<VersionEntry>();
        var skipped = new List<VersionEntry>();
        foreach (var v in newVersions)
        {
            var cached = failedCache.FirstOrDefault(e => (e.GetValueOrDefault("Version")?.ToString() ?? "") == v.version && (e.GetValueOrDefault("Url")?.ToString() ?? "") == v.url);
            if (cached != null)
            {
                ConsoleEx.Print($"  ⚠ Pulando {v.version} (falhou em {cached.GetValueOrDefault("FailedDate")}) : {v.url}", ConsoleColor.Yellow);
                skipped.Add(v);
            }
            else toCheck.Add(v);
        }
        if (skipped.Count > 0) ConsoleEx.Print($"  {skipped.Count} versões puladas (cache de falhas)", ConsoleColor.Yellow);
        if (toCheck.Count == 0)
        {
            ConsoleEx.Print("  Nenhuma nova versão para verificar (todas no cache de falhas)", ConsoleColor.DarkGray);
            return new();
        }
        ConsoleEx.Print($"  Verificando {toCheck.Count} URLs de novas versões...", ConsoleColor.Yellow);
        var sem = new SemaphoreSlim(50);
        var results = new UrlCheckResult[toCheck.Count];
        await Task.WhenAll(toCheck.Select(async (v, i) =>
        {
            await sem.WaitAsync(ct);
            try
            {
                var r = await HttpUtil.HeadOrLightGetAsync(v.url, ct);
                results[i] = r;
                await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1000, 3000)), ct);
            }
            finally { sem.Release(); }
        }));
        var valid = new List<VersionEntry>();
        var failed = new List<VersionEntry>();
        foreach (var v in toCheck)
        {
            var r = results.FirstOrDefault(x => x != null && x.Url == v.url);
            if (r != null && r.IsValid)
            {
                var msg = $"  ✓ {v.version}: {v.url}" + (r.ContentLength > 0 ? $" ({r.ContentLength:N0} bytes)" : "");
                ConsoleEx.Print(msg, ConsoleColor.Green);
                if (!string.IsNullOrWhiteSpace(r.ContentError)) ConsoleEx.Print($"     Aviso: {r.ContentError}", ConsoleColor.Yellow);
                valid.Add(v);
            }
            else
            {
                var errMsg = $"  ✗ {v.version}: {v.url}";
                if (r != null)
                {
                    if (!string.IsNullOrWhiteSpace(r.ErrorMessage)) errMsg += $" - {r.ErrorMessage}";
                    if (!string.IsNullOrWhiteSpace(r.ContentError)) errMsg += $" | {r.ContentError}";
                }
                else errMsg += " - Resultado não encontrado";
                ConsoleEx.Print(errMsg, ConsoleColor.Red);
                v.ErrorMessage = r != null ? string.Join(" | ", new[] { r.ErrorMessage, r.ContentError }.Where(s => !string.IsNullOrWhiteSpace(s))) : "Resultado não encontrado";
                failed.Add(v);
            }
        }
        if (failed.Count > 0) CacheUtil.SaveFailed(component, failed);
        return valid;
    }
}

internal static class UrlTester
{
    public static async Task<List<UrlCheckResult>> TestUrlsAsync(List<string> urls, CancellationToken ct)
    {
        var res = new UrlCheckResult[urls.Count];
        var sem = new SemaphoreSlim(50);
        await Task.WhenAll(urls.Select(async (u, i) =>
        {
            await sem.WaitAsync(ct);
            try { res[i] = await HttpUtil.HeadOrLightGetAsync(u, ct); await Task.Delay(Random.Shared.Next(100, 300), ct); }
            finally { sem.Release(); }
        }));
        return res.ToList();
    }
}

internal static class Backups
{
    public static List<(string FileName, string FullPath, DateTime BackupDate, int DaysOld, double SizeKB)> GetBackupInfo()
    {
        if (!Directory.Exists(Paths.BackupPath)) return new();
        var files = Directory.EnumerateFiles(Paths.BackupPath, "versions.json_*.bak")
            .Where(f => !f.EndsWith("failed.json", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var list = new List<(string FileName, string FullPath, DateTime BackupDate, int DaysOld, double SizeKB)>();
        foreach (var f in files)
        {
            var fi = new FileInfo(f);
            var name = Path.GetFileName(f);
            var m = Regex.Match(name, @"^versions\.json_(?<d>\d{8})_(?<t>\d{6})\.bak$");
            if (m.Success)
            {
                var dt = DateTime.ParseExact(m.Groups["d"].Value + m.Groups["t"].Value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var days = (int)(DateTime.Now - dt).TotalDays;
                list.Add((name, f, dt, days, Math.Round(fi.Length / 1024.0, 2)));
            }
            else
            {
                var dt = fi.LastWriteTime;
                var days = (int)(DateTime.Now - dt).TotalDays;
                list.Add((name, f, dt, days, Math.Round(fi.Length / 1024.0, 2)));
            }
        }
        return list.OrderByDescending(x => x.BackupDate).ToList();
    }

    public static void ShowBackupInfo()
    {
        ConsoleEx.Print("\n=== Backups de versions.json ===", ConsoleColor.Cyan);
        var backups = GetBackupInfo();
        if (backups.Count == 0) { ConsoleEx.Print("Nenhum backup encontrado", ConsoleColor.DarkGray); return; }
        double total = 0;
        foreach (var b in backups)
        {
            var age = b.DaysOld > 30 ? " (ANTIGO)" : string.Empty;
            ConsoleEx.Print($"  • {b.FileName} - {b.BackupDate:dd/MM/yyyy HH:mm:ss} ({b.DaysOld} dias) - {b.SizeKB} KB{age}", ConsoleColor.DarkGray);
            total += b.SizeKB;
        }
        ConsoleEx.Print($"\nTotal: {backups.Count} backups - {Math.Round(total,2)} KB", ConsoleColor.Green);
    }

    public static void ClearOldBackups(int daysOld = 30)
    {
        var backups = GetBackupInfo();
        var old = backups.Where(b => b.DaysOld > daysOld).ToList();
        if (old.Count == 0)
        {
            ConsoleEx.Print($"Nenhum backup encontrado com mais de {daysOld} dias", ConsoleColor.DarkGray);
            return;
        }
        ConsoleEx.Print("\nBackups removidos:", ConsoleColor.Yellow);
        foreach (var b in old)
        {
            try 
            { 
                File.Delete(b.FullPath);
                ConsoleEx.Print($"  • {b.FileName} ({b.DaysOld} dias)", ConsoleColor.DarkGray);
            } 
            catch { }
        }
        ConsoleEx.Print($"{old.Count} backups removidos com sucesso", ConsoleColor.Green);
    }

    public static void ClearAllBackups()
    {
        var backups = GetBackupInfo();
        if (backups.Count == 0)
        {
            ConsoleEx.Print("Nenhum backup encontrado", ConsoleColor.DarkGray);
            return;
        }
        ConsoleEx.Print("\nBackups removidos:", ConsoleColor.Yellow);
        foreach (var b in backups)
        {
            try 
            { 
                File.Delete(b.FullPath);
                ConsoleEx.Print($"  • {b.FileName} ({b.DaysOld} dias)", ConsoleColor.DarkGray);
            } 
            catch { }
        }
        ConsoleEx.Print($"{backups.Count} backups removidos com sucesso", ConsoleColor.Green);
    }
}

internal static class ComponentProcessor
{
    private static readonly string[] SupportedComponents = new[]
    {
        "adminer", "composer", "dbeaver", "elasticsearch", "git", "go",
        "mongodb", "mysql", "nginx", "node", "openssl", "pgsql",
        "php", "phpcsfixer", "phpmyadmin", "python", "wpcli"
    };

    public static string[] GetSupportedComponents() => SupportedComponents;

    public static async Task ProcessAllAsync(VersionsFile versionsFile, CancellationToken ct = default)
    {
        foreach (var componentId in SupportedComponents)
        {
            await ProcessComponentAsync(versionsFile, componentId, ct);
        }
    }

    public static async Task ProcessComponentAsync(VersionsFile versionsFile, string componentId, CancellationToken ct = default)
    {
        ConsoleEx.Print($"\n=== Processando {componentId} ===", ConsoleColor.Cyan);
        
        var existing = FileUtil.GetComponentVersions(versionsFile, componentId);
        ConsoleEx.Print($"Carregadas {existing.Count} versões existentes", ConsoleColor.Green);
        
        // Validate existing URLs
        var urls = existing.Select(v => v.url).ToList();
        var results = await UrlTester.TestUrlsAsync(urls, ct);
        var valid = results.Count(r => r.IsValid);
        var invalid = results.Count(r => !r.IsValid);
        ConsoleEx.Print($"URLs válidas: {valid}", ConsoleColor.Green);
        if (invalid > 0)
        {
            ConsoleEx.Print($"URLs inválidas: {invalid}", ConsoleColor.Red);
            foreach (var r in results.Where(r => !r.IsValid))
            {
                var msg = $"  - {r.Url} (Status: {r.StatusCode})" + 
                    (!string.IsNullOrWhiteSpace(r.ErrorMessage) ? $" - Erro: {r.ErrorMessage}" : "") + 
                    (!string.IsNullOrWhiteSpace(r.ContentError) ? $" - Conteúdo: {r.ContentError}" : "");
                ConsoleEx.Print(msg, ConsoleColor.Red);
            }
        }
        
        // Discover new versions
        ConsoleEx.Print("\nBuscando novas versões...", ConsoleColor.Yellow);
        var newVersions = await DiscoverForAsync(componentId, existing, ct);
        if (newVersions.Count > 0)
        {
            ConsoleEx.Print($"Encontradas {newVersions.Count} novas versões:", ConsoleColor.Green);
            foreach (var nv in newVersions) ConsoleEx.Print($"  + {nv.version}: {nv.url}", ConsoleColor.Cyan);
        }
        else ConsoleEx.Print("Nenhuma nova versão encontrada", ConsoleColor.Yellow);
        
        // Remove invalid URLs
        if (invalid > 0)
        {
            var invalidUrls = results.Where(r => !r.IsValid).Select(r => r.Url).ToHashSet();
            existing = existing.Where(e => !invalidUrls.Contains(e.url)).ToList();
            ConsoleEx.Print($"Removendo {invalid} entradas com URLs inválidas...", ConsoleColor.Yellow);
        }
        
        // Update if there are changes
        if (newVersions.Count > 0 || invalid > 0)
        {
            var all = existing.Concat(newVersions).ToList();
            var sorted = VersionLogic.Sort(all);
            FileUtil.UpdateComponentVersions(versionsFile, componentId, sorted);
            ConsoleEx.Print($"Componente atualizado com {sorted.Count} versões (ordem crescente)", ConsoleColor.Green);
        }
    }

    private static async Task<List<VersionEntry>> DiscoverForAsync(string component, List<VersionEntry> existing, CancellationToken ct)
    {
        component = component.ToLowerInvariant();
        return component switch
        {
            "git" => await Discover.GitAsync(existing, ct),
            "node" => await Discover.NodeAsync(existing, ct),
            "php" => await Discover.PhpAsync(existing, ct),
            "python" => await Discover.PythonAsync(existing, ct),
            "mysql" => await Discover.MysqlAsync(existing, ct),
            "go" => await Discover.GoAsync(existing, ct),
            "mongodb" => await Discover.MongoDbAsync(existing, ct),
            "nginx" => await Discover.NginxAsync(existing, ct),
            "elasticsearch" => await Discover.ElasticAsync(existing, ct),
            "composer" => await Discover.ComposerAsync(existing, ct),
            "adminer" => await Discover.AdminerAsync(existing, ct),
            "dbeaver" => await Discover.DBeaverAsync(existing, ct),
            "openssl" => await Discover.OpenSslAsync(existing, ct),
            "pgsql" => await Discover.PgSqlAsync(existing, ct),
            "phpcsfixer" => await Discover.PhpCsFixerAsync(existing, ct),
            "phpmyadmin" => await Discover.PhpMyAdminAsync(existing, ct),
            "wpcli" => await Discover.WpCliAsync(existing, ct),
            _ => new List<VersionEntry>()
        };
    }
}

/// <summary>
/// Gerenciador de versões de componentes do DevStack.
/// Responsável por descobrir, validar e manter o arquivo versions.json atualizado.
/// </summary>
public static class VersionManager
{
    /// <summary>
    /// Define o diretório base onde o versions.json será salvo.
    /// </summary>
    /// <param name="path">Caminho do diretório base</param>
    public static void SetOutputPath(string path)
    {
        Paths.SetBasePath(path);
    }

    /// <summary>
    /// Reseta o diretório de saída para o padrão.
    /// </summary>
    public static void ResetOutputPath()
    {
        Paths.ResetBasePath();
    }

    /// <summary>
    /// Atualiza todos os componentes suportados e salva o arquivo versions.json.
    /// </summary>
    /// <param name="outputPath">Diretório onde o versions.json será salvo. Se null, usa o diretório padrão.</param>
    /// <param name="ct">Token de cancelamento</param>
    public static async Task UpdateAllAsync(string? outputPath = null, CancellationToken ct = default)
    {
        if (outputPath != null)
            Paths.SetBasePath(outputPath);

        try
        {
            ConsoleEx.Print("=== DevStack Version Manager ===", ConsoleColor.Cyan);
            ConsoleEx.Print($"Data: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", ConsoleColor.DarkGray);
            ConsoleEx.Print($"Arquivo de versões: {Paths.VersionsJsonPath}", ConsoleColor.DarkGray);

            var versionsFile = FileUtil.LoadVersionsJson();

            await ComponentProcessor.ProcessAllAsync(versionsFile, ct);

            // Create backup before saving
            FileUtil.CreateBackup();
            
            // Clear expired cache
            CacheUtil.ClearExpiredCache();
            
            // Save the updated versions file
            FileUtil.SaveVersionsJson(versionsFile);
            ConsoleEx.Print($"\nArquivo {Paths.VersionsJsonPath} atualizado com sucesso", ConsoleColor.Green);
            ConsoleEx.Print($"Total de componentes: {versionsFile.Components.Count}", ConsoleColor.Green);
            ConsoleEx.Print("\n=== Processamento concluído ===", ConsoleColor.Green);
        }
        finally
        {
            if (outputPath != null)
                Paths.ResetBasePath();
        }
    }

    /// <summary>
    /// Atualiza um componente específico e salva o arquivo versions.json.
    /// </summary>
    /// <param name="componentId">ID do componente (ex: "php", "node", "mysql")</param>
    public static async Task UpdateComponentAsync(string componentId, CancellationToken ct = default)
    {
        ConsoleEx.Print("=== DevStack Version Manager ===", ConsoleColor.Cyan);
        ConsoleEx.Print($"Data: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", ConsoleColor.DarkGray);

        var versionsFile = FileUtil.LoadVersionsJson();

        await ComponentProcessor.ProcessComponentAsync(versionsFile, componentId, ct);

        FileUtil.CreateBackup();
        CacheUtil.ClearExpiredCache();
        FileUtil.SaveVersionsJson(versionsFile);
        
        ConsoleEx.Print($"\nArquivo {Paths.VersionsJsonPath} atualizado com sucesso", ConsoleColor.Green);
    }

    /// <summary>
    /// Carrega o arquivo versions.json atual.
    /// </summary>
    public static VersionsFile LoadVersions() => FileUtil.LoadVersionsJson();

    /// <summary>
    /// Obtém as versões de um componente específico.
    /// </summary>
    public static Dictionary<string, string> GetComponentVersions(string componentId)
    {
        var versionsFile = FileUtil.LoadVersionsJson();
        var component = versionsFile.Components.FirstOrDefault(c => c.ComponentId == componentId);
        return component?.Versions ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Obtém a lista de componentes suportados.
    /// </summary>
    public static string[] GetSupportedComponents() => ComponentProcessor.GetSupportedComponents();

    /// <summary>
    /// Limpa o cache de versões falhadas.
    /// </summary>
    public static void ClearCache() => CacheUtil.ClearAllCache();

    /// <summary>
    /// Limpa o cache de um componente específico.
    /// </summary>
    public static void ClearComponentCache(string componentId)
    {
        var cacheFile = Path.Combine(Paths.CachePath, componentId + "-failed.json");
        if (File.Exists(cacheFile))
        {
            File.Delete(cacheFile);
            ConsoleEx.Print($"Cache de '{componentId}' removido com sucesso", ConsoleColor.Green);
        }
    }

    /// <summary>
    /// Limpa backups antigos (mais de 30 dias).
    /// </summary>
    public static void ClearOldBackups(int daysOld = 30) => Backups.ClearOldBackups(daysOld);

    /// <summary>
    /// Obtém o caminho do arquivo versions.json.
    /// </summary>
    public static string GetVersionsJsonPath() => Paths.VersionsJsonPath;
}
