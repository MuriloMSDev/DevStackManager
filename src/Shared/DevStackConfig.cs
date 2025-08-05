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
        public static string openSSLDir = "";
        public static string phpcsfixerDir = "";
        public static string dbeaverDir = "";
        public static string nginxSitesDir = "";
        public static string tmpDir = "";
        
        public static PathManager? pathManager;

        /// <summary>
        /// Lista de comandos disponíveis no DevStack CLI (compartilhada entre CLI e GUI)
        /// </summary>
        public static readonly (string cmd, string desc)[] cmds = new[]
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
            ("site <domínio> [opções]", "Cria configuração de site nginx."),
            ("help", "Exibe esta ajuda.")
        };

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
            baseDir = "C:\\devstack";
            phpDir = Path.Combine(baseDir, "php");
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
            nginxSitesDir = "conf\\sites-enabled";
            tmpDir = Path.Combine(baseDir, "tmp");

            // Inicializar PathManager
            pathManager = new PathManager(baseDir, phpDir, nodeDir, pythonDir, nginxDir, mysqlDir);
        }
        
        /// <summary>
        /// Escreve mensagem no log
        /// </summary>
        public static void WriteLog(string message)
        {
            try
            {
                var logFile = Path.Combine(baseDir, "devstack.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logFile) ?? baseDir);
                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
            }
            catch
            {
                // Ignore log errors
            }
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
            var cmdList = cmds.Select(c => new { cmd = c.cmd, desc = c.desc }).ToArray();
            var result = new System.Text.StringBuilder();

            int col1 = 50, col2 = 60;
            
            // Cabeçalho da tabela com box-drawing
            result.AppendLine($"╔{new string('═', col1)}╦{new string('═', col2)}╗");
            result.AppendLine($"║{CenterText("Comando", col1)}║{CenterText("Descrição", col2)}║");
            result.AppendLine($"╠{new string('═', col1)}╬{new string('═', col2)}╣");

            foreach (var c in cmdList)
            {
                result.AppendLine($"║{CenterText(c.cmd, col1)}║{CenterText(c.desc, col2)}║");
            }
            
            // Rodapé da tabela com box-drawing
            result.AppendLine($"╚{new string('═', col1)}╩{new string('═', col2)}╝");
            
            return result.ToString();
        }
    }
}
