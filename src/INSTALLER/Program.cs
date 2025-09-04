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
    public partial class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                // Initialize localization for installer
                var locManager = LocalizationManager.Initialize(ApplicationType.Installer);
                
                // Log available languages
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
                MessageBox.Show($"Error starting installer: {ex.Message}\n\nDetails: {ex}", 
                    "DevStack Installer Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

    public class InstallerWindow : Window
    {
        private InstallerStep currentStep = InstallerStep.Welcome;
        private Grid mainGrid = null!;
        private Grid contentGrid = null!;
        private TextBlock stepTitleText = null!;
        private TextBlock stepDescriptionText = null!;
        private Button backButton = null!;
        private Button nextButton = null!;
        private Button cancelButton = null!;
        private ProgressBar stepProgressBar = null!;
        
        // Localization
        private LocalizationManager localization = LocalizationManager.Instance!;
        private ComboBox languageComboBox = null!;
        private Label languageLabel = null!;
        private StackPanel languagePanel = null!;
        
        // Installation settings
        private string installationPath = "";
        private bool createDesktopShortcuts = true;
        private bool createStartMenuShortcuts = true;
        private bool addToPath = false;
        private bool launchAfterInstall = true;
        
        // Step-specific controls
        private TextBox pathTextBox = null!;
        private CheckBox desktopShortcutCheckBox = null!;
        private CheckBox startMenuShortcutCheckBox = null!;
        private CheckBox addToPathCheckBox = null!;
        private ProgressBar installProgressBar = null!;
        private Label installStatusText = null!;
        private ListBox installLogListBox = null!;

        public InstallerWindow()
        {
            try
            {
                // Assinar mudança de idioma antes de construir a UI
                localization.LanguageChanged += Localization_LanguageChanged;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing installer window: {ex.Message}", 
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

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

        private string ExtractEmbeddedZip()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            // Try to find the source resource
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

        private void InitializeComponent()
        {
            string version = GetVersion();
            
            // Log version info for debugging
            System.Diagnostics.Debug.WriteLine($"Installer version: {version}");
            
            // Obter window title com formatação explícita
            Title = localization.GetString("installer.window_title", version);
            System.Diagnostics.Debug.WriteLine($"Window title set to: {Title}");
            Width = 750;
            Height = 650;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            
            // Aplicar tema usando DevStackShared.ThemeManager
            DevStackShared.ThemeManager.ApplyThemeToWindow(this);

            // Initialize installation path
            installationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DevStack");

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
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
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
                Foreground = DevStackShared.ThemeManager.CurrentTheme.Foreground
            };

            stepDescriptionText = new TextBlock
            {
                FontSize = 13,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.TextSecondary,
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            headerStackPanel.Children.Add(stepTitleText);
            headerStackPanel.Children.Add(stepDescriptionText);

            // Progress indicator usando ThemeManager
            stepProgressBar = DevStackShared.ThemeManager.CreateStyledProgressBar(0, 6, false);
            stepProgressBar.Width = 220;
            stepProgressBar.Height = 6;
            stepProgressBar.Margin = new Thickness(25, 0, 25, 0);
            stepProgressBar.VerticalAlignment = VerticalAlignment.Center;

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
                Background = DevStackShared.ThemeManager.CurrentTheme.PanelBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
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

            languageLabel = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.welcome.language_label"), 
                false, true, DevStackShared.ThemeManager.LabelStyle.Secondary);
            languageLabel.VerticalAlignment = VerticalAlignment.Center;
            languageLabel.Margin = new Thickness(0, 0, 10, 0);

            languageComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();

            // Populate language options
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

            // Buttons panel no lado direito
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(25, 18, 25, 18)
            };

            // Botões usando ThemeManager
            backButton = DevStackShared.ThemeManager.CreateStyledButton(
                localization.GetString("common.buttons.back"), 
                BackButton_Click, 
                DevStackShared.ThemeManager.ButtonStyle.Secondary);
            backButton.Width = 90;
            backButton.Height = 36;
            backButton.Margin = new Thickness(0, 0, 12, 0);
            backButton.IsEnabled = false;

            nextButton = DevStackShared.ThemeManager.CreateStyledButton(
                localization.GetString("common.buttons.next"), 
                NextButton_Click, 
                DevStackShared.ThemeManager.ButtonStyle.Primary);
            nextButton.Width = 130;
            nextButton.Height = 36;
            nextButton.Margin = new Thickness(0, 0, 12, 0);

            cancelButton = DevStackShared.ThemeManager.CreateStyledButton(
                localization.GetString("common.buttons.cancel"), 
                CancelButton_Click, 
                DevStackShared.ThemeManager.ButtonStyle.Secondary);
            cancelButton.Width = 90;
            cancelButton.Height = 36;

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

        private void UpdateStepContent()
        {
            // Clear content
            contentGrid.Children.Clear();
            contentGrid.RowDefinitions.Clear();
            contentGrid.ColumnDefinitions.Clear();

            // Atualizar área de conteúdo com tema do ThemeManager
            contentGrid.Background = DevStackShared.ThemeManager.CurrentTheme.FormBackground;
            contentGrid.Margin = new Thickness(25);

            // Update progress
            stepProgressBar.Value = (int)currentStep;

            // Update buttons
            backButton.IsEnabled = currentStep != InstallerStep.Welcome;
            cancelButton.Visibility = Visibility.Visible; // Reset cancel button visibility
            
            // Show language panel only on welcome step
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

        private void CreateWelcomeStep()
        {
            // Log detailed diagnostic info
            System.Diagnostics.Debug.WriteLine("=========== CREATING WELCOME STEP ===========");
            
            var title = localization.GetString("installer.welcome.title");
            System.Diagnostics.Debug.WriteLine($"installer.welcome.title = \"{title}\"");
            stepTitleText.Text = title;
            
            var description = localization.GetString("installer.welcome.description");
            System.Diagnostics.Debug.WriteLine($"installer.welcome.description = \"{description}\"");
            stepDescriptionText.Text = description;

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Welcome panel com design moderno usando ThemeManager
            var welcomePanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                Margin = new Thickness(0, 20, 0, 20)
            };

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
                Margin = new Thickness(0)
            };

            var welcomeText = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.welcome.app_name"), 
                true, false, DevStackShared.ThemeManager.LabelStyle.Title);
            welcomeText.HorizontalAlignment = HorizontalAlignment.Center;
            welcomeText.FontSize = 28;
            welcomeText.Margin = new Thickness(0, 0, 0, 8);

            var versionText = DevStackShared.ThemeManager.CreateStyledLabel(
                localization.GetString("installer.welcome.version", GetVersion()),
                false, false, DevStackShared.ThemeManager.LabelStyle.Secondary);
            versionText.HorizontalAlignment = HorizontalAlignment.Center;
            versionText.FontSize = 15;
            versionText.Margin = new Thickness(0, 0, 0, 25);

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

            // Container principal com borda arredondada usando ThemeManager
            var welcomeContainer = DevStackShared.ThemeManager.CreateStyledCard(innerPanel, 12, true);
            welcomeContainer.Padding = new Thickness(40, 35, 40, 35);

            welcomePanel.Children.Add(welcomeContainer);

            Grid.SetRow(welcomePanel, 0);
            contentGrid.Children.Add(welcomePanel);
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (languageComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string languageCode)
            {
                localization.LoadLanguage(languageCode);
            }
        }

        private void Localization_LanguageChanged(object? sender, string newLanguage)
        {
            try
            {
                // Atualizar título e rótulos fixos
                Title = localization.GetString("installer.window_title", GetVersion());
                if (languageLabel != null)
                    languageLabel.Content = localization.GetString("installer.welcome.language_label");
                if (backButton != null)
                    backButton.Content = localization.GetString("common.buttons.back");
                if (cancelButton != null)
                    cancelButton.Content = localization.GetString("common.buttons.cancel");

                // Atualizar conteúdo específico do passo
                UpdateStepContent();
            }
            catch { }
        }

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

            // Aplicar cantos arredondados personalizados ao botão browser mantendo o estilo do CreateStyledButton
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

            // Adicionar informações adicionais usando notification panel
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

            // Container para as opções
            var optionsContainer = new Border
            {
                Background = DevStackShared.ThemeManager.CurrentTheme.ControlBackground,
                BorderBrush = DevStackShared.ThemeManager.CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 18, 20, 18)
            };

            var optionsPanel = new StackPanel();

            // CheckBoxes usando ThemeManager
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

            // Adicionar informações sobre PATH usando notification panel
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

            // Container para progress bar
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
            installProgressBar.Value = 0; // Start at 0

            progressContainer.Child = installProgressBar;

            // Container para log
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

            // Configurar scroll bars
            ScrollViewer.SetHorizontalScrollBarVisibility(installLogListBox, ScrollBarVisibility.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(installLogListBox, ScrollBarVisibility.Auto);

            // Estilo para itens do log
            var logItemStyle = new Style(typeof(ListBoxItem));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0, 2, 0, 2)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.MarginProperty, new Thickness(0)));
            logItemStyle.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground));

            // Remove hover e seleção
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

            // Painel de informações da instalação
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
                            // Adiciona 20% de margem de segurança para arquivos temporários e instalação
                            return (long)(resourceStream.Length * 1.2);
                        }
                    }
                }
                
                // Fallback: tamanho estimado se não conseguir ler o recurso
                return 50 * 1024 * 1024; // 50MB
            }
            catch
            {
                // Fallback: tamanho estimado em caso de erro
                return 50 * 1024 * 1024; // 50MB
            }
        }

        private string GetSpaceRequirementText()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(installationPath) ?? "C:\\");
                var availableSpaceMB = drive.AvailableFreeSpace / (1024 * 1024); // MB
                var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0); // MB
                
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
                var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0); // MB
                return localization.GetString("installer.installation_path.space_required", $"{requiredSpace:F1}");
            }
        }

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

            var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0); // MB
            summary += $"\n\n{localization.GetString("installer.ready_to_install.space_required_summary").Replace("{0}", requiredSpace.ToString("F1"))}";

            return summary;
        }

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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > InstallerStep.Welcome)
            {
                currentStep--;
                UpdateStepContent();
            }
        }

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
                        // Start installation
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
                    // Launch DevStackGUI if requested
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = DevStackShared.ThemeManager.CreateStyledMessageBox(localization.GetString("installer.dialogs.cancel_message"), 
                localization.GetString("installer.dialogs.cancel_title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            // Only show confirmation if installation hasn't finished
            if (currentStep != InstallerStep.Finished)
            {
                var result = DevStackShared.ThemeManager.CreateStyledMessageBox(localization.GetString("installer.dialogs.cancel_message"), 
                    localization.GetString("installer.dialogs.cancel_title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true; // Cancel the window closing
                }
            }
        }

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
            
            // Multiple fallback URLs for .NET 9 SDK
            var downloadUrls = new[]
            {
                "https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.304/dotnet-sdk-9.0.304-win-x64.zip",
                "https://dotnetcli.azureedge.net/dotnet/Sdk/9.0.304/dotnet-sdk-9.0.304-win-x64.zip",
                "https://dotnetcli.blob.core.windows.net/dotnet/Sdk/9.0.304/dotnet-sdk-9.0.304-win-x64.zip"
            };
            
            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(15); // 15 minute timeout for large download
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
                                break; // Success, exit the loop
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
                        
                        // Clean up partial download
                        if (File.Exists(zipPath))
                        {
                            try { File.Delete(zipPath); } catch { }
                        }
                    }
                }
                
                // Check if download was successful
                if (!File.Exists(zipPath) || new FileInfo(zipPath).Length < 1024 * 1024) // Less than 1MB means failure
                {
                    // Fallback: Try using dotnet install script
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
            
            File.Delete(zipPath); // Clean up zip file
            
            return dotnetDir;
        }

        private async Task<string> TryDotNetInstallScript(string dotnetTempDir, string dotnetVersion)
        {
            try
            {
                AddInstallationLog("Downloading dotnet-install script...");
                
                string scriptPath = Path.Combine(dotnetTempDir, "dotnet-install.ps1");
                string dotnetDir = Path.Combine(dotnetTempDir, "dotnet");
                
                // Download the official dotnet install script
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
                
                // Verify installation
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
            
            // Create temporary build directory
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
                    
                    // Set progress: 35% (after SDK download) + 50% (compilation) distributed across projects
                    var baseProgress = 35;
                    var compilationProgress = (currentProject - 1) * 50 / projectCount;
                    installProgressBar.Value = baseProgress + compilationProgress;
                    
                    // Build to temporary directory
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
                        
                        // Read output asynchronously to prevent deadlock
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
                    
                    // Copy only .exe files to the installation directory
                    var exeFiles = Directory.GetFiles(projectTempDir, "*.exe", SearchOption.AllDirectories);
                    foreach (var exeFile in exeFiles)
                    {
                        var fileName = Path.GetFileName(exeFile);
                        var targetPath = Path.Combine(installPath, fileName);
                        
                        // Use the specified output name if it's the main executable
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
                // Clean up temporary build directory
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
            
            // Copy additional files
            var iconsrc = Path.Combine(sourceDir, "src", "Shared", "DevStack.ico");
            var iconDest = Path.Combine(installPath, "DevStack.ico");
            if (File.Exists(iconsrc))
            {
                File.Copy(iconsrc, iconDest, true);
            }
            
            // Copy configs if exists
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

        private async Task PerformInstallation()
        {
            string? tempZipPath = null;
            string? tempSourceDir = null;
            string? dotnetTempDir = null;
            
            try
            {
                // Reset progress bar
                installProgressBar.Value = 0;
                
                installStatusText.Content = localization.GetString("installer.installing.extracting");
                AddInstallationLog(localization.GetString("installer.log_messages.starting"));
                
                // Extract embedded source package
                tempZipPath = await Task.Run(() => ExtractEmbeddedZip());
                installProgressBar.Value = 5; // 5% complete
                AddInstallationLog("Source package extracted");

                // Extract source files to temporary directory
                tempSourceDir = Path.Combine(Path.GetTempPath(), "DevStackSource_" + Guid.NewGuid().ToString("N")[..8]);
                Directory.CreateDirectory(tempSourceDir);
                
                await Task.Run(() => 
                {
                    ExtractZipWithDeflate(tempZipPath, tempSourceDir);
                });
                installProgressBar.Value = 10; // 10% complete
                AddInstallationLog("Source files extracted");

                installStatusText.Content = "Downloading .NET SDK...";
                AddInstallationLog("Downloading .NET SDK for compilation...");
                
                // Download and extract .NET SDK
                dotnetTempDir = await DownloadDotNetSDK(tempSourceDir);
                installProgressBar.Value = 35; // 35% complete
                AddInstallationLog(".NET SDK downloaded and extracted");

                installStatusText.Content = localization.GetString("installer.installing.creating_directory");
                AddInstallationLog(localization.GetString("installer.log_messages.creating_dir", installationPath));
                Directory.CreateDirectory(installationPath);
                installProgressBar.Value = 40; // 40% complete

                // Compile projects using temporary .NET SDK
                installStatusText.Content = "Compiling DevStack projects...";
                await CompileProjects(tempSourceDir, dotnetTempDir, installationPath);
                installProgressBar.Value = 85; // 85% complete

                // Create settings.conf with selected language
                var settingsPath = Path.Combine(installationPath, "settings.conf");
                var languageCode = localization.CurrentLanguage;
                try
                {
                    Newtonsoft.Json.Linq.JObject settingsObj;
                    if (File.Exists(settingsPath))
                    {
                        var json = File.ReadAllText(settingsPath);
                        settingsObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                    }
                    else
                    {
                        settingsObj = new Newtonsoft.Json.Linq.JObject();
                    }
                    settingsObj["language"] = languageCode;
                    using (var sw = new StreamWriter(settingsPath))
                    using (var writer = new Newtonsoft.Json.JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.Indented })
                    {
                        settingsObj.WriteTo(writer);
                    }
                }
                catch { /* silent to not break installation */ }

                installStatusText.Content = localization.GetString("installer.installing.registering");
                AddInstallationLog(localization.GetString("installer.log_messages.registering"));
                await Task.Run(() => RegisterInstallation(installationPath));
                installProgressBar.Value = 90; // 90% complete

                if (createDesktopShortcuts)
                {
                    installStatusText.Content = localization.GetString("installer.installing.creating_desktop");
                    AddInstallationLog(localization.GetString("installer.log_messages.desktop_shortcuts"));
                    CreateDesktopShortcuts(installationPath);
                    installProgressBar.Value = 95; // 95% complete
                }

                if (createStartMenuShortcuts)
                {
                    installStatusText.Content = localization.GetString("installer.installing.creating_start_menu");
                    AddInstallationLog(localization.GetString("installer.log_messages.start_menu_shortcuts"));
                    CreateStartMenuShortcuts(installationPath);
                    installProgressBar.Value = 97; // 97% complete
                }

                if (addToPath)
                {
                    installStatusText.Content = localization.GetString("installer.installing.adding_path");
                    AddInstallationLog(localization.GetString("installer.log_messages.adding_path"));
                    AddToSystemPath(installationPath);
                    installProgressBar.Value = 99; // 99% complete
                }

                installStatusText.Content = localization.GetString("installer.installing.completed");
                installProgressBar.Value = 100; // 100% complete
                AddInstallationLog(localization.GetString("installer.log_messages.completed_success"));

                await Task.Delay(1000); // Give user time to see completion

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
                
                // Re-enable navigation
                backButton.IsEnabled = true;
                cancelButton.IsEnabled = true;
            }
            finally
            {
                // Clean up temporary files
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

        private void CreateStartMenuShortcuts(string installPath)
        {
            try
            {
                string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "DevStack Manager");
                Directory.CreateDirectory(startMenuPath);
                
                // Create shortcut for CLI
                string cliPath = Path.Combine(installPath, "DevStack.exe");
                if (File.Exists(cliPath))
                {
                    CreateShortcut(Path.Combine(startMenuPath, "DevStack CLI.lnk"), cliPath, "DevStack Command Line Interface");
                }

                // Create shortcut for GUI
                string guiPath = Path.Combine(installPath, "DevStackGUI.exe");
                if (File.Exists(guiPath))
                {
                    CreateShortcut(Path.Combine(startMenuPath, "DevStack GUI.lnk"), guiPath, "DevStack Graphical Interface");
                }

                // Create uninstaller shortcut
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

        private void AddToSystemPath(string installPath)
        {
            try
            {
                // Add to user PATH environment variable
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

        private void BrowserButton_Click(object sender, RoutedEventArgs e)
        {
            // Use SaveFileDialog to select folder (workaround)
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

        private void CreateDesktopShortcuts(string installPath)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                
                // Create shortcut for CLI
                string cliPath = Path.Combine(installPath, "DevStack.exe");
                if (File.Exists(cliPath))
                {
                    CreateShortcut(Path.Combine(desktopPath, "DevStack CLI.lnk"), cliPath, "DevStack Command Line Interface");
                }

                // Create shortcut for GUI
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

        private void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            try
            {
                // Create a simple batch file shortcut as fallback
                string batchContent = $"@echo off\ncd /d \"{Path.GetDirectoryName(targetPath)}\"\nstart \"\" \"{Path.GetFileName(targetPath)}\"";
                string batchPath = shortcutPath.Replace(".lnk", ".bat");
                File.WriteAllText(batchPath, batchContent);
                
                // Try to create proper shortcut using PowerShell
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
                
                // If PowerShell succeeded, remove the batch file
                if (process.ExitCode == 0 && File.Exists(shortcutPath))
                {
                    try { File.Delete(batchPath); } catch { }
                }
            }
            catch
            {
                // Fallback already created above
            }
        }

        private void RegisterInstallation(string installPath)
        {
            try
            {
                var version = GetVersion();
                var uninstallerPath = Path.Combine(installPath, "DevStack-Uninstaller.exe");
                var displayName = "DevStack Manager";
                
                // Register installation path in HKCU for easy access by uninstaller
                using var userKey = Registry.CurrentUser.CreateSubKey(@"Software\DevStack");
                userKey.SetValue("InstallPath", installPath);
                userKey.SetValue("Version", version);
                userKey.SetValue("InstallDate", DateTime.Now.ToString("yyyy-MM-dd"));
                
                // Register in Windows Programs and Features (if running as admin)
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
                    // Not running as admin, skip HKLM registration
                    // The program will still appear in user-specific locations
                }
            }
            catch
            {
                // Registry operations failed, continue anyway
            }
        }

        private void ExtractZipWithDeflate(string zipPath, string extractPath)
        {
            try
            {
                // Use standard .NET extraction for DEFLATE-compressed ZIP files
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, true);
                AddInstallationLog("Successfully extracted ZIP with DEFLATE compression");
            }
            catch (Exception ex)
            {
                AddInstallationLog($"DEFLATE extraction failed: {ex.Message}");
                throw new Exception($"Failed to extract ZIP file: {ex.Message}");
            }
        }

        private int GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                var size = dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                return (int)(size / 1024); // Size in KB
            }
            catch
            {
                return 0;
            }
        }
    }
}
