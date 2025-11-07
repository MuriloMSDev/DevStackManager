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
                await Task.Delay(100); // Pequeno delay para garantir que a UI esteja construída
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
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.title"), true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Componente
            var componentLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_tool"));
            panel.Children.Add(componentLabel);

            var componentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, 5, 0, 15);
            componentCombo.Height = 30;
            componentCombo.Name = "UninstallComponentCombo";
            
            var selectedUninstallComponentBinding = new Binding("SelectedUninstallComponent") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallComponentBinding);
            // Exibir Label para cada item preenchido dinamicamente depois (Items.Add(Name))
            var uninstallItemTemplate = new DataTemplate();
            var uninstallTextFactory = new FrameworkElementFactory(typeof(TextBlock));
            uninstallTextFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new GuiInstallTab.NameToLabelConverter() });
            uninstallItemTemplate.VisualTree = uninstallTextFactory;
            componentCombo.ItemTemplate = uninstallItemTemplate;
            panel.Children.Add(componentCombo);

            // Versão
            var versionLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.labels.select_version"));
            panel.Children.Add(versionLabel);

            var versionCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            versionCombo.Margin = new Thickness(0, 5, 0, 20);
            versionCombo.Height = 30;
            versionCombo.Name = "UninstallVersionCombo";
            
            var selectedUninstallVersionBinding = new Binding("SelectedUninstallVersion") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallVersionBinding);
            panel.Children.Add(versionCombo);

            // Overlay de loading (spinner)
            var overlay = DevStackShared.ThemeManager.CreateLoadingOverlay();
            // Overlay sempre visível se desinstalando
            overlay.Visibility = mainWindow.IsUninstallingComponent ? Visibility.Visible : Visibility.Collapsed;
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsUninstallingComponent))
                {
                    overlay.Visibility = mainWindow.IsUninstallingComponent ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            // Botão Desinstalar
            var uninstallButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.buttons.uninstall"), async (s, e) =>
            {
                mainWindow.IsUninstallingComponent = true;
                overlay.Visibility = Visibility.Visible;
                try
                {
                    await UninstallComponent(mainWindow);
                }
                finally
                {
                    mainWindow.SelectedUninstallComponent = "";
                    mainWindow.SelectedUninstallVersion = "";
                    mainWindow.IsUninstallingComponent = false;
                    overlay.Visibility = Visibility.Collapsed;
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Danger);
            uninstallButton.Height = 40;
            uninstallButton.FontSize = 14;
            uninstallButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(uninstallButton);

            // Botão Atualizar Lista
            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.buttons.refresh"), async (s, e) => await mainWindow.LoadUninstallComponents());
            refreshButton.Height = 35;
            refreshButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(refreshButton);

            // Warning usando painel de notificação
            var warningPanel = DevStackShared.ThemeManager.CreateNotificationPanel(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.warning"), 
                DevStackShared.ThemeManager.NotificationType.Warning
            );
            warningPanel.Margin = new Thickness(0, 20, 0, 0);
            panel.Children.Add(warningPanel);

            // Adiciona painel e overlay ao grid
            grid.Children.Add(panel);
            grid.Children.Add(overlay);

            return grid;
        }

        /// <summary>
        /// Desinstala o componente selecionado
        /// </summary>
        public static async Task UninstallComponent(DevStackGui mainWindow)
        {
            if (string.IsNullOrEmpty(mainWindow.SelectedUninstallComponent))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_component"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(mainWindow.SelectedUninstallVersion))
            {
                DevStackShared.ThemeManager.CreateStyledMessageBox(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.select_version"), mainWindow.LocalizationManager.GetString("gui.common.dialogs.warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = DevStackShared.ThemeManager.CreateStyledMessageBox(
                mainWindow.LocalizationManager.GetString("gui.uninstall_tab.messages.confirm", mainWindow.SelectedUninstallComponent),
                mainWindow.LocalizationManager.GetString("gui.common.dialogs.confirmation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.uninstalling", mainWindow.SelectedUninstallComponent);
            
            await GuiConsolePanel.RunWithConsoleOutput(GuiConsolePanel.ConsoleTab.Uninstall, mainWindow, async progress =>
            {
                try
                {
                    var args = string.IsNullOrEmpty(mainWindow.SelectedUninstallVersion)
                        ? new[] { mainWindow.SelectedUninstallComponent }
                        : new[] { mainWindow.SelectedUninstallComponent, mainWindow.SelectedUninstallVersion };

                    UninstallManager.UninstallCommands(args);

                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.success", mainWindow.SelectedUninstallComponent);

                    // Recarregar componentes disponíveis para desinstalação
                    await mainWindow.LoadUninstallComponents();
                    await mainWindow.LoadUninstallVersions();
                }
                catch (Exception ex)
                {
                    progress.Report(mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.error", mainWindow.SelectedUninstallComponent, ex.Message));
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.uninstall_tab.status.error_short", mainWindow.SelectedUninstallComponent);
                    DevStackConfig.WriteLog($"Erro ao desinstalar {mainWindow.SelectedUninstallComponent} na GUI: {ex}");
                }
            });
        }

    }
}
