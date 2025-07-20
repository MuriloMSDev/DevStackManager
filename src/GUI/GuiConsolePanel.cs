

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
            var panel = new StackPanel { Margin = new Thickness(10) };
            var title = tab switch
            {
                ConsoleTab.Install => "Saída do Console - Instalar",
                ConsoleTab.Uninstall => "Saída do Console - Desinstalar",
                ConsoleTab.Sites => "Saída do Console - Sites",
                ConsoleTab.Config => "Saída do Console - Configurações",
                _ => "Saída do Console"
            };
            var titleLabel = GuiTheme.CreateStyledLabel(title, true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(titleLabel);

            var outputBox = GuiTheme.CreateStyledTextBox(true);
            outputBox.Height = 600;
            outputBox.IsReadOnly = true;
            outputBox.FontSize = 12;
            outputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.AcceptsReturn = true;
            outputBox.TextWrapping = TextWrapping.Wrap;
            outputBox.Name = $"ConsoleOutput_{tab}";
            outputBox.Text = Buffers[tab].ToString();
            ActiveTextBoxes[tab] = outputBox;

            panel.Children.Add(outputBox);

            var clearButton = GuiTheme.CreateStyledButton("🗑️ Limpar Console", (s, e) => Clear(tab));
            clearButton.Height = 35;
            clearButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(clearButton);

            return panel;
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
        public static void Append(ConsoleTab tab, string text)
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
