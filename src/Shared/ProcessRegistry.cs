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
    /// Informações sobre um processo principal registrado
    /// </summary>
    public class ServiceProcess
    {
        public List<int> MainProcessIds { get; set; } = new(); // Lista de PIDs dos processos principais
        public string Component { get; set; } = "";
        public string Version { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public DateTime StartTime { get; set; }
        public string UniqueId { get; set; } = "";
        
        // Propriedade de compatibilidade (obsoleta)
        public int MainProcessId 
        { 
            get => MainProcessIds.Count > 0 ? MainProcessIds[0] : 0;
            set => MainProcessIds = new List<int> { value };
        }
    }

    /// <summary>
    /// Registro simples e eficiente de processos principais dos serviços
    /// </summary>
    public static class ProcessRegistry
    {
        private static readonly ConcurrentDictionary<string, ServiceProcess> _services = new();
        private static readonly string _registryFile = Path.Combine(DevStackConfig.tmpDir, "devstack_services.json");

        /// <summary>
        /// Registra um processo principal de serviço
        /// </summary>
        public static void RegisterService(string component, string version, int mainProcessId, string executablePath)
        {
            var key = GetServiceKey(component, version);
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            
            var service = new ServiceProcess
            {
                MainProcessIds = new List<int> { mainProcessId },
                Component = component,
                Version = version,
                ExecutablePath = executablePath,
                StartTime = DateTime.Now,
                UniqueId = uniqueId
            };

            _services[key] = service;
            SaveToFile();
            
            DevStackConfig.WriteLog($"Serviço registrado: {component}-{version} PID={mainProcessId} ID={uniqueId}");
        }

        /// <summary>
        /// Registra múltiplos processos principais para um serviço (para componentes com MaxWorkers > 1)
        /// </summary>
        public static void RegisterServiceWithMultipleProcesses(string component, string version, List<int> mainProcessIds, string executablePath)
        {
            var key = GetServiceKey(component, version);
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            
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
            
            DevStackConfig.WriteLog($"Serviço registrado: {component}-{version} PIDs=[{string.Join(", ", mainProcessIds)}] ID={uniqueId}");
        }

        /// <summary>
        /// Adiciona um processo principal adicional para um serviço existente
        /// </summary>
        public static void AddMainProcess(string component, string version, int processId)
        {
            var key = GetServiceKey(component, version);
            
            if (_services.TryGetValue(key, out var service))
            {
                if (!service.MainProcessIds.Contains(processId))
                {
                    // Verificar se o processo está realmente vivo antes de adicionar
                    if (IsProcessAlive(processId))
                    {
                        service.MainProcessIds.Add(processId);
                        SaveToFile();
                        DevStackConfig.WriteLog($"Processo principal adicionado: {component}-{version} PID={processId}");
                    }
                }
            }
        }

        /// <summary>
        /// Remove o registro de um serviço
        /// </summary>
        public static void UnregisterService(string component, string version)
        {
            var key = GetServiceKey(component, version);
            if (_services.TryRemove(key, out var service))
            {
                SaveToFile();
                DevStackConfig.WriteLog($"Serviço removido: {component}-{version} PID={service.MainProcessId}");
            }
        }

        /// <summary>
        /// Limpa PIDs duplicados e mortos de um serviço específico
        /// </summary>
        public static void CleanupServicePids(string component, string version)
        {
            var key = GetServiceKey(component, version);
            
            if (_services.TryGetValue(key, out var service))
            {
                var originalCount = service.MainProcessIds.Count;
                
                // Manter apenas PIDs únicos e vivos
                var cleanPids = service.MainProcessIds
                    .Distinct()
                    .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                    .ToList();
                
                if (cleanPids.Count != originalCount)
                {
                    service.MainProcessIds = cleanPids;
                    SaveToFile();
                    
                    DevStackConfig.WriteLog($"PIDs limpos para {component}-{version}: {originalCount} -> {cleanPids.Count} PIDs");
                    
                    if (cleanPids.Count == 0)
                    {
                        UnregisterService(component, version);
                    }
                }
            }
        }

        /// <summary>
        /// Verifica se um serviço está ativo (pelo menos um processo principal existe)
        /// </summary>
        public static bool IsServiceActive(string component, string version)
        {
            var key = GetServiceKey(component, version);
            
            if (_services.TryGetValue(key, out var service))
            {
                // Verificar se pelo menos um processo principal está vivo
                var alivePids = new List<int>();
                foreach (var pid in service.MainProcessIds)
                {
                    if (IsProcessAlive(pid, service.ExecutablePath))
                    {
                        alivePids.Add(pid);
                    }
                }
                
                if (alivePids.Count > 0)
                {
                    // Atualizar lista apenas com processos vivos
                    if (alivePids.Count != service.MainProcessIds.Count)
                    {
                        service.MainProcessIds = alivePids;
                        SaveToFile();
                    }
                    return true;
                }
                else
                {
                    // Todos os processos morreram, remover registro
                    UnregisterService(component, version);
                    return false;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Obtém o PID do primeiro processo principal de um serviço (compatibilidade)
        /// </summary>
        public static int? GetMainProcessId(string component, string version)
        {
            var key = GetServiceKey(component, version);
            
            if (_services.TryGetValue(key, out var service))
            {
                var alivePids = new List<int>();
                foreach (var pid in service.MainProcessIds)
                {
                    if (IsProcessAlive(pid, service.ExecutablePath))
                    {
                        alivePids.Add(pid);
                    }
                }
                
                if (alivePids.Count > 0)
                {
                    // Atualizar lista apenas com processos vivos
                    if (alivePids.Count != service.MainProcessIds.Count)
                    {
                        service.MainProcessIds = alivePids;
                        SaveToFile();
                    }
                    return alivePids[0];
                }
                else
                {
                    UnregisterService(component, version);
                }
            }
            
            return null;
        }

        /// <summary>
        /// Obtém todos os PIDs dos processos principais de um serviço
        /// </summary>
        public static List<int> GetServicePids(string component, string version, int? maxWorkers)
        {
            var key = GetServiceKey(component, version);
            var resultPids = new List<int>();
            
            if (_services.TryGetValue(key, out var service))
            {
                // Primeiro, limpar PIDs duplicados e mortos
                var alivePids = service.MainProcessIds
                    .Distinct()
                    .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                    .ToList();
                
                if (alivePids.Count > 0)
                {
                    // Aplicar limitação de MaxWorkers se especificado
                    if (maxWorkers.HasValue && maxWorkers.Value > 0)
                    {
                        alivePids = alivePids.Take(maxWorkers.Value).ToList();
                    }
                    
                    // Atualizar lista apenas se houver mudanças
                    if (alivePids.Count != service.MainProcessIds.Count || 
                        !alivePids.SequenceEqual(service.MainProcessIds))
                    {
                        service.MainProcessIds = alivePids;
                        SaveToFile();
                        DevStackConfig.WriteLog($"PIDs atualizados para {component}-{version}: {string.Join(", ", alivePids)}");
                    }
                    
                    resultPids.AddRange(alivePids);
                }
                else
                {
                    // Todos os processos morreram, remover registro
                    UnregisterService(component, version);
                }
            }
            
            return resultPids;
        }

        /// <summary>
        /// Lista todos os serviços registrados ativos
        /// </summary>
        public static List<ServiceProcess> GetActiveServices()
        {
            var activeServices = new List<ServiceProcess>();
            var toRemove = new List<string>();

            foreach (var kvp in _services)
            {
                var alivePids = new List<int>();
                foreach (var pid in kvp.Value.MainProcessIds)
                {
                    if (IsProcessAlive(pid, kvp.Value.ExecutablePath))
                    {
                        alivePids.Add(pid);
                    }
                }

                if (alivePids.Count > 0)
                {
                    // Atualizar a lista de PIDs vivos
                    kvp.Value.MainProcessIds = alivePids;
                    activeServices.Add(kvp.Value);
                }
                else
                {
                    toRemove.Add(kvp.Key);
                }
            }

            // Limpar serviços mortos
            foreach (var key in toRemove)
            {
                _services.TryRemove(key, out _);
            }

            if (toRemove.Any())
            {
                SaveToFile();
            }

            return activeServices;
        }

        /// <summary>
        /// Descobrir e atualizar automaticamente todos os processos principais de um serviço
        /// </summary>
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

                // Limpar PIDs mortos primeiro
                var alivePids = service.MainProcessIds.Where(pid => IsProcessAlive(pid, service.ExecutablePath)).ToList();
                
                // Encontrar novos PIDs que não estão na lista atual
                var newPids = foundPids.Where(pid => !alivePids.Contains(pid)).ToList();
                
                // Atualizar apenas se houver mudanças
                if (alivePids.Count != service.MainProcessIds.Count || newPids.Count > 0)
                {
                    // Combinar PIDs vivos existentes + novos PIDs descobertos
                    var allValidPids = alivePids.Concat(newPids).Distinct().ToList();
                    
                    // Aplicar limitação de MaxWorkers se especificado
                    if (maxWorkers.HasValue && maxWorkers.Value > 0)
                    {
                        allValidPids = allValidPids.Take(maxWorkers.Value).ToList();
                    }
                    
                    service.MainProcessIds = allValidPids;
                    SaveToFile();
                    
                    if (newPids.Count > 0)
                    {
                        DevStackConfig.WriteLog($"Processos principais descobertos para {component}-{version}: {string.Join(", ", newPids)}");
                    }
                    
                    if (alivePids.Count != service.MainProcessIds.Count)
                    {
                        DevStackConfig.WriteLog($"PIDs limpos para {component}-{version}. PIDs ativos: {string.Join(", ", allValidPids)}");
                    }
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteLog($"Erro ao descobrir processos principais para {component}-{version}: {ex.Message}");
            }
        }

        /// <summary>
        /// Carrega os serviços do arquivo
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
                        
                        // Limpar PIDs mortos após carregar
                        CleanupAllServices();
                    }
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteLog($"Erro ao carregar serviços: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpa PIDs duplicados e mortos de todos os serviços registrados
        /// </summary>
        public static void CleanupAllServices()
        {
            var toRemove = new List<string>();
            bool hasChanges = false;
            
            foreach (var kvp in _services)
            {
                var service = kvp.Value;
                var originalCount = service.MainProcessIds.Count;
                
                // Remover duplicatas e PIDs mortos
                var cleanPids = service.MainProcessIds
                    .Distinct()
                    .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                    .ToList();
                
                if (cleanPids.Count == 0)
                {
                    toRemove.Add(kvp.Key);
                    hasChanges = true;
                }
                else if (cleanPids.Count != originalCount)
                {
                    service.MainProcessIds = cleanPids;
                    hasChanges = true;
                    DevStackConfig.WriteLog($"PIDs limpos para {service.Component}-{service.Version}: {originalCount} -> {cleanPids.Count}");
                }
            }
            
            // Remover serviços sem processos vivos
            foreach (var key in toRemove)
            {
                if (_services.TryRemove(key, out var removedService))
                {
                    DevStackConfig.WriteLog($"Serviço removido (sem processos vivos): {removedService.Component}-{removedService.Version}");
                }
            }
            
            if (hasChanges)
            {
                SaveToFile();
            }
        }

        /// <summary>
        /// Salva os serviços no arquivo
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
                DevStackConfig.WriteLog($"Erro ao salvar serviços: {ex.Message}");
            }
        }

        /// <summary>
        /// Gera chave única para o serviço
        /// </summary>
        private static string GetServiceKey(string component, string version)
        {
            return $"{component.ToLowerInvariant()}-{version}";
        }

        /// <summary>
        /// Verifica se um processo está vivo e é o esperado
        /// </summary>
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
        /// Verifica se um processo está vivo (sem verificar caminho)
        /// </summary>
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
