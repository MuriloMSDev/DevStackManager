using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace DevStackManager
{
    /// <summary>
    /// Status bar component responsible for creating and managing the bottom status bar.
    /// Provides status message display, theme selector (Dark/Light), and language selector.
    /// Integrates with ThemeManager and LocalizationManager for real-time UI updates.
    /// Persists user preferences to DevStackConfig.
    /// </summary>
    public static class GuiStatusBar
    {
        /// <summary>
        /// Height of the status bar.
        /// </summary>
        private const double STATUS_BAR_HEIGHT = 30;
        
        /// <summary>
        /// Left padding for status bar content.
        /// </summary>
        private const double STATUS_BAR_PADDING_LEFT = 5;
        
        /// <summary>
        /// Top border thickness for status bar.
        /// </summary>
        private const double STATUS_BAR_BORDER_TOP = 1;
        
        /// <summary>
        /// Minimum width for theme selector combo box.
        /// </summary>
        private const double THEME_COMBOBOX_MIN_WIDTH = 120;
        
        /// <summary>
        /// Minimum width for language selector combo box.
        /// </summary>
        private const double LANGUAGE_COMBOBOX_MIN_WIDTH = 160;
        
        /// <summary>
        /// Font size for status label text.
        /// </summary>
        private const double STATUS_LABEL_FONT_SIZE = 12;
        
        /// <summary>
        /// Right margin for dropdown arrow path.
        /// </summary>
        private const double ARROW_PATH_MARGIN_RIGHT = 8;
        
        /// <summary>
        /// Background color for combo box popup border.
        /// </summary>
        private const string POPUP_BORDER_BACKGROUND = "#FF2D2D30";
        
        /// <summary>
        /// Creates the complete status bar for the main interface.
        /// Layout: status label (left) + theme selector (center-right) + language selector (right).
        /// Status label uses data binding to display real-time status messages.
        /// </summary>
        /// <param name="mainGrid">Main grid where the status bar will be added.</param>
        /// <param name="gui">Main window instance for data binding and theme access.</param>
        public static void CreateStatusBar(Grid mainGrid, DevStackGui gui)
        {
            var localization = gui.LocalizationManager;
            var statusBar = CreateStatusBarBorder(gui);
            Grid.SetRow(statusBar, 1);

            var statusGrid = CreateStatusGrid();

            var statusLabel = CreateStatusLabel(gui);
            Grid.SetColumn(statusLabel, 0);
            statusGrid.Children.Add(statusLabel);

            var themeComboBox = CreateThemeComboBox(gui, localization);
            Grid.SetColumn(themeComboBox, 1);
            statusGrid.Children.Add(themeComboBox);

            var languageComboBox = CreateLanguageComboBox(gui, localization);
            Grid.SetColumn(languageComboBox, 2);
            statusGrid.Children.Add(languageComboBox);

            statusBar.Child = statusGrid;
            mainGrid.Children.Add(statusBar);
        }

        /// <summary>
        /// Creates the border container for the status bar with theme styling.
        /// </summary>
        /// <param name="gui">Main window instance for theme access.</param>
        /// <returns>Styled border with background and top separator line.</returns>
        private static Border CreateStatusBarBorder(DevStackGui gui)
        {
            return new Border
            {
                Background = gui.CurrentTheme.StatusBackground,
                BorderBrush = gui.CurrentTheme.Border,
                BorderThickness = new Thickness(0, STATUS_BAR_BORDER_TOP, 0, 0),
                Height = STATUS_BAR_HEIGHT,
                Padding = new Thickness(STATUS_BAR_PADDING_LEFT, 0, 0, 0)
            };
        }

        /// <summary>
        /// Creates the internal grid layout for status bar components.
        /// Three columns: status label (auto width) + theme selector (150px) + language selector (150px).
        /// </summary>
        /// <returns>Grid with three-column layout.</returns>
        private static Grid CreateStatusGrid()
        {
            var statusGrid = new Grid();
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            return statusGrid;
        }

        /// <summary>
        /// Creates the status label with data binding to StatusMessage property.
        /// Displays real-time status updates from background operations.
        /// </summary>
        /// <param name="gui">Main window instance for data binding.</param>
        /// <returns>Label with status message binding.</returns>
        private static Label CreateStatusLabel(DevStackGui gui)
        {
            var statusLabel = new Label
            {
                FontSize = STATUS_LABEL_FONT_SIZE,
                Foreground = gui.CurrentTheme.StatusForeground,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var statusBinding = new Binding("StatusMessage") { Source = gui };
            statusLabel.SetBinding(Label.ContentProperty, statusBinding);
            return statusLabel;
        }

        /// <summary>
        /// Creates the theme selector ComboBox (Dark/Light).
        /// Applies custom borderless template and synchronizes with ThemeManager.
        /// Persists selection to DevStackConfig and triggers UI-wide theme update.
        /// </summary>
        /// <param name="gui">Main window instance for theme access.</param>
        /// <param name="localization">Localization manager for translated theme names.</param>
        /// <returns>ComboBox with theme options and custom styling.</returns>
        private static ComboBox CreateThemeComboBox(DevStackGui gui, DevStackShared.LocalizationManager localization)
        {
            var themeComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            themeComboBox.Height = STATUS_BAR_HEIGHT;
            themeComboBox.MinWidth = THEME_COMBOBOX_MIN_WIDTH;
            themeComboBox.VerticalAlignment = VerticalAlignment.Center;
            themeComboBox.HorizontalAlignment = HorizontalAlignment.Right;
            themeComboBox.Margin = new Thickness(0);

            ApplyComboBoxTemplate(themeComboBox);

            PopulateThemeItems(themeComboBox, localization);

            return themeComboBox;
        }

        /// <summary>
        /// Creates the language selector ComboBox with available languages.
        /// Applies custom borderless template and synchronizes with LocalizationManager.
        /// Persists selection to DevStackConfig and triggers UI-wide language update.
        /// </summary>
        /// <param name="gui">Main window instance for theme access.</param>
        /// <param name="localization">Localization manager for available languages.</param>
        /// <returns>ComboBox with language options and custom styling.</returns>
        private static ComboBox CreateLanguageComboBox(DevStackGui gui, DevStackShared.LocalizationManager localization)
        {
            var languageComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            languageComboBox.Height = STATUS_BAR_HEIGHT;
            languageComboBox.MinWidth = LANGUAGE_COMBOBOX_MIN_WIDTH;
            languageComboBox.VerticalAlignment = VerticalAlignment.Center;
            languageComboBox.HorizontalAlignment = HorizontalAlignment.Right;
            languageComboBox.Margin = new Thickness(0, 0, 0, 0);

            ApplyComboBoxTemplate(languageComboBox);

            PopulateLanguageItems(languageComboBox, localization);

            return languageComboBox;
        }

        /// <summary>
        /// Applies custom borderless template to ComboBox.
        /// Creates XAML ControlTemplate with custom dropdown styling:
        /// - Borderless Border container
        /// - ContentPresenter for selected item
        /// - ToggleButton with dropdown arrow
        /// - Popup with scrollable items list
        /// </summary>
        /// <param name="comboBox">ComboBox to apply template to.</param>
        private static void ApplyComboBoxTemplate(ComboBox comboBox)
        {
            try
            {
                var templateXaml = @"
                <ControlTemplate TargetType='ComboBox' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Border Name='MainBorder'
                            Background='{TemplateBinding Background}'
                            BorderBrush='Transparent'
                            BorderThickness='0'
                            CornerRadius='3'>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width='*'/>
                                <ColumnDefinition Width='20'/>
                            </Grid.ColumnDefinitions>

                            <!-- Item selecionado -->
                            <ContentPresenter Name='ContentSite'
                                              Grid.Column='0'
                                              Margin='10,5,10,8'
                                              VerticalAlignment='Top'
                                              HorizontalAlignment='Left'
                                              Content='{TemplateBinding SelectionBoxItem}'
                                              ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}'
                                              ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}'
                                              IsHitTestVisible='False'/>

                            <!-- Botão de toggle (seta) -->
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

                            <!-- Popup do dropdown -->
                            <Popup Name='Popup'
                                   Placement='Bottom'
                                   IsOpen='{TemplateBinding IsDropDownOpen}'
                                   AllowsTransparency='True'
                                   Focusable='False'
                                   PopupAnimation='Slide'>
                                <Border Name='DropDownBorder'
                                        Background='#FF2D2D30'
                                        BorderBrush='Transparent'
                                        BorderThickness='0'
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
                </ControlTemplate>";

                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);

                comboBox.Template = template;
                comboBox.BorderThickness = new Thickness(0);
                comboBox.BorderBrush = Brushes.Transparent;
                comboBox.FocusVisualStyle = null;
            }
            catch { }
        }

        /// <summary>
        /// Populates the theme ComboBox with Dark and Light options.
        /// Synchronizes selection with current ThemeManager theme type.
        /// Applies theme changes via ThemeManager.ApplyTheme and persists to DevStackConfig.
        /// </summary>
        /// <param name="themeComboBox">ComboBox to populate.</param>
        /// <param name="localization">Localization manager for translated theme names.</param>
        private static void PopulateThemeItems(ComboBox themeComboBox, DevStackShared.LocalizationManager localization)
        {
            var darkItem = new ComboBoxItem { Content = localization.GetString("common.themes.dark"), Tag = DevStackShared.ThemeManager.ThemeType.Dark };
            var lightItem = new ComboBoxItem { Content = localization.GetString("common.themes.light"), Tag = DevStackShared.ThemeManager.ThemeType.Light };
            themeComboBox.Items.Add(darkItem);
            themeComboBox.Items.Add(lightItem);
            themeComboBox.SelectedItem = DevStackShared.ThemeManager.CurrentThemeType == DevStackShared.ThemeManager.ThemeType.Light ? lightItem : darkItem;
            
            themeComboBox.SelectionChanged += (s, e) =>
            {
                if (themeComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is DevStackShared.ThemeManager.ThemeType type)
                {
                    DevStackShared.ThemeManager.ApplyTheme(type);
                    DevStackConfig.PersistSetting("theme", type == DevStackShared.ThemeManager.ThemeType.Light ? "light" : "dark");
                }
            };
        }

        /// <summary>
        /// Populates the language ComboBox with available languages from LocalizationManager.
        /// Synchronizes selection with current language code.
        /// Applies language changes via LocalizationManager.ApplyLanguage and persists to DevStackConfig.
        /// </summary>
        /// <param name="languageComboBox">ComboBox to populate.</param>
        /// <param name="localization">Localization manager for available languages and names.</param>
        private static void PopulateLanguageItems(ComboBox languageComboBox, DevStackShared.LocalizationManager localization)
        {
            var languages = localization.GetAvailableLanguages();
            var languageItems = new Dictionary<string, ComboBoxItem>();
            
            foreach (var lang in languages)
            {
                var name = localization.GetLanguageName(lang);
                var item = new ComboBoxItem { Content = name, Tag = lang };
                languageComboBox.Items.Add(item);
                languageItems[lang] = item;
            }
            
            languageComboBox.SelectedItem = languageItems.ContainsKey(DevStackShared.LocalizationManager.CurrentLanguageStatic) 
                ? languageItems[DevStackShared.LocalizationManager.CurrentLanguageStatic] 
                : languageItems.Values.FirstOrDefault();
            
            languageComboBox.SelectionChanged += (s, e) =>
            {
                if (languageComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is string code)
                {
                    DevStackShared.LocalizationManager.ApplyLanguage(code);
                    DevStackConfig.PersistSetting("language", code);
                }
            };
        }
    }
}
