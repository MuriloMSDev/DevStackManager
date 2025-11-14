using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Node.js runtime component manager for DevStack.
    /// Handles Node.js installation, NPM package manager integration,
    /// and creation of versioned shortcuts for node, npm, and npx commands.
    /// </summary>
    public class NodeComponent : ComponentBase
    {
        /// <summary>
        /// Relative path to NPM package.json file for version detection.
        /// </summary>
        private const string NPM_PACKAGE_JSON_RELATIVE_PATH = "node_modules/npm/package.json";
        
        /// <summary>
        /// Array of Node.js tool command files (npm, npx) to create shortcuts for.
        /// </summary>
        private static readonly string[] NodeToolFiles = { "npm.cmd", "npx.cmd" };

        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "node";
        
        /// <summary>
        /// Gets the display label for Node.js.
        /// </summary>
        public override string Label => "Node.js";
        
        /// <summary>
        /// Gets the Node.js installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.nodeDir;
        
        /// <summary>
        /// Gets whether Node.js is executable from command line.
        /// </summary>
        public override bool IsExecutable => true;
        
        /// <summary>
        /// Gets whether Node.js is a command-line tool.
        /// </summary>
        public override bool IsCommandLine => true;
        
        /// <summary>
        /// Gets the Node.js executable pattern.
        /// </summary>
        public override string? ExecutablePattern => "node.exe";
        
        /// <summary>
        /// Gets the shortcut name pattern for Node.js binaries.
        /// </summary>
        public override string? CreateBinShortcut => "node-{version}.exe";

        /// <summary>
        /// Executes post-installation configuration for Node.js.
        /// Creates versioned shortcuts for npm and npx commands.
        /// </summary>
        /// <param name="version">Node.js version being installed.</param>
        /// <param name="targetDir">Installation target directory.</param>
        public override Task PostInstall(string version, string targetDir)
        {
            var nodePath = Path.Combine(DevStackConfig.nodeDir, $"node-{version}");
            CreateNpmToolShortcuts(nodePath);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates shortcuts for npm tools (npm, npx) with version suffix.
        /// </summary>
        /// <param name="nodePath">Node.js installation directory.</param>
        private void CreateNpmToolShortcuts(string nodePath)
        {
            var npmVersion = GetNpmVersionFromPackageJson(nodePath);
            if (string.IsNullOrEmpty(npmVersion))
            {
                return;
            }

            CreateShortcutsForNodeTools(nodePath, npmVersion);
        }

        /// <summary>
        /// Extracts npm version from package.json file in Node.js installation.
        /// </summary>
        /// <param name="nodePath">Node.js installation directory.</param>
        /// <returns>NPM version string if found, null otherwise.</returns>
        private string? GetNpmVersionFromPackageJson(string nodePath)
        {
            var packageJsonPath = Path.Combine(nodePath, NPM_PACKAGE_JSON_RELATIVE_PATH);
            
            if (!File.Exists(packageJsonPath))
            {
                Console.WriteLine("npm package.json file not found.");
                return null;
            }

            try
            {
                return ExtractNpmVersionFromPackageJson(packageJsonPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing npm package.json: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses package.json and extracts npm version number.
        /// </summary>
        /// <param name="packageJsonPath">Path to package.json file.</param>
        /// <returns>NPM version string.</returns>
        private string? ExtractNpmVersionFromPackageJson(string packageJsonPath)
        {
            var packageContent = File.ReadAllText(packageJsonPath);
            using var doc = JsonDocument.Parse(packageContent);
            
            var version = doc.RootElement.GetProperty("version").GetString();
            
            if (string.IsNullOrEmpty(version))
            {
                Console.WriteLine("Unable to determine npm version from package.json.");
            }

            return version;
        }

        /// <summary>
        /// Creates shortcuts for all Node.js tools (npm, npx).
        /// </summary>
        /// <param name="nodePath">Node.js installation directory.</param>
        /// <param name="npmVersion">NPM version for shortcut naming.</param>
        private void CreateShortcutsForNodeTools(string nodePath, string npmVersion)
        {
            foreach (var toolFile in NodeToolFiles)
            {
                TryCreateToolShortcut(nodePath, toolFile, npmVersion);
            }
        }

        /// <summary>
        /// Attempts to create a shortcut for a specific Node.js tool.
        /// </summary>
        /// <param name="nodePath">Node.js installation directory.</param>
        /// <param name="toolFile">Tool filename (e.g., npm.cmd, npx.cmd).</param>
        /// <param name="npmVersion">NPM version for shortcut naming.</param>
        private void TryCreateToolShortcut(string nodePath, string toolFile, string npmVersion)
        {
            var sourcePath = Path.Combine(nodePath, toolFile);
            
            if (!File.Exists(sourcePath))
            {
                return;
            }

            try
            {
                var toolName = Path.GetFileNameWithoutExtension(toolFile);
                CreateGlobalBinShortcut(nodePath, toolFile, npmVersion, toolName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: failed to create shortcut for {toolFile}: {ex.Message}");
            }
        }
    }
}
