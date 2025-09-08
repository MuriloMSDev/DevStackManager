using DevStackManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Microsoft.Win32;
using DevStackShared;

namespace DevStackUninstaller
{
    public partial class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                // Initialize localization for uninstaller
                var localization = LocalizationManager.Initialize(ApplicationType.Uninstaller);
                
                var app = new Application();
                app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                
                var window = new UninstallerWindow();
                app.MainWindow = window;
                
                app.Run(window);
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.Instance?.GetString("uninstaller.dialogs.startup_error_message", ex.Message, ex) ?? $"Error starting uninstaller: {ex.Message}\n\nDetails: {ex}", 
                    LocalizationManager.Instance?.GetString("uninstaller.dialogs.startup_error_title") ?? "DevStack Uninstaller Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public enum UninstallerStep
    {
        Welcome,
        Confirmation,
        UninstallOptions,
        ReadyToUninstall,
        Uninstalling,
        Finished
    }

    public class UninstallerWindow : Window
    {
        private readonly LocalizationManager localization = LocalizationManager.Instance!;
        private UninstallerStep currentStep = UninstallerStep.Welcome;
        private Grid mainGrid = null!;
        private Grid contentGrid = null!;
        private TextBlock stepTitleText = null!;
        private TextBlock stepDescriptionText = null!;
        private Button backButton = null!;
        private Button nextButton = null!;
        private Button cancelButton = null!;
        private ProgressBar stepProgressBar = null!;
        
        // Language selector
        private ComboBox languageComboBox = null!;
        private Label languageLabel = null!;
        private StackPanel languagePanel = null!;
        
        // Uninstall settings
        private string installationPath = "";
        private bool removeUserData = false;  // Default: preserve user data
        private bool removeRegistry = true;   // Default: clean registry
        private bool removeShortcuts = true;  // Default: remove shortcuts
        private bool removeFromPath = true;   // Default: remove from PATH
        
        // Step-specific controls
        private CheckBox removeUserDataCheckBox = null!;
#pragma warning disable CS0414 // Field is assigned but its value is never used - keeping for future implementation
        private CheckBox removeRegistryCheckBox = null!;
        private CheckBox removeShortcutsCheckBox = null!;
        private CheckBox removeFromPathCheckBox = null!;
#pragma warning restore CS0414
        private ProgressBar uninstallProgressBar = null!;
        private TextBlock uninstallStatusText = null!;
        private ListBox uninstallLogListBox = null!;

        public UninstallerWindow()
        {
            try
            {
                var lang = DevStackConfig.GetSetting("language")?.ToString();
                if (!string.IsNullOrWhiteSpace(lang))
                    LocalizationManager.ApplyLanguage(lang);

                localization.LanguageChanged += Localization_LanguageChanged;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(localization.GetString("uninstaller.dialogs.initialization_error_message", ex.Message), 
                    localization.GetString("uninstaller.dialogs.initialization_error_title"), MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private string GetVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>()?.Version;
                return version ?? localization.GetString("common.unknown");
            }
            catch
            {
                return localization.GetString("common.unknown");
            }
        }

        private void InitializeComponent()
        {
            string version = GetVersion();
            
            // Log version info for debugging
            System.Diagnostics.Debug.WriteLine($"Uninstaller version: {version}");
            
            // Get window title with explicit formatting
            Title = localization.GetString("uninstaller.window_title", version);
            System.Diagnostics.Debug.WriteLine($"Window title set to: {Title}");
            Width = 750;
            Height = 650;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            
            // Apply theme using ThemeManager
            ThemeManager.ApplyThemeToWindow(this);

            // Initialize installation path
            installationPath = GetInstallationPath();

            // Add window closing event handler
            Closing += OnWindowClosing;

            CreateMainLayout();
            UpdateStepContent();
        }

        private void CreateMainLayout()
        {
            mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(105) }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) }); // Buttons

            // Header
            CreateHeader();

            // Content area
            contentGrid = new Grid();
            contentGrid.Margin = new Thickness(20);
            Grid.SetRow(contentGrid, 1);
            mainGrid.Children.Add(contentGrid);

            // Buttons
            CreateButtonBar();

            Content = mainGrid;
        }

        private void CreateHeader()
        {
            var headerBorder = new Border
            {
                Background = ThemeManager.CurrentTheme.ControlBackground,
                BorderBrush = ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headerStackPanel = new StackPanel
            {
                Margin = new Thickness(25, 20, 25, 20),
                VerticalAlignment = VerticalAlignment.Center
            };

            stepTitleText = new TextBlock
            {
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeManager.CurrentTheme.Foreground
            };

            stepDescriptionText = new TextBlock
            {
                FontSize = 13,
                Foreground = ThemeManager.CurrentTheme.TextSecondary,
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            headerStackPanel.Children.Add(stepTitleText);
            headerStackPanel.Children.Add(stepDescriptionText);

            // Progress indicator
            stepProgressBar = ThemeManager.CreateStyledProgressBar(0, 5, false);
            stepProgressBar.Width = 220;
            stepProgressBar.Height = 6;
            stepProgressBar.Margin = new Thickness(25, 0, 25, 0);
            stepProgressBar.VerticalAlignment = VerticalAlignment.Center;
            stepProgressBar.Foreground = ThemeManager.CurrentTheme.Danger; // Red for uninstall

            stepProgressBar.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 3,
                ShadowDepth = 1,
                Opacity = 0.3,
                Color = Colors.Black
            };

            Grid.SetColumn(headerStackPanel, 0);
            Grid.SetColumn(stepProgressBar, 1);
            headerGrid.Children.Add(headerStackPanel);
            headerGrid.Children.Add(stepProgressBar);

            headerBorder.Child = headerGrid;
            Grid.SetRow(headerBorder, 0);
            mainGrid.Children.Add(headerBorder);
        }

        private void CreateButtonBar()
        {
            var buttonBorder = new Border
            {
                Background = ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(0, 1, 0, 0)
            };

            // Grid para organizar language selector à esquerda e botões à direita
            var buttonGrid = new Grid();
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Language selector panel no lado esquerdo
            languagePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(25, 18, 25, 18)
            };

            languageLabel = ThemeManager.CreateStyledLabel(localization.GetString("uninstaller.welcome.language_label"), false, false, ThemeManager.LabelStyle.Secondary);
            languageLabel.FontSize = 14;
            languageLabel.VerticalAlignment = VerticalAlignment.Center;
            languageLabel.Margin = new Thickness(0, 0, 10, 0);

            languageComboBox = ThemeManager.CreateStyledComboBox();

            // Populate language options
            var languages = localization.GetAvailableLanguages();
            foreach (var lang in languages)
            {
                var langName = localization.GetLanguageName(lang);
                var item = new ComboBoxItem
                {
                    Content = langName,
                    Tag = lang,
                    Foreground = ThemeManager.CurrentTheme.Foreground
                };
                languageComboBox.Items.Add(item);

                if (lang == LocalizationManager.CurrentLanguageStatic)
                {
                    languageComboBox.SelectedIndex = languageComboBox.Items.Count - 1;
                }
            }

            languageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;

            languagePanel.Children.Add(languageLabel);
            languagePanel.Children.Add(languageComboBox);

            // Buttons panel no lado direito
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(25, 18, 25, 18)
            };

            // Back button
            backButton = ThemeManager.CreateStyledButton(localization.GetString("common.buttons.back"), null, ThemeManager.ButtonStyle.Secondary);
            backButton.Width = 90;
            backButton.Height = 36;
            backButton.Margin = new Thickness(0, 0, 12, 0);
            backButton.IsEnabled = false;
            backButton.Click += BackButton_Click;

            // Next button - using Danger style for uninstall theme
            nextButton = ThemeManager.CreateStyledButton(localization.GetString("common.buttons.next"), null, ThemeManager.ButtonStyle.Danger);
            nextButton.Width = 130;
            nextButton.Height = 36;
            nextButton.Margin = new Thickness(0, 0, 12, 0);
            nextButton.Click += NextButton_Click;

            // Cancel button
            cancelButton = ThemeManager.CreateStyledButton(localization.GetString("common.buttons.cancel"), null, ThemeManager.ButtonStyle.Secondary);
            cancelButton.Width = 90;
            cancelButton.Height = 36;
            cancelButton.Click += CancelButton_Click;

            buttonPanel.Children.Add(backButton);
            buttonPanel.Children.Add(nextButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetColumn(languagePanel, 0);
            Grid.SetColumn(buttonPanel, 1);
            buttonGrid.Children.Add(languagePanel);
            buttonGrid.Children.Add(buttonPanel);

            buttonBorder.Child = buttonGrid;
            Grid.SetRow(buttonBorder, 2);
            mainGrid.Children.Add(buttonBorder);
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (languageComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string languageCode)
            {
                LocalizationManager.ApplyLanguage(languageCode);
            }
        }

        private void Localization_LanguageChanged(object? sender, string newLanguage)
        {
            // Reconstrói textos e layout principal para refletir o novo idioma
            Dispatcher.Invoke(() =>
            {
                // Atualiza título da janela
                var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStack-Uninstaller.exe");
                var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? localization.GetString("common.unknown");
                Title = localization.GetString("uninstaller.window_title", version);

                // Recria layout principal preservando o passo atual
                var mainGridRef = Content as Grid;
                if (mainGridRef != null)
                {
                    mainGridRef.Children.Clear();
                    mainGridRef.RowDefinitions.Clear();
                    mainGridRef.RowDefinitions.Add(new RowDefinition { Height = new GridLength(105) });
                    mainGridRef.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    mainGridRef.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

                    CreateHeader();
                    contentGrid = new Grid();
                    contentGrid.Margin = new Thickness(20);
                    Grid.SetRow(contentGrid, 1);
                    mainGridRef.Children.Add(contentGrid);
                    CreateButtonBar();
                    Content = mainGridRef;
                    UpdateStepContent();
                }
                else
                {
                    // Fallback: reconstruir tudo
                    InitializeComponent();
                }
            });
        }

        private string GetInstallationPath()
        {
            try
            {
                // Try to get installation path from registry
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\DevStack");
                if (key != null)
                {
                    var path = key.GetValue("InstallPath")?.ToString();
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        return path;
                    }
                }

                // Try to get from current executable location
                var currentPath = AppContext.BaseDirectory;
                if (!string.IsNullOrEmpty(currentPath))
                {
                    if (File.Exists(Path.Combine(currentPath, "DevStack.exe")) || 
                        File.Exists(Path.Combine(currentPath, "DevStackGUI.exe")))
                    {
                        return currentPath;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void UpdateStepContent()
        {
            // Clear content
            contentGrid.Children.Clear();
            contentGrid.RowDefinitions.Clear();
            contentGrid.ColumnDefinitions.Clear();

            contentGrid.Background = ThemeManager.CurrentTheme.FormBackground;
            contentGrid.Margin = new Thickness(25);

            // Update progress
            stepProgressBar.Value = (int)currentStep;

            // Update buttons
            backButton.IsEnabled = currentStep != UninstallerStep.Welcome;
            cancelButton.Visibility = Visibility.Visible;
            
            // Show/hide language selector (only on Welcome step)
            languagePanel.Visibility = currentStep == UninstallerStep.Welcome ? Visibility.Visible : Visibility.Collapsed;
            
            switch (currentStep)
            {
                case UninstallerStep.Welcome:
                    CreateWelcomeStep();
                    nextButton.Content = localization.GetString("common.buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case UninstallerStep.Confirmation:
                    CreateConfirmationStep();
                    nextButton.Content = localization.GetString("common.buttons.continue");
                    nextButton.IsEnabled = true;
                    break;
                case UninstallerStep.UninstallOptions:
                    CreateUninstallOptionsStep();
                    nextButton.Content = localization.GetString("common.buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case UninstallerStep.ReadyToUninstall:
                    CreateReadyToUninstallStep();
                    nextButton.Content = localization.GetString("common.buttons.uninstall");
                    nextButton.IsEnabled = true;
                    break;
                case UninstallerStep.Uninstalling:
                    CreateUninstallingStep();
                    nextButton.IsEnabled = false;
                    backButton.IsEnabled = false;
                    break;
                case UninstallerStep.Finished:
                    CreateFinishedStep();
                    nextButton.Content = localization.GetString("common.buttons.finish");
                    nextButton.IsEnabled = true;
                    backButton.IsEnabled = false;
                    cancelButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void CreateWelcomeStep()
        {
            // Log detailed diagnostic info
            System.Diagnostics.Debug.WriteLine("=========== CREATING UNINSTALLER WELCOME STEP ===========");
            
            var title = localization.GetString("uninstaller.welcome.title");
            System.Diagnostics.Debug.WriteLine($"uninstaller.welcome.title = \"{title}\"");
            stepTitleText.Text = title;
            
            var description = localization.GetString("uninstaller.welcome.description");
            System.Diagnostics.Debug.WriteLine($"uninstaller.welcome.description = \"{description}\"");
            stepDescriptionText.Text = description;

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var welcomePanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = ThemeManager.CurrentTheme.ControlBackground
            };

            var welcomeContainer = ThemeManager.CreateStyledCard(new StackPanel(), 12, true);
            welcomeContainer.Padding = new Thickness(40, 35, 40, 35);

            var innerPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var logoImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/DevStack.ico")),
                Width = 80,
                Height = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var welcomeText = new TextBlock
            {
                Text = localization.GetString("uninstaller.welcome.app_name"),
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = ThemeManager.CurrentTheme.Danger, // Danger color for uninstall
                Margin = new Thickness(0, 0, 0, 8)
            };

            var versionText = new TextBlock
            {
                Text = localization.GetString("uninstaller.welcome.version", GetVersion()),
                FontSize = 15,
                Foreground = ThemeManager.CurrentTheme.TextSecondary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 25)
            };

            var descriptionText = new TextBlock
            {
                Text = localization.GetString("uninstaller.welcome.app_description"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                LineHeight = 22,
                Foreground = ThemeManager.CurrentTheme.TextSecondary,
                MaxWidth = 420
            };

            innerPanel.Children.Add(logoImage);
            innerPanel.Children.Add(welcomeText);
            innerPanel.Children.Add(versionText);
            innerPanel.Children.Add(descriptionText);

            welcomeContainer.Child = innerPanel;
            welcomePanel.Children.Add(welcomeContainer);

            Grid.SetRow(welcomePanel, 0);
            contentGrid.Children.Add(welcomePanel);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > UninstallerStep.Welcome)
            {
                currentStep--;
                UpdateStepContent();
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (currentStep)
            {
                case UninstallerStep.Welcome:
                case UninstallerStep.Confirmation:
                case UninstallerStep.UninstallOptions:
                case UninstallerStep.ReadyToUninstall:
                    if (currentStep == UninstallerStep.ReadyToUninstall)
                    {
                        // Start uninstallation
                        currentStep = UninstallerStep.Uninstalling;
                        UpdateStepContent();
                        await PerformUninstallation();
                    }
                    else
                    {
                        currentStep++;
                        UpdateStepContent();
                    }
                    break;
                case UninstallerStep.Finished:
                    // Force application exit to ensure file handle is released
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(500); // Small delay to ensure UI updates
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Application.Current.Shutdown();
                        });
                        
                        // Additional forced exit if needed
                        await Task.Delay(1000);
                        Environment.Exit(0);
                    });
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ThemeManager.CreateStyledMessageBox(
                localization.GetString("uninstaller.dialogs.cancel_message"),
                localization.GetString("uninstaller.dialogs.cancel_title"),
                MessageBoxButton.YesNo, MessageBoxImage.Question
            );
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (currentStep != UninstallerStep.Finished)
            {
                var result = ThemeManager.CreateStyledMessageBox(
                    localization.GetString("uninstaller.dialogs.cancel_message"), 
                    localization.GetString("uninstaller.dialogs.cancel_title"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question
                );
                
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        // Placeholder methods - will be implemented in next parts
        private void CreateConfirmationStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.confirmation.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.confirmation.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Warning panel using ThemeManager notification panel
            var warningContainer = ThemeManager.CreateNotificationPanel(
                localization.GetString("uninstaller.confirmation.warning_text"), 
                ThemeManager.NotificationType.Warning, 
                true);
            warningContainer.Margin = new Thickness(0, 0, 0, 20);

            // Installation details panel
            var detailsContainer = ThemeManager.CreateStyledCard(new StackPanel(), 8, false);
            detailsContainer.Background = ThemeManager.CurrentTheme.ConsoleBackground;
            detailsContainer.BorderBrush = ThemeManager.CurrentTheme.Border;

            var detailsPanel = (StackPanel)detailsContainer.Child;
            detailsPanel.Margin = new Thickness(20, 18, 20, 18);

            if (!string.IsNullOrEmpty(installationPath))
            {
                detailsPanel.Children.Add(new TextBlock
                {
                    Text = localization.GetString("uninstaller.confirmation.install_found"),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeManager.CurrentTheme.Foreground,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                detailsPanel.Children.Add(new TextBlock
                {
                    Text = installationPath,
                    Foreground = ThemeManager.CurrentTheme.Accent,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 13,
                    Margin = new Thickness(20, 0, 0, 15)
                });

                // Calculate folder size
                var sizeText = GetInstallationSizeText();
                if (!string.IsNullOrEmpty(sizeText))
                {
                    detailsPanel.Children.Add(new TextBlock
                    {
                        Text = localization.GetString("uninstaller.confirmation.space_to_free", sizeText),
                        Foreground = ThemeManager.CurrentTheme.TextSecondary,
                        FontSize = 13,
                        Margin = new Thickness(0, 0, 0, 0)
                    });
                }
            }
            else
            {
                detailsPanel.Children.Add(new TextBlock
                {
                    Text = localization.GetString("uninstaller.confirmation.install_not_found"),
                    Foreground = ThemeManager.CurrentTheme.Danger,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                detailsPanel.Children.Add(new TextBlock
                {
                    Text = localization.GetString("uninstaller.confirmation.install_not_found_desc"),
                    Foreground = ThemeManager.CurrentTheme.TextSecondary,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 13
                });
            }

            detailsContainer.Child = detailsPanel;

            Grid.SetRow(warningContainer, 0);
            Grid.SetRow(detailsContainer, 1);
            contentGrid.Children.Add(warningContainer);
            contentGrid.Children.Add(detailsContainer);
        }

        private void CreateUninstallOptionsStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.uninstall_options.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.uninstall_options.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var optionsLabel = ThemeManager.CreateStyledLabel(
                localization.GetString("uninstaller.uninstall_options.label"), 
                false, false, ThemeManager.LabelStyle.Title);
            optionsLabel.FontSize = 15;
            optionsLabel.Margin = new Thickness(0, 0, 0, 15);

            var optionsContainer = ThemeManager.CreateStyledCard(new StackPanel(), 8, false);
            optionsContainer.Padding = new Thickness(20, 18, 20, 18);

            var optionsPanel = (StackPanel)optionsContainer.Child;

            // User data checkbox (only configurable option)
            removeUserDataCheckBox = ThemeManager.CreateStyledCheckBox(
                localization.GetString("uninstaller.uninstall_options.user_data"), removeUserData);
            removeUserDataCheckBox.IsChecked = removeUserData;
            removeUserDataCheckBox.Checked += (s, e) => removeUserData = true;
            removeUserDataCheckBox.Unchecked += (s, e) => removeUserData = false;
            removeUserDataCheckBox.Margin = new Thickness(0, 0, 0, 15);

            var userDataDescription = new TextBlock
            {
                Text = localization.GetString("uninstaller.uninstall_options.user_data_desc"),
                FontSize = 12,
                Foreground = ThemeManager.CurrentTheme.TextMuted,
                Margin = new Thickness(30, -10, 0, 0)
            };

            optionsPanel.Children.Add(removeUserDataCheckBox);
            optionsPanel.Children.Add(userDataDescription);

            // Info panel
            var infoPanel = ThemeManager.CreateNotificationPanel(
                localization.GetString("uninstaller.uninstall_options.info"), 
                ThemeManager.NotificationType.Info
            );
            infoPanel.Margin = new Thickness(0, 20, 0, 0);

            Grid.SetRow(optionsLabel, 0);
            Grid.SetRow(optionsContainer, 1);
            
            var containerPanel = new StackPanel();
            containerPanel.Children.Add(optionsContainer);
            containerPanel.Children.Add(infoPanel);
            
            Grid.SetRow(containerPanel, 1);
            contentGrid.Children.Add(optionsLabel);
            contentGrid.Children.Add(containerPanel);
        }

        private void CreateReadyToUninstallStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.ready_to_uninstall.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.ready_to_uninstall.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var summaryLabel = ThemeManager.CreateStyledLabel(
                localization.GetString("uninstaller.ready_to_uninstall.summary_label"), 
                false, false, ThemeManager.LabelStyle.Title);
            summaryLabel.FontSize = 15;
            summaryLabel.Margin = new Thickness(0, 0, 0, 15);

            var summaryContainer = ThemeManager.CreateStyledCard(new StackPanel(), 8, false);
            summaryContainer.Background = ThemeManager.CurrentTheme.ConsoleBackground;
            summaryContainer.BorderBrush = ThemeManager.CurrentTheme.Border;
            summaryContainer.Padding = new Thickness(20, 18, 20, 18);

            var summaryTextBox = ThemeManager.CreateStyledTextBox(true);
            summaryTextBox.Text = GetUninstallationSummary();
            summaryTextBox.IsReadOnly = true;
            summaryTextBox.TextWrapping = TextWrapping.Wrap;
            summaryTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            summaryTextBox.FontFamily = new FontFamily("Consolas");
            summaryTextBox.FontSize = 13;
            summaryTextBox.BorderThickness = new Thickness(0);

            summaryContainer.Child = summaryTextBox;

            Grid.SetRow(summaryLabel, 0);
            Grid.SetRow(summaryContainer, 1);
            contentGrid.Children.Add(summaryLabel);
            contentGrid.Children.Add(summaryContainer);
        }

        private void CreateUninstallingStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.uninstalling.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.uninstalling.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            uninstallStatusText = new TextBlock
            {
                Text = localization.GetString("uninstaller.uninstalling.preparing"),
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeManager.CurrentTheme.Foreground,
                FontSize = 15,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Progress bar container
            var progressContainer = ThemeManager.CreateStyledCard(new StackPanel(), 8, false);
            progressContainer.Padding = new Thickness(20, 18, 20, 18);
            progressContainer.Margin = new Thickness(0, 0, 0, 20);

            uninstallProgressBar = ThemeManager.CreateStyledProgressBar(0, 100, false);
            uninstallProgressBar.Height = 8;
            uninstallProgressBar.Value = 0; // Start at 0
            uninstallProgressBar.Foreground = ThemeManager.CurrentTheme.Danger; // Red for uninstall

            ((StackPanel)progressContainer.Child).Children.Add(uninstallProgressBar);

            // Log container
            var logContainer = ThemeManager.CreateStyledCard(new ListBox(), 8, false);
            logContainer.Background = ThemeManager.CurrentTheme.ConsoleBackground;
            logContainer.BorderBrush = ThemeManager.CurrentTheme.Border;
            logContainer.Padding = new Thickness(0);

            uninstallLogListBox = (ListBox)logContainer.Child;
            uninstallLogListBox.FontFamily = new FontFamily("Consolas");
            uninstallLogListBox.FontSize = 12;
            uninstallLogListBox.Background = ThemeManager.CurrentTheme.ConsoleBackground;
            uninstallLogListBox.Foreground = ThemeManager.CurrentTheme.ConsoleForeground;
            uninstallLogListBox.BorderThickness = new Thickness(0);
            uninstallLogListBox.Padding = new Thickness(15, 10, 15, 10);

            ScrollViewer.SetHorizontalScrollBarVisibility(uninstallLogListBox, ScrollBarVisibility.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(uninstallLogListBox, ScrollBarVisibility.Auto);

            // Style for log items
            var logItemStyle = new Style(typeof(ListBoxItem));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0, 2, 0, 2)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.MarginProperty, new Thickness(0)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, ThemeManager.CurrentTheme.ConsoleForeground));

            var hoverTrigger = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            logItemStyle.Triggers.Add(hoverTrigger);

            var selectedTrigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, ThemeManager.CurrentTheme.ConsoleForeground));
            logItemStyle.Triggers.Add(selectedTrigger);

            uninstallLogListBox.ItemContainerStyle = logItemStyle;

            Grid.SetRow(uninstallStatusText, 0);
            Grid.SetRow(progressContainer, 1);
            Grid.SetRow(logContainer, 2);
            
            contentGrid.Children.Add(uninstallStatusText);
            contentGrid.Children.Add(progressContainer);
            contentGrid.Children.Add(logContainer);
        }

        private void CreateFinishedStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.finished.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.finished.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var finishedPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var successIcon = new TextBlock
            {
                Text = "✅",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var successText = new TextBlock
            {
                Text = localization.GetString("uninstaller.finished.success_title"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = ThemeManager.CurrentTheme.Success
            };

            var finishedMessage = new TextBlock
            {
                Text = localization.GetString("uninstaller.finished.success_message"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                Margin = new Thickness(40, 0, 40, 20),
                Foreground = ThemeManager.CurrentTheme.ConsoleForeground
            };

            // Info panel with results
            var resultPanel = ThemeManager.CreateStyledCard(new StackPanel(), 8, false);
            resultPanel.Margin = new Thickness(20, 10, 20, 20);
            resultPanel.Padding = new Thickness(15);
            
            var resultContent = (StackPanel)resultPanel.Child;
            resultContent.Children.Add(new TextBlock
            {
                Text = localization.GetString("uninstaller.finished.summary_title"),
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeManager.CurrentTheme.Foreground,
                Margin = new Thickness(0, 0, 0, 10)
            });
            
            resultContent.Children.Add(new TextBlock
            {
                Text = GetUninstallationResultSummary(),
                Foreground = ThemeManager.CurrentTheme.ConsoleForeground,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                LineHeight = 18
            });

            finishedPanel.Children.Add(successIcon);
            finishedPanel.Children.Add(successText);
            finishedPanel.Children.Add(finishedMessage);
            finishedPanel.Children.Add(resultPanel);

            Grid.SetRow(finishedPanel, 0);
            contentGrid.Children.Add(finishedPanel);
        }

        private string GetInstallationSizeText()
        {
            try
            {
                if (string.IsNullOrEmpty(installationPath) || !Directory.Exists(installationPath))
                    return "";

                long totalSize = 0;
                var files = Directory.GetFiles(installationPath, "*", SearchOption.AllDirectories);
                
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch { }
                }

                if (totalSize >= 1024 * 1024 * 1024) // GB
                {
                    return $"{totalSize / (1024.0 * 1024.0 * 1024.0):F1} GB";
                }
                else if (totalSize >= 1024 * 1024) // MB
                {
                    return $"{totalSize / (1024.0 * 1024.0):F1} MB";
                }
                else if (totalSize >= 1024) // KB
                {
                    return $"{totalSize / 1024.0:F1} KB";
                }
                else
                {
                    return $"{totalSize} bytes";
                }
            }
            catch
            {
                return "";
            }
        }

        private string GetUninstallationSummary()
        {
            var summary = localization.GetString("uninstaller.ready_to_uninstall.components_header") + "\n\n";

            summary += $"{localization.GetString("uninstaller.ready_to_uninstall.installation_location")}\n  {(!string.IsNullOrEmpty(installationPath) ? installationPath : localization.GetString("uninstaller.ready_to_uninstall.not_found"))}\n\n";

            summary += localization.GetString("uninstaller.ready_to_uninstall.program_components") + "\n";
            summary += localization.GetString("uninstaller.ready_to_uninstall.executables") + "\n";
            summary += localization.GetString("uninstaller.ready_to_uninstall.libraries") + "\n";
            summary += localization.GetString("uninstaller.ready_to_uninstall.config_files") + "\n";
            summary += localization.GetString("uninstaller.ready_to_uninstall.documentation") + "\n\n";

            summary += localization.GetString("uninstaller.ready_to_uninstall.selected_options") + "\n\n";

            if (removeUserData)
                summary += localization.GetString("uninstaller.ready_to_uninstall.user_data_selected") + "\n";
            else
                summary += localization.GetString("uninstaller.ready_to_uninstall.user_data_preserved") + "\n";

            if (removeRegistry)
                summary += localization.GetString("uninstaller.ready_to_uninstall.registry_selected") + "\n";
            else
                summary += localization.GetString("uninstaller.ready_to_uninstall.registry_preserved") + "\n";

            if (removeShortcuts)
                summary += localization.GetString("uninstaller.ready_to_uninstall.shortcuts_selected") + "\n";
            else
                summary += localization.GetString("uninstaller.ready_to_uninstall.shortcuts_preserved") + "\n";

            if (removeFromPath)
                summary += localization.GetString("uninstaller.ready_to_uninstall.path_selected") + "\n";
            else
                summary += localization.GetString("uninstaller.ready_to_uninstall.path_preserved") + "\n";

            var sizeText = GetInstallationSizeText();
            if (!string.IsNullOrEmpty(sizeText))
            {
                summary += $"\n{localization.GetString("uninstaller.ready_to_uninstall.space_to_free", sizeText)}";
            }

            return summary;
        }

        private string GetUninstallationResultSummary()
        {
            var summary = "";

            if (!string.IsNullOrEmpty(installationPath))
                summary += localization.GetString("uninstaller.finished.files_removed", installationPath) + "\n";

            if (removeUserData)
                summary += localization.GetString("uninstaller.finished.user_data_removed") + "\n";

            if (removeRegistry)
                summary += localization.GetString("uninstaller.finished.registry_cleaned") + "\n";

            if (removeShortcuts)
                summary += localization.GetString("uninstaller.finished.shortcuts_removed") + "\n";

            if (removeFromPath)
                summary += localization.GetString("uninstaller.finished.path_removed") + "\n";

            summary += "\n" + localization.GetString("uninstaller.finished.system_clean");

            return summary;
        }

        private void AddUninstallationLog(string message)
        {
            if (uninstallLogListBox != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    uninstallLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                    uninstallLogListBox.ScrollIntoView(uninstallLogListBox.Items[uninstallLogListBox.Items.Count - 1]);
                });
            }
        }

        private async Task PerformUninstallation()
        {
            try
            {
                // Reset progress bar
                uninstallProgressBar.Value = 0;
                
                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.stopping_services");
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.starting"));
                
                // Step 1: Stop services
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.stopping_services"));
                await StopDevStackServices();
                uninstallProgressBar.Value = 15; // 15% complete

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.removing_shortcuts");
                uninstallProgressBar.Value = 25; // 25% complete
                
                // Step 2: Remove shortcuts (always executed)
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_shortcuts"));
                await RemoveShortcuts();

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.cleaning_registry");
                uninstallProgressBar.Value = 50; // 50% complete
                
                // Step 3: Clean registry (always executed)
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.cleaning_registry"));
                await CleanRegistry();

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.removing_path");
                uninstallProgressBar.Value = 65; // 65% complete
                
                // Step 4: Remove from PATH (always executed)
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_path"));
                await RemoveFromSystemPath();

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.removing_files");
                uninstallProgressBar.Value = 75; // 75% complete
                
                // Step 5: Remove files
                if (!string.IsNullOrEmpty(installationPath))
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_files", installationPath));
                    await RemoveFiles();
                }

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.finalizing");
                uninstallProgressBar.Value = 90; // 90% complete
                
                // Step 6: Clean user data (only if checkbox is checked)
                if (removeUserData)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_user_data"));
                    await RemoveUserData();
                }

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.completed");
                uninstallProgressBar.Value = 100; // 100% complete
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.uninstall_success"));

                await Task.Delay(1000);

                currentStep = UninstallerStep.Finished;
                UpdateStepContent();
            }
            catch (Exception ex)
            {
                AddUninstallationLog($"{localization.GetString("common.unknown")}: {ex.Message}");
                ThemeManager.CreateStyledMessageBox(
                    localization.GetString("uninstaller.dialogs.uninstall_error_message", ex.Message), 
                    localization.GetString("uninstaller.dialogs.uninstall_error_title"),
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
                
                uninstallStatusText.Text = localization.GetString("common.unknown");
                uninstallProgressBar.Value = 0;
                
                backButton.IsEnabled = true;
                cancelButton.IsEnabled = true;
            }
        }

        private async Task StopDevStackServices()
        {
            await Task.Run(() =>
            {
                try
                {
                    var processNames = new[] { "DevStack", "DevStackGUI", "DevStackCLI" };
                    
                    foreach (var processName in processNames)
                    {
                        var processes = Process.GetProcessesByName(processName);
                        foreach (var process in processes)
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit(5000);
                                AddUninstallationLog(localization.GetString("uninstaller.log_messages.process_stopped", processName));
                            }
                            catch (Exception ex)
                            {
                                AddUninstallationLog(localization.GetString("uninstaller.log_messages.process_stop_warning", processName, ex.Message));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.stop_services_error", ex.Message));
                }
            });
        }

        private async Task RemoveShortcuts()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Remove desktop shortcuts
                    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var shortcuts = new[] { "DevStack.lnk", "DevStack GUI.lnk", "DevStack CLI.lnk" };
                    
                    foreach (var shortcut in shortcuts)
                    {
                        var shortcutPath = Path.Combine(desktop, shortcut);
                        if (File.Exists(shortcutPath))
                        {
                            File.Delete(shortcutPath);
                            AddUninstallationLog(localization.GetString("uninstaller.log_messages.shortcut_removed", shortcut));
                        }
                    }

                    // Remove start menu shortcuts
                    var startMenuPaths = new[]
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
                    };

                    foreach (var startMenuPath in startMenuPaths)
                    {
                        var devstackFolder = Path.Combine(startMenuPath, "Programs", "DevStack Manager");
                        if (Directory.Exists(devstackFolder))
                        {
                            Directory.Delete(devstackFolder, true);
                            AddUninstallationLog(localization.GetString("uninstaller.log_messages.start_menu_removed", devstackFolder));
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.shortcuts_error", ex.Message));
                }
            });
        }

        private async Task CleanRegistry()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Remove DevStack registry entries
                    try
                    {
                        Registry.CurrentUser.DeleteSubKeyTree(@"Software\DevStack", false);
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.user_registry_removed"));
                    }
                    catch { }

                    try
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\DevStack", false);
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.machine_registry_removed"));
                    }
                    catch { }
                    
                    // Remove from Programs and Features
                    try
                    {
                        var uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DevStack";
                        Registry.LocalMachine.DeleteSubKeyTree(uninstallKey, false);
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.uninstall_registry_removed"));
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.registry_error", ex.Message));
                }
            });
        }

        private async Task RemoveFromSystemPath()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Remove from user PATH
                    var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                    if (userPath.Contains(installationPath))
                    {
                        var paths = userPath.Split(';').Where(p => !string.Equals(p, installationPath, StringComparison.OrdinalIgnoreCase)).ToArray();
                        var newPath = string.Join(";", paths);
                        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.user_path_removed"));
                    }

                    // Try to remove from system PATH (requires admin rights)
                    try
                    {
                        var systemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
                        if (systemPath.Contains(installationPath))
                        {
                            var paths = systemPath.Split(';').Where(p => !string.Equals(p, installationPath, StringComparison.OrdinalIgnoreCase)).ToArray();
                            var newPath = string.Join(";", paths);
                            Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine);
                            AddUninstallationLog(localization.GetString("uninstaller.log_messages.system_path_removed"));
                        }
                    }
                    catch
                    {
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.system_path_warning"));
                    }
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.path_error", ex.Message));
                }
            });
        }

        private async Task RemoveFiles()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(installationPath) || !Directory.Exists(installationPath))
                    {
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.install_not_found"));
                        return;
                    }

                    var uninstallerPath = Path.Combine(AppContext.BaseDirectory, "DevStack-Uninstaller.exe");
                    var toolsPath = Path.Combine(installationPath, "tools");
                    
                    // Remove all files except the uninstaller and tools folder (if UserData is not being removed)
                    var files = Directory.GetFiles(installationPath, "*", SearchOption.AllDirectories);
                    int removedFiles = 0;
                    int preservedFiles = 0;
                    
                    foreach (var file in files)
                    {
                        bool shouldPreserve = string.Equals(file, uninstallerPath, StringComparison.OrdinalIgnoreCase);
                        
                        // If UserData is not being removed, preserve tools folder content
                        if (!removeUserData && file.StartsWith(toolsPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        {
                            shouldPreserve = true;
                            preservedFiles++;
                        }
                        
                        if (!shouldPreserve)
                        {
                            try
                            {
                                File.Delete(file);
                                removedFiles++;
                            }
                            catch (Exception ex)
                            {
                                AddUninstallationLog(localization.GetString("uninstaller.log_messages.file_remove_warning", Path.GetFileName(file), ex.Message));
                            }
                        }
                    }

                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.files_removed_count", removedFiles.ToString()));
                    
                    if (!removeUserData && preservedFiles > 0)
                    {
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.tools_preserved", toolsPath));
                        AddUninstallationLog($"Preserved {preservedFiles} files in tools folder");
                    }

                    // Remove empty directories (but preserve tools folder if UserData is not being removed)
                    var directories = Directory.GetDirectories(installationPath, "*", SearchOption.AllDirectories);
                    int removedDirs = 0;
                    int preservedDirs = 0;
                    
                    foreach (var dir in directories.OrderByDescending(d => d.Length))
                    {
                        try
                        {
                            bool shouldPreserveDir = false;
                            
                            // If UserData is not being removed, preserve tools folder and its subdirectories
                            if (!removeUserData && (string.Equals(dir, toolsPath, StringComparison.OrdinalIgnoreCase) || 
                                                   dir.StartsWith(toolsPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                            {
                                shouldPreserveDir = true;
                                preservedDirs++;
                            }
                            
                            if (!shouldPreserveDir && !Directory.EnumerateFileSystemEntries(dir).Any())
                            {
                                Directory.Delete(dir);
                                removedDirs++;
                            }
                        }
                        catch { }
                    }

                    if (removedDirs > 0)
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.dirs_removed_count", removedDirs.ToString()));
                    
                    if (!removeUserData && preservedDirs > 0)
                        AddUninstallationLog($"Preserved {preservedDirs} directories in tools folder");

                    // Schedule removal of installation directory and uninstaller
                    ScheduleSelfDeletion().Wait();
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.files_error", ex.Message));
                }
            });
        }

        private async Task RemoveUserData()
        {
            await Task.Run(() =>
            {
                try
                {
                    var userDataPaths = new[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DevStack"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DevStack"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".devstack")
                    };

                    foreach (var dataPath in userDataPaths)
                    {
                        if (Directory.Exists(dataPath))
                        {
                            Directory.Delete(dataPath, true);
                            AddUninstallationLog(localization.GetString("uninstaller.log_messages.user_data_removed", dataPath));
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.user_data_error", ex.Message));
                }
            });
        }

        private async Task ScheduleSelfDeletion()
        {
            await Task.Run(() =>
            {
                try
                {
                    var currentPath = Path.Combine(AppContext.BaseDirectory, "DevStack-Uninstaller.exe");
                    
                    // Use PowerShell script for cleaner execution without visible windows
                    var psScript = $@"
# Wait for uninstaller to close
Start-Sleep -Seconds 2

# Attempt to delete the uninstaller
if (Test-Path '{currentPath}') {{
    try {{
        Remove-Item '{currentPath}' -Force -ErrorAction Stop
        Write-Host 'Uninstaller deleted successfully'
    }}
    catch {{
        # If first attempt fails, wait a bit more and try again
        Start-Sleep -Seconds 2
        try {{
            Remove-Item '{currentPath}' -Force -ErrorAction Stop
            Write-Host 'Uninstaller deleted on second attempt'
        }}
        catch {{
            Write-Host 'Failed to delete uninstaller: $_'
        }}
    }}
}} else {{
    Write-Host 'Uninstaller not found'
}}

# Clean exit
exit 0
";
                    var psPath = Path.Combine(Path.GetTempPath(), $"DevStackCleanup_{DateTime.Now:yyyyMMddHHmmss}.ps1");
                    File.WriteAllText(psPath, psScript, Encoding.UTF8);
                    
                    AddUninstallationLog($"Self-deletion script created: {psPath}");
                    
                    // Execute PowerShell script in completely hidden mode
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-WindowStyle Hidden -ExecutionPolicy Bypass -NoProfile -Command \"& '{psPath}'; Remove-Item '{psPath}' -Force -ErrorAction SilentlyContinue\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        RedirectStandardInput = false
                    };
                    
                    var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        AddUninstallationLog("Self-deletion PowerShell process started successfully");
                    }
                    else
                    {
                        AddUninstallationLog("Failed to start PowerShell self-deletion process");
                    }

                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.self_deletion_scheduled"));
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.self_deletion_warning", ex.Message));
                }
            });
        }
    }
}
