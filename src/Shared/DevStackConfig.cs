using System;
using System.IO;
using System.Linq;

namespace DevStackManager
{
    /// <summary>
    /// Centralized configuration management class for DevStack.
    /// Contains shared settings, directory paths, logging utilities, and console output helpers.
    /// </summary>
    public static class DevStackConfig
    {
        /// <summary>
        /// Default directory name for storing development tools.
        /// </summary>
        private const string TOOLS_DIRECTORY = "tools";
        
        /// <summary>
        /// Relative path to Nginx sites-enabled configuration directory.
        /// </summary>
        private const string SITES_ENABLED_PATH = "conf\\sites-enabled";
        
        /// <summary>
        /// File name for DevStack application log.
        /// </summary>
        private const string LOG_FILE_NAME = "devstack.log";
        
        /// <summary>
        /// File name for DevStack configuration settings.
        /// </summary>
        private const string SETTINGS_FILE_NAME = "settings.conf";
        
        /// <summary>
        /// Timestamp format used in log entries (yyyy-MM-dd HH:mm:ss).
        /// </summary>
        private const string LOG_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";
        
        /// <summary>
        /// Width of command column in help table display.
        /// </summary>
        private const int HELP_TABLE_CMD_WIDTH = 50;
        
        /// <summary>
        /// Width of description column in help table display.
        /// </summary>
        private const int HELP_TABLE_DESC_WIDTH = 60;

        /// <summary>
        /// Base directory for all DevStack tools and components.
        /// </summary>
        public static string baseDir = "";
        
        /// <summary>
        /// Directory containing PHP installations.
        /// </summary>
        public static string phpDir = "";
        
        /// <summary>
        /// Directory containing Nginx installations.
        /// </summary>
        public static string nginxDir = "";
        
        /// <summary>
        /// Directory containing MySQL installations.
        /// </summary>
        public static string mysqlDir = "";
        
        /// <summary>
        /// Directory containing Node.js installations.
        /// </summary>
        public static string nodeDir = "";
        
        /// <summary>
        /// Directory containing Python installations.
        /// </summary>
        public static string pythonDir = "";
        
        /// <summary>
        /// Directory containing Composer installations.
        /// </summary>
        public static string composerDir = "";
        
        /// <summary>
        /// Directory containing phpMyAdmin installations.
        /// </summary>
        public static string pmaDir = "";
        
        /// <summary>
        /// Directory containing MongoDB installations.
        /// </summary>
        public static string mongoDir = "";
        
        /// <summary>
        /// Directory containing PostgreSQL installations.
        /// </summary>
        public static string pgsqlDir = "";
        
        /// <summary>
        /// Directory containing Elasticsearch installations.
        /// </summary>
        public static string elasticDir = "";
        
        /// <summary>
        /// Directory containing WP-CLI installations.
        /// </summary>
        public static string wpcliDir = "";
        
        /// <summary>
        /// Directory containing Adminer installations.
        /// </summary>
        public static string adminerDir = "";
        
        /// <summary>
        /// Directory containing Go installations.
        /// </summary>
        public static string goDir = "";
        
        /// <summary>
        /// Directory containing Git installations.
        /// </summary>
        public static string gitDir = "";
        
        /// <summary>
        /// Directory containing OpenSSL installations.
        /// </summary>
        public static string openSSLDir = "";
        
        /// <summary>
        /// Directory containing PHP-CS-Fixer installations.
        /// </summary>
        public static string phpcsfixerDir = "";
        
        /// <summary>
        /// Directory containing DBeaver installations.
        /// </summary>
        public static string dbeaverDir = "";
        
        /// <summary>
        /// Directory containing Nginx site configurations.
        /// </summary>
        public static string nginxSitesDir = "";
        
        /// <summary>
        /// Temporary files directory.
        /// </summary>
        public static string tmpDir = "";
        
        /// <summary>
        /// PATH environment variable manager instance.
        /// </summary>
        public static PathManager? pathManager;

        /// <summary>
        /// List of available CLI commands. Initialized via GetLocalizedCommands() with localized descriptions.
        /// </summary>
        public static readonly (string cmd, string desc)[] cmds = null!;

        /// <summary>
        /// Returns localized command descriptions based on the current language setting.
        /// Falls back to default English commands if localization is unavailable.
        /// </summary>
        /// <returns>An array of command/description tuples.</returns>
        public static (string cmd, string desc)[] GetLocalizedCommands()
        {
            var lm = DevStackShared.LocalizationManager.Instance;
            if (lm == null)
                return GetDefaultCommands();

            return new[]
            {
                ("install <componente> [versão]", lm.GetString("cli.commands.help_install")),
                ("uninstall <componente> [versão]", lm.GetString("cli.commands.help_uninstall")),
                ("list <componente|--installed>", lm.GetString("cli.commands.help_list")),
                ("path [add|remove|list|help]", lm.GetString("cli.commands.help_path")),
                ("status", lm.GetString("cli.commands.help_status")),
                ("test", lm.GetString("cli.commands.help_test")),
                ("update <componente>", lm.GetString("cli.commands.help_update")),
                ("deps", lm.GetString("cli.commands.help_deps")),
                ("alias <componente> <versão>", lm.GetString("cli.commands.help_alias")),
                ("global", lm.GetString("cli.commands.help_global")),
                ("self-update", lm.GetString("cli.commands.help_self_update")),
                ("clean", lm.GetString("cli.commands.help_clean")),
                ("backup", lm.GetString("cli.commands.help_backup")),
                ("logs", lm.GetString("cli.commands.help_logs")),
                ("enable <serviço>", lm.GetString("cli.commands.help_enable")),
                ("disable <serviço>", lm.GetString("cli.commands.help_disable")),
                ("config", lm.GetString("cli.commands.help_config")),
                ("reset <componente>", lm.GetString("cli.commands.help_reset")),
                ("ssl <domínio> [-openssl <versão>]", lm.GetString("cli.commands.help_ssl")),
                ("db <mysql|pgsql|mongo> <comando> [args...]", lm.GetString("cli.commands.help_db")),
                ("service", lm.GetString("cli.commands.help_service")),
                ("doctor", lm.GetString("cli.commands.help_doctor")),
                ("language [código]", lm.GetString("cli.commands.help_language")),
                ("site <domínio> [opções]", lm.GetString("cli.commands.help_site")),
                ("help", lm.GetString("cli.commands.help_help"))
            };
        }

        /// <summary>
        /// Returns default (fallback) command descriptions in English when localization is unavailable.
        /// </summary>
        /// <returns>An array of command/description tuples.</returns>
        private static (string cmd, string desc)[] GetDefaultCommands()
        {
            return new[]
            {
                ("install <componente> [versão]", "Installs a tool or specific version."),
                ("uninstall <componente> [versão]", "Removes a tool or specific version."),
                ("list <componente|--installed>", "Lists available or installed versions."),
                ("path [add|remove|list|help]", "Manages PATH for installed tools."),
                ("status", "Shows status of all tools."),
                ("test", "Tests all installed tools."),
                ("update <componente>", "Updates a tool to the latest version."),
                ("deps", "Checks system dependencies."),
                ("alias <componente> <versão>", "Creates a .bat alias for the tool version."),
                ("global", "Adds DevStack to PATH and creates global alias."),
                ("self-update", "Updates DevStackManager."),
                ("clean", "Removes logs and temporary files."),
                ("backup", "Creates backup of configs and logs."),
                ("logs", "Displays recent log entries."),
                ("enable <serviço>", "Enables a Windows service."),
                ("disable <serviço>", "Disables a Windows service."),
                ("config", "Opens the configuration directory."),
                ("reset <componente>", "Removes and reinstalls a tool."),
                ("ssl <domínio> [-openssl <versão>]", "Generates self-signed SSL certificate."),
                ("db <mysql|pgsql|mongo> <comando> [args...]", "Manages basic database operations."),
                ("service", "Lists DevStack services (Windows)."),
                ("doctor", "DevStack environment diagnostic."),
                ("language [código]", "Lists or changes interface language."),
                ("site <domínio> [opções]", "Creates nginx site configuration."),
                ("help", "Displays this help.")
            };
        }

        /// <summary>
        /// Array of all supported component names for validation and iteration.
        /// </summary>
        public static readonly string[] components = new[]
        {
            "php", "nginx", "mysql", "node", "python", "composer", "phpmyadmin", 
            "git", "mongodb", "pgsql", "elasticsearch", "wpcli", "adminer",
            "go", "openssl", "phpcsfixer", "dbeaver"
        };

        /// <summary>
        /// Initializes all directory configuration paths based on the application base directory.
        /// Creates the PathManager instance for managing system PATH modifications.
        /// </summary>
        public static void Initialize()
        {
            string exeDirectory = System.AppContext.BaseDirectory;
            baseDir = Path.Combine(exeDirectory, TOOLS_DIRECTORY);
            
            InitializeComponentDirectories();
            InitializeSpecialDirectories();
            InitializePathManager();
        }

        /// <summary>
        /// Initializes the directory paths for all supported components.
        /// </summary>
        private static void InitializeComponentDirectories()
        {
            phpDir = Path.Combine(baseDir, "php");
            gitDir = Path.Combine(baseDir, "git");
            nginxDir = Path.Combine(baseDir, "nginx");
            mysqlDir = Path.Combine(baseDir, "mysql");
            nodeDir = Path.Combine(baseDir, "node");
            pythonDir = Path.Combine(baseDir, "python");
            composerDir = Path.Combine(baseDir, "composer");
            pmaDir = Path.Combine(baseDir, "phpmyadmin");
            mongoDir = Path.Combine(baseDir, "mongodb");
            pgsqlDir = Path.Combine(baseDir, "pgsql");
            elasticDir = Path.Combine(baseDir, "elasticsearch");
            wpcliDir = Path.Combine(baseDir, "wpcli");
            adminerDir = Path.Combine(baseDir, "adminer");
            goDir = Path.Combine(baseDir, "go");
            openSSLDir = Path.Combine(baseDir, "openssl");
            phpcsfixerDir = Path.Combine(baseDir, "phpcsfixer");
            dbeaverDir = Path.Combine(baseDir, "dbeaver");
        }

        /// <summary>
        /// Initializes special-purpose directories (nginx sites, temp folder).
        /// </summary>
        private static void InitializeSpecialDirectories()
        {
            nginxSitesDir = SITES_ENABLED_PATH;
            tmpDir = Path.Combine(baseDir, "tmp");
        }

        /// <summary>
        /// Creates and initializes the PathManager instance for managing system PATH environment variable.
        /// </summary>
        private static void InitializePathManager()
        {
            pathManager = new PathManager(baseDir, phpDir, nodeDir, pythonDir, nginxDir, mysqlDir);
        }
        
        /// <summary>
        /// Writes a message to the DevStack log file with timestamp.
        /// Errors during logging are silently ignored.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void WriteLog(string message)
        {
            try
            {
                var logFile = GetLogFilePath();
                var logEntry = FormatLogEntry(message);
                File.AppendAllText(logFile, logEntry);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gets the full path to the DevStack log file.
        /// </summary>
        /// <returns>The log file path.</returns>
        private static string GetLogFilePath()
        {
            return Path.Combine(System.AppContext.BaseDirectory, LOG_FILE_NAME);
        }

        /// <summary>
        /// Formats a log message with timestamp.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <returns>The formatted log entry string.</returns>
        private static string FormatLogEntry(string message)
        {
            return $"[{DateTime.Now.ToString(LOG_TIMESTAMP_FORMAT)}] {message}\n";
        }
        
        /// <summary>
        /// Writes a colored line to the console. Falls back to plain text if color output fails.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="color">The console color to use.</param>
        public static void WriteColoredLine(string message, ConsoleColor color)
        {
            try
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
            catch
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Centers text within a specified width by adding padding on both sides.
        /// </summary>
        /// <param name="text">The text to center.</param>
        /// <param name="width">The total width for centering.</param>
        /// <returns>The centered text string.</returns>
        private static string CenterText(string text, int width)
        {
            int pad = Math.Max(0, width - text.Length);
            int padLeft = (int)Math.Floor(pad / 2.0);
            int padRight = pad - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        /// <summary>
        /// Generates a formatted help table displaying all available commands with box-drawing characters.
        /// </summary>
        /// <returns>The formatted help table as a string.</returns>
        public static string ShowHelpTable()
        {
            var result = new System.Text.StringBuilder();

            AppendTableHeader(result);
            AppendTableRows(result);
            AppendTableFooter(result);

            return result.ToString();
        }

        /// <summary>
        /// Appends the table header with localized column titles.
        /// </summary>
        /// <param name="result">The StringBuilder to append to.</param>
        private static void AppendTableHeader(System.Text.StringBuilder result)
        {
            var lm = DevStackShared.LocalizationManager.Instance;
            string cmdHeader = lm?.GetString("cli.commands.table_header_cmd") ?? "Command";
            string descHeader = lm?.GetString("cli.commands.table_header_desc") ?? "Description";
            
            result.AppendLine($"╔{new string('═', HELP_TABLE_CMD_WIDTH)}╦{new string('═', HELP_TABLE_DESC_WIDTH)}╗");
            result.AppendLine($"║{CenterText(cmdHeader, HELP_TABLE_CMD_WIDTH)}║{CenterText(descHeader, HELP_TABLE_DESC_WIDTH)}║");
            result.AppendLine($"╠{new string('═', HELP_TABLE_CMD_WIDTH)}╬{new string('═', HELP_TABLE_DESC_WIDTH)}╣");
        }

        /// <summary>
        /// Appends the table rows with localized command information.
        /// </summary>
        /// <param name="result">The StringBuilder to append to.</param>
        private static void AppendTableRows(System.Text.StringBuilder result)
        {
            var commands = GetLocalizedCommands();
            foreach (var cmd in commands)
            {
                result.AppendLine($"║{CenterText(cmd.cmd, HELP_TABLE_CMD_WIDTH)}║{CenterText(cmd.desc, HELP_TABLE_DESC_WIDTH)}║");
            }
        }

        /// <summary>
        /// Appends the table footer.
        /// </summary>
        /// <param name="result">The StringBuilder to append to.</param>
        private static void AppendTableFooter(System.Text.StringBuilder result)
        {
            result.AppendLine($"╚{new string('═', HELP_TABLE_CMD_WIDTH)}╩{new string('═', HELP_TABLE_DESC_WIDTH)}╝");
        }

        /// <summary>
        /// Persists a generic setting to the settings.conf file.
        /// Failures are silently ignored to prevent blocking the UI.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The setting value.</param>
        /// <param name="settingsDir">Optional directory for the settings file. Defaults to application base directory.</param>
        public static void PersistSetting(string key, object value, string? settingsDir = null)
        {
            var settingsPath = GetSettingsFilePath(settingsDir);
            try
            {
                var settingsObj = LoadOrCreateSettings(settingsPath);
                UpdateSetting(settingsObj, key, value);
                SaveSettings(settingsPath, settingsObj);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gets the full path to the settings.conf file.
        /// </summary>
        /// <param name="settingsDir">Optional directory for the settings file.</param>
        /// <returns>The settings file path.</returns>
        private static string GetSettingsFilePath(string? settingsDir)
        {
            var baseDir = settingsDir ?? System.AppContext.BaseDirectory;
            return Path.Combine(baseDir, SETTINGS_FILE_NAME);
        }

        /// <summary>
        /// Loads existing settings from the settings file or creates a new empty settings object.
        /// </summary>
        /// <param name="settingsPath">The path to the settings file.</param>
        /// <returns>A JObject containing the settings.</returns>
        private static Newtonsoft.Json.Linq.JObject LoadOrCreateSettings(string settingsPath)
        {
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                return Newtonsoft.Json.Linq.JObject.Parse(json);
            }
            return new Newtonsoft.Json.Linq.JObject();
        }

        /// <summary>
        /// Updates a setting key/value pair in the settings object. Converts enums to lowercase strings.
        /// </summary>
        /// <param name="settingsObj">The settings object to update.</param>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The setting value.</param>
        private static void UpdateSetting(Newtonsoft.Json.Linq.JObject settingsObj, string key, object value)
        {
            settingsObj[key] = value is Enum
                ? (value as Enum)?.ToString()?.ToLower() ?? string.Empty
                : Newtonsoft.Json.Linq.JToken.FromObject(value ?? string.Empty);
        }

        /// <summary>
        /// Saves the settings object to the settings file with formatted JSON.
        /// </summary>
        /// <param name="settingsPath">The path to the settings file.</param>
        /// <param name="settingsObj">The settings object to save.</param>
        private static void SaveSettings(string settingsPath, Newtonsoft.Json.Linq.JObject settingsObj)
        {
            using (var sw = new StreamWriter(settingsPath))
            using (var writer = new Newtonsoft.Json.JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.Indented })
            {
                settingsObj.WriteTo(writer);
            }
        }

        /// <summary>
        /// Reads settings from settings.conf. Returns a specific value if key is provided, or all settings as a Dictionary.
        /// </summary>
        /// <param name="key">Optional setting key. Can be a string, string array, or null for all settings.</param>
        /// <param name="settingsDir">Optional directory for the settings file.</param>
        /// <returns>The requested setting value(s), or null if not found.</returns>
        public static object? GetSetting(object? key = null, string? settingsDir = null)
        {
            var baseDir = settingsDir ?? System.AppContext.BaseDirectory;
            var settingsPath = System.IO.Path.Combine(baseDir, "settings.conf");
            if (!System.IO.File.Exists(settingsPath))
                return null;
            try
            {
                using (var sr = new System.IO.StreamReader(settingsPath))
                using (var reader = new Newtonsoft.Json.JsonTextReader(sr))
                {
                    var serializer = new Newtonsoft.Json.JsonSerializer();
                    var dict = serializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(reader);
                    if (dict == null)
                        return null;

                    if (key == null)
                        return dict;

                    if (key is string strKey)
                        return dict.TryGetValue(strKey, out var value) ? value : null;

                    var arrKeys = key as string[];
                    if (arrKeys != null)
                    {
                        var result = new System.Collections.Generic.Dictionary<string, object?>();
                        foreach (var k in arrKeys)
                        {
                            result[k] = dict.TryGetValue(k, out var v) ? v : null;
                        }
                        return result;
                    }

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
