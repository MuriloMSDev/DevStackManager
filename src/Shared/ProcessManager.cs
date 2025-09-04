using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DevStackManager
{
    public static class ProcessManager
    {
        /// <summary>
        /// Executa um comando no terminal e retorna a saída
        /// </summary>
        public static async Task<string> ExecuteProcessAsync(string fileName, string arguments, string? workingDirectory = null, System.Diagnostics.ProcessWindowStyle? windowStyle = null)
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

            var output = string.Empty;
            var error = string.Empty;

            var outputTcs = new TaskCompletionSource<string>();
            var errorTcs = new TaskCompletionSource<string>();

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null)
                    outputTcs.TrySetResult(output);
                else
                    output += e.Data + Environment.NewLine;
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null)
                    errorTcs.TrySetResult(error);
                else
                    error += e.Data + Environment.NewLine;
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.WhenAll(
                Task.Run(() => process.WaitForExit()),
                outputTcs.Task,
                errorTcs.Task
            );

            return output.Trim();
        }

        // Legacy sync version for compatibility
        public static string ExecuteProcess(string fileName, string arguments, string? workingDirectory = null, System.Diagnostics.ProcessWindowStyle? windowStyle = null)
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
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Trim();
        }

        /// <summary>
        /// Inicia um componente específico com uma versão
        /// </summary>
        public static void StartComponent(string component, string version)
        {
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp == null || !comp.IsService)
            {
                DevStackConfig.WriteColoredLine($"Componente desconhecido ou não é um serviço: {component}", ConsoleColor.Red);
                return;
            }

            switch (component.ToLowerInvariant())
            {
                case "nginx":
                    StartNginx(version);
                    break;
                case "php":
                    StartPhp(version);
                    break;
                default:
                    StartGenericService(comp, version);
                    break;
            }
        }

        /// <summary>
        /// Para um componente específico com uma versão
        /// </summary>
        public static void StopComponent(string component, string version)
        {
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp == null || !comp.IsService)
            {
                DevStackConfig.WriteColoredLine($"Componente desconhecido ou não é um serviço: {component}", ConsoleColor.Red);
                return;
            }

            switch (component.ToLowerInvariant())
            {
                case "nginx":
                    StopNginx(version);
                    break;
                case "php":
                    StopPhp(version);
                    break;
                default:
                    StopGenericService(comp, version);
                    break;
            }
        }

        /// <summary>
        /// Inicia o Nginx de uma versão específica
        /// </summary>
        private static void StartNginx(string version)
        {
            var nginxExe = Path.Combine(DevStackConfig.nginxDir, $"nginx-{version}", $"nginx.exe");
            var nginxWorkDir = Path.Combine(DevStackConfig.nginxDir, $"nginx-{version}");

            if (!File.Exists(nginxExe))
            {
                DevStackConfig.WriteColoredLine($"Nginx {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(nginxExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    DevStackConfig.WriteColoredLine($"Nginx {version} já está em execução.", ConsoleColor.Yellow);
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = nginxExe,
                    WorkingDirectory = nginxWorkDir,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(startInfo);
                DevStackConfig.WriteColoredLine($"Nginx {version} iniciado.", ConsoleColor.Green);
                WriteLog($"Nginx {version} iniciado.");
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao iniciar Nginx {version}: {ex.Message}", ConsoleColor.Red);
                WriteLog($"Erro ao iniciar Nginx {version}: {ex.Message}");
            }
        }

        /// <summary>
        /// Para o Nginx de uma versão específica
        /// </summary>
        private static void StopNginx(string version)
        {
            var nginxExe = Path.Combine(DevStackConfig.nginxDir, $"nginx-{version}", $"nginx.exe");

            if (!File.Exists(nginxExe))
            {
                DevStackConfig.WriteColoredLine($"Nginx {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(nginxExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    foreach (var process in runningProcesses)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // Aguarda até 5 segundos
                        }
                        catch (Exception ex)
                        {
                            DevStackConfig.WriteColoredLine($"Erro ao parar processo Nginx {process.Id}: {ex.Message}", ConsoleColor.Yellow);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                    DevStackConfig.WriteColoredLine($"Nginx {version} parado.", ConsoleColor.Green);
                    WriteLog($"Nginx {version} parado.");
                }
                else
                {
                    DevStackConfig.WriteColoredLine($"Nginx {version} não está em execução.", ConsoleColor.Yellow);
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao verificar processos Nginx: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Inicia o PHP-CGI de uma versão específica
        /// </summary>
        private static void StartPhp(string version)
        {
            var phpExe = Path.Combine(DevStackConfig.phpDir, $"php-{version}", $"php-cgi.exe");
            var phpWorkDir = Path.Combine(DevStackConfig.phpDir, $"php-{version}");

            if (!File.Exists(phpExe))
            {
                DevStackConfig.WriteColoredLine($"php-cgi {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(phpExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    DevStackConfig.WriteColoredLine($"php-cgi {version} já está em execução.", ConsoleColor.Yellow);
                    return;
                }

                // Inicia 6 workers php-cgi para FastCGI
                for (int i = 1; i <= 6; i++)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = phpExe,
                        Arguments = $"-b 127.{version}:9000",
                        WorkingDirectory = phpWorkDir,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process.Start(startInfo);
                }

                DevStackConfig.WriteColoredLine($"php-cgi {version} iniciado com 6 workers em 127.{version}:9000.", ConsoleColor.Green);
                WriteLog($"php-cgi {version} iniciado com 6 workers em 127.{version}:9000.");
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao iniciar php-cgi {version}: {ex.Message}", ConsoleColor.Red);
                WriteLog($"Erro ao iniciar php-cgi {version}: {ex.Message}");
            }
        }

        /// <summary>
        /// Para o PHP-CGI de uma versão específica
        /// </summary>
        private static void StopPhp(string version)
        {
            var phpExe = Path.Combine(DevStackConfig.phpDir, $"php-{version}", $"php-cgi.exe");

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(phpExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    foreach (var process in runningProcesses)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // Aguarda até 5 segundos
                        }
                        catch (Exception ex)
                        {
                            DevStackConfig.WriteColoredLine($"Erro ao parar processo PHP {process.Id}: {ex.Message}", ConsoleColor.Yellow);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                    DevStackConfig.WriteColoredLine($"php-cgi {version} parado.", ConsoleColor.Green);
                    WriteLog($"php-cgi {version} parado.");
                }
                else
                {
                    DevStackConfig.WriteColoredLine($"php-cgi {version} não está em execução.", ConsoleColor.Yellow);
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao verificar processos PHP: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Inicia um serviço genérico
        /// </summary>
        private static void StartGenericService(Components.ComponentInterface component, string version)
        {
            if (string.IsNullOrEmpty(component.ServicePattern))
            {
                DevStackConfig.WriteColoredLine($"Padrão de serviço não definido para {component.Name}.", ConsoleColor.Red);
                return;
            }

            var serviceExe = Path.Combine(component.ToolDir, $"{component.Name}-{version}", component.ServicePattern);
            var workDir = Path.Combine(component.ToolDir, $"{component.Name}-{version}");

            if (!File.Exists(serviceExe))
            {
                DevStackConfig.WriteColoredLine($"{component.Name} {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(serviceExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    DevStackConfig.WriteColoredLine($"{component.Name} {version} já está em execução.", ConsoleColor.Yellow);
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = serviceExe,
                    WorkingDirectory = workDir,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(startInfo);
                DevStackConfig.WriteColoredLine($"{component.Name} {version} iniciado.", ConsoleColor.Green);
                WriteLog($"{component.Name} {version} iniciado.");
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao iniciar {component.Name} {version}: {ex.Message}", ConsoleColor.Red);
                WriteLog($"Erro ao iniciar {component.Name} {version}: {ex.Message}");
            }
        }

        /// <summary>
        /// Para um serviço genérico
        /// </summary>
        private static void StopGenericService(Components.ComponentInterface component, string version)
        {
            if (string.IsNullOrEmpty(component.ServicePattern))
            {
                DevStackConfig.WriteColoredLine($"Padrão de serviço não definido para {component.Name}.", ConsoleColor.Red);
                return;
            }

            var serviceExe = Path.Combine(component.ToolDir, $"{component.Name}-{version}", component.ServicePattern);

            if (!File.Exists(serviceExe))
            {
                DevStackConfig.WriteColoredLine($"{component.Name} {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(serviceExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    foreach (var process in runningProcesses)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // Aguarda até 5 segundos
                        }
                        catch (Exception ex)
                        {
                            DevStackConfig.WriteColoredLine($"Erro ao parar processo {component.Name} {process.Id}: {ex.Message}", ConsoleColor.Yellow);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                    DevStackConfig.WriteColoredLine($"{component.Name} {version} parado.", ConsoleColor.Green);
                    WriteLog($"{component.Name} {version} parado.");
                }
                else
                {
                    DevStackConfig.WriteColoredLine($"{component.Name} {version} não está em execução.", ConsoleColor.Yellow);
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao verificar processos {component.Name}: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Executa uma ação para cada versão de um componente
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

        /// <summary>
        /// Verifica se um componente específico está em execução
        /// </summary>
        public static bool IsComponentRunning(string component, string version)
        {
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp == null || !comp.IsService || string.IsNullOrEmpty(comp.ServicePattern))
            {
                return false;
            }

            var exePath = Path.Combine(comp.ToolDir, $"{comp.Name}-{version}", comp.ServicePattern);

            if (!File.Exists(exePath))
            {
                return false;
            }

            try
            {
                return Process.GetProcesses()
                    .Any(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(exePath, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lista o status de todos os componentes e versões
        /// </summary>
        public static void ListComponentsStatus()
        {
            var serviceComponents = Components.ComponentsFactory.GetAll()
                .Where(c => c.IsService)
                .ToList();
            
            DevStackConfig.WriteColoredLine("Status dos componentes:", ConsoleColor.Cyan);
            Console.WriteLine();
            
            foreach (var component in serviceComponents)
            {
                DevStackConfig.WriteColoredLine($"{component.Name.ToUpper()}:", ConsoleColor.Yellow);
                var versions = component.ListInstalled();
                if (versions.Any())
                {
                    foreach (var version in versions)
                    {
                        var isRunning = IsComponentRunning(component.Name, version);
                        var status = isRunning ? "EXECUTANDO" : "PARADO";
                        var color = isRunning ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.Write($"  {component.Name}-{version}: ");
                        DevStackConfig.WriteColoredLine(status, color);
                    }
                }
                else
                {
                    DevStackConfig.WriteColoredLine($"  Nenhuma versão de {component.Name} instalada.", ConsoleColor.Gray);
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Para todos os componentes em execução
        /// </summary>
        public static void StopAllComponents()
        {
            var serviceComponents = Components.ComponentsFactory.GetAll()
                .Where(c => c.IsService)
                .ToList();
            
            foreach (var component in serviceComponents)
            {
                var versions = component.ListInstalled();
                foreach (var version in versions)
                {
                    if (IsComponentRunning(component.Name, version))
                    {
                        StopComponent(component.Name, version);
                    }
                }
            }
        }

        /// <summary>
        /// Inicia todos os componentes instalados
        /// </summary>
        public static void StartAllComponents()
        {
            var serviceComponents = Components.ComponentsFactory.GetAll()
                .Where(c => c.IsService)
                .ToList();
            
            foreach (var component in serviceComponents)
            {
                var versions = component.ListInstalled();
                foreach (var version in versions)
                {
                    if (!IsComponentRunning(component.Name, version))
                    {
                        StartComponent(component.Name, version);
                    }
                }
            }
        }

        /// <summary>
        /// Reinicia um componente específico
        /// </summary>
        public static void RestartComponent(string component, string version)
        {
            DevStackConfig.WriteColoredLine($"Reiniciando {component} {version}...", ConsoleColor.Cyan);
            
            if (IsComponentRunning(component, version))
            {
                StopComponent(component, version);
                
                // Aguarda um pouco para garantir que o processo foi finalizado
                System.Threading.Thread.Sleep(2000);
            }
            
            StartComponent(component, version);
        }

        /// <summary>
        /// Escreve uma mensagem no log
        /// </summary>
        private static void WriteLog(string message)
        {
            try
            {
                string logFile = Path.Combine(System.AppContext.BaseDirectory, "devstack.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignora erros de logging
            }
        }
    }
}
