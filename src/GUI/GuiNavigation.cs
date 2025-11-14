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
    /// Navigation component responsible for main interface layout and navigation.
    /// Manages sidebar creation, navigation items, and section routing.
    /// Provides navigation to Dashboard, Installed, Install, Uninstall, Services, Sites, Utilities, and Config sections.
    /// </summary>
    public static class GuiNavigation
    {
        /// <summary>
        /// Width of the sidebar in pixels.
        /// </summary>
        private const double SIDEBAR_WIDTH = 250;
        
        /// <summary>
        /// Content margin in pixels.
        /// </summary>
        private const double CONTENT_MARGIN = 10;
        
        /// <summary>
        /// Size of the main icon in pixels.
        /// </summary>
        private const double ICON_SIZE = 50;
        
        /// <summary>
        /// Font size for title text.
        /// </summary>
        private const double TITLE_FONT_SIZE = 20;
        
        /// <summary>
        /// Font size for navigation item icons.
        /// </summary>
        private const double NAV_ICON_FONT_SIZE = 18;
        
        /// <summary>
        /// Font size for navigation item titles.
        /// </summary>
        private const double NAV_TITLE_FONT_SIZE = 14;
        
        /// <summary>
        /// Horizontal margin for title panel in pixels.
        /// </summary>
        private const double TITLE_PANEL_MARGIN_HORIZONTAL = 5;
        
        /// <summary>
        /// Top margin for title panel in pixels.
        /// </summary>
        private const double TITLE_PANEL_MARGIN_TOP = 15;
        
        /// <summary>
        /// Bottom margin for title panel in pixels.
        /// </summary>
        private const double TITLE_PANEL_MARGIN_BOTTOM = 10;
        
        /// <summary>
        /// Top margin for icon in pixels.
        /// </summary>
        private const double ICON_MARGIN_TOP = 6;
        
        /// <summary>
        /// Horizontal margin for separator in pixels.
        /// </summary>
        private const double SEPARATOR_MARGIN_HORIZONTAL = 10;
        
        /// <summary>
        /// Bottom margin for separator in pixels.
        /// </summary>
        private const double SEPARATOR_MARGIN_BOTTOM = 10;
        
        /// <summary>
        /// Margin for navigation list in pixels.
        /// </summary>
        private const double NAV_LIST_MARGIN = 8;
        
        /// <summary>
        /// Vertical margin for navigation list in pixels.
        /// </summary>
        private const double NAV_LIST_MARGIN_VERTICAL = 5;
        
        /// <summary>
        /// Right margin for navigation icon in pixels.
        /// </summary>
        private const double NAV_ICON_MARGIN_RIGHT = 12;
        
        /// <summary>
        /// Height of separator in pixels.
        /// </summary>
        private const double SEPARATOR_HEIGHT = 1;
        
        /// <summary>
        /// Right border width for sidebar in pixels.
        /// </summary>
        private const double SIDEBAR_BORDER_RIGHT = 1;
        
        /// <summary>
        /// Navigation index for Dashboard tab.
        /// </summary>
        private const int NAV_INDEX_DASHBOARD = 0;
        
        /// <summary>
        /// Navigation index for Installed tab.
        /// </summary>
        private const int NAV_INDEX_INSTALLED = 1;
        
        /// <summary>
        /// Navigation index for Install tab.
        /// </summary>
        private const int NAV_INDEX_INSTALL = 2;
        
        /// <summary>
        /// Navigation index for Uninstall tab.
        /// </summary>
        private const int NAV_INDEX_UNINSTALL = 3;
        
        /// <summary>
        /// Navigation index for Services tab.
        /// </summary>
        private const int NAV_INDEX_SERVICES = 4;
        
        /// <summary>
        /// Navigation index for Sites tab.
        /// </summary>
        private const int NAV_INDEX_SITES = 5;
        
        /// <summary>
        /// Navigation index for Utilities tab.
        /// </summary>
        private const int NAV_INDEX_UTILITIES = 6;
        
        /// <summary>
        /// Navigation index for Config tab.
        /// </summary>
        private const int NAV_INDEX_CONFIG = 7;
        
        /// <summary>
        /// Represents a navigation menu item with title, icon, and description.
        /// Used to build the navigation sidebar menu.
        /// </summary>
        public class NavigationItem
        {
            /// <summary>
            /// Gets or sets the navigation item title displayed in the menu.
            /// </summary>
            public string Title { get; set; } = "";
            
            /// <summary>
            /// Gets or sets the emoji icon displayed next to the title.
            /// </summary>
            public string Icon { get; set; } = "";
            
            /// <summary>
            /// Gets or sets the item description (not currently displayed).
            /// </summary>
            public string Description { get; set; } = "";
        }

        /// <summary>
        /// Creates the main content layout with sidebar navigation and content area.
        /// Sets up two-column grid: sidebar (250px) and main content area (remaining space).
        /// Automatically navigates to Dashboard on initial load.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding.</param>
        /// <param name="mainGrid">Grid container to add content to.</param>
        public static void CreateMainContent(DevStackGui mainWindow, Grid mainGrid)
        {
            var contentGrid = new Grid { Margin = new Thickness(0) };
            Grid.SetRow(contentGrid, 0);

            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SIDEBAR_WIDTH) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var sidebar = CreateSidebar(mainWindow);
            Grid.SetColumn(sidebar, 0);
            contentGrid.Children.Add(sidebar);

            mainWindow._mainContent = new ContentControl
            {
                Margin = new Thickness(CONTENT_MARGIN)
            };
            Grid.SetColumn(mainWindow._mainContent, 1);
            contentGrid.Children.Add(mainWindow._mainContent);

            NavigateToSection(mainWindow, NAV_INDEX_DASHBOARD);

            mainGrid.Children.Add(contentGrid);
        }

        /// <summary>
        /// Creates the sidebar with DevStack title, separator, and navigation menu.
        /// Applies theme styling for background and borders.
        /// </summary>
        /// <param name="mainWindow">Main window instance for theme access.</param>
        /// <returns>Border containing the complete sidebar.</returns>
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

            sidebarContainer.Children.Add(CreateSidebarTitleUnified(mainWindow));
            sidebarContainer.Children.Add(CreateSeparatorUnified(mainWindow));
            sidebarContainer.Children.Add(CreateNavigationListUnified(mainWindow));

            sidebar.Child = sidebarContainer;
            return sidebar;
        }

        /// <summary>
        /// Creates the sidebar title panel with DevStack icon and title text.
        /// </summary>
        /// <param name="mainWindow">The main window instance.</param>
        /// <returns>A StackPanel containing the sidebar title elements.</returns>
        private static StackPanel CreateSidebarTitleUnified(DevStackGui mainWindow)
        {
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(TITLE_PANEL_MARGIN_HORIZONTAL, TITLE_PANEL_MARGIN_TOP, TITLE_PANEL_MARGIN_HORIZONTAL, TITLE_PANEL_MARGIN_BOTTOM)
            };

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
        /// Creates the DevStack icon image from DevStack.ico file.
        /// Returns null if icon file is not found or cannot be loaded.
        /// </summary>
        /// <returns>Image control with DevStack icon, or null if loading fails.</returns>
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
        /// Creates the sidebar title label with "DevStack" text.
        /// Applies bold font and theme styling.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>Styled label with sidebar title.</returns>
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

        /// <summary>
        /// Creates a horizontal separator line for the sidebar.
        /// </summary>
        /// <param name="mainWindow">The main window instance.</param>
        /// <returns>A Border element representing the separator.</returns>
        private static Border CreateSeparatorUnified(DevStackGui mainWindow)
        {
            return new Border
            {
                Height = SEPARATOR_HEIGHT,
                Margin = new Thickness(SEPARATOR_MARGIN_HORIZONTAL, 0, SEPARATOR_MARGIN_HORIZONTAL, SEPARATOR_MARGIN_BOTTOM),
                Background = mainWindow.CurrentTheme.Border
            };
        }

        /// <summary>
        /// Creates the navigation list box with all menu items.
        /// </summary>
        /// <param name="mainWindow">The main window instance.</param>
        /// <returns>A ListBox containing all navigation items.</returns>
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

            var binding = new Binding("SelectedNavIndex") { Source = mainWindow };
            navList.SetBinding(ListBox.SelectedIndexProperty, binding);

            DevStackShared.ThemeManager.ApplySidebarListBoxTheme(navList);
            navList.UpdateLayout();
            return navList;
        }

        /// <summary>
        /// Creates the list of navigation menu items with localized titles.
        /// Returns 8 items: Dashboard, Installed, Install, Uninstall, Services, Sites, Utilities, Config.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization access.</param>
        /// <returns>List of NavigationItem objects with icons, titles, and descriptions.</returns>
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
        /// Creates a single navigation list item with icon and title.
        /// Used to build each entry in the navigation menu.
        /// </summary>
        /// <param name="mainWindow">Main window instance for theme access.</param>
        /// <param name="item">Navigation item data (icon, title).</param>
        /// <returns>ListBoxItem with formatted navigation entry.</returns>
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
        /// Creates the navigation icon label (emoji).
        /// </summary>
        /// <param name="mainWindow">Main window instance for theme access.</param>
        /// <param name="icon">Emoji icon string.</param>
        /// <returns>Label with styled icon.</returns>
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
        /// Creates the navigation title label.
        /// </summary>
        /// <param name="mainWindow">Main window instance for theme access.</param>
        /// <param name="title">Navigation item title text.</param>
        /// <returns>Label with styled title.</returns>
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
        /// Navigates to a specific section based on index.
        /// Routes to appropriate tab content (Dashboard, Install, Services, etc.).
        /// Updates main content area with selected section.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        /// <param name="index">Navigation index (0=Dashboard, 1=Installed, 2=Install, 3=Uninstall, 4=Services, 5=Sites, 6=Utilities, 7=Config).</param>
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
        /// Navigates to Dashboard tab.
        /// Renders dashboard immediately and loads data (installed components, services) in background.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        private static void NavigateToDashboard(DevStackGui mainWindow)
        {
            if (mainWindow._mainContent == null) return;
            
            mainWindow._mainContent.Content = GuiDashboardTab.CreateDashboardContent(mainWindow);
            
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
        /// Navigates to Installed Components tab.
        /// Loads installed components data if not already loaded.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        private static void NavigateToInstalled(DevStackGui mainWindow)
        {
            if (mainWindow._mainContent == null) return;
            
            if (mainWindow.InstalledComponents?.Count == 0)
            {
                _ = mainWindow.LoadInstalledComponents();
            }
            mainWindow._mainContent.Content = GuiInstalledTab.CreateInstalledContent(mainWindow);
        }

        /// <summary>
        /// Navigates to Install tab.
        /// Loads available components and shortcut components if not already loaded.
        /// </summary>
        /// <param name="mainWindow">Main window instance.</param>
        private static void NavigateToInstall(DevStackGui mainWindow)
        {
            if (mainWindow._mainContent == null) return;
            
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
