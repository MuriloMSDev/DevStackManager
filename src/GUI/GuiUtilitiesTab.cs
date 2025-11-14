using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Utilities tab component for executing DevStack commands and viewing console output.
    /// Provides interactive console interface with command input, quick action buttons, and real-time output display.
    /// Features: command execution (status, list, doctor, test, help), clear console button overlay,
    /// direct CLI integration via DevStack.exe or fallback to C# managers.
    /// </summary>
    public static class GuiUtilitiesTab
    {
        /// <summary>
        /// Font size for header text.
        /// </summary>
        private const int HEADER_FONT_SIZE = 18;
        
        /// <summary>
        /// Margin for header element.
        /// </summary>
        private const int HEADER_MARGIN = 10;
        
        /// <summary>
        /// Size of the clear console button.
        /// </summary>
        private const int CLEAR_BUTTON_SIZE = 32;
        
        /// <summary>
        /// Top margin for clear button.
        /// </summary>
        private const int CLEAR_BUTTON_TOP_MARGIN = 1;
        
        /// <summary>
        /// Right margin for clear button when scrollbar is not visible.
        /// </summary>
        private const int CLEAR_BUTTON_RIGHT_MARGIN_DEFAULT = 6;
        
        /// <summary>
        /// Right margin for clear button when scrollbar is visible.
        /// </summary>
        private const int CLEAR_BUTTON_RIGHT_MARGIN_WITH_SCROLLBAR = 23;
        
        /// <summary>
        /// Z-index for clear button to ensure it appears above console output.
        /// </summary>
        private const int CLEAR_BUTTON_Z_INDEX = 10;
        
        /// <summary>
        /// Width of command label.
        /// </summary>
        private const int COMMAND_LABEL_WIDTH = 90;
        
        /// <summary>
        /// Height of command input field.
        /// </summary>
        private const int COMMAND_INPUT_HEIGHT = 30;
        
        /// <summary>
        /// Horizontal margin for command input.
        /// </summary>
        private const int COMMAND_INPUT_HORIZONTAL_MARGIN = 5;
        
        /// <summary>
        /// Width of execute button.
        /// </summary>
        private const int EXECUTE_BUTTON_WIDTH = 100;
        
        /// <summary>
        /// Height of execute button.
        /// </summary>
        private const int EXECUTE_BUTTON_HEIGHT = 30;
        
        /// <summary>
        /// Left margin for execute button.
        /// </summary>
        private const int EXECUTE_BUTTON_LEFT_MARGIN = 5;
        
        /// <summary>
        /// Horizontal padding for execute button.
        /// </summary>
        private const int EXECUTE_BUTTON_HORIZONTAL_PADDING = 12;
        
        /// <summary>
        /// Vertical padding for execute button.
        /// </summary>
        private const int EXECUTE_BUTTON_VERTICAL_PADDING = 4;
        
        /// <summary>
        /// Font size for console output text.
        /// </summary>
        private const int CONSOLE_FONT_SIZE = 12;
        
        /// <summary>
        /// Margin for console container.
        /// </summary>
        private const int CONSOLE_MARGIN = 5;
        
        /// <summary>
        /// Horizontal margin for console output.
        /// </summary>
        private const int CONSOLE_HORIZONTAL_MARGIN = 10;
        
        /// <summary>
        /// Width of quick action buttons.
        /// </summary>
        private const int QUICK_BUTTON_WIDTH = 120;
        
        /// <summary>
        /// Height of quick action buttons.
        /// </summary>
        private const int QUICK_BUTTON_HEIGHT = 35;
        
        /// <summary>
        /// Margin between quick action buttons.
        /// </summary>
        private const int QUICK_BUTTON_MARGIN = 5;
        
        /// <summary>
        /// Top margin for buttons panel.
        /// </summary>
        private const int BUTTONS_PANEL_TOP_MARGIN = 10;
        
        /// <summary>
        /// Margin for input panel.
        /// </summary>
        private const int INPUT_PANEL_MARGIN = 10;
        
        /// <summary>
        /// Top margin for input panel.
        /// </summary>
        private const int INPUT_PANEL_TOP_MARGIN = 5;
        
        /// <summary>
        /// Bottom margin for input panel.
        /// </summary>
        private const int INPUT_PANEL_BOTTOM_MARGIN = 10;

        /// <summary>
        /// Font family for console output (monospaced).
        /// </summary>
        private const string CONSOLE_FONT_FAMILY = "Consolas";

        /// <summary>
        /// Creates the complete Utilities tab content with console interface.
        /// Four-row layout: header (title), quick buttons panel, console output with clear button overlay, command input panel.
        /// Console output displays command results with read-only Consolas font text.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and data binding.</param>
        /// <returns>Grid with utilities console interface.</returns>
        public static Grid CreateUtilitiesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.utilities_tab.console_title"), true);
            headerLabel.FontSize = HEADER_FONT_SIZE;
            headerLabel.Margin = new Thickness(HEADER_MARGIN, HEADER_MARGIN, HEADER_MARGIN, 0);
            Grid.SetRow(headerLabel, 0);
            grid.Children.Add(headerLabel);

            var inputPanel = CreateCommandInputPanel(mainWindow);
            Grid.SetRow(inputPanel, 3);
            grid.Children.Add(inputPanel);

            var consoleGrid = new Grid();

            var consoleScrollViewer = CreateConsoleScrollViewer(mainWindow);
            consoleGrid.Children.Add(consoleScrollViewer);

            var clearButton = CreateClearButton(mainWindow);
            clearButton.Click += (s, e) => ClearConsole(mainWindow);
            Panel.SetZIndex(clearButton, CLEAR_BUTTON_Z_INDEX);
            consoleGrid.Children.Add(clearButton);

            SetupClearButtonMarginAdjustment(consoleScrollViewer, clearButton);

            Grid.SetRow(consoleGrid, 2);
            grid.Children.Add(consoleGrid);

            var buttonsPanel = CreateQuickButtonsPanel(mainWindow, includeClearButton: false);
            Grid.SetRow(buttonsPanel, 1);
            grid.Children.Add(buttonsPanel);

            return grid;
        }

        /// <summary>
        /// Creates the command input panel with label, TextBox, and execute button.
        /// TextBox supports Enter key to execute commands.
        /// Three-column layout: label (auto) | TextBox (star) | execute button (auto).
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>Grid with command input controls.</returns>
        private static Grid CreateCommandInputPanel(DevStackGui mainWindow)
        {
            var inputPanel = new Grid
            {
                Margin = new Thickness(INPUT_PANEL_MARGIN, INPUT_PANEL_TOP_MARGIN, INPUT_PANEL_MARGIN, INPUT_PANEL_BOTTOM_MARGIN)
            };
            
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var commandLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.utilities_tab.command_label"));
            commandLabel.Width = COMMAND_LABEL_WIDTH;
            commandLabel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(commandLabel, 0);
            inputPanel.Children.Add(commandLabel);

            var commandTextBox = CreateCommandTextBox(mainWindow);
            Grid.SetColumn(commandTextBox, 1);
            inputPanel.Children.Add(commandTextBox);

            var executeButton = CreateExecuteButton(mainWindow, commandTextBox);
            Grid.SetColumn(executeButton, 2);
            inputPanel.Children.Add(executeButton);

            return inputPanel;
        }

        /// <summary>
        /// Creates the command input TextBox with Enter key execution support.
        /// Named "UtilsCommandTextBox" for UI element lookup.
        /// </summary>
        /// <param name="mainWindow">Main window instance for styling.</param>
        /// <returns>TextBox for command input.</returns>
        private static TextBox CreateCommandTextBox(DevStackGui mainWindow)
        {
            var commandTextBox = DevStackShared.ThemeManager.CreateStyledTextBox();
            commandTextBox.Height = COMMAND_INPUT_HEIGHT;
            commandTextBox.Margin = new Thickness(COMMAND_INPUT_HORIZONTAL_MARGIN, 0, COMMAND_INPUT_HORIZONTAL_MARGIN, 0);
            commandTextBox.Name = "UtilsCommandTextBox";
            
            commandTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    ExecuteCommand(mainWindow, commandTextBox.Text);
                    commandTextBox.Text = "";
                }
            };
            
            return commandTextBox;
        }

        /// <summary>
        /// Creates the execute button with Success style.
        /// Executes command from TextBox and clears input after execution.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <param name="commandTextBox">TextBox containing the command to execute.</param>
        /// <returns>Execute button with click handler.</returns>
        private static Button CreateExecuteButton(DevStackGui mainWindow, TextBox commandTextBox)
        {
            var executeButton = DevStackShared.ThemeManager.CreateStyledButton(
                mainWindow.LocalizationManager.GetString("gui.utilities_tab.execute_button"), 
                (s, e) =>
                {
                    ExecuteCommand(mainWindow, commandTextBox.Text);
                    commandTextBox.Text = "";
                }, 
                DevStackShared.ThemeManager.ButtonStyle.Success);
            
            executeButton.Width = EXECUTE_BUTTON_WIDTH;
            executeButton.Height = EXECUTE_BUTTON_HEIGHT;
            executeButton.Margin = new Thickness(EXECUTE_BUTTON_LEFT_MARGIN, 0, 0, 0);
            executeButton.Padding = new Thickness(EXECUTE_BUTTON_HORIZONTAL_PADDING, EXECUTE_BUTTON_VERTICAL_PADDING, EXECUTE_BUTTON_HORIZONTAL_PADDING, EXECUTE_BUTTON_VERTICAL_PADDING);
            
            return executeButton;
        }

        /// <summary>
        /// Creates the clear console button (❌) overlaid on top-right corner of console.
        /// Transparent background with custom ControlTemplate to remove hover/pressed effects.
        /// Z-Index 10 to float above console output.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization and tooltip.</param>
        /// <returns>Clear button with transparent styling.</returns>
        private static Button CreateClearButton(DevStackGui mainWindow)
        {
            var clearButton = new Button
            {
                Content = "❌",
                BorderBrush = Brushes.Transparent,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.Danger,
                Width = CLEAR_BUTTON_SIZE,
                Height = CLEAR_BUTTON_SIZE,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, CLEAR_BUTTON_TOP_MARGIN, CLEAR_BUTTON_RIGHT_MARGIN_DEFAULT, 0),
                ToolTip = mainWindow.LocalizationManager.GetString("gui.utilities_tab.clear_console_tooltip")
            };
            
            var buttonTemplate = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.SnapsToDevicePixelsProperty, true);
            
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentPresenterFactory);
            buttonTemplate.VisualTree = borderFactory;
            
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));
            style.Setters.Add(new Setter(Button.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.Danger));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(0)));
            
            clearButton.Style = style;
            
            return clearButton;
        }

        /// <summary>
        /// Sets up dynamic margin adjustment for clear button based on scrollbar visibility.
        /// Adjusts right margin when vertical scrollbar appears to prevent button overlap.
        /// </summary>
        /// <param name="consoleScrollViewer">ScrollViewer containing console output.</param>
        /// <param name="clearButton">Clear button to adjust margin for.</param>
        private static void SetupClearButtonMarginAdjustment(ScrollViewer consoleScrollViewer, Button clearButton)
        {
            void UpdateClearButtonMargin()
            {
                if (consoleScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    clearButton.Margin = new Thickness(0, CLEAR_BUTTON_TOP_MARGIN, CLEAR_BUTTON_RIGHT_MARGIN_WITH_SCROLLBAR, 0);
                }
                else
                {
                    clearButton.Margin = new Thickness(0, CLEAR_BUTTON_TOP_MARGIN, CLEAR_BUTTON_RIGHT_MARGIN_DEFAULT, 0);
                }
            }
            
            consoleScrollViewer.ScrollChanged += (s, e) => UpdateClearButtonMargin();
            consoleScrollViewer.SizeChanged += (s, e) => UpdateClearButtonMargin();
            consoleScrollViewer.Loaded += (s, e) => UpdateClearButtonMargin();
        }

        /// <summary>
        /// Creates the ScrollViewer for console output display.
        /// Contains read-only TextBox named "UtilsConsoleOutput" with Consolas font and console colors.
        /// </summary>
        /// <param name="mainWindow">Main window instance for initial help text.</param>
        /// <returns>ScrollViewer with console TextBox.</returns>
        private static ScrollViewer CreateConsoleScrollViewer(DevStackGui mainWindow)
        {
            var consoleScrollViewer = new ScrollViewer
            {
                Margin = new Thickness(CONSOLE_HORIZONTAL_MARGIN, CONSOLE_MARGIN, CONSOLE_HORIZONTAL_MARGIN, CONSOLE_MARGIN),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            DevStackShared.ThemeManager.ApplyCustomScrollbar(consoleScrollViewer);

            var consoleTextBox = CreateConsoleTextBox(mainWindow);
            consoleScrollViewer.Content = consoleTextBox;
            
            return consoleScrollViewer;
        }

        /// <summary>
        /// Creates the console output TextBox with initial help text.
        /// Read-only, Consolas font, console theme colors, text wrapping enabled.
        /// Named "UtilsConsoleOutput" for UI element lookup.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>TextBox configured for console output display.</returns>
        private static TextBox CreateConsoleTextBox(DevStackGui mainWindow)
        {
            var consoleTextBox = new TextBox
            {
                FontFamily = new FontFamily(CONSOLE_FONT_FAMILY),
                FontSize = CONSOLE_FONT_SIZE,
                Background = DevStackShared.ThemeManager.CurrentTheme.ConsoleBackground,
                Foreground = DevStackShared.ThemeManager.CurrentTheme.ConsoleForeground,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Name = "UtilsConsoleOutput"
            };

            consoleTextBox.Text = GetInitialHelpText(mainWindow);

            return consoleTextBox;
        }

        /// <summary>
        /// Creates the quick action buttons panel with predefined commands.
        /// Buttons: Status, Installed, Diagnostic, Test, Help.
        /// Overload allows excluding clear button (already overlaid on console).
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>WrapPanel with quick action buttons.</returns>
        private static WrapPanel CreateQuickButtonsPanel(DevStackGui mainWindow)
        {
            return CreateQuickButtonsPanel(mainWindow, includeClearButton: true);
        }

        /// <summary>
        /// Creates the quick action buttons panel with optional clear button inclusion.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <param name="includeClearButton">Whether to include clear button (obsolete - now overlaid on console).</param>
        /// <returns>WrapPanel with quick action buttons.</returns>
        private static WrapPanel CreateQuickButtonsPanel(DevStackGui mainWindow, bool includeClearButton)
        {
            var buttonsPanel = new WrapPanel
            {
                Margin = new Thickness(BUTTONS_PANEL_TOP_MARGIN, BUTTONS_PANEL_TOP_MARGIN, BUTTONS_PANEL_TOP_MARGIN, CONSOLE_MARGIN),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var quickButtons = new[]
            {
                new { Text = mainWindow.LocalizationManager.GetString("gui.utilities_tab.status_button"), Command = "status" },
                new { Text = mainWindow.LocalizationManager.GetString("gui.utilities_tab.installed_button"), Command = "list --installed" },
                new { Text = mainWindow.LocalizationManager.GetString("gui.utilities_tab.diagnostic_button"), Command = "doctor" },
                new { Text = mainWindow.LocalizationManager.GetString("gui.utilities_tab.test_button"), Command = "test" },
                new { Text = mainWindow.LocalizationManager.GetString("gui.utilities_tab.help_button"), Command = "help" }
            };

            foreach (var btn in quickButtons)
            {
                var button = CreateQuickButton(mainWindow, btn.Text, btn.Command);
                buttonsPanel.Children.Add(button);
            }

            return buttonsPanel;
        }

        /// <summary>
        /// Creates a quick action button for predefined commands.
        /// Help command receives special handling to show help text without execution.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <param name="text">Button display text.</param>
        /// <param name="command">Command to execute when button is clicked.</param>
        /// <returns>Button with command execution handler.</returns>
        private static Button CreateQuickButton(DevStackGui mainWindow, string text, string command)
        {
            var button = DevStackShared.ThemeManager.CreateStyledButton(text);
            button.Width = QUICK_BUTTON_WIDTH;
            button.Height = QUICK_BUTTON_HEIGHT;
            button.Margin = new Thickness(QUICK_BUTTON_MARGIN);
            
            if (command == "help")
            {
                button.Click += (s, e) => ShowHelpInConsole(mainWindow);
            }
            else
            {
                button.Click += (s, e) => ExecuteCommand(mainWindow, command);
            }
            
            return button;
        }

        /// <summary>
        /// Executes a DevStack command and displays output in console.
        /// First attempts to execute via DevStack.exe, falls back to direct C# manager calls.
        /// Special handling for "help" command to display initial help text.
        /// </summary>
        /// <param name="mainWindow">Main window instance for status updates.</param>
        /// <param name="command">Command to execute (e.g., "status", "list --installed", "doctor").</param>
        private static async void ExecuteCommand(DevStackGui mainWindow, string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            var consoleOutput = GuiHelpers.FindChild<TextBox>(mainWindow, "UtilsConsoleOutput");
            if (consoleOutput == null) return;

            if (command.Trim().ToLowerInvariant() == "help")
            {
                consoleOutput.Text = GetInitialHelpText(mainWindow);
                consoleOutput.ScrollToEnd();
                return;
            }

            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.utilities_tab.status.executing", command);
            
            consoleOutput.AppendText($"\n> {command}\n");
            consoleOutput.ScrollToEnd();

            try
            {
                await Task.Run(() =>
                {
                    var output = "";
                    var devStackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DevStack.exe");
                    
                    if (!File.Exists(devStackPath))
                    {
                        devStackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "DevStack.exe");
                    }
                    
                    if (!File.Exists(devStackPath))
                    {
                        devStackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DevStack.exe");
                    }

                    if (File.Exists(devStackPath))
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = devStackPath,
                            Arguments = command,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            StandardOutputEncoding = System.Text.Encoding.UTF8,
                            StandardErrorEncoding = System.Text.Encoding.UTF8
                        };

                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            var stdout = process.StandardOutput.ReadToEnd();
                            var stderr = process.StandardError.ReadToEnd();
                            process.WaitForExit();

                            output = stdout;
                            if (!string.IsNullOrEmpty(stderr))
                            {
                                output += $"\nERROS:\n{stderr}";
                            }
                            
                            if (string.IsNullOrEmpty(output))
                            {
                                output = mainWindow.LocalizationManager.GetString("gui.utilities_tab.no_output");
                            }
                        }
                        else
                        {
                            output = mainWindow.LocalizationManager.GetString("gui.utilities_tab.devstack_not_found");
                        }
                    }
                    else
                    {
                        output = ExecuteCommandDirect(command);
                    }

                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        if (output == "help_command_special")
                        {
                            consoleOutput.Text = GetInitialHelpText(mainWindow);
                            consoleOutput.ScrollToEnd();
                        }
                        else
                        {
                            consoleOutput.AppendText($"{output}\n\n");
                            consoleOutput.ScrollToEnd();
                            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.utilities_tab.status.executed");
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    consoleOutput.AppendText($"{mainWindow.LocalizationManager.GetString("gui.utilities_tab.error")}: {ex.Message}\n\n");
                    consoleOutput.ScrollToEnd();
                    mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.utilities_tab.status.error");
                });
            }
        }

        /// <summary>
        /// Executes commands directly using C# managers when DevStack.exe is not available.
        /// Supports: status (ProcessManager), list (ListManager), help (special signal).
        /// Returns "help_command_special" signal for help command to trigger UI update.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <returns>Command output or special help signal.</returns>
        private static string ExecuteCommandDirect(string command)
        {
            try
            {
                var localizationManager = DevStackShared.LocalizationManager.Instance ?? DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);
                
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    return localizationManager.GetString("gui.utilities_tab.empty_command");
                }

                switch (parts[0].ToLowerInvariant())
                {
                    case "status":
                        using (var writer = new StringWriter())
                        {
                            var originalOut = Console.Out;
                            Console.SetOut(writer);
                            ProcessManager.ListComponentsStatus();
                            Console.SetOut(originalOut);
                            return writer.ToString();
                        }
                    case "list":
                        if (parts.Length > 1 && parts[1] == "--installed")
                        {
                            using (var writer = new StringWriter())
                            {
                                var originalOut = Console.Out;
                                Console.SetOut(writer);
                                ListManager.ListInstalledVersions();
                                Console.SetOut(originalOut);
                                return writer.ToString();
                            }
                        }
                        else if (parts.Length > 1)
                        {
                            using (var writer = new StringWriter())
                            {
                                var originalOut = Console.Out;
                                Console.SetOut(writer);
                                ListManager.ListVersions(parts[1]);
                                Console.SetOut(originalOut);
                                return writer.ToString();
                            }
                        }
                        return localizationManager.GetString("gui.utilities_tab.messages.list_usage");
                    case "help":
                        return "help_command_special";
                    default:
                        return localizationManager.GetString("gui.utilities_tab.messages.command_not_recognized", parts[0]);
                }
            }
            catch (Exception ex)
            {
                var localizationManager = DevStackShared.LocalizationManager.Instance ?? DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);
                return localizationManager.GetString("gui.utilities_tab.command_execution_error", ex.Message);
            }
        }

        /// <summary>
        /// Clears the utilities console output.
        /// Displays localized "console cleared" message.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        private static void ClearConsole(DevStackGui mainWindow)
        {
            var consoleOutput = GuiHelpers.FindChild<TextBox>(mainWindow, "UtilsConsoleOutput");
            if (consoleOutput != null)
            {
                consoleOutput.Text = mainWindow.LocalizationManager.GetString("gui.utilities_tab.console_cleared");
                mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.utilities_tab.status.cleared");
            }
        }

        /// <summary>
        /// Shows help text in console by clearing and displaying initial help content.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        private static void ShowHelpInConsole(DevStackGui mainWindow)
        {
            var consoleOutput = GuiHelpers.FindChild<TextBox>(mainWindow, "UtilsConsoleOutput");
            if (consoleOutput != null)
            {
                consoleOutput.Text = GetInitialHelpText(mainWindow);
                consoleOutput.ScrollToEnd();
            }
        }

        /// <summary>
        /// Gets the initial help text displayed in console on tab load.
        /// Includes localized header, available commands list from DevStackConfig.ShowHelpTable(), and tip message.
        /// </summary>
        /// <param name="mainWindow">Main window instance for localization.</param>
        /// <returns>Formatted help text with command table.</returns>
        private static string GetInitialHelpText(DevStackGui mainWindow)
        {
            var localizationManager = mainWindow.LocalizationManager;
            var helpText = localizationManager.GetString("gui.utilities_tab.console_header") + "\n" +
                          localizationManager.GetString("gui.utilities_tab.available_commands") + "\n\n";

            helpText += DevStackConfig.ShowHelpTable();

            helpText += "\n" + localizationManager.GetString("gui.utilities_tab.tip_message") + "\n";

            return helpText;
        }
    }
}
