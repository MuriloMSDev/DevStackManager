using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para PostgreSQL
    /// </summary>
    public class PgsqlVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the PostgreSQL component.
        /// </summary>
        public string ComponentName => "PostgreSQL";
        /// <summary>
        /// Gets the unique identifier for the PostgreSQL component.
        /// </summary>
        public string ComponentId => "pgsql";
        
        /// <summary>
        /// List of available PostgreSQL versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("9.2.24", "https://get.enterprisedb.com/postgresql/postgresql-9.2.24-1-windows-x64-binaries.zip?ls=Crossover&amp;type=Crossover"),
            new VersionInfo("9.3.25", "https://get.enterprisedb.com/postgresql/postgresql-9.3.25-1-windows-x64-binaries.zip?ls=Crossover&amp;type=Crossover"),
            new VersionInfo("9.4.26", "https://sbp.enterprisedb.com/getfile.jsp?fileid=12372"),
            new VersionInfo("9.6.24", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1257907"),
            new VersionInfo("10.23.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1258258"),
            new VersionInfo("11.21.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1258670"),
            new VersionInfo("12.22.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1259241"),
            new VersionInfo("13.21.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1259611"),
            new VersionInfo("14.18.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1259608"),
            new VersionInfo("15.13.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1259618"),
            new VersionInfo("16.9.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1259621"),
            new VersionInfo("17.5.0", "https://sbp.enterprisedb.com/getfile.jsp?fileid=1259623")
        };
        
        /// <summary>
        /// Gets the list of all available PostgreSQL versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available PostgreSQL version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific PostgreSQL version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
