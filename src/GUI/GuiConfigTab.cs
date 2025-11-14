using System;
using System.Windows;
using System.Windows.Controls;

namespace DevStackManager
{
    /// <summary>
    /// Configuration tab component for managing DevStack system settings.
    /// Provides interface for PATH management (add/remove DevStack tools), directory access (executable and tools folders),
    /// language selection, and theme selection (Dark/Light mode).
    /// Two-column layout: configuration panel (left) and console output panel (right).
    /// </summary>
    public static class GuiConfigTab
    {
        /// <summary>
        /// Margin for scroll viewer in pixels.
        /// </summary>
        private const int SCROLL_VIEWER_MARGIN = 10;
        
        /// <summary>
        /// Bottom margin for panel in pixels.
        /// </summary>
        private const int PANEL_BOTTOM_MARGIN = 10;
        
        /// <summary>
        /// Font size for title text.
        /// </summary>
        private const int TITLE_FONT_SIZE = 18;
        
        /// <summary>
        /// Bottom margin for title in pixels.
        /// </summary>
        private const int TITLE_BOTTOM_MARGIN = 10;
        
        /// <summary>
        /// Top margin for section in pixels.
        /// </summary>
        private const int SECTION_TOP_MARGIN = 10;
        
        /// <summary>
        /// Bottom margin for section in pixels.
        /// </summary>
        private const int SECTION_BOTTOM_MARGIN = 5;
        
        /// <summary>
        /// Width of buttons in pixels.
        /// </summary>
        private const int BUTTON_WIDTH = 200;
        
        /// <summary>
        /// Height of buttons in pixels.
        /// </summary>
        private const int BUTTON_HEIGHT = 35;
        
        /// <summary>
        /// Top margin for buttons in pixels.
        /// </summary>
        private const int BUTTON_TOP_MARGIN = 10;
        
        /// <summary>
        /// Vertical margin for buttons in pixels.
        /// </summary>
        private const int BUTTON_VERTICAL_MARGIN = 5;
        
        /// <summary>
        /// Height of combo boxes in pixels.
        /// </summary>
        private const int COMBO_HEIGHT = 30;
        
        /// <summary>
        /// Top margin for combo boxes in pixels.
        /// </summary>
        private const int COMBO_TOP_MARGIN = 5;
        
        /// <summary>
        /// Bottom margin for combo boxes in pixels.
        /// </summary>
        private const int COMBO_BOTTOM_MARGIN = 5;

        /// <summary>
        /// Creates the complete Configuration tab content.
        /// Two-column layout: configuration selection panel (left) and console output panel (right).
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and data binding.</param>
        /// <returns>Grid with configuration interface and console panel.</returns>
        public static Grid CreateConfigContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftPanel = CreateConfigSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Config);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Creates the configuration selection panel with scrollable content.
        /// Contains PATH management, directory access, language selection, and theme selection sections.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>ScrollViewer with configuration options.</returns>
        private static ScrollViewer CreateConfigSelectionPanel(DevStackGui mainWindow)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(SCROLL_VIEWER_MARGIN)
            };

            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var panel = new StackPanel();

            var pathPanel = CreatePathConfigPanel(mainWindow);
            panel.Children.Add(pathPanel);

            scrollViewer.Content = panel;
            return scrollViewer;
        }

        /// <summary>
        /// Creates the PATH configuration panel with management sections.
        /// Sections: PATH management (add/remove buttons), directory management (open folders buttons),
        /// language selection (ComboBox), and theme selection (ComboBox).
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and data binding.</param>
        /// <returns>StackPanel with all configuration sections.</returns>
        private static StackPanel CreatePathConfigPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, PANEL_BOTTOM_MARGIN)
            };

            var pathTitleLabel = CreateSectionTitle(mainWindow, "gui.config_tab.title");
            panel.Children.Add(pathTitleLabel);

            var pathLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.path.title"));
            pathLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(pathLabel);

            var addPathButton = CreateActionButton(mainWindow, "gui.config_tab.path.buttons.add", AddToPath);
            addPathButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_TOP_MARGIN, 0, BUTTON_VERTICAL_MARGIN);
            panel.Children.Add(addPathButton);

            var removePathButton = CreateActionButton(mainWindow, "gui.config_tab.path.buttons.remove", RemoveFromPath);
            removePathButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_VERTICAL_MARGIN, 0, BUTTON_TOP_MARGIN);
            panel.Children.Add(removePathButton);

            var dirsTitleLabel = CreateSectionHeader(mainWindow, "gui.config_tab.directories.title");
            panel.Children.Add(dirsTitleLabel);

            var openExeFolderButton = CreateActionButton(mainWindow, "gui.config_tab.directories.buttons.devstack_manager", OpenExeFolder);
            openExeFolderButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_VERTICAL_MARGIN, 0, BUTTON_VERTICAL_MARGIN);
            panel.Children.Add(openExeFolderButton);

            var openBaseDirFolderButton = CreateActionButton(mainWindow, "gui.config_tab.directories.buttons.tools", OpenBaseDir);
            openBaseDirFolderButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_TOP_MARGIN, 0, BUTTON_TOP_MARGIN);
            panel.Children.Add(openBaseDirFolderButton);

            var languagesTitleLabel = CreateSectionHeader(mainWindow, "gui.config_tab.languages.title");
            panel.Children.Add(languagesTitleLabel);

            var interfaceLanguageLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.languages.labels.interface_language"));
            panel.Children.Add(interfaceLanguageLabel);

            var languageComboBox = CreateLanguageComboBox(mainWindow);
            panel.Children.Add(languageComboBox);

            var themesTitleLabel = CreateSectionHeader(mainWindow, "gui.config_tab.themes.title");
            panel.Children.Add(themesTitleLabel);

            var themeLanguageLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.themes.labels.interface_theme"));
            panel.Children.Add(themeLanguageLabel);

            var themeComboBox = CreateThemeComboBox(mainWindow);
            panel.Children.Add(themeComboBox);

            return panel;
        }

        /// <summary>
        /// Creates a main section title label with large font size.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <param name="localizationKey">Localization key for title text.</param>
        /// <returns>Styled label for section title.</returns>
        private static Label CreateSectionTitle(DevStackGui mainWindow, string localizationKey)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString(localizationKey), true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_BOTTOM_MARGIN);
            return titleLabel;
        }

        /// <summary>
        /// Creates a subsection header label with bold formatting.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <param name="localizationKey">Localization key for header text.</param>
        /// <returns>Styled label for subsection header.</returns>
        private static Label CreateSectionHeader(DevStackGui mainWindow, string localizationKey)
        {
            var headerLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString(localizationKey), true);
            headerLabel.Margin = new Thickness(0, SECTION_TOP_MARGIN, 0, SECTION_BOTTOM_MARGIN);
            return headerLabel;
        }

        /// <summary>
        /// Creates an action button with fixed dimensions and click handler.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <param name="localizationKey">Localization key for button text.</param>
        /// <param name="action">Action to execute when button is clicked.</param>
        /// <returns>Styled button with configured click handler.</returns>
        private static Button CreateActionButton(DevStackGui mainWindow, string localizationKey, Action<DevStackGui> action)
        {
            var button = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString(localizationKey), 
                (s, e) => action(mainWindow));
            button.Width = BUTTON_WIDTH;
            button.Height = BUTTON_HEIGHT;
            button.HorizontalAlignment = HorizontalAlignment.Left;
            return button;
        }

        /// <summary>
        /// Creates the language selection ComboBox with all available languages.
        /// Automatically selects current language and persists changes to DevStackConfig.
        /// Updates entire application UI when language is changed.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>ComboBox with language options.</returns>
        private static ComboBox CreateLanguageComboBox(DevStackGui mainWindow)
        {
            var languageComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            languageComboBox.Height = COMBO_HEIGHT;
            languageComboBox.Margin = new Thickness(0, COMBO_TOP_MARGIN, 0, COMBO_BOTTOM_MARGIN);
            languageComboBox.Name = "LanguageComboBox";

            var localization = mainWindow.LocalizationManager;
            var availableLanguages = localization.GetAvailableLanguages();
            var languageItems = new System.Collections.Generic.Dictionary<string, ComboBoxItem>();
            
            foreach (var lang in availableLanguages)
            {
                var langName = localization.GetLanguageName(lang);
                var item = new ComboBoxItem { Content = langName, Tag = lang };
                languageComboBox.Items.Add(item);
                languageItems[lang] = item;
            }
            
            languageComboBox.SelectedItem = languageItems.ContainsKey(DevStackShared.LocalizationManager.CurrentLanguageStatic)
                ? languageItems[DevStackShared.LocalizationManager.CurrentLanguageStatic]
                : languageItems.Values.FirstOrDefault();
                
            languageComboBox.SelectionChanged += (s, e) =>
            {
                if (languageComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is string code)
                {
                    DevStackShared.LocalizationManager.ApplyLanguage(code);
                    DevStackConfig.PersistSetting("language", code);
                }
            };
            
            return languageComboBox;
        }

        /// <summary>
        /// Creates the theme selection ComboBox with Dark and Light options.
        /// Automatically selects current theme and persists changes to DevStackConfig.
        /// Applies theme immediately when changed.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>ComboBox with theme options.</returns>
        private static ComboBox CreateThemeComboBox(DevStackGui mainWindow)
        {
            var themeComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            themeComboBox.Height = COMBO_HEIGHT;
            themeComboBox.Margin = new Thickness(0, COMBO_TOP_MARGIN, 0, COMBO_BOTTOM_MARGIN);
            themeComboBox.Name = "ThemeComboBox";

            var localization = mainWindow.LocalizationManager;
            
            var darkItem = new ComboBoxItem { Content = localization.GetString("common.themes.dark"), Tag = DevStackShared.ThemeManager.ThemeType.Dark };
            var lightItem = new ComboBoxItem { Content = localization.GetString("common.themes.light"), Tag = DevStackShared.ThemeManager.ThemeType.Light };
            themeComboBox.Items.Add(darkItem);
            themeComboBox.Items.Add(lightItem);
            themeComboBox.SelectedItem = DevStackShared.ThemeManager.CurrentThemeType == DevStackShared.ThemeManager.ThemeType.Light ? lightItem : darkItem;

            themeComboBox.SelectionChanged += (s, e) =>
            {
                if (themeComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is DevStackShared.ThemeManager.ThemeType type)
                {
                    DevStackShared.ThemeManager.ApplyTheme(type);
                    DevStackConfig.PersistSetting("theme", type);
                }
            };

            return themeComboBox;
        }

        /// <summary>
        /// Adds DevStack tool directories to system PATH environment variable.
        /// Executes via PathManager.AddBinDirsToPath with console output reporting.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status updates and console output.</param>
        private static void AddToPath(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
            {
                try
                {
                    ExecutePathOperation(
                        () => DevStackConfig.pathManager?.AddBinDirsToPath(),
                        mainWindow,
                        "gui.config_tab.messages.path_updated",
                        "gui.config_tab.messages.path_error",
                        "gui.config_tab.messages.path_update_error",
                        progress);
                }
                catch (Exception ex)
                {
                    HandleOperationError(mainWindow, progress, ex, "gui.config_tab.messages.path_error", "gui.config_tab.messages.path_update_error");
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Removes all DevStack tool directories from system PATH environment variable.
        /// Executes via PathManager.RemoveAllDevStackFromPath with console output reporting.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status updates and console output.</param>
        private static void RemoveFromPath(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
            {
                try
                {
                    ExecutePathOperation(
                        () => DevStackConfig.pathManager?.RemoveAllDevStackFromPath(),
                        mainWindow,
                        "gui.config_tab.messages.path_cleaned",
                        "gui.config_tab.messages.path_remove_error",
                        "gui.config_tab.messages.path_clean_error",
                        progress);
                }
                catch (Exception ex)
                {
                    HandleOperationError(mainWindow, progress, ex, "gui.config_tab.messages.path_remove_error", "gui.config_tab.messages.path_clean_error");
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Opens the folder containing the DevStackGUI.exe executable in Windows Explorer.
        /// Uses AppContext.BaseDirectory to locate the executable folder.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status updates and console output.</param>
        private static void OpenExeFolder(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
            {
                try
                {
                    var folder = System.AppContext.BaseDirectory;
                    ExecuteFolderOperation(
                        folder,
                        mainWindow,
                        progress,
                        "gui.config_tab.messages.exe_folder_opened",
                        "gui.config_tab.messages.exe_folder_not_found",
                        "gui.config_tab.messages.exe_folder_error");
                }
                catch (Exception ex)
                {
                    HandleOperationError(mainWindow, progress, ex, "gui.config_tab.messages.exe_folder_error", "gui.config_tab.messages.exe_folder_error");
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Opens the base tools directory (DevStackConfig.baseDir) in Windows Explorer.
        /// This is the root directory where all DevStack tools are installed.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status updates and console output.</param>
        private static void OpenBaseDir(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
            {
                try
                {
                    var baseDir = DevStackConfig.baseDir;
                    ExecuteFolderOperation(
                        baseDir,
                        mainWindow,
                        progress,
                        "gui.config_tab.messages.tools_folder_opened",
                        "gui.config_tab.messages.tools_folder_not_found",
                        "gui.config_tab.messages.tools_folder_error",
                        true);
                }
                catch (Exception ex)
                {
                    HandleOperationError(mainWindow, progress, ex, "gui.config_tab.messages.tools_folder_error", "gui.config_tab.messages.tools_folder_error");
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Executes a PATH operation with error handling and status reporting.
        /// Updates status bar with success or error messages based on operation result.
        /// </summary>
        /// <param name="operation">PATH operation to execute.</param>
        /// <param name="mainWindow">Main window instance for status updates.</param>
        /// <param name="successMessageKey">Localization key for success message.</param>
        /// <param name="errorMessageKey">Localization key for console error message.</param>
        /// <param name="statusErrorMessageKey">Localization key for status bar error message.</param>
        /// <param name="progress">Progress reporter for console output.</param>
        private static void ExecutePathOperation(
            Action operation,
            DevStackGui mainWindow,
            string successMessageKey,
            string errorMessageKey,
            string statusErrorMessageKey,
            IProgress<string> progress)
        {
            try
            {
                operation();
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(successMessageKey);
            }
            catch (Exception ex)
            {
                progress.Report(mainWindow.LocalizationManager.GetString(errorMessageKey, ex.Message));
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(statusErrorMessageKey);
            }
        }

        /// <summary>
        /// Executes a folder open operation with validation and error handling.
        /// Opens folder in Windows Explorer with UseShellExecute.
        /// </summary>
        /// <param name="folder">Folder path to open.</param>
        /// <param name="mainWindow">Main window instance for status updates.</param>
        /// <param name="progress">Progress reporter for console output.</param>
        /// <param name="successMessageKey">Localization key for success message.</param>
        /// <param name="notFoundMessageKey">Localization key for folder not found message.</param>
        /// <param name="errorMessageKey">Localization key for error message.</param>
        /// <param name="checkExists">Whether to check if folder exists before opening.</param>
        private static void ExecuteFolderOperation(
            string folder,
            DevStackGui mainWindow,
            IProgress<string> progress,
            string successMessageKey,
            string notFoundMessageKey,
            string errorMessageKey,
            bool checkExists = false)
        {
            if (!string.IsNullOrEmpty(folder) && (!checkExists || System.IO.Directory.Exists(folder)))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true,
                    Verb = "open"
                });
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(successMessageKey);
            }
            else
            {
                progress.Report(mainWindow.LocalizationManager.GetString(notFoundMessageKey));
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(errorMessageKey);
            }
        }

        /// <summary>
        /// Handles operation errors by reporting to console and status bar.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status updates.</param>
        /// <param name="progress">Progress reporter for console output.</param>
        /// <param name="ex">Exception that occurred.</param>
        /// <param name="errorMessageKey">Localization key for console error message.</param>
        /// <param name="statusMessageKey">Localization key for status bar error message.</param>
        private static void HandleOperationError(
            DevStackGui mainWindow,
            IProgress<string> progress,
            Exception ex,
            string errorMessageKey,
            string statusMessageKey)
        {
            progress.Report(mainWindow.LocalizationManager.GetString(errorMessageKey, ex.Message));
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(statusMessageKey);
        }
    }
}
