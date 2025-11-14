using System;
using System.IO;
using System.Text;
using System.Windows;

namespace DevStackManager
{
    /// <summary>
    /// GUI application entry point for DevStack Manager.
    /// This program is compiled as WinExe to launch without console window.
    /// Initializes WPF application and displays the main DevStack GUI window.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments (not used in GUI mode).</param>
        /// <returns>0 on success, 1 on error.</returns>
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                LoadConfiguration();

                var app = new Application();
                var mainWindow = new DevStackGui();
                app.MainWindow = mainWindow;

                var result = app.Run(mainWindow);

                return result;
            }
            catch (Exception ex)
            {
                var localizationManager = DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);
                DevStackShared.ThemeManager.CreateStyledMessageBox(
                    localizationManager.GetString("gui.window.initialization_error", ex.Message),
                    localizationManager.GetString("gui.window.error_title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return 1;
            }
        }

        /// <summary>
        /// Loads DevStack configuration from disk.
        /// </summary>
        private static void LoadConfiguration()
        {
            DevStackConfig.Initialize();
        }
    }
}
