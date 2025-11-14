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
    /// Advanced theme and style system for DevStackManager.
    /// Provides color definitions, styles, animations, and helper methods for creating themed controls.
    /// Supports multiple themes (Dark/Light/HighContrast) with smooth animations and modern UI components.
    /// </summary>
    public static class ThemeManager
    {
        #region UI Constants
        /// <summary>
        /// Animation duration in milliseconds for button hover effects.
        /// </summary>
        private const int ANIMATION_BUTTON_HOVER_MS = 200;
        
        /// <summary>
        /// Animation duration in milliseconds for fade-in transitions.
        /// </summary>
        private const int ANIMATION_FADE_IN_MS = 300;
        
        /// <summary>
        /// Animation duration in milliseconds for fade-out transitions.
        /// </summary>
        private const int ANIMATION_FADE_OUT_MS = 200;
        
        /// <summary>
        /// Animation duration in milliseconds for slide-in transitions.
        /// </summary>
        private const int ANIMATION_SLIDE_IN_MS = 400;
        
        /// <summary>
        /// Standard font size for regular text.
        /// </summary>
        private const double FONT_SIZE_STANDARD = 14.0;
        
        /// <summary>
        /// Font size for console output text.
        /// </summary>
        private const double FONT_SIZE_CONSOLE = 13.0;
        
        /// <summary>
        /// Font size for header text.
        /// </summary>
        private const double FONT_SIZE_HEADER = 14.0;
        
        /// <summary>
        /// Font size for table row text.
        /// </summary>
        private const double FONT_SIZE_ROW = 14.0;
        
        /// <summary>
        /// Standard horizontal padding for controls.
        /// </summary>
        private const double PADDING_STANDARD_HORIZONTAL = 10;
        
        /// <summary>
        /// Standard vertical padding for controls.
        /// </summary>
        private const double PADDING_STANDARD_VERTICAL = 8;
        
        /// <summary>
        /// Vertical padding for console text areas.
        /// </summary>
        private const double PADDING_CONSOLE_VERTICAL = 8;
        
        /// <summary>
        /// Horizontal padding for label controls.
        /// </summary>
        private const double PADDING_LABEL_HORIZONTAL = 10;
        
        /// <summary>
        /// Top vertical padding for label controls.
        /// </summary>
        private const double PADDING_LABEL_VERTICAL_TOP = 4;
        
        /// <summary>
        /// Bottom vertical padding for label controls.
        /// </summary>
        private const double PADDING_LABEL_VERTICAL_BOTTOM = 4;
        
        /// <summary>
        /// Horizontal padding for header elements.
        /// </summary>
        private const double PADDING_HEADER_HORIZONTAL = 12;
        
        /// <summary>
        /// Vertical padding for header elements.
        /// </summary>
        private const double PADDING_HEADER_VERTICAL = 10;
        
        /// <summary>
        /// Horizontal padding for combobox controls.
        /// </summary>
        private const double PADDING_COMBOBOX_HORIZONTAL = 10;
        
        /// <summary>
        /// Vertical padding for combobox controls.
        /// </summary>
        private const double PADDING_COMBOBOX_VERTICAL = 8;
        
        /// <summary>
        /// Horizontal padding for combobox items.
        /// </summary>
        private const double PADDING_COMBOBOX_ITEM_HORIZONTAL = 10;
        
        /// <summary>
        /// Vertical padding for combobox items.
        /// </summary>
        private const double PADDING_COMBOBOX_ITEM_VERTICAL = 6;
        
        /// <summary>
        /// Horizontal padding for checkbox controls.
        /// </summary>
        private const double PADDING_CHECKBOX_HORIZONTAL = 12;
        
        /// <summary>
        /// Vertical padding for checkbox controls.
        /// </summary>
        private const double PADDING_CHECKBOX_VERTICAL = 8;
        
        /// <summary>
        /// Horizontal padding for card components.
        /// </summary>
        private const double PADDING_CARD_HORIZONTAL = 16;
        
        /// <summary>
        /// Vertical padding for card components.
        /// </summary>
        /// <summary>
        /// Vertical padding for card components.
        /// </summary>
        private const double PADDING_CARD_VERTICAL = 12;
        
        /// <summary>
        /// Horizontal margin for combobox content.
        /// </summary>
        private const double MARGIN_COMBOBOX_CONTENT_HORIZONTAL = 10;
        
        /// <summary>
        /// Top vertical margin for combobox content.
        /// </summary>
        private const double MARGIN_COMBOBOX_CONTENT_VERTICAL_TOP = 5;
        
        /// <summary>
        /// Bottom vertical margin for combobox content.
        /// </summary>
        private const double MARGIN_COMBOBOX_CONTENT_VERTICAL_BOTTOM = 8;
        
        /// <summary>
        /// Horizontal margin for message box content.
        /// </summary>
        private const double MARGIN_MESSAGEBOX_CONTENT_HORIZONTAL = 55;
        
        /// <summary>
        /// Top vertical margin for message box content.
        /// </summary>
        private const double MARGIN_MESSAGEBOX_CONTENT_VERTICAL_TOP = 20;
        
        /// <summary>
        /// Right margin for message box content.
        /// </summary>
        private const double MARGIN_MESSAGEBOX_CONTENT_VERTICAL_RIGHT = 30;
        
        /// <summary>
        /// Bottom vertical margin for message box content.
        /// </summary>
        /// <summary>
        /// Bottom vertical margin for message box content.
        /// </summary>
        private const double MARGIN_MESSAGEBOX_CONTENT_VERTICAL_BOTTOM = 10;
        
        /// <summary>
        /// Minimum height for textbox controls in pixels.
        /// </summary>
        private const double MIN_HEIGHT_TEXTBOX = 35;
        
        /// <summary>
        /// Minimum height for combobox controls in pixels.
        /// </summary>
        private const double MIN_HEIGHT_COMBOBOX = 35;
        
        /// <summary>
        /// Minimum height for table rows in pixels.
        /// </summary>
        private const double MIN_HEIGHT_ROW = 35;
        
        /// <summary>
        /// Width of scrollbar controls in pixels.
        /// </summary>
        private const double SCROLLBAR_WIDTH = 12;
        
        /// <summary>
        /// Width of combobox dropdown button in pixels.
        /// </summary>
        private const double COMBOBOX_DROPDOWN_WIDTH = 20;
        
        /// <summary>
        /// Minimum width for button controls in pixels.
        /// </summary>
        private const double MIN_BUTTON_WIDTH = 80;
        
        /// <summary>
        /// Size of message box icon in pixels.
        /// </summary>
        private const double MESSAGEBOX_ICON_SIZE = 500;
        
        /// <summary>
        /// Opacity value for disabled controls.
        /// </summary>
        private const double OPACITY_DISABLED = 0.6;
        
        /// <summary>
        /// Opacity value for shadow effects.
        /// </summary>
        private const double OPACITY_SHADOW = 0.3;
        
        /// <summary>
        /// Opacity value for scrollbar track background.
        /// </summary>
        private const double OPACITY_SCROLLBAR_TRACK = 0.3;
        
        /// <summary>
        /// Default radius for glow effects in pixels.
        /// </summary>
        private const double GLOW_RADIUS_DEFAULT = 10;
        
        /// <summary>
        /// Default opacity for glow effects.
        /// </summary>
        private const double GLOW_OPACITY_DEFAULT = 0.8;
        
        /// <summary>
        /// Default opacity for shadow effects.
        /// </summary>
        private const double SHADOW_OPACITY_DEFAULT = 0.3;
        
        /// <summary>
        /// Factor for darkening colors (0.0-1.0).
        /// </summary>
        private const double COLOR_DARKEN_FACTOR = 0.3;
        
        /// <summary>
        /// Default initial value for progress bars.
        /// </summary>
        private const double PROGRESS_BAR_DEFAULT_VALUE = 0;
        
        /// <summary>
        /// Default maximum value for progress bars.
        /// </summary>
        private const double PROGRESS_BAR_DEFAULT_MAXIMUM = 100;
        
        /// <summary>
        /// Center point for render transformations (0.5 = 50%).
        /// </summary>
        private const double RENDER_TRANSFORM_CENTER = 0.5;
        
        /// <summary>
        /// Red component for dark danger button hover color.
        /// </summary>
        /// <summary>
        /// Red component for dark danger button hover color.
        /// </summary>
        private const byte DARK_DANGER_HOVER_R = 200;
        
        /// <summary>
        /// Green component for dark danger button hover color.
        /// </summary>
        private const byte DARK_DANGER_HOVER_G = 35;
        
        /// <summary>
        /// Blue component for dark danger button hover color.
        /// </summary>
        private const byte DARK_DANGER_HOVER_B = 51;
        
        /// <summary>
        /// Red component for dark danger button pressed color.
        /// </summary>
        private const byte DARK_DANGER_PRESSED_R = 180;
        
        /// <summary>
        /// Green component for dark danger button pressed color.
        /// </summary>
        private const byte DARK_DANGER_PRESSED_G = 25;
        
        /// <summary>
        /// Blue component for dark danger button pressed color.
        /// </summary>
        private const byte DARK_DANGER_PRESSED_B = 41;
        
        /// <summary>
        /// Red component for dark form background color.
        /// </summary>
        private const byte DARK_FORM_BACKGROUND_R = 22;
        
        /// <summary>
        /// Green component for dark form background color.
        /// </summary>
        private const byte DARK_FORM_BACKGROUND_G = 27;
        
        /// <summary>
        /// Blue component for dark form background color.
        /// </summary>
        private const byte DARK_FORM_BACKGROUND_B = 34;
        
        /// <summary>
        /// Red component for dark control background color.
        /// </summary>
        private const byte DARK_CONTROL_BACKGROUND_R = 32;
        
        /// <summary>
        /// Green component for dark control background color.
        /// </summary>
        private const byte DARK_CONTROL_BACKGROUND_G = 39;
        
        /// <summary>
        /// Blue component for dark control background color.
        /// </summary>
        private const byte DARK_CONTROL_BACKGROUND_B = 49;
        
        /// <summary>
        /// Red component for dark button hover color.
        /// </summary>
        private const byte DARK_BUTTON_HOVER_R = 58;
        
        /// <summary>
        /// Green component for dark button hover color.
        /// </summary>
        private const byte DARK_BUTTON_HOVER_G = 150;
        
        /// <summary>
        /// Blue component for dark button hover color.
        /// </summary>
        private const byte DARK_BUTTON_HOVER_B = 255;
        
        /// <summary>
        /// Red component for dark button pressed color.
        /// </summary>
        private const byte DARK_BUTTON_PRESSED_R = 25;
        
        /// <summary>
        /// Green component for dark button pressed color.
        /// </summary>
        private const byte DARK_BUTTON_PRESSED_G = 118;
        
        /// <summary>
        /// Blue component for dark button pressed color.
        /// </summary>
        /// <summary>
        /// Blue component for dark button pressed color.
        /// </summary>
        private const byte DARK_BUTTON_PRESSED_B = 220;
        
        /// <summary>
        /// Red component for dark accent pressed color.
        /// </summary>
        private const byte DARK_ACCENT_PRESSED_R = 40;
        
        /// <summary>
        /// Green component for dark accent pressed color.
        /// </summary>
        private const byte DARK_ACCENT_PRESSED_G = 175;
        
        /// <summary>
        /// Blue component for dark accent pressed color.
        /// </summary>
        private const byte DARK_ACCENT_PRESSED_B = 131;
        
        /// <summary>
        /// Red component for dark info color.
        /// </summary>
        private const byte DARK_INFO_R = 58;
        
        /// <summary>
        /// Green component for dark info color.
        /// </summary>
        private const byte DARK_INFO_G = 150;
        
        /// <summary>
        /// Blue component for dark info color.
        /// </summary>
        private const byte DARK_INFO_B = 255;
        
        /// <summary>
        /// Alpha channel value for notification background transparency.
        /// </summary>
        private const byte ALPHA_NOTIFICATION_BACKGROUND = 25;
        
        /// <summary>
        /// Alpha channel value for selection overlay transparency.
        /// </summary>
        private const byte ALPHA_SELECTION = 128;
        
        /// <summary>
        /// Alpha channel value for dark overlay transparency.
        /// </summary>
        private const byte ALPHA_OVERLAY_DARK = 180;
        
        /// <summary>
        /// Alpha channel value for semi-transparent overlay.
        /// </summary>
        private const byte ALPHA_OVERLAY_SEMI = 50;
        
        /// <summary>
        /// Gradient angle in degrees for horizontal gradients.
        /// </summary>
        private const double GRADIENT_ANGLE_HORIZONTAL = 90;
        
        /// <summary>
        /// Gradient angle in degrees for diagonal gradients.
        /// </summary>
        private const double GRADIENT_ANGLE_DIAGONAL = 45;
        #endregion
        
        #region Theme Classes
        /// <summary>
        /// Available theme types enumeration.
        /// </summary>
        public enum ThemeType
        {
            Dark,
            Light,
            HighContrast
        }

        /// <summary>
        /// Theme color palette supporting multiple color schemes.
        /// </summary>
        public class ThemeColors
        {
            /// <summary>
            /// Pure white color brush (hardcoded for control use).
            /// </summary>
            public SolidColorBrush PureWhite { get; set; } = null!;
            
            /// <summary>
            /// Pure black color brush (hardcoded for control use).
            /// </summary>
            public SolidColorBrush PureBlack { get; set; } = null!;
            
            /// <summary>
            /// Danger button hover state color.
            /// </summary>
            public SolidColorBrush DangerHover { get; set; } = null!;
            
            /// <summary>
            /// Danger button pressed state color.
            /// </summary>
            public SolidColorBrush DangerPressed { get; set; } = null!;
            
            /// <summary>
            /// Warning button hover state color.
            /// </summary>
            public SolidColorBrush WarningHover { get; set; } = null!;
            
            /// <summary>
            /// Warning button pressed state color.
            /// </summary>
            public SolidColorBrush WarningPressed { get; set; } = null!;
            
            /// <summary>
            /// Selection brush for text selection.
            /// </summary>
            public SolidColorBrush SelectionBrush { get; set; } = null!;
            
            /// <summary>
            /// White overlay color with transparency.
            /// </summary>
            public SolidColorBrush OverlayWhite { get; set; } = null!;
            
            /// <summary>
            /// Console text selection color.
            /// </summary>
            public SolidColorBrush ConsoleSelectionBrush { get; set; } = null!;
            
            /// <summary>
            /// Foreground color for selected rows.
            /// </summary>
            public SolidColorBrush RowSelectedForeground { get; set; } = null!;
            
            /// <summary>
            /// Main form background color.
            /// </summary>
            public SolidColorBrush FormBackground { get; set; } = null!;
            
            /// <summary>
            /// Primary text/foreground color.
            /// </summary>
            public SolidColorBrush Foreground { get; set; } = null!;
            
            /// <summary>
            /// Control background color.
            /// </summary>
            public SolidColorBrush ControlBackground { get; set; } = null!;

            /// <summary>
            /// Success notification background color.
            /// </summary>
            public SolidColorBrush SuccessBackground { get; set; } = null!;
            
            /// <summary>
            /// Warning notification background color.
            /// </summary>
            public SolidColorBrush WarningBackground { get; set; } = null!;
            
            /// <summary>
            /// Danger notification background color.
            /// </summary>
            public SolidColorBrush DangerBackground { get; set; } = null!;
            
            /// <summary>
            /// Info notification background color.
            /// </summary>
            public SolidColorBrush InfoBackground { get; set; } = null!;

            /// <summary>
            /// Default button background color.
            /// </summary>
            public SolidColorBrush ButtonBackground { get; set; } = null!;
            
            /// <summary>
            /// Button text/foreground color.
            /// </summary>
            public SolidColorBrush ButtonForeground { get; set; } = null!;
            
            /// <summary>
            /// Button hover state color.
            /// </summary>
            public SolidColorBrush ButtonHover { get; set; } = null!;
            
            /// <summary>
            /// Button pressed state color.
            /// </summary>
            public SolidColorBrush ButtonPressed { get; set; } = null!;
            
            /// <summary>
            /// Button disabled state color.
            /// </summary>
            public SolidColorBrush ButtonDisabled { get; set; } = null!;

            /// <summary>
            /// Primary accent color for highlights.
            /// </summary>
            public SolidColorBrush Accent { get; set; } = null!;
            
            /// <summary>
            /// Accent color hover state.
            /// </summary>
            public SolidColorBrush AccentHover { get; set; } = null!;
            
            /// <summary>
            /// Accent color pressed state.
            /// </summary>
            public SolidColorBrush AccentPressed { get; set; } = null!;
            
            /// <summary>
            /// Warning color for alerts.
            /// </summary>
            public SolidColorBrush Warning { get; set; } = null!;
            
            /// <summary>
            /// Danger color for errors and destructive actions.
            /// </summary>
            public SolidColorBrush Danger { get; set; } = null!;
            
            /// <summary>
            /// Success color for positive feedback.
            /// </summary>
            public SolidColorBrush Success { get; set; } = null!;
            
            /// <summary>
            /// Info color for informational messages.
            /// </summary>
            public SolidColorBrush Info { get; set; } = null!;

            /// <summary>
            /// Grid/table background color.
            /// </summary>
            public SolidColorBrush GridBackground { get; set; } = null!;
            
            /// <summary>
            /// Grid text color.
            /// </summary>
            public SolidColorBrush GridForeground { get; set; } = null!;
            
            /// <summary>
            /// Grid header background color.
            /// </summary>
            public SolidColorBrush GridHeaderBackground { get; set; } = null!;
            
            /// <summary>
            /// Grid header text color.
            /// </summary>
            public SolidColorBrush GridHeaderForeground { get; set; } = null!;
            
            /// <summary>
            /// Alternating row background color in grids.
            /// </summary>
            public SolidColorBrush GridAlternateRow { get; set; } = null!;
            
            /// <summary>
            /// Selected row background color in grids.
            /// </summary>
            public SolidColorBrush GridSelectedRow { get; set; } = null!;
            
            /// <summary>
            /// Hover row background color in grids.
            /// </summary>
            public SolidColorBrush GridHoverRow { get; set; } = null!;

            /// <summary>
            /// Status bar background color.
            /// </summary>
            public SolidColorBrush StatusBackground { get; set; } = null!;
            
            /// <summary>
            /// Status bar text color.
            /// </summary>
            public SolidColorBrush StatusForeground { get; set; } = null!;
            
            /// <summary>
            /// Sidebar/navigation background color.
            /// </summary>
            public SolidColorBrush SidebarBackground { get; set; } = null!;
            
            /// <summary>
            /// Sidebar selected item background color.
            /// </summary>
            public SolidColorBrush SidebarSelected { get; set; } = null!;
            
            /// <summary>
            /// Sidebar hover state background color.
            /// </summary>
            public SolidColorBrush SidebarHover { get; set; } = null!;

            /// <summary>
            /// Default border color.
            /// </summary>
            public SolidColorBrush Border { get; set; } = null!;
            
            /// <summary>
            /// Border color on hover.
            /// </summary>
            public SolidColorBrush BorderHover { get; set; } = null!;
            
            /// <summary>
            /// Border color when focused.
            /// </summary>
            public SolidColorBrush BorderFocus { get; set; } = null!;
            
            /// <summary>
            /// Border color for active/selected state.
            /// </summary>
            public SolidColorBrush BorderActive { get; set; } = null!;

            /// <summary>
            /// Main content area background color.
            /// </summary>
            public SolidColorBrush ContentBackground { get; set; } = null!;
            
            /// <summary>
            /// Panel background color.
            /// </summary>
            public SolidColorBrush PanelBackground { get; set; } = null!;
            
            /// <summary>
            /// Console background color.
            /// </summary>
            public SolidColorBrush ConsoleBackground { get; set; } = null!;
            
            /// <summary>
            /// Console text color.
            /// </summary>
            public SolidColorBrush ConsoleForeground { get; set; } = null!;

            /// <summary>
            /// Input field background color.
            /// </summary>
            public SolidColorBrush InputBackground { get; set; } = null!;
            
            /// <summary>
            /// Input field text color.
            /// </summary>
            public SolidColorBrush InputForeground { get; set; } = null!;
            
            /// <summary>
            /// Input field border color.
            /// </summary>
            public SolidColorBrush InputBorder { get; set; } = null!;
            
            /// <summary>
            /// Input field border color when focused.
            /// </summary>
            public SolidColorBrush InputFocusBorder { get; set; } = null!;
            
            /// <summary>
            /// Input field border color on hover.
            /// </summary>
            public SolidColorBrush InputHoverBorder { get; set; } = null!;
            
            /// <summary>
            /// Dropdown menu background color.
            /// </summary>
            public SolidColorBrush DropdownBackground { get; set; } = null!;

            /// <summary>
            /// Muted/secondary text color.
            /// </summary>
            public SolidColorBrush TextMuted { get; set; } = null!;
            
            /// <summary>
            /// Secondary text color (lighter than muted).
            /// </summary>
            public SolidColorBrush TextSecondary { get; set; } = null!;
            
            /// <summary>
            /// Disabled text color.
            /// </summary>
            public SolidColorBrush TextDisabled { get; set; } = null!;
            
            /// <summary>
            /// Link text color.
            /// </summary>
            public SolidColorBrush TextLink { get; set; } = null!;
            
            /// <summary>
            /// Link text color on hover.
            /// </summary>
            public SolidColorBrush TextLinkHover { get; set; } = null!;

            /// <summary>
            /// Overlay/modal background color.
            /// </summary>
            public SolidColorBrush OverlayBackground { get; set; } = null!;
            
            /// <summary>
            /// Tooltip background color.
            /// </summary>
            public SolidColorBrush TooltipBackground { get; set; } = null!;
            
            /// <summary>
            /// Tooltip text color.
            /// </summary>
            public SolidColorBrush TooltipForeground { get; set; } = null!;

            /// <summary>
            /// Button gradient brush for modern effects.
            /// </summary>
            public LinearGradientBrush ButtonGradient { get; set; } = null!;
            
            /// <summary>
            /// Accent gradient brush for modern effects.
            /// </summary>
            public LinearGradientBrush AccentGradient { get; set; } = null!;
            
            /// <summary>
            /// Header gradient brush for modern effects.
            /// </summary>
            public LinearGradientBrush HeaderGradient { get; set; } = null!;
            
            /// <summary>
            /// Dashboard card background color.
            /// </summary>
            public SolidColorBrush DashboardCardBackground { get; set; } = null!;
            
            /// <summary>
            /// Dashboard card hover background color.
            /// </summary>
            public SolidColorBrush DashboardCardHover { get; set; } = null!;
            
            /// <summary>
            /// Dashboard card default hover background color.
            /// </summary>
            public SolidColorBrush DashboardCardHoverDefault { get; set; } = null!;
            
            /// <summary>
            /// Dashboard error text color.
            /// </summary>
            public SolidColorBrush DashboardErrorText { get; set; } = null!;
            
            /// <summary>
            /// Dashboard muted text color.
            /// </summary>
            public SolidColorBrush DashboardMutedText { get; set; } = null!;
            
            /// <summary>
            /// Dashboard accent blue color.
            /// </summary>
            public SolidColorBrush DashboardAccentBlue { get; set; } = null!;
            
            /// <summary>
            /// Dashboard service status yellow color.
            /// </summary>
            public SolidColorBrush DashboardServiceYellow { get; set; } = null!;
            
            /// <summary>
            /// Dashboard footer background color.
            /// </summary>
            public SolidColorBrush DashboardFooterBackground { get; set; } = null!;
        }

        /// <summary>
        /// Animation settings for theme transitions and effects.
        /// </summary>
        public class ThemeAnimationSettings
        {
            /// <summary>
            /// Button hover animation duration.
            /// </summary>
            public TimeSpan ButtonHoverDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_BUTTON_HOVER_MS);
            
            /// <summary>
            /// Fade in animation duration.
            /// </summary>
            public TimeSpan FadeInDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_FADE_IN_MS);
            
            /// <summary>
            /// Fade out animation duration.
            /// </summary>
            public TimeSpan FadeOutDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_FADE_OUT_MS);
            
            /// <summary>
            /// Slide in animation duration.
            /// </summary>
            public TimeSpan SlideInDuration { get; set; } = TimeSpan.FromMilliseconds(ANIMATION_SLIDE_IN_MS);
            
            /// <summary>
            /// Standard easing function for smooth animations.
            /// </summary>
            public IEasingFunction StandardEasing { get; set; } = new CubicEase { EasingMode = EasingMode.EaseOut };
            
            /// <summary>
            /// Bounce easing function for playful animations.
            /// </summary>
            public IEasingFunction BounceEasing { get; set; } = new BounceEase { EasingMode = EasingMode.EaseOut };
        }
        #endregion

        #region Theme Definition
        /// <summary>
        /// Modern optimized dark theme with comprehensive color palette.
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

            SuccessBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, 56, 211, 159)),
            WarningBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, 255, 196, 0)),
            DangerBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, 248, 81, 73)),
            InfoBackground = new SolidColorBrush(Color.FromArgb(ALPHA_NOTIFICATION_BACKGROUND, DARK_BUTTON_HOVER_R, DARK_BUTTON_HOVER_G, DARK_BUTTON_HOVER_B)),

            ButtonBackground = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            ButtonForeground = new SolidColorBrush(Colors.White),
            ButtonHover = new SolidColorBrush(Color.FromRgb(DARK_BUTTON_HOVER_R, DARK_BUTTON_HOVER_G, DARK_BUTTON_HOVER_B)),
            ButtonPressed = new SolidColorBrush(Color.FromRgb(DARK_BUTTON_PRESSED_R, DARK_BUTTON_PRESSED_G, DARK_BUTTON_PRESSED_B)),
            ButtonDisabled = new SolidColorBrush(Color.FromRgb(87, 96, 106)),

            Accent = new SolidColorBrush(Color.FromRgb(56, 211, 159)),
            AccentHover = new SolidColorBrush(Color.FromRgb(46, 194, 145)),
            AccentPressed = new SolidColorBrush(Color.FromRgb(DARK_ACCENT_PRESSED_R, DARK_ACCENT_PRESSED_G, DARK_ACCENT_PRESSED_B)),
            Warning = new SolidColorBrush(Color.FromRgb(255, 196, 0)),
            Danger = new SolidColorBrush(Color.FromRgb(248, 81, 73)),
            Success = new SolidColorBrush(Color.FromRgb(56, 211, 159)),
            Info = new SolidColorBrush(Color.FromRgb(DARK_INFO_R, DARK_INFO_G, DARK_INFO_B)),

            GridBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            GridForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            GridHeaderBackground = new SolidColorBrush(Color.FromRgb(45, 55, 68)),
            GridHeaderForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            GridAlternateRow = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            GridSelectedRow = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            GridHoverRow = new SolidColorBrush(Color.FromRgb(45, 55, 68)),

            StatusBackground = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
            StatusForeground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
            SidebarBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            SidebarSelected = new SolidColorBrush(Color.FromRgb(36, 46, 59)),
            SidebarHover = new SolidColorBrush(Color.FromRgb(45, 55, 68)),

            Border = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
            BorderHover = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            BorderFocus = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            BorderActive = new SolidColorBrush(Color.FromRgb(46, 194, 145)),

            ContentBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            PanelBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            ConsoleBackground = new SolidColorBrush(Color.FromRgb(13, 17, 23)),
            ConsoleForeground = new SolidColorBrush(Color.FromRgb(201, 209, 217)),

            InputBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            InputForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            InputBorder = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
            InputFocusBorder = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            InputHoverBorder = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            DropdownBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),

            TextMuted = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
            TextSecondary = new SolidColorBrush(Color.FromRgb(166, 173, 186)),
            TextDisabled = new SolidColorBrush(Color.FromRgb(87, 96, 106)),
            TextLink = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            TextLinkHover = new SolidColorBrush(Color.FromRgb(33, 136, 255)),

            OverlayBackground = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            TooltipBackground = new SolidColorBrush(Color.FromRgb(45, 55, 68)),
            TooltipForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),

            DashboardCardBackground = new SolidColorBrush(Color.FromRgb(55, 58, 64)),
            DashboardCardHover = new SolidColorBrush(Color.FromRgb(75, 85, 99)),
            DashboardCardHoverDefault = new SolidColorBrush(Color.FromRgb(65, 68, 74)),
            DashboardErrorText = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            DashboardMutedText = new SolidColorBrush(Color.FromRgb(169, 169, 169)),
            DashboardAccentBlue = new SolidColorBrush(Color.FromRgb(100, 149, 237)),
            DashboardServiceYellow = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            DashboardFooterBackground = new SolidColorBrush(Color.FromArgb(50, 75, 85, 99)),

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
        /// Modern light theme (for future implementations).
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

            SuccessBackground = new SolidColorBrush(Color.FromArgb(25, 40, 167, 69)),
            WarningBackground = new SolidColorBrush(Color.FromArgb(25, 255, 193, 7)),
            DangerBackground = new SolidColorBrush(Color.FromArgb(25, 220, 53, 69)),
            InfoBackground = new SolidColorBrush(Color.FromArgb(25, 23, 162, 184)),

            ButtonBackground = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            ButtonForeground = new SolidColorBrush(Colors.White),
            ButtonHover = new SolidColorBrush(Color.FromRgb(0, 110, 230)),
            ButtonPressed = new SolidColorBrush(Color.FromRgb(0, 100, 200)),
            ButtonDisabled = new SolidColorBrush(Color.FromRgb(173, 181, 189)),

            Accent = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
            AccentHover = new SolidColorBrush(Color.FromRgb(33, 136, 56)),
            AccentPressed = new SolidColorBrush(Color.FromRgb(25, 105, 43)),
            Warning = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            Danger = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            Success = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
            Info = new SolidColorBrush(Color.FromRgb(23, 162, 184)),

            GridBackground = new SolidColorBrush(Colors.White),
            GridForeground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
            GridHeaderBackground = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
            GridHeaderForeground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
            GridAlternateRow = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            GridSelectedRow = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            GridHoverRow = new SolidColorBrush(Color.FromRgb(233, 236, 239)),

            StatusBackground = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            StatusForeground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            SidebarBackground = new SolidColorBrush(Color.FromRgb(243, 244, 246)),
            SidebarSelected = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
            SidebarHover = new SolidColorBrush(Color.FromRgb(248, 249, 250)),

            Border = new SolidColorBrush(Color.FromRgb(206, 212, 218)),
            BorderHover = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            BorderFocus = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            BorderActive = new SolidColorBrush(Color.FromRgb(40, 167, 69)),

            ContentBackground = new SolidColorBrush(Colors.White),
            PanelBackground = new SolidColorBrush(Color.FromRgb(244, 245, 246)),
            ConsoleBackground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
            ConsoleForeground = new SolidColorBrush(Color.FromRgb(248, 249, 250)),

            InputBackground = new SolidColorBrush(Colors.White),
            InputForeground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
            InputBorder = new SolidColorBrush(Color.FromRgb(206, 212, 218)),
            InputFocusBorder = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            InputHoverBorder = new SolidColorBrush(Color.FromRgb(134, 142, 150)),
            DropdownBackground = new SolidColorBrush(Colors.White),

            TextMuted = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            TextSecondary = new SolidColorBrush(Color.FromRgb(134, 142, 150)),
            TextDisabled = new SolidColorBrush(Color.FromRgb(173, 181, 189)),
            TextLink = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            TextLinkHover = new SolidColorBrush(Color.FromRgb(0, 110, 230)),

            OverlayBackground = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
            TooltipBackground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
            TooltipForeground = new SolidColorBrush(Colors.White),
                
            DashboardCardBackground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            DashboardCardHover = new SolidColorBrush(Color.FromRgb(228, 229, 230)),
            DashboardCardHoverDefault = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
            DashboardErrorText = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            DashboardMutedText = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            DashboardAccentBlue = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
            DashboardServiceYellow = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            DashboardFooterBackground = new SolidColorBrush(Color.FromArgb(50, 108, 117, 125)),

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

        /// <summary>
        /// Current theme type (Dark, Light, or HighContrast).
        /// </summary>
        private static ThemeType _currentThemeType;
        
        /// <summary>
        /// Gets or sets the current theme type.
        /// Setting this property triggers the theme changed event.
        /// </summary>
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
        /// Applies a theme in real-time, fires change event, and provides fallback on error.
        /// </summary>
        /// <param name="themeType">The theme type to apply.</param>
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
            }
            catch (Exception ex)
            {
                logMessage += $"[ApplyTheme] Failed to apply theme: {ex.Message}\n";
                System.Diagnostics.Debug.WriteLine(logMessage);
                _currentThemeType = ThemeType.Dark;
                OnThemeChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets the current theme colors based on CurrentThemeType.
        /// Falls back to DarkTheme if the type is unrecognized.
        /// </summary>
        public static ThemeColors CurrentTheme => CurrentThemeType switch
        {
            ThemeType.Dark => DarkTheme,
            ThemeType.Light => LightTheme,
            ThemeType.HighContrast => DarkTheme,
            _ => DarkTheme
        };

        /// <summary>
        /// Global animation settings for all theme animations.
        /// </summary>
        public static readonly ThemeAnimationSettings AnimationSettings = new();

        /// <summary>
        /// Event fired when the theme is changed.
        /// Subscribe to this to update UI elements dynamically.
        /// </summary>
        public static event Action? OnThemeChanged;
        #endregion

        #region Animation Helpers
        /// <summary>
        /// Creates a fade-in animation for UI elements.
        /// </summary>
        /// <param name="element">The element to animate.</param>
        /// <param name="from">Starting opacity value (default 0).</param>
        /// <param name="to">Target opacity value (default 1).</param>
        /// <param name="duration">Animation duration (uses AnimationSettings.FadeInDuration if null).</param>
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
        /// Creates a fade-out animation for UI elements.
        /// </summary>
        /// <param name="element">The element to animate.</param>
        /// <param name="from">Starting opacity value (default 1).</param>
        /// <param name="to">Target opacity value (default 0).</param>
        /// <param name="duration">Animation duration (uses AnimationSettings.FadeOutDuration if null).</param>
        /// <param name="onCompleted">Optional callback when animation completes.</param>
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
        /// Creates a slide-in animation for framework elements.
        /// </summary>
        /// <param name="element">The element to animate.</param>
        /// <param name="fromX">Starting horizontal position.</param>
        /// <param name="toX">Target horizontal position.</param>
        /// <param name="duration">Animation duration (uses AnimationSettings.SlideInDuration if null).</param>
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
        /// Creates a hover animation for buttons with scale effect.
        /// </summary>
        /// <param name="button">The button to animate.</param>
        /// <param name="isEntering">True for mouse enter (scale up), false for mouse leave (scale down).</param>
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
        /// Applies a glow effect to UI elements using DropShadowEffect.
        /// </summary>
        /// <param name="element">The element to apply glow to.</param>
        /// <param name="color">Glow color.</param>
        /// <param name="radius">Blur radius for glow effect (default 10).</param>
        /// <param name="opacity">Glow opacity (default 0.8).</param>
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
        /// Removes all visual effects from a UI element.
        /// </summary>
        /// <param name="element">The element to clear effects from.</param>
        public static void RemoveEffects(UIElement element)
        {
            element.Effect = null;
        }

        /// <summary>
        /// Darkens a SolidColorBrush by a specific percentage.
        /// </summary>
        /// <param name="brush">The brush to darken.</param>
        /// <param name="percentage">Percentage to darken (0.0 to 1.0).</param>
        /// <returns>New darkened SolidColorBrush.</returns>
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
        /// Lightens a SolidColorBrush by a specific percentage.
        /// </summary>
        /// <param name="brush">The brush to lighten.</param>
        /// <param name="percentage">Percentage to lighten (0.0 to 1.0).</param>
        /// <returns>New lightened SolidColorBrush.</returns>
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
        /// Applies dark theme styling to a DataGrid with modern look and optimized performance.
        /// Configures headers, rows, cells, alternating colors, hover effects, and selection styling.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to style.</param>
        public static void SetDataGridDarkTheme(DataGrid dataGrid)
        {
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

            dataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;

            dataGrid.Style = null;

            dataGrid.Resources.Clear();

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

            var rowStyle = new Style(typeof(DataGridRow));
            rowStyle.Setters.Add(new Setter(DataGridRow.MinHeightProperty, MIN_HEIGHT_ROW));
            rowStyle.Setters.Add(new Setter(DataGridRow.FontSizeProperty, FONT_SIZE_ROW));
            rowStyle.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.GridBackground));
            rowStyle.Setters.Add(new Setter(DataGridRow.ForegroundProperty, CurrentTheme.GridForeground));

            var hoverTrigger = new Trigger
            {
                Property = DataGridRow.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.SidebarHover));

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

            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(8, 6, 8, 6)));
            cellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)));
            cellStyle.Setters.Add(new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            cellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent));
            cellStyle.Setters.Add(new Setter(DataGridCell.ForegroundProperty, CurrentTheme.GridForeground));

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
        /// Creates a styled button with theme-based colors, animations, and visual effects.
        /// Supports multiple button styles (Primary, Secondary, Success, Danger, Warning, Info).
        /// </summary>
        /// <param name="content">Button text content.</param>
        /// <param name="clickHandler">Optional click event handler.</param>
        /// <param name="style">Button style type (default Primary).</param>
        /// <returns>Fully styled Button with animations and shadow effects.</returns>
        public static Button CreateStyledButton(string content, RoutedEventHandler? clickHandler = null, ButtonStyle style = ButtonStyle.Primary)
        {
            var button = new Button
            {
                Content = content
            };

            if (clickHandler != null)
                button.Click += clickHandler;

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
                default:
                    backgroundColor = CurrentTheme.ButtonBackground;
                    hoverColor = CurrentTheme.ButtonHover;
                    pressedColor = CurrentTheme.ButtonPressed;
                    borderColor = DarkenColor(CurrentTheme.ButtonBackground, COLOR_DARKEN_FACTOR);
                    borderHoverColor = DarkenColor(CurrentTheme.ButtonHover, COLOR_DARKEN_FACTOR);
                    break;
            }

            var buttonStyle = new Style(typeof(Button));

            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, backgroundColor));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, CurrentTheme.ButtonForeground));
            buttonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, borderColor));
            buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            buttonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(0)));
            buttonStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Medium));
            buttonStyle.Setters.Add(new Setter(Button.FontSizeProperty, FONT_SIZE_STANDARD));
            buttonStyle.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));

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

            button.MouseEnter += (s, e) => AnimateButtonHover(button, true);
            button.MouseLeave += (s, e) => AnimateButtonHover(button, false);

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
        /// Button style enumeration for CreateStyledButton method.
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
        /// Creates a styled TextBox with dark theme support.
        /// Supports both regular and console modes with custom styling and focus effects.
        /// </summary>
        /// <param name="isConsole">True for console-style TextBox with monospace font, false for regular input.</param>
        /// <returns>Fully styled TextBox with custom scrollbar and focus effects.</returns>
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

            var textBoxStyle = new Style(typeof(TextBox));

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
        /// Creates a styled ComboBox with dark theme support.
        /// Includes custom dropdown styling, hover effects, and automatic scroll-to-top on open.
        /// </summary>
        /// <returns>Fully styled ComboBox with custom item containers and dropdown behavior.</returns>
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

            comboBox.DropDownOpened += (s, e) =>
            {
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

            var comboStyle = new Style(typeof(ComboBox));

            comboStyle.Setters.Add(new Setter(ComboBox.BackgroundProperty, CurrentTheme.InputBackground));
            comboStyle.Setters.Add(new Setter(ComboBox.ForegroundProperty, CurrentTheme.InputForeground));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputBorder));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(1)));
            comboStyle.Setters.Add(new Setter(ComboBox.PaddingProperty, new Thickness(PADDING_COMBOBOX_HORIZONTAL, PADDING_COMBOBOX_VERTICAL, PADDING_COMBOBOX_HORIZONTAL, PADDING_COMBOBOX_VERTICAL)));
            comboStyle.Setters.Add(new Setter(ComboBox.FontSizeProperty, FONT_SIZE_STANDARD));
            comboStyle.Setters.Add(new Setter(ComboBox.MinHeightProperty, MIN_HEIGHT_COMBOBOX));

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

            var itemStyle = new Style(typeof(ComboBoxItem));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.DropdownBackground));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, CurrentTheme.InputForeground));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.PaddingProperty, new Thickness(PADDING_COMBOBOX_ITEM_HORIZONTAL, PADDING_COMBOBOX_ITEM_VERTICAL, PADDING_COMBOBOX_ITEM_HORIZONTAL, PADDING_COMBOBOX_ITEM_VERTICAL)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BorderThicknessProperty, new Thickness(0)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.FontSizeProperty, FONT_SIZE_STANDARD));

            var itemHoverTrigger = new Trigger
            {
                Property = ComboBoxItem.IsMouseOverProperty,
                Value = true
            };
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.SidebarHover));
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, CurrentTheme.InputForeground));

            var itemSelectedTrigger = new Trigger
            {
                Property = ComboBoxItem.IsSelectedProperty,
                Value = true
            };
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.ButtonBackground));
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));

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

            comboBox.Resources.Clear();
            comboBox.Resources.Add(SystemColors.WindowBrushKey, CurrentTheme.DropdownBackground);
            comboBox.Resources.Add(SystemColors.ControlBrushKey, CurrentTheme.InputBackground);
            comboBox.Resources.Add(SystemColors.ControlTextBrushKey, CurrentTheme.InputForeground);
            comboBox.Resources.Add(SystemColors.HighlightBrushKey, CurrentTheme.ButtonBackground);
            comboBox.Resources.Add(SystemColors.HighlightTextBrushKey, new SolidColorBrush(Colors.White));

            return comboBox;
        }

        /// <summary>
        /// Creates a styled ToolTip with dark theme support and rounded corners.
        /// </summary>
        /// <param name="content">Tooltip text content.</param>
        /// <returns>Styled ToolTip with custom appearance.</returns>
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
        /// Adds a styled tooltip to any framework element.
        /// </summary>
        /// <param name="element">The element to attach tooltip to.</param>
        /// <param name="content">Tooltip text content.</param>
        public static void AddToolTip(FrameworkElement element, string content)
        {
            element.ToolTip = CreateStyledToolTip(content);
        }

    /// <summary>
    /// Creates a styled progress bar with smooth animations and customizable appearance.
    /// </summary>
    /// <param name="value">Initial progress value (default 0).</param>
    /// <param name="maximum">Maximum progress value (default 100).</param>
    /// <param name="isIndeterminate">True for indeterminate (animated) progress bar.</param>
    /// <param name="animateValueChanges">True (default) to animate value changes smoothly over 1.2 seconds.</param>
    /// <returns>Styled ProgressBar with custom indicator and optional animations.</returns>
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
                progressBar.ApplyTemplate();
                var track = progressBar.Template.FindName("PART_Track", progressBar) as Border;
                var indicator = progressBar.Template.FindName("PART_Indicator", progressBar) as Rectangle;

                if (track == null || indicator == null)
                    return;

                double? pendingValueForAnimation = null;

                void UpdateIndicator(double v, bool animate)
                {
                    if (progressBar.Maximum <= 0)
                        return;

                    if (track.ActualWidth <= 0)
                    {
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

                if (queuedValueBeforeLoaded.HasValue && animateValueChanges)
                {
                    UpdateIndicator(queuedValueBeforeLoaded.Value, true);
                    queuedValueBeforeLoaded = null;
                    hasAnimatedInitialValue = true;
                }
                else
                {
                    UpdateIndicator(progressBar.Value, false);
                }

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

                progressBar.ValueChanged += (o, args) =>
                {
                    if (progressBar.IsIndeterminate)
                        return;
                    
                    bool shouldAnimate = animateValueChanges;
                    if (!hasAnimatedInitialValue && Math.Abs(args.NewValue - initialValue) > 0.001)
                    {
                        hasAnimatedInitialValue = true;
                        shouldAnimate = animateValueChanges;
                    }
                    
                    UpdateIndicator(args.NewValue, shouldAnimate);
                };
            };
            return progressBar;
        }

        /// <summary>
        /// Creates a styled visual separator (horizontal or vertical line).
        /// </summary>
        /// <param name="orientation">Separator orientation (Horizontal or Vertical).</param>
        /// <returns>Styled Separator with appropriate sizing and margins.</returns>
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
        /// Creates a modern notification panel with icon and styled appearance.
        /// </summary>
        /// <param name="message">Notification message text.</param>
        /// <param name="type">Notification type (Info, Success, Warning, Error).</param>
        /// <param name="showIcon">True to show type-specific icon.</param>
        /// <returns>Styled Border containing the notification UI with fade-in animation.</returns>
        public static Border CreateNotificationPanel(string message, NotificationType type = NotificationType.Info, bool showIcon = true)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(PADDING_CARD_HORIZONTAL, PADDING_CARD_VERTICAL, PADDING_CARD_HORIZONTAL, PADDING_CARD_VERTICAL),
                Margin = new Thickness(0, 4, 0, 4)
            };

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
                default:
                    border.Background = CurrentTheme.InfoBackground;
                    border.BorderBrush = CurrentTheme.Info;
                    break;
            }

            border.BorderThickness = new Thickness(1, 1, 1, 1);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

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

            AnimateFadeIn(border);

            return border;
        }

        /// <summary>
        /// Notification type enumeration for CreateNotificationPanel method.
        /// </summary>
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error
        }

        /// <summary>
        /// Creates a styled toggle switch checkbox with animated switch appearance.
        /// </summary>
        /// <param name="content">Switch label text.</param>
        /// <param name="isChecked">Initial checked state (default false).</param>
        /// <returns>Styled CheckBox with toggle switch appearance and animations.</returns>
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

            var style = new Style(typeof(CheckBox));
            var template = new ControlTemplate(typeof(CheckBox));

            var grid = new FrameworkElementFactory(typeof(Grid));
            
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

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

            var thumb = new FrameworkElementFactory(typeof(Ellipse));
            thumb.Name = "SwitchThumb";
            thumb.SetValue(Ellipse.WidthProperty, 18.0);
            thumb.SetValue(Ellipse.HeightProperty, 18.0);
            thumb.SetValue(Ellipse.FillProperty, CurrentTheme.ButtonForeground);
            thumb.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            thumb.SetValue(FrameworkElement.MarginProperty, new Thickness(3, 0, 0, 0));

            var translateTransform = new FrameworkElementFactory(typeof(TranslateTransform));
            thumb.SetValue(UIElement.RenderTransformProperty, translateTransform);

            track.AppendChild(thumb);

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(Grid.ColumnProperty, 1);
            contentPresenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            grid.AppendChild(track);
            grid.AppendChild(contentPresenter);
            template.VisualTree = grid;

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
        /// Creates a styled Label with theme-based typography.
        /// Supports multiple label styles (Normal, Title, Subtitle, Muted, Secondary, Link).
        /// </summary>
        /// <param name="content">Label text content.</param>
        /// <param name="isTitle">Legacy parameter for title styling (deprecated, use style parameter).</param>
        /// <param name="isMuted">Legacy parameter for muted styling (deprecated, use style parameter).</param>
        /// <param name="style">Label style type (default Normal).</param>
        /// <returns>Styled Label with appropriate typography.</returns>
        public static Label CreateStyledLabel(string content, bool isTitle = false, bool isMuted = false, LabelStyle style = LabelStyle.Normal)
        {
            var label = new Label
            {
                Content = content
            };

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
                    
                    label.MouseEnter += (s, e) => {
                        label.Foreground = CurrentTheme.TextLinkHover;
                    };
                    label.MouseLeave += (s, e) => {
                        label.Foreground = CurrentTheme.TextLink;
                    };
                    break;
                default:
                    label.FontWeight = FontWeights.Normal;
                    label.FontSize = 14;
                    label.Foreground = CurrentTheme.Foreground;
                    break;
            }

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
        /// Label style enumeration for CreateStyledLabel method.
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
        /// Displays a styled message box with dark theme and custom appearance.
        /// </summary>
        /// <param name="message">Message text to display.</param>
        /// <param name="title">Dialog title (uses localized default if null).</param>
        /// <param name="buttons">Button configuration (OK, OKCancel, YesNo, YesNoCancel).</param>
        /// <param name="icon">Icon type to display (None, Information, Warning, Error, Question).</param>
        /// <returns>MessageBoxResult indicating which button was clicked.</returns>
        public static MessageBoxResult CreateStyledMessageBox(string message, string? title = null, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            title ??= LocalizationManager.Instance?.GetString("common.dialogs.default_title") ?? "Mensagem";
            
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                Background = Brushes.Transparent,
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
        /// Creates a styled CheckBox with dark theme support and modern appearance.
        /// </summary>
        /// <param name="content">Checkbox label text.</param>
        /// <param name="isBold">True to use bold font weight.</param>
        /// <returns>Fully styled CheckBox with custom check mark and hover effects.</returns>
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

            var template = new ControlTemplate(typeof(CheckBox));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "Border";

            var panel = new FrameworkElementFactory(typeof(StackPanel));
            panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            panel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

            var boxBorder = new FrameworkElementFactory(typeof(Border));
            boxBorder.Name = "CheckBoxBox";
            boxBorder.SetValue(Border.WidthProperty, 18.0);
            boxBorder.SetValue(Border.HeightProperty, 18.0);
            boxBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            boxBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            boxBorder.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(CheckBox.BorderBrushProperty));
            boxBorder.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            boxBorder.SetValue(Border.MarginProperty, new Thickness(0, 0, 8, 0));
            boxBorder.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            boxBorder.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);

            var checkPath = new FrameworkElementFactory(typeof(Path));
            checkPath.Name = "CheckMark";
            checkPath.SetValue(Path.DataProperty, Geometry.Parse("M 3 7 L 6 12 L 13 5"));
            checkPath.SetValue(Path.StrokeThicknessProperty, 2.0);
            checkPath.SetValue(Path.StrokeProperty, Brushes.White);
            checkPath.SetValue(Path.SnapsToDevicePixelsProperty, true);
            checkPath.SetValue(Path.StrokeEndLineCapProperty, PenLineCap.Round);
            checkPath.SetValue(Path.StrokeStartLineCapProperty, PenLineCap.Round);
            checkPath.SetValue(FrameworkElement.VisibilityProperty, Visibility.Collapsed);

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);

            boxBorder.AppendChild(checkPath);
            panel.AppendChild(boxBorder);
            panel.AppendChild(contentPresenter);
            border.AppendChild(panel);
            template.VisualTree = border;

            var hoverTrigger = new Trigger { Property = CheckBox.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover, "CheckBoxBox"));
            hoverTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));

            var focusTrigger = new Trigger { Property = CheckBox.IsFocusedProperty, Value = true };
            focusTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.InputFocusBorder, "CheckBoxBox"));

            var checkedTrigger = new Trigger { Property = CheckBox.IsCheckedProperty, Value = true };
            checkedTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "CheckMark"));
            checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.ButtonBackground, "CheckBoxBox"));
            checkedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.ButtonBackground, "CheckBoxBox"));

            var disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
            disabledTrigger.Setters.Add(new Setter(UIElement.OpacityProperty, 0.6));

            template.Triggers.Add(hoverTrigger);
            template.Triggers.Add(focusTrigger);
            template.Triggers.Add(checkedTrigger);
            template.Triggers.Add(disabledTrigger);

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
        /// Applies custom dark theme styling to a sidebar ListBox.
        /// Configures background, hover effects, selection styling, and right border indicator.
        /// </summary>
        /// <param name="listBox">The ListBox to style as sidebar.</param>
        public static void ApplySidebarListBoxTheme(ListBox listBox)
        {
            listBox.Background = CurrentTheme.SidebarBackground;
            listBox.Foreground = CurrentTheme.Foreground;
            listBox.BorderBrush = CurrentTheme.Border;
            listBox.BorderThickness = new Thickness(0);
            ScrollViewer.SetCanContentScroll(listBox, false);

            var itemTemplate = new ControlTemplate(typeof(ListBoxItem));

            var mainBorder = new FrameworkElementFactory(typeof(Border));
            mainBorder.Name = "MainBorder";
            mainBorder.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            mainBorder.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 4, 0));
            mainBorder.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            mainBorder.SetValue(Border.PaddingProperty, new Thickness(15, 12, 15, 12));
            mainBorder.SetValue(Border.MarginProperty, new Thickness(4, 2, 4, 2));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            mainBorder.AppendChild(contentPresenter);
            itemTemplate.VisualTree = mainBorder;

            var hoverTrigger = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarHover) { TargetName = "MainBorder" });
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover) { TargetName = "MainBorder" });
            itemTemplate.Triggers.Add(hoverTrigger);

            var selectedTrigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarSelected) { TargetName = "MainBorder" });
            selectedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderFocus) { TargetName = "MainBorder" });
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.FontWeightProperty, FontWeights.SemiBold));
            itemTemplate.Triggers.Add(selectedTrigger);

            var selectedHoverTrigger = new MultiTrigger();
            selectedHoverTrigger.Conditions.Add(new Condition(ListBoxItem.IsSelectedProperty, true));
            selectedHoverTrigger.Conditions.Add(new Condition(ListBoxItem.IsMouseOverProperty, true));
            selectedHoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarHover) { TargetName = "MainBorder" });
            selectedHoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover) { TargetName = "MainBorder" });
            selectedHoverTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));
            selectedHoverTrigger.Setters.Add(new Setter(ListBoxItem.FontWeightProperty, FontWeights.SemiBold));
            itemTemplate.Triggers.Add(selectedHoverTrigger);

            var itemStyle = new Style(typeof(ListBoxItem));
            itemStyle.Setters.Add(new Setter(ListBoxItem.TemplateProperty, itemTemplate));
            itemStyle.Setters.Add(new Setter(ListBoxItem.FontSizeProperty, 14.0));
            itemStyle.Setters.Add(new Setter(ListBoxItem.CursorProperty, Cursors.Hand));
            itemStyle.Setters.Add(new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            itemStyle.Setters.Add(new Setter(ListBoxItem.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            listBox.ItemContainerStyle = itemStyle;
        }
        
        /// <summary>
        /// Creates a loading overlay with centered animated spinner.
        /// </summary>
        /// <returns>Border containing semi-transparent overlay with spinner, initially collapsed.</returns>
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
        /// Creates an animated circular spinner for loading indicators with customizable size.
        /// </summary>
        /// <param name="width">Spinner width in pixels.</param>
        /// <param name="height">Spinner height in pixels.</param>
        /// <returns>Canvas containing animated spinner with rotating dots.</returns>
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
        /// Applies the current theme to a window and all its child controls recursively.
        /// </summary>
        /// <param name="window">The window to apply theme to.</param>
        public static void ApplyThemeToWindow(Window window)
        {
            window.Background = CurrentTheme.FormBackground;
            window.Foreground = CurrentTheme.Foreground;
            
            ApplyThemeToContainer(window);
        }

        /// <summary>
        /// Recursively applies the current theme to all child controls within a container.
        /// Handles DataGrid, TextBox, ComboBox, and Label controls with appropriate styling.
        /// </summary>
        /// <param name="container">The parent container to apply theme to.</param>
        public static void ApplyThemeToContainer(DependencyObject container)
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(container);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(container, i);
                
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
                
                ApplyThemeToContainer(child);
            }
        }

        /// <summary>
        /// Switches the current theme at runtime and applies it to all open windows.
        /// </summary>
        /// <param name="newTheme">The theme type to switch to.</param>
        public static void SwitchTheme(ThemeType newTheme)
        {
            CurrentThemeType = newTheme;
            
            foreach (Window window in Application.Current.Windows)
            {
                ApplyThemeToWindow(window);
            }
        }

        /// <summary>
        /// Creates a styled card container with optional shadow effect.
        /// </summary>
        /// <param name="content">The UI element to place inside the card.</param>
        /// <param name="cornerRadius">Border corner radius in pixels (default 8).</param>
        /// <param name="hasShadow">True to apply drop shadow effect (default true).</param>
        /// <returns>Styled Border containing the content with card appearance.</returns>
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
        /// Creates a ScrollViewer with custom styled scrollbar already applied.
        /// </summary>
        /// <param name="verticalVisibility">Vertical scrollbar visibility mode (default Auto).</param>
        /// <param name="horizontalVisibility">Horizontal scrollbar visibility mode (default Auto).</param>
        /// <returns>Styled ScrollViewer with custom dark theme scrollbars.</returns>
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
        /// Applies custom dark theme scrollbar to a ScrollViewer with overlay appearance.
        /// Creates slim scrollbars that appear on top of content without taking layout space.
        /// </summary>
        /// <param name="scrollViewer">The ScrollViewer to apply custom scrollbar to.</param>
        public static void ApplyCustomScrollbar(ScrollViewer scrollViewer)
        {
            var scrollViewerStyle = new Style(typeof(ScrollViewer));
            
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
                scrollViewer.Background = CurrentTheme.PanelBackground;
            }
        }

        /// <summary>
        /// Returns formatted information about the current theme.
        /// </summary>
        /// <returns>Multi-line string with theme details (type, colors, dark mode status).</returns>
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
