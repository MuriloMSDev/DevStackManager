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
    /// <summary>
    /// Main entry point for the DevStack Command Line Interface (CLI).
    /// Provides interactive shell and direct command execution for managing development stack components.
    /// </summary>
    public class Program
    {
        #region Constants
        /// <summary>
        /// Delay in milliseconds before restarting a service.
        /// </summary>
        private const int RESTART_DELAY_MS = 1000;
        
        /// <summary>
        /// Number of log lines to display in log commands.
        /// </summary>
        private const int LOG_DISPLAY_LINES = 50;
        
        /// <summary>
        /// IP prefix for PHP upstream configuration (localhost prefix).
        /// </summary>
        private const string PHP_UPSTREAM_IP_PREFIX = "127.";
        
        /// <summary>
        /// Port number for PHP upstream configuration.
        /// </summary>
        private const string PHP_UPSTREAM_PORT = ":9000";
        
        /// <summary>
        /// Minimum number of arguments required for site commands.
        /// </summary>
        private const int SITE_MIN_ARGS = 4;
        #endregion
        
        /// <summary>
        /// Localization manager instance for language support.
        /// </summary>
        private static DevStackShared.LocalizationManager? _localizationManager;
        
        /// <summary>
        /// Gets the localization manager for translation support.
        /// </summary>
        private static DevStackShared.LocalizationManager LocalizationManager => 
            _localizationManager ?? throw new InvalidOperationException("LocalizationManager not initialized");
        
        /// <summary>
        /// Gets the base directory path for DevStack installation.
        /// </summary>
        public static string baseDir => DevStackConfig.baseDir;
        
        /// <summary>
        /// Gets the PHP installation directory path.
        /// </summary>
        public static string phpDir => DevStackConfig.phpDir;
        
        /// <summary>
        /// Gets the Nginx installation directory path.
        /// </summary>
        public static string nginxDir => DevStackConfig.nginxDir;
        
        /// <summary>
        /// Gets the MySQL installation directory path.
        /// </summary>
        public static string mysqlDir => DevStackConfig.mysqlDir;
        
        /// <summary>
        /// Gets the Node.js installation directory path.
        /// </summary>
        public static string nodeDir => DevStackConfig.nodeDir;
        
        /// <summary>
        /// Gets the Python installation directory path.
        /// </summary>
        public static string pythonDir => DevStackConfig.pythonDir;
        
        /// <summary>
        /// Gets the Composer installation directory path.
        /// </summary>
        public static string composerDir => DevStackConfig.composerDir;
        
        /// <summary>
        /// Gets the phpMyAdmin installation directory path.
        /// </summary>
        public static string pmaDir => DevStackConfig.pmaDir;
        
        /// <summary>
        /// Gets the MongoDB installation directory path.
        /// </summary>
        public static string mongoDir => DevStackConfig.mongoDir;
        
        /// <summary>
        /// Gets the PostgreSQL installation directory path.
        /// </summary>
        public static string pgsqlDir => DevStackConfig.pgsqlDir;
        
        /// <summary>
        /// Gets the Elasticsearch installation directory path.
        /// </summary>
        public static string elasticDir => DevStackConfig.elasticDir;
        
        /// <summary>
        /// Gets the WP-CLI installation directory path.
        /// </summary>
        public static string wpcliDir => DevStackConfig.wpcliDir;
        
        /// <summary>
        /// Gets the Adminer installation directory path.
        /// </summary>
        public static string adminerDir => DevStackConfig.adminerDir;
        
        /// <summary>
        /// Gets the Go installation directory path.
        /// </summary>
        public static string goDir => DevStackConfig.goDir;
        
        /// <summary>
        /// Gets the OpenSSL installation directory path.
        /// </summary>
        public static string openSSLDir => DevStackConfig.openSSLDir;
        
        /// <summary>
        /// Gets the PHP CS Fixer installation directory path.
        /// </summary>
        public static string phpcsfixerDir => DevStackConfig.phpcsfixerDir;
        
        /// <summary>
        /// Gets the Nginx sites-enabled directory path.
        /// </summary>
        public static string nginxSitesDir => DevStackConfig.nginxSitesDir;
        
        /// <summary>
        /// Gets the temporary files directory path.
        /// </summary>
        public static string tmpDir => DevStackConfig.tmpDir;
        
        /// <summary>
        /// Gets the PATH manager instance for system PATH manipulation.
        /// </summary>
        public static PathManager? pathManager => DevStackConfig.pathManager;

        /// <summary>
        /// Main entry point for the DevStack CLI application.
        /// Supports both interactive REPL mode and direct command execution.
        /// Automatically elevates to administrator privileges when required by specific commands.
        /// </summary>
        /// <param name="args">Command line arguments. Empty for REPL mode, or command followed by arguments for direct execution.</param>
        /// <returns>Exit code: 0 for success, non-zero for failure.</returns>
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            try
            {
                if (RequiresAdministrator(args))
                {
                    if (!IsAdministrator())
                    {
                        return RestartAsAdministrator(args);
                    }
                }

                LoadConfiguration();

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

                        var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        string command = split[0].ToLowerInvariant();
                        string[] commandArgs = split.Skip(1).ToArray();
                        
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
        /// Determines if a command requires administrator privileges to execute.
        /// </summary>
        /// <param name="args">Command line arguments where the first element is the command name.</param>
        /// <returns>True if the command requires administrator privileges, false otherwise.</returns>
        private static bool RequiresAdministrator(string[] args)
        {
            if (args.Length == 0)
                return false;

            string command = args[0].ToLowerInvariant();
            
            string[] adminCommands = {
                "install", "uninstall", "start", "stop", "restart", 
                "site", "enable", "disable", "service", "global"
            };

            return adminCommands.Contains(command);
        }

        /// <summary>
        /// Restarts the application with administrator privileges using UAC elevation.
        /// </summary>
        /// <param name="args">Original command line arguments to pass to the elevated process.</param>
        /// <returns>Exit code from the elevated process, or 1 if elevation failed.</returns>
        private static int RestartAsAdministrator(string[] args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "DevStack.exe"),
                    Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = true,
                    Verb = "runas"
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

        /// <summary>
        /// Checks if the current process is running with administrator privileges.
        /// </summary>
        /// <returns>True if running as administrator, false otherwise.</returns>
#pragma warning disable CA1416
        private static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
#pragma warning restore CA1416

        /// <summary>
        /// Loads application configuration including DevStack base configuration and localization settings.
        /// Initializes the localization manager with the saved language preference or defaults to pt_BR.
        /// </summary>
        private static void LoadConfiguration()
        {
            DevStackConfig.Initialize();
            
            string defaultLanguage = "pt_BR";
            string? savedLanguage = DevStackConfig.GetSetting("language") as string;
            string languageToUse = savedLanguage ?? defaultLanguage;
            
            _localizationManager = DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);
            DevStackShared.LocalizationManager.ApplyLanguage(languageToUse);
        }

        /// <summary>
        /// Writes an informational message to the console in cyan color.
        /// </summary>
        /// <param name="message">The message to display.</param>
        private static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes a warning message to the console in yellow color.
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        private static void WriteWarningMsg(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes an error message to the console in red color.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private static void WriteErrorMsg(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes a message to the console with a specified color.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="color">The console color to use for the message.</param>
        public static void WriteColoredLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Appends a timestamped log entry to the devstack.log file.
        /// Silently ignores any logging errors to prevent application interruption.
        /// </summary>
        /// <param name="message">The message to log.</param>
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
            }
        }

        /// <summary>
        /// Displays the installation status for a specific component.
        /// Shows all installed versions or a warning message if the component is not installed.
        /// </summary>
        /// <param name="component">The name of the component to check.</param>
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

        /// <summary>
        /// Displays the installation and running status for all DevStack components.
        /// For service components (PHP, Nginx), also indicates whether each version is running or stopped.
        /// </summary>
        private static void StatusAll()
        {
            Console.WriteLine(LocalizationManager.GetString("cli.status.title"));
            var allStatus = DevStackManager.DataManager.GetAllComponentsStatus();
            foreach (var comp in allStatus.Keys)
            {
                var status = allStatus[comp];
                if (status.Installed && status.Versions.Count > 0)
                {
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

        /// <summary>
        /// Tests all installed DevStack components by executing version commands.
        /// Verifies that each tool is properly installed and can be executed.
        /// </summary>
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

        /// <summary>
        /// Checks system dependencies required to run DevStack.
        /// Verifies administrator privileges and displays missing requirements.
        /// </summary>
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

        /// <summary>
        /// Centers text within a specified width by adding padding spaces.
        /// </summary>
        /// <param name="text">The text to center.</param>
        /// <param name="width">The total width to center the text within.</param>
        /// <returns>The centered text with appropriate padding.</returns>
        private static string CenterText(string text, int width)
        {
            int pad = Math.Max(0, width - text.Length);
            int padLeft = (int)Math.Floor(pad / 2.0);
            int padRight = pad - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        /// <summary>
        /// Updates a component by triggering its installation process.
        /// This effectively downloads and installs the latest version of the component.
        /// </summary>
        /// <param name="component">The name of the component to update.</param>
        private static void UpdateComponent(string component)
        {
            _ = HandleInstallCommand([component]);
        }

        /// <summary>
        /// Creates a command-line alias (batch file) for a specific component version.
        /// The alias allows quick access to the component executable from any directory.
        /// </summary>
        /// <param name="component">The name of the component to alias.</param>
        /// <param name="version">The version of the component to create an alias for.</param>
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

        /// <summary>
        /// Displays the CLI help information with all available commands and their descriptions.
        /// Shows a formatted table with color-coded command names and descriptions.
        /// </summary>
        private static void ShowUsage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(LocalizationManager.GetString("cli.commands.help_title"));
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine(LocalizationManager.GetString("cli.commands.gui_hint"));
            Console.WriteLine();

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
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("║");
                    Console.ResetColor();
                    
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

        /// <summary>
        /// Routes and executes the specified command with its arguments.
        /// Logs the command execution and returns the appropriate exit code.
        /// </summary>
        /// <param name="command">The command name to execute.</param>
        /// <param name="args">Arguments to pass to the command handler.</param>
        /// <returns>Exit code: 0 for success, non-zero for failure.</returns>
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

        /// <summary>
        /// Displays usage information and returns success code.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        private static int ShowUsageAndReturn()
        {
            ShowUsage();
            return 0;
        }

        /// <summary>
        /// Executes the status command to display all component statuses.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        private static int ExecuteStatusCommand()
        {
            StatusAll();
            return 0;
        }

        /// <summary>
        /// Executes the test command to verify all installed components.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        private static int ExecuteTestCommand()
        {
            TestAll();
            return 0;
        }

        /// <summary>
        /// Executes the dependencies check command.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        private static int ExecuteDepsCommand()
        {
            DepsCheck();
            return 0;
        }

        /// <summary>
        /// Handles the update command for one or more components.
        /// </summary>
        /// <param name="args">Array of component names to update.</param>
        /// <returns>Always returns 0.</returns>
        private static int HandleUpdateCommand(string[] args)
        {
            foreach (var component in args)
            {
                UpdateComponent(component);
            }
            return 0;
        }

        /// <summary>
        /// Handles unknown commands by displaying an error message.
        /// </summary>
        /// <param name="command">The unknown command that was attempted.</param>
        /// <returns>Always returns 1 to indicate error.</returns>
        private static int HandleUnknownCommand(string command)
        {
            Console.WriteLine(LocalizationManager.GetString("cli.commands.unknown", command));
            return 1;
        }

        /// <summary>
        /// Handles the list command to display available or installed versions of components.
        /// </summary>
        /// <param name="args">Command arguments. Use "--installed" to list installed versions, or component name to list available versions.</param>
        /// <returns>0 on success, 1 if arguments are invalid.</returns>
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

        /// <summary>
        /// Handles the site command to create an Nginx site configuration.
        /// Creates a site configuration file with the specified domain, root directory, PHP upstream, and Nginx version.
        /// </summary>
        /// <param name="args">Command arguments including domain, -root, -php, and -nginx flags with their values.</param>
        /// <returns>0 on success, 1 if arguments are invalid or configuration fails.</returns>
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

        /// <summary>
        /// Parses site command arguments into a SiteConfig object.
        /// Extracts domain, root directory, PHP upstream, and Nginx version from command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments for site configuration.</param>
        /// <returns>A SiteConfig object populated with the parsed values.</returns>
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
        /// Builds the PHP upstream address in the format 127.x.x.x:9000.
        /// </summary>
        /// <param name="value">The IP address suffix to append to 127.</param>
        /// <returns>A complete PHP upstream address string.</returns>
        private static string BuildPhpUpstream(string value)
        {
            return $"{PHP_UPSTREAM_IP_PREFIX}{value}{PHP_UPSTREAM_PORT}";
        }

        /// <summary>
        /// Validates the site configuration to ensure all required fields are present.
        /// </summary>
        /// <param name="config">The site configuration to validate.</param>
        /// <returns>True if configuration is valid, false otherwise.</returns>
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

        /// <summary>
        /// Internal class representing a site configuration with all necessary parameters
        /// for creating an Nginx site configuration file.
        /// </summary>
        private class SiteConfig
        {
            /// <summary>
            /// Domain name for the site configuration.
            /// </summary>
            public string Domain { get; set; } = string.Empty;
            
            /// <summary>
            /// Root directory path for the site.
            /// </summary>
            public string Root { get; set; } = string.Empty;
            
            /// <summary>
            /// PHP upstream configuration (IP:port).
            /// </summary>
            public string PhpUpstream { get; set; } = string.Empty;
            
            /// <summary>
            /// Nginx version for compatibility configuration.
            /// </summary>
            public string NginxVersion { get; set; } = string.Empty;
        }

        /// <summary>
        /// Handles the install command for one or more components.
        /// </summary>
        /// <param name="args">Array of component names to install.</param>
        /// <returns>Always returns 0.</returns>
        private static async Task<int> HandleInstallCommand(string[] args)
        {
            await InstallCommands(args);
            return 0;
        }

        /// <summary>
        /// Handles the uninstall command for one or more components.
        /// </summary>
        /// <param name="args">Array of component names or versions to uninstall.</param>
        /// <returns>Always returns 0.</returns>
        private static int HandleUninstallCommand(string[] args)
        {
            UninstallCommands(args);
            return 0;
        }

        /// <summary>
        /// Handles the path command to manage Windows PATH environment variable.
        /// Supports adding, removing, and listing DevStack paths.
        /// </summary>
        /// <param name="args">Subcommand and arguments: add, remove [paths...], list, or help.</param>
        /// <returns>0 on success, 1 on error.</returns>
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

        /// <summary>
        /// Handles the start command to start component services.
        /// Can start all services with --all flag, or a specific component version.
        /// </summary>
        /// <param name="args">Arguments: "--all" to start all services, or component name followed by version.</param>
        /// <returns>0 on success, 1 if arguments are invalid.</returns>
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

        /// <summary>
        /// Handles the stop command to stop running component services.
        /// Can stop all services with --all flag, or a specific component version.
        /// </summary>
        /// <param name="args">Arguments: "--all" to stop all services, or component name followed by version.</param>
        /// <returns>0 on success, 1 if arguments are invalid.</returns>
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

        /// <summary>
        /// Handles the restart command to restart running component services.
        /// Stops the service, waits briefly, then starts it again.
        /// Can restart all services with --all flag, or a specific component version.
        /// </summary>
        /// <param name="args">Arguments: "--all" to restart all services, or component name followed by version.</param>
        /// <returns>0 on success, 1 if arguments are invalid.</returns>
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

        /// <summary>
        /// Handles the alias command to create a command-line alias for a component version.
        /// </summary>
        /// <param name="args">Arguments: component name followed by version.</param>
        /// <returns>0 on success, 1 if arguments are invalid.</returns>
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

        /// <summary>
        /// Handles the self-update command to update DevStack itself via git pull.
        /// Only works if DevStack is installed from a git repository.
        /// </summary>
        /// <returns>Always returns 0.</returns>
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

        /// <summary>
        /// Handles the clean command to remove temporary files and logs.
        /// Deletes logs directory, tmp directory, and the main devstack.log file.
        /// </summary>
        /// <returns>Always returns 0.</returns>
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

        /// <summary>
        /// Handles the backup command to create a timestamped backup of configurations and logs.
        /// </summary>
        /// <returns>Always returns 0.</returns>
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

        /// <summary>
        /// Handles the logs command to display the last N lines of the devstack log file.
        /// </summary>
        /// <returns>Always returns 0.</returns>
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

        /// <summary>
        /// Handles the enable command to start a Windows service.
        /// </summary>
        /// <param name="args">Arguments: service name to enable.</param>
        /// <returns>0 on success, 1 if arguments are invalid.</returns>
        private static int HandleEnableCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.enable"));
                return 1;
            }

            string svc = args[0];
#pragma warning disable CA1416
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

        /// <summary>
        /// Handles the disable command to stop a Windows service.
        /// </summary>
        /// <param name="args">Arguments: service name to disable.</param>
        /// <returns>Always returns 0.</returns>
        private static int HandleDisableCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(LocalizationManager.GetString("cli.usage.disable"));
                return 1;
            }

            string svc = args[0];
#pragma warning disable CA1416
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

        /// <summary>
        /// Handles the config command to open the configurations directory in Windows Explorer.
        /// </summary>
        /// <returns>Always returns 0.</returns>
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

        /// <summary>
        /// Handles the reset command to uninstall and reinstall a component.
        /// Useful for fixing broken installations.
        /// </summary>
        /// <param name="args">Arguments: component name to reset.</param>
        /// <returns>0 on success, 1 if arguments are invalid.</returns>
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

        /// <summary>
        /// Handles the SSL command to generate SSL certificates for domains.
        /// </summary>
        /// <param name="args">Arguments: domain names for SSL certificate generation.</param>
        /// <returns>Always returns 0.</returns>
        private static async Task<int> HandleSslCommand(string[] args)
        {
            await GenerateManager.GenerateSslCertificate(args);
            return 0;
        }

        /// <summary>
        /// Handles database management commands for MySQL, PostgreSQL, and MongoDB.
        /// Supports list, create, and drop operations.
        /// </summary>
        /// <param name="args">Arguments: database type (mysql/pgsql/mongo), command (list/create/drop), and optional database name.</param>
        /// <returns>0 on success, 1 if arguments are invalid or database is not found.</returns>
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

        /// <summary>
        /// Handles the service command to list all DevStack-related Windows services.
        /// Displays service name, status, and display name in a formatted table.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        private static int HandleServiceCommand()
        {
#pragma warning disable CA1416
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

        /// <summary>
        /// Handles the doctor command to perform system diagnostics.
        /// Displays installed components, PATH environment variable entries, user information, and system details.
        /// Synchronizes the process PATH with user PATH settings.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        private static int HandleDoctorCommand()
        {
            Console.WriteLine(LocalizationManager.GetString("cli.doctor.title"));
            ListManager.ListInstalledVersions();

            RefreshProcessPathFromUser();
            WriteInfo(LocalizationManager.GetString("cli.doctor.path_synced"));

            var processPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            
            var processPathList = processPath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var userPathList = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var combinedPaths = processPathList.Concat(userPathList).Distinct().ToArray();
            
            int maxPathLen = combinedPaths.Length > 0 ? combinedPaths.Max(p => p.Length) : 20;
            string headerPath = string.Concat(Enumerable.Repeat('_', maxPathLen + 4));
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(headerPath);
            Console.Write("| ");
            Console.ForegroundColor = ConsoleColor.Gray;
            
            string fullHeaderText = LocalizationManager.GetString("cli.doctor.path_header");
            int devstackIndex = fullHeaderText.IndexOf("DevStack", StringComparison.OrdinalIgnoreCase);
            
            if (devstackIndex >= 0)
            {
                string beforeDevstack = fullHeaderText.Substring(0, devstackIndex);
                string afterDevstack = fullHeaderText.Substring(devstackIndex + 8);
                
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

        /// <summary>
        /// Handles the global command to add DevStack to the user's PATH environment variable.
        /// This allows DevStack commands to be run from any directory.
        /// </summary>
        /// <returns>Always returns 0.</returns>
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

        /// <summary>
        /// Handles the language command to display or change the application language.
        /// Without arguments, lists all available languages with the current one marked.
        /// With a language code argument, changes to that language and saves the preference.
        /// </summary>
        /// <param name="args">Optional language code to switch to (e.g., "en_US", "pt_BR").</param>
        /// <returns>0 on success, 1 if the specified language is not available.</returns>
        private static int HandleLanguageCommand(string[] args)
        {
            var localizationManager = DevStackShared.LocalizationManager.Instance 
                ?? DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);

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
                DevStackShared.LocalizationManager.ApplyLanguage(newLanguage);

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
        /// Synchronizes the current process's PATH environment variable with the user's PATH.
        /// Combines machine PATH and user PATH in the Windows default order and updates the current process.
        /// This ensures that changes to the user PATH are immediately reflected in the running process.
        /// </summary>
        private static void RefreshProcessPathFromUser()
        {
            try
            {
                var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
                var machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) ?? "";
                var processPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                
                var combinedPath = string.IsNullOrEmpty(machinePath) ? userPath : 
                                  string.IsNullOrEmpty(userPath) ? machinePath : 
                                  $"{machinePath};{userPath}";
                
                Environment.SetEnvironmentVariable("PATH", combinedPath);
                
                Program.WriteLog($"PATH do processo sincronizado com PATH do usuário. Total de entradas: {combinedPath.Split(';').Length}");
            }
            catch (Exception ex)
            {
                Program.WriteLog($"Erro ao sincronizar PATH: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the installation status for a specific component.
        /// Returns installation state, message, and list of installed versions.
        /// For service components, includes running/stopped status for each version.
        /// </summary>
        /// <param name="component">The component name to check status for.</param>
        /// <returns>Tuple containing installation status, status message, and array of installed versions.</returns>
        private static (bool installed, string message, string[] versions) GetComponentStatus(string component)
        {
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

        /// <summary>
        /// Gets installation status for all supported DevStack components.
        /// </summary>
        /// <returns>Dictionary mapping component names to their installation status tuples.</returns>
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

        /// <summary>
        /// Searches for a file matching a pattern in a directory.
        /// </summary>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="pattern">File name pattern to match (supports wildcards).</param>
        /// <param name="recursive">Whether to search subdirectories recursively.</param>
        /// <param name="pathFilter">Optional path filter to further narrow results.</param>
        /// <returns>The first matching file path, or null if not found.</returns>
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

        /// <summary>
        /// Recursively copies a directory and all its contents to a destination directory.
        /// </summary>
        /// <param name="sourceDir">The source directory path to copy from.</param>
        /// <param name="destinationDir">The destination directory path to copy to.</param>
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
        
        /// <summary>
        /// Installs one or more components using the InstallManager.
        /// After successful installation, updates the PATH environment variable with component bin directories.
        /// </summary>
        /// <param name="args">Array of component names to install.</param>
        private static async Task InstallCommands(string[] args) 
        {
            await InstallManager.InstallCommands(args);
            
            if (pathManager != null)
            {
                pathManager.AddBinDirsToPath();
            }
        }
        
        /// <summary>
        /// Uninstalls one or more components or specific component versions.
        /// Parses component-version strings in format "component-version" and extracts version information.
        /// Supports explicit version targeting (e.g., "php-8.4.10") or component name with separate version argument.
        /// </summary>
        /// <param name="args">Array of component names or component-version strings to uninstall.</param>
        private static void UninstallCommands(string[] args) 
        { 
            var specificVersions = new List<string>();
            
            foreach (string arg in args)
            {
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
                else
                {
                    int currentIndex = Array.IndexOf(args, arg);
                    if (currentIndex >= 0 && currentIndex + 1 < args.Length)
                    {
                        string nextArg = args[currentIndex + 1];
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