using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DevStackManager
{
    /// <summary>
    /// Information about a registered service main process.
    /// Supports multiple main process IDs for services with worker processes.
    /// </summary>
    public class ServiceProcess
    {
        /// <summary>
        /// Gets or sets the list of main process IDs for this service.
        /// Supports multiple worker processes for components with MaxWorkers > 1.
        /// </summary>
        public List<int> MainProcessIds { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the component name (e.g., "php", "nginx", "mysql").
        /// </summary>
        public string Component { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the installed version string for this service instance.
        /// </summary>
        public string Version { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the full path to the service executable file.
        /// </summary>
        public string ExecutablePath { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the timestamp when the service was started.
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the unique identifier for this service instance (8-character short GUID).
        /// </summary>
        public string UniqueId { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the primary main process ID.
        /// Returns the first PID from MainProcessIds, or 0 if empty.
        /// Setting this property replaces MainProcessIds with a single-element list.
        /// </summary>
        public int MainProcessId 
        { 
            get => MainProcessIds.Count > 0 ? MainProcessIds[0] : 0;
            set => MainProcessIds = new List<int> { value };
        }
    }

    /// <summary>
    /// Simple and efficient registry for tracking service main processes.
    /// Manages process lifecycle, persistence, and cleanup for DevStack services.
    /// </summary>
    public static class ProcessRegistry
    {
        /// <summary>
        /// Concurrent dictionary storing registered services by component-version key.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ServiceProcess> _services = new();
        
        /// <summary>
        /// File path for persisting service registry data.
        /// </summary>
        private static readonly string _registryFile = Path.Combine(DevStackConfig.tmpDir, "devstack_services.json");
        
        /// <summary>
        /// Length of shortened GUID used for unique service identifiers.
        /// </summary>
        private const int GUID_SHORT_LENGTH = 8;

        /// <summary>
        /// Registers a service main process.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="mainProcessId">Main process ID.</param>
        /// <param name="executablePath">Full path to the service executable.</param>
        public static void RegisterService(string component, string version, int mainProcessId, string executablePath)
        {
            RegisterServiceInternal(component, version, new List<int> { mainProcessId }, executablePath);
        }

        /// <summary>
        /// Registers multiple main processes for a service (for components with MaxWorkers > 1).
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="mainProcessIds">List of main process IDs.</param>
        /// <param name="executablePath">Full path to the service executable.</param>
        public static void RegisterServiceWithMultipleProcesses(string component, string version, List<int> mainProcessIds, string executablePath)
        {
            RegisterServiceInternal(component, version, mainProcessIds, executablePath);
        }

        /// <summary>
        /// Internal method to register a service with its process information.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="mainProcessIds">List of main process IDs.</param>
        /// <param name="executablePath">Full path to the service executable.</param>
        private static void RegisterServiceInternal(string component, string version, List<int> mainProcessIds, string executablePath)
        {
            var key = GetServiceKey(component, version);
            var uniqueId = GenerateUniqueId();

            var service = new ServiceProcess
            {
                MainProcessIds = new List<int>(mainProcessIds),
                Component = component,
                Version = version,
                ExecutablePath = executablePath,
                StartTime = DateTime.Now,
                UniqueId = uniqueId
            };

            _services[key] = service;
            SaveToFile();

            var pidsText = mainProcessIds.Count == 1
                ? $"PID={mainProcessIds[0]}"
                : $"PIDs=[{string.Join(", ", mainProcessIds)}]";

            DevStackConfig.WriteLog($"Service registered: {component}-{version} {pidsText} ID={uniqueId}");
        }

        /// <summary>
        /// Generates a short unique identifier (8 characters) for service tracking.
        /// </summary>
        /// <returns>Short GUID string.</returns>
        private static string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString("N")[..GUID_SHORT_LENGTH];
        }

        /// <summary>
        /// Adds an additional main process for an existing service.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="processId">Process ID to add.</param>
        public static void AddMainProcess(string component, string version, int processId)
        {
            var key = GetServiceKey(component, version);
            
            if (_services.TryGetValue(key, out var service))
            {
                if (!service.MainProcessIds.Contains(processId))
                {
                    if (IsProcessAlive(processId))
                    {
                        service.MainProcessIds.Add(processId);
                        SaveToFile();
                        DevStackConfig.WriteLog($"Main process added: {component}-{version} PID={processId}");
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters a service from the registry.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        public static void UnregisterService(string component, string version)
        {
            var key = GetServiceKey(component, version);
            if (_services.TryRemove(key, out var service))
            {
                SaveToFile();
                DevStackConfig.WriteLog($"Service removed: {component}-{version} PID={service.MainProcessId}");
            }
        }

        /// <summary>
        /// Cleans up duplicate and dead PIDs for a specific service.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        public static void CleanupServicePids(string component, string version)
        {
            var key = GetServiceKey(component, version);
            
            if (_services.TryGetValue(key, out var service))
            {
                var originalCount = service.MainProcessIds.Count;
                
                var cleanPids = service.MainProcessIds
                    .Distinct()
                    .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                    .ToList();
                
                if (cleanPids.Count != originalCount)
                {
                    service.MainProcessIds = cleanPids;
                    SaveToFile();
                    
                    DevStackConfig.WriteLog($"PIDs cleaned for {component}-{version}: {originalCount} -> {cleanPids.Count} PIDs");
                    
                    if (cleanPids.Count == 0)
                    {
                        UnregisterService(component, version);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a service is active (at least one main process exists).
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <returns>True if service has at least one alive process, false otherwise.</returns>
        public static bool IsServiceActive(string component, string version)
        {
            return UpdateServicePidsAndCheckActive(component, version);
        }

        /// <summary>
        /// Gets the PID of the first main process of a service (compatibility method).
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <returns>First alive process ID, or null if none exist.</returns>
        public static int? GetMainProcessId(string component, string version)
        {
            var alivePids = GetAndUpdateAlivePids(component, version);
            return alivePids.Count > 0 ? alivePids[0] : null;
        }

        /// <summary>
        /// Updates service PIDs and checks if service is still active.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <returns>True if service is active, false if no alive processes found.</returns>
        private static bool UpdateServicePidsAndCheckActive(string component, string version)
        {
            var key = GetServiceKey(component, version);

            if (!_services.TryGetValue(key, out var service))
            {
                return false;
            }

            var alivePids = GetAlivePidsFromService(service);

            if (alivePids.Count > 0)
            {
                UpdateServicePidsIfChanged(service, alivePids);
                return true;
            }

            UnregisterService(component, version);
            return false;
        }

        /// <summary>
        /// Gets alive PIDs for a service and updates the registry if needed.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <returns>List of alive process IDs.</returns>
        private static List<int> GetAndUpdateAlivePids(string component, string version)
        {
            var key = GetServiceKey(component, version);

            if (!_services.TryGetValue(key, out var service))
            {
                return new List<int>();
            }

            var alivePids = GetAlivePidsFromService(service);

            if (alivePids.Count > 0)
            {
                UpdateServicePidsIfChanged(service, alivePids);
                return alivePids;
            }

            UnregisterService(component, version);
            return new List<int>();
        }

        /// <summary>
        /// Gets the list of alive PIDs from a service process record.
        /// </summary>
        /// <param name="service">The service process record.</param>
        /// <returns>List of alive process IDs.</returns>
        private static List<int> GetAlivePidsFromService(ServiceProcess service)
        {
            return service.MainProcessIds
                .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                .ToList();
        }

        /// <summary>
        /// Updates service PIDs in the registry if they have changed.
        /// </summary>
        /// <param name="service">The service process record.</param>
        /// <param name="alivePids">List of currently alive PIDs.</param>
        private static void UpdateServicePidsIfChanged(ServiceProcess service, List<int> alivePids)
        {
            if (alivePids.Count != service.MainProcessIds.Count)
            {
                service.MainProcessIds = alivePids;
                SaveToFile();
            }
        }

        /// <summary>
        /// Gets all PIDs of the main processes for a service, limited by maxWorkers if specified.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="maxWorkers">Optional maximum number of worker processes.</param>
        /// <returns>List of alive process IDs.</returns>
        public static List<int> GetServicePids(string component, string version, int? maxWorkers)
        {
            var key = GetServiceKey(component, version);

            if (!_services.TryGetValue(key, out var service))
            {
                return new List<int>();
            }

            var alivePids = GetCleanedAlivePids(service);

            if (alivePids.Count == 0)
            {
                UnregisterService(component, version);
                return new List<int>();
            }

            var limitedPids = ApplyMaxWorkersLimit(alivePids, maxWorkers);
            UpdateServicePidsIfNeeded(service, component, version, alivePids, limitedPids);

            return limitedPids;
        }

        /// <summary>
        /// Gets cleaned list of alive PIDs, removing duplicates and dead processes.
        /// </summary>
        /// <param name="service">The service process record.</param>
        /// <returns>Cleaned list of alive process IDs.</returns>
        private static List<int> GetCleanedAlivePids(ServiceProcess service)
        {
            return service.MainProcessIds
                .Distinct()
                .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                .ToList();
        }

        /// <summary>
        /// Applies the maximum workers limit to a list of PIDs.
        /// </summary>
        /// <param name="pids">List of process IDs.</param>
        /// <param name="maxWorkers">Optional maximum number of workers.</param>
        /// <returns>Limited list of process IDs.</returns>
        private static List<int> ApplyMaxWorkersLimit(List<int> pids, int? maxWorkers)
        {
            if (maxWorkers.HasValue && maxWorkers.Value > 0)
            {
                return pids.Take(maxWorkers.Value).ToList();
            }
            return pids;
        }

        /// <summary>
        /// Updates service PIDs in the registry if they have changed from the tracked list.
        /// </summary>
        /// <param name="service">The service process record.</param>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="alivePids">List of all alive PIDs.</param>
        /// <param name="limitedPids">List of PIDs after applying worker limit.</param>
        private static void UpdateServicePidsIfNeeded(
            ServiceProcess service,
            string component,
            string version,
            List<int> alivePids,
            List<int> limitedPids)
        {
            if (alivePids.Count != service.MainProcessIds.Count ||
                !alivePids.SequenceEqual(service.MainProcessIds))
            {
                service.MainProcessIds = alivePids;
                SaveToFile();
                DevStackConfig.WriteLog($"PIDs updated for {component}-{version}: {string.Join(", ", limitedPids)}");
            }
        }

        /// <summary>
        /// Lists all registered active services.
        /// </summary>
        /// <returns>List of active service processes.</returns>
        public static List<ServiceProcess> GetActiveServices()
        {
            var activeServices = new List<ServiceProcess>();
            var deadServiceKeys = new List<string>();

            foreach (var kvp in _services)
            {
                var alivePids = GetAlivePidsFromService(kvp.Value);

                if (alivePids.Count > 0)
                {
                    kvp.Value.MainProcessIds = alivePids;
                    activeServices.Add(kvp.Value);
                }
                else
                {
                    deadServiceKeys.Add(kvp.Key);
                }
            }

            RemoveDeadServices(deadServiceKeys);
            return activeServices;
        }

        /// <summary>
        /// Removes dead services from the registry based on provided service keys.
        /// </summary>
        /// <param name="deadServiceKeys">List of service keys to remove.</param>
        private static void RemoveDeadServices(List<string> deadServiceKeys)
        {
            if (deadServiceKeys.Count == 0) return;

            foreach (var key in deadServiceKeys)
            {
                _services.TryRemove(key, out _);
            }

            SaveToFile();
        }

        /// <summary>
        /// Discovers and updates all main processes for a service automatically.
        /// Scans running processes matching the service pattern and updates the registry.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="servicePattern">Service executable filename pattern.</param>
        /// <param name="toolDir">Tool installation directory.</param>
        /// <param name="maxWorkers">Optional maximum number of worker processes.</param>
        public static void DiscoverAndUpdateMainProcesses(string component, string version, string servicePattern, string toolDir, int? maxWorkers)
        {
            var key = GetServiceKey(component, version);
            
            if (!_services.TryGetValue(key, out var service))
                return;

            try
            {
                var processName = Path.GetFileNameWithoutExtension(servicePattern);
                var serviceExe = Path.Combine(toolDir, $"{component}-{version}", servicePattern);
                
                if (!File.Exists(serviceExe))
                    return;

                var runningProcesses = Process.GetProcessesByName(processName);
                var foundPids = new List<int>();
                
                foreach (var proc in runningProcesses)
                {
                    try
                    {
                        var processPath = proc.MainModule?.FileName;
                        if (processPath?.Equals(serviceExe, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            foundPids.Add(proc.Id);
                        }
                    }
                    catch { }
                    finally { proc.Dispose(); }
                }

                var alivePids = service.MainProcessIds.Where(pid => IsProcessAlive(pid, service.ExecutablePath)).ToList();
                
                var newPids = foundPids.Where(pid => !alivePids.Contains(pid)).ToList();
                
                if (alivePids.Count != service.MainProcessIds.Count || newPids.Count > 0)
                {
                    var allValidPids = alivePids.Concat(newPids).Distinct().ToList();
                    
                    if (maxWorkers.HasValue && maxWorkers.Value > 0)
                    {
                        allValidPids = allValidPids.Take(maxWorkers.Value).ToList();
                    }
                    
                    service.MainProcessIds = allValidPids;
                    SaveToFile();
                    
                    if (newPids.Count > 0)
                    {
                        DevStackConfig.WriteLog($"Main processes discovered for {component}-{version}: {string.Join(", ", newPids)}");
                    }
                    
                    if (alivePids.Count != service.MainProcessIds.Count)
                    {
                        DevStackConfig.WriteLog($"PIDs cleaned for {component}-{version}. Active PIDs: {string.Join(", ", allValidPids)}");
                    }
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteLog($"Error discovering main processes for {component}-{version}: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads services from the registry file.
        /// </summary>
        public static void LoadFromFile()
        {
            try
            {
                if (File.Exists(_registryFile))
                {
                    var json = File.ReadAllText(_registryFile);
                    var services = JsonSerializer.Deserialize<Dictionary<string, ServiceProcess>>(json);
                    
                    if (services != null)
                    {
                        _services.Clear();
                        foreach (var kvp in services)
                        {
                            _services[kvp.Key] = kvp.Value;
                        }
                        
                        CleanupAllServices();
                    }
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteLog($"Error loading services: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up duplicate and dead PIDs for all registered services.
        /// </summary>
        public static void CleanupAllServices()
        {
            var deadServiceKeys = new List<string>();
            bool hasChanges = false;

            foreach (var kvp in _services)
            {
                var cleanResult = CleanServicePids(kvp.Value, kvp.Key);
                
                if (cleanResult.shouldRemove)
                {
                    deadServiceKeys.Add(kvp.Key);
                    hasChanges = true;
                }
                else if (cleanResult.hasChanges)
                {
                    hasChanges = true;
                }
            }

            RemoveServices(deadServiceKeys);

            if (hasChanges)
            {
                SaveToFile();
            }
        }

        /// <summary>
        /// Cleans dead PIDs from a service and determines if service should be removed.
        /// </summary>
        /// <param name="service">The service to clean.</param>
        /// <param name="key">The service key.</param>
        /// <returns>Tuple indicating if service should be removed and if changes were made.</returns>
        private static (bool shouldRemove, bool hasChanges) CleanServicePids(ServiceProcess service, string key)
        {
            var originalCount = service.MainProcessIds.Count;

            var cleanPids = service.MainProcessIds
                .Distinct()
                .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                .ToList();

            if (cleanPids.Count == 0)
            {
                return (shouldRemove: true, hasChanges: false);
            }

            if (cleanPids.Count != originalCount)
            {
                service.MainProcessIds = cleanPids;
                DevStackConfig.WriteLog($"PIDs cleaned for {service.Component}-{service.Version}: {originalCount} -> {cleanPids.Count}");
                return (shouldRemove: false, hasChanges: true);
            }

            return (shouldRemove: false, hasChanges: false);
        }

        /// <summary>
        /// Removes multiple services from the registry by their keys.
        /// </summary>
        /// <param name="serviceKeys">List of service keys to remove.</param>
        private static void RemoveServices(List<string> serviceKeys)
        {
            foreach (var key in serviceKeys)
            {
                if (_services.TryRemove(key, out var removedService))
                {
                    DevStackConfig.WriteLog($"Service removed (no alive processes): {removedService.Component}-{removedService.Version}");
                }
            }
        }

        /// <summary>
        /// Saves services to the registry file.
        /// </summary>
        private static void SaveToFile()
        {
            try
            {
                var servicesDict = _services.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var json = JsonSerializer.Serialize(servicesDict, new JsonSerializerOptions { WriteIndented = true });
                
                var directory = Path.GetDirectoryName(_registryFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(_registryFile, json);
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteLog($"Error saving services: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a unique key for a service based on component and version.
        /// </summary>
        /// <param name="component">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <returns>Service key string.</returns>
        private static string GetServiceKey(string component, string version)
        {
            return $"{component.ToLowerInvariant()}-{version}";
        }

        /// <summary>
        /// Checks if a process is alive and matches the expected executable path.
        /// </summary>
        /// <param name="processId">Process ID to check.</param>
        /// <param name="expectedPath">Expected executable path.</param>
        /// <returns>True if process is alive and path matches, false otherwise.</returns>
        private static bool IsProcessAlive(int processId, string expectedPath)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                using (process)
                {
                    if (process.HasExited)
                        return false;
                        
                    var actualPath = process.MainModule?.FileName;
                    return actualPath?.Equals(expectedPath, StringComparison.OrdinalIgnoreCase) == true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a process is alive (without verifying path).
        /// </summary>
        /// <param name="processId">Process ID to check.</param>
        /// <returns>True if process is alive, false otherwise.</returns>
        private static bool IsProcessAlive(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                using (process)
                {
                    return !process.HasExited;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
