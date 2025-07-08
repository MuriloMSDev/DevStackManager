using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
            var statusBar = new Border
            {
                Background = GuiTheme.DarkTheme.StatusBackground,
                BorderBrush = GuiTheme.DarkTheme.Border,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Height = 30,
                Padding = new Thickness(5, 0, 0, 0)
            };
            Grid.SetRow(statusBar, 1);

            var statusGrid = new Grid();
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var statusLabel = new Label
            {
                FontSize = 12,
                Foreground = GuiTheme.DarkTheme.StatusForeground,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var statusBinding = new Binding("StatusMessage") { Source = gui };
            statusLabel.SetBinding(Label.ContentProperty, statusBinding);
            Grid.SetColumn(statusLabel, 0);
            statusGrid.Children.Add(statusLabel);

            // Create refresh button with only icon
            var refreshButton = GuiTheme.CreateStyledButton("🔄");
            refreshButton.Width = 30;
            refreshButton.Height = 30; // Mesma altura da barra (30 - padding)
            refreshButton.FontSize = 10;
            refreshButton.Padding = new Thickness(0);
            refreshButton.ToolTip = "Atualizar status";
            refreshButton.VerticalAlignment = VerticalAlignment.Center;
            refreshButton.HorizontalAlignment = HorizontalAlignment.Right;
            refreshButton.Click += async (s, e) => {
                gui.StatusMessage = "Atualizando...";
                await GuiInstalledTab.LoadInstalledComponents(gui);
                await GuiServicesTab.LoadServices(gui);
                gui.StatusMessage = "Status atualizado";
            };
            Grid.SetColumn(refreshButton, 1);
            statusGrid.Children.Add(refreshButton);

            statusBar.Child = statusGrid;
            mainGrid.Children.Add(statusBar);
        }
    }
}
