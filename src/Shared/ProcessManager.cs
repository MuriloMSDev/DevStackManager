using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Gerenciador genérico e eficiente de serviços DevStack
    /// </summary>
    public static class ProcessManager
    {
        private const int PROCESS_START_DELAY_MS = 100;
        private const int RESTART_DELAY_MS = 2000;
        private const int PROCESS_EXIT_TIMEOUT_MS = 3000;
        /// <summary>
        /// Inicia um serviço de forma genérica e eficiente
        /// </summary>
        public static async Task<bool> StartServiceAsync(string component, string version)
        {
            try
            {
                if (IsServiceRunning(component, version))
                {
                    DevStackConfig.WriteColoredLine($"{component} {version} já está em execução.", ConsoleColor.Yellow);
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
                    DevStackConfig.WriteColoredLine($"{component} {version} não está instalado.", ConsoleColor.Red);
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
                DevStackConfig.WriteColoredLine($"Erro ao iniciar {component} {version}: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        private static bool ValidateServiceComponent(Components.ComponentInterface? comp, string component)
        {
            if (comp == null || !comp.IsService || string.IsNullOrEmpty(comp.ServicePattern))
            {
                DevStackConfig.WriteColoredLine($"Componente {component} não é um serviço válido.", ConsoleColor.Red);
                return false;
            }
            return true;
        }

        private static (string executablePath, string workingDirectory) GetServicePaths(
            Components.ComponentInterface comp, 
            string component, 
            string version)
        {
            var executablePath = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}", comp.ServicePattern!);
            var workingDirectory = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}");
            return (executablePath, workingDirectory);
        }

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
                    DevStackConfig.WriteColoredLine($"Falha ao iniciar processo {i + 1} de {component} {version}.", ConsoleColor.Red);
                    await KillStartedProcesses(processPids);
                    return new List<int>();
                }

                processPids.Add(process.Id);
            }

            return processPids;
        }

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

        private static void PrintServiceStartedMessage(string component, string version, List<int> processPids)
        {
            var pidsText = processPids.Count == 1
                ? $"PID {processPids[0]}"
                : $"PIDs [{string.Join(", ", processPids)}]";

            DevStackConfig.WriteColoredLine($"{component} {version} iniciado com {pidsText}.", ConsoleColor.Green);
        }

        /// <summary>
        /// Para um serviço de forma genérica e eficiente
        /// </summary>
        public static async Task<bool> StopServiceAsync(string component, string version)
        {
            try
            {
                // Verificar se está rodando
                if (!IsServiceRunning(component, version))
                {
                    DevStackConfig.WriteColoredLine($"{component} {version} não está em execução.", ConsoleColor.Yellow);
                    ProcessRegistry.UnregisterService(component, version); // Limpar registro
                    return true;
                }

                // Obter PID do processo principal
                var mainPid = ProcessRegistry.GetMainProcessId(component, version);
                if (mainPid == null)
                {
                    DevStackConfig.WriteColoredLine($"Processo principal de {component} {version} não encontrado.", ConsoleColor.Yellow);
                    return true;
                }

                // Para todos os processos relacionados
                await StopRelatedProcessesAsync(component, version, mainPid.Value);

                // Remover do registro
                ProcessRegistry.UnregisterService(component, version);

                DevStackConfig.WriteColoredLine($"{component} {version} parado.", ConsoleColor.Green);
                return true;
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao parar {component} {version}: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        /// <summary>
        /// Reinicia um serviço
        /// </summary>
        public static async Task<bool> RestartServiceAsync(string component, string version)
        {
            DevStackConfig.WriteColoredLine($"Reiniciando {component} {version}...", ConsoleColor.Cyan);

            if (IsServiceRunning(component, version))
            {
                await StopServiceAsync(component, version);
                await Task.Delay(RESTART_DELAY_MS);
            }

            return await StartServiceAsync(component, version);
        }

        /// <summary>
        /// Verifica se um serviço está rodando
        /// </summary>
        public static bool IsServiceRunning(string component, string version)
        {
            return ProcessRegistry.IsServiceActive(component, version);
        }

        /// <summary>
        /// Lista o status de todos os serviços
        /// </summary>
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
        /// Para todos os serviços em execução
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
        /// Inicia todos os serviços instalados
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
        /// Versões síncronas para compatibilidade
        /// </summary>
        public static bool StartService(string component, string version) => 
            StartServiceAsync(component, version).GetAwaiter().GetResult();

        public static bool StopService(string component, string version) => 
            StopServiceAsync(component, version).GetAwaiter().GetResult();

        public static bool RestartService(string component, string version) => 
            RestartServiceAsync(component, version).GetAwaiter().GetResult();

        public static void StopAllServices() => 
            StopAllServicesAsync().GetAwaiter().GetResult();

        public static void StartAllServices() => 
            StartAllServicesAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Métodos de compatibilidade com nomes antigos
        /// </summary>
        public static async Task<bool> StartComponentAsync(string component, string version) => 
            await StartServiceAsync(component, version);

        public static async Task<bool> StopComponentAsync(string component, string version) => 
            await StopServiceAsync(component, version);

        public static async Task RestartComponentAsync(string component, string version) => 
            await RestartServiceAsync(component, version);

        public static bool IsComponentRunning(string component, string version) => 
            IsServiceRunning(component, version);

        public static void StartComponent(string component, string version) => 
            StartService(component, version);

        public static void StopComponent(string component, string version) => 
            StopService(component, version);

        public static void RestartComponent(string component, string version) => 
            RestartService(component, version);

        public static async Task ListComponentsStatusAsync()
        {
            var services = await GetAllServicesStatusAsync();
            
            DevStackConfig.WriteColoredLine("Status dos serviços:", ConsoleColor.Cyan);
            Console.WriteLine();

            foreach (var group in services.GroupBy(s => s.Component))
            {
                DevStackConfig.WriteColoredLine($"{group.Key.ToUpper()}:", ConsoleColor.Yellow);
                
                foreach (var service in group)
                {
                    var status = service.IsRunning ? "EM EXECUÇÃO" : "PARADO";
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

        public static void ListComponentsStatus() => 
            ListComponentsStatusAsync().GetAwaiter().GetResult();

        // ================= MÉTODOS PRIVADOS =================

        /// <summary>
        /// Inicia um processo de forma assíncrona
        /// </summary>
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
        /// Para todos os processos relacionados a um serviço
        /// </summary>
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

        private static List<int> GetRegisteredPidsOrFallback(string component, string version, int mainPid)
        {
            var registeredPids = ProcessRegistry.GetServicePids(component, version, null);

            if (registeredPids.Count == 0)
            {
                registeredPids.Add(mainPid);
            }

            return registeredPids;
        }

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
                // Processo já morreu ou não existe
            }
        }

        /// <summary>
        /// Obtém todos os PIDs relacionados a um serviço
        /// </summary>
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
        /// Verifica se um processo PHP é da versão específica
        /// </summary>
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
                // Se não conseguir verificar, assumir que é da versão
            }
            
            return true;
        }

        /// <summary>
        /// Obtém argumentos específicos para cada componente
        /// </summary>
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
        /// Executa um comando no terminal e retorna a saída (compatibilidade)
        /// </summary>
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
        /// Versão síncrona do ExecuteProcessAsync
        /// </summary>
        public static string ExecuteProcess(string fileName, string arguments, string? workingDirectory = null, ProcessWindowStyle? windowStyle = null)
        {
            return ExecuteProcessAsync(fileName, arguments, workingDirectory, windowStyle).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Métodos adicionais para compatibilidade
        /// </summary>
        public static void ForEachVersion(string component, Action<string> action)
        {
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp == null)
            {
                DevStackConfig.WriteColoredLine($"Componente desconhecido: {component}", ConsoleColor.Red);
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
                DevStackConfig.WriteColoredLine($"Erro ao listar versões de {component}: {ex.Message}", ConsoleColor.Red);
            }
        }

        public static async Task StopAllComponentsAsync() => await StopAllServicesAsync();
        public static void StopAllComponents() => StopAllServices();
        public static async Task StartAllComponentsAsync() => await StartAllServicesAsync();
        public static void StartAllComponents() => StartAllServices();
    }

    /// <summary>
    /// Status de um serviço
    /// </summary>
    public class ServiceStatus
    {
        public string Component { get; set; } = "";
        public string Version { get; set; } = "";
        public bool IsRunning { get; set; }
        public int? MainProcessId { get; set; }
        public List<int> AllProcessIds { get; set; } = new();
    }
}
