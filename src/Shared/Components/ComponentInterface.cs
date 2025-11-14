using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Interface defining the contract for all DevStack component implementations.
    /// Components represent installable development tools like PHP, Nginx, MySQL, etc.
    /// </summary>
    public interface ComponentInterface
    {
        /// <summary>
        /// Installs a specific version of the component. If no version is specified, installs the latest version.
        /// </summary>
        /// <param name="version">The version to install, or null for the latest version.</param>
        /// <returns>A task representing the asynchronous installation operation.</returns>
        Task Install(string? version = null);
        
        /// <summary>
        /// Uninstalls a specific version of the component.
        /// </summary>
        /// <param name="version">The version to uninstall. Must be specified.</param>
        void Uninstall(string? version = null);
        
        /// <summary>
        /// Lists all installed versions of the component on the local system.
        /// </summary>
        /// <returns>A list of installed version strings.</returns>
        List<string> ListInstalled();
        
        /// <summary>
        /// Lists all available versions of the component that can be installed.
        /// </summary>
        /// <returns>A list of available version strings from the version provider.</returns>
        List<string> ListAvailable();
        
        /// <summary>
        /// Gets the unique name identifier for this component (e.g., "php", "nginx").
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the display label for this component.
        /// </summary>
        string Label { get; }
        
        /// <summary>
        /// Gets the base directory where this component is installed.
        /// </summary>
        string ToolDir { get; }
        
        /// <summary>
        /// Gets the latest available version of the component.
        /// </summary>
        /// <returns>The latest version string.</returns>
        string GetLatestVersion();
        
        /// <summary>
        /// Indicates whether this component runs as a background service.
        /// </summary>
        bool IsService { get; }
        
        /// <summary>
        /// Indicates whether this component is an executable program.
        /// </summary>
        bool IsExecutable { get; }
        
        /// <summary>
        /// Indicates whether this component is a command-line tool.
        /// </summary>
        bool IsCommandLine { get; }
        
        /// <summary>
        /// Gets the subfolder containing the executable within the component installation directory.
        /// </summary>
        string? ExecutableFolder { get; }
        
        /// <summary>
        /// Gets the filename pattern for the component's main executable.
        /// </summary>
        string? ExecutablePattern { get; }
        
        /// <summary>
        /// Gets the service name pattern for monitoring running services.
        /// </summary>
        string? ServicePattern { get; }
        
        /// <summary>
        /// Gets the relative path to the binary for which to create a shortcut in the bin folder.
        /// </summary>
        string? CreateBinShortcut { get; }
        
        /// <summary>
        /// Gets the maximum number of worker processes for this component.
        /// </summary>
        int? MaxWorkers { get; }
        
        /// <summary>
        /// Gets the localized service type string for this component (e.g., "Web Server", "Database").
        /// </summary>
        /// <param name="localizationManager">The localization manager for string translation.</param>
        /// <returns>The localized service type string.</returns>
        string GetServiceType(DevStackShared.LocalizationManager localizationManager);
        
        /// <summary>
        /// Gets the service description string with version information.
        /// </summary>
        /// <param name="version">The installed version of the component.</param>
        /// <param name="localizationManager">The localization manager for string translation.</param>
        /// <returns>The formatted service description string.</returns>
        string GetServiceDescription(string version, DevStackShared.LocalizationManager localizationManager);
    }
}
