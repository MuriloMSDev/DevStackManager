using System;
using System.Windows;
using System.Windows.Controls;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela aba "Configurações" - gerencia configurações do sistema
    /// </summary>
    public static class GuiConfigTab
    {
        // UI Dimensions Constants
        private const int SCROLL_VIEWER_MARGIN = 10;
        private const int PANEL_BOTTOM_MARGIN = 10;
        private const int TITLE_FONT_SIZE = 18;
        private const int TITLE_BOTTOM_MARGIN = 10;
        private const int SECTION_TOP_MARGIN = 10;
        private const int SECTION_BOTTOM_MARGIN = 5;
        private const int BUTTON_WIDTH = 200;
        private const int BUTTON_HEIGHT = 35;
        private const int BUTTON_TOP_MARGIN = 10;
        private const int BUTTON_VERTICAL_MARGIN = 5;
        private const int COMBO_HEIGHT = 30;
        private const int COMBO_TOP_MARGIN = 5;
        private const int COMBO_BOTTOM_MARGIN = 5;

        /// <summary>
        /// Cria o conteúdo completo da aba "Configurações"
        /// </summary>
        public static Grid CreateConfigContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Configurações
            var leftPanel = CreateConfigSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console de saída
            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Config);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de seleção de configurações
        /// </summary>
        private static ScrollViewer CreateConfigSelectionPanel(DevStackGui mainWindow)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(SCROLL_VIEWER_MARGIN)
            };

            // Aplicar scrollbar customizada do ThemeManager
            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var panel = new StackPanel();

            // Configurações de Path
            var pathPanel = CreatePathConfigPanel(mainWindow);
            panel.Children.Add(pathPanel);

            scrollViewer.Content = panel;
            return scrollViewer;
        }

        /// <summary>
        /// Cria o painel de configurações do PATH (StackPanel estilizado)
        /// </summary>
        private static StackPanel CreatePathConfigPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, PANEL_BOTTOM_MARGIN)
            };

            // Gerenciamento do PATH
            var pathTitleLabel = CreateSectionTitle(mainWindow, "gui.config_tab.title");
            panel.Children.Add(pathTitleLabel);

            // Descrição
            var pathLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.path.title"));
            pathLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(pathLabel);

            // Botão Adicionar
            var addPathButton = CreateActionButton(mainWindow, "gui.config_tab.path.buttons.add", AddToPath);
            addPathButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_TOP_MARGIN, 0, BUTTON_VERTICAL_MARGIN);
            panel.Children.Add(addPathButton);

            // Botão Remover
            var removePathButton = CreateActionButton(mainWindow, "gui.config_tab.path.buttons.remove", RemoveFromPath);
            removePathButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_VERTICAL_MARGIN, 0, BUTTON_TOP_MARGIN);
            panel.Children.Add(removePathButton);

            // Gerenciamento dos Diretórios
            var dirsTitleLabel = CreateSectionHeader(mainWindow, "gui.config_tab.directories.title");
            panel.Children.Add(dirsTitleLabel);

            // Botão Abrir Pasta do Executável
            var openExeFolderButton = CreateActionButton(mainWindow, "gui.config_tab.directories.buttons.devstack_manager", OpenExeFolder);
            openExeFolderButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_VERTICAL_MARGIN, 0, BUTTON_VERTICAL_MARGIN);
            panel.Children.Add(openExeFolderButton);

            // Botão Abrir Pasta das Ferramentas
            var openBaseDirFolderButton = CreateActionButton(mainWindow, "gui.config_tab.directories.buttons.tools", OpenBaseDir);
            openBaseDirFolderButton.Margin = new Thickness(BUTTON_TOP_MARGIN, BUTTON_TOP_MARGIN, 0, BUTTON_TOP_MARGIN);
            panel.Children.Add(openBaseDirFolderButton);

            // Gerenciamento da Linguagem
            var languagesTitleLabel = CreateSectionHeader(mainWindow, "gui.config_tab.languages.title");
            panel.Children.Add(languagesTitleLabel);

            // Language ComboBox (nova lógica igual à StatusBar)
            var interfaceLanguageLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.languages.labels.interface_language"));
            panel.Children.Add(interfaceLanguageLabel);

            var languageComboBox = CreateLanguageComboBox(mainWindow);
            panel.Children.Add(languageComboBox);

            // Gerenciamento dos temas
            var themesTitleLabel = CreateSectionHeader(mainWindow, "gui.config_tab.themes.title");
            panel.Children.Add(themesTitleLabel);

            // tema ComboBox
            var themeLanguageLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.themes.labels.interface_theme"));
            panel.Children.Add(themeLanguageLabel);

            var themeComboBox = CreateThemeComboBox(mainWindow);
            panel.Children.Add(themeComboBox);

            return panel;
        }

        private static Label CreateSectionTitle(DevStackGui mainWindow, string localizationKey)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString(localizationKey), true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_BOTTOM_MARGIN);
            return titleLabel;
        }

        private static Label CreateSectionHeader(DevStackGui mainWindow, string localizationKey)
        {
            var headerLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString(localizationKey), true);
            headerLabel.Margin = new Thickness(0, SECTION_TOP_MARGIN, 0, SECTION_BOTTOM_MARGIN);
            return headerLabel;
        }

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

        private static ComboBox CreateThemeComboBox(DevStackGui mainWindow)
        {
            var themeComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            themeComboBox.Height = COMBO_HEIGHT;
            themeComboBox.Margin = new Thickness(0, COMBO_TOP_MARGIN, 0, COMBO_BOTTOM_MARGIN);
            themeComboBox.Name = "ThemeComboBox";

            var localization = mainWindow.LocalizationManager;
            
            // Popular opções de tema
            var darkItem = new ComboBoxItem { Content = localization.GetString("common.themes.dark"), Tag = DevStackShared.ThemeManager.ThemeType.Dark };
            var lightItem = new ComboBoxItem { Content = localization.GetString("common.themes.light"), Tag = DevStackShared.ThemeManager.ThemeType.Light };
            themeComboBox.Items.Add(darkItem);
            themeComboBox.Items.Add(lightItem);
            themeComboBox.SelectedItem = DevStackShared.ThemeManager.CurrentThemeType == DevStackShared.ThemeManager.ThemeType.Light ? lightItem : darkItem;

            // Evento para troca de tema
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
        /// Adiciona as ferramentas do DevStack ao PATH do sistema
        /// </summary>
        private static void AddToPath(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, mainWindow, async progress =>
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
        /// Remove as ferramentas do DevStack do PATH do sistema
        /// </summary>
        private static void RemoveFromPath(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, mainWindow, async progress =>
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
        /// Abre a pasta onde está o executável DevStackGUI.exe
        /// </summary>
        private static void OpenExeFolder(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, mainWindow, async progress =>
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
        /// Abre a pasta base das ferramentas (DevStackConfig.baseDir)
        /// </summary>
        private static void OpenBaseDir(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, mainWindow, async progress =>
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
