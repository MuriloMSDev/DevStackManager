

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Multi-tab console panel component for operation output display.
    /// Each tab (Install, Uninstall, Sites, Config) maintains its own persistent message buffer.
    /// Supports thread-safe message appending from background operations with automatic UI updates.
    /// Provides Console.Out redirection for capturing all console output.
    /// </summary>
    public static class GuiConsolePanel
    {
        /// <summary>
        /// Console tab types for different operation categories.
        /// </summary>
        public enum ConsoleTab { Install, Uninstall, Sites, Config }

        /// <summary>
        /// Panel margin in pixels.
        /// </summary>
        private const double PANEL_MARGIN = 10;
        
        /// <summary>
        /// Font size for title text.
        /// </summary>
        private const double TITLE_FONT_SIZE = 18;
        
        /// <summary>
        /// Bottom margin for title in pixels.
        /// </summary>
        private const double TITLE_MARGIN_BOTTOM = 10;
        
        /// <summary>
        /// Height of output text box in pixels.
        /// </summary>
        private const double OUTPUT_BOX_HEIGHT = 600;
        
        /// <summary>
        /// Font size for output text.
        /// </summary>
        private const double OUTPUT_BOX_FONT_SIZE = 12;
        
        /// <summary>
        /// Height of clear button in pixels.
        /// </summary>
        private const double CLEAR_BUTTON_HEIGHT = 35;
        
        /// <summary>
        /// Top margin for clear button in pixels.
        /// </summary>
        private const double CLEAR_BUTTON_MARGIN_TOP = 10;

        /// <summary>
        /// Persistent message buffers for each console tab.
        /// Thread-safe concurrent dictionary storing StringBuilder for each tab type.
        /// </summary>
        private static readonly ConcurrentDictionary<ConsoleTab, StringBuilder> Buffers = new();
        
        /// <summary>
        /// Active TextBox UI elements for each console tab.
        /// Used to update visible output when appending messages.
        /// </summary>
        private static readonly ConcurrentDictionary<ConsoleTab, TextBox> ActiveTextBoxes = new();
        
        /// <summary>
        /// Lock object for thread-safe buffer write operations.
        /// </summary>
        private static readonly object _lock = new();

        static GuiConsolePanel()
        {
            foreach (ConsoleTab tab in Enum.GetValues(typeof(ConsoleTab)))
                Buffers[tab] = new StringBuilder();
        }

        /// <summary>
        /// Creates a console panel for a specific tab.
        /// Panel contains: localized title label, read-only output TextBox (600px height), and Clear button.
        /// Registers TextBox in ActiveTextBoxes for message updates.
        /// </summary>
        /// <param name="tab">Console tab type to create panel for.</param>
        /// <returns>StackPanel containing the complete console panel.</returns>
        public static StackPanel CreateConsolePanel(ConsoleTab tab)
        {
            var panel = new StackPanel { Margin = new Thickness(PANEL_MARGIN) };
            var mainWindow = (DevStackGui)Application.Current.MainWindow;
            
            var titleLabel = CreateConsoleTitleLabel(tab, mainWindow);
            panel.Children.Add(titleLabel);

            var outputBox = CreateOutputTextBox(mainWindow, tab);
            panel.Children.Add(outputBox);

            var clearButton = CreateClearButton(tab, mainWindow);
            panel.Children.Add(clearButton);

            return panel;
        }

        /// <summary>
        /// Creates the console title label with localized text.
        /// </summary>
        /// <param name="tab">Console tab type for title text.</param>
        /// <param name="mainWindow">Main window instance for theme and localization access.</param>
        /// <returns>Styled label with console title.</returns>
        private static Label CreateConsoleTitleLabel(ConsoleTab tab, DevStackGui mainWindow)
        {
            var title = GetTabTitle(tab, mainWindow);
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(title, true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_MARGIN_BOTTOM);
            return titleLabel;
        }

        /// <summary>
        /// Gets the localized title for a console tab.
        /// </summary>
        /// <param name="tab">Console tab type.</param>
        /// <param name="mainWindow">Main window instance for localization access.</param>
        /// <returns>Localized title string.</returns>
        private static string GetTabTitle(ConsoleTab tab, DevStackGui mainWindow)
        {
            return tab switch
            {
                ConsoleTab.Install => mainWindow.LocalizationManager.GetString("gui.console.titles.install"),
                ConsoleTab.Uninstall => mainWindow.LocalizationManager.GetString("gui.console.titles.uninstall"),
                ConsoleTab.Sites => mainWindow.LocalizationManager.GetString("gui.console.titles.sites"),
                ConsoleTab.Config => mainWindow.LocalizationManager.GetString("gui.console.titles.config"),
                _ => mainWindow.LocalizationManager.GetString("gui.console.titles.utilities")
            };
        }

        /// <summary>
        /// Creates the output TextBox for console messages.
        /// Read-only, 600px height, Consolas font, with initial buffer content.
        /// </summary>
        /// <param name="gui">Main window instance for theme access.</param>
        /// <param name="tab">Console tab type for buffer retrieval.</param>
        /// <returns>Styled TextBox with current buffer content.</returns>
        private static TextBox CreateOutputTextBox(DevStackGui gui, ConsoleTab tab)
        {
            var outputBox = DevStackShared.ThemeManager.CreateStyledTextBox(true);
            outputBox.Height = OUTPUT_BOX_HEIGHT;
            outputBox.IsReadOnly = true;
            outputBox.FontSize = OUTPUT_BOX_FONT_SIZE;
            outputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.AcceptsReturn = true;
            outputBox.TextWrapping = TextWrapping.Wrap;
            outputBox.Name = $"ConsoleOutput_{tab}";
            outputBox.Text = Buffers[tab].ToString();
            ActiveTextBoxes[tab] = outputBox;
            return outputBox;
        }

        /// <summary>
        /// Creates the Clear button for console output.
        /// Styled with Danger theme and clears buffer on click.
        /// </summary>
        /// <param name="tab">Console tab type to clear.</param>
        /// <param name="mainWindow">Main window instance for theme and localization access.</param>
        /// <returns>Styled Clear button.</returns>
        private static Button CreateClearButton(ConsoleTab tab, DevStackGui mainWindow)
        {
            var clearButton = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString("gui.console.buttons.clear"), 
                (s, e) => Clear(tab), 
                DevStackShared.ThemeManager.ButtonStyle.Danger);
            clearButton.Height = CLEAR_BUTTON_HEIGHT;
            clearButton.Margin = new Thickness(0, CLEAR_BUTTON_MARGIN_TOP, 0, 0);
            return clearButton;
        }

        /// <summary>
        /// Clears the console buffer for a specific tab.
        /// Removes buffer content and updates UI if TextBox is active.
        /// </summary>
        /// <param name="tab">Console tab type to clear.</param>
        public static void Clear(ConsoleTab tab)
        {
            lock (_lock)
            {
                Buffers[tab].Clear();
                if (ActiveTextBoxes.TryGetValue(tab, out var box))
                {
                    Application.Current?.Dispatcher.Invoke(() => box.Text = "");
                }
            }
        }

        /// <summary>
        /// Appends a message to the console buffer for a specific tab (thread-safe).
        /// Prefixes message with [HH:mm:ss] timestamp.
        /// Updates UI TextBox if active and scrolls to end.
        /// Can be called from any thread - marshals UI updates to Dispatcher.
        /// </summary>
        /// <param name="tab">Console tab type to append to.</param>
        /// <param name="message">Message text to append.</param>
        public static void Append(ConsoleTab tab, string message)
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                Buffers[tab].AppendLine($"[{timestamp}] {message}");
                if (ActiveTextBoxes.TryGetValue(tab, out var box))
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        box.Text = Buffers[tab].ToString();
                        box.ScrollToEnd();
                    });
                }
            }
        }

        /// <summary>
        /// Retrieves the current buffer content for a tab.
        /// Used to restore output when switching between tabs.
        /// </summary>
        /// <param name="tab">Console tab type to get buffer for.</param>
        /// <returns>Current buffer content as string.</returns>
        public static string GetBuffer(ConsoleTab tab)
        {
            lock (_lock)
            {
                return Buffers[tab].ToString();
            }
        }

        /// <summary>
        /// Should be called when switching to a tab to restore its buffer content.
        /// Updates the active TextBox with current buffer and scrolls to end.
        /// </summary>
        /// <param name="tab">Console tab type being activated.</param>
        public static void OnTabActivated(ConsoleTab tab)
        {
            if (ActiveTextBoxes.TryGetValue(tab, out var box))
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    box.Text = Buffers[tab].ToString();
                    box.ScrollToEnd();
                });
            }
        }

        /// <summary>
        /// Executes an action with console output redirection to a specific tab.
        /// Redirects Console.Out to send all output to the tab's buffer, even in background.
        /// Restores original Console.Out after execution.
        /// </summary>
        /// <param name="tab">Console tab type to send output to.</param>
        /// <param name="action">Async action to execute.</param>
        /// <returns>Task representing the async operation.</returns>
        public static async Task RunWithConsoleOutput(ConsoleTab tab, Func<IProgress<string>, Task> action)
        {
            var progress = new Progress<string>(msg => Append(tab, msg));
            var originalOut = Console.Out;
            try
            {
                using (var writer = new ProgressTextWriter(progress))
                {
                    Console.SetOut(writer);
                    await action(progress);
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        /// <summary>
        /// Custom TextWriter that redirects Console.Out to progress reporting.
        /// Sends all written text to IProgress for appending to console buffer.
        /// </summary>
        private class ProgressTextWriter : System.IO.TextWriter
        {
            /// <summary>
            /// Progress reporter for sending console output to buffer.
            /// </summary>
            private readonly IProgress<string> _progress;
            
            /// <summary>
            /// Initializes a new instance of ProgressTextWriter with progress reporting.
            /// </summary>
            /// <param name="progress">Progress reporter for console output.</param>
            public ProgressTextWriter(IProgress<string> progress) { _progress = progress; }
            /// <summary>
            /// Gets the character encoding used for console output (UTF-8).
            /// </summary>
            public override Encoding Encoding => Encoding.UTF8;
            /// <summary>
            /// Writes a line to the console output.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public override void WriteLine(string? value) => _progress.Report(value ?? "");
            /// <summary>
            /// Writes text to the console output.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public override void Write(string? value) => _progress.Report(value ?? "");
        }
    }
}
