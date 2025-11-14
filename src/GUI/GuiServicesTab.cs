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
    /// Boolean to Visibility converter for WPF data binding.
    /// Converts true to Visible and false to Collapsed.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Visible if true, Collapsed if false.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value.
        /// </summary>
        /// <param name="value">The Visibility value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>True if Visible, false otherwise.</returns>
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
    /// Services tab component for managing and controlling DevStack services.
    /// Provides DataGrid display of running services with start/stop/restart controls.
    /// Shows component name, version, status, PID, and action buttons for each service.
    /// Includes bulk operations: Start All, Stop All, Restart All services.
    /// </summary>
    public static class GuiServicesTab
    {
        /// <summary>
        /// Width of the component name column in the services DataGrid.
        /// </summary>
        private const int COMPONENT_COLUMN_WIDTH = 120;
        
        /// <summary>
        /// Width of the version column in the services DataGrid.
        /// </summary>
        private const int VERSION_COLUMN_WIDTH = 100;
        
        /// <summary>
        /// Width of the status column in the services DataGrid.
        /// </summary>
        private const int STATUS_COLUMN_WIDTH = 120;
        
        /// <summary>
        /// Width of the PID column in the services DataGrid.
        /// </summary>
        private const int PID_COLUMN_WIDTH = 330;
        
        /// <summary>
        /// Width of the copy PID button column in the services DataGrid.
        /// </summary>
        private const int COPY_PID_COLUMN_WIDTH = 100;
        
        /// <summary>
        /// Width of the actions column in the services DataGrid.
        /// </summary>
        private const int ACTIONS_COLUMN_WIDTH = 120;
        
        /// <summary>
        /// Width of action buttons (start/stop/restart) in pixels.
        /// </summary>
        private const int BUTTON_WIDTH = 30;
        
        /// <summary>
        /// Height of action buttons (start/stop/restart) in pixels.
        /// </summary>
        private const int BUTTON_HEIGHT = 25;
        
        /// <summary>
        /// Width of the copy PID button in pixels.
        /// </summary>
        private const int COPY_BUTTON_WIDTH = 35;
        
        /// <summary>
        /// Width of control buttons (Start All/Stop All/Restart All) in pixels.
        /// </summary>
        private const int CONTROL_BUTTON_WIDTH = 150;
        
        /// <summary>
        /// Height of control buttons (Start All/Stop All/Restart All) in pixels.
        /// </summary>
        private const int CONTROL_BUTTON_HEIGHT = 40;
        
        /// <summary>
        /// Delay in milliseconds after starting a service before UI refresh.
        /// </summary>
        private const int POST_START_DELAY_MS = 1000;
        
        /// <summary>
        /// Delay in milliseconds after stopping a service before UI refresh.
        /// </summary>
        private const int POST_STOP_DELAY_MS = 500;
        
        /// <summary>
        /// Delay in milliseconds after restarting a service before UI refresh.
        /// </summary>
        private const int POST_RESTART_DELAY_MS = 1500;
        
        /// <summary>
        /// Delay in milliseconds after restart all operation before UI refresh.
        /// </summary>
        private const int RESTART_ALL_DELAY_MS = 2000;

        /// <summary>
        /// Loading overlay border displayed during service operations.
        /// </summary>
        private static Border? ServicesLoadingOverlay;
        
        /// <summary>
        /// Creates the complete Services tab content with three-row layout.
        /// Row 1: Header with title. Row 2: Control panel with Start All/Stop All/Restart All buttons.
        /// Row 3: DataGrid with service list (Component, Version, Status, PID, Copy PID button, Start/Stop/Restart action buttons).
        /// Loading overlay spans all rows during service operations.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and localization</param>
        /// <returns>Grid with services management interface</returns>
        public static Grid CreateServicesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var headerPanel = CreateServicesHeader(mainWindow);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            var servicesGrid = CreateServicesDataGrid(mainWindow);
            Grid.SetRow(servicesGrid, 2);
            grid.Children.Add(servicesGrid);

            var controlPanel = CreateServicesControlPanel(mainWindow);
            Grid.SetRow(controlPanel, 1);
            grid.Children.Add(controlPanel);

            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
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
        /// Creates the header panel for Services tab with title label.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>StackPanel with Services title</returns>
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
        /// Creates the DataGrid container with fixed header and scrollable content for services display.
        /// Uses two-DataGrid approach: header-only DataGrid for column headers, content DataGrid wrapped in ScrollViewer.
        /// Columns: Component (120px), Version (100px), Status (120px with color coding), PID (330px), Copy PID button (100px), Actions (120px with Start/Stop/Restart buttons).
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and localization</param>
        /// <returns>Grid container with fixed header and scrollable service DataGrid</returns>
        private static Grid CreateServicesDataGrid(DevStackGui mainWindow)
        {
            var containerGrid = new Grid
            {
                Margin = new Thickness(10)
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
                CanUserSortColumns = false,
                Name = "ServicesDataGrid"
            };

            void ConfigureColumns(DataGrid dataGrid, bool isHeader = false)
            {
                var componentColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.component") : null,
                    Width = new DataGridLength(120)
                };

                var componentTemplate = new DataTemplate();
                var componentTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                componentTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Label"));
                componentTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
                componentTextBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                componentTextBlockFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                componentTemplate.VisualTree = componentTextBlockFactory;
                componentColumn.CellTemplate = componentTemplate;

                dataGrid.Columns.Add(componentColumn);

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

                var copyButtonTemplate = new DataTemplate();
                
                var copyButtonStyleTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.copy_pid"), null, DevStackShared.ThemeManager.ButtonStyle.Secondary);
                
                var copyButtonFactory = new FrameworkElementFactory(typeof(Button));
                copyButtonFactory.SetValue(Button.ContentProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.copy_pid"));
                copyButtonFactory.SetValue(Button.HeightProperty, 25.0);
                copyButtonFactory.SetValue(Button.WidthProperty, 35.0);
                copyButtonFactory.SetValue(Button.FontSizeProperty, 12.0);
                copyButtonFactory.SetValue(Button.ToolTipProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.tooltips.copy_pid"));
                copyButtonFactory.SetValue(Button.StyleProperty, copyButtonStyleTemplate.Style);
                
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

                var actionsTemplate = new DataTemplate();
                var actionsPanel = new FrameworkElementFactory(typeof(StackPanel));
                actionsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                actionsPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

                var startButtonTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.start"), null, DevStackShared.ThemeManager.ButtonStyle.Success);
                var stopButtonTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.stop"), null, DevStackShared.ThemeManager.ButtonStyle.Danger);
                var restartButtonTemplate = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.restart"), null, DevStackShared.ThemeManager.ButtonStyle.Warning);

                var startButton = new FrameworkElementFactory(typeof(Button));
                startButton.SetValue(Button.ContentProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.start"));
                startButton.SetValue(Button.WidthProperty, 30.0);
                startButton.SetValue(Button.HeightProperty, 25.0);
                startButton.SetValue(Button.MarginProperty, new Thickness(2));
                startButton.SetValue(Button.ToolTipProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.tooltips.start"));
                startButton.SetValue(Button.StyleProperty, startButtonTemplate.Style);
                startButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => StartServiceButton_Click(sender, e, mainWindow)));
                actionsPanel.AppendChild(startButton);

                var stopButton = new FrameworkElementFactory(typeof(Button));
                stopButton.SetValue(Button.ContentProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.buttons.stop"));
                stopButton.SetValue(Button.WidthProperty, 30.0);
                stopButton.SetValue(Button.HeightProperty, 25.0);
                stopButton.SetValue(Button.MarginProperty, new Thickness(2));
                stopButton.SetValue(Button.ToolTipProperty, mainWindow.LocalizationManager.GetString("gui.services_tab.tooltips.stop"));
                stopButton.SetValue(Button.StyleProperty, stopButtonTemplate.Style);
                stopButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => StopServiceButton_Click(sender, e, mainWindow)));
                actionsPanel.AppendChild(stopButton);

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

                var actionsColumn = new DataGridTemplateColumn
                {
                    Header = isHeader ? mainWindow.LocalizationManager.GetString("gui.services_tab.headers.actions") : null,
                    Width = new DataGridLength(120)
                };
                
                actionsColumn.CellTemplate = actionsTemplate;
                dataGrid.Columns.Add(actionsColumn);

                if (isHeader)
                {
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
                    
                    componentColumn.HeaderStyle = leftHeaderStyle;
                    versionColumn.HeaderStyle = leftHeaderStyle;
                    statusColumn.HeaderStyle = centerHeaderStyle;
                    pidColumn.HeaderStyle = leftHeaderStyle;
                    dataGrid.Columns[4].HeaderStyle = centerHeaderStyle;
                    actionsColumn.HeaderStyle = centerHeaderStyle;
                }
                else
                {
                    var hiddenHeaderStyle = new Style(typeof(DataGridColumnHeader));
                    hiddenHeaderStyle.Setters.Add(new Setter(DataGridColumnHeader.VisibilityProperty, Visibility.Collapsed));
                    componentColumn.HeaderStyle = hiddenHeaderStyle;
                    versionColumn.HeaderStyle = hiddenHeaderStyle;
                    statusColumn.HeaderStyle = hiddenHeaderStyle;
                    pidColumn.HeaderStyle = hiddenHeaderStyle;
                    dataGrid.Columns[4].HeaderStyle = hiddenHeaderStyle;
                    actionsColumn.HeaderStyle = hiddenHeaderStyle;
                }
            }

            ConfigureColumns(headerDataGrid, true);
            ConfigureColumns(contentDataGrid, false);

            var servicesBinding = new Binding("Services") { Source = mainWindow };
            contentDataGrid.SetBinding(DataGrid.ItemsSourceProperty, servicesBinding);

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
        /// Handles click event on Copy PID button.
        /// Copies the service PID to clipboard and shows success message.
        /// Shows warning if PID is not available.
        /// </summary>
        /// <param name="sender">Button that was clicked</param>
        /// <param name="e">Routed event arguments</param>
        /// <param name="mainWindow">Main window reference for status updates and localization</param>
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
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_copy_pid", ex.Message));
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.error_copy_pid", ex.Message);
            }
        }

        /// <summary>
        /// Handles click event on Start service button for individual service.
        /// Starts the service via ProcessManager.StartComponent and updates service list after 1 second delay.
        /// </summary>
        /// <param name="sender">Button that was clicked</param>
        /// <param name="e">Routed event arguments</param>
        /// <param name="mainWindow">Main window reference for service operations</param>
        private static async void StartServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            await ExecuteServiceAction(sender, mainWindow, async (service) =>
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.starting", service.Name, service.Version);
                await Task.Run(() => ProcessManager.StartComponent(service.Name, service.Version));
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.started", service.Name);
                await Task.Delay(POST_START_DELAY_MS);
            }, "gui.services_tab.messages.error_start");
        }

        /// <summary>
        /// Handles click event on Stop service button for individual service.
        /// Stops the service via ProcessManager.StopComponent and updates service list after 500ms delay.
        /// </summary>
        /// <param name="sender">Button that was clicked</param>
        /// <param name="e">Routed event arguments</param>
        /// <param name="mainWindow">Main window reference for service operations</param>
        private static async void StopServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            await ExecuteServiceAction(sender, mainWindow, async (service) =>
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.stopping", service.Name, service.Version);
                await Task.Run(() => ProcessManager.StopComponent(service.Name, service.Version));
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.stopped", service.Name);
                await Task.Delay(POST_STOP_DELAY_MS);
            }, "gui.services_tab.messages.error_stop");
        }

        /// <summary>
        /// Handles click event on Restart service button for individual service.
        /// Restarts the service via ProcessManager.RestartComponent and updates service list after 1.5 second delay.
        /// </summary>
        /// <param name="sender">Button that was clicked</param>
        /// <param name="e">Routed event arguments</param>
        /// <param name="mainWindow">Main window reference for service operations</param>
        private static async void RestartServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            await ExecuteServiceAction(sender, mainWindow, async (service) =>
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.restarting", service.Name, service.Version);
                await Task.Run(() => ProcessManager.RestartComponent(service.Name, service.Version));
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.services_tab.messages.restarted", service.Name);
                await Task.Delay(POST_RESTART_DELAY_MS);
            }, "gui.services_tab.messages.error_restart");
        }

        /// <summary>
        /// Executes a service action with error handling and loading state management.
        /// Shows loading overlay, performs action, refreshes service status, and handles errors.
        /// </summary>
        /// <param name="sender">Button that triggered the action</param>
        /// <param name="mainWindow">Main window reference for loading state and service refresh</param>
        /// <param name="action">Async action to perform on the service</param>
        /// <param name="errorMessageKey">Localization key for error message</param>
        /// <returns>Task representing the async service action</returns>
        private static async Task ExecuteServiceAction(
            object sender,
            DevStackGui mainWindow,
            Func<ServiceViewModel, Task> action,
            string errorMessageKey)
        {
            SetLoadingState(mainWindow, true);
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    await action(service);
                    await mainWindow.RefreshServicesStatus();
                }
            }
            catch (Exception ex)
            {
                HandleServiceError(mainWindow, errorMessageKey, ex);
            }
            finally
            {
                SetLoadingState(mainWindow, false);
            }
        }

        /// <summary>
        /// Sets the loading state for services operations.
        /// Controls IsLoadingServices property and ServicesLoadingOverlay visibility.
        /// </summary>
        /// <param name="mainWindow">Main window with IsLoadingServices property</param>
        /// <param name="isLoading">True to show loading overlay, false to hide</param>
        private static void SetLoadingState(DevStackGui mainWindow, bool isLoading)
        {
            mainWindow.IsLoadingServices = isLoading;
            if (ServicesLoadingOverlay != null)
            {
                ServicesLoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles errors that occur during service operations.
        /// Logs error to console, updates status bar, and writes to DevStack log.
        /// </summary>
        /// <param name="mainWindow">Main window reference for status and localization</param>
        /// <param name="errorMessageKey">Localization key for error message</param>
        /// <param name="ex">Exception that occurred</param>
        private static void HandleServiceError(DevStackGui mainWindow, string errorMessageKey, Exception ex)
        {
            var errorMessage = mainWindow.LocalizationManager.GetString(errorMessageKey, ex.Message);
            GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, errorMessage);
            mainWindow.StatusMessage = errorMessage;
            DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString(errorMessageKey, ex));
        }

        /// <summary>
        /// Creates the service control panel with bulk operation buttons.
        /// Buttons: Start All (Success style), Stop All (Danger style), Restart All (Warning style), Refresh (Secondary style).
        /// </summary>
        /// <param name="mainWindow">Main window reference for localization and operations</param>
        /// <returns>StackPanel with control buttons</returns>
        private static StackPanel CreateServicesControlPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            panel.Children.Add(CreateControlButton(
                mainWindow,
                "gui.services_tab.buttons.start_all",
                StartAllServices,
                DevStackShared.ThemeManager.ButtonStyle.Success
            ));

            panel.Children.Add(CreateControlButton(
                mainWindow,
                "gui.services_tab.buttons.stop_all",
                StopAllServices,
                DevStackShared.ThemeManager.ButtonStyle.Danger
            ));

            panel.Children.Add(CreateControlButton(
                mainWindow,
                "gui.services_tab.buttons.restart_all",
                RestartAllServices,
                DevStackShared.ThemeManager.ButtonStyle.Warning
            ));

            panel.Children.Add(CreateControlButton(
                mainWindow,
                "gui.services_tab.buttons.refresh",
                async (mw) => await mw.LoadServices(),
                DevStackShared.ThemeManager.ButtonStyle.Primary
            ));

            return panel;
        }

        /// <summary>
        /// Creates a control button for bulk service operations.
        /// Wraps the action with loading state management (shows/hides overlay).
        /// </summary>
        /// <param name="mainWindow">Main window reference</param>
        /// <param name="labelKey">Localization key for button label</param>
        /// <param name="action">Async action to execute on button click</param>
        /// <param name="style">Button visual style (Success, Danger, Warning, Primary)</param>
        /// <returns>Styled button with async action handler</returns>
        private static Button CreateControlButton(
            DevStackGui mainWindow,
            string labelKey,
            Func<DevStackGui, Task> action,
            DevStackShared.ThemeManager.ButtonStyle style)
        {
            var button = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString(labelKey),
                async (s, e) =>
                {
                    SetLoadingState(mainWindow, true);
                    try { await action(mainWindow); }
                    finally { SetLoadingState(mainWindow, false); }
                },
                style
            );

            button.Width = CONTROL_BUTTON_WIDTH;
            button.Height = CONTROL_BUTTON_HEIGHT;
            button.Margin = new Thickness(10);
            return button;
        }

        /// <summary>
        /// Starts all registered services via ProcessManager.StartAllComponents.
        /// Shows progress messages and refreshes service list after completion.
        /// </summary>
        /// <param name="mainWindow">Main window reference for status updates and service refresh</param>
        /// <returns>Task representing the async start operation</returns>
        private static async Task StartAllServices(DevStackGui mainWindow)
        {
            await ExecuteAllServicesAction(
                mainWindow,
                "gui.services_tab.messages.starting_all",
                "gui.services_tab.messages.started_all",
                "gui.services_tab.messages.error_start_all",
                "gui.services_tab.debug.start_all_services_error",
                () => ProcessManager.StartAllComponents()
            );
        }

        /// <summary>
        /// Stops all registered services via ProcessManager.StopAllComponents.
        /// Shows progress messages and refreshes service list after completion.
        /// </summary>
        /// <param name="mainWindow">Main window reference for status updates and service refresh</param>
        /// <returns>Task representing the async stop operation</returns>
        private static async Task StopAllServices(DevStackGui mainWindow)
        {
            await ExecuteAllServicesAction(
                mainWindow,
                "gui.services_tab.messages.stopping_all",
                "gui.services_tab.messages.stopped_all",
                "gui.services_tab.messages.error_stop_all",
                "gui.services_tab.debug.stop_all_services_error",
                () => ProcessManager.StopAllComponents()
            );
        }

        /// <summary>
        /// Restarts all registered services by stopping then starting them.
        /// Includes 2-second delay between stop and start operations.
        /// Shows progress messages and refreshes service list after completion.
        /// </summary>
        /// <param name="mainWindow">Main window reference for status updates and service refresh</param>
        /// <returns>Task representing the async restart operation</returns>
        private static async Task RestartAllServices(DevStackGui mainWindow)
        {
            await ExecuteAllServicesAction(
                mainWindow,
                "gui.services_tab.messages.restarting_all",
                "gui.services_tab.messages.restarted_all",
                "gui.services_tab.messages.error_restart_all",
                "gui.services_tab.debug.restart_all_services_error",
                () =>
                {
                    ProcessManager.StopAllComponents();
                    System.Threading.Thread.Sleep(RESTART_ALL_DELAY_MS);
                    ProcessManager.StartAllComponents();
                }
            );
        }

        /// <summary>
        /// Executes a bulk service action with error handling and status updates.
        /// Shows start message, performs action in background thread, shows success message, refreshes service list.
        /// Logs errors to console and DevStack log if operation fails.
        /// </summary>
        /// <param name="mainWindow">Main window reference for status and service refresh</param>
        /// <param name="startMessageKey">Localization key for operation start message</param>
        /// <param name="successMessageKey">Localization key for operation success message</param>
        /// <param name="errorMessageKey">Localization key for error message shown to user</param>
        /// <param name="logMessageKey">Localization key for error message written to log</param>
        /// <param name="action">Synchronous action to execute in background thread</param>
        /// <returns>Task representing the async operation</returns>
        private static async Task ExecuteAllServicesAction(
            DevStackGui mainWindow,
            string startMessageKey,
            string successMessageKey,
            string errorMessageKey,
            string logMessageKey,
            Action action)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(startMessageKey);

            try
            {
                await Task.Run(action);
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(successMessageKey);
                await mainWindow.LoadServices();
            }
            catch (Exception ex)
            {
                var errorMessage = mainWindow.LocalizationManager.GetString(errorMessageKey, ex.Message);
                GuiConsolePanel.Append(GuiConsolePanel.ConsoleTab.Sites, errorMessage);
                mainWindow.StatusMessage = errorMessage;
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString(logMessageKey, ex));
            }
        }
    }
}
