using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        // Constantes de dimensões e configuração
        private const int INPUT_HEIGHT = 30;
        private const int BUTTON_HEIGHT = 40;
        private const int BUTTON_FONT_SIZE = 14;
        private const int BROWSE_BUTTON_WIDTH = 120;
        private const int TITLE_FONT_SIZE = 18;
        private const int SECTION_FONT_SIZE = 16;
        private const int INPUT_MARGIN_BOTTOM = 15;
        private const int SECTION_MARGIN_TOP = 10;
        private const int SECTION_MARGIN_BOTTOM = 10;
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

            // Painel direito - Console dedicado da aba Sites
            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Sites);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de criação de sites
        /// </summary>
        private static UIElement CreateSiteCreationPanel(DevStackGui mainWindow)
        {
            // Usar Grid para permitir overlay
            var grid = new Grid();
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sites_tab.title"), true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Domínio
            var domainLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sites_tab.labels.domain"));
            panel.Children.Add(domainLabel);

            var domainTextBox = DevStackShared.ThemeManager.CreateStyledTextBox();
            domainTextBox.Height = 30;
            domainTextBox.Name = "DomainTextBox";
            panel.Children.Add(domainTextBox);

            // Diretório raiz com botão procurar
            var rootLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sites_tab.labels.root_directory"));
            panel.Children.Add(rootLabel);

            var rootPanel = new Grid
            {
                Margin = new Thickness(0, 5, 0, 15)
            };
            rootPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rootPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var rootTextBox = DevStackShared.ThemeManager.CreateStyledTextBox();
            rootTextBox.Height = 30;
            rootTextBox.Name = "RootTextBox";
            Grid.SetColumn(rootTextBox, 0);
            rootPanel.Children.Add(rootTextBox);

            var browseButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.sites_tab.buttons.browse"), (s, e) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.ValidateNames = false;
                dialog.CheckFileExists = false;
                dialog.CheckPathExists = true;
                dialog.FileName = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.select_folder");
                dialog.Filter = "Pastas|*.";
                dialog.Title = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.select_folder");

                if (dialog.ShowDialog() == true)
                {
                    var selectedPath = Path.GetDirectoryName(dialog.FileName);
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        rootTextBox.Text = selectedPath;
                    }
                }
            });
            browseButton.Width = 120;
            browseButton.Height = 30;
            browseButton.Padding = new Thickness(12, 3, 12, 3);

            // Aplicar cantos arredondados personalizados ao botão browser mantendo o estilo do CreateStyledButton
            var browseStyle = new Style(typeof(Button), browseButton.Style);
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(0, 3, 3, 0));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, DevStackShared.ThemeManager.CurrentTheme.ButtonBackground));
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, DevStackShared.ThemeManager.CurrentTheme.ButtonBackground));

            template.Triggers.Add(hoverTrigger);
            browseStyle.Setters.Add(new Setter(Button.TemplateProperty, template));
            browseButton.Style = browseStyle;

            Grid.SetColumn(browseButton, 1);
            rootPanel.Children.Add(browseButton);
            panel.Children.Add(rootPanel);

            // PHP Upstream com ComboBox para versões instaladas
            var phpLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sites_tab.labels.php_upstream"));
            panel.Children.Add(phpLabel);

            var phpComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            phpComboBox.Height = 30;
            phpComboBox.Margin = new Thickness(0, 5, 0, 15);
            phpComboBox.Name = "PhpComboBox";
            
            // Carregar versões PHP instaladas
            LoadPhpVersions(phpComboBox);
            panel.Children.Add(phpComboBox);

            // Nginx Version ComboBox
            var nginxLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sites_tab.labels.nginx_version"));
            panel.Children.Add(nginxLabel);

            var nginxComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            nginxComboBox.Height = 30;
            nginxComboBox.Margin = new Thickness(0, 5, 0, 15);
            nginxComboBox.Name = "NginxComboBox";

            // Carregar versões Nginx instaladas
            LoadNginxVersions(nginxComboBox);
            panel.Children.Add(nginxComboBox);

            // Checkbox SSL
            var sslCheckBox = DevStackShared.ThemeManager.CreateStyledCheckBox(mainWindow.LocalizationManager.GetString("gui.sites_tab.ssl.generate_ssl"));
            panel.Children.Add(sslCheckBox);

            // Overlay de loading (spinner)
            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
            // Overlay sempre visível se criando site
            overlay.Visibility = mainWindow.IsCreatingSite ? Visibility.Visible : Visibility.Collapsed;
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsCreatingSite))
                {
                    overlay.Visibility = mainWindow.IsCreatingSite ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            // Botão Criar Site
            var createButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.sites_tab.buttons.create_site"), async (s, e) =>
            {
                mainWindow.IsCreatingSite = true;
                overlay.Visibility = Visibility.Visible;
                try
                {
                    var domain = domainTextBox.Text.Trim();
                    var root = rootTextBox.Text.Trim();
                    var phpUpstream = phpComboBox.SelectedItem?.ToString() ?? "";
                    var nginxVersion = nginxComboBox.SelectedItem?.ToString() ?? "";

                    bool siteCreated = await CreateSite(mainWindow, domain, root, phpUpstream, nginxVersion);

                    // Se o checkbox SSL estiver marcado, gerar SSL
                    bool sslCreated = true;
                    if (siteCreated && sslCheckBox.IsChecked == true)
                    {
                        sslCreated = await GenerateSslCertificate(mainWindow, domain);
                    }

                    // Reiniciar serviços do Nginx apenas se o site e SSL foram criados
                    if (siteCreated && sslCreated)
                    {
                        phpComboBox.SelectedIndex = -1;
                        nginxComboBox.SelectedIndex = -1;
                        sslCheckBox.IsChecked = false;
                        domainTextBox.Text = "";
                        rootTextBox.Text = "";

                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.restarting_nginx");
                        await RestartNginxServices(mainWindow);
                    }
                }
                finally
                {
                    mainWindow.IsCreatingSite = false;
                    overlay.Visibility = Visibility.Collapsed;
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Success);
            createButton.Height = 40;
            createButton.FontSize = 14;
            createButton.Margin = new Thickness(0, 10, 0, 20);
            panel.Children.Add(createButton);

            var sslTitle = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sites_tab.ssl.title"), true);
            sslTitle.FontSize = 16;
            sslTitle.Margin = new Thickness(0, 10, 0, 10);
            panel.Children.Add(sslTitle);

            var sslDomainLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sites_tab.labels.ssl_domain"));
            panel.Children.Add(sslDomainLabel);

            var sslDomainTextBox = DevStackShared.ThemeManager.CreateStyledTextBox();
            sslDomainTextBox.Height = 30;
            sslDomainTextBox.Margin = new Thickness(0, 5, 0, 15);
            sslDomainTextBox.Name = "SslDomainTextBox";
            panel.Children.Add(sslDomainTextBox);

            Button? generateSslButton = null;
            generateSslButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.sites_tab.buttons.generate_ssl"), async (s, e) =>
            {
                mainWindow.IsCreatingSite = true;
                overlay.Visibility = Visibility.Visible;
                if (generateSslButton != null)
                    generateSslButton.IsEnabled = false;
                try
                {
                    var domain = sslDomainTextBox.Text;
                    var sslCreated = await GenerateSslCertificate(mainWindow, domain);

                    if (sslCreated)
                    {
                        sslDomainTextBox.Text = "";

                        // Reiniciar serviços do Nginx após criar a configuração
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.restarting_nginx");
                        await RestartNginxServices(mainWindow);
                    }
                }
                finally
                {
                    if (generateSslButton != null)
                        generateSslButton.IsEnabled = true;
                    overlay.Visibility = Visibility.Collapsed;
                    mainWindow.IsCreatingSite = false;
                }
            });
            generateSslButton.Height = 40;
            generateSslButton.FontSize = 14;
            generateSslButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(generateSslButton);

            // Info
            var infoNotification = DevStackShared.ThemeManager.CreateNotificationPanel(
                mainWindow.LocalizationManager.GetString("gui.sites_tab.info"),
                DevStackShared.ThemeManager.NotificationType.Info
            );
            infoNotification.Margin = new Thickness(0, 20, 0, 0);
            panel.Children.Add(infoNotification);

            // Adiciona painel e overlay ao grid
            grid.Children.Add(panel);
            grid.Children.Add(overlay);

            return grid;
        }

        /// <summary>
        /// Carrega as versões PHP instaladas no ComboBox
        /// </summary>
        private static void LoadPhpVersions(ComboBox phpComboBox)
        {
            try
            {
                var phpDir = DevStackConfig.phpDir;
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
                }
            }
            catch {}
        }

        /// <summary>
        /// Carrega as versões Nginx instaladas no ComboBox
        /// </summary>
        private static void LoadNginxVersions(ComboBox nginxComboBox)
        {
            try
            {
                var nginxDir = DevStackConfig.nginxDir;
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
                }
            }
            catch {}
        }

        /// <summary>
        /// Cria uma configuração de site Nginx
        /// </summary>
        private static async Task<bool> CreateSite(DevStackGui mainWindow, string domain, string root, string phpUpstream, string nginxVersion)
        {
            if (!ValidateSiteInputs(mainWindow, domain, root, phpUpstream, nginxVersion))
            {
                return false;
            }

            try
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.creating_site", domain);
                await Task.Run(() => InstallManager.CreateNginxSiteConfig(domain, root, $"127.{phpUpstream}:9000", nginxVersion));
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.site_created", domain);
                return true;
            }
            catch (Exception ex)
            {
                HandleSiteError(mainWindow, domain, ex);
                return false;
            }
        }

        private static bool ValidateSiteInputs(
            DevStackGui mainWindow,
            string domain,
            string root,
            string phpUpstream,
            string nginxVersion)
        {
            if (string.IsNullOrEmpty(domain))
            {
                ShowWarning(mainWindow, "gui.sites_tab.messages.enter_domain");
                return false;
            }

            if (string.IsNullOrEmpty(root))
            {
                ShowWarning(mainWindow, "gui.sites_tab.messages.enter_root");
                return false;
            }

            if (string.IsNullOrEmpty(phpUpstream))
            {
                ShowWarning(mainWindow, "gui.sites_tab.messages.select_php");
                return false;
            }

            if (string.IsNullOrEmpty(nginxVersion))
            {
                ShowWarning(mainWindow, "gui.sites_tab.messages.select_nginx");
                return false;
            }

            return true;
        }

        private static void ShowWarning(DevStackGui mainWindow, string messageKey)
        {
            DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString(messageKey),
                mainWindow.LocalizationManager.GetString("gui.common.warning"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        private static void HandleSiteError(DevStackGui mainWindow, string domain, Exception ex)
        {
            var errorMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.site_error", domain, ex.Message);
            GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, errorMessage, mainWindow);
            mainWindow.StatusMessage = errorMessage;
            DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.site_config_error", ex.Message),
                mainWindow.LocalizationManager.GetString("gui.common.error"),
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        /// <summary>
        /// Gera um certificado SSL para o domínio especificado
        /// </summary>
        private static async Task<bool> GenerateSslCertificate(DevStackGui mainWindow, string domain)
        {
            if (!await ValidateSslDomain(mainWindow, domain))
            {
                return false;
            }

            return await ExecuteSslGeneration(mainWindow, domain);
        }

        private static async Task<bool> ValidateSslDomain(DevStackGui mainWindow, string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                Application.Current.Dispatcher.Invoke(() =>
                    ShowWarning(mainWindow, "gui.sites_tab.messages.enter_ssl_domain")
                );
                return false;
            }

            bool domainResolves = await CheckDomainResolution(domain);
            if (!domainResolves)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DevStackShared.ThemeManager.CreateStyledMessageBox(
                        mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.domain_not_exists", domain),
                        mainWindow.LocalizationManager.GetString("gui.common.warning"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                });
                return false;
            }

            return true;
        }

        private static async Task<bool> CheckDomainResolution(string domain)
        {
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        var hostEntry = System.Net.Dns.GetHostEntry(domain);
                        return hostEntry != null && hostEntry.AddressList.Length > 0;
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

        private static async Task<bool> ExecuteSslGeneration(DevStackGui mainWindow, string domain)
        {
            bool success = true;
            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Sites, mainWindow, async progress =>
            {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.generating_ssl", domain);
                    var args = new string[] { domain };
                    await Task.Run(() => GenerateManager.GenerateSslCertificate(args));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.ssl_generated", domain);
                }
                catch (Exception ex)
                {
                    success = false;
                    HandleSslError(mainWindow, domain, ex, progress);
                }
            });
            return success;
        }

        private static void HandleSslError(DevStackGui mainWindow, string domain, Exception ex, IProgress<string> progress)
        {
            var errorMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.ssl_error", ex.Message);
            progress.Report(errorMessage);
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.ssl_error", domain);
            DevStackShared.ThemeManager.CreateStyledMessageBox(
                errorMessage,
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.error"),
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        /// <summary>
        /// Reinicia os serviços do Nginx
        /// </summary>
        private static async Task RestartNginxServices(DevStackGui mainWindow)
        {
            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Sites, mainWindow, progress =>
            {
                try
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.restarting_nginx"));
                    var nginxComponents = mainWindow.InstalledComponents
                        .Where(component => component.Name.Equals("nginx", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (nginxComponents.Any())
                    {
                        int restartedCount = 0;
                        foreach (var nginxComponent in nginxComponents)
                        {
                            foreach (var version in nginxComponent.Versions)
                            {
                                try
                                {
                                    progress.Report(mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.restarting_nginx", version));
                                    ProcessManager.RestartComponent("nginx", version);
                                    progress.Report(mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.nginx_restarted", version));
                                    restartedCount++;
                                }
                                catch (Exception ex)
                                {
                                    progress.Report(mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.nginx_restart_error", version, ex.Message));
                                }
                            }
                        }
                        if (restartedCount == 0)
                        {
                            progress.Report(mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.no_nginx_restarted"));
                        }
                    }
                    else
                    {
                        progress.Report(mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.no_nginx_found"));
                    }
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.sites_tab.messages.nginx_restart_general_error", ex.Message));
                }
                return Task.CompletedTask;
            });
        }
    }
}
