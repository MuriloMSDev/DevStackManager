using System.Collections.Generic;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Color constants and view model classes for GUI data binding.
    /// Provides data structures for displaying components and services in the UI.
    /// </summary>
    public static class GuiColors
    {
        /// <summary>
        /// Color for active/running state indicators.
        /// </summary>
        public static readonly SolidColorBrush ActiveGreen = new(Colors.LightGreen);
        
        /// <summary>
        /// Color for inactive/stopped state indicators.
        /// </summary>
        public static readonly SolidColorBrush InactiveCrimson = new(Colors.Crimson);
        
        /// <summary>
        /// Color for warning state indicators.
        /// </summary>
        public static readonly SolidColorBrush WarningYellow = new(Colors.Gold);
        
        /// <summary>
        /// Color for informational state indicators.
        /// </summary>
        public static readonly SolidColorBrush InfoBlue = new(Colors.LightBlue);
    }

    /// <summary>
    /// View model for displaying component installation status.
    /// Used in InstalledComponents tab to show which components are installed with their versions.
    /// </summary>
    public class ComponentViewModel
    {
        /// <summary>
        /// Gets or sets the technical component name (e.g., "php", "node").
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the display label (e.g., "PHP", "Node.js").
        /// </summary>
        public string Label { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether the component is installed.
        /// </summary>
        public bool Installed { get; set; }
        
        /// <summary>
        /// Gets or sets whether the component is executable from command line.
        /// </summary>
        public bool IsExecutable { get; set; }
        
        /// <summary>
        /// Gets or sets the list of installed versions.
        /// </summary>
        public List<string> Versions { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the status text (e.g., "OK", "Error").
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the comma-separated versions text for display.
        /// </summary>
        public string VersionsText { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the status color based on installation state.
        /// Green if installed, crimson if not installed.
        /// </summary>
        public SolidColorBrush StatusColor => 
            Installed ? GuiColors.ActiveGreen : GuiColors.InactiveCrimson;
    }

    /// <summary>
    /// View model for displaying service status and control information.
    /// Used in Services tab to show running services with their PIDs and status.
    /// </summary>
    public class ServiceViewModel
    {
        /// <summary>
        /// Gets or sets the technical service name (e.g., "php", "nginx").
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the display label (e.g., "PHP", "Nginx").
        /// </summary>
        public string Label { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public string Version { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the status text (e.g., "Running", "Stopped").
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the service type description.
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the service description.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the process ID(s) of running service instances.
        /// </summary>
        public string Pid { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether the service is currently running.
        /// </summary>
        public bool IsRunning { get; set; }
        
        /// <summary>
        /// Gets the status color based on running state.
        /// Green if running, crimson if stopped.
        /// </summary>
        public SolidColorBrush StatusColor => 
            IsRunning ? GuiColors.ActiveGreen : GuiColors.InactiveCrimson;
    }
}
