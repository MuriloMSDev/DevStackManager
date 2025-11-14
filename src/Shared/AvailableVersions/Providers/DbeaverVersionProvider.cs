using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para DBeaver
    /// </summary>
    public class DbeaverVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the DBeaver component.
        /// </summary>
        public string ComponentName => "DBeaver";
        /// <summary>
        /// Gets the unique identifier for the DBeaver component.
        /// </summary>
        public string ComponentId => "dbeaver";
        
        /// <summary>
        /// List of available DBeaver versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("3.5.9", "https://dbeaver.io/files/3.5.9/dbeaver-ce-3.5.9-win32.win32.x86_64.zip"),
            new VersionInfo("3.6.10", "https://dbeaver.io/files/3.6.10/dbeaver-ce-3.6.10-win32.win32.x86_64.zip"),
            new VersionInfo("3.7.8", "https://dbeaver.io/files/3.7.8/dbeaver-ce-3.7.8-win32.win32.x86_64.zip"),
            new VersionInfo("3.8.5", "https://dbeaver.io/files/3.8.5/dbeaver-ce-3.8.5-win32.win32.x86_64.zip"),
            new VersionInfo("4.0.5", "https://dbeaver.io/files/4.0.5/dbeaver-ce-4.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("4.1.3", "https://dbeaver.io/files/4.1.3/dbeaver-ce-4.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("4.2.6", "https://dbeaver.io/files/4.2.6/dbeaver-ce-4.2.6-win32.win32.x86_64.zip"),
            new VersionInfo("5.0.6", "https://dbeaver.io/files/5.0.6/dbeaver-ce-5.0.6-win32.win32.x86_64.zip"),
            new VersionInfo("5.1.6", "https://dbeaver.io/files/5.1.6/dbeaver-ce-5.1.6-win32.win32.x86_64.zip"),
            new VersionInfo("5.2.5", "https://dbeaver.io/files/5.2.5/dbeaver-ce-5.2.5-win32.win32.x86_64.zip"),
            new VersionInfo("5.3.5", "https://dbeaver.io/files/5.3.5/dbeaver-ce-5.3.5-win32.win32.x86_64.zip"),
            new VersionInfo("6.0.0", "https://dbeaver.io/files/6.0.0/dbeaver-ce-6.0.0-win32.win32.x86_64.zip"),
            new VersionInfo("6.0.5", "https://dbeaver.io/files/6.0.5/dbeaver-ce-6.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("6.1.0", "https://dbeaver.io/files/6.1.0/dbeaver-ce-6.1.0-win32.win32.x86_64.zip"),
            new VersionInfo("6.1.1", "https://dbeaver.io/files/6.1.1/dbeaver-ce-6.1.1-win32.win32.x86_64.zip"),
            new VersionInfo("6.1.2", "https://dbeaver.io/files/6.1.2/dbeaver-ce-6.1.2-win32.win32.x86_64.zip"),
            new VersionInfo("6.1.3", "https://dbeaver.io/files/6.1.3/dbeaver-ce-6.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("6.1.4", "https://dbeaver.io/files/6.1.4/dbeaver-ce-6.1.4-win32.win32.x86_64.zip"),
            new VersionInfo("6.1.5", "https://dbeaver.io/files/6.1.5/dbeaver-ce-6.1.5-win32.win32.x86_64.zip"),
            new VersionInfo("6.2.0", "https://dbeaver.io/files/6.2.0/dbeaver-ce-6.2.0-win32.win32.x86_64.zip"),
            new VersionInfo("6.2.1", "https://dbeaver.io/files/6.2.1/dbeaver-ce-6.2.1-win32.win32.x86_64.zip"),
            new VersionInfo("6.2.2", "https://dbeaver.io/files/6.2.2/dbeaver-ce-6.2.2-win32.win32.x86_64.zip"),
            new VersionInfo("6.2.3", "https://dbeaver.io/files/6.2.3/dbeaver-ce-6.2.3-win32.win32.x86_64.zip"),
            new VersionInfo("6.2.4", "https://dbeaver.io/files/6.2.4/dbeaver-ce-6.2.4-win32.win32.x86_64.zip"),
            new VersionInfo("6.2.5", "https://dbeaver.io/files/6.2.5/dbeaver-ce-6.2.5-win32.win32.x86_64.zip"),
            new VersionInfo("6.3.0", "https://dbeaver.io/files/6.3.0/dbeaver-ce-6.3.0-win32.win32.x86_64.zip"),
            new VersionInfo("6.3.1", "https://dbeaver.io/files/6.3.1/dbeaver-ce-6.3.1-win32.win32.x86_64.zip"),
            new VersionInfo("6.3.2", "https://dbeaver.io/files/6.3.2/dbeaver-ce-6.3.2-win32.win32.x86_64.zip"),
            new VersionInfo("6.3.3", "https://dbeaver.io/files/6.3.3/dbeaver-ce-6.3.3-win32.win32.x86_64.zip"),
            new VersionInfo("6.3.4", "https://dbeaver.io/files/6.3.4/dbeaver-ce-6.3.4-win32.win32.x86_64.zip"),
            new VersionInfo("6.3.5", "https://dbeaver.io/files/6.3.5/dbeaver-ce-6.3.5-win32.win32.x86_64.zip"),
            new VersionInfo("7.0.0", "https://dbeaver.io/files/7.0.0/dbeaver-ce-7.0.0-win32.win32.x86_64.zip"),
            new VersionInfo("7.0.1", "https://dbeaver.io/files/7.0.1/dbeaver-ce-7.0.1-win32.win32.x86_64.zip"),
            new VersionInfo("7.0.2", "https://dbeaver.io/files/7.0.2/dbeaver-ce-7.0.2-win32.win32.x86_64.zip"),
            new VersionInfo("7.0.3", "https://dbeaver.io/files/7.0.3/dbeaver-ce-7.0.3-win32.win32.x86_64.zip"),
            new VersionInfo("7.0.4", "https://dbeaver.io/files/7.0.4/dbeaver-ce-7.0.4-win32.win32.x86_64.zip"),
            new VersionInfo("7.0.5", "https://dbeaver.io/files/7.0.5/dbeaver-ce-7.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("7.1.0", "https://dbeaver.io/files/7.1.0/dbeaver-ce-7.1.0-win32.win32.x86_64.zip"),
            new VersionInfo("7.1.1", "https://dbeaver.io/files/7.1.1/dbeaver-ce-7.1.1-win32.win32.x86_64.zip"),
            new VersionInfo("7.1.2", "https://dbeaver.io/files/7.1.2/dbeaver-ce-7.1.2-win32.win32.x86_64.zip"),
            new VersionInfo("7.1.3", "https://dbeaver.io/files/7.1.3/dbeaver-ce-7.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("7.1.4", "https://dbeaver.io/files/7.1.4/dbeaver-ce-7.1.4-win32.win32.x86_64.zip"),
            new VersionInfo("7.1.5", "https://dbeaver.io/files/7.1.5/dbeaver-ce-7.1.5-win32.win32.x86_64.zip"),
            new VersionInfo("7.2.0", "https://dbeaver.io/files/7.2.0/dbeaver-ce-7.2.0-win32.win32.x86_64.zip"),
            new VersionInfo("7.2.1", "https://dbeaver.io/files/7.2.1/dbeaver-ce-7.2.1-win32.win32.x86_64.zip"),
            new VersionInfo("7.2.2", "https://dbeaver.io/files/7.2.2/dbeaver-ce-7.2.2-win32.win32.x86_64.zip"),
            new VersionInfo("7.2.3", "https://dbeaver.io/files/7.2.3/dbeaver-ce-7.2.3-win32.win32.x86_64.zip"),
            new VersionInfo("7.2.4", "https://dbeaver.io/files/7.2.4/dbeaver-ce-7.2.4-win32.win32.x86_64.zip"),
            new VersionInfo("7.2.5", "https://dbeaver.io/files/7.2.5/dbeaver-ce-7.2.5-win32.win32.x86_64.zip"),
            new VersionInfo("7.3.0", "https://dbeaver.io/files/7.3.0/dbeaver-ce-7.3.0-win32.win32.x86_64.zip"),
            new VersionInfo("7.3.1", "https://dbeaver.io/files/7.3.1/dbeaver-ce-7.3.1-win32.win32.x86_64.zip"),
            new VersionInfo("7.3.2", "https://dbeaver.io/files/7.3.2/dbeaver-ce-7.3.2-win32.win32.x86_64.zip"),
            new VersionInfo("7.3.3", "https://dbeaver.io/files/7.3.3/dbeaver-ce-7.3.3-win32.win32.x86_64.zip"),
            new VersionInfo("7.3.4", "https://dbeaver.io/files/7.3.4/dbeaver-ce-7.3.4-win32.win32.x86_64.zip"),
            new VersionInfo("7.3.5", "https://dbeaver.io/files/7.3.5/dbeaver-ce-7.3.5-win32.win32.x86_64.zip"),
            new VersionInfo("21.0.0", "https://dbeaver.io/files/21.0.0/dbeaver-ce-21.0.0-win32.win32.x86_64.zip"),
            new VersionInfo("21.0.1", "https://dbeaver.io/files/21.0.1/dbeaver-ce-21.0.1-win32.win32.x86_64.zip"),
            new VersionInfo("21.0.2", "https://dbeaver.io/files/21.0.2/dbeaver-ce-21.0.2-win32.win32.x86_64.zip"),
            new VersionInfo("21.0.3", "https://dbeaver.io/files/21.0.3/dbeaver-ce-21.0.3-win32.win32.x86_64.zip"),
            new VersionInfo("21.0.4", "https://dbeaver.io/files/21.0.4/dbeaver-ce-21.0.4-win32.win32.x86_64.zip"),
            new VersionInfo("21.0.5", "https://dbeaver.io/files/21.0.5/dbeaver-ce-21.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("21.1.0", "https://dbeaver.io/files/21.1.0/dbeaver-ce-21.1.0-win32.win32.x86_64.zip"),
            new VersionInfo("21.1.1", "https://dbeaver.io/files/21.1.1/dbeaver-ce-21.1.1-win32.win32.x86_64.zip"),
            new VersionInfo("21.1.2", "https://dbeaver.io/files/21.1.2/dbeaver-ce-21.1.2-win32.win32.x86_64.zip"),
            new VersionInfo("21.1.3", "https://dbeaver.io/files/21.1.3/dbeaver-ce-21.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("21.1.4", "https://dbeaver.io/files/21.1.4/dbeaver-ce-21.1.4-win32.win32.x86_64.zip"),
            new VersionInfo("21.1.5", "https://dbeaver.io/files/21.1.5/dbeaver-ce-21.1.5-win32.win32.x86_64.zip"),
            new VersionInfo("21.2.0", "https://dbeaver.io/files/21.2.0/dbeaver-ce-21.2.0-win32.win32.x86_64.zip"),
            new VersionInfo("21.2.1", "https://dbeaver.io/files/21.2.1/dbeaver-ce-21.2.1-win32.win32.x86_64.zip"),
            new VersionInfo("21.2.2", "https://dbeaver.io/files/21.2.2/dbeaver-ce-21.2.2-win32.win32.x86_64.zip"),
            new VersionInfo("21.2.3", "https://dbeaver.io/files/21.2.3/dbeaver-ce-21.2.3-win32.win32.x86_64.zip"),
            new VersionInfo("21.2.4", "https://dbeaver.io/files/21.2.4/dbeaver-ce-21.2.4-win32.win32.x86_64.zip"),
            new VersionInfo("21.2.5", "https://dbeaver.io/files/21.2.5/dbeaver-ce-21.2.5-win32.win32.x86_64.zip"),
            new VersionInfo("21.3.0", "https://dbeaver.io/files/21.3.0/dbeaver-ce-21.3.0-win32.win32.x86_64.zip"),
            new VersionInfo("21.3.1", "https://dbeaver.io/files/21.3.1/dbeaver-ce-21.3.1-win32.win32.x86_64.zip"),
            new VersionInfo("21.3.2", "https://dbeaver.io/files/21.3.2/dbeaver-ce-21.3.2-win32.win32.x86_64.zip"),
            new VersionInfo("21.3.3", "https://dbeaver.io/files/21.3.3/dbeaver-ce-21.3.3-win32.win32.x86_64.zip"),
            new VersionInfo("21.3.4", "https://dbeaver.io/files/21.3.4/dbeaver-ce-21.3.4-win32.win32.x86_64.zip"),
            new VersionInfo("21.3.5", "https://dbeaver.io/files/21.3.5/dbeaver-ce-21.3.5-win32.win32.x86_64.zip"),
            new VersionInfo("22.0.0", "https://dbeaver.io/files/22.0.0/dbeaver-ce-22.0.0-win32.win32.x86_64.zip"),
            new VersionInfo("22.0.1", "https://dbeaver.io/files/22.0.1/dbeaver-ce-22.0.1-win32.win32.x86_64.zip"),
            new VersionInfo("22.0.2", "https://dbeaver.io/files/22.0.2/dbeaver-ce-22.0.2-win32.win32.x86_64.zip"),
            new VersionInfo("22.0.3", "https://dbeaver.io/files/22.0.3/dbeaver-ce-22.0.3-win32.win32.x86_64.zip"),
            new VersionInfo("22.0.4", "https://dbeaver.io/files/22.0.4/dbeaver-ce-22.0.4-win32.win32.x86_64.zip"),
            new VersionInfo("22.0.5", "https://dbeaver.io/files/22.0.5/dbeaver-ce-22.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("22.1.0", "https://dbeaver.io/files/22.1.0/dbeaver-ce-22.1.0-win32.win32.x86_64.zip"),
            new VersionInfo("22.1.1", "https://dbeaver.io/files/22.1.1/dbeaver-ce-22.1.1-win32.win32.x86_64.zip"),
            new VersionInfo("22.1.2", "https://dbeaver.io/files/22.1.2/dbeaver-ce-22.1.2-win32.win32.x86_64.zip"),
            new VersionInfo("22.1.3", "https://dbeaver.io/files/22.1.3/dbeaver-ce-22.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("22.1.4", "https://dbeaver.io/files/22.1.4/dbeaver-ce-22.1.4-win32.win32.x86_64.zip"),
            new VersionInfo("22.1.5", "https://dbeaver.io/files/22.1.5/dbeaver-ce-22.1.5-win32.win32.x86_64.zip"),
            new VersionInfo("22.2.0", "https://dbeaver.io/files/22.2.0/dbeaver-ce-22.2.0-win32.win32.x86_64.zip"),
            new VersionInfo("22.2.1", "https://dbeaver.io/files/22.2.1/dbeaver-ce-22.2.1-win32.win32.x86_64.zip"),
            new VersionInfo("22.2.2", "https://dbeaver.io/files/22.2.2/dbeaver-ce-22.2.2-win32.win32.x86_64.zip"),
            new VersionInfo("22.2.3", "https://dbeaver.io/files/22.2.3/dbeaver-ce-22.2.3-win32.win32.x86_64.zip"),
            new VersionInfo("22.2.4", "https://dbeaver.io/files/22.2.4/dbeaver-ce-22.2.4-win32.win32.x86_64.zip"),
            new VersionInfo("22.2.5", "https://dbeaver.io/files/22.2.5/dbeaver-ce-22.2.5-win32.win32.x86_64.zip"),
            new VersionInfo("22.3.0", "https://dbeaver.io/files/22.3.0/dbeaver-ce-22.3.0-win32.win32.x86_64.zip"),
            new VersionInfo("22.3.1", "https://dbeaver.io/files/22.3.1/dbeaver-ce-22.3.1-win32.win32.x86_64.zip"),
            new VersionInfo("22.3.2", "https://dbeaver.io/files/22.3.2/dbeaver-ce-22.3.2-win32.win32.x86_64.zip"),
            new VersionInfo("22.3.3", "https://dbeaver.io/files/22.3.3/dbeaver-ce-22.3.3-win32.win32.x86_64.zip"),
            new VersionInfo("22.3.4", "https://dbeaver.io/files/22.3.4/dbeaver-ce-22.3.4-win32.win32.x86_64.zip"),
            new VersionInfo("22.3.5", "https://dbeaver.io/files/22.3.5/dbeaver-ce-22.3.5-win32.win32.x86_64.zip"),
            new VersionInfo("23.0.0", "https://dbeaver.io/files/23.0.0/dbeaver-ce-23.0.0-win32.win32.x86_64.zip"),
            new VersionInfo("23.0.1", "https://dbeaver.io/files/23.0.1/dbeaver-ce-23.0.1-win32.win32.x86_64.zip"),
            new VersionInfo("23.0.2", "https://dbeaver.io/files/23.0.2/dbeaver-ce-23.0.2-win32.win32.x86_64.zip"),
            new VersionInfo("23.0.3", "https://dbeaver.io/files/23.0.3/dbeaver-ce-23.0.3-win32.win32.x86_64.zip"),
            new VersionInfo("23.0.4", "https://dbeaver.io/files/23.0.4/dbeaver-ce-23.0.4-win32.win32.x86_64.zip"),
            new VersionInfo("23.0.5", "https://dbeaver.io/files/23.0.5/dbeaver-ce-23.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("23.1.0", "https://dbeaver.io/files/23.1.0/dbeaver-ce-23.1.0-win32.win32.x86_64.zip"),
            new VersionInfo("23.1.1", "https://dbeaver.io/files/23.1.1/dbeaver-ce-23.1.1-win32.win32.x86_64.zip"),
            new VersionInfo("23.1.2", "https://dbeaver.io/files/23.1.2/dbeaver-ce-23.1.2-win32.win32.x86_64.zip"),
            new VersionInfo("23.1.3", "https://dbeaver.io/files/23.1.3/dbeaver-ce-23.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("23.1.4", "https://dbeaver.io/files/23.1.4/dbeaver-ce-23.1.4-win32.win32.x86_64.zip"),
            new VersionInfo("23.1.5", "https://dbeaver.io/files/23.1.5/dbeaver-ce-23.1.5-win32.win32.x86_64.zip"),
            new VersionInfo("23.2.0", "https://dbeaver.io/files/23.2.0/dbeaver-ce-23.2.0-win32.win32.x86_64.zip"),
            new VersionInfo("23.2.1", "https://dbeaver.io/files/23.2.1/dbeaver-ce-23.2.1-win32.win32.x86_64.zip"),
            new VersionInfo("23.2.2", "https://dbeaver.io/files/23.2.2/dbeaver-ce-23.2.2-win32.win32.x86_64.zip"),
            new VersionInfo("23.2.3", "https://dbeaver.io/files/23.2.3/dbeaver-ce-23.2.3-win32.win32.x86_64.zip"),
            new VersionInfo("23.2.4", "https://dbeaver.io/files/23.2.4/dbeaver-ce-23.2.4-win32.win32.x86_64.zip"),
            new VersionInfo("23.2.5", "https://dbeaver.io/files/23.2.5/dbeaver-ce-23.2.5-win32.win32.x86_64.zip"),
            new VersionInfo("23.3.0", "https://dbeaver.io/files/23.3.0/dbeaver-ce-23.3.0-win32.win32.x86_64.zip"),
            new VersionInfo("23.3.1", "https://dbeaver.io/files/23.3.1/dbeaver-ce-23.3.1-win32.win32.x86_64.zip"),
            new VersionInfo("23.3.2", "https://dbeaver.io/files/23.3.2/dbeaver-ce-23.3.2-win32.win32.x86_64.zip"),
            new VersionInfo("23.3.3", "https://dbeaver.io/files/23.3.3/dbeaver-ce-23.3.3-win32.win32.x86_64.zip"),
            new VersionInfo("23.3.4", "https://dbeaver.io/files/23.3.4/dbeaver-ce-23.3.4-win32.win32.x86_64.zip"),
            new VersionInfo("23.3.5", "https://dbeaver.io/files/23.3.5/dbeaver-ce-23.3.5-win32.win32.x86_64.zip"),
            new VersionInfo("24.0.0", "https://dbeaver.io/files/24.0.0/dbeaver-ce-24.0.0-win32.win32.x86_64.zip"),
            new VersionInfo("24.0.1", "https://dbeaver.io/files/24.0.1/dbeaver-ce-24.0.1-win32.win32.x86_64.zip"),
            new VersionInfo("24.0.2", "https://dbeaver.io/files/24.0.2/dbeaver-ce-24.0.2-win32.win32.x86_64.zip"),
            new VersionInfo("24.0.3", "https://dbeaver.io/files/24.0.3/dbeaver-ce-24.0.3-win32.win32.x86_64.zip"),
            new VersionInfo("24.0.4", "https://dbeaver.io/files/24.0.4/dbeaver-ce-24.0.4-win32.win32.x86_64.zip"),
            new VersionInfo("24.0.5", "https://dbeaver.io/files/24.0.5/dbeaver-ce-24.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("24.1.0", "https://dbeaver.io/files/24.1.0/dbeaver-ce-24.1.0-win32.win32.x86_64.zip"),
            new VersionInfo("24.1.1", "https://dbeaver.io/files/24.1.1/dbeaver-ce-24.1.1-win32.win32.x86_64.zip"),
            new VersionInfo("24.1.2", "https://dbeaver.io/files/24.1.2/dbeaver-ce-24.1.2-win32.win32.x86_64.zip"),
            new VersionInfo("24.1.3", "https://dbeaver.io/files/24.1.3/dbeaver-ce-24.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("24.1.4", "https://dbeaver.io/files/24.1.4/dbeaver-ce-24.1.4-win32.win32.x86_64.zip"),
            new VersionInfo("24.1.5", "https://dbeaver.io/files/24.1.5/dbeaver-ce-24.1.5-win32.win32.x86_64.zip"),
            new VersionInfo("24.2.0", "https://dbeaver.io/files/24.2.0/dbeaver-ce-24.2.0-win32.win32.x86_64.zip"),
            new VersionInfo("24.2.1", "https://dbeaver.io/files/24.2.1/dbeaver-ce-24.2.1-win32.win32.x86_64.zip"),
            new VersionInfo("24.2.2", "https://dbeaver.io/files/24.2.2/dbeaver-ce-24.2.2-win32.win32.x86_64.zip"),
            new VersionInfo("24.2.3", "https://dbeaver.io/files/24.2.3/dbeaver-ce-24.2.3-win32.win32.x86_64.zip"),
            new VersionInfo("24.2.4", "https://dbeaver.io/files/24.2.4/dbeaver-ce-24.2.4-win32.win32.x86_64.zip"),
            new VersionInfo("24.2.5", "https://dbeaver.io/files/24.2.5/dbeaver-ce-24.2.5-win32.win32.x86_64.zip"),
            new VersionInfo("24.3.0", "https://dbeaver.io/files/24.3.0/dbeaver-ce-24.3.0-win32.win32.x86_64.zip"),
            new VersionInfo("24.3.1", "https://dbeaver.io/files/24.3.1/dbeaver-ce-24.3.1-win32.win32.x86_64.zip"),
            new VersionInfo("24.3.2", "https://dbeaver.io/files/24.3.2/dbeaver-ce-24.3.2-win32.win32.x86_64.zip"),
            new VersionInfo("24.3.3", "https://dbeaver.io/files/24.3.3/dbeaver-ce-24.3.3-win32.win32.x86_64.zip"),
            new VersionInfo("24.3.4", "https://dbeaver.io/files/24.3.4/dbeaver-ce-24.3.4-win32.win32.x86_64.zip"),
            new VersionInfo("24.3.5", "https://dbeaver.io/files/24.3.5/dbeaver-ce-24.3.5-win32.win32.x86_64.zip"),
            new VersionInfo("25.0.0", "https://dbeaver.io/files/25.0.0/dbeaver-ce-25.0.0-win32.win32.x86_64.zip"),
            new VersionInfo("25.0.1", "https://dbeaver.io/files/25.0.1/dbeaver-ce-25.0.1-win32.win32.x86_64.zip"),
            new VersionInfo("25.0.2", "https://dbeaver.io/files/25.0.2/dbeaver-ce-25.0.2-win32.win32.x86_64.zip"),
            new VersionInfo("25.0.3", "https://dbeaver.io/files/25.0.3/dbeaver-ce-25.0.3-win32.win32.x86_64.zip"),
            new VersionInfo("25.0.4", "https://dbeaver.io/files/25.0.4/dbeaver-ce-25.0.4-win32.win32.x86_64.zip"),
            new VersionInfo("25.0.5", "https://dbeaver.io/files/25.0.5/dbeaver-ce-25.0.5-win32.win32.x86_64.zip"),
            new VersionInfo("25.1.0", "https://dbeaver.io/files/25.1.0/dbeaver-ce-25.1.0-win32.win32.x86_64.zip"),
            new VersionInfo("25.1.1", "https://dbeaver.io/files/25.1.1/dbeaver-ce-25.1.1-win32.win32.x86_64.zip"),
            new VersionInfo("25.1.2", "https://dbeaver.io/files/25.1.2/dbeaver-ce-25.1.2-win32.win32.x86_64.zip"),
            new VersionInfo("25.1.3", "https://dbeaver.io/files/25.1.3/dbeaver-ce-25.1.3-win32.win32.x86_64.zip"),
            new VersionInfo("25.1.4", "https://dbeaver.io/files/25.1.4/dbeaver-ce-25.1.4-win32.win32.x86_64.zip"),
            new VersionInfo("25.1.5", "https://dbeaver.io/files/25.1.5/dbeaver-ce-25.1.5-win32.win32.x86_64.zip"),
            new VersionInfo("25.2.0", "https://dbeaver.io/files/25.2.0/dbeaver-ce-25.2.0-win32.win32.x86_64.zip"),
            new VersionInfo("25.2.1", "https://dbeaver.io/files/25.2.1/dbeaver-ce-25.2.1-win32.win32.x86_64.zip"),
            new VersionInfo("25.2.2", "https://dbeaver.io/files/25.2.2/dbeaver-ce-25.2.2-win32.win32.x86_64.zip"),
            new VersionInfo("25.2.3", "https://dbeaver.io/files/25.2.3/dbeaver-ce-25.2.3-win32.win32.x86_64.zip"),
            new VersionInfo("25.2.4", "https://dbeaver.io/files/25.2.4/dbeaver-ce-25.2.4-win32.win32.x86_64.zip")
        };
        
        /// <summary>
        /// Gets the list of all available DBeaver versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available DBeaver version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific DBeaver version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
