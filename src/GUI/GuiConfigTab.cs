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
                Margin = new Thickness(10)
            };

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
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Gerenciamento do PATH
            var pathTitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.title"), true);
            pathTitleLabel.FontSize = 18;
            pathTitleLabel.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(pathTitleLabel);

            // Descrição
            var pathLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.path.title"));
            pathLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(pathLabel);

            // Botão Adicionar
            var addPathButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.config_tab.path.buttons.add"), (s, e) => AddToPath(mainWindow));
            addPathButton.Width = 200;
            addPathButton.Height = 35;
            addPathButton.Margin = new Thickness(10, 10, 0, 5);
            addPathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(addPathButton);

            // Botão Remover
            var removePathButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.config_tab.path.buttons.remove"), (s, e) => RemoveFromPath(mainWindow));
            removePathButton.Width = 200;
            removePathButton.Height = 35;
            removePathButton.Margin = new Thickness(10, 5, 0, 10);
            removePathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(removePathButton);

            // Gerenciamento dos Diretórios
            var dirsTitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.directories.title"), true);
            dirsTitleLabel.Margin = new Thickness(0, 10, 0, 5);
            panel.Children.Add(dirsTitleLabel);

            // Botão Abrir Pasta do Executável
            var openExeFolderButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.config_tab.directories.buttons.devstack_manager"), (s, e) => OpenExeFolder(mainWindow));
            openExeFolderButton.Width = 200;
            openExeFolderButton.Height = 35;
            openExeFolderButton.Margin = new Thickness(10, 5, 0, 5);
            openExeFolderButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(openExeFolderButton);

            // Botão Abrir Pasta das Ferramentas
            var openBaseDirFolderButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.config_tab.directories.buttons.tools"), (s, e) => OpenBaseDir(mainWindow));
            openBaseDirFolderButton.Width = 200;
            openBaseDirFolderButton.Height = 35;
            openBaseDirFolderButton.Margin = new Thickness(10, 10, 0, 10);
            openBaseDirFolderButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(openBaseDirFolderButton);

            // Gerenciamento da Linguagem
            var languagesTitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.languages.title"), true);
            languagesTitleLabel.Margin = new Thickness(0, 10, 0, 5);
            panel.Children.Add(languagesTitleLabel);

            // Language ComboBox
            var interfaceLanguageLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.languages.labels.interface_language"));
            panel.Children.Add(interfaceLanguageLabel);

            var languageComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            languageComboBox.Height = 30;
            languageComboBox.Margin = new Thickness(0, 5, 0, 5);
            languageComboBox.Name = "LanguageComboBox";

            // Popular opções de idioma
            var localization = mainWindow.LocalizationManager;
            var availableLanguages = localization.GetAvailableLanguages();
            foreach (var lang in availableLanguages)
            {
                var langName = localization.GetLanguageName(lang);
                var item = new ComboBoxItem
                {
                    Content = langName,
                    Tag = lang
                };
                languageComboBox.Items.Add(item);
                if (lang == localization.CurrentLanguage)
                {
                    languageComboBox.SelectedItem = item;
                }
            }

            // Evento para troca de idioma
            languageComboBox.SelectionChanged += (s, e) =>
            {
                if (languageComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string languageCode)
                {
                    localization.LoadLanguage(languageCode);
                    DevStackConfig.PersistSetting("language", languageCode);
                    // A janela principal ouvirá o evento e reconstruirá a UI
                }
            };

            panel.Children.Add(languageComboBox);

            // Gerenciamento dos temas
            var themesTitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.themes.title"), true);
            themesTitleLabel.Margin = new Thickness(0, 10, 0, 5);
            panel.Children.Add(themesTitleLabel);

            // tema ComboBox
            var themeLanguageLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.config_tab.themes.labels.interface_theme"));
            panel.Children.Add(themeLanguageLabel);

            var themeComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            themeComboBox.Height = 30;
            themeComboBox.Margin = new Thickness(0, 5, 0, 5);
            themeComboBox.Name = "ThemeComboBox";

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

            panel.Children.Add(themeComboBox);

            return panel;
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
                    DevStackConfig.pathManager?.AddBinDirsToPath();
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.path_updated");
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.config_tab.messages.path_error", ex.Message));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.path_update_error");
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
                    DevStackConfig.pathManager?.RemoveAllDevStackFromPath();
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.path_cleaned");
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.config_tab.messages.path_remove_error", ex.Message));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.path_clean_error");
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
                    if (!string.IsNullOrEmpty(folder))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = folder,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.exe_folder_opened");
                    }
                    else
                    {
                        progress.Report(mainWindow.LocalizationManager.GetString("gui.config_tab.messages.exe_folder_not_found"));
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.exe_folder_error");
                    }
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.config_tab.messages.exe_folder_error", ex.Message));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.exe_folder_error");
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
                    if (!string.IsNullOrEmpty(baseDir) && System.IO.Directory.Exists(baseDir))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = baseDir,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.tools_folder_opened");
                    }
                    else
                    {
                        progress.Report(mainWindow.LocalizationManager.GetString("gui.config_tab.messages.tools_folder_not_found"));
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.tools_folder_error");
                    }
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.config_tab.messages.tools_folder_error", ex.Message));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.config_tab.messages.tools_folder_error");
                }
                await Task.CompletedTask;
            });
        }
    }
}
