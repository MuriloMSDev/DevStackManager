using System.Windows;
using System.Windows.Controls;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela área de conteúdo principal da aplicação
    /// </summary>
    public static class GuiMainContent
    {
        // Layout Constants
        private const double SIDEBAR_WIDTH = 250;
        private const double CONTENT_MARGIN = 10;
        private const int DEFAULT_NAV_INDEX = 0;
        
        /// <summary>
        /// Cria o conteúdo principal da aplicação com sidebar e área de conteúdo
        /// </summary>
        public static void CreateMainContent(Grid mainGrid, DevStackGui mainWindow)
        {
            var contentGrid = new Grid { Margin = new Thickness(0) };
            Grid.SetRow(contentGrid, 0);

            // Definir colunas: Sidebar | Content
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SIDEBAR_WIDTH) }); // Sidebar
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
                Margin = new Thickness(CONTENT_MARGIN)
            };
            Grid.SetColumn(mainWindow._mainContent, 1);
            contentGrid.Children.Add(mainWindow._mainContent);

            // Definir o SelectedNavIndex para 0 primeiro para sincronizar com o sidebar
            mainWindow.SelectedNavIndex = DEFAULT_NAV_INDEX;
        }
    }
}
