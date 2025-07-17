using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class NodeComponent : ComponentBase
    {
        public override string Name => "node";

        public override async Task Install(string? version = null)
        {
            version ??= GetLatestVersion();
            string subDir = $"node-{version}";
            string zipUrl = GetUrlForVersion(version);
            await InstallGenericTool(DevStackConfig.nodeDir, version, zipUrl, subDir, "node.exe", "node", false);

            string nodePath = System.IO.Path.Combine(DevStackConfig.nodeDir, subDir);
            string binDir = System.IO.Path.Combine(DevStackConfig.nodeDir, "bin");
            if (!System.IO.Directory.Exists(binDir))
            {
                System.IO.Directory.CreateDirectory(binDir);
            }
            string srcExe = System.IO.Path.Combine(nodePath, "node.exe");
            string dstExe = System.IO.Path.Combine(binDir, $"node-{version}.exe");
            if (System.IO.File.Exists(srcExe))
            {
                System.IO.File.Copy(srcExe, dstExe, true);
                Console.WriteLine($"Atalho node-{version}.exe criado em {binDir}");
            }

            // Handle npm and npx renaming
            string npmPkgJson = System.IO.Path.Combine(nodePath, "node_modules", "npm", "package.json");
            if (System.IO.File.Exists(npmPkgJson))
            {
                var npmPackageContent = System.IO.File.ReadAllText(npmPkgJson);
                using var doc = System.Text.Json.JsonDocument.Parse(npmPackageContent);
                string? npmVersion = doc.RootElement.GetProperty("version").GetString();
                if (string.IsNullOrEmpty(npmVersion))
                {
                    Console.WriteLine("Não foi possível determinar a versão do npm no package.json.");
                }
                string[] npmFiles = { "npm", "npm.cmd", "npm.ps1" };
                string[] npxFiles = { "npx", "npx.cmd", "npx.ps1" };
                foreach (string npmFile in npmFiles)
                {
                    string fullPath = System.IO.Path.Combine(nodePath, npmFile);
                    if (System.IO.File.Exists(fullPath))
                    {
                        string ext = System.IO.Path.GetExtension(fullPath);
                        string newName = $"npm-{npmVersion}{ext}";
                        string newPath = System.IO.Path.Combine(nodePath, newName);
                        System.IO.File.Move(fullPath, newPath);
                        Console.WriteLine($"Renomeado {npmFile} para {newName}");
                    }
                }
                foreach (string npxFile in npxFiles)
                {
                    string fullPath = System.IO.Path.Combine(nodePath, npxFile);
                    if (System.IO.File.Exists(fullPath))
                    {
                        string ext = System.IO.Path.GetExtension(fullPath);
                        string newName = $"npx-{npmVersion}{ext}";
                        string newPath = System.IO.Path.Combine(nodePath, newName);
                        System.IO.File.Move(fullPath, newPath);
                        Console.WriteLine($"Renomeado {npxFile} para {newName}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Arquivo package.json do npm não encontrado.");
            }
        }
    }
}
