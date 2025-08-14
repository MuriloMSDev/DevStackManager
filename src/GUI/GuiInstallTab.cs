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

            // Título
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.title"), true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

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
            });
            installButton.Height = 40;
            installButton.FontSize = 14;
            installButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(installButton);

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
    }
}
