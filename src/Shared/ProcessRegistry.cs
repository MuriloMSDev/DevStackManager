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
        
        private const int GUID_SHORT_LENGTH = 8;

        /// <summary>
        /// Registra um processo principal de serviço
        /// </summary>
        public static void RegisterService(string component, string version, int mainProcessId, string executablePath)
        {
            RegisterServiceInternal(component, version, new List<int> { mainProcessId }, executablePath);
        }

        /// <summary>
        /// Registra múltiplos processos principais para um serviço (para componentes com MaxWorkers > 1)
        /// </summary>
        public static void RegisterServiceWithMultipleProcesses(string component, string version, List<int> mainProcessIds, string executablePath)
        {
            RegisterServiceInternal(component, version, mainProcessIds, executablePath);
        }

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

            DevStackConfig.WriteLog($"Serviço registrado: {component}-{version} {pidsText} ID={uniqueId}");
        }

        private static string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString("N")[..GUID_SHORT_LENGTH];
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
            return UpdateServicePidsAndCheckActive(component, version);
        }

        /// <summary>
        /// Obtém o PID do primeiro processo principal de um serviço (compatibilidade)
        /// </summary>
        public static int? GetMainProcessId(string component, string version)
        {
            var alivePids = GetAndUpdateAlivePids(component, version);
            return alivePids.Count > 0 ? alivePids[0] : null;
        }

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

        private static List<int> GetAlivePidsFromService(ServiceProcess service)
        {
            return service.MainProcessIds
                .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                .ToList();
        }

        private static void UpdateServicePidsIfChanged(ServiceProcess service, List<int> alivePids)
        {
            if (alivePids.Count != service.MainProcessIds.Count)
            {
                service.MainProcessIds = alivePids;
                SaveToFile();
            }
        }

        /// <summary>
        /// Obtém todos os PIDs dos processos principais de um serviço
        /// </summary>
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

        private static List<int> GetCleanedAlivePids(ServiceProcess service)
        {
            return service.MainProcessIds
                .Distinct()
                .Where(pid => IsProcessAlive(pid, service.ExecutablePath))
                .ToList();
        }

        private static List<int> ApplyMaxWorkersLimit(List<int> pids, int? maxWorkers)
        {
            if (maxWorkers.HasValue && maxWorkers.Value > 0)
            {
                return pids.Take(maxWorkers.Value).ToList();
            }
            return pids;
        }

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
                DevStackConfig.WriteLog($"PIDs atualizados para {component}-{version}: {string.Join(", ", limitedPids)}");
            }
        }

        /// <summary>
        /// Lista todos os serviços registrados ativos
        /// </summary>
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
                DevStackConfig.WriteLog($"PIDs limpos para {service.Component}-{service.Version}: {originalCount} -> {cleanPids.Count}");
                return (shouldRemove: false, hasChanges: true);
            }

            return (shouldRemove: false, hasChanges: false);
        }

        private static void RemoveServices(List<string> serviceKeys)
        {
            foreach (var key in serviceKeys)
            {
                if (_services.TryRemove(key, out var removedService))
                {
                    DevStackConfig.WriteLog($"Serviço removido (sem processos vivos): {removedService.Component}-{removedService.Version}");
                }
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
