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
            var rightPanel = GuiConsolePanel.CreateConsoleOutputPanel(mainWindow);
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
            var pathGroup = CreatePathConfigGroup(mainWindow);
            panel.Children.Add(pathGroup);

            // Configurações de Proxy
            var proxyGroup = CreateProxyConfigGroup(mainWindow);
            panel.Children.Add(proxyGroup);

            scrollViewer.Content = panel;
            return scrollViewer;
        }

        /// <summary>
        /// Cria o grupo de configurações do PATH
        /// </summary>
        private static GroupBox CreatePathConfigGroup(DevStackGui mainWindow)
        {
            var group = new GroupBox
            {
                Header = "Gerenciamento do PATH",
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(10)
            };

            var panel = new StackPanel();
            
            var pathLabel = GuiTheme.CreateStyledLabel("Adicionar ferramentas ao PATH do sistema");
            pathLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(pathLabel);

            var addPathButton = GuiTheme.CreateStyledButton("➕ Adicionar ao PATH", (s, e) => AddToPath(mainWindow));
            addPathButton.Width = 200;
            addPathButton.Height = 35;
            addPathButton.Margin = new Thickness(0, 10, 0, 5);
            addPathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(addPathButton);

            var removePathButton = GuiTheme.CreateStyledButton("➖ Remover do PATH", (s, e) => RemoveFromPath(mainWindow));
            removePathButton.Width = 200;
            removePathButton.Height = 35;
            removePathButton.Margin = new Thickness(0, 5, 0, 5);
            removePathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(removePathButton);

            var listPathButton = GuiTheme.CreateStyledButton("📋 Listar PATH Atual", (s, e) => ListCurrentPath(mainWindow));
            listPathButton.Width = 200;
            listPathButton.Height = 35;
            listPathButton.Margin = new Thickness(0, 5, 0, 10);
            listPathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(listPathButton);

            group.Content = panel;
            return group;
        }

        /// <summary>
        /// Cria o grupo de configurações de proxy
        /// </summary>
        private static GroupBox CreateProxyConfigGroup(DevStackGui mainWindow)
        {
            var group = new GroupBox
            {
                Header = "Configurações de Proxy",
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(10)
            };

            var panel = new StackPanel();

            var proxyLabel = GuiTheme.CreateStyledLabel("URL do Proxy (opcional):");
            proxyLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(proxyLabel);

            var proxyTextBox = GuiTheme.CreateStyledTextBox();
            proxyTextBox.Height = 30;
            proxyTextBox.Margin = new Thickness(0, 5, 0, 10);
            proxyTextBox.Text = Environment.GetEnvironmentVariable("HTTP_PROXY") ?? "";
            panel.Children.Add(proxyTextBox);

            var setProxyButton = GuiTheme.CreateStyledButton("✅ Definir Proxy", (s, e) => SetProxy(mainWindow, proxyTextBox.Text));
            setProxyButton.Width = 150;
            setProxyButton.Height = 35;
            setProxyButton.Margin = new Thickness(0, 5, 5, 5);
            setProxyButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(setProxyButton);

            var removeProxyButton = GuiTheme.CreateStyledButton("❌ Remover Proxy", (s, e) => { proxyTextBox.Text = ""; SetProxy(mainWindow, ""); });
            removeProxyButton.Width = 150;
            removeProxyButton.Height = 35;
            removeProxyButton.Margin = new Thickness(0, 5, 0, 10);
            removeProxyButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(removeProxyButton);

            group.Content = panel;
            return group;
        }

        /// <summary>
        /// Adiciona as ferramentas do DevStack ao PATH do sistema
        /// </summary>
        private static void AddToPath(DevStackGui mainWindow)
        {
            try
            {
                DevStackConfig.pathManager?.AddBinDirsToPath();
                
                mainWindow.StatusMessage = "PATH atualizado com sucesso";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao adicionar ao PATH: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao atualizar PATH";
            }
        }

        /// <summary>
        /// Remove as ferramentas do DevStack do PATH do sistema
        /// </summary>
        private static void RemoveFromPath(DevStackGui mainWindow)
        {
            try
            {
                DevStackConfig.pathManager?.RemoveAllDevStackFromPath();
                
                mainWindow.StatusMessage = "PATH limpo com sucesso";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao remover do PATH: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao limpar PATH";
            }
        }

        /// <summary>
        /// Lista o PATH atual do sistema
        /// </summary>
        private static void ListCurrentPath(DevStackGui mainWindow)
        {
            try
            {
                DevStackConfig.pathManager?.ListCurrentPath();

                mainWindow.StatusMessage = "PATH listado";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao listar PATH: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao listar PATH";
            }
        }

        /// <summary>
        /// Define ou remove o proxy do sistema
        /// </summary>
        private static void SetProxy(DevStackGui mainWindow, string proxyUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(proxyUrl))
                {
                    Environment.SetEnvironmentVariable("HTTP_PROXY", null);
                    Environment.SetEnvironmentVariable("HTTPS_PROXY", null);
                }
                else
                {
                    Environment.SetEnvironmentVariable("HTTP_PROXY", proxyUrl);
                    Environment.SetEnvironmentVariable("HTTPS_PROXY", proxyUrl);
                }
                mainWindow.StatusMessage = "Configuração de proxy atualizada";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao configurar proxy: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao configurar proxy";
            }
        }
    }
}
