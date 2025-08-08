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

namespace DevStackInstaller
{
    public partial class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
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
        private LocalizationManager localization = LocalizationManager.Instance;
        private ComboBox languageComboBox = null!;
        private TextBlock languageLabel = null!;
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
        private TextBlock installStatusText = null!;
        private ListBox installLogListBox = null!;

        public InstallerWindow()
        {
            try
            {
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
            
            // Try to find the resource with different possible names
            var resourceNames = assembly.GetManifestResourceNames();
            var zipResourceName = resourceNames.FirstOrDefault(r => r.EndsWith("DevStack.zip"));
            
            if (zipResourceName == null)
            {
                throw new Exception("Embedded DevStack.zip resource not found in installer.");
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "DevStack_Install_" + Guid.NewGuid().ToString("N")[..8] + ".zip");
            
            using (var resourceStream = assembly.GetManifestResourceStream(zipResourceName))
            {
                if (resourceStream == null)
                {
                    throw new Exception("Could not access embedded DevStack.zip resource.");
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
            Title = localization.GetString("window_title", version);
            Width = 750;
            Height = 650;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            
            // Tema escuro moderno baseado na GUI
            Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)); // FormBackground
            Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)); // Foreground

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
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
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
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)) // Foreground
            };

            stepDescriptionText = new TextBlock
            {
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 186)), // TextSecondary
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            headerStackPanel.Children.Add(stepTitleText);
            headerStackPanel.Children.Add(stepDescriptionText);

            // Progress indicator com estilo moderno
            stepProgressBar = new ProgressBar
            {
                Width = 220,
                Height = 6,
                Margin = new Thickness(25, 0, 25, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Minimum = 0,
                Maximum = 6, // Total steps - 1
                Value = 0,
                Background = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                Foreground = new SolidColorBrush(Color.FromRgb(33, 136, 255)), // ButtonBackground
                BorderThickness = new Thickness(0)
            };

            // Adiciona efeito de sombra sutil
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
                Background = new SolidColorBrush(Color.FromRgb(27, 32, 40)), // PanelBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
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

            languageLabel = new TextBlock
            {
                Text = localization.GetString("welcome.language_label"),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 186)), // TextSecondary
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            languageComboBox = CreateStyledComboBox();

            // Populate language options
            var languages = localization.GetAvailableLanguages();
            foreach (var lang in languages)
            {
                var langName = localization.GetLanguageName(lang);
                var item = new ComboBoxItem
                {
                    Content = langName,
                    Tag = lang,
                    Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243))
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

            // Botão Back com estilo moderno
            backButton = CreateStyledButton(localization.GetString("buttons.back"), 90, false);
            backButton.Margin = new Thickness(0, 0, 12, 0);
            backButton.IsEnabled = false;
            backButton.Click += BackButton_Click;

            // Botão Next com estilo de destaque
            nextButton = CreateStyledButton(localization.GetString("buttons.next"), 90, true);
            nextButton.Margin = new Thickness(0, 0, 12, 0);
            nextButton.Click += NextButton_Click;

            // Botão Cancel
            cancelButton = CreateStyledButton(localization.GetString("buttons.cancel"), 90, false);
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

        private Button CreateStyledButton(string content, double width, bool isPrimary)
        {
            var button = new Button
            {
                Content = content,
                Width = width,
                Height = 36,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };

            if (isPrimary)
            {
                // Botão primário (estilo destaque)
                button.Background = new SolidColorBrush(Color.FromRgb(33, 136, 255)); // ButtonBackground
                button.Foreground = new SolidColorBrush(Colors.White);
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(33, 136, 255));
                button.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                // Botão secundário
                button.Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)); // ControlBackground
                button.Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)); // Foreground
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)); // Border
            }

            // Estilo simplificado com triggers
            var buttonStyle = new Style(typeof(Button));
            
            // Template básico com cantos arredondados
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            // Triggers para hover e pressed
            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            if (isPrimary)
            {
                hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(58, 150, 255)))); // ButtonHover
                hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(58, 150, 255))));
            }
            else
            {
                hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 55, 68))));
                hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));
            }

            var pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressedTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.8));

            var disabledTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disabledTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.5));
            disabledTrigger.Setters.Add(new Setter(Button.CursorProperty, Cursors.Arrow));

            template.Triggers.Add(hoverTrigger);
            template.Triggers.Add(pressedTrigger);
            template.Triggers.Add(disabledTrigger);

            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, template));
            button.Style = buttonStyle;

            // Adiciona efeito de sombra sutil
            button.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 6,
                ShadowDepth = 2,
                Opacity = 0.2,
                Color = Colors.Black
            };

            return button;
        }

        private ComboBox CreateStyledComboBox()
        {
            var comboBox = new ComboBox
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // InputBackground
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // InputForeground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // InputBorder
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 14,
                MinHeight = 35,
                Width = 180
            };

            // Evento para rolar o dropdown para o início ao abrir
            comboBox.DropDownOpened += (s, e) =>
            {
                // Tenta encontrar o ScrollViewer do dropdown
                if (comboBox.Template != null)
                {
                    comboBox.ApplyTemplate();
                    var popup = comboBox.Template.FindName("Popup", comboBox) as System.Windows.Controls.Primitives.Popup;
                    if (popup != null && popup.Child is Border border)
                    {
                        var scrollViewer = FindScrollViewer(border);
                        if (scrollViewer != null)
                        {
                            scrollViewer.ScrollToTop();
                        }
                    }
                }
            };

            // Helper para buscar ScrollViewer dentro do Border
            static ScrollViewer? FindScrollViewer(DependencyObject parent)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is ScrollViewer sv)
                        return sv;
                    var result = FindScrollViewer(child);
                    if (result != null)
                        return result;
                }
                return null;
            }

            // Create a simplified but effective style for dark theme
            var comboStyle = new Style(typeof(ComboBox));

            // Basic styling - force dark colors
            comboStyle.Setters.Add(new Setter(ComboBox.BackgroundProperty, new SolidColorBrush(Color.FromRgb(32, 39, 49))));
            comboStyle.Setters.Add(new Setter(ComboBox.ForegroundProperty, new SolidColorBrush(Color.FromRgb(230, 237, 243))));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(48, 54, 61))));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(1)));
            comboStyle.Setters.Add(new Setter(ComboBox.PaddingProperty, new Thickness(10, 8, 10, 8)));
            comboStyle.Setters.Add(new Setter(ComboBox.FontSizeProperty, 14.0));
            comboStyle.Setters.Add(new Setter(ComboBox.MinHeightProperty, 35.0));

            // Create simplified template using XAML string
            var templateXaml = @"
                <ControlTemplate TargetType='ComboBox' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Border Name='MainBorder' 
                            Background='{TemplateBinding Background}'
                            BorderBrush='{TemplateBinding BorderBrush}'
                            BorderThickness='{TemplateBinding BorderThickness}'
                            CornerRadius='3'>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width='*'/>
                                <ColumnDefinition Width='20'/>
                            </Grid.ColumnDefinitions>
                            
                            <!-- Content presenter for selected item -->
                            <ContentPresenter Name='ContentSite'
                                            Grid.Column='0'
                                            Margin='10,5,10,8'
                                            VerticalAlignment='Top'
                                            HorizontalAlignment='Left'
                                            Content='{TemplateBinding SelectionBoxItem}'
                                            ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}'
                                            ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}'
                                            IsHitTestVisible='False'/>
                            
                            <!-- Toggle button for dropdown -->
                            <ToggleButton Name='ToggleButton'
                                        Grid.Column='0'
                                        Grid.ColumnSpan='2'
                                        Background='Transparent'
                                        BorderBrush='Transparent'
                                        BorderThickness='0'
                                        Focusable='False'
                                        ClickMode='Press'
                                        IsChecked='{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}}'>
                                <ToggleButton.Template>
                                    <ControlTemplate TargetType='ToggleButton'>
                                        <Border Background='Transparent'>
                                            <Path Name='Arrow'
                                                  Data='M 0 0 L 4 4 L 8 0 Z'
                                                  Fill='{Binding Foreground, RelativeSource={RelativeSource AncestorType=ComboBox}}'
                                                  HorizontalAlignment='Right'
                                                  VerticalAlignment='Center'
                                                  Margin='0,0,8,0'/>
                                        </Border>
                                    </ControlTemplate>
                                </ToggleButton.Template>
                            </ToggleButton>
                            
                            <!-- Popup for dropdown -->
                            <Popup Name='Popup'
                                   Placement='Bottom'
                                   IsOpen='{TemplateBinding IsDropDownOpen}'
                                   AllowsTransparency='True'
                                   Focusable='False'
                                   PopupAnimation='Slide'>
                                <Border Name='DropDownBorder'
                                        Background='#FF2D2D30'
                                        BorderBrush='#FF3F3F46'
                                        BorderThickness='1'
                                        CornerRadius='3'
                                        MinWidth='{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}'
                                        MaxHeight='{TemplateBinding MaxDropDownHeight}'>
                                    <ScrollViewer Name='DropDownScrollViewer'
                                                  CanContentScroll='True'>
                                        <ItemsPresenter KeyboardNavigation.DirectionalNavigation='Contained'/>
                                    </ScrollViewer>
                                </Border>
                            </Popup>
                        </Grid>
                    </Border>
                    
                    <ControlTemplate.Triggers>
                        <Trigger Property='IsMouseOver' Value='True'>
                            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#FF007ACC'/>
                        </Trigger>
                        <Trigger Property='IsFocused' Value='True'>
                            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#FF007ACC'/>
                            <Setter TargetName='MainBorder' Property='BorderThickness' Value='2'/>
                        </Trigger>
                        <Trigger Property='IsDropDownOpen' Value='True'>
                            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#FF007ACC'/>
                            <Setter TargetName='MainBorder' Property='BorderThickness' Value='2'/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>";

            try
            {
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);
                comboStyle.Setters.Add(new Setter(ComboBox.TemplateProperty, template));
            }
            catch
            {
                // Fallback to basic triggers if XAML parsing fails
                var hoverTrigger = new Trigger { Property = ComboBox.IsMouseOverProperty, Value = true };
                hoverTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));
                comboStyle.Triggers.Add(hoverTrigger);

                var focusTrigger = new Trigger { Property = ComboBox.IsFocusedProperty, Value = true };
                focusTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));
                focusTrigger.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(2)));
                comboStyle.Triggers.Add(focusTrigger);

                var dropdownTrigger = new Trigger { Property = ComboBox.IsDropDownOpenProperty, Value = true };
                dropdownTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));
                dropdownTrigger.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(2)));
                comboStyle.Triggers.Add(dropdownTrigger);
            }

            comboBox.Style = comboStyle;

            // Style the dropdown items for dark theme
            var itemStyle = new Style(typeof(ComboBoxItem));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(27, 32, 40))));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Color.FromRgb(230, 237, 243))));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.PaddingProperty, new Thickness(10, 6, 10, 6)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BorderThicknessProperty, new Thickness(0)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.FontSizeProperty, 14.0));

            // Hover trigger for items
            var itemHoverTrigger = new Trigger
            {
                Property = ComboBoxItem.IsMouseOverProperty,
                Value = true
            };
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 55, 68))));
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Color.FromRgb(230, 237, 243))));

            // Selected trigger for items
            var itemSelectedTrigger = new Trigger
            {
                Property = ComboBoxItem.IsSelectedProperty,
                Value = true
            };
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));

            // Highlighted trigger for items
            var itemHighlightedTrigger = new Trigger
            {
                Property = ComboBoxItem.IsHighlightedProperty,
                Value = true
            };
            itemHighlightedTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));
            itemHighlightedTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));

            itemStyle.Triggers.Add(itemHoverTrigger);
            itemStyle.Triggers.Add(itemSelectedTrigger);
            itemStyle.Triggers.Add(itemHighlightedTrigger);

            comboBox.ItemContainerStyle = itemStyle;

            // Override system colors to force dark theme
            comboBox.Resources.Clear();
            comboBox.Resources.Add(SystemColors.WindowBrushKey, new SolidColorBrush(Color.FromRgb(27, 32, 40)));
            comboBox.Resources.Add(SystemColors.ControlBrushKey, new SolidColorBrush(Color.FromRgb(32, 39, 49)));
            comboBox.Resources.Add(SystemColors.ControlTextBrushKey, new SolidColorBrush(Color.FromRgb(230, 237, 243)));
            comboBox.Resources.Add(SystemColors.HighlightBrushKey, new SolidColorBrush(Color.FromRgb(33, 136, 255)));
            comboBox.Resources.Add(SystemColors.HighlightTextBrushKey, new SolidColorBrush(Colors.White));

            return comboBox;
        }

        private MessageBoxResult CreateStyledMessageBox(string message, string title = "Mensagem", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            // Cria uma janela customizada para garantir tema escuro real
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                Background = Brushes.Transparent, // Para permitir borda arredondada
                AllowsTransparency = true,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                Owner = this
            };

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)), // FormBackground
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                SnapsToDevicePixels = true
            };

            var grid = new Grid { Margin = new Thickness(0) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Mensagem
            var msg = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                Background = Brushes.Transparent,
                FontSize = 16,
                Margin = new Thickness(55, 15, 30, 10),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(msg, 0);
            grid.Children.Add(msg);

            // Botões
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 20, 20)
            };

            MessageBoxResult result = MessageBoxResult.None;
            void CloseAndSetResult(MessageBoxResult r) { result = r; dialog.DialogResult = true; dialog.Close(); }

            void AddButton(string text, MessageBoxResult r, bool isDefault = false, Color? customColor = null)
            {
                var btn = CreateStyledButton(text, 80, !isDefault); // Use secondary style for non-default buttons
                btn.MinWidth = 80;
                btn.Margin = new Thickness(8, 0, 0, 0);
                btn.Padding = new Thickness(10, 5, 10, 5);
                btn.FontWeight = isDefault ? FontWeights.Bold : FontWeights.Normal;
                
                // Override styling for default button
                if (isDefault)
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(33, 136, 255)); // ButtonBackground
                    btn.Foreground = new SolidColorBrush(Colors.White);
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(33, 136, 255));
                }
                
                // Apply custom color if provided
                if (customColor.HasValue)
                {
                    btn.Background = new SolidColorBrush(customColor.Value);
                    btn.Foreground = new SolidColorBrush(Colors.White);
                    btn.BorderBrush = new SolidColorBrush(customColor.Value);
                }
                
                btn.Click += (s, e) => CloseAndSetResult(r);
                buttonPanel.Children.Add(btn);
            }

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK, true);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, true);
                    AddButton(localization.GetString("buttons.cancel"), MessageBoxResult.Cancel);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton(localization.GetString("dialogs.yes"), MessageBoxResult.Yes, true);
                    AddButton(localization.GetString("dialogs.no"), MessageBoxResult.No, false, Color.FromRgb(248, 81, 73)); // Danger color
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton(localization.GetString("dialogs.yes"), MessageBoxResult.Yes, true);
                    AddButton(localization.GetString("dialogs.no"), MessageBoxResult.No, false, Color.FromRgb(248, 81, 73)); // Danger color
                    AddButton(localization.GetString("buttons.cancel"), MessageBoxResult.Cancel);
                    break;
            }

            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            // Ícone
            if (icon != MessageBoxImage.None)
            {
                var iconText = new TextBlock
                {
                    Margin = new Thickness(5, 10, 15, 0),
                    FontSize = 32,
                    VerticalAlignment = VerticalAlignment.Top
                };
                switch (icon)
                {
                    case MessageBoxImage.Information:
                        iconText.Text = "ℹ️";
                        iconText.Foreground = new SolidColorBrush(Color.FromRgb(56, 211, 159)); // Accent
                        break;
                    case MessageBoxImage.Warning:
                        iconText.Text = "⚠️";
                        iconText.Foreground = new SolidColorBrush(Color.FromRgb(255, 196, 0)); // Warning
                        break;
                    case MessageBoxImage.Error:
                        iconText.Text = "❌";
                        iconText.Foreground = new SolidColorBrush(Color.FromRgb(248, 81, 73)); // Danger
                        break;
                    case MessageBoxImage.Question:
                        iconText.Text = "❓";
                        iconText.Foreground = new SolidColorBrush(Color.FromRgb(56, 211, 159)); // Accent
                        break;
                }
                grid.Children.Add(iconText);
            }

            border.Child = grid;
            dialog.Content = border;
            dialog.ShowDialog();
            return result;
        }

        private void UpdateStepContent()
        {
            // Clear content
            contentGrid.Children.Clear();
            contentGrid.RowDefinitions.Clear();
            contentGrid.ColumnDefinitions.Clear();

            // Atualizar área de conteúdo com tema escuro
            contentGrid.Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)); // FormBackground
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
                    nextButton.Content = localization.GetString("buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.License:
                    CreateLicenseStep();
                    nextButton.Content = localization.GetString("buttons.accept");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.InstallationPath:
                    CreateInstallationPathStep();
                    nextButton.Content = localization.GetString("buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.Components:
                    CreateComponentsStep();
                    nextButton.Content = localization.GetString("buttons.next");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.ReadyToInstall:
                    CreateReadyToInstallStep();
                    nextButton.Content = localization.GetString("buttons.install");
                    nextButton.IsEnabled = true;
                    break;
                case InstallerStep.Installing:
                    CreateInstallingStep();
                    nextButton.IsEnabled = false;
                    backButton.IsEnabled = false;
                    break;
                case InstallerStep.Finished:
                    CreateFinishedStep();
                    nextButton.Content = localization.GetString("buttons.finish");
                    nextButton.IsEnabled = true;
                    backButton.IsEnabled = false;
                    cancelButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void CreateWelcomeStep()
        {
            stepTitleText.Text = localization.GetString("welcome.title");
            stepDescriptionText.Text = localization.GetString("welcome.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Welcome panel com design moderno
            var welcomePanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                Margin = new Thickness(0, 20, 0, 20)
            };

            // Container principal com borda arredondada
            var welcomeContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(40, 35, 40, 35),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 10,
                    ShadowDepth = 3,
                    Opacity = 0.3,
                    Color = Colors.Black
                }
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

            var welcomeText = new TextBlock
            {
                Text = localization.GetString("welcome.app_name"),
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                Margin = new Thickness(0, 0, 0, 8)
            };

            var versionText = new TextBlock
            {
                Text = localization.GetString("welcome.version", GetVersion()),
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 186)), // TextSecondary
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 25)
            };

            var descriptionText = new TextBlock
            {
                Text = localization.GetString("welcome.app_description"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                LineHeight = 22,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 186)), // TextSecondary
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

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (languageComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string languageCode)
            {
                localization.LoadLanguage(languageCode);
                
                // Update window title
                Title = localization.GetString("window_title", GetVersion());
                
                // Update language label
                languageLabel.Text = localization.GetString("welcome.language_label");
                
                // Update all UI text
                UpdateStepContent();
            }
        }

        private void CreateLicenseStep()
        {
            stepTitleText.Text = localization.GetString("license.title");
            stepDescriptionText.Text = localization.GetString("license.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var licenseLabel = new TextBlock
            {
                Text = localization.GetString("license.label"),
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                FontSize = 15,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var licenseContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 17, 23)), // ConsoleBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(0)
            };

            var licenseTextBox = CreateStyledTextBox();
            licenseTextBox.Text = localization.GetString("license.text");
            licenseTextBox.IsReadOnly = true;
            licenseTextBox.TextWrapping = TextWrapping.Wrap;
            licenseTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            licenseTextBox.FontFamily = new FontFamily("Consolas");
            licenseTextBox.FontSize = 12;
            licenseTextBox.Background = new SolidColorBrush(Color.FromRgb(13, 17, 23)); // ConsoleBackground
            licenseTextBox.Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)); // ConsoleForeground
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
            stepTitleText.Text = localization.GetString("installation_path.title");
            stepDescriptionText.Text = localization.GetString("installation_path.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var pathLabel = new TextBlock
            {
                Text = localization.GetString("installation_path.label"),
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                FontSize = 15,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var pathContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(0), // Remove padding para integração total
                Margin = new Thickness(0, 0, 0, 20)
            };

            var pathGrid = new Grid();
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            pathTextBox = new TextBox
            {
                Text = installationPath,
                Height = 40,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                BorderThickness = new Thickness(0),
                Padding = new Thickness(15, 0, 15, 0),
                Margin = new Thickness(0),
                SelectionBrush = new SolidColorBrush(Color.FromArgb(128, 33, 136, 255))
            };
            pathTextBox.TextChanged += (s, e) => installationPath = pathTextBox.Text;

            var browserButton = new Button
            {
                Content = localization.GetString("installation_path.browser"),
                Width = 130,
                Height = 40, 
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Background = new SolidColorBrush(Color.FromRgb(45, 55, 68)),
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
                BorderThickness = new Thickness(1, 0, 0, 0),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0)
            };
            browserButton.Click += BrowserButton_Click;

            // Adicionar estilo hover ao botão Browse
            var browseStyle = new Style(typeof(Button));
            
            // Template com cantos arredondados
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
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255))));

            template.Triggers.Add(hoverTrigger);
            browseStyle.Setters.Add(new Setter(Button.TemplateProperty, template));
            browserButton.Style = browseStyle;

            Grid.SetColumn(pathTextBox, 0);
            Grid.SetColumn(browserButton, 1);
            pathGrid.Children.Add(pathTextBox);
            pathGrid.Children.Add(browserButton);
            pathContainer.Child = pathGrid;

            var spaceLabel = new TextBlock
            {
                Text = GetSpaceRequirementText(),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)), // TextMuted
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Adicionar informações adicionais
            var infoPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(56, 211, 159)), // Accent (para info)
                BorderThickness = new Thickness(1, 1, 1, 3),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 10, 0, 0)
            };

            var infoText = new TextBlock
            {
                Text = localization.GetString("installation_path.info"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 186)), // TextSecondary
                TextWrapping = TextWrapping.Wrap
            };

            infoPanel.Child = infoText;

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
            stepTitleText.Text = localization.GetString("components.title");
            stepDescriptionText.Text = localization.GetString("components.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var optionsLabel = new TextBlock
            {
                Text = localization.GetString("components.label"),
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                FontSize = 15,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Container para as opções
            var optionsContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 18, 20, 18)
            };

            var optionsPanel = new StackPanel();

            // CheckBoxes com estilo moderno
            desktopShortcutCheckBox = CreateStyledCheckBox(localization.GetString("components.desktop_shortcuts"), createDesktopShortcuts);
            desktopShortcutCheckBox.Checked += (s, e) => createDesktopShortcuts = true;
            desktopShortcutCheckBox.Unchecked += (s, e) => createDesktopShortcuts = false;
            desktopShortcutCheckBox.Margin = new Thickness(0, 0, 0, 15);

            startMenuShortcutCheckBox = CreateStyledCheckBox(localization.GetString("components.start_menu_shortcuts"), createStartMenuShortcuts);
            startMenuShortcutCheckBox.Checked += (s, e) => createStartMenuShortcuts = true;
            startMenuShortcutCheckBox.Unchecked += (s, e) => createStartMenuShortcuts = false;
            startMenuShortcutCheckBox.Margin = new Thickness(0, 0, 0, 15);

            addToPathCheckBox = CreateStyledCheckBox(localization.GetString("components.add_to_path"), addToPath);
            addToPathCheckBox.Checked += (s, e) => addToPath = true;
            addToPathCheckBox.Unchecked += (s, e) => addToPath = false;
            addToPathCheckBox.Margin = new Thickness(0, 0, 0, 0);

            optionsPanel.Children.Add(desktopShortcutCheckBox);
            optionsPanel.Children.Add(startMenuShortcutCheckBox);
            optionsPanel.Children.Add(addToPathCheckBox);

            optionsContainer.Child = optionsPanel;

            // Adicionar informações sobre PATH
            var pathInfoPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 196, 0)), // Warning
                BorderThickness = new Thickness(1, 1, 1, 3),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 20, 0, 0)
            };

            var pathInfoText = new TextBlock
            {
                Text = localization.GetString("components.path_info"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(166, 173, 186)), // TextSecondary
                TextWrapping = TextWrapping.Wrap
            };

            pathInfoPanel.Child = pathInfoText;

            Grid.SetRow(optionsLabel, 0);
            Grid.SetRow(optionsContainer, 1);
            Grid.SetRow(pathInfoPanel, 2);
            
            contentGrid.Children.Add(optionsLabel);
            contentGrid.Children.Add(optionsContainer);
            contentGrid.Children.Add(pathInfoPanel);
        }

        private CheckBox CreateStyledCheckBox(string content, bool isChecked)
        {
            var checkBox = new CheckBox
            {
                Content = content,
                IsChecked = isChecked,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            // Template customizado para o CheckBox
            var template = new ControlTemplate(typeof(CheckBox));
            var panel = new FrameworkElementFactory(typeof(StackPanel));
            panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            panel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Box do check
            var checkBorder = new FrameworkElementFactory(typeof(Border));
            checkBorder.Name = "CheckBorder";
            checkBorder.SetValue(Border.WidthProperty, 20.0);
            checkBorder.SetValue(Border.HeightProperty, 20.0);
            checkBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            checkBorder.SetValue(Border.BorderThicknessProperty, new Thickness(2));
            checkBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(48, 54, 61))); // Border
            checkBorder.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            checkBorder.SetValue(Border.MarginProperty, new Thickness(0, 0, 12, 0));

            // Ícone de check
            var checkIcon = new FrameworkElementFactory(typeof(TextBlock));
            checkIcon.Name = "CheckIcon";
            checkIcon.SetValue(TextBlock.TextProperty, "✓");
            checkIcon.SetValue(TextBlock.FontSizeProperty, 14.0);
            checkIcon.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            checkIcon.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            checkIcon.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkIcon.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            checkIcon.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);

            // Content presenter
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            checkBorder.AppendChild(checkIcon);
            panel.AppendChild(checkBorder);
            panel.AppendChild(contentPresenter);
            template.VisualTree = panel;

            // Triggers
            var checkedTrigger = new Trigger { Property = CheckBox.IsCheckedProperty, Value = true };
            checkedTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "CheckIcon"));
            checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255)), "CheckBorder")); // ButtonBackground
            checkedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255)), "CheckBorder"));

            var hoverTrigger = new Trigger { Property = CheckBox.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(33, 136, 255)), "CheckBorder")); // ButtonBackground

            template.Triggers.Add(checkedTrigger);
            template.Triggers.Add(hoverTrigger);

            var style = new Style(typeof(CheckBox));
            style.Setters.Add(new Setter(CheckBox.TemplateProperty, template));
            checkBox.Style = style;

            return checkBox;
        }

        private TextBox CreateStyledTextBox()
        {
            var textBox = new TextBox();
            
            // Estilo customizado com scrollbar moderna
            var textBoxStyle = new Style(typeof(TextBox));
            
            // Template XAML para TextBox com scrollbar customizada
            var templateXaml = @"
                <ControlTemplate TargetType='TextBox' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Border Name='Border'
                            Background='{TemplateBinding Background}'
                            BorderBrush='{TemplateBinding BorderBrush}'
                            BorderThickness='{TemplateBinding BorderThickness}'
                            CornerRadius='0'>
                        <ScrollViewer Name='PART_ContentHost'
                                      Focusable='False'
                                      HorizontalScrollBarVisibility='{TemplateBinding HorizontalScrollBarVisibility}'
                                      VerticalScrollBarVisibility='{TemplateBinding VerticalScrollBarVisibility}'>
                            <ScrollViewer.Resources>
                                <!-- Estilo customizado para ScrollBar -->
                                <Style TargetType='ScrollBar'>
                                    <Setter Property='Background' Value='Transparent'/>
                                    <Setter Property='Width' Value='12'/>
                                    <Setter Property='Template'>
                                        <Setter.Value>
                                            <ControlTemplate TargetType='ScrollBar'>
                                                <Grid>
                                                    <Rectangle Fill='#FF1B1F23' Opacity='0.3'/>
                                                    <Track Name='PART_Track' IsDirectionReversed='True'>
                                                        <Track.Thumb>
                                                            <Thumb>
                                                                <Thumb.Template>
                                                                    <ControlTemplate TargetType='Thumb'>
                                                                        <Border Background='#FF555555' 
                                                                                CornerRadius='6' 
                                                                                Margin='2'/>
                                                                    </ControlTemplate>
                                                                </Thumb.Template>
                                                            </Thumb>
                                                        </Track.Thumb>
                                                    </Track>
                                                </Grid>
                                                
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter TargetName='PART_Track' Property='Thumb.Template'>
                                                            <Setter.Value>
                                                                <ControlTemplate TargetType='Thumb'>
                                                                    <Border Background='#FF777777' 
                                                                            CornerRadius='6' 
                                                                            Margin='2'/>
                                                                </ControlTemplate>
                                                            </Setter.Value>
                                                        </Setter>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </ScrollViewer.Resources>
                        </ScrollViewer>
                    </Border>
                </ControlTemplate>";

            try
            {
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);
                textBoxStyle.Setters.Add(new Setter(TextBox.TemplateProperty, template));
                textBox.Style = textBoxStyle;
            }
            catch
            {
                // Fallback - usar estilo básico se XAML falhar
                // Ao menos define algumas propriedades básicas
            }
            
            return textBox;
        }

        private void CreateReadyToInstallStep()
        {
            stepTitleText.Text = localization.GetString("ready_to_install.title");
            stepDescriptionText.Text = localization.GetString("ready_to_install.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var summaryLabel = new TextBlock
            {
                Text = localization.GetString("ready_to_install.summary_label"),
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                FontSize = 15,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var summaryContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 17, 23)), // ConsoleBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(0)
            };

            var summaryText = new TextBlock
            {
                Text = GetInstallationSummary(),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)), // ConsoleForeground
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
            stepTitleText.Text = localization.GetString("installing.title");
            stepDescriptionText.Text = localization.GetString("installing.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            installStatusText = new TextBlock
            {
                Text = localization.GetString("installing.preparing"),
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)), // Foreground
                FontSize = 15,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Container para progress bar
            var progressContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 18, 20, 18),
                Margin = new Thickness(0, 0, 0, 20)
            };

            installProgressBar = new ProgressBar
            {
                Height = 8,
                IsIndeterminate = true,
                Background = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                Foreground = new SolidColorBrush(Color.FromRgb(33, 136, 255)), // ButtonBackground
                BorderThickness = new Thickness(0)
            };

            progressContainer.Child = installProgressBar;

            // Container para log
            var logContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 17, 23)), // ConsoleBackground
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)), // Border
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(0)
            };

            installLogListBox = new ListBox
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromRgb(13, 17, 23)), // ConsoleBackground
                Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)), // ConsoleForeground
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
            logItemStyle.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Color.FromRgb(201, 209, 217))));

            // Remove hover e seleção
            var hoverTrigger = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            logItemStyle.Triggers.Add(hoverTrigger);

            var selectedTrigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Color.FromRgb(201, 209, 217))));
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
            stepTitleText.Text = localization.GetString("finished.title");
            stepDescriptionText.Text = localization.GetString("finished.description");

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var finishedPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var successIcon = new TextBlock
            {
                Text = localization.GetString("finished.success_icon"),
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var successText = new TextBlock
            {
                Text = localization.GetString("finished.success_title"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(63, 185, 80)) // SuccessColor
            };

            var finishedMessage = new TextBlock
            {
                Text = localization.GetString("finished.success_message"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                Margin = new Thickness(40, 0, 40, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)) // TextColor
            };

            // Painel de informações da instalação
            var infoPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 39, 49)), // ControlBackground
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(20, 10, 20, 20),
                Padding = new Thickness(15),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = localization.GetString("finished.install_location"),
                            FontWeight = FontWeights.SemiBold,
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)), // White
                            Margin = new Thickness(0, 0, 0, 5)
                        },
                        new TextBlock
                        {
                            Text = installationPath,
                            Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)), // TextColor
                            FontFamily = new FontFamily("Consolas"),
                            FontSize = 11
                        }
                    }
                }
            };

            var launchPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var launchCheckBox = CreateStyledCheckBox(localization.GetString("finished.launch_now"), launchAfterInstall);
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
                
                return localization.GetString("installation_path.space_required", $"{requiredSpace:F1}") + "  |  " + 
                       localization.GetString("installation_path.space_available", availableSpaceText);
            }
            catch
            {
                var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0); // MB
                return localization.GetString("installation_path.space_required", $"{requiredSpace:F1}");
            }
        }

        private string GetInstallationSummary()
        {
            var summary = $@"{localization.GetString("ready_to_install.destination")}
  {installationPath}

{localization.GetString("ready_to_install.components_header")}
{localization.GetString("ready_to_install.cli_component")}
{localization.GetString("ready_to_install.gui_component")}
{localization.GetString("ready_to_install.uninstaller_component")}
{localization.GetString("ready_to_install.config_component")}

{localization.GetString("ready_to_install.options_header")}";

            if (createDesktopShortcuts)
                summary += $"\n{localization.GetString("ready_to_install.create_desktop")}";
            if (createStartMenuShortcuts)
                summary += $"\n{localization.GetString("ready_to_install.create_start_menu")}";
            if (addToPath)
                summary += $"\n{localization.GetString("ready_to_install.add_path")}";

            var requiredSpace = GetRequiredSpaceBytes() / (1024.0 * 1024.0); // MB
            summary += $"\n\n{localization.GetString("ready_to_install.space_required_summary").Replace("{0}", requiredSpace.ToString("F1"))}";

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
                            CreateStyledMessageBox($"Could not launch DevStack GUI: {ex.Message}", 
                                "Launch Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    Application.Current.Shutdown();
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = CreateStyledMessageBox(localization.GetString("dialogs.cancel_message"), 
                localization.GetString("dialogs.cancel_title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            
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
                var result = CreateStyledMessageBox(localization.GetString("dialogs.cancel_message"), 
                    localization.GetString("dialogs.cancel_title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true; // Cancel the window closing
                }
            }
        }

        private async Task PerformInstallation()
        {
            string? tempZipPath = null;
            try
            {
                installStatusText.Text = localization.GetString("installing.extracting");
                AddInstallationLog(localization.GetString("log_messages.starting"));
                
                tempZipPath = await Task.Run(() => ExtractEmbeddedZip());
                AddInstallationLog(localization.GetString("log_messages.extracted"));

                installStatusText.Text = localization.GetString("installing.creating_directory");
                AddInstallationLog(localization.GetString("log_messages.creating_dir", installationPath));
                Directory.CreateDirectory(installationPath);

                installStatusText.Text = localization.GetString("installing.installing_files");
                AddInstallationLog(localization.GetString("log_messages.installing"));
                await Task.Run(() => 
                {
                    ZipFile.ExtractToDirectory(tempZipPath, installationPath, true);
                });

                installStatusText.Text = localization.GetString("installing.registering");
                AddInstallationLog(localization.GetString("log_messages.registering"));
                await Task.Run(() => RegisterInstallation(installationPath));

                if (createDesktopShortcuts)
                {
                    installStatusText.Text = localization.GetString("installing.creating_desktop");
                    AddInstallationLog(localization.GetString("log_messages.desktop_shortcuts"));
                    CreateDesktopShortcuts(installationPath);
                }

                if (createStartMenuShortcuts)
                {
                    installStatusText.Text = localization.GetString("installing.creating_start_menu");
                    AddInstallationLog(localization.GetString("log_messages.start_menu_shortcuts"));
                    CreateStartMenuShortcuts(installationPath);
                }

                if (addToPath)
                {
                    installStatusText.Text = localization.GetString("installing.adding_path");
                    AddInstallationLog(localization.GetString("log_messages.adding_path"));
                    AddToSystemPath(installationPath);
                }

                installStatusText.Text = localization.GetString("installing.completed");
                installProgressBar.IsIndeterminate = false;
                installProgressBar.Value = 100;
                AddInstallationLog(localization.GetString("log_messages.completed_success"));

                await Task.Delay(1000); // Give user time to see completion

                currentStep = InstallerStep.Finished;
                UpdateStepContent();
            }
            catch (Exception ex)
            {
                AddInstallationLog($"ERROR: {ex.Message}");
                CreateStyledMessageBox(localization.GetString("dialogs.installation_error_message", ex.Message), 
                    localization.GetString("dialogs.installation_error_title"), 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                installStatusText.Text = localization.GetString("dialogs.installation_error_title");
                installProgressBar.IsIndeterminate = false;
                installProgressBar.Value = 0;
                
                // Re-enable navigation
                backButton.IsEnabled = true;
                cancelButton.IsEnabled = true;
            }
            finally
            {
                // Clean up temporary zip file
                if (tempZipPath != null && File.Exists(tempZipPath))
                {
                    try
                    {
                        File.Delete(tempZipPath);
                        AddInstallationLog(localization.GetString("log_messages.cleanup"));
                    }
                    catch (Exception ex)
                    {
                        AddInstallationLog($"Warning: Could not delete temporary file: {ex.Message}");
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
                    AddInstallationLog(localization.GetString("log_messages.path_added"));
                }
                else
                {
                    AddInstallationLog(localization.GetString("log_messages.path_exists"));
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
                Title = localization.GetString("dialogs.folder_dialog_title"),
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
