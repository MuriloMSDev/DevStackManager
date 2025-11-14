using System.Collections.Generic;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Interface for component version providers.
    /// Defines the contract for retrieving available versions of DevStack components from various sources
    /// (GitHub releases, official download pages, package registries, etc.).
    /// </summary>
    public interface IVersionProvider
    {
        /// <summary>
        /// Gets the human-readable display name of the component (e.g., "PHP", "Node.js", "MySQL").
        /// Used for UI display and logging purposes.
        /// </summary>
        string ComponentName { get; }
        
        /// <summary>
        /// Gets the unique identifier for the component (e.g., "php", "node", "mysql").
        /// Must match the component name used throughout DevStack for consistency.
        /// Used as a key for component lookup and registration.
        /// </summary>
        string ComponentId { get; }
        
        /// <summary>
        /// Retrieves all available versions of the component from the version source.
        /// Returns versions in ascending order (oldest to newest) for consistent display.
        /// </summary>
        /// <returns>List of VersionInfo objects containing version numbers, download URLs, and metadata.</returns>
        List<VersionInfo> GetAvailableVersions();
        
        /// <summary>
        /// Retrieves the most recent available version of the component.
        /// Useful for "install latest" operations and version update checks.
        /// </summary>
        /// <returns>VersionInfo for the latest version, or null if no versions are available.</returns>
        VersionInfo? GetLatestVersion();
        
        /// <summary>
        /// Searches for a specific version by its version number string.
        /// Performs exact match comparison on the version string.
        /// </summary>
        /// <param name="version">The version number to search for (e.g., "8.2.5", "20.10.0").</param>
        /// <returns>VersionInfo for the specified version, or null if not found.</returns>
        VersionInfo? GetVersion(string version);
    }
}
