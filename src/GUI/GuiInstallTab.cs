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
    /// Install tab component for installing new development tools and creating shortcuts.
    /// Provides component selection, version selection, installation progress, and shortcut creation.
    /// Uses GuiConsolePanel for installation output display.
    /// </summary>
    public static class GuiInstallTab
    {
        /// <summary>
        /// Font size for title text.
        /// </summary>
        private const double TITLE_FONT_SIZE = 18;
        
        /// <summary>
        /// Font size for section headers.
        /// </summary>
        private const double SECTION_FONT_SIZE = 14;
        
        /// <summary>
        /// Font size for buttons.
        /// </summary>
        private const double BUTTON_FONT_SIZE = 14;
        
        /// <summary>
        /// Height of input controls (ComboBoxes, TextBoxes).
        /// </summary>
        private const double CONTROL_HEIGHT = 30;
        
        /// <summary>
        /// Height of action buttons.
        /// </summary>
        private const double BUTTON_HEIGHT = 40;

        /// <summary>
        /// Converter that transforms technical component Name to user-friendly Label.
        /// Used in ComboBox ItemTemplates to display localized component names.
        /// </summary>
        public class NameToLabelConverter : IValueConverter
        {
            /// <summary>
            /// Converts component Name string to Label using ComponentsFactory.
            /// </summary>
            /// <param name="value">Component Name string.</param>
            /// <param name="targetType">Target property type.</param>
            /// <param name="parameter">Converter parameter (unused).</param>
            /// <param name="culture">Culture info.</param>
            /// <returns>Component Label or original Name if component not found.</returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string name && !string.IsNullOrWhiteSpace(name))
                {
                    var comp = Components.ComponentsFactory.GetComponent(name);
                    return comp?.Label ?? name;
                }
                return value ?? string.Empty;
            }

            /// <summary>
            /// Not implemented - one-way conversion only.
            /// </summary>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// Creates the complete Install tab content with two-column layout.
        /// Left column: component selection and installation controls.
        /// Right column: console output for installation progress.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and localization.</param>
        /// <returns>Grid with install controls and console panel.</returns>
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

        /// <summary>
        /// Creates two-column grid layout for Install tab.
        /// </summary>
        /// <returns>Grid with two equal-width columns.</returns>
        private static Grid CreateTwoColumnGrid()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            return grid;
        }

        /// <summary>
        /// Creates the component selection panel with installation and shortcut sections.
        /// Includes loading overlay for installation progress feedback.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding.</param>
        /// <returns>UIElement with selection controls and loading overlay.</returns>
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

        /// <summary>
        /// Adds the title section to the install panel.
        /// </summary>
        /// <param name="panel">StackPanel to add title to.</param>
        /// <param name="mainWindow">Main window instance for localization.</param>
        private static void AddTitleSection(StackPanel panel, DevStackGui mainWindow)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.install_tab.title"), 
                true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);
        }

        /// <summary>
        /// Adds the component installation section with selectors and install button.
        /// </summary>
        /// <param name="panel">StackPanel to add controls to.</param>
        /// <param name="mainWindow">Main window instance for data binding.</param>
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

        /// <summary>
        /// Creates a styled section label with bold font.
        /// </summary>
        /// <param name="text">Label text.</param>
        /// <returns>Styled Label with section formatting.</returns>
        private static Label CreateSectionLabel(string text)
        {
            var label = DevStackShared.ThemeManager.CreateStyledLabel(text, true);
            label.FontSize = SECTION_FONT_SIZE;
            label.Margin = new Thickness(0, 10, 0, 10);
            return label;
        }

        /// <summary>
        /// Creates the component selection ComboBox with data binding.
        /// Binds to AvailableComponents and SelectedComponent properties.
        /// Uses NameToLabelConverter to display user-friendly names.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding.</param>
        /// <returns>UIElement with label and ComboBox.</returns>
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

        /// <summary>
        /// Creates DataTemplate for component ComboBox items.
        /// Uses NameToLabelConverter to display component Labels instead of Names.
        /// </summary>
        /// <returns>DataTemplate with TextBlock and converter binding.</returns>
        private static DataTemplate CreateComponentItemTemplate()
        {
            var template = new DataTemplate();
            var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, 
                new Binding(".") { Converter = new NameToLabelConverter() });
            template.VisualTree = textFactory;
            return template;
        }

        /// <summary>
        /// Creates the version selection ComboBox with data binding.
        /// Binds to AvailableVersions and SelectedVersion properties.
        /// Automatically populated when component is selected.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding.</param>
        /// <returns>UIElement with label and ComboBox.</returns>
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

        /// <summary>
        /// Creates the Install button with click handler.
        /// Styled with Success theme and calls HandleInstallButtonClick on click.
        /// </summary>
        /// <param name="mainWindow">Main window instance for event handling.</param>
        /// <returns>Styled Install button.</returns>
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

        /// <summary>
        /// Handles Install button click event.
        /// Sets IsInstallingComponent flag, calls InstallComponent, and resets state.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        /// <returns>Task representing the async operation.</returns>
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

        /// <summary>
        /// Resets installation state after completion.
        /// Clears IsInstallingComponent flag and selected component/version.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        private static void ResetInstallState(DevStackGui mainWindow)
        {
            mainWindow.IsInstallingComponent = false;
            mainWindow.SelectedComponent = string.Empty;
            mainWindow.SelectedVersion = string.Empty;
        }

        /// <summary>
        /// Creates loading overlay that displays during component installation.
        /// Visibility bound to IsInstallingComponent property.
        /// </summary>
        /// <param name="mainWindow">Main window instance for property change notification.</param>
        /// <returns>UIElement with loading indicator.</returns>
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

        /// <summary>
        /// Adds the shortcut creation section to the install panel.
        /// Provides component/version selection and shortcut creation button.
        /// </summary>
        /// <param name="panel">StackPanel to add shortcut controls to.</param>
        /// <param name="mainWindow">Main window instance for data binding.</param>
        private static void AddShortcutSection(StackPanel panel, DevStackGui mainWindow)
        {
            var sectionLabel = CreateSectionLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.sections.create_shortcuts"));
            panel.Children.Add(sectionLabel);

            var installedComponentLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.labels.installed_component"));
            panel.Children.Add(installedComponentLabel);

            var installedComponentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            installedComponentCombo.Margin = new Thickness(0, 5, 0, 15);
            installedComponentCombo.Height = 30;
            installedComponentCombo.Name = "ShortcutComponentCombo";
            
            var shortcutComponentsBinding = new Binding("ShortcutComponents") { Source = mainWindow };
            installedComponentCombo.SetBinding(ComboBox.ItemsSourceProperty, shortcutComponentsBinding);
            
            var selectedShortcutComponentBinding = new Binding("SelectedShortcutComponent") { Source = mainWindow };
            installedComponentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedShortcutComponentBinding);
            
            var shortcutItemTemplate = new DataTemplate();
            var shortcutTextFactory = new FrameworkElementFactory(typeof(TextBlock));
            shortcutTextFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new NameToLabelConverter() });
            shortcutItemTemplate.VisualTree = shortcutTextFactory;
            installedComponentCombo.ItemTemplate = shortcutItemTemplate;
            panel.Children.Add(installedComponentCombo);

            var installedVersionLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.install_tab.labels.installed_version"));
            panel.Children.Add(installedVersionLabel);

            var installedVersionCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            installedVersionCombo.Margin = new Thickness(0, 5, 0, 20);
            installedVersionCombo.Height = 30;
            installedVersionCombo.Name = "ShortcutVersionCombo";
            
            var shortcutVersionsBinding = new Binding("ShortcutVersions") { Source = mainWindow };
            installedVersionCombo.SetBinding(ComboBox.ItemsSourceProperty, shortcutVersionsBinding);
            
            var selectedShortcutVersionBinding = new Binding("SelectedShortcutVersion") { Source = mainWindow };
            installedVersionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedShortcutVersionBinding);
            panel.Children.Add(installedVersionCombo);

            var createShortcutButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.install_tab.buttons.create_shortcut"), async (s, e) =>
            {
                await CreateShortcutForComponent(mainWindow);
            });
            createShortcutButton.Height = 35;
            createShortcutButton.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(createShortcutButton);
        }

        /// <summary>
        /// Installs the selected component with optional version specification.
        /// Uses InstallManager for installation and updates PATH environment variable.
        /// Outputs progress to Install console tab and updates installed components list.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        /// <returns>Task representing the async installation operation.</returns>
        public static async Task InstallComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedComponent))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.select_component"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.installing", mainWindow.SelectedComponent);

            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Install, async progress =>
            {
                try
                {
                    var args = string.IsNullOrEmpty(mainWindow.SelectedVersion)
                        ? new[] { mainWindow.SelectedComponent }
                        : new[] { mainWindow.SelectedComponent, mainWindow.SelectedVersion };

                    await InstallManager.InstallCommands(args);

                    if (DevStackConfig.pathManager != null)
                    {
                        DevStackConfig.pathManager.AddBinDirsToPath();
                    }
                    else
                    {
                        progress.Report(mainWindow.LocalizationManager.GetString("gui.services_tab.path_manager.not_initialized"));
                    }

                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.install_tab.messages.success", mainWindow.SelectedComponent);

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
        /// Creates a desktop/bin shortcut for the selected component and version.
        /// Validates component supports shortcuts, locates installation directory, and creates global bin shortcut.
        /// Outputs progress to Install console tab.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        /// <returns>Task representing the async shortcut creation operation.</returns>
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

            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Install, async progress =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var comp = Components.ComponentsFactory.GetComponent(mainWindow.SelectedShortcutComponent);
                        if (comp == null)
                        {
                            throw new Exception(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.shortcut_component_not_found", mainWindow.SelectedShortcutComponent));
                        }

                        if (string.IsNullOrEmpty(comp.CreateBinShortcut))
                        {
                            throw new Exception(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.shortcut_not_supported", mainWindow.SelectedShortcutComponent));
                        }

                        string baseToolDir = !string.IsNullOrEmpty(comp.ToolDir) ? comp.ToolDir : System.IO.Path.Combine(DevStackConfig.baseDir, comp.Name);
                        string subDir = $"{comp.Name}-{mainWindow.SelectedShortcutVersion}";
                        string targetDir = System.IO.Path.Combine(baseToolDir, subDir);

                        if (!System.IO.Directory.Exists(targetDir))
                        {
                            throw new Exception(mainWindow.LocalizationManager.GetString("gui.install_tab.messages.shortcut_install_dir_not_found", targetDir));
                        }

                        string shortcutName = comp.CreateBinShortcut.Replace("{version}", mainWindow.SelectedShortcutVersion);
                        string sourceDir = targetDir;
                        string sourcePattern;

                        if (!string.IsNullOrEmpty(comp.ExecutablePattern))
                        {
                            sourcePattern = comp.ExecutablePattern;

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
                            sourcePattern = shortcutName;
                        }

                        Components.ComponentBase.CreateGlobalBinShortcut(sourceDir, sourcePattern, mainWindow.SelectedShortcutVersion, comp.Name, shortcutName);

                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.shortcut_created", mainWindow.SelectedShortcutComponent, mainWindow.SelectedShortcutVersion);
                    }
                    catch (Exception ex)
                    {
                        progress.Report(mainWindow.LocalizationManager.GetString("gui.status_bar.shortcut_create_error", ex.Message));
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.shortcut_error", mainWindow.SelectedShortcutComponent);
                        DevStackConfig.WriteLog($"Error creating shortcut in GUI: {ex}");
                    }
                });
            });
        }

    }
}
