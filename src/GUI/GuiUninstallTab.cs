using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Uninstall tab component for removing installed development tools.
    /// Provides interface for selecting components and versions to uninstall with confirmation dialogs.
    /// Shows uninstall progress in dedicated console tab.
    /// </summary>
    public static class GuiUninstallTab
    {
        /// <summary>
        /// Font size for title text.
        /// </summary>
        private const int TITLE_FONT_SIZE = 18;
        
        /// <summary>
        /// Font size for buttons.
        /// </summary>
        private const int BUTTON_FONT_SIZE = 14;
        
        /// <summary>
        /// Height of combo boxes.
        /// </summary>
        private const int COMBO_HEIGHT = 30;
        
        /// <summary>
        /// Height of uninstall button.
        /// </summary>
        private const int UNINSTALL_BUTTON_HEIGHT = 40;
        
        /// <summary>
        /// Height of refresh button.
        /// </summary>
        private const int REFRESH_BUTTON_HEIGHT = 35;
        
        /// <summary>
        /// Standard panel margin.
        /// </summary>
        private const int PANEL_MARGIN = 10;
        
        /// <summary>
        /// Bottom margin for title element.
        /// </summary>
        private const int TITLE_BOTTOM_MARGIN = 20;
        
        /// <summary>
        /// Top margin for combo boxes.
        /// </summary>
        private const int COMBO_TOP_MARGIN = 5;
        
        /// <summary>
        /// Bottom margin for combo boxes.
        /// </summary>
        private const int COMBO_BOTTOM_MARGIN = 15;
        
        /// <summary>
        /// Bottom margin for version combo box.
        /// </summary>
        private const int VERSION_COMBO_BOTTOM_MARGIN = 20;
        
        /// <summary>
        /// Top margin for buttons.
        /// </summary>
        private const int BUTTON_TOP_MARGIN = 10;
        
        /// <summary>
        /// Top margin for warning panel.
        /// </summary>
        private const int WARNING_TOP_MARGIN = 20;
        
        /// <summary>
        /// Delay in milliseconds for UI initialization before loading data.
        /// </summary>
        private const int UI_INITIALIZATION_DELAY_MS = 100;

        /// <summary>
        /// Creates the complete Uninstall tab content with two-column layout.
        /// Left column: component/version selection panel with uninstall button.
        /// Right column: dedicated console output for uninstall operations.
        /// Loads installed components with 100ms delay to ensure UI initialization.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and component loading</param>
        /// <returns>Grid with uninstall interface and console output</returns>
        public static Grid CreateUninstallContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftPanel = CreateUninstallSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Uninstall);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            _ = Task.Run(async () =>
            {
                await Task.Delay(UI_INITIALIZATION_DELAY_MS);
                await mainWindow.Dispatcher.InvokeAsync(async () => await mainWindow.LoadUninstallComponents());
            });

            return grid;
        }

        /// <summary>
        /// Creates the selection panel for choosing components and versions to uninstall.
        /// Grid overlay structure allows loading spinner during uninstall operations.
        /// Sections: title, component ComboBox, version ComboBox, uninstall button, refresh button, warning notification.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and localization</param>
        /// <returns>Grid with selection controls and loading overlay</returns>
        private static UIElement CreateUninstallSelectionPanel(DevStackGui mainWindow)
        {
            var grid = new Grid();
            var panel = new StackPanel
            {
                Margin = new Thickness(PANEL_MARGIN)
            };

            var titleLabel = CreateTitleLabel(mainWindow);
            panel.Children.Add(titleLabel);

            var componentLabel = CreateComponentLabel(mainWindow);
            panel.Children.Add(componentLabel);

            var componentCombo = CreateComponentComboBox(mainWindow);
            panel.Children.Add(componentCombo);

            var versionLabel = CreateVersionLabel(mainWindow);
            panel.Children.Add(versionLabel);

            var versionCombo = CreateVersionComboBox(mainWindow);
            panel.Children.Add(versionCombo);

            var overlay = CreateLoadingOverlay(mainWindow);

            var uninstallButton = CreateUninstallButton(mainWindow, overlay);
            panel.Children.Add(uninstallButton);

            var refreshButton = CreateRefreshButton(mainWindow);
            panel.Children.Add(refreshButton);

            var warningPanel = CreateWarningPanel(mainWindow);
            panel.Children.Add(warningPanel);

            grid.Children.Add(panel);
            grid.Children.Add(overlay);

            return grid;
        }

        /// <summary>
        /// Creates the title label for the Uninstall tab.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>Styled label with "Uninstall" title</returns>
        private static Label CreateTitleLabel(DevStackGui mainWindow)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.title"), 
                true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_BOTTOM_MARGIN);
            return titleLabel;
        }

        /// <summary>
        /// Creates the component selection label.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>Styled label for component ComboBox</returns>
        private static Label CreateComponentLabel(DevStackGui mainWindow)
        {
            return DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_tool"));
        }

        /// <summary>
        /// Creates the component ComboBox for selecting which tool to uninstall.
        /// Binds to SelectedUninstallComponent property and uses NameToLabelConverter for display.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding</param>
        /// <returns>Styled ComboBox with component items</returns>
        private static ComboBox CreateComponentComboBox(DevStackGui mainWindow)
        {
            var componentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, COMBO_TOP_MARGIN, 0, COMBO_BOTTOM_MARGIN);
            componentCombo.Height = COMBO_HEIGHT;
            componentCombo.Name = "UninstallComponentCombo";
            
            var selectedUninstallComponentBinding = new Binding("SelectedUninstallComponent") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallComponentBinding);
            
            var uninstallItemTemplate = new DataTemplate();
            var uninstallTextFactory = new FrameworkElementFactory(typeof(TextBlock));
            uninstallTextFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new GuiInstallTab.NameToLabelConverter() });
            uninstallItemTemplate.VisualTree = uninstallTextFactory;
            componentCombo.ItemTemplate = uninstallItemTemplate;
            
            return componentCombo;
        }

        /// <summary>
        /// Creates the version selection label.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>Styled label for version ComboBox</returns>
        private static Label CreateVersionLabel(DevStackGui mainWindow)
        {
            return DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_version"));
        }

        /// <summary>
        /// Creates the version ComboBox for selecting which version to uninstall.
        /// Binds to SelectedUninstallVersion property, populated dynamically based on selected component.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding</param>
        /// <returns>Styled ComboBox with version items</returns>
        private static ComboBox CreateVersionComboBox(DevStackGui mainWindow)
        {
            var versionCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            versionCombo.Margin = new Thickness(0, COMBO_TOP_MARGIN, 0, VERSION_COMBO_BOTTOM_MARGIN);
            versionCombo.Height = COMBO_HEIGHT;
            versionCombo.Name = "UninstallVersionCombo";
            
            var selectedUninstallVersionBinding = new Binding("SelectedUninstallVersion") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallVersionBinding);
            
            return versionCombo;
        }

        /// <summary>
        /// Creates the loading overlay shown during uninstall operations.
        /// Visibility bound to IsUninstallingComponent property via PropertyChanged event.
        /// </summary>
        /// <param name="mainWindow">Main window instance for property binding</param>
        /// <returns>Border with loading spinner overlay</returns>
        private static Border CreateLoadingOverlay(DevStackGui mainWindow)
        {
            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
            overlay.Visibility = mainWindow.IsUninstallingComponent ? Visibility.Visible : Visibility.Collapsed;
            
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsUninstallingComponent))
                {
                    overlay.Visibility = mainWindow.IsUninstallingComponent ? Visibility.Visible : Visibility.Collapsed;
                }
            };
            
            return overlay;
        }

        /// <summary>
        /// Creates the uninstall button that triggers component removal.
        /// Uses Danger button style (typically red) to indicate destructive action.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and uninstall execution</param>
        /// <param name="overlay">Loading overlay to show during operation</param>
        /// <returns>Styled Danger button with async uninstall handler</returns>
        private static Button CreateUninstallButton(DevStackGui mainWindow, Border overlay)
        {
            var uninstallButton = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.buttons.uninstall"), 
                async (s, e) =>
                {
                    await ExecuteUninstallAction(mainWindow, overlay);
                }, 
                DevStackShared.ThemeManager.ButtonStyle.Danger);
            
            uninstallButton.Height = UNINSTALL_BUTTON_HEIGHT;
            uninstallButton.FontSize = BUTTON_FONT_SIZE;
            uninstallButton.Margin = new Thickness(0, BUTTON_TOP_MARGIN, 0, 0);
            
            return uninstallButton;
        }

        /// <summary>
        /// Executes the uninstall action with loading overlay and state management.
        /// Shows overlay, performs uninstall, resets selection, hides overlay in finally block.
        /// </summary>
        /// <param name="mainWindow">Main window instance for state management</param>
        /// <param name="overlay">Loading overlay to control visibility</param>
        /// <returns>Task representing the async uninstall operation</returns>
        private static async Task ExecuteUninstallAction(DevStackGui mainWindow, Border overlay)
        {
            mainWindow.IsUninstallingComponent = true;
            overlay.Visibility = Visibility.Visible;
            try
            {
                await UninstallComponent(mainWindow);
            }
            finally
            {
                ResetUninstallSelection(mainWindow);
                mainWindow.IsUninstallingComponent = false;
                overlay.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Resets uninstall component and version selections to empty strings after operation.
        /// </summary>
        /// <param name="mainWindow">Main window instance with selection properties</param>
        private static void ResetUninstallSelection(DevStackGui mainWindow)
        {
            mainWindow.SelectedUninstallComponent = "";
            mainWindow.SelectedUninstallVersion = "";
        }

        /// <summary>
        /// Creates the refresh button for reloading the uninstall components list.
        /// Calls mainWindow.LoadUninstallComponents() on click.
        /// </summary>
        /// <param name="mainWindow">Main window instance for component loading and localization</param>
        /// <returns>Styled button with async refresh handler</returns>
        private static Button CreateRefreshButton(DevStackGui mainWindow)
        {
            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.buttons.refresh"), 
                async (s, e) => await mainWindow.LoadUninstallComponents());
            
            refreshButton.Height = REFRESH_BUTTON_HEIGHT;
            refreshButton.Margin = new Thickness(0, BUTTON_TOP_MARGIN, 0, 0);
            
            return refreshButton;
        }

        /// <summary>
        /// Creates the warning notification panel shown at bottom of uninstall tab.
        /// Warns users about uninstall action consequences.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>Warning notification panel</returns>
        private static UIElement CreateWarningPanel(DevStackGui mainWindow)
        {
            var warningPanel = DevStackShared.ThemeManager.CreateNotificationPanel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.warning"), 
                DevStackShared.ThemeManager.NotificationType.Warning);
            
            warningPanel.Margin = new Thickness(0, WARNING_TOP_MARGIN, 0, 0);
            
            return warningPanel;
        }

        /// <summary>
        /// Uninstalls the selected component with validation and confirmation.
        /// Workflow: validate inputs → confirm with user → perform uninstall via UninstallManager → refresh lists.
        /// </summary>
        /// <param name="mainWindow">Main window instance for selections, status, and component loading</param>
        /// <returns>Task representing the async uninstall operation</returns>
        public static async Task UninstallComponent(DevStackGui mainWindow)
        {
            if (!ValidateUninstallInputs(mainWindow))
            {
                return;
            }

            if (!ConfirmUninstallation(mainWindow))
            {
                return;
            }

            await PerformUninstall(mainWindow);
        }

        /// <summary>
        /// Validates that component and optionally version are selected before uninstall.
        /// Shows warning dialogs if required selections are missing.
        /// </summary>
        /// <param name="mainWindow">Main window instance for selections and localization</param>
        /// <returns>True if inputs are valid, false otherwise</returns>
        private static bool ValidateUninstallInputs(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedUninstallComponent))
            {
                ShowComponentRequiredWarning(mainWindow);
                return false;
            }

            if (string.IsNullOrEmpty(mainWindow.SelectedUninstallVersion))
            {
                ShowVersionRequiredWarning(mainWindow);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows a warning dialog indicating that a component must be selected.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        private static void ShowComponentRequiredWarning(DevStackGui mainWindow)
        {
            DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_component"), 
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }

        /// <summary>
        /// Shows a warning dialog indicating that a version must be selected.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        private static void ShowVersionRequiredWarning(DevStackGui mainWindow)
        {
            DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_version"), 
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }

        /// <summary>
        /// Shows a confirmation dialog asking user to confirm uninstallation.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and component name</param>
        /// <returns>True if user clicked Yes, false if No</returns>
        private static bool ConfirmUninstallation(DevStackGui mainWindow)
        {
            var result = DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.confirm", mainWindow.SelectedUninstallComponent),
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.confirmation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Performs the actual uninstall operation via UninstallManager.
        /// Runs with console output progress reporting and handles success/error states.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status updates and component reloading</param>
        /// <returns>Task representing the async uninstall operation</returns>
        private static async Task PerformUninstall(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.uninstalling", 
                mainWindow.SelectedUninstallComponent);
            
            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Uninstall, async progress =>
            {
                try
                {
                    var args = BuildUninstallArgs(mainWindow);
                    UninstallManager.UninstallCommands(args);

                    await HandleSuccessfulUninstall(mainWindow);
                }
                catch (Exception ex)
                {
                    HandleUninstallError(mainWindow, progress, ex);
                }
            });
        }

        /// <summary>
        /// Builds the command-line arguments array for UninstallManager.
        /// If version is selected, includes both component and version; otherwise just component.
        /// </summary>
        /// <param name="mainWindow">Main window instance for component and version selections</param>
        /// <returns>String array with uninstall arguments</returns>
        private static string[] BuildUninstallArgs(DevStackGui mainWindow)
        {
            return string.IsNullOrEmpty(mainWindow.SelectedUninstallVersion)
                ? new[] { mainWindow.SelectedUninstallComponent }
                : new[] { mainWindow.SelectedUninstallComponent, mainWindow.SelectedUninstallVersion };
        }

        /// <summary>
        /// Handles successful uninstall by updating status and reloading component/version lists.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status and list reloading</param>
        /// <returns>Task representing the async reload operations</returns>
        private static async Task HandleSuccessfulUninstall(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.success", 
                mainWindow.SelectedUninstallComponent);

            await mainWindow.LoadUninstallComponents();
            await mainWindow.LoadUninstallVersions();
        }

        /// <summary>
        /// Handles errors that occur during uninstall operation.
        /// Reports error to progress console, updates status bar, and logs error details.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status and localization</param>
        /// <param name="progress">Progress reporter for console output</param>
        /// <param name="ex">Exception that occurred</param>
        private static void HandleUninstallError(DevStackGui mainWindow, IProgress<string> progress, Exception ex)
        {
            progress.Report(mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.error", 
                mainWindow.SelectedUninstallComponent, 
                ex.Message));
            
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.error_short", 
                mainWindow.SelectedUninstallComponent);
            
            DevStackConfig.WriteLog($"GUI uninstall error for {mainWindow.SelectedUninstallComponent}: {ex}");
        }

    }
}
