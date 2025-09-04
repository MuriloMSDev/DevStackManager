using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela aba "Desinstalar" - remove ferramentas instaladas
    /// </summary>
    public static class GuiUninstallTab
    {
        /// <summary>
        /// Cria o conteúdo completo da aba "Desinstalar"
        /// </summary>
        public static Grid CreateUninstallContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Seleção
            var leftPanel = CreateUninstallSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);


            // Painel direito - Console dedicado da aba Uninstall
            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Uninstall);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            // Carregar componentes instalados ao criar a aba (com delay para garantir que UI esteja pronta)
            _ = Task.Run(async () =>
            {
                await Task.Delay(100); // Pequeno delay para garantir que a UI esteja construída
                mainWindow.Dispatcher.Invoke(() => LoadUninstallComponents(mainWindow));
            });

            return grid;
        }

        /// <summary>
        /// Cria o painel de seleção de componentes para desinstalação
        /// </summary>
        private static UIElement CreateUninstallSelectionPanel(DevStackGui mainWindow)
        {
            // Usar Grid para permitir overlay
            var grid = new Grid();
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.title"), true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Componente
            var componentLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_tool"));
            panel.Children.Add(componentLabel);

            var componentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, 5, 0, 15);
            componentCombo.Height = 30;
            componentCombo.Name = "UninstallComponentCombo";
            
            var selectedUninstallComponentBinding = new Binding("SelectedUninstallComponent") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallComponentBinding);
            panel.Children.Add(componentCombo);

            // Versão
            var versionLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_version"));
            panel.Children.Add(versionLabel);

            var versionCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            versionCombo.Margin = new Thickness(0, 5, 0, 20);
            versionCombo.Height = 30;
            versionCombo.Name = "UninstallVersionCombo";
            
            var selectedUninstallVersionBinding = new Binding("SelectedUninstallVersion") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallVersionBinding);
            panel.Children.Add(versionCombo);

            // Overlay de loading (spinner)
            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
            // Overlay sempre visível se desinstalando
            overlay.Visibility = mainWindow.IsUninstallingComponent ? Visibility.Visible : Visibility.Collapsed;
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsUninstallingComponent))
                {
                    overlay.Visibility = mainWindow.IsUninstallingComponent ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            // Botão Desinstalar
            var uninstallButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.buttons.uninstall"), async (s, e) =>
            {
                mainWindow.IsUninstallingComponent = true;
                overlay.Visibility = Visibility.Visible;
                try
                {
                    await UninstallComponent(mainWindow);
                }
                finally
                {
                    mainWindow.SelectedUninstallComponent = "";
                    mainWindow.SelectedUninstallVersion = "";
                    mainWindow.IsUninstallingComponent = false;
                    overlay.Visibility = Visibility.Collapsed;
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Danger);
            uninstallButton.Height = 40;
            uninstallButton.FontSize = 14;
            uninstallButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(uninstallButton);

            // Botão Atualizar Lista
            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.buttons.refresh"), (s, e) => LoadUninstallComponents(mainWindow));
            refreshButton.Height = 35;
            refreshButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(refreshButton);

            // Warning usando painel de notificação
            var warningPanel = DevStackShared.ThemeManager.CreateNotificationPanel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.warning"), 
                DevStackShared.ThemeManager.NotificationType.Warning
            );
            warningPanel.Margin = new Thickness(0, 20, 0, 0);
            panel.Children.Add(warningPanel);

            // Adiciona painel e overlay ao grid
            grid.Children.Add(panel);
            grid.Children.Add(overlay);

            return grid;
        }

        /// <summary>
        /// Desinstala o componente selecionado
        /// </summary>
        public static async Task UninstallComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedUninstallComponent))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_component"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(mainWindow.SelectedUninstallVersion))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_version"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.confirm", mainWindow.SelectedUninstallComponent),
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.confirmation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.uninstalling", mainWindow.SelectedUninstallComponent);
            
            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Uninstall, mainWindow, async progress =>
            {
                try
                {
                    var args = string.IsNullOrEmpty(mainWindow.SelectedUninstallVersion)
                        ? new[] { mainWindow.SelectedUninstallComponent }
                        : new[] { mainWindow.SelectedUninstallComponent, mainWindow.SelectedUninstallVersion };

                    UninstallManager.UninstallCommands(args);

                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.success", mainWindow.SelectedUninstallComponent);

                    // Recarregar lista de instalados
                    await GuiInstalledTab.LoadInstalledComponents(mainWindow);
                    await GuiInstallTab.LoadShortcutComponents(mainWindow);

                    // Recarregar componentes disponíveis para desinstalação
                    LoadUninstallComponents(mainWindow);
                    await LoadUninstallVersions(mainWindow);
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.error", mainWindow.SelectedUninstallComponent, ex.Message));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.error_short", mainWindow.SelectedUninstallComponent);
                    DevStackConfig.WriteLog($"Erro ao desinstalar {mainWindow.SelectedUninstallComponent} na GUI: {ex}");
                }
            });
        }

        /// <summary>
        /// Carrega as versões instaladas do componente selecionado para desinstalação
        /// </summary>
        public static async Task LoadUninstallVersions(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedUninstallComponent))
            {
                // Limpar versões se nenhum componente selecionado
                var versionCombo = GuiHelpers.FindChild<ComboBox>(mainWindow, "UninstallVersionCombo");
                if (versionCombo != null)
                {
                    versionCombo.Items.Clear();
                }
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.loading_versions", mainWindow.SelectedUninstallComponent);
                    var status = DataManager.GetComponentStatus(mainWindow.SelectedUninstallComponent);
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        var versionCombo = GuiHelpers.FindChild<ComboBox>(mainWindow, "UninstallVersionCombo");
                        if (versionCombo != null)
                        {
                            versionCombo.Items.Clear();
                            if (status.Installed && status.Versions.Any())
                            {
                                // Ordena as versões em ordem decrescente
                                foreach (var version in status.Versions
                                    .OrderByDescending(v => Version.TryParse(
                                        mainWindow.SelectedUninstallComponent == "git" && v.StartsWith("git-")
                                            ? v.Substring(4)
                                            : v.StartsWith($"{mainWindow.SelectedUninstallComponent}-")
                                                ? v.Substring(mainWindow.SelectedUninstallComponent.Length + 1)
                                                : v,
                                        out var parsed) ? parsed : new Version(0, 0)))
                                {
                                    // Extrair apenas a parte da versão, removendo o nome do componente
                                    var versionNumber = version;
                                    if (mainWindow.SelectedUninstallComponent == "git" && version.StartsWith("git-"))
                                    {
                                        versionNumber = version.Substring(4); // Remove "git-"
                                    }
                                    else if (version.StartsWith($"{mainWindow.SelectedUninstallComponent}-"))
                                    {
                                        versionNumber = version.Substring(mainWindow.SelectedUninstallComponent.Length + 1);
                                    }
                                    versionCombo.Items.Add(versionNumber);
                                }
                            }
                            else
                            {
                                DevStackShared.ThemeManager.CreateStyledMessageBox(
                                    mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.no_versions", mainWindow.SelectedUninstallComponent), 
                                    mainWindow.LocalizationManager.GetString("gui.common.dialogs.info"), 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Information);
                            }
                        }
                        mainWindow.StatusMessage = status.Installed ?
                            mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.versions_loaded", mainWindow.SelectedUninstallComponent) :
                            mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.not_installed", mainWindow.SelectedUninstallComponent);
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.error_loading_versions", ex.Message);
                        DevStackConfig.WriteLog($"Erro ao carregar versões para desinstalação na GUI: {ex}");
                    });
                }
            });
        }

        /// <summary>
        /// Carrega os componentes disponíveis para desinstalação
        /// </summary>
        public static void LoadUninstallComponents(DevStackGui mainWindow)
        {
            try
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.loading_components");
                
                var componentCombo = GuiHelpers.FindChild<ComboBox>(mainWindow, "UninstallComponentCombo");
                if (componentCombo == null)
                {
                    // Se não encontrou o combo, tentar novamente após um delay
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(200);
                        mainWindow.Dispatcher.Invoke(() => LoadUninstallComponents(mainWindow));
                    });
                    return;
                }
                
                componentCombo.Items.Clear();
                
                // Obter componentes instalados
                var installedComponents = mainWindow.InstalledComponents.Where(c => c.Installed).ToList();
                
                if (installedComponents.Any())
                {
                    foreach (var comp in installedComponents)
                    {
                        componentCombo.Items.Add(comp.Name);
                    }

                    componentCombo.SelectedIndex = -1;
                    
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.components_count", componentCombo.Items.Count);
                }
                else
                {
                    // Sem componentes instalados: não reagendar carregamento infinito
                    componentCombo.Items.Clear();
                    componentCombo.SelectedIndex = -1;
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.components_count", 0);
                }
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.error_loading_components", ex.Message);
                DevStackConfig.WriteLog($"Erro ao carregar componentes para desinstalação na GUI: {ex}");
            }
        }
    }
}
