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
        /// <summary>
        /// Cria o conteúdo completo da aba "Utilitários"
        /// </summary>
        public static Grid CreateUtilitiesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Command input
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Console output
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

            // Header
            var headerLabel = GuiTheme.CreateStyledLabel("Console DevStack - Execute comandos diretamente", true);
            headerLabel.FontSize = 18;
            headerLabel.Margin = new Thickness(10, 10, 10, 0);
            Grid.SetRow(headerLabel, 0);
            grid.Children.Add(headerLabel);

            // Command input panel
            var inputPanel = CreateCommandInputPanel(mainWindow);
            Grid.SetRow(inputPanel, 1);
            grid.Children.Add(inputPanel);

            // Console output (with overlay Clear button)
            var consoleGrid = new Grid();
            // Layer 0: Console
            var consoleScrollViewer = CreateConsoleScrollViewer(mainWindow);
            consoleGrid.Children.Add(consoleScrollViewer);

            // Layer 1: Clear button (top-right)
            var clearButton = new Button
            {
                Content = "❌",
                BorderBrush = Brushes.Transparent,
                Foreground = GuiTheme.CurrentTheme.Danger,
                Width = 32,
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 6, 0),
                ToolTip = "Limpar Console"
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
            style.Setters.Add(new Setter(Button.ForegroundProperty, GuiTheme.CurrentTheme.Danger));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(0)));
            // No triggers needed: template always keeps background transparent
            clearButton.Style = style;
            clearButton.Click += (s, e) => ClearConsole(mainWindow);
            Panel.SetZIndex(clearButton, 10);
            consoleGrid.Children.Add(clearButton);

            Grid.SetRow(consoleGrid, 2);
            grid.Children.Add(consoleGrid);

            // Buttons panel (without Clear button)
            var buttonsPanel = CreateQuickButtonsPanel(mainWindow, includeClearButton: false);
            Grid.SetRow(buttonsPanel, 3);
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
                Margin = new Thickness(10, 10, 10, 5)
            };
            
            // Definir colunas: Label (auto) | TextBox (star) | Button (auto)
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var commandLabel = GuiTheme.CreateStyledLabel("Comando:");
            commandLabel.Width = 80;
            commandLabel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(commandLabel, 0);
            inputPanel.Children.Add(commandLabel);

            var commandTextBox = GuiTheme.CreateStyledTextBox();
            commandTextBox.Height = 30;
            commandTextBox.Margin = new Thickness(5, 0, 5, 0);
            commandTextBox.Name = "UtilsCommandTextBox";
            Grid.SetColumn(commandTextBox, 1);
            
            // Enter para executar comando
            commandTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    ExecuteCommand(mainWindow, commandTextBox.Text);
                    commandTextBox.Text = "";
                }
            };
            
            inputPanel.Children.Add(commandTextBox);

            var executeButton = GuiTheme.CreateStyledButton("▶️ Executar", (s, e) =>
            {
                ExecuteCommand(mainWindow, commandTextBox.Text);
                commandTextBox.Text = "";
            });
            executeButton.Width = 100;
            executeButton.Height = 30;
            executeButton.Margin = new Thickness(5, 0, 0, 0);
            executeButton.Padding = new Thickness(12, 4, 12, 4);
            Grid.SetColumn(executeButton, 2);
            inputPanel.Children.Add(executeButton);

            return inputPanel;
        }

        /// <summary>
        /// Cria o ScrollViewer para o console de saída
        /// </summary>
        private static ScrollViewer CreateConsoleScrollViewer(DevStackGui mainWindow)
        {
            var consoleScrollViewer = new ScrollViewer
            {
                Margin = new Thickness(10, 5, 10, 5),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var consoleTextBox = new TextBox
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Background = GuiTheme.CurrentTheme.ConsoleBackground,
                Foreground = GuiTheme.CurrentTheme.ConsoleForeground,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Name = "UtilsConsoleOutput"
            };

            // Texto inicial de ajuda
            consoleTextBox.Text = GetInitialHelpText();

            consoleScrollViewer.Content = consoleTextBox;
            return consoleScrollViewer;
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
                Margin = new Thickness(10, 5, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var quickButtons = new[]
            {
                new { Text = "📊 Status", Command = "status" },
                new { Text = "📦 Instalados", Command = "list --installed" },
                new { Text = "🔍 Diagnóstico", Command = "doctor" },
                new { Text = "🧪 Testar", Command = "test" },
                new { Text = "❓ Ajuda", Command = "help" }
            };

            foreach (var btn in quickButtons)
            {
                var button = GuiTheme.CreateStyledButton(btn.Text);
                button.Width = 120;
                button.Height = 35;
                button.Margin = new Thickness(5);
                var command = btn.Command;
                
                // Tratamento especial para o botão de ajuda
                if (command == "help")
                {
                    button.Click += (s, e) => ShowHelpInConsole(mainWindow);
                }
                else
                {
                    button.Click += (s, e) => ExecuteCommand(mainWindow, command);
                }
                
                buttonsPanel.Children.Add(button);
            }


            // O botão de limpar não é mais adicionado aqui, pois agora é sobreposto ao console

            return buttonsPanel;
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
                consoleOutput.Text = GetInitialHelpText();
                consoleOutput.ScrollToEnd();
                return;
            }

            mainWindow.StatusMessage = $"Executando: {command}";
            
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
                                output = "(Comando executado, sem saída gerada)";
                            }
                        }
                        else
                        {
                            output = "Erro: Não foi possível iniciar o processo DevStack.exe";
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
                            consoleOutput.Text = GetInitialHelpText();
                            consoleOutput.ScrollToEnd();
                        }
                        else
                        {
                            consoleOutput.AppendText($"{output}\n\n");
                            consoleOutput.ScrollToEnd();
                            mainWindow.StatusMessage = "Comando executado";
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    consoleOutput.AppendText($"ERRO: {ex.Message}\n\n");
                    consoleOutput.ScrollToEnd();
                    mainWindow.StatusMessage = "Erro ao executar comando";
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
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return "Comando vazio";

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
                        return "Uso: list --installed ou list <componente>";
                    case "help":
                        return "help_command_special"; // Sinal especial para limpar console
                    default:
                        return $"Comando '{parts[0]}' não reconhecido. Use 'help' para ver comandos disponíveis.";
                }
            }
            catch (Exception ex)
            {
                return $"Erro ao executar comando: {ex.Message}";
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
                consoleOutput.Text = "Console limpo.\n\n";
                mainWindow.StatusMessage = "Console limpo";
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
                consoleOutput.Text = GetInitialHelpText();
                consoleOutput.ScrollToEnd();
            }
        }

        /// <summary>
        /// Obtém o texto inicial de ajuda para o console
        /// </summary>
        private static string GetInitialHelpText()
        {
            var helpText = "DevStack Manager GUI Console\n" +
                          "COMANDOS DISPONÍVEIS:\n\n";

            helpText += DevStackConfig.ShowHelpTable();

            helpText += "\n💡 Dica: Digite um comando acima ou use os botões de atalho\n";

            return helpText;
        }
    }
}
