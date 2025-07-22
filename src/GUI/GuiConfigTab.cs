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
            var pathTitleLabel = GuiTheme.CreateStyledLabel("Gerenciamento do PATH", true);
            pathTitleLabel.FontSize = 18;
            pathTitleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(pathTitleLabel);

            // Descrição
            var pathLabel = GuiTheme.CreateStyledLabel("Adicionar ferramentas ao PATH do sistema");
            pathLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(pathLabel);

            // Botão Adicionar
            var addPathButton = GuiTheme.CreateStyledButton("➕ Adicionar ao PATH", (s, e) => AddToPath(mainWindow));
            addPathButton.Width = 200;
            addPathButton.Height = 35;
            addPathButton.Margin = new Thickness(0, 10, 0, 5);
            addPathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(addPathButton);

            // Botão Remover
            var removePathButton = GuiTheme.CreateStyledButton("➖ Remover do PATH", (s, e) => RemoveFromPath(mainWindow));
            removePathButton.Width = 200;
            removePathButton.Height = 35;
            removePathButton.Margin = new Thickness(0, 5, 0, 5);
            removePathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(removePathButton);

            // Botão Listar
            var listPathButton = GuiTheme.CreateStyledButton("📋 Listar PATH Atual", (s, e) => ListCurrentPath(mainWindow));
            listPathButton.Width = 200;
            listPathButton.Height = 35;
            listPathButton.Margin = new Thickness(0, 5, 0, 10);
            listPathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(listPathButton);

            // Info
            var infoLabel = GuiTheme.CreateStyledLabel("ℹ️ As alterações no PATH afetam o terminal e o sistema.");
            infoLabel.FontStyle = FontStyles.Italic;
            infoLabel.Margin = new Thickness(0, 10, 0, 20);
            panel.Children.Add(infoLabel);

            // Gerenciamento do PATH
            var dirsTitleLabel = GuiTheme.CreateStyledLabel("Diretórios", true);
            dirsTitleLabel.FontSize = 18;
            dirsTitleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(dirsTitleLabel);

            // Botão Abrir Pasta do Executável
            var openExeFolderButton = GuiTheme.CreateStyledButton("📂 DevStack Manager", (s, e) => OpenExeFolder(mainWindow));
            openExeFolderButton.Width = 200;
            openExeFolderButton.Height = 35;
            openExeFolderButton.Margin = new Thickness(0, 5, 0, 10);
            openExeFolderButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(openExeFolderButton);

            // Botão Abrir Pasta das Ferramentas
            var openBaseDirFolderButton = GuiTheme.CreateStyledButton("📂 Ferramentas", (s, e) => OpenBaseDir(mainWindow));
            openBaseDirFolderButton.Width = 200;
            openBaseDirFolderButton.Height = 35;
            openBaseDirFolderButton.Margin = new Thickness(0, 5, 0, 10);
            openBaseDirFolderButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(openBaseDirFolderButton);
            
            return panel;
        }

        /// <summary>
        /// Adiciona as ferramentas do DevStack ao PATH do sistema
        /// </summary>
        private static void AddToPath(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
            {
                try
                {
                    DevStackConfig.pathManager?.AddBinDirsToPath();
                    mainWindow.StatusMessage = "PATH atualizado com sucesso";
                }
                catch (Exception ex)
                {
                    progress.Report($"❌ Erro ao adicionar ao PATH: {ex.Message}");
                    mainWindow.StatusMessage = "Erro ao atualizar PATH";
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Remove as ferramentas do DevStack do PATH do sistema
        /// </summary>
        private static void RemoveFromPath(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
            {
                try
                {
                    DevStackConfig.pathManager?.RemoveAllDevStackFromPath();
                    mainWindow.StatusMessage = "PATH limpo com sucesso";
                }
                catch (Exception ex)
                {
                    progress.Report($"❌ Erro ao remover do PATH: {ex.Message}");
                    mainWindow.StatusMessage = "Erro ao limpar PATH";
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Lista o PATH atual do sistema
        /// </summary>
        private static void ListCurrentPath(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
            {
                try
                {
                    DevStackConfig.pathManager?.ListCurrentPath();
                    mainWindow.StatusMessage = "PATH listado";
                }
                catch (Exception ex)
                {
                    progress.Report($"❌ Erro ao listar PATH: {ex.Message}");
                    mainWindow.StatusMessage = "Erro ao listar PATH";
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Abre a pasta onde está o executável DevStackGUI.exe
        /// </summary>
        private static void OpenExeFolder(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
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
                        mainWindow.StatusMessage = "Pasta do executável aberta";
                    }
                    else
                    {
                        progress.Report($"❌ Não foi possível localizar a pasta do executável.");
                        mainWindow.StatusMessage = "Erro ao abrir pasta do executável";
                    }
                }
                catch (Exception ex)
                {
                    progress.Report($"❌ Erro ao abrir pasta do executável: {ex.Message}");
                    mainWindow.StatusMessage = "Erro ao abrir pasta do executável";
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Abre a pasta base das ferramentas (DevStackConfig.baseDir)
        /// </summary>
        private static void OpenBaseDir(DevStackGui mainWindow)
        {
            _ = GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Config, async progress =>
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
                        mainWindow.StatusMessage = "Pasta de ferramentas aberta";
                    }
                    else
                    {
                        progress.Report($"❌ Não foi possível localizar a pasta de ferramentas.");
                        mainWindow.StatusMessage = "Erro ao abrir pasta de ferramentas";
                    }
                }
                catch (Exception ex)
                {
                    progress.Report($"❌ Erro ao abrir pasta de ferramentas: {ex.Message}");
                    mainWindow.StatusMessage = "Erro ao abrir pasta de ferramentas";
                }
                await Task.CompletedTask;
            });
        }
    }
}
