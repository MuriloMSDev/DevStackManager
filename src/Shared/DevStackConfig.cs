using System;
using System.IO;
using System.Linq;
using System.Linq;

namespace DevStackManager
{
    /// <summary>
    /// Classe que contém as configurações e diretórios compartilhados entre CLI e GUI
    /// </summary>
    public static class DevStackConfig
    {
        // Constantes
        private const string TOOLS_DIRECTORY = "tools";
        private const string SITES_ENABLED_PATH = "conf\\sites-enabled";
        private const string LOG_FILE_NAME = "devstack.log";
        private const string SETTINGS_FILE_NAME = "settings.conf";
        private const string LOG_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";
        private const int HELP_TABLE_CMD_WIDTH = 50;
        private const int HELP_TABLE_DESC_WIDTH = 60;

        // Diretórios públicos
        public static string baseDir = "";
        public static string phpDir = "";
        public static string nginxDir = "";
        public static string mysqlDir = "";
        public static string nodeDir = "";
        public static string pythonDir = "";
        public static string composerDir = "";
        public static string pmaDir = "";
        public static string mongoDir = "";
        public static string pgsqlDir = "";
        public static string elasticDir = "";
        public static string wpcliDir = "";
        public static string adminerDir = "";
        public static string goDir = "";
        public static string gitDir = "";
        public static string openSSLDir = "";
        public static string phpcsfixerDir = "";
        public static string dbeaverDir = "";
        public static string nginxSitesDir = "";
        public static string tmpDir = "";
        
        public static PathManager? pathManager;

        /// <summary>
        /// Lista de comandos disponíveis no DevStack CLI (compartilhada entre CLI e GUI)
        /// </summary>
        public static readonly (string cmd, string desc)[] cmds = null!; // Será inicializado via GetLocalizedCommands()

        /// <summary>
        /// Retorna os comandos traduzidos baseado no idioma atual
        /// </summary>
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

        private static (string cmd, string desc)[] GetDefaultCommands()
        {
            return new[]
            {
                ("install <componente> [versão]", "Instala uma ferramenta ou versão específica."),
                ("uninstall <componente> [versão]", "Remove uma ferramenta ou versão específica."),
                ("list <componente|--installed>", "Lista versões disponíveis ou instaladas."),
                ("path [add|remove|list|help]", "Gerencia PATH das ferramentas instaladas."),
                ("status", "Mostra status de todas as ferramentas."),
                ("test", "Testa todas as ferramentas instaladas."),
                ("update <componente>", "Atualiza uma ferramenta para a última versão."),
                ("deps", "Verifica dependências do sistema."),
                ("alias <componente> <versão>", "Cria um alias .bat para a versão da ferramenta."),
                ("global", "Adiciona DevStack ao PATH e cria alias global."),
                ("self-update", "Atualiza o DevStackManager."),
                ("clean", "Remove logs e arquivos temporários."),
                ("backup", "Cria backup das configs e logs."),
                ("logs", "Exibe as últimas linhas do log."),
                ("enable <serviço>", "Ativa um serviço do Windows."),
                ("disable <serviço>", "Desativa um serviço do Windows."),
                ("config", "Abre o diretório de configuração."),
                ("reset <componente>", "Remove e reinstala uma ferramenta."),
                ("ssl <domínio> [-openssl <versão>]", "Gera certificado SSL autoassinado."),
                ("db <mysql|pgsql|mongo> <comando> [args...]", "Gerencia bancos de dados básicos."),
                ("service", "Lista serviços DevStack (Windows)."),
                ("doctor", "Diagnóstico do ambiente DevStack."),
                ("language [código]", "Lista ou altera o idioma da interface."),
                ("site <domínio> [opções]", "Cria configuração de site nginx."),
                ("help", "Exibe esta ajuda.")
            };
        }

        public static readonly string[] components = new[]
        {
            "php", "nginx", "mysql", "node", "python", "composer", "phpmyadmin", 
            "git", "mongodb", "pgsql", "elasticsearch", "wpcli", "adminer",
            "go", "openssl", "phpcsfixer", "dbeaver"
        };

        /// <summary>
        /// Inicializa todas as configurações de diretórios
        /// </summary>
        public static void Initialize()
        {
            string exeDirectory = System.AppContext.BaseDirectory;
            baseDir = Path.Combine(exeDirectory, TOOLS_DIRECTORY);
            
            InitializeComponentDirectories();
            InitializeSpecialDirectories();
            InitializePathManager();
        }

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

        private static void InitializeSpecialDirectories()
        {
            nginxSitesDir = SITES_ENABLED_PATH;
            tmpDir = Path.Combine(baseDir, "tmp");
        }

        private static void InitializePathManager()
        {
            pathManager = new PathManager(baseDir, phpDir, nodeDir, pythonDir, nginxDir, mysqlDir);
        }
        
        /// <summary>
        /// Escreve mensagem no log
        /// </summary>
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
                // Ignore log errors
            }
        }

        private static string GetLogFilePath()
        {
            return Path.Combine(System.AppContext.BaseDirectory, LOG_FILE_NAME);
        }

        private static string FormatLogEntry(string message)
        {
            return $"[{DateTime.Now.ToString(LOG_TIMESTAMP_FORMAT)}] {message}\n";
        }
        
        /// <summary>
        /// Escreve linha colorida no console (para compatibilidade com CLI)
        /// </summary>
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
                // Fallback para texto simples
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Centraliza texto dentro de uma largura específica
        /// </summary>
        private static string CenterText(string text, int width)
        {
            int pad = Math.Max(0, width - text.Length);
            int padLeft = (int)Math.Floor(pad / 2.0);
            int padRight = pad - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        /// <summary>
        /// Retorna a tabela de comandos com box-drawing characters como string
        /// </summary>
        public static string ShowHelpTable()
        {
            var result = new System.Text.StringBuilder();

            AppendTableHeader(result);
            AppendTableRows(result);
            AppendTableFooter(result);

            return result.ToString();
        }

        private static void AppendTableHeader(System.Text.StringBuilder result)
        {
            var lm = DevStackShared.LocalizationManager.Instance;
            string cmdHeader = lm?.GetString("cli.commands.table_header_cmd") ?? "Comando";
            string descHeader = lm?.GetString("cli.commands.table_header_desc") ?? "Descrição";
            
            result.AppendLine($"╔{new string('═', HELP_TABLE_CMD_WIDTH)}╦{new string('═', HELP_TABLE_DESC_WIDTH)}╗");
            result.AppendLine($"║{CenterText(cmdHeader, HELP_TABLE_CMD_WIDTH)}║{CenterText(descHeader, HELP_TABLE_DESC_WIDTH)}║");
            result.AppendLine($"╠{new string('═', HELP_TABLE_CMD_WIDTH)}╬{new string('═', HELP_TABLE_DESC_WIDTH)}╣");
        }

        private static void AppendTableRows(System.Text.StringBuilder result)
        {
            var commands = GetLocalizedCommands();
            foreach (var cmd in commands)
            {
                result.AppendLine($"║{CenterText(cmd.cmd, HELP_TABLE_CMD_WIDTH)}║{CenterText(cmd.desc, HELP_TABLE_DESC_WIDTH)}║");
            }
        }

        private static void AppendTableFooter(System.Text.StringBuilder result)
        {
            result.AppendLine($"╚{new string('═', HELP_TABLE_CMD_WIDTH)}╩{new string('═', HELP_TABLE_DESC_WIDTH)}╝");
        }

        /// <summary>
        /// Persistir configuração genérica em settings.conf
        /// </summary>
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
                // Silencioso para não travar a UI caso não tenha permissão
            }
        }

        private static string GetSettingsFilePath(string? settingsDir)
        {
            var baseDir = settingsDir ?? System.AppContext.BaseDirectory;
            return Path.Combine(baseDir, SETTINGS_FILE_NAME);
        }

        private static Newtonsoft.Json.Linq.JObject LoadOrCreateSettings(string settingsPath)
        {
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                return Newtonsoft.Json.Linq.JObject.Parse(json);
            }
            return new Newtonsoft.Json.Linq.JObject();
        }

        private static void UpdateSetting(Newtonsoft.Json.Linq.JObject settingsObj, string key, object value)
        {
            settingsObj[key] = value is Enum
                ? (value as Enum)?.ToString()?.ToLower() ?? string.Empty
                : Newtonsoft.Json.Linq.JToken.FromObject(value ?? string.Empty);
        }

        private static void SaveSettings(string settingsPath, Newtonsoft.Json.Linq.JObject settingsObj)
        {
            using (var sw = new StreamWriter(settingsPath))
            using (var writer = new Newtonsoft.Json.JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.Indented })
            {
                settingsObj.WriteTo(writer);
            }
        }

        /// <summary>
        /// Lê configuração do settings.conf. Se key for fornecida, retorna apenas o valor; senão retorna todas as configurações como Dictionary.
        /// </summary>
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

                    // Se key for null, retorna todas as settings
                    if (key == null)
                        return dict;

                    // Se key for string, retorna apenas o valor
                    if (key is string strKey)
                        return dict.TryGetValue(strKey, out var value) ? value : null;

                    // Se key for array de string, retorna dicionário apenas com as chaves solicitadas
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

                    // Se key for outro tipo, retorna null
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
