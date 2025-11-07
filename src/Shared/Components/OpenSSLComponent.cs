using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class OpenSSLComponent : ComponentBase
    {
        public override string Name => "openssl";
        public override string Label => "OpenSSL";
        public override string ToolDir => DevStackConfig.openSSLDir;
        public override bool IsArchive => false;
        public override bool RunInstaller => true;

        public override string? GetInstallerArgs(string version)
        {
            string arch = "x64";
            string archPrefix = arch == "x86" ? "Win32OpenSSL" : "Win64OpenSSL";
            string subDir = $"openssl-{version}";
            string installDir = System.IO.Path.Combine(DevStackConfig.openSSLDir, subDir);
            // Return installer args; InstallGenericTool will download and execute the installer
            return $"/VERYSILENT /DIR=\"{installDir}\"";
        }
    }
}
