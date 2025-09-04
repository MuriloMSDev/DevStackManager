using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevStackManager
{
    public class PathManager
    {
        private readonly string baseDir;
        private readonly string phpDir;
        private readonly string nodeDir;
        private readonly string pythonDir;
        private readonly string nginxDir;
        private readonly string mysqlDir;

        public PathManager(string baseDir, string phpDir, string nodeDir, string pythonDir, string nginxDir, string mysqlDir)
        {
            this.baseDir = baseDir;
            this.phpDir = phpDir;
            this.nodeDir = nodeDir;
            this.pythonDir = pythonDir;
            this.nginxDir = nginxDir;
            this.mysqlDir = mysqlDir;
        }

        /// <summary>
        /// Normaliza um caminho, removendo espaços em branco e barras finais
        /// </summary>
        public static string? NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                return Path.GetFullPath(path).TrimEnd('\\');
            }
            catch
            {
                return path?.TrimEnd('\\');
            }
        }

        /// <summary>
        /// Adiciona o diretório bin global das ferramentas ao PATH do usuário
        /// </summary>
        public void AddBinDirsToPath()
        {
            // Adicionar apenas a pasta bin global do DevStack
            var globalBinDir = Path.Combine(baseDir, "bin");
            var pathsToAdd = new List<string>();

            if (Directory.Exists(globalBinDir))
            {
                pathsToAdd.Add(globalBinDir);
            }

            // Obter PATH atual do usuário
            var currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            var currentPathList = currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(NormalizePath)
                                            .Where(p => !string.IsNullOrEmpty(p))
                                            .ToList();

            // Filtrar apenas novos caminhos
            var newPaths = pathsToAdd.Select(NormalizePath)
                                    .Where(p => !string.IsNullOrEmpty(p) && !currentPathList.Contains(p))
                                    .ToList();

            if (newPaths.Count > 0)
            {
                var allPaths = currentPathList.Concat(newPaths);
                var newPathValue = string.Join(";", allPaths);
                
                // Atualizar PATH do usuário
                Environment.SetEnvironmentVariable("Path", newPathValue, EnvironmentVariableTarget.User);
                
                // Atualizar PATH da sessão atual (processo)
                UpdateProcessPath();

                DevStackConfig.WriteColoredLine("O diretório bin global foi adicionado ao PATH do usuário:", ConsoleColor.Green);
                foreach (var path in newPaths)
                {
                    DevStackConfig.WriteColoredLine($"  {path}", ConsoleColor.Yellow);
                }
                DevStackConfig.WriteColoredLine("O PATH do terminal atual também foi atualizado.", ConsoleColor.Green);
            }
            else
            {
                // Mesmo sem novos paths, garantir que o processo está sincronizado
                UpdateProcessPath();
                DevStackConfig.WriteColoredLine("O diretório bin global já está no PATH.", ConsoleColor.Yellow);
            }
        }

        /// <summary>
        /// Remove diretórios específicos do PATH do usuário
        /// </summary>
        public void RemoveFromPath(string[] dirsToRemove)
        {
            if (dirsToRemove == null || dirsToRemove.Length == 0)
                return;

            var currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            var currentPathList = currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(NormalizePath)
                                            .Where(p => !string.IsNullOrEmpty(p))
                                            .ToList();

            var dirsToRemoveNorm = dirsToRemove.Select(NormalizePath)
                                              .Where(p => !string.IsNullOrEmpty(p))
                                              .ToHashSet();

            var newPathList = currentPathList.Where(p => !dirsToRemoveNorm.Contains(p)).ToList();
            var newPathValue = string.Join(";", newPathList);

            // Atualizar PATH do usuário
            Environment.SetEnvironmentVariable("Path", newPathValue, EnvironmentVariableTarget.User);
            
            // Atualizar PATH da sessão atual
            UpdateProcessPath();

            var removedPaths = dirsToRemoveNorm.Where(dir => currentPathList.Contains(dir)).ToList();
            if (removedPaths.Count > 0)
            {
                DevStackConfig.WriteColoredLine("Os seguintes diretórios foram removidos do PATH do usuário:", ConsoleColor.Green);
                foreach (var path in removedPaths)
                {
                    DevStackConfig.WriteColoredLine($"  {path}", ConsoleColor.Yellow);
                }
            }
        }

        /// <summary>
        /// Remove todos os diretórios relacionados às ferramentas DevStack do PATH
        /// </summary>
        public void RemoveAllDevStackFromPath()
        {
            // Remover apenas a pasta bin global do DevStack
            var globalBinDir = Path.Combine(baseDir, "bin");
            var dirsToRemove = new List<string>();

            var currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            var currentPathList = currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(NormalizePath)
                                            .Where(p => !string.IsNullOrEmpty(p));

            if (Directory.Exists(globalBinDir) && currentPathList.Any(p => p == NormalizePath(globalBinDir)))
            {
                dirsToRemove.Add(globalBinDir);
            }

            if (dirsToRemove.Count > 0)
            {
                RemoveFromPath(dirsToRemove.ToArray());
            }
            else
            {
                DevStackConfig.WriteColoredLine("Nenhum diretório DevStack encontrado no PATH.", ConsoleColor.Yellow);
            }
        }

        /// <summary>
        /// Lista todos os diretórios atualmente no PATH do usuário
        /// </summary>
        public void ListCurrentPath()
        {
            var currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            var pathList = currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(NormalizePath)
                                     .Where(p => !string.IsNullOrEmpty(p))
                                     .ToList();

            var globalBinDir = NormalizePath(Path.Combine(baseDir, "bin"));
            var filteredPaths = pathList.Where(p => p != null && p.Equals(globalBinDir, StringComparison.OrdinalIgnoreCase)).ToList();

            DevStackConfig.WriteColoredLine("Diretórios do DevStack no PATH do usuário:", ConsoleColor.Cyan);
            if (filteredPaths.Count > 0)
            {
                foreach (var path in filteredPaths)
                {
                    var exists = Directory.Exists(path) ? "✓" : "✗";
                    var color = Directory.Exists(path) ? ConsoleColor.Green : ConsoleColor.Red;
                    DevStackConfig.WriteColoredLine($"  {exists} {path}", color);
                }
            }
            else
            {
                DevStackConfig.WriteColoredLine("  Nenhum diretório DevStack encontrado no PATH.", ConsoleColor.Yellow);
            }
        }

        /// <summary>
        /// Atualiza o PATH do processo atual combinando Machine PATH + User PATH
        /// </summary>
        private void UpdateProcessPath()
        {
            try
            {
                var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
                var machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) ?? "";
                
                // Combinar Machine PATH + User PATH (ordem padrão do Windows)
                var combinedPath = string.IsNullOrEmpty(machinePath) ? userPath : 
                                  string.IsNullOrEmpty(userPath) ? machinePath : 
                                  $"{machinePath};{userPath}";
                
                // Atualizar PATH do processo atual
                Environment.SetEnvironmentVariable("PATH", combinedPath);
                
                DevStackConfig.WriteLog($"PATH do processo atualizado. Total de entradas: {combinedPath.Split(';').Length}");
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteLog($"Erro ao atualizar PATH do processo: {ex.Message}");
            }
        }
    }
}
