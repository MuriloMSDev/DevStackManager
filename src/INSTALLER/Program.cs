using DevStackManager;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using DevStackShared;
using System.Net.Http;

namespace DevStackInstaller
{
    /// <summary>
    /// Main program class for the DevStack installer application.
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Entry point for the DevStack installer application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
            {
                var locManager = LocalizationManager.Initialize(ApplicationType.Installer);
                
                var availableLanguages = locManager.GetAvailableLanguages();
                System.Diagnostics.Debug.WriteLine($"Available languages: {string.Join(", ", availableLanguages)}");
                
                var app = new Application();
                app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                
                var window = new InstallerWindow();
                app.MainWindow = window;
                
                app.Run(window);
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.Instance?.GetString("installer.dialogs.startup_error_message", ex.Message, ex) ?? $"Error starting installer: {ex.Message}\n\nDetails: {ex}", 
                    LocalizationManager.Instance?.GetString("installer.dialogs.startup_error_title") ?? "DevStack Installer Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public enum InstallerStep
    {
        Welcome,
        License,
        InstallationPath,
        Components,
        ReadyToInstall,
        Installing,
        Finished
    }

    /// <summary>
    /// Main window class for the DevStack installer wizard interface.
    /// </summary>
    public class InstallerWindow : Window
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
        /// Font size for button text.
        /// </summary>
        private const double BUTTON_FONT_SIZE = 14;
        
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
        /// Width of Browse button in pixels.
        /// </summary>
        private const double BROWSE_BUTTON_WIDTH = 130;
        
        /// <summary>
        /// Height of input fields in pixels.
        /// </summary>
        private const double INPUT_HEIGHT = 40;
        
        /// <summary>
        /// Width of step progress bar in pixels.
        /// </summary>
        private const double PROGRESS_BAR_WIDTH = 220;
        
        /// <summary>
        /// Height of step progress bar in pixels.
        /// </summary>
        private const double PROGRESS_BAR_HEIGHT = 6;
        
        /// <summary>
        /// Height of installation progress bar in pixels.
        /// </summary>
        private const double INSTALL_PROGRESS_HEIGHT = 8;
        
        /// <summary>
        /// Size of logo icon in pixels.
        /// </summary>
        private const double LOGO_SIZE = 80;
        
        /// <summary>
        /// Font size for success icon.
        /// </summary>
        private const double SUCCESS_ICON_FONT_SIZE = 48;
        
        /// <summary>
        /// Corner radius for card containers in pixels.
        /// </summary>
        private const double CARD_CORNER_RADIUS = 12;
        
        /// <summary>
        /// Corner radius for standard containers in pixels.
        /// </summary>
        private const double CONTAINER_CORNER_RADIUS = 8;
        
        /// <summary>
        /// Corner radius for small containers in pixels.
        /// </summary>
        private const double CONTAINER_CORNER_RADIUS_SMALL = 6;
        
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
        /// Progress percentage after ZIP extraction (5%).
        /// </summary>
        private const int PROGRESS_ZIP_EXTRACTED = 5;
        
        /// <summary>
        /// Progress percentage after source extraction (10%).
        /// </summary>
        private const int PROGRESS_SOURCE_EXTRACTED = 10;
        
        /// <summary>
        /// Progress percentage after SDK download (35%).
        /// </summary>
        private const int PROGRESS_SDK_DOWNLOADED = 35;
        
        /// <summary>
        /// Progress percentage after directory creation (40%).
        /// </summary>
        private const int PROGRESS_DIR_CREATED = 40;
        
        /// <summary>
        /// Progress percentage after compilation (85%).
        /// </summary>
        private const int PROGRESS_COMPILED = 85;
        
        /// <summary>
        /// Progress percentage after registry setup (90%).
        /// </summary>
        private const int PROGRESS_REGISTERED = 90;
        
        /// <summary>
        /// Progress percentage after desktop shortcuts creation (95%).
        /// </summary>
        private const int PROGRESS_DESKTOP_SHORTCUTS = 95;
        
        /// <summary>
        /// Progress percentage after Start Menu shortcuts creation (97%).
        /// </summary>
        private const int PROGRESS_START_MENU = 97;
        
        /// <summary>
        /// Progress percentage after PATH addition (99%).
        /// </summary>
        private const int PROGRESS_PATH_ADDED = 99;
        
        /// <summary>
        /// Progress percentage for completed installation (100%).
        /// </summary>
        private const int PROGRESS_COMPLETE = 100;
        
        /// <summary>
        /// Delay in milliseconds before showing completion screen.
        /// </summary>
        private const int COMPLETION_DELAY_MS = 1000;
        
        /// <summary>
        /// Length of shortened GUID used for temporary paths.
        /// </summary>
        private const int GUID_SHORT_LENGTH = 8;
        
        /// <summary>
        /// Timeout in minutes for .NET SDK download.
        /// </summary>
        private const int SDK_DOWNLOAD_TIMEOUT_MINUTES = 15;
        
        /// <summary>
        /// Minimum file size in MB for SDK validation.
        /// </summary>
        private const int SDK_MIN_FILE_SIZE_MB = 1;
        
        /// <summary>
        /// Timeout in minutes for install script execution.
        /// </summary>
        private const int INSTALL_SCRIPT_TIMEOUT_MINUTES = 5;
        
        /// <summary>
        /// Buffer size for file operations in bytes.
        /// </summary>
        private const int BUFFER_SIZE = 8192;
        #endregion
        
        /// <summary>
        /// Current installation step in the wizard workflow.
        /// </summary>
        private InstallerStep currentStep = InstallerStep.Welcome;
        
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
        /// Cancel button to abort the installation.
        /// </summary>
        private Button cancelButton = null!;
        
        /// <summary>
        /// Progress bar showing wizard step progress.
        /// </summary>
        private ProgressBar stepProgressBar = null!;
        
        /// <summary>
        /// Localization manager for multi-language support.
        /// </summary>
        private LocalizationManager localization = LocalizationManager.Instance!;
        
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
        /// Target directory path for installation.
        /// </summary>
        private string installationPath = "";
        
        /// <summary>
        /// Indicates whether to create desktop shortcuts.
        /// </summary>
        private bool createDesktopShortcuts = true;
        
        /// <summary>
        /// Indicates whether to create Start Menu shortcuts.
        /// </summary>
        private bool createStartMenuShortcuts = true;
        
        /// <summary>
        /// Indicates whether to add DevStack to system PATH.
        /// </summary>
        private bool addToPath = false;
        
        /// <summary>
        /// Indicates whether to launch DevStack after installation completes.
        /// </summary>
        private bool launchAfterInstall = true;
        
        /// <summary>
        /// Text box for entering installation path.
        /// </summary>
        private TextBox pathTextBox = null!;
        
        /// <summary>
        /// Check box for desktop shortcuts option.
        /// </summary>
        private CheckBox desktopShortcutCheckBox = null!;
        
        /// <summary>
        /// Check box for Start Menu shortcuts option.
        /// </summary>
        private CheckBox startMenuShortcutCheckBox = null!;
        
        /// <summary>
        /// Check box for adding to PATH option.
        /// </summary>
        private CheckBox addToPathCheckBox = null!;
        
        /// <summary>
        /// Progress bar showing installation progress percentage.
        /// </summary>
        private ProgressBar installProgressBar = null!;
        
        /// <summary>
        /// Label displaying current installation status message.
        /// </summary>
        private Label installStatusText = null!;
        
        /// <summary>
        /// List box displaying installation log messages.
        /// </summary>
        private ListBox installLogListBox = null!;

        /// <summary>
        /// Initializes a new instance of the InstallerWindow class.
        /// </summary>
        public InstallerWindow()
        {
            try
            {
                localization.LanguageChanged += Localization_LanguageChanged;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(localization.GetString("installer.dialogs.initialization_error_message", ex.Message), 
                    localization.GetString("installer.dialogs.initialization_error_title"), MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the version of the installer from assembly attributes.
        /// </summary>
        /// <returns>The version string or "Unknown" if not available.</returns>
        private string GetVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>()?.Version;
                return version ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Extracts the embedded DevStackSource.zip resource to a temporary file.
        /// </summary>
        /// <returns>The path to the extracted temporary zip file.</returns>
        private string ExtractEmbeddedZip()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            var resourceNames = assembly.GetManifestResourceNames();
            var zipResourceName = resourceNames.FirstOrDefault(r => r.EndsWith("DevStackSource.zip"));
            
            if (zipResourceName == null)
            {
                throw new Exception("Embedded DevStackSource.zip resource not found in installer.");
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "DevStackSource_" + Guid.NewGuid().ToString("N")[..8] + ".zip");
            
            using (var resourceStream = assembly.GetManifestResourceStream(zipResourceName))
            {
                if (resourceStream == null)
                {
                    throw new Exception("Could not access embedded DevStackSource.zip resource.");
                }
                
                using (var fileStream = File.Create(tempPath))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
            
            return tempPath;
        }

        /// <summary>
        /// Initializes the installer window components, sets up the UI, and applies theme.
        /// </summary>
        private void InitializeComponent()
        {
            string version = GetVersion();
            
            System.Diagnostics.Debug.WriteLine($"Installer version: {version}");
            
            Title = localization.GetString("installer.window_title", version);
            System.Diagnostics.Debug.WriteLine($"Window title set to: {Title}");
            Width = WINDOW_WIDTH;
            Height = WINDOW_HEIGHT;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            
            DevStackShared.ThemeManager.ApplyThemeToWindow(this);

            installationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DevStack");

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
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
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
                Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground
            };

            stepDescriptionText = new TextBlock
            {
                FontSize = DESCRIPTION_FONT_SIZE,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.TextSecondary,
                Margin = new Thickness(0, DESCRIPTION_MARGIN_TOP, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            headerStackPanel.Children.Add(stepTitleText);
            headerStackPanel.Children.Add(stepDescriptionText);

            stepProgressBar = DevStackShared.ThemeManager.CreateStyledProgressBar(0, 6, false);
            stepProgressBar.Width = PROGRESS_BAR_WIDTH;
            stepProgressBar.Height = PROGRESS_BAR_HEIGHT;
            stepProgressBar.Margin = new Thickness(HEADER_MARGIN, 0, HEADER_MARGIN, 0);
            stepProgressBar.VerticalAlignment = VerticalAlignment.Center;

            Grid.SetColumn(headerStackPanel, 0);
            Grid.SetColumn(stepProgressBar, 1);
            headerGrid.Children.Add(headerStackPanel);
            headerGrid.Children.Add(stepProgressBar);

            headerBorder.Child = headerGrid;
            Grid.SetRow(headerBorder, 0);
            mainGrid.Children.Add(headerBorder);
        }

        /// <summary>
        /// Creates the button bar with Back, Next/Install/Finish, and Cancel buttons.
        /// </summary>
        private void CreateButtonBar()
        {
            var buttonBorder = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
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

            languageLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.welcome.language_label"), 
                false, true, DevStackShared.ThemeManager.LabelStyle.Secondary);
            languageLabel.VerticalAlignment = VerticalAlignment.Center;
            languageLabel.Margin = new Thickness(0, 0, 10, 0);

            languageComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();

            var languages = localization.GetAvailableLanguages();
            foreach (var lang in languages)
            {
                var langName = localization.GetLanguageName(lang);
                var item = new ComboBoxItem
                {
                    Content = langName,
                    Tag = lang,
                    Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground
                };
                languageComboBox.Items.Add(item);

                if (lang == localization.CurrentLanguage)
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

            backButton = DevStackShared.ThemeManager.CreateStyledButton(
                localization.GetString("common.buttons.back"), 
                BackButton_Click, 
                DevStackShared.ThemeManager.ButtonStyle.Secondary);
            backButton.Width = BACK_BUTTON_WIDTH;
            backButton.Height = BUTTON_HEIGHT;
            backButton.Margin = new Thickness(0, 0, BUTTON_SPACING, 0);
            backButton.IsEnabled = false;

            nextButton = DevStackShared.ThemeManager.CreateStyledButton(
                localization.GetString("common.buttons.next"), 
                NextButton_Click, 
                DevStackShared.ThemeManager.ButtonStyle.Primary);
            nextButton.Width = NEXT_BUTTON_WIDTH;
            nextButton.Height = BUTTON_HEIGHT;
            nextButton.Margin = new Thickness(0, 0, BUTTON_SPACING, 0);

            cancelButton = DevStackShared.ThemeManager.CreateStyledButton(
                localization.GetString("common.buttons.cancel"), 
                CancelButton_Click, 
                DevStackShared.ThemeManager.ButtonStyle.Secondary);
            cancelButton.Width = CANCEL_BUTTON_WIDTH;
            cancelButton.Height = BUTTON_HEIGHT;

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
        /// Updates the content area based on the current installation step.
        /// </summary>
        private void UpdateStepContent()
        {
            contentGrid.Children.Clear();
            contentGrid.RowDefinitions.Clear();
            contentGrid.ColumnDefinitions.Clear();

            contentGrid.Background = DevStackShared.ThemeManager.CurrentTheme.FormBackground;
            contentGrid.Margin = new Thickness(HEADER_MARGIN);

            stepProgressBar.Value = (int)currentStep;

            backButton.IsEnabled = currentStep != InstallerStep.Welcome;
            cancelButton.Visibility = Visibility.Visible;
            
            languagePanel.Visibility = currentStep == InstallerStep.Welcome ? Visibility.Visible : Visibility.Collapsed;
            
            switch (currentStep)
            {
                case InstallerStep.Welcome:
                    CreateWelcomeStep();
                    nextButton.Content = localization.GetString("common.buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.License:
                    CreateLicenseStep();
                    nextButton.Content = localization.GetString("common.buttons.accept");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.InstallationPath:
                    CreateInstallationPathStep();
                    nextButton.Content = localization.GetString("common.buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.Components:
                    CreateComponentsStep();
                    nextButton.Content = localization.GetString("common.buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.ReadyToInstall:
                    CreateReadyToInstallStep();
                    nextButton.Content = localization.GetString("common.buttons.install");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.Installing:
                    CreateInstallingStep();
                    nextButton.IsEnabled = false;
                    backButton.IsEnabled = false;
                    break;
                case InstallerStep.Finished:
                    CreateFinishedStep();
                    nextButton.Content = localization.GetString("common.buttons.finish");
                    nextButton.IsEnabled = true;
                    backButton.IsEnabled = false;
                    cancelButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Creates the welcome step UI with branding and version information.
        /// </summary>
        private void CreateWelcomeStep()
        {
            System.Diagnostics.Debug.WriteLine("=========== CREATING WELCOME STEP ===========");
            
            var title = localization.GetString("installer.welcome.title");
            System.Diagnostics.Debug.WriteLine($"installer.welcome.title = \"{title}\"");
            stepTitleText.Text = title;
            
            var description = localization.GetString("installer.welcome.description");
            System.Diagnostics.Debug.WriteLine($"installer.welcome.description = \"{description}\"");
            stepDescriptionText.Text = description;

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var welcomePanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                Margin = new Thickness(0, WELCOME_MARGIN_VERTICAL, 0, WELCOME_MARGIN_VERTICAL)
            };

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
                Margin = new Thickness(0)
            };

            var welcomeText = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.welcome.app_name"), 
                true, false, DevStackShared.ThemeManager.LabelStyle.Title);
            welcomeText.HorizontalAlignment = HorizontalAlignment.Center;
            welcomeText.FontSize = WELCOME_TITLE_FONT_SIZE;
            welcomeText.Margin = new Thickness(0, 0, 0, TITLE_MARGIN_BOTTOM);

            var versionText = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.welcome.version", GetVersion()),
                false, false, DevStackShared.ThemeManager.LabelStyle.Secondary);
            versionText.HorizontalAlignment = HorizontalAlignment.Center;
            versionText.FontSize = VERSION_FONT_SIZE;
            versionText.Margin = new Thickness(0, 0, 0, VERSION_MARGIN_BOTTOM);

            var descriptionText = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.welcome.app_description"),
                false, false, DevStackShared.ThemeManager.LabelStyle.Secondary);
            descriptionText.Content = new TextBlock
            {
                Text = localization.GetString("installer.welcome.app_description"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                LineHeight = 22,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.TextSecondary,
                MaxWidth = 420
            };

            innerPanel.Children.Add(logoImage);
            innerPanel.Children.Add(welcomeText);
            innerPanel.Children.Add(versionText);
            innerPanel.Children.Add(descriptionText);

            var welcomeContainer = DevStackShared.ThemeManager.CreateStyledCard(innerPanel, CARD_CORNER_RADIUS, true);
            welcomeContainer.Padding = new Thickness(CARD_PADDING, CARD_PADDING_VERTICAL, CARD_PADDING, CARD_PADDING_VERTICAL);

            welcomePanel.Children.Add(welcomeContainer);

            Grid.SetRow(welcomePanel, 0);
            contentGrid.Children.Add(welcomePanel);
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
            try
            {
                Title = localization.GetString("installer.window_title", GetVersion());
                if (languageLabel != null)
                    languageLabel.Content = localization.GetString("installer.welcome.language_label");
                if (backButton != null)
                    backButton.Content = localization.GetString("common.buttons.back");
                if (cancelButton != null)
                    cancelButton.Content = localization.GetString("common.buttons.cancel");

                UpdateStepContent();
            }
            catch { }
        }

        /// <summary>
        /// Creates the license agreement step with scrollable text and accept checkbox.
        /// </summary>
        private void CreateLicenseStep()
        {
            stepTitleText.Text = localization.GetString("installer.license.title");
            stepDescriptionText.Text = localization.GetString("installer.license.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var licenseLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.license.label"),
                false, false, DevStackShared.ThemeManager.LabelStyle.Normal);
            licenseLabel.FontWeight = FontWeights.SemiBold;
            licenseLabel.FontSize = 15;
            licenseLabel.Margin = new Thickness(0, 0, 0, 15);

            var licenseContainer = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.ConsoleBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(0)
            };

            var licenseTextBox = DevStackShared.ThemeManager.CreateStyledTextBox(true);
            licenseTextBox.Text = localization.GetString("installer.license.text");
            licenseTextBox.IsReadOnly = true;
            licenseTextBox.TextWrapping = TextWrapping.Wrap;
            licenseTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            licenseTextBox.FontFamily = new FontFamily("Consolas");
            licenseTextBox.FontSize = 12;
            licenseTextBox.BorderThickness = new Thickness(0);
            licenseTextBox.Padding = new Thickness(20, 15, 20, 15);

            licenseContainer.Child = licenseTextBox;

            Grid.SetRow(licenseLabel, 0);
            Grid.SetRow(licenseContainer, 1);
            contentGrid.Children.Add(licenseLabel);
            contentGrid.Children.Add(licenseContainer);
        }

        /// <summary>
        /// Creates the installation path selection step with path input and browse button.
        /// </summary>
        private void CreateInstallationPathStep()
        {
            stepTitleText.Text = localization.GetString("installer.installation_path.title");
            stepDescriptionText.Text = localization.GetString("installer.installation_path.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var pathLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.installation_path.label"),
                false, false, DevStackShared.ThemeManager.LabelStyle.Normal);
            pathLabel.FontWeight = FontWeights.SemiBold;
            pathLabel.FontSize = 15;
            pathLabel.Margin = new Thickness(0, 0, 0, 15);

            var pathContainer = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var pathGrid = new Grid();
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            pathTextBox = DevStackShared.ThemeManager.CreateStyledTextBox();
            pathTextBox.Text = installationPath;
            pathTextBox.Height = 40;
            pathTextBox.VerticalContentAlignment = VerticalAlignment.Center;
            pathTextBox.FontSize = 14;
            pathTextBox.BorderThickness = new Thickness(0);
            pathTextBox.Padding = new Thickness(15, 0, 15, 0);
            pathTextBox.Margin = new Thickness(0);
            pathTextBox.TextChanged += (s, e) => installationPath = pathTextBox.Text;

            var browserButton = DevStackShared.ThemeManager.CreateStyledButton(
                localization.GetString("installer.installation_path.browser"),
                BrowserButton_Click,
                DevStackShared.ThemeManager.ButtonStyle.Primary
            );
            browserButton.Width = 130;
            browserButton.Height = 40;
            browserButton.FontSize = 14;
            browserButton.FontWeight = FontWeights.Medium;
            browserButton.BorderThickness = new Thickness(1, 0, 0, 0);
            browserButton.Margin = new Thickness(0);

            var browseStyle = new Style(typeof(Button), browserButton.Style);
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(0, 6, 6, 0));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, DevStackShared.ThemeManager.CurrentTheme.ButtonBackground));
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, DevStackShared.ThemeManager.CurrentTheme.ButtonBackground));

            template.Triggers.Add(hoverTrigger);
            browseStyle.Setters.Add(new Setter(Button.TemplateProperty, template));
            browserButton.Style = browseStyle;

            Grid.SetColumn(pathTextBox, 0);
            Grid.SetColumn(browserButton, 1);
            pathGrid.Children.Add(pathTextBox);
            pathGrid.Children.Add(browserButton);
            pathContainer.Child = pathGrid;

            var spaceLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                GetSpaceRequirementText(),
                false, true, DevStackShared.ThemeManager.LabelStyle.Muted);
            spaceLabel.FontSize = 12;
            spaceLabel.Margin = new Thickness(0, 0, 0, 20);

            var infoPanel = DevStackShared.ThemeManager.CreateNotificationPanel(
                localization.GetString("installer.installation_path.info"), 
                DevStackShared.ThemeManager.NotificationType.Info);
            infoPanel.Margin = new Thickness(0, 10, 0, 0);

            Grid.SetRow(pathLabel, 0);
            Grid.SetRow(pathContainer, 1);
            Grid.SetRow(spaceLabel, 2);
            Grid.SetRow(infoPanel, 3);
            
            contentGrid.Children.Add(pathLabel);
            contentGrid.Children.Add(pathContainer);
            contentGrid.Children.Add(spaceLabel);
            contentGrid.Children.Add(infoPanel);
        }

        /// <summary>
        /// Creates the components selection step with checkboxes for shortcuts and system path.
        /// </summary>
        private void CreateComponentsStep()
        {
            stepTitleText.Text = localization.GetString("installer.components.title");
            stepDescriptionText.Text = localization.GetString("installer.components.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var optionsLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.components.label"),
                false, false, DevStackShared.ThemeManager.LabelStyle.Normal);
            optionsLabel.FontWeight = FontWeights.SemiBold;
            optionsLabel.FontSize = 15;
            optionsLabel.Margin = new Thickness(0, 0, 0, 15);

            var optionsContainer = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 18, 20, 18)
            };

            var optionsPanel = new StackPanel();

            desktopShortcutCheckBox = DevStackShared.ThemeManager.CreateStyledCheckBox(
                localization.GetString("installer.components.desktop_shortcuts"));
            desktopShortcutCheckBox.IsChecked = createDesktopShortcuts;
            desktopShortcutCheckBox.Checked += (s, e) => createDesktopShortcuts = true;
            desktopShortcutCheckBox.Unchecked += (s, e) => createDesktopShortcuts = false;
            desktopShortcutCheckBox.Margin = new Thickness(0, 0, 0, 15);

            startMenuShortcutCheckBox = DevStackShared.ThemeManager.CreateStyledCheckBox(
                localization.GetString("installer.components.start_menu_shortcuts"));
            startMenuShortcutCheckBox.IsChecked = createStartMenuShortcuts;
            startMenuShortcutCheckBox.Checked += (s, e) => createStartMenuShortcuts = true;
            startMenuShortcutCheckBox.Unchecked += (s, e) => createStartMenuShortcuts = false;
            startMenuShortcutCheckBox.Margin = new Thickness(0, 0, 0, 15);

            addToPathCheckBox = DevStackShared.ThemeManager.CreateStyledCheckBox(
                localization.GetString("installer.components.add_to_path"));
            addToPathCheckBox.IsChecked = addToPath;
            addToPathCheckBox.Checked += (s, e) => addToPath = true;
            addToPathCheckBox.Unchecked += (s, e) => addToPath = false;
            addToPathCheckBox.Margin = new Thickness(0, 0, 0, 0);

            optionsPanel.Children.Add(desktopShortcutCheckBox);
            optionsPanel.Children.Add(startMenuShortcutCheckBox);
            optionsPanel.Children.Add(addToPathCheckBox);

            optionsContainer.Child = optionsPanel;

            var pathInfoPanel = DevStackShared.ThemeManager.CreateNotificationPanel(
                localization.GetString("installer.components.path_info"), 
                DevStackShared.ThemeManager.NotificationType.Warning);
            pathInfoPanel.Margin = new Thickness(0, 20, 0, 0);

            Grid.SetRow(optionsLabel, 0);
            Grid.SetRow(optionsContainer, 1);
            Grid.SetRow(pathInfoPanel, 2);
            
            contentGrid.Children.Add(optionsLabel);
            contentGrid.Children.Add(optionsContainer);
            contentGrid.Children.Add(pathInfoPanel);
        }

        /// <summary>
        /// Creates the ready to install step displaying installation summary.
        /// </summary>
        private void CreateReadyToInstallStep()
        {
            stepTitleText.Text = localization.GetString("installer.ready_to_install.title");
            stepDescriptionText.Text = localization.GetString("installer.ready_to_install.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var summaryLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.ready_to_install.summary_label"),
                false, false, DevStackShared.ThemeManager.LabelStyle.Normal);
            summaryLabel.FontWeight = FontWeights.SemiBold;
            summaryLabel.FontSize = 15;
            summaryLabel.Margin = new Thickness(0, 0, 0, 15);

            var summaryContainer = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.ConsoleBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(0)
            };

            var summaryText = new TextBlock
            {
                Text = GetInstallationSummary(),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground,
                Padding = new Thickness(20, 18, 20, 18),
                LineHeight = 22
            };

            summaryContainer.Child = summaryText;

            Grid.SetRow(summaryLabel, 0);
            Grid.SetRow(summaryContainer, 1);
            contentGrid.Children.Add(summaryLabel);
            contentGrid.Children.Add(summaryContainer);
        }

        /// <summary>
        /// Creates the installing step with progress indicators and log display.
        /// </summary>
        private void CreateInstallingStep()
        {
            stepTitleText.Text = localization.GetString("installer.installing.title");
            stepDescriptionText.Text = localization.GetString("installer.installing.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            installStatusText = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.installing.preparing"),
                false, false, DevStackShared.ThemeManager.LabelStyle.Normal);
            installStatusText.FontWeight = FontWeights.SemiBold;
            installStatusText.FontSize = 15;
            installStatusText.Margin = new Thickness(0, 0, 0, 15);

            var progressContainer = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 18, 20, 18),
                Margin = new Thickness(0, 0, 0, 20)
            };

            installProgressBar = DevStackShared.ThemeManager.CreateStyledProgressBar(0, 100, false);
            installProgressBar.Height = 8;
            installProgressBar.Value = 0;

            progressContainer.Child = installProgressBar;

            var logContainer = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.ConsoleBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(0)
            };

            installLogListBox = new ListBox
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Background = DevStackShared.ThemeManager.CurrentTheme.ConsoleBackground,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(15, 10, 15, 10)
            };

            ScrollViewer.SetHorizontalScrollBarVisibility(installLogListBox, ScrollBarVisibility.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(installLogListBox, ScrollBarVisibility.Auto);

            var logItemStyle = new Style(typeof(ListBoxItem));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0, 2, 0, 2)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.MarginProperty, new Thickness(0)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground));

            var hoverTrigger = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            logItemStyle.Triggers.Add(hoverTrigger);

            var selectedTrigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground));
            logItemStyle.Triggers.Add(selectedTrigger);

            installLogListBox.ItemContainerStyle = logItemStyle;
            logContainer.Child = installLogListBox;

            Grid.SetRow(installStatusText, 0);
            Grid.SetRow(progressContainer, 1);
            Grid.SetRow(logContainer, 2);
            
            contentGrid.Children.Add(installStatusText);
            contentGrid.Children.Add(progressContainer);
            contentGrid.Children.Add(logContainer);
        }

        /// <summary>
        /// Creates the finished step displaying success message and installation details.
        /// </summary>
        private void CreateFinishedStep()
        {
            stepTitleText.Text = localization.GetString("installer.finished.title");
            stepDescriptionText.Text = localization.GetString("installer.finished.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var finishedPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var successIcon = new TextBlock
            {
                Text = localization.GetString("installer.finished.success_icon"),
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var successText = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.finished.success_title"),
                true, false, DevStackShared.ThemeManager.LabelStyle.Normal);
            successText.FontSize = 18;
            successText.FontWeight = FontWeights.Bold;
            successText.HorizontalAlignment = HorizontalAlignment.Center;
            successText.Margin = new Thickness(0, 0, 0, 20);
            successText.Foreground = DevStackShared.ThemeManager.CurrentTheme.Success;

            var finishedMessage = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.finished.success_message"),
                false, false, DevStackShared.ThemeManager.LabelStyle.Secondary);
            finishedMessage.Content = new TextBlock
            {
                Text = localization.GetString("installer.finished.success_message"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground
            };
            finishedMessage.Margin = new Thickness(40, 0, 40, 20);

            var infoPanel = DevStackShared.ThemeManager.CreateStyledCard(
                new StackPanel
                {
                    Children =
                    {
                        DevStackShared.ThemeManager.CreateStyledLabel(
                            localization.GetString("installer.finished.install_location"),
                            false, false, DevStackShared.ThemeManager.LabelStyle.Normal),
                        new TextBlock
                        {
                            Text = installationPath,
                            Foreground = DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground,
                            FontFamily = new FontFamily("Consolas"),
                            FontSize = 11
                        }
                    }
                }, 8, false);
            infoPanel.Margin = new Thickness(20, 10, 20, 20);
            infoPanel.Padding = new Thickness(15);

            var launchPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var launchCheckBox = DevStackShared.ThemeManager.CreateStyledCheckBox(
                localization.GetString("installer.finished.launch_now"));
            launchCheckBox.IsChecked = launchAfterInstall;
            launchCheckBox.Checked += (s, e) => launchAfterInstall = true;
            launchCheckBox.Unchecked += (s, e) => launchAfterInstall = false;

            finishedPanel.Children.Add(successIcon);
            finishedPanel.Children.Add(successText);
            finishedPanel.Children.Add(finishedMessage);
            finishedPanel.Children.Add(infoPanel);
            finishedPanel.Children.Add(launchPanel);
            launchPanel.Children.Add(launchCheckBox);

            Grid.SetRow(finishedPanel, 0);
            contentGrid.Children.Add(finishedPanel);
        }

        /// <summary>
        /// Calculates the required disk space in bytes for installation.
        /// </summary>
        /// <returns>The required space in bytes.</returns>
        private long GetRequiredSpaceBytes()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                var zipResourceName = resourceNames.FirstOrDefault(r => r.EndsWith("DevStack.zip"));
                
                if (zipResourceName != null)
                {
                    using (var resourceStream = assembly.GetManifestResourceStream(zipResourceName))
                    {
                        if (resourceStream != null)
                        {
                            return (long)(resourceStream.Length * 1.2);
                        }
                    }
                }
                
                return 50 * 1024 * 1024;
            }
            catch
            {
                return 50 * 1024 * 1024;
            }
        }

        /// <summary>
        /// Generates a formatted text string showing required and available disk space.
        /// </summary>
        /// <returns>A formatted string with space requirements.</returns>
        private string GetSpaceRequirementText()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(installationPath) ?? "C:\\");
                var availableSpaceMB = drive.AvailableFreeSpace / (1024 * 1024);
                var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0);
                
                string availableSpaceText;
                if (availableSpaceMB >= 1024)
                {
                    var availableSpaceGB = availableSpaceMB / 1024.0;
                    availableSpaceText = $"{availableSpaceGB:F1} GB";
                }
                else
                {
                    availableSpaceText = $"{availableSpaceMB:N0} MB";
                }
                
                return localization.GetString("installer.installation_path.space_required", $"{requiredSpace:F1}") + "  |  " + 
                       localization.GetString("installer.installation_path.space_available", availableSpaceText);
            }
            catch
            {
                var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0);
                return localization.GetString("installer.installation_path.space_required", $"{requiredSpace:F1}");
            }
        }

        /// <summary>
        /// Generates a summary text of the installation settings for review.
        /// </summary>
        /// <returns>A formatted summary of installation configuration.</returns>
        private string GetInstallationSummary()
        {
            var summary = $@"{localization.GetString("installer.ready_to_install.destination")}
  {installationPath}

{localization.GetString("installer.ready_to_install.components_header")}
{localization.GetString("installer.ready_to_install.cli_component")}
{localization.GetString("installer.ready_to_install.gui_component")}
{localization.GetString("installer.ready_to_install.uninstaller_component")}
{localization.GetString("installer.ready_to_install.config_component")}

{localization.GetString("installer.ready_to_install.options_header")}";

            if (createDesktopShortcuts)
                summary += $"\n{localization.GetString("installer.ready_to_install.create_desktop")}";
            if (createStartMenuShortcuts)
                summary += $"\n{localization.GetString("installer.ready_to_install.create_start_menu")}";
            if (addToPath)
                summary += $"\n{localization.GetString("installer.ready_to_install.add_path")}";

            var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0);
            summary += $"\n\n{localization.GetString("installer.ready_to_install.space_required_summary").Replace("{0}", requiredSpace.ToString("F1"))}";

            return summary;
        }

        /// <summary>
        /// Adds a timestamped message to the installation log display.
        /// </summary>
        /// <param name="message">The message to add to the log.</param>
        private void AddInstallationLog(string message)
        {
            if (installLogListBox != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    installLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                    installLogListBox.ScrollIntoView(installLogListBox.Items[installLogListBox.Items.Count - 1]);
                });
            }
        }

        /// <summary>
        /// Handles the Back button click to navigate to the previous installation step.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > InstallerStep.Welcome)
            {
                currentStep--;
                UpdateStepContent();
            }
        }

        /// <summary>
        /// Handles the Next/Install/Finish button click to proceed with installation or navigate steps.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (currentStep)
            {
                case InstallerStep.Welcome:
                case InstallerStep.License:
                case InstallerStep.InstallationPath:
                case InstallerStep.Components:
                case InstallerStep.ReadyToInstall:
                    if (currentStep == InstallerStep.ReadyToInstall)
                    {
                        currentStep = InstallerStep.Installing;
                        UpdateStepContent();
                        await PerformInstallation();
                    }
                    else
                    {
                        currentStep++;
                        UpdateStepContent();
                    }
                    break;
                case InstallerStep.Finished:
                    if (launchAfterInstall)
                    {
                        try
                        {
                            string guiPath = Path.Combine(installationPath, "DevStackGUI.exe");
                            if (File.Exists(guiPath))
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = guiPath,
                                    UseShellExecute = true
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            DevStackShared.ThemeManager.CreateStyledMessageBox($"Could not launch DevStack GUI: {ex.Message}", 
                                "Launch Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    Application.Current.Shutdown();
                    break;
            }
        }

        /// <summary>
        /// Handles the Cancel button click to confirm and exit the installer.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = DevStackShared.ThemeManager.CreateStyledMessageBox(localization.GetString("installer.dialogs.cancel_message"), 
                localization.GetString("installer.dialogs.cancel_title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Handles the window closing event to prevent accidental closure during installation.
        /// </summary>
        /// <param name="sender">The window that is closing.</param>
        /// <param name="e">Event arguments for the closing event.</param>
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (currentStep != InstallerStep.Finished)
            {
                var result = DevStackShared.ThemeManager.CreateStyledMessageBox(localization.GetString("installer.dialogs.cancel_message"), 
                    localization.GetString("installer.dialogs.cancel_title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Downloads the .NET SDK required for building DevStack components.
        /// </summary>
        /// <param name="tempDir">Temporary directory containing build information.</param>
        /// <returns>Path to the extracted .NET SDK directory.</returns>
        private async Task<string> DownloadDotNetSDK(string tempDir)
        {
            var buildInfoPath = Path.Combine(tempDir, "build_info.json");
            if (!File.Exists(buildInfoPath))
            {
                throw new Exception("build_info.json not found in source package");
            }

            var buildInfoText = File.ReadAllText(buildInfoPath);
            var buildInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(buildInfoText);
            
            if (buildInfo == null)
                throw new Exception("Failed to parse build_info.json");
                
            string dotnetVersion = buildInfo.dotnet_version?.ToString() ?? "9.0.100";

            AddInstallationLog($"Downloading .NET SDK {dotnetVersion}...");
            
            string dotnetTempDir = Path.Combine(Path.GetTempPath(), "DevStack_DotNet_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(dotnetTempDir);
            
            string zipPath = Path.Combine(dotnetTempDir, "dotnet-sdk.zip");
            
            var downloadUrls = new[]
            {
                "https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.304/dotnet-sdk-9.0.304-win-x64.zip",
                "https://dotnetcli.azureedge.net/dotnet/Sdk/9.0.304/dotnet-sdk-9.0.304-win-x64.zip",
                "https://dotnetcli.blob.core.windows.net/dotnet/Sdk/9.0.304/dotnet-sdk-9.0.304-win-x64.zip"
            };
            
            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(15);
                client.DefaultRequestHeaders.Add("User-Agent", "DevStack-Installer/1.0");
                
                Exception? lastException = null;
                
                foreach (var downloadUrl in downloadUrls)
                {
                    try
                    {
                        AddInstallationLog($"Trying download from: {downloadUrl.Substring(0, Math.Min(50, downloadUrl.Length))}...");
                        
                        using (var response = await client.GetAsync(downloadUrl, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                                var downloadedBytes = 0L;
                                
                                AddInstallationLog($"Download started, size: {totalBytes / 1024 / 1024:F1}MB");
                                
                                using (var contentStream = await response.Content.ReadAsStreamAsync())
                                using (var fileStream = File.Create(zipPath))
                                {
                                    var buffer = new byte[8192];
                                    int bytesRead;
                                    
                                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                    {
                                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                                        downloadedBytes += bytesRead;
                                    }
                                }
                                
                                AddInstallationLog("Download completed successfully");
                                break;
                            }
                            else
                            {
                                AddInstallationLog($"Download failed with status: {response.StatusCode}");
                                lastException = new Exception($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddInstallationLog($"Download attempt failed: {ex.Message}");
                        lastException = ex;
                        
                        if (File.Exists(zipPath))
                        {
                            try { File.Delete(zipPath); } catch { }
                        }
                    }
                }
                
                if (!File.Exists(zipPath) || new FileInfo(zipPath).Length < 1024 * 1024)
                {
                    AddInstallationLog("Direct download failed, trying dotnet install script...");
                    return await TryDotNetInstallScript(dotnetTempDir, dotnetVersion);
                }
            }
            
            AddInstallationLog("Extracting .NET SDK...");
            string dotnetDir = Path.Combine(dotnetTempDir, "dotnet");
            Directory.CreateDirectory(dotnetDir);
            
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, dotnetDir);
                AddInstallationLog(".NET SDK extracted successfully");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract .NET SDK: {ex.Message}");
            }
            
            File.Delete(zipPath);
            
            return dotnetDir;
        }

        /// <summary>
        /// Attempts to install .NET SDK using the official dotnet-install PowerShell script.
        /// </summary>
        /// <param name="dotnetTempDir">Temporary directory for .NET installation.</param>
        /// <param name="dotnetVersion">Version of .NET SDK to install.</param>
        /// <returns>Path to the installed .NET SDK directory.</returns>
        private async Task<string> TryDotNetInstallScript(string dotnetTempDir, string dotnetVersion)
        {
            try
            {
                AddInstallationLog("Downloading dotnet-install script...");
                
                string scriptPath = Path.Combine(dotnetTempDir, "dotnet-install.ps1");
                string dotnetDir = Path.Combine(dotnetTempDir, "dotnet");
                
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var scriptContent = await client.GetStringAsync("https://dot.net/v1/dotnet-install.ps1");
                    File.WriteAllText(scriptPath, scriptContent);
                }
                
                AddInstallationLog("Running dotnet install script...");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -Channel 9.0 -Version {dotnetVersion} -InstallDir \"{dotnetDir}\" -NoPath",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        throw new Exception("Failed to start PowerShell process");
                    }
                    
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();
                    
                    var output = await outputTask;
                    var error = await errorTask;
                    
                    if (process.ExitCode != 0)
                    {
                        AddInstallationLog($"Install script failed: {error}");
                        throw new Exception($"dotnet-install script failed with exit code {process.ExitCode}: {error}");
                    }
                    
                    AddInstallationLog("dotnet install script completed successfully");
                }
                
                string dotnetExe = Path.Combine(dotnetDir, "dotnet.exe");
                if (!File.Exists(dotnetExe))
                {
                    throw new Exception("dotnet.exe not found after script installation");
                }
                
                return dotnetDir;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to install .NET SDK using install script: {ex.Message}");
            }
        }

        /// <summary>
        /// Compiles all DevStack projects using the provided .NET SDK.
        /// </summary>
        /// <param name="sourceDir">Directory containing the source code.</param>
        /// <param name="dotnetDir">Directory containing the .NET SDK.</param>
        /// <param name="installPath">Target installation directory.</param>
        private async Task CompileProjects(string sourceDir, string dotnetDir, string installPath)
        {
            var buildInfoPath = Path.Combine(sourceDir, "build_info.json");
            var buildInfoText = File.ReadAllText(buildInfoPath);
            var buildInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(buildInfoText);
            
            if (buildInfo == null)
                throw new Exception("Failed to parse build_info.json");
            
            string dotnetExe = Path.Combine(dotnetDir, "dotnet.exe");
            if (!File.Exists(dotnetExe))
            {
                throw new Exception($"dotnet.exe not found at {dotnetExe}");
            }
            
            AddInstallationLog("Compiling DevStack projects...");
            
            var projects = buildInfo.projects ?? throw new Exception("projects not found in build_info.json");
            int projectCount = projects.Count;
            int currentProject = 0;
            
            string tempBuildDir = Path.Combine(Path.GetTempPath(), "DevStackBuild_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(tempBuildDir);
            
            try
            {
                foreach (var project in projects)
                {
                    currentProject++;
                    string projectName = project.name;
                    string projectPath = project.path;
                    string outputName = project.output_name;
                    
                    AddInstallationLog($"Building {projectName}...");
                    
                    string fullProjectPath = Path.Combine(sourceDir, projectPath.ToString().Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(fullProjectPath))
                    {
                        throw new Exception($"Project file not found: {fullProjectPath}");
                    }
                    
                    var baseProgress = 35;
                    var compilationProgress = (currentProject - 1) * 50 / projectCount;
                    installProgressBar.Value = baseProgress + compilationProgress;
                    
                    string projectTempDir = Path.Combine(tempBuildDir, projectName);
                    Directory.CreateDirectory(projectTempDir);
                    
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = dotnetExe,
                        Arguments = $"publish \"{fullProjectPath}\" -c Release -r win-x64 --self-contained true -o \"{projectTempDir}\"",
                        WorkingDirectory = sourceDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    
                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null)
                        {
                            throw new Exception($"Failed to start dotnet process for {projectName}");
                        }
                        
                        var outputTask = process.StandardOutput.ReadToEndAsync();
                        var errorTask = process.StandardError.ReadToEndAsync();
                        
                        await process.WaitForExitAsync();
                        
                        var output = await outputTask;
                        var error = await errorTask;
                        
                        if (process.ExitCode != 0)
                        {
                            AddInstallationLog($"Build failed for {projectName}:");
                            AddInstallationLog($"Error: {error}");
                            throw new Exception($"Build failed for {projectName}: {error}");
                        }
                        
                        AddInstallationLog($"Successfully built {projectName}");
                    }
                    
                    var exeFiles = Directory.GetFiles(projectTempDir, "*.exe", SearchOption.AllDirectories);
                    foreach (var exeFile in exeFiles)
                    {
                        var fileName = Path.GetFileName(exeFile);
                        var targetPath = Path.Combine(installPath, fileName);
                        
                        if (fileName.Equals(projectName + ".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            targetPath = Path.Combine(installPath, outputName.ToString());
                        }
                        
                        File.Copy(exeFile, targetPath, true);
                        AddInstallationLog($"Copied {fileName} to installation directory");
                    }
                }
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempBuildDir))
                    {
                        Directory.Delete(tempBuildDir, true);
                        AddInstallationLog("Temporary build files cleaned up");
                    }
                }
                catch (Exception ex)
                {
                    AddInstallationLog($"Warning: Could not delete temporary build directory: {ex.Message}");
                }
            }
            
            var iconsrc = Path.Combine(sourceDir, "src", "Shared", "DevStack.ico");
            var iconDest = Path.Combine(installPath, "DevStack.ico");
            if (File.Exists(iconsrc))
            {
                File.Copy(iconsrc, iconDest, true);
            }
            
            var configsDir = Path.Combine(sourceDir, "configs");
            if (Directory.Exists(configsDir))
            {
                var configsDest = Path.Combine(installPath, "configs");
                if (Directory.Exists(configsDest))
                    Directory.Delete(configsDest, true);
                DirectoryCopy(configsDir, configsDest, true);
            }
            
            AddInstallationLog("All projects compiled successfully!");
        }

        /// <summary>
        /// Recursively copies all files and subdirectories from source to destination.
        /// </summary>
        /// <param name="sourceDirName">Source directory path.</param>
        /// <param name="destDirName">Destination directory path.</param>
        /// <param name="copySubDirs">Whether to copy subdirectories recursively.</param>
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Executes the complete installation process including extraction, compilation, and configuration.
        /// </summary>
        private async Task PerformInstallation()
        {
            string? tempZipPath = null;
            string? tempSourceDir = null;
            string? dotnetTempDir = null;
            
            try
            {
                installProgressBar.Value = 0;
                
                installStatusText.Content = localization.GetString("installer.installing.extracting");
                AddInstallationLog(localization.GetString("installer.log_messages.starting"));
                
                tempZipPath = await Task.Run(() => ExtractEmbeddedZip());
                installProgressBar.Value = 5;
                AddInstallationLog("Source package extracted");

                tempSourceDir = Path.Combine(Path.GetTempPath(), "DevStackSource_" + Guid.NewGuid().ToString("N")[..8]);
                Directory.CreateDirectory(tempSourceDir);
                
                await Task.Run(() => 
                {
                    ExtractZipWithDeflate(tempZipPath, tempSourceDir);
                });
                installProgressBar.Value = 10;
                AddInstallationLog(localization.GetString("installer.log_messages.source_extracted"));

                installStatusText.Content = localization.GetString("installer.installing.downloading_sdk");
                AddInstallationLog(localization.GetString("installer.log_messages.downloading_sdk"));
                
                dotnetTempDir = await DownloadDotNetSDK(tempSourceDir);
                installProgressBar.Value = 35;
                AddInstallationLog(localization.GetString("installer.log_messages.sdk_downloaded"));

                installStatusText.Content = localization.GetString("installer.installing.creating_directory");
                AddInstallationLog(localization.GetString("installer.log_messages.creating_dir", installationPath));
                Directory.CreateDirectory(installationPath);
                installProgressBar.Value = 40;

                installStatusText.Content = localization.GetString("installer.installing.compiling_projects");
                AddInstallationLog(localization.GetString("installer.log_messages.compiling"));
                await CompileProjects(tempSourceDir, dotnetTempDir, installationPath);
                installProgressBar.Value = 85;
                AddInstallationLog(localization.GetString("installer.log_messages.compilation_complete"));

                try
                {
                    DevStackConfig.PersistSetting("language", localization.CurrentLanguage, installationPath);
                }
                catch { /* silent to not break installation */ }

                installStatusText.Content = localization.GetString("installer.installing.registering");
                AddInstallationLog(localization.GetString("installer.log_messages.registering"));
                await Task.Run(() => RegisterInstallation(installationPath));
                installProgressBar.Value = 90;

                if (createDesktopShortcuts)
                {
                    installStatusText.Content = localization.GetString("installer.installing.creating_desktop");
                    AddInstallationLog(localization.GetString("installer.log_messages.desktop_shortcuts"));
                    CreateDesktopShortcuts(installationPath);
                    installProgressBar.Value = 95;
                }

                if (createStartMenuShortcuts)
                {
                    installStatusText.Content = localization.GetString("installer.installing.creating_start_menu");
                    AddInstallationLog(localization.GetString("installer.log_messages.start_menu_shortcuts"));
                    CreateStartMenuShortcuts(installationPath);
                    installProgressBar.Value = 97;
                }

                if (addToPath)
                {
                    installStatusText.Content = localization.GetString("installer.installing.adding_path");
                    AddInstallationLog(localization.GetString("installer.log_messages.adding_path"));
                    AddToSystemPath(installationPath);
                    installProgressBar.Value = 99;
                }

                installStatusText.Content = localization.GetString("installer.installing.completed");
                installProgressBar.Value = 100;
                AddInstallationLog(localization.GetString("installer.log_messages.completed_success"));

                await Task.Delay(1000);

                currentStep = InstallerStep.Finished;
                UpdateStepContent();
            }
            catch (Exception ex)
            {
                AddInstallationLog($"ERROR: {ex.Message}");
                DevStackShared.ThemeManager.CreateStyledMessageBox(localization.GetString("installer.dialogs.installation_error_message", ex.Message), 
                    localization.GetString("installer.dialogs.installation_error_title"), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                installStatusText.Content = localization.GetString("installer.dialogs.installation_error_title");
                installProgressBar.Value = 0;
                
                backButton.IsEnabled = true;
                cancelButton.IsEnabled = true;
            }
            finally
            {
                if (tempZipPath != null && File.Exists(tempZipPath))
                {
                    try
                    {
                        File.Delete(tempZipPath);
                        AddInstallationLog("Temporary source package cleaned up");
                    }
                    catch (Exception ex)
                    {
                        AddInstallationLog($"Warning: Could not delete temporary source file: {ex.Message}");
                    }
                }
                
                if (tempSourceDir != null && Directory.Exists(tempSourceDir))
                {
                    try
                    {
                        Directory.Delete(tempSourceDir, true);
                        AddInstallationLog("Temporary source directory cleaned up");
                    }
                    catch (Exception ex)
                    {
                        AddInstallationLog($"Warning: Could not delete temporary source directory: {ex.Message}");
                    }
                }
                
                if (dotnetTempDir != null && Directory.Exists(dotnetTempDir))
                {
                    try
                    {
                        Directory.Delete(Path.GetDirectoryName(dotnetTempDir) ?? dotnetTempDir, true);
                        AddInstallationLog(".NET SDK temporary files cleaned up");
                    }
                    catch (Exception ex)
                    {
                        AddInstallationLog($"Warning: Could not delete .NET SDK temporary files: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates shortcuts in the Windows Start Menu for DevStack applications.
        /// </summary>
        /// <param name="installPath">Path where DevStack is installed.</param>
        private void CreateStartMenuShortcuts(string installPath)
        {
            try
            {
                string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "DevStack Manager");
                Directory.CreateDirectory(startMenuPath);
                
                string cliPath = Path.Combine(installPath, "DevStack.exe");
                if (File.Exists(cliPath))
                {
                    CreateShortcut(Path.Combine(startMenuPath, "DevStack CLI.lnk"), cliPath, "DevStack Command Line Interface");
                }

                string guiPath = Path.Combine(installPath, "DevStackGUI.exe");
                if (File.Exists(guiPath))
                {
                    CreateShortcut(Path.Combine(startMenuPath, "DevStack GUI.lnk"), guiPath, "DevStack Graphical Interface");
                }

                string uninstallerPath = Path.Combine(installPath, "DevStack-Uninstaller.exe");
                if (File.Exists(uninstallerPath))
                {
                    CreateShortcut(Path.Combine(startMenuPath, "Uninstall DevStack.lnk"), uninstallerPath, "Uninstall DevStack Manager");
                }
            }
            catch (Exception ex)
            {
                AddInstallationLog($"Warning: Could not create Start Menu shortcuts: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds the DevStack installation directory to the user's system PATH environment variable.
        /// </summary>
        /// <param name="installPath">Path where DevStack is installed.</param>
        private void AddToSystemPath(string installPath)
        {
            try
            {
                var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                if (!userPath.Contains(installPath))
                {
                    var newPath = string.IsNullOrEmpty(userPath) ? installPath : $"{userPath};{installPath}";
                    Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                    AddInstallationLog(localization.GetString("installer.log_messages.path_added"));
                }
                else
                {
                    AddInstallationLog(localization.GetString("installer.log_messages.path_exists"));
                }
            }
            catch (Exception ex)
            {
                AddInstallationLog($"Warning: Could not add to PATH: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the browse button click to select a custom installation directory.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private void BrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = localization.GetString("installer.dialogs.folder_dialog_title"),
                FileName = "Select Folder",
                Filter = "Folder|*.folder",
                InitialDirectory = installationPath
            };

            if (dialog.ShowDialog() == true)
            {
                var selectedPath = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    pathTextBox.Text = selectedPath;
                    installationPath = selectedPath;
                }
            }
        }

        /// <summary>
        /// Creates desktop shortcuts for DevStack applications.
        /// </summary>
        /// <param name="installPath">Path where DevStack is installed.</param>
        private void CreateDesktopShortcuts(string installPath)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                
                string cliPath = Path.Combine(installPath, "DevStack.exe");
                if (File.Exists(cliPath))
                {
                    CreateShortcut(Path.Combine(desktopPath, "DevStack CLI.lnk"), cliPath, "DevStack Command Line Interface");
                }

                string guiPath = Path.Combine(installPath, "DevStackGUI.exe");
                if (File.Exists(guiPath))
                {
                    CreateShortcut(Path.Combine(desktopPath, "DevStack GUI.lnk"), guiPath, "DevStack Graphical Interface");
                }
            }
            catch (Exception ex)
            {
                AddInstallationLog($"Warning: Could not create desktop shortcuts: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a Windows shortcut (.lnk file) for the specified target.
        /// </summary>
        /// <param name="shortcutPath">Path where the shortcut will be created.</param>
        /// <param name="targetPath">Target executable path.</param>
        /// <param name="description">Description for the shortcut.</param>
        private void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            try
            {
                string batchContent = $"@echo off\ncd /d \"{Path.GetDirectoryName(targetPath)}\"\nstart \"\" \"{Path.GetFileName(targetPath)}\"";
                string batchPath = shortcutPath.Replace(".lnk", ".bat");
                File.WriteAllText(batchPath, batchContent);
                
                string psScript = $@"
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
$Shortcut.TargetPath = '{targetPath}'
$Shortcut.WorkingDirectory = '{Path.GetDirectoryName(targetPath)}'
$Shortcut.Description = '{description}'
$Shortcut.Save()
";
                
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{psScript}\"",
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                process.WaitForExit();
                
                if (process.ExitCode == 0 && File.Exists(shortcutPath))
                {
                    try { File.Delete(batchPath); } catch { }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Registers the DevStack installation in the Windows registry for uninstaller support.
        /// </summary>
        /// <param name="installPath">Path where DevStack is installed.</param>
        private void RegisterInstallation(string installPath)
        {
            try
            {
                var version = GetVersion();
                var uninstallerPath = Path.Combine(installPath, "DevStack-Uninstaller.exe");
                var displayName = "DevStack Manager";
                
                using var userKey = Registry.CurrentUser.CreateSubKey(@"Software\DevStack");
                userKey.SetValue("InstallPath", installPath);
                userKey.SetValue("Version", version);
                userKey.SetValue("InstallDate", DateTime.Now.ToString("yyyy-MM-dd"));
                
                try
                {
                    using var uninstallKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DevStack");
                    uninstallKey.SetValue("DisplayName", displayName);
                    uninstallKey.SetValue("DisplayVersion", version);
                    uninstallKey.SetValue("Publisher", "DevStackManager");
                    uninstallKey.SetValue("InstallLocation", installPath);
                    uninstallKey.SetValue("UninstallString", $"\"{uninstallerPath}\"");
                    uninstallKey.SetValue("QuietUninstallString", $"\"{uninstallerPath}\" /S");
                    uninstallKey.SetValue("NoModify", 1);
                    uninstallKey.SetValue("NoRepair", 1);
                    uninstallKey.SetValue("EstimatedSize", GetDirectorySize(installPath));
                    uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                    uninstallKey.SetValue("HelpLink", "https://github.com/devstack/devstack");
                    uninstallKey.SetValue("URLInfoAbout", "https://github.com/devstack/devstack");
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Extracts a ZIP file using DEFLATE compression to the specified directory.
        /// </summary>
        /// <param name="zipPath">Path to the ZIP file to extract.</param>
        /// <param name="extractPath">Destination directory for extraction.</param>
        private void ExtractZipWithDeflate(string zipPath, string extractPath)
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, true);
                AddInstallationLog("Successfully extracted ZIP with DEFLATE compression");
            }
            catch (Exception ex)
            {
                AddInstallationLog($"DEFLATE extraction failed: {ex.Message}");
                throw new Exception($"Failed to extract ZIP file: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the total size of a directory in kilobytes.
        /// </summary>
        /// <param name="path">Directory path to calculate size for.</param>
        /// <returns>Total size in kilobytes.</returns>
        private int GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                var size = dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                return (int)(size / 1024);
            }
            catch
            {
                return 0;
            }
        }
    }
}
