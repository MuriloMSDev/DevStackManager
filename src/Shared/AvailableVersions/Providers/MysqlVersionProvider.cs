using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Provedor de versões disponíveis para MySQL
    /// </summary>
    public class MysqlVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets the display name of the MySQL component.
        /// </summary>
        public string ComponentName => "MySQL";
        /// <summary>
        /// Gets the unique identifier for the MySQL component.
        /// </summary>
        public string ComponentId => "mysql";
        
        /// <summary>
        /// List of available MySQL versions with download URLs.
        /// </summary>
        private static readonly List<VersionInfo> _versions = new List<VersionInfo>
        {
            new VersionInfo("5.0.19", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.19-winx64.zip"),
            new VersionInfo("5.0.21", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.21-winx64.zip"),
            new VersionInfo("5.0.24", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.24-winx64.zip"),
            new VersionInfo("5.0.37", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.37-winx64.zip"),
            new VersionInfo("5.0.41", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.41-winx64.zip"),
            new VersionInfo("5.0.45", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.45-winx64.zip"),
            new VersionInfo("5.0.51a", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.51a-winx64.zip"),
            new VersionInfo("5.0.51b", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.51b-winx64.zip"),
            new VersionInfo("5.0.67", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.67-winx64.zip"),
            new VersionInfo("5.0.77", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.77-winx64.zip"),
            new VersionInfo("5.0.81", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.81-winx64.zip"),
            new VersionInfo("5.0.82", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.82-winx64.zip"),
            new VersionInfo("5.0.83", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.83-winx64.zip"),
            new VersionInfo("5.0.84", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.84-winx64.zip"),
            new VersionInfo("5.0.85", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.85-winx64.zip"),
            new VersionInfo("5.0.86", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.86-winx64.zip"),
            new VersionInfo("5.0.87", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.87-winx64.zip"),
            new VersionInfo("5.0.88", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.88-winx64.zip"),
            new VersionInfo("5.0.89", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.89-winx64.zip"),
            new VersionInfo("5.0.90", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.90-winx64.zip"),
            new VersionInfo("5.0.91", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.91-winx64.zip"),
            new VersionInfo("5.0.95", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.95-winx64.zip"),
            new VersionInfo("5.0.96", "https://dev.mysql.com/get/Downloads/MySQL-5.0/mysql-5.0.96-winx64.zip"),
            new VersionInfo("5.1.30", "https://dev.mysql.com/get/Downloads/MySQL-5.1/mysql-5.1.30-winx64.zip"),
            new VersionInfo("5.5.8", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.8-winx64.zip"),
            new VersionInfo("5.5.9", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.9-winx64.zip"),
            new VersionInfo("5.5.10", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.10-winx64.zip"),
            new VersionInfo("5.5.11", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.11-winx64.zip"),
            new VersionInfo("5.5.12", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.12-winx64.zip"),
            new VersionInfo("5.5.13", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.13-winx64.zip"),
            new VersionInfo("5.5.14", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.14-winx64.zip"),
            new VersionInfo("5.5.15", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.15-winx64.zip"),
            new VersionInfo("5.5.16", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.16-winx64.zip"),
            new VersionInfo("5.5.17", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.17-winx64.zip"),
            new VersionInfo("5.5.18", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.18-winx64.zip"),
            new VersionInfo("5.5.19", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.19-winx64.zip"),
            new VersionInfo("5.5.20", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.20-winx64.zip"),
            new VersionInfo("5.5.21", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.21-winx64.zip"),
            new VersionInfo("5.5.22", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.22-winx64.zip"),
            new VersionInfo("5.5.23", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.23-winx64.zip"),
            new VersionInfo("5.5.24", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.24-winx64.zip"),
            new VersionInfo("5.5.25a", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.25a-winx64.zip"),
            new VersionInfo("5.5.27", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.27-winx64.zip"),
            new VersionInfo("5.5.28", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.28-winx64.zip"),
            new VersionInfo("5.5.29", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.29-winx64.zip"),
            new VersionInfo("5.5.30", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.30-winx64.zip"),
            new VersionInfo("5.5.31", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.31-winx64.zip"),
            new VersionInfo("5.5.32", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.32-winx64.zip"),
            new VersionInfo("5.5.33", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.33-winx64.zip"),
            new VersionInfo("5.5.34", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.34-winx64.zip"),
            new VersionInfo("5.5.35", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.35-winx64.zip"),
            new VersionInfo("5.5.36", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.36-winx64.zip"),
            new VersionInfo("5.5.37", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.37-winx64.zip"),
            new VersionInfo("5.5.38", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.38-winx64.zip"),
            new VersionInfo("5.5.39", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.39-winx64.zip"),
            new VersionInfo("5.5.40", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.40-winx64.zip"),
            new VersionInfo("5.5.41", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.41-winx64.zip"),
            new VersionInfo("5.5.42", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.42-winx64.zip"),
            new VersionInfo("5.5.43", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.43-winx64.zip"),
            new VersionInfo("5.5.44", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.44-winx64.zip"),
            new VersionInfo("5.5.45", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.45-winx64.zip"),
            new VersionInfo("5.5.46", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.46-winx64.zip"),
            new VersionInfo("5.5.47", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.47-winx64.zip"),
            new VersionInfo("5.5.48", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.48-winx64.zip"),
            new VersionInfo("5.5.49", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.49-winx64.zip"),
            new VersionInfo("5.5.50", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.50-winx64.zip"),
            new VersionInfo("5.5.51", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.51-winx64.zip"),
            new VersionInfo("5.5.52", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.52-winx64.zip"),
            new VersionInfo("5.5.53", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.53-winx64.zip"),
            new VersionInfo("5.5.54", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.54-winx64.zip"),
            new VersionInfo("5.5.55", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.55-winx64.zip"),
            new VersionInfo("5.5.56", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.56-winx64.zip"),
            new VersionInfo("5.5.57", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.57-winx64.zip"),
            new VersionInfo("5.5.58", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.58-winx64.zip"),
            new VersionInfo("5.5.59", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.59-winx64.zip"),
            new VersionInfo("5.5.60", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.60-winx64.zip"),
            new VersionInfo("5.5.61", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.61-winx64.zip"),
            new VersionInfo("5.5.62", "https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.62-winx64.zip"),
            new VersionInfo("5.6.10", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.10-winx64.zip"),
            new VersionInfo("5.6.11", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.11-winx64.zip"),
            new VersionInfo("5.6.12", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.12-winx64.zip"),
            new VersionInfo("5.6.13", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.13-winx64.zip"),
            new VersionInfo("5.6.14", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.14-winx64.zip"),
            new VersionInfo("5.6.15", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.15-winx64.zip"),
            new VersionInfo("5.6.16", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.16-winx64.zip"),
            new VersionInfo("5.6.17", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.17-winx64.zip"),
            new VersionInfo("5.6.19", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.19-winx64.zip"),
            new VersionInfo("5.6.20", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.20-winx64.zip"),
            new VersionInfo("5.6.21", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.21-winx64.zip"),
            new VersionInfo("5.6.22", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.22-winx64.zip"),
            new VersionInfo("5.6.23", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.23-winx64.zip"),
            new VersionInfo("5.6.24", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.24-winx64.zip"),
            new VersionInfo("5.6.25", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.25-winx64.zip"),
            new VersionInfo("5.6.26", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.26-winx64.zip"),
            new VersionInfo("5.6.27", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.27-winx64.zip"),
            new VersionInfo("5.6.28", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.28-winx64.zip"),
            new VersionInfo("5.6.29", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.29-winx64.zip"),
            new VersionInfo("5.6.30", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.30-winx64.zip"),
            new VersionInfo("5.6.31", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.31-winx64.zip"),
            new VersionInfo("5.6.32", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.32-winx64.zip"),
            new VersionInfo("5.6.33", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.33-winx64.zip"),
            new VersionInfo("5.6.34", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.34-winx64.zip"),
            new VersionInfo("5.6.35", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.35-winx64.zip"),
            new VersionInfo("5.6.36", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.36-winx64.zip"),
            new VersionInfo("5.6.37", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.37-winx64.zip"),
            new VersionInfo("5.6.38", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.38-winx64.zip"),
            new VersionInfo("5.6.39", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.39-winx64.zip"),
            new VersionInfo("5.6.40", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.40-winx64.zip"),
            new VersionInfo("5.6.41", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.41-winx64.zip"),
            new VersionInfo("5.6.42", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.42-winx64.zip"),
            new VersionInfo("5.6.43", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.43-winx64.zip"),
            new VersionInfo("5.6.44", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.44-winx64.zip"),
            new VersionInfo("5.6.45", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.45-winx64.zip"),
            new VersionInfo("5.6.46", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.46-winx64.zip"),
            new VersionInfo("5.6.47", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.47-winx64.zip"),
            new VersionInfo("5.6.48", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.48-winx64.zip"),
            new VersionInfo("5.6.49", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.49-winx64.zip"),
            new VersionInfo("5.6.50", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.50-winx64.zip"),
            new VersionInfo("5.6.51", "https://dev.mysql.com/get/Downloads/MySQL-5.6/mysql-5.6.51-winx64.zip"),
            new VersionInfo("5.7.9", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.9-winx64.zip"),
            new VersionInfo("5.7.10", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.10-winx64.zip"),
            new VersionInfo("5.7.11", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.11-winx64.zip"),
            new VersionInfo("5.7.12", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.12-winx64.zip"),
            new VersionInfo("5.7.13", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.13-winx64.zip"),
            new VersionInfo("5.7.14", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.14-winx64.zip"),
            new VersionInfo("5.7.15", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.15-winx64.zip"),
            new VersionInfo("5.7.16", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.16-winx64.zip"),
            new VersionInfo("5.7.17", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.17-winx64.zip"),
            new VersionInfo("5.7.18", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.18-winx64.zip"),
            new VersionInfo("5.7.19", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.19-winx64.zip"),
            new VersionInfo("5.7.20", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.20-winx64.zip"),
            new VersionInfo("5.7.21", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.21-winx64.zip"),
            new VersionInfo("5.7.22", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.22-winx64.zip"),
            new VersionInfo("5.7.23", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.23-winx64.zip"),
            new VersionInfo("5.7.24", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.24-winx64.zip"),
            new VersionInfo("5.7.25", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.25-winx64.zip"),
            new VersionInfo("5.7.26", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.26-winx64.zip"),
            new VersionInfo("5.7.27", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.27-winx64.zip"),
            new VersionInfo("5.7.28", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.28-winx64.zip"),
            new VersionInfo("5.7.29", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.29-winx64.zip"),
            new VersionInfo("5.7.30", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.30-winx64.zip"),
            new VersionInfo("5.7.31", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.31-winx64.zip"),
            new VersionInfo("5.7.32", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.32-winx64.zip"),
            new VersionInfo("5.7.33", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.33-winx64.zip"),
            new VersionInfo("5.7.34", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.34-winx64.zip"),
            new VersionInfo("5.7.35", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.35-winx64.zip"),
            new VersionInfo("5.7.36", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.36-winx64.zip"),
            new VersionInfo("5.7.37", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.37-winx64.zip"),
            new VersionInfo("5.7.38", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.38-winx64.zip"),
            new VersionInfo("5.7.39", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.39-winx64.zip"),
            new VersionInfo("5.7.40", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.40-winx64.zip"),
            new VersionInfo("5.7.41", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.41-winx64.zip"),
            new VersionInfo("5.7.42", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.42-winx64.zip"),
            new VersionInfo("5.7.43", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.43-winx64.zip"),
            new VersionInfo("5.7.44", "https://dev.mysql.com/get/Downloads/MySQL-5.7/mysql-5.7.44-winx64.zip"),
            new VersionInfo("8.0.11", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.11-winx64.zip"),
            new VersionInfo("8.0.12", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.12-winx64.zip"),
            new VersionInfo("8.0.13", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.13-winx64.zip"),
            new VersionInfo("8.0.14", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.14-winx64.zip"),
            new VersionInfo("8.0.15", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.15-winx64.zip"),
            new VersionInfo("8.0.16", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.16-winx64.zip"),
            new VersionInfo("8.0.17", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.17-winx64.zip"),
            new VersionInfo("8.0.18", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.18-winx64.zip"),
            new VersionInfo("8.0.19", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.19-winx64.zip"),
            new VersionInfo("8.0.20", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.20-winx64.zip"),
            new VersionInfo("8.0.21", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.21-winx64.zip"),
            new VersionInfo("8.0.22", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.22-winx64.zip"),
            new VersionInfo("8.0.23", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.23-winx64.zip"),
            new VersionInfo("8.0.24", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.24-winx64.zip"),
            new VersionInfo("8.0.25", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.25-winx64.zip"),
            new VersionInfo("8.0.26", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.26-winx64.zip"),
            new VersionInfo("8.0.27", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.27-winx64.zip"),
            new VersionInfo("8.0.28", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.28-winx64.zip"),
            new VersionInfo("8.0.30", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.30-winx64.zip"),
            new VersionInfo("8.0.31", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.31-winx64.zip"),
            new VersionInfo("8.0.32", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.32-winx64.zip"),
            new VersionInfo("8.0.33", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.33-winx64.zip"),
            new VersionInfo("8.0.34", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.34-winx64.zip"),
            new VersionInfo("8.0.35", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.35-winx64.zip"),
            new VersionInfo("8.0.36", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.36-winx64.zip"),
            new VersionInfo("8.0.37", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.37-winx64.zip"),
            new VersionInfo("8.0.39", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.39-winx64.zip"),
            new VersionInfo("8.0.40", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.40-winx64.zip"),
            new VersionInfo("8.0.41", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.41-winx64.zip"),
            new VersionInfo("8.1.0", "https://dev.mysql.com/get/Downloads/MySQL-8.1/mysql-8.1.0-winx64.zip"),
            new VersionInfo("8.2.0", "https://dev.mysql.com/get/Downloads/MySQL-8.2/mysql-8.2.0-winx64.zip"),
            new VersionInfo("8.3.0", "https://dev.mysql.com/get/Downloads/MySQL-8.3/mysql-8.3.0-winx64.zip"),
            new VersionInfo("8.4.0", "https://dev.mysql.com/get/Downloads/MySQL-8.4/mysql-8.4.0-winx64.zip"),
            new VersionInfo("8.4.2", "https://dev.mysql.com/get/Downloads/MySQL-8.4/mysql-8.4.2-winx64.zip"),
            new VersionInfo("8.4.3", "https://dev.mysql.com/get/Downloads/MySQL-8.4/mysql-8.4.3-winx64.zip"),
            new VersionInfo("8.4.4", "https://dev.mysql.com/get/Downloads/MySQL-8.4/mysql-8.4.4-winx64.zip"),
            new VersionInfo("9.0.1", "https://dev.mysql.com/get/Downloads/MySQL-9.0/mysql-9.0.1-winx64.zip"),
            new VersionInfo("9.1.0", "https://dev.mysql.com/get/Downloads/MySQL-9.1/mysql-9.1.0-winx64.zip"),
            new VersionInfo("9.2.0", "https://dev.mysql.com/get/Downloads/MySQL-9.2/mysql-9.2.0-winx64.zip"),
            new VersionInfo("8.0.42", "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-8.0.42-winx64.zip"),
            new VersionInfo("8.4.5", "https://dev.mysql.com/get/Downloads/MySQL-8.4/mysql-8.4.5-winx64.zip"),
            new VersionInfo("9.3.0", "https://dev.mysql.com/get/Downloads/MySQL-9.3/mysql-9.3.0-winx64.zip")
        };
        
        /// <summary>
        /// Gets the list of all available MySQL versions.
        /// </summary>
        /// <returns>List of available version information.</returns>
        public List<VersionInfo> GetAvailableVersions()
        {
            return new List<VersionInfo>(_versions);
        }
        
        /// <summary>
        /// Gets the latest available MySQL version.
        /// </summary>
        /// <returns>Latest version information or null if no versions available.</returns>
        public VersionInfo? GetLatestVersion()
        {
            return _versions.LastOrDefault();
        }
        
        /// <summary>
        /// Gets a specific MySQL version by version string.
        /// </summary>
        /// <param name="version">The version string to find.</param>
        /// <returns>Version information or null if not found.</returns>
        public VersionInfo? GetVersion(string version)
        {
            return _versions.FirstOrDefault(v => v.Version == version);
        }
    }
}
