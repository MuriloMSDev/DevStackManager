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
        // UI Layout Constants
        private const double SIDEBAR_WIDTH = 250;
        private const double CONTENT_MARGIN = 10;
        
        // Icon and Title Constants
        private const double ICON_SIZE = 50;
        private const double TITLE_FONT_SIZE = 20;
        private const double NAV_ICON_FONT_SIZE = 18;
        private const double NAV_TITLE_FONT_SIZE = 14;
        
        // Margin Constants
        private const double TITLE_PANEL_MARGIN_HORIZONTAL = 5;
        private const double TITLE_PANEL_MARGIN_TOP = 15;
        private const double TITLE_PANEL_MARGIN_BOTTOM = 10;
        private const double ICON_MARGIN_TOP = 6;
        private const double SEPARATOR_MARGIN_HORIZONTAL = 10;
        private const double SEPARATOR_MARGIN_BOTTOM = 10;
        private const double NAV_LIST_MARGIN = 8;
        private const double NAV_LIST_MARGIN_VERTICAL = 5;
        private const double NAV_ICON_MARGIN_RIGHT = 12;
        
        // Separator Constants
        private const double SEPARATOR_HEIGHT = 1;
        
        // Border Constants
        private const double SIDEBAR_BORDER_RIGHT = 1;
        
        // Navigation Indices
        private const int NAV_INDEX_DASHBOARD = 0;
        private const int NAV_INDEX_INSTALLED = 1;
        private const int NAV_INDEX_INSTALL = 2;
        private const int NAV_INDEX_UNINSTALL = 3;
        private const int NAV_INDEX_SERVICES = 4;
        private const int NAV_INDEX_SITES = 5;
        private const int NAV_INDEX_UTILITIES = 6;
        private const int NAV_INDEX_CONFIG = 7;
        
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
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SIDEBAR_WIDTH) }); // Sidebar
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Content

            // Criar sidebar
            var sidebar = CreateSidebar(mainWindow);
            Grid.SetColumn(sidebar, 0);
            contentGrid.Children.Add(sidebar);

            // Criar área de conteúdo principal
            mainWindow._mainContent = new ContentControl
            {
                Margin = new Thickness(CONTENT_MARGIN)
            };
            Grid.SetColumn(mainWindow._mainContent, 1);
            contentGrid.Children.Add(mainWindow._mainContent);

            // Navegar para a primeira seção por padrão
            NavigateToSection(mainWindow, NAV_INDEX_DASHBOARD);

            mainGrid.Children.Add(contentGrid);
        }

        /// <summary>
        /// Cria a sidebar com título, separador e navegação
        /// </summary>
        private static Border CreateSidebar(DevStackGui mainWindow)
        {
            var sidebar = new Border
            {
                Background = mainWindow.CurrentTheme.SidebarBackground,
                BorderBrush = mainWindow.CurrentTheme.Border,
                BorderThickness = new Thickness(0, 0, SIDEBAR_BORDER_RIGHT, 0)
            };

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
            return sidebar;
        }

        // Métodos unificados da sidebar
        private static StackPanel CreateSidebarTitleUnified(DevStackGui mainWindow)
        {
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(TITLE_PANEL_MARGIN_HORIZONTAL, TITLE_PANEL_MARGIN_TOP, TITLE_PANEL_MARGIN_HORIZONTAL, TITLE_PANEL_MARGIN_BOTTOM)
            };

            // Ícone DevStack
            var iconImage = CreateDevStackIcon();
            if (iconImage != null)
            {
                titlePanel.Children.Add(iconImage);
            }

            var sidebarTitleLabel = CreateTitleLabel(mainWindow);
            titlePanel.Children.Add(sidebarTitleLabel);

            return titlePanel;
        }

        /// <summary>
        /// Cria o ícone do DevStack
        /// </summary>
        private static Image? CreateDevStackIcon()
        {
            var iconImage = new Image
            {
                Width = ICON_SIZE,
                Height = ICON_SIZE,
                Margin = new Thickness(0, ICON_MARGIN_TOP, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DevStack.ico");
                if (File.Exists(iconPath))
                {
                    iconImage.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                    return iconImage;
                }
            }
            catch { }
            
            return null;
        }

        /// <summary>
        /// Cria o label do título da sidebar
        /// </summary>
        private static Label CreateTitleLabel(DevStackGui mainWindow)
        {
            var sidebarTitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.sidebar.title"));
            sidebarTitleLabel.FontSize = TITLE_FONT_SIZE;
            sidebarTitleLabel.FontWeight = FontWeights.Bold;
            sidebarTitleLabel.HorizontalAlignment = HorizontalAlignment.Left;
            sidebarTitleLabel.VerticalAlignment = VerticalAlignment.Center;
            sidebarTitleLabel.Margin = new Thickness(0);
            sidebarTitleLabel.Padding = new Thickness(0);
            return sidebarTitleLabel;
        }

        private static Border CreateSeparatorUnified(DevStackGui mainWindow)
        {
            return new Border
            {
                Height = SEPARATOR_HEIGHT,
                Margin = new Thickness(SEPARATOR_MARGIN_HORIZONTAL, 0, SEPARATOR_MARGIN_HORIZONTAL, SEPARATOR_MARGIN_BOTTOM),
                Background = mainWindow.CurrentTheme.Border
            };
        }

        private static ListBox CreateNavigationListUnified(DevStackGui mainWindow)
        {
            var navList = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = System.Windows.Media.Brushes.Transparent,
                Margin = new Thickness(NAV_LIST_MARGIN, NAV_LIST_MARGIN_VERTICAL, NAV_LIST_MARGIN, NAV_LIST_MARGIN_VERTICAL),
                SelectedIndex = NAV_INDEX_DASHBOARD
            };

            var navItems = CreateNavigationItems(mainWindow);

            foreach (var item in navItems)
            {
                var listItem = CreateNavigationListItem(mainWindow, item);
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
        /// Cria a lista de itens de navegação
        /// </summary>
        private static List<NavigationItem> CreateNavigationItems(DevStackGui mainWindow)
        {
            return new List<NavigationItem>
            {
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.dashboard.title"), Icon = "📊", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.dashboard.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.installed.title"), Icon = "📦", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.installed.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.install.title"), Icon = "📥", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.install.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.uninstall.title"), Icon = "🗑️", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.uninstall.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.services.title"), Icon = "⚙️", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.services.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.sites.title"), Icon = "🌐", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.sites.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.utilities.title"), Icon = "🛠️", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.utilities.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.config.title"), Icon = "🔧", Description = mainWindow.LocalizationManager.GetString("gui.sidebar.navigation_items.config.description") }
            };
        }

        /// <summary>
        /// Cria um item de navegação individual
        /// </summary>
        private static ListBoxItem CreateNavigationListItem(DevStackGui mainWindow, NavigationItem item)
        {
            var listItem = new ListBoxItem();
            listItem.ClearValue(ListBoxItem.StyleProperty);
            
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            
            var iconLabel = CreateNavigationIconLabel(mainWindow, item.Icon);
            var titleLabel = CreateNavigationTitleLabel(mainWindow, item.Title);
            
            panel.Children.Add(iconLabel);
            panel.Children.Add(titleLabel);
            listItem.Content = panel;
            
            return listItem;
        }

        /// <summary>
        /// Cria o label do ícone de navegação
        /// </summary>
        private static Label CreateNavigationIconLabel(DevStackGui mainWindow, string icon)
        {
            return new Label
            {
                Content = icon,
                FontSize = NAV_ICON_FONT_SIZE,
                Margin = new Thickness(0, 0, NAV_ICON_MARGIN_RIGHT, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = mainWindow.CurrentTheme.Foreground,
                Background = System.Windows.Media.Brushes.Transparent
            };
        }

        /// <summary>
        /// Cria o label do título de navegação
        /// </summary>
        private static Label CreateNavigationTitleLabel(DevStackGui mainWindow, string title)
        {
            return new Label
            {
                Content = title,
                FontWeight = FontWeights.SemiBold,
                FontSize = NAV_TITLE_FONT_SIZE,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = mainWindow.CurrentTheme.Foreground,
                Background = System.Windows.Media.Brushes.Transparent
            };
        }

        /// <summary>
        /// Navega para uma seção específica baseada no índice
        /// </summary>
        public static void NavigateToSection(DevStackGui mainWindow, int index)
        {
            if (mainWindow._mainContent == null) return;

            switch (index)
            {
                case NAV_INDEX_DASHBOARD:
                    NavigateToDashboard(mainWindow);
                    break;
                case NAV_INDEX_INSTALLED:
                    NavigateToInstalled(mainWindow);
                    break;
                case NAV_INDEX_INSTALL:
                    NavigateToInstall(mainWindow);
                    break;
                case NAV_INDEX_UNINSTALL:
                    mainWindow._mainContent.Content = GuiUninstallTab.CreateUninstallContent(mainWindow);
                    break;
                case NAV_INDEX_SERVICES:
                    mainWindow._mainContent.Content = GuiServicesTab.CreateServicesContent(mainWindow);
                    break;
                case NAV_INDEX_SITES:
                    mainWindow._mainContent.Content = GuiSitesTab.CreateSitesContent(mainWindow);
                    break;
                case NAV_INDEX_UTILITIES:
                    mainWindow._mainContent.Content = GuiUtilitiesTab.CreateUtilitiesContent(mainWindow);
                    break;
                case NAV_INDEX_CONFIG:
                    mainWindow._mainContent.Content = GuiConfigTab.CreateConfigContent(mainWindow);
                    break;
                default:
                    mainWindow._mainContent.Content = GuiDashboardTab.CreateDashboardContent(mainWindow);
                    break;
            }
        }

        /// <summary>
        /// Navega para o Dashboard
        /// </summary>
        private static void NavigateToDashboard(DevStackGui mainWindow)
        {
            if (mainWindow._mainContent == null) return;
            
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
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.status_bar.error_loading_dashboard", ex.Message);
                    });
                }
            });
        }

        /// <summary>
        /// Navega para a aba Instalados
        /// </summary>
        private static void NavigateToInstalled(DevStackGui mainWindow)
        {
            if (mainWindow._mainContent == null) return;
            
            // Instalados - carregar dados se necessário
            if (mainWindow.InstalledComponents?.Count == 0)
            {
                _ = mainWindow.LoadInstalledComponents();
            }
            mainWindow._mainContent.Content = GuiInstalledTab.CreateInstalledContent(mainWindow);
        }

        /// <summary>
        /// Navega para a aba Instalar
        /// </summary>
        private static void NavigateToInstall(DevStackGui mainWindow)
        {
            if (mainWindow._mainContent == null) return;
            
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
        }
    }
}
