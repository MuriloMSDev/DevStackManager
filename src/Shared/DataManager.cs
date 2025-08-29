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
                return new ComponentStatus
                {
                    Installed = false,
                    Versions = new List<string>(),
                    Message = "Componente desconhecido"
                };
            }

            var installedVersions = comp.ListInstalled();
            if (installedVersions.Count == 0)
            {
                return new ComponentStatus
                {
                    Installed = false,
                    Versions = new List<string>(),
                    Message = $"{component} não está instalado."
                };
            }

            if (comp != null && comp.IsService)
            {
                var processList = System.Diagnostics.Process.GetProcesses();
                var runningList = new Dictionary<string, bool>();
                foreach (var version in installedVersions)
                {
                    string search = $"{component.ToLowerInvariant()}-{version}";
                    bool running = false;
                    if (component.Equals(comp.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        running = processList.Any(p => {
                            try
                            {
                                if (p.ProcessName.StartsWith(comp.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    var processPath = p.MainModule?.FileName;
                                    return !string.IsNullOrEmpty(processPath) && processPath.Contains(search, StringComparison.OrdinalIgnoreCase);
                                }
                            }
                            catch { }
                            return false;
                        });
                    }
                    runningList[version] = running;
                }
                return new ComponentStatus
                {
                    Installed = true,
                    Versions = installedVersions,
                    Message = $"{component} instalado(s)",
                    RunningList = runningList
                };
            }

            return new ComponentStatus
            {
                Installed = true,
                Versions = installedVersions,
                Message = $"{component} instalado(s)"
            };
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
