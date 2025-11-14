using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para WP-CLI
    /// </summary>
    public class WpcliVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the WP-CLI component.
        /// </summary>
        public string ComponentName => "WP-CLI";
        /// <summary>
        /// Gets the unique identifier for the WP-CLI component.
        /// </summary>
        public string ComponentId => "wpcli";
        
        /// <summary>
        /// List of available WP-CLI versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("0.12.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.12.0/wp-cli-0.12.0.phar"),
            new VersionInfo("0.12.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.12.1/wp-cli-0.12.1.phar"),
            new VersionInfo("0.13.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.13.0/wp-cli-0.13.0.phar"),
            new VersionInfo("0.14.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.14.0/wp-cli-0.14.0.phar"),
            new VersionInfo("0.14.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.14.1/wp-cli-0.14.1.phar"),
            new VersionInfo("0.15.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.15.0/wp-cli-0.15.0.phar"),
            new VersionInfo("0.15.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.15.1/wp-cli-0.15.1.phar"),
            new VersionInfo("0.17.2", "https://github.com/wp-cli/wp-cli/releases/download/v0.17.2/wp-cli-0.17.2.phar"),
            new VersionInfo("0.18.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.18.0/wp-cli-0.18.0.phar"),
            new VersionInfo("0.18.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.18.1/wp-cli-0.18.1.phar"),
            new VersionInfo("0.19.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.19.0/wp-cli-0.19.0.phar"),
            new VersionInfo("0.19.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.19.1/wp-cli-0.19.1.phar"),
            new VersionInfo("0.19.2", "https://github.com/wp-cli/wp-cli/releases/download/v0.19.2/wp-cli-0.19.2.phar"),
            new VersionInfo("0.19.3", "https://github.com/wp-cli/wp-cli/releases/download/v0.19.3/wp-cli-0.19.3.phar"),
            new VersionInfo("0.20.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.20.0/wp-cli-0.20.0.phar"),
            new VersionInfo("0.20.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.20.1/wp-cli-0.20.1.phar"),
            new VersionInfo("0.20.2", "https://github.com/wp-cli/wp-cli/releases/download/v0.20.2/wp-cli-0.20.2.phar"),
            new VersionInfo("0.20.3", "https://github.com/wp-cli/wp-cli/releases/download/v0.20.3/wp-cli-0.20.3.phar"),
            new VersionInfo("0.20.4", "https://github.com/wp-cli/wp-cli/releases/download/v0.20.4/wp-cli-0.20.4.phar"),
            new VersionInfo("0.21.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.21.0/wp-cli-0.21.0.phar"),
            new VersionInfo("0.21.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.21.1/wp-cli-0.21.1.phar"),
            new VersionInfo("0.22.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.22.0/wp-cli-0.22.0.phar"),
            new VersionInfo("0.23.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.23.0/wp-cli-0.23.0.phar"),
            new VersionInfo("0.23.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.23.1/wp-cli-0.23.1.phar"),
            new VersionInfo("0.24.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.24.0/wp-cli-0.24.0.phar"),
            new VersionInfo("0.24.1", "https://github.com/wp-cli/wp-cli/releases/download/v0.24.1/wp-cli-0.24.1.phar"),
            new VersionInfo("0.25.0", "https://github.com/wp-cli/wp-cli/releases/download/v0.25.0/wp-cli-0.25.0.phar"),
            new VersionInfo("1.0.0", "https://github.com/wp-cli/wp-cli/releases/download/v1.0.0/wp-cli-1.0.0.phar"),
            new VersionInfo("1.1.0", "https://github.com/wp-cli/wp-cli/releases/download/v1.1.0/wp-cli-1.1.0.phar"),
            new VersionInfo("1.2.0", "https://github.com/wp-cli/wp-cli/releases/download/v1.2.0/wp-cli-1.2.0.phar"),
            new VersionInfo("1.2.1", "https://github.com/wp-cli/wp-cli/releases/download/v1.2.1/wp-cli-1.2.1.phar"),
            new VersionInfo("1.3.0", "https://github.com/wp-cli/wp-cli/releases/download/v1.3.0/wp-cli-1.3.0.phar"),
            new VersionInfo("1.4.0", "https://github.com/wp-cli/wp-cli/releases/download/v1.4.0/wp-cli-1.4.0.phar"),
            new VersionInfo("1.4.1", "https://github.com/wp-cli/wp-cli/releases/download/v1.4.1/wp-cli-1.4.1.phar"),
            new VersionInfo("1.5.0", "https://github.com/wp-cli/wp-cli/releases/download/v1.5.0/wp-cli-1.5.0.phar"),
            new VersionInfo("1.5.1", "https://github.com/wp-cli/wp-cli/releases/download/v1.5.1/wp-cli-1.5.1.phar"),
            new VersionInfo("2.0.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.0.0/wp-cli-2.0.0.phar"),
            new VersionInfo("2.0.1", "https://github.com/wp-cli/wp-cli/releases/download/v2.0.1/wp-cli-2.0.1.phar"),
            new VersionInfo("2.1.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.1.0/wp-cli-2.1.0.phar"),
            new VersionInfo("2.2.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.2.0/wp-cli-2.2.0.phar"),
            new VersionInfo("2.3.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.3.0/wp-cli-2.3.0.phar"),
            new VersionInfo("2.4.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.4.0/wp-cli-2.4.0.phar"),
            new VersionInfo("2.5.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.5.0/wp-cli-2.5.0.phar"),
            new VersionInfo("2.6.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.6.0/wp-cli-2.6.0.phar"),
            new VersionInfo("2.7.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.7.0/wp-cli-2.7.0.phar"),
            new VersionInfo("2.7.1", "https://github.com/wp-cli/wp-cli/releases/download/v2.7.1/wp-cli-2.7.1.phar"),
            new VersionInfo("2.8.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.8.0/wp-cli-2.8.0.phar"),
            new VersionInfo("2.8.1", "https://github.com/wp-cli/wp-cli/releases/download/v2.8.1/wp-cli-2.8.1.phar"),
            new VersionInfo("2.9.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.9.0/wp-cli-2.9.0.phar"),
            new VersionInfo("2.10.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.10.0/wp-cli-2.10.0.phar"),
            new VersionInfo("2.11.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.11.0/wp-cli-2.11.0.phar"),
            new VersionInfo("2.12.0", "https://github.com/wp-cli/wp-cli/releases/download/v2.12.0/wp-cli-2.12.0.phar")
        };
        
        /// <summary>
        /// Gets the list of all available WP-CLI versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available WP-CLI version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific WP-CLI version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
