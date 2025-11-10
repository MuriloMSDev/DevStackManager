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
    /// Componente responsável pela aba "Desinstalar" - remove ferramentas instaladas
    /// </summary>
    public static class GuiUninstallTab
    {
        // UI Dimensions Constants
        private const int TITLE_FONT_SIZE = 18;
        private const int BUTTON_FONT_SIZE = 14;
        private const int COMBO_HEIGHT = 30;
        private const int UNINSTALL_BUTTON_HEIGHT = 40;
        private const int REFRESH_BUTTON_HEIGHT = 35;
        private const int PANEL_MARGIN = 10;
        private const int TITLE_BOTTOM_MARGIN = 20;
        private const int COMBO_TOP_MARGIN = 5;
        private const int COMBO_BOTTOM_MARGIN = 15;
        private const int VERSION_COMBO_BOTTOM_MARGIN = 20;
        private const int BUTTON_TOP_MARGIN = 10;
        private const int WARNING_TOP_MARGIN = 20;
        
        // Timing Constants
        private const int UI_INITIALIZATION_DELAY_MS = 100;

        /// <summary>
        /// Cria o conteúdo completo da aba "Desinstalar"
        /// </summary>
        public static Grid CreateUninstallContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Seleção
            var leftPanel = CreateUninstallSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);


            // Painel direito - Console dedicado da aba Uninstall
            var rightPanel = GuiConsolePanel.CreateConsolePanel(GuiConsolePanel.ConsoleTab.Uninstall);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            // Carregar componentes instalados ao criar a aba (com delay para garantir que UI esteja pronta)
            _ = Task.Run(async () =>
            {
                await Task.Delay(UI_INITIALIZATION_DELAY_MS);
                await mainWindow.Dispatcher.InvokeAsync(async () => await mainWindow.LoadUninstallComponents());
            });

            return grid;
        }

        /// <summary>
        /// Cria o painel de seleção de componentes para desinstalação
        /// </summary>
        private static UIElement CreateUninstallSelectionPanel(DevStackGui mainWindow)
        {
            // Usar Grid para permitir overlay
            var grid = new Grid();
            var panel = new StackPanel
            {
                Margin = new Thickness(PANEL_MARGIN)
            };

            // Título
            var titleLabel = CreateTitleLabel(mainWindow);
            panel.Children.Add(titleLabel);

            // Componente
            var componentLabel = CreateComponentLabel(mainWindow);
            panel.Children.Add(componentLabel);

            var componentCombo = CreateComponentComboBox(mainWindow);
            panel.Children.Add(componentCombo);

            // Versão
            var versionLabel = CreateVersionLabel(mainWindow);
            panel.Children.Add(versionLabel);

            var versionCombo = CreateVersionComboBox(mainWindow);
            panel.Children.Add(versionCombo);

            // Overlay de loading (spinner)
            var overlay = CreateLoadingOverlay(mainWindow);

            // Botão Desinstalar
            var uninstallButton = CreateUninstallButton(mainWindow, overlay);
            panel.Children.Add(uninstallButton);

            // Botão Atualizar Lista
            var refreshButton = CreateRefreshButton(mainWindow);
            panel.Children.Add(refreshButton);

            // Warning usando painel de notificação
            var warningPanel = CreateWarningPanel(mainWindow);
            panel.Children.Add(warningPanel);

            // Adiciona painel e overlay ao grid
            grid.Children.Add(panel);
            grid.Children.Add(overlay);

            return grid;
        }

        private static Label CreateTitleLabel(DevStackGui mainWindow)
        {
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.title"), 
                true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_BOTTOM_MARGIN);
            return titleLabel;
        }

        private static Label CreateComponentLabel(DevStackGui mainWindow)
        {
            return DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_tool"));
        }

        private static ComboBox CreateComponentComboBox(DevStackGui mainWindow)
        {
            var componentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, COMBO_TOP_MARGIN, 0, COMBO_BOTTOM_MARGIN);
            componentCombo.Height = COMBO_HEIGHT;
            componentCombo.Name = "UninstallComponentCombo";
            
            var selectedUninstallComponentBinding = new Binding("SelectedUninstallComponent") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallComponentBinding);
            
            // Exibir Label para cada item preenchido dinamicamente depois (Items.Add(Name))
            var uninstallItemTemplate = new DataTemplate();
            var uninstallTextFactory = new FrameworkElementFactory(typeof(TextBlock));
            uninstallTextFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new GuiInstallTab.NameToLabelConverter() });
            uninstallItemTemplate.VisualTree = uninstallTextFactory;
            componentCombo.ItemTemplate = uninstallItemTemplate;
            
            return componentCombo;
        }

        private static Label CreateVersionLabel(DevStackGui mainWindow)
        {
            return DevStackShared.ThemeManager.CreateStyledLabel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_version"));
        }

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

        private static void ResetUninstallSelection(DevStackGui mainWindow)
        {
            mainWindow.SelectedUninstallComponent = "";
            mainWindow.SelectedUninstallVersion = "";
        }

        private static Button CreateRefreshButton(DevStackGui mainWindow)
        {
            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.buttons.refresh"), 
                async (s, e) => await mainWindow.LoadUninstallComponents());
            
            refreshButton.Height = REFRESH_BUTTON_HEIGHT;
            refreshButton.Margin = new Thickness(0, BUTTON_TOP_MARGIN, 0, 0);
            
            return refreshButton;
        }

        private static UIElement CreateWarningPanel(DevStackGui mainWindow)
        {
            var warningPanel = DevStackShared.ThemeManager.CreateNotificationPanel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.warning"), 
                DevStackShared.ThemeManager.NotificationType.Warning);
            
            warningPanel.Margin = new Thickness(0, WARNING_TOP_MARGIN, 0, 0);
            
            return warningPanel;
        }

        /// <summary>
        /// Desinstala o componente selecionado
        /// </summary>
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

        private static void ShowComponentRequiredWarning(DevStackGui mainWindow)
        {
            DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_component"), 
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }

        private static void ShowVersionRequiredWarning(DevStackGui mainWindow)
        {
            DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_version"), 
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }

        private static bool ConfirmUninstallation(DevStackGui mainWindow)
        {
            var result = DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.confirm", mainWindow.SelectedUninstallComponent),
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.confirmation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        private static async Task PerformUninstall(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.uninstalling", 
                mainWindow.SelectedUninstallComponent);
            
            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Uninstall, mainWindow, async progress =>
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

        private static string[] BuildUninstallArgs(DevStackGui mainWindow)
        {
            return string.IsNullOrEmpty(mainWindow.SelectedUninstallVersion)
                ? new[] { mainWindow.SelectedUninstallComponent }
                : new[] { mainWindow.SelectedUninstallComponent, mainWindow.SelectedUninstallVersion };
        }

        private static async Task HandleSuccessfulUninstall(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.success", 
                mainWindow.SelectedUninstallComponent);

            await mainWindow.LoadUninstallComponents();
            await mainWindow.LoadUninstallVersions();
        }

        private static void HandleUninstallError(DevStackGui mainWindow, IProgress<string> progress, Exception ex)
        {
            progress.Report(mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.error", 
                mainWindow.SelectedUninstallComponent, 
                ex.Message));
            
            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString(
                "gui.uninstall_tab.status.error_short", 
                mainWindow.SelectedUninstallComponent);
            
            DevStackConfig.WriteLog($"Erro ao desinstalar {mainWindow.SelectedUninstallComponent} na GUI: {ex}");
        }

    }
}
