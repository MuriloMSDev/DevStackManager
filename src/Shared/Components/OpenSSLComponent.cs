using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// OpenSSL cryptography toolkit component manager for DevStack.
    /// Handles OpenSSL installation using a silent installer with custom installation directory.
    /// OpenSSL provides SSL/TLS protocols and general-purpose cryptography library.
    /// </summary>
    public class OpenSSLComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "openssl";
        
        /// <summary>
        /// Gets the display label for OpenSSL.
        /// </summary>
        public override string Label => "OpenSSL";
        
        /// <summary>
        /// Gets the OpenSSL installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.openSSLDir;
        
        /// <summary>
        /// Gets whether OpenSSL is distributed as an archive.
        /// Returns false because OpenSSL uses an executable installer.
        /// </summary>
        public override bool IsArchive => false;
        
        /// <summary>
        /// Gets whether OpenSSL requires running an installer executable.
        /// </summary>
        public override bool RunInstaller => true;

        /// <summary>
        /// Gets the command-line arguments for OpenSSL silent installation.
        /// </summary>
        /// <param name="version">OpenSSL version being installed.</param>
        /// <returns>Installer arguments for silent installation to versioned directory.</returns>
        public override string? GetInstallerArgs(string version)
        {
            string arch = "x64";
            string archPrefix = arch == "x86" ? "Win32OpenSSL" : "Win64OpenSSL";
            string subDir = $"openssl-{version}";
            string installDir = System.IO.Path.Combine(DevStackConfig.openSSLDir, subDir);
            return $"/VERYSILENT /DIR=\"{installDir}\"";
        }
    }
}
