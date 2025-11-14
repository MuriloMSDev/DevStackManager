using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para phpMyAdmin
    /// </summary>
    public class PhpmyadminVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the phpMyAdmin component.
        /// </summary>
        public string ComponentName => "phpMyAdmin";
        /// <summary>
        /// Gets the unique identifier for the phpMyAdmin component.
        /// </summary>
        public string ComponentId => "phpmyadmin";
        
        /// <summary>
        /// List of available phpMyAdmin versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("2.11.11.0", "https://files.phpmyadmin.net/phpMyAdmin/2.11.11/phpMyAdmin-2.11.11-all-languages.zip"),
            new VersionInfo("2.11.11.1", "https://files.phpmyadmin.net/phpMyAdmin/2.11.11.1/phpMyAdmin-2.11.11.1-all-languages.zip"),
            new VersionInfo("2.11.11.2", "https://files.phpmyadmin.net/phpMyAdmin/2.11.11.2/phpMyAdmin-2.11.11.2-all-languages.zip"),
            new VersionInfo("2.11.11.3", "https://files.phpmyadmin.net/phpMyAdmin/2.11.11.3/phpMyAdmin-2.11.11.3-all-languages.zip"),
            new VersionInfo("3.3.5.1", "https://files.phpmyadmin.net/phpMyAdmin/3.3.5.1/phpMyAdmin-3.3.5.1-all-languages.zip"),
            new VersionInfo("3.3.6.0", "https://files.phpmyadmin.net/phpMyAdmin/3.3.6/phpMyAdmin-3.3.6-all-languages.zip"),
            new VersionInfo("3.3.7.0", "https://files.phpmyadmin.net/phpMyAdmin/3.3.7/phpMyAdmin-3.3.7-all-languages.zip"),
            new VersionInfo("3.3.8.0", "https://files.phpmyadmin.net/phpMyAdmin/3.3.8/phpMyAdmin-3.3.8-all-languages.zip"),
            new VersionInfo("3.3.8.1", "https://files.phpmyadmin.net/phpMyAdmin/3.3.8.1/phpMyAdmin-3.3.8.1-all-languages.zip"),
            new VersionInfo("3.3.9.0", "https://files.phpmyadmin.net/phpMyAdmin/3.3.9/phpMyAdmin-3.3.9-all-languages.zip"),
            new VersionInfo("3.3.9.1", "https://files.phpmyadmin.net/phpMyAdmin/3.3.9.1/phpMyAdmin-3.3.9.1-all-languages.zip"),
            new VersionInfo("3.3.9.2", "https://files.phpmyadmin.net/phpMyAdmin/3.3.9.2/phpMyAdmin-3.3.9.2-all-languages.zip"),
            new VersionInfo("3.3.10.0", "https://files.phpmyadmin.net/phpMyAdmin/3.3.10/phpMyAdmin-3.3.10-all-languages.zip"),
            new VersionInfo("3.3.10.1", "https://files.phpmyadmin.net/phpMyAdmin/3.3.10.1/phpMyAdmin-3.3.10.1-all-languages.zip"),
            new VersionInfo("3.3.10.2", "https://files.phpmyadmin.net/phpMyAdmin/3.3.10.2/phpMyAdmin-3.3.10.2-all-languages.zip"),
            new VersionInfo("3.3.10.3", "https://files.phpmyadmin.net/phpMyAdmin/3.3.10.3/phpMyAdmin-3.3.10.3-all-languages.zip"),
            new VersionInfo("3.3.10.4", "https://files.phpmyadmin.net/phpMyAdmin/3.3.10.4/phpMyAdmin-3.3.10.4-all-languages.zip"),
            new VersionInfo("3.3.10.5", "https://files.phpmyadmin.net/phpMyAdmin/3.3.10.5/phpMyAdmin-3.3.10.5-all-languages.zip"),
            new VersionInfo("3.4.0.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.0/phpMyAdmin-3.4.0-all-languages.zip"),
            new VersionInfo("3.4.1.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.1/phpMyAdmin-3.4.1-all-languages.zip"),
            new VersionInfo("3.4.2.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.2/phpMyAdmin-3.4.2-all-languages.zip"),
            new VersionInfo("3.4.3.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.3/phpMyAdmin-3.4.3-all-languages.zip"),
            new VersionInfo("3.4.3.1", "https://files.phpmyadmin.net/phpMyAdmin/3.4.3.1/phpMyAdmin-3.4.3.1-all-languages.zip"),
            new VersionInfo("3.4.3.2", "https://files.phpmyadmin.net/phpMyAdmin/3.4.3.2/phpMyAdmin-3.4.3.2-all-languages.zip"),
            new VersionInfo("3.4.4.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.4/phpMyAdmin-3.4.4-all-languages.zip"),
            new VersionInfo("3.4.5.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.5/phpMyAdmin-3.4.5-all-languages.zip"),
            new VersionInfo("3.4.6.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.6/phpMyAdmin-3.4.6-all-languages.zip"),
            new VersionInfo("3.4.7.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.7/phpMyAdmin-3.4.7-all-languages.zip"),
            new VersionInfo("3.4.7.1", "https://files.phpmyadmin.net/phpMyAdmin/3.4.7.1/phpMyAdmin-3.4.7.1-all-languages.zip"),
            new VersionInfo("3.4.8.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.8/phpMyAdmin-3.4.8-all-languages.zip"),
            new VersionInfo("3.4.9.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.9/phpMyAdmin-3.4.9-all-languages.zip"),
            new VersionInfo("3.4.10.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.10/phpMyAdmin-3.4.10-all-languages.zip"),
            new VersionInfo("3.4.10.1", "https://files.phpmyadmin.net/phpMyAdmin/3.4.10.1/phpMyAdmin-3.4.10.1-all-languages.zip"),
            new VersionInfo("3.4.10.2", "https://files.phpmyadmin.net/phpMyAdmin/3.4.10.2/phpMyAdmin-3.4.10.2-all-languages.zip"),
            new VersionInfo("3.4.11.0", "https://files.phpmyadmin.net/phpMyAdmin/3.4.11/phpMyAdmin-3.4.11-all-languages.zip"),
            new VersionInfo("3.4.11.1", "https://files.phpmyadmin.net/phpMyAdmin/3.4.11.1/phpMyAdmin-3.4.11.1-all-languages.zip"),
            new VersionInfo("3.5.0.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.0/phpMyAdmin-3.5.0-all-languages.zip"),
            new VersionInfo("3.5.1.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.1/phpMyAdmin-3.5.1-all-languages.zip"),
            new VersionInfo("3.5.2.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.2/phpMyAdmin-3.5.2-all-languages.zip"),
            new VersionInfo("3.5.2.1", "https://files.phpmyadmin.net/phpMyAdmin/3.5.2.1/phpMyAdmin-3.5.2.1-all-languages.zip"),
            new VersionInfo("3.5.2.2", "https://files.phpmyadmin.net/phpMyAdmin/3.5.2.2/phpMyAdmin-3.5.2.2-all-languages.zip"),
            new VersionInfo("3.5.3.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.3/phpMyAdmin-3.5.3-all-languages.zip"),
            new VersionInfo("3.5.4.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.4/phpMyAdmin-3.5.4-all-languages.zip"),
            new VersionInfo("3.5.5.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.5/phpMyAdmin-3.5.5-all-languages.zip"),
            new VersionInfo("3.5.6.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.6/phpMyAdmin-3.5.6-all-languages.zip"),
            new VersionInfo("3.5.7.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.7/phpMyAdmin-3.5.7-all-languages.zip"),
            new VersionInfo("3.5.8.0", "https://files.phpmyadmin.net/phpMyAdmin/3.5.8/phpMyAdmin-3.5.8-all-languages.zip"),
            new VersionInfo("3.5.8.1", "https://files.phpmyadmin.net/phpMyAdmin/3.5.8.1/phpMyAdmin-3.5.8.1-all-languages.zip"),
            new VersionInfo("3.5.8.2", "https://files.phpmyadmin.net/phpMyAdmin/3.5.8.2/phpMyAdmin-3.5.8.2-all-languages.zip"),
            new VersionInfo("4.0.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.0/phpMyAdmin-4.0.0-all-languages.zip"),
            new VersionInfo("4.0.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.1/phpMyAdmin-4.0.1-all-languages.zip"),
            new VersionInfo("4.0.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.2/phpMyAdmin-4.0.2-all-languages.zip"),
            new VersionInfo("4.0.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.3/phpMyAdmin-4.0.3-all-languages.zip"),
            new VersionInfo("4.0.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.4/phpMyAdmin-4.0.4-all-languages.zip"),
            new VersionInfo("4.0.4.1", "https://files.phpmyadmin.net/phpMyAdmin/4.0.4.1/phpMyAdmin-4.0.4.1-all-languages.zip"),
            new VersionInfo("4.0.4.2", "https://files.phpmyadmin.net/phpMyAdmin/4.0.4.2/phpMyAdmin-4.0.4.2-all-languages.zip"),
            new VersionInfo("4.0.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.5/phpMyAdmin-4.0.5-all-languages.zip"),
            new VersionInfo("4.0.6.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.6/phpMyAdmin-4.0.6-all-languages.zip"),
            new VersionInfo("4.0.7.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.7/phpMyAdmin-4.0.7-all-languages.zip"),
            new VersionInfo("4.0.8.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.8/phpMyAdmin-4.0.8-all-languages.zip"),
            new VersionInfo("4.0.9.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.9/phpMyAdmin-4.0.9-all-languages.zip"),
            new VersionInfo("4.0.10.0", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10/phpMyAdmin-4.0.10-all-languages.zip"),
            new VersionInfo("4.0.10.1", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.1/phpMyAdmin-4.0.10.1-all-languages.zip"),
            new VersionInfo("4.0.10.2", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.2/phpMyAdmin-4.0.10.2-all-languages.zip"),
            new VersionInfo("4.0.10.3", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.3/phpMyAdmin-4.0.10.3-all-languages.zip"),
            new VersionInfo("4.0.10.4", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.4/phpMyAdmin-4.0.10.4-all-languages.zip"),
            new VersionInfo("4.0.10.5", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.5/phpMyAdmin-4.0.10.5-all-languages.zip"),
            new VersionInfo("4.0.10.6", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.6/phpMyAdmin-4.0.10.6-all-languages.zip"),
            new VersionInfo("4.0.10.7", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.7/phpMyAdmin-4.0.10.7-all-languages.zip"),
            new VersionInfo("4.0.10.8", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.8/phpMyAdmin-4.0.10.8-all-languages.zip"),
            new VersionInfo("4.0.10.9", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.9/phpMyAdmin-4.0.10.9-all-languages.zip"),
            new VersionInfo("4.0.10.10", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.10/phpMyAdmin-4.0.10.10-all-languages.zip"),
            new VersionInfo("4.0.10.11", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.11/phpMyAdmin-4.0.10.11-all-languages.zip"),
            new VersionInfo("4.0.10.12", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.12/phpMyAdmin-4.0.10.12-all-languages.zip"),
            new VersionInfo("4.0.10.13", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.13/phpMyAdmin-4.0.10.13-all-languages.zip"),
            new VersionInfo("4.0.10.14", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.14/phpMyAdmin-4.0.10.14-all-languages.zip"),
            new VersionInfo("4.0.10.15", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.15/phpMyAdmin-4.0.10.15-all-languages.zip"),
            new VersionInfo("4.0.10.16", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.16/phpMyAdmin-4.0.10.16-all-languages.zip"),
            new VersionInfo("4.0.10.17", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.17/phpMyAdmin-4.0.10.17-all-languages.zip"),
            new VersionInfo("4.0.10.18", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.18/phpMyAdmin-4.0.10.18-all-languages.zip"),
            new VersionInfo("4.0.10.19", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.19/phpMyAdmin-4.0.10.19-all-languages.zip"),
            new VersionInfo("4.0.10.20", "https://files.phpmyadmin.net/phpMyAdmin/4.0.10.20/phpMyAdmin-4.0.10.20-all-languages.zip"),
            new VersionInfo("4.1.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.0/phpMyAdmin-4.1.0-all-languages.zip"),
            new VersionInfo("4.1.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.1/phpMyAdmin-4.1.1-all-languages.zip"),
            new VersionInfo("4.1.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.2/phpMyAdmin-4.1.2-all-languages.zip"),
            new VersionInfo("4.1.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.3/phpMyAdmin-4.1.3-all-languages.zip"),
            new VersionInfo("4.1.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.4/phpMyAdmin-4.1.4-all-languages.zip"),
            new VersionInfo("4.1.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.5/phpMyAdmin-4.1.5-all-languages.zip"),
            new VersionInfo("4.1.6.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.6/phpMyAdmin-4.1.6-all-languages.zip"),
            new VersionInfo("4.1.7.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.7/phpMyAdmin-4.1.7-all-languages.zip"),
            new VersionInfo("4.1.8.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.8/phpMyAdmin-4.1.8-all-languages.zip"),
            new VersionInfo("4.1.9.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.9/phpMyAdmin-4.1.9-all-languages.zip"),
            new VersionInfo("4.1.10.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.10/phpMyAdmin-4.1.10-all-languages.zip"),
            new VersionInfo("4.1.11.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.11/phpMyAdmin-4.1.11-all-languages.zip"),
            new VersionInfo("4.1.12.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.12/phpMyAdmin-4.1.12-all-languages.zip"),
            new VersionInfo("4.1.13.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.13/phpMyAdmin-4.1.13-all-languages.zip"),
            new VersionInfo("4.1.14.0", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14/phpMyAdmin-4.1.14-all-languages.zip"),
            new VersionInfo("4.1.14.1", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.1/phpMyAdmin-4.1.14.1-all-languages.zip"),
            new VersionInfo("4.1.14.2", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.2/phpMyAdmin-4.1.14.2-all-languages.zip"),
            new VersionInfo("4.1.14.3", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.3/phpMyAdmin-4.1.14.3-all-languages.zip"),
            new VersionInfo("4.1.14.4", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.4/phpMyAdmin-4.1.14.4-all-languages.zip"),
            new VersionInfo("4.1.14.5", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.5/phpMyAdmin-4.1.14.5-all-languages.zip"),
            new VersionInfo("4.1.14.6", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.6/phpMyAdmin-4.1.14.6-all-languages.zip"),
            new VersionInfo("4.1.14.7", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.7/phpMyAdmin-4.1.14.7-all-languages.zip"),
            new VersionInfo("4.1.14.8", "https://files.phpmyadmin.net/phpMyAdmin/4.1.14.8/phpMyAdmin-4.1.14.8-all-languages.zip"),
            new VersionInfo("4.2.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.0/phpMyAdmin-4.2.0-all-languages.zip"),
            new VersionInfo("4.2.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.1/phpMyAdmin-4.2.1-all-languages.zip"),
            new VersionInfo("4.2.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.2/phpMyAdmin-4.2.2-all-languages.zip"),
            new VersionInfo("4.2.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.3/phpMyAdmin-4.2.3-all-languages.zip"),
            new VersionInfo("4.2.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.4/phpMyAdmin-4.2.4-all-languages.zip"),
            new VersionInfo("4.2.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.5/phpMyAdmin-4.2.5-all-languages.zip"),
            new VersionInfo("4.2.6.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.6/phpMyAdmin-4.2.6-all-languages.zip"),
            new VersionInfo("4.2.7.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.7/phpMyAdmin-4.2.7-all-languages.zip"),
            new VersionInfo("4.2.7.1", "https://files.phpmyadmin.net/phpMyAdmin/4.2.7.1/phpMyAdmin-4.2.7.1-all-languages.zip"),
            new VersionInfo("4.2.8.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.8/phpMyAdmin-4.2.8-all-languages.zip"),
            new VersionInfo("4.2.8.1", "https://files.phpmyadmin.net/phpMyAdmin/4.2.8.1/phpMyAdmin-4.2.8.1-all-languages.zip"),
            new VersionInfo("4.2.9.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.9/phpMyAdmin-4.2.9-all-languages.zip"),
            new VersionInfo("4.2.9.1", "https://files.phpmyadmin.net/phpMyAdmin/4.2.9.1/phpMyAdmin-4.2.9.1-all-languages.zip"),
            new VersionInfo("4.2.10.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.10/phpMyAdmin-4.2.10-all-languages.zip"),
            new VersionInfo("4.2.10.1", "https://files.phpmyadmin.net/phpMyAdmin/4.2.10.1/phpMyAdmin-4.2.10.1-all-languages.zip"),
            new VersionInfo("4.2.11.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.11/phpMyAdmin-4.2.11-all-languages.zip"),
            new VersionInfo("4.2.12.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.12/phpMyAdmin-4.2.12-all-languages.zip"),
            new VersionInfo("4.2.13.0", "https://files.phpmyadmin.net/phpMyAdmin/4.2.13/phpMyAdmin-4.2.13-all-languages.zip"),
            new VersionInfo("4.2.13.1", "https://files.phpmyadmin.net/phpMyAdmin/4.2.13.1/phpMyAdmin-4.2.13.1-all-languages.zip"),
            new VersionInfo("4.2.13.2", "https://files.phpmyadmin.net/phpMyAdmin/4.2.13.2/phpMyAdmin-4.2.13.2-all-languages.zip"),
            new VersionInfo("4.2.13.3", "https://files.phpmyadmin.net/phpMyAdmin/4.2.13.3/phpMyAdmin-4.2.13.3-all-languages.zip"),
            new VersionInfo("4.3.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.0/phpMyAdmin-4.3.0-all-languages.zip"),
            new VersionInfo("4.3.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.1/phpMyAdmin-4.3.1-all-languages.zip"),
            new VersionInfo("4.3.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.2/phpMyAdmin-4.3.2-all-languages.zip"),
            new VersionInfo("4.3.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.3/phpMyAdmin-4.3.3-all-languages.zip"),
            new VersionInfo("4.3.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.4/phpMyAdmin-4.3.4-all-languages.zip"),
            new VersionInfo("4.3.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.5/phpMyAdmin-4.3.5-all-languages.zip"),
            new VersionInfo("4.3.6.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.6/phpMyAdmin-4.3.6-all-languages.zip"),
            new VersionInfo("4.3.7.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.7/phpMyAdmin-4.3.7-all-languages.zip"),
            new VersionInfo("4.3.8.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.8/phpMyAdmin-4.3.8-all-languages.zip"),
            new VersionInfo("4.3.9.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.9/phpMyAdmin-4.3.9-all-languages.zip"),
            new VersionInfo("4.3.10.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.10/phpMyAdmin-4.3.10-all-languages.zip"),
            new VersionInfo("4.3.11.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.11/phpMyAdmin-4.3.11-all-languages.zip"),
            new VersionInfo("4.3.11.1", "https://files.phpmyadmin.net/phpMyAdmin/4.3.11.1/phpMyAdmin-4.3.11.1-all-languages.zip"),
            new VersionInfo("4.3.12.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.12/phpMyAdmin-4.3.12-all-languages.zip"),
            new VersionInfo("4.3.13.0", "https://files.phpmyadmin.net/phpMyAdmin/4.3.13/phpMyAdmin-4.3.13-all-languages.zip"),
            new VersionInfo("4.3.13.1", "https://files.phpmyadmin.net/phpMyAdmin/4.3.13.1/phpMyAdmin-4.3.13.1-all-languages.zip"),
            new VersionInfo("4.3.13.2", "https://files.phpmyadmin.net/phpMyAdmin/4.3.13.2/phpMyAdmin-4.3.13.2-all-languages.zip"),
            new VersionInfo("4.3.13.3", "https://files.phpmyadmin.net/phpMyAdmin/4.3.13.3/phpMyAdmin-4.3.13.3-all-languages.zip"),
            new VersionInfo("4.4.15.0", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15/phpMyAdmin-4.4.15-all-languages.zip"),
            new VersionInfo("4.4.15.1", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.1/phpMyAdmin-4.4.15.1-all-languages.zip"),
            new VersionInfo("4.4.15.2", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.2/phpMyAdmin-4.4.15.2-all-languages.zip"),
            new VersionInfo("4.4.15.3", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.3/phpMyAdmin-4.4.15.3-all-languages.zip"),
            new VersionInfo("4.4.15.4", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.4/phpMyAdmin-4.4.15.4-all-languages.zip"),
            new VersionInfo("4.4.15.5", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.5/phpMyAdmin-4.4.15.5-all-languages.zip"),
            new VersionInfo("4.4.15.6", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.6/phpMyAdmin-4.4.15.6-all-languages.zip"),
            new VersionInfo("4.4.15.7", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.7/phpMyAdmin-4.4.15.7-all-languages.zip"),
            new VersionInfo("4.4.15.8", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.8/phpMyAdmin-4.4.15.8-all-languages.zip"),
            new VersionInfo("4.4.15.9", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.9/phpMyAdmin-4.4.15.9-all-languages.zip"),
            new VersionInfo("4.4.15.10", "https://files.phpmyadmin.net/phpMyAdmin/4.4.15.10/phpMyAdmin-4.4.15.10-all-languages.zip"),
            new VersionInfo("4.5.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.5.0/phpMyAdmin-4.5.0-all-languages.zip"),
            new VersionInfo("4.5.0.1", "https://files.phpmyadmin.net/phpMyAdmin/4.5.0.1/phpMyAdmin-4.5.0.1-all-languages.zip"),
            new VersionInfo("4.5.0.2", "https://files.phpmyadmin.net/phpMyAdmin/4.5.0.2/phpMyAdmin-4.5.0.2-all-languages.zip"),
            new VersionInfo("4.5.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.5.1/phpMyAdmin-4.5.1-all-languages.zip"),
            new VersionInfo("4.5.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.5.2/phpMyAdmin-4.5.2-all-languages.zip"),
            new VersionInfo("4.5.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.5.3/phpMyAdmin-4.5.3-all-languages.zip"),
            new VersionInfo("4.5.3.1", "https://files.phpmyadmin.net/phpMyAdmin/4.5.3.1/phpMyAdmin-4.5.3.1-all-languages.zip"),
            new VersionInfo("4.5.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.5.4/phpMyAdmin-4.5.4-all-languages.zip"),
            new VersionInfo("4.5.4.1", "https://files.phpmyadmin.net/phpMyAdmin/4.5.4.1/phpMyAdmin-4.5.4.1-all-languages.zip"),
            new VersionInfo("4.6.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.6.0/phpMyAdmin-4.6.0-all-languages.zip"),
            new VersionInfo("4.6.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.6.1/phpMyAdmin-4.6.1-all-languages.zip"),
            new VersionInfo("4.6.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.6.2/phpMyAdmin-4.6.2-all-languages.zip"),
            new VersionInfo("4.6.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.6.3/phpMyAdmin-4.6.3-all-languages.zip"),
            new VersionInfo("4.6.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.6.4/phpMyAdmin-4.6.4-all-languages.zip"),
            new VersionInfo("4.6.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.6.5/phpMyAdmin-4.6.5-all-languages.zip"),
            new VersionInfo("4.6.5.1", "https://files.phpmyadmin.net/phpMyAdmin/4.6.5.1/phpMyAdmin-4.6.5.1-all-languages.zip"),
            new VersionInfo("4.6.5.2", "https://files.phpmyadmin.net/phpMyAdmin/4.6.5.2/phpMyAdmin-4.6.5.2-all-languages.zip"),
            new VersionInfo("4.6.6.0", "https://files.phpmyadmin.net/phpMyAdmin/4.6.6/phpMyAdmin-4.6.6-all-languages.zip"),
            new VersionInfo("4.7.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.0/phpMyAdmin-4.7.0-all-languages.zip"),
            new VersionInfo("4.7.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.1/phpMyAdmin-4.7.1-all-languages.zip"),
            new VersionInfo("4.7.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.2/phpMyAdmin-4.7.2-all-languages.zip"),
            new VersionInfo("4.7.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.3/phpMyAdmin-4.7.3-all-languages.zip"),
            new VersionInfo("4.7.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.4/phpMyAdmin-4.7.4-all-languages.zip"),
            new VersionInfo("4.7.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.5/phpMyAdmin-4.7.5-all-languages.zip"),
            new VersionInfo("4.7.6.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.6/phpMyAdmin-4.7.6-all-languages.zip"),
            new VersionInfo("4.7.7.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.7/phpMyAdmin-4.7.7-all-languages.zip"),
            new VersionInfo("4.7.8.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.8/phpMyAdmin-4.7.8-all-languages.zip"),
            new VersionInfo("4.7.9.0", "https://files.phpmyadmin.net/phpMyAdmin/4.7.9/phpMyAdmin-4.7.9-all-languages.zip"),
            new VersionInfo("4.8.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.8.0/phpMyAdmin-4.8.0-all-languages.zip"),
            new VersionInfo("4.8.0.1", "https://files.phpmyadmin.net/phpMyAdmin/4.8.0.1/phpMyAdmin-4.8.0.1-all-languages.zip"),
            new VersionInfo("4.8.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.8.1/phpMyAdmin-4.8.1-all-languages.zip"),
            new VersionInfo("4.8.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.8.2/phpMyAdmin-4.8.2-all-languages.zip"),
            new VersionInfo("4.8.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.8.3/phpMyAdmin-4.8.3-all-languages.zip"),
            new VersionInfo("4.8.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.8.4/phpMyAdmin-4.8.4-all-languages.zip"),
            new VersionInfo("4.8.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.8.5/phpMyAdmin-4.8.5-all-languages.zip"),
            new VersionInfo("4.9.0.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.0/phpMyAdmin-4.9.0-all-languages.zip"),
            new VersionInfo("4.9.0.1", "https://files.phpmyadmin.net/phpMyAdmin/4.9.0.1/phpMyAdmin-4.9.0.1-all-languages.zip"),
            new VersionInfo("4.9.1.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.1/phpMyAdmin-4.9.1-all-languages.zip"),
            new VersionInfo("4.9.2.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.2/phpMyAdmin-4.9.2-all-languages.zip"),
            new VersionInfo("4.9.3.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.3/phpMyAdmin-4.9.3-all-languages.zip"),
            new VersionInfo("4.9.4.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.4/phpMyAdmin-4.9.4-all-languages.zip"),
            new VersionInfo("4.9.5.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.5/phpMyAdmin-4.9.5-all-languages.zip"),
            new VersionInfo("4.9.6.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.6/phpMyAdmin-4.9.6-all-languages.zip"),
            new VersionInfo("4.9.7.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.7/phpMyAdmin-4.9.7-all-languages.zip"),
            new VersionInfo("4.9.8.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.8/phpMyAdmin-4.9.8-all-languages.zip"),
            new VersionInfo("4.9.9.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.9/phpMyAdmin-4.9.9-all-languages.zip"),
            new VersionInfo("4.9.10.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.10/phpMyAdmin-4.9.10-all-languages.zip"),
            new VersionInfo("4.9.11.0", "https://files.phpmyadmin.net/phpMyAdmin/4.9.11/phpMyAdmin-4.9.11-all-languages.zip"),
            new VersionInfo("5.0.0.0", "https://files.phpmyadmin.net/phpMyAdmin/5.0.0/phpMyAdmin-5.0.0-all-languages.zip"),
            new VersionInfo("5.0.1.0", "https://files.phpmyadmin.net/phpMyAdmin/5.0.1/phpMyAdmin-5.0.1-all-languages.zip"),
            new VersionInfo("5.0.2.0", "https://files.phpmyadmin.net/phpMyAdmin/5.0.2/phpMyAdmin-5.0.2-all-languages.zip"),
            new VersionInfo("5.0.3.0", "https://files.phpmyadmin.net/phpMyAdmin/5.0.3/phpMyAdmin-5.0.3-all-languages.zip"),
            new VersionInfo("5.0.4.0", "https://files.phpmyadmin.net/phpMyAdmin/5.0.4/phpMyAdmin-5.0.4-all-languages.zip"),
            new VersionInfo("5.1.0.0", "https://files.phpmyadmin.net/phpMyAdmin/5.1.0/phpMyAdmin-5.1.0-all-languages.zip"),
            new VersionInfo("5.1.1.0", "https://files.phpmyadmin.net/phpMyAdmin/5.1.1/phpMyAdmin-5.1.1-all-languages.zip"),
            new VersionInfo("5.1.2.0", "https://files.phpmyadmin.net/phpMyAdmin/5.1.2/phpMyAdmin-5.1.2-all-languages.zip"),
            new VersionInfo("5.1.3.0", "https://files.phpmyadmin.net/phpMyAdmin/5.1.3/phpMyAdmin-5.1.3-all-languages.zip"),
            new VersionInfo("5.1.4.0", "https://files.phpmyadmin.net/phpMyAdmin/5.1.4/phpMyAdmin-5.1.4-all-languages.zip"),
            new VersionInfo("5.2.0.0", "https://files.phpmyadmin.net/phpMyAdmin/5.2.0/phpMyAdmin-5.2.0-all-languages.zip"),
            new VersionInfo("5.2.1.0", "https://files.phpmyadmin.net/phpMyAdmin/5.2.1/phpMyAdmin-5.2.1-all-languages.zip"),
            new VersionInfo("5.2.2.0", "https://files.phpmyadmin.net/phpMyAdmin/5.2.2/phpMyAdmin-5.2.2-all-languages.zip")
        };
        
        /// <summary>
        /// Gets the list of all available phpMyAdmin versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available phpMyAdmin version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific phpMyAdmin version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
