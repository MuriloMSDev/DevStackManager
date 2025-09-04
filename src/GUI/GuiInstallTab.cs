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
            // Carregar componentes disponíveis
            LoadAvailableComponents(mainWindow);
            
            // Carregar componentes instalados com atalhos na inicialização
            _ = Task.Run(async () =>
            {
                await Task.Delay(100); // Pequeno delay para garantir que a UI esteja construída
                await mainWindow.Dispatcher.InvokeAsync(async () => await LoadShortcutComponents(mainWindow));
            });

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
                    await GuiInstalledTab.LoadInstalledComponents(mainWindow);
                    await LoadShortcutComponents(mainWindow);
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
        /// Carrega as versões disponíveis para o componente selecionado
        /// </summary>
        public static async Task LoadVersionsForComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedComponent))
            {
                mainWindow.AvailableVersions.Clear();
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.loading_versions", mainWindow.SelectedComponent);

                    var selectedComponent = mainWindow.SelectedComponent; // capture safely
                    var versionData = GetVersionDataForComponent(selectedComponent);
                    if (versionData.Status != "ok")
                    {
                        throw new Exception(string.IsNullOrWhiteSpace(versionData.Message) ? "Failed to load versions" : versionData.Message);
                    }

                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.AvailableVersions.Clear();
                        foreach (var version in versionData.Versions
                            .OrderByDescending(v =>
                                Version.TryParse(v, out var parsed) ? parsed : new Version(0, 0)))
                        {
                            mainWindow.AvailableVersions.Add(version);
                        }

                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.versions_loaded", mainWindow.AvailableVersions.Count, selectedComponent);
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.versions_error", ex.Message);
                        DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.versions_error", ex));
                    });
                }
            });
        }

        /// <summary>
        /// Carrega a lista de componentes disponíveis para instalação
        /// </summary>
        public static void LoadAvailableComponents(DevStackGui mainWindow)
        {
            mainWindow.AvailableComponents.Clear();
            foreach (var component in DevStackConfig.components)
            {
                mainWindow.AvailableComponents.Add(component);
            }
        }

        /// <summary>
        /// Obtém os dados de versão para um componente específico
        /// </summary>
        public static VersionData GetVersionDataForComponent(string component)
        {
            try
            {
                var comp = Components.ComponentsFactory.GetComponent(component);
                if (comp != null)
                {
                    var versions = comp.ListAvailable();
                    return new VersionData
                    {
                        Status = "ok",
                        Versions = versions,
                        Message = string.Empty
                    };
                }
                else
                {
                    return new VersionData { Status = "error", Versions = new System.Collections.Generic.List<string>(), Message = $"Component '{component}' not found" };
                }
            }
            catch (Exception ex)
            {
                return new VersionData { Status = "error", Versions = new System.Collections.Generic.List<string>(), Message = ex.Message };
            }
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

        /// <summary>
        /// Carrega os componentes instalados que têm CreateBinShortcut definido
        /// </summary>
        public static async Task LoadShortcutComponents(DevStackGui mainWindow)
        {
            try
            {
                var componentCombo = GuiHelpers.FindChild<ComboBox>(mainWindow, "ShortcutComponentCombo");
                if (componentCombo == null)
                {
                    // Se não encontrou o combo, tentar novamente após um delay
                    await Task.Delay(200);
                    await mainWindow.Dispatcher.InvokeAsync(async () => await LoadShortcutComponents(mainWindow));
                    return;
                }

                componentCombo.Items.Clear();

                // Obter componentes instalados que têm CreateBinShortcut definido - executar em background
                var installedComponents = await Task.Run(() => mainWindow.InstalledComponents.Where(c => c.Installed).ToList());

                foreach (var comp in installedComponents)
                {
                    try
                    {
                        var component = await Task.Run(() => Components.ComponentsFactory.GetComponent(comp.Name));
                        if (component != null && !string.IsNullOrEmpty(component.CreateBinShortcut))
                        {
                            componentCombo.Items.Add(comp.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        DevStackConfig.WriteLog($"Erro ao verificar componente {comp.Name} para atalhos: {ex}");
                    }
                }

                componentCombo.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = $"Erro ao carregar componentes para atalhos: {ex.Message}";
                DevStackConfig.WriteLog($"Erro ao carregar componentes para atalhos na GUI: {ex}");
            }
        }

        /// <summary>
        /// Carrega as versões instaladas do componente selecionado para criação de atalho
        /// </summary>
        public static async Task LoadShortcutVersions(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedShortcutComponent))
            {
                // Limpar versões se nenhum componente selecionado
                var versionCombo = GuiHelpers.FindChild<ComboBox>(mainWindow, "ShortcutVersionCombo");
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
                    var status = DataManager.GetComponentStatus(mainWindow.SelectedShortcutComponent);
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        var versionCombo = GuiHelpers.FindChild<ComboBox>(mainWindow, "ShortcutVersionCombo");
                        if (versionCombo != null)
                        {
                            versionCombo.Items.Clear();
                            if (status.Installed && status.Versions.Any())
                            {
                                // Ordena as versões em ordem decrescente
                                foreach (var version in status.Versions
                                    .OrderByDescending(v => 
                                    {
                                        // Extrair apenas a parte da versão, removendo o nome do componente se presente
                                        var versionNumber = v;
                                        if (v.StartsWith($"{mainWindow.SelectedShortcutComponent}-"))
                                        {
                                            versionNumber = v.Substring(mainWindow.SelectedShortcutComponent.Length + 1);
                                        }
                                        return Version.TryParse(versionNumber, out var parsed) ? parsed : new Version(0, 0);
                                    }))
                                {
                                    // Extrair apenas a parte da versão, removendo o nome do componente
                                    var versionNumber = version;
                                    if (version.StartsWith($"{mainWindow.SelectedShortcutComponent}-"))
                                    {
                                        versionNumber = version.Substring(mainWindow.SelectedShortcutComponent.Length + 1);
                                    }
                                    versionCombo.Items.Add(versionNumber);
                                }
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = $"Erro ao carregar versões para atalho: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar versões para atalho na GUI: {ex}");
                    });
                }
            });
        }
    }
}
