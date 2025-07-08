using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela aba "Sites" - gerencia configurações de sites Nginx
    /// </summary>
    public static class GuiSitesTab
    {
        /// <summary>
        /// Cria o conteúdo completo da aba "Sites"
        /// </summary>
        public static Grid CreateSitesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Criar site
            var leftPanel = CreateSiteCreationPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console
            var rightPanel = GuiConsolePanel.CreateConsoleOutputPanel(mainWindow);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de criação de sites
        /// </summary>
        private static StackPanel CreateSiteCreationPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = GuiTheme.CreateStyledLabel("Criar Configuração de Site Nginx", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Domínio
            var domainLabel = GuiTheme.CreateStyledLabel("Domínio do site:");
            panel.Children.Add(domainLabel);

            var domainTextBox = GuiTheme.CreateStyledTextBox();
            domainTextBox.Height = 30;
            domainTextBox.Name = "DomainTextBox";
            panel.Children.Add(domainTextBox);

            // Diretório raiz com botão procurar
            var rootLabel = GuiTheme.CreateStyledLabel("Diretório raiz (opcional):");
            panel.Children.Add(rootLabel);

            var rootPanel = new Grid
            {
                Margin = new Thickness(0, 5, 0, 15)
            };
            rootPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rootPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var rootTextBox = GuiTheme.CreateStyledTextBox();
            rootTextBox.Height = 30;
            rootTextBox.Name = "RootTextBox";
            Grid.SetColumn(rootTextBox, 0);
            rootPanel.Children.Add(rootTextBox);

            var browseButton = GuiTheme.CreateStyledButton("📁 Procurar", (s, e) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.ValidateNames = false;
                dialog.CheckFileExists = false;
                dialog.CheckPathExists = true;
                dialog.FileName = "Selecionar Pasta";
                dialog.Filter = "Pastas|*.";
                dialog.Title = "Selecionar Pasta do Site";

                if (dialog.ShowDialog() == true)
                {
                    var selectedPath = Path.GetDirectoryName(dialog.FileName);
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        rootTextBox.Text = selectedPath;
                    }
                }
            });
            browseButton.Width = 100;
            browseButton.Height = 30;
            browseButton.Padding = new Thickness(12, 3, 12, 3);
            Grid.SetColumn(browseButton, 1);
            rootPanel.Children.Add(browseButton);
            panel.Children.Add(rootPanel);

            // PHP Upstream com ComboBox para versões instaladas
            var phpLabel = GuiTheme.CreateStyledLabel("PHP Upstream:");
            panel.Children.Add(phpLabel);

            var phpComboBox = GuiTheme.CreateStyledComboBox();
            phpComboBox.Height = 30;
            phpComboBox.Margin = new Thickness(0, 5, 0, 15);
            phpComboBox.Name = "PhpComboBox";
            
            // Carregar versões PHP instaladas
            LoadPhpVersions(phpComboBox);
            panel.Children.Add(phpComboBox);

            // Nginx Version ComboBox
            var nginxLabel = GuiTheme.CreateStyledLabel("Versão Nginx:");
            panel.Children.Add(nginxLabel);

            var nginxComboBox = GuiTheme.CreateStyledComboBox();
            nginxComboBox.Height = 30;
            nginxComboBox.Margin = new Thickness(0, 5, 0, 15);
            nginxComboBox.Name = "NginxComboBox";

            // Carregar versões Nginx instaladas
            LoadNginxVersions(nginxComboBox);
            panel.Children.Add(nginxComboBox);

            // Botão Criar Site
            var createButton = GuiTheme.CreateStyledButton("🌐 Criar Configuração de Site", (s, e) => 
            {
                var domain = domainTextBox.Text.Trim();
                var root = rootTextBox.Text.Trim();
                var phpUpstream = phpComboBox.SelectedItem?.ToString() ?? "";
                var nginxVersion = nginxComboBox.SelectedItem?.ToString() ?? "";
                
                CreateSite(mainWindow, domain, root, phpUpstream, nginxVersion);
            });
            createButton.Height = 40;
            createButton.FontSize = 14;
            createButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(createButton);

            // Seção SSL
            var sslSeparator = new Separator { Margin = new Thickness(0, 20, 0, 10) };
            panel.Children.Add(sslSeparator);

            var sslTitle = GuiTheme.CreateStyledLabel("Certificados SSL", true);
            sslTitle.FontSize = 16;
            sslTitle.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(sslTitle);

            var sslDomainLabel = GuiTheme.CreateStyledLabel("Domínio para SSL:");
            panel.Children.Add(sslDomainLabel);

            var sslDomainTextBox = GuiTheme.CreateStyledTextBox();
            sslDomainTextBox.Height = 30;
            sslDomainTextBox.Margin = new Thickness(0, 5, 0, 15);
            sslDomainTextBox.Name = "SslDomainTextBox";
            panel.Children.Add(sslDomainTextBox);

            var generateSslButton = GuiTheme.CreateStyledButton("🔒 Gerar Certificado SSL", (s, e) => GenerateSslCertificate(mainWindow, sslDomainTextBox.Text));
            generateSslButton.Height = 40;
            generateSslButton.FontSize = 14;
            generateSslButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(generateSslButton);

            // Info
            var infoLabel = GuiTheme.CreateStyledLabel("ℹ️ Os arquivos de configuração serão criados automaticamente");
            infoLabel.FontStyle = FontStyles.Italic;
            infoLabel.Margin = new Thickness(0, 20, 0, 0);
            panel.Children.Add(infoLabel);

            return panel;
        }

        /// <summary>
        /// Carrega as versões PHP instaladas no ComboBox
        /// </summary>
        private static void LoadPhpVersions(ComboBox phpComboBox)
        {
            try
            {
                var phpDir = Path.Combine("C:\\devstack", "php");
                if (Directory.Exists(phpDir))
                {
                    var phpVersions = Directory.GetDirectories(phpDir)
                        .Select(d => Path.GetFileName(d))
                        .Where(name => name.StartsWith("php-"))
                        .Select(name => name.Replace("php-", ""))
                        .ToList();

                    foreach (var version in phpVersions)
                    {
                        phpComboBox.Items.Add(version);
                    }

                    if (phpComboBox.Items.Count > 0)
                    {
                        phpComboBox.SelectedIndex = 0;
                    }
                }
                
                // Adicionar opção padrão se nenhuma versão instalada
                if (phpComboBox.Items.Count == 0)
                {
                    phpComboBox.Items.Add("127.0.0.1:9000");
                    phpComboBox.SelectedIndex = 0;
                }
            }
            catch
            {
                phpComboBox.Items.Add("127.0.0.1:9000");
                phpComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Carrega as versões Nginx instaladas no ComboBox
        /// </summary>
        private static void LoadNginxVersions(ComboBox nginxComboBox)
        {
            try
            {
                var nginxDir = Path.Combine("C:\\devstack", "nginx");
                if (Directory.Exists(nginxDir))
                {
                    var nginxVersions = Directory.GetDirectories(nginxDir)
                        .Select(d => Path.GetFileName(d))
                        .Where(name => name.StartsWith("nginx-"))
                        .Select(name => name.Replace("nginx-", ""))
                        .ToList();

                    foreach (var version in nginxVersions)
                    {
                        nginxComboBox.Items.Add(version);
                    }

                    if (nginxComboBox.Items.Count > 0)
                    {
                        nginxComboBox.SelectedIndex = 0;
                    }
                }

                if (nginxComboBox.Items.Count == 0)
                {
                    nginxComboBox.Items.Add("latest");
                    nginxComboBox.SelectedIndex = 0;
                }
            }
            catch
            {
                nginxComboBox.Items.Add("latest");
                nginxComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Cria uma configuração de site Nginx
        /// </summary>
        private static void CreateSite(DevStackGui mainWindow, string domain, string root, string phpUpstream, string nginxVersion = "")
        {
            if (string.IsNullOrEmpty(domain))
            {
                MessageBox.Show("Digite um domínio para o site.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                mainWindow.StatusMessage = $"Criando configuração para o site {domain}...";
                InstallManager.CreateNginxSiteConfig(domain, root, phpUpstream, null, nginxVersion);
                
                var message = $"Configuração para o site {domain} criada com sucesso.\n\n" +
                             $"Não se esqueça de adicionar uma entrada no arquivo hosts:\n" +
                             $"127.0.0.1    {domain}";
                
                MessageBox.Show(message, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                
                mainWindow.StatusMessage = $"Site {domain} criado";
                
                // Limpar os campos após sucesso
                var domainTextBox = GuiHelpers.FindChild<TextBox>(mainWindow, "DomainTextBox");
                var rootTextBox = GuiHelpers.FindChild<TextBox>(mainWindow, "RootTextBox");
                if (domainTextBox != null) domainTextBox.Text = "";
                if (rootTextBox != null) rootTextBox.Text = "";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao criar site {domain}: {ex.Message}");
                mainWindow.StatusMessage = $"Erro ao criar site {domain}";
                MessageBox.Show($"Erro ao criar configuração do site: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gera um certificado SSL para o domínio especificado
        /// </summary>
        private static void GenerateSslCertificate(DevStackGui mainWindow, string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                MessageBox.Show("Digite um domínio para gerar o certificado SSL.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                mainWindow.StatusMessage = $"Gerando certificado SSL para {domain}...";
                
                // Chamar o setup.ps1 com comando SSL
                var setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "setup.ps1");
                if (!File.Exists(setupPath))
                {
                    setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "setup.ps1");
                }

                if (File.Exists(setupPath))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "pwsh.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{setupPath}\" ssl {domain}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    // Fallback para PowerShell Windows se pwsh não estiver disponível
                    try
                    {
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            process.WaitForExit();
                            var output = process.StandardOutput.ReadToEnd();
                            var error = process.StandardError.ReadToEnd();

                            if (process.ExitCode == 0)
                            {
                                MessageBox.Show($"Certificado SSL para {domain} gerado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                                mainWindow.StatusMessage = $"Certificado SSL para {domain} gerado";
                                
                                // Limpar o campo
                                var sslDomainTextBox = GuiHelpers.FindChild<TextBox>(mainWindow, "SslDomainTextBox");
                                if (sslDomainTextBox != null) sslDomainTextBox.Text = "";
                            }
                            else
                            {
                                throw new Exception($"Processo falhou com código {process.ExitCode}: {error}");
                            }
                        }
                    }
                    catch
                    {
                        // Fallback para powershell.exe
                        startInfo.FileName = "powershell.exe";
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            process.WaitForExit();
                            if (process.ExitCode == 0)
                            {
                                MessageBox.Show($"Certificado SSL para {domain} gerado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                                
                                mainWindow.StatusMessage = $"Certificado SSL para {domain} gerado";
                            }
                            else
                            {
                                throw new Exception("Falha na execução do comando SSL");
                            }
                        }
                    }
                }
                else
                {
                    mainWindow.StatusMessage = $"SSL para {domain} - em desenvolvimento";
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao gerar certificado SSL: {ex.Message}");
                mainWindow.StatusMessage = $"Erro ao gerar SSL para {domain}";
                MessageBox.Show($"Erro ao gerar certificado SSL: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
