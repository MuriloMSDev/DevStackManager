using System.Collections.Generic;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Modelos de dados para a interface gráfica
    /// </summary>
    
    public class ComponentViewModel
    {
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public bool Installed { get; set; }
        public bool IsExecutable { get; set; }
        public List<string> Versions { get; set; } = new();
        public string Status { get; set; } = "";
        public string VersionsText { get; set; } = "";
        public SolidColorBrush StatusColor => Installed ? 
            new SolidColorBrush(Colors.LightGreen) : 
            new SolidColorBrush(Colors.Crimson);
    }

    public class ServiceViewModel
    {
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public string Version { get; set; } = "";
        public string Status { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public string Pid { get; set; } = "";
        public bool IsRunning { get; set; } = false;
        public SolidColorBrush StatusColor => IsRunning ? 
            new SolidColorBrush(Colors.LightGreen) : 
            new SolidColorBrush(Colors.Crimson);
    }
}
