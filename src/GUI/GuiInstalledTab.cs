using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela aba "Instalados" - lista e exibe ferramentas instaladas
    /// </summary>
    public static class GuiInstalledTab
    {
        /// <summary>
        /// Cria o conteúdo completo da aba "Instalados"
        /// </summary>
        public static Grid CreateInstalledContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerPanel = CreateInstalledHeader(mainWindow);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            // DataGrid
            var dataGrid = CreateInstalledDataGrid(mainWindow);
            Grid.SetRow(dataGrid, 1);
            grid.Children.Add(dataGrid);

            // Info panel
            var infoPanel = CreateInstalledInfoPanel();
            Grid.SetRow(infoPanel, 2);
            grid.Children.Add(infoPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de cabeçalho da aba instalados
        /// </summary>
        private static StackPanel CreateInstalledHeader(DevStackGui mainWindow)
        {
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10)
            };
            
            var titleLabel = GuiTheme.CreateStyledLabel("Ferramentas Instaladas", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            headerPanel.Children.Add(titleLabel);

            var refreshButton = GuiTheme.CreateStyledButton("🔄 Atualizar Lista", async (s, e) => await LoadInstalledComponents(mainWindow));
            refreshButton.Width = 130;
            refreshButton.Height = 35;
            refreshButton.Margin = new Thickness(20, 0, 0, 20);
            headerPanel.Children.Add(refreshButton);

            return headerPanel;
        }

        /// <summary>
        /// Cria o DataGrid para exibir as ferramentas instaladas
        /// </summary>
        private static DataGrid CreateInstalledDataGrid(DevStackGui mainWindow)
        {
            var dataGrid = new DataGrid
            {
                Margin = new Thickness(10),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single
            };

            // Colunas
            var nameColumn = new DataGridTemplateColumn
            {
                Header = "Ferramenta",
                Width = new DataGridLength(200)
            };

            var nameTemplate = new DataTemplate();
            var nameTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            nameTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            nameTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
            nameTemplate.VisualTree = nameTextBlockFactory;
            nameColumn.CellTemplate = nameTemplate;

            dataGrid.Columns.Add(nameColumn);

            var versionsColumn = new DataGridTemplateColumn
            {
                Header = "Versões Instaladas",
                Width = new DataGridLength(400)
            };

            var versionsTemplate = new DataTemplate();
            var versionsTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            versionsTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("VersionsText"));
            versionsTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(13, 0, 0, 0));
            versionsTemplate.VisualTree = versionsTextBlockFactory;
            versionsColumn.CellTemplate = versionsTemplate;

            dataGrid.Columns.Add(versionsColumn);

            var statusColumn = new DataGridTemplateColumn
            {
                Header = "Status",
                Width = new DataGridLength(100)
            };
            
            // Centralizar o header da coluna Status
            var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, GuiTheme.CurrentTheme.GridHeaderBackground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, GuiTheme.CurrentTheme.GridHeaderForeground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, 14.0));
            statusColumn.HeaderStyle = headerStyle;
            
            var statusTemplate = new DataTemplate();
            var statusTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            statusTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Status"));
            statusTextBlockFactory.SetBinding(TextBlock.ForegroundProperty, new Binding("StatusColor"));
            statusTextBlockFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            statusTextBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            statusTextBlockFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            statusTemplate.VisualTree = statusTextBlockFactory;
            statusColumn.CellTemplate = statusTemplate;
            
            dataGrid.Columns.Add(statusColumn);

            var installedBinding = new Binding("InstalledComponents") { Source = mainWindow };
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty, installedBinding);

            // Apply dark theme to DataGrid
            GuiTheme.SetDataGridDarkTheme(dataGrid);

            return dataGrid;
        }

        /// <summary>
        /// Cria o painel de informações no rodapé da aba
        /// </summary>
        private static StackPanel CreateInstalledInfoPanel()
        {
            var infoPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var infoLabel = GuiTheme.CreateStyledLabel("ℹ️ Use as abas 'Instalar' e 'Desinstalar' para gerenciar as ferramentas", false, true);
            infoLabel.FontStyle = FontStyles.Italic;
            infoPanel.Children.Add(infoLabel);

            return infoPanel;
        }

        /// <summary>
        /// Carrega a lista de componentes instalados de forma assíncrona
        /// </summary>
        public static async Task LoadInstalledComponents(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    mainWindow.StatusMessage = "Carregando componentes instalados...";
                    
                    var data = DataManager.GetInstalledVersions();
                    var components = new ObservableCollection<ComponentViewModel>();
                    
                    foreach (var comp in data.Components)
                    {
                        components.Add(new ComponentViewModel
                        {
                            Name = comp.Name,
                            Installed = comp.Installed,
                            Versions = comp.Versions,
                            Status = comp.Installed ? "✔️" : "❌",
                            VersionsText = comp.Installed ? string.Join(", ", comp.Versions) : "N/A"
                        });
                    }
                    
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.InstalledComponents = components;
                        mainWindow.StatusMessage = $"Carregados {components.Count} componentes";
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = $"Erro ao carregar componentes: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar componentes na GUI: {ex}");
                    });
                }
            });
        }
    }
}
