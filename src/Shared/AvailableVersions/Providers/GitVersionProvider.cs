using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para Git
    /// </summary>
    public class GitVersionProvider : IVersionProvider
    {
        public string ComponentName => "Git";
        public string ComponentId => "git";
        
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("2.9.2", "https://github.com/git-for-windows/git/releases/download/v2.9.2.windows.1/MinGit-2.9.2-64-bit.zip"),
            new VersionInfo("2.9.3", "https://github.com/git-for-windows/git/releases/download/v2.9.3.windows.1/MinGit-2.9.3-64-bit.zip"),
            new VersionInfo("2.10.0", "https://github.com/git-for-windows/git/releases/download/v2.10.0.windows.1/MinGit-2.10.0-64-bit.zip"),
            new VersionInfo("2.10.1", "https://github.com/git-for-windows/git/releases/download/v2.10.1.windows.1/MinGit-2.10.1-64-bit.zip"),
            new VersionInfo("2.10.2", "https://github.com/git-for-windows/git/releases/download/v2.10.2.windows.1/MinGit-2.10.2-64-bit.zip"),
            new VersionInfo("2.11.0", "https://github.com/git-for-windows/git/releases/download/v2.11.0.windows.1/MinGit-2.11.0-64-bit.zip"),
            new VersionInfo("2.11.1", "https://github.com/git-for-windows/git/releases/download/v2.11.1.windows.1/MinGit-2.11.1-64-bit.zip"),
            new VersionInfo("2.12.0", "https://github.com/git-for-windows/git/releases/download/v2.12.0.windows.1/MinGit-2.12.0-64-bit.zip"),
            new VersionInfo("2.12.1", "https://github.com/git-for-windows/git/releases/download/v2.12.1.windows.1/MinGit-2.12.1-64-bit.zip"),
            new VersionInfo("2.12.2", "https://github.com/git-for-windows/git/releases/download/v2.12.2.windows.1/MinGit-2.12.2-64-bit.zip"),
            new VersionInfo("2.13.0", "https://github.com/git-for-windows/git/releases/download/v2.13.0.windows.1/MinGit-2.13.0-64-bit.zip"),
            new VersionInfo("2.13.1", "https://github.com/git-for-windows/git/releases/download/v2.13.1.windows.1/MinGit-2.13.1-64-bit.zip"),
            new VersionInfo("2.13.2", "https://github.com/git-for-windows/git/releases/download/v2.13.2.windows.1/MinGit-2.13.2-64-bit.zip"),
            new VersionInfo("2.13.3", "https://github.com/git-for-windows/git/releases/download/v2.13.3.windows.1/MinGit-2.13.3-64-bit.zip"),
            new VersionInfo("2.14.0", "https://github.com/git-for-windows/git/releases/download/v2.14.0.windows.1/MinGit-2.14.0-64-bit.zip"),
            new VersionInfo("2.14.1", "https://github.com/git-for-windows/git/releases/download/v2.14.1.windows.1/MinGit-2.14.1-64-bit.zip"),
            new VersionInfo("2.14.2", "https://github.com/git-for-windows/git/releases/download/v2.14.2.windows.1/MinGit-2.14.2-64-bit.zip"),
            new VersionInfo("2.14.3", "https://github.com/git-for-windows/git/releases/download/v2.14.3.windows.1/MinGit-2.14.3-64-bit.zip"),
            new VersionInfo("2.15.0", "https://github.com/git-for-windows/git/releases/download/v2.15.0.windows.1/MinGit-2.15.0-64-bit.zip"),
            new VersionInfo("2.15.1", "https://github.com/git-for-windows/git/releases/download/v2.15.1.windows.1/MinGit-2.15.1-64-bit.zip"),
            new VersionInfo("2.16.1", "https://github.com/git-for-windows/git/releases/download/v2.16.1.windows.1/MinGit-2.16.1-64-bit.zip"),
            new VersionInfo("2.16.2", "https://github.com/git-for-windows/git/releases/download/v2.16.2.windows.1/MinGit-2.16.2-64-bit.zip"),
            new VersionInfo("2.16.3", "https://github.com/git-for-windows/git/releases/download/v2.16.3.windows.1/MinGit-2.16.3-64-bit.zip"),
            new VersionInfo("2.17.0", "https://github.com/git-for-windows/git/releases/download/v2.17.0.windows.1/MinGit-2.17.0-64-bit.zip"),
            new VersionInfo("2.17.1", "https://github.com/git-for-windows/git/releases/download/v2.17.1.windows.1/MinGit-2.17.1-64-bit.zip"),
            new VersionInfo("2.18.0", "https://github.com/git-for-windows/git/releases/download/v2.18.0.windows.1/MinGit-2.18.0-64-bit.zip"),
            new VersionInfo("2.19.0", "https://github.com/git-for-windows/git/releases/download/v2.19.0.windows.1/MinGit-2.19.0-64-bit.zip"),
            new VersionInfo("2.19.1", "https://github.com/git-for-windows/git/releases/download/v2.19.1.windows.1/MinGit-2.19.1-64-bit.zip"),
            new VersionInfo("2.19.2", "https://github.com/git-for-windows/git/releases/download/v2.19.2.windows.1/MinGit-2.19.2-64-bit.zip"),
            new VersionInfo("2.20.0", "https://github.com/git-for-windows/git/releases/download/v2.20.0.windows.1/MinGit-2.20.0-64-bit.zip"),
            new VersionInfo("2.20.1", "https://github.com/git-for-windows/git/releases/download/v2.20.1.windows.1/MinGit-2.20.1-64-bit.zip"),
            new VersionInfo("2.21.0", "https://github.com/git-for-windows/git/releases/download/v2.21.0.windows.1/MinGit-2.21.0-64-bit.zip"),
            new VersionInfo("2.22.0", "https://github.com/git-for-windows/git/releases/download/v2.22.0.windows.1/MinGit-2.22.0-64-bit.zip"),
            new VersionInfo("2.23.0", "https://github.com/git-for-windows/git/releases/download/v2.23.0.windows.1/MinGit-2.23.0-64-bit.zip"),
            new VersionInfo("2.24.0", "https://github.com/git-for-windows/git/releases/download/v2.24.0.windows.1/MinGit-2.24.0-64-bit.zip"),
            new VersionInfo("2.25.0", "https://github.com/git-for-windows/git/releases/download/v2.25.0.windows.1/MinGit-2.25.0-64-bit.zip"),
            new VersionInfo("2.25.1", "https://github.com/git-for-windows/git/releases/download/v2.25.1.windows.1/MinGit-2.25.1-64-bit.zip"),
            new VersionInfo("2.26.0", "https://github.com/git-for-windows/git/releases/download/v2.26.0.windows.1/MinGit-2.26.0-64-bit.zip"),
            new VersionInfo("2.26.1", "https://github.com/git-for-windows/git/releases/download/v2.26.1.windows.1/MinGit-2.26.1-64-bit.zip"),
            new VersionInfo("2.26.2", "https://github.com/git-for-windows/git/releases/download/v2.26.2.windows.1/MinGit-2.26.2-64-bit.zip"),
            new VersionInfo("2.27.0", "https://github.com/git-for-windows/git/releases/download/v2.27.0.windows.1/MinGit-2.27.0-64-bit.zip"),
            new VersionInfo("2.28.0", "https://github.com/git-for-windows/git/releases/download/v2.28.0.windows.1/MinGit-2.28.0-64-bit.zip"),
            new VersionInfo("2.29.0", "https://github.com/git-for-windows/git/releases/download/v2.29.0.windows.1/MinGit-2.29.0-64-bit.zip"),
            new VersionInfo("2.29.1", "https://github.com/git-for-windows/git/releases/download/v2.29.1.windows.1/MinGit-2.29.1-64-bit.zip"),
            new VersionInfo("2.29.2", "https://github.com/git-for-windows/git/releases/download/v2.29.2.windows.1/MinGit-2.29.2-64-bit.zip"),
            new VersionInfo("2.30.0", "https://github.com/git-for-windows/git/releases/download/v2.30.0.windows.1/MinGit-2.30.0-64-bit.zip"),
            new VersionInfo("2.30.1", "https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/MinGit-2.30.1-64-bit.zip"),
            new VersionInfo("2.30.2", "https://github.com/git-for-windows/git/releases/download/v2.30.2.windows.1/MinGit-2.30.2-64-bit.zip"),
            new VersionInfo("2.31.0", "https://github.com/git-for-windows/git/releases/download/v2.31.0.windows.1/MinGit-2.31.0-64-bit.zip"),
            new VersionInfo("2.31.1", "https://github.com/git-for-windows/git/releases/download/v2.31.1.windows.1/MinGit-2.31.1-64-bit.zip"),
            new VersionInfo("2.32.0", "https://github.com/git-for-windows/git/releases/download/v2.32.0.windows.1/MinGit-2.32.0-64-bit.zip"),
            new VersionInfo("2.33.0", "https://github.com/git-for-windows/git/releases/download/v2.33.0.windows.1/MinGit-2.33.0-64-bit.zip"),
            new VersionInfo("2.33.1", "https://github.com/git-for-windows/git/releases/download/v2.33.1.windows.1/MinGit-2.33.1-64-bit.zip"),
            new VersionInfo("2.34.0", "https://github.com/git-for-windows/git/releases/download/v2.34.0.windows.1/MinGit-2.34.0-64-bit.zip"),
            new VersionInfo("2.34.1", "https://github.com/git-for-windows/git/releases/download/v2.34.1.windows.1/MinGit-2.34.1-64-bit.zip"),
            new VersionInfo("2.35.0", "https://github.com/git-for-windows/git/releases/download/v2.35.0.windows.1/MinGit-2.35.0-64-bit.zip"),
            new VersionInfo("2.35.1", "https://github.com/git-for-windows/git/releases/download/v2.35.1.windows.1/MinGit-2.35.1-64-bit.zip"),
            new VersionInfo("2.35.2", "https://github.com/git-for-windows/git/releases/download/v2.35.2.windows.1/MinGit-2.35.2-64-bit.zip"),
            new VersionInfo("2.35.3", "https://github.com/git-for-windows/git/releases/download/v2.35.3.windows.1/MinGit-2.35.3-64-bit.zip"),
            new VersionInfo("2.36.0", "https://github.com/git-for-windows/git/releases/download/v2.36.0.windows.1/MinGit-2.36.0-64-bit.zip"),
            new VersionInfo("2.36.1", "https://github.com/git-for-windows/git/releases/download/v2.36.1.windows.1/MinGit-2.36.1-64-bit.zip"),
            new VersionInfo("2.37.0", "https://github.com/git-for-windows/git/releases/download/v2.37.0.windows.1/MinGit-2.37.0-64-bit.zip"),
            new VersionInfo("2.37.1", "https://github.com/git-for-windows/git/releases/download/v2.37.1.windows.1/MinGit-2.37.1-64-bit.zip"),
            new VersionInfo("2.37.2", "https://github.com/git-for-windows/git/releases/download/v2.37.2.windows.1/MinGit-2.37.2-64-bit.zip"),
            new VersionInfo("2.37.3", "https://github.com/git-for-windows/git/releases/download/v2.37.3.windows.1/MinGit-2.37.3-64-bit.zip"),
            new VersionInfo("2.38.0", "https://github.com/git-for-windows/git/releases/download/v2.38.0.windows.1/MinGit-2.38.0-64-bit.zip"),
            new VersionInfo("2.38.1", "https://github.com/git-for-windows/git/releases/download/v2.38.1.windows.1/MinGit-2.38.1-64-bit.zip"),
            new VersionInfo("2.39.0", "https://github.com/git-for-windows/git/releases/download/v2.39.0.windows.1/MinGit-2.39.0-64-bit.zip"),
            new VersionInfo("2.39.1", "https://github.com/git-for-windows/git/releases/download/v2.39.1.windows.1/MinGit-2.39.1-64-bit.zip"),
            new VersionInfo("2.39.2", "https://github.com/git-for-windows/git/releases/download/v2.39.2.windows.1/MinGit-2.39.2-64-bit.zip"),
            new VersionInfo("2.40.0", "https://github.com/git-for-windows/git/releases/download/v2.40.0.windows.1/MinGit-2.40.0-64-bit.zip"),
            new VersionInfo("2.40.1", "https://github.com/git-for-windows/git/releases/download/v2.40.1.windows.1/MinGit-2.40.1-64-bit.zip"),
            new VersionInfo("2.41.0", "https://github.com/git-for-windows/git/releases/download/v2.41.0.windows.1/MinGit-2.41.0-64-bit.zip"),
            new VersionInfo("2.42.0", "https://github.com/git-for-windows/git/releases/download/v2.42.0.windows.1/MinGit-2.42.0-64-bit.zip"),
            new VersionInfo("2.43.0", "https://github.com/git-for-windows/git/releases/download/v2.43.0.windows.1/MinGit-2.43.0-64-bit.zip"),
            new VersionInfo("2.44.0", "https://github.com/git-for-windows/git/releases/download/v2.44.0.windows.1/MinGit-2.44.0-64-bit.zip"),
            new VersionInfo("2.44.1", "https://github.com/git-for-windows/git/releases/download/v2.44.1.windows.1/MinGit-2.44.1-64-bit.zip"),
            new VersionInfo("2.45.0", "https://github.com/git-for-windows/git/releases/download/v2.45.0.windows.1/MinGit-2.45.0-64-bit.zip"),
            new VersionInfo("2.45.1", "https://github.com/git-for-windows/git/releases/download/v2.45.1.windows.1/MinGit-2.45.1-64-bit.zip"),
            new VersionInfo("2.45.2", "https://github.com/git-for-windows/git/releases/download/v2.45.2.windows.1/MinGit-2.45.2-64-bit.zip"),
            new VersionInfo("2.46.0", "https://github.com/git-for-windows/git/releases/download/v2.46.0.windows.1/MinGit-2.46.0-64-bit.zip"),
            new VersionInfo("2.46.1", "https://github.com/git-for-windows/git/releases/download/v2.46.1.windows.1/MinGit-2.46.1-64-bit.zip"),
            new VersionInfo("2.46.2", "https://github.com/git-for-windows/git/releases/download/v2.46.2.windows.1/MinGit-2.46.2-64-bit.zip"),
            new VersionInfo("2.47.0", "https://github.com/git-for-windows/git/releases/download/v2.47.0.windows.1/MinGit-2.47.0-64-bit.zip"),
            new VersionInfo("2.47.1", "https://github.com/git-for-windows/git/releases/download/v2.47.1.windows.1/MinGit-2.47.1-64-bit.zip"),
            new VersionInfo("2.47.3", "https://github.com/git-for-windows/git/releases/download/v2.47.3.windows.1/MinGit-2.47.3-64-bit.zip"),
            new VersionInfo("2.48.1", "https://github.com/git-for-windows/git/releases/download/v2.48.1.windows.1/MinGit-2.48.1-64-bit.zip"),
            new VersionInfo("2.49.0", "https://github.com/git-for-windows/git/releases/download/v2.49.0.windows.1/MinGit-2.49.0-64-bit.zip"),
            new VersionInfo("2.49.1", "https://github.com/git-for-windows/git/releases/download/v2.49.1.windows.1/MinGit-2.49.1-64-bit.zip"),
            new VersionInfo("2.50.0", "https://github.com/git-for-windows/git/releases/download/v2.50.0.windows.1/MinGit-2.50.0-64-bit.zip"),
            new VersionInfo("2.50.1", "https://github.com/git-for-windows/git/releases/download/v2.50.1.windows.1/MinGit-2.50.1-64-bit.zip"),
            new VersionInfo("2.51.0", "https://github.com/git-for-windows/git/releases/download/v2.51.0.windows.1/MinGit-2.51.0-64-bit.zip"),
            new VersionInfo("2.51.1", "https://github.com/git-for-windows/git/releases/download/v2.51.1.windows.1/MinGit-2.51.1-64-bit.zip"),
            new VersionInfo("2.51.2", "https://github.com/git-for-windows/git/releases/download/v2.51.2.windows.1/MinGit-2.51.2-64-bit.zip")
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
