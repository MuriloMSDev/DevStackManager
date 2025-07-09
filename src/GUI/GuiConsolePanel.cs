using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pelo painel de console compartilhado - usado por várias abas
    /// Captura saída do terminal principal em tempo real
    /// </summary>
    public static class GuiConsolePanel
    {
        private static DevStackGui? _currentMainWindow;
        private static StringWriter? _consoleCapture;
        private static TextWriter? _originalConsoleOut;
        private static bool _isCapturing = false;
        /// <summary>
        /// Inicia a captura da saída do console principal
        /// </summary>
        public static void StartConsoleCapture(DevStackGui mainWindow)
        {
            if (_isCapturing) return;

            _currentMainWindow = mainWindow;
            _originalConsoleOut = Console.Out;
            _consoleCapture = new StringWriter();

            // Redirecionar Console.Out para capturar saída
            Console.SetOut(new ConsoleWriter(mainWindow));
            _isCapturing = true;
        }

        /// <summary>
        /// Para a captura da saída do console principal
        /// </summary>
        public static void StopConsoleCapture()
        {
            if (!_isCapturing) return;

            if (_originalConsoleOut != null)
            {
                Console.SetOut(_originalConsoleOut);
            }

            _consoleCapture?.Dispose();
            _consoleCapture = null;
            _originalConsoleOut = null;
            _currentMainWindow = null;
            _isCapturing = false;
        }

        /// <summary>
        /// Limpa o console
        /// </summary>
        public static void ClearConsole(DevStackGui mainWindow)
        {
            mainWindow.ConsoleOutput = "";
        }

        /// <summary>
        /// Cria o painel de saída do console compartilhado
        /// </summary>
        public static StackPanel CreateConsoleOutputPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = GuiTheme.CreateStyledLabel("Saída do Console", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(titleLabel);

            // Console output
            var outputBox = GuiTheme.CreateStyledTextBox(true);
            outputBox.Height = 600;
            outputBox.IsReadOnly = true;
            outputBox.FontSize = 12;
            outputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.AcceptsReturn = true;
            outputBox.TextWrapping = TextWrapping.Wrap;
            outputBox.Name = "ConsoleOutput";

            var outputBinding = new Binding("ConsoleOutput") { Source = mainWindow };
            outputBox.SetBinding(TextBox.TextProperty, outputBinding);

            panel.Children.Add(outputBox);

            // Botão limpar
            var clearButton = GuiTheme.CreateStyledButton("🗑️ Limpar Console", (s, e) => ClearConsole(mainWindow));
            clearButton.Height = 35;
            clearButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(clearButton);

            // Iniciar captura do console quando o painel é criado
            StartConsoleCapture(mainWindow);

            return panel;
        }

        /// <summary>
        /// Adiciona uma mensagem ao console principal (apenas para erros)
        /// </summary>
        public static void AppendToConsole(DevStackGui mainWindow, string message)
        {
            // Só usar para mensagens de erro - a saída normal será capturada automaticamente
            if (message.Contains("❌") || message.Contains("⚠️"))
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                AppendToConsoleInternal(mainWindow, $"[{timestamp}] {message}");
            }
        }

        /// <summary>
        /// Adiciona texto diretamente ao console (uso interno)
        /// </summary>
        internal static void AppendToConsoleInternal(DevStackGui mainWindow, string text)
        {
            if (mainWindow != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    mainWindow.ConsoleOutput += text + "\n";
                    
                    // Limitar o tamanho do console (manter últimas 1000 linhas)
                    var lines = mainWindow.ConsoleOutput.Split('\n');
                    if (lines.Length > 1000)
                    {
                        mainWindow.ConsoleOutput = string.Join("\n", lines.TakeLast(1000));
                    }
                });
            }
        }
    }

    /// <summary>
    /// Writer customizado para capturar saída do console em tempo real
    /// </summary>
    internal class ConsoleWriter : TextWriter
    {
        private readonly DevStackGui _mainWindow;
        private readonly StringBuilder _lineBuffer = new StringBuilder();

        public ConsoleWriter(DevStackGui mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            if (value == '\n')
            {
                // Linha completa - enviar para o console da GUI
                var line = _lineBuffer.ToString();
                if (!string.IsNullOrEmpty(line))
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    GuiConsolePanel.AppendToConsoleInternal(_mainWindow, $"[{timestamp}] {line}");
                }
                _lineBuffer.Clear();
            }
            else if (value != '\r')
            {
                _lineBuffer.Append(value);
            }
        }

        public override void WriteLine(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                GuiConsolePanel.AppendToConsoleInternal(_mainWindow, $"[{timestamp}] {value}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _lineBuffer.Length > 0)
            {
                // Enviar qualquer texto restante no buffer
                var remaining = _lineBuffer.ToString();
                if (!string.IsNullOrEmpty(remaining))
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    GuiConsolePanel.AppendToConsoleInternal(_mainWindow, $"[{timestamp}] {remaining}");
                }
            }
            base.Dispose(disposing);
        }
    }
}
