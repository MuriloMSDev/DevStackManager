using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace DevStackManager
{
    /// <summary>
    /// Classe responsável pela criação e gerenciamento da barra de status
    /// </summary>
    public static class GuiStatusBar
    {
        /// <summary>
        /// Cria a barra de status para a interface principal
        /// </summary>
        /// <param name="mainGrid">Grid principal onde a barra será adicionada</param>
        /// <param name="gui">Instância da interface principal para binding</param>
        public static void CreateStatusBar(Grid mainGrid, DevStackGui gui)
        {
            var localization = gui.LocalizationManager;
            var statusBar = new Border
            {
                Background = gui.CurrentTheme.StatusBackground,
                BorderBrush = gui.CurrentTheme.Border,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Height = 30,
                Padding = new Thickness(5, 0, 0, 0)
            };
            Grid.SetRow(statusBar, 1);

            var statusGrid = new Grid();
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var statusLabel = new Label
            {
                FontSize = 12,
                Foreground = gui.CurrentTheme.StatusForeground,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var statusBinding = new Binding("StatusMessage") { Source = gui };
            statusLabel.SetBinding(Label.ContentProperty, statusBinding);
            Grid.SetColumn(statusLabel, 0);
            statusGrid.Children.Add(statusLabel);

            // ComboBox para seleção de tema (Light/Dark) com estilo do ThemeManager
            var themeComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            themeComboBox.Height = 30;
            themeComboBox.MinWidth = 120;
            themeComboBox.VerticalAlignment = VerticalAlignment.Center;
            themeComboBox.HorizontalAlignment = HorizontalAlignment.Right;
            themeComboBox.Margin = new Thickness(0);

            // Language selector (substitui o botão de atualizar)
            var languageComboBox = DevStackShared.ThemeManager.CreateStyledComboBox();
            languageComboBox.Height = 30; // Mesma altura da status bar
            languageComboBox.MinWidth = 160;
            languageComboBox.VerticalAlignment = VerticalAlignment.Center;
            languageComboBox.HorizontalAlignment = HorizontalAlignment.Right;
            languageComboBox.Margin = new Thickness(0, 0, 0, 0);

            // Substituir o template para remover bordas (mantendo o Background atual via TemplateBinding)
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

                themeComboBox.Template = template;
                // Garantir que nenhuma borda de controle seja aplicada
                themeComboBox.BorderThickness = new Thickness(0);
                themeComboBox.BorderBrush = Brushes.Transparent;
                themeComboBox.FocusVisualStyle = null;

                languageComboBox.Template = template;
                // Garantir que nenhuma borda de controle seja aplicada
                languageComboBox.BorderThickness = new Thickness(0);
                languageComboBox.BorderBrush = Brushes.Transparent;
                languageComboBox.FocusVisualStyle = null;
            }
            catch { }

            // Popular temas
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

            // Popular idiomas seguindo exatamente o padrão do themeComboBox
            var languages = localization.GetAvailableLanguages();
            var languageItems = new Dictionary<string, ComboBoxItem>();
            
            foreach (var lang in languages)
            {
                var name = localization.GetLanguageName(lang);
                var item = new ComboBoxItem { Content = name, Tag = lang };
                languageComboBox.Items.Add(item);
                languageItems[lang] = item;
            }
            
            // Definir item selecionado baseado no idioma atual estático (igual ao tema)
            languageComboBox.SelectedItem = languageItems.ContainsKey(DevStackShared.LocalizationManager.CurrentLanguageStatic) 
                ? languageItems[DevStackShared.LocalizationManager.CurrentLanguageStatic] 
                : languageItems.Values.FirstOrDefault();
            
            // Alterar idioma usando ApplyLanguage (exatamente como o tema)
            languageComboBox.SelectionChanged += (s, e) =>
            {
                if (languageComboBox.SelectedItem is ComboBoxItem selected && selected.Tag is string code)
                {
                    DevStackShared.LocalizationManager.ApplyLanguage(code);
                    DevStackConfig.PersistSetting("language", code);
                }
            };

            Grid.SetColumn(themeComboBox, 1);
            statusGrid.Children.Add(themeComboBox);
            Grid.SetColumn(languageComboBox, 2);
            statusGrid.Children.Add(languageComboBox);

            statusBar.Child = statusGrid;
            mainGrid.Children.Add(statusBar);
        }
    }
}
