using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para MongoDB
    /// </summary>
    public class MongodbVersionProvider : IVersionProvider
    {
        public string ComponentName => "MongoDB";
        public string ComponentId => "mongodb";
        
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("4.3.3", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.3.3.zip"),
            new VersionInfo("4.3.4", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.3.4.zip"),
            new VersionInfo("4.3.5", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.3.5.zip"),
            new VersionInfo("4.3.6", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.3.6.zip"),
            new VersionInfo("4.4.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.0.zip"),
            new VersionInfo("4.4.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.1.zip"),
            new VersionInfo("4.4.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.2.zip"),
            new VersionInfo("4.4.3", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.3.zip"),
            new VersionInfo("4.4.4", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.4.zip"),
            new VersionInfo("4.4.5", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.5.zip"),
            new VersionInfo("4.4.6", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.6.zip"),
            new VersionInfo("4.4.7", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.7.zip"),
            new VersionInfo("4.4.8", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.8.zip"),
            new VersionInfo("4.4.9", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.9.zip"),
            new VersionInfo("4.4.10", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.10.zip"),
            new VersionInfo("4.4.11", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.11.zip"),
            new VersionInfo("4.4.12", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.12.zip"),
            new VersionInfo("4.4.13", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.13.zip"),
            new VersionInfo("4.4.14", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.14.zip"),
            new VersionInfo("4.4.15", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.15.zip"),
            new VersionInfo("4.4.16", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.16.zip"),
            new VersionInfo("4.4.17", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.17.zip"),
            new VersionInfo("4.4.18", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.18.zip"),
            new VersionInfo("4.4.19", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.19.zip"),
            new VersionInfo("4.4.20", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.20.zip"),
            new VersionInfo("4.4.21", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.21.zip"),
            new VersionInfo("4.4.22", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.22.zip"),
            new VersionInfo("4.4.23", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.23.zip"),
            new VersionInfo("4.4.24", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.24.zip"),
            new VersionInfo("4.4.25", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.25.zip"),
            new VersionInfo("4.4.26", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.26.zip"),
            new VersionInfo("4.4.27", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.27.zip"),
            new VersionInfo("4.4.28", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.28.zip"),
            new VersionInfo("4.4.29", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-4.4.29.zip"),
            new VersionInfo("5.0.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.0.zip"),
            new VersionInfo("5.0.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.1.zip"),
            new VersionInfo("5.0.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.2.zip"),
            new VersionInfo("5.0.3", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.3.zip"),
            new VersionInfo("5.0.4", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.4.zip"),
            new VersionInfo("5.0.5", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.5.zip"),
            new VersionInfo("5.0.6", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.6.zip"),
            new VersionInfo("5.0.7", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.7.zip"),
            new VersionInfo("5.0.8", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.8.zip"),
            new VersionInfo("5.0.9", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.9.zip"),
            new VersionInfo("5.0.10", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.10.zip"),
            new VersionInfo("5.0.11", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.11.zip"),
            new VersionInfo("5.0.12", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.12.zip"),
            new VersionInfo("5.0.13", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.13.zip"),
            new VersionInfo("5.0.14", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.14.zip"),
            new VersionInfo("5.0.15", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.15.zip"),
            new VersionInfo("5.0.16", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.16.zip"),
            new VersionInfo("5.0.17", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.17.zip"),
            new VersionInfo("5.0.18", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.18.zip"),
            new VersionInfo("5.0.19", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.19.zip"),
            new VersionInfo("5.0.20", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.20.zip"),
            new VersionInfo("5.0.21", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.21.zip"),
            new VersionInfo("5.0.22", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.22.zip"),
            new VersionInfo("5.0.23", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.23.zip"),
            new VersionInfo("5.0.24", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.24.zip"),
            new VersionInfo("5.0.25", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.25.zip"),
            new VersionInfo("5.0.26", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.26.zip"),
            new VersionInfo("5.0.27", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.27.zip"),
            new VersionInfo("5.0.28", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.28.zip"),
            new VersionInfo("5.0.29", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.29.zip"),
            new VersionInfo("5.0.30", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.30.zip"),
            new VersionInfo("5.0.31", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.31.zip"),
            new VersionInfo("5.1.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.1.0.zip"),
            new VersionInfo("5.1.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.1.1.zip"),
            new VersionInfo("5.2.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.2.0.zip"),
            new VersionInfo("5.2.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.2.1.zip"),
            new VersionInfo("5.3.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.3.1.zip"),
            new VersionInfo("5.3.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.3.2.zip"),
            new VersionInfo("6.0.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.0.zip"),
            new VersionInfo("6.0.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.1.zip"),
            new VersionInfo("6.0.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.2.zip"),
            new VersionInfo("6.0.3", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.3.zip"),
            new VersionInfo("6.0.4", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.4.zip"),
            new VersionInfo("6.0.5", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.5.zip"),
            new VersionInfo("6.0.6", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.6.zip"),
            new VersionInfo("6.0.7", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.7.zip"),
            new VersionInfo("6.0.8", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.8.zip"),
            new VersionInfo("6.0.9", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.9.zip"),
            new VersionInfo("6.0.10", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.10.zip"),
            new VersionInfo("6.0.11", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.11.zip"),
            new VersionInfo("6.0.12", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.12.zip"),
            new VersionInfo("6.0.13", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.13.zip"),
            new VersionInfo("6.0.14", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.14.zip"),
            new VersionInfo("6.0.15", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.15.zip"),
            new VersionInfo("6.0.16", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.16.zip"),
            new VersionInfo("6.0.17", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.17.zip"),
            new VersionInfo("6.0.18", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.18.zip"),
            new VersionInfo("6.0.19", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.19.zip"),
            new VersionInfo("6.0.20", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.20.zip"),
            new VersionInfo("6.0.21", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.21.zip"),
            new VersionInfo("6.0.22", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.22.zip"),
            new VersionInfo("6.0.23", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.23.zip"),
            new VersionInfo("6.0.24", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.0.24.zip"),
            new VersionInfo("6.1.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.1.0.zip"),
            new VersionInfo("6.1.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.1.1.zip"),
            new VersionInfo("6.2.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.2.0.zip"),
            new VersionInfo("6.2.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.2.1.zip"),
            new VersionInfo("6.3.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.3.0.zip"),
            new VersionInfo("6.3.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.3.1.zip"),
            new VersionInfo("6.3.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-6.3.2.zip"),
            new VersionInfo("7.0.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.0.zip"),
            new VersionInfo("7.0.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.1.zip"),
            new VersionInfo("7.0.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.2.zip"),
            new VersionInfo("7.0.3", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.3.zip"),
            new VersionInfo("7.0.4", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.4.zip"),
            new VersionInfo("7.0.5", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.5.zip"),
            new VersionInfo("7.0.6", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.6.zip"),
            new VersionInfo("7.0.7", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.7.zip"),
            new VersionInfo("7.0.8", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.8.zip"),
            new VersionInfo("7.0.9", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.9.zip"),
            new VersionInfo("7.0.11", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.11.zip"),
            new VersionInfo("7.0.12", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.12.zip"),
            new VersionInfo("7.0.14", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.14.zip"),
            new VersionInfo("7.0.15", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.15.zip"),
            new VersionInfo("7.0.16", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.16.zip"),
            new VersionInfo("7.0.17", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.17.zip"),
            new VersionInfo("7.0.18", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.18.zip"),
            new VersionInfo("7.0.19", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.19.zip"),
            new VersionInfo("7.0.20", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.20.zip"),
            new VersionInfo("7.0.21", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.0.21.zip"),
            new VersionInfo("7.1.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.1.0.zip"),
            new VersionInfo("7.1.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.1.1.zip"),
            new VersionInfo("7.2.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.2.0.zip"),
            new VersionInfo("7.2.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.2.1.zip"),
            new VersionInfo("7.2.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.2.2.zip"),
            new VersionInfo("7.3.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.3.0.zip"),
            new VersionInfo("7.3.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.3.1.zip"),
            new VersionInfo("7.3.2", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.3.2.zip"),
            new VersionInfo("7.3.3", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.3.3.zip"),
            new VersionInfo("7.3.4", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-7.3.4.zip"),
            new VersionInfo("8.0.0", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.0.zip"),
            new VersionInfo("8.0.1", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.1.zip"),
            new VersionInfo("8.0.3", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.3.zip"),
            new VersionInfo("8.0.4", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.4.zip"),
            new VersionInfo("8.0.5", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.5.zip"),
            new VersionInfo("8.0.6", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.6.zip"),
            new VersionInfo("8.0.7", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.7.zip"),
            new VersionInfo("8.0.8", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.8.zip"),
            new VersionInfo("8.0.9", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.9.zip"),
            new VersionInfo("8.0.10", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.10.zip"),
            new VersionInfo("8.0.11", "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.0.11.zip")
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
