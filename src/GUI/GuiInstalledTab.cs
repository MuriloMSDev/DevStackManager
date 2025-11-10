using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        // UI Dimensions Constants
        private const int HEADER_FONT_SIZE = 18;
        private const int HEADER_FONT_SIZE_COLUMN = 14;
        private const int TITLE_BOTTOM_MARGIN = 20;
        private const int REFRESH_BUTTON_WIDTH = 150;
        private const int REFRESH_BUTTON_HEIGHT = 35;
        private const int REFRESH_BUTTON_LEFT_MARGIN = 20;
        private const int VERSION_BUTTON_WIDTH = 80;
        private const int VERSION_BUTTON_HEIGHT = 25;
        private const int VERSION_BUTTON_MARGIN = 2;
        private const int VERSION_ICON_FONT_SIZE = 16;
        private const int VERSION_ICON_LEFT_MARGIN = 4;
        private const int COLUMN_NAME_WIDTH = 200;
        private const int COLUMN_VERSIONS_WIDTH = 400;
        private const int COLUMN_STATUS_WIDTH = 100;
        private const int PANEL_MARGIN = 10;
        private const int TEXT_PADDING_LEFT = 12;
        private const int TEXT_PADDING_LEFT_VERSIONS = 13;
        private const int VISUAL_TREE_DEPTH_LIMIT = 10;
        
        // Color Constants for Version Icon Gradient
        private const byte GRADIENT_GOLD_R = 255;
        private const byte GRADIENT_GOLD_G = 215;
        private const byte GRADIENT_GOLD_B = 0;
        private const byte GRADIENT_ORANGE_R = 255;
        private const byte GRADIENT_ORANGE_G = 140;
        private const byte GRADIENT_ORANGE_B = 0;
        private const byte GRADIENT_CRIMSON_R = 220;
        private const byte GRADIENT_CRIMSON_G = 20;
        private const byte GRADIENT_CRIMSON_B = 60;
        
        // Gradient Stop Positions
        private const double GRADIENT_STOP_START = 0.0;
        private const double GRADIENT_STOP_MIDDLE = 0.5;
        private const double GRADIENT_STOP_END = 1.0;
        
        // Icon Constants
        private const string LIGHTNING_ICON = " 🗲";

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
            var infoPanel = CreateInstalledInfoPanel(mainWindow);
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
                Margin = new Thickness(PANEL_MARGIN)
            };
            
            var titleLabel = CreateTitleLabel(mainWindow);
            headerPanel.Children.Add(titleLabel);

            var refreshButton = CreateRefreshButton(mainWindow);
            headerPanel.Children.Add(refreshButton);

            return headerPanel;
        }

        private static Label CreateTitleLabel(DevStackGui mainWindow)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.installed_tab.title"), 
                true);
            titleLabel.FontSize = HEADER_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_BOTTOM_MARGIN);
            return titleLabel;
        }

        private static Button CreateRefreshButton(DevStackGui mainWindow)
        {
            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString("gui.installed_tab.buttons.refresh"), 
                async (s, e) => await mainWindow.LoadInstalledComponents());
            refreshButton.Width = REFRESH_BUTTON_WIDTH;
            refreshButton.Height = REFRESH_BUTTON_HEIGHT;
            refreshButton.Margin = new Thickness(REFRESH_BUTTON_LEFT_MARGIN, 0, 0, TITLE_BOTTOM_MARGIN);
            return refreshButton;
        }

        /// <summary>
        /// Cria o DataGrid para exibir as ferramentas instaladas
        /// </summary>
        private static Grid CreateInstalledDataGrid(DevStackGui mainWindow)
        {
            // Criar um Grid para separar header fixo do conteúdo scrollável
            var containerGrid = new Grid
            {
                Margin = new Thickness(PANEL_MARGIN)
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

            // Aplicar scrollbar customizada do ThemeManager
            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

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
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.installed_tab.headers.tool") : null,
                    Width = new DataGridLength(COLUMN_NAME_WIDTH)
                };

                var nameTemplate = new DataTemplate();
                var nameTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                nameTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Label"));
                nameTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(TEXT_PADDING_LEFT, 0, 0, 0));
                nameTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                nameTextBlockFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                nameTemplate.VisualTree = nameTextBlockFactory;
                nameColumn.CellTemplate = nameTemplate;

                dataGrid.Columns.Add(nameColumn);

                // Coluna Versões Instaladas
                var versionsColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.installed_tab.headers.versions") : null,
                    Width = new DataGridLength(COLUMN_VERSIONS_WIDTH)
                };
                var versionsTemplate = new DataTemplate();
                // StackPanel para botões ou texto
                var versionsPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
                versionsPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                versionsPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                // MultiBinding para visibilidade dos botões
                var multiBinding = new System.Windows.Data.MultiBinding { Converter = new InstalledAndExecutableToVisibilityConverter() };
                multiBinding.Bindings.Add(new Binding("Installed"));
                multiBinding.Bindings.Add(new Binding("IsExecutable"));
                // Botões para cada versão
                var versionsBinding = new Binding("Versions");
                versionsPanelFactory.SetBinding(FrameworkElement.TagProperty, versionsBinding);
                // ItemsControl para botões
                var itemTemplate = new DataTemplate();
                var buttonFactory = new FrameworkElementFactory(typeof(Button));
                buttonFactory.SetValue(Button.WidthProperty, (double)VERSION_BUTTON_WIDTH);
                buttonFactory.SetValue(Button.HeightProperty, (double)VERSION_BUTTON_HEIGHT);
                buttonFactory.SetValue(Button.MarginProperty, new Thickness(VERSION_BUTTON_MARGIN));
                buttonFactory.SetValue(Button.StyleProperty, DevStackShared.ThemeManager.CreateStyledButton("", null, DevStackShared.ThemeManager.ButtonStyle.Secondary).Style);
                // StackPanel para versão + ícone
                var btnStackPanel = new FrameworkElementFactory(typeof(StackPanel));
                btnStackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                btnStackPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                // Texto da versão
                var btnTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                btnTextBlock.SetBinding(TextBlock.TextProperty, new Binding("."));
                btnTextBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                btnStackPanel.AppendChild(btnTextBlock);
                // Ícone 🗲 com gradiente
                var btnIconBlock = CreateVersionIconWithGradient();
                btnStackPanel.AppendChild(btnIconBlock);
                buttonFactory.AppendChild(btnStackPanel);
                buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => ExecuteComponentVersionButton_Click(sender, e, mainWindow)));
                itemTemplate.VisualTree = buttonFactory;
                // ItemsControl para botões horizontalmente
                var itemsControlFactory = new FrameworkElementFactory(typeof(ItemsControl));
                itemsControlFactory.SetBinding(ItemsControl.ItemsSourceProperty, versionsBinding);
                itemsControlFactory.SetValue(ItemsControl.ItemTemplateProperty, itemTemplate);
                // WrapPanel para layout horizontal dos botões
                var itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(WrapPanel)));
                itemsControlFactory.SetValue(ItemsControl.ItemsPanelProperty, itemsPanelTemplate);
                // Visibilidade dos botões
                itemsControlFactory.SetBinding(ItemsControl.VisibilityProperty, multiBinding);
                versionsPanelFactory.AppendChild(itemsControlFactory);
                // TextBlock para texto das versões
                var versionsTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                versionsTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("VersionsText"));
                versionsTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(TEXT_PADDING_LEFT_VERSIONS, 0, 0, 0));
                versionsTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                // Visibilidade do texto: inverso do MultiBinding
                var inverseMultiBinding = new System.Windows.Data.MultiBinding { Converter = new InstalledAndExecutableToCollapsedConverter() };
                inverseMultiBinding.Bindings.Add(new Binding("Installed"));
                inverseMultiBinding.Bindings.Add(new Binding("IsExecutable"));
                versionsTextBlockFactory.SetBinding(TextBlock.VisibilityProperty, inverseMultiBinding);
                versionsPanelFactory.AppendChild(versionsTextBlockFactory);
                versionsTemplate.VisualTree = versionsPanelFactory;
                versionsColumn.CellTemplate = versionsTemplate;
                dataGrid.Columns.Add(versionsColumn);

                var statusColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.installed_tab.headers.status") : null,
                    Width = new DataGridLength(COLUMN_STATUS_WIDTH)
                };
                
                // Centralizar o header da coluna Status apenas se for header
                if (isHeader)
                {
                    var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderBackground));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderForeground));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, (double)HEADER_FONT_SIZE_COLUMN));
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
                statusTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
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
        /// Cria o painel de informações no rodapé da aba
        /// </summary>
        private static StackPanel CreateInstalledInfoPanel(DevStackGui mainWindow)
        {
            var infoPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(PANEL_MARGIN),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var infoNotification = DevStackShared.ThemeManager.CreateNotificationPanel(
                mainWindow.LocalizationManager.GetString("gui.installed_tab.info"),
                DevStackShared.ThemeManager.NotificationType.Info
            );
            infoPanel.Children.Add(infoNotification);

            return infoPanel;
        }

        /// <summary>
        /// Cria o ícone de raio com gradiente para os botões de versão
        /// </summary>
        private static FrameworkElementFactory CreateVersionIconWithGradient()
        {
            var btnIconBlock = new FrameworkElementFactory(typeof(TextBlock));
            btnIconBlock.SetValue(TextBlock.TextProperty, LIGHTNING_ICON);
            btnIconBlock.SetValue(TextBlock.FontSizeProperty, (double)VERSION_ICON_FONT_SIZE);
            btnIconBlock.SetValue(TextBlock.MarginProperty, new Thickness(VERSION_ICON_LEFT_MARGIN, 0, 0, 0));
            btnIconBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            // Gradiente amarelo → laranja → vermelho
            var iconGradientBrush = CreateLightningGradientBrush();
            btnIconBlock.SetValue(TextBlock.ForegroundProperty, iconGradientBrush);
            
            return btnIconBlock;
        }

        /// <summary>
        /// Cria o brush de gradiente para o ícone de raio (Gold → DarkOrange → Crimson)
        /// </summary>
        private static LinearGradientBrush CreateLightningGradientBrush()
        {
            var iconGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            
            iconGradientBrush.GradientStops.Add(new GradientStop(
                Color.FromRgb(GRADIENT_GOLD_R, GRADIENT_GOLD_G, GRADIENT_GOLD_B), 
                GRADIENT_STOP_START));
            
            iconGradientBrush.GradientStops.Add(new GradientStop(
                Color.FromRgb(GRADIENT_ORANGE_R, GRADIENT_ORANGE_G, GRADIENT_ORANGE_B), 
                GRADIENT_STOP_MIDDLE));
            
            iconGradientBrush.GradientStops.Add(new GradientStop(
                Color.FromRgb(GRADIENT_CRIMSON_R, GRADIENT_CRIMSON_G, GRADIENT_CRIMSON_B), 
                GRADIENT_STOP_END));
            
            return iconGradientBrush;
        }

        /// <summary>
        /// Executa o componente na versão selecionada ao clicar no botão
        /// </summary>
        private static void ExecuteComponentVersionButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is string version)
                {
                    // Buscar o ItemsControl e seu DataContext (ComponentViewModel) usando VisualTreeHelper
                    DependencyObject parent = btn;
                    ComponentViewModel? vm = null;
                    for (int i = 0; i < VISUAL_TREE_DEPTH_LIMIT && parent != null; i++)
                    {
                        parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                        if (parent is ItemsControl ic && ic.DataContext is ComponentViewModel model)
                        {
                            vm = model;
                            break;
                        }
                    }
                    if (vm != null)
                    {
                        var comp = DevStackManager.Components.ComponentsFactory.GetComponent(vm.Name);
                        if (comp != null && comp.IsExecutable && vm.Installed)
                        {
                            // local helper: procura executável no PATH
                            string? FindOnPath(string name)
                            {
                                try
                                {
                                    var p = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                                    var parts = p.Split(';');
                                    foreach (var part in parts)
                                    {
                                        if (string.IsNullOrWhiteSpace(part)) continue;
                                        try
                                        {
                                            var cand = System.IO.Path.Combine(part.Trim(), name);
                                            if (System.IO.File.Exists(cand)) return cand;
                                        }
                                        catch { }
                                    }
                                }
                                catch { }
                                return null;
                            }
                            // Compute base tool dir and target (installed) directory following ComponentBase semantics
                            string baseToolDir = !string.IsNullOrEmpty(comp.ToolDir) ? comp.ToolDir! : System.IO.Path.Combine(DevStackConfig.baseDir, comp.Name);
                            // Try to read SubDirectory from ComponentBase when available, otherwise fall back to default
                            string subDir = (comp is DevStackManager.Components.ComponentBase cb && !string.IsNullOrEmpty(cb.SubDirectory)) ? cb.SubDirectory! : $"{comp.Name}-{version}";
                            string installDir;

                            if (!string.IsNullOrEmpty(comp.ExecutableFolder))
                            {
                                if (System.IO.Path.IsPathRooted(comp.ExecutableFolder))
                                {
                                    // legacy: ExecutableFolder contained the absolute tool directory
                                    var toolDirArg = comp.ExecutableFolder!;
                                    installDir = System.IO.Path.Combine(toolDirArg, subDir);
                                }
                                else
                                {
                                    installDir = System.IO.Path.Combine(baseToolDir, subDir, comp.ExecutableFolder);
                                }
                            }
                            else
                            {
                                installDir = System.IO.Path.Combine(baseToolDir, subDir);
                            }

                            if (System.IO.Directory.Exists(installDir))
                            {
                                var exeFiles = System.IO.Directory.GetFiles(installDir, "*.exe", System.IO.SearchOption.TopDirectoryOnly);
                                // Busca pelo padrão configurado
                                string? exePath = null;
                                if (!string.IsNullOrEmpty(comp.ExecutablePattern))
                                {
                                    var pattern = comp.ExecutablePattern.Replace("{version}", version);
                                    var patternPath = System.IO.Path.Combine(installDir, pattern);
                                    if (System.IO.File.Exists(patternPath))
                                    {
                                        exePath = patternPath;
                                    }
                                }
                                // Fallback: pega o primeiro .exe
                                if (exePath == null && exeFiles.Length > 0)
                                {
                                    exePath = exeFiles[0];
                                }
                                if (exePath != null && comp.IsExecutable)
                                {
                                    if (comp.IsCommandLine)
                                    {
                                        var wt = FindOnPath("wt.exe");
                                        if (!string.IsNullOrEmpty(wt))
                                        {
                                            var p = new System.Diagnostics.Process();
                                            p.StartInfo.FileName = wt;
                                            p.StartInfo.Arguments = $"new-tab pwsh -NoExit -Command \"& '{exePath}'\"";
                                            p.StartInfo.WorkingDirectory = installDir;
                                            p.StartInfo.UseShellExecute = true;
                                            p.Start();
                                        }
                                        else
                                        {
                                            var ps = new System.Diagnostics.Process();
                                            ps.StartInfo.FileName = "pwsh.exe";
                                            ps.StartInfo.Arguments = $"-NoExit -Command \"& '{exePath}'\"";
                                            ps.StartInfo.WorkingDirectory = installDir;
                                            ps.StartInfo.UseShellExecute = true;
                                            ps.Start();
                                        }
                                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.opening_shell", vm.Name, version);
                                    }
                                    else
                                    {
                                        var process = new System.Diagnostics.Process();
                                        process.StartInfo.FileName = exePath;
                                        process.StartInfo.WorkingDirectory = installDir;
                                        process.StartInfo.UseShellExecute = true;
                                        process.Start();
                                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.executing_component", vm.Name, version, exePath);
                                    }
                                }
                                else
                                {
                                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.no_executable_found", installDir);
                                }
                            }
                            else
                            {
                                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.version_folder_not_found", installDir);
                            }
                        }
                        else
                        {
                            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.component_not_executable", vm.Name);
                        }
                    }
                    else
                    {
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.component_not_available");
                    }
                }
                else
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.version_not_available");
                }
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.error_executing_component", ex.Message);
            }
        }

        /// <summary>
        /// Converter para mostrar o painel de execução apenas se Installed e IsExecutable forem verdadeiros
        /// </summary>
        public class InstalledAndExecutableToVisibilityConverter : System.Windows.Data.IMultiValueConverter
        {
            public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (values.Length == 2 && values[0] is bool installed && values[1] is bool isExecutable)
                {
                    return (installed && isExecutable) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                }
                return System.Windows.Visibility.Collapsed;
            }
            public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Converter para mostrar o texto das versões apenas se NÃO for executável ou não estiver instalado
        /// </summary>
        public class InstalledAndExecutableToCollapsedConverter : System.Windows.Data.IMultiValueConverter
        {
            public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (values.Length == 2 && values[0] is bool installed && values[1] is bool isExecutable)
                {
                    return (installed && isExecutable) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }
                return System.Windows.Visibility.Visible;
            }
            public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
