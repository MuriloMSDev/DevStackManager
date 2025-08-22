using System.Windows;
using System.Windows.Controls;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela área de conteúdo principal da aplicação
    /// </summary>
    public static class GuiMainContent
    {
        /// <summary>
        /// Cria o conteúdo principal da aplicação com sidebar e área de conteúdo
        /// </summary>
        public static void CreateMainContent(Grid mainGrid, DevStackGui mainWindow)
        {
            var contentGrid = new Grid { Margin = new Thickness(0) };
            Grid.SetRow(contentGrid, 0);

            // Definir colunas: Sidebar | Content
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) }); // Sidebar
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Content

            // Criar sidebar e área de conteúdo principal (unificado)
            GuiNavigation.CreateMainContent(mainWindow, mainGrid);
        }

        /// <summary>
        /// Cria a área de conteúdo principal onde as tabs são exibidas
        /// </summary>
        private static void CreateContentArea(Grid contentGrid, DevStackGui mainWindow)
        {
            mainWindow._mainContent = new ContentControl
            {
                Margin = new Thickness(10)
            };
            Grid.SetColumn(mainWindow._mainContent, 1);
            contentGrid.Children.Add(mainWindow._mainContent);

            // Definir o SelectedNavIndex para 0 primeiro para sincronizar com o sidebar
            mainWindow.SelectedNavIndex = 0;
        }
    }
}
