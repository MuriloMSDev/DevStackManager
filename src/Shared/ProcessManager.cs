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
        /// <summary>
        /// Inicia um serviço de forma genérica e eficiente
        /// </summary>
        public static async Task<bool> StartServiceAsync(string component, string version)
        {
            try
            {
                // Verificar se já está rodando
                if (IsServiceRunning(component, version))
                {
                    DevStackConfig.WriteColoredLine($"{component} {version} já está em execução.", ConsoleColor.Yellow);
                    return true;
                }

                // Obter informações do componente
                var comp = Components.ComponentsFactory.GetComponent(component);
                if (comp == null || !comp.IsService || string.IsNullOrEmpty(comp.ServicePattern))
                {
                    DevStackConfig.WriteColoredLine($"Componente {component} não é um serviço válido.", ConsoleColor.Red);
                    return false;
                }

                var executablePath = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}", comp.ServicePattern);
                var workingDirectory = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}");

                if (!File.Exists(executablePath))
                {
                    DevStackConfig.WriteColoredLine($"{component} {version} não está instalado.", ConsoleColor.Red);
                    return false;
                }

                // Configurar argumentos específicos por componente
                var arguments = GetServiceArguments(component, version);

                // Determinar quantos processos principais iniciar baseado no MaxWorkers
                var processCount = comp.MaxWorkers ?? 1; // Padrão é 1 se não definido
                var processPids = new List<int>();

                // Iniciar os processos principais
                for (int i = 0; i < processCount; i++)
                {
                    // Pequena pausa entre cada processo para evitar conflitos
                    if (i > 0)
                    {
                        await Task.Delay(100);
                    }

                    var process = await StartProcessAsync(executablePath, arguments, workingDirectory);
                    if (process == null)
                    {
                        DevStackConfig.WriteColoredLine($"Falha ao iniciar processo {i + 1} de {component} {version}.", ConsoleColor.Red);
                        
                        // Se falhou, parar os processos já iniciados
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
                        return false;
                    }

                    processPids.Add(process.Id);
                }

                // Registrar todos os processos principais
                if (processPids.Count > 0)
                {
                    // Registrar o primeiro processo como principal (para compatibilidade)
                    ProcessRegistry.RegisterService(component, version, processPids[0], executablePath);
                    
                    // Registrar os demais como processos principais adicionais
                    for (int i = 1; i < processPids.Count; i++)
                    {
                        ProcessRegistry.AddMainProcess(component, version, processPids[i]);
                    }
                }

                var pidsText = processPids.Count == 1 
                    ? $"PID {processPids[0]}" 
                    : $"PIDs [{string.Join(", ", processPids)}]";
                    
                DevStackConfig.WriteColoredLine($"{component} {version} iniciado com {pidsText}.", ConsoleColor.Green);
                return true;
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao iniciar {component} {version}: {ex.Message}", ConsoleColor.Red);
                return false;
            }
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
                await Task.Delay(2000); // Aguardar finalização
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
                // Obter todos os PIDs registrados do serviço
                var registeredPids = ProcessRegistry.GetServicePids(component, version, null);
                
                if (registeredPids.Count == 0)
                {
                    // Fallback: usar o PID principal se não há registros
                    registeredPids.Add(mainPid);
                }
                
                foreach (var pid in registeredPids)
                {
                    try
                    {
                        var process = Process.GetProcessById(pid);
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(3000);
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // Processo já morreu ou não existe
                    }
                }
            });
        }

        /// <summary>
        /// Obtém todos os PIDs relacionados a um serviço
        /// </summary>
        private static List<int> GetAllRelatedProcessIds(string component, string version)
        {
            var pids = new List<int>();
            
            try
            {
                var comp = Components.ComponentsFactory.GetComponent(component);
                if (comp?.ServicePattern == null) return pids;

                var executablePath = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}", comp.ServicePattern);
                var processName = Path.GetFileNameWithoutExtension(comp.ServicePattern);

                // Buscar todos os processos com o mesmo nome e caminho
                var processes = Process.GetProcessesByName(processName);
                
                foreach (var process in processes)
                {
                    try
                    {
                        var processPath = process.MainModule?.FileName;
                        if (processPath?.Equals(executablePath, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            // Para PHP, verificar argumentos específicos da versão
                            if (component.Equals("php", StringComparison.OrdinalIgnoreCase))
                            {
                                if (IsPhpProcessForVersion(process.Id, version))
                                {
                                    pids.Add(process.Id);
                                }
                            }
                            else
                            {
                                pids.Add(process.Id);
                            }
                        }
                    }
                    catch
                    {
                        // Ignorar processos inacessíveis
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch
            {
                // Em caso de erro, retornar lista vazia
            }

            return pids;
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
