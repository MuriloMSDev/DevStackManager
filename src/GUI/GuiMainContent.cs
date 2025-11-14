using System.Windows;
using System.Windows.Controls;

namespace DevStackManager
{
    /// <summary>
    /// Main content area component responsible for the application's primary display region.
    /// Creates the layout structure with sidebar navigation and content display area.
    /// Delegates to GuiNavigation for actual implementation.
    /// </summary>
    public static class GuiMainContent
    {
        /// <summary>
        /// Width of the sidebar in pixels.
        /// </summary>
        private const double SIDEBAR_WIDTH = 250;
        
        /// <summary>
        /// Content margin in pixels.
        /// </summary>
        private const double CONTENT_MARGIN = 10;
        
        /// <summary>
        /// Default navigation index (Dashboard).
        /// </summary>
        private const int DEFAULT_NAV_INDEX = 0;
        
        /// <summary>
        /// Creates the main content layout with sidebar and content area.
        /// Delegates to GuiNavigation.CreateMainContent for actual implementation.
        /// </summary>
        /// <param name="mainGrid">Main grid container to add content to.</param>
        /// <param name="mainWindow">Main window instance for navigation setup.</param>
        public static void CreateMainContent(Grid mainGrid, DevStackGui mainWindow)
        {
            var contentGrid = new Grid { Margin = new Thickness(0) };
            Grid.SetRow(contentGrid, 0);

            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SIDEBAR_WIDTH) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            GuiNavigation.CreateMainContent(mainWindow, mainGrid);
        }

        /// <summary>
        /// Creates the content area where tab content is displayed.
        /// Initializes the main ContentControl and sets default navigation index.
        /// </summary>
        /// <param name="contentGrid">Grid container for content area.</param>
        /// <param name="mainWindow">Main window instance with ContentControl reference.</param>
        private static void CreateContentArea(Grid contentGrid, DevStackGui mainWindow)
        {
            mainWindow._mainContent = new ContentControl
            {
                Margin = new Thickness(CONTENT_MARGIN)
            };
            Grid.SetColumn(mainWindow._mainContent, 1);
            contentGrid.Children.Add(mainWindow._mainContent);

            mainWindow.SelectedNavIndex = DEFAULT_NAV_INDEX;
        }
    }
}
