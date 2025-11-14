using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevStackManager
{
    /// <summary>
    /// Manages Windows PATH environment variable modifications for DevStack components.
    /// Handles adding/removing directories to user PATH and synchronizing process environment.
    /// </summary>
    public class PathManager
    {
        /// <summary>
        /// Base directory for DevStack installation.
        /// </summary>
        private readonly string _baseDir;
        
        /// <summary>
        /// Name of the bin directory.
        /// </summary>
        private const string BIN_DIRECTORY_NAME = "bin";
        
        /// <summary>
        /// Environment variable name for PATH (mixed case).
        /// </summary>
        private const string PATH_ENVIRONMENT_VARIABLE = "Path";
        
        /// <summary>
        /// Environment variable name for PATH (uppercase).
        /// </summary>
        private const string PATH_ENVIRONMENT_VARIABLE_UPPERCASE = "PATH";
        
        /// <summary>
        /// Character used to separate PATH entries.
        /// </summary>
        private const char PATH_SEPARATOR = ';';
        
        /// <summary>
        /// Check mark symbol for status display.
        /// </summary>
        private const string STATUS_CHECK_MARK = "✓";
        
        /// <summary>
        /// Cross mark symbol for status display.
        /// </summary>
        private const string STATUS_CROSS_MARK = "✗";
        
        /// <summary>
        /// Indentation string for formatted output.
        /// </summary>
        private const string INDENT = "  ";

        /// <summary>
        /// Initializes a new instance of the PathManager class.
        /// </summary>
        /// <param name="baseDir">The base directory for DevStack.</param>
        /// <param name="phpDir">PHP directory path (legacy parameter, unused).</param>
        /// <param name="nodeDir">Node.js directory path (legacy parameter, unused).</param>
        /// <param name="pythonDir">Python directory path (legacy parameter, unused).</param>
        /// <param name="nginxDir">Nginx directory path (legacy parameter, unused).</param>
        /// <param name="mysqlDir">MySQL directory path (legacy parameter, unused).</param>
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

        /// <summary>
        /// Normalizes a file system path by converting to full path and removing trailing backslashes.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path, or null if the input is null or whitespace.</returns>
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

        /// <summary>
        /// Adds the global bin directory to the user's PATH environment variable.
        /// Updates both persistent user PATH and current process PATH.
        /// </summary>
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

        /// <summary>
        /// Removes specified directories from the user's PATH environment variable.
        /// </summary>
        /// <param name="dirsToRemove">Array of directory paths to remove.</param>
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

        /// <summary>
        /// Removes all DevStack-related directories from the user's PATH.
        /// Specifically removes the global bin directory.
        /// </summary>
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

        /// <summary>
        /// Lists all DevStack directories currently in the user's PATH and displays their existence status.
        /// </summary>
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

        /// <summary>
        /// Logs a message indicating the bin directory does not exist.
        /// </summary>
        private void LogBinDirectoryNotExists()
        {
            DevStackConfig.WriteColoredLine("The global bin directory does not exist.", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs a message indicating the bin directory is already in PATH.
        /// </summary>
        private void LogBinAlreadyInPath()
        {
            DevStackConfig.WriteColoredLine("The global bin directory is already in PATH.", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs a success message after adding the bin directory to PATH.
        /// </summary>
        /// <param name="binDir">The bin directory path that was added.</param>
        private void LogBinAddedToPath(string binDir)
        {
            DevStackConfig.WriteColoredLine("The global bin directory was added to user PATH:", ConsoleColor.Green);
            DevStackConfig.WriteColoredLine($"{INDENT}{binDir}", ConsoleColor.Yellow);
            DevStackConfig.WriteColoredLine("The current terminal PATH has also been updated.", ConsoleColor.Green);
        }

        /// <summary>
        /// Logs the directories that were removed from PATH.
        /// </summary>
        /// <param name="removedPaths">List of removed directory paths.</param>
        private void LogPathsRemoved(List<string?> removedPaths)
        {
            DevStackConfig.WriteColoredLine("The following directories were removed from user PATH:", ConsoleColor.Green);
            foreach (var path in removedPaths)
            {
                DevStackConfig.WriteColoredLine($"{INDENT}{path}", ConsoleColor.Yellow);
            }
        }

        /// <summary>
        /// Logs a message indicating no DevStack directories were found in PATH.
        /// </summary>
        private void LogNoDevStackInPath()
        {
            DevStackConfig.WriteColoredLine("No DevStack directories found in PATH.", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs the header for listing DevStack directories in PATH.
        /// </summary>
        private void LogDevStackPathsHeader()
        {
            DevStackConfig.WriteColoredLine("DevStack directories in user PATH:", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Logs a path with a status indicator showing whether it exists.
        /// </summary>
        /// <param name="path">The path to log.</param>
        private void LogPathWithStatus(string path)
        {
            var exists = Directory.Exists(path);
            var status = exists ? STATUS_CHECK_MARK : STATUS_CROSS_MARK;
            var color = exists ? ConsoleColor.Green : ConsoleColor.Red;
            DevStackConfig.WriteColoredLine($"{INDENT}{status} {path}", color);
        }

        /// <summary>
        /// Retrieves the current user PATH as a list of normalized directory paths.
        /// </summary>
        /// <returns>A list of directory paths from the user PATH variable.</returns>
        private List<string?> GetCurrentUserPathList()
        {
            var currentPath = Environment.GetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, EnvironmentVariableTarget.User) ?? string.Empty;
            
            return currentPath
                .Split(PATH_SEPARATOR, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizePath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
        }

        /// <summary>
        /// Adds a new path to the user's PATH environment variable.
        /// </summary>
        /// <param name="newPath">The path to add.</param>
        /// <param name="currentPathList">The current list of paths.</param>
        private void AddPathToUser(string newPath, List<string?> currentPathList)
        {
            var allPaths = currentPathList.Append(newPath);
            var newPathValue = string.Join(PATH_SEPARATOR, allPaths);
            Environment.SetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, newPathValue, EnvironmentVariableTarget.User);
        }

        /// <summary>
        /// Updates the user's PATH environment variable with a new list of paths.
        /// </summary>
        /// <param name="newPathList">The new list of paths.</param>
        private void UpdateUserPath(List<string?> newPathList)
        {
            var newPathValue = string.Join(PATH_SEPARATOR, newPathList);
            Environment.SetEnvironmentVariable(PATH_ENVIRONMENT_VARIABLE, newPathValue, EnvironmentVariableTarget.User);
        }

        /// <summary>
        /// Updates the current process PATH by combining machine and user PATH variables.
        /// This makes PATH changes immediately available in the current terminal session.
        /// </summary>
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

        /// <summary>
        /// Combines machine and user PATH strings with proper separator handling.
        /// </summary>
        /// <param name="machinePath">The system-wide PATH.</param>
        /// <param name="userPath">The user-specific PATH.</param>
        /// <returns>The combined PATH string.</returns>
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

        /// <summary>
        /// Logs a successful process PATH update with entry count.
        /// </summary>
        /// <param name="combinedPath">The combined PATH value.</param>
        private void LogPathUpdateSuccess(string combinedPath)
        {
            var entryCount = combinedPath.Split(PATH_SEPARATOR).Length;
            DevStackConfig.WriteLog($"Process PATH updated. Total entries: {entryCount}");
        }

        /// <summary>
        /// Logs an error that occurred during process PATH update.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        private void LogPathUpdateError(Exception ex)
        {
            DevStackConfig.WriteLog($"Error updating process PATH: {ex.Message}");
        }
    }
}
