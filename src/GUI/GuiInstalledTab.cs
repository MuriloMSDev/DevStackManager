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
    /// Installed Components tab component for displaying and managing installed development tools.
    /// Provides DataGrid display with executable component launch buttons.
    /// Shows component name, installed versions, and installation status.
    /// Executable components display launch buttons with lightning icon gradient.
    /// </summary>
    public static class GuiInstalledTab
    {
        /// <summary>
        /// Font size for header text.
        /// </summary>
        private const int HEADER_FONT_SIZE = 18;
        
        /// <summary>
        /// Font size for column headers.
        /// </summary>
        private const int HEADER_FONT_SIZE_COLUMN = 14;
        
        /// <summary>
        /// Bottom margin for title in pixels.
        /// </summary>
        private const int TITLE_BOTTOM_MARGIN = 20;
        
        /// <summary>
        /// Width of refresh button in pixels.
        /// </summary>
        private const int REFRESH_BUTTON_WIDTH = 150;
        
        /// <summary>
        /// Height of refresh button in pixels.
        /// </summary>
        private const int REFRESH_BUTTON_HEIGHT = 35;
        
        /// <summary>
        /// Left margin for refresh button in pixels.
        /// </summary>
        private const int REFRESH_BUTTON_LEFT_MARGIN = 20;
        
        /// <summary>
        /// Width of version launch buttons in pixels.
        /// </summary>
        private const int VERSION_BUTTON_WIDTH = 80;
        
        /// <summary>
        /// Height of version launch buttons in pixels.
        /// </summary>
        private const int VERSION_BUTTON_HEIGHT = 25;
        
        /// <summary>
        /// Margin around version buttons in pixels.
        /// </summary>
        private const int VERSION_BUTTON_MARGIN = 2;
        
        /// <summary>
        /// Font size for version button icon.
        /// </summary>
        private const int VERSION_ICON_FONT_SIZE = 16;
        
        /// <summary>
        /// Left margin for version icon in pixels.
        /// </summary>
        private const int VERSION_ICON_LEFT_MARGIN = 4;
        
        /// <summary>
        /// Width of Name column in pixels.
        /// </summary>
        private const int COLUMN_NAME_WIDTH = 200;
        
        /// <summary>
        /// Width of Versions column in pixels.
        /// </summary>
        private const int COLUMN_VERSIONS_WIDTH = 400;
        
        /// <summary>
        /// Width of Status column in pixels.
        /// </summary>
        private const int COLUMN_STATUS_WIDTH = 100;
        
        /// <summary>
        /// Panel margin in pixels.
        /// </summary>
        private const int PANEL_MARGIN = 10;
        
        /// <summary>
        /// Left padding for text in pixels.
        /// </summary>
        private const int TEXT_PADDING_LEFT = 12;
        
        /// <summary>
        /// Left padding for versions text in pixels.
        /// </summary>
        private const int TEXT_PADDING_LEFT_VERSIONS = 13;
        
        /// <summary>
        /// Maximum depth for visual tree traversal.
        /// </summary>
        private const int VISUAL_TREE_DEPTH_LIMIT = 10;
        
        /// <summary>
        /// Red component of gradient gold color.
        /// </summary>
        private const byte GRADIENT_GOLD_R = 255;
        
        /// <summary>
        /// Green component of gradient gold color.
        /// </summary>
        private const byte GRADIENT_GOLD_G = 215;
        
        /// <summary>
        /// Blue component of gradient gold color.
        /// </summary>
        private const byte GRADIENT_GOLD_B = 0;
        
        /// <summary>
        /// Red component of gradient orange color.
        /// </summary>
        private const byte GRADIENT_ORANGE_R = 255;
        
        /// <summary>
        /// Green component of gradient orange color.
        /// </summary>
        private const byte GRADIENT_ORANGE_G = 140;
        
        /// <summary>
        /// Blue component of gradient orange color.
        /// </summary>
        private const byte GRADIENT_ORANGE_B = 0;
        
        /// <summary>
        /// Red component of gradient crimson color.
        /// </summary>
        private const byte GRADIENT_CRIMSON_R = 220;
        
        /// <summary>
        /// Green component of gradient crimson color.
        /// </summary>
        private const byte GRADIENT_CRIMSON_G = 20;
        
        /// <summary>
        /// Blue component of gradient crimson color.
        /// </summary>
        private const byte GRADIENT_CRIMSON_B = 60;
        
        /// <summary>
        /// Start position for gradient stop (0.0).
        /// </summary>
        private const double GRADIENT_STOP_START = 0.0;
        
        /// <summary>
        /// Middle position for gradient stop (0.5).
        /// </summary>
        private const double GRADIENT_STOP_MIDDLE = 0.5;
        
        /// <summary>
        /// End position for gradient stop (1.0).
        /// </summary>
        private const double GRADIENT_STOP_END = 1.0;
        
        /// <summary>
        /// Lightning icon character for executable components.
        /// </summary>
        private const string LIGHTNING_ICON = " 🗲";

        /// <summary>
        /// Creates the complete Installed Components tab content.
        /// Layout: header with title and refresh button, DataGrid with installed components, info panel.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and localization.</param>
        /// <returns>Grid with installed components display.</returns>
        public static Grid CreateInstalledContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerPanel = CreateInstalledHeader(mainWindow);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            var dataGridContainer = CreateInstalledDataGrid(mainWindow);
            Grid.SetRow(dataGridContainer, 1);
            grid.Children.Add(dataGridContainer);

            var infoPanel = CreateInstalledInfoPanel(mainWindow);
            Grid.SetRow(infoPanel, 2);
            grid.Children.Add(infoPanel);

            return grid;
        }

        /// <summary>
        /// Creates the header panel for Installed tab with title and refresh button.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>StackPanel with title label and refresh button.</returns>
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

        /// <summary>
        /// Creates the title label for the Installed tab header.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>Styled label with "Installed Components" title</returns>
        private static Label CreateTitleLabel(DevStackGui mainWindow)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.installed_tab.title"), 
                true);
            titleLabel.FontSize = HEADER_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_BOTTOM_MARGIN);
            return titleLabel;
        }

        /// <summary>
        /// Creates the refresh button for reloading installed components.
        /// Calls mainWindow.LoadInstalledComponents() on click.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and component loading</param>
        /// <returns>Styled button with async refresh handler</returns>
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
        /// Creates the DataGrid container with fixed header and scrollable content for installed components.
        /// Uses two-DataGrid approach: header-only DataGrid for fixed column headers, content DataGrid wrapped in ScrollViewer.
        /// Columns: Tool Name (200px), Installed Versions (400px with launch buttons), Status (100px).
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and localization</param>
        /// <returns>Grid container with fixed header and scrollable content DataGrid</returns>
        private static Grid CreateInstalledDataGrid(DevStackGui mainWindow)
        {
            var containerGrid = new Grid
            {
                Margin = new Thickness(PANEL_MARGIN)
            };
            
            containerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            containerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var headerDataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                IsHitTestVisible = false
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                CanContentScroll = false,
                PanningMode = PanningMode.VerticalOnly,
                IsManipulationEnabled = true
            };

            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

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
                ColumnHeaderHeight = 0,
                CanUserResizeColumns = false,
                CanUserReorderColumns = false,
                CanUserSortColumns = false
            };

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

                var versionsColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.installed_tab.headers.versions") : null,
                    Width = new DataGridLength(COLUMN_VERSIONS_WIDTH)
                };
                var versionsTemplate = new DataTemplate();
                var versionsPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
                versionsPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                versionsPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                var multiBinding = new System.Windows.Data.MultiBinding { Converter = new InstalledAndExecutableToVisibilityConverter() };
                multiBinding.Bindings.Add(new Binding("Installed"));
                multiBinding.Bindings.Add(new Binding("IsExecutable"));
                var versionsBinding = new Binding("Versions");
                versionsPanelFactory.SetBinding(FrameworkElement.TagProperty, versionsBinding);
                var itemTemplate = new DataTemplate();
                var buttonFactory = new FrameworkElementFactory(typeof(Button));
                buttonFactory.SetValue(Button.WidthProperty, (double)VERSION_BUTTON_WIDTH);
                buttonFactory.SetValue(Button.HeightProperty, (double)VERSION_BUTTON_HEIGHT);
                buttonFactory.SetValue(Button.MarginProperty, new Thickness(VERSION_BUTTON_MARGIN));
                buttonFactory.SetValue(Button.StyleProperty, DevStackShared.ThemeManager.CreateStyledButton("", null, DevStackShared.ThemeManager.ButtonStyle.Secondary).Style);
                var btnStackPanel = new FrameworkElementFactory(typeof(StackPanel));
                btnStackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                btnStackPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                var btnTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                btnTextBlock.SetBinding(TextBlock.TextProperty, new Binding("."));
                btnTextBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                btnStackPanel.AppendChild(btnTextBlock);
                var btnIconBlock = CreateVersionIconWithGradient();
                btnStackPanel.AppendChild(btnIconBlock);
                buttonFactory.AppendChild(btnStackPanel);
                buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => ExecuteComponentVersionButton_Click(sender, e, mainWindow)));
                itemTemplate.VisualTree = buttonFactory;
                var itemsControlFactory = new FrameworkElementFactory(typeof(ItemsControl));
                itemsControlFactory.SetBinding(ItemsControl.ItemsSourceProperty, versionsBinding);
                itemsControlFactory.SetValue(ItemsControl.ItemTemplateProperty, itemTemplate);
                var itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(WrapPanel)));
                itemsControlFactory.SetValue(ItemsControl.ItemsPanelProperty, itemsPanelTemplate);
                itemsControlFactory.SetBinding(ItemsControl.VisibilityProperty, multiBinding);
                versionsPanelFactory.AppendChild(itemsControlFactory);
                var versionsTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                versionsTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("VersionsText"));
                versionsTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(TEXT_PADDING_LEFT_VERSIONS, 0, 0, 0));
                versionsTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
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

            ConfigureColumns(headerDataGrid, true);
            ConfigureColumns(contentDataGrid, false);

            var installedBinding = new Binding("InstalledComponents") { Source = mainWindow };
            contentDataGrid.SetBinding(DataGrid.ItemsSourceProperty, installedBinding);

            DevStackShared.ThemeManager.SetDataGridDarkTheme(headerDataGrid);
            DevStackShared.ThemeManager.SetDataGridDarkTheme(contentDataGrid);

            scrollViewer.Content = contentDataGrid;

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

            Grid.SetRow(headerDataGrid, 0);
            Grid.SetRow(scrollViewer, 1);
            
            containerGrid.Children.Add(headerDataGrid);
            containerGrid.Children.Add(scrollViewer);

            return containerGrid;
        }

        /// <summary>
        /// Creates the info panel displayed at the footer of the Installed tab.
        /// Shows informational notification with usage instructions.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>StackPanel with centered info notification</returns>
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
        /// Creates a lightning icon (🗲) with gradient foreground for version launch buttons.
        /// Gradient transitions from Gold → Orange → Crimson for visual emphasis on executable components.
        /// </summary>
        /// <returns>FrameworkElementFactory for TextBlock with lightning icon and gradient brush</returns>
        private static FrameworkElementFactory CreateVersionIconWithGradient()
        {
            var btnIconBlock = new FrameworkElementFactory(typeof(TextBlock));
            btnIconBlock.SetValue(TextBlock.TextProperty, LIGHTNING_ICON);
            btnIconBlock.SetValue(TextBlock.FontSizeProperty, (double)VERSION_ICON_FONT_SIZE);
            btnIconBlock.SetValue(TextBlock.MarginProperty, new Thickness(VERSION_ICON_LEFT_MARGIN, 0, 0, 0));
            btnIconBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            var iconGradientBrush = CreateLightningGradientBrush();
            btnIconBlock.SetValue(TextBlock.ForegroundProperty, iconGradientBrush);
            
            return btnIconBlock;
        }

        /// <summary>
        /// Creates a linear gradient brush for the lightning icon.
        /// Gradient colors: Gold (255,215,0) → Orange (255,140,0) → Crimson (220,20,60).
        /// Gradient stops at 0.0, 0.5, and 1.0 for smooth color transition.
        /// </summary>
        /// <returns>LinearGradientBrush with Gold to Crimson gradient</returns>
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
        /// Handles click event on component version launch buttons.
        /// Traverses visual tree to find parent ComponentViewModel (max 10 levels), determines if component is command-line or GUI-based,
        /// then executes via ProcessManager.ExecuteComponent. Command-line components open in external terminal.
        /// </summary>
        /// <param name="sender">Button that was clicked</param>
        /// <param name="e">Routed event arguments</param>
        /// <param name="mainWindow">Main window reference for error dialogs and localization</param>
        private static void ExecuteComponentVersionButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is string version)
                {
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
                            string baseToolDir = !string.IsNullOrEmpty(comp.ToolDir) ? comp.ToolDir! : System.IO.Path.Combine(DevStackConfig.baseDir, comp.Name);
                            string subDir = (comp is DevStackManager.Components.ComponentBase cb && !string.IsNullOrEmpty(cb.SubDirectory)) ? cb.SubDirectory! : $"{comp.Name}-{version}";
                            string installDir;

                            if (!string.IsNullOrEmpty(comp.ExecutableFolder))
                            {
                                if (System.IO.Path.IsPathRooted(comp.ExecutableFolder))
                                {
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
        /// Multi-value converter for showing executable version buttons.
        /// Shows buttons (Visible) only when component is both Installed and IsExecutable are true.
        /// Otherwise returns Collapsed visibility.
        /// </summary>
        public class InstalledAndExecutableToVisibilityConverter : System.Windows.Data.IMultiValueConverter
        {
            /// <summary>
            /// Converts installed and executable status to visibility.
            /// </summary>
            /// <param name="values">Array of two booleans: installed status and executable status.</param>
            /// <param name="targetType">The type of the binding target property.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Visible if both installed and executable, otherwise Collapsed.</returns>
            public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (values.Length == 2 && values[0] is bool installed && values[1] is bool isExecutable)
                {
                    return (installed && isExecutable) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                }
                return System.Windows.Visibility.Collapsed;
            }
            /// <summary>
            /// Converts back from visibility to installed and executable status (not implemented).
            /// </summary>
            /// <param name="value">The value produced by the binding target.</param>
            /// <param name="targetTypes">The array of types to convert to.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Array of values for the binding sources.</returns>
            public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Multi-value converter for showing version text instead of buttons.
        /// Shows text (Visible) when component is NOT executable or NOT installed.
        /// Returns Collapsed when component is both installed and executable (buttons shown instead).
        /// </summary>
        public class InstalledAndExecutableToCollapsedConverter : System.Windows.Data.IMultiValueConverter
        {
            /// <summary>
            /// Converts installed and executable status to collapsed visibility.
            /// </summary>
            /// <param name="values">Array of two booleans: installed status and executable status.</param>
            /// <param name="targetType">The type of the binding target property.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Collapsed if both installed and executable, otherwise Visible.</returns>
            public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (values.Length == 2 && values[0] is bool installed && values[1] is bool isExecutable)
                {
                    return (installed && isExecutable) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }
                return System.Windows.Visibility.Visible;
            }
            /// <summary>
            /// Converts back from visibility to installed and executable status (not implemented).
            /// </summary>
            /// <param name="value">The value produced by the binding target.</param>
            /// <param name="targetTypes">The array of types to convert to.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Array of values for the binding sources.</returns>
            public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
