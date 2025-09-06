using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela aba "Instalar" - instala novas ferramentas
    /// </summary>
    public static class GuiInstallTab
    {
        /// <summary>
        /// Cria o conteúdo completo da aba "Instalar"
        /// </summary>
        public static Grid CreateInstallContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Seleção
            var leftPanel = CreateInstallSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console dedicado da aba Install
            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Install);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de seleção de componentes para instalação
        /// </summary>
        private static UIElement CreateInstallSelectionPanel(DevStackGui mainWindow)
        {
            // Usar Grid para permitir overlay
            var grid = new Grid();
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título principal
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.title"), true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Seção de Instalação
            var installSectionLabel = DevStackShared.ThemeManager.CreateStyledLabel("Instalar Componente", true);
            installSectionLabel.FontSize = 14;
            installSectionLabel.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(installSectionLabel);

            // Componente
            var componentLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.labels.select_tool"));
            panel.Children.Add(componentLabel);

            var componentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, 5, 0, 15);
            componentCombo.Height = 30;
            var componentBinding = new Binding("AvailableComponents") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.ItemsSourceProperty, componentBinding);
            var selectedComponentBinding = new Binding("SelectedComponent") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedComponentBinding);
            panel.Children.Add(componentCombo);

            // Versão
            var versionLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.labels.select_version"));
            panel.Children.Add(versionLabel);

            var versionCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            versionCombo.Margin = new Thickness(0, 5, 0, 20);
            versionCombo.Height = 30;
            var versionBinding = new Binding("AvailableVersions") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.ItemsSourceProperty, versionBinding);
            var selectedVersionBinding = new Binding("SelectedVersion") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedVersionBinding);
            panel.Children.Add(versionCombo);

            // Overlay de loading (spinner)
            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
            // Overlay sempre visível se instalando
            overlay.Visibility = mainWindow.IsInstallingComponent ? Visibility.Visible : Visibility.Collapsed;
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsInstallingComponent))
                {
                    overlay.Visibility = mainWindow.IsInstallingComponent ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            // Botão Instalar
            var installButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.install_tab.buttons.install"), async (s, e) =>
            {
                mainWindow.IsInstallingComponent = true;
                overlay.Visibility = Visibility.Visible;
                try
                {
                    await InstallComponent(mainWindow);
                }
                finally
                {
                    mainWindow.IsInstallingComponent = false;
                    mainWindow.SelectedComponent = "";
                    mainWindow.SelectedVersion = "";
                    overlay.Visibility = Visibility.Collapsed;
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Success);
            installButton.Height = 40;
            installButton.FontSize = 14;
            installButton.Margin = new Thickness(0, 10, 0, 20);
            panel.Children.Add(installButton);

            // Seção de Criação de Atalhos
            var shortcutSectionLabel = DevStackShared.ThemeManager.CreateStyledLabel("Criar Atalhos para Componentes Instalados", true);
            shortcutSectionLabel.FontSize = 14;
            shortcutSectionLabel.Margin = new Thickness(0, 10, 0, 10);
            panel.Children.Add(shortcutSectionLabel);

            // Componente Instalado
            var installedComponentLabel = DevStackShared.ThemeManager.CreateStyledLabel("Componente Instalado:");
            panel.Children.Add(installedComponentLabel);

            var installedComponentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            installedComponentCombo.Margin = new Thickness(0, 5, 0, 15);
            installedComponentCombo.Height = 30;
            installedComponentCombo.Name = "ShortcutComponentCombo";
            
            // Binding para a coleção de componentes que suportam shortcuts
            var shortcutComponentsBinding = new Binding("ShortcutComponents") { Source = mainWindow };
            installedComponentCombo.SetBinding(ComboBox.ItemsSourceProperty, shortcutComponentsBinding);
            
            var selectedShortcutComponentBinding = new Binding("SelectedShortcutComponent") { Source = mainWindow };
            installedComponentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedShortcutComponentBinding);
            panel.Children.Add(installedComponentCombo);

            // Versão Instalada
            var installedVersionLabel = DevStackShared.ThemeManager.CreateStyledLabel("Versão Instalada:");
            panel.Children.Add(installedVersionLabel);

            var installedVersionCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            installedVersionCombo.Margin = new Thickness(0, 5, 0, 20);
            installedVersionCombo.Height = 30;
            installedVersionCombo.Name = "ShortcutVersionCombo";
            
            // Binding para a coleção de versões do componente selecionado
            var shortcutVersionsBinding = new Binding("ShortcutVersions") { Source = mainWindow };
            installedVersionCombo.SetBinding(ComboBox.ItemsSourceProperty, shortcutVersionsBinding);
            
            var selectedShortcutVersionBinding = new Binding("SelectedShortcutVersion") { Source = mainWindow };
            installedVersionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedShortcutVersionBinding);
            panel.Children.Add(installedVersionCombo);

            // Botão Criar Atalho
            var createShortcutButton = DevStackShared.ThemeManager.CreateStyledButton("Criar Atalho", async (s, e) =>
            {
                await CreateShortcutForComponent(mainWindow);
            });
            createShortcutButton.Height = 35;
            createShortcutButton.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(createShortcutButton);

            // Adiciona painel e overlay ao grid
            grid.Children.Add(panel);
            grid.Children.Add(overlay);

            return grid;
        }

        /// <summary>
        /// Instala o componente selecionado
        /// </summary>
        public static async Task InstallComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedComponent))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.select_component"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.installing", mainWindow.SelectedComponent);

            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Install, mainWindow, async progress =>
            {
                try
                {
                    var args = string.IsNullOrEmpty(mainWindow.SelectedVersion)
                        ? new[] { mainWindow.SelectedComponent }
                        : new[] { mainWindow.SelectedComponent, mainWindow.SelectedVersion };

                    await InstallManager.InstallCommands(args);

                    // Atualizar PATH após instalação bem-sucedida
                    if (DevStackConfig.pathManager != null)
                    {
                        DevStackConfig.pathManager.AddBinDirsToPath();
                    }
                    else
                    {
                        progress.Report(mainWindow.LocalizationManager.GetString("gui.services_tab.path_manager.not_initialized"));
                    }

                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.success", mainWindow.SelectedComponent);

                    // Recarregar lista de instalados
                    await mainWindow.LoadInstalledComponents();
                    await mainWindow.LoadShortcutComponents();
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.error", mainWindow.SelectedComponent));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.error", mainWindow.SelectedComponent);
                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.error", ex));
                }
            });
        }

        /// <summary>
        /// Cria atalho para o componente selecionado
        /// </summary>
        public static async Task CreateShortcutForComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedShortcutComponent))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox("Selecione um componente", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(mainWindow.SelectedShortcutVersion))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox("Selecione uma versão", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainWindow.StatusMessage = $"Criando atalho para {mainWindow.SelectedShortcutComponent} {mainWindow.SelectedShortcutVersion}...";

            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Install, mainWindow, async progress =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        // Obter o componente
                        var comp = Components.ComponentsFactory.GetComponent(mainWindow.SelectedShortcutComponent);
                        if (comp == null)
                        {
                            throw new Exception($"Componente '{mainWindow.SelectedShortcutComponent}' não encontrado");
                        }

                        // Verificar se o componente tem CreateBinShortcut definido
                        if (string.IsNullOrEmpty(comp.CreateBinShortcut))
                        {
                            throw new Exception($"Componente '{mainWindow.SelectedShortcutComponent}' não suporta criação de atalhos");
                        }

                        // Construir caminhos
                        string baseToolDir = !string.IsNullOrEmpty(comp.ToolDir) ? comp.ToolDir : System.IO.Path.Combine(DevStackConfig.baseDir, comp.Name);
                        string subDir = $"{comp.Name}-{mainWindow.SelectedShortcutVersion}";
                        string targetDir = System.IO.Path.Combine(baseToolDir, subDir);

                        if (!System.IO.Directory.Exists(targetDir))
                        {
                            throw new Exception($"Diretório de instalação não encontrado: {targetDir}");
                        }

                        // Criar atalho
                        string shortcutName = comp.CreateBinShortcut.Replace("{version}", mainWindow.SelectedShortcutVersion);
                        string sourceDir = targetDir;
                        string sourcePattern;

                        // Se ExecutablePattern estiver definido, usar ele como source file
                        if (!string.IsNullOrEmpty(comp.ExecutablePattern))
                        {
                            sourcePattern = comp.ExecutablePattern;

                            // Se ExecutableFolder estiver definido, usar ele como source directory
                            if (!string.IsNullOrEmpty(comp.ExecutableFolder))
                            {
                                if (System.IO.Path.IsPathRooted(comp.ExecutableFolder))
                                {
                                    sourceDir = comp.ExecutableFolder;
                                }
                                else
                                {
                                    sourceDir = System.IO.Path.Combine(targetDir, comp.ExecutableFolder);
                                }
                            }
                        }
                        else
                        {
                            // Fallback para usar o próprio CreateBinShortcut como source
                            sourcePattern = shortcutName;
                        }

                        Components.ComponentBase.CreateGlobalBinShortcut(sourceDir, sourcePattern, mainWindow.SelectedShortcutVersion, comp.Name, shortcutName);

                        mainWindow.StatusMessage = $"Atalho criado com sucesso para {mainWindow.SelectedShortcutComponent} {mainWindow.SelectedShortcutVersion}";
                    }
                    catch (Exception ex)
                    {
                        progress.Report($"Erro ao criar atalho: {ex.Message}");
                        mainWindow.StatusMessage = $"Erro ao criar atalho para {mainWindow.SelectedShortcutComponent}";
                        DevStackConfig.WriteLog($"Erro ao criar atalho na GUI: {ex}");
                    }
                });
            });
        }

    }
}
