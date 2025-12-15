using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// NPM (Node Package Manager) component - child component of Node.js.
    /// This component represents NPM installations that come bundled with Node.js.
    /// It is not independently installable but is detected from existing Node.js installations.
    /// </summary>
    public class NpmComponent : ComponentBase
    {
        /// <summary>
        /// Relative path to NPM package.json file for version detection.
        /// </summary>
        private const string NPM_PACKAGE_JSON_RELATIVE_PATH = "node_modules/npm/package.json";

        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "npm";
        
        /// <summary>
        /// Gets the display label for NPM.
        /// </summary>
        public override string Label => "NPM";
        
        /// <summary>
        /// Gets the NPM installation base directory (same as Node.js).
        /// </summary>
        public override string ToolDir => DevStackConfig.nodeDir;
        
        /// <summary>
        /// Gets the shortcut name pattern for NPM binaries.
        /// </summary>
        public override string? CreateBinShortcut => "npm-{version}.cmd";

        /// <summary>
        /// Gets the parent component name for NPM.
        /// NPM is bundled with Node.js.
        /// </summary>
        /// <returns>"node" as NPM's parent component.</returns>
        public override string? GetParentComponent() => "node";

        /// <summary>
        /// Lists all installed versions of NPM by scanning Node.js installation directories.
        /// Each Node.js installation contains a bundled NPM version.
        /// </summary>
        /// <returns>A sorted list of installed NPM version strings.</returns>
        public override List<string> ListInstalled()
        {
            var nodeBaseDir = Path.Combine(DevStackConfig.baseDir, "node");
            if (!Directory.Exists(nodeBaseDir))
            {
                return new List<string>();
            }

            var npmVersions = new List<string>();
            var nodePrefix = "node-";

            try
            {
                foreach (var nodeDir in Directory.GetDirectories(nodeBaseDir, $"{nodePrefix}*"))
                {
                    var npmVersion = ExtractNpmVersionFromNodeInstallation(nodeDir);
                    if (!string.IsNullOrEmpty(npmVersion))
                    {
                        npmVersions.Add(npmVersion);
                    }
                }

                npmVersions.Sort();
                return npmVersions;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Extracts NPM version from a Node.js installation directory.
        /// </summary>
        /// <param name="nodeDir">Node.js installation directory path.</param>
        /// <returns>NPM version string if found, null otherwise.</returns>
        private string? ExtractNpmVersionFromNodeInstallation(string nodeDir)
        {
            var packageJsonPath = Path.Combine(nodeDir, NPM_PACKAGE_JSON_RELATIVE_PATH);
            
            if (!File.Exists(packageJsonPath))
            {
                return null;
            }

            try
            {
                var packageContent = File.ReadAllText(packageJsonPath);
                using var doc = JsonDocument.Parse(packageContent);
                return doc.RootElement.GetProperty("version").GetString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// NPM cannot be installed independently - it is bundled with Node.js.
        /// This method throws an exception to prevent direct installation.
        /// </summary>
        /// <param name="version">The version to install (not used).</param>
        /// <exception cref="InvalidOperationException">Always thrown as NPM cannot be installed directly.</exception>
        public override Task Install(string? version = null)
        {
            throw new InvalidOperationException("NPM cannot be installed independently. It is bundled with Node.js installations.");
        }

        /// <summary>
        /// NPM cannot be uninstalled independently - it is bundled with Node.js.
        /// This method does nothing as NPM is managed by Node.js component.
        /// </summary>
        /// <param name="version">The version to uninstall (not used).</param>
        public new void Uninstall(string? version = null)
        {
            // NPM is bundled with Node.js and cannot be uninstalled separately
            Console.WriteLine("NPM is bundled with Node.js and cannot be uninstalled separately.");
        }

        /// <summary>
        /// NPM versions are bundled with Node.js and do not have separate availability listings.
        /// </summary>
        /// <returns>Empty list as NPM has no independent version listings.</returns>
        public new List<string> ListAvailable()
        {
            return new List<string>();
        }
    }
}
