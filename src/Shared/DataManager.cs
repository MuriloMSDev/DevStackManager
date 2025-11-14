using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Manages data operations related to component installation status and version information.
    /// Provides functionality to query installed versions, component status, and running services.
    /// </summary>
    public static class DataManager
    {
        /// <summary>
        /// Gets comprehensive installation information for all configured components.
        /// Includes installation status, executability, and installed versions for each component.
        /// </summary>
        /// <returns>InstallationData object containing status and component information for all tools.</returns>
        public static InstallationData GetInstalledVersions()
        {
            var result = new List<ComponentInfo>();

            foreach (var name in DevStackConfig.components)
            {
                var comp = Components.ComponentsFactory.GetComponent(name);
                var installedVersions = comp?.ListInstalled() ?? new List<string>();
                var item = new ComponentInfo
                {
                    Name = name,
                    Installed = installedVersions.Count > 0,
                    IsExecutable = comp?.IsExecutable ?? false,
                    Versions = installedVersions
                };
                result.Add(item);
            }

            return new InstallationData
            {
                Status = "success",
                Message = "",
                Components = result
            };
        }

        /// <summary>
        /// Gets the detailed status of a specific component including installation state and running services.
        /// For service components, includes running/stopped status for each version.
        /// </summary>
        /// <param name="component">The name of the component to check status for.</param>
        /// <returns>ComponentStatus object with installation details and service status information.</returns>
        public static ComponentStatus GetComponentStatus(string component)
        {
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp == null)
            {
                return CreateUnknownComponentStatus();
            }

            var installedVersions = comp.ListInstalled();
            if (installedVersions.Count == 0)
            {
                return CreateNotInstalledStatus(component);
            }

            if (comp.IsService)
            {
                return CreateServiceStatus(component, comp, installedVersions);
            }

            return CreateInstalledStatus(component, installedVersions);
        }

        /// <summary>
        /// Creates a status object indicating the component is unknown or not supported.
        /// </summary>
        /// <returns>ComponentStatus with installed set to false and unknown component message.</returns>
        private static ComponentStatus CreateUnknownComponentStatus()
        {
            return new ComponentStatus
            {
                Installed = false,
                Versions = new List<string>(),
                Message = "Componente desconhecido"
            };
        }

        /// <summary>
        /// Creates a status object indicating the component is not installed.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <returns>ComponentStatus with installed set to false and not installed message.</returns>
        private static ComponentStatus CreateNotInstalledStatus(string component)
        {
            return new ComponentStatus
            {
                Installed = false,
                Versions = new List<string>(),
                Message = $"{component} não está instalado."
            };
        }

        /// <summary>
        /// Creates a status object for service components with running/stopped information for each version.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="comp">The component interface instance.</param>
        /// <param name="installedVersions">List of installed versions.</param>
        /// <returns>ComponentStatus with service running status for each version.</returns>
        private static ComponentStatus CreateServiceStatus(string component, Components.ComponentInterface comp, List<string> installedVersions)
        {
            var runningList = GetRunningServiceVersions(component, comp, installedVersions);
            
            return new ComponentStatus
            {
                Installed = true,
                Versions = installedVersions,
                Message = $"{component} instalado(s)",
                RunningList = runningList
            };
        }

        /// <summary>
        /// Creates a status object for non-service components with simple installation information.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="installedVersions">List of installed versions.</param>
        /// <returns>ComponentStatus with basic installation information.</returns>
        private static ComponentStatus CreateInstalledStatus(string component, List<string> installedVersions)
        {
            return new ComponentStatus
            {
                Installed = true,
                Versions = installedVersions,
                Message = $"{component} instalado(s)"
            };
        }

        /// <summary>
        /// Determines the running status for each installed version of a service component.
        /// Checks running processes to see if each version is currently active.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="comp">The component interface instance.</param>
        /// <param name="installedVersions">List of installed versions to check.</param>
        /// <returns>Dictionary mapping each version to its running status (true if running, false if stopped).</returns>
        private static Dictionary<string, bool> GetRunningServiceVersions(string component, Components.ComponentInterface comp, List<string> installedVersions)
        {
            var processList = System.Diagnostics.Process.GetProcesses();
            var runningList = new Dictionary<string, bool>();

            foreach (var version in installedVersions)
            {
                bool running = IsServiceVersionRunning(component, comp, version, processList);
                runningList[version] = running;
            }

            return runningList;
        }

        /// <summary>
        /// Checks if a specific version of a service component is currently running.
        /// Examines running processes to match the component name and version.
        /// </summary>
        /// <param name="component">The component name.</param>
        /// <param name="comp">The component interface instance.</param>
        /// <param name="version">The specific version to check.</param>
        /// <param name="processList">Array of currently running processes.</param>
        /// <returns>True if the specified version is running, false otherwise.</returns>
        private static bool IsServiceVersionRunning(string component, Components.ComponentInterface comp, string version, System.Diagnostics.Process[] processList)
        {
            if (!component.Equals(comp.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string searchPattern = $"{component.ToLowerInvariant()}-{version}";

            return processList.Any(p => IsProcessMatchingVersion(p, comp.Name, searchPattern));
        }

        /// <summary>
        /// Determines if a running process matches the specified component and version pattern.
        /// Compares process name and executable path to identify matching instances.
        /// </summary>
        /// <param name="process">The process to check.</param>
        /// <param name="componentName">The component name to match.</param>
        /// <param name="searchPattern">The version-specific search pattern.</param>
        /// <returns>True if the process matches the component and version, false otherwise.</returns>
        private static bool IsProcessMatchingVersion(System.Diagnostics.Process process, string componentName, string searchPattern)
        {
            try
            {
                if (!process.ProcessName.StartsWith(componentName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var processPath = process.MainModule?.FileName;
                return !string.IsNullOrEmpty(processPath) && 
                       processPath.Contains(searchPattern, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets installation and running status for all supported DevStack components.
        /// Provides a comprehensive view of the entire development stack.
        /// </summary>
        /// <returns>Dictionary mapping component names to their complete status information.</returns>
        public static Dictionary<string, ComponentStatus> GetAllComponentsStatus()
        {
            var results = new Dictionary<string, ComponentStatus>();
            foreach (var comp in DevStackConfig.components)
            {
                results[comp] = GetComponentStatus(comp);
            }

            return results;
        }
    }
}
