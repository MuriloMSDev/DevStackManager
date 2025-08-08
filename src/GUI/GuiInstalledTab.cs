using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;

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
            var dataGridContainer = CreateInstalledDataGrid(mainWindow);
            Grid.SetRow(dataGridContainer, 1);
            grid.Children.Add(dataGridContainer);

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
        private static Grid CreateInstalledDataGrid(DevStackGui mainWindow)
        {
            // Criar um Grid para separar header fixo do conteúdo scrollável
            var containerGrid = new Grid
            {
                Margin = new Thickness(10)
            };
            
            containerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header fixo
            containerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Conteúdo scrollável

            // Criar DataGrid apenas para extrair o header
            var headerDataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                IsHitTestVisible = false // Não interativo, apenas visual
            };

            // Criar ScrollViewer customizado para o conteúdo
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                CanContentScroll = false, // Importante para scroll suave da roda do mouse
                PanningMode = PanningMode.VerticalOnly,
                IsManipulationEnabled = true
            };

            // Aplicar template moderno para ScrollViewer
            var scrollViewerStyle = new Style(typeof(ScrollViewer));
            
            var templateXaml = @"
                <ControlTemplate TargetType='ScrollViewer' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Grid>
                        <!-- Conteúdo principal ocupa toda a área -->
                        <ScrollContentPresenter Margin='{TemplateBinding Padding}'
                                                Content='{TemplateBinding Content}'
                                                ContentTemplate='{TemplateBinding ContentTemplate}'
                                                CanContentScroll='{TemplateBinding CanContentScroll}'/>
                        
                        <!-- ScrollBar Vertical - Sobreposta no canto direito -->
                        <ScrollBar Name='PART_VerticalScrollBar'
                                   HorizontalAlignment='Right'
                                   VerticalAlignment='Stretch'
                                   Orientation='Vertical'
                                   Value='{TemplateBinding VerticalOffset}'
                                   Maximum='{TemplateBinding ScrollableHeight}'
                                   ViewportSize='{TemplateBinding ViewportHeight}'
                                   Visibility='{TemplateBinding ComputedVerticalScrollBarVisibility}'
                                   Margin='0,2,2,2'>
                            <ScrollBar.Style>
                                <Style TargetType='ScrollBar'>
                                    <Setter Property='Background' Value='Transparent'/>
                                    <Setter Property='Width' Value='8'/>
                                    <Setter Property='Opacity' Value='0.7'/>
                                    <Setter Property='Template'>
                                        <Setter.Value>
                                            <ControlTemplate TargetType='ScrollBar'>
                                                <Grid>
                                                    <Track Name='PART_Track' IsDirectionReversed='True'>
                                                        <Track.Thumb>
                                                            <Thumb>
                                                                <Thumb.Template>
                                                                    <ControlTemplate TargetType='Thumb'>
                                                                        <Border Background='#FF555555' 
                                                                                CornerRadius='4' 
                                                                                Margin='1'/>
                                                                    </ControlTemplate>
                                                                </Thumb.Template>
                                                            </Thumb>
                                                        </Track.Thumb>
                                                    </Track>
                                                </Grid>
                                                
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter Property='Opacity' Value='1.0'/>
                                                        <Setter TargetName='PART_Track' Property='Thumb.Template'>
                                                            <Setter.Value>
                                                                <ControlTemplate TargetType='Thumb'>
                                                                    <Border Background='#FF777777' 
                                                                            CornerRadius='4' 
                                                                            Margin='1'/>
                                                                </ControlTemplate>
                                                            </Setter.Value>
                                                        </Setter>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </ScrollBar.Style>
                        </ScrollBar>
                        
                        <!-- ScrollBar Horizontal - Sobreposta na parte inferior -->
                        <ScrollBar Name='PART_HorizontalScrollBar'
                                   HorizontalAlignment='Stretch'
                                   VerticalAlignment='Bottom'
                                   Orientation='Horizontal'
                                   Value='{TemplateBinding HorizontalOffset}'
                                   Maximum='{TemplateBinding ScrollableWidth}'
                                   ViewportSize='{TemplateBinding ViewportWidth}'
                                   Visibility='{TemplateBinding ComputedHorizontalScrollBarVisibility}'
                                   Margin='2,0,2,2'>
                            <ScrollBar.Style>
                                <Style TargetType='ScrollBar'>
                                    <Setter Property='Background' Value='Transparent'/>
                                    <Setter Property='Height' Value='8'/>
                                    <Setter Property='Opacity' Value='0.7'/>
                                    <Setter Property='Template'>
                                        <Setter.Value>
                                            <ControlTemplate TargetType='ScrollBar'>
                                                <Grid>
                                                    <Track Name='PART_Track'>
                                                        <Track.Thumb>
                                                            <Thumb>
                                                                <Thumb.Template>
                                                                    <ControlTemplate TargetType='Thumb'>
                                                                        <Border Background='#FF555555' 
                                                                                CornerRadius='4' 
                                                                                Margin='1'/>
                                                                    </ControlTemplate>
                                                                </Thumb.Template>
                                                            </Thumb>
                                                        </Track.Thumb>
                                                    </Track>
                                                </Grid>
                                                
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter Property='Opacity' Value='1.0'/>
                                                        <Setter TargetName='PART_Track' Property='Thumb.Template'>
                                                            <Setter.Value>
                                                                <ControlTemplate TargetType='Thumb'>
                                                                    <Border Background='#FF777777' 
                                                                            CornerRadius='4' 
                                                                            Margin='1'/>
                                                                </ControlTemplate>
                                                            </Setter.Value>
                                                        </Setter>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </ScrollBar.Style>
                        </ScrollBar>
                    </Grid>
                </ControlTemplate>";

            try
            {
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);
                scrollViewerStyle.Setters.Add(new Setter(ScrollViewer.TemplateProperty, template));
                scrollViewer.Style = scrollViewerStyle;
            }
            catch
            {
                // Fallback - usar ScrollViewer padrão se XAML falhar
            }

            // Criar DataGrid para conteúdo SEM header
            var contentDataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                BorderThickness = new Thickness(0),
                HeadersVisibility = DataGridHeadersVisibility.None,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                ColumnHeaderHeight = 0, // Remove completely the header height
                CanUserResizeColumns = false,
                CanUserReorderColumns = false,
                CanUserSortColumns = false
            };

            // Configurar colunas para ambos DataGrids
            void ConfigureColumns(DataGrid dataGrid, bool isHeader = false)
            {
                var nameColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? "Ferramenta" : null,
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
                    Header = isHeader ? "Versões Instaladas" : null,
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
                    Header = isHeader ? "Status" : null,
                    Width = new DataGridLength(100)
                };
                
                // Centralizar o header da coluna Status apenas se for header
                if (isHeader)
                {
                    var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, GuiTheme.CurrentTheme.GridHeaderBackground));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, GuiTheme.CurrentTheme.GridHeaderForeground));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, 14.0));
                    statusColumn.HeaderStyle = headerStyle;
                }
                else
                {
                    // Hide headers if not header row - apply to all columns
                    var hiddenHeaderStyle = new Style(typeof(DataGridColumnHeader));
                    hiddenHeaderStyle.Setters.Add(new Setter(DataGridColumnHeader.VisibilityProperty, Visibility.Collapsed));
                    nameColumn.HeaderStyle = hiddenHeaderStyle;
                    versionsColumn.HeaderStyle = hiddenHeaderStyle;
                    statusColumn.HeaderStyle = hiddenHeaderStyle;
                }

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
            }

            // Configurar colunas
            ConfigureColumns(headerDataGrid, true);
            ConfigureColumns(contentDataGrid, false);

            var installedBinding = new Binding("InstalledComponents") { Source = mainWindow };
            contentDataGrid.SetBinding(DataGrid.ItemsSourceProperty, installedBinding);

            // Apply dark theme to both DataGrids
            GuiTheme.SetDataGridDarkTheme(headerDataGrid);
            GuiTheme.SetDataGridDarkTheme(contentDataGrid);

            // Colocar DataGrid do conteúdo dentro do ScrollViewer
            scrollViewer.Content = contentDataGrid;

            // Fix mouse wheel scrolling - redirect DataGrid mouse wheel events to ScrollViewer
            contentDataGrid.PreviewMouseWheel += (sender, e) =>
            {
                if (!e.Handled)
                {
                    e.Handled = true;
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender
                    };
                    scrollViewer.RaiseEvent(eventArg);
                }
            };

            // Adicionar ao Grid principal
            Grid.SetRow(headerDataGrid, 0); // Header fixo
            Grid.SetRow(scrollViewer, 1);   // Conteúdo scrollável
            
            containerGrid.Children.Add(headerDataGrid);
            containerGrid.Children.Add(scrollViewer);

            return containerGrid;
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
