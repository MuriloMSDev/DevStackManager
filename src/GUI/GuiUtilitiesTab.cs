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
    /// Componente responsável pela aba "Utilitários" - console e ferramentas de diagnóstico
    /// </summary>
    public static class GuiUtilitiesTab
    {
        // UI Dimensions Constants
        private const int HEADER_FONT_SIZE = 18;
        private const int HEADER_MARGIN = 10;
        private const int CLEAR_BUTTON_SIZE = 32;
        private const int CLEAR_BUTTON_TOP_MARGIN = 1;
        private const int CLEAR_BUTTON_RIGHT_MARGIN_DEFAULT = 6;
        private const int CLEAR_BUTTON_RIGHT_MARGIN_WITH_SCROLLBAR = 23;
        private const int CLEAR_BUTTON_Z_INDEX = 10;
        private const int COMMAND_LABEL_WIDTH = 90;
        private const int COMMAND_INPUT_HEIGHT = 30;
        private const int COMMAND_INPUT_HORIZONTAL_MARGIN = 5;
        private const int EXECUTE_BUTTON_WIDTH = 100;
        private const int EXECUTE_BUTTON_HEIGHT = 30;
        private const int EXECUTE_BUTTON_LEFT_MARGIN = 5;
        private const int EXECUTE_BUTTON_HORIZONTAL_PADDING = 12;
        private const int EXECUTE_BUTTON_VERTICAL_PADDING = 4;
        private const int CONSOLE_FONT_SIZE = 12;
        private const int CONSOLE_MARGIN = 5;
        private const int CONSOLE_HORIZONTAL_MARGIN = 10;
        private const int QUICK_BUTTON_WIDTH = 120;
        private const int QUICK_BUTTON_HEIGHT = 35;
        private const int QUICK_BUTTON_MARGIN = 5;
        private const int BUTTONS_PANEL_TOP_MARGIN = 10;
        private const int INPUT_PANEL_MARGIN = 10;
        private const int INPUT_PANEL_TOP_MARGIN = 5;
        private const int INPUT_PANEL_BOTTOM_MARGIN = 10;

        // Font Constants
        private const string CONSOLE_FONT_FAMILY = "Consolas";

        /// <summary>
        /// Cria o conteúdo completo da aba "Utilitários"
        /// </summary>
        public static Grid CreateUtilitiesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Console output
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Command input

            // Header
            var headerLabel = DevStackShared.ThemeManager.CreateStyledLabel(mainWindow.LocalizationManager.GetString("gui.utilities_tab.console_title"), true);
            headerLabel.FontSize = HEADER_FONT_SIZE;
            headerLabel.Margin = new Thickness(HEADER_MARGIN, HEADER_MARGIN, HEADER_MARGIN, 0);
            Grid.SetRow(headerLabel, 0);
            grid.Children.Add(headerLabel);

            // Command input panel
            var inputPanel = CreateCommandInputPanel(mainWindow);
            Grid.SetRow(inputPanel, 3);
            grid.Children.Add(inputPanel);

            // Console output (with overlay Clear button)
            var consoleGrid = new Grid();
            // Layer 0: Console

            var consoleScrollViewer = CreateConsoleScrollViewer(mainWindow);
            consoleGrid.Children.Add(consoleScrollViewer);

            // Layer 1: Clear button (top-right)
            var clearButton = CreateClearButton(mainWindow);
            clearButton.Click += (s, e) => ClearConsole(mainWindow);
            Panel.SetZIndex(clearButton, CLEAR_BUTTON_Z_INDEX);
            consoleGrid.Children.Add(clearButton);

            // Ajuste dinâmico da margem do botão Clear conforme o scroll horizontal
            SetupClearButtonMarginAdjustment(consoleScrollViewer, clearButton);

            Grid.SetRow(consoleGrid, 2);
            grid.Children.Add(consoleGrid);

            // Buttons panel (without Clear button)
            var buttonsPanel = CreateQuickButtonsPanel(mainWindow, includeClearButton: false);
            Grid.SetRow(buttonsPanel, 1);
            grid.Children.Add(buttonsPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de entrada de comandos
        /// </summary>
        private static Grid CreateCommandInputPanel(DevStackGui mainWindow)
        {
            var inputPanel = new Grid
            {
                Margin = new Thickness(INPUT_PANEL_MARGIN, INPUT_PANEL_TOP_MARGIN, INPUT_PANEL_MARGIN, INPUT_PANEL_BOTTOM_MARGIN)
            };
            
            // Definir colunas: Label (auto) | TextBox (star) | Button (auto)
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

        private static TextBox CreateCommandTextBox(DevStackGui mainWindow)
        {
            var commandTextBox = DevStackShared.ThemeManager.CreateStyledTextBox();
            commandTextBox.Height = COMMAND_INPUT_HEIGHT;
            commandTextBox.Margin = new Thickness(COMMAND_INPUT_HORIZONTAL_MARGIN, 0, COMMAND_INPUT_HORIZONTAL_MARGIN, 0);
            commandTextBox.Name = "UtilsCommandTextBox";
            
            // Enter para executar comando
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
            
            // Custom ControlTemplate: always transparent background, even on hover/press
            var buttonTemplate = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.SnapsToDevicePixelsProperty, true);
            
            // ContentPresenter for the icon
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentPresenterFactory);
            buttonTemplate.VisualTree = borderFactory;
            
            // Remove highlight/hover/pressed background
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));
            style.Setters.Add(new Setter(Button.ForegroundProperty, DevStackShared.ThemeManager.CurrentTheme.Danger));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(0)));
            
            clearButton.Style = style;
            
            return clearButton;
        }

        private static void SetupClearButtonMarginAdjustment(ScrollViewer consoleScrollViewer, Button clearButton)
        {
            void UpdateClearButtonMargin()
            {
                // Se o ScrollViewer mostrar a barra de rolagem vertical, aumente a margem da direita
                if (consoleScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    clearButton.Margin = new Thickness(0, CLEAR_BUTTON_TOP_MARGIN, CLEAR_BUTTON_RIGHT_MARGIN_WITH_SCROLLBAR, 0);
                }
                else
                {
                    clearButton.Margin = new Thickness(0, CLEAR_BUTTON_TOP_MARGIN, CLEAR_BUTTON_RIGHT_MARGIN_DEFAULT, 0);
                }
            }
            
            // Atualiza ao carregar e ao rolar
            consoleScrollViewer.ScrollChanged += (s, e) => UpdateClearButtonMargin();
            consoleScrollViewer.SizeChanged += (s, e) => UpdateClearButtonMargin();
            consoleScrollViewer.Loaded += (s, e) => UpdateClearButtonMargin();
        }

        /// <summary>
        /// Cria o ScrollViewer para o console de saída
        /// </summary>
        private static ScrollViewer CreateConsoleScrollViewer(DevStackGui mainWindow)
        {
            var consoleScrollViewer = new ScrollViewer
            {
                Margin = new Thickness(CONSOLE_HORIZONTAL_MARGIN, CONSOLE_MARGIN, CONSOLE_HORIZONTAL_MARGIN, CONSOLE_MARGIN),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            // Aplicar scrollbar customizada do ThemeManager
            DevStackShared.ThemeManager.ApplyCustomScrollbar(consoleScrollViewer);

            var consoleTextBox = CreateConsoleTextBox(mainWindow);
            consoleScrollViewer.Content = consoleTextBox;
            
            return consoleScrollViewer;
        }

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

            // Texto inicial de ajuda usando o LocalizationManager da mainWindow
            consoleTextBox.Text = GetInitialHelpText(mainWindow);

            return consoleTextBox;
        }

        /// <summary>
        /// Cria o painel de botões rápidos
        /// </summary>
        private static WrapPanel CreateQuickButtonsPanel(DevStackGui mainWindow)
        {
            return CreateQuickButtonsPanel(mainWindow, includeClearButton: true);
        }

        // Nova sobrecarga para permitir remover o botão de limpar
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

            // O botão de limpar não é mais adicionado aqui, pois agora é sobreposto ao console

            return buttonsPanel;
        }

        private static Button CreateQuickButton(DevStackGui mainWindow, string text, string command)
        {
            var button = DevStackShared.ThemeManager.CreateStyledButton(text);
            button.Width = QUICK_BUTTON_WIDTH;
            button.Height = QUICK_BUTTON_HEIGHT;
            button.Margin = new Thickness(QUICK_BUTTON_MARGIN);
            
            // Tratamento especial para o botão de ajuda
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
        /// Executa um comando no console do DevStack
        /// </summary>
        private static async void ExecuteCommand(DevStackGui mainWindow, string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            var consoleOutput = GuiHelpers.FindChild<TextBox>(mainWindow, "UtilsConsoleOutput");
            if (consoleOutput == null) return;

            // Tratamento especial para comando help
            if (command.Trim().ToLowerInvariant() == "help")
            {
                consoleOutput.Text = GetInitialHelpText(mainWindow);
                consoleOutput.ScrollToEnd();
                return;
            }

            mainWindow.StatusMessage = mainWindow.LocalizationManager.GetString("gui.utilities_tab.status.executing", command);
            
            // Adicionar comando ao console
            consoleOutput.AppendText($"\n> {command}\n");
            consoleOutput.ScrollToEnd();

            try
            {
                await Task.Run(() =>
                {
                    var output = "";
                    var devStackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DevStack.exe");
                    
                    // Verificar se o arquivo DevStack.exe existe no diretório atual
                    if (!File.Exists(devStackPath))
                    {
                        devStackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "DevStack.exe");
                    }
                    
                    // Verificar no diretório raiz do workspace
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
                        // Fallback: usar managers C# diretamente
                        output = ExecuteCommandDirect(command);
                    }

                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        // Verificar se é o comando help especial
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
        /// Executa comandos diretamente usando os managers C#
        /// </summary>
        private static string ExecuteCommandDirect(string command)
        {
            try
            {
                // Usar a instância existente ao invés de criar uma nova
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
                        return "help_command_special"; // Sinal especial para limpar console
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
        /// Limpa o console de utilitários
        /// </summary>
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
        /// Mostra a ajuda no console (limpa e exibe o texto inicial)
        /// </summary>
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
        /// Obtém o texto inicial de ajuda para o console
        /// </summary>
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
