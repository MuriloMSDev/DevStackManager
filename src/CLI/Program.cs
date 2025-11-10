using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace DevStackManager
{
    public class Program
    {
        #region Constants
        // Time Constants (milliseconds)
        private const int RESTART_DELAY_MS = 1000;
        
        // Log Display Constants
        private const int LOG_DISPLAY_LINES = 50;
        
        // IP Configuration Constants
        private const string PHP_UPSTREAM_IP_PREFIX = "127.";
        private const string PHP_UPSTREAM_PORT = ":9000";
        
        // Minimum Arguments Required
        private const int SITE_MIN_ARGS = 4;
        #endregion
        
        // LocalizationManager instance
        private static DevStackShared.LocalizationManager? _localizationManager;
        private static DevStackShared.LocalizationManager LocalizationManager => 
            _localizationManager ?? throw new InvalidOperationException("LocalizationManager not initialized");
        
        public static string baseDir => DevStackConfig.baseDir;
        public static string phpDir => DevStackConfig.phpDir;
        public static string nginxDir => DevStackConfig.nginxDir;
        public static string mysqlDir => DevStackConfig.mysqlDir;
        public static string nodeDir => DevStackConfig.nodeDir;
        public static string pythonDir => DevStackConfig.pythonDir;
        public static string composerDir => DevStackConfig.composerDir;
        public static string pmaDir => DevStackConfig.pmaDir;
        public static string mongoDir => DevStackConfig.mongoDir;
        public static string pgsqlDir => DevStackConfig.pgsqlDir;
        public static string elasticDir => DevStackConfig.elasticDir;
        public static string wpcliDir => DevStackConfig.wpcliDir;
        public static string adminerDir => DevStackConfig.adminerDir;
        public static string goDir => DevStackConfig.goDir;
        public static string openSSLDir => DevStackConfig.openSSLDir;
        public static string phpcsfixerDir => DevStackConfig.phpcsfixerDir;
        public static string nginxSitesDir => DevStackConfig.nginxSitesDir;
        public static string tmpDir => DevStackConfig.tmpDir;
        
        public static PathManager? pathManager => DevStackConfig.pathManager;

        public static int Main(string[] args)
        {
            // Configurar console para CLI
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            try
            {
                // Verificar se o comando requer privilégios de administrador
                if (RequiresAdministrator(args))
                {
                    if (!IsAdministrator())
                    {
                        // Solicitar elevação e reexecutar com privilégios de administrador
                        return RestartAsAdministrator(args);
                    }
                }

                LoadConfiguration();

                // Se não há argumentos, entrar em modo REPL
                if (args.Length == 0)
                {
                    Console.WriteLine(LocalizationManager.GetString("cli.shell.interactive_prompt"));
                    while (true)
                    {
                        Console.Write(LocalizationManager.GetString("cli.shell.prompt"));
                        string? input = Console.ReadLine();
                        if (input == null) continue;
                        input = input.Trim();
                        if (string.IsNullOrEmpty(input)) continue;
                        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                            break;

                        // Separar comando e argumentos
                        var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        string command = split[0].ToLowerInvariant();
                        string[] commandArgs = split.Skip(1).ToArray();
                        
                        // Verificar se o comando requer privilégios de administrador
                        if (RequiresAdministrator(new[] { command }))
                        {
                            if (!IsAdministrator())
                            {
                                WriteWarningMsg(LocalizationManager.GetString("cli.shell.command_requires_admin", command));
                                WriteInfo(LocalizationManager.GetString("cli.shell.run_as_admin_hint", command));
                                continue;
                            }
                        }

                        try
                        {
                            int result = ExecuteCommand(command, commandArgs);
                            // Opcional: mostrar código de saída se não for 0
                            if (result != 0)
                                Console.WriteLine(LocalizationManager.GetString("cli.shell.exit_code", result));
                        }
                        catch (Exception ex)
                        {
                            WriteErrorMsg(LocalizationManager.GetString("cli.error.unexpected", ex.Message));
                            WriteLog($"Erro inesperado: {ex}");
                        }
                    }
                    return 0;
                }

                // Modo tradicional (com argumentos)
                string commandArg = args[0].ToLowerInvariant();
                string[] commandArgsArr = args.Skip(1).ToArray();
                return ExecuteCommand(commandArg, commandArgsArr);
            }
            catch (Exception ex)
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.error.unexpected", ex.Message));
                WriteLog($"Erro inesperado: {ex}");
                return 1;
            }
        }

        /// <summary>
        /// Verifica se o comando atual requer privilégios de administrador
        /// </summary>
        private static bool RequiresAdministrator(string[] args)
        {
            if (args.Length == 0)
                return false; // Modo REPL não requer admin

            string command = args[0].ToLowerInvariant();
            
            // Comandos que requerem privilégios de administrador
            string[] adminCommands = {
                "install", "uninstall", "start", "stop", "restart", 
                "site", "enable", "disable", "service", "global"
            };

            return adminCommands.Contains(command);
        }

        /// <summary>
        /// Reinicia a aplicação como administrador
        /// </summary>
        private static int RestartAsAdministrator(string[] args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "DevStack.exe"),
                    Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = true,
                    Verb = "runas" // Solicita elevação
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    return process.ExitCode;
                }
                return 1;
            }
            catch (Exception ex)
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.error.admin_request", ex.Message));
                return 1;
            }
        }

#pragma warning disable CA1416 // Validate platform compatibility
        private static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
#pragma warning restore CA1416

        private static void LoadConfiguration()
        {
            DevStackConfig.Initialize();
            
            // Carregar idioma do settings.conf ou usar pt_BR como padrão
            string defaultLanguage = "pt_BR";
            string? savedLanguage = DevStackConfig.GetSetting("language") as string;
            string languageToUse = savedLanguage ?? defaultLanguage;
            
            // Inicializar LocalizationManager com o idioma apropriado
            _localizationManager = DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);
            DevStackShared.LocalizationManager.ApplyLanguage(languageToUse);
        }

        private static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteWarningMsg(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteErrorMsg(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteColoredLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteLog(string message)
        {
            try
            {
                var logFile = System.IO.Path.Combine(System.AppContext.BaseDirectory, "devstack.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private static void StatusComponent(string component)
        {
            var status = GetComponentStatus(component);
            
            if (!status.installed)
            {
                WriteWarningMsg(status.message);
                return;
            }
            
            WriteInfo(LocalizationManager.GetString("cli.status.installed_versions", component));
            foreach (var version in status.versions)
            {
                Console.WriteLine($"  {version}");
            }
        }

        private static void StatusAll()
        {
            Console.WriteLine(LocalizationManager.GetString("cli.status.title"));
            var allStatus = DevStackManager.DataManager.GetAllComponentsStatus();
            foreach (var comp in allStatus.Keys)
            {
                var status = allStatus[comp];
                if (status.Installed && status.Versions.Count > 0)
                {
                    // Detectar se é serviço monitorado
                    var isService = comp == "php" || comp == "nginx";
                    WriteInfo(LocalizationManager.GetString("cli.status.installed", comp));
                    foreach (var version in status.Versions)
                    {
                        if (isService)
                        {
                            bool running = status.RunningList != null && status.RunningList.TryGetValue(version, out var isRunning) && isRunning;
                            string runningText = running ? LocalizationManager.GetString("cli.status.running") : LocalizationManager.GetString("cli.status.stopped");
                            Console.WriteLine($"  {version} {runningText}");
                        }
                        else
                        {
                            Console.WriteLine($"  {version}");
                        }
                    }
                }
            }
        }

        private static void TestAll()
        {
            Console.WriteLine(LocalizationManager.GetString("cli.test.title"));
            
            var tools = new[]
            {
                new { name = "php", exe = "php-*.exe", dir = phpDir, args = "-v" },
                new { name = "nginx", exe = "nginx-*.exe", dir = nginxDir, args = "-v" },
                new { name = "mysql", exe = "mysqld-*.exe", dir = mysqlDir, args = "--version" },
                new { name = "node", exe = "node-*.exe", dir = nodeDir, args = "-v" },
                new { name = "python", exe = "python-*.exe", dir = pythonDir, args = "--version" },
                new { name = "git", exe = "git.exe", dir = baseDir, args = "--version" },
                new { name = "composer", exe = "composer-*.phar", dir = composerDir, args = "--version" },
                new { name = "phpmyadmin", exe = "index.php", dir = pmaDir, args = "" },
                new { name = "mongodb", exe = "mongo.exe", dir = mongoDir, args = "--version" },
                new { name = "pgsql", exe = "psql.exe", dir = pgsqlDir, args = "--version" },
                new { name = "elasticsearch", exe = "elasticsearch.exe", dir = elasticDir, args = "--version" },
                new { name = "wpcli", exe = "wp-cli.phar", dir = wpcliDir, args = "--version" },
                new { name = "adminer", exe = "adminer-*.php", dir = adminerDir, args = "" },
                new { name = "go", exe = "go.exe", dir = goDir, args = "version" },
                new { name = "openssl", exe = "openssl.exe", dir = openSSLDir, args = "version" },
                new { name = "phpcsfixer", exe = "php-cs-fixer-*.phar", dir = phpcsfixerDir, args = "--version" }
            };

            foreach (var tool in tools)
            {
                string? found = null;
                
                if (tool.name == "git")
                {
                    found = FindFile(tool.dir, tool.exe, true, "*\\cmd\\git.exe");
                }
                else
                {
                    found = FindFile(tool.dir, tool.exe, true);
                }

                if (!string.IsNullOrEmpty(found))
                {
                    try
                    {
                        var output = ProcessManager.ExecuteProcess(found, tool.args);
                        WriteInfo(LocalizationManager.GetString("cli.test.tool_output", tool.name, output));
                    }
                    catch
                    {
                        WriteErrorMsg(LocalizationManager.GetString("cli.test.error_executing", tool.name, found));
                    }
                }
                else
                {
                    WriteWarningMsg(LocalizationManager.GetString("cli.test.not_found", tool.name));
                }
            }
        }

        private static void DepsCheck()
        {
            Console.WriteLine(LocalizationManager.GetString("cli.deps.title"));
            var missing = new List<string>();

            if (!IsAdministrator())
            {
                missing.Add(LocalizationManager.GetString("cli.deps.missing_admin"));
            }

            if (missing.Count == 0)
            {
                WriteInfo(LocalizationManager.GetString("cli.deps.all_present"));
            }
            else
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.deps.missing_deps", string.Join(", ", missing)));
            }
        }

        private static string CenterText(string text, int width)
        {
            int pad = Math.Max(0, width - text.Length);
            int padLeft = (int)Math.Floor(pad / 2.0);
            int padRight = pad - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        private static void UpdateComponent(string component)
        {
            _ = HandleInstallCommand([component]);
        }

        private static void AliasComponent(string component, string version)
        {
            string aliasDir = Path.Combine(baseDir, "aliases");
            if (!Directory.Exists(aliasDir))
            {
                Directory.CreateDirectory(aliasDir);
            }

            string? exe = component.ToLowerInvariant() switch
            {
                "php" => Path.Combine(phpDir, $"php-{version}", $"php-{version}.exe"),
                "nginx" => Path.Combine(nginxDir, $"nginx-{version}", $"nginx-{version}.exe"),
                "node" => Path.Combine(nodeDir, $"node-{version}", $"node-{version}.exe"),
                "python" => Path.Combine(pythonDir, $"python-{version}", $"python-{version}.exe"),
                "git" => Path.Combine(baseDir, $"git-{version}", "cmd", "git.exe"),
                "mysql" => Path.Combine(mysqlDir, $"mysql-{version}", "bin", "mysql.exe"),
                "phpmyadmin" => Path.Combine(pmaDir, $"phpmyadmin-{version}", "index.php"),
                "mongodb" => Path.Combine(mongoDir, $"mongodb-{version}", "bin", "mongo.exe"),
                "pgsql" => Path.Combine(pgsqlDir, $"pgsql-{version}", "bin", "psql.exe"),
                "elasticsearch" => Path.Combine(elasticDir, $"elasticsearch-{version}", "bin", "elasticsearch.exe"),
                "wpcli" => Path.Combine(wpcliDir, $"wp-cli-{version}", "wp-cli.phar"),
                "adminer" => Path.Combine(adminerDir, $"adminer-{version}.php"),
                "go" => Path.Combine(goDir, $"go{version}", "bin", "go.exe"),
                "openssl" => Path.Combine(openSSLDir, $"openssl-{version}", "bin", "openssl.exe"),
                "phpcsfixer" => Path.Combine(phpcsfixerDir, $"phpcsfixer-{version}", $"php-cs-fixer-{version}.phar"),
                _ => null
            };

            if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
            {
                string bat = Path.Combine(aliasDir, $"{component}{version}.bat");
                File.WriteAllText(bat, $"@echo off\r\n\"{exe}\" %*", Encoding.UTF8);
                WriteInfo(LocalizationManager.GetString("cli.alias.created", bat));
            }
            else
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.alias.executable_not_found", component, version));
            }
        }

        private static void ShowUsage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(LocalizationManager.GetString("cli.commands.help_title"));
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine(LocalizationManager.GetString("cli.commands.gui_hint"));
            Console.WriteLine();

            // Obter a tabela como string e exibi-la com cores
            var helpTable = DevStackConfig.ShowHelpTable();
            var lines = helpTable.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.Contains("Comando") || line.Contains("Descrição") || line.StartsWith("╔") || line.StartsWith("╠") || line.StartsWith("╚"))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(line);
                    Console.ResetColor();
                }
                else if (line.StartsWith("║"))
                {
                    // Linha de comando - colorir diferente
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("║");
                    Console.ResetColor();
                    
                    // Extrair comando e descrição
                    var parts = line.Split('║');
                    if (parts.Length >= 3)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(parts[1]);
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("║");
                        Console.ResetColor();
                        Console.Write(parts[2]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("║");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    Console.WriteLine(line);
                }
            }
        }

        private static int ExecuteCommand(string command, string[] args)
        {
            WriteLog($"Comando executado: {command} {string.Join(" ", args)}");

            return command switch
            {
                "help" => ShowUsageAndReturn(),
                "list" => HandleListCommand(args),
                "site" => HandleSiteCommand(args),
                "install" => HandleInstallCommand(args).Result,
                "path" => HandlePathCommand(args),
                "uninstall" => HandleUninstallCommand(args),
                "start" => HandleStartCommand(args),
                "stop" => HandleStopCommand(args),
                "restart" => HandleRestartCommand(args),
                "status" => ExecuteStatusCommand(),
                "test" => ExecuteTestCommand(),
                "deps" => ExecuteDepsCommand(),
                "update" => HandleUpdateCommand(args),
                "alias" => HandleAliasCommand(args),
                "self-update" => HandleSelfUpdateCommand(),
                "clean" => HandleCleanCommand(),
                "backup" => HandleBackupCommand(),
                "logs" => HandleLogsCommand(),
                "enable" => HandleEnableCommand(args),
                "disable" => HandleDisableCommand(args),
                "config" => HandleConfigCommand(),
                "reset" => HandleResetCommand(args),
                "ssl" => HandleSslCommand(args).Result,
                "db" => HandleDbCommand(args),
                "service" => HandleServiceCommand(),
                "doctor" => HandleDoctorCommand(),
                "global" => HandleGlobalCommand(),
                "language" or "lang" => HandleLanguageCommand(args),
                _ => HandleUnknownCommand(command)
            };
        }

        private static int ShowUsageAndReturn()
        {
            ShowUsage();
            return 0;
        }

        private static int ExecuteStatusCommand()
        {
            StatusAll();
            return 0;
        }

        private static int ExecuteTestCommand()
        {
            TestAll();
            return 0;
        }

        private static int ExecuteDepsCommand()
        {
            DepsCheck();
            return 0;
        }

        private static int HandleUpdateCommand(string[] args)
        {
            foreach (var component in args)
            {
                UpdateComponent(component);
            }
            return 0;
        }

        private static int HandleUnknownCommand(string command)
        {
            Console.WriteLine(LocalizationManager.GetString("cli.commands.unknown", command));
            return 1;
        }

        private static int HandleListCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.list"));
                return 1;
            }

            string firstArg = args[0].Trim();
            if (firstArg == "--installed")
            {
                ListManager.ListInstalledVersions();
                return 0;
            }

            ListManager.ListVersions(firstArg);
            return 0;
        }

        private static int HandleSiteCommand(string[] args)
        {
            if (args.Length < SITE_MIN_ARGS)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.site"));
                return 1;
            }

            var siteConfig = ParseSiteArguments(args);
            
            if (!ValidateSiteConfig(siteConfig))
            {
                return 1;
            }

            InstallManager.CreateNginxSiteConfig(
                siteConfig.Domain, 
                siteConfig.Root, 
                siteConfig.PhpUpstream, 
                siteConfig.NginxVersion);
            
            return 0;
        }

        private static SiteConfig ParseSiteArguments(string[] args)
        {
            var config = new SiteConfig { Domain = args[0] };

            for (int i = 1; i < args.Length - 1; i++)
            {
                var flag = args[i].ToLowerInvariant();
                var value = args[i + 1];

                switch (flag)
                {
                    case "-root":
                        config.Root = value;
                        i++;
                        break;
                    case "-php":
                        config.PhpUpstream = BuildPhpUpstream(value);
                        i++;
                        break;
                    case "-nginx":
                        config.NginxVersion = value;
                        i++;
                        break;
                }
            }

            return config;
        }

        /// <summary>
        /// Constrói o endereço PHP upstream no formato 127.x.x.x:9000
        /// </summary>
        private static string BuildPhpUpstream(string value)
        {
            return $"{PHP_UPSTREAM_IP_PREFIX}{value}{PHP_UPSTREAM_PORT}";
        }

        private static bool ValidateSiteConfig(SiteConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.Domain))
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.site_error_domain"));
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.Root))
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.site_error_root"));
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.PhpUpstream))
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.site_error_php"));
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.NginxVersion))
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.site_error_nginx"));
                return false;
            }
            return true;
        }

        private class SiteConfig
        {
            public string Domain { get; set; } = string.Empty;
            public string Root { get; set; } = string.Empty;
            public string PhpUpstream { get; set; } = string.Empty;
            public string NginxVersion { get; set; } = string.Empty;
        }

        private static async Task<int> HandleInstallCommand(string[] args)
        {
            await InstallCommands(args);
            return 0;
        }

        private static int HandleUninstallCommand(string[] args)
        {
            UninstallCommands(args);
            return 0;
        }

        private static int HandlePathCommand(string[] args)
        {
            if (pathManager == null)
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.path.manager_not_initialized"));
                return 1;
            }

            if (args.Length == 0)
            {
                pathManager.AddBinDirsToPath();
                return 0;
            }

            string subCommand = args[0].ToLowerInvariant();
            switch (subCommand)
            {
                case "add":
                    pathManager.AddBinDirsToPath();
                    break;

                case "remove":
                    if (args.Length > 1)
                    {
                        var dirsToRemove = args.Skip(1).ToArray();
                        pathManager.RemoveFromPath(dirsToRemove);
                    }
                    else
                    {
                        pathManager.RemoveAllDevStackFromPath();
                    }
                    break;

                case "list":
                    pathManager.ListCurrentPath();
                    break;

                case "help":
                    WriteInfo(LocalizationManager.GetString("cli.path.help_title"));
                    WriteInfo(LocalizationManager.GetString("cli.path.help_add"));
                    WriteInfo(LocalizationManager.GetString("cli.path.help_add_explicit"));
                    WriteInfo(LocalizationManager.GetString("cli.path.help_remove"));
                    WriteInfo(LocalizationManager.GetString("cli.path.help_remove_specific"));
                    WriteInfo(LocalizationManager.GetString("cli.path.help_list"));
                    WriteInfo(LocalizationManager.GetString("cli.path.help_help"));
                    break;

                default:
                    WriteErrorMsg(LocalizationManager.GetString("cli.path.unknown_subcommand", subCommand));
                    WriteInfo(LocalizationManager.GetString("cli.path.use_help"));
                    return 1;
            }

            return 0;
        }

        private static int HandleStartCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.start"));
                return 1;
            }

            string target = args[0].ToLowerInvariant();
            if (target == "--all")
            {
                if (Directory.Exists(nginxDir))
                {
                    ProcessManager.ForEachVersion("nginx", v => ProcessManager.StartComponent("nginx", v));
                }
                else
                {
                    WriteWarningMsg(LocalizationManager.GetString("cli.directories.nginx_not_found"));
                }

                if (Directory.Exists(phpDir))
                {
                    ProcessManager.ForEachVersion("php", v => ProcessManager.StartComponent("php", v));
                }
                else
                {
                    WriteWarningMsg(LocalizationManager.GetString("cli.directories.php_not_found"));
                }
                return 0;
            }

            if (args.Length < 2)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.start_version"));
                return 1;
            }

            string version = args[1];
            ProcessManager.StartComponent(target, version);
            return 0;
        }

        private static int HandleStopCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.stop"));
                return 1;
            }

            string target = args[0].ToLowerInvariant();
            if (target == "--all")
            {
                if (Directory.Exists(nginxDir))
                {
                    ProcessManager.ForEachVersion("nginx", v => ProcessManager.StopComponent("nginx", v));
                }
                else
                {
                    WriteWarningMsg(LocalizationManager.GetString("cli.directories.nginx_not_found"));
                }

                if (Directory.Exists(phpDir))
                {
                    ProcessManager.ForEachVersion("php", v => ProcessManager.StopComponent("php", v));
                }
                else
                {
                    WriteWarningMsg(LocalizationManager.GetString("cli.directories.php_not_found"));
                }
                return 0;
            }

            if (args.Length < 2)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.stop_version"));
                return 1;
            }

            string version = args[1];
            ProcessManager.StopComponent(target, version);
            return 0;
        }

        private static int HandleRestartCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.restart"));
                return 1;
            }

            string target = args[0].ToLowerInvariant();
            if (target == "--all")
            {
                if (Directory.Exists(nginxDir))
                {
                    ProcessManager.ForEachVersion("nginx", v => 
                    {
                        ProcessManager.StopComponent("nginx", v);
                        Thread.Sleep(RESTART_DELAY_MS);
                        ProcessManager.StartComponent("nginx", v);
                    });
                }
                else
                {
                    WriteWarningMsg(LocalizationManager.GetString("cli.directories.nginx_not_found"));
                }

                if (Directory.Exists(phpDir))
                {
                    ProcessManager.ForEachVersion("php", v => 
                    {
                        ProcessManager.StopComponent("php", v);
                        Thread.Sleep(RESTART_DELAY_MS);
                        ProcessManager.StartComponent("php", v);
                    });
                }
                else
                {
                    WriteWarningMsg(LocalizationManager.GetString("cli.directories.php_not_found"));
                }
                return 0;
            }

            if (args.Length < 2)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.restart_version"));
                return 1;
            }

            string version = args[1];
            ProcessManager.StopComponent(target, version);
            Thread.Sleep(RESTART_DELAY_MS);
            ProcessManager.StartComponent(target, version);
            return 0;
        }

        private static int HandleAliasCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.alias"));
                return 1;
            }

            AliasComponent(args[0], args[1]);
            return 0;
        }

        private static int HandleSelfUpdateCommand()
        {
            string repoDir = System.AppContext.BaseDirectory;
            if (Directory.Exists(Path.Combine(repoDir, ".git")))
            {
                WriteInfo(LocalizationManager.GetString("cli.self_update.updating"));
                try
                {
                    var result = ProcessManager.ExecuteProcess("git", "pull", repoDir);
                    WriteInfo(LocalizationManager.GetString("cli.self_update.success"));
                }
                catch (Exception ex)
                {
                    WriteErrorMsg(LocalizationManager.GetString("cli.self_update.error", ex.Message));
                }
            }
            else
            {
                WriteWarningMsg(LocalizationManager.GetString("cli.self_update.not_git_repo"));
            }
            return 0;
        }

        private static int HandleCleanCommand()
        {
            string logDir = Path.Combine(baseDir, "logs");
            string tmpDir = Path.Combine(baseDir, "tmp");
            var logFile = System.IO.Path.Combine(System.AppContext.BaseDirectory, "devstack.log");
            int count = 0;

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
                count++;
            }

            if (Directory.Exists(logDir))
            {
                Directory.Delete(logDir, true);
                count++;
            }

            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
                count++;
            }

            WriteInfo(LocalizationManager.GetString("cli.clean.completed", count));
            return 0;
        }

        private static int HandleBackupCommand()
        {
            string backupDir = Path.Combine(baseDir, "backups", $"backup-{DateTime.Now:yyyyMMdd-HHmmss}");
            string[] toBackup = { Path.Combine("tools", "configs"), "devstack.log" };

            Directory.CreateDirectory(backupDir);

            foreach (string item in toBackup)
            {
                string src = Path.Combine(System.AppContext.BaseDirectory, item);
                if (File.Exists(src))
                {
                    File.Copy(src, Path.Combine(backupDir, item));
                }
                else if (Directory.Exists(src))
                {
                    CopyDirectory(src, Path.Combine(backupDir, item));
                }
            }

            WriteInfo(LocalizationManager.GetString("cli.backup.created", backupDir));
            return 0;
        }

        private static int HandleLogsCommand()
        {
            string logFile = Path.Combine(System.AppContext.BaseDirectory, "devstack.log");
            if (File.Exists(logFile))
            {
                Console.WriteLine(LocalizationManager.GetString("cli.logs.last_lines", LOG_DISPLAY_LINES, logFile));
                var lines = File.ReadAllLines(logFile, Encoding.UTF8);
                var lastLines = lines.Skip(Math.Max(0, lines.Length - LOG_DISPLAY_LINES));
                foreach (var line in lastLines)
                {
                    Console.WriteLine(line);
                }
            }
            else
            {
                WriteWarningMsg(LocalizationManager.GetString("cli.logs.not_found"));
            }
            return 0;
        }

        private static int HandleEnableCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.enable"));
                return 1;
            }

            string svc = args[0];
#pragma warning disable CA1416 // Validate platform compatibility
            try
            {
                var service = new ServiceController(svc);
                service.Start();
                WriteInfo(LocalizationManager.GetString("cli.service.enabled", svc));
            }
            catch (Exception ex)
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.service.error_enable", svc, ex.Message));
            }
#pragma warning restore CA1416
            return 0;
        }

        private static int HandleDisableCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.disable"));
                return 1;
            }

            string svc = args[0];
#pragma warning disable CA1416 // Validate platform compatibility
            try
            {
                var service = new ServiceController(svc);
                service.Stop();
                WriteInfo(LocalizationManager.GetString("cli.service.disabled", svc));
            }
            catch (Exception ex)
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.service.error_disable", svc, ex.Message));
            }
#pragma warning restore CA1416
            return 0;
        }

        private static int HandleConfigCommand()
        {
            string configDir = Path.Combine(baseDir, "configs");
            if (Directory.Exists(configDir))
            {
                Process.Start("explorer.exe", configDir);
                WriteInfo(LocalizationManager.GetString("cli.config.opened"));
            }
            else
            {
                WriteWarningMsg(LocalizationManager.GetString("cli.config.not_found"));
            }
            return 0;
        }

        private static int HandleResetCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.reset"));
                return 1;
            }

            string comp = args[0];
            WriteInfo(LocalizationManager.GetString("cli.reset.resetting", comp));
            UninstallCommands(new[] { comp });
            _ = InstallCommands(new[] { comp });
            WriteInfo(LocalizationManager.GetString("cli.reset.completed", comp));
            return 0;
        }

        private static async Task<int> HandleSslCommand(string[] args)
        {
            await GenerateManager.GenerateSslCertificate(args);
            return 0;
        }

        private static int HandleDbCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.db"));
                return 1;
            }

            string db = args[0].ToLowerInvariant();
            string cmd = args[1].ToLowerInvariant();

            switch (db)
            {
                case "mysql":
                    {
                        string? mysqlExe = FindFile(mysqlDir, "mysql.exe", true);
                        if (string.IsNullOrEmpty(mysqlExe))
                        {
                            WriteErrorMsg(LocalizationManager.GetString("cli.db.mysql_not_found"));
                            return 1;
                        }

                        switch (cmd)
                        {
                            case "list":
                                ProcessManager.ExecuteProcess(mysqlExe, "-e \"SHOW DATABASES;\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mysqlExe, $"-e \"CREATE DATABASE {args[2]};\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mysqlExe, $"-e \"DROP DATABASE {args[2]};\"");
                                break;
                            default:
                                Console.WriteLine(LocalizationManager.GetString("cli.db.unknown_command_mysql"));
                                break;
                        }
                        break;
                    }

                case "pgsql":
                    {
                        string? psqlExe = FindFile(pgsqlDir, "psql.exe", true);
                        if (string.IsNullOrEmpty(psqlExe))
                        {
                            WriteErrorMsg(LocalizationManager.GetString("cli.db.pgsql_not_found"));
                            return 1;
                        }

                        switch (cmd)
                        {
                            case "list":
                                ProcessManager.ExecuteProcess(psqlExe, "-c \"\\l\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(psqlExe, $"-c \"CREATE DATABASE {args[2]};\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(psqlExe, $"-c \"DROP DATABASE {args[2]};\"");
                                break;
                            default:
                                Console.WriteLine(LocalizationManager.GetString("cli.db.unknown_command_pgsql"));
                                break;
                        }
                        break;
                    }

                case "mongo":
                    {
                        string? mongoExe = FindFile(mongoDir, "mongo.exe", true);
                        if (string.IsNullOrEmpty(mongoExe))
                        {
                            WriteErrorMsg(LocalizationManager.GetString("cli.db.mongo_not_found"));
                            return 1;
                        }

                        switch (cmd)
                        {
                            case "list":
                                ProcessManager.ExecuteProcess(mongoExe, "--eval \"db.adminCommand('listDatabases')\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mongoExe, $"--eval \"db.getSiblingDB('{args[2]}')\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mongoExe, $"--eval \"db.getSiblingDB('{args[2]}').dropDatabase()\"");
                                break;
                            default:
                                Console.WriteLine(LocalizationManager.GetString("cli.db.unknown_command_mongo"));
                                break;
                        }
                        break;
                    }

                default:
                    Console.WriteLine(LocalizationManager.GetString("cli.db.unsupported_db", db));
                    break;
            }
            return 0;
        }

        private static int HandleServiceCommand()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            try
            {
                var services = ServiceController.GetServices()
                    .Where(s => s.DisplayName.Contains("devstack", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (services.Any())
                {
                    Console.WriteLine(LocalizationManager.GetString("cli.service.list_header"));
                    Console.WriteLine(new string('-', 75));
                    foreach (var service in services)
                    {
                        Console.WriteLine($"{service.ServiceName.PadRight(20)} {service.Status.ToString().PadRight(15)} {service.DisplayName.PadRight(40)}");
                    }
                }
                else
                {
                    Console.WriteLine(LocalizationManager.GetString("cli.service.none_found"));
                }
            }
            catch (Exception ex)
            {
                WriteErrorMsg(LocalizationManager.GetString("cli.error.list_services", ex.Message));
            }
#pragma warning restore CA1416
            return 0;
        }

        private static int HandleDoctorCommand()
        {
            Console.WriteLine(LocalizationManager.GetString("cli.doctor.title"));
            ListManager.ListInstalledVersions();

            // Forçar sincronização do PATH com as configurações do usuário
            RefreshProcessPathFromUser();
            WriteInfo(LocalizationManager.GetString("cli.doctor.path_synced"));

            // Tabela PATH (agora mostra o PATH atual do processo + do usuário)
            var processPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            
            // Combinar e remover duplicatas para mostrar o PATH efetivo
            var processPathList = processPath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var userPathList = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var combinedPaths = processPathList.Concat(userPathList).Distinct().ToArray();
            
            int maxPathLen = combinedPaths.Length > 0 ? combinedPaths.Max(p => p.Length) : 20;
            string headerPath = string.Concat(Enumerable.Repeat('_', maxPathLen + 4));
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(headerPath);
            Console.Write("| ");
            Console.ForegroundColor = ConsoleColor.Gray;
            
            // Get the full header text and split it to highlight "DevStack"
            string fullHeaderText = LocalizationManager.GetString("cli.doctor.path_header");
            int devstackIndex = fullHeaderText.IndexOf("DevStack", StringComparison.OrdinalIgnoreCase);
            
            if (devstackIndex >= 0)
            {
                string beforeDevstack = fullHeaderText.Substring(0, devstackIndex);
                string afterDevstack = fullHeaderText.Substring(devstackIndex + 8); // 8 = "DevStack".Length
                
                Console.Write(beforeDevstack);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("DevStack");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(afterDevstack);
            }
            else
            {
                Console.Write(fullHeaderText);
            }
            
            Console.WriteLine(new string(' ', maxPathLen - fullHeaderText.Length) + " |");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"|{string.Concat(Enumerable.Repeat('-', maxPathLen + 2))}|");
            
            foreach (string p in combinedPaths)
            {
                if (!string.IsNullOrWhiteSpace(p))
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("| ");
                    Console.ResetColor();
                    
                    // Destacar paths do DevStack
                    if (p.Contains("devstack", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    Console.Write(p.PadRight(maxPathLen));
                    
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(" |");
                    Console.ResetColor();
                }
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(string.Concat(Enumerable.Repeat('¯', maxPathLen + 4)));
            Console.ResetColor();

            // Tabela Usuário
            string user = Environment.UserName;
            int colUser = Math.Max(8, user.Length);
            string headerUser = string.Concat(Enumerable.Repeat('_', colUser + 4));
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(headerUser);
            Console.WriteLine($"| {LocalizationManager.GetString("cli.doctor.user_header").PadRight(colUser)} |");
            Console.WriteLine($"|{string.Concat(Enumerable.Repeat('-', colUser + 2))}|");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("| ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{user.PadRight(colUser)}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" |");
            Console.WriteLine(string.Concat(Enumerable.Repeat('¯', colUser + 4)));
            Console.ResetColor();

            // Tabela Sistema
            string os = Environment.OSVersion.ToString();
            int colOS = Math.Max(8, os.Length);
            string headerOS = string.Concat(Enumerable.Repeat('_', colOS + 4));
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(headerOS);
            Console.WriteLine($"| {LocalizationManager.GetString("cli.doctor.system_header").PadRight(colOS)} |");
            Console.WriteLine($"|{string.Concat(Enumerable.Repeat('-', colOS + 2))}|");
            Console.Write("| ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{os.PadRight(colOS)}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" |");
            Console.WriteLine(string.Concat(Enumerable.Repeat('¯', colOS + 4)));
            Console.ResetColor();

            return 0;
        }

        private static int HandleGlobalCommand()
        {
            string devstackDir = System.AppContext.BaseDirectory;
            string currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            
            if (!currentPath.Contains(devstackDir))
            {
                Environment.SetEnvironmentVariable("Path", $"{currentPath};{devstackDir}", EnvironmentVariableTarget.User);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(LocalizationManager.GetString("cli.global.added", devstackDir));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(LocalizationManager.GetString("cli.global.already_exists", devstackDir));
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(LocalizationManager.GetString("cli.global.run_anywhere"));
            Console.ResetColor();
            
            return 0;
        }

        private static int HandleLanguageCommand(string[] args)
        {
            var localizationManager = DevStackShared.LocalizationManager.Instance 
                ?? DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);

            // Se não há argumentos, listar idiomas disponíveis e mostrar o atual
            if (args.Length == 0)
            {
                var availableLanguages = localizationManager.GetAvailableLanguages();
                var currentLanguage = DevStackShared.LocalizationManager.CurrentLanguageStatic;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(LocalizationManager.GetString("cli.language.available_title"));
                Console.ResetColor();
                Console.WriteLine();

                foreach (var lang in availableLanguages)
                {
                    var langName = localizationManager.GetLanguageName(lang);
                    var isCurrent = lang == currentLanguage;

                    if (isCurrent)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("  ▶ ");
                        Console.Write($"{lang.PadRight(10)} - {langName}");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(LocalizationManager.GetString("cli.language.current_marker"));
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"    {lang.PadRight(10)} - {langName}");
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(LocalizationManager.GetString("cli.language.change_hint"));
                Console.WriteLine(LocalizationManager.GetString("cli.language.example"));
                Console.ResetColor();

                return 0;
            }

            // Se há argumentos, alterar o idioma
            string newLanguage = args[0];
            var availableLangs = localizationManager.GetAvailableLanguages();

            if (!availableLangs.Contains(newLanguage))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(LocalizationManager.GetString("cli.language.not_found", newLanguage));
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine(LocalizationManager.GetString("cli.language.available_list"));
                foreach (var lang in availableLangs)
                {
                    Console.WriteLine($"  - {lang} ({localizationManager.GetLanguageName(lang)})");
                }
                return 1;
            }

            try
            {
                // Aplicar o novo idioma
                DevStackShared.LocalizationManager.ApplyLanguage(newLanguage);

                // Salvar nas configurações
                DevStackConfig.PersistSetting("language", newLanguage);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(LocalizationManager.GetString("cli.language.changed", localizationManager.GetLanguageName(newLanguage), newLanguage));
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(LocalizationManager.GetString("cli.language.note_gui"));
                Console.WriteLine(LocalizationManager.GetString("cli.language.note_cli"));
                Console.ResetColor();

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(LocalizationManager.GetString("cli.language.error_changing", ex.Message));
                Console.ResetColor();
                WriteLog($"Erro ao alterar idioma: {ex}");
                return 1;
            }
        }

        /// <summary>
        /// Sincroniza o PATH do processo atual com o PATH do usuário
        /// Isso garante que mudanças no PATH do usuário sejam refletidas imediatamente
        /// </summary>
        private static void RefreshProcessPathFromUser()
        {
            try
            {
                var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
                var machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) ?? "";
                var processPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                
                // Combinar Machine PATH + User PATH (ordem padrão do Windows)
                var combinedPath = string.IsNullOrEmpty(machinePath) ? userPath : 
                                  string.IsNullOrEmpty(userPath) ? machinePath : 
                                  $"{machinePath};{userPath}";
                
                // Atualizar PATH do processo atual
                Environment.SetEnvironmentVariable("PATH", combinedPath);
                
                Program.WriteLog($"PATH do processo sincronizado com PATH do usuário. Total de entradas: {combinedPath.Split(';').Length}");
            }
            catch (Exception ex)
            {
                Program.WriteLog($"Erro ao sincronizar PATH: {ex.Message}");
            }
        }

        // Helper methods that need to be implemented
        private static (bool installed, string message, string[] versions) GetComponentStatus(string component)
        {
            // Map component name to directory
            string? dir = component.ToLowerInvariant() switch
            {
                "php" => phpDir,
                "nginx" => nginxDir,
                "mysql" => mysqlDir,
                "node" or "nodejs" => nodeDir,
                "python" => pythonDir,
                "composer" => composerDir,
                "git" => baseDir,
                "phpmyadmin" or "pma" => pmaDir,
                "mongodb" or "mongo" => mongoDir,
                "pgsql" or "postgresql" => pgsqlDir,
                "elasticsearch" or "elastic" => elasticDir,
                "wpcli" or "wp-cli" => wpcliDir,
                "adminer" => adminerDir,
                "go" or "golang" => goDir,
                "openssl" => openSSLDir,
                "phpcsfixer" => phpcsfixerDir,
                _ => null
            };

            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return (false, string.Empty, Array.Empty<string>());
            }

            string[] versions;
            if (component.Equals("git", StringComparison.OrdinalIgnoreCase))
            {
                // git: procurar subdiretórios git-*
                versions = Directory.GetDirectories(dir, "git-*")
                    .Select(f => Path.GetFileName(f) ?? string.Empty)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToArray();
            }
            else
            {
                versions = Directory.GetDirectories(dir)
                    .Select(f => Path.GetFileName(f) ?? string.Empty)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToArray();
            }

            if (versions.Length == 0)
            {
                return (false, string.Empty, Array.Empty<string>());
            }

            // Serviços monitorados pela aba Serviços
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp != null && comp.IsService)
            {
                var processList = System.Diagnostics.Process.GetProcesses();
                var versionsWithStatus = versions.Select(version => {
                    string dirName = version;
                    string status = "parado";
                    if (component.Equals(comp.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        string search = dirName;
                        var running = processList.Any(p => {
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
                        if (running) status = "executando";
                    }
                    return $"{version} ({status})";
                }).ToArray();
                return (true, $"{component} instalado(s)", versionsWithStatus);
            }

            return (true, $"{component} instalado(s)", versions);
        }

        private static Dictionary<string, (bool installed, string message, string[] versions)> GetAllComponentsStatus()
        {
            string[] components = new[]
            {
                "php", "nginx", "mysql", "nodejs", "python", "composer", "git", "phpmyadmin",
                "mongodb", "pgsql", "elasticsearch", "wpcli", "adminer",
                "go", "openssl", "phpcsfixer"
            };

            var results = new Dictionary<string, (bool installed, string message, string[] versions)>();
            foreach (var comp in components)
            {
                results[comp] = GetComponentStatus(comp);
            }
            return results;
        }

        private static string? FindFile(string directory, string pattern, bool recursive, string? pathFilter = null)
        {
            try
            {
                if (!Directory.Exists(directory)) return null;

                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(directory, pattern, searchOption);
                
                if (!string.IsNullOrEmpty(pathFilter))
                {
                    files = files.Where(f => f.Contains(pathFilter)).ToArray();
                }

                return files.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
        
        private static async Task InstallCommands(string[] args) 
        {
            await InstallManager.InstallCommands(args);
            
            // Atualizar PATH após instalação bem-sucedida
            if (pathManager != null)
            {
                pathManager.AddBinDirsToPath();
            }
        }
        
        private static void UninstallCommands(string[] args) 
        { 
            // Extrair os componentes e versões específicas dos argumentos
            var specificVersions = new List<string>();
            
            foreach (string arg in args)
            {
                // Tentar extrair componente e versão
                if (Regex.IsMatch(arg, @"^php-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^php-", "");
                    specificVersions.Add($"php:{version}");
                }
                else if (Regex.IsMatch(arg, @"^nginx-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^nginx-", "");
                    specificVersions.Add($"nginx:{version}");
                }
                else if (Regex.IsMatch(arg, @"^mysql-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^mysql-", "");
                    specificVersions.Add($"mysql:{version}");
                }
                else if (Regex.IsMatch(arg, @"^node-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^node-", "");
                    specificVersions.Add($"node:{version}");
                }
                else if (Regex.IsMatch(arg, @"^python-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^python-", "");
                    specificVersions.Add($"python:{version}");
                }
                else if (Regex.IsMatch(arg, @"^git-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^git-", "");
                    specificVersions.Add($"git:{version}");
                }
                else if (Regex.IsMatch(arg, @"^composer-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^composer-", "");
                    specificVersions.Add($"composer:{version}");
                }
                // Para comandos sem versão específica, detectar se há argumentos adicionais
                else
                {
                    // Para comandos como "uninstall php 8.4.10", o próximo argumento seria a versão
                    int currentIndex = Array.IndexOf(args, arg);
                    if (currentIndex >= 0 && currentIndex + 1 < args.Length)
                    {
                        string nextArg = args[currentIndex + 1];
                        // Verificar se o próximo argumento parece uma versão
                        if (Regex.IsMatch(nextArg, @"^\d+\.\d+"))
                        {
                            string component = arg.ToLowerInvariant();
                            if (component == "node") component = "node";
                            specificVersions.Add($"{component}:{nextArg}");
                        }
                    }
                }
            }
            
            UninstallManager.UninstallCommands(args);
        }
    }
}
