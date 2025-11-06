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
        public string ComponentName => "OpenSSL";
        public string ComponentId => "openssl";
        
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("3.1.8", "https://slproweb.com/download/Win64OpenSSL-3_1_8.exe")
        };
        
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
