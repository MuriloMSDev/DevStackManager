using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DevStackShared
{
    /// <summary>
    /// Sistema avançado de temas e estilos para o DevStackManager
    /// Contém as definições de cores, estilos, animações e helpers para criação de controles temáticos
    /// Versão melhorada com suporte a múltiplos temas e animações fluidas
    /// </summary>
    public static class ThemeManager
    {
        #region UI Constants
        // Animation Durations (milliseconds)
        private const int ANIMATION_BUTTON_HOVER_MS = 200;
        private const int ANIMATION_FADE_IN_MS = 300;
        private const int ANIMATION_FADE_OUT_MS = 200;
        private const int ANIMATION_SLIDE_IN_MS = 400;
        
        // Font Sizes
        private const double FONT_SIZE_STANDARD = 14.0;
        private const double FONT_SIZE_CONSOLE = 13.0;
        private const double FONT_SIZE_HEADER = 14.0;
        private const double FONT_SIZE_ROW = 14.0;
        
        // Padding Values
        private const double PADDING_STANDARD_HORIZONTAL = 10;
        private const double PADDING_STANDARD_VERTICAL = 8;
        private const double PADDING_CONSOLE_VERTICAL = 8;
        private const double PADDING_LABEL_HORIZONTAL = 10;
        private const double PADDING_LABEL_VERTICAL_TOP = 4;
        private const double PADDING_LABEL_VERTICAL_BOTTOM = 4;
        private const double PADDING_HEADER_HORIZONTAL = 12;
        private const double PADDING_HEADER_VERTICAL = 10;
        private const double PADDING_COMBOBOX_HORIZONTAL = 10;
        private const double PADDING_COMBOBOX_VERTICAL = 8;
        private const double PADDING_COMBOBOX_ITEM_HORIZONTAL = 10;
        private const double PADDING_COMBOBOX_ITEM_VERTICAL = 6;
        private const double PADDING_CHECKBOX_HORIZONTAL = 12;
        private const double PADDING_CHECKBOX_VERTICAL = 8;
        private const double PADDING_CARD_HORIZONTAL = 16;
        private const double PADDING_CARD_VERTICAL = 12;
        
        // Margins
        private const double MARGIN_COMBOBOX_CONTENT_HORIZONTAL = 10;
        private const double MARGIN_COMBOBOX_CONTENT_VERTICAL_TOP = 5;
        private const double MARGIN_COMBOBOX_CONTENT_VERTICAL_BOTTOM = 8;
        private const double MARGIN_MESSAGEBOX_CONTENT_HORIZONTAL = 55;
        private const double MARGIN_MESSAGEBOX_CONTENT_VERTICAL_TOP = 20;
        private const double MARGIN_MESSAGEBOX_CONTENT_VERTICAL_RIGHT = 30;
        private const double MARGIN_MESSAGEBOX_CONTENT_VERTICAL_BOTTOM = 10;
        
        // Dimensions
        private const double MIN_HEIGHT_TEXTBOX = 35;
        private const double MIN_HEIGHT_COMBOBOX = 35;
        private const double MIN_HEIGHT_ROW = 35;
        private const double SCROLLBAR_WIDTH = 12;
        private const double COMBOBOX_DROPDOWN_WIDTH = 20;
        private const double MIN_BUTTON_WIDTH = 80;
        private const double MESSAGEBOX_ICON_SIZE = 500;
        
        // Opacity Values
        private const double OPACITY_DISABLED = 0.6;
        private const double OPACITY_SHADOW = 0.3;
        private const double OPACITY_SCROLLBAR_TRACK = 0.3;
        
        // Effects
        private const double GLOW_RADIUS_DEFAULT = 10;
        private const double GLOW_OPACITY_DEFAULT = 0.8;
        private const double SHADOW_OPACITY_DEFAULT = 0.3;
        
        // Color Adjustment
        private const double COLOR_DARKEN_FACTOR = 0.3;
        
        // Progress Bar
        private const double PROGRESS_BAR_DEFAULT_VALUE = 0;
        private const double PROGRESS_BAR_DEFAULT_MAXIMUM = 100;
        
        // Render Transform
        private const double RENDER_TRANSFORM_CENTER = 0.5;
        
        // RGB Color Values - Dark Theme
        private const byte DARK_DANGER_HOVER_R = 200;
        private const byte DARK_DANGER_HOVER_G = 35;
        private const byte DARK_DANGER_HOVER_B = 51;
        
        private const byte DARK_DANGER_PRESSED_R = 180;
        private const byte DARK_DANGER_PRESSED_G = 25;
        private const byte DARK_DANGER_PRESSED_B = 41;
        
        private const byte DARK_FORM_BACKGROUND_R = 22;
        private const byte DARK_FORM_BACKGROUND_G = 27;
        private const byte DARK_FORM_BACKGROUND_B = 34;
        
        private const byte DARK_CONTROL_BACKGROUND_R = 32;
        private const byte DARK_CONTROL_BACKGROUND_G = 39;
        private const byte DARK_CONTROL_BACKGROUND_B = 49;
        
        private const byte DARK_BUTTON_HOVER_R = 58;
        private const byte DARK_BUTTON_HOVER_G = 150;
        private const byte DARK_BUTTON_HOVER_B = 255;
        
        private const byte DARK_BUTTON_PRESSED_R = 25;
        private const byte DARK_BUTTON_PRESSED_G = 118;
        private const byte DARK_BUTTON_PRESSED_B = 220;
        
        private const byte DARK_ACCENT_PRESSED_R = 40;
        private const byte DARK_ACCENT_PRESSED_G = 175;
        private const byte DARK_ACCENT_PRESSED_B = 131;
        
        private const byte DARK_INFO_R = 58;
        private const byte DARK_INFO_G = 150;
        private const byte DARK_INFO_B = 255;
        
        // Alpha Values
        private const byte ALPHA_NOTIFICATION_BACKGROUND = 25;
        private const byte ALPHA_SELECTION = 128;
        private const byte ALPHA_OVERLAY_DARK = 180;
        private const byte ALPHA_OVERLAY_SEMI = 50;
        
        // Gradient Angles
        private const double GRADIENT_ANGLE_HORIZONTAL = 90;
        private const double GRADIENT_ANGLE_DIAGONAL = 45;
        #endregion
        
        #region Theme Classes
        /// <summary>
        /// Enumeração dos temas disponíveis
        /// </summary>
        public enum ThemeType
        {
            Dark,
            Light,
            HighContrast
        }

        /// <summary>
        /// Cores do tema com suporte a múltiplos esquemas
        /// </summary>
        public class ThemeColors
        {
            // Backgrounds principais
            // Cores hardcoded utilizadas em controles
            public SolidColorBrush PureWhite { get; set; } = null!;
            public SolidColorBrush PureBlack { get; set; } = null!;
            public SolidColorBrush DangerHover { get; set; } = null!;
            public SolidColorBrush DangerPressed { get; set; } = null!;
            public SolidColorBrush WarningHover { get; set; } = null!;
            public SolidColorBrush WarningPressed { get; set; } = null!;
            public SolidColorBrush SelectionBrush { get; set; } = null!;
            public SolidColorBrush OverlayWhite { get; set; } = null!;
            public SolidColorBrush ConsoleSelectionBrush { get; set; } = null!;
            public SolidColorBrush RowSelectedForeground { get; set; } = null!;
            public SolidColorBrush FormBackground { get; set; } = null!;
            public SolidColorBrush Foreground { get; set; } = null!;
            public SolidColorBrush ControlBackground { get; set; } = null!;

            // Backgrounds para notificações
            public SolidColorBrush SuccessBackground { get; set; } = null!;
            public SolidColorBrush WarningBackground { get; set; } = null!;
            public SolidColorBrush DangerBackground { get; set; } = null!;
            public SolidColorBrush InfoBackground { get; set; } = null!;

            // Botões
            public SolidColorBrush ButtonBackground { get; set; } = null!;
            public SolidColorBrush ButtonForeground { get; set; } = null!;
            public SolidColorBrush ButtonHover { get; set; } = null!;
            public SolidColorBrush ButtonPressed { get; set; } = null!;
            public SolidColorBrush ButtonDisabled { get; set; } = null!;

            // Cores de destaque com gradientes
            public SolidColorBrush Accent { get; set; } = null!;
            public SolidColorBrush AccentHover { get; set; } = null!;
            public SolidColorBrush AccentPressed { get; set; } = null!;
            public SolidColorBrush Warning { get; set; } = null!;
            public SolidColorBrush Danger { get; set; } = null!;
            public SolidColorBrush Success { get; set; } = null!;
            public SolidColorBrush Info { get; set; } = null!;

            // Grid e tabelas
            public SolidColorBrush GridBackground { get; set; } = null!;
            public SolidColorBrush GridForeground { get; set; } = null!;
            public SolidColorBrush GridHeaderBackground { get; set; } = null!;
            public SolidColorBrush GridHeaderForeground { get; set; } = null!;
            public SolidColorBrush GridAlternateRow { get; set; } = null!;
            public SolidColorBrush GridSelectedRow { get; set; } = null!;
            public SolidColorBrush GridHoverRow { get; set; } = null!;

            // Status e navegação
            public SolidColorBrush StatusBackground { get; set; } = null!;
            public SolidColorBrush StatusForeground { get; set; } = null!;
            public SolidColorBrush SidebarBackground { get; set; } = null!;
            public SolidColorBrush SidebarSelected { get; set; } = null!;
            public SolidColorBrush SidebarHover { get; set; } = null!;

            // Bordas e separadores
            public SolidColorBrush Border { get; set; } = null!;
            public SolidColorBrush BorderHover { get; set; } = null!;
            public SolidColorBrush BorderFocus { get; set; } = null!;
            public SolidColorBrush BorderActive { get; set; } = null!;

            // Áreas de conteúdo
            public SolidColorBrush ContentBackground { get; set; } = null!;
            public SolidColorBrush PanelBackground { get; set; } = null!;
            public SolidColorBrush ConsoleBackground { get; set; } = null!;
            public SolidColorBrush ConsoleForeground { get; set; } = null!;

            // Inputs com estados melhorados
            public SolidColorBrush InputBackground { get; set; } = null!;
            public SolidColorBrush InputForeground { get; set; } = null!;
            public SolidColorBrush InputBorder { get; set; } = null!;
            public SolidColorBrush InputFocusBorder { get; set; } = null!;
            public SolidColorBrush InputHoverBorder { get; set; } = null!;
            public SolidColorBrush DropdownBackground { get; set; } = null!;

            // Texto com hierarquia expandida
            public SolidColorBrush TextMuted { get; set; } = null!;
            public SolidColorBrush TextSecondary { get; set; } = null!;
            public SolidColorBrush TextDisabled { get; set; } = null!;
            public SolidColorBrush TextLink { get; set; } = null!;
            public SolidColorBrush TextLinkHover { get; set; } = null!;

            // Overlays e modais
            public SolidColorBrush OverlayBackground { get; set; } = null!;
            public SolidColorBrush TooltipBackground { get; set; } = null!;
            public SolidColorBrush TooltipForeground { get; set; } = null!;

            // Gradientes para efeitos modernos
            public LinearGradientBrush ButtonGradient { get; set; } = null!;
            public LinearGradientBrush AccentGradient { get; set; } = null!;
            public LinearGradientBrush HeaderGradient { get; set; } = null!;
            
            // Dashboard específico
            public SolidColorBrush DashboardCardBackground { get; set; } = null!;
            public SolidColorBrush DashboardCardHover { get; set; } = null!;
            public SolidColorBrush DashboardCardHoverDefault { get; set; } = null!;
            public SolidColorBrush DashboardErrorText { get; set; } = null!;
            public SolidColorBrush DashboardMutedText { get; set; } = null!;
            public SolidColorBrush DashboardAccentBlue { get; set; } = null!;
            public SolidColorBrush DashboardServiceYellow { get; set; } = null!;
            public SolidColorBrush DashboardFooterBackground { get; set; } = null!;
        }

        /// <summary>
        /// Configurações de animação para o tema
        /// </summary>
        public class ThemeAnimationSettings
        {
            public TimeSpan ButtonHoverDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_BUTTON_HOVER_MS);
            public TimeSpan FadeInDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_FADE_IN_MS);
            public TimeSpan FadeOutDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_FADE_OUT_MS);
            public TimeSpan SlideInDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_SLIDE_IN_MS);
            public IEasingFunction StandardEasing { get; set; } = new CubicEase { EasingMode = EasingMode.EaseOut };
            public IEasingFunction BounceEasing { get; set; } = new BounceEase { EasingMode = EasingMode.EaseOut };
        }
        #endregion

        #region Theme Definition
        /// <summary>
        /// Tema escuro moderno e otimizado
        /// </summary>
        public static readonly ThemeColors DarkTheme = new()
        {
            PureWhite = new SolidColorBrush(Colors.White),
            PureBlack = new SolidColorBrush(Colors.Black),
            DangerHover = new SolidColorBrush(Color.FromRgb(DARK_DANGER_HOVER_R, DARK_DANGER_HOVER_G, DARK_DANGER_HOVER_B)),
            DangerPressed = new SolidColorBrush(Color.FromRgb(DARK_DANGER_PRESSED_R, DARK_DANGER_PRESSED_G, DARK_DANGER_PRESSED_B)),
            WarningHover = new SolidColorBrush(Color.FromRgb(217, 164, 6)),
            WarningPressed = new SolidColorBrush(Color.FromRgb(195, 147, 5)),
            SelectionBrush = new SolidColorBrush(Color.FromArgb(ALPHA_SELECTION, 0, 123, 255)),
            OverlayWhite = new SolidColorBrush(Color.FromArgb(ALPHA_SELECTION, 255, 255, 255)),
            ConsoleSelectionBrush = new SolidColorBrush(Color.FromRgb(201, 209, 217)),
            RowSelectedForeground = new SolidColorBrush(Colors.White),
            FormBackground = new SolidColorBrush(Color.FromRgb(DARK_FORM_BACKGROUND_R, DARK_FORM_BACKGROUND_G, DARK_FORM_BACKGROUND_B)),
            Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235)),
            ControlBackground = new SolidColorBrush(Color.FromRgb(DARK_CONTROL_BACKGROUND_R, DARK_CONTROL_BACKGROUND_G, DARK_CONTROL_BACKGROUND_B)),

            // Notificações
            SuccessBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, 56, 211, 159)),
            WarningBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, 255, 196, 0)),
            DangerBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, 248, 81, 73)),
            InfoBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, DARK_BUTTON_HOVER_R, DARK_BUTTON_HOVER_G, DARK_BUTTON_HOVER_B)),

            // Botões com gradientes e estados melhorados
            ButtonBackground = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            ButtonForeground = new SolidColorBrush(Colors.White),
            ButtonHover = new SolidColorBrush(Color.FromRgb(DARK_BUTTON_HOVER_R, DARK_BUTTON_HOVER_G, DARK_BUTTON_HOVER_B)),
            ButtonPressed = new SolidColorBrush(Color.FromRgb(DARK_BUTTON_PRESSED_R, DARK_BUTTON_PRESSED_G, DARK_BUTTON_PRESSED_B)),
            ButtonDisabled = new SolidColorBrush(Color.FromRgb(87, 96, 106)),

            // Cores de destaque com estados expandidos
            Accent = new SolidColorBrush(Color.FromRgb(56, 211, 159)),
            AccentHover = new SolidColorBrush(Color.FromRgb(46, 194, 145)),
            AccentPressed = new SolidColorBrush(Color.FromRgb(DARK_ACCENT_PRESSED_R, DARK_ACCENT_PRESSED_G, DARK_ACCENT_PRESSED_B)),
            Warning = new SolidColorBrush(Color.FromRgb(255, 196, 0)),
            Danger = new SolidColorBrush(Color.FromRgb(248, 81, 73)),
            Success = new SolidColorBrush(Color.FromRgb(56, 211, 159)),
            Info = new SolidColorBrush(Color.FromRgb(DARK_INFO_R, DARK_INFO_G, DARK_INFO_B)),

            // Grid com excelente legibilidade e hover
            GridBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            GridForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            GridHeaderBackground = new SolidColorBrush(Color.FromRgb(45, 55, 68)),
            GridHeaderForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            GridAlternateRow = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            GridSelectedRow = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            GridHoverRow = new SolidColorBrush(Color.FromRgb(45, 55, 68)),

            // Status e navegação
            StatusBackground = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
            StatusForeground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
            SidebarBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            SidebarSelected = new SolidColorBrush(Color.FromRgb(36, 46, 59)),
            SidebarHover = new SolidColorBrush(Color.FromRgb(45, 55, 68)),

            // Bordas expandidas
            Border = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
            BorderHover = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            BorderFocus = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            BorderActive = new SolidColorBrush(Color.FromRgb(46, 194, 145)),

            // Áreas de conteúdo
            ContentBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            PanelBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            ConsoleBackground = new SolidColorBrush(Color.FromRgb(13, 17, 23)),
            ConsoleForeground = new SolidColorBrush(Color.FromRgb(201, 209, 217)),

            // Inputs com estados melhorados
            InputBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            InputForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            InputBorder = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
            InputFocusBorder = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            InputHoverBorder = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            DropdownBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),

            // Texto expandido
            TextMuted = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
            TextSecondary = new SolidColorBrush(Color.FromRgb(166, 173, 186)),
            TextDisabled = new SolidColorBrush(Color.FromRgb(87, 96, 106)),
            TextLink = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            TextLinkHover = new SolidColorBrush(Color.FromRgb(33, 136, 255)),

            // Overlays e tooltips
            OverlayBackground = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            TooltipBackground = new SolidColorBrush(Color.FromRgb(45, 55, 68)),
            TooltipForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),

            // Dashboard específico
            DashboardCardBackground = new SolidColorBrush(Color.FromRgb(55, 58, 64)),
            DashboardCardHover = new SolidColorBrush(Color.FromRgb(75, 85, 99)),
            DashboardCardHoverDefault = new SolidColorBrush(Color.FromRgb(65, 68, 74)),
            DashboardErrorText = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            DashboardMutedText = new SolidColorBrush(Color.FromRgb(169, 169, 169)),
            DashboardAccentBlue = new SolidColorBrush(Color.FromRgb(100, 149, 237)),
            DashboardServiceYellow = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            DashboardFooterBackground = new SolidColorBrush(Color.FromArgb(50, 75, 85, 99)),

            // Gradientes modernos
            ButtonGradient = new LinearGradientBrush(
                Color.FromRgb(33, 136, 255), 
                Color.FromRgb(58, 150, 255), 
                90),
            AccentGradient = new LinearGradientBrush(
                Color.FromRgb(56, 211, 159), 
                Color.FromRgb(46, 194, 145), 
                45),
            HeaderGradient = new LinearGradientBrush(
                Color.FromRgb(45, 55, 68), 
                Color.FromRgb(32, 39, 49), 
                90)
        };

        /// <summary>
        /// Tema claro moderno (para futuras implementações)
        /// </summary>
        public static readonly ThemeColors LightTheme = new()
        {
            PureWhite = new SolidColorBrush(Colors.White),
            PureBlack = new SolidColorBrush(Colors.Black),
            DangerHover = new SolidColorBrush(Color.FromRgb(255, 102, 102)),
            DangerPressed = new SolidColorBrush(Color.FromRgb(255, 51, 51)),
            WarningHover = new SolidColorBrush(Color.FromRgb(255, 223, 99)),
            WarningPressed = new SolidColorBrush(Color.FromRgb(255, 213, 49)),
            SelectionBrush = new SolidColorBrush(Color.FromArgb(128, 0, 123, 255)),
            OverlayWhite = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
            ConsoleSelectionBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
            RowSelectedForeground = new SolidColorBrush(Colors.Black),
            FormBackground = new SolidColorBrush(Color.FromRgb(245, 247, 250)),
            Foreground = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
            ControlBackground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            // Notificações
            SuccessBackground = new SolidColorBrush(Color.FromArgb(25, 40, 167, 69)),
            WarningBackground = new SolidColorBrush(Color.FromArgb(25, 255, 193, 7)),
            DangerBackground = new SolidColorBrush(Color.FromArgb(25, 220, 53, 69)),
            InfoBackground = new SolidColorBrush(Color.FromArgb(25, 23, 162, 184)),

            // Botões com gradientes e estados melhorados
            ButtonBackground = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            ButtonForeground = new SolidColorBrush(Colors.White),
            ButtonHover = new SolidColorBrush(Color.FromRgb(0, 110, 230)),
            ButtonPressed = new SolidColorBrush(Color.FromRgb(0, 100, 200)),
            ButtonDisabled = new SolidColorBrush(Color.FromRgb(173, 181, 189)),

            // Cores de destaque com estados expandidos
            Accent = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
            AccentHover = new SolidColorBrush(Color.FromRgb(33, 136, 56)),
            AccentPressed = new SolidColorBrush(Color.FromRgb(25, 105, 43)),
            Warning = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            Danger = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            Success = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
            Info = new SolidColorBrush(Color.FromRgb(23, 162, 184)),

            // Grid com excelente legibilidade e hover
            GridBackground = new SolidColorBrush(Colors.White),
            GridForeground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
            GridHeaderBackground = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
            GridHeaderForeground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
            GridAlternateRow = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            GridSelectedRow = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            GridHoverRow = new SolidColorBrush(Color.FromRgb(233, 236, 239)),

            // Status e navegação
            StatusBackground = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            StatusForeground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            SidebarBackground = new SolidColorBrush(Color.FromRgb(243, 244, 246)),
            SidebarSelected = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
            SidebarHover = new SolidColorBrush(Color.FromRgb(248, 249, 250)),

            // Bordas expandidas
            Border = new SolidColorBrush(Color.FromRgb(206, 212, 218)),
            BorderHover = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            BorderFocus = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            BorderActive = new SolidColorBrush(Color.FromRgb(40, 167, 69)),

            // Áreas de conteúdo
            ContentBackground = new SolidColorBrush(Colors.White),
            PanelBackground = new SolidColorBrush(Color.FromRgb(244, 245, 246)),
            ConsoleBackground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
            ConsoleForeground = new SolidColorBrush(Color.FromRgb(248, 249, 250)),

            // Inputs com estados melhorados
            InputBackground = new SolidColorBrush(Colors.White),
            InputForeground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
            InputBorder = new SolidColorBrush(Color.FromRgb(206, 212, 218)),
            InputFocusBorder = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            InputHoverBorder = new SolidColorBrush(Color.FromRgb(134, 142, 150)),
            DropdownBackground = new SolidColorBrush(Colors.White),

            // Texto expandido
            TextMuted = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            TextSecondary = new SolidColorBrush(Color.FromRgb(134, 142, 150)),
            TextDisabled = new SolidColorBrush(Color.FromRgb(173, 181, 189)),
            TextLink = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            TextLinkHover = new SolidColorBrush(Color.FromRgb(0, 110, 230)),

            // Overlays e tooltips
            OverlayBackground = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
            TooltipBackground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
            TooltipForeground = new SolidColorBrush(Colors.White),
                
            // Dashboard específico
            DashboardCardBackground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            DashboardCardHover = new SolidColorBrush(Color.FromRgb(228, 229, 230)),
            DashboardCardHoverDefault = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
            DashboardErrorText = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            DashboardMutedText = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            DashboardAccentBlue = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            DashboardServiceYellow = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            DashboardFooterBackground = new SolidColorBrush(Color.FromArgb(50, 108, 117, 125)),

            // Gradientes modernos
            ButtonGradient = new LinearGradientBrush(
                Color.FromRgb(0, 123, 255),
                Color.FromRgb(0, 110, 230),
                90),
            AccentGradient = new LinearGradientBrush(
                Color.FromRgb(40, 167, 69),
                Color.FromRgb(33, 136, 56),
                45),
            HeaderGradient = new LinearGradientBrush(
                Color.FromRgb(233, 236, 239),
                Color.FromRgb(245, 247, 250),
                90)
        };

        // Tema atual e configurações
        private static ThemeType _currentThemeType;
        public static ThemeType CurrentThemeType 
        { 
            get => _currentThemeType; 
            set 
            { 
                _currentThemeType = value;
                OnThemeChanged?.Invoke();
            } 
        }

        /// <summary>
        /// Aplica o tema em tempo real, dispara evento e faz fallback se necessário
        /// </summary>
        public static void ApplyTheme(ThemeType themeType)
        {
            string logMessage = $"[ApplyTheme] Applying theme: {themeType}\n";
            try
            {
                if (_currentThemeType != themeType)
                {
                    _currentThemeType = themeType;
                    logMessage += $"[ApplyTheme] Theme set to: {_currentThemeType}\n";
                    OnThemeChanged?.Invoke();
                }
                System.Diagnostics.Debug.WriteLine(logMessage);
                // Opcional: salvar log em arquivo
                // AppendToLogFile(logMessage);
            }
            catch (Exception ex)
            {
                logMessage += $"[ApplyTheme] Failed to apply theme: {ex.Message}\n";
                System.Diagnostics.Debug.WriteLine(logMessage);
                // Fallback para tema padrão
                _currentThemeType = ThemeType.Dark;
                OnThemeChanged?.Invoke();
            }
        }

        public static ThemeColors CurrentTheme => CurrentThemeType switch
        {
            ThemeType.Dark => DarkTheme,
            ThemeType.Light => LightTheme,
            ThemeType.HighContrast => DarkTheme, // Por enquanto usa o Dark
            _ => DarkTheme
        };

        public static readonly ThemeAnimationSettings AnimationSettings = new();

        /// <summary>
        /// Evento disparado quando o tema é alterado
        /// </summary>
        public static event Action? OnThemeChanged;
        #endregion

        #region Animation Helpers
        /// <summary>
        /// Cria uma animação de fade in para elementos
        /// </summary>
        public static void AnimateFadeIn(UIElement element, double from = 0, double to = 1, TimeSpan? duration = null)
        {
            var fadeIn = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration ?? AnimationSettings.FadeInDuration,
                EasingFunction = AnimationSettings.StandardEasing
            };
            
            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Cria uma animação de fade out para elementos
        /// </summary>
        public static void AnimateFadeOut(UIElement element, double from = 1, double to = 0, TimeSpan? duration = null, EventHandler? onCompleted = null)
        {
            var fadeOut = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration ?? AnimationSettings.FadeOutDuration,
                EasingFunction = AnimationSettings.StandardEasing
            };
            
            if (onCompleted != null)
                fadeOut.Completed += onCompleted;
                
            element.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        /// <summary>
        /// Cria uma animação de slide para elementos
        /// </summary>
        public static void AnimateSlideIn(FrameworkElement element, double fromX, double toX, TimeSpan? duration = null)
        {
            var transform = new TranslateTransform(fromX, 0);
            element.RenderTransform = transform;
            
            var slideIn = new DoubleAnimation
            {
                From = fromX,
                To = toX,
                Duration = duration ?? AnimationSettings.SlideInDuration,
                EasingFunction = AnimationSettings.BounceEasing
            };
            
            transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
        }

        /// <summary>
        /// Cria uma animação de hover para botões
        /// </summary>
        public static void AnimateButtonHover(Button button, bool isEntering)
        {
            var scaleTransform = button.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
            button.RenderTransform = scaleTransform;
            button.RenderTransformOrigin = new Point(RENDER_TRANSFORM_CENTER, RENDER_TRANSFORM_CENTER);
            
            var targetScale = isEntering ? 1.05 : 1.0;
            var animation = new DoubleAnimation
            {
                To = targetScale,
                Duration = AnimationSettings.ButtonHoverDuration,
                EasingFunction = AnimationSettings.StandardEasing
            };
            
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        /// <summary>
        /// Aplica um efeito de glow nos elementos
        /// </summary>
        public static void ApplyGlowEffect(UIElement element, Color color, double radius = GLOW_RADIUS_DEFAULT, double opacity = GLOW_OPACITY_DEFAULT)
        {
            var glow = new DropShadowEffect
            {
                Color = color,
                BlurRadius = radius,
                ShadowDepth = 0,
                Opacity = opacity
            };
            element.Effect = glow;
        }

        /// <summary>
        /// Remove todos os efeitos visuais de um elemento
        /// </summary>
        public static void RemoveEffects(UIElement element)
        {
            element.Effect = null;
        }

        /// <summary>
        /// Escurece uma cor SolidColorBrush por uma porcentagem específica
        /// </summary>
        public static SolidColorBrush DarkenColor(SolidColorBrush brush, double percentage)
        {
            var color = brush.Color;
            var factor = 1.0 - percentage;
            
            var newColor = Color.FromArgb(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor)
            );
            
            return new SolidColorBrush(newColor);
        }

        /// <summary>
        /// Clareia uma cor SolidColorBrush por uma porcentagem específica
        /// </summary>
        public static SolidColorBrush LightenColor(SolidColorBrush brush, double percentage)
        {
            var color = brush.Color;
            var factor = percentage;
            
            var newColor = Color.FromArgb(
                color.A,
                (byte)(color.R + (255 - color.R) * factor),
                (byte)(color.G + (255 - color.G) * factor),
                (byte)(color.B + (255 - color.B) * factor)
            );
            
            return new SolidColorBrush(newColor);
        }
        #endregion

        #region Advanced UI Helper Methods
        /// <summary>
        /// Aplica o tema escuro a um DataGrid
        /// </summary>
        public static void SetDataGridDarkTheme(DataGrid dataGrid)
        {
            // Forçar as cores do tema escuro em todas as propriedades
            dataGrid.Background = CurrentTheme.GridBackground;
            dataGrid.Foreground = CurrentTheme.GridForeground;
            dataGrid.BorderBrush = CurrentTheme.Border;
            dataGrid.BorderThickness = new Thickness(1);
            dataGrid.RowBackground = CurrentTheme.GridBackground;
            dataGrid.AlternatingRowBackground = CurrentTheme.GridAlternateRow;
            dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
            dataGrid.HorizontalGridLinesBrush = CurrentTheme.Border;
            dataGrid.VerticalGridLinesBrush = CurrentTheme.Border;
            dataGrid.CanUserResizeRows = false;
            dataGrid.SelectionMode = DataGridSelectionMode.Single;
            dataGrid.SelectionUnit = DataGridSelectionUnit.FullRow;

            // Remover os cabeçalhos de linha (botões finos à esquerda)
            dataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;

            // Remover qualquer estilo existente que possa interferir
            dataGrid.Style = null;

            // Forçar recursos para garantir que cores padrão não sejam aplicadas
            dataGrid.Resources.Clear();

            // Header styling with modern look - SEMPRE aplicar
            var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, CurrentTheme.GridHeaderBackground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, CurrentTheme.GridHeaderForeground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, FONT_SIZE_HEADER));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.PaddingProperty, new Thickness(PADDING_HEADER_HORIZONTAL, PADDING_HEADER_VERTICAL, PADDING_HEADER_HORIZONTAL, PADDING_HEADER_VERTICAL)));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderBrushProperty, CurrentTheme.Border));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            // Forçar o template para garantir que não haja elementos visuais claros
            var headerTemplate = new ControlTemplate(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, CurrentTheme.GridHeaderBackground);
            border.SetValue(Border.BorderBrushProperty, CurrentTheme.Border);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 1, 1));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(PADDING_HEADER_HORIZONTAL, PADDING_HEADER_VERTICAL, PADDING_HEADER_HORIZONTAL, PADDING_HEADER_VERTICAL));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            headerTemplate.VisualTree = border;
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.TemplateProperty, headerTemplate));

            dataGrid.ColumnHeaderStyle = headerStyle;

            // Row styling with hover and selection effects - SEMPRE aplicar
            var rowStyle = new Style(typeof(DataGridRow));
            rowStyle.Setters.Add(new Setter(DataGridRow.MinHeightProperty, MIN_HEIGHT_ROW));
            rowStyle.Setters.Add(new Setter(DataGridRow.FontSizeProperty, FONT_SIZE_ROW));
            rowStyle.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.GridBackground));
            rowStyle.Setters.Add(new Setter(DataGridRow.ForegroundProperty, CurrentTheme.GridForeground));

            // Hover trigger
            var hoverTrigger = new Trigger
            {
                Property = DataGridRow.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.SidebarHover));

            // Selection trigger
            var selectedTrigger = new Trigger
            {
                Property = DataGridRow.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.GridSelectedRow));
            selectedTrigger.Setters.Add(new Setter(DataGridRow.ForegroundProperty, CurrentTheme.RowSelectedForeground));

            rowStyle.Triggers.Add(hoverTrigger);
            rowStyle.Triggers.Add(selectedTrigger);
            dataGrid.RowStyle = rowStyle;

            // Cell styling for better padding - SEMPRE aplicar
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(8, 6, 8, 6)));
            cellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)));
            cellStyle.Setters.Add(new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            cellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent));
            cellStyle.Setters.Add(new Setter(DataGridCell.ForegroundProperty, CurrentTheme.GridForeground));

            // Triggers para células selecionadas
            var cellSelectedTrigger = new Trigger
            {
                Property = DataGridCell.IsSelectedProperty,
                Value = true
            };
            cellSelectedTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent));
            cellSelectedTrigger.Setters.Add(new Setter(DataGridCell.ForegroundProperty, CurrentTheme.RowSelectedForeground));
            cellStyle.Triggers.Add(cellSelectedTrigger);

            dataGrid.CellStyle = cellStyle;
        }

        /// <summary>
        /// Cria um botão estilizado com tema e animações avançadas
        /// </summary>
        public static Button CreateStyledButton(string content, RoutedEventHandler? clickHandler = null, ButtonStyle style = ButtonStyle.Primary)
        {
            var button = new Button
            {
                Content = content
            };

            if (clickHandler != null)
                button.Click += clickHandler;

            // Determinar cores baseado no estilo do botão
            SolidColorBrush backgroundColor, hoverColor, pressedColor, borderColor, borderHoverColor;

            switch (style)
            {
                case ButtonStyle.Success:
                    backgroundColor = CurrentTheme.Success;
                    hoverColor = CurrentTheme.AccentHover;
                    pressedColor = CurrentTheme.AccentPressed;
                    borderColor = DarkenColor(CurrentTheme.Success, COLOR_DARKEN_FACTOR);
                    borderHoverColor = DarkenColor(CurrentTheme.AccentHover, COLOR_DARKEN_FACTOR);
                    break;
                case ButtonStyle.Danger:
                    backgroundColor = CurrentTheme.Danger;
                    hoverColor = CurrentTheme.DangerHover;
                    pressedColor = CurrentTheme.DangerPressed;
                    borderColor = DarkenColor(CurrentTheme.Danger, COLOR_DARKEN_FACTOR);
                    borderHoverColor = DarkenColor(CurrentTheme.DangerHover, COLOR_DARKEN_FACTOR);
                    break;
                case ButtonStyle.Warning:
                    backgroundColor = CurrentTheme.Warning;
                    hoverColor = CurrentTheme.WarningHover;
                    pressedColor = CurrentTheme.WarningPressed;
                    borderColor = DarkenColor(CurrentTheme.Warning, COLOR_DARKEN_FACTOR);
                    borderHoverColor = DarkenColor(CurrentTheme.WarningHover, COLOR_DARKEN_FACTOR);
                    break;
                case ButtonStyle.Info:
                    backgroundColor = CurrentTheme.Info;
                    hoverColor = CurrentTheme.ButtonHover;
                    pressedColor = CurrentTheme.ButtonPressed;
                    borderColor = DarkenColor(CurrentTheme.Info, COLOR_DARKEN_FACTOR);
                    borderHoverColor = DarkenColor(CurrentTheme.ButtonHover, COLOR_DARKEN_FACTOR);
                    break;
                case ButtonStyle.Secondary:
                    backgroundColor = CurrentTheme.TextMuted;
                    hoverColor = CurrentTheme.TextSecondary;
                    pressedColor = CurrentTheme.TextDisabled;
                    borderColor = DarkenColor(CurrentTheme.TextMuted, COLOR_DARKEN_FACTOR);
                    borderHoverColor = DarkenColor(CurrentTheme.TextSecondary, COLOR_DARKEN_FACTOR);
                    break;
                default: // Primary
                    backgroundColor = CurrentTheme.ButtonBackground;
                    hoverColor = CurrentTheme.ButtonHover;
                    pressedColor = CurrentTheme.ButtonPressed;
                    borderColor = DarkenColor(CurrentTheme.ButtonBackground, COLOR_DARKEN_FACTOR);
                    borderHoverColor = DarkenColor(CurrentTheme.ButtonHover, COLOR_DARKEN_FACTOR);
                    break;
            }

            // Criar estilo com template customizado
            var buttonStyle = new Style(typeof(Button));

            // Propriedades base
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, backgroundColor));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, CurrentTheme.ButtonForeground));
            buttonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, borderColor));
            buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            buttonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(0)));
            buttonStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Medium));
            buttonStyle.Setters.Add(new Setter(Button.FontSizeProperty, FONT_SIZE_STANDARD));
            buttonStyle.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));

            // Template melhorado com gradientes
            var buttonTemplate = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "ButtonBorder";
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenterFactory.SetBinding(ContentPresenter.MarginProperty, new Binding("Padding") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

            borderFactory.AppendChild(contentPresenterFactory);
            buttonTemplate.VisualTree = borderFactory;
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));

            // Triggers com animações
            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, hoverColor));
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, borderHoverColor));
            hoverTrigger.Setters.Add(new Setter(Button.ForegroundProperty, CurrentTheme.ButtonForeground));

            var pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressedTrigger.Setters.Add(new Setter(Button.BackgroundProperty, pressedColor));

            var disabledTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disabledTrigger.Setters.Add(new Setter(Button.BackgroundProperty, CurrentTheme.ButtonDisabled));
            disabledTrigger.Setters.Add(new Setter(Button.ForegroundProperty, CurrentTheme.TextMuted));
            disabledTrigger.Setters.Add(new Setter(Button.OpacityProperty, OPACITY_DISABLED));
            disabledTrigger.Setters.Add(new Setter(Button.CursorProperty, Cursors.No));

            buttonStyle.Triggers.Add(hoverTrigger);
            buttonStyle.Triggers.Add(pressedTrigger);
            buttonStyle.Triggers.Add(disabledTrigger);

            button.Style = buttonStyle;

            // Adicionar eventos para animações
            button.MouseEnter += (s, e) => AnimateButtonHover(button, true);
            button.MouseLeave += (s, e) => AnimateButtonHover(button, false);

            // Efeito de sombra sutil
            button.Effect = new DropShadowEffect
            {
                BlurRadius = 6,
                ShadowDepth = 3,
                Opacity = SHADOW_OPACITY_DEFAULT,
                Color = CurrentTheme.PureBlack.Color
            };

            return button;
        }

        /// <summary>
        /// Enum para estilos de botão
        /// </summary>
        public enum ButtonStyle
        {
            Primary,
            Secondary,
            Success,
            Danger,
            Warning,
            Info
        }

        /// <summary>
        /// Cria um TextBox estilizado com tema escuro
        /// </summary>
        public static TextBox CreateStyledTextBox(bool isConsole = false)
        {
            var textBox = new TextBox
            {
                BorderThickness = new Thickness(1),
                Padding = isConsole ? new Thickness(PADDING_STANDARD_HORIZONTAL, PADDING_CONSOLE_VERTICAL, PADDING_STANDARD_HORIZONTAL, PADDING_CONSOLE_VERTICAL) : new Thickness(PADDING_LABEL_HORIZONTAL, PADDING_LABEL_VERTICAL_TOP, PADDING_LABEL_HORIZONTAL, PADDING_LABEL_VERTICAL_BOTTOM),
                FontSize = isConsole ? FONT_SIZE_CONSOLE : FONT_SIZE_STANDARD
            };

            if (isConsole)
            {
                textBox.Background = CurrentTheme.ConsoleBackground;
                textBox.Foreground = CurrentTheme.ConsoleForeground;
                textBox.BorderBrush = CurrentTheme.Border;
                textBox.FontFamily = new FontFamily("Consolas");
                textBox.SelectionBrush = CurrentTheme.SelectionBrush;
            }
            else
            {
                textBox.Background = CurrentTheme.InputBackground;
                textBox.Foreground = CurrentTheme.InputForeground;
                textBox.BorderBrush = CurrentTheme.InputBorder;
                textBox.SelectionBrush = CurrentTheme.SelectionBrush;
            }

            // Create style for focus effects and modern scrollbar
            var textBoxStyle = new Style(typeof(TextBox));

            // Template XAML para TextBox com scrollbar customizada
            var templateXaml = @"
                <ControlTemplate TargetType='TextBox' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Border Name='Border'
                            Background='{TemplateBinding Background}'
                            BorderBrush='{TemplateBinding BorderBrush}'
                            BorderThickness='{TemplateBinding BorderThickness}'
                            CornerRadius='3'>
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
                    
                    <ControlTemplate.Triggers>
                        <Trigger Property='IsFocused' Value='True'>
                            <Setter TargetName='Border' Property='BorderBrush' Value='#FF2188FF'/>
                            <Setter TargetName='Border' Property='BorderThickness' Value='2'/>
                        </Trigger>
                        <Trigger Property='IsMouseOver' Value='True'>
                            <Setter TargetName='Border' Property='BorderBrush' Value='#FF2188FF'/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>";

            try
            {
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);
                textBoxStyle.Setters.Add(new Setter(TextBox.TemplateProperty, template));
            }
            catch
            {
                // Fallback - usar triggers básicos se XAML falhar
                var focusTrigger = new Trigger
                {
                    Property = TextBox.IsFocusedProperty,
                    Value = true
                };
                focusTrigger.Setters.Add(new Setter(TextBox.BorderBrushProperty, CurrentTheme.InputFocusBorder));
                focusTrigger.Setters.Add(new Setter(TextBox.BorderThicknessProperty, new Thickness(2)));

                var hoverTrigger = new Trigger
                {
                    Property = TextBox.IsMouseOverProperty,
                    Value = true
                };
                hoverTrigger.Setters.Add(new Setter(TextBox.BorderBrushProperty, CurrentTheme.BorderHover));

                textBoxStyle.Triggers.Add(focusTrigger);
                textBoxStyle.Triggers.Add(hoverTrigger);
            }

            textBox.Style = textBoxStyle;

            return textBox;
        }

        /// <summary>
        /// Cria um ComboBox estilizado com tema escuro
        /// </summary>
        public static ComboBox CreateStyledComboBox()
        {
            var comboBox = new ComboBox
            {
                Background = CurrentTheme.InputBackground,
                Foreground = CurrentTheme.InputForeground,
                BorderBrush = CurrentTheme.InputBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(PADDING_COMBOBOX_HORIZONTAL, PADDING_COMBOBOX_VERTICAL, PADDING_COMBOBOX_HORIZONTAL, PADDING_COMBOBOX_VERTICAL),
                FontSize = FONT_SIZE_STANDARD,
                MinHeight = MIN_HEIGHT_COMBOBOX
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
            comboStyle.Setters.Add(new Setter(ComboBox.BackgroundProperty, CurrentTheme.InputBackground));
            comboStyle.Setters.Add(new Setter(ComboBox.ForegroundProperty, CurrentTheme.InputForeground));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputBorder));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(1)));
            comboStyle.Setters.Add(new Setter(ComboBox.PaddingProperty, new Thickness(PADDING_COMBOBOX_HORIZONTAL, PADDING_COMBOBOX_VERTICAL, PADDING_COMBOBOX_HORIZONTAL, PADDING_COMBOBOX_VERTICAL)));
            comboStyle.Setters.Add(new Setter(ComboBox.FontSizeProperty, FONT_SIZE_STANDARD));
            comboStyle.Setters.Add(new Setter(ComboBox.MinHeightProperty, MIN_HEIGHT_COMBOBOX));

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
                hoverTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.BorderHover));
                comboStyle.Triggers.Add(hoverTrigger);

                var focusTrigger = new Trigger { Property = ComboBox.IsFocusedProperty, Value = true };
                focusTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputFocusBorder));
                focusTrigger.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(2)));
                comboStyle.Triggers.Add(focusTrigger);

                var dropdownTrigger = new Trigger { Property = ComboBox.IsDropDownOpenProperty, Value = true };
                dropdownTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputFocusBorder));
                dropdownTrigger.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(2)));
                comboStyle.Triggers.Add(dropdownTrigger);
            }

            comboBox.Style = comboStyle;

            // Style the dropdown items for dark theme
            var itemStyle = new Style(typeof(ComboBoxItem));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.DropdownBackground));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, CurrentTheme.InputForeground));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.PaddingProperty, new Thickness(PADDING_COMBOBOX_ITEM_HORIZONTAL, PADDING_COMBOBOX_ITEM_VERTICAL, PADDING_COMBOBOX_ITEM_HORIZONTAL, PADDING_COMBOBOX_ITEM_VERTICAL)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BorderThicknessProperty, new Thickness(0)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.FontSizeProperty, FONT_SIZE_STANDARD));

            // Hover trigger for items
            var itemHoverTrigger = new Trigger
            {
                Property = ComboBoxItem.IsMouseOverProperty,
                Value = true
            };
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.SidebarHover));
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, CurrentTheme.InputForeground));

            // Selected trigger for items
            var itemSelectedTrigger = new Trigger
            {
                Property = ComboBoxItem.IsSelectedProperty,
                Value = true
            };
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.ButtonBackground));
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));

            // Highlighted trigger for items
            var itemHighlightedTrigger = new Trigger
            {
                Property = ComboBoxItem.IsHighlightedProperty,
                Value = true
            };
            itemHighlightedTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.ButtonBackground));
            itemHighlightedTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));

            itemStyle.Triggers.Add(itemHoverTrigger);
            itemStyle.Triggers.Add(itemSelectedTrigger);
            itemStyle.Triggers.Add(itemHighlightedTrigger);

            comboBox.ItemContainerStyle = itemStyle;

            // Override system colors to force dark theme
            comboBox.Resources.Clear();
            comboBox.Resources.Add(SystemColors.WindowBrushKey, CurrentTheme.DropdownBackground);
            comboBox.Resources.Add(SystemColors.ControlBrushKey, CurrentTheme.InputBackground);
            comboBox.Resources.Add(SystemColors.ControlTextBrushKey, CurrentTheme.InputForeground);
            comboBox.Resources.Add(SystemColors.HighlightBrushKey, CurrentTheme.ButtonBackground);
            comboBox.Resources.Add(SystemColors.HighlightTextBrushKey, new SolidColorBrush(Colors.White));

            return comboBox;
        }

        /// <summary>
        /// Cria um tooltip estilizado com tema escuro
        /// </summary>
        public static ToolTip CreateStyledToolTip(string content)
        {
            var tooltip = new ToolTip
            {
                Content = content,
                Background = CurrentTheme.TooltipBackground,
                Foreground = CurrentTheme.TooltipForeground,
                BorderBrush = CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(PADDING_CHECKBOX_HORIZONTAL, PADDING_CHECKBOX_VERTICAL, PADDING_CHECKBOX_HORIZONTAL, PADDING_CHECKBOX_VERTICAL),
                FontSize = FONT_SIZE_CONSOLE,
                HasDropShadow = true
            };

            // Estilo customizado para bordas arredondadas
            var style = new Style(typeof(ToolTip));
            var template = new ControlTemplate(typeof(ToolTip));
            
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetBinding(ContentPresenter.MarginProperty, new Binding("Padding") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            
            border.AppendChild(contentPresenter);
            template.VisualTree = border;
            style.Setters.Add(new Setter(ToolTip.TemplateProperty, template));
            
            tooltip.Style = style;
            return tooltip;
        }

        /// <summary>
        /// Adiciona um tooltip a qualquer elemento
        /// </summary>
        public static void AddToolTip(FrameworkElement element, string content)
        {
            element.ToolTip = CreateStyledToolTip(content);
        }

    /// <summary>
    /// Cria uma barra de progresso estilizada
    /// </summary>
    /// <param name="value">Valor inicial.</param>
    /// <param name="maximum">Valor máximo.</param>
    /// <param name="isIndeterminate">Se true, o progresso é indeterminado.</param>
    /// <param name="animateValueChanges">Se true (default), mudanças no Value serão animadas progressivamente em 1.2s.</param>
    public static ProgressBar CreateStyledProgressBar(double value = PROGRESS_BAR_DEFAULT_VALUE, double maximum = PROGRESS_BAR_DEFAULT_MAXIMUM, bool isIndeterminate = false, bool animateValueChanges = true)
        {
            var progressBar = new ProgressBar
            {
                Value = value,
                Maximum = maximum,
                IsIndeterminate = isIndeterminate,
                Height = 6,
                Background = CurrentTheme.PanelBackground,
                Foreground = CurrentTheme.Accent,
                BorderThickness = new Thickness(0)
            };

            // Template customizado usando XAML simplificado para garantir comportamento da esquerda para direita
            var style = new Style(typeof(ProgressBar));
            
            var templateXaml = @"
                <ControlTemplate TargetType='ProgressBar' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Border Name='PART_Track'
                            Background='{TemplateBinding Background}'
                            BorderBrush='{TemplateBinding BorderBrush}'
                            BorderThickness='{TemplateBinding BorderThickness}'
                            CornerRadius='3'>
                        <Grid ClipToBounds='True'>
                            <Rectangle Name='PART_Indicator'
                                       Fill='{TemplateBinding Foreground}'
                                       HorizontalAlignment='Left'
                                       RadiusX='3'
                                       RadiusY='3'/>
                        </Grid>
                    </Border>
                </ControlTemplate>";

            try
            {
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);
                style.Setters.Add(new Setter(ProgressBar.TemplateProperty, template));
            }
            catch
            {
                // Fallback: usar template WPF padrão com modificações mínimas
                var template = new ControlTemplate(typeof(ProgressBar));
                
                var border = new FrameworkElementFactory(typeof(Border));
                border.Name = "PART_Track";
                border.SetValue(Border.BackgroundProperty, CurrentTheme.PanelBackground);
                border.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
                border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                border.SetValue(Border.BorderBrushProperty, CurrentTheme.Border);
                
                var grid = new FrameworkElementFactory(typeof(Grid));
                grid.SetValue(Panel.ClipToBoundsProperty, true);
                
                var indicator = new FrameworkElementFactory(typeof(Rectangle));
                indicator.Name = "PART_Indicator";
                indicator.SetValue(Rectangle.FillProperty, CurrentTheme.AccentGradient);
                indicator.SetValue(Rectangle.RadiusXProperty, 3.0);
                indicator.SetValue(Rectangle.RadiusYProperty, 3.0);
                indicator.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                
                grid.AppendChild(indicator);
                border.AppendChild(grid);
                template.VisualTree = border;
                style.Setters.Add(new Setter(ProgressBar.TemplateProperty, template));
            }
            
            progressBar.Style = style;

            // Animação visual do indicador (evita recursão de ValueChanged)
            // Sempre prepara handlers; respeita 'animateValueChanges' ao decidir animar ou setar direto
            // Captura mudanças de valor antes do Loaded para animar a primeira alteração
            double? queuedValueBeforeLoaded = null;
            double initialValue = progressBar.Value;
            bool hasAnimatedInitialValue = false;
            
            progressBar.ValueChanged += (s0, e0) =>
            {
                if (!progressBar.IsLoaded)
                {
                    queuedValueBeforeLoaded = e0.NewValue;
                }
            };

            progressBar.Loaded += (s, e) =>
            {
                // Garante que o template esteja aplicado e peças nomeadas disponíveis
                progressBar.ApplyTemplate();
                var track = progressBar.Template.FindName("PART_Track", progressBar) as Border;
                var indicator = progressBar.Template.FindName("PART_Indicator", progressBar) as Rectangle;

                if (track == null || indicator == null)
                    return;

                // Mantém uma animação pendente caso o track ainda não tenha tamanho definido
                double? pendingValueForAnimation = null;

                void UpdateIndicator(double v, bool animate)
                {
                    if (progressBar.Maximum <= 0)
                        return;

                    if (track.ActualWidth <= 0)
                    {
                        // Deferir até medição
                        if (animate)
                            pendingValueForAnimation = v;
                        return;
                    }

                    var clamped = Math.Max(0, Math.Min(v, progressBar.Maximum));
                    double targetWidth = track.ActualWidth * (clamped / progressBar.Maximum);

                    if (double.IsNaN(indicator.Width))
                        indicator.Width = 0;

                    if (!animate)
                    {
                        indicator.BeginAnimation(FrameworkElement.WidthProperty, null);
                        indicator.Width = targetWidth;
                        return;
                    }

                    // Para animação, garantir que comece do valor atual
                    var currentWidth = double.IsNaN(indicator.Width) ? 0 : indicator.Width;
                    var widthAnim = new DoubleAnimation
                    {
                        From = currentWidth,
                        To = targetWidth,
                        Duration = TimeSpan.FromSeconds(1.2),
                        EasingFunction = AnimationSettings.StandardEasing
                    };
                    indicator.BeginAnimation(FrameworkElement.WidthProperty, widthAnim, HandoffBehavior.SnapshotAndReplace);
                }

                // Ajusta ao iniciar: se houve mudança antes do Loaded, anima essa primeira mudança
                if (queuedValueBeforeLoaded.HasValue && animateValueChanges)
                {
                    UpdateIndicator(queuedValueBeforeLoaded.Value, true);
                    queuedValueBeforeLoaded = null;
                    hasAnimatedInitialValue = true;
                }
                else
                {
                    // Define o valor inicial sem animação
                    UpdateIndicator(progressBar.Value, false);
                }

                // Garante animação da primeira mudança quando track só obtém tamanho após o Loaded
                progressBar.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (track.ActualWidth > 0)
                    {
                        if (pendingValueForAnimation.HasValue && animateValueChanges)
                        {
                            UpdateIndicator(pendingValueForAnimation.Value, true);
                            pendingValueForAnimation = null;
                        }
                    }
                }), DispatcherPriority.Render);

                // Recalcula em mudanças de tamanho do track
                track.SizeChanged += (o, args) =>
                {
                    if (pendingValueForAnimation.HasValue && animateValueChanges)
                    {
                        UpdateIndicator(pendingValueForAnimation.Value, true);
                        pendingValueForAnimation = null;
                    }
                    else
                    {
                        UpdateIndicator(progressBar.Value, false);
                    }
                };

                // Caso o container do ProgressBar dimensione após o Loaded
                progressBar.SizeChanged += (o, args) =>
                {
                    if (track.ActualWidth > 0)
                    {
                        if (pendingValueForAnimation.HasValue && animateValueChanges)
                        {
                            UpdateIndicator(pendingValueForAnimation.Value, true);
                            pendingValueForAnimation = null;
                        }
                        else
                        {
                            UpdateIndicator(progressBar.Value, false);
                        }
                    }
                };

                // Anima a cada alteração de Value (inclusive diminuições)
                progressBar.ValueChanged += (o, args) =>
                {
                    if (progressBar.IsIndeterminate)
                        return;
                    
                    // Se ainda não animou o valor inicial e esta é a primeira mudança significativa, anima
                    bool shouldAnimate = animateValueChanges;
                    if (!hasAnimatedInitialValue && Math.Abs(args.NewValue - initialValue) > 0.001)
                    {
                        hasAnimatedInitialValue = true;
                        shouldAnimate = animateValueChanges; // Força animação na primeira mudança
                    }
                    
                    UpdateIndicator(args.NewValue, shouldAnimate);
                };
            };
            return progressBar;
        }

        /// <summary>
        /// Cria um separador visual estilizado
        /// </summary>
        public static Separator CreateStyledSeparator(Orientation orientation = Orientation.Horizontal)
        {
            var separator = new Separator
            {
                Background = CurrentTheme.Border,
                Opacity = OPACITY_DISABLED
            };

            if (orientation == Orientation.Horizontal)
            {
                separator.Height = 1;
                separator.Margin = new Thickness(0, 8, 0, 8);
            }
            else
            {
                separator.Width = 1;
                separator.Margin = new Thickness(8, 0, 8, 0);
            }

            return separator;
        }

        /// <summary>
        /// Cria um painel de notificação moderno
        /// </summary>
        public static Border CreateNotificationPanel(string message, NotificationType type = NotificationType.Info, bool showIcon = true)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(PADDING_CARD_HORIZONTAL, PADDING_CARD_VERTICAL, PADDING_CARD_HORIZONTAL, PADDING_CARD_VERTICAL),
                Margin = new Thickness(0, 4, 0, 4)
            };

            // Definir cores baseado no tipo usando propriedades do tema
            switch (type)
            {
                case NotificationType.Success:
                    border.Background = CurrentTheme.SuccessBackground;
                    border.BorderBrush = CurrentTheme.Success;
                    break;
                case NotificationType.Warning:
                    border.Background = CurrentTheme.WarningBackground;
                    border.BorderBrush = CurrentTheme.Warning;
                    break;
                case NotificationType.Error:
                    border.Background = CurrentTheme.DangerBackground;
                    border.BorderBrush = CurrentTheme.Danger;
                    break;
                default: // Info
                    border.Background = CurrentTheme.InfoBackground;
                    border.BorderBrush = CurrentTheme.Info;
                    break;
            }

            border.BorderThickness = new Thickness(1, 1, 1, 1);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Para o ícone
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Para o texto

            // Adicionar ícone se solicitado
            if (showIcon)
            {
                var icon = new TextBlock
                {
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 12, 0)
                };

                switch (type)
                {
                    case NotificationType.Success:
                        icon.Text = "✅";
                        icon.Foreground = CurrentTheme.Success;
                        break;
                    case NotificationType.Warning:
                        icon.Text = "⚠️";
                        icon.Foreground = CurrentTheme.Warning;
                        break;
                    case NotificationType.Error:
                        icon.Text = "❌";
                        icon.Foreground = CurrentTheme.Danger;
                        break;
                    default:
                        icon.Text = "ℹ️";
                        icon.Foreground = CurrentTheme.Info;
                        break;
                }

                Grid.SetColumn(icon, 0);
                grid.Children.Add(icon);
            }

            // Adicionar texto da mensagem
            var messageText = new TextBlock
            {
                Text = message,
                Foreground = CurrentTheme.Foreground,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            Grid.SetColumn(messageText, showIcon ? 1 : 0);
            if (!showIcon)
            {
                Grid.SetColumnSpan(messageText, 2);
            }
            grid.Children.Add(messageText);
            border.Child = grid;

            // Animação de entrada
            AnimateFadeIn(border);

            return border;
        }

        /// <summary>
        /// Enum para tipos de notificação
        /// </summary>
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error
        }

        /// <summary>
        /// Cria um toggle switch estilizado
        /// </summary>
        public static CheckBox CreateStyledToggleSwitch(string content, bool isChecked = false)
        {
            var toggle = new CheckBox
            {
                Content = content,
                IsChecked = isChecked,
                Foreground = CurrentTheme.Foreground,
                FontSize = 14,
                Margin = new Thickness(0, 4, 0, 4)
            };

            // Template customizado para aparência de switch
            var style = new Style(typeof(CheckBox));
            var template = new ControlTemplate(typeof(CheckBox));

            var grid = new FrameworkElementFactory(typeof(Grid));
            
            // Adicionar definições de coluna
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

            // Switch track
            var track = new FrameworkElementFactory(typeof(Border));
            track.Name = "SwitchTrack";
            track.SetValue(Border.WidthProperty, 44.0);
            track.SetValue(Border.HeightProperty, 24.0);
            track.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
            track.SetValue(Border.BackgroundProperty, CurrentTheme.PanelBackground);
            track.SetValue(Border.BorderBrushProperty, CurrentTheme.Border);
            track.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            track.SetValue(Grid.ColumnProperty, 0);
            track.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 8, 0));

            // Switch thumb
            var thumb = new FrameworkElementFactory(typeof(Ellipse));
            thumb.Name = "SwitchThumb";
            thumb.SetValue(Ellipse.WidthProperty, 18.0);
            thumb.SetValue(Ellipse.HeightProperty, 18.0);
            thumb.SetValue(Ellipse.FillProperty, CurrentTheme.ButtonForeground);
            thumb.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            thumb.SetValue(FrameworkElement.MarginProperty, new Thickness(3, 0, 0, 0));

            // Transform para animação
            var translateTransform = new FrameworkElementFactory(typeof(TranslateTransform));
            thumb.SetValue(UIElement.RenderTransformProperty, translateTransform);

            track.AppendChild(thumb);

            // Content
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(Grid.ColumnProperty, 1);
            contentPresenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            grid.AppendChild(track);
            grid.AppendChild(contentPresenter);
            template.VisualTree = grid;

            // Triggers para animação
            var checkedTrigger = new Trigger { Property = CheckBox.IsCheckedProperty, Value = true };
            checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.Accent) { TargetName = "SwitchTrack" });
            checkedTrigger.Setters.Add(new Setter(TranslateTransform.XProperty, 20.0) { TargetName = "SwitchThumb" });

            var hoverTrigger = new Trigger { Property = CheckBox.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover) { TargetName = "SwitchTrack" });

            template.Triggers.Add(checkedTrigger);
            template.Triggers.Add(hoverTrigger);

            style.Setters.Add(new Setter(CheckBox.TemplateProperty, template));
            toggle.Style = style;

            return toggle;
        }

        /// <summary>
        /// Cria um Label estilizado com tema moderno
        /// </summary>
        public static Label CreateStyledLabel(string content, bool isTitle = false, bool isMuted = false, LabelStyle style = LabelStyle.Normal)
        {
            var label = new Label
            {
                Content = content
            };

            // Aplicar estilo baseado no parâmetro
            switch (style)
            {
                case LabelStyle.Title:
                    label.FontWeight = FontWeights.Bold;
                    label.FontSize = 18;
                    label.Foreground = CurrentTheme.Foreground;
                    break;
                case LabelStyle.Subtitle:
                    label.FontWeight = FontWeights.SemiBold;
                    label.FontSize = 16;
                    label.Foreground = CurrentTheme.Foreground;
                    break;
                case LabelStyle.Muted:
                    label.FontWeight = FontWeights.Normal;
                    label.FontSize = 14;
                    label.Foreground = CurrentTheme.TextMuted;
                    break;
                case LabelStyle.Secondary:
                    label.FontWeight = FontWeights.Normal;
                    label.FontSize = 14;
                    label.Foreground = CurrentTheme.TextSecondary;
                    break;
                case LabelStyle.Link:
                    label.FontWeight = FontWeights.Normal;
                    label.FontSize = 14;
                    label.Foreground = CurrentTheme.TextLink;
                    label.Cursor = Cursors.Hand;
                    
                    // Adicionar efeito hover para links
                    label.MouseEnter += (s, e) => {
                        label.Foreground = CurrentTheme.TextLinkHover;
                    };
                    label.MouseLeave += (s, e) => {
                        label.Foreground = CurrentTheme.TextLink;
                    };
                    break;
                default: // Normal
                    label.FontWeight = FontWeights.Normal;
                    label.FontSize = 14;
                    label.Foreground = CurrentTheme.Foreground;
                    break;
            }

            // Manter retrocompatibilidade com parâmetros antigos
            if (isTitle)
            {
                label.FontWeight = FontWeights.Bold;
                label.FontSize = 16;
                label.Foreground = CurrentTheme.Foreground;
            }
            else if (isMuted)
            {
                label.Foreground = CurrentTheme.TextMuted;
            }

            return label;
        }

        /// <summary>
        /// Enum para estilos de label
        /// </summary>
        public enum LabelStyle
        {
            Normal,
            Title,
            Subtitle,
            Muted,
            Secondary,
            Link
        }

        /// <summary>
        /// Exibe uma MessageBox estilizada com tema escuro
        /// </summary>
        public static MessageBoxResult CreateStyledMessageBox(string message, string? title = null, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            // Use localized default title if none provided
            title ??= LocalizationManager.Instance?.GetString("common.dialogs.default_title") ?? "Mensagem";
            
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
                Foreground = CurrentTheme.Foreground,
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current?.MainWindow
            };

            var border = new Border
            {
                Background = CurrentTheme.FormBackground,
                CornerRadius = new CornerRadius(12),
                BorderBrush = CurrentTheme.Border,
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
                Foreground = CurrentTheme.Foreground,
                Background = Brushes.Transparent,
                FontSize = 16,
                Margin = new Thickness(55, 20, 30, 10),
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

            void AddButton(string text, MessageBoxResult r, bool isDefault = false, ButtonStyle style = ButtonStyle.Primary)
            {
                var btn = CreateStyledButton(text, (s, e) => CloseAndSetResult(r), style);
                btn.MinWidth = 80;
                btn.Margin = new Thickness(8, 0, 0, 0);
                btn.Padding = new Thickness(10, 5, 10, 5);
                btn.FontWeight = isDefault ? FontWeights.Bold : FontWeights.Normal;
                buttonPanel.Children.Add(btn);
            }

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.ok") ?? "OK", MessageBoxResult.OK, true);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.ok") ?? "OK", MessageBoxResult.OK, true);
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.cancel") ?? "Cancelar", MessageBoxResult.Cancel, false, ButtonStyle.Warning);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.yes") ?? "Sim", MessageBoxResult.Yes, true);
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.no") ?? "Não", MessageBoxResult.No, false, ButtonStyle.Danger);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.yes") ?? "Sim", MessageBoxResult.Yes, true);
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.no") ?? "Não", MessageBoxResult.No, false, ButtonStyle.Danger);
                    AddButton(LocalizationManager.Instance?.GetString("common.buttons.cancel") ?? "Cancelar", MessageBoxResult.Cancel, false, ButtonStyle.Warning);
                    break;
            }

            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            // Ícone
            if (icon != MessageBoxImage.None)
            {
                var iconText = new TextBlock
                {
                    Margin = new Thickness(10, 10, 0, 0),
                    FontSize = 32,
                    VerticalAlignment = VerticalAlignment.Top
                };
                switch (icon)
                {
                    case MessageBoxImage.Information:
                        iconText.Text = "❕";
                        iconText.Foreground = CurrentTheme.Accent;
                        break;
                    case MessageBoxImage.Warning:
                        iconText.Text = "⚠️";
                        iconText.Foreground = CurrentTheme.Warning;
                        break;
                    case MessageBoxImage.Error:
                        iconText.Text = "⛔";
                        iconText.Foreground = CurrentTheme.Danger;
                        break;
                    case MessageBoxImage.Question:
                        iconText.Text = "❔";
                        iconText.Foreground = CurrentTheme.Accent;
                        break;
                }
                grid.Children.Add(iconText);
            }

            border.Child = grid;
            dialog.Content = border;
            dialog.ShowDialog();
            return result;
        }

        
        /// <summary>
        /// Cria um CheckBox estilizado com tema escuro
        /// </summary>
        public static CheckBox CreateStyledCheckBox(string content, bool isBold = false)
        {
            var checkBox = new CheckBox
            {
                Content = content,
                Margin = new Thickness(0, 0, 0, 10),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Foreground = CurrentTheme.InputForeground,
                Background = CurrentTheme.InputBackground,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
            };

            // ControlTemplate para customizar o visual do CheckBox
            var template = new ControlTemplate(typeof(CheckBox));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "Border";

            // StackPanel para alinhar box e conteúdo
            var panel = new FrameworkElementFactory(typeof(StackPanel));
            panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            panel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Box do check
            var boxBorder = new FrameworkElementFactory(typeof(Border));
            boxBorder.Name = "CheckBoxBox";
            boxBorder.SetValue(Border.WidthProperty, 18.0);
            boxBorder.SetValue(Border.HeightProperty, 18.0);
            boxBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            boxBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            boxBorder.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(CheckBox.BorderBrushProperty));
            boxBorder.SetValue(Border.BackgroundProperty, Brushes.Transparent); // Sem background
            boxBorder.SetValue(Border.MarginProperty, new Thickness(0, 0, 8, 0));
            boxBorder.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            boxBorder.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Path do check (visível só quando IsChecked)
            var checkPath = new FrameworkElementFactory(typeof(Path));
            checkPath.Name = "CheckMark";
            checkPath.SetValue(Path.DataProperty, Geometry.Parse("M 3 7 L 6 12 L 13 5"));
            checkPath.SetValue(Path.StrokeThicknessProperty, 2.0);
            checkPath.SetValue(Path.StrokeProperty, Brushes.White); // Branco para o check
            checkPath.SetValue(Path.SnapsToDevicePixelsProperty, true);
            checkPath.SetValue(Path.StrokeEndLineCapProperty, PenLineCap.Round);
            checkPath.SetValue(Path.StrokeStartLineCapProperty, PenLineCap.Round);
            checkPath.SetValue(FrameworkElement.VisibilityProperty, Visibility.Collapsed);

            // ContentPresenter para o texto
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);

            // Monta a árvore
            boxBorder.AppendChild(checkPath);
            panel.AppendChild(boxBorder);
            panel.AppendChild(contentPresenter);
            border.AppendChild(panel);
            template.VisualTree = border;

            // Triggers para hover, foco e check
            // Hover: muda BorderBrush para BorderHover e Foreground para branco
            var hoverTrigger = new Trigger { Property = CheckBox.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover, "CheckBoxBox"));
            hoverTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));

            // Foco: muda BorderBrush para InputFocusBorder
            var focusTrigger = new Trigger { Property = CheckBox.IsFocusedProperty, Value = true };
            focusTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.InputFocusBorder, "CheckBoxBox"));

            // Checked: mostra o Path e deixa o box com cor de fundo
            var checkedTrigger = new Trigger { Property = CheckBox.IsCheckedProperty, Value = true };
            checkedTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "CheckMark"));
            checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.ButtonBackground, "CheckBoxBox"));
            checkedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.ButtonBackground, "CheckBoxBox"));

            // Disabled: reduz opacidade
            var disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
            disabledTrigger.Setters.Add(new Setter(UIElement.OpacityProperty, 0.6));

            template.Triggers.Add(hoverTrigger);
            template.Triggers.Add(focusTrigger);
            template.Triggers.Add(checkedTrigger);
            template.Triggers.Add(disabledTrigger);

            // Estilo base
            var style = new Style(typeof(CheckBox));
            style.Setters.Add(new Setter(CheckBox.ForegroundProperty, CurrentTheme.InputForeground));
            style.Setters.Add(new Setter(CheckBox.BackgroundProperty, CurrentTheme.InputBackground));
            style.Setters.Add(new Setter(CheckBox.BorderBrushProperty, CurrentTheme.InputBorder));
            style.Setters.Add(new Setter(CheckBox.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(CheckBox.PaddingProperty, new Thickness(6, 2, 6, 2)));
            style.Setters.Add(new Setter(CheckBox.FontSizeProperty, 14.0));
            style.Setters.Add(new Setter(CheckBox.TemplateProperty, template));

            checkBox.Style = style;
            return checkBox;
        }

        /// <summary>
        /// Aplica o tema escuro personalizado a um ListBox usado na barra lateral
        /// </summary>
        public static void ApplySidebarListBoxTheme(ListBox listBox)
        {
            listBox.Background = CurrentTheme.SidebarBackground;
            listBox.Foreground = CurrentTheme.Foreground;
            listBox.BorderBrush = CurrentTheme.Border;
            listBox.BorderThickness = new Thickness(0);
            ScrollViewer.SetCanContentScroll(listBox, false);

            // Create custom template para garantir controle total sobre o visual
            var itemTemplate = new ControlTemplate(typeof(ListBoxItem));

            // Container principal (Border para background e borda direita)
            var mainBorder = new FrameworkElementFactory(typeof(Border));
            mainBorder.Name = "MainBorder";
            mainBorder.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            mainBorder.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 4, 0));
            mainBorder.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            mainBorder.SetValue(Border.PaddingProperty, new Thickness(15, 12, 15, 12));
            mainBorder.SetValue(Border.MarginProperty, new Thickness(4, 2, 4, 2));

            // ContentPresenter para o conteúdo do item
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            mainBorder.AppendChild(contentPresenter);
            itemTemplate.VisualTree = mainBorder;

            // Triggers no template para garantir prioridade máxima

            // 1. Hover trigger (baixa prioridade)
            var hoverTrigger = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarHover) { TargetName = "MainBorder" });
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover) { TargetName = "MainBorder" });
            itemTemplate.Triggers.Add(hoverTrigger);

            // 2. Selected trigger (MÁXIMA PRIORIDADE - deve ser o último)
            var selectedTrigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarSelected) { TargetName = "MainBorder" });
            selectedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderFocus) { TargetName = "MainBorder" });
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.FontWeightProperty, FontWeights.SemiBold));
            itemTemplate.Triggers.Add(selectedTrigger);

            // 3. Selected + Hover trigger (sobrescreve hover quando selecionado)
            var selectedHoverTrigger = new MultiTrigger();
            selectedHoverTrigger.Conditions.Add(new Condition(ListBoxItem.IsSelectedProperty, true));
            selectedHoverTrigger.Conditions.Add(new Condition(ListBoxItem.IsMouseOverProperty, true));
            selectedHoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarHover) { TargetName = "MainBorder" });
            selectedHoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover) { TargetName = "MainBorder" });
            selectedHoverTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));
            selectedHoverTrigger.Setters.Add(new Setter(ListBoxItem.FontWeightProperty, FontWeights.SemiBold));
            itemTemplate.Triggers.Add(selectedHoverTrigger);

            // Style com template customizado
            var itemStyle = new Style(typeof(ListBoxItem));
            itemStyle.Setters.Add(new Setter(ListBoxItem.TemplateProperty, itemTemplate));
            itemStyle.Setters.Add(new Setter(ListBoxItem.FontSizeProperty, 14.0));
            itemStyle.Setters.Add(new Setter(ListBoxItem.CursorProperty, Cursors.Hand));
            itemStyle.Setters.Add(new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            itemStyle.Setters.Add(new Setter(ListBoxItem.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            // Aplicar o estilo
            listBox.ItemContainerStyle = itemStyle;
        }
        
        /// <summary>
        /// Cria um overlay de loading com spinner centralizado
        /// </summary>
        public static Border CreateLoadingOverlay()
        {
            double spinnerSize = 20;
            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                CornerRadius = new CornerRadius(16),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Child = new Viewbox
                {
                    Width = 500,
                    Height = 500,
                    Child = CreateSpinner(spinnerSize, spinnerSize)
                }
            };
            return overlay;
        }
        
        /// <summary>
        /// Cria um spinner animado circular, usado para overlays de loading (tamanho customizável)
        /// </summary>
        public static UIElement CreateSpinner(double width, double height)
        {
            int dotCount = 11;
            double minDim = Math.Min(width, height);
            double radius = minDim / 4.0;
            double dotRadius = minDim / 20.0;
            double centerX = width / 2.0;
            double centerY = height / 2.0;

            var canvas = new System.Windows.Controls.Canvas
            {
                Width = width,
                Height = height
            };

            double[] opacities = new double[] { 0.12, 0.18, 0.24, 0.32, 0.44, 0.56, 0.68, 0.8, 0.86, 0.92, 1.0 };

            for (int i = 0; i < dotCount; i++)
            {
                double angle = i * 360.0 / 12.0;
                double rad = angle * Math.PI / 180.0;
                double x = centerX + radius * Math.Sin(rad) - dotRadius;
                double y = centerY - radius * Math.Cos(rad) - dotRadius;

                var ellipse = new System.Windows.Shapes.Ellipse
                {
                    Width = dotRadius * 2,
                    Height = dotRadius * 2,
                    Fill = System.Windows.Media.Brushes.White,
                    Opacity = opacities[i % opacities.Length]
                };
                System.Windows.Controls.Canvas.SetLeft(ellipse, x);
                System.Windows.Controls.Canvas.SetTop(ellipse, y);
                canvas.Children.Add(ellipse);
            }

            var rotate = new System.Windows.Media.RotateTransform(0, centerX, centerY);
            canvas.RenderTransform = rotate;

            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(1.1)),
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };
            canvas.Loaded += (s, e) =>
            {
                rotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animation);
            };
            return canvas;
        }

        /// <summary>
        /// Aplica o tema atual a uma janela e seus controles filhos
        /// </summary>
        public static void ApplyThemeToWindow(Window window)
        {
            window.Background = CurrentTheme.FormBackground;
            window.Foreground = CurrentTheme.Foreground;
            
            // Aplicar tema a todos os controles filhos
            ApplyThemeToContainer(window);
        }

        /// <summary>
        /// Aplica o tema recursivamente a todos os controles filhos de um container
        /// </summary>
        public static void ApplyThemeToContainer(DependencyObject container)
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(container);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(container, i);
                
                // Aplicar tema baseado no tipo do controle
                switch (child)
                {
                    case DataGrid dataGrid:
                        SetDataGridDarkTheme(dataGrid);
                        break;
                        
                    case TextBox textBox:
                        if (textBox.Style == null)
                        {
                            textBox.Background = CurrentTheme.InputBackground;
                            textBox.Foreground = CurrentTheme.InputForeground;
                            textBox.BorderBrush = CurrentTheme.InputBorder;
                        }
                        break;
                        
                    case ComboBox comboBox:
                        if (comboBox.Style == null)
                        {
                            comboBox.Background = CurrentTheme.InputBackground;
                            comboBox.Foreground = CurrentTheme.InputForeground;
                            comboBox.BorderBrush = CurrentTheme.InputBorder;
                        }
                        break;
                        
                    case Label label:
                        if (label.Style == null)
                        {
                            label.Foreground = CurrentTheme.Foreground;
                        }
                        break;
                }
                
                // Aplicar recursivamente aos filhos
                ApplyThemeToContainer(child);
            }
        }

        /// <summary>
        /// Método utilitário para alterar o tema em tempo de execução
        /// </summary>
        public static void SwitchTheme(ThemeType newTheme)
        {
            CurrentThemeType = newTheme;
            
            // Aplicar novo tema a todas as janelas abertas
            foreach (Window window in Application.Current.Windows)
            {
                ApplyThemeToWindow(window);
            }
        }

        /// <summary>
        /// Cria um card container estilizado
        /// </summary>
        public static Border CreateStyledCard(UIElement content, double cornerRadius = 8, bool hasShadow = true)
        {
            var card = new Border
            {
                Background = CurrentTheme.ContentBackground,
                BorderBrush = CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(cornerRadius),
                Padding = new Thickness(16),
                Child = content
            };

            if (hasShadow)
            {
                card.Effect = new DropShadowEffect
                {
                    BlurRadius = 10,
                    ShadowDepth = 4,
                    Opacity = 0.2,
                    Color = Colors.Black
                };
            }

            return card;
        }

        /// <summary>
        /// Cria um ScrollViewer com scrollbar customizada já aplicada
        /// </summary>
        public static ScrollViewer CreateStyledScrollViewer(ScrollBarVisibility verticalVisibility = ScrollBarVisibility.Auto, ScrollBarVisibility horizontalVisibility = ScrollBarVisibility.Auto)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = verticalVisibility,
                HorizontalScrollBarVisibility = horizontalVisibility,
                Background = Brushes.Transparent
            };

            ApplyCustomScrollbar(scrollViewer);
            return scrollViewer;
        }

        /// <summary>
        /// Aplica scrollbar customizada a um ScrollViewer com tema escuro
        /// </summary>
        public static void ApplyCustomScrollbar(ScrollViewer scrollViewer)
        {
            // Criar style customizado para o ScrollViewer
            var scrollViewerStyle = new Style(typeof(ScrollViewer));
            
            // Template XAML para ScrollViewer com scrollbar customizada sobreposta
            var templateXaml = @"
                <ControlTemplate TargetType='ScrollViewer' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Grid>
                        <!-- Conteúdo principal ocupa toda a área -->
                        <ScrollContentPresenter Margin='{TemplateBinding Padding}'
                                                Content='{TemplateBinding Content}'
                                                ContentTemplate='{TemplateBinding ContentTemplate}'
                                                CanContentScroll='{TemplateBinding CanContentScroll}'/>
                        
                        <!-- ScrollBar Vertical - Sobreposta no canto direito -->
                        <ScrollBar Name='PART_VerticalScrollBar'
                                   HorizontalAlignment='Right'
                                   VerticalAlignment='Stretch'
                                   Orientation='Vertical'
                                   Value='{TemplateBinding VerticalOffset}'
                                   Maximum='{TemplateBinding ScrollableHeight}'
                                   ViewportSize='{TemplateBinding ViewportHeight}'
                                   Visibility='{TemplateBinding ComputedVerticalScrollBarVisibility}'
                                   Margin='0,4,4,4'
                                   Width='10'
                                   Background='Transparent'
                                   Opacity='0.6'>
                            <ScrollBar.Style>
                                <Style TargetType='ScrollBar'>
                                    <Setter Property='Template'>
                                        <Setter.Value>
                                            <ControlTemplate TargetType='ScrollBar'>
                                                <Grid>
                                                    <Track Name='PART_Track' IsDirectionReversed='True'>
                                                        <Track.Thumb>
                                                            <Thumb>
                                                                <Thumb.Template>
                                                                    <ControlTemplate TargetType='Thumb'>
                                                                        <Border Background='#FF666666' 
                                                                                CornerRadius='5' 
                                                                                Margin='1'/>
                                                                    </ControlTemplate>
                                                                </Thumb.Template>
                                                            </Thumb>
                                                        </Track.Thumb>
                                                    </Track>
                                                </Grid>
                                                
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter Property='Opacity' Value='1.0'/>
                                                        <Setter TargetName='PART_Track' Property='Thumb.Template'>
                                                            <Setter.Value>
                                                                <ControlTemplate TargetType='Thumb'>
                                                                    <Border Background='#FF888888' 
                                                                            CornerRadius='5' 
                                                                            Margin='1'/>
                                                                </ControlTemplate>
                                                            </Setter.Value>
                                                        </Setter>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </ScrollBar.Style>
                        </ScrollBar>
                        
                        <!-- ScrollBar Horizontal - Sobreposta na parte inferior -->
                        <ScrollBar Name='PART_HorizontalScrollBar'
                                   HorizontalAlignment='Stretch'
                                   VerticalAlignment='Bottom'
                                   Orientation='Horizontal'
                                   Value='{TemplateBinding HorizontalOffset}'
                                   Maximum='{TemplateBinding ScrollableWidth}'
                                   ViewportSize='{TemplateBinding ViewportWidth}'
                                   Visibility='{TemplateBinding ComputedHorizontalScrollBarVisibility}'
                                   Margin='4,0,4,4'
                                   Height='10'
                                   Background='Transparent'
                                   Opacity='0.6'>
                            <ScrollBar.Style>
                                <Style TargetType='ScrollBar'>
                                    <Setter Property='Template'>
                                        <Setter.Value>
                                            <ControlTemplate TargetType='ScrollBar'>
                                                <Grid>
                                                    <Track Name='PART_Track'>
                                                        <Track.Thumb>
                                                            <Thumb>
                                                                <Thumb.Template>
                                                                    <ControlTemplate TargetType='Thumb'>
                                                                        <Border Background='#FF666666' 
                                                                                CornerRadius='5' 
                                                                                Margin='1'/>
                                                                    </ControlTemplate>
                                                                </Thumb.Template>
                                                            </Thumb>
                                                        </Track.Thumb>
                                                    </Track>
                                                </Grid>
                                                
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter Property='Opacity' Value='1.0'/>
                                                        <Setter TargetName='PART_Track' Property='Thumb.Template'>
                                                            <Setter.Value>
                                                                <ControlTemplate TargetType='Thumb'>
                                                                    <Border Background='#FF888888' 
                                                                            CornerRadius='5' 
                                                                            Margin='1'/>
                                                                </ControlTemplate>
                                                            </Setter.Value>
                                                        </Setter>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </ScrollBar.Style>
                        </ScrollBar>
                    </Grid>
                </ControlTemplate>";

            try
            {
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);
                scrollViewerStyle.Setters.Add(new Setter(ScrollViewer.TemplateProperty, template));
                scrollViewer.Style = scrollViewerStyle;
            }
            catch
            {
                // Fallback: aplicar apenas cores básicas se o XAML falhar
                scrollViewer.Background = CurrentTheme.PanelBackground;
            }
        }

        /// <summary>
        /// Retorna informações sobre o tema atual
        /// </summary>
        public static string GetThemeInfo()
        {
            return $"Tema Atual: {CurrentThemeType}\n" +
                   $"Cor Principal: {CurrentTheme.FormBackground}\n" +
                   $"Cor de Destaque: {CurrentTheme.Accent}\n" +
                   $"Modo Escuro: {(CurrentThemeType == ThemeType.Dark ? "Sim" : "Não")}";
        }
        #endregion
    }
}
