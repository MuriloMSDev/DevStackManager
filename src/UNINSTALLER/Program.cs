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
    /// <summary>
    /// Main program class for the DevStack uninstaller application.
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Entry point for the DevStack uninstaller application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
            {
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

    /// <summary>
    /// Main window class for the DevStack uninstaller wizard interface.
    /// </summary>
    public class UninstallerWindow : Window
    {
        #region Constants
        /// <summary>
        /// Default window width in pixels.
        /// </summary>
        private const double WINDOW_WIDTH = 750;
        
        /// <summary>
        /// Default window height in pixels.
        /// </summary>
        private const double WINDOW_HEIGHT = 650;
        
        /// <summary>
        /// Height of the header section in pixels.
        /// </summary>
        private const double HEADER_HEIGHT = 105;
        
        /// <summary>
        /// Height of the button bar section in pixels.
        /// </summary>
        private const double BUTTON_BAR_HEIGHT = 80;
        
        /// <summary>
        /// Standard content margin in pixels.
        /// </summary>
        private const double CONTENT_MARGIN = 20;
        
        /// <summary>
        /// Header horizontal margin in pixels.
        /// </summary>
        private const double HEADER_MARGIN = 25;
        
        /// <summary>
        /// Header vertical margin in pixels.
        /// </summary>
        private const double HEADER_VERTICAL_MARGIN = 20;
        
        /// <summary>
        /// Button panel horizontal margin in pixels.
        /// </summary>
        private const double BUTTON_PANEL_MARGIN = 25;
        
        /// <summary>
        /// Button panel vertical margin in pixels.
        /// </summary>
        private const double BUTTON_PANEL_VERTICAL_MARGIN = 18;
        
        /// <summary>
        /// Spacing between buttons in pixels.
        /// </summary>
        private const double BUTTON_SPACING = 12;
        
        /// <summary>
        /// Font size for title text.
        /// </summary>
        private const double TITLE_FONT_SIZE = 18;
        
        /// <summary>
        /// Font size for description text.
        /// </summary>
        private const double DESCRIPTION_FONT_SIZE = 13;
        
        /// <summary>
        /// Font size for label text.
        /// </summary>
        private const double LABEL_FONT_SIZE = 15;
        
        /// <summary>
        /// Font size for welcome screen title.
        /// </summary>
        private const double WELCOME_TITLE_FONT_SIZE = 28;
        
        /// <summary>
        /// Font size for version text.
        /// </summary>
        private const double VERSION_FONT_SIZE = 15;
        
        /// <summary>
        /// Font size for console output text.
        /// </summary>
        private const double CONSOLE_FONT_SIZE = 12;
        
        /// <summary>
        /// Standard font size for text.
        /// </summary>
        private const double TEXT_FONT_SIZE = 14;
        
        /// <summary>
        /// Width of Back button in pixels.
        /// </summary>
        private const double BACK_BUTTON_WIDTH = 90;
        
        /// <summary>
        /// Width of Next button in pixels.
        /// </summary>
        private const double NEXT_BUTTON_WIDTH = 130;
        
        /// <summary>
        /// Width of Cancel button in pixels.
        /// </summary>
        private const double CANCEL_BUTTON_WIDTH = 90;
        
        /// <summary>
        /// Height of buttons in pixels.
        /// </summary>
        private const double BUTTON_HEIGHT = 36;
        
        /// <summary>
        /// Width of step progress bar in pixels.
        /// </summary>
        private const double PROGRESS_BAR_WIDTH = 220;
        
        /// <summary>
        /// Height of step progress bar in pixels.
        /// </summary>
        private const double PROGRESS_BAR_HEIGHT = 6;
        
        /// <summary>
        /// Height of uninstallation progress bar in pixels.
        /// </summary>
        private const double UNINSTALL_PROGRESS_HEIGHT = 8;
        
        /// <summary>
        /// Size of logo icon in pixels.
        /// </summary>
        private const double LOGO_SIZE = 80;
        
        /// <summary>
        /// Font size for warning icon.
        /// </summary>
        private const double WARNING_ICON_FONT_SIZE = 48;
        
        /// <summary>
        /// Corner radius for card containers in pixels.
        /// </summary>
        private const double CARD_CORNER_RADIUS = 12;
        
        /// <summary>
        /// Corner radius for standard containers in pixels.
        /// </summary>
        private const double CONTAINER_CORNER_RADIUS = 8;
        
        /// <summary>
        /// Horizontal padding for card containers in pixels.
        /// </summary>
        private const double CARD_PADDING = 40;
        
        /// <summary>
        /// Vertical padding for card containers in pixels.
        /// </summary>
        private const double CARD_PADDING_VERTICAL = 35;
        
        /// <summary>
        /// Horizontal padding for standard containers in pixels.
        /// </summary>
        private const double CONTAINER_PADDING = 20;
        
        /// <summary>
        /// Vertical padding for standard containers in pixels.
        /// </summary>
        private const double CONTAINER_PADDING_VERTICAL = 18;
        
        /// <summary>
        /// Spacing between option elements in pixels.
        /// </summary>
        private const double OPTION_SPACING = 15;
        
        /// <summary>
        /// Vertical margin for welcome screen in pixels.
        /// </summary>
        private const double WELCOME_MARGIN_VERTICAL = 20;
        
        /// <summary>
        /// Bottom margin for title elements in pixels.
        /// </summary>
        private const double TITLE_MARGIN_BOTTOM = 8;
        
        /// <summary>
        /// Bottom margin for version text in pixels.
        /// </summary>
        private const double VERSION_MARGIN_BOTTOM = 25;
        
        /// <summary>
        /// Bottom margin for label elements in pixels.
        /// </summary>
        private const double LABEL_MARGIN_BOTTOM = 15;
        
        /// <summary>
        /// Bottom margin for container elements in pixels.
        /// </summary>
        private const double CONTAINER_MARGIN_BOTTOM = 20;
        
        /// <summary>
        /// Top margin for description text in pixels.
        /// </summary>
        private const double DESCRIPTION_MARGIN_TOP = 6;
        
        /// <summary>
        /// Top margin for info panel in pixels.
        /// </summary>
        private const double INFO_PANEL_MARGIN_TOP = 10;
        
        /// <summary>
        /// Progress percentage at uninstallation start (0%).
        /// </summary>
        private const int PROGRESS_START = 0;
        
        /// <summary>
        /// Progress percentage after registry cleanup (20%).
        /// </summary>
        private const int PROGRESS_REGISTRY_CLEANED = 20;
        
        /// <summary>
        /// Progress percentage after shortcuts removal (40%).
        /// </summary>
        private const int PROGRESS_SHORTCUTS_REMOVED = 40;
        
        /// <summary>
        /// Progress percentage after PATH cleanup (60%).
        /// </summary>
        private const int PROGRESS_PATH_CLEANED = 60;
        
        /// <summary>
        /// Progress percentage after files deletion (80%).
        /// </summary>
        private const int PROGRESS_FILES_DELETED = 80;
        
        /// <summary>
        /// Progress percentage for completed uninstallation (100%).
        /// </summary>
        private const int PROGRESS_COMPLETE = 100;
        
        /// <summary>
        /// Delay in milliseconds before showing completion screen.
        /// </summary>
        private const int COMPLETION_DELAY_MS = 1000;
        
        /// <summary>
        /// Delay in milliseconds between file deletion retry attempts.
        /// </summary>
        private const int FILE_DELETE_RETRY_DELAY_MS = 500;
        
        /// <summary>
        /// Maximum number of retry attempts for file deletion.
        /// </summary>
        private const int MAX_DELETE_RETRIES = 3;
        
        /// <summary>
        /// Delay in milliseconds for UI updates.
        /// </summary>
        private const int UI_UPDATE_DELAY_MS = 500;
        
        /// <summary>
        /// Left margin for checkbox description text in pixels.
        /// </summary>
        private const double CHECKBOX_DESCRIPTION_MARGIN_LEFT = 30;
        
        /// <summary>
        /// Top margin (negative) for checkbox description text in pixels.
        /// </summary>
        private const double CHECKBOX_DESCRIPTION_MARGIN_TOP = -10;
        
        /// <summary>
        /// Margin for language selector in pixels.
        /// </summary>
        private const double LANGUAGE_SELECTOR_MARGIN = 10;
        
        /// <summary>
        /// Normal line height for text in pixels.
        /// </summary>
        private const double LINE_HEIGHT_NORMAL = 22;
        
        /// <summary>
        /// Maximum width for description text in pixels.
        /// </summary>
        private const double MAX_DESCRIPTION_WIDTH = 420;
        
        /// <summary>
        /// Vertical padding for log items in pixels.
        /// </summary>
        private const double LOG_ITEM_PADDING_VERTICAL = 2;
        #endregion
        
        /// <summary>
        /// Localization manager for multi-language support.
        /// </summary>
        private readonly LocalizationManager localization = LocalizationManager.Instance!;
        
        /// <summary>
        /// Current uninstallation step in the wizard workflow.
        /// </summary>
        private UninstallerStep currentStep = UninstallerStep.Welcome;
        
        /// <summary>
        /// Main grid container for the window layout.
        /// </summary>
        private Grid mainGrid = null!;
        
        /// <summary>
        /// Grid container for the dynamic content area.
        /// </summary>
        private Grid contentGrid = null!;
        
        /// <summary>
        /// Text block displaying the current step title.
        /// </summary>
        private TextBlock stepTitleText = null!;
        
        /// <summary>
        /// Text block displaying the current step description.
        /// </summary>
        private TextBlock stepDescriptionText = null!;
        
        /// <summary>
        /// Back button to navigate to previous step.
        /// </summary>
        private Button backButton = null!;
        
        /// <summary>
        /// Next button to proceed to next step.
        /// </summary>
        private Button nextButton = null!;
        
        /// <summary>
        /// Cancel button to abort the uninstallation.
        /// </summary>
        private Button cancelButton = null!;
        
        /// <summary>
        /// Progress bar showing wizard step progress.
        /// </summary>
        private ProgressBar stepProgressBar = null!;
        
        /// <summary>
        /// Combo box for language selection.
        /// </summary>
        private ComboBox languageComboBox = null!;
        
        /// <summary>
        /// Label for language selection control.
        /// </summary>
        private Label languageLabel = null!;
        
        /// <summary>
        /// Panel containing language selection controls.
        /// </summary>
        private StackPanel languagePanel = null!;
        
        /// <summary>
        /// Path to the DevStack installation directory.
        /// </summary>
        private string installationPath = "";
        
        /// <summary>
        /// Indicates whether to remove user data during uninstallation.
        /// </summary>
        private bool removeUserData = false;
        
        /// <summary>
        /// Indicates whether to remove registry entries.
        /// </summary>
        private bool removeRegistry = true;
        
        /// <summary>
        /// Indicates whether to remove shortcuts.
        /// </summary>
        private bool removeShortcuts = true;
        
        /// <summary>
        /// Indicates whether to remove DevStack from system PATH.
        /// </summary>
        private bool removeFromPath = true;
        
        /// <summary>
        /// Check box for removing user data option.
        /// </summary>
        private CheckBox removeUserDataCheckBox = null!;
#pragma warning disable CS0414
        /// <summary>
        /// Check box for removing registry entries option.
        /// </summary>
        private CheckBox removeRegistryCheckBox = null!;
        
        /// <summary>
        /// Check box for removing shortcuts option.
        /// </summary>
        private CheckBox removeShortcutsCheckBox = null!;
        
        /// <summary>
        /// Check box for removing from PATH option.
        /// </summary>
        private CheckBox removeFromPathCheckBox = null!;
#pragma warning restore CS0414
        /// <summary>
        /// Progress bar showing uninstallation progress percentage.
        /// </summary>
        private ProgressBar uninstallProgressBar = null!;
        
        /// <summary>
        /// Text block displaying current uninstallation status message.
        /// </summary>
        private TextBlock uninstallStatusText = null!;
        
        /// <summary>
        /// List box displaying uninstallation log messages.
        /// </summary>
        private ListBox uninstallLogListBox = null!;

        /// <summary>
        /// Initializes a new instance of the UninstallerWindow class.
        /// </summary>
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

        /// <summary>
        /// Retrieves the version of the uninstaller from assembly attributes.
        /// </summary>
        /// <returns>The version string or "Unknown" if not available.</returns>
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

        /// <summary>
        /// Initializes the uninstaller window components, sets up the UI, and applies theme.
        /// </summary>
        private void InitializeComponent()
        {
            string version = GetVersion();
            
            System.Diagnostics.Debug.WriteLine($"Uninstaller version: {version}");
            
            Title = localization.GetString("uninstaller.window_title", version);
            System.Diagnostics.Debug.WriteLine($"Window title set to: {Title}");
            Width = WINDOW_WIDTH;
            Height = WINDOW_HEIGHT;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            
            ThemeManager.ApplyThemeToWindow(this);

            installationPath = GetInstallationPath();

            Closing += OnWindowClosing;

            CreateMainLayout();
            UpdateStepContent();
        }

        /// <summary>
        /// Creates the main grid layout with header, content, and button bar sections.
        /// </summary>
        private void CreateMainLayout()
        {
            mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HEADER_HEIGHT) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(BUTTON_BAR_HEIGHT) });

            CreateHeader();

            contentGrid = new Grid();
            contentGrid.Margin = new Thickness(CONTENT_MARGIN);
            Grid.SetRow(contentGrid, 1);
            mainGrid.Children.Add(contentGrid);

            CreateButtonBar();

            Content = mainGrid;
        }

        /// <summary>
        /// Creates the header section with step title, description, and progress bar.
        /// </summary>
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
                Margin = new Thickness(HEADER_MARGIN, HEADER_VERTICAL_MARGIN, HEADER_MARGIN, HEADER_VERTICAL_MARGIN),
                VerticalAlignment = VerticalAlignment.Center
            };

            stepTitleText = new TextBlock
            {
                FontSize = TITLE_FONT_SIZE,
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeManager.CurrentTheme.Foreground
            };

            stepDescriptionText = new TextBlock
            {
                FontSize = DESCRIPTION_FONT_SIZE,
                Foreground = ThemeManager.CurrentTheme.TextSecondary,
                Margin = new Thickness(0, DESCRIPTION_MARGIN_TOP, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            headerStackPanel.Children.Add(stepTitleText);
            headerStackPanel.Children.Add(stepDescriptionText);

            stepProgressBar = ThemeManager.CreateStyledProgressBar(0, 5, false);
            stepProgressBar.Width = PROGRESS_BAR_WIDTH;
            stepProgressBar.Height = PROGRESS_BAR_HEIGHT;
            stepProgressBar.Margin = new Thickness(HEADER_MARGIN, 0, HEADER_MARGIN, 0);
            stepProgressBar.VerticalAlignment = VerticalAlignment.Center;
            stepProgressBar.Foreground = ThemeManager.CurrentTheme.Danger;

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

        /// <summary>
        /// Creates the button bar with Back, Next/Uninstall/Finish, and Cancel buttons.
        /// </summary>
        private void CreateButtonBar()
        {
            var buttonBorder = new Border
            {
                Background = ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(0, 1, 0, 0)
            };

            var buttonGrid = new Grid();
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            languagePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(BUTTON_PANEL_MARGIN, BUTTON_PANEL_VERTICAL_MARGIN, BUTTON_PANEL_MARGIN, BUTTON_PANEL_VERTICAL_MARGIN)
            };

            languageLabel = ThemeManager.CreateStyledLabel(localization.GetString("uninstaller.welcome.language_label"), false, false, ThemeManager.LabelStyle.Secondary);
            languageLabel.FontSize = TEXT_FONT_SIZE;
            languageLabel.VerticalAlignment = VerticalAlignment.Center;
            languageLabel.Margin = new Thickness(0, 0, LANGUAGE_SELECTOR_MARGIN, 0);

            languageComboBox = ThemeManager.CreateStyledComboBox();

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

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(BUTTON_PANEL_MARGIN, BUTTON_PANEL_VERTICAL_MARGIN, BUTTON_PANEL_MARGIN, BUTTON_PANEL_VERTICAL_MARGIN)
            };

            backButton = ThemeManager.CreateStyledButton(localization.GetString("common.buttons.back"), null, ThemeManager.ButtonStyle.Secondary);
            backButton.Width = BACK_BUTTON_WIDTH;
            backButton.Height = BUTTON_HEIGHT;
            backButton.Margin = new Thickness(0, 0, BUTTON_SPACING, 0);
            backButton.IsEnabled = false;
            backButton.Click += BackButton_Click;

            nextButton = ThemeManager.CreateStyledButton(localization.GetString("common.buttons.next"), null, ThemeManager.ButtonStyle.Danger);
            nextButton.Width = NEXT_BUTTON_WIDTH;
            nextButton.Height = BUTTON_HEIGHT;
            nextButton.Margin = new Thickness(0, 0, BUTTON_SPACING, 0);
            nextButton.Click += NextButton_Click;

            cancelButton = ThemeManager.CreateStyledButton(localization.GetString("common.buttons.cancel"), null, ThemeManager.ButtonStyle.Secondary);
            cancelButton.Width = CANCEL_BUTTON_WIDTH;
            cancelButton.Height = BUTTON_HEIGHT;
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

        /// <summary>
        /// Handles language selection changes in the language combo box.
        /// </summary>
        /// <param name="sender">The combo box that triggered the event.</param>
        /// <param name="e">Event arguments containing selection change data.</param>
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (languageComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string languageCode)
            {
                LocalizationManager.ApplyLanguage(languageCode);
            }
        }

        /// <summary>
        /// Handles language changes by updating all localized UI elements.
        /// </summary>
        /// <param name="sender">The object that triggered the language change.</param>
        /// <param name="newLanguage">The new language code.</param>
        private void Localization_LanguageChanged(object? sender, string newLanguage)
        {
            Dispatcher.Invoke(() =>
            {
                var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStack-Uninstaller.exe");
                var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? localization.GetString("common.unknown");
                Title = localization.GetString("uninstaller.window_title", version);

                var mainGridRef = Content as Grid;
                if (mainGridRef != null)
                {
                    mainGridRef.Children.Clear();
                    mainGridRef.RowDefinitions.Clear();
                    mainGridRef.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HEADER_HEIGHT) });
                    mainGridRef.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    mainGridRef.RowDefinitions.Add(new RowDefinition { Height = new GridLength(BUTTON_BAR_HEIGHT) });

                    CreateHeader();
                    contentGrid = new Grid();
                    contentGrid.Margin = new Thickness(CONTENT_MARGIN);
                    Grid.SetRow(contentGrid, 1);
                    mainGridRef.Children.Add(contentGrid);
                    CreateButtonBar();
                    Content = mainGridRef;
                    UpdateStepContent();
                }
                else
                {
                    InitializeComponent();
                }
            });
        }

        /// <summary>
        /// Gets the DevStack installation path from registry or default location.
        /// </summary>
        /// <returns>The installation path string.</returns>
        private string GetInstallationPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\DevStack");
                if (key != null)
                {
                    var path = key.GetValue("InstallPath")?.ToString();
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        return path;
                    }
                }

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

        /// <summary>
        /// Updates the content area to display the current uninstaller step.
        /// </summary>
        private void UpdateStepContent()
        {
            contentGrid.Children.Clear();
            contentGrid.RowDefinitions.Clear();
            contentGrid.ColumnDefinitions.Clear();

            contentGrid.Background = ThemeManager.CurrentTheme.FormBackground;
            contentGrid.Margin = new Thickness(HEADER_MARGIN);

            stepProgressBar.Value = (int)currentStep;

            backButton.IsEnabled = currentStep != UninstallerStep.Welcome;
            cancelButton.Visibility = Visibility.Visible;
            
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

        /// <summary>
        /// Creates the welcome step UI with branding and uninstaller information.
        /// </summary>
        private void CreateWelcomeStep()
        {
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

            var welcomeContainer = ThemeManager.CreateStyledCard(new StackPanel(), CARD_CORNER_RADIUS, true);
            welcomeContainer.Padding = new Thickness(CARD_PADDING, CARD_PADDING_VERTICAL, CARD_PADDING, CARD_PADDING_VERTICAL);

            var innerPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var logoImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/DevStack.ico")),
                Width = LOGO_SIZE,
                Height = LOGO_SIZE,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, LABEL_MARGIN_BOTTOM)
            };

            var welcomeText = new TextBlock
            {
                Text = localization.GetString("uninstaller.welcome.app_name"),
                FontSize = WELCOME_TITLE_FONT_SIZE,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = ThemeManager.CurrentTheme.Danger,
                Margin = new Thickness(0, 0, 0, TITLE_MARGIN_BOTTOM)
            };

            var versionText = new TextBlock
            {
                Text = localization.GetString("uninstaller.welcome.version", GetVersion()),
                FontSize = VERSION_FONT_SIZE,
                Foreground = ThemeManager.CurrentTheme.TextSecondary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, VERSION_MARGIN_BOTTOM)
            };

            var descriptionText = new TextBlock
            {
                Text = localization.GetString("uninstaller.welcome.app_description"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = TEXT_FONT_SIZE,
                LineHeight = LINE_HEIGHT_NORMAL,
                Foreground = ThemeManager.CurrentTheme.TextSecondary,
                MaxWidth = MAX_DESCRIPTION_WIDTH
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

        /// <summary>
        /// Handles the Back button click to navigate to the previous uninstallation step.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > UninstallerStep.Welcome)
            {
                currentStep--;
                UpdateStepContent();
            }
        }

        /// <summary>
        /// Handles the Next/Uninstall/Finish button click to proceed with uninstallation or navigate steps.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
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
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(UI_UPDATE_DELAY_MS);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Application.Current.Shutdown();
                        });
                        
                        await Task.Delay(COMPLETION_DELAY_MS);
                        Environment.Exit(0);
                    });
                    break;
            }
        }

        /// <summary>
        /// Handles the Cancel button click to confirm and exit the uninstaller.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
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

        /// <summary>
        /// Handles the window closing event to prevent accidental closure during uninstallation.
        /// </summary>
        /// <param name="sender">The window that is closing.</param>
        /// <param name="e">Event arguments for the closing event.</param>
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

        /// <summary>
        /// Creates the UI for the confirmation step where users verify uninstallation details.
        /// </summary>
        private void CreateConfirmationStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.confirmation.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.confirmation.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var warningContainer = ThemeManager.CreateNotificationPanel(
                localization.GetString("uninstaller.confirmation.warning_text"), 
                ThemeManager.NotificationType.Warning, 
                true);
            warningContainer.Margin = new Thickness(0, 0, 0, CONTAINER_MARGIN_BOTTOM);

            var detailsContainer = ThemeManager.CreateStyledCard(new StackPanel(), CONTAINER_CORNER_RADIUS, false);
            detailsContainer.Background = ThemeManager.CurrentTheme.ConsoleBackground;
            detailsContainer.BorderBrush = ThemeManager.CurrentTheme.Border;

            var detailsPanel = (StackPanel)detailsContainer.Child;
            detailsPanel.Margin = new Thickness(CONTAINER_PADDING, CONTAINER_PADDING_VERTICAL, CONTAINER_PADDING, CONTAINER_PADDING_VERTICAL);

            if (!string.IsNullOrEmpty(installationPath))
            {
                detailsPanel.Children.Add(new TextBlock
                {
                    Text = localization.GetString("uninstaller.confirmation.install_found"),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeManager.CurrentTheme.Foreground,
                    FontSize = TEXT_FONT_SIZE,
                    Margin = new Thickness(0, 0, 0, TITLE_MARGIN_BOTTOM)
                });

                detailsPanel.Children.Add(new TextBlock
                {
                    Text = installationPath,
                    Foreground = ThemeManager.CurrentTheme.Accent,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = DESCRIPTION_FONT_SIZE,
                    Margin = new Thickness(CONTENT_MARGIN, 0, 0, LABEL_MARGIN_BOTTOM)
                });

                var sizeText = GetInstallationSizeText();
                if (!string.IsNullOrEmpty(sizeText))
                {
                    detailsPanel.Children.Add(new TextBlock
                    {
                        Text = localization.GetString("uninstaller.confirmation.space_to_free", sizeText),
                        Foreground = ThemeManager.CurrentTheme.TextSecondary,
                        FontSize = DESCRIPTION_FONT_SIZE,
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
                    FontSize = TEXT_FONT_SIZE,
                    Margin = new Thickness(0, 0, 0, INFO_PANEL_MARGIN_TOP)
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

        /// <summary>
        /// Creates the UI for the uninstall options step where users choose what to remove.
        /// </summary>
        private void CreateUninstallOptionsStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.uninstall_options.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.uninstall_options.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var optionsLabel = ThemeManager.CreateStyledLabel(
                localization.GetString("uninstaller.uninstall_options.label"), 
                false, false, ThemeManager.LabelStyle.Title);
            optionsLabel.FontSize = LABEL_FONT_SIZE;
            optionsLabel.Margin = new Thickness(0, 0, 0, LABEL_MARGIN_BOTTOM);

            var optionsContainer = ThemeManager.CreateStyledCard(new StackPanel(), CONTAINER_CORNER_RADIUS, false);
            optionsContainer.Padding = new Thickness(CONTAINER_PADDING, CONTAINER_PADDING_VERTICAL, CONTAINER_PADDING, CONTAINER_PADDING_VERTICAL);

            var optionsPanel = (StackPanel)optionsContainer.Child;

            removeUserDataCheckBox = ThemeManager.CreateStyledCheckBox(
                localization.GetString("uninstaller.uninstall_options.user_data"), removeUserData);
            removeUserDataCheckBox.IsChecked = removeUserData;
            removeUserDataCheckBox.Checked += (s, e) => removeUserData = true;
            removeUserDataCheckBox.Unchecked += (s, e) => removeUserData = false;
            removeUserDataCheckBox.Margin = new Thickness(0, 0, 0, LABEL_MARGIN_BOTTOM);

            var userDataDescription = new TextBlock
            {
                Text = localization.GetString("uninstaller.uninstall_options.user_data_desc"),
                FontSize = CONSOLE_FONT_SIZE,
                Foreground = ThemeManager.CurrentTheme.TextMuted,
                Margin = new Thickness(CHECKBOX_DESCRIPTION_MARGIN_LEFT, CHECKBOX_DESCRIPTION_MARGIN_TOP, 0, 0)
            };

            optionsPanel.Children.Add(removeUserDataCheckBox);
            optionsPanel.Children.Add(userDataDescription);

            var infoPanel = ThemeManager.CreateNotificationPanel(
                localization.GetString("uninstaller.uninstall_options.info"), 
                ThemeManager.NotificationType.Info
            );
            infoPanel.Margin = new Thickness(0, CONTAINER_MARGIN_BOTTOM, 0, 0);

            Grid.SetRow(optionsLabel, 0);
            Grid.SetRow(optionsContainer, 1);
            
            var containerPanel = new StackPanel();
            containerPanel.Children.Add(optionsContainer);
            containerPanel.Children.Add(infoPanel);
            
            Grid.SetRow(containerPanel, 1);
            contentGrid.Children.Add(optionsLabel);
            contentGrid.Children.Add(containerPanel);
        }

        /// <summary>
        /// Creates the UI for the ready to uninstall step showing final summary before uninstallation.
        /// </summary>
        private void CreateReadyToUninstallStep()
        {
            stepTitleText.Text = localization.GetString("uninstaller.ready_to_uninstall.title");
            stepDescriptionText.Text = localization.GetString("uninstaller.ready_to_uninstall.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var summaryLabel = ThemeManager.CreateStyledLabel(
                localization.GetString("uninstaller.ready_to_uninstall.summary_label"), 
                false, false, ThemeManager.LabelStyle.Title);
            summaryLabel.FontSize = LABEL_FONT_SIZE;
            summaryLabel.Margin = new Thickness(0, 0, 0, LABEL_MARGIN_BOTTOM);

            var summaryContainer = ThemeManager.CreateStyledCard(new StackPanel(), CONTAINER_CORNER_RADIUS, false);
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

        /// <summary>
        /// Creates the UI for the uninstalling step showing progress and log output.
        /// </summary>
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
                FontSize = LABEL_FONT_SIZE,
                Margin = new Thickness(0, 0, 0, LABEL_MARGIN_BOTTOM)
            };

            var progressContainer = ThemeManager.CreateStyledCard(new StackPanel(), CONTAINER_CORNER_RADIUS, false);
            progressContainer.Padding = new Thickness(CONTAINER_PADDING, CONTAINER_PADDING_VERTICAL, CONTAINER_PADDING, CONTAINER_PADDING_VERTICAL);
            progressContainer.Margin = new Thickness(0, 0, 0, CONTAINER_MARGIN_BOTTOM);

            uninstallProgressBar = ThemeManager.CreateStyledProgressBar(PROGRESS_START, PROGRESS_COMPLETE, false);
            uninstallProgressBar.Height = UNINSTALL_PROGRESS_HEIGHT;
            uninstallProgressBar.Value = PROGRESS_START;
            uninstallProgressBar.Foreground = ThemeManager.CurrentTheme.Danger;

            ((StackPanel)progressContainer.Child).Children.Add(uninstallProgressBar);

            var logContainer = ThemeManager.CreateStyledCard(new ListBox(), CONTAINER_CORNER_RADIUS, false);
            logContainer.Background = ThemeManager.CurrentTheme.ConsoleBackground;
            logContainer.BorderBrush = ThemeManager.CurrentTheme.Border;
            logContainer.Padding = new Thickness(0);

            uninstallLogListBox = (ListBox)logContainer.Child;
            uninstallLogListBox.FontFamily = new FontFamily("Consolas");
            uninstallLogListBox.FontSize = CONSOLE_FONT_SIZE;
            uninstallLogListBox.Background = ThemeManager.CurrentTheme.ConsoleBackground;
            uninstallLogListBox.Foreground = ThemeManager.CurrentTheme.ConsoleForeground;
            uninstallLogListBox.BorderThickness = new Thickness(0);
            uninstallLogListBox.Padding = new Thickness(LABEL_MARGIN_BOTTOM, INFO_PANEL_MARGIN_TOP, LABEL_MARGIN_BOTTOM, INFO_PANEL_MARGIN_TOP);

            ScrollViewer.SetHorizontalScrollBarVisibility(uninstallLogListBox, ScrollBarVisibility.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(uninstallLogListBox, ScrollBarVisibility.Auto);

            var logItemStyle = new Style(typeof(ListBoxItem));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0, LOG_ITEM_PADDING_VERTICAL, 0, LOG_ITEM_PADDING_VERTICAL)));
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

        /// <summary>
        /// Creates the UI for the finished step showing uninstallation completion status.
        /// </summary>
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
                FontSize = WARNING_ICON_FONT_SIZE,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, CONTAINER_MARGIN_BOTTOM)
            };

            var successText = new TextBlock
            {
                Text = localization.GetString("uninstaller.finished.success_title"),
                FontSize = TITLE_FONT_SIZE,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, CONTAINER_MARGIN_BOTTOM),
                Foreground = ThemeManager.CurrentTheme.Success
            };

            var finishedMessage = new TextBlock
            {
                Text = localization.GetString("uninstaller.finished.success_message"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = TEXT_FONT_SIZE,
                Margin = new Thickness(CARD_PADDING, 0, CARD_PADDING, CONTAINER_MARGIN_BOTTOM),
                Foreground = ThemeManager.CurrentTheme.ConsoleForeground
            };

            var resultPanel = ThemeManager.CreateStyledCard(new StackPanel(), CONTAINER_CORNER_RADIUS, false);
            resultPanel.Margin = new Thickness(CONTENT_MARGIN, INFO_PANEL_MARGIN_TOP, CONTENT_MARGIN, CONTAINER_MARGIN_BOTTOM);
            resultPanel.Padding = new Thickness(LABEL_MARGIN_BOTTOM);
            
            var resultContent = (StackPanel)resultPanel.Child;
            resultContent.Children.Add(new TextBlock
            {
                Text = localization.GetString("uninstaller.finished.summary_title"),
                FontWeight = FontWeights.SemiBold,
                Foreground = ThemeManager.CurrentTheme.Foreground,
                Margin = new Thickness(0, 0, 0, INFO_PANEL_MARGIN_TOP)
            });
            
            resultContent.Children.Add(new TextBlock
            {
                Text = GetUninstallationResultSummary(),
                Foreground = ThemeManager.CurrentTheme.ConsoleForeground,
                FontFamily = new FontFamily("Consolas"),
                FontSize = CONSOLE_FONT_SIZE,
                LineHeight = 18
            });

            finishedPanel.Children.Add(successIcon);
            finishedPanel.Children.Add(successText);
            finishedPanel.Children.Add(finishedMessage);
            finishedPanel.Children.Add(resultPanel);

            Grid.SetRow(finishedPanel, 0);
            contentGrid.Children.Add(finishedPanel);
        }

        /// <summary>
        /// Gets formatted text showing the installation size on disk.
        /// </summary>
        /// <returns>Formatted size string or empty string if unable to calculate.</returns>
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

                if (totalSize >= 1024 * 1024 * 1024)
                {
                    return $"{totalSize / (1024.0 * 1024.0 * 1024.0):F1} GB";
                }
                else if (totalSize >= 1024 * 1024)
                {
                    return $"{totalSize / (1024.0 * 1024.0):F1} MB";
                }
                else if (totalSize >= 1024)
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

        /// <summary>
        /// Gets formatted summary text of what will be uninstalled based on user selections.
        /// </summary>
        /// <returns>Formatted uninstallation summary string.</returns>
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

        /// <summary>
        /// Gets formatted summary of what was actually uninstalled after completion.
        /// </summary>
        /// <returns>Formatted uninstallation result string.</returns>
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

        /// <summary>
        /// Adds a message to the uninstallation log display.
        /// </summary>
        /// <param name="message">The message to add to the log.</param>
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

        /// <summary>
        /// Performs the actual uninstallation process asynchronously.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task PerformUninstallation()
        {
            try
            {
                uninstallProgressBar.Value = 0;
                
                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.stopping_services");
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.starting"));
                
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.stopping_services"));
                await StopDevStackServices();
                uninstallProgressBar.Value = 15;

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.removing_shortcuts");
                uninstallProgressBar.Value = 25;
                
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_shortcuts"));
                await RemoveShortcuts();

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.cleaning_registry");
                uninstallProgressBar.Value = 50;
                
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.cleaning_registry"));
                await CleanRegistry();

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.removing_path");
                uninstallProgressBar.Value = 65;
                
                AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_path"));
                await RemoveFromSystemPath();

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.removing_files");
                uninstallProgressBar.Value = 75;
                
                if (!string.IsNullOrEmpty(installationPath))
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_files", installationPath));
                    await RemoveFiles();
                }

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.finalizing");
                uninstallProgressBar.Value = 90;
                
                if (removeUserData)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.removing_user_data"));
                    await RemoveUserData();
                }

                uninstallStatusText.Text = localization.GetString("uninstaller.uninstalling.completed");
                uninstallProgressBar.Value = 100;
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

        /// <summary>
        /// Stops all running DevStack processes and services.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Removes DevStack shortcuts from desktop and start menu.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task RemoveShortcuts()
        {
            await Task.Run(() =>
            {
                try
                {
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

        /// <summary>
        /// Removes DevStack registry entries from Windows registry.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task CleanRegistry()
        {
            await Task.Run(() =>
            {
                try
                {
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

        /// <summary>
        /// Removes DevStack installation path from system PATH environment variable.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task RemoveFromSystemPath()
        {
            await Task.Run(() =>
            {
                try
                {
                    var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                    if (userPath.Contains(installationPath))
                    {
                        var paths = userPath.Split(';').Where(p => !string.Equals(p, installationPath, StringComparison.OrdinalIgnoreCase)).ToArray();
                        var newPath = string.Join(";", paths);
                        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                        AddUninstallationLog(localization.GetString("uninstaller.log_messages.user_path_removed"));
                    }

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

        /// <summary>
        /// Removes DevStack installation files and directories from disk.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
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
                    
                    var files = Directory.GetFiles(installationPath, "*", SearchOption.AllDirectories);
                    int removedFiles = 0;
                    int preservedFiles = 0;
                    
                    foreach (var file in files)
                    {
                        bool shouldPreserve = string.Equals(file, uninstallerPath, StringComparison.OrdinalIgnoreCase);
                        
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

                    var directories = Directory.GetDirectories(installationPath, "*", SearchOption.AllDirectories);
                    int removedDirs = 0;
                    int preservedDirs = 0;
                    
                    foreach (var dir in directories.OrderByDescending(d => d.Length))
                    {
                        try
                        {
                            bool shouldPreserveDir = false;
                            
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

                    ScheduleSelfDeletion().Wait();
                }
                catch (Exception ex)
                {
                    AddUninstallationLog(localization.GetString("uninstaller.log_messages.files_error", ex.Message));
                }
            });
        }

        /// <summary>
        /// Removes user-specific DevStack data from AppData and user profile folders.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Schedules the uninstaller executable to delete itself after completion using PowerShell.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task ScheduleSelfDeletion()
        {
            await Task.Run(() =>
            {
                try
                {
                    var currentPath = Path.Combine(AppContext.BaseDirectory, "DevStack-Uninstaller.exe");
                    
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
