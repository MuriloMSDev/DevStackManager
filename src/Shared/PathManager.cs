using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevStackManager
{
    public class PathManager
    {
        private readonly string _baseDir;
        
        // Directory Constants
        private const string BIN_DIRECTORY_NAME = "bin";
        private const string PATH_ENVIRONMENT_VARIABLE = "Path";
        private const string PATH_ENVIRONMENT_VARIABLE_UPPERCASE = "PATH";
        
        // Path Separator
        private const char PATH_SEPARATOR = ';';
        
        // Status Symbols
        private const string STATUS_CHECK_MARK = "✓";
        private const string STATUS_CROSS_MARK = "✗";
        
        // Indentation
        private const string INDENT = "  ";

        public PathManager(
            string baseDir, 
            string phpDir, 
            string nodeDir, 
            string pythonDir, 
            string nginxDir, 
            string mysqlDir)
        {
            _baseDir = baseDir;
        }

        public static string? NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return Path.GetFullPath(path).TrimEnd('\\');
            }
            catch
            {
                return path?.TrimEnd('\\');
            }
        }

        public void AddBinDirsToPath()
        {
            var globalBinDir = Path.Combine(_baseDir, BIN_DIRECTORY_NAME);
            
            if (!Directory.Exists(globalBinDir))
            {
                LogBinDirectoryNotExists();
                return;
            }

            var currentPathList = GetCurrentUserPathList();
            var normalizedBinDir = NormalizePath(globalBinDir);

            if (currentPathList.Contains(normalizedBinDir))
            {
                UpdateProcessPath();
                LogBinAlreadyInPath();
                return;
            }

            AddPathToUser(normalizedBinDir!, currentPathList);
            UpdateProcessPath();

            LogBinAddedToPath(normalizedBinDir!);
        }

        public void RemoveFromPath(string[] dirsToRemove)
        {
            if (dirsToRemove == null || dirsToRemove.Length == 0)
            {
                return;
            }

            var currentPathList = GetCurrentUserPathList();
            var normalizedDirsToRemove = dirsToRemove
                .Select(NormalizePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet();

            var newPathList = currentPathList
                .Where(p => !normalizedDirsToRemove.Contains(p))
                .ToList();

            UpdateUserPath(newPathList);
            UpdateProcessPath();

            var removedPaths = normalizedDirsToRemove
                .Where(dir => currentPathList.Contains(dir))
                .ToList();

            if (removedPaths.Any())
            {
                LogPathsRemoved(removedPaths);
            }
        }

        public void RemoveAllDevStackFromPath()
        {
            var globalBinDir = Path.Combine(_baseDir, BIN_DIRECTORY_NAME);
            var normalizedBinDir = NormalizePath(globalBinDir);

            if (!Directory.Exists(globalBinDir))
            {
                LogNoDevStackInPath();
                return;
            }

            var currentPathList = GetCurrentUserPathList();
            
            if (!currentPathList.Contains(normalizedBinDir))
            {
                LogNoDevStackInPath();
                return;
            }

            RemoveFromPath(new[] { globalBinDir });
        }

        public void ListCurrentPath()
        {
            var globalBinDir = NormalizePath(Path.Combine(_baseDir, BIN_DIRECTORY_NAME));
            var currentPathList = GetCurrentUserPathList();
            var devStackPaths = currentPathList
                .Where(p => p?.Equals(globalBinDir, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            LogDevStackPathsHeader();
            
            if (!devStackPaths.Any())
            {
                LogNoDevStackInPath();
                return;
            }

            foreach (var path in devStackPaths)
            {
                LogPathWithStatus(path!);
            }
        }

        private void LogBinDirectoryNotExists()
        {
            DevStackConfig.WriteColoredLine("O diretório bin global não existe.", ConsoleColor.Yellow);
        }

        private void LogBinAlreadyInPath()
        {
            DevStackConfig.WriteColoredLine("O diretório bin global já está no PATH.", ConsoleColor.Yellow);
        }

        private void LogBinAddedToPath(string binDir)
        {
            DevStackConfig.WriteColoredLine("O diretório bin global foi adicionado ao PATH do usuário:", ConsoleColor.Green);
            DevStackConfig.WriteColoredLine($"{INDENT}{binDir}", ConsoleColor.Yellow);
            DevStackConfig.WriteColoredLine("O PATH do terminal atual também foi atualizado.", ConsoleColor.Green);
        }

        private void LogPathsRemoved(List<string?> removedPaths)
        {
            DevStackConfig.WriteColoredLine("Os seguintes diretórios foram removidos do PATH do usuário:", ConsoleColor.Green);
            foreach (var path in removedPaths)
            {
                DevStackConfig.WriteColoredLine($"{INDENT}{path}", ConsoleColor.Yellow);
            }
        }

        private void LogNoDevStackInPath()
        {
            DevStackConfig.WriteColoredLine("Nenhum diretório DevStack encontrado no PATH.", ConsoleColor.Yellow);
        }

        private void LogDevStackPathsHeader()
        {
            DevStackConfig.WriteColoredLine("Diretórios do DevStack no PATH do usuário:", ConsoleColor.Cyan);
        }

        private void LogPathWithStatus(string path)
        {
            var exists = Directory.Exists(path);
            var status = exists ? STATUS_CHECK_MARK : STATUS_CROSS_MARK;
            var color = exists ? ConsoleColor.Green : ConsoleColor.Red;
            DevStackConfig.WriteColoredLine($"{INDENT}{status} {path}", color);
        }

        private List<string?> GetCurrentUserPathList()
        {
            var currentPath = Environment.GetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, EnvironmentVariableTarget.User) ?? string.Empty;
            
            return currentPath
                .Split(PATH_SEPARATOR, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
        }

        private void AddPathToUser(string newPath, List<string?> currentPathList)
        {
            var allPaths = currentPathList.Append(newPath);
            var newPathValue = string.Join(PATH_SEPARATOR, allPaths);
            Environment.SetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, newPathValue, EnvironmentVariableTarget.User);
        }

        private void UpdateUserPath(List<string?> newPathList)
        {
            var newPathValue = string.Join(PATH_SEPARATOR, newPathList);
            Environment.SetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, newPathValue, EnvironmentVariableTarget.User);
        }

        private void UpdateProcessPath()
        {
            try
            {
                var userPath = Environment.GetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, EnvironmentVariableTarget.User) ?? string.Empty;
                var machinePath = Environment.GetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, EnvironmentVariableTarget.Machine) ?? string.Empty;
                
                var combinedPath = CombinePaths(machinePath, userPath);
                Environment.SetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE_UPPERCASE, combinedPath);
                
                LogPathUpdateSuccess(combinedPath);
            }
            catch (Exception ex)
            {
                LogPathUpdateError(ex);
            }
        }

        private static string CombinePaths(string machinePath, string userPath)
        {
            if (string.IsNullOrEmpty(machinePath))
            {
                return userPath;
            }

            if (string.IsNullOrEmpty(userPath))
            {
                return machinePath;
            }

            return $"{machinePath}{PATH_SEPARATOR}{userPath}";
        }

        private void LogPathUpdateSuccess(string combinedPath)
        {
            var entryCount = combinedPath.Split(PATH_SEPARATOR).Length;
            DevStackConfig.WriteLog($"PATH do processo atualizado. Total de entradas: {entryCount}");
        }

        private void LogPathUpdateError(Exception ex)
        {
            DevStackConfig.WriteLog($"Erro ao atualizar PATH do processo: {ex.Message}");
        }
    }
}
