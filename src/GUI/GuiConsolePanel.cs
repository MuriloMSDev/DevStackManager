

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// ConsolePanel multi-abas: cada aba (Install, Uninstall, Sites) tem seu próprio buffer persistente.
    /// Outputs de execuções em background continuam sendo enviados para o buffer correto.
    /// </summary>
    public static class GuiConsolePanel
    {
        public enum ConsoleTab { Install, Uninstall, Sites, Config }

        // Console Panel Dimensions
        private const double PANEL_MARGIN = 10;
        private const double TITLE_FONT_SIZE = 18;
        private const double TITLE_MARGIN_BOTTOM = 10;
        private const double OUTPUT_BOX_HEIGHT = 600;
        private const double OUTPUT_BOX_FONT_SIZE = 12;
        private const double CLEAR_BUTTON_HEIGHT = 35;
        private const double CLEAR_BUTTON_MARGIN_TOP = 10;

        // Buffer persistente para cada aba
        private static readonly ConcurrentDictionary<ConsoleTab, StringBuilder> Buffers = new();
        // Lista de TextBox ativos por aba (atualiza output ao alternar)
        private static readonly ConcurrentDictionary<ConsoleTab, TextBox> ActiveTextBoxes = new();
        // Lock para escrita
        private static readonly object _lock = new();

        static GuiConsolePanel()
        {
            foreach (ConsoleTab tab in Enum.GetValues(typeof(ConsoleTab)))
                Buffers[tab] = new StringBuilder();
        }

        /// <summary>
        /// Cria painel de console para uma aba específica
        /// </summary>
        public static StackPanel CreateConsolePanel(ConsoleTab tab)
        {
            var panel = new StackPanel { Margin = new Thickness(PANEL_MARGIN) };
            var mainWindow = (DevStackGui)Application.Current.MainWindow;
            
            var titleLabel = CreateConsoleTitleLabel(tab, mainWindow);
            panel.Children.Add(titleLabel);

            var outputBox = CreateOutputTextBox(tab);
            panel.Children.Add(outputBox);

            var clearButton = CreateClearButton(tab, mainWindow);
            panel.Children.Add(clearButton);

            return panel;
        }

        /// <summary>
        /// Cria o label de título do console
        /// </summary>
        private static Label CreateConsoleTitleLabel(ConsoleTab tab, DevStackGui mainWindow)
        {
            var title = GetTabTitle(tab, mainWindow);
            var titleLabel = DevStackShared.ThemeManager.CreateStyledLabel(title, true);
            titleLabel.FontSize = TITLE_FONT_SIZE;
            titleLabel.Margin = new Thickness(0, 0, 0, TITLE_MARGIN_BOTTOM);
            return titleLabel;
        }

        /// <summary>
        /// Obtém o título localizado para a aba
        /// </summary>
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
        /// Cria o TextBox de output do console
        /// </summary>
        private static TextBox CreateOutputTextBox(ConsoleTab tab)
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
        /// Cria o botão Clear do console
        /// </summary>
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
        /// Limpa o buffer do console da aba
        /// </summary>
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
        /// Adiciona output ao buffer da aba (thread-safe, pode ser chamado de qualquer thread)
        /// </summary>
        public static void Append(ConsoleTab tab, string text, DevStackGui mainWindow)
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                Buffers[tab].AppendLine($"[{timestamp}] {text}");
                // Atualiza textbox se a aba estiver visível
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
        /// Recupera o buffer atual da aba (para restaurar ao alternar)
        /// </summary>
        public static string GetBuffer(ConsoleTab tab)
        {
            lock (_lock)
            {
                return Buffers[tab].ToString();
            }
        }

        /// <summary>
        /// Deve ser chamado ao alternar para uma aba para restaurar o output
        /// </summary>
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
        /// Permite executar uma ação e enviar output para o buffer correto, mesmo em background
        /// </summary>
        public static async Task RunWithConsoleOutput(ConsoleTab tab, DevStackGui mainWindow, Func<IProgress<string>, Task> action)
        {
            var progress = new Progress<string>(msg => Append(tab, msg, mainWindow));
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

        // Writer que envia tudo para progress.Report
        private class ProgressTextWriter : System.IO.TextWriter
        {
            private readonly IProgress<string> _progress;
            public ProgressTextWriter(IProgress<string> progress) { _progress = progress; }
            public override Encoding Encoding => Encoding.UTF8;
            public override void WriteLine(string? value) => _progress.Report(value ?? "");
            public override void Write(string? value) => _progress.Report(value ?? "");
        }
    }
}
