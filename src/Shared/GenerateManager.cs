using System;
using System.IO;
using System.Threading.Tasks;

namespace DevStackManager
{
    public static class GenerateManager
    {
        /// <summary>
        /// Gera um certificado SSL para o domínio especificado usando OpenSSL
        /// </summary>
        public static async Task GenerateSslCertificate(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager ssl <dominio> [-openssl <versao>]");
                return;
            }

            string domain = args[0];
            string? opensslVersion = null;
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-openssl" && (i + 1) < args.Length)
                {
                    opensslVersion = args[i + 1];
                    break;
                }
            }
            string sslDir = Path.Combine(DevStackConfig.baseDir, "configs", "nginx", "ssl");
            if (!Directory.Exists(sslDir))
            {
                Directory.CreateDirectory(sslDir);
            }

            string crt = Path.Combine(sslDir, $"{domain}.crt");
            string key = Path.Combine(sslDir, $"{domain}.key");

            if (string.IsNullOrEmpty(opensslVersion))
            {
                var opensslComponent = Components.ComponentsFactory.GetComponent("openssl");
                if (opensslComponent != null)
                {
                    opensslVersion = opensslComponent.GetLatestVersion();
                }
                else
                {
                    Console.WriteLine("Versão do OpenSSL não especificada.");
                    return;
                }
            }

            string dir = Path.Combine(DevStackConfig.openSSLDir, $"openssl-{opensslVersion}", "bin");
            string opensslExe = Path.Combine(dir, "openssl.exe");

            if (!File.Exists(opensslExe))
            {
                // Tenta instalar o OpenSSL se não existir
                await InstallManager.InstallCommands(["openssl", opensslVersion]);
            }

            if (!File.Exists(opensslExe))
            {
                Console.WriteLine("OpenSSL não encontrado.");
                return;
            }

            // Executa o comando para gerar o certificado
            // Cria arquivo de configuração temporário para SAN
            string opensslConfigPath = Path.Combine(sslDir, $"openssl-{domain}-san.conf");
            File.WriteAllText(opensslConfigPath, $"[req]\ndistinguished_name = req_distinguished_name\nreq_extensions = v3_req\n[req_distinguished_name]\n[v3_req]\nsubjectAltName = @alt_names\n[alt_names]\nDNS.1 = {domain}\n");

            // Executa o comando para gerar o certificado com SAN
            await ProcessManager.ExecuteProcessAsync(opensslExe, $"req -x509 -nodes -days 730 -newkey rsa:2048 -keyout \"{key}\" -out \"{crt}\" -subj \"/CN={domain}\" -extensions v3_req -config \"{opensslConfigPath}\"");

            // Remove arquivo temporário
            if (File.Exists(opensslConfigPath)) File.Delete(opensslConfigPath);

            bool success = File.Exists(crt) && File.Exists(key);
            if (success)
            {
                Console.WriteLine($"Certificado SSL gerado para {domain}.");

                // Instala o certificado na store de autoridades confiáveis do Windows
                try
                {
                    // Remove certificado antigo (se existir) e adiciona o novo usando certutil
                    var subject = domain;
                    var delPsi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "certutil.exe",
                        Arguments = $"-delstore Root {subject}",
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = true
                    };
                    System.Diagnostics.Process.Start(delPsi);

                    var addPsi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "certutil.exe",
                        Arguments = $"-addstore Root \"{crt}\"",
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = true
                    };
                    System.Diagnostics.Process.Start(addPsi);
                    Console.WriteLine($"Certificado removido (se existia) e instalado na store de autoridades confiáveis do Windows.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Falha ao instalar certificado na store do Windows: {ex.Message}");
                }

                // Descobre em qual nginxVersion o domínio está sendo executado
                string foundConfPath = string.Empty;
                string foundNginxVersion = string.Empty;
                var nginxRootDir = DevStackConfig.nginxDir;
                if (Directory.Exists(nginxRootDir))
                {
                    var nginxVersions = Directory.GetDirectories(nginxRootDir, "nginx-*");
                    foreach (var nginxVersionDir in nginxVersions)
                    {
                        string versionName = (Path.GetFileName(nginxVersionDir) ?? string.Empty).Replace("nginx-", "");
                        string sitesDir = Path.Combine(nginxVersionDir, DevStackConfig.nginxSitesDir);
                        string confPath = Path.Combine(sitesDir, $"{domain}.conf");
                        if (File.Exists(confPath))
                        {
                            foundConfPath = confPath;
                            foundNginxVersion = versionName;
                            break;
                        }
                    }
                }

                if (foundConfPath != null)
                {
                    var lines = File.ReadAllLines(foundConfPath).ToList();
                    int insertIndex = lines.FindIndex(l => l.Trim().StartsWith("server {"));
                    if (insertIndex == -1) insertIndex = 0;
                    string listenSslLine1 = $"    listen 443 ssl;";
                    string listenSslLine2 = $"    listen [::]:443 ssl;";
                    string sslCertLine = $"    ssl_certificate      {crt.Replace("\\", "/")};";
                    string sslKeyLine = $"    ssl_certificate_key  {key.Replace("\\", "/")};";

                    // Adiciona após 'listen' ou logo após 'server {'
                    int listenIndex = lines.FindIndex(l => l.Contains("listen [::]:80"));
                    int sslInsertIndex = listenIndex != -1 ? listenIndex + 1 : insertIndex + 1;
                    // Adiciona diretivas SSL apenas se ainda não existirem
                    var sslLinesToAdd = new[] { listenSslLine1, listenSslLine2, sslCertLine, sslKeyLine };
                    int addedCount = 0;
                    foreach (var sslLine in sslLinesToAdd)
                    {
                        if (!lines.Any(l => l.Trim() == sslLine.Trim()))
                        {
                            lines.Insert(sslInsertIndex + addedCount, sslLine);
                            addedCount++;
                        }
                    }
                    if (addedCount > 0)
                    {
                        File.WriteAllLines(foundConfPath, lines);
                        Console.WriteLine($"Diretivas SSL adicionadas ao arquivo {foundConfPath} (nginx v{foundNginxVersion}).");
                    }
                    else
                    {
                        Console.WriteLine($"Diretivas SSL já presentes em {foundConfPath} (nginx v{foundNginxVersion}).");
                    }
                }
                else
                {
                    Console.WriteLine($"Arquivo de configuração do domínio {domain}.conf não encontrado em nenhuma versão do Nginx.");
                }
                return;
            }
            else
            {
                Console.WriteLine($"Falha ao gerar certificado SSL para {domain}.");
                return;
            }
        }
    }
}
