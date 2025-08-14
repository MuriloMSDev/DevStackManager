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
            CreateSidebar(mainWindow, contentGrid);

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

        /// <summary>
        /// Cria a sidebar de navegação
        /// </summary>
        private static void CreateSidebar(DevStackGui mainWindow, Grid contentGrid)
        {
            var sidebar = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.SidebarBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(sidebar, 0);

            // Container principal da sidebar com o título no topo
            var sidebarContainer = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Título no topo da sidebar com ícone
            var titlePanel = CreateSidebarTitle(mainWindow);
            sidebarContainer.Children.Add(titlePanel);

            // Separador sutil
            var separator = new Border
            {
                Height = 1,
                Margin = new Thickness(10, 0, 10, 10),
                Background = DevStackShared.ThemeManager.CurrentTheme.Border
            };
            sidebarContainer.Children.Add(separator);

            // Lista de navegação
            var navList = CreateNavigationList(mainWindow);
            sidebarContainer.Children.Add(navList);
            
            // Adicionar o container à sidebar
            sidebar.Child = sidebarContainer;
            contentGrid.Children.Add(sidebar);
        }

        /// <summary>
        /// Cria o título da sidebar com ícone
        /// </summary>
        private static StackPanel CreateSidebarTitle(DevStackGui mainWindow)
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

            // Tentar carregar o ícone com fallback para erro
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, mainWindow.LocalizationManager.GetString("gui.common.icon_file"));
                if (File.Exists(iconPath))
                {
                    iconImage.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                }
            }
            catch {}

            if (iconImage != null)
                titlePanel.Children.Add(iconImage);

            var sidebarTitleLabel = new Label
            {
                Content = mainWindow.LocalizationManager.GetString("gui.navigation.title"),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            titlePanel.Children.Add(sidebarTitleLabel);

            return titlePanel;
        }

        /// <summary>
        /// Cria a lista de navegação
        /// </summary>
        private static ListBox CreateNavigationList(DevStackGui mainWindow)
        {
            var navList = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = System.Windows.Media.Brushes.Transparent,
                Margin = new Thickness(8, 5, 8, 5),
                SelectedIndex = 0
            };

            // Criar itens de navegação com ícones modernos
            var navItems = new List<NavigationItem>
            {
                new() { Title = mainWindow.LocalizationManager.GetString("gui.navigation.items.installed.title"), Icon = "📦", Description = mainWindow.LocalizationManager.GetString("gui.navigation.items.installed.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.navigation.items.install.title"), Icon = "📥", Description = mainWindow.LocalizationManager.GetString("gui.navigation.items.install.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.navigation.items.uninstall.title"), Icon = "🗑️", Description = mainWindow.LocalizationManager.GetString("gui.navigation.items.uninstall.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.navigation.items.services.title"), Icon = "⚙️", Description = mainWindow.LocalizationManager.GetString("gui.navigation.items.services.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.navigation.items.config.title"), Icon = "🔧", Description = mainWindow.LocalizationManager.GetString("gui.navigation.items.config.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.navigation.items.sites.title"), Icon = "🌐", Description = mainWindow.LocalizationManager.GetString("gui.navigation.items.sites.description") },
                new() { Title = mainWindow.LocalizationManager.GetString("gui.navigation.items.utilities.title"), Icon = "🛠️", Description = mainWindow.LocalizationManager.GetString("gui.navigation.items.utilities.description") }
            };

            foreach (var item in navItems)
            {
                var listItem = new ListBoxItem();

                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                
                var iconLabel = new Label
                {
                    Content = item.Icon,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground
                };
                
                var textPanel = new StackPanel();
                var titleLabel = new Label
                {
                    Content = item.Title,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground
                };
                var descLabel = new Label
                {
                    Content = item.Description,
                    FontSize = 11,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = DevStackShared.ThemeManager.CurrentTheme.TextMuted
                };
                
                textPanel.Children.Add(titleLabel);
                textPanel.Children.Add(descLabel);
                
                panel.Children.Add(iconLabel);
                panel.Children.Add(textPanel);
                
                listItem.Content = panel;
                navList.Items.Add(listItem);
            }

            // Apply theme to the navigation list
            DevStackShared.ThemeManager.ApplySidebarListBoxTheme(navList);

            // Bind da seleção
            var binding = new Binding("SelectedNavIndex") { Source = mainWindow };
            navList.SetBinding(ListBox.SelectedIndexProperty, binding);

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
                    mainWindow._mainContent.Content = GuiInstalledTab.CreateInstalledContent(mainWindow);
                    break;
                case 1:
                    mainWindow._mainContent.Content = GuiInstallTab.CreateInstallContent(mainWindow);
                    break;
                case 2:
                    mainWindow._mainContent.Content = GuiUninstallTab.CreateUninstallContent(mainWindow);
                    break;
                case 3:
                    // Atualizar serviços automaticamente ao navegar para a aba
                    _ = GuiServicesTab.LoadServices(mainWindow);
                    mainWindow._mainContent.Content = GuiServicesTab.CreateServicesContent(mainWindow);
                    break;
                case 4:
                    mainWindow._mainContent.Content = GuiConfigTab.CreateConfigContent(mainWindow);
                    break;
                case 5:
                    mainWindow._mainContent.Content = GuiSitesTab.CreateSitesContent(mainWindow);
                    break;
                case 6:
                    mainWindow._mainContent.Content = GuiUtilitiesTab.CreateUtilitiesContent(mainWindow);
                    break;
                default:
                    mainWindow._mainContent.Content = GuiInstalledTab.CreateInstalledContent(mainWindow);
                    break;
            }
        }

        /// <summary>
        /// Cria a barra de status
        /// </summary>
        public static void CreateStatusBar(DevStackGui mainWindow, Grid mainGrid)
        {
            var statusBar = new Grid
            {
                Height = 35,
                Background = DevStackShared.ThemeManager.CurrentTheme.StatusBackground
            };
            Grid.SetRow(statusBar, 1);
            
            // Define columns: status message (left) and refresh button (right)
            statusBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Add a subtle top border to separate from content
            var topBorder = new Border
            {
                Height = 1,
                VerticalAlignment = VerticalAlignment.Top,
                Background = DevStackShared.ThemeManager.CurrentTheme.Border
            };
            Grid.SetColumnSpan(topBorder, 2);
            statusBar.Children.Add(topBorder);

            var statusLabel = new Label
            {
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.StatusForeground
            };
            Grid.SetColumn(statusLabel, 0);
            var statusBinding = new Binding("StatusMessage") { Source = mainWindow };
            statusLabel.SetBinding(Label.ContentProperty, statusBinding);
            
            // Create refresh button with only icon
            var refreshButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.refresh_icon"));
            refreshButton.Width = 45;
            refreshButton.Height = 35;
            refreshButton.FontSize = 14;
            refreshButton.Margin = new Thickness(0);
            refreshButton.VerticalAlignment = VerticalAlignment.Center;
            refreshButton.HorizontalAlignment = HorizontalAlignment.Right;
            refreshButton.ToolTip = mainWindow.LocalizationManager.GetString("gui.navigation.refresh_tooltip");
            refreshButton.Click += (s, e) => mainWindow.RefreshAllData();
            Grid.SetColumn(refreshButton, 1);
            
            statusBar.Children.Add(statusLabel);
            statusBar.Children.Add(refreshButton);
            mainGrid.Children.Add(statusBar);
        }
    }
}
