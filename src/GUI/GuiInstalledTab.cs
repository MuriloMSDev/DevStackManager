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
                Margin = new Thickness(10)
            };
            
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.installed_tab.title"), true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            headerPanel.Children.Add(titleLabel);

            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.installed_tab.buttons.refresh"), async (s, e) => await LoadInstalledComponents(mainWindow));
            refreshButton.Width = 150;
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
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.installed_tab.headers.tool") : null,
                    Width = new DataGridLength(200)
                };

                var nameTemplate = new DataTemplate();
                var nameTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                nameTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
                nameTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
                nameTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                nameTemplate.VisualTree = nameTextBlockFactory;
                nameColumn.CellTemplate = nameTemplate;

                dataGrid.Columns.Add(nameColumn);

                // Coluna Versões Instaladas
                var versionsColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.installed_tab.headers.versions") : null,
                    Width = new DataGridLength(400)
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
                buttonFactory.SetValue(Button.WidthProperty, 80.0);
                buttonFactory.SetValue(Button.HeightProperty, 25.0);
                buttonFactory.SetValue(Button.MarginProperty, new Thickness(2));
                buttonFactory.SetValue(Button.StyleProperty, DevStackShared.ThemeManager.CreateStyledButton("", null, DevStackShared.ThemeManager.ButtonStyle.Success).Style);
                // StackPanel para versão + ícone
                var btnStackPanel = new FrameworkElementFactory(typeof(StackPanel));
                btnStackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                btnStackPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                // Texto da versão
                var btnTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                btnTextBlock.SetBinding(TextBlock.TextProperty, new Binding("."));
                btnTextBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                btnStackPanel.AppendChild(btnTextBlock);
                // Ícone ▶️
                var btnIconBlock = new FrameworkElementFactory(typeof(TextBlock));
                btnIconBlock.SetValue(TextBlock.TextProperty, " ▶️");
                btnIconBlock.SetValue(TextBlock.MarginProperty, new Thickness(4, 0, 0, 0));
                btnIconBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
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
                versionsTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(13, 0, 0, 0));
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
                    Width = new DataGridLength(100)
                };
                
                // Centralizar o header da coluna Status apenas se for header
                if (isHeader)
                {
                    var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderBackground));
                    headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.GridHeaderForeground));
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
                Margin = new Thickness(10),
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
        /// Carrega a lista de componentes instalados de forma assíncrona
        /// </summary>
        public static async Task LoadInstalledComponents(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.installed_tab.loading");
                    
                    var data = DataManager.GetInstalledVersions();
                    var components = new ObservableCollection<ComponentViewModel>();
                    
                    foreach (var comp in data.Components)
                    {
                        components.Add(new ComponentViewModel
                        {
                            Name = comp.Name,
                            Installed = comp.Installed,
                            IsExecutable = comp.IsExecutable,
                            Versions = comp.Versions,
                            Status = mainWindow.LocalizationManager.GetString("gui.common.status." + (comp.Installed ? "ok" : "error")),
                            VersionsText = comp.Installed ? string.Join(", ", comp.Versions) : mainWindow.LocalizationManager.GetString("gui.common.status.na")
                        });
                    }
                    
                    // Ordena: instalados primeiro
                    var ordered = components.OrderByDescending(c => c.Installed).ThenBy(c => c.Name).ToList();
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.InstalledComponents = new ObservableCollection<ComponentViewModel>(ordered);
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.installed_tab.loaded", ordered.Count);
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.installed_tab.error", ex.Message);
                        DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.installed_tab.error", ex));
                    });
                }
            });
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
                    for (int i = 0; i < 10 && parent != null; i++)
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
                                        mainWindow.StatusMessage = $"Abrindo shell interativo para {vm.Name} versão {version}";
                                    }
                                    else
                                    {
                                        var process = new System.Diagnostics.Process();
                                        process.StartInfo.FileName = exePath;
                                        process.StartInfo.WorkingDirectory = installDir;
                                        process.StartInfo.UseShellExecute = true;
                                        process.Start();
                                        mainWindow.StatusMessage = $"Executando {vm.Name} versão {version}: {exePath}";
                                    }
                                }
                                else
                                {
                                    mainWindow.StatusMessage = $"Nenhum executável encontrado em {installDir}";
                                }
                            }
                            else
                            {
                                mainWindow.StatusMessage = $"Pasta da versão não encontrada: {installDir}";
                            }
                        }
                        else
                        {
                            mainWindow.StatusMessage = $"Componente {vm.Name} não é executável ou não está instalado.";
                        }
                    }
                    else
                    {
                        mainWindow.StatusMessage = $"Não foi possível obter o componente para execução.";
                    }
                }
                else
                {
                    mainWindow.StatusMessage = $"Não foi possível obter a versão para execução.";
                }
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = $"Erro ao executar componente: {ex.Message}";
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
