using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Centralized manager for DevStack service lifecycle operations.
    /// Handles starting, stopping, restarting, and monitoring component services like PHP-FPM, Nginx, MySQL, etc.
    /// </summary>
    public static class ProcessManager
    {
        /// <summary>
        /// Delay in milliseconds after starting a process.
        /// </summary>
        private const int PROCESS_START_DELAY_MS = 100;
        
        /// <summary>
        /// Delay in milliseconds between stop and start during service restart.
        /// </summary>
        private const int RESTART_DELAY_MS = 2000;
        
        /// <summary>
        /// Timeout in milliseconds for waiting process to exit gracefully.
        /// </summary>
        private const int PROCESS_EXIT_TIMEOUT_MS = 3000;
        
        /// <summary>
        /// Starts a component service asynchronously with support for multi-process workers.
        /// </summary>
        /// <param name="component">The component name (e.g., "php", "nginx").</param>
        /// <param name="version">The version to start.</param>
        /// <returns>True if the service started successfully; false otherwise.</returns>
        public static async Task<bool> StartServiceAsync(string component, string version)
        {
            try
            {
                if (IsServiceRunning(component, version))
                {
                    DevStackConfig.WriteColoredLine($"{component} {version} is already running.", ConsoleColor.Yellow);
                    return true;
                }

                var comp = Components.ComponentsFactory.GetComponent(component);
                if (!ValidateServiceComponent(comp, component))
                {
                    return false;
                }

                var (executablePath, workingDirectory) = GetServicePaths(comp!, component, version);
                if (!File.Exists(executablePath))
                {
                    DevStackConfig.WriteColoredLine($"{component} {version} is not installed.", ConsoleColor.Red);
                    return false;
                }

                var arguments = GetServiceArguments(component, version);
                var processCount = comp!.MaxWorkers ?? 1;
                var processPids = await StartServiceProcesses(executablePath, arguments, workingDirectory, processCount, component, version);

                if (processPids.Count == 0)
                {
                    return false;
                }

                RegisterServiceProcesses(component, version, processPids, executablePath);
                PrintServiceStartedMessage(component, version, processPids);
                return true;
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Error starting {component} {version}: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        /// <summary>
        /// Validates that the component is a valid service with required configuration.
        /// </summary>
        /// <param name="comp">The component interface instance.</param>
        /// <param name="component">The component name for error messaging.</param>
        /// <returns>True if valid; false otherwise.</returns>
        private static bool ValidateServiceComponent(Components.ComponentInterface? comp, string component)
        {
            if (comp == null || !comp.IsService || string.IsNullOrEmpty(comp.ServicePattern))
            {
                DevStackConfig.WriteColoredLine($"Component {component} is not a valid service.", ConsoleColor.Red);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Resolves the executable path and working directory for a service component.
        /// </summary>
        /// <param name="comp">The component interface.</param>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <returns>A tuple containing the executable path and working directory.</returns>
        private static (string executablePath, string workingDirectory) GetServicePaths(
            Components.ComponentInterface comp, 
            string component, 
            string version)
        {
            var executablePath = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}", comp.ServicePattern!);
            var workingDirectory = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}");
            return (executablePath, workingDirectory);
        }

        /// <summary>
        /// Starts multiple service processes based on the configured worker count.
        /// Rolls back all processes if any fail to start.
        /// </summary>
        /// <param name="executablePath">The path to the service executable.</param>
        /// <param name="arguments">Command-line arguments for the service.</param>
        /// <param name="workingDirectory">The working directory for the processes.</param>
        /// <param name="processCount">The number of worker processes to start.</param>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <returns>A list of process IDs, or an empty list if startup failed.</returns>
        private static async Task<List<int>> StartServiceProcesses(
            string executablePath,
            string arguments,
            string workingDirectory,
            int processCount,
            string component,
            string version)
        {
            var processPids = new List<int>();

            for (int i = 0; i < processCount; i++)
            {
                if (i > 0)
                {
                    await Task.Delay(PROCESS_START_DELAY_MS);
                }

                var process = await StartProcessAsync(executablePath, arguments, workingDirectory);
                if (process == null)
                {
                    DevStackConfig.WriteColoredLine($"Failed to start process {i + 1} of {component} {version}.", ConsoleColor.Red);
                    await KillStartedProcesses(processPids);
                    return new List<int>();
                }

                processPids.Add(process.Id);
            }

            return processPids;
        }

        /// <summary>
        /// Kills all processes in the provided list of PIDs. Used for rollback on startup failure.
        /// </summary>
        /// <param name="processPids">The list of process IDs to kill.</param>
        private static async Task KillStartedProcesses(List<int> processPids)
        {
            await Task.Run(() =>
            {
                foreach (var pid in processPids)
                {
                    try
                    {
                        var proc = Process.GetProcessById(pid);
                        proc.Kill();
                        proc.Dispose();
                    }
                    catch { }
                }
            });
        }

        /// <summary>
        /// Registers the service processes in the ProcessRegistry for tracking and management.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <param name="processPids">The list of process IDs to register.</param>
        /// <param name="executablePath">The path to the service executable.</param>
        private static void RegisterServiceProcesses(string component, string version, List<int> processPids, string executablePath)
        {
            if (processPids.Count > 0)
            {
                ProcessRegistry.RegisterService(component, version, processPids[0], executablePath);

                for (int i = 1; i < processPids.Count; i++)
                {
                    ProcessRegistry.AddMainProcess(component, version, processPids[i]);
                }
            }
        }

        /// <summary>
        /// Prints a formatted message indicating successful service startup with process IDs.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <param name="processPids">The list of started process IDs.</param>
        private static void PrintServiceStartedMessage(string component, string version, List<int> processPids)
        {
            var pidsText = processPids.Count == 1
                ? $"PID {processPids[0]}"
                : $"PIDs [{string.Join(", ", processPids)}]";

            DevStackConfig.WriteColoredLine($"{component} {version} started with {pidsText}.", ConsoleColor.Green);
        }

        /// <summary>
        /// Stops a running service by terminating all related processes.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version to stop.</param>
        /// <returns>True if the service was stopped successfully; false otherwise.</returns>
        public static async Task<bool> StopServiceAsync(string component, string version)
        {
            try
            {
                if (!IsServiceRunning(component, version))
                {
                    DevStackConfig.WriteColoredLine($"{component} {version} is not running.", ConsoleColor.Yellow);
                    ProcessRegistry.UnregisterService(component, version);
                    return true;
                }

                var mainPid = ProcessRegistry.GetMainProcessId(component, version);
                if (mainPid == null)
                {
                    DevStackConfig.WriteColoredLine($"Main process of {component} {version} not found.", ConsoleColor.Yellow);
                    return true;
                }

                await StopRelatedProcessesAsync(component, version, mainPid.Value);

                ProcessRegistry.UnregisterService(component, version);

                DevStackConfig.WriteColoredLine($"{component} {version} stopped.", ConsoleColor.Green);
                return true;
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Error stopping {component} {version}: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        /// <summary>
        /// Restarts a service by stopping and then starting it with a delay between operations.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version to restart.</param>
        /// <returns>True if the service restarted successfully; false otherwise.</returns>
        public static async Task<bool> RestartServiceAsync(string component, string version)
        {
            DevStackConfig.WriteColoredLine($"Restarting {component} {version}...", ConsoleColor.Cyan);

            if (IsServiceRunning(component, version))
            {
                await StopServiceAsync(component, version);
                await Task.Delay(RESTART_DELAY_MS);
            }

            return await StartServiceAsync(component, version);
        }

        /// <summary>
        /// Checks if a service is currently running.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <returns>True if the service is running; false otherwise.</returns>
        public static bool IsServiceRunning(string component, string version)
        {
            return ProcessRegistry.IsServiceActive(component, version);
        }

        /// <summary>
        /// Retrieves the status of all installed service components.
        /// </summary>
        /// <returns>A list of ServiceStatus objects containing component information and running state.</returns>
        public static async Task<List<ServiceStatus>> GetAllServicesStatusAsync()
        {
            var services = new List<ServiceStatus>();
            var serviceComponents = Components.ComponentsFactory.GetAll().Where(c => c.IsService);

            await Task.Run(() =>
            {
                foreach (var component in serviceComponents)
                {
                    var versions = component.ListInstalled();
                    foreach (var version in versions)
                    {
                        var isRunning = IsServiceRunning(component.Name, version);
                        var mainPid = isRunning ? ProcessRegistry.GetMainProcessId(component.Name, version) : null;
                        
                        services.Add(new ServiceStatus
                        {
                            Component = component.Name,
                            Version = version,
                            IsRunning = isRunning,
                            MainProcessId = mainPid,
                            AllProcessIds = isRunning ? GetAllRelatedProcessIds(component.Name, version) : new List<int>()
                        });
                    }
                }
            });

            return services;
        }

        /// <summary>
        /// Stops all currently running services.
        /// </summary>
        public static async Task StopAllServicesAsync()
        {
            var activeServices = ProcessRegistry.GetActiveServices();
            var stopTasks = activeServices.Select(service => 
                StopServiceAsync(service.Component, service.Version)
            );

            await Task.WhenAll(stopTasks);
        }

        /// <summary>
        /// Starts all installed service components that are not currently running.
        /// </summary>
        public static async Task StartAllServicesAsync()
        {
            var serviceComponents = Components.ComponentsFactory.GetAll().Where(c => c.IsService);
            var startTasks = new List<Task<bool>>();

            foreach (var component in serviceComponents)
            {
                var versions = component.ListInstalled();
                foreach (var version in versions)
                {
                    if (!IsServiceRunning(component.Name, version))
                    {
                        startTasks.Add(StartServiceAsync(component.Name, version));
                    }
                }
            }

            await Task.WhenAll(startTasks);
        }

        /// <summary>
        /// Synchronous wrapper for StartServiceAsync.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version to start.</param>
        /// <returns>True if started successfully.</returns>
        public static bool StartService(string component, string version) => 
            StartServiceAsync(component, version).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for StopServiceAsync.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version to stop.</param>
        /// <returns>True if stopped successfully.</returns>
        public static bool StopService(string component, string version) => 
            StopServiceAsync(component, version).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for RestartServiceAsync.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version to restart.</param>
        /// <returns>True if restarted successfully.</returns>
        public static bool RestartService(string component, string version) => 
            RestartServiceAsync(component, version).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for StopAllServicesAsync.
        /// </summary>
        public static void StopAllServices() => 
            StopAllServicesAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for StartAllServicesAsync.
        /// </summary>
        public static void StartAllServices() => 
            StartAllServicesAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Legacy alias for StartServiceAsync. Provided for backward compatibility.
        /// </summary>
        public static async Task<bool> StartComponentAsync(string component, string version) => 
            await StartServiceAsync(component, version);

        /// <summary>
        /// Legacy alias for StopServiceAsync. Provided for backward compatibility.
        /// </summary>
        public static async Task<bool> StopComponentAsync(string component, string version) => 
            await StopServiceAsync(component, version);

        /// <summary>
        /// Legacy alias for RestartServiceAsync. Provided for backward compatibility.
        /// </summary>
        public static async Task RestartComponentAsync(string component, string version) => 
            await RestartServiceAsync(component, version);

        /// <summary>
        /// Legacy alias for IsServiceRunning. Provided for backward compatibility.
        /// </summary>
        public static bool IsComponentRunning(string component, string version) => 
            IsServiceRunning(component, version);

        /// <summary>
        /// Legacy alias for StartService. Provided for backward compatibility.
        /// </summary>
        public static void StartComponent(string component, string version) => 
            StartService(component, version);

        /// <summary>
        /// Legacy alias for StopService. Provided for backward compatibility.
        /// </summary>
        public static void StopComponent(string component, string version) => 
            StopService(component, version);

        /// <summary>
        /// Legacy alias for RestartService. Provided for backward compatibility.
        /// </summary>
        public static void RestartComponent(string component, string version) => 
            RestartService(component, version);

        /// <summary>
        /// Displays a formatted list of all service statuses in the console.
        /// </summary>
        public static async Task ListComponentsStatusAsync()
        {
            var services = await GetAllServicesStatusAsync();
            
            DevStackConfig.WriteColoredLine("Service status:", ConsoleColor.Cyan);
            Console.WriteLine();

            foreach (var group in services.GroupBy(s => s.Component))
            {
                DevStackConfig.WriteColoredLine($"{group.Key.ToUpper()}:", ConsoleColor.Yellow);
                
                foreach (var service in group)
                {
                    var status = service.IsRunning ? "RUNNING" : "STOPPED";
                    var color = service.IsRunning ? ConsoleColor.Green : ConsoleColor.Red;
                    var pidInfo = service.IsRunning && service.AllProcessIds.Any() 
                        ? $" (PIDs: {string.Join(",", service.AllProcessIds)})" 
                        : "";
                    
                    Console.Write($"  {service.Component}-{service.Version}: ");
                    DevStackConfig.WriteColoredLine($"{status}{pidInfo}", color);
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Synchronous wrapper for ListComponentsStatusAsync.
        /// </summary>
        public static void ListComponentsStatus() => 
            ListComponentsStatusAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Starts a new process asynchronously with the specified configuration.
        /// </summary>
        /// <param name="fileName">The executable file name or path.</param>
        /// <param name="arguments">Command-line arguments.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The started Process instance, or null if startup failed.</returns>
        private static async Task<Process?> StartProcessAsync(string fileName, string arguments, string workingDirectory)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        WorkingDirectory = workingDirectory,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    return Process.Start(startInfo);
                }
                catch
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// Terminates all processes related to a service component.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <param name="mainPid">The main process ID.</param>
        private static async Task StopRelatedProcessesAsync(string component, string version, int mainPid)
        {
            await Task.Run(() =>
            {
                var registeredPids = GetRegisteredPidsOrFallback(component, version, mainPid);

                foreach (var pid in registeredPids)
                {
                    TryKillProcess(pid);
                }
            });
        }

        /// <summary>
        /// Retrieves registered PIDs from the registry or falls back to the main PID if none are registered.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <param name="mainPid">The main process ID to use as fallback.</param>
        /// <returns>A list of process IDs.</returns>
        private static List<int> GetRegisteredPidsOrFallback(string component, string version, int mainPid)
        {
            var registeredPids = ProcessRegistry.GetServicePids(component, version, null);

            if (registeredPids.Count == 0)
            {
                registeredPids.Add(mainPid);
            }

            return registeredPids;
        }

        /// <summary>
        /// Attempts to kill a process by PID, silently ignoring errors if the process doesn't exist or has already exited.
        /// </summary>
        /// <param name="pid">The process ID to kill.</param>
        private static void TryKillProcess(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(PROCESS_EXIT_TIMEOUT_MS);
                }
                process.Dispose();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Retrieves all process IDs related to a specific service component and version.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <returns>A list of process IDs, or an empty list if none are found.</returns>
        private static List<int> GetAllRelatedProcessIds(string component, string version)
        {
            try
            {
                var comp = Components.ComponentsFactory.GetComponent(component);
                if (comp?.ServicePattern == null)
                {
                    return new List<int>();
                }

                return FindProcessesByComponentAndVersion(comp, component, version);
            }
            catch
            {
                return new List<int>();
            }
        }

        /// <summary>
        /// Finds all processes matching a component and version by searching for running processes with matching executable paths.
        /// </summary>
        /// <param name="comp">The component interface.</param>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <returns>A list of matching process IDs.</returns>
        private static List<int> FindProcessesByComponentAndVersion(
            Components.ComponentInterface comp,
            string component,
            string version)
        {
            var pids = new List<int>();
            var executablePath = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}", comp.ServicePattern!);
            var processName = Path.GetFileNameWithoutExtension(comp.ServicePattern);

            var processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
            {
                try
                {
                    if (IsProcessMatchingVersion(process, executablePath, component, version))
                    {
                        pids.Add(process.Id);
                    }
                }
                catch { }
                finally
                {
                    process.Dispose();
                }
            }

            return pids;
        }

        /// <summary>
        /// Checks if a process matches the expected version by comparing executable paths and additional criteria.
        /// </summary>
        /// <param name="process">The process to check.</param>
        /// <param name="expectedPath">The expected executable path.</param>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <returns>True if the process matches; false otherwise.</returns>
        private static bool IsProcessMatchingVersion(
            Process process,
            string expectedPath,
            string component,
            string version)
        {
            var processPath = process.MainModule?.FileName;
            if (processPath?.Equals(expectedPath, StringComparison.OrdinalIgnoreCase) != true)
            {
                return false;
            }

            if (component.Equals("php", StringComparison.OrdinalIgnoreCase))
            {
                return IsPhpProcessForVersion(process.Id, version);
            }

            return true;
        }

        /// <summary>
        /// Verifies if a PHP-FPM process belongs to a specific version by checking its command-line arguments for the bind address.
        /// </summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="version">The version string.</param>
        /// <returns>True if the process matches the version; false or true (fallback) if verification fails.</returns>
        private static bool IsPhpProcessForVersion(int processId, string version)
        {
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
                
                foreach (System.Management.ManagementObject obj in searcher.Get())
                {
                    var commandLine = obj["CommandLine"]?.ToString();
                    return commandLine?.Contains($"127.{version}:9000") == true;
                }
            }
            catch
            {
            }
            
            return true;
        }

        /// <summary>
        /// Returns component-specific command-line arguments for service startup.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="version">The version string.</param>
        /// <returns>The command-line arguments string.</returns>
        private static string GetServiceArguments(string component, string version)
        {
            return component.ToLowerInvariant() switch
            {
                "php" => $"-b 127.{version}:9000",
                "nginx" => "",
                "mysql" => "--console",
                _ => ""
            };
        }

        /// <summary>
        /// Executes a process and captures its standard output.
        /// </summary>
        /// <param name="fileName">The executable file name or path.</param>
        /// <param name="arguments">Command-line arguments.</param>
        /// <param name="workingDirectory">Optional working directory.</param>
        /// <param name="windowStyle">Optional window style.</param>
        /// <returns>The trimmed standard output from the process.</returns>
        public static async Task<string> ExecuteProcessAsync(string fileName, string arguments, string? workingDirectory = null, ProcessWindowStyle? windowStyle = null)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? "",
                WindowStyle = windowStyle ?? ProcessWindowStyle.Hidden
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Trim();
        }

        /// <summary>
        /// Synchronous wrapper for ExecuteProcessAsync.
        /// </summary>
        /// <param name="fileName">The executable file name or path.</param>
        /// <param name="arguments">Command-line arguments.</param>
        /// <param name="workingDirectory">Optional working directory.</param>
        /// <param name="windowStyle">Optional window style.</param>
        /// <returns>The trimmed standard output from the process.</returns>
        public static string ExecuteProcess(string fileName, string arguments, string? workingDirectory = null, ProcessWindowStyle? windowStyle = null)
        {
            return ExecuteProcessAsync(fileName, arguments, workingDirectory, windowStyle).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Iterates through all installed versions of a component and executes an action for each version.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="action">The action to execute for each version.</param>
        public static void ForEachVersion(string component, Action<string> action)
        {
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp == null)
            {
                DevStackConfig.WriteColoredLine($"Unknown component: {component}", ConsoleColor.Red);
                return;
            }

            if (!Directory.Exists(comp.ToolDir))
            {
                return;
            }

            var prefix = $"{comp.Name}-";
            
            try
            {
                var directories = Directory.GetDirectories(comp.ToolDir, $"{prefix}*");
                
                foreach (var directory in directories)
                {
                    var dirName = Path.GetFileName(directory);
                    if (dirName.StartsWith(prefix))
                    {
                        var version = dirName.Substring(prefix.Length);
                        action(version);
                    }
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Error listing versions of {component}: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Legacy alias for StopAllServicesAsync.
        /// </summary>
        public static async Task StopAllComponentsAsync() => await StopAllServicesAsync();
        
        /// <summary>
        /// Legacy alias for StopAllServices.
        /// </summary>
        public static void StopAllComponents() => StopAllServices();
        
        /// <summary>
        /// Legacy alias for StartAllServicesAsync.
        /// </summary>
        public static async Task StartAllComponentsAsync() => await StartAllServicesAsync();
        
        /// <summary>
        /// Legacy alias for StartAllServices.
        /// </summary>
        public static void StartAllComponents() => StartAllServices();
    }

    /// <summary>
    /// Represents the running status of a DevStack service component.
    /// </summary>
    public class ServiceStatus
    {
        /// <summary>
        /// The component name (e.g., "php", "nginx").
        /// </summary>
        public string Component { get; set; } = "";
        
        /// <summary>
        /// The installed version string.
        /// </summary>
        public string Version { get; set; } = "";
        
        /// <summary>
        /// Indicates whether the service is currently running.
        /// </summary>
        public bool IsRunning { get; set; }
        
        /// <summary>
        /// The main process ID, or null if not running.
        /// </summary>
        public int? MainProcessId { get; set; }
        
        /// <summary>
        /// All process IDs related to this service (including workers).
        /// </summary>
        public List<int> AllProcessIds { get; set; } = new();
    }
}
