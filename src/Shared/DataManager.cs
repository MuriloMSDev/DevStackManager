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
    public static class DataManager
    {
        /// <summary>
        /// Obtém informações sobre versões instaladas de todas as ferramentas
        /// </summary>
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
        /// Obtém status de um componente específico
        /// </summary>
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

        private static ComponentStatus CreateUnknownComponentStatus()
        {
            return new ComponentStatus
            {
                Installed = false,
                Versions = new List<string>(),
                Message = "Componente desconhecido"
            };
        }

        private static ComponentStatus CreateNotInstalledStatus(string component)
        {
            return new ComponentStatus
            {
                Installed = false,
                Versions = new List<string>(),
                Message = $"{component} não está instalado."
            };
        }

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

        private static ComponentStatus CreateInstalledStatus(string component, List<string> installedVersions)
        {
            return new ComponentStatus
            {
                Installed = true,
                Versions = installedVersions,
                Message = $"{component} instalado(s)"
            };
        }

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

        private static bool IsServiceVersionRunning(string component, Components.ComponentInterface comp, string version, System.Diagnostics.Process[] processList)
        {
            if (!component.Equals(comp.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string searchPattern = $"{component.ToLowerInvariant()}-{version}";

            return processList.Any(p => IsProcessMatchingVersion(p, comp.Name, searchPattern));
        }

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
        /// Obtém status de todos os componentes
        /// </summary>
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
