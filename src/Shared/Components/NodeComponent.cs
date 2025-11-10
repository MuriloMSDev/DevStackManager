using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class NodeComponent : ComponentBase
    {
        private const string NPM_PACKAGE_JSON_RELATIVE_PATH = "node_modules/npm/package.json";
        private static readonly string[] NodeToolFiles = { "npm.cmd", "npx.cmd" };

        public override string Name => "node";
        public override string Label => "Node.js";
        public override string ToolDir => DevStackConfig.nodeDir;
        public override bool IsExecutable => true;
        public override bool IsCommandLine => true;
        public override string? ExecutablePattern => "node.exe";
        public override string? CreateBinShortcut => "node-{version}.exe";

        public override Task PostInstall(string version, string targetDir)
        {
            var nodePath = Path.Combine(DevStackConfig.nodeDir, $"node-{version}");
            CreateNpmToolShortcuts(nodePath);
            return Task.CompletedTask;
        }

        private void CreateNpmToolShortcuts(string nodePath)
        {
            var npmVersion = GetNpmVersionFromPackageJson(nodePath);
            if (string.IsNullOrEmpty(npmVersion))
            {
                return;
            }

            CreateShortcutsForNodeTools(nodePath, npmVersion);
        }

        private string? GetNpmVersionFromPackageJson(string nodePath)
        {
            var packageJsonPath = Path.Combine(nodePath, NPM_PACKAGE_JSON_RELATIVE_PATH);
            
            if (!File.Exists(packageJsonPath))
            {
                Console.WriteLine("Arquivo package.json do npm não encontrado.");
                return null;
            }

            try
            {
                return ExtractNpmVersionFromPackageJson(packageJsonPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar package.json do npm: {ex.Message}");
                return null;
            }
        }

        private string? ExtractNpmVersionFromPackageJson(string packageJsonPath)
        {
            var packageContent = File.ReadAllText(packageJsonPath);
            using var doc = JsonDocument.Parse(packageContent);
            
            var version = doc.RootElement.GetProperty("version").GetString();
            
            if (string.IsNullOrEmpty(version))
            {
                Console.WriteLine("Não foi possível determinar a versão do npm no package.json.");
            }

            return version;
        }

        private void CreateShortcutsForNodeTools(string nodePath, string npmVersion)
        {
            foreach (var toolFile in NodeToolFiles)
            {
                TryCreateToolShortcut(nodePath, toolFile, npmVersion);
            }
        }

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
                Console.WriteLine($"Aviso: falha ao criar atalho para {toolFile}: {ex.Message}");
            }
        }
    }
}
