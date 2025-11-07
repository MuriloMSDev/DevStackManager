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
    /// Componente responsável pela aba "Dashboard" - visão geral do sistema reutilizando funcionalidades existentes
    /// </summary>
    public static class GuiDashboardTab
    {
        private static DispatcherTimer? _refreshTimer;
        
        /// <summary>
        /// Cria o conteúdo completo da aba "Dashboard"
        /// </summary>
        public static ScrollViewer CreateDashboardContent(DevStackGui mainWindow)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };

            // Aplicar scrollbar customizada do ThemeManager
            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Grid de cards
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Conteúdo detalhado

            // Header
            var headerPanel = CreateDashboardHeader(mainWindow);
            Grid.SetRow(headerPanel, 0);
            mainGrid.Children.Add(headerPanel);

            // Grid de cards - overview resumido
            var cardsGrid = CreateOverviewCardsGrid(mainWindow);
            Grid.SetRow(cardsGrid, 1);
            mainGrid.Children.Add(cardsGrid);

            // Conteúdo detalhado - painéis das outras tabs
            var detailGrid = CreateDetailedContentGrid(mainWindow);
            Grid.SetRow(detailGrid, 2);
            mainGrid.Children.Add(detailGrid);

            scrollViewer.Content = mainGrid;
            
            // Configurar sistema de binding e atualizações
            SetupDataBindings(mainWindow);
            
            // Carregar dados iniciais imediatamente
            Task.Run(async () => {
                System.Threading.Thread.Sleep(500); // Pequena pausa para garantir que a UI esteja pronta
                await mainWindow.Dispatcher.BeginInvoke(() => {
                    UpdateComponentsData(mainWindow);
                });
                
                // Usar método principal otimizado para carregamento dos serviços
                await mainWindow.LoadServices();
                await mainWindow.Dispatcher.BeginInvoke(() => UpdateServicesData(mainWindow));
            });
            
            return scrollViewer;
        }

        /// <summary>
        /// Cria o cabeçalho do dashboard
        /// </summary>
        private static StackPanel CreateDashboardHeader(DevStackGui mainWindow)
        {
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Row superior com título e botões
            var topRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.title"), true);
            titleLabel.FontSize = 28;
            titleLabel.Margin = new Thickness(0, 0, 20, 0);
            topRow.Children.Add(titleLabel);

            // Status indicator
            var statusPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 20, 0)
            };

            topRow.Children.Add(statusPanel);

            // Spacer flexível
            var spacer = new Border { Width = 1, HorizontalAlignment = HorizontalAlignment.Stretch };
            topRow.Children.Add(spacer);

            // Botões de ação
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            topRow.Children.Add(buttonsPanel);
            headerPanel.Children.Add(topRow);

            // Separador visual elegante
            var separator = DevStackShared.ThemeManager.CreateStyledSeparator();
            separator.Margin = new Thickness(0, 5, 0, 0);
            headerPanel.Children.Add(separator);

            return headerPanel;
        }

        /// <summary>
        /// Cria o grid de cards de overview
        /// </summary>
        private static Grid CreateOverviewCardsGrid(DevStackGui mainWindow)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 30) // Aumentado o espaçamento inferior
            };
            
            // 3 colunas para os cards com espaçamento melhor distribuído
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Card 1: Componentes Instalados (usando dados do GuiInstalledTab)
            var installedCard = CreateSummaryCard(
                "📦", 
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.title"),
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.loading"),
                () => mainWindow.SelectedNavIndex = 1, // Navega para tab Instalados
                mainWindow.CurrentTheme.Success,
                mainWindow
            );
            installedCard.Tag = "installedCard";
            Grid.SetColumn(installedCard, 0);
            grid.Children.Add(installedCard);

            // Card 2: Instalar (ações rápidas do GuiInstallTab)
            var installCard = CreateSummaryCard(
                "📥",
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.install.title"),
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.install.description"),
                () => mainWindow.SelectedNavIndex = 2, // Navega para tab Instalar
                mainWindow.CurrentTheme.Info,
                mainWindow
            );
            Grid.SetColumn(installCard, 1);
            grid.Children.Add(installCard);

            // Card 3: Serviços (usando dados do GuiServicesTab)
            var servicesCard = CreateSummaryCard(
                "⚙️",
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.title"),
                mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.loading"),
                () => mainWindow.SelectedNavIndex = 4, // Navega para tab Serviços
                mainWindow.CurrentTheme.Warning,
                mainWindow
            );
            servicesCard.Tag = "servicesCard";
            Grid.SetColumn(servicesCard, 2);
            grid.Children.Add(servicesCard);

            return grid;
        }

        /// <summary>
        /// Cria um card de resumo clicável com animações e feedback visual
        /// </summary>
        private static Border CreateSummaryCard(string icon, string title, string content, Action clickAction, SolidColorBrush accentColor, DevStackGui mainWindow)
        {
            var card = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(12), // Aumentado de 8 para 12 para melhor espaçamento
                Padding = new Thickness(20),
                Cursor = System.Windows.Input.Cursors.Hand,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 2,
                    Opacity = 0.1,
                    BlurRadius = 8
                }
            };

            var cardContent = new StackPanel();

            // Header do card melhorado
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var iconBorder = new Border
            {
                Background = accentColor,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 12, 0)
            };

            var iconLabel = new Label
            {
                Content = icon,
                FontSize = 20,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0)
            };
            iconBorder.Child = iconLabel;
            headerPanel.Children.Add(iconBorder);

            var titleStack = new StackPanel();
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(title, true);
            titleLabel.FontSize = 16;
            titleLabel.Margin = new Thickness(0);
            titleStack.Children.Add(titleLabel);

            var subtitleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.subtitle"));
            subtitleLabel.FontSize = 11;
            subtitleLabel.Foreground = DevStackShared.ThemeManager.CurrentTheme.TextMuted;
            subtitleLabel.Margin = new Thickness(0);
            titleStack.Children.Add(subtitleLabel);

            headerPanel.Children.Add(titleStack);
            cardContent.Children.Add(headerPanel);

            // Linha de separação com gradiente
            var separator = new Border
            {
                Height = 3,
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
                Margin = new Thickness(0, 0, 0, 12)
            };
            cardContent.Children.Add(separator);

            // Conteúdo do card melhorado com seta alinhada
            var contentPanel = new Grid();
            contentPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var contentLabel = DevStackShared.ThemeManager.CreateStyledLabel(content);
            contentLabel.FontSize = 14;
            contentLabel.Tag = "content";
            contentLabel.Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground;
            contentLabel.FontWeight = FontWeights.Medium;
            contentLabel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(contentLabel, 0);
            contentPanel.Children.Add(contentLabel);

            // Indicador visual de interação (seta) alinhado com o conteúdo
            var interactionHint = new Label
            {
                Content = "⎘", // ícone de abrir em outra página
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = accentColor,
                Opacity = 0.7,
            };
            Grid.SetColumn(interactionHint, 1);
            contentPanel.Children.Add(interactionHint);

            cardContent.Children.Add(contentPanel);

            card.Child = cardContent;

            // Eventos de interação melhorados
            card.MouseLeftButtonUp += (s, e) => {
                // Simular animação de clique
                card.Opacity = 0.8;
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Tick += (_, _) =>
                {
                    card.Opacity = 1.0;
                    timer.Stop();
                };
                timer.Start();
                
                clickAction();
            };

            card.MouseEnter += (s, e) => {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.DashboardCardHover;
                interactionHint.Opacity = 1.0;
                // Animação de hover
                var scaleTransform = new ScaleTransform(1.02, 1.02);
                card.RenderTransform = scaleTransform;
                card.RenderTransformOrigin = new Point(0.5, 0.5);
            };

            card.MouseLeave += (s, e) => {
                card.Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground;
                card.BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border;
                interactionHint.Opacity = 0.7;
                card.RenderTransform = null;
            };

            return card;
        }

        /// <summary>
        /// Cria o grid de conteúdo detalhado
        /// </summary>
        private static Grid CreateDetailedContentGrid(DevStackGui mainWindow)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0) // Adicionar margem superior para espaçamento
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Apenas uma linha com altura aumentada, já que removemos o console
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 300 });

            // Painel 1: Componentes instalados resumidos (reutilizando GuiInstalledTab)
            var installedPanel = CreateInstalledSummaryPanel(mainWindow);
            Grid.SetColumn(installedPanel, 0);
            Grid.SetRow(installedPanel, 0);
            grid.Children.Add(installedPanel);

            // Painel 2: Serviços (reutilizando GuiServicesTab)
            var servicesPanel = CreateServicesSummaryPanel(mainWindow);
            Grid.SetColumn(servicesPanel, 1);
            Grid.SetRow(servicesPanel, 0);
            grid.Children.Add(servicesPanel);

            return grid;
        }

        /// <summary>
        /// Cria painel resumido de componentes instalados com elementos visuais melhorados
        /// </summary>
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
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Footer

            // Header melhorado com ícone e gradiente usando Grid para posicionamento
            var headerPanel = new Grid
            {
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(headerPanel, 0);
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Lado esquerdo: ícone e título
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

            // Lado direito: botão de refresh
            var refreshComponentsButton = DevStackShared.ThemeManager.CreateStyledButton("🔄", async (s, e) => {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.updating_components");
                    
                    // Carregar componentes instalados
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

            // Container para o conteúdo principal
            var mainContentPanel = new StackPanel();
            Grid.SetRow(mainContentPanel, 1);

            // Separador moderno
            var separator = DevStackShared.ThemeManager.CreateStyledSeparator();
            separator.Margin = new Thickness(0, 0, 0, 10);
            mainContentPanel.Children.Add(separator);

            // Grid de componentes em cards para melhor aproveitamento do espaço
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 200,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Aplicar scrollbar customizada
            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var componentsGrid = new Grid
            {
                Background = Brushes.Transparent,
                Tag = "ComponentsList",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Criar grid dinâmico baseado na largura disponível
            int columnsPerRow = 4; // Máximo de 4 colunas por linha
            int currentRow = 0;
            int currentColumn = 0;

            // Usar dados dos componentes instalados já carregados no mainWindow
            try
            {
                var installedComponents = mainWindow.InstalledComponents?.Where(c => c.Installed).OrderBy(c => c.Label).ToList() ?? new List<ComponentViewModel>();
                
                if (installedComponents.Count > 0)
                {
                    // Definir colunas do grid
                    for (int i = 0; i < columnsPerRow; i++)
                    {
                        componentsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    }

                    foreach (var component in installedComponents)
                    {
                        // Criar um card para cada versão instalada do componente
                        if (component.Versions != null && component.Versions.Count > 0)
                        {
                            foreach (var version in component.Versions.OrderBy(v => v))
                            {
                                // Adicionar nova linha se necessário
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
                            // Se não há versões específicas, criar um card padrão
                            // Adicionar nova linha se necessário
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
                // Se houver erro, mostrar mensagem de erro
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

            // Footer com botões de ação
            var footerPanel = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.DashboardFooterBackground,
                CornerRadius = new CornerRadius(0, 0, 12, 12),
                Margin = new Thickness(-15, 10, -15, -15), // Margem negativa para ocupar toda a largura
                Padding = new Thickness(15, 10, 15, 10),
                Height = 48 // Altura padrão do footer
            };
            Grid.SetRow(footerPanel, 2);

            var footerContent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Botão para ir para Install Tab
            var installButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.install_button"), (s, e) => {
                mainWindow.SelectedNavIndex = 2; // Navegar para tab Instalar
            }, DevStackShared.ThemeManager.ButtonStyle.Info);
            installButton.Width = 100;
            installButton.Height = 28;
            installButton.FontSize = 11;
            installButton.Margin = new Thickness(5, 0, 5, 0);
            footerContent.Children.Add(installButton);

            // Botão para ir para Uninstall Tab
            // Botão para ir para Uninstall Tab
            var uninstallButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.components.uninstall_button"), (s, e) => {
                mainWindow.SelectedNavIndex = 3; // Navegar para tab Desinstalar
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
        /// Cria painel de instalação rápida
        /// </summary>
        private static Border CreateQuickInstallPanel(DevStackGui mainWindow)
        {
            var panel = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(5),
                Padding = new Thickness(10)
            };

            var content = new StackPanel();

            // Header
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.install.title"), true);
            titleLabel.FontSize = 14;
            titleLabel.Margin = new Thickness(0, 0, 0, 10);
            content.Children.Add(titleLabel);

            // ComboBox para seleção de componente (reutilizando binding existente)
            var componentCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            componentCombo.Height = 30;
            componentCombo.Margin = new Thickness(0, 0, 0, 10);
            var componentBinding = new Binding("AvailableComponents") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.ItemsSourceProperty, componentBinding);
            var selectedComponentBinding = new Binding("SelectedComponent") { Source = mainWindow };
            componentCombo.SetBinding(ComboBox.SelectedItemProperty, selectedComponentBinding);
            // Display Label via converter
            var quickInstallItemTemplate = new DataTemplate();
            var quickInstallTextFactory = new FrameworkElementFactory(typeof(TextBlock));
            quickInstallTextFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new GuiInstallTab.NameToLabelConverter() });
            quickInstallItemTemplate.VisualTree = quickInstallTextFactory;
            componentCombo.ItemTemplate = quickInstallItemTemplate;
            content.Children.Add(componentCombo);

            // ComboBox para versão (reutilizando binding existente)
            var versionCombo = DevStackShared.ThemeManager.CreateStyledComboBox();
            versionCombo.Height = 30;
            versionCombo.Margin = new Thickness(0, 0, 0, 10);
            var versionBinding = new Binding("AvailableVersions") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.ItemsSourceProperty, versionBinding);
            var selectedVersionBinding = new Binding("SelectedVersion") { Source = mainWindow };
            versionCombo.SetBinding(ComboBox.SelectedItemProperty, selectedVersionBinding);
            content.Children.Add(versionCombo);

            // Botões de ação
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var installButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.install.install_button"), async (s, e) => {
                if (string.IsNullOrEmpty(mainWindow.SelectedComponent))
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.install.select_component");
                    return;
                }
                
                mainWindow.IsInstallingComponent = true;
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.install.installing", mainWindow.SelectedComponent);
                
                await Task.Run(async () => {
                    try
                    {
                        string[] args = string.IsNullOrEmpty(mainWindow.SelectedVersion) 
                            ? new[] { mainWindow.SelectedComponent }
                            : new[] { mainWindow.SelectedComponent, mainWindow.SelectedVersion };
                        
                        await InstallManager.InstallCommands(args);
                        
                        mainWindow.Dispatcher.Invoke(() => {
                            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.install.success", mainWindow.SelectedComponent);
                        });
                    }
                    catch (Exception ex)
                    {
                        mainWindow.Dispatcher.Invoke(() => {
                            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.install.error", mainWindow.SelectedComponent, ex.Message);
                        });
                    }
                    finally
                    {
                        mainWindow.Dispatcher.Invoke(() => {
                            mainWindow.IsInstallingComponent = false;
                        });
                    }
                });
            });
            installButton.Height = 30;
            installButton.Margin = new Thickness(0, 0, 5, 0);
            buttonsPanel.Children.Add(installButton);

            var fullInstallButton = DevStackShared.ThemeManager.CreateStyledButton(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.panels.install.go_to_install"), (s, e) => mainWindow.SelectedNavIndex = 2, DevStackShared.ThemeManager.ButtonStyle.Secondary);
            fullInstallButton.Height = 30;
            buttonsPanel.Children.Add(fullInstallButton);

            content.Children.Add(buttonsPanel);

            panel.Child = content;
            return panel;
        }

        /// <summary>
        /// Cria painel resumido de serviços com elementos visuais modernos
        /// </summary>
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
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Footer

            // Header melhorado com ícone e gradiente usando Grid para posicionamento
            var headerPanel = new Grid
            {
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(headerPanel, 0);
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Lado esquerdo: ícone e título
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

            // Lado direito: botão de refresh
            var refreshServicesButton = DevStackShared.ThemeManager.CreateStyledButton("🔄", async (s, e) => {
                try
                {
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.updating_services");
                    
                    // Carregar serviços usando o método principal otimizado
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

            // Container para o conteúdo principal
            var mainContentPanel = new StackPanel();
            Grid.SetRow(mainContentPanel, 1);

            // Separador moderno
            var separator = DevStackShared.ThemeManager.CreateStyledSeparator();
            separator.Margin = new Thickness(0, 0, 0, 10);
            mainContentPanel.Children.Add(separator);

            // Lista melhorada com scroll customizado ou mensagem vazia
            var contentArea = new Grid();
            
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 120,
                Background = Brushes.Transparent
            };

            // Aplicar scrollbar customizada
            DevStackShared.ThemeManager.ApplyCustomScrollbar(scrollViewer);

            var servicesGrid = new Grid
            {
                Background = Brushes.Transparent,
                Tag = "ServicesList", // Manter a tag correta para o sistema de atualização
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Não carregar dados aqui - deixar para o sistema de atualização dinâmica
            // Inicializar apenas com uma mensagem de carregamento
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

            // Footer com botões de ação para serviços
            var footerPanel = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.DashboardFooterBackground,
                CornerRadius = new CornerRadius(0, 0, 12, 12),
                Margin = new Thickness(-15, 10, -15, -15), // Margem negativa para ocupar toda a largura
                Padding = new Thickness(15, 8, 15, 8),
                Height = 48 // Altura padrão do footer
            };
            Grid.SetRow(footerPanel, 2);

            var footerContent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Botão para iniciar todos os serviços
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

            // Botão para parar todos os serviços
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

            // Botão para reiniciar todos os serviços
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
        /// Atualiza o conteúdo de um card
        /// </summary>
        private static void UpdateCardContent(Border card, string newContent)
        {
            if (card.Child is StackPanel stackPanel)
            {
                // Procurar diretamente por Labels com tag "content"
                foreach (var child in stackPanel.Children)
                {
                    if (child is Label label && label.Tag?.ToString() == "content")
                    {
                        label.Content = newContent;
                        return;
                    }
                    // Se o child for um Grid (nova estrutura), procurar dentro dele
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
        /// Atualiza estatísticas específicas nos painéis do Dashboard
        /// </summary>
        private static void UpdatePanelStatistics(DevStackGui mainWindow, string tagName, string newContent)
        {
            try
            {
                // Procurar pelo elemento com a tag específica na janela principal
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
                // Falhar silenciosamente se não conseguir atualizar
            }
        }

        /// <summary>
        /// Atualiza a lista de componentes instalados no painel usando dados existentes
        /// </summary>
        private static void UpdateInstalledComponentsList(DevStackGui mainWindow, List<object> installedComponents)
        {
            try
            {
                // Buscar o Grid no MainContainer da janela
                var element = FindElementByTag(mainWindow, "ComponentsList");
                if (element is Grid componentsGrid)
                {
                    componentsGrid.Children.Clear();
                    componentsGrid.RowDefinitions.Clear();
                    componentsGrid.ColumnDefinitions.Clear();
                    
                    if (installedComponents.Count > 0)
                    {
                        // Ordenar a lista por nome antes de processar
                        var sortedComponents = installedComponents
                            .Cast<ComponentViewModel>()
                            .OrderBy(c => c.Label)
                            .ToList();

                        int columnsPerRow = 4; // Máximo de 4 colunas por linha
                        int currentRow = 0;
                        int currentColumn = 0;

                        // Definir colunas do grid
                        for (int i = 0; i < columnsPerRow; i++)
                        {
                            componentsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        }

                        foreach (var component in sortedComponents)
                        {
                            // Criar um card para cada versão instalada do componente
                            if (component.Versions != null && component.Versions.Count > 0)
                            {
                                foreach (var version in component.Versions.OrderBy(v => v))
                                {
                                    // Adicionar nova linha se necessário
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
                                // Se não há versões específicas, criar um card padrão
                                // Adicionar nova linha se necessário
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
                        // Mostrar mensagem quando não há componentes
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
                // Falha silenciosa se o elemento não for encontrado
            }
        }

        /// <summary>
        /// Busca um elemento pela tag na árvore visual
        /// </summary>
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
        /// Configura o data binding com as ObservableCollections do mainWindow
        /// </summary>
        private static void SetupDataBindings(DevStackGui mainWindow)
        {
            // Subscrever aos eventos de mudança das coleções
            if (mainWindow.InstalledComponents != null)
            {
                mainWindow.InstalledComponents.CollectionChanged += (sender, e) =>
                {
                    // Atualizar cards e listas quando componentes mudarem
                    mainWindow.Dispatcher.BeginInvoke(() => UpdateComponentsData(mainWindow));
                };
            }

            if (mainWindow.Services != null)
            {
                mainWindow.Services.CollectionChanged += (sender, e) =>
                {
                    // Atualizar cards e listas quando serviços mudarem
                    mainWindow.Dispatcher.BeginInvoke(() => UpdateServicesData(mainWindow));
                };
            }

            // Carregar dados iniciais com delay para garantir que a UI esteja pronta
            Task.Run(async () =>
            {
                await Task.Delay(1000); // Aguardar 1 segundo para a UI estar completamente carregada
                await mainWindow.Dispatcher.BeginInvoke(() =>
                {
                    UpdateComponentsData(mainWindow);
                });
                
                // Usar método principal otimizado para carregamento dos serviços
                await mainWindow.LoadServices();
                await mainWindow.Dispatcher.BeginInvoke(() =>
                {
                    UpdateServicesData(mainWindow);
                });
            });
        }

        /// <summary>
        /// Cria um card visual para representar um componente instalado, com funcionalidade de clique opcional
        /// </summary>
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

            // Definir colunas: texto à esquerda, ícone à direita
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Determinar se o componente é executável para definir a cor da borda
            bool isExecutable = component != null && mainWindow != null;
            if (isExecutable)
            {
                var comp = DevStackManager.Components.ComponentsFactory.GetComponent(component!.Name);
                isExecutable = comp != null && comp.IsExecutable && component.Installed;
            }

            // Borda superior com gradiente - posicionada para ignorar padding
            var topBorder = new Border
            {
                Height = 3,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Margin = new Thickness(-8, -9, -8, 0) // Margem negativa para anular o padding
            };
            Grid.SetColumnSpan(topBorder, 2);

            // Criar gradiente baseado na capacidade de execução do componente
            var gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = new Point(0, 0);
            gradientBrush.EndPoint = new Point(1, 0);

            if (isExecutable)
            {
                // Gradiente azul para componentes executáveis
                gradientBrush.GradientStops.Add(new GradientStop(DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color, 0.0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.R, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.G, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.B), 0.5));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.R, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.G, DevStackShared.ThemeManager.CurrentTheme.DashboardAccentBlue.Color.B), 1.0));
            }
            else
            {
                // Gradiente verde para componentes não executáveis
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

            // Nome do componente
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

            // Versão do componente
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

            // Adicionar elementos ao grid
            mainGrid.Children.Add(topBorder);
            mainGrid.Children.Add(stackPanel);

            // Adicionar ícone de raio ao lado direito para componentes executáveis
            if (isExecutable)
            {
                // Criar gradiente amarelo-laranja-vermelho
                var iconGradientBrush = new LinearGradientBrush();
                iconGradientBrush.StartPoint = new Point(0, 0);
                iconGradientBrush.EndPoint = new Point(0, 1);
                iconGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 215, 0), 0.0));   // Amarelo (Gold)
                iconGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 140, 0), 0.5));  // Laranja (DarkOrange)
                iconGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(220, 20, 60), 1.0));  // Vermelho (Crimson)

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

            // Verificar se deve adicionar funcionalidade de clique
            bool isClickable = component != null && mainWindow != null;
            if (isClickable)
            {
                var comp = DevStackManager.Components.ComponentsFactory.GetComponent(component!.Name);
                if (comp != null && comp.IsExecutable && component.Installed)
                {
                    card.Cursor = System.Windows.Input.Cursors.Hand;
                    
                    // Adicionar evento de clique
                    card.MouseLeftButtonUp += (s, e) =>
                    {
                        ExecuteComponent(component.Name, version, mainWindow!);
                    };

                    // Melhorar feedback visual para componentes executáveis
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

            // Adicionar efeito hover padrão (para componentes não clicáveis)
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
        /// Executa um componente específico com a versão especificada
        /// </summary>
        private static void ExecuteComponent(string componentName, string version, DevStackGui mainWindow)
        {
            try
            {
                var comp = DevStackManager.Components.ComponentsFactory.GetComponent(componentName);
                if (comp != null && comp.IsExecutable)
                {
                    // local helper: procura executável no PATH
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

                    // Compute base tool dir and target (installed) directory following ComponentBase semantics
                    string baseToolDir = !string.IsNullOrEmpty(comp.ToolDir) ? comp.ToolDir! : System.IO.Path.Combine(DevStackConfig.baseDir, comp.Name);
                    // Try to read SubDirectory from ComponentBase when available, otherwise fall back to default
                    string subDir = (comp is DevStackManager.Components.ComponentBase cb && !string.IsNullOrEmpty(cb.SubDirectory)) ? cb.SubDirectory! : $"{comp.Name}-{version}";
                    string installDir;

                    if (!string.IsNullOrEmpty(comp.ExecutableFolder))
                    {
                        if (System.IO.Path.IsPathRooted(comp.ExecutableFolder))
                        {
                            // legacy: ExecutableFolder contained the absolute tool directory
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
                        // Busca pelo padrão configurado
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
                        // Fallback: pega o primeiro .exe
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
        /// Cria um card visual para representar um serviço
        /// </summary>
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

            // Borda superior laranja com gradiente - posicionada para ignorar padding
            var topBorder = new Border
            {
                Height = 3,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Margin = new Thickness(-6, -7, -6, 0) // Margem negativa para anular o padding
            };

            // Criar gradiente laranja que vai de opaco para transparente
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

            // Nome do serviço
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

            // Versão do componente
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

            // Status do serviço
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

            // Adicionar elementos ao grid
            mainGrid.Children.Add(topBorder);
            mainGrid.Children.Add(stackPanel);

            card.Child = mainGrid;

            // Adicionar efeito hover
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
        /// Atualiza os dados dos componentes na interface
        /// </summary>
        private static void UpdateComponentsData(DevStackGui mainWindow)
        {
            try
            {
                var installedComponents = mainWindow.InstalledComponents?.Where(c => c.Installed).ToList() ?? new List<ComponentViewModel>();
                var totalComponents = mainWindow.InstalledComponents?.Count ?? 0;
                
                // Atualizar card de componentes
                var cardText = installedComponents.Count > 0 
                    ? mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.installed_count", installedComponents.Count, totalComponents)
                    : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.components.none");
                UpdateCardContentByTag(mainWindow, "installedCard", cardText);
                
                // Atualizar a lista de componentes instalados no painel
                UpdateInstalledComponentsList(mainWindow, new List<object>(installedComponents.Cast<object>()));
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_updating_component_data", ex.Message);
            }
        }

        /// <summary>
        /// Atualiza os dados dos serviços na interface
        /// </summary>
        private static void UpdateServicesData(DevStackGui mainWindow)
        {
            try
            {
                var services = mainWindow.Services?.OrderBy(s => s.Name).ToList() ?? new List<ServiceViewModel>();
                var runningServices = services.Where(s => s.IsRunning).ToList();
                
                // Atualizar card de serviços - sempre mostrar contagem, mesmo se 0
                var cardText = runningServices.Count > 0 
                    ? mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.active_count", runningServices.Count, services.Count)
                    : mainWindow.LocalizationManager.GetString("gui.dashboard_tab.cards.services.none");
                UpdateCardContentByTag(mainWindow, "servicesCard", cardText);
                
                // Atualizar a lista de serviços no painel
                UpdateServicesList(mainWindow, services);
            }
            catch (Exception ex)
            {
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_updating_service_data", ex.Message);
            }
        }

        /// <summary>
        /// Atualiza o conteúdo de um card específico usando sua tag
        /// </summary>
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
        /// Atualiza a lista de serviços no painel
        /// </summary>
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
                        int columnsPerRow = 4; // Máximo de 4 colunas por linha
                        int currentRow = 0;
                        int currentColumn = 0;

                        // Definir colunas do grid
                        for (int i = 0; i < columnsPerRow; i++)
                        {
                            servicesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        }

                        // Sempre mostrar os serviços organizados em grid
                        foreach (var service in services)
                        {
                            // Adicionar nova linha se necessário
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
                        // Mostrar mensagem quando não há serviços
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
        /// Cleanup quando o Dashboard é destruído
        /// </summary>
        public static void Cleanup()
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
        }

        /// <summary>
        /// Inicia todos os serviços de forma otimizada
        /// </summary>
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

            // Recarregar serviços e atualizar interface
            await mainWindow.LoadServices();
            UpdateServicesData(mainWindow);
        }

        /// <summary>
        /// Para todos os serviços de forma otimizada
        /// </summary>
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

            // Recarregar serviços e atualizar interface
            await mainWindow.LoadServices();
            UpdateServicesData(mainWindow);
        }

        /// <summary>
        /// Reinicia todos os serviços de forma otimizada
        /// </summary>
        private static async Task RestartAllServicesOptimized(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Parar todos e depois iniciar todos
                    ProcessManager.StopAllComponents();
                    System.Threading.Thread.Sleep(2000); // Aguardar 2 segundos
                    ProcessManager.StartAllComponents();
                }
                catch (Exception ex)
                {
                    DevStackConfig.WriteLog(mainWindow.LocalizationManager.GetString("gui.dashboard_tab.messages.error_restarting_services", ex.Message));
                    throw;
                }
            });

            // Recarregar serviços e atualizar interface
            await mainWindow.LoadServices();
            UpdateServicesData(mainWindow);
        }
    }
}
