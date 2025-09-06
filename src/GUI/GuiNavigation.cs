using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela navegação principal e layout da interface
    /// </summary>
    public static class GuiNavigation
    {
        /// <summary>
        /// Classe para representar itens de navegação
        /// </summary>
        public class NavigationItem
        {
            public string Title { get; set; } = "";
            public string Icon { get; set; } = "";
            public string Description { get; set; } = "";
        }

        /// <summary>
        /// Cria o conteúdo principal com sidebar
        /// </summary>
        public static void CreateMainContent(DevStackGui mainWindow, Grid mainGrid)
        {
            var contentGrid = new Grid { Margin = new Thickness(0) };
            Grid.SetRow(contentGrid, 0);

            // Definir colunas: Sidebar | Content
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) }); // Sidebar
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Content

            // Criar sidebar
            var sidebar = new Border
            {
                Background = mainWindow.CurrentTheme.SidebarBackground,
                BorderBrush = mainWindow.CurrentTheme.Border,
                BorderThickness = new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(sidebar, 0);

            var sidebarContainer = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Título e ícone
            sidebarContainer.Children.Add(CreateSidebarTitleUnified(mainWindow));
            // Separador
            sidebarContainer.Children.Add(CreateSeparatorUnified(mainWindow));
            // Lista de navegação
            sidebarContainer.Children.Add(CreateNavigationListUnified(mainWindow));

            sidebar.Child = sidebarContainer;
            contentGrid.Children.Add(sidebar);

            // Criar área de conteúdo principal
            mainWindow._mainContent = new ContentControl
            {
                Margin = new Thickness(10)
            };
            Grid.SetColumn(mainWindow._mainContent, 1);
            contentGrid.Children.Add(mainWindow._mainContent);

            // Navegar para a primeira seção por padrão
            NavigateToSection(mainWindow, 0);

            mainGrid.Children.Add(contentGrid);
        }

        // Métodos unificados da sidebar
        private static StackPanel CreateSidebarTitleUnified(DevStackGui mainWindow)
        {
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 15, 5, 10)
            };

            // Ícone DevStack
            var iconImage = new Image
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, 6, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DevStack.ico");
                if (File.Exists(iconPath))
                {
                    iconImage.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                    titlePanel.Children.Add(iconImage);
                }
            }
            catch { }

            var sidebarTitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sidebar.title"));
            sidebarTitleLabel.FontSize = 20;
            sidebarTitleLabel.FontWeight = FontWeights.Bold;
            sidebarTitleLabel.HorizontalAlignment = HorizontalAlignment.Left;
            sidebarTitleLabel.VerticalAlignment = VerticalAlignment.Center;
            sidebarTitleLabel.Margin = new Thickness(0);
            sidebarTitleLabel.Padding = new Thickness(0);
            titlePanel.Children.Add(sidebarTitleLabel);

            return titlePanel;
        }

        private static Border CreateSeparatorUnified(DevStackGui mainWindow)
        {
            return new Border
            {
                Height = 1,
                Margin = new Thickness(10, 0, 10, 10),
                Background = mainWindow.CurrentTheme.Border
            };
        }

        private static ListBox CreateNavigationListUnified(DevStackGui mainWindow)
        {
            var navList = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = System.Windows.Media.Brushes.Transparent,
                Margin = new Thickness(8, 5, 8, 5),
                SelectedIndex = 0
            };

            var navItems = new List<NavigationItem>
            {
                new() { Title = "Dashboard", Icon = "📊", Description = "Visão geral do sistema" },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.installed.title"), Icon = "📦", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.installed.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.install.title"), Icon = "📥", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.install.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.uninstall.title"), Icon = "🗑️", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.uninstall.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.services.title"), Icon = "⚙️", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.services.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.sites.title"), Icon = "🌐", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.sites.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.utilities.title"), Icon = "🛠️", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.utilities.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.config.title"), Icon = "🔧", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.config.description") }
            };

            foreach (var item in navItems)
            {
                var listItem = new ListBoxItem();
                listItem.ClearValue(ListBoxItem.StyleProperty);
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                var iconLabel = new Label
                {
                    Content = item.Icon,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = mainWindow.CurrentTheme.Foreground,
                    Background = System.Windows.Media.Brushes.Transparent
                };
                var titleLabel = new Label
                {
                    Content = item.Title,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = mainWindow.CurrentTheme.Foreground,
                    Background = System.Windows.Media.Brushes.Transparent
                };
                panel.Children.Add(iconLabel);
                panel.Children.Add(titleLabel);
                listItem.Content = panel;
                navList.Items.Add(listItem);
            }

            // Bind da seleção
            var binding = new Binding("SelectedNavIndex") { Source = mainWindow };
            navList.SetBinding(ListBox.SelectedIndexProperty, binding);

            // Apply theme to the navigation list
            DevStackShared.ThemeManager.ApplySidebarListBoxTheme(navList);
            navList.UpdateLayout();
            return navList;
        }

        /// <summary>
        /// Navega para uma seção específica baseada no índice
        /// </summary>
        public static void NavigateToSection(DevStackGui mainWindow, int index)
        {
            if (mainWindow._mainContent == null) return;

            switch (index)
            {
                case 0:
                    // Dashboard - renderizar imediatamente e carregar dados em background
                    mainWindow._mainContent.Content = GuiDashboardTab.CreateDashboardContent(mainWindow);
                    
                    // Carregar dados em background
                    _ = System.Threading.Tasks.Task.Run(async () =>
                    {
                        try
                        {
                            await mainWindow.LoadInstalledComponents();
                            await mainWindow.LoadServices();
                        }
                        catch (Exception ex)
                        {
                            mainWindow.Dispatcher.Invoke(() =>
                            {
                                mainWindow.StatusMessage = $"Erro ao carregar dados do Dashboard: {ex.Message}";
                            });
                        }
                    });
                    break;
                case 1:
                    // Instalados - carregar dados se necessário
                    if (mainWindow.InstalledComponents?.Count == 0)
                    {
                        _ = mainWindow.LoadInstalledComponents();
                    }
                    mainWindow._mainContent.Content = GuiInstalledTab.CreateInstalledContent(mainWindow);
                    break;
                case 2:
                    // Instalar - carregar componentes disponíveis e componentes para shortcuts se necessário
                    if (mainWindow.AvailableComponents?.Count == 0)
                    {
                        _ = mainWindow.LoadAvailableComponents();
                    }
                    if (mainWindow.ShortcutComponents?.Count == 0)
                    {
                        _ = mainWindow.LoadShortcutComponents();
                    }
                    mainWindow._mainContent.Content = GuiInstallTab.CreateInstallContent(mainWindow);
                    break;
                case 3:
                    mainWindow._mainContent.Content = GuiUninstallTab.CreateUninstallContent(mainWindow);
                    break;
                case 4:
                    mainWindow._mainContent.Content = GuiServicesTab.CreateServicesContent(mainWindow);
                    break;
                case 5:
                    mainWindow._mainContent.Content = GuiSitesTab.CreateSitesContent(mainWindow);
                    break;
                case 6:
                    mainWindow._mainContent.Content = GuiUtilitiesTab.CreateUtilitiesContent(mainWindow);
                    break;
                case 7:
                    mainWindow._mainContent.Content = GuiConfigTab.CreateConfigContent(mainWindow);
                    break;
                default:
                    mainWindow._mainContent.Content = GuiDashboardTab.CreateDashboardContent(mainWindow);
                    break;
            }
        }
    }
}
