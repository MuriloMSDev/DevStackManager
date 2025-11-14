using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para Python
    /// </summary>
    public class PythonVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the Python component.
        /// </summary>
        public string ComponentName => "Python";
        /// <summary>
        /// Gets the unique identifier for the Python component.
        /// </summary>
        public string ComponentId => "python";
        
        /// <summary>
        /// List of available Python versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("3.5.0", "https://www.python.org/ftp/python/3.5.0/python-3.5.0-embed-amd64.zip"),
            new VersionInfo("3.5.1", "https://www.python.org/ftp/python/3.5.1/python-3.5.1-embed-amd64.zip"),
            new VersionInfo("3.5.2", "https://www.python.org/ftp/python/3.5.2/python-3.5.2-embed-amd64.zip"),
            new VersionInfo("3.5.3", "https://www.python.org/ftp/python/3.5.3/python-3.5.3-embed-amd64.zip"),
            new VersionInfo("3.5.4", "https://www.python.org/ftp/python/3.5.4/python-3.5.4-embed-amd64.zip"),
            new VersionInfo("3.6.0", "https://www.python.org/ftp/python/3.6.0/python-3.6.0-embed-amd64.zip"),
            new VersionInfo("3.6.1", "https://www.python.org/ftp/python/3.6.1/python-3.6.1-embed-amd64.zip"),
            new VersionInfo("3.6.2", "https://www.python.org/ftp/python/3.6.2/python-3.6.2-embed-amd64.zip"),
            new VersionInfo("3.6.3", "https://www.python.org/ftp/python/3.6.3/python-3.6.3-embed-amd64.zip"),
            new VersionInfo("3.6.4", "https://www.python.org/ftp/python/3.6.4/python-3.6.4-embed-amd64.zip"),
            new VersionInfo("3.6.5", "https://www.python.org/ftp/python/3.6.5/python-3.6.5-embed-amd64.zip"),
            new VersionInfo("3.6.6", "https://www.python.org/ftp/python/3.6.6/python-3.6.6-embed-amd64.zip"),
            new VersionInfo("3.6.7", "https://www.python.org/ftp/python/3.6.7/python-3.6.7-embed-amd64.zip"),
            new VersionInfo("3.6.8", "https://www.python.org/ftp/python/3.6.8/python-3.6.8-embed-amd64.zip"),
            new VersionInfo("3.7.0", "https://www.python.org/ftp/python/3.7.0/python-3.7.0-embed-amd64.zip"),
            new VersionInfo("3.7.1", "https://www.python.org/ftp/python/3.7.1/python-3.7.1-embed-amd64.zip"),
            new VersionInfo("3.7.3", "https://www.python.org/ftp/python/3.7.3/python-3.7.3-embed-amd64.zip"),
            new VersionInfo("3.7.4", "https://www.python.org/ftp/python/3.7.4/python-3.7.4-embed-amd64.zip"),
            new VersionInfo("3.7.5", "https://www.python.org/ftp/python/3.7.5/python-3.7.5-embed-amd64.zip"),
            new VersionInfo("3.7.6", "https://www.python.org/ftp/python/3.7.6/python-3.7.6-embed-amd64.zip"),
            new VersionInfo("3.7.7", "https://www.python.org/ftp/python/3.7.7/python-3.7.7-embed-amd64.zip"),
            new VersionInfo("3.7.8", "https://www.python.org/ftp/python/3.7.8/python-3.7.8-embed-amd64.zip"),
            new VersionInfo("3.7.9", "https://www.python.org/ftp/python/3.7.9/python-3.7.9-embed-amd64.zip"),
            new VersionInfo("3.8.0", "https://www.python.org/ftp/python/3.8.0/python-3.8.0-embed-amd64.zip"),
            new VersionInfo("3.8.1", "https://www.python.org/ftp/python/3.8.1/python-3.8.1-embed-amd64.zip"),
            new VersionInfo("3.8.2", "https://www.python.org/ftp/python/3.8.2/python-3.8.2-embed-amd64.zip"),
            new VersionInfo("3.8.3", "https://www.python.org/ftp/python/3.8.3/python-3.8.3-embed-amd64.zip"),
            new VersionInfo("3.8.4", "https://www.python.org/ftp/python/3.8.4/python-3.8.4-embed-amd64.zip"),
            new VersionInfo("3.8.5", "https://www.python.org/ftp/python/3.8.5/python-3.8.5-embed-amd64.zip"),
            new VersionInfo("3.8.6", "https://www.python.org/ftp/python/3.8.6/python-3.8.6-embed-amd64.zip"),
            new VersionInfo("3.8.7", "https://www.python.org/ftp/python/3.8.7/python-3.8.7-embed-amd64.zip"),
            new VersionInfo("3.8.8", "https://www.python.org/ftp/python/3.8.8/python-3.8.8-embed-amd64.zip"),
            new VersionInfo("3.8.9", "https://www.python.org/ftp/python/3.8.9/python-3.8.9-embed-amd64.zip"),
            new VersionInfo("3.8.10", "https://www.python.org/ftp/python/3.8.10/python-3.8.10-embed-amd64.zip"),
            new VersionInfo("3.9.0", "https://www.python.org/ftp/python/3.9.0/python-3.9.0-embed-amd64.zip"),
            new VersionInfo("3.9.1", "https://www.python.org/ftp/python/3.9.1/python-3.9.1-embed-amd64.zip"),
            new VersionInfo("3.9.2", "https://www.python.org/ftp/python/3.9.2/python-3.9.2-embed-amd64.zip"),
            new VersionInfo("3.9.3", "https://www.python.org/ftp/python/3.9.3/python-3.9.3-embed-amd64.zip"),
            new VersionInfo("3.9.4", "https://www.python.org/ftp/python/3.9.4/python-3.9.4-embed-amd64.zip"),
            new VersionInfo("3.9.5", "https://www.python.org/ftp/python/3.9.5/python-3.9.5-embed-amd64.zip"),
            new VersionInfo("3.9.6", "https://www.python.org/ftp/python/3.9.6/python-3.9.6-embed-amd64.zip"),
            new VersionInfo("3.9.7", "https://www.python.org/ftp/python/3.9.7/python-3.9.7-embed-amd64.zip"),
            new VersionInfo("3.9.8", "https://www.python.org/ftp/python/3.9.8/python-3.9.8-embed-amd64.zip"),
            new VersionInfo("3.9.9", "https://www.python.org/ftp/python/3.9.9/python-3.9.9-embed-amd64.zip"),
            new VersionInfo("3.9.10", "https://www.python.org/ftp/python/3.9.10/python-3.9.10-embed-amd64.zip"),
            new VersionInfo("3.9.11", "https://www.python.org/ftp/python/3.9.11/python-3.9.11-embed-amd64.zip"),
            new VersionInfo("3.9.12", "https://www.python.org/ftp/python/3.9.12/python-3.9.12-embed-amd64.zip"),
            new VersionInfo("3.9.13", "https://www.python.org/ftp/python/3.9.13/python-3.9.13-embed-amd64.zip"),
            new VersionInfo("3.10.0", "https://www.python.org/ftp/python/3.10.0/python-3.10.0-embed-amd64.zip"),
            new VersionInfo("3.10.1", "https://www.python.org/ftp/python/3.10.1/python-3.10.1-embed-amd64.zip"),
            new VersionInfo("3.10.2", "https://www.python.org/ftp/python/3.10.2/python-3.10.2-embed-amd64.zip"),
            new VersionInfo("3.10.3", "https://www.python.org/ftp/python/3.10.3/python-3.10.3-embed-amd64.zip"),
            new VersionInfo("3.10.4", "https://www.python.org/ftp/python/3.10.4/python-3.10.4-embed-amd64.zip"),
            new VersionInfo("3.10.5", "https://www.python.org/ftp/python/3.10.5/python-3.10.5-embed-amd64.zip"),
            new VersionInfo("3.10.6", "https://www.python.org/ftp/python/3.10.6/python-3.10.6-embed-amd64.zip"),
            new VersionInfo("3.10.7", "https://www.python.org/ftp/python/3.10.7/python-3.10.7-embed-amd64.zip"),
            new VersionInfo("3.10.8", "https://www.python.org/ftp/python/3.10.8/python-3.10.8-embed-amd64.zip"),
            new VersionInfo("3.10.9", "https://www.python.org/ftp/python/3.10.9/python-3.10.9-embed-amd64.zip"),
            new VersionInfo("3.10.10", "https://www.python.org/ftp/python/3.10.10/python-3.10.10-embed-amd64.zip"),
            new VersionInfo("3.10.11", "https://www.python.org/ftp/python/3.10.11/python-3.10.11-embed-amd64.zip"),
            new VersionInfo("3.11.0", "https://www.python.org/ftp/python/3.11.0/python-3.11.0-amd64.zip"),
            new VersionInfo("3.11.1", "https://www.python.org/ftp/python/3.11.1/python-3.11.1-amd64.zip"),
            new VersionInfo("3.11.2", "https://www.python.org/ftp/python/3.11.2/python-3.11.2-amd64.zip"),
            new VersionInfo("3.11.3", "https://www.python.org/ftp/python/3.11.3/python-3.11.3-amd64.zip"),
            new VersionInfo("3.11.4", "https://www.python.org/ftp/python/3.11.4/python-3.11.4-amd64.zip"),
            new VersionInfo("3.11.5", "https://www.python.org/ftp/python/3.11.5/python-3.11.5-amd64.zip"),
            new VersionInfo("3.11.6", "https://www.python.org/ftp/python/3.11.6/python-3.11.6-amd64.zip"),
            new VersionInfo("3.11.7", "https://www.python.org/ftp/python/3.11.7/python-3.11.7-amd64.zip"),
            new VersionInfo("3.11.8", "https://www.python.org/ftp/python/3.11.8/python-3.11.8-amd64.zip"),
            new VersionInfo("3.11.9", "https://www.python.org/ftp/python/3.11.9/python-3.11.9-amd64.zip"),
            new VersionInfo("3.12.0", "https://www.python.org/ftp/python/3.12.0/python-3.12.0-amd64.zip"),
            new VersionInfo("3.12.1", "https://www.python.org/ftp/python/3.12.1/python-3.12.1-amd64.zip"),
            new VersionInfo("3.12.2", "https://www.python.org/ftp/python/3.12.2/python-3.12.2-amd64.zip"),
            new VersionInfo("3.12.3", "https://www.python.org/ftp/python/3.12.3/python-3.12.3-amd64.zip"),
            new VersionInfo("3.12.4", "https://www.python.org/ftp/python/3.12.4/python-3.12.4-amd64.zip"),
            new VersionInfo("3.12.5", "https://www.python.org/ftp/python/3.12.5/python-3.12.5-embed-amd64.zip"),
            new VersionInfo("3.12.6", "https://www.python.org/ftp/python/3.12.6/python-3.12.6-amd64.zip"),
            new VersionInfo("3.12.7", "https://www.python.org/ftp/python/3.12.7/python-3.12.7-amd64.zip"),
            new VersionInfo("3.12.8", "https://www.python.org/ftp/python/3.12.8/python-3.12.8-amd64.zip"),
            new VersionInfo("3.12.9", "https://www.python.org/ftp/python/3.12.9/python-3.12.9-amd64.zip"),
            new VersionInfo("3.12.10", "https://www.python.org/ftp/python/3.12.10/python-3.12.10-amd64.zip"),
            new VersionInfo("3.13.0", "https://www.python.org/ftp/python/3.13.0/python-3.13.0-amd64.zip"),
            new VersionInfo("3.13.1", "https://www.python.org/ftp/python/3.13.1/python-3.13.1-amd64.zip"),
            new VersionInfo("3.13.2", "https://www.python.org/ftp/python/3.13.2/python-3.13.2-amd64.zip"),
            new VersionInfo("3.13.3", "https://www.python.org/ftp/python/3.13.3/python-3.13.3-amd64.zip"),
            new VersionInfo("3.13.4", "https://www.python.org/ftp/python/3.13.4/python-3.13.4-amd64.zip"),
            new VersionInfo("3.13.5", "https://www.python.org/ftp/python/3.13.5/python-3.13.5-amd64.zip")
        };
        
        /// <summary>
        /// Gets the list of all available Python versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available Python version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific Python version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
