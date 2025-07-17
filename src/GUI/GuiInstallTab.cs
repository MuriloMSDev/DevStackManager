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

            // Painel direito - Console de saída
            var rightPanel = GuiConsolePanel.CreateConsoleOutputPanel(mainWindow);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de seleção de componentes para instalação
        /// </summary>
        private static StackPanel CreateInstallSelectionPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = GuiTheme.CreateStyledLabel("Instalar Nova Ferramenta", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Componente
            var componentLabel = GuiTheme.CreateStyledLabel("Selecione a ferramenta:");
            panel.Children.Add(componentLabel);

            var componentCombo = GuiTheme.CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, 5, 0, 15);
            componentCombo.Height = 30;
            var componentBinding = new Binding("AvailableComponents") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.ItemsSourceProperty, componentBinding);
            var selectedComponentBinding = new Binding("SelectedComponent") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedComponentBinding);
            panel.Children.Add(componentCombo);

            // Versão
            var versionLabel = GuiTheme.CreateStyledLabel("Selecione a versão (deixe vazio para a mais recente):");
            panel.Children.Add(versionLabel);

            var versionCombo = GuiTheme.CreateStyledComboBox();
            versionCombo.Margin = new Thickness(0, 5, 0, 20);
            versionCombo.Height = 30;
            var versionBinding = new Binding("AvailableVersions") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.ItemsSourceProperty, versionBinding);
            var selectedVersionBinding = new Binding("SelectedVersion") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedVersionBinding);
            panel.Children.Add(versionCombo);

            // Botão Instalar
            var installButton = GuiTheme.CreateStyledButton("📥 Instalar", async (s, e) => await InstallComponent(mainWindow));
            installButton.Height = 40;
            installButton.FontSize = 14;
            installButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(installButton);

            // Botão Listar Versões
            var listVersionsButton = GuiTheme.CreateStyledButton("📋 Listar Versões Disponíveis", (s, e) => ListVersionsForSelectedComponent(mainWindow));
            listVersionsButton.Height = 35;
            listVersionsButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(listVersionsButton);

            return panel;
        }

        /// <summary>
        /// Instala o componente selecionado
        /// </summary>
        public static async Task InstallComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedComponent))
            {
                MessageBox.Show("Selecione um componente para instalar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainWindow.IsLoading = true;
            mainWindow.StatusMessage = $"Instalando {mainWindow.SelectedComponent}...";
            
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
                    GuiConsolePanel.AppendToConsole(mainWindow, "⚠️ PathManager não foi inicializado - PATH não foi atualizado");
                }
                
                mainWindow.StatusMessage = $"{mainWindow.SelectedComponent} instalado com sucesso!";
                
                // Recarregar lista de instalados
                await GuiInstalledTab.LoadInstalledComponents(mainWindow);
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao instalar {mainWindow.SelectedComponent}: {ex.Message}");
                mainWindow.StatusMessage = $"Erro ao instalar {mainWindow.SelectedComponent}";
                DevStackConfig.WriteLog($"Erro ao instalar {mainWindow.SelectedComponent} na GUI: {ex}");
            }
            finally
            {
                mainWindow.IsLoading = false;
            }
        }

        /// <summary>
        /// Lista as versões disponíveis para o componente selecionado
        /// </summary>
        public static void ListVersionsForSelectedComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedComponent))
            {
                MessageBox.Show("Selecione um componente primeiro.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainWindow.StatusMessage = $"Listando versões de {mainWindow.SelectedComponent}...";
            
            try
            {
                ListManager.ListVersions(mainWindow.SelectedComponent);
                
                mainWindow.StatusMessage = $"Versões de {mainWindow.SelectedComponent} listadas";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao listar versões de {mainWindow.SelectedComponent}: {ex.Message}");
                mainWindow.StatusMessage = $"Erro ao listar versões de {mainWindow.SelectedComponent}";
            }
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
                    mainWindow.StatusMessage = $"Carregando versões de {mainWindow.SelectedComponent}...";
                    
                    var versionData = GetVersionDataForComponent(mainWindow.SelectedComponent);
                    
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.AvailableVersions.Clear();
                        foreach (var version in versionData.Versions
                            .OrderByDescending(v => 
                                Version.TryParse(v, out var parsed) ? parsed : new Version(0, 0)))
                        {
                            mainWindow.AvailableVersions.Add(version);
                        }
                        
                        mainWindow.StatusMessage = $"{mainWindow.AvailableVersions.Count} versões carregadas para {mainWindow.SelectedComponent}";
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = $"Erro ao carregar versões: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar versões na GUI: {ex}");
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
                        Message = $"{versions.Count} versões encontradas para {component}"
                    };
                }
                else
                {
                    return new VersionData { Status = "error", Message = $"Componente '{component}' não suportado" };
                }
            }
            catch (Exception ex)
            {
                return new VersionData { Status = "error", Message = $"Erro ao obter versões: {ex.Message}" };
            }
        }
    }
}
