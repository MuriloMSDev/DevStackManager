using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para OpenSSL
    /// </summary>
    public class OpensslVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the OpenSSL component.
        /// </summary>
        public string ComponentName => "OpenSSL";
        /// <summary>
        /// Gets the unique identifier for the OpenSSL component.
        /// </summary>
        public string ComponentId => "openssl";
        
        /// <summary>
        /// List of available OpenSSL versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("3.1.8", "https://slproweb.com/download/Win64OpenSSL-3_1_8.exe")
        };
        
        /// <summary>
        /// Gets the list of all available OpenSSL versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available OpenSSL version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific OpenSSL version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
