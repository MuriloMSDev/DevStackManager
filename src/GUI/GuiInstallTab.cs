using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
        private const double TITLE_FONT_SIZE = 18;
        private const double SECTION_FONT_SIZE = 14;
        private const double BUTTON_FONT_SIZE = 14;
        private const double CONTROL_HEIGHT = 30;
        private const double BUTTON_HEIGHT = 40;

        /// <summary>
        /// Converter que transforma o Name técnico (string) em Label do componente
        /// </summary>
        public class NameToLabelConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string name && !string.IsNullOrWhiteSpace(name))
                {
                    var comp = Components.ComponentsFactory.GetComponent(name);
                    return comp?.Label ?? name;
                }
                return value ?? string.Empty;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// Cria o conteúdo completo da aba "Instalar"
        /// </summary>
        public static Grid CreateInstallContent(DevStackGui mainWindow)
        {
            var grid = CreateTwoColumnGrid();
            
            var leftPanel = CreateInstallSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Install);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        private static Grid CreateTwoColumnGrid()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            return grid;
        }

        /// <summary>
        /// Cria o painel de seleção de componentes para instalação
        /// </summary>
        private static UIElement CreateInstallSelectionPanel(DevStackGui mainWindow)
        {
            var grid = new Grid();
            var panel = new StackPanel { Margin = new Thickness(10) };

            AddTitleSection(panel, mainWindow);
            AddInstallSection(panel, mainWindow);
            AddShortcutSection(panel, mainWindow);

            grid.Children.Add(panel);
            
            var overlay = CreateLoadingOverlay(mainWindow);
            grid.Children.Add(overlay);

            return grid;
        }

        private static void AddTitleSection(StackPanel panel, DevStackGui mainWindow)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.install_tab.title"), 
                true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);
        }

        private static void AddInstallSection(StackPanel panel, DevStackGui mainWindow)
        {
            var sectionLabel = CreateSectionLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.sections.install_component"));
            panel.Children.Add(sectionLabel);

            var componentCombo = CreateComponentComboBox(mainWindow);
            panel.Children.Add(componentCombo);

            var versionCombo = CreateVersionComboBox(mainWindow);
            panel.Children.Add(versionCombo);

            var installButton = CreateInstallButton(mainWindow);
            panel.Children.Add(installButton);
        }

        private static Label CreateSectionLabel(string text)
        {
            var label = DevStackShared.ThemeManager.CreateStyledLabel(text, true);
            label.FontSize = SECTION_FONT_SIZE;
            label.Margin = new Thickness(0, 10, 0, 10);
            return label;
        }

        private static UIElement CreateComponentComboBox(DevStackGui mainWindow)
        {
            var container = new StackPanel();
            
            var label = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.install_tab.labels.select_tool"));
            container.Children.Add(label);

            var combo = DevStackShared.ThemeManager.CreateStyledComboBox();
            combo.Margin = new Thickness(0, 5, 0, 15);
            combo.Height = CONTROL_HEIGHT;
            
            combo.SetBinding(ComboBox.ItemsSourceProperty, 
                new Binding("AvailableComponents") { Source = mainWindow });
            combo.SetBinding(ComboBox.SelectedValueProperty, 
                new Binding("SelectedComponent") { Source = mainWindow });
            
            combo.ItemTemplate = CreateComponentItemTemplate();
            container.Children.Add(combo);

            return container;
        }

        private static DataTemplate CreateComponentItemTemplate()
        {
            var template = new DataTemplate();
            var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, 
                new Binding(".") { Converter = new NameToLabelConverter() });
            template.VisualTree = textFactory;
            return template;
        }

        private static UIElement CreateVersionComboBox(DevStackGui mainWindow)
        {
            var container = new StackPanel();
            
            var label = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.install_tab.labels.select_version"));
            container.Children.Add(label);

            var combo = DevStackShared.ThemeManager.CreateStyledComboBox();
            combo.Margin = new Thickness(0, 5, 0, 20);
            combo.Height = CONTROL_HEIGHT;
            
            combo.SetBinding(ComboBox.ItemsSourceProperty, 
                new Binding("AvailableVersions") { Source = mainWindow });
            combo.SetBinding(ComboBox.SelectedValueProperty, 
                new Binding("SelectedVersion") { Source = mainWindow });
            
            container.Children.Add(combo);

            return container;
        }

        private static Button CreateInstallButton(DevStackGui mainWindow)
        {
            var button = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString("gui.install_tab.buttons.install"),
                async (s, e) => await HandleInstallButtonClick(mainWindow),
                DevStackShared.ThemeManager.ButtonStyle.Success);
            
            button.Height = BUTTON_HEIGHT;
            button.FontSize = BUTTON_FONT_SIZE;
            button.Margin = new Thickness(0, 10, 0, 20);
            
            return button;
        }

        private static async Task HandleInstallButtonClick(DevStackGui mainWindow)
        {
            mainWindow.IsInstallingComponent = true;
            try
            {
                await InstallComponent(mainWindow);
            }
            finally
            {
                ResetInstallState(mainWindow);
            }
        }

        private static void ResetInstallState(DevStackGui mainWindow)
        {
            mainWindow.IsInstallingComponent = false;
            mainWindow.SelectedComponent = string.Empty;
            mainWindow.SelectedVersion = string.Empty;
        }

        private static UIElement CreateLoadingOverlay(DevStackGui mainWindow)
        {
            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
            overlay.Visibility = mainWindow.IsInstallingComponent ? Visibility.Visible : Visibility.Collapsed;
            
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsInstallingComponent))
                {
                    overlay.Visibility = mainWindow.IsInstallingComponent 
                        ? Visibility.Visible 
                        : Visibility.Collapsed;
                }
            };

            return overlay;
        }

        private static void AddShortcutSection(StackPanel panel, DevStackGui mainWindow)
        {
            var sectionLabel = CreateSectionLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.sections.create_shortcuts"));
            panel.Children.Add(sectionLabel);

            // Componente Instalado
            var installedComponentLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.labels.installed_component"));
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
            // Mostrar Label para cada item (items são strings com o Name técnico)
            var shortcutItemTemplate = new DataTemplate();
            var shortcutTextFactory = new FrameworkElementFactory(typeof(TextBlock));
            shortcutTextFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new NameToLabelConverter() });
            shortcutItemTemplate.VisualTree = shortcutTextFactory;
            installedComponentCombo.ItemTemplate = shortcutItemTemplate;
            panel.Children.Add(installedComponentCombo);

            // Versão Instalada
            var installedVersionLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.labels.installed_version"));
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
            var createShortcutButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.install_tab.buttons.create_shortcut"), async (s, e) =>
            {
                await CreateShortcutForComponent(mainWindow);
            });
            createShortcutButton.Height = 35;
            createShortcutButton.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(createShortcutButton);
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
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.select_component_warning"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(mainWindow.SelectedShortcutVersion))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.select_version_warning"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.creating_shortcut", mainWindow.SelectedShortcutComponent, mainWindow.SelectedShortcutVersion);

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
                            throw new Exception(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.shortcut_component_not_found", mainWindow.SelectedShortcutComponent));
                        }

                        // Verificar se o componente tem CreateBinShortcut definido
                        if (string.IsNullOrEmpty(comp.CreateBinShortcut))
                        {
                            throw new Exception(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.shortcut_not_supported", mainWindow.SelectedShortcutComponent));
                        }

                        // Construir caminhos
                        string baseToolDir = !string.IsNullOrEmpty(comp.ToolDir) ? comp.ToolDir : System.IO.Path.Combine(DevStackConfig.baseDir, comp.Name);
                        string subDir = $"{comp.Name}-{mainWindow.SelectedShortcutVersion}";
                        string targetDir = System.IO.Path.Combine(baseToolDir, subDir);

                        if (!System.IO.Directory.Exists(targetDir))
                        {
                            throw new Exception(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.shortcut_install_dir_not_found", targetDir));
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

                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.shortcut_created", mainWindow.SelectedShortcutComponent, mainWindow.SelectedShortcutVersion);
                    }
                    catch (Exception ex)
                    {
                        progress.Report(mainWindow.LocalizationManager.GetString("gui.status_bar.shortcut_create_error", ex.Message));
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.shortcut_error", mainWindow.SelectedShortcutComponent);
                        DevStackConfig.WriteLog($"Erro ao criar atalho na GUI: {ex}");
                    }
                });
            });
        }

    }
}
