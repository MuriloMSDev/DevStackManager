using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class NodeComponent : ComponentBase
    {
        public override string Name => "node";
        public override string ToolDir => DevStackConfig.nodeDir;
        public override bool IsExecutable => true;
        public override bool IsCommandLine => true;
        public override string? ExecutablePattern => "node.exe";
        public override string? CreateBinShortcut => "node-{version}.exe";

        public override Task PostInstall(string version, string targetDir)
        {
            string nodePath = System.IO.Path.Combine(DevStackConfig.nodeDir, $"node-{version}");

            // Create shortcuts for npm and npx in global bin (não renomear arquivos)
            string npmPkgJson = System.IO.Path.Combine(nodePath, "node_modules", "npm", "package.json");
            if (System.IO.File.Exists(npmPkgJson))
            {
                try
                {
                    var npmPackageContent = System.IO.File.ReadAllText(npmPkgJson);
                    using var doc = System.Text.Json.JsonDocument.Parse(npmPackageContent);
                    string? npmVersion = doc.RootElement.GetProperty("version").GetString();
                    
                    if (!string.IsNullOrEmpty(npmVersion))
                    {
                        // Criar atalhos para npm e npx no bin global
                        string[] toolFiles = { "npm.cmd", "npx.cmd" };
                        
                        foreach (string toolFile in toolFiles)
                        {
                            string sourcePath = System.IO.Path.Combine(nodePath, toolFile);
                            if (System.IO.File.Exists(sourcePath))
                            {
                                try
                                {
                                    string toolName = System.IO.Path.GetFileNameWithoutExtension(toolFile);
                                    CreateGlobalBinShortcut(nodePath, toolFile, npmVersion, toolName);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Aviso: falha ao criar atalho para {toolFile}: {e.Message}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Não foi possível determinar a versão do npm no package.json.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Erro ao processar package.json do npm: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("Arquivo package.json do npm não encontrado.");
            }
            return Task.CompletedTask;
        }
    }
}
