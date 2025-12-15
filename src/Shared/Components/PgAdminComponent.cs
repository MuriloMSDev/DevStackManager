using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// PgAdmin database management tool - child component of PostgreSQL.
    /// This component represents PgAdmin installations that come bundled with PostgreSQL.
    /// It is not independently installable but is detected from existing PostgreSQL installations.
    /// </summary>
    public class PgAdminComponent : ComponentBase
    {
        /// <summary>
        /// Relative path to PgAdmin directory within PostgreSQL installation.
        /// </summary>
        private const string PGADMIN_RELATIVE_PATH = "pgAdmin 4";

        /// <summary>
        /// Relative path to PgAdmin runtime directory containing the executable.
        /// </summary>
        private const string PGADMIN_RUNTIME_PATH = "runtime";

        /// <summary>
        /// PgAdmin executable filename.
        /// </summary>
        private const string PGADMIN_EXECUTABLE = "pgAdmin4.exe";

        /// <summary>
        /// Maps PgAdmin versions to their corresponding PostgreSQL versions.
        /// Key: PgAdmin version, Value: PostgreSQL version
        /// </summary>
        private readonly Dictionary<string, string> _pgadminToPgsqlVersionMap = new Dictionary<string, string>();

        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "pgadmin";
        
        /// <summary>
        /// Gets the display label for PgAdmin.
        /// </summary>
        public override string Label => "PgAdmin";
        
        /// <summary>
        /// Gets the PgAdmin installation base directory (same as PostgreSQL).
        /// </summary>
        public override string ToolDir => DevStackConfig.pgsqlDir;
        
        /// <summary>
        /// Gets whether PgAdmin is executable from command line.
        /// </summary>
        public override bool IsExecutable => true;
        
        /// <summary>
        /// Gets whether PgAdmin is a command-line tool.
        /// </summary>
        public override bool IsCommandLine => false;
        
        /// <summary>
        /// Gets the PgAdmin executable folder relative to the PostgreSQL installation.
        /// Path is: pgAdmin 4\runtime
        /// </summary>
        public override string? ExecutableFolder => Path.Combine(PGADMIN_RELATIVE_PATH, PGADMIN_RUNTIME_PATH);
        
        /// <summary>
        /// Gets the PgAdmin executable pattern.
        /// </summary>
        public override string? ExecutablePattern => PGADMIN_EXECUTABLE;

        /// <summary>
        /// Gets the parent component name for PgAdmin.
        /// PgAdmin is bundled with PostgreSQL.
        /// </summary>
        /// <returns>"pgsql" as PgAdmin's parent component.</returns>
        public override string? GetParentComponent() => "pgsql";

        /// <summary>
        /// Lists all installed versions of PgAdmin by scanning PostgreSQL installation directories.
        /// Each PostgreSQL installation may contain a bundled PgAdmin version.
        /// Returns PgAdmin versions for display, while maintaining a map to PostgreSQL versions for execution.
        /// If multiple PostgreSQL installations have the same PgAdmin version, only the one with the highest PostgreSQL version is shown.
        /// </summary>
        /// <returns>A sorted list of PgAdmin version strings extracted from installed executables.</returns>
        public override List<string> ListInstalled()
        {
            var pgsqlBaseDir = Path.Combine(DevStackConfig.baseDir, "pgsql");
            if (!Directory.Exists(pgsqlBaseDir))
            {
                return new List<string>();
            }

            _pgadminToPgsqlVersionMap.Clear();
            var pgsqlPrefix = "pgsql-";
            
            // Temporary map to track all PgAdmin versions found with their PostgreSQL versions
            var pgadminToPgsqlMap = new Dictionary<string, List<string>>();

            try
            {
                foreach (var pgsqlDir in Directory.GetDirectories(pgsqlBaseDir, $"{pgsqlPrefix}*"))
                {
                    var pgsqlDirName = Path.GetFileName(pgsqlDir);
                    if (pgsqlDirName.StartsWith(pgsqlPrefix))
                    {
                        var pgsqlVersion = pgsqlDirName.Substring(pgsqlPrefix.Length);
                        var pgadminExe = Path.Combine(pgsqlDir, PGADMIN_RELATIVE_PATH, PGADMIN_RUNTIME_PATH, PGADMIN_EXECUTABLE);
                        
                        if (File.Exists(pgadminExe))
                        {
                            var pgadminVersion = ExtractPgAdminVersion(pgadminExe);
                            if (!string.IsNullOrEmpty(pgadminVersion))
                            {
                                if (!pgadminToPgsqlMap.ContainsKey(pgadminVersion))
                                {
                                    pgadminToPgsqlMap[pgadminVersion] = new List<string>();
                                }
                                pgadminToPgsqlMap[pgadminVersion].Add(pgsqlVersion);
                            }
                        }
                    }
                }

                // For each PgAdmin version, select the highest PostgreSQL version
                var pgadminVersions = new List<string>();
                foreach (var kvp in pgadminToPgsqlMap)
                {
                    var pgadminVersion = kvp.Key;
                    var pgsqlVersions = kvp.Value;
                    
                    // Sort PostgreSQL versions and take the highest one
                    pgsqlVersions.Sort(CompareVersions);
                    var highestPgsqlVersion = pgsqlVersions[pgsqlVersions.Count - 1];
                    
                    // Map this PgAdmin version to the highest PostgreSQL version
                    _pgadminToPgsqlVersionMap[pgadminVersion] = highestPgsqlVersion;
                    pgadminVersions.Add(pgadminVersion);
                }

                pgadminVersions.Sort();
                return pgadminVersions;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Compares two version strings numerically.
        /// </summary>
        /// <param name="v1">First version string.</param>
        /// <param name="v2">Second version string.</param>
        /// <returns>-1 if v1 &lt; v2, 0 if equal, 1 if v1 &gt; v2.</returns>
        private int CompareVersions(string v1, string v2)
        {
            try
            {
                var parts1 = v1.Split('.').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();
                var parts2 = v2.Split('.').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();
                
                var maxLength = Math.Max(parts1.Length, parts2.Length);
                for (int i = 0; i < maxLength; i++)
                {
                    var p1 = i < parts1.Length ? parts1[i] : 0;
                    var p2 = i < parts2.Length ? parts2[i] : 0;
                    
                    if (p1 < p2) return -1;
                    if (p1 > p2) return 1;
                }
                
                return 0;
            }
            catch
            {
                return string.Compare(v1, v2, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Extracts the PgAdmin version from its executable file.
        /// </summary>
        /// <param name="executablePath">Path to PgAdmin executable.</param>
        /// <returns>PgAdmin version string, or empty if extraction fails.</returns>
        private string ExtractPgAdminVersion(string executablePath)
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(executablePath);
                var productVersion = versionInfo.ProductVersion;
                
                if (!string.IsNullOrEmpty(productVersion))
                {
                    // Remove any trailing characters after version number (e.g., "9.4''" -> "9.4")
                    productVersion = productVersion.Trim('\'', '"', ' ', '\0');
                    return productVersion;
                }
            }
            catch
            {
                // Version extraction failed
            }
            
            return string.Empty;
        }

        /// <summary>
        /// Gets the PostgreSQL version corresponding to a PgAdmin version.
        /// Used internally to resolve the correct installation path.
        /// </summary>
        /// <param name="pgadminVersion">PgAdmin version.</param>
        /// <returns>PostgreSQL version string, or the input version if mapping not found.</returns>
        public string GetPgsqlVersionForPgAdmin(string pgadminVersion)
        {
            return _pgadminToPgsqlVersionMap.TryGetValue(pgadminVersion, out var pgsqlVersion) 
                ? pgsqlVersion 
                : pgadminVersion;
        }

        /// <summary>
        /// PgAdmin cannot be installed independently - it is bundled with PostgreSQL.
        /// This method throws an exception to prevent direct installation.
        /// </summary>
        /// <param name="version">The version to install (not used).</param>
        /// <exception cref="InvalidOperationException">Always thrown as PgAdmin cannot be installed directly.</exception>
        public override Task Install(string? version = null)
        {
            throw new InvalidOperationException("PgAdmin cannot be installed independently. It is bundled with PostgreSQL installations.");
        }

        /// <summary>
        /// PgAdmin cannot be uninstalled independently - it is bundled with PostgreSQL.
        /// This method does nothing as PgAdmin is managed by PostgreSQL component.
        /// </summary>
        /// <param name="version">The version to uninstall (not used).</param>
        public new void Uninstall(string? version = null)
        {
            // PgAdmin is bundled with PostgreSQL and cannot be uninstalled separately
            Console.WriteLine("PgAdmin is bundled with PostgreSQL and cannot be uninstalled separately.");
        }

        /// <summary>
        /// PgAdmin versions are bundled with PostgreSQL and do not have separate availability listings.
        /// </summary>
        /// <returns>Empty list as PgAdmin has no independent version listings.</returns>
        public new List<string> ListAvailable()
        {
            return new List<string>();
        }
    }
}
