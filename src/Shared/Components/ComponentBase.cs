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
    /// <summary>
    /// Abstract base class for all DevStack components providing common installation, uninstallation,
    /// and version management functionality. Implements the core component lifecycle operations including
    /// download, extraction, installation, and cleanup with support for both archive-based and installer-based deployments.
    /// </summary>
    public abstract class ComponentBase : ComponentInterface
    {
        /// <summary>
        /// Shared HTTP client instance for component downloads with browser-like headers and compression support.
        /// </summary>
        private static readonly HttpClient _httpClient;
        
        /// <summary>
        /// Number of retry attempts for file deletion operations.
        /// </summary>
        private const int FILE_DELETE_RETRY_COUNT = 5;
        
        /// <summary>
        /// Delay in milliseconds between file deletion retry attempts.
        /// </summary>
        private const int FILE_DELETE_RETRY_DELAY_MS = 200;
        
        /// <summary>
        /// Delay in milliseconds after installation before proceeding to next operation.
        /// </summary>
        private const int POST_INSTALL_DELAY_MS = 100;

        /// <summary>
        /// Static constructor initializing the HTTP client with browser-like headers and decompression support.
        /// Configures the client for reliable component downloads from various sources.
        /// </summary>
        static ComponentBase()
        {
            _httpClient = CreateConfiguredHttpClient();
        }

        /// <summary>
        /// Creates and configures an HTTP client with cookie support, automatic redirects,
        /// and compression handling for reliable component downloads.
        /// </summary>
        /// <returns>A fully configured HttpClient instance.</returns>
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

        /// <summary>
        /// Creates an HTTP GET request with browser-like headers to improve compatibility with download sources.
        /// </summary>
        /// <param name="url">The URL to request.</param>
        /// <param name="referer">Optional referer URL to include in headers.</param>
        /// <returns>A configured HttpRequestMessage for browser-like requests.</returns>
        private static HttpRequestMessage CreateBrowserGetRequest(string url, string? referer = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            ConfigureBrowserHeaders(request, referer);
            return request;
        }

        /// <summary>
        /// Configures HTTP request headers to mimic a browser request, improving download reliability.
        /// </summary>
        /// <param name="request">The HTTP request message to configure.</param>
        /// <param name="referer">Optional referer URL.</param>
        private static void ConfigureBrowserHeaders(HttpRequestMessage request, string? referer)
        {
            request.Headers.Accept.ParseAdd("*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            
            if (!string.IsNullOrEmpty(referer))
            {
                try { request.Headers.Referrer = new Uri(referer); } 
                catch { }
            }
            
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "cross-site");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "no-cors");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
            request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            request.Headers.Connection.ParseAdd("keep-alive");
        }

        /// <summary>
        /// Sends an HTTP request with browser-like headers and optional preflight request.
        /// </summary>
        /// <param name="url">The URL to download from.</param>
        /// <param name="doPreflight">Whether to perform a preflight request to the base URL first.</param>
        /// <returns>The HTTP response message.</returns>
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

        /// <summary>
        /// Attempts a preflight request to the base URL to establish a session before the main download.
        /// Failures are silently ignored as they are not critical.
        /// </summary>
        /// <param name="url">The full URL from which to extract the base URL.</param>
        private static async Task TryPreflightRequest(string url)
        {
            try
            {
                var baseUri = new Uri(url).GetLeftPart(UriPartial.Authority);
                using var preRequest = CreateBrowserGetRequest(baseUri);
                await _httpClient.SendAsync(preRequest, HttpCompletionOption.ResponseHeadersRead);
            }
            catch { }
        }

        /// <summary>
        /// Gets the unique name identifier for this component (e.g., "php", "nginx").
        /// Must be implemented by derived classes.
        /// </summary>
        public abstract string Name { get; }
        
        /// <summary>
        /// Gets the display label for this component.
        /// Must be implemented by derived classes.
        /// </summary>
        public abstract string Label { get; }
        
        /// <summary>
        /// Gets the base directory where this component is installed.
        /// Must be implemented by derived classes.
        /// </summary>
        public abstract string ToolDir { get; }

        /// <summary>
        /// Indicates whether this component runs as a background service.
        /// Default is false. Override to return true for service components.
        /// </summary>
        public virtual bool IsService => false;
        
        /// <summary>
        /// Indicates whether this component is an executable program.
        /// Default is false. Override to return true for executable components.
        /// </summary>
        public virtual bool IsExecutable => false;
        
        /// <summary>
        /// Indicates whether this component is a command-line tool.
        /// Default is false. Override to return true for CLI tools.
        /// </summary>
        public virtual bool IsCommandLine => false;
        
        /// <summary>
        /// Gets the subfolder containing the executable within the component installation directory.
        /// Default is empty string. Override to specify a subfolder like "bin" or "cmd".
        /// </summary>
        public virtual string? ExecutableFolder => string.Empty;
        
        /// <summary>
        /// Gets the filename pattern for the component's main executable.
        /// Default is empty string. Override to specify patterns like "php-{version}.exe".
        /// </summary>
        public virtual string? ExecutablePattern => string.Empty;
        
        /// <summary>
        /// Gets the service name pattern for monitoring running services.
        /// Default is empty string. Override for service components.
        /// </summary>
        public virtual string? ServicePattern => string.Empty;
        
        /// <summary>
        /// Gets the subdirectory within the extracted archive where the component files are located.
        /// Default is null (files are at the root of the archive).
        /// </summary>
        public virtual string? SubDirectory => null;
        
        /// <summary>
        /// Indicates whether the component is distributed as an archive (zip, tar.gz) or an installer.
        /// Default is true. Override to return false for components that use installers.
        /// </summary>
        public virtual bool IsArchive => true;
        
        /// <summary>
        /// Indicates whether to run an installer executable after downloading.
        /// Default is false. Override to return true for components that require installer execution.
        /// </summary>
        public virtual bool RunInstaller => false;
        
        /// <summary>
        /// Gets the command-line arguments to pass to the installer executable.
        /// Default is null. Override to provide installer-specific arguments.
        /// </summary>
        /// <param name="version">The version being installed.</param>
        /// <returns>The installer arguments string, or null if not applicable.</returns>
        public virtual string? GetInstallerArgs(string version) => null;
        
        /// <summary>
        /// Gets the relative path to the binary for which to create a shortcut in the bin folder.
        /// Default is null. Override to enable bin shortcut creation.
        /// </summary>
        public virtual string? CreateBinShortcut => null;
        
        /// <summary>
        /// Gets the maximum number of worker processes for this component.
        /// Default is null. Override for components that support worker configuration (e.g., Nginx, PHP-FPM).
        /// </summary>
        /// <summary>
        /// Gets the maximum number of worker processes for this component.
        /// Default is null. Override for components that support worker configuration (e.g., Nginx, PHP-FPM).
        /// </summary>
        public virtual int? MaxWorkers => null;

        /// <summary>
        /// Gets the localized service type string for this component (e.g., "Web Server", "Database").
        /// </summary>
        /// <param name="localizationManager">The localization manager for string translation.</param>
        /// <returns>The localized service type string.</returns>
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

        /// <summary>
        /// Gets the service description string with version information.
        /// </summary>
        /// <param name="version">The installed version of the component.</param>
        /// <param name="localizationManager">The localization manager for string translation.</param>
        /// <returns>The formatted service description string.</returns>
        public virtual string GetServiceDescription(string version, DevStackShared.LocalizationManager localizationManager)
        {
            var componentName = Name.ToLowerInvariant();
            
            return componentName switch
            {
                "php" => $"PHP {version} {localizationManager.GetString("gui.services_tab.types.fastcgi")}",
                _ => $"{Name} {version}"
            };
        }

        /// <summary>
        /// Performs post-installation tasks for the component (e.g., configuration file generation, permission setup).
        /// Default implementation does nothing. Override to add custom post-install logic.
        /// </summary>
        /// <param name="version">The installed version of the component.</param>
        /// <param name="targetDir">The directory where the component was installed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task PostInstall(string version, string targetDir) => Task.CompletedTask;

        /// <summary>
        /// Gets the latest available version of the component from the version provider.
        /// </summary>
        /// <returns>The latest version string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no versions are available.</exception>
        public string GetLatestVersion()
        {
            var available = ListAvailable();
            if (!available.Any())
            {
                throw new InvalidOperationException($"Unable to retrieve the latest version of {Name}.");
            }
            
            return available.Last();
        }

        /// <summary>
        /// Lists all available versions of the component from the version provider.
        /// </summary>
        /// <returns>A list of available version strings, or an empty list if none are found.</returns>
        public List<string> ListAvailable()
        {
            var versionInfos = GetVersions();
            return versionInfos?.Select(v => v.Version).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Lists all installed versions of the component by scanning the tool directory.
        /// </summary>
        /// <returns>A sorted list of installed version strings.</returns>
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

        /// <summary>
        /// Gets the download URL for a specific version of the component.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <returns>The download URL.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the URL for the specified version is not found.</exception>
        public string GetUrlForVersion(string version)
        {
            var versionInfo = DevStackShared.AvailableVersions.Providers.VersionRegistry.GetVersion(Name, version);
            if (versionInfo == null)
            {
                throw new InvalidOperationException($"URL for version {version} of {Name} not found.");
            }
            
            return versionInfo.Url;
        }

        /// <summary>
        /// Retrieves available version information from the version provider registry.
        /// </summary>
        /// <returns>A list of version information objects, or null if unavailable.</returns>
        private List<DevStackShared.AvailableVersions.Models.VersionInfo>? GetVersions()
        {
            return DevStackShared.AvailableVersions.Providers.VersionRegistry.GetAvailableVersions(Name);
        }

        /// <summary>
        /// Uninstalls a specific version of the component.
        /// </summary>
        /// <param name="version">The version to uninstall. If null or empty, displays an error message.</param>
        public void Uninstall(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine($"Error: {Name} version must be specified for uninstallation.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine($"Example: uninstall {Name} <version>", ConsoleColor.Yellow);
                return;
            }
            
            var subDir = $"{Name}-{version}";
            UninstallGenericTool(Path.Combine(DevStackConfig.baseDir, Name), subDir);
        }

        /// <summary>
        /// Generic installation method for downloading, extracting, and installing a component version.
        /// Handles both archive extraction and installer execution based on component configuration.
        /// </summary>
        /// <param name="toolDir">The base directory where the component will be installed.</param>
        /// <param name="version">The version to install.</param>
        /// <param name="url">The download URL for the component archive or installer.</param>
        /// <param name="subDir">Optional subdirectory name within toolDir. Defaults to "{prefix}-{version}".</param>
        /// <param name="exeRelativePath">Optional relative path to the main executable within the archive.</param>
        /// <param name="prefix">Optional prefix for the installation directory. Defaults to the toolDir name.</param>
        /// <param name="isArchive">True if the download is an archive to extract; false if it's a standalone executable.</param>
        /// <param name="runInstaller">True if the downloaded file should be executed as an installer.</param>
        /// <param name="installerArgs">Optional command-line arguments to pass to the installer.</param>
        /// <returns>True if installation succeeded; false otherwise.</returns>
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

        /// <summary>
        /// Checks if the component version is already installed at the target directory.
        /// </summary>
        /// <param name="targetDir">The target installation directory.</param>
        /// <param name="prefix">The component prefix.</param>
        /// <param name="version">The version being installed.</param>
        /// <returns>True if already installed; false otherwise.</returns>
        private static Task<bool> CheckAlreadyInstalled(string targetDir, string prefix, string version)
        {
            if (Directory.Exists(targetDir))
            {
                Console.WriteLine(GetLocalizedString("shared.install.already_installed", 
                    $"{prefix} {version} is already installed.", prefix, version));
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// Ensures the specified directory exists, creating it if necessary.
        /// </summary>
        /// <param name="directory">The directory path to ensure exists.</param>
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Logs a message indicating the download has started.
        /// </summary>
        /// <param name="prefix">The component prefix.</param>
        /// <param name="version">The version being downloaded.</param>
        private static void LogDownloadStart(string prefix, string version)
        {
            Console.WriteLine(GetLocalizedString("shared.install.downloading", 
                $"Downloading {prefix} {version}...", prefix, version));
        }

        /// <summary>
        /// Validates that the download URL is not empty.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the URL is null or empty.</exception>
        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Invalid URL for download.", nameof(url));
            }
        }

        /// <summary>
        /// Extracts the filename from the download URL, or generates a default name.
        /// </summary>
        /// <param name="url">The download URL.</param>
        /// <param name="prefix">The component prefix.</param>
        /// <param name="version">The version being installed.</param>
        /// <returns>The filename for the downloaded file.</returns>
        private static string GetDownloadFileName(string url, string prefix, string version)
        {
            var fileName = Path.GetFileName(new Uri(url).LocalPath);
            return string.IsNullOrEmpty(fileName) ? $"{prefix}-{version}" : fileName;
        }

        /// <summary>
        /// Determines if the installer should be run based on configuration and URL extension.
        /// </summary>
        /// <param name="runInstaller">Whether the component is configured to run an installer.</param>
        /// <param name="url">The download URL.</param>
        /// <param name="installerArgs">The installer arguments, if any.</param>
        /// <returns>True if the installer should be executed.</returns>
        private static bool ShouldRunInstaller(bool runInstaller, string url, string? installerArgs)
        {
            return runInstaller || (url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && installerArgs != null);
        }

        /// <summary>
        /// Determines if the downloaded file is an archive that needs extraction.
        /// </summary>
        /// <param name="isArchive">Whether the component is configured as an archive.</param>
        /// <param name="url">The download URL.</param>
        /// <returns>True if the file should be extracted as an archive.</returns>
        private static bool ShouldExtractArchive(bool isArchive, string url)
        {
            return isArchive || url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Installs a component by downloading and running an installer executable.
        /// </summary>
        /// <param name="url">The download URL.</param>
        /// <param name="downloadPath">The path where the installer will be downloaded.</param>
        /// <param name="targetDir">The target installation directory.</param>
        /// <param name="toolDir">The base tool directory.</param>
        /// <param name="installerArgs">Command-line arguments for the installer.</param>
        /// <param name="prefix">The component prefix.</param>
        /// <param name="version">The version being installed.</param>
        /// <returns>True indicating rollback is enabled.</returns>
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
                $"Running installer {Path.GetFileName(downloadPath)} {args}...", 
                Path.GetFileName(downloadPath), args));
            
            await ProcessManager.ExecuteProcessAsync(downloadPath, args, toolDir);

            TryDeleteFile(downloadPath);

            Console.WriteLine(GetLocalizedString("shared.install.installed_via_installer", 
                $"{prefix} {version} installed via installer at {targetDir}", 
                prefix, version, targetDir));
            
            return true;
        }

        /// <summary>
        /// Installs a component by downloading and extracting an archive.
        /// </summary>
        /// <param name="url">The download URL.</param>
        /// <param name="toolDir">The base tool directory.</param>
        /// <param name="targetDir">The target installation directory.</param>
        /// <param name="prefix">The component prefix.</param>
        /// <param name="version">The version being installed.</param>
        /// <returns>True indicating rollback is enabled.</returns>
        private static async Task<bool> InstallViaArchive(
            string url, 
            string toolDir, 
            string targetDir, 
            string prefix, 
            string version)
        {
            var zipPath = Path.Combine(toolDir, $"{prefix}-{version}.zip");

            await DownloadFile(url, zipPath);

            Console.WriteLine(GetLocalizedString("shared.install.extracting", "Extracting..."));
            
            await ExtractAndFlattenArchive(zipPath, targetDir);
            await DeleteFileWithRetry(zipPath);

            Console.WriteLine(GetLocalizedString("shared.install.installed", 
                $"{prefix} {version} installed.", prefix, version));
            
            return true;
        }

        /// <summary>
        /// Installs a component by directly downloading a file to the target directory.
        /// </summary>
        /// <param name="url">The download URL.</param>
        /// <param name="targetDir">The target installation directory.</param>
        /// <param name="downloadFileName">The name for the downloaded file.</param>
        /// <param name="prefix">The component prefix.</param>
        /// <param name="version">The version being installed.</param>
        /// <returns>True indicating rollback is enabled.</returns>
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
                $"{prefix} {version} installed at {targetDir}.", prefix, version, targetDir));
            
            return true;
        }

        /// <summary>
        /// Downloads a file from a URL to a local destination path.
        /// </summary>
        /// <param name="url">The download URL.</param>
        /// <param name="destinationPath">The local file path where the download will be saved.</param>
        private static async Task DownloadFile(string url, string destinationPath)
        {
            using var response = await SendBrowserRequestAsync(url);
            await using var fs = File.Create(destinationPath);
            await response.Content.CopyToAsync(fs);
        }

        /// <summary>
        /// Extracts a zip archive and flattens the directory structure if there's a single root folder.
        /// </summary>
        /// <param name="zipPath">The path to the zip archive.</param>
        /// <param name="targetDir">The target directory for extraction.</param>
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

        /// <summary>
        /// Determines the top-level folder name in a zip archive if all entries share the same root.
        /// </summary>
        /// <param name="archive">The zip archive to analyze.</param>
        /// <returns>The top-level folder name if unique; otherwise null.</returns>
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

        /// <summary>
        /// Moves the contents of the extracted root folder up one level to flatten the directory structure.
        /// Deletes the now-empty root folder after moving its contents.
        /// </summary>
        /// <param name="targetDir">The target directory containing the extracted folder.</param>
        /// <param name="topFolder">The name of the root folder to flatten.</param>
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

        /// <summary>
        /// Attempts to delete a file with multiple retries, waiting between attempts.
        /// </summary>
        /// <param name="filePath">The file to delete.</param>
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

        /// <summary>
        /// Attempts to delete a file, silently ignoring any errors.
        /// </summary>
        /// <param name="filePath">The file to delete.</param>
        private static void TryDeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Attempts to delete a directory and all its contents, silently ignoring any errors.
        /// </summary>
        /// <param name="directory">The directory to delete.</param>
        private static void TryDeleteDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs an installation error message to the console.
        /// </summary>
        /// <param name="prefix">The component prefix.</param>
        /// <param name="version">The version being installed.</param>
        /// <param name="ex">The exception that occurred.</param>
        private static void LogInstallError(string prefix, string version, Exception ex)
        {
            Console.WriteLine(GetLocalizedString("shared.install.error_installing", 
                $"Error installing {prefix} {version}: {ex.Message}", prefix, version, ex.Message));
        }

        /// <summary>
        /// Retrieves a localized string from the localization manager, falling back to a default message.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <param name="fallback">The fallback message if localization is unavailable.</param>
        /// <param name="args">Optional arguments for string formatting.</param>
        /// <returns>The localized or fallback string.</returns>
        private static string GetLocalizedString(string key, string fallback, params object[] args)
        {
            return DevStackShared.LocalizationManager.Instance?.GetString(key, args) ?? fallback;
        }

        /// <summary>
        /// Installs a specific version of the component. If no version is specified, installs the latest version.
        /// </summary>
        /// <param name="version">The version to install, or null for the latest version.</param>
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
                $"{Name} {version} installed.", Name, version));
        }

        /// <summary>
        /// Gets the base directory where the component will be installed.
        /// Uses ToolDir if specified, otherwise defaults to baseDir/Name.
        /// </summary>
        /// <returns>The base tool directory path.</returns>
        private string GetBaseToolDirectory()
        {
            return !string.IsNullOrEmpty(ToolDir) 
                ? ToolDir 
                : Path.Combine(DevStackConfig.baseDir, Name);
        }

        /// <summary>
        /// Resolves the installation directory and executable path based on component configuration.
        /// Handles both absolute and relative ExecutableFolder paths.
        /// </summary>
        /// <param name="baseToolDir">The base tool directory.</param>
        /// <param name="version">The version being installed.</param>
        /// <returns>A tuple containing the tool directory and the relative path to the executable.</returns>
        private (string toolDir, string? exeRelativePath) ResolveInstallationPaths(string baseToolDir, string version)
        {
            if (string.IsNullOrEmpty(ExecutableFolder))
            {
                return (baseToolDir, GetExecutablePattern(version));
            }

            if (Path.IsPathRooted(ExecutableFolder))
            {
                return (ExecutableFolder, GetExecutablePattern(version));
            }

            var exePath = !string.IsNullOrEmpty(ExecutablePattern)
                ? Path.Combine(ExecutableFolder, ExecutablePattern.Replace("{version}", version))
                : null;

            return (baseToolDir, exePath);
        }

        /// <summary>
        /// Gets the executable filename pattern with version placeholder replaced.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <returns>The executable pattern with version substituted, or null if no pattern is defined.</returns>
        private string? GetExecutablePattern(string version)
        {
            return !string.IsNullOrEmpty(ExecutablePattern) 
                ? ExecutablePattern.Replace("{version}", version) 
                : null;
        }

        /// <summary>
        /// Creates a bin directory shortcut to the component executable if configured.
        /// </summary>
        /// <param name="version">The installed version.</param>
        /// <param name="targetDir">The component installation directory.</param>
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
                    $"Warning: failed to create shortcut: {ex.Message}", ex.Message));
            }
        }

        /// <summary>
        /// Resolves the source directory and file pattern for creating a shortcut.
        /// </summary>
        /// <param name="version">The installed version.</param>
        /// <param name="targetDir">The component installation directory.</param>
        /// <returns>A tuple containing the source directory and file pattern.</returns>
        private (string sourceDir, string sourcePattern) ResolveShortcutSource(string version, string targetDir)
        {
            if (!string.IsNullOrEmpty(ExecutablePattern))
            {
                var sourceDir = ResolveShortcutSourceDirectory(targetDir);
                return (sourceDir, ExecutablePattern);
            }

            var shortcutName = CreateBinShortcut!.Replace("{version}", version);
            return (targetDir, shortcutName);
        }

        /// <summary>
        /// Resolves the directory containing the executable for shortcut creation.
        /// </summary>
        /// <param name="targetDir">The component installation directory.</param>
        /// <returns>The directory path containing the executable.</returns>
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

        /// <summary>
        /// Creates a global bin directory shortcut (batch file wrapper) for a component executable.
        /// </summary>
        /// <param name="sourceDir">The directory containing the source executable.</param>
        /// <param name="sourcePattern">The filename pattern for the source executable.</param>
        /// <param name="version">The version of the component.</param>
        /// <param name="componentName">The name of the component.</param>
        /// <param name="shortcutName">Optional custom name for the shortcut. Defaults to the source pattern.</param>
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
                    $"Warning: file {sourcePattern} not found for shortcut creation", sourcePattern));
                return;
            }

            var globalBinDir = Path.Combine(DevStackConfig.baseDir, "bin");
            EnsureDirectoryExists(globalBinDir);

            var finalShortcutName = BuildShortcutFileName(shortcutName, version, componentName);
            var shortcutPath = Path.Combine(globalBinDir, finalShortcutName);

            TryCreateShortcutFile(shortcutPath, sourceFile, finalShortcutName);
        }

        /// <summary>
        /// Builds the shortcut filename, using the provided shortcut name or defaulting to componentName-version.cmd.
        /// </summary>
        /// <param name="shortcutName">The custom shortcut name, if provided.</param>
        /// <param name="version">The version of the component.</param>
        /// <param name="componentName">The name of the component.</param>
        /// <returns>The shortcut filename with .cmd extension.</returns>
        private static string BuildShortcutFileName(string? shortcutName, string version, string componentName)
        {
            if (!string.IsNullOrEmpty(shortcutName))
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(shortcutName.Replace("{version}", version));
                return $"{nameWithoutExtension}.cmd";
            }

            return $"{componentName}-{version}.cmd";
        }

        /// <summary>
        /// Creates a batch file shortcut that wraps the source executable.
        /// </summary>
        /// <param name="shortcutPath">The path where the shortcut will be created.</param>
        /// <param name="sourceFile">The path to the source executable.</param>
        /// <param name="finalShortcutName">The final shortcut filename.</param>
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
                    $"Shortcut {nameWithoutExtension} created pointing to {sourceFile}", 
                    nameWithoutExtension, sourceFile));
            }
            catch (Exception ex)
            {
                Console.WriteLine(GetLocalizedString("shared.shortcuts.error_creating", 
                    $"Error creating shortcut: {ex.Message}", ex.Message));
            }
        }

        /// <summary>
        /// Removes a global bin shortcut for a specific component version.
        /// </summary>
        /// <param name="componentName">The name of the component.</param>
        /// <param name="version">The version of the component.</param>
        /// <param name="shortcutPattern">The shortcut filename pattern.</param>
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
                        $"Shortcut {finalShortcutName} removed", finalShortcutName));
                }
                else
                {
                    Console.WriteLine(GetLocalizedString("shared.shortcuts.not_found", 
                        $"Shortcut {finalShortcutName} not found for removal", finalShortcutName));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(GetLocalizedString("shared.shortcuts.error_removing", 
                    $"Error removing shortcut: {ex.Message}", ex.Message));
                DevStackConfig.WriteLog($"Error removing global shortcut: {ex}");
            }
        }

        /// <summary>
        /// Generic uninstallation method that removes a component version from the file system.
        /// Also removes the tool directory from the PATH environment variable.
        /// </summary>
        /// <param name="toolDir">The base directory containing the component.</param>
        /// <param name="subDir">The subdirectory name for the specific version (e.g., "php-8.2").</param>
        public static void UninstallGenericTool(string toolDir, string subDir)
        {
            var targetDir = Path.Combine(toolDir, subDir);
            
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
                DevStackConfig.pathManager?.RemoveFromPath(new[] { targetDir });
                DevStackConfig.WriteColoredLine($"{subDir} removed.", ConsoleColor.Green);
            }
            else
            {
                DevStackConfig.WriteColoredLine($"{subDir} is not installed.", ConsoleColor.Yellow);
            }
        }
    }
}
