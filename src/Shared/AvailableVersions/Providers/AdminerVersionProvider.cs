using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para Adminer
    /// </summary>
    public class AdminerVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the Adminer component.
        /// </summary>
        public string ComponentName => "Adminer";
        /// <summary>
        /// Gets the unique identifier for the Adminer component.
        /// </summary>
        public string ComponentId => "adminer";
        
        /// <summary>
        /// List of available Adminer versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("4.2.5", "https://github.com/vrana/adminer/releases/download/v4.2.5/adminer-4.2.5.php"),
            new VersionInfo("4.3.0", "https://github.com/vrana/adminer/releases/download/v4.3.0/adminer-4.3.0.php"),
            new VersionInfo("4.3.1", "https://github.com/vrana/adminer/releases/download/v4.3.1/adminer-4.3.1.php"),
            new VersionInfo("4.4.0", "https://github.com/vrana/adminer/releases/download/v4.4.0/adminer-4.4.0.php"),
            new VersionInfo("4.5.0", "https://github.com/vrana/adminer/releases/download/v4.5.0/adminer-4.5.0.php"),
            new VersionInfo("4.6.0", "https://github.com/vrana/adminer/releases/download/v4.6.0/adminer-4.6.0.php"),
            new VersionInfo("4.6.1", "https://github.com/vrana/adminer/releases/download/v4.6.1/adminer-4.6.1.php"),
            new VersionInfo("4.6.2", "https://github.com/vrana/adminer/releases/download/v4.6.2/adminer-4.6.2.php"),
            new VersionInfo("4.6.3", "https://github.com/vrana/adminer/releases/download/v4.6.3/adminer-4.6.3.php"),
            new VersionInfo("4.7.0", "https://github.com/vrana/adminer/releases/download/v4.7.0/adminer-4.7.0.php"),
            new VersionInfo("4.7.1", "https://github.com/vrana/adminer/releases/download/v4.7.1/adminer-4.7.1.php"),
            new VersionInfo("4.7.2", "https://github.com/vrana/adminer/releases/download/v4.7.2/adminer-4.7.2.php"),
            new VersionInfo("4.7.3", "https://github.com/vrana/adminer/releases/download/v4.7.3/adminer-4.7.3.php"),
            new VersionInfo("4.7.4", "https://github.com/vrana/adminer/releases/download/v4.7.4/adminer-4.7.4.php"),
            new VersionInfo("4.7.5", "https://github.com/vrana/adminer/releases/download/v4.7.5/adminer-4.7.5.php"),
            new VersionInfo("4.7.6", "https://github.com/vrana/adminer/releases/download/v4.7.6/adminer-4.7.6.php"),
            new VersionInfo("4.7.7", "https://github.com/vrana/adminer/releases/download/v4.7.7/adminer-4.7.7.php"),
            new VersionInfo("4.7.8", "https://github.com/vrana/adminer/releases/download/v4.7.8/adminer-4.7.8.php"),
            new VersionInfo("4.7.9", "https://github.com/vrana/adminer/releases/download/v4.7.9/adminer-4.7.9.php"),
            new VersionInfo("4.8.0", "https://github.com/vrana/adminer/releases/download/v4.8.0/adminer-4.8.0.php"),
            new VersionInfo("4.8.1", "https://github.com/vrana/adminer/releases/download/v4.8.1/adminer-4.8.1.php"),
            new VersionInfo("4.16.0", "https://github.com/vrana/adminer/releases/download/v4.16.0/adminer-4.16.0.php"),
            new VersionInfo("4.17.0", "https://github.com/vrana/adminer/releases/download/v4.17.0/adminer-4.17.0.php"),
            new VersionInfo("4.17.1", "https://github.com/vrana/adminer/releases/download/v4.17.1/adminer-4.17.1.php"),
            new VersionInfo("5.0.0", "https://github.com/vrana/adminer/releases/download/v5.0.0/adminer-5.0.0.php"),
            new VersionInfo("5.0.1", "https://github.com/vrana/adminer/releases/download/v5.0.1/adminer-5.0.1.php"),
            new VersionInfo("5.0.2", "https://github.com/vrana/adminer/releases/download/v5.0.2/adminer-5.0.2.php"),
            new VersionInfo("5.0.3", "https://github.com/vrana/adminer/releases/download/v5.0.3/adminer-5.0.3.php"),
            new VersionInfo("5.0.4", "https://github.com/vrana/adminer/releases/download/v5.0.4/adminer-5.0.4.php"),
            new VersionInfo("5.0.5", "https://github.com/vrana/adminer/releases/download/v5.0.5/adminer-5.0.5.php"),
            new VersionInfo("5.0.6", "https://github.com/vrana/adminer/releases/download/v5.0.6/adminer-5.0.6.php"),
            new VersionInfo("5.1.0", "https://github.com/vrana/adminer/releases/download/v5.1.0/adminer-5.1.0.php"),
            new VersionInfo("5.1.1", "https://github.com/vrana/adminer/releases/download/v5.1.1/adminer-5.1.1.php"),
            new VersionInfo("5.2.0", "https://github.com/vrana/adminer/releases/download/v5.2.0/adminer-5.2.0.php"),
            new VersionInfo("5.2.1", "https://github.com/vrana/adminer/releases/download/v5.2.1/adminer-5.2.1.php"),
            new VersionInfo("5.3.0", "https://github.com/vrana/adminer/releases/download/v5.3.0/adminer-5.3.0.php"),
            new VersionInfo("5.4.0", "https://github.com/vrana/adminer/releases/download/v5.4.0/adminer-5.4.0.php"),
            new VersionInfo("5.4.1", "https://github.com/vrana/adminer/releases/download/v5.4.1/adminer-5.4.1.php")
        };
        
        /// <summary>
        /// Gets the list of all available Adminer versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available Adminer version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific Adminer version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
