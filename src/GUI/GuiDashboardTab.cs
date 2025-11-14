using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevStackManager
{
    /// <summary>
    /// Dashboard tab component providing system overview with visual cards.
    /// Displays installed components, running services, and quick access to key features.
    /// Features auto-refresh timer and real-time status updates with interactive cards.
    /// Reuses existing tab functionalities in a consolidated overview interface.
    /// </summary>
    public static class GuiDashboardTab
    {
        /// <summary>
        /// Main panel margin in pixels.
        /// </summary>
        private const int MAIN_MARGIN = 10;
        
        /// <summary>
        /// Bottom margin for header section in pixels.
        /// </summary>
        private const int HEADER_BOTTOM_MARGIN = 20;
        
        /// <summary>
        /// Bottom margin for top row in pixels.
        /// </summary>
        private const int TOP_ROW_BOTTOM_MARGIN = 10;
        
        /// <summary>
        /// Right margin for title element in pixels.
        /// </summary>
        private const int TITLE_RIGHT_MARGIN = 20;
        
        /// <summary>
        /// Font size for title text.
        /// </summary>
        private const int TITLE_FONT_SIZE = 28;
        
        /// <summary>
        /// Right margin for status panel in pixels.
        /// </summary>
        private const int STATUS_PANEL_RIGHT_MARGIN = 20;
        
        /// <summary>
        /// Top margin for separator element in pixels.
        /// </summary>
        private const int SEPARATOR_TOP_MARGIN = 5;
        
        /// <summary>
        /// Bottom margin for cards grid in pixels.
        /// </summary>
        private const int CARDS_GRID_BOTTOM_MARGIN = 30;
        
        /// <summary>
        /// Margin around each card in pixels.
        /// </summary>
        private const int CARD_MARGIN = 12;
        
        /// <summary>
        /// Internal padding for card content in pixels.
        /// </summary>
        private const int CARD_PADDING = 20;
        
        /// <summary>
        /// Thickness of card border in pixels.
        /// </summary>
        private const int CARD_BORDER_THICKNESS = 1;
        
        /// <summary>
        /// Corner radius for card containers in pixels.
        /// </summary>
        private const int CARD_CORNER_RADIUS = 12;
        
        /// <summary>
        /// Bottom margin for card header in pixels.
        /// </summary>
        private const int CARD_HEADER_BOTTOM_MARGIN = 15;
        
        /// <summary>
        /// Corner radius for icon border in pixels.
        /// </summary>
        private const int ICON_BORDER_CORNER_RADIUS = 8;
        
        /// <summary>
        /// Padding for icon border in pixels.
        /// </summary>
        private const int ICON_BORDER_PADDING = 8;
        
        /// <summary>
        /// Right margin for icon border in pixels.
        /// </summary>
        private const int ICON_BORDER_RIGHT_MARGIN = 12;
        
        /// <summary>
        /// Font size for icon text.
        /// </summary>
        private const int ICON_FONT_SIZE = 20;
        
        /// <summary>
        /// Font size for card title text.
        /// </summary>
        private const int TITLE_CARD_FONT_SIZE = 16;
        
        /// <summary>
        /// Font size for subtitle text.
        /// </summary>
        private const int SUBTITLE_FONT_SIZE = 11;
        
        /// <summary>
        /// Height of separator in pixels.
        /// </summary>
        private const int SEPARATOR_HEIGHT = 3;
        
        /// <summary>
        /// Bottom margin for separator in pixels.
        /// </summary>
        private const int SEPARATOR_BOTTOM_MARGIN = 12;
        
        /// <summary>
        /// Font size for content text.
        /// </summary>
        private const int CONTENT_FONT_SIZE = 14;
        
        /// <summary>
        /// Font size for interaction hint icon.
        /// </summary>
        private const int INTERACTION_HINT_FONT_SIZE = 48;
        
        /// <summary>
        /// <summary>
        /// Width of spacer element in pixels.
        /// </summary>
        private const int SPACER_WIDTH = 1;
        
        /// <summary>
        /// Direction angle in degrees for drop shadow effect.
        /// </summary>
        private const int SHADOW_DIRECTION = 315;
        
        /// <summary>
        /// Depth of drop shadow in pixels.
        /// </summary>
        private const int SHADOW_DEPTH = 2;
        
        /// <summary>
        /// Opacity value for drop shadow effect.
        /// </summary>
        private const double SHADOW_OPACITY = 0.1;
        
        /// <summary>
        /// Blur radius for drop shadow in pixels.
        /// </summary>
        private const int SHADOW_BLUR_RADIUS = 8;
        
        /// <summary>
        /// Opacity value for card when hovered.
        /// </summary>
        private const double CARD_HOVER_OPACITY = 0.8;
        
        /// <summary>
        /// Normal opacity value for card.
        /// </summary>
        private const double CARD_NORMAL_OPACITY = 1.0;
        
        /// <summary>
        /// Opacity value for interaction hint text.
        /// </summary>
        private const double INTERACTION_HINT_OPACITY = 0.7;
        
        /// <summary>
        /// Opacity value for interaction hint text when hovered.
        /// </summary>
        private const double INTERACTION_HINT_HOVER_OPACITY = 1.0;
        
        /// <summary>
        /// Delay in milliseconds before starting hover animation.
        /// </summary>
        private const int HOVER_ANIMATION_DELAY_MS = 100;
        
        /// <summary>
        /// Scale factor for card hover animation.
        /// </summary>
        private const double CARD_HOVER_SCALE = 1.02;
        
        /// <summary>
        /// Navigation index for Installed tab.
        /// </summary>
        private const int NAV_INDEX_INSTALLED = 1;
        
        /// <summary>
        /// Navigation index for Install tab.
        /// </summary>
        private const int NAV_INDEX_INSTALL = 2;
        
        /// <summary>
        /// Navigation index for Services tab.
        /// </summary>
        private const int NAV_INDEX_SERVICES = 4;
        
        /// <summary>
        /// Delay in milliseconds before UI initialization.
        /// </summary>
        private const int UI_INITIALIZATION_DELAY_MS = 500;
        
        /// <summary>
        /// Timer for automatic dashboard refresh to update component status.
        /// </summary>
        private static DispatcherTimer? _refreshTimer;
        
        /// <summary>
        /// Creates the complete Dashboard tab content with ScrollViewer container.
        /// Layout: Header with title and status, overview cards grid, detailed content sections.
        /// Initializes data bindings and starts automatic refresh timer.
        /// Loads initial component and service data after 500ms delay.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and operations</param>
        /// <returns>ScrollViewer with complete dashboard UI</returns>
        public static ScrollViewer CreateDashboardContent(DevStackGui mainWindow)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(MAIN_MARGIN)
            };

            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var headerPanel = CreateDashboardHeader(mainWindow);
            Grid.SetRow(headerPanel, 0);
            mainGrid.Children.Add(headerPanel);

            var cardsGrid = CreateOverviewCardsGrid(mainWindow);
            Grid.SetRow(cardsGrid, 1);
            mainGrid.Children.Add(cardsGrid);

            var detailGrid = CreateDetailedContentGrid(mainWindow);
            Grid.SetRow(detailGrid, 2);
            mainGrid.Children.Add(detailGrid);

            scrollViewer.Content = mainGrid;
            
            SetupDataBindings(mainWindow);
            
            Task.Run(async () => {
                System.Threading.Thread.Sleep(UI_INITIALIZATION_DELAY_MS);
                await mainWindow.Dispatcher.BeginInvoke(() => {
                    UpdateComponentsData(mainWindow);
                });
                
                await mainWindow.LoadServices();
                await mainWindow.Dispatcher.BeginInvoke(() => UpdateServicesData(mainWindow));
            });
            
            return scrollViewer;
        }

        /// <summary>
        /// Creates the dashboard header with title and status indicators.
        /// Layout: Title label (28pt font), status indicator panel.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <returns>StackPanel with header elements</returns>
        private static StackPanel CreateDashboardHeader(DevStackGui mainWindow)
        {
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, HEADER_BOTTOM_MARGIN)
            };

            var topRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, TOP_ROW_BOTTOM_MARGIN)
            };

            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.title"), true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, TITLE_RIGHT_MARGIN, 0);
            topRow.Children.Add(titleLabel);

            var statusPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, STATUS_PANEL_RIGHT_MARGIN, 0)
            };

            topRow.Children.Add(statusPanel);

            var spacer = new Border { Width = SPACER_WIDTH, HorizontalAlignment = HorizontalAlignment.Stretch };
            topRow.Children.Add(spacer);

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            topRow.Children.Add(buttonsPanel);
            headerPanel.Children.Add(topRow);

            var separator = DevStackShared.ThemeManager.CreateStyledSeparator();
            separator.Margin = new Thickness(0, SEPARATOR_TOP_MARGIN, 0, 0);
            headerPanel.Children.Add(separator);

            return headerPanel;
        }

        /// <summary>
        /// Creates grid of overview cards displaying component and service status.
        /// Layout: 3-column grid with equal width (Star sizing).
        /// Cards: Installed Components (navigates to Installed tab), 
        ///        Install Actions (navigates to Install tab),
        ///        Running Services (navigates to Services tab).
        /// Each card is tagged for dynamic content updates.
        /// </summary>
        /// <param name="mainWindow">Main window instance for navigation and localization</param>
        /// <returns>Grid with three interactive summary cards</returns>
        private static Grid CreateOverviewCardsGrid(DevStackGui mainWindow)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, CARDS_GRID_BOTTOM_MARGIN)
            };
            
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var installedCard = CreateSummaryCard(
                "📦", 
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.title"),
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.loading"),
                () => mainWindow.SelectedNavIndex = NAV_INDEX_INSTALLED,
                mainWindow.CurrentTheme.Success,
                mainWindow
            );
            installedCard.Tag = "installedCard";
            Grid.SetColumn(installedCard, 0);
            grid.Children.Add(installedCard);

            var installCard = CreateSummaryCard(
                "📥",
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.install.title"),
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.install.description"),
                () => mainWindow.SelectedNavIndex = NAV_INDEX_INSTALL,
                mainWindow.CurrentTheme.Info,
                mainWindow
            );
            Grid.SetColumn(installCard, 1);
            grid.Children.Add(installCard);

            var servicesCard = CreateSummaryCard(
                "⚙️",
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.title"),
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.loading"),
                () => mainWindow.SelectedNavIndex = NAV_INDEX_SERVICES,
                mainWindow.CurrentTheme.Warning,
                mainWindow
            );
            servicesCard.Tag = "servicesCard";
            Grid.SetColumn(servicesCard, 2);
            grid.Children.Add(servicesCard);

            return grid;
        }

        /// <summary>
        /// Creates interactive summary card with icon, title, and content.
        /// Features: Click navigation, hover scaling animation, accent color border.
        /// Visual: Drop shadow effect, rounded corners (12px), cursor hand on hover.
        /// </summary>
        /// <param name="icon">Emoji icon displayed at top of card</param>
        /// <param name="title">Card title text</param>
        /// <param name="content">Card content/description text</param>
        /// <param name="clickAction">Action executed when card is clicked</param>
        /// <param name="accentColor">Border and title accent color</param>
        /// <param name="mainWindow">Main window instance for theme access</param>
        /// <returns>Border element with card UI and interaction handlers</returns>
        private static Border CreateSummaryCard(string icon, string title, string content, Action clickAction, SolidColorBrush accentColor, DevStackGui mainWindow)
        {
            var card = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(CARD_BORDER_THICKNESS),
                CornerRadius = new CornerRadius(CARD_CORNER_RADIUS),
                Margin = new Thickness(CARD_MARGIN),
                Padding = new Thickness(CARD_PADDING),
                Cursor = System.Windows.Input.Cursors.Hand,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = SHADOW_DIRECTION,
                    ShadowDepth = SHADOW_DEPTH,
                    Opacity = SHADOW_OPACITY,
                    BlurRadius = SHADOW_BLUR_RADIUS
                }
            };

            var cardContent = new StackPanel();

            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, CARD_HEADER_BOTTOM_MARGIN)
            };

            var iconBorder = new Border
            {
                Background = accentColor,
                CornerRadius = new CornerRadius(ICON_BORDER_CORNER_RADIUS),
                Padding = new Thickness(ICON_BORDER_PADDING),
                Margin = new Thickness(0, 0, ICON_BORDER_RIGHT_MARGIN, 0)
            };

            var iconLabel = new Label
            {
                Content = icon,
                FontSize = ICON_FONT_SIZE,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0)
            };
            iconBorder.Child = iconLabel;
            headerPanel.Children.Add(iconBorder);

            var titleStack = new StackPanel();
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(title, true);
            titleLabel.FontSize = TITLE_CARD_FONT_SIZE;
            titleLabel.Margin = new Thickness(0);
            titleStack.Children.Add(titleLabel);

            var subtitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.subtitle"));
            subtitleLabel.FontSize = SUBTITLE_FONT_SIZE;
            subtitleLabel.Foreground = DevStackShared.ThemeManager.CurrentTheme.TextMuted;
            subtitleLabel.Margin = new Thickness(0);
            titleStack.Children.Add(subtitleLabel);

            headerPanel.Children.Add(titleStack);
            cardContent.Children.Add(headerPanel);

            var separator = new Border
            {
                Height = SEPARATOR_HEIGHT,
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(accentColor.Color, 0),
                        new GradientStop(Colors.Transparent, 1)
                    }
                },
                Margin = new Thickness(0, 0, 0, SEPARATOR_BOTTOM_MARGIN)
            };
            cardContent.Children.Add(separator);

            var contentPanel = new Grid();
            contentPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var contentLabel = DevStackShared.ThemeManager.CreateStyledLabel(content);
            contentLabel.FontSize = CONTENT_FONT_SIZE;
            contentLabel.Tag = "content";
            contentLabel.Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground;
            contentLabel.FontWeight = FontWeights.Medium;
            contentLabel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(contentLabel, 0);
            contentPanel.Children.Add(contentLabel);

            var interactionHint = new Label
            {
                Content = "⎘",
                FontSize = INTERACTION_HINT_FONT_SIZE,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = accentColor,
                Opacity = INTERACTION_HINT_OPACITY,
            };
            Grid.SetColumn(interactionHint, 1);
            contentPanel.Children.Add(interactionHint);

            cardContent.Children.Add(contentPanel);

            card.Child = cardContent;

            card.MouseLeftButtonUp += (s, e) => {
                card.Opacity = CARD_HOVER_OPACITY;
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(HOVER_ANIMATION_DELAY_MS) };
                timer.Tick += (_, _) =>
                {
                    card.Opacity = CARD_NORMAL_OPACITY;
                    timer.Stop();
                };
                timer.Start();
                
                clickAction();
            };

            card.MouseEnter += (s, e) => {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardHover;
                interactionHint.Opacity = INTERACTION_HINT_HOVER_OPACITY;
                var scaleTransform = new ScaleTransform(CARD_HOVER_SCALE, CARD_HOVER_SCALE);
                card.RenderTransform = scaleTransform;
                card.RenderTransformOrigin = new Point(0.5, 0.5);
            };

            card.MouseLeave += (s, e) => {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground;
                card.BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border;
                interactionHint.Opacity = INTERACTION_HINT_OPACITY;
                card.RenderTransform = null;
            };

            return card;
        }

        /// <summary>
        /// Creates detailed content grid with component and service summary panels.
        /// Layout: 2-column grid (1 Star width each), 1 row (Star height, 300px minimum).
        /// Left panel: Installed components summary with refresh button.
        /// Right panel: Services summary with refresh button.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding</param>
        /// <returns>Grid with two side-by-side summary panels</returns>
        private static Grid CreateDetailedContentGrid(DevStackGui mainWindow)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 300 });

            var installedPanel = CreateInstalledSummaryPanel(mainWindow);
            Grid.SetColumn(installedPanel, 0);
            Grid.SetRow(installedPanel, 0);
            grid.Children.Add(installedPanel);

            var servicesPanel = CreateServicesSummaryPanel(mainWindow);
            Grid.SetColumn(servicesPanel, 1);
            Grid.SetRow(servicesPanel, 0);
            grid.Children.Add(servicesPanel);

            return grid;
        }

        /// <summary>
        /// Creates installed components summary panel with component cards.
        /// Features: Drop shadow effect, rounded corners (12px), refresh button.
        /// Layout: Header with icon/title/refresh button, separator, scrollable component cards, footer with "View All" link.
        /// Component cards display: icon, name, version, launch button.
        /// Uses data binding to InstalledComponents collection for automatic updates.
        /// </summary>
        /// <param name="mainWindow">Main window instance for data binding and localization</param>
        /// <returns>Border with complete installed components panel</returns>
        private static Border CreateInstalledSummaryPanel(DevStackGui mainWindow)
        {
            var panel = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(8),
                Padding = new Thickness(15),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 1,
                    Opacity = 0.05,
                    BlurRadius = 6
                }
            };

            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerPanel = new Grid
            {
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(headerPanel, 0);
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconBorder = new Border
            {
                Background = mainWindow.CurrentTheme.Success,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var iconLabel = new Label
            {
                Content = "📦",
                FontSize = 16,
                Foreground = Brushes.White,
                Margin = new Thickness(0)
            };
            iconBorder.Child = iconLabel;
            leftPanel.Children.Add(iconBorder);

            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.title"), true);
            titleLabel.FontSize = 16;
            titleLabel.VerticalAlignment = VerticalAlignment.Center;
            leftPanel.Children.Add(titleLabel);

            Grid.SetColumn(leftPanel, 0);
            headerPanel.Children.Add(leftPanel);

            var refreshComponentsButton = DevStackShared.ThemeManager.CreateStyledButton("🔄", async (s, e) => {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.updating_components");
                    
                    await Task.Run(async () => {
                        await mainWindow.Dispatcher.InvokeAsync(async () => {
                            await mainWindow.LoadInstalledComponents();
                            UpdateComponentsData(mainWindow);
                        });
                    });
                    
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.components_updated");
                }
                catch (Exception ex)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_updating_components", ex.Message);
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Info);
            refreshComponentsButton.Width = 28;
            refreshComponentsButton.Height = 28;
            refreshComponentsButton.FontSize = 12;
            refreshComponentsButton.VerticalAlignment = VerticalAlignment.Center;
            refreshComponentsButton.HorizontalAlignment = HorizontalAlignment.Right;
            refreshComponentsButton.ToolTip = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.refresh_tooltip");
            Grid.SetColumn(refreshComponentsButton, 2);
            headerPanel.Children.Add(refreshComponentsButton);

            content.Children.Add(headerPanel);

            var mainContentPanel = new StackPanel();
            Grid.SetRow(mainContentPanel, 1);

            var separator = DevStackShared.ThemeManager.CreateStyledSeparator();
            separator.Margin = new Thickness(0, 0, 0, 10);
            mainContentPanel.Children.Add(separator);

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 200,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var componentsGrid = new Grid
            {
                Background = Brushes.Transparent,
                Tag = "ComponentsList",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            int columnsPerRow = 4;
            int currentRow = 0;
            int currentColumn = 0;

            try
            {
                var installedComponents = mainWindow.InstalledComponents?
                    .Where(c => c.Installed)
                    .OrderByDescending(c => c.IsExecutable)
                    .ThenBy(c => c.Label)
                    .ToList() ?? new List<ComponentViewModel>();
                
                if (installedComponents.Count > 0)
                {
                    for (int i = 0; i < columnsPerRow; i++)
                    {
                        componentsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    }

                    foreach (var component in installedComponents)
                    {
                        if (component.Versions != null && component.Versions.Count > 0)
                        {
                            foreach (var version in component.Versions.OrderBy(v => v))
                            {
                                if (currentColumn == 0)
                                {
                                    componentsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                }

                                var componentCard = CreateComponentCard(component.Label, version, component, mainWindow);
                                Grid.SetRow(componentCard, currentRow);
                                Grid.SetColumn(componentCard, currentColumn);
                                componentsGrid.Children.Add(componentCard);

                                currentColumn++;
                                if (currentColumn >= columnsPerRow)
                                {
                                    currentColumn = 0;
                                    currentRow++;
                                }
                            }
                        }
                        else
                        {
                            if (currentColumn == 0)
                            {
                                componentsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            }

                            var versionsText = !string.IsNullOrEmpty(component.VersionsText) ? component.VersionsText : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.installed_default");
                            var componentCard = CreateComponentCard(component.Label, versionsText, component, mainWindow);
                            Grid.SetRow(componentCard, currentRow);
                            Grid.SetColumn(componentCard, currentColumn);
                            componentsGrid.Children.Add(componentCard);

                            currentColumn++;
                            if (currentColumn >= columnsPerRow)
                            {
                                currentColumn = 0;
                                currentRow++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.error_loading"),
                    Foreground = DevStackShared.ThemeManager.CurrentTheme.DashboardErrorText,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10)
                };
                componentsGrid.Children.Add(errorText);
                DevStackConfig.WriteLog($"Erro ao carregar componentes no painel dashboard: {ex.Message}");
            }

            scrollViewer.Content = componentsGrid;
            mainContentPanel.Children.Add(scrollViewer);

            content.Children.Add(mainContentPanel);

            var footerPanel = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.DashboardFooterBackground,
                CornerRadius = new CornerRadius(0, 0, 12, 12),
                Margin = new Thickness(-15, 10, -15, -15),
                Padding = new Thickness(15, 10, 15, 10),
                Height = 48
            };
            Grid.SetRow(footerPanel, 2);

            var footerContent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var installButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.install_button"), (s, e) => {
                mainWindow.SelectedNavIndex = 2;
            }, DevStackShared.ThemeManager.ButtonStyle.Info);
            installButton.Width = 100;
            installButton.Height = 28;
            installButton.FontSize = 11;
            installButton.Margin = new Thickness(5, 0, 5, 0);
            footerContent.Children.Add(installButton);

            var uninstallButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.uninstall_button"), (s, e) => {
                mainWindow.SelectedNavIndex = 3;
            }, DevStackShared.ThemeManager.ButtonStyle.Danger);
            uninstallButton.Width = 100;
            uninstallButton.Height = 28;
            uninstallButton.FontSize = 11;
            uninstallButton.Margin = new Thickness(5, 0, 5, 0);
            footerContent.Children.Add(uninstallButton);

            footerPanel.Child = footerContent;
            content.Children.Add(footerPanel);

            panel.Child = content;
            return panel;
        }

        /// <summary>
        /// Creates services summary panel with service status list.
        /// Features: Drop shadow effect, rounded corners (12px), refresh button.
        /// Layout: Header with icon/title/refresh button, separator, scrollable service list, footer with Start/Stop/Restart All buttons.
        /// Service list displays: component name, version, status indicator, PID.
        /// Includes bulk operations: Start All, Stop All, Restart All services.
        /// </summary>
        /// <param name="mainWindow">Main window instance for service operations and localization</param>
        /// <returns>Border with complete services panel</returns>
        private static Border CreateServicesSummaryPanel(DevStackGui mainWindow)
        {
            var panel = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(8),
                Padding = new Thickness(15),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 1,
                    Opacity = 0.05,
                    BlurRadius = 6
                }
            };

            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerPanel = new Grid
            {
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(headerPanel, 0);
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconBorder = new Border
            {
                Background = mainWindow.CurrentTheme.Warning,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var iconLabel = new Label
            {
                Content = "⚙️",
                FontSize = 16,
                Foreground = Brushes.White,
                Margin = new Thickness(0)
            };
            iconBorder.Child = iconLabel;
            leftPanel.Children.Add(iconBorder);

            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.title"), true);
            titleLabel.FontSize = 16;
            titleLabel.VerticalAlignment = VerticalAlignment.Center;
            leftPanel.Children.Add(titleLabel);

            Grid.SetColumn(leftPanel, 0);
            headerPanel.Children.Add(leftPanel);

            var refreshServicesButton = DevStackShared.ThemeManager.CreateStyledButton("🔄", async (s, e) => {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.updating_services");
                    
                    await Task.Run(async () => {
                        await mainWindow.Dispatcher.InvokeAsync(async () => {
                            await mainWindow.LoadServices();
                            UpdateServicesData(mainWindow);
                        });
                    });
                    
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.services_updated");
                }
                catch (Exception ex)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_updating_services", ex.Message);
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Info);
            refreshServicesButton.Width = 28;
            refreshServicesButton.Height = 28;
            refreshServicesButton.FontSize = 12;
            refreshServicesButton.VerticalAlignment = VerticalAlignment.Center;
            refreshServicesButton.HorizontalAlignment = HorizontalAlignment.Right;
            refreshServicesButton.ToolTip = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.refresh_tooltip");
            Grid.SetColumn(refreshServicesButton, 2);
            headerPanel.Children.Add(refreshServicesButton);

            content.Children.Add(headerPanel);

            var mainContentPanel = new StackPanel();
            Grid.SetRow(mainContentPanel, 1);

            var separator = DevStackShared.ThemeManager.CreateStyledSeparator();
            separator.Margin = new Thickness(0, 0, 0, 10);
            mainContentPanel.Children.Add(separator);

            var contentArea = new Grid();
            
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 120,
                Background = Brushes.Transparent
            };

            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var servicesGrid = new Grid
            {
                Background = Brushes.Transparent,
                Tag = "ServicesList",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var loadingText = new TextBlock
            {
                Text = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.loading"),
                Foreground = DevStackShared.ThemeManager.CurrentTheme.DashboardMutedText,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };
            servicesGrid.Children.Add(loadingText);

            scrollViewer.Content = servicesGrid;
            contentArea.Children.Add(scrollViewer);
            mainContentPanel.Children.Add(contentArea);

            content.Children.Add(mainContentPanel);

            var footerPanel = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.DashboardFooterBackground,
                CornerRadius = new CornerRadius(0, 0, 12, 12),
                Margin = new Thickness(-15, 10, -15, -15),
                Padding = new Thickness(15, 8, 15, 8),
                Height = 48
            };
            Grid.SetRow(footerPanel, 2);

            var footerContent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var startAllButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.start_all"), async (s, e) => {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.starting_all_services");
                    await StartAllServicesOptimized(mainWindow);
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.all_services_started");
                }
                catch (Exception ex)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_starting_services", ex.Message);
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Success);
            startAllButton.Width = 80;
            startAllButton.Height = 28;
            startAllButton.FontSize = 11;
            startAllButton.Margin = new Thickness(3, 0, 3, 0);
            footerContent.Children.Add(startAllButton);

            var stopAllButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.stop_all"), async (s, e) => {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.stopping_all_services");
                    await StopAllServicesOptimized(mainWindow);
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.all_services_stopped");
                }
                catch (Exception ex)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_stopping_services", ex.Message);
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Danger);
            stopAllButton.Width = 80;
            stopAllButton.Height = 28;
            stopAllButton.FontSize = 11;
            stopAllButton.Margin = new Thickness(3, 0, 3, 0);
            footerContent.Children.Add(stopAllButton);

            var restartAllButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.restart_all"), async (s, e) => {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.restarting_all_services");
                    await RestartAllServicesOptimized(mainWindow);
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.all_services_restarted");
                }
                catch (Exception ex)
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_restarting_services", ex.Message);
                }
            }, DevStackShared.ThemeManager.ButtonStyle.Warning);
            restartAllButton.Width = 80;
            restartAllButton.Height = 28;
            restartAllButton.FontSize = 11;
            restartAllButton.Margin = new Thickness(3, 0, 3, 0);
            footerContent.Children.Add(restartAllButton);

            footerPanel.Child = footerContent;
            content.Children.Add(footerPanel);

            panel.Child = content;
            return panel;
        }

        /// <summary>
        /// Updates card content label text dynamically.
        /// Searches for Label with "content" tag within Border's StackPanel or Grid structure.
        /// Used to update card statistics without recreating entire card.
        /// </summary>
        /// <param name="card">Border element containing card UI</param>
        /// <param name="newContent">New text content to display</param>
        private static void UpdateCardContent(Border card, string newContent)
        {
            if (card.Child is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is Label label && label.Tag?.ToString() == "content")
                    {
                        label.Content = newContent;
                        return;
                    }
                    else if (child is Grid grid)
                    {
                        foreach (var gridChild in grid.Children)
                        {
                            if (gridChild is Label gridLabel && gridLabel.Tag?.ToString() == "content")
                            {
                                gridLabel.Content = newContent;
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates panel statistics by finding element with specific tag and updating content.
        /// Supports Label and TextBlock elements.
        /// Fails silently if element not found or update fails.
        /// </summary>
        /// <param name="mainWindow">Main window instance to search within</param>
        /// <param name="tagName">Tag identifier of element to update</param>
        /// <param name="newContent">New text content to display</param>
        private static void UpdatePanelStatistics(DevStackGui mainWindow, string tagName, string newContent)
        {
            try
            {
                var element = FindElementByTag(mainWindow, tagName);
                if (element != null)
                {
                    if (element is Label label)
                    {
                        label.Content = newContent;
                    }
                    else if (element is TextBlock textBlock)
                    {
                        textBlock.Text = newContent;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Updates installed components list in dashboard panel.
        /// Clears and rebuilds component cards grid (4 columns per row).
        /// Sorting: Executable components first, then by label, then by version.
        /// Creates individual cards for each installed version.
        /// Shows "no components" message when list is empty.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization</param>
        /// <param name="installedComponents">List of ComponentViewModel objects to display</param>
        private static void UpdateInstalledComponentsList(DevStackGui mainWindow, List<object> installedComponents)
        {
            try
            {
                var element = FindElementByTag(mainWindow, "ComponentsList");
                if (element is Grid componentsGrid)
                {
                    componentsGrid.Children.Clear();
                    componentsGrid.RowDefinitions.Clear();
                    componentsGrid.ColumnDefinitions.Clear();
                    
                    if (installedComponents.Count > 0)
                    {
                        var sortedComponents = installedComponents
                            .Cast<ComponentViewModel>()
                            .OrderByDescending(c => c.IsExecutable)
                            .ThenBy(c => c.Label)
                            .ToList();

                        int columnsPerRow = 4;
                        int currentRow = 0;
                        int currentColumn = 0;

                        for (int i = 0; i < columnsPerRow; i++)
                        {
                            componentsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        }

                        foreach (var component in sortedComponents)
                        {
                            if (component.Versions != null && component.Versions.Count > 0)
                            {
                                foreach (var version in component.Versions.OrderBy(v => v))
                                {
                                    if (currentColumn == 0)
                                    {
                                        componentsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                    }

                                    var componentCard = CreateComponentCard(component.Label, version, component, mainWindow);
                                    Grid.SetRow(componentCard, currentRow);
                                    Grid.SetColumn(componentCard, currentColumn);
                                    componentsGrid.Children.Add(componentCard);

                                    currentColumn++;
                                    if (currentColumn >= columnsPerRow)
                                    {
                                        currentColumn = 0;
                                        currentRow++;
                                    }
                                }
                            }
                            else
                            {
                                if (currentColumn == 0)
                                {
                                    componentsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                }

                                var versionText = !string.IsNullOrEmpty(component.VersionsText) ? component.VersionsText : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.installed_default");
                                var componentCard = CreateComponentCard(component.Label, versionText, component, mainWindow);
                                Grid.SetRow(componentCard, currentRow);
                                Grid.SetColumn(componentCard, currentColumn);
                                componentsGrid.Children.Add(componentCard);

                                currentColumn++;
                                if (currentColumn >= columnsPerRow)
                                {
                                    currentColumn = 0;
                                    currentRow++;
                                }
                            }
                        }
                    }
                    else
                    {
                        var noComponentsText = new TextBlock
                        {
                            Text = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.none"),
                            Foreground = DevStackShared.ThemeManager.CurrentTheme.DashboardMutedText,
                            FontSize = 12,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(10)
                        };
                        componentsGrid.Children.Add(noComponentsText);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Finds element in visual tree by Tag property.
        /// Performs recursive depth-first search through visual tree hierarchy.
        /// Returns first element matching the specified tag name.
        /// </summary>
        /// <param name="parent">Starting point for visual tree search</param>
        /// <param name="tagName">Tag value to search for</param>
        /// <returns>FrameworkElement with matching tag, or null if not found</returns>
        private static FrameworkElement? FindElementByTag(DependencyObject parent, string tagName)
        {
            if (parent == null) return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is FrameworkElement element && element.Tag?.ToString() == tagName)
                {
                    return element;
                }

                var result = FindElementByTag(child, tagName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Configures data binding with main window's ObservableCollection properties.
        /// Subscribes to InstalledComponents and Services CollectionChanged events.
        /// Triggers automatic UI updates when collections change.
        /// Loads initial data after 1-second delay to ensure UI is ready.
        /// Calls LoadServices method for optimized service data loading.
        /// </summary>
        /// <param name="mainWindow">Main window instance with ObservableCollection properties</param>
        private static void SetupDataBindings(DevStackGui mainWindow)
        {
            if (mainWindow.InstalledComponents != null)
            {
                mainWindow.InstalledComponents.CollectionChanged += (sender, e) =>
                {
                    mainWindow.Dispatcher.BeginInvoke(() => UpdateComponentsData(mainWindow));
                };
            }

            if (mainWindow.Services != null)
            {
                mainWindow.Services.CollectionChanged += (sender, e) =>
                {
                    mainWindow.Dispatcher.BeginInvoke(() => UpdateServicesData(mainWindow));
                };
            }

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await mainWindow.Dispatcher.BeginInvoke(() =>
                {
                    UpdateComponentsData(mainWindow);
                });
                
                await mainWindow.LoadServices();
                await mainWindow.Dispatcher.BeginInvoke(() =>
                {
                    UpdateServicesData(mainWindow);
                });
            });
        }

        /// <summary>
        /// Creates visual card representing installed component.
        /// Features: Drop shadow effect, 3px gradient top border (lightning colors for executables, success color for non-executables),
        ///          component icon, name label, version text, lightning button for executables.
        /// Card height: 55px fixed, width: stretch to fill column.
        /// Layout: 2-column grid (text left, icon/button right).
        /// </summary>
        /// <param name="name">Component display name</param>
        /// <param name="version">Version text to display</param>
        /// <param name="component">Optional ComponentViewModel for executable detection</param>
        /// <param name="mainWindow">Optional main window instance for executable launch functionality</param>
        /// <returns>Border with complete component card UI</returns>
        private static Border CreateComponentCard(string name, string version, ComponentViewModel? component = null, DevStackGui? mainWindow = null)
        {
            var card = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardBackground,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 9, 8, 6),
                Margin = new Thickness(2, 2, 2, 2),
                Height = 55,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                ClipToBounds = false,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 2,
                    Opacity = 0.1,
                    BlurRadius = 8
                }
            };

            var mainGrid = new Grid
            {
                ClipToBounds = false
            };

            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            bool isExecutable = component != null && mainWindow != null;
            if (isExecutable)
            {
                var comp = DevStackManager.Components.ComponentsFactory.GetComponent(component!.Name);
                isExecutable = comp != null && comp.IsExecutable && component.Installed;
            }

            var topBorder = new Border
            {
                Height = 3,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Margin = new Thickness(-8, -9, -8, 0)
            };
            Grid.SetColumnSpan(topBorder, 2);

            var gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = new Point(0, 0);
            gradientBrush.EndPoint = new Point(1, 0);

            if (isExecutable)
            {
                gradientBrush.GradientStops.Add(new GradientStop(DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color, 0.0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.R, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.G, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.B), 0.5));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.R, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.G, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.B), 1.0));
            }
            else
            {
                gradientBrush.GradientStops.Add(new GradientStop(DevStackShared.ThemeManager.CurrentTheme.Success.Color, 0.0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, DevStackShared.ThemeManager.CurrentTheme.Success.Color.R, DevStackShared.ThemeManager.CurrentTheme.Success.Color.G, DevStackShared.ThemeManager.CurrentTheme.Success.Color.B), 0.5));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, DevStackShared.ThemeManager.CurrentTheme.Success.Color.R, DevStackShared.ThemeManager.CurrentTheme.Success.Color.G, DevStackShared.ThemeManager.CurrentTheme.Success.Color.B), 1.0));
            }
            
            topBorder.Background = gradientBrush;

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };
            Grid.SetColumn(stackPanel, 0);

            var nameText = new TextBlock
            {
                Text = name,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 2)
            };

            var versionText = new TextBlock
            {
                Text = version,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(versionText);

            mainGrid.Children.Add(topBorder);
            mainGrid.Children.Add(stackPanel);

            if (isExecutable)
            {
                var iconGradientBrush = new LinearGradientBrush();
                iconGradientBrush.StartPoint = new Point(0, 0);
                iconGradientBrush.EndPoint = new Point(0, 1);
                iconGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 215, 0), 0.0));
                iconGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 140, 0), 0.5));
                iconGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(220, 20, 60), 1.0));

                var iconText = new TextBlock
                {
                    Text = "🗲",
                    FontSize = 30,
                    Foreground = iconGradientBrush,
                    Opacity = 0.90,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, -4, 3, 0),
                    IsHitTestVisible = false
                };
                Grid.SetColumn(iconText, 1);
                mainGrid.Children.Add(iconText);
            }

            card.Child = mainGrid;

            bool isClickable = component != null && mainWindow != null;
            if (isClickable)
            {
                var comp = DevStackManager.Components.ComponentsFactory.GetComponent(component!.Name);
                if (comp != null && comp.IsExecutable && component.Installed)
                {
                    card.Cursor = System.Windows.Input.Cursors.Hand;
                    
                    card.MouseLeftButtonUp += (s, e) =>
                    {
                        ExecuteComponent(component.Name, version, mainWindow!);
                    };

                    card.MouseEnter += (s, e) =>
                    {
                        card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardHover;
                    };

                    card.MouseLeave += (s, e) =>
                    {
                        card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardBackground;
                        card.BorderBrush = Brushes.Transparent;
                        card.BorderThickness = new Thickness(0);
                    };
                    
                    return card;
                }
            }

            card.MouseEnter += (s, e) =>
            {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardHoverDefault;
            };

            card.MouseLeave += (s, e) =>
            {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardBackground;
            };

            return card;
        }

        /// <summary>
        /// Executes component executable for specified version.
        /// Process: Locates component via ComponentsFactory, determines installation directory from ToolDir/SubDirectory/ExecutableFolder,
        ///         searches for .exe files matching ExecutablePattern or uses first .exe found.
        /// Command-line executables: Launches in Windows Terminal (wt.exe) or PowerShell with -NoExit flag.
        /// GUI executables: Launches directly with working directory set to installation folder.
        /// Updates status message with execution result or error information.
        /// </summary>
        /// <param name="componentName">Component name to execute</param>
        /// <param name="version">Version to execute</param>
        /// <param name="mainWindow">Main window instance for status updates and localization</param>
        private static void ExecuteComponent(string componentName, string version, DevStackGui mainWindow)
        {
            try
            {
                var comp = DevStackManager.Components.ComponentsFactory.GetComponent(componentName);
                if (comp != null && comp.IsExecutable)
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
                                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.opening_shell", componentName, version);
                            }
                            else
                            {
                                var process = new System.Diagnostics.Process();
                                process.StartInfo.FileName = exePath;
                                process.StartInfo.WorkingDirectory = installDir;
                                process.StartInfo.UseShellExecute = true;
                                process.Start();
                                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.executing", componentName, version);
                            }
                        }
                        else
                        {
                            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.no_executable", componentName, version);
                        }
                    }
                    else
                    {
                        mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.version_folder_not_found", installDir);
                    }
                }
                else
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.component_not_executable", componentName);
                }
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_executing", componentName, version, ex.Message);
                DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_executing", componentName, version, ex.Message));
            }
        }

        /// <summary>
        /// Creates visual card representing service with status indicator.
        /// Features: Drop shadow effect, 3px orange gradient top border, service name, version, status with color coding.
        /// Card height: 55px fixed, width: stretch to fill column.
        /// Status colors: Green (Running), Red (Stopped), Gray (Unknown).
        /// Layout: Single column StackPanel with centered text elements.
        /// </summary>
        /// <param name="name">Service display name</param>
        /// <param name="version">Service version text</param>
        /// <param name="status">Service status text (Running/Stopped/Unknown)</param>
        /// <param name="mainWindow">Main window instance for theme access</param>
        /// <returns>Border with complete service card UI</returns>
        private static Border CreateServiceCard(string name, string version, string status, DevStackGui mainWindow)
        {
            var card = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardBackground,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 7, 6, 5),
                Margin = new Thickness(2, 2, 2, 2),
                Height = 55,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 2,
                    Opacity = 0.1,
                    BlurRadius = 8
                }
            };

            var mainGrid = new Grid();

            var topBorder = new Border
            {
                Height = 3,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Margin = new Thickness(-6, -7, -6, 0)
            };

            var gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = new Point(0, 0);
            gradientBrush.EndPoint = new Point(1, 0);
            gradientBrush.GradientStops.Add(new GradientStop(DevStackShared.ThemeManager.CurrentTheme.DashboardServiceYellow.Color, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, DevStackShared.ThemeManager.CurrentTheme.DashboardServiceYellow.Color.R, DevStackShared.ThemeManager.CurrentTheme.DashboardServiceYellow.Color.G, DevStackShared.ThemeManager.CurrentTheme.DashboardServiceYellow.Color.B), 0.5));
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, DevStackShared.ThemeManager.CurrentTheme.DashboardServiceYellow.Color.R, DevStackShared.ThemeManager.CurrentTheme.DashboardServiceYellow.Color.G, DevStackShared.ThemeManager.CurrentTheme.DashboardServiceYellow.Color.B), 1.0));
            
            topBorder.Background = gradientBrush;

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var nameText = new TextBlock
            {
                Text = name,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 1)
            };

            var versionText = new TextBlock
            {
                Text = !string.IsNullOrEmpty(version) ? version : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.version_na"),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 1)
            };

            var statusText = new TextBlock
            {
                Text = status,
                FontSize = 9,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.DashboardMutedText,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(versionText);
            stackPanel.Children.Add(statusText);

            mainGrid.Children.Add(topBorder);
            mainGrid.Children.Add(stackPanel);

            card.Child = mainGrid;

            card.MouseEnter += (s, e) =>
            {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardHoverDefault;
            };

            card.MouseLeave += (s, e) =>
            {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardBackground;
            };

            return card;
        }

        /// <summary>
        /// Updates component data in dashboard interface.
        /// Counts installed components, updates "installedCard" with count text,
        /// refreshes installed components list panel.
        /// Handles errors gracefully and updates status message.
        /// </summary>
        /// <param name="mainWindow">Main window instance with InstalledComponents collection</param>
        private static void UpdateComponentsData(DevStackGui mainWindow)
        {
            try
            {
                var installedComponents = mainWindow.InstalledComponents?.Where(c => c.Installed).ToList() ?? new List<ComponentViewModel>();
                var totalComponents = mainWindow.InstalledComponents?.Count ?? 0;
                
                var cardText = installedComponents.Count > 0 
                    ? mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.installed_count", installedComponents.Count, totalComponents)
                    : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.none");
                UpdateCardContentByTag(mainWindow, "installedCard", cardText);
                
                UpdateInstalledComponentsList(mainWindow, new List<object>(installedComponents.Cast<object>()));
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_updating_component_data", ex.Message);
            }
        }

        /// <summary>
        /// Updates service data in dashboard interface.
        /// Counts running services, updates "servicesCard" with running/total count,
        /// refreshes services list panel with current status.
        /// Handles errors gracefully and updates status message.
        /// </summary>
        /// <param name="mainWindow">Main window instance with Services collection</param>
        private static void UpdateServicesData(DevStackGui mainWindow)
        {
            try
            {
                var services = mainWindow.Services?.OrderBy(s => s.Name).ToList() ?? new List<ServiceViewModel>();
                var runningServices = services.Where(s => s.IsRunning).ToList();
                
                var cardText = runningServices.Count > 0 
                    ? mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.active_count", runningServices.Count, services.Count)
                    : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.none");
                UpdateCardContentByTag(mainWindow, "servicesCard", cardText);
                
                UpdateServicesList(mainWindow, services);
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_updating_service_data", ex.Message);
            }
        }

        /// <summary>
        /// Updates card content by finding card using tag identifier.
        /// Searches main content ScrollViewer for Border element with specified tag,
        /// then calls UpdateCardContent to modify label text.
        /// </summary>
        /// <param name="mainWindow">Main window instance to search within</param>
        /// <param name="tag">Tag identifier of card to update</param>
        /// <param name="newContent">New content text to display</param>
        private static void UpdateCardContentByTag(DevStackGui mainWindow, string tag, string newContent)
        {
            if (mainWindow._mainContent?.Content is ScrollViewer scrollViewer &&
                scrollViewer.Content is Grid mainGrid)
            {
                var card = FindElementByTag(mainGrid, tag) as Border;
                if (card != null)
                {
                    UpdateCardContent(card, newContent);
                }
            }
        }

        /// <summary>
        /// Updates services list in dashboard panel.
        /// Clears and rebuilds services grid (2 columns per row).
        /// Creates service cards for each service with name, version, and status.
        /// Shows "no services" message when list is empty.
        /// Finds grid by "ServicesList" tag in visual tree.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and theme access</param>
        /// <param name="services">List of ServiceViewModel objects to display</param>
        private static void UpdateServicesList(DevStackGui mainWindow, List<ServiceViewModel> services)
        {
            if (mainWindow._mainContent?.Content is ScrollViewer scrollViewer &&
                scrollViewer.Content is Grid mainGrid)
            {
                var servicesGrid = FindElementByTag(mainGrid, "ServicesList") as Grid;
                if (servicesGrid != null)
                {
                    servicesGrid.Children.Clear();
                    servicesGrid.RowDefinitions.Clear();
                    servicesGrid.ColumnDefinitions.Clear();
                    
                    if (services.Count > 0)
                    {
                        int columnsPerRow = 4;
                        int currentRow = 0;
                        int currentColumn = 0;

                        for (int i = 0; i < columnsPerRow; i++)
                        {
                            servicesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        }

                        foreach (var service in services)
                        {
                            if (currentColumn == 0)
                            {
                                servicesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            }

                            var status = service.IsRunning ? mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.status.active") : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.status.stopped");
                            var serviceCard = CreateServiceCard(string.IsNullOrEmpty(service.Label) ? service.Name : service.Label, service.Version, status, mainWindow);
                            Grid.SetRow(serviceCard, currentRow);
                            Grid.SetColumn(serviceCard, currentColumn);
                            servicesGrid.Children.Add(serviceCard);

                            currentColumn++;
                            if (currentColumn >= columnsPerRow)
                            {
                                currentColumn = 0;
                                currentRow++;
                            }
                        }
                    }
                    else
                    {
                        var noServicesText = new TextBlock
                        {
                            Text = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.services.none"),
                            Foreground = DevStackShared.ThemeManager.CurrentTheme.DashboardMutedText,
                            FontSize = 12,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(10)
                        };
                        servicesGrid.Children.Add(noServicesText);
                    }
                }
            }
        }

        /// <summary>
        /// Cleanup resources when Dashboard tab is destroyed.
        /// Stops and disposes refresh timer if active.
        /// Call this method when switching away from dashboard or closing application.
        /// </summary>
        public static void Cleanup()
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
        }

        /// <summary>
        /// Starts all services using optimized batch processing.
        /// Executes ProcessManager.StartAllComponents in background thread,
        /// then reloads services and updates dashboard UI.
        /// Logs errors with localized message.
        /// </summary>
        /// <param name="mainWindow">Main window instance for service operations</param>
        /// <returns>Task representing async operation</returns>
        private static async Task StartAllServicesOptimized(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    ProcessManager.StartAllComponents();
                }
                catch (Exception ex)
                {
                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_starting_services", ex.Message));
                    throw;
                }
            });

            await mainWindow.LoadServices();
            UpdateServicesData(mainWindow);
        }

        /// <summary>
        /// Stops all services using optimized batch processing.
        /// Executes ProcessManager.StopAllComponents in background thread,
        /// then reloads services and updates dashboard UI.
        /// Logs errors with localized message.
        /// </summary>
        /// <param name="mainWindow">Main window instance for service operations</param>
        /// <returns>Task representing async operation</returns>
        private static async Task StopAllServicesOptimized(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    ProcessManager.StopAllComponents();
                }
                catch (Exception ex)
                {
                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_stopping_services", ex.Message));
                    throw;
                }
            });

            await mainWindow.LoadServices();
            UpdateServicesData(mainWindow);
        }

        /// <summary>
        /// Restarts all services using optimized batch processing.
        /// Executes ProcessManager.StopAllComponents, waits 2 seconds,
        /// then ProcessManager.StartAllComponents in background thread.
        /// Reloads services and updates dashboard UI after restart.
        /// Logs errors with localized message.
        /// </summary>
        /// <param name="mainWindow">Main window instance for service operations</param>
        /// <returns>Task representing async operation</returns>
        private static async Task RestartAllServicesOptimized(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    ProcessManager.StopAllComponents();
                    System.Threading.Thread.Sleep(2000);
                    ProcessManager.StartAllComponents();
                }
                catch (Exception ex)
                {
                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_restarting_services", ex.Message));
                    throw;
                }
            });

            await mainWindow.LoadServices();
            UpdateServicesData(mainWindow);
        }
    }
}
