using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Converter personalizado para converter bool em Visibility
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Componente responsável pela aba "Serviços" - gerencia e controla serviços do DevStack
    /// </summary>
    public static class GuiServicesTab
    {
        // Overlays de loading usados apenas nesta tab
        private static Border? ServicesLoadingOverlay;
        /// <summary>
        /// Cria o conteúdo completo da aba "Serviços"
        /// </summary>
        public static Grid CreateServicesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header
            var headerPanel = CreateServicesHeader(mainWindow);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            // DataGrid de Serviços
            var servicesGrid = CreateServicesDataGrid(mainWindow);
            Grid.SetRow(servicesGrid, 2);
            grid.Children.Add(servicesGrid);

            // Botões de controle
            var controlPanel = CreateServicesControlPanel(mainWindow);
            Grid.SetRow(controlPanel, 1);
            grid.Children.Add(controlPanel);


            // Overlay de loading único cobrindo toda a área de serviços
            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
            // Overlay sempre visível se carregando serviços
            overlay.Visibility = mainWindow.IsLoadingServices ? Visibility.Visible : Visibility.Collapsed;
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsLoadingServices))
                {
                    overlay.Visibility = mainWindow.IsLoadingServices ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            Grid.SetRowSpan(overlay, 3);
            grid.Children.Add(overlay);
            ServicesLoadingOverlay = overlay;

            return grid;
        }

        /// <summary>
        /// Cria o cabeçalho da aba de serviços
        /// </summary>
        private static StackPanel CreateServicesHeader(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10)
            };

            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.services_tab.title"), true);
            titleLabel.FontSize = 18;
            panel.Children.Add(titleLabel);

            return panel;
        }

        /// <summary>
        /// Cria o DataGrid para exibir os serviços
        /// </summary>
        private static Grid CreateServicesDataGrid(DevStackGui mainWindow)
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
                CanUserSortColumns = false,
                Name = "ServicesDataGrid"
            };

            // Configurar colunas para ambos DataGrids
            void ConfigureColumns(DataGrid dataGrid, bool isHeader = false)
            {
                // Coluna Componente usando DataTemplate
                var componentColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.component") : null,
                    Width = new DataGridLength(120)
                };

                var componentTemplate = new DataTemplate();
                var componentTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                componentTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
                componentTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
                componentTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                componentTemplate.VisualTree = componentTextBlockFactory;
                componentColumn.CellTemplate = componentTemplate;

                dataGrid.Columns.Add(componentColumn);

                // Coluna Versão usando DataTemplate
                var versionColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.version") : null,
                    Width = new DataGridLength(100)
                };

                var versionTemplate = new DataTemplate();
                var versionTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                versionTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Version"));
                versionTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
                versionTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                versionTemplate.VisualTree = versionTextBlockFactory;
                versionColumn.CellTemplate = versionTemplate;

                dataGrid.Columns.Add(versionColumn);

                // Coluna Status com colorização usando DataTemplate
                var statusColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.status") : null,
                    Width = new DataGridLength(120)
                };

                var statusTemplate = new DataTemplate();
                var statusTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                statusTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Status"));
                statusTextBlockFactory.SetBinding(TextBlock.ForegroundProperty, new Binding("StatusColor"));
                statusTextBlockFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                statusTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
                statusTextBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                statusTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                statusTextBlockFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                statusTemplate.VisualTree = statusTextBlockFactory;
                statusColumn.CellTemplate = statusTemplate;

                dataGrid.Columns.Add(statusColumn);

                // Coluna PID usando DataTemplate
                var pidColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.pid") : null,
                    Width = new DataGridLength(330)
                };

                var pidTemplate = new DataTemplate();
                var pidTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                pidTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Pid"));
                pidTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
                pidTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                pidTemplate.VisualTree = pidTextBlockFactory;
                pidColumn.CellTemplate = pidTemplate;

                dataGrid.Columns.Add(pidColumn);

                // Coluna Copiar PID (botão) - Aparece apenas quando executando
                var copyButtonTemplate = new DataTemplate();
                
                // Criar botão usando DevStackShared.ThemeManager.CreateStyledButton
                var copyButtonStyleTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.copy_pid"), null, DevStackShared.ThemeManager.ButtonStyle.Secondary);
                
                var copyButtonFactory = new FrameworkElementFactory(typeof(Button));
                copyButtonFactory.SetValue(Button.ContentProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.copy_pid"));
                copyButtonFactory.SetValue(Button.HeightProperty, 25.0);
                copyButtonFactory.SetValue(Button.WidthProperty, 35.0);
                copyButtonFactory.SetValue(Button.FontSizeProperty, 12.0);
                copyButtonFactory.SetValue(Button.ToolTipProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.tooltips.copy_pid"));
                copyButtonFactory.SetValue(Button.StyleProperty, copyButtonStyleTemplate.Style);
                
                // Usar binding com converter personalizado para visibilidade
                var visibilityBinding = new Binding("IsRunning");
                visibilityBinding.Converter = new BooleanToVisibilityConverter();
                copyButtonFactory.SetBinding(Button.VisibilityProperty, visibilityBinding);
                
                copyButtonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => CopyPidButton_Click(sender, e, mainWindow)));
                copyButtonTemplate.VisualTree = copyButtonFactory;

                dataGrid.Columns.Add(new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.copy_pid") : null,
                    CellTemplate = copyButtonTemplate,
                    Width = new DataGridLength(100)
                });

                // Coluna Ações (Start, Stop, Restart)
                var actionsTemplate = new DataTemplate();
                var actionsPanel = new FrameworkElementFactory(typeof(StackPanel));
                actionsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                actionsPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

                // Criar botões usando DevStackShared.ThemeManager.CreateStyledButton diretamente
                var startButtonTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.start"), null, DevStackShared.ThemeManager.ButtonStyle.Success);
                var stopButtonTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.stop"), null, DevStackShared.ThemeManager.ButtonStyle.Danger);
                var restartButtonTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.restart"), null, DevStackShared.ThemeManager.ButtonStyle.Warning);

                // Botão Start
                var startButton = new FrameworkElementFactory(typeof(Button));
                startButton.SetValue(Button.ContentProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.start"));
                startButton.SetValue(Button.WidthProperty, 30.0);
                startButton.SetValue(Button.HeightProperty, 25.0);
                startButton.SetValue(Button.MarginProperty, new Thickness(2));
                startButton.SetValue(Button.ToolTipProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.tooltips.start"));
                startButton.SetValue(Button.StyleProperty, startButtonTemplate.Style);
                startButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => StartServiceButton_Click(sender, e, mainWindow)));
                actionsPanel.AppendChild(startButton);

                // Botão Stop
                var stopButton = new FrameworkElementFactory(typeof(Button));
                stopButton.SetValue(Button.ContentProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.stop"));
                stopButton.SetValue(Button.WidthProperty, 30.0);
                stopButton.SetValue(Button.HeightProperty, 25.0);
                stopButton.SetValue(Button.MarginProperty, new Thickness(2));
                stopButton.SetValue(Button.ToolTipProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.tooltips.stop"));
                stopButton.SetValue(Button.StyleProperty, stopButtonTemplate.Style);
                stopButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => StopServiceButton_Click(sender, e, mainWindow)));
                actionsPanel.AppendChild(stopButton);

                // Botão Restart
                var restartButton = new FrameworkElementFactory(typeof(Button));
                restartButton.SetValue(Button.ContentProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.restart"));
                restartButton.SetValue(Button.WidthProperty, 30.0);
                restartButton.SetValue(Button.HeightProperty, 25.0);
                restartButton.SetValue(Button.MarginProperty, new Thickness(2));
                restartButton.SetValue(Button.ToolTipProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.tooltips.restart"));
                restartButton.SetValue(Button.StyleProperty, restartButtonTemplate.Style);
                restartButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => RestartServiceButton_Click(sender, e, mainWindow)));
                actionsPanel.AppendChild(restartButton);

                actionsTemplate.VisualTree = actionsPanel;

                // Coluna Ações usando DataTemplate
                var actionsColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.actions") : null,
                    Width = new DataGridLength(120)
                };
                
                actionsColumn.CellTemplate = actionsTemplate;
                dataGrid.Columns.Add(actionsColumn);

                // Aplicar estilos de header apenas se for header
                if (isHeader)
                {
                    // Style for left-aligned columns (Componente, Versão, PID)
                    var leftHeaderStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.VerticalContentAlignmentProperty, VerticalAlignment.Center));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderBackground));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderForeground));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, 14.0));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderThicknessProperty, new Thickness(0)));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderBrushProperty, Brushes.Transparent));
                    leftHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.PaddingProperty, new Thickness(12, 5, 5, 5)));

                    // Style for center-aligned columns (Status, Copiar PID, Ações)
                    var centerHeaderStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.VerticalContentAlignmentProperty, VerticalAlignment.Center));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderBackground));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderForeground));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, 14.0));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderThicknessProperty, new Thickness(0)));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderBrushProperty, Brushes.Transparent));
                    centerHeaderStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.PaddingProperty, new Thickness(5)));
                    
                    // Apply appropriate header styles to each column
                    componentColumn.HeaderStyle = leftHeaderStyle;       // Left-aligned
                    versionColumn.HeaderStyle = leftHeaderStyle;         // Left-aligned
                    statusColumn.HeaderStyle = centerHeaderStyle;        // Center-aligned
                    pidColumn.HeaderStyle = leftHeaderStyle;             // Left-aligned
                    dataGrid.Columns[4].HeaderStyle = centerHeaderStyle; // Copiar PID column - Center-aligned
                    actionsColumn.HeaderStyle = centerHeaderStyle;       // Center-aligned
                }
                else
                {
                    // Hide headers if not header row - apply to all columns
                    var hiddenHeaderStyle = new Style(typeof(DataGridColumnHeader));
                    hiddenHeaderStyle.Setters.Add(new Setter(DataGridColumnHeader.VisibilityProperty, Visibility.Collapsed));
                    componentColumn.HeaderStyle = hiddenHeaderStyle;
                    versionColumn.HeaderStyle = hiddenHeaderStyle;
                    statusColumn.HeaderStyle = hiddenHeaderStyle;
                    pidColumn.HeaderStyle = hiddenHeaderStyle;
                    dataGrid.Columns[4].HeaderStyle = hiddenHeaderStyle; // Copiar PID column
                    actionsColumn.HeaderStyle = hiddenHeaderStyle;       // Ações column
                }
            }

            // Configurar colunas
            ConfigureColumns(headerDataGrid, true);
            ConfigureColumns(contentDataGrid, false);

            var servicesBinding = new Binding("Services") { Source = mainWindow };
            contentDataGrid.SetBinding(DataGrid.ItemsSourceProperty, servicesBinding);

            // Apply dark theme to both DataGrids
            DevStackShared.ThemeManager.SetDataGridDarkTheme(headerDataGrid);
            DevStackShared.ThemeManager.SetDataGridDarkTheme(contentDataGrid);

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
        /// Manipula o clique no botão de copiar PID
        /// </summary>
        private static void CopyPidButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    if (service.Pid != "-" && !string.IsNullOrEmpty(service.Pid))
                    {
                        Clipboard.SetText(service.Pid);
                        
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.pid_copied", service.Pid);
                    }
                    else
                    {
                        DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.services_tab.messages.no_pid"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_copy_pid", ex.Message), mainWindow);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_copy_pid", ex.Message);
            }
        }

        /// <summary>
        /// Manipula o clique no botão de iniciar serviço individual
        /// </summary>
        private static async void StartServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            mainWindow.IsLoadingServices = true;
            if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.starting", service.Name, service.Version);
                    await Task.Run(() =>
                    {
                        ProcessManager.StartComponent(service.Name, service.Version);
                    });
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.started", service.Name);
                    await LoadServices(mainWindow); // Recarregar lista
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_start", ex.Message), mainWindow);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_start", ex.Message);
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_start", ex));
            }
            finally
            {
                mainWindow.IsLoadingServices = false;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Manipula o clique no botão de parar serviço individual
        /// </summary>
        private static async void StopServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            mainWindow.IsLoadingServices = true;
            if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.stopping", service.Name, service.Version);
                    await Task.Run(() =>
                    {
                        ProcessManager.StopComponent(service.Name, service.Version);
                    });
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.stopped", service.Name);
                    await LoadServices(mainWindow); // Recarregar lista
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_stop", ex.Message), mainWindow);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_stop", ex.Message);
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_stop", ex));
            }
            finally
            {
                mainWindow.IsLoadingServices = false;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Manipula o clique no botão de reiniciar serviço individual
        /// </summary>
        private static async void RestartServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            mainWindow.IsLoadingServices = true;
            if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.restarting", service.Name, service.Version);
                    await Task.Run(() =>
                    {
                        ProcessManager.RestartComponent(service.Name, service.Version);
                    });
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.restarted", service.Name);
                    await LoadServices(mainWindow); // Recarregar lista
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_restart", ex.Message), mainWindow);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_restart", ex.Message);
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_restart", ex));
            }
            finally
            {
                mainWindow.IsLoadingServices = false;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Cria o painel de controle dos serviços
        /// </summary>
        private static StackPanel CreateServicesControlPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Botões para todos os serviços
            var startAllButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.start_all"), async (s, e) =>
            {
                mainWindow.IsLoadingServices = true;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
                try { await StartAllServices(mainWindow); }
                finally
                {
                    mainWindow.IsLoadingServices = false;
                    if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Success);
            startAllButton.Width = 150;
            startAllButton.Height = 40;
            startAllButton.Margin = new Thickness(10);
            panel.Children.Add(startAllButton);

            var stopAllButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.stop_all"), async (s, e) =>
            {
                mainWindow.IsLoadingServices = true;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
                try { await StopAllServices(mainWindow); }
                finally
                {
                    mainWindow.IsLoadingServices = false;
                    if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Danger);
            stopAllButton.Width = 150;
            stopAllButton.Height = 40;
            stopAllButton.Margin = new Thickness(10);
            panel.Children.Add(stopAllButton);

            var restartAllButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.restart_all"), async (s, e) =>
            {
                mainWindow.IsLoadingServices = true;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
                try { await RestartAllServices(mainWindow); }
                finally
                {
                    mainWindow.IsLoadingServices = false;
                    if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Warning);
            restartAllButton.Width = 150;
            restartAllButton.Height = 40;
            restartAllButton.Margin = new Thickness(10);
            panel.Children.Add(restartAllButton);

            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.refresh"), async (s, e) => await LoadServices(mainWindow));
            refreshButton.Width = 150;
            refreshButton.Height = 40;
            refreshButton.Margin = new Thickness(10);
            panel.Children.Add(refreshButton);

            return panel;
        }

        /// <summary>
        /// Carrega a lista de serviços
        /// </summary>
        public static async Task LoadServices(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.loading");
                    
                    var services = new ObservableCollection<ServiceViewModel>();
                    
                    // Usar o diretório base do DevStackConfig
                    var devStackPath = DevStackConfig.baseDir;
                    
                    // Verificar se as configurações foram inicializadas
                    if (string.IsNullOrEmpty(devStackPath))
                    {
                        DevStackConfig.Initialize();
                        devStackPath = DevStackConfig.baseDir;
                    }
                    
                    // Obter todos os componentes que são serviços
                    var serviceComponents = Components.ComponentsFactory.GetAll()
                        .Where(c => c.IsService)
                        .ToList();
                    
                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.found_service_components", serviceComponents.Count));
                    
                    // Debug: Listar todos os processos relevantes
                    var allProcesses = Process.GetProcesses();
                    var debugProcesses = allProcesses
                        .Where(p => serviceComponents.Any(sc => 
                            p.ProcessName.StartsWith(sc.Name, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                    
                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.processes_found", debugProcesses.Count));
                    foreach (var proc in debugProcesses)
                    {
                        try
                        {
                            var path = proc.MainModule?.FileName ?? "N/A";
                            DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.process_info", proc.ProcessName, proc.Id, path));
                        }
                        catch (Exception ex)
                        {
                            DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.process_error", proc.ProcessName, proc.Id, ex.Message));
                        }
                    }
                    
                    // Detectar serviços para cada componente de serviço
                    foreach (var component in serviceComponents)
                    {
                        if (!Directory.Exists(component.ToolDir))
                        {
                            DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.component_dir_not_found", component.Name, component.ToolDir));
                            continue;
                        }
                        
                        var installedVersions = component.ListInstalled();
                        DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.component_versions_found", component.Name, installedVersions.Count, string.Join(", ", installedVersions)));
                        
                        foreach (var version in installedVersions)
                        {
                            DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.checking_component_version", component.Name, version));
                            
                            try
                            {
                                bool isRunning = false;
                                string pids = "-";
                                
                                if (!string.IsNullOrEmpty(component.ServicePattern))
                                {
                                    var serviceExePath = Path.Combine(component.ToolDir, $"{component.Name}-{version}", component.ServicePattern);
                                    
                                    // Buscar processos do serviço
                                    var serviceProcesses = allProcesses
                                        .Where(p => {
                                            try
                                            {
                                                var processPath = p.MainModule?.FileName;
                                                var matches = !string.IsNullOrEmpty(processPath) && 
                                                            processPath.Equals(serviceExePath, StringComparison.OrdinalIgnoreCase);
                                                if (matches)
                                                {
                                                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.service_process_found", component.Name, p.ProcessName, p.Id, processPath ?? "N/A"));
                                                }
                                                return matches;
                                            }
                                            catch (Exception ex)
                                            {
                                                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.process_check_error", p.ProcessName, ex.Message));
                                                return false;
                                            }
                                        })
                                        .ToList();
                                    
                                    if (serviceProcesses.Any())
                                    {
                                        isRunning = true;
                                        pids = string.Join(", ", serviceProcesses.Select(p => p.Id));
                                        DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.service_running", component.Name, version, pids));
                                    }
                                    else
                                    {
                                        DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.service_not_running", component.Name, version));
                                    }
                                }
                                else
                                {
                                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.no_service_pattern", component.Name));
                                }
                                
                                services.Add(new ServiceViewModel 
                                { 
                                    Name = component.Name, 
                                    Version = version,
                                    Status = isRunning ? mainWindow.LocalizationManager.GetString("gui.services_tab.status.running") : mainWindow.LocalizationManager.GetString("gui.services_tab.status.stopped"), 
                                    Type = component.GetServiceType(mainWindow.LocalizationManager), 
                                    Description = component.GetServiceDescription(version, mainWindow.LocalizationManager),
                                    Pid = pids,
                                    IsRunning = isRunning
                                });
                            }
                            catch (Exception ex)
                            {
                                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.component_check_error", component.Name, ex.Message));
                            }
                        }
                    }
                    
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.Services.Clear();
                        foreach (var service in services)
                        {
                            mainWindow.Services.Add(service);
                        }
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.loaded", new object[] { mainWindow.Services.Count });
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error", ex.Message ?? string.Empty);
                        DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.load_services_error", ex));
                    });
                }
            });
        }

        /// <summary>
        /// Inicia todos os serviços
        /// </summary>
        private static async Task StartAllServices(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.starting_all");
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StartAllComponents();
                });

                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.started_all");
                await LoadServices(mainWindow); // Recarregar lista
            }
            catch (Exception ex)
            {
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_start_all", ex.Message), mainWindow);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_start_all", ex.Message);
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.start_all_services_error", ex));
            }
        }

        /// <summary>
        /// Para todos os serviços
        /// </summary>
        private static async Task StopAllServices(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.stopping_all");
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StopAllComponents();
                });

                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.stopped_all");
                await LoadServices(mainWindow); // Recarregar lista
            }
            catch (Exception ex)
            {
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_stop_all", ex.Message), mainWindow);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_stop_all", ex.Message);
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.stop_all_services_error", ex));
            }
        }

        /// <summary>
        /// Reinicia todos os serviços
        /// </summary>
        private static async Task RestartAllServices(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.restarting_all");
            
            try
            {
                await Task.Run(() =>
                {
                    // Parar todos e depois iniciar todos
                    ProcessManager.StopAllComponents();
                    System.Threading.Thread.Sleep(2000); // Aguardar 2 segundos
                    ProcessManager.StartAllComponents();
                });

                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.restarted_all");
                await LoadServices(mainWindow); // Recarregar lista
            }
            catch (Exception ex)
            {
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_restart_all", ex.Message), mainWindow);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_restart_all", ex.Message);
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.services_tab.debug.restart_all_services_error", ex));
            }
        }
    }
}
