using System.Collections.Generic;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Modelos de dados para a interface gráfica
    /// </summary>
    public static class GuiColors
    {
        public static readonly SolidColorBrush ActiveGreen = new(Colors.LightGreen);
        public static readonly SolidColorBrush InactiveCrimson = new(Colors.Crimson);
        public static readonly SolidColorBrush WarningYellow = new(Colors.Gold);
        public static readonly SolidColorBrush InfoBlue = new(Colors.LightBlue);
    }

    public class ComponentViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Installed { get; set; }
        public bool IsExecutable { get; set; }
        public List<string> Versions { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string VersionsText { get; set; } = string.Empty;
        
        public SolidColorBrush StatusColor => 
            Installed ? GuiColors.ActiveGreen : GuiColors.InactiveCrimson;
    }

    public class ServiceViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Pid { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
        
        public SolidColorBrush StatusColor => 
            IsRunning ? GuiColors.ActiveGreen : GuiColors.InactiveCrimson;
    }
}
